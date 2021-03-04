using System.Collections.Generic;
using UnityEngine;

namespace SmallMultiplayerGame.Shared
{
	public enum WeaponSlot
	{
		BFG = 0,
		Pistol,
		GrenadeLauncher
	}

	public enum ProjectileType
	{
		Hitscan = 0,
		Grenade,
		Rocket
	}

	public class Weapons
	{
		public static List<Weapon> AllWeapons = new List<Weapon>()
		{
			new Weapon
			{
				ClientPrefab = (GameObject)Resources.Load("Prefabs/Client/Weapons/BFG"), //PLACEHOLDER NAME! CHANGE THIS!
				WeaponName = "BFG", //PLACEHOLDER NAME! CHANGE THIS!
				ProjectileType = ProjectileType.Hitscan,
				Ammo = 1,
				MaxAmmo = 1,
				Damage = byte.MaxValue
			},
			new Weapon
			{
				ClientPrefab = (GameObject)Resources.Load("Prefabs/Client/Weapons/Pistol"),
				WeaponName = "Pistol",
				ProjectileType = ProjectileType.Hitscan,
				Ammo = ushort.MaxValue,
				MaxAmmo = ushort.MaxValue,
				IsPickedUp = true,
				Damage = 15
			},
			new Weapon
			{
				ClientPrefab = (GameObject)Resources.Load("Prefabs/Client/Weapons/GrenadeLauncherClient"),
				ProjectilePrefabClient = (GameObject)Resources.Load("Prefabs/Client/Weapons/ClientGrenade"),
				ProjectilePrefabServer = (GameObject)Resources.Load("Prefabs/Server/ServerGrenade"),
				WeaponName = "GrenadeLauncher",
				ProjectileType = ProjectileType.Grenade,
				projExitVelocity = 600.0f,
				Damage = 40,
				MaxAmmo = 20
			}
		};

		public static List<Weapon> GetNewWeapons()
		{
			var tempList = new List<Weapon>();

			for (int i = 0; i < AllWeapons.Count; i++)
			{
				tempList.Add(new Weapon
				{
					ClientPrefab = AllWeapons[i].ClientPrefab,
					ProjectilePrefabClient = AllWeapons[i].ProjectilePrefabClient,
					ProjectilePrefabServer = AllWeapons[i].ProjectilePrefabServer,
					ProjectileType = AllWeapons[i].ProjectileType,
					WeaponName = AllWeapons[i].WeaponName,
					projExitVelocity = AllWeapons[i].projExitVelocity,
					Ammo = AllWeapons[i].Ammo,
					MaxAmmo = AllWeapons[i].MaxAmmo,
					IsPickedUp = AllWeapons[i].IsPickedUp,
					Damage = AllWeapons[i].Damage
				});
			}

			return tempList;
		}
	}

	public class Weapon
	{
		public GameObject ClientPrefab, ProjectilePrefabClient, ProjectilePrefabServer;

		public ProjectileType ProjectileType;

		public string WeaponName;
		public float projExitVelocity;
		public ushort Ammo;
		public ushort MaxAmmo;
		public bool IsPickedUp;
		public byte Damage;
	}
}