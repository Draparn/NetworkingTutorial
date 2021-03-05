using SmallMultiplayerGame.Server.Net;
using System.Collections;
using UnityEngine;

namespace SmallMultiplayerGame.Server.Gameplay.Environment
{
	public class Elevator : MonoBehaviour
	{
		private Vector3 startpoint;
		[SerializeField] private Transform endpoint;

		private float lerpValue = 0, yMidpoint;
		private bool isMoving;

		void Start()
		{
			startpoint = transform.localPosition;
			yMidpoint = (endpoint.localPosition.y - startpoint.y) / 2;
		}

		public void OnTriggerEnter(Collider other)
		{
			if (!isMoving && other.CompareTag("Player") && lerpValue <= 0)
			{
				isMoving = true;
				StartCoroutine(ServerElevatorMove(true));
				Invoke(nameof(GoDown), 3);
			}
		}

		private void GoDown()
		{
			isMoving = true;
			StartCoroutine(ServerElevatorMove(false));
		}

		private IEnumerator ServerElevatorMove(bool goingUp)
		{
			do
			{
				lerpValue = goingUp ? lerpValue + Time.deltaTime : lerpValue - Time.deltaTime;
				transform.localPosition = Vector3.Lerp(startpoint, endpoint.localPosition, lerpValue);
				ServerSnapshot.AddElevatorMovement(lerpValue);

				yield return new WaitForSeconds(Time.deltaTime);

				if (lerpValue < 0 || lerpValue > 1)
				{
					lerpValue = lerpValue <= 0 ? 0 : 1;
					isMoving = false;
				}
			} while (isMoving);
		}

		public void ClientElevatorMove(float? lerpValue)
		{
			if (lerpValue == null)
				lerpValue = transform.localPosition.y < yMidpoint ? 0 : 1;

			transform.localPosition = Vector3.Lerp(startpoint, endpoint.localPosition, (float)lerpValue);
		}

	}

}
