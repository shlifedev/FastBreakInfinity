using System;
using System.Collections.Generic; 
using LD.Numeric.IdleNumber; 

public partial class Program
{ 
    public static void Main()
    {
        BigDouble value = new BigDouble("1e99999");
        Console.WriteLine(value);
    }
}
 