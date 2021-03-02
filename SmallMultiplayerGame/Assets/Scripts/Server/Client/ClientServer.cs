using SmallMultiplayerGame.Server.Gameplay;
using SmallMultiplayerGame.Server.Net;
using SmallMultiplayerGame.Shared;
using SmallMultiplayerGame.Shared.Net;
using System.Net;
using UnityEngine;

namespace SmallMultiplayerGame.Server.Client
{
	public class ClientServer
	{
		public PlayerServer Player;
		public UDP Connection;

		public float DisconnectTimer;
		public byte Id;

		public ClientServer(byte id)
		{
			Id = id;
			Connection = new UDP(Id);
		}

		public void SendIntoGame(string playerName)
		{
			Player = GameManagerServer.Instance.InstantiatePlayer();
			Player.Init(Id, playerName);

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

		public class UDP
		{
			public IPEndPoint endPoint;

			private byte clientId;

			public UDP(byte id)
			{
				clientId = id;
			}

			public void Connect(IPEndPoint endPoint)
			{
				this.endPoint = endPoint;
			}

			public void SendData(Packet packet)
			{
				Server.SendPacket(endPoint, packet);
			}

			public void HandleData(Packet packet)
			{
				int packetLength = packet.ReadUShort();
				byte[] packetBytes = packet.ReadBytes(packetLength);

				ThreadManager.ExecuteOnMainThread(() =>
				{
					using (Packet pkt = new Packet(packetBytes))
					{
						var packetId = pkt.ReadByte();
						Server.PacketHandlers[packetId](clientId, pkt);
					}
				});
			}

			public void Disconnect()
			{
				endPoint = null;
			}

		}

	}
}
