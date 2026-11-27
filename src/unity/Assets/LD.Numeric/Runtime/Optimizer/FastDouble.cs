using System;
using System.Globalization;

namespace LD.Numeric.IdleNumber
{
    public static class FastDouble
    {
        private static readonly double[] PositivePowersOf10 =
        {
            1e0,
            1e1,
            1e2,
            1e3,
            1e4,
            1e5,
            1e6,
            1e7,
            1e8,
            1e9,
            1e10,
            1e11,
            1e12,
            1e13,
            1e14,
            1e15,
            1e16,
            1e17,
            1e18,
            1e19,
            1e20,
            1e21,
            1e22,
        };

        internal static double Pow10(int exp)
        {
            if (exp >= 0 && exp < PositivePowersOf10.Length)
                return PositivePowersOf10[exp];
            if (exp < 0 && -exp < PositivePowersOf10.Length)
                return 1.0 / PositivePowersOf10[-exp];
            return Math.Pow(10.0, exp);
        }

        public static double ParseDouble(string s, int maxDecimalPlaces = 6)
        {
            if (string.IsNullOrEmpty(s))
            {
                return 0;
            }
            return ParseDouble(s.AsSpan(), maxDecimalPlaces);
        }

        public static double ParseDouble(ReadOnlySpan<char> s, int maxDecimalPlaces = 6)
        {
            if (s.IsEmpty)
            {
                return 0;
            }
            if (maxDecimalPlaces < 0)
                maxDecimalPlaces = 0;
            if (s.IndexOfAny('e', 'E') >= 0)
                maxDecimalPlaces = int.MaxValue;

            bool isNegative = s[0] == '-';
            int startIndex = isNegative || s[0] == '+' ? 1 : 0;

            double mantissa = 0.0;
            long exponent = 0;
            bool negativeExponent = false;
            bool hasExponent = false;
            bool hasMantissaDigits = false;
            bool hasExponentDigits = false;
            int decimalPlaces = 0;
            int decimalPoint = -1;

            for (int i = startIndex; i < s.Length; i++)
            {
                char c = s[i];
                if (c >= '0' && c <= '9')
                {
                    if (hasExponent)
                    {
                        hasExponentDigits = true;
                        if (exponent <= int.MaxValue)
                            exponent = exponent * 10 + (c - '0');
                    }
                    else
                    {
                        hasMantissaDigits = true;
                        if (decimalPoint < 0 || decimalPlaces < maxDecimalPlaces)
                        {
                            mantissa = mantissa * 10.0 + (c - '0');
                            if (decimalPoint >= 0)
                                decimalPlaces++;
                        }
                    }
                }
                else if (c == '.' && decimalPoint < 0 && !hasExponent)
                {
                    decimalPoint = i;
                }
                else if ((c == 'e' || c == 'E') && !hasExponent)
                {
                    hasExponent = true;
                    if (i + 1 < s.Length && (s[i + 1] == '-' || s[i + 1] == '+'))
                    {
                        negativeExponent = s[i + 1] == '-';
                        i++;
                    }
                }
                else
                {
                    if (s.SequenceEqual("NaN".AsSpan()))
                        return double.NaN;
                    if (s.SequenceEqual("Infinity".AsSpan()))
                        return double.PositiveInfinity;
                    if (s.SequenceEqual("-Infinity".AsSpan()))
                        return double.NegativeInfinity;
                    throw new FormatException("입력값이 잘못 됨 => " + s.ToString());
                }
            }

            if (!hasMantissaDigits || (hasExponent && !hasExponentDigits))
            {
                throw new FormatException("입력값이 잘못 됨 => " + s.ToString());
            }

            if (mantissa == 0)
                return isNegative ? -0.0 : 0.0;
            long netExponent = (negativeExponent ? -exponent : exponent) - decimalPlaces;
            // 정수 가수와 10의 거듭제곱이 모두 정확히 표현될 때만 직접 스케일링한다.
            if (mantissa >= (1L << 53) || Math.Abs(netExponent) >= PositivePowersOf10.Length)
            {
                if (!hasExponent && decimalPoint >= 0)
                    s = s.Slice(0, decimalPoint + 1 + decimalPlaces);
                if (
                    double.TryParse(
                        s,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out double parsed
                    )
                )
                    return parsed == 0 && isNegative ? -0.0 : parsed;
                return isNegative ? double.NegativeInfinity : double.PositiveInfinity;
            }
            if (netExponent > 0)
            {
                mantissa *= PositivePowersOf10[netExponent];
            }
            else if (netExponent < 0)
            {
                mantissa /= PositivePowersOf10[-netExponent];
            }
            return isNegative ? -mantissa : mantissa;
        }

        public static string OptimizeToString(this double value, int decimalPlaces)
        {
            if (decimalPlaces < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(decimalPlaces),
                    "Decimal places 는 음수일 수 없습니다."
                );

            if (double.IsNaN(value))
                return "NaN";
            if (double.IsInfinity(value))
                return value > 0 ? "Infinity" : "-Infinity";
            if (Math.Abs(value) >= 9.2e18 || decimalPlaces > 17)
            {
                return value.ToString("F" + decimalPlaces, CultureInfo.InvariantCulture);
            }

            Span<char> buffer = stackalloc char[40];
            int pos = 0;
            if (BitConverter.DoubleToInt64Bits(value) < 0)
            {
                buffer[pos++] = '-';
                value = -value;
            }

            if (decimalPlaces == 0)
            {
                long rounded = (long)Math.Round(value, MidpointRounding.AwayFromZero);
                pos += IntegerToString(rounded, buffer.Slice(pos));
                return buffer.Slice(0, pos).ToString();
            }

            long integerPart = (long)value;
            double scale = Pow10(decimalPlaces);
            double fractionalPart = value - integerPart;
            long fraction =
                decimalPlaces >= 16
                    ? RoundHighPrecisionFraction(fractionalPart, decimalPlaces, (long)scale)
                    : (long)Math.Round(fractionalPart * scale, MidpointRounding.AwayFromZero);
            if (fraction >= (long)scale)
            {
                integerPart++;
                fraction = 0;
            }

            pos += IntegerToString(integerPart, buffer.Slice(pos));
            buffer[pos++] = '.';
            pos += IntegerToString(fraction, buffer.Slice(pos), decimalPlaces);
            return buffer.Slice(0, pos).ToString();
        }

        private static long RoundHighPrecisionFraction(double fraction, int places, long scale)
        {
            long bits = BitConverter.DoubleToInt64Bits(fraction);
            int biasedExponent = (int)(bits >> 52);
            int divisorShift = 1023 + 52 - biasedExponent - places;
            // 분자가 2^93 미만이므로 이 구간의 몫은 0.5 미만이다.
            if (divisorShift >= 95)
                return 0;

            long significand = (bits & ((1L << 52) - 1)) | (1L << 52);
            decimal numerator = (decimal)significand * (scale >> places);
            decimal divisor =
                (decimal)(1L << (divisorShift / 2)) * (1L << ((divisorShift + 1) / 2));
            decimal remainder = numerator % divisor;
            long whole = (long)((numerator - remainder) / divisor);
            return remainder * 2 >= divisor ? whole + 1 : whole;
        }

        private static int IntegerToString(long value, Span<char> buffer, int minLength = 1)
        {
            int pos = 0;
            while (value > 0 || pos < minLength)
            {
                buffer[pos++] = (char)('0' + value % 10);
                value /= 10;
            }
            for (int i = 0; i < pos / 2; i++)
                (buffer[i], buffer[pos - i - 1]) = (buffer[pos - i - 1], buffer[i]);

            return pos;
        }
    }
}
