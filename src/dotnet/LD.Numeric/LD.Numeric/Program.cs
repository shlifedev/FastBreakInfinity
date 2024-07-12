using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LD;
using LD.Numeric;
using LD.Numeric.ProbabilityTable;

public partial class Program
{
    static partial void HelloFrom(string name);
    public static void Main()
    {
        CharacterDataExample dataExample = new CharacterDataExample();
        dataExample.Damage = 100;
        dataExample.AddtionalEffects = new List<PercentValue>
        {
            new PercentValue(0.5),
            new PercentValue(0.1),
            new PercentValue(1.5),
        };
         
 Console.WriteLine(
         dataExample.AddtionalEffects.Multiply());
        HelloFrom("A");
        dataExample.Print();  
    }
    
}
 
public class CharacterDataExample
{
    public float Damage { get; set; }
    public List<PercentValue> AddtionalEffects { get; set; } = new();
    
    public float GetTotalDamage()
    {
        float defaultDamage = Damage;
        PercentValue addtinalPercentages = AddtionalEffects.Sum();
        float changedValue = defaultDamage + addtinalPercentages;
           
        
        return changedValue;
    }

    public void Print()
    {
        foreach (var effect in AddtionalEffects)
        {
            Console.WriteLine($"공격력 추가 효과 : {effect.ToString()} 만큼 증가했습니다.");
        }
        Console.WriteLine($"총합 : {AddtionalEffects.Sum().ToString()} 이 증가하여 {Damage} => {Damage+AddtionalEffects.Sum()} 이 되었음.");
    }
}