using System;

namespace LD.Numeric.Runtime
{
    public static class SharedRandom
    {
        public static Random Random { get; } = new Random();
    }
}