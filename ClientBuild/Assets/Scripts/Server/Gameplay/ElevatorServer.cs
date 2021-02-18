using System.Collections;
using UnityEngine;

public class ElevatorServer : MonoBehaviour
{
	private Vector3 startpoint;
	private Vector3 endpoint;

	private float lerpValue = 0;

	private bool isMoving;

	void Start()
	{
		startpoint = transform.position;
		endpoint = GetComponentInChildren<Transform>().position;
	}

	private void OnTriggerEnter(Collider other)
	{
		MoveElevator(other, true);
	}

	private void OnTriggerExit(Collider other)
	{
		MoveElevator(other, false);
	}

	private void MoveElevator(Collider other, bool enteredElevator)
	{
		if (!isMoving && other.CompareTag("Player"))
		{
			lerpValue = 0;
			isMoving = true;
			StartCoroutine(Move(enteredElevator));
		}
	}

	private IEnumerator Move(bool goingUp)
	{
		Vector3 sp = goingUp ? startpoint : endpoint, ep = goingUp ? endpoint : startpoint;
		do
		{
			lerpValue += Time.deltaTime;
			Vector3.Lerp(sp, ep, lerpValue);
			yield return new WaitForEndOfFrame();

			if (lerpValue >= 1)
				isMoving = false;
		} while (lerpValue < 1);
	}
}
