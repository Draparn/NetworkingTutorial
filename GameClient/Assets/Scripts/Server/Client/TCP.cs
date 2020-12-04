using NetworkTutorial.Shared;
using NetworkTutorial.Shared.Net;
using UnityEngine;
using System.Net.Sockets;
using System;
using NetworkTutorial.Server.Net;

namespace NetworkTutorial.Server.Client
{
	public class TCP
	{
		public TcpClient socket;
		private NetworkStream stream;
		private Packet receivedData;

		private readonly int id;

		private byte[] receiveBuffer;

		public TCP(int id)
		{
			this.id = id;
		}

		public void Connect(TcpClient socket)
		{
			this.socket = socket;
			socket.ReceiveBufferSize = ConstantValues.DATA_BUFFER_SIZE;
			socket.SendBufferSize = ConstantValues.DATA_BUFFER_SIZE;

			stream = socket.GetStream();

			receivedData = new Packet();
			receiveBuffer = new byte[ConstantValues.DATA_BUFFER_SIZE];

			stream.BeginRead(receiveBuffer, 0, ConstantValues.DATA_BUFFER_SIZE, ReceiveCallback, null);

			ServerSend.SendWelcomeMessage_TCP_CLIENT(id, "Welcome to the party, pal!");
		}

		public void SendData(Packet packet)
		{
			try
			{
				if (socket != null)
					stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
			}
			catch (Exception ex)
			{
				Debug.Log($"Error sending data to player {id} via TCP: {ex}");
			}
		}

		private void ReceiveCallback(IAsyncResult result)
		{
			try
			{
				int byteLength = stream.EndRead(result);

				if (byteLength <= 0)
				{
					Server.Clients[id].Disconnect();
					return;
				}

				var data = new byte[byteLength];
				Array.Copy(receiveBuffer, data, byteLength);

				receivedData.Reset(HandleData(data));

				stream.BeginRead(receiveBuffer, 0, ConstantValues.DATA_BUFFER_SIZE, ReceiveCallback, null);
			}
			catch (Exception ex)
			{
				Debug.Log($"Error receiving TCP data: {ex}");
				Server.Clients[id].Disconnect();
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
						var packetId = packet.ReadInt();
						Server.PacketHandlers[packetId](id, packet);
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

		public void Disconnect()
		{
			socket.Close();
			stream = null;
			receivedData = null;
			receiveBuffer = null;
			socket = null;
		}

	}
}
