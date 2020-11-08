using System;

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
			
			//Send player into game here
		}
	}
}
