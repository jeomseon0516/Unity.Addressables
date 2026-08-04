using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using AddressablesApi = UnityEngine.AddressableAssets.Addressables;

namespace Jeomseon.Addressables
{
    /// <summary>
    /// Owns one Addressable prefab instance and its operation handle.
    /// Addressable Prefab 인스턴스 하나와 해당 Operation Handle을 소유합니다.
    /// </summary>
    public sealed class AddressableInstanceHandle : IDisposable
    {
        private AsyncOperationHandle<GameObject> _handle;
        private AddressableInstanceReleaseObserver _observer;
        private Action<IDisposable> _onReleased;

        /// <summary>Gets the instantiated GameObject. 생성된 GameObject를 가져옵니다.</summary>
        public GameObject Instance { get; }

        /// <summary>Gets whether this handle still owns the instance operation. 이 Handle이 Instance Operation을 계속 소유하는지 가져옵니다.</summary>
        public bool IsValid { get; private set; }

        internal AddressableInstanceHandle(
            AsyncOperationHandle<GameObject> handle,
            AddressableInstanceReleasePolicy releasePolicy,
            Action<IDisposable> onReleased)
        {
            _handle = handle;
            _onReleased = onReleased;
            Instance = handle.Result;
            IsValid = true;
            if (releasePolicy != AddressableInstanceReleasePolicy.ReleaseOnDestroy) return;
            _observer = Instance.AddComponent<AddressableInstanceReleaseObserver>();
            _observer.Initialize(ReleaseAfterExternalDestroy);
        }

        /// <summary>Gets a Component from the instantiated prefab. 생성된 Prefab에서 Component를 가져옵니다.</summary>
        public T GetComponent<T>() where T : Component => Instance.GetComponent<T>();

        /// <summary>
        /// Destroys and releases the instance through Addressables exactly once.
        /// Addressables를 통해 인스턴스를 파괴하고 정확히 한 번 해제합니다.
        /// </summary>
        public void Dispose()
        {
            if (!TryBeginRelease()) return;
            _observer?.Detach();
            _observer = null;
            if (_handle.IsValid()) AddressablesApi.ReleaseInstance(_handle);
            NotifyReleased();
        }

        private void ReleaseAfterExternalDestroy()
        {
            if (!TryBeginRelease()) return;
            _observer = null;
            if (_handle.IsValid()) AddressablesApi.Release(_handle);
            NotifyReleased();
        }

        private bool TryBeginRelease()
        {
            if (!IsValid) return false;
            IsValid = false;
            return true;
        }

        private void NotifyReleased()
        {
            Action<IDisposable> callback = _onReleased;
            _onReleased = null;
            callback?.Invoke(this);
        }
    }
}
