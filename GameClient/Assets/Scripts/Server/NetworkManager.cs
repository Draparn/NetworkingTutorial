﻿using NetworkTutorial.Server.Client;
using NetworkTutorial.Shared;
using UnityEngine;

namespace NetworkTutorial.Server.Managers
{
	public class NetworkManager : MonoBehaviour
	{
		public static NetworkManager instance;

		public GameObject PlayerPrefab;
		public GameObject ProjectilePrefab;


		private void Awake()
		{
			if (instance == null)
				instance = this;
			else if (instance != this)
				Destroy(this);
		}

		private void Start()
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 30;
			Server.StartServer(ConstantValues.SERVER_MAX_PLAYERS, ConstantValues.SERVER_PORT);
		}

		private void OnApplicationQuit()
		{
			Server.StopServer();
		}

		public Player InstantiatePlayer()
		{
			return Instantiate(PlayerPrefab, new Vector3(0, 0.5f, 0), Quaternion.identity).GetComponent<Player>();
		}

		public Projectile InstantiateProjectile(Transform shootOrigin)
		{
			return Instantiate(ProjectilePrefab, shootOrigin.position + shootOrigin.forward * 0.7f, Quaternion.identity).GetComponent<Projectile>();
		}

	}
}