using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkTutorial.Client
{
	public class UIManager : MonoBehaviour
	{
		public static UIManager Instance;

		public GameObject StartMenu;
		public GameObject CrossHair;

		public Image HurtScreen;
		private Color hurt = new Color(1, 0, 0, 0.2f);
		private Color dead = new Color(0.5f, 0, 0, 0.8f);

		public InputField ConnectToIPField;
		public InputField UserNameField;


		private void Awake()
		{
			if (Instance == null)
				Instance = this;
			else if (Instance != this)
				Destroy(this);

			ConnectToIPField.text = "127.0.0.1";
			UserNameField.text = "ClientName";
		}

		public void ConnectToServer()
		{
			StartMenu.SetActive(false);
			CrossHair.SetActive(true);
			ConnectToIPField.interactable = false;
			UserNameField.interactable = false;

			Client.Instance.ConnectToServer(ConnectToIPField.text);
		}

		public void TakeDamage(bool isDead)
		{
			if (!isDead)
				StartCoroutine(DamageTint());
			else
			{
				HurtScreen.color = dead;
				HurtScreen.enabled = true;
			}
		}

		private IEnumerator DamageTint()
		{
			HurtScreen.enabled = true;
			yield return new WaitForSeconds(0.5f);
			HurtScreen.enabled = false;
		}

		public void Respawn()
		{
			HurtScreen.color = hurt;
			HurtScreen.enabled = false;
		}
	}
}