using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using AddressablesApi = UnityEngine.AddressableAssets.Addressables;

namespace Jeomseon.Unity.Addressables
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
        private Action<IDisposable> _onRetained;

        /// <summary>Gets the loaded assets. 로드된 Asset 목록을 가져옵니다.</summary>
        public IReadOnlyList<T> Assets { get; }

        /// <summary>Gets whether this lease owns a valid handle. 이 Lease가 유효한 Handle을 소유하는지 가져옵니다.</summary>
        public bool IsValid { get; private set; }

        internal AddressableAssetCollectionLease(
            AsyncOperationHandle<IList<T>> handle,
            Action<IDisposable> onReleased,
            Action<IDisposable> onRetained = null)
        {
            _handle = handle;
            _onReleased = onReleased;
            _onRetained = onRetained;
            Assets = new List<T>(handle.Result);
            IsValid = true;
        }

        /// <summary>
        /// Creates an independently owned lease for the same collection operation.
        /// 같은 Collection Operation에 대한 독립 소유 Lease를 생성합니다.
        /// </summary>
        public AddressableAssetCollectionLease<T> Retain()
        {
            if (!IsValid)
                throw new ObjectDisposedException(nameof(AddressableAssetCollectionLease<T>));

            AsyncOperationHandle<IList<T>> retainedHandle =
                AddressablesApi.ResourceManager.Acquire(_handle);
            var retained = new AddressableAssetCollectionLease<T>(retainedHandle, _onReleased, _onRetained);
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
        /// Releases the collection operation and all of its owned references exactly once.
        /// Collection Operation과 소유 참조를 정확히 한 번 해제합니다.
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
