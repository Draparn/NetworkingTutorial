using System.Collections;
using UnityEngine;

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

	private void OnTriggerEnter(Collider other)
	{
		other.gameObject.transform.SetParent(transform);
		MoveElevator(other, true);
	}

	private void OnTriggerExit(Collider other)
	{
		other.gameObject.transform.SetParent(null);
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
		Vector3 sp = goingUp ? startpoint : endpoint.localPosition, ep = goingUp ? endpoint.localPosition: startpoint;
		do
		{
			lerpValue += Time.deltaTime;
			transform.localPosition = Vector3.Lerp(sp, ep, lerpValue);
			yield return new WaitForSeconds(Time.deltaTime);

			if (lerpValue >= 1)
				isMoving = false;
		} while (lerpValue < 1);
	}
}
