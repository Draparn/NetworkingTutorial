using UnityEngine;

namespace Assets.Scripts.Client
{
	public class Client : MonoBehaviour
	{
		public static Client Instance;

		public TCP tcp;

		public int MyId = 0;


		private void Awake()
		{
			if (Instance == null)
				Instance = this;
			else if (Instance != this)
				Destroy(this);
		}

		private void Start()
		{
			tcp = new TCP();
		}

		public void ConnectToServer()
		{
			tcp.InitializeClientData();
			tcp.Connect();
		}
	}
}
