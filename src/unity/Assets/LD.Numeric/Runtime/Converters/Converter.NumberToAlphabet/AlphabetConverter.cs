using System;
using System.Globalization;
using System.Numerics;
using LD;

namespace LD.Numeric.IdleNumber
{
    public static class AlphabetConverter
    {
        private const int ExponentUnit = 3;

        /// <summary>
        /// 값의 3자리 단위 지수를 계산합니다.
        /// </summary>
        public static long GetExponent(double value)
        {
            return (long)(Math.Log10(value) / ExponentUnit) * ExponentUnit;
        }

        // int, float, long은 암묵적으로 double로 변환되어 위 메서드 사용
        public static long GetExponent(BigInteger value)
        {
            return (long)(BigInteger.Log10(value) / ExponentUnit) * ExponentUnit;
        }

        #region 알파벳포함값 => 일반값 역함수

        public static double ConvertFromAlphabetUnit(string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0;

            int lastAlphaIndex = str.Length - 1;
            while (lastAlphaIndex >= 0 && char.IsLetter(str[lastAlphaIndex]))
                lastAlphaIndex--;

            // 숫자 부분이 없는 경우 (전체가 알파벳)
            if (lastAlphaIndex < 0)
                return 0;

            // 알파벳 부분이 없는 경우 (전체가 숫자)
            if (lastAlphaIndex == str.Length - 1)
                return double.Parse(str, CultureInfo.InvariantCulture);

            string numberPart = str.Substring(0, lastAlphaIndex + 1);
            string unitPart = str.Substring(lastAlphaIndex + 1);
            double number = double.Parse(numberPart, CultureInfo.InvariantCulture);
            long exponent = AlphabetManager.GetIndexFromUnit(unitPart) * 3;
            return number * Math.Pow(10, exponent);
        }

        #endregion

        #region Value To Alphabet

        public static string ConvertToAlphabetUnit(this int number, int maxDecimalPoint = 2)
            => ConvertToAlphabetUnit((double)number, maxDecimalPoint);

        public static string ConvertToAlphabetUnit(this float number, int maxDecimalPoint = 2)
            => ConvertToAlphabetUnit((double)number, maxDecimalPoint);

        public static string ConvertToAlphabetUnit(this double number, int maxDecimalPoint = 2)
        {
            if (number < 1000)
                return number.ToString(CultureInfo.InvariantCulture);

            long exponent = GetExponent(number);
            double divisor = Math.Pow(10, exponent);
            double newNumber = number / divisor;
            string unit = AlphabetManager.GetAlphabetUnitFromExponent(exponent);
            return $"{newNumber.ToString($"F{maxDecimalPoint}", CultureInfo.InvariantCulture)}{unit}";
        }

        #endregion
    }
}
