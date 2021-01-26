using System.Collections;
using UnityEngine;

namespace NetworkTutorial.Client.Gameplay.WeaponScrips
{
	public class ClientWeapon : MonoBehaviour
	{
		private SpriteRenderer muzzleFlash;

		private void Start()
		{
			muzzleFlash = GetComponentInChildren<SpriteRenderer>();
			muzzleFlash.enabled = false;
		}

		public void Shoot()
		{
			StartCoroutine(Flash());
		}

		private IEnumerator Flash()
		{
			muzzleFlash.transform.parent.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
			muzzleFlash.enabled = true;
			yield return new WaitForSeconds(0.1f);
			muzzleFlash.enabled = false;
		}

	}
}
