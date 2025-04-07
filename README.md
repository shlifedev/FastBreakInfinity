# LD.Numeric

<p align="center">
<a href="/README_KR.md">  한국어  </a> 
<a href="/README.md">  English  </a>
</p>

It is an extended fork of [BreakInfinity] (https://github.com/Razenpok/BreakInfinity.cs).

- In an environment where decimal accuracy is not required, the CPU time has been significantly reduced by efficiently calculating the number of digits of double.
- In addition, an efficient alphabetic conversion process for the number of digits has been added.

Many optimisations have been made.
If you want to minimise CPU time and memory usage in an environment where you need to use big doubles frequently, try this library.
