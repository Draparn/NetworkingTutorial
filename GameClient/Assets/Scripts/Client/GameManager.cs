using NetworkTutorial.Client.Net;
using NetworkTutorial.Client.Player;
using NetworkTutorial.Shared;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Client
{
	public struct LocalPredictionData
	{
		public uint FrameNumber;
		public Transform TransformAfterMove;
		public float yVelocityPreMove;
		public bool IsGroundedPreMove;
		public bool[] Inputs;

		public LocalPredictionData(uint frameNumber, bool[] inputs, float yVelocityPreMove, bool isGroundedPreMove, Transform transformAfterMove)
		{
			FrameNumber = frameNumber;
			Inputs = inputs;
			this.yVelocityPreMove = yVelocityPreMove;
			IsGroundedPreMove = isGroundedPreMove;
			TransformAfterMove = transformAfterMove;
		}
	}

	public class GameManager : MonoBehaviour
	{
		public static GameManager Instance;

		public Dictionary<int, PlayerManager> Players = new Dictionary<int, PlayerManager>();
		public Dictionary<int, ProjectileManager> Projectiles = new Dictionary<int, ProjectileManager>();
		public Dictionary<byte, GameObject> Healthpacks = new Dictionary<byte, GameObject>();
		private Dictionary<int, Vector3> projectilesOriginalPositions = new Dictionary<int, Vector3>();
		private Dictionary<int, Vector3> playersOriginalPositions = new Dictionary<int, Vector3>();

		public List<LocalPredictionData> LocalPositionPredictions = new List<LocalPredictionData>();

		public GameObject LocalPlayerPrefab;
		public GameObject RemotePlayerPrefab;
		public GameObject ProjectilePrefab;
		public GameObject HealthpackPrefab;

		private Transform projectilePool;
		private Transform pickups;

		private float snapshotBufferTimer = ConstantValues.CLIENT_SNAPSHOT_BUFFER_LENGTH;
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
		}

		private void Update()
		{
			if (snapshotBufferTimer <= 0)
			{
				//players
				foreach (var playerData in ClientSnapshot.Snapshots[0].players)
				{
					if (Client.Instance.MyId == playerData.PlayerId)
					{
						for (int i = 0; i < LocalPositionPredictions.Count; i++)
						{
							if (LocalPositionPredictions[i].FrameNumber == playerData.FrameNumber)
							{
								if (Vector3.Distance(LocalPositionPredictions[i].TransformAfterMove.position, playerData.Position) > 1.4f)
								{
									Debug.LogError("Correcting...");
									LocalPositionPredictions.RemoveRange(0, i);

									for (int j = 0; j < LocalPositionPredictions.Count; j++)
									{
										if (j == 0)
											LocalPositionPredictions[j].TransformAfterMove.position = playerData.Position;
										else
										{
											LocalPositionPredictions[j].TransformAfterMove.position += PlayerMovementCalculations.CalculatePlayerPosition(
												LocalPositionPredictions[j].Inputs,
												LocalPositionPredictions[j - 1].TransformAfterMove,
												LocalPositionPredictions[j].yVelocityPreMove,
												LocalPositionPredictions[j].IsGroundedPreMove
												);
										}

										PlayerController.correctPos = LocalPositionPredictions[j].TransformAfterMove.position;
									}

									break;
								}

								LocalPositionPredictions.RemoveRange(0, i + 1);
								break;
							}
						}

						continue;
					}

					if (Players.ContainsKey(playerData.PlayerId))
					{
						if (!playersOriginalPositions.ContainsKey(playerData.PlayerId))
							playersOriginalPositions.Add(playerData.PlayerId, Players[playerData.PlayerId].transform.position);

						Players[playerData.PlayerId].transform.position =
							Vector3.Lerp(playersOriginalPositions[playerData.PlayerId], playerData.Position, lerpValue / (ConstantValues.SERVER_TICK_RATE * bufferTimeMultiplier));
					}
				}

				//projectiles
				foreach (var projData in ClientSnapshot.Snapshots[0].projectiles)
				{
					if (Projectiles.ContainsKey(projData.ProjectileId))
					{
						if (!projectilesOriginalPositions.ContainsKey(projData.ProjectileId))
							projectilesOriginalPositions.Add(projData.ProjectileId, Projectiles[projData.ProjectileId].transform.position);

						Projectiles[projData.ProjectileId].transform.position = Vector3.Lerp(projectilesOriginalPositions[projData.ProjectileId], projData.Position, lerpValue / (ConstantValues.SERVER_TICK_RATE));
					}
				}

				lerpValue += Time.deltaTime;

				if (lerpValue / (ConstantValues.SERVER_TICK_RATE * bufferTimeMultiplier) >= 1)
				{
					lerpValue = 0;
					ClientSnapshot.Snapshots.RemoveAt(0);

					if (ClientSnapshot.Snapshots.Count < 2)
						bufferTimeMultiplier = 2.5f;
					else if (ClientSnapshot.Snapshots.Count <= 3)
						bufferTimeMultiplier = 1.0f;
					else
						bufferTimeMultiplier = 0.5f;

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
