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

        // 두 지수가 17 이상 차이나면 덧셈이 무의미하므로 큰 값을 반환
        private const int MaxSignificantDigits = 17;

        private const long ExpLimit = long.MaxValue;

        // Double에서 나타날 수 있는 최대 지수
        private const long DoubleExpMax = 308;

        // Double에서 나타날 수 있는 최소 지수
        private const long DoubleExpMin = -324;

        // 덧셈 정밀도 팩터 (정수 스케일링용)
        private const double AdditionPrecisionFactor = 1e14;
        private const int AdditionPrecisionExponent = 14;

        // 수학 상수
        private const double Ln10 = 2.30258509299404568402; // Math.Log(10)
        private const double Log2Of10 = 3.32192809488736234787; // Math.Log2(10)
        private const double Sqrt10 = 3.16227766016838; // Math.Sqrt(10)
        private const double CubeRoot10 = 2.1544346900318837; // Math.Pow(10, 1.0/3)
        private const double TwoThirdsPowerOf10 = 4.6415888336127789; // Math.Pow(10, 2.0/3)

        private double mantissa;

        private long exponent;

        // This constructor is used to prevent non-normalized values to be created via constructor.
        // ReSharper disable once UnusedParameter.Local
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
            //SAFETY: Handle Infinity and NaN in a somewhat meaningful way.
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
                return FromMantissaExponentNoNormalize(mantissa / 10, exponent + 1);
            }

            if (absMantissa >= 0.1 && absMantissa < 1)
            {
                return FromMantissaExponentNoNormalize(mantissa * 10, exponent - 1);
            }

            long tempExponent = (long)Math.Floor(Math.Log10(Math.Abs(mantissa)));
            //SAFETY: handle 5e-324, -5e-324 separately
            if (tempExponent == DoubleExpMin)
            {
                mantissa = mantissa * 10 / 1e-323;
            }
            else
            {
                mantissa = mantissa / PowersOf10.Lookup(tempExponent);
            }

            // subnormal 구간(~1e-323)에선 Log10이 1 어긋나 mantissa가 [1,10)을 벗어남
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
            return FromMantissaExponentNoNormalize(mantissa, exponent + tempExponent);
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
            // CompareTo(null)/Equals(null)도 implicit string 변환을 타고 여기로 와서
            // NRE 대신 명확한 예외를 던지게 한다
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (ContainsExponentMarker(value))
            {
                var info = BigValueInfo.ExponentFormatToBigValueInfo(value);
                return Normalize(info.Mantissa, info.Exponent);
            }

            if (value == "NaN")
            {
                return NaN;
            }

            var result = new BigDouble(FastDouble.ParseDouble(value, FractionalPartAccuracy));
            if (IsNaN(result))
            {
                throw new Exception("Invalid argument: " + value);
            }

            return result;
        }

        public float ToFloat()
        {
            return (float)ToDouble();
        }

        public double ToDouble()
        {
            if (IsNaN(this))
            {
                return double.NaN;
            }

            if (Exponent > DoubleExpMax)
            {
                return Mantissa > 0 ? double.PositiveInfinity : double.NegativeInfinity;
            }

            if (Exponent < DoubleExpMin)
            {
                return 0.0;
            }

            //SAFETY: again, handle 5e-324, -5e-324 separately
            if (Exponent == DoubleExpMin)
            {
                return Mantissa > 0 ? 5e-324 : -5e-324;
            }

            var result = Mantissa * PowersOf10.Lookup(Exponent);
            if (!IsFinite(result) || Exponent < 0)
            {
                return result;
            }

            var resultrounded = Math.Round(result);
            // 절대 임계값(1e-10)만으로는 1e8 이상 정수의 정규화 오차(상대 ~1e-16)를 못 잡아서
            // 123456789가 123456788.99999999로 돌아옴. 상대 임계값(수 ulp 수준)을 함께 사용
            if (Math.Abs(resultrounded - result) < Math.Max(1e-10, Math.Abs(result) * 1e-15))
                return resultrounded;
            return result;
        }

        internal string CreateString(int decimalCount)
        {
            // AdjustedMantissa의 3자리 단위 보정은 알파벳 표기용이라 음수 지수에 적용하면
            // 0.5가 500으로 표시됨. 1 미만 값은 double 그대로 출력
            if (exponent < 0)
            {
                return ToDouble().ToString(CultureInfo.InvariantCulture);
            }

            var (adjustedMantissa, displayExponent) = AdjustedMantissaForDisplay();
            return CreateString(decimalCount, adjustedMantissa, displayExponent);
        }

        private static string CreateString(
            int decimalCount,
            double adjustedMantissa,
            long displayExponent
        )
        {
            if (displayExponent < 3)
            {
                return adjustedMantissa.OptimizeToString(decimalCount);
            }

            return adjustedMantissa.OptimizeToString(decimalCount)
                + AlphabetManager.GetAlphabetUnitFromExponent(displayExponent);
        }

        public string ToStringMantissaExponent()
        {
            return mantissa.ToString(CultureInfo.InvariantCulture)
                + "e"
                + exponent.ToString(CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            // 1 미만 값은 알파벳 단위/소수 자리 규칙을 적용하지 않고 double 그대로 표시한다.
            if (exponent < 0)
            {
                return ToDouble().ToString(CultureInfo.InvariantCulture);
            }

            var (adjustedMantissa, displayExponent) = AdjustedMantissaForDisplay();
            var digits = NumberUtility.GetDigits(adjustedMantissa);
            int decimalCount = 0;
            switch (digits)
            {
                case 0:
                    decimalCount = 0;
                    break;
                case 1:
                    decimalCount = 2;
                    break;
                case 2:
                    decimalCount = 1;
                    break;
                case 3:
                    decimalCount = 0;
                    break;
                default:
                    break;
            }
            if (exponent <= 2 && mantissa > -10 && mantissa < 10)
                decimalCount = 0;
            return CreateString(decimalCount, adjustedMantissa, displayExponent);
        }

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
                return Zero;
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
            //figure out which is bigger, shrink the mantissa of the smaller by the difference in exponents, add mantissas, normalize and return

            //TODO: Optimizations and simplification may be possible, see https://github.com/Patashu/break_infinity.js/issues/8

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
                // Let Double handle these cases.
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

            if (bigger.Exponent - smaller.Exponent > MaxSignificantDigits)
            {
                return bigger;
            }

            // 정수로 스케일업 후 연산하여 정밀도 유지 (예: 299 + 18)
            return Normalize(
                Math.Round(
                    AdditionPrecisionFactor * bigger.Mantissa
                        + AdditionPrecisionFactor
                            * smaller.Mantissa
                            * PowersOf10.Lookup(smaller.Exponent - bigger.Exponent)
                ),
                bigger.Exponent - AdditionPrecisionExponent
            );
        }

        public static BigDouble Subtract(BigDouble left, BigDouble right)
        {
            return left + -right;
        }

        public static BigDouble Multiply(BigDouble left, BigDouble right)
        {
            // 2e3 * 4e5 = (2 * 4)e(3 + 5)
            // Exponent 오버플로우 방어
            long newExponent;
            try
            {
                newExponent = checked(left.Exponent + right.Exponent);
            }
            catch (OverflowException)
            {
                // NaN의 지수는 long.MinValue 센티널이라 여기로 흘러올 수 있음 — Zero로 둔갑 방지
                if (IsNaN(left) || IsNaN(right))
                    return NaN;

                // 덧셈 오버플로는 두 지수의 부호가 같을 때만 발생 — 양수 방향이면 Infinity, 음수 방향이면 Zero
                bool positiveResult = (left.Mantissa > 0) == (right.Mantissa > 0);
                if (left.Exponent > 0)
                    return positiveResult ? PositiveInfinity : NegativeInfinity;
                return Zero;
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
                if (IsNaN(left) || IsNaN(right) || double.IsNaN(mantissa))
                    return NaN;

                if (right.Exponent < 0)
                    return mantissa > 0 ? PositiveInfinity : NegativeInfinity;
                return Zero;
            }

            return Normalize(mantissa, newExponent);
        }

        public static BigDouble Reciprocate(BigDouble value)
        {
            return Normalize(1.0 / value.Mantissa, -value.Exponent);
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
            return new BigDouble(value.ToString());
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
            // Equals는 mantissa 0이면 지수를 무시하므로 해시도 지수를 무시해야 계약이 성립
            if (IsZero(Mantissa))
            {
                return 0;
            }
            unchecked
            {
                return (Mantissa.GetHashCode() * 397) ^ Exponent.GetHashCode();
            }
        }

        public bool Equals(BigDouble other)
        {
            return !IsNaN(this)
                && !IsNaN(other)
                && (
                    AreSameInfinity(this, other)
                    || (IsZero(Mantissa) && IsZero(other.Mantissa))
                    || Exponent == other.Exponent && AreEqual(Mantissa, other.Mantissa)
                );
        }

        /// <summary>
        /// Relative comparison with tolerance being adjusted with greatest exponent.
        /// <para>
        /// For example, if you put in 1e-9, then any number closer to the larger number
        /// than (larger number) * 1e-9 will be considered equal.
        /// </para>
        /// </summary>
        public bool Equals(BigDouble other, double tolerance)
        {
            return !IsNaN(this)
                && !IsNaN(other)
                && (
                    AreSameInfinity(this, other)
                    || Abs(this - other) <= Max(Abs(this), Abs(other)) * tolerance
                );
        }

        private static bool AreSameInfinity(BigDouble first, BigDouble second)
        {
            return IsPositiveInfinity(first) && IsPositiveInfinity(second)
                || IsNegativeInfinity(first) && IsNegativeInfinity(second);
        }

        public static bool operator ==(BigDouble left, BigDouble right)
        {
            return left.Equals(right);
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

            // Infinity 처리
            if (IsNegativeInfinity(a))
                return !IsNegativeInfinity(b); // -Inf < anything except -Inf
            if (IsPositiveInfinity(b))
                return !IsPositiveInfinity(a); // anything except +Inf < +Inf
            if (IsPositiveInfinity(a) || IsNegativeInfinity(b))
                return false; // +Inf is not < anything, nothing is < -Inf

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

            // Infinity 처리
            if (IsPositiveInfinity(a))
                return !IsPositiveInfinity(b); // +Inf > anything except +Inf
            if (IsNegativeInfinity(b))
                return !IsNegativeInfinity(a); // anything except -Inf > -Inf
            if (IsNegativeInfinity(a) || IsPositiveInfinity(b))
                return false; // -Inf is not > anything, nothing is > +Inf

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
            return value.Exponent + Math.Log10(Math.Abs(value.Mantissa));
        }

        public static double Log10(BigDouble value)
        {
            return value.Exponent + Math.Log10(value.Mantissa);
        }

        public static double Log(BigDouble value, BigDouble @base)
        {
            return Log(value, @base.ToDouble());
        }

        public static double Log(BigDouble value, double @base)
        {
            // Math.Log(x, newBase)와 동일하게 base 0/1은 정의되지 않음 (1은 0으로 나누기가 됨)
            if (IsZero(@base) || @base == 1)
            {
                return double.NaN;
            }

            // 대부분의 증분 게임에서 log(number >= 1, base >= 2)이므로 간단히 처리
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
            return IsInteger(power)
                ? Pow10((long)power)
                : Normalize(Math.Pow(10, power % 1), (long)Math.Truncate(power));
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
            {
                return One;
            }

            if (power == 1)
            {
                return Normalize(value.Mantissa, value.Exponent);
            }

            if (Is10(value))
            {
                return Pow10(power);
            }

            var mantissa = Math.Pow(value.Mantissa, power);
            if (double.IsInfinity(mantissa))
            {
                // TODO: This is rather dumb, but works anyway
                // Power is too big for our mantissa, so we do multiple Pow with smaller powers.
                // 제곱하는 순간 음수 밑의 부호가 사라지므로 홀수 거듭제곱이면 복원
                var result = Pow(Pow(value, 2), (double)power / 2);
                return value.Mantissa < 0 && (power & 1) == 1 ? -result : result;
            }

            // Exponent 오버플로우 방어
            long newExponent;
            try
            {
                newExponent = checked(value.Exponent * power);
            }
            catch (OverflowException)
            {
                // NaN의 지수는 long.MinValue 센티널이라 여기로 흘러올 수 있음 — Zero로 둔갑 방지
                if (IsNaN(value))
                    return NaN;

                bool positiveResult = mantissa > 0;
                bool exponentPositive =
                    (value.Exponent > 0 && power > 0) || (value.Exponent < 0 && power < 0);
                if (exponentPositive)
                    return positiveResult ? PositiveInfinity : NegativeInfinity;
                return Zero;
            }

            return Normalize(mantissa, newExponent);
        }

        public static BigDouble Pow(BigDouble value, double power)
        {
            // TODO: power can be greater that long.MaxValue, which can bring troubles in fast track
            var powerIsInteger = IsInteger(power);
            if (value < 0 && !powerIsInteger)
            {
                return NaN;
            }
            return Is10(value) && powerIsInteger ? Pow10(power) : PowInternal(value, power);
        }

        private static bool Is10(BigDouble value)
        {
            // Math.Abs 없이는 mantissa가 -1인 -10도 통과해 Pow(-10, n)의 부호가 사라짐
            return value.Exponent == 1 && Math.Abs(value.Mantissa - 1) < double.Epsilon;
        }

        private static BigDouble PowInternal(BigDouble value, double other)
        {
            //UN-SAFETY: Accuracy not guaranteed beyond ~9~11 decimal places.

            //TODO: Fast track seems about neutral for performance. It might become faster if an integer pow is implemented, or it might not be worth doing (see https://github.com/Patashu/break_infinity.js/issues/4 )

            //Fast track: If (this.exponent*value) is an integer and mantissa^value fits in a Number, we can do a very fast method.
            var temp = value.Exponent * other;
            double newMantissa;
            if (IsInteger(temp) && IsFinite(temp) && Math.Abs(temp) < ExpLimit)
            {
                newMantissa = Math.Pow(value.Mantissa, other);
                if (IsFinite(newMantissa))
                {
                    return Normalize(newMantissa, (long)temp);
                }
            }

            //Same speed and usually more accurate. (An arbitrary-precision version of this calculation is used in break_break_infinity.js, sacrificing performance for utter accuracy.)

            var newexponent = Math.Truncate(temp);
            var residue = temp - newexponent;
            newMantissa = Math.Pow(10, other * Math.Log10(value.Mantissa) + residue);
            if (IsFinite(newMantissa))
            {
                return Normalize(newMantissa, (long)newexponent);
            }

            //UN-SAFETY: This should return NaN when mantissa is negative and value is noninteger.
            var result = Pow10(other * AbsLog10(value)); //this is 2x faster and gives same values AFAIK
            if (Sign(value) == -1 && AreEqual(Math.Abs(other % 2), 1))
            {
                return -result;
            }

            return result;
        }

        public static BigDouble Factorial(BigDouble value)
        {
            //Using Stirling's Approximation. https://en.wikipedia.org/wiki/Stirling%27s_approximation#Versions_suitable_for_calculators

            var n = value.ToDouble() + 1;

            // 결과 지수 log10(n!) ≈ n·(log10 n − log10 e)가 long 범위를 넘으면 표현 불가.
            // 방치하면 Pow 내부의 double→long 포화 캐스팅이 가짜 유한값을 만들어냄
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
                // 음수의 mod는 음수이므로 != 0은 '1 또는 -1'을 의미
                return Normalize(
                    Math.Sqrt(value.Mantissa) * Sqrt10,
                    (long)Math.Floor(value.Exponent / 2.0)
                );
            }

            return Normalize(Math.Sqrt(value.Mantissa), (long)Math.Floor(value.Exponent / 2.0));
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

            // floor(e/3) 분해와 짝을 이루는 나머지는 항상 0..2여야 함 — C#의 %는 음수에서
            // 음수 나머지를 줘서 10^(1/3)과 10^(2/3) 보정이 서로 뒤바뀌었음 (예: 1e-4)
            var mod = ((value.Exponent % 3) + 3) % 3;
            if (mod == 1)
            {
                return Normalize(newmantissa * CubeRoot10, (long)Math.Floor(value.Exponent / 3.0));
            }

            if (mod == 2)
            {
                return Normalize(
                    newmantissa * TwoThirdsPowerOf10,
                    (long)Math.Floor(value.Exponent / 3.0)
                );
            }

            return Normalize(newmantissa, (long)Math.Floor(value.Exponent / 3.0));
        }

        public static BigDouble Sinh(BigDouble value)
        {
            return (Exp(value) - Exp(-value)) / 2;
        }

        public static BigDouble Cosh(BigDouble value)
        {
            return (Exp(value) + Exp(-value)) / 2;
        }

        public static BigDouble Tanh(BigDouble value)
        {
            return Sinh(value) / Cosh(value);
        }

        public static double Asinh(BigDouble value)
        {
            return Ln(value + Sqrt(Pow(value, 2) + 1));
        }

        public static double Acosh(BigDouble value)
        {
            return Ln(value + Sqrt(Pow(value, 2) - 1));
        }

        public static double Atanh(BigDouble value)
        {
            if (Abs(value) >= 1)
                return double.NaN;
            return Ln((value + 1) / (One - value)) / 2;
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

        private static bool ContainsExponentMarker(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (c == 'e' || c == 'E')
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// We need this lookup table because Math.pow(10, exponent) when exponent's absolute value
        /// is large is slightly inaccurate. you can fix it with the power of math... or just make
        /// a lookup table. Faster AND simpler.
        /// </summary>
        private static class PowersOf10
        {
            private static double[] Powers { get; } = new double[DoubleExpMax - DoubleExpMin];

            private const long IndexOf0 = -DoubleExpMin - 1;

            static PowersOf10()
            {
                // FastDouble.ParseDouble은 내부적으로 Math.Pow를 쓰므로 여기서 사용하면
                // 테이블의 존재 이유가 사라짐. 정확히 반올림된 값을 주는 double.Parse 사용
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
        // ThreadLocal로 스레드 안전성 확보
        private static readonly ThreadLocal<Random> ThreadLocalRandom = new ThreadLocal<Random>(
            () =>
                new Random()
        );
        private static Random Random => ThreadLocalRandom.Value!;

        /// <summary>
        /// This doesn't follow any kind of sane random distribution, so use this for testing purposes only.
        /// <para>5% of the time, mantissa is 0.</para>
        /// <para>10% of the time, mantissa is round.</para>
        /// </summary>
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

        /// <summary>
        /// If you're willing to spend 'resourcesAvailable' and want to buy something with
        /// exponentially increasing cost each purchase (start at priceStart, multiply by priceRatio,
        /// already own currentOwned), how much of it can you buy?
        /// <para>
        /// Adapted from Trimps source code.
        /// </para>
        /// </summary>
        public static BigDouble AffordGeometricSeries(
            BigDouble resourcesAvailable,
            BigDouble priceStart,
            BigDouble priceRatio,
            BigDouble currentOwned
        )
        {
            // 비율 1은 고정가라 등비 공식이 성립하지 않음 (log10(1) 나눗셈 → NaN)
            if (priceRatio == BigDouble.One)
            {
                return BigDouble.Floor(resourcesAvailable / priceStart);
            }

            var actualStart = priceStart * BigDouble.Pow(priceRatio, currentOwned);

            //return Math.floor(log10(((resourcesAvailable / (priceStart * Math.pow(priceRatio, currentOwned))) * (priceRatio - 1)) + 1) / log10(priceRatio));

            return BigDouble.Floor(
                BigDouble.Log10(resourcesAvailable / actualStart * (priceRatio - 1) + 1)
                    / BigDouble.Log10(priceRatio)
            );
        }

        /// <summary>
        /// How much resource would it cost to buy (numItems) items if you already have currentOwned,
        /// the initial price is priceStart and it multiplies by priceRatio each purchase?
        /// </summary>
        public static BigDouble SumGeometricSeries(
            BigDouble numItems,
            BigDouble priceStart,
            BigDouble priceRatio,
            BigDouble currentOwned
        )
        {
            // 비율 1은 고정가 — 등비 공식은 0/0이 됨
            if (priceRatio == BigDouble.One)
            {
                return priceStart * numItems;
            }

            var actualStart = priceStart * BigDouble.Pow(priceRatio, currentOwned);

            return actualStart * (1 - BigDouble.Pow(priceRatio, numItems)) / (1 - priceRatio);
        }

        /// <summary>
        /// If you're willing to spend 'resourcesAvailable' and want to buy something with
        /// additively increasing cost each purchase (start at priceStart, add by priceAdd,
        /// already own currentOwned), how much of it can you buy?
        /// </summary>
        public static BigDouble AffordArithmeticSeries(
            BigDouble resourcesAvailable,
            BigDouble priceStart,
            BigDouble priceAdd,
            BigDouble currentOwned
        )
        {
            var actualStart = priceStart + currentOwned * priceAdd;

            //n = (-(a-d/2) + sqrt((a-d/2)^2+2dS))/d
            //where a is actualStart, d is priceAdd and S is resourcesAvailable
            //then floor it and you're done!

            var b = actualStart - priceAdd / 2;
            var b2 = BigDouble.Pow(b, 2);

            return BigDouble.Floor(
                (BigDouble.Sqrt(b2 + priceAdd * resourcesAvailable * 2) - b) / priceAdd
            );
        }

        /// <summary>
        /// How much resource would it cost to buy (numItems) items if you already have currentOwned,
        /// the initial price is priceStart and it adds priceAdd each purchase?
        /// <para>
        /// Adapted from http://www.mathwords.com/a/arithmetic_series.htm
        /// </para>
        /// </summary>
        public static BigDouble SumArithmeticSeries(
            BigDouble numItems,
            BigDouble priceStart,
            BigDouble priceAdd,
            BigDouble currentOwned
        )
        {
            var actualStart = priceStart + currentOwned * priceAdd;

            //(n/2)*(2*a+(n-1)*d)

            return numItems / 2 * (2 * actualStart + (numItems - 1) * priceAdd);
        }

        /// <summary>
        /// When comparing two purchases that cost (resource) and increase your resource/sec by (delta_RpS),
        /// the lowest efficiency score is the better one to purchase.
        /// <para>
        /// From Frozen Cookies: http://cookieclicker.wikia.com/wiki/Frozen_Cookies_(JavaScript_Add-on)#Efficiency.3F_What.27s_that.3F
        /// </para>
        /// </summary>
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
        /// <summary>
        /// Joke function from Realm Grinder.
        /// </summary>
        public static BigDouble AscensionPenalty(this BigDouble value, double ascensions)
        {
            return Math.Abs(ascensions) < double.Epsilon
                ? value
                : BigDouble.Pow(value, Math.Pow(10, -ascensions));
        }

        /// <summary>
        /// Joke function from Cookie Clicker. It's an 'egg'.
        /// </summary>
        public static BigDouble Egg(this BigDouble value)
        {
            return value + 9;
        }
#endif
    }
}
