using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GameServer
{
	public class Server
	{
		public delegate void PacketHandler(int clientId, Packet packet);

		public static Dictionary<int, PacketHandler> PacketHandlers;
		public static Dictionary<int, Client> Clients = new Dictionary<int, Client>();

		private static TcpListener tcpListener;

		public static int MaxPlayers { get; set; }
		public static int Port { get; set; }


		public static void StartServer(int maxPlayers, int port)
		{
			MaxPlayers = maxPlayers;
			Port = port;

			Console.WriteLine("Starting server...");
			InitializeServerData();

			tcpListener = new TcpListener(IPAddress.Any, Port);
			tcpListener.Start();
			tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

			Console.WriteLine($"Server started on port {Port}.");
		}

		private static void TCPConnectCallback(IAsyncResult result)
		{
			TcpClient client = tcpListener.EndAcceptTcpClient(result);
			tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);


			Console.WriteLine($"Incoming connection attempt from {client.Client.RemoteEndPoint}");
			for (int i = 1; i <= MaxPlayers; i++)
			{
				if (Clients[i].tcp.socket == null)
				{
					Clients[i].tcp.Connect(client);
					return;
				}
			}

			Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server was full.");
		}

		private static void InitializeServerData()
		{
			for (int i = 1; i <= MaxPlayers; i++)
				Clients.Add(i, new Client(i));

			PacketHandlers = new Dictionary<int, PacketHandler>() 
			{
				{
					(int)ClientPackets.welcomeReceived,
					ServerHandle.WelcomeReceived
				}
			};
			
			//PacketHandlers.Add((int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived);
		}

	}
}
