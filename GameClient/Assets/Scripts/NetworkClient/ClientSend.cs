namespace Assets.Scripts.Client
{
	public class ClientSend
	{
		public static void WelcomeReceived()
		{
			using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
			{
				packet.Write(Client.Instance.MyId);
				packet.Write(UIManager.Instance.UserNameField.text);

				SendTCPData(packet);
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
