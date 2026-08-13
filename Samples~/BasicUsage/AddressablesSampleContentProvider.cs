using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Jeomseon.Unity.Addressables;
using UnityEngine;

namespace Jeomseon.Samples.Addressables
{
    /// <summary>
    /// Owns the sample's Addressables leases and handles behind a domain-facing API.
    /// Sample의 Addressables Lease와 Handle을 도메인 API 뒤에서 소유합니다.
    /// </summary>
    public sealed class AddressablesSampleContentProvider : IAddressablesSampleContentProvider
    {
        private readonly IAddressablesService _service;
        private readonly AddressablesSampleAssets _assets;
        private readonly Dictionary<GameObject, AddressableInstanceHandle> _actors =
            new(ReferenceComparer.Instance);
        private AddressableAssetLease<TextAsset> _message;

        /// <inheritdoc />
        public int OwnedActorCount
        {
            get
            {
                PruneReleasedActors();
                return _actors.Count;
            }
        }

        /// <summary>Creates a provider from a Service and serialized asset configuration.</summary>
        /// <remarks>Service와 직렬화 Asset Configuration으로 Provider를 생성합니다.</remarks>
        public AddressablesSampleContentProvider(
            IAddressablesService service,
            AddressablesSampleAssets assets)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _assets = assets != null
                ? assets
                : throw new ArgumentNullException(nameof(assets));
        }

        /// <inheritdoc />
        public async Awaitable<GameObject> InstantiateActorAsync(
            Transform parent,
            CancellationToken cancellationToken = default)
        {
            PruneReleasedActors();
            AddressableInstanceHandle handle = await _service.InstantiateReferenceAsync(
                _assets.Prefab,
                parent,
                cancellationToken);
            _actors.Add(handle.Instance, handle);
            return handle.Instance;
        }

        /// <inheritdoc />
        public bool ReleaseActor(GameObject actor)
        {
            if (ReferenceEquals(actor, null) || !_actors.Remove(actor, out var handle))
            {
                return false;
            }

            handle.Dispose();
            return true;
        }

        /// <inheritdoc />
        public void ReleaseAllActors()
        {
            foreach (AddressableInstanceHandle handle in _actors.Values) handle.Dispose();
            _actors.Clear();
        }

        /// <inheritdoc />
        public void PruneReleasedActors()
        {
            if (_actors.Count == 0) return;
            var released = new List<GameObject>();
            foreach (KeyValuePair<GameObject, AddressableInstanceHandle> pair in _actors)
            {
                if (!pair.Value.IsValid) released.Add(pair.Key);
            }

            foreach (GameObject actor in released) _actors.Remove(actor);
        }

        /// <inheritdoc />
        public async Awaitable<TextAsset> LoadMessageAsync(
            CancellationToken cancellationToken = default)
        {
            if (_message is { IsValid: true }) return _message.Asset;
            _message = await _service.LoadAssetReferenceAsync<TextAsset>(
                _assets.Message,
                cancellationToken);
            return _message.Asset;
        }

        /// <inheritdoc />
        public void ReleaseMessage()
        {
            _message?.Dispose();
            _message = null;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            ReleaseAllActors();
            ReleaseMessage();
        }

        private sealed class ReferenceComparer : IEqualityComparer<GameObject>
        {
            internal static ReferenceComparer Instance { get; } = new();

            public bool Equals(GameObject x, GameObject y) => ReferenceEquals(x, y);

            public int GetHashCode(GameObject value) => RuntimeHelpers.GetHashCode(value);
        }
    }
}
