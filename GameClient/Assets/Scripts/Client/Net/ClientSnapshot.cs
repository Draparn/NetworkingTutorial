using System;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Client.Net
{
	public class ClientSnapshot
	{
		public static List<ClientSnapshot> Snapshots = new List<ClientSnapshot>();

		internal List<Tuple<int, Vector3>> players = new List<Tuple<int, Vector3>>();
		internal List<Tuple<int, Vector3>> projectiles = new List<Tuple<int, Vector3>>();

		public ClientSnapshot(List<Tuple<int, Vector3>> players, List<Tuple<int, Vector3>> projectiles)
		{
			this.players = players;
			this.projectiles = projectiles;
		}
	}
}
