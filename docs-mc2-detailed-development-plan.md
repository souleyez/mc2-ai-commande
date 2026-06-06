# MC2 Current Detailed Development Plan

> **For Codex:** This document is retained as product and architecture background. Use `docs-playable-demo-overall-detailed-plan-2026-06-07.md` for the current overall roadmap, and execute the day-to-day queue in `docs-playable-demo-current-execution-plan-2026-06-07.md`.

**Goal:** 做出一版 Windows 可玩的战术机甲指挥 Demo：玩家能进入任务、指挥 1-6 台机甲完成一张参考关卡、看到可读的 3D 地形/建筑/机甲/爆炸损伤效果，并能回到机甲装配界面调整小队。AI 指挥官、地图服务器、公开换皮内容包都保留接口，但不阻塞第一版可见体验。

**Architecture:** `BattleCore` 负责确定性战斗规则、任务状态、单位命令、伤害、物理占位和结算；Unity 只负责表现、输入、镜头、UI、素材绑定和本地启动参数。原版素材只作为本地私有参考包用于验证画面和节奏，公开发布必须通过可替换内容包切换到自有素材。

**Tech Stack:** Unity 6, C#, Windows Standalone, PowerShell validation scripts, `mc2-unity-demo-contract-v1`, private local reference content pack, Git/GitHub.

---

## 1. 当前阶段

日期：2026-06-07。

当前更细的执行型计划书：

- `docs-playable-demo-overall-detailed-plan-2026-06-07.md`：当前整体计划入口，梳理产品范围、阶段门、架构边界、验收总线、长期平台延后项。
- `docs-playable-demo-current-execution-plan-2026-06-07.md`：当前唯一日常执行入口。后续按 `Current Commit Queue` 从第一个 `Next` 或 `In Progress` 任务继续。
- `docs-reference-visual-audit-2026-06-07.md`：截图、sidecar、validator、smoke 证据记录。视觉或战斗表现任务完成后更新这里。
- `docs-content-replacement-plan.md` 与 `docs-content-pack.md`：私有参考内容包和公开替换包边界。
- `docs-ai-commander-directive-contract.md`：AI 副官 observation/directive 合同。
- 其他 `docs-playable-demo-*` 旧文档保留历史上下文，不再作为下一步执行入口。

当前执行状态：

- 第一张 `mc2_01` 地图已经能本地构建、启动、smoke、截图，地形、水域、道路、建筑基底、模型比例和固定镜头构图都有证据。
- BattleCore occupancy 已覆盖单位、目标建筑、硬 terrain object、水域/地图边界 landing predicate 和 destination fallback；Unity 可显示碰撞占位，但规则仍由 BattleCore 决定。
- 默认全队命令、状态栏单机独立命令、独立完成后自动归队、喷射合法落点和可见闭环都已有 smoke 或 validator 覆盖。
- Combat Feel 当前锁定到可继续回归的程度：武器类型方向 cue、断臂/腿瘫/驾驶舱弹射 cue、装甲硬度规则已经完成。
- MechLab 已完成 mounted weapon 语义审计：装上的武器就是启用的武器，不再做玩家可见武器开关。
- Stage 4 / B2 已完成：MechLab 格子显示已经有整块武器外框、块内分格线和单格 filler 语言的 smoke 证据。
- Stage 4 / B3 已完成：装配预览通过 BattleCore 的 `UnitLoadoutCombatOverrideBuilder` 进入 UnitState，validator 已证明武器、散热器、装甲硬度和重量影响 battle-ready stats。
- Stage 5 / C1 已完成：战报普通玩家动作收成 Repair & Mech Lab、Next Contract、Retry Battle、Close，debrief smoke 不再暴露保存槽、账号、继续、End Run 或 Restart Mission 文案。
- Stage 5 / C2 已完成：validator 已证明 destroyed 机甲阻断 relaunch，立即维修精确扣代币，恢复 100% 可部署 roster，并保留 loadout identity 构建新任务。
- 当前下一步是 Stage 6 / D1：冻结 AI 副官 observation 合同，确保模型只做高层建议，不进入逐帧战斗细节。
- 后续短线顺序是：D1-D3 AI 副官能力窗口，E1-E5 公开边界和演示交付。
- BattleCore 仍是权威规则层。任何影响移动、伤害、胜负、维修、奖励或 AI 决策的内容，必须先有 BattleCore/contract 证据，再做 Unity 表现。

当前项目不是从零开始，已经进入“可见 Demo 打磨”阶段。核心问题已经从“能不能跑”转为“看起来是否像一款可信的战术机甲游戏”。

已经完成的基础：

- Unity 6 Windows Demo 可以构建、启动、跑 smoke test。
- `mc2_01` 参考任务合同可以加载单位、地形采样、目标、静态建筑、地图物件、导航点和脚本触发。
- 战斗核心支持小队移动、攻击、跳跃/喷射、敌方触发、目标完成、单位损伤、驾驶舱逃生和残骸提示。
- 默认全队指挥、单机甲独立命令、命令完成自动归队的基础逻辑已经建立。
- 指挥官机甲作为第一顺位，镜头跟随指挥官的方向已经确定。
- 机甲状态栏、战斗信息、任务目标、机甲装配界面和战后流程已有可运行版本。
- 武器装配规则已经向原版靠拢：武器装上就启用，不做启用/关闭开关。
- 本地私有参考素材桥已经能读到部分 OBJ/TGA/地形/道具，并生成可视化效果。
- 最近一轮已经加入战斗阵型偏移和 BattleCore 级单位物理占位，避免多个单位堆成一坨。
- 已加入 targetable structure 和大型 terrain object 的 BattleCore 占位，建筑、停机坪物件、quonset、portable building、barricade、sandbag 等硬物不再只是视觉装饰。
- 开火阶段右侧任务面板已收成 compact objective card，减少 `hangar-contact`、`damage-demo` 主战区被 UI 覆盖的问题。

