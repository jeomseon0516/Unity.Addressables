using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Jeomseon.Samples.Addressables
{
    /// <summary>
    /// Restricts the sample's serialized Addressables field to TextAsset values.
    /// Sample의 직렬화 Addressables 필드를 TextAsset 값으로 제한합니다.
    /// </summary>
    [Serializable]
    public sealed class TextAssetReference : AssetReferenceT<TextAsset>
    {
        /// <summary>
        /// Creates a TextAsset reference from its asset GUID.
        /// Asset GUID로 TextAsset Reference를 생성합니다.
        /// </summary>
        public TextAssetReference(string guid) : base(guid)
        {
        }
    }
}
