using System;

namespace LD.Numeric.IdleNumber
{
    public partial struct BigDouble
    {
        /// <summary>
        /// BigDouble의 소수점 정확도
        /// </summary>
        private const int FractionalPartAccuracy = 6;

        /// <summary>
        /// 몇 단위로 자를건지 (1000 단위 = 3자리)
        /// </summary>
        private const int ExponentUnit = 3;

        public static (long rangeA, long rangeB) GetExponentFromAlphabetUnit(string unit)
        {
            if (string.IsNullOrEmpty(unit))
            {
                throw new ArgumentException("알파벳을 올바르게 입력했나요.. 값이 비어있습니다.");
            }

            // AlphabetManager는 0-indexed(A=0)이므로 +1해서 1-indexed 값으로 환산.
            // (int)Math.Pow(26, n) 방식은 7글자 이상 단위에서 int 오버플로가 났음
            long exponent = AlphabetManager.GetIndexFromUnit(unit) + 1;

            return (exponent * ExponentUnit, exponent * ExponentUnit + 2);
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
            return range.rangeB / ExponentUnit;
        }

        /// <summary>
        /// 컬럼내임을 카운트로 바꾼다.
        /// 이값에 EXPONENT UNIT 상수를 곱하면 대응되는 지수가 나온다.
        /// </summary>
        public static long GetExponentFromUnitName(string unit)
        {
            return GetNumberFromUnitName(unit) * ExponentUnit;
        }

        /// <summary>
        /// 1000 => 1로 변경, 10000 => 10으로 변경.
        /// 알파벳과 함께 값을 일반화해서 표현하기 위해 사용한다.
        /// </summary>
        /// <returns></returns>
        public double AdjustedMantissa()
        {
            // 음수 지수엔 알파벳 단위가 없음 — 3자리 보정을 적용하면 0.5가 500이 됨
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
