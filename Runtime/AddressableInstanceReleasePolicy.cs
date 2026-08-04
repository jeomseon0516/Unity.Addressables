namespace Jeomseon.Addressables
{
    /// <summary>
    /// Selects how an Addressable prefab instance releases its operation handle.
    /// Addressable Prefab 인스턴스가 Operation Handle을 해제하는 방식을 선택합니다.
    /// </summary>
    public enum AddressableInstanceReleasePolicy
    {
        /// <summary>
        /// Requires the returned instance handle to be disposed explicitly.
        /// 반환된 Instance Handle을 명시적으로 Dispose해야 합니다.
        /// </summary>
        Explicit,

        /// <summary>
        /// Also releases the operation when the instance is destroyed externally.
        /// 인스턴스가 외부에서 파괴될 때도 Operation을 해제합니다.
        /// </summary>
        ReleaseOnDestroy
    }
}
