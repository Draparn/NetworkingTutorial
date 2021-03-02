using SmallMultiplayerGame.Server.Client;
using SmallMultiplayerGame.Server.Net;
using SmallMultiplayerGame.Shared;
using System.Collections.Generic;
using UnityEngine;

namespace SmallMultiplayerGame.Server.Gameplay
{
	public class ProjectileServer : MonoBehaviour
	{
		public static Dictionary<int, ProjectileServer> Projectiles = new Dictionary<int, ProjectileServer>();
		private Rigidbody rb;

		private Vector3 initialForce;

		private static ushort nextProjectileId = 1;
		public ushort id;
		private float fuseTimer = 1, explosionRadius = 2.5f;
		public byte shotFromWeapon;
		private byte shotByPlayer;

		private void Start()
		{
			bool useNewId = true;
			for (ushort i = 1; i <= nextProjectileId; i++)
			{
				if (!Projectiles.ContainsKey(i) && i < nextProjectileId)
				{
					id = i;
					Projectiles.Add(i, this);
					useNewId = false;
					break;
				}
			}

			if (useNewId)
			{
				id = nextProjectileId;
				nextProjectileId++;
				Projectiles.Add(id, this);
			}

			ServerSend.SendProjectileSpawn_ALL(this);

			rb = GetComponent<Rigidbody>();
			rb.AddForce(initialForce);

			Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), Server.Clients[shotByPlayer].Player.GetComponent<CharacterController>());

			Invoke(nameof(Explode), fuseTimer);
		}

		private void Update()
		{
			ServerSnapshot.AddProjectileMovement(this);
		}

		private void OnCollisionEnter(Collision other)
		{
			var playerComp = other.transform.GetComponent<PlayerServer>();

			if (playerComp != null && playerComp.PlayerId != shotByPlayer && playerComp.CurrentHealth > 0)
				Explode();
		}

		public void Init(Vector3 viewDirection, Weapon shotFromWeapon, byte shotByPlayer)
		{
			for (int i = 0; i < Weapons.AllWeapons.Count; i++)
			{
				if (Weapons.AllWeapons[i].WeaponName == shotFromWeapon.WeaponName)
				{
					this.shotFromWeapon = (byte)i;
					break;
				}
			}
			this.shotByPlayer = shotByPlayer;
			initialForce = viewDirection * shotFromWeapon.projExitVelocity;
		}

		private void Explode()
		{
			ServerSnapshot.RemoveProjectileMovement(this);
			ServerSend.SendProjectileExplosion_ALL(this);

			var nearbyColliders = Physics.OverlapSphere(transform.position, explosionRadius);
			foreach (var collider in nearbyColliders)
			{
				if (collider.CompareTag("Player"))
					collider.GetComponent<PlayerServer>().TakeDamage(Weapons.AllWeapons[shotFromWeapon].Damage);
			}

			Projectiles.Remove(id);
			Destroy(gameObject);
		}
	}
}
