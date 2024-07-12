using System;
using LD;

namespace LD.Numeric
{
 
    /// <summary>
    /// BigPercentValue는 1을 기준으로 하는 백분율 값을 나타냅니다.
    /// 예를 들어서 0.1은 10%입니다, 0.01은 1%입니다.
    /// - RPG 게임에서 데미지 증가량, 경험치 증가량 등을 표현할 때 사용할 수 있습니다.
    /// 당신의 플레이어 데미지가 100이라고 가정하고 100 * new BigPercentValue(1)을 하면 200이됩니다. (100%증가)
    /// </summary>
    public partial struct BigPercentValue
    {
        public BigPercentValue(BigDouble basePercent)
        {
            BasePercent = basePercent;
        }

        /// <summary>
        /// 기존 값
        /// </summary>
        public BigDouble BasePercent { get; }

        /// <summary>
        /// 기존 값에 1을 더한 값
        /// </summary>
        public BigDouble AdjustedPercent => 1 + BasePercent;


        public override string ToString()
        {
            return ToString("F1");
        }

        public string ToString(string format)
        {
            return (BasePercent * 100).ToString(1) + "%";
        }



        public static implicit operator BigPercentValue(BigDouble value)
        {
            return new BigPercentValue(value);
        }

        public static implicit operator BigPercentValue(double value)
        {
            return new BigPercentValue(value);
        }

        public static implicit operator BigPercentValue(PercentValue value)
        {
            return new BigPercentValue(value.BasePercent);
        }

        public static implicit operator BigPercentValue(float value)
        {
            return new BigPercentValue(value);
        }



        public static BigDouble operator *(BigDouble a, BigPercentValue b)
        {
            return a * b.AdjustedPercent;
        }

        public static BigPercentValue operator *(BigPercentValue a, BigDouble b)
        {
            return new BigPercentValue(a.BasePercent * b);
        }

        public static BigPercentValue operator *(BigPercentValue a, BigPercentValue b)
        {
            return new BigPercentValue(a.BasePercent * b.BasePercent);
        }



        public static BigPercentValue operator +(BigPercentValue a, BigDouble b)
        {
            return new BigPercentValue(a.BasePercent + b);
        }

        public static BigPercentValue operator -(BigPercentValue a, BigDouble b)
        {
            return new BigPercentValue(a.BasePercent - b);
        }

        /// <summary>
        /// 곱연산과 똑같습니다.
        /// 1 + 30% = 1.3d;
        /// </summary>  
        public static BigDouble operator +(BigDouble a, BigPercentValue b)
        {
            return a * b;
        }

        /// <summary>
        /// 곱연산과 똑같습니다.
        /// 1 + -30% = 0.7d;
        /// </summary>  
        public static BigDouble operator -(BigDouble a, BigPercentValue b)
        {
            return a * new BigPercentValue(-b.BasePercent);
        }


        public static BigPercentValue operator +(BigPercentValue a, BigPercentValue b)
        {
            return new BigPercentValue(a.BasePercent + b.BasePercent);
        }

        public static BigPercentValue operator -(BigPercentValue a, BigPercentValue b)
        {
            return new BigPercentValue(a.BasePercent - b.BasePercent);
        }




    }
}