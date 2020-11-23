using NetworkTutorial.Shared.Net;

namespace NetworkTutorial.Client
{
	public class ClientSend
	{
		public static void SendWelcomeReceived()
		{
			using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
			{
				packet.Write(Client.Instance.MyId);
				packet.Write(UIManager.Instance.UserNameField.text);

				SendTCPData(packet);
			}
		}

		public static void SendPlayerInputs(bool[] inputs)
		{
			using (Packet packet = new Packet((int)ClientPackets.playerMovement))
			{
				packet.Write(inputs.Length);
				for (int i = 0; i < inputs.Length; i++)
					packet.Write(inputs[i]);

				packet.Write(GameManager.Instance.Players[Client.Instance.MyId].transform.rotation);

				SendUDPData(packet);
			}
		}

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

	}
}
