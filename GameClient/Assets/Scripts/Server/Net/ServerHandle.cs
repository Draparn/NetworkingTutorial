﻿using NetworkTutorial.Shared.Net;
using UnityEngine;

namespace NetworkTutorial.Server.Net
{
	class ServerHandle
	{
		public static void WelcomeReceived(int clientId, Packet packet)
		{
			var claimedId = packet.ReadInt();
			var userName = packet.ReadString();

			Debug.Log($"{Server.Clients[clientId].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {clientId}.");

			if (clientId != claimedId)
				Debug.Log($"Player \"{userName}\" (ID: {clientId} has assumed the wrong client ID ({claimedId})!");

			Server.Clients[clientId].SendIntoGame(userName);
		}

		public static void PlayerMovement(int clientId, Packet packet)
		{
			bool[] inputs = new bool[packet.ReadInt()];
			for (int i = 0; i < inputs.Length; i++)
				inputs[i] = packet.ReadBool();

			var rotation = packet.ReadQuaternion();

			Server.Clients[clientId].player.UpdatePosAndRot(inputs, rotation);
		}

	}
}
