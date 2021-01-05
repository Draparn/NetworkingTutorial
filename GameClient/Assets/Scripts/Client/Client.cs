using NetworkTutorial.Client.Net;
using NetworkTutorial.Client.Player;
using NetworkTutorial.Shared;
using NetworkTutorial.Shared.Net;
using System;
using System.Collections;
using System.Collections.Generic;
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

		public UDP udp;

		[HideInInspector] public byte MyId = 0;

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
			udp = new UDP(ip);
			udp.InitializeClientData();
			udp.Connect();
			StartCoroutine(ConnectTimer());
		}

		public void Disconnect()
		{
			if (isConnected)
			{
				ClientSend.SendDisconnect();
				isConnected = false;
				udp.socket.Close();
			}
		}

		public class UDP
		{
			public UdpClient socket;
			private IPEndPoint endpoint;

			public UDP(string ip)
			{
				endpoint = new IPEndPoint(IPAddress.Parse(ip), ConstantValues.PORT);
			}

			public void Connect()
			{
				socket = new UdpClient(ConstantValues.PORT + 1);
				socket.Connect(endpoint);
				socket.BeginReceive(ReceiveCallback, null);

				ClientSend.SendConnectRequest();
			}

			public void InitializeClientData()
			{
				packethandlers = new Dictionary<int, PacketHandler>();
				packethandlers.Add((int)ServerPackets.welcome, ClientHandle.OnWelcomeMessage);
				packethandlers.Add((int)ServerPackets.spawnPlayer, ClientHandle.OnPlayerConnected);
				packethandlers.Add((int)ServerPackets.playerRotation, ClientHandle.OnPlayerRotationUpdate);
				packethandlers.Add((int)ServerPackets.playerDisconnected, ClientHandle.OnPlayerDisconnected);
				packethandlers.Add((int)ServerPackets.playerHealth, ClientHandle.OnPlayerHealthUpdate);
				packethandlers.Add((int)ServerPackets.playerRespawn, ClientHandle.OnPlayerRespawn);
				packethandlers.Add((int)ServerPackets.projectileSpawn, ClientHandle.OnProjectileSpawn);
				packethandlers.Add((int)ServerPackets.projectileExplosion, ClientHandle.OnProjectieExplosion);
				packethandlers.Add((int)ServerPackets.healthpackActivate, ClientHandle.OnHealthpackActivate);
				packethandlers.Add((int)ServerPackets.healthpackDeactivate, ClientHandle.OnHealthpackDeactivate);
				packethandlers.Add((int)ServerPackets.healthpackSpawn, ClientHandle.OnHealthpackSpawn);
				packethandlers.Add((int)ServerPackets.serverSnapshot, ClientHandle.OnNewSnapshot);
			}

			public void SendData(Packet packet)
			{
				try
				{
					packet.InsertByte(Instance.MyId);
					if (socket != null)
					{
						socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
					}
				}
				catch (Exception ex)
				{
					Debug.Log($"Error sending data to server via UDP: {ex}");
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
						int packetId = packet.ReadByte();
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

		private IEnumerator ConnectTimer()
		{
			yield return new WaitForSeconds(10);
			UIManager.Instance.ConnectionTimedOut();
		}

		public void StopConnectionTimer()
		{
			StopAllCoroutines();
			isConnected = true;
		}
	}
}