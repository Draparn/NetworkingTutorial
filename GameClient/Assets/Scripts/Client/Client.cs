using NetworkTutorial.Shared;
using NetworkTutorial.Shared.Net;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace NetworkTutorial.Client
{
	public class Client : MonoBehaviour
	{
		public static Client Instance;

		protected delegate void PacketHandler(Packet packet);
		protected static Dictionary<int, PacketHandler> packethandlers;

		public TCP tcp;
		public UDP udp;

		public int MyId = 0;

		private bool isConnected = false;

		private void Awake()
		{
			if (Instance == null)
				Instance = this;
			else if (Instance != this)
				Destroy(this);
		}

		private void OnApplicationQuit()
		{
			Disconnect();
		}

		public void ConnectToServer(string ip)
		{
			tcp = new TCP();
			udp = new UDP(ip);
			tcp.InitializeClientData();
			tcp.Connect(ip);
			isConnected = true;
		}

		public void Disconnect()
		{
			if (isConnected)
			{
				isConnected = false;
				tcp.socket.Close();
				udp.socket.Close();
			}
		}

		public class TCP
		{
			public TcpClient socket;

			private NetworkStream stream;
			private Packet receivedData;

			byte[] receiveBuffer;

			public void Connect(string ip)
			{
				socket = new TcpClient
				{
					ReceiveBufferSize = ConstantValues.DATA_BUFFER_SIZE,
					SendBufferSize = ConstantValues.DATA_BUFFER_SIZE
				};

				receiveBuffer = new byte[ConstantValues.DATA_BUFFER_SIZE];
				socket.BeginConnect(ip, ConstantValues.SERVER_PORT, ConnectCallback, socket);
			}

			public void InitializeClientData()
			{
				packethandlers = new Dictionary<int, PacketHandler>();
				packethandlers.Add((int)ServerPackets.welcome, ClientHandle.OnWelcomeMessage);
				packethandlers.Add((int)ServerPackets.spawnPlayer, ClientHandle.OnPlayerConnected);
				packethandlers.Add((int)ServerPackets.playerPosition, ClientHandle.OnPlayerPositionUpdate);
				packethandlers.Add((int)ServerPackets.playerRotation, ClientHandle.OnPlayerRotationUpdate);
				packethandlers.Add((int)ServerPackets.playerDisconnected, ClientHandle.OnPlayerDisconnected);
				packethandlers.Add((int)ServerPackets.playerHealth, ClientHandle.OnPlayerHealthUpdate);
				packethandlers.Add((int)ServerPackets.playerRespawn, ClientHandle.OnPlayerRespawn);
				packethandlers.Add((int)ServerPackets.projectileSpawn, ClientHandle.OnProjectileSpawn);
				packethandlers.Add((int)ServerPackets.projectilePosition, ClientHandle.OnProjectiePositionUpdate);
				packethandlers.Add((int)ServerPackets.projectileExplosion, ClientHandle.OnProjectieExplosion);
			}

			private void ConnectCallback(IAsyncResult result)
			{
				socket.EndConnect(result);

				if (!socket.Connected)
					return;

				stream = socket.GetStream();

				receivedData = new Packet();

				stream.BeginRead(receiveBuffer, 0, ConstantValues.DATA_BUFFER_SIZE, ReceiveCallback, null);
			}

			private void ReceiveCallback(IAsyncResult result)
			{
				try
				{
					int byteLength = stream.EndRead(result);

					if (byteLength <= 0)
					{
						Instance.Disconnect();
						return;
					}

					var data = new byte[byteLength];
					Array.Copy(receiveBuffer, data, byteLength);

					receivedData.Reset(HandleData(data));
					stream.BeginRead(receiveBuffer, 0, ConstantValues.DATA_BUFFER_SIZE, ReceiveCallback, null);
				}
				catch
				{
					Disconnect();
				}
			}

			private bool HandleData(byte[] data)
			{
				int packetLength = 0;

				receivedData.SetBytes(data);

				if (receivedData.UnreadLength() >= 4)
				{
					packetLength = receivedData.ReadInt();

					if (packetLength <= 0)
						return true;
				}

				while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
				{
					byte[] packetBytes = receivedData.ReadBytes(packetLength);
					ThreadManager.ExecuteOnMainThread(() =>
					{
						using (Packet packet = new Packet(packetBytes))
						{
							int packetId = packet.ReadInt();
							packethandlers[packetId](packet);
						}
					});

					packetLength = 0;

					if (receivedData.UnreadLength() >= 4)
					{
						packetLength = receivedData.ReadInt();

						if (packetLength <= 0)
							return true;
					}
				}

				if (packetLength <= 1)
					return true;

				return false;
			}

			public void SendData(Packet packet)
			{
				try
				{
					if (socket != null)
					{
						stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
					}
				}
				catch (Exception ex)
				{
					Debug.Log($"Error sending data to server via TCP: {ex}");
				}
			}

			public void Disconnect()
			{
				Instance.Disconnect();

				stream = null;
				receivedData = null;
				receiveBuffer = null;
				socket = null;
			}

		}
		public class UDP
		{
			public UdpClient socket;
			private IPEndPoint endpoint;

			public UDP(string ip)
			{
				endpoint = new IPEndPoint(IPAddress.Parse(ip), ConstantValues.SERVER_PORT);
			}

			public void Connect(int localPort)
			{
				socket = new UdpClient(localPort);
				socket.Connect(endpoint);

				socket.BeginReceive(ReceiveCallback, null);

				using (Packet packet = new Packet())
				{
					SendData(packet);
				}
			}

			public void SendData(Packet packet)
			{
				try
				{
					packet.InsertInt(Instance.MyId);
					if (socket != null)
					{
						socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
					}
				}
				catch (Exception ex)
				{
					Debug.Log($"Error sending data to server vis UDP: {ex}");
				}
			}

			private void ReceiveCallback(IAsyncResult result)
			{
				try
				{
					byte[] data = socket.EndReceive(result, ref endpoint);
					socket.BeginReceive(ReceiveCallback, null);

					if (data.Length < 4)
					{
						Instance.Disconnect();
						return;
					}

					HandleData(data);
				}
				catch
				{
					Disconnect();
				}
			}

			private void HandleData(byte[] data)
			{
				using (Packet packet = new Packet(data))
				{
					int packetLength = packet.ReadInt();
					data = packet.ReadBytes(packetLength);
				}

				ThreadManager.ExecuteOnMainThread(() =>
				{
					using (Packet packet = new Packet(data))
					{
						int packetId = packet.ReadInt();
						packethandlers[packetId](packet);
					}
				});
			}

			private void Disconnect()
			{
				Instance.Disconnect();

				endpoint = null;
				socket = null;
			}

		}
	}
}