using UnityEngine;

namespace SmallMultiplayerGame.Server.Gameplay
{
	public class RespawnPoint : MonoBehaviour
	{
		void Start()
		{
			GameManagerServer.Instance.respawnPoints.Add(transform.position);
		}
	}
}