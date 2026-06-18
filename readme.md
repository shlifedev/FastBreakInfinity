# FastBreakInfinity

<p align="center">
<a href="/readme_ko.md">  한국어  </a>
<a href="/readme.md">  English  </a>
</p>

An extended fork of [BreakInfinity.cs](https://github.com/Razenpok/BreakInfinity.cs).

- Significantly reduces CPU time by efficiently calculating double digit counts in environments where decimal precision is not required.
- Includes an efficient alphabetic conversion process for digit counts.

<details>
<summary>What is better than BreakInfinity.cs?</summary>

- Optimized for idle-game hot paths instead of general-purpose numeric formatting.
- Uses a custom `FastDouble` parser to avoid the overhead of `double.Parse` in common save/load paths.
- Keeps large-number display fast with a narrow formatter tailored to compact idle-game notation.
- Converts exponent ranges to alphabet units directly, with caching for repeated UI updates.
- Avoids unnecessary allocations in arithmetic and most conversion paths, keeping GC pressure low.
- Preserves the familiar `BigDouble` API while focusing the implementation on CPU and memory cost.

</details>

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

```cs
using LD.Numeric.IdleNumber;
```

```cs
BigDouble gold = new BigDouble(0);
BigDouble cost = new BigDouble("1e100");

gold += new BigDouble("1e10");

if (gold >= cost)
{
    gold -= cost;
}
```

`BigDouble` supports normal arithmetic operators such as `+`, `-`, `*`, `/` and comparison operators such as `>`, `<`, `>=`, `<=`.

```cs
new BigDouble(999).ToString();    // "999"
new BigDouble(1000).ToString();   // "1.00A"
new BigDouble("1e6").ToString();  // "1.00B"
new BigDouble("1e81").ToString(); // "1.00AA"
```

Alphabet units are used every 1000x range:

```text
A = 1e3
B = 1e6
C = 1e9
...
Z = 1e78
AA = 1e81
AB = 1e84
```

`BigDouble` can theoretically display up to `1e9223372036854775807`, with
`AFEPVDFLPDCJTB` as the largest alphabet unit. Safe reverse conversion through
`GetExponentFromAlphabetUnit` is up to `AFEPVDFLPDCJTA`; this limit applies to
`BigDouble`, not plain `double`.

For save/load, exponent format is recommended:

```cs
BigDouble value = new BigDouble("1.234e100");

string saved = value.ToStringMantissaExponent();
BigDouble loaded = new BigDouble(saved);
```

-----

Many optimizations have been made.
If you want to minimize CPU time and memory usage in an environment where you frequently use big doubles, try this library.
