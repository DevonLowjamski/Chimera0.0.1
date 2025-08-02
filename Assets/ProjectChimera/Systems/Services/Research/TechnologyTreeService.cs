using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Research;
using ProjectChimera.Systems.Registry;
using Technology = ProjectChimera.Data.Research.Technology;
using TechnologyTree = ProjectChimera.Data.Research.TechnologyTree;
using TechnologyNode = ProjectChimera.Data.Research.TechnologyNode;
using TechnologyType = ProjectChimera.Data.Research.TechnologyType;
using UnlockedTechnology = ProjectChimera.Data.Research.UnlockedTechnology;
using TechnologyUnlock = ProjectChimera.Data.Research.TechnologyUnlock;
using UnlockRequirements = ProjectChimera.Data.Research.UnlockRequirements;
using TechnologyPathAnalysis = ProjectChimera.Data.Research.TechnologyPathAnalysis;
using NodeStatus = ProjectChimera.Data.Research.NodeStatus;

namespace ProjectChimera.Systems.Services.Research
{
    /// <summary>
    /// PC014-2b: Technology Tree Service
    /// Technology dependency management and unlock progression
    /// Decomposed from ResearchManager (400 lines target)
    /// </summary>
    public class TechnologyTreeService : MonoBehaviour, ITechnologyTreeService
    {
        #region Properties
        
        public bool IsInitialized { get; private set; }
        
        #endregion

        #region Private Fields
        
        [Header("Technology Configuration")]
        [SerializeField] private bool _enableTechnologyProgression = true;
        [SerializeField] private bool _requireDependencies = true;
        [SerializeField] private float _unlockAnimationDuration = 2f;
        
        [Header("Technology Data")]
        [SerializeField] private List<TechnologyTree> _technologyTrees = new List<TechnologyTree>();
        [SerializeField] private Dictionary<TechnologyType, List<UnlockedTechnology>> _unlockedTechnologies = new Dictionary<TechnologyType, List<UnlockedTechnology>>();
        [SerializeField] private Dictionary<string, Technology> _allTechnologies = new Dictionary<string, Technology>();
        [SerializeField] private List<string> _availableTechnologies = new List<string>();
        
        private Dictionary<string, List<string>> _technologyDependencies = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> _dependentTechnologies = new Dictionary<string, List<string>>();
        private IResearchResourceService _resourceService;
        
        #endregion

        #region Events
        
        public event Action<string> OnTechnologyUnlocked;
        public event Action<List<string>> OnPathUpdated;
        public event Action<string> OnDependencyMet;
        public event Action<TechnologyType> OnTreeCompleted;
        
        #endregion

        #region IService Implementation
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            Debug.Log("Initializing TechnologyTreeService...");
            
            // Get service dependencies
            _resourceService = ServiceRegistry.Instance.GetService<IResearchResourceService>();
            
            // Initialize technology system
            InitializeTechnologySystem();
            
            // Build dependency graph
            BuildDependencyGraph();
            
            // Load existing unlocks
            LoadExistingUnlocks();
            
            // Update available technologies
            UpdateAvailableTechnologies();
            
            // Register with ServiceRegistry
            ServiceRegistry.Instance.RegisterService<ITechnologyTreeService>(this, ServiceDomain.Research);
            
            IsInitialized = true;
            Debug.Log("TechnologyTreeService initialized successfully");
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            Debug.Log("Shutting down TechnologyTreeService...");
            
            // Save technology state
            SaveTechnologyState();
            
            // Clear collections
            _technologyTrees.Clear();
            _unlockedTechnologies.Clear();
            _allTechnologies.Clear();
            _availableTechnologies.Clear();
            _technologyDependencies.Clear();
            _dependentTechnologies.Clear();
            
            _resourceService = null;
            
            IsInitialized = false;
            Debug.Log("TechnologyTreeService shutdown complete");
        }
        
        #endregion

        #region Tree Navigation
        
        public List<Technology> GetAvailableTechnologies()
        {
            return _availableTechnologies
                .Where(techId => _allTechnologies.ContainsKey(techId))
                .Select(techId => _allTechnologies[techId])
                .ToList();
        }

