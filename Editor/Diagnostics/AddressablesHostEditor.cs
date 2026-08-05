using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Jeomseon.Addressables.Diagnostics
{
    /// <summary>
    /// Displays configuration validation and active resource ownership for AddressablesHost.
    /// AddressablesHost의 구성 검증과 활성 Resource 소유권을 표시합니다.
    /// </summary>
    [CustomEditor(typeof(AddressablesHost))]
    internal sealed class AddressablesHostEditor : UnityEditor.Editor
    {
        private bool _showResources = true;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawConfigurationWarnings();
            DrawRuntimeDiagnostics();
        }

        private void DrawConfigurationWarnings()
        {
            SerializedProperty persistent = serializedObject.FindProperty("_dontDestroyOnLoad");
            var host = (AddressablesHost)target;
            if (persistent.boolValue && host.transform.parent != null)
            {
                EditorGUILayout.HelpBox(
                    "A persistent AddressablesHost must be placed on a root GameObject. / " +
                    "영속 AddressablesHost는 Root GameObject에 배치해야 합니다.",
                    MessageType.Error);
            }
        }

        private void DrawRuntimeDiagnostics()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Diagnostics / 런타임 진단", EditorStyles.boldLabel);
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Runtime resource ownership is available in Play Mode. / " +
                    "Runtime Resource 소유권은 Play Mode에서 확인할 수 있습니다.",
                    MessageType.Info);
                return;
            }

            var host = (AddressablesHost)target;
            if (!host.TryGetCreatedService(out IAddressablesService service))
            {
                EditorGUILayout.LabelField("Service", "Not created / 생성되지 않음");
                return;
            }

            IReadOnlyList<AddressableResourceInfo> resources = service.ActiveResources;
            EditorGUILayout.LabelField(
                "Initialized / 초기화",
                service.IsInitialized ? "Yes / 예" : "No / 아니요");
            EditorGUILayout.LabelField(
                "Active Resources / 활성 Resource",
                resources.Count.ToString());

            _showResources = EditorGUILayout.Foldout(
                _showResources,
                "Owned Resources / 소유 Resource",
                true);
            if (!_showResources) return;

            if (resources.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No resources are currently owned. / 현재 소유한 Resource가 없습니다.",
                    MessageType.None);
                return;
            }

            foreach (AddressableResourceInfo info in resources)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(info.Kind.ToString(), EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Key", info.Key);
                    EditorGUILayout.LabelField("Type", info.ResourceType.FullName);
                    TimeSpan age = DateTimeOffset.UtcNow - info.CreatedAtUtc;
                    EditorGUILayout.LabelField("Age / 경과", $"{age.TotalSeconds:F1}s");
                    if (!string.IsNullOrEmpty(info.AllocationStackTrace))
                    {
                        EditorGUILayout.TextArea(
                            info.AllocationStackTrace,
                            GUILayout.MinHeight(72f));
                    }
                }
            }

            Repaint();
        }
    }
}
