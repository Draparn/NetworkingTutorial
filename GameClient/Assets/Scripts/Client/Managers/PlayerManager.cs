using UnityEngine;

namespace NetworkTutorial.Client
{
	public class PlayerManager : MonoBehaviour
	{
		public string PlayerName;
		public int PlayerId;


		private void Awake()
		{
			var menuCamera = GameObject.FindGameObjectWithTag("MainCamera");
			if (menuCamera != null)
				Destroy(menuCamera);
		}

	}
}
