using LD.Numeric.IdleNumber;

namespace LD.Numeric.Test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }


    [Test]
    public void FastDoubleDecimalAccur()
    {
        var test = IdleNumber.FastDouble.ParseDouble("1.12345678123", 3); 
        Assert.IsTrue(test.ToString() == "1.123");
        var test2 = IdleNumber.FastDouble.ParseDouble("1.123123123321321123", 9); 
        Assert.IsTrue(test2.ToString() == "1.123123123");
    }
    [Test]
    public void MultiplyTestGreaterThan()
    {
        BigDouble value1 = new BigDouble("1.1e5");
        BigDouble value2 = new BigDouble("1.1e5");

        Console.WriteLine(value1.ToStringMantissaExponent()); 
        Console.WriteLine(value2.ToStringMantissaExponent()); 
        
        
        BigDouble result = value1 * value2; 
        
        Console.WriteLine(result.ToStringMantissaExponent());
        Assert.IsTrue(result >= new BigDouble("1.21e10")); 
    }
     
   
    [Test]
    public void Alphabet()
    {
        double p = 1.234d;

        for (long i = 0; i < (3 * 26 * 30 * 30) + 1; i++)
        {
            var bigDouble = new BigDouble(p, i);
            var str = bigDouble.ToString();
            switch (i)
            {
                case 0:
                    Assert.IsTrue(str == "1");
                    break;
                case 1:
                    Assert.IsTrue(str == "12");
                    break;
                case 2:
                    Assert.IsTrue(str == "123");
                    break;
                case 3:
                    Assert.IsTrue(str == "1.23A");
                    break;
                case 4:
                    Assert.IsTrue(str == "12.3A");
                    break;
                case 5:
                    Assert.IsTrue(str == "123A");
                    break;
                case 6:
                    Assert.IsTrue(str == "1.23B");
                    break;
                case (3 * 26):
                    Assert.IsTrue(str == "1.23Z");
                    break;

                case (3 * 26) + 3:
                    Assert.IsTrue(str == "1.23AA");
                    break;
                case (3 * 26) + 6:
                    Assert.IsTrue(str == "1.23AB");
                    break;
            }
        }
    }
    
    [Test]
    public void Scenario1MustBeZero()
    {
        BigDouble itemPrice = new BigDouble("1.1e1001");
        BigDouble money = new BigDouble("1.1e1001");
        if(money >= itemPrice) 
            money -= itemPrice; 
        Assert.IsTrue(money == 0);
        Console.WriteLine(money.ToString());
    }
    
    [Test]
    public void Scenario1MustBe9_9e1000()
    {
        BigDouble itemPrice = new BigDouble("1.1e1000");
        BigDouble money = new BigDouble("1.1e1001");
        if(money >= itemPrice) 
            money -= itemPrice; 
        Assert.IsTrue(money == new BigDouble("9.9e1000"));
        Console.WriteLine(money.ToStringMantissaExponent());
    } 
    /// <summary>
    /// 정확도 테스트 
    /// </summary>
    [Test]
    public void DigitAaccuracy()
    {
        {
            BigDouble value = new BigDouble("1.12345e999999");
            BigDouble value2 = new BigDouble("1.1234500000000000e999999");
            Assert.That(value == value2);
        }
        {
            BigDouble value = new BigDouble("1.12345e999999");
            BigDouble value2 = new BigDouble("1.12345e999999");
            Assert.That(value == value2);
        }
        {
            BigDouble value = new BigDouble("1.10000e123");
            BigDouble value2 = new BigDouble("1.100000000001e123");
            Assert.That(value < value2);
        } 
        {
            BigDouble value = new BigDouble("1.10000e123");
            BigDouble value2 = new BigDouble("1.100000000002e123");
            Assert.That(value < value2);
        } 
        {
            BigDouble value = new BigDouble("1.10000e123");
            BigDouble value2 = new BigDouble("1.10000000001e123");
            Assert.That(value < value2);
        }  
    }
}