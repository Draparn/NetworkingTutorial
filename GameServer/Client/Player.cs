using System;
using System.Collections.Generic;
using System.Numerics;

namespace GameServer
{
	public class Player
	{
		public int PlayerId;
		public string PlayerName;

		public Vector3 Position;
		public Quaternion Rotation;

		public Player(int id, string name, Vector3 pos)
		{
			PlayerId = id;
			PlayerName = name;
			Position = pos;
			Rotation = Quaternion.Identity;
		}
	}
}
