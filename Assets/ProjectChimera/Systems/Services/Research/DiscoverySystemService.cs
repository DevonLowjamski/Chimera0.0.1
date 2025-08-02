using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Research;
using ProjectChimera.Systems.Registry;
using Discovery = ProjectChimera.Data.Research.Discovery;
using DiscoveryEvent = ProjectChimera.Data.Research.DiscoveryEvent;
using Innovation = ProjectChimera.Data.Research.Innovation;
using Breakthrough = ProjectChimera.Data.Research.Breakthrough;
using PotentialDiscovery = ProjectChimera.Data.Research.PotentialDiscovery;
using DiscoveryContext = ProjectChimera.Data.Research.DiscoveryContext;
using DiscoveryType = ProjectChimera.Data.Research.DiscoveryType;
using InnovationType = ProjectChimera.Data.Research.InnovationType;
using InnovationTrigger = ProjectChimera.Data.Research.InnovationTrigger;
using BreakthroughConditions = ProjectChimera.Data.Research.BreakthroughConditions;

namespace ProjectChimera.Systems.Services.Research
{
    /// <summary>
    /// PC014-2c: Discovery System Service
    /// New technology discovery and innovation events
    /// Decomposed from ResearchManager (300 lines target)
    /// </summary>
    public class DiscoverySystemService : MonoBehaviour, IDiscoverySystemService
    {
        #region Properties
        
        public bool IsInitialized { get; private set; }
        
        #endregion

        #region Private Fields
        
        [Header("Discovery Configuration")]
        [SerializeField] private bool _enableDiscoveries = true;
        [SerializeField] private bool _enableRandomDiscoveries = true;
        [SerializeField] private float _discoveryCheckInterval = 300f; // 5 minutes
        [Range(0f, 1f)] [SerializeField] private float _baseDiscoveryChance = 0.01f;
        
        [Header("Discovery Data")]
        [SerializeField] private List<Discovery> _recentDiscoveries = new List<Discovery>();
        [SerializeField] private List<Innovation> _playerInnovations = new List<Innovation>();
        [SerializeField] private List<Breakthrough> _breakthroughs = new List<Breakthrough>();
        [SerializeField] private List<PotentialDiscovery> _potentialDiscoveries = new List<PotentialDiscovery>();
        
        private float _timeSinceLastDiscoveryCheck;
        private IResearchProjectService _projectService;
        private ITechnologyTreeService _technologyService;
        private System.Random _discoveryRandom;
        
        #endregion

        #region Events
        
        public event Action<Discovery> OnDiscoveryMade;
        public event Action<Innovation> OnInnovationAchieved;
        public event Action<Breakthrough> OnBreakthroughOccurred;
        public event Action<PotentialDiscovery> OnPotentialDiscoveryIdentified;
        
        #endregion

        #region IService Implementation
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            Debug.Log("Initializing DiscoverySystemService...");
            
            // Get service dependencies
            _projectService = ServiceRegistry.Instance.GetService<IResearchProjectService>();
            _technologyService = ServiceRegistry.Instance.GetService<ITechnologyTreeService>();
            
            // Initialize discovery system
            InitializeDiscoverySystem();
            
            // Generate initial potential discoveries
            GenerateInitialPotentialDiscoveries();
            
            // Register with ServiceRegistry
            ServiceRegistry.Instance.RegisterService<IDiscoverySystemService>(this, ServiceDomain.Research);
            
            IsInitialized = true;
            Debug.Log("DiscoverySystemService initialized successfully");
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            Debug.Log("Shutting down DiscoverySystemService...");
            
            // Save discovery state
            SaveDiscoveryState();
            
            // Clear collections
            _recentDiscoveries.Clear();
            _playerInnovations.Clear();
            _breakthroughs.Clear();
            _potentialDiscoveries.Clear();
            
            _projectService = null;
            _technologyService = null;
            
            IsInitialized = false;
            Debug.Log("DiscoverySystemService shutdown complete");
        }
        
        #endregion

        #region Discovery Mechanics
        
