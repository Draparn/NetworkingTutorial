

namespace NetworkTutorial.Shared
{
	public class ConstantValues
	{
		public const int DATA_BUFFER_SIZE = 4096;
		public const int SERVER_PORT = 1986;
		public const int SERVER_MAX_PLAYERS = 3;
		public const int SERVER_TICKS_PER_SECOND = 20;

		public const float SERVER_TICK_RATE = 1.0f / SERVER_TICKS_PER_SECOND;
		public const float CLIENT_SNAPSHOT_BUFFER_LENGTH = SERVER_TICK_RATE * 2.0f;

		public const float PLAYER_MOVE_SPEED = 7.0f;
		public const float PLAYER_JUMP_SPEED = 5.0f;

		public const float WORLD_GRAVITY = -15.0f;
	}
}