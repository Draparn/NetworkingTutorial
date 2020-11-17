using System.Net;
using UnityEngine;

namespace Assets.Scripts.Client
{
	public class ClientHandle
	{
		public static void Welcome(Packet packet)
		{
			var message = packet.ReadString();
			var id = packet.ReadInt();

			Debug.Log($"Message from server: {message}");
			Client.Instance.MyId = id;
			ClientSend.WelcomeReceived();

			Client.Instance.udp.Connect(((IPEndPoint)Client.Instance.tcp.socket.Client.LocalEndPoint).Port);
		}

		public static void UDPTest(Packet packet)
		{
			string msg = packet.ReadString();
			
			Debug.Log($"Received packed via UDP from server. Contains message: {msg}");
			ClientSend.UDPTestReceived();
		}
	}
}