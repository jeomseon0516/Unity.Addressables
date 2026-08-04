# 변경 기록

## [0.1.2] - 2026-07-29

- asmdef의 `rootNamespace`와 소스 파일 위치를 공개 namespace에 맞게 정리했습니다.

## [0.1.1] - 2026-07-29

- Addressable 프리팹 복제를 확인하는 `Basic Usage` 샘플을 추가했습니다.

## [Unreleased]

- Unity Addressables 최소 의존성을 2.9.1로 변경했습니다.
- 정적 `PrototypeManager`와 수동 handle cache를 제거했습니다.
- ScriptableObject Configuration, runtime Service와 MonoBehaviour Host를 추가했습니다.
- 일반 에셋용 단일·복수 lease와 Prefab instance handle을 추가했습니다.
- Unity 기본 Label·Callback·동기 wrapper와 `ClearResourceLocators` 갱신을 제거했습니다.
- 외부 `Destroy` 시 Prefab operation을 한 번만 해제하는 선택 정책을 추가했습니다.
- 활성 lease와 instance 수를 확인하는 최소 진단 API를 추가했습니다.

## [0.1.0] - 2026-07-29

- JeomseonScriptPack의 관련 모듈을 독립 UPM 패키지로 분리했습니다.
