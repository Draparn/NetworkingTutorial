using UnityEngine;

namespace SmallMultiplayerGame.Shared.Utils
{
	public struct InputsStruct
	{
		public bool Forward, Back, Left, Right, Jump;

		public InputsStruct(bool forward, bool back, bool left, bool right, bool jump)
		{
			Forward = forward;
			Back = back;
			Left = left;
			Right = right;
			Jump = jump;
		}
	}

	public class PlayerMovementCalculations : MonoBehaviour
	{
		private static Vector3 velocity;

		public static Vector3 ReCalculatePlayerPosition(InputsStruct inputs, Transform transform, float yVelocity, bool isGrounded)
		{
			return CalculatePlayerPosition(inputs, transform, ref yVelocity, isGrounded);
		}

		public static Vector3 CalculatePlayerPosition(InputsStruct inputs, Transform transform, ref float yVelocity, bool isGrounded)
		{
			velocity = Vector3.zero;
			if (inputs.Forward)  //W
				velocity.z += 1;
			if (inputs.Back)  //S
				velocity.z -= 1;
			if (inputs.Left)  //A
				velocity.x -= 1;
			if (inputs.Right)  //D
				velocity.x += 1;

			velocity = transform.right * velocity.x + transform.forward * velocity.z;
			velocity.Normalize();
			velocity *= ConstantValues.PLAYER_MOVE_SPEED * ConstantValues.SERVER_TICK_RATE;

			if (isGrounded)
			{
				yVelocity = 0;

				if (inputs.Jump) //Spacebar
					yVelocity = ConstantValues.PLAYER_JUMP_FORCE * ConstantValues.SERVER_TICK_RATE;
			}
			else
				yVelocity += ConstantValues.WORLD_GRAVITY * ConstantValues.SERVER_TICK_RATE * ConstantValues.SERVER_TICK_RATE;

			velocity.y = yVelocity;

			return velocity;
		}

	}
}
