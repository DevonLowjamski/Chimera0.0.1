using UnityEngine;
using System.Collections.Generic;

namespace ProjectChimera.Data.Progression
{
    /// <summary>
    /// ScriptableObject configuration for content unlocking system
    /// </summary>
    [CreateAssetMenu(fileName = "UnlockConfig", menuName = "Project Chimera/Progression/Unlock Config")]
    public class UnlockConfigSO : ScriptableObject
    {
        [Header("Unlock Rules")]
        [SerializeField] private List<UnlockRule> _unlockRules = new List<UnlockRule>();
        
        [Header("Content Categories")]
        [SerializeField] private List<UnlockCategory> _categories = new List<UnlockCategory>();
        
        [Header("Configuration")]
        [SerializeField] private bool _enableProgressiveUnlocking = true;
        [SerializeField] private bool _showUnlockNotifications = true;
        [SerializeField] private float _unlockNotificationDuration = 3f;
        
        public List<UnlockRule> UnlockRules => _unlockRules;
        public List<UnlockCategory> Categories => _categories;
        public bool EnableProgressiveUnlocking => _enableProgressiveUnlocking;
        public bool ShowUnlockNotifications => _showUnlockNotifications;
        public float UnlockNotificationDuration => _unlockNotificationDuration;
        
        /// <summary>
        /// Get unlock rule for specific content
        /// </summary>
        public UnlockRule GetUnlockRule(string contentId)
        {
            return _unlockRules.Find(r => r.ContentId == contentId);
        }
        
        /// <summary>
        /// Get all unlock rules for a category
        /// </summary>
        public List<UnlockRule> GetUnlockRulesForCategory(UnlockContentType category)
        {
            return _unlockRules.FindAll(r => r.ContentType == category);
        }
        
        /// <summary>
        /// Get all unlockable content as UnlockableContent objects
        /// </summary>
        public List<UnlockableContent> GetAllUnlockables()
        {
            var unlockables = new List<UnlockableContent>();
            
            foreach (var rule in _unlockRules)
            {
                unlockables.Add(new UnlockableContent
                {
                    ContentId = rule.ContentId,
                    ContentName = rule.ContentName,
                    ContentType = ConvertToUnlockableContentType(rule.ContentType),
                    Requirements = rule.Requirements,
                    Description = rule.UnlockDescription
                });
            }
            
            return unlockables;
        }
        
        /// <summary>
        /// Convert UnlockContentType to UnlockableContentType
        /// </summary>
        private UnlockableContentType ConvertToUnlockableContentType(UnlockContentType contentType)
        {
            switch (contentType)
            {
                case UnlockContentType.Skill:
                    return UnlockableContentType.Skill_Node;
                case UnlockContentType.Research:
                    return UnlockableContentType.Research_Project;
                case UnlockContentType.Equipment:
                    return UnlockableContentType.Equipment;
                case UnlockContentType.Facility:
                    return UnlockableContentType.Facility_Type;
                case UnlockContentType.Feature:
                    return UnlockableContentType.Feature;
                case UnlockContentType.Achievement:
                    return UnlockableContentType.Achievement;
                case UnlockContentType.Customization:
                    return UnlockableContentType.Cosmetic;
                default:
                    return UnlockableContentType.Feature;
            }
        }
        
        /// <summary>
        /// Check if content should be unlocked based on progression
        /// </summary>
        public bool ShouldUnlockContent(string contentId, PlayerProgressionData progression)
        {
            var rule = GetUnlockRule(contentId);
            if (rule == null) return false;
            
            // Check all requirements
            foreach (var requirement in rule.Requirements)
            {
                if (!IsRequirementMet(requirement, progression))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private bool IsRequirementMet(UnlockRequirement requirement, PlayerProgressionData progression)
        {
            switch (requirement.RequirementType)
            {
                case UnlockRequirementType.PlayerLevel:
                    return progression.PlayerLevel >= requirement.RequiredValue;
                    
                case UnlockRequirementType.SkillLevel:
                    return progression.GetSkillLevel(requirement.TargetId) >= requirement.RequiredValue;
                    
                case UnlockRequirementType.AchievementCompleted:
                    return progression.HasCompletedAchievement(requirement.TargetId);
                    
                case UnlockRequirementType.ResearchCompleted:
                    return progression.HasCompletedResearch(requirement.TargetId);
                    
                case UnlockRequirementType.ContentUnlocked:
                    return progression.HasUnlockedContent(requirement.TargetId);
                    
                default:
                    return false;
            }
        }
    }
    
    /// <summary>
    /// Rule for unlocking specific content
    /// </summary>
    [System.Serializable]
    public class UnlockRule
    {
        public string ContentId;
        public string ContentName;
        public UnlockContentType ContentType;
        public List<UnlockRequirement> Requirements = new List<UnlockRequirement>();
        [TextArea(2, 3)] public string UnlockDescription;
        public Sprite ContentIcon;
        public bool IsHidden = false;
    }
    
    /// <summary>
    /// Requirement for unlocking content
    /// </summary>
    [System.Serializable]
    public class UnlockRequirement
    {
        public UnlockRequirementType RequirementType;
        public string TargetId;
        public float RequiredValue;
        public string Description;
    }
    
    /// <summary>
    /// Category of unlockable content
    /// </summary>
    [System.Serializable]
    public class UnlockCategory
    {
        public UnlockContentType ContentType;
        public string CategoryName;
        public string CategoryDescription;
        public Sprite CategoryIcon;
        public Color CategoryColor = Color.white;
    }
    
    /// <summary>
    /// Types of content that can be unlocked
    /// </summary>
    public enum UnlockContentType
    {
        Skill,
        Research,
        Equipment,
        Facility,
        Feature,
        Achievement,
        Tutorial,
        Challenge,
        Customization,
        Advanced_Feature
    }
    
    /// <summary>
    /// Types of requirements for unlocking
    /// </summary>
    public enum UnlockRequirementType
    {
        PlayerLevel,
        SkillLevel,
        AchievementCompleted,
        ResearchCompleted,
        ContentUnlocked,
        TimeInGame,
        StatThreshold,
        CampaignProgress
    }
} 