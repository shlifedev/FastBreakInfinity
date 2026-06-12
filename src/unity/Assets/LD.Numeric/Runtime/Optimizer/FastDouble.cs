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

        private static double Pow10(int exp)
        {
            if (exp >= 0 && exp < PositivePowersOf10.Length)
                return PositivePowersOf10[exp];
            if (exp < 0 && -exp < PositivePowersOf10.Length)
                return 1.0 / PositivePowersOf10[-exp];
            return Math.Pow(10.0, exp);
        }

        /// <summary>
        /// 스트링을 더블로 변환함. null/빈 문자열은 0을 반환하고,
        /// 그 외에 숫자가 하나도 없는 입력("-", ".", "1e" 등)은 FormatException을 던진다.
        /// </summary>
        /// <param name="s">스트링 (invariant 표기만 허용, 공백·천 단위 구분자 불가)</param>
        /// <param name="maxDecimalPlaces">
        /// 초과하는 소수 자릿수는 반올림 없이 절삭된다. 정확도를 버리고 속도를 얻는 손잡이.
        /// 지수 표기(e/E) 입력에는 적용되지 않음 — 리터럴 자릿수 기준 절삭이라 지수 스케일링을
        /// 거치면 정수부 유효숫자까지 깎이기 때문에 전체 정밀도로 파싱한다.
        /// </param>
        /// <exception cref="FormatException"></exception>
        public static double ParseDouble(string s, int maxDecimalPlaces = 6)
        {
            if (string.IsNullOrEmpty(s))
            {
                return 0;
            }
            return ParseDouble(s.AsSpan(), maxDecimalPlaces);
        }

        /// <summary>
        /// substring 할당 없이 파싱할 수 있는 오버로드.
        /// 동작은 <see cref="ParseDouble(string, int)"/>와 동일하다.
        /// </summary>
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
            int exponent = 0;
            bool negativeExponent = false;
            bool hasExponent = false;
            bool hasDecimal = false;
            bool hasMantissaDigits = false;
            bool hasExponentDigits = false;
            int decimalPlaces = 0;

            for (int i = startIndex; i < s.Length; i++)
            {
                char c = s[i];
                if (c >= '0' && c <= '9')
                {
                    if (hasExponent)
                    {
                        hasExponentDigits = true;
                        // int 오버플로 랩어라운드 방지 — 이 크기면 결과는 어차피 0 또는 Infinity
                        if (exponent < 100_000_000)
                            exponent = exponent * 10 + (c - '0');
                    }
                    else
                    {
                        hasMantissaDigits = true;
                        if (!hasDecimal || decimalPlaces < maxDecimalPlaces)
                        {
                            mantissa = mantissa * 10.0 + (c - '0');
                            if (hasDecimal)
                                decimalPlaces++;
                        }
                        // maxDecimalPlaces 초과 소수 자릿수는 절삭
                    }
                }
                else if (c == '.' && !hasDecimal && !hasExponent)
                {
                    hasDecimal = true;
                }
                else if ((c == 'e' || c == 'E') && !hasExponent)
                {
                    hasExponent = true;
                    // 지수부 부호 처리 (+/-)
                    if (i + 1 < s.Length && (s[i + 1] == '-' || s[i + 1] == '+'))
                    {
                        negativeExponent = s[i + 1] == '-';
                        i++;
                    }
                }
                else
                {
                    throw new FormatException("입력값이 잘못 됨 => " + s.ToString());
                }
            }

            // "-", ".", "1e" 같은 입력이 조용히 0/1로 로드되는 것을 막는다
            if (!hasMantissaDigits || (hasExponent && !hasExponentDigits))
            {
                throw new FormatException("입력값이 잘못 됨 => " + s.ToString());
            }

            // 소수부 보정과 지수를 합쳐 곱셈/나눗셈 한 번으로 처리 — 이중 반올림 방지
            int netExponent =
                (hasExponent ? (negativeExponent ? -exponent : exponent) : 0) - decimalPlaces;
            if (netExponent > 0)
            {
                mantissa *= Pow10(netExponent);
            }
            else if (netExponent < 0)
            {
                mantissa /= Pow10(-netExponent);
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
            // long 캐스팅 범위를 벗어나는 값은 표준 포맷터로 처리
            if (Math.Abs(value) >= 9.2e18 || decimalPlaces > 17)
            {
                return value.ToString("F" + decimalPlaces, CultureInfo.InvariantCulture);
            }

            Span<char> buffer = stackalloc char[40];
            int pos = 0;
            if (value < 0)
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
            long fraction = (long)
                Math.Round((value - integerPart) * scale, MidpointRounding.AwayFromZero);
            // 소수부 반올림이 자리올림되면 정수부로 캐리 (예: 1.999 → 2.00)
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
