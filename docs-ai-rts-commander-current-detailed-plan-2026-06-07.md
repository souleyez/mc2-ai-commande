# AI RTS Commander Current Detailed Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use executing-plans to implement this plan task-by-task.

**Goal:** 把当前 Unity 6 原型推进到 PC 可展示、移动端可迁移的一版 AI 副官机甲战术 RTS Demo：Windows 继续作为开发验证和投资演示环境；Android/iOS 可行性、触控 UI 和移动端性能仍是产品优先级，但 G3 真机验证等待设备期间先执行 PC 端优化。

**Architecture:** `BattleCore` 继续做确定性规则核心，负责移动、喷射、占位、武器、热量、装甲、部位损伤、任务触发、结算和 AI observation/directive。Unity 6 只负责固定镜头、输入、稀疏 HUD、MechLab、模型材质、特效、截图和 smoke 证据。开发期可以使用本机私有参考素材验证比例和节奏，但公开版本必须切换到项目自有或合规授权内容包。

**Tech Stack:** Unity 6, C#, Windows PC build/smoke/capture as active demo loop, Android/iOS mobile-first after device blocker clears, deterministic BattleCore, PowerShell build/smoke/capture scripts, replaceable content packs, optional high-level AI deputy, later main server/map server/Web ranking contracts.

**Revision:** 2026-06-13 v129. This file is the fine-grained execution plan paired with `docs-ai-rts-commander-current-master-plan-2026-06-07.md`. The private reference visual bridge, local investor evidence package, art-safe metadata contract, AI deputy offline guard, reward authority contract, machine handoff plan, mobile-first priority reset, Android build smoke, PC optimization resumption, PC1 baseline audit, PC2 battle readability pass, PC3 MechLab PC flow polish, PC4 controlled demo evidence package, PC5 Windows demo launcher preflight, PC6 controlled demo evidence health check, PC7 controlled demo public boundary preflight, PC8 controlled demo readiness preflight, PC9 controlled demo handoff consistency check, PC10 Android device-smoke preflight, PC11 PC core playable contract check, PC12 mobile command model preflight, PC13 current plan gate check, PC14 Android smoke log crash scan, PC15 Android smoke plan mode, PC16 battle HUD sparse contract check, PC17 demo source hygiene check, PC18 AI deputy contract check, PC19 Windows demo build freshness check, PC20 controlled demo evidence freshness check, PC21 controlled demo capture log freshness check, PC22 Android APK freshness check, PC23 Android APK identity check, PC24 Android APK compatibility check, PC25 Android APK signing check, PC26 Android APK manifest check, PC27 Android APK payload check, PC28 Android APK size budget check, PC29 Android SDK tooling check, PC30 Android smoke artifact hygiene check, PC31 Android smoke screenshot evidence capture, PC32 Android smoke summary evidence output, PC33 Android smoke summary schema check, PC34 Android smoke summary preflight check, PC35 Android smoke plan/preflight consistency check, PC36 Android G3 readiness check, PC37 Android G3 device requirement check, PC38 PC visual capture sanity check, PC39 PC visual capture sanity self-test, PC40 PC capture sidecar schema check, PC41 PC capture preset contract check, PC42 PC capture artifact hygiene check, PC43 PC window contract check, PC44 PC launch log hygiene check, PC45 PC build artifact hygiene check, PC46 PC smoke artifact hygiene check, PC47 current plan queue consistency check, PC48 Android device connection check, PC49 Android smoke connection gate wiring, PC50 Android smoke connection gate check, PC51 Android visible-flow command-file smoke, PC52 Android WPD-only device diagnosis, PC53 Android ADB setup guidance, PC54 Android ADB readiness watch, PC55 Android G3 device status report, PC56 Android G3 when-ready runner, PC57 Android ADB driver package probe, `Pass Android G3 device smoke`, the landscape `G4 Touch UI pass`, `G5 Mobile performance budget`, `G6 iOS feasibility gate`, `F2 map authoring contract`, `F3 web ranking contract`, `F4 creator economy boundary`, `F5 server implementation boundary`, `F6 local main-server prototype`, `F7 document Unity main-server integration contract`, `F8 implement optional Unity main-server client adapter`, `F9 wire optional Unity main-server adapter into launch/debrief smoke`, `F10 wire optional Unity inventory bootstrap smoke`, and `F11 plan inventory-to-MechBay binding boundary` are now sealed for the current Demo. H2 validator/build/smoke is green; G2 Android build smoke is green with a generated APK; `F12 implement opt-in inventory-to-MechBay preview binding` through `F69 implement post-F68 PC controlled-demo investor route evidence refresh audit fixes` are complete; `F70 refresh PC controlled-demo investor route evidence after F68 audit fixes` is the formal next task. Mobile phones remain first-version landscape-only as 手机端横版 / horizontal phone build; portrait is not a first-slice support target.

---

## 0. Current Snapshot

当前项目不是从零开始。已经完成的基础包括：

| Area | Status | Evidence |
| --- | --- | --- |
| Windows 本地构建 | 已可 batch build | `analysis-output/unity-build-*.log` 多轮成功 |
| 任务 Demo | `mc2_01` 可加载地形、单位、结构、目标、触发 | Unity validator 通过 |
| 指挥操作 | 默认全队、状态栏单选、独立命令、自动归队、喷射、集火 | visible-flow smoke 通过 |
| MechLab | 整块武器占格、装甲板、散热器、热量、重量、合法性 | `mechlab` capture sidecar |
| 装配影响战斗 | 合法装配改动能进入 BattleCore combat fields | loadout battle effect smoke |
| 部位损伤 | 断臂、腿瘫、驾驶舱损毁/弹射、残骸故事已有 | `damage-demo` sidecar |
| 稀疏战斗 UI | 大日志、存档、账号、调试覆盖层已被 sidecar 守护 | `SparseBattleUi` gate |
| 物理占位 | 单位、建筑、硬道具、水域、地图边界已有规则证据 | `ContactClearance` gate |
| AI 副官 | observation/directive 基础存在，保持高层慢频率；`withdraw-if-critical` 已纳入合法高层指令 | no-key/offline-first direction |
| 内容边界 | README 已改成 AI RTS Commander Lab 叙事；text-safe、visual-id、art-safe metadata 均通过 boundary check | public boundary docs and checker |
| 演示证据 | 六截图、visible-flow、walkthrough 和 investor evidence 已刷新 | C1/C2 docs and ignored capture sidecars |
| 公开替换合同 | `project-owned-art-safe-slice.example.json` 已定义一张图的 clean art target | metadata-only, not runtime pack |
| 换机交接 | `docs-machine-handoff-plan-2026-06-07.md` 已写清旧机推送、新机克隆、Unity 校验和本地私有资料边界 | H2 validator/build/smoke passed |
| 移动端优先 | `docs-mobile-first-plan-2026-06-10.md` 已把 Android build、真机 smoke、触控 UI、性能预算和 iOS gate 提到平台工作之前 | BuildAndroid entry added; AndroidPlayer module missing |

当前最重要的问题：

1. `H2` validator、Windows build 和 visible-flow smoke 已通过；Unity scene fileID churn 已恢复，工作区保持干净。
2. `Pass Android G3 device smoke` 已完成：Mi 11 Lite 上真实安装、启动、推送 visible-flow command file，并通过 debrief/loadout compact logcat 成功标记。
3. 当前移动端横屏 `G4 Touch UI pass` 已通过：第一版手机端就是手机端横版（horizontal phone game），APK manifest 横屏、运行时禁用竖屏自动旋转、真机 smoke 截图为 2400x1080 横屏。
4. Android 等待期间完成的 PC1-PC69 前置包仍保留为证据；G5 性能预算已记录 Mi 11 Lite 基线，G6 已记录 iOS 的 Mac/Xcode/签名可行性边界，F2 已记录地图包/编辑器契约，F3 已记录 Web 排行/战绩公开边界，F4 已记录创作者经济/可选链上边界，F5 已记录服务器实现边界，F6 已搭起本地主服务器原型，F7 已写清 Unity-main-server 可选集成契约，F8 已落地可选 Unity 客户端适配器，F9 已接通 opt-in launch/debrief smoke，F10 已接通 opt-in inventory bootstrap smoke，F11 已写清 inventory-to-MechBay binding boundary，F12/F13/F14 已完成，F15 plan server-backed receipt slice 已完成；F16 implement server-backed receipt evidence gate 已完成；F17 plan post-receipt inventory refresh boundary 已完成；F18 implement opt-in post-receipt inventory refresh binding 已完成；F19 capture opt-in post-receipt refresh evidence 已完成；F20 refresh Android landscape build/smoke evidence 已完成；F21 audit landscape touch UI ergonomics 已完成；F22 audit landscape MechLab touch controls 已完成；F23 capture landscape MechLab touch evidence 已完成；F24 capture Android MechLab touch evidence 已完成；F25 capture Android battle command touch evidence 已完成；`F26 reduce Android combat effect log noise` 已完成；`F27 audit Android entity placeholder collision path` 已完成，证据 gate: `scripts/unity/check_android_entity_placeholder_collision_path.ps1` -> `Android entity placeholder collision path check OK`；F28 capture Android entity placeholder collision runtime evidence 已完成，证据 gate: `scripts/unity/capture_android_entity_placeholder_collision_runtime_evidence.ps1` -> `Android entity placeholder collision runtime evidence capture OK`；F29 audit PC controlled-demo visual readability 已完成，证据 gate: `scripts/unity/audit_pc_controlled_demo_visual_readability.ps1` -> `PC controlled-demo visual readability audit OK`；F30 implement PC controlled-demo visual readability fixes 已完成，证据 gate: `scripts/unity/check_pc_controlled_demo_visual_readability_fixes.ps1` -> `PC controlled-demo visual readability fixes check OK`；F31 refresh PC controlled-demo visual evidence after readability fixes complete; Evidence gate: `scripts/unity/capture_pc_controlled_demo_visual_evidence.ps1` -> `PC controlled-demo visual evidence refresh OK`; F32 audit PC controlled-demo command readability and formation feel complete; Evidence gate: `scripts/unity/audit_pc_controlled_demo_command_readability_formation.ps1` -> `PC controlled-demo command readability formation audit OK`; `F33 implement PC controlled-demo command readability and formation fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_command_readability_fixes.ps1` -> `PC controlled-demo command readability fixes check OK`; `F34 refresh PC controlled-demo command evidence after readability fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F35 audit post-F34 PC controlled-demo playable flow polish` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_playable_flow_polish.ps1` -> `PC controlled-demo playable flow polish audit OK`; `F36 implement post-F34 PC controlled-demo playable flow polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_playable_flow_polish_fixes.ps1` -> `PC controlled-demo playable flow polish fixes check OK`; `F37 refresh PC controlled-demo playable-flow evidence after polish fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F38 audit post-F37 PC controlled-demo investor readiness` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_readiness.ps1` -> `PC controlled-demo investor readiness audit OK`; `F39 implement post-F37 PC controlled-demo investor readiness fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_readiness_fixes.ps1` -> `PC controlled-demo investor readiness fixes check OK`; `F40 refresh PC controlled-demo investor-readiness evidence after fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F41 audit post-F40 PC controlled-demo investor evidence package` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_package.ps1` -> `PC controlled-demo investor evidence package audit OK`; `F42 implement post-F41 PC controlled-demo investor evidence package fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_package_fixes.ps1` -> `PC controlled-demo investor evidence package fixes check OK`; `F43 refresh PC controlled-demo investor evidence package after fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_refresh.ps1` -> `PC controlled-demo investor evidence refresh check OK`; `F44 audit post-F43 PC controlled-demo investor evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_refresh.ps1` -> `PC controlled-demo investor evidence refresh audit OK`; `F45 implement post-F44 PC controlled-demo investor evidence polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_polish_fixes.ps1` -> `PC controlled-demo investor evidence polish fixes check OK`; `F46 refresh PC controlled-demo investor route evidence after polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_refresh.ps1` -> `PC controlled-demo investor route evidence refresh check OK`; `F47 audit post-F46 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_refresh.ps1` -> `PC controlled-demo investor route evidence refresh audit OK`; `F48 implement post-F47 PC controlled-demo investor route evidence audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fixes check OK`; `F49 refresh PC controlled-demo investor route evidence after audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh check OK`; `F50 audit post-F49 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit OK`; `F51 implement post-F50 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fixes check OK`; `F52 refresh PC controlled-demo investor route evidence after F50 audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh check OK`; `F53 audit post-F52 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit OK`; `F54 implement post-F53 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fixes check OK`; next task was `F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.
5. D1 只是 art-safe metadata 合同，不是可挂载 runtime pack；后续 D2 才能进入清权资产生产和 mountable pack。
6. 私有参考素材可以继续用于本机开发验证，但公开材料不能把它描述成最终产品内容。
7. `F2-F4` 平台化方向仍保留，但现在只在移动端可行性通过后继续。

当前工作区注意事项：

- 若当前 `git status` 只剩计划文档改动，先完成校验并提交；之后等待 G3 设备，或先写清 PC47 目标。
- D1 新增的是 metadata 示例文件；不允许把私有 OBJ/TGA/PNG/JSON、截图、log 或 Unity build 输出加入 git。
- G2-G6 已完成移动端可行性收口；F2-F11 平台契约、本地主服务器原型、Unity-main-server 集成契约、可选 Unity 客户端适配器、opt-in launch/debrief smoke、opt-in inventory bootstrap smoke 和 inventory-to-MechBay binding boundary 已收口；F12 下一步只做 opt-in MechBay 预览绑定，仍不让当前 Unity Demo 强依赖远端服务。
- 如果 Unity batch 运行后只造成 `unity-mc2-demo/Assets/Scenes/Mc2Demo.unity` fileID churn，不要纳入提交。

## 1. Execution Rules

当用户说“按计划继续”时，按下面顺序执行：

1. 先看本文件 `## 4. Fine-Grained Commit Queue`。
2. 只执行第一个 `Next` 或 `In Progress` 任务。
3. 每次只做一个小提交，尽量 1 到 4 个源文件加必要文档。
4. 规则变化先进入 BattleCore，Unity 只消费规则结果。
5. 视觉变化必须配截图或 sidecar 证据。
6. 不提交 `analysis-output/`、PNG、JSON、log、Unity build output、私有参考导出。
7. Unity batch 运行后检查 `unity-mc2-demo/Assets/Scenes/Mc2Demo.unity`，仅 fileID churn 不入库。
8. 不扩大第一版范围，不做实时 PVP、账号、充值、链上、地图服务器实装；但移动端 build、触控 UI 和性能验证现在是第一优先项。
9. 公共文案只讲本项目自己的 AI 副官战术 RTS 探索。
10. 原始或原始派生素材只能当本机开发验证材料，不进入公开发布路径。

## 2. Product Lock

### 2.1 First Demo Must Show

- Windows 本地启动一局。
- MechLab 装配前置流程：机甲、武器、装甲板、散热器、热量、重量、格子合法性。
- 一张固定视角 3D 任务图：地形、水域、岸线、跑道/道路、建筑、树木/道具、敌我单位。
- 默认全队指挥：点地点移动，点目标移动攻击。
- 状态栏单选：点机甲状态行后给独立命令，完成后自动归队。
- 喷射：朝目标方向尝试固定距离位移，非法落点不动。
- 稀疏战斗 UI：机甲状态行、喷射、任务地图/目标、系统/暂停。
- 物理占位：单位、建筑、硬道具、水域、地图边界都有 BattleCore 证据。
- 武器和部位损伤：能看出能量/导弹/弹道/爆炸等武器族，能看出断臂、腿瘫、驾驶舱损毁/弹射。
- 战后简报、一键维修、返回机库、再次出战。
- AI 副官能力窗口：只做高层建议，不拖慢本地战斗。

### 2.2 First Demo Must Not Show

- 实时 PVP。
- 完整商店级移动端上线；但 Android/iOS 可行性、触控 UI 和性能预算现在是下一阶段必做。
- 地图服务器、地图编辑器、Web 排行的实装。
- 账号、充值、提现、链上资产。
- 复杂保存槽或保存游戏 UI。
- AI 导演。
- 大模型逐帧控制移动、开火、回避。
- 未清权素材作为公开产品内容。

## 3. Milestone Map

| Milestone | Phase | Current State | Exit Gate |
| --- | --- | --- | --- |
| M0 | 项目叙事与边界 | Done | README 不再围绕旧作描述 |
| M1 | Windows 本地可运行 | Done | build + smoke 通过 |
| M2 | 第一任务玩法闭环 | Done for V1 | command, jet, objective, debrief, repair, relaunch |
| M3 | MechLab 核心乐趣 | Done for V1, polish later | 格子、装配合法性、战斗影响均有证据 |
| M4 | 部位损伤卖点 | Done for V1, polish later | `damage-demo` 有断臂、腿、驾驶舱故事 |
| M5 | 稀疏 UI | Done with guard | `SparseBattleUi` sidecar gate |
| M6 | 3D 地图视觉与占位 | Done with gate | 五张战斗截图通过 `FirstMapVisual` gate |
| M7 | 私有参考视觉包稳定化 | Done | manifest-driven, missing-safe, replaceable |
| M8 | 可展示 Demo 封口 | Done | 六截图、visible-flow、walkthrough、一页证据 |
| M9 | Public art-safe slice | Done for metadata | 替换包 provenance 和 boundary check |
| M10 | AI 副官守护 | Done | no-token smoke, high-level directive only |
| M11 | 平台契约 | Done for adapter boundary | 奖励认证、地图包、排行、创作者边界、服务器边界、本地主服务器、Unity 集成契约和可选客户端适配器已写清；F9 只补 opt-in smoke 接线 |
| M12 | 换机开发交接 | Done | H2 validator/build/smoke 已通过 |
| M13 | 移动端优先可行性 | Waiting on Device | Android APK build smoke 已通过；真机 smoke 等授权手机，之后再做触控 UI 和性能预算 |
| M14 | PC/移动等待态优化 | Done for current pass | PC1-PC69 passed: PC1 baseline, PC2 battle readability, PC3 MechLab polish, PC4 controlled demo evidence package, PC5 launch preflight, PC6 evidence health check, PC7 public boundary preflight, PC8 readiness preflight, PC9 handoff consistency check, PC10 Android device-smoke preflight, PC11 PC core playable contract check, PC12 mobile command model preflight, PC13 current plan gate check, PC14 Android smoke log crash scan, PC15 Android smoke plan mode, PC16 battle HUD sparse contract check, PC17 demo source hygiene check, PC18 AI deputy contract check, PC19 Windows demo build freshness check, PC20 controlled demo evidence freshness check, PC21 controlled demo capture log freshness check, PC22 Android APK freshness check, PC23 Android APK identity check, PC24 Android APK compatibility check, PC25 Android APK signing check, PC26 Android APK manifest check, PC27 Android APK payload check, PC28 Android APK size budget check, PC29 Android SDK tooling check, PC30 Android smoke artifact hygiene check, PC31 Android smoke screenshot evidence capture, PC32 Android smoke summary evidence output, PC33 Android smoke summary schema check, PC34 Android smoke summary preflight check, PC35 Android smoke plan/preflight consistency check, PC36 Android G3 readiness check, PC37 Android G3 device requirement check, PC38 PC visual capture sanity check, PC39 PC visual capture sanity self-test, PC40 PC capture sidecar schema check, PC41 PC capture preset contract check, PC42 PC capture artifact hygiene check, PC43 PC window contract check, PC44 PC launch log hygiene check, PC45 PC build artifact hygiene check, PC46 PC smoke artifact hygiene check, PC47 current plan queue consistency check, PC48 Android device connection check, PC49 Android smoke connection gate wiring, PC50 Android smoke connection gate check, PC51 Android visible-flow command-file smoke, PC52 Android WPD-only device diagnosis, PC53 Android ADB setup guidance, PC54 Android ADB readiness watch, PC55 Android G3 device status report, PC56 Android G3 when-ready runner and PC57 Android ADB driver package probe |

## 4. Fine-Grained Commit Queue