当前主要不足：

- 战场画面仍然不够清楚，尤其是机甲、载具、建筑、地形比例和镜头距离还需要继续校准。
- 原版 3D 地形、树木、建筑、炮塔、机甲模型的还原还没有完全达到“截图可融资”的程度。
- 碰撞占位已经有了第一版，但还需要把敌方攻击环、目标周围停靠点、建筑/树木/硬物、水域非法落点和阵型 fallback 继续做稳。
- UI 可运行，但需要围绕“少操作、少文字、手机友好、战斗中少显示信息”继续收紧。
- 机甲装配界面还需要更像原版：整格摆放、清楚显示槽位形状、热量、载重、武器射程/冷却/伤害。
- AI 指挥官现在只应保留能力窗口和高层命令接口，不进入逐帧战斗决策。
- 公开内容替换、地图服务器、合作任务、经济、好友驾驶员和 Web 排行都属于后续路线，不应抢第一版 Demo 的精力。

## 2. 第一版可玩 Demo 定义

第一版只追求一条闭环：

1. 启动 Windows Demo。
2. 进入一张参考小地图任务。
3. 玩家默认指挥整支小队移动、攻击、喷射。
4. 玩家可以从状态栏点选单台机甲下达独立命令，完成后自动归队。
5. 地图上有可读的 3D 地形、建筑、树木/道具、敌人和我方机甲。
6. 敌方按任务触发出现，不需要强 AI。
7. 战斗有部位损伤、断臂/损毁提示、驾驶舱逃生、爆炸和残骸。
8. 任务可以胜利或失败，进入简洁战后结算。
9. 回到机甲装配界面，能看机甲、武器、装甲、热量、重量和槽位。
10. 可以重新进任务验证改装效果。

第一版明确不做：

- 实时玩家对战。
- 地图服务器。
- 真实充值、链上结算、可提现资产。
- 完整经济循环。
- 复杂存档系统。
- 大模型逐帧控制战斗。
- 公开发布原版素材、原版名称、原版剧情和原版商标。

成功标准：

- 本机启动后 1 分钟内能看到战斗场景。
- 截图能清楚分辨地形、机甲、建筑和战斗状态。
- 机甲不会明显堆叠在同一点。
- 玩家只用“点地点/目标、喷射、状态栏选机甲、暂停/系统”就能完成第一场任务。
- 机甲装配界面能让人一眼理解“哪些格子放了什么、为什么超重/过热、怎么改”。

## 3. 产品主线

### 3.1 战斗主线

战斗是第一优先级。玩家乐趣来自“像指挥老兵一样下达意图”，而不是频繁微操。

战斗规则：

- 小队规模 1-6 台机甲，常规 4 台。
- 默认选中全队。
- 点击地点：全队按阵型移动到附近合法位置。
- 点击敌方或目标：全队移动攻击或集中火力。
- 点状态栏选择某台机甲后，再点地点或目标：该机甲进入独立命令状态。
- 独立命令完成后自动归队，继续接受最新全队命令。
- 喷射是从每台机甲当前位置向目标方向快速位移一段固定距离。
- 喷射落点非法时，该机甲保持不动，其他有合法落点的机甲正常位移。
- 回避状态在第一版中等价于“独立命令”，不额外增加新按钮。
- 战斗中 UI 少显示信息，状态清楚比数据多更重要。

镜头规则：

- 固定俯视战术视角。
- 默认跟随指挥官机甲。
- 可以有限缩放。
- 不做自由旋转。
- 遮挡处理优先保证玩家能看到自己的队伍和目标。

### 3.2 机甲装配主线

装配是第二优先级，也是长期乐趣核心。

装配规则：

- 每台机甲有热量上限、载重上限、槽位上限。
- 槽位以原版类似的格子布局表达。
- 武器有重量、热量、伤害、射程、冷却时间和占用形状。
- 武器装上就启用，不做启用/关闭。
- 装甲板和散热器主要填充剩余单格槽位。
- 装甲计算保持单一简单：装甲板累加为一项整体硬度，先减免进入机体的伤害，再由驾驶舱、躯干、手臂、腿部等部位伤害系统体现结果；高伤害仍然可以打毁部位。
- 部分机甲有雷达和推进器额外槽位，第一版可先显示为固定组件。

装配 UI 目标：

- 像原版一样直观看格子，不要抽象成列表开关。
- 武器按整块占格显示，不能让玩家猜形状。
- 过热、超重、槽位冲突要即时提示。
- 当前机甲是否能出战要一眼看清。
- 尽量少文本，必要信息放在对的位置。

### 3.3 AI 指挥官主线

AI 指挥官暂不抢战斗主循环。第一版只做接口和能力窗口。

AI 负责：

- 观察当前任务摘要。
- 给出高层作战意图。
- 在托管模式下选择大方向命令。
- 给玩家一个“AI 副官建议”的预览能力。

AI 不负责：

- 每帧移动。
- 每发武器选择。
- 复杂路径规划。
- 高频实时微操。
- 直接改写 BattleCore 状态。

AI 接口原则：

- BattleCore 输出 compact observation。
- AI 返回 high-level directive。
- 本地规则把 directive 翻译成队伍命令。
- 模型延迟高时可以跳过，不影响本地战斗。

### 3.4 内容包主线

开发期可以用原版素材做私有参考验证，但项目结构必须允许整包替换。

内容包分层：

