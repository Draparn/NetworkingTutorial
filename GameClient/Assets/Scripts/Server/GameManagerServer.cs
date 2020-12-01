using NetworkTutorial.Server.Net;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerServer : MonoBehaviour
{
	public static Dictionary<byte, HealthpackServer> healthpacks = new Dictionary<byte, HealthpackServer>();

	private static byte nextHealthpackId = 0;

	private void Update()
	{
		foreach (var healthpack in healthpacks.Values)
		{
			if (healthpack.IsActive)
				continue;

			healthpack.currentRespawnTime -= Time.deltaTime;
			if (healthpack.currentRespawnTime <= 0)
				ActivateHealthpack(healthpack);
		}
	}

	public static void AddHealthpackToDict(HealthpackServer hps)
	{
		hps.MyId = nextHealthpackId;
		healthpacks.Add(nextHealthpackId, hps);
		nextHealthpackId++;
	}
	public static void DeactivateHealthpack(byte id)
	{
		healthpacks[id].IsActive = false;
		ServerSend.SendHealthpackDeactivate_TCP_ALL(id);
	}
	private static void ActivateHealthpack(HealthpackServer hp)
	{
		hp.IsActive = true;
		ServerSend.SendHealthpackActive_TCP_ALL(hp.MyId);
	}

}
