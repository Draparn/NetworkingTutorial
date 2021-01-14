﻿using NetworkTutorial.Client.Gameplay;
using NetworkTutorial.Client.Player;
using NetworkTutorial.Shared;
using NetworkTutorial.Shared.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace NetworkTutorial.Client.Net
{
	public class LocalClient : MonoBehaviour
	{
		public static LocalClient Instance;

		protected delegate void PacketHandler(Packet packet);
		protected static Dictionary<int, PacketHandler> packethandlers;

		public UDP Connection;

		public string playerName;

		private float disconnectTimer;

		[HideInInspector] public byte MyId = 0;

		public bool isConnected = false;

		private void Awake()
		{
			if (Instance == null)
				Instance = this;
			else if (Instance != this)
				Destroy(this);
		}

		private void Update()
		{
			if (isConnected)
			{
				disconnectTimer += Time.deltaTime;

				if (disconnectTimer >= 10.0)
				{
					Debug.Log("Server not responding. Disconnecting...");
					Instance.Disconnect();
					GameObject.Destroy(GameManagerClient.Instance.Players[Instance.MyId].gameObject);
					GameManagerClient.Instance.Players.Remove(Instance.MyId);
					UIManager.Instance.ConnectionTimedOut();
				}
			}
		}

		private void OnApplicationQuit()
		{
			Disconnect();
		}

		public void ConnectToServer(string ip, string playerName)
		{
			Connection = new UDP(ip);
			Connection.InitializeClientData();
			Connection.Connect();
			this.playerName = playerName;

			StartCoroutine(SendConnectAndWait());
		}

		public void Disconnect()
		{
			if (isConnected)
			{
				ClientSend.SendDisconnect();
				isConnected = false;
				Connection.socket.Close();
			}
		}

		public class UDP
		{
			public UdpClient socket;
			private IPEndPoint endpoint;

			public UDP(string ip)
			{
				endpoint = new IPEndPoint(IPAddress.Parse(ip), ConstantValues.PORT);
				socket = new UdpClient();
			}

			public void Connect()
			{
				socket.Connect(endpoint);
				socket.BeginReceive(ReceiveCallback, null);
			}

			public void InitializeClientData()
			{
				packethandlers = new Dictionary<int, PacketHandler>();
				packethandlers.Add((int)ServerPackets.welcome, ClientHandle.OnWelcomeMessage);
				packethandlers.Add((int)ServerPackets.serverFull, ClientHandle.OnServerFull);
				packethandlers.Add((int)ServerPackets.spawnPlayer, ClientHandle.OnPlayerConnected);
				packethandlers.Add((int)ServerPackets.playerDisconnected, ClientHandle.OnPlayerDisconnected);
				packethandlers.Add((int)ServerPackets.playerHealth, ClientHandle.OnPlayerHealthUpdate);
				packethandlers.Add((int)ServerPackets.playerRespawn, ClientHandle.OnPlayerRespawn);
				packethandlers.Add((int)ServerPackets.projectileSpawn, ClientHandle.OnProjectileSpawn);
				packethandlers.Add((int)ServerPackets.projectileExplosion, ClientHandle.OnProjectieExplosion);
				packethandlers.Add((int)ServerPackets.healthpackActivate, ClientHandle.OnHealthpackActivate);
				packethandlers.Add((int)ServerPackets.healthpackDeactivate, ClientHandle.OnHealthpackDeactivate);
				packethandlers.Add((int)ServerPackets.healthpackSpawn, ClientHandle.OnHealthpackSpawn);
				packethandlers.Add((int)ServerPackets.serverSnapshot, ClientHandle.OnNewSnapshot);
			}

			public void SendData(Packet packet)
			{
				try
				{
					packet.InsertByte(Instance.MyId);
					if (socket != null)
					{
						socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
					}
				}
				catch (Exception ex)
				{
					Debug.Log($"Error sending data to server via UDP: {ex}");
				}
			}

			private void ReceiveCallback(IAsyncResult result)
			{
				try
				{
					byte[] data = socket.EndReceive(result, ref endpoint);
					socket.BeginReceive(ReceiveCallback, null);

					if (data.Length < 3)
					{
						Instance.Disconnect();
						return;
					}

					HandleData(data);
					Instance.disconnectTimer = 0;
				}
				catch
				{
					Disconnect();
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

			private void Disconnect()
			{
				Instance.Disconnect();

				endpoint = null;
				socket = null;
			}

		}

		private IEnumerator SendConnectAndWait()
		{
			byte ticker = 0;

			do
			{
				ClientSend.SendConnectRequest();
				yield return new WaitForSeconds(2);
				ticker++;
			} while (ticker < 5);

			UIManager.Instance.ConnectionTimedOut();
		}

		public void StopConnectionTimer()
		{
			StopAllCoroutines();
		}
	}
}