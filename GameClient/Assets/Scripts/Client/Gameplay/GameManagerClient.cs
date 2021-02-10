using NetworkTutorial.Client.Net;
using NetworkTutorial.Client.Player;
using NetworkTutorial.Shared;
using NetworkTutorial.Shared.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Client.Gameplay
{
	public struct LocalPredictionData
	{
		public ushort SequenceNumber;
		public Vector3 Position;
		public Transform Transform;
		public float yVelocityPreMove;
		public bool IsGroundedPreMove;
		public InputsStruct Inputs;

		public LocalPredictionData(ushort sequenceNumber, InputsStruct inputs, Vector3 position, Transform transform, float yVelocityPreMove, bool isGroundedPreMove)
		{
			SequenceNumber = sequenceNumber;
			Inputs = inputs;
			Position = position;
			Transform = transform;
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
		public Dictionary<byte, GameObject> WeaponPickups = new Dictionary<byte, GameObject>();
		private Dictionary<int, Vector3> projectilesOriginalPositions = new Dictionary<int, Vector3>();
		private Dictionary<int, Tuple<Vector3, Quaternion>> playersOriginalPosAndRot = new Dictionary<int, Tuple<Vector3, Quaternion>>();

		public List<LocalPredictionData> LocalPositionPredictions = new List<LocalPredictionData>();

		public GameObject LocalPlayerPrefab, RemotePlayerPrefab, HealthpackPrefab;

		[SerializeField] private Transform projectilePool, pickups;

		private float lerpValue, bufferTimeMultiplier = 1;

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
								if (!playersOriginalPosAndRot.ContainsKey(playerData.PlayerId))
									playersOriginalPosAndRot.Add(playerData.PlayerId, new Tuple<Vector3, Quaternion>(
											Players[playerData.PlayerId].transform.position,
											Players[playerData.PlayerId].transform.rotation));

								//position
								Players[playerData.PlayerId].transform.position = Vector3.Lerp(
									playersOriginalPosAndRot[playerData.PlayerId].Item1,
									playerData.Position,
									lerpValue / (ConstantValues.SERVER_TICK_RATE * bufferTimeMultiplier)
									);

								//rotation
								Players[playerData.PlayerId].transform.rotation = Quaternion.Lerp(
									playersOriginalPosAndRot[playerData.PlayerId].Item2,
									playerData.Rotation,
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

					playersOriginalPosAndRot.Clear();
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

		public Vector3 GetPredictedPositionBySequence(ushort targetSequenceNumber)
		{
			for (int i = 0; i < LocalPositionPredictions.Count; i++)
			{
				if (LocalPositionPredictions[i].SequenceNumber == targetSequenceNumber)
					return LocalPositionPredictions[i].Position;
			}

			Debug.LogError("Error: LocalPositionPredictions didn't contain targetSequenceNumber.");
			return Vector3.zero;
		}

		public void SpawnPlayer(byte playerId, string playerName, Vector3 pos, Quaternion rot)
		{
			var player = Instantiate(playerId == LocalClient.Instance.MyId ? LocalPlayerPrefab : RemotePlayerPrefab, pos, rot);
			var playerComponent = player.GetComponent<PlayerClient>();
			playerComponent.Init(playerId, playerName);

			Players.Add(playerId, playerComponent);
		}

		public void SpawnProjectile(int id, Vector3 position, byte shotFromWeapon)
		{
			if (Projectiles.ContainsKey(id))
				Projectiles[id].transform.position = position;
			else
			{
				var projectileManagerComponent =
					Instantiate(Weapons.AllWeapons[shotFromWeapon].ProjectilePrefabClient, position, Quaternion.identity, projectilePool).GetComponent<ProjectileClient>();
				Projectiles.Add(id, projectileManagerComponent);
			}
		}

		public void SpawnWeapon(byte id, WeaponSlot slot, Vector3 pos, bool isActive)
		{
			if (!WeaponPickups.ContainsKey(id))
				WeaponPickups.Add(id, Instantiate(Weapons.AllWeapons[(int)slot].ClientPrefab, pos, Quaternion.identity, pickups));

			WeaponPickups[id].SetActive(isActive);
		}
		public void WeaponUpdate(byte id, bool isActive)
		{
			WeaponPickups[id].SetActive(isActive);
		}

		public void SpawnHealthPack(byte id, Vector3 position, bool isActive)
		{
			if (!Healthpacks.ContainsKey(id))
				Healthpacks.Add(id, Instantiate(HealthpackPrefab, position, Quaternion.identity, pickups));
			
			Healthpacks[id].SetActive(isActive);
		}
		public void HealthpackUpdate(byte id, bool isActive)
		{
			Healthpacks[id].SetActive(isActive);
		}

	}
}
