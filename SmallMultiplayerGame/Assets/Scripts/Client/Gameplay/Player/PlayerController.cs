using SmallMultiplayerGame.Client.Net;
using SmallMultiplayerGame.Shared;
using SmallMultiplayerGame.Shared.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace SmallMultiplayerGame.Client.Gameplay.Player
{
	public class PlayerController : MonoBehaviour
	{
		public static PlayerController Instance;

		public List<Weapon> pickedUpWeapons;
		private Transform cameraTransform;
		private CharacterController controller;
		private PlayerObjectClient player;

		private InputsStruct inputs;
		private Vector3 prevPos, nextPos, currentVelocity, previousVelocity;

		private float yVelocity, yVelocityPreMove, clientTickRate;
		private uint sequenceNumber = 0;
		private byte? PressedWeaponKey = null;
		public byte currentWeapon;
		private bool isGroundedPreMove;

		private void Awake()
		{
			if (Instance == null)
				Instance = this;
			else if (Instance != this)
				Destroy(this);
		}

		private void Start()
		{
			cameraTransform = GetComponentInChildren<CameraController>().transform;
			player = GetComponent<PlayerObjectClient>();
			controller = GetComponent<CharacterController>();
			inputs = new InputsStruct();
			pickedUpWeapons = Weapons.GetNewWeapons();
			prevPos = gameObject.transform.position;
			nextPos = gameObject.transform.position;
			currentWeapon = 1;
		}

		private void Update()
		{
			if (player.currentHealth <= 0)
				return;

			//Switched weapon
			PressedWeaponKey = GetPressedWeaponKey();
			if (PressedWeaponKey.HasValue && PressedWeaponKey != currentWeapon && HasWeapon())
			{
				ClientSend.SendWeaponSwitch((byte)PressedWeaponKey);
				currentWeapon = (byte)PressedWeaponKey;
			}

			//Primary Fire
			if (Input.GetKeyDown(KeyCode.Mouse0) && !UIManager.Instance.MenuIsActive)
			{
				if (pickedUpWeapons[currentWeapon].Ammo <= 0)
				{
					ClientSend.SendWeaponSwitch((byte)WeaponSlot.Pistol);
					currentWeapon = (byte)WeaponSlot.Pistol;
					return;
				}

				ClientSend.SendPlayerPrimaryFire(cameraTransform.forward);
				player.FireWeapon();

				if (currentWeapon != (byte)WeaponSlot.Pistol)
					UIManager.Instance.SetAmmoCount(pickedUpWeapons[currentWeapon].Ammo.ToString());
			}

			if (Input.GetButtonDown("Cancel"))
				UIManager.Instance.EscapePressed();

			//Inputs
			inputs.Forward = Input.GetKey(KeyCode.W);
			inputs.Back = Input.GetKey(KeyCode.S);
			inputs.Left = Input.GetKey(KeyCode.A);
			inputs.Right = Input.GetKey(KeyCode.D);
			inputs.Jump = Input.GetKey(KeyCode.Space);

			clientTickRate += Time.deltaTime;

			gameObject.transform.position = Vector3.Lerp(
				prevPos,
				nextPos,
				clientTickRate / ConstantValues.SERVER_TICK_RATE
				);

			if (clientTickRate >= ConstantValues.SERVER_TICK_RATE)
			{
				SendAndPredict();
				clientTickRate = 0;
				sequenceNumber++;
			}
		}

		private void SendAndPredict()
		{
			ClientSend.SendPlayerInputs(sequenceNumber, inputs, transform.rotation);
			PredictPlayerPosition();
		}

		private void PredictPlayerPosition()
		{
			prevPos = gameObject.transform.position;
			gameObject.transform.position = GameManagerClient.Instance.GetLastPredictedPos();
			if (gameObject.transform.position == Vector3.zero)
				gameObject.transform.position = prevPos;

			yVelocityPreMove = yVelocity;
			isGroundedPreMove = controller.isGrounded;


			controller.enabled = true;
			currentVelocity = PlayerMovementCalculations.CalculateCurrentVelocity(inputs, transform, ref yVelocity, controller.isGrounded);
			if (currentVelocity.x == 0 && currentVelocity.z == 0)
			{
				PlayerMovementCalculations.CalculatePreviousVelocity(yVelocity, ref previousVelocity);
				currentVelocity = previousVelocity;
			}
			else
				previousVelocity = currentVelocity;

			if (controller.Move(currentVelocity) == CollisionFlags.Above)
				yVelocity = 0;
			controller.enabled = false;


			GameManagerClient.Instance.LocalPositionPredictions.Add(new LocalPredictionData(
				sequenceNumber, inputs, gameObject.transform.position, transform, yVelocityPreMove, isGroundedPreMove));

			nextPos = gameObject.transform.position;
			gameObject.transform.position = prevPos;
		}

		public void SetRespawnValues()
		{
			prevPos = transform.position;
			nextPos = transform.position;

			pickedUpWeapons = Weapons.GetNewWeapons();
			currentWeapon = 1;
		}

		private byte? GetPressedWeaponKey()
		{
			for (byte i = (byte)KeyCode.Alpha0; i <= (byte)KeyCode.Alpha9; i++)
			{
				if (Input.GetKeyDown((KeyCode)i))
					return (byte)(i - (byte)KeyCode.Alpha0);
			}

			return null;
		}

		private bool HasWeapon()
		{
			return PressedWeaponKey < pickedUpWeapons.Count && pickedUpWeapons[(byte)PressedWeaponKey].IsPickedUp;
		}

		public void NewAmmoCount(ushort ammoCount)
		{
			pickedUpWeapons[currentWeapon].Ammo = ammoCount;
			UIManager.Instance.SetAmmoCount(ammoCount.ToString());
		}

	}
}