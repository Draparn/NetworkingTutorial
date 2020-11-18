using System;
using System.Numerics;

namespace GameServer
{
	class ServerHandle
	{
		public static void WelcomeReceived(int clientId, Packet packet)
		{
			var claimedId = packet.ReadInt();
			var userName = packet.ReadString();

			Console.WriteLine($"{Server.Clients[clientId].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {clientId}.");

			if (clientId != claimedId)
				Console.WriteLine($"Player \"{userName}\" (ID: {clientId} has assumed the wrong client ID ({claimedId})!");

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
