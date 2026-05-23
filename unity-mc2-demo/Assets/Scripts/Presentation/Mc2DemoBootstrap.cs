using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MC2Demo.BattleCore;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MC2Demo.Presentation
{
    public sealed class Mc2DemoBootstrap : MonoBehaviour
    {
        [SerializeField] private string missionContractRelativePath = "Missions/mc2_01/mission-contract.json";
        [SerializeField] private string combatDataRelativePath = "Data/combat-data.json";
        [SerializeField] private float cameraHeight = 62f;
        [SerializeField] private float cameraPitch = 58f;
        [SerializeField] private float cameraYaw = 45f;
        private const float JumpDistance = 520f;
        private const float MiniMaxCommanderAdvanceSeconds = 8f;
        private const float LoadoutCardHeight = 396f;
        private const float LoadoutCardStride = 408f;

        private readonly Dictionary<string, DemoUnitView> unitViews = new();
        private readonly Dictionary<string, DemoStructureView> structureViews = new();
        private readonly Dictionary<string, GameObject> unitSelectionMarkers = new();
        private readonly Dictionary<string, GameObject> unitOrderMarkers = new();
        private readonly Dictionary<string, GameObject> unitFocusMarkers = new();
        private readonly Dictionary<string, GameObject> unitRangeMarkers = new();
        private readonly Dictionary<string, GameObject> unitTargetLines = new();
        private readonly Dictionary<string, GameObject> unitHealthBarBacks = new();
        private readonly Dictionary<string, GameObject> unitHealthBarFills = new();
        private readonly Dictionary<int, List<GameObject>> objectiveAreaMarkers = new();
        private readonly Dictionary<string, GameObject> structureHealthBarBacks = new();
        private readonly Dictionary<string, GameObject> structureHealthBarFills = new();
        private readonly Dictionary<string, Material> materialCache = new(StringComparer.Ordinal);
        private readonly Dictionary<string, bool[]> loadoutWeaponEnabledByUnit = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, CombatLoadoutPlacementOverride[]> loadoutPlacementOverridesByUnit = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<CombatLoadoutFillerOverride>> loadoutFillerOverridesByUnit = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, bool[]> appliedLoadoutWeaponEnabledByUnit = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, CombatLoadoutPlacementOverride[]> appliedLoadoutPlacementOverridesByUnit = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, CombatLoadoutFillerOverride[]> appliedLoadoutFillerOverridesByUnit = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> selectedLoadoutWeaponByUnit = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<Material> ownedMaterials = new();
        private readonly List<string> combatLog = new();
        private BattleMission mission;
        private MissionScriptBridge scriptBridge;
        private CommanderCommandPort commandPort;
        private CommanderObservationPort observationPort;
        private CombatProfileCatalog combatProfiles = CombatProfileCatalog.Empty;
        private MechBayInventoryContract demoInventory;
        private MechBayInventoryValidationResult demoInventoryValidation;
        private MechBayMissionReceipt missionReceipt;
        private bool missionReceiptApplied;
        private string pendingDetachedUnitId;
        private bool pendingJumpOrder;
        private bool showMissionMap;
        private bool showLoadoutPanel;
        private bool showSystemPanel;
        private bool isPaused;
        private MissionResultState lastMissionResult = MissionResultState.InProgress;
        private Camera mainCamera;
        private Vector2 loadoutScroll;
        private int selectedRosterIndex;
        private bool showWarehouseDraftFitPreview;
        private bool showSquadSelectionPreview;
        private string warehouseDraftFitPreviewMechId;
        private string squadSelectionDraftOutgoingOwnedMechId;
        private string squadSelectionDraftIncomingOwnedMechId;
        private string squadSelectionLastOutgoingDisplayName;
        private string squadSelectionLastIncomingDisplayName;
        private string statusText = "Loading";
        private bool startupSmokeFailed;
        private const string StartupSmokeDepotOwnedMechId = "assembled-smoke-depot";
        private const string UpdatedSquadLoadedStatusText = "Updated squad loaded - Mech bay open";

        private void Start()
        {
            LoadMission();
            BuildWorld();
            RunStartupCommanderSequence();
            ScheduleSmokeTestQuitIfRequested();
        }

        private void Update()
        {
            if (mission == null)
            {
                return;
            }

            if (!isPaused && mission.Result == MissionResultState.InProgress)
            {
                mission.Tick(Time.deltaTime);
                scriptBridge?.CaptureFrame();
                CaptureCombatEvents();
                CaptureObjectiveEvents();
                CaptureUnitActivationEvents();
                CaptureScriptEvents();
                HandleWorldClick();
            }

            CaptureMissionResult();
            UpdateUnitVisibility();
            UpdateObjectiveAreaVisibility();
            UpdateCommandOverlays();
            FollowCommander();
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            foreach (Material material in ownedMaterials)
            {
                Destroy(material);
            }
        }

        private void LoadMission()
        {
            combatProfiles = LoadCombatProfiles();
            string path = Path.Combine(Application.streamingAssetsPath, missionContractRelativePath);
            if (!File.Exists(path))
            {
                statusText = "Missing mission contract: " + path;
                Debug.LogError(statusText);
                return;
            }

            mission = BattleMission.FromJson(File.ReadAllText(path), combatProfiles);
            scriptBridge = new MissionScriptBridge(mission);
            commandPort = new CommanderCommandPort(mission, JumpDistance, DemoTerrainView.IsUsableLandingPosition);
            observationPort = new CommanderObservationPort(mission);
            demoInventory = MechBayInventoryBuilder.BuildDemoInventory(mission.PlayerUnits());
            RefreshDemoInventoryValidation();
            if (!demoInventoryValidation.IsValid)
            {
                Debug.LogWarning("MC2 mech bay inventory review: " + FirstInventoryError(demoInventoryValidation));
            }

            statusText = "Loaded " + mission.Contract.mission.id;
            Debug.Log("MC2 demo loaded mission contract: " + mission.Contract.mission.id);
        }

        private CombatProfileCatalog LoadCombatProfiles()
        {
            string path = Path.Combine(Application.streamingAssetsPath, combatDataRelativePath);
            if (!File.Exists(path))
            {
                Debug.LogWarning("MC2 combat data missing, using fallback profiles: " + path);
                return CombatProfileCatalog.Empty;
            }

            CombatProfileCatalog catalog = CombatProfileCatalog.FromJson(File.ReadAllText(path));
            Debug.Log("MC2 demo loaded combat profiles: " + catalog.UnitProfileCount);
            return catalog;
        }

        private void BuildWorld()
        {
            if (mission == null)
            {
                return;
            }

            CreateGround();
            CreateLights();
            CreateCamera();
            CreateTerrainObjects();
            CreateUnits();
            CreateStaticObjects();
            CreateMarkers();
            CreateForests();
            CreateObjectiveAreas();
            CreateCommandOverlays();
            Debug.Log(
                "MC2 demo world built: units="
                + mission.Units.Count
                + ", structures="
                + mission.Structures.Count
                + ", objectives="
                + mission.Objectives.Count
                + ", terrainSamples="
                + (mission.Contract.terrainMesh == null || mission.Contract.terrainMesh.samples == null ? 0 : mission.Contract.terrainMesh.samples.Length)
                + ", terrainObjects="
                + (mission.Contract.terrainObjects == null ? 0 : mission.Contract.terrainObjects.Length)
                + ", forests="
                + (mission.Contract.forests == null ? 0 : mission.Contract.forests.Length));
        }

        private bool TryApplyMissionRestartRuntimeSwap(bool keepMechBayOpen = false)
        {
            MechBayMissionRestartRuntimeSwapResult result =
                MechBayMissionHandoffPreviewService.TryBuildRestartRuntimeSwap(
                    demoInventory,
                    mission?.Contract,
                    combatProfiles);
            if (result == null || !result.Accepted || result.Mission == null)
            {
                statusText = result?.Reason ?? "Restart unavailable";
                AddCombatLogLine("Mission restart rejected: " + statusText);
                return false;
            }

            ApplyRestartedMission(result.Mission, result, keepMechBayOpen);
            return true;
        }

        private void ApplyRestartedMission(
            BattleMission replacementMission,
            MechBayMissionRestartRuntimeSwapResult result,
            bool keepMechBayOpen)
        {
            if (replacementMission == null)
            {
                statusText = "Restart unavailable";
                return;
            }

            string completedReplacementText = HasSquadSelectionCompletedReplacement()
                ? SquadSelectionCompletedReplacementText()
                : null;
            Time.timeScale = 1f;
            int clearedRoots = ClearRuntimeWorld();
            mission = replacementMission;
            scriptBridge = new MissionScriptBridge(mission);
            commandPort = new CommanderCommandPort(mission, JumpDistance, DemoTerrainView.IsUsableLandingPosition);
            observationPort = new CommanderObservationPort(mission);
            missionReceipt = null;
            missionReceiptApplied = false;
            pendingDetachedUnitId = null;
            pendingJumpOrder = false;
            showMissionMap = false;
            showLoadoutPanel = keepMechBayOpen;
            showSystemPanel = false;
            showWarehouseDraftFitPreview = false;
            showSquadSelectionPreview = false;
            warehouseDraftFitPreviewMechId = null;
            ClearSquadSelectionDraft();
            ClearSquadSelectionCompletedReplacement();
            lastMissionResult = mission.Result;

            BuildWorld();
            RefreshDemoInventoryValidation();
            SetPaused(keepMechBayOpen && mission.Result == MissionResultState.InProgress);
            statusText = MissionRestartStatusText(result, keepMechBayOpen);
            AddCombatLogLine(statusText + ": " + (result?.Summary ?? "replacement ready"));
            if (!string.IsNullOrWhiteSpace(completedReplacementText))
            {
                AddCombatLogLine("Updated squad loaded: " + completedReplacementText);
            }

            AddCombatLogLine("Identity map: " + RestartIdentityText(result));
            MechBayOwnedRosterEntry[] roster = MechBayOwnedRosterService.BuildRosterPreview(demoInventory);
            AddCombatLogLine("Roster state: " + RosterMissionStateText(BuildRosterMissionState(roster)));
            Debug.Log(
                "MC2 mission restart applied: clearedRoots="
                + clearedRoots.ToString(CultureInfo.InvariantCulture)
                + " keepMechBayOpen="
                + keepMechBayOpen);
        }

        private static string MissionRestartStatusText(
            MechBayMissionRestartRuntimeSwapResult result,
            bool keepMechBayOpen)
        {
            if (keepMechBayOpen && result?.IncludesDepotMissionSlot == true)
            {
                return UpdatedSquadLoadedStatusText;
            }

            if (keepMechBayOpen)
            {
                return "Mission restarted - Mech bay open";
            }

            return result?.Message ?? "Mission restarted";
        }

        private int ClearRuntimeWorld()
        {
            unitViews.Clear();
            structureViews.Clear();
            unitSelectionMarkers.Clear();
            unitOrderMarkers.Clear();
            unitFocusMarkers.Clear();
            unitRangeMarkers.Clear();
            unitTargetLines.Clear();
            unitHealthBarBacks.Clear();
            unitHealthBarFills.Clear();
            objectiveAreaMarkers.Clear();
            structureHealthBarBacks.Clear();
            structureHealthBarFills.Clear();
            loadoutWeaponEnabledByUnit.Clear();
            loadoutPlacementOverridesByUnit.Clear();
            loadoutFillerOverridesByUnit.Clear();
            appliedLoadoutWeaponEnabledByUnit.Clear();
            appliedLoadoutPlacementOverridesByUnit.Clear();
            appliedLoadoutFillerOverridesByUnit.Clear();
            selectedLoadoutWeaponByUnit.Clear();
            combatLog.Clear();
            mainCamera = null;

            int clearedRoots = 0;
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                GameObject root = roots[index];
                if (root == null || root == gameObject || root.GetComponent<Mc2DemoBootstrap>() != null)
                {
                    continue;
                }

                root.SetActive(false);
                Destroy(root);
                clearedRoots++;
            }

            return clearedRoots;
        }

        private void ScheduleSmokeTestQuitIfRequested()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int index = 0; index < args.Length; index++)
            {
                if (args[index] == "-mc2SmokeTest")
                {
                    StartCoroutine(QuitSmokeTestAfterRealtimeDelay());
                    return;
                }
            }
        }

        private IEnumerator QuitSmokeTestAfterRealtimeDelay()
        {
            yield return new WaitForSecondsRealtime(0.25f);
            QuitSmokeTest();
        }

        private void RunStartupCommanderSequence()
        {
            if (mission == null)
            {
                return;
            }

            string[] args = Environment.GetCommandLineArgs();
            for (int index = 0; index < args.Length; index++)
            {
                switch (args[index])
                {
                    case "-mc2Command":
                        index = RunStartupCommanderCommand(args, index);
                        break;
                    case "-mc2CommandFile":
                        index = RunStartupCommanderCommandFile(args, index);
                        break;
                    case "-mc2AdvanceSeconds":
                        index = RunStartupSimulationAdvance(args, index);
                        break;
                    case "-mc2ReportState":
                        ReportStartupCommanderState();
                        break;
                    case "-mc2RestartMission":
                        RunStartupMissionRestart();
                        break;
                    case "-mc2MinimaxCommanderSteps":
                        index = RunStartupMiniMaxCommander(args, index);
                        break;
                }
            }
        }

        private int RunStartupCommanderCommand(string[] args, int index)
        {
            if (commandPort == null)
            {
                return index;
            }

            if (index + 1 >= args.Length)
            {
                LogStartupCommanderCommand("", CommanderCommandResult.Blocked("Missing command text after -mc2Command."));
                return index;
            }

            string commandLine = args[index + 1];
            RunStartupCommanderCommand(commandLine);
            return index + 1;
        }

        private void RunStartupCommanderCommand(string commandLine)
        {
            if (commandPort == null)
            {
                return;
            }

            LogStartupCommanderCommand(commandLine, commandPort.IssueText(commandLine));
        }

        private int RunStartupCommanderCommandFile(string[] args, int index)
        {
            if (index + 1 >= args.Length)
            {
                Debug.LogWarning("MC2 commander command file blocked: missing path after -mc2CommandFile.");
                return index;
            }

            string requestedPath = args[index + 1];
            string resolvedPath = ResolveStartupCommanderCommandFilePath(requestedPath);
            try
            {
                StartupCommanderScriptAction[] actions = StartupCommanderScript.LoadFile(resolvedPath);
                Debug.Log("MC2 commander command file: path=" + resolvedPath + " actions=" + actions.Length);
                RunStartupCommanderActions(actions);
            }
            catch (Exception exception)
            {
                AddCombatLogLine("CLI command file blocked: " + requestedPath);
                statusText = "Command file blocked";
                startupSmokeFailed = true;
                Debug.LogError("MC2 commander command file blocked: path=" + resolvedPath + " error=" + exception.Message);
            }

            return index + 1;
        }

        private void RunStartupCommanderActions(IEnumerable<StartupCommanderScriptAction> actions)
        {
            foreach (StartupCommanderScriptAction action in actions)
            {
                switch (action.Kind)
                {
                    case StartupCommanderScriptActionKind.Command:
                        RunStartupCommanderCommand(action.CommandText);
                        break;
                    case StartupCommanderScriptActionKind.Advance:
                        AdvanceStartupSimulation(action.AdvanceSeconds);
                        break;
                    case StartupCommanderScriptActionKind.Report:
                        ReportStartupCommanderState();
                        break;
                    case StartupCommanderScriptActionKind.Restart:
                        RunStartupMissionRestart();
                        break;
                    case StartupCommanderScriptActionKind.MechBayLaunch:
                        RunStartupMechBayLaunch();
                        break;
                    case StartupCommanderScriptActionKind.PrepareDepotCandidate:
                        RunStartupPrepareDepotCandidate();
                        break;
                    case StartupCommanderScriptActionKind.PrepareLocalCandidate:
                        RunStartupPrepareLocalCandidate();
                        break;
                    case StartupCommanderScriptActionKind.SquadSwap:
                        RunStartupSquadSwap();
                        break;
                    case StartupCommanderScriptActionKind.AssertRestartIdentity:
                        RunStartupRestartIdentityAssertion(action.RequireDepotIdentity);
                        break;
                }
            }
        }

        private static string ResolveStartupCommanderCommandFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path))
            {
                return path;
            }

            if (File.Exists(path))
            {
                return Path.GetFullPath(path);
            }

            string streamingAssetPath = Path.Combine(Application.streamingAssetsPath, path);
            if (File.Exists(streamingAssetPath))
            {
                return streamingAssetPath;
            }

            return Path.GetFullPath(path);
        }

        private void LogStartupCommanderCommand(string commandLine, CommanderCommandResult result)
        {
            string line = "CLI command: " + (string.IsNullOrWhiteSpace(commandLine) ? "<empty>" : commandLine);
            line += result.Accepted ? " accepted=" + result.AcceptedCount : " blocked";
            AddCombatLogLine(line);
            statusText = result.Message;
            Debug.Log("MC2 commander command: " + line + " message=" + result.Message);
        }

        private void RunStartupMissionRestart()
        {
            bool accepted = TryApplyMissionRestartRuntimeSwap();
            Debug.Log(
                "MC2 commander restart: accepted="
                + accepted
                + " mission="
                + (mission?.Contract?.mission?.id ?? "none")
                + " result="
                + (mission == null ? "none" : mission.Result.ToString())
                + " status="
                + statusText);
        }

        private void RunStartupMechBayLaunch()
        {
            MechBayMissionRestartDryRun dryRun = MechBayMissionHandoffPreviewService.BuildRestartDryRun(demoInventory);
            bool expectsUpdatedSquadStatus = dryRun?.IncludesDepotMissionSlot == true;
            bool accepted = TryApplyMissionRestartRuntimeSwap(keepMechBayOpen: true);
            bool bayOpen = showLoadoutPanel;
            bool paused = isPaused;
            bool statusOk = !expectsUpdatedSquadStatus
                || string.Equals(statusText, UpdatedSquadLoadedStatusText, StringComparison.Ordinal);
            string summary = "accepted="
                + accepted
                + " bayOpen="
                + bayOpen
                + " paused="
                + paused
                + " depot="
                + expectsUpdatedSquadStatus
                + " status="
                + statusText;

            if (accepted && bayOpen && paused && statusOk)
            {
                AddCombatLogLine("CLI mech bay launch OK: " + statusText);
                Debug.Log("MC2 commander mech bay launch: " + summary);
                return;
            }

            startupSmokeFailed = true;
            AddCombatLogLine("CLI mech bay launch failed: " + summary);
            Debug.LogError("MC2 commander mech bay launch failed: " + summary);
        }

        private void RunStartupPrepareDepotCandidate()
        {
            bool accepted = EnsureStartupDepotSwapCandidate();
            string message = accepted ? "prepared" : "blocked";
            AddCombatLogLine("CLI depot candidate " + message);
            Debug.Log("MC2 commander depot candidate: " + message);
        }

        private void RunStartupPrepareLocalCandidate()
        {
            string ownedMechId = EnsureLocalReadyCandidate("CLI local", true);
            if (!string.IsNullOrWhiteSpace(ownedMechId))
            {
                AddCombatLogLine("CLI local candidate ready: " + ownedMechId);
                Debug.Log("MC2 commander local candidate: ready ownedMechId=" + ownedMechId);
                return;
            }

            AddCombatLogLine("CLI local candidate blocked");
            Debug.LogError("MC2 commander local candidate blocked: status=" + statusText);
        }

        private string EnsureLocalReadyCandidate(string logPrefix, bool markStartupSmokeFailure)
        {
            if (demoInventory == null)
            {
                return BlockLocalCandidate("Inventory missing", logPrefix, markStartupSmokeFailure);
            }

            string existingCandidate = FirstStartupDepotCandidateOwnedMechId();
            if (!string.IsNullOrWhiteSpace(existingCandidate))
            {
                statusText = "Local candidate ready";
                return existingCandidate;
            }

            string unitType = FirstRuntimePlayerUnitType();
            MechBayMissionReceipt receipt = MechBayMissionReceiptService.ApplyMissionReceipt(
                demoInventory,
                new MissionResultSummary
                {
                    destroyedEnemyUnits = 3,
                    completedRewardResourcePoints = 3000,
                    visibleRewardResourcePoints = 3000,
                    salvageClaimCount = 3,
                    destroyedEnemyUnitLabels = new[]
                    {
                        "enemy-1 " + unitType,
                        "enemy-2 " + unitType,
                        "enemy-3 " + unitType
                    }
                });
            AddCombatLogLine(
                logPrefix + " receipt: +" + FormatTokens(receipt.TokenDelta)
                + " token fragments +" + receipt.SalvageFragmentCount
                + ReceiptAssemblyLogText(receipt));
            RefreshDemoInventoryValidation();

            string targetOwnedMechId = FirstStartupPendingWarehouseMechId();
            if (string.IsNullOrWhiteSpace(targetOwnedMechId))
            {
                return BlockLocalCandidate("No assembled warehouse mech", logPrefix, markStartupSmokeFailure);
            }

            MechBayOwnedRosterEntry target = StartupRosterEntryByOwnedMechId(targetOwnedMechId);
            if (target == null)
            {
                return BlockLocalCandidate("Assembled mech missing", logPrefix, markStartupSmokeFailure);
            }

            if (!target.hasPilotAssignment)
            {
                MechBayPilotHireCandidate candidate =
                    FirstPilotHireCandidate(MechBayPilotHirePreviewService.BuildPreview(demoInventory));
                if (candidate == null)
                {
                    return BlockLocalCandidate("No NPC pilot candidate", logPrefix, markStartupSmokeFailure);
                }

                MechBayPilotHireResult result =
                    MechBayPilotHirePreviewService.TryApplyDemoHire(demoInventory, targetOwnedMechId, candidate.pilotId);
                if (result == null || !result.Accepted)
                {
                    return BlockLocalCandidate(result?.Message ?? "Pilot hire unavailable", logPrefix, markStartupSmokeFailure);
                }

                AddCombatLogLine(logPrefix + " pilot: " + result.Message);
                RefreshDemoInventoryValidation();
            }

            target = StartupRosterEntryByOwnedMechId(targetOwnedMechId);
            if (target == null)
            {
                return BlockLocalCandidate("Assembled mech missing after pilot hire", logPrefix, markStartupSmokeFailure);
            }

            if (!target.hasSpareWeaponStock)
            {
                MechBayWeaponShopEntry entry =
                    FirstWeaponShopEntry(MechBayWeaponShopPreviewService.BuildPreview(demoInventory));
                if (entry == null)
                {
                    return BlockLocalCandidate("No ordinary weapon shop entry", logPrefix, markStartupSmokeFailure);
                }

                MechBayWeaponPurchasePreviewResult result =
                    MechBayWeaponShopPreviewService.TryApplyDemoPurchase(demoInventory, entry.itemId);
                if (result == null || !result.Accepted)
                {
                    return BlockLocalCandidate(result?.Message ?? "Weapon purchase unavailable", logPrefix, markStartupSmokeFailure);
                }

                AddCombatLogLine(logPrefix + " weapon: " + result.Message);
                RefreshDemoInventoryValidation();
            }

            target = StartupRosterEntryByOwnedMechId(targetOwnedMechId);
            if (target == null)
            {
                return BlockLocalCandidate("Assembled mech missing before draft fit", logPrefix, markStartupSmokeFailure);
            }

            if (target.hasDraftFitStub)
            {
                MechBayWarehouseDraftFitApplyResult result =
                    MechBayWarehouseDraftFitPreviewService.TryApplyDemoFit(demoInventory, targetOwnedMechId);
                if (result == null || !result.Accepted)
                {
                    return BlockLocalCandidate(result?.Message ?? "Draft fit unavailable", logPrefix, markStartupSmokeFailure);
                }

                AddCombatLogLine(logPrefix + " fit: " + result.Message);
                RefreshDemoInventoryValidation();
            }

            string readyCandidate = FirstStartupDepotCandidateOwnedMechId();
            if (string.IsNullOrWhiteSpace(readyCandidate))
            {
                return BlockLocalCandidate("No next-squad candidate after local setup", logPrefix, markStartupSmokeFailure);
            }

            statusText = "Local candidate ready";
            return readyCandidate;
        }

        private string BlockLocalCandidate(string reason, string logPrefix, bool markStartupSmokeFailure)
        {
            if (markStartupSmokeFailure)
            {
                startupSmokeFailed = true;
            }

            statusText = "Local candidate blocked";
            Debug.LogError("MC2 commander local candidate blocked: " + logPrefix + " " + reason);
            return null;
        }

        private bool EnsureStartupDepotSwapCandidate()
        {
            if (demoInventory == null)
            {
                startupSmokeFailed = true;
                statusText = "Depot candidate blocked";
                return false;
            }

            List<MechBayOwnedMechDefinition> ownedMechs = new(demoInventory.ownedMechs ?? Array.Empty<MechBayOwnedMechDefinition>());
            MechBayOwnedMechDefinition existing = FindOwnedMechByOwnedId(StartupSmokeDepotOwnedMechId);
            if (existing == null)
            {
                string unitType = FirstRuntimePlayerUnitType();
                existing = new MechBayOwnedMechDefinition
                {
                    ownedMechId = StartupSmokeDepotOwnedMechId,
                    unitId = "warehouse-" + StartupSmokeDepotOwnedMechId,
                    unitType = unitType,
                    chassisId = unitType,
                    displayName = unitType + " Smoke Depot",
                    activeLoadoutId = MechBayWarehouseDraftFitPreviewService.DemoWarehouseFitLoadoutId,
                    availableForMission = false,
                    conditionPercent = 100,
                    pilotId = "pilot-smoke-depot",
                    pilotDisplayName = "Smoke Depot Pilot",
                    pilotType = "NPC"
                };
                ownedMechs.Add(existing);
                demoInventory.ownedMechs = ownedMechs.ToArray();
            }

            existing.availableForMission = false;
            existing.conditionPercent = Math.Max(0, Math.Min(100, existing.conditionPercent <= 0 ? 100 : existing.conditionPercent));
            existing.activeLoadoutId = MechBayWarehouseDraftFitPreviewService.DemoWarehouseFitLoadoutId;
            if (string.IsNullOrWhiteSpace(existing.pilotId))
            {
                existing.pilotId = "pilot-smoke-depot";
                existing.pilotDisplayName = "Smoke Depot Pilot";
                existing.pilotType = "NPC";
            }

            RefreshDemoInventoryValidation();
            MechBaySquadSelectionPreview preview = MechBaySquadSelectionPreviewService.BuildPreview(demoInventory);
            bool ready = StartupDepotCandidateReady(preview);
            if (!ready)
            {
                startupSmokeFailed = true;
                statusText = "Depot candidate blocked";
            }
            else
            {
                statusText = "Depot candidate ready";
            }

            return ready;
        }

        private static bool StartupDepotCandidateReady(MechBaySquadSelectionPreview preview)
        {
            MechBaySquadSelectionSlot[] candidates = preview?.DepotCandidates ?? Array.Empty<MechBaySquadSelectionSlot>();
            for (int index = 0; index < candidates.Length; index++)
            {
                if (string.Equals(
                    candidates[index]?.ownedMechId,
                    StartupSmokeDepotOwnedMechId,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private string FirstStartupDepotCandidateOwnedMechId()
        {
            return FirstStartupDepotCandidateOwnedMechId(
                MechBaySquadSelectionPreviewService.BuildPreview(demoInventory));
        }

        private static string FirstStartupDepotCandidateOwnedMechId(MechBaySquadSelectionPreview preview)
        {
            MechBaySquadSelectionSlot[] candidates = preview?.DepotCandidates ?? Array.Empty<MechBaySquadSelectionSlot>();
            for (int index = 0; index < candidates.Length; index++)
            {
                if (!string.IsNullOrWhiteSpace(candidates[index]?.ownedMechId))
                {
                    return candidates[index].ownedMechId;
                }
            }

            return null;
        }

        private string FirstStartupPendingWarehouseMechId()
        {
            MechBayOwnedRosterEntry[] roster = MechBayOwnedRosterService.BuildRosterPreview(demoInventory);
            for (int index = 0; index < roster.Length; index++)
            {
                MechBayOwnedRosterEntry entry = roster[index];
                if (entry != null
                    && entry.isWarehouseMech
                    && !entry.availableForMission
                    && (entry.hasDraftFitStub || entry.squadSelectionCandidate)
                    && !string.IsNullOrWhiteSpace(entry.ownedMechId))
                {
                    return entry.ownedMechId;
                }
            }

            return null;
        }

        private MechBayOwnedRosterEntry StartupRosterEntryByOwnedMechId(string ownedMechId)
        {
            if (string.IsNullOrWhiteSpace(ownedMechId))
            {
                return null;
            }

            MechBayOwnedRosterEntry[] roster = MechBayOwnedRosterService.BuildRosterPreview(demoInventory);
            for (int index = 0; index < roster.Length; index++)
            {
                MechBayOwnedRosterEntry entry = roster[index];
                if (entry != null
                    && string.Equals(entry.ownedMechId, ownedMechId, StringComparison.OrdinalIgnoreCase))
                {
                    return entry;
                }
            }

            return null;
        }

        private string FirstRuntimePlayerUnitType()
        {
            if (mission != null)
            {
                foreach (UnitState unit in mission.PlayerUnits())
                {
                    if (!string.IsNullOrWhiteSpace(unit?.UnitType))
                    {
                        return unit.UnitType;
                    }
                }
            }

            MechBayOwnedMechDefinition[] ownedMechs = demoInventory?.ownedMechs ?? Array.Empty<MechBayOwnedMechDefinition>();
            for (int index = 0; index < ownedMechs.Length; index++)
            {
                if (!string.IsNullOrWhiteSpace(ownedMechs[index]?.unitType))
                {
                    return ownedMechs[index].unitType;
                }
            }

            return "Bushwacker";
        }

        private void RunStartupSquadSwap()
        {
            string incomingOwnedMechId = FirstStartupDepotCandidateOwnedMechId();
            if (string.IsNullOrWhiteSpace(incomingOwnedMechId))
            {
                if (!EnsureStartupDepotSwapCandidate())
                {
                    return;
                }

                incomingOwnedMechId = FirstStartupDepotCandidateOwnedMechId();
            }

            if (string.IsNullOrWhiteSpace(incomingOwnedMechId))
            {
                startupSmokeFailed = true;
                statusText = "CLI squad swap blocked";
                AddCombatLogLine("CLI squad swap blocked: no depot candidate");
                Debug.LogError("MC2 commander squad swap blocked: no depot candidate");
                return;
            }

            MechBaySquadSelectionDraftState draft =
                MechBaySquadSelectionPreviewService.BuildDraftState(demoInventory, null, incomingOwnedMechId);
            MechBaySquadSelectionApplyResult result =
                MechBaySquadSelectionPreviewService.TryApplyPendingSwap(demoInventory, draft);
            if (result?.Accepted == true)
            {
                ClearSquadSelectionDraft();
                demoInventoryValidation = MechBayInventoryValidator.Validate(demoInventory);
                statusText = "CLI squad swap applied";
                AddCombatLogLine("CLI squad swap: " + result.Summary);
                Debug.Log("MC2 commander squad swap: accepted summary=" + result.Summary);
                return;
            }

            startupSmokeFailed = true;
            statusText = "CLI squad swap blocked";
            string reason = string.IsNullOrWhiteSpace(result?.Reason) ? "unknown" : result.Reason;
            AddCombatLogLine("CLI squad swap blocked: " + reason);
            Debug.LogError("MC2 commander squad swap blocked: " + reason);
        }

        private void RunStartupRestartIdentityAssertion(bool requireDepotIdentity)
        {
            RestartIdentityAssertionResult result = BuildRestartIdentityAssertion(requireDepotIdentity);
            if (result.Accepted)
            {
                AddCombatLogLine("CLI identity assert OK: " + result.Summary);
                Debug.Log("MC2 restart identity assertion OK: " + result.Summary);
                return;
            }

            startupSmokeFailed = true;
            statusText = "Restart identity assertion failed";
            AddCombatLogLine("CLI identity assert failed: " + result.Summary);
            Debug.LogError("MC2 restart identity assertion failed: " + result.Summary);
        }

        private RestartIdentityAssertionResult BuildRestartIdentityAssertion(bool requireDepotIdentity)
        {
            MechBayMissionRestartDryRun dryRun = MechBayMissionHandoffPreviewService.BuildRestartDryRun(demoInventory);
            MechBayMissionRestartSpawnIntent[] intents = dryRun?.SpawnIntents ?? Array.Empty<MechBayMissionRestartSpawnIntent>();
            if (mission == null || intents.Length == 0)
            {
                return RestartIdentityAssertionResult.Blocked("No restart identity intents");
            }

            int unitIndex = 0;
            int matched = 0;
            bool depotMatched = false;
            List<string> mismatches = new();
            foreach (UnitState unit in mission.PlayerUnits())
            {
                MechBayMissionRestartSpawnIntent intent = unitIndex < intents.Length ? intents[unitIndex] : null;
                unitIndex++;
                string expected = intent?.ownedMechId ?? "";
                string actual = unit?.OwnedMechId ?? "";
                bool same = !string.IsNullOrWhiteSpace(expected)
                    && string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);
                if (same)
                {
                    matched++;
                    depotMatched = depotMatched || intent?.isDepotMissionSlot == true;
                    continue;
                }

                if (mismatches.Count < 3)
                {
                    mismatches.Add((string.IsNullOrWhiteSpace(expected) ? "<none>" : expected)
                        + "!="
                        + (string.IsNullOrWhiteSpace(actual) ? "<none>" : actual));
                }
            }

            bool countOk = unitIndex == intents.Length;
            bool matchedAll = matched == intents.Length;
            bool depotOk = !requireDepotIdentity || depotMatched;
            string summary = "matched "
                + matched.ToString(CultureInfo.InvariantCulture)
                + "/"
                + intents.Length.ToString(CultureInfo.InvariantCulture)
                + " units"
                + " depot="
                + depotMatched
                + " countOk="
                + countOk;
            if (mismatches.Count > 0)
            {
                summary += " mismatches " + string.Join("; ", mismatches);
            }

            return new RestartIdentityAssertionResult
            {
                Accepted = countOk && matchedAll && depotOk,
                Summary = summary
            };
        }

        private int RunStartupSimulationAdvance(string[] args, int index)
        {
            if (index + 1 >= args.Length)
            {
                Debug.LogWarning("MC2 commander advance blocked: missing seconds after -mc2AdvanceSeconds.");
                return index;
            }

            string value = args[index + 1];
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float seconds))
            {
                Debug.LogWarning("MC2 commander advance blocked: invalid seconds '" + value + "'.");
                return index + 1;
            }

            AdvanceStartupSimulation(seconds);
            return index + 1;
        }

        private void AdvanceStartupSimulation(float seconds)
        {
            float clampedSeconds = Mathf.Clamp(seconds, 0f, 60f);
            if (clampedSeconds <= 0f)
            {
                Debug.Log("MC2 commander advance: seconds=0 steps=0 result=" + mission.Result);
                return;
            }

            const float StepSeconds = 0.25f;
            int steps = Mathf.CeilToInt(clampedSeconds / StepSeconds);
            for (int step = 0; step < steps && mission.Result == MissionResultState.InProgress; step++)
            {
                float delta = Mathf.Min(StepSeconds, clampedSeconds - step * StepSeconds);
                mission.Tick(delta);
                scriptBridge?.CaptureFrame();
            }

            AddCombatLogLine("CLI advance: " + clampedSeconds.ToString("0.##", CultureInfo.InvariantCulture) + "s");
            Debug.Log("MC2 commander advance: seconds=" + clampedSeconds.ToString("0.###", CultureInfo.InvariantCulture)
                + " steps=" + steps
                + " result=" + mission.Result);
        }

        private void ReportStartupCommanderState()
        {
            if (observationPort == null)
            {
                return;
            }

            CommanderObservation observation = observationPort.Observe();
            string json = JsonUtility.ToJson(observation);
            AddCombatLogLine("CLI report: state #" + observation.reportIndex);
            Debug.Log("MC2 commander observation #" + observation.reportIndex + ": " + json);
        }

        private int RunStartupMiniMaxCommander(string[] args, int index)
        {
            if (index + 1 >= args.Length)
            {
                Debug.LogWarning("MC2 MiniMax commander blocked: missing step count after -mc2MinimaxCommanderSteps.");
                return index;
            }

            string value = args[index + 1];
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int requestedSteps))
            {
                Debug.LogWarning("MC2 MiniMax commander blocked: invalid step count '" + value + "'.");
                return index + 1;
            }

            int steps = Mathf.Clamp(requestedSteps, 0, 8);
            if (steps <= 0)
            {
                Debug.Log("MC2 MiniMax commander skipped: steps=0.");
                return index + 1;
            }

            MiniMaxCommanderConfig config = MiniMaxCommander.ConfigFromEnvironment();
            Debug.Log("MC2 MiniMax commander config: " + config.DescribeWithoutSecrets());
            if (!config.IsConfigured)
            {
                Debug.LogWarning("MC2 MiniMax commander blocked: set MINIMAX_API_KEY in the process environment.");
                return index + 1;
            }

            MiniMaxCommander commander = new(config);
            RuleCommander fallbackCommander = new();
            for (int step = 0; step < steps && mission.Result == MissionResultState.InProgress; step++)
            {
                CommanderObservation observation = observationPort.Observe();
                MiniMaxCommanderResult result = commander.ChooseDirective(observation);
                string directive = result.Directive;
                string source = "minimax";

                if (!result.Success || string.IsNullOrWhiteSpace(directive))
                {
                    directive = RuleCommander.DirectiveAssaultObjective;
                    source = "rule_fallback";
                }

                string commandLine = fallbackCommander.ChooseCommandForDirective(observation, directive);
                if (string.IsNullOrWhiteSpace(commandLine))
                {
                    Debug.LogWarning("MC2 MiniMax commander stopped: directive=" + directive + " no local command available. message=" + result.Message);
                    break;
                }

                Debug.Log("MC2 MiniMax commander step="
                    + (step + 1)
                    + " source="
                    + source
                    + " directive="
                    + directive
                    + " command="
                    + commandLine
                    + " message="
                    + result.Message);
                RunStartupCommanderCommand(commandLine);
                AdvanceStartupSimulation(MiniMaxCommanderAdvanceSeconds);
            }

            ReportStartupCommanderState();
            return index + 1;
        }

        private void QuitSmokeTest()
        {
            int exitCode = mission == null || startupSmokeFailed ? 1 : 0;
            Debug.Log("MC2 demo smoke test exiting with code " + exitCode);
            Application.Quit(exitCode);
        }

        private void CaptureCombatEvents()
        {
            foreach (CombatEvent combatEvent in mission.RecentCombatEvents)
            {
                string line = WeaponLabel(combatEvent.WeaponType)
                    + " "
                    + combatEvent.AttackerId
                    + " hit "
                    + combatEvent.TargetId
                    + " "
                    + combatEvent.SectionName
                    + " for "
                    + Mathf.RoundToInt(combatEvent.Damage);
                if (combatEvent.MitigatedDamage > 0.4f)
                {
                    line += " block " + Mathf.RoundToInt(combatEvent.MitigatedDamage);
                }

                if (combatEvent.DestroyedTarget)
                {
                    line += " destroyed";
                }

                AddCombatLogLine(line);
                SpawnCombatEffect(combatEvent);
                Debug.Log("MC2 combat: " + line);
            }
        }

        private void SpawnCombatEffect(CombatEvent combatEvent)
        {
            if (combatEvent.Damage <= 0f
                || !TryGetCombatPoint(combatEvent.AttackerId, out Vector3 attackerPoint)
                || !TryGetCombatPoint(combatEvent.TargetId, out Vector3 targetPoint))
            {
                return;
            }

            Color weaponColor = combatEvent.DestroyedTarget
                ? new Color(1f, 0.42f, 0.08f, 0.85f)
                : WeaponColor(combatEvent.WeaponType);
            CreateWeaponTrace(attackerPoint, targetPoint, combatEvent, weaponColor);
            CreateImpact(targetPoint, weaponColor, combatEvent.DestroyedTarget, ImpactScale(combatEvent.WeaponType));
            PulseTarget(combatEvent.TargetId, new Color(1f, 0.9f, 0.52f), combatEvent.DestroyedTarget ? 0.28f : 0.18f);
        }

        private bool TryGetCombatPoint(string id, out Vector3 point)
        {
            if (unitViews.TryGetValue(id, out DemoUnitView unitView) && unitView != null && unitView.Unit != null)
            {
                point = unitView.transform.position + Vector3.up * 0.55f;
                return true;
            }

            if (structureViews.TryGetValue(id, out DemoStructureView structureView) && structureView != null && structureView.Structure != null)
            {
                point = structureView.transform.position + Vector3.up * 0.65f;
                return true;
            }

            point = Vector3.zero;
            return false;
        }

        private void PulseTarget(string targetId, Color color, float duration)
        {
            if (unitViews.TryGetValue(targetId, out DemoUnitView unitView) && unitView != null)
            {
                unitView.PulseHit(color, duration);
                return;
            }

            if (structureViews.TryGetValue(targetId, out DemoStructureView structureView) && structureView != null)
            {
                structureView.PulseHit(color, duration);
            }
        }

        private static Color WeaponColor(string weaponType)
        {
            if (ContainsWeaponType(weaponType, "Energy"))
            {
                return new Color(0.32f, 0.88f, 1f, 0.82f);
            }

            if (ContainsWeaponType(weaponType, "Missile"))
            {
                return new Color(1f, 0.56f, 0.14f, 0.80f);
            }

            if (ContainsWeaponType(weaponType, "Ballistic"))
            {
                return new Color(1f, 0.92f, 0.68f, 0.76f);
            }

            return new Color(1f, 0.82f, 0.28f, 0.78f);
        }

        private static string WeaponLabel(string weaponType)
        {
            if (ContainsWeaponType(weaponType, "Energy"))
            {
                return "Energy";
            }

            if (ContainsWeaponType(weaponType, "Missile"))
            {
                return "Missile";
            }

            if (ContainsWeaponType(weaponType, "Ballistic"))
            {
                return "Ballistic";
            }

            return "Weapon";
        }

        private static float ImpactScale(string weaponType)
        {
            if (ContainsWeaponType(weaponType, "Missile"))
            {
                return 1.45f;
            }

            if (ContainsWeaponType(weaponType, "Ballistic"))
            {
                return 0.78f;
            }

            if (ContainsWeaponType(weaponType, "Energy"))
            {
                return 0.86f;
            }

            return 1f;
        }

        private static bool ContainsWeaponType(string weaponType, string value)
        {
            return !string.IsNullOrEmpty(weaponType)
                && weaponType.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void CreateWeaponTrace(Vector3 from, Vector3 to, CombatEvent combatEvent, Color color)
        {
            if (ContainsWeaponType(combatEvent.WeaponType, "Missile"))
            {
                CreateMissileTrace(from, to, color);
                return;
            }

            if (ContainsWeaponType(combatEvent.WeaponType, "Ballistic"))
            {
                CreateBallisticTrace(from, to, color);
                return;
            }

            if (ContainsWeaponType(combatEvent.WeaponType, "Energy"))
            {
                CreateEnergyTrace(from, to, color);
                return;
            }

            CreateBeam(from, to, color, 0.16f, 0.045f);
        }

        private void CreateMissileTrace(Vector3 from, Vector3 to, Color color)
        {
            Vector3 direction = to - from;
            float distance = direction.magnitude;
            if (distance <= 0.01f)
            {
                return;
            }

            Vector3 side = LateralVector(direction) * 0.12f;
            float arcHeight = Mathf.Clamp(distance * 0.16f, 0.45f, 2.2f);
            for (int index = 0; index < 3; index++)
            {
                float startT = 0.10f + index * 0.21f;
                float endT = Mathf.Min(0.95f, startT + 0.18f);
                Vector3 offset = side * (index - 1);
                Vector3 segmentStart = ArcPoint(from, to, startT, arcHeight) + offset;
                Vector3 segmentEnd = ArcPoint(from, to, endT, arcHeight) + offset * 0.45f;
                CreateBeam(segmentStart, segmentEnd, color, 0.34f, 0.055f);
                CreateImpact(segmentStart - direction.normalized * 0.08f, new Color(0.20f, 0.20f, 0.18f, 0.34f), false, 0.28f + index * 0.08f);
            }

            CreateImpact(to + Vector3.up * 0.10f, new Color(0.15f, 0.14f, 0.12f, 0.48f), false, 0.80f);
        }

        private void CreateBallisticTrace(Vector3 from, Vector3 to, Color color)
        {
            Vector3 direction = to - from;
            float distance = direction.magnitude;
            if (distance <= 0.01f)
            {
                return;
            }

            Vector3 normalized = direction.normalized;
            Vector3 side = LateralVector(direction) * 0.07f;
            Vector3 tracerStart = Vector3.Lerp(from, to, 0.58f);
            Vector3 tracerEnd = Vector3.Lerp(from, to, 0.96f);
            CreateBeam(tracerStart, tracerEnd, color, 0.075f, 0.018f);
            CreateBeam(from + side, from + normalized * Mathf.Min(distance * 0.18f, 1.25f) + side * 0.35f, new Color(1f, 0.64f, 0.22f, 0.72f), 0.08f, 0.026f);

            Vector3 sparkBase = to - normalized * 0.22f;
            CreateBeam(sparkBase, to + side * 0.80f + Vector3.up * 0.12f, new Color(1f, 0.86f, 0.36f, 0.68f), 0.07f, 0.012f);
            CreateBeam(sparkBase, to - side * 0.68f + Vector3.up * 0.07f, new Color(1f, 0.70f, 0.20f, 0.56f), 0.06f, 0.010f);
        }

        private void CreateEnergyTrace(Vector3 from, Vector3 to, Color color)
        {
            Color halo = new(color.r, color.g, color.b, 0.26f);
            Color core = new(0.72f, 1f, 1f, 0.92f);
            CreateBeam(from, to, halo, 0.16f, 0.070f);
            CreateBeam(from, to, color, 0.13f, 0.038f);
            CreateBeam(from + Vector3.up * 0.04f, to + Vector3.up * 0.04f, core, 0.08f, 0.016f);
            CreateImpact(to + Vector3.up * 0.05f, new Color(0.50f, 0.95f, 1f, 0.50f), false, 0.48f);
        }

        private static Vector3 ArcPoint(Vector3 from, Vector3 to, float t, float arcHeight)
        {
            return Vector3.Lerp(from, to, t) + Vector3.up * Mathf.Sin(t * Mathf.PI) * arcHeight;
        }

        private static Vector3 LateralVector(Vector3 direction)
        {
            Vector3 side = Vector3.Cross(direction.normalized, Vector3.up);
            return side.sqrMagnitude <= 0.0001f ? Vector3.right : side.normalized;
        }

        private void CreateBeam(Vector3 from, Vector3 to, Color color, float duration, float radius)
        {
            Vector3 direction = to - from;
            float length = direction.magnitude;
            if (length <= 0.01f)
            {
                return;
            }

            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beam.name = "Weapon Beam";
            beam.transform.position = (from + to) * 0.5f;
            beam.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            Collider beamCollider = beam.GetComponent<Collider>();
            if (beamCollider != null)
            {
                Destroy(beamCollider);
            }

            DemoTransientEffect transient = beam.AddComponent<DemoTransientEffect>();
            transient.Begin(color, duration, new Vector3(radius, length * 0.5f, radius), new Vector3(radius * 0.28f, length * 0.5f, radius * 0.28f));
        }

        private void CreateImpact(Vector3 position, Color color, bool destroyedTarget, float scale)
        {
            GameObject impact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            impact.name = destroyedTarget ? "Kill Impact" : "Hit Impact";
            impact.transform.position = position;
            Collider impactCollider = impact.GetComponent<Collider>();
            if (impactCollider != null)
            {
                Destroy(impactCollider);
            }

            float duration = destroyedTarget ? 0.36f : 0.22f;
            Vector3 fromScale = Vector3.one * (destroyedTarget ? 0.24f : 0.14f) * scale;
            Vector3 toScale = Vector3.one * (destroyedTarget ? 1.25f : 0.72f) * scale;
            DemoTransientEffect transient = impact.AddComponent<DemoTransientEffect>();
            transient.Begin(color, duration, fromScale, toScale);
        }

        private void CaptureObjectiveEvents()
        {
            foreach (ObjectiveEvent objectiveEvent in mission.RecentObjectiveEvents)
            {
                if (objectiveEvent.IsHidden)
                {
                    continue;
                }

                if (objectiveEvent.Kind == ObjectiveEventKind.Activated)
                {
                    AddCombatLogLine("Objective active: " + objectiveEvent.Title);
                    Debug.Log("MC2 objective active: " + objectiveEvent.Title);
                }
                else if (objectiveEvent.Kind == ObjectiveEventKind.Completed)
                {
                    AddCombatLogLine("Objective done: " + objectiveEvent.Title);
                    Debug.Log("MC2 objective complete: " + objectiveEvent.Title);
                }
            }
        }

        private void CaptureUnitActivationEvents()
        {
            foreach (UnitActivationEvent activationEvent in mission.RecentUnitActivationEvents)
            {
                string line = "Contact: " + activationEvent.UnitType;
                AddCombatLogLine(line);
                Debug.Log("MC2 enemy activated: " + activationEvent.UnitId + " " + activationEvent.UnitType + " " + activationEvent.Brain);
            }
        }

        private void CaptureScriptEvents()
        {
            if (scriptBridge == null)
            {
                return;
            }

            foreach (MissionScriptEvent scriptEvent in scriptBridge.RecentEvents)
            {
                string line = "Script: " + scriptEvent.Signal;
                AddCombatLogLine(line);
                statusText = scriptEvent.Message;
                Debug.Log(
                    "MC2 script signal: "
                    + scriptEvent.Signal
                    + " kind="
                    + scriptEvent.Kind
                    + " source="
                    + scriptEvent.SourceId
                    + " message="
                    + scriptEvent.Message);
            }
        }

        private void CaptureMissionResult()
        {
            if (mission.Result == lastMissionResult)
            {
                return;
            }

            lastMissionResult = mission.Result;
            if (mission.Result == MissionResultState.Victory)
            {
                ApplyMissionReceiptOnce();
                statusText = "Mission complete";
                AddCombatLogLine("Mission complete");
                Debug.Log("MC2 mission complete: " + mission.ResultReason);
                SetPaused(true);
            }
            else if (mission.Result == MissionResultState.Defeat)
            {
                ApplyMissionReceiptOnce();
                statusText = "Mission failed";
                AddCombatLogLine("Mission failed");
                Debug.Log("MC2 mission failed: " + mission.ResultReason);
                SetPaused(true);
            }
        }

        private void ApplyMissionReceiptOnce()
        {
            if (missionReceiptApplied || mission == null || demoInventory == null)
            {
                return;
            }

            missionReceipt = MechBayMissionReceiptService.ApplyMissionReceipt(demoInventory, mission.ResultSummary);
            missionReceiptApplied = true;
            RefreshDemoInventoryValidation();
            if (missionReceipt.TokenDelta > 0 || missionReceipt.SalvageFragmentCount > 0)
            {
                AddCombatLogLine(
                    "Receipt token +" + FormatTokens(missionReceipt.TokenDelta)
                    + " fragments +" + missionReceipt.SalvageFragmentCount
                    + ReceiptAssemblyLogText(missionReceipt));
            }
        }

        private void AddCombatLogLine(string line)
        {
            combatLog.Insert(0, line);
            while (combatLog.Count > 6)
            {
                combatLog.RemoveAt(combatLog.Count - 1);
            }
        }

        private void CreateGround()
        {
            if (mission.Contract.terrainMesh == null || mission.Contract.terrainMesh.samples == null)
            {
                GameObject fallbackGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
                fallbackGround.name = "Mission Ground";
                fallbackGround.transform.localScale = new Vector3(140f, 1f, 140f);
                AssignMaterial(fallbackGround, "Ground", new Color(0.16f, 0.20f, 0.16f));
                return;
            }

            GameObject terrainObject = new("Mission Terrain");
            MeshFilter meshFilter = terrainObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = terrainObject.AddComponent<MeshRenderer>();
            DemoTerrainView terrainView = terrainObject.AddComponent<DemoTerrainView>();
            terrainView.Bind(mission.Contract.terrainMesh, mission.Contract.mission.terrain.waterElevation);

            meshFilter.sharedMesh = BuildTerrainMesh(mission.Contract.terrainMesh);
            meshRenderer.sharedMaterial = MakeMaterial("GroundMesh", new Color(0.16f, 0.22f, 0.15f));

            CreateWaterPlane(mission.Contract.terrainMesh);
        }

        private Mesh BuildTerrainMesh(TerrainMeshDefinition terrain)
        {
            int side = terrain.sampleSide;
            int sampleCount = terrain.samples.Length;
            Vector3[] vertices = new Vector3[sampleCount];
            Color[] colors = new Color[sampleCount];
            Vector2[] uvs = new Vector2[sampleCount];
            float spacing = Mathf.Max(1f, terrain.worldUnitsPerVertex * Mathf.Max(1, terrain.sampleStep));

            for (int row = 0; row < side; row++)
            {
                for (int col = 0; col < side; col++)
                {
                    int index = row * side + col;
                    TerrainMeshSample sample = terrain.samples[index];
                    float missionX = terrain.minX + col * spacing;
                    float missionY = terrain.minY - row * spacing;
                    Vector3 worldPosition = DemoTerrainView.MissionToWorld(new Vector2(missionX, missionY));
                    worldPosition.y = DemoTerrainView.ElevationToWorldHeight(sample.elevation, mission.Contract.mission.terrain.waterElevation);
                    vertices[index] = worldPosition;
                    colors[index] = TerrainVertexColor(sample);
                    uvs[index] = new Vector2(col / (float)(side - 1), row / (float)(side - 1));
                }
            }

            int[] triangles = new int[(side - 1) * (side - 1) * 6];
            int triangleIndex = 0;
            for (int row = 0; row < side - 1; row++)
            {
                for (int col = 0; col < side - 1; col++)
                {
                    int topLeft = row * side + col;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + side;
                    int bottomRight = bottomLeft + 1;
                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = bottomRight;
                }
            }

            Mesh mesh = new()
            {
                name = "MC2 Source Terrain",
                vertices = vertices,
                triangles = triangles,
                colors = colors,
                uv = uvs
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private void CreateWaterPlane(TerrainMeshDefinition terrain)
        {
            GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
            water.name = "Mission Water";
            float sourceSize = (terrain.sampleSide - 1) * terrain.worldUnitsPerVertex / 1000f;
            water.transform.localScale = new Vector3(sourceSize, 1f, sourceSize);
            Vector3 center = DemoTerrainView.MissionToWorld(new Vector2(0f, 0f));
            center.y = DemoTerrainView.WaterHeight() + 0.015f;
            water.transform.position = center;
            AssignMaterial(water, "Water", new Color(0.08f, 0.24f, 0.34f, 0.52f));
        }

        private Color TerrainVertexColor(TerrainMeshSample sample)
        {
            float waterLevel = mission.Contract.mission.terrain.waterElevation;
            if (sample.elevation <= waterLevel + 4f)
            {
                return new Color(0.10f, 0.28f, 0.22f);
            }

            if (sample.terrainType == 13 || sample.terrainType == 14 || sample.terrainType == 15 || sample.terrainType == 16)
            {
                return new Color(0.32f, 0.29f, 0.22f);
            }

            float heightT = Mathf.InverseLerp(mission.Contract.terrainMesh.elevationMin, mission.Contract.terrainMesh.elevationMax, sample.elevation);
            return Color.Lerp(new Color(0.13f, 0.24f, 0.12f), new Color(0.36f, 0.36f, 0.25f), heightT);
        }

        private void CreateLights()
        {
            GameObject lightObject = new("Key Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightObject.transform.rotation = Quaternion.Euler(55f, 30f, 0f);
        }

        private void CreateCamera()
        {
            GameObject cameraObject = new("Demo Camera");
            mainCamera = cameraObject.AddComponent<Camera>();
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 42f;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.06f, 0.07f, 0.075f);
            Camera.SetupCurrent(mainCamera);
        }

        private void CreateUnits()
        {
            foreach (UnitState unit in mission.Units)
            {
                PrimitiveType primitive = unit.IsPlayerUnit ? PrimitiveType.Capsule : PrimitiveType.Cube;
                GameObject unitObject = GameObject.CreatePrimitive(primitive);
                unitObject.transform.localScale = unit.IsPlayerUnit
                    ? new Vector3(1.3f, 1.5f, 1.3f)
                    : new Vector3(1.2f, 0.7f, 1.2f);
                AssignMaterial(
                    unitObject,
                    unit.Id,
                    unit.IsPlayerUnit ? new Color(0.1f, 0.65f, 0.86f) : new Color(0.74f, 0.20f, 0.18f));

                DemoUnitView view = unitObject.AddComponent<DemoUnitView>();
                view.Bind(unit);
                unitObject.SetActive(unit.IsActive || unit.IsPlayerUnit || unit.IsDestroyed);
                unitViews[unit.Id] = view;
            }
        }

        private void CreateStaticObjects()
        {
            foreach (StructureState structure in mission.Structures)
            {
                GameObject structureObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                structureObject.name = structure.Id + " " + structure.ObjectType;
                structureObject.transform.localScale = StructureScale(structure);
                AssignMaterial(
                    structureObject,
                    structure.Id,
                    structure.IsTargetable ? new Color(0.62f, 0.48f, 0.34f) : new Color(0.32f, 0.34f, 0.34f));

                DemoStructureView view = structureObject.AddComponent<DemoStructureView>();
                view.Bind(structure);
                structureViews[structure.Id] = view;
            }
        }

        private Vector3 StructureScale(StructureState structure)
        {
            if (structure.ObjectType == "Hangar")
            {
                return new Vector3(3.8f, 1.3f, 2.4f);
            }

            float footprint = Mathf.Clamp(structure.Radius / 80f, 1.4f, 3.2f);
            return new Vector3(footprint, 1f, footprint);
        }

        private void CreateTerrainObjects()
        {
            if (mission.Contract.terrainObjects == null)
            {
                return;
            }

            foreach (TerrainObjectSpawn terrainObject in mission.Contract.terrainObjects)
            {
                if (terrainObject.position == null || IsCoveredByTargetStructure(terrainObject))
                {
                    continue;
                }

                PrimitiveType primitive = TerrainPrimitive(terrainObject);
                GameObject prop = GameObject.CreatePrimitive(primitive);
                prop.name = terrainObject.objectId + " " + terrainObject.fileName;
                Vector3 scale = TerrainObjectScale(terrainObject);
                Vector3 position = DemoUnitView.MissionToWorld(new Vector2(terrainObject.position.x, terrainObject.position.y));
                Vector2 missionPosition = new(terrainObject.position.x, terrainObject.position.y);
                position.y = DemoTerrainView.HeightAt(missionPosition) + Mathf.Max(0.03f, scale.y * 0.5f);
                prop.transform.position = position;
                prop.transform.rotation = Quaternion.Euler(0f, -terrainObject.position.rotation, 0f);
                prop.transform.localScale = scale;

                Collider collider = prop.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = false;
                }

                AssignMaterial(prop, TerrainMaterialName(terrainObject), TerrainObjectColor(terrainObject));
            }
        }

        private bool IsCoveredByTargetStructure(TerrainObjectSpawn terrainObject)
        {
            Vector2 objectPosition = new(terrainObject.position.x, terrainObject.position.y);
            foreach (StructureState structure in mission.Structures)
            {
                if (Vector2.Distance(objectPosition, structure.MissionPosition) < 40f)
                {
                    return true;
                }
            }

            return false;
        }

        private static PrimitiveType TerrainPrimitive(TerrainObjectSpawn terrainObject)
        {
            if (string.Equals(terrainObject.objectClass, "TREE", StringComparison.OrdinalIgnoreCase))
            {
                return PrimitiveType.Capsule;
            }

            if (string.Equals(terrainObject.objectClass, "BUILDING", StringComparison.OrdinalIgnoreCase))
            {
                return PrimitiveType.Cube;
            }

            return PrimitiveType.Cylinder;
        }

        private static Vector3 TerrainObjectScale(TerrainObjectSpawn terrainObject)
        {
            if (string.Equals(terrainObject.objectClass, "TREE", StringComparison.OrdinalIgnoreCase))
            {
                if (ContainsIgnoreCase(terrainObject.fileName, "Light"))
                {
                    return new Vector3(0.12f, 0.7f, 0.12f);
                }

                return new Vector3(0.36f, 1.15f, 0.36f);
            }

            if (string.Equals(terrainObject.objectClass, "BUILDING", StringComparison.OrdinalIgnoreCase))
            {
                if (terrainObject.fitId == 559 || ContainsIgnoreCase(terrainObject.fileName, "Hangar"))
                {
                    return new Vector3(3.8f, 1.2f, 2.4f);
                }

                return new Vector3(1.25f, 0.7f, 1.25f);
            }

            return new Vector3(0.45f, 0.35f, 0.45f);
        }

        private static string TerrainMaterialName(TerrainObjectSpawn terrainObject)
        {
            if (terrainObject.damage != 0)
            {
                return "TerrainDamaged";
            }

            if (string.Equals(terrainObject.objectClass, "TREE", StringComparison.OrdinalIgnoreCase))
            {
                return "TerrainTree";
            }

            if (string.Equals(terrainObject.objectClass, "BUILDING", StringComparison.OrdinalIgnoreCase))
            {
                return terrainObject.teamId >= 0 ? "TerrainTeamBuilding" : "TerrainBuilding";
            }

            return "TerrainProp";
        }

        private static Color TerrainObjectColor(TerrainObjectSpawn terrainObject)
        {
            if (terrainObject.damage != 0)
            {
                return new Color(0.30f, 0.27f, 0.22f);
            }

            if (string.Equals(terrainObject.objectClass, "TREE", StringComparison.OrdinalIgnoreCase))
            {
                if (ContainsIgnoreCase(terrainObject.fileName, "Light"))
                {
                    return new Color(0.82f, 0.73f, 0.46f);
                }

                return new Color(0.13f, 0.34f, 0.17f);
            }

            if (string.Equals(terrainObject.objectClass, "BUILDING", StringComparison.OrdinalIgnoreCase))
            {
                return terrainObject.teamId >= 0
                    ? new Color(0.55f, 0.44f, 0.33f)
                    : new Color(0.33f, 0.35f, 0.34f);
            }

            return new Color(0.42f, 0.42f, 0.38f);
        }

        private static bool ContainsIgnoreCase(string value, string fragment)
        {
            return !string.IsNullOrEmpty(value)
                && value.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void CreateMarkers()
        {
            if (mission.Contract.navMarkers == null)
            {
                return;
            }

            foreach (NavMarker marker in mission.Contract.navMarkers)
            {
                GameObject markerObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                markerObject.name = "Nav Marker " + marker.index;
                Vector3 markerPosition = DemoUnitView.MissionToWorld(new Vector2(marker.x, marker.y));
                markerPosition.y = 0.04f;
                markerObject.transform.position = markerPosition;
                markerObject.transform.localScale = new Vector3(marker.radius / 100f, 0.04f, marker.radius / 100f);
                AssignMaterial(markerObject, "Marker" + marker.index, new Color(0.95f, 0.76f, 0.22f, 0.48f));
            }
        }

        private void CreateForests()
        {
            if (mission.Contract.forests == null)
            {
                return;
            }

            foreach (ForestRegion forest in mission.Contract.forests)
            {
                if (forest.center == null || forest.radius <= 0f)
                {
                    continue;
                }

                Vector2 missionCenter = new(forest.center.x, forest.center.y);
                float worldRadius = Mathf.Max(1.8f, forest.radius / 100f);
                GameObject root = new("Forest " + forest.index + " " + forest.name);
                root.transform.position = DemoUnitView.MissionToWorld(missionCenter);

                GameObject groundPatch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                groundPatch.name = "Forest Canopy Footprint";
                groundPatch.transform.SetParent(root.transform, false);
                groundPatch.transform.localPosition = new Vector3(0f, 0.025f, 0f);
                groundPatch.transform.localScale = new Vector3(worldRadius, 0.02f, worldRadius);
                AssignMaterial(groundPatch, "ForestFootprint" + forest.index, new Color(0.09f, 0.24f, 0.12f, 0.45f));

                int treeCount = Mathf.Clamp(Mathf.RoundToInt(worldRadius * 1.4f), 8, 18);
                for (int index = 0; index < treeCount; index++)
                {
                    float angle = (forest.index * 43f + index * 137.5f) * Mathf.Deg2Rad;
                    float ring = 0.18f + (((index * 37 + forest.index * 11) % 100) / 100f) * 0.72f;
                    Vector3 localPosition = new(
                        Mathf.Cos(angle) * worldRadius * ring,
                        0f,
                        Mathf.Sin(angle) * worldRadius * ring);
                    CreateTree(root.transform, forest.index, index, localPosition);
                }
            }
        }

        private void CreateTree(Transform parent, int forestIndex, int treeIndex, Vector3 localPosition)
        {
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Tree Trunk";
            trunk.transform.SetParent(parent, false);
            trunk.transform.localPosition = localPosition + new Vector3(0f, 0.22f, 0f);
            trunk.transform.localScale = new Vector3(0.08f, 0.22f, 0.08f);
            AssignMaterial(trunk, "TreeTrunk" + forestIndex + "-" + treeIndex, new Color(0.32f, 0.22f, 0.12f));

            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = "Tree Canopy";
            canopy.transform.SetParent(parent, false);
            canopy.transform.localPosition = localPosition + new Vector3(0f, 0.56f, 0f);
            float canopyScale = 0.34f + ((treeIndex + forestIndex) % 4) * 0.04f;
            canopy.transform.localScale = new Vector3(canopyScale, canopyScale * 0.82f, canopyScale);
            AssignMaterial(canopy, "TreeCanopy" + forestIndex + "-" + treeIndex, new Color(0.12f, 0.36f, 0.17f));
        }

        private void CreateObjectiveAreas()
        {
            HashSet<string> renderedAreas = new();
            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (objective.Definition.hidden)
                {
                    continue;
                }

                if (objective.Definition.conditions == null)
                {
                    continue;
                }

                foreach (ObjectiveCondition condition in objective.Definition.conditions)
                {
                    TargetArea area = condition.targetArea;
                    if (area == null || area.radius <= 0f)
                    {
                        continue;
                    }

                    string key = objective.Definition.index + ":"
                        + Mathf.RoundToInt(area.x) + ":"
                        + Mathf.RoundToInt(area.y) + ":"
                        + Mathf.RoundToInt(area.radius);
                    if (!renderedAreas.Add(key))
                    {
                        continue;
                    }

                    GameObject disc = CreateAreaDisc(
                        "Objective Area " + objective.Definition.index,
                        new Vector2(area.x, area.y),
                        area.radius,
                        new Color(0.15f, 0.55f, 0.78f, 0.34f));
                    if (!objectiveAreaMarkers.TryGetValue(objective.Definition.index, out List<GameObject> markers))
                    {
                        markers = new List<GameObject>();
                        objectiveAreaMarkers[objective.Definition.index] = markers;
                    }

                    markers.Add(disc);
                }
            }

            UpdateObjectiveAreaVisibility();
        }

        private GameObject CreateAreaDisc(string objectName, Vector2 missionCenter, float missionRadius, Color color)
        {
            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = objectName;
            Vector3 position = DemoUnitView.MissionToWorld(missionCenter);
            position.y = 0.035f;
            disc.transform.position = position;
            float worldRadius = Mathf.Max(0.6f, missionRadius / 100f);
            disc.transform.localScale = new Vector3(worldRadius, 0.018f, worldRadius);
            AssignMaterial(disc, objectName, color);
            return disc;
        }

        private void UpdateObjectiveAreaVisibility()
        {
            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (!objectiveAreaMarkers.TryGetValue(objective.Definition.index, out List<GameObject> markers))
                {
                    continue;
                }

                bool visible = ShouldShowCurrentObjectiveHint(objective);
                foreach (GameObject marker in markers)
                {
                    if (marker != null)
                    {
                        marker.SetActive(visible);
                    }
                }
            }
        }

        private void CreateCommandOverlays()
        {
            foreach (UnitState unit in mission.PlayerUnits())
            {
                unitSelectionMarkers[unit.Id] = CreateMarkerDisc(
                    unit.Id + " Selection Ring",
                    "CommandSelection",
                    new Color(0.22f, 0.92f, 1f, 0.32f),
                    new Vector3(1.45f, 0.012f, 1.45f));
                unitOrderMarkers[unit.Id] = CreateMarkerDisc(
                    unit.Id + " Order Marker",
                    "CommandMove",
                    new Color(0.15f, 0.66f, 1f, 0.42f),
                    new Vector3(0.88f, 0.014f, 0.88f));
                unitFocusMarkers[unit.Id] = CreateMarkerDisc(
                    unit.Id + " Focus Marker",
                    "CommandFocus",
                    new Color(1f, 0.58f, 0.12f, 0.48f),
                    new Vector3(1.18f, 0.016f, 1.18f));
                unitRangeMarkers[unit.Id] = CreateMarkerDisc(
                    unit.Id + " Weapon Range",
                    "CommandWeaponRange",
                    new Color(0.25f, 0.82f, 1f, 0.12f),
                    Vector3.one);
                unitTargetLines[unit.Id] = CreateTargetLine(unit.Id + " Target Line");
            }
        }

        private GameObject CreateMarkerDisc(string objectName, string materialName, Color color, Vector3 scale)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = objectName;
            marker.transform.localScale = scale;
            Collider markerCollider = marker.GetComponent<Collider>();
            if (markerCollider != null)
            {
                markerCollider.enabled = false;
            }

            AssignMaterial(marker, materialName, color);
            marker.SetActive(false);
            return marker;
        }

        private void UpdateCommandOverlays()
        {
            if (mission == null)
            {
                return;
            }

            foreach (UnitState unit in mission.PlayerUnits())
            {
                UpdateSelectionMarker(unit);
                UpdateOrderMarker(unit);
                UpdateFocusMarker(unit);
                UpdateRangeMarker(unit);
                UpdateTargetLine(unit);
            }

            UpdateTargetHealthBars();
        }

        private void UpdateUnitVisibility()
        {
            foreach (UnitState unit in mission.Units)
            {
                if (!unitViews.TryGetValue(unit.Id, out DemoUnitView view) || view == null)
                {
                    continue;
                }

                view.gameObject.SetActive(unit.IsActive || unit.IsPlayerUnit || unit.IsDestroyed);
            }
        }

        private void UpdateSelectionMarker(UnitState unit)
        {
            if (!unitSelectionMarkers.TryGetValue(unit.Id, out GameObject marker))
            {
                return;
            }

            bool isVisible = !unit.IsDestroyed && (unit.IsDetached || pendingDetachedUnitId == unit.Id);
            marker.SetActive(isVisible);
            if (isVisible)
            {
                marker.transform.position = GroundMarkerPosition(unit.MissionPosition, 0.06f);
            }
        }

        private void UpdateOrderMarker(UnitState unit)
        {
            if (!unitOrderMarkers.TryGetValue(unit.Id, out GameObject marker))
            {
                return;
            }

            bool isVisible = !unit.IsDestroyed && (unit.HasMoveOrder || unit.IsJumping || unit.HasAttackOrder);
            marker.SetActive(isVisible);
            if (isVisible)
            {
                marker.transform.position = GroundMarkerPosition(unit.MoveTarget, 0.08f);
            }
        }

        private void UpdateFocusMarker(UnitState unit)
        {
            if (!unitFocusMarkers.TryGetValue(unit.Id, out GameObject marker))
            {
                return;
            }

            Vector2 position = default;
            float radius = 0f;
            bool isVisible = !unit.IsDestroyed
                && unit.HasAttackOrder
                && TryGetTargetMarker(unit.AttackTargetId, out position, out radius);
            marker.SetActive(isVisible);
            if (isVisible)
            {
                marker.transform.position = GroundMarkerPosition(position, 0.1f);
                float scale = Mathf.Clamp(radius / 95f, 1.1f, 4.2f);
                marker.transform.localScale = new Vector3(scale, 0.016f, scale);
            }
        }

        private void UpdateRangeMarker(UnitState unit)
        {
            if (!unitRangeMarkers.TryGetValue(unit.Id, out GameObject marker))
            {
                return;
            }

            bool isVisible = !unit.IsDestroyed
                && unit.CombatWeaponRange > 0f
                && (pendingDetachedUnitId == unit.Id
                    || unit.IsDetached
                    || unit.HasAttackOrder
                    || !string.IsNullOrEmpty(unit.CurrentTargetId));
            marker.SetActive(isVisible);
            if (isVisible)
            {
                marker.transform.position = GroundMarkerPosition(unit.MissionPosition, 0.045f);
                float worldDiameter = Mathf.Clamp((unit.CombatWeaponRange / 100f) * 2f, 1.2f, 8f);
                marker.transform.localScale = new Vector3(worldDiameter, 0.01f, worldDiameter);
            }
        }

        private void UpdateTargetLine(UnitState unit)
        {
            if (!unitTargetLines.TryGetValue(unit.Id, out GameObject line))
            {
                return;
            }

            string targetId = string.IsNullOrEmpty(unit.AttackTargetId) ? unit.CurrentTargetId : unit.AttackTargetId;
            bool hasTargetId = !string.IsNullOrEmpty(targetId);
            Vector3 targetPoint = Vector3.zero;
            bool hasTargetPoint = hasTargetId && TryGetTargetWorldPoint(targetId, out targetPoint);
            bool hasUnitView = unitViews.TryGetValue(unit.Id, out DemoUnitView unitView) && unitView != null;
            bool isVisible = !unit.IsDestroyed
                && hasTargetId
                && hasTargetPoint
                && hasUnitView;
            line.SetActive(isVisible);
            if (!isVisible)
            {
                return;
            }

            Vector3 unitPoint = unitView.transform.position + Vector3.up * 0.18f;
            targetPoint += Vector3.up * 0.18f;
            PositionLine(line, unitPoint, targetPoint, 0.028f);
            AssignMaterial(line, TargetLineMaterialName(unit, targetId), TargetLineColor(unit, targetId));
        }

        private GameObject CreateTargetLine(string objectName)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            line.name = objectName;
            Collider lineCollider = line.GetComponent<Collider>();
            if (lineCollider != null)
            {
                lineCollider.enabled = false;
            }

            AssignMaterial(line, "TargetLineReady", new Color(0.28f, 0.78f, 1f, 0.48f));
            line.SetActive(false);
            return line;
        }

        private void PositionLine(GameObject line, Vector3 from, Vector3 to, float radius)
        {
            Vector3 direction = to - from;
            float length = direction.magnitude;
            if (length <= 0.01f)
            {
                line.SetActive(false);
                return;
            }

            line.transform.position = (from + to) * 0.5f;
            line.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            line.transform.localScale = new Vector3(radius, length * 0.5f, radius);
        }

        private bool TryGetTargetWorldPoint(string targetId, out Vector3 point)
        {
            if (unitViews.TryGetValue(targetId, out DemoUnitView unitView)
                && unitView != null
                && unitView.Unit != null
                && unitView.Unit.IsActive
                && !unitView.Unit.IsDestroyed)
            {
                point = unitView.transform.position;
                return true;
            }

            if (structureViews.TryGetValue(targetId, out DemoStructureView structureView) && structureView != null && structureView.Structure != null && !structureView.Structure.IsDestroyed)
            {
                point = structureView.transform.position;
                return true;
            }

            point = Vector3.zero;
            return false;
        }

        private string TargetLineMaterialName(UnitState unit, string targetId)
        {
            if (unit.IsHeatLocked || !IsTargetInWeaponRange(unit, targetId))
            {
                return "TargetLineBlocked";
            }

            if (unit.IsWeaponCoolingDown)
            {
                return "TargetLineCooling";
            }

            return "TargetLineReady";
        }

        private Color TargetLineColor(UnitState unit, string targetId)
        {
            if (unit.IsHeatLocked || !IsTargetInWeaponRange(unit, targetId))
            {
                return new Color(1f, 0.18f, 0.12f, 0.50f);
            }

            if (unit.IsWeaponCoolingDown)
            {
                return new Color(1f, 0.62f, 0.14f, 0.46f);
            }

            return new Color(0.28f, 0.78f, 1f, 0.48f);
        }

        private bool IsTargetInWeaponRange(UnitState unit, string targetId)
        {
            UnitState targetUnit = mission.FindUnit(targetId);
            if (targetUnit != null)
            {
                return unit.IsInWeaponRange(targetUnit);
            }

            StructureState targetStructure = mission.FindStructure(targetId);
            return targetStructure != null && unit.IsInWeaponRange(targetStructure);
        }

        private void UpdateTargetHealthBars()
        {
            foreach (UnitState unit in mission.Units)
            {
                if (unit.IsPlayerUnit)
                {
                    continue;
                }

                EnsureUnitHealthBar(unit);
                bool isVisible = unit.IsActive && !unit.IsDestroyed && (unit.Structure < 0.995f || IsPlayerTargeting(unit.Id));
                UpdateHealthBar(
                    unitHealthBarBacks[unit.Id],
                    unitHealthBarFills[unit.Id],
                    isVisible,
                    UnitHealthBarPosition(unit),
                    1.35f,
                    unit.Structure);
            }

            foreach (StructureState structure in mission.Structures)
            {
                if (!structure.IsTargetable)
                {
                    continue;
                }

                EnsureStructureHealthBar(structure);
                bool isVisible = !structure.IsDestroyed && (structure.Structure < 0.995f || IsPlayerTargeting(structure.Id));
                UpdateHealthBar(
                    structureHealthBarBacks[structure.Id],
                    structureHealthBarFills[structure.Id],
                    isVisible,
                    StructureHealthBarPosition(structure),
                    Mathf.Clamp(structure.Radius / 70f, 1.65f, 3.3f),
                    structure.Structure);
            }
        }

        private void EnsureUnitHealthBar(UnitState unit)
        {
            if (unitHealthBarBacks.ContainsKey(unit.Id))
            {
                return;
            }

            unitHealthBarBacks[unit.Id] = CreateHealthBarPart(unit.Id + " Health Back", "TargetHealthBack", new Color(0.06f, 0.06f, 0.06f, 0.82f));
            unitHealthBarFills[unit.Id] = CreateHealthBarPart(unit.Id + " Health Fill", "TargetHealthFill", new Color(0.24f, 0.88f, 0.28f, 0.92f));
        }

        private void EnsureStructureHealthBar(StructureState structure)
        {
            if (structureHealthBarBacks.ContainsKey(structure.Id))
            {
                return;
            }

            structureHealthBarBacks[structure.Id] = CreateHealthBarPart(structure.Id + " Health Back", "TargetHealthBack", new Color(0.06f, 0.06f, 0.06f, 0.82f));
            structureHealthBarFills[structure.Id] = CreateHealthBarPart(structure.Id + " Health Fill", "TargetHealthFill", new Color(0.24f, 0.88f, 0.28f, 0.92f));
        }

        private GameObject CreateHealthBarPart(string objectName, string materialName, Color color)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = objectName;
            Collider partCollider = part.GetComponent<Collider>();
            if (partCollider != null)
            {
                partCollider.enabled = false;
            }

            AssignMaterial(part, materialName, color);
            part.SetActive(false);
            return part;
        }

        private void UpdateHealthBar(GameObject back, GameObject fill, bool isVisible, Vector3 center, float width, float ratio)
        {
            back.SetActive(isVisible);
            fill.SetActive(isVisible);
            if (!isVisible)
            {
                return;
            }

            float clampedRatio = Mathf.Clamp01(ratio);
            back.transform.position = center;
            back.transform.localScale = new Vector3(width, 0.055f, 0.12f);

            float fillWidth = Mathf.Max(0.03f, width * clampedRatio);
            fill.transform.position = center + Vector3.right * ((fillWidth - width) * 0.5f);
            fill.transform.localScale = new Vector3(fillWidth, 0.062f, 0.13f);
            AssignMaterial(fill, HealthFillMaterialName(clampedRatio), HealthFillColor(clampedRatio));
        }

        private Vector3 UnitHealthBarPosition(UnitState unit)
        {
            if (unitViews.TryGetValue(unit.Id, out DemoUnitView view) && view != null)
            {
                return view.transform.position + Vector3.up * 1.15f;
            }

            return GroundMarkerPosition(unit.MissionPosition, 1.15f);
        }

        private Vector3 StructureHealthBarPosition(StructureState structure)
        {
            if (structureViews.TryGetValue(structure.Id, out DemoStructureView view) && view != null)
            {
                return view.transform.position + Vector3.up * 1.05f;
            }

            return GroundMarkerPosition(structure.MissionPosition, 1.05f);
        }

        private bool IsPlayerTargeting(string targetId)
        {
            foreach (UnitState unit in mission.PlayerUnits())
            {
                if (unit.AttackTargetId == targetId || unit.CurrentTargetId == targetId)
                {
                    return true;
                }
            }

            return false;
        }

        private static string HealthFillMaterialName(float ratio)
        {
            if (ratio <= 0.33f)
            {
                return "TargetHealthCritical";
            }

            if (ratio <= 0.66f)
            {
                return "TargetHealthDamaged";
            }

            return "TargetHealthFill";
        }

        private static Color HealthFillColor(float ratio)
        {
            if (ratio <= 0.33f)
            {
                return new Color(0.95f, 0.16f, 0.12f, 0.94f);
            }

            if (ratio <= 0.66f)
            {
                return new Color(1f, 0.72f, 0.14f, 0.94f);
            }

            return new Color(0.24f, 0.88f, 0.28f, 0.92f);
        }

        private bool TryGetTargetMarker(string targetId, out Vector2 missionPosition, out float radius)
        {
            UnitState unit = mission.FindUnit(targetId);
            if (unit != null && !unit.IsDestroyed)
            {
                missionPosition = unit.MissionPosition;
                radius = 115f;
                return true;
            }

            StructureState structure = mission.FindStructure(targetId);
            if (structure != null && !structure.IsDestroyed)
            {
                missionPosition = structure.MissionPosition;
                radius = structure.Radius;
                return true;
            }

            missionPosition = default;
            radius = 0f;
            return false;
        }

        private static Vector3 GroundMarkerPosition(Vector2 missionPoint, float lift)
        {
            Vector3 position = DemoUnitView.MissionToWorld(missionPoint);
            position.y = DemoTerrainView.HeightAt(missionPoint) + lift;
            return position;
        }

        private void AssignMaterial(GameObject target, string materialName, Color color)
        {
            Material material = MakeMaterial(materialName, color);
            if (material == null)
            {
                return;
            }

            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private Material MakeMaterial(string materialName, Color color)
        {
            string cacheKey = materialName + ":" + ColorUtility.ToHtmlStringRGBA(color);
            if (materialCache.TryGetValue(cacheKey, out Material cachedMaterial))
            {
                return cachedMaterial;
            }

            Shader shader = Shader.Find("Standard") ?? Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
            {
                Debug.LogWarning("No shader available for material " + materialName + "; skipping custom material.");
                return null;
            }

            Material material = new(shader)
            {
                name = materialName,
                color = color
            };
            if (color.a < 0.99f)
            {
                material.SetFloat("_Mode", 3f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }

            ownedMaterials.Add(material);
            materialCache[cacheKey] = material;
            return material;
        }

        private void HandleWorldClick()
        {
            if (!Input.GetMouseButtonDown(0) || mainCamera == null)
            {
                return;
            }

            Vector2 guiPoint = new(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            if (IsGuiPointBlocked(guiPoint))
            {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 500f))
            {
                DemoUnitView unitView = hit.collider.GetComponentInParent<DemoUnitView>();
                if (unitView != null && unitView.Unit != null && unitView.Unit.IsActive && !unitView.Unit.IsPlayerUnit && !unitView.Unit.IsDestroyed)
                {
                    if (pendingJumpOrder)
                    {
                        IssueMoveOrder(unitView.Unit.MissionPosition, "Jet toward " + unitView.Unit.UnitType);
                    }
                    else
                    {
                        IssueUnitAttackOrder(unitView.Unit);
                    }

                    return;
                }

                DemoStructureView structureView = hit.collider.GetComponentInParent<DemoStructureView>();
                if (structureView != null && structureView.Structure != null && !structureView.Structure.IsDestroyed)
                {
                    if (pendingJumpOrder)
                    {
                        IssueMoveOrder(structureView.Structure.MissionPosition, "Jet toward " + structureView.Structure.ObjectType);
                    }
                    else
                    {
                        IssueStructureAttackOrder(structureView.Structure);
                    }

                    return;
                }
            }

            Plane groundPlane = new(Vector3.up, Vector3.zero);
            if (!groundPlane.Raycast(ray, out float distance))
            {
                return;
            }

            Vector2 target = DemoUnitView.WorldToMission(ray.GetPoint(distance));
            IssueMoveOrder(target, null);
        }

        private void IssueMoveOrder(Vector2 target, string label)
        {
            if (pendingJumpOrder)
            {
                IssueJumpOrder(target, label);
                return;
            }

            if (!string.IsNullOrEmpty(pendingDetachedUnitId))
            {
                CommanderCommandResult result = commandPort.MoveUnit(pendingDetachedUnitId, target);
                statusText = result.Accepted
                    ? (string.IsNullOrEmpty(label) ? "Detached order: " + pendingDetachedUnitId : label + " with " + pendingDetachedUnitId)
                    : result.Message;
                pendingDetachedUnitId = null;
            }
            else
            {
                CommanderCommandResult result = commandPort.MoveSquad(target);
                statusText = result.Accepted
                    ? (string.IsNullOrEmpty(label) ? "Squad move" : label)
                    : result.Message;
            }
        }

        private void IssueUnitAttackOrder(UnitState target)
        {
            CommanderCommandResult result;
            string detachedUnitId = pendingDetachedUnitId;
            if (!string.IsNullOrEmpty(detachedUnitId))
            {
                result = commandPort.AttackUnit(detachedUnitId, target.Id);
                statusText = result.Accepted ? "Focus " + target.UnitType + " with " + detachedUnitId : result.Message;
            }
            else
            {
                result = commandPort.AttackUnit(null, target.Id);
                statusText = result.Accepted ? "Squad focus: " + target.UnitType : result.Message;
            }

            pendingDetachedUnitId = null;
        }

        private void IssueStructureAttackOrder(StructureState target)
        {
            CommanderCommandResult result;
            string detachedUnitId = pendingDetachedUnitId;
            if (!string.IsNullOrEmpty(detachedUnitId))
            {
                result = commandPort.AttackStructure(detachedUnitId, target.Id);
                statusText = result.Accepted ? "Attack " + target.ObjectType + " with " + detachedUnitId : result.Message;
            }
            else
            {
                result = commandPort.AttackStructure(null, target.Id);
                statusText = result.Accepted ? "Squad attack: " + target.ObjectType : result.Message;
            }

            pendingDetachedUnitId = null;
        }

        private void IssueJumpOrder(Vector2 target, string label)
        {
            CommanderCommandResult result;
            string detachedUnitId = pendingDetachedUnitId;
            if (!string.IsNullOrEmpty(detachedUnitId))
            {
                result = commandPort.JumpUnit(detachedUnitId, target);
                statusText = result.Accepted
                    ? (string.IsNullOrEmpty(label) ? "Jet order: " + detachedUnitId : "Jet vector: " + detachedUnitId)
                    : result.Message;
            }
            else
            {
                result = commandPort.JumpSquad(target);
                statusText = result.Accepted
                    ? (string.IsNullOrEmpty(label) ? "Squad jet: " + result.AcceptedCount : "Squad jet vector: " + result.AcceptedCount)
                    : result.Message;
            }

            pendingDetachedUnitId = null;
            pendingJumpOrder = false;
        }

        private void FollowCommander()
        {
            UnitState commander = null;
            foreach (UnitState unit in mission.PlayerUnits())
            {
                commander = unit;
                break;
            }

            if (commander == null || mainCamera == null)
            {
                return;
            }

            Vector3 commanderWorld = DemoUnitView.MissionToWorld(commander.MissionPosition);
            Quaternion rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
            Vector3 offset = rotation * new Vector3(0f, 0f, -cameraHeight);
            mainCamera.transform.SetPositionAndRotation(commanderWorld + offset, rotation);
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(12, 12, 330, 34), statusText);

            if (mission == null)
            {
                return;
            }

            DrawUnitPanel();
            DrawCombatPanel();
            DrawMissionBriefPanel();
            DrawMissionMap();
            DrawLoadoutPanel();
            DrawSystemPanel();
            DrawMissionResultPanel();
        }

        private void DrawUnitPanel()
        {
            float y = 54f;
            GUI.Label(new Rect(18, y, 320, 22), "Lance");
            y += 24f;

            if (GUI.Button(new Rect(18, y, 54, 28), "All"))
            {
                pendingDetachedUnitId = null;
                pendingJumpOrder = false;
                statusText = "Squad selected";
            }

            if (GUI.Button(new Rect(78, y, 54, 28), pendingJumpOrder ? "Jet..." : "Jet"))
            {
                pendingJumpOrder = true;
                statusText = string.IsNullOrEmpty(pendingDetachedUnitId)
                    ? "Select squad jet destination"
                    : "Select jet destination for " + pendingDetachedUnitId;
            }

            if (GUI.Button(new Rect(138, y, 54, 28), showMissionMap ? "Map-" : "Map"))
            {
                showMissionMap = !showMissionMap;
                showLoadoutPanel = false;
                showWarehouseDraftFitPreview = false;
                showSquadSelectionPreview = false;
                warehouseDraftFitPreviewMechId = null;
                ClearSquadSelectionDraft();
                ClearSquadSelectionCompletedReplacement();
                showSystemPanel = false;
                if (mission.Result == MissionResultState.InProgress)
                {
                    SetPaused(false);
                }

                statusText = showMissionMap ? "Mission map open" : "Mission map closed";
            }

            if (GUI.Button(new Rect(198, y, 54, 28), showLoadoutPanel ? "Bay-" : "Bay"))
            {
                if (showLoadoutPanel)
                {
                    showLoadoutPanel = false;
                    showWarehouseDraftFitPreview = false;
                    showSquadSelectionPreview = false;
                    warehouseDraftFitPreviewMechId = null;
                    ClearSquadSelectionDraft();
                    ClearSquadSelectionCompletedReplacement();
                    if (mission.Result == MissionResultState.InProgress)
                    {
                        SetPaused(false);
                    }

                    statusText = "Mech bay closed";
                }
                else
                {
                    OpenLoadoutPanel();
                }
            }

            if (GUI.Button(new Rect(258, y, 64, 28), "System"))
            {
                OpenSystemPanel();
            }

            y += 36f;

            foreach (UnitState unit in mission.PlayerUnits())
            {
                string label = unit.UnitType;
                if (unit.IsDestroyed)
                {
                    label += "  DESTROYED";
                }
                else if (unit.IsJumping)
                {
                    label += "  JET";
                }
                else if (unit.IsDetached)
                {
                    label += "  DETACHED";
                }
                else if (unit.HasAttackOrder)
                {
                    label += "  TARGET";
                }
                else if (unit.HasMoveOrder)
                {
                    label += "  MOVING";
                }
                else if (!string.IsNullOrEmpty(unit.CurrentTargetId))
                {
                    label += "  FIRING";
                }

                if (unit.IsHeatLocked)
                {
                    label += "  HOT";
                }

                if (!unit.IsDestroyed && (unit.MobilityRatio < 0.99f || unit.FirepowerRatio < 0.99f))
                {
                    label += "  M" + Mathf.RoundToInt(unit.MobilityRatio * 100f) + "/F" + Mathf.RoundToInt(unit.FirepowerRatio * 100f);
                }

                if (GUI.Button(new Rect(18, y, 304, 34), label))
                {
                    pendingDetachedUnitId = unit.Id;
                    statusText = pendingJumpOrder
                        ? "Select jet destination for " + unit.UnitType
                        : "Select destination for " + unit.UnitType;
                }

                Rect barBack = new(24, y + 24, 292, 4);
                GUI.DrawTexture(barBack, Texture2D.grayTexture);
                Rect bar = new(barBack.x, barBack.y, barBack.width * unit.Structure, barBack.height);
                DrawColorRect(bar, unit.IsDestroyed ? Color.red : Color.green);

                if (unit.CombatHeatPerShot > 0f)
                {
                    Rect heatBack = new(24, y + 31, 292, 4);
                    GUI.DrawTexture(heatBack, Texture2D.grayTexture);
                    Rect heatBar = new(heatBack.x, heatBack.y, heatBack.width * Mathf.Clamp01(unit.HeatRatio), heatBack.height);
                    DrawColorRect(heatBar, unit.IsHeatLocked ? Color.red : new Color(1f, 0.62f, 0.12f));
                }

                Rect readyBack = new(24, y + 38, 292, 3);
                GUI.DrawTexture(readyBack, Texture2D.grayTexture);
                Rect readyBar = new(readyBack.x, readyBack.y, readyBack.width * unit.WeaponReadinessRatio, readyBack.height);
                DrawColorRect(readyBar, unit.IsHeatLocked ? Color.red : new Color(0.24f, 0.72f, 1f));

                GUI.Label(new Rect(24, y + 42, 300, 16), WeaponStatusText(unit));
                DrawSectionLine(unit, y + 58);
                y += 78f;
            }
        }

        private string WeaponStatusText(UnitState unit)
        {
            string weapon = ShortWeaponName(unit.CombatPrimaryWeaponName);
            string state = "Ready";
            if (unit.IsDestroyed)
            {
                state = "Offline";
            }
            else if (unit.IsHeatLocked)
            {
                state = "Hot";
            }
            else if (unit.IsWeaponCoolingDown)
            {
                state = "CD " + Mathf.RoundToInt(unit.WeaponReadinessRatio * 100f) + "%";
            }
            else if (HasAttackTargetOutOfRange(unit))
            {
                state = "Out of range";
            }

            return weapon + "  R" + Mathf.RoundToInt(unit.CombatWeaponRange) + "  " + state + CombatBonusText(unit);
        }

        private string CombatBonusText(UnitState unit)
        {
            if (unit == null || !unit.HasAppliedDemoLoadout)
            {
                return "";
            }

            string text = "";
            if (unit.CombatArmorHardnessBonus > 0.01f)
            {
                text += " A+" + FormatDecimal(unit.CombatArmorHardnessBonus);
            }

            float coolingBonus = unit.CombatHeatDissipationPerSecond - unit.Profile.HeatDissipationPerSecond;
            if (coolingBonus > 0.01f)
            {
                text += " C+" + FormatDecimal(coolingBonus);
            }

            return text;
        }

        private string ShortWeaponName(string weaponName)
        {
            if (string.IsNullOrWhiteSpace(weaponName))
            {
                return "Weapon";
            }

            return weaponName.Length <= 20 ? weaponName : weaponName.Substring(0, 20);
        }

        private bool HasAttackTargetOutOfRange(UnitState unit)
        {
            if (unit == null || string.IsNullOrEmpty(unit.AttackTargetId))
            {
                return false;
            }

            UnitState targetUnit = mission.FindUnit(unit.AttackTargetId);
            if (targetUnit != null)
            {
                return !unit.IsInWeaponRange(targetUnit);
            }

            StructureState targetStructure = mission.FindStructure(unit.AttackTargetId);
            return targetStructure != null && !unit.IsInWeaponRange(targetStructure);
        }

        private void DrawObjectivePanel()
        {
            float y = 350f;
            GUI.Label(new Rect(18, y, 320, 22), "Objectives");
            y += 24f;

            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (objective.Definition.hidden)
                {
                    continue;
                }

                string state = objective.IsComplete ? "[done] " : objective.IsActive ? "[active] " : "[locked] ";
                GUI.Label(new Rect(18, y, 320, 24), state + objective.Definition.title);
                y += 24f;
            }
        }

        private void DrawStructurePanel()
        {
            if (mission.Structures.Count == 0)
            {
                return;
            }

            float y = 522f;
            GUI.Label(new Rect(18, y, 320, 22), "Targets");
            y += 24f;

            foreach (StructureState structure in mission.Structures)
            {
                string label = structure.ObjectType;
                if (structure.IsDestroyed)
                {
                    label += "  DESTROYED";
                }

                GUI.Label(new Rect(18, y, 304, 20), label);
                Rect barBack = new(24, y + 20, 292, 5);
                GUI.DrawTexture(barBack, Texture2D.grayTexture);
                Rect bar = new(barBack.x, barBack.y, barBack.width * structure.Structure, barBack.height);
                DrawColorRect(bar, structure.IsDestroyed ? Color.red : new Color(0.95f, 0.62f, 0.18f));
                y += 34f;
            }
        }

        private void DrawMissionBriefPanel()
        {
            if (!ShouldDrawMissionBriefPanel())
            {
                return;
            }

            Rect panel = MissionBriefPanelRect();
            GUI.Box(panel, "Mission");
            float x = panel.x + 12f;
            float y = panel.y + 32f;
            float width = panel.width - 24f;
            int visibleObjectives = CountVisibleObjectives();
            int completedObjectives = CountCompletedVisibleObjectives();
            int liveStructures = CountLiveStructures();
            GUI.Label(
                new Rect(x, y, width, 20f),
                "Objectives " + completedObjectives + "/" + visibleObjectives + "    Targets " + liveStructures + "/" + mission.Structures.Count);
            y += 24f;
            MissionResultSummary summary = mission.ResultSummary;
            GUI.Label(
                new Rect(x, y, width, 20f),
                "Bounty " + FormatTokens(summary.completedRewardResourcePoints) + " / " + FormatTokens(summary.visibleRewardResourcePoints));
            y += 22f;

            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (objective.Definition.hidden)
                {
                    continue;
                }

                if (y > panel.yMax - 54f)
                {
                    GUI.Label(new Rect(x, y, width, 20f), "...");
                    y += 20f;
                    break;
                }

                Color previous = GUI.color;
                GUI.color = objective.IsComplete
                    ? new Color(0.55f, 1f, 0.55f)
                    : objective.IsActive ? new Color(1f, 0.88f, 0.42f) : new Color(0.62f, 0.68f, 0.72f);
                GUI.Label(new Rect(x, y, width, 20f), ObjectiveStateLabel(objective) + TruncateText(objective.Definition.title, 36));
                GUI.color = previous;
                y += 20f;
            }

            if (mission.Structures.Count == 0 || y > panel.yMax - 42f)
            {
                return;
            }

            y += 4f;
            GUI.Label(new Rect(x, y, width, 18f), "Target structures");
            y += 20f;
            foreach (StructureState structure in mission.Structures)
            {
                if (y > panel.yMax - 22f)
                {
                    break;
                }

                DrawStructureBrief(structure, x, y, width);
                y += 28f;
            }
        }

        private bool ShouldDrawMissionBriefPanel()
        {
            return !showMissionMap && mission.Result == MissionResultState.InProgress;
        }

        private static string ObjectiveStateLabel(ObjectiveState objective)
        {
            if (objective.IsComplete)
            {
                return "[done] ";
            }

            return objective.IsActive ? "[active] " : "[locked] ";
        }

        private void DrawStructureBrief(StructureState structure, float x, float y, float width)
        {
            string label = TruncateText(structure.ObjectType, 32);
            if (structure.IsDestroyed)
            {
                label += " destroyed";
            }

            GUI.Label(new Rect(x, y, width, 18f), label);
            Rect back = new(x, y + 18f, width, 5f);
            GUI.DrawTexture(back, Texture2D.grayTexture);
            DrawColorRect(
                new Rect(back.x, back.y, back.width * Mathf.Clamp01(structure.Structure), back.height),
                structure.IsDestroyed ? Color.red : new Color(0.95f, 0.62f, 0.18f));
        }

        private int CountVisibleObjectives()
        {
            int count = 0;
            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (!objective.Definition.hidden)
                {
                    count++;
                }
            }

            return count;
        }

        private int CountCompletedVisibleObjectives()
        {
            int count = 0;
            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (!objective.Definition.hidden && objective.IsComplete)
                {
                    count++;
                }
            }

            return count;
        }

        private int CountLiveStructures()
        {
            int count = 0;
            foreach (StructureState structure in mission.Structures)
            {
                if (!structure.IsDestroyed)
                {
                    count++;
                }
            }

            return count;
        }

        private void DrawCombatPanel()
        {
            Rect panel = CombatPanelRect();
            float x = panel.x;
            GUI.Box(panel, "Combat");
            GUI.Label(new Rect(x + 12, 38, 320, 22), "Active units: " + CountLiveUnits() + " / " + mission.Units.Count);
            float y = 64f;
            foreach (string line in combatLog)
            {
                GUI.Label(new Rect(x + 12, y, 320, 20), line);
                y += 20f;
            }
        }

        private void DrawMissionMap()
        {
            if (!showMissionMap)
            {
                return;
            }

            Rect panel = MissionMapRect();
            GUI.Box(panel, "Mission Map");
            Rect map = new(panel.x + 14f, panel.y + 34f, panel.width - 28f, panel.height - 58f);
            DrawColorRect(map, new Color(0.035f, 0.045f, 0.048f, 0.92f));
            DrawColorRect(new Rect(map.x, map.y, map.width, 1f), new Color(0.35f, 0.42f, 0.44f));
            DrawColorRect(new Rect(map.x, map.yMax - 1f, map.width, 1f), new Color(0.35f, 0.42f, 0.44f));
            DrawColorRect(new Rect(map.x, map.y, 1f, map.height), new Color(0.35f, 0.42f, 0.44f));
            DrawColorRect(new Rect(map.xMax - 1f, map.y, 1f, map.height), new Color(0.35f, 0.42f, 0.44f));

            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (!ShouldShowCurrentObjectiveHint(objective))
                {
                    continue;
                }

                DrawObjectiveMapMarker(map, objective);
            }

            if (GUI.Button(new Rect(panel.xMax - 66f, panel.y + 6f, 52f, 24f), "Close"))
            {
                showMissionMap = false;
                statusText = "Mission map closed";
            }
        }

        private static bool ShouldShowCurrentObjectiveHint(ObjectiveState objective)
        {
            return objective != null
                && !objective.Definition.hidden
                && objective.IsActive
                && !objective.IsComplete;
        }

        private void DrawObjectiveMapMarker(Rect map, ObjectiveState objective)
        {
            if (objective.Definition.conditions == null)
            {
                return;
            }

            foreach (ObjectiveCondition condition in objective.Definition.conditions)
            {
                if (condition == null)
                {
                    continue;
                }

                if (condition.targetArea != null)
                {
                    Vector2 center = new(condition.targetArea.x, condition.targetArea.y);
                    DrawMapMarker(map, center, objective.IsComplete ? Color.green : Color.yellow, objective.IsActive ? 9f : 6f);
                    continue;
                }

                if (condition.targetUnit?.position != null)
                {
                    DrawMapMarker(
                        map,
                        new Vector2(condition.targetUnit.position.x, condition.targetUnit.position.y),
                        objective.IsComplete ? Color.green : Color.yellow,
                        9f);
                }

                if (condition.targetStructure?.position != null)
                {
                    DrawMapMarker(
                        map,
                        new Vector2(condition.targetStructure.position.x, condition.targetStructure.position.y),
                        objective.IsComplete ? Color.green : new Color(1f, 0.72f, 0.15f),
                        10f);
                }
            }
        }

        private void DrawLoadoutPanel()
        {
            if (!showLoadoutPanel)
            {
                return;
            }

            Rect panel = LoadoutPanelRect();
            GUI.Box(panel, "Mech Bay");
            float x = panel.x + 14f;
            float y = panel.y + 34f;
            float width = panel.width - 28f;

            GUI.Label(new Rect(x, y, width - 76f, 22f), "Squad loadout");
            if (GUI.Button(new Rect(panel.xMax - 66f, panel.y + 6f, 52f, 24f), "Close"))
            {
                showLoadoutPanel = false;
                showWarehouseDraftFitPreview = false;
                showSquadSelectionPreview = false;
                warehouseDraftFitPreviewMechId = null;
                ClearSquadSelectionDraft();
                ClearSquadSelectionCompletedReplacement();
                if (mission.Result == MissionResultState.InProgress)
                {
                    SetPaused(false);
                }

                statusText = "Mech bay closed";
            }

            y += 30f;
            bool showInlineMechBayPreview = showWarehouseDraftFitPreview || showSquadSelectionPreview;
            DrawMechBayInventorySummary(x, y, width, !showInlineMechBayPreview);
            y += showInlineMechBayPreview ? 260f : 456f;
            if (showWarehouseDraftFitPreview)
            {
                DrawWarehouseDraftFitPreview(x, y, width);
                y += 116f;
            }
            else if (showSquadSelectionPreview)
            {
                DrawSquadSelectionPreview(x, y, width);
                y += 250f;
            }

            int unitCount = CountPlayerUnits();
            float viewportHeight = Mathf.Max(40f, panel.yMax - y - 12f);
            Rect viewport = new(x, y, width, viewportHeight);
            Rect content = new(0f, 0f, width - 20f, Mathf.Max(viewport.height, unitCount * LoadoutCardStride));
            loadoutScroll = GUI.BeginScrollView(viewport, loadoutScroll, content);
            float itemY = 0f;
            foreach (UnitState unit in mission.PlayerUnits())
            {
                DrawLoadoutUnit(unit, 0f, itemY, content.width);
                itemY += LoadoutCardStride;
            }

            GUI.EndScrollView();
        }

        private void DrawMechBayInventorySummary(float x, float y, float width, bool showRosterDetail)
        {
            if (demoInventoryValidation == null)
            {
                GUI.Label(new Rect(x, y, width, 18f), "Inventory loading");
                return;
            }

            MechBayInventorySummary summary = demoInventoryValidation.Summary;
            MechBayInventoryAvailabilityResult availability = CurrentDraftInventoryAvailability();
            bool inventoryReady = demoInventoryValidation.IsValid && availability != null && availability.IsValid;
            Color previous = GUI.color;
            GUI.color = inventoryReady ? new Color(0.58f, 0.82f, 1f, 1f) : new Color(1f, 0.78f, 0.28f, 1f);
            GUI.Label(
                new Rect(x, y, width * 0.46f, 18f),
                inventoryReady
                    ? "Inventory OK"
                    : "Inventory Review: " + TruncateText(InventoryStatusError(availability), 28));
            GUI.color = previous;

            GUI.Label(
                new Rect(x + width * 0.48f, y, width * 0.50f, 18f),
                "Token " + summary.TokenBalance.ToString(CultureInfo.InvariantCulture));
            GUI.Label(
                new Rect(x, y + 20f, width, 18f),
                "Mechs " + summary.MechCount.ToString(CultureInfo.InvariantCulture)
                + "  Weapons " + summary.WeaponCount.ToString(CultureInfo.InvariantCulture)
                + "  Armor " + InventoryUseText(availability?.Usage?.ArmorPlateCount ?? 0, availability?.AvailableArmorPlateCount ?? summary.ArmorPlateCount)
                + "  Sinks " + InventoryUseText(availability?.Usage?.HeatSinkCount ?? 0, availability?.AvailableHeatSinkCount ?? summary.HeatSinkCount)
                + "  Frags " + summary.MechFragmentCount.ToString(CultureInfo.InvariantCulture));
            GUI.Label(
                new Rect(x, y + 40f, width, 18f),
                "Assembly " + AssemblyPreviewText(MechBayAssemblyPreviewService.BestAssemblyProgress(demoInventory)));
            DrawLocalCandidatePrepAction(x, y + 62f, width);
            MechBayWeaponShopPreview shopPreview = MechBayWeaponShopPreviewService.BuildPreview(demoInventory);
            GUI.Label(
                new Rect(x, y + 82f, width, 18f),
                "Shop " + WeaponShopPreviewText(shopPreview));
            DrawWeaponShopPurchaseStub(x, y + 104f, width, shopPreview, demoInventory);
            MechBayPilotHirePreview pilotHirePreview = MechBayPilotHirePreviewService.BuildPreview(demoInventory);
            GUI.Label(
                new Rect(x, y + 126f, width, 18f),
                "Pilot Hire " + PilotHirePreviewText(pilotHirePreview));
            MechBayOwnedRosterEntry[] roster = MechBayOwnedRosterService.BuildRosterPreview(demoInventory);
            ClampSelectedRosterIndex(roster);
            GUI.Label(
                new Rect(x, y + 148f, width, 18f),
                "Roster " + TruncateText(OwnedRosterText(roster), 62));
            DrawRosterMissionStateLine(x, y + 170f, width, roster);
            MechBayMissionHandoffPreview handoffPreview =
                MechBayMissionHandoffPreviewService.BuildPreview(demoInventory);
            MechBayMissionRestartApplyGuard restartGuard =
                MechBayMissionHandoffPreviewService.BuildRestartApplyGuard(
                    demoInventory,
                    mission?.Contract,
                    combatProfiles);
            GUI.Label(
                new Rect(x, y + 192f, width, 18f),
                "Next Mission " + TruncateText(MissionHandoffPlayerSummaryText(handoffPreview, restartGuard), 58));
            DrawMissionHandoffLaunchAction(x, y + 214f, width, handoffPreview, restartGuard);
            DrawMissionHandoffLineup(x, y + 236f, width, handoffPreview);
            if (showRosterDetail)
            {
                DrawOwnedRosterDetail(x, y + 258f, width, roster, pilotHirePreview);
            }
        }

        private void DrawLoadoutUnit(UnitState unit, float x, float y, float width)
        {
            Rect card = new(x, y, width, LoadoutCardHeight);
            GUI.Box(card, TruncateText(LoadoutUnitTitle(unit), 82));

            float left = x + 12f;
            float right = x + width * 0.50f;
            float lineY = y + 24f;
            CombatLoadoutPreview loadoutPreview = LoadoutPreviewFor(unit);
            GUI.Label(
                new Rect(left, lineY, width * 0.46f, 18f),
                "Structure " + Mathf.RoundToInt(unit.CurrentStructure) + "/" + Mathf.RoundToInt(unit.Profile.MaxStructure)
                + "  Move " + Mathf.RoundToInt(unit.Profile.MoveSpeed));
            GUI.Label(
                new Rect(right, lineY, width * 0.46f, 18f),
                "Heat " + FormatDecimal(unit.CurrentHeat) + "/" + FormatDecimal(unit.Profile.HeatCapacity)
                + "  Load " + FormatDecimal(unit.CombatTotalWeaponWeight));

            lineY += 20f;
            GUI.Label(
                new Rect(left, lineY, width * 0.46f, 18f),
                "Primary " + TruncateText(unit.CombatPrimaryWeaponName, 28));
            GUI.Label(
                new Rect(right, lineY, width * 0.46f, 18f),
                "Range " + Mathf.RoundToInt(unit.CombatWeaponRange)
                + "  Damage " + FormatDecimal(unit.CombatWeaponDamage)
                + "  CD " + FormatDecimal(unit.CombatWeaponCooldown));

            lineY += 20f;
            DrawMechConditionLine(unit, left, right, lineY, width);

            lineY += 20f;
            DrawProjectedLoadoutStatus(loadoutPreview, left, right, lineY, width);

            lineY += 24f;
            DrawLoadoutSectionLine(unit, left, lineY, width - 24f);

            lineY += 34f;
            float gridHeight = DrawProjectedLoadoutGrid(unit, loadoutPreview, left, lineY, width - 24f);

            lineY += gridHeight + 8f;
            float weaponHeight = DrawWeaponLoadoutLines(unit, left, lineY, width - 24f);

            lineY += weaponHeight + 6f;
            DrawLoadoutEditControls(unit, loadoutPreview, left, lineY, width - 24f);
        }

        private static string LoadoutUnitTitle(UnitState unit)
        {
            if (unit == null)
            {
                return "Mech";
            }

            string title = unit.UnitType + "  " + unit.Id;
            if (!string.IsNullOrWhiteSpace(unit.OwnedMechId))
            {
                title += "  owned " + unit.OwnedMechId;
            }

            if (!string.IsNullOrWhiteSpace(unit.PilotDisplayName))
            {
                title += "  pilot " + unit.PilotDisplayName;
            }

            return title;
        }

        private void DrawProjectedLoadoutStatus(CombatLoadoutPreview preview, float left, float right, float y, float width)
        {
            LoadoutValidationResult result = preview.Validation;
            Color previous = GUI.color;
            GUI.color = result.IsValid ? new Color(0.50f, 1f, 0.62f, 1f) : new Color(1f, 0.78f, 0.28f, 1f);
            GUI.Label(
                new Rect(left, y, width * 0.46f, 18f),
                result.IsValid ? "Fit OK" : "Fit Review: " + TruncateText(FirstLoadoutError(result), 28));
            GUI.color = previous;

            GUI.Label(
                new Rect(right, y, width * 0.46f, 18f),
                "Heat " + FormatDecimal(result.TotalHeat) + "/" + FormatDecimal(preview.HeatLimit)
                + "  Load " + FormatDecimal(result.TotalWeight) + "/" + FormatDecimal(preview.WeightLimit)
                + "  Grid " + result.OccupiedGridCells + "/" + preview.GridCapacity);
        }

        private CombatLoadoutPreview LoadoutPreviewFor(UnitState unit)
        {
            string key = unit?.UnitType ?? "";
            return CombatLoadoutPreviewBuilder.Build(
                key,
                unit?.Profile,
                WeaponEnabledStateFor(unit),
                LoadoutPlacementOverridesFor(unit),
                LoadoutFillerOverridesFor(unit));
        }

        private static string FirstLoadoutError(LoadoutValidationResult result)
        {
            string[] errors = result.Errors;
            return errors.Length == 0 ? "" : errors[0];
        }

        private static string FirstInventoryError(MechBayInventoryValidationResult result)
        {
            string[] errors = result.Errors;
            return errors.Length == 0 ? "" : errors[0];
        }

        private MechBayInventoryAvailabilityResult CurrentDraftInventoryAvailability()
        {
            MechBayInventoryUsage usage = new();
            if (mission != null)
            {
                foreach (UnitState unit in mission.PlayerUnits())
                {
                    usage.AddLoadoutPreview(LoadoutPreviewFor(unit));
                }
            }

            return MechBayInventoryValidator.ValidateUsage(demoInventory, usage);
        }

        private string InventoryStatusError(MechBayInventoryAvailabilityResult availability)
        {
            if (availability != null && !availability.IsValid)
            {
                return FirstInventoryAvailabilityError(availability);
            }

            return demoInventoryValidation == null ? "" : FirstInventoryError(demoInventoryValidation);
        }

        private static string FirstInventoryAvailabilityError(MechBayInventoryAvailabilityResult result)
        {
            if (result == null)
            {
                return "";
            }

            string[] errors = result.Errors;
            return errors.Length == 0 ? "" : errors[0];
        }

        private static string InventoryUseText(int used, int available)
        {
            return used.ToString(CultureInfo.InvariantCulture)
                + "/"
                + available.ToString(CultureInfo.InvariantCulture);
        }

        private static string AssemblyPreviewText(MechBayAssemblyProgress progress)
        {
            if (progress == null)
            {
                return "No fragments";
            }

            string text = progress.displayName
                + " "
                + progress.fragments.ToString(CultureInfo.InvariantCulture)
                + "/"
                + progress.requiredFragments.ToString(CultureInfo.InvariantCulture);
            if (progress.canAssemble)
            {
                text += " ready";
            }

            return text;
        }

        private static string OwnedRosterText(MechBayOwnedRosterEntry[] roster)
        {
            if (roster == null || roster.Length == 0)
            {
                return "No owned mechs";
            }

            List<string> entries = new();
            int visibleCount = Mathf.Min(3, roster.Length);
            for (int index = 0; index < visibleCount; index++)
            {
                MechBayOwnedRosterEntry entry = roster[index];
                if (entry == null)
                {
                    continue;
                }

                string label = string.IsNullOrWhiteSpace(entry.unitType) ? "Mech" : entry.unitType;
                if (entry.isWarehouseMech)
                {
                    label += " depot";
                }

                label += " " + entry.conditionPercent.ToString(CultureInfo.InvariantCulture) + "%";
                if (!entry.availableForMission)
                {
                    label += " " + UnavailableRosterText(entry);
                }

                entries.Add(label);
            }

            if (entries.Count == 0)
            {
                return "No owned mechs";
            }

            string text = roster.Length.ToString(CultureInfo.InvariantCulture) + ": " + string.Join(" | ", entries);
            if (roster.Length > visibleCount)
            {
                text += " +" + (roster.Length - visibleCount).ToString(CultureInfo.InvariantCulture);
            }

            return text;
        }

        private void DrawRosterMissionStateLine(float x, float y, float width, MechBayOwnedRosterEntry[] roster)
        {
            RosterMissionState state = BuildRosterMissionState(roster);
            Color previous = GUI.color;
            GUI.color = state.NeedsRepairCount > 0 || state.NeedsFitCount > 0 || state.UnavailableCount > 0
                ? new Color(1f, 0.78f, 0.28f, 1f)
                : new Color(0.58f, 0.82f, 1f, 1f);
            GUI.Label(new Rect(x, y, width, 18f), "Mission State " + TruncateText(RosterMissionStateText(state), 72));
            GUI.color = previous;
        }

        private RosterMissionState BuildRosterMissionState(MechBayOwnedRosterEntry[] roster)
        {
            int ownedCount = 0;
            int deployedCount = 0;
            int readyCount = 0;
            int needsRepairCount = 0;
            int heldCount = 0;
            int needsFitCount = 0;
            int unavailableCount = 0;
            MechBayOwnedRosterEntry[] safeRoster = roster ?? Array.Empty<MechBayOwnedRosterEntry>();
            for (int index = 0; index < safeRoster.Length; index++)
            {
                MechBayOwnedRosterEntry entry = safeRoster[index];
                if (entry == null)
                {
                    continue;
                }

                ownedCount++;
                bool needsFit = RosterEntryNeedsFit(entry);
                if (entry.availableForMission)
                {
                    deployedCount++;
                    if (entry.conditionPercent >= 100 && !needsFit)
                    {
                        readyCount++;
                    }
                }
                else if (entry.isWarehouseMech)
                {
                    heldCount++;
                }
                else
                {
                    unavailableCount++;
                }

                if (entry.conditionPercent < 100)
                {
                    needsRepairCount++;
                }

                if (needsFit)
                {
                    needsFitCount++;
                }
            }

            return new RosterMissionState(
                ownedCount,
                CountActivePlayerUnits(),
                CountPlayerUnits(),
                deployedCount,
                readyCount,
                needsRepairCount,
                heldCount,
                needsFitCount,
                unavailableCount);
        }

        private static string RosterMissionStateText(RosterMissionState state)
        {
            return "Act "
                + state.ActivePlayerCount.ToString(CultureInfo.InvariantCulture)
                + "/"
                + state.PlayerSlotCount.ToString(CultureInfo.InvariantCulture)
                + "  Depl "
                + state.DeployedCount.ToString(CultureInfo.InvariantCulture)
                + "/"
                + state.OwnedCount.ToString(CultureInfo.InvariantCulture)
                + "  Ready "
                + state.ReadyCount.ToString(CultureInfo.InvariantCulture)
                + "  Repair "
                + state.NeedsRepairCount.ToString(CultureInfo.InvariantCulture)
                + "  Held "
                + state.HeldCount.ToString(CultureInfo.InvariantCulture)
                + "  Fit "
                + state.NeedsFitCount.ToString(CultureInfo.InvariantCulture)
                + "  Unavail "
                + state.UnavailableCount.ToString(CultureInfo.InvariantCulture);
        }

        private static bool RosterEntryNeedsFit(MechBayOwnedRosterEntry entry)
        {
            return entry != null
                && (string.Equals(entry.loadoutStatus, "Needs loadout", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(entry.loadoutStatus, "No loadout", StringComparison.OrdinalIgnoreCase));
        }

        private struct RosterMissionState
        {
            public RosterMissionState(
                int ownedCount,
                int activePlayerCount,
                int playerSlotCount,
                int deployedCount,
                int readyCount,
                int needsRepairCount,
                int heldCount,
                int needsFitCount,
                int unavailableCount)
            {
                OwnedCount = ownedCount;
                ActivePlayerCount = activePlayerCount;
                PlayerSlotCount = playerSlotCount;
                DeployedCount = deployedCount;
                ReadyCount = readyCount;
                NeedsRepairCount = needsRepairCount;
                HeldCount = heldCount;
                NeedsFitCount = needsFitCount;
                UnavailableCount = unavailableCount;
            }

            public int OwnedCount { get; }
            public int ActivePlayerCount { get; }
            public int PlayerSlotCount { get; }
            public int DeployedCount { get; }
            public int ReadyCount { get; }
            public int NeedsRepairCount { get; }
            public int HeldCount { get; }
            public int NeedsFitCount { get; }
            public int UnavailableCount { get; }
        }

        private struct RestartIdentityAssertionResult
        {
            public bool Accepted { get; set; }
            public string Summary { get; set; }

            public static RestartIdentityAssertionResult Blocked(string summary)
            {
                return new RestartIdentityAssertionResult
                {
                    Accepted = false,
                    Summary = summary ?? "blocked"
                };
            }
        }

        private string RestartIdentityText(MechBayMissionRestartRuntimeSwapResult result)
        {
            MechBayMissionRestartSpawnIntent[] intents = result?.SpawnIntents ?? Array.Empty<MechBayMissionRestartSpawnIntent>();
            if (mission == null || intents.Length == 0)
            {
                return "no owned-mech identity intents";
            }

            int bound = 0;
            int playerIndex = 0;
            List<string> sample = new();
            foreach (UnitState unit in mission.PlayerUnits())
            {
                if (unit == null)
                {
                    continue;
                }

                MechBayMissionRestartSpawnIntent intent = playerIndex < intents.Length ? intents[playerIndex] : null;
                playerIndex++;
                string ownedId = unit.OwnedMechId ?? "";
                if (!string.IsNullOrWhiteSpace(ownedId)
                    && string.Equals(ownedId, intent?.ownedMechId, StringComparison.OrdinalIgnoreCase))
                {
                    bound++;
                    if (sample.Count < 3)
                    {
                        sample.Add(ownedId + "->" + unit.Id);
                    }
                }
            }

            string text = bound.ToString(CultureInfo.InvariantCulture)
                + "/"
                + intents.Length.ToString(CultureInfo.InvariantCulture)
                + " runtime units bound";
            if (sample.Count > 0)
            {
                text += "  " + string.Join("; ", sample);
            }

            return text;
        }

        private static string MissionHandoffPreviewText(MechBayMissionHandoffPreview preview)
        {
            if (preview == null)
            {
                return "handoff unavailable";
            }

            string launch = string.IsNullOrWhiteSpace(preview.LaunchStatus) ? "Launch preview unavailable" : preview.LaunchStatus;
            string summary = string.IsNullOrWhiteSpace(preview.Summary) ? "No available mission slots" : preview.Summary;
            return preview.MissionSlotCount.ToString(CultureInfo.InvariantCulture)
                + " slots  "
                + TruncateText(summary, 38)
                + "  "
                + launch;
        }

        private static string MissionHandoffPlayerSummaryText(
            MechBayMissionHandoffPreview preview,
            MechBayMissionRestartApplyGuard guard)
        {
            if (preview == null)
            {
                return "No mission roster";
            }

            MechBaySquadSelectionSlot[] slots = preview.MissionSlots ?? Array.Empty<MechBaySquadSelectionSlot>();
            string state = guard?.ApplyEnabled == true ? "Ready" : "Blocked";
            string depot = preview.IncludesDepotMissionSlot ? "depot included" : "current squad";
            return state
                + "  "
                + slots.Length.ToString(CultureInfo.InvariantCulture)
                + " mechs  "
                + depot;
        }

        private bool DrawActionButton(Rect rect, string label, bool ready)
        {
            Color cue = ready
                ? new Color(0.42f, 0.82f, 1f, 1f)
                : new Color(1f, 0.62f, 0.26f, 0.75f);
            Color previous = GUI.color;
            GUI.color = cue;
            bool clicked = GUI.Button(rect, label);
            GUI.color = previous;
            DrawRectBorder(rect, cue, 1f);
            return clicked;
        }

        private void DrawActionStateLabel(float x, float y, float width, string text, bool ready, int maxLength)
        {
            Color cue = ready
                ? new Color(0.42f, 0.82f, 1f, 0.92f)
                : new Color(1f, 0.62f, 0.26f, 0.92f);
            Rect cueRect = new(x, y + 4f, 10f, 10f);
            DrawColorRect(cueRect, cue);
            DrawRectBorder(cueRect, new Color(0.02f, 0.025f, 0.03f, 0.95f), 1f);
            GUI.Label(
                new Rect(x + 16f, y, Mathf.Max(0f, width - 16f), 18f),
                TruncateText(text, maxLength));
        }

        private void DrawMissionHandoffLaunchAction(
            float x,
            float y,
            float width,
            MechBayMissionHandoffPreview preview,
            MechBayMissionRestartApplyGuard guard)
        {
            bool previousEnabled = GUI.enabled;
            bool ready = guard?.ApplyEnabled == true;
            GUI.enabled = previousEnabled && ready;
            if (DrawActionButton(new Rect(x, y - 2f, 58f, 22f), "Launch", ready))
            {
                TryApplyMissionRestartRuntimeSwap(keepMechBayOpen: true);
            }

            GUI.enabled = previousEnabled;
            DrawActionStateLabel(
                x + 66f,
                y,
                width - 66f,
                MissionHandoffLaunchActionText(guard, preview),
                ready,
                56);
        }

        private static string MissionHandoffLaunchActionText(
            MechBayMissionRestartApplyGuard guard,
            MechBayMissionHandoffPreview preview)
        {
            if (guard == null)
            {
                return "Blocked  launch unavailable";
            }

            if (guard.ApplyEnabled)
            {
                string depot = preview?.IncludesDepotMissionSlot == true ? "depot in squad" : "current squad";
                return "Ready  "
                    + guard.SpawnIntentCount.ToString(CultureInfo.InvariantCulture)
                    + " mechs  "
                    + depot;
            }

            return "Blocked  " + MissionHandoffPlayerBlockedReason(guard.Reason);
        }

        private static string MissionHandoffPlayerBlockedReason(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return "need ready squad";
            }

            if (reason.IndexOf("available mission slot", StringComparison.OrdinalIgnoreCase) >= 0
                || reason.IndexOf("ready mission squad", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "need ready squad";
            }

            if (reason.IndexOf("BattleMission", StringComparison.OrdinalIgnoreCase) >= 0
                || reason.IndexOf("contract", StringComparison.OrdinalIgnoreCase) >= 0
                || reason.IndexOf("template", StringComparison.OrdinalIgnoreCase) >= 0
                || reason.IndexOf("dry run", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "mission setup not ready";
            }

            return reason;
        }

        private static void DrawMissionHandoffLineup(
            float x,
            float y,
            float width,
            MechBayMissionHandoffPreview preview)
        {
            string lineup = preview?.IncludesDepotMissionSlot == true
                ? SquadSelectionCompactLineupSummary(preview.MissionSlots, "none ready")
                : SquadSelectionSlotSummary(preview?.MissionSlots, "none ready");
            GUI.Label(new Rect(x, y, width, 18f), TruncateText("Lineup " + lineup, 76));
        }

        private void DrawMissionRestartDryRun(float x, float y, float width)
        {
            MechBayMissionRestartDryRun dryRun =
                MechBayMissionHandoffPreviewService.BuildRestartDryRun(demoInventory);
            GUI.Label(new Rect(x, y, width, 18f), "Restart Dry Run " + TruncateText(MissionRestartDryRunText(dryRun), 58));
        }

        private static string MissionRestartDryRunText(MechBayMissionRestartDryRun dryRun)
        {
            if (dryRun == null)
            {
                return "unavailable";
            }

            string status = string.IsNullOrWhiteSpace(dryRun.Status) ? "Restart dry run unavailable" : dryRun.Status;
            string summary = string.IsNullOrWhiteSpace(dryRun.Summary) ? "No spawn intents" : dryRun.Summary;
            return dryRun.SpawnIntentCount.ToString(CultureInfo.InvariantCulture)
                + " intents  "
                + status
                + "  "
                + TruncateText(summary, 34);
        }

        private void DrawMissionRestartApplyGuard(float x, float y, float width)
        {
            MechBayMissionRestartApplyGuard guard =
                MechBayMissionHandoffPreviewService.BuildRestartApplyGuard(
                    demoInventory,
                    mission?.Contract,
                    combatProfiles);
            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && guard?.ApplyEnabled == true;
            if (GUI.Button(new Rect(x, y - 2f, 58f, 22f), "Apply"))
            {
                TryApplyMissionRestartRuntimeSwap(keepMechBayOpen: true);
            }

            GUI.enabled = previousEnabled;
            GUI.Label(new Rect(x + 66f, y, width - 66f, 18f), TruncateText(MissionRestartApplyGuardText(guard), 56));
        }

        private static string MissionRestartApplyGuardText(MechBayMissionRestartApplyGuard guard)
        {
            if (guard == null)
            {
                return "Restart apply unavailable";
            }

            string message = string.IsNullOrWhiteSpace(guard.Message) ? "Restart apply unavailable" : guard.Message;
            string reason = string.IsNullOrWhiteSpace(guard.Reason) ? "Runtime swap unavailable" : guard.Reason;
            return message
                + "  "
                + reason
                + "  "
                + guard.SpawnIntentCount.ToString(CultureInfo.InvariantCulture)
                + " intents";
        }

        private void DrawMissionRestartContractPreview(float x, float y, float width)
        {
            MechBayMissionRestartContractPreview preview =
                MechBayMissionHandoffPreviewService.BuildRestartContractPreview(demoInventory);
            GUI.Label(
                new Rect(x, y, width, 18f),
                "Restart Contract " + TruncateText(MissionRestartContractPreviewText(preview), 58));
        }

        private static string MissionRestartContractPreviewText(MechBayMissionRestartContractPreview preview)
        {
            if (preview == null)
            {
                return "unavailable";
            }

            string mission = string.IsNullOrWhiteSpace(preview.MissionTemplateId) ? "mission" : preview.MissionTemplateId;
            string commander = string.IsNullOrWhiteSpace(preview.CommanderDisplayName)
                ? "commander"
                : preview.CommanderDisplayName;
            string note = string.IsNullOrWhiteSpace(preview.PreviewNote) ? "contract not instantiated" : preview.PreviewNote;
            return preview.SpawnIntentCount.ToString(CultureInfo.InvariantCulture)
                + " spawns  "
                + mission
                + "  "
                + commander
                + "  "
                + note;
        }

        private void DrawMissionRestartContractCloneDryRun(float x, float y, float width)
        {
            MechBayMissionRestartContractCloneDryRun dryRun =
                MechBayMissionHandoffPreviewService.BuildRestartContractCloneDryRun(demoInventory, mission?.Contract);
            GUI.Label(
                new Rect(x, y, width, 18f),
                "Clone Dry Run " + TruncateText(MissionRestartContractCloneDryRunText(dryRun), 60));
        }

        private static string MissionRestartContractCloneDryRunText(MechBayMissionRestartContractCloneDryRun dryRun)
        {
            if (dryRun == null)
            {
                return "unavailable";
            }

            string status = string.IsNullOrWhiteSpace(dryRun.Status)
                ? "Restart contract clone dry run unavailable"
                : dryRun.Status;
            return dryRun.ReplacedPlayerSpawnCount.ToString(CultureInfo.InvariantCulture)
                + "/"
                + dryRun.TemplatePlayerUnitCount.ToString(CultureInfo.InvariantCulture)
                + " player spawns  "
                + status
                + "  "
                + (dryRun.PreparedContractAvailable ? "contract ready" : "no contract");
        }

        private void DrawMissionRestartConstructionDryRun(float x, float y, float width)
        {
            MechBayMissionRestartConstructionDryRun dryRun =
                MechBayMissionHandoffPreviewService.BuildRestartConstructionDryRun(
                    demoInventory,
                    mission?.Contract,
                    combatProfiles);
            GUI.Label(
                new Rect(x, y, width, 18f),
                "Mission Dry Run " + TruncateText(MissionRestartConstructionDryRunText(dryRun), 58));
        }

        private static string MissionRestartConstructionDryRunText(MechBayMissionRestartConstructionDryRun dryRun)
        {
            if (dryRun == null)
            {
                return "unavailable";
            }

            string status = string.IsNullOrWhiteSpace(dryRun.Status)
                ? "BattleMission construction dry run unavailable"
                : dryRun.Status;
            return dryRun.ConstructedPlayerUnitCount.ToString(CultureInfo.InvariantCulture)
                + "/"
                + dryRun.SpawnIntentCount.ToString(CultureInfo.InvariantCulture)
                + " players  "
                + status
                + "  "
                + (dryRun.ThrowawayMissionConstructed ? "throwaway OK" : "not built");
        }

        private static string WeaponShopPreviewText(MechBayWeaponShopPreview preview)
        {
            MechBayWeaponShopEntry[] entries = preview?.Entries ?? Array.Empty<MechBayWeaponShopEntry>();
            if (entries.Length == 0)
            {
                return "No ordinary weapons";
            }

            int cheapestCost = int.MaxValue;
            int affordableCount = 0;
            for (int index = 0; index < entries.Length; index++)
            {
                MechBayWeaponShopEntry entry = entries[index];
                if (entry == null)
                {
                    continue;
                }

                cheapestCost = Mathf.Min(cheapestCost, Mathf.Max(0, entry.tokenCost));
                if (entry.canAfford)
                {
                    affordableCount++;
                }
            }

            string cost = cheapestCost == int.MaxValue ? "unknown" : FormatTokens(cheapestCost) + " token";
            return entries.Length.ToString(CultureInfo.InvariantCulture)
                + " ordinary weapons from "
                + cost
                + "  afford "
                + affordableCount.ToString(CultureInfo.InvariantCulture)
                + "  demo buy";
        }

        private void DrawWeaponShopPurchaseStub(
            float x,
            float y,
            float width,
            MechBayWeaponShopPreview preview,
            MechBayInventoryContract inventory)
        {
            MechBayWeaponShopEntry firstEntry = FirstWeaponShopEntry(preview);
            MechBayWeaponPurchasePreviewResult purchasePreview =
                MechBayWeaponShopPreviewService.PreviewPurchase(inventory, firstEntry?.itemId);
            bool canPurchase = firstEntry != null
                && firstEntry.purchaseEnabled
                && purchasePreview != null
                && purchasePreview.CanAfford;

            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && canPurchase;
            if (GUI.Button(new Rect(x, y - 2f, 46f, 22f), "Buy"))
            {
                MechBayWeaponPurchasePreviewResult result =
                    MechBayWeaponShopPreviewService.TryApplyDemoPurchase(inventory, firstEntry.itemId);
                statusText = result?.Message ?? "Purchase unavailable";
                if (result != null && result.Accepted)
                {
                    AddCombatLogLine("Shop " + result.Message);
                    RefreshDemoInventoryValidation();
                    purchasePreview = MechBayWeaponShopPreviewService.PreviewPurchase(inventory, firstEntry.itemId);
                }
            }

            GUI.enabled = previousEnabled;
            GUI.Label(new Rect(x + 54f, y, width - 54f, 18f), TruncateText(WeaponShopPurchaseStubText(purchasePreview), 56));
        }

        private static MechBayWeaponShopEntry FirstWeaponShopEntry(MechBayWeaponShopPreview preview)
        {
            MechBayWeaponShopEntry[] entries = preview?.Entries ?? Array.Empty<MechBayWeaponShopEntry>();
            for (int index = 0; index < entries.Length; index++)
            {
                if (entries[index] != null)
                {
                    return entries[index];
                }
            }

            return null;
        }

        private static string WeaponShopPurchaseStubText(MechBayWeaponPurchasePreviewResult purchasePreview)
        {
            if (purchasePreview == null || string.IsNullOrWhiteSpace(purchasePreview.displayName))
            {
                return "Purchase unavailable";
            }

            string cost = FormatTokens(purchasePreview.TokenCost) + " token";
            return purchasePreview.displayName + " " + cost + "  " + purchasePreview.Message;
        }

        private void DrawLocalCandidatePrepAction(float x, float y, float width)
        {
            string readyCandidate = FirstStartupDepotCandidateOwnedMechId();
            bool ready = !string.IsNullOrWhiteSpace(readyCandidate);
            bool canAct = demoInventory != null;
            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && canAct;
            if (DrawActionButton(new Rect(x, y - 2f, 58f, 22f), ready ? "Open" : "Prep", canAct))
            {
                string ownedMechId = readyCandidate;
                if (string.IsNullOrWhiteSpace(ownedMechId))
                {
                    ownedMechId = EnsureLocalReadyCandidate("Mech bay candidate", false);
                }

                if (!string.IsNullOrWhiteSpace(ownedMechId))
                {
                    OpenSquadSelectionPreviewIncoming(ownedMechId, "Candidate prep");
                }
            }

            GUI.enabled = previousEnabled;
            DrawActionStateLabel(
                x + 66f,
                y,
                width - 66f,
                "Candidate " + LocalCandidatePrepText(readyCandidate),
                canAct,
                56);
        }

        private string LocalCandidatePrepText(string readyCandidate)
        {
            if (!string.IsNullOrWhiteSpace(readyCandidate))
            {
                return "Ready " + readyCandidate;
            }

            if (demoInventory == null)
            {
                return "Unavailable";
            }

            string pendingOwnedMechId = FirstStartupPendingWarehouseMechId();
            if (!string.IsNullOrWhiteSpace(pendingOwnedMechId))
            {
                return "Prep pending depot mech";
            }

            return "Build + pilot + weapon + fit";
        }

        private static string PilotHirePreviewText(MechBayPilotHirePreview preview)
        {
            MechBayPilotHireCandidate[] candidates = preview?.Candidates ?? Array.Empty<MechBayPilotHireCandidate>();
            if (candidates.Length == 0)
            {
                return "No NPC pilots";
            }

            int cheapestCost = int.MaxValue;
            int affordableCount = 0;
            for (int index = 0; index < candidates.Length; index++)
            {
                MechBayPilotHireCandidate candidate = candidates[index];
                if (candidate == null)
                {
                    continue;
                }

                cheapestCost = Mathf.Min(cheapestCost, Mathf.Max(0, candidate.hireCost));
                if (candidate.canAfford)
                {
                    affordableCount++;
                }
            }

            string cost = cheapestCost == int.MaxValue ? "unknown" : FormatTokens(cheapestCost) + " token";
            return candidates.Length.ToString(CultureInfo.InvariantCulture)
                + " NPC candidates from "
                + cost
                + "  afford "
                + affordableCount.ToString(CultureInfo.InvariantCulture)
                + "  demo hire";
        }

        private void DrawOwnedRosterDetail(
            float x,
            float y,
            float width,
            MechBayOwnedRosterEntry[] roster,
            MechBayPilotHirePreview pilotHirePreview)
        {
            if (roster == null || roster.Length == 0)
            {
                GUI.Label(new Rect(x, y, width, 18f), "Detail No owned mechs");
                return;
            }

            MechBayOwnedRosterEntry entry = roster[Mathf.Clamp(selectedRosterIndex, 0, roster.Length - 1)];
            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && roster.Length > 1;
            if (GUI.Button(new Rect(x, y - 2f, 28f, 22f), "<"))
            {
                selectedRosterIndex = (selectedRosterIndex + roster.Length - 1) % roster.Length;
                entry = roster[selectedRosterIndex];
                statusText = "Roster " + TruncateText(entry.displayName, 20);
            }

            if (GUI.Button(new Rect(x + 34f, y - 2f, 28f, 22f), ">"))
            {
                selectedRosterIndex = (selectedRosterIndex + 1) % roster.Length;
                entry = roster[selectedRosterIndex];
                statusText = "Roster " + TruncateText(entry.displayName, 20);
            }

            GUI.enabled = previousEnabled;
            GUI.Label(
                new Rect(x + 70f, y, width - 70f, 18f),
                TruncateText(OwnedRosterDetailText(entry), 62));
            GUI.Label(
                new Rect(x + 70f, y + 20f, width - 70f, 18f),
                TruncateText(OwnedRosterFitText(entry), 62));
            GUI.Label(
                new Rect(x + 70f, y + 42f, width - 70f, 18f),
                TruncateText(OwnedRosterPilotText(entry), 62));
            GUI.Label(
                new Rect(x + 70f, y + 64f, width - 70f, 18f),
                TruncateText(OwnedRosterDeploymentText(entry), 62));
            DrawOwnedRosterPilotHireStub(x + 70f, y + 86f, width - 70f, entry, pilotHirePreview);
            GUI.Label(
                new Rect(x + 70f, y + 108f, width - 70f, 18f),
                TruncateText(OwnedRosterWeaponStockText(entry), 62));
            DrawOwnedRosterDraftFitStub(x + 70f, y + 130f, width - 70f, entry);
            DrawOwnedRosterSquadSelectionStub(x + 70f, y + 174f, width - 70f, entry);
        }

        private void ClampSelectedRosterIndex(MechBayOwnedRosterEntry[] roster)
        {
            int count = roster?.Length ?? 0;
            if (count <= 0)
            {
                selectedRosterIndex = 0;
                return;
            }

            selectedRosterIndex = Mathf.Clamp(selectedRosterIndex, 0, count - 1);
        }

        private static string OwnedRosterDetailText(MechBayOwnedRosterEntry entry)
        {
            if (entry == null)
            {
                return "Detail unavailable";
            }

            string source = entry.isWarehouseMech ? "Depot" : "Squad";
            string state = entry.availableForMission ? "Ready" : UnavailableRosterText(entry);
            string name = string.IsNullOrWhiteSpace(entry.displayName) ? entry.unitType : entry.displayName;
            return source + " " + name
                + "  " + entry.conditionPercent.ToString(CultureInfo.InvariantCulture) + "%"
                + "  " + state;
        }

        private static string UnavailableRosterText(MechBayOwnedRosterEntry entry)
        {
            return entry != null && string.Equals(entry.loadoutStatus, "Needs loadout", StringComparison.OrdinalIgnoreCase)
                ? "needs fit"
                : "hold";
        }

        private static string OwnedRosterFitText(MechBayOwnedRosterEntry entry)
        {
            if (entry == null)
            {
                return "";
            }

            string chassis = string.IsNullOrWhiteSpace(entry.chassisId) ? entry.unitType : entry.chassisId;
            string loadoutStatus = string.IsNullOrWhiteSpace(entry.loadoutStatus) ? "Unknown" : entry.loadoutStatus;
            string loadout = string.IsNullOrWhiteSpace(entry.activeLoadoutId) ? "none" : entry.activeLoadoutId;
            string id = string.IsNullOrWhiteSpace(entry.ownedMechId) ? "unknown" : entry.ownedMechId;
            return "Chassis " + chassis + "  Fit " + loadoutStatus + " (" + loadout + ")  Id " + id;
        }

        private static string OwnedRosterPilotText(MechBayOwnedRosterEntry entry)
        {
            if (entry == null)
            {
                return "Pilot unavailable";
            }

            string status = string.IsNullOrWhiteSpace(entry.pilotStatus) ? "Unknown" : entry.pilotStatus;
            string pilot = string.IsNullOrWhiteSpace(entry.pilotDisplayName) ? "No pilot assigned" : entry.pilotDisplayName;
            return "Pilot " + status + " (" + pilot + ")";
        }

        private static string OwnedRosterDeploymentText(MechBayOwnedRosterEntry entry)
        {
            if (entry == null)
            {
                return "Deploy unavailable";
            }

            string status = string.IsNullOrWhiteSpace(entry.deploymentStatus) ? "Unknown" : entry.deploymentStatus;
            string requirements = string.IsNullOrWhiteSpace(entry.deploymentRequirements) ? "Requirements unknown" : entry.deploymentRequirements;
            return "Deploy " + status + "  " + requirements;
        }

        private void DrawOwnedRosterPilotHireStub(
            float x,
            float y,
            float width,
            MechBayOwnedRosterEntry entry,
            MechBayPilotHirePreview preview)
        {
            MechBayPilotHireCandidate candidate = FirstPilotHireCandidate(preview);
            MechBayPilotHireResult hirePreview = entry != null && entry.hasPilotPlaceholder
                ? MechBayPilotHirePreviewService.PreviewHire(demoInventory, entry.ownedMechId, candidate?.pilotId)
                : null;
            bool canHire = entry != null
                && entry.hasPilotPlaceholder
                && candidate != null
                && hirePreview != null
                && hirePreview.CanAfford
                && string.Equals(hirePreview.Message, "Ready demo hire", StringComparison.Ordinal);

            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && canHire;
            if (GUI.Button(new Rect(x, y - 2f, 46f, 22f), "Hire"))
            {
                MechBayPilotHireResult result =
                    MechBayPilotHirePreviewService.TryApplyDemoHire(demoInventory, entry.ownedMechId, candidate.pilotId);
                statusText = result?.Message ?? "Pilot hire unavailable";
                if (result != null && result.Accepted)
                {
                    AddCombatLogLine("Pilot " + result.Message);
                    RefreshDemoInventoryValidation();
                    hirePreview = MechBayPilotHirePreviewService.PreviewHire(demoInventory, entry.ownedMechId, candidate.pilotId);
                }
            }

            GUI.enabled = previousEnabled;
            GUI.Label(new Rect(x + 54f, y, width - 54f, 18f), TruncateText(OwnedRosterPilotHireText(entry, hirePreview), 52));
        }

        private static string OwnedRosterPilotHireText(MechBayOwnedRosterEntry entry, MechBayPilotHireResult hirePreview)
        {
            if (entry == null)
            {
                return "Hire Pilot unavailable";
            }

            if (!entry.hasPilotPlaceholder)
            {
                return "Hire Pilot already assigned";
            }

            if (hirePreview == null || string.IsNullOrWhiteSpace(hirePreview.displayName))
            {
                return "Hire Pilot unavailable";
            }

            string cost = FormatTokens(hirePreview.TokenCost) + " token";
            string risk = string.IsNullOrWhiteSpace(hirePreview.RiskProfile) ? "" : "  " + hirePreview.RiskProfile;
            return hirePreview.displayName + " " + cost + "  " + hirePreview.Message + risk;
        }

        private static MechBayPilotHireCandidate FirstPilotHireCandidate(MechBayPilotHirePreview preview)
        {
            MechBayPilotHireCandidate[] candidates = preview?.Candidates ?? Array.Empty<MechBayPilotHireCandidate>();
            for (int index = 0; index < candidates.Length; index++)
            {
                if (candidates[index] != null)
                {
                    return candidates[index];
                }
            }

            return null;
        }

        private static string OwnedRosterWeaponStockText(MechBayOwnedRosterEntry entry)
        {
            if (entry == null)
            {
                return "Stock unavailable";
            }

            string stock = string.IsNullOrWhiteSpace(entry.spareWeaponStockStatus)
                ? "No weapon stock"
                : entry.spareWeaponStockStatus;
            return "Stock " + stock;
        }

        private void DrawOwnedRosterDraftFitStub(float x, float y, float width, MechBayOwnedRosterEntry entry)
        {
            bool canOpenDraftFitGate = entry != null && entry.hasDraftFitStub && entry.draftFitReady;
            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && canOpenDraftFitGate;
            if (GUI.Button(new Rect(x, y - 2f, 72f, 22f), "Draft Fit"))
            {
                string name = entry == null || string.IsNullOrWhiteSpace(entry.displayName) ? "depot mech" : entry.displayName;
                showWarehouseDraftFitPreview = true;
                showSquadSelectionPreview = false;
                warehouseDraftFitPreviewMechId = entry?.ownedMechId;
                ClearSquadSelectionDraft();
                ClearSquadSelectionCompletedReplacement();
                statusText = "Draft fit ready: " + TruncateText(name, 24);
                AddCombatLogLine("Mech bay draft fit gate ready for " + name);
            }

            GUI.enabled = previousEnabled;

            string status = entry == null || string.IsNullOrWhiteSpace(entry.draftFitStatus)
                ? "Fit draft unavailable"
                : entry.draftFitStatus;
            GUI.Label(new Rect(x + 80f, y, width - 80f, 18f), TruncateText(status, 44));

            string requirements = entry == null || string.IsNullOrWhiteSpace(entry.draftFitRequirements)
                ? "Requirements unknown"
                : "Requires " + entry.draftFitRequirements;
            GUI.Label(new Rect(x + 80f, y + 22f, width - 80f, 18f), TruncateText(requirements, 44));
        }

        private void DrawOwnedRosterSquadSelectionStub(float x, float y, float width, MechBayOwnedRosterEntry entry)
        {
            bool canPreviewSquad = entry != null;
            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && canPreviewSquad;
            if (GUI.Button(new Rect(x, y - 2f, 92f, 22f), "Next Squad"))
            {
                OpenSquadSelectionPreview(entry);
            }

            GUI.enabled = previousEnabled;
            GUI.Label(new Rect(x + 100f, y, width - 100f, 18f), TruncateText(OwnedRosterSquadSelectionText(entry), 42));
        }

        private static string OwnedRosterSquadSelectionText(MechBayOwnedRosterEntry entry)
        {
            if (entry == null)
            {
                return "Selection unavailable";
            }

            string status = string.IsNullOrWhiteSpace(entry.squadSelectionStatus)
                ? "Selection unavailable"
                : entry.squadSelectionStatus;
            string requirements = string.IsNullOrWhiteSpace(entry.squadSelectionRequirements)
                ? "Requirements unknown"
                : entry.squadSelectionRequirements;
            return status + "  " + requirements;
        }

        private void OpenSquadSelectionPreview(MechBayOwnedRosterEntry entry)
        {
            string name = string.IsNullOrWhiteSpace(entry?.displayName) ? "owned mech" : entry.displayName;
            showSquadSelectionPreview = true;
            showWarehouseDraftFitPreview = false;
            warehouseDraftFitPreviewMechId = null;
            ClearSquadSelectionDraft();
            ClearSquadSelectionCompletedReplacement();

            if (entry?.availableForMission == true)
            {
                squadSelectionDraftOutgoingOwnedMechId = entry.ownedMechId;
                statusText = "Next squad out: " + TruncateText(name, 20);
                AddCombatLogLine("Next mission squad opened with outgoing " + name);
                return;
            }

            if (entry?.squadSelectionCandidate == true)
            {
                squadSelectionDraftIncomingOwnedMechId = entry.ownedMechId;
                statusText = "Next squad in: " + TruncateText(name, 20);
                AddCombatLogLine("Next mission squad opened with incoming " + name);
                return;
            }

            statusText = "Next squad preview: " + TruncateText(name, 18);
            AddCombatLogLine("Next mission squad preview opened");
        }

        private void OpenSquadSelectionPreviewIncoming(string incomingOwnedMechId, string sourceLabel)
        {
            showSquadSelectionPreview = true;
            showWarehouseDraftFitPreview = false;
            warehouseDraftFitPreviewMechId = null;
            ClearSquadSelectionDraft();
            ClearSquadSelectionCompletedReplacement();
            squadSelectionDraftIncomingOwnedMechId = incomingOwnedMechId;

            string name = StartupRosterDisplayName(incomingOwnedMechId);
            statusText = "Next squad in: " + TruncateText(name, 20);
            AddCombatLogLine((sourceLabel ?? "Next mission squad") + " opened with incoming " + name);
        }

        private string StartupRosterDisplayName(string ownedMechId)
        {
            MechBayOwnedRosterEntry entry = StartupRosterEntryByOwnedMechId(ownedMechId);
            if (!string.IsNullOrWhiteSpace(entry?.displayName))
            {
                return entry.displayName;
            }

            return string.IsNullOrWhiteSpace(ownedMechId) ? "owned mech" : ownedMechId;
        }

        private void DrawSquadSelectionPreview(float x, float y, float width)
        {
            MechBaySquadSelectionPreview preview = MechBaySquadSelectionPreviewService.BuildPreview(demoInventory);
            MechBaySquadSelectionDraftState draft =
                MechBaySquadSelectionPreviewService.BuildDraftState(
                    demoInventory,
                    squadSelectionDraftOutgoingOwnedMechId,
                    squadSelectionDraftIncomingOwnedMechId);
            squadSelectionDraftOutgoingOwnedMechId = draft?.OutgoingOwnedMechId;
            squadSelectionDraftIncomingOwnedMechId = draft?.IncomingOwnedMechId;

            GUI.Box(new Rect(x, y, width, 238f), "Next Mission Squad");
            if (GUI.Button(new Rect(x + width - 58f, y + 4f, 48f, 22f), "Hide"))
            {
                showSquadSelectionPreview = false;
                ClearSquadSelectionDraft();
                ClearSquadSelectionCompletedReplacement();
                statusText = "Squad preview closed";
            }

            string status = string.IsNullOrWhiteSpace(preview?.Status) ? "Preview unavailable" : preview.Status;
            GUI.Label(new Rect(x + 12f, y + 24f, width - 24f, 18f), TruncateText(status, 76));
            if (SquadSelectionCompleted(preview))
            {
                GUI.Label(
                    new Rect(x + 12f, y + 44f, width - 24f, 18f),
                    "Done  Next mission squad ready  "
                    + (preview?.MissionSlotCount ?? 0).ToString(CultureInfo.InvariantCulture)
                    + " mechs");
                DrawSquadSelectionCompletedSetLine(x + 12f, y + 64f, width - 24f);
                GUI.Label(
                    new Rect(x + 12f, y + 92f, width - 24f, 18f),
                    TruncateText("Lineup " + SquadSelectionCompactLineupSummary(preview?.MissionSlots, "none ready"), 76));
                DrawSquadSelectionPendingSwap(x + 12f, y + 114f, width - 24f, preview, draft);
                DrawSquadSelectionRestartHandoff(
                    x + 12f,
                    y + 136f,
                    width - 24f,
                    "Launch restarts with updated squad  ");
                string completedNote = string.IsNullOrWhiteSpace(preview?.PreviewNote) ? "Swap complete" : preview.PreviewNote;
                GUI.Label(new Rect(x + 12f, y + 158f, width - 24f, 18f), TruncateText(completedNote, 76));
                return;
            }

            GUI.Label(
                new Rect(x + 12f, y + 44f, width - 24f, 18f),
                "Current Slots "
                + (preview?.MissionSlotCount ?? 0).ToString(CultureInfo.InvariantCulture)
                + "  Depot Candidates "
                + (preview?.CandidateCount ?? 0).ToString(CultureInfo.InvariantCulture));
            GUI.Label(
                new Rect(x + 12f, y + 64f, width - 24f, 18f),
                TruncateText("Slots " + SquadSelectionSlotSummary(preview?.MissionSlots, "none"), 76));
            GUI.Label(
                new Rect(x + 12f, y + 84f, width - 24f, 18f),
                TruncateText("Candidates " + SquadSelectionSlotSummary(preview?.DepotCandidates, "none ready"), 76));
            DrawSquadSelectionDraftPickers(x + 12f, y + 106f, width - 24f, preview, draft);
            DrawSquadSelectionSwapPlan(x + 12f, y + 154f, width - 24f, draft);
            DrawSquadSelectionPendingSwap(x + 12f, y + 176f, width - 24f, preview, draft);
            DrawSquadSelectionRestartHandoff(x + 12f, y + 198f, width - 24f, "Next Mission  ");
            string note = string.IsNullOrWhiteSpace(preview?.PreviewNote) ? "Preview only" : preview.PreviewNote;
            GUI.Label(new Rect(x + 12f, y + 220f, width - 24f, 18f), TruncateText(note, 76));
        }

        private void DrawSquadSelectionDraftPickers(
            float x,
            float y,
            float width,
            MechBaySquadSelectionPreview preview,
            MechBaySquadSelectionDraftState draft)
        {
            DrawSquadSelectionDraftPicker(
                x,
                y,
                width,
                "Out",
                preview?.MissionSlots,
                draft?.OutgoingOwnedMechId,
                true);
            DrawSquadSelectionDraftPicker(
                x,
                y + 22f,
                width,
                "In",
                preview?.DepotCandidates,
                draft?.IncomingOwnedMechId,
                false);
        }

        private void DrawSquadSelectionDraftPicker(
            float x,
            float y,
            float width,
            string label,
            MechBaySquadSelectionSlot[] slots,
            string selectedOwnedMechId,
            bool outgoing)
        {
            MechBaySquadSelectionSlot[] safeSlots = slots ?? Array.Empty<MechBaySquadSelectionSlot>();
            Color cueColor = outgoing
                ? new Color(1f, 0.42f, 0.32f, 0.9f)
                : new Color(0.42f, 0.82f, 1f, 0.9f);
            Rect rowRect = new(x, y - 3f, width, 24f);
            DrawColorRect(rowRect, new Color(cueColor.r, cueColor.g, cueColor.b, 0.13f));
            DrawRectBorder(rowRect, new Color(cueColor.r, cueColor.g, cueColor.b, 0.55f), 1f);

            bool canCycle = safeSlots.Length > 1;
            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && canCycle;
            if (GUI.Button(new Rect(x, y - 2f, 28f, 22f), "<"))
            {
                CycleSquadSelectionDraft(safeSlots, outgoing, -1);
            }

            if (GUI.Button(new Rect(x + 34f, y - 2f, 28f, 22f), ">"))
            {
                CycleSquadSelectionDraft(safeSlots, outgoing, 1);
            }

            GUI.enabled = previousEnabled;
            int selectedIndex = SquadSelectionSlotIndex(safeSlots, selectedOwnedMechId);
            MechBaySquadSelectionSlot selected = selectedIndex >= 0 ? safeSlots[selectedIndex] : null;
            string count = safeSlots.Length > 0
                ? " (" + (selectedIndex + 1).ToString(CultureInfo.InvariantCulture) + "/"
                  + safeSlots.Length.ToString(CultureInfo.InvariantCulture) + ")"
                : " (0/0)";
            string cueLabel = string.IsNullOrWhiteSpace(label)
                ? (outgoing ? "OUT" : "IN")
                : label.ToUpperInvariant();
            string direction = outgoing ? " leaves next mission  " : " joins next mission  ";
            DrawColorRect(new Rect(x + 70f, y + 4f, 10f, 10f), cueColor);
            GUI.Label(
                new Rect(x + 86f, y, width - 86f, 18f),
                TruncateText(cueLabel + direction + SquadSelectionSlotName(selected) + count, 64));
        }

        private static void DrawSquadSelectionSwapPlan(
            float x,
            float y,
            float width,
            MechBaySquadSelectionDraftState draft)
        {
            string text;
            if (draft?.Ready == true)
            {
                string outgoing = string.IsNullOrWhiteSpace(draft.OutgoingDisplayName)
                    ? "mission slot"
                    : draft.OutgoingDisplayName;
                string incoming = string.IsNullOrWhiteSpace(draft.IncomingDisplayName)
                    ? "depot mech"
                    : draft.IncomingDisplayName;
                text = "Replacement  " + outgoing + " -> " + incoming + "  Set for next launch";
            }
            else
            {
                string requirements = string.IsNullOrWhiteSpace(draft?.Requirements)
                    ? "Need mission slot + fitted depot candidate"
                    : draft.Requirements;
                text = "Plan blocked  " + requirements;
            }

            GUI.Label(new Rect(x, y, width, 18f), TruncateText(text, 76));
        }

        private void DrawSquadSelectionCompletedSetLine(float x, float y, float width)
        {
            Color cueColor = new(0.42f, 0.82f, 1f, 0.9f);
            Rect rowRect = new(x, y - 3f, width, 24f);
            DrawColorRect(rowRect, new Color(cueColor.r, cueColor.g, cueColor.b, 0.13f));
            DrawRectBorder(rowRect, new Color(cueColor.r, cueColor.g, cueColor.b, 0.55f), 1f);
            DrawColorRect(new Rect(x + 6f, y + 4f, 10f, 10f), cueColor);
            GUI.Label(
                new Rect(x + 22f, y, width - 22f, 18f),
                TruncateText("SET  " + SquadSelectionCompletedReplacementText(), 72));
        }

        private string SquadSelectionCompletedReplacementText()
        {
            string incoming = string.IsNullOrWhiteSpace(squadSelectionLastIncomingDisplayName)
                ? "Depot candidate"
                : squadSelectionLastIncomingDisplayName;
            string outgoing = string.IsNullOrWhiteSpace(squadSelectionLastOutgoingDisplayName)
                ? "previous mission slot"
                : squadSelectionLastOutgoingDisplayName;
            return incoming + " replaces " + outgoing;
        }

        private bool HasSquadSelectionCompletedReplacement()
        {
            return !string.IsNullOrWhiteSpace(squadSelectionLastIncomingDisplayName)
                || !string.IsNullOrWhiteSpace(squadSelectionLastOutgoingDisplayName);
        }

        private void DrawSquadSelectionPendingSwap(
            float x,
            float y,
            float width,
            MechBaySquadSelectionPreview preview,
            MechBaySquadSelectionDraftState draft)
        {
            bool previousEnabled = GUI.enabled;
            bool canConfirm = draft?.Ready == true;
            GUI.enabled = previousEnabled && canConfirm;
            if (DrawActionButton(new Rect(x, y - 2f, 72f, 22f), "Set", canConfirm))
            {
                MechBaySquadSelectionApplyResult result =
                    MechBaySquadSelectionPreviewService.TryApplyPendingSwap(demoInventory, draft);
                statusText = SquadSelectionApplyStatusText(result);
                if (result?.Accepted == true)
                {
                    RecordSquadSelectionCompletedReplacement(draft);
                    ClearSquadSelectionDraft();
                    AddCombatLogLine(statusText + ": " + result.Summary);
                }
                else
                {
                    AddCombatLogLine("Squad selection " + statusText);
                }

                demoInventoryValidation = MechBayInventoryValidator.Validate(demoInventory);
                preview = MechBaySquadSelectionPreviewService.BuildPreview(demoInventory);
                draft = MechBaySquadSelectionPreviewService.BuildDraftState(
                    demoInventory,
                    squadSelectionDraftOutgoingOwnedMechId,
                    squadSelectionDraftIncomingOwnedMechId);
                squadSelectionDraftOutgoingOwnedMechId = draft?.OutgoingOwnedMechId;
                squadSelectionDraftIncomingOwnedMechId = draft?.IncomingOwnedMechId;
            }

            GUI.enabled = previousEnabled;
            DrawActionStateLabel(
                x + 80f,
                y,
                width - 80f,
                SquadSelectionConfirmLineText(preview, draft),
                canConfirm,
                64);
        }

        private static string SquadSelectionApplyStatusText(MechBaySquadSelectionApplyResult result)
        {
            if (result?.Accepted == true)
            {
                return "Squad updated";
            }

            if (result == null)
            {
                return "Swap blocked";
            }

            string reason = string.IsNullOrWhiteSpace(result.Reason) ? result.Message : result.Reason;
            return "Swap blocked: " + SquadSelectionPlayerBlockedReason(reason);
        }

        private static string SquadSelectionConfirmLineText(
            MechBaySquadSelectionPreview preview,
            MechBaySquadSelectionDraftState draft)
        {
            if (draft?.Ready == true)
            {
                return "Ready  set next mission squad";
            }

            if (preview?.HasRefreshedMissionSlot == true)
            {
                return "Done  next mission updated";
            }

            int missionSlots = preview?.MissionSlotCount ?? 0;
            int candidates = preview?.CandidateCount ?? 0;
            if (missionSlots <= 0)
            {
                return "Blocked  no mission mech";
            }

            if (candidates <= 0)
            {
                return "Blocked  no depot mech ready";
            }

            return "Blocked  choose Out and In";
        }

        private static string SquadSelectionPlayerBlockedReason(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return "choose Out and In";
            }

            if (reason.IndexOf("mission slot", StringComparison.OrdinalIgnoreCase) >= 0
                && reason.IndexOf("depot", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "choose Out and In";
            }

            if (reason.IndexOf("Selected mech missing", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "selection missing";
            }

            if (reason.IndexOf("Inventory", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "inventory unavailable";
            }

            if (reason.IndexOf("No pending", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "choose Out and In";
            }

            return reason;
        }

        private void RecordSquadSelectionCompletedReplacement(MechBaySquadSelectionDraftState draft)
        {
            squadSelectionLastOutgoingDisplayName = string.IsNullOrWhiteSpace(draft?.OutgoingDisplayName)
                ? "mission slot"
                : draft.OutgoingDisplayName;
            squadSelectionLastIncomingDisplayName = string.IsNullOrWhiteSpace(draft?.IncomingDisplayName)
                ? "depot mech"
                : draft.IncomingDisplayName;
        }

        private void ClearSquadSelectionCompletedReplacement()
        {
            squadSelectionLastOutgoingDisplayName = null;
            squadSelectionLastIncomingDisplayName = null;
        }

        private void DrawSquadSelectionRestartHandoff(float x, float y, float width, string labelPrefix)
        {
            MechBayMissionHandoffPreview preview =
                MechBayMissionHandoffPreviewService.BuildPreview(demoInventory);
            MechBayMissionRestartApplyGuard guard =
                MechBayMissionHandoffPreviewService.BuildRestartApplyGuard(
                    demoInventory,
                    mission?.Contract,
                    combatProfiles);
            bool previousEnabled = GUI.enabled;
            bool ready = guard?.ApplyEnabled == true;
            GUI.enabled = previousEnabled && ready;
            if (DrawActionButton(new Rect(x, y - 2f, 72f, 22f), "Launch", ready))
            {
                TryApplyMissionRestartRuntimeSwap(keepMechBayOpen: true);
            }

            GUI.enabled = previousEnabled;
            string text = HasSquadSelectionCompletedReplacement()
                ? MissionHandoffCompletedLaunchText(guard)
                : MissionHandoffLaunchActionText(guard, preview);
            string prefix = string.IsNullOrWhiteSpace(labelPrefix) ? "Next Mission  " : labelPrefix;
            DrawActionStateLabel(x + 80f, y, width - 80f, prefix + text, ready, 64);
        }

        private string MissionHandoffCompletedLaunchText(MechBayMissionRestartApplyGuard guard)
        {
            if (guard == null || !guard.ApplyEnabled)
            {
                return "Blocked  " + MissionHandoffPlayerBlockedReason(guard?.Reason);
            }

            return "Ready  launch updated squad";
        }

        private static string SquadSelectionSlotSummary(MechBaySquadSelectionSlot[] slots, string emptyText)
        {
            if (slots == null || slots.Length == 0)
            {
                return emptyText;
            }

            string text = "";
            int count = Math.Min(3, slots.Length);
            for (int index = 0; index < count; index++)
            {
                MechBaySquadSelectionSlot slot = slots[index];
                if (slot == null)
                {
                    continue;
                }

                if (text.Length > 0)
                {
                    text += "; ";
                }

                string name = string.IsNullOrWhiteSpace(slot.displayName) ? slot.unitType : slot.displayName;
                string pilot = string.IsNullOrWhiteSpace(slot.pilotDisplayName) ? "No pilot" : slot.pilotDisplayName;
                text += name
                    + " "
                    + slot.conditionPercent.ToString(CultureInfo.InvariantCulture)
                    + "% "
                    + pilot;
            }

            if (slots.Length > count)
            {
                text += " +" + (slots.Length - count).ToString(CultureInfo.InvariantCulture);
            }

            return text.Length == 0 ? emptyText : text;
        }

        private static bool SquadSelectionCompleted(MechBaySquadSelectionPreview preview)
        {
            return preview?.HasRefreshedMissionSlot == true && (preview.CandidateCount <= 0);
        }

        private static string SquadSelectionCompactLineupSummary(
            MechBaySquadSelectionSlot[] slots,
            string emptyText)
        {
            if (slots == null || slots.Length == 0)
            {
                return emptyText;
            }

            MechBaySquadSelectionSlot depotSlot = null;
            for (int index = 0; index < slots.Length; index++)
            {
                MechBaySquadSelectionSlot slot = slots[index];
                if (slot?.isDepotMissionSlot == true)
                {
                    depotSlot = slot;
                    break;
                }
            }

            string text = slots.Length.ToString(CultureInfo.InvariantCulture) + " mechs ready";
            if (depotSlot != null)
            {
                string name = string.IsNullOrWhiteSpace(depotSlot.displayName)
                    ? depotSlot.unitType
                    : depotSlot.displayName;
                text += "  " + name + " joined";
            }

            return text;
        }

        private void CycleSquadSelectionDraft(MechBaySquadSelectionSlot[] slots, bool outgoing, int direction)
        {
            MechBaySquadSelectionSlot[] safeSlots = slots ?? Array.Empty<MechBaySquadSelectionSlot>();
            if (safeSlots.Length == 0)
            {
                return;
            }

            string selectedOwnedMechId = outgoing
                ? squadSelectionDraftOutgoingOwnedMechId
                : squadSelectionDraftIncomingOwnedMechId;
            int selectedIndex = SquadSelectionSlotIndex(safeSlots, selectedOwnedMechId);
            if (selectedIndex < 0)
            {
                selectedIndex = 0;
            }
            else
            {
                selectedIndex = (selectedIndex + direction + safeSlots.Length) % safeSlots.Length;
            }

            MechBaySquadSelectionSlot selected = safeSlots[selectedIndex];
            if (outgoing)
            {
                squadSelectionDraftOutgoingOwnedMechId = selected?.ownedMechId;
                statusText = "Draft outgoing: " + TruncateText(SquadSelectionSlotName(selected), 24);
            }
            else
            {
                squadSelectionDraftIncomingOwnedMechId = selected?.ownedMechId;
                statusText = "Draft incoming: " + TruncateText(SquadSelectionSlotName(selected), 24);
            }
        }

        private static int SquadSelectionSlotIndex(MechBaySquadSelectionSlot[] slots, string ownedMechId)
        {
            if (slots == null || slots.Length == 0)
            {
                return -1;
            }

            if (string.IsNullOrWhiteSpace(ownedMechId))
            {
                return 0;
            }

            for (int index = 0; index < slots.Length; index++)
            {
                if (string.Equals(slots[index]?.ownedMechId, ownedMechId, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return 0;
        }

        private static string SquadSelectionSlotName(MechBaySquadSelectionSlot slot)
        {
            if (slot == null)
            {
                return "none";
            }

            return string.IsNullOrWhiteSpace(slot.displayName) ? slot.unitType ?? "mech" : slot.displayName;
        }

        private void DrawWarehouseDraftFitPreview(float x, float y, float width)
        {
            MechBayWarehouseDraftFitPreview preview =
                MechBayWarehouseDraftFitPreviewService.BuildPreview(demoInventory, warehouseDraftFitPreviewMechId);
            GUI.Box(new Rect(x, y, width, 104f), "Warehouse Draft Fit Preview");
            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && preview != null && preview.Ready;
            if (GUI.Button(new Rect(x + width - 112f, y + 4f, 48f, 22f), "Apply"))
            {
                MechBayWarehouseDraftFitApplyResult result =
                    MechBayWarehouseDraftFitPreviewService.TryApplyDemoFit(demoInventory, warehouseDraftFitPreviewMechId);
                RefreshDemoInventoryValidation();
                statusText = result?.Message ?? "Draft fit unavailable";
                if (result != null && result.Accepted)
                {
                    AddCombatLogLine("Mech bay " + result.Message + " for " + result.displayName);
                    showWarehouseDraftFitPreview = false;
                    warehouseDraftFitPreviewMechId = null;
                }
            }

            GUI.enabled = previousEnabled;
            if (GUI.Button(new Rect(x + width - 58f, y + 4f, 48f, 22f), "Hide"))
            {
                showWarehouseDraftFitPreview = false;
                warehouseDraftFitPreviewMechId = null;
                statusText = "Draft fit preview closed";
            }

            string mech = string.IsNullOrWhiteSpace(preview?.displayName) ? "Mech unavailable" : preview.displayName;
            string chassis = string.IsNullOrWhiteSpace(preview?.chassisId) ? "unknown" : preview.chassisId;
            string status = string.IsNullOrWhiteSpace(preview?.Status) ? "Preview unavailable" : preview.Status;
            GUI.Label(new Rect(x + 12f, y + 24f, width - 24f, 18f), TruncateText(mech + "  Chassis " + chassis + "  " + status, 76));

            string pilot = string.IsNullOrWhiteSpace(preview?.pilotDisplayName) ? "No pilot assigned" : preview.pilotDisplayName;
            string pilotStatus = string.IsNullOrWhiteSpace(preview?.pilotStatus) ? "Pilot required" : preview.pilotStatus;
            GUI.Label(new Rect(x + 12f, y + 44f, width - 24f, 18f), TruncateText("Pilot " + pilotStatus + " (" + pilot + ")", 76));

            string weapon = string.IsNullOrWhiteSpace(preview?.weaponDisplayName) ? "No spare weapon selected" : preview.weaponDisplayName;
            string stock = preview == null ? "0" : preview.spareWeaponStockCount.ToString(CultureInfo.InvariantCulture);
            GUI.Label(new Rect(x + 12f, y + 64f, width - 24f, 18f), TruncateText("Weapon " + weapon + "  spare " + stock, 76));

            string requirements = string.IsNullOrWhiteSpace(preview?.Requirements) ? "Requirements unknown" : preview.Requirements;
            string note = string.IsNullOrWhiteSpace(preview?.PreviewNote) ? "Preview only" : preview.PreviewNote;
            GUI.Label(new Rect(x + 12f, y + 84f, width - 24f, 18f), TruncateText(requirements + "  " + note, 76));
        }

        private static string ReceiptAssemblyLogText(MechBayMissionReceipt receipt)
        {
            if (receipt == null || receipt.AssembledMechCount <= 0)
            {
                return "";
            }

            return " built +" + receipt.AssembledMechCount.ToString(CultureInfo.InvariantCulture)
                + " " + AssemblyNameOrGeneric(receipt);
        }

        private static string ReceiptAssemblyText(MechBayMissionReceipt receipt)
        {
            if (receipt == null || receipt.AssembledMechCount <= 0)
            {
                return "";
            }

            string text = "Built +" + receipt.AssembledMechCount.ToString(CultureInfo.InvariantCulture)
                + " " + AssemblyNameOrGeneric(receipt);
            return text;
        }

        private static string AssemblyNameOrGeneric(MechBayMissionReceipt receipt)
        {
            if (receipt != null && receipt.AssembledMechCount > 1)
            {
                return "mechs";
            }

            string[] names = receipt?.AssembledMechNames ?? Array.Empty<string>();
            return names.Length == 0 || string.IsNullOrWhiteSpace(names[0]) ? "Mech" : names[0];
        }

        private void DrawMechConditionLine(UnitState unit, float left, float right, float y, float width)
        {
            int condition = MechConditionPercent(unit);
            int repairCost = MechBayRepairService.EstimateRepairCostResourcePoints(unit);
            GUI.Label(
                new Rect(left, y, width * 0.46f, 18f),
                "Condition " + condition.ToString(CultureInfo.InvariantCulture) + "%"
                + "  Repair " + FormatTokens(repairCost));

            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled
                && repairCost > 0
                && demoInventory != null
                && demoInventory.tokenBalance >= repairCost;
            if (GUI.Button(new Rect(right, y - 2f, 86f, 22f), "Repair"))
            {
                MechBayRepairResult result = MechBayRepairService.TryRepair(demoInventory, unit);
                RefreshDemoInventoryValidation();
                statusText = result.Message;
            }

            GUI.enabled = previousEnabled;
            if (repairCost > 0 && demoInventory != null && demoInventory.tokenBalance < repairCost)
            {
                GUI.Label(new Rect(right + 92f, y, width * 0.46f - 92f, 18f), "Need " + FormatTokens(repairCost));
            }
            else
            {
                GUI.Label(new Rect(right + 92f, y, width * 0.46f - 92f, 18f), UnitMissionStateText(unit));
            }
        }

        private static string UnitMissionStateText(UnitState unit)
        {
            if (unit == null)
            {
                return "State unknown";
            }

            if (unit.IsDestroyed)
            {
                return "Destroyed";
            }

            if (!unit.IsActive)
            {
                return "Held by trigger";
            }

            if (unit.IsJumping)
            {
                return "Jetting";
            }

            if (unit.IsDetached)
            {
                return "Solo order";
            }

            if (unit.HasAttackOrder)
            {
                return "Attacking";
            }

            if (unit.HasMoveOrder)
            {
                return "Moving";
            }

            return "Ready";
        }

        private void RefreshDemoInventoryValidation()
        {
            SyncDemoInventorySquadCondition();
            demoInventoryValidation = MechBayInventoryValidator.Validate(demoInventory);
        }

        private void SyncDemoInventorySquadCondition()
        {
            if (demoInventory?.ownedMechs == null || mission == null)
            {
                return;
            }

            foreach (UnitState unit in mission.PlayerUnits())
            {
                if (unit == null)
                {
                    continue;
                }

                MechBayOwnedMechDefinition mech = FindOwnedMechForRuntimeUnit(unit);
                if (mech == null)
                {
                    continue;
                }

                mech.unitId = unit.Id;
                mech.conditionPercent = MechConditionPercent(unit);
                mech.availableForMission = unit.IsActive && !unit.IsDestroyed;
            }
        }

        private MechBayOwnedMechDefinition FindOwnedMechForRuntimeUnit(UnitState unit)
        {
            if (unit == null || demoInventory?.ownedMechs == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(unit.OwnedMechId))
            {
                MechBayOwnedMechDefinition byOwnedId = FindOwnedMechByOwnedId(unit.OwnedMechId);
                if (byOwnedId != null)
                {
                    return byOwnedId;
                }
            }

            return FindOwnedMechByUnitId(unit.Id);
        }

        private MechBayOwnedMechDefinition FindOwnedMechByOwnedId(string ownedMechId)
        {
            if (string.IsNullOrWhiteSpace(ownedMechId) || demoInventory?.ownedMechs == null)
            {
                return null;
            }

            for (int index = 0; index < demoInventory.ownedMechs.Length; index++)
            {
                MechBayOwnedMechDefinition mech = demoInventory.ownedMechs[index];
                if (mech != null && string.Equals(mech.ownedMechId, ownedMechId, StringComparison.OrdinalIgnoreCase))
                {
                    return mech;
                }
            }

            return null;
        }

        private MechBayOwnedMechDefinition FindOwnedMechByUnitId(string unitId)
        {
            if (string.IsNullOrWhiteSpace(unitId) || demoInventory?.ownedMechs == null)
            {
                return null;
            }

            for (int index = 0; index < demoInventory.ownedMechs.Length; index++)
            {
                MechBayOwnedMechDefinition mech = demoInventory.ownedMechs[index];
                if (mech != null && string.Equals(mech.unitId, unitId, StringComparison.OrdinalIgnoreCase))
                {
                    return mech;
                }
            }

            return null;
        }

        private static int MechConditionPercent(UnitState unit)
        {
            if (unit?.Profile == null || unit.Profile.MaxStructure <= 0f)
            {
                return 0;
            }

            return Mathf.RoundToInt(Mathf.Clamp01(unit.CurrentStructure / unit.Profile.MaxStructure) * 100f);
        }

        private void DrawLoadoutSectionLine(UnitState unit, float x, float y, float width)
        {
            int count = Mathf.Max(1, unit.Sections.Length);
            float gap = 6f;
            float segmentWidth = (width - gap * (count - 1)) / count;
            for (int index = 0; index < unit.Sections.Length; index++)
            {
                DamageSection section = unit.Sections[index];
                float segmentX = x + index * (segmentWidth + gap);
                Rect back = new(segmentX, y, segmentWidth, 7f);
                DrawColorRect(back, new Color(0.08f, 0.09f, 0.10f, 1f));
                DrawColorRect(new Rect(segmentX, y, segmentWidth * section.Ratio, 7f), SectionHealthColor(section));
                string value = section.IsDestroyed ? "X" : Mathf.RoundToInt(section.Ratio * 100f).ToString(CultureInfo.InvariantCulture);
                GUI.Label(new Rect(segmentX, y + 8f, segmentWidth + 4f, 16f), ShortSectionName(section.Name) + " " + value);
            }
        }

        private float DrawProjectedLoadoutGrid(UnitState unit, CombatLoadoutPreview preview, float x, float y, float width)
        {
            if (preview == null || preview.GridWidth <= 0 || preview.GridHeight <= 0)
            {
                return 0f;
            }

            float gap = 2f;
            float maxGridWidth = Mathf.Min(width, 96f);
            float cellSize = Mathf.Clamp((maxGridWidth - gap * (preview.GridWidth - 1)) / preview.GridWidth, 16f, 22f);
            float gridWidth = preview.GridWidth * cellSize + (preview.GridWidth - 1) * gap;
            float gridHeight = preview.GridHeight * cellSize + (preview.GridHeight - 1) * gap;
            int selectedWeaponIndex = SelectedLoadoutWeaponIndexFor(unit, preview);
            DrawColorRect(new Rect(x, y, gridWidth + 2f, gridHeight + 2f), new Color(0.04f, 0.05f, 0.055f, 0.96f));

            for (int row = 0; row < preview.GridHeight; row++)
            {
                for (int column = 0; column < preview.GridWidth; column++)
                {
                    Rect cell = new(x + 1f + column * (cellSize + gap), y + 1f + row * (cellSize + gap), cellSize, cellSize);
                    CombatLoadoutPreviewGridCell occupiedCell = LoadoutCellAt(preview, column, row);
                    int cellStack = CountLoadoutCellsAt(preview, column, row);
                    Color fill = occupiedCell == null
                        ? new Color(0.10f, 0.12f, 0.13f, 1f)
                        : cellStack > 1
                            ? new Color(1f, 0.22f, 0.16f, 1f)
                            : LoadoutCellColor(occupiedCell);
                    fill.a = 1f;
                    DrawColorRect(cell, fill);
                    DrawColorRect(new Rect(cell.x, cell.y, cell.width, 1f), new Color(0.35f, 0.42f, 0.44f, 0.95f));
                    DrawColorRect(new Rect(cell.x, cell.yMax - 1f, cell.width, 1f), new Color(0.02f, 0.025f, 0.03f, 0.95f));

                    if (Event.current.type == EventType.MouseDown
                        && Event.current.button == 0
                        && cell.Contains(Event.current.mousePosition))
                    {
                        if (occupiedCell != null && occupiedCell.SourceWeaponIndex >= 0)
                        {
                            SetSelectedLoadoutWeapon(unit, occupiedCell.SourceWeaponIndex);
                            statusText = "Selected " + TruncateText(occupiedCell.DisplayName, 24);
                        }
                        else if (cellStack <= 1)
                        {
                            CycleFillerOverride(unit, column, row, occupiedCell?.Category);
                        }

                        Event.current.Use();
                    }

                    if (occupiedCell != null)
                    {
                        GUI.Label(
                            new Rect(cell.x + 4f, cell.y + 3f, cell.width - 6f, cell.height - 4f),
                            cellStack > 1 ? "!" : LoadoutCellLabel(occupiedCell));
                        if (occupiedCell.SourceWeaponIndex >= 0 && occupiedCell.SourceWeaponIndex == selectedWeaponIndex)
                        {
                            DrawRectBorder(cell, new Color(1f, 0.95f, 0.22f, 1f), 2f);
                        }
                    }
                }
            }

            GUI.Label(
                new Rect(x + gridWidth + 10f, y + 2f, Mathf.Max(120f, width - gridWidth - 10f), 18f),
                "Slots " + preview.Validation.OccupiedGridCells + "/" + preview.GridCapacity);
            GUI.Label(
                new Rect(x + gridWidth + 10f, y + 20f, Mathf.Max(120f, width - gridWidth - 10f), 18f),
                "Armor +" + FormatDecimal(preview.Validation.TotalArmorHardnessBonus)
                + "  Sink +" + FormatDecimal(preview.Validation.TotalHeatDissipationBonus));
            DrawLoadoutPlacementControls(unit, preview, x + gridWidth + 10f, y + 42f, Mathf.Max(160f, width - gridWidth - 10f));
            return Mathf.Max(gridHeight + 2f, 104f);
        }

        private static CombatLoadoutPreviewGridCell LoadoutCellAt(CombatLoadoutPreview preview, int x, int y)
        {
            CombatLoadoutPreviewGridCell[] occupiedCells = preview.OccupiedCells;
            for (int index = 0; index < occupiedCells.Length; index++)
            {
                CombatLoadoutPreviewGridCell cell = occupiedCells[index];
                if (cell != null && cell.X == x && cell.Y == y)
                {
                    return cell;
                }
            }

            return null;
        }

        private static Color LoadoutCellColor(CombatLoadoutPreviewGridCell cell)
        {
            if (cell == null)
            {
                return new Color(0.10f, 0.12f, 0.13f, 1f);
            }

            if (cell.Category == LoadoutItemCategory.ArmorPlate)
            {
                return new Color(0.40f, 0.58f, 0.52f, 1f);
            }

            if (cell.Category == LoadoutItemCategory.HeatSink)
            {
                return new Color(0.28f, 0.58f, 0.95f, 1f);
            }

            return WeaponColor(cell.WeaponType);
        }

        private static string LoadoutCellLabel(CombatLoadoutPreviewGridCell cell)
        {
            if (cell == null)
            {
                return "";
            }

            if (cell.Category == LoadoutItemCategory.ArmorPlate)
            {
                return "A";
            }

            if (cell.Category == LoadoutItemCategory.HeatSink)
            {
                return "H";
            }

            return cell.SourceWeaponIndex >= 0
                ? (cell.SourceWeaponIndex + 1).ToString(CultureInfo.InvariantCulture)
                : "+";
        }

        private static int CountLoadoutCellsAt(CombatLoadoutPreview preview, int x, int y)
        {
            int count = 0;
            CombatLoadoutPreviewGridCell[] occupiedCells = preview.OccupiedCells;
            for (int index = 0; index < occupiedCells.Length; index++)
            {
                CombatLoadoutPreviewGridCell cell = occupiedCells[index];
                if (cell != null && cell.X == x && cell.Y == y)
                {
                    count++;
                }
            }

            return count;
        }

        private void DrawLoadoutPlacementControls(UnitState unit, CombatLoadoutPreview preview, float x, float y, float width)
        {
            int selectedWeaponIndex = SelectedLoadoutWeaponIndexFor(unit, preview);
            CombatLoadoutPreviewItem selectedItem = LoadoutPreviewItemForWeapon(preview, selectedWeaponIndex);
            if (selectedItem == null)
            {
                GUI.Label(new Rect(x, y, width, 18f), "Select an enabled weapon");
                return;
            }

            GUI.Label(
                new Rect(x, y, width, 18f),
                "Edit " + (selectedWeaponIndex + 1).ToString(CultureInfo.InvariantCulture)
                + " " + TruncateText(selectedItem.DisplayName, 18)
                + " @ " + selectedItem.GridX.ToString(CultureInfo.InvariantCulture)
                + "," + selectedItem.GridY.ToString(CultureInfo.InvariantCulture));

            if (GUI.Button(new Rect(x + 32f, y + 24f, 40f, 22f), "Up"))
            {
                MoveSelectedLoadoutWeapon(unit, preview, 0, -1);
            }

            if (GUI.Button(new Rect(x, y + 50f, 40f, 22f), "Left"))
            {
                MoveSelectedLoadoutWeapon(unit, preview, -1, 0);
            }

            if (GUI.Button(new Rect(x + 44f, y + 50f, 50f, 22f), "Reset"))
            {
                ResetSelectedLoadoutWeapon(unit);
            }

            if (GUI.Button(new Rect(x + 98f, y + 50f, 44f, 22f), "Right"))
            {
                MoveSelectedLoadoutWeapon(unit, preview, 1, 0);
            }

            if (GUI.Button(new Rect(x + 32f, y + 76f, 40f, 22f), "Down"))
            {
                MoveSelectedLoadoutWeapon(unit, preview, 0, 1);
            }
        }

        private static CombatLoadoutPreviewItem LoadoutPreviewItemForWeapon(CombatLoadoutPreview preview, int sourceWeaponIndex)
        {
            if (preview == null || sourceWeaponIndex < 0)
            {
                return null;
            }

            CombatLoadoutPreviewItem[] items = preview.Items;
            for (int index = 0; index < items.Length; index++)
            {
                CombatLoadoutPreviewItem item = items[index];
                if (item != null && item.SourceWeaponIndex == sourceWeaponIndex)
                {
                    return item;
                }
            }

            return null;
        }

        private float DrawWeaponLoadoutLines(UnitState unit, float x, float y, float width)
        {
            CombatWeaponDefinition[] weapons = unit?.Profile?.Weapons;
            if (weapons == null || weapons.Length == 0)
            {
                GUI.Label(new Rect(x, y, width, 18f), "Weapons: aggregate profile");
                return 24f;
            }

            int count = Mathf.Min(8, weapons.Length);
            int columns = Mathf.Min(4, count);
            int rows = Mathf.CeilToInt(count / (float)columns);
            float columnWidth = width / columns;
            bool[] enabledWeapons = WeaponEnabledStateFor(unit);
            for (int index = 0; index < count; index++)
            {
                CombatWeaponDefinition weapon = weapons[index];
                if (weapon == null)
                {
                    continue;
                }

                int row = index / columns;
                int column = index % columns;
                float columnX = x + column * columnWidth;
                float rowY = y + row * 26f;
                string label = (index + 1).ToString(CultureInfo.InvariantCulture) + " " + TruncateText(weapon.name, 7)
                    + " H" + FormatDecimal(weapon.heat)
                    + " W" + FormatDecimal(weapon.weight);
                if (GUI.Button(new Rect(columnX, rowY - 2f, columnWidth - 52f, 22f), label))
                {
                    SetSelectedLoadoutWeapon(unit, index);
                    statusText = "Selected " + TruncateText(weapon.name, 24);
                }

                if (GUI.Button(new Rect(columnX + columnWidth - 46f, rowY - 2f, 42f, 22f), enabledWeapons[index] ? "On" : "Off"))
                {
                    enabledWeapons[index] = !enabledWeapons[index];
                    statusText = (enabledWeapons[index] ? "Enabled " : "Disabled ") + TruncateText(weapon.name, 20);
                }
            }

            return rows * 26f;
        }

        private void DrawLoadoutEditControls(UnitState unit, CombatLoadoutPreview preview, float x, float y, float width)
        {
            bool hasPendingEdits = HasPendingLoadoutEdits(unit);
            MechBayInventoryAvailabilityResult availability = CurrentDraftInventoryAvailability();
            bool hasInventory = availability != null && availability.IsValid;
            Color previousColor = GUI.color;
            GUI.color = !hasInventory
                ? new Color(1f, 0.78f, 0.28f, 1f)
                : hasPendingEdits
                    ? new Color(1f, 0.86f, 0.32f, 1f)
                    : new Color(0.58f, 0.82f, 1f, 1f);
            GUI.Label(
                new Rect(x, y, width - 146f, 18f),
                !hasInventory
                    ? "Inventory short: " + TruncateText(FirstInventoryAvailabilityError(availability), 24)
                    : hasPendingEdits ? "Draft fit" : "Applied fit");
            GUI.color = previousColor;

            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && hasPendingEdits && preview != null && preview.Validation.IsValid && hasInventory;
            if (GUI.Button(new Rect(x + width - 144f, y - 2f, 66f, 22f), "Apply"))
            {
                ApplyLoadoutDraft(unit, preview);
            }

            GUI.enabled = previousEnabled && hasPendingEdits;
            if (GUI.Button(new Rect(x + width - 72f, y - 2f, 64f, 22f), "Reset"))
            {
                ResetLoadoutDraft(unit);
            }

            GUI.enabled = previousEnabled;
        }

        private bool[] WeaponEnabledStateFor(UnitState unit)
        {
            string key = unit?.Id ?? "";
            int weaponCount = unit?.Profile?.Weapons?.Length ?? 0;
            if (!loadoutWeaponEnabledByUnit.TryGetValue(key, out bool[] enabledWeapons) || enabledWeapons.Length != weaponCount)
            {
                enabledWeapons = new bool[weaponCount];
                for (int index = 0; index < enabledWeapons.Length; index++)
                {
                    enabledWeapons[index] = true;
                }

                loadoutWeaponEnabledByUnit[key] = enabledWeapons;
            }

            return enabledWeapons;
        }

        private CombatLoadoutPlacementOverride[] LoadoutPlacementOverridesFor(UnitState unit)
        {
            if (unit == null)
            {
                return null;
            }

            string key = unit.Id ?? "";
            int weaponCount = unit.Profile?.Weapons?.Length ?? 0;
            if (!loadoutPlacementOverridesByUnit.TryGetValue(key, out CombatLoadoutPlacementOverride[] placementOverrides)
                || placementOverrides.Length != weaponCount)
            {
                placementOverrides = new CombatLoadoutPlacementOverride[weaponCount];
                loadoutPlacementOverridesByUnit[key] = placementOverrides;
            }

            return placementOverrides;
        }

        private CombatLoadoutFillerOverride[] LoadoutFillerOverridesFor(UnitState unit)
        {
            if (unit == null)
            {
                return null;
            }

            if (!loadoutFillerOverridesByUnit.TryGetValue(unit.Id ?? "", out List<CombatLoadoutFillerOverride> fillerOverrides)
                || fillerOverrides.Count == 0)
            {
                return null;
            }

            return fillerOverrides.ToArray();
        }

        private int SelectedLoadoutWeaponIndexFor(UnitState unit, CombatLoadoutPreview preview)
        {
            if (unit == null || preview == null || preview.Items.Length == 0)
            {
                return -1;
            }

            string key = unit.Id ?? "";
            int weaponCount = unit.Profile?.Weapons?.Length ?? 0;
            if (!selectedLoadoutWeaponByUnit.TryGetValue(key, out int selectedWeaponIndex)
                || selectedWeaponIndex < 0
                || selectedWeaponIndex >= weaponCount)
            {
                selectedWeaponIndex = preview.Items[0].SourceWeaponIndex;
                selectedLoadoutWeaponByUnit[key] = selectedWeaponIndex;
            }

            return selectedWeaponIndex;
        }

        private void SetSelectedLoadoutWeapon(UnitState unit, int sourceWeaponIndex)
        {
            if (unit == null || sourceWeaponIndex < 0)
            {
                return;
            }

            selectedLoadoutWeaponByUnit[unit.Id ?? ""] = sourceWeaponIndex;
        }

        private void MoveSelectedLoadoutWeapon(UnitState unit, CombatLoadoutPreview preview, int deltaX, int deltaY)
        {
            int selectedWeaponIndex = SelectedLoadoutWeaponIndexFor(unit, preview);
            CombatLoadoutPreviewItem selectedItem = LoadoutPreviewItemForWeapon(preview, selectedWeaponIndex);
            if (selectedItem == null)
            {
                statusText = "Select an enabled weapon first";
                return;
            }

            CombatLoadoutPlacementOverride[] placementOverrides = LoadoutPlacementOverridesFor(unit);
            placementOverrides[selectedWeaponIndex] = new CombatLoadoutPlacementOverride
            {
                sourceWeaponIndex = selectedWeaponIndex,
                gridX = selectedItem.GridX + deltaX,
                gridY = selectedItem.GridY + deltaY
            };
            statusText = "Moved " + TruncateText(selectedItem.DisplayName, 20);
        }

        private void ResetSelectedLoadoutWeapon(UnitState unit)
        {
            int selectedWeaponIndex = SelectedLoadoutWeaponIndexFor(unit, LoadoutPreviewFor(unit));
            CombatLoadoutPlacementOverride[] placementOverrides = LoadoutPlacementOverridesFor(unit);
            if (placementOverrides == null || selectedWeaponIndex < 0 || selectedWeaponIndex >= placementOverrides.Length)
            {
                return;
            }

            placementOverrides[selectedWeaponIndex] = null;
            statusText = "Reset temporary slot";
        }

        private bool HasPendingLoadoutEdits(UnitState unit)
        {
            if (unit == null)
            {
                return false;
            }

            string key = unit.Id ?? "";
            int weaponCount = unit.Profile?.Weapons?.Length ?? 0;
            return !WeaponStatesEqual(WeaponEnabledStateFor(unit), AppliedWeaponStateFor(key), weaponCount)
                || !PlacementStatesEqual(LoadoutPlacementOverridesFor(unit), AppliedPlacementStateFor(key), weaponCount)
                || !FillerStatesEqual(LoadoutFillerOverridesFor(unit), AppliedFillerStateFor(key));
        }

        private void ApplyLoadoutDraft(UnitState unit, CombatLoadoutPreview preview)
        {
            if (unit == null || preview == null)
            {
                return;
            }

            if (!preview.Validation.IsValid)
            {
                statusText = "Fix fit before apply";
                return;
            }

            MechBayInventoryAvailabilityResult availability = CurrentDraftInventoryAvailability();
            if (availability == null || !availability.IsValid)
            {
                statusText = "Inventory short: " + FirstInventoryAvailabilityError(availability);
                return;
            }

            string key = unit.Id ?? "";
            int weaponCount = unit.Profile?.Weapons?.Length ?? 0;
            appliedLoadoutWeaponEnabledByUnit[key] = CloneWeaponState(WeaponEnabledStateFor(unit), weaponCount);
            appliedLoadoutPlacementOverridesByUnit[key] = ClonePlacementOverrides(LoadoutPlacementOverridesFor(unit), weaponCount);
            appliedLoadoutFillerOverridesByUnit[key] = CloneFillerOverrides(LoadoutFillerOverridesFor(unit));
            unit.ApplyDemoLoadout(BuildAppliedLoadoutCombatOverride(unit, preview));
            statusText = "Applied demo fit";
        }

        private void ResetLoadoutDraft(UnitState unit)
        {
            if (unit == null)
            {
                return;
            }

            string key = unit.Id ?? "";
            int weaponCount = unit.Profile?.Weapons?.Length ?? 0;
            bool hasAppliedState = HasAppliedLoadoutState(key);
            loadoutWeaponEnabledByUnit[key] = CloneWeaponState(AppliedWeaponStateFor(key), weaponCount);
            loadoutPlacementOverridesByUnit[key] = ClonePlacementOverrides(AppliedPlacementStateFor(key), weaponCount);
            CombatLoadoutFillerOverride[] appliedFillers = AppliedFillerStateFor(key);
            if (appliedFillers == null || appliedFillers.Length == 0)
            {
                loadoutFillerOverridesByUnit.Remove(key);
            }
            else
            {
                loadoutFillerOverridesByUnit[key] = new List<CombatLoadoutFillerOverride>(CloneFillerOverrides(appliedFillers));
            }

            if (hasAppliedState)
            {
                unit.ApplyDemoLoadout(BuildAppliedLoadoutCombatOverride(unit, LoadoutPreviewFor(unit)));
            }
            else
            {
                unit.ClearDemoLoadout();
            }

            statusText = "Reset draft fit";
        }

        private UnitLoadoutCombatOverride BuildAppliedLoadoutCombatOverride(UnitState unit, CombatLoadoutPreview preview)
        {
            CombatProfile profile = unit?.Profile;
            if (profile == null)
            {
                return null;
            }

            CombatWeaponDefinition[] weapons = profile.Weapons ?? Array.Empty<CombatWeaponDefinition>();
            bool[] enabledWeapons = WeaponEnabledStateFor(unit);
            int enabledCount = 0;
            bool allWeaponsEnabled = weapons.Length > 0;
            float enabledDamage = 0f;
            float totalDamage = 0f;
            float enabledHeat = 0f;
            float enabledWeight = 0f;
            float enabledRange = 0f;
            float cooldownWeightedTotal = 0f;
            float cooldownWeight = 0f;
            CombatWeaponDefinition primaryWeapon = null;
            float primaryScore = float.MinValue;
            for (int index = 0; index < weapons.Length; index++)
            {
                CombatWeaponDefinition weapon = weapons[index];
                if (weapon == null)
                {
                    continue;
                }

                totalDamage += Mathf.Max(0f, weapon.damage);
                bool enabled = WeaponEnabledAt(enabledWeapons, index);
                if (!enabled)
                {
                    allWeaponsEnabled = false;
                    continue;
                }

                enabledCount++;
                enabledDamage += Mathf.Max(0f, weapon.damage);
                enabledHeat += Mathf.Max(0f, weapon.heat);
                enabledWeight += Mathf.Max(0f, weapon.weight);
                enabledRange = Mathf.Max(enabledRange, Mathf.Max(0f, weapon.rangeMax));
                float weight = Mathf.Max(1f, weapon.damage);
                if (weapon.recycleTime > 0f)
                {
                    cooldownWeightedTotal += weapon.recycleTime * weight;
                    cooldownWeight += weight;
                }

                float score = weapon.damagePerTenSeconds > 0f
                    ? weapon.damagePerTenSeconds
                    : weapon.damage;
                score += Mathf.Max(0f, weapon.rangeMax) * 0.001f;
                if (score > primaryScore)
                {
                    primaryScore = score;
                    primaryWeapon = weapon;
                }
            }

            float heatSinkBonus = preview?.Validation.TotalHeatDissipationBonus ?? 0f;
            float armorHardnessBonus = preview?.Validation.TotalArmorHardnessBonus ?? 0f;
            if (enabledCount <= 0)
            {
                return new UnitLoadoutCombatOverride
                {
                    weaponRange = 0f,
                    weaponDamage = 0f,
                    weaponCooldown = profile.WeaponCooldown,
                    heatPerShot = 0f,
                    heatDissipationPerSecond = profile.HeatDissipationPerSecond + heatSinkBonus,
                    armorHardnessBonus = armorHardnessBonus,
                    totalWeaponWeight = preview?.Validation.TotalWeight ?? 0f,
                    primaryWeaponName = "No Weapons",
                    primaryWeaponType = "Generic",
                    primarySpecialEffect = 0
                };
            }

            float weaponDamage = allWeaponsEnabled || totalDamage <= 0f
                ? profile.WeaponDamage
                : profile.WeaponDamage * Mathf.Clamp01(enabledDamage / totalDamage);
            return new UnitLoadoutCombatOverride
            {
                weaponRange = allWeaponsEnabled ? profile.WeaponRange : enabledRange,
                weaponDamage = weaponDamage,
                weaponCooldown = allWeaponsEnabled || cooldownWeight <= 0f
                    ? profile.WeaponCooldown
                    : cooldownWeightedTotal / cooldownWeight,
                heatPerShot = allWeaponsEnabled ? profile.HeatPerShot : enabledHeat,
                heatDissipationPerSecond = profile.HeatDissipationPerSecond + heatSinkBonus,
                armorHardnessBonus = armorHardnessBonus,
                totalWeaponWeight = preview?.Validation.TotalWeight ?? enabledWeight,
                primaryWeaponName = allWeaponsEnabled
                    ? profile.PrimaryWeaponName
                    : string.IsNullOrEmpty(primaryWeapon?.name) ? "Enabled Weapons" : primaryWeapon.name,
                primaryWeaponType = allWeaponsEnabled
                    ? profile.PrimaryWeaponType
                    : string.IsNullOrEmpty(primaryWeapon?.type) ? "Generic" : primaryWeapon.type,
                primarySpecialEffect = allWeaponsEnabled
                    ? profile.PrimarySpecialEffect
                    : primaryWeapon?.specialEffect ?? 0
            };
        }

        private bool HasAppliedLoadoutState(string key)
        {
            key ??= "";
            return appliedLoadoutWeaponEnabledByUnit.ContainsKey(key)
                || appliedLoadoutPlacementOverridesByUnit.ContainsKey(key)
                || appliedLoadoutFillerOverridesByUnit.ContainsKey(key);
        }

        private bool[] AppliedWeaponStateFor(string key)
        {
            return appliedLoadoutWeaponEnabledByUnit.TryGetValue(key ?? "", out bool[] appliedWeapons)
                ? appliedWeapons
                : null;
        }

        private CombatLoadoutPlacementOverride[] AppliedPlacementStateFor(string key)
        {
            return appliedLoadoutPlacementOverridesByUnit.TryGetValue(key ?? "", out CombatLoadoutPlacementOverride[] appliedPlacements)
                ? appliedPlacements
                : null;
        }

        private CombatLoadoutFillerOverride[] AppliedFillerStateFor(string key)
        {
            return appliedLoadoutFillerOverridesByUnit.TryGetValue(key ?? "", out CombatLoadoutFillerOverride[] appliedFillers)
                ? appliedFillers
                : null;
        }

        private static bool WeaponStatesEqual(bool[] currentState, bool[] appliedState, int weaponCount)
        {
            for (int index = 0; index < weaponCount; index++)
            {
                if (WeaponEnabledAt(currentState, index) != WeaponEnabledAt(appliedState, index))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool WeaponEnabledAt(bool[] state, int index)
        {
            return state == null || index >= state.Length || state[index];
        }

        private static bool PlacementStatesEqual(
            CombatLoadoutPlacementOverride[] currentState,
            CombatLoadoutPlacementOverride[] appliedState,
            int weaponCount)
        {
            for (int index = 0; index < weaponCount; index++)
            {
                CombatLoadoutPlacementOverride current = PlacementAt(currentState, index);
                CombatLoadoutPlacementOverride applied = PlacementAt(appliedState, index);
                if (!PlacementEquals(current, applied))
                {
                    return false;
                }
            }

            return true;
        }

        private static CombatLoadoutPlacementOverride PlacementAt(CombatLoadoutPlacementOverride[] state, int index)
        {
            return state != null && index >= 0 && index < state.Length ? state[index] : null;
        }

        private static bool PlacementEquals(CombatLoadoutPlacementOverride current, CombatLoadoutPlacementOverride applied)
        {
            if (current == null || applied == null)
            {
                return current == null && applied == null;
            }

            return current.sourceWeaponIndex == applied.sourceWeaponIndex
                && current.gridX == applied.gridX
                && current.gridY == applied.gridY;
        }

        private static bool FillerStatesEqual(CombatLoadoutFillerOverride[] currentState, CombatLoadoutFillerOverride[] appliedState)
        {
            int currentCount = currentState?.Length ?? 0;
            int appliedCount = appliedState?.Length ?? 0;
            if (currentCount != appliedCount)
            {
                return false;
            }

            for (int index = 0; index < currentCount; index++)
            {
                CombatLoadoutFillerOverride current = currentState[index];
                CombatLoadoutFillerOverride applied = FindFillerOverride(appliedState, current?.gridX ?? -1, current?.gridY ?? -1);
                if (!FillerEquals(current, applied))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool FillerEquals(CombatLoadoutFillerOverride current, CombatLoadoutFillerOverride applied)
        {
            if (current == null || applied == null)
            {
                return current == null && applied == null;
            }

            return current.gridX == applied.gridX
                && current.gridY == applied.gridY
                && current.category == applied.category;
        }

        private static bool[] CloneWeaponState(bool[] source, int weaponCount)
        {
            bool[] clone = new bool[weaponCount];
            for (int index = 0; index < clone.Length; index++)
            {
                clone[index] = WeaponEnabledAt(source, index);
            }

            return clone;
        }

        private static CombatLoadoutPlacementOverride[] ClonePlacementOverrides(
            CombatLoadoutPlacementOverride[] source,
            int weaponCount)
        {
            CombatLoadoutPlacementOverride[] clone = new CombatLoadoutPlacementOverride[weaponCount];
            for (int index = 0; index < clone.Length; index++)
            {
                CombatLoadoutPlacementOverride sourcePlacement = PlacementAt(source, index);
                if (sourcePlacement == null)
                {
                    continue;
                }

                clone[index] = new CombatLoadoutPlacementOverride
                {
                    sourceWeaponIndex = sourcePlacement.sourceWeaponIndex,
                    gridX = sourcePlacement.gridX,
                    gridY = sourcePlacement.gridY
                };
            }

            return clone;
        }

        private static CombatLoadoutFillerOverride[] CloneFillerOverrides(CombatLoadoutFillerOverride[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<CombatLoadoutFillerOverride>();
            }

            CombatLoadoutFillerOverride[] clone = new CombatLoadoutFillerOverride[source.Length];
            for (int index = 0; index < source.Length; index++)
            {
                CombatLoadoutFillerOverride sourceFiller = source[index];
                if (sourceFiller == null)
                {
                    continue;
                }

                clone[index] = new CombatLoadoutFillerOverride
                {
                    gridX = sourceFiller.gridX,
                    gridY = sourceFiller.gridY,
                    category = sourceFiller.category
                };
            }

            return clone;
        }

        private void CycleFillerOverride(UnitState unit, int gridX, int gridY, string currentCategory)
        {
            if (unit == null)
            {
                return;
            }

            List<CombatLoadoutFillerOverride> fillerOverrides = MutableLoadoutFillerOverridesFor(unit);
            CombatLoadoutFillerOverride fillerOverride = FindFillerOverride(fillerOverrides, gridX, gridY);
            string nextCategory = NextFillerCategory(currentCategory);
            if (fillerOverride == null)
            {
                fillerOverrides.Add(new CombatLoadoutFillerOverride
                {
                    gridX = gridX,
                    gridY = gridY,
                    category = nextCategory
                });
            }
            else
            {
                fillerOverride.category = nextCategory;
            }

            statusText = "Filler " + FillerStatusName(nextCategory);
        }

        private List<CombatLoadoutFillerOverride> MutableLoadoutFillerOverridesFor(UnitState unit)
        {
            string key = unit?.Id ?? "";
            if (!loadoutFillerOverridesByUnit.TryGetValue(key, out List<CombatLoadoutFillerOverride> fillerOverrides))
            {
                fillerOverrides = new List<CombatLoadoutFillerOverride>();
                loadoutFillerOverridesByUnit[key] = fillerOverrides;
            }

            return fillerOverrides;
        }

        private static CombatLoadoutFillerOverride FindFillerOverride(
            List<CombatLoadoutFillerOverride> fillerOverrides,
            int gridX,
            int gridY)
        {
            if (fillerOverrides == null)
            {
                return null;
            }

            for (int index = 0; index < fillerOverrides.Count; index++)
            {
                CombatLoadoutFillerOverride fillerOverride = fillerOverrides[index];
                if (fillerOverride != null && fillerOverride.gridX == gridX && fillerOverride.gridY == gridY)
                {
                    return fillerOverride;
                }
            }

            return null;
        }

        private static CombatLoadoutFillerOverride FindFillerOverride(
            CombatLoadoutFillerOverride[] fillerOverrides,
            int gridX,
            int gridY)
        {
            if (fillerOverrides == null)
            {
                return null;
            }

            for (int index = 0; index < fillerOverrides.Length; index++)
            {
                CombatLoadoutFillerOverride fillerOverride = fillerOverrides[index];
                if (fillerOverride != null && fillerOverride.gridX == gridX && fillerOverride.gridY == gridY)
                {
                    return fillerOverride;
                }
            }

            return null;
        }

        private static string NextFillerCategory(string currentCategory)
        {
            if (currentCategory == LoadoutItemCategory.ArmorPlate)
            {
                return LoadoutItemCategory.HeatSink;
            }

            if (currentCategory == LoadoutItemCategory.HeatSink)
            {
                return LoadoutFillerOverrideCategory.Empty;
            }

            return LoadoutItemCategory.ArmorPlate;
        }

        private static string FillerStatusName(string category)
        {
            if (category == LoadoutItemCategory.ArmorPlate)
            {
                return "Armor";
            }

            if (category == LoadoutItemCategory.HeatSink)
            {
                return "Heat Sink";
            }

            return "Empty";
        }

        private void DrawSystemPanel()
        {
            if (!showSystemPanel)
            {
                return;
            }

            Rect panel = SystemPanelRect();
            GUI.Box(panel, "System");
            GUI.Label(new Rect(panel.x + 18f, panel.y + 36f, panel.width - 36f, 24f), isPaused ? "Paused" : "Running");

            if (GUI.Button(new Rect(panel.x + 18f, panel.y + 70f, panel.width - 36f, 30f), isPaused ? "Resume" : "Pause"))
            {
                if (mission.Result == MissionResultState.InProgress)
                {
                    SetPaused(!isPaused);
                    statusText = isPaused ? "Paused" : "Resumed";
                }
                else
                {
                    statusText = MissionResultText();
                }
            }

            if (GUI.Button(new Rect(panel.x + 18f, panel.y + 108f, panel.width - 36f, 30f), "Restart Mission"))
            {
                TryApplyMissionRestartRuntimeSwap();
            }

            if (GUI.Button(new Rect(panel.x + 18f, panel.y + 146f, panel.width - 36f, 30f), "End Demo"))
            {
                Application.Quit(0);
            }

            if (GUI.Button(new Rect(panel.x + 18f, panel.y + 184f, panel.width - 36f, 30f), "Close"))
            {
                showSystemPanel = false;
                if (mission.Result == MissionResultState.InProgress)
                {
                    SetPaused(false);
                }

                statusText = "System closed";
            }
        }

        private void DrawMissionResultPanel()
        {
            if (mission.Result == MissionResultState.InProgress)
            {
                return;
            }

            Rect panel = new((Screen.width - 400f) * 0.5f, 72f, 400f, 338f);
            GUI.Box(panel, MissionResultText());
            GUI.Label(new Rect(panel.x + 18f, panel.y + 36f, panel.width - 36f, 42f), mission.ResultReason);
            DrawMissionResultSummary(panel, mission.ResultSummary);

            if (GUI.Button(new Rect(panel.x + 18f, panel.y + 278f, 172f, 30f), "Restart"))
            {
                TryApplyMissionRestartRuntimeSwap();
            }

            if (GUI.Button(new Rect(panel.x + 210f, panel.y + 278f, 172f, 30f), "End Demo"))
            {
                Application.Quit(0);
            }
        }

        private void DrawMissionResultSummary(Rect panel, MissionResultSummary summary)
        {
            if (summary == null)
            {
                return;
            }

            float y = panel.y + 82f;
            GUI.Label(
                new Rect(panel.x + 18f, y, panel.width - 36f, 20f),
                "Objectives " + summary.completedVisibleObjectives + "/" + summary.visibleObjectives
                + "    Structures " + summary.destroyedStructures);
            y += 22f;

            GUI.Label(
                new Rect(panel.x + 18f, y, panel.width - 36f, 20f),
                "Enemy kills " + summary.destroyedEnemyUnits
                + "    Player damage " + summary.damagedPlayerUnits);
            y += 22f;

            GUI.Label(
                new Rect(panel.x + 18f, y, panel.width - 36f, 20f),
                "Token " + SignedTokens(summary.completedRewardResourcePoints)
                + "    Repair " + SignedTokens(-summary.repairCostResourcePoints)
                + "    Net " + SignedTokens(summary.netResourcePoints));
            y += 22f;

            GUI.Label(
                new Rect(panel.x + 18f, y, panel.width - 36f, 20f),
                "Salvage claims " + summary.salvageClaimCount
                + "    Total bounty " + FormatTokens(summary.visibleRewardResourcePoints));
            y += 22f;

            if (missionReceipt != null)
            {
                GUI.Label(
                    new Rect(panel.x + 18f, y, panel.width - 36f, 20f),
                    "Receipt Token " + SignedTokens(missionReceipt.TokenDelta)
                    + "    Frags +" + missionReceipt.SalvageFragmentCount
                    + "    Balance " + FormatTokens(missionReceipt.TokenBalance));
                y += 22f;

                string assemblyText = ReceiptAssemblyText(missionReceipt);
                if (!string.IsNullOrEmpty(assemblyText))
                {
                    GUI.Label(new Rect(panel.x + 18f, y, panel.width - 36f, 20f), assemblyText);
                    y += 22f;
                }

                GUI.Label(
                    new Rect(panel.x + 18f, y, panel.width - 36f, 20f),
                    "Assembly " + AssemblyPreviewText(MechBayAssemblyPreviewService.BestAssemblyProgress(demoInventory)));
                y += 22f;
            }

            string completed = FirstSummaryItem(summary.completedVisibleObjectiveTitles);
            if (!string.IsNullOrEmpty(completed))
            {
                GUI.Label(new Rect(panel.x + 18f, y, panel.width - 36f, 20f), "Done: " + TruncateText(completed, 42));
                y += 20f;
            }

            string damaged = FirstSummaryItem(summary.damagedPlayerUnitLabels);
            if (!string.IsNullOrEmpty(damaged))
            {
                GUI.Label(new Rect(panel.x + 18f, y, panel.width - 36f, 20f), "Damage: " + TruncateText(damaged, 40));
            }
        }

        private static string FirstSummaryItem(string[] values)
        {
            return values == null || values.Length == 0 ? "" : values[0];
        }

        private static string FormatTokens(int value)
        {
            return value.ToString("N0", CultureInfo.InvariantCulture);
        }

        private static string FormatDecimal(float value)
        {
            return value.ToString("0.#", CultureInfo.InvariantCulture);
        }

        private static string SignedTokens(int value)
        {
            return (value >= 0 ? "+" : "") + FormatTokens(value);
        }

        private string MissionResultText()
        {
            if (mission.Result == MissionResultState.Victory)
            {
                return "Mission Complete";
            }

            if (mission.Result == MissionResultState.Defeat)
            {
                return "Mission Failed";
            }

            return isPaused ? "Paused" : "Running";
        }

        private void OpenSystemPanel()
        {
            showSystemPanel = true;
            showLoadoutPanel = false;
            showWarehouseDraftFitPreview = false;
            showSquadSelectionPreview = false;
            warehouseDraftFitPreviewMechId = null;
            ClearSquadSelectionDraft();
            ClearSquadSelectionCompletedReplacement();
            showMissionMap = false;
            if (mission.Result == MissionResultState.InProgress)
            {
                SetPaused(true);
            }

            pendingDetachedUnitId = null;
            pendingJumpOrder = false;
            statusText = "System open";
        }

        private void OpenLoadoutPanel()
        {
            showLoadoutPanel = true;
            showWarehouseDraftFitPreview = false;
            showSquadSelectionPreview = false;
            warehouseDraftFitPreviewMechId = null;
            ClearSquadSelectionDraft();
            ClearSquadSelectionCompletedReplacement();
            showMissionMap = false;
            showSystemPanel = false;
            if (mission.Result == MissionResultState.InProgress)
            {
                SetPaused(true);
            }

            pendingDetachedUnitId = null;
            pendingJumpOrder = false;
            RefreshDemoInventoryValidation();
            statusText = "Mech bay open";
        }

        private void ClearSquadSelectionDraft()
        {
            squadSelectionDraftOutgoingOwnedMechId = null;
            squadSelectionDraftIncomingOwnedMechId = null;
        }

        private void SetPaused(bool paused)
        {
            isPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
        }

        private bool IsGuiPointBlocked(Vector2 guiPoint)
        {
            if (guiPoint.x < 360f)
            {
                return true;
            }

            if (CombatPanelRect().Contains(guiPoint))
            {
                return true;
            }

            if (ShouldDrawMissionBriefPanel() && MissionBriefPanelRect().Contains(guiPoint))
            {
                return true;
            }

            if (showMissionMap && MissionMapRect().Contains(guiPoint))
            {
                return true;
            }

            if (showLoadoutPanel && LoadoutPanelRect().Contains(guiPoint))
            {
                return true;
            }

            return showSystemPanel && SystemPanelRect().Contains(guiPoint);
        }

        private Rect MissionMapRect()
        {
            float width = Mathf.Clamp(Screen.width * 0.32f, 280f, 390f);
            float height = Mathf.Clamp(Screen.height * 0.38f, 230f, 330f);
            return new Rect(Screen.width - width - 16f, Screen.height - height - 16f, width, height);
        }

        private Rect SystemPanelRect()
        {
            float width = 260f;
            float height = 232f;
            return new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        }

        private Rect LoadoutPanelRect()
        {
            float width = Mathf.Clamp(Screen.width * 0.54f, 560f, 760f);
            float height = Mathf.Clamp(Screen.height * 0.88f, 420f, 720f);
            return new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        }

        private Rect CombatPanelRect()
        {
            return new Rect(Screen.width - 360f, 12f, 344f, 178f);
        }

        private Rect MissionBriefPanelRect()
        {
            Rect combatPanel = CombatPanelRect();
            float y = combatPanel.yMax + 8f;
            float height = Mathf.Min(Mathf.Clamp(Screen.height * 0.28f, 150f, 230f), Mathf.Max(120f, Screen.height - y - 16f));
            return new Rect(combatPanel.x, y, combatPanel.width, height);
        }

        private void DrawMapMarker(Rect map, Vector2 missionPoint, Color color, float size)
        {
            Vector2 point = MissionToMapPoint(map, missionPoint);
            float half = size * 0.5f;
            DrawColorRect(new Rect(point.x - half, point.y - half, size, size), color);
        }

        private Vector2 MissionToMapPoint(Rect map, Vector2 missionPoint)
        {
            GetMissionBounds(out float minX, out float maxX, out float minY, out float maxY);
            float x = map.x + Mathf.InverseLerp(minX, maxX, missionPoint.x) * map.width;
            float y = map.y + (1f - Mathf.InverseLerp(minY, maxY, missionPoint.y)) * map.height;
            return new Vector2(x, y);
        }

        private void GetMissionBounds(out float minX, out float maxX, out float minY, out float maxY)
        {
            TerrainMeshDefinition terrain = mission.Contract.terrainMesh;
            if (terrain != null && terrain.samples != null && terrain.samples.Length > 0)
            {
                int side = Mathf.Max(1, terrain.sampleSide);
                float spacing = Mathf.Max(1f, terrain.worldUnitsPerVertex * Mathf.Max(1, terrain.sampleStep));
                minX = terrain.minX;
                maxX = terrain.minX + spacing * (side - 1);
                maxY = terrain.minY;
                minY = terrain.minY - spacing * (side - 1);
                return;
            }

            minX = mission.Contract.mission.terrain.minX;
            maxY = mission.Contract.mission.terrain.minY;
            maxX = -minX;
            minY = -maxY;
        }

        private void DrawSectionLine(UnitState unit, float y)
        {
            float x = 24f;
            for (int index = 0; index < unit.Sections.Length; index++)
            {
                DamageSection section = unit.Sections[index];
                float width = 52f;
                Rect back = new(x, y, width, 7);
                DrawColorRect(back, new Color(0.08f, 0.09f, 0.10f, 1f));
                DrawColorRect(new Rect(x, y, width * section.Ratio, 7), SectionHealthColor(section));
                string value = section.IsDestroyed ? "X" : Mathf.RoundToInt(section.Ratio * 100f).ToString();
                GUI.Label(new Rect(x, y + 8, width + 8, 16), ShortSectionName(section.Name) + " " + value);
                x += 58f;
            }
        }

        private Color SectionHealthColor(DamageSection section)
        {
            if (section.IsDestroyed)
            {
                return new Color(0.95f, 0.08f, 0.05f, 1f);
            }

            if (section.Ratio < 0.35f)
            {
                return new Color(1f, 0.24f, 0.08f, 1f);
            }

            if (section.Ratio < 0.70f)
            {
                return new Color(1f, 0.74f, 0.16f, 1f);
            }

            return new Color(0.20f, 0.88f, 0.92f, 1f);
        }

        private string ShortSectionName(string sectionName)
        {
            switch (sectionName)
            {
                case "Cockpit":
                    return "CP";
                case "Torso":
                    return "TR";
                case "Left Arm":
                    return "LA";
                case "Right Arm":
                    return "RA";
                case "Legs":
                    return "LG";
                case "Front":
                    return "FR";
                case "Rear":
                    return "RR";
                case "Turret":
                    return "TU";
                case "Left":
                    return "L";
                case "Right":
                    return "R";
                default:
                    return sectionName;
            }
        }

        private int CountLiveUnits()
        {
            int count = 0;
            foreach (UnitState unit in mission.Units)
            {
                if (unit.IsActive && !unit.IsDestroyed)
                {
                    count++;
                }
            }

            return count;
        }

        private int CountPlayerUnits()
        {
            int count = 0;
            if (mission == null)
            {
                return count;
            }

            foreach (UnitState unit in mission.PlayerUnits())
            {
                count++;
            }

            return count;
        }

        private int CountActivePlayerUnits()
        {
            int count = 0;
            if (mission == null)
            {
                return count;
            }

            foreach (UnitState unit in mission.PlayerUnits())
            {
                if (unit.IsActive && !unit.IsDestroyed)
                {
                    count++;
                }
            }

            return count;
        }

        private static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            {
                return text;
            }

            return text.Substring(0, Mathf.Max(0, maxLength - 3)) + "...";
        }

        private void DrawColorRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private void DrawRectBorder(Rect rect, Color color, float thickness)
        {
            DrawColorRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            DrawColorRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            DrawColorRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            DrawColorRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }
    }

}