        public Discovery TriggerDiscovery(DiscoveryContext context)
        {
            if (!IsInitialized)
            {
                Debug.LogError("DiscoverySystemService not initialized");
                return null;
            }

            if (!_enableDiscoveries)
            {
                Debug.LogWarning("Discoveries are currently disabled");
                return null;
            }

            // Find potential discovery matching context
            var potentialDiscovery = FindMatchingPotentialDiscovery(context);
            if (potentialDiscovery == null)
            {
                potentialDiscovery = GenerateDynamicDiscovery(context);
            }

            if (potentialDiscovery == null)
            {
                return null;
            }

            // Check discovery probability
            if (!CheckDiscoveryProbability(potentialDiscovery, context))
            {
                return null;
            }

            // Create actual discovery
            var discovery = CreateDiscovery(potentialDiscovery, context);
            
            // Process discovery
            ProcessDiscovery(discovery);
            
            return discovery;
        }

        public bool ProcessDiscoveryEvent(DiscoveryEvent discoveryEvent)
        {
            if (!IsInitialized)
            {
                Debug.LogError("DiscoverySystemService not initialized");
                return false;
            }

            try
            {
                // Create context from event
                var context = CreateContextFromEvent(discoveryEvent);
                
                // Trigger discovery
                var discovery = TriggerDiscovery(context);
                
                return discovery != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing discovery event: {ex.Message}");
                return false;
            }
        }

        public List<Discovery> GetRecentDiscoveries()
        {
            return new List<Discovery>(_recentDiscoveries);
        }

        public Discovery GetDiscovery(string discoveryId)
        {
            return _recentDiscoveries.FirstOrDefault(d => d.DiscoveryId == discoveryId);
        }
        
        #endregion

        #region Innovation System
        
        public Innovation ProcessInnovation(InnovationTrigger trigger)
        {
            if (!IsInitialized)
            {
                Debug.LogError("DiscoverySystemService not initialized");
                return null;
            }

            var innovation = GenerateInnovation(trigger);
            if (innovation == null)
                return null;

            // Validate innovation
            if (!ValidateInnovation(innovation))
            {
                Debug.LogWarning($"Innovation validation failed: {innovation.Name}");
                return null;
            }

            // Add to player innovations
            _playerInnovations.Add(innovation);
            
            // Apply innovation effects
            ApplyInnovationEffects(innovation);

            OnInnovationAchieved?.Invoke(innovation);
            Debug.Log($"Innovation achieved: {innovation.Name}");
            
            return innovation;
        }

        public List<Innovation> GetPlayerInnovations(string playerId)
        {
            // For now, return all innovations (single player context)
            return new List<Innovation>(_playerInnovations);
        }

        public bool ValidateInnovation(Innovation innovation)
        {
            if (innovation == null)
                return false;

            if (string.IsNullOrEmpty(innovation.Name))
                return false;

            // Check for duplicate innovations
            if (_playerInnovations.Any(i => i.Name == innovation.Name))
                return false;

            return true;
        }
        
        #endregion

        #region Breakthrough Mechanics
        
        public Breakthrough ProcessBreakthrough(BreakthroughConditions conditions)
        {
            if (!IsInitialized)
            {
                Debug.LogError("DiscoverySystemService not initialized");
                return null;
            }

            if (!IsBreakthroughEligible(conditions))
            {
                Debug.LogWarning("Breakthrough conditions not met");
                return null;
            }

            var breakthrough = GenerateBreakthrough(conditions);
            if (breakthrough == null)
                return null;

            // Add to breakthroughs
            _breakthroughs.Add(breakthrough);
            
            // Process breakthrough effects
            ProcessBreakthroughEffects(breakthrough);

            OnBreakthroughOccurred?.Invoke(breakthrough);
            Debug.Log($"Breakthrough achieved: {breakthrough.Name}");
            
            return breakthrough;
        }

        public List<Breakthrough> GetBreakthroughs()
        {
            return new List<Breakthrough>(_breakthroughs);
        }

        public bool IsBreakthroughEligible(BreakthroughConditions conditions)
        {
            return conditions switch
            {
                BreakthroughConditions.MultipleProjects => CheckMultipleProjectsCondition(),
                BreakthroughConditions.HighExperience => CheckHighExperienceCondition(),
                BreakthroughConditions.RareConditions => CheckRareConditionsCondition(),
                BreakthroughConditions.PerfectTiming => CheckPerfectTimingCondition(),
                BreakthroughConditions.RandomChance => CheckRandomChanceCondition(),
                _ => false
            };
        }
        
        #endregion

        #region Private Helper Methods
        
        private void InitializeDiscoverySystem()
        {
            _discoveryRandom = new System.Random();
            _timeSinceLastDiscoveryCheck = 0f;
            
            Debug.Log("Discovery system initialized");
        }

        private void SaveDiscoveryState()
        {
            // TODO: Save to persistent storage
            Debug.Log("Saving discovery state...");
        }

