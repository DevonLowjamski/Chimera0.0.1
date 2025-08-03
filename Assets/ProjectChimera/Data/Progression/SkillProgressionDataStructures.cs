using System;
using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Equipment;

namespace ProjectChimera.Data.Progression.Skills
{
    /// <summary>
    /// Skill progression data structures extracted from ProgressionDataStructures.cs
    /// Contains skill system, learning paths, synergies, expertise areas, and skill trees
    /// Part of Phase 1 Foundation Data Structures refactoring
    /// </summary>

    #region Core Skill System Types

    /// <summary>
    /// Skill category enumeration for organizing skill types
    /// </summary>
    public enum SkillCategory
    {
        Cultivation,
        Genetics,
        Business,
        Processing,
        Technology,
        Construction,
        Environment,
        Research,
        Marketing,
        Finance,
        Quality_Control,
        Automation,
        Post_Harvest,
        Security,
        Compliance
    }

    /// <summary>
    /// Skill domain enumeration for skill specialization areas
    /// </summary>
    public enum SkillDomain
    {
        Growing,
        Breeding,
        Management,
        Laboratory,
        Engineering,
        Development,
        Control,
        Innovation,
        Sales,
        Analysis,
        Production,
        Operations,
        Processing,
        Safety,
        Legal
    }

    /// <summary>
    /// Learning method types for skill acquisition
    /// </summary>
    public enum LearningMethod
    {
        Hands_On_Practice,
        Mentorship,
        Research_Study,
        Trial_And_Error,
        Online_Course,
        Workshop,
        Certification,
        Collaboration,
        Self_Directed_Learning,
        Equipment_Training
    }

    /// <summary>
    /// Synergy types for skill interactions
    /// </summary>
    public enum SynergyType
    {
        Multiplicative,
        Additive,
        Conditional,
        Threshold_Based,
        Exponential,
        Diminishing_Returns
    }

    /// <summary>
    /// Learning path types for structured progression
    /// </summary>
    public enum LearningPathType
    {
        Sequential,
        Branching,
        Flexible,
        Guided,
        Self_Paced,
        Accelerated,
        Comprehensive,
        Specialized
    }

    #endregion

    #region Skill Requirements and Costs

    /// <summary>
    /// Requirements for learning a skill
    /// </summary>
    [System.Serializable]
    public class SkillRequirements
    {
        public int MinimumPlayerLevel = 1;
        public List<string> PrerequisiteSkills = new List<string>();
        public float MinimumExperience = 0f;
        public int RequiredSkillPoints = 1;
        public bool RequiresMentor = false;
        public List<string> RequiredEquipment = new List<string>();
        public List<string> RequiredResearch = new List<string>();
        public int MinimumFacilityLevel = 1;
        public float MinimumReputation = 0f;
        public bool RequiresLicense = false;
    }

    /// <summary>
    /// Cost structure for leveling up skills
    /// </summary>
    [System.Serializable]
    public class SkillCost
    {
        public int SkillLevel = 1;
        public int SkillPointCost = 1;
        public float MoneyCost = 0f;
        public float TimeCost = 1f; // In game hours
        public List<string> RequiredMaterials = new List<string>();
        public Dictionary<string, int> MaterialQuantities = new Dictionary<string, int>();
        public bool RequiresMentor = false;
        public float MentorshipCost = 0f;
    }

    #endregion

    #region Skill Effects and Bonuses

    /// <summary>
    /// Effect applied by a skill
    /// </summary>
    [System.Serializable]
    public class SkillEffect
    {
        public string EffectId;
        public string EffectName;
        public string Description;
        public SkillEffectType EffectType;
        public string TargetSystem;
        public float EffectMagnitude = 1f;
        public bool IsPercentage = false;
        public bool IsStackable = true;
        public int MaxStacks = 1;
        public float Duration = -1f; // -1 = permanent
        public List<string> AffectedProcesses = new List<string>();
    }

    /// <summary>
    /// Bonus provided by a skill
    /// </summary>
    [System.Serializable]
    public class SkillBonus
    {
        public string BonusId;
        public string BonusName;
        public BonusType BonusType;
        public float BonusValue = 0f;
        public bool IsPercentageBonus = false;
        public string TargetAttribute;
        public List<string> ApplicableScenarios = new List<string>();
        public bool RequiresActivation = false;
        public float ActivationCost = 0f;
        public float CooldownTime = 0f;
    }

    /// <summary>
    /// Feature unlocked by a skill
    /// </summary>
    [System.Serializable]
    public class UnlockableFeature
    {
        public string FeatureId;
        public string FeatureName;
        public string Description;
        public UnlockableFeatureType FeatureType;
        public string UnlockCondition;
        public bool IsVisible = true;
        public bool RequiresActivation = false;
        public List<string> Dependencies = new List<string>();
    }

