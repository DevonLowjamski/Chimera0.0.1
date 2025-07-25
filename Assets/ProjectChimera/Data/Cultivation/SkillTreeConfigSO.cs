using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Data.Cultivation
{
    /// <summary>
    /// Skill Tree Configuration - ScriptableObject for 3D skill tree settings
    /// Configures visual representation, growth patterns, and progression mechanics
    /// </summary>
    [CreateAssetMenu(fileName = "SkillTreeConfig", menuName = "Project Chimera/Cultivation/Skill Tree Config")]
    public class SkillTreeConfigSO : ScriptableObject
    {
        [Header("Tree Structure")]
        [Range(3, 12)] public int MaxBranches = 6;
        [Range(5, 50)] public int MaxNodesPerBranch = 20;
        [Range(0.5f, 5f)] public float BranchSpacing = 2f;
        [Range(0.2f, 2f)] public float NodeSpacing = 0.8f;
        
        [Header("Growth Visualization")]
        [Range(0.1f, 5f)] public float GrowthAnimationSpeed = 1.5f;
        [Range(0.5f, 10f)] public float BranchGrowthDuration = 3f;
        [Range(0.2f, 5f)] public float NodeUnlockDuration = 1.5f;
        [Range(0.1f, 2f)] public float TreeVibrancySpeed = 0.8f;
        
        [Header("Visual Effects")]
        public bool EnableParticleEffects = true;
        public bool EnableGlowEffects = true;
        public bool EnableAnimatedBranches = true;
        [Range(0.1f, 2f)] public float EffectIntensity = 1f;
        
        [Header("Branch Definitions")]
        [SerializeField] private SkillBranchConfig[] _branchConfigs = new SkillBranchConfig[]
        {
            new SkillBranchConfig { Branch = SkillBranch.Cultivation, BranchName = "Cultivation", MaxNodes = 15, BranchColor = Color.green },
            new SkillBranchConfig { Branch = SkillBranch.Automation, BranchName = "Automation", MaxNodes = 12, BranchColor = Color.blue },
            new SkillBranchConfig { Branch = SkillBranch.Science, BranchName = "Science", MaxNodes = 18, BranchColor = Color.cyan },
            new SkillBranchConfig { Branch = SkillBranch.Business, BranchName = "Business", MaxNodes = 10, BranchColor = Color.yellow },
            new SkillBranchConfig { Branch = SkillBranch.Genetics, BranchName = "Genetics", MaxNodes = 20, BranchColor = Color.magenta },
            new SkillBranchConfig { Branch = SkillBranch.Processing, BranchName = "Processing", MaxNodes = 8, BranchColor = Color.red }
        };
        
        [Header("Tree Growth Stages")]
        [SerializeField] private TreeGrowthStageConfig[] _growthStages = new TreeGrowthStageConfig[]
        {
            new TreeGrowthStageConfig { Stage = TreeGrowthLevel.Seed, StageName = "Seed", RequiredNodes = 0, TreeScale = 0.1f },
            new TreeGrowthStageConfig { Stage = TreeGrowthLevel.Seedling, StageName = "Seedling", RequiredNodes = 3, TreeScale = 0.3f },
            new TreeGrowthStageConfig { Stage = TreeGrowthLevel.Vegetative, StageName = "Vegetative", RequiredNodes = 8, TreeScale = 0.6f },
            new TreeGrowthStageConfig { Stage = TreeGrowthLevel.Mature, StageName = "Mature", RequiredNodes = 15, TreeScale = 0.9f },
            new TreeGrowthStageConfig { Stage = TreeGrowthLevel.Flowering, StageName = "Flowering", RequiredNodes = 25, TreeScale = 1.2f },
            new TreeGrowthStageConfig { Stage = TreeGrowthLevel.FullyFlowered, StageName = "Fully Flowered", RequiredNodes = 40, TreeScale = 1.5f }
        };
        
        [Header("Progression Settings")]
        [Range(0.1f, 10f)] public float BaseExperienceRequired = 100f;
        [Range(1f, 3f)] public float ExperienceScaling = 1.5f;
        [Range(0.1f, 5f)] public float SkillBonusMultiplier = 1.2f;
        [Range(0.5f, 3f)] public float BranchSynergyBonus = 1.3f;
        
        [Header("Visual Customization")]
        public Material BranchMaterial;
        public Material NodeMaterial;
        public Material UnlockedNodeMaterial;
        public Material LockedNodeMaterial;
        public GameObject ParticleEffectPrefab;
        public GameObject GlowEffectPrefab;
        
        // Public Properties
        public SkillBranchConfig[] BranchConfigs => _branchConfigs;
        public TreeGrowthStageConfig[] GrowthStages => _growthStages;
        
        /// <summary>
        /// Get branch configuration by branch type
        /// </summary>
        public SkillBranchConfig GetBranchConfig(SkillBranch branch)
        {
            foreach (var config in _branchConfigs)
            {
                if (config.Branch == branch)
                    return config;
            }
            
            return _branchConfigs[0]; // Return first as default
        }
        
        /// <summary>
        /// Get growth stage configuration by stage
        /// </summary>
        public TreeGrowthStageConfig GetGrowthStageConfig(TreeGrowthLevel stage)
        {
            foreach (var config in _growthStages)
            {
                if (config.Stage == stage)
                    return config;
            }
            
            return _growthStages[0]; // Return first as default
        }
        
        /// <summary>
        /// Calculate required experience for node unlock
        /// </summary>
        public float CalculateRequiredExperience(int nodeLevel, SkillBranch branch)
        {
            var branchConfig = GetBranchConfig(branch);
            var branchMultiplier = branchConfig.ExperienceMultiplier;
            
            return BaseExperienceRequired * Mathf.Pow(ExperienceScaling, nodeLevel) * branchMultiplier;
        }
        
        /// <summary>
        /// Get tree growth stage based on total unlocked nodes
        /// </summary>
        public TreeGrowthLevel GetTreeGrowthStage(int totalUnlockedNodes)
        {
            TreeGrowthLevel currentStage = TreeGrowthLevel.Seed;
            
            foreach (var stageConfig in _growthStages)
            {
                if (totalUnlockedNodes >= stageConfig.RequiredNodes)
                    currentStage = stageConfig.Stage;
                else
                    break;
            }
            
            return currentStage;
        }
        
        /// <summary>
        /// Calculate branch synergy bonus
        /// </summary>
        public float CalculateBranchSynergyBonus(Dictionary<SkillBranch, int> branchProgress)
        {
            var activeBranches = 0;
            var totalProgress = 0;
            
            foreach (var kvp in branchProgress)
            {
                if (kvp.Value > 0)
                {
                    activeBranches++;
                    totalProgress += kvp.Value;
                }
            }
            
            if (activeBranches <= 1) return 1f;
            
            var synergyFactor = (activeBranches - 1) * 0.1f;
            var progressFactor = totalProgress / 100f;
            
            return 1f + (synergyFactor * progressFactor * BranchSynergyBonus);
        }
        
        /// <summary>
        /// Get branch position in 3D space
        /// </summary>
        public Vector3 GetBranchPosition(SkillBranch branch, Vector3 treeCenter)
        {
            var branchIndex = System.Array.IndexOf(_branchConfigs.Select(c => c.Branch).ToArray(), branch);
            var angle = (branchIndex * 360f / _branchConfigs.Length) * Mathf.Deg2Rad;
            
            var x = Mathf.Cos(angle) * BranchSpacing;
            var z = Mathf.Sin(angle) * BranchSpacing;
            
            return treeCenter + new Vector3(x, 0, z);
        }
        
        private void OnValidate()
        {
            ValidateBranchConfigs();
            ValidateGrowthStages();
        }
        
        private void ValidateBranchConfigs()
        {
            for (int i = 0; i < _branchConfigs.Length; i++)
            {
                var config = _branchConfigs[i];
                
                if (string.IsNullOrEmpty(config.BranchName))
                    config.BranchName = config.Branch.ToString();
                
                config.MaxNodes = Mathf.Clamp(config.MaxNodes, 1, MaxNodesPerBranch);
                config.ExperienceMultiplier = Mathf.Clamp(config.ExperienceMultiplier, 0.5f, 3f);
            }
        }
        
        private void ValidateGrowthStages()
        {
            for (int i = 0; i < _growthStages.Length; i++)
            {
                var stage = _growthStages[i];
                
                if (string.IsNullOrEmpty(stage.StageName))
                    stage.StageName = stage.Stage.ToString();
                
                stage.TreeScale = Mathf.Clamp(stage.TreeScale, 0.1f, 3f);
                
                // Ensure stages are in ascending order
                if (i > 0)
                {
                    var previousStage = _growthStages[i - 1];
                    if (stage.RequiredNodes <= previousStage.RequiredNodes)
                        stage.RequiredNodes = previousStage.RequiredNodes + 1;
                }
            }
        }
    }
    
    [System.Serializable]
    public class SkillBranchConfig
    {
        [Header("Branch Identity")]
        public SkillBranch Branch;
        public string BranchName;
        [TextArea(1, 3)] public string Description;
        
        [Header("Branch Properties")]
        [Range(1, 50)] public int MaxNodes = 15;
        [Range(0.5f, 3f)] public float ExperienceMultiplier = 1f;
        [Range(0.1f, 2f)] public float GrowthRate = 1f;
        
        [Header("Visual Properties")]
        public Color BranchColor = Color.white;
        [Range(0.1f, 2f)] public float BranchThickness = 0.5f;
        [Range(0.5f, 5f)] public float BranchLength = 2f;
        
        [Header("Positioning")]
        [Range(0f, 360f)] public float BranchAngle = 0f;
        [Range(0.5f, 5f)] public float BranchHeight = 1f;
        public Vector3 BranchOffset = Vector3.zero;
    }
    
    [System.Serializable]
    public class TreeGrowthStageConfig
    {
        [Header("Stage Identity")]
        public TreeGrowthLevel Stage;
        public string StageName;
        [TextArea(1, 2)] public string Description;
        
        [Header("Requirements")]
        [Range(0, 100)] public int RequiredNodes;
        [Range(0f, 1000f)] public float RequiredExperience;
        
        [Header("Visual Properties")]
        [Range(0.1f, 3f)] public float TreeScale = 1f;
        [Range(0f, 1f)] public float TreeVibrancy = 0.5f;
        public Color StageColor = Color.white;
        
        [Header("Effects")]
        public bool EnableSpecialEffects = false;
        public GameObject StageEffectPrefab;
        public AudioClip StageUnlockSound;
    }
    
    // Note: SkillBranch and TreeGrowthLevel are defined in SkillTreeDataStructures.cs
}