| Order | Status | Commit | Purpose | Gate |
| --- | --- | --- | --- | --- |
| A0 | Done by this doc | `Refresh detailed master plan` | 收束当前阶段、缺口、执行批次 | `git diff --check` |
| A1 | Done | `Audit first map visual readability` | 固化五张截图的视觉问题分类和 sidecar 门槛 | screenshot review + docs |
| A2 | Done | `Improve terrain and water readability` | 地面、水域、岸线、跑道/道路不再像一整块平色 | build + `spawn,airfield,north-patrol` captures |
| A3 | Done | `Improve unit silhouette readability` | 敌我机甲/车辆在默认镜头下更容易分辨 | build + `spawn,hangar-contact,damage-demo` captures |
| A4 | Done | `Improve structure and prop readability` | 建筑、树木、机场道具不再是灰色糊团 | build + `airfield,hangar-contact,north-patrol` captures |
| A5 | Done | `Gate first map visual slice` | sidecar 检查第一图视觉、稀疏 UI、碰撞不回退 | build + five battle captures |
| A6 | Done | `Refresh demo evidence after visual pass` | 更新证据页和审计文档 | six captures + docs |
| B1 | Done | `Stabilize reference visual manifest export` | 私有参考单位/道具/地形资源导出 manifest | exporter dry run + missing-source probe |
| B2 | Done | `Harden Unity reference visual loader` | Unity 优先读 manifest，缺失安全回退 | build + fallback capture |
| B3 | Done | `Document replaceable visual ids` | 固化换包 id，方便以后整包替换 | docs + boundary check |
| C1 | Done | `Seal visible playable walkthrough` | 启动、机库、战斗、损伤、结算、重开完整流程 | visible-flow smoke |
| C2 | Done | `Refresh investor evidence package` | 更新本地演示证据，不提交生成截图 | six captures + docs |
| D1 | Done | `Prepare public art-safe mission slice` | 从 text-safe 进入公开视觉替换包计划 | boundary check |
| E1 | Done | `Guard AI deputy offline behavior` | AI 高层、可离线、不逐帧、不花 smoke token | validator |
| F1 | Done | `Document reward authority contract` | 主服务器认证奖励，地图服务器只提交 claim | docs |
| H1 | Done | `Prepare machine handoff plan` | 换机前后如何推送、克隆、验证、恢复本地私有资料和 AI key | docs |
| H2 | Done | `Push machine handoff checkpoint` | 代码已推送，validator/build/smoke 基线通过 | git + Unity smoke |
| G1 | Done | `Reframe plan around mobile first` | 移动端第一优先，Unreal MCP 不进主线，平台契约后移 | docs |
| G2 | Done | `Add Android build smoke path` | Android Build Support、SDK/NDK/JDK/CMake 已补齐；APK 已产出 | Android build |
| G3 | Waiting on Device | `Run Android device smoke` | 真机启动并跑最小 visible-flow/command smoke；当前 adb 无授权设备 | device smoke |
| PC1 | Done | `Audit PC demo baseline` | Android G3 等设备期间，重跑 PC validator/build/visible-flow/six captures 并锁定下一处问题 | PC evidence |
| PC2 | Done | `Polish PC battle readability` | 根据 PC1 证据先修地形、水域、岸线、道路/跑道和可战斗陆地区域可读性 | Windows build + captures |
| PC3 | Done | `Polish PC MechLab flow` | 装配格子、整块武器、热量/重量/合法性更直观，不恢复武器开关 | Windows build + `mechlab` capture |
| PC4 | Done | `Package PC controlled demo evidence` | 刷新 walkthrough/evidence，形成 PC 可展示包说明 | docs |
| PC5 | Done | `Add PC demo launch preflight` | 受控启动脚本检查 Windows build 并用固定窗口参数启动 | script check |
| PC6 | Done | `Add controlled demo evidence check` | 机器检查当前 Windows 受控演示证据包仍完整 | script check |
| PC7 | Done | `Add controlled demo public boundary preflight` | 机器检查项目自有 metadata 示例包仍 public-safe，并可确认 dev build 非 public-safe | boundary preflight |
| PC8 | Done | `Add controlled demo readiness preflight` | 一键汇总受控启动、演示证据和公开边界 gate | readiness preflight |
| PC9 | Done | `Add controlled demo handoff consistency check` | 检查关键脚本、README、BUILD-WIN、计划、证据和换机文档仍指向同一套受控演示入口 | handoff consistency |
| PC10 | Done | `Add Android device smoke preflight` | 检查 APK、adb、aapt、包名、Activity 和设备状态，当前允许明确停在 Waiting on Device | Android preflight |
| PC11 | Done | `Add PC core playable contract check` | 单独验证指挥状态、独立命令归队、Jet 合法性、占位、损伤/弹射、战报/重开这些 PC 演示核心规则仍被 BattleCore 守住 | `check_pc_core_playable_contract.ps1` |
| PC12 | Done | `Add mobile command model preflight` | 不启动 Unity，读取现有 sidecar 和源码/文档标记，证明 PC 演示仍符合移动端低复杂度指挥模型 | `check_mobile_command_model_preflight.ps1` |
| PC13 | Done | `Add current plan gate check` | 串联交接/readiness、移动指挥模型预检和 Android 等设备预检，一条命令证明当前计划状态仍可交接且 G3 只差授权真机 | `check_current_plan_gate.ps1` |
| PC14 | Done | `Add Android smoke log crash scan` | Android 真机 smoke 抓取 logcat 后扫描 fatal exception、fatal signal、ANR、进程死亡和强制结束，避免只看进程存活导致误判 | `check_android_smoke_log.ps1 -SelfTest` |
| PC15 | Done | `Add Android smoke plan mode` | `android_device_smoke.ps1 -PlanOnly` 在无设备时解析 APK/adb/aapt/package/activity/log path 和安装/启动/log-check 开关，预演真机 smoke 动作 | `android_device_smoke.ps1 -PlanOnly` |
| PC16 | Done | `Add battle HUD sparse contract check` | 不启动 Unity，检查源码、capture gate 和移动指挥预检都要求稀疏战斗 HUD、关闭任务地图、大日志/存档/账号/调试覆盖层不回流 | `check_battle_hud_sparse_contract.ps1` |
| PC17 | Done | `Add demo source hygiene check` | 不启动 Unity，检查 tracked/staged 路径和 `.gitignore`，防止生成截图、日志、Unity build、APK/AAB 和私有参考导出误进入源码提交 | `check_demo_source_hygiene.ps1` |
| PC18 | Done | `Add AI deputy contract check` | 不启动 Unity、不调用模型，检查 MiniMax 仍是可选慢频高层 directive、无 key fallback、默认 smoke 不请求模型步数、frame loop 不调用模型 | `check_ai_deputy_contract.ps1` |
| PC19 | Done | `Add Windows demo build freshness check` | 不启动 Unity，检查 ignored Windows player 输出晚于 tracked Unity build 输入，并接入 readiness preflight | `check_windows_demo_build_freshness.ps1` |
| PC20 | Done | `Add controlled demo evidence freshness check` | 受控演示 evidence check 拒绝早于当前 build/证据输入的 visible-flow 日志和六截图 sidecar | `check_controlled_demo_evidence.ps1` |
| PC21 | Done | `Add controlled demo capture log freshness check` | 受控演示 evidence check 要求六个 capture 日志存在、足够新且含 preset/截图/sidecar 标记 | `check_controlled_demo_evidence.ps1` |
| PC22 | Done | `Add Android APK freshness check` | Android 真机 smoke 前确认 ignored APK 晚于 tracked Unity build 输入，并接入 G3 preflight/smoke/current gate | `check_android_apk_freshness.ps1` |
| PC23 | Done | `Add Android APK identity check` | Android 真机 smoke 前确认 package/activity 与预期 install/launch 身份一致 | `check_android_apk_identity.ps1` |
| PC24 | Done | `Add Android APK compatibility check` | Android 真机 smoke 前确认 minSdk、targetSdk 和 native ABI 与预期设备兼容性一致 | `check_android_apk_compatibility.ps1` |
| PC25 | Done | `Add Android APK signing check` | Android 真机 smoke 前确认 apksigner verify、v2 签名和 debug signer DN 均通过 | `check_android_apk_signing.ps1` |
| PC26 | Done | `Add Android APK manifest check` | Android 真机 smoke 前确认权限白名单、无 required hardware feature、屏幕支持与预期一致 | `check_android_apk_manifest.ps1` |
| PC27 | Done | `Add Android APK payload check` | Android 真机 smoke 前确认 Unity/IL2CPP native libraries、`assets/bin/Data` 和 ABI 目录完整 | `check_android_apk_payload.ps1` |
| PC28 | Done | `Add Android APK size budget check` | Android 真机 smoke 前确认 APK 包体没有低于合理下限或超过当前早期移动 Demo 预算 | `check_android_apk_size_budget.ps1` |
| PC29 | Done | `Add Android SDK tooling check` | Android 真机 smoke 前确认 Unity AndroidPlayer SDK、NDK、OpenJDK、build-tools、platform、adb、aapt 和 apksigner 可用 | `check_android_sdk_tooling.ps1` |
| PC30 | Done | `Add Android smoke artifact hygiene check` | Android 真机 smoke 前确认 APK/AAB、log、截图等 ignored 生成物不会被 tracked/staged 路径误带入提交 | `check_android_smoke_artifact_hygiene.ps1` |
| PC31 | Done | `Add Android smoke screenshot evidence capture` | Android 真机 smoke 预演和真实运行都记录 ignored 截图路径，真实设备到位后可同时产出 logcat 和启动截图证据 | `android_device_smoke.ps1 -PlanOnly` |
| PC32 | Done | `Add Android smoke summary evidence output` | Android 真机 smoke 预演和真实运行都记录 ignored summary JSON 路径，真实设备到位后自动写入设备、包、log、截图和进程摘要 | `android_device_smoke.ps1 -PlanOnly` |
| PC33 | Done | `Add Android smoke summary schema check` | Android 真机 smoke 摘要输出后立刻校验 JSON schema、包名、时间戳、设备/进程、证据路径和执行布尔标记，PC 等待态可用自测覆盖 | `check_android_smoke_summary.ps1 -SelfTest` |
| PC34 | Done | `Add Android smoke summary preflight check` | Android 真机 smoke 前置检查直接运行 summary schema 自测，确保 G3 入口本身覆盖 ignored summary 证据格式 | `check_android_device_preflight.ps1 -AllowNoDevice` |
| PC35 | Done | `Add Android smoke plan/preflight consistency check` | Android 真机 smoke 预演和 G3 preflight 对 package/activity、证据路径和关键开关保持一致 | `check_android_smoke_plan_consistency.ps1` |
| PC36 | Done | `Add Android G3 readiness check` | Android 真机 smoke 前把 preflight、plan consistency、plan mode、log scanner、summary schema 收成一个直接 G3 readiness 入口 | `check_android_g3_readiness.ps1` |
| PC37 | Done | `Add Android G3 device requirement check` | 严格 G3 readiness 在无授权手机时必须等待设备，有授权手机时才允许进入真实 smoke | `check_android_g3_device_requirement.ps1` |
| PC38 | Done | `Add PC visual capture sanity check` | 受控演示六张 PNG 截图不能退化为空白、纯色、粉框或低信息量色块 | `check_pc_visual_capture_sanity.ps1` |
| PC39 | Done | `Add PC visual capture sanity self-test` | 视觉截图 sanity 门禁能自测识别合格图、纯色坏图和粉色 fallback 坏图 | `check_pc_visual_capture_sanity.ps1 -SelfTest` |
| PC40 | Done | `Add PC capture sidecar schema check` | 六张受控演示 JSON sidecar 的截图路径、尺寸、flow、camera、摘要字段和 referenceAssets 结构可独立复核 | `check_pc_capture_sidecar_schema.ps1` |
| PC41 | Done | `Add PC capture preset contract check` | 六张受控演示标准截图 preset 在 capture、evidence、sanity、sidecar schema 和文档入口中保持一致 | `check_pc_capture_preset_contract.ps1` |
| PC42 | Done | `Add PC capture artifact hygiene check` | 本地参考截图、sidecar、capture log 和视觉 sanity 自测图保持 ignored，且不在 tracked/staged 源码路径中 | `check_pc_capture_artifact_hygiene.ps1` |
| PC43 | Done | `Add PC window contract check` | 受控 Windows launcher 和参考截图 helper 保持 1280x720 windowed 参数，避免异常巨大窗口回流 | `check_pc_window_contract.ps1` |
| PC44 | Done | `Add PC launch log hygiene check` | 受控 Windows launcher runtime log 固定到 ignored `analysis-output/windows-demo-run.log`，并禁止 launch log 进入 tracked/staged 路径 | `check_pc_launch_log_hygiene.ps1` |
| PC45 | Done | `Add PC build artifact hygiene check` | Windows player 输出固定到 ignored `unity-mc2-demo/Builds/Windows/`，并禁止 player build 进入 tracked/staged 路径 | `check_pc_build_artifact_hygiene.ps1` |
| PC46 | Done | `Add PC smoke artifact hygiene check` | PC smoke、validator、build 和 saved-account evidence 输出固定到 ignored `analysis-output/`，并禁止运行证据进入 tracked/staged 路径 | `check_pc_smoke_artifact_hygiene.ps1` |
| PC47 | Done | `Add current plan queue consistency check` | 主计划、细计划、PC 计划、移动计划、证据页和换机文档在当时明确 PC1-PC47 封口，且正式下一步仍是 G3 真机 smoke | `check_current_plan_queue.ps1` |
| PC48 | Done | `Add Android device connection check` | 单独读取 `adb devices -l`，区分 no device、unauthorized、offline、多设备和 ready，帮助 G3 真机 smoke 快速定位连接状态 | `check_android_device_connection.ps1` |
| PC49 | Done | `Wire Android smoke connection gate` | 真正的 `android_device_smoke.ps1` 在安装/启动前强制通过 `check_android_device_connection.ps1 -RequireDevice`，PlanOnly 暴露 connection marker | `ConnectionCheck: check_android_device_connection.ps1 -RequireDevice` |
| PC50 | Done | `Add Android smoke connection gate check` | 独立复验无授权设备时真实 smoke 在安装/启动前失败，并确认 smoke log、截图和 summary 不被改写 | `check_android_smoke_connection_gate.ps1` |
| PC51 | Done | `Add Android visible-flow command-file smoke` | 真机 smoke 到位后推送 `mc2_01-visible-flow-audit.txt`，通过 Unity `-mc2CommandFile` 启动，并要求复盘与装配回流成功 marker | `CommandFileSmoke: True` |
| PC52 | Done | `Add Android WPD-only device diagnosis` | Windows 只把手机暴露为 WPD/MTP、但 adb 无设备行时，连接检查能明确报告不是可安装真机 | `WpdOnlyAndroidProbe: True` |
| PC53 | Done | `Add Android ADB setup guidance` | WPD/MTP-only 时输出当前 Windows driver/provider/inf/service 和下一步 ADB 设置提示，避免把 MTP 误当可安装真机 | `AdbSetupHint: True` |
| PC54 | Done | `Add Android ADB readiness watch` | 手机侧切 USB 调试或驱动状态时可轮询等待 adb `device`，当前无设备时可用 no-device-safe 模式验证 | `AdbWatchHint: True` |
| PC55 | Done | `Add Android G3 device status report` | 生成 ignored `analysis-output/android-g3-device-status.json`，记录当前 G3 设备 ready/waiting、blocker 和 helper 输出 | `G3DeviceStatusReport: True` |
| PC56 | Done | `Add Android G3 when-ready runner` | 等待 adb `device` 后才调用真实 `android_device_smoke.ps1`，PlanOnly 只预演不安装/启动 | `G3WhenReady: True` |
| PC57 | Done | `Add Android ADB driver package probe` | 只读枚举 `pnputil /enum-drivers` 和当前 PnP 手机驱动，确认 ADB/WinUSB 候选驱动包状态 | `AdbDriverPackageProbe: True` |
| G4 | Done | `Adapt command UI for mobile touch` | 状态行、Jet、地图、系统和 MechLab 手机横屏触控可用 | device smoke |
| G5 | Done | `Define mobile performance budget` | Mi 11 Lite 记录 FPS、内存、包体、加载、热量/电量基线 | `docs-mobile-performance-budget-2026-06-10.md`; `check_mobile_performance_budget.ps1` |
| G6 | Done | `Document iOS feasibility gate` | macOS/Xcode/签名/Metal/真机要求 | `check_ios_feasibility_gate.ps1` |
| F2 | Done | `Document map authoring contract` | 开源地图编辑器和地图包最小契约，移动 gate 后恢复 | `check_map_authoring_contract.ps1` |
| F3 | Done | `Document web ranking contract` | Web 排行、战绩、地图页和隐私边界 | `check_web_ranking_contract.ps1` |
| F4 | Done | `Document creator economy boundary` | 皮肤、地图、分成、可选链上边界 | `check_creator_economy_boundary.ps1` |
| F5 | Done | `Document server implementation boundary` | 主服务器最小原型边界和模块切片 | `check_server_implementation_boundary.ps1` |
| F6 | Done | `Scaffold local main-server prototype` | 本地主服务器健康检查、fixture 和最小 API 骨架 | `check_local_main_server.ps1` |
| F7 | Done | `Document Unity main-server integration contract` | Unity 未来如何请求签名小队和提交奖励 claim，同时保持当前 Demo 离线优先 | `check_unity_main_server_integration_contract.ps1` |
| F8 | Done | `Implement optional Unity main-server client adapter` | 在 Unity 内实现可禁用的签名小队/奖励 claim 客户端适配层，保持离线 fixture fallback | `check_optional_unity_main_server_client_adapter.ps1` |
| F9 | Done | `Wire optional Unity main-server adapter into launch/debrief smoke` | 只在 opt-in smoke 中请求签名小队和提交奖励 claim，默认离线演示不变 | `check_optional_unity_main_server_launch_debrief_smoke.ps1` |
| F10 | Done | `Wire optional Unity inventory bootstrap smoke` | 只在 opt-in smoke 中拉取 dev account/inventory bootstrap，默认离线演示不变 | `check_optional_unity_inventory_bootstrap_smoke.ps1` |
| F11 | Done | `Plan inventory-to-MechBay binding boundary` | 规划主服务器 inventory 如何进入 MechLab/机库，同时保持默认离线演示和横屏移动路线 | `check_inventory_mechbay_binding_boundary.ps1` |
| F12 | Done | `Implement opt-in inventory-to-MechBay preview binding` | 只在 opt-in 路径把主服务器 inventory 投影到 MechBay 预览，默认 Demo、本地 fixture、BattleCore 帧循环和手机横版布局保持不变 | `check_optional_inventory_mechbay_preview_binding.ps1` |
| F13 | Done | `Capture opt-in MechBay preview evidence` | 捕获 MechLab 库存来源行和 opt-in 预览 sidecar 证据，默认 Demo、本地 fixture、BattleCore 帧循环和手机横版布局保持不变 | `capture_inventory_mechbay_preview_evidence.ps1` |
| F14 | Done | `Capture landscape-phone MechLab source-line evidence` | 用 2400x1080 横版手机比例捕获 MechLab 库存来源行/装配界面证据，确认首版手机端横版布局不拥挤、不引入竖屏目标 | `capture_landscape_phone_mechlab_source_line_evidence.ps1` |
| F15 | Done | `Plan server-backed receipt slice` | 规划主服务器战斗回执、奖励、库存和排行榜最小实现切片，保持 BattleCore 帧循环、本地 Demo 离线优先和手机横版第一版约束 | `check_server_backed_receipt_slice_plan.ps1` |
| F16 | Done | `Implement server-backed receipt evidence gate` | 运行本地主服务器回执流证据：签名小队、提交战报、重复 claim 幂等、库存余额只变一次、拒绝 claim 不变更库存、排行榜投影存在；不启动 Unity、不加入逐帧服务端依赖 | `capture_server_backed_receipt_evidence.ps1` |
| F17 | Done | `Plan post-receipt inventory refresh boundary` | 规划服务端回执通过后如何刷新库存、战报和 MechBay/机库显示，保持默认离线、本地 fixture fallback、手机横版第一版和无逐帧服务端依赖 | `check_post_receipt_inventory_refresh_boundary.ps1` |
| F18 | Done | `Implement opt-in post-receipt inventory refresh binding` | 只在 opt-in post-debrief 路径消费主服务器 accepted reward claim 返回的 inventory snapshot，刷新战报和 MechBay/机库显示；默认离线、本地 fixture fallback、手机横版第一版和无逐帧服务端依赖保持不变 | `check_post_receipt_inventory_refresh_binding.ps1` |
| F19 | Done | `Capture opt-in post-receipt refresh evidence` | 捕获 opt-in post-debrief 主服务器 reward inventory refresh 的 debrief 文案、机库来源行、重复 claim 不重复应用和横版布局证据；默认离线、本地 fixture fallback、手机横版第一版和无逐帧服务端依赖保持不变 | `capture_post_receipt_refresh_evidence.ps1` |
| F20 | Done | `Refresh Android landscape build/smoke evidence` | 基于 F19 后的新源码刷新 Android 横版构建和真机 smoke 证据，保持手机端第一版横屏、command-file smoke 和主服务器 opt-in 边界 | android build/smoke evidence |
| F21 | Done | `Audit landscape touch UI ergonomics` | 基于 F20 横屏真机证据收紧战斗可见流程：任务列表和战报动作按钮在手机横版布局下使用 44px 触控目标，sidecar 摘要记录 mission/debrief 按钮和面板适配，继续排除竖屏首版支持 | `check_landscape_touch_ui_ergonomics.ps1` |
| F22 | Done | `Audit landscape MechLab touch controls` | 基于 F21 后横屏触控审计，MechLab 购买、雇佣、支援队伍、Reserve Fit、维修、武器块选择和 Apply/Reset 等按钮在手机横版布局下使用 44px 触控目标，sidecar 摘要记录 mechLab* 触控标记，继续排除竖屏首版支持 | `check_landscape_mechlab_touch_controls.ps1` |
| F23 | Done | `Capture landscape MechLab touch evidence` | 捕获 2400x1080 横屏手机和 PC 参考 MechLab touch 证据，校验 PNG、sidecar 和日志中购买、雇佣、支援队伍、格子编辑触控目标、整块武器/单格组件装配和横屏边界 | `capture_landscape_mechlab_touch_evidence.ps1` |
| F24 | Done | `Capture Android MechLab touch evidence` | 在已连接 Android 横屏 APK 上捕获 MechLab touch 截图、logcat 和 summary，验证真机横屏装配按钮、格子触控和 F23 Windows phone-ratio 证据一致，继续排除竖屏首版支持 | `capture_android_mechlab_touch_evidence.ps1` |
| F25 | Done | `Capture Android battle command touch evidence` | 在已连接 Android 横屏 APK 上捕获战斗指挥触控证据，验证状态栏单选、默认全队、移动/集火、喷射、任务地图/系统入口和稀疏 HUD 在真机横版布局下可用；真机证据为 Mi 11 Lite 2400x1080 横屏截图、logcat 和 summary | `capture_android_battle_command_touch_evidence.ps1` |
| F26 | Done | `Reduce Android combat effect log noise` | 基于 F25 真机 logcat 发现的 CreatePrimitive collider 噪声，把战斗特效、损伤提示、结构特效、指挥覆盖层、血条和地图视觉标记改为 Android 友好的无 collider 几何生成路径，同时保留实体/世界占位 CreatePrimitive 边界 | `check_android_combat_effect_log_noise.ps1` |
| F27 | Done | `Audit Android entity placeholder collision path` | 审计 Android 横版实体/建筑/硬 prop 占位碰撞路径，确认 BattleCore radii/push resolver、occupancy sidecar 和无 Collider visual primitive 工厂分离 | `check_android_entity_placeholder_collision_path.ps1` |
| F28 | Done | `Capture Android entity placeholder collision runtime evidence` | 在已连接 Android 横屏 APK 上捕获实体/建筑/硬 prop 占位碰撞运行证据，验证 sidecar-log 摘要和真机 logcat 保持 overlap=0、collision/pathing unchanged、无 Collider 类缺失噪声；真机证据为 Mi 11 Lite 2400x1080 横屏截图、logcat、summary 和 sidecar-log JSON | `capture_android_entity_placeholder_collision_runtime_evidence.ps1` |
| F29 | Done | `Audit PC controlled-demo visual readability` | 回到 PC 端优化，基于当前碰撞/sidecar 证据审计 spawn、hangar-contact、damage-demo 的单位、建筑、地形和稀疏 HUD 可读性，输出 10 条下一轮视觉修正清单，当前基线为 pass-with-followups | `audit_pc_controlled_demo_visual_readability.ps1` |
| F30 | Done | `Implement PC controlled-demo visual readability fixes` | 按 F29 审计结果优先修 terrain 对比、objective-near 树/prop 遮挡、接触压力下单位环对比、damage/ejection cue 与 target-hot 红环竞争、左侧状态 rail 密度 | `check_pc_controlled_demo_visual_readability_fixes.ps1` |
| F31 | Done | `Refresh PC controlled-demo visual evidence after readability fixes` | 基于 F30 修正重新封存 PC controlled-demo 视觉证据，确认 spawn/hangar-contact/damage-demo 截图、sidecar、报告与计划交接一致 | `capture_pc_controlled_demo_visual_evidence.ps1` |
| F32 | Done | `Audit PC controlled-demo command readability and formation feel` | F32 audit completed against refreshed PC visual evidence; pass-with-followups recorded for formation spacing, solo-order screenshot state, commander-follow sidecar, and command/damage cue separation | `audit_pc_controlled_demo_command_readability_formation.ps1` |
| F33 | Done | `Implement PC controlled-demo command readability and formation fixes` | Applied F32 followups: widened player formation spacing, added CommandReadability/CommanderFollow sidecar fields, added solo-order capture preset, and separated command/target/damage/hostile pressure cue palette while preserving the landscape-phone command model. | `check_pc_controlled_demo_command_readability_fixes.ps1` |
| F34 | Done | `Refresh PC controlled-demo command evidence after readability fixes` | Rebuilt the Windows player, refreshed spawn/hangar-contact/damage-demo/solo-order PC command evidence, and verified CommandReadability/CommanderFollow sidecars plus formation/contact cue separation after F33. Kept first phone slice landscape-only. | `capture_pc_controlled_demo_command_evidence.ps1` |
| F35 | Done | `Audit post-F34 PC controlled-demo playable flow polish` | Audited the refreshed command evidence and playable loop across command clarity, contact pressure, damage story, sparse HUD, solo-order isolation and handoff/demo readiness; recorded F36 followups without expanding mobile beyond landscape. | `audit_pc_controlled_demo_playable_flow_polish.ps1` |
| F36 | Done | `Implement post-F34 PC controlled-demo playable flow polish fixes` | Implemented the F35 followups: objective-panel contact pressure cue, damage/ejection-to-debrief repair consequence line, solo-return settled preset, denser 280px PC status rail, and unified playable-flow sidecar field while keeping mobile landscape-only. | `check_pc_controlled_demo_playable_flow_polish_fixes.ps1` |
| F37 | Done | `Refresh PC controlled-demo playable-flow evidence after polish fixes` | Refreshed PC controlled-demo visual/command evidence for spawn, hangar-contact, damage-demo, solo-order and solo-return; the command report now links command clarity, contact pressure, damage/debrief story, sparse HUD and the landscape-only mobile contract after F36. | `capture_pc_controlled_demo_command_evidence.ps1` |
| F38 | Done | `Audit post-F37 PC controlled-demo investor readiness` | Added `audit_pc_controlled_demo_investor_readiness.ps1`; it audits F37 spawn, hangar-contact, damage-demo, solo-order and solo-return as a pass-with-followups investor evidence package while preserving prototype-art and landscape-only mobile boundaries. | `audit_pc_controlled_demo_investor_readiness.ps1` |
| F39 | Done | `Implement post-F37 PC controlled-demo investor readiness fixes` | Implemented the narrow F38 followups: command evidence now exposes damage-demo debrief/repair consequence via the actual sidecar field, Unity sidecar records development-safe investor proxy visuals for fallback units/props, and `docs-pc-investor-demo-route-2026-06-13.md` gives a concise PC investor route while preserving the landscape mobile command model. | `check_pc_controlled_demo_investor_readiness_fixes.ps1` |
| F40 | Done | `Refresh PC controlled-demo investor-readiness evidence after fixes` | Rebuilt the Windows player, refreshed spawn, hangar-contact, damage-demo, solo-order and solo-return command evidence after F39, and regenerated the report with F40/F41 metadata, damage-demo debrief summary and visible investor proxy visual fields. | `capture_pc_controlled_demo_command_evidence.ps1` |
| F41 | Done | `Audit post-F40 PC controlled-demo investor evidence package` | Added `audit_pc_controlled_demo_investor_evidence_package.ps1`; it audits the refreshed F40 PC investor evidence package for presentation readiness, visible damage/debrief story, proxy-art clarity, sparse HUD fit and the mobile landscape-only boundary, then records F42 follow-ups. | `audit_pc_controlled_demo_investor_evidence_package.ps1` |
| F42 | Done | `Implement post-F41 PC controlled-demo investor evidence package fixes` | Added compact investor report highlights, stronger public-safe proxy visual identity markers, a damage/ejection/debrief investor callout, and a fast source/report evidence gate while preserving the mobile landscape-only boundary. | `check_pc_controlled_demo_investor_evidence_package_fixes.ps1` |
| F43 | Done | `Refresh PC controlled-demo investor evidence package after fixes` | Refreshed the PC controlled-demo command evidence package from existing sidecars after F42; the report metadata now records F43/F44 and the markdown carries executive summary, preset highlights, proxy identity markers, damage callout and the mobile landscape-only boundary. | `check_pc_controlled_demo_investor_evidence_refresh.ps1` |
| F44 | Done | `Audit post-F43 PC controlled-demo investor evidence refresh` | Added `audit_pc_controlled_demo_investor_evidence_refresh.ps1`; it audits the refreshed F43 PC investor evidence package for presentation readiness, freshness, public-safe proxy clarity, damage/debrief story strength, sparse HUD fit and mobile landscape-only boundary, then records F45 follow-ups. | `audit_pc_controlled_demo_investor_evidence_refresh.ps1` |
| F45 | Done | `Implement post-F44 PC controlled-demo investor evidence polish fixes` | Applied the F44 follow-ups: compact investor route summary, explicit damage-demo screenshot/sidecar/log links, visible landscape-phone proof line, and easier proxy-language parsing source markers without launching Unity or expanding mobile beyond landscape. | `check_pc_controlled_demo_investor_evidence_polish_fixes.ps1` |
| F46 | Done | `Refresh PC controlled-demo investor route evidence after polish fixes` | Refreshed the ignored command evidence markdown/json from existing sidecars after F45, carrying route summary, damage links, horizontal-phone proof line and proxy parsing marker under F46/F47 metadata without launching Unity. | `check_pc_controlled_demo_investor_route_evidence_refresh.ps1` |
| F47 | Done | `Audit post-F46 PC controlled-demo investor route evidence refresh` | Added `audit_pc_controlled_demo_investor_route_evidence_refresh.ps1`; it audits the refreshed F46 investor route evidence for presentation readiness, exact route proof, damage/ejection proof links, horizontal-phone proof line and public-safe proxy parsing clarity, then records F48 follow-ups. | `audit_pc_controlled_demo_investor_route_evidence_refresh.ps1` |
| F48 | Done | `Implement post-F47 PC controlled-demo investor route evidence audit fixes` | Implemented the narrow F47 follow-ups so the route-audit findings and next fixes are visible in the investor route, playable evidence and handoff docs without launching Unity or expanding mobile beyond horizontal phone scope. | `check_pc_controlled_demo_investor_route_evidence_audit_fixes.ps1` |
| F49 | Done | `Refresh PC controlled-demo investor route evidence after audit fixes` | Refreshed the ignored route evidence package after F48 so command evidence/report metadata carries the route-audit fix closure and F49/F50 state without launching Unity. | `check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1` |
| F50 | Done | `Audit post-F49 PC controlled-demo investor route evidence refresh` | Added `audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1`; it audits the refreshed F49 route evidence package for route-proof clarity, damage/ejection links, horizontal-phone proof visibility, public-safe proxy language and route-audit fix closure, then records F51 follow-ups. | `audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1` |
| F51 | Done | `Implement post-F50 PC controlled-demo investor route evidence refresh audit fixes` | Made the F50 audit findings and follow-ups visible in plan, evidence and handoff surfaces before the next command-evidence metadata refresh, with no Unity launch and first phone version still landscape-only. | `check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fixes.ps1` |
| F52 | Done | `Refresh PC controlled-demo investor route evidence after F50 audit fixes` | Refreshed command evidence metadata so it explicitly consumes the F50 audit report and F51 fix report while preserving route, damage/ejection, public-safe proxy and landscape phone proof lines. | `check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh.ps1` |
| F53 | Done | `Audit post-F52 PC controlled-demo investor route evidence refresh` | Audited the refreshed F52 route evidence package for investor-route clarity, damage/ejection continuity, horizontal-phone proof visibility, public-safe proxy wording and evidence/report closure. | `audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh.ps1` |
| F54 | Done | `Implement post-F53 PC controlled-demo investor route evidence refresh audit fixes` | Made the F53 audit findings and follow-ups visible in plan, evidence and handoff surfaces before the next command-evidence metadata refresh, keeping no-Unity-launch evidence and landscape-only phone scope. | `check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` |
| F55 | Done | `Refresh PC controlled-demo investor route evidence after F53 audit fixes` | Refreshed command evidence metadata so it explicitly consumes the F53 audit report and F54 fix report while preserving the route, damage/ejection, public-safe proxy and horizontal-phone proof lines. | `check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` |
| F56 | Done | `Audit post-F55 PC controlled-demo investor route evidence refresh` | Audited the refreshed F55 route evidence package for traceability, investor-route clarity, damage/ejection continuity, horizontal-phone proof visibility, public-safe proxy wording and report closure, with follow-up fixes queued. | `audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` |
| F57 | Done | `Implement post-F56 PC controlled-demo investor route evidence refresh audit fixes` | Made the F56 audit findings and follow-ups visible in plan, evidence and handoff surfaces before the next command-evidence metadata refresh, keeping no-Unity-launch evidence and landscape-only phone scope. | `check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` |
| F58 | Done | `Refresh PC controlled-demo investor route evidence after F56 audit fixes` | Refreshed command evidence metadata so it explicitly consumes the F56 audit report and F57 fix report while preserving route, damage/ejection, public-safe proxy and landscape phone proof lines. | `check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` |
| F59 | Done | `Audit post-F58 PC controlled-demo investor route evidence refresh` | Audited the F58 command evidence refresh for F56/F57 source traceability, route readability, damage/ejection continuity, public-safe proxy wording and landscape phone proof visibility. | `audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1` |
| F60 | Done | `Implement post-F59 PC controlled-demo investor route evidence refresh audit fixes` | Made the F59 audit findings and follow-ups visible in plan, evidence and handoff surfaces before the next command-evidence metadata refresh, keeping no-Unity-launch evidence and landscape-only phone scope. | `check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` |
| F61 | Done | `Refresh PC controlled-demo investor route evidence after F59 audit fixes` | Refreshed command evidence metadata so it explicitly consumes the F59 audit report and F60 fix report while preserving route, damage/ejection, public-safe proxy and landscape phone proof lines. | `check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` |
| F62 | Done | `Audit post-F61 PC controlled-demo investor route evidence refresh` | Audited the refreshed F61 route evidence package for F59/F60 source traceability, route readability, damage/ejection continuity, public-safe proxy wording and landscape phone proof visibility. | `audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1` |
| F63 | Done | `Implement post-F62 PC controlled-demo investor route evidence refresh audit fixes` | Made the F62 audit findings and follow-ups visible in plan, evidence and handoff surfaces before the next command-evidence metadata refresh, keeping no-Unity-launch evidence and landscape-only phone scope. | `check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` |
| F64 | Done | `Refresh PC controlled-demo investor route evidence after F62 audit fixes` | Refreshed command evidence metadata so it explicitly consumes the F62 audit report and F63 fix report while preserving route, damage/ejection, public-safe proxy and landscape phone proof lines. | `check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` |
| F65 | Done | `Audit post-F64 PC controlled-demo investor route evidence refresh` | Audited the refreshed F64 route evidence package for F62/F63 source traceability, route readability, damage/ejection continuity, public-safe proxy wording and landscape phone proof visibility. | `audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1` |
| F66 | Done | `Implement post-F65 PC controlled-demo investor route evidence refresh audit fixes` | Made the F65 audit findings and follow-ups visible in plan, evidence and handoff surfaces before the next command-evidence metadata refresh, keeping no-Unity-launch evidence and landscape-only phone scope. | `check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` |
| F67 | Done | `Refresh PC controlled-demo investor route evidence after F65 audit fixes` | Refreshed command evidence metadata so it explicitly consumes the F65 audit report and F66 fix report while preserving route, damage/ejection, public-safe proxy and landscape phone proof lines. | `check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` |
| F68 | Done | `Audit post-F67 PC controlled-demo investor route evidence refresh` | Audited the refreshed F67 command evidence package for F65/F66 traceability, route readability, damage/ejection continuity, public-safe proxy wording and landscape phone proof visibility. | `audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1` |
| F69 | Done | `Implement post-F68 PC controlled-demo investor route evidence refresh audit fixes` | Made F68 audit findings and follow-ups visible in plan, evidence and handoff surfaces before the next command-evidence metadata refresh, keeping no-Unity-launch evidence and landscape-only phone scope. | `check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` |
| F70 | Next | `Refresh PC controlled-demo investor route evidence after F68 audit fixes` | Refresh command evidence metadata so it explicitly consumes the F68 audit report and F69 fix report while preserving route, damage/ejection, public-safe proxy and landscape phone proof lines. | PC investor route evidence refresh |

## 4.1 Immediate Micro-Queue

这张表是接下来最应该照着做的“短步队列”。每一行都应尽量对应一个小验证点；只有达到 Gate 才进入下一行。

