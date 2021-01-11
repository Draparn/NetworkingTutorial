using NetworkTutorial.Client.Gameplay;
using NetworkTutorial.Client.Player;
using NetworkTutorial.Shared.Net;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Client.Net
{
	public struct PlayerPosData
	{
		public byte PlayerId;
		public ushort Sequencenumber;
		public Vector3 Position;

		public PlayerPosData(byte playerId, ushort sequenceNumber, Vector3 position)
		{
			PlayerId = playerId;
			Sequencenumber = sequenceNumber;
			Position = position;
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
			byte amount;
			byte playerId;
			ushort projId;
			ushort sequenceNumber;
			Vector3 position;
			var players = new List<PlayerPosData>();
			var projectiles = new List<ProjectileData>();


			//players
			amount = packet.ReadByte();
			for (int i = 0; i < amount; i++)
			{
				playerId = packet.ReadByte();

				sequenceNumber = packet.ReadUShort();

				position.x = packet.ReadFloat();
				position.y = packet.ReadFloat();
				position.z = packet.ReadFloat();

				if (GameManagerClient.Instance.Players.ContainsKey(playerId))
					players.Add(new PlayerPosData((byte)playerId, sequenceNumber, position));
			}

			//projcetiles
			amount = packet.ReadByte();
			for (int i = 0; i < amount; i++)
			{
				projId = packet.ReadUShort();

				position.x = packet.ReadFloat();
				position.y = packet.ReadFloat();
				position.z = packet.ReadFloat();

				if (GameManagerClient.Instance.Projectiles.ContainsKey(projId))
					projectiles.Add(new ProjectileData(projId, position));
			}

			ClientSnapshot.Snapshots.Add(new ClientSnapshot(players, projectiles));
		}

		public static void OnPlayerConnected(Packet packet)
		{
			var id = packet.ReadByte();
			var playerName = packet.ReadString();
			var position = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
			var rotation = new Quaternion(0, packet.ReadFloat(), 0, packet.ReadFloat());

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

			GameManagerClient.Instance.Players.Remove(clientId);
			GameObject.Destroy(GameManagerClient.Instance.Players[clientId].gameObject);
		}

		public static void OnPlayerRotationUpdate(Packet packet)
		{
			var id = packet.ReadByte();
			var rotation = new Quaternion(0, packet.ReadFloat(), 0, packet.ReadFloat());

			GameManagerClient.Instance.Players[id].transform.rotation = rotation;
		}

		public static void OnPlayerHealthUpdate(Packet packet)
		{
			var clientId = packet.ReadByte();
			var newHealth = packet.ReadFloat();
			GameManagerClient.Instance.Players[clientId].SetHealth(clientId, newHealth);
		}
		public static void OnPlayerRespawn(Packet packet)
		{
			var id = packet.ReadByte();
			var pos = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());

			GameManagerClient.Instance.Players[id].Respawn(pos);
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
			var pos = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());

			GameManagerClient.Instance.SpawnHealthPack(id, pos);
		}

		public static void OnProjectileSpawn(Packet packet)
		{
			var id = packet.ReadUShort();
			var pos = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());

			GameManagerClient.Instance.SpawnProjectile(id, pos);
		}

		public static void OnProjectieExplosion(Packet packet)
		{
			var id = packet.ReadUShort();
			var pos = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());

			GameManagerClient.Instance.Projectiles[id].Explode(pos);
		}

	}
}