# Jeomseon Unity Addressables

Unity Addressables 위에 직렬화 설정, 명시적인 에셋 소유권과 Prefab 인스턴스 수명 관리를
제공합니다. Unity의 로드·참조 카운팅 기능을 다시 구현하지 않고, 호출부가
`AsyncOperationHandle`을 직접 관리하지 않도록 `IAddressablesService`, Lease와 Instance
Handle로 소유권 경계를 표현합니다.

## 요구 사항

- Unity 6000.5.7f1 이상
- `com.unity.addressables` 2.9.1 이상
- 패키지 ID: `com.jeomseon.unity.addressables`

`package.json`이 Unity Addressables 2.9.1을 의존성으로 선언하므로 UPM 설치 시 함께
해결됩니다. Git URL로 설치할 때는 Unity Package Manager의
`Install package from git URL`에 다음 주소를 사용합니다.

```text
https://github.com/jeomseon0516/Unity.Addressables.git#v0.3.1
```

로컬 개발 프로젝트에서는 `Packages/manifest.json`에 저장소 경로를 연결할 수 있습니다.

```json
{
  "dependencies": {
    "com.jeomseon.unity.addressables": "file:../../Jeomseon.Unity.Addressables"
  },
  "testables": [
    "com.jeomseon.unity.addressables"
  ]
}
```

## 제공 범위

- `AddressablesConfiguration`: 직렬화 가능한 초기화·Prefab 해제 정책
- `AddressablesHost`: Scene 또는 Application 수명으로 Service를 소유하는 Component
- `IAddressablesService`: 호출부와 DI가 의존할 런타임 계약
- `AddressablesService`: Unity Addressables 기반 기본 구현
- `AddressableAssetLease<T>`: 단일 일반 에셋 operation의 소유권
- `AddressableAssetCollectionLease<T>`: Label 또는 Key로 로드한 에셋 묶음의 소유권
- `AddressableInstanceHandle`: 생성된 Prefab과 instance operation의 소유권
- 문자열·동적 Key와 타입 제한 `AssetReference` 입력을 모두 지원
- 초기 Catalog 갱신과 선택적인 미사용 Bundle cache 정리
- 외부 `Destroy` 시 Prefab operation 자동 해제 정책
- Unity `Awaitable` 및 `CancellationToken` 기반 비동기 API

Scene 로드, 다운로드 크기 조회, 의존성 선다운로드, Catalog를 임의 시점에 반복 갱신하는 관리
API는 현재 공개 범위에 포함하지 않습니다. 필요한 경우 Unity 공식 Addressables API를 직접
사용하거나 차후 별도 기능으로 확장합니다.

## 기본 구성

### 1. Configuration 생성

Unity 메뉴에서 다음 자산을 생성합니다.

```text
Assets/Create/Tool/Addressables/Configuration
```

`AddressablesConfiguration`은 다음 정책을 직렬화합니다.

| 설정 | 의미 |
| --- | --- |
| `Instance Release Policy` | Prefab 인스턴스의 명시적·자동 해제 정책 |
| `Update Catalog On Initialize` | Service 최초 초기화 전에 원격 Catalog 갱신 확인 및 적용 |
| `Clean Bundle Cache After Catalog Update` | Catalog 갱신 후 더 이상 참조되지 않는 Bundle cache 정리 |
| `Log Outstanding Resources On Dispose` | Service 종료 시 호출부가 먼저 해제하지 않은 Resource 경고 |
| `Capture Allocation Stack Trace` | 활성 Resource가 생성된 StackTrace 기록 |

Configuration은 설정만 보관하며 runtime handle이나 로드된 에셋을 소유하지 않습니다.
여러 Host 또는 Service가 같은 Configuration을 공유할 수 있습니다.

### 2. Host 배치

Scene의 GameObject에 `AddressablesHost`를 추가하고 Configuration을 연결합니다.

