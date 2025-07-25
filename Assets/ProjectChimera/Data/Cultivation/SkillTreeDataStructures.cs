using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Cultivation
{
    /// <summary>
    /// Skill Tree Data Structures - Data structures specifically for skill tree progression system
    /// Contains tree-specific structures that don't conflict with existing definitions
    /// </summary>
    
    [System.Serializable]
    public class SkillNodeState
    {
        public string NodeId;
        public SkillNodeType NodeType;
        public SkillBranch Branch;
        public bool IsUnlocked = false;
        public bool IsConceptIntroduced = false;
        public bool IsEquipmentUnlocked = false;
        public float CurrentExperience = 0f;
        public float RequiredExperience = 100f;
        public List<string> Prerequisites = new List<string>();
        public float UnlockTimestamp = 0f;
        public Dictionary<string, object> NodeData = new Dictionary<string, object>();
    }
    
    [System.Serializable]
    public class BranchProgressionState
    {
        public SkillBranch Branch;
        public BranchDefinition Definition;
        public int UnlockedNodes = 0;
        public int TotalNodes = 0;
        public float BranchVibrancy = 0f;
        public TreeGrowthLevel BranchGrowthLevel = TreeGrowthLevel.Seed;
        public float LastProgressTime = 0f;
        public bool IsRootBranch = false;
        public Dictionary<string, object> BranchData = new Dictionary<string, object>();
    }
    
    [System.Serializable]
    public class BranchDefinition
    {
        public SkillBranch Branch;
        public int MaxNodes = 10;
        public float GrowthRate = 1f;
        public float RequiredVibrancy = 0.7f;
        public List<string> Prerequisites = new List<string>();
        public Dictionary<string, object> BranchParameters = new Dictionary<string, object>();
    }
    
    [System.Serializable]
    public class TreeProgressionMetrics
    {
        public TreeGrowthLevel CurrentGrowthLevel;
        public float OverallVibrancy;
        public float TotalExperience;
        public int UnlockedNodes;
        public int TotalNodes;
        public Dictionary<SkillBranch, BranchProgressionState> BranchStates = new Dictionary<SkillBranch, BranchProgressionState>();
    }
    
    #region Event Data Structures
    
    [System.Serializable]
    public class SkillNodeUnlockedEventData
    {
        public string NodeId;
        public SkillNodeState NodeState;
        public float Timestamp;
        public SkillBranch Branch;
        public float ExperienceGained;
        public List<string> UnlockedFeatures = new List<string>();
    }
    
    [System.Serializable]
    public class BranchProgressedEventData
    {
        public SkillBranch Branch;
        public BranchProgressionState BranchState;
        public float Timestamp;
        public TreeGrowthLevel PreviousGrowthLevel;
        public TreeGrowthLevel NewGrowthLevel;
        public float ProgressAmount;
    }
    
    [System.Serializable]
    public class TreeGrowthLevelChangedEventData
    {
        public TreeGrowthLevel PreviousLevel;
        public TreeGrowthLevel NewLevel;
        public float OverallVibrancy;
        public float Timestamp;
        public int NodesUnlocked;
        public float TotalExperience;
    }
    
    #endregion
    
    #region Additional Required Enums
    
    public enum SkillNodeType
    {
        BasicSkill,
        PlantCare,
        EnvironmentalControl,
        Genetics,
        Automation,
        AutomationUnlock,
        Construction,
        Business,
        Science,
        Core,           // Basic essential skills
        Advanced,       // Specialized techniques
        Mastery         // Expert-level skills
    }
    
    public enum ExperienceSource
    {
        ManualTask,
        AutomatedProcess,
        Discovery,
        Achievement,
        Training,
        Research
    }
    
    // Note: CareQuality is defined in BurdenCalculationConfigSO.cs
    
    public enum SkillBranch
    {
        Cultivation,
        BasicCultivation,
        AdvancedCultivation,
        Automation,
        Science,
        Business,
        Genetics,
        Processing,
        Environmental,
        Environment,
        Economic,
        Construction,
        Research,
        PostHarvest
    }
    
    public enum TreeGrowthLevel
    {
        Seed,
        Seedling,
        Sapling,
        YoungTree,
        MatureTree,
        AncientTree,
        Legendary,
        Vegetative,
        Mature,
        Flowering,
        FullyFlowered
    }
    
    #endregion
    
    #region Visualization Data Structures
    
    /// <summary>
    /// Branch visualization component data
    /// </summary>
    [System.Serializable]
    public class BranchVisualization
    {
        public SkillBranch Branch;
        public GameObject GameObject;
        public Renderer Renderer;
        public float CurrentVibrancy = 0.1f;
        public float TargetVibrancy = 0.1f;
        public TreeGrowthLevel GrowthLevel = TreeGrowthLevel.Seed;
        public bool IsAnimating = false;
        public float AnimationProgress = 0f;
    }
    
    /// <summary>
    /// Node visualization component data
    /// </summary>
    [System.Serializable]
    public class NodeVisualization
    {
        public string NodeId;
        public SkillNodeState NodeState;
        public GameObject GameObject;
        public Renderer Renderer;
        public bool IsUnlocked = false;
        public bool IsGlowing = false;
        public float GlowIntensity = 0f;
    }
    
    /// <summary>
    /// Tree growth stage enumeration
    /// </summary>
    public enum TreeGrowthStage
    {
        Seed = TreeGrowthLevel.Seed,
        Seedling = TreeGrowthLevel.Seedling,
        Sapling = TreeGrowthLevel.Sapling,
        YoungTree = TreeGrowthLevel.YoungTree,
        MatureTree = TreeGrowthLevel.MatureTree,
        AncientTree = TreeGrowthLevel.AncientTree,
        Legendary = TreeGrowthLevel.Legendary
    }
    
    #endregion
}