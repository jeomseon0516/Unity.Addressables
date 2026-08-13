using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using AddressablesApi = UnityEngine.AddressableAssets.Addressables;

namespace Jeomseon.Unity.Addressables
{
    /// <summary>
    /// Owns runtime Addressables leases and instances for one composition scope.
    /// 하나의 Composition Scope에 속한 Addressables Lease와 Instance를 소유합니다.
    /// </summary>
    public sealed class AddressablesService : IAddressablesService
    {
        private readonly AddressableInstanceReleasePolicy _instanceReleasePolicy;
        private readonly bool _updateCatalogOnInitialize;
        private readonly bool _cleanBundleCacheAfterCatalogUpdate;
        private readonly Dictionary<IDisposable, AddressableResourceInfo> _resources = new();
        private readonly bool _logOutstandingResourcesOnDispose;
        private readonly bool _captureAllocationStackTrace;
        private Task _initializationTask;
        private bool _initialized;
        private bool _disposed;

        /// <inheritdoc />
        public bool IsInitialized => _initialized;

        /// <inheritdoc />
        public int ActiveResourceCount => _resources.Count;

        /// <inheritdoc />
        public IReadOnlyList<AddressableResourceInfo> ActiveResources
        {
            get
            {
                var snapshot = new AddressableResourceInfo[_resources.Count];
                _resources.Values.CopyTo(snapshot, 0);
                return snapshot;
            }
        }

        /// <summary>
        /// Creates a runtime service from optional serialized policy.
        /// 선택적인 직렬화 정책으로 Runtime Service를 생성합니다.
        /// </summary>
        public AddressablesService(AddressablesConfiguration configuration = null)
        {
            _instanceReleasePolicy = configuration != null
                ? configuration.InstanceReleasePolicy
                : AddressableInstanceReleasePolicy.ReleaseOnDestroy;
            _updateCatalogOnInitialize = configuration != null &&
                configuration.UpdateCatalogOnInitialize;
            _cleanBundleCacheAfterCatalogUpdate = configuration == null ||
                configuration.CleanBundleCacheAfterCatalogUpdate;
            _logOutstandingResourcesOnDispose = configuration == null ||
                configuration.LogOutstandingResourcesOnDispose;
            _captureAllocationStackTrace = configuration != null &&
                configuration.CaptureAllocationStackTrace;
        }

        /// <inheritdoc />
        public async Awaitable InitializeAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            if (_initialized) return;
            cancellationToken.ThrowIfCancellationRequested();
            _initializationTask ??= InitializeCoreAsync();
            try
            {
                await _initializationTask;
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch
            {
                if (!_initialized) _initializationTask = null;
                throw;
            }
        }

        /// <inheritdoc />
        public async Awaitable<AddressableAssetCollectionLease<T>> LoadAssetsAsync<T>(
            object key,
            bool releaseDependenciesOnFailure = true,
            CancellationToken cancellationToken = default)
            where T : UnityEngine.Object
        {
            ValidateKey(key);
            await InitializeAsync(cancellationToken);
            AsyncOperationHandle<IList<T>> handle = AddressablesApi.LoadAssetsAsync<T>(
                key,
                null,
                releaseDependenciesOnFailure);
            try
            {
                await handle.Task;
                cancellationToken.ThrowIfCancellationRequested();
                EnsureSucceeded(handle, key);
                var lease = new AddressableAssetCollectionLease<T>(handle, RemoveResource);
                TrackResource(
                    lease,
                    AddressableResourceKind.AssetCollection,
                    key,
                    typeof(T));
                return lease;
            }
            catch
            {
                if (handle.IsValid()) AddressablesApi.Release(handle);
                throw;
            }
        }

        /// <inheritdoc />
        public Awaitable<AddressableAssetCollectionLease<T>> LoadLabelAssetsAsync<T>(
            AssetLabelReference label,
            bool releaseDependenciesOnFailure = true,
            CancellationToken cancellationToken = default)
            where T : UnityEngine.Object
        {
            ValidateReference(label, nameof(label));
            return LoadAssetsAsync<T>(
                label.RuntimeKey,
                releaseDependenciesOnFailure,
                cancellationToken);
        }

        /// <inheritdoc />
        public async Awaitable<AddressableAssetLease<T>> LoadAssetAsync<T>(
            object key,
            CancellationToken cancellationToken = default)
            where T : UnityEngine.Object
        {
            ValidateKey(key);
            await InitializeAsync(cancellationToken);
            AsyncOperationHandle<T> handle = AddressablesApi.LoadAssetAsync<T>(key);
            try
            {
                await handle.Task;
                cancellationToken.ThrowIfCancellationRequested();
                EnsureSucceeded(handle, key);
                var lease = new AddressableAssetLease<T>(handle, RemoveResource);
                TrackResource(lease, AddressableResourceKind.Asset, key, typeof(T));
                return lease;
            }
            catch
            {
                if (handle.IsValid()) AddressablesApi.Release(handle);
                throw;
            }
        }

        /// <inheritdoc />
        public Awaitable<AddressableAssetLease<T>> LoadAssetReferenceAsync<T>(
            AssetReferenceT<T> reference,
            CancellationToken cancellationToken = default)
            where T : UnityEngine.Object
        {
            ValidateReference(reference, nameof(reference));
            return LoadAssetAsync<T>(reference.RuntimeKey, cancellationToken);
        }

