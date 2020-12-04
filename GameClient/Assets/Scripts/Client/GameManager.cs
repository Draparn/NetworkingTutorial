using NetworkTutorial.Client.Net;
using NetworkTutorial.Client.Player;
using NetworkTutorial.Shared;
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
		private Dictionary<int, Vector3> playersOriginalPositions = new Dictionary<int, Vector3>();
		private Dictionary<int, Vector3> projectilesOriginalPositions = new Dictionary<int, Vector3>();

		public GameObject LocalPlayerPrefab;
		public GameObject RemotePlayerPrefab;
		public GameObject ProjectilePrefab;
		public GameObject HealthpackPrefab;

		private Transform projectilePool;
		private Transform pickups;

		private float snapshotBufferTimer;
		private float lerpValue = 0;
		private float bufferTimeMultiplier = 1;

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
			snapshotBufferTimer = ConstantValues.CLIENT_SNAPSHOT_BUFFER_LENGTH;
		}

		private void Update()
		{
			if (snapshotBufferTimer <= 0)
			{
				//players
				foreach (var tuple in ClientSnapshot.Snapshots[0].players)
				{
					if (Players.ContainsKey(tuple.Item1))
					{
						if (!playersOriginalPositions.ContainsKey(tuple.Item1))
							playersOriginalPositions.Add(tuple.Item1, Players[tuple.Item1].transform.position);

						Players[tuple.Item1].transform.position = Vector3.Lerp(playersOriginalPositions[tuple.Item1], tuple.Item2, lerpValue / (ConstantValues.SERVER_TICK_RATE * bufferTimeMultiplier));
					}
				}

				//projectiles
				foreach (var tuple in ClientSnapshot.Snapshots[0].projectiles)
				{
					if (Projectiles.ContainsKey(tuple.Item1))
					{
						if (!projectilesOriginalPositions.ContainsKey(tuple.Item1))
							projectilesOriginalPositions.Add(tuple.Item1, Projectiles[tuple.Item1].transform.position);

						Projectiles[tuple.Item1].transform.position = Vector3.Lerp(projectilesOriginalPositions[tuple.Item1], tuple.Item2, lerpValue / (ConstantValues.SERVER_TICK_RATE * bufferTimeMultiplier));
					}
				}

				lerpValue += Time.deltaTime;

				if (lerpValue / (ConstantValues.SERVER_TICK_RATE * bufferTimeMultiplier) >= 1)
				{
					lerpValue = 0;
					ClientSnapshot.Snapshots.RemoveAt(0);

					if (ClientSnapshot.Snapshots.Count < 2)
					{
						bufferTimeMultiplier = 2.5f;
					}
					else if (ClientSnapshot.Snapshots.Count <= 3)
					{
						bufferTimeMultiplier = 1.0f;
					}
					else
					{
						bufferTimeMultiplier = 0.5f;
					}

					playersOriginalPositions.Clear();
					projectilesOriginalPositions.Clear();
				}
			}
			else if (ClientSnapshot.Snapshots.Count > 0)
				snapshotBufferTimer -= Time.deltaTime;
		}

		public void SpawnPlayer(bool isLocal, int playerId, string playerName, Vector3 pos, Quaternion rot)
		{
			var player = Instantiate(isLocal ? LocalPlayerPrefab : RemotePlayerPrefab, pos, rot);
			var playermanagerComponent = player.GetComponent<PlayerManager>();
			playermanagerComponent.Init(playerId, playerName);

			Players.Add(playerId, playermanagerComponent);
		}

		public void SpawnProjectile(int id, Vector3 position)
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
