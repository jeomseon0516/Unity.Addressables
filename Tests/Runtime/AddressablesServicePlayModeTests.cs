using System;
using System.Collections;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.TestTools;

namespace Jeomseon.Tests
{
    public sealed class AddressablesServicePlayModeTests
    {
        private const string PrefabKey = "jeomseon-addressables-sample-prefab";
        private const string MessageKey = "jeomseon-addressables-sample-message";

        [UnityTest]
        public IEnumerator InitializeAsync_CoalescesRepeatedInitialization()
        {
            async Awaitable TestImplementation()
            {
                using var service = new Jeomseon.Unity.Addressables.AddressablesService();
                await service.InitializeAsync();
                await service.InitializeAsync();
            }

            return TestImplementation();
        }

        [UnityTest]
        public IEnumerator LoadAssetAsync_RejectsMissingKey()
        {
            async Awaitable TestImplementation()
            {
                using var service = new Jeomseon.Unity.Addressables.AddressablesService();
                try
                {
                    await service.LoadAssetAsync<Material>(null);
                    Assert.Fail("A missing Addressables key must be rejected.");
                }
                catch (ArgumentException)
                {
                }
            }

            return TestImplementation();
        }

        [UnityTest]
        public IEnumerator LoadAssetReferenceAsync_RejectsInvalidSerializedReference()
        {
            async Awaitable TestImplementation()
            {
                using var service = new Jeomseon.Unity.Addressables.AddressablesService();
                var reference = new AssetReferenceT<Material>(string.Empty);
                try
                {
                    await service.LoadAssetReferenceAsync(reference);
                    Assert.Fail("An invalid serialized AssetReference must be rejected.");
                }
                catch (ArgumentException)
                {
                }
            }

            return TestImplementation();
        }

        [UnityTest]
        public IEnumerator LoadLabelAssetsAsync_RejectsInvalidSerializedLabel()
        {
            async Awaitable TestImplementation()
            {
                using var service = new Jeomseon.Unity.Addressables.AddressablesService();
                var label = new AssetLabelReference();
                try
                {
                    await service.LoadLabelAssetsAsync<TextAsset>(label);
                    Assert.Fail("An invalid serialized label must be rejected.");
                }
                catch (ArgumentException)
                {
                }
            }

            return TestImplementation();
        }

        [UnityTest]
        public IEnumerator InstantiateReferenceAsync_RejectsInvalidSerializedReference()
        {
            async Awaitable TestImplementation()
            {
                using var service = new Jeomseon.Unity.Addressables.AddressablesService();
                var reference = new AssetReferenceGameObject(string.Empty);
                try
                {
                    await service.InstantiateReferenceAsync(reference);
                    Assert.Fail("An invalid serialized prefab reference must be rejected.");
                }
                catch (ArgumentException)
                {
                }
            }

            return TestImplementation();
        }

        [UnityTest]
        public IEnumerator InitializeAsync_RejectsDisposedService()
        {
            async Awaitable TestImplementation()
            {
                var service = new Jeomseon.Unity.Addressables.AddressablesService();
                service.Dispose();
                try
                {
                    await service.InitializeAsync();
                    Assert.Fail("A disposed Addressables service must not initialize.");
                }
                catch (ObjectDisposedException)
                {
                }
            }

            return TestImplementation();
        }

        [UnityTest]
        public IEnumerator LoadAssetAsync_TracksAndReleasesLease()
        {
            async Awaitable TestImplementation()
            {
                using var service = new Jeomseon.Unity.Addressables.AddressablesService();
                var lease = await service.LoadAssetAsync<TextAsset>(MessageKey);

                Assert.That(lease.IsValid, Is.True);
                Assert.That(lease.Asset, Is.Not.Null);
                Assert.That(service.ActiveResourceCount, Is.EqualTo(1));
                Assert.That(service.ActiveResources.Single().Kind,
                    Is.EqualTo(Jeomseon.Unity.Addressables.AddressableResourceKind.Asset));
                Assert.That(service.ActiveResources.Single().Key, Is.EqualTo(MessageKey));

                lease.Dispose();
                lease.Dispose();
                Assert.That(lease.IsValid, Is.False);
                Assert.That(service.ActiveResourceCount, Is.Zero);
            }

            return TestImplementation();
        }

