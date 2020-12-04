using NetworkTutorial.Server.Client;
using NetworkTutorial.Server.Net;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Server
{
	public class Projectile : MonoBehaviour
	{
		public static Dictionary<int, Projectile> Projectiles = new Dictionary<int, Projectile>();
		private static ushort nextProjectileId = 1;

		[HideInInspector] public int id;

		private Rigidbody rb;

		private Vector3 initialForce;

		private int thrownByPlayer;

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

			ServerSend.SendProjectileSpawn_TCP_ALL(this);

			rb = GetComponent<Rigidbody>();
			rb.AddForce(initialForce);

			Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), Server.Clients[thrownByPlayer].player.GetComponent<CharacterController>());

			Invoke(nameof(Explode), fuseTimer);
		}

		private void FixedUpdate()
		{
			ServerSnapshot.AddProjectileMovement(this);
			//ServerSend.SendProjectileUpdatePosition_UDP_ALL(this);
		}

		private void OnCollisionEnter(Collision other)
		{
			var playerComp = other.transform.GetComponent<Player>();

			if (playerComp != null && playerComp.PlayerId != thrownByPlayer && playerComp.CurrentHealth > 0)
				Explode();
		}

		public void Init(Vector3 viewDirection, float initialforceStrength, int thrownByPlayer)
		{
			initialForce = viewDirection * initialforceStrength;
			this.thrownByPlayer = thrownByPlayer;
		}

		private void Explode()
		{
			ServerSnapshot.RemoveProjectileMovement(this);
			ServerSend.SendProjectileExplosion_TCP_ALL(this);

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
