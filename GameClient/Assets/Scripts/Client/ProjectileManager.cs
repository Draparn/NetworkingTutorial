using UnityEngine;

namespace NetworkTutorial.Client
{
	public class ProjectileManager : MonoBehaviour
	{
		public GameObject ExplosionPrefab;

		private ushort id;


		public void Init(ushort id)
		{
			this.id = id;
		}

		public void Explode(Vector3 pos)
		{
			transform.position = pos;
			Instantiate(ExplosionPrefab, transform.position, Quaternion.identity);
			gameObject.SetActive(false);
		}
	}
}