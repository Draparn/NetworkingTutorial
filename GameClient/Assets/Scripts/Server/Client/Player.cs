using NetworkTutorial.Server.Managers;
using NetworkTutorial.Server.Net;
using NetworkTutorial.Shared;
using UnityEngine;

namespace NetworkTutorial.Server.Client
{
	public class Player : MonoBehaviour
	{
		private Vector2 inputDirection;

		private Vector3[] respawnPoints = new Vector3[3]
		{
			new Vector3(0, 0.75f, 0),
			new Vector3(0, 2.5f, -18),
			new Vector3(-5, 0.75f, 20)
		};

		public Transform ShootOrigin;
		private CharacterController controller;

		public string PlayerName;

		public float CurrentHealth = 100.0f;
		public float MaxHealth = 100.0f;
		private float PrimaryFireDamage = 10.0f;
		private float ThrowForce = 600.0f;
		private float yVelocity = 0;

		private bool hitScan = false;

		public int PlayerId;
		public uint FrameNumber;

		private bool[] playerInput;

		private void Start()
		{
			controller = gameObject.GetComponent<CharacterController>();
		}

		public void FixedUpdate()
		{
			if (CurrentHealth <= 0)
				return;

			MovePlayer();
		}

		public void Init(int id, string name)
		{
			PlayerName = name;
			PlayerId = id;
			CurrentHealth = MaxHealth;

			playerInput = new bool[5];
		}

		private void MovePlayer()
		{
			controller.Move(PlayerMovementCalculations.CalculatePlayerPosition(playerInput, transform, ref yVelocity, controller.isGrounded));

			ServerSnapshot.AddPlayerMovement(PlayerId, transform.position, FrameNumber);
			ServerSend.SendPlayerRotationUpdate_UDP_ALLEXCEPT(this);
		}

		public void PrimaryFire(Vector3 viewDirection)
		{
			if (CurrentHealth <= 0)
				return;

			if (hitScan)
			{
				if (Physics.Raycast(ShootOrigin.position, viewDirection, out RaycastHit hit))
				{
					if (hit.collider.CompareTag("Player"))
					{
						hit.collider.GetComponent<Player>().TakeDamage(PrimaryFireDamage);
					}
				}
			}
			else
			{
				NetworkManager.instance.InstantiateProjectile(ShootOrigin).Init(viewDirection, ThrowForce, PlayerId);
			}
		}

		public void TakeDamage(float damage)
		{
			if (CurrentHealth <= 0)
				return;

			CurrentHealth -= damage;

			if (CurrentHealth <= 0)
				PlayerDied();

			ServerSend.SendPlayerHealthUpdate_TCP_ALL(this);
		}
		public void HealDamage(float healing)
		{
			CurrentHealth = CurrentHealth + healing > MaxHealth ? MaxHealth : CurrentHealth + healing;
		}

		private void PlayerDied()
		{
			CurrentHealth = 0;
			controller.enabled = false;

			Invoke(nameof(PlayerRespawn), 3);
		}

		private void PlayerRespawn()
		{
			transform.position = respawnPoints[Random.Range(0, 3)];
			CurrentHealth = MaxHealth;
			controller.enabled = true;

			ServerSend.SendPlayerRespawned_TCP_ALL(this);
		}

		public void UpdatePosAndRot(uint frameNumber, bool[] inputs, Quaternion rot)
		{
			FrameNumber = frameNumber;
			playerInput = inputs;
			transform.rotation = rot;
		}

	}
}
