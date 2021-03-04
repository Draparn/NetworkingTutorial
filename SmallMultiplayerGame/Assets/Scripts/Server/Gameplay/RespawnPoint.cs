using UnityEngine;

namespace SmallMultiplayerGame.Server.Gameplay
{
	public class RespawnPoint : MonoBehaviour
	{
		void Awake()
		{
			GameManagerServer.Instance.respawnPoints.Add(transform.position);
		}
	}
}