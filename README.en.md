# Jeomseon Unity Addressables

Provides serialized policy, explicit asset ownership, and prefab-instance lifetime management
on top of Unity Addressables. Unity still performs loading, instantiation, reference counting,
catalog updates, and bundle caching; this package expresses ownership through a scoped service,
leases, and instance handles.

## Requirements

- Unity 6000.6.0f1 or newer
- `com.unity.addressables` 2.9.1 or newer
- Package ID: `com.jeomseon.unity.addressables`

`com.unity.addressables` is declared as a dependency in `package.json`, so UPM resolves it automatically.

## Install via OpenUPM

Register the OpenUPM scoped registry once in your project's `Packages/manifest.json`.

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.jeomseon"
      ]
    }
  ],
  "dependencies": {
    "com.jeomseon.unity.addressables": "0.3.1"
  }
}
```

## Install via Git URL

Enter the following URL in Unity Package Manager's `Install package from git URL`.

```text
https://github.com/jeomseon0516/Unity.Addressables.git#v0.3.1
```

## Setup

1. Create an `AddressablesConfiguration` from `Tool/Addressables/Configuration`.
2. Add `AddressablesHost` to a root GameObject.
3. Assign the configuration to the host.
4. Pass `host.Service` as `IAddressablesService` to a composition root or provider.

Configuration assets contain policy only and never own runtime handles. A host lazily creates
its service and disposes every remaining lease and instance when the host is destroyed. Enable
`Dont Destroy On Load` for an application-lifetime host; a persistent host must be on a root
GameObject.

Configuration options control:

- explicit or external-destroy-safe prefab release;
- catalog update during first initialization;
- unused bundle-cache cleanup after a catalog update;
- warnings for resources still outstanding when the service is disposed;
- optional allocation stack traces for leak diagnostics.

## Dynamic keys and serialized references

Use dynamic keys for server-provided or runtime-selected content:

```csharp
AddressableAssetLease<Material> lease =
    await service.LoadAssetAsync<Material>(runtimeKey, cancellationToken);
```

Use a type-safe serialized reference for project-owned assets:

```csharp
[SerializeField] private AssetReferenceTexture2D _texture;

AddressableAssetLease<Texture2D> lease =
    await service.LoadAssetReferenceAsync<Texture2D>(
        _texture,
        cancellationToken);
```

When Unity does not provide a concrete reference for a type, define a small derived reference:

```csharp
[Serializable]
public sealed class MaterialReference : AssetReferenceT<Material>
{
    public MaterialReference(string guid) : base(guid) { }
}
```

An asmdef that uses the serialized-reference API must directly reference `Unity.Addressables`.
Consumers that only use dynamic-key methods do not need that extra assembly reference.

## Asset ownership

Keep a lease alive for as long as another object uses its asset:

```csharp
private AddressableAssetLease<Material> _materialLease;

private async Awaitable LoadAsync(CancellationToken cancellationToken)
{
    _materialLease?.Dispose();
    _materialLease = await service.LoadAssetReferenceAsync<Material>(
        _material,
        cancellationToken);
    renderer.sharedMaterial = _materialLease.Asset;
}
```

Clear consumers and dispose the lease when ownership ends. A local `using` is appropriate only
when all use of the asset finishes inside that scope.

Load a serialized label into one collection lease:

```csharp
AddressableAssetCollectionLease<Sprite> icons =
    await service.LoadLabelAssetsAsync<Sprite>(
        _uiIcons,
        releaseDependenciesOnFailure: true,
        cancellationToken: cancellationToken);
```

Dispose the collection lease once; do not release each item separately.

## Prefab instances

```csharp
AddressableInstanceHandle handle =
    await service.InstantiateReferenceAsync(
        _enemyPrefab,
        parent,
        cancellationToken);

Enemy enemy = handle.GetComponent<Enemy>();
```

Call `handle.Dispose()` for normal removal. It uses `Addressables.ReleaseInstance` to destroy the
GameObject and release its operation together.

`Explicit` requires the owner to dispose the handle. The default `ReleaseOnDestroy` policy also
attaches an internal observer so an external `Destroy` or scene shutdown releases the operation
exactly once. Explicit disposal remains the preferred normal path.

## Initialization, catalogs, and cancellation

Every load and instantiate operation ensures initialization. You can initialize earlier:

```csharp
await service.InitializeAsync(cancellationToken);
```

Concurrent initialization calls share one task. If configured, the first initialization checks
for catalog updates, applies discovered catalogs, and optionally removes unused cached bundles
before resources are loaded.

A cancellation token does not forcibly abort Unity's underlying Addressables download. Once the
operation completes, the service observes cancellation, releases the resulting handle, and throws
`OperationCanceledException`, so canceled results never remain owned by the service.

## Diagnostics

```csharp
Debug.Log(service.ActiveResourceCount);

foreach (AddressableResourceInfo info in service.ActiveResources)
{
    Debug.Log($"{info.Kind}: {info.Key}, {info.ResourceType.Name}");
}
```

Each diagnostic entry contains its resource kind, key, type, UTC creation time, and an optional
allocation stack trace. In Play Mode, the `AddressablesHost` Inspector displays initialization,
active ownership, age, and stack traces without creating a service merely for inspection.

Allocation stack capture has a cost and should normally be enabled only while diagnosing a leak.
The service can also warn when it must dispose resources that their immediate owners did not
release first.

## Recommended provider boundary

Production gameplay should usually depend on a domain provider rather than keys, leases, or
Addressables types. The provider receives `IAddressablesService` and a serialized asset
configuration, owns leases and instance handles, and returns domain objects.

The `Basic Usage` sample demonstrates:

- a ScriptableObject containing typed prefab and TextAsset references;
- a provider that owns multiple prefab handles and a shared TextAsset lease;
- individual release, release-all, and external-destroy cleanup;
- active resource diagnostics in the host Inspector.

## API guide

| Input and target | API | Owned result |
| --- | --- | --- |
| Dynamic key, one asset | `LoadAssetAsync<T>` | `AddressableAssetLease<T>` |
| Serialized reference, one asset | `LoadAssetReferenceAsync<T>` | `AddressableAssetLease<T>` |
| Dynamic key or label, many assets | `LoadAssetsAsync<T>` | `AddressableAssetCollectionLease<T>` |
| Serialized label, many assets | `LoadLabelAssetsAsync<T>` | `AddressableAssetCollectionLease<T>` |
| Dynamic prefab key | `InstantiateAsync` | `AddressableInstanceHandle` |
| Serialized prefab reference | `InstantiateReferenceAsync` | `AddressableInstanceHandle` |

## Future considerations

The public interface records future consideration for owned scene leases, dependency download and
size operations, explicit runtime catalog updates, progress and timeout policy, and location
queries. These are intentionally deferred until concrete project requirements justify expanding
the ownership model.

## Basic Usage sample

Import `Basic Usage` from Package Manager, run
`Tool/Addressables/Setup Basic Usage Sample`, and open
`Scenes/AddressablesBasicUsage`. The scene verifies serialized references, multiple provider-owned
instances, explicit release, external-destroy protection, a TextAsset lease, and live diagnostics.

## Tests

Add the package to `testables`, then run
`Jeomseon.Unity.Addressables.PlayModeTests` in Unity Test Runner.

## License

[MIT License](./LICENSE.md)
