

namespace NetworkTutorial.Shared
{
	public class ConstantValues
	{
		public const int DATA_BUFFER_SIZE = 4096;
		public const int SERVER_PORT = 1986;
		public const int SERVER_MAX_PLAYERS = 3;
		public const int SERVER_TICKS_PER_SECOND = 50;

		public const float SERVER_TICK_RATE = 1.0f / SERVER_TICKS_PER_SECOND;
		public const float CLIENT_SNAPSHOT_BUFFER_LENGTH = SERVER_TICK_RATE * 2.0f;
	}
}