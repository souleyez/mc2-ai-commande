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
    internal enum DemoFlowScreen
    {
        Title,
        Battle,
        MechBay,
        MissionSelect,
        SaveChoices,
        System,
        Debrief
    }

    public sealed class Mc2DemoBootstrap : MonoBehaviour
    {
        [SerializeField] private string missionContractRelativePath = "Missions/mc2_01/mission-contract.json";
        [SerializeField] private string combatDataRelativePath = "Data/combat-data.json";
        [SerializeField] private float cameraHeight = 62f;
        [SerializeField] private float cameraPitch = 58f;
        [SerializeField] private float cameraYaw = 45f;
        private const float JumpDistance = 520f;
        private const float MiniMaxCommanderAdvanceSeconds = 8f;
        private const float DefaultCameraOrthographicSize = 24f;
        private const float CameraZoomWheelStep = 0.08f;
        private const string CapturePresetSpawn = "spawn";
        private const string CapturePresetAirfield = "airfield";
        private const string CapturePresetHangarContact = "hangar-contact";
        private const string CapturePresetNorthPatrol = "north-patrol";
        private const string CapturePresetDamageDemo = "damage-demo";
        private const float LoadoutCardHeight = 456f;
        private const float LoadoutCardStride = 468f;
        private const float LoadoutGridSectionMinHeight = 204f;
        private const float MechLabDedicatedLayoutMinWidth = 900f;
        private const float MechLabDedicatedLayoutMinHeight = 560f;
        private const float MechLabDedicatedBaySummaryHeight = 520f;
        private const float MechLabDedicatedBayMinWidth = 330f;
        private const float MechLabDedicatedBayMaxWidth = 460f;
        private const float MechLabDedicatedLoadoutMinWidth = 520f;
        private const float LoadoutRepairButtonWidth = 64f;
        private const float LoadoutRepairStateOffset = 70f;
        private const float LoadoutEditStatusReservedWidth = 146f;
        private const float LoadoutApplyButtonRightOffset = 144f;
        private const float LoadoutApplyButtonWidth = 66f;
        private const string LoadoutPanelFrameTitle = "Mech Lab / 机库整备";
        private const string LoadoutPanelHeaderTitle = "Mech Lab  /  机库整备";
        private const string MechLabCompanyBayPanelTitle = "Company Bay";
        private const string MechLabLoadoutPanelTitle = "Loadout";
        private const string MechLabBayOpsPageLabel = "Ops";
        private const string MechLabBayRosterPageLabel = "Roster";
        private const int MechLabBayOpsPageIndex = 0;
        private const int MechLabBayRosterPageIndex = 1;
        private const string MechLabReadyLabel = "Bay Ready";
        private const string MechLabReviewPrefix = "Bay Review: ";
        private const string MechLabLoadingText = "Bay loading";
        private const string MechLabUnavailableText = "Bay unavailable";
        private const string MechLabCompanyPrefix = "Company ";
        private const string MechLabPartsLabel = "Parts";
        private const string MechLabBuildPrefix = "Build ";
        private const string SavedAccountIdleLabel = "Ready  no recent save action";
        private const string SavedAccountPathReadyStatusText = "Save slot path ready";
        private const string SavedAccountNoLoadPreviewText = "No save checked";
        private const string SavedAccountLoadPathLabel = "Load";
        private const string SavedAccountLoadPreviewPrefix = "Load Check ";
        private const string SavedAccountLoadButtonLabel = "Load";
        private const string SavedAccountCheckButtonLabel = "Check";
        private const string SavedAccountSlotPathPrefix = "Slot ";
        private const string SavedAccountNoSlotPathText = "No slot path";
        private const string SavedAccountDefaultPathResultText = "Save Slot path";
        private const string SavedAccountResultPrefix = "Save Result ";
        private const string SavedAccountSlotButtonLabel = "Slot";
        private const string SavedAccountRoundtripReadyStatusText = "Save check ready";
        private const string SavedAccountRoundtripFailedStatusText = "Save check failed";
        private const string SavedAccountLoadCheckReadyStatusText = "Load check ready";
        private const string SavedAccountLoadCheckFailedStatusText = "Load check failed";
        private const string SavedAccountLoadBlockedStatusText = "Load blocked";
        private const string SavedAccountLoadFailedStatusText = "Load failed";
        private const string SavedAccountLoadedStatusText = "Save loaded";
        private const string SaveSlotTitleReadyText = "Company ready";
        private const string SaveSlotResultLabel = "Result ";
        private const string SaveSlotResetConfirmText = "Reset starts fresh; old save is copied first.";
        private const string SaveSlotNewCompanyConfirmText = "New company starts fresh; save stays untouched.";
        private const string SaveSlotConfirmResetButtonLabel = "Reset Slot";
        private const string SaveSlotStartFreshButtonLabel = "Start Fresh";
        private const string SaveSlotResetFreshDetailText = "fresh save";
        private const string SaveSlotResetOldCopyPrefix = "old copy ";
        private const string SaveSlotNoSavedCopyText = "No saved game to copy";
        private const string SaveSlotContinueCheckFailedText = "Save check failed";
        private const string EndRunButtonLabel = "End Run";
        private const string DebriefNextStepText = "Next: repair, inspect Mech Lab, choose next contract.";
        private const string DebriefPayoutLabel = "Payout";
        private const string DebriefSalvageLabel = "Salvage";
        private const string DebriefBountyLabel = "Bounty";
        private const string SaveSlotNeedsReviewText = "Save slot needs review";
        private const string NoSaveSlotText = "No save slot";
        private const bool SaveGameUiEnabled = false;
        private const string ContractsOpenStatusText = "Contracts open";
        private const string ContractsHeaderText = "Available Contracts";
        private const string ContractsCardTitleText = "Airfield Contract";
        private const string ContractsMapReadyText = "Mission map ready";
        private const string ContractsLaunchButtonLabel = "Launch";
        private const string ContractsReturnBattleButtonLabel = "Back to Battle";
        private const string ContractsReturnDebriefButtonLabel = "Back to Debrief";
        private const string SystemRestartButtonLabel = "Restart Mission";
        private const string SystemBackButtonLabel = "Back";
        private const string MissionMapBackButtonLabel = "Back";
        private const string MechLabBackButtonLabel = "Back";
        private const string TopStatusModePrefix = "MODE ";
        private const string TopStatusFundsPrefix = "Funds ";
        private const string SquadSelectionBackButtonLabel = "Back";
        private const string SquadSelectionUnavailableStatusText = "Squad plan unavailable";
        private const string ReserveFitBackButtonLabel = "Back";
        private const string AfterActionMechLabStatusText = "After Action: Mech Lab";
        private const string AfterActionContractsStatusText = "After Action: Contracts";
        private const string MechLabRosterEmptyText = "No mechs in bay";
        private const string MechLabRosterStateLabel = "Squad State ";
        private const string LoadoutStockShortPrefix = "Stock short: ";
        private const string LoadoutFitAppliedStatusText = "Fit applied";
        private const string LoadoutEditPendingLabel = "Pending";
        private const float LoadoutResetButtonRightOffset = 72f;
        private const float LoadoutResetButtonWidth = 64f;
        private const float LoadoutSelectedResetButtonWidth = 50f;
        private const float LoadoutTargetControlOffset = 148f;
        private const float LoadoutTargetButtonWidth = 62f;
        private const float LoadoutTargetStatusMinWidth = 80f;
        private const float LoadoutNudgeButtonWidth = 40f;
        private const float LoadoutNudgeEastButtonWidth = 44f;
        private const float LoadoutNudgeStatusWidth = 142f;
        private const float CriticalSectionStatusRatio = 0.35f;
        private const string LoadoutConditionPrefix = "Cond ";
        private const string LoadoutFitOkLabel = "Fit OK";
        private const string LoadoutFitReviewPrefix = "Review ";
        private const string LoadoutHeatPressureLabel = "H ";
        private const string LoadoutWeightPressureLabel = "W ";
        private const string LoadoutGridPressureLabel = "G ";
        private static readonly Color UiPanelColor = new(0.035f, 0.045f, 0.055f, 0.92f);
        private static readonly Color UiButtonColor = new(0.075f, 0.105f, 0.125f, 0.96f);
        private static readonly Color UiTrackColor = new(0.015f, 0.02f, 0.025f, 0.9f);
        private static readonly Color UiBorderColor = new(0.22f, 0.34f, 0.38f, 0.75f);
        private static readonly Color UiCyanColor = new(0.25f, 0.82f, 1f, 0.95f);
        private static readonly Color UiAmberColor = new(1f, 0.62f, 0.18f, 0.95f);
        private static readonly Color UiTextColor = new(0.82f, 0.9f, 0.92f, 1f);
        private static readonly Color LoadoutEmptySlotColor = new(0.46f, 0.22f, 0.04f, 1f);
        private static readonly Color LoadoutShortWeaponColor = new(0.12f, 0.62f, 0.28f, 1f);
        private static readonly Color LoadoutMediumWeaponColor = new(0.10f, 0.36f, 0.82f, 1f);
        private static readonly Color LoadoutLongWeaponColor = new(0.78f, 0.14f, 0.14f, 1f);
        private static readonly Color LoadoutComponentColor = new(0.90f, 0.72f, 0.16f, 1f);

        [Serializable]
        private sealed class VisualCaptureSidecar
        {
            public string preset;
            public string screenshot;
            public string missionId;
            public string result;
            public string status;
            public string flowScreen;
            public float missionTimeSeconds;
            public int screenWidth;
            public int screenHeight;
            public int playerUnitCount;
            public int activeHostileCount;
            public int visibleHostileCount;
            public int targetableStructureCount;
            public int currentObjectiveCount;
            public string[] activeHostiles;
            public string[] visibleHostiles;
            public string[] currentObjectives;
            public VisualCaptureCameraState camera;
            public VisualCaptureReferenceState referenceAssets;
        }

        [Serializable]
        private sealed class VisualCaptureCameraState
        {
            public float orthographicSize;
            public float zoomScale;
            public float pitch;
            public float yaw;
            public CaptureVector3 position;
            public CaptureVector3 rotationEuler;
            public CaptureVector3 followOffset;
        }

        [Serializable]
        private sealed class VisualCaptureReferenceState
        {
            public string terrain;
            public string structures;
            public string props;
        }

        [Serializable]
        private struct CaptureVector3
        {
            public float x;
            public float y;
            public float z;

            public static CaptureVector3 From(Vector3 value)
            {
                return new CaptureVector3
                {
                    x = value.x,
                    y = value.y,
                    z = value.z
                };
            }
        }

        private readonly Dictionary<string, DemoUnitView> unitViews = new();
        private readonly Dictionary<string, DemoStructureView> structureViews = new();
        private readonly Dictionary<string, GameObject> unitSelectionMarkers = new();
        private readonly Dictionary<string, GameObject> unitSoloOrderBeacons = new();
        private readonly Dictionary<string, GameObject> unitOrderMarkers = new();
        private readonly Dictionary<string, GameObject> unitOrderLines = new();
        private readonly Dictionary<string, GameObject> unitFocusMarkers = new();
        private readonly Dictionary<string, GameObject> unitDamageWarningMarkers = new();
        private readonly Dictionary<string, GameObject> unitDamageWarningBeacons = new();
        private readonly Dictionary<string, GameObject> unitRangeMarkers = new();
        private readonly Dictionary<string, GameObject> unitWeaponReadinessMarkers = new();
        private readonly Dictionary<string, GameObject> unitWeaponReadinessBeacons = new();
        private readonly Dictionary<string, GameObject> unitCommanderAnchors = new();
        private readonly Dictionary<string, GameObject> unitCommanderBeacons = new();
        private readonly Dictionary<string, GameObject> unitTargetLines = new();
        private readonly Dictionary<string, GameObject> unitTargetLineEndMarkers = new();
        private readonly Dictionary<string, string> lastTargetLockByUnit = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> visibleFocusTargetCounts = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> hostileFocusTargetCounts = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, GameObject> unitHealthBarBacks = new();
        private readonly Dictionary<string, GameObject> unitHealthBarFills = new();
        private readonly Dictionary<int, List<GameObject>> objectiveAreaMarkers = new();
        private GameObject activeObjectiveGuideMarker;
        private GameObject activeObjectiveGuideBeacon;
        private GameObject activeObjectiveRouteLine;
        private GameObject activeObjectiveRouteStartMarker;
        private GameObject activeObjectiveRouteEndMarker;
        private readonly Dictionary<string, GameObject> structureHealthBarBacks = new();
        private readonly Dictionary<string, GameObject> structureHealthBarFills = new();
        private readonly HashSet<string> detachedUnitIdsLastFrame = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> movingUnitIdsLastFrame = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> jumpingUnitIdsLastFrame = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Material> materialCache = new(StringComparer.Ordinal);
        private readonly Dictionary<string, CombatLoadoutPlacementOverride[]> loadoutPlacementOverridesByUnit = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<CombatLoadoutFillerOverride>> loadoutFillerOverridesByUnit = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, CombatLoadoutPlacementOverride[]> appliedLoadoutPlacementOverridesByUnit = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, CombatLoadoutFillerOverride[]> appliedLoadoutFillerOverridesByUnit = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> selectedLoadoutWeaponByUnit = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Vector2Int> selectedLoadoutGridCellByUnit = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<Material> ownedMaterials = new();
        private readonly List<Texture2D> ownedTextures = new();
        private readonly List<string> combatLog = new();
        private GameObject squadFocusMarker;
        private GameObject squadFocusBeacon;
        private GameObject hostileFocusMarker;
        private GameObject hostileFocusBeacon;
        private GameObject combatPressureMarker;
        private GameObject combatPressureBeacon;
        private GameObject hostilePressureMarker;
        private GameObject hostilePressureBeacon;
        private GameObject hostilePressureVectorLine;
        private GameObject hostilePressureVectorEndMarker;
        private string selectedMechBayLoadoutUnitId;
        private float lastCombatEventTimeSeconds = -999f;
        private float lastContactEventTimeSeconds = -999f;
        private string lastContactLabel = "";
        private int lastContactCount;
        private BattleMission mission;
        private MissionScriptBridge scriptBridge;
        private CommanderCommandPort commandPort;
        private CommanderCommandResult lastStartupCommanderCommandResult;
        private string lastStartupCommanderCommandLine = "";
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
        private bool showStartupContinuePanel;
        private bool showMissionListPanel;
        private bool showMissionResultPanel;
        private bool startupSaveChoicesOpenedFromSystem;
        private bool startupNewGameConfirmPending;
        private bool startupResetSlotConfirmPending;
        private bool startupContinueSaveReady;
        private string startupContinueSummaryText;
        private string startupContinueRosterText;
        private string startupContinueDeltaText;
        private string startupContinueFileText;
        private bool isPaused;
        private MissionResultState lastMissionResult = MissionResultState.InProgress;
        private Camera mainCamera;
        private Vector3 cameraFollowWorldOffset;
        private float cameraBaseHeight = 62f;
        private float cameraBaseOrthographicSize = 24f;
        private float cameraZoomScale = 1f;
        private float cameraZoomMin = 0.76f;
        private float cameraZoomMax = 1.2f;
        private string lastCapturePreset = CapturePresetSpawn;
        private string lastTerrainTextureSummary = "terrain texture not attempted";
        private string lastReferencePropSummary = "ReferenceProps=not attempted";
        private string lastReferenceStructureSummary = "ReferenceStructures=not attempted";
        private Vector2 loadoutScroll;
        private Vector2 mechLabBayScroll;
        private int selectedMechLabBayPage;
        private int selectedRosterIndex;
        private bool showWarehouseDraftFitPreview;
        private bool showSquadSelectionPreview;
        private string warehouseDraftFitPreviewMechId;
        private string squadSelectionDraftOutgoingOwnedMechId;
        private string squadSelectionDraftIncomingOwnedMechId;
        private string squadSelectionLastOutgoingDisplayName;
        private string squadSelectionLastIncomingDisplayName;
        private string lastSavedAccountDeltaText;
        private string lastSavedAccountFileResultText;
        private bool lastSavedAccountFileResultReady;
        private string lastSavedAccountImportApplyPreviewText;
        private string lastSavedAccountImportApplyPreviewPath;
        private string lastSavedAccountImportApplyPreviewDeltaText;
        private int lastSavedAccountImportApplyPreviewJsonCharCount;
        private bool lastSavedAccountImportApplyPreviewReady;
        private string savedAccountImportPreviewInputPath;
        private DemoFlowScreen demoFlowScreen = DemoFlowScreen.Battle;
        private string statusText = "Loading";
        private bool startupSmokeFailed;
        private GUIStyle uiBoxStyle;
        private GUIStyle uiLabelStyle;
        private GUIStyle uiButtonStyle;
        private GUIStyle uiTextFieldStyle;
        private GUIStyle uiToggleStyle;
        private GUIStyle uiHeaderStyle;
        private GUIStyle uiStatusStyle;
        private Texture2D uiPanelTexture;
        private Texture2D uiButtonTexture;
        private Texture2D uiButtonHoverTexture;
        private Texture2D uiTextFieldTexture;
        private const string StartupSmokeDepotOwnedMechId = "assembled-smoke-depot";
        private const string UpdatedSquadLoadedStatusText = "Mech Lab squad loaded";

        private void Start()
        {
            LoadMission();
            BuildWorld();
            RunStartupCommanderSequence();
            ConfigureStartupContinuePanel(Environment.GetCommandLineArgs());
            ScheduleStartupScreenshotIfRequested();
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
                CaptureMissionTickEvents();
                HandleWorldClick();
            }

            CaptureMissionResult();
            UpdateUnitVisibility();
            UpdateObjectiveAreaVisibility();
            UpdateCommandOverlays();
            HandleCameraZoom();
            FollowCommander();
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            foreach (Material material in ownedMaterials)
            {
                Destroy(material);
            }

            DestroyOwnedTextures();
            DestroyUiTexture(ref uiPanelTexture);
            DestroyUiTexture(ref uiButtonTexture);
            DestroyUiTexture(ref uiButtonHoverTexture);
            DestroyUiTexture(ref uiTextFieldTexture);
        }

        private void DestroyOwnedTextures()
        {
            foreach (Texture2D texture in ownedTextures)
            {
                Destroy(texture);
            }

            ownedTextures.Clear();
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
            savedAccountImportPreviewInputPath = DefaultSavedAccountFilePath();
            if (!demoInventoryValidation.IsValid)
            {
                Debug.LogWarning("MC2 mech bay inventory review: " + FirstInventoryError(demoInventoryValidation));
            }

            SetDemoFlowScreen(DemoFlowScreen.Battle);
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

        private bool TryStartFreshDemoRun(string resultStatus)
        {
            Time.timeScale = 1f;
            int clearedRoots = ClearRuntimeWorld();
            missionReceipt = null;
            missionReceiptApplied = false;
            pendingDetachedUnitId = null;
            pendingJumpOrder = false;
            showStartupContinuePanel = false;
            startupSaveChoicesOpenedFromSystem = false;
            startupNewGameConfirmPending = false;
            startupResetSlotConfirmPending = false;
            showMissionMap = false;
            showLoadoutPanel = false;
            showSystemPanel = false;
            showMissionListPanel = false;
            showMissionResultPanel = false;
            CloseTransientMechBayDrafts();

            mission = null;
            scriptBridge = null;
            commandPort = null;
            observationPort = null;
            demoInventory = null;
            demoInventoryValidation = null;
            LoadMission();
            if (mission == null)
            {
                statusText = "New company failed";
                Debug.LogError("MC2 new game failed: mission missing after reset");
                return false;
            }

            BuildWorld();
            lastMissionResult = mission.Result;
            SetPaused(false);
            SetDemoFlowScreen(DemoFlowScreen.Battle);
            statusText = string.IsNullOrWhiteSpace(resultStatus) ? "New company started" : resultStatus;
            RecordSavedAccountFileResult("New Company", false, "save slot kept");
            AddCombatLogLine("New company started: save slot kept");
            Debug.Log(
                "MC2 new game started: clearedRoots="
                + clearedRoots.ToString(CultureInfo.InvariantCulture)
                + " defaultSave="
                + DefaultSavedAccountFilePath());
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
            showMissionListPanel = false;
            showMissionResultPanel = false;
            showWarehouseDraftFitPreview = false;
            showSquadSelectionPreview = false;
            warehouseDraftFitPreviewMechId = null;
            ClearSquadSelectionDraft();
            ClearSquadSelectionCompletedReplacement();
            lastMissionResult = mission.Result;

            BuildWorld();
            RefreshDemoInventoryValidation();
            SetPaused(keepMechBayOpen && mission.Result == MissionResultState.InProgress);
            SetDemoFlowScreen(keepMechBayOpen ? DemoFlowScreen.MechBay : DemoFlowScreen.Battle);
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
                return "Mission restarted - Mech Lab open";
            }

            return result?.Message ?? "Mission restarted";
        }

        private int ClearRuntimeWorld()
        {
            unitViews.Clear();
            structureViews.Clear();
            unitSelectionMarkers.Clear();
            unitSoloOrderBeacons.Clear();
            unitOrderMarkers.Clear();
            unitOrderLines.Clear();
            unitFocusMarkers.Clear();
            unitDamageWarningMarkers.Clear();
            unitDamageWarningBeacons.Clear();
            unitRangeMarkers.Clear();
            unitWeaponReadinessMarkers.Clear();
            unitWeaponReadinessBeacons.Clear();
            unitCommanderAnchors.Clear();
            unitCommanderBeacons.Clear();
            unitTargetLines.Clear();
            unitTargetLineEndMarkers.Clear();
            lastTargetLockByUnit.Clear();
            visibleFocusTargetCounts.Clear();
            hostileFocusTargetCounts.Clear();
            unitHealthBarBacks.Clear();
            unitHealthBarFills.Clear();
            objectiveAreaMarkers.Clear();
            activeObjectiveGuideMarker = null;
            activeObjectiveGuideBeacon = null;
            activeObjectiveRouteLine = null;
            activeObjectiveRouteStartMarker = null;
            activeObjectiveRouteEndMarker = null;
            structureHealthBarBacks.Clear();
            structureHealthBarFills.Clear();
            detachedUnitIdsLastFrame.Clear();
            movingUnitIdsLastFrame.Clear();
            jumpingUnitIdsLastFrame.Clear();
            loadoutPlacementOverridesByUnit.Clear();
            loadoutFillerOverridesByUnit.Clear();
            appliedLoadoutPlacementOverridesByUnit.Clear();
            appliedLoadoutFillerOverridesByUnit.Clear();
            selectedLoadoutWeaponByUnit.Clear();
            selectedLoadoutGridCellByUnit.Clear();
            combatLog.Clear();
            squadFocusMarker = null;
            squadFocusBeacon = null;
            hostileFocusMarker = null;
            hostileFocusBeacon = null;
            combatPressureMarker = null;
            combatPressureBeacon = null;
            hostilePressureMarker = null;
            hostilePressureBeacon = null;
            hostilePressureVectorLine = null;
            hostilePressureVectorEndMarker = null;
            lastCombatEventTimeSeconds = -999f;
            lastContactEventTimeSeconds = -999f;
            lastContactLabel = "";
            lastContactCount = 0;
            mainCamera = null;
            DestroyOwnedTextures();

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

        private void ScheduleStartupScreenshotIfRequested()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int index = 0; index < args.Length; index++)
            {
                if (args[index] != "-mc2CaptureScreenshot")
                {
                    continue;
                }

                if (index + 1 >= args.Length || string.IsNullOrWhiteSpace(args[index + 1]))
                {
                    Debug.LogWarning("MC2 screenshot capture blocked: missing path after -mc2CaptureScreenshot.");
                    return;
                }

                string sidecarPath = CommandLineValue(args, "-mc2CaptureSidecar");
                StartCoroutine(CaptureStartupScreenshotAfterRealtimeDelay(args[index + 1], sidecarPath, HasCommandLineArg(args, "-mc2CaptureQuit")));
                return;
            }
        }

        private IEnumerator CaptureStartupScreenshotAfterRealtimeDelay(string path, string sidecarPath, bool quitAfterCapture)
        {
            yield return new WaitForSecondsRealtime(1.0f);
            yield return new WaitForEndOfFrame();

            int exitCode = 0;
            string fullPath = "";
            try
            {
                fullPath = Path.GetFullPath(path);
                string folder = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                ScreenCapture.CaptureScreenshot(fullPath);
                Debug.Log("MC2 screenshot capture requested: " + fullPath);
                WriteCaptureSidecar(SidecarPathFor(fullPath, sidecarPath), fullPath);
            }
            catch (Exception ex)
            {
                exitCode = 1;
                Debug.LogError("MC2 screenshot capture failed"
                    + (string.IsNullOrWhiteSpace(fullPath) ? "" : " path=" + fullPath)
                    + " error="
                    + ex.Message);
            }

            if (quitAfterCapture)
            {
                yield return new WaitForSecondsRealtime(0.75f);
                Application.Quit(exitCode);
            }
        }

        private static string CommandLineValue(string[] args, string expected)
        {
            if (args == null)
            {
                return "";
            }

            for (int index = 0; index < args.Length - 1; index++)
            {
                if (string.Equals(args[index], expected, StringComparison.Ordinal))
                {
                    return args[index + 1] ?? "";
                }
            }

            return "";
        }

        private static string SidecarPathFor(string screenshotPath, string requestedSidecarPath)
        {
            if (!string.IsNullOrWhiteSpace(requestedSidecarPath))
            {
                return Path.GetFullPath(requestedSidecarPath);
            }

            return Path.ChangeExtension(screenshotPath, ".json");
        }

        private static bool HasCommandLineArg(string[] args, string expected)
        {
            if (args == null)
            {
                return false;
            }

            for (int index = 0; index < args.Length; index++)
            {
                if (string.Equals(args[index], expected, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
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
                    case "-mc2CapturePreset":
                        index = RunStartupCapturePreset(args, index);
                        break;
                    case "-mc2ReportState":
                        ReportStartupCommanderState();
                        break;
                    case "-mc2RestartMission":
                        RunStartupMissionRestart();
                        break;
                    case "-mc2LoadDefaultSave":
                        RunStartupLoadDefaultSave();
                        break;
                    case "-mc2MinimaxCommanderSteps":
                        index = RunStartupMiniMaxCommander(args, index);
                        break;
                }
            }
        }

        private void ConfigureStartupContinuePanel(string[] args)
        {
            showStartupContinuePanel = false;
            startupSaveChoicesOpenedFromSystem = false;
            startupNewGameConfirmPending = false;
            startupResetSlotConfirmPending = false;
            if (!SaveGameUiEnabled)
            {
                return;
            }

            if (HasStartupAutomationArgs(args) || !RefreshStartupContinueSavePreview())
            {
                return;
            }

            showStartupContinuePanel = true;
            showLoadoutPanel = false;
            showSystemPanel = false;
            showMissionMap = false;
            CloseTransientMechBayDrafts();
            SetDemoFlowScreen(DemoFlowScreen.Title);
            if (mission?.Result == MissionResultState.InProgress)
            {
                SetPaused(true);
            }

            statusText = "Continue available";
            RecordSavedAccountFileResult("Continue available", startupContinueSaveReady, startupContinueSummaryText);
        }

        private bool RefreshStartupContinueSavePreview()
        {
            string defaultPath = DefaultSavedAccountFilePath();
            startupContinueSaveReady = false;
            startupContinueSummaryText = "No saved progress found";
            startupContinueRosterText = "New Company starts a fresh run";
            startupContinueDeltaText = string.Empty;
            startupContinueFileText = "Save " + SavedAccountFileName(defaultPath);
            if (!File.Exists(defaultPath))
            {
                return false;
            }

            startupContinueFileText = SavedAccountFileSummaryText(defaultPath);
            MechBaySavedAccountFileResult preview =
                MechBaySavedAccountService.PreviewImportApplyJsonFile(defaultPath, CurrentSavedAccountSnapshot());
            if (preview == null || !preview.Accepted)
            {
                startupContinueSummaryText = SaveSlotNeedsReviewText;
                startupContinueRosterText = SavedAccountStartupContinueReviewText(preview?.Message);
                startupContinueDeltaText = "Continue disabled";
                return true;
            }

            startupContinueSaveReady = true;
            startupContinueSummaryText = MechBaySavedAccountService.SummaryText(preview.LoadedAccount);
            MechBaySavedAccountCounters counters =
                preview.Validation?.Counters ?? new MechBaySavedAccountCounters();
            startupContinueRosterText =
                "Tokens "
                + counters.tokenBalance.ToString(CultureInfo.InvariantCulture)
                + "  Reserve "
                + counters.warehouseMechCount.ToString(CultureInfo.InvariantCulture)
                + "  Items "
                + counters.itemStackCount.ToString(CultureInfo.InvariantCulture);
            startupContinueDeltaText = MechBaySavedAccountService.DeltaText(preview.Delta);
            return true;
        }

        private static string SavedAccountFileSummaryText(string path)
        {
            try
            {
                FileInfo file = new(path);
                return "File "
                    + SavedAccountFileName(path)
                    + "  Saved "
                    + file.LastWriteTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                return "File " + SavedAccountFileName(path) + "  " + exception.Message;
            }
        }

        private static string SavedAccountStartupContinueReviewText(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return SaveSlotContinueCheckFailedText;
            }

            if (message.IndexOf("preview", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("import", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("account", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("JSON", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return SaveSlotContinueCheckFailedText;
            }

            return SavedAccountImportApplyPlayerBlockReason(message);
        }

        private static bool HasStartupAutomationArgs(string[] args)
        {
            if (args == null)
            {
                return false;
            }

            for (int index = 0; index < args.Length; index++)
            {
                string arg = args[index];
                if (string.Equals(arg, "-mc2SmokeTest", StringComparison.Ordinal)
                    || string.Equals(arg, "-mc2Command", StringComparison.Ordinal)
                    || string.Equals(arg, "-mc2CommandFile", StringComparison.Ordinal)
                    || string.Equals(arg, "-mc2AdvanceSeconds", StringComparison.Ordinal)
                    || string.Equals(arg, "-mc2CapturePreset", StringComparison.Ordinal)
                    || string.Equals(arg, "-mc2ReportState", StringComparison.Ordinal)
                    || string.Equals(arg, "-mc2RestartMission", StringComparison.Ordinal)
                    || string.Equals(arg, "-mc2LoadDefaultSave", StringComparison.Ordinal)
                    || string.Equals(arg, "-mc2MinimaxCommanderSteps", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
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

            CommanderCommandPort.TryParse(commandLine, out CommanderCommand parsedCommand, out _);
            CommanderCommandResult result = commandPort.IssueText(commandLine);
            LogStartupCommanderCommand(commandLine, result);
            SpawnCommandResultCue(parsedCommand, result);
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
                    case StartupCommanderScriptActionKind.HideSquadPreview:
                        RunStartupHideSquadPreview();
                        break;
                    case StartupCommanderScriptActionKind.CompleteVisibleObjectives:
                        RunStartupCompleteVisibleObjectives();
                        break;
                    case StartupCommanderScriptActionKind.OpenDebrief:
                        RunStartupOpenDebrief();
                        break;
                    case StartupCommanderScriptActionKind.SavedAccountReport:
                        RunStartupSavedAccountReport();
                        break;
                    case StartupCommanderScriptActionKind.SavedAccountSaveLoadPreview:
                        RunStartupSavedAccountSaveLoadPreview();
                        break;
                    case StartupCommanderScriptActionKind.SavedAccountExport:
                        RunStartupSavedAccountExport(action.FilePath);
                        break;
                    case StartupCommanderScriptActionKind.SavedAccountImportPreview:
                        RunStartupSavedAccountImportPreview(action.FilePath);
                        break;
                    case StartupCommanderScriptActionKind.SavedAccountImportApplyPreview:
                        RunStartupSavedAccountImportApplyPreview(action.FilePath);
                        break;
                    case StartupCommanderScriptActionKind.SavedAccountImportApply:
                        RunStartupSavedAccountImportApply(action.FilePath);
                        break;
                    case StartupCommanderScriptActionKind.SavedAccountLoadDefaultPreview:
                        RunStartupSavedAccountLoadDefaultPreview();
                        break;
                    case StartupCommanderScriptActionKind.SavedAccountSaveCurrentDefault:
                        RunStartupSavedAccountSaveCurrentDefault();
                        break;
                    case StartupCommanderScriptActionKind.SavedAccountLoadDefaultApply:
                        RunStartupSavedAccountLoadDefaultApply();
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
                    case StartupCommanderScriptActionKind.AssertDebriefSummary:
                        RunStartupDebriefSummaryAssertion();
                        break;
                    case StartupCommanderScriptActionKind.AssertDebriefVisible:
                        RunStartupDebriefVisibleAssertion();
                        break;
                    case StartupCommanderScriptActionKind.AssertCommandResult:
                        RunStartupCommandResultAssertion(action.ExpectedCommandBlocked, action.ExpectedCommandAcceptedCount);
                        break;
                    case StartupCommanderScriptActionKind.AssertCombatSituation:
                        RunStartupCombatSituationAssertion(action.ExpectedCombatTempo, action.ExpectedSoloCount, action.ExpectedJumpingCount, action.ExpectedSalvageCount);
                        break;
                    case StartupCommanderScriptActionKind.AssertEncounterPacing:
                        RunStartupEncounterPacingAssertion();
                        break;
                    case StartupCommanderScriptActionKind.AssertObjectiveGraph:
                        RunStartupObjectiveGraphAssertion();
                        break;
                    case StartupCommanderScriptActionKind.AssertLoadoutCompact:
                        RunStartupLoadoutCompactAssertion();
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

        private static string ResolveStartupCommanderDataFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path))
            {
                return path;
            }

            return Path.GetFullPath(path);
        }

        private static string DefaultSavedAccountFilePath()
        {
            return Path.Combine(Application.persistentDataPath, "mc2-demo-saved-account.json");
        }

        private static string DefaultSavedAccountExportCopyPath()
        {
            string defaultPath = DefaultSavedAccountFilePath();
            string directory = Path.GetDirectoryName(defaultPath);
            string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
            return Path.Combine(directory ?? string.Empty, "mc2-demo-saved-account-export-" + stamp + ".json");
        }

        private void LogStartupCommanderCommand(string commandLine, CommanderCommandResult result)
        {
            lastStartupCommanderCommandLine = commandLine ?? "";
            lastStartupCommanderCommandResult = result;
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

        private void RunStartupSavedAccountReport()
        {
            int sourceTokenBalance = Math.Max(0, demoInventory?.tokenBalance ?? 0);
            MechBaySavedAccountContract account = MechBaySavedAccountService.BuildDemoSnapshot(demoInventory);
            MechBaySavedAccountValidationResult validation = MechBaySavedAccountService.Validate(account);
            string json = JsonUtility.ToJson(account);
            bool jsonOk = !string.IsNullOrWhiteSpace(json)
                && json.Contains(MechBaySavedAccountService.Schema)
                && json.Contains(account?.accountId ?? "");
            bool sourceUnchanged = Math.Max(0, demoInventory?.tokenBalance ?? 0) == sourceTokenBalance;
            bool accepted = validation.IsValid && jsonOk && sourceUnchanged;
            MechBaySavedAccountCounters counters = validation.Counters ?? new MechBaySavedAccountCounters();
            string summary = MechBaySavedAccountService.SummaryText(account);
            string line = "summary="
                + summary
                + " tokens="
                + counters.tokenBalance.ToString(CultureInfo.InvariantCulture)
                + " mechs="
                + counters.ownedMechCount.ToString(CultureInfo.InvariantCulture)
                + " ready="
                + counters.readyMissionMechCount.ToString(CultureInfo.InvariantCulture)
                + " jsonChars="
                + (json?.Length ?? 0).ToString(CultureInfo.InvariantCulture)
                + " unchanged="
                + sourceUnchanged;

            if (accepted)
            {
                statusText = "Account report ready";
                AddCombatLogLine("CLI saved account report OK: " + summary);
                Debug.Log("MC2 saved account report: " + line);
                return;
            }

            startupSmokeFailed = true;
            statusText = "Account report failed";
            string reason = validation.IsValid ? "json or mutation check failed" : FirstSavedAccountValidationError(validation);
            AddCombatLogLine("CLI saved account report failed: " + reason);
            Debug.LogError("MC2 saved account report failed: " + reason + " " + line);
        }

        private void RunStartupSavedAccountSaveLoadPreview()
        {
            int sourceTokenBalance = Math.Max(0, demoInventory?.tokenBalance ?? 0);
            MechBaySavedAccountContract account = CurrentSavedAccountSnapshot();
            MechBaySavedAccountJsonPreviewResult preview =
                MechBaySavedAccountService.PreviewJsonSaveLoad(account);
            bool sourceUnchanged = Math.Max(0, demoInventory?.tokenBalance ?? 0) == sourceTokenBalance;
            string summary = MechBaySavedAccountService.SummaryText(preview.LoadedAccount ?? account);
            string deltaText = MechBaySavedAccountService.DeltaText(preview.Delta);
            string line = "summary="
                + summary
                + " jsonChars="
                + preview.JsonCharCount.ToString(CultureInfo.InvariantCulture)
                + " delta="
                + deltaText
                + " unchanged="
                + sourceUnchanged
                + " message="
                + preview.Message;

            if (preview.Accepted && sourceUnchanged)
            {
                statusText = SavedAccountRoundtripReadyStatusText;
                RecordSavedAccountFileResult("Roundtrip OK", true, deltaText);
                AddCombatLogLine("CLI saved account save/load preview OK: " + summary);
                Debug.Log("MC2 saved account save/load preview: " + line);
                return;
            }

            startupSmokeFailed = true;
            statusText = SavedAccountRoundtripFailedStatusText;
            string reason = preview.Message ?? "preview failed";
            RecordSavedAccountFileResult("Roundtrip failed", false, reason);
            AddCombatLogLine("CLI saved account save/load preview failed: " + reason);
            Debug.LogError("MC2 saved account save/load preview failed: " + reason + " " + line);
        }

        private void RunStartupSavedAccountExport(string requestedPath)
        {
            TryExportSavedAccount(requestedPath, true, "CLI");
        }

        private bool TryExportSavedAccount(string requestedPath, bool markStartupSmokeFailure, string logPrefix)
        {
            string resolvedPath = ResolveStartupCommanderDataFilePath(requestedPath);
            MechBaySavedAccountFileResult export =
                MechBaySavedAccountService.ExportJsonFile(CurrentSavedAccountSnapshot(), resolvedPath);
            string summary = MechBaySavedAccountService.SummaryText(CurrentSavedAccountSnapshot());
            string line = "path="
                + resolvedPath
                + " jsonChars="
                + export.JsonCharCount.ToString(CultureInfo.InvariantCulture)
                + " message="
                + export.Message;

            if (export.Accepted)
            {
                statusText = "Account export ready";
                savedAccountImportPreviewInputPath = resolvedPath;
                RecordSavedAccountFileResult("Export OK", true, SavedAccountFileName(resolvedPath) + "  " + summary);
                AddCombatLogLine((logPrefix ?? "Saved account") + " saved account export OK: " + summary);
                Debug.Log("MC2 saved account export: source=" + (logPrefix ?? "unknown") + " " + line);
                return true;
            }

            if (markStartupSmokeFailure)
            {
                startupSmokeFailed = true;
            }

            statusText = "Account export failed";
            string reason = export.Message ?? "export failed";
            RecordSavedAccountFileResult("Export failed", false, reason);
            AddCombatLogLine((logPrefix ?? "Saved account") + " saved account export failed: " + reason);
            Debug.LogError(
                "MC2 saved account export failed: source="
                + (logPrefix ?? "unknown")
                + " "
                + reason
                + " "
                + line);
            return false;
        }

        private bool TryCopyDefaultSavedAccountFile(out string copyPath, out string reason)
        {
            string defaultPath = DefaultSavedAccountFilePath();
            copyPath = string.Empty;
            reason = string.Empty;
            if (!File.Exists(defaultPath))
            {
                reason = SaveSlotNoSavedCopyText;
                return true;
            }

            copyPath = DefaultSavedAccountExportCopyPath();
            try
            {
                string directory = Path.GetDirectoryName(copyPath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.Copy(defaultPath, copyPath, false);
                if (File.Exists(copyPath))
                {
                    reason = "Copy " + SavedAccountFileName(copyPath);
                    return true;
                }

                reason = "Copy file missing";
                return false;
            }
            catch (Exception exception)
            {
                reason = "Copy exception: " + exception.Message;
                return false;
            }
        }

        private void RunStartupSavedAccountImportPreview(string requestedPath)
        {
            int sourceTokenBalance = Math.Max(0, demoInventory?.tokenBalance ?? 0);
            string resolvedPath = ResolveStartupCommanderDataFilePath(requestedPath);
            MechBaySavedAccountFileResult importPreview =
                MechBaySavedAccountService.PreviewImportJsonFile(
                    resolvedPath,
                    CurrentSavedAccountSnapshot());
            bool sourceUnchanged = Math.Max(0, demoInventory?.tokenBalance ?? 0) == sourceTokenBalance;
            string summary = MechBaySavedAccountService.SummaryText(importPreview.LoadedAccount ?? CurrentSavedAccountSnapshot());
            string deltaText = MechBaySavedAccountService.DeltaText(importPreview.Delta);
            string line = "path="
                + resolvedPath
                + " summary="
                + summary
                + " jsonChars="
                + importPreview.JsonCharCount.ToString(CultureInfo.InvariantCulture)
                + " delta="
                + deltaText
                + " unchanged="
                + sourceUnchanged
                + " message="
                + importPreview.Message;

            if (importPreview.Accepted && sourceUnchanged)
            {
                statusText = SavedAccountRoundtripReadyStatusText;
                RecordSavedAccountFileResult("Save check OK", true, deltaText);
                AddCombatLogLine("CLI saved account import preview OK: " + deltaText);
                Debug.Log("MC2 saved account import preview: " + line);
                return;
            }

            startupSmokeFailed = true;
            statusText = SavedAccountRoundtripFailedStatusText;
            string reason = importPreview.Message ?? "import preview failed";
            RecordSavedAccountFileResult("Save check failed", false, reason);
            AddCombatLogLine("CLI saved account import preview failed: " + reason);
            Debug.LogError("MC2 saved account import preview failed: " + reason + " " + line);
        }

        private void RunStartupSavedAccountImportApplyPreview(string requestedPath)
        {
            TryPreviewSavedAccountImportApply(requestedPath, true, "CLI");
        }

        private bool TryPreviewSavedAccountImportApply(
            string requestedPath,
            bool markStartupSmokeFailure,
            string logPrefix)
        {
            int sourceTokenBalance = Math.Max(0, demoInventory?.tokenBalance ?? 0);
            string resolvedPath = ResolveStartupCommanderDataFilePath(requestedPath);
            MechBaySavedAccountFileResult applyPreview =
                MechBaySavedAccountService.PreviewImportApplyJsonFile(
                    resolvedPath,
                    CurrentSavedAccountSnapshot());
            bool sourceUnchanged = Math.Max(0, demoInventory?.tokenBalance ?? 0) == sourceTokenBalance;
            string summary = MechBaySavedAccountService.SummaryText(applyPreview.LoadedAccount ?? CurrentSavedAccountSnapshot());
            string deltaText = MechBaySavedAccountService.DeltaText(applyPreview.Delta);
            lastSavedAccountImportApplyPreviewReady = applyPreview.Accepted && sourceUnchanged;
            lastSavedAccountImportApplyPreviewPath = resolvedPath;
            lastSavedAccountImportApplyPreviewDeltaText = deltaText;
            lastSavedAccountImportApplyPreviewJsonCharCount = applyPreview.JsonCharCount;
            savedAccountImportPreviewInputPath = resolvedPath;
            lastSavedAccountImportApplyPreviewText =
                SavedAccountImportApplyPreviewText(applyPreview, deltaText, sourceUnchanged);
            string line = "path="
                + resolvedPath
                + " summary="
                + summary
                + " jsonChars="
                + applyPreview.JsonCharCount.ToString(CultureInfo.InvariantCulture)
                + " wouldChange="
                + applyPreview.WouldChange
                + " delta="
                + deltaText
                + " unchanged="
                + sourceUnchanged
                + " message="
                + applyPreview.Message;

            if (applyPreview.Accepted && sourceUnchanged)
            {
                statusText = SavedAccountLoadCheckReadyStatusText;
                RecordSavedAccountFileResult(
                    applyPreview.WouldChange ? "Load check review" : "Load check ready",
                    true,
                    deltaText);
                AddCombatLogLine((logPrefix ?? "Saved account") + " saved account import apply preview OK: " + deltaText);
                Debug.Log("MC2 saved account import apply preview: source=" + (logPrefix ?? "unknown") + " " + line);
                return true;
            }

            if (markStartupSmokeFailure)
            {
                startupSmokeFailed = true;
            }

            statusText = SavedAccountLoadCheckFailedStatusText;
            string reason = applyPreview.Message ?? "import apply preview failed";
            RecordSavedAccountFileResult("Load check failed", false, reason);
            AddCombatLogLine((logPrefix ?? "Saved account") + " saved account import apply preview failed: " + reason);
            Debug.LogError(
                "MC2 saved account import apply preview failed: source="
                + (logPrefix ?? "unknown")
                + " "
                + reason
                + " "
                + line);
            return false;
        }

        private void RunStartupSavedAccountImportApply(string requestedPath)
        {
            TryApplySavedAccountImport(requestedPath, true, "CLI");
        }

        private void RunStartupSavedAccountLoadDefaultPreview()
        {
            TryPreviewSavedAccountImportApply(DefaultSavedAccountFilePath(), true, "CLI default load");
        }

        private void RunStartupSavedAccountSaveCurrentDefault()
        {
            TryExportSavedAccount(DefaultSavedAccountFilePath(), true, "CLI default save current");
        }

        private void RunStartupSavedAccountLoadDefaultApply()
        {
            TryApplySavedAccountImport(DefaultSavedAccountFilePath(), true, "CLI default load");
        }

        private void RunStartupLoadDefaultSave()
        {
            string defaultPath = DefaultSavedAccountFilePath();
            savedAccountImportPreviewInputPath = defaultPath;
            if (!File.Exists(defaultPath))
            {
                statusText = "Default save missing";
                RecordSavedAccountFileResult("Load skipped", false, SavedAccountFileName(defaultPath));
                AddCombatLogLine("CLI default save load skipped: no default save");
                Debug.Log("MC2 saved account default load skipped: path=" + defaultPath);
                return;
            }

            if (TryPreviewSavedAccountImportApply(defaultPath, true, "CLI default load"))
            {
                TryApplySavedAccountImport(defaultPath, true, "CLI default load");
            }
        }

        private bool TryLoadDefaultSavedAccount(string logPrefix)
        {
            string defaultPath = DefaultSavedAccountFilePath();
            savedAccountImportPreviewInputPath = defaultPath;
            return TryPreviewSavedAccountImportApply(defaultPath, false, logPrefix)
                && TryApplySavedAccountImport(defaultPath, false, logPrefix);
        }

        private bool TryApplySavedAccountImport(
            string requestedPath,
            bool markStartupSmokeFailure,
            string logPrefix)
        {
            string resolvedPath = ResolveStartupCommanderDataFilePath(requestedPath);
            MechBaySavedAccountContract currentAccount = CurrentSavedAccountSnapshot();
            MechBaySavedAccountFileResult preview =
                MechBaySavedAccountService.PreviewImportApplyJsonFile(resolvedPath, currentAccount);
            string deltaText = MechBaySavedAccountService.DeltaText(preview.Delta);
            string blockReason =
                SavedAccountImportApplyBlockReason(resolvedPath, preview, deltaText);
            if (!string.IsNullOrWhiteSpace(blockReason))
            {
                if (markStartupSmokeFailure)
                {
                    startupSmokeFailed = true;
                }

                statusText = SavedAccountLoadBlockedStatusText;
                string playerBlockReason = SavedAccountImportApplyPlayerBlockReason(blockReason);
                lastSavedAccountImportApplyPreviewReady = false;
                lastSavedAccountImportApplyPreviewText = "Blocked  " + playerBlockReason;
                RecordSavedAccountFileResult("Load blocked", false, playerBlockReason);
                AddCombatLogLine((logPrefix ?? "Saved account") + " saved account import apply blocked: " + blockReason);
                Debug.LogError(
                    "MC2 saved account import apply blocked: path="
                    + resolvedPath
                    + " source="
                    + (logPrefix ?? "unknown")
                    + " delta="
                    + deltaText
                    + " message="
                    + (preview?.Message ?? blockReason));
                return false;
            }

            MechBaySavedAccountFileResult apply =
                MechBaySavedAccountService.ApplyImportJsonFile(resolvedPath, currentAccount);
            if (apply == null || !apply.Accepted || apply.AppliedInventory == null)
            {
                if (markStartupSmokeFailure)
                {
                    startupSmokeFailed = true;
                }

                statusText = SavedAccountLoadFailedStatusText;
                string reason = apply?.Message ?? "apply failed";
                lastSavedAccountImportApplyPreviewReady = false;
                lastSavedAccountImportApplyPreviewText = "Blocked  " + reason;
                RecordSavedAccountFileResult("Load failed", false, reason);
                AddCombatLogLine((logPrefix ?? "Saved account") + " saved account import apply failed: " + reason);
                Debug.LogError(
                    "MC2 saved account import apply failed: path="
                    + resolvedPath
                    + " source="
                    + (logPrefix ?? "unknown")
                    + " message="
                    + reason);
                return false;
            }

            demoInventory = apply.AppliedInventory;
            RefreshDemoInventoryValidation();
            CloseTransientMechBayDrafts();
            lastSavedAccountDeltaText = deltaText;
            lastSavedAccountImportApplyPreviewReady = false;
            lastSavedAccountImportApplyPreviewPath = resolvedPath;
            lastSavedAccountImportApplyPreviewDeltaText = deltaText;
            lastSavedAccountImportApplyPreviewJsonCharCount = apply.JsonCharCount;
            lastSavedAccountImportApplyPreviewText = "Applied  " + deltaText;
            statusText = SavedAccountLoadedStatusText;
            RecordSavedAccountFileResult("Load OK", true, deltaText);
            AddCombatLogLine((logPrefix ?? "Saved account") + " saved account import apply OK: " + deltaText);
            TryAutoSaveSavedAccount("import apply " + deltaText);
            Debug.Log(
                "MC2 saved account import apply: path="
                + resolvedPath
                + " source="
                + (logPrefix ?? "unknown")
                + " jsonChars="
                + apply.JsonCharCount.ToString(CultureInfo.InvariantCulture)
                + " delta="
                + deltaText
                + " message="
                + apply.Message);
            return true;
        }

        private string SavedAccountImportApplyBlockReason(
            string resolvedPath,
            MechBaySavedAccountFileResult preview,
            string deltaText)
        {
            if (!lastSavedAccountImportApplyPreviewReady)
            {
                return "preview not ready";
            }

            if (!string.Equals(
                    lastSavedAccountImportApplyPreviewPath,
                    resolvedPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                return "preview path mismatch";
            }

            if (preview == null || !preview.Accepted)
            {
                return preview?.Message ?? "preview rejected";
            }

            if (preview.JsonCharCount != lastSavedAccountImportApplyPreviewJsonCharCount)
            {
                return "preview file changed";
            }

            if (!string.Equals(
                    lastSavedAccountImportApplyPreviewDeltaText,
                    deltaText,
                    StringComparison.Ordinal))
            {
                return "preview delta changed";
            }

            return null;
        }

        private static string SavedAccountImportApplyPlayerBlockReason(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return "check save first";
            }

            if (reason.IndexOf("not ready", StringComparison.OrdinalIgnoreCase) >= 0
                || reason.IndexOf("rejected", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "check save first";
            }

            if (reason.IndexOf("path mismatch", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "save path changed";
            }

            if (reason.IndexOf("file changed", StringComparison.OrdinalIgnoreCase) >= 0
                || reason.IndexOf("delta changed", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "save changed after check";
            }

            string result = reason;
            result = result.Replace("import apply preview", "load check");
            result = result.Replace("Import apply preview", "Load check");
            result = result.Replace("import apply", "load");
            result = result.Replace("Import apply", "Load");
            result = result.Replace("preview", "check");
            result = result.Replace("Preview", "Check");
            result = result.Replace("account", "save");
            result = result.Replace("Account", "Save");
            return result;
        }

        private static string SavedAccountImportApplyPreviewText(
            MechBaySavedAccountFileResult preview,
            string deltaText,
            bool sourceUnchanged)
        {
            if (preview == null)
            {
                return "Blocked  check unavailable";
            }

            if (!sourceUnchanged)
            {
                return "Blocked  source changed during check";
            }

            if (!preview.Accepted)
            {
                return "Blocked  " + SavedAccountImportApplyPlayerBlockReason(preview.Message ?? "preview rejected");
            }

            string delta = string.IsNullOrWhiteSpace(deltaText) ? "Delta none" : deltaText;
            return (preview.WouldChange ? "Review  " : "Ready  ") + delta;
        }

        private static string FirstSavedAccountValidationError(MechBaySavedAccountValidationResult validation)
        {
            string[] errors = validation?.Errors ?? Array.Empty<string>();
            return errors.Length == 0 ? "unknown" : errors[0];
        }

        private MechBaySavedAccountContract CurrentSavedAccountSnapshot()
        {
            return MechBaySavedAccountService.BuildDemoSnapshot(demoInventory);
        }

        private string RecordSavedAccountDelta(
            MechBaySavedAccountContract beforeAccount,
            MechBaySavedAccountContract afterAccount,
            string logPrefix)
        {
            MechBaySavedAccountDelta delta = MechBaySavedAccountService.BuildDelta(beforeAccount, afterAccount);
            string text = MechBaySavedAccountService.DeltaText(delta);
            lastSavedAccountDeltaText = text;
            AddCombatLogLine((logPrefix ?? "Saved account") + " account " + text);
            Debug.Log("MC2 saved account delta: source=" + (logPrefix ?? "unknown") + " " + text);
            return text;
        }

        private void RecordSavedAccountFileResult(string action, bool ready, string detail)
        {
            string safeAction = string.IsNullOrWhiteSpace(action) ? "Save/load" : action.Trim();
            string safeDetail = string.IsNullOrWhiteSpace(detail) ? string.Empty : "  " + detail.Trim();
            lastSavedAccountFileResultText = safeAction + safeDetail;
            lastSavedAccountFileResultReady = ready;
        }

        private bool TryAutoSaveSavedAccount(string reason)
        {
            string resolvedPath = DefaultSavedAccountFilePath();
            MechBaySavedAccountContract account = CurrentSavedAccountSnapshot();
            MechBaySavedAccountFileResult export =
                MechBaySavedAccountService.ExportJsonFile(account, resolvedPath);
            string safeReason = string.IsNullOrWhiteSpace(reason) ? "account change" : reason.Trim();
            string line = "reason="
                + safeReason
                + " path="
                + resolvedPath
                + " jsonChars="
                + export.JsonCharCount.ToString(CultureInfo.InvariantCulture)
                + " message="
                + export.Message;

            if (export.Accepted)
            {
                RecordSavedAccountFileResult("Auto Save OK", true, SavedAccountFileName(resolvedPath) + "  " + safeReason);
                Debug.Log("MC2 saved account auto-save: " + line);
                return true;
            }

            statusText = "Auto-save failed";
            string failure = export.Message ?? "auto-save failed";
            RecordSavedAccountFileResult("Auto Save failed", false, failure);
            AddCombatLogLine("Auto-save failed: " + failure);
            Debug.LogError("MC2 saved account auto-save failed: " + failure + " " + line);
            return false;
        }

        private static string SavedAccountFileName(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return "No path";
            }

            string fileName = Path.GetFileName(path);
            return string.IsNullOrWhiteSpace(fileName) ? path : fileName;
        }

        private void CloseTransientMechBayDrafts()
        {
            showWarehouseDraftFitPreview = false;
            showSquadSelectionPreview = false;
            warehouseDraftFitPreviewMechId = null;
            ClearSquadSelectionDraft();
            ClearSquadSelectionCompletedReplacement();
        }

        private void RunStartupHideSquadPreview()
        {
            MechBaySquadSelectionPreview squadPreview = MechBaySquadSelectionPreviewService.BuildPreview(demoInventory);
            bool completed = SquadSelectionCompleted(squadPreview);
            bool hadCue = HasSquadSelectionCompletedReplacement();
            string replacementText = hadCue ? SquadSelectionCompletedReplacementText() : "no replacement cue";
            showLoadoutPanel = true;
            showSquadSelectionPreview = false;
            ClearSquadSelectionDraft();
            if (!completed)
            {
                ClearSquadSelectionCompletedReplacement();
            }

            MechBayMissionHandoffPreview handoffPreview =
                MechBayMissionHandoffPreviewService.BuildPreview(demoInventory);
            MechBayMissionRestartApplyGuard guard =
                MechBayMissionHandoffPreviewService.BuildRestartApplyGuard(
                    demoInventory,
                    mission?.Contract,
                    combatProfiles);
            bool keptCue = HasSquadSelectionCompletedReplacement();
            string summary = keptCue
                ? MissionHandoffCompletedSummaryText(handoffPreview, guard)
                : MissionHandoffPlayerSummaryText(handoffPreview, guard);
            string launchText = keptCue
                ? MissionHandoffCompletedLaunchText(guard)
                : MissionHandoffLaunchActionText(guard, handoffPreview);
            bool summaryOk = keptCue
                && summary.IndexOf("updated squad", StringComparison.OrdinalIgnoreCase) >= 0
                && summary.IndexOf(replacementText, StringComparison.OrdinalIgnoreCase) >= 0;
            bool launchOk = keptCue
                && launchText.IndexOf("launch updated squad", StringComparison.OrdinalIgnoreCase) >= 0;
            bool backOk = string.Equals(SquadSelectionBackButtonLabel, "Back", StringComparison.Ordinal)
                && SquadSelectionBackButtonLabel.IndexOf("Hide", StringComparison.OrdinalIgnoreCase) < 0;
            bool reserveBackOk = string.Equals(ReserveFitBackButtonLabel, "Back", StringComparison.Ordinal)
                && ReserveFitBackButtonLabel.IndexOf("Hide", StringComparison.OrdinalIgnoreCase) < 0;
            string squadStatus = SquadSelectionStatusText(squadPreview?.Status);
            bool statusCopyOk = squadStatus.IndexOf("preview", StringComparison.OrdinalIgnoreCase) < 0
                && squadStatus.IndexOf("depot", StringComparison.OrdinalIgnoreCase) < 0
                && squadStatus.IndexOf("candidate", StringComparison.OrdinalIgnoreCase) < 0;
            bool accepted = completed
                && hadCue
                && !showSquadSelectionPreview
                && summaryOk
                && launchOk
                && backOk
                && reserveBackOk
                && statusCopyOk;
            string line = "hidden="
                + (!showSquadSelectionPreview)
                + " back="
                + SquadSelectionBackButtonLabel
                + "/"
                + ReserveFitBackButtonLabel
                + " squadStatus="
                + squadStatus
                + " completed="
                + completed
                + " cue="
                + keptCue
                + " summary="
                + summary
                + " launch="
                + launchText;
            if (accepted)
            {
                statusText = "Next contract squad set";
                AddCombatLogLine("CLI squad preview hide OK: " + replacementText);
                Debug.Log("MC2 commander squad preview hide: " + line);
                return;
            }

            startupSmokeFailed = true;
            statusText = "Squad preview hide failed";
            AddCombatLogLine("CLI squad preview hide failed: " + line);
            Debug.LogError("MC2 commander squad preview hide failed: " + line);
        }

        private void RunStartupPrepareDepotCandidate()
        {
            bool accepted = EnsureStartupDepotSwapCandidate();
            string message = accepted ? "prepared" : "blocked";
            AddCombatLogLine("CLI reserve candidate " + message);
            Debug.Log("MC2 commander reserve candidate: " + message);
        }

        private void RunStartupPrepareLocalCandidate()
        {
            MechBaySavedAccountContract beforeAccount = CurrentSavedAccountSnapshot();
            string ownedMechId = EnsureLocalReadyCandidate("CLI local", true);
            if (!string.IsNullOrWhiteSpace(ownedMechId))
            {
                string deltaText = RecordSavedAccountDelta(
                    beforeAccount,
                    CurrentSavedAccountSnapshot(),
                    "CLI reserve prep");
                TryAutoSaveSavedAccount("reserve prep " + deltaText);
                AddCombatLogLine("CLI reserve prep ready: " + ownedMechId);
                Debug.Log("MC2 commander reserve prep: ready ownedMechId=" + ownedMechId + " " + deltaText);
                return;
            }

            AddCombatLogLine("CLI reserve prep blocked");
            Debug.LogError("MC2 commander reserve prep blocked: status=" + statusText);
        }

        private string EnsureLocalReadyCandidate(string logPrefix, bool markStartupSmokeFailure)
        {
            if (demoInventory == null)
            {
                return BlockLocalCandidate(MechLabUnavailableText, logPrefix, markStartupSmokeFailure);
            }

            string existingCandidate = FirstStartupDepotCandidateOwnedMechId();
            if (!string.IsNullOrWhiteSpace(existingCandidate))
            {
                statusText = "Reserve prep ready";
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
                MissionReceiptLogText(logPrefix, receipt));
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
                return BlockLocalCandidate("Assembled mech missing before fit review", logPrefix, markStartupSmokeFailure);
            }

            if (target.hasDraftFitStub)
            {
                MechBayWarehouseDraftFitApplyResult result =
                    MechBayWarehouseDraftFitPreviewService.TryApplyDemoFit(demoInventory, targetOwnedMechId);
                if (result == null || !result.Accepted)
                {
                    return BlockLocalCandidate(result?.Message ?? "Fit review unavailable", logPrefix, markStartupSmokeFailure);
                }

                AddCombatLogLine(logPrefix + " fit: " + result.Message);
                RefreshDemoInventoryValidation();
            }

            string readyCandidate = FirstStartupDepotCandidateOwnedMechId();
            if (string.IsNullOrWhiteSpace(readyCandidate))
            {
                return BlockLocalCandidate("No next-squad candidate after local setup", logPrefix, markStartupSmokeFailure);
            }

            statusText = "Reserve prep ready";
            return readyCandidate;
        }

        private string BlockLocalCandidate(string reason, string logPrefix, bool markStartupSmokeFailure)
        {
            if (markStartupSmokeFailure)
            {
                startupSmokeFailed = true;
            }

            statusText = "Reserve prep blocked";
            Debug.LogError("MC2 commander reserve prep blocked: " + logPrefix + " " + reason);
            return null;
        }

        private bool EnsureStartupDepotSwapCandidate()
        {
            if (demoInventory == null)
            {
                startupSmokeFailed = true;
                statusText = "Reserve candidate blocked";
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
                    displayName = unitType + " Smoke Reserve",
                    activeLoadoutId = MechBayWarehouseDraftFitPreviewService.DemoWarehouseFitLoadoutId,
                    availableForMission = false,
                    conditionPercent = 100,
                    pilotId = "pilot-smoke-depot",
                    pilotDisplayName = "Smoke Reserve Pilot",
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
                existing.pilotDisplayName = "Smoke Reserve Pilot";
                existing.pilotType = "NPC";
            }

            RefreshDemoInventoryValidation();
            MechBaySquadSelectionPreview preview = MechBaySquadSelectionPreviewService.BuildPreview(demoInventory);
            bool ready = StartupDepotCandidateReady(preview);
            if (!ready)
            {
                startupSmokeFailed = true;
                statusText = "Reserve candidate blocked";
            }
            else
            {
                statusText = "Reserve candidate ready";
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
                AddCombatLogLine("CLI squad swap blocked: no reserve candidate");
                Debug.LogError("MC2 commander squad swap blocked: no reserve candidate");
                return;
            }

            MechBaySquadSelectionDraftState draft =
                MechBaySquadSelectionPreviewService.BuildDraftState(demoInventory, null, incomingOwnedMechId);
            MechBaySquadSelectionApplyResult result =
                MechBaySquadSelectionPreviewService.TryApplyPendingSwap(demoInventory, draft);
            if (result?.Accepted == true)
            {
                RecordSquadSelectionCompletedReplacement(draft);
                ClearSquadSelectionDraft();
                demoInventoryValidation = MechBayInventoryValidator.Validate(demoInventory);
                showLoadoutPanel = true;
                showSquadSelectionPreview = true;
                if (mission?.Result == MissionResultState.InProgress)
                {
                    SetPaused(true);
                }

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

        private void RunStartupCompleteVisibleObjectives()
        {
            if (mission == null)
            {
                startupSmokeFailed = true;
                statusText = "Debrief resolve failed";
                AddCombatLogLine("CLI resolve debrief failed: no mission");
                Debug.LogError("MC2 debrief resolve failed: no mission");
                return;
            }

            int completed = mission.CompleteVisibleObjectivesForPresentationAudit();
            CaptureMissionResult();
            MissionResultSummary summary = mission.ResultSummary;
            bool accepted = mission.Result != MissionResultState.InProgress
                && summary != null
                && summary.visibleObjectives > 0
                && summary.completedVisibleObjectives == summary.visibleObjectives;
            string result = "completed="
                + completed.ToString(CultureInfo.InvariantCulture)
                + " objectives="
                + (summary?.completedVisibleObjectives ?? 0).ToString(CultureInfo.InvariantCulture)
                + "/"
                + (summary?.visibleObjectives ?? 0).ToString(CultureInfo.InvariantCulture)
                + " result="
                + mission.Result;

            if (accepted)
            {
                AddCombatLogLine("CLI resolve debrief OK: " + result);
                Debug.Log("MC2 debrief resolve OK: " + result);
                return;
            }

            startupSmokeFailed = true;
            statusText = "Debrief resolve failed";
            AddCombatLogLine("CLI resolve debrief failed: " + result);
            Debug.LogError("MC2 debrief resolve failed: " + result);
        }

        private void RunStartupOpenDebrief()
        {
            if (TryOpenDebriefPanelForStartup(out string summary))
            {
                AddCombatLogLine("CLI debrief open OK: " + summary);
                Debug.Log("MC2 debrief open OK: " + summary);
                return;
            }

            startupSmokeFailed = true;
            statusText = "Debrief open failed";
            AddCombatLogLine("CLI debrief open failed: " + summary);
            Debug.LogError("MC2 debrief open failed: " + summary);
        }

        private void RunStartupDebriefSummaryAssertion()
        {
            DebriefSummaryAssertionResult result = BuildDebriefSummaryAssertion();
            if (result.Accepted)
            {
                AddCombatLogLine("CLI debrief summary assert OK: " + result.Summary);
                Debug.Log("MC2 debrief summary assertion OK: " + result.Summary);
                return;
            }

            startupSmokeFailed = true;
            statusText = "Debrief summary assertion failed";
            AddCombatLogLine("CLI debrief summary assert failed: " + result.Summary);
            Debug.LogError("MC2 debrief summary assertion failed: " + result.Summary);
        }

        private void RunStartupDebriefVisibleAssertion()
        {
            DebriefVisibleAssertionResult result = BuildDebriefVisibleAssertion();
            if (result.Accepted)
            {
                AddCombatLogLine("CLI debrief visible assert OK: " + result.Summary);
                Debug.Log("MC2 debrief visible assertion OK: " + result.Summary);
                return;
            }

            startupSmokeFailed = true;
            statusText = "Debrief visible assertion failed";
            AddCombatLogLine("CLI debrief visible assert failed: " + result.Summary);
            Debug.LogError("MC2 debrief visible assertion failed: " + result.Summary);
        }

        private bool TryOpenDebriefPanelForStartup(out string summary)
        {
            if (mission == null)
            {
                summary = "no mission";
                return false;
            }

            if (mission.Result == MissionResultState.InProgress)
            {
                summary = "mission still in progress";
                return false;
            }

            ApplyMissionReceiptOnce();
            showMissionResultPanel = true;
            showLoadoutPanel = false;
            showSystemPanel = false;
            showMissionListPanel = false;
            showStartupContinuePanel = false;
            showMissionMap = false;
            CloseTransientMechBayDrafts();
            pendingDetachedUnitId = null;
            pendingJumpOrder = false;
            SetPaused(true);
            SetDemoFlowScreen(DemoFlowScreen.Debrief);
            statusText = "Debrief open";
            DebriefVisibleAssertionResult assertion = BuildDebriefVisibleAssertion();
            summary = assertion.Summary;
            return assertion.Accepted;
        }

        private void RunStartupCommandResultAssertion(bool expectedBlocked, int expectedAcceptedCount)
        {
            CommandResultAssertionResult result = BuildStartupCommandResultAssertion(expectedBlocked, expectedAcceptedCount);
            if (result.Accepted)
            {
                AddCombatLogLine("CLI command result assert OK: " + result.Summary);
                Debug.Log("MC2 command result assertion OK: " + result.Summary);
                return;
            }

            startupSmokeFailed = true;
            statusText = "Command result assertion failed";
            AddCombatLogLine("CLI command result assert failed: " + result.Summary);
            Debug.LogError("MC2 command result assertion failed: " + result.Summary);
        }

        private CommandResultAssertionResult BuildStartupCommandResultAssertion(bool expectedBlocked, int expectedAcceptedCount)
        {
            if (lastStartupCommanderCommandResult == null)
            {
                return CommandResultAssertionResult.Blocked("No command result");
            }

            bool blockedOk = !expectedBlocked || !lastStartupCommanderCommandResult.Accepted;
            bool acceptedCountOk = expectedAcceptedCount < 0
                || (lastStartupCommanderCommandResult.Accepted
                    && lastStartupCommanderCommandResult.AcceptedCount == expectedAcceptedCount);
            string summary = "lastCommand="
                + (string.IsNullOrWhiteSpace(lastStartupCommanderCommandLine) ? "<empty>" : lastStartupCommanderCommandLine)
                + " accepted="
                + lastStartupCommanderCommandResult.Accepted
                + " count="
                + lastStartupCommanderCommandResult.AcceptedCount.ToString(CultureInfo.InvariantCulture)
                + (expectedBlocked ? " expected=blocked" : "")
                + (expectedAcceptedCount < 0 ? "" : " expectedAccepted=" + expectedAcceptedCount.ToString(CultureInfo.InvariantCulture))
                + " message="
                + lastStartupCommanderCommandResult.Message;

            return new CommandResultAssertionResult
            {
                Accepted = blockedOk && acceptedCountOk,
                Summary = summary
            };
        }

        private void RunStartupCombatSituationAssertion(string expectedTempoMode, int expectedSoloCount, int expectedJumpingCount, int expectedSalvageCount)
        {
            CombatSituationAssertionResult result = BuildCombatSituationAssertion(expectedTempoMode, expectedSoloCount, expectedJumpingCount, expectedSalvageCount);
            if (result.Accepted)
            {
                AddCombatLogLine("CLI combat situation assert OK: " + result.Summary);
                Debug.Log("MC2 combat situation assertion OK: " + result.Summary);
                return;
            }

            startupSmokeFailed = true;
            statusText = "Combat situation assertion failed";
            AddCombatLogLine("CLI combat situation assert failed: " + result.Summary);
            Debug.LogError("MC2 combat situation assertion failed: " + result.Summary);
        }

        private void RunStartupEncounterPacingAssertion()
        {
            EncounterPacingAssertionResult result = BuildEncounterPacingAssertion();
            if (result.Accepted)
            {
                AddCombatLogLine("CLI encounter pacing assert OK: " + result.Summary);
                Debug.Log("MC2 encounter pacing assertion OK: " + result.Summary);
                return;
            }

            startupSmokeFailed = true;
            statusText = "Encounter pacing assertion failed";
            AddCombatLogLine("CLI encounter pacing assert failed: " + result.Summary);
            Debug.LogError("MC2 encounter pacing assertion failed: " + result.Summary);
        }

        private void RunStartupObjectiveGraphAssertion()
        {
            ObjectiveGraphAssertionResult result = BuildObjectiveGraphAssertion();
            if (result.Accepted)
            {
                AddCombatLogLine("CLI objective graph assert OK: " + result.Summary);
                Debug.Log("MC2 objective graph assertion OK: " + result.Summary);
                return;
            }

            startupSmokeFailed = true;
            statusText = "Objective graph assertion failed";
            AddCombatLogLine("CLI objective graph assert failed: " + result.Summary);
            Debug.LogError("MC2 objective graph assertion failed: " + result.Summary);
        }

        private CombatSituationAssertionResult BuildCombatSituationAssertion(string expectedTempoMode, int expectedSoloCount, int expectedJumpingCount, int expectedSalvageCount)
        {
            if (mission == null)
            {
                return CombatSituationAssertionResult.Blocked("No mission");
            }

            string text = CombatSituationText();
            int playerReady = CountActivePlayerUnits();
            int playerSlots = CountPlayerUnits();
            int detached = CountDetachedPlayerUnits();
            int jumping = CountJumpingPlayerUnits();
            int activeHostiles = CountActiveHostileUnits();
            int liveTargets = CountLiveStructures();
            int salvage = CountDestroyedHostileUnits();
            bool textOk = text.IndexOf("Cmd ", StringComparison.OrdinalIgnoreCase) >= 0
                && text.IndexOf("Squad ", StringComparison.OrdinalIgnoreCase) >= 0
                && text.IndexOf("Solo ", StringComparison.OrdinalIgnoreCase) >= 0
                && text.IndexOf("Hostiles ", StringComparison.OrdinalIgnoreCase) >= 0
                && text.IndexOf("Targets ", StringComparison.OrdinalIgnoreCase) >= 0
                && (salvage <= 0
                    ? text.IndexOf("Salvage ", StringComparison.OrdinalIgnoreCase) < 0
                    : text.IndexOf("Salvage " + salvage.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase) >= 0)
                && (text.EndsWith("quiet", StringComparison.OrdinalIgnoreCase)
                    || text.EndsWith("contact", StringComparison.OrdinalIgnoreCase));
            bool countsOk = playerSlots > 0
                && playerReady >= 0
                && playerReady <= playerSlots
                && detached >= 0
                && detached <= playerSlots
                && activeHostiles >= 0
                && liveTargets >= 0;
            string topMode = TopStatusModeText(DemoFlowScreen.Battle);
            bool mapBackOk = string.Equals(MissionMapBackButtonLabel, "Back", StringComparison.Ordinal)
                && MissionMapBackButtonLabel.IndexOf("Close", StringComparison.OrdinalIgnoreCase) < 0;
            bool topModeOk = string.Equals(topMode, "MODE Battle", StringComparison.Ordinal)
                && topMode.IndexOf("FLOW", StringComparison.OrdinalIgnoreCase) < 0;
            string funds = TopStatusFundsText(1234);
            bool fundsOk = string.Equals(funds, "Funds 1,234", StringComparison.Ordinal)
                && funds.IndexOf("TOKEN", StringComparison.OrdinalIgnoreCase) < 0;
            string weaponFx = WeaponFxCueSummary();
            bool weaponFxOk = weaponFx.IndexOf("Energy=beam+pillar+muzzle+flash+scorch", StringComparison.Ordinal) >= 0
                && weaponFx.IndexOf("Missile=arc+blast+salvo-spread+crater", StringComparison.Ordinal) >= 0
                && weaponFx.IndexOf("Ballistic=tracer+sparks+muzzle+punch+debris", StringComparison.Ordinal) >= 0;
            string weaponStatusFx = WeaponStatusCueSummary();
            bool weaponStatusFxOk = weaponStatusFx.IndexOf("WeaponStatus=ENG-burn+MSL-blast+BAL-punch+D+B+KILL", StringComparison.Ordinal) >= 0
                && weaponStatusFx.IndexOf("ENG burn Alpha > Target Torso D24 B4", StringComparison.Ordinal) >= 0
                && weaponStatusFx.IndexOf("MSL blast Alpha > Target Legs D32 KILL", StringComparison.Ordinal) >= 0
                && weaponStatusFx.IndexOf("BAL punch Alpha > Target Arms D18", StringComparison.Ordinal) >= 0;
            string hitSeverityFx = HitSeverityCueSummary();
            bool hitSeverityFxOk = hitSeverityFx.IndexOf("HitSeverity=damage+kill+shock", StringComparison.Ordinal) >= 0;
            string hitDirectionFx = HitDirectionCueSummary();
            bool hitDirectionFxOk = hitDirectionFx.IndexOf("HitDirection=inbound+slash", StringComparison.Ordinal) >= 0;
            string muzzleFx = WeaponMuzzlePointCueSummary();
            bool muzzleFxOk = muzzleFx.IndexOf("WeaponMuzzlePoint=energy+missile+ballistic", StringComparison.Ordinal) >= 0;
            string armorFx = ArmorMitigationCueSummary();
            bool armorFxOk = armorFx.IndexOf("ArmorMitigation=glint+spark", StringComparison.Ordinal) >= 0;
            string sectionHitFx = SectionHitPointCueSummary();
            bool sectionHitFxOk = sectionHitFx.IndexOf("SectionHitPoint=cockpit+arms+legs+torso", StringComparison.Ordinal) >= 0;
            string sectionFlashFx = SectionHitCueSummary();
            bool sectionFlashFxOk = sectionFlashFx.IndexOf("SectionHitCue=cockpit+arms+legs+torso", StringComparison.Ordinal) >= 0;
            string sectionStatusFx = SectionStatusCueSummary();
            bool sectionStatusFxOk = sectionStatusFx.IndexOf("SectionStatus=bar+short-label+critical+destroyed", StringComparison.Ordinal) >= 0
                && SectionStatusLabelSamplesOk();
            string targetLineFx = TargetLineCueSummary();
            bool targetLineFxOk = targetLineFx.IndexOf("TargetLine=ready+cooling+blocked+endcap", StringComparison.Ordinal) >= 0;
            string rangeRingFx = RangeRingCueSummary();
            bool rangeRingFxOk = rangeRingFx.IndexOf("RangeRing=selection+target+heat", StringComparison.Ordinal) >= 0;
            string weaponReadinessFx = WeaponReadinessCueSummary();
            bool weaponReadinessFxOk = weaponReadinessFx.IndexOf("WeaponReadiness=ready+cooling+blocked+beacon", StringComparison.Ordinal) >= 0;
            string targetLockFx = TargetLockCueSummary();
            bool targetLockFxOk = targetLockFx.IndexOf("TargetLock=auto+command+beacon", StringComparison.Ordinal) >= 0;
            string commandFocusFx = CommandFocusCueSummary();
            bool commandFocusFxOk = commandFocusFx.IndexOf("CommandFocus=closing+ready+hot+cycling", StringComparison.Ordinal) >= 0;
            string squadFocusFx = SquadFocusCueSummary();
            bool squadFocusFxOk = squadFocusFx.IndexOf("SquadFocus=ring+pressure+beacon", StringComparison.Ordinal) >= 0;
            string threatFocusFx = ThreatFocusCueSummary();
            bool threatFocusFxOk = threatFocusFx.IndexOf("ThreatFocus=ring+warning+beacon", StringComparison.Ordinal) >= 0;
            string playerDamageFx = PlayerDamageCueSummary();
            bool playerDamageFxOk = playerDamageFx.IndexOf("PlayerDamage=warning+critical+beacon", StringComparison.Ordinal) >= 0;
            string combatPressureFx = CombatPressureCueSummary();
            bool combatPressureFxOk = combatPressureFx.IndexOf("CombatPressure=tracking+contact+fire+beacon", StringComparison.Ordinal) >= 0;
            string hostilePressureFx = HostilePressureCueSummary();
            bool hostilePressureFxOk = hostilePressureFx.IndexOf("HostilePressure=centroid+tempo+beacon+vector", StringComparison.Ordinal) >= 0;
            string sectionFx = DemoUnitView.SectionDamageCueSummary();
            bool sectionFxOk = sectionFx.IndexOf("Arms=missing-socket+flag+flight+landing-debris", StringComparison.Ordinal) >= 0
                && sectionFx.IndexOf("Legs=collapse+red-cross+skid+dust", StringComparison.Ordinal) >= 0
                && sectionFx.IndexOf("Cockpit=breach+ejection-pod+chute+landing+arc+distress", StringComparison.Ordinal) >= 0
                && sectionFx.IndexOf("Critical=smoke+sparks", StringComparison.Ordinal) >= 0
                && sectionFx.IndexOf("Ground=critical+lost+pilot", StringComparison.Ordinal) >= 0
                && sectionFx.IndexOf("Wreck=blast+smoke+marker+debris+salvage", StringComparison.Ordinal) >= 0;
            string heatFx = DemoUnitView.HeatCueSummary();
            bool heatFxOk = heatFx.IndexOf("Heat=vent+lock", StringComparison.Ordinal) >= 0;
            string objectiveFx = ObjectiveEventCueSummary();
            bool objectiveFxOk = objectiveFx.IndexOf("ObjectivePulse=active+complete+target", StringComparison.Ordinal) >= 0;
            string objectiveGuideFx = ObjectiveGuideCueSummary();
            bool objectiveGuideFxOk = objectiveGuideFx.IndexOf("ObjectiveGuide=active+beacon+target", StringComparison.Ordinal) >= 0;
            string objectiveRouteFx = ObjectiveRouteCueSummary();
            bool objectiveRouteFxOk = objectiveRouteFx.IndexOf("ObjectiveRoute=commander+target+endcaps", StringComparison.Ordinal) >= 0;
            string objectivePressureFx = ObjectivePressureCueSummary();
            bool objectivePressureFxOk = objectivePressureFx.IndexOf("ObjectivePressure=steady+damaged+critical", StringComparison.Ordinal) >= 0;
            string jetFx = DemoUnitView.JetCueSummary();
            bool jetFxOk = jetFx.IndexOf("Jet=takeoff+trail+landing", StringComparison.Ordinal) >= 0;
            string structureFx = DemoStructureView.StructureDamageCueSummary();
            bool structureFxOk = structureFx.IndexOf("Structure=scar+smoke+collapse+rubble", StringComparison.Ordinal) >= 0;
            string scriptFx = ScriptEventCueSummary();
            bool scriptFxOk = scriptFx.IndexOf("ScriptCue=ring+beacon+signal", StringComparison.Ordinal) >= 0;
            string resultFx = MissionResultCueSummary();
            bool resultFxOk = resultFx.IndexOf("ResultCue=complete+failed", StringComparison.Ordinal) >= 0;
            string commandFx = CommandCueSummary();
            bool commandFxOk = commandFx.IndexOf("Command=move+attack+single+blocked+partial+reject-unit", StringComparison.Ordinal) >= 0;
            string soloOrderFx = SoloOrderCueSummary();
            bool soloOrderFxOk = soloOrderFx.IndexOf("SoloOrder=ring+beacon", StringComparison.Ordinal) >= 0;
            string orderPathFx = OrderPathCueSummary();
            bool orderPathFxOk = orderPathFx.IndexOf("OrderPath=move+jet+attack+endcap", StringComparison.Ordinal) >= 0;
            string orderArrivalFx = OrderArrivalCueSummary();
            bool orderArrivalFxOk = orderArrivalFx.IndexOf("OrderArrival=move+jet+attack-ready", StringComparison.Ordinal) >= 0;
            string commanderFx = CommanderCueSummary();
            bool commanderFxOk = commanderFx.IndexOf("Commander=anchor+beacon", StringComparison.Ordinal) >= 0;
            string soloReturnFx = SoloReturnCueSummary();
            bool soloReturnFxOk = soloReturnFx.IndexOf("SoloReturn=ring+beacon", StringComparison.Ordinal) >= 0;
            string tempoText = CombatTempoText();
            string tempoMode = CombatTempoMode();
            bool tempoOk = CombatTempoTextMatchesState(tempoText);
            bool expectedTempoOk = string.IsNullOrWhiteSpace(expectedTempoMode)
                || string.Equals(tempoMode, expectedTempoMode, StringComparison.OrdinalIgnoreCase);
            bool expectedSoloOk = expectedSoloCount < 0 || detached == expectedSoloCount;
            bool expectedJumpingOk = expectedJumpingCount < 0 || jumping == expectedJumpingCount;
            bool expectedSalvageOk = expectedSalvageCount < 0 || salvage == expectedSalvageCount;
            string pressureText = CombatContactPressureText();
            bool pressureOk = CombatContactPressureTextMatchesState(pressureText);
            string focusText = CombatFocusText();
            bool focusOk = CombatFocusTextMatchesState(focusText);
            string pulseText = CombatTacticalPulseText();
            bool pulseOk = CombatTacticalPulseTextMatchesState(pulseText);
            LoadoutCompactCheck missionBrief = BuildMissionBriefPanelTextCheck();
            string summary = "squad="
                + playerReady.ToString(CultureInfo.InvariantCulture)
                + "/"
                + playerSlots.ToString(CultureInfo.InvariantCulture)
                + " solo="
                + detached.ToString(CultureInfo.InvariantCulture)
                + (expectedSoloCount < 0 ? "" : " expectedSolo=" + expectedSoloCount.ToString(CultureInfo.InvariantCulture))
                + " jumping="
                + jumping.ToString(CultureInfo.InvariantCulture)
                + (expectedJumpingCount < 0 ? "" : " expectedJumping=" + expectedJumpingCount.ToString(CultureInfo.InvariantCulture))
                + " hostiles="
                + activeHostiles.ToString(CultureInfo.InvariantCulture)
                + " targets="
                + liveTargets.ToString(CultureInfo.InvariantCulture)
                + " salvage="
                + salvage.ToString(CultureInfo.InvariantCulture)
                + (expectedSalvageCount < 0 ? "" : " expectedSalvage=" + expectedSalvageCount.ToString(CultureInfo.InvariantCulture))
                + " top="
                + topMode
                + " funds="
                + funds
                + " mapBack="
                + MissionMapBackButtonLabel
                + " fx="
                + weaponFx
                + " weaponStatusFx="
                + weaponStatusFx
                + " hitSeverityFx="
                + hitSeverityFx
                + " hitDirectionFx="
                + hitDirectionFx
                + " muzzleFx="
                + muzzleFx
                + " armorFx="
                + armorFx
                + " sectionHitFx="
                + sectionHitFx
                + " sectionFlashFx="
                + sectionFlashFx
                + " sectionStatusFx="
                + sectionStatusFx
                + " targetLineFx="
                + targetLineFx
                + " rangeRingFx="
                + rangeRingFx
                + " weaponReadinessFx="
                + weaponReadinessFx
                + " targetLockFx="
                + targetLockFx
                + " commandFocusFx="
                + commandFocusFx
                + " squadFocusFx="
                + squadFocusFx
                + " threatFocusFx="
                + threatFocusFx
                + " playerDamageFx="
                + playerDamageFx
                + " combatPressureFx="
                + combatPressureFx
                + " hostilePressureFx="
                + hostilePressureFx
                + " sectionFx="
                + sectionFx
                + " heatFx="
                + heatFx
                + " objectiveFx="
                + objectiveFx
                + " objectiveGuideFx="
                + objectiveGuideFx
                + " objectiveRouteFx="
                + objectiveRouteFx
                + " objectivePressureFx="
                + objectivePressureFx
                + " jetFx="
                + jetFx
                + " structureFx="
                + structureFx
                + " scriptFx="
                + scriptFx
                + " resultFx="
                + resultFx
                + " commandFx="
                + commandFx
                + " soloOrderFx="
                + soloOrderFx
                + " orderPathFx="
                + orderPathFx
                + " orderArrivalFx="
                + orderArrivalFx
                + " commanderFx="
                + commanderFx
                + " soloReturnFx="
                + soloReturnFx
                + " tempoMode="
                + tempoMode
                + " tempo="
                + tempoText
                + (string.IsNullOrWhiteSpace(expectedTempoMode) ? "" : " expectedTempo=" + expectedTempoMode)
                + " pressure="
                + pressureText
                + " focus="
                + focusText
                + " pulse="
                + pulseText
                + " "
                + missionBrief.Summary
                + " text="
                + text;

            return new CombatSituationAssertionResult
            {
                Accepted = textOk
                    && countsOk
                    && mapBackOk
                    && topModeOk
                    && fundsOk
                    && weaponFxOk
                    && weaponStatusFxOk
                    && hitSeverityFxOk
                    && hitDirectionFxOk
                    && muzzleFxOk
                    && armorFxOk
                    && sectionHitFxOk
                    && sectionFlashFxOk
                    && sectionStatusFxOk
                    && targetLineFxOk
                    && rangeRingFxOk
                    && weaponReadinessFxOk
                    && targetLockFxOk
                    && commandFocusFxOk
                    && squadFocusFxOk
                    && threatFocusFxOk
                    && playerDamageFxOk
                    && combatPressureFxOk
                    && hostilePressureFxOk
                    && sectionFxOk
                    && heatFxOk
                    && objectiveFxOk
                    && objectiveGuideFxOk
                    && objectiveRouteFxOk
                    && objectivePressureFxOk
                    && jetFxOk
                    && structureFxOk
                    && scriptFxOk
                    && resultFxOk
                    && commandFxOk
                    && soloOrderFxOk
                    && orderPathFxOk
                    && orderArrivalFxOk
                    && commanderFxOk
                    && soloReturnFxOk
                    && tempoOk
                    && expectedTempoOk
                    && expectedSoloOk
                    && expectedJumpingOk
                    && expectedSalvageOk
                    && pressureOk
                    && focusOk
                    && pulseOk
                    && missionBrief.Accepted,
                Summary = summary
            };
        }

        private static string CommanderCueSummary()
        {
            return "Commander=anchor+beacon";
        }

        private static string SoloReturnCueSummary()
        {
            return "SoloReturn=ring+beacon";
        }

        private static string SoloOrderCueSummary()
        {
            return "SoloOrder=ring+beacon";
        }

        private static string TargetLineCueSummary()
        {
            return "TargetLine=ready+cooling+blocked+endcap";
        }

        private static string RangeRingCueSummary()
        {
            return "RangeRing=selection+target+heat";
        }

        private static string WeaponReadinessCueSummary()
        {
            return "WeaponReadiness=ready+cooling+blocked+beacon";
        }

        private static string TargetLockCueSummary()
        {
            return "TargetLock=auto+command+beacon";
        }

        private static string CommandFocusCueSummary()
        {
            return "CommandFocus=closing+ready+hot+cycling";
        }

        private static string SquadFocusCueSummary()
        {
            return "SquadFocus=ring+pressure+beacon";
        }

        private static string ThreatFocusCueSummary()
        {
            return "ThreatFocus=ring+warning+beacon";
        }

        private static string PlayerDamageCueSummary()
        {
            return "PlayerDamage=warning+critical+beacon";
        }

        private static string CombatPressureCueSummary()
        {
            return "CombatPressure=tracking+contact+fire+beacon";
        }

        private static string HostilePressureCueSummary()
        {
            return "HostilePressure=centroid+tempo+beacon+vector";
        }

        private bool CombatTempoTextMatchesState(string tempoText)
        {
            if (string.IsNullOrWhiteSpace(tempoText) || !tempoText.StartsWith("Tempo ", StringComparison.Ordinal))
            {
                return false;
            }

            string mode = CombatTempoMode();
            if (string.Equals(mode, "fire", StringComparison.Ordinal))
            {
                return tempoText.StartsWith("Tempo Fire  hostile pressure ", StringComparison.Ordinal);
            }

            if (string.Equals(mode, "contact", StringComparison.Ordinal))
            {
                return tempoText.StartsWith("Tempo Contact  ", StringComparison.Ordinal)
                    && !string.IsNullOrWhiteSpace(lastContactLabel)
                    && tempoText.IndexOf(TruncateText(lastContactLabel, 24), StringComparison.Ordinal) >= 0;
            }

            if (string.Equals(mode, "tracking", StringComparison.Ordinal))
            {
                return tempoText.StartsWith("Tempo Tracking  hostiles ", StringComparison.Ordinal);
            }

            return string.Equals(tempoText, "Tempo Quiet  no contact", StringComparison.Ordinal);
        }

        private bool CombatContactPressureTextMatchesState(string pressureText)
        {
            if (string.IsNullOrWhiteSpace(pressureText)
                || !pressureText.StartsWith("Contacts ", StringComparison.Ordinal)
                || pressureText.Length > 56)
            {
                return false;
            }

            int airfield = CountLiveEncounterGroup("airfield");
            int north = CountLiveEncounterGroup("north");
            int ambush = CountLiveEncounterGroup("ambush");
            int starslayer = CountLiveEncounterGroup("starslayer");
            int activeHostiles = CountActiveHostileUnits();
            if (activeHostiles <= 0)
            {
                return string.Equals(pressureText, "Contacts none", StringComparison.Ordinal);
            }

            bool labelOk = pressureText.IndexOf("none", StringComparison.OrdinalIgnoreCase) < 0;
            if (airfield > 0)
            {
                labelOk = labelOk && pressureText.IndexOf("Air " + airfield.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal) >= 0;
            }

            if (north > 0)
            {
                labelOk = labelOk && pressureText.IndexOf("North " + north.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal) >= 0;
            }

            if (ambush > 0)
            {
                labelOk = labelOk && pressureText.IndexOf("Amb " + ambush.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal) >= 0;
            }

            if (starslayer > 0)
            {
                labelOk = labelOk && pressureText.IndexOf("Star " + starslayer.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal) >= 0;
            }

            return labelOk;
        }

        private bool CombatFocusTextMatchesState(string focusText)
        {
            if (string.IsNullOrWhiteSpace(focusText)
                || !focusText.StartsWith("Focus ", StringComparison.Ordinal)
                || focusText.Length > 48
                || focusText.IndexOf("unit-", StringComparison.OrdinalIgnoreCase) >= 0
                || focusText.IndexOf("structure-", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }

            string targetId = PrimaryCommandedFocusTargetId(out int focusCount);
            if (string.IsNullOrEmpty(targetId) || focusCount <= 0)
            {
                return string.Equals(focusText, "Focus none", StringComparison.Ordinal);
            }

            string expectedLabel = TruncateText(CombatTargetLabel(targetId), 18);
            bool stateOk = focusText.IndexOf(" ready ", StringComparison.Ordinal) >= 0
                || focusText.EndsWith(" closing", StringComparison.Ordinal)
                || focusText.EndsWith(" hot", StringComparison.Ordinal)
                || focusText.EndsWith(" cycling", StringComparison.Ordinal)
                || focusText.EndsWith(" tracking", StringComparison.Ordinal);
            return focusText.IndexOf(expectedLabel, StringComparison.Ordinal) >= 0
                && focusText.IndexOf(" x" + focusCount.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal) >= 0
                && focusText.IndexOf(" HP ", StringComparison.Ordinal) >= 0
                && stateOk;
        }

        private bool CombatTacticalPulseTextMatchesState(string pulseText)
        {
            if (string.IsNullOrWhiteSpace(pulseText)
                || pulseText.Length > 56
                || pulseText.IndexOf("Contacts ", StringComparison.OrdinalIgnoreCase) >= 0
                || pulseText.IndexOf("unit-", StringComparison.OrdinalIgnoreCase) >= 0
                || pulseText.IndexOf("structure-", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }

            string mode = CombatTempoMode();
            string stage = EncounterPacingStageText();
            bool modeOk = string.Equals(mode, "fire", StringComparison.Ordinal)
                ? pulseText.StartsWith("Fire " + stage + " hostiles ", StringComparison.Ordinal)
                : string.Equals(mode, "contact", StringComparison.Ordinal)
                    ? pulseText.StartsWith("Contact " + stage + " ", StringComparison.Ordinal)
                    : string.Equals(mode, "tracking", StringComparison.Ordinal)
                        ? pulseText.StartsWith("Tracking " + stage + " hostiles ", StringComparison.Ordinal)
                        : string.Equals(pulseText, "Quiet " + stage, StringComparison.Ordinal)
                            || pulseText.StartsWith("Quiet " + stage + "  Focus ", StringComparison.Ordinal);
            string targetId = PrimaryCommandedFocusTargetId(out int focusCount);
            bool focusOk = string.IsNullOrEmpty(targetId) || focusCount <= 0
                ? pulseText.IndexOf("Focus ", StringComparison.Ordinal) < 0
                : pulseText.IndexOf("Focus " + TruncateText(CombatTargetLabel(targetId), 14), StringComparison.Ordinal) >= 0
                    && pulseText.IndexOf(" x" + focusCount.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal) >= 0;
            return modeOk && focusOk;
        }

        private LoadoutCompactCheck BuildMissionBriefPanelTextCheck()
        {
            if (mission?.Objectives == null)
            {
                return new LoadoutCompactCheck(false, "missionBrief=unavailable");
            }

            int visible = 0;
            int compact = 0;
            bool stateOk = true;
            bool noEllipsis = true;
            bool hasAirfield = false;
            bool hasNorthIsland = false;
            bool hasExtraction = false;
            List<string> samples = new();
            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (objective?.Definition == null || objective.Definition.hidden)
                {
                    continue;
                }

                string line = MissionBriefObjectiveLine(objective);
                visible++;
                if (line.Length <= 28)
                {
                    compact++;
                }

                stateOk = stateOk
                    && (line.StartsWith("[done] ", StringComparison.Ordinal)
                        || line.StartsWith("[active] ", StringComparison.Ordinal)
                        || line.StartsWith("[locked] ", StringComparison.Ordinal));
                noEllipsis = noEllipsis && line.IndexOf("...", StringComparison.Ordinal) < 0;
                hasAirfield = hasAirfield || line.IndexOf("1 Airfield", StringComparison.OrdinalIgnoreCase) >= 0;
                hasNorthIsland = hasNorthIsland || line.IndexOf("4 North island", StringComparison.OrdinalIgnoreCase) >= 0;
                hasExtraction = hasExtraction || line.IndexOf("6 Extraction", StringComparison.OrdinalIgnoreCase) >= 0;

                if (samples.Count < 6)
                {
                    samples.Add(line);
                }
            }

            Rect panel = MissionBriefPanelRect();
            float objectiveStartY = MissionBriefObjectiveStartY(panel);
            float lineHeight = MissionBriefObjectiveLineHeight(panel, objectiveStartY, visible);
            int drawableRows = MissionBriefDrawableObjectiveRows(panel, objectiveStartY, lineHeight);
            bool panelFitsAllObjectives = drawableRows >= visible;
            bool accepted = visible == 6
                && compact == visible
                && stateOk
                && noEllipsis
                && hasAirfield
                && hasNorthIsland
                && hasExtraction
                && panelFitsAllObjectives;
            string summary = "missionBrief="
                + compact.ToString(CultureInfo.InvariantCulture)
                + "/"
                + visible.ToString(CultureInfo.InvariantCulture)
                + " rows="
                + drawableRows.ToString(CultureInfo.InvariantCulture)
                + "/"
                + visible.ToString(CultureInfo.InvariantCulture)
                + " lineH="
                + lineHeight.ToString("0.#", CultureInfo.InvariantCulture)
                + " "
                + string.Join("|", samples);
            return new LoadoutCompactCheck(accepted, summary);
        }

        private ObjectiveGraphAssertionResult BuildObjectiveGraphAssertion()
        {
            if (mission?.Contract?.objectives == null)
            {
                return ObjectiveGraphAssertionResult.Blocked("No mission objectives");
            }

            int visibleObjectives = 0;
            int hiddenObjectives = 0;
            foreach (ObjectiveDefinition objective in mission.Contract.objectives)
            {
                if (objective == null)
                {
                    continue;
                }

                if (objective.hidden)
                {
                    hiddenObjectives++;
                }
                else
                {
                    visibleObjectives++;
                }
            }

            ObjectiveDefinition airfield = FindContractObjective(0);
            ObjectiveDefinition hangar = FindContractObjective(1);
            ObjectiveDefinition firstPatrol = FindContractObjective(2);
            ObjectiveDefinition northRoute = FindContractObjective(3);
            ObjectiveDefinition northBandits = FindContractObjective(4);
            ObjectiveDefinition extraction = FindContractObjective(5);
            ObjectiveDefinition hiddenFirstPatrol = FindContractObjective(6);
            ObjectiveDefinition starslayerArea = FindContractObjective(7);
            ObjectiveDefinition starslayerVo = FindContractObjective(8);

            int firstPatrolTargets = CountContractConditions(firstPatrol, "DestroySpecificEnemyUnit");
            int hiddenFirstPatrolTargets = CountContractConditions(hiddenFirstPatrol, "DestroySpecificEnemyUnit");
            int northBanditTargets = CountContractConditions(northBandits, "DestroySpecificEnemyUnit");
            int starslayerVoTargets = CountContractConditions(starslayerVo, "DestroySpecificEnemyUnit");

            bool totalsOk = visibleObjectives == 6 && hiddenObjectives == 3;
            bool objectivePresenceOk = airfield != null
                && hangar != null
                && firstPatrol != null
                && northRoute != null
                && northBandits != null
                && extraction != null
                && hiddenFirstPatrol != null
                && starslayerArea != null
                && starslayerVo != null;
            bool edgeOk = HasObjectiveEdge(0, 1, "0")
                && HasObjectiveEdge(1, 2, "2")
                && HasObjectiveEdge(6, 3, "3")
                && HasObjectiveEdge(3, 4, "4")
                && HasObjectiveEdge(4, 5, "5");
            bool firstPatrolOk = firstPatrolTargets == 8
                && hiddenFirstPatrolTargets == 8
                && HasFlagAction(hiddenFirstPatrol, "3", true)
                && firstPatrol != null
                && !HasFlagAction(firstPatrol, "3", true);
            bool northRouteOk = northRoute != null
                && northRoute.activateOnFlag
                && string.Equals(northRoute.activateFlagId, "3", StringComparison.Ordinal)
                && HasMoveAreaCondition(northRoute, "MoveAnyUnitToArea", 832f, 2752f, 256f);
            bool northBanditOk = northBandits != null
                && northBandits.activateOnFlag
                && string.Equals(northBandits.activateFlagId, "4", StringComparison.Ordinal)
                && northBanditTargets == 4
                && HasFlagAction(northBandits, "5", true);
            bool extractionOk = extraction != null
                && extraction.activateOnFlag
                && string.Equals(extraction.activateFlagId, "5", StringComparison.Ordinal)
                && extraction.requiresAllPreviousPrimary
                && HasMoveAreaCondition(extraction, "MoveAllSurvivingMechsToArea", -4117.333f, 513.333f, 256f);
            bool starslayerOk = starslayerArea != null
                && starslayerArea.hidden
                && HasMoveAreaCondition(starslayerArea, "MoveAnyUnitToArea", -2240f, 1600f, 500f)
                && starslayerVo != null
                && starslayerVo.hidden
                && starslayerVoTargets == 6;
            string summary = "visible="
                + visibleObjectives.ToString(CultureInfo.InvariantCulture)
                + " hidden="
                + hiddenObjectives.ToString(CultureInfo.InvariantCulture)
                + " edges="
                + CountObjectiveEdges().ToString(CultureInfo.InvariantCulture)
                + " firstPatrol="
                + firstPatrolTargets.ToString(CultureInfo.InvariantCulture)
                + "/"
                + hiddenFirstPatrolTargets.ToString(CultureInfo.InvariantCulture)
                + " hidden6=flag3"
                + " north=flag3@832,2752"
                + " northBandits="
                + northBanditTargets.ToString(CultureInfo.InvariantCulture)
                + " extraction=flag5"
                + " starslayer=area7+vo"
                + starslayerVoTargets.ToString(CultureInfo.InvariantCulture);

            return new ObjectiveGraphAssertionResult
            {
                Accepted = totalsOk
                    && objectivePresenceOk
                    && edgeOk
                    && firstPatrolOk
                    && northRouteOk
                    && northBanditOk
                    && extractionOk
                    && starslayerOk,
                Summary = summary
            };
        }

        private ObjectiveDefinition FindContractObjective(int objectiveIndex)
        {
            if (mission?.Contract?.objectives == null)
            {
                return null;
            }

            foreach (ObjectiveDefinition objective in mission.Contract.objectives)
            {
                if (objective != null && objective.index == objectiveIndex)
                {
                    return objective;
                }
            }

            return null;
        }

        private int CountContractConditions(ObjectiveDefinition objective, string conditionType)
        {
            if (objective?.conditions == null)
            {
                return 0;
            }

            int count = 0;
            foreach (ObjectiveCondition condition in objective.conditions)
            {
                if (condition != null && string.Equals(condition.type, conditionType, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        private bool HasFlagAction(ObjectiveDefinition objective, string flagId, bool value)
        {
            if (objective?.actions == null)
            {
                return false;
            }

            foreach (ObjectiveAction action in objective.actions)
            {
                if (action?.flag != null
                    && string.Equals(action.type, "SetBooleanFlag", StringComparison.Ordinal)
                    && string.Equals(action.flag.id, flagId, StringComparison.Ordinal)
                    && action.flag.value == value)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasMoveAreaCondition(
            ObjectiveDefinition objective,
            string conditionType,
            float x,
            float y,
            float radius)
        {
            if (objective?.conditions == null)
            {
                return false;
            }

            foreach (ObjectiveCondition condition in objective.conditions)
            {
                TargetArea area = condition?.targetArea;
                if (area != null
                    && string.Equals(condition.type, conditionType, StringComparison.Ordinal)
                    && Approximately(area.x, x)
                    && Approximately(area.y, y)
                    && Approximately(area.radius, radius))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasObjectiveEdge(int from, int to, string flagId)
        {
            if (mission?.Contract?.objectiveEdges == null)
            {
                return false;
            }

            foreach (ObjectiveEdge edge in mission.Contract.objectiveEdges)
            {
                if (edge != null
                    && edge.from == from
                    && edge.to == to
                    && string.Equals(edge.flagId, flagId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private int CountObjectiveEdges()
        {
            int count = 0;
            if (mission?.Contract?.objectiveEdges == null)
            {
                return count;
            }

            foreach (ObjectiveEdge edge in mission.Contract.objectiveEdges)
            {
                if (edge != null)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool Approximately(float actual, float expected)
        {
            return Mathf.Abs(actual - expected) <= 0.05f;
        }

        private EncounterPacingAssertionResult BuildEncounterPacingAssertion()
        {
            if (mission == null)
            {
                return EncounterPacingAssertionResult.Blocked("No mission");
            }

            EncounterPacingCounts airfield = CountEncounterGroup("airfield");
            EncounterPacingCounts north = CountEncounterGroup("north");
            EncounterPacingCounts ambush = CountEncounterGroup("ambush");
            EncounterPacingCounts starslayer = CountEncounterGroup("starslayer");
            bool airfieldComplete = IsObjectiveCompleteForAssertion(0);
            bool hangarDamaged = IsHangarDamagedForAssertion();
            bool starslayerTriggered = IsObjectiveCompleteForAssertion(7);
            bool starslayerVoReady = IsObjectiveCompleteForAssertion(8);
            bool starslayerCleared = starslayer.Total > 0 && starslayer.Destroyed == starslayer.Total;
            string stage = EncounterPacingStage(airfieldComplete, hangarDamaged, starslayerTriggered);
            string stageText = EncounterPacingStageLabel(stage);
            bool totalsOk = airfield.Total > 0 && north.Total > 0 && ambush.Total > 0 && starslayer.Total > 0;
            bool voiceOverOk = !starslayerCleared || starslayerVoReady;
            string activationFx = EncounterActivationCueSummary();
            bool activationFxOk = activationFx.IndexOf("ContactWake=ring+beacon+ping+pips+vector", StringComparison.Ordinal) >= 0;
            bool stageTextOk = !string.IsNullOrWhiteSpace(stageText)
                && stageText.IndexOf(" ", StringComparison.Ordinal) < 0
                && stageText.Length <= 9;
            bool activeOk;
            if (!airfieldComplete)
            {
                activeOk = airfield.Active == 0 && north.Active == 0 && ambush.Active == 0 && starslayer.Active == 0;
            }
            else if (!hangarDamaged)
            {
                activeOk = airfield.Active == airfield.Total
                    && north.Active == north.Total
                    && ambush.Active == 0
                    && starslayer.Active == 0;
            }
            else if (!starslayerTriggered)
            {
                activeOk = airfield.Active == airfield.Total
                    && north.Active == north.Total
                    && ambush.Active == ambush.Total
                    && starslayer.Active == 0;
            }
            else
            {
                activeOk = airfield.Active == airfield.Total
                    && north.Active == north.Total
                    && ambush.Active == ambush.Total
                    && starslayer.Active == starslayer.Total;
            }

            string summary = "stage="
                + stage
                + " stageText="
                + stageText
                + " airfield="
                + airfield.Summary
                + " north="
                + north.Summary
                + " ambush="
                + ambush.Summary
                + " starslayer="
                + starslayer.Summary
                + " starslayerDestroyed="
                + starslayer.DestroyedSummary
                + " starslayerVO="
                + (starslayerVoReady ? "ready" : "pending")
                + " contract="
                + EncounterPacingCueSummary()
                + " activationFx="
                + activationFx;

            return new EncounterPacingAssertionResult
            {
                Accepted = totalsOk && activeOk && voiceOverOk && activationFxOk && stageTextOk,
                Summary = summary
            };
        }

        private static string EncounterPacingStage(bool airfieldComplete, bool hangarDamaged, bool starslayerTriggered)
        {
            if (!airfieldComplete)
            {
                return "initial";
            }

            if (!hangarDamaged)
            {
                return "airfield";
            }

            return starslayerTriggered ? "starslayer" : "hangar";
        }

        private string EncounterPacingStageText()
        {
            return EncounterPacingStageLabel(EncounterPacingStage(
                IsObjectiveCompleteForAssertion(0),
                IsHangarDamagedForAssertion(),
                IsObjectiveCompleteForAssertion(7)));
        }

        private static string EncounterPacingStageLabel(string stage)
        {
            switch (stage)
            {
                case "initial":
                    return "Initial";
                case "airfield":
                    return "Airfield";
                case "hangar":
                    return "Hangar";
                case "starslayer":
                    return "Star";
                default:
                    return "Battle";
            }
        }

        private static string EncounterPacingCueSummary()
        {
            return "Airfield=Pat1+eastLRM North=Pat2+Pat4 Ambush=hangar-damage Starslayer=area7";
        }

        private static string EncounterActivationCueSummary()
        {
            return "ContactWake=ring+beacon+ping+pips+vector";
        }

        private EncounterPacingCounts CountEncounterGroup(string groupKey)
        {
            EncounterPacingCounts counts = new();
            foreach (UnitState unit in mission.Units)
            {
                if (unit.IsPlayerUnit)
                {
                    continue;
                }

                if (!string.Equals(EncounterGroupKey(unit), groupKey, StringComparison.Ordinal))
                {
                    continue;
                }

                counts.Total++;
                if (unit.IsActive || unit.IsDestroyed)
                {
                    counts.Active++;
                }

                if (unit.IsDestroyed)
                {
                    counts.Destroyed++;
                }
            }

            return counts;
        }

        private int CountLiveEncounterGroup(string groupKey)
        {
            int count = 0;
            if (mission == null)
            {
                return count;
            }

            foreach (UnitState unit in mission.Units)
            {
                if (unit.IsPlayerUnit || unit.IsDestroyed || !unit.IsActive)
                {
                    continue;
                }

                if (string.Equals(EncounterGroupKey(unit), groupKey, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        private bool IsObjectiveCompleteForAssertion(int objectiveIndex)
        {
            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (objective.Definition.index == objectiveIndex)
                {
                    return objective.IsComplete;
                }
            }

            return false;
        }

        private bool IsHangarDamagedForAssertion()
        {
            StructureState hangar = mission.FindStructure("structure-1-0");
            return hangar != null && hangar.CurrentStructure < hangar.MaxStructure;
        }

        private void RunStartupLoadoutCompactAssertion()
        {
            LoadoutCompactAssertionResult result = BuildLoadoutCompactAssertion();
            if (result.Accepted)
            {
                AddCombatLogLine("CLI loadout compact assert OK: " + result.Summary);
                Debug.Log("MC2 loadout compact assertion OK: " + result.Summary);
                return;
            }

            startupSmokeFailed = true;
            statusText = "Loadout compact assertion failed";
            AddCombatLogLine("CLI loadout compact assert failed: " + result.Summary);
            Debug.LogError("MC2 loadout compact assertion failed: " + result.Summary);
        }

        private LoadoutCompactAssertionResult BuildLoadoutCompactAssertion()
        {
            UnitState unit = SelectedMechBayLoadoutUnit();
            if (unit == null)
            {
                return LoadoutCompactAssertionResult.Blocked("No selected loadout unit");
            }

            CombatLoadoutPreview preview = LoadoutPreviewFor(unit);
            CombatWeaponDefinition[] weapons = unit.Profile?.Weapons ?? Array.Empty<CombatWeaponDefinition>();
            if (preview == null || weapons.Length == 0 || weapons[0] == null)
            {
                return LoadoutCompactAssertionResult.Blocked("No loadout preview weapon");
            }

            string title = LoadoutUnitTitle(unit);
            string button = LoadoutWeaponButtonLabel(weapons[0], preview, 0, false, HasLoadoutWeaponPlacementOverride(unit, 0));
            bool titleOk = title.StartsWith("Fit ", StringComparison.Ordinal)
                && title.Length <= 46
                && title.IndexOf("#", StringComparison.Ordinal) < 0
                && title.IndexOf("demo", StringComparison.OrdinalIgnoreCase) < 0
                && title.IndexOf("mech-", StringComparison.OrdinalIgnoreCase) < 0
                && title.IndexOf("unit-", StringComparison.OrdinalIgnoreCase) < 0
                && title.IndexOf(" owned ", StringComparison.OrdinalIgnoreCase) < 0
                && title.IndexOf(" pilot ", StringComparison.OrdinalIgnoreCase) < 0;
            bool buttonOk = button.Length <= 12
                && button.IndexOf(" ", StringComparison.Ordinal) >= 0
                && button.IndexOf(TruncateText(weapons[0].name, 4), StringComparison.OrdinalIgnoreCase) < 0;
            bool heightOk = LoadoutGridSectionMinHeight >= 204f;
            LoadoutCompactCheck conditionCheck = BuildLoadoutConditionCompactCheck(unit);
            LoadoutCompactCheck pressureCheck = BuildProjectedLoadoutStatusCompactCheck(preview);
            LoadoutCompactCheck fitIdentityCheck = BuildMechBayFitIdentityCompactCheck(unit, preview);
            LoadoutCompactCheck editCheck = BuildLoadoutEditControlsCompactCheck(preview);
            LoadoutCompactCheck selectedResetCheck = BuildLoadoutSelectedResetCompactCheck();
            LoadoutCompactCheck targetCheck = BuildLoadoutTargetControlsCompactCheck();
            LoadoutCompactCheck nudgeCheck = BuildLoadoutNudgeCompactCheck();
            LoadoutCompactCheck placementHeaderCheck = BuildLoadoutPlacementHeaderCompactCheck(unit, preview);
            LoadoutCompactCheck blockLabelCheck = BuildLoadoutBlockLabelCompactCheck(unit, preview);
            LoadoutCompactCheck selectedSummaryCheck = BuildLoadoutSelectedSummaryCompactCheck(unit, preview, weapons[0]);
            LoadoutCompactCheck weaponDetailCheck = BuildLoadoutWeaponDetailCompactCheck(unit, preview);
            LoadoutCompactCheck componentDetailCheck = BuildLoadoutComponentDetailCompactCheck();
            LoadoutCompactCheck openSlotDetailCheck = BuildLoadoutOpenSlotDetailCompactCheck();
            LoadoutCompactCheck routeCheck = BuildLoadoutRouteCompactCheck();
            LoadoutCompactCheck baySummaryCheck = BuildMechLabSummaryLabelCheck();
            LoadoutCompactCheck actionCopyCheck = BuildMechLabActionCopyCheck();
            LoadoutCompactCheck receiptLogCheck = BuildMissionReceiptLogCopyCheck();
            LoadoutCompactCheck rosterCopyCheck = BuildMechLabRosterCopyCheck();
            string summary = "title="
                + title
                + " button="
                + button
                + " gridMin="
                + LoadoutGridSectionMinHeight.ToString(CultureInfo.InvariantCulture)
                + " "
                + conditionCheck.Summary
                + " "
                + pressureCheck.Summary
                + " "
                + fitIdentityCheck.Summary
                + " "
                + editCheck.Summary
                + " "
                + selectedResetCheck.Summary
                + " "
                + targetCheck.Summary
                + " "
                + nudgeCheck.Summary
                + " "
                + placementHeaderCheck.Summary
                + " "
                + blockLabelCheck.Summary
                + " "
                + selectedSummaryCheck.Summary
                + " "
                + weaponDetailCheck.Summary
                + " "
                + componentDetailCheck.Summary
                + " "
                + openSlotDetailCheck.Summary
                + " "
                + routeCheck.Summary
                + " "
                + baySummaryCheck.Summary
                + " "
                + actionCopyCheck.Summary
                + " "
                + receiptLogCheck.Summary
                + " "
                + rosterCopyCheck.Summary;

            return new LoadoutCompactAssertionResult
            {
                Accepted = titleOk
                    && buttonOk
                    && heightOk
                    && conditionCheck.Accepted
                    && pressureCheck.Accepted
                    && fitIdentityCheck.Accepted
                    && editCheck.Accepted
                    && selectedResetCheck.Accepted
                    && targetCheck.Accepted
                    && nudgeCheck.Accepted
                    && placementHeaderCheck.Accepted
                    && blockLabelCheck.Accepted
                    && selectedSummaryCheck.Accepted
                    && weaponDetailCheck.Accepted
                    && componentDetailCheck.Accepted
                    && openSlotDetailCheck.Accepted
                    && routeCheck.Accepted
                    && baySummaryCheck.Accepted
                    && actionCopyCheck.Accepted
                    && receiptLogCheck.Accepted
                    && rosterCopyCheck.Accepted,
                Summary = summary
            };
        }

        private LoadoutCompactCheck BuildLoadoutRouteCompactCheck()
        {
            string flow = DemoFlowScreenName(demoFlowScreen);
            bool panelOk = showLoadoutPanel;
            bool flowOk = string.Equals(flow, "Mech Lab", StringComparison.Ordinal);
            bool statusOk = statusText?.IndexOf("Mech Lab", StringComparison.OrdinalIgnoreCase) >= 0;
            bool titleOk = LoadoutPanelFrameTitle.StartsWith("Mech Lab", StringComparison.Ordinal)
                && LoadoutPanelHeaderTitle.StartsWith("Mech Lab", StringComparison.Ordinal);
            Rect desktopPanel = LoadoutPanelRectFor(1280f, 720f);
            float desktopInnerWidth = desktopPanel.width - 28f;
            float desktopGap = 14f;
            float desktopBayWidth = MechLabDedicatedBayWidthFor(desktopInnerWidth, desktopGap);
            float desktopLoadoutWidth = desktopInnerWidth - desktopBayWidth - desktopGap;
            bool dedicatedLayoutOk = ShouldUseDedicatedMechLabLayout(desktopPanel)
                && desktopPanel.width >= 1000f
                && desktopPanel.x <= 120f
                && desktopBayWidth >= 420f
                && desktopLoadoutWidth >= MechLabDedicatedLoadoutMinWidth
                && string.Equals(MechLabCompanyBayPanelTitle, "Company Bay", StringComparison.Ordinal)
                && string.Equals(MechLabLoadoutPanelTitle, "Loadout", StringComparison.Ordinal)
                && MechLabDedicatedBaySummaryHeight >= 500f;
            bool bayPageOk = selectedMechLabBayPage == MechLabBayOpsPageIndex
                && string.Equals(MechLabBayOpsPageLabel, "Ops", StringComparison.Ordinal)
                && string.Equals(MechLabBayRosterPageLabel, "Roster", StringComparison.Ordinal);
            bool backOk = string.Equals(MechLabBackButtonLabel, "Back", StringComparison.Ordinal)
                && MechLabBackButtonLabel.IndexOf("Close", StringComparison.OrdinalIgnoreCase) < 0;
            string topMode = TopStatusModeText(demoFlowScreen);
            bool topModeOk = string.Equals(topMode, "MODE Mech Lab", StringComparison.Ordinal)
                && topMode.IndexOf("FLOW", StringComparison.OrdinalIgnoreCase) < 0;
            string funds = TopStatusFundsText(5678);
            bool fundsOk = string.Equals(funds, "Funds 5,678", StringComparison.Ordinal)
                && funds.IndexOf("TOKEN", StringComparison.OrdinalIgnoreCase) < 0;
            string summary = "route="
                + flow
                + " top="
                + topMode
                + " funds="
                + funds
                + " panel="
                + panelOk
                + " layout="
                + (dedicatedLayoutOk ? "Dedicated" : "Compact")
                + " layoutW="
                + FormatDecimal(desktopPanel.width)
                + " bayW="
                + FormatDecimal(desktopBayWidth)
                + " fitW="
                + FormatDecimal(desktopLoadoutWidth)
                + " bay="
                + MechLabCompanyBayPanelTitle
                + " fit="
                + MechLabLoadoutPanelTitle
                + " page="
                + MechLabBayOpsPageLabel
                + "/"
                + MechLabBayRosterPageLabel
                + " status="
                + TruncateText(statusText ?? string.Empty, 30)
                + " back="
                + MechLabBackButtonLabel;

            return new LoadoutCompactCheck(
                panelOk && flowOk && statusOk && titleOk && dedicatedLayoutOk && bayPageOk && backOk && topModeOk && fundsOk,
                summary);
        }

        private static LoadoutCompactCheck BuildMechLabSummaryLabelCheck()
        {
            bool accepted = string.Equals(MechLabReadyLabel, "Bay Ready", StringComparison.Ordinal)
                && MechLabReviewPrefix.StartsWith("Bay Review", StringComparison.Ordinal)
                && string.Equals(MechLabLoadingText, "Bay loading", StringComparison.Ordinal)
                && MechLabLoadingText.IndexOf("Inventory", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(MechLabUnavailableText, "Bay unavailable", StringComparison.Ordinal)
                && MechLabUnavailableText.IndexOf("Inventory", StringComparison.OrdinalIgnoreCase) < 0
                && MechLabCompanyPrefix.StartsWith("Company", StringComparison.Ordinal)
                && string.Equals(MechLabPartsLabel, "Parts", StringComparison.Ordinal)
                && MechLabBuildPrefix.StartsWith("Build", StringComparison.Ordinal)
                && string.Equals(MechLabFundsText(13500), "Funds 13,500", StringComparison.Ordinal)
                && MechLabFundsText(13500).IndexOf("Token", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(MechLabBayOpsPageLabel, "Ops", StringComparison.Ordinal)
                && string.Equals(MechLabBayRosterPageLabel, "Roster", StringComparison.Ordinal)
                && SavedAccountIdleLabel.IndexOf("save/load", StringComparison.OrdinalIgnoreCase) < 0
                && SavedAccountIdleLabel.IndexOf("Idle", StringComparison.OrdinalIgnoreCase) < 0
                && SavedAccountPathReadyStatusText.StartsWith("Save slot", StringComparison.Ordinal)
                && SavedAccountPathReadyStatusText.IndexOf("account", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(SavedAccountNoLoadPreviewText, "No save checked", StringComparison.Ordinal)
                && SavedAccountNoLoadPreviewText.IndexOf("Idle", StringComparison.OrdinalIgnoreCase) < 0
                && SavedAccountNoLoadPreviewText.IndexOf("import apply", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(SavedAccountLoadPathLabel, "Load", StringComparison.Ordinal)
                && SavedAccountLoadPreviewPrefix.StartsWith("Load Check", StringComparison.Ordinal)
                && SavedAccountLoadPreviewPrefix.IndexOf("Import", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(SavedAccountCheckButtonLabel, "Check", StringComparison.Ordinal)
                && SavedAccountCheckButtonLabel.IndexOf("Preview", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(SavedAccountLoadButtonLabel, "Load", StringComparison.Ordinal)
                && string.Equals(SavedAccountSlotPathPrefix, "Slot ", StringComparison.Ordinal)
                && string.Equals(SavedAccountNoSlotPathText, "No slot path", StringComparison.Ordinal)
                && string.Equals(SavedAccountDefaultPathResultText, "Save Slot path", StringComparison.Ordinal)
                && SavedAccountDefaultPathResultText.IndexOf("Default", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(SavedAccountResultPrefix, "Save Result ", StringComparison.Ordinal)
                && SavedAccountResultPrefix.IndexOf("Last", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(SavedAccountSlotButtonLabel, "Slot", StringComparison.Ordinal)
                && SavedAccountSlotButtonLabel.IndexOf("Default", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(SavedAccountRoundtripReadyStatusText, "Save check ready", StringComparison.Ordinal)
                && SavedAccountRoundtripReadyStatusText.IndexOf("preview", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(SavedAccountRoundtripFailedStatusText, "Save check failed", StringComparison.Ordinal)
                && SavedAccountRoundtripFailedStatusText.IndexOf("preview", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(SavedAccountLoadCheckReadyStatusText, "Load check ready", StringComparison.Ordinal)
                && SavedAccountLoadCheckReadyStatusText.IndexOf("account", StringComparison.OrdinalIgnoreCase) < 0
                && SavedAccountLoadCheckReadyStatusText.IndexOf("preview", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(SavedAccountLoadCheckFailedStatusText, "Load check failed", StringComparison.Ordinal)
                && SavedAccountLoadCheckFailedStatusText.IndexOf("account", StringComparison.OrdinalIgnoreCase) < 0
                && SavedAccountLoadCheckFailedStatusText.IndexOf("preview", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(SavedAccountLoadBlockedStatusText, "Load blocked", StringComparison.Ordinal)
                && string.Equals(SavedAccountLoadFailedStatusText, "Load failed", StringComparison.Ordinal)
                && string.Equals(SavedAccountLoadedStatusText, "Save loaded", StringComparison.Ordinal)
                && SavedAccountLoadedStatusText.IndexOf("import", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(SaveSlotTitleReadyText, "Company ready", StringComparison.Ordinal)
                && string.Equals(SaveSlotResultLabel, "Result ", StringComparison.Ordinal)
                && SaveSlotResultLabel.IndexOf("Last", StringComparison.OrdinalIgnoreCase) < 0
                && SaveSlotResetConfirmText.IndexOf("default", StringComparison.OrdinalIgnoreCase) < 0
                && SaveSlotResetConfirmText.IndexOf("copied first", StringComparison.OrdinalIgnoreCase) >= 0
                && SaveSlotNewCompanyConfirmText.IndexOf("save stays untouched", StringComparison.OrdinalIgnoreCase) >= 0
                && string.Equals(SaveSlotConfirmResetButtonLabel, "Reset Slot", StringComparison.Ordinal)
                && string.Equals(SaveSlotStartFreshButtonLabel, "Start Fresh", StringComparison.Ordinal)
                && string.Equals(SaveSlotResetFreshDetailText, "fresh save", StringComparison.Ordinal)
                && SaveSlotResetFreshDetailText.IndexOf("default", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(SaveSlotResetOldCopyPrefix, "old copy ", StringComparison.Ordinal)
                && SaveSlotResetOldCopyPrefix.IndexOf("default", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(SaveSlotNoSavedCopyText, "No saved game to copy", StringComparison.Ordinal)
                && SaveSlotNoSavedCopyText.IndexOf("default", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(SaveSlotContinueCheckFailedText, "Save check failed", StringComparison.Ordinal)
                && SavedAccountStartupContinueReviewText("Preview failed").IndexOf("preview", StringComparison.OrdinalIgnoreCase) < 0
                && SavedAccountStartupContinueReviewText("JSON import apply preview rejected").IndexOf("import", StringComparison.OrdinalIgnoreCase) < 0
                && SavedAccountStartupContinueReviewText("JSON import apply preview rejected").IndexOf("account", StringComparison.OrdinalIgnoreCase) < 0
                && !SaveGameUiEnabled;
            string summary = "bayLabels="
                + MechLabReadyLabel
                + "/"
                + MechLabCompanyPrefix.Trim()
                + "/"
                + MechLabPartsLabel
                + "/"
                + MechLabBuildPrefix.Trim()
                + "/"
                + SavedAccountIdleLabel
                + " bayFunds="
                + MechLabFundsText(13500)
                + " bayState="
                + MechLabLoadingText
                + "/"
                + MechLabUnavailableText
                + " bayPages="
                + MechLabBayOpsPageLabel
                + "/"
                + MechLabBayRosterPageLabel
                + " saveCopy="
                + SavedAccountPathReadyStatusText
                + "/"
                + SavedAccountNoLoadPreviewText
                + "/"
                + SavedAccountLoadPreviewPrefix.Trim()
                + " slotPath="
                + SavedAccountSlotPathPrefix.Trim()
                + "/"
                + SavedAccountNoSlotPathText
                + " result="
                + SavedAccountResultPrefix.Trim();
            summary += " roundtrip="
                + SavedAccountRoundtripReadyStatusText
                + "/"
                + SavedAccountRoundtripFailedStatusText;
            summary += " loadStatus="
                + SavedAccountLoadCheckReadyStatusText
                + "/"
                + SavedAccountLoadBlockedStatusText
                + "/"
                + SavedAccountLoadedStatusText;
            summary += " slotButton="
                + SavedAccountSlotButtonLabel
                + " title="
                + SaveSlotTitleReadyText
                + " confirm="
                + SaveSlotStartFreshButtonLabel
                + " reset="
                + SaveSlotResetFreshDetailText
                + " continueCheck="
                + SaveSlotContinueCheckFailedText
                + " saveUi="
                + (SaveGameUiEnabled ? "Visible" : "Hidden");

            return new LoadoutCompactCheck(accepted, summary);
        }

        private static LoadoutCompactCheck BuildMechLabActionCopyCheck()
        {
            string purchaseReady = PlayerMechLabActionStatusText("Ready demo purchase");
            string hireReady = PlayerMechLabActionStatusText("Ready demo hire");
            string unavailable = PlayerMechLabActionStatusText("Inventory missing");
            string repairBlocked = RepairAllBlockedFundsText(1200);
            string repairDone = RepairAllCompleteStatusText(1, 1200);
            string shopSummary = WeaponShopPreviewText(new MechBayWeaponShopPreview
            {
                Entries = new[]
                {
                    new MechBayWeaponShopEntry
                    {
                        tokenCost = 400,
                        canAfford = true
                    }
                }
            });
            string purchaseSummary = WeaponShopPurchaseStubText(new MechBayWeaponPurchasePreviewResult
            {
                displayName = "AC/5",
                TokenCost = 450,
                Message = "Ready demo purchase"
            });
            string pilotSummary = PilotHirePreviewText(new MechBayPilotHirePreview
            {
                Candidates = new[]
                {
                    new MechBayPilotHireCandidate
                    {
                        hireCost = 600,
                        canAfford = true
                    }
                }
            });
            bool accepted = string.Equals(purchaseReady, "Ready to buy", StringComparison.Ordinal)
                && purchaseReady.IndexOf("demo", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(hireReady, "Ready to hire", StringComparison.Ordinal)
                && hireReady.IndexOf("demo", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(unavailable, MechLabUnavailableText, StringComparison.Ordinal)
                && string.Equals(repairBlocked, "Need 1,200 funds for all repairs", StringComparison.Ordinal)
                && repairBlocked.IndexOf("token", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(repairDone, "Repaired 1 mech for 1,200 funds", StringComparison.Ordinal)
                && repairDone.IndexOf("token", StringComparison.OrdinalIgnoreCase) < 0
                && shopSummary.IndexOf("400 funds", StringComparison.OrdinalIgnoreCase) >= 0
                && shopSummary.IndexOf("token", StringComparison.OrdinalIgnoreCase) < 0
                && purchaseSummary.IndexOf("450 funds", StringComparison.OrdinalIgnoreCase) >= 0
                && purchaseSummary.IndexOf("token", StringComparison.OrdinalIgnoreCase) < 0
                && pilotSummary.IndexOf("NPC pilot from 600 funds", StringComparison.OrdinalIgnoreCase) >= 0
                && pilotSummary.IndexOf("token", StringComparison.OrdinalIgnoreCase) < 0
                && !ContainsPrototypeCopy(pilotSummary);
            return new LoadoutCompactCheck(
                accepted,
                "shopHire="
                + purchaseReady
                + "/"
                + hireReady
                + "/"
                + unavailable
                + " repair="
                + repairBlocked
                + "/"
                + repairDone
                + " shop="
                + shopSummary
                + " buy="
                + purchaseSummary
                + " pilotSummary="
                + pilotSummary);
        }

        private static LoadoutCompactCheck BuildMissionReceiptLogCopyCheck()
        {
            MechBayMissionReceipt receipt = MechBayMissionReceiptService.BuildMissionReceipt(new MissionResultSummary
            {
                completedRewardResourcePoints = 3000,
                destroyedEnemyUnitLabels = new[]
                {
                    "enemy-1 Shadow Cat"
                }
            });
            string battleLog = MissionReceiptLogText(null, receipt);
            string prepLog = MissionReceiptLogText("Reserve prep", receipt);
            bool accepted = string.Equals(battleLog, "Payout +3,000  Salvage +1", StringComparison.Ordinal)
                && string.Equals(prepLog, "Reserve prep payout +3,000  Salvage +1", StringComparison.Ordinal)
                && battleLog.IndexOf("Receipt", StringComparison.OrdinalIgnoreCase) < 0
                && battleLog.IndexOf("token", StringComparison.OrdinalIgnoreCase) < 0
                && battleLog.IndexOf("fragments", StringComparison.OrdinalIgnoreCase) < 0
                && prepLog.IndexOf("Receipt", StringComparison.OrdinalIgnoreCase) < 0
                && prepLog.IndexOf("token", StringComparison.OrdinalIgnoreCase) < 0
                && prepLog.IndexOf("fragments", StringComparison.OrdinalIgnoreCase) < 0;
            return new LoadoutCompactCheck(
                accepted,
                "receiptLog="
                + battleLog
                + "/"
                + prepLog);
        }

        private static LoadoutCompactCheck BuildMechLabRosterCopyCheck()
        {
            string loadout = PlayerRosterStatusText("Needs loadout");
            string fitReady = PlayerFitStatusText("Read-only draft fit preview");
            string fitLocked = PlayerFitStatusText("Draft fitting locked for this demo");
            string depotHeld = PlayerRosterStatusText("Held: needs depot fit");
            string futureDeploy = PlayerRosterStatusText("Future depot deployment flow");
            string selection = PlayerRosterStatusText("Finish fit before selection");
            string squadOpen = NextSquadOpenStatusText("Reserve preview candidate");
            bool accepted = string.Equals(loadout, "Needs fit", StringComparison.Ordinal)
                && string.Equals(fitReady, "Ready fit review", StringComparison.Ordinal)
                && string.Equals(fitLocked, "Fit review locked", StringComparison.Ordinal)
                && string.Equals(depotHeld, "Held: needs reserve fit", StringComparison.Ordinal)
                && string.Equals(futureDeploy, "Reserve not ready", StringComparison.Ordinal)
                && string.Equals(selection, "Finish fit first", StringComparison.Ordinal)
                && squadOpen.StartsWith("Next squad open: ", StringComparison.Ordinal)
                && string.Equals(MechLabRosterEmptyText, "No mechs in bay", StringComparison.Ordinal)
                && MechLabRosterEmptyText.IndexOf("owned", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(MechLabRosterStateLabel, "Squad State ", StringComparison.Ordinal)
                && MechLabRosterStateLabel.IndexOf("Mission", StringComparison.OrdinalIgnoreCase) < 0
                && !ContainsPrototypeCopy(loadout)
                && !ContainsPrototypeCopy(fitReady)
                && !ContainsPrototypeCopy(fitLocked)
                && !ContainsPrototypeCopy(depotHeld)
                && !ContainsPrototypeCopy(futureDeploy)
                && !ContainsPrototypeCopy(selection)
                && !ContainsPrototypeCopy(squadOpen);
            return new LoadoutCompactCheck(
                accepted,
                "rosterCopy="
                + loadout
                + "/"
                + fitReady
                + "/"
                + fitLocked
                + "/"
                + depotHeld
                + "/"
                + futureDeploy
                + "/"
                + selection
                + " squadOpen="
                + squadOpen
                + " rosterUi="
                + MechLabRosterEmptyText
                + "/"
                + MechLabRosterStateLabel.Trim());
        }

        private static bool ContainsPrototypeCopy(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            return text.IndexOf("demo", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("depot", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("future", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("preview", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("candidate", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("read-only", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("loadout", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private LoadoutCompactCheck BuildLoadoutConditionCompactCheck(UnitState unit)
        {
            string conditionLine = MechConditionCompactText(unit);
            string healthyLine = MechConditionCompactText(100, 0);
            string damagedLine = MechConditionCompactText(74, 1200);
            string healthyRepairLabel = LoadoutRepairButtonLabel(0);
            string damagedRepairLabel = LoadoutRepairButtonLabel(1200);
            bool accepted = conditionLine.StartsWith(LoadoutConditionPrefix, StringComparison.Ordinal)
                && conditionLine.IndexOf("Condition", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(healthyLine, "Cond 100%", StringComparison.Ordinal)
                && string.Equals(damagedLine, "Cond 74%  Repair 1,200", StringComparison.Ordinal)
                && string.Equals(healthyRepairLabel, "OK", StringComparison.Ordinal)
                && string.Equals(damagedRepairLabel, "Repair", StringComparison.Ordinal)
                && healthyLine.IndexOf("Repair", StringComparison.OrdinalIgnoreCase) < 0
                && damagedLine.IndexOf("Repair 0", StringComparison.OrdinalIgnoreCase) < 0
                && LoadoutRepairButtonWidth <= 64f
                && LoadoutRepairStateOffset <= 70f;
            return new LoadoutCompactCheck(
                accepted,
                "condition="
                + conditionLine
                + " damaged="
                + damagedLine
                + " repairButton="
                + healthyRepairLabel
                + "/"
                + damagedRepairLabel
                + " repairW="
                + LoadoutRepairButtonWidth.ToString(CultureInfo.InvariantCulture));
        }

        private static LoadoutCompactCheck BuildProjectedLoadoutStatusCompactCheck(CombatLoadoutPreview preview)
        {
            string stateText = ProjectedLoadoutStateText(preview);
            string pressureText = ProjectedLoadoutPressureText(preview);
            LoadoutValidationResult result = preview?.Validation;
            bool stateOk = string.Equals(stateText, LoadoutFitOkLabel, StringComparison.Ordinal)
                || stateText.StartsWith(LoadoutFitReviewPrefix, StringComparison.Ordinal);
            bool overLimit = preview != null
                && result != null
                && ((preview.HeatLimit > 0f && result.TotalHeat > preview.HeatLimit)
                    || (preview.WeightLimit > 0f && result.TotalWeight > preview.WeightLimit)
                    || (preview.GridCapacity > 0 && result.OccupiedGridCells > preview.GridCapacity));
            bool accepted = stateOk
                && stateText.Length <= 22
                && stateText.IndexOf("...", StringComparison.Ordinal) < 0
                && stateText.IndexOf(".", StringComparison.Ordinal) < 0
                && stateText.IndexOf("exceeds", StringComparison.OrdinalIgnoreCase) < 0
                && stateText.IndexOf("limit", StringComparison.OrdinalIgnoreCase) < 0
                && pressureText.StartsWith(LoadoutHeatPressureLabel, StringComparison.Ordinal)
                && pressureText.IndexOf("  " + LoadoutWeightPressureLabel, StringComparison.Ordinal) >= 0
                && pressureText.IndexOf("  " + LoadoutGridPressureLabel, StringComparison.Ordinal) >= 0
                && (pressureText.IndexOf("!", StringComparison.Ordinal) >= 0) == overLimit
                && !ContainsPrototypeCopy(stateText)
                && stateText.IndexOf("Fit Review", StringComparison.OrdinalIgnoreCase) < 0
                && pressureText.IndexOf("Heat", StringComparison.OrdinalIgnoreCase) < 0
                && pressureText.IndexOf("Load ", StringComparison.OrdinalIgnoreCase) < 0
                && pressureText.IndexOf("Grid", StringComparison.OrdinalIgnoreCase) < 0
                && pressureText.Length <= 38;
            return new LoadoutCompactCheck(
                accepted,
                "pressure="
                + stateText
                + "/"
                + pressureText);
        }

        private LoadoutCompactCheck BuildMechBayFitIdentityCompactCheck(UnitState unit, CombatLoadoutPreview preview)
        {
            string identity = MechBayFitIdentityText(unit, preview, HasPendingLoadoutEdits(unit));
            bool accepted = identity.Length <= 58
                && identity.IndexOf("...", StringComparison.Ordinal) < 0
                && identity.IndexOf("Pilot", StringComparison.OrdinalIgnoreCase) < 0
                && identity.IndexOf("exceeds", StringComparison.OrdinalIgnoreCase) < 0
                && identity.IndexOf("limit", StringComparison.OrdinalIgnoreCase) < 0
                && identity.IndexOf("  S ", StringComparison.Ordinal) >= 0
                && (identity.IndexOf(LoadoutFitOkLabel, StringComparison.Ordinal) >= 0
                    || identity.IndexOf(LoadoutFitReviewPrefix, StringComparison.Ordinal) >= 0);
            return new LoadoutCompactCheck(accepted, "fitIdentity=" + identity);
        }

        private static LoadoutCompactCheck BuildLoadoutEditControlsCompactCheck(CombatLoadoutPreview preview)
        {
            string applyLabel = LoadoutApplyButtonLabel(false, preview, true);
            string readyApplyLabel = LoadoutApplyButtonLabel(true, preview, true);
            string invalidApplyLabel = LoadoutApplyButtonLabel(true, null, true);
            string stockApplyLabel = LoadoutApplyButtonLabel(true, preview, false);
            string resetLabel = LoadoutDraftResetButtonLabel(false);
            string dirtyResetLabel = LoadoutDraftResetButtonLabel(true);
            string cleanStatus = LoadoutEditStatusText(true, false, "");
            string pendingStatus = LoadoutEditStatusText(true, true, "");
            string stockStatus = LoadoutEditStatusText(false, true, "armor plates +1");
            bool readyApplyOk = string.Equals(readyApplyLabel, "Apply", StringComparison.Ordinal)
                || string.Equals(readyApplyLabel, "Invalid", StringComparison.Ordinal);
            bool stockApplyOk = string.Equals(stockApplyLabel, "Stock", StringComparison.Ordinal)
                || string.Equals(stockApplyLabel, "Invalid", StringComparison.Ordinal);
            bool accepted = string.Equals(applyLabel, "Done", StringComparison.Ordinal)
                && readyApplyOk
                && string.Equals(invalidApplyLabel, "Invalid", StringComparison.Ordinal)
                && stockApplyOk
                && string.Equals(resetLabel, "Clean", StringComparison.Ordinal)
                && string.Equals(dirtyResetLabel, "Reset", StringComparison.Ordinal)
                && readyApplyLabel.Length <= 7
                && stockApplyLabel.Length <= 7
                && LoadoutApplyButtonWidth <= 66f
                && LoadoutResetButtonWidth <= 64f
                && LoadoutEditStatusReservedWidth <= 146f
                && LoadoutStockShortPrefix.StartsWith("Stock short", StringComparison.Ordinal)
                && LoadoutStockShortPrefix.IndexOf("Inventory", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(LoadoutFitAppliedStatusText, "Fit applied", StringComparison.Ordinal)
                && LoadoutFitAppliedStatusText.IndexOf("demo", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(cleanStatus, LoadoutFitOkLabel, StringComparison.Ordinal)
                && string.Equals(pendingStatus, LoadoutEditPendingLabel, StringComparison.Ordinal)
                && stockStatus.StartsWith("Stock ", StringComparison.Ordinal)
                && stockStatus.IndexOf("Applied", StringComparison.OrdinalIgnoreCase) < 0
                && stockStatus.IndexOf("Pending fit", StringComparison.OrdinalIgnoreCase) < 0
                && stockStatus.Length <= 32;
            return new LoadoutCompactCheck(
                accepted,
                "edit="
                + applyLabel
                + "/"
                + resetLabel
                + "/"
                + readyApplyLabel
                + "/"
                + stockApplyLabel
                + " status="
                + LoadoutFitAppliedStatusText
                + " editState="
                + cleanStatus
                + "/"
                + pendingStatus
                + "/"
                + stockStatus
                + " applyW="
                + LoadoutApplyButtonWidth.ToString(CultureInfo.InvariantCulture)
                + " resetW="
                + LoadoutResetButtonWidth.ToString(CultureInfo.InvariantCulture));
        }

        private static LoadoutCompactCheck BuildLoadoutSelectedResetCompactCheck()
        {
            string selectedResetBaseLabel = LoadoutSelectedResetButtonLabel(false);
            string selectedResetDirtyLabel = LoadoutSelectedResetButtonLabel(true);
            bool accepted = string.Equals(selectedResetBaseLabel, "Base", StringComparison.Ordinal)
                && string.Equals(selectedResetDirtyLabel, "Reset", StringComparison.Ordinal)
                && LoadoutSelectedResetButtonWidth <= 50f;
            return new LoadoutCompactCheck(
                accepted,
                "selectedReset="
                + selectedResetBaseLabel
                + "/"
                + selectedResetDirtyLabel
                + " selectedResetW="
                + LoadoutSelectedResetButtonWidth.ToString(CultureInfo.InvariantCulture));
        }

        private static LoadoutCompactCheck BuildLoadoutTargetControlsCompactCheck()
        {
            string targetPlaceLabel = LoadoutPlaceButtonLabel(true);
            string targetBlockLabel = LoadoutPlaceButtonLabel(false);
            string targetPickLabel = LoadoutTargetPickLabel();
            string fillerArmorLabel = FillerButtonLabel(null, true, 0);
            string fillerSinkLabel = FillerButtonLabel(LoadoutItemCategory.ArmorPlate, true, 1);
            string fillerClearLabel = FillerButtonLabel(LoadoutItemCategory.HeatSink, true, 1);
            string fillerLockLabel = FillerButtonLabel(null, false, 0);
            string fillerStackLabel = FillerButtonLabel(null, true, 2);
            string targetDetail = SyntheticTargetPlacementDetail();
            bool accepted = string.Equals(targetPlaceLabel, "Place", StringComparison.Ordinal)
                && string.Equals(targetBlockLabel, "Block", StringComparison.Ordinal)
                && string.Equals(targetPickLabel, "Pick", StringComparison.Ordinal)
                && string.Equals(fillerArmorLabel, "+Armor", StringComparison.Ordinal)
                && string.Equals(fillerSinkLabel, "+Sink", StringComparison.Ordinal)
                && string.Equals(fillerClearLabel, "Clear", StringComparison.Ordinal)
                && string.Equals(fillerLockLabel, "Lock", StringComparison.Ordinal)
                && string.Equals(fillerStackLabel, "Stk", StringComparison.Ordinal)
                && LoadoutTargetControlOffset <= 148f
                && LoadoutTargetButtonWidth <= 62f
                && LoadoutTargetStatusMinWidth <= 80f
                && targetDetail.StartsWith("T 2,0 OK / +Armor for W1 AC10", StringComparison.Ordinal)
                && targetDetail.IndexOf("Autocannon", StringComparison.OrdinalIgnoreCase) < 0
                && targetDetail.IndexOf("(", StringComparison.Ordinal) < 0
                && targetDetail.Length <= 42;
            return new LoadoutCompactCheck(
                accepted,
                "target="
                + targetPlaceLabel
                + "/"
                + targetBlockLabel
                + "/"
                + targetPickLabel
                + " filler="
                + fillerArmorLabel
                + "/"
                + fillerSinkLabel
                + "/"
                + fillerClearLabel
                + "/"
                + fillerLockLabel
                + "/"
                + fillerStackLabel
                + " targetDetail="
                + targetDetail
                + " targetW="
                + LoadoutTargetButtonWidth.ToString(CultureInfo.InvariantCulture));
        }

        private static string SyntheticTargetPlacementDetail()
        {
            CombatLoadoutPreviewGridCell left = new(
                0,
                0,
                0,
                "synthetic-ac10",
                "Autocannon (AC10)",
                "",
                "");
            CombatLoadoutPreviewGridCell right = new(
                1,
                0,
                0,
                "synthetic-ac10",
                "Autocannon (AC10)",
                "",
                "");
            CombatLoadoutPreviewItem item = new(
                0,
                0,
                0,
                "synthetic-ac10",
                "Autocannon (AC10)",
                "",
                "");
            CombatLoadoutPreview preview = new(
                new LoadoutValidationResult(),
                8,
                4,
                4,
                new[] { left, right },
                new[] { item },
                0f,
                0f);
            return LoadoutTargetPlacementDetailText(null, preview, 0, new Vector2Int(2, 0));
        }

        private static LoadoutCompactCheck BuildLoadoutNudgeCompactCheck()
        {
            string nudgeNorthLabel = LoadoutNudgeButtonLabel(0, -1);
            string nudgeWestLabel = LoadoutNudgeButtonLabel(-1, 0);
            string nudgeEastLabel = LoadoutNudgeButtonLabel(1, 0);
            string nudgeSouthLabel = LoadoutNudgeButtonLabel(0, 1);
            string nudgeReadyLabel = LoadoutNudgeStatusLabel("");
            string nudgeBlockedLabel = LoadoutNudgeStatusLabel("N outside");
            bool accepted = string.Equals(nudgeNorthLabel, "N", StringComparison.Ordinal)
                && string.Equals(nudgeWestLabel, "W", StringComparison.Ordinal)
                && string.Equals(nudgeEastLabel, "E", StringComparison.Ordinal)
                && string.Equals(nudgeSouthLabel, "S", StringComparison.Ordinal)
                && string.Equals(nudgeReadyLabel, "Move OK", StringComparison.Ordinal)
                && nudgeBlockedLabel.StartsWith("Block ", StringComparison.Ordinal)
                && LoadoutNudgeButtonWidth <= 40f
                && LoadoutNudgeEastButtonWidth <= 44f
                && LoadoutNudgeStatusWidth <= 142f;
            return new LoadoutCompactCheck(
                accepted,
                "nudge="
                + nudgeNorthLabel
                + nudgeWestLabel
                + nudgeEastLabel
                + nudgeSouthLabel
                + "/"
                + nudgeReadyLabel
                + " nudgeW="
                + LoadoutNudgeButtonWidth.ToString(CultureInfo.InvariantCulture)
                + "/"
                + LoadoutNudgeEastButtonWidth.ToString(CultureInfo.InvariantCulture));
        }

        private LoadoutCompactCheck BuildLoadoutPlacementHeaderCompactCheck(UnitState unit, CombatLoadoutPreview preview)
        {
            CombatLoadoutPreviewItem selectedItem = LoadoutPreviewItemForWeapon(preview, 0);
            CombatLoadoutPreviewItem baseItem = LoadoutPreviewItemForWeapon(LoadoutBasePreviewFor(unit), 0);
            string header = LoadoutPlacementHeaderText(unit, preview, 0, selectedItem, baseItem);
            bool accepted = header.StartsWith("W1 Base @", StringComparison.Ordinal)
                && header.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) < 0
                && header.IndexOf("Autocannon", StringComparison.OrdinalIgnoreCase) < 0
                && header.IndexOf("Weapon", StringComparison.OrdinalIgnoreCase) < 0
                && header.IndexOf("  D ", StringComparison.Ordinal) < 0
                && header.Length <= 32;
            return new LoadoutCompactCheck(accepted, "placementHeader=" + header);
        }

        private static LoadoutCompactCheck BuildLoadoutBlockLabelCompactCheck(UnitState unit, CombatLoadoutPreview preview)
        {
            string weaponLabel = LoadoutBlockLabel(unit, LoadoutCellForSelectedWeapon(preview, 0));
            string armorLabel = LoadoutBlockLabel(
                null,
                new CombatLoadoutPreviewGridCell(
                    0,
                    0,
                    -1,
                    "synthetic-armor",
                    "Armor Plate",
                    LoadoutItemCategory.ArmorPlate,
                    ""));
            string sinkLabel = LoadoutBlockLabel(
                null,
                new CombatLoadoutPreviewGridCell(
                    0,
                    0,
                    -1,
                    "synthetic-sink",
                    "Heat Sink",
                    LoadoutItemCategory.HeatSink,
                    ""));
            bool accepted = string.Equals(weaponLabel, "1 AC10", StringComparison.Ordinal)
                && weaponLabel.IndexOf("Autocannon", StringComparison.OrdinalIgnoreCase) < 0
                && weaponLabel.IndexOf("(", StringComparison.Ordinal) < 0
                && weaponLabel.Length <= 8
                && string.Equals(armorLabel, "A+", StringComparison.Ordinal)
                && string.Equals(sinkLabel, "C+", StringComparison.Ordinal)
                && armorLabel.IndexOf("ARM", StringComparison.OrdinalIgnoreCase) < 0
                && sinkLabel.IndexOf("SINK", StringComparison.OrdinalIgnoreCase) < 0;
            return new LoadoutCompactCheck(
                accepted,
                "blockLabel="
                + weaponLabel
                + "/"
                + armorLabel
                + "/"
                + sinkLabel);
        }

        private LoadoutCompactCheck BuildLoadoutSelectedSummaryCompactCheck(
            UnitState unit,
            CombatLoadoutPreview preview,
            CombatWeaponDefinition weapon)
        {
            CombatLoadoutPreviewItem selectedItem = LoadoutPreviewItemForWeapon(preview, 0);
            CombatLoadoutPreviewItem baseItem = LoadoutPreviewItemForWeapon(LoadoutBasePreviewFor(unit), 0);
            string selectedSummary = LoadoutSelectedWeaponSummaryText(unit, preview, 0, weapon, selectedItem, baseItem);
            bool accepted = selectedSummary.StartsWith("W1 ", StringComparison.Ordinal)
                && selectedSummary.IndexOf(" Base ", StringComparison.Ordinal) >= 0
                && selectedSummary.IndexOf("AC10", StringComparison.Ordinal) >= 0
                && selectedSummary.IndexOf("  D ", StringComparison.Ordinal) >= 0
                && selectedSummary.IndexOf("  R ", StringComparison.Ordinal) >= 0
                && selectedSummary.IndexOf("  CD ", StringComparison.Ordinal) >= 0
                && selectedSummary.IndexOf("  H ", StringComparison.Ordinal) >= 0
                && selectedSummary.IndexOf("  W ", StringComparison.Ordinal) >= 0
                && selectedSummary.IndexOf("  C ", StringComparison.Ordinal) >= 0
                && selectedSummary.IndexOf("x", StringComparison.Ordinal) >= 0
                && selectedSummary.IndexOf("Autocannon", StringComparison.OrdinalIgnoreCase) < 0
                && selectedSummary.IndexOf("(", StringComparison.Ordinal) < 0
                && selectedSummary.IndexOf("Cells", StringComparison.OrdinalIgnoreCase) < 0
                && selectedSummary.IndexOf("Shape", StringComparison.OrdinalIgnoreCase) < 0
                && selectedSummary.Length <= 104;
            return new LoadoutCompactCheck(
                accepted,
                "selectedSummary=" + selectedSummary);
        }

        private static LoadoutCompactCheck BuildLoadoutWeaponDetailCompactCheck(UnitState unit, CombatLoadoutPreview preview)
        {
            CombatLoadoutPreviewGridCell selectedCell = LoadoutCellForSelectedWeapon(preview, 0);
            string weaponDetail = LoadoutBlockDetailText(
                unit,
                preview,
                selectedCell,
                selectedCell?.X ?? 0,
                selectedCell?.Y ?? 0,
                false,
                default,
                0);
            bool accepted = weaponDetail.StartsWith("1 AC10", StringComparison.Ordinal)
                && weaponDetail.IndexOf(" H", StringComparison.Ordinal) >= 0
                && weaponDetail.IndexOf(" W", StringComparison.Ordinal) >= 0
                && weaponDetail.IndexOf(" D", StringComparison.Ordinal) >= 0
                && weaponDetail.IndexOf(" R", StringComparison.Ordinal) >= 0
                && weaponDetail.IndexOf(" CD", StringComparison.Ordinal) >= 0
                && weaponDetail.IndexOf(" C", StringComparison.Ordinal) >= 0
                && weaponDetail.IndexOf("x", StringComparison.Ordinal) >= 0
                && weaponDetail.IndexOf("Autocannon", StringComparison.OrdinalIgnoreCase) < 0
                && weaponDetail.IndexOf("(", StringComparison.Ordinal) < 0
                && weaponDetail.IndexOf("Cells", StringComparison.OrdinalIgnoreCase) < 0
                && weaponDetail.IndexOf("Shape", StringComparison.OrdinalIgnoreCase) < 0
                && weaponDetail.Length <= 48;
            return new LoadoutCompactCheck(
                accepted,
                "weaponDetail=" + weaponDetail);
        }

        private static LoadoutCompactCheck BuildLoadoutComponentDetailCompactCheck()
        {
            string armorDetail = SyntheticComponentDetail(LoadoutItemCategory.ArmorPlate, "Armor Plate");
            string sinkDetail = SyntheticComponentDetail(LoadoutItemCategory.HeatSink, "Heat Sink");
            bool accepted = armorDetail.IndexOf("A+1", StringComparison.Ordinal) >= 0
                && armorDetail.IndexOf("W0.5", StringComparison.Ordinal) >= 0
                && sinkDetail.IndexOf("C+1.5", StringComparison.Ordinal) >= 0
                && sinkDetail.IndexOf("W1", StringComparison.Ordinal) >= 0
                && armorDetail.IndexOf("Armor", StringComparison.OrdinalIgnoreCase) < 0
                && sinkDetail.IndexOf("Sink", StringComparison.OrdinalIgnoreCase) < 0
                && armorDetail.IndexOf("Hard", StringComparison.OrdinalIgnoreCase) < 0
                && armorDetail.IndexOf("Load ", StringComparison.OrdinalIgnoreCase) < 0
                && armorDetail.IndexOf("Plate", StringComparison.OrdinalIgnoreCase) < 0
                && sinkDetail.IndexOf("Cool", StringComparison.OrdinalIgnoreCase) < 0
                && sinkDetail.IndexOf("Load ", StringComparison.OrdinalIgnoreCase) < 0
                && sinkDetail.IndexOf("Heat ", StringComparison.OrdinalIgnoreCase) < 0
                && armorDetail.Length <= 18
                && sinkDetail.Length <= 18;
            return new LoadoutCompactCheck(
                accepted,
                "componentDetail="
                + armorDetail
                + "/"
                + sinkDetail);
        }

        private static string SyntheticComponentDetail(string category, string displayName)
        {
            CombatLoadoutPreviewGridCell cell = new(
                0,
                0,
                -1,
                "synthetic-" + category,
                displayName,
                category,
                "");
            CombatLoadoutPreview preview = new(
                new LoadoutValidationResult(),
                1,
                1,
                1,
                new[] { cell },
                Array.Empty<CombatLoadoutPreviewItem>(),
                0f,
                0f);
            return LoadoutBlockDetailText(null, preview, cell, 0, 0, false, default, -1);
        }

        private static LoadoutCompactCheck BuildLoadoutOpenSlotDetailCompactCheck()
        {
            string emptyDetail = SyntheticOpenSlotDetail(false);
            string selectedDetail = SyntheticOpenSlotDetail(true);
            bool accepted = string.Equals(emptyDetail, "Slot 2,3 open", StringComparison.Ordinal)
                && string.Equals(selectedDetail, "Slot 2,3 open", StringComparison.Ordinal)
                && emptyDetail.IndexOf("Empty", StringComparison.OrdinalIgnoreCase) < 0
                && selectedDetail.IndexOf("Selected", StringComparison.OrdinalIgnoreCase) < 0
                && emptyDetail.Length <= 16
                && selectedDetail.Length <= 16;
            return new LoadoutCompactCheck(
                accepted,
                "openSlot="
                + emptyDetail
                + "/"
                + selectedDetail);
        }

        private static string SyntheticOpenSlotDetail(bool selected)
        {
            CombatLoadoutPreview preview = new(
                new LoadoutValidationResult(),
                16,
                4,
                4,
                Array.Empty<CombatLoadoutPreviewGridCell>(),
                Array.Empty<CombatLoadoutPreviewItem>(),
                0f,
                0f);
            return selected
                ? LoadoutBlockDetailText(null, preview, null, -1, -1, true, new Vector2Int(2, 3), -1)
                : LoadoutBlockDetailText(null, preview, null, 2, 3, false, default, -1);
        }

        private DebriefSummaryAssertionResult BuildDebriefSummaryAssertion()
        {
            MissionResultSummary summary = mission?.ResultSummary;
            if (summary == null)
            {
                return DebriefSummaryAssertionResult.Blocked("No result summary");
            }

            int destroyedLabelCount = summary.destroyedEnemyUnitLabels?.Length ?? 0;
            int destroyedStructureLabelCount = summary.destroyedStructureLabels?.Length ?? 0;
            int damagedLabelCount = summary.damagedPlayerUnitLabels?.Length ?? 0;
            int completedLabelCount = summary.completedVisibleObjectiveTitles?.Length ?? 0;
            bool objectiveTotalsOk = summary.visibleObjectives > 0
                && summary.completedVisibleObjectives >= 0
                && summary.completedVisibleObjectives <= summary.visibleObjectives
                && completedLabelCount == summary.completedVisibleObjectives;
            bool combatTotalsOk = destroyedLabelCount == summary.destroyedEnemyUnits
                && destroyedStructureLabelCount == summary.destroyedStructures
                && damagedLabelCount == summary.damagedPlayerUnits
                && summary.salvageClaimCount == summary.destroyedEnemyUnits;
            bool economyTotalsOk = summary.visibleRewardResourcePoints >= summary.completedRewardResourcePoints
                && summary.repairCostResourcePoints >= 0
                && summary.netResourcePoints == summary.completedRewardResourcePoints - summary.repairCostResourcePoints;
            string outcomeLine = MissionResultOutcomeLine(summary);
            bool outcomeLineOk = outcomeLine.StartsWith("Outcome Obj ", StringComparison.Ordinal)
                && outcomeLine.IndexOf(" Kills ", StringComparison.Ordinal) >= 0
                && outcomeLine.IndexOf(" Dmg ", StringComparison.Ordinal) >= 0
                && outcomeLine.IndexOf(" Net ", StringComparison.Ordinal) >= 0
                && outcomeLine.IndexOf(" Salvage ", StringComparison.Ordinal) >= 0
                && outcomeLine.Length <= 72;
            string combatLine = MissionResultCombatLine(summary);
            bool combatLineOk = combatLine.IndexOf("Combat: Kills ", StringComparison.OrdinalIgnoreCase) >= 0
                && combatLine.IndexOf("Damage ", StringComparison.OrdinalIgnoreCase) >= 0;
            bool overflowOk = string.Equals(
                SummaryItemsText(new[] { "A", "B", "C" }, 2),
                "A, B +1",
                StringComparison.Ordinal);
            bool endRunLabelOk = string.Equals(EndRunButtonLabel, "End Run", StringComparison.Ordinal)
                && EndRunButtonLabel.IndexOf("Demo", StringComparison.OrdinalIgnoreCase) < 0
                && EndRunButtonLabel.Length <= 8;
            bool debriefCopyOk = string.Equals(DebriefPayoutLabel, "Payout", StringComparison.Ordinal)
                && DebriefPayoutLabel.IndexOf("Receipt", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(DebriefSalvageLabel, "Salvage", StringComparison.Ordinal)
                && DebriefSalvageLabel.IndexOf("claims", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(DebriefBountyLabel, "Bounty", StringComparison.Ordinal)
                && DebriefBountyLabel.IndexOf("Total", StringComparison.OrdinalIgnoreCase) < 0
                && DebriefNextStepText.IndexOf("choose next contract", StringComparison.OrdinalIgnoreCase) >= 0
                && DebriefNextStepText.IndexOf("save", StringComparison.OrdinalIgnoreCase) < 0
                && DebriefNextStepText.IndexOf("launch again", StringComparison.OrdinalIgnoreCase) < 0;
            string resultFx = MissionResultCueSummary();
            bool resultFxOk = resultFx.IndexOf("ResultCue=complete+failed", StringComparison.Ordinal) >= 0;
            string debriefDamageFx = DebriefDamageCueSummary();
            string syntheticDamageLine = MissionResultCombatLine(new MissionResultSummary
            {
                destroyedEnemyUnitLabels = new[] { "enemy-1 LRMC", "enemy-2 Harasser", "enemy-3 UrbanMech" },
                damagedPlayerUnitLabels = new[] { "W CP X", "B LA !30", "B LG 72" }
            });
            bool debriefDamageFxOk = debriefDamageFx.IndexOf("DebriefDamage=unit+section+overflow", StringComparison.Ordinal) >= 0
                && syntheticDamageLine.IndexOf("CP X", StringComparison.Ordinal) >= 0
                && syntheticDamageLine.IndexOf("LA !30", StringComparison.Ordinal) >= 0
                && syntheticDamageLine.IndexOf("+1", StringComparison.Ordinal) >= 0;
            string debriefTargetFx = DebriefTargetCueSummary();
            string syntheticTargetLine = MissionResultSalvageTargetLine(new MissionResultSummary
            {
                salvageClaimCount = 2,
                visibleRewardResourcePoints = 3000,
                destroyedStructures = 3,
                destroyedStructureLabels = new[] { "Hangar", "Generator", "Tower" }
            });
            bool debriefTargetFxOk = debriefTargetFx.IndexOf("DebriefTargets=count+labels+overflow", StringComparison.Ordinal) >= 0
                && syntheticTargetLine.IndexOf("Targets Hangar, Generator +1", StringComparison.Ordinal) >= 0
                && syntheticTargetLine.IndexOf("Bounty 3,000", StringComparison.Ordinal) >= 0;
            string debriefTopMode = TopStatusModeText(DemoFlowScreen.Debrief);
            string debriefFunds = TopStatusFundsText(null);
            string debriefFundsRow = MissionResultFundsLine(new MissionResultSummary
            {
                completedRewardResourcePoints = 3000,
                repairCostResourcePoints = 400,
                netResourcePoints = 2600
            });
            bool flowStatusCopyOk = SaveSlotNeedsReviewText.IndexOf("slot", StringComparison.OrdinalIgnoreCase) >= 0
                && SaveSlotNeedsReviewText.IndexOf("review", StringComparison.OrdinalIgnoreCase) >= 0
                && NoSaveSlotText.IndexOf("default", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(ContractsOpenStatusText, "Contracts open", StringComparison.Ordinal)
                && string.Equals(ContractsHeaderText, "Available Contracts", StringComparison.Ordinal)
                && string.Equals(ContractsCardTitleText, "Airfield Contract", StringComparison.Ordinal)
                && string.Equals(ContractsMapReadyText, "Mission map ready", StringComparison.Ordinal)
                && string.Equals(ContractsLaunchButtonLabel, "Launch", StringComparison.Ordinal)
                && string.Equals(ContractsReturnBattleButtonLabel, "Back to Battle", StringComparison.Ordinal)
                && string.Equals(ContractsReturnDebriefButtonLabel, "Back to Debrief", StringComparison.Ordinal)
                && ContractsLaunchButtonLabel.IndexOf("Contract", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(SystemRestartButtonLabel, "Restart Mission", StringComparison.Ordinal)
                && SystemRestartButtonLabel.IndexOf("Contract", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(SystemBackButtonLabel, "Back", StringComparison.Ordinal)
                && AfterActionMechLabStatusText.StartsWith("After Action", StringComparison.Ordinal)
                && AfterActionContractsStatusText.StartsWith("After Action", StringComparison.Ordinal)
                && string.Equals(debriefTopMode, "MODE Debrief", StringComparison.Ordinal)
                && debriefTopMode.IndexOf("FLOW", StringComparison.OrdinalIgnoreCase) < 0
                && string.Equals(debriefFunds, "Funds --", StringComparison.Ordinal)
                && debriefFunds.IndexOf("TOKEN", StringComparison.OrdinalIgnoreCase) < 0
                && debriefFundsRow.StartsWith(DebriefPayoutLabel + " +3,000", StringComparison.Ordinal)
                && debriefFundsRow.IndexOf("Token", StringComparison.OrdinalIgnoreCase) < 0
                && debriefFundsRow.IndexOf("Repair -400", StringComparison.Ordinal) >= 0
                && debriefFundsRow.IndexOf("Net +2,600", StringComparison.Ordinal) >= 0;

            string result = "objectives="
                + summary.completedVisibleObjectives.ToString(CultureInfo.InvariantCulture)
                + "/"
                + summary.visibleObjectives.ToString(CultureInfo.InvariantCulture)
                + " kills="
                + summary.destroyedEnemyUnits.ToString(CultureInfo.InvariantCulture)
                + " damaged="
                + summary.damagedPlayerUnits.ToString(CultureInfo.InvariantCulture)
                + " targets="
                + summary.destroyedStructures.ToString(CultureInfo.InvariantCulture)
                + " net="
                + summary.netResourcePoints.ToString(CultureInfo.InvariantCulture)
                + " end="
                + EndRunButtonLabel
                + " payout="
                + DebriefPayoutLabel
                + " salvage="
                + DebriefSalvageLabel
                + " bounty="
                + DebriefBountyLabel
                + " resultFx="
                + resultFx
                + " debriefDamageFx="
                + debriefDamageFx
                + " debriefTargetFx="
                + debriefTargetFx
                + " flow="
                + ContractsOpenStatusText
                + "/"
                + ContractsLaunchButtonLabel
                + "/"
                + SystemRestartButtonLabel
                + "/"
                + AfterActionMechLabStatusText
                + " top="
                + debriefTopMode
                + " funds="
                + debriefFunds
                + " fundsRow="
                + debriefFundsRow
                + " outcomeLine="
                + outcomeLine
                + " combatLine="
                + combatLine
                + " targetLine="
                + syntheticTargetLine;

            return new DebriefSummaryAssertionResult
            {
                Accepted = objectiveTotalsOk
                    && combatTotalsOk
                    && economyTotalsOk
                    && outcomeLineOk
                    && combatLineOk
                    && overflowOk
                    && endRunLabelOk
                    && debriefCopyOk
                    && resultFxOk
                    && debriefDamageFxOk
                    && debriefTargetFxOk
                    && flowStatusCopyOk,
                Summary = result
            };
        }

        private DebriefVisibleAssertionResult BuildDebriefVisibleAssertion()
        {
            MissionResultSummary summary = mission?.ResultSummary;
            bool resultReady = mission != null && mission.Result != MissionResultState.InProgress;
            bool summaryReady = summary != null && summary.visibleObjectives > 0;
            bool flowOk = demoFlowScreen == DemoFlowScreen.Debrief;
            bool panelOk = showMissionResultPanel;
            bool overlayOk = !showLoadoutPanel
                && !showSystemPanel
                && !showMissionListPanel
                && !showStartupContinuePanel;
            bool pausedOk = isPaused;
            string result = "result="
                + (mission == null ? "none" : mission.Result.ToString())
                + " panel="
                + showMissionResultPanel
                + " flow="
                + DemoFlowScreenName(demoFlowScreen)
                + " paused="
                + isPaused
                + " overlays="
                + (overlayOk ? "clear" : "blocked")
                + " objectives="
                + (summary?.completedVisibleObjectives ?? 0).ToString(CultureInfo.InvariantCulture)
                + "/"
                + (summary?.visibleObjectives ?? 0).ToString(CultureInfo.InvariantCulture)
                + " status="
                + statusText;

            return new DebriefVisibleAssertionResult
            {
                Accepted = resultReady && summaryReady && flowOk && panelOk && overlayOk && pausedOk,
                Summary = result
            };
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

        private int RunStartupCapturePreset(string[] args, int index)
        {
            if (index + 1 >= args.Length)
            {
                Debug.LogWarning("MC2 capture preset blocked: missing preset after -mc2CapturePreset.");
                return index;
            }

            RunStartupCapturePreset(args[index + 1]);
            return index + 1;
        }

        private void RunStartupCapturePreset(string preset)
        {
            string normalizedPreset = NormalizeCapturePreset(preset);
            lastCapturePreset = normalizedPreset;
            Debug.Log("MC2 capture preset: " + normalizedPreset);

            switch (normalizedPreset)
            {
                case CapturePresetSpawn:
                    AdvanceStartupSimulation(0.25f);
                    break;
                case CapturePresetAirfield:
                    RunStartupAirfieldCapturePrelude();
                    break;
                case CapturePresetHangarContact:
                    RunStartupHangarContactCapturePrelude();
                    break;
                case CapturePresetNorthPatrol:
                    RunStartupNorthPatrolCapturePrelude();
                    break;
                case CapturePresetDamageDemo:
                    RunStartupDamageDemoCapturePrelude();
                    break;
                default:
                    startupSmokeFailed = true;
                    Debug.LogWarning("MC2 capture preset blocked: unknown preset '" + preset + "'.");
                    break;
            }
        }

        private static string NormalizeCapturePreset(string preset)
        {
            if (string.IsNullOrWhiteSpace(preset))
            {
                return CapturePresetSpawn;
            }

            return preset.Trim().ToLowerInvariant().Replace("_", "-");
        }

        private void RunStartupAirfieldCapturePrelude()
        {
            RunStartupCommanderCommand("squad move 3136 -789");
            AdvanceStartupSimulation(12f);
        }

        private void RunStartupHangarContactCapturePrelude()
        {
            RunStartupAirfieldCapturePrelude();
            RunStartupCommanderCommand("squad attack structure structure-1-0");
            AdvanceStartupSimulation(8f);
        }

        private void RunStartupNorthPatrolCapturePrelude()
        {
            RunStartupHangarContactCapturePrelude();
            RunStartupCommanderCommand("squad move -2240 1600");
            AdvanceStartupSimulation(34f);
        }

        private void RunStartupDamageDemoCapturePrelude()
        {
            RunStartupHangarContactCapturePrelude();
            AdvanceStartupSimulation(18f);
            ForceStartupDamageDemoSections();
            cameraZoomScale = Mathf.Min(cameraZoomScale, 0.82f);
            ApplyCameraProjection();
        }

        private void ForceStartupDamageDemoSections()
        {
            if (mission == null)
            {
                return;
            }

            UnitState first = null;
            UnitState second = null;
            UnitState third = null;
            foreach (UnitState unit in mission.PlayerUnits())
            {
                if (unit == null || !unit.IsActive)
                {
                    continue;
                }

                first ??= unit;
                if (first != unit)
                {
                    second ??= unit;
                }

                if (first != unit && second != unit)
                {
                    third ??= unit;
                    break;
                }
            }

            if (first == null)
            {
                Debug.LogWarning("MC2 damage demo capture blocked: no active player units.");
                return;
            }

            second ??= first;
            third ??= first;
            float leftArmDamage = ApplyStartupDamageDemoSection(first, "Left Arm");
            float legDamage = ApplyStartupDamageDemoSection(second, "Legs");
            float cockpitDamage = ApplyStartupDamageDemoSection(third, "Cockpit");
            AddCombatLogLine("Capture damage demo: " + first.Id + " left arm, " + second.Id + " legs, " + third.Id + " cockpit");
            Debug.Log(
                "MC2 damage demo forced sections: leftArm="
                + leftArmDamage.ToString("0.##", CultureInfo.InvariantCulture)
                + " legs="
                + legDamage.ToString("0.##", CultureInfo.InvariantCulture)
                + " cockpit="
                + cockpitDamage.ToString("0.##", CultureInfo.InvariantCulture));
        }

        private static float ApplyStartupDamageDemoSection(UnitState unit, string sectionName)
        {
            if (unit?.Sections == null)
            {
                return 0f;
            }

            for (int index = 0; index < unit.Sections.Length; index++)
            {
                DamageSection section = unit.Sections[index];
                if (section != null && string.Equals(section.Name, sectionName, StringComparison.OrdinalIgnoreCase))
                {
                    return unit.ApplyDirectSectionDamage(section.Name, section.HitPoints);
                }
            }

            return 0f;
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
                CaptureMissionTickEvents();
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

        private void WriteCaptureSidecar(string sidecarPath, string screenshotPath)
        {
            if (string.IsNullOrWhiteSpace(sidecarPath))
            {
                return;
            }

            string folder = Path.GetDirectoryName(sidecarPath);
            if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            VisualCaptureSidecar sidecar = BuildCaptureSidecar(screenshotPath);
            File.WriteAllText(sidecarPath, JsonUtility.ToJson(sidecar, true));
            Debug.Log("MC2 capture sidecar written: " + sidecarPath);
        }

        private VisualCaptureSidecar BuildCaptureSidecar(string screenshotPath)
        {
            CommanderObservation observation = observationPort?.Observe();
            return new VisualCaptureSidecar
            {
                preset = lastCapturePreset,
                screenshot = screenshotPath,
                missionId = mission?.Contract?.mission?.id ?? "",
                result = mission == null ? "Unavailable" : mission.Result.ToString(),
                status = statusText ?? "",
                flowScreen = DemoFlowScreenName(demoFlowScreen),
                missionTimeSeconds = mission?.MissionTimeSeconds ?? 0f,
                screenWidth = Screen.width,
                screenHeight = Screen.height,
                playerUnitCount = observation?.playerUnits?.Length ?? CountCapturePlayerUnits(),
                activeHostileCount = observation?.activeHostiles?.Length ?? CountActiveHostiles(),
                visibleHostileCount = VisibleHostileLabels().Length,
                targetableStructureCount = observation?.targetableStructures?.Length ?? 0,
                currentObjectiveCount = observation?.currentObjectives?.Length ?? 0,
                activeHostiles = UnitLabels(observation?.activeHostiles),
                visibleHostiles = VisibleHostileLabels(),
                currentObjectives = ObjectiveLabels(observation?.currentObjectives),
                camera = BuildCaptureCameraState(),
                referenceAssets = new VisualCaptureReferenceState
                {
                    terrain = lastTerrainTextureSummary,
                    structures = lastReferenceStructureSummary,
                    props = lastReferencePropSummary
                }
            };
        }

        private VisualCaptureCameraState BuildCaptureCameraState()
        {
            Transform cameraTransform = mainCamera == null ? null : mainCamera.transform;
            return new VisualCaptureCameraState
            {
                orthographicSize = mainCamera == null ? 0f : mainCamera.orthographicSize,
                zoomScale = cameraZoomScale,
                pitch = cameraPitch,
                yaw = cameraYaw,
                followOffset = CaptureVector3.From(cameraFollowWorldOffset),
                position = cameraTransform == null ? new CaptureVector3() : CaptureVector3.From(cameraTransform.position),
                rotationEuler = cameraTransform == null ? new CaptureVector3() : CaptureVector3.From(cameraTransform.rotation.eulerAngles)
            };
        }

        private int CountCapturePlayerUnits()
        {
            int count = 0;
            if (mission == null)
            {
                return count;
            }

            foreach (UnitState unit in mission.PlayerUnits())
            {
                if (unit != null)
                {
                    count++;
                }
            }

            return count;
        }

        private int CountActiveHostiles()
        {
            int count = 0;
            if (mission == null)
            {
                return count;
            }

            foreach (UnitState unit in mission.Units)
            {
                if (unit != null && !unit.IsPlayerUnit && unit.IsActive && !unit.IsDestroyed)
                {
                    count++;
                }
            }

            return count;
        }

        private string[] VisibleHostileLabels()
        {
            List<string> labels = new();
            if (mission == null || mainCamera == null)
            {
                return labels.ToArray();
            }

            foreach (UnitState unit in mission.Units)
            {
                if (unit == null || unit.IsPlayerUnit || !unit.IsActive || unit.IsDestroyed)
                {
                    continue;
                }

                Vector3 position = DemoUnitView.MissionToWorld(unit.MissionPosition);
                position.y = DemoTerrainView.HeightAt(unit.MissionPosition) + 1f;
                Vector3 viewport = mainCamera.WorldToViewportPoint(position);
                if (viewport.z > 0f && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f)
                {
                    labels.Add(UnitLabel(unit.Id, unit.UnitType));
                }
            }

            return labels.ToArray();
        }

        private static string[] UnitLabels(CommanderUnitObservation[] units)
        {
            if (units == null || units.Length == 0)
            {
                return Array.Empty<string>();
            }

            string[] labels = new string[units.Length];
            for (int index = 0; index < units.Length; index++)
            {
                CommanderUnitObservation unit = units[index];
                labels[index] = unit == null ? "" : UnitLabel(unit.id, unit.type);
            }

            return labels;
        }

        private static string[] ObjectiveLabels(CommanderObjectiveObservation[] objectives)
        {
            if (objectives == null || objectives.Length == 0)
            {
                return Array.Empty<string>();
            }

            string[] labels = new string[objectives.Length];
            for (int index = 0; index < objectives.Length; index++)
            {
                CommanderObjectiveObservation objective = objectives[index];
                labels[index] = objective == null
                    ? ""
                    : objective.index.ToString(CultureInfo.InvariantCulture) + " " + objective.title;
            }

            return labels;
        }

        private static string UnitLabel(string id, string type)
        {
            return (string.IsNullOrWhiteSpace(id) ? "<unit>" : id)
                + " "
                + (string.IsNullOrWhiteSpace(type) ? "<unknown>" : type);
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
                string line = BuildCombatLogLine(combatEvent);
                lastCombatEventTimeSeconds = mission?.MissionTimeSeconds ?? Time.time;
                AddCombatLogLine(line);
                SpawnCombatEffect(combatEvent);
                Debug.Log("MC2 combat: " + line);
            }
        }

        private static string BuildCombatLogLine(CombatEvent combatEvent)
        {
            string line = WeaponStatusLabel(combatEvent.WeaponType)
                + " "
                + TruncateText(combatEvent.AttackerId, 16)
                + " > "
                + TruncateText(combatEvent.TargetId, 16)
                + " "
                + CombatSectionLabel(combatEvent.SectionName)
                + " D"
                + Mathf.RoundToInt(combatEvent.Damage).ToString(CultureInfo.InvariantCulture);
            if (combatEvent.MitigatedDamage > 0.4f)
            {
                line += " B" + Mathf.RoundToInt(combatEvent.MitigatedDamage).ToString(CultureInfo.InvariantCulture);
            }

            if (combatEvent.DestroyedTarget)
            {
                line += " KILL";
            }

            return line;
        }

        private void SpawnCombatEffect(CombatEvent combatEvent)
        {
            if (combatEvent.Damage <= 0f
                || !TryGetCombatMuzzlePoint(combatEvent.AttackerId, combatEvent.WeaponType, out Vector3 attackerPoint)
                || !TryGetCombatHitPoint(combatEvent.TargetId, combatEvent.SectionName, out Vector3 targetPoint))
            {
                return;
            }

            Color weaponColor = combatEvent.DestroyedTarget
                ? new Color(1f, 0.42f, 0.08f, 0.85f)
                : WeaponColor(combatEvent.WeaponType);
            float severityScale = HitSeverityScale(combatEvent);
            CreateWeaponTrace(attackerPoint, targetPoint, combatEvent, weaponColor);
            CreateImpact(targetPoint, weaponColor, combatEvent.DestroyedTarget, ImpactScale(combatEvent.WeaponType) * severityScale);
            CreateWeaponImpactCue(attackerPoint, targetPoint, combatEvent, weaponColor, severityScale);
            CreateWeaponImpactAftermath(attackerPoint, targetPoint, combatEvent, weaponColor, severityScale);
            CreateHitDirectionCue(attackerPoint, targetPoint, severityScale);
            CreateSectionHitCue(attackerPoint, targetPoint, combatEvent.SectionName, combatEvent.DestroyedTarget, severityScale);
            CreateKillShockCue(targetPoint, combatEvent.DestroyedTarget, severityScale);
            CreateArmorMitigationCue(attackerPoint, targetPoint, combatEvent);
            PulseTarget(combatEvent.TargetId, new Color(1f, 0.9f, 0.52f), Mathf.Clamp((combatEvent.DestroyedTarget ? 0.28f : 0.18f) * severityScale, 0.14f, 0.42f));
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

        private bool TryGetCombatMuzzlePoint(string id, string weaponType, out Vector3 point)
        {
            if (unitViews.TryGetValue(id, out DemoUnitView unitView) && unitView != null && unitView.Unit != null)
            {
                point = unitView.transform.TransformPoint(CombatWeaponMuzzleLocalPoint(weaponType));
                return true;
            }

            return TryGetCombatPoint(id, out point);
        }

        private bool TryGetCombatHitPoint(string id, string sectionName, out Vector3 point)
        {
            if (unitViews.TryGetValue(id, out DemoUnitView unitView) && unitView != null && unitView.Unit != null)
            {
                point = unitView.transform.TransformPoint(CombatSectionLocalPoint(sectionName));
                return true;
            }

            return TryGetCombatPoint(id, out point);
        }

        private static Vector3 CombatSectionLocalPoint(string sectionName)
        {
            if (IsCombatSectionName(sectionName, "Cockpit") || IsCombatSectionName(sectionName, "Turret"))
            {
                return new Vector3(0f, 0.76f, 0.32f);
            }

            if (IsCombatSectionName(sectionName, "Left Arm") || IsCombatSectionName(sectionName, "Left"))
            {
                return new Vector3(-0.58f, 0.16f, 0.08f);
            }

            if (IsCombatSectionName(sectionName, "Right Arm") || IsCombatSectionName(sectionName, "Right"))
            {
                return new Vector3(0.58f, 0.16f, 0.08f);
            }

            if (IsCombatSectionName(sectionName, "Legs"))
            {
                return new Vector3(0f, -0.28f, 0.10f);
            }

            return new Vector3(0f, 0.18f, 0.20f);
        }

        private static Vector3 CombatWeaponMuzzleLocalPoint(string weaponType)
        {
            if (ContainsWeaponType(weaponType, "Missile"))
            {
                return new Vector3(0f, 0.72f, -0.18f);
            }

            if (ContainsWeaponType(weaponType, "Ballistic"))
            {
                return new Vector3(0.58f, 0.18f, 0.10f);
            }

            if (ContainsWeaponType(weaponType, "Energy"))
            {
                return new Vector3(-0.58f, 0.20f, 0.12f);
            }

            return new Vector3(0f, 0.46f, 0.20f);
        }

        private static bool IsCombatSectionName(string sectionName, string expected)
        {
            return string.Equals(sectionName, expected, StringComparison.OrdinalIgnoreCase);
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

        private static string WeaponStatusLabel(string weaponType)
        {
            if (ContainsWeaponType(weaponType, "Energy"))
            {
                return "ENG burn";
            }

            if (ContainsWeaponType(weaponType, "Missile"))
            {
                return "MSL blast";
            }

            if (ContainsWeaponType(weaponType, "Ballistic"))
            {
                return "BAL punch";
            }

            return "WPN hit";
        }

        private static string CombatSectionLabel(string sectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                return "Section";
            }

            if (sectionName.IndexOf("Cockpit", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Cockpit";
            }

            if (sectionName.IndexOf("Arm", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Arms";
            }

            if (sectionName.IndexOf("Leg", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Legs";
            }

            if (sectionName.IndexOf("Structure", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Structure";
            }

            if (sectionName.IndexOf("Torso", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Torso";
            }

            return TruncateText(sectionName, 9);
        }

        private static string WeaponFxCueSummary()
        {
            return "Energy=beam+pillar+muzzle+flash+scorch Missile=arc+blast+salvo-spread+crater Ballistic=tracer+sparks+muzzle+punch+debris";
        }

        private static string WeaponStatusCueSummary()
        {
            string energy = BuildCombatLogLine(new CombatEvent("Alpha", "Target", "Torso", 24f, false, "Energy", mitigatedDamage: 4f));
            string missile = BuildCombatLogLine(new CombatEvent("Alpha", "Target", "Legs", 32f, true, "Missile"));
            string ballistic = BuildCombatLogLine(new CombatEvent("Alpha", "Target", "Arms", 18f, false, "Ballistic"));
            return "WeaponStatus=ENG-burn+MSL-blast+BAL-punch+D+B+KILL samples="
                + energy
                + "|"
                + missile
                + "|"
                + ballistic;
        }

        private static string HitSeverityCueSummary()
        {
            return "HitSeverity=damage+kill+shock";
        }

        private static string HitDirectionCueSummary()
        {
            return "HitDirection=inbound+slash";
        }

        private static string WeaponMuzzlePointCueSummary()
        {
            return "WeaponMuzzlePoint=energy+missile+ballistic";
        }

        private static string ArmorMitigationCueSummary()
        {
            return "ArmorMitigation=glint+spark";
        }

        private static string SectionHitPointCueSummary()
        {
            return "SectionHitPoint=cockpit+arms+legs+torso";
        }

        private static string SectionHitCueSummary()
        {
            return "SectionHitCue=cockpit+arms+legs+torso";
        }

        private static string SectionStatusCueSummary()
        {
            return "SectionStatus=bar+short-label+critical+destroyed";
        }

        private static bool SectionStatusLabelSamplesOk()
        {
            DamageSection healthy = new("Legs", 10f);
            DamageSection critical = new("Left Arm", 10f);
            DamageSection destroyed = new("Cockpit", 10f, true);
            critical.ApplyDamage(7f);
            destroyed.ApplyDamage(10f);
            return string.Equals(SectionStatusLabel(healthy), "LG 100", StringComparison.Ordinal)
                && string.Equals(SectionStatusLabel(critical), "LA !30", StringComparison.Ordinal)
                && string.Equals(SectionStatusLabel(destroyed), "CP X", StringComparison.Ordinal);
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

        private static float HitSeverityScale(CombatEvent combatEvent)
        {
            float incomingDamage = Mathf.Max(0f, combatEvent.Damage + combatEvent.MitigatedDamage);
            float scale = Mathf.Lerp(0.82f, 1.28f, Mathf.InverseLerp(2f, 18f, incomingDamage));
            if (combatEvent.DestroyedTarget)
            {
                scale += 0.30f;
            }

            return Mathf.Clamp(scale, 0.78f, 1.68f);
        }

        private void CreateWeaponImpactCue(Vector3 from, Vector3 to, CombatEvent combatEvent, Color color, float severityScale)
        {
            if (ContainsWeaponType(combatEvent.WeaponType, "Missile"))
            {
                CreateMissileImpactCue(to, color, combatEvent.DestroyedTarget, severityScale);
                return;
            }

            if (ContainsWeaponType(combatEvent.WeaponType, "Ballistic"))
            {
                CreateBallisticImpactCue(from, to, color, severityScale);
                return;
            }

            if (ContainsWeaponType(combatEvent.WeaponType, "Energy"))
            {
                CreateEnergyImpactCue(to, color, severityScale);
            }
        }

        private void CreateEnergyImpactCue(Vector3 targetPoint, Color color, float severityScale)
        {
            float flashScale = Mathf.Clamp(severityScale, 0.82f, 1.50f);
            Color pillar = new(color.r, color.g, color.b, 0.70f);
            CreateBeam(targetPoint + Vector3.up * 0.08f, targetPoint + Vector3.up * (1.20f * flashScale), pillar, 0.16f, 0.046f * flashScale);
            CreateBeam(
                targetPoint + Vector3.up * 0.10f,
                targetPoint + Vector3.up * (0.74f * flashScale),
                new Color(0.86f, 1f, 1f, 0.94f),
                0.10f,
                0.018f * flashScale);
            CreateImpactDisc(
                "Energy Flash Ring",
                targetPoint + Vector3.up * 0.065f,
                new Color(0.72f, 1f, 1f, 0.46f),
                0.14f,
                0.14f * flashScale,
                0.58f * flashScale,
                0.012f);
            CreateImpactDisc(
                "Energy Ion Ring",
                targetPoint + Vector3.up * 0.06f,
                new Color(0.35f, 0.95f, 1f, 0.38f),
                0.22f,
                0.32f * flashScale,
                0.88f * flashScale,
                0.025f);
        }

        private void CreateMissileImpactCue(Vector3 targetPoint, Color color, bool destroyedTarget, float severityScale)
        {
            float scale = (destroyedTarget ? 1.35f : 1f) * severityScale;
            CreateImpactDisc(
                "Missile Blast Ring",
                targetPoint + Vector3.up * 0.04f,
                new Color(1f, 0.46f, 0.08f, 0.42f),
                0.42f,
                0.30f * scale,
                1.25f * scale,
                0.035f);
            CreateBeam(
                targetPoint + Vector3.up * 0.10f,
                targetPoint + Vector3.up * (1.05f * scale),
                new Color(0.16f, 0.14f, 0.12f, 0.38f),
                0.72f,
                0.085f * scale);
            CreateImpact(targetPoint + Vector3.up * 0.18f, color, destroyedTarget, 0.62f * scale);
        }

        private void CreateBallisticImpactCue(Vector3 from, Vector3 to, Color color, float severityScale)
        {
            Vector3 direction = to - from;
            Vector3 forward = SafeDirection(direction);
            Vector3 side = LateralVector(direction);
            Vector3 sparkBase = to + Vector3.up * 0.08f;
            CreateBeam(sparkBase, sparkBase + side * (0.62f * severityScale) + Vector3.up * (0.12f * severityScale), new Color(1f, 0.88f, 0.36f, 0.70f), 0.075f, 0.012f * severityScale);
            CreateBeam(sparkBase, sparkBase - side * (0.58f * severityScale) + Vector3.up * (0.10f * severityScale), new Color(1f, 0.64f, 0.18f, 0.62f), 0.070f, 0.010f * severityScale);
            CreateBeam(sparkBase, sparkBase + Vector3.up * (0.52f * severityScale), color, 0.065f, 0.010f * severityScale);

            float punchScale = Mathf.Clamp(severityScale, 0.78f, 1.55f);
            Vector3 punchCenter = to - forward * (0.08f * punchScale) + Vector3.up * 0.06f;
            CreateImpactDisc(
                "Ballistic Punch Ring",
                punchCenter,
                new Color(1f, 0.78f, 0.22f, 0.38f),
                0.16f,
                0.10f * punchScale,
                0.36f * punchScale,
                0.010f);
            CreateBeam(
                punchCenter - side * (0.20f * punchScale),
                punchCenter + side * (0.28f * punchScale) + Vector3.up * (0.04f * punchScale),
                new Color(1f, 0.95f, 0.48f, 0.62f),
                0.055f,
                0.010f * punchScale);
            CreateBeam(
                punchCenter - forward * (0.18f * punchScale),
                punchCenter + forward * (0.12f * punchScale) + Vector3.up * (0.05f * punchScale),
                new Color(1f, 0.56f, 0.18f, 0.56f),
                0.050f,
                0.012f * punchScale);
        }

        private void CreateWeaponImpactAftermath(Vector3 from, Vector3 to, CombatEvent combatEvent, Color color, float severityScale)
        {
            if (ContainsWeaponType(combatEvent.WeaponType, "Missile"))
            {
                CreateMissileImpactAftermath(to, severityScale, combatEvent.DestroyedTarget);
                return;
            }

            if (ContainsWeaponType(combatEvent.WeaponType, "Ballistic"))
            {
                CreateBallisticImpactAftermath(from, to, severityScale);
                return;
            }

            if (ContainsWeaponType(combatEvent.WeaponType, "Energy"))
            {
                CreateEnergyImpactAftermath(to, color, severityScale);
            }
        }

        private void CreateEnergyImpactAftermath(Vector3 targetPoint, Color color, float severityScale)
        {
            float scale = Mathf.Clamp(severityScale, 0.82f, 1.50f);
            CreateImpactDisc(
                "Energy Scorch Glow",
                targetPoint + Vector3.up * 0.055f,
                new Color(0.42f, 1f, 1f, 0.24f),
                0.82f,
                0.18f * scale,
                0.42f * scale,
                0.010f);
            CreateBeam(
                targetPoint + Vector3.up * 0.12f,
                targetPoint + Vector3.up * (0.42f * scale),
                new Color(color.r, color.g, color.b, 0.30f),
                0.58f,
                0.012f * scale);
        }

        private void CreateMissileImpactAftermath(Vector3 targetPoint, float severityScale, bool destroyedTarget)
        {
            float scale = Mathf.Clamp(severityScale + (destroyedTarget ? 0.24f : 0f), 0.95f, 1.85f);
            CreateImpactDisc(
                "Missile Crater Afterglow",
                targetPoint + Vector3.up * 0.040f,
                new Color(1f, 0.30f, 0.08f, 0.24f),
                0.96f,
                0.42f * scale,
                0.94f * scale,
                0.014f);
            CreateImpactDisc(
                "Missile Smoke Footprint",
                targetPoint + Vector3.up * 0.070f,
                new Color(0.08f, 0.07f, 0.06f, 0.22f),
                1.20f,
                0.28f * scale,
                1.18f * scale,
                0.018f);
        }

        private void CreateBallisticImpactAftermath(Vector3 from, Vector3 to, float severityScale)
        {
            Vector3 direction = SafeDirection(to - from);
            Vector3 side = LateralVector(direction);
            float scale = Mathf.Clamp(severityScale, 0.78f, 1.55f);
            Vector3 center = to - direction * (0.06f * scale) + Vector3.up * 0.055f;
            CreateImpactDisc(
                "Ballistic Bite Mark",
                center,
                new Color(1f, 0.72f, 0.22f, 0.24f),
                0.62f,
                0.08f * scale,
                0.22f * scale,
                0.008f);
            CreateBeam(
                center + side * (0.06f * scale),
                center + side * (0.36f * scale) + Vector3.up * (0.05f * scale),
                new Color(0.82f, 0.76f, 0.62f, 0.38f),
                0.42f,
                0.008f * scale);
            CreateBeam(
                center - side * (0.05f * scale),
                center - side * (0.30f * scale) + direction * (0.12f * scale) + Vector3.up * (0.04f * scale),
                new Color(1f, 0.56f, 0.18f, 0.32f),
                0.36f,
                0.007f * scale);
        }

        private void CreateHitDirectionCue(Vector3 from, Vector3 to, float severityScale)
        {
            Vector3 direction = SafeDirection(to - from);
            Vector3 side = LateralVector(direction);
            float scale = Mathf.Clamp(severityScale, 0.78f, 1.55f);
            Vector3 lift = Vector3.up * 0.10f;
            Vector3 strikeStart = to - direction * (0.58f * scale) + side * (0.08f * scale) + lift;
            Vector3 strikeEnd = to + direction * (0.10f * scale) - side * (0.02f * scale) + lift * 0.65f;
            CreateBeam(strikeStart, strikeEnd, new Color(1f, 0.95f, 0.58f, 0.56f), 0.090f, 0.010f * scale);
            CreateBeam(to - direction * (0.34f * scale) - side * (0.10f * scale) + lift * 0.60f, to - direction * (0.02f * scale) + side * (0.12f * scale) + lift * 0.92f, new Color(1f, 0.52f, 0.16f, 0.42f), 0.075f, 0.007f * scale);
        }

        private void CreateKillShockCue(Vector3 targetPoint, bool destroyedTarget, float severityScale)
        {
            if (!destroyedTarget)
            {
                return;
            }

            float scale = Mathf.Clamp(severityScale, 1.0f, 1.72f);
            Vector3 center = targetPoint + Vector3.up * 0.04f;
            CreateImpactDisc(
                "Kill Shock Ring",
                center,
                new Color(1f, 0.20f, 0.06f, 0.46f),
                0.46f,
                0.34f * scale,
                1.46f * scale,
                0.030f);
            CreateImpactDisc(
                "Kill Shock Outer Ring",
                center + Vector3.up * 0.02f,
                new Color(1f, 0.72f, 0.22f, 0.28f),
                0.62f,
                0.62f * scale,
                2.10f * scale,
                0.018f);
            CreateBeam(
                targetPoint + Vector3.up * 0.10f,
                targetPoint + Vector3.up * (1.32f * scale),
                new Color(1f, 0.36f, 0.08f, 0.42f),
                0.34f,
                0.055f * scale);
        }

        private void CreateSectionHitCue(Vector3 from, Vector3 to, string sectionName, bool destroyedTarget, float hitSeverityScale)
        {
            Vector3 direction = to - from;
            Vector3 side = LateralVector(direction);
            float scale = (destroyedTarget ? 1.15f : 1f) * hitSeverityScale;
            if (IsCombatSectionName(sectionName, "Cockpit") || IsCombatSectionName(sectionName, "Turret"))
            {
                Color cockpit = new(0.62f, 0.96f, 1f, 0.62f);
                CreateImpactDisc("Cockpit Hit Ring", to + Vector3.up * 0.02f, cockpit, 0.18f, 0.12f * scale, 0.42f * scale, 0.012f);
                CreateBeam(to - Vector3.up * 0.05f, to + Vector3.up * (0.62f * scale), cockpit, 0.11f, 0.010f);
                return;
            }

            if (IsCombatSectionName(sectionName, "Left Arm") || IsCombatSectionName(sectionName, "Right Arm") || IsCombatSectionName(sectionName, "Left") || IsCombatSectionName(sectionName, "Right"))
            {
                Color armSpark = new(1f, 0.64f, 0.16f, 0.74f);
                CreateBeam(to - side * (0.18f * scale), to + side * (0.52f * scale) + Vector3.up * 0.06f, armSpark, 0.075f, 0.010f);
                CreateBeam(to + side * (0.10f * scale), to - side * (0.42f * scale) + Vector3.up * 0.12f, new Color(1f, 0.90f, 0.36f, 0.62f), 0.070f, 0.008f);
                return;
            }

            if (IsCombatSectionName(sectionName, "Legs"))
            {
                CreateImpactDisc(
                    "Leg Hit Shock",
                    to + Vector3.down * 0.02f,
                    new Color(1f, 0.24f, 0.08f, 0.44f),
                    0.24f,
                    0.18f * scale,
                    0.58f * scale,
                    0.012f);
                CreateBeam(to, to + Vector3.up * (0.32f * scale), new Color(1f, 0.46f, 0.10f, 0.62f), 0.080f, 0.009f);
                return;
            }

            CreateImpactDisc(
                "Torso Hit Core",
                to + Vector3.up * 0.04f,
                new Color(1f, 0.76f, 0.24f, 0.38f),
                0.20f,
                0.16f * scale,
                0.48f * scale,
                0.014f);
            CreateBeam(to - side * (0.18f * scale), to + side * (0.18f * scale) + Vector3.up * 0.20f, new Color(1f, 0.82f, 0.28f, 0.60f), 0.080f, 0.010f);
        }

        private void CreateArmorMitigationCue(Vector3 from, Vector3 to, CombatEvent combatEvent)
        {
            if (combatEvent.MitigatedDamage <= 0.4f)
            {
                return;
            }

            Vector3 direction = SafeDirection(to - from);
            Vector3 side = LateralVector(direction);
            float totalIncoming = Mathf.Max(1f, combatEvent.Damage + combatEvent.MitigatedDamage);
            float mitigationRatio = Mathf.Clamp01(combatEvent.MitigatedDamage / totalIncoming);
            float scale = Mathf.Lerp(0.72f, 1.10f, mitigationRatio);
            Vector3 center = to - direction * 0.10f + Vector3.up * 0.16f;
            Color glint = new(0.74f, 1f, 1f, 0.56f);
            Color spark = new(1f, 0.94f, 0.58f, 0.70f);
            CreateImpactDisc(
                "Armor Mitigation Glint",
                center,
                glint,
                0.18f,
                0.16f * scale,
                0.48f * scale,
                0.012f);
            CreateBeam(center - side * (0.24f * scale), center + side * (0.30f * scale) + Vector3.up * 0.04f, spark, 0.085f, 0.009f);
            CreateBeam(center - Vector3.up * 0.04f, center + Vector3.up * (0.38f * scale), glint, 0.095f, 0.008f);
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

            CreateMuzzleFlash(from, color, 0.46f, 0.25f);
            CreateMissileMuzzleCue(from, direction, color);
            Vector3 side = LateralVector(direction);
            float arcHeight = Mathf.Clamp(distance * 0.16f, 0.45f, 2.2f);
            for (int index = 0; index < 3; index++)
            {
                float lane = index - 1f;
                float startT = 0.09f + index * 0.045f;
                float midT = 0.52f + index * 0.035f;
                float endT = Mathf.Min(0.96f, 0.86f + index * 0.035f);
                float laneArc = arcHeight * (0.92f + index * 0.08f);
                Vector3 laneOffset = side * (lane * 0.26f);
                Vector3 laneFrom = from + laneOffset + Vector3.up * (0.04f * index);
                Vector3 laneTo = to + laneOffset * 0.38f;
                Vector3 segmentStart = ArcPoint(laneFrom, laneTo, startT, laneArc);
                Vector3 segmentMid = ArcPoint(laneFrom, laneTo, midT, laneArc);
                Vector3 segmentEnd = ArcPoint(laneFrom, laneTo, endT, laneArc);
                CreateBeam(segmentStart, segmentMid, color, 0.28f, 0.046f);
                CreateBeam(segmentMid, segmentEnd, new Color(color.r, color.g, color.b, 0.58f), 0.22f, 0.034f);
                CreateImpact(segmentStart - direction.normalized * 0.08f, new Color(0.20f, 0.20f, 0.18f, 0.34f), false, 0.28f + index * 0.08f);
            }

            CreateImpact(ArcPoint(from, to, 0.72f, arcHeight) + side * 0.15f, new Color(1f, 0.72f, 0.24f, 0.72f), false, 0.38f);
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
            CreateMuzzleFlash(from, new Color(1f, 0.62f, 0.18f, 0.82f), 0.30f, 0.14f);
            CreateBallisticMuzzleCue(from, direction);
            Vector3 tracerStart = Vector3.Lerp(from, to, 0.58f);
            Vector3 tracerEnd = Vector3.Lerp(from, to, 0.96f);
            CreateBeam(tracerStart, tracerEnd, color, 0.075f, 0.018f);
            CreateBeam(
                Vector3.Lerp(from, to, 0.36f) - side * 0.42f,
                Vector3.Lerp(from, to, 0.70f) - side * 0.14f,
                new Color(1f, 0.86f, 0.38f, 0.62f),
                0.06f,
                0.012f);
            CreateBeam(from + side, from + normalized * Mathf.Min(distance * 0.18f, 1.25f) + side * 0.35f, new Color(1f, 0.64f, 0.22f, 0.72f), 0.08f, 0.026f);

            Vector3 sparkBase = to - normalized * 0.22f;
            CreateBeam(sparkBase, to + side * 0.80f + Vector3.up * 0.12f, new Color(1f, 0.86f, 0.36f, 0.68f), 0.07f, 0.012f);
            CreateBeam(sparkBase, to - side * 0.68f + Vector3.up * 0.07f, new Color(1f, 0.70f, 0.20f, 0.56f), 0.06f, 0.010f);
        }

        private void CreateEnergyTrace(Vector3 from, Vector3 to, Color color)
        {
            Vector3 direction = to - from;
            Color halo = new(color.r, color.g, color.b, 0.36f);
            Color core = new(0.82f, 1f, 1f, 0.98f);
            CreateMuzzleFlash(from, new Color(0.56f, 1f, 1f, 0.82f), 0.38f, 0.12f);
            CreateEnergyMuzzleCue(from, direction, color);
            CreateBeam(from, to, halo, 0.12f, 0.074f);
            CreateBeam(from, to, new Color(color.r, color.g, color.b, 0.92f), 0.095f, 0.040f);
            CreateBeam(from + Vector3.up * 0.04f, to + Vector3.up * 0.04f, core, 0.055f, 0.018f);
            CreateImpact(to + Vector3.up * 0.05f, new Color(0.62f, 1f, 1f, 0.60f), false, 0.50f);
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

        private void CreateEnergyMuzzleCue(Vector3 from, Vector3 direction, Color color)
        {
            Vector3 forward = SafeDirection(direction);
            CreateImpactDisc(
                "Energy Muzzle Charge",
                from + forward * 0.10f + Vector3.up * 0.02f,
                new Color(0.44f, 1f, 1f, 0.38f),
                0.18f,
                0.18f,
                0.52f,
                0.018f);
            CreateBeam(
                from + Vector3.up * 0.05f,
                from + forward * 0.42f + Vector3.up * 0.08f,
                new Color(color.r, color.g, color.b, 0.62f),
                0.14f,
                0.026f);
        }

        private void CreateMissileMuzzleCue(Vector3 from, Vector3 direction, Color color)
        {
            Vector3 forward = SafeDirection(direction);
            Vector3 side = LateralVector(forward) * 0.16f;
            Vector3 lift = Vector3.up * 0.05f;
            CreateBeam(from - side + lift, from + forward * 0.46f - side * 0.35f + Vector3.up * 0.12f, color, 0.18f, 0.026f);
            CreateBeam(from + lift * 0.75f, from + forward * 0.52f + Vector3.up * 0.14f, new Color(color.r, color.g, color.b, 0.62f), 0.16f, 0.020f);
            CreateBeam(from + side + lift, from + forward * 0.46f + side * 0.35f + Vector3.up * 0.12f, color, 0.18f, 0.026f);
            CreateImpact(from - forward * 0.08f + Vector3.up * 0.02f, new Color(0.16f, 0.15f, 0.13f, 0.36f), false, 0.34f);
        }

        private void CreateBallisticMuzzleCue(Vector3 from, Vector3 direction)
        {
            Vector3 forward = SafeDirection(direction);
            Vector3 side = LateralVector(forward) * 0.18f;
            Vector3 center = from + forward * 0.18f + Vector3.up * 0.04f;
            CreateBeam(center - side, center + side, new Color(1f, 0.76f, 0.20f, 0.62f), 0.10f, 0.012f);
            CreateBeam(center, center + forward * 0.44f, new Color(1f, 0.90f, 0.42f, 0.74f), 0.09f, 0.018f);
        }

        private static Vector3 SafeDirection(Vector3 direction)
        {
            return direction.sqrMagnitude <= 0.0001f ? Vector3.forward : direction.normalized;
        }

        private void CreateMuzzleFlash(Vector3 position, Color color, float scale, float duration)
        {
            CreateImpact(position + Vector3.up * 0.05f, color, false, scale);
            CreateBeam(
                position + Vector3.left * 0.12f,
                position + Vector3.right * 0.12f,
                new Color(color.r, color.g, color.b, Mathf.Min(color.a, 0.55f)),
                duration,
                0.010f);
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

        private void CreateImpactDisc(string effectName, Vector3 position, Color color, float duration, float startRadius, float endRadius, float height)
        {
            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = effectName;
            disc.transform.position = position;
            Collider discCollider = disc.GetComponent<Collider>();
            if (discCollider != null)
            {
                Destroy(discCollider);
            }

            DemoTransientEffect transient = disc.AddComponent<DemoTransientEffect>();
            transient.Begin(
                color,
                duration,
                new Vector3(startRadius, height, startRadius),
                new Vector3(endRadius, height * 0.35f, endRadius));
        }

        private void CaptureMissionTickEvents()
        {
            CaptureCombatEvents();
            CaptureObjectiveEvents();
            CaptureUnitActivationEvents();
            CaptureScriptEvents();
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
                    SpawnObjectiveEventCue(objectiveEvent);
                    Debug.Log("MC2 objective active: " + objectiveEvent.Title);
                }
                else if (objectiveEvent.Kind == ObjectiveEventKind.Completed)
                {
                    AddCombatLogLine("Objective done: " + objectiveEvent.Title);
                    SpawnObjectiveEventCue(objectiveEvent);
                    Debug.Log("MC2 objective complete: " + objectiveEvent.Title);
                }
            }
        }

        private void SpawnObjectiveEventCue(ObjectiveEvent objectiveEvent)
        {
            ObjectiveState objective = FindObjectiveState(objectiveEvent.ObjectiveIndex);
            if (objective == null || !TryGetObjectiveCuePoint(objective.Definition, out Vector2 missionPoint, out float missionRadius))
            {
                return;
            }

            bool completed = objectiveEvent.Kind == ObjectiveEventKind.Completed;
            Vector3 center = GroundMarkerPosition(missionPoint, 0.13f);
            Color color = completed
                ? new Color(0.28f, 1f, 0.56f, 0.84f)
                : new Color(1f, 0.82f, 0.26f, 0.78f);
            float scale = Mathf.Clamp(missionRadius / 150f, 0.85f, 2.15f);
            CreateImpactDisc(
                completed ? "Objective Complete Pulse" : "Objective Active Pulse",
                center,
                new Color(color.r, color.g, color.b, completed ? 0.34f : 0.30f),
                completed ? 0.95f : 0.78f,
                0.34f * scale,
                (completed ? 1.72f : 1.38f) * scale,
                0.024f);
            CreateBeam(
                center + Vector3.up * 0.08f,
                center + Vector3.up * (completed ? 1.55f : 1.15f),
                new Color(color.r, color.g, color.b, completed ? 0.54f : 0.44f),
                completed ? 0.84f : 0.62f,
                completed ? 0.034f : 0.026f);
            CreateImpact(center + Vector3.up * 0.18f, new Color(color.r, color.g, color.b, 0.48f), false, completed ? 0.48f : 0.36f);
        }

        private ObjectiveState FindObjectiveState(int objectiveIndex)
        {
            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (objective?.Definition != null && objective.Definition.index == objectiveIndex)
                {
                    return objective;
                }
            }

            return null;
        }

        private static bool TryGetObjectiveCuePoint(ObjectiveDefinition objective, out Vector2 missionPoint, out float missionRadius)
        {
            if (objective?.conditions != null)
            {
                foreach (ObjectiveCondition condition in objective.conditions)
                {
                    if (condition?.targetStructure?.position != null)
                    {
                        missionPoint = new Vector2(condition.targetStructure.position.x, condition.targetStructure.position.y);
                        missionRadius = 180f;
                        return true;
                    }

                    if (condition?.targetArea != null)
                    {
                        missionPoint = new Vector2(condition.targetArea.x, condition.targetArea.y);
                        missionRadius = Mathf.Max(80f, condition.targetArea.radius);
                        return true;
                    }

                    if (condition?.targetUnit?.position != null)
                    {
                        missionPoint = new Vector2(condition.targetUnit.position.x, condition.targetUnit.position.y);
                        missionRadius = 115f;
                        return true;
                    }
                }
            }

            if (objective?.marker != null)
            {
                missionPoint = new Vector2(objective.marker.x, objective.marker.y);
                missionRadius = 120f;
                return true;
            }

            missionPoint = default;
            missionRadius = 0f;
            return false;
        }

        private static string ObjectiveEventCueSummary()
        {
            return "ObjectivePulse=active+complete+target";
        }

        private static string ObjectiveGuideCueSummary()
        {
            return "ObjectiveGuide=active+beacon+target";
        }

        private static string ObjectiveRouteCueSummary()
        {
            return "ObjectiveRoute=commander+target+endcaps";
        }

        private static string ObjectivePressureCueSummary()
        {
            return "ObjectivePressure=steady+damaged+critical";
        }

        private void CaptureUnitActivationEvents()
        {
            List<string> contactKeys = new();
            Dictionary<string, int> contactCounts = new(StringComparer.Ordinal);
            Dictionary<string, string> contactLabels = new(StringComparer.Ordinal);
            Dictionary<string, Vector2> contactCenters = new(StringComparer.Ordinal);
            Dictionary<string, int> contactCenterSamples = new(StringComparer.Ordinal);
            foreach (UnitActivationEvent activationEvent in mission.RecentUnitActivationEvents)
            {
                UnitState unit = mission.FindUnit(activationEvent.UnitId);
                string key = EncounterGroupKey(unit);
                string label = EncounterGroupLabel(key, activationEvent.UnitType);
                if (!contactCounts.ContainsKey(key))
                {
                    contactKeys.Add(key);
                    contactCounts[key] = 0;
                    contactLabels[key] = label;
                }

                contactCounts[key]++;
                if (unit != null)
                {
                    if (!contactCenters.ContainsKey(key))
                    {
                        contactCenters[key] = Vector2.zero;
                        contactCenterSamples[key] = 0;
                    }

                    contactCenters[key] += unit.MissionPosition;
                    contactCenterSamples[key]++;
                }

                Debug.Log("MC2 enemy activated: " + activationEvent.UnitId + " " + activationEvent.UnitType + " " + activationEvent.Brain);
            }

            for (int index = 0; index < contactKeys.Count; index++)
            {
                string key = contactKeys[index];
                int count = contactCounts[key];
                string label = contactLabels[key];
                string line = "Contact: " + label + (count > 1 ? " x" + count.ToString(CultureInfo.InvariantCulture) : "");
                lastContactEventTimeSeconds = mission?.MissionTimeSeconds ?? Time.time;
                lastContactLabel = label;
                lastContactCount = count;
                AddCombatLogLine(line);
                if (contactCenters.TryGetValue(key, out Vector2 center)
                    && contactCenterSamples.TryGetValue(key, out int samples)
                    && samples > 0)
                {
                    SpawnEncounterActivationCue(key, center / samples, count);
                }

                Debug.Log("MC2 enemy contact group: " + key + " count=" + count.ToString(CultureInfo.InvariantCulture));
            }
        }

        private void SpawnEncounterActivationCue(string key, Vector2 missionPoint, int count)
        {
            Vector3 center = GroundMarkerPosition(missionPoint, 0.12f);
            Color color = EncounterActivationCueColor(key);
            float scale = Mathf.Clamp(0.95f + count * 0.10f, 1.05f, 2.05f);
            CreateImpactDisc(
                "Encounter Wake Ring",
                center,
                new Color(color.r, color.g, color.b, 0.36f),
                0.86f,
                0.44f * scale,
                1.55f * scale,
                0.028f);
            CreateBeam(
                center + Vector3.up * 0.08f,
                center + Vector3.up * Mathf.Clamp(1.0f + count * 0.10f, 1.1f, 1.8f),
                new Color(color.r, color.g, color.b, 0.58f),
                0.72f,
                0.035f);
            CreateImpact(center + Vector3.up * 0.22f, new Color(color.r, color.g, color.b, 0.52f), false, 0.42f * scale);
            CreateEncounterActivationPips(center, color, count, scale);
            CreateEncounterActivationVector(missionPoint, center, color, scale);
        }

        private void CreateEncounterActivationVector(Vector2 missionPoint, Vector3 center, Color color, float scale)
        {
            if (!TryGetClosestPlayerMissionPosition(missionPoint, out Vector2 playerPoint))
            {
                return;
            }

            Vector2 missionDirection = playerPoint - missionPoint;
            if (missionDirection.sqrMagnitude <= 0.01f)
            {
                return;
            }

            Vector3 direction = SafeDirection(new Vector3(missionDirection.x, 0f, missionDirection.y));
            Vector3 side = LateralVector(direction);
            Vector3 start = center + direction * (0.42f * scale) + Vector3.up * 0.045f;
            Vector3 end = center + direction * (1.36f * scale) + Vector3.up * 0.045f;
            Color vectorColor = new(color.r, color.g, color.b, 0.52f);
            CreateBeam(start, end, vectorColor, 0.82f, 0.026f * scale);
            CreateBeam(end - direction * (0.22f * scale), end - direction * (0.45f * scale) + side * (0.26f * scale), vectorColor, 0.72f, 0.018f * scale);
            CreateBeam(end - direction * (0.22f * scale), end - direction * (0.45f * scale) - side * (0.26f * scale), vectorColor, 0.72f, 0.018f * scale);
            CreateImpactDisc(
                "Encounter Wake Vector End",
                end + Vector3.down * 0.025f,
                new Color(color.r, color.g, color.b, 0.36f),
                0.78f,
                0.10f * scale,
                0.28f * scale,
                0.016f);
        }

        private bool TryGetClosestPlayerMissionPosition(Vector2 from, out Vector2 playerPoint)
        {
            UnitState best = null;
            float bestDistanceSqr = float.MaxValue;
            foreach (UnitState unit in mission.PlayerUnits())
            {
                if (unit == null || !unit.IsActive || unit.IsDestroyed)
                {
                    continue;
                }

                float distanceSqr = (unit.MissionPosition - from).sqrMagnitude;
                if (distanceSqr < bestDistanceSqr)
                {
                    bestDistanceSqr = distanceSqr;
                    best = unit;
                }
            }

            if (best == null)
            {
                playerPoint = default;
                return false;
            }

            playerPoint = best.MissionPosition;
            return true;
        }

        private void CreateEncounterActivationPips(Vector3 center, Color color, int count, float scale)
        {
            int pipCount = Mathf.Clamp(count, 1, 8);
            float ringRadius = 0.58f * scale;
            float pipStart = 0.055f * scale;
            float pipEnd = 0.16f * scale;
            for (int index = 0; index < pipCount; index++)
            {
                float angle = Mathf.PI * 2f * index / pipCount;
                Vector3 offset = new(Mathf.Cos(angle) * ringRadius, 0.035f, Mathf.Sin(angle) * ringRadius);
                CreateImpactDisc(
                    "Encounter Wake Pip",
                    center + offset,
                    new Color(color.r, color.g, color.b, 0.48f),
                    0.74f,
                    pipStart,
                    pipEnd,
                    0.018f);
            }
        }

        private static Color EncounterActivationCueColor(string key)
        {
            switch (key)
            {
                case "ambush":
                    return new Color(1f, 0.32f, 0.10f, 0.90f);
                case "starslayer":
                    return new Color(0.86f, 0.42f, 1f, 0.86f);
                case "north":
                    return new Color(1f, 0.60f, 0.16f, 0.86f);
                default:
                    return new Color(1f, 0.78f, 0.22f, 0.84f);
            }
        }

        private static string EncounterGroupKey(UnitState unit)
        {
            string brain = unit?.Brain ?? string.Empty;
            if (BrainContains(brain, "infantry_ambush"))
            {
                return "ambush";
            }

            if (BrainStartsWith(brain, "mc2_01_Pat1") || IsEastLrmUnit(brain, unit))
            {
                return "airfield";
            }

            if (BrainStartsWith(brain, "mc2_01_Pat2") || BrainEquals(brain, "mc2_01_Pat4"))
            {
                return "north";
            }

            if (BrainEquals(brain, "mc2_01_Starslayer")
                || BrainEquals(brain, "mc2_01_Urbies")
                || IsWestLrmUnit(brain, unit))
            {
                return "starslayer";
            }

            return string.IsNullOrWhiteSpace(unit?.UnitType) ? "other" : "other:" + unit.UnitType;
        }

        private static string EncounterGroupLabel(string key, string fallbackUnitType)
        {
            switch (key)
            {
                case "airfield":
                    return "Airfield patrol";
                case "north":
                    return "North patrol";
                case "ambush":
                    return "Infantry ambush";
                case "starslayer":
                    return "Starslayer lance";
                default:
                    return string.IsNullOrWhiteSpace(fallbackUnitType) ? "Hostile" : fallbackUnitType;
            }
        }

        private static bool IsEastLrmUnit(string brain, UnitState unit)
        {
            return BrainEquals(brain, "mc2_01_LRMs") && unit != null && unit.SpawnPosition.x > 0f;
        }

        private static bool IsWestLrmUnit(string brain, UnitState unit)
        {
            return BrainEquals(brain, "mc2_01_LRMs") && unit != null && unit.SpawnPosition.x < 0f;
        }

        private static bool BrainContains(string brain, string value)
        {
            return !string.IsNullOrEmpty(brain)
                && brain.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool BrainStartsWith(string brain, string value)
        {
            return !string.IsNullOrEmpty(brain)
                && brain.StartsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        private static bool BrainEquals(string brain, string value)
        {
            return string.Equals(brain, value, StringComparison.OrdinalIgnoreCase);
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
                SpawnScriptEventCue(scriptEvent);
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

        private void SpawnScriptEventCue(MissionScriptEvent scriptEvent)
        {
            if (scriptEvent == null
                || !TryGetScriptEventCuePoint(scriptEvent, out Vector2 missionPoint, out float missionRadius))
            {
                return;
            }

            if (scriptEvent.Kind == MissionScriptEventKind.MissionResult)
            {
                SpawnMissionResultCue(scriptEvent, missionPoint, missionRadius);
                return;
            }

            Vector3 center = GroundMarkerPosition(missionPoint, 0.16f);
            Color color = ScriptEventCueColor(scriptEvent);
            float scale = Mathf.Clamp(missionRadius / 160f, 0.85f, 2.25f);
            CreateImpactDisc(
                "Script Signal Pulse",
                center,
                new Color(color.r, color.g, color.b, 0.28f),
                0.72f,
                0.30f * scale,
                1.28f * scale,
                0.022f);
            CreateBeam(
                center + Vector3.up * 0.08f,
                center + Vector3.up * Mathf.Clamp(0.95f * scale, 0.9f, 1.75f),
                new Color(color.r, color.g, color.b, 0.46f),
                0.62f,
                0.026f);
            CreateImpact(center + Vector3.up * 0.22f, new Color(color.r, color.g, color.b, 0.44f), false, 0.34f * scale);
        }

        private void SpawnMissionResultCue(MissionScriptEvent scriptEvent, Vector2 missionPoint, float missionRadius)
        {
            bool failed = ContainsIgnoreCase(scriptEvent?.Signal, "Player");
            Color color = failed
                ? new Color(1f, 0.18f, 0.12f, 0.86f)
                : new Color(0.28f, 1f, 0.56f, 0.86f);
            Vector3 center = GroundMarkerPosition(missionPoint, 0.18f);
            float scale = Mathf.Clamp(missionRadius / 145f, 0.95f, 2.45f);
            CreateImpactDisc(
                failed ? "Mission Failed Result Ring" : "Mission Complete Result Ring",
                center,
                new Color(color.r, color.g, color.b, 0.34f),
                1.10f,
                0.42f * scale,
                1.90f * scale,
                0.030f);
            CreateImpactDisc(
                failed ? "Mission Failed Result Shock" : "Mission Complete Result Shock",
                center + Vector3.up * 0.025f,
                new Color(color.r, color.g, color.b, 0.24f),
                0.72f,
                0.18f * scale,
                1.15f * scale,
                0.020f);
            CreateBeam(
                center + Vector3.up * 0.10f,
                center + Vector3.up * Mathf.Clamp(1.42f * scale, 1.20f, 2.45f),
                new Color(color.r, color.g, color.b, 0.58f),
                0.92f,
                failed ? 0.034f : 0.038f);
            CreateImpact(center + Vector3.up * 0.30f, new Color(color.r, color.g, color.b, 0.52f), false, 0.48f * scale);
        }

        private bool TryGetScriptEventCuePoint(MissionScriptEvent scriptEvent, out Vector2 missionPoint, out float missionRadius)
        {
            if (!string.IsNullOrWhiteSpace(scriptEvent.SourceId))
            {
                StructureState sourceStructure = mission.FindStructure(scriptEvent.SourceId);
                if (sourceStructure != null)
                {
                    missionPoint = sourceStructure.MissionPosition;
                    missionRadius = sourceStructure.Radius;
                    return true;
                }

                UnitState sourceUnit = mission.FindUnit(scriptEvent.SourceId);
                if (sourceUnit != null)
                {
                    missionPoint = sourceUnit.MissionPosition;
                    missionRadius = 120f;
                    return true;
                }
            }

            if (TryGetScriptEventObjectiveCuePoint(scriptEvent.Signal, out missionPoint, out missionRadius))
            {
                return true;
            }

            return TryGetPlayerSquadCueCenter(out missionPoint, out missionRadius);
        }

        private bool TryGetScriptEventObjectiveCuePoint(string signal, out Vector2 missionPoint, out float missionRadius)
        {
            int objectiveIndex = -1;
            if (string.Equals(signal, "Objective_0_Decided", StringComparison.OrdinalIgnoreCase))
            {
                objectiveIndex = 0;
            }
            else if (string.Equals(signal, "patrols_Area_Trigger", StringComparison.OrdinalIgnoreCase))
            {
                objectiveIndex = 3;
            }
            else if (string.Equals(signal, "Starslayer_Trigger", StringComparison.OrdinalIgnoreCase))
            {
                objectiveIndex = 7;
            }
            else if (string.Equals(signal, "Starslayer_VO_Check", StringComparison.OrdinalIgnoreCase))
            {
                objectiveIndex = 8;
            }

            ObjectiveState objective = objectiveIndex < 0 ? null : FindObjectiveState(objectiveIndex);
            if (objective?.Definition != null && TryGetObjectiveCuePoint(objective.Definition, out missionPoint, out missionRadius))
            {
                return true;
            }

            missionPoint = default;
            missionRadius = 0f;
            return false;
        }

        private bool TryGetPlayerSquadCueCenter(out Vector2 missionPoint, out float missionRadius)
        {
            Vector2 total = Vector2.zero;
            int count = 0;
            foreach (UnitState unit in mission.PlayerUnits())
            {
                if (unit == null || !unit.IsActive || unit.IsDestroyed)
                {
                    continue;
                }

                total += unit.MissionPosition;
                count++;
            }

            if (count <= 0)
            {
                missionPoint = default;
                missionRadius = 0f;
                return false;
            }

            missionPoint = total / count;
            missionRadius = 140f;
            return true;
        }

        private static Color ScriptEventCueColor(MissionScriptEvent scriptEvent)
        {
            string signal = scriptEvent?.Signal ?? "";
            if (ContainsIgnoreCase(signal, "Starslayer"))
            {
                return new Color(0.86f, 0.42f, 1f, 0.82f);
            }

            if (ContainsIgnoreCase(signal, "Hangar") || scriptEvent?.Kind == MissionScriptEventKind.Combat)
            {
                return new Color(1f, 0.48f, 0.12f, 0.82f);
            }

            if (scriptEvent?.Kind == MissionScriptEventKind.MissionResult)
            {
                return ContainsIgnoreCase(signal, "Player")
                    ? new Color(1f, 0.20f, 0.12f, 0.82f)
                    : new Color(0.28f, 1f, 0.56f, 0.82f);
            }

            if (scriptEvent?.Kind == MissionScriptEventKind.UnitActivation)
            {
                return new Color(1f, 0.76f, 0.22f, 0.80f);
            }

            return new Color(0.36f, 0.88f, 1f, 0.78f);
        }

        private static string ScriptEventCueSummary()
        {
            return "ScriptCue=ring+beacon+signal";
        }

        private static string MissionResultCueSummary()
        {
            return "ResultCue=complete+failed";
        }

        private static string DebriefDamageCueSummary()
        {
            return "DebriefDamage=unit+section+overflow";
        }

        private static string DebriefTargetCueSummary()
        {
            return "DebriefTargets=count+labels+overflow";
        }

        private static string CommandCueSummary()
        {
            return "Command=move+attack+single+blocked+partial+reject-unit";
        }

        private static string OrderPathCueSummary()
        {
            return "OrderPath=move+jet+attack+endcap";
        }

        private static string OrderArrivalCueSummary()
        {
            return "OrderArrival=move+jet+attack-ready";
        }

        private void SpawnCommandResultCue(CommanderCommand command, CommanderCommandResult result)
        {
            if (command == null || result == null)
            {
                return;
            }

            if (result.Accepted)
            {
                SpawnAcceptedCommandCue(command, result);
                int expectedCount = CountCommandEligiblePlayerUnits(command);
                if (expectedCount > result.AcceptedCount && result.AcceptedCount > 0)
                {
                    SpawnPartialCommandCue(command, result.AcceptedCount, expectedCount);
                    SpawnPartialRejectedUnitCues(command);
                }

                return;
            }

            SpawnBlockedCommandCue(command);
        }

        private void SpawnAcceptedCommandCue(CommanderCommand command, CommanderCommandResult result)
        {
            if (command == null || result == null || !result.Accepted)
            {
                return;
            }

            bool isSingleUnitCommand = command.Scope == CommanderCommandScope.Unit && !string.IsNullOrEmpty(command.UnitId);
            switch (command.Kind)
            {
                case CommanderCommandKind.Move:
                    SpawnCommandMoveCue(command.MissionPoint, isSingleUnitCommand, false);
                    break;
                case CommanderCommandKind.Jump:
                    SpawnCommandMoveCue(command.MissionPoint, isSingleUnitCommand, true);
                    break;
                case CommanderCommandKind.AttackUnit:
                case CommanderCommandKind.AttackStructure:
                    SpawnCommandAttackCue(command.TargetId, isSingleUnitCommand);
                    break;
            }

            if (isSingleUnitCommand)
            {
                SpawnCommandSingleCue(command.UnitId, command.Kind);
            }
        }

        private void SpawnBlockedCommandCue(CommanderCommand command)
        {
            if (!TryGetCommandCuePoint(command, out Vector2 missionPoint, out float radius))
            {
                return;
            }

            Vector3 center = GroundMarkerPosition(missionPoint, 0.20f);
            float scale = Mathf.Clamp(radius / 140f, 0.85f, 1.85f);
            Color color = new(1f, 0.12f, 0.10f, 0.78f);
            CreateImpactDisc("Command Blocked Pulse", center, new Color(color.r, color.g, color.b, 0.32f), 0.56f, 0.28f * scale, 1.10f * scale, 0.026f);
            CreateBeam(center + Vector3.up * 0.05f, center + Vector3.up * Mathf.Clamp(0.74f * scale, 0.62f, 1.20f), new Color(color.r, color.g, color.b, 0.52f), 0.42f, 0.030f);
            CreateImpact(center + Vector3.up * 0.12f, new Color(color.r, color.g, color.b, 0.44f), false, 0.30f * scale);
        }

        private void SpawnPartialCommandCue(CommanderCommand command, int acceptedCount, int expectedCount)
        {
            if (!TryGetCommandCuePoint(command, out Vector2 missionPoint, out float radius))
            {
                return;
            }

            float ratio = Mathf.Clamp01(expectedCount <= 0 ? 0f : acceptedCount / (float)expectedCount);
            Vector3 center = GroundMarkerPosition(missionPoint, 0.23f);
            float scale = Mathf.Clamp(radius / 150f, 0.90f, 1.90f);
            Color color = Color.Lerp(new Color(1f, 0.16f, 0.08f, 0.74f), new Color(1f, 0.78f, 0.18f, 0.72f), ratio);
            CreateImpactDisc("Command Partial Pulse", center, new Color(color.r, color.g, color.b, 0.28f), 0.62f, 0.34f * scale, 1.34f * scale, 0.024f);
            CreateBeam(center + Vector3.up * 0.06f, center + Vector3.up * Mathf.Clamp(0.92f * scale, 0.72f, 1.42f), new Color(color.r, color.g, color.b, 0.48f), 0.48f, 0.024f);
        }

        private void SpawnPartialRejectedUnitCues(CommanderCommand command)
        {
            if (mission == null
                || command == null
                || command.Scope != CommanderCommandScope.Squad
                || command.Kind != CommanderCommandKind.Jump)
            {
                return;
            }

            foreach (UnitState unit in mission.PlayerUnits())
            {
                if (!unit.IsActive || unit.IsDestroyed || unit.IsDetached || unit.IsJumping)
                {
                    continue;
                }

                Vector3 center = GroundMarkerPosition(unit.MissionPosition, 0.18f);
                Color color = new(1f, 0.22f, 0.10f, 0.68f);
                CreateImpactDisc("Command Rejected Unit Pulse", center, new Color(color.r, color.g, color.b, 0.30f), 0.52f, 0.24f, 0.88f, 0.022f);
                CreateBeam(center + Vector3.up * 0.05f, center + Vector3.up * 0.78f, new Color(color.r, color.g, color.b, 0.46f), 0.34f, 0.020f);
                CreateImpact(center + Vector3.up * 0.14f, new Color(color.r, color.g, color.b, 0.42f), false, 0.28f);
            }
        }

        private int CountCommandEligiblePlayerUnits(CommanderCommand command)
        {
            if (command == null || mission == null)
            {
                return 0;
            }

            if (command.Scope == CommanderCommandScope.Unit)
            {
                UnitState unit = mission.FindUnit(command.UnitId);
                return unit != null && unit.IsPlayerUnit && unit.IsActive && !unit.IsDestroyed ? 1 : 0;
            }

            int count = 0;
            foreach (UnitState unit in mission.PlayerUnits())
            {
                if (unit.IsActive && !unit.IsDestroyed && !unit.IsDetached)
                {
                    count++;
                }
            }

            return count;
        }

        private bool TryGetCommandCuePoint(CommanderCommand command, out Vector2 missionPoint, out float radius)
        {
            missionPoint = default;
            radius = 120f;
            if (command == null)
            {
                return false;
            }

            switch (command.Kind)
            {
                case CommanderCommandKind.Move:
                case CommanderCommandKind.Jump:
                    missionPoint = command.MissionPoint;
                    return true;
                case CommanderCommandKind.AttackUnit:
                case CommanderCommandKind.AttackStructure:
                    return TryGetTargetMarker(command.TargetId, out missionPoint, out radius);
                default:
                    return false;
            }
        }

        private void SpawnCommandMoveCue(Vector2 missionPoint, bool isSingleUnitCommand, bool isJump)
        {
            Vector3 center = GroundMarkerPosition(missionPoint, 0.17f);
            Color color = isJump
                ? new Color(0.44f, 0.92f, 1f, 0.72f)
                : isSingleUnitCommand
                    ? new Color(1f, 0.64f, 0.18f, 0.72f)
                    : new Color(0.22f, 0.82f, 1f, 0.68f);
            float duration = isJump ? 0.72f : 0.58f;
            CreateImpactDisc("Command Move Pulse", center, new Color(color.r, color.g, color.b, 0.34f), duration, 0.26f, isJump ? 1.30f : 1.05f, 0.024f);
            CreateBeam(center + Vector3.up * 0.06f, center + Vector3.up * (isJump ? 1.28f : 0.88f), new Color(color.r, color.g, color.b, 0.54f), duration * 0.82f, isJump ? 0.034f : 0.026f);
            CreateImpact(center + Vector3.up * 0.14f, new Color(color.r, color.g, color.b, 0.46f), false, isJump ? 0.48f : 0.34f);
        }

        private void SpawnCommandAttackCue(string targetId, bool isSingleUnitCommand)
        {
            if (string.IsNullOrEmpty(targetId) || !TryGetTargetMarker(targetId, out Vector2 missionPoint, out float radius))
            {
                return;
            }

            Vector3 center = GroundMarkerPosition(missionPoint, 0.18f);
            float scale = Mathf.Clamp(radius / 120f, 0.85f, 2.25f);
            Color color = isSingleUnitCommand
                ? new Color(1f, 0.42f, 0.12f, 0.76f)
                : new Color(1f, 0.62f, 0.16f, 0.72f);
            CreateImpactDisc("Command Attack Pulse", center, new Color(color.r, color.g, color.b, 0.34f), 0.64f, 0.32f * scale, 1.25f * scale, 0.030f);
            CreateBeam(center + Vector3.up * 0.08f, center + Vector3.up * Mathf.Clamp(0.95f * scale, 0.85f, 1.85f), new Color(color.r, color.g, color.b, 0.56f), 0.54f, 0.032f);
            CreateImpact(center + Vector3.up * 0.18f, new Color(color.r, color.g, color.b, 0.48f), false, 0.42f * scale);
        }

        private void SpawnCommandSingleCue(string unitId, CommanderCommandKind commandKind)
        {
            UnitState unit = mission?.FindUnit(unitId);
            if (unit == null || unit.IsDestroyed)
            {
                return;
            }

            Color color = commandKind == CommanderCommandKind.Move || commandKind == CommanderCommandKind.Jump
                ? new Color(1f, 0.72f, 0.20f, 0.62f)
                : new Color(1f, 0.34f, 0.12f, 0.66f);
            Vector3 center = GroundMarkerPosition(unit.MissionPosition, 0.15f);
            CreateImpactDisc("Command Single Pulse", center, new Color(color.r, color.g, color.b, 0.30f), 0.48f, 0.22f, 0.86f, 0.022f);
            CreateBeam(center + Vector3.up * 0.05f, center + Vector3.up * 0.70f, new Color(color.r, color.g, color.b, 0.48f), 0.38f, 0.020f);
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
                showMissionResultPanel = true;
                SetDemoFlowScreen(DemoFlowScreen.Debrief);
                statusText = "Mission complete";
                AddCombatLogLine("Mission complete");
                Debug.Log("MC2 mission complete: " + mission.ResultReason);
                SetPaused(true);
            }
            else if (mission.Result == MissionResultState.Defeat)
            {
                ApplyMissionReceiptOnce();
                showMissionResultPanel = true;
                SetDemoFlowScreen(DemoFlowScreen.Debrief);
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
                AddCombatLogLine(MissionReceiptLogText(null, missionReceipt));
                TryAutoSaveSavedAccount("mission receipt");
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
            Texture2D terrainTexture = null;
            if (ReferenceTerrainTextureLibrary.TryBuildCompositeTexture(mission.Contract, out Texture2D compositeTexture, out string terrainTextureSummary))
            {
                terrainTexture = compositeTexture;
                ownedTextures.Add(compositeTexture);
                lastTerrainTextureSummary = terrainTextureSummary;
                Debug.Log("Loaded private terrain texture composite: " + terrainTextureSummary);
            }
            else
            {
                lastTerrainTextureSummary = terrainTextureSummary;
                Debug.Log("Using source terrain vertex colors without private terrain textures: " + terrainTextureSummary);
            }

            meshRenderer.sharedMaterial = MakeTerrainMaterial(terrainTexture);

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
            float light = TerrainLightMultiplier(sample);
            if (sample.elevation <= waterLevel + 4f)
            {
                return ApplyTerrainLight(new Color(0.06f, 0.22f, 0.30f), light);
            }

            if (sample.elevation <= waterLevel + 18f)
            {
                return ApplyTerrainLight(new Color(0.20f, 0.30f, 0.22f), light);
            }

            int textureId = SourceTextureId(sample);
            if (sample.terrainType == 13 || sample.terrainType == 14 || sample.terrainType == 15 || sample.terrainType == 16)
            {
                Color runway = sample.terrainType == 14
                    ? new Color(0.38f, 0.38f, 0.34f)
                    : new Color(0.31f, 0.32f, 0.30f);
                return ApplyTerrainLight(TerrainTileVariation(runway, textureId, 0.10f), light);
            }

            if (sample.terrainType == 20)
            {
                return ApplyTerrainLight(TerrainTileVariation(new Color(0.37f, 0.30f, 0.20f), textureId, 0.12f), light);
            }

            if (textureId > 2)
            {
                return ApplyTerrainLight(TerrainTileVariation(new Color(0.29f, 0.31f, 0.22f), textureId, 0.16f), light);
            }

            float heightT = Mathf.InverseLerp(mission.Contract.terrainMesh.elevationMin, mission.Contract.terrainMesh.elevationMax, sample.elevation);
            Color baseColor = Color.Lerp(new Color(0.12f, 0.24f, 0.12f), new Color(0.35f, 0.34f, 0.22f), heightT);
            return ApplyTerrainLight(TerrainTileVariation(baseColor, textureId, 0.05f), light);
        }

        private static int SourceTextureId(TerrainMeshSample sample)
        {
            if (sample.textureId > 0)
            {
                return sample.textureId;
            }

            return (int)(sample.textureData & 0xffffL);
        }

        private static Color TerrainTileVariation(Color color, int textureId, float strength)
        {
            int hash = Mathf.Abs((textureId * 1103515245 + 12345) >> 8);
            float variation = ((hash % 1000) / 999f - 0.5f) * strength;
            float warm = (((hash / 1000) % 1000) / 999f - 0.5f) * strength * 0.5f;
            return new Color(
                Mathf.Clamp01(color.r + variation + warm),
                Mathf.Clamp01(color.g + variation),
                Mathf.Clamp01(color.b + variation - warm),
                1f);
        }

        private static float TerrainLightMultiplier(TerrainMeshSample sample)
        {
            if (sample.light <= 0)
            {
                return 1f;
            }

            int lowByte = (int)(sample.light & 0xffL);
            return Mathf.Lerp(0.78f, 1.16f, lowByte / 255f);
        }

        private static Color ApplyTerrainLight(Color color, float light)
        {
            return new Color(
                Mathf.Clamp01(color.r * light),
                Mathf.Clamp01(color.g * light),
                Mathf.Clamp01(color.b * light),
                color.a);
        }

        private Material MakeTerrainMaterial(Texture2D terrainTexture)
        {
            string cacheKey = "SourceTerrainVertexColor";
            if (materialCache.TryGetValue(cacheKey, out Material cachedMaterial))
            {
                ConfigureTerrainMaterial(cachedMaterial, terrainTexture);
                return cachedMaterial;
            }

            Shader shader = Shader.Find("MC2Demo/Source Terrain Vertex Color")
                ?? Shader.Find("Legacy Shaders/Diffuse")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
            {
                Debug.LogWarning("No shader available for source terrain material.");
                return null;
            }

            Material material = new(shader)
            {
                name = "SourceTerrainVertexColor",
                color = Color.white
            };
            if (material.HasProperty("_Tint"))
            {
                material.SetColor("_Tint", Color.white);
            }

            ConfigureTerrainMaterial(material, terrainTexture);
            ownedMaterials.Add(material);
            materialCache[cacheKey] = material;
            return material;
        }

        private static void ConfigureTerrainMaterial(Material material, Texture2D terrainTexture)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", terrainTexture == null ? Texture2D.whiteTexture : terrainTexture);
            }

            if (material.HasProperty("_TextureStrength"))
            {
                material.SetFloat("_TextureStrength", terrainTexture == null ? 0f : 0.72f);
            }
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
            ConfigureCameraFromMission();
            GameObject cameraObject = new("Demo Camera");
            mainCamera = cameraObject.AddComponent<Camera>();
            mainCamera.orthographic = true;
            ApplyCameraProjection();
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.06f, 0.07f, 0.075f);
            Camera.SetupCurrent(mainCamera);
        }

        private void CreateUnits()
        {
            int playerPresentationIndex = 0;
            foreach (UnitState unit in mission.Units)
            {
                int presentationIndex = unit.IsPlayerUnit ? playerPresentationIndex++ : -1;
                PrimitiveType primitive = unit.IsPlayerUnit ? PrimitiveType.Capsule : PrimitiveType.Cube;
                GameObject unitObject = GameObject.CreatePrimitive(primitive);
                bool tallUnit = unit.IsPlayerUnit || ReferenceObjMeshLibrary.IsTallReferenceUnit(unit.UnitType);
                unitObject.transform.localScale = tallUnit
                    ? new Vector3(1.3f, 1.5f, 1.3f)
                    : new Vector3(1.2f, 0.7f, 1.2f);
                AssignMaterial(
                    unitObject,
                    unit.Id,
                    unit.IsPlayerUnit ? new Color(0.1f, 0.65f, 0.86f) : new Color(0.74f, 0.20f, 0.18f));

                DemoUnitView view = unitObject.AddComponent<DemoUnitView>();
                view.Bind(unit);
                if (unit.IsPlayerUnit)
                {
                    view.SetPresentationOffset(PlayerPresentationOffset(presentationIndex));
                }

                unitObject.SetActive(unit.IsActive || unit.IsPlayerUnit || unit.IsDestroyed);
                unitViews[unit.Id] = view;
            }
        }

        private static Vector3 PlayerPresentationOffset(int index)
        {
            switch (index)
            {
                case 0:
                    return new Vector3(0f, 0f, 1.35f);
                case 1:
                    return new Vector3(-2.25f, 0f, -0.65f);
                case 2:
                    return new Vector3(2.25f, 0f, -0.65f);
                case 3:
                    return new Vector3(0f, 0f, -2.45f);
                case 4:
                    return new Vector3(-3.25f, 0f, 1.25f);
                case 5:
                    return new Vector3(3.25f, 0f, 1.25f);
                default:
                    return Vector3.zero;
            }
        }

        private void CreateStaticObjects()
        {
            int referenceStructures = 0;
            int fallbackStructures = 0;
            foreach (StructureState structure in mission.Structures)
            {
                GameObject structureObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                structureObject.name = structure.Id + " " + structure.ObjectType;
                structureObject.transform.localScale = StructureScale(structure);
                structureObject.transform.rotation = Quaternion.Euler(0f, -structure.RotationDegrees, 0f);
                Color structureColor = structure.IsTargetable ? new Color(0.62f, 0.48f, 0.34f) : new Color(0.32f, 0.34f, 0.34f);
                AssignMaterial(structureObject, structure.Id, structureColor);
                Renderer rootRenderer = structureObject.GetComponent<Renderer>();
                Renderer visualRenderer = null;
                if (ReferencePropLibrary.TryAttachStructure(structure, structureObject.transform, structureColor, out Renderer referenceRenderer))
                {
                    referenceStructures++;
                    visualRenderer = referenceRenderer;
                    if (rootRenderer != null)
                    {
                        rootRenderer.enabled = false;
                    }
                }
                else
                {
                    fallbackStructures++;
                }

                DemoStructureView view = structureObject.AddComponent<DemoStructureView>();
                view.Bind(structure, visualRenderer);
                structureViews[structure.Id] = view;
            }

            lastReferenceStructureSummary = "ReferenceStructures=loaded "
                + referenceStructures.ToString(CultureInfo.InvariantCulture)
                + " fallback "
                + fallbackStructures.ToString(CultureInfo.InvariantCulture);
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

            int referenceProps = 0;
            int fallbackProps = 0;
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

                Color propColor = TerrainObjectColor(terrainObject);
                AssignMaterial(prop, TerrainMaterialName(terrainObject), propColor);
                Renderer rootRenderer = prop.GetComponent<Renderer>();
                if (ReferencePropLibrary.TryAttachTerrainObject(terrainObject, prop.transform, propColor, out _))
                {
                    referenceProps++;
                    if (rootRenderer != null)
                    {
                        rootRenderer.enabled = false;
                    }
                }
                else
                {
                    fallbackProps++;
                }
            }

            lastReferencePropSummary = ReferencePropLibrary.Summary(referenceProps, fallbackProps);
            Debug.Log("MC2 reference prop visuals: " + lastReferencePropSummary);
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

            activeObjectiveGuideMarker = CreateMarkerDisc(
                "Active Objective Guide",
                "ObjectiveGuideActive",
                new Color(1f, 0.82f, 0.24f, 0.30f),
                new Vector3(1.05f, 0.014f, 1.05f));
            activeObjectiveGuideBeacon = CreateMarkerDisc(
                "Active Objective Beacon",
                "ObjectiveGuideBeacon",
                new Color(1f, 0.88f, 0.32f, 0.58f),
                new Vector3(0.06f, 0.42f, 0.06f));
            activeObjectiveRouteLine = CreateTargetLine("Active Objective Route");
            activeObjectiveRouteStartMarker = CreateMarkerDisc(
                "Active Objective Route Start",
                "ObjectiveRouteStart",
                new Color(1f, 0.86f, 0.30f, 0.32f),
                new Vector3(0.34f, 0.014f, 0.34f));
            activeObjectiveRouteEndMarker = CreateMarkerDisc(
                "Active Objective Route End",
                "ObjectiveRouteEnd",
                new Color(1f, 0.86f, 0.30f, 0.38f),
                new Vector3(0.46f, 0.014f, 0.46f));
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
            ObjectiveState activeObjective = null;
            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (!objectiveAreaMarkers.TryGetValue(objective.Definition.index, out List<GameObject> markers))
                {
                    if (ShouldShowCurrentObjectiveHint(objective))
                    {
                        activeObjective ??= objective;
                    }

                    continue;
                }

                bool visible = ShouldShowCurrentObjectiveHint(objective);
                if (visible)
                {
                    activeObjective ??= objective;
                }

                foreach (GameObject marker in markers)
                {
                    if (marker != null)
                    {
                        marker.SetActive(visible);
                    }
                }
            }

            UpdateActiveObjectiveGuide(activeObjective);
        }

        private void UpdateActiveObjectiveGuide(ObjectiveState activeObjective)
        {
            Vector2 missionPoint = default;
            float missionRadius = 0f;
            bool visible = activeObjective != null
                && mission != null
                && mission.Result == MissionResultState.InProgress
                && TryGetObjectiveCuePoint(activeObjective.Definition, out missionPoint, out missionRadius);
            if (activeObjectiveGuideMarker != null)
            {
                activeObjectiveGuideMarker.SetActive(visible);
            }

            if (activeObjectiveGuideBeacon != null)
            {
                activeObjectiveGuideBeacon.SetActive(visible);
            }

            UnitState commander = FirstPlayerUnit();
            bool routeVisible = visible
                && commander != null
                && commander.IsActive
                && !commander.IsDestroyed;
            if (activeObjectiveRouteLine != null)
            {
                activeObjectiveRouteLine.SetActive(routeVisible);
            }

            if (activeObjectiveRouteStartMarker != null)
            {
                activeObjectiveRouteStartMarker.SetActive(routeVisible);
            }

            if (activeObjectiveRouteEndMarker != null)
            {
                activeObjectiveRouteEndMarker.SetActive(routeVisible);
            }

            if (!visible)
            {
                return;
            }

            float pulse = 0.98f + Mathf.Sin(Time.time * 3.4f) * 0.06f;
            float pressure = ObjectiveTargetPressure(activeObjective);
            float scale = Mathf.Clamp(missionRadius / 165f, 0.84f, 2.05f) * pulse * Mathf.Lerp(1f, 1.16f, pressure);
            Color color = ObjectivePressureColor(pressure, 0.34f);
            if (activeObjectiveGuideMarker != null)
            {
                activeObjectiveGuideMarker.transform.position = GroundMarkerPosition(missionPoint, 0.085f);
                activeObjectiveGuideMarker.transform.localScale = new Vector3(0.82f * scale, 0.014f, 0.82f * scale);
                AssignMaterial(activeObjectiveGuideMarker, "ObjectiveGuideActive", color);
            }

            if (activeObjectiveGuideBeacon != null)
            {
                activeObjectiveGuideBeacon.transform.position = GroundMarkerPosition(missionPoint, 0.52f);
                float beaconScale = Mathf.Clamp(0.055f * scale, 0.045f, 0.095f);
                activeObjectiveGuideBeacon.transform.localScale = new Vector3(beaconScale, 0.42f * pulse, beaconScale);
                AssignMaterial(activeObjectiveGuideBeacon, "ObjectiveGuideBeacon", ObjectivePressureColor(pressure, 0.56f));
            }

            if (routeVisible && activeObjectiveRouteLine != null)
            {
                Vector3 commanderPoint = GroundMarkerPosition(commander.MissionPosition, 0.20f);
                Vector3 targetPoint = GroundMarkerPosition(missionPoint, 0.20f);
                PositionLine(activeObjectiveRouteLine, commanderPoint, targetPoint, 0.014f);
                AssignMaterial(activeObjectiveRouteLine, "ObjectiveRoute", ObjectivePressureColor(pressure, 0.36f));
                Color routeCapColor = ObjectivePressureColor(pressure, 0.42f);
                if (activeObjectiveRouteStartMarker != null)
                {
                    activeObjectiveRouteStartMarker.transform.position = GroundMarkerPosition(commander.MissionPosition, 0.092f);
                    activeObjectiveRouteStartMarker.transform.localScale = new Vector3(0.30f * pulse, 0.014f, 0.30f * pulse);
                    AssignMaterial(activeObjectiveRouteStartMarker, "ObjectiveRouteStart", routeCapColor);
                }

                if (activeObjectiveRouteEndMarker != null)
                {
                    float endScale = Mathf.Lerp(0.38f, 0.54f, pressure) * pulse;
                    activeObjectiveRouteEndMarker.transform.position = GroundMarkerPosition(missionPoint, 0.105f);
                    activeObjectiveRouteEndMarker.transform.localScale = new Vector3(endScale, 0.014f, endScale);
                    AssignMaterial(activeObjectiveRouteEndMarker, "ObjectiveRouteEnd", ObjectivePressureColor(pressure, 0.48f));
                }
            }
        }

        private float ObjectiveTargetPressure(ObjectiveState activeObjective)
        {
            StructureState target = ActiveObjectiveTargetStructure(activeObjective);
            return target == null ? 0f : 1f - Mathf.Clamp01(target.Structure);
        }

        private StructureState ActiveObjectiveTargetStructure(ObjectiveState activeObjective)
        {
            if (activeObjective?.Definition?.conditions == null || mission == null)
            {
                return null;
            }

            foreach (ObjectiveCondition condition in activeObjective.Definition.conditions)
            {
                if (condition?.targetStructure?.position == null)
                {
                    continue;
                }

                Vector2 targetPoint = new(condition.targetStructure.position.x, condition.targetStructure.position.y);
                StructureState best = null;
                float bestDistance = float.MaxValue;
                foreach (StructureState structure in mission.Structures)
                {
                    if (structure == null || !structure.IsTargetable)
                    {
                        continue;
                    }

                    float distance = Vector2.Distance(structure.MissionPosition, targetPoint);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        best = structure;
                    }
                }

                if (best != null && bestDistance <= Mathf.Max(220f, best.Radius + 80f))
                {
                    return best;
                }
            }

            return null;
        }

        private static Color ObjectivePressureColor(float pressure, float alpha)
        {
            float clamped = Mathf.Clamp01(pressure);
            if (clamped >= 0.68f)
            {
                return new Color(1f, 0.26f, 0.12f, alpha);
            }

            if (clamped >= 0.34f)
            {
                return new Color(1f, 0.58f, 0.16f, alpha);
            }

            return new Color(1f, 0.82f, 0.24f, alpha);
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
                unitSoloOrderBeacons[unit.Id] = CreateMarkerDisc(
                    unit.Id + " Solo Order Beacon",
                    "SoloOrderBeacon",
                    new Color(1f, 0.62f, 0.18f, 0.52f),
                    new Vector3(0.072f, 0.36f, 0.072f));
                unitOrderMarkers[unit.Id] = CreateMarkerDisc(
                    unit.Id + " Order Marker",
                    "CommandMove",
                    new Color(0.15f, 0.66f, 1f, 0.42f),
                    new Vector3(0.88f, 0.014f, 0.88f));
                unitOrderLines[unit.Id] = CreateTargetLine(unit.Id + " Order Path");
                unitFocusMarkers[unit.Id] = CreateMarkerDisc(
                    unit.Id + " Focus Marker",
                    "CommandFocus",
                    new Color(1f, 0.58f, 0.12f, 0.48f),
                    new Vector3(1.18f, 0.016f, 1.18f));
                unitDamageWarningMarkers[unit.Id] = CreateMarkerDisc(
                    unit.Id + " Damage Warning",
                    "PlayerDamageWarning",
                    new Color(1f, 0.44f, 0.10f, 0.36f),
                    new Vector3(1.16f, 0.018f, 1.16f));
                unitDamageWarningBeacons[unit.Id] = CreateMarkerDisc(
                    unit.Id + " Damage Warning Beacon",
                    "PlayerDamageBeacon",
                    new Color(1f, 0.34f, 0.08f, 0.50f),
                    new Vector3(0.078f, 0.36f, 0.078f));
                unitRangeMarkers[unit.Id] = CreateMarkerDisc(
                    unit.Id + " Weapon Range",
                    "CommandWeaponRange",
                    new Color(0.25f, 0.82f, 1f, 0.12f),
                    Vector3.one);
                unitWeaponReadinessMarkers[unit.Id] = CreateMarkerDisc(
                    unit.Id + " Weapon Readiness",
                    "WeaponReadinessReady",
                    new Color(0.34f, 1f, 0.62f, 0.26f),
                    new Vector3(0.84f, 0.012f, 0.84f));
                unitWeaponReadinessBeacons[unit.Id] = CreateMarkerDisc(
                    unit.Id + " Weapon Readiness Beacon",
                    "WeaponReadinessBeaconReady",
                    new Color(0.34f, 1f, 0.62f, 0.42f),
                    new Vector3(0.062f, 0.24f, 0.062f));
                unitCommanderAnchors[unit.Id] = CreateMarkerDisc(
                    unit.Id + " Commander Anchor",
                    "CommanderAnchor",
                    new Color(1f, 0.78f, 0.18f, 0.34f),
                    new Vector3(1.08f, 0.018f, 1.08f));
                unitCommanderBeacons[unit.Id] = CreateMarkerDisc(
                    unit.Id + " Commander Beacon",
                    "CommanderBeacon",
                    new Color(1f, 0.84f, 0.26f, 0.70f),
                    new Vector3(0.085f, 0.42f, 0.085f));
                unitTargetLines[unit.Id] = CreateTargetLine(unit.Id + " Target Line");
                unitTargetLineEndMarkers[unit.Id] = CreateMarkerDisc(
                    unit.Id + " Target Line End",
                    "TargetLineEndReady",
                    new Color(0.28f, 0.78f, 1f, 0.34f),
                    new Vector3(0.36f, 0.014f, 0.36f));
            }

            squadFocusMarker = CreateMarkerDisc(
                "Squad Focus Pressure",
                "SquadFocusPressure",
                new Color(1f, 0.72f, 0.18f, 0.34f),
                new Vector3(1.40f, 0.018f, 1.40f));
            squadFocusBeacon = CreateMarkerDisc(
                "Squad Focus Beacon",
                "SquadFocusBeacon",
                new Color(1f, 0.76f, 0.18f, 0.50f),
                new Vector3(0.080f, 0.38f, 0.080f));
            hostileFocusMarker = CreateMarkerDisc(
                "Hostile Focus Warning",
                "HostileFocusWarning",
                new Color(1f, 0.22f, 0.10f, 0.38f),
                new Vector3(1.34f, 0.020f, 1.34f));
            hostileFocusBeacon = CreateMarkerDisc(
                "Hostile Focus Beacon",
                "HostileFocusBeacon",
                new Color(1f, 0.16f, 0.08f, 0.58f),
                new Vector3(0.095f, 0.46f, 0.095f));
            combatPressureMarker = CreateMarkerDisc(
                "Combat Pressure Ring",
                "CombatPressureTracking",
                new Color(0.44f, 0.84f, 1f, 0.20f),
                new Vector3(1.72f, 0.014f, 1.72f));
            combatPressureBeacon = CreateMarkerDisc(
                "Combat Pressure Beacon",
                "CombatPressureBeacon",
                new Color(1f, 0.58f, 0.14f, 0.42f),
                new Vector3(0.070f, 0.34f, 0.070f));
            hostilePressureMarker = CreateMarkerDisc(
                "Hostile Pressure Center",
                "HostilePressureTracking",
                new Color(1f, 0.42f, 0.12f, 0.22f),
                new Vector3(1.54f, 0.016f, 1.54f));
            hostilePressureBeacon = CreateMarkerDisc(
                "Hostile Pressure Beacon",
                "HostilePressureBeacon",
                new Color(1f, 0.34f, 0.10f, 0.44f),
                new Vector3(0.075f, 0.36f, 0.075f));
            hostilePressureVectorLine = CreateTargetLine("Hostile Pressure Vector");
            hostilePressureVectorEndMarker = CreateMarkerDisc(
                "Hostile Pressure Vector End",
                "HostilePressureVectorEnd",
                new Color(1f, 0.46f, 0.12f, 0.38f),
                new Vector3(0.32f, 0.014f, 0.32f));
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

            UnitState commander = FirstPlayerUnit();
            foreach (UnitState unit in mission.PlayerUnits())
            {
                UpdateSoloReturnCue(unit);
                UpdateOrderArrivalCue(unit);
                UpdateCommanderMarker(unit, commander);
                UpdateSelectionMarker(unit);
                UpdateOrderMarker(unit);
                UpdateOrderPathLine(unit);
                UpdateFocusMarker(unit);
                UpdatePlayerDamageWarningMarker(unit);
                UpdateRangeMarker(unit);
                UpdateWeaponReadinessMarker(unit);
                UpdateTargetLine(unit);
            }

            UpdateSquadFocusMarker();
            UpdateHostileFocusMarker();
            UpdateCombatPressureMarker(commander);
            UpdateHostilePressureMarker(commander);
            UpdateTargetHealthBars();
        }

        private void UpdateOrderArrivalCue(UnitState unit)
        {
            if (unit == null || string.IsNullOrEmpty(unit.Id))
            {
                return;
            }

            bool wasMoving = movingUnitIdsLastFrame.Contains(unit.Id);
            bool wasJumping = jumpingUnitIdsLastFrame.Contains(unit.Id);
            bool isMoving = unit.IsActive && !unit.IsDestroyed && (unit.HasMoveOrder || unit.IsJumping);
            if (wasMoving && !isMoving && unit.IsActive && !unit.IsDestroyed)
            {
                SpawnOrderArrivalCue(unit, wasJumping, !wasJumping && unit.HasAttackOrder);
            }

            if (isMoving)
            {
                movingUnitIdsLastFrame.Add(unit.Id);
                if (unit.IsJumping)
                {
                    jumpingUnitIdsLastFrame.Add(unit.Id);
                }
                else
                {
                    jumpingUnitIdsLastFrame.Remove(unit.Id);
                }
            }
            else
            {
                movingUnitIdsLastFrame.Remove(unit.Id);
                jumpingUnitIdsLastFrame.Remove(unit.Id);
            }
        }

        private void SpawnOrderArrivalCue(UnitState unit, bool wasJumping, bool attackReady)
        {
            Vector3 center = GroundMarkerPosition(unit.MissionPosition, wasJumping ? 0.17f : attackReady ? 0.16f : 0.13f);
            Color color = wasJumping
                ? new Color(0.56f, 0.96f, 1f, 0.68f)
                : attackReady
                    ? new Color(1f, 0.56f, 0.14f, 0.68f)
                    : new Color(0.30f, 0.82f, 1f, 0.58f);
            float scale = wasJumping ? 1.18f : attackReady ? 1.06f : 0.94f;
            CreateImpactDisc("Order Arrival Pulse", center, new Color(color.r, color.g, color.b, 0.30f), 0.48f, 0.20f * scale, 0.92f * scale, 0.020f);
            CreateBeam(
                center + Vector3.up * 0.04f,
                center + Vector3.up * (wasJumping ? 0.72f : attackReady ? 0.64f : 0.48f),
                new Color(color.r, color.g, color.b, 0.42f),
                0.34f,
                wasJumping ? 0.022f : attackReady ? 0.020f : 0.016f);
        }

        private void UpdateSoloReturnCue(UnitState unit)
        {
            if (unit == null || string.IsNullOrEmpty(unit.Id))
            {
                return;
            }

            bool wasDetached = detachedUnitIdsLastFrame.Contains(unit.Id);
            bool isDetached = unit.IsActive && !unit.IsDestroyed && unit.IsDetached;
            if (wasDetached && !isDetached && unit.IsActive && !unit.IsDestroyed)
            {
                SpawnSoloReturnCue(unit);
            }

            if (isDetached)
            {
                detachedUnitIdsLastFrame.Add(unit.Id);
            }
            else
            {
                detachedUnitIdsLastFrame.Remove(unit.Id);
            }
        }

        private void SpawnSoloReturnCue(UnitState unit)
        {
            Vector3 center = GroundMarkerPosition(unit.MissionPosition, 0.16f);
            Color color = new(0.34f, 1f, 0.62f, 0.70f);
            CreateImpactDisc("Solo Return Pulse", center, new Color(color.r, color.g, color.b, 0.32f), 0.58f, 0.24f, 1.04f, 0.024f);
            CreateBeam(center + Vector3.up * 0.06f, center + Vector3.up * 0.78f, new Color(color.r, color.g, color.b, 0.48f), 0.44f, 0.022f);
            CreateImpact(center + Vector3.up * 0.14f, new Color(color.r, color.g, color.b, 0.42f), false, 0.34f);
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

        private void UpdateCommanderMarker(UnitState unit, UnitState commander)
        {
            bool hasAnchor = unitCommanderAnchors.TryGetValue(unit.Id, out GameObject anchor);
            bool hasBeacon = unitCommanderBeacons.TryGetValue(unit.Id, out GameObject beacon);
            if (!hasAnchor && !hasBeacon)
            {
                return;
            }

            bool isVisible = commander != null
                && string.Equals(unit.Id, commander.Id, StringComparison.OrdinalIgnoreCase)
                && unit.IsActive
                && !unit.IsDestroyed;
            if (hasAnchor)
            {
                anchor.SetActive(isVisible);
            }

            if (hasBeacon)
            {
                beacon.SetActive(isVisible);
            }

            if (!isVisible)
            {
                return;
            }

            float pulse = 0.92f + Mathf.Sin(Time.time * 3.8f) * 0.08f;
            if (hasAnchor)
            {
                anchor.transform.position = GroundMarkerPosition(unit.MissionPosition, 0.075f);
                anchor.transform.localScale = new Vector3(1.08f * pulse, 0.018f, 1.08f * pulse);
            }

            if (hasBeacon)
            {
                beacon.transform.position = CommanderBeaconPosition(unit);
                beacon.transform.localScale = new Vector3(0.085f * pulse, 0.40f + pulse * 0.08f, 0.085f * pulse);
            }
        }

        private void UpdateSelectionMarker(UnitState unit)
        {
            bool hasMarker = unitSelectionMarkers.TryGetValue(unit.Id, out GameObject marker);
            bool hasBeacon = unitSoloOrderBeacons.TryGetValue(unit.Id, out GameObject beacon);
            if (!hasMarker && !hasBeacon)
            {
                return;
            }

            bool isPending = pendingDetachedUnitId == unit.Id;
            bool isVisible = unit.IsActive && !unit.IsDestroyed && (unit.IsDetached || isPending);
            if (hasMarker)
            {
                marker.SetActive(isVisible);
            }

            if (hasBeacon)
            {
                beacon.SetActive(isVisible);
            }

            if (!isVisible)
            {
                return;
            }

            float pulse = isPending
                ? 0.94f + Mathf.Sin(Time.time * 5.2f) * 0.09f
                : 0.96f + Mathf.Sin(Time.time * 4.0f) * 0.06f;
            Color color = isPending
                ? new Color(0.30f, 0.92f, 1f, 0.44f)
                : new Color(1f, 0.60f, 0.16f, 0.48f);
            if (hasMarker)
            {
                float scale = (isPending ? 1.52f : 1.34f) * pulse;
                marker.transform.position = GroundMarkerPosition(unit.MissionPosition, 0.06f);
                marker.transform.localScale = new Vector3(scale, 0.012f, scale);
                AssignMaterial(marker, isPending ? "SoloOrderPending" : "SoloOrderActive", color);
            }

            if (hasBeacon)
            {
                beacon.transform.position = GroundMarkerPosition(unit.MissionPosition, isPending ? 0.50f : 0.44f);
                beacon.transform.localScale = new Vector3(0.072f * pulse, (isPending ? 0.42f : 0.34f) + pulse * 0.055f, 0.072f * pulse);
                AssignMaterial(
                    beacon,
                    isPending ? "SoloOrderBeaconPending" : "SoloOrderBeaconActive",
                    isPending ? new Color(0.30f, 0.92f, 1f, 0.56f) : new Color(1f, 0.60f, 0.16f, 0.54f));
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
                bool isJet = unit.IsJumping;
                bool isAttack = !isJet && unit.HasAttackOrder;
                float pulse = isJet
                    ? 0.96f + Mathf.Sin(Time.time * 5.4f) * 0.10f
                    : isAttack
                        ? 0.98f + Mathf.Sin(Time.time * 4.2f) * 0.07f
                        : 1f + Mathf.Sin(Time.time * 3.1f) * 0.04f;
                float scale = (isJet ? 1.04f : isAttack ? 0.96f : 0.84f) * pulse;
                marker.transform.position = GroundMarkerPosition(unit.MoveTarget, 0.08f);
                marker.transform.localScale = new Vector3(scale, 0.014f, scale);
                AssignMaterial(
                    marker,
                    isJet ? "CommandJetEndcap" : isAttack ? "CommandAttackEndcap" : "CommandMoveEndcap",
                    isJet
                        ? new Color(0.58f, 0.96f, 1f, 0.50f)
                        : isAttack
                            ? new Color(1f, 0.54f, 0.12f, 0.46f)
                            : new Color(0.18f, 0.70f, 1f, 0.42f));
            }
        }

        private void UpdateOrderPathLine(UnitState unit)
        {
            if (!unitOrderLines.TryGetValue(unit.Id, out GameObject line))
            {
                return;
            }

            bool hasUnitView = unitViews.TryGetValue(unit.Id, out DemoUnitView unitView) && unitView != null;
            bool isVisible = unit.IsActive
                && !unit.IsDestroyed
                && unit.HasMoveOrder
                && hasUnitView;
            line.SetActive(isVisible);
            if (!isVisible)
            {
                return;
            }

            Vector3 unitPoint = unitView.transform.position + Vector3.up * 0.14f;
            Vector3 targetPoint = GroundMarkerPosition(unit.MoveTarget, unit.IsJumping ? 0.24f : 0.14f);
            bool isAttack = !unit.IsJumping && unit.HasAttackOrder;
            PositionLine(line, unitPoint, targetPoint, unit.IsJumping ? 0.034f : isAttack ? 0.030f : 0.024f);
            AssignMaterial(
                line,
                unit.IsJumping ? "OrderPathJet" : isAttack ? "OrderPathAttack" : "OrderPathMove",
                unit.IsJumping
                    ? new Color(0.64f, 0.96f, 1f, 0.46f)
                    : isAttack
                        ? new Color(1f, 0.52f, 0.12f, 0.44f)
                        : new Color(0.24f, 0.72f, 1f, 0.34f));
        }

        private void UpdateFocusMarker(UnitState unit)
        {
            if (!unitFocusMarkers.TryGetValue(unit.Id, out GameObject marker))
            {
                return;
            }

            Vector2 position = default;
            float radius = 0f;
            bool isVisible = unit.IsActive
                && !unit.IsDestroyed
                && unit.HasAttackOrder
                && TryGetTargetMarker(unit.AttackTargetId, out position, out radius);
            marker.SetActive(isVisible);
            if (isVisible)
            {
                string state = CommandFocusStateKey(unit, unit.AttackTargetId);
                float pulse = 0.96f + Mathf.Sin(Time.time * CommandFocusPulseSpeed(state)) * CommandFocusPulseRange(state);
                marker.transform.position = GroundMarkerPosition(position, 0.1f);
                float scale = Mathf.Clamp(radius / 95f, 1.1f, 4.2f) * CommandFocusScale(state) * pulse;
                marker.transform.localScale = new Vector3(scale, 0.016f, scale);
                AssignMaterial(marker, "CommandFocus" + state, CommandFocusColor(state));
            }
        }

        private string CommandFocusStateKey(UnitState unit, string targetId)
        {
            if (unit == null || string.IsNullOrEmpty(targetId))
            {
                return "Tracking";
            }

            if (!IsTargetInWeaponRange(unit, targetId))
            {
                return "Closing";
            }

            if (unit.IsHeatLocked)
            {
                return "Hot";
            }

            if (unit.IsWeaponCoolingDown)
            {
                return "Cycling";
            }

            return "Ready";
        }

        private static Color CommandFocusColor(string state)
        {
            if (string.Equals(state, "Ready", StringComparison.Ordinal))
            {
                return new Color(0.34f, 1f, 0.62f, 0.42f);
            }

            if (string.Equals(state, "Hot", StringComparison.Ordinal))
            {
                return new Color(1f, 0.18f, 0.10f, 0.48f);
            }

            if (string.Equals(state, "Cycling", StringComparison.Ordinal))
            {
                return new Color(1f, 0.68f, 0.16f, 0.40f);
            }

            return new Color(1f, 0.48f, 0.12f, 0.42f);
        }

        private static float CommandFocusScale(string state)
        {
            if (string.Equals(state, "Hot", StringComparison.Ordinal))
            {
                return 1.08f;
            }

            if (string.Equals(state, "Ready", StringComparison.Ordinal))
            {
                return 1.02f;
            }

            return 0.96f;
        }

        private static float CommandFocusPulseSpeed(string state)
        {
            if (string.Equals(state, "Hot", StringComparison.Ordinal))
            {
                return 6.4f;
            }

            if (string.Equals(state, "Cycling", StringComparison.Ordinal))
            {
                return 4.8f;
            }

            return 3.8f;
        }

        private static float CommandFocusPulseRange(string state)
        {
            return string.Equals(state, "Hot", StringComparison.Ordinal) ? 0.10f : 0.06f;
        }

        private void UpdatePlayerDamageWarningMarker(UnitState unit)
        {
            bool hasMarker = unitDamageWarningMarkers.TryGetValue(unit.Id, out GameObject marker);
            bool hasBeacon = unitDamageWarningBeacons.TryGetValue(unit.Id, out GameObject beacon);
            if (!hasMarker && !hasBeacon)
            {
                return;
            }

            bool hasSectionPenalty = unit.MobilityRatio < 0.99f || unit.FirepowerRatio < 0.99f;
            bool isVisible = unit.IsActive
                && !unit.IsDestroyed
                && (unit.Structure < 0.82f || hasSectionPenalty);
            if (hasMarker)
            {
                marker.SetActive(isVisible);
            }

            if (hasBeacon)
            {
                beacon.SetActive(isVisible);
            }

            if (!isVisible)
            {
                return;
            }

            bool isCritical = unit.Structure <= 0.45f
                || unit.MobilityRatio <= 0.55f
                || unit.FirepowerRatio <= 0.55f;
            float pulseSpeed = isCritical ? 7.2f : 4.8f;
            float pulse = 0.96f + Mathf.Sin(Time.time * pulseSpeed) * (isCritical ? 0.10f : 0.06f);
            if (hasMarker)
            {
                float scale = (isCritical ? 1.34f : 1.14f) * pulse;
                marker.transform.position = GroundMarkerPosition(unit.MissionPosition, 0.115f);
                marker.transform.localScale = new Vector3(scale, 0.018f, scale);
                AssignMaterial(
                    marker,
                    isCritical ? "PlayerDamageCritical" : "PlayerDamageWarning",
                    isCritical ? new Color(1f, 0.12f, 0.08f, 0.48f) : new Color(1f, 0.46f, 0.10f, 0.34f));
            }

            if (hasBeacon)
            {
                beacon.transform.position = GroundMarkerPosition(unit.MissionPosition, isCritical ? 0.58f : 0.48f);
                beacon.transform.localScale = new Vector3(
                    0.078f * pulse,
                    (isCritical ? 0.48f : 0.32f) + pulse * 0.08f,
                    0.078f * pulse);
                AssignMaterial(
                    beacon,
                    isCritical ? "PlayerDamageBeaconCritical" : "PlayerDamageBeaconWarning",
                    isCritical ? new Color(1f, 0.08f, 0.06f, 0.54f) : new Color(1f, 0.42f, 0.10f, 0.42f));
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
                bool isSelectionRange = pendingDetachedUnitId == unit.Id || unit.IsDetached;
                bool isTargetingRange = unit.HasAttackOrder || !string.IsNullOrEmpty(unit.CurrentTargetId);
                bool isHeatRange = unit.IsHeatLocked;
                float pulse = isHeatRange
                    ? 0.97f + Mathf.Sin(Time.time * 5.8f) * 0.06f
                    : isTargetingRange
                        ? 0.99f + Mathf.Sin(Time.time * 3.6f) * 0.035f
                        : 1f;
                Color color = isHeatRange
                    ? new Color(1f, 0.18f, 0.12f, 0.18f)
                    : isTargetingRange
                        ? new Color(1f, 0.62f, 0.14f, 0.16f)
                        : isSelectionRange
                            ? new Color(0.25f, 0.82f, 1f, 0.13f)
                            : new Color(0.25f, 0.82f, 1f, 0.10f);
                marker.transform.position = GroundMarkerPosition(unit.MissionPosition, 0.045f);
                float worldDiameter = Mathf.Clamp((unit.CombatWeaponRange / 100f) * 2f, 1.2f, 8f);
                marker.transform.localScale = new Vector3(worldDiameter * pulse, 0.01f, worldDiameter * pulse);
                AssignMaterial(
                    marker,
                    isHeatRange ? "CommandWeaponRangeHeat" : isTargetingRange ? "CommandWeaponRangeTarget" : "CommandWeaponRangeSelection",
                    color);
            }
        }

        private void UpdateWeaponReadinessMarker(UnitState unit)
        {
            bool hasMarker = unitWeaponReadinessMarkers.TryGetValue(unit.Id, out GameObject marker);
            bool hasBeacon = unitWeaponReadinessBeacons.TryGetValue(unit.Id, out GameObject beacon);
            if (!hasMarker && !hasBeacon)
            {
                return;
            }

            string targetId = string.IsNullOrEmpty(unit.AttackTargetId) ? unit.CurrentTargetId : unit.AttackTargetId;
            bool hasTarget = !string.IsNullOrEmpty(targetId);
            bool isVisible = unit.IsActive
                && !unit.IsDestroyed
                && hasTarget
                && unit.CombatWeaponRange > 0f;
            if (hasMarker)
            {
                marker.SetActive(isVisible);
            }

            if (hasBeacon)
            {
                beacon.SetActive(isVisible);
            }

            if (!isVisible)
            {
                return;
            }

            bool blocked = unit.IsHeatLocked || !IsTargetInWeaponRange(unit, targetId);
            bool cooling = !blocked && unit.IsWeaponCoolingDown;
            float readiness = Mathf.Clamp01(unit.WeaponReadinessRatio);
            float pulse = blocked
                ? 0.96f + Mathf.Sin(Time.time * 6.2f) * 0.10f
                    : cooling
                        ? 0.92f + Mathf.Sin(Time.time * 4.8f) * 0.08f
                        : 0.98f + Mathf.Sin(Time.time * 3.2f) * 0.04f;
            string materialName = blocked ? "WeaponReadinessBlocked" : cooling ? "WeaponReadinessCooling" : "WeaponReadinessReady";
            Color color = blocked
                ? new Color(1f, 0.16f, 0.10f, 0.36f)
                : cooling
                    ? new Color(1f, 0.62f, 0.14f, 0.30f)
                    : new Color(0.34f, 1f, 0.62f, 0.26f);
            if (hasMarker)
            {
                float baseScale = blocked ? 1.02f : cooling ? Mathf.Lerp(0.72f, 0.96f, readiness) : 0.88f;
                marker.transform.position = GroundMarkerPosition(unit.MissionPosition, 0.095f);
                marker.transform.localScale = new Vector3(baseScale * pulse, 0.012f, baseScale * pulse);
                AssignMaterial(marker, materialName, color);
            }

            if (hasBeacon)
            {
                float beaconHeight = blocked ? 0.38f : cooling ? Mathf.Lerp(0.18f, 0.32f, readiness) : 0.24f;
                beacon.transform.position = GroundMarkerPosition(unit.MissionPosition, blocked ? 0.47f : 0.39f);
                beacon.transform.localScale = new Vector3(0.062f * pulse, beaconHeight + pulse * 0.045f, 0.062f * pulse);
                AssignMaterial(
                    beacon,
                    materialName + "Beacon",
                    blocked
                        ? new Color(1f, 0.12f, 0.08f, 0.48f)
                        : cooling
                            ? new Color(1f, 0.58f, 0.10f, 0.40f)
                            : new Color(0.34f, 1f, 0.62f, 0.36f));
            }
        }

        private void UpdateTargetLine(UnitState unit)
        {
            bool hasLine = unitTargetLines.TryGetValue(unit.Id, out GameObject line);
            bool hasEndMarker = unitTargetLineEndMarkers.TryGetValue(unit.Id, out GameObject endMarker);
            if (!hasLine && !hasEndMarker)
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
            if (hasLine)
            {
                line.SetActive(isVisible);
            }

            if (hasEndMarker)
            {
                endMarker.SetActive(isVisible);
            }

            if (!isVisible)
            {
                lastTargetLockByUnit.Remove(unit.Id);
                return;
            }

            Vector3 unitPoint = unitView.transform.position + Vector3.up * 0.18f;
            SpawnTargetLockCueIfChanged(unit, targetId, targetPoint);
            Vector3 lineTargetPoint = targetPoint + Vector3.up * 0.18f;
            string materialName = TargetLineMaterialName(unit, targetId);
            Color color = TargetLineColor(unit, targetId);
            if (hasLine)
            {
                PositionLine(line, unitPoint, lineTargetPoint, TargetLineRadius(unit, targetId));
                AssignMaterial(line, materialName, color);
            }

            if (hasEndMarker)
            {
                bool blocked = unit.IsHeatLocked || !IsTargetInWeaponRange(unit, targetId);
                bool cooling = !blocked && unit.IsWeaponCoolingDown;
                float pulse = blocked
                    ? 0.98f + Mathf.Sin(Time.time * 6.4f) * 0.12f
                    : cooling
                        ? 0.94f + Mathf.Sin(Time.time * 4.6f) * 0.08f
                        : 1f + Mathf.Sin(Time.time * 3.0f) * 0.04f;
                float scale = blocked ? 0.46f : cooling ? 0.38f : 0.34f;
                endMarker.transform.position = targetPoint + Vector3.up * 0.105f;
                endMarker.transform.localScale = new Vector3(scale * pulse, 0.014f, scale * pulse);
                AssignMaterial(
                    endMarker,
                    materialName + "End",
                    new Color(color.r, color.g, color.b, blocked ? 0.42f : cooling ? 0.36f : 0.32f));
            }
        }

        private void SpawnTargetLockCueIfChanged(UnitState unit, string targetId, Vector3 targetPoint)
        {
            if (unit == null || string.IsNullOrEmpty(unit.Id) || string.IsNullOrEmpty(targetId))
            {
                return;
            }

            if (lastTargetLockByUnit.TryGetValue(unit.Id, out string previousTargetId)
                && string.Equals(previousTargetId, targetId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            lastTargetLockByUnit[unit.Id] = targetId;
            CreateTargetLockCue(targetPoint, unit.HasAttackOrder);
        }

        private void CreateTargetLockCue(Vector3 targetPoint, bool commanded)
        {
            Color color = commanded
                ? new Color(1f, 0.62f, 0.16f, 0.58f)
                : new Color(0.30f, 0.92f, 1f, 0.50f);
            float scale = commanded ? 1.08f : 0.88f;
            Vector3 center = targetPoint + Vector3.up * 0.18f;
            CreateImpactDisc(
                "Target Lock Ring",
                center,
                new Color(color.r, color.g, color.b, 0.30f),
                0.34f,
                0.20f * scale,
                0.72f * scale,
                0.018f);
            float beaconHeight = commanded ? 0.82f : 0.64f;
            CreateBeam(
                center + Vector3.up * 0.04f,
                center + Vector3.up * beaconHeight,
                new Color(color.r, color.g, color.b, commanded ? 0.58f : 0.46f),
                commanded ? 0.34f : 0.28f,
                (commanded ? 0.020f : 0.016f) * scale);
            CreateImpactDisc(
                "Target Lock Beacon Cap",
                center + Vector3.up * beaconHeight,
                new Color(color.r, color.g, color.b, commanded ? 0.42f : 0.34f),
                commanded ? 0.30f : 0.24f,
                0.08f * scale,
                0.24f * scale,
                0.010f);
        }

        private void UpdateSquadFocusMarker()
        {
            if (squadFocusMarker == null && squadFocusBeacon == null)
            {
                return;
            }

            string targetId = PrimaryVisibleSquadFocusTargetId(out int focusCount);
            Vector2 position = default;
            float radius = 0f;
            bool isVisible = focusCount >= 2 && TryGetTargetMarker(targetId, out position, out radius);
            if (squadFocusMarker != null)
            {
                squadFocusMarker.SetActive(isVisible);
            }

            if (squadFocusBeacon != null)
            {
                squadFocusBeacon.SetActive(isVisible);
            }

            if (!isVisible)
            {
                return;
            }

            float pulse = 0.96f + Mathf.Sin(Time.time * 4.4f) * 0.06f;
            if (squadFocusMarker != null)
            {
                float scale = Mathf.Clamp(radius / 88f, 1.35f, 4.8f) * pulse;
                squadFocusMarker.transform.position = GroundMarkerPosition(position, 0.12f);
                squadFocusMarker.transform.localScale = new Vector3(scale, 0.018f, scale);
            }

            if (squadFocusBeacon != null)
            {
                squadFocusBeacon.transform.position = GroundMarkerPosition(position, 0.54f);
                squadFocusBeacon.transform.localScale = new Vector3(
                    0.080f * pulse,
                    0.34f + pulse * 0.12f,
                    0.080f * pulse);
                AssignMaterial(
                    squadFocusBeacon,
                    "SquadFocusBeacon",
                    new Color(1f, 0.70f, 0.16f, 0.48f));
            }
        }

        private string PrimaryVisibleSquadFocusTargetId(out int focusCount)
        {
            visibleFocusTargetCounts.Clear();
            string bestTargetId = string.Empty;
            focusCount = 0;

            foreach (UnitState unit in mission.PlayerUnits())
            {
                if (unit == null || unit.IsDestroyed)
                {
                    continue;
                }

                string targetId = string.IsNullOrEmpty(unit.AttackTargetId) ? unit.CurrentTargetId : unit.AttackTargetId;
                if (string.IsNullOrEmpty(targetId) || !TryGetTargetMarker(targetId, out _, out _))
                {
                    continue;
                }

                visibleFocusTargetCounts.TryGetValue(targetId, out int count);
                count++;
                visibleFocusTargetCounts[targetId] = count;
                if (count > focusCount)
                {
                    focusCount = count;
                    bestTargetId = targetId;
                }
            }

            return bestTargetId;
        }

        private void UpdateHostileFocusMarker()
        {
            if (hostileFocusMarker == null && hostileFocusBeacon == null)
            {
                return;
            }

            string targetId = PrimaryHostileFocusTargetId(out int focusCount);
            Vector2 position = default;
            float radius = 0f;
            bool isVisible = focusCount >= 2 && TryGetTargetMarker(targetId, out position, out radius);
            if (hostileFocusMarker != null)
            {
                hostileFocusMarker.SetActive(isVisible);
            }

            if (hostileFocusBeacon != null)
            {
                hostileFocusBeacon.SetActive(isVisible);
            }

            if (!isVisible)
            {
                return;
            }

            float pulse = 0.95f + Mathf.Sin(Time.time * 5.2f) * 0.08f;
            if (hostileFocusMarker != null)
            {
                float scale = Mathf.Clamp(radius / 92f, 1.28f, 4.6f) * pulse;
                hostileFocusMarker.transform.position = GroundMarkerPosition(position, 0.14f);
                hostileFocusMarker.transform.localScale = new Vector3(scale, 0.020f, scale);
            }

            if (hostileFocusBeacon != null)
            {
                hostileFocusBeacon.transform.position = GroundMarkerPosition(position, 0.62f);
                hostileFocusBeacon.transform.localScale = new Vector3(
                    0.095f * pulse,
                    0.42f + pulse * 0.14f,
                    0.095f * pulse);
                AssignMaterial(
                    hostileFocusBeacon,
                    "HostileFocusBeacon",
                    new Color(1f, 0.14f, 0.08f, 0.54f));
            }
        }

        private string PrimaryHostileFocusTargetId(out int focusCount)
        {
            hostileFocusTargetCounts.Clear();
            string bestTargetId = string.Empty;
            focusCount = 0;

            foreach (UnitState unit in mission.Units)
            {
                if (unit == null || unit.IsPlayerUnit || !unit.IsActive || unit.IsDestroyed)
                {
                    continue;
                }

                string targetId = string.IsNullOrEmpty(unit.AttackTargetId) ? unit.CurrentTargetId : unit.AttackTargetId;
                UnitState target = string.IsNullOrEmpty(targetId) ? null : mission.FindUnit(targetId);
                if (target == null || !target.IsPlayerUnit || target.IsDestroyed || !TryGetTargetMarker(targetId, out _, out _))
                {
                    continue;
                }

                hostileFocusTargetCounts.TryGetValue(targetId, out int count);
                count++;
                hostileFocusTargetCounts[targetId] = count;
                if (count > focusCount)
                {
                    focusCount = count;
                    bestTargetId = targetId;
                }
            }

            return bestTargetId;
        }

        private void UpdateCombatPressureMarker(UnitState commander)
        {
            if (combatPressureMarker == null && combatPressureBeacon == null)
            {
                return;
            }

            string mode = CombatTempoMode();
            int activeHostiles = CountActiveHostileUnits();
            bool isVisible = commander != null
                && commander.IsActive
                && !commander.IsDestroyed
                && !string.Equals(mode, "quiet", StringComparison.Ordinal)
                && activeHostiles > 0;
            if (combatPressureMarker != null)
            {
                combatPressureMarker.SetActive(isVisible);
            }

            if (combatPressureBeacon != null)
            {
                combatPressureBeacon.SetActive(isVisible);
            }

            if (!isVisible)
            {
                return;
            }

            bool isFire = string.Equals(mode, "fire", StringComparison.Ordinal);
            bool isContact = string.Equals(mode, "contact", StringComparison.Ordinal);
            float pressureScale = Mathf.Clamp01(activeHostiles / 20f);
            float pulseSpeed = isFire ? 6.6f : isContact ? 4.8f : 3.2f;
            float pulse = 0.96f + Mathf.Sin(Time.time * pulseSpeed) * (isFire ? 0.11f : 0.06f);
            Color color = isFire
                ? new Color(1f, 0.18f, 0.08f, 0.34f)
                : isContact
                    ? new Color(1f, 0.56f, 0.12f, 0.28f)
                    : new Color(0.44f, 0.84f, 1f, 0.20f);
            if (combatPressureMarker != null)
            {
                float scale = Mathf.Lerp(1.62f, 2.42f, pressureScale) * pulse;
                combatPressureMarker.transform.position = GroundMarkerPosition(commander.MissionPosition, 0.055f);
                combatPressureMarker.transform.localScale = new Vector3(scale, 0.014f, scale);
                AssignMaterial(
                    combatPressureMarker,
                    isFire ? "CombatPressureFire" : isContact ? "CombatPressureContact" : "CombatPressureTracking",
                    color);
            }

            if (combatPressureBeacon != null)
            {
                float beaconHeight = Mathf.Lerp(0.28f, 0.52f, pressureScale);
                combatPressureBeacon.transform.position = GroundMarkerPosition(commander.MissionPosition, 0.46f);
                combatPressureBeacon.transform.localScale = new Vector3(
                    0.070f * pulse,
                    beaconHeight + pulse * 0.075f,
                    0.070f * pulse);
                AssignMaterial(
                    combatPressureBeacon,
                    isFire ? "CombatPressureBeaconFire" : isContact ? "CombatPressureBeaconContact" : "CombatPressureBeaconTracking",
                    new Color(color.r, color.g, color.b, isFire ? 0.46f : isContact ? 0.38f : 0.30f));
            }
        }

        private void UpdateHostilePressureMarker(UnitState commander)
        {
            if (hostilePressureMarker == null
                && hostilePressureBeacon == null
                && hostilePressureVectorLine == null
                && hostilePressureVectorEndMarker == null)
            {
                return;
            }

            bool isVisible = TryGetActiveHostileCentroid(out Vector2 center, out int hostileCount);
            if (hostilePressureMarker != null)
            {
                hostilePressureMarker.SetActive(isVisible);
            }

            if (hostilePressureBeacon != null)
            {
                hostilePressureBeacon.SetActive(isVisible);
            }

            bool vectorVisible = isVisible
                && commander != null
                && commander.IsActive
                && !commander.IsDestroyed;
            if (hostilePressureVectorLine != null)
            {
                hostilePressureVectorLine.SetActive(vectorVisible);
            }

            if (hostilePressureVectorEndMarker != null)
            {
                hostilePressureVectorEndMarker.SetActive(vectorVisible);
            }

            if (!isVisible)
            {
                return;
            }

            string mode = CombatTempoMode();
            bool isFire = string.Equals(mode, "fire", StringComparison.Ordinal);
            bool isContact = string.Equals(mode, "contact", StringComparison.Ordinal);
            float pressureScale = Mathf.Clamp01(hostileCount / 20f);
            float pulseSpeed = isFire ? 5.8f : isContact ? 4.4f : 3.0f;
            float pulse = 0.96f + Mathf.Sin(Time.time * pulseSpeed) * (isFire ? 0.10f : 0.055f);
            Color color = isFire
                ? new Color(1f, 0.16f, 0.06f, 0.32f)
                : isContact
                    ? new Color(1f, 0.48f, 0.10f, 0.27f)
                    : new Color(1f, 0.72f, 0.22f, 0.20f);
            if (hostilePressureMarker != null)
            {
                float scale = Mathf.Lerp(1.28f, 2.18f, pressureScale) * pulse;
                hostilePressureMarker.transform.position = GroundMarkerPosition(center, 0.105f);
                hostilePressureMarker.transform.localScale = new Vector3(scale, 0.016f, scale);
                AssignMaterial(
                    hostilePressureMarker,
                    isFire ? "HostilePressureFire" : isContact ? "HostilePressureContact" : "HostilePressureTracking",
                    color);
            }

            if (hostilePressureBeacon != null)
            {
                float beaconHeight = Mathf.Lerp(0.30f, 0.58f, pressureScale);
                hostilePressureBeacon.transform.position = GroundMarkerPosition(center, 0.48f);
                hostilePressureBeacon.transform.localScale = new Vector3(
                    0.075f * pulse,
                    beaconHeight + pulse * 0.08f,
                    0.075f * pulse);
                AssignMaterial(
                    hostilePressureBeacon,
                    isFire ? "HostilePressureBeaconFire" : isContact ? "HostilePressureBeaconContact" : "HostilePressureBeaconTracking",
                    new Color(color.r, color.g, color.b, isFire ? 0.48f : isContact ? 0.40f : 0.32f));
            }

            if (vectorVisible)
            {
                Vector3 hostilePoint = GroundMarkerPosition(center, 0.18f);
                Vector3 commanderPoint = GroundMarkerPosition(commander.MissionPosition, 0.18f);
                PositionLine(hostilePressureVectorLine, hostilePoint, commanderPoint, isFire ? 0.024f : isContact ? 0.020f : 0.016f);
                Color vectorColor = new(color.r, color.g, color.b, isFire ? 0.46f : isContact ? 0.38f : 0.30f);
                AssignMaterial(
                    hostilePressureVectorLine,
                    isFire ? "HostilePressureVectorFire" : isContact ? "HostilePressureVectorContact" : "HostilePressureVectorTracking",
                    vectorColor);
                if (hostilePressureVectorEndMarker != null)
                {
                    float endPulse = 0.92f + Mathf.Sin(Time.time * (isFire ? 6.8f : 4.6f)) * (isFire ? 0.10f : 0.06f);
                    hostilePressureVectorEndMarker.transform.position = GroundMarkerPosition(commander.MissionPosition, 0.105f);
                    hostilePressureVectorEndMarker.transform.localScale = new Vector3(0.30f * endPulse, 0.014f, 0.30f * endPulse);
                    AssignMaterial(
                        hostilePressureVectorEndMarker,
                        isFire ? "HostilePressureVectorEndFire" : isContact ? "HostilePressureVectorEndContact" : "HostilePressureVectorEndTracking",
                        new Color(color.r, color.g, color.b, isFire ? 0.44f : isContact ? 0.36f : 0.28f));
                }
            }
        }

        private bool TryGetActiveHostileCentroid(out Vector2 center, out int hostileCount)
        {
            Vector2 sum = Vector2.zero;
            hostileCount = 0;
            foreach (UnitState unit in mission.Units)
            {
                if (unit == null || unit.IsPlayerUnit || !unit.IsActive || unit.IsDestroyed)
                {
                    continue;
                }

                sum += unit.MissionPosition;
                hostileCount++;
            }

            if (hostileCount <= 0)
            {
                center = default;
                return false;
            }

            center = sum / hostileCount;
            return true;
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

        private float TargetLineRadius(UnitState unit, string targetId)
        {
            if (unit == null)
            {
                return 0.028f;
            }

            if (unit.IsHeatLocked || !IsTargetInWeaponRange(unit, targetId))
            {
                return 0.040f;
            }

            if (unit.IsWeaponCoolingDown)
            {
                float readiness = Mathf.Clamp01(unit.WeaponReadinessRatio);
                float pulse = 0.0035f + Mathf.Sin(Time.time * 7.0f) * 0.0035f;
                return 0.018f + readiness * 0.012f + pulse;
            }

            return 0.032f;
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

        private Vector3 CommanderBeaconPosition(UnitState unit)
        {
            if (unitViews.TryGetValue(unit.Id, out DemoUnitView view) && view != null)
            {
                return view.transform.position + Vector3.up * 1.06f;
            }

            return GroundMarkerPosition(unit.MissionPosition, 1.06f);
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
                string detachedUnitId = pendingDetachedUnitId;
                CommanderCommandResult result = commandPort.MoveUnit(detachedUnitId, target);
                statusText = result.Accepted
                    ? (string.IsNullOrEmpty(label) ? "Detached order: " + detachedUnitId : label + " with " + detachedUnitId)
                    : result.Message;
                SpawnCommandResultCue(CommanderCommand.UnitMove(detachedUnitId, target), result);
                pendingDetachedUnitId = null;
            }
            else
            {
                CommanderCommandResult result = commandPort.MoveSquad(target);
                statusText = result.Accepted
                    ? (string.IsNullOrEmpty(label) ? "Squad move" : label)
                    : result.Message;
                SpawnCommandResultCue(CommanderCommand.SquadMove(target), result);
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
                SpawnCommandResultCue(CommanderCommand.AttackUnit(detachedUnitId, target.Id), result);
            }
            else
            {
                result = commandPort.AttackUnit(null, target.Id);
                statusText = result.Accepted ? "Squad focus: " + target.UnitType : result.Message;
                SpawnCommandResultCue(CommanderCommand.AttackUnit(null, target.Id), result);
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
                SpawnCommandResultCue(CommanderCommand.AttackStructure(detachedUnitId, target.Id), result);
            }
            else
            {
                result = commandPort.AttackStructure(null, target.Id);
                statusText = result.Accepted ? "Squad attack: " + target.ObjectType : result.Message;
                SpawnCommandResultCue(CommanderCommand.AttackStructure(null, target.Id), result);
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
                SpawnCommandResultCue(CommanderCommand.UnitJump(detachedUnitId, target), result);
            }
            else
            {
                CommanderCommand command = CommanderCommand.SquadJump(target);
                result = commandPort.JumpSquad(target);
                int eligibleCount = CountCommandEligiblePlayerUnits(command);
                statusText = result.Accepted
                    ? (string.IsNullOrEmpty(label) ? "Squad jet: " : "Squad jet vector: ")
                        + result.AcceptedCount
                        + "/"
                        + Mathf.Max(result.AcceptedCount, eligibleCount).ToString(CultureInfo.InvariantCulture)
                    : result.Message;
                SpawnCommandResultCue(command, result);
            }

            pendingDetachedUnitId = null;
            pendingJumpOrder = false;
        }

        private void ConfigureCameraFromMission()
        {
            cameraBaseHeight = Mathf.Max(8f, cameraHeight);
            cameraBaseOrthographicSize = DefaultCameraOrthographicSize;
            cameraZoomScale = 1f;
            cameraZoomMin = 0.76f;
            cameraZoomMax = 1.2f;
            cameraFollowWorldOffset = Vector3.zero;

            CameraDefinition sourceCamera = mission?.Contract?.mission?.camera;
            if (sourceCamera == null)
            {
                return;
            }

            if (sourceCamera.projectionAngle > 0.001f)
            {
                cameraPitch = Mathf.Clamp(90f - sourceCamera.projectionAngle, 54f, 72f);
            }

            if (Mathf.Abs(sourceCamera.startRotation) > 0.001f)
            {
                cameraYaw = -sourceCamera.startRotation;
            }

            float normalizedScale = 0.5f;
            if (sourceCamera.zoomMax > sourceCamera.zoomMin + 0.001f)
            {
                normalizedScale = Mathf.InverseLerp(sourceCamera.zoomMin, sourceCamera.zoomMax, sourceCamera.newScale);
            }

            cameraBaseOrthographicSize = Mathf.Clamp(Mathf.Lerp(36f, 18f, normalizedScale), 20f, 34f);
            cameraBaseHeight = Mathf.Clamp(cameraBaseOrthographicSize * 2.75f, 48f, 96f);

            if (sourceCamera.newScale > 0.001f)
            {
                cameraZoomMin = Mathf.Clamp(sourceCamera.zoomMin / sourceCamera.newScale, 0.72f, 1f);
                cameraZoomMax = Mathf.Clamp(sourceCamera.zoomMax / sourceCamera.newScale, 1f, 1.25f);
            }

            UnitSpawn commanderSpawn = FirstPlayerUnitSpawn();
            if (sourceCamera.startPosition != null && commanderSpawn?.position != null)
            {
                Vector3 sourceCameraWorld = DemoTerrainView.MissionToWorld(new Vector2(sourceCamera.startPosition.x, sourceCamera.startPosition.y));
                Vector3 commanderStartWorld = DemoTerrainView.MissionToWorld(new Vector2(commanderSpawn.position.x, commanderSpawn.position.y));
                cameraFollowWorldOffset = sourceCameraWorld - commanderStartWorld;
                cameraFollowWorldOffset.y = 0f;
            }

            Debug.Log(
                "MC2 source camera configured: pitch="
                + cameraPitch.ToString("0.###", CultureInfo.InvariantCulture)
                + " yaw="
                + cameraYaw.ToString("0.###", CultureInfo.InvariantCulture)
                + " ortho="
                + cameraBaseOrthographicSize.ToString("0.###", CultureInfo.InvariantCulture)
                + " height="
                + cameraBaseHeight.ToString("0.###", CultureInfo.InvariantCulture)
                + " zoom="
                + cameraZoomMin.ToString("0.###", CultureInfo.InvariantCulture)
                + "-"
                + cameraZoomMax.ToString("0.###", CultureInfo.InvariantCulture)
                + " followOffset="
                + cameraFollowWorldOffset.ToString("F2"));
        }

        private UnitSpawn FirstPlayerUnitSpawn()
        {
            if (mission?.Contract?.units == null)
            {
                return null;
            }

            foreach (UnitSpawn spawn in mission.Contract.units)
            {
                if (spawn != null && spawn.isPlayerUnit)
                {
                    return spawn;
                }
            }

            return null;
        }

        private void HandleCameraZoom()
        {
            if (mainCamera == null)
            {
                return;
            }

            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) < 0.01f)
            {
                return;
            }

            cameraZoomScale = Mathf.Clamp(cameraZoomScale + scroll * CameraZoomWheelStep, cameraZoomMin, cameraZoomMax);
            ApplyCameraProjection();
        }

        private void ApplyCameraProjection()
        {
            if (mainCamera == null)
            {
                return;
            }

            mainCamera.orthographicSize = Mathf.Max(8f, cameraBaseOrthographicSize / Mathf.Max(0.1f, cameraZoomScale));
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

            Vector3 commanderWorld = DemoUnitView.MissionToWorld(commander.MissionPosition) + cameraFollowWorldOffset;
            Quaternion rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
            float effectiveHeight = cameraBaseHeight / Mathf.Max(0.1f, cameraZoomScale);
            Vector3 offset = rotation * new Vector3(0f, 0f, -effectiveHeight);
            mainCamera.transform.SetPositionAndRotation(commanderWorld + offset, rotation);
        }

        private void OnGUI()
        {
            EnsureUiSkin();
            GUIStyle previousBox = GUI.skin.box;
            GUIStyle previousLabel = GUI.skin.label;
            GUIStyle previousButton = GUI.skin.button;
            GUIStyle previousTextField = GUI.skin.textField;
            GUIStyle previousToggle = GUI.skin.toggle;
            Color previousColor = GUI.color;
            Color previousBackgroundColor = GUI.backgroundColor;
            Color previousContentColor = GUI.contentColor;

            GUI.skin.box = uiBoxStyle;
            GUI.skin.label = uiLabelStyle;
            GUI.skin.button = uiButtonStyle;
            GUI.skin.textField = uiTextFieldStyle;
            GUI.skin.toggle = uiToggleStyle;
            GUI.backgroundColor = Color.white;
            GUI.contentColor = UiTextColor;

            try
            {
                DrawTopStatusStrip();

                if (mission == null)
                {
                    return;
                }

                if (showStartupContinuePanel)
                {
                    DrawStartupContinuePanel();
                    return;
                }

                DrawUnitPanel();
                if (!showLoadoutPanel)
                {
                    DrawCombatPanel();
                    DrawMissionBriefPanel();
                    DrawMissionMap();
                }

                DrawLoadoutPanel();
                DrawSystemPanel();
                DrawMissionResultPanel();
                DrawMissionListPanel();
            }
            finally
            {
                GUI.skin.box = previousBox;
                GUI.skin.label = previousLabel;
                GUI.skin.button = previousButton;
                GUI.skin.textField = previousTextField;
                GUI.skin.toggle = previousToggle;
                GUI.color = previousColor;
                GUI.backgroundColor = previousBackgroundColor;
                GUI.contentColor = previousContentColor;
            }
        }

        private void EnsureUiSkin()
        {
            if (uiBoxStyle != null)
            {
                return;
            }

            uiPanelTexture = CreateUiTexture(UiPanelColor);
            uiButtonTexture = CreateUiTexture(UiButtonColor);
            uiButtonHoverTexture = CreateUiTexture(new Color(0.10f, 0.16f, 0.19f, 0.98f));
            uiTextFieldTexture = CreateUiTexture(new Color(0.02f, 0.027f, 0.034f, 0.96f));

            uiBoxStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(10, 10, 7, 7),
                border = new RectOffset(1, 1, 1, 1)
            };
            uiBoxStyle.normal.background = uiPanelTexture;
            uiBoxStyle.normal.textColor = UiTextColor;

            uiLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                padding = new RectOffset(0, 0, 0, 0)
            };
            uiLabelStyle.normal.textColor = UiTextColor;

            uiButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(6, 6, 3, 3),
                border = new RectOffset(1, 1, 1, 1)
            };
            uiButtonStyle.normal.background = uiButtonTexture;
            uiButtonStyle.hover.background = uiButtonHoverTexture;
            uiButtonStyle.active.background = uiButtonHoverTexture;
            uiButtonStyle.focused.background = uiButtonHoverTexture;
            uiButtonStyle.normal.textColor = UiTextColor;
            uiButtonStyle.hover.textColor = Color.white;
            uiButtonStyle.active.textColor = Color.white;
            uiButtonStyle.focused.textColor = Color.white;

            uiTextFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 12,
                padding = new RectOffset(6, 6, 3, 3),
                border = new RectOffset(1, 1, 1, 1)
            };
            uiTextFieldStyle.normal.background = uiTextFieldTexture;
            uiTextFieldStyle.focused.background = uiTextFieldTexture;
            uiTextFieldStyle.normal.textColor = UiTextColor;
            uiTextFieldStyle.focused.textColor = Color.white;

            uiToggleStyle = new GUIStyle(GUI.skin.toggle)
            {
                fontSize = 12
            };
            uiToggleStyle.normal.textColor = UiTextColor;
            uiToggleStyle.hover.textColor = Color.white;
            uiToggleStyle.onNormal.textColor = UiCyanColor;
            uiToggleStyle.onHover.textColor = UiCyanColor;

            uiHeaderStyle = new GUIStyle(uiLabelStyle)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
            uiHeaderStyle.normal.textColor = UiCyanColor;

            uiStatusStyle = new GUIStyle(uiLabelStyle)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            uiStatusStyle.normal.textColor = UiTextColor;
        }

        private static Texture2D CreateUiTexture(Color color)
        {
            Texture2D texture = new(1, 1, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private void DestroyUiTexture(ref Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            Destroy(texture);
            texture = null;
        }

        private void DrawTopStatusStrip()
        {
            float stripWidth = Mathf.Min(Mathf.Max(440f, Screen.width * 0.52f), 660f);
            Rect strip = new(12f, 12f, stripWidth, 34f);
            DrawColorRect(strip, UiPanelColor);
            DrawRectBorder(strip, UiBorderColor, 1f);
            DrawColorRect(new Rect(strip.x, strip.y, 4f, strip.height), UiCyanColor);

            string flow = TopStatusModeText(demoFlowScreen);
            string funds = TopStatusFundsText(demoInventory?.tokenBalance);
            GUI.Label(
                new Rect(strip.x + 14f, strip.y + 8f, strip.width - 170f, 18f),
                flow + "  " + TruncateText(statusText, 58),
                uiStatusStyle);
            GUI.Label(
                new Rect(strip.xMax - 142f, strip.y + 8f, 126f, 18f),
                funds,
                uiStatusStyle);
        }

        private void DrawDesignPanelFrame(Rect panel, string title, Color accent)
        {
            DrawColorRect(panel, UiPanelColor);
            DrawRectBorder(panel, UiBorderColor, 1f);
            DrawColorRect(new Rect(panel.x, panel.y, 4f, panel.height), accent);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 8f, panel.width - 24f, 18f), title, uiHeaderStyle);
        }

        private void DrawDesignInsetFrame(Rect panel, Color accent)
        {
            DrawColorRect(panel, new Color(0.018f, 0.026f, 0.032f, 0.86f));
            DrawRectBorder(panel, new Color(accent.r, accent.g, accent.b, 0.5f), 1f);
            DrawColorRect(new Rect(panel.x, panel.y, 3f, panel.height), accent);
        }

        private void DrawModalBackdrop()
        {
            DrawColorRect(new Rect(0f, 46f, Screen.width, Mathf.Max(0f, Screen.height - 46f)), new Color(0f, 0f, 0f, 0.46f));
        }

        private void DrawStartupContinuePanel()
        {
            if (!showStartupContinuePanel)
            {
                return;
            }

            Rect panel = StartupContinuePanelRect();
            DrawDesignPanelFrame(
                panel,
                startupSaveChoicesOpenedFromSystem ? "Save Slot / 存档" : "Save Slot / 继续",
                UiCyanColor);
            float x = panel.x + 18f;
            float y = panel.y + 36f;
            float width = panel.width - 36f;
            string defaultPath = DefaultSavedAccountFilePath();
            bool fileExists = File.Exists(defaultPath);
            bool canContinue = fileExists && startupContinueSaveReady;
            GUI.Label(new Rect(x, y, width, 20f), canContinue ? SaveSlotTitleReadyText : SaveSlotNeedsReviewText);
            y += 24f;
            float summaryHeight = startupSaveChoicesOpenedFromSystem ? 112f : 88f;
            DrawDesignInsetFrame(new Rect(x - 8f, y - 6f, width + 16f, summaryHeight), UiCyanColor);
            GUI.Label(new Rect(x, y, width, 20f), TruncateText(startupContinueSummaryText, 60));
            y += 22f;
            GUI.Label(new Rect(x, y, width, 20f), TruncateText(startupContinueRosterText, 60));
            y += 22f;
            GUI.Label(new Rect(x, y, width, 20f), TruncateText(startupContinueDeltaText, 60));
            y += 22f;
            GUI.Label(new Rect(x, y, width, 20f), TruncateText(startupContinueFileText, 60));
            y += 22f;
            if (startupSaveChoicesOpenedFromSystem)
            {
                string lastSave = string.IsNullOrWhiteSpace(lastSavedAccountFileResultText)
                    ? SavedAccountIdleLabel
                    : lastSavedAccountFileResultText;
                GUI.Label(new Rect(x, y, width, 20f), SaveSlotResultLabel + TruncateText(lastSave, 58));
                y += 22f;
            }

            y += 32f;

            bool previousEnabled = GUI.enabled;
            bool titleActions = !startupSaveChoicesOpenedFromSystem
                && !startupResetSlotConfirmPending
                && !startupNewGameConfirmPending;
            if (titleActions)
            {
                float halfWidth = (width - 8f) * 0.5f;
                GUI.enabled = previousEnabled && canContinue;
                if (GUI.Button(new Rect(x, y, halfWidth, 30f), "Continue"))
                {
                    if (TryLoadDefaultSavedAccount("Startup continue"))
                    {
                        showStartupContinuePanel = false;
                        startupSaveChoicesOpenedFromSystem = false;
                        startupNewGameConfirmPending = false;
                        startupResetSlotConfirmPending = false;
                        if (mission.Result == MissionResultState.InProgress)
                        {
                            SetPaused(false);
                        }

                        statusText = "Continue loaded";
                        SetDemoFlowScreen(DemoFlowScreen.Battle);
                    }
                }

                GUI.enabled = previousEnabled;
                if (GUI.Button(new Rect(x + halfWidth + 8f, y, halfWidth, 30f), "New Company"))
                {
                    startupNewGameConfirmPending = true;
                    startupResetSlotConfirmPending = false;
                    statusText = "Confirm new company";
                    RecordSavedAccountFileResult("New Company confirm", false, "save slot kept");
                }

                return;
            }

            GUI.enabled = previousEnabled && canContinue;
            if (GUI.Button(new Rect(x, y, width, 30f), "Continue"))
            {
                if (TryLoadDefaultSavedAccount("Startup continue"))
                {
                    showStartupContinuePanel = false;
                    startupSaveChoicesOpenedFromSystem = false;
                    startupNewGameConfirmPending = false;
                    startupResetSlotConfirmPending = false;
                    if (mission.Result == MissionResultState.InProgress)
                    {
                        SetPaused(false);
                    }

                    statusText = "Continue loaded";
                    SetDemoFlowScreen(DemoFlowScreen.Battle);
                }
            }

            GUI.enabled = previousEnabled;
            y += 38f;
            if (startupSaveChoicesOpenedFromSystem)
            {
                float halfWidth = (width - 8f) * 0.5f;
                if (GUI.Button(new Rect(x, y, halfWidth, 30f), "Save Current"))
                {
                    TrySaveCurrentFromSaveChoices();
                }

                if (GUI.Button(new Rect(x + halfWidth + 8f, y, halfWidth, 30f), "Export Copy"))
                {
                    TryExportCopyFromSaveChoices();
                }

                y += 38f;
            }

            if (startupResetSlotConfirmPending)
            {
                GUI.Label(new Rect(x, y - 2f, width, 18f), SaveSlotResetConfirmText);
                y += 24f;
                float halfWidth = (width - 8f) * 0.5f;
                if (GUI.Button(new Rect(x, y, halfWidth, 30f), SaveSlotConfirmResetButtonLabel))
                {
                    TryResetDefaultSaveSlotFromSaveChoices();
                }

                if (GUI.Button(new Rect(x + halfWidth + 8f, y, halfWidth, 30f), "Cancel"))
                {
                    startupResetSlotConfirmPending = false;
                    statusText = "Reset canceled";
                }
            }
            else if (startupNewGameConfirmPending)
            {
                GUI.Label(new Rect(x, y - 2f, width, 18f), SaveSlotNewCompanyConfirmText);
                y += 24f;
                float halfWidth = (width - 8f) * 0.5f;
                if (GUI.Button(new Rect(x, y, halfWidth, 30f), SaveSlotStartFreshButtonLabel))
                {
                    TryStartFreshDemoRun("New company started");
                }

                if (GUI.Button(new Rect(x + halfWidth + 8f, y, halfWidth, 30f), "Cancel"))
                {
                    startupNewGameConfirmPending = false;
                    statusText = "New company canceled";
                }
            }
            else
            {
                if (startupSaveChoicesOpenedFromSystem)
                {
                    float halfWidth = (width - 8f) * 0.5f;
                    if (GUI.Button(new Rect(x, y, halfWidth, 30f), "New Company"))
                    {
                        startupNewGameConfirmPending = true;
                        startupResetSlotConfirmPending = false;
                        statusText = "Confirm new company";
                        RecordSavedAccountFileResult("New Company confirm", false, "save slot kept");
                    }

                    if (GUI.Button(new Rect(x + halfWidth + 8f, y, halfWidth, 30f), "Reset Slot"))
                    {
                        startupResetSlotConfirmPending = true;
                        startupNewGameConfirmPending = false;
                        statusText = "Confirm slot reset";
                        RecordSavedAccountFileResult("Reset Slot confirm", false, "old save copied first");
                    }
                }
                else if (GUI.Button(new Rect(x, y, width, 30f), "New Company"))
                {
                    startupNewGameConfirmPending = true;
                    startupResetSlotConfirmPending = false;
                    statusText = "Confirm new company";
                    RecordSavedAccountFileResult("New Company confirm", false, "save slot kept");
                }

                y += 38f;
                if (startupSaveChoicesOpenedFromSystem
                    && GUI.Button(new Rect(x, y, width, 30f), "Back"))
                {
                    ReturnFromSaveChoicesToSystem();
                }
            }
        }

        private void DrawUnitPanel()
        {
            float panelX = 12f;
            float panelY = 54f;
            float panelWidth = 320f;
            float panelHeight = 74f + (CountPlayerUnits() * 78f);
            DrawDesignPanelFrame(new Rect(panelX, panelY, panelWidth, panelHeight), "Squad Command", UiCyanColor);

            float x = panelX + 10f;
            float y = panelY + 34f;

            if (GUI.Button(new Rect(x, y, 50f, 28f), "All"))
            {
                pendingDetachedUnitId = null;
                pendingJumpOrder = false;
                statusText = "Squad selected";
            }

            if (GUI.Button(new Rect(x + 56f, y, 50f, 28f), pendingJumpOrder ? "Jet..." : "Jet"))
            {
                pendingJumpOrder = true;
                statusText = string.IsNullOrEmpty(pendingDetachedUnitId)
                    ? "Select squad jet destination"
                    : "Select jet destination for " + pendingDetachedUnitId;
            }

            if (GUI.Button(new Rect(x + 112f, y, 50f, 28f), showMissionMap ? "Map-" : "Map"))
            {
                showMissionMap = !showMissionMap;
                showLoadoutPanel = false;
                showWarehouseDraftFitPreview = false;
                showSquadSelectionPreview = false;
                warehouseDraftFitPreviewMechId = null;
                ClearSquadSelectionDraft();
                ClearSquadSelectionCompletedReplacement();
                showSystemPanel = false;
                showMissionListPanel = false;
                if (mission.Result == MissionResultState.InProgress)
                {
                    SetPaused(false);
                }

                statusText = showMissionMap ? "Mission map open" : "Mission map closed";
                SetDemoFlowScreen(DemoFlowScreen.Battle);
            }

            if (GUI.Button(new Rect(x + 168f, y, 50f, 28f), showLoadoutPanel ? "Bay-" : "Bay"))
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

                    statusText = "Mech Lab closed";
                    SetDemoFlowScreen(DemoFlowScreen.Battle);
                }
                else
                {
                    OpenLoadoutPanel();
                }
            }

            if (GUI.Button(new Rect(x + 224f, y, 76f, 28f), "System"))
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

                Rect rowRect = new(x, y, panelWidth - 20f, 34f);
                if (GUI.Button(rowRect, label))
                {
                    pendingDetachedUnitId = unit.Id;
                    statusText = pendingJumpOrder
                        ? "Select jet destination for " + unit.UnitType
                        : "Select destination for " + unit.UnitType;
                }

                Color rowCue = unit.IsDetached ? UiAmberColor : unit.IsDestroyed ? Color.red : UiBorderColor;
                DrawRectBorder(rowRect, rowCue, unit.IsDetached ? 2f : 1f);

                Rect barBack = new(x + 6f, y + 24f, panelWidth - 32f, 4f);
                DrawColorRect(barBack, UiTrackColor);
                Rect bar = new(barBack.x, barBack.y, barBack.width * unit.Structure, barBack.height);
                DrawColorRect(bar, unit.IsDestroyed ? Color.red : new Color(0.35f, 0.88f, 0.46f, 0.96f));

                if (unit.CombatHeatPerShot > 0f)
                {
                    Rect heatBack = new(x + 6f, y + 31f, panelWidth - 32f, 4f);
                    DrawColorRect(heatBack, UiTrackColor);
                    Rect heatBar = new(heatBack.x, heatBack.y, heatBack.width * Mathf.Clamp01(unit.HeatRatio), heatBack.height);
                    DrawColorRect(heatBar, unit.IsHeatLocked ? Color.red : UiAmberColor);
                }

                Rect readyBack = new(x + 6f, y + 38f, panelWidth - 32f, 3f);
                DrawColorRect(readyBack, UiTrackColor);
                Rect readyBar = new(readyBack.x, readyBack.y, readyBack.width * unit.WeaponReadinessRatio, readyBack.height);
                DrawColorRect(readyBar, unit.IsHeatLocked ? Color.red : UiCyanColor);

                GUI.Label(new Rect(x + 6f, y + 42f, panelWidth - 26f, 16f), WeaponStatusText(unit));
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
            DrawDesignPanelFrame(panel, "Mission / 任务", UiAmberColor);
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

            float objectiveLineHeight = MissionBriefObjectiveLineHeight(panel, y, visibleObjectives);
            foreach (ObjectiveState objective in mission.Objectives)
            {
                if (objective.Definition.hidden)
                {
                    continue;
                }

                if (y + objectiveLineHeight > panel.yMax - 8f)
                {
                    break;
                }

                Color previous = GUI.color;
                GUI.color = objective.IsComplete
                    ? new Color(0.55f, 1f, 0.55f)
                    : objective.IsActive ? new Color(1f, 0.88f, 0.42f) : new Color(0.62f, 0.68f, 0.72f);
                GUI.Label(new Rect(x, y, width, objectiveLineHeight), MissionBriefObjectiveLine(objective));
                GUI.color = previous;
                y += objectiveLineHeight;
            }

            if (mission.Structures.Count == 0 || y + 52f > panel.yMax - 6f)
            {
                return;
            }

            y += 4f;
            GUI.Label(new Rect(x, y, width, 18f), "Target structures");
            y += 20f;
            foreach (StructureState structure in mission.Structures)
            {
                if (y + 24f > panel.yMax - 6f)
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

        private static float MissionBriefObjectiveStartY(Rect panel)
        {
            return panel.y + 32f + 24f + 22f;
        }

        private static float MissionBriefObjectiveLineHeight(Rect panel, float objectiveStartY, int visibleObjectives)
        {
            if (visibleObjectives <= 0)
            {
                return 18f;
            }

            float available = Mathf.Max(0f, panel.yMax - objectiveStartY - 8f);
            return Mathf.Clamp(available / visibleObjectives, 14f, 18f);
        }

        private static int MissionBriefDrawableObjectiveRows(Rect panel, float objectiveStartY, float objectiveLineHeight)
        {
            if (objectiveLineHeight <= 0f)
            {
                return 0;
            }

            float available = Mathf.Max(0f, panel.yMax - objectiveStartY - 8f);
            return Mathf.FloorToInt(available / objectiveLineHeight);
        }

        private static string ObjectiveStateLabel(ObjectiveState objective)
        {
            if (objective.IsComplete)
            {
                return "[done] ";
            }

            return objective.IsActive ? "[active] " : "[locked] ";
        }

        private static string MissionBriefObjectiveLine(ObjectiveState objective)
        {
            if (objective == null)
            {
                return "[locked] Objective";
            }

            return ObjectiveStateLabel(objective) + MissionBriefObjectiveTitle(objective?.Definition);
        }

        private static string MissionBriefObjectiveTitle(ObjectiveDefinition definition)
        {
            if (definition == null)
            {
                return "Objective";
            }

            string sourceTitle = string.IsNullOrWhiteSpace(definition.title) ? "Objective" : definition.title.Trim();
            string number = MissionBriefObjectiveNumber(sourceTitle, definition.index);
            string body = StripObjectiveNumber(sourceTitle);
            return number + " " + CompactMissionBriefObjectiveBody(body);
        }

        private static string MissionBriefObjectiveNumber(string sourceTitle, int objectiveIndex)
        {
            if (!string.IsNullOrWhiteSpace(sourceTitle))
            {
                int dotIndex = sourceTitle.IndexOf('.');
                if (dotIndex > 0 && dotIndex <= 3)
                {
                    string prefix = sourceTitle.Substring(0, dotIndex).Trim();
                    if (int.TryParse(prefix, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
                        && parsed > 0)
                    {
                        return parsed.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }

            return (objectiveIndex + 1).ToString(CultureInfo.InvariantCulture);
        }

        private static string StripObjectiveNumber(string sourceTitle)
        {
            if (string.IsNullOrWhiteSpace(sourceTitle))
            {
                return "Objective";
            }

            int dotIndex = sourceTitle.IndexOf('.');
            if (dotIndex > 0 && dotIndex < sourceTitle.Length - 1 && dotIndex <= 3)
            {
                string prefix = sourceTitle.Substring(0, dotIndex).Trim();
                if (int.TryParse(prefix, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                {
                    return sourceTitle.Substring(dotIndex + 1).Trim();
                }
            }

            return sourceTitle.Trim();
        }

        private static string CompactMissionBriefObjectiveBody(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return "Objective";
            }

            if (body.IndexOf("Abandoned Airfield", StringComparison.OrdinalIgnoreCase) >= 0
                || body.IndexOf("Airfield", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Airfield";
            }

            if (body.IndexOf("Hangar", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Hangar";
            }

            if (body.IndexOf("Bandit Patrol", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Bandit patrol";
            }

            if (body.IndexOf("North Island", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "North island";
            }

            if (body.IndexOf("Bandits", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Bandits";
            }

            if (body.IndexOf("Extraction", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Extraction";
            }

            return TruncateText(body.Replace("'Mechs", "Mechs"), 24);
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
            DrawDesignPanelFrame(panel, "Combat / 战况", UiCyanColor);
            GUI.Label(new Rect(x + 12f, panel.y + 36f, panel.width - 24f, 18f), CombatSituationText());
            DrawCombatTacticalPulseLine(x + 12f, panel.y + 58f, panel.width - 24f);
            float y = panel.y + 84f;
            foreach (string line in combatLog)
            {
                if (y > panel.yMax - 22f)
                {
                    GUI.Label(new Rect(x + 12f, y, 320f, 20f), "...");
                    break;
                }

                GUI.Label(new Rect(x + 12f, y, 320f, 20f), TruncateText(line, 58));
                y += 20f;
            }
        }

        private void DrawCombatTacticalPulseLine(float x, float y, float width)
        {
            Color previous = GUI.color;
            GUI.color = CombatTempoColor();
            GUI.Label(new Rect(x, y, width, 18f), CombatTacticalPulseText());
            GUI.color = previous;
        }

        private string CombatSituationText()
        {
            int playerReady = CountActivePlayerUnits();
            int playerSlots = CountPlayerUnits();
            int detached = CountDetachedPlayerUnits();
            int activeHostiles = CountActiveHostileUnits();
            int liveTargets = CountLiveStructures();
            int salvage = CountDestroyedHostileUnits();
            string tempo = HasRecentCombatEvent() ? "contact" : "quiet";
            return "Cmd "
                + CommanderUnitLabel()
                + "  Squad "
                + playerReady.ToString(CultureInfo.InvariantCulture)
                + "/"
                + playerSlots.ToString(CultureInfo.InvariantCulture)
                + "  Solo "
                + detached.ToString(CultureInfo.InvariantCulture)
                + "  Hostiles "
                + activeHostiles.ToString(CultureInfo.InvariantCulture)
                + "  Targets "
                + liveTargets.ToString(CultureInfo.InvariantCulture)
                + (salvage <= 0 ? "" : "  Salvage " + salvage.ToString(CultureInfo.InvariantCulture))
                + "  "
                + tempo;
        }

        private string CombatTempoText()
        {
            string mode = CombatTempoMode();
            int activeHostiles = CountActiveHostileUnits();
            if (string.Equals(mode, "fire", StringComparison.Ordinal))
            {
                return "Tempo Fire  hostile pressure "
                    + activeHostiles.ToString(CultureInfo.InvariantCulture);
            }

            if (string.Equals(mode, "contact", StringComparison.Ordinal))
            {
                string count = lastContactCount > 1 ? " x" + lastContactCount.ToString(CultureInfo.InvariantCulture) : "";
                return "Tempo Contact  " + TruncateText(lastContactLabel, 24) + count;
            }

            if (string.Equals(mode, "tracking", StringComparison.Ordinal))
            {
                return "Tempo Tracking  hostiles "
                    + activeHostiles.ToString(CultureInfo.InvariantCulture);
            }

            return "Tempo Quiet  no contact";
        }

        private string CombatTempoMode()
        {
            if (HasRecentCombatEvent())
            {
                return "fire";
            }

            if (HasRecentContactEvent())
            {
                return "contact";
            }

            return CountActiveHostileUnits() > 0 ? "tracking" : "quiet";
        }

        private string CombatContactPressureText()
        {
            List<string> parts = new();
            AddContactPressurePart(parts, "Air", CountLiveEncounterGroup("airfield"));
            AddContactPressurePart(parts, "North", CountLiveEncounterGroup("north"));
            AddContactPressurePart(parts, "Amb", CountLiveEncounterGroup("ambush"));
            AddContactPressurePart(parts, "Star", CountLiveEncounterGroup("starslayer"));

            int known = 0;
            for (int index = 0; index < parts.Count; index++)
            {
                string part = parts[index];
                int separator = part.LastIndexOf(' ');
                if (separator >= 0
                    && int.TryParse(part.Substring(separator + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
                {
                    known += parsed;
                }
            }

            int other = Mathf.Max(0, CountActiveHostileUnits() - known);
            AddContactPressurePart(parts, "Other", other);
            return parts.Count == 0 ? "Contacts none" : "Contacts " + string.Join("  ", parts);
        }

        private static void AddContactPressurePart(List<string> parts, string label, int count)
        {
            if (count <= 0)
            {
                return;
            }

            parts.Add(label + " " + count.ToString(CultureInfo.InvariantCulture));
        }

        private string CombatFocusText()
        {
            string targetId = PrimaryCommandedFocusTargetId(out int focusCount);
            if (string.IsNullOrEmpty(targetId) || focusCount <= 0)
            {
                return "Focus none";
            }

            string label = CombatTargetLabel(targetId);
            int health = CombatTargetHealthPercent(targetId);
            string state = CombatFocusStateText(targetId);
            return "Focus "
                + TruncateText(label, 18)
                + " x"
                + focusCount.ToString(CultureInfo.InvariantCulture)
                + " HP "
                + health.ToString(CultureInfo.InvariantCulture)
                + "% "
                + state;
        }

        private string CombatTacticalPulseText()
        {
            string pulse = CombatTempoCompactText();
            string focus = CombatFocusCompactText();
            return string.IsNullOrEmpty(focus) ? pulse : pulse + "  " + focus;
        }

        private string CombatTempoCompactText()
        {
            string mode = CombatTempoMode();
            int activeHostiles = CountActiveHostileUnits();
            string stage = EncounterPacingStageText();
            if (string.Equals(mode, "fire", StringComparison.Ordinal))
            {
                return "Fire "
                    + stage
                    + " hostiles "
                    + activeHostiles.ToString(CultureInfo.InvariantCulture);
            }

            if (string.Equals(mode, "contact", StringComparison.Ordinal))
            {
                string count = lastContactCount > 1 ? " x" + lastContactCount.ToString(CultureInfo.InvariantCulture) : "";
                return "Contact " + stage + " " + TruncateText(lastContactLabel, 16) + count;
            }

            if (string.Equals(mode, "tracking", StringComparison.Ordinal))
            {
                return "Tracking "
                    + stage
                    + " hostiles "
                    + activeHostiles.ToString(CultureInfo.InvariantCulture);
            }

            return "Quiet " + stage;
        }

        private string CombatFocusCompactText()
        {
            string targetId = PrimaryCommandedFocusTargetId(out int focusCount);
            if (string.IsNullOrEmpty(targetId) || focusCount <= 0)
            {
                return "";
            }

            return "Focus "
                + TruncateText(CombatTargetLabel(targetId), 14)
                + " x"
                + focusCount.ToString(CultureInfo.InvariantCulture)
                + " "
                + CombatTargetHealthPercent(targetId).ToString(CultureInfo.InvariantCulture)
                + "% "
                + CombatFocusStateText(targetId);
        }

        private string PrimaryCommandedFocusTargetId(out int focusCount)
        {
            Dictionary<string, int> counts = new(StringComparer.Ordinal);
            string bestTarget = "";
            int bestCount = 0;
            foreach (UnitState unit in mission.PlayerUnits())
            {
                if (unit == null || unit.IsDestroyed || string.IsNullOrEmpty(unit.AttackTargetId))
                {
                    continue;
                }

                string targetId = unit.AttackTargetId;
                counts.TryGetValue(targetId, out int count);
                count += 1;
                counts[targetId] = count;
                if (count > bestCount)
                {
                    bestTarget = targetId;
                    bestCount = count;
                }
            }

            focusCount = bestCount;
            return bestTarget;
        }

        private int CountCommandedFocusTargets()
        {
            PrimaryCommandedFocusTargetId(out int focusCount);
            return focusCount;
        }

        private string CombatTargetLabel(string targetId)
        {
            UnitState unit = mission.FindUnit(targetId);
            if (unit != null)
            {
                return string.IsNullOrWhiteSpace(unit.UnitType) ? "Unit" : unit.UnitType;
            }

            StructureState structure = mission.FindStructure(targetId);
            if (structure != null)
            {
                return string.IsNullOrWhiteSpace(structure.ObjectType) ? "Structure" : structure.ObjectType;
            }

            return "Target";
        }

        private int CombatTargetHealthPercent(string targetId)
        {
            UnitState unit = mission.FindUnit(targetId);
            if (unit != null)
            {
                return Mathf.RoundToInt(Mathf.Clamp01(unit.Structure) * 100f);
            }

            StructureState structure = mission.FindStructure(targetId);
            if (structure != null)
            {
                return Mathf.RoundToInt(Mathf.Clamp01(structure.Structure) * 100f);
            }

            return 0;
        }

        private string CombatFocusStateText(string targetId)
        {
            int ready = 0;
            int cooling = 0;
            int hot = 0;
            int closing = 0;
            foreach (UnitState unit in mission.PlayerUnits())
            {
                if (unit == null || unit.IsDestroyed || !string.Equals(unit.AttackTargetId, targetId, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!IsTargetInWeaponRange(unit, targetId))
                {
                    closing += 1;
                }
                else if (unit.IsHeatLocked)
                {
                    hot += 1;
                }
                else if (unit.IsWeaponCoolingDown)
                {
                    cooling += 1;
                }
                else
                {
                    ready += 1;
                }
            }

            if (ready > 0)
            {
                return "ready " + ready.ToString(CultureInfo.InvariantCulture);
            }

            if (closing > 0)
            {
                return "closing";
            }

            if (hot > 0)
            {
                return "hot";
            }

            if (cooling > 0)
            {
                return "cycling";
            }

            return "tracking";
        }

        private Color CombatTempoColor()
        {
            string mode = CombatTempoMode();
            if (string.Equals(mode, "fire", StringComparison.Ordinal))
            {
                return new Color(1f, 0.46f, 0.26f);
            }

            if (string.Equals(mode, "contact", StringComparison.Ordinal)
                || string.Equals(mode, "tracking", StringComparison.Ordinal))
            {
                return new Color(1f, 0.84f, 0.34f);
            }

            return new Color(0.62f, 0.78f, 0.86f);
        }

        private string CommanderUnitLabel()
        {
            UnitState commander = FirstPlayerUnit();
            if (commander == null)
            {
                return "--";
            }

            string label = string.IsNullOrWhiteSpace(commander.UnitType) ? commander.Id : commander.UnitType;
            return TruncateText(label, 10);
        }

        private UnitState FirstPlayerUnit()
        {
            if (mission == null)
            {
                return null;
            }

            foreach (UnitState unit in mission.PlayerUnits())
            {
                return unit;
            }

            return null;
        }

        private bool HasRecentCombatEvent()
        {
            if (mission == null || lastCombatEventTimeSeconds < 0f)
            {
                return false;
            }

            return mission.MissionTimeSeconds - lastCombatEventTimeSeconds <= 4f;
        }

        private bool HasRecentContactEvent()
        {
            if (mission == null || lastContactEventTimeSeconds < 0f)
            {
                return false;
            }

            return mission.MissionTimeSeconds - lastContactEventTimeSeconds <= 8f;
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

            if (GUI.Button(new Rect(panel.xMax - 66f, panel.y + 6f, 52f, 24f), MissionMapBackButtonLabel))
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
            DrawModalBackdrop();
            DrawDesignPanelFrame(panel, LoadoutPanelFrameTitle, UiAmberColor);
            float x = panel.x + 14f;
            float y = panel.y + 34f;
            float width = panel.width - 28f;

            GUI.Label(new Rect(x, y, width - 76f, 22f), LoadoutPanelHeaderTitle, uiHeaderStyle);
            if (GUI.Button(new Rect(panel.xMax - 70f, panel.y + 7f, 56f, 24f), MechLabBackButtonLabel))
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

                statusText = "Mech Lab closed";
                SetDemoFlowScreen(DemoFlowScreen.Battle);
            }

            y += 30f;
            if (ShouldUseDedicatedMechLabLayout(panel))
            {
                DrawDedicatedMechLabSurface(panel, x, y, width);
                return;
            }

            DrawCompactMechLabSurface(panel, x, y, width);
        }

        private void DrawCompactMechLabSurface(Rect panel, float x, float y, float width)
        {
            DrawMechBayInventorySummary(x, y, width, false);
            y += 178f;
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

            DrawSelectedMechLabLoadoutEditor(x, y, width, panel.yMax - 12f);
        }

        private void DrawDedicatedMechLabSurface(Rect panel, float x, float y, float width)
        {
            float gap = 14f;
            float columnHeight = Mathf.Max(260f, panel.yMax - y - 14f);
            float bayWidth = MechLabDedicatedBayWidthFor(width, gap);
            float loadoutWidth = width - bayWidth - gap;
            Rect bayColumn = new(x, y, bayWidth, columnHeight);
            Rect loadoutColumn = new(bayColumn.xMax + gap, y, loadoutWidth, columnHeight);

            DrawMechLabColumnFrame(bayColumn, MechLabCompanyBayPanelTitle, UiAmberColor);
            DrawMechLabColumnFrame(loadoutColumn, MechLabLoadoutPanelTitle, UiCyanColor);

            float bayInset = 10f;
            Rect bayViewport = new(
                bayColumn.x + bayInset,
                bayColumn.y + 28f,
                bayColumn.width - bayInset * 2f,
                bayColumn.height - 38f);
            selectedMechLabBayPage = Mathf.Clamp(
                selectedMechLabBayPage,
                MechLabBayOpsPageIndex,
                MechLabBayRosterPageIndex);
            float pageY = 136f;
            float pageHeight = selectedMechLabBayPage == MechLabBayRosterPageIndex ? 324f : 194f;
            float previewHeight = showWarehouseDraftFitPreview
                ? 118f
                : showSquadSelectionPreview ? 252f : 0f;
            Rect bayContent = new(
                0f,
                0f,
                bayViewport.width - 20f,
                Mathf.Max(bayViewport.height, pageY + pageHeight + previewHeight + 22f));
            mechLabBayScroll = GUI.BeginScrollView(bayViewport, mechLabBayScroll, bayContent);
            DrawMechBayInventorySummary(0f, 0f, bayContent.width, false);
            DrawDedicatedMechLabBayPages(0f, pageY, bayContent.width);
            float previewY = pageY + pageHeight + 10f;
            if (showWarehouseDraftFitPreview)
            {
                DrawWarehouseDraftFitPreview(0f, previewY, bayContent.width);
            }
            else if (showSquadSelectionPreview)
            {
                DrawSquadSelectionPreview(0f, previewY, bayContent.width);
            }

            GUI.EndScrollView();

            float loadoutInset = 10f;
            DrawSelectedMechLabLoadoutEditor(
                loadoutColumn.x + loadoutInset,
                loadoutColumn.y + 28f,
                loadoutColumn.width - loadoutInset * 2f,
                loadoutColumn.yMax - 10f);
        }

        private void DrawDedicatedMechLabBayPages(float x, float y, float width)
        {
            Rect tabStrip = new(x, y - 2f, width, 26f);
            DrawColorRect(tabStrip, new Color(0.015f, 0.025f, 0.03f, 0.74f));
            DrawRectBorder(tabStrip, new Color(UiAmberColor.r, UiAmberColor.g, UiAmberColor.b, 0.36f), 1f);

            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled;
            if (GUI.Button(new Rect(x + 4f, y, 58f, 22f), MechLabBayOpsPageLabel))
            {
                selectedMechLabBayPage = MechLabBayOpsPageIndex;
                mechLabBayScroll = Vector2.zero;
                statusText = "Company Bay: Ops";
            }

            if (GUI.Button(new Rect(x + 68f, y, 70f, 22f), MechLabBayRosterPageLabel))
            {
                selectedMechLabBayPage = MechLabBayRosterPageIndex;
                mechLabBayScroll = Vector2.zero;
                statusText = "Company Bay: Roster";
            }

            GUI.enabled = previousEnabled;
            Rect selectedTab = selectedMechLabBayPage == MechLabBayRosterPageIndex
                ? new Rect(x + 68f, y, 70f, 22f)
                : new Rect(x + 4f, y, 58f, 22f);
            DrawRectBorder(selectedTab, UiAmberColor, 2f);

            float contentY = y + 32f;
            if (selectedMechLabBayPage == MechLabBayRosterPageIndex)
            {
                DrawDedicatedMechLabRosterPage(x, contentY, width);
                return;
            }

            DrawDedicatedMechLabOpsPage(x, contentY, width);
        }

        private void DrawDedicatedMechLabOpsPage(float x, float y, float width)
        {
            MechBayWeaponShopPreview shopPreview = MechBayWeaponShopPreviewService.BuildPreview(demoInventory);
            GUI.Label(new Rect(x, y, width, 18f), "Shop " + WeaponShopPreviewText(shopPreview));
            DrawWeaponShopPurchaseStub(x, y + 22f, width, shopPreview, demoInventory);

            MechBayPilotHirePreview pilotHirePreview = MechBayPilotHirePreviewService.BuildPreview(demoInventory);
            GUI.Label(new Rect(x, y + 48f, width, 18f), "Pilot Hire " + PilotHirePreviewText(pilotHirePreview));

            MechBayMissionHandoffPreview handoffPreview =
                MechBayMissionHandoffPreviewService.BuildPreview(demoInventory);
            MechBayMissionRestartApplyGuard restartGuard =
                MechBayMissionHandoffPreviewService.BuildRestartApplyGuard(
                    demoInventory,
                    mission?.Contract,
                    combatProfiles);
            string handoffSummary = HasSquadSelectionCompletedReplacement()
                ? MissionHandoffCompletedSummaryText(handoffPreview, restartGuard)
                : MissionHandoffPlayerSummaryText(handoffPreview, restartGuard);
            GUI.Label(
                new Rect(x, y + 76f, width, 18f),
                "Next Contract " + TruncateText(handoffSummary, 62));
            DrawMissionHandoffLaunchAction(x, y + 100f, width, handoffPreview, restartGuard);
            DrawMissionHandoffLineup(x, y + 126f, width, handoffPreview);
        }

        private void DrawDedicatedMechLabRosterPage(float x, float y, float width)
        {
            MechBayOwnedRosterEntry[] roster = MechBayOwnedRosterService.BuildRosterPreview(demoInventory);
            ClampSelectedRosterIndex(roster);
            GUI.Label(new Rect(x, y, width, 18f), "Roster " + TruncateText(OwnedRosterText(roster), 62));
            DrawRosterMissionStateLine(x, y + 22f, width, roster);
            MechBayPilotHirePreview pilotHirePreview = MechBayPilotHirePreviewService.BuildPreview(demoInventory);
            DrawOwnedRosterDetail(x, y + 48f, width, roster, pilotHirePreview);
        }

        private void DrawSelectedMechLabLoadoutEditor(float x, float y, float width, float bottomY)
        {
            UnitState selectedLoadoutUnit = SelectedMechBayLoadoutUnit();
            int unitCount = CountPlayerUnits();
            if (unitCount > 0)
            {
                DrawMechBayLoadoutUnitSelector(x, y, width, selectedLoadoutUnit);
                y += 52f;
            }

            float viewportHeight = Mathf.Max(40f, bottomY - y);
            Rect viewport = new(x, y, width, viewportHeight);
            float contentHeight = selectedLoadoutUnit == null ? viewport.height : LoadoutCardStride;
            Rect content = new(0f, 0f, width - 20f, Mathf.Max(viewport.height, contentHeight));
            loadoutScroll = GUI.BeginScrollView(viewport, loadoutScroll, content);
            if (selectedLoadoutUnit == null)
            {
                GUI.Label(new Rect(0f, 0f, content.width, 18f), "No player mechs available");
            }
            else
            {
                DrawLoadoutUnit(selectedLoadoutUnit, 0f, 0f, content.width);
            }

            GUI.EndScrollView();
        }

        private void DrawMechLabColumnFrame(Rect rect, string title, Color accent)
        {
            DrawColorRect(rect, new Color(0.015f, 0.025f, 0.03f, 0.74f));
            DrawRectBorder(rect, new Color(accent.r, accent.g, accent.b, 0.42f), 1f);
            GUI.Label(new Rect(rect.x + 10f, rect.y + 6f, rect.width - 20f, 18f), title);
        }

        private static float MechLabDedicatedBayWidthFor(float availableWidth, float gap)
        {
            float bayWidth = Mathf.Clamp(
                availableWidth * 0.40f,
                MechLabDedicatedBayMinWidth,
                MechLabDedicatedBayMaxWidth);
            float loadoutWidth = availableWidth - bayWidth - gap;
            if (loadoutWidth < MechLabDedicatedLoadoutMinWidth)
            {
                bayWidth = Mathf.Max(
                    MechLabDedicatedBayMinWidth,
                    availableWidth - gap - MechLabDedicatedLoadoutMinWidth);
            }

            return bayWidth;
        }

        private void DrawMechBayInventorySummary(float x, float y, float width, bool showRosterDetail)
        {
            if (demoInventoryValidation == null)
            {
                GUI.Label(new Rect(x, y, width, 18f), MechLabLoadingText);
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
                    ? MechLabReadyLabel
                    : MechLabReviewPrefix + TruncateText(InventoryStatusError(availability), 28));
            GUI.color = previous;

            GUI.Label(
                new Rect(x + width * 0.48f, y, width * 0.50f, 18f),
                MechLabFundsText(summary.TokenBalance));
            GUI.Label(
                new Rect(x, y + 20f, width, 18f),
                "Mechs " + summary.MechCount.ToString(CultureInfo.InvariantCulture)
                + "  Weapons " + summary.WeaponCount.ToString(CultureInfo.InvariantCulture)
                + "  Armor " + InventoryUseText(availability?.Usage?.ArmorPlateCount ?? 0, availability?.AvailableArmorPlateCount ?? summary.ArmorPlateCount)
                + "  Sinks " + InventoryUseText(availability?.Usage?.HeatSinkCount ?? 0, availability?.AvailableHeatSinkCount ?? summary.HeatSinkCount)
                + "  " + MechLabPartsLabel + " " + summary.MechFragmentCount.ToString(CultureInfo.InvariantCulture));
            MechBaySavedAccountContract accountSnapshot = MechBaySavedAccountService.BuildDemoSnapshot(demoInventory);
            GUI.Label(
                new Rect(x, y + 40f, width, 18f),
                MechLabCompanyPrefix + TruncateText(MechBaySavedAccountService.SummaryText(accountSnapshot), 70));
            GUI.Label(
                new Rect(x, y + 60f, width, 18f),
                MechLabBuildPrefix + AssemblyPreviewText(MechBayAssemblyPreviewService.BestAssemblyProgress(demoInventory)));
            MechBayMissionHandoffPreview handoffPreview =
                MechBayMissionHandoffPreviewService.BuildPreview(demoInventory);
            MechBayMissionRestartApplyGuard restartGuard =
                MechBayMissionHandoffPreviewService.BuildRestartApplyGuard(
                    demoInventory,
                    mission?.Contract,
                    combatProfiles);
            float actionY = y + 82f;
            if (SaveGameUiEnabled)
            {
                DrawSavedAccountFileResultLine(x, actionY, width);
                actionY += 22f;
            }

            DrawPostBattleMechBayLane(x, actionY, width, handoffPreview, restartGuard);
            DrawLocalCandidatePrepAction(x, actionY + 24f, width);
            if (!showRosterDetail)
            {
                return;
            }

            float detailY = actionY + 48f;
            if (SaveGameUiEnabled)
            {
                DrawSavedAccountImportPathToolsLine(x, detailY, width);
                DrawSavedAccountImportPreviewPathLine(x, detailY + 22f, width);
                DrawSavedAccountImportApplyPreviewLine(x, detailY + 44f, width);
                detailY += 64f;
            }

            MechBayWeaponShopPreview shopPreview = MechBayWeaponShopPreviewService.BuildPreview(demoInventory);
            GUI.Label(
                new Rect(x, detailY, width, 18f),
                "Shop " + WeaponShopPreviewText(shopPreview));
            DrawWeaponShopPurchaseStub(x, detailY + 22f, width, shopPreview, demoInventory);
            MechBayPilotHirePreview pilotHirePreview = MechBayPilotHirePreviewService.BuildPreview(demoInventory);
            GUI.Label(
                new Rect(x, detailY + 44f, width, 18f),
                "Pilot Hire " + PilotHirePreviewText(pilotHirePreview));
            MechBayOwnedRosterEntry[] roster = MechBayOwnedRosterService.BuildRosterPreview(demoInventory);
            ClampSelectedRosterIndex(roster);
            GUI.Label(
                new Rect(x, detailY + 66f, width, 18f),
                "Roster " + TruncateText(OwnedRosterText(roster), 62));
            DrawRosterMissionStateLine(x, detailY + 88f, width, roster);
            string handoffSummary = HasSquadSelectionCompletedReplacement()
                ? MissionHandoffCompletedSummaryText(handoffPreview, restartGuard)
                : MissionHandoffPlayerSummaryText(handoffPreview, restartGuard);
            GUI.Label(
                new Rect(x, detailY + 110f, width, 18f),
                "Next Contract " + TruncateText(handoffSummary, 58));
            DrawMissionHandoffLaunchAction(x, detailY + 132f, width, handoffPreview, restartGuard);
            DrawMissionHandoffLineup(x, detailY + 154f, width, handoffPreview);
            if (showRosterDetail)
            {
                DrawOwnedRosterDetail(x, detailY + 176f, width, roster, pilotHirePreview);
            }
        }

        private void DrawPostBattleMechBayLane(
            float x,
            float y,
            float width,
            MechBayMissionHandoffPreview handoffPreview,
            MechBayMissionRestartApplyGuard restartGuard)
        {
            int repairCount = CountRepairNeededPlayerMechs();
            int repairCost = EstimateRepairAllPlayerMechCost();
            bool repairReady = repairCount > 0
                && repairCost > 0
                && demoInventory != null
                && demoInventory.tokenBalance >= repairCost;
            bool launchReady = restartGuard?.ApplyEnabled == true;
            bool previousEnabled = GUI.enabled;
            Rect laneRect = new(x - 4f, y - 5f, width + 8f, 30f);
            DrawColorRect(laneRect, new Color(0.015f, 0.025f, 0.03f, 0.82f));
            DrawRectBorder(laneRect, new Color(UiAmberColor.r, UiAmberColor.g, UiAmberColor.b, 0.45f), 1f);

            float buttonX = x;
            GUI.enabled = previousEnabled && repairReady;
            if (DrawActionButton(new Rect(buttonX, y - 2f, 78f, 22f), "Repair All", repairReady))
            {
                TryRepairAllPlayerMechs();
            }

            buttonX += 86f;

            GUI.enabled = previousEnabled;
            if (DrawActionButton(new Rect(buttonX, y - 2f, 68f, 22f), "Contracts", true))
            {
                OpenMissionListFromMechBayLane();
            }

            buttonX += 76f;

            GUI.enabled = previousEnabled && launchReady;
            if (DrawActionButton(new Rect(buttonX, y - 2f, 58f, 22f), "Launch", launchReady))
            {
                TryApplyMissionRestartRuntimeSwap(keepMechBayOpen: true);
            }

            buttonX += 68f;
            GUI.enabled = previousEnabled;
            bool laneReady = repairCount > 0 ? repairReady : launchReady;
            DrawActionStateLabel(
                buttonX,
                y,
                width - (buttonX - x),
                PostBattleMechBayLaneText(repairCount, repairCost, handoffPreview, restartGuard),
                laneReady,
                54);
        }

        private string PostBattleMechBayLaneText(
            int repairCount,
            int repairCost,
            MechBayMissionHandoffPreview handoffPreview,
            MechBayMissionRestartApplyGuard restartGuard)
        {
            string prefix = mission != null && mission.Result != MissionResultState.InProgress
                ? "After Action"
                : "Ready Bay";
            if (demoInventory == null)
            {
                return prefix + "  " + MechLabUnavailableText;
            }

            if (repairCount > 0)
            {
                if (demoInventory.tokenBalance < repairCost)
                {
                    return prefix + "  repair needed  "
                        + repairCount.ToString(CultureInfo.InvariantCulture)
                        + " mechs  need "
                        + FormatTokens(repairCost);
                }

                return prefix + "  repair "
                    + repairCount.ToString(CultureInfo.InvariantCulture)
                    + " mechs  "
                    + FormatTokens(repairCost)
                    + " then launch";
            }

            if (restartGuard?.ApplyEnabled == true)
            {
                int slotCount = handoffPreview?.MissionSlotCount ?? restartGuard.SpawnIntentCount;
                return prefix + "  squad ready  "
                    + slotCount.ToString(CultureInfo.InvariantCulture)
                    + " mechs";
            }

            return prefix + "  choose ready squad  " + MissionHandoffPlayerBlockedReason(restartGuard?.Reason);
        }

        private int CountRepairNeededPlayerMechs()
        {
            int count = 0;
            if (mission == null)
            {
                return count;
            }

            foreach (UnitState unit in mission.PlayerUnits())
            {
                if (MechBayRepairService.EstimateRepairCostResourcePoints(unit) > 0)
                {
                    count++;
                }
            }

            return count;
        }

        private int EstimateRepairAllPlayerMechCost()
        {
            int total = 0;
            if (mission == null)
            {
                return total;
            }

            foreach (UnitState unit in mission.PlayerUnits())
            {
                total += MechBayRepairService.EstimateRepairCostResourcePoints(unit);
            }

            return total;
        }

        private void TryRepairAllPlayerMechs()
        {
            if (demoInventory == null)
            {
                statusText = MechLabUnavailableText;
                AddCombatLogLine(statusText);
                return;
            }

            int repairCount = CountRepairNeededPlayerMechs();
            int repairCost = EstimateRepairAllPlayerMechCost();
            if (repairCount <= 0 || repairCost <= 0)
            {
                statusText = "No repairs needed";
                AddCombatLogLine(statusText);
                return;
            }

            if (demoInventory.tokenBalance < repairCost)
            {
                statusText = RepairAllBlockedFundsText(repairCost);
                AddCombatLogLine(statusText);
                return;
            }

            int repaired = 0;
            int spent = 0;
            foreach (UnitState unit in mission.PlayerUnits())
            {
                int unitCost = MechBayRepairService.EstimateRepairCostResourcePoints(unit);
                if (unitCost <= 0)
                {
                    continue;
                }

                MechBayRepairResult result = MechBayRepairService.TryRepair(demoInventory, unit);
                if (!result.Accepted)
                {
                    statusText = result.Message;
                    AddCombatLogLine("Repair all stopped: " + result.Message);
                    break;
                }

                repaired++;
                spent += unitCost;
            }

            RefreshDemoInventoryValidation();
            bool saved = TryAutoSaveSavedAccount("repair all");
            if (saved)
            {
                statusText = RepairAllCompleteStatusText(repaired, spent);
            }

            AddCombatLogLine(statusText);
        }

        private static string RepairAllBlockedFundsText(int repairCost)
        {
            return "Need " + FundsCostText(repairCost) + " for all repairs";
        }

        private static string RepairAllCompleteStatusText(int repaired, int spent)
        {
            int safeRepaired = Math.Max(0, repaired);
            string mechLabel = safeRepaired == 1 ? " mech" : " mechs";
            return "Repaired "
                + safeRepaired.ToString(CultureInfo.InvariantCulture)
                + mechLabel
                + " for "
                + FundsCostText(spent);
        }

        private void TrySaveCurrentFromMechBayLane()
        {
            bool saved = TryExportSavedAccount(DefaultSavedAccountFilePath(), false, "Mech Lab lane");
            statusText = saved ? "Progress saved" : "Save failed";
        }

        private void OpenMissionListFromMechBayLane()
        {
            showLoadoutPanel = false;
            OpenMissionListPanelFromSystem();
            statusText = ContractsOpenStatusText + " from Mech Lab";
        }

        private UnitState SelectedMechBayLoadoutUnit()
        {
            if (mission == null)
            {
                selectedMechBayLoadoutUnitId = null;
                return null;
            }

            UnitState firstUnit = null;
            foreach (UnitState unit in mission.PlayerUnits())
            {
                firstUnit ??= unit;
                if (unit != null
                    && !string.IsNullOrEmpty(selectedMechBayLoadoutUnitId)
                    && string.Equals(unit.Id, selectedMechBayLoadoutUnitId, StringComparison.OrdinalIgnoreCase))
                {
                    return unit;
                }
            }

            selectedMechBayLoadoutUnitId = firstUnit?.Id;
            return firstUnit;
        }

        private void DrawMechBayLoadoutUnitSelector(float x, float y, float width, UnitState selectedUnit)
        {
            Rect strip = new(x - 4f, y - 4f, width + 8f, 48f);
            DrawColorRect(strip, new Color(0.015f, 0.025f, 0.03f, 0.82f));
            DrawRectBorder(strip, new Color(UiCyanColor.r, UiCyanColor.g, UiCyanColor.b, 0.36f), 1f);
            GUI.Label(new Rect(x, y, 68f, 18f), "Mech Lab");

            int unitCount = CountPlayerUnits();
            float gap = 5f;
            float startX = x + 74f;
            float buttonWidth = unitCount <= 0
                ? 0f
                : Mathf.Clamp((width - 74f - gap * (unitCount - 1)) / unitCount, 58f, 108f);
            int index = 0;
            foreach (UnitState unit in mission.PlayerUnits())
            {
                if (unit == null)
                {
                    continue;
                }

                bool selected = selectedUnit != null
                    && string.Equals(unit.Id, selectedUnit.Id, StringComparison.OrdinalIgnoreCase);
                bool draft = HasPendingLoadoutEdits(unit);
                Color previous = GUI.color;
                GUI.color = selected
                    ? new Color(0.42f, 0.82f, 1f, 1f)
                    : draft
                        ? new Color(1f, 0.82f, 0.30f, 0.95f)
                        : new Color(0.82f, 0.90f, 0.92f, 0.92f);

                Rect button = new(startX + index * (buttonWidth + gap), y - 2f, buttonWidth, 22f);
                string label = (index + 1).ToString(CultureInfo.InvariantCulture)
                    + " "
                    + TruncateText(unit.UnitType, buttonWidth < 74f ? 5 : 8)
                    + (draft ? "*" : "");
                if (GUI.Button(button, label))
                {
                    selectedMechBayLoadoutUnitId = unit.Id;
                    loadoutScroll = Vector2.zero;
                    statusText = "Mech Lab focus: " + TruncateText(unit.UnitType, 24);
                }

                GUI.color = previous;
                if (selected)
                {
                    DrawRectBorder(button, UiCyanColor, 2f);
                }
                else if (draft)
                {
                    DrawRectBorder(button, UiAmberColor, 1f);
                }

                index++;
            }

            if (selectedUnit != null)
            {
                CombatLoadoutPreview preview = LoadoutPreviewFor(selectedUnit);
                LoadoutValidationResult result = preview.Validation;
                string identity = MechBayFitIdentityText(selectedUnit, preview, HasPendingLoadoutEdits(selectedUnit));
                DrawSelectedMechBayFitPressureLine(
                    x,
                    y + 24f,
                    width,
                    identity,
                    result.IsValid && !HasPendingLoadoutEdits(selectedUnit),
                    preview,
                    result);
            }
        }

        private void DrawSelectedMechBayFitPressureLine(
            float x,
            float y,
            float width,
            string text,
            bool ready,
            CombatLoadoutPreview preview,
            LoadoutValidationResult result)
        {
            Color cue = ready
                ? new Color(0.42f, 0.82f, 1f, 0.92f)
                : new Color(1f, 0.62f, 0.26f, 0.92f);
            Rect cueRect = new(x, y + 4f, 10f, 10f);
            DrawColorRect(cueRect, cue);
            DrawRectBorder(cueRect, new Color(0.02f, 0.025f, 0.03f, 0.95f), 1f);

            float barsWidth = Mathf.Clamp(width * 0.34f, 118f, 164f);
            float textWidth = Mathf.Max(60f, width - barsWidth - 22f);
            GUI.Label(new Rect(x + 16f, y, textWidth, 18f), TruncateText(text, 58));

            float barX = x + width - barsWidth;
            float barWidth = Mathf.Max(24f, (barsWidth - 42f) / 3f);
            DrawMechBayFitPressureBar(ref barX, y, "H", result.TotalHeat, preview.HeatLimit, UiCyanColor, barWidth);
            DrawMechBayFitPressureBar(ref barX, y, "W", result.TotalWeight, preview.WeightLimit, UiAmberColor, barWidth);
            DrawMechBayFitPressureBar(
                ref barX,
                y,
                "G",
                result.OccupiedGridCells,
                preview.GridCapacity,
                LoadoutComponentColor,
                barWidth);
        }

        private static string MechBayFitIdentityText(
            UnitState selectedUnit,
            CombatLoadoutPreview preview,
            bool hasPendingEdits)
        {
            if (selectedUnit == null)
            {
                return "";
            }

            string fitState = hasPendingEdits ? "Pending" : "Applied";
            string fitReview = ProjectedLoadoutStateText(preview);
            float maxStructure = selectedUnit.Profile == null
                ? selectedUnit.CurrentStructure
                : selectedUnit.Profile.MaxStructure;
            return TruncateText(selectedUnit.UnitType, 18)
                + "  "
                + fitState
                + "  "
                + fitReview
                + "  S "
                + Mathf.RoundToInt(selectedUnit.CurrentStructure).ToString(CultureInfo.InvariantCulture)
                + "/"
                + Mathf.RoundToInt(maxStructure).ToString(CultureInfo.InvariantCulture);
        }

        private void DrawMechBayFitPressureBar(
            ref float x,
            float y,
            string label,
            float value,
            float limit,
            Color color,
            float width)
        {
            GUI.Label(new Rect(x, y, 10f, 18f), label);
            DrawLoadoutUsageBar(new Rect(x + 12f, y + 7f, width, 5f), value, limit, color);
            x += width + 21f;
        }

        private void DrawLoadoutUnit(UnitState unit, float x, float y, float width)
        {
            Rect card = new(x, y, width, LoadoutCardHeight);
            GUI.Box(card, TruncateText(LoadoutUnitTitle(unit), 82));

            float left = x + 12f;
            float right = x + width * 0.50f;
            float lineY = y + 24f;
            CombatLoadoutPreview loadoutPreview = LoadoutPreviewFor(unit);
            DrawLoadoutUnitCombatSummary(unit, left, right, lineY, width);

            lineY += 20f;
            DrawMechConditionLine(unit, left, right, lineY, width);

            lineY += 20f;
            DrawProjectedLoadoutStatus(loadoutPreview, left, right, lineY, width);

            lineY += 24f;
            DrawLoadoutSectionLine(unit, left, lineY, width - 24f);

            lineY += 34f;
            float gridHeight = DrawProjectedLoadoutGrid(unit, loadoutPreview, left, lineY, width - 24f);

            lineY += gridHeight + 8f;
            float weaponHeight = DrawWeaponLoadoutLines(unit, loadoutPreview, left, lineY, width - 24f);

            lineY += weaponHeight + 6f;
            DrawLoadoutEditControls(unit, loadoutPreview, left, lineY, width - 24f);
        }

        private void DrawLoadoutUnitCombatSummary(UnitState unit, float left, float right, float y, float width)
        {
            GUI.Label(
                new Rect(left, y, width * 0.46f, 18f),
                "STR "
                + Mathf.RoundToInt(unit.CurrentStructure).ToString(CultureInfo.InvariantCulture)
                + "/"
                + Mathf.RoundToInt(unit.Profile.MaxStructure).ToString(CultureInfo.InvariantCulture)
                + "  MV "
                + Mathf.RoundToInt(unit.Profile.MoveSpeed).ToString(CultureInfo.InvariantCulture)
                + "  "
                + TruncateText(unit.CombatPrimaryWeaponName, 18));
            GUI.Label(
                new Rect(right, y, width * 0.46f, 18f),
                "D "
                + FormatDecimal(unit.CombatWeaponDamage)
                + "  R "
                + Mathf.RoundToInt(unit.CombatWeaponRange).ToString(CultureInfo.InvariantCulture)
                + "  CD "
                + FormatDecimal(unit.CombatWeaponCooldown)
                + "  H "
                + FormatDecimal(unit.CurrentHeat)
                + "/"
                + FormatDecimal(unit.Profile.HeatCapacity)
                + "  W "
                + FormatDecimal(unit.CombatTotalWeaponWeight));
        }

        private static string LoadoutUnitTitle(UnitState unit)
        {
            if (unit == null)
            {
                return "Mech";
            }

            string title = "Fit " + unit.UnitType;
            if (!string.IsNullOrWhiteSpace(unit.PilotDisplayName))
            {
                title += "  " + TruncateText(unit.PilotDisplayName, 14);
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
                ProjectedLoadoutStateText(preview));
            GUI.color = previous;

            GUI.Label(
                new Rect(right, y, width * 0.46f, 18f),
                ProjectedLoadoutPressureText(preview));
            float meterWidth = width * 0.46f;
            float meterGap = 6f;
            float thirdMeter = Mathf.Max(26f, (meterWidth - meterGap * 2f) / 3f);
            DrawLoadoutUsageBar(
                new Rect(right, y + 17f, thirdMeter, 5f),
                result.TotalHeat,
                preview.HeatLimit,
                UiCyanColor);
            DrawLoadoutUsageBar(
                new Rect(right + thirdMeter + meterGap, y + 17f, thirdMeter, 5f),
                result.TotalWeight,
                preview.WeightLimit,
                UiAmberColor);
            DrawLoadoutUsageBar(
                new Rect(right + (thirdMeter + meterGap) * 2f, y + 17f, thirdMeter, 5f),
                result.OccupiedGridCells,
                preview.GridCapacity,
                LoadoutComponentColor);
        }

        private static string ProjectedLoadoutStateText(CombatLoadoutPreview preview)
        {
            LoadoutValidationResult result = preview?.Validation;
            if (result == null)
            {
                return LoadoutFitReviewPrefix + "Missing";
            }

            return result.IsValid
                ? LoadoutFitOkLabel
                : LoadoutFitReviewPrefix + ProjectedLoadoutReviewReasonText(result);
        }

        private static string ProjectedLoadoutPressureText(CombatLoadoutPreview preview)
        {
            LoadoutValidationResult result = preview?.Validation;
            if (preview == null || result == null)
            {
                return LoadoutHeatPressureLabel + "-/-  "
                    + LoadoutWeightPressureLabel + "-/-  "
                    + LoadoutGridPressureLabel + "-/-";
            }

            return LoadoutHeatPressureLabel + LoadoutPressureValueText(result.TotalHeat, preview.HeatLimit)
                + "  " + LoadoutWeightPressureLabel + LoadoutPressureValueText(result.TotalWeight, preview.WeightLimit)
                + "  " + LoadoutGridPressureLabel + LoadoutPressureValueText(result.OccupiedGridCells, preview.GridCapacity);
        }

        private static string ProjectedLoadoutReviewReasonText(LoadoutValidationResult result)
        {
            if (result == null)
            {
                return "Missing";
            }

            List<string> reasons = new();
            AddCompactLoadoutIssue(reasons, result, "weight", "Weight");
            AddCompactLoadoutIssue(reasons, result, "heat", "Heat");
            AddCompactLoadoutIssue(reasons, result, "grid", "Grid");
            AddCompactLoadoutIssue(reasons, result, "cell", "Grid");
            AddCompactLoadoutIssue(reasons, result, "slot", "Slot");
            AddCompactLoadoutIssue(reasons, result, "unknown chassis", "Chassis");
            AddCompactLoadoutIssue(reasons, result, "item", "Item");
            if (reasons.Count > 0)
            {
                return TruncateText(string.Join("+", reasons), 18);
            }

            string reason = PlayerFitStatusText(FirstLoadoutError(result));
            if (reason.StartsWith("Fit ", StringComparison.Ordinal))
            {
                reason = reason.Substring(4);
            }

            if (reason.Length > 0)
            {
                reason = char.ToUpperInvariant(reason[0]) + reason.Substring(1);
            }

            return string.IsNullOrWhiteSpace(reason) ? "Blocked" : TruncateText(reason, 18);
        }

        private static void AddCompactLoadoutIssue(
            List<string> reasons,
            LoadoutValidationResult result,
            string needle,
            string label)
        {
            if (reasons.Contains(label))
            {
                return;
            }

            foreach (string error in result.Errors)
            {
                if (!string.IsNullOrWhiteSpace(error)
                    && error.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    reasons.Add(label);
                    return;
                }
            }
        }

        private static string LoadoutPressureValueText(float value, float limit)
        {
            string text = FormatDecimal(value) + "/" + FormatDecimal(limit);
            return limit > 0f && value > limit ? text + "!" : text;
        }

        private void DrawLoadoutUsageBar(Rect rect, float value, float limit, Color normalColor)
        {
            DrawColorRect(rect, UiTrackColor);
            float ratio = limit <= 0f ? 0f : Mathf.Clamp01(value / limit);
            Color fillColor = value <= limit ? normalColor : new Color(1f, 0.22f, 0.12f, 0.96f);
            DrawColorRect(new Rect(rect.x, rect.y, rect.width * ratio, rect.height), fillColor);
            DrawRectBorder(rect, new Color(0.18f, 0.28f, 0.30f, 0.92f), 1f);
        }

        private CombatLoadoutPreview LoadoutPreviewFor(UnitState unit)
        {
            string key = unit?.UnitType ?? "";
            return CombatLoadoutPreviewBuilder.Build(
                key,
                unit?.Profile,
                AllMountedWeaponsStateFor(unit),
                LoadoutPlacementOverridesFor(unit),
                LoadoutFillerOverridesFor(unit));
        }

        private CombatLoadoutPreview LoadoutBasePreviewFor(UnitState unit)
        {
            string key = unit?.UnitType ?? "";
            return CombatLoadoutPreviewBuilder.Build(
                key,
                unit?.Profile,
                AllMountedWeaponsStateFor(unit),
                null,
                null);
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
                return "No parts";
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
                return MechLabRosterEmptyText;
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
                    label += " reserve";
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
                return MechLabRosterEmptyText;
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
            GUI.Label(new Rect(x, y, width, 18f), MechLabRosterStateLabel + TruncateText(RosterMissionStateText(state), 72));
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

        private struct DebriefSummaryAssertionResult
        {
            public bool Accepted { get; set; }
            public string Summary { get; set; }

            public static DebriefSummaryAssertionResult Blocked(string summary)
            {
                return new DebriefSummaryAssertionResult
                {
                    Accepted = false,
                    Summary = summary ?? "blocked"
                };
            }
        }

        private struct DebriefVisibleAssertionResult
        {
            public bool Accepted { get; set; }
            public string Summary { get; set; }

            public static DebriefVisibleAssertionResult Blocked(string summary)
            {
                return new DebriefVisibleAssertionResult
                {
                    Accepted = false,
                    Summary = summary ?? "blocked"
                };
            }
        }

        private struct CommandResultAssertionResult
        {
            public bool Accepted { get; set; }
            public string Summary { get; set; }

            public static CommandResultAssertionResult Blocked(string summary)
            {
                return new CommandResultAssertionResult
                {
                    Accepted = false,
                    Summary = summary ?? "blocked"
                };
            }
        }

        private struct CombatSituationAssertionResult
        {
            public bool Accepted { get; set; }
            public string Summary { get; set; }

            public static CombatSituationAssertionResult Blocked(string summary)
            {
                return new CombatSituationAssertionResult
                {
                    Accepted = false,
                    Summary = summary ?? "blocked"
                };
            }
        }

        private struct EncounterPacingAssertionResult
        {
            public bool Accepted { get; set; }
            public string Summary { get; set; }

            public static EncounterPacingAssertionResult Blocked(string summary)
            {
                return new EncounterPacingAssertionResult
                {
                    Accepted = false,
                    Summary = summary ?? "blocked"
                };
            }
        }

        private struct ObjectiveGraphAssertionResult
        {
            public bool Accepted { get; set; }
            public string Summary { get; set; }

            public static ObjectiveGraphAssertionResult Blocked(string summary)
            {
                return new ObjectiveGraphAssertionResult
                {
                    Accepted = false,
                    Summary = summary ?? "blocked"
                };
            }
        }

        private struct EncounterPacingCounts
        {
            public int Active { get; set; }
            public int Destroyed { get; set; }
            public int Total { get; set; }
            public string Summary => Active.ToString(CultureInfo.InvariantCulture) + "/" + Total.ToString(CultureInfo.InvariantCulture);
            public string DestroyedSummary => Destroyed.ToString(CultureInfo.InvariantCulture) + "/" + Total.ToString(CultureInfo.InvariantCulture);
        }

        private struct LoadoutCompactAssertionResult
        {
            public bool Accepted { get; set; }
            public string Summary { get; set; }

            public static LoadoutCompactAssertionResult Blocked(string summary)
            {
                return new LoadoutCompactAssertionResult
                {
                    Accepted = false,
                    Summary = summary ?? "blocked"
                };
            }
        }

        private readonly struct LoadoutCompactCheck
        {
            public LoadoutCompactCheck(bool accepted, string summary)
            {
                Accepted = accepted;
                Summary = summary ?? "";
            }

            public bool Accepted { get; }
            public string Summary { get; }
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
            string depot = preview.IncludesDepotMissionSlot ? "reserve included" : "current squad";
            return state
                + "  "
                + slots.Length.ToString(CultureInfo.InvariantCulture)
                + " mechs  "
                + depot;
        }

        private string MissionHandoffCompletedSummaryText(
            MechBayMissionHandoffPreview preview,
            MechBayMissionRestartApplyGuard guard)
        {
            string state = guard?.ApplyEnabled == true ? "Ready" : "Blocked";
            int slotCount = preview?.MissionSlotCount ?? guard?.SpawnIntentCount ?? 0;
            return state
                + "  updated squad  "
                + slotCount.ToString(CultureInfo.InvariantCulture)
                + " mechs  "
                + SquadSelectionCompletedReplacementText();
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
            string text = HasSquadSelectionCompletedReplacement()
                ? MissionHandoffCompletedLaunchText(guard)
                : MissionHandoffLaunchActionText(guard, preview);
            DrawActionStateLabel(
                x + 66f,
                y,
                width - 66f,
                text,
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
                string depot = preview?.IncludesDepotMissionSlot == true ? "reserve in squad" : "current squad";
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

            string cost = cheapestCost == int.MaxValue ? "unknown" : FundsCostText(cheapestCost);
            return entries.Length.ToString(CultureInfo.InvariantCulture)
                + " ordinary weapons from "
                + cost
                + "  afford "
                + affordableCount.ToString(CultureInfo.InvariantCulture)
                + "  buy ready";
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
                statusText = PlayerMechLabActionStatusText(result?.Message ?? "Purchase unavailable");
                if (result != null && result.Accepted)
                {
                    AddCombatLogLine("Shop " + result.Message);
                    RefreshDemoInventoryValidation();
                    TryAutoSaveSavedAccount("weapon purchase");
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

            string cost = FundsCostText(purchasePreview.TokenCost);
            return purchasePreview.displayName
                + " "
                + cost
                + "  "
                + PlayerMechLabActionStatusText(purchasePreview.Message);
        }

        private static string PlayerMechLabActionStatusText(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return "";
            }

            string result = message;
            result = result.Replace("Ready demo purchase", "Ready to buy");
            result = result.Replace("Demo purchase", "Ready to buy");
            result = result.Replace("Ready demo hire", "Ready to hire");
            result = result.Replace("Demo hire", "Ready to hire");
            result = result.Replace("Inventory missing", MechLabUnavailableText);
            return result;
        }

        private static string PlayerRosterStatusText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            string result = text;
            result = result.Replace("Needs loadout", "Needs fit");
            result = result.Replace("No loadout", "No fit");
            result = result.Replace("Loadout", "Fit");
            result = result.Replace("loadout", "fit");
            result = result.Replace("Depot fit read-only", "Reserve fit locked");
            result = result.Replace("Held: needs depot fit", "Held: needs reserve fit");
            result = result.Replace("Held: depot mech", "Held reserve");
            result = result.Replace("Future depot deployment flow", "Reserve not ready");
            result = result.Replace("Ready for future fitting", "Ready to fit");
            result = result.Replace("Finish fit before selection", "Finish fit first");
            result = result.Replace("Locked: needs depot fit", "Locked: needs reserve fit");
            result = result.Replace("Depot selection locked", "Reserve selection locked");
            result = result.Replace("depot", "reserve");
            result = result.Replace("Depot", "Reserve");
            result = result.Replace(" for this demo", "");
            result = result.Replace("read-only", "locked");
            return result;
        }

        private void DrawSavedAccountFileResultLine(float x, float y, float width)
        {
            string text = string.IsNullOrWhiteSpace(lastSavedAccountFileResultText)
                ? SavedAccountIdleLabel
                : lastSavedAccountFileResultText;
            DrawActionStateLabel(
                x,
                y,
                width,
                SavedAccountResultPrefix + text,
                lastSavedAccountFileResultReady,
                66);
        }

        private void DrawSavedAccountImportPathToolsLine(float x, float y, float width)
        {
            bool previousEnabled = GUI.enabled;
            if (DrawActionButton(new Rect(x, y - 2f, 62f, 22f), SavedAccountSlotButtonLabel, true))
            {
                savedAccountImportPreviewInputPath = DefaultSavedAccountFilePath();
                statusText = SavedAccountPathReadyStatusText;
                RecordSavedAccountFileResult(SavedAccountDefaultPathResultText, false, SavedAccountFileName(savedAccountImportPreviewInputPath));
            }

            bool canExport = demoInventory != null
                && !string.IsNullOrWhiteSpace(savedAccountImportPreviewInputPath);
            GUI.enabled = previousEnabled && canExport;
            if (DrawActionButton(new Rect(x + 70f, y - 2f, 58f, 22f), "Export", canExport))
            {
                TryExportSavedAccount(savedAccountImportPreviewInputPath, false, "Mech Lab");
            }

            string defaultPath = DefaultSavedAccountFilePath();
            bool canLoadDefault = demoInventory != null && File.Exists(defaultPath);
            GUI.enabled = previousEnabled && canLoadDefault;
            if (DrawActionButton(new Rect(x + 136f, y - 2f, 48f, 22f), "Load", canLoadDefault))
            {
                TryLoadDefaultSavedAccount("Mech Lab default load");
            }

            GUI.enabled = previousEnabled;
            string pathText = string.IsNullOrWhiteSpace(savedAccountImportPreviewInputPath)
                ? SavedAccountNoSlotPathText
                : savedAccountImportPreviewInputPath;
            DrawActionStateLabel(
                x + 192f,
                y,
                width - 192f,
                SavedAccountSlotPathPrefix + pathText,
                canExport || canLoadDefault,
                42);
        }

        private void DrawSavedAccountImportPreviewPathLine(float x, float y, float width)
        {
            GUI.Label(new Rect(x, y, 52f, 18f), SavedAccountLoadPathLabel);
            float buttonWidth = 72f;
            float fieldX = x + 56f;
            float fieldWidth = Mathf.Max(48f, width - 56f - buttonWidth - 8f);
            savedAccountImportPreviewInputPath = GUI.TextField(
                new Rect(fieldX, y - 2f, fieldWidth, 22f),
                savedAccountImportPreviewInputPath ?? string.Empty,
                260);

            bool canPreview = !string.IsNullOrWhiteSpace(savedAccountImportPreviewInputPath);
            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && canPreview;
            if (DrawActionButton(
                    new Rect(fieldX + fieldWidth + 8f, y - 2f, buttonWidth, 22f),
                    SavedAccountCheckButtonLabel,
                    canPreview))
            {
                TryPreviewSavedAccountImportApply(savedAccountImportPreviewInputPath, false, "Mech Lab");
            }

            GUI.enabled = previousEnabled;
        }

        private void DrawSavedAccountImportApplyPreviewLine(float x, float y, float width)
        {
            bool hasPreview = !string.IsNullOrWhiteSpace(lastSavedAccountImportApplyPreviewText);
            bool ready = hasPreview
                && lastSavedAccountImportApplyPreviewReady
                && !string.IsNullOrWhiteSpace(lastSavedAccountImportApplyPreviewPath);
            string text = hasPreview
                ? lastSavedAccountImportApplyPreviewText
                : SavedAccountNoLoadPreviewText;
            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && ready;
            if (DrawActionButton(new Rect(x, y - 2f, 58f, 22f), SavedAccountLoadButtonLabel, ready))
            {
                TryApplySavedAccountImport(lastSavedAccountImportApplyPreviewPath, false, "Mech Lab");
            }

            GUI.enabled = previousEnabled;
            DrawActionStateLabel(
                x + 66f,
                y,
                width - 66f,
                SavedAccountLoadPreviewPrefix + text,
                ready,
                56);
        }

        private void DrawLocalCandidatePrepAction(float x, float y, float width)
        {
            string readyCandidate = FirstStartupDepotCandidateOwnedMechId();
            bool ready = !string.IsNullOrWhiteSpace(readyCandidate);
            bool canAct = demoInventory != null;
            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && canAct;
            if (DrawActionButton(new Rect(x, y - 2f, 58f, 22f), ready ? "Squad" : "Ready", canAct))
            {
                string ownedMechId = readyCandidate;
                MechBaySavedAccountContract beforeAccount = null;
                if (string.IsNullOrWhiteSpace(ownedMechId))
                {
                    beforeAccount = CurrentSavedAccountSnapshot();
                    ownedMechId = EnsureLocalReadyCandidate("Mech Lab reserve prep", false);
                    if (!string.IsNullOrWhiteSpace(ownedMechId))
                    {
                        RecordSavedAccountDelta(
                            beforeAccount,
                            CurrentSavedAccountSnapshot(),
                            "Mech Lab reserve prep");
                        TryAutoSaveSavedAccount("reserve prep");
                    }
                }

                if (!string.IsNullOrWhiteSpace(ownedMechId))
                {
                    OpenSquadSelectionPreviewIncoming(ownedMechId, "Reserve prep");
                }
            }

            GUI.enabled = previousEnabled;
            DrawActionStateLabel(
                x + 66f,
                y,
                width - 66f,
                "Reserve " + LocalCandidatePrepText(readyCandidate),
                canAct,
                56);
        }

        private string LocalCandidatePrepText(string readyCandidate)
        {
            if (!string.IsNullOrWhiteSpace(readyCandidate))
            {
                if (!string.IsNullOrWhiteSpace(lastSavedAccountDeltaText)
                    && !string.Equals(lastSavedAccountDeltaText, "Delta none", StringComparison.Ordinal))
                {
                    return lastSavedAccountDeltaText;
                }

                return "ready  " + readyCandidate;
            }

            if (demoInventory == null)
            {
                return "Unavailable";
            }

            string pendingOwnedMechId = FirstStartupPendingWarehouseMechId();
            if (!string.IsNullOrWhiteSpace(pendingOwnedMechId))
            {
                return "needs pilot and weapon";
            }

            return "build mech, hire pilot, mount weapon";
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

            string cost = cheapestCost == int.MaxValue ? "unknown" : FundsCostText(cheapestCost);
            string pilotLabel = candidates.Length == 1 ? " NPC pilot from " : " NPC pilots from ";
            return candidates.Length.ToString(CultureInfo.InvariantCulture)
                + pilotLabel
                + cost
                + "  afford "
                + affordableCount.ToString(CultureInfo.InvariantCulture)
                + "  hire ready";
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
                GUI.Label(new Rect(x, y, width, 18f), MechLabRosterEmptyText);
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

            string source = entry.isWarehouseMech ? "Reserve" : "Squad";
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
            string loadoutStatus = string.IsNullOrWhiteSpace(entry.loadoutStatus)
                ? "Unknown"
                : PlayerRosterStatusText(entry.loadoutStatus);
            string bay = entry.isWarehouseMech ? "Reserve Bay" : "Ready Bay";
            return "Chassis " + chassis + "  Fit " + loadoutStatus + "  " + bay;
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

            string status = string.IsNullOrWhiteSpace(entry.deploymentStatus)
                ? "Unknown"
                : PlayerRosterStatusText(entry.deploymentStatus);
            string requirements = string.IsNullOrWhiteSpace(entry.deploymentRequirements)
                ? "Requirements unknown"
                : PlayerRosterStatusText(entry.deploymentRequirements);
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
                statusText = PlayerMechLabActionStatusText(result?.Message ?? "Pilot hire unavailable");
                if (result != null && result.Accepted)
                {
                    AddCombatLogLine("Pilot " + result.Message);
                    RefreshDemoInventoryValidation();
                    TryAutoSaveSavedAccount("pilot hire");
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

            string cost = FundsCostText(hirePreview.TokenCost);
            string risk = string.IsNullOrWhiteSpace(hirePreview.RiskProfile) ? "" : "  " + hirePreview.RiskProfile;
            return hirePreview.displayName
                + " "
                + cost
                + "  "
                + PlayerMechLabActionStatusText(hirePreview.Message)
                + risk;
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
            if (GUI.Button(new Rect(x, y - 2f, 72f, 22f), "Review"))
            {
                string name = entry == null || string.IsNullOrWhiteSpace(entry.displayName) ? "reserve mech" : entry.displayName;
                showWarehouseDraftFitPreview = true;
                showSquadSelectionPreview = false;
                warehouseDraftFitPreviewMechId = entry?.ownedMechId;
                ClearSquadSelectionDraft();
                ClearSquadSelectionCompletedReplacement();
                statusText = "Fit review ready: " + TruncateText(name, 24);
                AddCombatLogLine("Mech Lab reserve fit ready for " + name);
            }

            GUI.enabled = previousEnabled;

            string status = entry == null || string.IsNullOrWhiteSpace(entry.draftFitStatus)
                ? "Fit review unavailable"
                : PlayerFitStatusText(entry.draftFitStatus);
            GUI.Label(new Rect(x + 80f, y, width - 80f, 18f), TruncateText(status, 44));

            string requirements = entry == null || string.IsNullOrWhiteSpace(entry.draftFitRequirements)
                ? "Requirements unknown"
                : "Requires " + PlayerRosterStatusText(entry.draftFitRequirements);
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
                : PlayerRosterStatusText(entry.squadSelectionStatus);
            string requirements = string.IsNullOrWhiteSpace(entry.squadSelectionRequirements)
                ? "Requirements unknown"
                : PlayerRosterStatusText(entry.squadSelectionRequirements);
            return status + "  " + requirements;
        }

        private static string NextSquadOpenStatusText(string name)
        {
            string display = string.IsNullOrWhiteSpace(name) ? "mech" : PlayerRosterStatusText(name);
            display = display.Replace("preview", "review");
            display = display.Replace("Preview", "Review");
            display = display.Replace("candidate", "reserve");
            display = display.Replace("Candidate", "Reserve");
            return "Next squad open: " + TruncateText(PlayerRosterStatusText(display), 20);
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
                AddCombatLogLine("Next contract squad opened with outgoing " + name);
                return;
            }

            if (entry?.squadSelectionCandidate == true)
            {
                squadSelectionDraftIncomingOwnedMechId = entry.ownedMechId;
                statusText = "Next squad in: " + TruncateText(name, 20);
                AddCombatLogLine("Next contract squad opened with incoming " + name);
                return;
            }

            statusText = NextSquadOpenStatusText(name);
            AddCombatLogLine("Next contract squad opened");
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
            AddCombatLogLine((sourceLabel ?? "Next contract squad") + " opened with incoming " + name);
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

            GUI.Box(new Rect(x, y, width, 238f), "Next Contract Squad");
            if (GUI.Button(new Rect(x + width - 58f, y + 4f, 48f, 22f), SquadSelectionBackButtonLabel))
            {
                bool keepCompletedReplacementCue = SquadSelectionCompleted(preview);
                showSquadSelectionPreview = false;
                ClearSquadSelectionDraft();
                if (!keepCompletedReplacementCue)
                {
                    ClearSquadSelectionCompletedReplacement();
                }

                statusText = keepCompletedReplacementCue ? "Next contract squad set" : "Next squad closed";
            }

            string status = SquadSelectionStatusText(preview?.Status);
            GUI.Label(new Rect(x + 12f, y + 24f, width - 24f, 18f), TruncateText(status, 76));
            if (SquadSelectionCompleted(preview))
            {
                GUI.Label(
                    new Rect(x + 12f, y + 44f, width - 24f, 18f),
                    "Done  Next contract squad ready  "
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
                    "Launch updated squad  ");
                GUI.Label(
                    new Rect(x + 12f, y + 158f, width - 24f, 18f),
                    TruncateText(SquadSelectionPlayerNote(preview, completed: true), 76));
                return;
            }

            GUI.Label(
                new Rect(x + 12f, y + 44f, width - 24f, 18f),
                "Current Slots "
                + (preview?.MissionSlotCount ?? 0).ToString(CultureInfo.InvariantCulture)
                + "  Reserve Ready "
                + (preview?.CandidateCount ?? 0).ToString(CultureInfo.InvariantCulture));
            GUI.Label(
                new Rect(x + 12f, y + 64f, width - 24f, 18f),
                TruncateText("Slots " + SquadSelectionSlotSummary(preview?.MissionSlots, "none"), 76));
            GUI.Label(
                new Rect(x + 12f, y + 84f, width - 24f, 18f),
                TruncateText("Reserve " + SquadSelectionSlotSummary(preview?.DepotCandidates, "none ready"), 76));
            DrawSquadSelectionDraftPickers(x + 12f, y + 106f, width - 24f, preview, draft);
            DrawSquadSelectionSwapPlan(x + 12f, y + 154f, width - 24f, draft);
            DrawSquadSelectionPendingSwap(x + 12f, y + 176f, width - 24f, preview, draft);
            DrawSquadSelectionRestartHandoff(x + 12f, y + 198f, width - 24f, "Next Contract  ");
            GUI.Label(
                new Rect(x + 12f, y + 220f, width - 24f, 18f),
                TruncateText(SquadSelectionPlayerNote(preview, completed: false), 76));
        }

        private static string SquadSelectionPlayerNote(MechBaySquadSelectionPreview preview, bool completed)
        {
            if (completed)
            {
                return "Squad ready for next contract";
            }

            if (preview?.CandidateCount > 0)
            {
                return "Review replacement, Set it, then Launch";
            }

            return preview?.MissionSlotCount > 0
                ? "Prepare a reserve mech before replacing a slot"
                : "No current squad slots available";
        }

        private static string SquadSelectionStatusText(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return SquadSelectionUnavailableStatusText;
            }

            string result = status;
            result = result.Replace("Preview unavailable", SquadSelectionUnavailableStatusText);
            result = result.Replace("No depot candidates ready", "No reserve mechs ready");
            result = result.Replace("fitted depot candidate", "ready reserve mech");
            result = result.Replace("depot candidate", "reserve mech");
            result = result.Replace("depot candidates", "reserve mechs");
            result = result.Replace("depot", "reserve");
            result = result.Replace("Depot", "Reserve");
            result = result.Replace("candidate", "reserve");
            result = result.Replace("Candidate", "Reserve");
            result = result.Replace("preview", "plan");
            result = result.Replace("Preview", "Plan");
            return result;
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
            string direction = outgoing ? " leaves next contract  " : " joins next contract  ";
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
                    ? "reserve mech"
                    : draft.IncomingDisplayName;
                text = "Replacement  " + outgoing + " -> " + incoming + "  Set for next launch";
            }
            else
            {
                string requirements = string.IsNullOrWhiteSpace(draft?.Requirements)
                    ? "Need current slot + ready reserve"
                    : draft.Requirements;
                text = "Plan blocked  " + SquadSelectionPlayerBlockedReason(requirements);
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
                ? "Reserve mech"
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
                    TryAutoSaveSavedAccount("squad selection");
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
                return "Ready  set next contract squad";
            }

            if (preview?.HasRefreshedMissionSlot == true)
            {
                return "Done  next contract updated";
            }

            int missionSlots = preview?.MissionSlotCount ?? 0;
            int candidates = preview?.CandidateCount ?? 0;
            if (missionSlots <= 0)
            {
                return "Blocked  no mission mech";
            }

            if (candidates <= 0)
            {
                return "Blocked  no reserve ready";
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

            if (reason.IndexOf("fitted depot", StringComparison.OrdinalIgnoreCase) >= 0
                || reason.IndexOf("depot candidate", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "need ready reserve";
            }

            if (reason.IndexOf("Selected mech missing", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "selection missing";
            }

            if (reason.IndexOf("Inventory", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return MechLabUnavailableText;
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
                ? "reserve mech"
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
            string prefix = string.IsNullOrWhiteSpace(labelPrefix) ? "Next Contract  " : labelPrefix;
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
                statusText = "Outgoing set: " + TruncateText(SquadSelectionSlotName(selected), 24);
            }
            else
            {
                squadSelectionDraftIncomingOwnedMechId = selected?.ownedMechId;
                statusText = "Reserve set: " + TruncateText(SquadSelectionSlotName(selected), 24);
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
            GUI.Box(new Rect(x, y, width, 104f), "Reserve Fit Review");
            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && preview != null && preview.Ready;
            if (GUI.Button(new Rect(x + width - 112f, y + 4f, 48f, 22f), "Apply"))
            {
                MechBayWarehouseDraftFitApplyResult result =
                    MechBayWarehouseDraftFitPreviewService.TryApplyDemoFit(demoInventory, warehouseDraftFitPreviewMechId);
                RefreshDemoInventoryValidation();
                statusText = PlayerFitStatusText(result?.Message ?? "Fit review unavailable");
                if (result != null && result.Accepted)
                {
                    AddCombatLogLine("Mech Lab " + result.Message + " for " + result.displayName);
                    TryAutoSaveSavedAccount("warehouse fit review");
                    showWarehouseDraftFitPreview = false;
                    warehouseDraftFitPreviewMechId = null;
                }
            }

            GUI.enabled = previousEnabled;
            if (GUI.Button(new Rect(x + width - 58f, y + 4f, 48f, 22f), ReserveFitBackButtonLabel))
            {
                showWarehouseDraftFitPreview = false;
                warehouseDraftFitPreviewMechId = null;
                statusText = "Fit review closed";
            }

            string mech = string.IsNullOrWhiteSpace(preview?.displayName) ? "Mech unavailable" : preview.displayName;
            string chassis = string.IsNullOrWhiteSpace(preview?.chassisId) ? "unknown" : preview.chassisId;
            string status = string.IsNullOrWhiteSpace(preview?.Status) ? "Review unavailable" : PlayerFitStatusText(preview.Status);
            GUI.Label(new Rect(x + 12f, y + 24f, width - 24f, 18f), TruncateText(mech + "  Chassis " + chassis + "  " + status, 76));

            string pilot = string.IsNullOrWhiteSpace(preview?.pilotDisplayName) ? "No pilot assigned" : preview.pilotDisplayName;
            string pilotStatus = string.IsNullOrWhiteSpace(preview?.pilotStatus) ? "Pilot required" : preview.pilotStatus;
            GUI.Label(new Rect(x + 12f, y + 44f, width - 24f, 18f), TruncateText("Pilot " + pilotStatus + " (" + pilot + ")", 76));

            string weapon = string.IsNullOrWhiteSpace(preview?.weaponDisplayName) ? "No spare weapon selected" : preview.weaponDisplayName;
            string stock = preview == null ? "0" : preview.spareWeaponStockCount.ToString(CultureInfo.InvariantCulture);
            GUI.Label(new Rect(x + 12f, y + 64f, width - 24f, 18f), TruncateText("Weapon " + weapon + "  spare " + stock, 76));

            string requirements = string.IsNullOrWhiteSpace(preview?.Requirements) ? "Requirements unknown" : preview.Requirements;
            GUI.Label(
                new Rect(x + 12f, y + 84f, width - 24f, 18f),
                TruncateText(requirements + "  " + ReserveFitReviewNote(preview), 76));
        }

        private static string ReserveFitReviewNote(MechBayWarehouseDraftFitPreview preview)
        {
            if (preview == null)
            {
                return "Review unavailable";
            }

            return preview.Ready ? "Ready to fit reserve mech" : "Resolve requirements before fitting";
        }

        private static string PlayerFitStatusText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "Fit review unavailable";
            }

            string result = text;
            result = PlayerRosterStatusText(result);
            result = result.Replace("Read-only draft fit preview", "Ready fit review");
            result = result.Replace("Draft fit preview locked", "Reserve fit locked");
            result = result.Replace("Draft fit preview unavailable", "Fit review unavailable");
            result = result.Replace("Draft fitting", "Fit review");
            result = result.Replace("draft fitting", "fit review");
            result = result.Replace("Draft fit", "Fit review");
            result = result.Replace("draft fit", "fit review");
            return result;
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

        private static string MissionReceiptLogText(string prefix, MechBayMissionReceipt receipt)
        {
            string lead = string.IsNullOrWhiteSpace(prefix) ? "Payout" : prefix.Trim() + " payout";
            if (receipt == null)
            {
                return lead + " unavailable";
            }

            return lead
                + " "
                + SignedTokens(receipt.TokenDelta)
                + "  Salvage +"
                + Math.Max(0, receipt.SalvageFragmentCount).ToString(CultureInfo.InvariantCulture)
                + ReceiptAssemblyLogText(receipt);
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
            int repairCost = MechBayRepairService.EstimateRepairCostResourcePoints(unit);
            GUI.Label(
                new Rect(left, y, width * 0.46f, 18f),
                MechConditionCompactText(unit));

            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled
                && repairCost > 0
                && demoInventory != null
                && demoInventory.tokenBalance >= repairCost;
            if (GUI.Button(new Rect(right, y - 2f, LoadoutRepairButtonWidth, 22f), LoadoutRepairButtonLabel(repairCost)))
            {
                MechBayRepairResult result = MechBayRepairService.TryRepair(demoInventory, unit);
                RefreshDemoInventoryValidation();
                statusText = result.Message;
                if (result.Accepted)
                {
                    TryAutoSaveSavedAccount("repair " + unit.UnitType);
                }
            }

            GUI.enabled = previousEnabled;
            if (repairCost > 0 && demoInventory != null && demoInventory.tokenBalance < repairCost)
            {
                GUI.Label(
                    new Rect(right + LoadoutRepairStateOffset, y, width * 0.46f - LoadoutRepairStateOffset, 18f),
                    "Need " + FormatTokens(repairCost));
            }
            else
            {
                GUI.Label(
                    new Rect(right + LoadoutRepairStateOffset, y, width * 0.46f - LoadoutRepairStateOffset, 18f),
                    UnitMissionStateText(unit));
            }
        }

        private string MechConditionCompactText(UnitState unit)
        {
            int condition = MechConditionPercent(unit);
            int repairCost = MechBayRepairService.EstimateRepairCostResourcePoints(unit);
            return MechConditionCompactText(condition, repairCost);
        }

        private static string MechConditionCompactText(int condition, int repairCost)
        {
            string text = LoadoutConditionPrefix
                + Math.Max(0, condition).ToString(CultureInfo.InvariantCulture)
                + "%";
            if (repairCost <= 0)
            {
                return text;
            }

            return text + "  Repair " + FormatTokens(repairCost);
        }

        private static string LoadoutRepairButtonLabel(int repairCost)
        {
            return repairCost > 0 ? "Repair" : "OK";
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
                DrawSectionStatusLabel(new Rect(segmentX, y + 8f, segmentWidth + 4f, 16f), section);
            }
        }

        private float DrawProjectedLoadoutGrid(UnitState unit, CombatLoadoutPreview preview, float x, float y, float width)
        {
            if (preview == null || preview.GridWidth <= 0 || preview.GridHeight <= 0)
            {
                return 0f;
            }

            float gap = 3f;
            float maxGridWidth = Mathf.Min(width * 0.48f, 220f);
            float cellSize = Mathf.Clamp((maxGridWidth - gap * (preview.GridWidth - 1)) / preview.GridWidth, 28f, 42f);
            float gridWidth = preview.GridWidth * cellSize + (preview.GridWidth - 1) * gap;
            float gridHeight = preview.GridHeight * cellSize + (preview.GridHeight - 1) * gap;
            int selectedWeaponIndex = SelectedLoadoutWeaponIndexFor(unit, preview);
            bool hasSelectedGridCell = TryGetSelectedLoadoutGridCell(unit, preview, out Vector2Int selectedGridCell);
            Rect gridRect = new(x, y, gridWidth + 2f, gridHeight + 2f);
            DrawColorRect(gridRect, new Color(0.035f, 0.042f, 0.048f, 0.98f));
            DrawRectBorder(gridRect, new Color(0.52f, 0.34f, 0.12f, 0.92f), 1f);
            CombatLoadoutPreviewGridCell hoveredCell = null;
            int hoveredColumn = -1;
            int hoveredRow = -1;

            for (int row = 0; row < preview.GridHeight; row++)
            {
                for (int column = 0; column < preview.GridWidth; column++)
                {
                    Rect cell = new(x + 1f + column * (cellSize + gap), y + 1f + row * (cellSize + gap), cellSize, cellSize);
                    CombatLoadoutPreviewGridCell occupiedCell = LoadoutCellAt(preview, column, row);
                    DrawColorRect(cell, occupiedCell == null ? LoadoutEmptySlotColor : new Color(0.07f, 0.075f, 0.075f, 1f));
                    DrawRectBorder(cell, new Color(0.05f, 0.08f, 0.085f, 0.95f), 1f);
                    if (cell.Contains(Event.current.mousePosition))
                    {
                        hoveredCell = occupiedCell;
                        hoveredColumn = column;
                        hoveredRow = row;
                    }

                    if (Event.current.type == EventType.MouseDown
                        && Event.current.button == 0
                        && cell.Contains(Event.current.mousePosition))
                    {
                        if (occupiedCell != null && occupiedCell.SourceWeaponIndex >= 0)
                        {
                            SetSelectedLoadoutWeapon(unit, occupiedCell.SourceWeaponIndex);
                            SetSelectedLoadoutGridCell(unit, column, row);
                            statusText = "Selected " + TruncateText(occupiedCell.DisplayName, 24);
                        }
                        else if (CountLoadoutCellsAt(preview, column, row) <= 1)
                        {
                            SetSelectedLoadoutGridCell(unit, column, row);
                            statusText = LoadoutTargetSlotStatusText(preview, column, row);
                        }

                        Event.current.Use();
                    }
                }
            }

            HashSet<string> drawnBlocks = new(StringComparer.Ordinal);
            CombatLoadoutPreviewGridCell[] occupiedCells = preview.OccupiedCells;
            for (int index = 0; index < occupiedCells.Length; index++)
            {
                CombatLoadoutPreviewGridCell occupiedCell = occupiedCells[index];
                string blockKey = LoadoutBlockKey(occupiedCell);
                if (occupiedCell == null || !drawnBlocks.Add(blockKey))
                {
                    continue;
                }

                Rect block = LoadoutBlockRect(occupiedCells, occupiedCell, x + 1f, y + 1f, cellSize, gap);
                DrawColorRect(new Rect(block.x + 1f, block.y + 1f, block.width - 2f, block.height - 2f), LoadoutBlockColor(unit, occupiedCell));
                DrawRectBorder(block, new Color(0.015f, 0.02f, 0.025f, 0.95f), 1f);
                if (occupiedCell.SourceWeaponIndex >= 0 && occupiedCell.SourceWeaponIndex == selectedWeaponIndex)
                {
                    DrawRectBorder(block, new Color(1f, 0.95f, 0.22f, 1f), 2f);
                }

                DrawLoadoutBlockLabel(block, LoadoutBlockLabel(unit, occupiedCell));
            }

            for (int row = 0; row < preview.GridHeight; row++)
            {
                for (int column = 0; column < preview.GridWidth; column++)
                {
                    int cellStack = CountLoadoutCellsAt(preview, column, row);
                    if (cellStack <= 1)
                    {
                        continue;
                    }

                    Rect cell = new(x + 1f + column * (cellSize + gap), y + 1f + row * (cellSize + gap), cellSize, cellSize);
                    DrawColorRect(cell, new Color(1f, 0.12f, 0.08f, 0.86f));
                    DrawLoadoutBlockLabel(cell, "!");
                }
            }

            DrawLoadoutTargetPlacementPreview(
                preview,
                selectedWeaponIndex,
                hasSelectedGridCell,
                selectedGridCell,
                x + 1f,
                y + 1f,
                cellSize,
                gap);
            DrawLoadoutSelectedGridCellFrame(preview, occupiedCells, hasSelectedGridCell, selectedGridCell, x + 1f, y + 1f, cellSize, gap);
            DrawLoadoutHoverFrame(preview, occupiedCells, hoveredCell, hoveredColumn, hoveredRow, x + 1f, y + 1f, cellSize, gap);

            float railX = x + gridWidth + 10f;
            float railWidth = Mathf.Max(160f, width - gridWidth - 10f);
            GUI.Label(
                new Rect(railX, y + 2f, railWidth, 18f),
                "Grid "
                + preview.Validation.OccupiedGridCells.ToString(CultureInfo.InvariantCulture)
                + "/"
                + preview.GridCapacity.ToString(CultureInfo.InvariantCulture)
                + "  A+"
                + FormatDecimal(preview.Validation.TotalArmorHardnessBonus)
                + "  S+"
                + FormatDecimal(preview.Validation.TotalHeatDissipationBonus));
            DrawLoadoutLegend(railX, y + 24f, railWidth);
            GUI.Label(
                new Rect(railX, y + 44f, railWidth, 18f),
                TruncateText(LoadoutBlockDetailText(
                    unit,
                    preview,
                    hoveredCell,
                    hoveredColumn,
                    hoveredRow,
                    hasSelectedGridCell,
                    selectedGridCell,
                    selectedWeaponIndex),
                    64));
            DrawLoadoutPlacementControls(unit, preview, railX, y + 78f, railWidth);
            return Mathf.Max(gridHeight + 2f, LoadoutGridSectionMinHeight);
        }

        private void DrawLoadoutTargetPlacementPreview(
            CombatLoadoutPreview preview,
            int selectedWeaponIndex,
            bool hasSelectedGridCell,
            Vector2Int selectedGridCell,
            float gridOriginX,
            float gridOriginY,
            float cellSize,
            float gap)
        {
            if (preview == null || !hasSelectedGridCell || selectedWeaponIndex < 0)
            {
                return;
            }

            CombatLoadoutPreviewItem selectedItem = LoadoutPreviewItemForWeapon(preview, selectedWeaponIndex);
            if (selectedItem == null
                || (selectedGridCell.x == selectedItem.GridX && selectedGridCell.y == selectedItem.GridY))
            {
                return;
            }

            bool targetClear = IsLoadoutTargetPlacementClear(preview, selectedWeaponIndex, selectedGridCell);
            Color fillColor = targetClear
                ? new Color(0.28f, 0.92f, 0.72f, 0.24f)
                : new Color(1f, 0.18f, 0.08f, 0.32f);
            Color borderColor = targetClear
                ? new Color(0.45f, 1f, 0.82f, 0.95f)
                : new Color(1f, 0.20f, 0.08f, 0.98f);

            CombatLoadoutPreviewGridCell[] occupiedCells = preview.OccupiedCells;
            for (int index = 0; index < occupiedCells.Length; index++)
            {
                CombatLoadoutPreviewGridCell sourceCell = occupiedCells[index];
                if (sourceCell == null || sourceCell.SourceWeaponIndex != selectedWeaponIndex)
                {
                    continue;
                }

                int targetX = selectedGridCell.x + sourceCell.X - selectedItem.GridX;
                int targetY = selectedGridCell.y + sourceCell.Y - selectedItem.GridY;
                if (targetX < 0 || targetY < 0 || targetX >= preview.GridWidth || targetY >= preview.GridHeight)
                {
                    continue;
                }

                Rect cell = new(
                    gridOriginX + targetX * (cellSize + gap),
                    gridOriginY + targetY * (cellSize + gap),
                    cellSize,
                    cellSize);
                DrawColorRect(new Rect(cell.x + 2f, cell.y + 2f, cell.width - 4f, cell.height - 4f), fillColor);
                DrawRectBorder(cell, borderColor, 2f);
            }
        }

        private void DrawLoadoutHoverFrame(
            CombatLoadoutPreview preview,
            CombatLoadoutPreviewGridCell[] occupiedCells,
            CombatLoadoutPreviewGridCell hoveredCell,
            int hoveredColumn,
            int hoveredRow,
            float gridOriginX,
            float gridOriginY,
            float cellSize,
            float gap)
        {
            if (preview == null || hoveredColumn < 0 || hoveredRow < 0)
            {
                return;
            }

            if (hoveredCell != null)
            {
                Rect block = LoadoutBlockRect(occupiedCells, hoveredCell, gridOriginX, gridOriginY, cellSize, gap);
                DrawRectBorder(new Rect(block.x - 1f, block.y - 1f, block.width + 2f, block.height + 2f), UiCyanColor, 2f);
                return;
            }

            Rect cell = new(
                gridOriginX + hoveredColumn * (cellSize + gap),
                gridOriginY + hoveredRow * (cellSize + gap),
                cellSize,
                cellSize);
            DrawRectBorder(cell, UiAmberColor, 2f);
        }

        private void DrawLoadoutSelectedGridCellFrame(
            CombatLoadoutPreview preview,
            CombatLoadoutPreviewGridCell[] occupiedCells,
            bool hasSelectedGridCell,
            Vector2Int selectedGridCell,
            float gridOriginX,
            float gridOriginY,
            float cellSize,
            float gap)
        {
            if (preview == null || !hasSelectedGridCell)
            {
                return;
            }

            CombatLoadoutPreviewGridCell selectedCell = LoadoutCellAt(preview, selectedGridCell.x, selectedGridCell.y);
            if (selectedCell != null)
            {
                Rect block = LoadoutBlockRect(occupiedCells, selectedCell, gridOriginX, gridOriginY, cellSize, gap);
                DrawRectBorder(new Rect(block.x - 1f, block.y - 1f, block.width + 2f, block.height + 2f), new Color(1f, 0.95f, 0.22f, 1f), 2f);
                return;
            }

            Rect cell = new(
                gridOriginX + selectedGridCell.x * (cellSize + gap),
                gridOriginY + selectedGridCell.y * (cellSize + gap),
                cellSize,
                cellSize);
            DrawRectBorder(cell, new Color(1f, 0.95f, 0.22f, 1f), 2f);
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

        private static CombatLoadoutPreviewGridCell LoadoutCellForSelectedWeapon(CombatLoadoutPreview preview, int selectedWeaponIndex)
        {
            CombatLoadoutPreviewGridCell[] occupiedCells = preview?.OccupiedCells ?? Array.Empty<CombatLoadoutPreviewGridCell>();
            for (int index = 0; index < occupiedCells.Length; index++)
            {
                CombatLoadoutPreviewGridCell cell = occupiedCells[index];
                if (cell != null && cell.SourceWeaponIndex == selectedWeaponIndex)
                {
                    return cell;
                }
            }

            return null;
        }

        private static string LoadoutBlockKey(CombatLoadoutPreviewGridCell cell)
        {
            if (cell == null)
            {
                return "";
            }

            if (cell.SourceWeaponIndex >= 0)
            {
                return "weapon:" + cell.SourceWeaponIndex.ToString(CultureInfo.InvariantCulture);
            }

            return "item:" + (cell.ItemId ?? cell.Category ?? "");
        }

        private static bool SameLoadoutBlock(CombatLoadoutPreviewGridCell left, CombatLoadoutPreviewGridCell right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            if (left.SourceWeaponIndex >= 0 || right.SourceWeaponIndex >= 0)
            {
                return left.SourceWeaponIndex == right.SourceWeaponIndex;
            }

            return string.Equals(left.ItemId, right.ItemId, StringComparison.OrdinalIgnoreCase);
        }

        private static Rect LoadoutBlockRect(
            CombatLoadoutPreviewGridCell[] cells,
            CombatLoadoutPreviewGridCell blockCell,
            float gridX,
            float gridY,
            float cellSize,
            float gap)
        {
            int minX = blockCell.X;
            int minY = blockCell.Y;
            int maxX = blockCell.X;
            int maxY = blockCell.Y;
            for (int index = 0; index < cells.Length; index++)
            {
                CombatLoadoutPreviewGridCell cell = cells[index];
                if (!SameLoadoutBlock(cell, blockCell))
                {
                    continue;
                }

                minX = Math.Min(minX, cell.X);
                minY = Math.Min(minY, cell.Y);
                maxX = Math.Max(maxX, cell.X);
                maxY = Math.Max(maxY, cell.Y);
            }

            float x = gridX + minX * (cellSize + gap);
            float y = gridY + minY * (cellSize + gap);
            float width = (maxX - minX + 1) * cellSize + (maxX - minX) * gap;
            float height = (maxY - minY + 1) * cellSize + (maxY - minY) * gap;
            return new Rect(x, y, width, height);
        }

        private void DrawLoadoutBlockLabel(Rect block, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            GUIStyle style = new(uiLabelStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
                fontSize = block.height >= 36f ? 10 : 9,
                fontStyle = FontStyle.Bold
            };
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(block.x + 3f, block.y + 2f, block.width - 6f, block.height - 4f), text, style);
        }

        private void DrawLoadoutLegend(float x, float y, float width)
        {
            float swatch = 10f;
            float labelX = x;
            DrawLoadoutLegendSwatch(ref labelX, y, swatch, LoadoutEmptySlotColor, "E");
            DrawLoadoutLegendSwatch(ref labelX, y, swatch, LoadoutShortWeaponColor, "S");
            DrawLoadoutLegendSwatch(ref labelX, y, swatch, LoadoutMediumWeaponColor, "M");
            DrawLoadoutLegendSwatch(ref labelX, y, swatch, LoadoutLongWeaponColor, "L");
            DrawLoadoutLegendSwatch(ref labelX, y, swatch, LoadoutComponentColor, "C");
            if (labelX > x + width)
            {
                GUI.Label(new Rect(x, y + 14f, width, 18f), "E empty  S/M/L range  C comp");
            }
        }

        private void DrawLoadoutLegendSwatch(ref float x, float y, float swatchSize, Color color, string label)
        {
            DrawColorRect(new Rect(x, y + 4f, swatchSize, swatchSize), color);
            GUI.Label(new Rect(x + swatchSize + 4f, y, 18f, 18f), label);
            x += swatchSize + 26f;
        }

        private static Color LoadoutBlockColor(UnitState unit, CombatLoadoutPreviewGridCell cell)
        {
            if (cell == null)
            {
                return LoadoutEmptySlotColor;
            }

            if (cell.Category == LoadoutItemCategory.ArmorPlate || cell.Category == LoadoutItemCategory.HeatSink)
            {
                return LoadoutComponentColor;
            }

            CombatWeaponDefinition weapon = LoadoutWeaponForCell(unit, cell);
            float range = weapon?.rangeMax ?? 0f;
            if (range <= 0f)
            {
                return WeaponColor(cell.WeaponType);
            }

            if (range < 450f)
            {
                return LoadoutShortWeaponColor;
            }

            if (range < 850f)
            {
                return LoadoutMediumWeaponColor;
            }

            return LoadoutLongWeaponColor;
        }

        private static string LoadoutBlockLabel(UnitState unit, CombatLoadoutPreviewGridCell cell)
        {
            if (cell == null)
            {
                return "";
            }

            if (cell.Category == LoadoutItemCategory.ArmorPlate)
            {
                return "A+";
            }

            if (cell.Category == LoadoutItemCategory.HeatSink)
            {
                return "C+";
            }

            CombatWeaponDefinition weapon = LoadoutWeaponForCell(unit, cell);
            string name = weapon?.name ?? cell.DisplayName ?? "Weapon";
            string prefix = cell.SourceWeaponIndex >= 0
                ? (cell.SourceWeaponIndex + 1).ToString(CultureInfo.InvariantCulture) + " "
                : "";
            return prefix + ShortLoadoutItemName(name);
        }

        private static string LoadoutBlockDetailText(
            UnitState unit,
            CombatLoadoutPreview preview,
            CombatLoadoutPreviewGridCell hoveredCell,
            int hoveredColumn,
            int hoveredRow,
            bool hasSelectedGridCell,
            Vector2Int selectedGridCell,
            int selectedWeaponIndex)
        {
            if (hasSelectedGridCell)
            {
                bool hoverMatchesSelectedTarget = hoveredColumn == selectedGridCell.x && hoveredRow == selectedGridCell.y;
                if (hoverMatchesSelectedTarget || hoveredColumn < 0 || hoveredRow < 0)
                {
                    string targetPlacementText = LoadoutTargetPlacementDetailText(unit, preview, selectedWeaponIndex, selectedGridCell);
                    if (!string.IsNullOrEmpty(targetPlacementText))
                    {
                        return targetPlacementText;
                    }
                }
            }

            if (hoveredColumn >= 0 && hoveredRow >= 0 && hoveredCell == null)
            {
                return "Slot " + hoveredColumn.ToString(CultureInfo.InvariantCulture)
                    + "," + hoveredRow.ToString(CultureInfo.InvariantCulture)
                    + " open";
            }

            CombatLoadoutPreviewGridCell detailCell = hoveredCell;
            if (detailCell == null && hasSelectedGridCell)
            {
                detailCell = LoadoutCellAt(preview, selectedGridCell.x, selectedGridCell.y);
                if (detailCell == null)
                {
                    return "Slot " + selectedGridCell.x.ToString(CultureInfo.InvariantCulture)
                        + "," + selectedGridCell.y.ToString(CultureInfo.InvariantCulture)
                        + " open";
                }
            }

            detailCell ??= LoadoutCellForSelectedWeapon(preview, selectedWeaponIndex);
            if (detailCell == null)
            {
                return "Payload detail unavailable";
            }

            int cells = CountLoadoutBlockCells(preview, detailCell);
            string shapeText = LoadoutBlockShapeText(preview, detailCell);
            string compactShapeText = LoadoutBlockCompactShapeText(cells, shapeText);
            if (detailCell.Category == LoadoutItemCategory.ArmorPlate)
            {
                return "A+1 W0.5  " + compactShapeText;
            }

            if (detailCell.Category == LoadoutItemCategory.HeatSink)
            {
                return "C+1.5 W1  " + compactShapeText;
            }

            CombatWeaponDefinition weapon = LoadoutWeaponForCell(unit, detailCell);
            if (weapon == null)
            {
                return (detailCell.DisplayName ?? "Payload")
                    + "  "
                    + compactShapeText;
            }

            string weaponName = ShortLoadoutItemName(weapon.name);
            if (string.IsNullOrWhiteSpace(weaponName))
            {
                weaponName = "Weapon";
            }

            return (detailCell.SourceWeaponIndex + 1).ToString(CultureInfo.InvariantCulture)
                + " " + weaponName
                + "  H" + FormatDecimal(weapon.heat)
                + " W" + FormatDecimal(weapon.weight)
                + " D" + FormatDecimal(weapon.damage)
                + " R" + Mathf.RoundToInt(weapon.rangeMax).ToString(CultureInfo.InvariantCulture)
                + " CD" + FormatDecimal(weapon.recycleTime)
                + " " + compactShapeText;
        }

        private static string LoadoutBlockCompactShapeText(int cells, string shapeText)
        {
            return "C" + Math.Max(0, cells).ToString(CultureInfo.InvariantCulture)
                + " "
                + (string.IsNullOrWhiteSpace(shapeText) ? "?x?" : shapeText);
        }

        private static string LoadoutTargetPlacementDetailText(
            UnitState unit,
            CombatLoadoutPreview preview,
            int selectedWeaponIndex,
            Vector2Int targetCell)
        {
            CombatLoadoutPreviewItem selectedItem = LoadoutPreviewItemForWeapon(preview, selectedWeaponIndex);
            if (preview == null
                || selectedItem == null
                || (targetCell.x == selectedItem.GridX && targetCell.y == selectedItem.GridY))
            {
                return null;
            }

            CombatLoadoutPreviewGridCell selectedCell = LoadoutCellForSelectedWeapon(preview, selectedWeaponIndex);
            int cells = Math.Max(1, CountLoadoutBlockCells(preview, selectedCell));
            string shapeText = LoadoutBlockShapeText(preview, selectedCell);
            CombatWeaponDefinition weapon = LoadoutWeaponForCell(unit, selectedCell);
            string weaponName = string.IsNullOrWhiteSpace(weapon?.name) ? selectedItem.DisplayName : weapon.name;
            string weaponLabel = LoadoutTargetWeaponShortLabel(selectedWeaponIndex, weaponName);
            string targetIssue = LoadoutTargetPlacementIssueText(preview, selectedWeaponIndex, targetCell);
            string targetState = string.IsNullOrEmpty(targetIssue) ? "OK" : "Block " + targetIssue;
            string fillerActionSuffix = LoadoutTargetFillerActionSuffix(preview, targetCell);
            return "T "
                + targetCell.x.ToString(CultureInfo.InvariantCulture)
                + ","
                + targetCell.y.ToString(CultureInfo.InvariantCulture)
                + " "
                + targetState
                + fillerActionSuffix
                + " for "
                + weaponLabel
                + " C"
                + cells.ToString(CultureInfo.InvariantCulture)
                + " "
                + shapeText;
        }

        private static string LoadoutTargetWeaponShortLabel(int selectedWeaponIndex, string weaponName)
        {
            string shortName = ShortLoadoutItemName(weaponName);
            if (string.IsNullOrWhiteSpace(shortName))
            {
                shortName = "Weapon";
            }

            return LoadoutWeaponStateLabel(selectedWeaponIndex, shortName);
        }

        private static int CountLoadoutBlockCells(CombatLoadoutPreview preview, CombatLoadoutPreviewGridCell blockCell)
        {
            int count = 0;
            CombatLoadoutPreviewGridCell[] cells = preview?.OccupiedCells ?? Array.Empty<CombatLoadoutPreviewGridCell>();
            for (int index = 0; index < cells.Length; index++)
            {
                if (SameLoadoutBlock(cells[index], blockCell))
                {
                    count++;
                }
            }

            return count;
        }

        private static string LoadoutBlockShapeText(CombatLoadoutPreview preview, CombatLoadoutPreviewGridCell blockCell)
        {
            if (preview == null || blockCell == null)
            {
                return "1x1";
            }

            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;
            CombatLoadoutPreviewGridCell[] cells = preview.OccupiedCells;
            for (int index = 0; index < cells.Length; index++)
            {
                CombatLoadoutPreviewGridCell cell = cells[index];
                if (!SameLoadoutBlock(cell, blockCell))
                {
                    continue;
                }

                minX = Math.Min(minX, cell.X);
                minY = Math.Min(minY, cell.Y);
                maxX = Math.Max(maxX, cell.X);
                maxY = Math.Max(maxY, cell.Y);
            }

            if (minX == int.MaxValue || minY == int.MaxValue)
            {
                return "1x1";
            }

            return (maxX - minX + 1).ToString(CultureInfo.InvariantCulture)
                + "x"
                + (maxY - minY + 1).ToString(CultureInfo.InvariantCulture);
        }

        private static CombatWeaponDefinition LoadoutWeaponForCell(UnitState unit, CombatLoadoutPreviewGridCell cell)
        {
            CombatWeaponDefinition[] weapons = unit?.Profile?.Weapons;
            if (weapons == null || cell == null || cell.SourceWeaponIndex < 0 || cell.SourceWeaponIndex >= weapons.Length)
            {
                return null;
            }

            return weapons[cell.SourceWeaponIndex];
        }

        private static string ShortLoadoutItemName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }

            string trimmed = name.Trim();
            int open = trimmed.LastIndexOf('(');
            int close = trimmed.LastIndexOf(')');
            if (open >= 0 && close > open + 1)
            {
                string code = trimmed.Substring(open + 1, close - open - 1).Trim();
                if (code.Length > 0 && code.Length <= 8 && code.IndexOf(' ') < 0)
                {
                    return code;
                }
            }

            string normalized = trimmed
                .Replace("Large ", "L ")
                .Replace("Medium ", "M ")
                .Replace("Small ", "S ")
                .Replace("Laser", "Las")
                .Replace("Autocannon", "AC")
                .Replace("Missile", "Msl")
                .Replace("Rack", "Rk");
            return TruncateText(normalized, 10);
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
                GUI.Label(new Rect(x, y, width, 18f), "Select a mounted weapon");
                return;
            }

            CombatLoadoutPreviewItem baseItem = LoadoutPreviewItemForWeapon(LoadoutBasePreviewFor(unit), selectedWeaponIndex);
            GUI.Label(
                new Rect(x, y, width, 18f),
                LoadoutPlacementHeaderText(unit, preview, selectedWeaponIndex, selectedItem, baseItem));

            DrawLoadoutNudgeButton(
                unit,
                preview,
                selectedItem,
                new Rect(x + 32f, y + 24f, LoadoutNudgeButtonWidth, 22f),
                LoadoutNudgeButtonLabel(0, -1),
                0,
                -1);
            DrawLoadoutNudgeButton(
                unit,
                preview,
                selectedItem,
                new Rect(x, y + 50f, LoadoutNudgeButtonWidth, 22f),
                LoadoutNudgeButtonLabel(-1, 0),
                -1,
                0);

            bool canResetSelectedWeapon = HasSelectedLoadoutWeaponPlacementOverride(unit, selectedWeaponIndex);
            bool previousResetEnabled = GUI.enabled;
            Color previousResetColor = GUI.color;
            GUI.color = LoadoutSelectedResetButtonColor(canResetSelectedWeapon);
            GUI.enabled = previousResetEnabled && canResetSelectedWeapon;
            if (GUI.Button(
                new Rect(x + 44f, y + 50f, LoadoutSelectedResetButtonWidth, 22f),
                LoadoutSelectedResetButtonLabel(canResetSelectedWeapon)))
            {
                ResetSelectedLoadoutWeapon(unit);
            }

            GUI.color = previousResetColor;
            GUI.enabled = previousResetEnabled;
            DrawLoadoutNudgeButton(
                unit,
                preview,
                selectedItem,
                new Rect(x + 98f, y + 50f, LoadoutNudgeEastButtonWidth, 22f),
                LoadoutNudgeButtonLabel(1, 0),
                1,
                0);
            DrawLoadoutNudgeButton(
                unit,
                preview,
                selectedItem,
                new Rect(x + 32f, y + 76f, LoadoutNudgeButtonWidth, 22f),
                LoadoutNudgeButtonLabel(0, 1),
                0,
                1);
            DrawLoadoutNudgeStatus(preview, selectedItem, x, y + 102f, LoadoutNudgeStatusWidth);

            if (TryGetSelectedLoadoutGridCell(unit, preview, out Vector2Int targetCell))
            {
                bool previousEnabled = GUI.enabled;
                bool canPlace = targetCell.x != selectedItem.GridX || targetCell.y != selectedItem.GridY;
                string targetIssue = LoadoutTargetPlacementIssueText(preview, selectedWeaponIndex, targetCell);
                bool targetClear = string.IsNullOrEmpty(targetIssue);
                GUI.enabled = previousEnabled && canPlace && targetClear;
                Color previousColor = GUI.color;
                GUI.color = !canPlace
                    ? previousColor
                    : targetClear
                        ? new Color(0.50f, 1f, 0.82f, 1f)
                        : new Color(1f, 0.32f, 0.18f, 1f);
                string placeLabel = LoadoutPlaceButtonLabel(targetClear);
                if (GUI.Button(new Rect(x + LoadoutTargetControlOffset, y + 24f, LoadoutTargetButtonWidth, 22f), placeLabel))
                {
                    PlaceSelectedLoadoutWeaponAt(unit, preview, targetCell.x, targetCell.y);
                }

                GUI.color = previousColor;
                CombatLoadoutPreviewGridCell occupiedCell = LoadoutCellAt(preview, targetCell.x, targetCell.y);
                bool canFill = occupiedCell == null || occupiedCell.SourceWeaponIndex < 0;
                int targetCellStack = CountLoadoutCellsAt(preview, targetCell.x, targetCell.y);
                bool canCycleFiller = canFill && targetCellStack <= 1;
                GUI.enabled = previousEnabled && canCycleFiller;
                string fillerAction = FillerButtonLabel(occupiedCell?.Category, canFill, targetCellStack);
                GUI.color = LoadoutFillerActionButtonColor(occupiedCell?.Category, canCycleFiller);
                if (GUI.Button(
                    new Rect(x + LoadoutTargetControlOffset, y + 50f, LoadoutTargetButtonWidth, 22f),
                    fillerAction))
                {
                    CycleFillerOverride(unit, targetCell.x, targetCell.y, occupiedCell?.Category);
                }

                GUI.color = previousColor;
                string targetPosition = LoadoutGridPositionText(targetCell.x, targetCell.y);
                string targetStatus = canPlace
                    ? targetClear ? "T " + targetPosition + " OK" : "T " + targetPosition + " Block " + TruncateText(targetIssue, 12)
                    : "T " + targetPosition + " Same";
                if (canCycleFiller)
                {
                    targetStatus += " / " + FillerCompactActionLabel(occupiedCell?.Category);
                }

                GUI.enabled = previousEnabled;
                GUI.color = LoadoutTargetStatusColor(canPlace, targetClear);
                GUI.Label(
                    new Rect(
                        x + LoadoutTargetControlOffset,
                        y + 76f,
                        Mathf.Max(LoadoutTargetStatusMinWidth, width - LoadoutTargetControlOffset),
                        18f),
                    targetStatus);
                GUI.color = previousColor;
            }
            else
            {
                Color previousColor = GUI.color;
                GUI.color = LoadoutTargetStatusColor(false, true);
                GUI.Label(
                    new Rect(
                        x + LoadoutTargetControlOffset,
                        y + 24f,
                        Mathf.Max(LoadoutTargetStatusMinWidth, width - LoadoutTargetControlOffset),
                        18f),
                    LoadoutTargetPickLabel());
                GUI.color = previousColor;
            }
        }

        private static string LoadoutPlaceButtonLabel(bool targetClear)
        {
            return targetClear ? "Place" : "Block";
        }

        private static string LoadoutTargetPickLabel()
        {
            return "Pick";
        }

        private static string LoadoutNudgeButtonLabel(int deltaX, int deltaY)
        {
            if (deltaY < 0)
            {
                return "N";
            }

            if (deltaX < 0)
            {
                return "W";
            }

            if (deltaX > 0)
            {
                return "E";
            }

            return "S";
        }

        private void DrawLoadoutNudgeButton(
            UnitState unit,
            CombatLoadoutPreview preview,
            CombatLoadoutPreviewItem selectedItem,
            Rect rect,
            string label,
            int deltaX,
            int deltaY)
        {
            bool canNudge = IsLoadoutNudgeTargetClear(preview, selectedItem, deltaX, deltaY);
            bool previousEnabled = GUI.enabled;
            Color previousColor = GUI.color;
            GUI.enabled = previousEnabled && canNudge;
            GUI.color = canNudge
                ? previousColor
                : new Color(1f, 0.34f, 0.22f, 1f);
            if (GUI.Button(rect, label))
            {
                MoveSelectedLoadoutWeapon(unit, preview, deltaX, deltaY);
            }

            GUI.color = previousColor;
            GUI.enabled = previousEnabled;
        }

        private void DrawLoadoutNudgeStatus(
            CombatLoadoutPreview preview,
            CombatLoadoutPreviewItem selectedItem,
            float x,
            float y,
            float width)
        {
            string blockedSummary = LoadoutNudgeBlockedSummary(preview, selectedItem);
            bool hasBlockedDirections = !string.IsNullOrEmpty(blockedSummary);
            Color previousColor = GUI.color;
            GUI.color = LoadoutNudgeStatusColor(hasBlockedDirections);
            GUI.Label(
                new Rect(x, y, width, 18f),
                LoadoutNudgeStatusLabel(blockedSummary));
            GUI.color = previousColor;
        }

        private static string LoadoutNudgeStatusLabel(string blockedSummary)
        {
            return string.IsNullOrEmpty(blockedSummary)
                ? "Move OK"
                : "Block " + TruncateText(blockedSummary, 22);
        }

        private static bool IsLoadoutTargetPlacementClear(
            CombatLoadoutPreview preview,
            int selectedWeaponIndex,
            Vector2Int targetCell)
        {
            return string.IsNullOrEmpty(LoadoutTargetPlacementIssueText(preview, selectedWeaponIndex, targetCell));
        }

        private static bool IsLoadoutNudgeTargetClear(
            CombatLoadoutPreview preview,
            CombatLoadoutPreviewItem selectedItem,
            int deltaX,
            int deltaY)
        {
            if (preview == null || selectedItem == null)
            {
                return false;
            }

            Vector2Int targetCell = new(selectedItem.GridX + deltaX, selectedItem.GridY + deltaY);
            return IsLoadoutTargetPlacementClear(preview, selectedItem.SourceWeaponIndex, targetCell);
        }

        private static string LoadoutNudgeBlockedSummary(
            CombatLoadoutPreview preview,
            CombatLoadoutPreviewItem selectedItem)
        {
            List<string> blockedDirections = new();
            AddLoadoutNudgeBlock(blockedDirections, preview, selectedItem, "N", 0, -1);
            AddLoadoutNudgeBlock(blockedDirections, preview, selectedItem, "W", -1, 0);
            AddLoadoutNudgeBlock(blockedDirections, preview, selectedItem, "E", 1, 0);
            AddLoadoutNudgeBlock(blockedDirections, preview, selectedItem, "S", 0, 1);

            return blockedDirections.Count == 0 ? "" : string.Join(", ", blockedDirections);
        }

        private static void AddLoadoutNudgeBlock(
            List<string> blockedDirections,
            CombatLoadoutPreview preview,
            CombatLoadoutPreviewItem selectedItem,
            string direction,
            int deltaX,
            int deltaY)
        {
            string issue = LoadoutNudgeIssueText(preview, selectedItem, deltaX, deltaY);
            if (!string.IsNullOrEmpty(issue))
            {
                blockedDirections.Add(direction + " " + ShortLoadoutNudgeIssue(issue));
            }
        }

        private static string LoadoutNudgeIssueText(
            CombatLoadoutPreview preview,
            CombatLoadoutPreviewItem selectedItem,
            int deltaX,
            int deltaY)
        {
            if (preview == null || selectedItem == null)
            {
                return "invalid target";
            }

            Vector2Int targetCell = new(selectedItem.GridX + deltaX, selectedItem.GridY + deltaY);
            return LoadoutTargetPlacementIssueText(preview, selectedItem.SourceWeaponIndex, targetCell);
        }

        private static string ShortLoadoutNudgeIssue(string issue)
        {
            if (string.IsNullOrWhiteSpace(issue))
            {
                return "";
            }

            if (issue.StartsWith("outside", StringComparison.OrdinalIgnoreCase))
            {
                return "outside";
            }

            if (issue.StartsWith("overlap", StringComparison.OrdinalIgnoreCase))
            {
                return "overlap";
            }

            return "blocked";
        }

        private static Color LoadoutNudgeStatusColor(bool hasBlockedDirections)
        {
            return hasBlockedDirections
                ? new Color(1f, 0.78f, 0.28f, 1f)
                : new Color(0.50f, 1f, 0.82f, 1f);
        }

        private static string LoadoutTargetFillerActionSuffix(CombatLoadoutPreview preview, Vector2Int targetCell)
        {
            if (preview == null)
            {
                return "";
            }

            CombatLoadoutPreviewGridCell occupiedCell = LoadoutCellAt(preview, targetCell.x, targetCell.y);
            if (occupiedCell != null && occupiedCell.SourceWeaponIndex >= 0)
            {
                return "";
            }

            if (CountLoadoutCellsAt(preview, targetCell.x, targetCell.y) > 1)
            {
                return "";
            }

            return " / " + FillerCompactActionLabel(occupiedCell?.Category);
        }

        private static string LoadoutTargetSlotStatusText(CombatLoadoutPreview preview, int column, int row)
        {
            string text = "T " + LoadoutGridPositionText(column, row);
            CombatLoadoutPreviewGridCell occupiedCell = LoadoutCellAt(preview, column, row);
            if (occupiedCell != null && occupiedCell.SourceWeaponIndex >= 0)
            {
                return text;
            }

            if (CountLoadoutCellsAt(preview, column, row) > 1)
            {
                return text;
            }

            return text + " " + FillerCompactActionLabel(occupiedCell?.Category);
        }

        private static string LoadoutTargetPlacementIssueText(
            CombatLoadoutPreview preview,
            int selectedWeaponIndex,
            Vector2Int targetCell)
        {
            CombatLoadoutPreviewItem selectedItem = LoadoutPreviewItemForWeapon(preview, selectedWeaponIndex);
            if (preview == null || selectedItem == null)
            {
                return "invalid target";
            }

            CombatLoadoutPreviewGridCell[] occupiedCells = preview.OccupiedCells;
            for (int index = 0; index < occupiedCells.Length; index++)
            {
                CombatLoadoutPreviewGridCell sourceCell = occupiedCells[index];
                if (sourceCell == null || sourceCell.SourceWeaponIndex != selectedWeaponIndex)
                {
                    continue;
                }

                int targetX = targetCell.x + sourceCell.X - selectedItem.GridX;
                int targetY = targetCell.y + sourceCell.Y - selectedItem.GridY;
                if (targetX < 0 || targetY < 0 || targetX >= preview.GridWidth || targetY >= preview.GridHeight)
                {
                    return "outside grid";
                }

                CombatLoadoutPreviewGridCell targetOccupiedCell = LoadoutCellAt(preview, targetX, targetY);
                if (targetOccupiedCell != null && targetOccupiedCell.SourceWeaponIndex != selectedWeaponIndex)
                {
                    string label = targetOccupiedCell.DisplayName;
                    if (string.IsNullOrWhiteSpace(label))
                    {
                        label = targetOccupiedCell.Category;
                    }

                    if (string.IsNullOrWhiteSpace(label))
                    {
                        label = "item";
                    }

                    return "overlap " + ShortLoadoutItemName(label);
                }
            }

            return "";
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

        private float DrawWeaponLoadoutLines(UnitState unit, CombatLoadoutPreview preview, float x, float y, float width)
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
            int selectedWeaponIndex = SelectedLoadoutWeaponIndexFor(unit, preview);
            DrawSelectedWeaponSummaryLine(unit, preview, selectedWeaponIndex, x, y, width);
            float listY = y + 22f;
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
                float rowY = listY + row * 22f;
                bool isSelected = index == selectedWeaponIndex;
                bool hasPlacementOverride = HasLoadoutWeaponPlacementOverride(unit, index);
                string label = LoadoutWeaponButtonLabel(weapon, preview, index, isSelected, hasPlacementOverride);
                Color previousColor = GUI.color;
                Color buttonCue = isSelected ? new Color(1f, 0.95f, 0.22f, 1f) : LoadoutWeaponRangeBandColor(weapon);
                GUI.color = buttonCue;
                Rect buttonRect = new(columnX, rowY - 1f, columnWidth - 4f, 20f);
                if (GUI.Button(buttonRect, label))
                {
                    SetSelectedLoadoutWeapon(unit, index);
                    statusText = LoadoutWeaponSelectionStatus(unit, preview, index, weapon);
                }

                GUI.color = previousColor;
                DrawRectBorder(buttonRect, buttonCue, isSelected ? 2f : 1f);
            }

            return 22f + rows * 22f;
        }

        private string LoadoutWeaponSelectionStatus(
            UnitState unit,
            CombatLoadoutPreview preview,
            int selectedWeaponIndex,
            CombatWeaponDefinition weapon)
        {
            CombatLoadoutPreviewItem selectedItem = LoadoutPreviewItemForWeapon(preview, selectedWeaponIndex);
            CombatLoadoutPreviewItem baseItem = LoadoutPreviewItemForWeapon(LoadoutBasePreviewFor(unit), selectedWeaponIndex);
            string placementState = HasLoadoutWeaponPlacementOverride(unit, selectedWeaponIndex) ? "Moved" : "Base";
            string positionText = selectedItem == null
                ? ""
                : LoadoutWeaponPositionSummary(unit, preview, selectedItem, baseItem);
            return LoadoutWeaponStateLabel(selectedWeaponIndex, placementState)
                + positionText
                + " "
                + TruncateText(weapon?.name ?? "Weapon", 18);
        }

        private static string LoadoutWeaponButtonLabel(
            CombatWeaponDefinition weapon,
            CombatLoadoutPreview preview,
            int sourceWeaponIndex,
            bool isSelected,
            bool hasPlacementOverride)
        {
            string selector = LoadoutWeaponButtonSelector(sourceWeaponIndex, isSelected, hasPlacementOverride);
            return selector
                + " "
                + LoadoutWeaponRangeBandLabel(weapon)
                + " "
                + LoadoutWeaponShapeLabel(preview, sourceWeaponIndex);
        }

        private static string LoadoutWeaponButtonSelector(
            int sourceWeaponIndex,
            bool isSelected,
            bool hasPlacementOverride)
        {
            if (isSelected)
            {
                return hasPlacementOverride ? ">*" : ">";
            }

            return hasPlacementOverride
                ? "*"
                : (sourceWeaponIndex + 1).ToString(CultureInfo.InvariantCulture);
        }

        private static string LoadoutWeaponShapeLabel(CombatLoadoutPreview preview, int sourceWeaponIndex)
        {
            CombatLoadoutPreviewGridCell selectedCell = LoadoutCellForSelectedWeapon(preview, sourceWeaponIndex);
            return LoadoutBlockShapeText(preview, selectedCell);
        }

        private static string LoadoutWeaponRangeBandLabel(CombatWeaponDefinition weapon)
        {
            float range = weapon?.rangeMax ?? 0f;
            if (range < 450f)
            {
                return "S";
            }

            if (range < 850f)
            {
                return "M";
            }

            return "L";
        }

        private static Color LoadoutWeaponRangeBandColor(CombatWeaponDefinition weapon)
        {
            float range = weapon?.rangeMax ?? 0f;
            if (range < 450f)
            {
                return new Color(LoadoutShortWeaponColor.r, LoadoutShortWeaponColor.g, LoadoutShortWeaponColor.b, 0.92f);
            }

            if (range < 850f)
            {
                return new Color(LoadoutMediumWeaponColor.r, LoadoutMediumWeaponColor.g, LoadoutMediumWeaponColor.b, 0.92f);
            }

            return new Color(LoadoutLongWeaponColor.r, LoadoutLongWeaponColor.g, LoadoutLongWeaponColor.b, 0.92f);
        }

        private void DrawSelectedWeaponSummaryLine(
            UnitState unit,
            CombatLoadoutPreview preview,
            int selectedWeaponIndex,
            float x,
            float y,
            float width)
        {
            CombatWeaponDefinition[] weapons = unit?.Profile?.Weapons;
            if (weapons == null || selectedWeaponIndex < 0 || selectedWeaponIndex >= weapons.Length)
            {
                GUI.Label(new Rect(x, y, width, 18f), "Selected weapon unavailable");
                return;
            }

            CombatWeaponDefinition weapon = weapons[selectedWeaponIndex];
            if (weapon == null)
            {
                GUI.Label(new Rect(x, y, width, 18f), "Selected weapon unavailable");
                return;
            }

            CombatLoadoutPreviewItem selectedItem = LoadoutPreviewItemForWeapon(preview, selectedWeaponIndex);
            CombatLoadoutPreviewItem baseItem = LoadoutPreviewItemForWeapon(LoadoutBasePreviewFor(unit), selectedWeaponIndex);
            bool hasPlacementOverride = HasLoadoutWeaponPlacementOverride(unit, selectedWeaponIndex);
            Rect strip = new(x - 4f, y - 2f, width + 8f, 22f);
            DrawColorRect(strip, new Color(0.015f, 0.025f, 0.03f, 0.76f));
            Color borderColor = hasPlacementOverride ? UiAmberColor : UiCyanColor;
            DrawRectBorder(strip, new Color(borderColor.r, borderColor.g, borderColor.b, hasPlacementOverride ? 0.52f : 0.28f), 1f);
            GUI.Label(
                new Rect(x, y, width, 18f),
                LoadoutSelectedWeaponSummaryText(unit, preview, selectedWeaponIndex, weapon, selectedItem, baseItem));
        }

        private string LoadoutSelectedWeaponSummaryText(
            UnitState unit,
            CombatLoadoutPreview preview,
            int selectedWeaponIndex,
            CombatWeaponDefinition weapon,
            CombatLoadoutPreviewItem selectedItem,
            CombatLoadoutPreviewItem baseItem)
        {
            CombatLoadoutPreviewGridCell selectedCell = LoadoutCellForSelectedWeapon(preview, selectedWeaponIndex);
            int cells = Math.Max(1, CountLoadoutBlockCells(preview, selectedCell));
            string shapeText = LoadoutBlockShapeText(preview, selectedCell);
            string positionText = LoadoutWeaponPositionSummary(unit, preview, selectedItem, baseItem);
            string placementState = HasLoadoutWeaponPlacementOverride(unit, selectedWeaponIndex) ? "Moved" : "Base";
            string weaponName = ShortLoadoutItemName(weapon?.name ?? "Weapon");
            if (string.IsNullOrWhiteSpace(weaponName))
            {
                weaponName = "Weapon";
            }

            return "W"
                + (selectedWeaponIndex + 1).ToString(CultureInfo.InvariantCulture)
                + " "
                + placementState
                + " "
                + weaponName
                + positionText
                + "  D "
                + FormatDecimal(weapon?.damage ?? 0f)
                + "  R "
                + Mathf.RoundToInt(weapon?.rangeMax ?? 0f).ToString(CultureInfo.InvariantCulture)
                + "  CD "
                + FormatDecimal(weapon?.recycleTime ?? 0f)
                + "  H "
                + FormatDecimal(weapon?.heat ?? 0f)
                + "  W "
                + FormatDecimal(weapon?.weight ?? 0f)
                + "  C "
                + cells.ToString(CultureInfo.InvariantCulture)
                + "  "
                + shapeText;
        }

        private string LoadoutPlacementHeaderText(
            UnitState unit,
            CombatLoadoutPreview preview,
            int selectedWeaponIndex,
            CombatLoadoutPreviewItem selectedItem,
            CombatLoadoutPreviewItem baseItem)
        {
            if (selectedItem == null)
            {
                return "W? unavailable";
            }

            string placementState = HasLoadoutWeaponPlacementOverride(unit, selectedWeaponIndex) ? "Moved" : "Base";
            return LoadoutWeaponStateLabel(selectedWeaponIndex, placementState)
                + LoadoutWeaponPositionSummary(unit, preview, selectedItem, baseItem);
        }

        private string LoadoutWeaponPositionSummary(
            UnitState unit,
            CombatLoadoutPreview preview,
            CombatLoadoutPreviewItem selectedItem,
            CombatLoadoutPreviewItem baseItem)
        {
            if (selectedItem == null)
            {
                return "";
            }

            string currentPosition = LoadoutGridPositionText(selectedItem.GridX, selectedItem.GridY);
            string position = " @" + currentPosition;
            if (baseItem != null
                && (baseItem.GridX != selectedItem.GridX || baseItem.GridY != selectedItem.GridY))
            {
                position = " @"
                    + LoadoutGridPositionText(baseItem.GridX, baseItem.GridY)
                    + ">"
                    + currentPosition;
            }

            if (!TryGetSelectedLoadoutGridCell(unit, preview, out Vector2Int targetCell)
                || (targetCell.x == selectedItem.GridX && targetCell.y == selectedItem.GridY))
            {
                return position;
            }

            return position
                + " >"
                + LoadoutGridPositionText(targetCell.x, targetCell.y);
        }

        private static string LoadoutGridPositionText(int gridX, int gridY)
        {
            return gridX.ToString(CultureInfo.InvariantCulture)
                + ","
                + gridY.ToString(CultureInfo.InvariantCulture);
        }

        private void DrawLoadoutEditControls(UnitState unit, CombatLoadoutPreview preview, float x, float y, float width)
        {
            bool hasPendingEdits = HasPendingLoadoutEdits(unit);
            MechBayInventoryAvailabilityResult availability = CurrentDraftInventoryAvailability();
            bool hasInventory = availability != null && availability.IsValid;
            bool canApply = hasPendingEdits && preview != null && preview.Validation.IsValid && hasInventory;
            Rect lane = new(x - 4f, y - 5f, width + 8f, 30f);
            Color laneCue = canApply
                ? new Color(0.50f, 1f, 0.82f, 0.62f)
                : hasPendingEdits
                    ? new Color(1f, 0.78f, 0.28f, 0.58f)
                    : new Color(0.42f, 0.82f, 1f, 0.42f);
            DrawColorRect(lane, new Color(0.015f, 0.025f, 0.03f, 0.82f));
            DrawRectBorder(lane, laneCue, 1f);
            Color previousColor = GUI.color;
            GUI.color = !hasInventory
                ? new Color(1f, 0.78f, 0.28f, 1f)
                : hasPendingEdits
                    ? new Color(1f, 0.86f, 0.32f, 1f)
                    : new Color(0.58f, 0.82f, 1f, 1f);
            GUI.Label(
                new Rect(x, y, width - LoadoutEditStatusReservedWidth, 18f),
                LoadoutEditStatusText(
                    hasInventory,
                    hasPendingEdits,
                    FirstInventoryAvailabilityError(availability)));
            GUI.color = previousColor;

            bool previousEnabled = GUI.enabled;
            string applyLabel = LoadoutApplyButtonLabel(hasPendingEdits, preview, hasInventory);
            Color previousButtonColor = GUI.color;
            GUI.color = LoadoutApplyButtonColor(hasPendingEdits, preview, hasInventory);
            GUI.enabled = previousEnabled && canApply;
            if (GUI.Button(
                new Rect(x + width - LoadoutApplyButtonRightOffset, y - 2f, LoadoutApplyButtonWidth, 22f),
                applyLabel))
            {
                ApplyLoadoutDraft(unit, preview);
            }

            GUI.color = previousButtonColor;
            string resetLabel = LoadoutDraftResetButtonLabel(hasPendingEdits);
            GUI.color = LoadoutDraftResetButtonColor(hasPendingEdits);
            GUI.enabled = previousEnabled && hasPendingEdits;
            if (GUI.Button(
                new Rect(x + width - LoadoutResetButtonRightOffset, y - 2f, LoadoutResetButtonWidth, 22f),
                resetLabel))
            {
                ResetLoadoutDraft(unit);
            }

            GUI.color = previousButtonColor;
            GUI.enabled = previousEnabled;
        }

        private static string LoadoutApplyButtonLabel(bool hasPendingEdits, CombatLoadoutPreview preview, bool hasInventory)
        {
            if (!hasPendingEdits)
            {
                return "Done";
            }

            if (preview == null || !preview.Validation.IsValid)
            {
                return "Invalid";
            }

            return hasInventory ? "Apply" : "Stock";
        }

        private static string LoadoutEditStatusText(bool hasInventory, bool hasPendingEdits, string stockIssue)
        {
            if (!hasInventory)
            {
                return "Stock " + TruncateText(stockIssue, 24);
            }

            return hasPendingEdits ? LoadoutEditPendingLabel : LoadoutFitOkLabel;
        }

        private static Color LoadoutApplyButtonColor(bool hasPendingEdits, CombatLoadoutPreview preview, bool hasInventory)
        {
            if (!hasPendingEdits)
            {
                return new Color(0.58f, 0.82f, 1f, 1f);
            }

            if (preview == null || !preview.Validation.IsValid)
            {
                return new Color(1f, 0.34f, 0.22f, 1f);
            }

            return hasInventory
                ? new Color(0.50f, 1f, 0.82f, 1f)
                : new Color(1f, 0.78f, 0.28f, 1f);
        }

        private static Color LoadoutSelectedResetButtonColor(bool hasPlacementOverride)
        {
            return hasPlacementOverride
                ? new Color(1f, 0.78f, 0.28f, 1f)
                : new Color(0.58f, 0.82f, 1f, 1f);
        }

        private static string LoadoutSelectedResetButtonLabel(bool hasPlacementOverride)
        {
            return hasPlacementOverride ? "Reset" : "Base";
        }

        private static string LoadoutDraftResetButtonLabel(bool hasPendingEdits)
        {
            return hasPendingEdits ? "Reset" : "Clean";
        }

        private static Color LoadoutDraftResetButtonColor(bool hasPendingEdits)
        {
            return hasPendingEdits
                ? new Color(1f, 0.78f, 0.28f, 1f)
                : new Color(0.58f, 0.82f, 1f, 1f);
        }

        private static Color LoadoutTargetStatusColor(bool canPlace, bool targetClear)
        {
            if (!canPlace)
            {
                return new Color(0.58f, 0.82f, 1f, 1f);
            }

            return targetClear
                ? new Color(0.50f, 1f, 0.82f, 1f)
                : new Color(1f, 0.34f, 0.22f, 1f);
        }

        private static bool[] AllMountedWeaponsStateFor(UnitState unit)
        {
            int weaponCount = unit?.Profile?.Weapons?.Length ?? 0;
            bool[] mountedWeapons = new bool[weaponCount];
            for (int index = 0; index < mountedWeapons.Length; index++)
            {
                mountedWeapons[index] = true;
            }

            return mountedWeapons;
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
                || selectedWeaponIndex >= weaponCount
                || LoadoutPreviewItemForWeapon(preview, selectedWeaponIndex) == null)
            {
                selectedWeaponIndex = FirstPreviewWeaponIndex(preview);
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

            string key = unit.Id ?? "";
            selectedLoadoutWeaponByUnit[key] = sourceWeaponIndex;
            selectedLoadoutGridCellByUnit.Remove(key);
        }

        private static int FirstPreviewWeaponIndex(CombatLoadoutPreview preview)
        {
            CombatLoadoutPreviewItem[] items = preview?.Items ?? Array.Empty<CombatLoadoutPreviewItem>();
            for (int index = 0; index < items.Length; index++)
            {
                CombatLoadoutPreviewItem item = items[index];
                if (item != null && item.SourceWeaponIndex >= 0)
                {
                    return item.SourceWeaponIndex;
                }
            }

            return -1;
        }

        private bool TryGetSelectedLoadoutGridCell(UnitState unit, CombatLoadoutPreview preview, out Vector2Int selectedGridCell)
        {
            selectedGridCell = default;
            if (unit == null || preview == null)
            {
                return false;
            }

            string key = unit.Id ?? "";
            if (!selectedLoadoutGridCellByUnit.TryGetValue(key, out selectedGridCell))
            {
                return false;
            }

            return selectedGridCell.x >= 0
                && selectedGridCell.y >= 0
                && selectedGridCell.x < preview.GridWidth
                && selectedGridCell.y < preview.GridHeight;
        }

        private void SetSelectedLoadoutGridCell(UnitState unit, int gridX, int gridY)
        {
            if (unit == null)
            {
                return;
            }

            selectedLoadoutGridCellByUnit[unit.Id ?? ""] = new Vector2Int(gridX, gridY);
        }

        private void MoveSelectedLoadoutWeapon(UnitState unit, CombatLoadoutPreview preview, int deltaX, int deltaY)
        {
            int selectedWeaponIndex = SelectedLoadoutWeaponIndexFor(unit, preview);
            CombatLoadoutPreviewItem selectedItem = LoadoutPreviewItemForWeapon(preview, selectedWeaponIndex);
            if (selectedItem == null)
            {
                statusText = "Select mounted weapon first";
                return;
            }

            CombatLoadoutPlacementOverride[] placementOverrides = LoadoutPlacementOverridesFor(unit);
            placementOverrides[selectedWeaponIndex] = new CombatLoadoutPlacementOverride
            {
                sourceWeaponIndex = selectedWeaponIndex,
                gridX = selectedItem.GridX + deltaX,
                gridY = selectedItem.GridY + deltaY
            };
            SetSelectedLoadoutGridCell(unit, selectedItem.GridX + deltaX, selectedItem.GridY + deltaY);
            statusText = LoadoutWeaponEditStatus(
                selectedWeaponIndex,
                "Moved",
                selectedItem.GridX,
                selectedItem.GridY,
                selectedItem.GridX + deltaX,
                selectedItem.GridY + deltaY);
        }

        private void PlaceSelectedLoadoutWeaponAt(UnitState unit, CombatLoadoutPreview preview, int gridX, int gridY)
        {
            int selectedWeaponIndex = SelectedLoadoutWeaponIndexFor(unit, preview);
            CombatLoadoutPreviewItem selectedItem = LoadoutPreviewItemForWeapon(preview, selectedWeaponIndex);
            CombatLoadoutPlacementOverride[] placementOverrides = LoadoutPlacementOverridesFor(unit);
            if (selectedItem == null
                || placementOverrides == null
                || selectedWeaponIndex < 0
                || selectedWeaponIndex >= placementOverrides.Length)
            {
                statusText = "Select mounted weapon first";
                return;
            }

            placementOverrides[selectedWeaponIndex] = new CombatLoadoutPlacementOverride
            {
                sourceWeaponIndex = selectedWeaponIndex,
                gridX = gridX,
                gridY = gridY
            };
            SetSelectedLoadoutGridCell(unit, gridX, gridY);
            statusText = LoadoutWeaponEditStatus(
                selectedWeaponIndex,
                "Moved",
                selectedItem.GridX,
                selectedItem.GridY,
                gridX,
                gridY);
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
            selectedLoadoutGridCellByUnit.Remove(unit.Id ?? "");
            CombatLoadoutPreviewItem baseItem = LoadoutPreviewItemForWeapon(LoadoutBasePreviewFor(unit), selectedWeaponIndex);
            statusText = baseItem == null
                ? LoadoutWeaponStateLabel(selectedWeaponIndex, "Base")
                : LoadoutWeaponStateLabel(selectedWeaponIndex, "Base")
                    + " @"
                    + LoadoutGridPositionText(baseItem.GridX, baseItem.GridY);
        }

        private static string LoadoutWeaponEditStatus(
            int selectedWeaponIndex,
            string state,
            int fromGridX,
            int fromGridY,
            int toGridX,
            int toGridY)
        {
            return LoadoutWeaponStateLabel(selectedWeaponIndex, state)
                + " @"
                + LoadoutGridPositionText(fromGridX, fromGridY)
                + ">"
                + LoadoutGridPositionText(toGridX, toGridY);
        }

        private static string LoadoutWeaponStateLabel(int selectedWeaponIndex, string state)
        {
            return "W"
                + (selectedWeaponIndex + 1).ToString(CultureInfo.InvariantCulture)
                + " "
                + state;
        }

        private bool HasSelectedLoadoutWeaponPlacementOverride(UnitState unit, int selectedWeaponIndex)
        {
            return HasLoadoutWeaponPlacementOverride(unit, selectedWeaponIndex);
        }

        private bool HasLoadoutWeaponPlacementOverride(UnitState unit, int selectedWeaponIndex)
        {
            CombatLoadoutPlacementOverride[] placementOverrides = LoadoutPlacementOverridesFor(unit);
            return PlacementAt(placementOverrides, selectedWeaponIndex) != null;
        }

        private bool HasPendingLoadoutEdits(UnitState unit)
        {
            if (unit == null)
            {
                return false;
            }

            string key = unit.Id ?? "";
            int weaponCount = unit.Profile?.Weapons?.Length ?? 0;
            return !PlacementStatesEqual(LoadoutPlacementOverridesFor(unit), AppliedPlacementStateFor(key), weaponCount)
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
                statusText = LoadoutStockShortPrefix + FirstInventoryAvailabilityError(availability);
                return;
            }

            string key = unit.Id ?? "";
            int weaponCount = unit.Profile?.Weapons?.Length ?? 0;
            appliedLoadoutPlacementOverridesByUnit[key] = ClonePlacementOverrides(LoadoutPlacementOverridesFor(unit), weaponCount);
            appliedLoadoutFillerOverridesByUnit[key] = CloneFillerOverrides(LoadoutFillerOverridesFor(unit));
            unit.ApplyDemoLoadout(BuildAppliedLoadoutCombatOverride(unit, preview));
            statusText = LoadoutFitAppliedStatusText;
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
            selectedLoadoutGridCellByUnit.Remove(key);
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

            statusText = "Reset fit review";
        }

        private UnitLoadoutCombatOverride BuildAppliedLoadoutCombatOverride(UnitState unit, CombatLoadoutPreview preview)
        {
            CombatProfile profile = unit?.Profile;
            if (profile == null)
            {
                return null;
            }

            CombatWeaponDefinition[] weapons = profile.Weapons ?? Array.Empty<CombatWeaponDefinition>();
            int mountedCount = 0;
            float mountedWeight = 0f;
            CombatWeaponDefinition primaryWeapon = null;
            float primaryScore = float.MinValue;
            for (int index = 0; index < weapons.Length; index++)
            {
                CombatWeaponDefinition weapon = weapons[index];
                if (weapon == null)
                {
                    continue;
                }

                mountedCount++;
                mountedWeight += Mathf.Max(0f, weapon.weight);
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
            if (mountedCount <= 0)
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

            return new UnitLoadoutCombatOverride
            {
                weaponRange = profile.WeaponRange,
                weaponDamage = profile.WeaponDamage,
                weaponCooldown = profile.WeaponCooldown,
                heatPerShot = profile.HeatPerShot,
                heatDissipationPerSecond = profile.HeatDissipationPerSecond + heatSinkBonus,
                armorHardnessBonus = armorHardnessBonus,
                totalWeaponWeight = preview?.Validation.TotalWeight ?? mountedWeight,
                primaryWeaponName = string.IsNullOrEmpty(profile.PrimaryWeaponName)
                    ? string.IsNullOrEmpty(primaryWeapon?.name) ? "Mounted Weapons" : primaryWeapon.name
                    : profile.PrimaryWeaponName,
                primaryWeaponType = string.IsNullOrEmpty(profile.PrimaryWeaponType)
                    ? string.IsNullOrEmpty(primaryWeapon?.type) ? "Generic" : primaryWeapon.type
                    : profile.PrimaryWeaponType,
                primarySpecialEffect = profile.PrimarySpecialEffect != 0
                    ? profile.PrimarySpecialEffect
                    : primaryWeapon?.specialEffect ?? 0
            };
        }

        private bool HasAppliedLoadoutState(string key)
        {
            key ??= "";
            return appliedLoadoutPlacementOverridesByUnit.ContainsKey(key)
                || appliedLoadoutFillerOverridesByUnit.ContainsKey(key);
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

            statusText = "T " + LoadoutGridPositionText(gridX, gridY)
                + " " + FillerCompactActionLabel(currentCategory);
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

        private static string FillerButtonLabel(string currentCategory, bool canFill, int targetCellStack)
        {
            if (!canFill)
            {
                return "Lock";
            }

            if (targetCellStack > 1)
            {
                return "Stk";
            }

            return FillerCompactActionLabel(currentCategory);
        }

        private static Color LoadoutFillerActionButtonColor(string currentCategory, bool canCycleFiller)
        {
            if (!canCycleFiller)
            {
                return new Color(1f, 0.34f, 0.22f, 1f);
            }

            if (currentCategory == LoadoutItemCategory.ArmorPlate)
            {
                return new Color(0.35f, 0.95f, 1f, 1f);
            }

            if (currentCategory == LoadoutItemCategory.HeatSink)
            {
                return new Color(1f, 0.42f, 0.28f, 1f);
            }

            return new Color(1f, 0.78f, 0.28f, 1f);
        }

        private static string FillerCompactActionLabel(string currentCategory)
        {
            if (currentCategory == LoadoutItemCategory.ArmorPlate)
            {
                return "+Sink";
            }

            if (currentCategory == LoadoutItemCategory.HeatSink)
            {
                return "Clear";
            }

            return "+Armor";
        }

        private void DrawSystemPanel()
        {
            if (!showSystemPanel)
            {
                return;
            }

            Rect panel = SystemPanelRect();
            DrawDesignPanelFrame(panel, "System / 系统", UiAmberColor);
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

            if (GUI.Button(new Rect(panel.x + 18f, panel.y + 108f, panel.width - 36f, 30f), SystemRestartButtonLabel))
            {
                TryApplyMissionRestartRuntimeSwap();
            }

            if (GUI.Button(new Rect(panel.x + 18f, panel.y + 146f, panel.width - 36f, 30f), "Contracts"))
            {
                OpenMissionListPanelFromSystem();
            }

            if (GUI.Button(new Rect(panel.x + 18f, panel.y + 184f, panel.width - 36f, 30f), EndRunButtonLabel))
            {
                Application.Quit(0);
            }

            if (GUI.Button(new Rect(panel.x + 18f, panel.y + 222f, panel.width - 36f, 30f), SystemBackButtonLabel))
            {
                showSystemPanel = false;
                if (mission.Result == MissionResultState.InProgress)
                {
                    SetPaused(false);
                }

                SetDemoFlowScreen(mission.Result == MissionResultState.InProgress ? DemoFlowScreen.Battle : DemoFlowScreen.Debrief);
                statusText = "System closed";
            }
        }

        private void DrawMissionListPanel()
        {
            if (!showMissionListPanel)
            {
                return;
            }

            Rect panel = MissionListPanelRect();
            DrawDesignPanelFrame(panel, "Contracts / 任务", UiCyanColor);
            float x = panel.x + 18f;
            float y = panel.y + 36f;
            float width = panel.width - 36f;
            string missionId = mission?.Contract?.mission?.id ?? "mc2_01";
            int objectiveCount = mission?.Objectives?.Count ?? 0;
            int playerCount = CountPlayerUnits();
            GUI.Label(new Rect(x, y, width, 20f), ContractsHeaderText, uiHeaderStyle);
            y += 26f;
            DrawDesignInsetFrame(new Rect(x, y, width, 86f), UiCyanColor);
            GUI.Label(new Rect(x + 12f, y + 10f, width - 24f, 20f), missionId + "  " + ContractsCardTitleText);
            GUI.Label(new Rect(x + 12f, y + 34f, width - 24f, 20f), "Objectives " + objectiveCount.ToString(CultureInfo.InvariantCulture) + "  " + ContractsMapReadyText);
            GUI.Label(new Rect(x + 12f, y + 58f, width - 24f, 20f), "Squad " + playerCount.ToString(CultureInfo.InvariantCulture) + " mechs  Ready");
            y += 102f;

            if (GUI.Button(new Rect(x, y, width, 30f), ContractsLaunchButtonLabel))
            {
                bool launched = TryApplyMissionRestartRuntimeSwap();
                showMissionListPanel = !launched;
                SetDemoFlowScreen(launched ? DemoFlowScreen.Battle : DemoFlowScreen.MissionSelect);
                statusText = launched ? "Contract launched" : statusText;
            }

            y += 38f;
            float halfWidth = (width - 8f) * 0.5f;
            if (GUI.Button(new Rect(x, y, halfWidth, 30f), "Mech Lab"))
            {
                showMissionListPanel = false;
                OpenLoadoutPanel();
            }

            if (GUI.Button(new Rect(x + halfWidth + 8f, y, halfWidth, 30f), "System"))
            {
                showMissionListPanel = false;
                OpenSystemPanel();
                statusText = "System open";
            }

            y += 38f;
            string returnText = mission.Result == MissionResultState.InProgress
                ? ContractsReturnBattleButtonLabel
                : ContractsReturnDebriefButtonLabel;
            if (GUI.Button(new Rect(x, y, width, 30f), returnText))
            {
                showMissionListPanel = false;
                if (mission.Result == MissionResultState.InProgress)
                {
                    SetPaused(false);
                    SetDemoFlowScreen(DemoFlowScreen.Battle);
                }
                else
                {
                    showMissionResultPanel = true;
                    SetDemoFlowScreen(DemoFlowScreen.Debrief);
                }

                statusText = mission.Result == MissionResultState.InProgress ? "Battle resumed" : "Debrief open";
            }
        }

        private void OpenPostMissionMechBay()
        {
            showMissionResultPanel = false;
            AddCombatLogLine("Debrief accepted: open mech lab");
            OpenLoadoutPanel();
            statusText = AfterActionMechLabStatusText;
        }

        private void OpenPostMissionListPanel()
        {
            showMissionResultPanel = false;
            AddCombatLogLine("Debrief accepted: open contracts");
            OpenMissionListPanelFromSystem();
            statusText = AfterActionContractsStatusText;
        }

        private void DrawMissionResultPanel()
        {
            if (mission.Result == MissionResultState.InProgress || !showMissionResultPanel)
            {
                return;
            }

            Rect panel = new((Screen.width - 460f) * 0.5f, 72f, 460f, 380f);
            Color resultAccent = mission.Result == MissionResultState.Victory
                ? UiCyanColor
                : new Color(1f, 0.28f, 0.14f, 0.95f);
            DrawDesignPanelFrame(panel, MissionResultText() + " / 战报", resultAccent);
            GUI.Label(new Rect(panel.x + 18f, panel.y + 36f, panel.width - 36f, 42f), mission.ResultReason);
            DrawMissionResultSummary(panel, mission.ResultSummary);

            GUI.Label(new Rect(panel.x + 18f, panel.y + 270f, panel.width - 36f, 20f), DebriefNextStepText);
            float actionWidth = (panel.width - 44f) * 0.5f;
            if (GUI.Button(new Rect(panel.x + 18f, panel.y + 296f, actionWidth, 30f), "Mech Lab"))
            {
                OpenPostMissionMechBay();
            }

            if (GUI.Button(new Rect(panel.x + 26f + actionWidth, panel.y + 296f, actionWidth, 30f), "Contracts"))
            {
                OpenPostMissionListPanel();
            }

            if (GUI.Button(new Rect(panel.x + 18f, panel.y + 334f, actionWidth, 30f), SystemRestartButtonLabel))
            {
                TryApplyMissionRestartRuntimeSwap();
            }

            if (GUI.Button(new Rect(panel.x + 26f + actionWidth, panel.y + 334f, actionWidth, 30f), EndRunButtonLabel))
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
            Rect outcomeRect = new(panel.x + 18f, y - 3f, panel.width - 36f, 26f);
            DrawDesignInsetFrame(outcomeRect, summary.netResourcePoints >= 0 ? UiCyanColor : UiAmberColor);
            GUI.Label(
                new Rect(panel.x + 28f, y, panel.width - 56f, 20f),
                MissionResultOutcomeLine(summary));
            y += 30f;

            GUI.Label(
                new Rect(panel.x + 18f, y, panel.width - 36f, 20f),
                MissionResultFundsLine(summary));
            y += 22f;

            GUI.Label(
                new Rect(panel.x + 18f, y, panel.width - 36f, 20f),
                MissionResultSalvageTargetLine(summary));
            y += 22f;

            if (missionReceipt != null)
            {
                GUI.Label(
                    new Rect(panel.x + 18f, y, panel.width - 36f, 20f),
                    DebriefPayoutLabel + " " + SignedTokens(missionReceipt.TokenDelta)
                    + "    " + MechLabPartsLabel + " +" + missionReceipt.SalvageFragmentCount
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
                    MechLabBuildPrefix + AssemblyPreviewText(MechBayAssemblyPreviewService.BestAssemblyProgress(demoInventory)));
                y += 22f;
            }

            string completed = MissionResultDoneText(summary);
            if (!string.IsNullOrEmpty(completed))
            {
                GUI.Label(new Rect(panel.x + 18f, y, panel.width - 36f, 20f), "Done: " + TruncateText(completed, 58));
                y += 20f;
            }

            GUI.Label(new Rect(panel.x + 18f, y, panel.width - 36f, 20f), MissionResultCombatLine(summary));
        }

        private static string MissionResultDoneText(MissionResultSummary summary)
        {
            return SummaryItemsText(summary?.completedVisibleObjectiveTitles, 2);
        }

        private static string MissionResultOutcomeLine(MissionResultSummary summary)
        {
            return "Outcome Obj "
                + (summary?.completedVisibleObjectives ?? 0).ToString(CultureInfo.InvariantCulture)
                + "/"
                + (summary?.visibleObjectives ?? 0).ToString(CultureInfo.InvariantCulture)
                + "  Kills "
                + (summary?.destroyedEnemyUnits ?? 0).ToString(CultureInfo.InvariantCulture)
                + "  Dmg "
                + (summary?.damagedPlayerUnits ?? 0).ToString(CultureInfo.InvariantCulture)
                + "  Net "
                + SignedTokens(summary?.netResourcePoints ?? 0)
                + "  Salvage "
                + (summary?.salvageClaimCount ?? 0).ToString(CultureInfo.InvariantCulture);
        }

        private static string MissionResultCombatLine(MissionResultSummary summary)
        {
            string kills = SummaryItemsText(summary?.destroyedEnemyUnitLabels, 2);
            string damaged = SummaryItemsText(summary?.damagedPlayerUnitLabels, 2);
            string killText = string.IsNullOrEmpty(kills) ? "none" : TruncateText(kills, 26);
            string damageText = string.IsNullOrEmpty(damaged) ? "none" : TruncateText(damaged, 24);
            return "Combat: Kills " + killText + "    Damage " + damageText;
        }

        private static string MissionResultSalvageTargetLine(MissionResultSummary summary)
        {
            string targets = SummaryItemsText(summary?.destroyedStructureLabels, 2);
            string targetText = string.IsNullOrEmpty(targets)
                ? (summary?.destroyedStructures ?? 0).ToString(CultureInfo.InvariantCulture)
                : TruncateText(targets, 24);
            return DebriefSalvageLabel + " " + (summary?.salvageClaimCount ?? 0).ToString(CultureInfo.InvariantCulture)
                + "    Targets " + targetText
                + "    " + DebriefBountyLabel + " " + FormatTokens(summary?.visibleRewardResourcePoints ?? 0);
        }

        private static string MissionResultFundsLine(MissionResultSummary summary)
        {
            return DebriefPayoutLabel + " " + SignedTokens(summary?.completedRewardResourcePoints ?? 0)
                + "    Repair " + SignedTokens(-(summary?.repairCostResourcePoints ?? 0))
                + "    Net " + SignedTokens(summary?.netResourcePoints ?? 0);
        }

        private static string SummaryItemsText(string[] values, int maxItems)
        {
            if (values == null || values.Length == 0 || maxItems <= 0)
            {
                return "";
            }

            int count = Math.Min(maxItems, values.Length);
            string[] shown = new string[count];
            for (int index = 0; index < count; index++)
            {
                shown[index] = values[index] ?? "";
            }

            string text = string.Join(", ", shown);
            int remaining = values.Length - count;
            if (remaining > 0)
            {
                text += " +" + remaining.ToString(CultureInfo.InvariantCulture);
            }

            return text;
        }

        private static string FormatTokens(int value)
        {
            return value.ToString("N0", CultureInfo.InvariantCulture);
        }

        private static string FundsCostText(int value)
        {
            return FormatTokens(Math.Max(0, value)) + " funds";
        }

        private static string MechLabFundsText(int tokenBalance)
        {
            return TopStatusFundsText(tokenBalance);
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
            RefreshStartupContinueSavePreview();
            showLoadoutPanel = false;
            showMissionListPanel = false;
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
            SetDemoFlowScreen(DemoFlowScreen.System);
            statusText = "System open";
        }

        private void OpenSaveChoicePanelFromSystem()
        {
            RefreshStartupContinueSavePreview();
            showStartupContinuePanel = true;
            startupSaveChoicesOpenedFromSystem = true;
            startupNewGameConfirmPending = false;
            startupResetSlotConfirmPending = false;
            showSystemPanel = false;
            showLoadoutPanel = false;
            showMissionListPanel = false;
            showMissionMap = false;
            CloseTransientMechBayDrafts();
            if (mission.Result == MissionResultState.InProgress)
            {
                SetPaused(true);
            }

            pendingDetachedUnitId = null;
            pendingJumpOrder = false;
            SetDemoFlowScreen(DemoFlowScreen.SaveChoices);
            statusText = startupContinueSaveReady ? "Save slot open" : "New company available";
            RecordSavedAccountFileResult("Save Slot", startupContinueSaveReady, startupContinueSummaryText);
        }

        private void OpenMissionListPanelFromSystem()
        {
            showMissionListPanel = true;
            showSystemPanel = false;
            showLoadoutPanel = false;
            showStartupContinuePanel = false;
            showMissionMap = false;
            CloseTransientMechBayDrafts();
            if (mission.Result == MissionResultState.InProgress)
            {
                SetPaused(true);
            }

            pendingDetachedUnitId = null;
            pendingJumpOrder = false;
            SetDemoFlowScreen(DemoFlowScreen.MissionSelect);
            statusText = ContractsOpenStatusText;
        }

        private void TrySaveCurrentFromSaveChoices()
        {
            bool saved = TryExportSavedAccount(DefaultSavedAccountFilePath(), false, "Save slot");
            RefreshStartupContinueSavePreview();
            startupNewGameConfirmPending = false;
            startupResetSlotConfirmPending = false;
            statusText = saved ? "Current progress saved" : "Save failed";
        }

        private void TryExportCopyFromSaveChoices()
        {
            bool saved = TryExportSavedAccount(DefaultSavedAccountExportCopyPath(), false, "Save slot copy");
            RefreshStartupContinueSavePreview();
            startupNewGameConfirmPending = false;
            startupResetSlotConfirmPending = false;
            statusText = saved ? "Save copy exported" : "Export copy failed";
        }

        private void TryResetDefaultSaveSlotFromSaveChoices()
        {
            if (!TryCopyDefaultSavedAccountFile(out string copyPath, out string copyReason))
            {
                startupResetSlotConfirmPending = false;
                statusText = "Reset blocked";
                RecordSavedAccountFileResult("Reset blocked", false, copyReason);
                AddCombatLogLine("Save slot reset blocked: " + copyReason);
                Debug.LogError("MC2 save slot reset blocked: " + copyReason);
                return;
            }

            bool started = TryStartFreshDemoRun("New save slot started");
            if (!started)
            {
                RecordSavedAccountFileResult("Reset failed", false, "fresh run failed");
                return;
            }

            bool saved = TryExportSavedAccount(DefaultSavedAccountFilePath(), false, "Save slot reset");
            string detail = File.Exists(copyPath)
                ? SaveSlotResetFreshDetailText + "; " + SaveSlotResetOldCopyPrefix + SavedAccountFileName(copyPath)
                : SaveSlotResetFreshDetailText;
            RecordSavedAccountFileResult(saved ? "Reset Slot OK" : "Reset Slot failed", saved, saved ? detail : "fresh export failed");
            statusText = saved ? "Save slot reset" : "Reset save failed";
            AddCombatLogLine(saved ? "Save slot reset: " + detail : "Save slot reset failed");
            Debug.Log("MC2 save slot reset: saved=" + saved + " " + detail + " copy=" + copyReason);
        }

        private void ReturnFromSaveChoicesToSystem()
        {
            showStartupContinuePanel = false;
            startupSaveChoicesOpenedFromSystem = false;
            startupNewGameConfirmPending = false;
            startupResetSlotConfirmPending = false;
            OpenSystemPanel();
            SetDemoFlowScreen(DemoFlowScreen.System);
            statusText = "System open";
        }

        private void OpenLoadoutPanel()
        {
            showLoadoutPanel = true;
            showMissionListPanel = false;
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
            SetDemoFlowScreen(DemoFlowScreen.MechBay);
            statusText = "Mech Lab open";
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

        private void SetDemoFlowScreen(DemoFlowScreen screen)
        {
            demoFlowScreen = screen;
        }

        private static string DemoFlowScreenName(DemoFlowScreen screen)
        {
            return screen switch
            {
                DemoFlowScreen.Title => "Title",
                DemoFlowScreen.Battle => "Battle",
                DemoFlowScreen.MechBay => "Mech Lab",
                DemoFlowScreen.MissionSelect => "Contracts",
                DemoFlowScreen.SaveChoices => "Saves",
                DemoFlowScreen.System => "System",
                DemoFlowScreen.Debrief => "Debrief",
                _ => "Unknown"
            };
        }

        private static string TopStatusModeText(DemoFlowScreen screen)
        {
            return TopStatusModePrefix + DemoFlowScreenName(screen);
        }

        private static string TopStatusFundsText(int? tokenBalance)
        {
            if (!tokenBalance.HasValue)
            {
                return TopStatusFundsPrefix + "--";
            }

            return TopStatusFundsPrefix + FormatTokens(Math.Max(0, tokenBalance.Value));
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

            if (showMissionListPanel && MissionListPanelRect().Contains(guiPoint))
            {
                return true;
            }

            if (showStartupContinuePanel && StartupContinuePanelRect().Contains(guiPoint))
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
            float width = 330f;
            float height = 334f;
            return new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        }

        private Rect MissionListPanelRect()
        {
            float width = Mathf.Clamp(Screen.width - 48f, 380f, 520f);
            float height = 292f;
            return new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        }

        private Rect StartupContinuePanelRect()
        {
            float width = Mathf.Clamp(Screen.width - 48f, 360f, 460f);
            float height = startupSaveChoicesOpenedFromSystem ? 392f : 264f;
            return new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        }

        private Rect LoadoutPanelRect()
        {
            return LoadoutPanelRectFor(Screen.width, Screen.height);
        }

        private static Rect LoadoutPanelRectFor(float screenWidth, float screenHeight)
        {
            float width = Mathf.Clamp(screenWidth * 0.86f, 760f, 1160f);
            float height = Mathf.Clamp(screenHeight * 0.90f, 420f, 760f);
            return new Rect(
                Mathf.Max(12f, (screenWidth - width) * 0.5f),
                (screenHeight - height) * 0.5f,
                width,
                height);
        }

        private static bool ShouldUseDedicatedMechLabLayout(Rect panel)
        {
            return panel.width >= MechLabDedicatedLayoutMinWidth
                && panel.height >= MechLabDedicatedLayoutMinHeight;
        }

        private Rect CombatPanelRect()
        {
            return new Rect(Screen.width - 360f, 12f, 344f, 154f);
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
                DrawSectionStatusLabel(new Rect(x, y + 8, width + 8, 16), section);
                x += 58f;
            }
        }

        private void DrawSectionStatusLabel(Rect rect, DamageSection section)
        {
            Color labelColor = SectionStatusLabelColor(section);
            if (section.IsDestroyed || section.Ratio < CriticalSectionStatusRatio)
            {
                DrawColorRect(new Rect(rect.x - 1f, rect.y + 1f, rect.width - 3f, rect.height - 3f), new Color(labelColor.r, labelColor.g, labelColor.b, 0.12f));
            }

            Color previous = GUI.color;
            GUI.color = labelColor;
            GUI.Label(rect, SectionStatusLabel(section));
            GUI.color = previous;
        }

        private static string SectionStatusLabel(DamageSection section)
        {
            string sectionName = ShortSectionName(section.Name);
            if (section.IsDestroyed)
            {
                return sectionName + " X";
            }

            int percent = Mathf.RoundToInt(Mathf.Clamp01(section.Ratio) * 100f);
            return section.Ratio < CriticalSectionStatusRatio
                ? sectionName + " !" + percent.ToString(CultureInfo.InvariantCulture)
                : sectionName + " " + percent.ToString(CultureInfo.InvariantCulture);
        }

        private static Color SectionStatusLabelColor(DamageSection section)
        {
            if (section.IsDestroyed)
            {
                return new Color(1f, 0.22f, 0.14f, 1f);
            }

            if (section.Ratio < CriticalSectionStatusRatio)
            {
                return new Color(1f, 0.58f, 0.18f, 1f);
            }

            if (section.Ratio < 0.70f)
            {
                return new Color(1f, 0.82f, 0.32f, 1f);
            }

            return UiTextColor;
        }

        private Color SectionHealthColor(DamageSection section)
        {
            if (section.IsDestroyed)
            {
                return new Color(0.95f, 0.08f, 0.05f, 1f);
            }

            if (section.Ratio < CriticalSectionStatusRatio)
            {
                return new Color(1f, 0.24f, 0.08f, 1f);
            }

            if (section.Ratio < 0.70f)
            {
                return new Color(1f, 0.74f, 0.16f, 1f);
            }

            return new Color(0.20f, 0.88f, 0.92f, 1f);
        }

        private static string ShortSectionName(string sectionName)
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

        private int CountDetachedPlayerUnits()
        {
            int count = 0;
            if (mission == null)
            {
                return count;
            }

            foreach (UnitState unit in mission.PlayerUnits())
            {
                if (unit.IsActive && !unit.IsDestroyed && unit.IsDetached)
                {
                    count++;
                }
            }

            return count;
        }

        private int CountJumpingPlayerUnits()
        {
            int count = 0;
            if (mission == null)
            {
                return count;
            }

            foreach (UnitState unit in mission.PlayerUnits())
            {
                if (unit.IsActive && !unit.IsDestroyed && unit.IsJumping)
                {
                    count++;
                }
            }

            return count;
        }

        private int CountActiveHostileUnits()
        {
            int count = 0;
            if (mission == null)
            {
                return count;
            }

            foreach (UnitState unit in mission.Units)
            {
                if (!unit.IsPlayerUnit && unit.IsActive && !unit.IsDestroyed)
                {
                    count++;
                }
            }

            return count;
        }

        private int CountDestroyedHostileUnits()
        {
            int count = 0;
            if (mission == null)
            {
                return count;
            }

            foreach (UnitState unit in mission.Units)
            {
                if (!unit.IsPlayerUnit && unit.IsDestroyed)
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
