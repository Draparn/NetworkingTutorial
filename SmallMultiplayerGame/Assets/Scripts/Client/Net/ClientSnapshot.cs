using SmallMultiplayerGame.Client.Gameplay;
using SmallMultiplayerGame.Shared.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace SmallMultiplayerGame.Client.Net
{
	public class ClientSnapshot
	{
		public static List<ClientSnapshot> Snapshots = new List<ClientSnapshot>();

		public List<PlayerPosData> players;
		public List<ProjectileData> projectiles;

		private LocalPredictionData predData;
		private PlayerPosData playerData;

		public uint sequenceNumber;
		private int count;

		public ClientSnapshot(uint sequenceNumber, List<PlayerPosData> players, List<ProjectileData> projectiles)
		{
			this.sequenceNumber = sequenceNumber;
			this.players = players.Count > 0 ? players : null;
			this.projectiles = projectiles.Count > 0 ? projectiles : null;

			count = players.Count;
			for (int i = 0; i < count; i++)
			{
				playerData = players[i];

				if (LocalClient.Instance.MyId == playerData.PlayerId)
				{
					CheckPosAndReconcile(playerData);
					break;
				}
			}
		}

		private void CheckPosAndReconcile(PlayerPosData playerData)
		{
			count = GameManagerClient.Instance.LocalPositionPredictions.Count;
			for (int i = 0; i < count; i++)
			{
				predData = GameManagerClient.Instance.LocalPositionPredictions[i];

				if (predData.SequenceNumber == playerData.Sequencenumber)
				{
					if (predData.IsGroundedPreMove && Vector3.Distance(predData.Position, playerData.Position) > 0.1f)
					{
						GameManagerClient.Instance.LocalPositionPredictions.RemoveRange(0, i);

						count = GameManagerClient.Instance.LocalPositionPredictions.Count;
						for (int j = 0; j < count; j++)
						{
							if (j == 0)
							{
								predData.Position = playerData.Position;
								GameManagerClient.Instance.LocalPositionPredictions[j] = new LocalPredictionData(predData);
							}
							else
							{
								predData.Position = GameManagerClient.Instance.LocalPositionPredictions[j - 1].Position +
									PlayerMovementCalculations.ReCalculateCurrentVelocity(
									GameManagerClient.Instance.LocalPositionPredictions[j].Inputs,
									GameManagerClient.Instance.LocalPositionPredictions[j].Transform,
									GameManagerClient.Instance.LocalPositionPredictions[j].yVelocityPreMove,
									GameManagerClient.Instance.LocalPositionPredictions[j].IsGroundedPreMove
									);

								GameManagerClient.Instance.LocalPositionPredictions[j] = new LocalPredictionData(predData);
							}
						}

						break;
					}

					GameManagerClient.Instance.LocalPositionPredictions.RemoveRange(0, i + 1);
					break;
				}
			}

		}

	}
}