| 설정 | 의미 |
| --- | --- |
| `Configuration` | Service 생성에 사용할 정책 자산 |
| `Initialize On Start` | `Start`에서 Service를 미리 초기화 |
| `Dont Destroy On Load` | Host와 Service를 Scene 전환 후에도 유지 |

`Dont Destroy On Load`를 활성화한 Host는 반드시 Root GameObject에 있어야 합니다. 자식에
배치하면 실행 시 구성 오류가 발생합니다.

```text
Application Services
└── AddressablesHost
```

Host는 Service를 지연 생성합니다.

```csharp
IAddressablesService service = host.Service;
```

Host가 파괴되면 Service를 Dispose하고, Service가 아직 소유한 모든 Lease와 InstanceHandle을
해제합니다. Scene 전용 Host는 Scene 수명, `Dont Destroy On Load` Host는 Application 수명에
적합합니다.

## 입력 방식 선택

### 동적 Key

서버 응답, 런타임 테이블 또는 DLC처럼 주소가 실행 중 결정되면 `object key` API를 사용합니다.

```csharp
AddressableAssetLease<Material> lease =
    await service.LoadAssetAsync<Material>(
        serverResponse.MaterialAddress,
        cancellationToken);
```

문자열 외에도 Unity Addressables가 Key로 해석할 수 있는 객체를 전달할 수 있습니다. `null`과
빈 문자열은 호출 전에 거부됩니다.

### 직렬화 AssetReference

프로젝트에 미리 존재하는 정적 에셋은 주소 문자열보다 Inspector에 직렬화한 타입 제한
Reference를 권장합니다.

```csharp
using UnityEngine.AddressableAssets;

[SerializeField] private AssetReferenceTexture2D _texture;

AddressableAssetLease<Texture2D> lease =
    await service.LoadAssetReferenceAsync<Texture2D>(
        _texture,
        cancellationToken);
```

Unity는 `AssetReferenceGameObject`, `AssetReferenceTexture`, `AssetReferenceTexture2D`,
`AssetReferenceTexture3D`, `AssetReferenceSprite` 등을 제공합니다. Material, AudioClip,
TextAsset 또는 프로젝트 ScriptableObject처럼 기본 구체 Reference가 없는 타입은 작은 파생
타입을 정의합니다.

```csharp
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public sealed class MaterialReference : AssetReferenceT<Material>
{
    public MaterialReference(string guid) : base(guid)
    {
    }
}
```

```csharp
[SerializeField] private MaterialReference _material;

AddressableAssetLease<Material> lease =
    await service.LoadAssetReferenceAsync<Material>(
        _material,
        cancellationToken);
```

타입 제한 Reference API를 호출하는 asmdef는 `Unity.Addressables`를 직접 참조해야 합니다.
동적 Key API만 사용하는 소비 asmdef에는 이 추가 참조가 필요하지 않습니다.

## 일반 에셋 하나 로드

Material, Sprite, Texture, AudioClip, TextAsset, ScriptableObject 등은
`AddressableAssetLease<T>`로 소유합니다.

```csharp
private AddressableAssetLease<Material> _materialLease;

private async Awaitable LoadMaterialAsync(CancellationToken cancellationToken)
{
    _materialLease?.Dispose();
    _materialLease = await service.LoadAssetReferenceAsync<Material>(
        _material,
        cancellationToken);

    renderer.sharedMaterial = _materialLease.Asset;
}

private void ReleaseMaterial()
{
    renderer.sharedMaterial = null;
    _materialLease?.Dispose();
    _materialLease = null;
}
```

`Asset`을 사용하는 동안 Lease를 유지해야 합니다. 메서드가 끝난 뒤에도 Renderer, UI 또는
게임 시스템이 에셋을 사용한다면 Lease를 필드나 Provider에 보관합니다.

다음 형태는 에셋 사용이 해당 블록 안에서 완전히 끝날 때만 사용합니다.

```csharp
using AddressableAssetLease<TextAsset> lease =
    await service.LoadAssetAsync<TextAsset>("legal-notice", cancellationToken);

ParseImmediately(lease.Asset.text);
```

