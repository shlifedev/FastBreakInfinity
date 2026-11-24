using System;
using System.Globalization;

namespace LD.Numeric.IdleNumber
{
    public struct BigValueInfo
    {
        public double Mantissa { get; set; }
        public long Exponent { get; set; }

        public static BigValueInfo ExponentFormatToBigValueInfo(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new FormatException("빈 문자열은 파싱할 수 없습니다.");
            }

            return Parse(input, input.AsSpan().IndexOfAny('e', 'E'));
        }

        internal static BigValueInfo Parse(string input, int exponentMarker)
        {
            var mantissaText = input.AsSpan();
            long exponent = 0;
            if (exponentMarker >= 0)
            {
                mantissaText = mantissaText.Slice(0, exponentMarker);
                var exponentText = input.AsSpan(exponentMarker + 1);
                // TryParse는 숫자 뒤의 NUL도 허용하므로 마지막 문자가 숫자인지 확인한다.
                if (
                    mantissaText.IsEmpty
                    || (uint)(input[input.Length - 1] - '0') > 9
                    || !long.TryParse(
                        exponentText,
                        NumberStyles.AllowLeadingSign,
                        CultureInfo.InvariantCulture,
                        out exponent
                    )
                )
                {
                    throw new FormatException("잘못된 지수 표기: " + input);
                }
            }

            return new BigValueInfo
            {
                Mantissa = FastDouble.ParseDouble(mantissaText, int.MaxValue),
                Exponent = exponent,
            };
        }
    }
}
