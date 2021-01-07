using NetworkTutorial.Client.Gameplay;
using NetworkTutorial.Shared;
using NetworkTutorial.Shared.Net;
using UnityEngine;

namespace NetworkTutorial.Client.Net
{
	public class ClientSend
	{
		public static void SendConnectRequest()
		{
			using (Packet packet = new Packet((byte)ClientPackets.connectRequest))
			{
				SendPacket(packet);
			}
		}

		public static void SendWelcomeReceived()
		{
			using (Packet packet = new Packet((byte)ClientPackets.welcomeReceived))
			{
				packet.Write(LocalClient.Instance.MyId);
				packet.Write(LocalClient.Instance.playerName);
				SendPacket(packet);
			}
		}

		public static void SendDisconnect()
		{
			using (Packet packet = new Packet((byte)ClientPackets.disconnect))
			{
				SendPacket(packet);
			}
		}

		public static void SendPlayerInputs(uint frameNumber, InputsStruct inputs)
		{
			using (Packet packet = new Packet((byte)ClientPackets.playerMovement))
			{
				packet.Write(frameNumber);
				packet.Write(inputs);
				packet.Write(GameManagerClient.Instance.Players[LocalClient.Instance.MyId].transform.rotation);

				SendPacket(packet);
			}
		}

		public static void SendPlayerPrimaryFire(Vector3 facing)
		{
			using (Packet packet = new Packet((byte)ClientPackets.playerPrimaryFire))
			{
				packet.Write(facing);

				SendPacket(packet);
			}
		}


		private static void SendPacket(Packet packet)
		{
			packet.WriteLength();
			LocalClient.Instance.Connection.SendData(packet);
		}
	}
}

