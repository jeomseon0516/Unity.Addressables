using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using AddressablesApi = UnityEngine.AddressableAssets.Addressables;

namespace Jeomseon.Addressables
{
    /// <summary>
    /// Owns one Addressables operation that loaded a collection of assets.
    /// Asset 컬렉션을 로드한 Addressables Operation 하나를 소유합니다.
    /// </summary>
    public sealed class AddressableAssetCollectionLease<T> : IDisposable
        where T : UnityEngine.Object
    {
        private AsyncOperationHandle<IList<T>> _handle;
        private Action<IDisposable> _onReleased;

        /// <summary>Gets the loaded assets. 로드된 Asset 목록을 가져옵니다.</summary>
        public IReadOnlyList<T> Assets { get; }

        /// <summary>Gets whether this lease owns a valid handle. 이 Lease가 유효한 Handle을 소유하는지 가져옵니다.</summary>
        public bool IsValid { get; private set; }

        internal AddressableAssetCollectionLease(
            AsyncOperationHandle<IList<T>> handle,
            Action<IDisposable> onReleased)
        {
            _handle = handle;
            _onReleased = onReleased;
            Assets = new List<T>(handle.Result);
            IsValid = true;
        }

        /// <summary>
        /// Releases the collection operation and all of its owned references exactly once.
        /// Collection Operation과 소유 참조를 정확히 한 번 해제합니다.
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
