using System;

namespace LD.Numeric.IdleNumber
{
    public partial struct BigDouble
    {
        private const int FractionalPartAccuracy = 6;

        private const int ExponentUnit = 3;

        public static (long rangeA, long rangeB) GetExponentFromAlphabetUnit(string unit)
        {
            long first = GetExponentFromUnitName(unit);
            return (first, checked(first + (ExponentUnit - 1)));
        }

        public static string GetAlphabetUnit(long exponent)
        {
            return AlphabetManager.GetAlphabetUnitFromExponent(exponent);
        }

        public static long GetNumberFromUnitName(string unit)
        {
            if (string.IsNullOrEmpty(unit))
            {
                throw new ArgumentException("알파벳을 올바르게 입력했나요.. 값이 비어있습니다.");
            }

            return checked(AlphabetManager.GetIndexFromUnit(unit) + 1);
        }

        public static long GetExponentFromUnitName(string unit)
        {
            return checked(GetNumberFromUnitName(unit) * ExponentUnit);
        }

        public double AdjustedMantissa()
        {
            if (Exponent < 0)
            {
                return ToDouble();
            }

            return ScaleForAlphabetUnit(Math.Round(mantissa, FractionalPartAccuracy));
        }

        private double ScaleForAlphabetUnit(double value)
        {
            long remainder = exponent % ExponentUnit;
            if (remainder == 0)
                return value;
            return remainder == 1 ? value * 10 : value * 100;
        }
    }
}
