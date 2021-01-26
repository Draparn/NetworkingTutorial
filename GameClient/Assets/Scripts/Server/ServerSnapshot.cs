using NetworkTutorial.Server.Client;
using NetworkTutorial.Server.Gameplay;
using NetworkTutorial.Shared;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Server
{
	public struct PlayerPosData
	{
		public byte Id;
		public Vector3 Position;
		public Quaternion Rotation;
		public ushort SequenceNumber;

		public PlayerPosData(byte id, Vector3 pos, Quaternion rot, ushort sequenceNumber)
		{
			Id = id;
			Position = pos;
			Rotation = rot;
			SequenceNumber = sequenceNumber;
		}
	}

	public class ServerSnapshot
	{
		public ServerSnapshot(ServerSnapshot snapshot = null)
		{
			if (snapshot != null)
			{
				SequenceNumber = snapshot.SequenceNumber;
				PlayerPositions = snapshot.PlayerPositions;
				ProjectilePositions = snapshot.ProjectilePositions;
			}
		}

		public static List<ServerSnapshot> OldSnapshots = new List<ServerSnapshot>();
		public static ServerSnapshot currentSnapshot = new ServerSnapshot();

		public Dictionary<byte, PlayerPosData> PlayerPositions = new Dictionary<byte, PlayerPosData>();
		public List<ProjectileServer> ProjectilePositions = new List<ProjectileServer>();

		public uint SequenceNumber;

		public static void AddPlayerMovement(byte id, Vector3 pos, Quaternion rot, ushort sequenceNumber)
		{
			if (currentSnapshot.PlayerPositions.ContainsKey(id))
				currentSnapshot.PlayerPositions[id] = new PlayerPosData(id, pos, rot, sequenceNumber);
			else
				currentSnapshot.PlayerPositions.Add(id, new PlayerPosData(id, pos, rot, sequenceNumber));
		}
		public static void RemovePlayerMovement(PlayerServer player)
		{
			currentSnapshot.PlayerPositions.Remove(player.PlayerId);
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

		public static ServerSnapshot GetOldSnapshot(uint sequenceNumber)
		{
			for (int i = 0; i < OldSnapshots.Count; i++)
			{
				if (OldSnapshots[i].SequenceNumber == sequenceNumber)
					return OldSnapshots[i];
			}

			return OldSnapshots[0];
		}

		public static void ClearSnapshot()
		{
			OldSnapshots.Add(new ServerSnapshot(currentSnapshot));

			if (OldSnapshots.Count > 1 / ConstantValues.SERVER_TICK_RATE) //One second's worth of snapshots
				OldSnapshots.RemoveAt(0);

			currentSnapshot.PlayerPositions.Clear();
			currentSnapshot.ProjectilePositions.Clear();
			currentSnapshot.SequenceNumber++;
		}

	}
}
