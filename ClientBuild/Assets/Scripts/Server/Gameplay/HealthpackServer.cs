using NetworkTutorial.Server.Client;
using NetworkTutorial.Server.Net;
using UnityEngine;

namespace NetworkTutorial.Server.Gameplay
{
	public enum Size
	{
		Small,
		Big,
		Mega
	}

	public class HealthpackServer : MonoBehaviour
	{
		public float HealthGain = 50.0f, RespawnTime = 30.0f, currentRespawnTime;
		public byte MyId;
		public Size size;
		public bool IsActive = true;

		private void Start()
		{
			GameManagerServer.Instance.AddHealthpackToDict(this);
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player") && IsActive)
			{
				var playerComp = other.GetComponent<PlayerServer>();
				if (size != Size.Mega && playerComp.CurrentHealth >= playerComp.MaxHealth)
					return;

				if (size == Size.Mega)
					playerComp.CurrentHealth = playerComp.MaxHealth * 2;
				else
					playerComp.HealDamage(HealthGain);

				ServerSend.SendPlayerHealthUpdate_ALL(playerComp);

				currentRespawnTime = RespawnTime;
				GameManagerServer.Instance.DeactivateHealthpack(MyId);
			}
		}

		public bool RespawnCollisionCheck()
		{
			var overlappingColliders = Physics.OverlapSphere(transform.position, 1);
			foreach (var col in overlappingColliders)
			{
				var playerComp = col.GetComponent<PlayerServer>();
				if (playerComp != null && playerComp.CurrentHealth < playerComp.MaxHealth)
				{
					playerComp.HealDamage(HealthGain);
					ServerSend.SendPlayerHealthUpdate_ALL(playerComp);

					currentRespawnTime = RespawnTime;
					return true;
				}
			}

			return false;
		}
	}
}
