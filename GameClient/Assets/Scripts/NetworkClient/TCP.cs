using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace Assets.Scripts.Client
{
	public class TCP : Client
	{
		public TcpClient socket;

		private NetworkStream stream;
		private Packet receivedData;

		byte[] receiveBuffer;

		public void Connect(string ip)
		{
			socket = new TcpClient
			{
				ReceiveBufferSize = ConstantValues.dataBufferSize,
				SendBufferSize = ConstantValues.dataBufferSize
			};

			receiveBuffer = new byte[ConstantValues.dataBufferSize];
			socket.BeginConnect(ip, ConstantValues.Port, ConnectCallback, socket);
		}

		public void InitializeClientData()
		{
			packethandlers = new Dictionary<int, PacketHandler>();
			packethandlers.Add((int)ServerPackets.welcome, ClientHandle.Welcome);
		}

		private void ConnectCallback(IAsyncResult result)
		{
			socket.EndConnect(result);

			if (!socket.Connected)
				return;

			stream = socket.GetStream();

			receivedData = new Packet();

			stream.BeginRead(receiveBuffer, 0, ConstantValues.dataBufferSize, ReceiveCallback, null);
		}

		private void ReceiveCallback(IAsyncResult result)
		{
			try
			{
				int byteLength = stream.EndRead(result);

				if (byteLength <= 0)
				{
					//disconnect here
					return;
				}

				var data = new byte[byteLength];
				Array.Copy(receiveBuffer, data, byteLength);

				receivedData.Reset(HandleData(data));
				stream.BeginRead(receiveBuffer, 0, ConstantValues.dataBufferSize, ReceiveCallback, null);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error receiving TCP data: {ex}");
				//disconnect here
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

	}
}