| Step | Status | Action | Files | Gate |
| --- | --- | --- | --- | --- |
| P0 | Done by this doc | 更新当前主计划和细计划到 v3 | `docs-ai-rts-commander-current-master-plan-2026-06-07.md`; `docs-ai-rts-commander-current-detailed-plan-2026-06-07.md` | `git diff --check` |
| B1.1 | Done | 完成参考视觉导出 manifest 字段：assetClass、provenance、generatedPaths、materialIds、textureRecords、nodeBuckets、missingSources、warnings | `scripts/content-pack/export_tgl_to_obj.py`; `scripts/content-pack/export_terrain_texture_audit.ps1` | `python -m py_compile` |
| B1.2 | Done | 修正 `export_reference_visual_pack.ps1 -Names` 对逗号分隔名称的展开，并保持 `-IncludeMissionProps` 能同时导出单位和道具 | `scripts/content-pack/export_reference_visual_pack.ps1` | PowerShell syntax check |
| B1.3 | Done | 跑七个首图单位导出验证 | exporter scripts | `export_reference_visual_pack.ps1 -Names werewolf,bushwacker,centipede,harasser,lrmc,urbanmech,starslayer` reports 7 exports and 0 missing sources |
| B1.4 | Done | 跑缺失源 probe，确认 warning 清晰、manifest 不破 | exporter scripts | missing probe reports 0 exports, 1 missing source, generated manifest |
| B1.5 | Done | 跑地形纹理 audit/export，确认 terrain texture manifest 也带 provenance/assetClass | `scripts/content-pack/export_terrain_texture_audit.ps1` | `export_terrain_texture_audit.ps1 -ExportReferenceTextures` reports expected texture manifest |
| B1.6 | Done | 恢复本地完整 unit+prop ignored manifest，避免后续 Unity capture 只剩道具或只剩单位 | exporter scripts | `export_reference_visual_pack.ps1 -Names ... -IncludeMissionProps` contains units and mission props |
| B1.7 | Done | 更新参考视觉恢复计划，记录 B1 manifest v2 字段、验证命令和 private-development-only 边界 | `docs-reference-visual-restoration-plan.md` | `git diff --check` |
| B1.8 | Done | 检查 generated private derivatives 未进入 git，提交 B1 | scripts + docs only | `git status --short --branch --untracked-files=all`; commit `Stabilize reference visual manifest export` |
| B2.1 | Done | 审计 Unity 三个 reference loader 读 manifest 的字段和 fallback 逻辑 | `ReferenceObjMeshLibrary.cs`; `ReferencePropLibrary.cs`; `ReferenceTerrainTextureLibrary.cs` | docs note or code diff identifies exact fallback path |
| B2.2 | Done | Loader 优先使用 manifest，并把缺失 manifest、缺失 OBJ、缺失 texture 的日志分清楚 | Unity reference loader files | Unity build |
| B2.3 | Done | 做 manifest-missing fallback capture，证明没私有包也不会崩 | Unity build + capture scripts | `spawn,airfield` capture passes with fallback |
| B2.4 | Done | 提交 B2 | Unity loader files + docs | commit `Harden Unity reference visual loader` |
| B3.1 | Done | 定义 stable visual id 命名规则：unit、prop、terrain、texture、fx、ui | `docs-content-pack.md`; `docs-content-replacement-plan.md` | docs check |
| B3.2 | Done | 补一个 project-owned visual slice 示例，不含私有路径和旧专有名称 | `content-packs/project-owned-visual-slice.example.json` if needed | public boundary dry run |
| B3.3 | Done | 提交 B3 | content docs/example | commit `Document replaceable visual ids` |
| C1.1 | Done | 跑 visible-flow：机库、出战、战斗、损伤、胜利、战报、维修、回机库、重开 | visible-flow command file + docs | player smoke exits 0 |
| C1.2 | Done | 修 walkthrough 到非开发人员能照着演示 | `docs-playable-demo-walkthrough-2026-06-07.md` | docs check |
| C2.1 | Done | 重跑六截图证据并刷新 investor evidence | capture scripts + evidence docs | six captures pass |
| D1.1 | Done | 审计 `project-owned-text-safe-slice` 与 `project-owned-visual-slice`，列出 art-safe mission slice 需要合并和新增的字段 | content-pack examples + docs | fields checklist is explicit |
| D1.2 | Done | 新建或更新 `project-owned-art-safe-slice.example.json`，只放项目自有 id、占位路径和 provenance，不放私有引用 | `content-packs/project-owned-art-safe-slice.example.json` | `python -m json.tool` |
| D1.3 | Done | 更新 content pack/replacement 文档，说明 text-safe、visual-id、art-safe、runtime pack 四者区别 | `docs-content-pack.md`; `docs-content-replacement-plan.md` | docs check |
| D1.4 | Done | 跑 public boundary 检查，必要时加强 checker 对新 manifest 字段的扫描 | content-pack checker + example | `Result: OK` |
| D1.5 | Done | 更新主计划/细计划/README/证据页，明确当前 dev build 仍非 public-safe | README + plan/evidence docs | `git diff --check` |
| D1.6 | Done | 提交 D1 | docs + metadata only | commit `Prepare public art-safe mission slice` |
| E1.1 | Done | 守住 AI 副官慢频高层决策，不花 smoke token | AI contract docs/code if needed | validator/no-key path |
| F1.1 | Done | 写主服务器奖励权威契约，定义 claim/grant、签名、重放校验、ledger 边界 | platform docs | `git diff --check` |
| H1.1 | Done | 写换机交接计划，明确旧机推送、新机克隆、Unity 版本、fallback 验证、私有参考视觉和 AI key 边界 | handoff docs | `git diff --check` |
| H2.1 | Done | 代码已推 `ai-origin master`；validator/build/visible-flow smoke 已过 | git + Unity | clean status + expected success strings |
| G1.1 | Done | 更新计划为移动端第一优先，新增 mobile-first plan，地图/平台契约后移 | docs | `git diff --check` |
| G2.1 | Done | 补 `BuildAndroid` editor entry 和 `BUILD-MOBILE.md`，缺模块时给明确错误 | `Mc2DemoBuilder.cs`; mobile docs | preflight reports Android Build Support blocker clearly |
| G2.2 | Done | 安装/确认 Unity `6000.4.7f1` Android Build Support、Android SDK & NDK Tools、OpenJDK | local Unity AndroidPlayer module | AndroidPlayer, SDK, NDK and OpenJDK paths exist |
| G2.3 | Done | 重新跑 Windows/BattleCore validator，确认移动构建前规则基线没变 | ignored validator log | `MC2 demo contract validation OK` |
| G2.4 | Done | 执行 Android batch build，产出 ignored APK | `unity-mc2-demo/Builds/Android/MC2UnityDemo.apk` | log contains `Build Finished, Result: Success` and `MC2 Unity demo Android build OK` |
| G2.5 | Done | 检查 Unity import/build 后的 git diff，只保留真实源码或文档变化 | source/docs only | no APK/AAB/log/sidecar/build output staged |
| G2.6 | Done | 如 Android build 暴露真实 project setting 需求，用最小提交固化 | `Mc2DemoBuilder.cs`; `ProjectSettings.asset` | Android build passes |
| G2.7 | Done | 更新计划状态：G2 Done，G3 Next | plan docs | `git diff --check` |
| G3.1 | Waiting on Device | 真机启动 Android Demo，记录命令流、UI、日志和设备表现 | `scripts/unity/android_device_smoke.ps1`; device + ignored logs | smoke path reaches battle/debrief |
| PC1.1 | Done | 重跑 PC validator、Windows build、visible-flow smoke 和六截图，锁定下一处 PC 可见问题 | validator/build/capture scripts; optional evidence docs | `PC1` commands pass |
| PC2.1 | Done | 根据 PC1 证据先修地形、水域、岸线、道路/跑道和可战斗陆地区域可读性 | Unity presentation + capture scripts | Windows build + selected captures |
| PC3.1 | Done | 打磨 PC MechLab 整块格子和合法性表现，不恢复武器开关 | `Mc2DemoBootstrap.cs`; loadout files if needed | `mechlab` capture |
| PC4.1 | Done | 刷新 PC controlled demo walkthrough/evidence，不提交生成物 | walkthrough/evidence docs | `git diff --check` |
| PC5.1 | Done | 增加 Windows demo 启动预检脚本和文档入口 | `scripts/unity/run_windows_demo.ps1`; `BUILD-WIN.md`; README/plans | `run_windows_demo.ps1 -CheckOnly` |
| PC6.1 | Done | 增加受控演示证据健康检查脚本和文档入口 | `scripts/unity/check_controlled_demo_evidence.ps1`; `BUILD-WIN.md`; README/plans | `check_controlled_demo_evidence.ps1` |
| PC7.1 | Done | 增加受控演示公开边界预检脚本和文档入口 | `scripts/content-pack/check_controlled_demo_public_boundary.ps1`; `BUILD-WIN.md`; README/plans/evidence | `check_controlled_demo_public_boundary.ps1` |
| PC8.1 | Done | 增加受控演示总预检脚本和文档入口 | `scripts/unity/check_controlled_demo_readiness.ps1`; `BUILD-WIN.md`; README/plans/evidence | `check_controlled_demo_readiness.ps1` |
| PC9.1 | Done | 增加受控演示交接一致性检查脚本和文档入口 | `scripts/unity/check_controlled_demo_handoff.ps1`; `BUILD-WIN.md`; README/plans/evidence/handoff | `check_controlled_demo_handoff.ps1` |
| PC10.1 | Done | 增加 Android 真机 smoke 前置检查脚本和移动文档入口 | `scripts/unity/check_android_device_preflight.ps1`; `BUILD-MOBILE.md`; README/mobile plans | `check_android_device_preflight.ps1 -AllowNoDevice` |
| PC11.1 | Done | 增加 PC 核心玩法合约检查脚本和文档入口 | `scripts/unity/check_pc_core_playable_contract.ps1`; `Mc2DemoValidator.cs`; `BUILD-WIN.md`; README/plans/evidence/handoff | `check_pc_core_playable_contract.ps1` |
| PC12.1 | Done | 增加移动指挥模型预检，确认 PC 演示仍保持默认全队、状态栏、Jet、地图/系统、稀疏 HUD 和无武器开关 | `scripts/unity/check_mobile_command_model_preflight.ps1`; `BUILD-WIN.md`; README/plans/evidence/handoff | `check_mobile_command_model_preflight.ps1` |
| PC13.1 | Done | 增加当前计划 gate 总预检，串联交接/readiness、移动指挥模型和 Android 等设备状态 | `scripts/unity/check_current_plan_gate.ps1`; `BUILD-WIN.md`; README/plans/evidence/handoff | `check_current_plan_gate.ps1` |
| PC14.1 | Done | 增加 Android smoke log 崩溃扫描并接入真机 smoke/current gate | `scripts/unity/check_android_smoke_log.ps1`; `scripts/unity/android_device_smoke.ps1`; `scripts/unity/check_current_plan_gate.ps1`; docs | `check_android_smoke_log.ps1 -SelfTest` |
| PC15.1 | Done | 给 Android 真机 smoke 增加 `-PlanOnly` 预演模式并接入 current gate | `scripts/unity/android_device_smoke.ps1`; `scripts/unity/check_current_plan_gate.ps1`; docs | `android_device_smoke.ps1 -PlanOnly` |
| PC16.1 | Done | 增加战斗 HUD 稀疏合约检查并接入 current gate，明确普通战斗保持状态行、紧凑目标、关闭任务地图、隐藏大日志/存档/账号/调试覆盖层 | `scripts/unity/check_battle_hud_sparse_contract.ps1`; `scripts/unity/check_current_plan_gate.ps1`; `Mc2DemoBootstrap.cs`; `capture_reference_visuals.ps1`; docs | `check_battle_hud_sparse_contract.ps1` |
| PC17.1 | Done | 增加源码/生成物卫生检查并接入 current gate，确认 tracked/staged 路径和 `.gitignore` 不会放行本地证据、Unity build、APK/AAB 或私有参考导出 | `scripts/unity/check_demo_source_hygiene.ps1`; `scripts/unity/check_current_plan_gate.ps1`; docs | `check_demo_source_hygiene.ps1` |
| PC18.1 | Done | 增加 AI 副官边界检查并接入 current gate，确认 MiniMax 只做慢频高层 directive、无 key fallback、默认 smoke 不花 token、frame loop 不调用模型 | `scripts/unity/check_ai_deputy_contract.ps1`; `scripts/unity/check_current_plan_gate.ps1`; docs | `check_ai_deputy_contract.ps1` |
| PC19.1 | Done | 增加 Windows 演示构建新鲜度检查并接入 readiness，确认 ignored player 输出不落后于 tracked Unity build 输入 | `scripts/unity/check_windows_demo_build_freshness.ps1`; `scripts/unity/check_controlled_demo_readiness.ps1`; docs | `check_windows_demo_build_freshness.ps1` |
| PC20.1 | Done | 增强受控演示 evidence check，确认 visible-flow 日志和六截图 PNG/JSON 晚于当前 build/证据输入，并刷新本地 ignored 证据 | `scripts/unity/check_controlled_demo_evidence.ps1`; ignored evidence outputs; docs | `check_controlled_demo_evidence.ps1` |
| PC21.1 | Done | 增强受控演示 evidence check，确认六个标准 capture 日志存在、晚于当前 build/capture helper，并包含 preset、截图请求和 sidecar 写入标记 | `scripts/unity/check_controlled_demo_evidence.ps1`; docs | `check_controlled_demo_evidence.ps1` |
| PC22.1 | Done | 增加 Android APK 新鲜度检查，确认 ignored APK 晚于 tracked Unity build 输入，并接入 Android preflight、device smoke 和 current plan gate | `scripts/unity/check_android_apk_freshness.ps1`; Android smoke scripts; docs; ignored APK output | `check_android_apk_freshness.ps1` |
| PC23.1 | Done | 增加 Android APK 身份检查，确认 package/activity 与 G3 预期安装启动入口一致，并接入 Android preflight、device smoke 和 current plan gate | `scripts/unity/check_android_apk_identity.ps1`; Android smoke scripts; docs | `check_android_apk_identity.ps1` |
| PC24.1 | Done | 增加 Android APK 兼容性检查，确认 minSdk、targetSdk 与 native ABI 符合 G3 目标，并接入 Android preflight、device smoke 和 current plan gate | `scripts/unity/check_android_apk_compatibility.ps1`; Android smoke scripts; docs | `check_android_apk_compatibility.ps1` |
| PC25.1 | Done | 增加 Android APK 签名检查，确认 apksigner verify、v2 签名和 debug signer DN 符合 G3 安装前置条件，并接入 Android preflight、device smoke 和 current plan gate | `scripts/unity/check_android_apk_signing.ps1`; Android smoke scripts; docs | `check_android_apk_signing.ps1` |
| PC26.1 | Done | 增加 Android APK 清单检查，确认权限白名单、required feature、not-required feature 和 screen support 符合 G3 安装前置条件，并接入 Android preflight、device smoke 和 current plan gate | `scripts/unity/check_android_apk_manifest.ps1`; Android smoke scripts; docs | `check_android_apk_manifest.ps1` |
| PC27.1 | Done | 增加 Android APK 载荷检查，确认 Unity/IL2CPP native libraries、`assets/bin/Data` 和 `arm64-v8a` ABI 目录符合 G3 安装前置条件，并接入 Android preflight、device smoke 和 current plan gate | `scripts/unity/check_android_apk_payload.ps1`; Android smoke scripts; docs | `check_android_apk_payload.ps1` |
| PC28.1 | Done | 增加 Android APK 包体预算检查，确认 APK 未低于合理下限且不超过当前 100 MiB 早期移动 Demo 预算，并接入 Android preflight、device smoke 和 current plan gate | `scripts/unity/check_android_apk_size_budget.ps1`; Android smoke scripts; docs | `check_android_apk_size_budget.ps1` |
| PC29.1 | Done | 增加 Android SDK 工具链检查，确认 Unity AndroidPlayer SDK、NDK、OpenJDK、build-tools、platform、adb、aapt 和 apksigner 可用，并接入 Android preflight、device smoke 和 current plan gate | `scripts/unity/check_android_sdk_tooling.ps1`; Android smoke scripts; docs | `check_android_sdk_tooling.ps1` |
| PC30.1 | Done | 增加 Android smoke 生成物卫生检查，确认 APK/AAB、log、截图和 `Builds/Android` 输出不会被 tracked/staged 路径误带入提交，并接入 Android preflight、current plan gate 和 handoff consistency | `scripts/unity/check_android_smoke_artifact_hygiene.ps1`; Android preflight/current gate/handoff scripts; docs | `check_android_smoke_artifact_hygiene.ps1` |
| PC31.1 | Done | 增加 Android smoke 截图证据捕获，`android_device_smoke.ps1` 在真实设备上写入 ignored `analysis-output\android-device-smoke.png`，并让 `-PlanOnly` 输出截图路径和 `ScreenshotCapture: True` | `scripts/unity/android_device_smoke.ps1`; `scripts/unity/check_current_plan_gate.ps1`; docs | `android_device_smoke.ps1 -PlanOnly` |
| PC32.1 | Done | 增加 Android smoke 摘要证据输出，真实设备 smoke 写入 ignored `analysis-output\android-device-smoke-summary.json`，并让 `-PlanOnly` 输出 summary 路径和 `SummaryWrite: True` | `scripts/unity/android_device_smoke.ps1`; `scripts/unity/check_current_plan_gate.ps1`; docs | `android_device_smoke.ps1 -PlanOnly` |
| PC33.1 | Done | 增加 Android smoke 摘要 schema 检查，真实设备 smoke 写入 summary 后自动校验字段、包名、时间戳、证据路径和执行标记，并接入 current gate 与 handoff consistency | `scripts/unity/check_android_smoke_summary.ps1`; `scripts/unity/android_device_smoke.ps1`; `scripts/unity/check_current_plan_gate.ps1`; docs | `check_android_smoke_summary.ps1 -SelfTest` |
| PC34.1 | Done | 增加 Android smoke 摘要 preflight 检查，让 `check_android_device_preflight.ps1 -AllowNoDevice` 直接运行 summary schema 自测并报告 `smoke summary schema` | `scripts/unity/check_android_device_preflight.ps1`; docs | `check_android_device_preflight.ps1 -AllowNoDevice` |
| PC35.1 | Done | 增加 Android smoke 预演/前置一致性检查，对比 `android_device_smoke.ps1 -PlanOnly` 和 `check_android_device_preflight.ps1 -AllowNoDevice` 的 package、activity、证据路径、关键开关和 summary schema readiness | `scripts/unity/check_android_smoke_plan_consistency.ps1`; current gate; docs | `check_android_smoke_plan_consistency.ps1` |
| PC36.1 | Done | 增加 Android G3 readiness 检查，把 device preflight、plan/preflight consistency、plan mode、log scanner self-test 和 summary schema self-test 收成直接移动 gate | `scripts/unity/check_android_g3_readiness.ps1`; current gate; docs | `check_android_g3_readiness.ps1` |
| PC37.1 | Done | 增加 Android G3 真机要求检查，运行 strict readiness，确认无授权手机时只能等待设备，有设备时才允许继续真实 smoke | `scripts/unity/check_android_g3_device_requirement.ps1`; current gate; docs | `check_android_g3_device_requirement.ps1` |
| PC38.1 | Done | 增加 PC 视觉截图 sanity 检查，读取六张受控演示 PNG，检查尺寸、体积、颜色数、中心可见性、亮度对比、粉色 fallback 占比和近单色占比 | `scripts/unity/check_pc_visual_capture_sanity.ps1`; current gate; docs | `check_pc_visual_capture_sanity.ps1` |
| PC39.1 | Done | 增加 PC 视觉截图 sanity 自测，生成合格图、纯色坏图和粉色坏图，确认阈值能区分正常截图与典型坏图 | `scripts/unity/check_pc_visual_capture_sanity.ps1 -SelfTest`; current gate; docs | `check_pc_visual_capture_sanity.ps1 -SelfTest` |
| PC40.1 | Done | 增加 PC 截图 sidecar schema 检查，读取六张受控演示 JSON，确认截图路径、尺寸、flow、camera、摘要字段和 referenceAssets 结构完整 | `scripts/unity/check_pc_capture_sidecar_schema.ps1`; current gate; docs | `check_pc_capture_sidecar_schema.ps1` |
| PC41.1 | Done | 增加 PC 截图 preset 契约检查，确认标准六张 preset 在 capture helper、evidence、visual sanity、sidecar schema 和文档中一致 | `scripts/unity/check_pc_capture_preset_contract.ps1`; current gate; docs | `check_pc_capture_preset_contract.ps1` |
| PC42.1 | Done | 增加 PC 截图生成物卫生检查，确认本地参考截图、sidecar、capture log 和视觉 sanity 自测图保持 ignored 且不在 tracked/staged 源码路径中 | `scripts/unity/check_pc_capture_artifact_hygiene.ps1`; current gate; docs | `check_pc_capture_artifact_hygiene.ps1` |
| PC43.1 | Done | 增加 PC 受控窗口尺寸契约检查，确认 launcher 和 capture helper 保持 `1280x720` windowed 参数 | `scripts/unity/check_pc_window_contract.ps1`; current gate; docs | `check_pc_window_contract.ps1` |
| PC44.1 | Done | 增加 PC 启动日志卫生检查，确认 launcher runtime log 固定到 ignored `analysis-output/windows-demo-run.log`，且 launch log 不进入 tracked/staged 路径 | `scripts/unity/check_pc_launch_log_hygiene.ps1`; current gate; docs | `check_pc_launch_log_hygiene.ps1` |
| PC45.1 | Done | 增加 PC 构建输出卫生检查，确认 Windows player 输出固定到 ignored `unity-mc2-demo/Builds/Windows/`，且 player build 不进入 tracked/staged 路径 | `scripts/unity/check_pc_build_artifact_hygiene.ps1`; current gate; docs | `check_pc_build_artifact_hygiene.ps1` |
| PC46.1 | Done | 增加 PC smoke 生成物卫生检查，确认 smoke、validator、build 和 saved-account evidence 输出固定到 ignored `analysis-output/`，且运行证据不进入 tracked/staged 路径 | `scripts/unity/check_pc_smoke_artifact_hygiene.ps1`; current gate; docs | `check_pc_smoke_artifact_hygiene.ps1` |
| PC47.1 | Done | 增加当前计划队列一致性检查，确认 README、BUILD-WIN、主/细/PC/移动/证据/换机文档和 helper scripts 在当时都指向 PC1-PC47、PC47、G3 真机 smoke | `scripts/unity/check_current_plan_queue.ps1`; current gate; handoff docs | `check_current_plan_queue.ps1` |
| PC48.1 | Done | 增加 Android 设备连接诊断检查，确认 adb 可读并能区分无设备、未授权、离线、多设备和 ready 状态 | `scripts/unity/check_android_device_connection.ps1`; current gate; docs | `check_android_device_connection.ps1` |
| PC49.1 | Done | 将 Android 设备连接诊断接入真实 smoke 入口，确认无授权设备时 `android_device_smoke.ps1` 在安装/启动前失败，PlanOnly 输出 `ConnectionCheck: check_android_device_connection.ps1 -RequireDevice` | `scripts/unity/android_device_smoke.ps1`; preflight; current gate; docs | `ConnectionCheck: check_android_device_connection.ps1 -RequireDevice` |
| PC50.1 | Done | 增加 Android smoke 连接 gate 自测，确认无设备时真实 smoke fail-fast 且不会改写 log、截图、summary evidence | `scripts/unity/check_android_smoke_connection_gate.ps1`; current gate; handoff/docs | `Android smoke connection gate check OK` |
| PC51.1 | Done | 增加 Android visible-flow command-file smoke，PlanOnly 暴露 `CommandFileSmoke: True`、`UnityArguments: -mc2CommandFile` 和复盘/装配回流成功 marker，真机 smoke 到位后自动推送并执行脚本 | `scripts/unity/android_device_smoke.ps1`; Android smoke checks; current gate; handoff/docs | `SmokeSuccessMarker: MC2 loadout compact assertion OK` |
| PC52.1 | Done | 增加 Android WPD-only device diagnosis，确认 Windows 只看到 WPD/MTP 手机时仍不能进入 G3 安装/启动 | `scripts/unity/check_android_device_connection.ps1`; current gate; handoff/docs | `WpdOnlyAndroidProbe: True` |
| PC53.1 | Done | 增加 Android ADB setup guidance，输出当前 Windows driver/provider/inf/service 和下一步 ADB 设置提示 | `scripts/unity/check_android_device_connection.ps1`; current gate; handoff/docs | `AdbSetupHint: True` |
| PC54.1 | Done | 增加 Android ADB readiness watch，允许手机侧切 USB 调试或驱动状态时轮询等待 adb `device`，无设备时可 no-device-safe 验证 | `scripts/unity/watch_android_device_connection.ps1`; current gate; handoff/docs | `AdbWatchHint: True` |
| PC55.1 | Done | 增加 Android G3 device status report，写入 ignored JSON 报告并把 G3 ready/waiting/blocker 固化为机器可读证据 | `scripts/unity/write_android_g3_device_status.ps1`; current gate; handoff/docs | `G3DeviceStatusReport: True` |
| PC56.1 | Done | 增加 Android G3 when-ready runner，一条命令等待 adb `device` 就绪后进入真实 smoke；PlanOnly/AllowWaiting 不安装不启动 | `scripts/unity/run_android_g3_when_ready.ps1`; current gate; handoff/docs | `G3WhenReady: True` |
| PC57.1 | Done | 增加 Android ADB driver package probe，只读枚举当前 PnP 手机驱动和已安装驱动包候选，不安装、不改驱动、不启动 APK | `scripts/unity/check_android_adb_driver_package.ps1`; current gate; handoff/docs | `AdbDriverPackageProbe: True` |
| G4.1 | Later | 调整触控命令 UI，保持无框选、稀疏 HUD、状态栏单选 | Unity presentation | mobile smoke |
| G5.1 | Later | 定义移动端性能预算并记录首轮基线 | docs + ignored evidence | budget doc |
| F2.1 | Later | 写地图包/编辑器契约，定义地图元数据、触发图、敌人、奖励引用和验证器边界 | platform docs | `git diff --check` |
| F3-F4 | Later | 只写排行和创作者契约，不先写服务器 | platform docs | `git diff --check` |

## 5. Detailed Execution Tasks

### A1: Audit First Map Visual Readability

**Goal:** 先用证据说清楚“糊成一坨”具体糊在哪里，避免盲目加效果。

**Files:**

- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify if needed: `docs-playable-demo-investor-evidence-2026-06-07.md`

**Steps:**

1. Open current captures:
   - `analysis-output/reference-visual-captures/spawn.png`
   - `analysis-output/reference-visual-captures/airfield.png`
   - `analysis-output/reference-visual-captures/hangar-contact.png`
   - `analysis-output/reference-visual-captures/north-patrol.png`
   - `analysis-output/reference-visual-captures/damage-demo.png`
2. For each screenshot classify:
   - terrain too flat;
   - water/shoreline weak;
   - mech silhouette too small;
   - enemy/friendly colors unclear;
   - structure/prop gray mush;
   - trees too same-color;
   - black occluders too hard;
   - effects stealing focus;
   - UI occlusion.
3. Read the matching `.json` sidecar and record:
   - `SparseBattleUi`;
   - `BattleOccupancy`;
   - `ContactSpread`;
   - `ContactClearance`;
   - `DamageStory` when present.
4. Decide which problem is visual-only and which would require BattleCore changes.
5. Do not touch gameplay code in this task.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- The audit names the top 3 visual causes.
- The next visual task has a precise target.
- No generated screenshot or sidecar is staged.

**Commit:** `Audit first map visual readability`

### A2: Improve Terrain And Water Readability

**Goal:** 让第一图的地表、水域、岸线、跑道/道路在默认镜头下能读成“地图”，不是一块暗色底板。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/DemoTerrainView.cs`
- Modify: `scripts/unity/capture_reference_visuals.ps1`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Implementation Notes:**

- 优先调材质、vertex color、texture blend、water alpha、shore color 和 runway/road contrast。
- 不改变任务坐标、不改变碰撞、不改变相机旋转。
- 不把地面做成单色高饱和，不用大片装饰渐变。
- 水域需要可识别，但不能亮到抢过机甲。
- 岸线和跑道是第一图最重要的地图阅读线索。

**Steps:**

1. Inspect terrain material creation in `Mc2DemoBootstrap`.
2. Add a compact terrain readability summary to capture sidecar if absent.
3. Tune terrain color/texture strength conservatively.
4. Tune water material color/alpha and shore contrast.
5. Keep click and pathfinding untouched.
6. Build Windows player.
7. Capture `spawn,airfield,north-patrol`.
8. Compare screenshots against A1 audit.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-terrain-readability.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,north-patrol
```

**Acceptance:**

- `spawn` 能区分陆地、水域、岸线和主要地表区域。
- `airfield` 能看出跑道/道路和机场结构关系。
-
orth-patrol` 的水域不再像一大片平蓝色或黑蓝色。
- `SparseBattleUi` gate 不回退。

**Commit:** `Improve terrain and water readability`

### A3: Improve Unit Silhouette Readability

**Goal:** 让玩家一眼知道哪些是自己的机甲、哪些是敌方单位、哪些是残骸/损伤单位。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs`
- Modify: `scripts/unity/capture_reference_visuals.ps1`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Implementation Notes:**

- 优先加低调的 ground contact shadow、敌我识别环、team tint 微调。
- 不用大字标签盖在单位上。
- 不靠扩大 UI 解释单位状态。
- 不用 presentation-only offset 假装解决碰撞。
- 单位缩放如要调整，必须同步审计 sidecar 文案，避免和碰撞占位认知冲突。

**Steps:**

1. Inspect `DemoUnitView` reference visual attach and damage cue layers.
2. Add persistent contact shadow under unit.
3. Add low-alpha faction ring or equivalent cue under unit.
4. Ensure destroyed/damaged cues remain readable.
5. Keep section damage cue stronger than faction ring, weaker than active explosion.
6. Build Windows player.
7. Capture `spawn,hangar-contact,damage-demo`.
8. Verify `ContactClearance` still reports `overlaps=0`.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-unit-readability.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,hangar-contact,damage-demo
```

**Acceptance:**

- `spawn` 中玩家小队不用解释也能看出是三台机甲。
- `hangar-contact` 中敌我不会完全融入建筑和特效。
- `damage-demo` 仍能讲断臂、腿瘫、驾驶舱损毁故事。
- 不新增大 HUD 文本。

**Commit:** `Improve unit silhouette readability`

### A4: Improve Structure And Prop Readability

**Goal:** 让建筑、树木、机场道具和目标结构读成地图元素，而不是灰色噪声或黑块。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoStructureView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/ReferencePropLibrary.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Implementation Notes:**

- 优先做结构底座阴影、目标结构轻微高亮、树木/硬道具色阶差异。
- 大型硬道具应继续由 BattleCore occupancy 证明，不靠视觉膨胀。
- 不把所有建筑涂成同一种灰。
- 树木可以作为背景层，但不能把单位吞掉。

**Steps:**

1. Inspect structure and terrain-object creation paths.
2. Add subtle base pad/contact shadow for targetable structures.
3. Tune fallback building/tree/prop colors for contrast.
4. Keep targetable hangar visual distinct but not UI-like.
5. Build Windows player.
6. Capture `airfield,hangar-contact,north-patrol`.
7. Update audit with before/after visual judgment.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-structure-prop-readability.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets airfield,hangar-contact,north-patrol
```

