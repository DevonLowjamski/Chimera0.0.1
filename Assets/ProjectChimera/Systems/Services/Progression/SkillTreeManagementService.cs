using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Progression;
using ProjectChimera.Systems.Registry;

namespace ProjectChimera.Systems.Services.Progression
{
    /// <summary>
    /// PC014-3b: Skill Tree Management Service
    /// Skill point allocation and ability unlock management
    /// Decomposed from ComprehensiveProgressionManager (400 lines target)
    /// </summary>
    public class SkillTreeManagementService : MonoBehaviour, ISkillTreeManagementService
    {
        #region Properties
        
        public bool IsInitialized { get; private set; }
        
        #endregion

        #region Private Fields
        
        [Header("Skill Configuration")]
        [SerializeField] private bool _enableSkillSystem = true;
        [SerializeField] private int _baseSkillPointsPerLevel = 1;
        [SerializeField] private int _maxSkillLevel = 10;
        [SerializeField] private float _skillPointMultiplier = 1.0f;
        
        [Header("Skill Trees")]
        [SerializeField] private List<SkillTreeNode> _allSkills = new List<SkillTreeNode>();
        [SerializeField] private Dictionary<string, List<string>> _skillDependencies = new Dictionary<string, List<string>>();
        [SerializeField] private Dictionary<string, List<string>> _skillCategories = new Dictionary<string, List<string>>();
        
        [Header("Player Data")]
        [SerializeField] private Dictionary<string, int> _playerSkillPoints = new Dictionary<string, int>();
        [SerializeField] private Dictionary<string, List<string>> _playerUnlockedSkills = new Dictionary<string, List<string>>();
        [SerializeField] private Dictionary<string, Dictionary<string, int>> _playerSkillLevels = new Dictionary<string, Dictionary<string, int>>();
        
        [Header("Skill Statistics")]
        [SerializeField] private int _totalSkillPointsAwarded = 0;
        [SerializeField] private int _totalSkillsUnlocked = 0;
        [SerializeField] private Dictionary<string, int> _skillUnlockCounts = new Dictionary<string, int>();
        
        #endregion

        #region Events
        
        public event Action<string, string> OnSkillUnlocked; // playerId, skillId
        public event Action<string, int> OnSkillPointsAwarded; // playerId, points
        public event Action<string, string, int> OnSkillLevelUp; // playerId, skillId, newLevel
        public event Action<string> OnSkillTreeCompleted; // playerId
        
        #endregion

        #region IService Implementation
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            Debug.Log("Initializing SkillTreeManagementService...");
            
            // Initialize skill system
            InitializeSkillSystem();
            
            // Initialize skill trees
            InitializeSkillTrees();
            
            // Initialize skill dependencies
            InitializeSkillDependencies();
            
            // Load existing player data
            LoadExistingSkillData();
            
            // Register with ServiceRegistry
            ServiceRegistry.Instance.RegisterService<ISkillTreeManagementService>(this, ServiceDomain.Progression);
            
            IsInitialized = true;
            Debug.Log("SkillTreeManagementService initialized successfully");
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            Debug.Log("Shutting down SkillTreeManagementService...");
            
            // Save skill state
            SaveSkillState();
            
            // Clear collections
            _allSkills.Clear();
            _skillDependencies.Clear();
            _skillCategories.Clear();
            _playerSkillPoints.Clear();
            _playerUnlockedSkills.Clear();
            _playerSkillLevels.Clear();
            _skillUnlockCounts.Clear();
            
            IsInitialized = false;
            Debug.Log("SkillTreeManagementService shutdown complete");
        }
        
        #endregion

        #region Skill Points Management
        
        public void AwardSkillPoints(string playerId, int points)
        {
            if (!_enableSkillSystem || !IsInitialized)
                return;

            if (points <= 0)
            {
                Debug.LogWarning($"Cannot award non-positive skill points: {points}");
                return;
            }

            if (!_playerSkillPoints.ContainsKey(playerId))
            {
                _playerSkillPoints[playerId] = 0;
            }

            _playerSkillPoints[playerId] += points;
            _totalSkillPointsAwarded += points;

            OnSkillPointsAwarded?.Invoke(playerId, points);
            Debug.Log($"Awarded {points} skill points to {playerId} (Total: {_playerSkillPoints[playerId]})");
        }

        public int GetAvailableSkillPoints(string playerId)
        {
            return _playerSkillPoints.GetValueOrDefault(playerId, 0);
        }

