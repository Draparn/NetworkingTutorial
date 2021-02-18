﻿using NetworkTutorial.Client.Gameplay;
using NetworkTutorial.Client.Net;
using NetworkTutorial.Shared;
using NetworkTutorial.Shared.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Client.Player
{
	public class PlayerController : MonoBehaviour
	{
		public static PlayerController Instance;

		private Transform cameraTransform;
		private CharacterController controller;
		private PlayerClient player;
		private InputsStruct inputs = new InputsStruct();
		private Vector3 prevPos, nextPos;

		private float yVelocity, yVelocityPreMove, clientTickRate;
		private byte? PressedWeaponKey = null;
		public byte currentWeapon;
		private bool isGroundedPreMove;
		public List<Weapon> pickedUpWeapons;

		private ushort sequenceNumber = 0;

		private void Awake()
		{
			if (Instance == null)
				Instance = this;
			else if (Instance != this)
				Destroy(this);

			pickedUpWeapons = Weapons.GetNewWeapons();
		}

		private void Start()
		{
			cameraTransform = GetComponentInChildren<CameraController>().transform;
			player = GetComponent<PlayerClient>();
			controller = GetComponent<CharacterController>();

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
			controller.Move(PlayerMovementCalculations.CalculatePlayerPosition(inputs, transform, ref yVelocity, controller.isGrounded));
			controller.enabled = false;
			GameManagerClient.Instance.LocalPositionPredictions.Add(new LocalPredictionData(
				sequenceNumber,
				inputs,
				gameObject.transform.position,
				transform,
				yVelocityPreMove,
				isGroundedPreMove
				));

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
			if (PressedWeaponKey < pickedUpWeapons.Count && pickedUpWeapons[(byte)PressedWeaponKey].IsPickedUp)
				return true;

			return false;
		}

		public void NewAmmoCount(ushort ammoCount)
		{
			pickedUpWeapons[currentWeapon].Ammo = ammoCount;
			UIManager.Instance.SetAmmoCount(ammoCount.ToString());
		}

	}
}