# BigDouble 포괄적 테스트 코드 작성 계획

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** BigDouble의 소수점 정밀도 비교 및 핵심 연산에 대한 포괄적인 테스트 코드를 작성하여 코드 안정성 확보

**Architecture:** 기존 `UnitTest1.cs`에 새로운 테스트 메서드들을 추가. NUnit 프레임워크 사용. 각 테스트는 특정 엣지 케이스와 정밀도 문제를 검증하도록 설계

**Tech Stack:** C#, NUnit, BigDouble (LD.Numeric.IdleNumber)

---

## 분석된 잠재적 문제 영역

1. **비교 연산자의 소수점 정밀도** - `AreEqual`이 `Tolerance = 1e-18` 사용하지만, 실제 비교 연산자(`<`, `>`, `==`)에서 일관성 없음
2. **Normalize 후 가수 범위** - 가수가 정확히 1~10 범위인지 보장되지 않을 수 있음
3. **Add 연산의 정밀도** - 지수 차이가 큰 경우 1e14 스케일링 후 Round 처리
4. **문자열 파싱** - `FRACTIONAL_PART_ACCURITY = 6`으로 제한된 정밀도
5. **AdjustedMantissa의 부작용** - 내부에서 `mantissa = Math.Round(mantissa, 6)` 호출하여 원본 수정

---

## Task 1: 비교 연산자 기본 테스트

**Files:**
- Modify: `LD.Numeric.Test/UnitTest1.cs`

**Step 1: 같은 지수에서 가수 비교 테스트 작성**

```csharp
[Test]
public void Comparison_SameExponent_DifferentMantissa()
{
    // 같은 지수, 다른 가수 비교
    BigDouble a = new BigDouble(1.1, 100);
    BigDouble b = new BigDouble(1.2, 100);

    Assert.That(a < b, Is.True, "1.1e100 < 1.2e100");
    Assert.That(b > a, Is.True, "1.2e100 > 1.1e100");
    Assert.That(a != b, Is.True, "1.1e100 != 1.2e100");
}
```

**Step 2: 테스트 실행하여 통과 확인**

Run: `dotnet test LD.Numeric.Test/LD.Numeric.Test.csproj --filter "FullyQualifiedName~Comparison_SameExponent_DifferentMantissa" -v n`
Expected: PASS

**Step 3: 커밋**

```bash
git add LD.Numeric.Test/UnitTest1.cs
git commit -m "test: 같은 지수에서 가수 비교 테스트 추가"
```

---

## Task 2: 소수점 미세 차이 비교 테스트

**Files:**
- Modify: `LD.Numeric.Test/UnitTest1.cs`

**Step 1: 소수점 미세 차이 비교 테스트 작성**

```csharp
[Test]
public void Comparison_SmallMantissaDifference()
{
    // 소수점 아래 미세한 차이 비교
    BigDouble a = new BigDouble(1.000000001, 100);
    BigDouble b = new BigDouble(1.000000002, 100);

    Assert.That(a < b, Is.True, "1.000000001e100 < 1.000000002e100");
    Assert.That(a != b, Is.True, "미세 차이도 구분해야 함");
}

[Test]
public void Comparison_VerySmallMantissaDifference()
{
    // 더 미세한 차이 (Tolerance = 1e-18 근처)
    BigDouble a = new BigDouble(1.0, 100);
    BigDouble b = new BigDouble(1.0 + 1e-15, 100);

    // 이 차이가 감지되는지 확인
    Console.WriteLine($"a.Mantissa: {a.Mantissa}");
    Console.WriteLine($"b.Mantissa: {b.Mantissa}");
    Console.WriteLine($"diff: {b.Mantissa - a.Mantissa}");

    // double 정밀도 한계 내에서 다르다면 다르게 인식해야 함
    if (Math.Abs(b.Mantissa - a.Mantissa) > double.Epsilon)
    {
        Assert.That(a != b, Is.True, "감지 가능한 차이는 다르게 처리해야 함");
    }
}
```

**Step 2: 테스트 실행**

Run: `dotnet test LD.Numeric.Test/LD.Numeric.Test.csproj --filter "FullyQualifiedName~Comparison_SmallMantissaDifference|FullyQualifiedName~Comparison_VerySmallMantissaDifference" -v n`
Expected: PASS (또는 실패 시 정밀도 문제 발견)

**Step 3: 커밋**

```bash
git add LD.Numeric.Test/UnitTest1.cs
git commit -m "test: 소수점 미세 차이 비교 테스트 추가"
```

