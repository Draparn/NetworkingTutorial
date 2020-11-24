using System.Collections.Generic;
using UnityEngine;


namespace NetworkTutorial.Client
{
	public class GameManager : MonoBehaviour
	{
		public static GameManager Instance;

		public Dictionary<int, PlayerManager> Players = new Dictionary<int, PlayerManager>();

		public GameObject LocalPlayerPrefab;
		public GameObject RemotePlayerPrefab;

		private void Awake()
		{
			if (Instance == null)
				Instance = this;
			else if (Instance != this)
				Destroy(this);
		}

		public void SpawnPlayer(bool isLocal, int playerId, string playerName, Vector3 pos, Quaternion rot)
		{
			var player = Instantiate(isLocal ? LocalPlayerPrefab : RemotePlayerPrefab, pos, rot);
			var playManComp = player.GetComponent<PlayerManager>();

			playManComp.Init(playerId, playerName);

			Players.Add(playerId, playManComp);
		}

	}
}
