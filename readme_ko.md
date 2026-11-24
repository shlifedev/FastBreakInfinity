# FastBreakInfinity

<p align="center">
<a href="/readme_ko.md">  한국어  </a>
<a href="/readme.md">  English  </a>
</p>

[BreakInfinity.cs](https://github.com/Razenpok/BreakInfinity.cs)의 확장 포크입니다.

- 소수점 정밀도가 필요하지 않은 환경에서 double의 자릿수를 효율적으로 계산하여 CPU 시간을 크게 단축했습니다.
- 자릿수에 대한 효율적인 알파벳 변환 기능도 추가되었습니다.

<details>
<summary>BreakInfinity.cs보다 나은 핵심</summary>

- 범용 숫자 포맷터보다 방치형 게임의 반복 호출 구간에 맞춰 최적화했습니다.
- `FastDouble` 파서로 일반적인 저장/로드 경로에서 `double.Parse` 비용을 줄였습니다.
- 큰 수 표시를 좁은 게임용 표기 방식에 맞춰 처리해 `ToString` 비용을 낮췄습니다.
- 지수 범위를 알파벳 단위로 직접 변환하고, 반복 UI 갱신을 위해 결과를 캐싱합니다.
- 산술과 주요 변환 경로에서 불필요한 할당을 줄여 GC 부담을 낮게 유지합니다.
- 기존 `BigDouble` 사용성을 유지하면서 CPU 시간과 메모리 사용량에 집중했습니다.

</details>

## 왜 빠른가요?

아래 사진에 보이는 함수들은 한 프레임에 1000번씩 호출되었습니다.

<img width="820" alt="image" src="https://github.com/shlifedev/FastBigDouble/assets/49047211/3623a23a-961d-435a-a555-e6f618d227a3">

**double.Parse**와 **double.ToString**은 범용성이 뛰어나지만, 특수한 상황에서는 매우 느립니다.

방치형 게임처럼 큰 숫자를 다루는 게임에서는 부동소수점 정밀도에 크게 신경 쓸 필요가 없으므로, 자체 알고리즘으로 double을 파싱하면 성능상 이점이 있습니다.

그리고 ToString처럼 새로운 문자열을 생성해야 하는 경우가 아니라면, **GC가 거의 없는** **메모리 효율적인** 코드로 동작합니다.

## 중요 공지

`BigDouble.Parse("1.123456789")`와 `new BigDouble("1.123456789")`는 일반 소수의 소수점 아래 6자리를 남겨 `1.123456`을 반환합니다. `FastDouble.ParseDouble`도 일반 소수를 기본 6자리까지 읽으며, `maxDecimalPlaces`로 자릿수를 지정할 수 있습니다.

`e` 또는 `E`를 사용한 지수 표기에서는 두 파서 모두 가수 전체를 `double` 정밀도로 읽습니다. 예를 들어 `new BigDouble("1.123456789e100")`은 가수 `1.123456789`와 지수 `100`을 유지합니다. 게임 저장이나 네트워크 전송에는 `ToStringMantissaExponent()`의 결과를 그대로 쓰고, `BigDouble.Parse` 또는 문자열 생성자로 읽으세요. 저장한 가수와 64비트 부호 있는 지수를 복원할 수 있습니다.

--------

## 사용 방법

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

`BigDouble`은 `+`, `-`, `*`, `/` 같은 기본 사칙연산과 `>`, `<`, `>=`, `<=` 같은 비교 연산을 지원합니다.

`double`과 마찬가지로 `NaN == NaN`은 거짓이고 `NaN.Equals(NaN)`은 참이므로 NaN을 딕셔너리 키로 사용할 수 있습니다. `Equals(other, tolerance)`는 두 수의 차이를 절댓값이 더 큰 수에 상대적으로 비교합니다. 허용 오차는 0 이상이어야 하며 NaN은 항상 거짓, 무한대는 부호가 같을 때만 참입니다.

```cs
new BigDouble(999).ToString();    // "999"
new BigDouble(1000).ToString();   // "1.00A"
new BigDouble("1e6").ToString();  // "1.00B"
new BigDouble("1e81").ToString(); // "1.00AA"
```

알파벳 단위는 1000배 단위로 증가합니다.

```text
A = 1e3
B = 1e6
C = 1e9
...
Z = 1e78
AA = 1e81
AB = 1e84
```

저장/로드에는 지수 표기 형식을 권장합니다.

```cs
BigDouble value = new BigDouble("1.234e100");

string saved = value.ToStringMantissaExponent();
BigDouble loaded = new BigDouble(saved);
```

## 콘솔 샘플

.NET 10 SDK를 설치한 뒤 저장소 루트에서 방치형 게임을 실행합니다.

```sh
dotnet run --project src/dotnet/LD.Numeric/LD.GameSample
```

스페이스를 누르면 골드를 얻습니다. 숫자 1~4로 생산 시설을 구매하면 시간이 지날수록 골드가 쌓입니다. M으로 구매 수량(1개, 10개, 100개, 최대)을 전환하고 Q로 종료합니다. 파이프에서도 같은 키를 사용할 수 있고, 입력이 끝나면 게임이 종료됩니다.

`dotnet run --project src/dotnet/LD.Numeric/Example`을 실행하면 한 줄씩 명령을 입력하는 방치형 게임과 `BigDouble` API 데모 중 하나를 선택할 수 있습니다.

-----

많은 최적화가 이루어졌습니다.
큰 숫자를 자주 사용하는 환경에서 CPU 시간과 메모리 사용량을 최소화하고 싶다면, 이 라이브러리를 사용해 보세요.
