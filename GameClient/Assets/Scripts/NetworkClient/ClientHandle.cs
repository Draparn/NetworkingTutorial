using System.Net;
using UnityEngine;

public class ClientHandle
{
	public static void Welcome(Packet packet)
	{
		var message = packet.ReadString();
		var id = packet.ReadInt();

		Debug.Log($"Message from server: {message}");
		Client.Instance.MyId = id;
		ClientSend.SendWelcomeReceived();

		Client.Instance.udp.Connect(((IPEndPoint)Client.Instance.tcp.socket.Client.LocalEndPoint).Port);
	}

	public static void SpawnPlayer(Packet packet)
	{
		var id = packet.ReadInt();
		var playerName = packet.ReadString();
		var position = packet.ReadVector3();
		var rotation = packet.ReadQuaternion();

		GameManager.Instance.SpawnPlayer(id == Client.Instance.MyId, id, playerName, position, rotation);
	}

	public static void UpdatePlayerPosition(Packet packet)
	{
		var id = packet.ReadInt();
		var position = packet.ReadVector3();

		GameManager.Instance.Players[id].transform.position = position;
	}

	public static void UpdatePlayerRotation(Packet packet)
	{
		var id = packet.ReadInt();
		var rotation = packet.ReadQuaternion();

		GameManager.Instance.Players[id].transform.rotation = rotation;
	}

}