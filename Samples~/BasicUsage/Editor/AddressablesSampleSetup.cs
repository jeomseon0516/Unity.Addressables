using System;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace Jeomseon.Samples.Addressables.Editor
{
    /// <summary>
    /// Registers imported sample assets with stable Addressables keys.
    /// Import된 Sample Asset을 고정된 Addressables Key로 등록합니다.
    /// </summary>
    internal static class AddressablesSampleSetup
    {
        private const string GroupName = "Jeomseon Addressables Basic Usage";
        private const string PrefabKey = "jeomseon-addressables-sample-prefab";
        private const string MessageKey = "jeomseon-addressables-sample-message";
        private const string SampleLabel = "jeomseon-addressables-sample";

        [MenuItem("Jeomseon/Addressables/Setup Basic Usage Sample")]
        private static void Setup()
        {
            AddressableAssetSettings settings =
                AddressableAssetSettingsDefaultObject.GetSettings(true);
            AddressableAssetGroup group = settings.FindGroup(GroupName) ??
                settings.CreateGroup(
                    GroupName,
                    false,
                    false,
                    true,
                    null,
                    typeof(BundledAssetGroupSchema),
                    typeof(ContentUpdateGroupSchema));

            Register(settings, group, "AddressablesSamplePrefab", "t:Prefab", PrefabKey);
            Register(settings, group, "SampleMessage", "t:TextAsset", MessageKey);
            settings.SetDirty(
                AddressableAssetSettings.ModificationEvent.EntryModified,
                group,
                true,
                true);
            AssetDatabase.SaveAssets();
            Debug.Log("[PASS] Addressables Basic Usage sample assets registered. / " +
                "Addressables Basic Usage Sample Asset 등록을 완료했습니다.");
        }

        private static void Register(
            AddressableAssetSettings settings,
            AddressableAssetGroup group,
            string assetName,
            string filter,
            string address)
        {
            string guid = AssetDatabase.FindAssets($"{assetName} {filter}")
                .FirstOrDefault(candidate => AssetDatabase.GUIDToAssetPath(candidate)
                    .Contains("Jeomseon Unity Addressables", StringComparison.Ordinal));
            if (string.IsNullOrEmpty(guid))
            {
                throw new InvalidOperationException($"Sample asset not found: {assetName}");
            }

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = address;
            settings.AddLabel(SampleLabel);
            entry.SetLabel(SampleLabel, true, true);
        }
    }
}
