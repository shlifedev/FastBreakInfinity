using System;
using System.Globalization;
using Cysharp.Text;

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
        /// 스트링을 더블로 변환함
        /// </summary>
        /// <param name="s">스트링</param>
        /// <param name="maxDecimalPlaces">최대 소수점. 정확도의 개념임.</param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static double ParseDouble(string s, int maxDecimalPlaces = 6)
        {
            if (string.IsNullOrEmpty(s))
            {
                return 0;
            }
            if (maxDecimalPlaces < 0)
                maxDecimalPlaces = 0;
            bool isNegative = s[0] == '-';
            int startIndex = isNegative || s[0] == '+' ? 1 : 0;

            double mantissa = 0.0;
            int exponent = 0;
            bool negativeExponent = false;
            bool hasExponent = false;
            bool hasDecimal = false;
            int decimalPlaces = 0;

            for (int i = startIndex; i < s.Length; i++)
            {
                char c = s[i];
                if (c >= '0' && c <= '9')
                {
                    if (hasExponent)
                    {
                        // int 오버플로 랩어라운드 방지 — 이 크기면 결과는 어차피 0 또는 Infinity
                        if (exponent < 100_000_000)
                            exponent = exponent * 10 + (c - '0');
                    }
                    else if (decimalPlaces < maxDecimalPlaces || !hasDecimal)
                    {
                        mantissa = mantissa * 10.0 + (c - '0');
                        if (hasDecimal)
                            decimalPlaces++;
                    }
                    else if (decimalPlaces >= maxDecimalPlaces)
                    {
                        // maxDecimalPlaces 초과 숫자는 무시하되 루프는 계속 (지수부 파싱 필요)
                        continue;
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
                    throw new FormatException("입력값이 잘못 됨 => " + s);
                }
            }

            if (hasDecimal)
            {
                mantissa /= Pow10(decimalPlaces);
            }

            if (hasExponent)
            {
                mantissa *= Pow10(negativeExponent ? -exponent : exponent);
            }
            return isNegative ? -mantissa : mantissa;
        }

        public static string OptimizeToString(this double value, int decimalPlaces)
        {
            if (decimalPlaces == 0)
                return value.ToString("0");
            if (decimalPlaces < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(decimalPlaces),
                    "Decimal places 는 음수일 수 없습니다."
                );

            if (double.IsNaN(value))
                return "NaN";
            if (double.IsInfinity(value))
                return value > 0 ? "Infinity" : "-Infinity";
            if (value == 0)
            {
                using (var sb = ZString.CreateStringBuilder())
                {
                    sb.Append("0.");
                    sb.Append('0', decimalPlaces);
                    return sb.ToString();
                }
            }
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

            long integerPart = (long)value;
            double scale = Pow10(decimalPlaces);
            long fraction = (long)Math.Round((value - integerPart) * scale);
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
