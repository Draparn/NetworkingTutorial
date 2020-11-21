using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace GameServer
{
	public class UDP
	{
		public IPEndPoint endPoint;

		private int id;

		public UDP(int id)
		{
			this.id = id;
		}

		public void Connect(IPEndPoint endPoint)
		{
			this.endPoint = endPoint;
		}

		public void SendData(Packet packet)
		{
			Server.SendUDPData(endPoint, packet);
		}

		public void HandleData(Packet packet)
		{
			int packetLength = packet.ReadInt();
			byte[] packetBytes = packet.ReadBytes(packetLength);

			ThreadManager.ExecuteOnMainThread(() =>
			{
				using (Packet packet = new Packet(packetBytes))
				{
					int packetId = packet.ReadInt();
					Server.PacketHandlers[packetId](id, packet);
				}
			});
		}

		public void Disconnect()
		{
			endPoint = null;
		}

	}
}
