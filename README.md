# LD.Numeric
 

When creating a game, you'll be dealing with complex types of values. 
Let's look at some examples

- Very large values over 10^100000000 (Most commonly used in idle games.)
- Values expressed as a percentage relative to 1 (often called percent value, To use an RPG game as an example, 100% increased power.)
- Values that require numeric units to be represented by alphabets (1000 => 1.0A, 1000000 => 1.0B)

I'm writing a tool to easily represent these types of values and apply them to calculations.


## Install
OpenUPM [https://openupm.com/packages/com.shlifedev.numeric/](https://openupm.com/packages/com.shlifedev.numeric/)



## 튜토리얼

<details>
    <summary>PercentValue 사용방법</summary>
 
## PercentValue
- 백분율 변수는 대입 한 값에 +1을 기준으로 한 조정된 값이 별도로 존재합니다.  


### 일반 double에 PercentValue를 곱 하는 경우 (좌항 double, 우항 백분율)
PercentValue은 일반 값과 연산시에 반드시 1을 추가로 포함한 AdjustedValue 라는 '조정된 값'을 사용합니다. 
이것이 의미하는 바는 실제 연산시에는 100 * 1.3 = 130 처럼 계산된다는 의미입니다.
 
```cs
        double Atk = 100; // My Player Attack Stat Value is '100'
        PercentValue Buff1 = 0.3f;  // Increased 30%, Adjusted 1.3f
        PercentValue Buff2 = 1.0f; // Increased 100%, Adjusted 2.0f 
        
        Console.WriteLine(Atk * Buff1); // 130
        Console.WriteLine((Atk * Buff1 )* Buff2); // 130 * 100% (2.0d) = 260%;
```

이 연산의 경우 곱셈 뿐만 아닌 덧셈을 통해서도 같은 결과를 얻을 수 있습니다. 
[공격력] 에 [퍼센트] 만큼 더한다. 라는 의미이기 때문입니다.

```cs
 double Atk = 100;  
 Atk += new PercentValue(0.3d);
 Ccnsole.WriteLine(Atk) // 130이 출력됩니다.
```


### PercentValue에 double을 곱 하는 경우 (좌항 백분율, 우항 double)
이 경우에는 PercentValue를 그대로 곱합니다. 
```cs
        PercentValue p1 = 0.5f; // 50%
        double p2 = 2; 
        Console.WriteLine(p1 * p2); // 50% * 2 = 100%;
```


### PercentValue 끼리 곱하는 경우 (좌,우항 모두 PercentValue)
이 경우는 특별합니다. PercentValue끼리의 곱은 백분율 끼리의 순수한 곱셈으로 동작합니다. 
```cs
        PercentValue p1 = 1; // 100%
        PercentValue p2 = 1;  // 100%
        Console.WriteLine(p1 * p2); // 100 * 100 = 10000%;
```

### PercentValue 끼리도 올바르게 곱하고 싶은경우
BasePercent 라는 '기본 값'을 제공하므로 좌항(double) 우항(PercentValue)연산을 수행할 수 있습니다.
```cs
        PercentValue p1 = 2; // 200%
        PercentValue p2 = 1;  // 100%
        Console.WriteLine(p1.BasePercent * p2); // 400%, 100%이 200%만큼 증가하여 400%이 되었다. 라는 맥락임.
```

</details>

### Thanks
[BreakInfinity](https://github.com/Razenpok/BreakInfinity.cs)

