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
			player = new Player(Id, playerName, new Vector3(0, 0, 0));

			foreach (var client in Server.Clients.Values)
			{
				if (client.Id != Id)
				{
					Server.SpawnRemotePlayer(Id, client.player);
					Server.SpawnRemotePlayer(client.Id, player);
				}
			}

		}
	}
}
