using System.Numerics;
using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

/// <summary>
/// BigDouble 변환 테스트 — 암묵/명시적 변환, ToDouble/ToFloat 경계, 생성자, 해시/동등성 계약
/// </summary>
public class ConversionTests
{
    // ===== 암묵적 변환 =====

    [Test]
    public void ImplicitConversion_FromNumericTypes()
    {
        BigDouble fromInt = 42;
        BigDouble fromLong = 42L;
        BigDouble fromFloat = 42f;
        BigDouble fromDouble = 42.0;

        Assert.That(fromInt == fromLong, Is.True);
        Assert.That(fromLong == fromFloat, Is.True);
        Assert.That(fromFloat == fromDouble, Is.True);
    }

    [Test]
    public void ImplicitConversion_FromString()
    {
        BigDouble fromString = "1e100";
        Assert.That(fromString == new BigDouble(1, 100), Is.True);
    }

    [Test]
    public void ExplicitConversion_FromBigInteger()
    {
        var big = (BigDouble)new BigInteger(12345);
        Assert.That(big == new BigDouble(12345), Is.True);
    }

    [Test]
    public void ImplicitConversion_SpecialDoubles()
    {
        BigDouble nan = double.NaN;
        BigDouble posInf = double.PositiveInfinity;
        BigDouble negInf = double.NegativeInfinity;

        Assert.That(BigDouble.IsNaN(nan), Is.True);
        Assert.That(BigDouble.IsPositiveInfinity(posInf), Is.True);
        Assert.That(BigDouble.IsNegativeInfinity(negInf), Is.True);
    }

    // ===== 생성자 =====

    [Test]
    public void Constructor_StringWithExponent()
    {
        Assert.That(new BigDouble("1.5e3") == new BigDouble(1500), Is.True);
        Assert.That(new BigDouble("1E10") == new BigDouble(1, 10), Is.True);
    }

    [Test]
    public void Constructor_StringPlainNumber()
    {
        Assert.That(new BigDouble("123") == new BigDouble(123), Is.True);
    }

    [Test]
    public void Constructor_StringNaN()
    {
        Assert.That(BigDouble.IsNaN(new BigDouble("NaN")), Is.True);
    }

    [Test]
    public void Constructor_ExplicitFormat()
    {
        var withExponent = new BigDouble("2.5e6", BigDouble.eFormat.NumberWithExponent);
        Assert.That(withExponent == new BigDouble(2.5, 6), Is.True);

        var plainNumber = new BigDouble("2500", BigDouble.eFormat.Number);
        Assert.That(plainNumber == new BigDouble(2500), Is.True);
    }

    [Test]
    public void Constructor_Copy()
    {
        var original = new BigDouble(3.14, 50);
        var copy = new BigDouble(original);
        Assert.That(copy == original, Is.True);
        Assert.That(copy.Mantissa, Is.EqualTo(original.Mantissa));
        Assert.That(copy.Exponent, Is.EqualTo(original.Exponent));
    }

    [Test]
    public void Parse_InvalidPlainString_Throws()
    {
        Assert.Throws<FormatException>(() => BigDouble.Parse("abc"));
    }