Lease의 `Dispose`는 operation을 정확히 한 번 해제합니다. 중복 Dispose는 안전하지만,
Dispose 이후 `IsValid`는 `false`이며 해당 Lease가 에셋 수명을 더 이상 보장하지 않습니다.

## Label 또는 Key로 여러 에셋 로드

여러 에셋은 `AddressableAssetCollectionLease<T>` 하나로 함께 소유합니다.

```csharp
using UnityEngine.AddressableAssets;

[SerializeField] private AssetLabelReference _uiIcons;

private AddressableAssetCollectionLease<Sprite> _iconsLease;

private async Awaitable LoadIconsAsync(CancellationToken cancellationToken)
{
    _iconsLease = await service.LoadLabelAssetsAsync<Sprite>(
        _uiIcons,
        releaseDependenciesOnFailure: true,
        cancellationToken: cancellationToken);

    foreach (Sprite icon in _iconsLease.Assets)
    {
        Debug.Log(icon.name);
    }
}
```

동적으로 결정된 Label 또는 Key에는 `LoadAssetsAsync<T>(object key, ...)`를 사용합니다.

```csharp
AddressableAssetCollectionLease<Sprite> lease =
    await service.LoadAssetsAsync<Sprite>(
        runtimeLabel,
        releaseDependenciesOnFailure: true,
        cancellationToken: cancellationToken);
```

Collection Lease를 Dispose하면 컬렉션 operation과 그 operation이 소유한 참조를 함께
해제합니다. 개별 `Assets` 항목마다 별도로 Release하지 않습니다.

## Prefab 인스턴스 생성

동적 Key로 생성합니다.

```csharp
AddressableInstanceHandle handle = await service.InstantiateAsync(
    "enemy-prefab",
    parent,
    cancellationToken);
```

Inspector Reference로 생성합니다.

```csharp
[SerializeField] private AssetReferenceGameObject _enemyPrefab;

AddressableInstanceHandle handle =
    await service.InstantiateReferenceAsync(
        _enemyPrefab,
        parent,
        cancellationToken);
```

생성된 GameObject와 Component는 Handle에서 가져옵니다.

```csharp
GameObject instance = handle.Instance;
Enemy enemy = handle.GetComponent<Enemy>();
```

정상적인 제거는 `Destroy`가 아니라 Handle의 Dispose입니다.

```csharp
handle.Dispose();
handle = null;
```

Dispose는 내부적으로 `Addressables.ReleaseInstance`를 호출하여 GameObject 파괴와 Addressables
operation 해제를 함께 처리합니다.

### Prefab 해제 정책

`AddressableInstanceReleasePolicy.Explicit`은 호출부가 반드시 InstanceHandle을 Dispose해야
합니다. 외부에서 GameObject만 `Destroy`하면 operation은 자동 해제되지 않습니다.

기본값인 `ReleaseOnDestroy`는 인스턴스에 내부 Observer를 추가합니다. 정상 Dispose뿐 아니라
외부 시스템이나 Scene 종료가 GameObject를 먼저 파괴해도 Observer가 남은 operation을 정확히
한 번 해제합니다. 그래도 일반 호출부에서는 소유권이 명확한 `Dispose`를 우선 사용합니다.

## 초기화와 Catalog 갱신

`Initialize On Start`가 활성화된 Host는 `Start`에서 초기화합니다. 직접 초기화할 수도 있습니다.

```csharp
await service.InitializeAsync(cancellationToken);
```

명시적으로 호출하지 않아도 모든 Load 및 Instantiate API가 최초 요청 전에 초기화를 보장합니다.
초기화는 중복 호출을 하나의 작업으로 합치며 성공 후 `IsInitialized`가 `true`가 됩니다.

Configuration의 `Update Catalog On Initialize`가 활성화되면 첫 에셋 로드 전에 다음 순서로
처리합니다.

1. 원격 Catalog 갱신 확인
2. 발견된 Catalog 적용
3. 설정에 따라 더 이상 참조되지 않는 Bundle cache 정리
4. Service 초기화 완료

