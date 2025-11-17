using System;
using System.Text; 
 
namespace LD.Numeric.IdleNumber
{
    public partial struct BigDouble
    {
        /// <summary>
        /// BigDouble의 소수점 정확도
        /// </summary>
        static int FRACTIONAL_PART_ACCURITY = 6;  
        
        /// <summary>
        /// 몇 단위로 자를건지
        /// </summary>
        static int EXPONENT_UNIT = 3;

        public static (long rangeA, long rangeB) GetExponentFromAlphabetUnit(string unit)
        {
            if (string.IsNullOrEmpty(unit))
            {
                throw new ArgumentException("알파벳을 올바르게 입력했나요.. 값이 비어있습니다.");
            }

            long exponent = 0;
            int length = unit.Length;
            for (int i = 0; i < length; i++)
            {
                int letterValue = unit[i] - 'A' + 1;
                exponent += letterValue * (int)Math.Pow(26, length - i - 1);
            }

            return (exponent * 3, exponent * 3 + 2);
        }

        public static string GetAlphabetUnit(long exponent)
        {
            return AlphabetManager.GetAlphabetUnitFromExponent(exponent);
        }

        /// <summary>
        /// 컬럼내임을 카운트로 바꾼다.
        /// 이값에 EXPONENT UNIT 상수를 곱하면 대응되는 지수가 나온다.
        /// </summary>
        public static long GetNumberFromUnitName(string unit)
        {
            var range = GetExponentFromAlphabetUnit(unit);
            return range.rangeB / 3;
        }

        /// <summary>
        /// 컬럼내임을 카운트로 바꾼다.
        /// 이값에 EXPONENT UNIT 상수를 곱하면 대응되는 지수가 나온다.
        /// </summary>
        public static long GetExponentFromUnitName(string unit)
        {
            return GetNumberFromUnitName(unit) * EXPONENT_UNIT;
        }

        /// <summary>
        /// 1000 => 1로 변경, 10000 => 10으로 변경.
        /// 알파벳과 함께 값을 일반화해서 표현하기 위해 사용한다.
        /// </summary>
        /// <returns></returns>
        public double AdjustedMantissa()
        {
            mantissa = Math.Round(mantissa, 6);
            double mul = Math.Pow(10, Exponent % 3);
            return Mantissa * mul;
        } 
    }
}
