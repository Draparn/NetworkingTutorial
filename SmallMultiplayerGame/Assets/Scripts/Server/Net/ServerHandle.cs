using SmallMultiplayerGame.Shared.Net;
using SmallMultiplayerGame.Shared.Utils;
using UnityEngine;

namespace SmallMultiplayerGame.Server.Net
{
	public class ServerHandle
	{
		private static InputsStruct inputs;
		private static Quaternion rotation;

		private static uint sequenceNumber;

		public static void OnWelcomeReceived(byte clientId, Packet packet)
		{
			var userName = packet.ReadString();

			if (Server.CheckNames(clientId, userName))
			{
				Debug.Log($"Connecting player had duplicate name: \"{userName}\". Disconnecting new client...");
				ServerSend.SendNameTaken(clientId);
				Server.Clients[clientId].Connection.Disconnect();
				return;
			}

			Debug.Log($"{Server.Clients[clientId].Connection.endPoint} connected successfully and is now player {clientId}.");

			Server.Clients[clientId].SendIntoGame(userName);
		}

		public static void OnDisconnect(byte clientId, Packet packet)
		{
			Server.Clients[clientId].Disconnect();
		}

		public static void OnPlayerMovement(byte clientId, Packet packet)
		{
			sequenceNumber = packet.ReadUInt();
			inputs = ValueTypeConversions.ReturnByteAsInput(packet.ReadByte());
			rotation = packet.ReadQuaternion();

			Server.Clients[clientId].Player.UpdatePosAndRot(sequenceNumber, inputs, rotation);
		}

		public static void OnPlayerPrimaryFire(byte clientId, Packet packet)
		{
			var viewDirection = packet.ReadVector3();
			var sequenceNumber = packet.ReadUInt();

			Server.Clients[clientId].Player.PrimaryFire(viewDirection, sequenceNumber);
		}

		public static void OnPlayerWeaponSwitch(byte clientId, Packet packet)
		{
			Server.Clients[clientId].Player.WeaponSwitch(packet.ReadByte());
		}

	}
}
