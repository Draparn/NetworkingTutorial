using NetworkTutorial.Server.Client;
using NetworkTutorial.Server.Gameplay;
using NetworkTutorial.Shared.Net;
using NetworkTutorial.Shared.Utils;
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

			packet.Write(ServerSnapshot.currentSnapshot.SequenceNumber);

			packet.Write((byte)ServerSnapshot.currentSnapshot.PlayerPositions.Count);
			foreach (var playerPosData in ServerSnapshot.currentSnapshot.PlayerPositions.Values)
			{
				packet.Write(playerPosData.Id);
				packet.Write(playerPosData.SequenceNumber);
				packet.Write(playerPosData.Position);
				packet.Write(playerPosData.Rotation);
			}

			packet.Write((byte)ServerSnapshot.currentSnapshot.ProjectilePositions.Count);
			foreach (var proj in ServerSnapshot.currentSnapshot.ProjectilePositions)
			{
				packet.Write(proj.id);
				packet.Write(proj.transform.position);
			}

			SendToAllClients(packet);
			packet.Reset();
			ServerSnapshot.ClearSnapshot();
		}

		public static void SendPlayerConnected_CLIENT(byte clientId, PlayerServer player)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.playerSpawn);

			packet.Write(player.PlayerId);
			packet.Write(player.PlayerName);
			packet.Write(player.transform.position);
			packet.Write(player.transform.rotation);

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

		public static void SendPlayerHealthUpdate_ALL(PlayerServer player)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.playerHealth);

			packet.Write(player.PlayerId);
			packet.Write((byte)player.CurrentHealth);
			packet.Write(ValueTypeConversions.ReturnDecimalsAsShort(player.CurrentHealth));

			SendToAllClients(packet);
			packet.Reset();
		}
		public static void SendPlayerSwitchedWeapon_ALL(PlayerServer player)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.playerWeaponSwitch);

			packet.Write(player.PlayerId);
			packet.Write(player.currentWeaponSlot);

			SendToAllClients(packet);
			packet.Reset();
		}
		public static void SendPlayerRespawned_ALL(PlayerServer player)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.playerRespawn);

			packet.Write(player.PlayerId);
			packet.Write(player.transform.position);

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
		public static void SendHealthpackSpawn_CLIENT(byte clientId, byte healthPackId, Vector3 position, bool isActive)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.healthpackSpawn);

			packet.Write(healthPackId);
			packet.Write(position);
			packet.Write(isActive);

			SendToClient(clientId, packet);
			packet.Reset();
		}

		public static void SendProjectileSpawn_ALL(ProjectileServer projectile)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.projectileSpawn);

			packet.Write(projectile.id);
			packet.Write(projectile.transform.position);
			packet.Write(projectile.shotFromWeapon);

			SendToAllClients(packet);
			packet.Reset();
		}

		public static void SendProjectileExplosion_ALL(ProjectileServer projectile)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.projectileExplosion);

			packet.Write(projectile.id);
			packet.Write(projectile.transform.position);

			SendToAllClients(packet);
			packet.Reset();
		}


		#region BroadcastOptions
		private static void SendToClient(byte clientId, Packet packet)
		{
			packet.WriteLength();
			if (Server.Clients[clientId].Connection.endPoint != null)
				Server.Clients[clientId].Connection.SendData(packet);
		}

		private static void SendToAllClients(Packet packet)
		{
			packet.WriteLength();
			for (byte i = 1; i <= Server.MaxPlayers; i++)
			{
				if (Server.Clients[i].Connection.endPoint == null)
					continue;

				Server.Clients[i].Connection.SendData(packet);
			}
		}

		private static void SendToAllClientsExcept(byte clientIdToExclude, Packet packet)
		{
			packet.WriteLength();
			for (byte i = 1; i <= Server.MaxPlayers; i++)
			{
				if (i == clientIdToExclude || Server.Clients[i].Connection.endPoint == null)
					continue;

				Server.Clients[i].Connection.SendData(packet);
			}
		}
		#endregion
	}
}