        /// <inheritdoc />
        public async Awaitable<AddressableInstanceHandle> InstantiateAsync(
            object key,
            Transform parent = null,
            CancellationToken cancellationToken = default)
        {
            ValidateKey(key);
            await InitializeAsync(cancellationToken);
            var parameters = new InstantiationParameters(parent, false);
            AsyncOperationHandle<GameObject> handle =
                AddressablesApi.InstantiateAsync(key, parameters, false);
            try
            {
                await handle.Task;
                cancellationToken.ThrowIfCancellationRequested();
                EnsureSucceeded(handle, key);
                var instance = new AddressableInstanceHandle(
                    handle,
                    _instanceReleasePolicy,
                    RemoveResource);
                TrackResource(instance, AddressableResourceKind.Instance, key, typeof(GameObject));
                return instance;
            }
            catch
            {
                if (handle.IsValid()) AddressablesApi.ReleaseInstance(handle);
                throw;
            }
        }

        /// <inheritdoc />
        public Awaitable<AddressableInstanceHandle> InstantiateReferenceAsync(
            AssetReferenceGameObject reference,
            Transform parent = null,
            CancellationToken cancellationToken = default)
        {
            ValidateReference(reference, nameof(reference));
            return InstantiateAsync(reference.RuntimeKey, parent, cancellationToken);
        }

        /// <summary>
        /// Releases every owned lease and instance. The service cannot be reused afterward.
        /// 소유한 모든 Lease와 Instance를 해제합니다. 이후 Service는 재사용할 수 없습니다.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            LogOutstandingResources();
            IDisposable[] resources = new IDisposable[_resources.Count];
            _resources.Keys.CopyTo(resources, 0);
            foreach (IDisposable resource in resources) resource.Dispose();
            _resources.Clear();
        }

        private async Awaitable UpdateCatalogsAsync(CancellationToken cancellationToken)
        {
            if (_resources.Count != 0)
            {
                throw new InvalidOperationException(
                    "Catalogs cannot be updated while this service owns resources. / " +
                    "Service가 Resource를 소유하는 동안 Catalog를 갱신할 수 없습니다.");
            }

            AsyncOperationHandle<List<string>> check =
                AddressablesApi.CheckForCatalogUpdates(false);
            try
            {
                await check.Task;
                cancellationToken.ThrowIfCancellationRequested();
                EnsureSucceeded(check, "catalog update check");
                if (check.Result == null || check.Result.Count == 0) return;
                AsyncOperationHandle<List<IResourceLocator>>
                    update = AddressablesApi.UpdateCatalogs(
                        _cleanBundleCacheAfterCatalogUpdate,
                        check.Result,
                        false);
                try
                {
                    await update.Task;
                    cancellationToken.ThrowIfCancellationRequested();
                    EnsureSucceeded(update, "catalog update");
                }
                finally
                {
                    if (update.IsValid()) AddressablesApi.Release(update);
                }
            }
            finally
            {
                if (check.IsValid()) AddressablesApi.Release(check);
            }
        }

        private async Task InitializeCoreAsync()
        {
            if (_updateCatalogOnInitialize) await UpdateCatalogsAsync(CancellationToken.None);
            ThrowIfDisposed();
            _initialized = true;
        }

        private void RemoveResource(IDisposable resource) => _resources.Remove(resource);

        private void TrackResource(
            IDisposable resource,
            AddressableResourceKind kind,
            object key,
            Type resourceType)
        {
            _resources.Add(
                resource,
                new AddressableResourceInfo(
                    kind,
                    key,
                    resourceType,
                    _captureAllocationStackTrace));
        }

        private void LogOutstandingResources()
        {
            if (!_logOutstandingResourcesOnDispose || _resources.Count == 0) return;

            Debug.LogWarning(
                $"[{nameof(AddressablesService)}] Disposing with {_resources.Count} " +
                "outstanding resource(s). Their owners should normally dispose them first. / " +
                $"해제되지 않은 Resource {_resources.Count}개를 Service가 정리합니다. " +
                "일반적으로 각 소유자가 먼저 Dispose해야 합니다.");

            foreach (AddressableResourceInfo info in _resources.Values)
            {
                string stack = string.IsNullOrEmpty(info.AllocationStackTrace)
                    ? string.Empty
                    : $"\n{info.AllocationStackTrace}";
                Debug.LogWarning(
                    $"- {info.Kind}: {info.Key} ({info.ResourceType.FullName}), " +
                    $"created {info.CreatedAtUtc:O}{stack}");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AddressablesService));
        }

        private static void ValidateKey(object key)
        {
            if (key == null || key is string text && string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException(
                    "An Addressables key is required. / Addressables Key가 필요합니다.",
                    nameof(key));
            }
        }

        private static void ValidateReference(
            IKeyEvaluator reference,
            string parameterName)
        {
            if (reference == null || !reference.RuntimeKeyIsValid())
            {
                throw new ArgumentException(
                    "A valid serialized Addressables reference is required. / " +
                    "유효한 직렬화 Addressables Reference가 필요합니다.",
                    parameterName);
            }
        }

        private static void EnsureSucceeded<T>(AsyncOperationHandle<T> handle, object key)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null) return;
            throw new InvalidOperationException(
                $"Addressables operation failed for '{key}'. / Addressables 작업에 실패했습니다: '{key}'.",
                handle.OperationException);
        }
    }
}