**Completed Evidence:**

```text
analysis-output/unity-build-structure-prop-readability.log: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
capture_reference_visuals.ps1 -Presets airfield,hangar-contact,north-patrol: MC2 reference visual captures passed: 3 preset(s).
StructureReadability: structures=1 structureViews=1 targetable=1 terrainProps=999 hardProps=80 building=32 aircraft=4 vehicle=12 barricade=108 treeObjects=956 visualOnly=yes collision=unchanged blockerGeometry=unchanged.
```

**Acceptance:**

- `airfield` 能看出机场、跑道/道路、建筑群和敌方方向。
- `hangar-contact` 能看出目标建筑的空间位置。
- 大面积树木不再把单位和建筑吞成一团。
- `BattleOccupancy` blocker counts 不回退。

**Commit:** `Improve structure and prop readability`

### A5: Gate First Map Visual Slice

**Goal:** 把“第一图可读”做成脚本门槛，后续 UI、模型、特效改动不能悄悄退回色块状态。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `scripts/unity/capture_reference_visuals.ps1`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Add or strengthen sidecar summary fields:
   - `FirstMapVisual`;
   - `TerrainReadability`;
   - `UnitReadability`;
   - `StructureReadability`.
2. In capture script, assert battle presets still include:
   - nonblank image;
   - sufficient unique colors;
   - sparse battle UI;
   - contact clearance;
   - battle occupancy;
   - first map visual summary.
3. Keep script checks textual and stable; do not overfit to exact PNG bytes.
4. Capture five battle presets.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-first-map-visual-gate.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,north-patrol,damage-demo
```

**Completed Evidence:**

```text
analysis-output/unity-build-first-map-visual-gate.log: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,north-patrol,damage-demo: MC2 reference visual captures passed: 5 preset(s).
FirstMapVisual: all five battle presets report status=ready, terrain=ready, unit=ready, structure=ready, sparseHud=ready, occupancy=ready, contact=separated, visualOnly=yes, pathing=unchanged, collision=unchanged.
```

**Acceptance:**

- 五个 battle presets 通过 capture gate。
- `hangar-contact` 不退化成堆叠。
- `damage-demo` 不丢失损伤故事。
- 默认战斗 UI 仍然稀疏。

**Commit:** `Gate first map visual slice`

### A6: Refresh Demo Evidence After Visual Pass

**Goal:** 在视觉切片稳定后，更新对外沟通用的本地证据页。

**Files:**

- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify: `docs-playable-demo-investor-evidence-2026-06-07.md`
- Modify if needed: `README.md`

**Steps:**

1. Run full capture set:
   - `mechlab`;
   - `spawn`;
   - `airfield`;
   - `hangar-contact`;
   - `damage-demo`;
   -
orth-patrol`.
2. Summarize what each screenshot proves.
3. State honestly which parts remain prototype-only.
4. Keep generated screenshots ignored.
5. Update demo talk track if visual quality improved enough.

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol
```

**Completed Evidence:**

```text
capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol: MC2 reference visual captures passed: 6 preset(s).
docs-playable-demo-investor-evidence-2026-06-07.md now leads with the current visual gate refresh, updated six screenshot beats, current sidecar highlights and a six-step three-minute demo order.
docs-reference-visual-audit-2026-06-07.md records the A6 evidence refresh and keeps private reference visuals marked development-only.
```

**Acceptance:**

- Evidence page matches current screenshots and sidecars.
- It does not imply private reference art is public-safe.
- It gives a clean three-minute demo order.

**Commit:** `Refresh demo evidence after visual pass`

### B1: Stabilize Reference Visual Manifest Export

**Goal:** 把本地参考模型、贴图、道具、地形纹理的导出结果整理成可审计 manifest，未来整包替换更容易。

**Status:** Completed 2026-06-07. Exporter-side manifest work is done; Unity loader behavior remains B2.

**Files:**

- Modify: `scripts/content-pack/export_tgl_to_obj.py`
- Modify: `scripts/content-pack/export_reference_visual_pack.ps1`
- Modify if needed: `scripts/content-pack/export_terrain_texture_audit.ps1`
- Modify: `docs-reference-visual-restoration-plan.md`

**Manifest Should Record:**

- asset id;
- source path;
- generated ignored path;
- asset class: unit, prop, terrain, texture;
- vertex/triangle/node counts;
- material and texture ids;
- helper/node buckets if known;
- provenance note: private-development-only.

**Steps:**

1. Keep `schema = mc2-reference-visual-manifest-v1` if Unity compatibility needs it, but add a separate manifest version field for richer export metadata.
2. Record per-export identity:
   - `assetId`;
   - normalized source name;
   - `assetClass`;
   - `sourcePath`;
   - `generatedPaths`;
   - ignored output root.
3. Record geometry counts:
   - node count;
   - shape/helper counts;
   - vertex count;
   - triangle count.
4. Record material and texture data:
   - material ids;
   - texture ids;
   - copied output path;
   - source texture name;
   - missing texture warning if applicable.
5. Record node buckets:
   - cockpit;
   - left/right arm;
   - left/right leg;
   - torso;
   - helpers;
   - unmatched shape nodes.
6. Add top-level `provenance` that clearly states private-development-only and not-public-safe.
7. Add top-level `requestedAssets`, `exportCount`, `missingSourceCount`, `missingSources`, and `warnings`.
8. Make missing TGL/texture sources produce warnings and manifest entries, not broken docs or Python tracebacks.
9. Fix the PowerShell wrapper so comma-separated `-Names` and `-IncludeMissionProps` can be used together.
10. Add the same provenance/assetClass discipline to terrain texture audit manifests.
11. Rerun the normal unit export, missing-source probe, terrain texture export and final unit+prop export.
12. Update `docs-reference-visual-restoration-plan.md` with the manifest v2 fields and validation evidence.
13. Ensure generated private derivatives remain ignored.

**Validation:**

```powershell
git diff --check
$env:PYTHONDONTWRITEBYTECODE='1'; python -m py_compile scripts/content-pack/export_tgl_to_obj.py
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\export_reference_visual_pack.ps1 -Names werewolf,bushwacker,centipede,harasser,lrmc,urbanmech,starslayer
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\export_reference_visual_pack.ps1 -Names __missing_reference_probe__ -OutputRoot analysis-output\unity-reference-art\missing-source-probe -ManifestPath analysis-output\unity-reference-art\missing-source-probe.json -NoCopyTextures
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\export_terrain_texture_audit.ps1 -ExportReferenceTextures
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\export_reference_visual_pack.ps1 -Names werewolf,bushwacker,centipede,harasser,lrmc,urbanmech,starslayer -IncludeMissionProps
```

**Acceptance:**

- Ignored manifest is generated.
- Seven-unit export reports seven exports and zero missing source shapes.
- Full unit+prop export contains both the first-slice unit assets and mission prop assets.
- Missing-source probe reports a clear missing source warning and still writes a manifest.
- Terrain texture manifest records private-development-only provenance.
- No generated private derivatives are staged.
- Missing source material yields clear warnings, not broken docs.

**Completed Evidence:**

```text
git diff --check -- scripts/content-pack/export_tgl_to_obj.py scripts/content-pack/export_reference_visual_pack.ps1 scripts/content-pack/export_terrain_texture_audit.ps1: passed, with only LF/CRLF warnings.
$env:PYTHONDONTWRITEBYTECODE='1'; python -m py_compile scripts/content-pack/export_tgl_to_obj.py: passed.
PowerShell syntax check for export_reference_visual_pack.ps1 and export_terrain_texture_audit.ps1: passed.
Seven-unit export: Reference visual exports: 7; missing sources: 0.
Missing-source probe: Reference visual exports: 0; missing sources: 1.
Terrain texture audit/export: Terrain reference textures exported: 103.
Final unit+prop export: Reference visual exports: 47; missing sources: 0; manifest has 7 unit entries and 40 prop entries.
Manifest v2 provenance: private-development-only.
```

**Commit:** `Stabilize reference visual manifest export`

### B2: Harden Unity Reference Visual Loader

**Goal:** Unity 读取 manifest 优先，缺失时能回退到开发占位，不因为本地私有素材缺失导致 Demo 无法启动。

**Status:** Completed 2026-06-07. Unity now consumes the B1 manifest v2 fields first and logs missing-manifest, loose-OBJ, primitive, texture, and terrain fallback paths clearly.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferencePropLibrary.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceTerrainTextureLibrary.cs`
- Modify: `docs-reference-visual-restoration-plan.md`

**Steps:**

1. Read current loader code paths and list where each loader obtains OBJ, TGA, prop, and terrain texture paths.
2. Ensure the manifest is the first source of truth for private reference visuals.
3. Keep direct folder probing only as a development fallback and log when it is used.
4. Map Unity unit/prop requests to stable manifest asset ids, not raw legacy file paths.
5. If a manifest entry is missing, keep the demo booting with obvious fallback visuals.
6. If an OBJ exists but texture is missing, load geometry with fallback material and log the missing texture id.
7. If the entire ignored reference art folder is missing, still pass smoke/capture using primitive development visuals.
8. Add sidecar/log evidence that records selected manifest asset ids for at least:
   - commander unit;
   - one enemy unit;
   - one target structure;
   - one terrain texture source or terrain fallback.
9. Build Windows player.
10. Capture `spawn,airfield`.
11. Temporarily point loader to a missing manifest or use a missing-manifest option if available, then run a fallback capture/log check.
12. Restore normal local manifest path before committing.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-reference-loader.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield
```

**Acceptance:**

- Loader log records selected manifest asset ids.
- Missing manifest still boots with clear fallback visuals.
- Public replacement pack can later reuse the same ids.

**Completed Evidence:**

```text
git diff --check -- unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs unity-mc2-demo/Assets/Scripts/Presentation/ReferenceTerrainTextureLibrary.cs: passed, with only LF/CRLF warnings.
analysis-output/unity-build-reference-loader.log: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
capture_reference_visuals.ps1 -Presets spawn,airfield: MC2 reference visual captures passed: 2 preset(s).
Normal manifest logs: Loaded private reference visual manifest schema=mc2-reference-visual-manifest-v1 version=2 exports=47; mapped unit assets werewolf/bushwacker/centipede/urbanmech/harasser/starslayer/lrmc and prop asset hangar.
Terrain manifest logs: Loaded private terrain texture manifest assetClass=terrain-texture-pack textures=103; mapped textureId=2; composite loaded with missingSamples=0.
Manifest-missing fallback capture: MC2 reference visual captures passed: 1 preset(s); logs reported missing visual/terrain manifests, loose OBJ fallback, primitive fallback for unmatched slayerp, and source terrain vertex-color fallback.
Final normal captures were rerun after restoring ignored manifests.
```

**Commit:** `Harden Unity reference visual loader`

### B3: Document Replaceable Visual IDs

**Goal:** 为以后“整包替换”和换皮项目留下稳定 id 体系，不把旧素材路径写死进产品逻辑。

**Status:** Completed 2026-06-07. Stable project-facing visual id rules and a boundary-clean metadata scaffold now exist.

**Files:**

- Modify: `docs-content-pack.md`
- Modify: `docs-content-replacement-plan.md`
- Modify: `docs-reference-visual-restoration-plan.md`
- Create if needed: `content-packs/project-owned-visual-slice.example.json`

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\check_public_content_boundary.ps1 -Path "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\content-packs\project-owned-starter.example.json" -DryRun
python -m json.tool content-packs/project-owned-visual-slice.example.json
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\check_public_content_boundary.ps1 -Path "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\content-packs\project-owned-visual-slice.example.json" -DryRun
```

**Acceptance:**

- Visual ids are project-facing, not legacy-name-facing.
- Docs clearly separate private reference pack and public replacement pack.
- Clean starter boundary still returns `Result: OK`.
- Visual slice scaffold boundary returns `Result: OK`.

**Completed Evidence:**

```text
content-packs/project-owned-visual-slice.example.json added as metadata-only-not-mountable visual id scaffold.
docs-content-pack.md now defines stable id prefixes for units, models, textures, terrain, props, weapon FX, damage FX and UI.
docs-content-replacement-plan.md now records the visual-id artifact under the art-safe vertical slice milestone.
python -m json.tool content-packs/project-owned-visual-slice.example.json: passed.
check_public_content_boundary.ps1 -Path content-packs/project-owned-visual-slice.example.json -DryRun: Result: OK.
```

**Commit:** `Document replaceable visual ids`

### C1: Seal Visible Playable Walkthrough

**Goal:** 确保真实演示时能从机库走到战斗，再到损伤、结算、维修、重开，不靠开发者口头补洞。

**Status:** Completed 2026-06-07. The smoke path reaches combat, Debrief, Debrief summary, repair/Mech Lab, squad relaunch identity and compact loadout review.

**Files:**

- Modify if needed: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-visible-flow-audit.txt`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- Modify: `docs-playable-demo-walkthrough-2026-06-07.md`
- Modify: `docs-playable-demo-investor-evidence-2026-06-07.md`

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-seal.log"
```

**Acceptance:**

- Smoke 到达胜利、战报、维修、回机库、再次出战。
- 不出现保存槽、账号面板、大日志。
- 文档能给非开发人员照着演示。

**Completed Evidence:**

```text
analysis-output/unity-player-visible-flow-seal.log: MC2 demo smoke test exiting with code 0.
Debrief evidence: MC2 debrief resolve OK; MC2 debrief open OK; MC2 debrief summary assertion OK.
Repair/relaunch evidence: actions=Repair & Mech Lab/Next Contract/Retry Battle/Close; MC2 saved account delta; assert-restart-identity depot; MC2 loadout compact assertion OK.
docs-playable-demo-walkthrough-2026-06-07.md now points to the seal log and describes the proven loop.
docs-playable-demo-investor-evidence-2026-06-07.md now leads with the visible-flow seal refresh.
```

**Commit:** `Seal visible playable walkthrough`

### C2: Refresh Investor Evidence Package

**Goal:** 把当前本地 Demo 的可展示能力整理成一页证据，方便后续融资沟通或内部演示。

**Status:** Completed 2026-06-07. Six captures and the current evidence page were refreshed after B1-B3 and C1.

**Files:**

- Modify: `docs-playable-demo-investor-evidence-2026-06-07.md`
- Modify: `README.md`

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol
```

**Acceptance:**

- Evidence page states local screenshot paths and what each proves.
- It clearly says public/commercial build needs clean content pack.
- README points to current evidence and plan without old-game framing.

**Completed Evidence:**

```text
capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol: MC2 reference visual captures passed: 6 preset(s).
docs-playable-demo-investor-evidence-2026-06-07.md now leads with current evidence package refresh and visible-flow seal refresh.
README.md points to current master plan, current detailed plan, walkthrough, investor evidence, text-safe metadata target and visual id metadata target.
content-packs/project-owned-visual-slice.example.json and project-owned-starter.example.json pass public content boundary checks.
```

**Commit:** `Refresh investor evidence package`

### D1: Prepare Public Art-Safe Mission Slice

**Goal:** 把第一张图从私有参考验证路线转成公开可替换路线，但不要求这一阶段就做完所有美术。

**Status:** Completed 2026-06-07 for metadata. The art-safe mission-slice target is clean and boundary-checked; it is not a mountable runtime pack yet.

**Files:**

- Modify: `docs-content-replacement-plan.md`
- Modify: `docs-content-pack.md`
- Create or modify: `content-packs/project-owned-art-safe-slice.example.json`
- Modify if needed: `scripts/content-pack/check_public_content_boundary.ps1`
- Modify if needed: `README.md`
- Modify: `docs-ai-rts-commander-current-master-plan-2026-06-07.md`
- Modify: `docs-ai-rts-commander-current-detailed-plan-2026-06-07.md`

**Slice Should Define:**

- project title and visible names;
- one mission id not tied to legacy marker;
- terrain material set;
- 3 to 4 mech silhouettes;
- common weapon FX;
- common damage FX;
- 3 to 5 structures/props;
- MechLab/status icons;
- provenance field for every public asset.
- explicit metadata-only status until real files exist.

**D1 Subtasks:**

1. Read `content-packs/project-owned-text-safe-slice.example.json` and `content-packs/project-owned-visual-slice.example.json`.
2. List which fields are reused directly and which new fields are needed for an art-safe mission slice.
3. Create `content-packs/project-owned-art-safe-slice.example.json` with one public-facing mission id, project-owned unit ids, terrain material ids, prop ids, weapon FX ids, damage FX ids, UI icon ids and provenance placeholders.
4. Keep every source path either absent, project-owned placeholder, or licensed placeholder; do not include private extraction paths, legacy names, local reference pack markers or development-only labels.
5. Mark the manifest as metadata-only and not mountable until cleared runtime files exist.
6. Run `python -m json.tool` on the new manifest.
7. Run `check_public_content_boundary.ps1 -DryRun` on the new manifest.
8. Update `docs-content-pack.md` and `docs-content-replacement-plan.md` so the four states are clear: text-safe, visual-id scaffold, art-safe metadata, clean runtime pack.
9. Update README/current plans only enough to point at the new D1 artifact.
10. Commit only docs and metadata; no screenshots, logs, OBJ, TGA, PNG, JSON sidecars or Unity build output.

**Validation:**

```powershell
git diff --check
python -m json.tool content-packs\project-owned-art-safe-slice.example.json
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\check_public_content_boundary.ps1 -Path "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\content-packs\project-owned-art-safe-slice.example.json" -DryRun
```

**Acceptance:**

- Clean candidate manifest passes boundary check.
- Docs do not claim the private dev build is public-safe.
- Public replacement work can begin without rewriting loaders.
- The new manifest is explicit about provenance and metadata-only status.
- The next engineer can start real cleared asset production without reading old private-reference notes first.

**Completed Evidence:**

```text
content-packs/project-owned-art-safe-slice.example.json created.
python -m json.tool content-packs\project-owned-art-safe-slice.example.json: passed.
check_public_content_boundary.ps1 -Path content-packs\project-owned-art-safe-slice.example.json -DryRun: Result: OK.
docs-content-pack.md now documents the metadata slice ladder: text-safe, visual-id, art-safe metadata, clean runtime pack.
docs-content-replacement-plan.md now records the art-safe target and validation command.
README and investor evidence now point at the art-safe metadata target while still saying the current screenshots are not public final art.
```

**Commit:** `Prepare public art-safe mission slice`

### E1: Guard AI Deputy Offline Behavior

**Goal:** 保持 AI 副官是大决策工具，不进入逐帧战斗，不让 API 延迟破坏 Demo。

**Files:**

- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/RuleCommander.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `docs-ai-commander-directive-contract.md`

**Directive Boundary:**

- Allowed:
  - `assault-objective`;
  - `engage-hostiles`;
  - `regroup`;
  - `hold`;
  - `withdraw-if-critical`.
- Not allowed:
  - per-frame steering;
  - per-shot weapon selection;
  - per-unit dodge loops;
  - blocking API calls in battle update;
  - token spending in normal smoke.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-deputy-offline.log"
```

**Acceptance:**

- No-key local demo works.
- AI failure falls back to rule commander or no advice.
- Compact observation remains bounded.

**Commit:** `Guard AI deputy offline behavior`

### F1: Document Reward Authority Contract

**Status:** Completed 2026-06-07.

**Goal:** 把“地图服务器可以由合作方搭，但奖励必须由主服务器认证”写成契约。

**Files:**

- Modify: `docs-platform-ecosystem-plan.md`
- Create if needed: `docs-platform-reward-contract-2026-06-07.md`

**Contract Should Define:**

- client submits reward claim, not reward grant;
- main server owns inventory, token ledger, reward table and rankings;
- map server can host map/session and submit signed result;
- claim includes account id, squad hash, map id/version, seed, result summary, timeline digest;
- high-value claims can require replay or deterministic re-simulation.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Map server cannot mint portable rewards.
- Main server can reject or cap suspicious claims.
- Contract does not require chain in first platform version.

**Completed Evidence:**

```text
docs-platform-reward-contract-2026-06-07.md defines actors, reward lifecycle, session ticket, reward claim payload, validation gates, claim states, grant calculation, ledger rules, rejection/capping examples, ranking publication and first implementation slice.
docs-platform-ecosystem-plan.md links to the detailed reward authority contract.
README.md lists the reward authority contract under key docs.
```

**Commit:** `Document reward authority contract`

### H1: Prepare Machine Handoff Plan

**Status:** Completed 2026-06-07.

**Goal:** 把换机开发前后必须做的事情写成可执行交接计划，避免新机器缺提交、缺 Unity 版本、缺 smoke 验证，或者把 ignored 私有资料误提交。

**Files:**

- Create: `docs-machine-handoff-plan-2026-06-07.md`
- Modify: `README.md`
- Modify: `docs-ai-rts-commander-current-master-plan-2026-06-07.md`
- Modify: `docs-ai-rts-commander-current-detailed-plan-2026-06-07.md`

**Completed Evidence:**

```text
docs-machine-handoff-plan-2026-06-07.md defines source push/copy, Unity 6000.4.7f1 setup, clone/import, clean fallback validator/build/smoke, optional private reference visual transfer, optional MiniMax environment variables, and stop conditions.
README.md lists the machine handoff plan under key docs.
Current master and detailed plans put H2 and the completed mobile-first gate before F2.
```

**Validation:**

```powershell
git diff --check
```

**Commit:** `Prepare machine handoff plan`

### H2: Push Machine Handoff Checkpoint

**Status:** Completed 2026-06-11.

**Goal:** 在真正换电脑前，把当前本机 `master` 同步到新机器能拿到的位置，并在新机器证明 Unity Demo 仍能跑。

**Files:**

- Modify: none expected
- Read: `docs-machine-handoff-plan-2026-06-07.md`
- Read: `BUILD-WIN.md`
- Read: `unity-mc2-demo/README.md`

**Steps:**

1. On the old machine, run:

```powershell
git status --short --branch --untracked-files=all
git log --oneline -5
```

2. Push if GitHub is available:

```powershell
git push ai-origin master
```

3. If push is not available, copy the whole repository with `.git`; do not stage ignored evidence.
4. On the new machine, clone/open the repository and confirm the handoff commit is visible.
5. Install Unity `6000.4.7f1` with Windows Build Support, or explicitly accept a compatible Unity 6 editor.
6. Run the validator, Windows build, and visible-flow smoke from `docs-machine-handoff-plan-2026-06-07.md`.

**Validation:**

```powershell
git status --short --branch --untracked-files=all
```

Expected after push:

```text
## master...ai-origin/master
```

Expected Unity strings on the new machine:

```text
MC2 demo contract validation OK
Build Finished, Result: Success
MC2 Unity demo Windows build OK
MC2 demo smoke test exiting with code 0
```

**Acceptance:**

- New machine can see the latest handoff commit.
- Normal validator and smoke do not require `MINIMAX_API_KEY`.
- Optional private reference visuals remain ignored and local-only.
- No generated screenshot, log, JSON sidecar, Unity build output, or private reference export is staged.

**Commit:** none expected after the push unless the new machine reveals a real doc/setup correction.

### G1: Reframe Plan Around Mobile First

**Status:** Completed 2026-06-10.

**Goal:** 把移动端支持提到第一产品优先级，明确 Unreal MCP 不进入主线，地图包、Web 排行和创作者生态后移到移动端可行性通过之后。

**Files:**

- Create: `docs-mobile-first-plan-2026-06-10.md`
- Modify: `README.md`
- Modify: `docs-ai-rts-commander-current-master-plan-2026-06-07.md`
- Modify: `docs-ai-rts-commander-current-detailed-plan-2026-06-07.md`
- Modify: `docs-machine-handoff-plan-2026-06-07.md`

**Completed Evidence:**

```text
docs-mobile-first-plan-2026-06-10.md defines Android build smoke, Android device smoke, touch command UI, mobile performance budget, iOS feasibility and deferred platform/server work.
README.md states Unity remains the main engine and mobile comes before map editor/ranking/creator ecosystem.
Current master and detailed plans completed G2-G6 before resuming F2-F4.
```

**Validation:**

```powershell
git diff --check
```

**Commit:** `Reframe plan around mobile first`

### G2: Add Android Build Smoke Path

**Status:** Completed 2026-06-11. `BuildAndroid` entry, Android tool path configuration, Unity Android Build Support, SDK/NDK/JDK/CMake dependencies and APK output are verified.

**Goal:** 证明当前 Unity 6 Demo 能构建 Android 包，先拿到移动端构建链路，再谈触控优化和性能。

**Files:**

- Modify if needed: `unity-mc2-demo/Assets/Editor/Mc2DemoBuilder.cs`
- Modify if needed: `unity-mc2-demo/README.md`
- Modify if needed: `BUILD-MOBILE.md`
- Modify if needed: `docs-mobile-first-plan-2026-06-10.md`
- Modify if needed: `docs-ai-rts-commander-current-master-plan-2026-06-07.md`
- Modify if needed: `docs-ai-rts-commander-current-detailed-plan-2026-06-07.md`
- Read: `docs-mobile-first-plan-2026-06-10.md`

**Preconditions:**

1. Worktree is clean or contains only this G2 documentation/source change:

```powershell
git status --short --branch --untracked-files=all
```

2. Unity editor exists:

```powershell
Test-Path "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe"
```

Expected:

```text
True
```

3. Android module exists:

```powershell
Test-Path "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Data\PlaybackEngines\AndroidPlayer"
```

Expected before continuing:

```text
True
```

If this returns `False`, stop G2 code work and install Android Build Support, Android SDK & NDK Tools, and OpenJDK from Unity Hub for Unity `6000.4.7f1`.

**Executable Steps:**

1. Confirm the module folder is present.
2. Run the Windows/BattleCore validator:

```powershell
$Repo = (Get-Location).Path
$Unity = "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe"
& $Unity `
  -batchmode -quit `
  -projectPath "$Repo\unity-mc2-demo" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract `
  -logFile "$Repo\analysis-output\unity-validate-mobile-baseline.log"
```

Expected:

```text
MC2 demo contract validation OK
```

3. Run Android build:

```powershell
$Repo = (Get-Location).Path
$Unity = "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe"
& $Unity `
  -batchmode -quit `
  -projectPath "$Repo\unity-mc2-demo" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildAndroid `
  -logFile "$Repo\analysis-output\unity-build-android.log"
```

Expected:

```text
Build Finished, Result: Success
MC2 Unity demo Android build OK
```

4. Confirm artifact:

```powershell
Test-Path .\unity-mc2-demo\Builds\Android\MC2UnityDemo.apk
```

Expected:

```text
True
```

5. Inspect git status and diff after Unity exits:

```powershell
git status --short --branch --untracked-files=all
git diff -- .\unity-mc2-demo\ProjectSettings .\unity-mc2-demo\Assets\Scenes .\unity-mc2-demo\Assets\Editor
```

6. If Unity changed scene fileIDs only, restore that churn by manual `apply_patch`.
7. If Unity changed real Android project settings, keep only the minimal tracked settings required for repeatable Android build and rerun validator/build.
8. Update `docs-mobile-first-plan-2026-06-10.md` and this file only if the actual command, output path, success string, or blocker changed.

**Validation:**

```powershell
git diff --check
git status --short --branch --untracked-files=all
Select-String -Path .\analysis-output\unity-validate-mobile-baseline.log -Pattern "MC2 demo contract validation OK"
Select-String -Path .\analysis-output\unity-build-android.log -Pattern "Build Finished, Result: Success","MC2 Unity demo Android build OK"
Test-Path .\unity-mc2-demo\Builds\Android\MC2UnityDemo.apk
```

**Acceptance:**

- Android module prerequisite is explicit.
- Android APK is generated under ignored build output.
- Validator still passes before/after Android build setup.
- `BUILD-MOBILE.md` is enough for G3 to install the artifact.
- `git status` is clean after commit, or only contains intentionally unstaged ignored outputs.
- No APK/AAB, Unity build output, generated logs, screenshots, JSON sidecars, or private reference exports are staged.

**Failure Handling:**

- Missing Android module: stop and install module; do not change gameplay or UI.
- Android build fails with SDK/NDK/JDK error: fix Unity module/toolchain path first.
- Android build fails with project setting error: make the smallest project/editor change and rerun validator/build.
- Any unexpected source diff: inspect before staging; do not use broad reset.

**Commit:** `Add Android build smoke path`

**Completed Evidence:**

```text
analysis-output/unity-validate-mobile-baseline.log: MC2 demo contract validation OK
analysis-output/unity-build-android.log: MC2 Android tools configured
analysis-output/unity-build-android.log: Build Finished, Result: Success.
analysis-output/unity-build-android.log: MC2 Unity demo Android build OK: ...\unity-mc2-demo\Builds\Android\MC2UnityDemo.apk
unity-mc2-demo\Builds\Android\MC2UnityDemo.apk exists, 20,666,724 bytes, ignored output.
```

### G3: Run Android Device Smoke

**Status:** Waiting on Device.

**Goal:** 在真 Android 设备上确认 Demo 可以启动并跑到战斗/战报核心路径。

**Files:**

