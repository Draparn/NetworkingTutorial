using System.Collections;
using UnityEngine;
using SmallMultiplayerGame.Server.Net;

namespace SmallMultiplayerGame.Server.Gameplay
{
	public class ElevatorServer : MonoBehaviour
	{
		private Vector3 startpoint;
		[SerializeField] private Transform endpoint;

		private float lerpValue = 0;

		private bool isMoving;

		void Start()
		{
			startpoint = transform.localPosition;
		}

		public void OnTriggerEnter(Collider other)
		{
			MoveElevator(other, true);
		}

		public void OnTriggerExit(Collider other)
		{
			MoveElevator(other, false);
		}

		private void MoveElevator(Collider other, bool enteredElevator)
		{
			if (!isMoving && other.CompareTag("Player"))
			{
				isMoving = true;
				StartCoroutine(ServerElevatorMove(enteredElevator));
			}
		}

		private IEnumerator ServerElevatorMove(bool goingUp)
		{
			do
			{
				lerpValue = goingUp ? lerpValue + Time.deltaTime : lerpValue - Time.deltaTime;
				transform.localPosition = Vector3.Lerp(startpoint, endpoint.localPosition, lerpValue);
				ServerSnapshot.AddElevatorMovement(lerpValue);

				yield return new WaitForSeconds(Time.deltaTime);

				if (lerpValue <= 0 || lerpValue >= 1)
					isMoving = false;
			} while (isMoving);
		}

		public void ClientElevatorMove(float lerpValue)
		{
			transform.localPosition = Vector3.Lerp(startpoint, endpoint.localPosition, lerpValue);
		}

	}

}
