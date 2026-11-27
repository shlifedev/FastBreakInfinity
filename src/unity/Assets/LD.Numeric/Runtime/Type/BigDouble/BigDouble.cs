using System;
using System.Globalization;
using System.Numerics;
using System.Threading;
using Random = System.Random;

namespace LD.Numeric.IdleNumber
{
    public partial struct BigDouble : IComparable<BigDouble>, IEquatable<BigDouble>
    {
        public const double Tolerance = 1e-18;

        private const int MaxSignificantDigits = 17;

        private const long ExpLimit = long.MaxValue;

        private const long DoubleExpMax = 308;

        private const long DoubleExpMin = -324;

        private const double AdditionPrecisionFactor = 1e14;
        private const int AdditionPrecisionExponent = 14;

        private const double Ln10 = 2.30258509299404568402;
        private const double Log2Of10 = 3.32192809488736234787;
        private const double Sqrt10 = 3.16227766016838;
        private const double CubeRoot10 = 2.1544346900318837;
        private const double TwoThirdsPowerOf10 = 4.6415888336127789;

        private double mantissa;

        private long exponent;

        private BigDouble(double mantissa, long exponent, PrivateConstructorArg _)
        {
            this.mantissa = mantissa;
            this.exponent = exponent;
        }

        public BigDouble(double mantissa, long exponent)
        {
            this = Normalize(mantissa, exponent);
        }

        public BigDouble(BigDouble other)
        {
            mantissa = other.mantissa;
            exponent = other.exponent;
        }

        public BigDouble(double value)
        {
            if (double.IsNaN(value))
            {
                this = NaN;
            }
            else if (double.IsPositiveInfinity(value))
            {
                this = PositiveInfinity;
            }
            else if (double.IsNegativeInfinity(value))
            {
                this = NegativeInfinity;
            }
            else if (IsZero(value))
            {
                this = Zero;
            }
            else
            {
                this = Normalize(value, 0);
            }
        }

        public static BigDouble Normalize(double mantissa, long exponent)
        {
            if (!IsFinite(mantissa))
            {
                if (double.IsNaN(mantissa))
                    return NaN;
                return mantissa > 0 ? PositiveInfinity : NegativeInfinity;
            }
            var absMantissa = Math.Abs(mantissa);
            if (absMantissa >= 1 && absMantissa < 10)
            {
                return FromMantissaExponentNoNormalize(mantissa, exponent);
            }
            if (IsZero(mantissa))
            {
                return Zero;
            }

            if (absMantissa >= 10 && absMantissa < 100)
            {
                return WithExponentOffset(mantissa / 10, exponent, 1);
            }

            if (absMantissa >= 0.1 && absMantissa < 1)
            {
                return WithExponentOffset(mantissa * 10, exponent, -1);
            }

            long tempExponent = (long)Math.Floor(Math.Log10(Math.Abs(mantissa)));
            // 비정규수는 나누기 전에 확대해 제수의 정밀도를 지킨다.
            if (tempExponent < -307)
            {
                mantissa = mantissa * 1e300 / PowersOf10.Lookup(tempExponent + 300);
            }
            else
            {
                mantissa = mantissa / PowersOf10.Lookup(tempExponent);
            }

            absMantissa = Math.Abs(mantissa);
            if (absMantissa >= 10)
            {
                mantissa /= 10;
                tempExponent++;
            }
            else if (absMantissa < 1)
            {
                mantissa *= 10;
                tempExponent--;
            }
            return WithExponentOffset(mantissa, exponent, tempExponent);
        }

        private static BigDouble WithExponentOffset(double mantissa, long exponent, long offset)
        {
            if (offset > 0 && exponent > long.MaxValue - offset)
                return mantissa > 0 ? PositiveInfinity : NegativeInfinity;
            if (offset < 0 && exponent < long.MinValue - offset)
                return Zero;
            return FromMantissaExponentNoNormalize(mantissa, exponent + offset);
        }

        private static BigDouble NormalizeWideExponent(double mantissa, decimal exponent)
        {
            var normalized = Normalize(mantissa, 0);
            if (!IsFinite(normalized.Mantissa) || IsZero(normalized.Mantissa))
                return normalized;
            exponent += normalized.Exponent;
            if (exponent > long.MaxValue)
                return mantissa > 0 ? PositiveInfinity : NegativeInfinity;
            if (exponent < long.MinValue)
                return Zero;
            return FromMantissaExponentNoNormalize(normalized.Mantissa, (long)exponent);
        }

        public double Mantissa => mantissa;

        public long Exponent => exponent;

        public static BigDouble FromMantissaExponentNoNormalize(double mantissa, long exponent)
        {
            return new BigDouble(mantissa, exponent, new PrivateConstructorArg());
        }

        public static readonly BigDouble Zero = FromMantissaExponentNoNormalize(0, 0);

        public static readonly BigDouble One = FromMantissaExponentNoNormalize(1, 0);

        public static readonly BigDouble NaN = FromMantissaExponentNoNormalize(
            double.NaN,
            long.MinValue
        );

        public static bool IsNaN(BigDouble value)
        {
            return double.IsNaN(value.Mantissa);
        }

        public static readonly BigDouble PositiveInfinity = FromMantissaExponentNoNormalize(
            double.PositiveInfinity,
            0
        );

