# AI RTS Commander Lab

这是一个面向 AI 副官指挥的战术 RTS 原型。项目重点不是复述旧游戏，
而是探索一种新的战斗体验：玩家负责队伍、装备、战场意图和关键决策，
AI 副官负责把这些意图翻译成可执行的战术动作，让战场像一支真正受训的
佣兵小队那样自己打起来。

核心想法很简单：你不用指导一个老兵如何战斗。玩家不应该被迫反复微操
每一台单位的每一次移动、转火和避险。AI 副官应该理解任务目标、队伍状态、
火力压力、地形威胁和撤退时机，替玩家处理战术执行层，让玩家专注于更有趣
的选择：带什么队伍、接什么任务、什么时候推进、什么时候交给 AI 托管、
什么时候亲自下达关键命令。

## 探索方向

- 用 AI 副官驱动 RTS 战斗中的高层战术决策，而不是替代本地战斗规则。
- 让本地 BattleCore 继续负责移动、射击、热量、伤害、部位损伤、任务触发
  和结算，保证战斗可预测、可测试、可复盘。
- 让 AI 负责开场计划、目标优先级、推进路线、风险判断、托管指挥和任务
  复盘建议。
- 用更少的玩家微操换取更大的战场、更复杂的任务、更真实的部队行为。
- 逐步探索可扩展地图、玩家自建战场、战绩排行、奖励认证和更大的世界。

## 当前原型

当前仓库包含一个 Unity 6 Windows 可玩原型和一组本地工具。原型聚焦两件事：

1. 改装界面：机甲、武器、装甲、散热和载重限制形成主要养成乐趣。
2. 地图战斗：固定战术视角、小队指挥、自动交战、部位损伤、战后结算和
   AI/CLI 指挥接口。

当前 AI 接入保持克制：模型只做高层计划和能力窗口，具体战斗仍交给本地
规则执行。这样可以避免延迟和不稳定性直接破坏战斗手感，也方便后续把不同
模型、服务器和玩家托管逻辑接进来。

当前产品优先级已经调整为移动端优先。Unreal MCP 暂不进入主线；Unity 6
继续作为主引擎。Android APK build smoke 已经通过；G3 真机 smoke 等待一台
已授权 USB 调试的 Android 手机。设备未到位时，PC1-PC42 已先完成当前 PC 端
受控展示包：Windows 构建、启动预检、证据健康检查、公开边界预检、
演示总预检、交接一致性检查、Android 真机 smoke 前置检查、PC 核心玩法合约检查、
移动指挥模型预检、战斗 HUD 稀疏合约检查、PC 视觉截图 sanity 检查、PC 视觉截图 sanity 自测、PC 截图 sidecar schema 检查、PC 截图 preset 契约检查、PC 截图生成物卫生检查、源码/生成物卫生检查、Android smoke 生成物卫生检查、Android smoke 截图证据捕获、Android smoke 摘要证据输出、Android smoke 摘要 schema 检查、Android smoke 摘要 preflight 检查、Android smoke 预演/前置一致性检查、Android G3 readiness 检查、Android G3 真机要求检查、AI 副官边界检查、Windows 演示构建新鲜度检查、受控演示证据新鲜度检查、capture 日志新鲜度检查、Android SDK 工具链检查、Android APK 新鲜度检查、Android APK 身份检查、Android APK 兼容性检查、Android APK 签名检查、Android APK 清单检查、Android APK 载荷检查、Android APK 包体预算检查、当前计划 gate 总预检、Android smoke 日志崩溃扫描、Android smoke 预演模式、visible-flow、截图证据、战场可读性和 MechLab 操作。
设备到位后再回到 Android 真机验证、触控指挥 UI 和移动端性能预算。

## 产品愿景

- 玩家拥有自己的机甲小队，收集武器、机体、驾驶员和资源。
- 玩家可以亲自指挥战斗，也可以把队伍委托给 AI 副官执行任务。
- 地图可以由官方、合作方或社区搭建，并通过主服务器认证奖励。
- 战斗成绩、地图贡献、队伍表现和活动排行可以在 Web 侧展示。
- 长期可以探索开放地图编辑、皮肤自定义、创作者分成和链上结算等机制。

