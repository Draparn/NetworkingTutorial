using UnityEngine;

namespace NetworkTutorial.Server.Gameplay
{
	public class RespawnPoint : MonoBehaviour
	{
		void Start()
		{
			GameManagerServer.Instance.respawnPoints.Add(transform.position);
		}
	}
}