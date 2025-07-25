using UnityEngine;
using System.Collections.Generic;
using System;
using ProjectChimera.Data.Environment;

namespace ProjectChimera.Data.Cultivation
{
    /// <summary>
    /// Cultivation Task Data - Defines cultivation task types and action structures
    /// Contains task definitions, requirements, and execution parameters
    /// </summary>
    
    [System.Serializable]
    public class CareAction
    {
        [Header("Action Identity")]
        public string ActionId;
        public string ActionName;
        public CultivationTaskType TaskType;
        public ActionComplexity Complexity = ActionComplexity.Simple;
        
        [Header("Execution Parameters")]
        public float Duration = 1f; // Minutes
        public float RequiredPrecision = 0.5f;
        public float SkillRequirement = 1f;
        public bool RequiresTools = false;
        
        [Header("Player Context")]
        public float PlayerSkillLevel = 1f;
        public float PlayerEngagement = 1f;
        public bool IsAutomated = false;
        public string[] RequiredCapabilities = new string[0];
        
        [Header("Quality Factors")]
        public float BaseQuality = 0.7f;
        public float QualityVariance = 0.2f;
        public Dictionary<string, float> QualityModifiers = new Dictionary<string, float>();
        
        [Header("Resources")]
        public ResourceRequirement[] ResourceRequirements = new ResourceRequirement[0];
        public string[] RequiredTools = new string[0];
        public EnvironmentalRequirement[] EnvironmentalRequirements = new EnvironmentalRequirement[0];
        
        public CareAction()
        {
            ActionId = Guid.NewGuid().ToString();
        }
        
        public CareAction(CultivationTaskType taskType, float playerSkillLevel)
        {
            ActionId = Guid.NewGuid().ToString();
            TaskType = taskType;
            PlayerSkillLevel = playerSkillLevel;
            ActionName = taskType.ToString();
        }
        
        /// <summary>
        /// Calculate the effective quality of this care action
        /// </summary>
        public float CalculateEffectiveQuality()
        {
            var skillFactor = Mathf.Clamp01(PlayerSkillLevel / SkillRequirement);
            var engagementFactor = PlayerEngagement;
            var precisionFactor = RequiredPrecision;
            
            var effectiveQuality = BaseQuality * skillFactor * engagementFactor;
            
            // Apply automation penalty/bonus
            if (IsAutomated)
            {
                effectiveQuality *= 0.9f; // Slight penalty for automation
                effectiveQuality += 0.1f; // But consistency bonus
            }
            
            // Apply quality modifiers
            foreach (var modifier in QualityModifiers)
            {
                effectiveQuality *= modifier.Value;
            }
            
            // Add variance
            var variance = UnityEngine.Random.Range(-QualityVariance, QualityVariance);
            effectiveQuality += variance;
            
            return Mathf.Clamp01(effectiveQuality);
        }
        
