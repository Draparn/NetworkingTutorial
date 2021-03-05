using SmallMultiplayerGame.Server.Client;
using SmallMultiplayerGame.Server.Net;
using UnityEngine;

namespace SmallMultiplayerGame.Server.Gameplay.Pickups
{
	public enum Size
	{
		Small = 0,
		Big,
		Mega
	}

	public class HealthpackServer : MonoBehaviour
	{
		public Size size;

		public float HealthGain, RespawnTime, currentRespawnTime;
		public byte MyId;
		public bool IsActive = true;

		private void Start()
		{
			GameManagerServer.Instance.AddHealthpackToDict(this);
			RespawnTime = (int)size + 1 * 20;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player") && IsActive)
			{
				var playerComp = other.GetComponent<PlayerObjectServer>();
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
				var playerComp = col.GetComponent<PlayerObjectServer>();
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
