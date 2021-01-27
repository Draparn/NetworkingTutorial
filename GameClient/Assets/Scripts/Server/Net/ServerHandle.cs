using NetworkTutorial.Shared.Net;
using NetworkTutorial.Shared.Utils;
using UnityEngine;

namespace NetworkTutorial.Server.Net
{
	public class ServerHandle
	{
		public static void OnWelcomeReceived(byte clientId, Packet packet)
		{
			var claimedId = packet.ReadByte();
			var userName = packet.ReadString();

			if (clientId != claimedId)
			{
				Debug.Log($"Player \"{userName}\" (ID: {clientId} has assumed the wrong client ID ({claimedId})!");
				//TODO: disconnect client here and return
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

			Server.Clients[clientId].PlayerObject.UpdatePosAndRot(sequenceNumber, inputs, rotation);
		}

		public static void OnPlayerPrimaryFire(byte clientId, Packet packet)
		{
			var viewDirection = packet.ReadVector3();
			var sequenceNumber = packet.ReadUInt();

			Server.Clients[clientId].PlayerObject.PrimaryFire(viewDirection, sequenceNumber);
		}

		public static void OnPlayerWeaponSwitch(byte clientId, Packet packet)
		{
			Server.Clients[clientId].PlayerObject.WeaponSwitch(packet.ReadByte());
		}

	}
}
