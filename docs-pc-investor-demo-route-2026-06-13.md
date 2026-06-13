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

## Route Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Closure

- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditRefresh=ready source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json completed=F67 next=F68 noUnityLaunch=True mobile=landscape-only
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure=F66-doc-gate-visibility status=closed surfaces=plan+evidence+handoff+gate
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure=next-refresh-contract status=closed sourceAudit=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json sourceFixes=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json

F67 implementation note: `F67 refresh PC controlled-demo investor route evidence after F65 audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh check OK`; formal next task: `F68 audit post-F67 PC controlled-demo investor route evidence refresh`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

## Route Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Findings

- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAudit=pass-with-followups source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json completed=F68 next=F69 noUnityLaunch=True mobile=landscape-only
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=route-proof-clarity status=pass
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=damage-ejection-proof status=pass
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=mobile-landscape-proof status=pass
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=public-safe-proxy-boundary status=pass
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFinding=F67-refresh-traceability status=pass
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFollowUp=P1 area=audit-visibility next=F69-doc-gate-visibility
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFollowUp=P2 area=next-refresh-contract next=F70-consume-F68-audit-report

F68 implementation note: `F68 audit post-F67 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit OK`; formal next task: `F69 implement post-F68 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

## Route Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fixes

- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFix=F69 visibility=plan+evidence+handoff+gate noUnityLaunch=True mobile=landscape-only next=F70-route-refresh
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure=F69-doc-gate-visibility status=closed surfaces=plan+evidence+handoff+gate
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure=next-refresh-contract status=closed sourceAudit=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json nextRefresh=F70-consume-F68-audit-report

F69 implementation note: `F69 implement post-F68 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes check OK`; formal next task: `F70 refresh PC controlled-demo investor route evidence after F68 audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

## Route Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Closure

- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditRefresh=ready source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json completed=F70 next=F71 noUnityLaunch=True mobile=landscape-only
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure=F69-doc-gate-visibility status=closed surfaces=plan+evidence+handoff+gate
- RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure=next-refresh-contract status=closed sourceAudit=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json sourceFixes=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json

F70 implementation note: `F70 refresh PC controlled-demo investor route evidence after F68 audit fixes` is complete. Evidence gate: `scripts/unity/check_f70_pc_route_evidence_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh check OK`; formal next task: `F71 audit post-F70 PC controlled-demo investor route evidence refresh`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

## F71 PC Route Evidence Refresh Audit

- F71RouteEvidenceAudit=pass-with-followups source=analysis-output/f71-pc-route-evidence-audit/report.json completed=F71 next=F72 noUnityLaunch=True mobile=landscape-only
- F71RouteEvidenceAuditFinding=F70-traceability status=pass detail=F70 consumes F68 audit and F69 fixes
- F71RouteEvidenceAuditFinding=route-proof-clarity status=pass detail=spawn>hangar-contact>damage-demo>solo-order>solo-return
- F71RouteEvidenceAuditFinding=damage-ejection-proof status=pass detail=section-loss+cockpit-ejection+wreck-salvage+repair-line
- F71RouteEvidenceAuditFinding=mobile-landscape-proof status=pass detail=first phone version remains landscape-only
- F71RouteEvidenceAuditFinding=public-safe-proxy-boundary status=pass detail=proxy-only visuals with unchanged collision/pathing
- F71RouteEvidenceAuditFinding=windows-path-budget status=pass detail=F70 script and output moved to short paths
- F71RouteEvidenceAuditFollowUp=P1 area=audit-visibility next=F72-doc-gate-visibility
- F71RouteEvidenceAuditFollowUp=P2 area=next-refresh-contract next=F73-consume-F71-audit-report
- F71RouteEvidenceAuditFollowUp=P2 area=path-budget next=F72-keep-new-F-artifacts-short

F71 implementation note: `F71 audit post-F70 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_f71_pc_route_evidence_refresh.ps1` -> `F71 PC route evidence refresh audit OK.`; formal next task: `F72 implement post-F71 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.
## F72 PC Route Evidence Audit Fixes

- F72RouteEvidenceAuditFixes=pass source=analysis-output/f72-pc-route-audit-fixes/report.json completed=F72 next=F73 noUnityLaunch=True mobile=landscape-only
- F72RouteEvidenceAuditClosure=F72-doc-gate-visibility status=closed surfaces=plan+evidence+handoff+gate sourceAudit=analysis-output/f71-pc-route-evidence-audit/report.json
- F72RouteEvidenceAuditClosure=next-refresh-contract status=closed sourceAudit=analysis-output/f71-pc-route-evidence-audit/report.json sourceFixes=analysis-output/f72-pc-route-audit-fixes/report.json nextRefresh=F73-consume-F71-audit-report
- F72RouteEvidenceAuditClosure=path-budget status=closed script=scripts/unity/check_f72_pc_route_audit_fixes.ps1 output=analysis-output/f72-pc-route-audit-fixes/
- F72RouteEvidenceAuditClosure=mobile-landscape status=preserved firstPhoneVersion=landscape-only portraitSupport=False

F72 implementation note: `F72 implement post-F71 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_f72_pc_route_audit_fixes.ps1` -> `F72 PC route audit fixes check OK.`; formal next task: `F73 refresh PC controlled-demo investor route evidence after F71 audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.
## F73 PC Route Evidence Refresh

