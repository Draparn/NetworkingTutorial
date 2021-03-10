using System;
using System.Collections;
using UnityEngine;

namespace SmallMultiplayerGame.Shared.Utils
{
	public class ValueTypeConversions
	{
		private static BitArray bitArray;
		private static byte value;

		public static byte ReturnDecimalsAsByte(float value)
		{
			long wholeNumber = (long)value;
			return (byte)Mathf.RoundToInt((value - wholeNumber) / 4 * 1000);
		}

		public static float ReturnByteAsFloat(byte value)
		{
			return (float)(value / 1000f * 4f);
		}

		public static short ReturnDecimalsAsShort(float value)
		{
			long wholeNumber = (long)value;
			return (short)Mathf.RoundToInt((value - wholeNumber) / 4 * 100000);
		}

		public static float ReturnShortAsFloat(short value)
		{
			return (float)(value / 100000f * 4f);
		}

		public static byte ReturnInputsAsByte(InputsStruct inputs)
		{
			value = 0;

			if (inputs.Forward)
				value += 1;
			if (inputs.Back)
				value += 2;
			if (inputs.Left)
				value += 4;
			if (inputs.Right)
				value += 8;
			if (inputs.Jump)
				value += 16;

			return value;
		}

		public static InputsStruct ReturnByteAsInput(byte value)
		{
			if (value == 0)
				return new InputsStruct(false, false, false, false, false);

			bitArray = new BitArray(BitConverter.GetBytes(value));

			return new InputsStruct(bitArray.Get(0), bitArray.Get(1), bitArray.Get(2), bitArray.Get(3), bitArray.Get(4));
		}
	}
}
