# Jeomseon Unity Addressables

[한국어](Overview.md) · [Installation](../README.en.md)

## Purpose

Adds service lifetimes, resource tracking, asset Leases, and prefab InstanceHandle release contracts to Unity Addressables.

## Usage flow

Use a Host or directly constructed Service, Load/Instantiate, keep the Lease/Handle during use, then Dispose. Host shutdown cleans up the Service's remaining resources.

## Contracts and limitations

Cancellation does not immediately stop I/O. Retain does not outlive its Service. Owner-bound automation is supplied by the separate Addressables Ownership package.

## Package requirements

- Package ID: `com.jeomseon.unity.addressables`
- Version: `0.4.0`
- Unity: `6000.6` / `0f1`
- `com.unity.addressables`: `2.9.1`

## Samples and verification

Import the following from Package Manager > Samples. Open the included Scene or Editor tool; follow any Setup step in its sample README.

- [Basic Usage](../Samples~/BasicUsage/README.md)
  - [AddressablesBasicUsage](../Samples~/BasicUsage/Scenes/AddressablesBasicUsage.unity)

[Test sources](../Tests)

Add the package ID to manifest `testables`, then run the relevant Unity Test Runner assemblies. These links describe available tests, not a new successful test run.

## Source map

- [AddressableAssetCollectionLease](../Runtime/AddressableAssetCollectionLease.cs)
- [AddressableAssetLease](../Runtime/AddressableAssetLease.cs)
- [AddressableInstanceHandle](../Runtime/AddressableInstanceHandle.cs)
- [AddressableInstanceReleasePolicy](../Runtime/AddressableInstanceReleasePolicy.cs)
- [AddressableResourceInfo](../Runtime/AddressableResourceInfo.cs)
- [AddressableResourceKind](../Runtime/AddressableResourceKind.cs)
- [AddressablesConfiguration](../Runtime/AddressablesConfiguration.cs)
- [AddressablesHost](../Runtime/AddressablesHost.cs)
- [AddressablesService](../Runtime/AddressablesService.cs)
- [IAddressablesService](../Runtime/IAddressablesService.cs)

[Detailed design](Design.en.md)