        public static bool IsPositiveInfinity(BigDouble value)
        {
            return double.IsPositiveInfinity(value.Mantissa);
        }

        public static readonly BigDouble NegativeInfinity = FromMantissaExponentNoNormalize(
            double.NegativeInfinity,
            0
        );

        public static bool IsNegativeInfinity(BigDouble value)
        {
            return double.IsNegativeInfinity(value.Mantissa);
        }

        public static bool IsInfinity(BigDouble value)
        {
            return double.IsInfinity(value.Mantissa);
        }

        public static BigDouble Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            int exponentMarker = value.AsSpan().IndexOfAny('e', 'E');
            if (exponentMarker >= 0)
            {
                var info = BigValueInfo.Parse(value, exponentMarker);
                return Normalize(info.Mantissa, info.Exponent);
            }

            return new BigDouble(FastDouble.ParseDouble(value, FractionalPartAccuracy));
        }

        public float ToFloat()
        {
            return (float)ToDouble();
        }

        public double ToDouble()
        {
            if (!IsFinite(Mantissa) || Mantissa == 0)
            {
                return Mantissa;
            }

            if (Exponent > DoubleExpMax)
            {
                return Mantissa > 0 ? double.PositiveInfinity : double.NegativeInfinity;
            }

            if (Exponent < DoubleExpMin)
            {
                return Mantissa < 0 ? -0.0 : 0.0;
            }

            if (Exponent < -307)
            {
                return Mantissa * PowersOf10.Lookup(Exponent + 300) / 1e300;
            }

            var scale = PowersOf10.Lookup(Exponent);
            var result = Mantissa * scale;
            if (!IsFinite(result) || Exponent < 0)
            {
                return result;
            }

            var integer = Math.Round(result);
            if (integer != result && integer / scale == Mantissa)
                return integer;
            return result;
        }

        internal string CreateString(int decimalCount) => FormatForDisplay(decimalCount);

        private string FormatForDisplay(int? fixedDecimalCount)
        {
            if (!IsFinite(mantissa))
                return mantissa.ToString(CultureInfo.InvariantCulture);
            if (exponent < DoubleExpMin)
                return ToStringMantissaExponent();
            if (exponent < 0)
                return ToDouble().ToString(CultureInfo.InvariantCulture);

            long unitNumber = exponent / ExponentUnit;
            double adjusted = ScaleForAlphabetUnit(mantissa);
            int decimals = fixedDecimalCount ?? GetDisplayDecimals(adjusted, unitNumber);
            if (decimals <= 12)
            {
                double scale = PowersOf10.Lookup(exponent % ExponentUnit + decimals);
                double magnitude = Math.Abs(mantissa);
                double lower = Math.Floor(magnitude * scale);
                if (magnitude >= (lower + 0.5) / scale)
                    lower++;
                adjusted = (mantissa < 0 ? -lower : lower) / PowersOf10.Lookup(decimals);
            }
            if (Math.Abs(adjusted) >= 1000)
            {
                adjusted /= 1000;
                unitNumber++;
            }

            decimals = fixedDecimalCount ?? GetDisplayDecimals(adjusted, unitNumber);
            string digits = adjusted.OptimizeToString(decimals);
            return unitNumber == 0
                ? digits
                : digits + AlphabetManager.GetAlphabetUnit(unitNumber - 1);
        }

        private static int GetDisplayDecimals(double adjusted, long unitNumber)
        {
            return unitNumber == 0 ? 0 : Math.Max(0, 3 - NumberUtility.GetDigits(adjusted));
        }

        public string ToStringMantissaExponent()
        {
#if NETCOREAPP3_0_OR_GREATER
            const string format = "R";
#else
            // Unity Mono에서 가수를 왕복 변환하려면 G17이 필요하다.
            const string format = "G17";
#endif
            return mantissa.ToString(format, CultureInfo.InvariantCulture)
                + "e"
                + exponent.ToString(CultureInfo.InvariantCulture);
        }

        public override string ToString() => FormatForDisplay(null);

        public static BigDouble Abs(BigDouble value)
        {
            return FromMantissaExponentNoNormalize(Math.Abs(value.Mantissa), value.Exponent);
        }

        public static BigDouble Negate(BigDouble value)
        {
            return FromMantissaExponentNoNormalize(-value.Mantissa, value.Exponent);
        }

        public static int Sign(BigDouble value)
        {
            return Math.Sign(value.Mantissa);
        }

        public static BigDouble Round(BigDouble value)
        {
            if (IsNaN(value))
            {
                return value;
            }

            if (value.Exponent < -1)
            {
                return Zero;
            }

            if (value.Exponent < MaxSignificantDigits)
            {
                return new BigDouble(Math.Round(value.ToDouble()));
            }

            return value;
        }

        public static BigDouble Round(BigDouble value, MidpointRounding mode)
        {
            if (IsNaN(value))
            {
                return value;
            }

            if (value.Exponent < -1)
            {
                return new BigDouble(Math.Round(Math.Sign(value.Mantissa) * 0.01, mode));
            }

            if (value.Exponent < MaxSignificantDigits)
            {
                return new BigDouble(Math.Round(value.ToDouble(), mode));
            }

            return value;
        }

        public static BigDouble Floor(BigDouble value)
        {
            if (IsNaN(value))
            {
                return value;
            }

            if (value.Exponent < -1)
            {
                return Math.Sign(value.Mantissa) >= 0 ? Zero : -One;
            }

            if (value.Exponent < MaxSignificantDigits)
            {
                return new BigDouble(Math.Floor(value.ToDouble()));
            }

            return value;
        }

