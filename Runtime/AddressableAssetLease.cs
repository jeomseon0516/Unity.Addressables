using System;
using UnityEngine.ResourceManagement.AsyncOperations;
using AddressablesApi = UnityEngine.AddressableAssets.Addressables;

namespace Jeomseon.Unity.Addressables
{
    /// <summary>
    /// Owns one loaded Addressable asset operation until explicitly disposed.
    /// 명시적으로 Dispose할 때까지 로드된 Addressable Asset Operation 하나를 소유합니다.
    /// </summary>
    public sealed class AddressableAssetLease<T> : IDisposable
        where T : UnityEngine.Object
    {
        private AsyncOperationHandle<T> _handle;
        private Action<IDisposable> _onReleased;
        private Action<IDisposable> _onRetained;

        /// <summary>Gets the loaded asset. 로드된 Asset을 가져옵니다.</summary>
        public T Asset { get; }

        /// <summary>Gets whether this lease still owns a valid handle. 이 Lease가 유효한 Handle을 소유하는지 가져옵니다.</summary>
        public bool IsValid { get; private set; }

        internal AddressableAssetLease(
            AsyncOperationHandle<T> handle,
            Action<IDisposable> onReleased,
            Action<IDisposable> onRetained = null)
        {
            _handle = handle;
            _onReleased = onReleased;
            _onRetained = onRetained;
            Asset = handle.Result;
            IsValid = true;
        }

        /// <summary>
        /// Creates an independently owned lease for the same loaded asset.
        /// 같은 로드 Asset에 대한 독립 소유 Lease를 생성합니다.
        /// </summary>
        public AddressableAssetLease<T> Retain()
        {
            if (!IsValid)
                throw new ObjectDisposedException(nameof(AddressableAssetLease<T>));

            AsyncOperationHandle<T> retainedHandle =
                AddressablesApi.ResourceManager.Acquire(_handle);
            var retained = new AddressableAssetLease<T>(
                retainedHandle,
                _onReleased,
                _onRetained);
            try
            {
                _onRetained?.Invoke(retained);
                return retained;
            }
            catch
            {
                retained.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Releases the Addressables operation exactly once.
        /// Addressables Operation을 정확히 한 번 해제합니다.
        /// </summary>
        public void Dispose()
        {
            if (!IsValid) return;
            IsValid = false;
            Action<IDisposable> callback = _onReleased;
            _onReleased = null;
            _onRetained = null;
            if (_handle.IsValid()) AddressablesApi.Release(_handle);
            callback?.Invoke(this);
        }
    }
}
