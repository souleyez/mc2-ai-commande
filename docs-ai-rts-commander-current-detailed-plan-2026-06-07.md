# AI RTS Commander Current Detailed Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use executing-plans to implement this plan task-by-task.

**Goal:** 把当前 Unity 6 原型推进到 PC 可展示、移动端可迁移的一版 AI 副官机甲战术 RTS Demo：Windows 继续作为开发验证和投资演示环境；Android/iOS 可行性、触控 UI 和移动端性能仍是产品优先级，但 G3 真机验证等待设备期间先执行 PC 端优化。

**Architecture:** `BattleCore` 继续做确定性规则核心，负责移动、喷射、占位、武器、热量、装甲、部位损伤、任务触发、结算和 AI observation/directive。Unity 6 只负责固定镜头、输入、稀疏 HUD、MechLab、模型材质、特效、截图和 smoke 证据。开发期可以使用本机私有参考素材验证比例和节奏，但公开版本必须切换到项目自有或合规授权内容包。

**Tech Stack:** Unity 6, C#, Windows PC build/smoke/capture as active demo loop, Android/iOS mobile-first after device blocker clears, deterministic BattleCore, PowerShell build/smoke/capture scripts, replaceable content packs, optional high-level AI deputy, later main server/map server/Web ranking contracts.

**Revision:** 2026-06-12 v35. This file is the fine-grained execution plan paired with `docs-ai-rts-commander-current-master-plan-2026-06-07.md`. The private reference visual bridge, local investor evidence package, art-safe metadata contract, AI deputy offline guard, reward authority contract, machine handoff plan, mobile-first priority reset, Android build smoke, PC optimization resumption, PC1 baseline audit, PC2 battle readability pass, PC3 MechLab PC flow polish, PC4 controlled demo evidence package, PC5 Windows demo launcher preflight, PC6 controlled demo evidence health check, PC7 controlled demo public boundary preflight, PC8 controlled demo readiness preflight, PC9 controlled demo handoff consistency check, PC10 Android device-smoke preflight, PC11 PC core playable contract check, PC12 mobile command model preflight, PC13 current plan gate check, PC14 Android smoke log crash scan, PC15 Android smoke plan mode, PC16 battle HUD sparse contract check, PC17 demo source hygiene check, PC18 AI deputy contract check, PC19 Windows demo build freshness check, PC20 controlled demo evidence freshness check, PC21 controlled demo capture log freshness check, and PC22 Android APK freshness check are now sealed for the current Demo. H2 validator/build/smoke is green; G2 Android build smoke is green with a generated APK; `G3 Run Android Device Smoke` is waiting on a physical authorized phone. The current PC optimization batch is complete unless a new PC23 target is explicitly defined.

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
2. `G3` 仍是当前移动端 gate：在真 Android 设备上安装并启动 APK，证明移动端运行链路可用。
3. 当前机器没有授权 Android 设备；G3 进入 Waiting on Device 状态，不推进 G4/G5。
4. Android 设备等待期间，PC1 基线审计、PC2 战场可读性优化、PC3 MechLab 打磨、PC4 受控演示证据包、PC5 Windows 启动预检、PC6 证据健康检查、PC7 公开边界预检、PC8 演示总预检、PC9 交接一致性检查、PC10 Android 真机 smoke 前置检查、PC11 PC 核心玩法合约检查、PC12 移动指挥模型预检、PC13 当前计划 gate 总预检、PC14 Android smoke 日志崩溃扫描、PC15 Android smoke 预演模式、PC16 战斗 HUD 稀疏合约检查、PC17 源码/生成物卫生检查、PC18 AI 副官边界检查、PC19 Windows 演示构建新鲜度检查、PC20 受控演示证据新鲜度检查、PC21 capture 日志新鲜度检查和 PC22 Android APK 新鲜度检查已通过；下一步回到 `G3` 真机 smoke，或先定义新的 PC23 再继续 PC/等待态工作。
5. D1 只是 art-safe metadata 合同，不是可挂载 runtime pack；后续 D2 才能进入清权资产生产和 mountable pack。
6. 私有参考素材可以继续用于本机开发验证，但公开材料不能把它描述成最终产品内容。
7. `F2-F4` 平台化方向仍保留，但现在只在移动端可行性通过后继续。

当前工作区注意事项：

