using NetworkTutorial.Client.Gameplay;
using NetworkTutorial.Client.Net;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkTutorial.Client.Player
{
	public class UIManager : MonoBehaviour
	{
		public static UIManager Instance;
		public Image[] WeaponsBar = new Image[2];
		public GameObject StartMenu, CrossHair, Connecting, InputFields, IngameMenu, ExitGame, Weapons;
		public Image HurtScreen;
		public InputField ConnectToIPField, UserNameField;
		public Text HealthText;

		private Color healed = new Color(0, 1, 0, 0.2f), hurt = new Color(1, 0, 0, 0.2f), dead = new Color(0.5f, 0, 0, 0.8f), dimmed = new Color(1, 1, 1, 0.15f), full = new Color(1, 1, 1, 1);

		public bool MenuIsActive = true;

		private void Awake()
		{
			if (Instance == null)
				Instance = this;
			else if (Instance != this)
				Destroy(this);

			ConnectToIPField.text = "127.0.0.1";
			UserNameField.text = "ClientName";
			HealthText.text = "100";

			StartMenu.SetActive(true);
			IngameMenu.SetActive(false);
		}

		//Menu
		public void ConnectToServer()
		{
			Connecting.SetActive(true);
			InputFields.SetActive(false);
			ExitGame.SetActive(false);

			ConnectToIPField.interactable = false;
			UserNameField.interactable = false;

			LocalClient.Instance.ConnectToServer(ConnectToIPField.text, UserNameField.text);
		}

		public void DisconnectFromServer()
		{
			ClientSend.SendDisconnect();
			LocalClient.Instance.Disconnect();
			GameObject.Destroy(GameManagerClient.Instance.Players[LocalClient.Instance.MyId].gameObject);
			GameManagerClient.Instance.Players.Remove(LocalClient.Instance.MyId);
			ShowMainMenu();
		}

		public void ShowMainMenu()
		{
			MenuIsActive = true;
			GameOff();
			Connecting.SetActive(false);
			InputFields.SetActive(true);

			ConnectToIPField.interactable = true;
			UserNameField.interactable = true;

			LocalClient.Instance.ResetNameAndId();
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		public void EscapePressed()
		{
			if (LocalClient.Instance.isConnected)
			{
				IngameMenu.SetActive(!IngameMenu.activeInHierarchy);
				MenuIsActive = IngameMenu.activeInHierarchy;
				Cursor.lockState = IngameMenu.activeInHierarchy ? CursorLockMode.None : CursorLockMode.Locked;
				Cursor.visible = IngameMenu.activeInHierarchy ? true : false;
			}
		}

		public void GameOn()
		{
			Connecting.SetActive(false);
			InputFields.SetActive(true);
			StartMenu.SetActive(false);
			ExitGame.SetActive(false);
			CrossHair.SetActive(true);
			Weapons.SetActive(true);
			HealthText.transform.parent.gameObject.SetActive(true);
			MenuIsActive = false;
		}

		private void GameOff()
		{
			StartMenu.SetActive(true);
			ExitGame.SetActive(true);
			IngameMenu.SetActive(false);
			CrossHair.SetActive(false);
			Weapons.SetActive(false);
			HealthText.transform.parent.gameObject.SetActive(false);
			MenuIsActive = true;
		}

		public void QuitGame()
		{
			if (LocalClient.Instance.isConnected)
				DisconnectFromServer();

#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
			Application.Quit();
		}

		//In-game
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

		public void PickedUpNewWeapon(byte weaponSlot)
		{
			WeaponsBar[weaponSlot - 1].color = full;
		}

		public void Respawn()
		{
			HurtScreen.color = hurt;
			HurtScreen.enabled = false;

			for (int i = 1; i < WeaponsBar.Length; i++)
				WeaponsBar[i].color = dimmed;
		}

	}
}