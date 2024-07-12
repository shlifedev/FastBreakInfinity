using System.Collections.Generic;
using System.Globalization;

namespace LD.Numeric
{
    public static class AlphabetManager
    {
        static Dictionary<long, string> unitCache = new Dictionary<long, string>();
        static Dictionary<string, long> reverseUnitCache = new Dictionary<string, long>();

        /// <summary>
        /// Exponent 인덱스를 가져 옴 = A = 0, B = 1, C = 2
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static long GetIndexFromUnit(string unit)
        {
            if (string.IsNullOrEmpty(unit)) return 0;
            if (reverseUnitCache.ContainsKey(unit))
            {
                return reverseUnitCache[unit];
            }

            int exponent = 0;
            foreach (char c in unit)
            {
                exponent = exponent * 26 + (c - 'A' + 1);
            }

            reverseUnitCache[unit] = exponent;

            return exponent;
        }


        /// <summary>
        /// 알파벳을 반환합니다. 인덱스는 0 = A , 1 = B , 2 = C, 27 = AA 로 1마다 계산됩니다. 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string GetAlphabetUnit(long index)
        {
            if (index < 0) return string.Empty;
            if (unitCache.ContainsKey(index))
            {
                return unitCache[index];
            }

            string unit = "";
            long originalIndex = index;
            while (index >= 0)
            {
                unit = (char)('A' + index % 26) + unit;
                index = index / 26 - 1;
            }

            unitCache[originalIndex] = unit;
            reverseUnitCache[unit] = originalIndex;
            return unit;
        }


        public static string GetAlphabetUnit(int index) => GetAlphabetUnit(index);

        /// <summary>
        /// -1~2 사이의 값을 입력시 빈 스트링이 반환됩니다. 
        ///  지수를 통해 알파벳 반환 3~5 = A, 6~8 = B, 9~11 = C . . .
        /// </summary>
        /// <param name="exponent"></param>
        /// <returns></returns>
        public static string GetAlphabetUnitFromExponent(int exponent)
        {
            var index = (exponent / 3) - 1;
            return GetAlphabetUnit(index);
        }

        public static string GetAlphabetUnitFromExponent(long exponent)
        {
            var index = (exponent / 3) - 1;
            return GetAlphabetUnit(index);
        }
    }
}