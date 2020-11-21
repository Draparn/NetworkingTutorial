using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public static UIManager Instance;

	public GameObject StartMenu;

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
		ConnectToIPField.interactable = false;
		UserNameField.interactable = false;

		Client.Instance.ConnectToServer(ConnectToIPField.text);
	}
}