- 若当前 `git status` 只剩计划文档改动，先完成校验并提交；之后等待 G3 设备，或先写清新的 PC23 目标。
- D1 新增的是 metadata 示例文件；不允许把私有 OBJ/TGA/PNG/JSON、截图、log 或 Unity build 输出加入 git。
- G2-G5 先做移动端可行性；F2-F4 后移，不先写服务器实现。
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
| M11 | 平台契约 | In Progress | 奖励认证契约完成；地图包、排行、创作者边界待写 |
| M12 | 换机开发交接 | Done | H2 validator/build/smoke 已通过 |
| M13 | 移动端优先可行性 | Waiting on Device | Android APK build smoke 已通过；真机 smoke 等授权手机，之后再做触控 UI 和性能预算 |
| M14 | PC/移动等待态优化 | Done for current pass | PC1-PC22 passed: PC1 baseline, PC2 battle readability, PC3 MechLab polish, PC4 controlled demo evidence package, PC5 launch preflight, PC6 evidence health check, PC7 public boundary preflight, PC8 readiness preflight, PC9 handoff consistency check, PC10 Android device-smoke preflight, PC11 PC core playable contract check, PC12 mobile command model preflight, PC13 current plan gate check, PC14 Android smoke log crash scan, PC15 Android smoke plan mode, PC16 battle HUD sparse contract check, PC17 demo source hygiene check, PC18 AI deputy contract check, PC19 Windows demo build freshness check, PC20 controlled demo evidence freshness check, PC21 controlled demo capture log freshness check and PC22 Android APK freshness check |

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
| G4 | Later | `Adapt command UI for mobile touch` | 状态行、Jet、地图、系统和 MechLab 手机触控可用 | device smoke |
| G5 | Later | `Define mobile performance budget` | FPS、内存、包体、加载、热量/电量基线 | docs + device evidence |
| G6 | Later | `Document iOS feasibility gate` | macOS/Xcode/签名/Metal/真机要求 | docs |
| F2 | Later | `Document map authoring contract` | 开源地图编辑器和地图包最小契约，移动 gate 后恢复 | docs |
| F3 | Later | `Document web ranking contract` | Web 排行、战绩、地图页和隐私边界 | docs |
| F4 | Later | `Document creator economy boundary` | 皮肤、地图、分成、可选链上边界 | docs |

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
- `north-patrol` 的水域不再像一大片平蓝色或黑蓝色。
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
   - `north-patrol`.
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
Current master and detailed plans put H2 and the mobile-first gate before F2.
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
Current master and detailed plans move G2-G6 before F2-F4.
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
- Requires MechLab sidecar to report whole-block/pressure-card fitting, all mounted weapons active and `noToggle=yes`.
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

- The evidence checker requires the six standard capture logs: `mechlab`, `spawn`, `airfield`, `hangar-contact`, `damage-demo`, and `north-patrol`.
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

### G4: Adapt Command UI For Mobile Touch

**Status:** Later.

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

**Status:** Later.

**Goal:** 用第一轮真机数据决定后续视觉、材质、特效、单位规模和资源包约束。

**Files:**

- Create: `docs-mobile-performance-budget-2026-06-10.md`

**Metrics:**

- FPS.
- Memory.
- Package size.
- First-load time.
- Battle-load time.
- Short-session thermal/battery notes.

**Commit:** `Define mobile performance budget`

### G6: Document iOS Feasibility Gate

**Status:** Later.

**Goal:** 记录 iOS 所需 macOS、Xcode、签名、Metal、真机测试要求；不让 iOS 签名阻塞 Android 先行验证。

**Files:**

- Create: `docs-ios-feasibility-2026-06-10.md`

**Commit:** `Document iOS feasibility gate`

### F2: Document Map Authoring Contract

**Status:** Later, resume after H2 and the mobile-first gate.

**Goal:** 为未来开源地图编辑器和社区地图包定义最小可验证格式。

**Files:**

- Modify: `docs-platform-ecosystem-plan.md`
- Create if needed: `docs-map-authoring-contract-2026-06-07.md`

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
```

**Acceptance:**

- Community maps can be open and editable.
- Portable rewards remain certified by main server.
- Validator requirements are clear enough to implement later.

**Commit:** `Document map authoring contract`

### F3: Document Web Ranking Contract

**Goal:** 规划 Web 侧战绩、排行、地图页、队伍资料和公开展示边界。

**Files:**

- Modify: `docs-platform-ecosystem-plan.md`
- Create if needed: `docs-web-ranking-plan-2026-06-07.md`

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
```

**Acceptance:**

- Web plan supports ranking and investment story.
- It does not expose private account ids, API keys, unpublished inventory or anti-cheat internals.

**Commit:** `Document web ranking contract`

### F4: Document Creator Economy Boundary

**Goal:** 把地图贡献、皮肤、自定义、收入分配和可选链上实验放在正确阶段，避免早期绑死核心玩法。

**Files:**

- Modify: `docs-platform-ecosystem-plan.md`
- Create if needed: `docs-creator-economy-boundary-2026-06-07.md`

**Rules:**

- Centralized ledger first.
- Creator revenue accounting can exist before chain.
- Chain later只适合 proof of revenue share、creator pools、cosmetic ownership proof、commemorative items。
- Core combat、机甲数值、武器数值、维修、普通库存变化和正常战斗结果不放链上。

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Chain is optional and late.
- Core gameplay remains deterministic and locally testable.

**Commit:** `Document creator economy boundary`

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

Windows 本地 Demo 的画面、碰撞、稀疏 UI、MechLab、损伤故事、受控演示证据、启动预检、构建新鲜度检查、证据健康检查、证据新鲜度检查、capture 日志新鲜度检查、Android APK 新鲜度检查、公开边界预检、演示总预检、交接一致性检查、Android 真机 smoke 前置检查、PC 核心玩法合约检查、移动指挥模型预检、战斗 HUD 稀疏合约检查、源码/生成物卫生检查、AI 副官边界检查、当前计划 gate 总预检、Android smoke 日志崩溃扫描、Android smoke 预演模式、公开 art-safe 元数据合同、AI 副官离线边界和主服务器奖励权威契约已经收稳；代码已推到 GitHub，H2 validator/build/smoke 已过，G2 Android APK build smoke 已过；PC1-PC22 PC/移动等待态优化包已封口；G3 真机 smoke 等待授权 Android 手机，设备到位后继续 G3-G5，设备不到位时如继续 PC 端必须先定义新的 PC23 目标。
