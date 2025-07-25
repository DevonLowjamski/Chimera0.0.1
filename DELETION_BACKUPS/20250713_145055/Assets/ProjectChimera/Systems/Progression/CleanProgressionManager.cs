using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Progression;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Clean, minimal progression manager that eliminates circular dependencies
    /// Uses only shared types from ProjectChimera.Data.Progression namespace
    /// </summary>
    public class CleanProgressionManager : ChimeraManager
    {
        [Header("Clean Progression Configuration")]
        [SerializeField] private bool _enableAchievements = true;
        [SerializeField] private bool _enableExperience = true;
        [SerializeField] private bool _enableSkillTrees = true;
        [SerializeField] private bool _enableCampaigns = true;
        
        // Clean data collections
        private List<CleanProgressionAchievement> _achievements = new List<CleanProgressionAchievement>();
        private List<CleanProgressionExperience> _experienceEntries = new List<CleanProgressionExperience>();
        private List<CleanProgressionMilestone> _milestones = new List<CleanProgressionMilestone>();
        private List<CleanProgressionSkillNode> _skillNodes = new List<CleanProgressionSkillNode>();
        private List<CleanProgressionLeaderboard> _leaderboards = new List<CleanProgressionLeaderboard>();
        private List<CleanProgressionReward> _rewards = new List<CleanProgressionReward>();
        private List<CleanProgressionCampaign> _campaigns = new List<CleanProgressionCampaign>();
        
        // Simple state tracking
        private bool _isInitialized = false;
        private float _totalExperience = 0f;
        private int _totalAchievements = 0;
        private int _unlockedSkills = 0;
        private int _completedCampaigns = 0;
        
        #region Manager Lifecycle
        
        public override string ManagerName => "Clean Progression Manager";
        
        protected override void OnManagerInitialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("CleanProgressionManager already initialized");
                return;
            }
            
            InitializeBasicSystems();
            _isInitialized = true;
            
            Debug.Log("CleanProgressionManager initialized successfully");
        }
        
        protected override void OnManagerShutdown()
        {
            _isInitialized = false;
            _achievements.Clear();
            _experienceEntries.Clear();
            _milestones.Clear();
            _skillNodes.Clear();
            _leaderboards.Clear();
            _rewards.Clear();
            _campaigns.Clear();
            
            Debug.Log("CleanProgressionManager shutdown successfully");
        }
        
        private void InitializeBasicSystems()
        {
            // Initialize with minimal data
            if (_enableAchievements)
            {
                CreateSampleAchievements();
            }
            
            if (_enableSkillTrees)
            {
                CreateSampleSkillNodes();
            }
            
            if (_enableCampaigns)
            {
                CreateSampleCampaign();
            }
        }
        
        #endregion
        
        #region Public API
        
        public bool IsSystemReady => _isInitialized;
        
        public float GetTotalExperience() => _totalExperience;
        public int GetTotalAchievements() => _totalAchievements;
        public int GetUnlockedSkills() => _unlockedSkills;
        public int GetCompletedCampaigns() => _completedCampaigns;
        
        public List<CleanProgressionAchievement> GetAchievements() => new List<CleanProgressionAchievement>(_achievements);
        public List<CleanProgressionSkillNode> GetSkillNodes() => new List<CleanProgressionSkillNode>(_skillNodes);
        public List<CleanProgressionCampaign> GetCampaigns() => new List<CleanProgressionCampaign>(_campaigns);
        
        public int GetCurrentPlayerLevel()
        {
            // Simple level calculation based on total experience
            if (_totalExperience < 100) return 1;
            if (_totalExperience < 500) return 2;
            if (_totalExperience < 1000) return 3;
            if (_totalExperience < 2000) return 4;
            return 5; // Max level for now
        }
        
        #endregion
        
        #region Achievement System
        
        public bool UnlockAchievement(string achievementID)
        {
            if (!_isInitialized || !_enableAchievements)
                return false;
            
            var achievement = _achievements.Find(a => a.AchievementID == achievementID);
            if (achievement != null && !achievement.IsUnlocked)
            {
                achievement.IsUnlocked = true;
                achievement.UnlockedDate = System.DateTime.Now;
                _totalAchievements++;
                
                Debug.Log($"Achievement unlocked: {achievement.AchievementName}");
                return true;
            }
            
            return false;
        }
        
        public void UpdateAchievementProgress(string achievementID, float progress)
        {
            if (!_isInitialized || !_enableAchievements)
                return;
            
            var achievement = _achievements.Find(a => a.AchievementID == achievementID);
            if (achievement != null && !achievement.IsUnlocked)
            {
                achievement.Progress = Mathf.Clamp(progress, 0f, achievement.RequiredProgress);
                
                if (achievement.Progress >= achievement.RequiredProgress)
                {
                    UnlockAchievement(achievementID);
                }
            }
        }
        
        private void CreateSampleAchievements()
        {
            var achievement1 = new CleanProgressionAchievement
            {
                AchievementID = System.Guid.NewGuid().ToString(),
                AchievementName = "First Steps",
                Description = "Complete your first cultivation cycle",
                Category = "Genetics",
                Points = 10,
                RequiredProgress = 1f
            };
            
            var achievement2 = new CleanProgressionAchievement
            {
                AchievementID = System.Guid.NewGuid().ToString(),
                AchievementName = "Master Breeder",
                Description = "Successfully breed 10 different strains",
                Category = "Genetics",
                Points = 50,
                RequiredProgress = 10f
            };
            
            _achievements.Add(achievement1);
            _achievements.Add(achievement2);
        }
        
        #endregion
        
        #region Experience System
        
        public void AddExperience(string sourceType, float amount, ProgressionCategory category = ProgressionCategory.General)
        {
            if (!_isInitialized || !_enableExperience)
                return;
            
            var experience = new CleanProgressionExperience
            {
                SourceID = System.Guid.NewGuid().ToString(),
                SourceType = sourceType,
                ExperienceGained = amount,
                Category = category.ToString()
            };
            
            _totalExperience += amount;
            experience.TotalExperience = _totalExperience;
            
            _experienceEntries.Add(experience);
            
            Debug.Log($"Experience gained: {amount} from {sourceType} (Total: {_totalExperience})");
        }
        
        #endregion
        
        #region Skill System
        
        public bool UnlockSkill(string skillNodeID)
        {
            if (!_isInitialized || !_enableSkillTrees)
                return false;
            
            var skillNode = _skillNodes.Find(s => s.NodeID == skillNodeID);
            if (skillNode != null && !skillNode.IsUnlocked)
            {
                skillNode.IsUnlocked = true;
                skillNode.CurrentLevel = 1;
                _unlockedSkills++;
                
                Debug.Log($"Skill unlocked: {skillNode.NodeName}");
                return true;
            }
            
            return false;
        }
        
        private void CreateSampleSkillNodes()
        {
            var skill1 = new CleanProgressionSkillNode
            {
                NodeID = System.Guid.NewGuid().ToString(),
                NodeName = "Basic Breeding",
                Description = "Learn the fundamentals of cannabis breeding",
                SkillTree = "Genetics",
                MaxLevel = 3
            };
            
            var skill2 = new CleanProgressionSkillNode
            {
                NodeID = System.Guid.NewGuid().ToString(),
                NodeName = "Advanced Genetics",
                Description = "Master complex genetic interactions",
                SkillTree = "Genetics",
                MaxLevel = 5
            };
            skill2.Prerequisites.Add(skill1.NodeID);
            
            _skillNodes.Add(skill1);
            _skillNodes.Add(skill2);
        }
        
        #endregion
        
        #region Campaign System
        
        public void StartCampaign(string campaignID)
        {
            if (!_isInitialized || !_enableCampaigns)
                return;
            
            var campaign = _campaigns.Find(c => c.CampaignID == campaignID);
            if (campaign != null && !campaign.IsActive)
            {
                campaign.IsActive = true;
                campaign.StartDate = System.DateTime.Now;
                
                Debug.Log($"Campaign started: {campaign.CampaignName}");
            }
        }
        
        private void CreateSampleCampaign()
        {
            var campaign = new CleanProgressionCampaign
            {
                CampaignID = System.Guid.NewGuid().ToString(),
                CampaignName = "Beginner's Journey",
                Description = "Learn the basics of cannabis cultivation"
            };
            
            campaign.AvailableObjectives.Add("Complete first breeding");
            campaign.AvailableObjectives.Add("Harvest first plant");
            campaign.AvailableObjectives.Add("Unlock basic skills");
            
            _campaigns.Add(campaign);
        }
        
        #endregion
        
        #region Debug and Testing
        
        [ContextMenu("Test Achievement")]
        private void TestAchievement()
        {
            if (_achievements.Count > 0)
            {
                var achievement = _achievements[0];
                UpdateAchievementProgress(achievement.AchievementID, achievement.RequiredProgress);
            }
        }
        
        [ContextMenu("Test Experience")]
        private void TestExperience()
        {
            AddExperience("Test Action", 25f, ProgressionCategory.Genetics);
        }
        
        [ContextMenu("Test Skill")]
        private void TestSkill()
        {
            if (_skillNodes.Count > 0)
            {
                UnlockSkill(_skillNodes[0].NodeID);
            }
        }
        
        [ContextMenu("Print Status")]
        private void PrintStatus()
        {
            Debug.Log($"Progression Manager Status: Initialized={_isInitialized}, " +
                     $"Experience={_totalExperience}, Achievements={_totalAchievements}, " +
                     $"Skills={_unlockedSkills}, Campaigns={_completedCampaigns}");
        }
        
        #endregion
    }
}