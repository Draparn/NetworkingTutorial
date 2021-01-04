using NetworkTutorial.Shared;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Client.Net
{
	public class ClientSnapshot
	{
		public static List<ClientSnapshot> Snapshots = new List<ClientSnapshot>();

		internal List<PlayerPosData> players;
		internal List<ProjectileData> projectiles;

		public ClientSnapshot(List<PlayerPosData> players, List<ProjectileData> projectiles)
		{
			this.players = players.Count > 0 ? players : null;
			this.projectiles = projectiles.Count > 0 ? projectiles : null;

			foreach (var playerData in players)
			{
				if (Client.Instance.MyId == playerData.PlayerId)
				{
					CheckPosAndReconcile(playerData);
					break;
				}
			}
		}

		private void CheckPosAndReconcile(PlayerPosData playerData)
		{
			for (int i = 0; i < GameManager.Instance.LocalPositionPredictions.Count; i++)
			{
				if (GameManager.Instance.LocalPositionPredictions[i].FrameNumber == playerData.FrameNumber)
				{
					if (Vector3.Distance(GameManager.Instance.LocalPositionPredictions[i].Position, playerData.Position) > 0.2f)
					{
						//Debug.LogError($"Correcting. Index:{i}. Frame:{playerData.FrameNumber}, Predicted pos was: {GameManager.Instance.LocalPositionPredictions[i].Position} and should be: {playerData.Position}");
						GameManager.Instance.LocalPositionPredictions.RemoveRange(0, i);

						for (int j = 0; j < GameManager.Instance.LocalPositionPredictions.Count; j++)
						{
							var prediction = GameManager.Instance.LocalPositionPredictions[j];

							if (j == 0)
							{
								prediction.Position = playerData.Position;
								GameManager.Instance.LocalPositionPredictions[j] = new LocalPredictionData(prediction);
							}
							else
							{
								prediction.Position = GameManager.Instance.LocalPositionPredictions[j - 1].Position +
									PlayerMovementCalculations.ReCalculatePlayerPosition(
									GameManager.Instance.LocalPositionPredictions[j].Inputs,
									GameManager.Instance.LocalPositionPredictions[j].TransformRight,
									GameManager.Instance.LocalPositionPredictions[j].TransformForward,
									GameManager.Instance.LocalPositionPredictions[j].yVelocityPreMove,
									GameManager.Instance.LocalPositionPredictions[j].IsGroundedPreMove
									);

								GameManager.Instance.LocalPositionPredictions[j] = new LocalPredictionData(prediction);
							}
						}

						break;
					}

					GameManager.Instance.LocalPositionPredictions.RemoveRange(0, i + 1);
					break;
				}
			}

		}

	}
}