        public static BigDouble Ceiling(BigDouble value)
        {
            if (IsNaN(value))
            {
                return value;
            }

            if (value.Exponent < -1)
            {
                return Math.Sign(value.Mantissa) > 0 ? One : Zero;
            }

            if (value.Exponent < MaxSignificantDigits)
            {
                return new BigDouble(Math.Ceiling(value.ToDouble()));
            }

            return value;
        }

        public static BigDouble Truncate(BigDouble value)
        {
            if (IsNaN(value))
            {
                return value;
            }

            if (value.Exponent < 0)
            {
                return Zero;
            }

            if (value.Exponent < MaxSignificantDigits)
            {
                return new BigDouble(Math.Truncate(value.ToDouble()));
            }

            return value;
        }

        public static BigDouble Add(BigDouble left, BigDouble right)
        {
            if (IsZero(left.Mantissa))
            {
                return right;
            }

            if (IsZero(right.Mantissa))
            {
                return left;
            }

            if (IsNaN(left) || IsNaN(right) || IsInfinity(left) || IsInfinity(right))
            {
                return left.Mantissa + right.Mantissa;
            }

            BigDouble bigger,
                smaller;
            if (left.Exponent >= right.Exponent)
            {
                bigger = left;
                smaller = right;
            }
            else
            {
                bigger = right;
                smaller = left;
            }

            if (unchecked((ulong)(bigger.Exponent - smaller.Exponent)) > MaxSignificantDigits)
            {
                return bigger;
            }

            var sum = Math.Round(
                AdditionPrecisionFactor * bigger.Mantissa
                    + AdditionPrecisionFactor
                        * smaller.Mantissa
                        * PowersOf10.Lookup(smaller.Exponent - bigger.Exponent)
            );
            if (bigger.Exponent < long.MinValue + AdditionPrecisionExponent)
                return NormalizeWideExponent(
                    sum,
                    (decimal)bigger.Exponent - AdditionPrecisionExponent
                );
            return Normalize(sum, bigger.Exponent - AdditionPrecisionExponent);
        }

        public static BigDouble Subtract(BigDouble left, BigDouble right)
        {
            return left + -right;
        }

        public static BigDouble Multiply(BigDouble left, BigDouble right)
        {
            long newExponent;
            try
            {
                newExponent = checked(left.Exponent + right.Exponent);
            }
            catch (OverflowException)
            {
                return NormalizeWideExponent(
                    left.Mantissa * right.Mantissa,
                    (decimal)left.Exponent + right.Exponent
                );
            }

            return Normalize(left.Mantissa * right.Mantissa, newExponent);
        }

        public static BigDouble Divide(BigDouble left, BigDouble right)
        {
            var mantissa = left.Mantissa / right.Mantissa;
            if (IsZero(right.Mantissa) || IsZero(left.Mantissa) || !IsFinite(mantissa))
            {
                return Normalize(mantissa, 0);
            }

            long newExponent;
            try
            {
                newExponent = checked(left.Exponent - right.Exponent);
            }
            catch (OverflowException)
            {
                return NormalizeWideExponent(mantissa, (decimal)left.Exponent - right.Exponent);
            }

            return Normalize(mantissa, newExponent);
        }

        public static BigDouble Reciprocate(BigDouble value)
        {
            return Divide(One, value);
        }

        public static implicit operator BigDouble(double value)
        {
            return new BigDouble(value);
        }

        public static implicit operator BigDouble(string value)
        {
            return new BigDouble(value);
        }

        public static explicit operator BigDouble(BigInteger value)
        {
            if (value >= int.MinValue && value <= int.MaxValue)
                return new BigDouble((int)value);
            return new BigDouble(value.ToString("E16", CultureInfo.InvariantCulture));
        }

        public static implicit operator BigDouble(int value)
        {
            return new BigDouble(value);
        }

        public static implicit operator BigDouble(long value)
        {
            return new BigDouble(value);
        }

        public static implicit operator BigDouble(float value)
        {
            return new BigDouble(value);
        }

        public static BigDouble operator -(BigDouble value)
        {
            return Negate(value);
        }

        public static BigDouble operator +(BigDouble left, BigDouble right)
        {
            return Add(left, right);
        }

        public static BigDouble operator -(BigDouble left, BigDouble right)
        {
            return Subtract(left, right);
        }

        public static BigDouble operator *(BigDouble left, BigDouble right)
        {
            return Multiply(left, right);
        }

        public static BigDouble operator /(BigDouble left, BigDouble right)
        {
            return Divide(left, right);
        }

        public static BigDouble operator ++(BigDouble value)
        {
            return value.Add(1);
        }

        public static BigDouble operator --(BigDouble value)
        {
            return value.Subtract(1);
        }

        public int CompareTo(object other)
        {
            if (other == null)
            {
                return 1;
            }
            if (other is BigDouble)
            {
                return CompareTo((BigDouble)other);
            }
            throw new ArgumentException("The parameter must be a BigDouble.");
        }

