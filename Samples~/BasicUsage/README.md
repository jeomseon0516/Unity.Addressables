# Addressables 기본 예제

1. `AddressablesConfiguration` 에셋을 생성합니다.
2. Root GameObject에 `AddressablesHost`를 추가하고 Configuration을 연결합니다.
3. `AddressablesSample`에 Host와 Addressable Prefab key를 연결합니다.
4. Play Mode에서 Context Menu의 생성·해제 항목을 실행합니다.

샘플 호출부는 정적 Manager를 사용하지 않고 Host가 제공하는 `IAddressablesService`에
의존합니다. `AddressableInstanceHandle.Dispose`로 정상 해제할 수 있고 기본 정책에서는
외부 `Destroy`도 handle 누수 없이 처리됩니다.
