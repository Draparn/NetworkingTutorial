using NetworkTutorial.Server.Net;
using NetworkTutorial.Shared;
using UnityEngine;

namespace NetworkTutorial.Server.Client
{
	public class Player : MonoBehaviour
	{
		private Vector2 InputDirection;

		private CharacterController controller;

		public string PlayerName;

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
			InputDirection = Vector2.zero;
			if (playerInput[0])			//W
				InputDirection.y += 1;
			if (playerInput[1])			//S
				InputDirection.y -= 1;
			if (playerInput[2])			//A
				InputDirection.x -= 1;
			if (playerInput[3])			//D
				InputDirection.x += 1;
			
			MovePlayer(InputDirection);
		}

		public void Init(int id, string name)
		{
			PlayerName = name;
			PlayerId = id;
			playerInput = new bool[5];
		}

		private void MovePlayer(Vector2 inputDirection)
		{
			var moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.y;
			moveDirection *= moveSpeed;

			if (controller.isGrounded)
			{
				yVelocity = 0;

				if (playerInput[4])	//Spacebar
					yVelocity = jumpSpeed;
			}
			else
				yVelocity += gravity;

			moveDirection.y = yVelocity;
			controller.Move(moveDirection);

			ServerSend.SendUpdatePlayerPosition(this);
			ServerSend.SendUpdatePlayerRotation(this);
		}

		public void UpdatePosAndRot(bool[] inputs, Quaternion rot)
		{
			playerInput = inputs;
			transform.rotation = rot;
		}

	}
}