        public int GetTotalSkillPoints(string playerId)
        {
            if (!_playerSkillLevels.ContainsKey(playerId))
                return 0;

            int totalSpent = _playerSkillLevels[playerId].Values.Sum();
            int available = GetAvailableSkillPoints(playerId);
            return totalSpent + available;
        }

        public bool SpendSkillPoints(string playerId, string skillId, int points)
        {
            if (!_enableSkillSystem || !IsInitialized)
                return false;

            if (points <= 0)
            {
                Debug.LogError("Cannot spend non-positive skill points");
                return false;
            }

            int availablePoints = GetAvailableSkillPoints(playerId);
            if (availablePoints < points)
            {
                Debug.LogError($"Insufficient skill points. Available: {availablePoints}, Required: {points}");
                return false;
            }

            if (!CanUpgradeSkill(playerId, skillId))
            {
                Debug.LogError($"Cannot upgrade skill {skillId} for player {playerId}");
                return false;
            }

            // Spend points
            _playerSkillPoints[playerId] -= points;

            // Upgrade skill
            if (!_playerSkillLevels.ContainsKey(playerId))
            {
                _playerSkillLevels[playerId] = new Dictionary<string, int>();
            }

            if (!_playerSkillLevels[playerId].ContainsKey(skillId))
            {
                _playerSkillLevels[playerId][skillId] = 0;
            }

            _playerSkillLevels[playerId][skillId] += points;

            // Check if skill should be unlocked
            if (!IsSkillUnlocked(playerId, skillId))
            {
                UnlockSkillInternal(playerId, skillId);
            }

            OnSkillLevelUp?.Invoke(playerId, skillId, _playerSkillLevels[playerId][skillId]);
            Debug.Log($"Player {playerId} upgraded {skillId} to level {_playerSkillLevels[playerId][skillId]}");

            return true;
        }
        
        #endregion

        #region Skill Management
        
        public bool UnlockSkill(string playerId, string skillId)
        {
            if (!_enableSkillSystem || !IsInitialized)
                return false;

            if (!CanUnlockSkill(playerId, skillId))
            {
                Debug.LogError($"Cannot unlock skill {skillId} for player {playerId}");
                return false;
            }

            return UnlockSkillInternal(playerId, skillId);
        }

        public bool IsSkillUnlocked(string playerId, string skillId)
        {
            if (!_playerUnlockedSkills.ContainsKey(playerId))
                return false;

            return _playerUnlockedSkills[playerId].Contains(skillId);
        }

        public int GetSkillLevel(string playerId, string skillId)
        {
            if (!_playerSkillLevels.ContainsKey(playerId))
                return 0;

            return _playerSkillLevels[playerId].GetValueOrDefault(skillId, 0);
        }

        public bool CanUnlockSkill(string playerId, string skillId)
        {
            // Check if skill exists
            var skill = _allSkills.FirstOrDefault(s => s.SkillId == skillId);
            if (skill == null)
            {
                Debug.LogError($"Skill not found: {skillId}");
                return false;
            }

            // Check if already unlocked
            if (IsSkillUnlocked(playerId, skillId))
                return false;

            // Check prerequisites
            if (_skillDependencies.ContainsKey(skillId))
            {
                foreach (string prerequisite in _skillDependencies[skillId])
                {
                    if (!IsSkillUnlocked(playerId, prerequisite))
                    {
                        Debug.LogWarning($"Missing prerequisite {prerequisite} for skill {skillId}");
                        return false;
                    }
                }
            }

            // Check if player has required skill points
            int availablePoints = GetAvailableSkillPoints(playerId);
            if (availablePoints < skill.RequiredSkillPoints)
            {
                Debug.LogWarning($"Insufficient skill points for {skillId}. Required: {skill.RequiredSkillPoints}, Available: {availablePoints}");
                return false;
            }

            return true;
        }
        
        #endregion

        #region Tree Navigation
        
        public List<Skill> GetAvailableSkills(string playerId)
        {
            var availableSkills = new List<Skill>();

            foreach (var skillNode in _allSkills)
            {
                if (CanUnlockSkill(playerId, skillNode.SkillId))
                {
                    availableSkills.Add(new Skill
                    {
                        SkillId = skillNode.SkillId,
                        SkillName = skillNode.SkillName,
                        Description = skillNode.Description,
                        Level = GetSkillLevel(playerId, skillNode.SkillId),
                        MaxLevel = skillNode.MaxLevel,
                        IsUnlocked = false
                    });
                }
            }

            return availableSkills;
        }

