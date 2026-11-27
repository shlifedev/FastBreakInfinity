using System;
using System.Threading;

namespace LD.Numeric.IdleNumber
{
    public static class AlphabetManager
    {
        private const int MaxUnitLength = 16;
        private const int CacheSize = 1024;

        private sealed class CacheEntry
        {
            public readonly long Index;
            public readonly string Unit;

            public CacheEntry(long index, string unit)
            {
                Index = index;
                Unit = unit;
            }
        }

        private static readonly CacheEntry[] UnitCache = new CacheEntry[CacheSize];
        private static readonly CacheEntry[] ReverseUnitCache = new CacheEntry[CacheSize];

#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(
            UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration
        )]
#endif
        internal static void Reset()
        {
            Array.Clear(UnitCache, 0, UnitCache.Length);
            Array.Clear(ReverseUnitCache, 0, ReverseUnitCache.Length);
        }

        public static long GetIndexFromUnit(string unit)
        {
            if (string.IsNullOrEmpty(unit))
                return 0;
            int slot = unit.GetHashCode() & (CacheSize - 1);
            var cached = Volatile.Read(ref ReverseUnitCache[slot]);
            if (cached != null && cached.Unit == unit)
            {
                return cached.Index;
            }

            long index = GetIndexFromUnit(unit.AsSpan());
            CacheUnit(index, unit, slot);
            return index;
        }

        internal static long GetIndexFromUnit(ReadOnlySpan<char> unit)
        {
            if (unit.IsEmpty)
                return 0;

            long index = -1;
            foreach (char c in unit)
            {
                if (c < 'A' || c > 'Z')
                {
                    throw new FormatException(
                        "알파벳 단위는 대문자 A~Z만 허용됩니다 => " + unit.ToString()
                    );
                }
                index = checked((index + 1) * 26 + (c - 'A'));
            }

            return index;
        }

        public static string GetAlphabetUnit(long index)
        {
            if (index < 0)
                return string.Empty;
            var cached = Volatile.Read(ref UnitCache[index & (CacheSize - 1)]);
            if (cached != null && cached.Index == index)
            {
                return cached.Unit;
            }

            Span<char> buffer = stackalloc char[MaxUnitLength];
            int position = buffer.Length;
            long originalIndex = index;
            while (index >= 0)
            {
                buffer[--position] = (char)('A' + index % 26);
                index = index / 26 - 1;
            }

            string unit = buffer.Slice(position).ToString();
            CacheUnit(originalIndex, unit, unit.GetHashCode() & (CacheSize - 1));
            return unit;
        }

        private static void CacheUnit(long index, string unit, int reverseSlot)
        {
            var entry = new CacheEntry(index, unit);
            Volatile.Write(ref UnitCache[index & (CacheSize - 1)], entry);
            Volatile.Write(ref ReverseUnitCache[reverseSlot], entry);
        }

        public static string GetAlphabetUnit(int index) => GetAlphabetUnit((long)index);

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
