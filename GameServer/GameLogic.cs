namespace GameServer
{
	class GameLogic
	{
		public static void Update()
		{
			foreach (var client in Server.Clients.Values)
			{
				if (client.player != null)
					client.player.Update();
			}

			ThreadManager.UpdateMain();
		}
	}
}
