using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Progression;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Data.Progression
{
    /// <summary>
    /// ScriptableObject that manages the unlock system configuration and rules
    /// </summary>
    [CreateAssetMenu(fileName = "UnlockSystem", menuName = "Project Chimera/Progression/Unlock System")]
    public class UnlockSystemSO : ScriptableObject
    {
        [Header("Unlock System Configuration")]
        [SerializeField] private List<UnlockRule> _unlockRules = new List<UnlockRule>();
        [SerializeField] private List<UnlockCategory> _unlockCategories = new List<UnlockCategory>();
        
        [Header("System Settings")]
        [SerializeField] private bool _enableProgressiveUnlocks = true;
        [SerializeField] private bool _enableSkillBasedUnlocks = true;
        [SerializeField] private bool _enableAchievementUnlocks = true;
        [SerializeField] private bool _enableTimeBasedUnlocks = false;
        
        [Header("Unlock Notifications")]
        [SerializeField] private bool _showUnlockNotifications = true;
        [SerializeField] private float _notificationDuration = 3f;
        [SerializeField] private AudioClip _unlockSound;
        
        /// <summary>
        /// All unlock rules in the system
        /// </summary>
        public List<UnlockRule> UnlockRules => _unlockRules;
        
        /// <summary>
        /// All unlock categories
        /// </summary>
        public List<UnlockCategory> UnlockCategories => _unlockCategories;
        
        /// <summary>
        /// System configuration properties
        /// </summary>
        public bool EnableProgressiveUnlocks => _enableProgressiveUnlocks;
        public bool EnableSkillBasedUnlocks => _enableSkillBasedUnlocks;
        public bool EnableAchievementUnlocks => _enableAchievementUnlocks;
        public bool EnableTimeBasedUnlocks => _enableTimeBasedUnlocks;
        public bool ShowUnlockNotifications => _showUnlockNotifications;
        public float NotificationDuration => _notificationDuration;
        public AudioClip UnlockSound => _unlockSound;
        
        /// <summary>
        /// Get all unlock rules for a specific content type
        /// </summary>
        public List<UnlockRule> GetUnlockRulesForContentType(UnlockContentType contentType)
        {
            return _unlockRules.Where(rule => rule.ContentType == contentType).ToList();
        }
        
        /// <summary>
        /// Get unlock rule by content ID
        /// </summary>
        public UnlockRule GetUnlockRule(string contentId)
        {
            return _unlockRules.FirstOrDefault(rule => rule.ContentId == contentId);
        }
        
        /// <summary>
        /// Get all unlockable content
        /// </summary>
        public List<UnlockableContent> GetAllUnlockables()
        {
            return _unlockRules.Select(rule => new UnlockableContent
            {
                ContentId = rule.ContentId,
                ContentName = rule.ContentName,
                ContentType = ConvertToUnlockableContentType(rule.ContentType),
                Description = rule.UnlockDescription,
                Requirements = rule.Requirements.Select(req => new UnlockRequirement
                {
                    RequirementType = req.RequirementType,
                    RequiredValue = req.RequiredValue,
                    TargetId = req.TargetId
                }).ToList()
            }).ToList();
        }
        
        /// <summary>
        /// Get default unlocks (content that should be unlocked from the start)
        /// </summary>
        public List<string> GetDefaultUnlocks()
        {
            var defaultUnlocks = new List<string>();
            
            foreach (var rule in _unlockRules)
            {
                // Content with no requirements should be unlocked from start
                if (rule.Requirements == null || rule.Requirements.Count == 0)
                {
                    defaultUnlocks.Add(rule.ContentId);
                }
            }
            
            return defaultUnlocks;
        }
        
        /// <summary>
        /// Convert UnlockContentType to UnlockableContentType for compatibility
        /// </summary>
        private UnlockableContentType ConvertToUnlockableContentType(UnlockContentType contentType)
        {
            return contentType switch
            {
                UnlockContentType.Skill => UnlockableContentType.Skill_Node,
                UnlockContentType.Research => UnlockableContentType.Research_Project,
                UnlockContentType.Equipment => UnlockableContentType.Equipment,
                UnlockContentType.Facility => UnlockableContentType.Facility_Type,
                UnlockContentType.Feature => UnlockableContentType.Feature,
                UnlockContentType.Achievement => UnlockableContentType.Achievement,
                UnlockContentType.Customization => UnlockableContentType.Cosmetic,
                _ => UnlockableContentType.Feature
            };
        }
        
        /// <summary>
        /// Get unlock category by type
        /// </summary>
        public UnlockCategory GetUnlockCategory(UnlockContentType contentType)
        {
            return _unlockCategories.FirstOrDefault(cat => cat.ContentType == contentType);
        }
        
        /// <summary>
        /// Validate unlock system configuration
        /// </summary>
        public bool ValidateUnlockSystem()
        {
            // Check for duplicate content IDs
            var duplicateIds = _unlockRules.GroupBy(rule => rule.ContentId)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);
            
            if (duplicateIds.Any())
            {
                Debug.LogError($"Duplicate unlock rule IDs found: {string.Join(", ", duplicateIds)}");
                return false;
            }
            
            // Check for invalid requirements
            foreach (var rule in _unlockRules)
            {
                foreach (var requirement in rule.Requirements)
                {
                    if (string.IsNullOrEmpty(requirement.TargetId) && 
                        requirement.RequirementType != UnlockRequirementType.PlayerLevel &&
                        requirement.RequirementType != UnlockRequirementType.TimeInGame)
                    {
                        Debug.LogError($"Unlock rule {rule.ContentId} has requirement with missing TargetId");
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Get unlock system statistics
        /// </summary>
        public UnlockSystemStats GetUnlockSystemStats()
        {
            return new UnlockSystemStats
            {
                TotalUnlockRules = _unlockRules.Count,
                UnlocksByType = System.Enum.GetValues(typeof(UnlockContentType))
                    .Cast<UnlockContentType>()
                    .ToDictionary(type => type, type => _unlockRules.Count(rule => rule.ContentType == type)),
                AverageRequirementsPerUnlock = _unlockRules.Count > 0 ? 
                    (float)_unlockRules.Average(rule => rule.Requirements.Count) : 0f,
                SystemConfigured = ValidateUnlockSystem()
            };
        }
    }
    
    /// <summary>
    /// Statistics for the unlock system
    /// </summary>
    [System.Serializable]
    public class UnlockSystemStats
    {
        public int TotalUnlockRules;
        public Dictionary<UnlockContentType, int> UnlocksByType = new Dictionary<UnlockContentType, int>();
        public float AverageRequirementsPerUnlock;
        public bool SystemConfigured;
    }
    
    // UnlockCategory is defined in UnlockConfigSO.cs to avoid duplicate definitions
} 