---

## Task 3: 다른 지수 비교 테스트

**Files:**
- Modify: `LD.Numeric.Test/UnitTest1.cs`

**Step 1: 다른 지수 비교 테스트 작성**

```csharp
[Test]
public void Comparison_DifferentExponent()
{
    // 다른 지수 비교 - 지수가 크면 무조건 큼
    BigDouble small = new BigDouble(9.9, 99);
    BigDouble large = new BigDouble(1.1, 100);

    Assert.That(small < large, Is.True, "9.9e99 < 1.1e100");
    Assert.That(large > small, Is.True, "1.1e100 > 9.9e99");
}

[Test]
public void Comparison_AdjacentExponent()
{
    // 인접 지수 비교 (경계값)
    BigDouble a = new BigDouble(9.999999999999999, 99);
    BigDouble b = new BigDouble(1.0, 100);

    // 9.999...e99 < 1.0e100 (수학적으로 거의 같지만 다름)
    Assert.That(a < b, Is.True, "9.999...e99 < 1.0e100");
}
```

**Step 2: 테스트 실행**

Run: `dotnet test LD.Numeric.Test/LD.Numeric.Test.csproj --filter "FullyQualifiedName~Comparison_DifferentExponent|FullyQualifiedName~Comparison_AdjacentExponent" -v n`
Expected: PASS

**Step 3: 커밋**

```bash
git add LD.Numeric.Test/UnitTest1.cs
git commit -m "test: 다른 지수 비교 테스트 추가"
```

---

## Task 4: 음수 비교 테스트

**Files:**
- Modify: `LD.Numeric.Test/UnitTest1.cs`

**Step 1: 음수 비교 테스트 작성**

```csharp
[Test]
public void Comparison_NegativeNumbers()
{
    // 음수 비교
    BigDouble a = new BigDouble(-1.1, 100);  // -1.1e100
    BigDouble b = new BigDouble(-1.2, 100);  // -1.2e100

    // -1.1e100 > -1.2e100 (음수에서는 절대값이 작은 것이 큼)
    Assert.That(a > b, Is.True, "-1.1e100 > -1.2e100");
    Assert.That(b < a, Is.True, "-1.2e100 < -1.1e100");
}

[Test]
public void Comparison_NegativeVsPositive()
{
    // 양수 vs 음수
    BigDouble positive = new BigDouble(1.0, 100);
    BigDouble negative = new BigDouble(-1.0, 100);

    Assert.That(positive > negative, Is.True, "양수 > 음수");
    Assert.That(negative < positive, Is.True, "음수 < 양수");
    Assert.That(positive > BigDouble.Zero, Is.True, "양수 > 0");
    Assert.That(negative < BigDouble.Zero, Is.True, "음수 < 0");
}

[Test]
public void Comparison_NegativeDifferentExponent()
{
    // 음수에서 다른 지수 비교
    BigDouble a = new BigDouble(-1.1, 100);  // -1.1e100
    BigDouble b = new BigDouble(-1.1, 99);   // -1.1e99

    // -1.1e100 < -1.1e99 (절대값이 큰 음수가 더 작음)
    Assert.That(a < b, Is.True, "-1.1e100 < -1.1e99");
}
```

**Step 2: 테스트 실행**

Run: `dotnet test LD.Numeric.Test/LD.Numeric.Test.csproj --filter "FullyQualifiedName~Comparison_Negative" -v n`
Expected: PASS

**Step 3: 커밋**

```bash
git add LD.Numeric.Test/UnitTest1.cs
git commit -m "test: 음수 비교 테스트 추가"
```

---

## Task 5: 특수값(NaN, Infinity, Zero) 비교 테스트

**Files:**
- Modify: `LD.Numeric.Test/UnitTest1.cs`

**Step 1: 특수값 비교 테스트 작성**

