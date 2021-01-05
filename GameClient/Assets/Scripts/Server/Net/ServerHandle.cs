using NetworkTutorial.Shared.Net;
using UnityEngine;

namespace NetworkTutorial.Server.Net
{
	public class ServerHandle
	{
		public static void OnWelcomeReceived(byte clientId, Packet packet)
		{
			var claimedId = packet.ReadByte();
			var userName = packet.ReadString();

			Debug.Log($"{Server.Clients[clientId].udp.endPoint} connected successfully and is now player {clientId}.");

			if (clientId != claimedId)
				Debug.Log($"Player \"{userName}\" (ID: {clientId} has assumed the wrong client ID ({claimedId})!");

			Server.Clients[clientId].SendIntoGame(userName);
		}

		public static void OnDisconnect(byte clientId, Packet packet)
		{
			Server.Clients[clientId].Disconnect();
		}

		public static void OnPlayerMovement(byte clientId, Packet packet)
		{
			var frameNumber = packet.ReadUInt();
			var inputs = packet.ReadInputs();
			var rotation = packet.ReadQuaternion();

			Server.Clients[clientId].Player.UpdatePosAndRot(frameNumber, inputs, rotation);
		}

		public static void OnPlayerPrimaryFire(byte clientId, Packet packet)
		{
			var viewDirection = packet.ReadVector3();

			Server.Clients[clientId].Player.PrimaryFire(viewDirection);
		}

	}
}
