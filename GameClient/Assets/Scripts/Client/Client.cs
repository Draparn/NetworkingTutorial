using System.Collections.Generic;
using UnityEngine;
using NetworkTutorial.Shared.Net;

namespace NetworkTutorial.Client
{
	public class Client : MonoBehaviour
	{
		public static Client Instance;

		protected delegate void PacketHandler(Packet packet);
		protected static Dictionary<int, PacketHandler> packethandlers;

		public TCP tcp;
		public UDP udp;

		public int MyId = 0;

		private bool isConnected = false;

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

		private void OnApplicationQuit()
		{
			Disconnect();
		}

		public void ConnectToServer(string ip)
		{
			tcp.InitializeClientData();

			isConnected = true;
			tcp.Connect(ip);
		}

		public void Disconnect()
		{
			if (isConnected)
			{
				isConnected = false;
				tcp.socket.Close();
				udp.socket.Close();

				Debug.Log($"Disconnected form server.");
			}
		}
	}
}