        public List<Skill> GetUnlockedSkills(string playerId)
        {
            var unlockedSkills = new List<Skill>();

            if (!_playerUnlockedSkills.ContainsKey(playerId))
                return unlockedSkills;

            foreach (string skillId in _playerUnlockedSkills[playerId])
            {
                var skillNode = _allSkills.FirstOrDefault(s => s.SkillId == skillId);
                if (skillNode != null)
                {
                    unlockedSkills.Add(new Skill
                    {
                        SkillId = skillNode.SkillId,
                        SkillName = skillNode.SkillName,
                        Description = skillNode.Description,
                        Level = GetSkillLevel(playerId, skillNode.SkillId),
                        MaxLevel = skillNode.MaxLevel,
                        IsUnlocked = true
                    });
                }
            }

            return unlockedSkills;
        }

        public List<Skill> GetSkillDependencies(string skillId)
        {
            var dependencies = new List<Skill>();

            if (_skillDependencies.ContainsKey(skillId))
            {
                foreach (string dependencyId in _skillDependencies[skillId])
                {
                    var skillNode = _allSkills.FirstOrDefault(s => s.SkillId == dependencyId);
                    if (skillNode != null)
                    {
                        dependencies.Add(new Skill
                        {
                            SkillId = skillNode.SkillId,
                            SkillName = skillNode.SkillName,
                            Description = skillNode.Description,
                            Level = skillNode.CurrentLevel,
                            MaxLevel = skillNode.MaxLevel,
                            IsUnlocked = skillNode.IsUnlocked
                        });
                    }
                }
            }

            return dependencies;
        }

        public SkillPath FindOptimalSkillPath(string playerId, string targetSkillId)
        {
            var path = new SkillPath
            {
                PathId = $"path_to_{targetSkillId}",
                PathName = $"Path to {GetSkillName(targetSkillId)}"
            };

            // Use BFS to find optimal path
            var visited = new HashSet<string>();
            var queue = new Queue<(string skillId, List<string> pathSoFar)>();
            queue.Enqueue((targetSkillId, new List<string> { targetSkillId }));

            while (queue.Count > 0)
            {
                var (currentSkill, currentPath) = queue.Dequeue();

                if (visited.Contains(currentSkill))
                    continue;

                visited.Add(currentSkill);

                // If player already has this skill unlocked, we can start from here
                if (IsSkillUnlocked(playerId, currentSkill))
                {
                    path.SkillSequence = currentPath.Skip(currentPath.IndexOf(currentSkill) + 1).ToList();
                    break;
                }

                // Add prerequisites to queue
                if (_skillDependencies.ContainsKey(currentSkill))
                {
                    foreach (string prerequisite in _skillDependencies[currentSkill])
                    {
                        var newPath = new List<string> { prerequisite };
                        newPath.AddRange(currentPath);
                        queue.Enqueue((prerequisite, newPath));
                    }
                }
            }

            // Calculate path metrics
            path.TotalSkillPointsCost = CalculatePathCost(path.SkillSequence);
            path.EstimatedCompletionTime = CalculateEstimatedTime(path.SkillSequence);
            path.Difficulty = DeterminePathDifficulty(path.SkillSequence);
            path.Benefits = GetPathBenefits(path.SkillSequence);

            return path;
        }
        
        #endregion

        #region Private Helper Methods
        
        private void InitializeSkillSystem()
        {
            if (_playerSkillPoints == null)
                _playerSkillPoints = new Dictionary<string, int>();
            
            if (_playerUnlockedSkills == null)
                _playerUnlockedSkills = new Dictionary<string, List<string>>();
            
            if (_playerSkillLevels == null)
                _playerSkillLevels = new Dictionary<string, Dictionary<string, int>>();
            
            Debug.Log("Skill system initialized");
        }

        private void InitializeSkillTrees()
        {
            // Create default skill trees if empty
            if (_allSkills.Count == 0)
            {
                CreateDefaultSkills();
            }

            // Initialize categories
            InitializeSkillCategories();

            Debug.Log($"Initialized {_allSkills.Count} skills in skill tree");
        }

