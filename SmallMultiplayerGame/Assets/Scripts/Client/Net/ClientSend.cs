using SmallMultiplayerGame.Shared.Net;
using SmallMultiplayerGame.Shared.Utils;
using UnityEngine;

namespace SmallMultiplayerGame.Client.Net
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

			packet.Write(rotation);

			SendPacket(packet);
			packet.Reset();
		}

		public static void SendPlayerPrimaryFire(Vector3 facing)
		{
			var packet = PacketFactory.GetClientPacketType(ClientPackets.playerPrimaryFire);

			packet.Write(facing);
			packet.Write(ClientSnapshot.Snapshots[0].sequenceNumber);

			SendPacket(packet);
			packet.Reset();
		}

		public static void SendWeaponSwitch(byte weaponKey)
		{
			var packet = PacketFactory.GetClientPacketType(ClientPackets.playerWeaponSwitch);

			packet.Write(weaponKey);

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

