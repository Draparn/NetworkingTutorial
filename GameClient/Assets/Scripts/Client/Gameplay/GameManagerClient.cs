using NetworkTutorial.Client.Net;
using NetworkTutorial.Client.Player;
using NetworkTutorial.Shared;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Client.Gameplay
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

	public class GameManagerClient : MonoBehaviour
	{
		public static GameManagerClient Instance;

		public Dictionary<int, PlayerClient> Players = new Dictionary<int, PlayerClient>();
		public Dictionary<int, ProjectileClient> Projectiles = new Dictionary<int, ProjectileClient>();
		public Dictionary<byte, GameObject> Healthpacks = new Dictionary<byte, GameObject>();
		private Dictionary<int, Vector3> projectilesOriginalPositions = new Dictionary<int, Vector3>();
		private Dictionary<int, Vector3> playersOriginalPositions = new Dictionary<int, Vector3>();

		public List<LocalPredictionData> LocalPositionPredictions = new List<LocalPredictionData>();

		public GameObject LocalPlayerPrefab;
		public GameObject RemotePlayerPrefab;
		public GameObject ProjectilePrefab;
		public GameObject HealthpackPrefab;

		[SerializeField] private Transform projectilePool;
		[SerializeField] private Transform pickups;

		private float lerpValue;
		private float bufferTimeMultiplier = 1;

		private void Awake()
		{
			if (Instance == null)
				Instance = this;
			else if (Instance != this)
				Destroy(this);
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
							if (playerData.PlayerId == LocalClient.Instance.MyId)
								continue;
							else
							{
								if (!playersOriginalPositions.ContainsKey(playerData.PlayerId))
									playersOriginalPositions.Add(playerData.PlayerId, Players[playerData.PlayerId].transform.position);

								Players[playerData.PlayerId].transform.position = Vector3.Lerp(
									playersOriginalPositions[playerData.PlayerId],
									playerData.Position,
									lerpValue / (ConstantValues.SERVER_TICK_RATE * bufferTimeMultiplier)
									);
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

		public Vector3 GetLastPredictedPos()
		{
			if (LocalPositionPredictions.Count < 1)
				return Vector3.zero;

			return LocalPositionPredictions[LocalPositionPredictions.Count - 1].Position;
		}

		public void SpawnPlayer(int playerId, string playerName, Vector3 pos, Quaternion rot)
		{
			var player = Instantiate(playerId == LocalClient.Instance.MyId ? LocalPlayerPrefab : RemotePlayerPrefab, pos, rot);
			var playerComponent = player.GetComponent<PlayerClient>();
			playerComponent.Init(playerId, playerName);

			Players.Add(playerId, playerComponent);
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
				var projectileManagerComponent = Instantiate(ProjectilePrefab, position, Quaternion.identity, projectilePool).GetComponent<ProjectileClient>();

				Projectiles.Add(id, projectileManagerComponent);
			}
		}

		public void SpawnHealthPack(byte id, Vector3 position)
		{
			if (!Healthpacks.ContainsKey(id))
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
