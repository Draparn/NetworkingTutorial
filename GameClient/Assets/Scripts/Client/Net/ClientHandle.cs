﻿using NetworkTutorial.Client.Gameplay;
using NetworkTutorial.Client.Player;
using NetworkTutorial.Shared.Net;
using NetworkTutorial.Shared.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Client.Net
{
	public struct PlayerPosData
	{
		public byte PlayerId;
		public ushort Sequencenumber;
		public Vector3 Position;
		public Quaternion Rotation;

		public PlayerPosData(byte playerId, ushort sequenceNumber, Vector3 position, Quaternion rotation)
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
		public static void OnWelcomeMessage(Packet packet)
		{
			var id = packet.ReadByte();

			LocalClient.Instance.StopConnectionTimer();
			LocalClient.Instance.MyId = id;
			LocalClient.Instance.isConnected = true;

			UIManager.Instance.GameOn();

			ClientSend.SendWelcomeReceived();
		}

		public static void OnServerFull(Packet packet)
		{
			var message = packet.ReadString();

			Debug.Log($"Message from server: {message}");
			LocalClient.Instance.StopConnectionTimer();
		}

		public static void OnNewSnapshot(Packet packet)
		{
			byte amount, playerId;
			ushort projId, sequenceNumber;
			Vector3 position;
			Quaternion rotation;
			var players = new List<PlayerPosData>();
			var projectiles = new List<ProjectileData>();

			//players
			amount = packet.ReadByte();
			for (int i = 0; i < amount; i++)
			{
				playerId = packet.ReadByte();

				sequenceNumber = packet.ReadUShort();
				position = packet.ReadVector3();
				rotation = packet.ReadQuaternion();

				if (GameManagerClient.Instance.Players.ContainsKey(playerId))
					players.Add(new PlayerPosData(playerId, sequenceNumber, position, rotation));
			}

			//projcetiles
			amount = packet.ReadByte();
			for (int i = 0; i < amount; i++)
			{
				projId = packet.ReadUShort();
				position = packet.ReadVector3();

				if (GameManagerClient.Instance.Projectiles.ContainsKey(projId))
					projectiles.Add(new ProjectileData(projId, position));
			}

			ClientSnapshot.Snapshots.Add(new ClientSnapshot(players, projectiles));
		}

		public static void OnPlayerConnected(Packet packet)
		{
			var id = packet.ReadByte();
			var playerName = packet.ReadString();
			var position = packet.ReadVector3();
			var rotation = packet.ReadQuaternion();

			GameManagerClient.Instance.SpawnPlayer(id, playerName, position, rotation);
		}
		public static void OnPlayerDisconnected(Packet packet)
		{
			var clientId = packet.ReadByte();

			if (clientId == LocalClient.Instance.MyId)
			{
				if (LocalClient.Instance.isConnected)
				{
					LocalClient.Instance.isConnected = false;
					LocalClient.Instance.Connection.socket.Close();
					UIManager.Instance.GameOff();
				}
			}

			GameObject.Destroy(GameManagerClient.Instance.Players[clientId].gameObject);
			GameManagerClient.Instance.Players.Remove(clientId);
		}

		public static void OnPlayerHealthUpdate(Packet packet)
		{
			var clientId = packet.ReadByte();
			var wholeNumber = packet.ReadByte();
			var decimalsValue = ValueTypeConversions.ReturnShortAsFloat(packet.ReadShort());

			GameManagerClient.Instance.Players[clientId].SetHealth(clientId, (float)wholeNumber + decimalsValue);
		}

		public static void OnPlayerRespawn(Packet packet)
		{
			var id = packet.ReadByte();
			var position = packet.ReadVector3();

			GameManagerClient.Instance.Players[id].Respawn(position);
		}

		public static void OnHealthpackActivate(Packet packet)
		{
			GameManagerClient.Instance.HealthpackActivate(packet.ReadByte());
		}
		public static void OnHealthpackDeactivate(Packet packet)
		{
			GameManagerClient.Instance.HealthpackDeactivate(packet.ReadByte());
		}
		public static void OnHealthpackSpawn(Packet packet)
		{
			var id = packet.ReadByte();
			var position = packet.ReadVector3();

			GameManagerClient.Instance.SpawnHealthPack(id, position);
		}

		public static void OnProjectileSpawn(Packet packet)
		{
			var id = packet.ReadUShort();
			var position = packet.ReadVector3();

			GameManagerClient.Instance.SpawnProjectile(id, position);
		}

		public static void OnProjectieExplosion(Packet packet)
		{
			var id = packet.ReadUShort();
			var position = packet.ReadVector3();

			GameManagerClient.Instance.Projectiles[id].Explode(position);
		}

	}
}