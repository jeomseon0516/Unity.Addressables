using System;

namespace Jeomseon.Addressables
{
    /// <summary>
    /// Describes one resource currently owned by an Addressables service.
    /// Addressables Service가 현재 소유하는 Resource 하나를 설명합니다.
    /// </summary>
    public readonly struct AddressableResourceInfo
    {
        /// <summary>Gets the ownership category. 소유권 종류를 가져옵니다.</summary>
        public AddressableResourceKind Kind { get; }

        /// <summary>Gets the diagnostic key. 진단용 Key를 가져옵니다.</summary>
        public string Key { get; }

        /// <summary>Gets the loaded or instantiated type. 로드 또는 생성 타입을 가져옵니다.</summary>
        public Type ResourceType { get; }

        /// <summary>Gets the UTC allocation time. UTC 할당 시각을 가져옵니다.</summary>
        public DateTimeOffset CreatedAtUtc { get; }

        /// <summary>Gets the optional allocation stack trace. 선택적 할당 StackTrace를 가져옵니다.</summary>
        public string AllocationStackTrace { get; }

        internal AddressableResourceInfo(
            AddressableResourceKind kind,
            object key,
            Type resourceType,
            bool captureAllocationStackTrace)
        {
            Kind = kind;
            Key = key?.ToString() ?? "<null>";
            ResourceType = resourceType ?? typeof(object);
            CreatedAtUtc = DateTimeOffset.UtcNow;
            AllocationStackTrace = captureAllocationStackTrace
                ? Environment.StackTrace
                : string.Empty;
        }
    }
}
