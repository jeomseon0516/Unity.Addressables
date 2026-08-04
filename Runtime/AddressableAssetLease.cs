using System;
using UnityEngine.ResourceManagement.AsyncOperations;
using AddressablesApi = UnityEngine.AddressableAssets.Addressables;

namespace Jeomseon.Addressables
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

        /// <summary>Gets the loaded asset. 로드된 Asset을 가져옵니다.</summary>
        public T Asset { get; }

        /// <summary>Gets whether this lease still owns a valid handle. 이 Lease가 유효한 Handle을 소유하는지 가져옵니다.</summary>
        public bool IsValid { get; private set; }

        internal AddressableAssetLease(
            AsyncOperationHandle<T> handle,
            Action<IDisposable> onReleased)
        {
            _handle = handle;
            _onReleased = onReleased;
            Asset = handle.Result;
            IsValid = true;
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
            if (_handle.IsValid()) AddressablesApi.Release(_handle);
            callback?.Invoke(this);
        }
    }
}
