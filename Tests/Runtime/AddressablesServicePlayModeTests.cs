using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Jeomseon.Tests
{
    public sealed class AddressablesServicePlayModeTests
    {
        [UnityTest]
        public IEnumerator InitializeAsync_CoalescesRepeatedInitialization()
        {
            async Awaitable TestImplementation()
            {
                using var service = new Jeomseon.Addressables.AddressablesService();
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
                using var service = new Jeomseon.Addressables.AddressablesService();
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
        public IEnumerator InitializeAsync_RejectsDisposedService()
        {
            async Awaitable TestImplementation()
            {
                var service = new Jeomseon.Addressables.AddressablesService();
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
    }
}
