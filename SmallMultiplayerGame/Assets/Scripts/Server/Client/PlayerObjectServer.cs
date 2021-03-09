using SmallMultiplayerGame.Server.Gameplay;
using SmallMultiplayerGame.Server.Net;
using SmallMultiplayerGame.Shared;
using SmallMultiplayerGame.Shared.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace SmallMultiplayerGame.Server.Client
{
	public class PlayerObjectServer : MonoBehaviour
	{
		private Dictionary<byte, Vector3> currentPositions;
		public List<Weapon> pickedUpWeapons;
		public Transform ShootOrigin;
		private CharacterController controller;
		private ServerSnapshot oldSnapshot;
		private Client client;

		private RaycastHit[] hits = new RaycastHit[7];
		private InputsStruct playerInput;
		private Quaternion prevRot;
		private Vector2 inputDirection;
		private Vector3 prevPos, currentVelocity, previousVelocity;

		public string PlayerName;
		public float CurrentHealth = 100.0f, MaxHealth = 100.0f;
		private float yVelocity, distance;
		private uint SequenceNumber = uint.MaxValue;
		public byte PlayerId, currentWeaponSlot;
		private byte hitCount, index;

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

			if (pickedUpWeapons[currentWeaponSlot].Ammo <= 0)
			{
				WeaponSwitch((byte)WeaponSlot.Pistol);
				return;
			}

			if (pickedUpWeapons[currentWeaponSlot].ProjectileType == ProjectileType.Hitscan)
			{
				ServerSend.SendPlayerFiredWeapon_ALL(PlayerId);

				oldSnapshot = ServerSnapshot.GetOldSnapshot(sequenceNumber);
				currentPositions = new Dictionary<byte, Vector3>();

				RewindPlayerPositions();

				hitCount = (byte)Physics.RaycastNonAlloc(ShootOrigin.position, viewDirection, hits);
				index = 0;
				distance = float.MaxValue;

				for (int i = 0; i < hitCount; i++)
				{
					if (hits[i].transform == transform)
						continue;

					if (hits[i].distance < distance)
					{
						distance = hits[i].distance;
						index = (byte)i;
					}
				}

				if (hits[index].collider.CompareTag("Player"))
					hits[index].collider.GetComponent<PlayerObjectServer>().TakeDamage(pickedUpWeapons[currentWeaponSlot].Damage);

				RestorePlayerPositions();
			}
			else if (pickedUpWeapons[currentWeaponSlot].ProjectileType == ProjectileType.Grenade || pickedUpWeapons[currentWeaponSlot].ProjectileType == ProjectileType.Rocket)
			{
				GameManagerServer.Instance.InstantiateProjectile(ShootOrigin, viewDirection, pickedUpWeapons[currentWeaponSlot].ProjectilePrefabServer)
					.Init(viewDirection, pickedUpWeapons[currentWeaponSlot], PlayerId);
			}

			if (currentWeaponSlot != (byte)WeaponSlot.Pistol)
			{
				pickedUpWeapons[currentWeaponSlot].Ammo--;
				ServerSend.SendWeaponAmmoUpdate_CLIENT(PlayerId, pickedUpWeapons[currentWeaponSlot].Ammo);
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

		public void UpdatePosAndRot(uint sequenceNumber, InputsStruct inputs, Quaternion rot)
		{
			if (!IsMoreRecent(sequenceNumber) && controller.enabled)
				return;

			prevRot = transform.rotation;
			SequenceNumber = sequenceNumber;
			playerInput = inputs;
			transform.rotation = rot;
			prevPos = transform.position;

			currentVelocity = PlayerMovementCalculations.CalculateCurrentVelocity(inputs, transform, ref yVelocity, controller.isGrounded);
			if (currentVelocity.x == 0 && currentVelocity.z == 0)
			{
				PlayerMovementCalculations.CalculatePreviousVelocity(yVelocity, ref previousVelocity);
				currentVelocity = previousVelocity;
			}
			else
				previousVelocity = currentVelocity;

			if (controller.Move(currentVelocity) == CollisionFlags.Above)
				yVelocity = 0;

			ServerSnapshot.AddPlayerMovement(PlayerId, transform.position, transform.rotation, SequenceNumber);
		}

		private bool IsMoreRecent(uint newSequenceNumber)
		{
			return ((newSequenceNumber > SequenceNumber) && (newSequenceNumber - SequenceNumber <= 32768)) ||
				((newSequenceNumber < SequenceNumber) && (SequenceNumber - newSequenceNumber > 32768));
		}

		private void RewindPlayerPositions()
		{
			for (byte i = 1; i < Server.Clients.Count; i++)
			{
				client = Server.Clients[i];
				if (client.Connection.endPoint != null && oldSnapshot.PlayerPositions.ContainsKey(client.Player.PlayerId))
				{
					currentPositions.Add(client.Id, client.Player.transform.position);
					client.Player.transform.position = oldSnapshot.PlayerPositions[client.Id].Position;
				}
			}
		}

		private void RestorePlayerPositions()
		{
			foreach (var kvp in currentPositions)
			{
				if (Server.Clients[kvp.Key].Player.CurrentHealth > 0)
					Server.Clients[kvp.Key].Player.transform.position = kvp.Value;
			}
		}
	}
}