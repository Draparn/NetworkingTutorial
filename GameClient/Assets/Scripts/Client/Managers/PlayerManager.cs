using System.Collections;
using UnityEngine;

namespace NetworkTutorial.Client
{
	public class PlayerManager : MonoBehaviour
	{
		public GameObject PlayerMesh;
		private MeshRenderer rend;
		private Color originalColor;

		private string PlayerName;

		public float currentHealth;
		public float maxHealth = 100.0f;

		private int PlayerId;

		private bool flickering = false;

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
			rend = PlayerMesh.GetComponent<MeshRenderer>();
			originalColor = rend.material.color;
		}

		public void SetHealth(int clientId, float healthValue)
		{
			if (healthValue < currentHealth)
			{
				if (!flickering)
					StartCoroutine(Flicker());

				if (PlayerId == clientId)
					UIManager.Instance.TakeDamage(healthValue <= 0);
			}

			currentHealth = healthValue;

			if (currentHealth <= 0)
				Die();
			
		}

		private void Die()
		{
			rend.gameObject.SetActive(false);
		}

		public void Respawn(Vector3 position)
		{
			UIManager.Instance.Respawn();
			transform.position = position;
			currentHealth = maxHealth;
			rend.gameObject.SetActive(true);
		}

		private IEnumerator Flicker()
		{
			flickering = true;

			byte counter = 0;
			do
			{
				rend.material.color = Color.red;
				yield return new WaitForSeconds(0.1f);
				rend.material.color = originalColor;
				yield return new WaitForSeconds(0.1f);

				counter++;
			} while (counter < 2);

			flickering = false;
		}

	}
}
