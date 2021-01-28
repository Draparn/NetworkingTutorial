using NetworkTutorial.Server.Gameplay;
using NetworkTutorial.Server.Net;
using NetworkTutorial.Shared;
using NetworkTutorial.Shared.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Server.Client
{
	public class PlayerServer : MonoBehaviour
	{
		private Dictionary<byte, Vector3> currentPositions;
		private List<Weapon> pickedUpWeapons;
		public Transform ShootOrigin;
		private CharacterController controller;
		private ServerSnapshot oldSnapshot;
		private ClientServer client;

		private InputsStruct playerInput;
		private Quaternion prevRot;
		private Vector2 inputDirection;
		private Vector3 prevPos;

		public string PlayerName;
		public float CurrentHealth = 100.0f, MaxHealth = 100.0f;
		private float yVelocity;
		public byte PlayerId, currentWeaponSlot;
		public ushort SequenceNumber = ushort.MaxValue;


		private void Start()
		{
			controller = gameObject.GetComponent<CharacterController>();

			pickedUpWeapons = Weapons.GetNewWeapons();
			currentWeaponSlot = 1;
		}

		public void Init(byte id, string name)
		{
			PlayerName = name;
			PlayerId = id;
			CurrentHealth = MaxHealth;

			playerInput = new InputsStruct();
		}

		public void PrimaryFire(Vector3 viewDirection, uint sequenceNumber)
		{
			if (CurrentHealth <= 0)
				return;

			if (pickedUpWeapons[currentWeaponSlot].ProjectileType == ProjectileType.Hitscan)
			{
				oldSnapshot = ServerSnapshot.GetOldSnapshot(sequenceNumber);
				currentPositions = new Dictionary<byte, Vector3>();

				RewindPlayerPositions();
				if (Physics.Raycast(ShootOrigin.position, viewDirection, out RaycastHit hit))
				{
					if (hit.collider.CompareTag("Player"))
						hit.collider.GetComponent<PlayerServer>().TakeDamage(pickedUpWeapons[currentWeaponSlot].Damage);
				}
				RestorePlayerPositions();
			}
			else if (pickedUpWeapons[currentWeaponSlot].ProjectileType == ProjectileType.Grenade || pickedUpWeapons[currentWeaponSlot].ProjectileType == ProjectileType.Rocket)
			{
				GameManagerServer.Instance.InstantiateProjectile(ShootOrigin, viewDirection, pickedUpWeapons[currentWeaponSlot].ProjectilePrefabServer)
					.Init(viewDirection, pickedUpWeapons[currentWeaponSlot], PlayerId);
			}
		}

		public void WeaponSwitch(byte weaponSlot)
		{
			if (weaponSlot < pickedUpWeapons.Count && pickedUpWeapons[weaponSlot].IsPickedUp)
			{
				currentWeaponSlot = weaponSlot;
				ServerSend.SendPlayerSwitchedWeapon_ALL(this);
			}
		}

		public void TakeDamage(float damage)
		{
			if (CurrentHealth <= 0)
				return;

			CurrentHealth -= damage;

			if (CurrentHealth <= 0)
				PlayerDied();

			ServerSend.SendPlayerHealthUpdate_ALL(this);
		}

		public void HealDamage(float healing)
		{
			CurrentHealth = CurrentHealth + healing > MaxHealth ? MaxHealth : CurrentHealth + healing;
		}

		private void PlayerDied()
		{
			CurrentHealth = 0;
			controller.enabled = false;

			Invoke(nameof(PlayerRespawn), ConstantValues.PLAYER_RESPAWN_TIME);
		}

		private void PlayerRespawn()
		{
			transform.position = GameManagerServer.Instance.respawnPoints[Random.Range(0, GameManagerServer.Instance.respawnPoints.Count)];
			CurrentHealth = MaxHealth;
			controller.enabled = true;

			pickedUpWeapons = Weapons.GetNewWeapons();
			currentWeaponSlot = 1;

			ServerSend.SendPlayerRespawned_ALL(this);
		}

		public void UpdatePosAndRot(ushort sequenceNumber, InputsStruct inputs, Quaternion rot)
		{
			if (!IsMoreRecent(sequenceNumber) && controller.enabled == true)
				return;

			prevRot = transform.rotation;

			SequenceNumber = sequenceNumber;
			playerInput = inputs;
			transform.rotation = rot;

			prevPos = transform.position;
			controller.Move(PlayerMovementCalculations.CalculatePlayerPosition(playerInput, transform, ref yVelocity, controller.isGrounded));

			ServerSnapshot.AddPlayerMovement(PlayerId, transform.position, transform.rotation, SequenceNumber);
		}

		private bool IsMoreRecent(ushort newSequenceNumber)
		{
			return ((newSequenceNumber > SequenceNumber) && (newSequenceNumber - SequenceNumber <= 32768)) ||
				((newSequenceNumber < SequenceNumber) && (SequenceNumber - newSequenceNumber > 32768));
		}

		private void RewindPlayerPositions()
		{
			for (byte i = 1; i < Server.Clients.Count; i++)
			{
				client = Server.Clients[i];
				if (client.Connection.endPoint != null && oldSnapshot.PlayerPositions.ContainsKey(client.PlayerObject.PlayerId))
				{
					currentPositions.Add(client.Id, client.PlayerObject.transform.position);
					client.PlayerObject.transform.position = oldSnapshot.PlayerPositions[client.Id].Position;
				}
			}
		}

		private void RestorePlayerPositions()
		{
			foreach (var kvp in currentPositions)
			{
				if (Server.Clients[kvp.Key].PlayerObject.CurrentHealth > 0)
					Server.Clients[kvp.Key].PlayerObject.transform.position = kvp.Value;
			}
		}
	}
}