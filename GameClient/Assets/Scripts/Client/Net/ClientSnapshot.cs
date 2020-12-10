using System.Collections.Generic;

namespace NetworkTutorial.Client.Net
{
	public class ClientSnapshot
	{
		public static List<ClientSnapshot> Snapshots = new List<ClientSnapshot>();

		internal List<PlayerData> players = new List<PlayerData>();
		internal List<ProjectileData> projectiles = new List<ProjectileData>();

		public ClientSnapshot(List<PlayerData> players, List<ProjectileData> projectiles)
		{
			this.players = players;
			this.projectiles = projectiles;
		}
	}
}
