namespace Jeomseon.Addressables
{
    /// <summary>
    /// Identifies the ownership category of an active Addressables resource.
    /// 활성 Addressables Resource의 소유권 종류를 식별합니다.
    /// </summary>
    public enum AddressableResourceKind
    {
        /// <summary>A single loaded asset. 단일 로드 Asset입니다.</summary>
        Asset,

        /// <summary>A collection loaded from a key or label. Key 또는 Label로 로드한 컬렉션입니다.</summary>
        AssetCollection,

        /// <summary>An instantiated prefab. 생성된 Prefab입니다.</summary>
        Instance
    }
}
