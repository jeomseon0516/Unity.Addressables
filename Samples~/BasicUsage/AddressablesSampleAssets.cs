using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace Jeomseon.Samples.Addressables
{
    /// <summary>
    /// Groups the sample's type-safe serialized Addressables references.
    /// Sample의 타입 안전한 직렬화 Addressables Reference를 묶습니다.
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(AddressablesSampleAssets),
        menuName = "Tool/Addressables/Samples/Basic Usage Assets")]
    public sealed class AddressablesSampleAssets : ScriptableObject
    {
        [SerializeField, FormerlySerializedAs("_prefab")] private AssetReferenceGameObject prefab;
        [SerializeField, FormerlySerializedAs("_message")] private TextAssetReference message;

        /// <summary>Gets the sample prefab reference. Sample Prefab Reference를 가져옵니다.</summary>
        public AssetReferenceGameObject Prefab => prefab;

        /// <summary>Gets the sample message reference. Sample Message Reference를 가져옵니다.</summary>
        public TextAssetReference Message => message;
    }
}
