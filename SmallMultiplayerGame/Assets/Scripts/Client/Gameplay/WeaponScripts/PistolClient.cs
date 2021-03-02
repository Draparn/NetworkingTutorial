using System.Collections;
using UnityEngine;

namespace SmallMultiplayerGame.Client.Gameplay.WeaponScrips
{
	public class PistolClient : WeaponClient
	{
		private SpriteRenderer muzzleFlash;

		private void Start()
		{
			muzzleFlash = GetComponentInChildren<SpriteRenderer>();
			muzzleFlash.enabled = false;
		}

		public override void Shoot()
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
