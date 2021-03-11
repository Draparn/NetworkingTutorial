using SmallMultiplayerGame.Server.Client;
using SmallMultiplayerGame.Server.Gameplay;
using SmallMultiplayerGame.Shared;
using SmallMultiplayerGame.Shared.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace SmallMultiplayerGame.Server.Net
{
	public struct PlayerPosData
	{
		public Vector3 Position;
		public Quaternion Rotation;

		public uint SequenceNumber;
		public byte Id;

		public PlayerPosData(byte id, Vector3 pos, Quaternion rot, uint sequenceNumber)
		{
			Id = id;
			Position = pos;
			Rotation = rot;
			SequenceNumber = sequenceNumber;
		}
	}

	public class ServerSnapshot
	{
		public static List<ServerSnapshot> OldSnapshots = new List<ServerSnapshot>();
		public static ServerSnapshot currentSnapshot = new ServerSnapshot();

		public ServerSnapshot() { }
		public ServerSnapshot(uint sequenceNumber, Dictionary<byte, PlayerPosData> PlayerPositions, List<ProjectileServer> ProjectilePositions)
		{
			SequenceNumber = sequenceNumber;
			this.PlayerPositions = new Dictionary<byte, PlayerPosData>(PlayerPositions);
			this.ProjectilePositions = new List<ProjectileServer>(ProjectilePositions);
		}

		public Dictionary<byte, PlayerPosData> PlayerPositions = new Dictionary<byte, PlayerPosData>();
		public List<ProjectileServer> ProjectilePositions = new List<ProjectileServer>();

		public byte? lerpValue;
		public uint SequenceNumber;

		public static void AddPlayerMovement(byte id, Vector3 pos, Quaternion rot, uint sequenceNumber)
		{
			if (currentSnapshot.PlayerPositions.ContainsKey(id))
				currentSnapshot.PlayerPositions[id] = new PlayerPosData(id, pos, rot, sequenceNumber);
			else
				currentSnapshot.PlayerPositions.Add(id, new PlayerPosData(id, pos, rot, sequenceNumber));
		}
		public static void RemovePlayerMovement(PlayerObjectServer player)
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

		public static void AddElevatorMovement(float lerpValue)
		{
			if (lerpValue <= 0 || lerpValue >= 1)
				currentSnapshot.lerpValue = null;
			else
				currentSnapshot.lerpValue = ValueTypeConversions.ReturnDecimalsAsByte(lerpValue);
		}

		public static ServerSnapshot GetOldSnapshot(uint sequenceNumber)
		{
			for (int i = 0; i < OldSnapshots.Count; i++)
			{
				if (OldSnapshots[i].SequenceNumber == sequenceNumber)
					return OldSnapshots[i];
			}

			//old snapshot wasn't found, returning the oldest one
			return OldSnapshots[0];
		}

		public static void ClearSnapshot()
		{
			OldSnapshots.Add(new ServerSnapshot(currentSnapshot.SequenceNumber, currentSnapshot.PlayerPositions, currentSnapshot.ProjectilePositions));

			if (OldSnapshots.Count > 1 / ConstantValues.SERVER_TICK_RATE) //One second's worth of snapshots
				OldSnapshots.RemoveAt(0);

			currentSnapshot.PlayerPositions.Clear();
			currentSnapshot.ProjectilePositions.Clear();
			currentSnapshot.SequenceNumber++;
		}

	}
}