Catalog 갱신은 Service가 아직 리소스를 소유하지 않은 초기 단계에서만 수행됩니다. 실행 중
임의 시점의 Catalog 갱신 API는 현재 공개하지 않습니다.

## 취소와 예외 처리

MonoBehaviour에서는 `destroyCancellationToken`을 전달하는 것이 가장 간단합니다.

```csharp
try
{
    _textureLease = await service.LoadAssetReferenceAsync<Texture2D>(
        _texture,
        destroyCancellationToken);
}
catch (OperationCanceledException)
{
    // 소유자가 파괴되어 결과가 더 이상 필요하지 않습니다.
}
catch (Exception exception)
{
    Debug.LogException(exception, this);
}
```

CancellationToken은 Unity Addressables의 진행 중인 다운로드를 강제로 중단하지 않습니다.
operation 완료 뒤 취소가 확인되면 패키지가 생성된 handle을 즉시 해제하고
`OperationCanceledException`을 전달합니다. 취소된 결과의 Addressables 참조가 Service에
남지 않도록 하는 정책입니다.

잘못된 Key, 유효하지 않은 Reference, 실패한 Addressables operation, Dispose된 Service 사용은
예외로 보고됩니다. `async void` 호출부에서는 반드시 예외를 처리합니다.

## Service 수명과 직접 생성

Inspector 구성이 필요 없으면 Host 없이 Service를 직접 만들 수 있습니다.

```csharp
private IAddressablesService _service;

private async Awaitable InitializeAsync()
{
    _service = new AddressablesService(configuration);
    await _service.InitializeAsync(destroyCancellationToken);
}

private void OnDestroy()
{
    _service?.Dispose();
    _service = null;
}
```

Configuration을 생략하면 다음 기본값을 사용합니다.

- Prefab 해제: `ReleaseOnDestroy`
- 초기 Catalog 갱신: 비활성
- Catalog 갱신 시 미사용 Bundle cache 정리: 활성

직접 생성한 Service는 생성한 Composition Root 또는 DI Container가 Dispose해야 합니다.
Service를 Dispose하면 현재 등록된 모든 Asset Lease, Collection Lease와 InstanceHandle을
해제하며 이후 재사용할 수 없습니다.

## Provider를 통한 권장 사용

작은 프로토타입은 Host와 Lease를 직접 사용해도 되지만, 완성된 게임 로직에는 도메인별
Provider를 두는 것을 권장합니다.

```text
AddressablesConfiguration
          ↓
    AddressablesHost
          ↓
  IAddressablesService
          ↓
도메인 Asset Configuration + Provider
          ↓
      게임 로직
```

Provider는 다음 책임을 가집니다.

- Addressables Key 또는 AssetReference 선택
- Lease와 InstanceHandle 보관 및 Dispose
- 도메인 타입으로 결과 변환
- 재로드·교체 시 기존 리소스 해제

게임 로직은 Addressables를 모르는 인터페이스에만 의존합니다.

```csharp
public interface IEnemyContentProvider
{
    Awaitable<Enemy> InstantiateAsync(
        Transform parent,
        CancellationToken cancellationToken);

    void Release();
}
```

`Basic Usage` 샘플의 `AddressablesSampleAssets`는 Prefab과 TextAsset Reference를 하나의
ScriptableObject에 직렬화하고, `AddressablesSampleContentProvider`가 실제 Lease와
InstanceHandle을 내부에서 소유하는 예제를 제공합니다.

## 진단

Service가 현재 소유한 리소스 수를 확인할 수 있습니다.

```csharp
Debug.Log(service.ActiveResourceCount);
```

각 Resource의 종류, Key, 타입, 생성 시각과 선택적 할당 StackTrace도 Snapshot으로 확인할 수
있습니다.

```csharp
foreach (AddressableResourceInfo info in service.ActiveResources)
{
    Debug.Log($"{info.Kind}: {info.Key}, {info.ResourceType.Name}");
}
```

