using System.Net;
using NetworkTutorial.Shared.Net;
using UnityEngine;

namespace NetworkTutorial.Client
{
	public class ClientHandle : MonoBehaviour
	{
		public static void OnWelcome(Packet packet)
		{
			var message = packet.ReadString();
			var id = packet.ReadInt();

			Debug.Log($"Message from server: {message}");
			Client.Instance.MyId = id;
			ClientSend.SendWelcomeReceived();

			Client.Instance.udp.Connect(((IPEndPoint)Client.Instance.tcp.socket.Client.LocalEndPoint).Port);
		}

		public static void OnPlayerConnected(Packet packet)
		{
			var id = packet.ReadInt();
			var playerName = packet.ReadString();
			var position = packet.ReadVector3();
			var rotation = packet.ReadQuaternion();

			GameManager.Instance.SpawnPlayer(id == Client.Instance.MyId, id, playerName, position, rotation);
		}
		public static void OnPlayerDisconnected(Packet packet)
		{
			var clientId = packet.ReadInt();

			Destroy(GameManager.Instance.Players[clientId].gameObject);
			GameManager.Instance.Players.Remove(clientId);
		}

		public static void OnUpdatePlayerPosition(Packet packet)
		{
			var id = packet.ReadInt();
			var position = packet.ReadVector3();

			GameManager.Instance.Players[id].transform.position = position;
		}
		public static void OnUpdatePlayerRotation(Packet packet)
		{
			var id = packet.ReadInt();
			var rotation = packet.ReadQuaternion();

			GameManager.Instance.Players[id].transform.rotation = rotation;
		}

		public static void OnPlayerHealth(Packet packet)
		{
			var clientId = packet.ReadInt();
			var currentHealth = packet.ReadFloat();

			GameManager.Instance.Players[clientId].SetHealth(currentHealth);
		}
		public static void OnPlayerRespawn(Packet packet)
		{
			var id = packet.ReadInt();
			var pos = packet.ReadVector3();

			GameManager.Instance.Players[id].Respawn(pos);
		}

	}
}