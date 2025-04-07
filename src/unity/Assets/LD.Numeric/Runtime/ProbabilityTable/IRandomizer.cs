using System;

namespace LD.Numeric
{
    public interface IRandomizer
    {
        long GetRandomValue(long min, long max);
    }
}