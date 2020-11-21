namespace GameServer
{
	public class ServerSend
	{
		public static void SendWelcome(int clientId, string msg)
		{
			using (Packet packet = new Packet((int)ServerPackets.welcome))
			{
				packet.Write(msg);
				packet.Write(clientId);

				SendTCPDataToClient(clientId, packet);
			}
		}

		public static void SendSpawnPlayer(int clientId, Player player)
		{
			using (Packet packet = new Packet((int)ServerPackets.spawnPlayer))
			{
				packet.Write(player.PlayerId);
				packet.Write(player.PlayerName);
				packet.Write(player.Position);
				packet.Write(player.Rotation);

				SendTCPDataToClient(clientId, packet);
			}
		}

		public static void SendUpdatePlayerPosition(Player player)
		{
			using (Packet packet = new Packet((int)ServerPackets.playerPosition))
			{
				packet.Write(player.PlayerId);
				packet.Write(player.Position);

				SendUDPDataToAllClients(packet);
			}
		}
		public static void SendUpdatePlayerRotation(Player player)
		{
			using (Packet packet = new Packet((int)ServerPackets.playerRotation))
			{
				packet.Write(player.PlayerId);
				packet.Write(player.Rotation);

				SendUDPDataToAllClientsExcept(player.PlayerId, packet);
			}
		}

		private static void SendTCPDataToClient(int clientId, Packet packet)
		{
			packet.WriteLength();
			Server.Clients[clientId].tcp.SendData(packet);
		}
		private static void SendUDPDataToClient(int clientId, Packet packet)
		{
			packet.WriteLength();
			Server.Clients[clientId].udp.SendData(packet);
		}

		private static void SendTCPDataToAllClients(Packet packet)
		{
			packet.WriteLength();
			for (int i = 1; i <= Server.MaxPlayers; i++)
			{
				Server.Clients[i].tcp.SendData(packet);
			}
		}
		private static void SendUDPDataToAllClients(Packet packet)
		{
			packet.WriteLength();
			for (int i = 1; i <= Server.MaxPlayers; i++)
			{
				if (Server.Clients[i].udp != null)
				{
					Server.Clients[i].udp.SendData(packet);
				}
			}
		}

		private static void SendTCPDataToAllClientsExcept(int clientIdToExclude, Packet packet)
		{
			packet.WriteLength();
			for (int i = 1; i <= Server.MaxPlayers; i++)
			{
				if (i == clientIdToExclude)
					continue;

				Server.Clients[i].tcp.SendData(packet);
			}
		}
		private static void SendUDPDataToAllClientsExcept(int clientIdToExclude, Packet packet)
		{
			packet.WriteLength();
			for (int i = 1; i <= Server.MaxPlayers; i++)
			{
				if (i == clientIdToExclude)
					continue;

				Server.Clients[i].udp.SendData(packet);
			}
		}

	}
}
