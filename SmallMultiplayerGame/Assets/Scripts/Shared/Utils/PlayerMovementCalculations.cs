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
		private static Vector3 moveDirection, velocity;

		public static Vector3 ReCalculatePlayerPosition(InputsStruct inputs, Transform transform, float yVelocity, bool isGrounded)
		{
			moveDirection = Vector3.zero;
			if (inputs.Forward)  //W
				moveDirection.z += 1;
			if (inputs.Back)  //S
				moveDirection.z -= 1;
			if (inputs.Left)  //A
				moveDirection.x -= 1;
			if (inputs.Right)  //D
				moveDirection.x += 1;

			velocity = transform.right * moveDirection.x + transform.forward * moveDirection.z;
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

		public static Vector3 CalculatePlayerPosition(InputsStruct inputs, Transform transform, ref float yVelocity, bool isGrounded)
		{
			moveDirection = Vector3.zero;
			if (inputs.Forward)  //W
				moveDirection.z += 1;
			if (inputs.Back)  //S
				moveDirection.z -= 1;
			if (inputs.Left)  //A
				moveDirection.x -= 1;
			if (inputs.Right)  //D
				moveDirection.x += 1;

			velocity = transform.right * moveDirection.x + transform.forward * moveDirection.z;
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
