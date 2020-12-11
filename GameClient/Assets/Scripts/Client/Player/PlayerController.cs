using NetworkTutorial.Client.Net;
using NetworkTutorial.Shared;
using UnityEngine;

namespace NetworkTutorial.Client.Player
{
	public class PlayerController : MonoBehaviour
	{
		private Transform cameraTransform;
		private CharacterController controller;
		private PlayerManager playerManager;

		public static Vector3 correctPos = Vector3.zero;
		private Vector2 inputDirection = new Vector2();

		private float moveSpeed, gravity, jumpSpeed, yVelocity;

		private uint frameNumber = 0;

		private bool forward, back, left, right, jump;

		private void Start()
		{
			cameraTransform = GetComponentInChildren<CameraController>().transform;
			playerManager = GetComponent<PlayerManager>();
			controller = GetComponent<CharacterController>();
			gravity = ConstantValues.WORLD_GRAVITY * Time.fixedDeltaTime * Time.fixedDeltaTime;
			moveSpeed = ConstantValues.PLAYER_MOVE_SPEED * Time.fixedDeltaTime;
			jumpSpeed = ConstantValues.PLAYER_JUMP_SPEED * Time.fixedDeltaTime;
			yVelocity = 0;
		}

		private void FixedUpdate()
		{
			if (playerManager.currentHealth <= 0)
				return;
			else if (correctPos != Vector3.zero)
			{
				gameObject.transform.position = correctPos;
				GameManager.Instance.LocalPositionPredictions.Clear();
				correctPos = Vector3.zero;
				return;
			}

			ClientSend.SendPlayerInputs(frameNumber, new bool[] { forward, back, left, right, jump });
			UpdatePlayerPosition();
			frameNumber++;
		}

		private void Update()
		{
			forward = Input.GetKey(KeyCode.W);
			back = Input.GetKey(KeyCode.S);
			left = Input.GetKey(KeyCode.A);
			right = Input.GetKey(KeyCode.D);
			jump = Input.GetKey(KeyCode.Space);

			if (Input.GetKeyDown(KeyCode.Mouse0))
				ClientSend.SendPlayerPrimaryFire(cameraTransform.forward);
		}

		private void UpdatePlayerPosition()
		{
			inputDirection = Vector2.zero;
			if (forward)  //W
				inputDirection.y += 1;
			if (back)  //S
				inputDirection.y -= 1;
			if (left)  //A
				inputDirection.x -= 1;
			if (right)  //D
				inputDirection.x += 1;

			var moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.y;
			moveDirection *= moveSpeed;

			if (controller.isGrounded)
			{
				yVelocity = 0;

				if (jump) //Spacebar
					yVelocity = jumpSpeed;
			}
			else
				yVelocity += gravity;

			moveDirection.y = yVelocity;
			controller.Move(moveDirection);

			GameManager.Instance.LocalPositionPredictions.Add(new LocalPosPrediction(frameNumber, gameObject.transform.position));
		}

	}
}