        private void CreateDefaultSkills()
        {
            // Cultivation Skills
            _allSkills.Add(new SkillTreeNode
            {
                SkillId = "cultivation_basics",
                SkillName = "Cultivation Basics",
                Description = "Fundamental cannabis cultivation knowledge",
                Category = SkillCategory.Cultivation,
                MaxLevel = 5,
                RequiredSkillPoints = 1,
                Prerequisites = new List<string>()
            });

            _allSkills.Add(new SkillTreeNode
            {
                SkillId = "advanced_cultivation",
                SkillName = "Advanced Cultivation",
                Description = "Expert cultivation techniques",
                Category = SkillCategory.Cultivation,
                MaxLevel = 10,
                RequiredSkillPoints = 3,
                Prerequisites = new List<string> { "cultivation_basics" }
            });

            // Genetics Skills
            _allSkills.Add(new SkillTreeNode
            {
                SkillId = "genetics_basics",
                SkillName = "Genetics Basics",
                Description = "Basic genetic understanding",
                Category = SkillCategory.Genetics,
                MaxLevel = 5,
                RequiredSkillPoints = 2,
                Prerequisites = new List<string>()
            });

            // Business Skills
            _allSkills.Add(new SkillTreeNode
            {
                SkillId = "business_management",
                SkillName = "Business Management",
                Description = "Business and financial management",
                Category = SkillCategory.Business,
                MaxLevel = 8,
                RequiredSkillPoints = 2,
                Prerequisites = new List<string>()
            });
        }

        private void InitializeSkillDependencies()
        {
            _skillDependencies.Clear();

            foreach (var skill in _allSkills)
            {
                if (skill.Prerequisites.Count > 0)
                {
                    _skillDependencies[skill.SkillId] = new List<string>(skill.Prerequisites);
                }
            }

            Debug.Log($"Initialized dependencies for {_skillDependencies.Count} skills");
        }

        private void InitializeSkillCategories()
        {
            _skillCategories.Clear();

            foreach (var skill in _allSkills)
            {
                string categoryName = skill.Category.ToString();
                if (!_skillCategories.ContainsKey(categoryName))
                {
                    _skillCategories[categoryName] = new List<string>();
                }
                _skillCategories[categoryName].Add(skill.SkillId);
            }

            Debug.Log($"Initialized {_skillCategories.Count} skill categories");
        }

        private void LoadExistingSkillData()
        {
            // TODO: Load from persistent storage
            Debug.Log("Loading existing skill data...");
        }

        private void SaveSkillState()
        {
            // TODO: Save to persistent storage
            Debug.Log("Saving skill state...");
        }

        private bool UnlockSkillInternal(string playerId, string skillId)
        {
            if (!_playerUnlockedSkills.ContainsKey(playerId))
            {
                _playerUnlockedSkills[playerId] = new List<string>();
            }

            if (_playerUnlockedSkills[playerId].Contains(skillId))
                return false;

            _playerUnlockedSkills[playerId].Add(skillId);
            _totalSkillsUnlocked++;

            if (!_skillUnlockCounts.ContainsKey(skillId))
            {
                _skillUnlockCounts[skillId] = 0;
            }
            _skillUnlockCounts[skillId]++;

            OnSkillUnlocked?.Invoke(playerId, skillId);
            Debug.Log($"Player {playerId} unlocked skill: {GetSkillName(skillId)}");

            return true;
        }

        private bool CanUpgradeSkill(string playerId, string skillId)
        {
            var skill = _allSkills.FirstOrDefault(s => s.SkillId == skillId);
            if (skill == null)
                return false;

            int currentLevel = GetSkillLevel(playerId, skillId);
            return currentLevel < skill.MaxLevel;
        }

        private string GetSkillName(string skillId)
        {
            var skill = _allSkills.FirstOrDefault(s => s.SkillId == skillId);
            return skill?.SkillName ?? skillId;
        }

        private int CalculatePathCost(List<string> skillSequence)
        {
            int totalCost = 0;
            foreach (string skillId in skillSequence)
            {
                var skill = _allSkills.FirstOrDefault(s => s.SkillId == skillId);
                if (skill != null)
                {
                    totalCost += skill.RequiredSkillPoints;
                }
            }
            return totalCost;
        }

        private float CalculateEstimatedTime(List<string> skillSequence)
        {
            // Estimate based on typical progression rate
            return skillSequence.Count * 2.5f; // 2.5 hours per skill on average
        }

        private PathDifficulty DeterminePathDifficulty(List<string> skillSequence)
        {
            if (skillSequence.Count <= 2) return PathDifficulty.Beginner;
            if (skillSequence.Count <= 4) return PathDifficulty.Intermediate;
            if (skillSequence.Count <= 6) return PathDifficulty.Advanced;
            if (skillSequence.Count <= 8) return PathDifficulty.Expert;
            return PathDifficulty.Master;
        }

        private List<string> GetPathBenefits(List<string> skillSequence)
        {
            var benefits = new List<string>();
            foreach (string skillId in skillSequence)
            {
                var skill = _allSkills.FirstOrDefault(s => s.SkillId == skillId);
                if (skill != null)
                {
                    benefits.Add($"Unlock {skill.SkillName}");
                }
            }
            return benefits;
        }
        
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Shutdown();
        }
        
        #endregion
    }
}