```csharp
[Test]
public void Comparison_SpecialValues_NaN()
{
    // NaN 비교 - 모든 비교가 false여야 함
    BigDouble nan = BigDouble.NaN;
    BigDouble normal = new BigDouble(1.0, 100);

    Assert.That(nan == nan, Is.False, "NaN == NaN should be false");
    Assert.That(nan < normal, Is.False, "NaN < normal should be false");
    Assert.That(nan > normal, Is.False, "NaN > normal should be false");
    Assert.That(nan <= normal, Is.False, "NaN <= normal should be false");
    Assert.That(nan >= normal, Is.False, "NaN >= normal should be false");
}

[Test]
public void Comparison_SpecialValues_Infinity()
{
    // Infinity 비교
    BigDouble posInf = BigDouble.PositiveInfinity;
    BigDouble negInf = BigDouble.NegativeInfinity;
    BigDouble normal = new BigDouble(9.9, 999999);

    Assert.That(posInf > normal, Is.True, "+Inf > any normal");
    Assert.That(negInf < normal, Is.True, "-Inf < any normal");
    Assert.That(posInf > negInf, Is.True, "+Inf > -Inf");
}

[Test]
public void Comparison_SpecialValues_Zero()
{
    // Zero 비교
    BigDouble zero1 = BigDouble.Zero;
    BigDouble zero2 = new BigDouble(0.0);
    BigDouble positive = new BigDouble(1e-300);
    BigDouble negative = new BigDouble(-1e-300);

    Assert.That(zero1 == zero2, Is.True, "Zero == Zero");
    Assert.That(zero1 < positive, Is.True, "0 < 양수");
    Assert.That(zero1 > negative, Is.True, "0 > 음수");
}
```

**Step 2: 테스트 실행**

Run: `dotnet test LD.Numeric.Test/LD.Numeric.Test.csproj --filter "FullyQualifiedName~Comparison_SpecialValues" -v n`
Expected: PASS

**Step 3: 커밋**

```bash
git add LD.Numeric.Test/UnitTest1.cs
git commit -m "test: 특수값(NaN, Infinity, Zero) 비교 테스트 추가"
```

---

## Task 6: Equals 메서드 테스트

**Files:**
- Modify: `LD.Numeric.Test/UnitTest1.cs`

**Step 1: Equals 메서드 테스트 작성**

```csharp
[Test]
public void Equals_ExactMatch()
{
    BigDouble a = new BigDouble("1.23456789e100");
    BigDouble b = new BigDouble("1.23456789e100");

    Assert.That(a.Equals(b), Is.True);
    Assert.That(a == b, Is.True);
}

[Test]
public void Equals_WithTolerance()
{
    BigDouble a = new BigDouble(1.0, 100);
    BigDouble b = new BigDouble(1.0 + 1e-10, 100);

    // 기본 Equals는 false
    Assert.That(a.Equals(b), Is.False, "정확한 비교는 false");

    // tolerance 있으면 true
    Assert.That(a.Equals(b, 1e-9), Is.True, "1e-9 tolerance로는 같음");
    Assert.That(a.Equals(b, 1e-11), Is.False, "1e-11 tolerance로는 다름");
}

[Test]
public void Equals_DifferentRepresentationSameValue()
{
    // 같은 값이지만 다른 표현
    BigDouble a = new BigDouble(10.0, 99);   // 정규화 전
    BigDouble b = new BigDouble(1.0, 100);   // 정규화 후

    // 생성자에서 정규화되므로 같아야 함
    Console.WriteLine($"a: {a.Mantissa}e{a.Exponent}");
    Console.WriteLine($"b: {b.Mantissa}e{b.Exponent}");

    Assert.That(a == b, Is.True, "10e99 == 1e100");
}
```

**Step 2: 테스트 실행**

Run: `dotnet test LD.Numeric.Test/LD.Numeric.Test.csproj --filter "FullyQualifiedName~Equals_" -v n`
Expected: PASS

**Step 3: 커밋**

```bash
git add LD.Numeric.Test/UnitTest1.cs
git commit -m "test: Equals 메서드 테스트 추가"
```

---

## Task 7: Add/Subtract 정밀도 테스트

**Files:**
- Modify: `LD.Numeric.Test/UnitTest1.cs`

**Step 1: 덧셈/뺄셈 정밀도 테스트 작성**

