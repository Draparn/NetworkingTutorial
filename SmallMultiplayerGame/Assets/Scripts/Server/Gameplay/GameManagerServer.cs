using SmallMultiplayerGame.Server.Client;
using SmallMultiplayerGame.Server.Net;
using SmallMultiplayerGame.Shared;
using System.Collections.Generic;
using UnityEngine;

namespace SmallMultiplayerGame.Server.Gameplay
{
	public class GameManagerServer : MonoBehaviour
	{
		public static GameManagerServer Instance;

		private Dictionary<byte, HealthpackServer> healthpacks = new Dictionary<byte, HealthpackServer>();
		public List<Vector3> respawnPoints = new List<Vector3>();
		public GameObject PlayerPrefab;

		private static byte nextHealthpackId = 0;

		private void Awake()
		{
			if (Instance == null)
				Instance = this;
			else
				Destroy(this);
		}

		private void Update()
		{
			//Healthpacks
			foreach (var healthpack in healthpacks.Values)
			{
				if (healthpack.IsActive)
					continue;

				healthpack.currentRespawnTime -= Time.deltaTime;
				if (healthpack.currentRespawnTime <= 0)
					ActivateHealthpack(healthpack);
			}

			//Clients
			foreach (var client in Server.Clients.Values)
			{
				if (client.Connection.endPoint != null)
				{
					client.DisconnectTimer += Time.deltaTime;
					if (client.DisconnectTimer >= ConstantValues.CONNECTION_TIMEOUT_TIMER)
						client.Disconnect();
				}
			}
		}

		public PlayerServer InstantiatePlayer()
		{
			return Instantiate(PlayerPrefab, respawnPoints[Random.Range(0, respawnPoints.Count)], Quaternion.identity).GetComponent<PlayerServer>();
		}

		public ProjectileServer InstantiateProjectile(Transform shootOrigin, Vector3 viewDirection, GameObject projectilePrefab)
		{
			return Instantiate(projectilePrefab, shootOrigin.position + viewDirection * 0.7f, Quaternion.identity).GetComponent<ProjectileServer>();
		}

		public void AddHealthpackToDict(HealthpackServer hps)
		{
			hps.MyId = nextHealthpackId;
			healthpacks.Add(nextHealthpackId, hps);
			nextHealthpackId++;
		}
		public void DeactivateHealthpack(byte id)
		{
			healthpacks[id].IsActive = false;
			ServerSend.SendHealthpackStatusUpdate_ALL(id, false);
		}
		private static void ActivateHealthpack(HealthpackServer hp)
		{
			if (!hp.RespawnCollisionCheck())
			{
				hp.IsActive = true;
				ServerSend.SendHealthpackStatusUpdate_ALL(hp.MyId, hp.IsActive);
			}
		}
		public Dictionary<byte, HealthpackServer> GetHealthpacks()
		{
			return healthpacks;
		}

	}
}
