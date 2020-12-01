using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Client
{
	public class GameManager : MonoBehaviour
	{
		public static GameManager Instance;

		public Dictionary<int, PlayerManager> Players = new Dictionary<int, PlayerManager>();
		public Dictionary<int, ProjectileManager> Projectiles = new Dictionary<int, ProjectileManager>();
		public Dictionary<byte, GameObject> Healthpacks = new Dictionary<byte, GameObject>();

		public GameObject LocalPlayerPrefab;
		public GameObject RemotePlayerPrefab;
		public GameObject ProjectilePrefab;
		public GameObject HealthpackPrefab;

		private Transform projectilePool;
		private Transform pickups;

		private void Awake()
		{
			if (Instance == null)
				Instance = this;
			else if (Instance != this)
				Destroy(this);
		}

		private void Start()
		{
			projectilePool = GameObject.Find("ProjectilePool").transform;
			pickups = GameObject.Find("Pickups").transform;
		}

		public void SpawnPlayer(bool isLocal, int playerId, string playerName, Vector3 pos, Quaternion rot)
		{
			var player = Instantiate(isLocal ? LocalPlayerPrefab : RemotePlayerPrefab, pos, rot);
			var playermanagerComponent = player.GetComponent<PlayerManager>();
			playermanagerComponent.Init(playerId, playerName);

			Players.Add(playerId, playermanagerComponent);
		}

		public void SpawnProjectile(ushort id, Vector3 position)
		{
			if (Projectiles.ContainsKey(id))
			{
				Projectiles[id].transform.position = position;
				Projectiles[id].gameObject.SetActive(true);
			}
			else
			{
				var projectileManagerComponent = Instantiate(ProjectilePrefab, position, Quaternion.identity, projectilePool).GetComponent<ProjectileManager>();
				projectileManagerComponent.Init(id);

				Projectiles.Add(id, projectileManagerComponent);
			}
		}

		public void SpawnHealthPack(byte id, Vector3 position)
		{
			Healthpacks.Add(id, Instantiate(HealthpackPrefab, position, Quaternion.identity, pickups));
		}
		public void HealthpackActivate(byte id)
		{
			Healthpacks[id].SetActive(true);
		}
		public void HealthpackDeactivate(byte id)
		{
			Healthpacks[id].SetActive(false);
		}

	}
}
