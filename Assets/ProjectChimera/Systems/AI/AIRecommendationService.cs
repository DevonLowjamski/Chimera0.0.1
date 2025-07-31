using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.AI;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Systems.Environment;
using ProjectChimera.Systems.Genetics;
using ProjectChimera.Systems.Progression;
using PlantGrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage;
// using GameEventBus = ProjectChimera.Systems.Gaming.GameEventBus; // Temporarily disabled - Gaming namespace not accessible
using AIRecommendationPriority = ProjectChimera.Data.AI.RecommendationPriority;
using AIEnvironmentalAnalysisResult = ProjectChimera.Data.AI.EnvironmentalAnalysisResult;
using EnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// PC-012-2: AI Recommendation Service - Specialized AI recommendation generation and management
    /// Extracted from AIAdvisorManager for improved modularity and testability
    /// Handles recommendation generation, prioritization, filtering, and lifecycle management
    /// Target: 750 lines focused on recommendation processing
    /// </summary>
    public class AIRecommendationService : MonoBehaviour, IAIRecommendationService
    {
        [Header("Recommendation Configuration")]
        [SerializeField] private RecommendationSettings _recommendationConfig;
        [SerializeField] private int _maxRecommendationsPerSession = 5;
        [SerializeField] private int _maxActiveRecommendations = 20;
        [SerializeField] private AIRecommendationPriority _defaultPriority = AIRecommendationPriority.Medium;
        [SerializeField] private float _recommendationValidityHours = 24f;
        [SerializeField] private bool _enableSmartFiltering = true;
        
        [Header("Generation Settings")]
        [SerializeField] private float _generationInterval = 60f;
        [SerializeField] private float _confidenceThreshold = 0.6f;
        [SerializeField] private bool _enableContextualRecommendations = true;
        [SerializeField] private bool _enableEducationalRecommendations = true;
        [SerializeField] private bool _enableOptimizationRecommendations = true;
        
        [Header("Filtering & Prioritization")]
        [SerializeField] private float _duplicateThreshold = 0.8f;
        [SerializeField] private int _maxRecommendationsPerCategory = 3;
        [SerializeField] private bool _enablePlayerHistoryFiltering = true;
        [SerializeField] private float _playerFeedbackWeight = 0.3f;
        
        // Service dependencies - injected via DI container
        private IAIAnalysisService _analysisService;
        private ICultivationAnalysisService _cultivationAnalysisService;
        private IEnvironmentalAnalysisService _environmentalAnalysisService;
        private IGeneticsAnalysisService _geneticsAnalysisService;
        private IPlayerProgressionService _progressionService;
        private MonoBehaviour _eventBus; // Temporarily using MonoBehaviour instead of GameEventBus
        
        // Recommendation processing state
        private bool _isGeneratingRecommendations;
        private Queue<AIRecommendation> _pendingRecommendations = new Queue<AIRecommendation>();
        private List<AIRecommendation> _activeRecommendations = new List<AIRecommendation>();
        private Dictionary<string, AIRecommendation> _recommendationHistory = new Dictionary<string, AIRecommendation>();
        private Dictionary<string, float> _categoryWeights = new Dictionary<string, float>();
        
        // Player interaction tracking
        private Dictionary<string, PlayerRecommendationFeedback> _playerFeedback = new Dictionary<string, PlayerRecommendationFeedback>();
        private List<string> _dismissedRecommendationTypes = new List<string>();
        private Dictionary<string, int> _implementedRecommendationCounts = new Dictionary<string, int>();
        
        // Generation timing
        private float _lastGenerationTime;
        private float _nextScheduledGeneration;
        
        // Properties
        public bool IsInitialized { get; private set; }
        public int ActiveRecommendationCount => _activeRecommendations.Count;
        public int PendingRecommendationCount => _pendingRecommendations.Count;
        public List<AIRecommendation> ActiveRecommendations => _activeRecommendations.ToList();
        public List<AIRecommendation> RecommendationHistory => _recommendationHistory.Values.ToList();
        public RecommendationStatistics Statistics { get; private set; }
        
        // Events
        public event Action<List<AIRecommendation>> OnRecommendationsGenerated;
        public event Action<AIRecommendation> OnRecommendationCreated;
        public event Action<AIRecommendation> OnRecommendationImplemented;
        public event Action<AIRecommendation> OnRecommendationDismissed;
        public event Action<AIRecommendation> OnRecommendationExpired;
        
        #region Initialization & Lifecycle
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            LogInfo("Initializing AI Recommendation Service...");
            
            InitializeServiceDependencies();
            InitializeRecommendationEngine();
            InitializeCategoryWeights();
            InitializePlayerFeedbackTracking();
            InitializeEventSubscriptions();
            
            _lastGenerationTime = Time.time;
            _nextScheduledGeneration = Time.time + _generationInterval;
            
            Statistics = new RecommendationStatistics();
            
            IsInitialized = true;
            LogInfo("AI Recommendation Service initialized successfully");
        }
        
        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            LogInfo("Shutting down AI Recommendation Service...");
            
            // Save recommendation state
            SaveRecommendationData();
            
            // Clean up active recommendations
            _activeRecommendations.Clear();
            _pendingRecommendations.Clear();
            _recommendationHistory.Clear();
            _playerFeedback.Clear();
            
            // Reset processing flags
            _isGeneratingRecommendations = false;
            
            IsInitialized = false;
            LogInfo("AI Recommendation Service shutdown complete");
        }
        
        private void InitializeServiceDependencies()
        {
            // Service dependencies will be injected via DI container or found in scene
            _analysisService = FindObjectOfType<MonoBehaviour>() as IAIAnalysisService ?? 
                            GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IAIAnalysisService>().FirstOrDefault();
            
            // Try to get the same service cast to specialized interfaces
            if (_analysisService != null)
            {
                _cultivationAnalysisService = _analysisService as ICultivationAnalysisService;
                _environmentalAnalysisService = _analysisService as IEnvironmentalAnalysisService;
                _geneticsAnalysisService = _analysisService as IGeneticsAnalysisService;
            }
            
            _progressionService = FindObjectOfType<MonoBehaviour>() as IPlayerProgressionService ?? 
                                GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IPlayerProgressionService>().FirstOrDefault();
            _eventBus = FindObjectOfType<MonoBehaviour>(); // Placeholder for GameEventBus
            
            // Allow null services with warnings
            if (_analysisService == null) LogWarning("AIAnalysisService not found - analysis-based recommendations will be limited");
            if (_cultivationAnalysisService == null) LogWarning("CultivationAnalysisService not available - cultivation recommendations will be limited");
            if (_environmentalAnalysisService == null) LogWarning("EnvironmentalAnalysisService not available - environmental recommendations will be limited");
            if (_geneticsAnalysisService == null) LogWarning("GeneticsAnalysisService not available - genetics recommendations will be limited");
            if (_progressionService == null) LogWarning("PlayerProgressionService not found - progression-based recommendations will be limited");
            if (_eventBus == null) LogWarning("EventBus not found - event integration will be limited");
        }
        
        private void InitializeRecommendationEngine()
        {
            // Initialize recommendation processing systems
            _categoryWeights = new Dictionary<string, float>
            {
                ["Plant Health"] = 1.0f,
                ["Growth Optimization"] = 0.9f,
                ["Environmental"] = 0.8f,
                ["Care Optimization"] = 0.7f,
                ["Breeding"] = 0.6f,
                ["Genetics"] = 0.6f,
                ["Educational"] = 0.5f,
                ["Climate Control"] = 0.8f,
                ["Trait Optimization"] = 0.6f,
                ["Economic"] = 0.4f,
                ["Strategic"] = 0.3f
            };
            
            LogInfo("Recommendation engine initialized with category weighting");
        }
        
        private void InitializeCategoryWeights()
        {
            // Adjust category weights based on player progression
            if (_progressionService != null)
            {
                // Placeholder for progression-based weight adjustment
                // var playerLevel = _progressionService.GetPlayerLevel();
                // AdjustCategoryWeightsForPlayerLevel(playerLevel);
            }
        }
        
        private void InitializePlayerFeedbackTracking()
        {
            // Initialize player feedback tracking systems
            _dismissedRecommendationTypes = new List<string>();
            _implementedRecommendationCounts = new Dictionary<string, int>();
        }
        
        private void InitializeEventSubscriptions()
        {
            // Subscribe to analysis completion events
            if (_analysisService is IAIAnalysisServiceExtended extendedService)
            {
                extendedService.OnCultivationAnalysisComplete += OnCultivationAnalysisComplete;
                extendedService.OnEnvironmentalAnalysisComplete += OnEnvironmentalAnalysisComplete;
                extendedService.OnGeneticsAnalysisComplete += OnGeneticsAnalysisComplete;
                extendedService.OnCriticalInsight += OnCriticalInsight;
                extendedService.OnOptimizationIdentified += OnOptimizationIdentified;
                
                LogInfo("Analysis service event subscriptions established");
            }
            
            if (_eventBus != null)
            {
                // Event subscriptions temporarily disabled - event types need to be verified
                // _eventBus.Subscribe<PlayerActionEvent>(OnPlayerAction);
                // _eventBus.Subscribe<RecommendationImplementedEvent>(OnRecommendationImplemented);
                // _eventBus.Subscribe<RecommendationDismissedEvent>(OnRecommendationDismissed);
                LogInfo("Event subscriptions prepared (event types pending verification)");
            }
        }
        
        #endregion
        
        #region Core Recommendation Interface
        
        public async Task<List<AIRecommendation>> GenerateRecommendationsAsync(CultivationAnalysisResult analysisResult)
        {
            if (!IsInitialized)
            {
                LogWarning("Recommendation service not initialized");
                return new List<AIRecommendation>();
            }
            
            if (_isGeneratingRecommendations)
            {
                LogWarning("Recommendation generation already in progress");
                return ActiveRecommendations;
            }
            
            try
            {
                _isGeneratingRecommendations = true;
                LogInfo("Starting comprehensive recommendation generation...");
                
                var recommendations = new List<AIRecommendation>();
                
                // Generate different types of recommendations
                recommendations.AddRange(await GenerateCultivationRecommendations(analysisResult));
                recommendations.AddRange(await GenerateEnvironmentalRecommendations(analysisResult));
                recommendations.AddRange(await GenerateGeneticsRecommendations(analysisResult));
                
                if (_enableEducationalRecommendations)
                {
                    recommendations.AddRange(await GenerateEducationalRecommendations(analysisResult));
                }
                
                if (_enableOptimizationRecommendations)
                {
                    recommendations.AddRange(await GenerateOptimizationRecommendations(analysisResult));
                }
                
                // Process recommendations through filtering and prioritization pipeline
                var processedRecommendations = await ProcessRecommendationPipeline(recommendations);
                
                // Update active recommendations
                UpdateActiveRecommendations(processedRecommendations);
                
                // Update statistics
                UpdateStatistics(processedRecommendations);
                
                // Publish recommendations generated event
                if (_eventBus != null)
                {
                    // Event publishing temporarily disabled - event types need verification
                    // await _eventBus.PublishAsync(new AIRecommendationsGeneratedEvent
                    // {
                    //     Recommendations = processedRecommendations,
                    //     AnalysisResult = analysisResult,
                    //     Timestamp = DateTime.UtcNow
                    // });
                }
                
                OnRecommendationsGenerated?.Invoke(processedRecommendations);
                
                _lastGenerationTime = Time.time;
                _nextScheduledGeneration = Time.time + _generationInterval;
                
                LogInfo($"Recommendation generation complete - {processedRecommendations.Count} recommendations generated");
                return processedRecommendations;
            }
            catch (Exception ex)
            {
                LogError($"Error during recommendation generation: {ex.Message}");
                return new List<AIRecommendation>();
            }
            finally
            {
                _isGeneratingRecommendations = false;
            }
        }
        
        public AIRecommendation CreateRecommendation(string title, string description, AIRecommendationPriority priority, string category)
        {
            if (!IsInitialized)
            {
                LogWarning("Recommendation service not initialized");
                return null;
            }
            
            var recommendation = new AIRecommendation
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Description = description,
                Priority = priority,
                Category = category,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(_recommendationValidityHours),
                Status = RecommendationStatus.Active,
                ConfidenceScore = CalculateConfidenceScore(category, priority),
                ImpactScore = CalculateImpactScore(category, priority)
            };
            
            // Add to tracking systems
            _recommendationHistory[recommendation.Id] = recommendation;
            
            OnRecommendationCreated?.Invoke(recommendation);
            
            return recommendation;
        }
        
        public void ImplementRecommendation(string recommendationId)
        {
            var recommendation = _activeRecommendations.FirstOrDefault(r => r.Id == recommendationId);
            if (recommendation == null)
            {
                LogWarning($"Recommendation {recommendationId} not found in active recommendations");
                return;
            }
            
            recommendation.Status = RecommendationStatus.Implemented;
            recommendation.ImplementedAt = DateTime.UtcNow;
            
            // Track implementation for learning
            TrackRecommendationImplementation(recommendation);
            
            // Remove from active recommendations
            _activeRecommendations.Remove(recommendation);
            
            // Update statistics
            Statistics.TotalImplemented++;
            if (_implementedRecommendationCounts.ContainsKey(recommendation.Category))
                _implementedRecommendationCounts[recommendation.Category]++;
            else
                _implementedRecommendationCounts[recommendation.Category] = 1;
            
            OnRecommendationImplemented?.Invoke(recommendation);
            
            LogInfo($"Recommendation implemented: {recommendation.Title}");
        }
        
        public void DismissRecommendation(string recommendationId, string dismissalReason)
        {
            var recommendation = _activeRecommendations.FirstOrDefault(r => r.Id == recommendationId);
            if (recommendation == null)
            {
                LogWarning($"Recommendation {recommendationId} not found in active recommendations");
                return;
            }
            
            recommendation.Status = RecommendationStatus.Dismissed;
            recommendation.DismissalReason = dismissalReason;
            
            // Track dismissal for learning
            TrackRecommendationDismissal(recommendation, dismissalReason);
            
            // Remove from active recommendations
            _activeRecommendations.Remove(recommendation);
            
            // Update statistics
            Statistics.TotalDismissed++;
            
            OnRecommendationDismissed?.Invoke(recommendation);
            
            LogInfo($"Recommendation dismissed: {recommendation.Title} - {dismissalReason}");
        }
        
        public List<AIRecommendation> GetRecommendationsByCategory(string category)
        {
            return _activeRecommendations.Where(r => r.Category == category).ToList();
        }
        
        public List<AIRecommendation> GetRecommendationsByPriority(AIRecommendationPriority priority)
        {
            return _activeRecommendations.Where(r => r.Priority == priority).ToList();
        }
        
        #endregion
        
        #region Recommendation Generation
        
        private async Task<List<AIRecommendation>> GenerateCultivationRecommendations(CultivationAnalysisResult analysisResult)
        {
            var recommendations = new List<AIRecommendation>();
            
            if (analysisResult == null) return recommendations;
            
            // Plant health recommendations
            if (_cultivationAnalysisService != null)
            {
                var healthRecommendations = _cultivationAnalysisService.AnalyzePlantHealth();
                recommendations.AddRange(healthRecommendations);
                
                var growthRecommendations = _cultivationAnalysisService.AnalyzeGrowthPatterns();
                recommendations.AddRange(growthRecommendations);
                
                var careRecommendations = _cultivationAnalysisService.AnalyzeCareOptimization();
                recommendations.AddRange(careRecommendations);
            }
            
            // Generate yield optimization recommendations
            if (analysisResult.PredictedYield < analysisResult.OptimalYield * 0.8f)
            {
                recommendations.Add(CreateRecommendation(
                    "Yield Optimization Opportunity",
                    $"Potential to increase yield by {(analysisResult.OptimalYield - analysisResult.PredictedYield):F1} units",
                    AIRecommendationPriority.High,
                    "Growth Optimization"
                ));
            }
            
            // Generate growth stage-specific recommendations
            foreach (var stageGroup in analysisResult.GrowthStageDistribution)
            {
                if (stageGroup.Value > 0)
                {
                    var stageRecommendations = GenerateStageSpecificRecommendations(stageGroup.Key, stageGroup.Value);
                    recommendations.AddRange(stageRecommendations);
                }
            }
            
            return await Task.FromResult(recommendations);
        }
        
        private async Task<List<AIRecommendation>> GenerateEnvironmentalRecommendations(CultivationAnalysisResult analysisResult)
        {
            var recommendations = new List<AIRecommendation>();
            
            if (_environmentalAnalysisService != null)
            {
                var environmentalRecommendations = _environmentalAnalysisService.AnalyzeEnvironmentalOptimization();
                recommendations.AddRange(environmentalRecommendations);
                
                var climateRecommendations = _environmentalAnalysisService.AnalyzeClimateControl();
                recommendations.AddRange(climateRecommendations);
            }
            
            return await Task.FromResult(recommendations);
        }
        
        private async Task<List<AIRecommendation>> GenerateGeneticsRecommendations(CultivationAnalysisResult analysisResult)
        {
            var recommendations = new List<AIRecommendation>();
            
            if (_geneticsAnalysisService != null)
            {
                var breedingRecommendations = _geneticsAnalysisService.AnalyzeBreedingOpportunities();
                recommendations.AddRange(breedingRecommendations);
                
                var diversityRecommendations = _geneticsAnalysisService.AnalyzeGeneticDiversity();
                recommendations.AddRange(diversityRecommendations);
                
                var traitRecommendations = _geneticsAnalysisService.AnalyzeTraitOptimization();
                recommendations.AddRange(traitRecommendations);
            }
            
            return await Task.FromResult(recommendations);
        }
        
        private async Task<List<AIRecommendation>> GenerateEducationalRecommendations(CultivationAnalysisResult analysisResult)
        {
            var recommendations = new List<AIRecommendation>();
            
            // Generate educational content based on current cultivation state
            if (analysisResult != null && analysisResult.TotalPlants > 0)
            {
                // Beginner tips for new growers
                if (analysisResult.TotalPlants <= 5)
                {
                    recommendations.Add(CreateRecommendation(
                        "Growing Basics: Plant Monitoring",
                        "Learn how to properly monitor plant health and growth indicators",
                        AIRecommendationPriority.Low,
                        "Educational"
                    ));
                }
                
                // Advanced tips for experienced growers
                if (analysisResult.EfficiencyScore > 0.8f)
                {
                    recommendations.Add(CreateRecommendation(
                        "Advanced Techniques: Yield Maximization",
                        "Explore advanced cultivation techniques to push yields to their genetic potential",
                        AIRecommendationPriority.Low,
                        "Educational"
                    ));
                }
            }
            
            return await Task.FromResult(recommendations);
        }
        
        private async Task<List<AIRecommendation>> GenerateOptimizationRecommendations(CultivationAnalysisResult analysisResult)
        {
            var recommendations = new List<AIRecommendation>();
            
            if (_environmentalAnalysisService != null)
            {
                var optimizations = _environmentalAnalysisService.GetEnvironmentalOptimizations();
                
                foreach (var optimization in optimizations)
                {
                    if (optimization.IsActive && optimization.BenefitScore > 0.5f)
                    {
                        recommendations.Add(CreateRecommendation(
                            optimization.Title,
                            optimization.Description,
                            AIRecommendationPriority.Medium, // Default priority since OptimizationOpportunity doesn't have Priority
                            optimization.Category ?? "Optimization"
                        ));
                    }
                }
            }
            
            return await Task.FromResult(recommendations);
        }
        
        private List<AIRecommendation> GenerateStageSpecificRecommendations(PlantGrowthStage stage, int plantCount)
        {
            var recommendations = new List<AIRecommendation>();
            
            switch (stage)
            {
                case PlantGrowthStage.Seedling:
                    recommendations.Add(CreateRecommendation(
                        "Seedling Care Focus",
                        $"{plantCount} seedlings require gentle care and stable environment",
                        AIRecommendationPriority.Medium,
                        "Growth Optimization"
                    ));
                    break;
                    
                case PlantGrowthStage.Vegetative:
                    recommendations.Add(CreateRecommendation(
                        "Vegetative Growth Optimization",
                        $"{plantCount} plants in vegetative stage - optimize lighting and nutrients",
                        AIRecommendationPriority.Medium,
                        "Growth Optimization"
                    ));
                    break;
                    
                case PlantGrowthStage.Flowering:
                    recommendations.Add(CreateRecommendation(
                        "Flowering Stage Monitoring",
                        $"{plantCount} flowering plants require close monitoring for harvest timing",
                        AIRecommendationPriority.High,
                        "Growth Optimization"
                    ));
                    break;
                    
                case PlantGrowthStage.Harvestable:
                    recommendations.Add(CreateRecommendation(
                        "Harvest Ready",
                        $"{plantCount} plants are ready for harvest - timing is critical for quality",
                        AIRecommendationPriority.Critical,
                        "Growth Optimization"
                    ));
                    break;
            }
            
            return recommendations;
        }
        
        #endregion
        
        #region Recommendation Processing Pipeline
        
        private async Task<List<AIRecommendation>> ProcessRecommendationPipeline(List<AIRecommendation> recommendations)
        {
            // Step 1: Filter by confidence threshold
            var filteredRecommendations = FilterByConfidence(recommendations);
            
            // Step 2: Remove duplicates and similar recommendations
            var deduplicatedRecommendations = RemoveDuplicates(filteredRecommendations);
            
            // Step 3: Apply player history filtering
            var historyFilteredRecommendations = ApplyPlayerHistoryFiltering(deduplicatedRecommendations);
            
            // Step 4: Prioritize recommendations
            var prioritizedRecommendations = PrioritizeRecommendations(historyFilteredRecommendations);
            
            // Step 5: Apply category limits
            var categoryLimitedRecommendations = ApplyCategoryLimits(prioritizedRecommendations);
            
            // Step 6: Apply session limits
            var finalRecommendations = ApplySessionLimits(categoryLimitedRecommendations);
            
            return await Task.FromResult(finalRecommendations);
        }
        
        private List<AIRecommendation> FilterByConfidence(List<AIRecommendation> recommendations)
        {
            return recommendations.Where(r => r.ConfidenceScore >= _confidenceThreshold).ToList();
        }
        
        private List<AIRecommendation> RemoveDuplicates(List<AIRecommendation> recommendations)
        {
            var uniqueRecommendations = new List<AIRecommendation>();
            
            foreach (var recommendation in recommendations)
            {
                bool isDuplicate = false;
                
                foreach (var existing in uniqueRecommendations)
                {
                    if (CalculateSimilarity(recommendation, existing) >= _duplicateThreshold)
                    {
                        isDuplicate = true;
                        break;
                    }
                }
                
                if (!isDuplicate)
                {
                    uniqueRecommendations.Add(recommendation);
                }
            }
            
            return uniqueRecommendations;
        }
        
        private List<AIRecommendation> ApplyPlayerHistoryFiltering(List<AIRecommendation> recommendations)
        {
            if (!_enablePlayerHistoryFiltering)
                return recommendations;
            
            return recommendations.Where(r => !ShouldFilterBasedOnHistory(r)).ToList();
        }
        
        public List<AIRecommendation> PrioritizeRecommendations(List<AIRecommendation> recommendations)
        {
            return recommendations
                .OrderByDescending(r => (int)r.Priority)
                .ThenByDescending(r => r.ImpactScore)
                .ThenByDescending(r => r.ConfidenceScore)
                .ThenByDescending(r => GetCategoryWeight(r.Category))
                .ToList();
        }
        
        public List<AIRecommendation> FilterRecommendations(List<AIRecommendation> recommendations, string category)
        {
            if (string.IsNullOrEmpty(category))
                return recommendations;
                
            return recommendations.Where(r => 
                string.Equals(r.Category, category, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        
        private List<AIRecommendation> ApplyCategoryLimits(List<AIRecommendation> recommendations)
        {
            var categoryCounts = new Dictionary<string, int>();
            var limitedRecommendations = new List<AIRecommendation>();
            
            foreach (var recommendation in recommendations)
            {
                var category = recommendation.Category ?? "General";
                var count = categoryCounts.GetValueOrDefault(category, 0);
                
                if (count < _maxRecommendationsPerCategory)
                {
                    limitedRecommendations.Add(recommendation);
                    categoryCounts[category] = count + 1;
                }
            }
            
            return limitedRecommendations;
        }
        
        private List<AIRecommendation> ApplySessionLimits(List<AIRecommendation> recommendations)
        {
            return recommendations.Take(_maxRecommendationsPerSession).ToList();
        }
        
        #endregion
        
        #region Helper Methods
        
        private float CalculateConfidenceScore(string category, AIRecommendationPriority priority)
        {
            float baseScore = 0.5f;
            
            // Adjust based on priority
            switch (priority)
            {
                case AIRecommendationPriority.Critical:
                    baseScore = 0.95f;
                    break;
                case AIRecommendationPriority.High:
                    baseScore = 0.8f;
                    break;
                case AIRecommendationPriority.Medium:
                    baseScore = 0.65f;
                    break;
                case AIRecommendationPriority.Low:
                    baseScore = 0.5f;
                    break;
            }
            
            // Adjust based on category weight
            float categoryWeight = GetCategoryWeight(category);
            return Mathf.Clamp01(baseScore * categoryWeight);
        }
        
        private float CalculateImpactScore(string category, AIRecommendationPriority priority)
        {
            float baseScore = 0.5f;
            
            // Critical recommendations have high impact
            if (priority == AIRecommendationPriority.Critical)
                baseScore = 0.9f;
            else if (priority == AIRecommendationPriority.High)
                baseScore = 0.7f;
            
            // Category-specific impact adjustments
            switch (category)
            {
                case "Plant Health":
                case "Growth Optimization":
                    baseScore *= 1.2f;
                    break;
                case "Environmental":
                case "Climate Control":
                    baseScore *= 1.1f;
                    break;
                case "Educational":
                    baseScore *= 0.8f;
                    break;
            }
            
            return Mathf.Clamp01(baseScore);
        }
        
        private float GetCategoryWeight(string category)
        {
            return _categoryWeights.GetValueOrDefault(category ?? "General", 0.5f);
        }
        
        private float CalculateSimilarity(AIRecommendation rec1, AIRecommendation rec2)
        {
            // Simple similarity calculation based on category and title
            if (rec1.Category != rec2.Category)
                return 0f;
            
            if (rec1.Title == rec2.Title)
                return 1f;
            
            // Basic string similarity
            return CalculateStringSimilarity(rec1.Title, rec2.Title);
        }
        
        private float CalculateStringSimilarity(string str1, string str2)
        {
            // Simple word-based similarity
            var words1 = str1.ToLower().Split(' ');
            var words2 = str2.ToLower().Split(' ');
            
            int commonWords = words1.Intersect(words2).Count();
            int totalWords = words1.Union(words2).Count();
            
            return totalWords > 0 ? (float)commonWords / totalWords : 0f;
        }
        
        private bool ShouldFilterBasedOnHistory(AIRecommendation recommendation)
        {
            // Filter out frequently dismissed recommendation types
            if (_dismissedRecommendationTypes.Contains(recommendation.Category))
                return true;
            
            // Filter out recommendations with poor feedback
            if (_playerFeedback.ContainsKey(recommendation.Category))
            {
                var feedback = _playerFeedback[recommendation.Category];
                if (feedback.AverageRating < 2f && feedback.TotalFeedback > 3)
                    return true;
            }
            
            return false;
        }
        
        private AIRecommendationPriority ConvertOptimizationPriorityToRecommendationPriority(float priority)
        {
            if (priority >= 0.8f) return AIRecommendationPriority.High;
            if (priority >= 0.6f) return AIRecommendationPriority.Medium;
            return AIRecommendationPriority.Low;
        }
        
        private void UpdateActiveRecommendations(List<AIRecommendation> newRecommendations)
        {
            // Remove expired recommendations
            var now = DateTime.UtcNow;
            var expiredRecommendations = _activeRecommendations.Where(r => r.ExpiresAt < now).ToList();
            
            foreach (var expired in expiredRecommendations)
            {
                expired.Status = RecommendationStatus.Expired;
                _activeRecommendations.Remove(expired);
                OnRecommendationExpired?.Invoke(expired);
            }
            
            // Add new recommendations
            foreach (var recommendation in newRecommendations)
            {
                if (_activeRecommendations.Count < _maxActiveRecommendations)
                {
                    _activeRecommendations.Add(recommendation);
                }
                else
                {
                    _pendingRecommendations.Enqueue(recommendation);
                }
            }
            
            LogInfo($"Active recommendations updated: {_activeRecommendations.Count} active, {_pendingRecommendations.Count} pending");
        }
        
        private void UpdateStatistics(List<AIRecommendation> recommendations)
        {
            Statistics.TotalGenerated += recommendations.Count;
            Statistics.LastGenerationTime = DateTime.UtcNow;
            Statistics.AverageConfidence = recommendations.Count > 0 ? 
                recommendations.Average(r => r.ConfidenceScore) : 0f;
            Statistics.AverageImpact = recommendations.Count > 0 ? 
                recommendations.Average(r => r.ImpactScore) : 0f;
        }
        
        private void TrackRecommendationImplementation(AIRecommendation recommendation)
        {
            var feedback = GetOrCreatePlayerFeedback(recommendation.Category);
            feedback.ImplementationCount++;
            feedback.LastInteraction = DateTime.UtcNow;
            
            // Positive feedback for implementations
            feedback.TotalRating += 4f;
            feedback.TotalFeedback++;
            feedback.AverageRating = feedback.TotalRating / feedback.TotalFeedback;
        }
        
        private void TrackRecommendationDismissal(AIRecommendation recommendation, string reason)
        {
            var feedback = GetOrCreatePlayerFeedback(recommendation.Category);
            feedback.DismissalCount++;
            feedback.LastInteraction = DateTime.UtcNow;
            
            // Negative feedback for dismissals
            feedback.TotalRating += 1f;
            feedback.TotalFeedback++;
            feedback.AverageRating = feedback.TotalRating / feedback.TotalFeedback;
            
            // Track frequently dismissed categories
            if (feedback.DismissalCount > 3 && feedback.AverageRating < 2f)
            {
                if (!_dismissedRecommendationTypes.Contains(recommendation.Category))
                {
                    _dismissedRecommendationTypes.Add(recommendation.Category);
                    LogInfo($"Category '{recommendation.Category}' added to dismissed types due to poor feedback");
                }
            }
        }
        
        private PlayerRecommendationFeedback GetOrCreatePlayerFeedback(string category)
        {
            if (!_playerFeedback.ContainsKey(category))
            {
                _playerFeedback[category] = new PlayerRecommendationFeedback
                {
                    Category = category,
                    AverageRating = 3f,
                    TotalFeedback = 0,
                    TotalRating = 0f
                };
            }
            return _playerFeedback[category];
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnCultivationAnalysisComplete(CultivationAnalysisResult result)
        {
            // Generate recommendations based on cultivation analysis
            if (_enableContextualRecommendations)
            {
                _ = GenerateRecommendationsAsync(result);
            }
        }
        
        private void OnEnvironmentalAnalysisComplete(AIEnvironmentalAnalysisResult result)
        {
            // React to environmental analysis completion
            LogInfo("Environmental analysis complete - triggering environmental recommendations");
        }
        
        private void OnGeneticsAnalysisComplete(GeneticsAnalysisResult result)
        {
            // React to genetics analysis completion
            LogInfo("Genetics analysis complete - triggering genetics recommendations");
        }
        
        private void OnCriticalInsight(DataInsight insight)
        {
            // Generate urgent recommendations based on critical insights
            if (insight.Severity == InsightSeverity.Critical)
            {
                var urgentRecommendation = CreateRecommendation(
                    $"Critical Alert: {insight.Title}",
                    insight.Description,
                    AIRecommendationPriority.Critical,
                    insight.Category
                );
                
                _activeRecommendations.Insert(0, urgentRecommendation);
                OnRecommendationCreated?.Invoke(urgentRecommendation);
            }
        }
        
        private void OnOptimizationIdentified(OptimizationOpportunity opportunity)
        {
            // Generate recommendations based on optimization opportunities
            if (opportunity.IsActive && opportunity.BenefitScore > 0.6f)
            {
                var optimizationRecommendation = CreateRecommendation(
                    opportunity.Title,
                    opportunity.Description,
                    ConvertOptimizationPriorityToRecommendationPriority(opportunity.BenefitScore),
                    opportunity.Category ?? "Optimization"
                );
                
                _pendingRecommendations.Enqueue(optimizationRecommendation);
            }
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Update()
        {
            if (!IsInitialized) return;
            
            // Check for scheduled recommendation generation
            if (Time.time >= _nextScheduledGeneration && !_isGeneratingRecommendations)
            {
                // Trigger periodic recommendation generation
                if (_analysisService is IAIAnalysisServiceExtended extendedService && extendedService.LastCultivationAnalysis != null)
                {
                    _ = GenerateRecommendationsAsync(extendedService.LastCultivationAnalysis);
                }
            }
            
            // Process pending recommendations
            ProcessPendingRecommendations();
        }
        
        private void ProcessPendingRecommendations()
        {
            while (_pendingRecommendations.Count > 0 && _activeRecommendations.Count < _maxActiveRecommendations)
            {
                var recommendation = _pendingRecommendations.Dequeue();
                _activeRecommendations.Add(recommendation);
                OnRecommendationCreated?.Invoke(recommendation);
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        private void SaveRecommendationData()
        {
            // Save recommendation history and player feedback for persistence
            LogInfo("Saving recommendation data...");
            // Implementation would save to persistent storage
        }
        
        #endregion
        
        #region Logging
        
        private void LogInfo(string message)
        {
            Debug.Log($"[AIRecommendationService] {message}");
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[AIRecommendationService] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[AIRecommendationService] {message}");
        }
        
        #endregion
    }
    
    // Supporting data structures
    [System.Serializable]
    public class RecommendationSettings
    {
        public float GenerationInterval = 60f;
        public int MaxActiveRecommendations = 20;
        public int MaxRecommendationsPerSession = 5;
        public float ConfidenceThreshold = 0.6f;
        public bool EnableSmartFiltering = true;
    }
    
    [System.Serializable]
    public class RecommendationStatistics
    {
        public int TotalGenerated;
        public int TotalImplemented;
        public int TotalDismissed;
        public float AverageConfidence;
        public float AverageImpact;
        public DateTime LastGenerationTime;
    }
    
    [System.Serializable]
    public class PlayerRecommendationFeedback
    {
        public string Category;
        public float AverageRating;
        public int TotalFeedback;
        public float TotalRating;
        public int ImplementationCount;
        public int DismissalCount;
        public DateTime LastInteraction;
    }
}