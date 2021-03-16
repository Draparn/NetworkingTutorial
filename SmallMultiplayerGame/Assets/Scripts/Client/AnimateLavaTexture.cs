using UnityEngine;

namespace SmallMultiplayerGame.Client
{
	public class AnimateLavaTexture : MonoBehaviour
	{
		private Renderer rend;
		private Vector2 start;
		private float speed = 0.05f;

		void Start()
		{
			rend = GetComponent<Renderer>();
			start = rend.material.GetTextureOffset("_MainTex");
		}

		void Update()
		{
			rend.material.SetTextureOffset("_MainTex", start + new Vector2(Time.time, Mathf.Sin(Time.time / 5) * 5) * speed);
		}
	}
}