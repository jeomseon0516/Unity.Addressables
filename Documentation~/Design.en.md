# Addressables: lifetime and design

[한국어](Design.md) · [Installation and API details](../README.en.md)

## Scope

Unity Addressables supplies loading, reference counts, and instantiation. This package coordinates
scope-owned resource tracking, initial Catalog policy, distinct asset/instance release paths, and
cleanup after cancellation. It does not provide content servers or Bundle build/deployment pipelines.

| Type | Responsibility | Consumer contract |
| --- | --- | --- |
| `AddressablesConfiguration` | Serialized policy | Owns no runtime handles |
| `AddressablesHost` | Connects Unity lifetime to a Service | Choose Scene or persistent Application lifetime |
| `IAddressablesService` | Loading/instantiation interface | Suitable dependency for a project Provider |
| `AddressablesService` | Initialization and resource tracking | Dispose it when constructed directly |
| `AddressableAssetLease<T>` | One asset operation's release responsibility | Keep the Lease while using its Asset |
| `AddressableAssetCollectionLease<T>` | One collection operation | Do not release individual items separately |
| `AddressableInstanceHandle` | Instance and operation | Dispose calls ReleaseInstance |

## Lifecycle

Obtain the Host Service or construct one, request by key or AssetReference, retain the returned
Lease/Handle while using its value, detach consumers, then Dispose. Load/Instantiate ensure initialization.
Host/Service shutdown releases remaining resources and can log outstanding allocations.

Retain creates an independent release responsibility but remains tracked by the same Service.
It does not extend a resource beyond that Service's lifetime.

## Cancellation, failure, and defaults

Cancellation does not promise to immediately abort Unity's operation. The implementation awaits the
operation, checks the token, releases the acquired handle on cancellation/failure, and propagates the
exception. Passing destroyCancellationToken does not mean I/O stops immediately.

Initialization is shared within a Service. One caller's cancellation does not cancel shared initialization.
Initialization failures permit retry. Catalog updates occur before first use; there is no public API for
arbitrary updates while resources are owned. Failed collection operations throw rather than returning
a successful partial-result Lease.

Defaults: HandleLifetime for instances, Catalog updates off, unused cache cleanup after updates on,
outstanding-resource diagnostics on, allocation stack capture off. Empty keys and invalid References
are rejected. A disposed Service cannot be reused.

## Automatic and low-level release

Low-level callers own and Dispose the returned Handle. The separate Addressables Ownership package
automates replacement and owner-destruction release with ManagedAsset and generated setters. Avoid
assigning competing lifetime responsibilities for the same resource.

Legacy ReleaseOnDestroy uses an instance Observer and is Obsolete. Ordinary assets such as Sprite and
Material lack that instance destruction event; owner-bound generation provides their automatic lifetime.
Explicit is the legacy name of HandleLifetime.

## Run and inspect

Import [Basic Usage](../Samples~/BasicUsage), run its Setup menu to register sample content in the
consumer project's Addressables settings, and exercise Prefab/TextAsset acquisition and release.
[AddressablesSample](../Samples~/BasicUsage/AddressablesSample.cs) shows calls and exception handling.
Read [Service](../Runtime/AddressablesService.cs), [asset Lease](../Runtime/AddressableAssetLease.cs),
then [instance Handle](../Runtime/AddressableInstanceHandle.cs).
[PlayMode tests](../Tests/Runtime) cover real loading, cancellation, retain, and external Destroy policies.
Add the package to manifest testables. Distinguish historical results from a new test run; successful
installation alone does not verify every asynchronous path.
