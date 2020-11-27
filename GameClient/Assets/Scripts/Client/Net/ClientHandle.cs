using NetworkTutorial.Shared.Net;
using System.Net;
using UnityEngine;

namespace NetworkTutorial.Client
{
	public class ClientHandle : MonoBehaviour
	{
		public static void OnWelcomeMessage(Packet packet)
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

		public static void OnPlayerPositionUpdate(Packet packet)
		{
			var id = packet.ReadInt();
			var position = packet.ReadVector3();

			if (GameManager.Instance.Players.ContainsKey(id))
				GameManager.Instance.Players[id].transform.position = position;
		}
		public static void OnPlayerRotationUpdate(Packet packet)
		{
			var id = packet.ReadInt();
			var rotation = packet.ReadQuaternion();

			GameManager.Instance.Players[id].transform.rotation = rotation;
		}

		public static void OnPlayerHealthUpdate(Packet packet)
		{
			var clientId = packet.ReadInt();
			var newHealth = packet.ReadFloat();
			GameManager.Instance.Players[clientId].SetHealth(clientId, newHealth);

			if (clientId == Client.Instance.MyId)
				GameManager.Instance.Players[clientId].FlashUI();
		}
		public static void OnPlayerRespawn(Packet packet)
		{
			var id = packet.ReadInt();
			var pos = packet.ReadVector3();

			GameManager.Instance.Players[id].Respawn(pos);
		}

		public static void OnProjectileSpawn(Packet packet)
		{
			var id = packet.ReadUShort();
			var pos = packet.ReadVector3();

			GameManager.Instance.SpawnProjectile(id, pos);
		}
		public static void OnProjectiePositionUpdate(Packet packet)
		{
			var id = packet.ReadUShort();
			var pos = packet.ReadVector3();

			if (GameManager.Instance.Projectiles.ContainsKey(id))
				GameManager.Instance.Projectiles[id].transform.position = pos;
		}
		public static void OnProjectieExplosion(Packet packet)
		{
			var id = packet.ReadUShort();
			var pos = packet.ReadVector3();

			GameManager.Instance.Projectiles[id].Explode(pos);
		}

	}
}