# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test Commands

| 작업 | 명령 |
| --- | --- |
| 전체 솔루션 빌드 | `dotnet build LD.Numeric.sln` |
| Release 빌드 | `dotnet build LD.Numeric.sln -c Release` |
| 테스트 실행 | `dotnet test LD.Numeric.Test/LD.Numeric.Test.csproj` |
| 특정 테스트 실행 | `dotnet test --filter "FullyQualifiedName~TestMethodName"` |

저장소 루트에서 Unity 런타임을 포함한 전체 소스의 포맷을 검사한다.

```bash
dotnet tool restore
dotnet csharpier check .
```

포맷 적용이 필요한 파일에는 `dotnet csharpier format <파일 경로>`를 사용한다.

## Architecture

LD.Numeric은 [BreakInfinity.cs](https://github.com/Razenpok/BreakInfinity.cs)의 성능 최적화 포크로, 아이들 게임에서 사용하는 큰 숫자를 효율적으로 처리한다.

### 소스 코드 구조

**중요:** 실제 소스 코드는 Unity 프로젝트에 위치하며 dotnet 프로젝트는 이를 참조한다.
- 소스 위치: `src/unity/Assets/LD.Numeric/Runtime/`
- csproj에서 `<Compile Include="../../../unity/Assets/LD.Numeric/Runtime/**/*.cs">` 로 참조

### 핵심 타입

**BigDouble** (`Type/BigDouble/`)
- `mantissa`(가수) + `exponent`(지수) 구조의 부동소수점 표현
- partial struct로 분리됨:
  - `BigDouble.cs`: 핵심 연산 (Add, Multiply, Pow, Log 등), 비교 연산자, 정적 상수
  - `BigDouble.Ctor.cs`: 문자열 생성자, eFormat enum
  - `BigDouble.Convert.cs`: 알파벳 단위 변환, AdjustedMantissa

**FastDouble** (`Optimizer/FastDouble.cs`)
- `double.Parse`/`ToString` 대체용 고성능 파서
- `ParseDouble(string, maxDecimalPlaces)`: 지정된 정밀도로 빠른 파싱
- `OptimizeToString()`: 스택 버퍼를 사용하는 저GC 문자열 변환

**AlphabetManager** (`Converters/Converter.NumberToAlphabet/`)
- 지수를 알파벳 단위로 변환 (1000=A, 1000000=B, ... AA, AB...)
- 3자리 지수마다 알파벳 증가 (exponent/3 - 1 = 알파벳 인덱스)
- 결과 캐싱으로 반복 변환 최적화

### 의존성

- `BenchmarkDotNet`: 런타임 성능 측정
- `NUnit`: 테스트 프레임워크

## Code Style

- CSharpier로 포맷팅 (printWidth: 100, indentSize: 4)
- PR 시 `dotnet csharpier check .` 자동 검사