```csharp
[Test]
public void Add_SameExponent()
{
    BigDouble a = new BigDouble(1.5, 100);
    BigDouble b = new BigDouble(2.5, 100);
    BigDouble result = a + b;

    Assert.That(result == new BigDouble(4.0, 100), Is.True, "1.5e100 + 2.5e100 = 4e100");
}

[Test]
public void Add_DifferentExponent_Small()
{
    BigDouble a = new BigDouble(1.0, 100);
    BigDouble b = new BigDouble(1.0, 99);
    BigDouble result = a + b;

    // 1e100 + 1e99 = 1.1e100
    Assert.That(result == new BigDouble(1.1, 100), Is.True, "1e100 + 1e99 = 1.1e100");
}

[Test]
public void Add_DifferentExponent_Large()
{
    // 지수 차이가 17 이상이면 큰 값만 반환
    BigDouble a = new BigDouble(1.0, 100);
    BigDouble b = new BigDouble(1.0, 80);  // 차이 20
    BigDouble result = a + b;

    Assert.That(result == a, Is.True, "지수 차이 > 17이면 큰 값 반환");
}

[Test]
public void Subtract_SameValue_ShouldBeZero()
{
    BigDouble a = new BigDouble("1.23456789e1000");
    BigDouble b = new BigDouble("1.23456789e1000");
    BigDouble result = a - b;

    Assert.That(result == BigDouble.Zero, Is.True, "같은 값 빼기 = 0");
}

[Test]
public void Subtract_CloseValues()
{
    // 가까운 값의 뺄셈 - 정밀도 손실 가능성
    BigDouble a = new BigDouble(1.000001, 100);
    BigDouble b = new BigDouble(1.0, 100);
    BigDouble result = a - b;

    // 1.000001e100 - 1e100 = 0.000001e100 = 1e94
    Console.WriteLine($"Result: {result.Mantissa}e{result.Exponent}");

    // 대략 1e94 근처여야 함
    Assert.That(result.Exponent >= 93 && result.Exponent <= 95, Is.True,
        "1.000001e100 - 1e100 ≈ 1e94");
}
```

**Step 2: 테스트 실행**

Run: `dotnet test LD.Numeric.Test/LD.Numeric.Test.csproj --filter "FullyQualifiedName~Add_|FullyQualifiedName~Subtract_" -v n`
Expected: PASS (또는 실패 시 정밀도 문제 발견)

**Step 3: 커밋**

```bash
git add LD.Numeric.Test/UnitTest1.cs
git commit -m "test: Add/Subtract 정밀도 테스트 추가"
```

---

## Task 8: Multiply/Divide 테스트

**Files:**
- Modify: `LD.Numeric.Test/UnitTest1.cs`

**Step 1: 곱셈/나눗셈 테스트 작성**

```csharp
[Test]
public void Multiply_Basic()
{
    BigDouble a = new BigDouble(2.0, 100);
    BigDouble b = new BigDouble(3.0, 50);
    BigDouble result = a * b;

    // 2e100 * 3e50 = 6e150
    Assert.That(result == new BigDouble(6.0, 150), Is.True, "2e100 * 3e50 = 6e150");
}

[Test]
public void Multiply_Overflow()
{
    // 가수 곱이 10 이상인 경우
    BigDouble a = new BigDouble(5.0, 100);
    BigDouble b = new BigDouble(5.0, 100);
    BigDouble result = a * b;

    // 5e100 * 5e100 = 25e200 = 2.5e201
    Assert.That(result.Mantissa >= 2.4 && result.Mantissa <= 2.6, Is.True, "가수 2.5");
    Assert.That(result.Exponent == 201, Is.True, "지수 201");
}

[Test]
public void Divide_Basic()
{
    BigDouble a = new BigDouble(6.0, 150);
    BigDouble b = new BigDouble(2.0, 100);
    BigDouble result = a / b;

    // 6e150 / 2e100 = 3e50
    Assert.That(result == new BigDouble(3.0, 50), Is.True, "6e150 / 2e100 = 3e50");
}

[Test]
public void Divide_ByZero()
{
    BigDouble a = new BigDouble(1.0, 100);
    BigDouble result = a / BigDouble.Zero;

    Assert.That(BigDouble.IsPositiveInfinity(result), Is.True, "양수 / 0 = +Inf");

    BigDouble negResult = (-a) / BigDouble.Zero;
    Assert.That(BigDouble.IsNegativeInfinity(negResult), Is.True, "음수 / 0 = -Inf");
}
```

**Step 2: 테스트 실행**

Run: `dotnet test LD.Numeric.Test/LD.Numeric.Test.csproj --filter "FullyQualifiedName~Multiply_|FullyQualifiedName~Divide_" -v n`
Expected: PASS

**Step 3: 커밋**

```bash
git add LD.Numeric.Test/UnitTest1.cs
git commit -m "test: Multiply/Divide 테스트 추가"
```

---

## Task 9: Normalize 테스트

**Files:**
- Modify: `LD.Numeric.Test/UnitTest1.cs`

**Step 1: Normalize 테스트 작성**

