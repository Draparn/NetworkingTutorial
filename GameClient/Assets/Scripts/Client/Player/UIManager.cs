using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkTutorial.Client.Player
{
	public class UIManager : MonoBehaviour
	{
		public static UIManager Instance;

		public GameObject StartMenu;
		public GameObject CrossHair;

		public Image HurtScreen;
		private Color healed = new Color(0, 1, 0, 0.2f);
		private Color hurt = new Color(1, 0, 0, 0.2f);
		private Color dead = new Color(0.5f, 0, 0, 0.8f);

		public InputField ConnectToIPField;
		public InputField UserNameField;

		public Text HealthText;


		private void Awake()
		{
			if (Instance == null)
				Instance = this;
			else if (Instance != this)
				Destroy(this);

			ConnectToIPField.text = "127.0.0.1";
			UserNameField.text = "ClientName";
			HealthText.text = "100";
		}

		public void ConnectToServer()
		{
			StartMenu.SetActive(false);
			CrossHair.SetActive(true);
			HealthText.transform.parent.gameObject.SetActive(true);

			ConnectToIPField.interactable = false;
			UserNameField.interactable = false;

			Client.Instance.ConnectToServer(ConnectToIPField.text);
		}

		public void ConnectionTimedOut()
		{
			StartMenu.SetActive(true);
			CrossHair.SetActive(false);
			HealthText.transform.parent.gameObject.SetActive(false);

			ConnectToIPField.interactable = true;
			UserNameField.interactable = true;
		}

		public void TakeDamage(bool isDead)
		{
			if (!isDead)
				StartCoroutine(DamageTint(hurt));
			else
			{
				StopAllCoroutines();
				HurtScreen.color = dead;
				HurtScreen.enabled = true;
			}
		}
		public void HealDamage()
		{
			StartCoroutine(DamageTint(healed));
		}

		public void SetHealthText(float healthValue)
		{
			HealthText.text = healthValue.ToString();
		}

		public void SetHealthTextColor(float healthValue)
		{
			Color.RGBToHSV(HealthText.color, out _, out float S, out float V);

			HealthText.color = Color.HSVToRGB(healthValue / 360, S, V);
		}

		private IEnumerator DamageTint(Color color)
		{
			HurtScreen.color = color;
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