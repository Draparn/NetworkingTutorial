using NetworkTutorial.Shared.Net;
using NetworkTutorial.Shared.Utils;
using UnityEngine;

namespace NetworkTutorial.Server.Net
{
	public class ServerHandle
	{
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
			var sequenceNumber = packet.ReadUShort();
			var inputs = new InputsStruct(packet.ReadBool(), packet.ReadBool(), packet.ReadBool(), packet.ReadBool(), packet.ReadBool());
			var rotation = packet.ReadQuaternion();

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
