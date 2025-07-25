using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Core.Events;
using ProjectChimera.Data.Events;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Core.Logging;
using ProjectChimera.Data.Progression; // For ExperienceSource enum
using ProjectChimera.Data.Construction; // For SkillLevel enum
// Type alias to resolve ExperienceSource ambiguity
using ProgressionExperienceSource = ProjectChimera.Data.Progression.ExperienceSource;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Tree Skill Progression System - 3D Tree-based skill progression for cultivation mastery
    /// Integrates with automation systems and provides visual skill tree representation
    /// Core component of Enhanced Cultivation Gaming System v2.0
    /// </summary>
    public class TreeSkillProgressionSystem : MonoBehaviour
    {
        [Header("Skill Tree Configuration")]
        [SerializeField] private SkillNodeLibrarySO _nodeLibrary;
        [SerializeField] private SkillTreeConfigSO _treeConfig;
        [SerializeField] private SkillTreeVisualizationController _visualizationController;
        
        [Header("Progression Settings")]
        [Range(0.1f, 5.0f)] public float BaseExperienceGain = 1.0f;
        [Range(1.0f, 10.0f)] public float SkillTreeGrowthRate = 3.0f;
        [Range(0.1f, 2.0f)] public float AutomationSkillMultiplier = 1.5f;
        [Range(0.5f, 3.0f)] public float ManualSkillMultiplier = 2.0f;
        
        [Header("Tree Visualization")]
        [Range(0.1f, 2.0f)] public float BranchGrowthSpeed = 1.0f;
        [Range(0.5f, 5.0f)] public float NodeUnlockAnimationDuration = 2.0f;
        [Range(0.1f, 1.0f)] public float TreeVibrancyThreshold = 0.7f;
        
        // System State
        private bool _isInitialized = false;
        private Dictionary<SkillBranch, BranchProgressionState> _branchStates = new Dictionary<SkillBranch, BranchProgressionState>();
        private Dictionary<string, SkillNodeState> _nodeStates = new Dictionary<string, SkillNodeState>();
        private SkillNodeUnlockManager _unlockManager;
        private AutomationUnlockManager _automationUnlockManager;
        
        // Tree Growth Tracking
        private TreeGrowthLevel _currentTreeGrowth = TreeGrowthLevel.Seed;
        private float _overallTreeVibrancy = 0f;
        private float _totalSkillExperience = 0f;
        private int _unlockedNodeCount = 0;
        
        // Events
        private GameEventChannelSO _onSkillNodeUnlocked;
        private GameEventChannelSO _onBranchProgressed;
        private GameEventChannelSO _onTreeGrowthLevelChanged;
        private GameEventChannelSO _onAutomationUnlocked;
        
        #region Initialization
        
        public void Initialize(SkillNodeLibrarySO nodeLibrary, SkillTreeConfigSO treeConfig)
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("TreeSkillProgressionSystem already initialized", this);
                return;
            }
            
            _nodeLibrary = nodeLibrary ?? _nodeLibrary;
            _treeConfig = treeConfig ?? _treeConfig;
            
            if (_nodeLibrary == null)
            {
                ChimeraLogger.LogError("SkillNodeLibrarySO is required for initialization", this);
                return;
            }
            
            InitializeBranchStates();
            InitializeNodeStates();
            InitializeUnlockManagers();
            SetupVisualization();
            SetupEventChannels();
            
            _isInitialized = true;
            ChimeraLogger.Log("TreeSkillProgressionSystem initialized successfully", this);
        }
        
        private void InitializeBranchStates()
        {
            foreach (SkillBranch branch in System.Enum.GetValues(typeof(SkillBranch)))
            {
                var definition = GetBranchDefinition(branch);
                _branchStates[branch] = new BranchProgressionState
                {
                    Branch = branch,
                    Definition = definition,
                    UnlockedNodes = 0,
                    TotalNodes = definition.MaxNodes,
                    BranchVibrancy = 0f,
                    BranchGrowthLevel = TreeGrowthLevel.Seed,
                    LastProgressTime = 0f,
                    IsRootBranch = IsRootBranch(branch)
                };
            }
        }
        
        private void InitializeNodeStates()
        {
            if (_nodeLibrary == null)
            {
                ChimeraLogger.LogWarning("SkillNodeLibrarySO not assigned, creating default nodes", this);
                CreateDefaultNodes();
                return;
            }
            
            foreach (var node in _nodeLibrary.AllNodes)
            {
                _nodeStates[node.NodeId] = new SkillNodeState
                {
                    NodeId = node.NodeId,
                    NodeType = node.NodeType,
                    Branch = node.Branch,
                    IsUnlocked = false,
                    IsConceptIntroduced = false,
                    IsEquipmentUnlocked = false,
                    CurrentExperience = 0f,
                    RequiredExperience = node.RequiredExperience,
                    Prerequisites = node.Prerequisites?.ToList() ?? new List<string>(),
                    UnlockTimestamp = 0f
                };
            }
        }
        
        private void InitializeUnlockManagers()
        {
            _unlockManager = GetComponentInChildren<SkillNodeUnlockManager>();
            if (_unlockManager == null)
            {
                var unlockGO = new GameObject("SkillNodeUnlockManager");
                unlockGO.transform.SetParent(transform);
                _unlockManager = unlockGO.AddComponent<SkillNodeUnlockManager>();
            }
            
            _automationUnlockManager = GetComponentInChildren<AutomationUnlockManager>();
            if (_automationUnlockManager == null)
            {
                var automationGO = new GameObject("AutomationUnlockManager");
                automationGO.transform.SetParent(transform);
                _automationUnlockManager = automationGO.AddComponent<AutomationUnlockManager>();
            }
        }
        
        private void SetupVisualization()
        {
            if (_visualizationController == null)
            {
                _visualizationController = GetComponentInChildren<SkillTreeVisualizationController>();
                if (_visualizationController == null)
                {
                    var visualGO = new GameObject("SkillTreeVisualization");
                    visualGO.transform.SetParent(transform);
                    _visualizationController = visualGO.AddComponent<SkillTreeVisualizationController>();
                }
            }
            
            _visualizationController.Initialize(_branchStates, _nodeStates);
        }
        
        private void SetupEventChannels()
        {
            // Event channels will be connected through the main cultivation gaming manager
        }
        
        #endregion
        
        #region Skill Progression
        
        /// <summary>
        /// Add experience to a specific skill node
        /// </summary>
        public bool AddSkillExperience(string nodeId, float experience, ProgressionExperienceSource source)
        {
            if (!_isInitialized || !_nodeStates.ContainsKey(nodeId))
                return false;
            
            var nodeState = _nodeStates[nodeId];
            
            // Apply source multiplier
            var multipliedExperience = experience * GetSourceMultiplier(source);
            
            // Add experience
            nodeState.CurrentExperience += multipliedExperience;
            _totalSkillExperience += multipliedExperience;
            
            // Check for node unlock
            if (!nodeState.IsUnlocked && nodeState.CurrentExperience >= nodeState.RequiredExperience)
            {
                if (ArePrerequisitesMet(nodeState))
                {
                    UnlockSkillNode(nodeId);
                }
            }
            
            // Update branch progression
            UpdateBranchProgression(nodeState.Branch);
            
            // Update tree growth
            UpdateTreeGrowth();
            
            return true;
        }
        
        private void UnlockSkillNode(string nodeId)
        {
            var nodeState = _nodeStates[nodeId];
            nodeState.IsUnlocked = true;
            nodeState.UnlockTimestamp = Time.time;
            _unlockedNodeCount++;
            
            // Process unlock with manager
            _unlockManager.ProcessNodeUnlock(nodeId, nodeState);
            
            // Check for automation unlocks
            CheckAutomationUnlocks(nodeState);
            
            // Update visualization
            _visualizationController.AnimateNodeUnlock(nodeId);
            
            // Raise event
            RaiseSkillNodeUnlockedEvent(nodeId, nodeState);
            
            ChimeraLogger.Log($"Skill node unlocked: {nodeId}", this);
        }
        
        private void CheckAutomationUnlocks(SkillNodeState nodeState)
        {
            if (nodeState.NodeType == SkillNodeType.AutomationUnlock)
            {
                _automationUnlockManager.ProcessAutomationUnlock(nodeState);
            }
        }
        
        #endregion
        
        #region Tree Growth Management
        
        private void UpdateTreeGrowth()
        {
            var previousGrowthLevel = _currentTreeGrowth;
            
            // Calculate overall progress
            var totalNodes = _nodeStates.Count;
            var progressPercentage = totalNodes > 0 ? (float)_unlockedNodeCount / totalNodes : 0f;
            
            // Determine growth level
            _currentTreeGrowth = progressPercentage switch
            {
                >= 0.9f => TreeGrowthLevel.FullyFlowered,
                >= 0.7f => TreeGrowthLevel.Flowering,
                >= 0.5f => TreeGrowthLevel.Mature,
                >= 0.3f => TreeGrowthLevel.Vegetative,
                >= 0.1f => TreeGrowthLevel.Seedling,
                _ => TreeGrowthLevel.Seed
            };
            
            // Update tree vibrancy
            _overallTreeVibrancy = CalculateTreeVibrancy();
            
            // Update visualization
            _visualizationController.UpdateTreeGrowth(_currentTreeGrowth, _overallTreeVibrancy);
            
            // Raise event if growth level changed
            if (_currentTreeGrowth != previousGrowthLevel)
            {
                RaiseTreeGrowthLevelChangedEvent(previousGrowthLevel, _currentTreeGrowth);
            }
        }
        
        private float CalculateTreeVibrancy()
        {
            if (_branchStates.Count == 0) return 0f;
            
            var totalVibrancy = 0f;
            foreach (var branchState in _branchStates.Values)
            {
                totalVibrancy += branchState.BranchVibrancy;
            }
            
            return totalVibrancy / _branchStates.Count;
        }
        
        #endregion
        
        #region Utility Methods
        
        private float GetSourceMultiplier(ProgressionExperienceSource source)
        {
            return source switch
            {
                ProgressionExperienceSource.PlantCare => ManualSkillMultiplier,
                ProgressionExperienceSource.Skill_Usage => AutomationSkillMultiplier,
                ProgressionExperienceSource.Research => 2.0f,
                ProgressionExperienceSource.Achievement => 1.5f,
                _ => 1.0f
            };
        }
        
        private bool ArePrerequisitesMet(SkillNodeState nodeState)
        {
            if (nodeState.Prerequisites == null || nodeState.Prerequisites.Count == 0)
                return true;
            
            return nodeState.Prerequisites.All(prereqId => 
                _nodeStates.ContainsKey(prereqId) && _nodeStates[prereqId].IsUnlocked);
        }
        
        private void UpdateBranchProgression(SkillBranch branch)
        {
            if (!_branchStates.ContainsKey(branch)) return;
            
            var branchState = _branchStates[branch];
            var branchNodes = _nodeStates.Values.Where(n => n.Branch == branch);
            var unlockedBranchNodes = branchNodes.Count(n => n.IsUnlocked);
            
            branchState.UnlockedNodes = unlockedBranchNodes;
            branchState.BranchVibrancy = branchNodes.Any() ? 
                (float)unlockedBranchNodes / branchNodes.Count() : 0f;
            branchState.LastProgressTime = Time.time;
            
            // Update branch growth level
            var previousBranchGrowth = branchState.BranchGrowthLevel;
            branchState.BranchGrowthLevel = CalculateBranchGrowthLevel(branchState.BranchVibrancy);
            
            // Update visualization
            _visualizationController.UpdateBranchGrowth(branch, branchState);
            
            // Raise event if branch progressed
            if (branchState.BranchGrowthLevel != previousBranchGrowth)
            {
                RaiseBranchProgressedEvent(branch, branchState);
            }
        }
        
        private TreeGrowthLevel CalculateBranchGrowthLevel(float vibrancy)
        {
            return vibrancy switch
            {
                >= 0.9f => TreeGrowthLevel.FullyFlowered,
                >= 0.7f => TreeGrowthLevel.Flowering,
                >= 0.5f => TreeGrowthLevel.Mature,
                >= 0.3f => TreeGrowthLevel.Vegetative,
                >= 0.1f => TreeGrowthLevel.Seedling,
                _ => TreeGrowthLevel.Seed
            };
        }
        
        private BranchDefinition GetBranchDefinition(SkillBranch branch)
        {
            return new BranchDefinition
            {
                Branch = branch,
                MaxNodes = 10,
                GrowthRate = 3.0f,
                RequiredVibrancy = 0.7f
            };
        }
        
        private bool IsRootBranch(SkillBranch branch)
        {
            return branch == SkillBranch.Cultivation;
        }
        
        private void CreateDefaultNodes()
        {
            var defaultNodes = new[]
            {
                new SkillNodeState
                {
                    NodeId = "basic_watering",
                    NodeType = SkillNodeType.BasicSkill,
                    Branch = SkillBranch.Cultivation,
                    RequiredExperience = 100f,
                    Prerequisites = new List<string>()
                }
            };
            
            foreach (var node in defaultNodes)
            {
                _nodeStates[node.NodeId] = node;
            }
        }
        
        #endregion
        
        #region Event Management
        
        private void RaiseSkillNodeUnlockedEvent(string nodeId, SkillNodeState nodeState)
        {
            var eventData = new SkillNodeUnlockedEventData
            {
                NodeId = nodeId,
                NodeState = nodeState,
                Timestamp = Time.time
            };
            
            _onSkillNodeUnlocked?.RaiseEvent(eventData);
        }
        
        private void RaiseBranchProgressedEvent(SkillBranch branch, BranchProgressionState branchState)
        {
            var eventData = new BranchProgressedEventData
            {
                Branch = branch,
                BranchState = branchState,
                Timestamp = Time.time
            };
            
            _onBranchProgressed?.RaiseEvent(eventData);
        }
        
        private void RaiseTreeGrowthLevelChangedEvent(TreeGrowthLevel previousLevel, TreeGrowthLevel newLevel)
        {
            var eventData = new TreeGrowthLevelChangedEventData
            {
                PreviousLevel = previousLevel,
                NewLevel = newLevel,
                OverallVibrancy = _overallTreeVibrancy,
                Timestamp = Time.time
            };
            
            _onTreeGrowthLevelChanged?.RaiseEvent(eventData);
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Get current skill node state
        /// </summary>
        public SkillNodeState GetNodeState(string nodeId)
        {
            return _nodeStates.ContainsKey(nodeId) ? _nodeStates[nodeId] : null;
        }
        
        /// <summary>
        /// Get current branch progression state
        /// </summary>
        public BranchProgressionState GetBranchState(SkillBranch branch)
        {
            return _branchStates.ContainsKey(branch) ? _branchStates[branch] : null;
        }
        
        /// <summary>
        /// Get overall tree progression metrics
        /// </summary>
        public TreeProgressionMetrics GetTreeMetrics()
        {
            return new TreeProgressionMetrics
            {
                CurrentGrowthLevel = _currentTreeGrowth,
                OverallVibrancy = _overallTreeVibrancy,
                TotalExperience = _totalSkillExperience,
                UnlockedNodes = _unlockedNodeCount,
                TotalNodes = _nodeStates.Count,
                BranchStates = new Dictionary<SkillBranch, BranchProgressionState>(_branchStates)
            };
        }
        
        /// <summary>
        /// Check if a specific skill node is unlocked
        /// </summary>
        public bool IsNodeUnlocked(string nodeId)
        {
            return _nodeStates.ContainsKey(nodeId) && _nodeStates[nodeId].IsUnlocked;
        }
        
        /// <summary>
        /// Progress a skill node by type with skill points
        /// </summary>
        public bool ProgressNode(SkillNodeType nodeType, int skillPoints)
        {
            // Find nodes of the specified type
            var matchingNodes = _nodeStates.Values
                .Where(n => n.NodeType == nodeType && !n.IsUnlocked)
                .OrderBy(n => n.RequiredExperience)
                .FirstOrDefault();
            
            if (matchingNodes != null)
            {
                return AddSkillExperience(matchingNodes.NodeId, skillPoints, ProgressionExperienceSource.Skill_Usage);
            }
            
            return false;
        }
        
        /// <summary>
        /// Get all unlocked nodes for a specific branch
        /// </summary>
        public List<SkillNodeState> GetUnlockedNodesForBranch(SkillBranch branch)
        {
            return _nodeStates.Values
                .Where(n => n.Branch == branch && n.IsUnlocked)
                .ToList();
        }
        
        /// <summary>
        /// Get overall skill level based on unlocked nodes and progression
        /// </summary>
        public SkillLevel GetOverallSkillLevel()
        {
            if (!_isInitialized || _nodeStates.Count == 0)
                return SkillLevel.Beginner;
            
            var unlockedNodes = _nodeStates.Values.Count(n => n.IsUnlocked);
            var totalNodes = _nodeStates.Count;
            var progressionRatio = (float)unlockedNodes / totalNodes;
            
            return progressionRatio switch
            {
                >= 0.8f => SkillLevel.Expert,
                >= 0.6f => SkillLevel.Advanced,
                >= 0.4f => SkillLevel.Intermediate,
                >= 0.2f => SkillLevel.Novice,
                _ => SkillLevel.Beginner
            };
        }
        
        /// <summary>
        /// Update system - called by EnhancedCultivationGamingManager
        /// </summary>
        public void UpdateSystem(float deltaTime)
        {
            if (!_isInitialized) return;
            
            // Update any time-based progression
            UpdateTreeGrowth();
        }
        
        #endregion
    }
} 