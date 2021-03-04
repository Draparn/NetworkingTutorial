using SmallMultiplayerGame.Shared;
using SmallMultiplayerGame.Shared.Net;
using System.Net;

namespace SmallMultiplayerGame.Server.Client
{
	public partial class Client
	{
		public class UDP
		{
			public IPEndPoint endPoint;

			private byte clientId;

			public UDP(byte id)
			{
				clientId = id;
			}

			public void Connect(IPEndPoint endPoint)
			{
				this.endPoint = endPoint;
			}

			public void SendData(Packet packet)
			{
				Server.SendPacket(endPoint, packet);
			}

			public void HandleData(Packet packet)
			{
				int packetLength = packet.ReadUShort();
				byte[] packetBytes = packet.ReadBytes(packetLength);

				ThreadManager.ExecuteOnMainThread(() =>
				{
					using (Packet pkt = new Packet(packetBytes))
					{
						var packetId = pkt.ReadByte();
						Server.PacketHandlers[packetId](clientId, pkt);
					}
				});
			}

			public void Disconnect()
			{
				endPoint = null;
			}

		}
	}
}
