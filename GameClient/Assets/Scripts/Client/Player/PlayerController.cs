using NetworkTutorial.Client.Net;
using NetworkTutorial.Shared;
using UnityEngine;

namespace NetworkTutorial.Client.Player
{
	public class PlayerController : MonoBehaviour
	{
		public static PlayerController Instance;

		private Transform cameraTransform;
		private CharacterController controller;
		private PlayerManager playerManager;
		private InputsStruct inputs = new InputsStruct();

		private float yVelocity, yVelocityPreMove;
		private bool isGroundedPreMove;

		private uint frameNumber = 0;

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
			playerManager = GetComponent<PlayerManager>();
			controller = GetComponent<CharacterController>();
		}

		private void FixedUpdate()
		{
			ClientSend.SendPlayerInputs(frameNumber, inputs);
			PredictPlayerPosition();
			frameNumber++;
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
		}

		private void PredictPlayerPosition()
		{
			yVelocityPreMove = yVelocity;
			isGroundedPreMove = controller.isGrounded;

			controller.Move(PlayerMovementCalculations.CalculatePlayerPosition(inputs, gameObject.transform.right, gameObject.transform.forward, ref yVelocity, isGroundedPreMove));
			GameManager.Instance.LocalPositionPredictions.Add(new LocalPredictionData(
				frameNumber,
				inputs,
				gameObject.transform.position,
				gameObject.transform.right,
				gameObject.transform.forward,
				yVelocityPreMove,
				isGroundedPreMove)
				);
		}

		public void CorrectPredictedPosition(Vector3 correctPos)
		{
			controller.enabled = false;
			gameObject.transform.position = correctPos;
			controller.enabled = true;
		}

	}
}