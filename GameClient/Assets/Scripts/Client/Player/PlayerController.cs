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
		//private Vector2 inputDirection = new Vector2();

		private float yVelocity = 0;

		private uint frameNumber = 0;

		private bool forward, back, left, right, jump;
		private bool[] inputs = new bool[5];

		private void Start()
		{
			cameraTransform = GetComponentInChildren<CameraController>().transform;
			playerManager = GetComponent<PlayerManager>();
			controller = GetComponent<CharacterController>();
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

			inputs[0] = forward;
			inputs[1] = back;
			inputs[2] = left;
			inputs[3] = right;
			inputs[4] = jump;

			ClientSend.SendPlayerInputs(frameNumber, inputs);
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
			var yVelocityPreMove = yVelocity;
			var isGroundedPreMove = controller.isGrounded;

			controller.Move(PlayerMovementCalculations.CalculatePlayerPosition(inputs, transform, ref yVelocity, controller.isGrounded));

			GameManager.Instance.LocalPositionPredictions.Add(new LocalPredictionData(frameNumber, inputs, yVelocityPreMove, isGroundedPreMove, transform));
		}

	}
}