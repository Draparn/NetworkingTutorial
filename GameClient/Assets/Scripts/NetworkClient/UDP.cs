﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Assets.Scripts.Client
{
	public class UDP : Client
	{
		UdpClient socket;
		IPEndPoint endpoint;

		public UDP()
		{
			endpoint = new IPEndPoint(IPAddress.Parse(ConstantValues.localHost), ConstantValues.Port);
		}

		public void Connect(int localPort)
		{
			socket = new UdpClient(localPort);
			socket.Connect(endpoint);

			socket.BeginReceive(ReceiveCallback, null);

			using (Packet packet = new Packet())
			{
				SendData(packet);
			}
		}

		public void SendData(Packet packet)
		{
			try
			{
				packet.InsertInt(Instance.MyId);
				if (socket != null)
				{
					socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error sending data to server vis UDP: {ex}");
			}
		}

		private void ReceiveCallback(IAsyncResult result)
		{
			try
			{
				byte[] data = socket.EndReceive(result, ref endpoint);
				socket.BeginReceive(ReceiveCallback, null);

				if (data.Length < 4)
				{
					//TODO: Disconnect
					return;
				}

				HandleData(data);
			}
			catch (Exception)
			{
				//TOOD: Disconnect
			}
		}

		private void HandleData(byte[] data)
		{
			using (Packet packet = new Packet(data))
			{
				int packetLength = packet.ReadInt();
				data = packet.ReadBytes(packetLength);
			}

			ThreadManager.ExecuteOnMainThread(() => 
			{
				using (Packet packet = new Packet(data))
				{
					int packetId = packet.ReadInt();
					packethandlers[packetId](packet);
				}
			});
		}

	}
}