- `reference-private`：本地私有参考素材，只用于开发验证，不进入公开发行。
- `project-original`：未来自有机甲、武器、建筑、地形、UI、音效和文案。
- `community-map`：未来玩家或合作商地图包。
- `server-certified`：未来主服务器认证过、可带出奖励的地图包。

公开安全规则：

- 不公开原版模型、贴图、音频、任务文本、商标和专有名称。
- 不把原版素材打进 GitHub release。
- 不把原版专有文件名暴露为产品文案。
- 所有对原版的学习只沉淀为规则、比例、手感和数据结构。

## 4. 当前里程碑

### Milestone A: 战场可读性锁定

目标：让当前任务截图从“色块和一团模型”变成“能看懂的 3D 战术战场”。

任务 A1：建立视觉审计矩阵。

- 捕获 `spawn`、`airfield`、`hangar-contact`、`damage-demo`、`north-patrol` 等关键视角。
- 对每张图记录：单位是否可分辨、建筑比例是否正常、地形纹理是否清楚、敌我是否重叠、UI 是否挡视线。
- 输出审计记录到计划或分析文档。

任务 A2：校准机甲/载具/建筑比例。

- 检查 `ReferenceObjMeshLibrary`、`ReferencePropLibrary`、`DemoUnitView` 中的缩放因子。
- 目标是机甲比地面单位醒目，但不压过建筑。
- 载具、炮塔、树木和建筑需要有相对可信尺寸。

任务 A3：完善物理占位。

- 当前已经有单位间占位，下一步扩展到关键静态障碍。
- 至少覆盖：建筑、树木大物件、水域非法落点、任务目标附近停靠点。
- BattleCore 仍保持权威，Unity 物理只做表现和辅助。

任务 A4：调镜头和遮挡。

- 默认视角继续固定，不自由旋转。
- 队伍、目标建筑、当前接敌方向必须同时可读。
- 高建筑或树木遮挡我方时，优先做透明/淡化/轮廓处理。

任务 A5：优化敌方触发密度。

- 保留原版触发节奏，但避免所有敌人在同一屏幕堆成难读的一团。
- 可以通过阵型、停靠点、激活顺序和视角预设改善，不急着改任务逻辑。

验收：

- `damage-demo.png` 里至少能清楚分辨我方机甲、敌方、爆炸/残骸和主要建筑。
- 所有关键截图没有大面积粉色材质、黑块或模型堆叠。
- smoke test 仍能通过。

### Milestone B: 原型视觉还原

目标：本地私有 Demo 能接近原版的 3D 地形、机甲、建筑和环境氛围，用来判断战斗复刻是否成立。

任务 B1：参考模型清单审计。

- 确认可用机甲、载具、炮塔、建筑、树木和辅助节点。
- 记录哪些模型已经绑定，哪些仍是占位。
- 对损坏模型、断臂模型、驾驶舱逃生节点单独标记。

任务 B2：材质和贴图修复。

- 继续修 TGA/TXM/PAK 地形贴图映射。
- 减少糊成一坨的问题，优先让地面类型、道路、水域、建筑边界清楚。
- 粉色材质视为必须修复。

任务 B3：机甲朝向和姿态。

- 机甲应面向移动/攻击方向。
- 不要求第一版有完整步行动画，但姿态不能像随机摆放的模型。
- 断臂、损毁、驾驶舱逃生至少要有清楚的视觉事件。

任务 B4：环境道具。

- 恢复原版里通用的 NPC 建筑、炮塔、环境树木和任务目标道具。
- 道具应参与视觉遮挡和物理占位。
- 不需要第一版每个小物件都可交互。

任务 B5：素材包边界复查。

- 所有生成出来的原版参考输出保持 ignored 或本地路径。
- 公开项目只保留加载器、映射表结构和自有占位示例。

验收：

- 本地截图具备“像一张真实 3D 地图”的第一印象。
- 关键单位不再只是色块。
- 项目仍然不会把原版资源提交到 Git。

### Milestone C: 指挥游戏闭环

目标：玩家不看说明也能完成第一场战斗。

任务 C1：任务流程收紧。

- 保留一张小图和原版多层任务触发。
- 任务目标用简短文字显示。
- 胜利/失败条件清楚。
- 结束后进入战后结算。

任务 C2：战斗 UI 收紧。

- 左侧状态栏显示每台机甲：生命/部位损伤/是否独立命令/是否可行动。
- 主按钮只保留必要项：喷射、任务地图、暂停/系统。
- 战斗中不显示过多数值。
- 全队默认选中，避免框选。

任务 C3：命令反馈。

- 点击移动点后有目标点/阵型提示。
- 单机甲独立命令要在状态栏清楚标记。
- 独立命令完成后自动归队，状态恢复。

任务 C4：战斗结果。

- 显示任务完成、损伤、奖励、缴获摘要。
- 第一版奖励可以是简化资金和掉落提示。
- 不做复杂经济，不做等待维修，一键修复即可。

验收：

- 一次 smoke command 可以进入任务、下命令、完成/失败、打开 Debrief。
- 玩家可在 3-5 分钟内理解基本操作。

### Milestone D: 机甲装配垂直切片

目标：让装配界面成为第二个可演示亮点。

任务 D1：原版式格子装配。

- 每台机甲显示完整槽位形状。
- 武器以整块形状放入槽位。
- 冲突格、合法格、不可用格要清楚。

任务 D2：热量/重量/硬度。

- 热量、载重、槽位冲突即时计算。
- 装甲板提高整体硬度。
- 散热器降低热压力。
- 雷达、推进器作为固定额外组件先占位。

任务 D3：库存和常用配置。