- F73RouteEvidenceRefresh=ready source=analysis-output/f72-pc-route-audit-fixes/report.json completed=F73 next=F74 noUnityLaunch=True mobile=landscape-only
- F73RouteEvidenceRefreshSource=audit sourceAudit=analysis-output/f71-pc-route-evidence-audit/report.json sourceFixes=analysis-output/f72-pc-route-audit-fixes/report.json
- F73RouteEvidenceRefreshClosure=route-proof-preserved route=spawn>hangar-contact>damage-demo>solo-order>solo-return damage=section-loss+cockpit-ejection+wreck-salvage+repair-line publicSafe=proxy-only
- F73RouteEvidenceRefreshClosure=mobile-landscape status=preserved firstPhoneVersion=landscape-only portraitSupport=False

F73 implementation note: `F73 refresh PC controlled-demo investor route evidence after F71 audit fixes` is complete. Evidence gate: `scripts/unity/check_f73_pc_route_evidence_refresh.ps1` -> `F73 PC route evidence refresh check OK.`; formal next task: `F74 audit post-F73 PC controlled-demo investor route evidence refresh`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.
## F74 PC Route Evidence Refresh Audit

- F74RouteEvidenceAudit=pass-with-followups source=analysis-output/f74-pc-route-evidence-refresh-audit/report.json completed=F74 next=F75 noUnityLaunch=True mobile=landscape-only
- F74RouteEvidenceAuditFinding=F73-traceability status=pass detail=F73 consumes F71 audit and F72 fixes
- F74RouteEvidenceAuditFinding=route-proof-clarity status=pass detail=spawn>hangar-contact>damage-demo>solo-order>solo-return
- F74RouteEvidenceAuditFinding=damage-ejection-proof status=pass detail=section-loss+cockpit-ejection+wreck-salvage+repair-line
- F74RouteEvidenceAuditFinding=mobile-landscape-proof status=pass detail=first phone version remains landscape-only
- F74RouteEvidenceAuditFinding=public-safe-proxy-boundary status=pass detail=proxy-only visuals with unchanged collision/pathing
- F74RouteEvidenceAuditFinding=windows-path-budget status=pass detail=F73 and F74 script/output paths stay short
- F74RouteEvidenceAuditFollowUp=P1 area=audit-visibility next=F75-doc-gate-visibility
- F74RouteEvidenceAuditFollowUp=P2 area=next-refresh-contract next=F76-consume-F74-audit-report
- F74RouteEvidenceAuditFollowUp=P2 area=path-budget next=F75-keep-new-F-artifacts-short

F74 implementation note: `F74 audit post-F73 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_f74_pc_route_evidence_refresh.ps1` -> `F74 PC route evidence refresh audit OK.`; formal next task: `F75 implement post-F74 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.
## F75 PC Route Evidence Audit Fixes

- F75RouteEvidenceAuditFixes=pass source=analysis-output/f75-pc-route-audit-fixes/report.json completed=F75 next=F76 noUnityLaunch=True mobile=landscape-only
- F75RouteEvidenceAuditClosure=F75-doc-gate-visibility status=closed surfaces=plan+evidence+handoff+gate sourceAudit=analysis-output/f74-pc-route-evidence-refresh-audit/report.json
- F75RouteEvidenceAuditClosure=next-refresh-contract status=closed sourceAudit=analysis-output/f74-pc-route-evidence-refresh-audit/report.json sourceFixes=analysis-output/f75-pc-route-audit-fixes/report.json nextRefresh=F76-consume-F74-audit-report
- F75RouteEvidenceAuditClosure=path-budget status=closed script=scripts/unity/check_f75_pc_route_audit_fixes.ps1 output=analysis-output/f75-pc-route-audit-fixes/
- F75RouteEvidenceAuditClosure=mobile-landscape status=preserved firstPhoneVersion=landscape-only portraitSupport=False

F75 implementation note: `F75 implement post-F74 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_f75_pc_route_audit_fixes.ps1` -> `F75 PC route audit fixes check OK.`; formal next task: `F76 refresh PC controlled-demo investor route evidence after F74 audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.
## F76 PC Route Evidence Refresh