    /// <summary>
    /// Practical application of a skill in gameplay
    /// </summary>
    [System.Serializable]
    public class PracticalApplication
    {
        public string ApplicationId;
        public string ApplicationName;
        public string Description;
        public ApplicationContext Context;
        public float SuccessRateBonus = 0f;
        public float EfficiencyBonus = 0f;
        public float QualityBonus = 0f;
        public List<string> RequiredConditions = new List<string>();
        public float ExperienceGain = 1f;
    }

    #endregion

    #region Equipment and Process Unlocks

    /// <summary>
    /// Equipment unlocked by a skill
    /// </summary>
    [System.Serializable]
    public class EquipmentUnlock
    {
        public string UnlockId;
        public EquipmentDataSO Equipment;
        public int RequiredSkillLevel = 1;
        public bool IsAvailableForPurchase = true;
        public bool IsAvailableForCrafting = false;
        public float DiscountPercentage = 0f;
        public List<string> AdditionalRequirements = new List<string>();
    }

    /// <summary>
    /// Process unlocked by a skill
    /// </summary>
    [System.Serializable]
    public class ProcessUnlock
    {
        public string ProcessId;
        public string ProcessName;
        public string Description;
        public ProcessType ProcessType;
        public int RequiredSkillLevel = 1;
        public float EfficiencyBonus = 0f;
        public float QualityBonus = 0f;
        public bool RequiresEquipment = false;
        public List<string> RequiredEquipmentTypes = new List<string>();
    }

    /// <summary>
    /// Research unlocked by a skill
    /// </summary>
    [System.Serializable]
    public class ResearchUnlock
    {
        public string ResearchId;
        public string ResearchName;
        public string Description;
        public ResearchCategory Category;
        public int RequiredSkillLevel = 1;
        public float ResearchSpeedBonus = 0f;
        public float SuccessRateBonus = 0f;
        public List<string> PrerequisiteResearch = new List<string>();
    }

    #endregion

    #region Mastery and Specialization

    /// <summary>
    /// Bonus granted at skill mastery levels
    /// </summary>
    [System.Serializable]
    public class MasteryBonus
    {
        public string BonusId;
        public string BonusName;
        public int MasteryLevel = 5;
        public MasteryBonusType BonusType;
        public float BonusValue = 0f;
        public string Description;
        public bool IsUnique = true;
        public List<string> AffectedSystems = new List<string>();
        public bool RequiresActivation = false;
    }

    /// <summary>
    /// Specialization path for advanced skill development
    /// </summary>
    [System.Serializable]
    public class SpecializationPath
    {
        public string PathId;
        public string PathName;
        public string Description;
        public SpecializationType SpecializationType;
        public List<string> RequiredSkills = new List<string>();
        public int MinimumSkillLevel = 5;
        public List<SpecializationBonus> Bonuses = new List<SpecializationBonus>();
        public List<string> UnlockedCapabilities = new List<string>();
        public bool IsExclusive = false;
    }

    /// <summary>
    /// Bonus granted by specialization
    /// </summary>
    [System.Serializable]
    public class SpecializationBonus
    {
        public string BonusName;
        public SpecializationBonusType BonusType;
        public float BonusValue = 0f;
        public string TargetArea;
        public bool IsPercentageBonus = false;
        public string Description;
    }

    #endregion

    #region Skill Synergies and Combinations

    /// <summary>
    /// Synergy between multiple skills
    /// </summary>
    [System.Serializable]
    public class SkillSynergy
    {
        public string SynergyId;
        public string Name;
        public string Description;
        public List<string> RequiredSkills = new List<string>();
        public SynergyType SynergyType;
        public float BonusMultiplier = 1.2f;
        public List<string> AffectedSkills = new List<string>();
        public List<SynergyEffect> SynergyEffects = new List<SynergyEffect>();
        public bool IsActive = false;
        
        // Additional properties for SkillTreeManager compatibility
        public SkillNodeSO PrimarySkill; // Primary skill for synergy
        public SkillNodeSO SecondarySkill; // Secondary skill for synergy
        public float SynergyStrength = 1.0f; // Strength of the synergy effect
        
        // Method for checking if both skills are required
        public bool RequiresBothSkills()
        {
            return PrimarySkill != null && SecondarySkill != null;
        }
        
        public int MinimumCombinedLevel = 5; // Minimum combined level for synergy activation
    }

    /// <summary>
    /// Effect created by skill synergy
    /// </summary>
    [System.Serializable]
    public class SynergyEffect
    {
        public string EffectName;
        public SynergyEffectType EffectType;
        public float EffectMagnitude = 1f;
        public string TargetSystem;
        public bool IsStackable = false;
        public float Duration = -1f; // -1 = permanent
        public string Description;
    }

    #endregion

