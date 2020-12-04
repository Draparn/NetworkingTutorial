using System.Collections;
using UnityEngine;

namespace NetworkTutorial.Client.Player
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

		public void SetHealth(int clientId, float newHealthValue)
		{
			if (clientId == Client.Instance.MyId)
				FlashUI(newHealthValue);

			if (newHealthValue < currentHealth)
			{
				if (!flickering)
					StartCoroutine(Flicker());
			}

			currentHealth = newHealthValue;

			if (currentHealth <= 0)
				Die();
		}

		private void FlashUI(float newHealthValue)
		{
			if (newHealthValue < currentHealth)
				UIManager.Instance.TakeDamage(newHealthValue <= 0);
			else if(newHealthValue > currentHealth)
				UIManager.Instance.HealDamage();

			UIManager.Instance.SetHealthText(newHealthValue);
			UIManager.Instance.SetHealthTextColor(newHealthValue);
		}

		private void Die()
		{
			rend.gameObject.SetActive(false);
		}

		public void Respawn(Vector3 position)
		{
			UIManager.Instance.Respawn();
			UIManager.Instance.SetHealthText(maxHealth);
			UIManager.Instance.SetHealthTextColor(maxHealth);
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
