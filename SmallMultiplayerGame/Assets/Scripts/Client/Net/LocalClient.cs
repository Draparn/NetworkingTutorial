using SmallMultiplayerGame.ClientLol.Gameplay;
using SmallMultiplayerGame.ClientLol.Gameplay.Player;
using SmallMultiplayerGame.Shared;
using SmallMultiplayerGame.Shared.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmallMultiplayerGame.ClientLol.Net
{
	public partial class LocalClient : MonoBehaviour
	{
		public static LocalClient Instance;

		protected delegate void PacketHandler(Packet packet);
		protected static Dictionary<int, PacketHandler> packethandlers;

		public UDP Connection;

		public string playerName;

		private float disconnectTimer;

		[HideInInspector] public byte MyId = 0;

		public bool isConnected = false;

		private void Awake()
		{
			if (Instance == null)
				Instance = this;
			else if (Instance != this)
				Destroy(this);
		}

		private void Update()
		{
			if (isConnected)
			{
				disconnectTimer += Time.deltaTime;

				if (disconnectTimer >= ConstantValues.CONNECTION_TIMEOUT_TIMER)
				{
					Debug.Log("Server not responding. Disconnecting...");
					ClientSend.SendDisconnect();
					Instance.Disconnect();
					GameObject.Destroy(GameManagerClient.Instance.Players[Instance.MyId].gameObject);
					GameManagerClient.Instance.Players.Remove(Instance.MyId);
					UIManager.Instance.ShowMainMenu();
				}
			}
		}

		private void OnApplicationQuit()
		{
			if (isConnected)
			{
				ClientSend.SendDisconnect();
				Disconnect();
			}
		}

		public void ConnectToServer(string ip, string playerName)
		{
			Connection = new UDP(ip);
			Connection.InitializeClientData();
			Connection.Connect();
			this.playerName = playerName;

			StartCoroutine(SendConnectAndWait());
		}

		public void Disconnect()
		{
			isConnected = false;
			Connection.udpClient.Close();
			Connection = null;
		}

		public void ResetNameAndId()
		{
			playerName = "ClientName";
			MyId = 0;
		}

		private IEnumerator SendConnectAndWait()
		{
			byte ticker = 0;

			do
			{
				ClientSend.SendConnectRequest();
				yield return new WaitForSeconds(2);
				ticker++;
			} while (ticker < 5);

			UIManager.Instance.ShowMainMenu();
		}

		public void StopConnectionTimer()
		{
			StopAllCoroutines();
		}
	}
}