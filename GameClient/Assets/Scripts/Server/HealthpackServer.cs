using NetworkTutorial.Server.Client;
using NetworkTutorial.Server.Net;
using UnityEngine;

public class HealthpackServer : MonoBehaviour
{
	public float HealthGain = 50.0f;
	public float RespawnTime = 30.0f;
	[HideInInspector] public float currentRespawnTime;

	[HideInInspector] public byte MyId;

	[HideInInspector] public bool IsActive = true;

	private void Start()
	{
		GameManagerServer.AddHealthpackToDict(this);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") && IsActive)
		{
			var playerComp = other.GetComponent<Player>();
			if (playerComp.CurrentHealth == playerComp.MaxHealth)
				return;

			playerComp.HealDamage(HealthGain);
			ServerSend.SendPlayerHealthUpdate_TCP_ALL(playerComp);

			currentRespawnTime = RespawnTime;
			GameManagerServer.DeactivateHealthpack(MyId);
		}
	}
}