        public int CompareTo(BigDouble other)
        {
            if (
                IsZero(Mantissa)
                || IsZero(other.Mantissa)
                || IsNaN(this)
                || IsNaN(other)
                || IsInfinity(this)
                || IsInfinity(other)
            )
            {
                return Mantissa.CompareTo(other.Mantissa);
            }
            if (Mantissa > 0 && other.Mantissa < 0)
            {
                return 1;
            }
            if (Mantissa < 0 && other.Mantissa > 0)
            {
                return -1;
            }

            var exponentComparison = Exponent.CompareTo(other.Exponent);
            return exponentComparison != 0
                ? (Mantissa > 0 ? exponentComparison : -exponentComparison)
                : Mantissa.CompareTo(other.Mantissa);
        }

        public override bool Equals(object? other)
        {
            return other is BigDouble && Equals((BigDouble)other);
        }

        public override int GetHashCode()
        {
            if (Mantissa == 0 || !IsFinite(Mantissa))
                return Mantissa.GetHashCode();
            unchecked
            {
                return (Mantissa.GetHashCode() * 397) ^ Exponent.GetHashCode();
            }
        }

        public bool Equals(BigDouble other)
        {
            return Mantissa.Equals(other.Mantissa)
                && (Exponent == other.Exponent || Mantissa == 0 || !IsFinite(Mantissa));
        }

        public bool Equals(BigDouble other, double tolerance)
        {
            if (!(tolerance >= 0) || IsNaN(this) || IsNaN(other))
                return false;
            if (Equals(other))
                return true;
            if (tolerance == 0 || IsInfinity(this) || IsInfinity(other))
                return false;
            if (Mantissa == 0 || other.Mantissa == 0)
                return tolerance >= 1;

            bool sameSign = (Mantissa > 0) == (other.Mantissa > 0);
            if (tolerance == 1)
                return sameSign;

            var high = Exponent >= other.Exponent ? this : other;
            var low = Exponent >= other.Exponent ? other : this;
            ulong gap = unchecked((ulong)(high.Exponent - low.Exponent));
            if (gap > MaxSignificantDigits)
                return tolerance > 1;

            var scaled = high.Mantissa * PowersOf10.Lookup((long)gap);
            return Math.Abs(scaled - low.Mantissa)
                <= Math.Max(Math.Abs(scaled), Math.Abs(low.Mantissa)) * tolerance;
        }

        public static bool operator ==(BigDouble left, BigDouble right)
        {
            return !IsNaN(left) && left.Equals(right);
        }

        public static bool operator !=(BigDouble left, BigDouble right)
        {
            return !(left == right);
        }

        public static bool operator <(BigDouble a, BigDouble b)
        {
            if (IsNaN(a) || IsNaN(b))
            {
                return false;
            }

            if (IsNegativeInfinity(a))
                return !IsNegativeInfinity(b);
            if (IsPositiveInfinity(b))
                return !IsPositiveInfinity(a);
            if (IsPositiveInfinity(a) || IsNegativeInfinity(b))
                return false;

            if (IsZero(a.Mantissa))
                return b.Mantissa > 0;
            if (IsZero(b.Mantissa))
                return a.Mantissa < 0;
            if (a.Exponent == b.Exponent)
                return a.Mantissa < b.Mantissa;
            if (a.Mantissa > 0)
                return b.Mantissa > 0 && a.Exponent < b.Exponent;
            return b.Mantissa > 0 || a.Exponent > b.Exponent;
        }

        public static bool operator <=(BigDouble a, BigDouble b)
        {
            if (IsNaN(a) || IsNaN(b))
            {
                return false;
            }

            return !(a > b);
        }

        public static bool operator >(BigDouble a, BigDouble b)
        {
            if (IsNaN(a) || IsNaN(b))
            {
                return false;
            }

            if (IsPositiveInfinity(a))
                return !IsPositiveInfinity(b);
            if (IsNegativeInfinity(b))
                return !IsNegativeInfinity(a);
            if (IsNegativeInfinity(a) || IsPositiveInfinity(b))
                return false;

            if (IsZero(a.Mantissa))
            {
                return b.Mantissa < 0;
            }

            if (IsZero(b.Mantissa))
            {
                return a.Mantissa > 0;
            }

            if (a.Exponent == b.Exponent)
            {
                return a.Mantissa > b.Mantissa;
            }

            if (a.Mantissa > 0)
            {
                return b.Mantissa < 0 || a.Exponent > b.Exponent;
            }

            return b.Mantissa < 0 && a.Exponent < b.Exponent;
        }

        public static bool operator >=(BigDouble a, BigDouble b)
        {
            if (IsNaN(a) || IsNaN(b))
            {
                return false;
            }

            return !(a < b);
        }

        public static BigDouble Max(BigDouble left, BigDouble right)
        {
            if (IsNaN(left) || IsNaN(right))
            {
                return NaN;
            }
            return left > right ? left : right;
        }

        public static BigDouble Min(BigDouble left, BigDouble right)
        {
            if (IsNaN(left) || IsNaN(right))
            {
                return NaN;
            }
            return left > right ? right : left;
        }

        public static double AbsLog10(BigDouble value)
        {
            var mantissa = Math.Abs(value.Mantissa);
            if (value.Exponent == -1 && mantissa >= 5 && mantissa < 10)
            {
                double delta = (mantissa - 10) / 10;
                double rounded = 1 + delta;
                return Math.Log(rounded) * (delta / (rounded - 1)) / Ln10;
            }
            return value.Exponent + Math.Log10(mantissa);
        }

