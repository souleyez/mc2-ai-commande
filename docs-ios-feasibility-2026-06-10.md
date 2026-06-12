# G6 iOS Feasibility Gate

Status: Done as a feasibility gate. This is not iOS build proof.

Date: 2026-06-12

## Stable Markers

```text
IOSFeasibilityGate: True
NotAnIOSBuildProof: True
AndroidContinues: True
MacBuildHostRequired: True
XcodeRequired: True
AppleSigningRequired: True
UnityIOSSupportRequired: True
LocalWindowsIOSBuild: Unsupported
IOSSupportInstalled: False
FirstIOSSmoke: Build Xcode project -> install on iOS device -> launch visible-flow battle
```

## Local Evidence

Current development machine:

- OS: Microsoft Windows 11 Home China, version 10.0.26200, 64-bit.
- Unity project version: `6000.4.7f1`.
- Installed Unity playback engines on this machine:
  - `AndroidPlayer`
  - `windowsstandalonesupport`
- `iOSSupport` playback engine folder on this machine: absent.

Conclusion: this Windows machine can keep proving Android and PC gameplay, but
it cannot close iOS build proof locally. iOS must be handled as a Mac/Xcode
handoff lane.

## Requirement Status

| ID | Requirement | Current State | Decision |
| --- | --- | --- | --- |
| G6-R1 | Identify macOS build machine | Not available on this Windows machine | Required for iOS proof |
| G6-R2 | Identify Xcode requirement | Xcode is not available on Windows | Record exact Xcode version on the Mac host |
| G6-R3 | Identify Apple developer/signing state | Not available in this repo and must not be committed | Required before device install |
| G6-R4 | Confirm Unity iOS module availability | `iOSSupport` is absent here | Install Unity iOS Build Support on the Mac host |
| G6-R5 | Define first iOS smoke | Defined below | Reuse the visible-flow battle path |

## First iOS Smoke Path

1. Prepare a macOS build host.
2. Install Unity `6000.4.7f1` and the iOS Build Support module.
3. Install Xcode compatible with the selected Unity editor and target iOS SDK.
4. Clone or pull this repository from `ai-origin`.
5. Build an Xcode project into ignored output, for example
   `unity-mc2-demo/Builds/iOS/`.
6. Configure bundle id, Apple team, provisioning profile and a physical iPhone
   or iPad in Xcode.
7. Install and launch on the device.
8. Drive the same visible-flow battle used by Android/PC smoke. If iOS cannot
   receive the current command-line arguments, add a small iOS-only smoke
   launch bridge that triggers the same command-file flow.
9. Capture ignored local evidence:
   - Xcode/device launch log;
   - screenshot or short clip;
   - visible-flow success markers;
   - package size and basic FPS/memory notes if available.

The first iOS smoke is accepted only when the device reaches the battle/debrief
path equivalent to the Android visible-flow command-file smoke. A successful
Mac build alone is not enough.

## Blockers To Schedule

- Mac build host.
- Unity iOS Build Support module on that Mac.
- Xcode installation and command-line tools.
- Apple developer account or team access.
- Bundle identifier decision.
- Provisioning profile and physical test device.
- Metal rendering check on the actual device.
- Package size check after iOS player output exists.

## Product Decision

Android remains the playable mobile baseline. iOS is feasible through a
separate Mac/Xcode/signing lane, and this setup must not block Android gameplay
development, PC demo polish or the next platform-contract work.

## Verification

Run from the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_ios_feasibility_gate.ps1 -RepoRoot .
```

Expected:

```text
iOS feasibility gate check OK.
```
