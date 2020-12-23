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
		public Vector3 Position;
		public Vector3 TransformRight;
		public Vector3 TransformForward;
		public float yVelocityPreMove;
		public bool IsGroundedPreMove;
		public InputsStruct Inputs;

		public LocalPredictionData(uint frameNumber, InputsStruct inputs, Vector3 position, Vector3 transformRight, Vector3 transformForward, float yVelocityPreMove, bool isGroundedPreMove)
		{
			FrameNumber = frameNumber;
			Inputs = inputs;
			Position = position;
			TransformRight = transformRight;
			TransformForward = transformForward;
			this.yVelocityPreMove = yVelocityPreMove;
			IsGroundedPreMove = isGroundedPreMove;
		}
		public LocalPredictionData(LocalPredictionData prediction)
		{
			this = prediction;
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
			if (ClientSnapshot.Snapshots.Count > 0)
			{
				//players
				if (ClientSnapshot.Snapshots[0].players != null)
				{
					foreach (var playerData in ClientSnapshot.Snapshots[0].players)
					{
						if (Players.ContainsKey(playerData.PlayerId))
						{
							if (!playersOriginalPositions.ContainsKey(playerData.PlayerId))
								playersOriginalPositions.Add(playerData.PlayerId, Players[playerData.PlayerId].transform.position);

							if (Client.Instance.MyId == playerData.PlayerId)
								continue;
							else
							{
								Players[playerData.PlayerId].transform.position = Vector3.Lerp(
										playersOriginalPositions[playerData.PlayerId],
										playerData.Position,
										lerpValue / (ConstantValues.SERVER_TICK_RATE * bufferTimeMultiplier));
							}
						}
					}
				}

				//projectiles
				if (ClientSnapshot.Snapshots[0].projectiles != null)
				{
					foreach (var projData in ClientSnapshot.Snapshots[0].projectiles)
					{
						if (Projectiles.ContainsKey(projData.ProjectileId))
						{
							if (!projectilesOriginalPositions.ContainsKey(projData.ProjectileId))
								projectilesOriginalPositions.Add(projData.ProjectileId, Projectiles[projData.ProjectileId].transform.position);

							Projectiles[projData.ProjectileId].transform.position = Vector3.Lerp(projectilesOriginalPositions[projData.ProjectileId], projData.Position, lerpValue / (ConstantValues.SERVER_TICK_RATE));
						}
					}
				}

				lerpValue += Time.deltaTime;

				if (lerpValue / (ConstantValues.SERVER_TICK_RATE * bufferTimeMultiplier) >= 1)
				{
					lerpValue = 0;
					ClientSnapshot.Snapshots.RemoveAt(0);

					if (ClientSnapshot.Snapshots.Count > 1)
						bufferTimeMultiplier = 0.5f;
					else
						bufferTimeMultiplier = 1.0f;

					playersOriginalPositions.Clear();
					projectilesOriginalPositions.Clear();
				}
			}
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

		/*
		private void CheckPosAndReconcile(PlayerPosData playerData)
		{
			for (int i = 0; i < LocalPositionPredictions.Count; i++)
			{
				if (LocalPositionPredictions[i].FrameNumber == playerData.FrameNumber)
				{
					if (Vector3.Distance(LocalPositionPredictions[i].Position, playerData.Position) > 0.5f)
					{
						Debug.LogError($"Correcting. Index:{i}. Frame:{playerData.FrameNumber}, Predicted pos was: {LocalPositionPredictions[i].Position} and should be: {playerData.Position}");
						LocalPositionPredictions.RemoveRange(0, i);

						for (int j = 0; j < LocalPositionPredictions.Count; j++)
						{
							var prediction = LocalPositionPredictions[j];

							if (j == 0)
							{
								prediction.Position = playerData.Position;
								LocalPositionPredictions[j] = new LocalPredictionData(prediction);
							}
							else
							{
								prediction.Position = LocalPositionPredictions[j - 1].Position +
									PlayerMovementCalculations.CalculatePlayerPosition(
									LocalPositionPredictions[j].Inputs,
									LocalPositionPredictions[j].TransformRight,
									LocalPositionPredictions[j].TransformForward,
									LocalPositionPredictions[j].yVelocityPreMove,
									LocalPositionPredictions[j].IsGroundedPreMove
									);

								LocalPositionPredictions[j] = new LocalPredictionData(prediction);
							}

							//Players[playerData.PlayerId].gameObject.transform.position = LocalPositionPredictions[j].Position;
						}

						break;
					}

					LocalPositionPredictions.RemoveRange(0, i + 1);
					break;
				}
			}

		}
		*/

	}
}
