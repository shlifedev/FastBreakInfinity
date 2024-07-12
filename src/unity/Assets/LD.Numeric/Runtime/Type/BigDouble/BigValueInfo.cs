namespace LD
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
                        continue; // Ignore the '+' sign
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
                        continue; // Ignore the '+' sign
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

            return new BigValueInfo()
            {
                Exponent = exponent,
                Mantissa = mantissa
            };
        }
    }
}