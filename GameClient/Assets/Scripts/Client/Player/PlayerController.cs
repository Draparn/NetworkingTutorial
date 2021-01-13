using NetworkTutorial.Client.Gameplay;
using NetworkTutorial.Client.Net;
using NetworkTutorial.Shared;
using NetworkTutorial.Shared.Utils;
using UnityEngine;

namespace NetworkTutorial.Client.Player
{
	public class PlayerController : MonoBehaviour
	{
		public static PlayerController Instance;

		private Transform cameraTransform;
		private CharacterController controller;
		private PlayerClient playerManager;
		private InputsStruct inputs = new InputsStruct();
		private Vector3 prevPos, nextPos;

		private float yVelocity, yVelocityPreMove, clientTickRate;
		private bool isGroundedPreMove;

		private ushort sequenceNumber = 0;

		private void Awake()
		{
			if (Instance == null)
				Instance = this;
			else if (Instance != this)
				Destroy(this);
		}

		private void Start()
		{
			cameraTransform = GetComponentInChildren<CameraController>().transform;
			playerManager = GetComponent<PlayerClient>();
			controller = GetComponent<CharacterController>();

			prevPos = gameObject.transform.position;
			nextPos = gameObject.transform.position;
		}

		private void Update()
		{
			if (playerManager.currentHealth <= 0)
				return;

			if (Input.GetKeyDown(KeyCode.Mouse0))
				ClientSend.SendPlayerPrimaryFire(cameraTransform.forward);

			inputs.Forward = Input.GetKey(KeyCode.W);
			inputs.Back = Input.GetKey(KeyCode.S);
			inputs.Left = Input.GetKey(KeyCode.A);
			inputs.Right = Input.GetKey(KeyCode.D);
			inputs.Jump = Input.GetKey(KeyCode.Space);

			clientTickRate += Time.deltaTime;

			gameObject.transform.position = Vector3.Lerp(
				prevPos,
				nextPos,
				clientTickRate / ConstantValues.SERVER_TICK_RATE
				);

			if (clientTickRate >= ConstantValues.SERVER_TICK_RATE)
			{
				SendAndPredict();
				clientTickRate = 0;
				sequenceNumber++;
			}
		}

		private void SendAndPredict()
		{
			ClientSend.SendPlayerInputs(sequenceNumber, inputs, transform.rotation);
			PredictPlayerPosition();
		}

		private void PredictPlayerPosition()
		{
			prevPos = gameObject.transform.position;
			gameObject.transform.position = GameManagerClient.Instance.GetLastPredictedPos();
			if (gameObject.transform.position == Vector3.zero)
				gameObject.transform.position = prevPos;

			yVelocityPreMove = yVelocity;
			isGroundedPreMove = controller.isGrounded;

			controller.enabled = true;
			controller.Move(PlayerMovementCalculations.CalculatePlayerPosition(inputs, transform, ref yVelocity, controller.isGrounded));
			controller.enabled = false;
			GameManagerClient.Instance.LocalPositionPredictions.Add(new LocalPredictionData(
				sequenceNumber,
				inputs,
				gameObject.transform.position,
				transform,
				yVelocityPreMove,
				isGroundedPreMove
				));

			nextPos = gameObject.transform.position;
			gameObject.transform.position = prevPos;
		}

	}
}