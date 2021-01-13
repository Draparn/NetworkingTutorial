using NetworkTutorial.Server.Client;
using NetworkTutorial.Server.Net;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Server.Gameplay
{
	public class GameManagerServer : MonoBehaviour
	{
		public static GameManagerServer Instance;

		public GameObject PlayerPrefab;
		public GameObject ProjectilePrefab;

		public static Dictionary<byte, HealthpackServer> healthpacks = new Dictionary<byte, HealthpackServer>();

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
					if (client.DisconnectTimer >= 10.0f)
						client.Disconnect();
				}
			}
		}

		public PlayerServer InstantiatePlayer()
		{
			return Instantiate(PlayerPrefab, new Vector3(24, 4, 22), Quaternion.identity).GetComponent<PlayerServer>();
		}

		public ProjectileServer InstantiateProjectile(Transform shootOrigin, Vector3 viewDirection)
		{
			return Instantiate(ProjectilePrefab, shootOrigin.position + viewDirection * 0.7f, Quaternion.identity).GetComponent<ProjectileServer>();
		}

		public static void AddHealthpackToDict(HealthpackServer hps)
		{
			hps.MyId = nextHealthpackId;
			healthpacks.Add(nextHealthpackId, hps);
			nextHealthpackId++;
		}
		public static void DeactivateHealthpack(byte id)
		{
			healthpacks[id].IsActive = false;
			ServerSend.SendHealthpackDeactivate_ALL(id);
		}
		private static void ActivateHealthpack(HealthpackServer hp)
		{
			if (!hp.RespawnCollisionCheck())
			{
				hp.IsActive = true;
				ServerSend.SendHealthpackActivate_ALL(hp.MyId);
			}
		}

	}
}
