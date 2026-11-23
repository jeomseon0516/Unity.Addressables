# Jeomseon Unity Addressables

[English](Overview.en.md) · [설치](../README.md)

## 목적

Unity Addressables 위에 서비스 수명, 리소스 추적, asset Lease와 prefab InstanceHandle의 해제 계약을 제공합니다.

## 사용 흐름

Host 또는 직접 생성한 Service → Load/Instantiate → Lease/Handle을 보관하며 사용 → Dispose. Host 종료는 Service의 남은 리소스도 정리합니다.

## 계약과 제한

취소가 실제 I/O를 즉시 멈추지는 않습니다. Retain도 원래 Service의 종료를 넘어서지 않습니다. 자동 owner 수명은 별도 Addressables Ownership 패키지로 연결합니다.

## 패키지 요구 사항

- Package ID: `com.jeomseon.unity.addressables`
- Version: `0.4.0`
- Unity: `6000.6` / `0f1`
- `com.unity.addressables`: `2.9.1`

## 샘플과 검증

Package Manager > Samples에서 다음 예제를 Import하세요. 포함된 Scene 또는 Editor 도구를 열고 샘플 README의 Setup 절차를 먼저 수행합니다.

- [Basic Usage](../Samples~/BasicUsage/README.md)
  - [AddressablesBasicUsage](../Samples~/BasicUsage/Scenes/AddressablesBasicUsage.unity)

[테스트 코드](../Tests)

소비 프로젝트 manifest의 `testables`에 패키지 ID를 추가하고 Unity Test Runner의 관련 어셈블리를 실행합니다. 위 링크는 테스트 위치이며 이번 문서 수정으로 새 테스트 실행을 주장하지 않습니다.

## 코드 지도

- [AddressableAssetCollectionLease](../Runtime/AddressableAssetCollectionLease.cs)
- [AddressableAssetLease](../Runtime/AddressableAssetLease.cs)
- [AddressableInstanceHandle](../Runtime/AddressableInstanceHandle.cs)
- [AddressableInstanceReleasePolicy](../Runtime/AddressableInstanceReleasePolicy.cs)
- [AddressableResourceInfo](../Runtime/AddressableResourceInfo.cs)
- [AddressableResourceKind](../Runtime/AddressableResourceKind.cs)
- [AddressablesConfiguration](../Runtime/AddressablesConfiguration.cs)
- [AddressablesHost](../Runtime/AddressablesHost.cs)
- [AddressablesService](../Runtime/AddressablesService.cs)
- [IAddressablesService](../Runtime/IAddressablesService.cs)

[상세 설계](Design.md)
