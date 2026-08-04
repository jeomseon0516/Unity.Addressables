# Jeomseon Unity Addressables

Serialized configuration, explicit asset ownership, and prefab-instance lifetime management
on top of Unity Addressables 2.9.1.

Create an `AddressablesConfiguration`, assign it to an `AddressablesHost`, and pass the host's
`IAddressablesService` to consumers. Configuration assets never own runtime handles; the host
disposes every owned lease and instance with its service.

```csharp
using AddressableAssetLease<Material> material =
    await service.LoadAssetAsync<Material>("EnemyMaterial", cancellationToken);

using AddressableInstanceHandle instance =
    await service.InstantiateAsync("EnemyPrefab", parent, cancellationToken);
```

The package delegates loading, instantiation, labels, reference counting, catalog updates, and
bundle caching to Unity's official APIs. Its added responsibilities are configuration, scoped
service ownership, leases, and optional protection against externally destroyed instances.
GameObject Pooling integration is intentionally outside this package.

The `Basic Usage` sample includes a playable scene. After importing it, run
`Tool/Addressables/Setup Basic Usage Sample`, open `Scenes/AddressablesBasicUsage`, and use the
in-game controls to verify explicit release, external-destroy protection, a TextAsset lease,
and the active-resource count.
