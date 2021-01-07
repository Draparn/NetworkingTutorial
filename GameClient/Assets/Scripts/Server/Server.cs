﻿using NetworkTutorial.Server.Client;
using NetworkTutorial.Server.Net;
using NetworkTutorial.Shared.Net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace NetworkTutorial.Server
{
	public class Server
	{
		public delegate void PacketHandler(byte clientId, Packet packet);

		public static Dictionary<byte, PacketHandler> PacketHandlers;
		public static Dictionary<byte, ClientServer> Clients = new Dictionary<byte, ClientServer>();

		private static UdpClient udpListener;

		public static int MaxPlayers { get; set; }
		public static int Port { get; set; }

		public static void StartServer(int maxPlayers, int port)
		{
			MaxPlayers = maxPlayers;
			Port = port;

			Debug.Log("Starting server...");
			InitializeServerData();

			udpListener = new UdpClient(Port);
			udpListener.BeginReceive(UDPReceiveCallback, null);

			Debug.Log($"Server started on port {Port}.");
		}

		public static void StopServer()
		{
			udpListener.Close();
		}

		private static void UDPReceiveCallback(IAsyncResult result)
		{
			try
			{
				IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
				var data = udpListener.EndReceive(result, ref endPoint);
				udpListener.BeginReceive(UDPReceiveCallback, null);

				if (data.Length < 4 || (!HasConnected(endPoint) && !CheckEmptySlots(endPoint)))
					return;

				using (Packet packet = new Packet(data))
				{
					var clientId = packet.ReadByte();

					if (clientId == 0)
					{
						Debug.Log($"Incoming connection request from {endPoint} received and accepted. Awaiting confirmation...");
						return;
					}

					if (Clients[clientId].Connection.endPoint.ToString() == endPoint.ToString())
						Clients[clientId].Connection.HandleData(packet);
				}
			}
			catch (Exception ex)
			{
				Debug.Log($"Error receiving UDP data: {ex}");
			}
		}

		public static void SendUDPData(IPEndPoint endPoint, Packet packet)
		{
			try
			{
				if (endPoint != null)
				{
					udpListener.BeginSend(packet.ToArray(), packet.Length(), endPoint, null, null);
				}
			}
			catch (Exception ex)
			{
				Debug.Log($"Error sending data to {endPoint} via UDP: {ex}");
			}
		}

		private static void InitializeServerData()
		{
			for (byte i = 1; i <= MaxPlayers; i++)
				Clients.Add(i, new ClientServer(i));

			PacketHandlers = new Dictionary<byte, PacketHandler>();
			PacketHandlers.Add((byte)ClientPackets.welcomeReceived, ServerHandle.OnWelcomeReceived);
			PacketHandlers.Add((byte)ClientPackets.disconnect, ServerHandle.OnDisconnect);
			PacketHandlers.Add((byte)ClientPackets.playerMovement, ServerHandle.OnPlayerMovement);
			PacketHandlers.Add((byte)ClientPackets.playerPrimaryFire, ServerHandle.OnPlayerPrimaryFire);
		}

		private static bool HasConnected(IPEndPoint endPoint)
		{
			foreach (var client in Clients.Values)
			{
				if (client.Connection.endPoint != null && client.Connection.endPoint.ToString() == endPoint.ToString())
					return true;
			}

			return false;
		}

		private static bool CheckEmptySlots(IPEndPoint endPoint)
		{
			for (byte i = 1; i <= MaxPlayers; i++)
			{
				if (Clients[i].Connection.endPoint == null)
				{
					Clients[i].Connection.Connect(endPoint);
					ServerSend.SendWelcomeMessage_CLIENT(i, "Welcome to the party, pal!");
					return true;
				}
			}

			ServerSend.SendServerFull(endPoint);
			Debug.Log($"{endPoint.Address} failed to connect: Server was full.");

			return false;
		}

	}
}
