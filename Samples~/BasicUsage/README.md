# Addressables 기본 예제

## 준비

1. Package Manager에서 `Basic Usage` 샘플을 Import합니다.
2. `Tool/Addressables/Setup Basic Usage Sample`을 실행합니다.
3. `Scenes/AddressablesBasicUsage` Scene을 엽니다.
4. Play Mode에 진입합니다.

Setup 메뉴는 샘플 Prefab과 TextAsset을 `Jeomseon Addressables Basic Usage` 그룹에 다음
고정 주소로 등록합니다.

- `jeomseon-addressables-sample-prefab`
- `jeomseon-addressables-sample-message`

## Scene에서 확인할 기능

- `Instantiate Prefab`: `AddressablesHost.Service`를 통해 Prefab을 생성합니다.
- `Release Instance`: `AddressableInstanceHandle.Dispose`로 정상 해제합니다.
- `Destroy Externally`: 일반 `Destroy` 후 Observer가 operation을 자동 해제하는지 확인합니다.
- `Load TextAsset Lease`: 일반 Asset을 `AddressableAssetLease<TextAsset>`로 로드합니다.
- `Dispose TextAsset Lease`: 명시적으로 Asset handle을 해제합니다.
- `Active resources`: Service가 현재 소유한 lease와 instance 수를 표시합니다.

## 호출 구조

```csharp
AddressableInstanceHandle instance = await host.Service.InstantiateAsync(
    prefabKey,
    parent,
    destroyCancellationToken);

using AddressableAssetLease<TextAsset> message =
    await host.Service.LoadAssetAsync<TextAsset>(messageKey, destroyCancellationToken);
```

샘플 호출부는 정적 Manager나 `AsyncOperationHandle`에 직접 의존하지 않습니다. 생성 입력은
Addressables key이며 실제 수명과 해제 책임은 반환된 Handle 또는 Lease가 표현합니다.
