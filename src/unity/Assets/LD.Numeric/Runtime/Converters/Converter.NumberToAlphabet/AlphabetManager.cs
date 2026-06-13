using System;
using System.Collections.Concurrent;

namespace LD.Numeric.IdleNumber
{
    public static class AlphabetManager
    {
        static ConcurrentDictionary<long, string> unitCache =
            new ConcurrentDictionary<long, string>();
        static ConcurrentDictionary<string, long> reverseUnitCache =
            new ConcurrentDictionary<string, long>();

#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(
            UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration
        )]
#endif
        internal static void Reset()
        {
            unitCache.Clear();
            reverseUnitCache.Clear();
        }

        /// <summary>
        /// 알파벳 단위를 0-indexed 인덱스로 변환 (A=0, B=1, ..., Z=25, AA=26)
        /// GetAlphabetUnit의 역함수.
        /// </summary>
        public static long GetIndexFromUnit(string unit)
        {
            if (string.IsNullOrEmpty(unit))
                return 0;
            if (reverseUnitCache.TryGetValue(unit, out var cached))
            {
                return cached;
            }

            long index = 0;
            foreach (char c in unit)
            {
                // 소문자/공백이 c-'A' 계산을 그대로 타면 쓰레기 지수가 됨 ("1.5a" → 1.5e99)
                if (c < 'A' || c > 'Z')
                {
                    throw new FormatException("알파벳 단위는 대문자 A~Z만 허용됩니다 => " + unit);
                }
                index = index * 26 + (c - 'A' + 1);
            }

            // 1-indexed → 0-indexed 변환 (A=1→0, B=2→1, AA=27→26)
            index -= 1;

            reverseUnitCache[unit] = index;

            return index;
        }

        /// <summary>
        /// 알파벳을 반환합니다. 인덱스는 0 = A , 1 = B , 2 = C, 26 = AA 로 1마다 계산됩니다.
        /// </summary>
        public static string GetAlphabetUnit(long index)
        {
            if (index < 0)
                return string.Empty;
            if (unitCache.TryGetValue(index, out var cached))
            {
                return cached;
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

        public static string GetAlphabetUnit(int index) => GetAlphabetUnit((long)index);

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
