using System;
using System.Globalization;

namespace LD.Numeric.IdleNumber
{
    public struct BigValueInfo
    {
        public double Mantissa { get; set; }
        public long Exponent { get; set; }

        public static BigValueInfo ExponentFormatToBigValueInfo(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new FormatException("빈 문자열은 파싱할 수 없습니다.");
            }

            double mantissa = 0;
            long exponent = 0;
            int decimalDigits = 0;
            int mantissaLength = input.Length;
            bool isExponent = false;
            bool isExponentNegative = false;
            bool isMantissaNegative = false;
            bool isDecimal = false;
            bool hasMantissaDigit = false;
            bool hasExponentDigit = false;

            for (var index = 0; index < input.Length; index++)
            {
                var c = input[index];
                if (c == 'e' || c == 'E')
                {
                    if (isExponent)
                    {
                        throw new FormatException("잘못된 입력: " + input);
                    }
                    mantissaLength = index;
                    isExponent = true;
                    continue;
                }

                if (c == '+' || c == '-')
                {
                    // 부호는 가수 맨 앞 또는 지수(e 바로 뒤)에서만 허용
                    bool validPosition =
                        index == 0 || input[index - 1] == 'e' || input[index - 1] == 'E';
                    if (!validPosition)
                    {
                        throw new FormatException("잘못된 입력: " + input);
                    }

                    if (c == '-')
                    {
                        if (isExponent)
                            isExponentNegative = true;
                        else
                            isMantissaNegative = true;
                    }
                    continue;
                }

                if (c == '.')
                {
                    if (isDecimal || isExponent)
                    {
                        throw new FormatException("잘못된 입력: " + input);
                    }
                    isDecimal = true;
                    continue;
                }

                if (c < '0' || c > '9')
                {
                    throw new FormatException("잘못된 입력: " + input);
                }

                int digit = c - '0';
                if (isExponent)
                {
                    hasExponentDigit = true;
                    // 음수로 누적하면 양수로 표현할 수 없는 long.MinValue의 절댓값도 처리한다.
                    long limit = isExponentNegative ? long.MinValue : -long.MaxValue;
                    if (exponent < (limit + digit) / 10)
                    {
                        throw new FormatException("지수가 표현 범위를 벗어남: " + input);
                    }
                    exponent = exponent * 10 - digit;
                }
                else
                {
                    hasMantissaDigit = true;
                    mantissa = mantissa * 10 + digit;
                    if (isDecimal)
                    {
                        decimalDigits++;
                    }
                }
            }

            if (!hasMantissaDigit || (isExponent && !hasExponentDigit))
            {
                throw new FormatException("잘못된 입력: " + input);
            }

            // 정수 누적값과 나눗수가 double에 정확히 들어갈 때만 한 번의 나눗셈으로 복원한다.
            if (mantissa >= 9007199254740992d || decimalDigits > 22)
            {
                mantissa = double.Parse(
                    input.AsSpan(0, mantissaLength),
                    NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture
                );
            }
            else
            {
                if (decimalDigits > 0)
                    mantissa /= FastDouble.Pow10(decimalDigits);
                if (isMantissaNegative)
                    mantissa = -mantissa;
            }

            return new BigValueInfo
            {
                Exponent = isExponentNegative ? exponent : -exponent,
                Mantissa = mantissa,
            };
        }
    }
}
