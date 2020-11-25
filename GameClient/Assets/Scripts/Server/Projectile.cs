using NetworkTutorial.Server.Client;
using NetworkTutorial.Server.Net;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Server
{
	public class Projectile : MonoBehaviour
	{
		public static Dictionary<ushort, Projectile> Projectiles = new Dictionary<ushort, Projectile>();
		private static ushort nextProjectileId = 1;

		public ushort id;
		private Rigidbody rb;
		private Vector3 initialForce;
		public int thrownByPlayer;
		private float fuseTimer = 1;
		private float explosionRadius = 1.5f;
		private float explosionDamage = 25.0f;

		private void Start()
		{
			bool useNewId = true;
			for (ushort i = 1; i <= nextProjectileId; i++)
			{
				if (!Projectiles.ContainsKey(i))
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

			ServerSend.SendProjectileSpawn_TCP(this);

			rb = GetComponent<Rigidbody>();
			rb.AddForce(initialForce);

			Invoke(nameof(Explode), fuseTimer);
		}

		private void FixedUpdate()
		{
			ServerSend.SendProjectileUpdatePosition_UDP(this);
		}
		private void OnCollisionEnter(Collision other)
		{
			if (other.transform.CompareTag("Player") && other.transform.GetComponent<Player>().CurrentHealth > 0)
				Explode();
		}

		public void Init(Vector3 viewDirection, float initialforceStrength, int thrownByPlayer)
		{
			initialForce = viewDirection * initialforceStrength;
			this.thrownByPlayer = thrownByPlayer;
		}

		private void Explode()
		{
			ServerSend.SendProjectileExplosion_TCP(this);

			var nearbyColliders = Physics.OverlapSphere(transform.position, explosionRadius);
			foreach (var collider in nearbyColliders)
			{
				if (collider.CompareTag("Player"))
					collider.GetComponent<Player>().TakeDamage(explosionDamage);
			}

			Projectiles.Remove(id);
			Destroy(gameObject);
		}
	}
}
