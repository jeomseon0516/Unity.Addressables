using System;
using System.Collections.Generic;
using Jeomseon.Addressables;
using UnityEngine;

namespace Jeomseon.Samples.Addressables
{
    /// <summary>
    /// Demonstrates prefab and general-asset ownership through an Addressables Host.
    /// Addressables Host를 통한 Prefab 및 일반 Asset 소유권을 보여줍니다.
    /// </summary>
    public sealed class AddressablesSample : MonoBehaviour
    {
        [SerializeField] private AddressablesHost _host;
        [SerializeField] private AddressablesSampleAssets _assets;
        private IAddressablesSampleContentProvider _provider;
        private readonly List<GameObject> _instances = new();
        private TextAsset _message;
        private string _status = "Run the sample setup menu, then use the buttons below.\n" +
            "Sample Setup 메뉴 실행 후 아래 버튼을 사용하세요.";

        private void Awake()
        {
            if (Camera.main != null) return;
            var cameraObject = new GameObject("Sample Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.transform.SetPositionAndRotation(
                new Vector3(0f, 1.5f, -6f),
                Quaternion.identity);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.1f, 0.14f);
        }

        private void OnGUI()
        {
            const float width = 420f;
            GUILayout.BeginArea(new Rect(20f, 20f, width, 310f), GUI.skin.box);
            GUILayout.Label("Jeomseon Addressables — Basic Usage");
            GUILayout.Space(8f);
            if (GUILayout.Button("Instantiate Another Prefab / Prefab 추가 생성")) InstantiatePrefab();
            if (GUILayout.Button("Release Last Instance / 마지막 Instance 해제")) ReleaseLastPrefab();
            if (GUILayout.Button("Release All Instances / 모든 Instance 해제")) ReleaseAllPrefabs();
            if (GUILayout.Button("Destroy Last Externally / 마지막 Instance 외부 Destroy")) DestroyExternally();
            GUILayout.Space(8f);
            if (GUILayout.Button("Load TextAsset Lease / TextAsset Lease 로드")) LoadMessage();
            if (GUILayout.Button("Dispose TextAsset Lease / TextAsset Lease 해제")) ReleaseMessage();
            GUILayout.Space(12f);
            GUILayout.Label($"Active resources / 활성 Resource: {ActiveResourceCount}");
            GUILayout.Label($"Provider actors / Provider Actor: {_provider?.OwnedActorCount ?? 0}");
            GUILayout.Label(_status);
            GUILayout.EndArea();
        }

        private int ActiveResourceCount => _host?.Service.ActiveResourceCount ?? 0;

        private IAddressablesSampleContentProvider Provider =>
            _provider ??= new AddressablesSampleContentProvider(_host.Service, _assets);

        private async void InstantiatePrefab()
        {
            if (_host == null) return;
            try
            {
                GameObject instance = await Provider.InstantiateActorAsync(
                    null,
                    destroyCancellationToken);
                instance.transform.position = Vector3.right * _instances.Count * 1.5f;
                _instances.Add(instance);
                _status = "Prefab instance created. / Prefab Instance를 생성했습니다.";
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                _status = exception.Message;
                Debug.LogException(exception, this);
            }
        }

        private void ReleaseLastPrefab()
        {
            if (_instances.Count == 0) return;
            int index = _instances.Count - 1;
            GameObject instance = _instances[index];
            _instances.RemoveAt(index);
            _provider?.ReleaseActor(instance);
            _status = "Instance released through its handle. / Handle로 Instance를 해제했습니다.";
        }

        private void ReleaseAllPrefabs()
        {
            _provider?.ReleaseAllActors();
            _instances.Clear();
            _status = "All instances released. / 모든 Instance를 해제했습니다.";
        }

        private void DestroyExternally()
        {
            if (_instances.Count == 0) return;
            int index = _instances.Count - 1;
            GameObject instance = _instances[index];
            _instances.RemoveAt(index);
            Destroy(instance);
            _status = "External Destroy requested; the observer releases its operation. / " +
                "외부 Destroy를 요청했으며 Observer가 Operation을 해제합니다.";
        }

        private async void LoadMessage()
        {
            if (_host == null) return;
            ReleaseMessage();
            try
            {
                _message = await Provider.LoadMessageAsync(destroyCancellationToken);
                _status = _message.text;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                _status = exception.Message;
                Debug.LogException(exception, this);
            }
        }

        private void ReleaseMessage()
        {
            _provider?.ReleaseMessage();
            _message = null;
            _status = "TextAsset lease disposed. / TextAsset Lease를 해제했습니다.";
        }

        private void OnDestroy()
        {
            _provider?.Dispose();
            _provider = null;
        }
    }
}
