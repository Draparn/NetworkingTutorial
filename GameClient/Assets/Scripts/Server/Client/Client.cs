using NetworkTutorial.Server.Managers;
using NetworkTutorial.Server.Net;
using NetworkTutorial.Shared;
using UnityEngine;

namespace NetworkTutorial.Server.Client
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
			player = NetworkManager.instance.InstantiatePlayer();
			player.Init(Id, playerName);

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
			Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

			ThreadManager.ExecuteOnMainThread(() =>
			{
				GameObject.Destroy(player.gameObject);
				player = null;
			});

			tcp.Disconnect();
			udp.Disconnect();
		}

	}
}
