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

		

		private static UdpClient udpListener;

		public static int MaxPlayers { get; set; }
		public static int Port { get; set; }

		#region Server Connectivity
		public static void StartServer(int maxPlayers, int port)
		{
			MaxPlayers = maxPlayers;
			Port = port;

			Console.WriteLine("Starting server...");
			InitializeServerData();

			tcpListener = new TcpListener(IPAddress.Any, Port);
			tcpListener.Start();
			tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

			udpListener = new UdpClient(Port);
			udpListener.BeginReceive(UDPReceiveCallback, null);

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
					int clientId = packet.ReadInt();
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
				Console.WriteLine($"Error receiving UDP data: {ex}");
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
				Console.WriteLine($"Error sending data to {endPoint} via UDP: {ex}");
			}
		}

		private static void InitializeServerData()
		{
			for (int i = 1; i <= MaxPlayers; i++)
				Clients.Add(i, new Client(i));

			PacketHandlers = new Dictionary<int, PacketHandler>();
			PacketHandlers.Add((int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived);
		}
		#endregion

		#region Game related
		public static void SpawnPlayer(int id, Player player)
		{

		}
		#endregion

	}
}
