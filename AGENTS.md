# AGENTS.md

이 저장소에서 작업하는 에이전트는 아래 지침을 따른다.

## 기본 원칙

- 사용자에게 답변할 때는 한국어를 사용한다.
- 변경 전에는 관련 파일을 먼저 읽고, 기존 구조와 스타일을 따른다.
- 사용자가 만들거나 수정한 변경사항을 되돌리지 않는다.
- 불필요한 리팩터링, 포맷 변경, 생성물 갱신은 요청 범위 밖이면 하지 않는다.

## 프로젝트 구조

- `src/unity/`: Unity 프로젝트.
- `src/unity/Assets/LD.Numeric/Runtime/`: 실제 런타임 소스 코드 위치.
- `src/unity/Assets/LD.Numeric/Samples/`: Unity 샘플 코드와 씬.
- `src/dotnet/LD.Numeric/`: .NET 솔루션, 테스트, 벤치마크, 예제.
- `src/dotnet/LD.Numeric/LD.Numeric/LD.Numeric.csproj`는 Unity Runtime 소스를 링크로 참조한다.
  따라서 라이브러리 코드를 수정할 때는 보통 `src/unity/Assets/LD.Numeric/Runtime/` 아래 파일을 수정한다.

## 핵심 타입

- `BigDouble`: 큰 수 표현과 연산을 담당하는 partial struct.
  - `Type/BigDouble/BigDouble.cs`: 핵심 연산, 비교 연산자, 정적 상수.
  - `Type/BigDouble/BigDouble.Ctor.cs`: 생성자와 문자열 파싱 관련 코드.
  - `Type/BigDouble/BigDouble.Convert.cs`: 단위 변환과 표시 보조 로직.
- `FastDouble`: `double.Parse`와 `ToString` 대체를 목표로 하는 고성능 파서/포매터.
- `AlphabetManager`, `AlphabetConverter`: 1000 단위 지수를 알파벳 단위로 변환하는 로직.
- `NumberUtility`: 숫자 관련 유틸리티.

## 빌드와 테스트

명령은 특별한 이유가 없으면 `src/dotnet/LD.Numeric/`에서 실행한다.

```bash
dotnet build LD.Numeric.sln
dotnet test LD.Numeric.Test/LD.Numeric.Test.csproj
dotnet test --filter "FullyQualifiedName~TestMethodName"
dotnet tool restore
dotnet csharpier check .
dotnet csharpier .
```

- 일반 변경 후에는 최소한 관련 테스트를 실행한다.
- 공유 숫자 연산, 파싱, 포매팅, 비교, 변환 로직을 건드렸다면 전체 테스트를 우선 고려한다.
- 포맷 검사는 `dotnet csharpier check .`를 사용한다.
- 포맷 적용이 필요할 때만 `dotnet csharpier .`를 실행한다.

## 코드 스타일

- C# 포맷은 CSharpier 설정을 따른다.
  - `printWidth`: 100
  - `indentSize`: 4
  - `useTabs`: false
- nullable 경고를 새로 만들지 않는다.
- 테스트는 NUnit을 사용한다.
- 새 테스트는 기존 테스트 파일의 네이밍과 배열 방식을 따른다.
- 성능 최적화 코드에서는 할당과 GC 영향을 명시적으로 고려한다.
- 문자열 파싱/포매팅 경로를 수정할 때는 경계값, 지수 표기, 음수, 0, NaN/Infinity 동작을 확인한다.

## Unity 관련 주의사항

- Unity 패키지 메타 파일(`*.meta`)은 필요한 경우에만 수정한다.
- 런타임 코드와 .NET 테스트 코드의 동작이 어긋나지 않도록 유지한다.
- Unity 패키지 정보는 `src/unity/Assets/LD.Numeric/package.json`에 있다.
- Unity 에디터 버전은 `src/unity/ProjectSettings/ProjectVersion.txt`를 기준으로 확인한다.

## 성능 관련 주의사항

- 이 프로젝트는 방치형 게임의 큰 수 처리 성능을 중시한다.
- `FastDouble.cs`는 소수점 6자리 정밀도 제한을 전제로 한다.
- 파싱, 포매팅, 알파벳 단위 변환 변경 시 기존 벤치마크와 README의 성능 의도를 훼손하지 않는다.
- 벤치마크 결과물(`BenchmarkDotNet.Artifacts/`)은 요청받지 않는 한 수정하지 않는다.

## 문서

- 영어 README는 `readme.md`, 한국어 README는 `readme_ko.md`다.
- 동작 변경이 사용자 API나 주의사항에 영향을 주면 README 갱신 필요성을 검토한다.
- 기존 `src/dotnet/LD.Numeric/CLAUDE.md`의 빌드/구조 정보와 충돌하지 않게 유지한다.
