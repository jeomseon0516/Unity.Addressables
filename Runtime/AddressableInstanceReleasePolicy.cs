namespace Jeomseon.Unity.Addressables
{
    /// <summary>
    /// Selects how an Addressable prefab instance releases its operation handle.
    /// Addressable Prefab 인스턴스가 Operation Handle을 해제하는 방식을 선택합니다.
    /// </summary>
    public enum AddressableInstanceReleasePolicy
    {
        /// <summary>
        /// Gives lifetime responsibility to the returned handle. The handle may be registered with an
        /// ownership host for automatic release.
        /// 반환된 Handle에 수명 책임을 부여합니다. Ownership Host에 등록하면 자동 해제됩니다.
        /// </summary>
        HandleLifetime = 0,

        /// <summary>Legacy name for handle-owned lifetime. Handle 소유 수명의 이전 이름입니다.</summary>
        [System.Obsolete(
            "Use HandleLifetime. For automatic release, use [ManagedAsset] and its generated setter from " +
            "com.jeomseon.unity.addressables.ownership.")]
        Explicit = HandleLifetime,

        /// <summary>
        /// Also releases the operation when the instance is destroyed externally.
        /// 인스턴스가 외부에서 파괴될 때도 Operation을 해제합니다.
        /// </summary>
        [System.Obsolete(
            "ReleaseOnDestroy uses a legacy observer component. Use HandleLifetime with " +
            "com.jeomseon.unity.addressables.ownership [ManagedAsset] for automatic owner-lifetime release.")]
        ReleaseOnDestroy = 1
    }
}