    [Test]
    public void Parse_NullString_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => BigDouble.Parse(null!));
        Assert.Throws<ArgumentNullException>(() => new BigDouble((string)null!));
        Assert.Throws<ArgumentNullException>(() =>
            new BigDouble((string)null!, BigDouble.eFormat.Number)
        );
    }

    [Test]
    public void CompareTo_NullLiteral_ResolvesToStringConversion_ThrowsClearException()
    {
        // 캐스트 없는 null 리터럴은 implicit string→BigDouble 변환을 타는 오버로드로 해석됨.
        // NRE 대신 ArgumentNullException이 나와야 함. object로 캐스트하면 CompareTo(object)로 감.
        string? nullString = null;
        Assert.Throws<ArgumentNullException>(() => new BigDouble(5).CompareTo(nullString!));
    }

    // ===== ToDouble 경계 =====

    [Test]
    public void ToDouble_ExponentAboveDoubleRange_ReturnsInfinity()
    {
        var positive = BigDouble.FromMantissaExponentNoNormalize(1, 309);
        var negative = BigDouble.FromMantissaExponentNoNormalize(-1, 309);
        Assert.That(positive.ToDouble(), Is.EqualTo(double.PositiveInfinity));
        Assert.That(negative.ToDouble(), Is.EqualTo(double.NegativeInfinity));
    }

    [Test]
    public void ToDouble_ExponentBelowDoubleRange_ReturnsZero()
    {
        var tiny = BigDouble.FromMantissaExponentNoNormalize(1, -325);
        Assert.That(tiny.ToDouble(), Is.EqualTo(0.0));
    }

    [Test]
    public void ToDouble_DenormalBoundary()
    {
        // -324 지수는 5e-324(비정규화 최솟값)로 별도 처리됨
        var positive = BigDouble.FromMantissaExponentNoNormalize(1, -324);
        var negative = BigDouble.FromMantissaExponentNoNormalize(-1, -324);
        Assert.That(positive.ToDouble(), Is.EqualTo(5e-324));
        Assert.That(negative.ToDouble(), Is.EqualTo(-5e-324));
    }

    [Test]
    public void ToDouble_IntegerValues_RoundTripExactly()
    {
        Assert.That(new BigDouble(123456789).ToDouble(), Is.EqualTo(123456789.0));
        Assert.That(new BigDouble(-42).ToDouble(), Is.EqualTo(-42.0));
    }

    [Test]
    public void ToDouble_NaN()
    {
        Assert.That(BigDouble.NaN.ToDouble(), Is.EqualTo(double.NaN));
    }

    [Test]
    public void ToFloat_BeyondFloatRange_ReturnsInfinity()
    {
        Assert.That(new BigDouble(1, 100).ToFloat(), Is.EqualTo(float.PositiveInfinity));
        Assert.That(new BigDouble(-1, 100).ToFloat(), Is.EqualTo(float.NegativeInfinity));
    }

    [Test]
    public void ToFloat_NormalRange()
    {
        Assert.That(new BigDouble(1.5, 10).ToFloat(), Is.EqualTo(1.5e10f).Within(1e4f));
    }

    // ===== ToStringMantissaExponent =====

    [Test]
    public void ToStringMantissaExponent_Format()
    {
        Assert.That(new BigDouble(1.5, 100).ToStringMantissaExponent(), Is.EqualTo("1.5e100"));
        Assert.That(BigDouble.Zero.ToStringMantissaExponent(), Is.EqualTo("0e0"));
    }

    // ===== Equals(object) / GetHashCode / CompareTo(object) 계약 =====

    [Test]
    public void EqualsObject_NonBigDouble_ReturnsFalse()
    {
        // 캐스트 없이 넘기면 implicit 변환 때문에 Equals(BigDouble) 오버로드로 가버림
        Assert.That(new BigDouble(5).Equals((object)"5"), Is.False);
        Assert.That(new BigDouble(5).Equals((object)null!), Is.False);
        Assert.That(
            new BigDouble(5).Equals((object)5.0),
            Is.False,
            "박싱된 double은 BigDouble이 아님"
        );
    }

    [Test]
    public void EqualsObject_BoxedBigDouble()
    {
        object boxed = new BigDouble(5);
        Assert.That(new BigDouble(5).Equals(boxed), Is.True);
    }

    [Test]
    public void GetHashCode_EqualValues_HaveEqualHashes()
    {
        // 10e99와 1e100은 정규화 후 같은 값
        var a = new BigDouble(10, 99);
        var b = new BigDouble(1, 100);
        Assert.That(a == b, Is.True);
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void GetHashCode_DefaultEqualsZero()
    {
        Assert.That(default(BigDouble) == BigDouble.Zero, Is.True);
        Assert.That(default(BigDouble).GetHashCode(), Is.EqualTo(BigDouble.Zero.GetHashCode()));
    }

    [Test]
    public void CompareToObject_Null_ReturnsPositive()
    {
        Assert.That(new BigDouble(5).CompareTo((object)null!), Is.EqualTo(1));
    }

    [Test]
    public void CompareToObject_WrongType_Throws()
    {
        Assert.Throws<ArgumentException>(() => new BigDouble(5).CompareTo((object)"5"));
    }

    [Test]
    public void CompareToObject_BoxedBigDouble()
    {
        object boxed = new BigDouble(10);
        Assert.That(new BigDouble(5).CompareTo(boxed), Is.EqualTo(-1));
    }
}
