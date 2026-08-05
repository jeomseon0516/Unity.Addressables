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
   - 공식 로드 구현을 감싸는 중복 Callback 및 동기 wrapper를 제거했습니다.
   - 동적 Key와 직렬화 `AssetReference` 입력은 동일한 Service 소유권 모델로 통합했습니다.
   - Unity 6000.3 기준 Addressables 의존성을 2.9.1로 갱신했습니다.
7. **P2-02 — 진단과 관찰 가능성**
   - 활성 lease·instance 수와 해제되지 않은 소유권을 Inspector에서 확인합니다.
   - Resource 종류, Key, 타입, 생성 시각과 선택적 StackTrace를 제공합니다.
   - Service 종료 시 호출부가 먼저 해제하지 않은 Resource를 진단할 수 있습니다.
8. **P2-03 — 직렬화 Reference와 Provider 샘플**
   - 타입 제한 AssetReference, ScriptableObject Asset Configuration과 다중 Instance Provider를 제공합니다.
9. **P2-04 — API·취소·Catalog 검증**
   - 실제 에셋 로드, Prefab 생성·외부 Destroy, 취소와 Catalog 초기화 경로를 PlayMode에서 검증합니다.

## 추후 검토

구체적인 프로젝트 요구가 생길 때 다음 기능을 현재 Lease 소유권 모델에 맞춰 검토합니다.

- Scene Lease 기반 Addressable Scene 로드
- 다운로드 크기 조회와 의존성 선다운로드
- 명시적인 Runtime Catalog 갱신
- 진행률 보고와 Timeout 정책
- 전역 Cache를 만들지 않는 Resource Location 조회