    #region Expertise and Learning Systems

    /// <summary>
    /// Expertise area definition
    /// </summary>
    [System.Serializable]
    public class ExpertiseArea
    {
        public string AreaId;
        public string Name;
        public string Description;
        public List<SkillCategory> RelevantCategories = new List<SkillCategory>();
        public int RequiredSkillCount = 5;
        public int RequiredMasteryCount = 3;
        public float ExpertiseThreshold = 0.8f;
        public List<ExpertiseBenefit> Benefits = new List<ExpertiseBenefit>();
    }

    /// <summary>
    /// Benefit provided by expertise
    /// </summary>
    [System.Serializable]
    public class ExpertiseBenefit
    {
        public string BenefitName;
        public ExpertiseBenefitType BenefitType;
        public float BenefitValue = 0f;
        public string Description;
        public bool IsPercentageBonus = false;
        public List<string> ApplicableAreas = new List<string>();
    }

    /// <summary>
    /// Learning path definition
    /// </summary>
    [System.Serializable]
    public class LearningPath
    {
        public string PathId;
        public string Name;
        public string Description;
        public LearningPathType PathType;
        public List<string> SkillSequence = new List<string>();
        public List<PathMilestone> Milestones = new List<PathMilestone>();
        public float EstimatedDurationDays = 30f;
        public int DifficultyLevel = 1;
    }

    /// <summary>
    /// Milestone within a learning path
    /// </summary>
    [System.Serializable]
    public class PathMilestone
    {
        public string MilestoneId;
        public string Name;
        public string Description;
        public int StepNumber = 1;
        public List<string> RequiredSkills = new List<string>();
        public List<int> RequiredSkillLevels = new List<int>();
        public List<PathMilestoneReward> Rewards = new List<PathMilestoneReward>();
        public bool IsOptional = false;
    }

    /// <summary>
    /// Reward for completing a learning path milestone
    /// </summary>
    [System.Serializable]
    public class PathMilestoneReward
    {
        public string RewardName;
        public PathRewardType RewardType;
        public float RewardValue = 0f;
        public string Description;
        public bool IsBonus = false;
    }

    #endregion

    #region Skill Tree Structure

    /// <summary>
    /// Skill tree branch definition
    /// </summary>
    [System.Serializable]
    public class SkillTreeBranch
    {
        public string BranchId;
        public string BranchName;
        public string Description;
        public SkillCategory Category;
        public SkillDomain Domain;
        public List<SkillNodeSO> Skills = new List<SkillNodeSO>();
        public Vector2 BranchPosition; // For UI positioning
        public Color BranchColor = Color.white;
        public string IconPath;
        public List<string> PrerequisiteBranches = new List<string>();
        public int UnlockLevel = 1;
        public bool IsVisible = true;
    }

    /// <summary>
    /// Skill tree data for player progression tracking
    /// </summary>
    [System.Serializable]
    public class SkillTreeData
    {
        public string PlayerId;
        public Dictionary<string, int> SkillLevels = new Dictionary<string, int>();
        public Dictionary<string, float> SkillExperience = new Dictionary<string, float>();
        public List<string> UnlockedSkills = new List<string>();
        public List<string> MasteredSkills = new List<string>();
        public List<string> ActiveSynergies = new List<string>();
        public int AvailableSkillPoints = 0;
        public DateTime LastUpdate;
    }

    #endregion

    #region Learning Accelerators and Boosts

    /// <summary>
    /// Learning accelerator definition
    /// </summary>
    [System.Serializable]
    public class LearningAccelerator
    {
        public string AcceleratorId;
        public string Name;
        public string Description;
        public AcceleratorType AcceleratorType;
        public float EffectMagnitude = 1.5f;
        public float Duration = 24f; // In game hours
        public List<AcceleratorRequirement> Requirements = new List<AcceleratorRequirement>();
        public List<string> AffectedSkills = new List<string>();
        public float CooldownTime = 0f;
        public bool IsStackable = false;
    }

    /// <summary>
    /// Requirement for using a learning accelerator
    /// </summary>
    [System.Serializable]
    public class AcceleratorRequirement
    {
        public AcceleratorRequirementType RequirementType;
        public float RequiredValue = 0f;
        public string RequiredItem;
        public string Description;
    }

    /// <summary>
    /// Collaboration opportunity for skill learning
    /// </summary>
    [System.Serializable]
    public class CollaborationOpportunity
    {
        public string OpportunityId;
        public string Name;
        public string Description;
        public CollaborationType CollaborationType;
        public List<string> RequiredSkills = new List<string>();
        public List<int> MinimumSkillLevels = new List<int>();
        public float ExperienceMultiplier = 1.5f;
        public List<CollaborationBenefit> Benefits = new List<CollaborationBenefit>();
        public float Duration = 7f; // In game days
        public int MaxParticipants = 4;
        public float ParticipationCost = 0f;
    }