        public List<Technology> GetUnlockedTechnologies()
        {
            var unlockedTechs = new List<Technology>();
            
            foreach (var techList in _unlockedTechnologies.Values)
            {
                foreach (var unlockedTech in techList)
                {
                    if (_allTechnologies.ContainsKey(unlockedTech.TechnologyUnlock.TechnologyId))
                    {
                        unlockedTechs.Add(_allTechnologies[unlockedTech.TechnologyUnlock.TechnologyId]);
                    }
                }
            }
            
            return unlockedTechs;
        }

        public List<Technology> GetDependencies(string technologyId)
        {
            if (!_technologyDependencies.ContainsKey(technologyId))
                return new List<Technology>();

            return _technologyDependencies[technologyId]
                .Where(depId => _allTechnologies.ContainsKey(depId))
                .Select(depId => _allTechnologies[depId])
                .ToList();
        }

        public List<Technology> GetDependents(string technologyId)
        {
            if (!_dependentTechnologies.ContainsKey(technologyId))
                return new List<Technology>();

            return _dependentTechnologies[technologyId]
                .Where(depId => _allTechnologies.ContainsKey(depId))
                .Select(depId => _allTechnologies[depId])
                .ToList();
        }
        
        #endregion

        #region Unlock System
        
        public bool UnlockTechnology(string technologyId)
        {
            if (!IsInitialized)
            {
                Debug.LogError("TechnologyTreeService not initialized");
                return false;
            }

            if (!_allTechnologies.ContainsKey(technologyId))
            {
                Debug.LogError($"Technology not found: {technologyId}");
                return false;
            }

            if (IsTechnologyUnlocked(technologyId))
            {
                Debug.LogWarning($"Technology already unlocked: {technologyId}");
                return true;
            }

            if (!CanUnlockTechnology(technologyId))
            {
                Debug.LogWarning($"Technology cannot be unlocked yet: {technologyId}");
                return false;
            }

            var technology = _allTechnologies[technologyId];
            technology.IsUnlocked = true;
            technology.UnlockDate = DateTime.Now;

            // Create unlock record
            var technologyUnlock = new TechnologyUnlock
            {
                TechnologyId = technologyId,
                TechnologyName = technology.Name,
                Type = technology.Type,
                UnlockDescription = $"Unlocked technology: {technology.Name}"
            };

            var unlockedTechnology = new UnlockedTechnology
            {
                TechnologyUnlock = technologyUnlock,
                UnlockDate = DateTime.Now,
                UnlockSource = "Research",
                ImpactScore = CalculateImpactScore(technology),
                IsActive = true
            };

            // Add to unlocked technologies
            if (!_unlockedTechnologies.ContainsKey(technology.Type))
            {
                _unlockedTechnologies[technology.Type] = new List<UnlockedTechnology>();
            }
            
            _unlockedTechnologies[technology.Type].Add(unlockedTechnology);

            // Update technology tree nodes
            UpdateTechnologyTreeNodes(technologyId);
            
            // Update available technologies
            UpdateAvailableTechnologies();

            OnTechnologyUnlocked?.Invoke(technologyId);
            Debug.Log($"Unlocked technology: {technology.Name}");
            
            return true;
        }

        public bool IsTechnologyUnlocked(string technologyId)
        {
            if (!_allTechnologies.ContainsKey(technologyId))
                return false;

            return _allTechnologies[technologyId].IsUnlocked;
        }

        public bool CanUnlockTechnology(string technologyId)
        {
            if (!_allTechnologies.ContainsKey(technologyId))
                return false;

            if (IsTechnologyUnlocked(technologyId))
                return true;

            var technology = _allTechnologies[technologyId];
            
            // Check dependencies
            if (_requireDependencies && technology.Dependencies != null)
            {
                foreach (var dependency in technology.Dependencies)
                {
                    if (!IsTechnologyUnlocked(dependency))
                        return false;
                }
            }

            // Check resource requirements
            if (_resourceService != null && technology.Requirements != null)
            {
                // Validate resource requirements through resource service
                // For now, assume resources are available
            }

            return true;
        }

        public UnlockRequirements GetUnlockRequirements(string technologyId)
        {
            if (!_allTechnologies.ContainsKey(technologyId))
                return null;

            return _allTechnologies[technologyId].Requirements;
        }
        
        #endregion

        #region Path Optimization
        
