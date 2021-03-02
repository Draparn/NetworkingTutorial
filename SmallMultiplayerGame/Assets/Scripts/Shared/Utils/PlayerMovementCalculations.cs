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
		public static Vector3 ReCalculatePlayerPosition(InputsStruct inputs, Transform transform, float yVelocity, bool isGrounded)
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

			var moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.z;
			moveDirection.Normalize();
			moveDirection *= ConstantValues.PLAYER_MOVE_SPEED * ConstantValues.SERVER_TICK_RATE;

			if (isGrounded)
			{
				yVelocity = 0;

				if (inputs.Jump) //Spacebar
					yVelocity = ConstantValues.PLAYER_JUMP_FORCE * ConstantValues.SERVER_TICK_RATE;
			}
			else
				yVelocity += ConstantValues.WORLD_GRAVITY * ConstantValues.SERVER_TICK_RATE * ConstantValues.SERVER_TICK_RATE;

			moveDirection.y = yVelocity;

			return moveDirection;
		}

		public static Vector3 CalculatePlayerPosition(InputsStruct inputs, Transform transform, ref float yVelocity, bool isGrounded)
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

			var moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.z;
			moveDirection.Normalize();
			moveDirection *= ConstantValues.PLAYER_MOVE_SPEED * ConstantValues.SERVER_TICK_RATE;

			if (isGrounded)
			{
				yVelocity = 0;

				if (inputs.Jump) //Spacebar
					yVelocity = ConstantValues.PLAYER_JUMP_FORCE * ConstantValues.SERVER_TICK_RATE;
			}
			else
				yVelocity += ConstantValues.WORLD_GRAVITY * ConstantValues.SERVER_TICK_RATE * ConstantValues.SERVER_TICK_RATE;

			moveDirection.y = yVelocity;

			return moveDirection;
		}

		public static void NewMovement(InputsStruct inputs, Transform transform, ref Rigidbody rb)
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

			var velocity = transform.right * inputDirection.x + transform.forward * inputDirection.z;
			velocity.Normalize();

			Debug.Log(rb.velocity.magnitude);
			if (rb.velocity.magnitude < ConstantValues.PLAYER_MOVE_SPEED)
			{
				rb.AddForce(velocity * ConstantValues.PLAYER_MOVE_SPEED * 4);
			}

			if (rb.velocity.y > -0.01f && rb.velocity.y < 0.01f)
			{
				if (inputs.Jump) //Spacebar
				{
					rb.AddForce(Vector3.up * ConstantValues.PLAYER_JUMP_FORCE, ForceMode.Impulse);
				}
			}
		}
	}
}
