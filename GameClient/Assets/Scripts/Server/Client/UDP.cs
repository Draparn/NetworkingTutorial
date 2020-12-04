using NetworkTutorial.Shared;
using NetworkTutorial.Shared.Net;
using System.Net;

namespace NetworkTutorial.Server.Client
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
				using (Packet pkt = new Packet(packetBytes))
				{
					var packetId = pkt.ReadInt();
					Server.PacketHandlers[packetId](id, pkt);
				}
			});
		}

		public void Disconnect()
		{
			endPoint = null;
		}

	}
}
