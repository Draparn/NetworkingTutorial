using System;
using System.Collections.Generic;
using System.Net;
using NetworkTutorial.Shared.Net;
using System.Net.Sockets;
using UnityEngine;
using NetworkTutorial.Server.Net;

namespace NetworkTutorial.Server
{
	public class Server
	{
		public delegate void PacketHandler(int clientId, Packet packet);

		public static Dictionary<int, PacketHandler> PacketHandlers;
		public static Dictionary<int, Client.Client> Clients = new Dictionary<int, Client.Client>();

		private static TcpListener tcpListener;
		private static UdpClient udpListener;

		public static int MaxPlayers { get; set; }
		public static int Port { get; set; }

		public static void StartServer(int maxPlayers, int port)
		{
			MaxPlayers = maxPlayers;
			Port = port;

			Debug.Log("Starting server...");
			InitializeServerData();

			tcpListener = new TcpListener(IPAddress.Any, Port);
			tcpListener.Start();
			tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

			udpListener = new UdpClient(Port);
			udpListener.BeginReceive(UDPReceiveCallback, null);

			Debug.Log($"Server started on port {Port}.");
		}

		public static void StopServer()
		{
			tcpListener.Stop();
			udpListener.Close();
		}

		private static void TCPConnectCallback(IAsyncResult result)
		{
			TcpClient client = tcpListener.EndAcceptTcpClient(result);
			tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

			Debug.Log($"Incoming connection attempt from {client.Client.RemoteEndPoint}");
			for (int i = 1; i <= MaxPlayers; i++)
			{
				if (Clients[i].tcp.socket == null)
				{
					Clients[i].tcp.Connect(client);
					return;
				}
			}

			Debug.Log($"{client.Client.RemoteEndPoint} failed to connect: Server was full.");
		}

		private static void UDPReceiveCallback(IAsyncResult result)
		{
			try
			{
				IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
				var data = udpListener.EndReceive(result, ref endPoint);
				udpListener.BeginReceive(UDPReceiveCallback, null);

				if (data.Length < 4)
					return;

				using (Packet packet = new Packet(data))
				{
					var clientId = packet.ReadInt();
					if (clientId == 0)
						return;

					if (Clients[clientId].udp.endPoint == null)
					{
						Clients[clientId].udp.Connect(endPoint);
						return;
					}

					if (Clients[clientId].udp.endPoint.ToString() == endPoint.ToString())
						Clients[clientId].udp.HandleData(packet);
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
			for (int i = 1; i <= MaxPlayers; i++)
				Clients.Add(i, new Client.Client(i));

			PacketHandlers = new Dictionary<int, PacketHandler>();
			PacketHandlers.Add((int)ClientPackets.welcomeReceived, ServerHandle.OnWelcomeReceived);
			PacketHandlers.Add((int)ClientPackets.playerMovement, ServerHandle.OnPlayerMovement);
			PacketHandlers.Add((int)ClientPackets.playerPrimaryFire, ServerHandle.OnPlayerPrimaryFire);
		}

	}
}
