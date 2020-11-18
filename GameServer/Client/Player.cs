using System.Numerics;

namespace GameServer
{
	public class Player
	{
		public Vector3 Position;
		public Vector2 InputDirection;
		public Quaternion Rotation;

		public string PlayerName;
		private float playerMovementSpeed;
		public int PlayerId;
		private bool[] playerInput;

		public Player(int id, string name, Vector3 pos)
		{
			Position = pos;
			Rotation = Quaternion.Identity;
			PlayerName = name;
			playerMovementSpeed = ConstantValues.MOVEMENTSPEED / ConstantValues.TICKS_PER_SECOND;
			PlayerId = id;
			playerInput = new bool[4];
		}

		public void UpdatePosAndRot(bool[] inputs, Quaternion rot)
		{
			playerInput = inputs;
			Rotation = rot;
		}

		public void Update()
		{
			InputDirection = Vector2.Zero;
			if (playerInput[0])
				InputDirection.Y += 1;
			if (playerInput[1])
				InputDirection.Y -= 1;
			if (playerInput[2])
				InputDirection.X += 1;
			if (playerInput[3])
				InputDirection.X -= 1;

			MovePlayer(InputDirection);
		}

		private void MovePlayer(Vector2 inputDirection)
		{
			var forward = Vector3.Transform(new Vector3(0, 0, 1), Rotation);
			var right = Vector3.Normalize(Vector3.Cross(forward, new Vector3(0, 1, 0)));

			var moveDirection = right * inputDirection.X + forward * inputDirection.Y;
			Position += moveDirection * playerMovementSpeed;

			ServerSend.SendUpdatePlayerPosition(this);
			ServerSend.SendUpdatePlayerRotation(this);
		}
	}
}