- 普通武器可从商店/仓库模拟获得。
- 高级机甲和 ER 武器第一版先用开发配置，不进入商店经济。
- 配置能进入战斗验证。

任务 D4：装配 UI 手感。

- 支持点击选择、点击放置，后续再考虑拖拽。
- 移动端思路优先，Windows 也要顺手。
- 不做武器启用/关闭。

验收：

- 玩家能在一台机甲上换武器/装甲/散热器并看到合法性。
- 修改后的配置能进入战斗并影响武器表现或数值。

### Milestone E: AI 指挥官能力窗口

目标：证明 AI 可以做“副官/托管大决策”，但不影响本地战斗稳定性。

任务 E1：冻结 observation。

- 输出任务阶段、我方状态、敌方摘要、目标状态、可用命令。
- 保持小 JSON，避免把完整战斗状态塞给模型。

任务 E2：冻结 directive。

- AI 只返回意图：进攻、防守、重组、撤离、集火目标、保护单位等。
- 本地转换成具体 BattleCore 命令。

任务 E3：能力窗口 UI。

- 显示 AI 建议，但玩家可以忽略。
- 托管模式后续再启用。

任务 E4：延迟保护。

- 模型超时就继续本地默认逻辑。
- 不允许 AI 响应阻塞战斗帧。

验收：

- 可以从当前战斗状态生成一份 AI 输入。
- 可以把一条 AI 高层指令转换成普通游戏命令。

### Milestone F: 私有参考包到公开内容包

目标：为后续融资展示和公开发布留好版权边界。

任务 F1：资源分包。

- 私有参考资源保持本地路径或 ignored 输出。
- 自有资源包使用同一加载接口。
- 缺资源时能回退到自有占位，不崩溃。

任务 F2：名称和文案替换。

- 系统机甲名、武器名、任务名、UI 文案统一换成自有中文。
- 背景和任务另写，不搬原版剧情。

任务 F3：美术替换路线。

- 用原版比例和玩法验证需求。
- 逐步替换机甲轮廓、建筑风格、武器特效、地形纹理和 UI 视觉。

任务 F4：公开构建检查。

- 增加 public build 检查，防止打包进私有参考路径。

验收：

- 私有 Demo 能用于本地判断手感。
- 公开 Demo 不包含原版素材和专有表达。

### Milestone G: 长期平台和生态

目标：在核心战斗成立后，扩展为轻量跨端多人/地图生态。

后续方向：

- 合作商或玩家搭建地图服务器。
- 玩家用自己的机甲小队进入地图战斗并获取奖励。
- 主服务器认证奖励，防止地图服务器乱发跨服物品。
- Web 网站展示成绩、排名、地图数据和战报。
- 开源地图编辑器，允许自建地图。
- 皮肤自定义和地图自定义。
- 可选研究以太坊或链上分账，但必须等核心玩法和商业闭环清楚后再做。

当前处理：

- 只保留接口想象，不进入第一版开发。
- 不为链上、服务器和多人拖慢本地 Demo。

## 5. 未来 12 个具体执行任务

### Task 1: 完成战场视觉审计表

目的：先把“哪里糊、哪里堆、哪里不像原版”记录清楚。

涉及文件：

- `analysis-output/reference-visual-captures/*`（ignored 输出）
- `docs-reference-visual-restoration-plan.md`
- `docs-mc2-detailed-development-plan.md`

