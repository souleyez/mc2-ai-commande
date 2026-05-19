using System;

namespace MC2Demo.BattleCore
{
    [Serializable]
    public sealed class MissionContract
    {
        public string schema;
        public ContractSource source;
        public MissionDefinition mission;
        public UnitSpawn[] units;
        public ObjectiveDefinition[] objectives;
        public ObjectiveEdge[] objectiveEdges;
        public StaticObjectSpawn[] staticObjects;
        public TerrainMeshDefinition terrainMesh;
        public TerrainObjectSpawn[] terrainObjects;
        public NavMarker[] navMarkers;
        public ForestRegion[] forests;
        public AiHooks aiHooks;
        public BoundaryNotes battleCoreBoundary;
    }

    [Serializable]
    public sealed class ContractSource
    {
        public string packId;
        public string missionId;
    }

    [Serializable]
    public sealed class MissionDefinition
    {
        public string id;
        public string displayName;
        public string author;
        public string scriptName;
        public MissionLimits limits;
        public CameraDefinition camera;
        public TerrainDefinition terrain;
    }

    [Serializable]
    public sealed class MissionLimits
    {
        public float timeSeconds;
        public float dropWeight;
        public int resourcePoints;
        public int maxTeams;
        public int maxPlayers;
    }

    [Serializable]
    public sealed class CameraDefinition
    {
        public float projectionAngle;
        public MissionVector3 startPosition;
        public float startRotation;
        public float newScale;
        public float zoomMin;
        public float zoomMax;
    }

    [Serializable]
    public sealed class TerrainDefinition
    {
        public float minX;
        public float minY;
        public float waterElevation;
    }

    [Serializable]
    public sealed class UnitSpawn
    {
        public string spawnId;
        public int sourceIndex;
        public int teamId;
        public int commanderId;
        public int pilotId;
        public bool isPlayerUnit;
        public string unitType;
        public string objectProfile;
        public int objectNumber;
        public int variantNumber;
        public string brain;
        public int squadId;
        public string activationFlagId;
        public bool activateOnObjective;
        public int activationObjectiveIndex;
        public MissionPose position;
    }

    [Serializable]
    public sealed class MissionPose
    {
        public float x;
        public float y;
        public float rotation;
    }

    [Serializable]
    public sealed class StaticObjectSpawn
    {
        public string objectId;
        public string source;
        public int objectiveIndex;
        public string objectType;
        public string objectProfile;
        public int teamId;
        public bool targetable;
        public bool objectiveTarget;
        public MissionPose position;
        public float radius;
        public float maxStructure;
    }

    [Serializable]
    public sealed class TerrainObjectSpawn
    {
        public string objectId;
        public int sourceIndex;
        public int fitId;
        public string fileName;
        public string objectClass;
        public string specialType;
        public string textureName;
        public int teamId;
        public int parentId;
        public int damage;
        public TerrainObjectPose position;
    }

    [Serializable]
    public sealed class TerrainMeshDefinition
    {
        public int sourceSide;
        public int sampleSide;
        public int sampleStep;
        public float worldUnitsPerVertex;
        public float minX;
        public float minY;
        public float elevationMin;
        public float elevationMax;
        public TerrainMeshSample[] samples;
    }

    [Serializable]
    public sealed class TerrainMeshSample
    {
        public float elevation;
        public int terrainType;
        public int water;
        public long textureData;
        public long light;
    }

    [Serializable]
    public sealed class TerrainObjectPose
    {
        public float x;
        public float y;
        public float z;
        public float rotation;
    }

    [Serializable]
    public sealed class MissionVector3
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public sealed class ObjectiveDefinition
    {
        public string id;
        public int index;
        public int team;
        public string title;
        public int titleResourceStringId;
        public bool hidden;
        public bool activateOnFlag;
        public string activateFlagId;
        public bool requiresAllPreviousPrimary;
        public bool displayMarker;
        public MissionPoint marker;
        public int rewardResourcePoints;
        public ObjectiveCondition[] conditions;
        public ObjectiveAction[] actions;
    }

    [Serializable]
    public sealed class ObjectiveCondition
    {
        public string type;
        public int sourceIndex;
        public TargetArea targetArea;
        public TargetUnit targetUnit;
        public TargetStructure targetStructure;
    }

    [Serializable]
    public sealed class ObjectiveAction
    {
        public string type;
        public int sourceIndex;
        public FlagAction flag;
    }

    [Serializable]
    public sealed class TargetArea
    {
        public float x;
        public float y;
        public float radius;
    }

    [Serializable]
    public sealed class TargetUnit
    {
        public int commander;
        public int group;
        public int mate;
        public MissionPoint position;
        public MissionCell cell;
    }

    [Serializable]
    public sealed class TargetStructure
    {
        public int commander;
        public MissionPoint position;
        public MissionCell cell;
    }

    [Serializable]
    public sealed class FlagAction
    {
        public string id;
        public bool value;
    }

    [Serializable]
    public sealed class ObjectiveEdge
    {
        public int from;
        public int to;
        public string flagId;
    }

    [Serializable]
    public sealed class MissionPoint
    {
        public float x;
        public float y;
    }

    [Serializable]
    public sealed class MissionCell
    {
        public int x;
        public int y;
    }

    [Serializable]
    public sealed class NavMarker
    {
        public int index;
        public float x;
        public float y;
        public float radius;
    }

    [Serializable]
    public sealed class ForestRegion
    {
        public int index;
        public string name;
        public MissionPoint center;
        public float radius;
        public bool random;
    }

    [Serializable]
    public sealed class AiHooks
    {
        public string[] enemyBrains;
        public string[] scriptSignals;
    }

    [Serializable]
    public sealed class BoundaryNotes
    {
        public string[] owns;
        public string[] unityPresentationOwns;
    }
}
