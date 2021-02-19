using UnityEngine;

namespace NetworkTutorial.Shared.Utils
{
	public class ValueTypeConversions
	{
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
	}
}