## 内容包边界

本地开发把运行壳、战斗规则、Unity 表现和内容包分开处理。公共展示或商业
版本应使用项目自有内容包，避免把任何第三方或本地参考素材混进发布物。
内容包边界见 `docs-content-pack.md`。

公开叙事只强调本项目自己的方向：AI 辅助战术 RTS 指挥、确定性的机甲小队
战斗、可选 AI 副官、可替换内容包和未来社区地图生态。本地参考内容只用于
验证比例、节奏、任务结构和可读性；不能作为公开发布物、商业素材库、商标、
剧情文案或最终美术承诺。

发布或对外打包前按这个顺序检查：

1. 使用项目自有或合规授权的内容包。
2. 不携带本地参考素材、参考包路径、第三方商标、旧剧情文本或未清权资源。
3. 保留 `analysis-output/`、Unity `Builds/` 和本地参考导出为 ignored 开发证据。
4. 运行 public content boundary check 和受控演示公开边界预检。

## 本地开发命令

Validate the current local development pack:

```powershell
& .\scripts\content-pack\validate_content_pack.ps1 -PackPath .\mc2-run64-dev
```

Preview mounting a pack into the local runtime shell:

```powershell
& .\scripts\content-pack\mount_content_pack.ps1 -PackPath .\content-packs\project-owned-starter.example.json -RunPath .\mc2-run64-dev -DryRun
```

Preview creating a clean runtime shell and mounting a pack:

```powershell
& .\scripts\content-pack\new_runtime_shell.ps1 -ShellSourcePath .\mc2-run64-dev -OutputPath .\runtime-shell-dev -PackPath .\content-packs\project-owned-starter.example.json -DryRun
```

Preview the full start flow:

```powershell
& .\scripts\content-pack\start_runtime_shell.ps1 -DryRun -RebuildShell -Force
```

Start the local development runtime:

```powershell
& .\scripts\content-pack\start_runtime_shell.ps1
```

When `content-packs\project-owned-linked-dev` exists, the start and shortcut
scripts use it as the default development pack. Otherwise they use the local
development manifest configured for this machine.

Check the current mounted content pack:

```powershell
& .\scripts\content-pack\status_runtime_shell.ps1
```

Generate a content index:

```powershell
& .\scripts\content-pack\index_content_pack.ps1
```

Current index notes are in `docs-content-index-notes.md`.

Extract the first local mission for analysis:

```powershell
& .\scripts\content-pack\extract_mission_from_pack.ps1 -MissionId mc2_01
```

Analyze the extracted mission into JSON and Markdown summaries:

```powershell
& .\scripts\content-pack\analyze_mission_extract.ps1
```

Export a Unity-facing demo contract from that analysis:

```powershell
& .\scripts\content-pack\export_unity_demo_contract.ps1
```

Build the Unity 6 command demo:

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -batchmode -quit `
  -projectPath ".\unity-mc2-demo" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64
```

Preflight or open the controlled Windows demo:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\run_windows_demo.ps1 -CheckOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\run_windows_demo.ps1
```

Check the Windows demo build freshness:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_windows_demo_build_freshness.ps1
```

Check the current controlled-demo evidence package:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_evidence.ps1
```

Run the full controlled-demo readiness preflight:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_readiness.ps1
```

Check the controlled-demo handoff consistency:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1
```

Check demo source hygiene:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
```

Check Android smoke artifact hygiene:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_artifact_hygiene.ps1
```

Check the AI deputy contract:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_ai_deputy_contract.ps1
```

Check the PC core playable BattleCore contract:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_core_playable_contract.ps1
```

Check the mobile command model preflight:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
```

Check the sparse battle HUD contract:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_battle_hud_sparse_contract.ps1
```

Check the PC visual capture sanity gate:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_visual_capture_sanity.ps1
```

Expected success string: `PC visual capture sanity check OK`.

Self-test the PC visual capture sanity thresholds:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_visual_capture_sanity.ps1 -SelfTest
```

Expected success string: `PC visual capture sanity self-test OK`.

Check the PC capture sidecar schema:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_capture_sidecar_schema.ps1
```

