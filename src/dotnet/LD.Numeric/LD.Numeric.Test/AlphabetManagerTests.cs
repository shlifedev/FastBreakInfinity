using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

public class AlphabetManagerTests
{
    [TestCase(0L, "A")]
    [TestCase(1L, "B")]
    [TestCase(25L, "Z")]
    [TestCase(26L, "AA")]
    [TestCase(27L, "AB")]
    [TestCase(51L, "AZ")]
    [TestCase(52L, "BA")]
    [TestCase(701L, "ZZ")]
    [TestCase(702L, "AAA")]
    [TestCase(703L, "AAB")]
    public void GetAlphabetUnit_KnownBoundaries(long index, string expected)
    {
        Assert.That(AlphabetManager.GetAlphabetUnit(index), Is.EqualTo(expected));
    }

    [Test]
    public void GetAlphabetUnit_NegativeIndex_ReturnsEmpty()
    {
        Assert.That(AlphabetManager.GetAlphabetUnit(-1L), Is.EqualTo(string.Empty));
        Assert.That(AlphabetManager.GetAlphabetUnit(long.MinValue), Is.EqualTo(string.Empty));
    }

    [Test]
    public void GetAlphabetUnit_IntOverload_MatchesLongOverload()
    {
        for (int i = 0; i < 100; i++)
        {
            Assert.That(
                AlphabetManager.GetAlphabetUnit(i),
                Is.EqualTo(AlphabetManager.GetAlphabetUnit((long)i))
            );
        }
    }

    [TestCase("A", 0L)]
    [TestCase("Z", 25L)]
    [TestCase("AA", 26L)]
    [TestCase("ZZ", 701L)]
    [TestCase("AAA", 702L)]
    [TestCase("CRPXNLSKVLJFHH", long.MaxValue)]
    public void GetIndexFromUnit_KnownBoundaries(string unit, long expected)
    {
        Assert.That(AlphabetManager.GetIndexFromUnit(unit), Is.EqualTo(expected));
    }

    [Test]
    public void GetIndexFromUnit_FirstUnrepresentableIndex_ThrowsOverflow()
    {
        Assert.Throws<OverflowException>(() => AlphabetManager.GetIndexFromUnit("CRPXNLSKVLJFHI"));
    }

    [Test]
    public void GetIndexFromUnit_IsInverseOfGetAlphabetUnit()
    {
        for (long i = 0; i < 2000; i++)
        {
            var unit = AlphabetManager.GetAlphabetUnit(i);
            Assert.That(
                AlphabetManager.GetIndexFromUnit(unit),
                Is.EqualTo(i),
                $"index {i} (unit '{unit}') 라운드트립 실패"
            );
        }
    }

    [Test]
    public void GetIndexFromUnit_LongUnit_NoOverflow()
    {
        var unit = AlphabetManager.GetAlphabetUnit(10_000_000_000L);
        Assert.That(AlphabetManager.GetIndexFromUnit(unit), Is.EqualTo(10_000_000_000L));
    }

    [TestCase(0L, "")]
    [TestCase(1L, "")]
    [TestCase(2L, "")]
    [TestCase(3L, "A")]
    [TestCase(4L, "A")]
    [TestCase(5L, "A")]
    [TestCase(6L, "B")]
    [TestCase(8L, "B")]
    [TestCase(9L, "C")]
    [TestCase(78L, "Z")]
    [TestCase(80L, "Z")]
    [TestCase(81L, "AA")]
    public void GetAlphabetUnitFromExponent_KnownBoundaries(long exponent, string expected)
    {
        Assert.That(AlphabetManager.GetAlphabetUnitFromExponent(exponent), Is.EqualTo(expected));
    }

    [Test]
    public void GetAlphabetUnitFromExponent_NegativeExponent_ReturnsEmpty()
    {
        Assert.That(AlphabetManager.GetAlphabetUnitFromExponent(-3L), Is.EqualTo(string.Empty));
    }

    [Test]
    public void GetAlphabetUnitFromExponent_IntOverload_MatchesLongOverload()
    {
        for (int e = 0; e < 300; e++)
        {
            Assert.That(
                AlphabetManager.GetAlphabetUnitFromExponent(e),
                Is.EqualTo(AlphabetManager.GetAlphabetUnitFromExponent((long)e))
            );
        }
    }

    [Test]
    public void GetAlphabetUnit_RepeatedCalls_ReturnSameResult()
    {
        var first = AlphabetManager.GetAlphabetUnit(12345L);
        var second = AlphabetManager.GetAlphabetUnit(12345L);
        Assert.That(second, Is.SameAs(first));
    }

    [Test]
    public void GetAlphabetUnit_ProgressingThroughUnits_BoundsRetainedStrings()
    {
        var references = Enumerable
            .Range(10000, 4096)
            .Select(index => new WeakReference<string>(AlphabetManager.GetAlphabetUnit(index)))
            .ToArray();

        GC.Collect();

        Assert.That(
            references.Count(reference => reference.TryGetTarget(out _)),
            Is.LessThanOrEqualTo(2048)
        );
    }

    [Test]
    public void GetAlphabetUnit_ConcurrentAccess_IsConsistent()
    {
        var failures = 0;
        Parallel.For(
            0,
            5000,
            i =>
            {
                long index = (i % 500) + (i % 3) * 1024;
                var unit = AlphabetManager.GetAlphabetUnit(index);
                if (AlphabetManager.GetIndexFromUnit(unit) != index)
                {
                    Interlocked.Increment(ref failures);
                }
            }
        );
        Assert.That(failures, Is.EqualTo(0));
    }
}
