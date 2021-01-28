using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Shared
{
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
				Damage = byte.MaxValue
			},
			new Weapon
			{
				ClientPrefab = (GameObject)Resources.Load("Prefabs/Client/Weapons/Pistol"),
				WeaponName = "Pistol",
				ProjectileType = ProjectileType.Hitscan,
				Ammo = ushort.MaxValue,
				IsPickedUp = true,
				Damage = 15
			},
			new Weapon
			{
				ClientPrefab = (GameObject)Resources.Load("Prefabs/Client/Weapons/GrenadeLauncher"),
				ProjectilePrefabClient = (GameObject)Resources.Load("Prefabs/Client/Weapons/ClientGrenade"),
				ProjectilePrefabServer = (GameObject)Resources.Load("Prefabs/Server/ServerGrenade"),
				WeaponName = "GrenadeLauncher",
				ProjectileType = ProjectileType.Grenade,
				projExitVelocity = 600.0f,
				Damage = 40
			}
		};

		public static List<Weapon> GetNewWeapons()
		{
			var tempList = new List<Weapon>();

			for (int i = 0; i < AllWeapons.Count; i++)
			{
				tempList.Add(new Weapon
				{
					Ammo = AllWeapons[i].Ammo,
					ClientPrefab = AllWeapons[i].ClientPrefab,
					Damage = AllWeapons[i].Damage,
					IsPickedUp = AllWeapons[i].IsPickedUp,
					ProjectilePrefabClient = AllWeapons[i].ProjectilePrefabClient,
					ProjectilePrefabServer = AllWeapons[i].ProjectilePrefabServer,
					ProjectileType = AllWeapons[i].ProjectileType,
					projExitVelocity = AllWeapons[i].projExitVelocity,
					WeaponName = AllWeapons[i].WeaponName
				});
			}

			return tempList;
		}
	}
}

public class Weapon
{
	public GameObject ClientPrefab, ProjectilePrefabClient, ProjectilePrefabServer;

	public ProjectileType ProjectileType;

	public string WeaponName;
	public float projExitVelocity;
	public ushort Ammo;
	public bool IsPickedUp;
	public byte Damage;
}

public enum ProjectileType
{
	Hitscan = 0,
	Grenade,
	Rocket
}