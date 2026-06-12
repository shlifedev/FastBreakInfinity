using System;

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
            double decimalFactor = 1;
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
                    if (exponent > (long.MaxValue - digit) / 10)
                    {
                        throw new FormatException("지수가 표현 범위를 벗어남: " + input);
                    }
                    exponent = exponent * 10 + digit;
                }
                else
                {
                    hasMantissaDigit = true;
                    if (isDecimal)
                    {
                        decimalFactor *= 0.1;
                        mantissa += digit * decimalFactor;
                    }
                    else
                    {
                        mantissa = mantissa * 10 + digit;
                    }
                }
            }

            if (!hasMantissaDigit || (isExponent && !hasExponentDigit))
            {
                throw new FormatException("잘못된 입력: " + input);
            }

            if (isMantissaNegative)
            {
                mantissa = -mantissa;
            }

            if (isExponentNegative)
            {
                exponent = -exponent;
            }

            return new BigValueInfo() { Exponent = exponent, Mantissa = mantissa };
        }
    }
}
