# Addressables 로드맵

## 완료

1. **P0-01 — 정적 전역 상태 제거**
   - 정적 Manager, handle dictionary와 수동 참조 카운터를 제거했습니다.
   - Service가 lease와 instance handle을 소유하고 Dispose에서 일괄 해제합니다.
2. **P0-02 — 공식 Catalog 갱신 경로 적용**
   - `ClearResourceLocators` 기반 갱신을 제거했습니다.
   - 리소스 로드 전에 `CheckForCatalogUpdates`와 `UpdateCatalogs`를 사용합니다.
3. **P1-01 — Configuration·Service·Host 분리**
   - ScriptableObject는 정책만 보관하고 Host가 런타임 Service를 소유합니다.
4. **P1-02 — 일반 에셋 수명 모델**
   - 단일·복수 에셋에 명시적인 `IDisposable` lease를 제공합니다.
5. **P1-03 — Prefab 인스턴스 수명 모델**
   - Unity `InstantiateAsync`와 `ReleaseInstance`를 사용합니다.
   - 외부 `Destroy` 자동 해제를 선택 정책으로 제공합니다.
6. **P2-01 — 공식 API 중복 제거**
   - Label, AssetReference, Callback 및 동기 wrapper를 제거했습니다.
   - Unity 6000.3 기준 Addressables 의존성을 2.9.1로 갱신했습니다.

## 다음 작업

1. **P2-02 — 진단과 관찰 가능성**
   - 활성 lease·instance 수와 해제되지 않은 소유권을 Inspector에서 확인합니다.
2. **P3-01 — 프로젝트 전체 리팩터링 이후 통합 패키지 검토**
   - Addressables와 GameObjectPooling 핵심 패키지는 서로를 참조하지 않습니다.
   - 모든 패키지 계약이 안정된 뒤 별도 통합 패키지를 검토합니다.
