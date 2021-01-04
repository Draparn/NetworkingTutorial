using UnityEngine;

namespace NetworkTutorial.Shared
{
	public struct InputsStruct
	{
		public bool Forward;
		public bool Back;
		public bool Left;
		public bool Right;
		public bool Jump;

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
		public static Vector3 ReCalculatePlayerPosition(InputsStruct inputs, Vector3 transformRight, Vector3 transformForward, float yVelocity, bool isGrounded)
		{
			var inputDirection = Vector3.zero;
			if (inputs.Forward)  //W
				inputDirection.z += 1;
			if (inputs.Back)  //S
				inputDirection.z -= 1;
			if (inputs.Left)  //A
				inputDirection.x -= 1;
			if (inputs.Right)  //D
				inputDirection.x += 1;

			var moveDirection = transformRight * inputDirection.x + transformForward * inputDirection.z;
			moveDirection.Normalize();
			moveDirection *= ConstantValues.PLAYER_MOVE_SPEED * ConstantValues.SERVER_TICK_RATE;

			if (isGrounded)
			{
				yVelocity = 0;

				if (inputs.Jump) //Spacebar
					yVelocity = ConstantValues.PLAYER_JUMP_SPEED * ConstantValues.SERVER_TICK_RATE;
			}
			else
				yVelocity += ConstantValues.WORLD_GRAVITY * ConstantValues.SERVER_TICK_RATE * ConstantValues.SERVER_TICK_RATE;

			moveDirection.y = yVelocity;

			return moveDirection;
		}

		public static Vector3 CalculatePlayerPosition(InputsStruct inputs, Vector3 transformRight, Vector3 transformForward, ref float yVelocity, bool isGrounded)
		{
			var inputDirection = Vector3.zero;
			if (inputs.Forward)  //W
				inputDirection.z += 1;
			if (inputs.Back)  //S
				inputDirection.z -= 1;
			if (inputs.Left)  //A
				inputDirection.x -= 1;
			if (inputs.Right)  //D
				inputDirection.x += 1;

			var moveDirection = transformRight * inputDirection.x + transformForward * inputDirection.z;
			moveDirection.Normalize();
			moveDirection *= ConstantValues.PLAYER_MOVE_SPEED * ConstantValues.SERVER_TICK_RATE;

			if (isGrounded)
			{
				yVelocity = 0;

				if (inputs.Jump) //Spacebar
					yVelocity = ConstantValues.PLAYER_JUMP_SPEED * ConstantValues.SERVER_TICK_RATE;
			}
			else
				yVelocity += ConstantValues.WORLD_GRAVITY * ConstantValues.SERVER_TICK_RATE * ConstantValues.SERVER_TICK_RATE;

			moveDirection.y = yVelocity;

			return moveDirection;
		}
	}
}
