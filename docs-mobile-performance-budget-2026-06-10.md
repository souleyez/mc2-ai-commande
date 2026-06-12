# G5 Mobile Performance Budget

**Status:** Done for first Android baseline.

**Scope:** This is a prototype decision gate for the current Unity 6 Android
demo. It is not a final optimization target. The first goal is to keep the
landscape phone build playable while we continue restoring real 3D assets and
combat detail.

## Budget

| Metric | Prototype Proceed Target | Investigate If Worse Than |
| --- | --- | --- |
| FPS | 30 FPS mobile target; steady battle average must stay at or above 25 FPS | steady battle average below 25 FPS |
| Launch | Record Android activity launch baseline | over 45 seconds |
| Battle load | Record first battle-ready baseline separately from FPS sample | over 15 seconds after app is open |
| Memory | Record PSS/RSS baseline | over 1.5 GB PSS on first mission |
| APK size | Keep early demo far below final art budget | over 500 MB before final art/audio |
| Thermal | No thermal throttling during short smoke | thermal status above light/no throttling |
| Battery | Record short-session note | severe drain visible in short smoke |

## Baseline Evidence

Evidence is local and ignored by git:

```text
analysis-output\android-performance-baseline.json
analysis-output\android-performance-logcat.txt
analysis-output\android-performance-meminfo.txt
analysis-output\android-performance-gfxinfo.txt
analysis-output\android-performance-battery.txt
analysis-output\android-performance-thermal.txt
```

Capture command:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\capture_android_performance_baseline.ps1 -RepoRoot . -SampleSeconds 10 -WarmupSeconds 2 -PostSampleWaitSeconds 2
```

Verification command:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_performance_budget.ps1 -RepoRoot .
```

## Mi 11 Lite Baseline

| Field | Value |
| --- | --- |
| Device | Mi 11 Lite / `M2101K9C` |
| Android | 13 |
| APK | 20,765,252 bytes / 19.80 MiB |
| Orientation | Landscape, 2400x1080 |
| Runtime target | `Application.targetFrameRate=30`, `vSync=0` |
| Capture preset | `north-patrol` battle state |
| Warmup | 2 seconds |
| FPS sample | 10.007 seconds |
| Frames | 305 |
| Average FPS | 30.48 |
| Max frame | 33.05 ms |
| Unity allocated memory | 90.31 MiB |
| Unity reserved memory | 187.45 MiB |
| Mono used | 7.82 MiB |
| Android TOTAL PSS | 273,342 KB |
| Android TOTAL RSS | 367,488 KB |
| Android TOTAL SWAP PSS | 243 KB |
| Activity launch | 260 ms from `am start -W`; logcat also reports fully drawn at 667 ms |
| Battle/performance marker upper bound | 18.07 seconds from launch command to performance marker |
| Battery | 100%, USB powered, 35.0 C |
| Thermal | `Thermal Status: 0` |

The `18.07` second marker is an upper bound for the performance capture script,
not a pure battle-load metric. It includes Unity startup, world build, the
`north-patrol` preset simulation, 2 seconds of warmup and the 10 second FPS
sample. Logcat shows the Android activity displayed in 260 ms, fully drawn in
667 ms, world built at `16:13:04.249`, and the performance sample marker at
`16:13:16.672`.

## Notes

- A no-warmup trial averaged 21.38 FPS because the sample included a 2106 ms
  cold startup frame. G5 FPS uses the warmup steady-state number; launch and
  load remain separate metrics.
- Memory is comfortably below the 1.5 GB investigation line.
- APK size is comfortably below the 500 MB investigation line.
- Thermal status stayed at 0 during the short smoke.
- `dumpsys gfxinfo` is captured for completeness, but Unity SurfaceView does
  not expose useful frame stats there on this device. The authoritative FPS
  evidence for this gate is the Unity log marker:
  `MC2 mobile performance baseline`.

## Decision

G5 passes for the current Android prototype. The next mobile planning gate is
G6 iOS feasibility. Keep the Android FPS budget at a steady 30 FPS target and
rerun this baseline after major art, terrain, unit-count, shader, or effects
changes.
