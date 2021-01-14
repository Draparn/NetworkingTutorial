using NetworkTutorial.Client.Net;
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
		public GameObject Connecting;
		public GameObject InputFields;

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
			Connecting.SetActive(true);
			InputFields.SetActive(false);

			ConnectToIPField.interactable = false;
			UserNameField.interactable = false;

			LocalClient.Instance.ConnectToServer(ConnectToIPField.text, UserNameField.text);
		}

		public void ConnectionTimedOut()
		{
			GameOff();
			Connecting.SetActive(false);
			InputFields.SetActive(true);

			ConnectToIPField.interactable = true;
			UserNameField.interactable = true;

			LocalClient.Instance.MyId = 0;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		public void GameOn()
		{
			Connecting.SetActive(false);
			InputFields.SetActive(true);
			StartMenu.SetActive(false);

			HealthText.transform.parent.gameObject.SetActive(true);
			CrossHair.SetActive(true);
		}

		public void GameOff()
		{
			StartMenu.SetActive(true);
			HealthText.transform.parent.gameObject.SetActive(false);
			CrossHair.SetActive(false);
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