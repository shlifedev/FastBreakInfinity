using System;

namespace LD.Numeric
{
    public static class SharedRandom
    {
        public static Random Random { get; } = new Random();
    }
}