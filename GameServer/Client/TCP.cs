using System;
using System.Net.Sockets;

namespace GameServer
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
			socket.ReceiveBufferSize = ConstantValues.DATABUFFER_SIZE;
			socket.SendBufferSize = ConstantValues.DATABUFFER_SIZE;

			stream = socket.GetStream();

			receivedData = new Packet();
			receiveBuffer = new byte[ConstantValues.DATABUFFER_SIZE];

			stream.BeginRead(receiveBuffer, 0, ConstantValues.DATABUFFER_SIZE, ReceiveCallback, null);

			ServerSend.Welcome(id, "Welcome to the party, pal!");
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
				Console.WriteLine($"Error sending data to player {id} via TCP: {ex}");
			}
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

				stream.BeginRead(receiveBuffer, 0, ConstantValues.DATABUFFER_SIZE, ReceiveCallback, null);
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

	}
}
