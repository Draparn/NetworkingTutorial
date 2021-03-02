using UnityEngine;

namespace SmallMultiplayerGame.Client.Gameplay
{
	public class ProjectileClient : MonoBehaviour
	{
		public GameObject ExplosionPrefab;

		public void Explode(Vector3 pos)
		{
			transform.position = pos;
			Instantiate(ExplosionPrefab, transform.position, Quaternion.identity);
			gameObject.SetActive(false);
		}
	}
}