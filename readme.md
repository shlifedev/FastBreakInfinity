# Fast Idle Number

<p align="center">
<a href="/readme_ko.md">  한국어  </a>
<a href="/readme.md">  English  </a>
</p>

An extended fork of [BreakInfinity.cs](https://github.com/Razenpok/BreakInfinity.cs).

- Significantly reduces CPU time by efficiently calculating double digit counts in environments where decimal precision is not required.
- Includes an efficient alphabetic conversion process for digit counts.

## Why Fast?

The functions shown in the picture were called 1,000 times per frame.

<img width="820" alt="image" src="https://github.com/shlifedev/FastBigDouble/assets/49047211/3623a23a-961d-435a-a555-e6f618d227a3">

While **double.Parse** and **double.ToString** are versatile, they are very slow in specialized scenarios.

In games that use large numbers, such as idle games, floating-point precision is less critical. Creating a custom algorithm to parse doubles provides a significant performance advantage.

Unless you need to create new strings like with ToString, the code is **memory efficient** with near **zero GC**.

## Important Notice

Since `FastDouble.cs` only handles precision up to 6 decimal places, make sure to truncate any values beyond 6 decimal places when sending data to the server or performing similar operations.

ex ) 1.123456789e10 => 1.123456e10

--------

## How to Use

The usage is identical to BigInfinity.cs, but you must follow these rules:

```cs
BigDouble _ = new BigDouble("1000000000000000000000"); // Number Constructor
BigDouble _ = new BigDouble("9.999e100"); // Exponent Constructor - Very Fast!
new BigDouble(1e3).ToString() // Result = "1.0A"
```

-----

Many optimizations have been made.
If you want to minimize CPU time and memory usage in an environment where you frequently use big doubles, try this library.
