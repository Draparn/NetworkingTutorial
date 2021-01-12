

namespace NetworkTutorial.Shared.Utils
{
	public class ValueTypeConversions
	{
		public static short ReturnFloatDecimalsAsShort(float value)
		{
			int wholeNumber = (int)value;
			return (short)((value - wholeNumber) * 10000);
		}

		public static float ReturnShortAsFloatDecimals(short value)
		{
			float decimals = value;
			return decimals /= 10000;
		}
	}
}
