using NetworkTutorial.Server.Net;
using UnityEngine;

namespace NetworkTutorial.Server.Client
{
	public class Player : MonoBehaviour
	{
		private Vector2 InputDirection;

		public Transform ShootOrigin;
		private CharacterController controller;

		public string PlayerName;

		public float CurrentHealth = 100;
		public float maxHealth = 100;
		public float PrimaryFireDamage = 10f;
		private float moveSpeed = 5.0f;
		private float jumpSpeed = 9.0f;
		private float gravity = -19.62f;
		private float yVelocity = 0;

		public int PlayerId;

		private bool[] playerInput;

		private void Start()
		{
			controller = gameObject.GetComponent<CharacterController>();
			gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
			moveSpeed *= Time.fixedDeltaTime;
			jumpSpeed *= Time.fixedDeltaTime;
		}

		public void FixedUpdate()
		{
			if (CurrentHealth <= 0)
				return;

			InputDirection = Vector2.zero;
			if (playerInput[0])         //W
				InputDirection.y += 1;
			if (playerInput[1])         //S
				InputDirection.y -= 1;
			if (playerInput[2])         //A
				InputDirection.x -= 1;
			if (playerInput[3])         //D
				InputDirection.x += 1;

			MovePlayer(InputDirection);
		}

		public void Init(int id, string name)
		{
			PlayerName = name;
			PlayerId = id;
			CurrentHealth = maxHealth;

			playerInput = new bool[5];
		}

		private void MovePlayer(Vector2 inputDirection)
		{
			var moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.y;
			moveDirection *= moveSpeed;

			if (controller.isGrounded)
			{
				yVelocity = 0;

				if (playerInput[4]) //Spacebar
					yVelocity = jumpSpeed;
			}
			else
				yVelocity += gravity;

			moveDirection.y = yVelocity;
			controller.Move(moveDirection);

			ServerSend.SendUpdatePlayerPosition_UDP(this);
			ServerSend.SendUpdatePlayerRotation_UDP(this);
		}

		public void PrimaryFire(Vector3 viewDirection)
		{
			if (Physics.Raycast(ShootOrigin.position, viewDirection, out RaycastHit hit))
			{
				if (hit.collider.CompareTag("Player"))
				{
					hit.collider.GetComponent<Player>().TakeDamage(PrimaryFireDamage);
				}
			}
		}

		private void TakeDamage(float damage)
		{
			if (CurrentHealth <= 0)
				return;

			CurrentHealth -= damage;

			if (CurrentHealth <= 0)
				PlayerDied();

			ServerSend.SendUpdatePlayerHealth_TCP(this);
		}

		private void PlayerDied()
		{
			CurrentHealth = 0;
			controller.enabled = false;

			Invoke(nameof(PlayerRespawn), 3);
		}

		private void PlayerRespawn()
		{
			transform.position = new Vector3(0, 0.5f, 0);
			CurrentHealth = maxHealth;
			controller.enabled = true;

			ServerSend.SendPlayerRespawned_TCP(this);
		}

		public void UpdatePosAndRot(bool[] inputs, Quaternion rot)
		{
			playerInput = inputs;
			transform.rotation = rot;
		}

	}
}