    /// <summary>
    /// Benefit from collaboration
    /// </summary>
    [System.Serializable]
    public class CollaborationBenefit
    {
        public string BenefitName;
        public CollaborationBenefitType BenefitType;
        public float BenefitValue = 0f;
        public string Description;
        public bool IsShared = true; // Benefit shared among participants
    }

    #endregion

    #region Supporting Enums

    public enum SkillEffectType
    {
        Efficiency_Boost,
        Quality_Improvement,
        Speed_Increase,
        Cost_Reduction,
        Success_Rate_Boost,
        Experience_Multiplier,
        Resource_Conservation,
        Automation_Enhancement,
        Safety_Improvement,
        Innovation_Catalyst
    }

    public enum BonusType
    {
        Flat_Bonus,
        Percentage_Bonus,
        Multiplicative_Bonus,
        Threshold_Bonus,
        Conditional_Bonus,
        Time_Based_Bonus,
        Stack_Based_Bonus
    }

    public enum UnlockableFeatureType
    {
        Equipment_Access,
        Process_Unlock,
        Research_Option,
        Facility_Feature,
        UI_Element,
        Game_Mechanic,
        Advanced_Option,
        Automation_Feature
    }

    public enum ApplicationContext
    {
        Cultivation,
        Processing,
        Business_Management,
        Research,
        Quality_Control,
        Equipment_Operation,
        Facility_Management,
        Market_Analysis,
        Compliance,
        Innovation
    }

    public enum ProcessType
    {
        Cultivation_Process,
        Extraction_Process,
        Quality_Testing,
        Business_Process,
        Research_Method,
        Automation_Sequence,
        Safety_Protocol,
        Compliance_Check
    }

    public enum ResearchCategory
    {
        Genetics,
        Cultivation_Techniques,
        Processing_Methods,
        Equipment_Development,
        Market_Analysis,
        Quality_Enhancement,
        Automation_Systems,
        Environmental_Control,
        Business_Strategy,
        Innovation_Research
    }

    public enum MasteryBonusType
    {
        Global_Efficiency,
        Category_Expertise,
        Innovation_Rate,
        Teaching_Ability,
        Research_Speed,
        Quality_Mastery,
        Cost_Optimization,
        Time_Management,
        Leadership_Bonus,
        Specialization_Unlock
    }

    public enum SpecializationType
    {
        Cultivation_Master,
        Genetics_Expert,
        Business_Specialist,
        Technology_Innovator,
        Quality_Specialist,
        Research_Leader,
        Operations_Manager,
        Market_Analyst,
        Process_Engineer,
        Automation_Expert
    }

    public enum SpecializationBonusType
    {
        Efficiency_Master,
        Quality_Expert,
        Innovation_Leader,
        Cost_Optimizer,
        Speed_Specialist,
        Research_Pioneer,
        Teaching_Master,
        Safety_Expert,
        Compliance_Specialist,
        Market_Strategist
    }

    public enum SynergyEffectType
    {
        Efficiency_Synergy,
        Quality_Synergy,
        Innovation_Synergy,
        Learning_Synergy,
        Cost_Synergy,
        Time_Synergy,
        Safety_Synergy,
        Research_Synergy,
        Market_Synergy,
        Process_Synergy
    }

    public enum ExpertiseBenefitType
    {
        Experience_Multiplier,
        Skill_Efficiency,
        Research_Speed,
        Quality_Bonus,
        Cost_Reduction,
        Time_Reduction,
        Success_Rate_Bonus,
        Innovation_Rate
    }

    public enum AcceleratorType
    {
        Experience_Boost,
        Learning_Speed,
        Skill_Focus,
        Research_Boost,
        Collaboration_Bonus,
        Innovation_Catalyst,
        Time_Acceleration
    }

    public enum AcceleratorRequirementType
    {
        Money_Investment,
        Skill_Level,
        Reputation_Level,
        Research_Completion,
        Achievement_Unlock,
        Player_Level,
        Time_Investment
    }

    public enum CollaborationType
    {
        Skill_Sharing,
        Joint_Research,
        Mentorship,
        Team_Project,
        Knowledge_Exchange,
        Innovation_Lab,
        Study_Group,
        Expert_Consultation
    }

    public enum CollaborationBenefitType
    {
        Experience_Boost,
        Skill_Transfer,
        Knowledge_Gain,
        Innovation_Spark,
        Network_Building,
        Reputation_Gain,
        Resource_Sharing,
        Efficiency_Gain
    }

    public enum PathRewardType
    {
        Experience_Points,
        Skill_Points,
        Reputation,
        Money,
        Equipment_Unlock,
        Research_Unlock,
        Feature_Access,
        Specialization_Option
    }

    #endregion
}