using SmallMultiplayerGame.ClientLol.Gameplay.Player;
using SmallMultiplayerGame.ClientLol.Gameplay.WeaponScrips;
using SmallMultiplayerGame.ClientLol.Net;
using SmallMultiplayerGame.Server.Gameplay.Environment;
using SmallMultiplayerGame.Shared;
using SmallMultiplayerGame.Shared.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmallMultiplayerGame.ClientLol.Gameplay
{
	public struct LocalPredictionData
	{
		public Transform Transform;

		public InputsStruct Inputs;
		public Vector3 Position;

		public uint SequenceNumber;
		public float yVelocityPreMove;
		public bool IsGroundedPreMove;

		public LocalPredictionData(uint sequenceNumber, InputsStruct inputs, Vector3 position, Transform transform, float yVelocityPreMove, bool isGroundedPreMove)
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

		public Dictionary<int, PlayerObjectClient> Players = new Dictionary<int, PlayerObjectClient>();
		public Dictionary<int, ProjectileClient> Projectiles = new Dictionary<int, ProjectileClient>();
		public Dictionary<byte, GameObject> Healthpacks = new Dictionary<byte, GameObject>();
		public Dictionary<byte, GameObject> WeaponPickups = new Dictionary<byte, GameObject>();
		private Dictionary<int, Vector3> projectilesOriginalPositions = new Dictionary<int, Vector3>();
		private Dictionary<int, Tuple<Vector3, Quaternion>> playersOriginalPosAndRot = new Dictionary<int, Tuple<Vector3, Quaternion>>();
		public GameObject[] HealthpackPrefabs = new GameObject[3];

		public List<LocalPredictionData> LocalPositionPredictions = new List<LocalPredictionData>();
		private List<ProjectileData> projectiles;
		private List<PlayerPosData> players;
		private ProjectileData projData;
		private PlayerPosData playerData;

		public GameObject LocalPlayerPrefab, RemotePlayerPrefab;
		public Elevator elevator;

		[SerializeField] private Transform projectilePool, pickups;
		private float lerpValue, bufferTimeMultiplier = 1;
		private int count;

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
				players = ClientSnapshot.Snapshots[0].players;

				//players
				if (players != null)
				{
					count = players.Count;
					for (int i = 0; i < count; i++)
					{
						playerData = players[i];

						if (Players.ContainsKey(playerData.PlayerId))
						{
							if (playerData.PlayerId == LocalClient.Instance.MyId)
								continue;

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

				//projectiles
				projectiles = ClientSnapshot.Snapshots[0].projectiles;
				if (projectiles != null)
				{
					count = projectiles.Count;
					for (int i = 0; i < count; i++)
					{
						projData = projectiles[i];

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

					bufferTimeMultiplier = ClientSnapshot.Snapshots.Count > 1 ? 0.5f : 1f;

					playersOriginalPosAndRot.Clear();
					projectilesOriginalPositions.Clear();
				}
			}
		}

		public Vector3 GetLastPredictedPos()
		{
			return LocalPositionPredictions.Count < 1 ? Vector3.zero : LocalPositionPredictions[LocalPositionPredictions.Count - 1].Position;
		}

		public Vector3 GetPredictedPositionBySequence(ushort targetSequenceNumber)
		{
			//This is here for later optimization
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
			var playerComponent = player.GetComponent<PlayerObjectClient>();
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

			WeaponPickups[id].AddComponent<ItemHoverMovement>();
			WeaponPickups[id].SetActive(isActive);
		}
		public void WeaponUpdate(byte id, bool isActive)
		{
			WeaponPickups[id].SetActive(isActive);
		}

		public void SpawnHealthPack(byte id, Vector3 position, bool isActive, byte size)
		{
			if (!Healthpacks.ContainsKey(id))
				Healthpacks.Add(id, Instantiate(HealthpackPrefabs[size], position, Quaternion.identity, pickups));

			Healthpacks[id].SetActive(isActive);
		}
		public void HealthpackUpdate(byte id, bool isActive)
		{
			Healthpacks[id].SetActive(isActive);
		}

	}
}