```csharp
[Test]
public void Normalize_MantissaInRange()
{
    // 정규화 후 가수는 1 <= |m| < 10
    var testCases = new[]
    {
        (0.5, 100L),
        (50.0, 100L),
        (0.001, 100L),
        (999.9, 100L),
    };

    foreach (var (m, e) in testCases)
    {
        BigDouble bd = new BigDouble(m, e);
        double absMantissa = Math.Abs(bd.Mantissa);

        Assert.That(absMantissa >= 1 && absMantissa < 10 || bd.Mantissa == 0, Is.True,
            $"Normalize({m}, {e}) => 가수 범위 확인: {bd.Mantissa}");
    }
}

[Test]
public void Normalize_PreservesValue()
{
    // 정규화 전후 값이 같아야 함
    BigDouble a = new BigDouble(123.456, 100);

    // 123.456e100 = 1.23456e102
    Assert.That(a.Mantissa >= 1.23 && a.Mantissa <= 1.24, Is.True, "가수 정규화");
    Assert.That(a.Exponent == 102, Is.True, "지수 조정");
}

[Test]
public void Normalize_VerySmallMantissa()
{
    // 매우 작은 가수
    BigDouble bd = new BigDouble(1e-10, 100);

    // 1e-10 * e100 = 1e90
    Assert.That(bd.Mantissa >= 1 && bd.Mantissa < 10, Is.True);
    Assert.That(bd.Exponent == 90, Is.True);
}
```

**Step 2: 테스트 실행**

Run: `dotnet test LD.Numeric.Test/LD.Numeric.Test.csproj --filter "FullyQualifiedName~Normalize_" -v n`
Expected: PASS

**Step 3: 커밋**

```bash
git add LD.Numeric.Test/UnitTest1.cs
git commit -m "test: Normalize 테스트 추가"
```

---

## Task 10: 문자열 파싱 테스트

**Files:**
- Modify: `LD.Numeric.Test/UnitTest1.cs`

**Step 1: 문자열 파싱 테스트 작성**

```csharp
[Test]
public void Parse_ExponentFormat()
{
    // 지수 형식 파싱
    var testCases = new[]
    {
        ("1e100", 1.0, 100L),
        ("1.5e100", 1.5, 100L),
        ("1.23456e999", 1.23456, 999L),
        ("-1.5e100", -1.5, 100L),
        ("9.9e-50", 9.9, -50L),
    };

    foreach (var (str, expectedM, expectedE) in testCases)
    {
        BigDouble bd = new BigDouble(str);

        Assert.That(Math.Abs(bd.Mantissa - expectedM) < 1e-10, Is.True,
            $"Parse '{str}' 가수: {bd.Mantissa} vs {expectedM}");
        Assert.That(bd.Exponent == expectedE, Is.True,
            $"Parse '{str}' 지수: {bd.Exponent} vs {expectedE}");
    }
}

[Test]
public void Parse_PlainNumber()
{
    // 일반 숫자 파싱
    BigDouble bd = new BigDouble("12345.6789");

    Console.WriteLine($"Parsed: {bd.Mantissa}e{bd.Exponent}");

    // 12345.6789 ≈ 1.23456789e4
    Assert.That(bd.Exponent == 4, Is.True);
    Assert.That(Math.Abs(bd.Mantissa - 1.23456789) < 1e-5, Is.True);
}

[Test]
public void Parse_LargeExponent()
{
    // 매우 큰 지수
    BigDouble bd = new BigDouble("1e999999999");

    Assert.That(bd.Mantissa == 1.0, Is.True);
    Assert.That(bd.Exponent == 999999999L, Is.True);
}

[Test]
public void Parse_PrecisionLimit()
{
    // 정밀도 제한 확인 (FRACTIONAL_PART_ACCURITY = 6)
    BigDouble a = new BigDouble("1.123456789e100");
    BigDouble b = new BigDouble("1.123456e100");

    // 소수점 6자리 이후는 무시될 수 있음
    Console.WriteLine($"a: {a.Mantissa}e{a.Exponent}");
    Console.WriteLine($"b: {b.Mantissa}e{b.Exponent}");

    // 파싱 시 정밀도가 제한되면 같아짐
    // 또는 다르다면 정밀도가 유지됨
    // 결과 확인용 테스트
}
```

**Step 2: 테스트 실행**

Run: `dotnet test LD.Numeric.Test/LD.Numeric.Test.csproj --filter "FullyQualifiedName~Parse_" -v n`
Expected: PASS

**Step 3: 커밋**

