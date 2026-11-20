using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.Addressables.Editor.Diagnostics
{
    /// <summary>
    /// Displays configuration validation and active resource ownership for AddressablesHost.
    /// AddressablesHost의 구성 검증과 활성 Resource 소유권을 표시합니다.
    /// </summary>
    [CustomEditor(typeof(AddressablesHost))]
    internal sealed class AddressablesHostEditor : UnityEditor.Editor
    {
        // .. IMGUI 경로는 Play Mode에서 소유 Resource를 표시하는 동안 매 OnInspectorGUI에서 Repaint를
        // .. 요청합니다. UI Toolkit 경로는 같은 연속 갱신을 짧은 주기의 scheduler로 대신합니다.
        private const long RefreshIntervalMs = 100;

        private bool _showResources = true;
        private HelpBox _persistentRootError;
        private VisualElement _runtimeRoot;
        private readonly List<ResourceView> _resourceViews = new();
        private RuntimeState _runtimeState = RuntimeState.None;
        private InfoField _initializedField;
        private InfoField _activeCountField;
        private Foldout _resourcesFoldout;
        private HelpBox _emptyResourcesBox;
        private VisualElement _resourceListRoot;

        public override VisualElement CreateInspectorGUI()
        {
            _resourceViews.Clear();
            _runtimeState = RuntimeState.None;
            var root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            _persistentRootError = new HelpBox(
                "A persistent AddressablesHost must be placed on a root GameObject. / " +
                "영속 AddressablesHost는 Root GameObject에 배치해야 합니다.",
                HelpBoxMessageType.Error);
            root.Add(_persistentRootError);

            root.Add(new VisualElement { style = { height = 6f } });
            root.Add(new Label("Runtime Diagnostics / 런타임 진단")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold }
            });
            _runtimeRoot = new VisualElement();
            root.Add(_runtimeRoot);

            Refresh();
            root.TrackSerializedObjectValue(serializedObject, _ => Refresh());
            root.schedule.Execute(Refresh).Every(RefreshIntervalMs);
            return root;
        }

        private void Refresh()
        {
            if (target == null || _runtimeRoot == null) return;

            serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty persistent = serializedObject.FindProperty("dontDestroyOnLoad");
            var host = (AddressablesHost)target;
            _persistentRootError.style.display = persistent.boolValue && host.transform.parent != null
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            RefreshRuntimeDiagnostics(host);
        }

        private void RefreshRuntimeDiagnostics(AddressablesHost host)
        {
            if (!Application.isPlaying)
            {
                ShowRuntimeState(RuntimeState.NotPlaying, () => _runtimeRoot.Add(new HelpBox(
                    "Runtime resource ownership is available in Play Mode. / " +
                    "Runtime Resource 소유권은 Play Mode에서 확인할 수 있습니다.",
                    HelpBoxMessageType.Info)));
                return;
            }

            if (!host.TryGetCreatedService(out IAddressablesService service))
            {
                ShowRuntimeState(RuntimeState.NoService, () =>
                {
                    var serviceField = new InfoField("Service");
                    serviceField.SetValueWithoutNotify("Not created / 생성되지 않음");
                    _runtimeRoot.Add(serviceField);
                });
                return;
            }

            ShowRuntimeState(RuntimeState.Service, BuildServiceView);
            IReadOnlyList<AddressableResourceInfo> resources = service.ActiveResources;
            _initializedField.SetValueWithoutNotify(service.IsInitialized ? "Yes / 예" : "No / 아니요");
            _activeCountField.SetValueWithoutNotify(resources.Count.ToString());
            _emptyResourcesBox.style.display =
                resources.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            RefreshResourceViews(resources);
        }

        private enum RuntimeState
        {
            None,
            NotPlaying,
            NoService,
            Service
        }

        private void ShowRuntimeState(RuntimeState state, Action build)
        {
            if (_runtimeState == state && _runtimeRoot.childCount > 0) return;

            _runtimeState = state;
            _runtimeRoot.Clear();
            _resourceViews.Clear();
            build();
        }

        private void BuildServiceView()
        {
            _initializedField = new InfoField("Initialized / 초기화");
            _activeCountField = new InfoField("Active Resources / 활성 Resource");
            _runtimeRoot.Add(_initializedField);
            _runtimeRoot.Add(_activeCountField);

            _resourcesFoldout = new Foldout
            {
                text = "Owned Resources / 소유 Resource",
                value = _showResources
            };
            _resourcesFoldout.RegisterValueChangedCallback(change =>
            {
                if (change.target == _resourcesFoldout) _showResources = change.newValue;
            });
            _emptyResourcesBox = new HelpBox(
                "No resources are currently owned. / 현재 소유한 Resource가 없습니다.",
                HelpBoxMessageType.None);
            _resourceListRoot = new VisualElement();
            _resourcesFoldout.Add(_emptyResourcesBox);
            _resourcesFoldout.Add(_resourceListRoot);
            _runtimeRoot.Add(_resourcesFoldout);
        }

        /// <summary>
        /// 목록 구성이 바뀐 경우에만 항목 UI를 다시 만들고, 그 외에는 경과 시간만 갱신해 Stack Trace
        /// 텍스트의 선택·스크롤 상태를 유지합니다.
        /// </summary>
        private void RefreshResourceViews(IReadOnlyList<AddressableResourceInfo> resources)
        {
            bool sameResources = _resourceViews.Count == resources.Count;
            for (int i = 0; sameResources && i < resources.Count; i++)
            {
                sameResources = _resourceViews[i].Matches(resources[i]);
            }

            if (!sameResources)
            {
                _resourceListRoot.Clear();
                _resourceViews.Clear();
                foreach (AddressableResourceInfo info in resources)
                {
                    var view = new ResourceView(info);
                    _resourceViews.Add(view);
                    _resourceListRoot.Add(view.Root);
                }
            }

            foreach (ResourceView view in _resourceViews)
            {
                view.RefreshAge();
            }
        }

        private sealed class ResourceView
        {
            private readonly AddressableResourceInfo _info;
            private readonly InfoField _ageField;

            public ResourceView(AddressableResourceInfo info)
            {
                _info = info;
                // .. IMGUI 경로의 EditorStyles.helpBox 세로 묶음과 같은 테두리 상자를 사용합니다.
                Root = new VisualElement();
                Root.AddToClassList("unity-help-box");
                Root.style.flexDirection = FlexDirection.Column;
                Root.style.alignItems = Align.Stretch;
                Root.Add(new Label(info.Kind.ToString())
                {
                    style = { unityFontStyleAndWeight = FontStyle.Bold }
                });
                Root.Add(CreateInfoField("Key", info.Key));
                Root.Add(CreateInfoField("Type", info.ResourceType.FullName));
                _ageField = new InfoField("Age / 경과");
                Root.Add(_ageField);
                if (!string.IsNullOrEmpty(info.AllocationStackTrace))
                {
                    Root.Add(new TextField
                    {
                        multiline = true,
                        isReadOnly = true,
                        value = info.AllocationStackTrace,
                        style = { minHeight = 72f }
                    });
                }

                RefreshAge();
            }

            public VisualElement Root { get; }

            public bool Matches(AddressableResourceInfo info)
            {
                return _info.Kind == info.Kind &&
                       _info.Key == info.Key &&
                       _info.ResourceType == info.ResourceType &&
                       _info.CreatedAtUtc == info.CreatedAtUtc &&
                       _info.AllocationStackTrace == info.AllocationStackTrace;
            }

            public void RefreshAge()
            {
                TimeSpan age = DateTimeOffset.UtcNow - _info.CreatedAtUtc;
                _ageField.SetValueWithoutNotify($"{age.TotalSeconds:F1}s");
            }

            private static InfoField CreateInfoField(string label, string value)
            {
                var field = new InfoField(label);
                field.SetValueWithoutNotify(value);
                return field;
            }
        }

        /// <summary>IMGUI의 LabelField(label, value)와 같이 Inspector 라벨 폭에 맞춘 읽기 전용 값을 표시합니다.</summary>
        private sealed class InfoField : BaseField<string>
        {
            private readonly Label _valueLabel;

            public InfoField(string label) : this(label, new Label()) { }

            private InfoField(string label, Label valueLabel) : base(label, valueLabel)
            {
                _valueLabel = valueLabel;
                AddToClassList(alignedFieldUssClassName);
            }

            public override void SetValueWithoutNotify(string newValue)
            {
                base.SetValueWithoutNotify(newValue);
                _valueLabel.text = newValue;
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawConfigurationWarnings();
            DrawRuntimeDiagnostics();
        }

        private void DrawConfigurationWarnings()
        {
            SerializedProperty persistent = serializedObject.FindProperty("dontDestroyOnLoad");
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
