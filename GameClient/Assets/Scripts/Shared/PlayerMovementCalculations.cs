using UnityEngine;

namespace NetworkTutorial.Shared
{
	public class PlayerMovementCalculations : MonoBehaviour
	{
		private static float moveSpeed = ConstantValues.PLAYER_MOVE_SPEED * Time.fixedDeltaTime;
		private static float jumpSpeed = ConstantValues.PLAYER_JUMP_SPEED * Time.fixedDeltaTime;
		private static float gravity = ConstantValues.WORLD_GRAVITY * Time.fixedDeltaTime * Time.fixedDeltaTime;

		public static Vector3 CalculatePlayerPosition(bool[] inputs, Transform transform, float yVelocity, bool isGrounded)
		{
			var inputDirection = Vector2.zero;
			if (inputs[0])  //W
				inputDirection.y += 1;
			if (inputs[1])  //S
				inputDirection.y -= 1;
			if (inputs[2])  //A
				inputDirection.x -= 1;
			if (inputs[3])  //D
				inputDirection.x += 1;

			var moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.y;
			moveDirection *= moveSpeed;

			if (isGrounded)
			{
				yVelocity = 0;

				if (inputs[4]) //Spacebar
					yVelocity = jumpSpeed;
			}
			else
				yVelocity += gravity;

			moveDirection.y = yVelocity;

			return moveDirection;
		}

		public static Vector3 CalculatePlayerPosition(bool[] inputs, Transform transform, ref float yVelocity, bool isGrounded)
		{
			var inputDirection = Vector2.zero;
			if (inputs[0])  //W
				inputDirection.y += 1;
			if (inputs[1])  //S
				inputDirection.y -= 1;
			if (inputs[2])  //A
				inputDirection.x -= 1;
			if (inputs[3])  //D
				inputDirection.x += 1;

			var moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.y;
			moveDirection *= moveSpeed;

			if (isGrounded)
			{
				yVelocity = 0;

				if (inputs[4]) //Spacebar
					yVelocity = jumpSpeed;
			}
			else
				yVelocity += gravity;

			moveDirection.y = yVelocity;

			return moveDirection;
		}
	}
}