        public static double Log10(BigDouble value)
        {
            return value.Mantissa < 0 ? double.NaN : AbsLog10(value);
        }

        public static double Log(BigDouble value, BigDouble @base)
        {
            if (@base == One || IsZero(@base.Mantissa))
                return double.NaN;
            if (IsInfinity(@base))
                return value == One && IsPositiveInfinity(@base) ? 0.0 : double.NaN;
            return Log10(value) / Log10(@base);
        }

        public static double Log(BigDouble value, double @base)
        {
            if (IsZero(@base) || @base == 1)
            {
                return double.NaN;
            }

            if (double.IsPositiveInfinity(@base))
                return value == One ? 0.0 : double.NaN;
            return Ln10 / Math.Log(@base) * Log10(value);
        }

        public static double Log2(BigDouble value)
        {
            return Log2Of10 * Log10(value);
        }

        public static double Ln(BigDouble value)
        {
            return Ln10 * Log10(value);
        }

        public static BigDouble Pow10(double power)
        {
            if (power >= -(double)long.MinValue)
                return PositiveInfinity;
            if (power < long.MinValue)
                return Zero;
            if (double.IsNaN(power))
                return NaN;
            long exponent = (long)power;
            return power == exponent
                ? Pow10(exponent)
                : Normalize(Math.Pow(10, power - exponent), exponent);
        }

        public static BigDouble Pow10(long power)
        {
            return FromMantissaExponentNoNormalize(1, power);
        }

        public static BigDouble Pow(BigDouble value, BigDouble power)
        {
            return Pow(value, power.ToDouble());
        }

        public static BigDouble Pow(BigDouble value, long power)
        {
            if (power == 0)
                return One;
            if (power == 1)
                return value;

            bool negative = value.Mantissa < 0 && (power & 1) != 0;
            var magnitude = Math.Abs(value.Mantissa);
            if (!IsFinite(magnitude) || magnitude == 0)
            {
                BigDouble special = Math.Pow(magnitude, power);
                return negative ? -special : special;
            }

            if (magnitude == 1 && value.Exponent == 1)
                return negative ? -Pow10(power) : Pow10(power);

            var poweredMantissa = Math.Pow(magnitude, power);
            if (poweredMantissa >= 1e-307 && IsFinite(poweredMantissa))
            {
                var scaled = Normalize(negative ? -poweredMantissa : poweredMantissa, 0);
                try
                {
                    long exponent = checked(value.Exponent * power + scaled.Exponent);
                    return FromMantissaExponentNoNormalize(scaled.Mantissa, exponent);
                }
                catch (OverflowException)
                {
                    if (Math.Abs((double)value.Exponent * power) <= -(double)long.MinValue)
                    {
                        decimal exponent = (decimal)value.Exponent * power + scaled.Exponent;
                        if (exponent >= long.MinValue && exponent <= long.MaxValue)
                            return FromMantissaExponentNoNormalize(scaled.Mantissa, (long)exponent);
                    }
                    if ((value.Exponent > 0) != (power > 0))
                        return Zero;
                    return negative ? NegativeInfinity : PositiveInfinity;
                }
            }

            if (value.Exponent == -1 && magnitude >= 5)
            {
                var nearOnePower = Pow10(power * AbsLog10(value));
                return negative ? -nearOnePower : nearOnePower;
            }

            var factor = power < 0 ? Reciprocate(value) : value;
            var result = One;
            ulong remaining = power < 0 ? unchecked((ulong)-power) : (ulong)power;
            while (remaining != 0)
            {
                if ((remaining & 1) != 0)
                    result *= factor;
                remaining >>= 1;
                if (remaining != 0)
                    factor *= factor;
            }

            return result;
        }

        public static BigDouble Pow(BigDouble value, double power)
        {
            if (power >= long.MinValue && power < -(double)long.MinValue && IsInteger(power))
                return Pow(value, (long)power);
            if (double.IsNaN(power))
                return value == One ? One : NaN;
            if (IsNaN(value))
                return NaN;

            if (double.IsInfinity(power))
            {
                int comparison = Abs(value).CompareTo(One);
                if (comparison == 0)
                    return One;
                return (comparison > 0) == (power > 0) ? PositiveInfinity : Zero;
            }

            if (value.Mantissa < 0 && !IsInteger(power))
                return NaN;
            if (!IsFinite(value.Mantissa) || value.Mantissa == 0)
                return new BigDouble(Math.Pow(Math.Abs(value.Mantissa), power));

            var exponentProduct = value.Exponent * power;
            if (
                !IsInteger(power)
                && Math.Abs(exponentProduct) >= 1L << 32
                && Math.Abs(exponentProduct) <= -(double)long.MinValue
            )
            {
                // 지수 곱의 소수부를 보존하려고 이진 가수로 계산한다.
                long bits = BitConverter.DoubleToInt64Bits(Math.Abs(power));
                long significand = (1L << 52) | (bits & ((1L << 52) - 1));
                int shift = 1075 - (int)(bits >> 52);
                BigInteger product =
                    (BigInteger)value.Exponent * (power < 0 ? -significand : significand);
                var whole = BigInteger.DivRem(product, BigInteger.One << shift, out var remainder);
                var fraction = (double)remainder * Math.Pow(2, -shift);
                var scaled = Normalize(
                    Math.Pow(Math.Abs(value.Mantissa), power) * Math.Pow(10, fraction),
                    0
                );
                if (IsFinite(scaled.Mantissa) && scaled.Mantissa != 0)
                {
                    whole += scaled.Exponent;
                    if (whole > long.MaxValue)
                        return PositiveInfinity;
                    if (whole < long.MinValue)
                        return Zero;
                    return FromMantissaExponentNoNormalize(scaled.Mantissa, (long)whole);
                }
            }
            else if (exponentProduct >= long.MinValue && exponentProduct < -(double)long.MinValue)
            {
                long exponent = (long)exponentProduct;
                var scaledMantissa =
                    Math.Pow(Math.Abs(value.Mantissa), power)
                    * Math.Pow(10, exponentProduct - exponent);
                if (scaledMantissa >= 1e-307 && IsFinite(scaledMantissa))
                    return Normalize(scaledMantissa, exponent);
            }

            return Pow10(power * AbsLog10(value));
        }

