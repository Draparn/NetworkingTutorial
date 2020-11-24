using UnityEngine;

namespace NetworkTutorial.Client
{
	public class PlayerManager : MonoBehaviour
	{
		public GameObject PlayerMesh;

		private string PlayerName;

		private float currentHealth;
		public float maxHealth = 100.0f;

		private int PlayerId;


		private void Awake()
		{
			var menuCamera = GameObject.FindGameObjectWithTag("MainCamera");
			if (menuCamera != null)
				Destroy(menuCamera);
		}

		public void Init(int id, string playerName)
		{
			PlayerId = id;
			PlayerName = playerName;
			currentHealth = maxHealth;
		}

		public void SetHealth(float healthValue)
		{
			currentHealth = healthValue;

			if (healthValue <= 0)
				Die();
		}

		private void Die()
		{
			PlayerMesh.SetActive(false);
		}

		public void Respawn(Vector3 position)
		{
			transform.position = position;
			PlayerMesh.SetActive(true);
			currentHealth = maxHealth;
		}

	}
}