        public List<string> FindOptimalPath(string targetTechnologyId)
        {
            if (!_allTechnologies.ContainsKey(targetTechnologyId))
                return new List<string>();

            if (IsTechnologyUnlocked(targetTechnologyId))
                return new List<string> { targetTechnologyId };

            var path = new List<string>();
            var visited = new HashSet<string>();
            
            if (FindPathRecursive(targetTechnologyId, path, visited))
            {
                path.Reverse(); // Reverse to get correct order
                OnPathUpdated?.Invoke(path);
                return path;
            }

            return new List<string>();
        }

        public TechnologyPathAnalysis AnalyzePath(string targetTechnologyId)
        {
            var path = FindOptimalPath(targetTechnologyId);
            
            var analysis = new TechnologyPathAnalysis
            {
                TargetTechnologyId = targetTechnologyId,
                OptimalPath = path,
                EstimatedTime = CalculatePathTime(path),
                TotalCost = CalculatePathCost(path),
                SuccessProbability = CalculatePathSuccessProbability(path),
                Alternatives = FindAlternativePaths(targetTechnologyId)
            };

            return analysis;
        }

        public List<Technology> GetRecommendedTechnologies()
        {
            var available = GetAvailableTechnologies();
            
            // Sort by priority and impact
            return available
                .OrderByDescending(tech => CalculateRecommendationScore(tech))
                .Take(5)
                .ToList();
        }
        
        #endregion

        #region Private Helper Methods
        
        private void InitializeTechnologySystem()
        {
            // Initialize technology trees with default data
            CreateDefaultTechnologyTrees();
            
            // Populate all technologies dictionary
            PopulateAllTechnologies();
            
            Debug.Log("Technology system initialized");
        }

        private void CreateDefaultTechnologyTrees()
        {
            // Create Genetics Technology Tree
            var geneticsTree = new TechnologyTree
            {
                TreeId = "genetics_tree",
                Name = "Genetics Research",
                Type = TechnologyType.Genetic,
                Nodes = new List<TechnologyNode>(),
                Dependencies = new Dictionary<string, List<string>>(),
                CompletionPercentage = 0f
            };

            // Add basic genetic technologies
            AddTechnologyNode(geneticsTree, "basic_genetics", "Basic Genetics", Vector2.zero);
            AddTechnologyNode(geneticsTree, "advanced_breeding", "Advanced Breeding", new Vector2(100, 0));
            AddTechnologyNode(geneticsTree, "genetic_markers", "Genetic Markers", new Vector2(200, 0));

            _technologyTrees.Add(geneticsTree);

            // Create Cultivation Technology Tree
            var cultivationTree = new TechnologyTree
            {
                TreeId = "cultivation_tree",
                Name = "Cultivation Techniques",
                Type = TechnologyType.Cultivation,
                Nodes = new List<TechnologyNode>(),
                Dependencies = new Dictionary<string, List<string>>(),
                CompletionPercentage = 0f
            };

            AddTechnologyNode(cultivationTree, "hydroponics", "Hydroponics", Vector2.zero);
            AddTechnologyNode(cultivationTree, "led_lighting", "LED Lighting", new Vector2(100, 0));
            AddTechnologyNode(cultivationTree, "climate_control", "Climate Control", new Vector2(200, 0));

            _technologyTrees.Add(cultivationTree);
        }

        private void AddTechnologyNode(TechnologyTree tree, string techId, string techName, Vector2 position)
        {
            var technology = new Technology
            {
                TechnologyId = techId,
                Name = techName,
                Type = tree.Type,
                IsUnlocked = false,
                Dependencies = new List<string>()
            };

            var node = new TechnologyNode
            {
                NodeId = techId,
                Technology = technology,
                Position = position,
                Status = NodeStatus.Locked,
                ConnectedNodes = new List<string>()
            };

            tree.Nodes.Add(node);
            _allTechnologies[techId] = technology;
        }

        private void PopulateAllTechnologies()
        {
            foreach (var tree in _technologyTrees)
            {
                foreach (var node in tree.Nodes)
                {
                    if (!_allTechnologies.ContainsKey(node.Technology.TechnologyId))
                    {
                        _allTechnologies[node.Technology.TechnologyId] = node.Technology;
                    }
                }
            }
        }

