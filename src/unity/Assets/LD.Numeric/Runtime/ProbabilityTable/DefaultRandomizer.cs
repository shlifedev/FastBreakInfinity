using System;
using LD.Numeric.Runtime;

namespace LD.Numeric.ProbabilityTable
{
    
    public class DefaultRandomizer : IRandomizer
    { 
        public long GetRandomValue(long min, long max)
        {
            return SharedRandom.Random.NextLong64(min, max);
        }
         
    }
}