        public static BigDouble Factorial(BigDouble value)
        {
            var n = value.ToDouble() + 1;

            if (
                double.IsPositiveInfinity(n) || (n > 1 && n * (Math.Log10(n) - 1 / Ln10) > ExpLimit)
            )
            {
                return PositiveInfinity;
            }

            return Pow(
                    n
                        / 2.71828182845904523536
                        * Math.Sqrt(n * Math.Sinh(1 / n) + 1 / (810 * Math.Pow(n, 6))),
                    n
                ) * Math.Sqrt(2 * 3.141592653589793238462 / n);
        }

        public static BigDouble Exp(BigDouble value)
        {
            return Pow(2.71828182845904523536, value);
        }

        public static BigDouble Sqrt(BigDouble value)
        {
            if (value.Mantissa < 0)
            {
                return new BigDouble(double.NaN);
            }

            if (value.Exponent % 2 != 0)
            {
                return Normalize(
                    Math.Sqrt(value.Mantissa) * Sqrt10,
                    value.Exponent / 2 - (value.Exponent < 0 ? 1 : 0)
                );
            }

            return Normalize(Math.Sqrt(value.Mantissa), value.Exponent / 2);
        }

        public static BigDouble Cbrt(BigDouble value)
        {
            var sign = 1;
            var mantissa = value.Mantissa;
            if (mantissa < 0)
            {
                sign = -1;
                mantissa = -mantissa;
            }

            var newmantissa = sign * Math.Pow(mantissa, 1 / 3.0);

            var mod = value.Exponent % 3;
            var rootExponent = value.Exponent / 3;
            if (mod < 0)
            {
                mod += 3;
                rootExponent--;
            }
            if (mod == 1)
            {
                return Normalize(newmantissa * CubeRoot10, rootExponent);
            }

            if (mod == 2)
            {
                return Normalize(newmantissa * TwoThirdsPowerOf10, rootExponent);
            }

            return Normalize(newmantissa, rootExponent);
        }

        public static BigDouble Sinh(BigDouble value)
        {
            if (value.Exponent < -8)
                return value;
            var x = value.ToDouble();
            if (Math.Abs(x) < 700)
                return new BigDouble(Math.Sinh(x));

            var magnitude = LargeHyperbolicMagnitude(Math.Abs(x));
            return x < 0 ? -magnitude : magnitude;
        }

        public static BigDouble Cosh(BigDouble value)
        {
            var x = Math.Abs(value.ToDouble());
            return x < 700 ? new BigDouble(Math.Cosh(x)) : LargeHyperbolicMagnitude(x);
        }

        private static BigDouble LargeHyperbolicMagnitude(double x)
        {
            if (double.IsNaN(x))
                return NaN;
            if (x >= -(double)long.MinValue * Ln10)
                return PositiveInfinity;
            return Exp(new BigDouble(x)) / 2;
        }

        public static BigDouble Tanh(BigDouble value)
        {
            return value.Exponent < -8 ? value : new BigDouble(Math.Tanh(value.ToDouble()));
        }

        public static double Asinh(BigDouble value)
        {
            if (value.Exponent > 150)
            {
                var magnitude = Ln(Abs(value)) + Math.Log(2);
                return value.Mantissa < 0 ? -magnitude : magnitude;
            }
            return Math.Asinh(value.ToDouble());
        }

        public static double Acosh(BigDouble value)
        {
            if (value.Exponent > 150)
                return Ln(value) + Math.Log(2);
            return Math.Acosh(value.Exponent == 0 ? value.Mantissa : value.ToDouble());
        }

        public static double Atanh(BigDouble value)
        {
            if (Abs(value) >= One)
                return double.NaN;
            if (value.Exponent == -1 && Math.Abs(value.Mantissa) >= 5)
            {
                double gap = (10 - Math.Abs(value.Mantissa)) / 10;
                return Math.Sign(value.Mantissa) * (Math.Log(2 - gap) - Math.Log(gap)) / 2;
            }
            return Math.Atanh(value.ToDouble());
        }

        private static bool IsZero(double value)
        {
            return Math.Abs(value) < double.Epsilon;
        }

        private static bool AreEqual(double first, double second)
        {
            return Math.Abs(first - second) < Tolerance;
        }

