using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	GameManager Instance;

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

	public void SpawnPlayerLocal(int playerId,  string playerName, Vector3 pos, Quaternion rot)
	{
		var player = Instantiate(LocalPlayerPrefab, pos, rot);
		var playManComp = player.GetComponent<PlayerManager>();

		playManComp.PlayerId = playerId;
		playManComp.PlayerName = playerName;

		Players.Add(playerId, playManComp);
	}

	public void SpawnPlayerRemote(int playerId, string playerName, Vector3 pos, Quaternion rot)
	{
		var remotePlayer = Instantiate(RemotePlayerPrefab, pos, rot);
		var playManComp = remotePlayer.GetComponent<PlayerManager>();

		playManComp.PlayerId = playerId;
		playManComp.PlayerName = playerName;

		Players.Add(playerId, playManComp);
	}
}
