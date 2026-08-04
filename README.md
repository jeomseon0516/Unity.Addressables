# Jeomseon Unity Addressables

Unity Addressables 2.9.1 위에서 직렬화 설정, 명시적인 에셋 소유권과 Prefab 인스턴스
수명 관리를 제공합니다.

## 요구 사항

- Unity 6000.3.15f1 이상
- `com.unity.addressables` 2.9.1 이상

## 구성

1. `Tool/Addressables/Configuration`에서 `AddressablesConfiguration`을 생성합니다.
2. Scene의 Root GameObject에 `AddressablesHost`를 추가합니다.
3. Configuration을 Host에 연결합니다.
4. 일반 호출부에는 `IAddressablesService`를 전달합니다.

Configuration은 설정만 보관하며 runtime handle을 소유하지 않습니다. Host가 Service를
생성하고 파괴될 때 Service가 소유한 모든 lease와 instance를 해제합니다.

## 일반 에셋

Material, Sprite, Texture, AudioClip, ScriptableObject 등은 lease를 명시적으로 소유합니다.

```csharp
using AddressableAssetLease<Material> lease =
    await service.LoadAssetAsync<Material>("EnemyMaterial", cancellationToken);

renderer.sharedMaterial = lease.Asset;
```

Label의 여러 에셋은 collection lease 하나로 관리합니다.

```csharp
using AddressableAssetCollectionLease<Sprite> lease =
    await service.LoadAssetsAsync<Sprite>("UIIcons", cancellationToken: cancellationToken);
```

## Prefab 인스턴스

```csharp
using AddressableInstanceHandle handle =
    await service.InstantiateAsync("EnemyPrefab", parent, cancellationToken);

Enemy enemy = handle.GetComponent<Enemy>();
```

`Explicit` 정책은 `Dispose`를 요구합니다. 기본 `ReleaseOnDestroy` 정책은 외부 코드가
`Destroy`를 호출한 경우에도 실제 operation handle을 정확히 한 번 해제합니다. 정상
`Dispose`는 Unity의 `Addressables.ReleaseInstance`를 사용합니다.

## 공식 Addressables와의 경계

로드, 생성, Label 조회, 참조 카운팅, Catalog 갱신과 Bundle cache 처리는 Unity 공식 API를
사용합니다. 이 패키지는 해당 API를 다시 구현하지 않고 Configuration, Service 소유권,
lease와 외부 Destroy 안전성만 제공합니다. GameObjectPooling과의 통합은 이 패키지에
포함하지 않습니다.

## Basic Usage 샘플

Package Manager에서 샘플을 Import한 뒤 `Tool/Addressables/Setup Basic Usage Sample`을
실행하고 `Scenes/AddressablesBasicUsage` Scene을 엽니다. Scene UI에서 Prefab 정상 해제,
외부 Destroy 자동 해제, TextAsset lease와 활성 Resource 수를 직접 확인할 수 있습니다.
