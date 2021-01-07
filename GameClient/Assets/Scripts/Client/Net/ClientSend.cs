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
			var packet = PacketFactory.GetClientPacketType(ClientPackets.connectRequest);

			SendPacket(packet);
			packet.Reset();
		}

		public static void SendWelcomeReceived()
		{
			var packet = PacketFactory.GetClientPacketType(ClientPackets.welcomeReceived);

			packet.Write(LocalClient.Instance.MyId);
			packet.Write(LocalClient.Instance.playerName);

			SendPacket(packet);
			packet.Reset();
		}

		public static void SendDisconnect()
		{
			var packet = PacketFactory.GetClientPacketType(ClientPackets.disconnect);

			SendPacket(packet);
			packet.Reset();
		}

		public static void SendPlayerInputs(uint frameNumber, InputsStruct inputs)
		{
			var packet = PacketFactory.GetClientPacketType(ClientPackets.playerMovement);

			packet.Write(frameNumber);
			packet.Write(inputs);
			packet.Write(GameManagerClient.Instance.Players[LocalClient.Instance.MyId].transform.rotation);

			SendPacket(packet);
			packet.Reset();
		}

		public static void SendPlayerPrimaryFire(Vector3 facing)
		{
			var packet = PacketFactory.GetClientPacketType(ClientPackets.playerPrimaryFire);

			packet.Write(facing);

			SendPacket(packet);
			packet.Reset();
		}


		private static void SendPacket(Packet packet)
		{
			packet.WriteLength();
			LocalClient.Instance.Connection.SendData(packet);
		}
	}
}

