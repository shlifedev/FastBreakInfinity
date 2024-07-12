using System;
using System.Globalization;
using System.Numerics; 
using LD;

namespace LD.Numeric
{


    public static partial class AlphabetConverter
    {
        public static long GetExponent(double value)
        {
            long exponent = (int)(Math.Log10(value) / 3) * 3;
            return exponent;
        }

        public static long GetExponent(int value)
        {
            long exponent = (int)(Math.Log10(value) / 3) * 3;
            return exponent;
        }

        public static long GetExponent(float value)
        {
            long exponent = (int)(Math.Log10(value) / 3) * 3;
            return exponent;
        }

        public static long GetExponent(long value)
        {
            long exponent = (int)(Math.Log10(value) / 3) * 3;
            return exponent;
        }

        public static long GetExponent(BigInteger value)
        {
            long exponent = (int)(BigInteger.Log10(value) / 3) * 3;
            return exponent;
        }



        #region 알파벳포함값 => 일반값 역함수

        public static double ConvertFromAlphabetUnit(string str)
        {
            int lastAlphaIndex = str.Length - 1;
            while (lastAlphaIndex >= 0 && char.IsLetter(str[lastAlphaIndex]))
                lastAlphaIndex--;

            string numberPart = str.Substring(0, lastAlphaIndex + 1);
            string unitPart = str.Substring(lastAlphaIndex + 1);
            double number = double.Parse(numberPart, CultureInfo.InvariantCulture);
            long exponent = AlphabetManager.GetIndexFromUnit(unitPart) * 3;
            return number * Math.Pow(10, exponent);
        }

        #endregion

        #region Value To Alphabet

        public static string ConvertToAlphabetUnit(this int number,int maxDecimalPoint = 2)
        {
            if (number < 1000)
                return number.ToString(CultureInfo.InvariantCulture);
            long exponent = GetExponent(number);

            double divisor = Math.Pow(10, exponent);
            double newNumber = (number / divisor); 
            string unit = AlphabetManager.GetAlphabetUnitFromExponent(exponent);
            return $"{newNumber.ToString($"F{maxDecimalPoint}")}{unit}";
        }

        public static string ConvertToAlphabetUnit(this float number, int maxDecimalPoint = 2)
        {
            return ConvertToAlphabetUnit((double)number, maxDecimalPoint);
        }

        public static string ConvertToAlphabetUnit(this double number, int maxDecimalPoint = 2)
        {
            if (number < 1000)
                return number.ToString(CultureInfo.InvariantCulture);
            long exponent = GetExponent(number);

            double divisor = Math.Pow(10, exponent);
            double newNumber = number / divisor;
            string unit = AlphabetManager.GetAlphabetUnitFromExponent(exponent);
            return $"{newNumber.ToString($"F{maxDecimalPoint}")}{unit}";
        }

        #endregion
    }
}