using NetworkTutorial.Shared.Net;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Client.Net
{
	public struct PlayerPosData
	{
		public int PlayerId;
		public uint FrameNumber;
		public Vector3 Position;

		public PlayerPosData(int playerId, uint frameNumber, Vector3 position)
		{
			PlayerId = playerId;
			FrameNumber = frameNumber;
			Position = position;
		}
	}
	public struct ProjectileData
	{
		public int ProjectileId;
		public Vector3 Position;

		public ProjectileData(int projectileId, Vector3 position)
		{
			ProjectileId = projectileId;
			Position = position;
		}
	}

	public class ClientHandle : MonoBehaviour
	{
		public static void OnWelcomeMessage(Packet packet)
		{
			var message = packet.ReadString();
			var id = packet.ReadByte();

			Debug.Log($"Message from server: {message}");
			Client.Instance.MyId = id;
			Client.Instance.StopConnectionTimer();

			ClientSend.SendWelcomeReceived();
		}

		public static void OnNewSnapshot(Packet packet)
		{
			int amount;
			int id;
			uint frameNumber;
			Vector3 position;
			var players = new List<PlayerPosData>();
			var projectiles = new List<ProjectileData>();


			//players
			amount = packet.ReadInt();
			for (int i = 0; i < amount; i++)
			{
				id = packet.ReadInt();
				frameNumber = packet.ReadUInt();
				position = packet.ReadVector3();

				if (GameManager.Instance.Players.ContainsKey(id))
				{
					players.Add(new PlayerPosData(id, frameNumber, position));
				}
			}

			//projcetiles
			amount = packet.ReadInt();
			for (int i = 0; i < amount; i++)
			{
				id = packet.ReadInt();
				position = packet.ReadVector3();

				if (GameManager.Instance.Projectiles.ContainsKey(id))
					projectiles.Add(new ProjectileData(id, position));
			}

			ClientSnapshot.Snapshots.Add(new ClientSnapshot(players, projectiles));
		}

		public static void OnPlayerConnected(Packet packet)
		{
			var id = packet.ReadByte();
			var playerName = packet.ReadString();
			var position = packet.ReadVector3();
			var rotation = packet.ReadQuaternion();

			GameManager.Instance.SpawnPlayer(id == Client.Instance.MyId, id, playerName, position, rotation);
		}
		public static void OnPlayerDisconnected(Packet packet)
		{
			var clientId = packet.ReadInt();

			Destroy(GameManager.Instance.Players[clientId].gameObject);
			GameManager.Instance.Players.Remove(clientId);
		}

		public static void OnPlayerRotationUpdate(Packet packet)
		{
			var id = packet.ReadInt();
			var rotation = packet.ReadQuaternion();

			GameManager.Instance.Players[id].transform.rotation = rotation;
		}

		public static void OnPlayerHealthUpdate(Packet packet)
		{
			var clientId = packet.ReadInt();
			var newHealth = packet.ReadFloat();
			GameManager.Instance.Players[clientId].SetHealth(clientId, newHealth);
		}
		public static void OnPlayerRespawn(Packet packet)
		{
			var id = packet.ReadInt();
			var pos = packet.ReadVector3();

			GameManager.Instance.Players[id].Respawn(pos);
		}

		public static void OnHealthpackActivate(Packet packet)
		{
			GameManager.Instance.HealthpackActivate(packet.ReadByte());
		}
		public static void OnHealthpackDeactivate(Packet packet)
		{
			GameManager.Instance.HealthpackDeactivate(packet.ReadByte());
		}
		public static void OnHealthpackSpawn(Packet packet)
		{
			var id = packet.ReadByte();
			var pos = packet.ReadVector3();

			GameManager.Instance.SpawnHealthPack(id, pos);
		}

		public static void OnProjectileSpawn(Packet packet)
		{
			var id = packet.ReadInt();
			var pos = packet.ReadVector3();

			GameManager.Instance.SpawnProjectile(id, pos);
		}

		public static void OnProjectieExplosion(Packet packet)
		{
			var id = packet.ReadInt();
			var pos = packet.ReadVector3();

			GameManager.Instance.Projectiles[id].Explode(pos);
		}

	}
}