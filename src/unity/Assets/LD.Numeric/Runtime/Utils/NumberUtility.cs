using System;

namespace LD.Numeric.IdleNumber
{
    public static class NumberUtility
    {
        public static int GetDigits(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return 0;
            }

            value = Math.Abs(value);
            if (value < 1)
            {
                return 0;
            }

            if (value < 10)
                return 1;
            if (value < 100)
                return 2;
            if (value < 1000)
                return 3;
            if (value < 10000)
                return 4;
            if (value < 100000)
                return 5;
            if (value < 1000000)
                return 6;
            if (value < 10000000)
                return 7;
            if (value < 100000000)
                return 8;
            if (value < 1000000000)
                return 9;
            if (value < 10000000000)
                return 10;
            if (value < 100000000000)
                return 11;
            if (value < 1000000000000)
                return 12;
            if (value < 10000000000000)
                return 13;
            if (value < 100000000000000)
                return 14;
            if (value < 1000000000000000)
                return 15;
            if (value < 10000000000000000)
                return 16;

            int digits = (int)Math.Floor(Math.Log10(value)) + 1;
            if (Math.Pow(10, digits) <= value)
            {
                digits++;
            }
            else if (Math.Pow(10, digits - 1) > value)
            {
                digits--;
            }

            return digits;
        }
    }
}