        /// <summary>
        /// Check if action can be performed
        /// </summary>
        public bool CanPerform(PlayerCultivationState playerState, ProjectChimera.Data.Environment.EnvironmentalConditions environment)
        {
            // Check skill requirements
            if (PlayerSkillLevel < SkillRequirement)
                return false;
            
            // Check required capabilities
            foreach (var capability in RequiredCapabilities)
            {
                if (!playerState.HasCapability(capability))
                    return false;
            }
            
            // Check tool requirements
            if (RequiresTools)
            {
                foreach (var tool in RequiredTools)
                {
                    if (!playerState.HasTool(tool))
                        return false;
                }
            }
            
            // Check resource requirements
            foreach (var requirement in ResourceRequirements)
            {
                if (!playerState.HasResource(requirement.ResourceType, requirement.Amount))
                    return false;
            }
            
            // Check environmental requirements
            foreach (var requirement in EnvironmentalRequirements)
            {
                if (!requirement.IsMet(environment))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Get experience gained from performing this action
        /// </summary>
        public float GetExperienceGain()
        {
            var baseExperience = (float)Complexity * 10f;
            var qualityBonus = CalculateEffectiveQuality() * 0.5f;
            var difficultyBonus = SkillRequirement * 0.3f;
            
            return baseExperience + qualityBonus + difficultyBonus;
        }
    }
    
    [System.Serializable]
    public class ResourceRequirement
    {
        public ResourceType ResourceType;
        public float Amount;
        public bool IsConsumed = true;
        public float QualityImpact = 1f;
        
        public ResourceRequirement(ResourceType type, float amount)
        {
            ResourceType = type;
            Amount = amount;
        }
    }
    
    [System.Serializable]
    public class EnvironmentalRequirement
    {
        public EnvironmentalFactor Factor;
        public float MinValue;
        public float MaxValue;
        public float OptimalValue;
        public bool IsOptional = false;
        
        public bool IsMet(ProjectChimera.Data.Environment.EnvironmentalConditions conditions)
        {
            var value = conditions.GetValue(Factor);
            return value >= MinValue && value <= MaxValue;
        }
        
        public float GetOptimalityScore(ProjectChimera.Data.Environment.EnvironmentalConditions conditions)
        {
            var value = conditions.GetValue(Factor);
            var distance = Mathf.Abs(value - OptimalValue);
            var range = MaxValue - MinValue;
            
            return 1f - (distance / range);
        }
    }
    
    [System.Serializable]
    public class PlayerCultivationState
    {
        [Header("Player Skills")]
        public Dictionary<SkillBranch, float> SkillLevels = new Dictionary<SkillBranch, float>();
        public List<string> UnlockedCapabilities = new List<string>();
        public List<string> UnlockedNodes = new List<string>();
        
        [Header("Equipment and Tools")]
        public List<string> AvailableTools = new List<string>();
        public List<string> AvailableEquipment = new List<string>();
        public Dictionary<string, float> ToolConditions = new Dictionary<string, float>();
        
        [Header("Resources")]
        public Dictionary<ResourceType, float> AvailableResources = new Dictionary<ResourceType, float>();
        public float Currency = 0f;
        
        [Header("Experience and Progression")]
        public float TotalExperience = 0f;
        public Dictionary<SkillBranch, float> BranchExperience = new Dictionary<SkillBranch, float>();
        public int PlayerLevel = 1;
        
        public bool HasCapability(string capability)
        {
            return UnlockedCapabilities.Contains(capability);
        }
        
        public bool HasTool(string tool)
        {
            return AvailableTools.Contains(tool);
        }
        
        public bool HasResource(ResourceType resourceType, float amount)
        {
            return AvailableResources.ContainsKey(resourceType) && 
                   AvailableResources[resourceType] >= amount;
        }
        
        public float GetSkillLevel(SkillBranch branch)
        {
            return SkillLevels.ContainsKey(branch) ? SkillLevels[branch] : 0f;
        }
        
        public void ConsumeResource(ResourceType resourceType, float amount)
        {
            if (HasResource(resourceType, amount))
            {
                AvailableResources[resourceType] -= amount;
            }
        }
        
        public void AddExperience(SkillBranch branch, float experience)
        {
            if (!BranchExperience.ContainsKey(branch))
                BranchExperience[branch] = 0f;
            
            BranchExperience[branch] += experience;
            TotalExperience += experience;
            
            // Update skill level based on experience
            UpdateSkillLevel(branch);
        }
        
        private void UpdateSkillLevel(SkillBranch branch)
        {
            var experience = BranchExperience[branch];
            var newLevel = Mathf.Sqrt(experience / 100f); // Simple square root progression
            
            SkillLevels[branch] = newLevel;
        }
    }
    
    // EnvironmentalConditions class is defined in ProjectChimera.Data.Environment namespace to avoid duplicates
    
    #region Enums
    
    public enum CultivationTaskType
    {
        None,
        Watering,
        Feeding,
        Fertilizing,
        Pruning,
        Training,
        Monitoring,
        Harvesting,
        Transplanting,
        Cleaning,
        Maintenance,
        Research,
        PestControl,
        Defoliation,
        EnvironmentalControl,
        EnvironmentalAdjustment,
        DataCollection,
        Breeding,
        Processing,
        Construction,
        Automation
    }
    
    public enum ActionComplexity
    {
        Simple = 1,
        Moderate = 2,
        Complex = 3,
        Advanced = 4,
        Expert = 5
    }
    
    public enum ResourceType
    {
        Water,
        Nutrients,
        Seeds,
        SoilMix,
        Fertilizer,
        PestControl,
        Energy,
        Tools,
        Time,
        Knowledge
    }
    
    // EnvironmentalFactor enum is now defined in ProjectChimera.Data.Environment.EnvironmentalConditions
    
    // LightSpectrum enum is defined in LightSpectrum.cs to avoid duplicates
    
    public enum TrainingMethod
    {
        LST,        // Low Stress Training
        HST,        // High Stress Training
        SCROG,      // Screen of Green
        SOG,        // Sea of Green
        Topping,
        Fimming,
        Supercropping,
        Defoliation
    }
    
    #endregion
    
    #region Supporting Classes
    
    /// <summary>
    /// Container for growing plants (pots, hydroponic containers, etc.)
    /// </summary>
    [System.Serializable]
    public class Container
    {
        public string ContainerId;
        public string ContainerName;
        public ContainerType Type;
        public float Volume; // Liters
        public float DrainageRating; // 0-1
        public Material Material;
        public Vector3 Dimensions;
        public bool HasDrainageHoles = true;
        public float RootSpaceMultiplier = 1f;
        
        public Container()
        {
            ContainerId = System.Guid.NewGuid().ToString();
        }
    }
    
    /// <summary>
    /// Skill milestone for tracking cultivation progression
    /// </summary>
    [System.Serializable]
    public class SkillMilestone
    {
        public string MilestoneId;
        public string MilestoneName;
        public string Description;
        public SkillBranch RequiredBranch;
        public float RequiredLevel;
        public int RequiredCompletions;
        public CultivationTaskType[] RequiredTasks;
        public bool IsUnlocked;
        public bool IsCompleted;
        public UnityEngine.Events.UnityEvent OnMilestoneReached;
        
        public SkillMilestone()
        {
            MilestoneId = System.Guid.NewGuid().ToString();
        }
    }
    
    // GrowingMedium class is defined in InteractivePlant.cs to avoid duplicates
    
    // ContainerType and MediumType enums are defined in other files to avoid duplicates
    
    #endregion
}