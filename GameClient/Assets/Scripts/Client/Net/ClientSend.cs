using NetworkTutorial.Client.Player;
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
				packet.Write(Client.Instance.MyId);
				SendPacket(packet);
			}
		}

		public static void SendWelcomeReceived()
		{
			using (Packet packet = new Packet((byte)ClientPackets.welcomeReceived))
			{
				packet.Write(Client.Instance.MyId);
				packet.Write(GameObject.FindObjectOfType<UIManager>().UserNameField.text);
				SendPacket(packet);
			}
		}

		public static void SendDisconnect()
		{
			using (Packet packet = new Packet((byte)ClientPackets.disconnect))
			{
				packet.Write(Client.Instance.MyId);
				SendPacket(packet);
			}
		}

		public static void SendPlayerInputs(uint frameNumber, InputsStruct inputs)
		{
			using (Packet packet = new Packet((byte)ClientPackets.playerMovement))
			{
				packet.Write(frameNumber);
				packet.Write(inputs);
				packet.Write(GameManager.Instance.Players[Client.Instance.MyId].transform.rotation);

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
			Client.Instance.udp.SendData(packet);
		}
	}
}

