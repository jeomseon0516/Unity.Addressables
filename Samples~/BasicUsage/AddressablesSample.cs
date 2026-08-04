using System;
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
        [SerializeField] private string _prefabKey = "jeomseon-addressables-sample-prefab";
        [SerializeField] private string _messageKey = "jeomseon-addressables-sample-message";
        private AddressableInstanceHandle _instance;
        private AddressableAssetLease<TextAsset> _message;
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
            if (GUILayout.Button("Instantiate Prefab / Prefab 생성")) InstantiatePrefab();
            if (GUILayout.Button("Release Instance / 정상 해제")) ReleasePrefab();
            if (GUILayout.Button("Destroy Externally / 외부 Destroy")) DestroyExternally();
            GUILayout.Space(8f);
            if (GUILayout.Button("Load TextAsset Lease / TextAsset Lease 로드")) LoadMessage();
            if (GUILayout.Button("Dispose TextAsset Lease / TextAsset Lease 해제")) ReleaseMessage();
            GUILayout.Space(12f);
            GUILayout.Label($"Active resources / 활성 Resource: {ActiveResourceCount}");
            GUILayout.Label(_status);
            GUILayout.EndArea();
        }

        private int ActiveResourceCount => _host?.Service.ActiveResourceCount ?? 0;

        private async void InstantiatePrefab()
        {
            if (_host == null) return;
            ReleasePrefab();
            try
            {
                _instance = await _host.Service.InstantiateAsync(
                    _prefabKey,
                    null,
                    destroyCancellationToken);
                _instance.Instance.transform.position = Vector3.zero;
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

        private void ReleasePrefab()
        {
            _instance?.Dispose();
            _instance = null;
            _status = "Instance released through its handle. / Handle로 Instance를 해제했습니다.";
        }

        private void DestroyExternally()
        {
            if (_instance?.Instance == null) return;
            Destroy(_instance.Instance);
            _status = "External Destroy requested; the observer releases its operation. / " +
                "외부 Destroy를 요청했으며 Observer가 Operation을 해제합니다.";
        }

        private async void LoadMessage()
        {
            if (_host == null) return;
            ReleaseMessage();
            try
            {
                _message = await _host.Service.LoadAssetAsync<TextAsset>(
                    _messageKey,
                    destroyCancellationToken);
                _status = _message.Asset.text;
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
            _message?.Dispose();
            _message = null;
            _status = "TextAsset lease disposed. / TextAsset Lease를 해제했습니다.";
        }

        private void OnDestroy()
        {
            ReleasePrefab();
            ReleaseMessage();
        }
    }
}
