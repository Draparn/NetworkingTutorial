using NetworkTutorial.Server.Client;
using NetworkTutorial.Shared.Net;
using UnityEngine;

namespace NetworkTutorial.Server.Net
{
	public class ServerSend
	{
		public static void SendWelcomeMessage_TCP_CLIENT(int clientId, string msg)
		{
			using (Packet packet = new Packet((int)ServerPackets.welcome))
			{
				packet.Write(msg);
				packet.Write(clientId);

				SendTCPDataToClient(clientId, packet);
			}
		}

		public static void SendSnapshot()
		{
			using (Packet packet = new Packet((int)ServerPackets.serverSnapshot))
			{
				packet.Write(ServerSnapshot.currentSnapshot.PlayerPositions.Count);
				foreach (var player in ServerSnapshot.currentSnapshot.PlayerPositions)
				{
					packet.Write(player.PlayerId);
					packet.Write(player.transform.position);
				}

				packet.Write(ServerSnapshot.currentSnapshot.ProjectilePositions.Count);
				foreach (var proj in ServerSnapshot.currentSnapshot.ProjectilePositions)
				{
					packet.Write(proj.id);
					packet.Write(proj.transform.position);
				}

				SendUDPDataToAllClients(packet);
			}

			ServerSnapshot.ClearSnapshot();
		}

		public static void SendPlayerConnected_TCP_CLIENT(int clientId, Player player)
		{
			using (Packet packet = new Packet((int)ServerPackets.spawnPlayer))
			{
				packet.Write(player.PlayerId);
				packet.Write(player.PlayerName);
				packet.Write(player.transform.position);
				packet.Write(player.transform.rotation);

				SendTCPDataToClient(clientId, packet);
			}
		}
		public static void SendPlayerDisconnected_TCP_ALL(int clientId)
		{
			using (Packet packet = new Packet((int)ServerPackets.playerDisconnected))
			{
				packet.Write(clientId);

				SendTCPDataToAllClients(packet);
			}
		}
		
		//obsolete since snapshot implementation
		public static void SendPlayerPositionUpdate_UDP_ALL(Player player)
		{
			using (Packet packet = new Packet((int)ServerPackets.playerPosition))
			{
				packet.Write(player.PlayerId);
				packet.Write(player.transform.position);

				SendUDPDataToAllClients(packet);
			}
		}
		public static void SendPlayerRotationUpdate_UDP_ALLEXCEPT(Player player)
		{
			using (Packet packet = new Packet((int)ServerPackets.playerRotation))
			{
				packet.Write(player.PlayerId);
				packet.Write(player.transform.rotation);

				SendUDPDataToAllClientsExcept(player.PlayerId, packet);
			}
		}

		public static void SendPlayerHealthUpdate_TCP_ALL(Player player)
		{
			using (Packet packet = new Packet((int)ServerPackets.playerHealth))
			{
				packet.Write(player.PlayerId);
				packet.Write(player.CurrentHealth);

				SendTCPDataToAllClients(packet);
			}
		}
		public static void SendPlayerRespawned_TCP_ALL(Player player)
		{
			using (Packet packet = new Packet((int)ServerPackets.playerRespawn))
			{
				packet.Write(player.PlayerId);
				packet.Write(player.transform.position);

				SendTCPDataToAllClients(packet);
			}
		}

		public static void SendHealthpackDeactivate_TCP_ALL(byte healthpackId)
		{
			using (Packet packet = new Packet((int)ServerPackets.healthpackDeactivate))
			{
				packet.Write(healthpackId);

				SendTCPDataToAllClients(packet);
			}
		}
		public static void SendHealthpackActivate_TCP_ALL(byte healthpackId)
		{
			using (Packet packet = new Packet((int)ServerPackets.healthpackActivate))
			{
				packet.Write(healthpackId);

				SendTCPDataToAllClients(packet);
			}
		}
		public static void SendHealthpackSpawn_TCP_CLIENT(int clientId, byte id, Vector3 position)
		{
			using (Packet packet = new Packet((int)ServerPackets.healthpackSpawn))
			{
				packet.Write(id);
				packet.Write(position);

				SendTCPDataToClient(clientId, packet);
			}
		}

		public static void SendProjectileSpawn_TCP_ALL(Projectile projectile)
		{
			using (Packet packet = new Packet((int)ServerPackets.projectileSpawn))
			{
				packet.Write(projectile.id);
				packet.Write(projectile.transform.position);

				SendTCPDataToAllClients(packet);
			}
		}
		//obsolete since snapshot implementation
		public static void SendProjectileUpdatePosition_UDP_ALL(Projectile projectile)
		{
			using (Packet packet = new Packet((int)ServerPackets.projectilePosition))
			{
				packet.Write(projectile.id);
				packet.Write(projectile.transform.position);

				SendUDPDataToAllClients(packet);
			}
		}
		public static void SendProjectileExplosion_TCP_ALL(Projectile projectile)
		{
			using (Packet packet = new Packet((int)ServerPackets.projectileExplosion))
			{
				packet.Write(projectile.id);
				packet.Write(projectile.transform.position);

				SendTCPDataToAllClients(packet);
			}
		}

		#region BroadcastOptions
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
			for (ushort i = 1; i <= Server.MaxPlayers; i++)
			{
				Server.Clients[i].tcp.SendData(packet);
			}
		}
		private static void SendUDPDataToAllClients(Packet packet)
		{
			packet.WriteLength();
			for (ushort i = 1; i <= Server.MaxPlayers; i++)
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
			for (ushort i = 1; i <= Server.MaxPlayers; i++)
			{
				if (i == clientIdToExclude)
					continue;

				Server.Clients[i].tcp.SendData(packet);
			}
		}
		private static void SendUDPDataToAllClientsExcept(int clientIdToExclude, Packet packet)
		{
			packet.WriteLength();
			for (ushort i = 1; i <= Server.MaxPlayers; i++)
			{
				if (i == clientIdToExclude)
					continue;

				Server.Clients[i].udp.SendData(packet);
			}
		}
		#endregion
	}
}
