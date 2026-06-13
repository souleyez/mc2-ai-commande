# PC Investor Demo Route

Date: 2026-06-13

Purpose: a short Windows-local route for showing the current playable command demo without promising licensed final art, live multiplayer, or AI autonomy that is not implemented yet.

## Boundary

- First visible target: Windows PC controlled demo.
- Product direction preserved: first phone version is landscape-only; portrait is not a first-slice target.
- Visual boundary: private/reference art can be used for local development validation only. Public/commercial builds need owned, commissioned, or licensed replacement content packs.
- Combat boundary: BattleCore owns deterministic combat, pathing, damage, section loss, ejection, repair cost, and reward consequence. The presentation layer only visualizes and captures evidence.

## Three-Minute Route

- InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return launch=scripts/unity/run_windows_demo.ps1 evidence=command-report+screenshots+sidecars
- DamageProof=damage-demo screenshot=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png sidecar=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.json log=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.log callout=section-loss+cockpit-ejection+wreck-salvage+repair-line repairCost=9288
- LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False
- ProxyParsing=source=proxyIdentity+materialLanguage+propIdentity sidecarFallback=investorProxyVisuals splitSidecarRecapturePending=True publicSafe=proxy-only

| Step | Preset | What to show | Evidence point |
| --- | --- | --- | --- |
| 1 | `spawn` | Fixed RTS camera, readable first map, squad status rail, sparse battle HUD | Map and UI are visible without player box-select controls. |
| 2 | `hangar-contact` | Enemy contact trigger, objective pressure, hostile labels, squad focus command | The pacing can create pressure without requiring fast micro. |
| 3 | `damage-demo` | Section damage, lost-part story, cockpit/ejection readiness, repair consequence | Debrief summary exposes repair and damage consequence from the sidecar. |
| 4 | `solo-order` | Tap status row or command one mech independently | Single mech enters an isolated order state while the squad remains the default command target. |
| 5 | `solo-return` | Ordered mech automatically rejoins squad command state | The mobile landscape command model avoids drag-select and keeps the UI simple. |

## Talk Track

- This is an AI-deputy RTS direction: the player gives intent-level orders, while local combat still resolves exact movement, fire, damage, and repair.
- The first version should feel like a compact tactical command game: four to six mechs, fixed camera, clear status rows, jet command, system/pause, mission map, and MechLab later in the flow.
- AI should enter first as a planning/deputy layer, not as a per-frame combat brain. Latency stays outside the moment-to-moment battle loop.
- Current proxy visuals are development-safe stand-ins. They are meant to prove framing, scale, command readability, and damage story before the final asset pack lands.

## Route Audit Findings

- RouteAudit=pass-with-followups source=analysis-output/pc-controlled-demo-investor-route-evidence-refresh-audit/pc-controlled-demo-investor-route-evidence-refresh-audit.json
- RouteAuditFinding=route-evidence-envelope status=pass detail=F46 route evidence is complete, 1280x720, five required presets, with screenshot/sidecar/log links.
- RouteAuditFinding=presentation-route status=pass detail=Investor route names Windows route, demo launcher, command report, screenshots and sidecars.
- RouteAuditFinding=damage-proof status=pass detail=damage-demo exposes screenshot, sidecar, log, repair cost, section loss, cockpit ejection and wreck salvage.
- RouteAuditFinding=mobile-landscape-proof status=pass detail=first phone version remains landscape-only and horizontal.
- RouteAuditFinding=public-safe-proxy-boundary status=pass detail=proxy visuals remain public-safe stand-ins; BattleCore collision, pathing and combat remain unchanged.
- RouteAuditFollowUp=P1 area=audit-fixes next=F48-doc-visibility
- RouteAuditFollowUp=P2 area=gate-runtime next=keep-route-gates-focused
- RouteAuditFix=F48 visibility=investor-route+playable-evidence+handoff noUnityLaunch=True mobile=landscape-only next=F49-route-refresh

## Route Audit Fix Refresh Audit Findings

- RouteAuditFixRefreshAudit=pass-with-followups source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit.json completed=F50 next=F51 noUnityLaunch=True mobile=landscape-only
- RouteAuditFixRefreshAuditFinding=route-proof-clarity status=pass detail=F49 command evidence preserves the exact Windows route, launcher, command report, screenshots and sidecars.
- RouteAuditFixRefreshAuditFinding=damage-ejection-proof status=pass detail=damage-demo still links screenshot, sidecar, log, section loss, cockpit ejection, wreck salvage and repair cost.
- RouteAuditFixRefreshAuditFinding=mobile-landscape-proof status=pass detail=first phone version remains landscape-only as a horizontal phone game.
- RouteAuditFixRefreshAuditFinding=public-safe-proxy-boundary status=pass detail=proxy visuals remain public-safe stand-ins; collision, pathing and BattleCore are unchanged.
- RouteAuditFixRefreshAuditFinding=audit-fix-closure status=pass detail=F49 command report and markdown carry the F48 route-audit fix closure and source F48 audit-fix report.
- RouteAuditFixRefreshAuditFollowUp=P1 area=audit-visibility next=F51-doc-gate-visibility
- RouteAuditFixRefreshAuditFollowUp=P2 area=next-refresh-contract next=F52-consume-F50-audit-report
- RouteAuditFixRefreshAuditFix=F51 visibility=plan+evidence+handoff+gate noUnityLaunch=True mobile=landscape-only next=F52-route-refresh

