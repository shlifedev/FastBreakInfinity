using System;
using System.Collections;
using System.Collections.Generic;
using LD;
using NUnit.Framework;
using UnityEditor.Experimental;
using UnityEditor.VersionControl;
using UnityEngine;
public class ToString
{
    [Test]
    public void DecimalPointTest()
    {
        // zero decimal point
        BigDouble p = new BigDouble(1.123456);
        Assert.IsTrue(p.ToString(0).Split(".").Length == 1);


        for (int i = 1; i < 15; i++)
        {
            var item = new BigDouble("1.1234567890");
            Assert.IsTrue(item.ToString(i).Split(".")[1].Length == i);
        }
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
}