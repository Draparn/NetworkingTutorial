using NetworkTutorial.Server.Client;
using NetworkTutorial.Server.Net;
using NetworkTutorial.Shared;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Server.Gameplay
{
	public class GrenadeLauncherServer : MonoBehaviour
	{
		public static List<GrenadeLauncherServer> GrenadeLaunchers = new List<GrenadeLauncherServer>();
		public static byte NextGrenadeLauncherId = 0;

		public WeaponSlot WeaponSlot = WeaponSlot.GrenadeLauncher;
		public float RespawnTime = 60.0f, CurrentRespawnTime;
		public byte MyId, AmmoPickup = 8;
		public bool IsActive = true;

		private void Start()
		{
			MyId = NextGrenadeLauncherId;
			NextGrenadeLauncherId++;
			GrenadeLaunchers.Add(this);
		}

		private void Update()
		{
			if (CurrentRespawnTime > 0)
			{
				CurrentRespawnTime -= Time.deltaTime;
				if (CurrentRespawnTime <= 0)
				{
					IsActive = true;
					ServerSend.SendWeaponUpdate_ALL(MyId, IsActive);
				}
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player") && IsActive)
			{
				var playerComp = other.GetComponent<PlayerServer>();
				var weapon = playerComp.pickedUpWeapons[(int)WeaponSlot.GrenadeLauncher];

				if (weapon.IsPickedUp)
				{
					if (weapon.Ammo < weapon.MaxAmmo)
						weapon.Ammo = weapon.Ammo + AmmoPickup > weapon.MaxAmmo ? weapon.MaxAmmo : weapon.Ammo += AmmoPickup;
					else
						return;
				}
				else
				{
					playerComp.pickedUpWeapons[(int)WeaponSlot.GrenadeLauncher].IsPickedUp = true;
					playerComp.pickedUpWeapons[(int)WeaponSlot.GrenadeLauncher].Ammo = AmmoPickup;
				}

				IsActive = false;
				CurrentRespawnTime = RespawnTime;

				ServerSend.SendWeaponPickup_CLIENT(playerComp.PlayerId, WeaponSlot.GrenadeLauncher, weapon.IsPickedUp, weapon.Ammo);
				ServerSend.SendWeaponUpdate_ALL(MyId, IsActive);
			}
		}

	}
}
