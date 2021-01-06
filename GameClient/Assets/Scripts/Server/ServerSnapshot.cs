using NetworkTutorial.Server.Client;
using NetworkTutorial.Server.Gameplay;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Server
{
	public struct PlayerPosData
	{
		public byte Id;
		public Vector3 Position;
		public uint FrameNumber;

		public PlayerPosData(byte id, Vector3 pos, uint frameNumber)
		{
			Id = id;
			Position = pos;
			FrameNumber = frameNumber;
		}

	}

	public class ServerSnapshot
	{
		public static ServerSnapshot currentSnapshot = new ServerSnapshot();

		public Dictionary<int, PlayerPosData> PlayerPosition = new Dictionary<int, PlayerPosData>();
		public List<ProjectileServer> ProjectilePositions = new List<ProjectileServer>();

		public static void AddPlayerMovement(byte id, Vector3 pos, uint frameNumber)
		{
			if (frameNumber == uint.MaxValue)
				return;

			if (currentSnapshot.PlayerPosition.ContainsKey(id))
				currentSnapshot.PlayerPosition[id] = new PlayerPosData(id, pos, frameNumber);
			else
				currentSnapshot.PlayerPosition.Add(id, new PlayerPosData(id, pos, frameNumber));
		}
		public static void RemovePlayerMovement(PlayerServer player)
		{
			if (currentSnapshot.PlayerPosition.ContainsKey(player.PlayerId))
				currentSnapshot.PlayerPosition.Remove(player.PlayerId);
		}

		public static void AddProjectileMovement(ProjectileServer proj)
		{
			if (currentSnapshot.ProjectilePositions.Contains(proj))
				currentSnapshot.ProjectilePositions[currentSnapshot.ProjectilePositions.IndexOf(proj)].transform.position = proj.transform.position;
			else
				currentSnapshot.ProjectilePositions.Add(proj);
		}
		public static void RemoveProjectileMovement(ProjectileServer proj)
		{
			currentSnapshot.ProjectilePositions.Remove(proj);
		}

		public static void ClearSnapshot()
		{
			currentSnapshot.PlayerPosition.Clear();
			currentSnapshot.ProjectilePositions.Clear();
		}

	}
}
