using Jeomseon.Prototype;
using UnityEngine;

namespace Jeomseon.Samples.Addressables
{
    public sealed class AddressablesSample : MonoBehaviour
    {
        [SerializeField] private string _addressableKey;

        [ContextMenu("주소로 프리팹 복제")]
        private void Clone()
        {
            PrototypeManager.ClonePrototypeAsync(
                _addressableKey,
                instance => Debug.Log(instance != null
                    ? $"Addressable 인스턴스 생성: {instance.name}"
                    : $"Addressable을 찾지 못했습니다: {_addressableKey}"),
                transform);
        }
    }
}
