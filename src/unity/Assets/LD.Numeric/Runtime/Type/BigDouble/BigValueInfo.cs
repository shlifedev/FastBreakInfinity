namespace LD.Numeric.IdleNumber
{
    public struct BigValueInfo
    {
        public double Mantissa { get; set; }
        public long Exponent { get; set; }

        public static BigValueInfo ExponentFormatToBigValueInfo(string input)
        {
            double mantissa = 0;
            long exponent = 0;
            double decimalFactor = 1;
            bool isExponent = false;
            bool isExponentNegative = false;
            bool isMantissaNegative = false;
            bool isDecimal = false;

            for (var index = 0; index < input.Length; index++)
            {
                var c = input[index];
                if (c == 'e' || c == 'E')
                {
                    isExponent = true;
                    continue;
                }

                if (!isExponent)
                {
                    if (c == '.')
                    {
                        isDecimal = true;
                        continue;
                    }

                    if (c == '-')
                    {
                        isMantissaNegative = true;
                        continue;
                    }

                    if (c == '+')
                    {
                        continue;
                    }

                    // 숫자 문자 검증 (0-9 범위만 허용)
                    if (c < '0' || c > '9')
                    {
                        continue;
                    }

                    int digit = c - '0';
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
                else
                {
                    if (c == '-')
                    {
                        isExponentNegative = true;
                        continue;
                    }

                    if (c == '+')
                    {
                        continue;
                    }

                    // 숫자 문자 검증 (0-9 범위만 허용)
                    if (c < '0' || c > '9')
                    {
                        continue;
                    }

                    exponent = exponent * 10 + (c - '0');
                }
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
