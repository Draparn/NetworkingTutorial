using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Client
{
	public class Client : MonoBehaviour
	{
		public static Client Instance;

		protected delegate void PacketHandler(Packet packet);
		protected static Dictionary<int, PacketHandler> packethandlers;

		public TCP tcp;
		public UDP udp;

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
			udp = new UDP();
		}

		public void ConnectToServer(string ip)
		{
			tcp.InitializeClientData();
			tcp.Connect(ip);
		}
	}
}