        [UnityTest]
        public IEnumerator LoadAssetAsync_CapturesAllocationStackWhenConfigured()
        {
            async Awaitable TestImplementation()
            {
                var configuration = ScriptableObject.CreateInstance<
                    Jeomseon.Unity.Addressables.AddressablesConfiguration>();
                JsonUtility.FromJsonOverwrite(
                    "{\"_captureAllocationStackTrace\":true," +
                    "\"_logOutstandingResourcesOnDispose\":false}",
                    configuration);
                using var service = new Jeomseon.Unity.Addressables.AddressablesService(configuration);
                using var lease = await service.LoadAssetAsync<TextAsset>(MessageKey);

                Assert.That(
                    service.ActiveResources.Single().AllocationStackTrace,
                    Is.Not.Empty);
                UnityEngine.Object.DestroyImmediate(configuration);
            }

            return TestImplementation();
        }

        [UnityTest]
        public IEnumerator LoadAssetsAsync_TracksAndReleasesCollectionLease()
        {
            async Awaitable TestImplementation()
            {
                using var service = new Jeomseon.Unity.Addressables.AddressablesService();
                var lease = await service.LoadAssetsAsync<TextAsset>(MessageKey);

                Assert.That(lease.Assets, Has.Count.EqualTo(1));
                Assert.That(service.ActiveResources.Single().Kind,
                    Is.EqualTo(Jeomseon.Unity.Addressables.AddressableResourceKind.AssetCollection));

                lease.Dispose();
                Assert.That(service.ActiveResourceCount, Is.Zero);
            }

            return TestImplementation();
        }

        [UnityTest]
        public IEnumerator InstantiateAsync_ExternalDestroyReleasesOwnership()
        {
            async Awaitable TestImplementation()
            {
                using var service = new Jeomseon.Unity.Addressables.AddressablesService();
                var handle = await service.InstantiateAsync(PrefabKey);
                Assert.That(service.ActiveResourceCount, Is.EqualTo(1));

                UnityEngine.Object.Destroy(handle.Instance);
                await Awaitable.NextFrameAsync();

                Assert.That(handle.IsValid, Is.False);
                Assert.That(service.ActiveResourceCount, Is.Zero);
            }

            return TestImplementation();
        }

        [UnityTest]
        public IEnumerator Dispose_ReleasesOutstandingResources()
        {
            async Awaitable TestImplementation()
            {
                var configuration = ScriptableObject.CreateInstance<
                    Jeomseon.Unity.Addressables.AddressablesConfiguration>();
                JsonUtility.FromJsonOverwrite(
                    "{\"_logOutstandingResourcesOnDispose\":false}",
                    configuration);
                var service = new Jeomseon.Unity.Addressables.AddressablesService(configuration);
                var lease = await service.LoadAssetAsync<TextAsset>(MessageKey);

                service.Dispose();

                Assert.That(lease.IsValid, Is.False);
                Assert.That(service.ActiveResourceCount, Is.Zero);
                UnityEngine.Object.DestroyImmediate(configuration);
            }

            return TestImplementation();
        }

        [UnityTest]
        public IEnumerator LoadAssetAsync_CanceledBeforeStartDoesNotOwnResource()
        {
            async Awaitable TestImplementation()
            {
                using var service = new Jeomseon.Unity.Addressables.AddressablesService();
                using var cancellation = new CancellationTokenSource();
                cancellation.Cancel();

                try
                {
                    await service.LoadAssetAsync<TextAsset>(MessageKey, cancellation.Token);
                    Assert.Fail("A pre-canceled load must throw.");
                }
                catch (OperationCanceledException)
                {
                }

                Assert.That(service.ActiveResourceCount, Is.Zero);
            }

            return TestImplementation();
        }

        [UnityTest]
        public IEnumerator InitializeAsync_WithCatalogUpdatePolicy_CompletesWithoutUpdates()
        {
            async Awaitable TestImplementation()
            {
                var configuration = ScriptableObject.CreateInstance<
                    Jeomseon.Unity.Addressables.AddressablesConfiguration>();
                JsonUtility.FromJsonOverwrite(
                    "{\"_updateCatalogOnInitialize\":true," +
                    "\"_cleanBundleCacheAfterCatalogUpdate\":true}",
                    configuration);
                using var service = new Jeomseon.Unity.Addressables.AddressablesService(configuration);

                await service.InitializeAsync();
                await service.InitializeAsync();

                Assert.That(service.IsInitialized, Is.True);
                Assert.That(service.ActiveResourceCount, Is.Zero);
                UnityEngine.Object.DestroyImmediate(configuration);
            }

            return TestImplementation();
        }
    }
}
