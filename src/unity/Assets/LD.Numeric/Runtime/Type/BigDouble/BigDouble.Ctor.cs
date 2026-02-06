using System;

namespace LD.Numeric.IdleNumber
{
    public partial struct BigDouble
    {
        public enum eFormat
        {
            Number,
            NumberWithExponent,
        }

        public BigDouble(string value, eFormat format)
        {
            switch (format)
            {
                case eFormat.Number:
                    this = BigDouble.Parse(value);
                    break;
                case eFormat.NumberWithExponent:
                    var split = BigValueInfo.ExponentFormatToBigValueInfo(value);
                    // mantissa를 1~10 범위로 정규화
                    this = Normalize(split.Mantissa, split.Exponent);
                    break;
                default:
                    this.exponent = 0;
                    this.mantissa = 0;
                    break;
            }
        }

        public BigDouble(string value)
        {
            if (value.IndexOf('e') != -1 || value.IndexOf('E') != -1)
            {
                this = new BigDouble(value, eFormat.NumberWithExponent);
            }
            else
            {
                this = BigDouble.Parse(value);
            }
        }
    }
}
