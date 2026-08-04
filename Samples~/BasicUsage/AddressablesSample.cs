using System;
using Jeomseon.Addressables;
using UnityEngine;

namespace Jeomseon.Samples.Addressables
{
    /// <summary>
    /// Demonstrates high-level prefab instantiation through an Addressables Host.
    /// Addressables Host를 통한 고수준 Prefab 생성을 보여줍니다.
    /// </summary>
    public sealed class AddressablesSample : MonoBehaviour
    {
        [SerializeField] private AddressablesHost _host;
        [SerializeField] private string _addressableKey;
        private AddressableInstanceHandle _instance;

        [ContextMenu("Instantiate Addressable Prefab / Addressable Prefab 생성")]
        private async void InstantiatePrefab()
        {
            if (_host == null || string.IsNullOrWhiteSpace(_addressableKey)) return;
            _instance?.Dispose();
            try
            {
                _instance = await _host.Service.InstantiateAsync(
                    _addressableKey,
                    transform,
                    destroyCancellationToken);
                Debug.Log($"Addressable instance created: {_instance.Instance.name}", this);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        [ContextMenu("Release Addressable Prefab / Addressable Prefab 해제")]
        private void ReleasePrefab()
        {
            _instance?.Dispose();
            _instance = null;
        }

        private void OnDestroy() => ReleasePrefab();
    }
}