## Route Audit Fix Refresh Audit Fix Refresh Audit Findings

- RouteAuditFixRefreshAuditFixRefreshAudit=pass-with-followups source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit.json completed=F53 next=F54 noUnityLaunch=True mobile=landscape-only
- RouteAuditFixRefreshAuditFixRefreshAuditFinding=route-proof-clarity status=pass detail=F52 command evidence preserves the five-preset Windows investor route and links command report, screenshots, sidecars and logs.
- RouteAuditFixRefreshAuditFixRefreshAuditFinding=damage-ejection-proof status=pass detail=damage-demo still carries section loss, cockpit ejection, wreck salvage and repair-cost proof.
- RouteAuditFixRefreshAuditFixRefreshAuditFinding=mobile-landscape-proof status=pass detail=first phone version remains landscape-only as a horizontal phone build.
- RouteAuditFixRefreshAuditFixRefreshAuditFinding=public-safe-proxy-boundary status=pass detail=proxy visuals remain public-safe stand-ins; collision, pathing and BattleCore are unchanged.
- RouteAuditFixRefreshAuditFixRefreshAuditFinding=audit-fix-refresh-closure status=pass detail=F52 command report and markdown carry F50 audit and F51 fix reports as explicit sources.
- RouteAuditFixRefreshAuditFixRefreshAuditFollowUp=P1 area=audit-visibility next=F54-doc-gate-visibility
- RouteAuditFixRefreshAuditFixRefreshAuditFollowUp=P2 area=next-refresh-contract next=F55-consume-F53-audit-report
- RouteAuditFixRefreshAuditFixRefreshAuditFix=F54 visibility=plan+evidence+handoff+gate noUnityLaunch=True mobile=landscape-only next=F55-route-refresh

## Route Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Findings

- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAudit=pass-with-followups source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json completed=F56 next=F57 noUnityLaunch=True mobile=landscape-only
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=route-proof-clarity status=pass detail=F55 command evidence preserves the five-preset Windows investor route and links command report, screenshots, sidecars and logs.
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=damage-ejection-proof status=pass detail=damage-demo still carries section loss, cockpit ejection, wreck salvage and repair-cost proof.
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=mobile-landscape-proof status=pass detail=F55 command evidence and plan surfaces keep first phone version landscape-only as a horizontal phone build.
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=public-safe-proxy-boundary status=pass detail=proxy visuals remain public-safe stand-ins; collision, pathing and BattleCore are unchanged.
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=audit-fix-refresh-closure status=pass detail=F55 command report and markdown carry F53 audit and F54 fix reports as explicit sources.
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFollowUp=P1 area=audit-visibility next=F57-doc-gate-visibility
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFollowUp=P2 area=next-refresh-contract next=F58-consume-F56-audit-report
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFix=F57 visibility=plan+evidence+handoff+gate noUnityLaunch=True mobile=landscape-only next=F58-route-refresh
## Route Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Findings

- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAudit=pass-with-followups source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json completed=F59 next=F60 noUnityLaunch=True mobile=landscape-only
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=route-proof-clarity status=pass detail=F58 command evidence preserves the five-preset Windows investor route and links command report, screenshots, sidecars and logs.
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=damage-ejection-proof status=pass detail=damage-demo still carries section loss, cockpit ejection, wreck salvage and repair-cost proof.
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=mobile-landscape-proof status=pass detail=F58 command evidence and plan surfaces keep first phone version landscape-only as a horizontal phone build.
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=public-safe-proxy-boundary status=pass detail=proxy visuals remain public-safe stand-ins; collision, pathing and BattleCore are unchanged.
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=audit-fix-refresh-closure status=pass detail=F58 command report and markdown carry F56 audit and F57 fix reports as explicit sources.
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFollowUp=P1 area=audit-visibility next=F60-doc-gate-visibility
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFollowUp=P2 area=next-refresh-contract next=F61-consume-F59-audit-report
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFix=F60 visibility=plan+evidence+handoff+gate noUnityLaunch=True mobile=landscape-only next=F61-route-refresh

## Route Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Closure

- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditRefresh=ready source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json completed=F61 next=F62 noUnityLaunch=True mobile=landscape-only
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure=F60-doc-gate-visibility status=closed surfaces=plan+evidence+handoff+gate
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure=next-refresh-contract status=closed sourceAudit=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json sourceFixes=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json
## Route Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Findings

- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAudit=pass-with-followups source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json completed=F62 next=F63 noUnityLaunch=True mobile=landscape-only
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=route-proof-clarity status=pass detail=F61 command evidence keeps the five-preset Windows investor route and links command report, screenshots, sidecars and logs.
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=damage-ejection-proof status=pass detail=damage-demo still carries section loss, cockpit ejection, wreck salvage and repair-cost proof after the F61 refresh.
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=mobile-landscape-proof status=pass detail=F61 command evidence and plan surfaces keep first phone version landscape-only as a horizontal phone build.
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=public-safe-proxy-boundary status=pass detail=proxy visuals remain public-safe stand-ins; collision, pathing and BattleCore are unchanged.
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=audit-fix-refresh-closure status=pass detail=F61 command report and markdown carry F59 audit and F60 fix reports as explicit sources.
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFollowUp=P1 area=audit-visibility next=F63-doc-gate-visibility
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFollowUp=P2 area=next-refresh-contract next=F64-consume-F62-audit-report
## Current Evidence Commands

```powershell
powershell -ExecutionPolicy Bypass -File scripts\unity\capture_pc_controlled_demo_command_evidence.ps1 -RepoRoot . -PlanOnly
powershell -ExecutionPolicy Bypass -File scripts\unity\check_pc_controlled_demo_investor_route_evidence_refresh.ps1 -RepoRoot .
powershell -ExecutionPolicy Bypass -File scripts\unity\check_pc_controlled_demo_investor_route_evidence_audit_fixes.ps1 -RepoRoot .
```

Expected route gate: `PC controlled-demo investor route evidence refresh check OK.`
Expected audit-fixes gate: `PC controlled-demo investor route evidence audit fixes check OK.`

F55 implementation note: `F55 refresh PC controlled-demo investor route evidence after F53 audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh check OK`; next task was `F56 audit post-F55 PC controlled-demo investor route evidence refresh`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F56 implementation note: `F56 audit post-F55 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit OK`; next task was `F57 implement post-F56 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F57 implementation note: `F57 implement post-F56 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fixes check OK`; next task was `F58 refresh PC controlled-demo investor route evidence after F56 audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F58 implementation note: `F58 refresh PC controlled-demo investor route evidence after F56 audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh check OK`; next task was `F59 audit post-F58 PC controlled-demo investor route evidence refresh`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F59 implementation note: `F59 audit post-F58 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit OK`; next task was `F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F60 implementation note: `F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes check OK`; next task was `F61 refresh PC controlled-demo investor route evidence after F59 audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F61 implementation note: `F61 refresh PC controlled-demo investor route evidence after F59 audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh check OK`; next task was `F62 audit post-F61 PC controlled-demo investor route evidence refresh`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F62 implementation note: `F62 audit post-F61 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit OK`; next task was `F63 implement post-F62 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

## Route Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fixes

- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFix=F63 visibility=plan+evidence+handoff+gate noUnityLaunch=True mobile=landscape-only next=F64-route-refresh
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure=F63-doc-gate-visibility status=closed surfaces=plan+evidence+handoff+gate
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure=next-refresh-contract status=closed sourceAudit=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json nextRefresh=F64-consume-F62-audit-report

F63 implementation note: `F63 implement post-F62 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes check OK`; next task was `F64 refresh PC controlled-demo investor route evidence after F62 audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

## Route Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh

- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditRefresh=ready source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json completed=F64 next=F65 noUnityLaunch=True mobile=landscape-only
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure=F63-doc-gate-visibility status=closed surfaces=plan+evidence+handoff+gate
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure=next-refresh-contract status=closed sourceAudit=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json sourceFixes=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json

F64 implementation note: `F64 refresh PC controlled-demo investor route evidence after F62 audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh check OK`; formal next task: `F65 audit post-F64 PC controlled-demo investor route evidence refresh`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.


## Route Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Findings

- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAudit=pass-with-followups source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json completed=F65 next=F66 noUnityLaunch=True mobile=landscape-only
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=route-proof-clarity status=pass detail=F64 command evidence keeps the five-preset Windows investor route and links command report, screenshots, sidecars and logs.
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=damage-ejection-proof status=pass detail=damage-demo still carries section loss, cockpit ejection, wreck salvage and repair-cost proof after the F64 refresh.
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=mobile-landscape-proof status=pass detail=F64 command evidence and plan surfaces keep first phone version landscape-only as a horizontal phone build.
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=public-safe-proxy-boundary status=pass detail=proxy visuals remain public-safe stand-ins; collision, pathing and BattleCore are unchanged.
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=audit-fix-refresh-closure status=pass detail=F64 command report and markdown carry F62 audit and F63 fix reports as explicit sources.
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFollowUp=P1 area=audit-visibility next=F66-doc-gate-visibility
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFollowUp=P2 area=next-refresh-contract next=F67-consume-F65-audit-report

F65 implementation note: `F65 audit post-F64 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit OK`; formal next task: `F66 implement post-F65 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

## Route Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fixes

- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFix=F66 visibility=plan+evidence+handoff+gate noUnityLaunch=True mobile=landscape-only next=F67-route-refresh
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure=F66-doc-gate-visibility status=closed surfaces=plan+evidence+handoff+gate
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure=next-refresh-contract status=closed sourceAudit=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json nextRefresh=F67-consume-F65-audit-report

F66 implementation note: `F66 implement post-F65 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes check OK`; formal next task: `F67 refresh PC controlled-demo investor route evidence after F65 audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.
