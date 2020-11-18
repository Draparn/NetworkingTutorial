using UnityEngine;

public class PlayerManager : MonoBehaviour
{
	public int PlayerId;
	public string PlayerName;

	private void Awake()
	{
		var menuCamera = GameObject.FindGameObjectWithTag("MainCamera");
		if (menuCamera != null)
			Destroy(menuCamera);
	}
}
