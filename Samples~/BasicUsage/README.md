# Addressables 기본 예제

## 준비

1. Package Manager에서 `Basic Usage` 샘플을 Import합니다.
2. `Tool/Addressables/Setup Basic Usage Sample`을 실행합니다.
3. `Scenes/AddressablesBasicUsage` Scene을 엽니다.
4. Play Mode에 진입합니다.

Setup 메뉴는 샘플 Prefab과 TextAsset을 `Jeomseon Addressables Basic Usage` 그룹에 다음
주소로 등록합니다. Runtime 샘플은 이 문자열을 직접 사용하지 않고 Scene에 직렬화된
`AssetReferenceGameObject`와 `TextAssetReference`를 Service에 전달합니다.

- `jeomseon-addressables-sample-prefab`
- `jeomseon-addressables-sample-message`

## Scene에서 확인할 기능

- `Instantiate Another Prefab`: Provider가 여러 Prefab Handle을 동시에 소유합니다.
- `Release Last Instance`: 마지막 Instance를 개별 해제합니다.
- `Release All Instances`: Provider가 소유한 Instance를 모두 해제합니다.
- `Destroy Last Externally`: 일반 `Destroy` 후 Observer가 operation을 자동 해제하는지 확인합니다.
- `Load TextAsset Lease`: 일반 Asset을 `AddressableAssetLease<TextAsset>`로 로드합니다.
- `Dispose TextAsset Lease`: 명시적으로 Asset handle을 해제합니다.
- `Active resources`: Service가 현재 소유한 lease와 instance 수를 표시합니다.

## 호출 구조

```csharp
AddressableInstanceHandle instance = await host.Service.InstantiateReferenceAsync(
    prefabReference,
    parent,
    destroyCancellationToken);

using AddressableAssetLease<TextAsset> message =
    await host.Service.LoadAssetReferenceAsync<TextAsset>(
        messageReference,
        destroyCancellationToken);
```

샘플 호출부는 정적 Manager나 `AsyncOperationHandle`에 직접 의존하지 않습니다. 생성 입력은
직렬화된 AssetReference이며 실제 수명과 해제 책임은 반환된 Handle 또는 Lease가 표현합니다.
서버 응답이나 동적 콘텐츠 주소에는 기존 `object key` 오버로드를 그대로 사용할 수 있습니다.

`AddressablesSampleAssets`는 Prefab과 TextAsset Reference를 하나의 ScriptableObject에
직렬화합니다. `AddressablesSampleContentProvider`는 이 Configuration과
`IAddressablesService`를 조합하고 Lease 및 InstanceHandle을 내부에서 소유합니다. 따라서
`AddressablesSample` 호출부는 Addressables key나 저수준 소유권 타입을 직접 다루지 않습니다.
Provider는 여러 Actor Handle을 참조 동일성으로 관리하고, 외부 Destroy로 이미 해제된 Handle을
정리하며, TextAsset Lease는 반복 요청에서 공유합니다.

Play Mode에서 `AddressablesHost`를 선택하면 Inspector의 Runtime Diagnostics에서 활성
Resource Key, 타입, 경과 시간과 할당 StackTrace를 확인할 수 있습니다.
