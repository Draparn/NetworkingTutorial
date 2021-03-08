using UnityEngine;

namespace SmallMultiplayerGame.Client.Gameplay.Player
{
	public class CameraController : MonoBehaviour
	{
		public PlayerObjectClient player;

		public float sensitivity = 200f;
		private float clampAngle = 85f, verticalRotation, horizontalRotation, mouseVertical, mouseHorizontal;

		private void Start()
		{
			verticalRotation = transform.localEulerAngles.x;
			horizontalRotation = player.transform.eulerAngles.y;
			Cursor.lockState = CursorLockMode.Locked;
		}

		private void Update()
		{
			if (player.currentHealth > 0 && !UIManager.Instance.MenuIsActive)
				Look();
		}

		private void Look()
		{
			mouseVertical = -Input.GetAxis("Mouse Y");
			mouseHorizontal = Input.GetAxis("Mouse X");

			verticalRotation += mouseVertical * sensitivity * Time.deltaTime;
			horizontalRotation += mouseHorizontal * sensitivity * Time.deltaTime;

			verticalRotation = Mathf.Clamp(verticalRotation, -clampAngle, clampAngle);

			transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
			player.transform.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);
		}
	}
}