using System;

namespace LD.Numeric.Runtime
{
    public static class RandomEx
    {
        public static long NextLong64(this Random rand, long min, long max ) {
            long result = rand.Next((Int32)(min >> 32), (Int32)(max >> 32));
            result = (result << 32);
            result = result | (long)rand.Next((Int32)min, (Int32)max);
            return result;
        }
    }
}