        private void GenerateInitialPotentialDiscoveries()
        {
            // Generate basic potential discoveries
            for (int i = 0; i < 10; i++)
            {
                var potentialDiscovery = new PotentialDiscovery
                {
                    DiscoveryId = $"potential_discovery_{i}",
                    Name = $"Potential Discovery {i + 1}",
                    Type = GetRandomDiscoveryType(),
                    DiscoveryProbability = UnityEngine.Random.Range(0.1f, 0.5f)
                };
                
                _potentialDiscoveries.Add(potentialDiscovery);
            }
            
            Debug.Log($"Generated {_potentialDiscoveries.Count} potential discoveries");
        }

        private PotentialDiscovery FindMatchingPotentialDiscovery(DiscoveryContext context)
        {
            return _potentialDiscoveries
                .Where(pd => IsDiscoveryContextMatch(pd, context))
                .OrderByDescending(pd => pd.DiscoveryProbability)
                .FirstOrDefault();
        }

        private PotentialDiscovery GenerateDynamicDiscovery(DiscoveryContext context)
        {
            return new PotentialDiscovery
            {
                DiscoveryId = Guid.NewGuid().ToString(),
                Name = $"Dynamic Discovery - {context.Category}",
                Type = GetDiscoveryTypeFromCategory(context.Category),
                DiscoveryProbability = _baseDiscoveryChance
            };
        }

        private bool CheckDiscoveryProbability(PotentialDiscovery potentialDiscovery, DiscoveryContext context)
        {
            float roll = (float)_discoveryRandom.NextDouble();
            float adjustedProbability = potentialDiscovery.DiscoveryProbability * GetContextMultiplier(context);
            
            return roll <= adjustedProbability;
        }

        private Discovery CreateDiscovery(PotentialDiscovery potentialDiscovery, DiscoveryContext context)
        {
            return new Discovery
            {
                DiscoveryId = potentialDiscovery.DiscoveryId,
                Name = potentialDiscovery.Name,
                Type = potentialDiscovery.Type,
                DiscoveryDate = DateTime.Now,
                DiscoverySource = "Research Activity",
                UnlockedTechnologies = GenerateUnlockedTechnologies(potentialDiscovery.Type)
            };
        }

        private void ProcessDiscovery(Discovery discovery)
        {
            // Add to recent discoveries
            _recentDiscoveries.Add(discovery);
            
            // Keep recent discoveries manageable
            if (_recentDiscoveries.Count > 50)
            {
                _recentDiscoveries.RemoveRange(0, 10);
            }

            // Unlock associated technologies
            if (_technologyService != null && discovery.UnlockedTechnologies != null)
            {
                foreach (var techUnlock in discovery.UnlockedTechnologies)
                {
                    _technologyService.UnlockTechnology(techUnlock.TechnologyId);
                }
            }

            OnDiscoveryMade?.Invoke(discovery);
        }

        private DiscoveryContext CreateContextFromEvent(DiscoveryEvent discoveryEvent)
        {
            return new DiscoveryContext
            {
                ContextId = Guid.NewGuid().ToString(),
                Category = ResearchCategory.Innovation, // Default category
                ActiveProjects = _projectService?.GetActiveProjects()?.Select(p => p.ProjectId).ToList() ?? new List<string>(),
                EnvironmentalFactors = new Dictionary<string, float>(),
                PlayerExperience = 50f // Default experience
            };
        }

        private Innovation GenerateInnovation(InnovationTrigger trigger)
        {
            var innovationType = trigger switch
            {
                InnovationTrigger.Research => InnovationType.Incremental,
                InnovationTrigger.Problem => InnovationType.Breakthrough,
                InnovationTrigger.Opportunity => InnovationType.Disruptive,
                _ => InnovationType.Incremental
            };

            return new Innovation
            {
                InnovationId = Guid.NewGuid().ToString(),
                Name = $"Innovation - {trigger}",
                Type = innovationType,
                Trigger = trigger,
                ImpactScore = UnityEngine.Random.Range(10f, 100f),
                CreationDate = DateTime.Now
            };
        }

        private void ApplyInnovationEffects(Innovation innovation)
        {
            // TODO: Apply gameplay effects
            Debug.Log($"Applied effects for innovation: {innovation.Name}");
        }