다음 객체가 생성되면 증가하고 Dispose되면 감소합니다.

- `AddressableAssetLease<T>`
- `AddressableAssetCollectionLease<T>`
- `AddressableInstanceHandle`

Scene 또는 기능 종료 후 값이 예상보다 크면 Lease나 Handle의 소유자가 Dispose를 누락했는지
확인합니다. Host 종료 시 Service가 남은 항목을 일괄 정리하지만, 장시간 유지되는 Host에서는
개별 소유자가 사용 종료 시점에 즉시 해제해야 합니다.

Play Mode에서 `AddressablesHost` Inspector는 초기화 여부, 활성 Resource 수, 각 Resource의
Key·타입·경과 시간·할당 StackTrace를 표시합니다. `Capture Allocation Stack Trace`는 진단
비용이 있으므로 문제를 추적할 때만 활성화하는 것을 권장합니다. 영속 Host가 Root가 아니면
Inspector에서도 즉시 오류를 표시합니다.

## 흔한 실수

- 로드된 에셋을 계속 사용하면서 지역 `using`으로 Lease를 즉시 Dispose
- Prefab InstanceHandle을 보관하지 않고 GameObject만 전달
- `Explicit` 정책에서 GameObject만 `Destroy`
- 같은 에셋을 반복 로드하면서 이전 Lease를 먼저 해제하지 않음
- Application 수명 Host를 자식 GameObject에 배치
- 타입 제한 Reference 사용 asmdef에서 `Unity.Addressables` 참조 누락
- Service를 직접 생성하고 Dispose하지 않음
- `async void`에서 취소와 로드 실패 예외를 처리하지 않음

## 공식 Addressables와의 경계

에셋 로드, Prefab 생성, Label 조회, 참조 카운팅, Catalog 갱신과 Bundle cache 처리는 Unity
공식 Addressables API를 사용합니다. 이 패키지는 Configuration, Service 소유권, Lease와
외부 Destroy 안전성만 추가합니다.

## Basic Usage 샘플

1. Package Manager에서 `Basic Usage` 샘플을 Import합니다.
2. `Tool/Addressables/Setup Basic Usage Sample`을 실행합니다.
3. `Scenes/AddressablesBasicUsage` Scene을 엽니다.
4. Play Mode에 진입합니다.

샘플에서 다음 동작을 확인할 수 있습니다.

- 직렬화된 Prefab Reference로 생성
- InstanceHandle을 통한 정상 해제
- 외부 `Destroy` 시 operation 자동 해제
- 직렬화된 TextAsset Reference와 Lease
- ScriptableObject Asset Configuration
- Lease를 내부 소유하는 도메인 Provider
- Service의 활성 리소스 수
- 여러 Prefab Instance를 동시에 소유하고 개별·전체 해제하는 Provider
- AddressablesHost Inspector의 Runtime Resource 진단

## API 선택표

| 입력과 대상 | API | 반환 소유권 |
| --- | --- | --- |
| 동적 Key의 단일 에셋 | `LoadAssetAsync<T>` | `AddressableAssetLease<T>` |
| 직렬화 Reference의 단일 에셋 | `LoadAssetReferenceAsync<T>` | `AddressableAssetLease<T>` |
| 동적 Key·Label의 여러 에셋 | `LoadAssetsAsync<T>` | `AddressableAssetCollectionLease<T>` |
| 직렬화 Label의 여러 에셋 | `LoadLabelAssetsAsync<T>` | `AddressableAssetCollectionLease<T>` |
| 동적 Key의 Prefab | `InstantiateAsync` | `AddressableInstanceHandle` |
| 직렬화 Prefab Reference | `InstantiateReferenceAsync` | `AddressableInstanceHandle` |

## 테스트

패키지를 `testables`에 등록한 뒤 Unity Test Runner의 PlayMode에서
`Jeomseon.Unity.Addressables.PlayModeTests`를 실행합니다.

## 라이선스

[MIT License](./LICENSE.md)
