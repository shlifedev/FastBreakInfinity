using System.Collections.Generic;
using System.Linq;

namespace LD.Numeric
{
    using System;


    public static class PercentValueExtension
    {
        public static PercentValue Sum(this IEnumerable<PercentValue> values)
        {
            double sum = 0;
            foreach (var value in values) 
                sum += value.BasePercent;  
            return new PercentValue(sum);
        }
        
    
        public static BigPercentValue Sum(this IEnumerable<BigPercentValue> values)
        {
            BigDouble sum = 0;
            foreach (var value in values) 
                sum += value.BasePercent;  
            return new BigPercentValue(sum);
        }


        public static PercentValue Multiply(this IEnumerable<PercentValue> values)
        {
            var enumerator = values.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return new PercentValue(0);
            }

            PercentValue result = enumerator.Current; 
            while (enumerator.MoveNext())
            {
                Console.Write($"{result} * {enumerator.Current} = "  );
                result *= enumerator.Current;
                Console.WriteLine($"{result}");
            }

            return result;
        }
    }
    /// <summary>
    /// PercentValue는 1을 기준으로 하는 백분율 값을 나타냅니다.
    /// 예를 들어서 0.1은 10%입니다, 0.01은 1%입니다.
    /// - RPG 게임에서 데미지 증가량, 경험치 증가량 등을 표현할 때 사용할 수 있습니다.
    /// 당신의 플레이어 데미지가 100이라고 가정하고 100 * new PercentValue(1)을 하면 200이됩니다. (100%증가)
    /// </summary>
    public partial struct PercentValue
    {
        public PercentValue(double basePercent)
        {
            BasePercent = basePercent;
        } 

        /// <summary>
        /// 기존 값
        /// </summary>
        public double BasePercent { get; }

        /// <summary>
        /// 기존 값에 1을 더한 값
        /// </summary>
        public double AdjustedPercent => 1 + BasePercent;


        public override string ToString()
        {
            return ToString("F1");
        }

        public string ToString(string format)
        {
            return (BasePercent * 100).ToString(format) + "%";
        }


        public static implicit operator PercentValue(BigPercentValue value)
        {
            return new PercentValue(value.BasePercent.ToDouble());
        }


        public static implicit operator PercentValue(double value)
        {
            return new PercentValue(value);
        }

        public static implicit operator PercentValue(float value)
        {
            return new PercentValue(value);
        }



        public static double operator *(double a, PercentValue b)
        {
            return a * b.AdjustedPercent;
        }

        public static PercentValue operator *(PercentValue a, double b)
        {
            return new PercentValue(a.BasePercent * b);
        }
 
        public static float operator *(float a, PercentValue b)
        {
            return a * (float)b.AdjustedPercent;
        }

        public static PercentValue operator *(PercentValue a, float b)
        {
            return new PercentValue(a.BasePercent * b);
        }

        public static PercentValue operator *(PercentValue a, PercentValue b)
        {
            return new PercentValue(a.BasePercent * b.BasePercent);
        }



        public static PercentValue operator +(PercentValue a, double b)
        {
            return new PercentValue(a.BasePercent + b);
        }

        public static PercentValue operator -(PercentValue a, double b)
        {
            return new PercentValue(a.BasePercent - b);
        }
        
        public static PercentValue operator +(PercentValue a, float b)
        {
            return new PercentValue(a.BasePercent + b);
        }

        public static PercentValue operator -(PercentValue a, float b)
        {
            return new PercentValue(a.BasePercent - b);
        }

        /// <summary>
        /// 곱연산과 똑같습니다.
        /// 1 + 30% = 1.3d;
        /// </summary>  
        public static double operator +(double a, PercentValue b)
        {
            return a * b;
        }
        
        /// <summary>
        /// 곱연산과 똑같습니다.
        /// 1 + 30% = 1.3d;
        /// </summary>  
        public static float operator +(float a, PercentValue b)
        {
            return a * b;
        }

        /// <summary>
        /// 곱연산과 똑같습니다.
        /// 1 + -30% = 0.7d;
        /// </summary>  
        public static double operator -(double a, PercentValue b)
        {
            return a * new PercentValue(-b.BasePercent);
        }


        
        /// <summary>
        /// 곱연산과 똑같습니다.
        /// 1 + -30% = 0.7d;
        /// </summary>  
        public static float operator -(float a, PercentValue b)
        {
            return a * new PercentValue(-b.BasePercent);
        }


        public static PercentValue operator +(PercentValue a, PercentValue b)
        {
            return new PercentValue(a.BasePercent + b.BasePercent);
        }

        public static PercentValue operator -(PercentValue a, PercentValue b)
        {
            return new PercentValue(a.BasePercent - b.BasePercent);
        }




    }
}