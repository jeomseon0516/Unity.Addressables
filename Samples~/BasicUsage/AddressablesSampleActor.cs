using UnityEngine;

namespace Jeomseon.Samples.Addressables
{
    /// <summary>
    /// Builds a visible primitive so the sample prefab has no render-pipeline dependency.
    /// Sample Prefab이 Render Pipeline에 의존하지 않도록 보이는 Primitive를 생성합니다.
    /// </summary>
    public sealed class AddressablesSampleActor : MonoBehaviour
    {
        private void Awake()
        {
            if (transform.childCount != 0) return;
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Runtime Sample Visual";
            visual.transform.SetParent(transform, false);
        }

        private void Update() => transform.Rotate(0f, 45f * Time.deltaTime, 0f);
    }
}
