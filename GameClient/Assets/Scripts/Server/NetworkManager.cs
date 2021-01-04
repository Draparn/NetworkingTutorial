using NetworkTutorial.Server.Client;
using NetworkTutorial.Server.Net;
using NetworkTutorial.Shared;
using UnityEngine;

namespace NetworkTutorial.Server.Managers
{
	public class NetworkManager : MonoBehaviour
	{
		public static NetworkManager instance;

		public GameObject PlayerPrefab;
		public GameObject ProjectilePrefab;

		private float snapshotInterval;

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
			Application.targetFrameRate = 60;
			Server.StartServer(ConstantValues.SERVER_MAX_PLAYERS, ConstantValues.SERVER_PORT);
		}

		private void LateUpdate()
		{
			snapshotInterval += Time.deltaTime;

			if (snapshotInterval >= ConstantValues.SERVER_TICK_RATE)
			{
				snapshotInterval = 0;
				ServerSend.SendSnapshot();
			}
		}

		private void OnApplicationQuit()
		{
			Server.StopServer();
		}

		public Player InstantiatePlayer()
		{
			return Instantiate(PlayerPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<Player>();
		}

		public Projectile InstantiateProjectile(Transform shootOrigin)
		{
			return Instantiate(ProjectilePrefab, shootOrigin.position + shootOrigin.forward * 0.7f, Quaternion.identity).GetComponent<Projectile>();
		}

	}
}