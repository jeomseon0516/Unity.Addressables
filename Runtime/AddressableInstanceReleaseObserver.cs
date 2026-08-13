using System;
using UnityEngine;

namespace Jeomseon.Unity.Addressables
{
    /// <summary>
    /// Reports external instance destruction to its owning handle.
    /// 외부 Instance 파괴를 소유 Handle에 알립니다.
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class AddressableInstanceReleaseObserver : MonoBehaviour
    {
        private Action _onDestroyed;

        internal void Initialize(Action onDestroyed) => _onDestroyed = onDestroyed;

        internal void Detach() => _onDestroyed = null;

        private void OnDestroy()
        {
            Action callback = _onDestroyed;
            _onDestroyed = null;
            callback?.Invoke();
        }
    }
}
