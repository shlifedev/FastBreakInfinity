using System;
using System.Globalization;
using System.Linq;

namespace LD.Numeric
{ 
    public static class FastDouble
    { 
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
                        exponent = exponent * 10 + (c - '0');
                    }
                    else if (decimalPlaces < maxDecimalPlaces || !hasDecimal)
                    {
                        mantissa = mantissa * 10.0 + (c - '0');
                        if (hasDecimal) decimalPlaces++;
                    }
                    else if (decimalPlaces == maxDecimalPlaces)
                    {
                        //소수점 이후에 지수부가 나올 예정이기 때문에 break해도 됨~~
                        break;
                    }
                }
                else if (c == '.' && !hasDecimal && !hasExponent)
                {
                    hasDecimal = true;
                }
                else if ((c == 'e' || c == 'E') && !hasExponent)
                {
                    hasExponent = true;
                    if (i + 1 < s.Length && (s[i + 1] == '-' || s[i + 1] == '+'))
                    {
                        if(s[i+1] == '-') 
                            negativeExponent = true; 
                        exponent = s[i + 1] == '-' ? -exponent : exponent; 
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
                mantissa /= Math.Pow(10.0, decimalPlaces);
            }

            if (hasExponent)
            {
                mantissa *= Math.Pow(10.0, negativeExponent ? -exponent : exponent);
            } 
            return isNegative ? -mantissa : mantissa;
        }
        
         
        public static string OptimizeToString(this double value, int decimalPlaces)
        {
            value = Math.Round(value, 3);
            if (decimalPlaces == 0) return value.ToString("0");
            if (decimalPlaces < 0)
                throw new ArgumentOutOfRangeException(nameof(decimalPlaces), "Decimal places cannot be negative.");

             
            if (double.IsNaN(value)) return "NaN";
            if (double.IsInfinity(value)) return value > 0 ? "Infinity" : "-Infinity";
            if (value == 0) return "0." + new string('0', decimalPlaces);

            Span<char> buffer = stackalloc char[32];  
            int pos = 0; 
            if (value < 0)
            {
                buffer[pos++] = '-';
                value = -value;
            }
 
            long integerPart = (long)value;
            double fractionalPart = value - integerPart;  
            pos += IntegerToString(integerPart, buffer.Slice(pos)); 
            buffer[pos++] = '.'; 
            fractionalPart = Math.Round(fractionalPart * Math.Pow(10, decimalPlaces), decimalPlaces);
            try
            {
                pos += IntegerToString((long)fractionalPart, buffer.Slice(pos), decimalPlaces);
            }
            catch(Exception e)
            { 
                throw e;
            }

            return new string(buffer.Slice(0, pos));
        }

        private static int IntegerToString(long value, Span<char> buffer, int minLength = 1)
        {
            int pos = 0;
            while (value > 0 || pos < minLength)
            {
                buffer[pos++] = (char)('0' + value % 10);
                value /= 10;
            }

            // Reverse the characters
            for (int i = 0; i < pos / 2; i++)
            {
                (buffer[i], buffer[pos - i - 1]) = (buffer[pos - i - 1], buffer[i]);
            }

            return pos;
        }
        
    }
}