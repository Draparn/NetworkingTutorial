using NetworkTutorial.Server.Managers;
using NetworkTutorial.Server.Net;
using NetworkTutorial.Shared;
using UnityEngine;

namespace NetworkTutorial.Server.Client
{
	public class Client
	{
		public byte Id;

		public Player Player;

		public UDP udp;

		public Client(byte id)
		{
			Id = id;
			udp = new UDP(Id);
		}

		public void SendIntoGame(string playerName)
		{
			Player = NetworkManager.instance.InstantiatePlayer();
			Player.Init(Id, playerName);

			//Players
			foreach (var client in Server.Clients.Values)
			{
				if (client.Player != null)
				{
					ServerSend.SendPlayerConnected_CLIENT(client.Id, Player);

					if (client.Id != Id)
						ServerSend.SendPlayerConnected_CLIENT(Id, client.Player);
				}
			}

			//Healthpacks
			foreach (var kvp in GameManagerServer.healthpacks)
			{
				ServerSend.SendHealthpackSpawn_CLIENT(Id, kvp.Key, kvp.Value.gameObject.transform.position);
			}

		}

		public void Disconnect()
		{
			Debug.Log($"{udp.endPoint.Address /*tcp.socket.Client.RemoteEndPoint*/} has disconnected.");

			ThreadManager.ExecuteOnMainThread(() =>
			{
				GameObject.Destroy(Player.gameObject);
				ServerSnapshot.RemovePlayerMovement(Player);
				Player = null;
			});

			udp.Disconnect();

			ServerSend.SendPlayerDisconnected_ALL(Id);
		}

	}
}