执行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
```

验收：

- 至少 5 张关键截图存在。
- 每张图有一句问题判断。
- 明确下一张最需要修的图。

建议提交：

```text
Document reference visual audit
```

### Task 2: 校准单位和建筑比例

目的：解决“机甲和道具堆在一起、比例读不出来”的问题。

涉及文件：

- `unity-mc2-demo/Assets/Scripts/MC2Demo/ReferenceObjMeshLibrary.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo/ReferencePropLibrary.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo/DemoUnitView.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo/Mc2DemoBootstrap.cs`

实现要点：

- 为 mech、vehicle、turret、building、tree 分开定义视觉缩放。
- 检查 parent placeholder scale 对导入模型的叠加影响。
- 保持 screenshot preset 稳定，便于前后对比。

验收：

- 机甲、载具、建筑不再像同一尺寸色块。
- damage-demo 截图中单位之间能看出前后层次。
- Unity validate/build/smoke 通过。

建议提交：

```text
Tune reference unit and prop scale
```

### Task 3: 扩展物理占位到静态障碍

目的：让建筑、树木、炮塔等地图物件也能影响落点和停靠。

涉及文件：

- `unity-mc2-demo/Assets/Scripts/MC2Demo.BattleCore/BattleMission.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo.BattleCore/UnitState.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo.BattleCore/MissionContract.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo/Editor/Mc2DemoValidator.cs`

实现要点：

- 给关键静态物件生成简化 collision radius 或 footprint。
- 小队目标点落在障碍中时，寻找附近合法位置。
- 喷射落点非法时保持原地。
- 保持 deterministic，不依赖 Unity runtime physics 做权威判定。

验收：

- 单位不会停进主建筑内部。
- 喷射不会落到水域或障碍中心。
- 新增 validator 覆盖至少一个障碍/水域场景。

建议提交：

```text
Add static obstacle occupancy
```

### Task 4: 固定镜头和遮挡处理

目的：在不自由旋转的前提下，让玩家永远能读到自己的队伍和目标。

涉及文件：

- `unity-mc2-demo/Assets/Scripts/MC2Demo/Mc2DemoBootstrap.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo/DemoUnitView.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo/DemoStructureView.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo/ReferencePropLibrary.cs`

实现要点：

- 镜头继续跟随指挥官。
- 大建筑/树木遮挡我方时淡化或降低遮挡强度。
- 保持有限缩放，不加入旋转。
- UI 不挤压主视角。

验收：

- hangar-contact 截图中我方单位可见。
- damage-demo 截图中战斗中心不被 UI 或建筑完全挡住。

建议提交：

```text
Improve fixed camera readability
```

### Task 5: 还原参考地形纹理

目的：让地图从“糊地面”变成能识别道路、水、坡地、建筑基底的 3D 地形。

涉及文件：

- `unity-mc2-demo/Assets/Scripts/MC2Demo/DemoTerrainView.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo/ReferenceTerrainTextureLibrary.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo/Mc2DemoBootstrap.cs`

实现要点：

- 优先修第一张小图用到的地形纹理。
- 道路、水边、建筑附近地面需要清楚。
- 保留生成材质输出在 ignored 目录。

验收：

- 远景能看出地形区域变化。
- 水域边界可读，便于解释喷射落点非法。

建议提交：

```text
Restore first mission terrain texture readability
```

### Task 6: 任务触发可读性整理

目的：保留原版触发节奏，同时让玩家看懂“为什么敌人来了、下一步做什么”。

涉及文件：

- `unity-mc2-demo/Assets/Scripts/MC2Demo.BattleCore/BattleMission.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo/Mc2DemoBootstrap.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo/Editor/Mc2DemoValidator.cs`

实现要点：

- 任务阶段词保持简短。
- 敌人触发后尽量在合理区域展开，不全堆同点。
- 目标状态变化要能在 UI 和战场上同时看见。

验收：

- smoke test 能断言关键阶段。
- 玩家能理解从初始、机场、机库到后续接敌的顺序。

建议提交：

```text
Clarify first mission encounter pacing
```

### Task 7: 机甲损伤表现增强

目的：保留原作最有味道的部位损伤、断手断脚、驾驶舱弹射。

涉及文件：

- `unity-mc2-demo/Assets/Scripts/MC2Demo.BattleCore/CombatDamageModel.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo/DemoUnitView.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo/DemoEffectsView.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo/ReferenceObjMeshLibrary.cs`

实现要点：

- 部位损伤在状态栏清楚显示。
- 断臂/损毁使用可见挂件、残片或短事件表现。
- 驾驶舱逃生有明确瞬间，不需要复杂动画。

验收：

- damage-demo 截图能看出一台单位发生严重损伤。
- 状态栏能对应到损伤部位。

建议提交：

```text
Improve section damage visual cues
```

### Task 8: 装配格子改成更接近原版

目的：把装配乐趣从表格配置推进到“看格子摆武器”。

涉及文件：

- `unity-mc2-demo/Assets/Scripts/MC2Demo/MechLabView.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo.BattleCore/MechLoadoutRules.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo.BattleCore/MechChassisCatalog.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo.BattleCore/WeaponCatalog.cs`

实现要点：

- 武器按占用形状绘制。
- 装甲板/散热器按单格绘制。
- 冲突、超重、过热即时提示。
- 删除任何“启用/关闭武器”的残留 UI。

验收：

- 一眼能看懂当前武器放在哪里。
- 超重/过热/槽位冲突不会被长文本淹没。

建议提交：

```text
Make mech lab grid closer to original fitting
```

### Task 9: 装配结果进入战斗

目的：让机甲装配不是假界面，而是真影响战斗。

涉及文件：

- `unity-mc2-demo/Assets/Scripts/MC2Demo.BattleCore/MechLoadoutRules.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo.BattleCore/BattleMission.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo/Mc2DemoBootstrap.cs`

实现要点：

- 当前装配决定武器射程、冷却、伤害、热量压力。
- 装甲硬度影响部位伤害。
- 不做复杂保存，只保留当前 Demo 流程内配置即可。

验收：

- 换武器后战斗行为可观察。
- 加装甲/散热器后状态或耐久表现可观察。

建议提交：

```text
Apply mech lab loadouts to battle stats
```

### Task 10: 战斗结算和一键修复收口

目的：完成一次任务后的最小闭环。

涉及文件：

- `unity-mc2-demo/Assets/Scripts/MC2Demo/DebriefView.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo/RepairView.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo.BattleCore/PostBattleReport.cs`

实现要点：

- 只用统一代币/资金。
- 机甲损毁就是维修扣钱，一键完成。
- 普通武器损毁后维修约等于重买。
- 不做等待时间。
- 不做复杂存档。

验收：

- 任务结束能看到损伤、奖励、修复按钮。
- 一键修复后能重新进任务。

建议提交：

```text
Tighten debrief and instant repair loop
```

### Task 11: AI 指挥接口冻结

目的：把 AI 放在正确层级，避免提前做复杂。

涉及文件：

- `docs-ai-commander-directive-contract.md`
- `unity-mc2-demo/Assets/Scripts/MC2Demo.BattleCore/AiCommanderObservation.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo.BattleCore/AiCommanderDirective.cs`
- `unity-mc2-demo/Assets/Scripts/MC2Demo/Mc2DemoBootstrap.cs`

实现要点：

- observation 小而稳定。
- directive 只表达意图。
- 本地规则负责执行。
- API 超时不影响游戏。

验收：

- 可以导出一份当前战斗 observation。
- 可以注入一份 directive 并转成普通命令。

建议提交：

```text
Freeze AI commander directive contract
```

### Task 12: 可演示构建整理

目的：给投资/合作演示准备一个稳定本地包。

涉及文件：

- `unity-mc2-demo/README.md`
- `README.md`
- `scripts/unity/*`
- `analysis-output/*`（ignored 输出）

实现要点：

- README 强调 AI 副官指挥 RTS 战斗探索，不提原版复刻作为公开卖点。
- 构建脚本、验证脚本、截图脚本保持可重复。
- 本地演示包标记 private reference build。

验收：

- 从 clean checkout 加本地参考包后能按文档跑起来。
- GitHub 上的公开说明不会宣传或分发原版资产。

建议提交：

```text
Prepare local demo handoff docs
```

## 6. 标准验证命令

Unity 合同验证：

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-current.log"
```

Windows 构建：

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-current.log"
```

本地 smoke：

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\smoke-command.json" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-smoke-current.log"
```

截图捕获：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
```

文档或小改检查：

```powershell
git diff --check
git status --short
```

验证原则：

- 改 BattleCore：必须跑 validator，最好加单项断言。
- 改 Unity 表现：必须至少跑 build 或 capture。
- 改战斗流程：必须跑 smoke。
- 只改文档：`git diff --check` 足够。

## 7. 提交节奏

每次只收一个清楚问题：

- 一次提交只做一个可解释目标。
- 每次提交说明“改了什么、如何验证、下一步是什么”。
- 不把 ignored 生成物、构建包、原版参考资产提交。
- 大量截图和日志只作为本地分析输出，不进 Git。

推荐推进顺序：

1. 先做 Task 1-4，把画面读清。
2. 再做 Task 5-7，把原型战斗感补起来。
3. 再做 Task 8-10，把装配和任务闭环收起来。
4. 最后做 Task 11-12，把 AI 能力窗口和演示交付整理好。

## 8. 决策记录

- 技术主线：Unity 6。
- 第一平台：Windows。
- 第一版不做实时 PVP。
- 第一版不做复杂存档。
- 第一版不做地图服务器。
- 第一版不做 AI 导演。
- 第一版 AI 只做高层指挥/副官建议接口。
- 战斗核心规则放在 BattleCore，Unity 不做权威判定。
- 物理占位优先在 BattleCore 做轻量确定性处理。
- 镜头固定，可有限缩放，不做自由旋转。
- 战斗 UI 尽量少信息，机甲状态栏承担主要反馈。
- 机甲装配尽量参考原版格子体验。
- 武器装上就启用。
- 装甲计算保持简单，部位损伤必须保留。
- 本地开发可用原版素材做私有参考。
- 公开发布必须整包替换为自有素材和自有文案。

## 9. 当前进度和细化执行计划

2026-06-07 更新：**Task 1** 已形成 `docs-reference-visual-audit-2026-06-07.md`，并完成第一轮 Task 2/Task 3 进展：BattleCore 已加入 targetable structure 占位、移动目标和喷射落点会避开结构核心，敌方攻击环改为 24 槽但保持在武器射程内。验证日志见 `analysis-output/unity-validate-structure-occupancy-r3.log`、`analysis-output/unity-build-structure-occupancy-r1.log`、`analysis-output/unity-player-structure-occupancy-smoke-r1.log`。

2026-06-07 追加：Task 4 已开始。开火阶段右侧任务面板改为 compact objective card，减少 `hangar-contact` 主战区遮挡；Task 3 继续补了大型 terrain object 占位，覆盖 airfield buildings、parked craft、quonsets、portable buildings、barricades、sandbags 等硬物。验证日志见 `analysis-output/unity-validate-ui-terrain-occupancy-r1.log`、`analysis-output/unity-build-ui-terrain-occupancy-r1.log`、`analysis-output/unity-player-ui-terrain-occupancy-smoke-r1.log`，截图见 `analysis-output/reference-visual-captures/hangar-contact.png` 和 `analysis-output/reference-visual-captures/damage-demo.png`。

当前阶段判断：

- 项目已经过了“能跑起来”的阶段，正在补“看得懂、能演示、能继续融资”的第一张地图战斗切片。
- 最紧急问题仍是可读性：模型比例、地形纹理、遮挡、敌我密度、UI 遮挡和特效层次。
- 不应马上扩平台、经济、地图服务器或 AI 大模型深度接入；这些路线已经在文档中保留，但第一版胜负手是战斗和装配。
- 近期开发以 1-2 天一个小提交为宜，每个提交只解决一个可截图验证的问题。

### Sprint 1: 战场可读性收口

目标：让 `hangar-contact` 和 `damage-demo` 截图从“堆在一起”提升到“能看懂这是一场机甲小队战斗”。

Commit 1：大型障碍占位和 compact 战斗 UI。

- 状态：已提交，提交 `af34a6a Add terrain object occupancy and detailed execution plan`。
- 修改文件：`BattleMission.cs`、`Mc2DemoBootstrap.cs`、`Mc2DemoValidator.cs`、本计划、视觉审计文档。
- 验证：validator、Windows build、smoke、`hangar-contact`/`damage-demo` capture。
- 验收：右侧任务面板不再盖住主战区，单位不会移动到大型建筑/硬物中心。

Commit 2：固定镜头遮挡处理。

- 状态：已提交，提交标题 `Add fixed-camera occlusion fading`。
- 当前实现：在表现层为 terrain objects、森林 footprint、树干、树冠增加基于固定相机和屏幕空间焦点的遮挡淡化；sidecar 记录 `OcclusionFade=active X/Y focus Z`。
- 验证：`analysis-output/unity-build-occlusion-fade-r4.log`、`analysis-output/unity-player-occlusion-fade-smoke-r5.log`、`analysis-output/reference-visual-captures/hangar-contact.json`、`analysis-output/reference-visual-captures/damage-demo.json`。
- 修改文件：`unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`、`DemoStructureView.cs`、`ReferencePropLibrary.cs`。
- 步骤 1：在 capture sidecar 中确认 `hangar-contact` 指挥官、目标建筑、接敌方向的屏幕位置。
- 步骤 2：找出遮挡我方/目标最严重的建筑和树木类型。
- 步骤 3：加入 camera-to-commander 遮挡检测，优先对遮挡物做半透明或淡化，不改变 BattleCore。
- 步骤 4：重新捕获 `hangar-contact`、`damage-demo`。
- 验证：Unity build + capture。
- 验收：我方 3-4 台机甲在主战斗截图里可见，目标建筑和敌方方向不被树木/建筑完全盖住。

Commit 3：地形对比和水域/道路可读性。

- 状态：已提交，提交 `89a686f Improve terrain and water readability`。根因确认是地形 mesh 顶面被 shader backface culling 剔除；修复后 `airfield`、`hangar-contact`、`damage-demo` 已能显示可读地面、水域、岸线、跑道/道路和建筑基底。
- 验证：`analysis-output/unity-build-terrain-readability-r7.log`、`analysis-output/unity-player-terrain-readability-smoke-r3.log`、`analysis-output/reference-visual-captures/airfield.png`、`analysis-output/reference-visual-captures/hangar-contact.png`、`analysis-output/reference-visual-captures/damage-demo.png`。
- 修改文件：`DemoTerrainView.cs`、`ReferenceTerrainTextureLibrary.cs`、`SourceTerrainVertexColor.shader`。
- 步骤 1：对比 `airfield`、`hangar-contact` 的地面区域，记录哪些地形读成一片糊色。
- 步骤 2：提高道路、跑道、水域、岸线、建筑基底的对比度。
- 步骤 3：保留原始纹理方向，但允许开发期做亮度/饱和度校正。
- 步骤 4：确认点击、移动和水域非法落点不受表现层影响。
- 验证：Unity build + smoke + capture。
- 验收：截图里能分清路、水、草地/泥地、建筑底座。

Commit 4：敌方密度和停靠点微调。

- 状态：已完成，提交标题 `Spread first mission enemy parking`。
- 修改文件：`unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`、`unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`、`docs-reference-visual-audit-2026-06-07.md`，必要时再碰 `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`。
- 步骤 1：用 sidecar 记录每个 preset 激活敌人和可见敌人的数量。
- 步骤 2：补强 validator，断言敌方攻击/停靠点有最小实用间距，且仍在武器有效范围内。
- 步骤 3：只调阵型展开、停靠半径、激活后的目标环，不改大任务逻辑，不删除敌方压力。
- 步骤 4：让 BattleCore 的单位、结构、terrain object 占位继续作为权威，Unity 只负责显示。
- 步骤 5：重新捕获 `hangar-contact`、`damage-demo`，视觉审计记录前后差异。
- 验证：`analysis-output/unity-validate-enemy-spacing-r4.log`、`analysis-output/unity-build-enemy-spacing-r2.log`、`analysis-output/unity-player-enemy-spacing-smoke-r2.log`、`analysis-output/reference-visual-captures/hangar-contact.png`、`analysis-output/reference-visual-captures/damage-demo.png`。
- 验收：密集战斗仍热闹，`hangar-contact` 仍为 20 active / 16 visible，`damage-demo` 仍为 20 active / 19 visible；步兵 ambush 不再全部停车到同一坐标，敌方 attack slots 有真实 combat-data validator 保护。

### Sprint 2: 原作感视觉要素补齐

目标：把第一张地图从“可读”推进到“有原型味道”：机甲轮廓、道具、炮塔、建筑、损伤事件都能被看出来。

Commit 5：机甲/载具/炮塔比例复查。

- 修改文件：`ReferenceObjMeshLibrary.cs`、`DemoUnitView.cs`、`ReferencePropLibrary.cs`。
- 步骤 1：列出当前任务实际出现的 mech、vehicle、turret、building asset id。
- 步骤 2：给不同类别设独立视觉比例表，不用一个全局 scale 硬撑。
- 步骤 3：确认 collider radius 与视觉尺寸不会严重打架。
- 验证：Unity build + capture。
- 验收：机甲、载具、炮塔、建筑有清楚尺寸层级。

Commit 6：武器开火和命中特效层次。

- 修改文件：`DemoEffectsView.cs`、`DemoUnitView.cs`、`WeaponCatalog.cs`。
- 步骤 1：按武器类型整理当前已有粒子/线束/爆炸表现。
- 步骤 2：优先补激光、导弹、炮弹三类基础区别。
- 步骤 3：让命中、未命中、结构受击、机甲部位受击有不同反馈。
- 验证：damage-demo capture + smoke。
- 验收：截图或短时观战中能分辨“谁在打谁、用什么打”。

Commit 7：断臂/瘸腿/驾驶舱弹射演示增强。

- 修改文件：`CombatDamageModel.cs`、`DemoUnitView.cs`、`DemoEffectsView.cs`、`ReferenceObjMeshLibrary.cs`。
- 步骤 1：固定一个 damage-demo 脚本触发左臂脱落、腿部瘫痪、驾驶舱逃生。
- 步骤 2：优先使用 reference node clone，没有节点时回退到清楚的自有占位。
- 步骤 3：状态栏部位损伤与场景事件对应起来。
- 验证：validator + damage-demo capture。
- 验收：不用读日志也能看出一台机甲发生严重部位损伤。

### Sprint 3: 指挥游戏闭环

目标：玩家能用极少操作打完第一张图。

Commit 8：战斗 UI 最小化冻结。

- 修改文件：`Mc2DemoBootstrap.cs`。
- 步骤 1：保留左侧机甲状态栏、喷射、任务地图、暂停/系统。
- 步骤 2：战斗中隐藏或压缩多余文字，只在状态栏给损伤/独立命令/可行动反馈。
- 步骤 3：给小屏和 1280x720 各检查一次布局。
- 验证：smoke + capture。
- 验收：主战场不被 UI 抢戏，状态栏足够表达当前队伍状态。

Commit 9：命令反馈打磨。

- 修改文件：`Mc2DemoBootstrap.cs`、`BattleMission.cs`。
- 步骤 1：全队移动显示阵型目标点。
- 步骤 2：单机甲独立命令在状态栏明确标记。
- 步骤 3：命令完成后自动归队，并接受最新全队命令。
- 验证：validator + smoke。
- 验收：不用框选也能稳定指挥 1-6 台机甲。

Commit 10：任务胜负和战后简报收口。

- 修改文件：`BattleMission.cs`、`DebriefView.cs`、`Mc2DemoBootstrap.cs`。
- 步骤 1：明确第一张图胜利/失败触发。
- 步骤 2：战后只显示损伤、奖励、缴获、维修入口。
- 步骤 3：一键修复，不做等待，不做复杂保存。
- 验证：smoke 覆盖胜利或失败路径。
- 验收：完成任务后可以自然回到下一轮改装/再战。

### Sprint 4: 机甲装配垂直切片

目标：装配界面能成为 Demo 的第二个卖点。

Commit 11：原版式格子装配显示。

- 修改文件：`MechLabView.cs`、`MechLoadoutRules.cs`、`MechChassisCatalog.cs`、`WeaponCatalog.cs`。
- 步骤 1：每台机甲显示槽位形状。
- 步骤 2：武器用整块格子形状显示，装上即启用。
- 步骤 3：装甲板、散热器作为单格组件显示。
- 步骤 4：冲突、过热、超重即时提示。
- 验证：编辑器 validator 或 UI smoke。
- 验收：玩家一眼能看懂“格子为什么放不下”。

Commit 12：装配结果进入 BattleCore。

- 修改文件：`MechLoadoutRules.cs`、`BattleMission.cs`、`CombatDamageModel.cs`。
- 步骤 1：武器射程、冷却、伤害来自当前 loadout。
- 步骤 2：热量和散热器先做简单战斗压力，不做复杂 UI。
- 步骤 3：装甲板提升整体硬度，再进入部位伤害计算。
- 验证：validator 增加 loadout 影响断言。
- 验收：换武器/装甲后战斗表现能观察到差异。

Commit 13：装配到任务的演示闭环。

- 修改文件：`Mc2DemoBootstrap.cs`、`MechLabView.cs`、`DebriefView.cs`。
- 步骤 1：从战后回到装配。
- 步骤 2：修改当前队伍配置。
- 步骤 3：重新进同一张任务。
- 步骤 4：不引入复杂保存，只保留当前运行内配置。
- 验证：smoke 或手动 capture。
- 验收：可以演示“改装 -> 出战 -> 受损/胜利 -> 再改装”。

### Sprint 5: AI 副官接口和演示包

目标：保留 AI 大方向，但不让模型延迟和不确定性拖累当前战斗。

Commit 14：AI observation/directive 合同冻结。

- 修改文件：`docs-ai-commander-directive-contract.md`、`AiCommanderObservation.cs`、`AiCommanderDirective.cs`。
- 步骤 1：observation 只包含任务阶段、我方状态、敌方摘要、目标状态、可用意图。
- 步骤 2：directive 只包含进攻、防守、重组、撤离、集火、保护等高层命令。
- 步骤 3：本地规则把 directive 转成普通命令。
- 验证：单元/validator 导出一份 observation 并回放一份 directive。
- 验收：AI 可以提供建议，但断网/超时不影响本地战斗。

Commit 15：本地演示包整理。

- 修改文件：`README.md`、`unity-mc2-demo/README.md`、`BUILD-WIN.md`、`scripts/unity/*`。
- 步骤 1：README 继续强调 AI 副官指挥 RTS 战斗探索，不公开宣传原版素材。
- 步骤 2：列出私有参考包、本地构建、截图验证步骤。
- 步骤 3：确认 GitHub release 不包含私有参考资产。
- 验证：clean-ish checkout + build/smoke。
- 验收：能交给别人本地复现，但公开仓库不碰版权雷区。

## 10. 暂缓事项

这些不是砍掉，而是等第一张图和装配闭环稳定后再做：

- 实时 PVP。
- 地图服务器和第三方地图奖励认证。
- Web 排名站。
- 链上分账或 NFT/皮肤资产。
- 完整经济、好友驾驶员、支援任务、活动碎片循环。
- AI 导演和复杂自然语言战术控制。
- 移动端适配和跨端包。

当前只给它们保留结构边界：BattleCore 确定性、内容包可替换、AI 接口高层化、奖励由主服务器认证。

## 11. 下一步

下一次继续开发时，按 `docs-playable-demo-locked-execution-plan-2026-06-07.md` 执行，从 **D1: Freeze AI Observation Contract** 开始。战场可读性、物理占位、固定镜头、武器 cue、部位损伤/弹射 cue、装甲硬度、mounted weapon 语义、MechLab grid block cue、loadout battle effects、简洁战报和维修再战闭环都已经收好，下一步重点是把 AI 副官能力边界固定住：

1. D1-D3 AI deputy：只做高层 observation/directive 和一个可选建议窗口，模型慢或无 key 不影响本地 Demo。
2. E1-E5 handoff：补公开内容边界、public build 安全检查、三分钟演示脚本、可重复 Windows 构建和证据包。

短期不要插入服务器、经济、PVP、移动端或链上功能。那些方向已经有长期边界，现在第一优先级仍是 Windows 本地可玩 Demo 的“装配 -> 出战 -> 指挥 -> 损伤 -> 战报 -> 维修再战”闭环。
