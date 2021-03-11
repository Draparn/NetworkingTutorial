using UnityEngine;

namespace SmallMultiplayerGame.Shared.Utils
{
	public class ValueTypeConversions
	{
		private static byte value;

		/// <summary>
		/// This converts a float value to a byte with three decimal points worth of precision.
		/// </summary>
		public static byte ReturnDecimalsAsByte(float value) //This will always be above 0 and below 1.
		{
			return (byte)Mathf.RoundToInt((value / 4) * 1000f);
		}

		/// <summary>
		/// This converts a float value to a short with five decimal points worth of precision.
		/// </summary>
		public static short ReturnDecimalsAsShort(float value)
		{
			return (short)Mathf.RoundToInt(((value - (long)value) / 4) * 100000);
		}

		/// <summary>
		/// This converts a byte value to a float with three decimal points worth of precision.
		/// </summary>
		public static float ReturnByteAsFloat(byte value)
		{
			return (value / 1000f) * 4f;
		}

		/// <summary>
		/// This converts a short value to a float with five decimal points worth of precision.
		/// </summary>
		public static float ReturnShortAsFloat(short value)
		{
			return (value / 100000f) * 4f;
		}

		/// <summary>
		/// Returns a byte with bits set according to the booleans in the array.
		/// </summary>
		public static byte ReturnBoolsAsByte(bool[] inputs)
		{
			value = 0;

			for (byte i = 0; i < inputs.Length; i++)
			{
				if (inputs[i])
					value += (byte)(1 << i);
			}

			return value;
		}

		/// <summary>
		/// Returns an InputStruct with booleans set according to the bits in the byte.
		/// </summary>
		public static InputsStruct ReturnByteAsInput(byte value)
		{
			return value == 0 ? new InputsStruct() : new InputsStruct(IsBitSet(value, 1), IsBitSet(value, 2), IsBitSet(value, 3), IsBitSet(value, 4), IsBitSet(value, 5));
		}

		private static bool IsBitSet(long value, byte bitNumber)
		{
			return (value & (1 << (bitNumber - 1))) != 0;
		}

	}
}
