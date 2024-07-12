using System;

namespace LD.Numeric.ProbabilityTable
{
    public interface IRandomizer
    {
        long GetRandomValue(long min, long max);
    }
}