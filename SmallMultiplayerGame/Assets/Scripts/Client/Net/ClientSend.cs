using SmallMultiplayerGame.Shared.Net;
using SmallMultiplayerGame.Shared.Utils;
using UnityEngine;

namespace SmallMultiplayerGame.Client.Net
{
	public class ClientSend
	{
		private static Packet packet;

		public static void SendConnectRequest()
		{
			var packet = PacketFactory.GetClientPacketType(ClientPackets.connectRequest);

			SendPacket(packet);
			packet.Reset();
		}

		public static void SendWelcomeReceived()
		{
			packet = PacketFactory.GetClientPacketType(ClientPackets.welcomeReceived);

			packet.Write(LocalClient.Instance.playerName);

			SendPacket(packet);
			packet.Reset();
		}

		public static void SendDisconnect()
		{
			packet = PacketFactory.GetClientPacketType(ClientPackets.disconnect);

			SendPacket(packet);
			packet.Reset();
		}

		public static void SendPlayerInputs(uint sequenceNumber, InputsStruct inputs, Quaternion rotation)
		{
			packet = PacketFactory.GetClientPacketType(ClientPackets.playerMovement);

			packet.Write(sequenceNumber);
			packet.Write(ValueTypeConversions.ReturnBoolsAsByte(new bool[] { inputs.Forward, inputs.Back, inputs.Left, inputs.Right, inputs.Jump }));
			packet.Write(rotation);

			SendPacket(packet);
			packet.Reset();
		}

		public static void SendPlayerPrimaryFire(Vector3 facing)
		{
			packet = PacketFactory.GetClientPacketType(ClientPackets.playerPrimaryFire);
			packet.Write(facing);
			packet.Write(ClientSnapshot.Snapshots[0].sequenceNumber);

			SendPacket(packet);
			packet.Reset();
		}

		public static void SendWeaponSwitch(byte weaponKey)
		{
			packet = PacketFactory.GetClientPacketType(ClientPackets.playerWeaponSwitch);

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

