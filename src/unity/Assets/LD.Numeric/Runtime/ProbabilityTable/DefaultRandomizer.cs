using System;

namespace LD.Numeric
{
    
    public class DefaultRandomizer : IRandomizer
    { 
        public long GetRandomValue(long min, long max)
        {
            return SharedRandom.Random.NextLong64(min, max);
        }
         
    }
}