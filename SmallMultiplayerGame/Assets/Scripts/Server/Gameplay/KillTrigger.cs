using SmallMultiplayerGame.Server.Client;
using UnityEngine;

namespace SmallMultiplayerGame.Server.Gameplay
{
	public class KillTrigger : MonoBehaviour
	{
		private void OnTriggerEnter(Collider other)
		{
			other.GetComponent<PlayerServer>()?.TakeDamage(Mathf.Infinity);
		}
	}

}
