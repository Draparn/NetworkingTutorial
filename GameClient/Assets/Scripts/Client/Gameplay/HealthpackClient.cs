using UnityEngine;

namespace NetworkTutorial.Client.Gameplay
{
	public class HealthpackClient : MonoBehaviour
	{
		Vector3 startPos;

		private void Start()
		{
			startPos = transform.position;
		}

		private void Update()
		{
			transform.position = startPos + new Vector3(0, Mathf.Sin(Time.time) * 0.2f, 0);
			transform.parent.Rotate(0, Time.deltaTime * 100, 0);
		}

	}
}
