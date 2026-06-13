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
