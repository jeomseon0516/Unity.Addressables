using System;
using System.Threading;
using UnityEngine;

namespace Jeomseon.Samples.Addressables
{
    /// <summary>
    /// Defines the high-level sample content boundary without exposing keys, leases, or handles.
    /// Key, Lease 또는 Handle을 노출하지 않는 고수준 Sample Content 경계를 정의합니다.
    /// </summary>
    public interface IAddressablesSampleContentProvider : IDisposable
    {
        /// <summary>Gets the number of actor handles still owned by this Provider.</summary>
        /// <remarks>이 Provider가 계속 소유하는 Actor Handle 수를 가져옵니다.</remarks>
        int OwnedActorCount { get; }

        /// <summary>Instantiates the sample actor. Sample Actor를 생성합니다.</summary>
        Awaitable<GameObject> InstantiateActorAsync(
            Transform parent,
            CancellationToken cancellationToken = default);

        /// <summary>Releases one owned actor. 소유한 Actor 하나를 해제합니다.</summary>
        bool ReleaseActor(GameObject actor);

        /// <summary>Releases every owned actor. 소유한 모든 Actor를 해제합니다.</summary>
        void ReleaseAllActors();

        /// <summary>Removes handles already released by external destruction.</summary>
        /// <remarks>외부 파괴로 이미 해제된 Handle을 제거합니다.</remarks>
        void PruneReleasedActors();

        /// <summary>Loads the sample message. Sample Message를 로드합니다.</summary>
        Awaitable<TextAsset> LoadMessageAsync(
            CancellationToken cancellationToken = default);

        /// <summary>Releases the currently owned message. 현재 소유한 Message를 해제합니다.</summary>
        void ReleaseMessage();
    }
}
