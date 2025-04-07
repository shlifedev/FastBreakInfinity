using System;
using System.Collections.Generic;
using System.Numerics; 
namespace LD.Numeric
{ 
    public partial struct BigDouble : IFormattable, IComparable, IComparable<BigDouble>, IEquatable<BigDouble>
    {
    
        /// <summary>
        /// 스트링을 계속 생성하지 않고 재사용한다.
        /// </summary> 
        private static Dictionary<int, string> CachedAlphabet = new Dictionary<int, string>(); 
        public enum eFormat
        {
            Number,
            NumberWithExponent
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
                    this.exponent = split.Exponent;
                    this.mantissa = split.Mantissa;
                    break;
                default:
                    this.exponent = 0;
                    this.mantissa = 0;
                    break;
            } 
        }

 
        public BigDouble(string value)
        { 
            if (value.IndexOf('e') != -1)
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