- Modify if needed: `scripts/unity/android_device_smoke.ps1`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- Generate ignored output only: `analysis-output/`

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1
git diff --check
```

**Current Evidence:**

```text
scripts\unity\android_device_smoke.ps1 added.
The helper discovers package/activity through aapt, checks adb authorization,
installs the APK, launches it, waits briefly, captures logcat, and fails if the
package does not stay running.
Current adb state on 2026-06-11: no device rows; physical phone still required.
```

**Commit:** `Document Android device smoke results`

### PC1: Audit Current PC Baseline

**Status:** Completed 2026-06-11.

**Goal:** 在 G3 等待 Android 真机期间，重跑 PC 端完整基线，确认当前 Windows Demo 的真实状态，并只选择一个下一步最高影响优化点。

**Files:**

- Read: `docs-pc-optimization-plan-2026-06-11.md`
- Modify if needed: `docs-reference-visual-audit-2026-06-07.md`
- Modify if needed: `docs-playable-demo-investor-evidence-2026-06-07.md`
- Modify if needed: `scripts/unity/capture_reference_visuals.ps1`
- Generate ignored output only: `analysis-output/`

**Preconditions:**

```powershell
git status --short --branch --untracked-files=all
Test-Path "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe"
```

Expected:

```text
## master...ai-origin/master
True
```

**Executable Steps:**

1. Run the mission validator.
2. Run the Windows player build.
3. Run visible-flow smoke.
4. Run standard captures: `mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol`.
5. Inspect ignored sidecars enough to classify the next PC polish target:
   - battle readability;
   - contact/collision presentation;
   - sparse HUD regression;
   - MechLab readability;
   - startup/walkthrough evidence.
6. Update evidence docs only if current screenshots or sidecars change the written judgment.

**Validation:**

```powershell
git diff --check
$Repo = (Get-Location).Path
$Unity = "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe"
& $Unity -batchmode -quit -projectPath "$Repo\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "$Repo\analysis-output\unity-validate-pc-baseline.log"
& $Unity -batchmode -quit -projectPath "$Repo\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "$Repo\analysis-output\unity-build-pc-baseline.log"
& "$Repo\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "$Repo\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "$Repo\analysis-output\unity-player-pc-visible-flow-baseline.log"
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol
git status --short --branch --untracked-files=all
```

**Expected Strings:**

```text
MC2 demo contract validation OK
Build Finished, Result: Success
MC2 Unity demo Windows build OK
MC2 demo smoke test exiting with code 0
MC2 reference visual captures passed
```

**Acceptance:**

- PC baseline has fresh validator/build/smoke/capture evidence.
- Generated logs, screenshots, sidecars and builds remain ignored.
- The next PC optimization target is written down before changing visuals.
- No mobile G4/G5 work starts before G3 has a real device.

**Completed Evidence:**

```text
analysis-output/unity-validate-pc-baseline.log: MC2 demo contract validation OK.
analysis-output/unity-build-pc-baseline.log: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
analysis-output/unity-player-pc-visible-flow-baseline.log: MC2 demo smoke test exiting with code 0.
capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol: MC2 reference visual captures passed: 6 preset(s).
docs-reference-visual-audit-2026-06-07.md records the PC1 baseline and selects PC2 terrain/water/land readability as the next target.
```

**Commit:** `Audit PC demo baseline`

### PC2: Polish PC Battle Readability

**Status:** Completed 2026-06-11.

**Goal:** 根据 PC1 证据只修一处最高影响的战场可读性问题：地形、水域、岸线、道路/跑道和可战斗陆地区域在默认 PC 镜头下必须更清楚，避免无边界地改 UI 或美术。

**Files:**

- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/DemoTerrainView.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/DemoStructureView.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `scripts/unity/capture_reference_visuals.ps1`
- Modify if needed: `docs-reference-visual-audit-2026-06-07.md`

**Acceptance:**

- Terrain/water/roads/buildings/units are more readable in the selected capture.
- `hangar-contact` does not regress into true overlap or visual pile.
- Battle UI remains sparse.
- The fix is presentation-level unless evidence proves a BattleCore rule bug.

**Completed Evidence 2026-06-11:**

```text
analysis-output/unity-build-pc-terrain-readability.log: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol: MC2 reference visual captures passed: 5 preset(s).
TerrainReadability: texture=composite textureStrength=0.28 waterSurface=readable-overlay alpha=0.48 style=land-outline+runway-contrast+water-muted pathing=unchanged.
ContactClearance: all five captures kept overlaps=0 status=separated.
```

**Commit:** `Polish PC battle readability`

### PC3: Polish PC MechLab Flow

**Status:** Completed 2026-06-11.

**Goal:** 让 PC 装配界面更直观地表达整块武器、热量、重量、装甲板和散热器；武器装上即启用，不做启用/关闭。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutContract.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- Modify if needed: `scripts/unity/capture_reference_visuals.ps1`

**Acceptance:**

- Whole weapon block shapes are obvious.
- Heat/weight/slot legality can be judged from the panel.
- Armor/sink fillers remain simple one-cell decisions.
- No weapon enable/disable toggle returns.
- `mechlab` capture has no text overlap at 1280x720.

**Completed Evidence 2026-06-11:**

```text
analysis-output/unity-build-pc-mechlab.log: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
capture_reference_visuals.ps1 -Presets mechlab: MC2 reference visual captures passed: 1 preset(s).
MechLabCapture: layout=pressure-cards+whole-blocks+single-fillers, pressure=H 12/22 W 16/16 G 12/16, alwaysMounted=weapons 6/6 items 6/6 noToggle=yes.
```

**Commit:** `Polish PC MechLab flow`

### PC4: Package PC Controlled Demo Evidence

**Status:** Completed 2026-06-12.

**Goal:** PC 可展示质量收稳后，刷新外部演示脚本和证据页。

**Files:**

- Modify: `docs-playable-demo-walkthrough-2026-06-07.md`
- Modify: `docs-playable-demo-investor-evidence-2026-06-07.md`
- Modify if needed: `README.md`

**Validation:**

```powershell
git diff --check
git status --short --branch --untracked-files=all
```

**Completed Evidence:**

```text
analysis-output/unity-build-pc-evidence-package.log: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
analysis-output/unity-player-pc-evidence-visible-flow.log: MC2 demo smoke test exiting with code 0; debrief and loadout compact assertions passed.
capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol: MC2 reference visual captures passed: 6 preset(s).
mechlab sidecar: layout=pressure-cards+whole-blocks+single-fillers, alwaysMounted=weapons 6/6 items 6/6 noToggle=yes.
battle sidecars: terrainReadability textureStrength=0.28, FirstMapVisual status=ready, ContactClearance overlaps=0 status=separated, DamageStory includes left-arm-lost, legs-lost and cockpit-lost.
generated screenshots, JSON sidecars, logs and Windows build outputs remain ignored and unstaged.
```

**Commit:** `Package PC controlled demo evidence`

### PC5: Add PC Demo Launch Preflight

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不继续扩大 PC 玩法，只把当前 Windows 受控演示的启动入口收稳。

**Files:**

- Create: `scripts/unity/run_windows_demo.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs

**Acceptance:**

- The helper validates `MC2UnityDemo.exe` and `MC2UnityDemo_Data` exist.
- `-CheckOnly` can be used without opening the player window.
- Default launch uses 1280x720 windowed Unity arguments to avoid depending on stale oversized window state.
- Runtime logs remain under ignored `analysis-output/`.
- No gameplay, BattleCore, HUD, MechLab or content-pack behavior changes.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\run_windows_demo.ps1 -CheckOnly
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add PC demo launch preflight`

### PC6: Add Controlled Demo Evidence Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不继续扩大 PC 玩法，只把当前 Windows 受控演示证据包变成可机器检查的状态。

**Files:**

- Create: `scripts/unity/check_controlled_demo_evidence.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs

**Acceptance:**

- The helper validates the Windows build and Unity data folder exist.
- The helper validates the current visible-flow log contains smoke exit `0`, debrief summary and compact loadout assertions.
- The helper validates six capture PNG/JSON pairs exist and are 1280x720.
- The helper checks MechLab whole-block/no-toggle evidence, terrain readability, sparse HUD, contact separation and damage-demo story markers.
- It reads ignored local evidence only and does not create new artifacts.
- No gameplay, BattleCore, HUD, MechLab, content-pack or generated evidence behavior changes.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_evidence.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add controlled demo evidence check`

### PC7: Add Controlled Demo Public Boundary Preflight

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不继续扩大 PC 玩法，只把当前受控演示的公开内容边界做成可机器检查状态：公开 metadata 示例必须 clean，当前 Windows 开发构建仍必须被识别为 development-only。

**Files:**

- Create: `scripts/content-pack/check_controlled_demo_public_boundary.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs and evidence page

**Acceptance:**

- The helper runs the existing public boundary checker through a child PowerShell process and does not create artifacts.
- The default clean target set covers `project-owned-starter`, `project-owned-text-safe-slice`, `project-owned-visual-slice`, and `project-owned-art-safe-slice`.
- Each clean metadata target must return `Result: OK`.
- With `-CheckDevBuild`, the current Windows development build must return expected `Result: FAILED`.
- No gameplay, BattleCore, HUD, MechLab, content-pack schema or generated evidence behavior changes.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\content-pack\check_controlled_demo_public_boundary.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\content-pack\check_controlled_demo_public_boundary.ps1 -CheckDevBuild
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add controlled demo public boundary preflight`

### PC8: Add Controlled Demo Readiness Preflight

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不继续扩大 PC 玩法，只把受控演示前的启动、证据和公开边界检查收成一条命令。

**Files:**

- Create: `scripts/unity/check_controlled_demo_readiness.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs and evidence page

**Acceptance:**

- The helper runs `run_windows_demo.ps1 -CheckOnly`.
- The helper runs `check_controlled_demo_evidence.ps1`.
- The helper runs `check_controlled_demo_public_boundary.ps1 -CheckDevBuild`.
- It reports one top-level readiness result and per-step OK rows.
- It does not start Unity, rebuild, regenerate screenshots, alter BattleCore, change HUD/MechLab behavior, or stage generated artifacts.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_readiness.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add controlled demo readiness preflight`

### PC9: Add Controlled Demo Handoff Consistency Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不继续扩大 PC 玩法，只把换机和演示前最容易漂移的文档/脚本入口做成可机器检查状态。

**Files:**

- Create: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: `docs-machine-handoff-plan-2026-06-07.md`
- Modify: current plan docs and evidence page

**Acceptance:**

- Checks key controlled demo scripts exist.
- Checks README, BUILD-WIN, current master plan, current detailed plan, PC optimization plan, investor evidence and machine handoff plan all mention the current controlled demo gate set.
- Rejects stale machine-handoff markers such as the old G2 next-task checkpoint, old ahead-count and old reward-contract commit.
- Optionally runs the readiness preflight through `-RunReadiness`.
- Does not start Unity, rebuild, regenerate screenshots, alter BattleCore, change HUD/MechLab behavior, or stage generated artifacts.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add controlled demo handoff consistency check`

### PC10: Add Android Device Smoke Preflight

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不继续触控 UI 或性能工作；先把 Android 真机 smoke 的前置条件做成可机器检查状态，证明当前只缺授权设备。

**Files:**

- Create: `scripts/unity/check_android_device_preflight.ps1`
- Modify: `BUILD-MOBILE.md`
- Modify: `README.md`
- Modify: current plan docs

**Acceptance:**

- Checks Android APK exists under ignored build output.
- Checks adb and aapt exist in the Unity AndroidPlayer toolchain.
- Uses aapt to extract package name and launchable activity from the APK.
- Checks adb device state and supports `-DeviceId` when multiple devices are connected.
- Strict mode fails when no authorized device exists.
- `-AllowNoDevice` passes the local waiting state while reporting `Android device smoke preflight waiting on device`.
- Does not install, launch, rebuild, capture logs, alter BattleCore, change HUD/MechLab behavior, or stage generated artifacts.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

The strict preflight is expected to fail on this machine until a USB-debugging-enabled and authorized Android phone is connected.

**Commit:** `Add Android device smoke preflight`

### PC11: Add PC Core Playable Contract Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不继续扩大 PC 玩法；先把 PC 受控演示最核心的 BattleCore 状态做成单独可运行的合约检查，证明演示不是只靠截图和旧日志维持。

**Files:**

- Create: `scripts/unity/check_pc_core_playable_contract.ps1`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs, evidence page and handoff plan

**Acceptance:**

- Runs the Unity editor validator from a single PowerShell entrypoint.
- Requires the validator success marker `MC2 demo contract validation OK`.
- Requires the explicit PC core marker `MC2 PC core playable contract OK`.
- The PC core marker covers command state, solo return, Jet legality, occupancy, damage/ejection and debrief/relaunch.
- Writes only an ignored log under `analysis-output/`.
- Does not launch the player, rebuild, regenerate screenshots, alter HUD/MechLab behavior, install Android packages, or stage generated artifacts.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_core_playable_contract.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add PC core playable contract check`

### PC12: Add Mobile Command Model Preflight

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不提前做 G4 触控 UI；先把当前 PC 演示是否仍符合移动端低复杂度指挥模型做成可运行预检，避免 PC 端继续打磨时悄悄长出手机上不好用的操作。

**Files:**

- Create: `scripts/unity/check_mobile_command_model_preflight.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs, evidence page and handoff plan

**Acceptance:**

- Reads only existing ignored capture sidecars and tracked source/docs.
- Requires active battle sidecars to report status rows, Jet, map, bay/system, compact objective, closed-but-available mission map, hidden combat log, hidden account UI, disabled save UI and hidden overlays.
- Requires MechLab sidecar to report whole-block/pressure-card fitting, all mounted weapons active and
oToggle=yes`.
- Requires Unity presentation code to still emit the matching sparse battle HUD and MechLab no-toggle markers.
- Requires plan/docs to name the current PC wait-state package and the mobile command preflight entrypoint.
- No drag-box selection.
- Does not launch Unity, rebuild, regenerate screenshots, alter HUD/MechLab behavior, install Android packages, or stage generated artifacts.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add mobile command model preflight`

### PC13: Add Current Plan Gate Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不继续扩大 PC 玩法；把当前“可交接、可演示、移动指挥模型未回退、Android 只差授权设备”的状态收成一条命令，减少每次继续前的人工拼检查。

**Files:**

- Create: `scripts/unity/check_current_plan_gate.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs, evidence page and handoff plan

**Acceptance:**

- Runs `check_controlled_demo_handoff.ps1`.
- Runs `check_controlled_demo_readiness.ps1` unless `-SkipReadiness` is used.
- Runs `check_mobile_command_model_preflight.ps1`.
- Runs `check_android_device_preflight.ps1 -AllowNoDevice`.
- Accepts either `Android device smoke preflight OK.` when a single authorized phone exists, or `Android device smoke preflight waiting on device.` when no phone is connected.
- Does not launch Unity, rebuild, regenerate screenshots, alter HUD/MechLab behavior, install Android packages, or stage generated artifacts.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add current plan gate check`

### PC14: Add Android Smoke Log Crash Scan

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，先加强真机 smoke 的失败判定：设备可用后不能只看 APK 是否启动和进程是否仍在，还要扫描 logcat 里的强崩溃标记。

**Files:**

- Create: `scripts/unity/check_android_smoke_log.ps1`
- Modify: `scripts/unity/android_device_smoke.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: `BUILD-MOBILE.md`
- Modify: current plan docs, evidence page and handoff plan

**Acceptance:**

- `android_device_smoke.ps1` captures logcat then calls `check_android_smoke_log.ps1` unless `-SkipLogCheck` is used.
- The scanner fails on strong crash markers: fatal exception, fatal signal, `SIGSEGV`, `SIGABRT`, ANR for the package, package process death, forced activity finish and Unity crash marker.
- The scanner has `-SelfTest`, so the parser can be verified without an Android device.
- `check_current_plan_gate.ps1` includes the log scanner self-test.
- Does not launch Unity, rebuild, regenerate screenshots, install Android packages without a device, alter gameplay or stage generated artifacts.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_log.ps1 -SelfTest
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android smoke log crash scan`

### PC15: Add Android Smoke Plan Mode

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，让真正的 Android device smoke helper 本身也能无设备预演，确认 APK、adb、aapt、package、activity、log path 和安装/启动/log-check 开关都能解析，设备到位后少踩路径和参数问题。

**Files:**

- Modify: `scripts/unity/android_device_smoke.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: `BUILD-MOBILE.md`
- Modify: README/current plan docs/evidence/handoff docs

**Acceptance:**

- `android_device_smoke.ps1 -PlanOnly` validates APK and adb paths, resolves package name through `aapt`, reports activity or monkey fallback, log path, install/launch/log-check switch state, and exits before adb device selection.
- `check_current_plan_gate.ps1` includes `android_device_smoke.ps1 -PlanOnly`.
- No install, launch, logcat capture, Unity run, rebuild, gameplay change or generated artifact is required.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android smoke plan mode`

### PC16: Add Battle HUD Sparse Contract Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不扩大玩法；把“战斗中不用显示太多信息”做成源码级和 capture gate 级合约，避免普通战斗 HUD 重新长出大日志、存档、账号或调试覆盖层。

**Files:**

- Create: `scripts/unity/check_battle_hud_sparse_contract.ps1`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `scripts/unity/capture_reference_visuals.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: README/BUILD-WIN/plans/evidence/handoff docs

**Acceptance:**

- The script checks Unity presentation source, capture gate and mobile command model preflight without launching Unity.
- `SparseBattleUiRegressionSummaryOk` requires `missionMap=available-closed`.
- `capture_reference_visuals.ps1` fails battle HUD sidecars that do not report `missionMap=available-closed`.
- Current plan gate includes the sparse HUD contract check.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_battle_hud_sparse_contract.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add battle HUD sparse contract check`

### PC17: Add Demo Source Hygiene Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不扩大玩法；把“生成截图、日志、Unity build、APK/AAB、私有参考导出不能进入源码提交”做成轻量检查，避免后续换机、演示或提交时污染仓库。

**Files:**

- Create: `scripts/unity/check_demo_source_hygiene.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: README/BUILD-WIN/current plans/evidence/handoff docs

**Acceptance:**

- The script checks tracked and staged paths without requiring a clean working tree.
- It fails tracked/staged generated evidence, Unity builds, APK/AAB outputs, logs, private reference art, non-example content packs and reference export paths.
- It checks `.gitignore` still contains the expected generated/private-output markers.
- Current plan gate includes the demo source hygiene check.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add demo source hygiene check`

### PC18: Add AI Deputy Contract Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不扩大玩法；把 AI 副官“慢频高层、可离线、无 key fallback、普通 smoke 不花 token、不进入帧循环”的产品边界做成轻量检查。

**Files:**

- Create: `scripts/unity/check_ai_deputy_contract.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: README/BUILD-WIN/current plans/evidence/handoff docs

**Acceptance:**

- The script checks source and command-script markers without launching Unity or calling MiniMax.
- MiniMax request remains directive-only with limited token output and no coordinate/unit-id control.
- Startup MiniMax command remains explicit, clamped and rule-fallback guarded.
- Normal visible-flow smoke asserts the AI deputy window without requesting MiniMax commander steps.
- Main frame loop does not construct MiniMax or call `ChooseDirective`.
- Current plan gate includes the AI deputy contract check.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_ai_deputy_contract.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add AI deputy contract check`

### PC19: Add Windows Demo Build Freshness Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不扩大玩法；把 Windows 受控演示 player 是否落后于当前 Unity build 输入做成轻量检查，避免拿旧构建演示。

**Files:**

- Create: `scripts/unity/check_windows_demo_build_freshness.ps1`
- Modify: `scripts/unity/check_controlled_demo_readiness.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: README/BUILD-WIN/current plans/evidence/handoff docs

**Acceptance:**

- The script checks tracked Unity `Assets`, `ProjectSettings` and `Packages` inputs against ignored Windows player outputs.
- It fails when the Windows player is missing or stale.
- It excludes generated `.unity` scene timestamp churn from freshness because the builder can rewrite scene fileIDs after build.
- It does not launch Unity or create artifacts.
- Controlled demo readiness includes the build freshness check.
- Windows player output remains ignored and unstaged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_windows_demo_build_freshness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_readiness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Windows demo build freshness check`

### PC20: Add Controlled Demo Evidence Freshness Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不扩大玩法；让受控演示证据包拒绝旧 visible-flow 日志和旧截图 sidecar，确保当前 evidence 对应当前 Windows build。

**Files:**

