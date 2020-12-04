using NetworkTutorial.Client.Net;
using UnityEngine;

namespace NetworkTutorial.Client.Player
{
	public class PlayerController : MonoBehaviour
	{
		private Transform cameraTransform;

		private void Start()
		{
			cameraTransform = GetComponentInChildren<CameraController>().transform;
		}

		private void FixedUpdate()
		{
			SendInputToServer();
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Mouse0))
				ClientSend.SendPlayerPrimaryFire(cameraTransform.forward);
		}

		public void SendInputToServer()
		{
			bool[] inputs = new bool[]
			{
				Input.GetKey(KeyCode.W),
				Input.GetKey(KeyCode.S),
				Input.GetKey(KeyCode.A),
				Input.GetKey(KeyCode.D),
				Input.GetKey(KeyCode.Space)
			};

			ClientSend.SendPlayerInputs(inputs);
		}
	}
}