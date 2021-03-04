namespace SmallMultiplayerGame.Shared
{
	public class ConstantValues
	{
		//Server
		public const int SERVER_PORT = 56789;
		public const int SERVER_MAX_PLAYERS = 3;
		public const float SERVER_TICK_RATE = 1.0f / 20.0f;

		//Gameplay
		public const float PLAYER_RESPAWN_TIME = 3.0f;
		public const float PLAYER_MOVE_SPEED = 10.0f;
		public const float PLAYER_JUMP_FORCE = 8.0f;
		public const float WORLD_GRAVITY = -25f;

		//Connection
		public const float CONNECTION_TIMEOUT_TIMER = 5.0f;
	}
}