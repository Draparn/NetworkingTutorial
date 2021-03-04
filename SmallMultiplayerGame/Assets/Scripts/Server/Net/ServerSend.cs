using SmallMultiplayerGame.Server.Client;
using SmallMultiplayerGame.Server.Gameplay;
using SmallMultiplayerGame.Server.Gameplay.Pickups;
using SmallMultiplayerGame.Shared;
using SmallMultiplayerGame.Shared.Net;
using SmallMultiplayerGame.Shared.Utils;
using System.Net;
using UnityEngine;

namespace SmallMultiplayerGame.Server.Net
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

		public static void SendNameTaken(byte clientId)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.nameTaken);

			packet.Write("Connection error: Name was already taken.");

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

		public static void SendNewClientInfo(byte clientId)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.newClientInfo);

			//All new info here...

			SendToClient(clientId, packet);
			packet.Reset();
		}

		public static void SendSnapshot()
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.serverSnapshot);

			packet.Write(ServerSnapshot.currentSnapshot.SequenceNumber);

			//Players
			var playerPositions = ServerSnapshot.currentSnapshot.PlayerPositions;
			packet.Write((byte)playerPositions.Count);
			foreach (var playerPosData in playerPositions.Values)
			{
				packet.Write(playerPosData.Id);
				packet.Write(playerPosData.SequenceNumber);
				packet.Write(playerPosData.Position);
				packet.Write(playerPosData.Rotation);
			}

			//Projectiles
			var projectilePositions = ServerSnapshot.currentSnapshot.ProjectilePositions;
			packet.Write((byte)projectilePositions.Count);
			foreach (var proj in projectilePositions)
			{
				packet.Write(proj.id);
				packet.Write(proj.transform.position);
			}

			//Elevator
			var lerpValue = ServerSnapshot.currentSnapshot.lerpValue;
			packet.Write(lerpValue != null);
			if (lerpValue != null)
				packet.Write((byte)lerpValue);

			SendToAllClients(packet);
			packet.Reset();
			ServerSnapshot.ClearSnapshot();
		}

		public static void SendPlayerConnected_CLIENT(byte clientId, PlayerObjectServer player)
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

		public static void SendPlayerHealthUpdate_ALL(PlayerObjectServer player)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.playerHealth);

			packet.Write(player.PlayerId);
			packet.Write((byte)player.CurrentHealth);
			packet.Write(ValueTypeConversions.ReturnDecimalsAsShort(player.CurrentHealth));

			SendToAllClients(packet);
			packet.Reset();
		}
		public static void SendPlayerSwitchedWeapon_ALL(PlayerObjectServer player)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.playerWeaponSwitch);

			packet.Write(player.PlayerId);
			packet.Write(player.currentWeaponSlot);

			SendToAllClients(packet);
			packet.Reset();
		}
		public static void SendPlayerFiredWeapon_ALL(byte playerId)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.playerFiredWeapon);

			packet.Write(playerId);

			SendToAllClients(packet);
			packet.Reset();
		}
		public static void SendPlayerRespawned_ALL(PlayerObjectServer player)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.playerRespawn);

			packet.Write(player.PlayerId);
			packet.Write(player.transform.position);

			SendToAllClients(packet);
			packet.Reset();
		}

		public static void SendHealthpackStatusUpdate_ALL(byte healthpackId, bool isActive)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.healthpackStatusUpdate);

			packet.Write(healthpackId);
			packet.Write(isActive);

			SendToAllClients(packet);
			packet.Reset();
		}
		public static void SendHealthpackSpawn_CLIENT(byte clientId, byte healthpackId, HealthpackServer hps)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.healthpackSpawn);

			packet.Write(healthpackId);
			packet.Write(hps.gameObject.transform.position);
			packet.Write(hps.IsActive);
			packet.Write((byte)hps.size);

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

		public static void SendWeaponSpawn_CLIENT(byte clientId, WeaponSlot weaponSlot, byte weaponId, Vector3 position, bool isActive)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.weaponSpawn);

			packet.Write(weaponId);
			packet.Write((byte)weaponSlot);
			packet.Write(position);
			packet.Write(isActive);

			SendToClient(clientId, packet);
			packet.Reset();
		}
		public static void SendWeaponPickedUp_CLIENT(byte playerId, WeaponSlot slot, bool isPickedUp, ushort ammoCount)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.weaponPickup);

			packet.Write((byte)slot);
			packet.Write(isPickedUp);
			packet.Write(ammoCount);

			SendToClient(playerId, packet);
			packet.Reset();
		}
		public static void SendWeaponAmmoUpdate_CLIENT(byte playerId, ushort ammoCount)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.weaponAmmoUpdate);

			packet.Write(ammoCount);

			SendToClient(playerId, packet);
			packet.Reset();
		}
		public static void SendWeaponPickupStatus_ALL(byte id, bool isActive)
		{
			var packet = PacketFactory.GetServerPacketType(ServerPackets.weaponPickupStatus);

			packet.Write(id);
			packet.Write(isActive);

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
