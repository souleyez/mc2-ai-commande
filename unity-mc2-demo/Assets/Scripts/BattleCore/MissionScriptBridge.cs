using System;
using System.Collections.Generic;

namespace MC2Demo.BattleCore
{
    public sealed class MissionScriptBridge
    {
        public IReadOnlyList<MissionScriptEvent> RecentEvents => recentEvents;

        private readonly BattleMission mission;
        private readonly List<MissionScriptEvent> recentEvents = new();
        private readonly HashSet<string> emittedSignals = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> knownSignals = new(StringComparer.OrdinalIgnoreCase);
        private MissionResultState lastResult;

        public MissionScriptBridge(BattleMission mission)
        {
            this.mission = mission ?? throw new ArgumentNullException(nameof(mission));
            lastResult = mission.Result;

            string[] scriptSignals = mission.Contract.aiHooks?.scriptSignals;
            if (scriptSignals == null)
            {
                return;
            }

            foreach (string signal in scriptSignals)
            {
                if (!string.IsNullOrWhiteSpace(signal))
                {
                    knownSignals.Add(signal);
                }
            }
        }

        public void CaptureFrame()
        {
            recentEvents.Clear();
            CaptureObjectiveSignals();
            CaptureUnitActivationSignals();
            CaptureCombatSignals();
            CaptureResultSignals();
        }

        private void CaptureObjectiveSignals()
        {
            foreach (ObjectiveEvent objectiveEvent in mission.RecentObjectiveEvents)
            {
                if (objectiveEvent.Kind != ObjectiveEventKind.Completed)
                {
                    continue;
                }

                switch (objectiveEvent.ObjectiveIndex)
                {
                    case 0:
                        Emit(
                            "Objective_0_Decided",
                            MissionScriptEventKind.Objective,
                            objectiveEvent.Title,
                            "Airfield investigated; first contact chain is armed.");
                        break;
                    case 6:
                        Emit(
                            "patrols_Area_Trigger",
                            MissionScriptEventKind.Objective,
                            objectiveEvent.Title,
                            "First bandit patrol cleared; north route objective can advance.");
                        break;
                    case 7:
                        Emit(
                            "Starslayer_Trigger",
                            MissionScriptEventKind.Objective,
                            objectiveEvent.Title,
                            "Starslayer encounter trigger completed.");
                        break;
                    case 8:
                        Emit(
                            "Starslayer_VO_Check",
                            MissionScriptEventKind.Objective,
                            objectiveEvent.Title,
                            "Starslayer lance destroyed; voice-over hook is ready.");
                        break;
                }
            }
        }

        private void CaptureUnitActivationSignals()
        {
            foreach (UnitActivationEvent activationEvent in mission.RecentUnitActivationEvents)
            {
                UnitState unit = mission.FindUnit(activationEvent.UnitId);
                string signal = SignalForBrain(activationEvent.Brain, unit);
                if (signal == null)
                {
                    continue;
                }

                Emit(
                    signal,
                    MissionScriptEventKind.UnitActivation,
                    activationEvent.UnitId,
                    "Activated " + activationEvent.UnitType + " brain " + activationEvent.Brain + ".");
            }
        }

        private void CaptureCombatSignals()
        {
            foreach (CombatEvent combatEvent in mission.RecentCombatEvents)
            {
                StructureState structure = mission.FindStructure(combatEvent.TargetId);
                if (structure == null || !structure.IsObjectiveTarget)
                {
                    continue;
                }

                Emit(
                    "HangarAttacked",
                    MissionScriptEventKind.Combat,
                    combatEvent.TargetId,
                    "Objective structure is under attack.");
            }
        }

        private void CaptureResultSignals()
        {
            if (mission.Result == lastResult)
            {
                return;
            }

            lastResult = mission.Result;
            if (mission.Result == MissionResultState.Defeat)
            {
                Emit("PlayerForceDead", MissionScriptEventKind.MissionResult, "Mission", "Player force destroyed.");
            }
            else if (mission.Result == MissionResultState.Victory)
            {
                Emit("ClanForceDead", MissionScriptEventKind.MissionResult, "Mission", "Enemy force defeated.");
            }
        }

        private void Emit(string signal, MissionScriptEventKind kind, string sourceId, string message)
        {
            if (string.IsNullOrWhiteSpace(signal))
            {
                return;
            }

            if (knownSignals.Count > 0 && !knownSignals.Contains(signal))
            {
                return;
            }

            if (!emittedSignals.Add(signal))
            {
                return;
            }

            recentEvents.Add(new MissionScriptEvent(signal, kind, sourceId, message));
        }

        private static string SignalForBrain(string brain, UnitState unit)
        {
            if (string.IsNullOrEmpty(brain))
            {
                return null;
            }

            if (Contains(brain, "infantry_ambush"))
            {
                return "infantry1_triggered";
            }

            if (StartsWith(brain, "mc2_01_Pat1") || (EqualsBrain(brain, "mc2_01_LRMs") && unit != null && unit.SpawnPosition.x > 0f))
            {
                return "patrol1_triggered";
            }

            if (StartsWith(brain, "mc2_01_Pat2"))
            {
                return "patrol2_triggered";
            }

            if (EqualsBrain(brain, "mc2_01_Pat4"))
            {
                return "patrol3_triggered";
            }

            if (EqualsBrain(brain, "mc2_01_Starslayer")
                || EqualsBrain(brain, "mc2_01_Urbies")
                || (EqualsBrain(brain, "mc2_01_LRMs") && unit != null && unit.SpawnPosition.x < 0f))
            {
                return "Starslayer_Trigger";
            }

            return null;
        }

        private static bool Contains(string value, string search)
        {
            return value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool StartsWith(string value, string prefix)
        {
            return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        private static bool EqualsBrain(string value, string brain)
        {
            return string.Equals(value, brain, StringComparison.OrdinalIgnoreCase);
        }
    }

    public sealed class MissionScriptEvent
    {
        public string Signal { get; }
        public MissionScriptEventKind Kind { get; }
        public string SourceId { get; }
        public string Message { get; }

        public MissionScriptEvent(string signal, MissionScriptEventKind kind, string sourceId, string message)
        {
            Signal = signal;
            Kind = kind;
            SourceId = sourceId;
            Message = message;
        }
    }

    public enum MissionScriptEventKind
    {
        Objective,
        UnitActivation,
        Combat,
        MissionResult
    }
}