- Modify: `scripts/unity/check_controlled_demo_evidence.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: README/BUILD-WIN/current plans/evidence/handoff docs
- Refresh ignored local evidence: `analysis-output/unity-player-pc-evidence-visible-flow.log` and `analysis-output/reference-visual-captures/*`

**Acceptance:**

- The evidence checker compares visible-flow log freshness against the current Windows build and visible-flow command file.
- It compares six standard capture PNG/JSON sidecars against the current Windows build and capture helper.
- It still checks smoke exit, debrief, compact loadout, MechLab no-toggle, terrain readability, sparse HUD, contact separation and damage story.
- It does not launch Unity or create artifacts itself.
- Fresh ignored visible-flow and six capture outputs exist locally and remain unstaged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_evidence.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_readiness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add controlled demo evidence freshness check`

### PC21: Add Controlled Demo Capture Log Freshness Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不扩大玩法；让受控演示证据包同时检查每个标准 capture 的运行日志，避免截图/sidecar 是新的但对应日志缺失、过旧或无法诊断截图生成链路。

**Files:**

- Modify: `scripts/unity/check_controlled_demo_evidence.ps1`
- Modify: README/BUILD-WIN/current plans/evidence/handoff/mobile docs
- Keep ignored local evidence unstaged: `analysis-output/reference-visual-captures/*.log`

**Acceptance:**

- The evidence checker requires the six standard capture logs: `mechlab`, `spawn`, `airfield`, `hangar-contact`, `damage-demo`, and
orth-patrol`.
- Each capture log must be newer than the current Windows build/capture helper freshness anchor.
- Each capture log must include the preset marker, screenshot request marker, and sidecar write marker.
- PNG/JSON freshness, visible-flow freshness, MechLab no-toggle, terrain readability, sparse HUD, contact separation and damage story checks still run.
- The checker does not launch Unity or create artifacts itself.
- Capture logs remain ignored local evidence and are not staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_evidence.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_readiness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add controlled demo capture log freshness check`

### PC22: Add Android APK Freshness Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不提前做 G4/G5；把 G3 依赖的 ignored Android APK 是否落后于当前 Unity 输入做成机器检查，确保设备到位后不会安装旧包。

**Files:**

- Create: `scripts/unity/check_android_apk_freshness.ps1`
- Modify: `scripts/unity/check_android_device_preflight.ps1`
- Modify: `scripts/unity/android_device_smoke.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: README/BUILD-MOBILE/BUILD-WIN/current plans/evidence/handoff/mobile docs
- Refresh ignored local output: `unity-mc2-demo/Builds/Android/MC2UnityDemo.apk`

**Acceptance:**

- The Android APK freshness checker fails if `MC2UnityDemo.apk` is missing, empty, or older than tracked Unity `Assets`, `ProjectSettings`, or `Packages` inputs.
- Generated `.unity` scene timestamp churn is excluded from freshness, matching the Windows build freshness policy.
- `check_android_device_preflight.ps1 -AllowNoDevice` checks APK freshness before reporting the expected waiting-on-device state.
- `android_device_smoke.ps1 -PlanOnly` and real device smoke reject stale APK before install/launch.
- `check_current_plan_gate.ps1` includes a visible Android APK freshness gate.
- Refreshed APK remains ignored and unstaged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_freshness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android APK freshness check`

### PC23: Add Android APK Identity Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不提前做 G4/G5；把 G3 安装/启动依赖的 APK 包名和 launch activity 做成机器检查，确保设备到位后不会安装错包或启动错入口。

**Files:**

- Create: `scripts/unity/check_android_apk_identity.ps1`
- Modify: `scripts/unity/check_android_device_preflight.ps1`
- Modify: `scripts/unity/android_device_smoke.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: README/BUILD-MOBILE/BUILD-WIN/current plans/evidence/handoff/mobile docs

**Acceptance:**

- The Android APK identity checker fails if `aapt` cannot read APK badging.
- It requires package name `com.DefaultCompany.unitymc2demo`.
- It requires launch activity `com.unity3d.player.UnityPlayerGameActivity`.
- `check_android_device_preflight.ps1 -AllowNoDevice` checks APK identity before reporting the expected waiting-on-device state.
- `android_device_smoke.ps1 -PlanOnly` and real device smoke reject wrong APK identity before install/launch.
- `check_current_plan_gate.ps1` includes a visible Android APK identity gate.
- No APK, log or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_identity.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android APK identity check`

### PC24: Add Android APK Compatibility Check

**Status:** Completed 2026-06-12.

**Goal:** Keep G3 waiting on an authorized phone without leaving APK device compatibility as a late surprise. Before install/launch, prove the ignored APK advertises the expected Android SDK and native ABI metadata.

**Implementation:**

- Added `scripts/unity/check_android_apk_compatibility.ps1`.
- Parses `aapt dump badging` for `sdkVersion`, `targetSdkVersion`, and
ative-code`.
- Requires `minSdkVersion` 25, `targetSdkVersion` 36, and native-code `arm64-v8a`.
- Wires the compatibility check into `check_android_device_preflight.ps1`, `android_device_smoke.ps1`, and `check_current_plan_gate.ps1`.
- Updates handoff, mobile and evidence docs to keep the sealed PC/mobile waiting-state package aligned.

**Acceptance:**

- The Android APK compatibility checker fails if `aapt` cannot read APK badging.
- It fails unless `minSdkVersion` is 25.
- It fails unless `targetSdkVersion` is 36.
- It fails unless native-code ABI is exactly `arm64-v8a`.
- `check_android_device_preflight.ps1 -AllowNoDevice` checks APK compatibility before reporting the expected waiting-on-device state.
- `android_device_smoke.ps1 -PlanOnly` and real device smoke reject incompatible APK metadata before install/launch.
- `check_current_plan_gate.ps1` includes a visible Android APK compatibility gate.
- No APK, log or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_compatibility.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android APK compatibility check`

### PC25: Add Android APK Signing Check

**Status:** Completed 2026-06-12.

**Goal:** Keep G3 waiting on an authorized phone without leaving APK signing as a late install failure. Before install/launch, prove the ignored APK verifies through the Unity Android SDK signing tool.

**Implementation:**

- Added `scripts/unity/check_android_apk_signing.ps1`.
- Runs `apksigner verify --verbose --print-certs`.
- Requires the APK to verify.
- Requires APK Signature Scheme v2 verification.
- Requires signer DN `C=US, O=Android, CN=Android Debug`.
- Reports signer SHA-256 digest for diagnosis.
- Wires the signing check into `check_android_device_preflight.ps1`, `android_device_smoke.ps1`, and `check_current_plan_gate.ps1`.
- Updates handoff, mobile and evidence docs to keep the sealed PC/mobile waiting-state package aligned.

**Acceptance:**

- The Android APK signing checker fails if `apksigner` cannot verify the APK.
- It fails unless the APK verifies.
- It fails unless APK Signature Scheme v2 is verified.
- It fails unless signer DN is `C=US, O=Android, CN=Android Debug`.
- It prints the signer SHA-256 digest for diagnosis.
- `check_android_device_preflight.ps1 -AllowNoDevice` checks APK signing before reporting the expected waiting-on-device state.
- `android_device_smoke.ps1 -PlanOnly` and real device smoke reject unsigned or wrongly signed APK metadata before install/launch.
- `check_current_plan_gate.ps1` includes a visible Android APK signing gate.
- No APK, log or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_signing.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android APK signing check`

### PC26: Add Android APK Manifest Check

**Status:** Completed 2026-06-12.

**Goal:** Keep G3 waiting on an authorized phone without leaving APK manifest install-target drift as a late device install surprise. Before install/launch, prove permissions, required hardware features and screen support remain within the intended demo envelope.

**Implementation:**

- Added `scripts/unity/check_android_apk_manifest.ps1`.
- Parses `aapt dump badging` for permissions, required features, not-required features and supported screens.
- Requires the current permission allowlist:
  - `android.permission.INTERNET`
  - `com.DefaultCompany.unitymc2demo.DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION`
- Fails if any required hardware feature appears.
- Requires `android.hardware.touchscreen` and `android.hardware.vulkan.version` to remain not-required features.
- Requires screen support for `small`,
ormal`, `large`, and `xlarge`.
- Wires the manifest check into `check_android_device_preflight.ps1`, `android_device_smoke.ps1`, and `check_current_plan_gate.ps1`.
- Updates handoff, mobile and evidence docs to keep the sealed PC/mobile waiting-state package aligned.

**Acceptance:**

- The Android APK manifest checker fails if `aapt` cannot read APK badging.
- It fails if unexpected permissions appear or expected permissions disappear.
- It fails if any required hardware feature appears.
- It fails unless touchscreen and Vulkan are not-required features.
- It fails unless all four screen classes are supported.
- `check_android_device_preflight.ps1 -AllowNoDevice` checks APK manifest before reporting the expected waiting-on-device state.
- `android_device_smoke.ps1 -PlanOnly` and real device smoke reject manifest drift before install/launch.
- `check_current_plan_gate.ps1` includes a visible Android APK manifest gate.
- No APK, log or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_manifest.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android APK manifest check`

### PC27: Add Android APK Payload Check

**Status:** Completed 2026-06-12.

**Goal:** Keep G3 waiting on an authorized phone without leaving APK runtime payload drift as a late device install/launch surprise. Before install/launch, prove Unity/IL2CPP native libraries, runtime data files and ABI folders are present and coherent.

**Implementation:**

- Added `scripts/unity/check_android_apk_payload.ps1`.
- Opens the APK as a zip and checks required entries without launching Unity.
- Requires a single ABI folder: `arm64-v8a`.
- Requires core APK entries, IL2CPP native libraries, Unity metadata and scene/runtime data entries.
- Fails if `assets/bin/Data` or `lib` entry counts look truncated.
- Wires the payload check into `check_android_device_preflight.ps1`, `android_device_smoke.ps1`, and `check_current_plan_gate.ps1`.
- Updates handoff, mobile and evidence docs to keep the sealed PC/mobile waiting-state package aligned.

**Acceptance:**

- The Android APK payload checker fails if the APK is missing.
- It fails if ABI folders drift from `arm64-v8a`.
- It fails if required native libraries or Unity data files are missing.
- It fails if data/native entry counts look too small for the Unity player output.
- `check_android_device_preflight.ps1 -AllowNoDevice` checks APK payload before reporting the expected waiting-on-device state.
- `android_device_smoke.ps1 -PlanOnly` and real device smoke reject payload drift before install/launch.
- `check_current_plan_gate.ps1` includes a visible Android APK payload gate.
- No APK, log or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_payload.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android APK payload check`

### PC28: Add Android APK Size Budget Check

**Status:** Completed 2026-06-12.

**Goal:** Keep G3 waiting on an authorized phone without leaving APK package size drift as a late install-readiness surprise. Before install/launch, prove the ignored APK is neither implausibly small nor above the current early mobile demo size budget.

**Implementation:**

- Added `scripts/unity/check_android_apk_size_budget.ps1`.
- Requires the APK to be at least 1 MiB.
- Requires the APK to stay at or below 100 MiB for the current early mobile demo.
- Reports exact bytes and MiB for the current APK and budget.
- Wires the size-budget check into `check_android_device_preflight.ps1`, `android_device_smoke.ps1`, and `check_current_plan_gate.ps1`.
- Updates handoff, mobile and evidence docs to keep the sealed PC/mobile waiting-state package aligned.

**Acceptance:**

- The Android APK size budget checker fails if the APK is missing.
- It fails if the APK is implausibly small.
- It fails if the APK exceeds the current 100 MiB early mobile demo budget.
- `check_android_device_preflight.ps1 -AllowNoDevice` checks APK size before reporting the expected waiting-on-device state.
- `android_device_smoke.ps1 -PlanOnly` and real device smoke reject size-budget drift before install/launch.
- `check_current_plan_gate.ps1` includes a visible Android APK size budget gate.
- No APK, log or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_size_budget.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android APK size budget check`

### PC29: Add Android SDK Tooling Check

**Status:** Completed 2026-06-12.

**Goal:** Keep G3 waiting on an authorized phone without leaving Android SDK tooling drift as a late device-smoke surprise. Before install/launch, prove Unity's AndroidPlayer SDK, NDK, OpenJDK, build tools, platform and command-line tools are present and usable.

**Implementation:**

- Added `scripts/unity/check_android_sdk_tooling.ps1`.
- Requires Unity AndroidPlayer SDK, NDK and OpenJDK paths.
- Requires `build-tools;36.0.0`, `platforms;android-36`, `android.jar`, adb, aapt and apksigner.
- Checks adb, aapt and apksigner version output.
- Wires the SDK tooling check into `check_android_device_preflight.ps1`, `android_device_smoke.ps1`, and `check_current_plan_gate.ps1`.
- Updates handoff, mobile and evidence docs to keep the sealed PC/mobile waiting-state package aligned.

**Acceptance:**

- The Android SDK tooling checker fails if AndroidPlayer, SDK, NDK or OpenJDK is missing.
- It fails if build-tools 36.0.0 or android-36 platform is missing.
- It fails if adb, aapt or apksigner is missing or cannot report a version.
- `check_android_device_preflight.ps1 -AllowNoDevice` checks SDK tooling before reporting the expected waiting-on-device state.
- `android_device_smoke.ps1 -PlanOnly` and real device smoke reject SDK tooling drift before install/launch.
- `check_current_plan_gate.ps1` includes a visible Android SDK tooling gate.
- No APK, log or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_sdk_tooling.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android SDK tooling check`

### PC30: Add Android Smoke Artifact Hygiene Check

**Status:** Completed 2026-06-12.

**Goal:** Keep G3 waiting on an authorized phone without allowing Android smoke outputs to drift into source control. Before real device evidence is collected, prove APK/AAB outputs, Android smoke logs/screenshots and `Builds/Android` paths are ignored and absent from tracked/staged paths.

**Implementation:**

- Added `scripts/unity/check_android_smoke_artifact_hygiene.ps1`.
- Requires `.gitignore` markers for Android smoke logs/screenshots, APK/AAB outputs and Unity `Builds/`.
- Fails if tracked or staged paths contain Android smoke logs/screenshots, APK/AAB files or `unity-mc2-demo/Builds/Android/` outputs.
- Wires the hygiene check into `check_android_device_preflight.ps1` and `check_current_plan_gate.ps1`.
- Updates handoff, mobile, build and evidence docs to keep the sealed PC/mobile waiting-state package aligned.

**Acceptance:**

- The Android smoke artifact hygiene checker fails if `.gitignore` no longer ignores logs, PNG captures, APK/AAB outputs or Unity builds.
- It fails if Android smoke logs/screenshots, APK/AAB files or Android build outputs are tracked or staged.
- `check_android_device_preflight.ps1 -AllowNoDevice` checks artifact hygiene before reporting the expected waiting-on-device state.
- `check_current_plan_gate.ps1` includes a visible Android smoke artifact hygiene gate.
- No APK, log, screenshot, sidecar or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_artifact_hygiene.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android smoke artifact hygiene check`

### PC31: Add Android Smoke Screenshot Evidence Capture

**Status:** Completed 2026-06-12.

**Goal:** Keep G3 waiting on an authorized phone without leaving real-device visual evidence as a manual afterthought. When a phone is available, the Android device smoke helper should collect both logcat and an ignored screenshot; without a phone, `-PlanOnly` should prove the screenshot path and capture switch are wired.

**Implementation:**

- Added `-ScreenshotPath` and `-SkipScreenshot` to `scripts/unity/android_device_smoke.ps1`.
- Defaults screenshot evidence to `analysis-output\android-device-smoke.png`.
- Captures a PNG through `adb exec-out screencap -p` using binary stream copying instead of text redirection.
- Fails if the captured screenshot is implausibly small.
- Updates `-PlanOnly` to print `Screenshot: ...android-device-smoke.png` and `ScreenshotCapture: True`.
- Tightens `check_current_plan_gate.ps1` so Android smoke plan mode must include the screenshot markers.

**Acceptance:**

- `android_device_smoke.ps1 -PlanOnly` prints `Android device smoke plan OK.`, `Screenshot:` and `ScreenshotCapture: True`.
- Real device smoke writes an ignored `analysis-output\android-device-smoke.png` unless `-SkipScreenshot` is passed.
- The current plan gate fails if screenshot capture is dropped from Android smoke plan mode.
- Android smoke artifact hygiene continues to keep the screenshot ignored and out of tracked/staged paths.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_artifact_hygiene.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android smoke screenshot evidence capture`

### PC32: Add Android Smoke Summary Evidence Output

**Status:** Completed 2026-06-12.

**Goal:** Keep G3 waiting on an authorized phone without leaving real-device result reporting as a manual console scrape. When a phone is available, the Android device smoke helper should write an ignored JSON summary with enough data to document the run; without a phone, `-PlanOnly` should prove the summary path and write switch are wired.

**Implementation:**

- Added `-SummaryPath` and `-SkipSummary` to `scripts/unity/android_device_smoke.ps1`.
- Defaults summary evidence to `analysis-output\android-device-smoke-summary.json`.
- Writes JSON with result, UTC timestamp, device id, model, Android version, package, activity, process, APK, log, screenshot, launch wait, install/launch/log-check/screenshot flags.
- Updates `-PlanOnly` to print `Summary: ...android-device-smoke-summary.json` and `SummaryWrite: True`.
- Tightens `check_current_plan_gate.ps1` so Android smoke plan mode must include the summary markers.

**Acceptance:**

- `android_device_smoke.ps1 -PlanOnly` prints `Android device smoke plan OK.`, `Summary:` and `SummaryWrite: True`.
- Real device smoke writes ignored `analysis-output\android-device-smoke-summary.json` unless `-SkipSummary` is passed.
- `check_current_plan_gate.ps1` fails if Android smoke plan mode no longer reports summary output.
- Android smoke artifact hygiene keeps the summary under ignored `analysis-output/`.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_artifact_hygiene.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android smoke summary evidence output`

### PC33: Add Android Smoke Summary Schema Check

**Status:** Completed 2026-06-12.

**Goal:** Keep G3 waiting on an authorized phone while making the real-device smoke summary self-verifying. The helper should not only write ignored JSON; it should fail if the summary misses required fields, has the wrong package name, bad timestamp, blank device/process evidence, invalid evidence paths or non-Boolean execution flags.

**Implementation:**

- Added `scripts/unity/check_android_smoke_summary.ps1`.
- Added `-SelfTest` coverage that validates an in-memory summary object without requiring a device or creating local artifacts.
- Wired `android_device_smoke.ps1` to run the summary checker immediately after writing `analysis-output\android-device-smoke-summary.json`.
- Wired `check_current_plan_gate.ps1` to run `check_android_smoke_summary.ps1 -SelfTest`.
- Tightened Android smoke artifact hygiene so ignored Android smoke summary JSON cannot be tracked or staged accidentally.
- Updated README, Windows/mobile build notes, master/detailed/mobile/evidence/handoff docs and handoff consistency markers for the then-current PC33 state.

**Acceptance:**

- `check_android_smoke_summary.ps1 -SelfTest` prints `Android smoke summary check self-test OK`.
- Real device smoke validates the summary JSON after writing it unless `-SkipSummary` is passed.
- `check_current_plan_gate.ps1` fails if the summary schema self-test stops passing.
- Android smoke artifact hygiene treats Android smoke summary JSON as an ignored smoke artifact.
- No APK, log, screenshot, summary or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_summary.ps1 -SelfTest
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_artifact_hygiene.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android smoke summary schema check`

### PC34: Add Android Smoke Summary Preflight Check

**Status:** Completed 2026-06-12.

**Goal:** Keep G3 waiting on an authorized phone while making the direct G3 preflight entry complete. `check_android_device_preflight.ps1 -AllowNoDevice` should not only verify APK/tooling/package/device state; it should also prove the smoke summary schema checker is present and passing before reporting waiting-on-device or OK.

**Implementation:**

- Wired `scripts/unity/check_android_device_preflight.ps1` to run `check_android_smoke_summary.ps1 -SelfTest`.
- Added a `smoke summary schema` preflight row with `Android smoke summary check self-test OK`.
- Updated README, Windows/mobile build notes, master/detailed/mobile/evidence/handoff docs and handoff consistency markers for the then-current PC34 checkpoint.

**Acceptance:**

- `check_android_device_preflight.ps1 -AllowNoDevice` prints `Android device smoke preflight waiting on device` when no phone is connected.
- The same output includes `smoke summary schema` and `Android smoke summary check self-test OK`.
- `check_current_plan_gate.ps1` and `check_controlled_demo_handoff.ps1 -RunReadiness` keep passing.
- No APK, log, screenshot, summary or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_summary.ps1 -SelfTest
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android smoke summary preflight check`

### PC35: Add Android Smoke Plan/Preflight Consistency Check

**Status:** Completed 2026-06-12.

**Goal:** Keep G3 waiting on an authorized phone while proving the two no-device Android entries describe the same smoke path. `android_device_smoke.ps1 -PlanOnly` and `check_android_device_preflight.ps1 -AllowNoDevice` should agree on package, activity, ignored evidence outputs, enabled install/launch/log/screenshot/summary switches and summary schema readiness.

**Implementation:**

- Added `scripts/unity/check_android_smoke_plan_consistency.ps1`.
- The checker runs `android_device_smoke.ps1 -PlanOnly` and `check_android_device_preflight.ps1 -AllowNoDevice`.
- It validates package `com.DefaultCompany.unitymc2demo`, activity `com.unity3d.player.UnityPlayerGameActivity`, ignored log/screenshot/summary output paths, execution flags and summary schema self-test markers.
- Wired the checker into `scripts/unity/check_current_plan_gate.ps1`.
- Updated README, Windows/mobile build notes, master/detailed/mobile/evidence/handoff docs and handoff consistency markers for the then-current PC35 checkpoint.

**Acceptance:**

- `check_android_smoke_plan_consistency.ps1` prints `Android smoke plan/preflight consistency check OK`.
- The check passes with no phone connected when preflight reports waiting-on-device.
- `check_current_plan_gate.ps1` and `check_controlled_demo_handoff.ps1 -RunReadiness` keep passing.
- No APK, log, screenshot, summary or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_plan_consistency.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android smoke plan/preflight consistency check`

### PC36: Add Android G3 Readiness Check

**Status:** Completed 2026-06-12.

**Goal:** Keep G3 waiting on an authorized phone while giving the mobile-first gate one direct no-install readiness entry. Before a phone is connected, the project should prove the Android APK/tooling/device preflight, plan/preflight consistency, smoke plan, log crash scanner and summary schema all remain aligned.

**Implementation:**

- Added `scripts/unity/check_android_g3_readiness.ps1`.
- The readiness checker runs `check_android_device_preflight.ps1 -AllowNoDevice`, `check_android_smoke_plan_consistency.ps1`, `android_device_smoke.ps1 -PlanOnly`, `check_android_smoke_log.ps1 -SelfTest` and `check_android_smoke_summary.ps1 -SelfTest`.
- It reports `Android G3 readiness check waiting on device` when no authorized phone is connected and `Android G3 readiness check OK` when preflight sees a valid device.
- Wired the checker into `scripts/unity/check_current_plan_gate.ps1`.
- Updated README, Windows/mobile build notes, master/detailed/mobile/evidence/handoff docs and handoff consistency markers to seal the then-current PC36 checkpoint.

**Acceptance:**

- `check_android_g3_readiness.ps1` prints `Android G3 readiness check waiting on device` on the current no-phone machine.
- The check includes device preflight, plan/preflight consistency, smoke plan, log scanner self-test and summary schema self-test.
- `check_current_plan_gate.ps1` includes an explicit Android G3 readiness gate.
- No APK, log, screenshot, summary or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_readiness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_plan_consistency.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android G3 readiness check`

### PC37: Add Android G3 Device Requirement Check

**Status:** Completed 2026-06-12.

**Goal:** Keep G3 honest while waiting on hardware. The no-device readiness bundle is useful, but strict G3 readiness must not pass without a physical authorized Android phone.

**Implementation:**

- Added `scripts/unity/check_android_g3_device_requirement.ps1`.
- The checker runs `check_android_g3_readiness.ps1 -RequireDevice`.
- It reports `Android G3 device requirement check waiting on device` when strict readiness fails only because no authorized phone is connected.
- It reports `Android G3 device requirement check OK` when strict readiness passes with an authorized phone.
- Wired the checker into `scripts/unity/check_current_plan_gate.ps1`.
- Updated README, Windows/mobile build notes, master/detailed/mobile/evidence/handoff docs and handoff consistency markers to seal the then-current PC37 checkpoint.

**Acceptance:**

- `check_android_g3_device_requirement.ps1` prints `Android G3 device requirement check waiting on device` on the current no-phone machine.
- It does not hide non-device failures behind waiting-on-device.
- `check_current_plan_gate.ps1` includes an explicit Android G3 device requirement gate.
- No APK, log, screenshot, summary or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_device_requirement.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_readiness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android G3 device requirement check`

### PC38: Add PC Visual Capture Sanity Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不扩大玩法；把受控演示六张 PNG 截图的最基本图像质量做成轻量机器检查，避免截图退化为空白、纯色、粉框或低信息量色块后仍被当作可展示证据。

**Implementation:**

- Added `scripts/unity/check_pc_visual_capture_sanity.ps1`.
- The checker reads `mechlab`, `spawn`, `airfield`, `hangar-contact`, `damage-demo` and
orth-patrol` PNG captures without launching Unity.
- It checks expected resolution, minimum PNG bytes, sampled unique colors, center unique colors, center lit ratio, luminance standard deviation, maximum magenta fallback ratio and maximum near-monochrome ratio.
- Wired the checker into `scripts/unity/check_current_plan_gate.ps1`.
- Updated README, Windows build notes, master/detailed/mobile/evidence/handoff docs and handoff consistency markers to seal the then-current PC38 checkpoint.

**Acceptance:**

- `check_pc_visual_capture_sanity.ps1` prints `PC visual capture sanity check OK`.
- `check_current_plan_gate.ps1` includes an explicit PC visual capture sanity gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` keeps passing after the new gate is documented.
- No screenshot, sidecar, log, APK or build output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_visual_capture_sanity.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add PC visual capture sanity check`

### PC39: Add PC Visual Capture Sanity Self-Test

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不扩大玩法；让 PC38 的截图质量门禁能证明自己会识别正常截图、纯色坏图和粉色 fallback 坏图，避免门禁逻辑失效后仍给当前演示证据放行。

**Implementation:**

- Added `-SelfTest` to `scripts/unity/check_pc_visual_capture_sanity.ps1`.
- The self-test writes ignored synthetic images under `analysis-output\pc-visual-sanity-selftest`.
- It generates one valid multi-color grid, one flat gray image and one magenta fallback image.
- It verifies the valid image passes the sanity thresholds and the flat/magenta samples trip the relevant thresholds.
- Wired the self-test into `scripts/unity/check_current_plan_gate.ps1`.
- Updated README, Windows build notes, master/detailed/mobile/evidence/handoff docs and handoff consistency markers to seal the then-current PC39 checkpoint.

**Acceptance:**

- `check_pc_visual_capture_sanity.ps1 -SelfTest` prints `PC visual capture sanity self-test OK`.
- `check_current_plan_gate.ps1` includes an explicit PC visual capture sanity self-test gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` keeps passing after the new self-test is documented.
- No screenshot, sidecar, log, APK or build output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_visual_capture_sanity.ps1 -SelfTest
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_visual_capture_sanity.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add PC visual capture sanity self-test`

### PC40: Add PC Capture Sidecar Schema Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不扩大玩法；让六张受控演示 JSON sidecar 也具备独立结构检查，避免截图 PNG 看起来正常但 sidecar 里的截图路径、flow、camera、摘要字段或 referenceAssets 证据漂移后仍被当作当前演示证据。

**Implementation:**

- Added `scripts/unity/check_pc_capture_sidecar_schema.ps1`.
- The checker reads `mechlab`, `spawn`, `airfield`, `hangar-contact`, `damage-demo` and
orth-patrol` JSON sidecars without launching Unity.
- It requires each sidecar to match its PNG screenshot path, expected `1280x720` dimensions, nonblank mission/status fields, nonnegative unit/contact counters, objective presence, flow-specific `Battle` or `Mech Lab` state, camera vectors and reference-asset metadata.
- Wired the checker into `scripts/unity/check_current_plan_gate.ps1`.
- Updated README, Windows build notes, master/detailed/mobile/evidence/handoff docs and handoff consistency markers for the then-current PC checkpoint.

**Acceptance:**

- `check_pc_capture_sidecar_schema.ps1` prints `PC capture sidecar schema check OK`.
- `check_current_plan_gate.ps1` includes an explicit PC capture sidecar schema gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` keeps passing after the new schema gate is documented.
- No screenshot, sidecar, log, APK or build output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_capture_sidecar_schema.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add PC capture sidecar schema check`

### PC41: Add PC Capture Preset Contract Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不扩大玩法；固定受控演示标准截图集合，避免 capture helper、evidence、visual sanity、sidecar schema 或文档入口对六张标准 preset 的理解漂移。

**Implementation:**

- Updated `scripts/unity/capture_reference_visuals.ps1` default presets to the full standard list: `mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol`.
- Added `scripts/unity/check_pc_capture_preset_contract.ps1`.
- The checker confirms the standard preset list in `capture_reference_visuals.ps1`, `check_controlled_demo_evidence.ps1`, `check_pc_visual_capture_sanity.ps1`, `check_pc_capture_sidecar_schema.ps1`, README, Windows build notes and PC optimization docs.
- Wired the checker into `scripts/unity/check_current_plan_gate.ps1`.
- Updated README, Windows build notes, master/detailed/mobile/evidence/handoff docs and handoff consistency markers for the then-current PC checkpoint.

**Acceptance:**

- `check_pc_capture_preset_contract.ps1` prints `PC capture preset contract check OK`.
- The standard preset list remains `mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol`.
- `check_current_plan_gate.ps1` includes an explicit PC capture preset contract gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` keeps passing after the new preset contract is documented.
- No screenshot, sidecar, log, APK or build output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_capture_preset_contract.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add PC capture preset contract check`

### PC42: Add PC Capture Artifact Hygiene Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不扩大玩法；把本地重采集截图产生的 PNG、JSON sidecar、capture log 和视觉 sanity 自测图片边界做成独立门禁，避免这些 ignored 证据误进源码提交。

**Implementation:**

- Added `scripts/unity/check_pc_capture_artifact_hygiene.ps1`.
- The checker confirms tracked and staged git paths do not contain PC capture artifact directories.
- It confirms `.gitignore` still covers `analysis-output/reference-visual-captures/`, `analysis-output/reference-visual-captures-no-placeholders/`, `analysis-output/pc-visual-sanity-selftest/`, `analysis-output/*.png` and `*.log`.
- It checks existing local PC capture artifacts are ignored by git.
- Wired the checker into `scripts/unity/check_current_plan_gate.ps1`.
- Updated README, Windows build notes, master/detailed/mobile/evidence/handoff docs and handoff consistency markers for the then-current PC checkpoint.

**Acceptance:**

- `check_pc_capture_artifact_hygiene.ps1` prints `PC capture artifact hygiene check OK`.
- `check_current_plan_gate.ps1` includes an explicit PC capture artifact hygiene gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` keeps passing after the new artifact hygiene gate is documented.
- No screenshot, sidecar, log, APK or build output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_capture_artifact_hygiene.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add PC capture artifact hygiene check`

### PC43: Add PC Window Contract Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不扩大玩法；把 PC 受控演示启动和截图的窗口尺寸固定为 `1280x720` windowed 契约，避免 Windows player 恢复到异常巨大窗口或截图尺寸与演示窗口漂移。

**Implementation:**

- Added `scripts/unity/check_pc_window_contract.ps1`.
- The checker confirms `run_windows_demo.ps1` and `capture_reference_visuals.ps1` default to `1280x720`.
- It confirms both scripts pass `-screen-width`, `-screen-height` and `-screen-fullscreen 0`.
- It runs `run_windows_demo.ps1 -CheckOnly` and requires the resolved launch args to include the controlled window settings.
- Wired the checker into `scripts/unity/check_current_plan_gate.ps1`.
- Updated README, Windows build notes, master/detailed/mobile/evidence/handoff docs and handoff consistency markers to seal the then-current pass through PC43.

**Acceptance:**

- `check_pc_window_contract.ps1` prints `PC window contract check OK`.
- `check_current_plan_gate.ps1` includes an explicit PC window contract gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` keeps passing after the new window contract is documented.
- No screenshot, sidecar, log, APK or build output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_window_contract.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add PC window contract check`

### PC44: Add PC Launch Log Hygiene Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不扩大玩法；把 PC 受控演示 runtime log 的默认路径和 Git 卫生固定成机器可检查契约，避免本地启动日志进入源码提交。

**Implementation:**

- Added `scripts/unity/check_pc_launch_log_hygiene.ps1`.
- The checker confirms `run_windows_demo.ps1` writes the default runtime log to `analysis-output\windows-demo-run.log`.
- It confirms README, BUILD-WIN and PC optimization plan document `analysis-output/windows-demo-run.log`.
- It confirms `.gitignore` covers launch logs through `*.log`.
- It uses `git check-ignore` to prove `analysis-output/windows-demo-run.log` is ignored.
- It checks tracked and staged paths contain no `analysis-output/*.log` launch logs.
- Wired the checker into `scripts/unity/check_current_plan_gate.ps1`.
- Updated README, Windows build notes, master/detailed/mobile/evidence/handoff docs and handoff consistency markers to seal the then-current pass through PC44.

**Acceptance:**

- `check_pc_launch_log_hygiene.ps1` prints `PC launch log hygiene check OK`.
- `check_current_plan_gate.ps1` includes an explicit PC launch log hygiene gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` keeps passing after the new log hygiene contract is documented.
- No screenshot, sidecar, log, APK or build output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_launch_log_hygiene.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add PC launch log hygiene check`

### PC45: Add PC Build Artifact Hygiene Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不扩大玩法；把 PC Windows player build 输出路径和 Git 卫生固定成机器可检查契约，避免本地 player build 进入源码提交。

**Implementation:**

- Added `scripts/unity/check_pc_build_artifact_hygiene.ps1`.
- The checker confirms `run_windows_demo.ps1` and `check_windows_demo_build_freshness.ps1` keep using `unity-mc2-demo\Builds\Windows`.
- It confirms README, BUILD-WIN and PC optimization plan document `unity-mc2-demo/Builds/Windows/`.
- It confirms `.gitignore` covers Unity build output paths and common player binary artifacts.
- It uses `git check-ignore` to prove representative Windows player output paths are ignored.
- It checks tracked and staged paths contain no `unity-mc2-demo/Build/` or `unity-mc2-demo/Builds/` artifacts.
- Wired the checker into `scripts/unity/check_current_plan_gate.ps1`.
- Updated README, Windows build notes, master/detailed/mobile/evidence/handoff docs and handoff consistency markers to seal the then-current pass through PC45.

**Acceptance:**

- `check_pc_build_artifact_hygiene.ps1` prints `PC build artifact hygiene check OK`.
- `check_current_plan_gate.ps1` includes an explicit PC build artifact hygiene gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` keeps passing after the new build artifact hygiene contract is documented.
- No screenshot, sidecar, log, APK or build output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_build_artifact_hygiene.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add PC build artifact hygiene check`

### PC46: Add PC Smoke Artifact Hygiene Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不扩大玩法；把 PC smoke、validator、build 和 saved-account 运行证据的 Git 卫生固定成机器可检查契约，避免本地运行产物进入源码提交。

**Implementation:**

- Added `scripts/unity/check_pc_smoke_artifact_hygiene.ps1`.
- The checker confirms README, BUILD-WIN and PC optimization plan document ignored `analysis-output/` PC smoke outputs.
- It confirms `.gitignore` covers `*.log`, `analysis-output/*saved-account*.json` and `analysis-output/*validator*.json`.
- It uses `git check-ignore` to prove representative PC player smoke logs, Unity build/validator logs, validator JSON and saved-account JSON outputs are ignored.
- It checks tracked and staged paths contain no `analysis-output/*.log`, `analysis-output/*saved-account*.json` or `analysis-output/*validator*.json` artifacts.
- Wired the checker into `scripts/unity/check_current_plan_gate.ps1`.
- Updated README, Windows build notes, master/detailed/mobile/evidence/handoff docs and handoff consistency markers for the then-current PC46 checkpoint.

**Acceptance:**

- `check_pc_smoke_artifact_hygiene.ps1` prints `PC smoke artifact hygiene check OK`.
- `check_current_plan_gate.ps1` includes an explicit PC smoke artifact hygiene gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` keeps passing after the new smoke artifact hygiene contract is documented.
- No screenshot, sidecar, log, APK or build output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_smoke_artifact_hygiene.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add PC smoke artifact hygiene check`

### PC47: Add Current Plan Queue Consistency Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不扩大玩法；把“当前计划到底封口到哪里、下一步是不是仍回到 G3 真机 smoke”做成机器可检查契约，避免换机、连续提交或文档同步时误把 G4/G5 提前。

**Implementation:**

- Added `scripts/unity/check_current_plan_queue.ps1`.
- The checker confirmed README, BUILD-WIN, master/detailed/PC/mobile/evidence/handoff docs contained the then-current `PC1-PC47`, `Add current plan queue consistency check`, `check_current_plan_queue.ps1`, `Current plan queue consistency check OK` and `G3 Run Android device smoke` markers.
- It confirms mobile G3 remains `Waiting on Device` while G4/G5 remain `Later`.
- It confirms handoff docs still list `G3 Run Android device smoke` as the formal next planned work.
- Wired the checker into `scripts/unity/check_current_plan_gate.ps1` and `scripts/unity/check_controlled_demo_handoff.ps1`.

**Acceptance:**

- `check_current_plan_queue.ps1` prints `Current plan queue consistency check OK`.
- `check_current_plan_gate.ps1` includes an explicit current plan queue consistency gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` keeps passing after the new PC47 status is documented.
- No G4/G5 implementation starts before G3 real-device smoke.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add current plan queue consistency check`

### PC48: Add Android Device Connection Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不提前做 G4/G5；把手机连接状态从 “adb 无设备” 这种人工判断变成独立机器检查，设备接上后能快速区分未连接、未授权、离线、多设备和 ready。

**Implementation:**

- Added `scripts/unity/check_android_device_connection.ps1`.
- The checker runs `adb version` and `adb devices -l` through the Unity AndroidPlayer SDK path.
- Default mode reports waiting-state strings for no device, unauthorized, offline or multiple-device selection without installing or launching the APK.
- `-RequireDevice` makes missing/unauthorized/offline/ambiguous device states fail for strict G3 runs.
- Wired the checker into `scripts/unity/check_current_plan_gate.ps1`.
- Updated README, Windows build notes, master/detailed/mobile/evidence/handoff docs and handoff consistency markers to seal PC1-PC48.

**Acceptance:**

- `check_android_device_connection.ps1` prints `Android device connection check waiting on device` on the current no-phone machine.
- `check_current_plan_gate.ps1` includes an explicit Android device connection gate.
- `check_controlled_demo_handoff.ps1 -RunReadiness` keeps passing after the PC48 status is documented.
- No Android install or launch is attempted by the connection checker.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_connection.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android device connection check`

### PC49: Wire Android Smoke Connection Gate

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不提前做 G4/G5；把 PC48 的连接诊断接到真实 Android smoke 入口，确保设备到位前不会尝试安装或启动，设备状态解释也不再分散在多套脚本里。

**Implementation:**

- `android_device_smoke.ps1 -PlanOnly` now prints `ConnectionCheck: check_android_device_connection.ps1 -RequireDevice`.
- Real `android_device_smoke.ps1` runs `check_android_device_connection.ps1 -RequireDevice` before `adb install`, launch, logcat, screenshot or summary.
- `check_android_device_preflight.ps1 -AllowNoDevice` now reports the same `device connection` waiting/OK states from `check_android_device_connection.ps1`.
- `check_android_smoke_plan_consistency.ps1` and `check_current_plan_gate.ps1` require the connection marker.
- README, Windows/mobile notes, master/detailed/mobile/evidence/handoff docs and handoff consistency markers now seal PC1-PC49.

**Acceptance:**

- `android_device_smoke.ps1 -PlanOnly` includes `ConnectionCheck: check_android_device_connection.ps1 -RequireDevice`.
- On the current no-phone machine, a real smoke attempt fails before install/launch with `Android device smoke requires a single authorized Android device before install or launch`.
- `check_android_device_preflight.ps1 -AllowNoDevice` keeps reporting `Android device smoke preflight waiting on device` with a `device connection` waiting row.
- `check_android_g3_readiness.ps1` remains waiting on device, not failed for unrelated reasons.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_plan_consistency.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_readiness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Wire Android smoke connection gate`

### PC50: Add Android Smoke Connection Gate Check

**Status:** Completed 2026-06-12.

**Goal:** G3 真机仍不可用时，不提前做 G4/G5；把 PC49 的严格连接门禁变成独立、可复验的机器检查。无授权设备时，它必须证明真实 Android smoke 在安装/启动前失败，且不会改写 smoke log、截图或 summary 证据；有授权设备时只报告可以进入 G3 真机 smoke，不自行安装。

**Implementation:**

- Added `scripts/unity/check_android_smoke_connection_gate.ps1`.
- The script first runs `check_android_device_connection.ps1`.
- If a valid authorized device is present, it reports `Android smoke connection gate check ready for G3 device smoke` and does not install or launch.
- If the machine is waiting on device, authorization, online state or explicit device selection, it runs the real `android_device_smoke.ps1` far enough to require the strict failure marker.
- The script snapshots `analysis-output\android-device-smoke.log`, `analysis-output\android-device-smoke.png` and `analysis-output\android-device-smoke-summary.json`, then confirms those evidence outputs are unchanged when no valid device is selected.
- `check_current_plan_gate.ps1`, `check_current_plan_queue.ps1`, `check_controlled_demo_handoff.ps1`, `check_mobile_command_model_preflight.ps1`, README, Windows/mobile docs, master/detailed/mobile/PC/evidence/handoff docs now seal PC1-PC50.

**Acceptance:**

- `check_android_smoke_connection_gate.ps1` prints `Android smoke connection gate check OK`.
- On the current no-phone machine, it prints `Android smoke connection gate check waiting on device`.
- The real smoke helper still fails before install/launch with `Android device smoke requires a single authorized Android device before install or launch`.
- Existing smoke log, screenshot and summary evidence files are not rewritten before a valid Android device is selected.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_connection_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android smoke connection gate check`

### PC51: Add Android Visible-Flow Command-File Smoke

**Status:** Completed 2026-06-12.

**Goal:** G3 仍等待授权 Android 手机，但真实 Android smoke 到位后不能只接受“进程还在”。它必须进入与 Windows visible-flow 相同的核心路径：战斗、复盘、维修/机库、重新出战或装配回流。PC51 先把这条自动化合同接入 smoke helper 和 gate。

**Implementation:**

- `android_device_smoke.ps1` 默认使用 `unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt` 作为 Android smoke command file。
- 真机运行时，脚本会把 command file 推到 `/sdcard/Android/data/<package>/files/`，再用 Unity Android 启动参数 `-mc2CommandFile <device-path>` 启动 activity。
- PlanOnly 输出 `CommandFileSmoke: True`、`UnityArguments: -mc2CommandFile`、`SmokeSuccessMarker: MC2 debrief summary assertion OK` 和 `SmokeSuccessMarker: MC2 loadout compact assertion OK`。
- 真机 logcat 必须包含复盘 summary 和 loadout compact 两个成功 marker；出现 assertion failure 或 command file blocked 会直接失败。
- Summary schema 增加 `commandFileSmoke`、`commandFilePath`、`deviceCommandFilePath`、`unityArguments` 和 `smokeTestPassed` 字段。
- `check_android_smoke_plan_consistency.ps1`、`check_android_g3_readiness.ps1`、`check_current_plan_gate.ps1`、`check_current_plan_queue.ps1`、`check_controlled_demo_handoff.ps1`、`check_mobile_command_model_preflight.ps1` 和当前计划文档同步到 PC1-PC52。

**Acceptance:**

- `android_device_smoke.ps1 -PlanOnly` reports `CommandFileSmoke: True`.
- `android_device_smoke.ps1 -PlanOnly` reports `UnityArguments: -mc2CommandFile`.
- `android_device_smoke.ps1 -PlanOnly` reports both visible-flow success markers.
- `check_android_smoke_summary.ps1 -SelfTest` accepts command-file smoke summaries.
- `check_android_smoke_plan_consistency.ps1` and `check_android_g3_readiness.ps1` require the command-file smoke markers.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_summary.ps1 -SelfTest
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_plan_consistency.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_readiness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android visible-flow command-file smoke`

### PC52: Add Android WPD-Only Device Diagnosis

**Status:** Completed 2026-06-12.

**Goal:** 用户已把手机接到 Windows，但 adb 仍没有 `device` 行。连接检查需要把“Windows 只识别为 WPD/MTP 媒体设备”从“完全没插手机”里拆出来，避免后续误判为 Unity、APK 或 smoke 脚本问题。

**Implementation:**

- `check_android_device_connection.ps1` 增加 Windows PnP 探测，识别常见 Android vendor id 和手机名称。
- 当 adb 没有设备行但 Windows 看到 WPD/MTP Android 手机时，输出 `WpdOnlyAndroidProbe: True` 和 `WpdOnlyAndroidDevice: True`。
- 当前计划 gate、queue、handoff 和 mobile command preflight 同步到 PC1-PC53。

**Acceptance:**

- `check_android_device_connection.ps1` 始终输出 `WpdOnlyAndroidProbe: True`。
- 当前 Mi 11 Lite 只以 WPD/MTP 可见、adb 无设备行时，输出 `WpdOnlyAndroidDevice: True`，且仍报告 `Android device connection check waiting on device`。
- `check_current_plan_queue.ps1`、`check_current_plan_gate.ps1`、`check_controlled_demo_handoff.ps1 -RunReadiness` 和 `check_mobile_command_model_preflight.ps1` 接受 PC53。

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_connection.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android WPD-only device diagnosis`

### PC53: Add Android ADB Setup Guidance

**Status:** Completed 2026-06-12.

**Goal:** G3 等待授权 Android 手机时，如果设备只以 WPD/MTP 连接、Windows 使用 Microsoft `wpdmtp.inf`/`WUDFWpdMtp` 而不是 ADB interface，连接检查需要把当前 driver/provider/inf/service 和下一步设置建议直接输出，减少人工排查。

**Implementation:**

- `check_android_device_connection.ps1` 为 Windows Android PnP 设备读取 `DEVPKEY_Device_Service`、`DriverProvider`、`DriverDesc` 和 `DriverInfPath`。
- `WindowsAndroidPnpDevices` 摘要附带 driver inf，例如 `driver=wpdmtp.inf`。
- 新增 `adb setup hint` 行，输出 `AdbSetupHint: True`。
- WPD/MTP-only 状态下，hint 会列出当前 MTP driver 并提示启用 USB debugging、接受 RSA 授权、切换 USB 模式或安装对应 vendor id 的 ADB driver。

**Acceptance:**

- `check_android_device_connection.ps1` 始终输出 `AdbSetupHint: True`。
- Mi 11 Lite WPD/MTP-only 样例状态下，输出包含 `provider=Microsoft`、`inf=wpdmtp.inf` 和 `service=WUDFWpdMtp`；当前 ADB-ready 实测状态输出 `inf=winusb.inf` 和 `service=WINUSB`。
- `check_current_plan_gate.ps1` 要求 Android device connection gate 同时包含 `WpdOnlyAndroidProbe: True` 和 `AdbSetupHint: True`。
- `check_current_plan_queue.ps1`、`check_controlled_demo_handoff.ps1 -RunReadiness` 和 `check_mobile_command_model_preflight.ps1` 接受 PC1-PC54。

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_connection.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android ADB setup guidance`

### PC54: Add Android ADB Readiness Watch

**Status:** Completed 2026-06-12.

**Goal:** 手机连线、USB 调试弹窗、Windows 驱动切换这些动作可能需要人工操作和等待。G3 仍不能绕过授权设备要求，但需要一个安全 watcher，能在不安装、不启动 APK 的前提下反复检查 adb 是否出现单个授权 `device`。

**Implementation:**

- 新增 `scripts/unity/watch_android_device_connection.ps1`，循环调用现有 `check_android_device_connection.ps1`。
- 默认模式在超时前等待 `Android device connection check OK.`，成功后输出 `Android device connection watch OK.`。
- `-Once -AllowWaiting` 用于当前 no-device-safe gate，在没有授权设备时输出 `Android device connection watch waiting on device.` 和 `AdbWatchHint: True`，退出码仍为 0。
- `check_current_plan_gate.ps1` 增加 Android device watch gate；queue、handoff 和 mobile command preflight 同步到 PC1-PC54。

**Acceptance:**

- `watch_android_device_connection.ps1 -Once -AllowWaiting` 在 WPD/MTP-only 状态下输出 `AdbWatchHint: True` 和 waiting state；当前 ADB-ready 实测状态输出 `Android device connection watch OK`。
- `check_current_plan_gate.ps1` 运行 Android device watch gate，接受 OK 或 waiting 两种状态，但仍不执行安装/启动。
- `check_current_plan_queue.ps1`、`check_controlled_demo_handoff.ps1 -RunReadiness` 和 `check_mobile_command_model_preflight.ps1` 接受 PC1-PC54。

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\watch_android_device_connection.ps1 -Once -AllowWaiting
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android ADB readiness watch`

### PC55: Add Android G3 Device Status Report

**Status:** Completed 2026-06-12.

**Goal:** G3 仍等待授权 Android 手机。为了让下一次重连、换线、换驱动或换机后能快速判断是否可以开始真机 smoke，需要一个不安装、不启动 APK 的设备状态报告，固化当前 ready/waiting、blocker 和 helper 原始输出。

**Implementation:**

- 新增 `scripts/unity/write_android_g3_device_status.ps1`。
- 默认写入 ignored `analysis-output/android-g3-device-status.json`。
- 报告包含 `g3DeviceStatusReport`、`deviceReady`、`status`、`blocker`、
extGate`、
oInstallOrLaunch` 以及 connection/watch/device requirement 三段输出。
- `check_current_plan_gate.ps1` 增加 Android G3 device status report gate；queue、handoff 和 mobile command preflight 同步到 PC1-PC69。

**Acceptance:**

- `write_android_g3_device_status.ps1` 输出 `Android G3 device status report OK.`。
- WPD/MTP-only 状态下，脚本输出 `G3DeviceStatusReport: True`、`G3DeviceReady: False` 和 `NoInstallOrLaunch: True`；当前 ADB-ready 实测状态输出 `G3DeviceReady: True`，但仍不安装、不启动。
- ignored `analysis-output/android-g3-device-status.json` 可解析，且不进入 git。
- `check_current_plan_queue.ps1`、`check_current_plan_gate.ps1`、`check_controlled_demo_handoff.ps1 -RunReadiness` 和 `check_mobile_command_model_preflight.ps1` 接受 PC1-PC69。

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\write_android_g3_device_status.ps1
powershell -NoProfile -Command "Get-Content .\analysis-output\android-g3-device-status.json -Raw | ConvertFrom-Json | Out-Null; 'Android G3 device status JSON parse OK'"
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android G3 device status report`

### PC56: Add Android G3 When-Ready Runner

**Status:** Completed 2026-06-12.

**Goal:** 设备一旦从 WPD/MTP-only 变成授权 adb `device`，需要一条稳定入口自动进入真实 G3 smoke；设备未就绪时，该入口必须只报告等待状态，不安装、不启动。

**Implementation:**

- 新增 `scripts/unity/run_android_g3_when_ready.ps1`。
- `-PlanOnly` 调用 `android_device_smoke.ps1 -PlanOnly` 并输出 `G3WhenReady: True`。
- 非 PlanOnly 先调用 `watch_android_device_connection.ps1`，只有看到 `Android device connection watch OK.` 才继续调用真实 `android_device_smoke.ps1`。
- `write_android_g3_device_status.ps1` 在 adb 已明确无授权设备时跳过较慢的 strict readiness 子链路，快速输出当前 blocker。
- `check_current_plan_gate.ps1` 增加 Android G3 when-ready plan gate；queue、handoff 和 mobile command preflight 同步到 PC1-PC69。

**Acceptance:**

- `run_android_g3_when_ready.ps1 -PlanOnly` 输出 `Android G3 when-ready plan OK.`、`G3WhenReady: True` 和 `NoInstallOrLaunchUntilDeviceReady: True`。
- `run_android_g3_when_ready.ps1 -TimeoutSeconds 30 -AllowWaiting -LaunchWaitSeconds 75` 在当前 Mi 11 Lite 上安装、启动、推送 visible-flow command file，并输出 `G3WhenReady: True`、`SmokeTestPassed: True`、`status=smokePassed`。
- 如果未来 ADB-ready 设备拒绝 ADB 安装并返回 `INSTALL_FAILED_USER_RESTRICTED`，`-AllowWaiting` 输出 `G3InstallPolicyBlocked: True`，保持 G3 等待手机侧 USB 安装权限。
- 当前计划 gate、handoff 和移动指挥模型预检接受 PC1-PC69、`Pass Android G3 device smoke`、横屏 `G4 Touch UI pass`、`G5 Mobile performance budget`、`G6 iOS feasibility gate`、`F2 map authoring contract`、`F3 web ranking contract`、`F4 creator economy boundary`、`F5 server implementation boundary`、`F6 local main-server prototype`、`F7 document Unity main-server integration contract` 和 `F8 implement optional Unity main-server client adapter`，并把正式下一步推进到 `F9 wire optional Unity main-server adapter into launch/debrief smoke`。

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\run_android_g3_when_ready.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\run_android_g3_when_ready.ps1 -TimeoutSeconds 30 -AllowWaiting -LaunchWaitSeconds 75
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android G3 when-ready runner`

### PC57: Add Android ADB Driver Package Probe

**Status:** Completed 2026-06-12.

**Goal:** G3 曾卡在 Windows 只把手机暴露为 MTP/WPD。为了区分“手机未授权”与“本机缺 ADB/WinUSB 驱动包”，增加一个只读驱动包探针，不安装、不改驱动、不启动 APK。

**Implementation:**

- 新增 `scripts/unity/check_android_adb_driver_package.ps1`。
- 脚本读取当前 Android-like PnP 设备的 provider/desc/inf/service/hardwareIds。
- 脚本只读运行 `pnputil /enum-drivers`，搜索 Android/ADB/Xiaomi/Google/WinUSB 等候选驱动包。
- `check_current_plan_gate.ps1` 增加 Android ADB driver package gate；queue、handoff 和 mobile command preflight 同步到 PC1-PC69。

**Acceptance:**

- 脚本输出 `Android ADB driver package probe OK.`。
- 输出 `AdbDriverPackageProbe: True` 和 `NoInstallOrLaunch: True`。
- 当前机器输出 `CandidateDriverPackages: none`；WPD/MTP-only 时可报告 `CurrentPhoneDriver: ... inf=wpdmtp.inf; service=WUDFWpdMtp`，当前 ADB-ready 实测状态报告 `inf=winusb.inf; service=WINUSB`。
- 当前计划 gate、handoff 和移动指挥模型预检接受 PC1-PC69。

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_adb_driver_package.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android ADB driver package probe`

### G4: Adapt Command UI For Mobile Touch

**Status:** Done.

**Goal:** 保持现有“默认全队、状态栏单选、点地/点目标、Jet、任务地图、系统”的低复杂度操作，同时让它在手机触控和窄屏比例下可用。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify if needed: command-file smoke scripts

**Acceptance:**

- No drag-box selection.
- No dense battle log.
- Touch targets are readable.
- MechLab grid remains usable enough for first mobile pass.

**Commit:** `Adapt command UI for mobile touch`

### G5: Define Mobile Performance Budget

**Status:** Done.

**Goal:** 用第一轮真机数据决定后续视觉、材质、特效、单位规模和资源包约束。

**Files:**

- Create: `docs-mobile-performance-budget-2026-06-10.md`
- Create: `scripts/unity/capture_android_performance_baseline.ps1`
- Create: `scripts/unity/check_mobile_performance_budget.ps1`

**Metrics:**

- FPS.
- Memory.
- Package size.
- First-load time.
- Battle-load time.
- Short-session thermal/battery notes.

**Evidence 2026-06-12:**

- Mi 11 Lite / M2101K9C / Android 13.
-
orth-patrol` steady-state sample: 30.48 FPS after 2 seconds warmup.
- Android TOTAL PSS: 273,342 KB.
- APK: 20,765,252 bytes / 19.80 MiB.
- Thermal Status: 0.
- Battery: USB powered, 100%, 35.0 C.
- `check_mobile_performance_budget.ps1` reports `Mobile performance budget check OK.`

**Commit:** `Define mobile performance budget`

### G6: Document iOS Feasibility Gate

**Status:** Done.

**Goal:** 记录 iOS 所需 macOS、Xcode、签名、Metal、真机测试要求；不让 iOS 签名阻塞 Android 先行验证。

**Files:**

- Create: `docs-ios-feasibility-2026-06-10.md`
- Create: `scripts/unity/check_ios_feasibility_gate.ps1`

**Evidence 2026-06-12:**

- `docs-ios-feasibility-2026-06-10.md` records the Mac/Xcode/signing/iOSSupport handoff lane.
- `check_ios_feasibility_gate.ps1` reports `iOS feasibility gate check OK.`
- Current Windows machine has Unity `6000.4.7f1`, AndroidPlayer and Windows support, but no `iOSSupport`; this is a documented blocker rather than a gameplay blocker.
- Android remains the playable mobile baseline while iOS proof is scheduled on a Mac.

**Commit:** `Document iOS feasibility gate`

### F2: Document Map Authoring Contract

**Status:** Done.

**Goal:** 为未来开源地图编辑器和社区地图包定义最小可验证格式。

**Files:**

- Modify: `docs-platform-ecosystem-plan.md`
- Create if needed: `docs-map-authoring-contract-2026-06-07.md`
- Create: `scripts/unity/check_map_authoring_contract.ps1`

**Map Package Should Include:**

- map id/version/title/author/license/provenance;
- terrain and navigation metadata;
- structures, props, turrets, cover, water;
- objectives and trigger graph;
- enemy waves and patrols;
- allowed squad size;
- difficulty estimate;
- reward table reference, not direct reward grant;
- BattleCore compatibility version.

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_map_authoring_contract.ps1 -RepoRoot .
```

**Acceptance:**

- Community maps can be open and editable.
- Portable rewards remain certified by main server.
- Validator requirements are clear enough to implement later.

**Evidence 2026-06-12:**

- `docs-map-authoring-contract-2026-06-07.md` defines required package fields, trigger graph kinds, rejected direct reward fields, validator failures and certification states.
- `docs-platform-ecosystem-plan.md` links the detailed contract and preserves `Maps can be open; rewards must be certified.`
- `check_map_authoring_contract.ps1` reports `Map authoring contract check OK.`

**Commit:** `Document map authoring contract`

### F3: Document Web Ranking Contract

**Status:** Done.

**Goal:** 规划 Web 侧战绩、排行、地图页、队伍资料和公开展示边界。

**Files:**

- Modify: `docs-platform-ecosystem-plan.md`
- Create if needed: `docs-web-ranking-plan-2026-06-07.md`
- Create: `scripts/unity/check_web_ranking_contract.ps1`

**Public Pages:**

- season leaderboard;
- map ranking;
- player public profile;
- squad loadout snapshot;
- battle record detail;
- creator/map author profile.

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_web_ranking_contract.ps1 -RepoRoot .
```

**Acceptance:**

- Web plan supports ranking and investment story.
- It does not expose private account ids, API keys, unpublished inventory or anti-cheat internals.

**Evidence 2026-06-12:**

- `docs-web-ranking-plan-2026-06-07.md` defines public leaderboard/profile/map/battle-record surfaces and public-safe fields.
- `docs-platform-ecosystem-plan.md` links the detailed Web ranking contract.
- `check_web_ranking_contract.ps1` reports `Web ranking contract check OK.`

**Commit:** `Document web ranking contract`

### F4: Document Creator Economy Boundary

**Status:** Done.

**Goal:** 把地图贡献、皮肤、自定义、收入分配和可选链上实验放在正确阶段，避免早期绑死核心玩法。

**Files:**

- Modify: `docs-platform-ecosystem-plan.md`
- Create if needed: `docs-creator-economy-boundary-2026-06-07.md`
- Create: `scripts/unity/check_creator_economy_boundary.ps1`

**Rules:**

- Centralized ledger first.
- Creator revenue accounting can exist before chain.
- Chain later只适合 proof of revenue share、creator pools、cosmetic ownership proof、commemorative items。
- Core combat、机甲数值、武器数值、维修、普通库存变化和正常战斗结果不放链上。

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_creator_economy_boundary.ps1 -RepoRoot .
```

**Acceptance:**

- Chain is optional and late.
- Core gameplay remains deterministic and locally testable.

**Evidence 2026-06-12:**

- `docs-creator-economy-boundary-2026-06-07.md` defines centralized ledger first, creator contribution types, revenue-share scope, moderation rollback and late optional chain use.
- `docs-platform-ecosystem-plan.md` links the detailed creator economy boundary and keeps core combat, stats, repair, normal inventory and anti-cheat-sensitive state off-chain.
- `check_creator_economy_boundary.ps1` reports `Creator economy boundary check OK.`

**Commit:** `Document creator economy boundary`

### F5: Document Server Implementation Boundary

**Status:** Done.

**Goal:** 在进入真实服务器代码之前，定义最小主服务器原型边界，避免把账号、库存、奖励、地图认证、排行和创作者经济一次性铺开。

**Files:**

- Modify: `docs-platform-ecosystem-plan.md`
- Create if needed: `docs-server-implementation-boundary-2026-06-07.md`
- Create: `scripts/unity/check_server_implementation_boundary.ps1`

**Rules:**

- Start as a modular main-server prototype, not many microservices.
- First slice covers account id, token ledger, inventory snapshot, signed squad loadout, reward claim endpoint and basic leaderboard.
- Map server stays outside first server slice except for future session/result contracts.
- No payment, marketplace, realtime PvP, chain integration or full moderation dashboard in the first server implementation.

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_server_implementation_boundary.ps1 -RepoRoot .
```

**Acceptance:**

- Server scope is small enough to implement and test locally.
- Existing BattleCore/Unity demo remains runnable without a remote server.

**Evidence 2026-06-12:**

- `docs-server-implementation-boundary-2026-06-07.md` defines the first local main-server slice, exclusions, modules, records, endpoints and Unity offline boundary.
- `docs-platform-ecosystem-plan.md` links the detailed server implementation boundary and excludes payment, marketplace, realtime PVP, chain integration, public map server registration and remote Unity dependency from the first server slice.
- `check_server_implementation_boundary.ps1` reports `Server implementation boundary check OK.`

**Commit:** `Document server implementation boundary`

### F6: Scaffold Local Main-Server Prototype

**Status:** Done.

**Goal:** 按 F5 边界搭一个本地主服务器原型骨架，先证明 health/version、fixture account/inventory、签名小队、奖励申领和基础排行榜可以本地启动和测试。

**Files:**

- Create if needed: `server/`
- Create if needed: `scripts/server/`
- Modify if needed: `README.md`

**Rules:**

- Keep it local and deterministic.
- Do not require Unity to call the server yet.
- Do not add payment, marketplace, realtime PVP, chain integration or public map server registration.
- Include a smoke command that starts or exercises the server without touching Unity build artifacts.

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\server\check_local_main_server.ps1 -RepoRoot .
```

**Acceptance:**

- A developer can run one command to prove the server prototype contract.
- The existing Unity validator, Windows smoke and Android smoke remain offline-first.

**Evidence 2026-06-12:**

- `server/main-server/main-server.mjs` implements local `GET /healthz`, `GET /version`, `POST /dev/accounts`, `GET /accounts/{accountId}/inventory`, `POST /squads/sign`, `POST /reward-claims`, `GET /leaderboards/basic` and `POST /dev/reset`.
- `server/main-server/fixtures/local-dev-fixture.json` provides deterministic `AccountRecord`, `PublicProfileRecord`, `InventorySnapshot` and seed `TokenLedgerEntry`.
- `server/main-server/smoke.mjs` starts the server in-process and proves fixture account/inventory, signed squad loadout, idempotent reward claim, token ledger update and basic leaderboard output without touching Unity build artifacts.
- `scripts/server/check_local_main_server.ps1` reports `Local main-server prototype check OK.`

**Commit:** `Scaffold local main-server prototype`

### F7: Document Unity Main-Server Integration Contract

**Status:** Done.

**Goal:** 写清 Unity 后续如何以可选方式调用本地主服务器：启动前可请求签名小队，战后可提交奖励 claim，但 validator、Windows smoke、Android smoke 和 MechLab 必须继续支持离线 fixture。

**Files:**

- `docs-unity-main-server-integration-contract-2026-06-12.md`
- `scripts/unity/check_unity_main_server_integration_contract.ps1`
- `README.md`
- `BUILD-WIN.md`
- `BUILD-MOBILE.md`
- Current plan, platform plan and handoff docs

**Rules:**

- No Unity runtime dependency on the server in this step.
- No account login, payment, marketplace, realtime PVP, chain integration or public map upload.
- Keep request/response shapes compatible with `server/main-server`.
- Define explicit fallback behavior for server unavailable, unsigned squad, rejected claim and duplicate claim.

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\server\check_local_main_server.ps1 -RepoRoot .
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_unity_main_server_integration_contract.ps1 -RepoRoot .
```

**Acceptance:**

- A future Unity client adapter can be implemented from the contract without changing server endpoint names.
- The current Unity demo remains offline-first and can ignore the server entirely.

**Commit:** `Document Unity main-server integration contract`

### F8: Implement Optional Unity Main-Server Client Adapter

**Status:** Completed 2026-06-12.

**Goal:** 在 Unity Demo 内实现一个默认可关闭、失败可回退的主服务器客户端适配层：出战前可请求 `POST /squads/sign`，战后可提交 `POST /reward-claims`，但 validator、Windows smoke、Android smoke、MechLab 和 BattleCore 帧循环继续离线优先。

**Files:**

- Add if needed: Unity C# DTO/client files under `unity-mc2-demo/Assets/Scripts/BattleCore` or a narrow integration namespace.
- Modify if needed: smoke/validator scripts only for opt-in contract coverage.
- Do not modify server endpoint names from F6/F7.

**Rules:**

- Server use must be opt-in and disabled by default for existing smoke.
- No per-frame server calls.
- No login, payment, marketplace, realtime PVP, chain integration or public map upload.
- Every failure path uses the offline fixture fallback defined in F7.

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\server\check_local_main_server.ps1 -RepoRoot .
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_unity_main_server_integration_contract.ps1 -RepoRoot .
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_optional_unity_main_server_client_adapter.ps1 -RepoRoot .
```

**Acceptance:**

- Unity has a typed optional boundary for signed squad and reward claim.
- Existing offline demo gates still pass with no server process running.

**Commit:** `Implement optional Unity main-server client adapter`

### F9: Wire Optional Unity Main-Server Adapter Into Launch/Debrief Smoke

**Status:** Done.

**Goal:** 把 F8 的 `UnityMainServerClient` 只接入显式 opt-in smoke：出战前尝试签名小队，战后只在签名有效时提交奖励 claim；默认 validator、Windows smoke、Android smoke、MechLab 和 BattleCore 帧循环仍离线优先。

**Files:**

- Modify if needed: Unity presentation command-file/smoke hook code.
- Modify if needed: smoke command fixture under `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts`.
- Add if needed: `scripts/unity/check_optional_unity_main_server_launch_debrief_smoke.ps1`.
- Do not make server startup a default build or demo prerequisite.

**Rules:**

- Opt-in flag required for any server call.
- Server calls are limited to pre-launch signing and post-debrief claim.
- No per-frame server calls, no AI decisions on server, no damage/pathing authority on server.
- If the server is unavailable, unsigned or rejects a claim, the local demo must remain playable and visible.

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\server\check_local_main_server.ps1 -RepoRoot .
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_optional_unity_main_server_client_adapter.ps1 -RepoRoot .
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_optional_unity_main_server_launch_debrief_smoke.ps1 -RepoRoot .
```

**Acceptance:**

- A dedicated opt-in smoke path can prove signed squad and reward claim through the local main-server.
- Existing offline demo gates still pass with no server process running.
- Mobile remains landscape-only; F9 must not introduce portrait UI work.

**Commit:** `Wire optional Unity main-server adapter into launch/debrief smoke`

### F10: Wire Optional Unity Inventory Bootstrap Smoke

**Status:** Done.

**Result:** Added explicit `inventory-bootstrap-smoke` and `assert-inventory-bootstrap-smoke` command-file actions. The opt-in Unity path now calls `TryBootstrapInventory` before launch, logs dev account, `tokenBalance=12000`, `ownedMechs=3`, `itemStacks=6`, and `NoPerFrameServerCalls: True`, and keeps local fixture inventory as the playable source of truth. The no-server fallback command file proves the local demo still reaches debrief when the main-server is unavailable.

**Goal:** 把 F8 的 `TryBootstrapInventory` 只接入显式 opt-in smoke：启动/出战前尝试拉取本地主服务器 dev account 和 inventory 快照，记录账号、代币、机甲和物品数量；默认 validator、Windows smoke、Android smoke、MechLab 和 BattleCore 帧循环仍离线优先。

**Files:**

- Modify if needed: Unity presentation command-file/smoke hook code.
- Modify if needed: smoke command fixture under `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts`.
- Add if needed: `scripts/unity/check_optional_unity_inventory_bootstrap_smoke.ps1`.
- Do not make server startup a default build or demo prerequisite.

**Rules:**

- Opt-in flag required for any inventory bootstrap server call.
- Bootstrap is a pre-launch snapshot only; no per-frame server calls.
- Local fixture inventory remains the playable source of truth if the server is unavailable.
- No login, payment, marketplace, realtime PVP or chain work in this task.

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\server\check_local_main_server.ps1 -RepoRoot .
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_optional_unity_main_server_client_adapter.ps1 -RepoRoot .
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_optional_unity_inventory_bootstrap_smoke.ps1 -RepoRoot .
```

**Acceptance:**

- A dedicated opt-in smoke path can prove dev account/inventory bootstrap through the local main-server.
- Existing offline demo gates still pass with no server process running.
- Mobile remains landscape-only; F10 must not introduce portrait UI work.

**Commit:** `Wire optional Unity inventory bootstrap smoke`

### F11: Plan Inventory-To-MechBay Binding Boundary

**Status:** Done.

**Result:** Added `docs-inventory-mechbay-binding-boundary-2026-06-12.md` and
`scripts/unity/check_inventory_mechbay_binding_boundary.ps1`. The boundary now
defines server-to-MechBay projection rules, validation gates, fallback behavior,
runtime call timing, explicit non-goals and the next F12 implementation shape.

**Goal:** 写清主服务器 inventory bootstrap 后续如何进入 MechLab/机库：哪些字段可展示，哪些字段不能直接驱动 BattleCore，离线 fixture 如何保持默认来源，opt-in 绑定如何验证，移动横屏 UI 如何避免增加复杂登录/商店/支付入口。

**Files:**

- Add if needed: `docs-inventory-mechbay-binding-boundary-2026-06-12.md`.
- Add if needed: `scripts/unity/check_inventory_mechbay_binding_boundary.ps1`.
- Modify plan docs and current gate after the boundary is written.

**Rules:**

- No default remote dependency for Windows smoke, Android smoke, MechLab or BattleCore frames.
- No login, payment, marketplace, realtime PVP or chain work.
- The first binding must be opt-in and reversible to local fixture data.
- Mobile remains landscape-only.

**Acceptance:**

- A boundary doc defines inventory-to-MechBay field mapping, fallback rules and verification gates.
- Current plan queue advances without requiring a remote server for default demo paths.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_inventory_mechbay_binding_boundary.ps1 -RepoRoot .
```

**Commit:** `Document inventory-to-MechBay binding boundary`

### F12: Implement Opt-In Inventory-To-MechBay Preview Binding

**Status:** Next.

**Goal:** 只在显式 opt-in smoke 或后续调试入口中，把主服务器 inventory snapshot 投影成 `MechBayInventoryContract` 预览。默认 Windows/Android visible-flow、MechLab 本地 fixture、BattleCore 帧循环和手机横版布局都不能被远端库存绑死。

**Files:**

- Modify if needed: `unity-mc2-demo/Assets/Scripts/Services/UnityMainServerClient.cs`.
- Add if needed: Unity projection helper from `UnityInventorySnapshot` to `MechBayInventoryContract`.
- Add if needed: command-file action and PowerShell gate for preview binding.
- Update docs and current plan gate after the opt-in smoke exists.

**Rules:**

- Server inventory is not combat authority.
- No per-frame server calls.
- Fallback must keep `MechBayInventoryBuilder.BuildDemoInventory` usable.
- No login, payment, marketplace, realtime PVP, map-server upload or chain work.
- Mobile remains landscape-only; no portrait UI or modal account flow.

**Acceptance:**

- Opt-in preview path validates projected server inventory through `MechBayInventoryValidator`.
- No-server path proves local MechBay and launch flow still work.
- Logs or sidecar metadata show preview source, fallback reason and `NoPerFrameServerCalls`.
- Current plan queue advances without requiring a remote server for default demo paths.

## 6. Validation Matrix

| Change Type | Required Commands |
| --- | --- |
| Docs only | `git diff --check` |
| BattleCore rules | `git diff --check`; Unity validator |
| Unity presentation | `git diff --check`; Windows build; relevant captures |
| MechLab UI | `git diff --check`; Windows build; `mechlab` capture; visible-flow if launch path changed |
| Battle HUD | `git diff --check`; Windows build; visible-flow smoke; `spawn,damage-demo` captures |
| Collision/occupancy | `git diff --check`; Unity validator; `hangar-contact,damage-demo` captures |
| Public content boundary | `git diff --check`; `check_public_content_boundary.ps1 -DryRun` |
| AI deputy | `git diff --check`; Unity validator; no-token/offline path check |

Canonical commands:

```powershell
git diff --check
git status --short --branch --untracked-files=all
```

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-current.log"
```

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-current.log"
```

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-current.log"
```

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol
```

## 7. First Controlled Demo Definition Of Done

Demo 可以给少量外部人看时，必须满足：

1. Windows build passes.
2. Mission validator passes.
3. Visible-flow smoke reaches debrief, repair and relaunch.
4. `mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol` captures pass.
5. `mechlab` shows whole weapon blocks, armor/cooling fillers, heat/weight/grid pressure and no weapon toggle.
6. `spawn` shows sparse battle HUD and commander-follow camera.
7. `airfield` shows terrain, water, runway/road, buildings and first enemy direction.
8. `hangar-contact` shows dense tactical contact with `ContactClearance overlaps=0`.
9. `damage-demo` shows weapon families and section damage story.
10. Normal battle UI does not show save slots, account UI, large combat log or debug overlays.
11. AI deputy remains optional and offline-safe.
12. README and evidence docs do not pitch the project as a clone.
13. Development-only reference content is clearly marked.
14. Public-safe slice has a written replacement path and provenance requirements.
15. Controlled demo readiness, handoff consistency, Android preflight, PC core playable contract, mobile command model preflight, current plan gate and Android smoke log scanner checks all pass or report the expected waiting-on-device state.
16. PC core playable contract check proves command state, solo return, Jet legality, occupancy, damage/ejection and debrief/relaunch remain covered by the Unity/BattleCore validator.
17. Mobile command model preflight proves the current PC command surface still matches the planned mobile low-complexity model before any G4 touch work starts.
18. Current plan gate check proves the current waiting state from one command: handoff/readiness OK, Windows build freshness OK, mobile command model OK, Android OK or waiting on authorized phone.
19. Android smoke log scanner self-test passes and the real device smoke helper calls it after logcat capture.
20. Android smoke plan mode proves the real-device smoke helper can resolve APK/tool paths and planned actions before a phone is connected.
21. Battle HUD sparse contract check proves source, capture gate and mobile command model preflight agree on sparse battle HUD requirements before any new PC visual pass.
22. Demo source hygiene check proves tracked/staged paths and `.gitignore` keep generated evidence, Unity builds, APK/AAB outputs and private reference art out of source commits.
23. AI deputy contract check proves the optional model path stays high-level, fallback-safe, smoke-token-free by default and absent from frame loops.
24. Windows demo build freshness check proves ignored player output is newer than tracked Unity build inputs before readiness is accepted.
25. Controlled demo evidence freshness check proves visible-flow and six capture PNG/JSON sidecars are newer than current Windows build/evidence inputs.
26. Controlled demo capture log freshness check proves six capture logs exist, are current, and contain preset, screenshot request and sidecar write markers.
27. Android APK freshness check proves the ignored APK is current before G3 preflight or device smoke can install it.
28. Android APK identity check proves package name and launch activity match the expected G3 install/launch identity.
29. Android APK compatibility check proves min SDK, target SDK and native ABI metadata match the expected Android smoke target.
30. Android APK signing check proves `apksigner verify`, APK Signature Scheme v2 and the expected debug signer DN pass before G3 install/launch.
31. Android APK manifest check proves expected permissions, no required hardware features, expected not-required features and broad screen support before G3 install/launch.
32. Android APK payload check proves required Unity/IL2CPP native libraries, `assets/bin/Data` runtime files and the expected `arm64-v8a` ABI folder are present before G3 install/launch.
33. Android APK size budget check proves the package is neither implausibly small nor above the current 100 MiB early mobile demo budget before G3 install/launch.
34. Android SDK tooling check proves Unity's AndroidPlayer SDK, NDK, OpenJDK, build-tools, platform, adb, aapt and apksigner are present before G3 install/launch.
35. Android smoke artifact hygiene check proves APK/AAB outputs, Android smoke logs/screenshots and `Builds/Android` outputs remain ignored and absent from tracked/staged source paths.
36. Android smoke screenshot evidence capture proves real-device smoke will write ignored `analysis-output\android-device-smoke.png`, and plan mode exposes `ScreenshotCapture: True`.
37. Android smoke summary evidence output proves real-device smoke will write ignored `analysis-output\android-device-smoke-summary.json`, and plan mode exposes `SummaryWrite: True`.
38. Android smoke summary schema check proves the ignored summary JSON can be self-tested on PC and is automatically validated after real-device smoke writes it.
39. Android smoke summary preflight check proves `check_android_device_preflight.ps1 -AllowNoDevice` directly runs the summary schema self-test before reporting waiting-on-device.
40. Android smoke plan/preflight consistency check proves `android_device_smoke.ps1 -PlanOnly` and `check_android_device_preflight.ps1 -AllowNoDevice` agree on package, activity, ignored evidence paths, execution flags and summary schema readiness.
41. Android G3 readiness check proves the direct mobile gate bundles device preflight, plan/preflight consistency, smoke plan, log scanner and summary schema checks before real device install.
42. Android G3 device requirement check proves strict G3 readiness cannot be accepted without an authorized Android phone.
43. PC visual capture sanity check proves the six controlled-demo PNG captures have expected size, sampled color variety, center visibility, luminance contrast, low magenta fallback color and non-monochrome content.
44. PC visual capture sanity self-test proves the image gate detects valid, flat and magenta fallback sample images before accepting current captures.
45. PC capture sidecar schema check proves the six controlled-demo JSON sidecars have matching screenshot paths, expected dimensions, flow state, camera state, core counters, summary fields and reference-asset metadata before they are accepted as evidence.
46. PC capture preset contract check proves the standard six controlled-demo presets remain `mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol` across capture generation, evidence, visual sanity, sidecar schema and handoff docs.
47. PC capture artifact hygiene check proves local reference screenshots, JSON sidecars, capture logs and visual sanity self-test images remain ignored generated evidence and are absent from tracked/staged source paths.
48. PC window contract check proves the controlled PC launcher and reference capture helper keep `1280x720` windowed defaults and pass `-screen-fullscreen 0`.
49. PC launch log hygiene check proves the controlled PC launcher runtime log stays fixed to ignored `analysis-output/windows-demo-run.log` and that `analysis-output/*.log` paths are absent from tracked/staged source paths.
50. PC build artifact hygiene check proves the Windows player output stays fixed to ignored `unity-mc2-demo/Builds/Windows/` paths and that Unity player build artifacts are absent from tracked/staged source paths.
51. PC smoke artifact hygiene check proves PC smoke, validator, build and saved-account evidence outputs stay under ignored `analysis-output/` paths and that those generated artifacts are absent from tracked/staged source paths.
52. Current plan queue consistency check proves README, BUILD-WIN, master/detailed/PC/mobile/evidence/handoff docs and helper scripts agree on the latest PC wait-state checkpoint, latest PC commit marker, current queue checker and `G3 Run Android device smoke`.
53. Android device connection check proves `adb devices -l` is readable and reports no-device, unauthorized, offline, multi-device or ready states before G3 tries to install or launch the APK.
54. Android WPD-only device diagnosis proves a Windows-visible WPD/MTP Android phone without an adb `device` row remains a G3 waiting state and reports `WpdOnlyAndroidProbe: True`.
55. Android ADB setup guidance proves WPD/MTP-only output includes `AdbSetupHint: True`, current driver/provider/inf/service, and explicit next action before G3 install or launch.
56. Android ADB driver package probe proves `check_android_adb_driver_package.ps1` reports `AdbDriverPackageProbe: True`, current phone driver details and installed ADB/WinUSB candidate package state without installing, launching or changing drivers.
57. Android ADB readiness watch proves `watch_android_device_connection.ps1 -Once -AllowWaiting` can safely report `AdbWatchHint: True` and waiting state without installing or launching, while the normal mode can wait for one adb `device` row.
58. Android G3 device status report proves `write_android_g3_device_status.ps1` writes ignored `analysis-output/android-g3-device-status.json`, reports `G3DeviceStatusReport: True`, and records ready/waiting state without installing or launching.
59. Android G3 when-ready runner proves `run_android_g3_when_ready.ps1 -PlanOnly` reports `G3WhenReady: True` and `NoInstallOrLaunchUntilDeviceReady: True`, then only calls real `android_device_smoke.ps1` after the adb watch reports one authorized device.
60. Android smoke connection gate wiring proves `android_device_smoke.ps1 -PlanOnly` exposes `ConnectionCheck: check_android_device_connection.ps1 -RequireDevice` and real smoke fails before install or launch with `Android device smoke requires a single authorized Android device before install or launch` unless the strict connection gate passes.
61. Android smoke connection gate check proves the real smoke helper fail-fast path is machine-checked without installing or launching, and no smoke log, screenshot or summary evidence is rewritten before a valid Android device is selected.
62. Android visible-flow command-file smoke proves `android_device_smoke.ps1 -PlanOnly` exposes `CommandFileSmoke: True`, `UnityArguments: -mc2CommandFile`, and the debrief/loadout success markers that real G3 will require after pushing `mc2_01-visible-flow-audit.txt` to the device.

## 8. Stop Conditions

Stop and reassess before committing if:

- `git status --short` shows unrelated user changes in files being edited.
- Unity scene changes are only fileID churn.
- A visual fix creates BattleCore/Unity disagreement about collisions.
- `hangar-contact` or `damage-demo` loses `ContactClearance overlaps=0`.
- Battle UI regrows large log, save UI, account UI or debug overlay.
- A public-facing doc says or implies private reference art is final product art.
- Any private original-derived file is staged.
- AI code starts calling model APIs from update loops or smoke tests.
- A platform task tries to implement server code before local Demo is stable.

## 9. One-Line Direction

Windows 本地 Demo 的画面、碰撞、稀疏 UI、MechLab、损伤故事、受控演示证据、PC 视觉截图 sanity 与自测、PC 截图 sidecar schema、PC 截图 preset 契约、PC 截图生成物卫生、PC 受控窗口尺寸契约、PC 启动日志卫生、PC 构建输出卫生、PC smoke 生成物卫生、当前计划队列一致性、Android 设备连接诊断、Android WPD-only device diagnosis、Android ADB setup guidance、Android ADB driver package probe、Android ADB readiness watch、Android G3 device status report、Android G3 when-ready runner、Android smoke 真实入口连接检查、Android smoke 连接 gate 自测、Android visible-flow command-file smoke、启动预检、构建新鲜度检查、证据健康检查、证据新鲜度检查、capture 日志新鲜度检查、Android SDK 工具链检查、Android APK 新鲜度检查、Android APK 身份检查、Android APK 兼容性检查、Android APK 签名检查、Android APK 清单检查、Android APK 载荷检查、Android APK 包体预算检查、Android smoke 生成物卫生检查、Android smoke 截图证据捕获、Android smoke 摘要证据输出、Android smoke 摘要 schema 检查、Android smoke 摘要 preflight 检查、Android smoke 预演/前置一致性检查、Android G3 readiness 检查、Android G3 真机要求检查、公开边界预检、演示总预检、交接一致性检查、Android 真机 smoke 前置检查、PC 核心玩法合约检查、移动指挥模型预检、战斗 HUD 稀疏合约检查、源码/生成物卫生检查、AI 副官边界检查、当前计划 gate 总预检、Android smoke 日志崩溃扫描、Android smoke 预演模式、Android performance baseline capture、mobile performance budget check、iOS feasibility gate check、map authoring contract check、web ranking contract check、creator economy boundary check、server implementation boundary check、local main-server prototype check、Unity main-server integration contract check、optional Unity main-server client adapter check、optional Unity main-server launch/debrief smoke check、optional Unity inventory bootstrap smoke check、inventory-to-MechBay binding boundary check、公开 art-safe 元数据合同、AI 副官离线边界和主服务器奖励权威契约已经收稳；代码已推到 GitHub，H2 validator/build/smoke 已过，G2 Android APK build smoke 已过；PC1-PC69 PC/移动等待态优化包已封口；`Pass Android G3 device smoke`、横屏 `G4 Touch UI pass`、`G5 Mobile performance budget`、`G6 iOS feasibility gate`、`F2 map authoring contract`、`F3 web ranking contract`、`F4 creator economy boundary`、`F5 server implementation boundary`、`F6 local main-server prototype`、`F7 document Unity main-server integration contract`、`F8 implement optional Unity main-server client adapter`、`F9 wire optional Unity main-server adapter into launch/debrief smoke`、`F10 wire optional Unity inventory bootstrap smoke` 和 `F11 plan inventory-to-MechBay binding boundary` 已完成；手机端第一版固定横屏；`F12 implement opt-in inventory-to-MechBay preview binding` 已完成；`F13 capture opt-in MechBay preview evidence` 已完成；`F14 capture landscape-phone MechLab source-line evidence` 已完成；F15 plan server-backed receipt slice 已完成；F16 implement server-backed receipt evidence gate 已完成；F17 plan post-receipt inventory refresh boundary 已完成；F18 implement opt-in post-receipt inventory refresh binding 已完成；F19 capture opt-in post-receipt refresh evidence 已完成；F20 refresh Android landscape build/smoke evidence 已完成；`F21 audit landscape touch UI ergonomics` 已完成；`F22 audit landscape MechLab touch controls` 已完成；`F23 capture landscape MechLab touch evidence` 已完成；`F24 capture Android MechLab touch evidence` 已完成；F25 capture Android battle command touch evidence 已完成；`F26 reduce Android combat effect log noise` 已完成；`F27 audit Android entity placeholder collision path` 已完成，证据 gate: `scripts/unity/check_android_entity_placeholder_collision_path.ps1` -> `Android entity placeholder collision path check OK`；F28 capture Android entity placeholder collision runtime evidence 已完成，证据 gate: `scripts/unity/capture_android_entity_placeholder_collision_runtime_evidence.ps1` -> `Android entity placeholder collision runtime evidence capture OK`；F29 audit PC controlled-demo visual readability 已完成，证据 gate: `scripts/unity/audit_pc_controlled_demo_visual_readability.ps1` -> `PC controlled-demo visual readability audit OK`；F30 implement PC controlled-demo visual readability fixes 已完成，证据 gate: `scripts/unity/check_pc_controlled_demo_visual_readability_fixes.ps1` -> `PC controlled-demo visual readability fixes check OK`；F31 refresh PC controlled-demo visual evidence after readability fixes complete; Evidence gate: `scripts/unity/capture_pc_controlled_demo_visual_evidence.ps1` -> `PC controlled-demo visual evidence refresh OK`; F32 audit PC controlled-demo command readability and formation feel complete; Evidence gate: `scripts/unity/audit_pc_controlled_demo_command_readability_formation.ps1` -> `PC controlled-demo command readability formation audit OK`; `F33 implement PC controlled-demo command readability and formation fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_command_readability_fixes.ps1` -> `PC controlled-demo command readability fixes check OK`; `F34 refresh PC controlled-demo command evidence after readability fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F35 audit post-F34 PC controlled-demo playable flow polish` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_playable_flow_polish.ps1` -> `PC controlled-demo playable flow polish audit OK`; `F36 implement post-F34 PC controlled-demo playable flow polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_playable_flow_polish_fixes.ps1` -> `PC controlled-demo playable flow polish fixes check OK`; `F37 refresh PC controlled-demo playable-flow evidence after polish fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F38 audit post-F37 PC controlled-demo investor readiness` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_readiness.ps1` -> `PC controlled-demo investor readiness audit OK`; `F39 implement post-F37 PC controlled-demo investor readiness fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_readiness_fixes.ps1` -> `PC controlled-demo investor readiness fixes check OK`; `F40 refresh PC controlled-demo investor-readiness evidence after fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F41 audit post-F40 PC controlled-demo investor evidence package` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_package.ps1` -> `PC controlled-demo investor evidence package audit OK`; `F42 implement post-F41 PC controlled-demo investor evidence package fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_package_fixes.ps1` -> `PC controlled-demo investor evidence package fixes check OK`; `F43 refresh PC controlled-demo investor evidence package after fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_refresh.ps1` -> `PC controlled-demo investor evidence refresh check OK`; `F44 audit post-F43 PC controlled-demo investor evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_refresh.ps1` -> `PC controlled-demo investor evidence refresh audit OK`; `F45 implement post-F44 PC controlled-demo investor evidence polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_polish_fixes.ps1` -> `PC controlled-demo investor evidence polish fixes check OK`; `F46 refresh PC controlled-demo investor route evidence after polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_refresh.ps1` -> `PC controlled-demo investor route evidence refresh check OK`; `F47 audit post-F46 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_refresh.ps1` -> `PC controlled-demo investor route evidence refresh audit OK`; `F48 implement post-F47 PC controlled-demo investor route evidence audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fixes check OK`; `F49 refresh PC controlled-demo investor route evidence after audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh check OK`; `F50 audit post-F49 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit OK`; `F51 implement post-F50 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fixes check OK`; `F52 refresh PC controlled-demo investor route evidence after F50 audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh check OK`; `F53 audit post-F52 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit OK`; `F54 implement post-F53 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fixes check OK`; next task was `F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

## F12 Preview Binding Checkpoint

`F12 implement opt-in inventory-to-MechBay preview binding` is complete. `F13 capture opt-in MechBay preview evidence` is complete. `F14 capture landscape-phone MechLab source-line evidence` is complete. The opt-in gate is `scripts/unity/check_optional_inventory_mechbay_preview_binding.ps1`, with expected success string `Optional inventory-to-MechBay preview binding check OK`; the preview evidence gate is `scripts/unity/capture_inventory_mechbay_preview_evidence.ps1`, with expected success string `Inventory MechBay preview evidence capture OK`; the landscape-phone evidence gate is `scripts/unity/capture_landscape_phone_mechlab_source_line_evidence.ps1`, with expected success string `Landscape-phone MechLab source-line evidence capture OK`. `F15 plan server-backed receipt slice` is complete. Evidence gate: `scripts/unity/check_server_backed_receipt_slice_plan.ps1` -> `Server-backed receipt slice plan check OK`. `F16 implement server-backed receipt evidence gate` is complete. Evidence gate: `scripts/unity/capture_server_backed_receipt_evidence.ps1` -> `Server-backed receipt evidence capture OK`. `F17 plan post-receipt inventory refresh boundary` is complete. Evidence gate: `scripts/unity/check_post_receipt_inventory_refresh_boundary.ps1` -> `Post-receipt inventory refresh boundary check OK`. `F18 implement opt-in post-receipt inventory refresh binding` is complete. Evidence gate: `scripts/unity/check_post_receipt_inventory_refresh_binding.ps1` -> `Post-receipt inventory refresh binding check OK`. `F19 capture opt-in post-receipt refresh evidence` is complete. Evidence gate: `scripts/unity/capture_post_receipt_refresh_evidence.ps1` -> `Post-receipt refresh evidence capture OK`. `F20 refresh Android landscape build/smoke evidence` is complete. `F21 audit landscape touch UI ergonomics` is complete. Evidence gate: `scripts/unity/check_landscape_touch_ui_ergonomics.ps1` -> `Landscape touch UI ergonomics check OK`. `F22 audit landscape MechLab touch controls` is complete. Evidence gate: `scripts/unity/check_landscape_mechlab_touch_controls.ps1` -> `Landscape MechLab touch controls check OK`. `F23 capture landscape MechLab touch evidence` is complete. Evidence gate: `scripts/unity/capture_landscape_mechlab_touch_evidence.ps1` -> `Landscape MechLab touch evidence capture OK`. `F24 capture Android MechLab touch evidence` is complete. Evidence gate: `scripts/unity/capture_android_mechlab_touch_evidence.ps1` -> `Android MechLab touch evidence capture OK`. F25 capture Android battle command touch evidence is complete. Evidence gate: `scripts/unity/capture_android_battle_command_touch_evidence.ps1` -> `Android battle command touch evidence capture OK`. `F26 reduce Android combat effect log noise` is complete. `F27 audit Android entity placeholder collision path` is complete. Evidence gate: `scripts/unity/check_android_entity_placeholder_collision_path.ps1` -> `Android entity placeholder collision path check OK`. `F28 capture Android entity placeholder collision runtime evidence` is complete. Evidence gate: `scripts/unity/capture_android_entity_placeholder_collision_runtime_evidence.ps1` -> `Android entity placeholder collision runtime evidence capture OK`. `F29 audit PC controlled-demo visual readability` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_visual_readability.ps1` -> `PC controlled-demo visual readability audit OK`. `F30 implement PC controlled-demo visual readability fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_visual_readability_fixes.ps1` -> `PC controlled-demo visual readability fixes check OK`. `F31 refresh PC controlled-demo visual evidence after readability fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_visual_evidence.ps1` -> `PC controlled-demo visual evidence refresh OK`; `F32 audit PC controlled-demo command readability and formation feel` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_command_readability_formation.ps1` -> `PC controlled-demo command readability formation audit OK`; `F33 implement PC controlled-demo command readability and formation fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_command_readability_fixes.ps1` -> `PC controlled-demo command readability fixes check OK`; `F34 refresh PC controlled-demo command evidence after readability fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F35 audit post-F34 PC controlled-demo playable flow polish` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_playable_flow_polish.ps1` -> `PC controlled-demo playable flow polish audit OK`; `F36 implement post-F34 PC controlled-demo playable flow polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_playable_flow_polish_fixes.ps1` -> `PC controlled-demo playable flow polish fixes check OK`; `F37 refresh PC controlled-demo playable-flow evidence after polish fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F38 audit post-F37 PC controlled-demo investor readiness` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_readiness.ps1` -> `PC controlled-demo investor readiness audit OK`; `F39 implement post-F37 PC controlled-demo investor readiness fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_readiness_fixes.ps1` -> `PC controlled-demo investor readiness fixes check OK`; `F40 refresh PC controlled-demo investor-readiness evidence after fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F41 audit post-F40 PC controlled-demo investor evidence package` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_package.ps1` -> `PC controlled-demo investor evidence package audit OK`; `F42 implement post-F41 PC controlled-demo investor evidence package fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_package_fixes.ps1` -> `PC controlled-demo investor evidence package fixes check OK`; `F43 refresh PC controlled-demo investor evidence package after fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_refresh.ps1` -> `PC controlled-demo investor evidence refresh check OK`; `F44 audit post-F43 PC controlled-demo investor evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_refresh.ps1` -> `PC controlled-demo investor evidence refresh audit OK`; `F45 implement post-F44 PC controlled-demo investor evidence polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_polish_fixes.ps1` -> `PC controlled-demo investor evidence polish fixes check OK`; `F46 refresh PC controlled-demo investor route evidence after polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_refresh.ps1` -> `PC controlled-demo investor route evidence refresh check OK`; `F47 audit post-F46 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_refresh.ps1` -> `PC controlled-demo investor route evidence refresh audit OK`; `F48 implement post-F47 PC controlled-demo investor route evidence audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fixes check OK`; `F49 refresh PC controlled-demo investor route evidence after audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh check OK`; `F50 audit post-F49 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit OK`; `F51 implement post-F50 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fixes check OK`; `F52 refresh PC controlled-demo investor route evidence after F50 audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh check OK`; `F53 audit post-F52 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit OK`; `F54 implement post-F53 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fixes check OK`; next task was `F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F54 implementation note: `F54 implement post-F53 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fixes check OK`; next task was `F55 refresh PC controlled-demo investor route evidence after F53 audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F55 implementation note: `F55 refresh PC controlled-demo investor route evidence after F53 audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh check OK`; next task was `F56 audit post-F55 PC controlled-demo investor route evidence refresh`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F56 implementation note: `F56 audit post-F55 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit OK`; next task was `F57 implement post-F56 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F57 implementation note: `F57 implement post-F56 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fixes check OK`; next task was `F58 refresh PC controlled-demo investor route evidence after F56 audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F58 implementation note: `F58 refresh PC controlled-demo investor route evidence after F56 audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh check OK`; next task was `F59 audit post-F58 PC controlled-demo investor route evidence refresh`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F59 implementation note: `F59 audit post-F58 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit OK`; next task was `F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F60 implementation note: `F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes check OK`; next task was `F61 refresh PC controlled-demo investor route evidence after F59 audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F61 implementation note: `F61 refresh PC controlled-demo investor route evidence after F59 audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh check OK`; next task was `F62 audit post-F61 PC controlled-demo investor route evidence refresh`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F62 implementation note: `F62 audit post-F61 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit OK`; next task was `F63 implement post-F62 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F63 implementation note: `F63 implement post-F62 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes check OK`; next task was `F64 refresh PC controlled-demo investor route evidence after F62 audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F64 implementation note: `F64 refresh PC controlled-demo investor route evidence after F62 audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh check OK`; formal next task: `F65 audit post-F64 PC controlled-demo investor route evidence refresh`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.



F65 implementation note: `F65 audit post-F64 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit OK`; formal next task: `F66 implement post-F65 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F66 implementation note: `F66 implement post-F65 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes check OK`; formal next task: `F67 refresh PC controlled-demo investor route evidence after F65 audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F67 implementation note: `F67 refresh PC controlled-demo investor route evidence after F65 audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh check OK`; formal next task: `F68 audit post-F67 PC controlled-demo investor route evidence refresh`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F68 implementation note: `F68 audit post-F67 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit OK`; formal next task: `F69 implement post-F68 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F69 implementation note: `F69 implement post-F68 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes check OK`; formal next task: `F70 refresh PC controlled-demo investor route evidence after F68 audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.
