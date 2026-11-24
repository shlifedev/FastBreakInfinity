using System;
using System.Globalization;
using System.Numerics;

namespace LD.Numeric.IdleNumber
{
    public static class AlphabetConverter
    {
        private const int ExponentUnit = 3;
        private static readonly string[] FixedPointFormats =
        {
            "F0",
            "F1",
            "F2",
            "F3",
            "F4",
            "F5",
            "F6",
            "F7",
            "F8",
            "F9",
            "F10",
            "F11",
            "F12",
            "F13",
            "F14",
            "F15",
            "F16",
            "F17",
        };

        /// <summary>
        /// 값의 3자리 단위 지수를 계산합니다.
        /// </summary>
        public static long GetExponent(double value)
        {
            if (!(value > 0) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "지수는 양수에 대해서만 계산할 수 있습니다."
                );
            }

            // Log10은 경계 양옆에서 같은 값을 줄 수 있으므로 가장 가까운 경계와 직접 비교한다.
            long exponent = (long)Math.Round(Math.Log10(value) / ExponentUnit) * ExponentUnit;
            double boundary = Math.Pow(10, exponent);
            if (value >= 1 && value < boundary)
                return exponent - ExponentUnit;
            if (value < 1 && value > boundary)
                return exponent + ExponentUnit;
            return exponent;
        }

        // int, float, long은 암묵적으로 double로 변환되어 위 메서드 사용
        public static long GetExponent(BigInteger value)
        {
            if (value.Sign <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "지수는 양수에 대해서만 계산할 수 있습니다."
                );

            long exponent = (long)Math.Round(BigInteger.Log10(value) / ExponentUnit) * ExponentUnit;
            var boundary = BigInteger.Pow(10, (int)exponent);
            return value < boundary ? exponent - ExponentUnit : exponent;
        }

        #region 알파벳포함값 => 일반값 역함수

        public static double ConvertFromAlphabetUnit(string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0;
            if (str == "Infinity")
                return double.PositiveInfinity;
            if (str == "-Infinity")
                return double.NegativeInfinity;
            if (str == "NaN")
                return double.NaN;

            int lastAlphaIndex = str.Length - 1;
            while (lastAlphaIndex >= 0 && char.IsLetter(str[lastAlphaIndex]))
                lastAlphaIndex--;

            // 숫자 부분이 없는 경우 (전체가 알파벳)
            if (lastAlphaIndex < 0)
                return 0;

            // 알파벳 부분이 없는 경우 (전체가 숫자)
            if (lastAlphaIndex == str.Length - 1)
                return double.Parse(str, CultureInfo.InvariantCulture);

            var number = str.AsSpan(0, lastAlphaIndex + 1).TrimEnd();
            long index = AlphabetManager.GetIndexFromUnit(str.AsSpan(lastAlphaIndex + 1));
            // 단위 지수를 적용한 뒤 double로 한 번만 파싱한다 (0.1 × 10^309도 유한).
            int marker = number.IndexOfAny('e', 'E');
            if (marker >= 0 || index >= long.MaxValue / ExponentUnit)
            {
                BigInteger exponent = ((BigInteger)index + 1) * ExponentUnit;
                if (marker >= 0)
                {
                    exponent += BigInteger.Parse(
                        number.Slice(marker + 1),
                        NumberStyles.AllowLeadingSign,
                        CultureInfo.InvariantCulture
                    );
                    number = number.Slice(0, marker);
                }
                return ParseScaledNumber(
                    number.ToString() + "e" + exponent.ToString(CultureInfo.InvariantCulture)
                );
            }

            long unitExponent = (index + 1) * ExponentUnit;
            Span<char> literal =
                number.Length <= 64
                    ? stackalloc char[number.Length + 20]
                    : new char[number.Length + 20];
            number.CopyTo(literal);
            literal[number.Length] = 'e';
            unitExponent.TryFormat(
                literal.Slice(number.Length + 1),
                out int written,
                default,
                CultureInfo.InvariantCulture
            );
            return ParseScaledNumber(literal.Slice(0, number.Length + 1 + written));
        }

        private static double ParseScaledNumber(ReadOnlySpan<char> literal)
        {
            try
            {
                return double.Parse(
                    literal,
                    NumberStyles.Float | NumberStyles.AllowThousands,
                    CultureInfo.InvariantCulture
                );
            }
            catch (OverflowException)
            {
                // Unity Mono는 범위를 넘는 유효 숫자를 Infinity로 반환하는 대신 예외를 던진다.
                return literal.TrimStart()[0] == '-'
                    ? double.NegativeInfinity
                    : double.PositiveInfinity;
            }
        }

        #endregion

        #region Value To Alphabet

        public static string ConvertToAlphabetUnit(this int number, int maxDecimalPoint = 2) =>
            ConvertToAlphabetUnit((double)number, maxDecimalPoint);

        public static string ConvertToAlphabetUnit(this float number, int maxDecimalPoint = 2) =>
            ConvertToAlphabetUnit((double)number, maxDecimalPoint);

        public static string ConvertToAlphabetUnit(this double number, int maxDecimalPoint = 2)
        {
            if (double.IsNaN(number) || double.IsInfinity(number))
                return number.ToString(CultureInfo.InvariantCulture);

            double magnitude = Math.Abs(number);
            if (magnitude < 1000)
                return number.ToString(CultureInfo.InvariantCulture);

            long exponent = GetExponent(magnitude);
            double divisor = Math.Pow(10, exponent);
            double newNumber = number / divisor;
            if (maxDecimalPoint <= 15)
                newNumber = Math.Round(newNumber, maxDecimalPoint);
            // 반올림이 단위 경계(1000)에 도달하면 단위를 한 칸 올림 (999999 → 1.00B)
            if (Math.Abs(newNumber) >= 1000)
            {
                newNumber /= 1000;
                exponent += ExponentUnit;
            }
            string unit = AlphabetManager.GetAlphabetUnitFromExponent(exponent);
            return string.Concat(
                newNumber.ToString(
                    GetFixedPointFormat(maxDecimalPoint),
                    CultureInfo.InvariantCulture
                ),
                unit
            );
        }

        #endregion

        private static string GetFixedPointFormat(int decimalPlaces)
        {
            if ((uint)decimalPlaces < (uint)FixedPointFormats.Length)
            {
                return FixedPointFormats[decimalPlaces];
            }

            return "F" + decimalPlaces.ToString(CultureInfo.InvariantCulture);
        }
    }
}
