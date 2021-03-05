using SmallMultiplayerGame.Server.Gameplay;
using SmallMultiplayerGame.Server.Gameplay.Pickups;
using SmallMultiplayerGame.Server.Net;
using SmallMultiplayerGame.Shared;
using UnityEngine;

namespace SmallMultiplayerGame.Server.Client
{
	public partial class Client
	{
		public PlayerObjectServer Player;
		public UDP Connection;

		public float DisconnectTimer;
		public byte Id;

		public Client(byte id)
		{
			Id = id;
			Connection = new UDP(Id);
		}

		public void SendIntoGame(string playerName)
		{
			Player = GameManagerServer.Instance.InstantiatePlayer();
			Player.Init(Id, playerName);

			//ServerSend.SendNewClientInfo(Id); //Here for later optimization

			//Players
			var clients = Server.Clients.Values;
			foreach (var client in clients)
			{
				if (client.Player != null)
				{
					ServerSend.SendPlayerConnected_CLIENT(client.Id, Player);

					if (client.Id != Id)
						ServerSend.SendPlayerConnected_CLIENT(Id, client.Player);
				}
			}

			//Healthpacks
			var healthpacks = GameManagerServer.Instance.GetHealthpacks();
			foreach (var kvp in healthpacks)
			{
				ServerSend.SendHealthpackSpawn_CLIENT(Id, kvp.Key, kvp.Value);
			}

			//Weapon pickups
			var grenadeLaunchers = GrenadeLauncherServer.GrenadeLaunchers;
			foreach (var gl in grenadeLaunchers)
			{
				ServerSend.SendWeaponSpawn_CLIENT(Id, gl.WeaponSlot, gl.MyId, gl.gameObject.transform.position, gl.IsActive);
			}

		}

		public void Disconnect()
		{
			Debug.Log($"{Connection.endPoint} has disconnected.");
			Connection.Disconnect();

			ThreadManager.ExecuteOnMainThread(() =>
			{
				ServerSnapshot.RemovePlayerMovement(Player);
				GameObject.Destroy(Player.gameObject);
				Player = null;
			});

			ServerSend.SendPlayerDisconnected_ALL(Id);
		}

	}
}
