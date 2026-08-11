using UnityEngine;
using UnityEngine.Serialization;

namespace Jeomseon.Addressables
{
    /// <summary>
    /// Stores serialized Addressables orchestration policy without owning runtime handles.
    /// Runtime Handle을 소유하지 않고 직렬화된 Addressables 오케스트레이션 정책을 보관합니다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "AddressablesConfiguration",
        menuName = "Tool/Addressables/Configuration")]
    public sealed class AddressablesConfiguration : ScriptableObject
    {
        [SerializeField, FormerlySerializedAs("_instanceReleasePolicy")] private AddressableInstanceReleasePolicy instanceReleasePolicy =
            AddressableInstanceReleasePolicy.ReleaseOnDestroy;
        [SerializeField, FormerlySerializedAs("_updateCatalogOnInitialize")] private bool updateCatalogOnInitialize;
        [SerializeField, FormerlySerializedAs("_cleanBundleCacheAfterCatalogUpdate")] private bool cleanBundleCacheAfterCatalogUpdate = true;
        [SerializeField, FormerlySerializedAs("_logOutstandingResourcesOnDispose")] private bool logOutstandingResourcesOnDispose = true;
        [SerializeField, FormerlySerializedAs("_captureAllocationStackTrace")] private bool captureAllocationStackTrace;

        /// <summary>
        /// Gets the prefab instance release policy.
        /// Prefab 인스턴스 해제 정책을 가져옵니다.
        /// </summary>
        public AddressableInstanceReleasePolicy InstanceReleasePolicy =>
            instanceReleasePolicy;

        /// <summary>
        /// Gets whether the service checks and applies catalog updates before first use.
        /// Service가 최초 사용 전에 Catalog 갱신을 확인하고 적용할지 가져옵니다.
        /// </summary>
        public bool UpdateCatalogOnInitialize => updateCatalogOnInitialize;

        /// <summary>
        /// Gets whether catalog updates remove unreferenced cached bundles.
        /// Catalog 갱신 후 참조되지 않는 캐시 Bundle을 제거할지 가져옵니다.
        /// </summary>
        public bool CleanBundleCacheAfterCatalogUpdate =>
            cleanBundleCacheAfterCatalogUpdate;

        /// <summary>
        /// Gets whether disposal logs resources that their callers did not release first.
        /// Dispose 전에 호출부가 해제하지 않은 Resource를 기록할지 가져옵니다.
        /// </summary>
        public bool LogOutstandingResourcesOnDispose =>
            logOutstandingResourcesOnDispose;

        /// <summary>
        /// Gets whether resource diagnostics capture allocation stack traces.
        /// Resource 진단에서 할당 StackTrace를 기록할지 가져옵니다.
        /// </summary>
        public bool CaptureAllocationStackTrace => captureAllocationStackTrace;
    }
}
