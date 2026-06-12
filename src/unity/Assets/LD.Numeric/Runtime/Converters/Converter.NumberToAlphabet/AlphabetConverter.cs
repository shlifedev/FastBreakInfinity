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
            // Log10이 0/음수에서 -Inf/NaN을 내고 long 캐스팅에서 포화돼 쓰레기 지수가 됨.
            // BigInteger 버전(BigInteger.Log10이 throw)과 동작을 맞춘다
            if (double.IsNaN(value) || value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "지수는 양수에 대해서만 계산할 수 있습니다."
                );
            }

            var exponent = (long)(Math.Log10(value) / ExponentUnit) * ExponentUnit;
            // Math.Log10(1e21)이 20.999...로 나오는 플랫폼에서 단위가 한 단계 어긋나는 것 보정
            if (value / Math.Pow(10, exponent) >= 1000)
            {
                exponent += ExponentUnit;
            }
            return exponent;
        }

        // int, float, long은 암묵적으로 double로 변환되어 위 메서드 사용
        public static long GetExponent(BigInteger value)
        {
            var exponent = (long)(BigInteger.Log10(value) / ExponentUnit) * ExponentUnit;
            // double 버전과 같은 보정 — Log10(10^30)이 29.999...로 나오면 단위가 한 단계 어긋남
            if (value / BigInteger.Pow(10, (int)exponent) >= 1000)
            {
                exponent += ExponentUnit;
            }
            return exponent;
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
            // GetIndexFromUnit은 0-indexed (A=0, B=1, ...) 이므로 +1하여 exponent 계산
            long exponent = (AlphabetManager.GetIndexFromUnit(unitPart) + 1) * 3;
            return number * Math.Pow(10, exponent);
        }

        #endregion

        #region Value To Alphabet

        public static string ConvertToAlphabetUnit(this int number, int maxDecimalPoint = 2) =>
            ConvertToAlphabetUnit((double)number, maxDecimalPoint);

        public static string ConvertToAlphabetUnit(this float number, int maxDecimalPoint = 2) =>
            ConvertToAlphabetUnit((double)number, maxDecimalPoint);

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
