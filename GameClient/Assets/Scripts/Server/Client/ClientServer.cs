using NetworkTutorial.Server.Gameplay;
using NetworkTutorial.Server.Net;
using NetworkTutorial.Shared;
using NetworkTutorial.Shared.Net;
using System.Net;
using UnityEngine;

namespace NetworkTutorial.Server.Client
{
	public class ClientServer
	{
		public PlayerServer PlayerObject;
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
			PlayerObject = GameManagerServer.Instance.InstantiatePlayer();
			PlayerObject.Init(Id, playerName);

			//Players
			foreach (var client in Server.Clients.Values)
			{
				if (client.PlayerObject != null)
				{
					ServerSend.SendPlayerConnected_CLIENT(client.Id, PlayerObject);

					if (client.Id != Id)
						ServerSend.SendPlayerConnected_CLIENT(Id, client.PlayerObject);
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
			Debug.Log($"{Connection.endPoint} has disconnected.");
			Connection.Disconnect();

			ThreadManager.ExecuteOnMainThread(() =>
			{
				ServerSnapshot.RemovePlayerMovement(PlayerObject);
				GameObject.Destroy(PlayerObject.gameObject);
				PlayerObject = null;
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
