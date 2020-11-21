using System;
using System.Numerics;

namespace GameServer
{
	public class Client
	{
		public int Id;

		public Player player;

		public TCP tcp;
		public UDP udp;

		public Client(int id)
		{
			Id = id;
			tcp = new TCP(Id);
			udp = new UDP(Id);
		}

		public void SendIntoGame(string playerName)
		{
			player = new Player(Id, playerName, new Vector3(0, 0.5f, 0));

			foreach (var client in Server.Clients.Values)
			{
				if (client.player != null)
				{
					if (client.Id != Id)
						ServerSend.SendSpawnPlayer(Id, client.player);
				}
			}

			foreach (var client in Server.Clients.Values)
			{
				if (client.player != null)
					ServerSend.SendSpawnPlayer(client.Id, player);
			}

		}

		public void Disconnect()
		{
			Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");
			player = null;
			tcp.Disconnect();
			udp.Disconnect();
		}

	}
}
