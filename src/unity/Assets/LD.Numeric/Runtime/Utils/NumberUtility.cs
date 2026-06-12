using System;

namespace LD.Numeric.IdleNumber
{
    public static class NumberUtility
    {
        public static int GetDigits(double value)
        {
            // (int)Infinity는 int.MinValue가 되고 Math.Abs(int.MinValue)는 OverflowException
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return 0;
            }

            int digits = 0;
            int number = Math.Abs((int)value);
            while (number > 0)
            {
                number /= 10;
                digits++;
            }

            return digits;
        }
    }
}
