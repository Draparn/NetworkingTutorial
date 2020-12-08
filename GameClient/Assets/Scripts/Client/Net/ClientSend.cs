using NetworkTutorial.Client.Player;
using NetworkTutorial.Shared.Net;
using UnityEngine;

namespace NetworkTutorial.Client.Net
{
	public class ClientSend
	{
		public static void SendWelcomeReceived()
		{
			using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
			{
				packet.Write(Client.Instance.MyId);
				packet.Write(GameObject.FindObjectOfType<UIManager>().UserNameField.text);
				SendTCPData(packet);
			}
		}

		public static void SendPlayerInputs(uint frameNumber, bool[] inputs)
		{
			using (Packet packet = new Packet((int)ClientPackets.playerMovement))
			{
				packet.Write(frameNumber);

				packet.Write(inputs.Length);
				for (int i = 0; i < inputs.Length; i++)
					packet.Write(inputs[i]);

				packet.Write(GameManager.Instance.Players[Client.Instance.MyId].transform.rotation);

				SendUDPData(packet);
			}
		}

		public static void SendPlayerPrimaryFire(Vector3 facing)
		{
			using (Packet packet = new Packet((int)ClientPackets.playerPrimaryFire))
			{
				packet.Write(facing);

				SendTCPData(packet);
			}
		}

		#region SendToServerOptions
		private static void SendTCPData(Packet packet)
		{
			packet.WriteLength();
			Client.Instance.tcp.SendData(packet);
		}

		private static void SendUDPData(Packet packet)
		{
			packet.WriteLength();
			Client.Instance.udp.SendData(packet);
		}
		#endregion
	}
}

