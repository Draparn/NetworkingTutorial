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
		private static float groundMultiplier = ConstantValues.PLAYER_VELOCITY_GROUND_MULTIPLIER, airMultiplier = ConstantValues.PLAYER_VELOCITY_AIR_MULTIPLIER;
		private static float tickRate = ConstantValues.SERVER_TICK_RATE;
		private static float gravity = ConstantValues.WORLD_GRAVITY;
		private static float jumpForce = ConstantValues.PLAYER_JUMP_FORCE;
		private static float playerMoveSpeed = ConstantValues.PLAYER_MOVE_SPEED;

		public static Vector3 ReCalculateCurrentVelocity(InputsStruct inputs, Transform transform, float yVelocity, bool isGrounded)
		{
			return CalculateCurrentVelocity(inputs, transform, ref yVelocity, isGrounded);
		}

		public static Vector3 CalculateCurrentVelocity(InputsStruct inputs, Transform transform, ref float yVelocity, bool isGrounded)
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
			velocity *= playerMoveSpeed * tickRate;

			if (isGrounded)
			{
				yVelocity = 0;

				if (inputs.Jump) //Spacebar
					yVelocity = jumpForce * tickRate;
			}
			else
				yVelocity += gravity * tickRate * tickRate;

			velocity.y = yVelocity;

			return velocity;
		}

		public static void CalculatePreviousVelocity(float yVelocity, ref Vector3 previousVelocity)
		{
			if (yVelocity == 0)
				previousVelocity = new Vector3(previousVelocity.x * groundMultiplier, yVelocity, previousVelocity.z * groundMultiplier);
			else
				previousVelocity = new Vector3(previousVelocity.x * airMultiplier, yVelocity, previousVelocity.z * airMultiplier);

			if (previousVelocity.magnitude <= 0.05f)
				previousVelocity = Vector3.zero;
		}

	}
}
