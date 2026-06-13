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

            // int 캐스팅은 ±21억 밖에서 포화/크래시 — Log10 기반으로 double 전 범위 처리
            int digits = (int)Math.Floor(Math.Log10(value)) + 1;
            // Log10 경계 오차 보정 (예: Log10(1000)이 2.999... 또는 3.000...1로 나오는 경우)
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
