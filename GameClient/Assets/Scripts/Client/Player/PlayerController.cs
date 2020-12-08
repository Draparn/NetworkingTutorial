using NetworkTutorial.Client.Net;
using UnityEngine;

namespace NetworkTutorial.Client.Player
{
	public class PlayerController : MonoBehaviour
	{
		private Transform cameraTransform;

		bool[] inputs = new bool[5];

		private void Start()
		{
			cameraTransform = GetComponentInChildren<CameraController>().transform;
		}

		private void FixedUpdate()
		{
			SendInputToServer();
			UpdatePlayerPosition();
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Mouse0))
				ClientSend.SendPlayerPrimaryFire(cameraTransform.forward);
		}

		public void SendInputToServer()
		{
			inputs[0] = Input.GetKey(KeyCode.W);
			inputs[1] = Input.GetKey(KeyCode.S);
			inputs[2] = Input.GetKey(KeyCode.A);
			inputs[3] = Input.GetKey(KeyCode.D);
			inputs[4] = Input.GetKey(KeyCode.Space);

			ClientSend.SendPlayerInputs((uint)Time.frameCount, inputs);
		}

		private void UpdatePlayerPosition()
		{
			//TODO: Local player position update here
		}
	}
}