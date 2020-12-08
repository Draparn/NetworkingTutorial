using NetworkTutorial.Server.Client;
using System.Collections.Generic;

namespace NetworkTutorial.Server.Net
{
	public class ServerSnapshot
	{
		public static ServerSnapshot currentSnapshot = new ServerSnapshot();

		public List<Player> PlayerPositions = new List<Player>();
		public List<Projectile> ProjectilePositions = new List<Projectile>();

		public static void AddPlayerMovement(uint frameNumber, Player player)
		{
			if (currentSnapshot.PlayerPositions.Contains(player))
				currentSnapshot.PlayerPositions[currentSnapshot.PlayerPositions.IndexOf(player)].transform.position = player.transform.position;
			else
				currentSnapshot.PlayerPositions.Add(player);
		}
		public static void RemovePlayerMovement(Player player)
		{
			currentSnapshot.PlayerPositions.Remove(player);
		}

		public static void AddProjectileMovement(Projectile proj)
		{
			if (currentSnapshot.ProjectilePositions.Contains(proj))
				currentSnapshot.ProjectilePositions[currentSnapshot.ProjectilePositions.IndexOf(proj)].transform.position = proj.transform.position;
			else
				currentSnapshot.ProjectilePositions.Add(proj);
		}
		public static void RemoveProjectileMovement(Projectile proj)
		{
			currentSnapshot.ProjectilePositions.Remove(proj);
		}

		public static void ClearSnapshot()
		{
			currentSnapshot.PlayerPositions.Clear();
			currentSnapshot.ProjectilePositions.Clear();
		}
	}
}
