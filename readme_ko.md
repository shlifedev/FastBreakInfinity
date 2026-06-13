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

`FastDouble.cs`는 소수점 6자리까지만 정밀도를 처리하므로, 서버로 값을 전송하거나 유사한 작업을 할 때는 반드시 6자리를 초과하는 소수점을 버림처리하세요.
ex ) 1.123456789e10 => 1.123456e10

--------

## 사용 방법

BigInfinity.cs와 사용법이 완전히 동일하지만, 다음 규칙을 따라야 합니다.

간단합니다.
```cs
BigDouble _ = new BigDouble("1000000000000000000000"); // 숫자 생성자
BigDouble _ = new BigDouble("9.999e100"); // 지수 생성자. 매우 빠릅니다!
new BigDouble(1e3).ToString() // 결과 = "1.0A"
```

-----

많은 최적화가 이루어졌습니다.
큰 숫자를 자주 사용하는 환경에서 CPU 시간과 메모리 사용량을 최소화하고 싶다면, 이 라이브러리를 사용해 보세요.
