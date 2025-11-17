using System;
using System.Collections.Generic; 
using LD.Numeric.IdleNumber; 

public partial class Program
{ 
    public static void Main()
    {
        
        BigDouble value = new BigDouble("1.132212345e99999");
        BigDouble value2 = new BigDouble("1.13221234500000999999e99999");
        Console.WriteLine(value);
        if (value == value2)
        {
            Console.WriteLine("Equal");
        }
        else
        {
            Console.WriteLine("Not Equal");
        }
    }
}
 