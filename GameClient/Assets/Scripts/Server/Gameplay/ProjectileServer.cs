using NetworkTutorial.Server.Client;
using NetworkTutorial.Server.Net;
using NetworkTutorial.Shared;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Server.Gameplay
{
	public class ProjectileServer : MonoBehaviour
	{
		public static Dictionary<int, ProjectileServer> Projectiles = new Dictionary<int, ProjectileServer>();
		private static ushort nextProjectileId = 1;

		[HideInInspector] public ushort id;

		private Rigidbody rb;

		private Vector3 initialForce;

		public byte shotFromWeapon;
		private byte shotByPlayer;

		[SerializeField] private float fuseTimer = 1;
		[SerializeField] private float explosionRadius = 2.5f;
		[SerializeField] private float explosionDamage = 15.0f;

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

			Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), Server.Clients[shotByPlayer].PlayerObject.GetComponent<CharacterController>());

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
			this.shotFromWeapon = (byte)Weapons.AllWeapons.IndexOf(shotFromWeapon);
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
					collider.GetComponent<PlayerServer>().TakeDamage(explosionDamage);
			}

			Projectiles.Remove(id);
			Destroy(gameObject);
		}
	}
}