Expected success string: `PC capture sidecar schema check OK`.

Check the PC capture preset contract:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_capture_preset_contract.ps1
```

Expected standard preset list: `mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol`.
Expected success string: `PC capture preset contract check OK`.

Check the PC capture artifact hygiene:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_capture_artifact_hygiene.ps1
```

Expected success string: `PC capture artifact hygiene check OK`.

Check the current plan gate:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
```

Check the Android SDK tooling before device smoke:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_sdk_tooling.ps1
```

Check the Android APK compatibility before device smoke:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_compatibility.ps1
```

Check the Android APK signing before device smoke:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_signing.ps1
```

Check the Android APK manifest before device smoke:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_manifest.ps1
```

Check the Android APK payload before device smoke:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_payload.ps1
```

Check the Android APK size budget before device smoke:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_size_budget.ps1
```

Check the Android smoke log scanner:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_log.ps1 -SelfTest
```

Check the Android smoke summary schema:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_summary.ps1 -SelfTest
```

Expected success string: `Android smoke summary check self-test OK`.

Preview the Android device smoke plan without a device:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
```

The plan output should include the ignored screenshot target
`analysis-output\android-device-smoke.png`, the ignored summary target
`analysis-output\android-device-smoke-summary.json`, `ScreenshotCapture: True`
and `SummaryWrite: True`.

Check Android device-smoke readiness:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
```

The waiting-state output should include `smoke summary schema` and
`Android smoke summary check self-test OK`.

Check Android smoke plan/preflight consistency:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_plan_consistency.ps1
```

Expected success string: `Android smoke plan/preflight consistency check OK`.

Check Android G3 readiness without installing:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_readiness.ps1
```

Expected waiting-state string without a phone: `Android G3 readiness check waiting on device`.

Check the strict Android G3 device requirement:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_device_requirement.ps1
```

Expected waiting-state string without a phone: `Android G3 device requirement check waiting on device`.

Check the current controlled-demo public boundary metadata package:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\content-pack\check_controlled_demo_public_boundary.ps1
```

Optionally confirm the current local Windows development build is still blocked
from public packaging:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\content-pack\check_controlled_demo_public_boundary.ps1 -CheckDevBuild
```

Install a desktop shortcut:

```powershell
& .\scripts\content-pack\install_dev_shortcut.ps1
```

Preview a new replacement pack scaffold:

```powershell
& .\scripts\content-pack\new_content_pack.ps1 -PackId project-owned-dev -Title "Project Owned Dev" -DryRun
```

