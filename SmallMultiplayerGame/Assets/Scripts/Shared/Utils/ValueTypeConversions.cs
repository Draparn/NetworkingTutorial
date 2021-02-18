using UnityEngine;

namespace NetworkTutorial.Shared.Utils
{
	public class ValueTypeConversions
	{
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