        private static bool IsInteger(double value)
        {
            return IsZero(Math.Abs(value % 1));
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static class PowersOf10
        {
            private static double[] Powers { get; } = new double[DoubleExpMax - DoubleExpMin];

            private const long IndexOf0 = -DoubleExpMin - 1;

            static PowersOf10()
            {
                // Math.Pow의 반올림 오차를 피하려고 10의 거듭제곱을 문자열에서 읽는다.
                for (var i = 0; i < Powers.Length; i++)
                {
                    Powers[i] = double.Parse("1e" + (i - IndexOf0), CultureInfo.InvariantCulture);
                }
            }

            public static double Lookup(long power)
            {
                var index = IndexOf0 + power;
                if (index < 0 || index >= Powers.Length)
                {
                    return Math.Pow(10, power);
                }

                return Powers[index];
            }
        }

        private struct PrivateConstructorArg { }
    }

    public static class BigMath
    {
        private static readonly ThreadLocal<Random> ThreadLocalRandom = new ThreadLocal<Random>(
            () =>
                new Random()
        );
        private static Random Random => ThreadLocalRandom.Value!;

        public static BigDouble RandomBigDouble(double absMaxExponent)
        {
            if (Random.NextDouble() * 20 < 1)
            {
                return BigDouble.Normalize(0, 0);
            }

            var mantissa = Random.NextDouble() * 10;
            if (Random.NextDouble() * 10 < 1)
            {
                mantissa = Math.Round(mantissa);
            }

            mantissa *= Math.Sign(Random.NextDouble() * 2 - 1);
            var exponent = (long)(
                Math.Floor(Random.NextDouble() * absMaxExponent * 2) - absMaxExponent
            );
            return BigDouble.Normalize(mantissa, exponent);
        }

        public static BigDouble AffordGeometricSeries(
            BigDouble resourcesAvailable,
            BigDouble priceStart,
            BigDouble priceRatio,
            BigDouble currentOwned
        )
        {
            if (priceRatio == BigDouble.One)
            {
                return AffordFixedPrice(resourcesAvailable, priceStart);
            }

            var actualStart = priceStart * BigDouble.Pow(priceRatio, currentOwned);

            var estimate =
                BigDouble.Log10(resourcesAvailable / actualStart * (priceRatio - 1) + 1)
                / BigDouble.Log10(priceRatio);
            BigDouble count = PurchaseCandidate(estimate, out bool checkCost);
            return checkCost && GeometricCost(count, actualStart, priceRatio) > resourcesAvailable
                ? count - 1
                : count;
        }

        public static BigDouble SumGeometricSeries(
            BigDouble numItems,
            BigDouble priceStart,
            BigDouble priceRatio,
            BigDouble currentOwned
        )
        {
            if (priceRatio == BigDouble.One)
            {
                return PurchaseCostProduct(priceStart, numItems);
            }

            var actualStart = priceStart * BigDouble.Pow(priceRatio, currentOwned);

            return GeometricCost(numItems, actualStart, priceRatio);
        }

        private static BigDouble GeometricCost(
            BigDouble numItems,
            BigDouble actualStart,
            BigDouble priceRatio
        )
        {
            var factor = (1 - BigDouble.Pow(priceRatio, numItems)) / (1 - priceRatio);
            return PurchaseCostProduct(actualStart, factor);
        }

        private static BigDouble AffordFixedPrice(BigDouble resourcesAvailable, BigDouble price)
        {
            var estimate = resourcesAvailable / price;
            if (estimate.Exponent >= 16)
                return estimate;
            BigDouble count = PurchaseCandidate(estimate.ToDouble(), out bool checkCost);
            return checkCost && PurchaseCostProduct(count, price) > resourcesAvailable
                ? count - 1
                : count;
        }

        private static double PurchaseCandidate(double estimate, out bool checkCost)
        {
            var nearest = Math.Round(estimate);
            checkCost =
                nearest > 0
                && nearest < (1L << 52)
                && Math.Abs(estimate - nearest) <= Math.Max(1e-10, Math.Abs(estimate) * 1e-14);
            return checkCost ? nearest : Math.Floor(estimate);
        }

        private static BigDouble PurchaseCostProduct(BigDouble left, BigDouble right)
        {
            if (
                left.Exponent >= 0
                && right.Exponent >= 0
                && left.Exponent <= 15
                && right.Exponent <= 15
            )
            {
                double a = left.ToDouble();
                double b = right.ToDouble();
                double product = a * b;
                if (
                    Math.Abs(product) <= (1L << 53)
                    && a == Math.Truncate(a)
                    && b == Math.Truncate(b)
                    && new BigDouble(a) == left
                    && new BigDouble(b) == right
                )
                    return new BigDouble(product);
            }
            return left * right;
        }

        public static BigDouble AffordArithmeticSeries(
            BigDouble resourcesAvailable,
            BigDouble priceStart,
            BigDouble priceAdd,
            BigDouble currentOwned
        )
        {
            if (priceAdd == BigDouble.Zero)
                return AffordFixedPrice(resourcesAvailable, priceStart);

            var actualStart = priceStart + currentOwned * priceAdd;
            var b = actualStart - priceAdd / 2;
            var root = BigDouble.Sqrt(b * b + 2 * priceAdd * resourcesAvailable);
            var estimate =
                b > BigDouble.Zero ? 2 * resourcesAvailable / (root + b) : (root - b) / priceAdd;
            if (estimate.Exponent >= 16)
                return estimate;
            BigDouble count = PurchaseCandidate(estimate.ToDouble(), out bool checkCost);
            return checkCost && ArithmeticCost(count, actualStart, priceAdd) > resourcesAvailable
                ? count - 1
                : count;
        }

