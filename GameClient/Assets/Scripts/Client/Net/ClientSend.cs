using NetworkTutorial.Shared.Net;
using NetworkTutorial.Shared.Utils;
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

		public static void SendPlayerInputs(ushort sequenceNumber, InputsStruct inputs, Quaternion rotation)
		{
			var packet = PacketFactory.GetClientPacketType(ClientPackets.playerMovement);

			packet.Write(sequenceNumber);

			packet.Write(inputs.Forward);
			packet.Write(inputs.Back);
			packet.Write(inputs.Left);
			packet.Write(inputs.Right);
			packet.Write(inputs.Jump);

			if (Mathf.Abs(rotation.y) > Mathf.Abs(rotation.w))
			{
				packet.Write(false);
				packet.Write(rotation.y < 0 ? -(rotation.w) : rotation.w);
			}
			else
			{
				packet.Write(true);
				packet.Write(rotation.w < 0 ? -(rotation.y) : rotation.y);
			}

			SendPacket(packet);
			packet.Reset();
		}

		public static void SendPlayerPrimaryFire(Vector3 facing)
		{
			var packet = PacketFactory.GetClientPacketType(ClientPackets.playerPrimaryFire);

			packet.Write(facing.x);
			packet.Write(facing.y);
			packet.Write(facing.z);

			SendPacket(packet);
			packet.Reset();
		}


		private static void SendPacket(Packet packet)
		{
			packet.WriteLength();
			LocalClient.Instance.Connection.SendData(packet);
			//Debug.Log(packet.Length());
		}
	}
}

