using SmallMultiplayerGame.Server.Client;
using UnityEngine;

namespace SmallMultiplayerGame.Server.Gameplay.Environment
{
	public class KillTrigger : MonoBehaviour
	{
		private void OnTriggerEnter(Collider other)
		{
			other.GetComponent<PlayerObjectServer>()?.TakeDamage(Mathf.Infinity);
		}
	}

}
