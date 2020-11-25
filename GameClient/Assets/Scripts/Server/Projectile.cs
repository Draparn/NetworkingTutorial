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

		private ushort id;
		private Rigidbody rb;
		private Vector3 initialForce;
		private float fuseTimer = 2;
		private float explosionRadius = 1.5f;
		private float explosionDamage = 25.0f;
		private int thrownByPlayer;

		private void Start()
		{
			id = nextProjectileId;
			nextProjectileId++;
			Projectiles.Add(id, this);

			rb = GetComponent<Rigidbody>();
			rb.AddForce(initialForce);

			Invoke(nameof(Explode), fuseTimer);
		}

		private void FixedUpdate()
		{

		}

		public void Init(Vector3 viewDirection, float initialforceStrength, int thrownByPlayer)
		{
			initialForce = viewDirection * initialforceStrength;
			this.thrownByPlayer = thrownByPlayer;
		}

		private void Explode()
		{
			var nearbyColliders = Physics.OverlapSphere(transform.position, explosionRadius);
			foreach (var collider in nearbyColliders)
			{
				if (collider.CompareTag("Player"))
					collider.GetComponent<Player>().TakeDamage(explosionDamage);
			}

			Destroy(gameObject);
		}
	}
}