        private void BuildDependencyGraph()
        {
            _technologyDependencies.Clear();
            _dependentTechnologies.Clear();

            foreach (var tech in _allTechnologies.Values)
            {
                if (tech.Dependencies != null)
                {
                    _technologyDependencies[tech.TechnologyId] = new List<string>(tech.Dependencies);

                    // Build reverse dependencies
                    foreach (var dependency in tech.Dependencies)
                    {
                        if (!_dependentTechnologies.ContainsKey(dependency))
                        {
                            _dependentTechnologies[dependency] = new List<string>();
                        }
                        _dependentTechnologies[dependency].Add(tech.TechnologyId);
                    }
                }
            }
        }

        private void LoadExistingUnlocks()
        {
            // TODO: Load from persistent storage
            Debug.Log("Loading existing technology unlocks...");
        }

        private void SaveTechnologyState()
        {
            // TODO: Save to persistent storage
            Debug.Log("Saving technology state...");
        }

        private void UpdateAvailableTechnologies()
        {
            _availableTechnologies.Clear();

            foreach (var tech in _allTechnologies.Values)
            {
                if (!tech.IsUnlocked && CanUnlockTechnology(tech.TechnologyId))
                {
                    _availableTechnologies.Add(tech.TechnologyId);
                }
            }

            Debug.Log($"Updated available technologies: {_availableTechnologies.Count} available");
        }

        private void UpdateTechnologyTreeNodes(string technologyId)
        {
            foreach (var tree in _technologyTrees)
            {
                var node = tree.Nodes.FirstOrDefault(n => n.Technology.TechnologyId == technologyId);
                if (node != null)
                {
                    node.Status = NodeStatus.Unlocked;
                    
                    // Update tree completion percentage
                    UpdateTreeCompletion(tree);
                    break;
                }
            }
        }

        private void UpdateTreeCompletion(TechnologyTree tree)
        {
            int totalNodes = tree.Nodes.Count;
            int unlockedNodes = tree.Nodes.Count(n => n.Status == NodeStatus.Unlocked);
            
            tree.CompletionPercentage = totalNodes > 0 ? (float)unlockedNodes / totalNodes : 0f;

            if (tree.CompletionPercentage >= 1.0f)
            {
                OnTreeCompleted?.Invoke(tree.Type);
            }
        }

        private float CalculateImpactScore(Technology technology)
        {
            // Calculate impact based on technology type and tier
            float baseScore = 50f;
            
            baseScore += technology.Type switch
            {
                TechnologyType.Genetic => 30f,
                TechnologyType.Cultivation => 25f,
                TechnologyType.Processing => 20f,
                _ => 10f
            };

            return baseScore;
        }

        private bool FindPathRecursive(string targetId, List<string> path, HashSet<string> visited)
        {
            if (visited.Contains(targetId))
                return false;

            visited.Add(targetId);

            if (IsTechnologyUnlocked(targetId))
            {
                path.Add(targetId);
                return true;
            }

            var dependencies = GetDependencies(targetId);
            if (!dependencies.Any())
            {
                path.Add(targetId);
                return true;
            }

            foreach (var dependency in dependencies)
            {
                if (FindPathRecursive(dependency.TechnologyId, path, visited))
                {
                    path.Add(targetId);
                    return true;
                }
            }

            return false;
        }

        private float CalculatePathTime(List<string> path)
        {
            // Estimate time based on path length
            return path.Count * 7f; // 7 days per technology
        }

        private ResourceCost CalculatePathCost(List<string> path)
        {
            return new ResourceCost
            {
                MonetaryCost = path.Count * 1000f,
                TimeCost = CalculatePathTime(path),
                ExpertiseRequired = path.Count * 10f
            };
        }

        private float CalculatePathSuccessProbability(List<string> path)
        {
            // Base probability decreases with path length
            return Mathf.Max(0.5f, 0.9f - (path.Count * 0.05f));
        }

        private List<string> FindAlternativePaths(string targetTechnologyId)
        {
            // For now, return empty list
            // TODO: Implement alternative path finding
            return new List<string>();
        }

        private float CalculateRecommendationScore(Technology technology)
        {
            float score = 50f;
            
            // Boost score for certain types
            score += technology.Type switch
            {
                TechnologyType.Genetic => 20f,
                TechnologyType.Cultivation => 15f,
                _ => 5f
            };

            // Reduce score if many dependencies
            if (technology.Dependencies != null)
            {
                score -= technology.Dependencies.Count * 5f;
            }

            return score;
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