```bash
git add LD.Numeric.Test/UnitTest1.cs
git commit -m "test: 문자열 파싱 테스트 추가"
```

---

## Task 11: Pow/Log 테스트

**Files:**
- Modify: `LD.Numeric.Test/UnitTest1.cs`

**Step 1: Pow/Log 테스트 작성**

```csharp
[Test]
public void Pow_IntegerPower()
{
    BigDouble a = new BigDouble(2.0, 10);
    BigDouble result = BigDouble.Pow(a, 3);

    // (2e10)^3 = 8e30
    Assert.That(result.Mantissa == 8.0, Is.True);
    Assert.That(result.Exponent == 30, Is.True);
}

[Test]
public void Pow_FractionalPower()
{
    BigDouble a = new BigDouble(4.0, 100);
    BigDouble result = BigDouble.Pow(a, 0.5);  // sqrt

    // sqrt(4e100) = 2e50
    Assert.That(Math.Abs(result.Mantissa - 2.0) < 0.01, Is.True, "가수 2.0");
    Assert.That(result.Exponent == 50, Is.True, "지수 50");
}

[Test]
public void Pow10_Basic()
{
    BigDouble result = BigDouble.Pow10(100);

    Assert.That(result.Mantissa == 1.0, Is.True);
    Assert.That(result.Exponent == 100, Is.True);
}

[Test]
public void Log10_Basic()
{
    BigDouble a = new BigDouble(5.0, 100);
    double result = BigDouble.Log10(a);

    // log10(5e100) = log10(5) + 100 ≈ 100.699
    Assert.That(Math.Abs(result - 100.699) < 0.01, Is.True);
}

[Test]
public void Sqrt_Basic()
{
    BigDouble a = new BigDouble(4.0, 100);
    BigDouble result = BigDouble.Sqrt(a);

    // sqrt(4e100) = 2e50
    Assert.That(Math.Abs(result.Mantissa - 2.0) < 0.01, Is.True);
    Assert.That(result.Exponent == 50, Is.True);
}

[Test]
public void Sqrt_OddExponent()
{
    BigDouble a = new BigDouble(4.0, 101);  // 홀수 지수
    BigDouble result = BigDouble.Sqrt(a);

    // sqrt(4e101) = sqrt(40e100) = sqrt(40) * e50 ≈ 6.32e50
    Console.WriteLine($"sqrt(4e101) = {result.Mantissa}e{result.Exponent}");
    Assert.That(result.Exponent == 50, Is.True);
}
```

**Step 2: 테스트 실행**

Run: `dotnet test LD.Numeric.Test/LD.Numeric.Test.csproj --filter "FullyQualifiedName~Pow_|FullyQualifiedName~Pow10_|FullyQualifiedName~Log10_|FullyQualifiedName~Sqrt_" -v n`
Expected: PASS

**Step 3: 커밋**

```bash
git add LD.Numeric.Test/UnitTest1.cs
git commit -m "test: Pow/Log/Sqrt 테스트 추가"
```

---

## Task 12: Round/Floor/Ceiling 테스트

**Files:**
- Modify: `LD.Numeric.Test/UnitTest1.cs`

**Step 1: 반올림 함수 테스트 작성**

```csharp
[Test]
public void Round_SmallExponent()
{
    BigDouble a = new BigDouble(1.5, 2);  // 150
    BigDouble result = BigDouble.Round(a);

    // Round(150) = 150
    Assert.That(result == new BigDouble(150), Is.True);
}

[Test]
public void Round_LargeExponent()
{
    // 지수가 MaxSignificantDigits 이상이면 그대로 반환
    BigDouble a = new BigDouble(1.5, 100);
    BigDouble result = BigDouble.Round(a);

    Assert.That(result == a, Is.True, "큰 지수는 반올림 무시");
}

[Test]
public void Floor_Basic()
{
    BigDouble a = new BigDouble(1.9, 2);  // 190
    BigDouble result = BigDouble.Floor(a);

    Assert.That(result == new BigDouble(190), Is.True);
}

[Test]
public void Ceiling_Basic()
{
    BigDouble a = new BigDouble(1.1, 2);  // 110
    BigDouble result = BigDouble.Ceiling(a);

    Assert.That(result == new BigDouble(110), Is.True);
}

[Test]
public void Truncate_NegativeNumber()
{
    BigDouble a = new BigDouble(-1.9, 2);  // -190
    BigDouble result = BigDouble.Truncate(a);

    // Truncate(-190) = -190 (정수라서 그대로)
    Assert.That(result == new BigDouble(-190), Is.True);
}
```

