# 변경 기록

## [0.3.1] - 2026-09-01

- Unity 최소 지원 버전을 `6000.5.7f1` → `6000.6.0f1`로 상향하고 한·영 README 요구 버전 문구를
  동기화했습니다. 코드·API 변경은 없습니다.
- 직렬화 필드 리네이밍 전 이름을 사용해 진단·Catalog 정책을 실제로 활성화하지 못하던 PlayMode
  테스트 JSON을 현재 필드명으로 수정하고 설정 적용 assertion을 추가했습니다.
- `Basic Usage` 샘플의 `AddressablesSampleSetup` 메뉴 경로를 `AGENTS.md` 규칙(루트 `Jeomseon/`)에
  맞춰 `Tool/Addressables/Setup Basic Usage Sample` → `Jeomseon/Addressables/Setup Basic Usage
  Sample`로 수정했습니다. 이 Setup 메뉴는 Import된 Prefab/TextAsset을 **소비 프로젝트 자신의**
  `AddressableAssetSettings`에 등록하는 단계라(프로젝트마다 로컬 GUID가 달라 패키지에 미리 구워
  넣을 수 없음) 그대로 유지합니다 — Shaders/Dispatcher 샘플과 달리 이 단계 자체를 없앨 수는
  없습니다.

## [0.3.0] - 2026-08-13

- **(Breaking)** Runtime/Editor 네임스페이스를 패키지 및 경로 규칙에 맞춰
  `Jeomseon.Unity.Addressables`와 `Jeomseon.Unity.Addressables.Editor.Diagnostics`로 변경했습니다.
  이전 네임스페이스 호환 별칭은 제공하지 않습니다.

## [0.2.4] - 2026-08-11

- 워크스페이스 명명 규칙에 맞춰 `[SerializeField] private` 필드를 `_camelCase`에서 `camelCase`로
  정리하고 기존 이름을 `[FormerlySerializedAs]`로 보존했습니다. `AddressablesHostEditor`의
  `FindProperty` 문자열도 함께 갱신했습니다. 공개 C# API 변경은 없으며 기존 Scene·Prefab의
  직렬화된 값은 그대로 유지됩니다.

## [0.1.2] - 2026-07-29

- asmdef의 `rootNamespace`와 소스 파일 위치를 공개 namespace에 맞게 정리했습니다.

## [0.1.1] - 2026-07-29

- Addressable 프리팹 복제를 확인하는 `Basic Usage` 샘플을 추가했습니다.

## [0.2.2] - 2026-08-05

- Unity Addressables 최소 의존성을 2.9.1로 변경했습니다.
- 정적 `PrototypeManager`와 수동 handle cache를 제거했습니다.
- ScriptableObject Configuration, runtime Service와 MonoBehaviour Host를 추가했습니다.
- 일반 에셋용 단일·복수 lease와 Prefab instance handle을 추가했습니다.
- Unity 기본 Label·Callback·동기 wrapper와 `ClearResourceLocators` 갱신을 제거했습니다.
- 외부 `Destroy` 시 Prefab operation을 한 번만 해제하는 선택 정책을 추가했습니다.
- 활성 lease와 instance 수를 확인하는 최소 진단 API를 추가했습니다.
- Prefab 정상 해제, 외부 Destroy 및 일반 Asset lease를 검증하는 Basic Usage Scene을 추가했습니다.
- 타입 제한 AssetReference와 AssetLabelReference 입력 API를 추가했습니다.
- 활성 Resource의 종류, Key, 타입, 생성 시각과 선택적 할당 StackTrace 진단을 추가했습니다.
- AddressablesHost Play Mode Inspector와 영속 Root 구성 경고를 추가했습니다.
- Service 종료 시 호출부가 해제하지 않은 Resource 경고 정책을 추가했습니다.
- ScriptableObject Asset Configuration과 다중 Prefab Provider 샘플을 추가했습니다.
- 실제 로드·생성·외부 Destroy·취소·Catalog 초기화 PlayMode 테스트를 확장했습니다.
- 향후 Scene Lease, 선다운로드, 진행률 및 Runtime Catalog API는 TODO로 명시했습니다.

## [0.1.0] - 2026-07-29

- JeomseonScriptPack의 관련 모듈을 독립 UPM 패키지로 분리했습니다.


## [0.2.3] - 2026-08-05

- Unity 6000.5.7f1을 최소 지원 버전으로 상향했습니다.
