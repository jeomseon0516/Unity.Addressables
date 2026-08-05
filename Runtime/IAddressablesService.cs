using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Jeomseon.Addressables
{
    /// <summary>
    /// Provides owned Addressable asset and prefab operations without exposing global caches.
    /// 전역 Cache를 노출하지 않고 소유권이 명확한 Addressable Asset 및 Prefab 작업을 제공합니다.
    /// </summary>
    public interface IAddressablesService : IDisposable
    {
        /* TODO(P3, addressables-advanced-operations): Extend the ownership model only when
         * concrete project requirements exist. / 실제 프로젝트 요구가 생길 때만 현재 소유권
         * 모델을 다음 기능으로 확장합니다.
         * - Scene loading through an owned scene lease / Scene Lease 기반 Scene 로드
         * - Dependency download size and predownload operations / 다운로드 크기 및 의존성 선다운로드
         * - Explicit runtime catalog update ownership / 명시적 Runtime Catalog 갱신 소유권
         * - Progress reporting and timeout policy / 진행률 보고와 Timeout 정책
         * - Resource-location queries without introducing a global cache / 전역 Cache 없는 Location 조회
         */

        /// <summary>
        /// Gets whether initialization completed successfully.
        /// 초기화가 성공적으로 완료됐는지 가져옵니다.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets the number of asset leases and prefab instances currently owned by the service.
        /// Service가 현재 소유한 Asset Lease와 Prefab Instance 수를 가져옵니다.
        /// </summary>
        int ActiveResourceCount { get; }

        /// <summary>
        /// Gets a snapshot of resources currently owned by this service.
        /// 이 Service가 현재 소유한 Resource Snapshot을 가져옵니다.
        /// </summary>
        IReadOnlyList<AddressableResourceInfo> ActiveResources { get; }

        /// <summary>
        /// Checks and applies configured catalog updates before assets are loaded.
        /// Asset 로드 전에 설정된 Catalog 갱신을 확인하고 적용합니다.
        /// </summary>
        Awaitable InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads an asset and returns a lease that owns its operation handle.
        /// Asset을 로드하고 Operation Handle을 소유하는 Lease를 반환합니다.
        /// </summary>
        Awaitable<AddressableAssetLease<T>> LoadAssetAsync<T>(
            object key,
            CancellationToken cancellationToken = default)
            where T : UnityEngine.Object;

        /// <summary>
        /// Loads a type-safe serialized AssetReference and returns its owning lease.
        /// 타입이 제한된 직렬화 AssetReference를 로드하고 소유 Lease를 반환합니다.
        /// </summary>
        Awaitable<AddressableAssetLease<T>> LoadAssetReferenceAsync<T>(
            AssetReferenceT<T> reference,
            CancellationToken cancellationToken = default)
            where T : UnityEngine.Object;

        /// <summary>
        /// Loads every asset resolved by a key or label and returns one owning collection lease.
        /// Key 또는 Label로 조회된 모든 Asset을 로드하고 하나의 소유 Collection Lease를 반환합니다.
        /// </summary>
        Awaitable<AddressableAssetCollectionLease<T>> LoadAssetsAsync<T>(
            object key,
            bool releaseDependenciesOnFailure = true,
            CancellationToken cancellationToken = default)
            where T : UnityEngine.Object;

        /// <summary>
        /// Loads every asset resolved by a serialized label reference.
        /// 직렬화된 Label Reference로 조회된 모든 Asset을 로드합니다.
        /// </summary>
        Awaitable<AddressableAssetCollectionLease<T>> LoadLabelAssetsAsync<T>(
            AssetLabelReference label,
            bool releaseDependenciesOnFailure = true,
            CancellationToken cancellationToken = default)
            where T : UnityEngine.Object;

        /// <summary>
        /// Instantiates an Addressable prefab and returns its owned instance handle.
        /// Addressable Prefab을 생성하고 소유권이 있는 Instance Handle을 반환합니다.
        /// </summary>
        Awaitable<AddressableInstanceHandle> InstantiateAsync(
            object key,
            Transform parent = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Instantiates a serialized GameObject reference and returns its owning handle.
        /// 직렬화된 GameObject Reference를 생성하고 소유 Handle을 반환합니다.
        /// </summary>
        Awaitable<AddressableInstanceHandle> InstantiateReferenceAsync(
            AssetReferenceGameObject reference,
            Transform parent = null,
            CancellationToken cancellationToken = default);
    }
}