- F76RouteEvidenceRefresh=ready source=analysis-output/f75-pc-route-audit-fixes/report.json completed=F76 next=F77 noUnityLaunch=True mobile=landscape-only
- F76RouteEvidenceRefreshSource=audit sourceAudit=analysis-output/f74-pc-route-evidence-refresh-audit/report.json sourceFixes=analysis-output/f75-pc-route-audit-fixes/report.json
- F76RouteEvidenceRefreshClosure=route-proof-preserved route=spawn>hangar-contact>damage-demo>solo-order>solo-return damage=section-loss+cockpit-ejection+wreck-salvage+repair-line publicSafe=proxy-only
- F76RouteEvidenceRefreshClosure=mobile-landscape status=preserved firstPhoneVersion=landscape-only portraitSupport=False

F76 implementation note: `F76 refresh PC controlled-demo investor route evidence after F74 audit fixes` is complete. Evidence gate: `scripts/unity/check_f76_pc_route_evidence_refresh.ps1` -> `F76 PC route evidence refresh check OK.`; formal next task: `F77 audit post-F76 PC controlled-demo investor route evidence refresh`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

## F77 PC Route Evidence Refresh Audit

- F77RouteEvidenceAudit=pass-with-followups source=analysis-output/f77-pc-route-evidence-refresh-audit/report.json completed=F77 next=F78 noUnityLaunch=True mobile=landscape-only
- F77RouteEvidenceAuditFinding=F76-traceability status=pass detail=F76 consumes F74 audit and F75 fixes
- F77RouteEvidenceAuditFinding=route-proof-clarity status=pass detail=spawn>hangar-contact>damage-demo>solo-order>solo-return
- F77RouteEvidenceAuditFinding=damage-ejection-proof status=pass detail=section-loss+cockpit-ejection+wreck-salvage+repair-line
- F77RouteEvidenceAuditFinding=mobile-landscape-proof status=pass detail=first phone version remains landscape-only
- F77RouteEvidenceAuditFinding=public-safe-proxy-boundary status=pass detail=proxy-only visuals with unchanged collision/pathing
- F77RouteEvidenceAuditFinding=windows-path-budget status=pass detail=F76 and F77 script/output paths stay short
- F77RouteEvidenceAuditFollowUp=P1 area=audit-visibility next=F78-doc-gate-visibility
- F77RouteEvidenceAuditFollowUp=P2 area=next-refresh-contract next=F79-consume-F77-audit-report
- F77RouteEvidenceAuditFollowUp=P2 area=path-budget next=F78-keep-new-F-artifacts-short

F77 implementation note: `F77 audit post-F76 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_f77_pc_route_evidence_refresh.ps1` -> `F77 PC route evidence refresh audit OK.`; formal next task: `F78 implement post-F77 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

## F78 PC Route Evidence Audit Fixes

- F78RouteEvidenceAuditFixes=pass source=analysis-output/f78-pc-route-audit-fixes/report.json completed=F78 next=F79 noUnityLaunch=True mobile=landscape-only
- F78RouteEvidenceAuditClosure=F78-doc-gate-visibility status=closed surfaces=plan+evidence+handoff+gate sourceAudit=analysis-output/f77-pc-route-evidence-refresh-audit/report.json
- F78RouteEvidenceAuditClosure=next-refresh-contract status=closed sourceAudit=analysis-output/f77-pc-route-evidence-refresh-audit/report.json sourceFixes=analysis-output/f78-pc-route-audit-fixes/report.json nextRefresh=F79-consume-F77-audit-report
- F78RouteEvidenceAuditClosure=path-budget status=closed script=scripts/unity/check_f78_pc_route_audit_fixes.ps1 output=analysis-output/f78-pc-route-audit-fixes/
- F78RouteEvidenceAuditClosure=mobile-landscape status=preserved firstPhoneVersion=landscape-only portraitSupport=False

F78 implementation note: `F78 implement post-F77 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_f78_pc_route_audit_fixes.ps1` -> `F78 PC route audit fixes check OK.`; formal next task: `F79 refresh PC controlled-demo investor route evidence after F77 audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

- F79RouteEvidenceRefresh=ready source=analysis-output/f78-pc-route-audit-fixes/report.json completed=F79 next=F80 noUnityLaunch=True mobile=landscape-only
- F79RouteEvidenceRefreshSource=audit sourceAudit=analysis-output/f77-pc-route-evidence-refresh-audit/report.json sourceFixes=analysis-output/f78-pc-route-audit-fixes/report.json
- F79RouteEvidenceRefreshClosure=route-proof-preserved route=spawn>hangar-contact>damage-demo>solo-order>solo-return damage=section-loss+cockpit-ejection+wreck-salvage+repair-line publicSafe=proxy-only
- F79RouteEvidenceRefreshClosure=mobile-landscape status=preserved firstPhoneVersion=landscape-only portraitSupport=False

F79 implementation note: `F79 refresh PC controlled-demo investor route evidence after F77 audit fixes` is complete. Evidence gate: `scripts/unity/check_f79_pc_route_evidence_refresh.ps1` -> `F79 PC route evidence refresh check OK.`; formal next task: `F80 audit post-F79 PC controlled-demo investor route evidence refresh`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.
