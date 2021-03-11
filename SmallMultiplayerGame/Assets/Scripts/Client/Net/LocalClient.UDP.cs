using SmallMultiplayerGame.Shared;
using SmallMultiplayerGame.Shared.Net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace SmallMultiplayerGame.Client.Net
{
	public partial class LocalClient
	{
		public class UDP
		{
			public UdpClient udpClient;
			private IPEndPoint endpoint;

			public UDP(string ip)
			{
				endpoint = new IPEndPoint(IPAddress.Parse(ip), ConstantValues.SERVER_PORT);
				udpClient = new UdpClient();
			}

			public void Connect()
			{
				udpClient.Connect(endpoint);
				udpClient.BeginReceive(ReceiveCallback, null);
			}

			public void InitializeClientData()
			{
				packethandlers = new Dictionary<int, PacketHandler>();
				packethandlers.Add((int)ServerPackets.welcome, ClientHandle.OnWelcomeMessage);
				packethandlers.Add((int)ServerPackets.serverFull, ClientHandle.OnServerFull);
				packethandlers.Add((int)ServerPackets.nameTaken, ClientHandle.OnNameTaken);
				packethandlers.Add((int)ServerPackets.playerSpawn, ClientHandle.OnPlayerConnected);
				packethandlers.Add((int)ServerPackets.playerDisconnected, ClientHandle.OnPlayerDisconnected);
				packethandlers.Add((int)ServerPackets.playerHealth, ClientHandle.OnPlayerHealthUpdate);
				packethandlers.Add((int)ServerPackets.playerRespawn, ClientHandle.OnPlayerRespawn);
				packethandlers.Add((int)ServerPackets.playerWeaponSwitch, ClientHandle.OnPlayerWeaponSwitch);
				packethandlers.Add((int)ServerPackets.playerFiredWeapon, ClientHandle.OnPlayerFiredWeapon);
				packethandlers.Add((int)ServerPackets.weaponPickup, ClientHandle.OnPlayerWeaponPickup);
				packethandlers.Add((int)ServerPackets.weaponSpawn, ClientHandle.OnWeaponSpawn);
				packethandlers.Add((int)ServerPackets.weaponAmmoUpdate, ClientHandle.OnPlayerWeaponAmmoUpdate);
				packethandlers.Add((int)ServerPackets.weaponPickupStatus, ClientHandle.OnWeaponPickupStatusUpdate);
				packethandlers.Add((int)ServerPackets.projectileSpawn, ClientHandle.OnProjectileSpawn);
				packethandlers.Add((int)ServerPackets.projectileExplosion, ClientHandle.OnProjectieExplosion);
				packethandlers.Add((int)ServerPackets.healthpackStatusUpdate, ClientHandle.OnHealthpackUpdate);
				packethandlers.Add((int)ServerPackets.healthpackSpawn, ClientHandle.OnHealthpackSpawn);
				packethandlers.Add((int)ServerPackets.serverSnapshot, ClientHandle.OnNewSnapshot);
			}

			public void SendData(Packet packet)
			{
				try
				{
					packet.InsertByte(Instance.MyId);
					if (udpClient != null)
					{
						udpClient.BeginSend(packet.ToArray(), packet.GetLength(), null, null);
					}
				}
				catch (Exception ex)
				{
					Debug.Log($"Error sending data to server: {ex}");
				}
			}

			private void ReceiveCallback(IAsyncResult result)
			{
				try
				{
					byte[] data = udpClient.EndReceive(result, ref endpoint);
					udpClient.BeginReceive(ReceiveCallback, null);

					if (data.Length < 3)
					{
						ClientSend.SendDisconnect();
						Instance.Disconnect();
						return;
					}

					HandleData(data);
					Instance.disconnectTimer = 0;
				}
				catch
				{
					ClientSend.SendDisconnect();
					Instance.Disconnect();
				}
			}

			private void HandleData(byte[] data)
			{
				using (Packet packet = new Packet(data))
				{
					int packetLength = packet.ReadUShort();
					data = packet.ReadBytes(packetLength);
				}

				ThreadManager.ExecuteOnMainThread(() =>
				{
					using (Packet packet = new Packet(data))
					{
						int packetId = packet.ReadByte();
						packethandlers[packetId](packet);
					}
				});
			}

		}
	}
}