        public static BigDouble SumArithmeticSeries(
            BigDouble numItems,
            BigDouble priceStart,
            BigDouble priceAdd,
            BigDouble currentOwned
        )
        {
            if (priceAdd == BigDouble.Zero)
                return PurchaseCostProduct(priceStart, numItems);
            var actualStart = priceStart + currentOwned * priceAdd;
            return ArithmeticCost(numItems, actualStart, priceAdd);
        }

        private static BigDouble ArithmeticCost(
            BigDouble numItems,
            BigDouble actualStart,
            BigDouble priceAdd
        )
        {
            return numItems / 2 * (2 * actualStart + (numItems - 1) * priceAdd);
        }

        public static BigDouble EfficiencyOfPurchase(
            BigDouble cost,
            BigDouble currentRpS,
            BigDouble deltaRpS
        )
        {
            return cost / currentRpS + cost / deltaRpS;
        }
    }

    public static class BigDoubleExtensions
    {
        public static BigDouble Abs(this BigDouble value)
        {
            return BigDouble.Abs(value);
        }

        public static BigDouble Negate(this BigDouble value)
        {
            return BigDouble.Negate(value);
        }

        public static int Sign(this BigDouble value)
        {
            return BigDouble.Sign(value);
        }

        public static BigDouble Round(this BigDouble value)
        {
            return BigDouble.Round(value);
        }

        public static BigDouble Floor(this BigDouble value)
        {
            return BigDouble.Floor(value);
        }

        public static BigDouble Ceiling(this BigDouble value)
        {
            return BigDouble.Ceiling(value);
        }

        public static BigDouble Truncate(this BigDouble value)
        {
            return BigDouble.Truncate(value);
        }

        public static BigDouble Add(this BigDouble value, BigDouble other)
        {
            return BigDouble.Add(value, other);
        }

        public static BigDouble Subtract(this BigDouble value, BigDouble other)
        {
            return BigDouble.Subtract(value, other);
        }

        public static BigDouble Multiply(this BigDouble value, BigDouble other)
        {
            return BigDouble.Multiply(value, other);
        }

        public static BigDouble Divide(this BigDouble value, BigDouble other)
        {
            return BigDouble.Divide(value, other);
        }

        public static BigDouble Reciprocate(this BigDouble value)
        {
            return BigDouble.Reciprocate(value);
        }

        public static BigDouble Max(this BigDouble value, BigDouble other)
        {
            return BigDouble.Max(value, other);
        }

        public static BigDouble Min(this BigDouble value, BigDouble other)
        {
            return BigDouble.Min(value, other);
        }

        public static double AbsLog10(this BigDouble value)
        {
            return BigDouble.AbsLog10(value);
        }

        public static double Log10(this BigDouble value)
        {
            return BigDouble.Log10(value);
        }

        public static double Log(BigDouble value, BigDouble @base)
        {
            return BigDouble.Log(value, @base);
        }

        public static double Log(this BigDouble value, double @base)
        {
            return BigDouble.Log(value, @base);
        }

        public static double Log2(this BigDouble value)
        {
            return BigDouble.Log2(value);
        }

        public static double Ln(this BigDouble value)
        {
            return BigDouble.Ln(value);
        }

        public static BigDouble Exp(this BigDouble value)
        {
            return BigDouble.Exp(value);
        }

        public static BigDouble Sinh(this BigDouble value)
        {
            return BigDouble.Sinh(value);
        }

        public static BigDouble Cosh(this BigDouble value)
        {
            return BigDouble.Cosh(value);
        }

        public static BigDouble Tanh(this BigDouble value)
        {
            return BigDouble.Tanh(value);
        }

        public static double Asinh(this BigDouble value)
        {
            return BigDouble.Asinh(value);
        }

        public static double Acosh(this BigDouble value)
        {
            return BigDouble.Acosh(value);
        }

        public static double Atanh(this BigDouble value)
        {
            return BigDouble.Atanh(value);
        }

        public static BigDouble Pow(this BigDouble value, BigDouble power)
        {
            return BigDouble.Pow(value, power);
        }

        public static BigDouble Pow(this BigDouble value, long power)
        {
            return BigDouble.Pow(value, power);
        }

        public static BigDouble Pow(this BigDouble value, double power)
        {
            return BigDouble.Pow(value, power);
        }

        public static BigDouble Factorial(this BigDouble value)
        {
            return BigDouble.Factorial(value);
        }

        public static BigDouble Sqrt(this BigDouble value)
        {
            return BigDouble.Sqrt(value);
        }

        public static BigDouble Cbrt(this BigDouble value)
        {
            return BigDouble.Cbrt(value);
        }

        public static BigDouble Sqr(this BigDouble value)
        {
            return BigDouble.Pow(value, 2);
        }

#if EXTENSIONS_EASTER_EGGS
        public static BigDouble AscensionPenalty(this BigDouble value, double ascensions)
        {
            return Math.Abs(ascensions) < double.Epsilon
                ? value
                : BigDouble.Pow(value, Math.Pow(10, -ascensions));
        }

        public static BigDouble Egg(this BigDouble value)
        {
            return value + 9;
        }
#endif
    }
}
