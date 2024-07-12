using System; 

public static class NumberUtility
{
    public static int GetDigits(double value)
    { 
        int digits = 0;
        int number = Math.Abs((int)value);
        while (number > 0)
        {
            number /= 10;
            digits++;
        }
        

        return digits;
    }
}