Check a build or package path before public packaging:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\content-pack\check_public_content_boundary.ps1 -Path ".\unity-mc2-demo\Builds\Windows" -DryRun
```

The current local development build is expected to fail this check until a clean
public pack and public build name are in place. A clean public build should
return `Result: OK`; a development build with private reference markers returns
`Result: FAILED` and lists the matching paths or lines.

The first clean text metadata target is
`content-packs/project-owned-text-safe-slice.example.json`. It contains
project-owned product, mission, unit, weapon, pilot, objective and UI names and
passes the public boundary check, but it is not a mountable runtime pack yet.
The first clean visual id metadata target is
`content-packs/project-owned-visual-slice.example.json`. It defines project-owned
unit, terrain, prop, weapon FX, damage FX and UI art ids for future cleared
assets, and also passes the public boundary check.
The first combined art-safe metadata target is
`content-packs/project-owned-art-safe-slice.example.json`. It merges clean text,
stable visual ids, planned cleared asset paths and provenance placeholders for
one mission slice. It is still metadata-only, not a mountable runtime pack.

## 关键文档

- `docs-ai-rts-commander-current-master-plan-2026-06-07.md`: 当前主计划书，梳理现阶段、详细提交队列、视觉/碰撞回归、私有参考视觉边界、公开替换包、AI 副官和后续平台路线。
- `docs-ai-rts-commander-current-detailed-plan-2026-06-07.md`: 当前细计划书，记录下一步提交队列、B/C/D/F 阶段状态、验证命令和停止条件。
- `docs-machine-handoff-plan-2026-06-07.md`: 换机开发交接计划，覆盖旧机推送、新机克隆、Unity 6000.4.7f1 校验、visible-flow smoke、可选私有参考视觉和 AI key 恢复边界。
- `docs-mobile-first-plan-2026-06-10.md`: 移动端优先计划，覆盖 Android build smoke、真机验证、触控 UI、性能预算和 iOS 可行性边界。
- `docs-pc-optimization-plan-2026-06-11.md`: PC 端优化计划，说明 Android 真机等待期间如何继续打磨 Windows 可展示流程、视觉证据和 MechLab。
- `docs-ai-rts-commander-v1-detailed-execution-plan-2026-06-07.md`: 上一版细计划书，保留 MechLab、装配影响战斗、损伤表现、公开替换包、AI 回归守护和平台契约的任务细节。
- `docs-ai-rts-commander-detailed-roadmap-2026-06-07.md`: 当前细化路线图，覆盖真实进度、V4 进行中工作、提交级任务、验收命令、停止条件和后续平台路线。
- `docs-ai-rts-commander-overall-implementation-plan-2026-06-07.md`: 当前定格版整体计划书，覆盖产品方向、架构边界、里程碑和总路线，并指向细化路线图。
- `docs-playable-demo-current-execution-plan-2026-06-07.md`: 旧的日常执行入口，保留当前仓库历史上下文、已完成任务和早期 `Current Commit Queue`。
- `docs-playable-demo-v1-detailed-plan-2026-06-07.md`: V1 Demo 细化计划，保留产品边界、架构边界、提交级任务、验收门和后续路线。
- `docs-playable-demo-walkthrough-2026-06-07.md`: 当前三分钟可玩 Demo 演示脚本，覆盖机库装配、战场指挥、喷射、损伤、战报、维修和后续方向。
- `docs-playable-demo-investor-evidence-2026-06-07.md`: 当前可玩 Demo 证据页，列出本地 ignored 截图、sidecar 摘要、三分钟展示顺序和公开内容边界。
- `docs-playable-demo-overall-detailed-plan-2026-06-07.md`: 总计划、产品分层、第一版工作包、阶段门、长期边界和历史上下文。
- `docs-playable-demo-fine-grained-current-plan-2026-06-07.md`: 旧的细粒度执行计划，保留阶段历史。
- `docs-playable-demo-locked-execution-plan-2026-06-07.md`: 旧的锁定执行计划，保留阶段历史、已完成任务和早期 Sprint Board。
- `docs-playable-demo-completion-plan-2026-06-07.md`: 阶段摘要计划，保留阶段清单、验证矩阵和最近提交队列。
- `docs-playable-demo-detailed-roadmap-2026-06-07.md`: 旧的细化路线图，保留历史上下文。
- `docs-playable-demo-current-detailed-plan-2026-06-07.md`: 旧的当前计划，保留历史上下文和已完成任务证据。
- `docs-playable-demo-detailed-execution-plan-2026-06-07.md`: 旧的阶段执行计划，保留历史上下文和早期任务拆解。
- `docs-mc2-ai-commander-demo-execution-plan.md`: 早期 AI 指挥 Demo 执行计划。
- `docs-ai-commander-directive-contract.md`: AI 副官高层指令边界。
- `docs-platform-ecosystem-plan.md`: 地图服务器、奖励认证、排行和创作者生态设想。
- `docs-platform-reward-contract-2026-06-07.md`: 主服务器奖励权威契约，说明地图服务器和客户端只提交 claim，最终 grant、库存、token ledger 和排行由主服务器认证。
- `docs-content-pack.md`: 可替换内容包边界。
- `unity-mc2-demo/README.md`: Unity 原型行为、构建和 smoke 命令。
- `BUILD-MOBILE.md`: Android build smoke、Android 真机 smoke 和 iOS 后置原则。

## 许可与发布提醒

仓库保留历史代码和第三方依赖的许可文件。发布、融资演示或商业化版本需要
逐项确认代码、素材、文字、音频、模型、商标和数据来源，优先使用项目自有
内容包。

## Windows / Linux

Windows 构建细节见 `BUILD-WIN.md`。当前重点开发目标是 Unity 6 Windows
可玩原型；Linux 侧保留为后续工程兼容方向。

