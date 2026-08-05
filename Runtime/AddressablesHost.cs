using System;
using UnityEngine;

namespace Jeomseon.Addressables
{
    /// <summary>
    /// Creates and owns an Addressables service from serialized configuration.
    /// 직렬화된 Configuration으로 Addressables Service를 생성하고 소유합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AddressablesHost : MonoBehaviour
    {
        [SerializeField] private AddressablesConfiguration _configuration;
        [SerializeField] private bool _initializeOnStart = true;
        [SerializeField] private bool _dontDestroyOnLoad;
        private AddressablesService _service;

        /// <summary>
        /// Gets the owned service, creating it lazily when required.
        /// 소유 Service를 가져오며 필요한 경우 지연 생성합니다.
        /// </summary>
        public IAddressablesService Service => _service ??= new AddressablesService(_configuration);

        /// <summary>
        /// Gets whether this Host has created its runtime Service.
        /// 이 Host가 Runtime Service를 생성했는지 가져옵니다.
        /// </summary>
        public bool HasCreatedService => _service != null;

        /// <summary>
        /// Gets the existing Service without creating it.
        /// Service를 새로 생성하지 않고 기존 Service를 가져옵니다.
        /// </summary>
        public bool TryGetCreatedService(out IAddressablesService service)
        {
            service = _service;
            return service != null;
        }

        private void Awake()
        {
            if (!_dontDestroyOnLoad || !Application.isPlaying) return;
            if (transform.parent != null)
            {
                throw new InvalidOperationException(
                    $"A persistent {nameof(AddressablesHost)} must be on a root GameObject. / " +
                    $"영속 {nameof(AddressablesHost)}는 Root GameObject에 있어야 합니다.");
            }

            DontDestroyOnLoad(gameObject);
        }

        private async void Start()
        {
            if (!_initializeOnStart) return;
            try
            {
                await Service.InitializeAsync(destroyCancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private void OnDestroy()
        {
            _service?.Dispose();
            _service = null;
        }
    }
}
