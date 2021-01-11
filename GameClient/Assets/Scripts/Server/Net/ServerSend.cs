using NetworkTutorial.Server.Client;
using NetworkTutorial.Server.Gameplay;
using NetworkTutorial.Shared.Net;
using System.Net;
using UnityEngine;

namespace NetworkTutorial.Server.Net
{
	public class ServerSend
	{
		public static void SendWelcomeMessage_CLIENT(byte clientId)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.welcome);

			packet.Write(clientId);

			SendToClient(clientId, packet);
			packet.Reset();
		}

		public static void SendServerFull(IPEndPoint endpoint)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.serverFull);

			packet.Write("Server is full.");

			Server.SendPacket(endpoint, packet);
			packet.Reset();
		}

		public static void SendSnapshot()
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.serverSnapshot);

			packet.Write((byte)ServerSnapshot.currentSnapshot.PlayerPosition.Count);
			foreach (var playerPosData in ServerSnapshot.currentSnapshot.PlayerPosition.Values)
			{
				packet.Write(playerPosData.Id);

				packet.Write(playerPosData.SequenceNumber);

				packet.Write(playerPosData.Position.x);
				packet.Write(playerPosData.Position.y);
				packet.Write(playerPosData.Position.z);
			}

			packet.Write((byte)ServerSnapshot.currentSnapshot.ProjectilePositions.Count);
			foreach (var proj in ServerSnapshot.currentSnapshot.ProjectilePositions)
			{
				packet.Write(proj.id);

				packet.Write(proj.transform.position.x);
				packet.Write(proj.transform.position.y);
				packet.Write(proj.transform.position.z);
			}

			SendToAllClients(packet);
			packet.Reset();
			ServerSnapshot.ClearSnapshot();
		}

		public static void SendPlayerConnected_CLIENT(byte clientId, PlayerServer player)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.spawnPlayer);

			packet.Write(player.PlayerId);
			packet.Write(player.PlayerName);

			packet.Write(player.transform.position.x);
			packet.Write(player.transform.position.y);
			packet.Write(player.transform.position.z);

			packet.Write(player.transform.rotation.y);
			packet.Write(player.transform.rotation.w);

			SendToClient(clientId, packet);
			packet.Reset();
		}
		public static void SendPlayerDisconnected_ALL(byte clientId)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.playerDisconnected);

			packet.Write(clientId);

			SendToAllClients(packet);
			packet.Reset();
		}

		public static void SendPlayerRotationUpdate_ALLEXCEPT(PlayerServer player)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.playerRotation);

			packet.Write(player.PlayerId);
			packet.Write(player.transform.rotation.y);
			packet.Write(player.transform.rotation.w);

			SendToAllClientsExcept(player.PlayerId, packet);
			packet.Reset();
		}

		public static void SendPlayerHealthUpdate_ALL(PlayerServer player)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.playerHealth);

			packet.Write(player.PlayerId);
			packet.Write(player.CurrentHealth);

			SendToAllClients(packet);
			packet.Reset();
		}
		public static void SendPlayerRespawned_ALL(PlayerServer player)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.playerRespawn);

			packet.Write(player.PlayerId);
			packet.Write(player.transform.position.x);
			packet.Write(player.transform.position.y);
			packet.Write(player.transform.position.z);

			SendToAllClients(packet);
			packet.Reset();
		}

		public static void SendHealthpackDeactivate_ALL(byte healthpackId)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.healthpackDeactivate);

			packet.Write(healthpackId);

			SendToAllClients(packet);
			packet.Reset();
		}
		public static void SendHealthpackActivate_ALL(byte healthpackId)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.healthpackActivate);

			packet.Write(healthpackId);

			SendToAllClients(packet);
			packet.Reset();
		}
		public static void SendHealthpackSpawn_CLIENT(byte clientId, byte healthPackId, Vector3 position)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.healthpackSpawn);

			packet.Write(healthPackId);
			packet.Write(position.x);
			packet.Write(position.y);
			packet.Write(position.z);

			SendToClient(clientId, packet);
			packet.Reset();
		}

		public static void SendProjectileSpawn_ALL(ProjectileServer projectile)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.projectileSpawn);

			packet.Write(projectile.id);
			packet.Write(projectile.transform.position.x);
			packet.Write(projectile.transform.position.y);
			packet.Write(projectile.transform.position.z);

			SendToAllClients(packet);
			packet.Reset();
		}

		public static void SendProjectileExplosion_ALL(ProjectileServer projectile)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.projectileExplosion);

			packet.Write(projectile.id);
			packet.Write(projectile.transform.position.x);
			packet.Write(projectile.transform.position.y);
			packet.Write(projectile.transform.position.z);

			SendToAllClients(packet);
			packet.Reset();
		}


		#region BroadcastOptions
		private static void SendToClient(byte clientId, Packet packet)
		{
			packet.WriteLength();
			Server.Clients[clientId].Connection.SendData(packet);
		}

		private static void SendToAllClients(Packet packet)
		{
			packet.WriteLength();
			for (byte i = 1; i <= Server.MaxPlayers; i++)
			{
				if (Server.Clients[i].Connection.endPoint != null)
				{
					Server.Clients[i].Connection.SendData(packet);
				}
			}
		}

		private static void SendToAllClientsExcept(byte clientIdToExclude, Packet packet)
		{
			packet.WriteLength();
			for (byte i = 1; i <= Server.MaxPlayers; i++)
			{
				if (i == clientIdToExclude)
					continue;

				Server.Clients[i].Connection.SendData(packet);
			}
		}
		#endregion
	}
}
