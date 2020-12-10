using NetworkTutorial.Server.Client;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Server.Net
{
	public struct PlayerPosRotData
	{
		public Vector3 Position;
		public int Id;
		public uint FrameNumber;

		public PlayerPosRotData(int id, Vector3 pos, uint frameNumber)
		{
			Id = id;
			Position = pos;
			FrameNumber = frameNumber;
		}

	}

	public class ServerSnapshot
	{
		public static ServerSnapshot currentSnapshot = new ServerSnapshot();

		public List<PlayerPosRotData> PlayerPositionAndRotation = new List<PlayerPosRotData>();
		public List<Projectile> ProjectilePositions = new List<Projectile>();

		public static void AddPlayerMovement(int id, Vector3 pos, uint frameNumber)
		{
			if (currentSnapshot.PlayerPositionAndRotation.Exists(entry => entry.Id == id))
			{
				var index = currentSnapshot.PlayerPositionAndRotation.FindIndex(entry => entry.Id == id);
				var data = currentSnapshot.PlayerPositionAndRotation[index];

				data.Position = pos;
				data.FrameNumber = frameNumber;
			}
			else
				currentSnapshot.PlayerPositionAndRotation.Add(new PlayerPosRotData(id, pos, frameNumber));
		}
		public static void RemovePlayerMovement(Player player)
		{
			if (currentSnapshot.PlayerPositionAndRotation.Exists(entry => entry.Id == player.PlayerId))
			{
				var index = currentSnapshot.PlayerPositionAndRotation.FindIndex(entry => entry.Id == player.PlayerId);
				currentSnapshot.PlayerPositionAndRotation.RemoveAt(index);
			}
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
			currentSnapshot.PlayerPositionAndRotation.Clear();
			currentSnapshot.ProjectilePositions.Clear();
		}
	}
}