        private Breakthrough GenerateBreakthrough(BreakthroughConditions conditions)
        {
            return new Breakthrough
            {
                BreakthroughId = Guid.NewGuid().ToString(),
                Name = $"Breakthrough - {conditions}",
                Description = $"Major breakthrough achieved through {conditions}",
                AchievementDate = DateTime.Now,
                IsRepeatable = conditions == BreakthroughConditions.RandomChance
            };
        }

        private void ProcessBreakthroughEffects(Breakthrough breakthrough)
        {
            // TODO: Apply breakthrough effects
            Debug.Log($"Processed breakthrough effects: {breakthrough.Name}");
        }

        private bool CheckMultipleProjectsCondition()
        {
            return _projectService?.GetActiveProjects()?.Count >= 3;
        }

        private bool CheckHighExperienceCondition()
        {
            // TODO: Check actual player research experience
            return true;
        }

        private bool CheckRareConditionsCondition()
        {
            // Rare random chance
            return _discoveryRandom.NextDouble() < 0.05; // 5% chance
        }

        private bool CheckPerfectTimingCondition()
        {
            // Check if conditions align perfectly
            return _discoveryRandom.NextDouble() < 0.1; // 10% chance
        }

        private bool CheckRandomChanceCondition()
        {
            return _discoveryRandom.NextDouble() < _baseDiscoveryChance;
        }

        private DiscoveryType GetRandomDiscoveryType()
        {
            var types = Enum.GetValues(typeof(DiscoveryType)).Cast<DiscoveryType>().ToArray();
            return types[_discoveryRandom.Next(types.Length)];
        }

        private DiscoveryType GetDiscoveryTypeFromCategory(ResearchCategory category)
        {
            return category switch
            {
                ResearchCategory.Genetics => DiscoveryType.Genetic,
                ResearchCategory.Cultivation => DiscoveryType.Cultivation,
                ResearchCategory.Processing => DiscoveryType.Processing,
                ResearchCategory.Equipment => DiscoveryType.Equipment,
                _ => DiscoveryType.Method
            };
        }

        private bool IsDiscoveryContextMatch(PotentialDiscovery discovery, DiscoveryContext context)
        {
            // Check if discovery type matches context category
            var expectedType = GetDiscoveryTypeFromCategory(context.Category);
            return discovery.Type == expectedType;
        }

        private float GetContextMultiplier(DiscoveryContext context)
        {
            float multiplier = 1.0f;
            
            // Boost based on active projects
            multiplier += context.ActiveProjects.Count * 0.1f;
            
            // Boost based on player experience
            multiplier += context.PlayerExperience * 0.01f;
            
            return Mathf.Clamp(multiplier, 0.5f, 3.0f);
        }

        private List<TechnologyUnlock> GenerateUnlockedTechnologies(DiscoveryType discoveryType)
        {
            var unlocks = new List<TechnologyUnlock>();
            
            // Generate 1-2 technology unlocks based on discovery type
            int unlockCount = _discoveryRandom.Next(1, 3);
            
            for (int i = 0; i < unlockCount; i++)
            {
                var unlock = new TechnologyUnlock
                {
                    TechnologyId = $"tech_{discoveryType}_{i}",
                    TechnologyName = $"Technology from {discoveryType}",
                    UnlockDescription = $"Unlocked through discovery of type {discoveryType}"
                };
                
                unlocks.Add(unlock);
            }
            
            return unlocks;
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

        private void Update()
        {
            if (!IsInitialized || !_enableRandomDiscoveries) return;

            _timeSinceLastDiscoveryCheck += Time.deltaTime;

            if (_timeSinceLastDiscoveryCheck >= _discoveryCheckInterval)
            {
                CheckForRandomDiscoveries();
                _timeSinceLastDiscoveryCheck = 0f;
            }
        }

        private void CheckForRandomDiscoveries()
        {
            if (_discoveryRandom.NextDouble() < _baseDiscoveryChance)
            {
                var context = new DiscoveryContext
                {
                    ContextId = Guid.NewGuid().ToString(),
                    Category = GetRandomResearchCategory(),
                    ActiveProjects = _projectService?.GetActiveProjects()?.Select(p => p.ProjectId).ToList() ?? new List<string>(),
                    PlayerExperience = 50f
                };

                TriggerDiscovery(context);
            }
        }

        private ResearchCategory GetRandomResearchCategory()
        {
            var categories = Enum.GetValues(typeof(ResearchCategory)).Cast<ResearchCategory>().ToArray();
            return categories[_discoveryRandom.Next(categories.Length)];
        }
        
        #endregion
    }
}