**Step 2: 테스트 실행**

Run: `dotnet test LD.Numeric.Test/LD.Numeric.Test.csproj --filter "FullyQualifiedName~Round_|FullyQualifiedName~Floor_|FullyQualifiedName~Ceiling_|FullyQualifiedName~Truncate_" -v n`
Expected: PASS

**Step 3: 커밋**

```bash
git add LD.Numeric.Test/UnitTest1.cs
git commit -m "test: Round/Floor/Ceiling/Truncate 테스트 추가"
```

---

## Task 13: AdjustedMantissa 부작용 테스트

**Files:**
- Modify: `LD.Numeric.Test/UnitTest1.cs`

**Step 1: AdjustedMantissa 부작용 테스트 작성**

```csharp
[Test]
public void AdjustedMantissa_SideEffect()
{
    // AdjustedMantissa가 내부적으로 mantissa를 수정하는 문제 확인
    BigDouble original = new BigDouble(1.23456789, 100);
    double originalMantissa = original.Mantissa;

    // AdjustedMantissa 호출
    double adjusted = original.AdjustedMantissa();

    // 원본이 수정되었는지 확인
    Console.WriteLine($"Original mantissa before: {originalMantissa}");
    Console.WriteLine($"Original mantissa after: {original.Mantissa}");
    Console.WriteLine($"Adjusted mantissa: {adjusted}");

    // 주의: 현재 구현에서 mantissa가 Round(mantissa, 6)으로 수정됨
    // 이것이 의도된 동작인지 버그인지 확인 필요
}

[Test]
public void AdjustedMantissa_ConsistentResults()
{
    BigDouble bd = new BigDouble(1.234567, 5);  // 123456.7

    double first = bd.AdjustedMantissa();
    double second = bd.AdjustedMantissa();

    Assert.That(first == second, Is.True, "연속 호출 시 일관된 결과");
}
```

**Step 2: 테스트 실행**

Run: `dotnet test LD.Numeric.Test/LD.Numeric.Test.csproj --filter "FullyQualifiedName~AdjustedMantissa_" -v n`
Expected: PASS (또는 부작용 발견)

**Step 3: 커밋**

```bash
git add LD.Numeric.Test/UnitTest1.cs
git commit -m "test: AdjustedMantissa 부작용 테스트 추가"
```

---

## Task 14: 경계값 및 극단 케이스 테스트

**Files:**
- Modify: `LD.Numeric.Test/UnitTest1.cs`

**Step 1: 경계값 테스트 작성**

```csharp
[Test]
public void EdgeCase_MaxExponent()
{
    // 최대 지수
    BigDouble bd = new BigDouble(1.0, long.MaxValue - 1);

    Assert.That(bd.Exponent == long.MaxValue - 1, Is.True);
}

[Test]
public void EdgeCase_MinExponent()
{
    // 최소 지수
    BigDouble bd = new BigDouble(1.0, long.MinValue + 1);

    // 정상 생성 확인
    Assert.That(!BigDouble.IsNaN(bd), Is.True);
}

[Test]
public void EdgeCase_DoublePrecisionLimit()
{
    // double 정밀도 한계 (약 15-17자리)
    BigDouble a = new BigDouble(1.0000000000000001, 100);  // 16자리
    BigDouble b = new BigDouble(1.0000000000000002, 100);  // 16자리

    Console.WriteLine($"a.Mantissa: {a.Mantissa:R}");
    Console.WriteLine($"b.Mantissa: {b.Mantissa:R}");

    // double 정밀도 한계로 같을 수 있음
    if (a.Mantissa != b.Mantissa)
    {
        Assert.That(a != b, Is.True);
    }
}

[Test]
public void EdgeCase_Denormalized()
{
    // 비정규화 숫자
    BigDouble bd = new BigDouble(double.Epsilon, 0);

    Console.WriteLine($"Epsilon: {bd.Mantissa}e{bd.Exponent}");
    Assert.That(!BigDouble.IsNaN(bd), Is.True);
    Assert.That(!BigDouble.IsInfinity(bd), Is.True);
}
```

**Step 2: 테스트 실행**

Run: `dotnet test LD.Numeric.Test/LD.Numeric.Test.csproj --filter "FullyQualifiedName~EdgeCase_" -v n`
Expected: PASS

**Step 3: 커밋**

