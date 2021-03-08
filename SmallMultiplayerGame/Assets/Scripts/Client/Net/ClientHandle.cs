using SmallMultiplayerGame.Client.Gameplay;
using SmallMultiplayerGame.Client.Gameplay.Player;
using SmallMultiplayerGame.Shared;
using SmallMultiplayerGame.Shared.Net;
using SmallMultiplayerGame.Shared.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace SmallMultiplayerGame.Client.Net
{
	public struct PlayerPosData
	{
		public byte PlayerId;
		public uint Sequencenumber;
		public Vector3 Position;
		public Quaternion Rotation;

		public PlayerPosData(byte playerId, uint sequenceNumber, Vector3 position, Quaternion rotation)
		{
			PlayerId = playerId;
			Sequencenumber = sequenceNumber;
			Position = position;
			Rotation = rotation;
		}
	}
	public struct ProjectileData
	{
		public ushort ProjectileId;
		public Vector3 Position;

		public ProjectileData(ushort projectileId, Vector3 position)
		{
			ProjectileId = projectileId;
			Position = position;
		}
	}

	public class ClientHandle
	{
		private static float decimalsValue;
		private static byte count, wholeNumber;

		public static void OnWelcomeMessage(Packet packet)
		{
			LocalClient.Instance.StopConnectionTimer();
			LocalClient.Instance.MyId = packet.ReadByte();
			LocalClient.Instance.isConnected = true;

			UIManager.Instance.GameOn();

			ClientSend.SendWelcomeReceived();
		}

		public static void OnServerFull(Packet packet)
		{
			UIManager.Instance.ShowErrorMessage(packet.ReadString());
			LocalClient.Instance.StopConnectionTimer();
		}

		public static void OnNameTaken(Packet packet)
		{
			UIManager.Instance.ShowErrorMessage(packet.ReadString());

			if (LocalClient.Instance.isConnected)
			{
				LocalClient.Instance.Disconnect();
				UIManager.Instance.ShowMainMenu();
			}
		}

		public static void OnNewSnapshot(Packet packet)
		{
			var players = new List<PlayerPosData>();
			var projectiles = new List<ProjectileData>();
			Vector3 position;
			Quaternion rotation;
			uint snapshotSequenceNumber, playerSequenceNumber;
			ushort id;

			snapshotSequenceNumber = packet.ReadUInt();
			//players
			count = packet.ReadByte();
			for (int i = 0; i < count; i++)
			{
				id = packet.ReadByte();

				playerSequenceNumber = packet.ReadUInt();
				position = packet.ReadVector3();
				rotation = packet.ReadQuaternion();

				if (GameManagerClient.Instance.Players.ContainsKey(id))
					players.Add(new PlayerPosData((byte)id, playerSequenceNumber, position, rotation));
			}

			//projcetiles
			count = packet.ReadByte();
			for (int i = 0; i < count; i++)
			{
				id = packet.ReadUShort();
				position = packet.ReadVector3();

				if (GameManagerClient.Instance.Projectiles.ContainsKey(id))
					projectiles.Add(new ProjectileData(id, position));
			}

			//Elevator
			if (packet.ReadBool())
			{
				var lerpValue = ValueTypeConversions.ReturnByteAsFloat(packet.ReadByte());
				GameManagerClient.Instance.elevator.ClientElevatorMove(lerpValue);
			}
			else
				GameManagerClient.Instance.elevator.ClientElevatorMove(null);

			ClientSnapshot.Snapshots.Add(new ClientSnapshot(snapshotSequenceNumber, players, projectiles));
		}

		public static void OnPlayerConnected(Packet packet)
		{
			var clientId = packet.ReadByte();
			var playerName = packet.ReadString();
			var position = packet.ReadVector3();
			var rotation = packet.ReadQuaternion();

			GameManagerClient.Instance.SpawnPlayer(clientId, playerName, position, rotation);
		}
		public static void OnPlayerDisconnected(Packet packet)
		{
			var clientId = packet.ReadByte();

			if (clientId == LocalClient.Instance.MyId)
			{
				if (LocalClient.Instance.isConnected)
				{
					LocalClient.Instance.Disconnect();
					UIManager.Instance.ShowMainMenu();
				}
			}

			GameObject.Destroy(GameManagerClient.Instance.Players[clientId].gameObject);
			GameManagerClient.Instance.Players.Remove(clientId);
		}

		public static void OnPlayerHealthUpdate(Packet packet)
		{
			var clientId = packet.ReadByte();
			wholeNumber = packet.ReadByte();
			decimalsValue = ValueTypeConversions.ReturnShortAsFloat(packet.ReadShort());

			GameManagerClient.Instance.Players[clientId].SetHealth(clientId, wholeNumber + decimalsValue);
		}
		public static void OnPlayerWeaponSwitch(Packet packet)
		{
			var id = packet.ReadByte();
			var slot = packet.ReadByte();

			GameManagerClient.Instance.Players[id].SetWeaponMesh(id, slot);
		}
		public static void OnPlayerWeaponPickup(Packet packet)
		{
			var slot = packet.ReadByte();
			var player = PlayerController.Instance;

			var weapon = player.pickedUpWeapons[slot];
			weapon.IsPickedUp = packet.ReadBool();
			weapon.Ammo = packet.ReadUShort();

			UIManager.Instance.NewWeaponAvailable(slot);

			if (player.currentWeapon == slot)
				UIManager.Instance.SetAmmoCount(weapon.Ammo.ToString());
		}
		public static void OnPlayerWeaponAmmoUpdate(Packet packet)
		{
			PlayerController.Instance.NewAmmoCount(packet.ReadUShort());
		}
		public static void OnPlayerFiredWeapon(Packet packet)
		{
			GameManagerClient.Instance.Players[packet.ReadByte()].FireWeapon();
		}

		public static void OnPlayerRespawn(Packet packet)
		{
			var clientId = packet.ReadByte();

			GameManagerClient.Instance.Players[clientId].Respawn(packet.ReadVector3(), clientId);
		}

		public static void OnWeaponSpawn(Packet packet)
		{
			GameManagerClient.Instance.SpawnWeapon(packet.ReadByte(), (WeaponSlot)packet.ReadByte(), packet.ReadVector3(), packet.ReadBool());
		}
		public static void OnWeaponPickupStatusUpdate(Packet packet)
		{
			GameManagerClient.Instance.WeaponUpdate(packet.ReadByte(), packet.ReadBool());
		}

		public static void OnHealthpackUpdate(Packet packet)
		{
			GameManagerClient.Instance.HealthpackUpdate(packet.ReadByte(), packet.ReadBool());
		}
		public static void OnHealthpackSpawn(Packet packet)
		{
			GameManagerClient.Instance.SpawnHealthPack(packet.ReadByte(), packet.ReadVector3(), packet.ReadBool(), packet.ReadByte());
		}

		public static void OnProjectileSpawn(Packet packet)
		{
			ushort projId = packet.ReadUShort();

			if (GameManagerClient.Instance.Projectiles.ContainsKey(projId))
				GameManagerClient.Instance.Projectiles[projId].gameObject.SetActive(true);

			GameManagerClient.Instance.SpawnProjectile(projId, packet.ReadVector3(), packet.ReadByte());
		}
		public static void OnProjectieExplosion(Packet packet)
		{
			GameManagerClient.Instance.Projectiles[packet.ReadUShort()].Explode(packet.ReadVector3());
		}

	}
}