```bash
git add LD.Numeric.Test/UnitTest1.cs
git commit -m "test: 경계값 및 극단 케이스 테스트 추가"
```

---

## Task 15: 게임 시나리오 테스트 (실제 사용 케이스)

**Files:**
- Modify: `LD.Numeric.Test/UnitTest1.cs`

**Step 1: 게임 시나리오 테스트 작성**

```csharp
[Test]
public void GameScenario_CurrencyAccumulation()
{
    // 재화 누적 시나리오
    BigDouble currency = BigDouble.Zero;
    BigDouble income = new BigDouble(1.5, 10);  // 초당 1.5e10

    // 1000초 누적
    for (int i = 0; i < 1000; i++)
    {
        currency += income;
    }

    // 1000 * 1.5e10 = 1.5e13
    BigDouble expected = new BigDouble(1.5, 13);

    Assert.That(currency.Equals(expected, 1e-10), Is.True,
        $"누적 결과: {currency.Mantissa}e{currency.Exponent}");
}

[Test]
public void GameScenario_Purchase()
{
    // 구매 시나리오
    BigDouble money = new BigDouble("1.5e100");
    BigDouble price = new BigDouble("1.2e100");

    Assert.That(money >= price, Is.True, "구매 가능 확인");

    money -= price;

    // 1.5e100 - 1.2e100 = 0.3e100 = 3e99
    Assert.That(money.Exponent == 99, Is.True);
    Assert.That(Math.Abs(money.Mantissa - 3.0) < 0.1, Is.True);
}

[Test]
public void GameScenario_Multiplier()
{
    // 배율 적용 시나리오
    BigDouble baseValue = new BigDouble("1e50");
    BigDouble multiplier = new BigDouble(2.5);

    BigDouble result = baseValue * multiplier;

    // 1e50 * 2.5 = 2.5e50
    Assert.That(result == new BigDouble(2.5, 50), Is.True);
}

[Test]
public void GameScenario_ExponentialGrowth()
{
    // 지수 성장 시나리오
    BigDouble value = new BigDouble(1.0);
    BigDouble growthRate = new BigDouble(1.1);  // 10% 성장

    // 100번 성장
    for (int i = 0; i < 100; i++)
    {
        value *= growthRate;
    }

    // 1.1^100 ≈ 1.38e4
    Console.WriteLine($"100번 1.1배 성장: {value.Mantissa}e{value.Exponent}");

    Assert.That(value.Exponent >= 3 && value.Exponent <= 5, Is.True,
        "1.1^100의 지수는 4 근처");
}
```

**Step 2: 테스트 실행**

Run: `dotnet test LD.Numeric.Test/LD.Numeric.Test.csproj --filter "FullyQualifiedName~GameScenario_" -v n`
Expected: PASS

**Step 3: 커밋**

```bash
git add LD.Numeric.Test/UnitTest1.cs
git commit -m "test: 게임 시나리오 테스트 추가"
```

---

## Task 16: 전체 테스트 실행 및 결과 분석

**Files:**
- None (테스트 실행만)

**Step 1: 전체 테스트 실행**

Run: `dotnet test LD.Numeric.Test/LD.Numeric.Test.csproj -v n`
Expected: 모든 테스트 PASS 또는 실패한 테스트 목록

**Step 2: CSharpier 포맷팅 적용**

Run: `dotnet csharpier LD.Numeric.Test/UnitTest1.cs`

**Step 3: 최종 커밋**

```bash
git add LD.Numeric.Test/UnitTest1.cs
git commit -m "test: BigDouble 포괄적 테스트 완료"
```

---

## 예상 발견 가능한 문제

1. **AdjustedMantissa 부작용**: 내부에서 `mantissa = Math.Round(mantissa, 6)` 호출하여 struct의 원본 수정 (struct이므로 복사본이 수정되지만 혼란 가능)

2. **소수점 비교 일관성**: `==` 연산자는 `AreEqual(Mantissa, other.Mantissa)`를 사용하지만 `<`, `>` 연산자는 직접 비교

3. **파싱 정밀도**: `FRACTIONAL_PART_ACCURITY = 6`으로 소수점 6자리 제한

4. **Add 연산 반올림**: `Math.Round(1e14 * ...)` 에서 정밀도 손실 가능

## 테스트 추가 후 다음 단계

테스트 실행 결과 실패한 테스트가 있다면:
1. 실패 원인 분석
2. 의도된 동작인지 버그인지 판단
3. 버그라면 수정 계획 수립
