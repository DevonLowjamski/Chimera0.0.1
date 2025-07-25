using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Tutorial;

namespace ProjectChimera.Systems.Tutorial
{
    /// <summary>
    /// Economics tutorial manager for Project Chimera.
    /// Manages business, financial, and market systems education.
    /// </summary>
    public class EconomicsTutorialManager : ChimeraManager
    {
        [Header("Economics Tutorial Configuration")]
        [SerializeField] private TutorialSequenceSO _economicsTutorialSequence;
        [SerializeField] private List<EconomicsTutorialModule> _tutorialModules;
        [SerializeField] private bool _autoStartAfterGenetics = true;
        
        [Header("Tutorial Prerequisites")]
        [SerializeField] private int _minimumPlayerLevel = 8;
        [SerializeField] private int _minimumBusinessExperience = 50; // Points from sales, etc.
        [SerializeField] private float _minimumRevenue = 10000f;
        [SerializeField] private List<string> _requiredUnlocks;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onEconomicsTutorialStarted;
        [SerializeField] private SimpleGameEventSO _onEconomicsTutorialCompleted;
        [SerializeField] private StringGameEventSO _onEconomicsModuleCompleted;
        
        // State
        private bool _isEconomicsTutorialActive;
        private bool _isEconomicsTutorialCompleted;
        private EconomicsTutorialModule _currentModule;
        private int _currentModuleIndex;
        private string _currentStepId;
        
        // Managers
        private TutorialManager _tutorialManager;
        // private MarketManager _marketManager; // Removed to prevent circular dependency
        
        // Tutorial tracking
        private Dictionary<string, bool> _moduleCompletionStatus;
        private Dictionary<string, float> _moduleProgressTracking;
        private Dictionary<string, int> _moduleAttemptCounts;
        private Dictionary<string, float> _modulePerformanceScores;
        
        // Properties
        public bool IsEconomicsTutorialActive => _isEconomicsTutorialActive;
        public bool IsEconomicsTutorialCompleted => _isEconomicsTutorialCompleted;
        public EconomicsTutorialModule CurrentModule => _currentModule;
        public int CurrentModuleIndex => _currentModuleIndex;
        public string CurrentStepId => _currentStepId;
        
        protected override void OnManagerInitialize()
        {
            // Get required managers
            _tutorialManager = GameManager.Instance.GetManager<TutorialManager>();
            // _marketManager = GameManager.Instance.GetManager<MarketManager>(); // Removed to prevent circular dependency
            
            // Initialize tracking dictionaries
            _moduleCompletionStatus = new Dictionary<string, bool>();
            _moduleProgressTracking = new Dictionary<string, float>();
            _moduleAttemptCounts = new Dictionary<string, int>();
            _modulePerformanceScores = new Dictionary<string, float>();
            
            // Initialize tutorial modules
            InitializeTutorialModules();
            
            // Check if tutorial should auto-start
            CheckTutorialPrerequisites();
            
            // Subscribe to events
            SubscribeToEvents();
            
            Debug.Log("Economics tutorial manager initialized");
        }

        protected override void OnManagerShutdown()
        {
            // Unsubscribe from events
            // Clean up any active tutorials
            
            Debug.Log("Economics tutorial manager shutdown");
        }
        
        /// <summary>
        /// Initialize tutorial modules
        /// </summary>
        private void InitializeTutorialModules()
        {
            if (_tutorialModules == null)
            {
                _tutorialModules = new List<EconomicsTutorialModule>();
            }
            
            // Create default modules if none exist
            if (_tutorialModules.Count == 0)
            {
                CreateDefaultTutorialModules();
            }
            
            // Initialize completion tracking
            foreach (var module in _tutorialModules)
            {
                _moduleCompletionStatus[module.ModuleId] = false;
                _moduleProgressTracking[module.ModuleId] = 0f;
                _moduleAttemptCounts[module.ModuleId] = 0;
                _modulePerformanceScores[module.ModuleId] = 0f;
            }
        }
        
        /// <summary>
        /// Create default tutorial modules
        /// </summary>
        private void CreateDefaultTutorialModules()
        {
            _tutorialModules.AddRange(new List<EconomicsTutorialModule>
            {
                new EconomicsTutorialModule
                {
                    ModuleId = "business_fundamentals",
                    ModuleName = "Cannabis Business Fundamentals",
                    Description = "Learn the basics of cannabis business operations, legal considerations, and industry structure.",
                    EstimatedDuration = 25f,
                    DifficultyLevel = TutorialDifficultyLevel.Beginner,
                    Prerequisites = new List<string> { "genetics_tutorial_completed" },
                    LearningObjectives = new List<string>
                    {
                        "Understand cannabis industry structure and regulations",
                        "Learn basic business terminology and concepts",
                        "Master cost calculation and profit margin analysis",
                        "Understand licensing and compliance requirements",
                        "Learn market segmentation and customer types"
                    },
                    BusinessSkills = new List<string> { "Legal Compliance", "Cost Analysis", "Market Research" }
                },
                
                new EconomicsTutorialModule
                {
                    ModuleId = "financial_management",
                    ModuleName = "Financial Management & Accounting",
                    Description = "Master financial planning, cash flow management, and accounting principles for cannabis operations.",
                    EstimatedDuration = 30f,
                    DifficultyLevel = TutorialDifficultyLevel.Intermediate,
                    Prerequisites = new List<string> { "business_fundamentals" },
                    LearningObjectives = new List<string>
                    {
                        "Learn financial statement preparation and analysis",
                        "Master cash flow forecasting and management",
                        "Understand budgeting and variance analysis",
                        "Learn investment evaluation methods (ROI, NPV, IRR)",
                        "Master tax planning and compliance strategies"
                    },
                    BusinessSkills = new List<string> { "Financial Analysis", "Cash Flow Management", "Tax Planning" }
                },
                
                new EconomicsTutorialModule
                {
                    ModuleId = "market_analysis",
                    ModuleName = "Market Analysis & Competitive Intelligence",
                    Description = "Learn market research, competitive analysis, and pricing strategies for cannabis products.",
                    EstimatedDuration = 28f,
                    DifficultyLevel = TutorialDifficultyLevel.Intermediate,
                    Prerequisites = new List<string> { "business_fundamentals" },
                    LearningObjectives = new List<string>
                    {
                        "Conduct comprehensive market research",
                        "Analyze competitor strategies and positioning",
                        "Master pricing strategies and elasticity analysis",
                        "Learn consumer behavior and segmentation",
                        "Understand supply and demand dynamics"
                    },
                    BusinessSkills = new List<string> { "Market Research", "Competitive Analysis", "Pricing Strategy" }
                },
                
                new EconomicsTutorialModule
                {
                    ModuleId = "supply_chain_management",
                    ModuleName = "Supply Chain & Operations Management",
                    Description = "Optimize supply chain operations, inventory management, and distribution networks.",
                    EstimatedDuration = 32f,
                    DifficultyLevel = TutorialDifficultyLevel.Advanced,
                    Prerequisites = new List<string> { "financial_management", "market_analysis" },
                    LearningObjectives = new List<string>
                    {
                        "Design efficient supply chain networks",
                        "Master inventory optimization techniques",
                        "Learn quality control and compliance systems",
                        "Understand logistics and distribution strategies",
                        "Master vendor relationship management"
                    },
                    BusinessSkills = new List<string> { "Supply Chain", "Inventory Management", "Quality Control" }
                },
                
                new EconomicsTutorialModule
                {
                    ModuleId = "investment_strategy",
                    ModuleName = "Investment Strategy & Capital Management",
                    Description = "Learn advanced investment strategies, capital allocation, and growth financing for cannabis businesses.",
                    EstimatedDuration = 35f,
                    DifficultyLevel = TutorialDifficultyLevel.Advanced,
                    Prerequisites = new List<string> { "financial_management" },
                    LearningObjectives = new List<string>
                    {
                        "Master capital budgeting and allocation strategies",
                        "Learn equity and debt financing options",
                        "Understand risk management and hedging strategies",
                        "Master portfolio theory for diversified operations",
                        "Learn exit strategies and business valuation"
                    },
                    BusinessSkills = new List<string> { "Capital Allocation", "Risk Management", "Business Valuation" }
                },
                
                new EconomicsTutorialModule
                {
                    ModuleId = "advanced_analytics",
                    ModuleName = "Advanced Business Analytics & Optimization",
                    Description = "Master data analytics, predictive modeling, and optimization techniques for cannabis operations.",
                    EstimatedDuration = 40f,
                    DifficultyLevel = TutorialDifficultyLevel.Expert,
                    Prerequisites = new List<string> { "supply_chain_management", "investment_strategy" },
                    LearningObjectives = new List<string>
                    {
                        "Master statistical analysis and predictive modeling",
                        "Learn machine learning applications in cannabis business",
                        "Understand optimization algorithms for operations",
                        "Master KPI development and dashboard design",
                        "Learn advanced forecasting techniques"
                    },
                    BusinessSkills = new List<string> { "Data Analytics", "Predictive Modeling", "Operations Research" }
                }
            });
        }
        
        /// <summary>
        /// Check tutorial prerequisites
        /// </summary>
        private void CheckTutorialPrerequisites()
        {
            if (!_autoStartAfterGenetics)
                return;
            
            // Check if genetics tutorial is completed
            var geneticsCompleted = PlayerPrefs.GetInt("GeneticsTutorialCompleted", 0) == 1;
            if (!geneticsCompleted)
                return;
            
            // Check player level
            var playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
            if (playerLevel < _minimumPlayerLevel)
                return;
            
            // Check business experience
            var businessExp = PlayerPrefs.GetInt("BusinessExperience", 0);
            if (businessExp < _minimumBusinessExperience)
                return;
            
            // Check revenue threshold
            var totalRevenue = PlayerPrefs.GetFloat("TotalRevenue", 0f);
            if (totalRevenue < _minimumRevenue)
                return;
            
            // Check required unlocks
            foreach (var unlock in _requiredUnlocks)
            {
                if (PlayerPrefs.GetInt($"Unlocked_{unlock}", 0) == 0)
                    return;
            }
            
            // All prerequisites met
            Debug.Log("Economics tutorial prerequisites met - ready to start");
        }
        
        /// <summary>
        /// Subscribe to events
        /// </summary>
        private void SubscribeToEvents()
        {
            // Subscribe to static events from TutorialManager
            TutorialManager.OnStepCompleted += HandleTutorialStepCompleted;
            TutorialManager.OnSequenceCompleted += HandleTutorialSequenceCompleted;
        }
        
        /// <summary>
        /// Start economics tutorial
        /// </summary>
        public bool StartEconomicsTutorial()
        {
            if (_isEconomicsTutorialActive || _isEconomicsTutorialCompleted)
                return false;
            
            // Check prerequisites one more time
            if (!ArePrerequisitesMet())
            {
                Debug.LogWarning("Prerequisites not met for economics tutorial");
                return false;
            }
            
            _isEconomicsTutorialActive = true;
            _currentModuleIndex = 0;
            
            // Start first module
            StartTutorialModule(0);
            
            _onEconomicsTutorialStarted?.Raise();
            
            Debug.Log("Started economics tutorial");
            return true;
        }
        
        /// <summary>
        /// Start specific tutorial module
        /// </summary>
        public bool StartTutorialModule(int moduleIndex)
        {
            if (moduleIndex < 0 || moduleIndex >= _tutorialModules.Count)
                return false;
            
            var module = _tutorialModules[moduleIndex];
            
            // Check module prerequisites
            if (!AreModulePrerequisitesMet(module))
            {
                Debug.LogWarning($"Prerequisites not met for module: {module.ModuleId}");
                return false;
            }
            
            _currentModule = module;
            _currentModuleIndex = moduleIndex;
            _moduleAttemptCounts[module.ModuleId]++;
            
            // Start first step of module
            StartModuleFirstStep(module);
            
            Debug.Log($"Started economics tutorial module: {module.ModuleName}");
            return true;
        }
        
        /// <summary>
        /// Start module first step
        /// </summary>
        private void StartModuleFirstStep(EconomicsTutorialModule module)
        {
            switch (module.ModuleId)
            {
                case "business_fundamentals":
                    StartBusinessFundamentalsModule();
                    break;
                    
                case "financial_management":
                    StartFinancialManagementModule();
                    break;
                    
                case "market_analysis":
                    StartMarketAnalysisModule();
                    break;
                    
                case "supply_chain_management":
                    StartSupplyChainModule();
                    break;
                    
                case "investment_strategy":
                    StartInvestmentStrategyModule();
                    break;
                    
                case "advanced_analytics":
                    StartAdvancedAnalyticsModule();
                    break;
                    
                default:
                    Debug.LogWarning($"Unknown economics module: {module.ModuleId}");
                    break;
            }
        }
        
        /// <summary>
        /// Start business fundamentals module
        /// </summary>
        private void StartBusinessFundamentalsModule()
        {
            var introStep = CreateBusinessFundamentalsIntroStep();
            _currentStepId = introStep.StepId;
            
            if (_tutorialManager != null)
            {
                _tutorialManager.StartTutorialStep(introStep);
            }
        }
        
        /// <summary>
        /// Create business fundamentals intro step
        /// </summary>
        private TutorialStepSO CreateBusinessFundamentalsIntroStep()
        {
            var stepSO = ScriptableObject.CreateInstance<TutorialStepSO>();
            stepSO.SetStepId("business_fundamentals_intro");
            stepSO.SetTitle("Cannabis Business Fundamentals");
            stepSO.SetShortDescription("Learn the foundation of cannabis business operations and industry dynamics.");
            stepSO.InstructionText = "The cannabis industry is unique with specific regulations, market dynamics, and business models. Understanding these fundamentals is crucial for success.";
            stepSO.SetStepType(TutorialStepType.Introduction);
            stepSO.SetValidationType(TutorialValidationType.ButtonClick);
            stepSO.SetTimeoutDuration(0f);
            stepSO.SetCanSkip(false);
            
            return stepSO;
        }
        
        /// <summary>
        /// Start financial management module
        /// </summary>
        private void StartFinancialManagementModule()
        {
            var introStep = CreateFinancialManagementIntroStep();
            _currentStepId = introStep.StepId;
            
            if (_tutorialManager != null)
            {
                _tutorialManager.StartTutorialStep(introStep);
            }
        }
        
        /// <summary>
        /// Create financial management intro step
        /// </summary>
        private TutorialStepSO CreateFinancialManagementIntroStep()
        {
            var stepSO = ScriptableObject.CreateInstance<TutorialStepSO>();
            stepSO.SetStepId("financial_management_intro");
            stepSO.SetTitle("Financial Management & Accounting");
            stepSO.SetShortDescription("Master financial planning and cash flow management for cannabis operations.");
            stepSO.InstructionText = "Sound financial management is critical in the cannabis industry due to banking restrictions and regulatory requirements. Learn professional financial practices.";
            stepSO.SetStepType(TutorialStepType.Introduction);
            stepSO.SetValidationType(TutorialValidationType.ButtonClick);
            stepSO.SetTimeoutDuration(0f);
            stepSO.SetCanSkip(false);
            
            return stepSO;
        }
        
        /// <summary>
        /// Start market analysis module
        /// </summary>
        private void StartMarketAnalysisModule()
        {
            var introStep = CreateMarketAnalysisIntroStep();
            _currentStepId = introStep.StepId;
            
            if (_tutorialManager != null)
            {
                _tutorialManager.StartTutorialStep(introStep);
            }
        }
        
        /// <summary>
        /// Create market analysis intro step
        /// </summary>
        private TutorialStepSO CreateMarketAnalysisIntroStep()
        {
            var stepSO = ScriptableObject.CreateInstance<TutorialStepSO>();
            stepSO.SetStepId("market_analysis_intro");
            stepSO.SetTitle("Market Analysis & Competitive Intelligence");
            stepSO.SetShortDescription("Learn market research and competitive analysis for strategic positioning.");
            stepSO.InstructionText = "Understanding your market and competition is essential for strategic positioning and pricing. Master research techniques and analytical frameworks.";
            stepSO.SetStepType(TutorialStepType.Introduction);
            stepSO.SetValidationType(TutorialValidationType.ButtonClick);
            stepSO.SetTimeoutDuration(0f);
            stepSO.SetCanSkip(false);
            
            return stepSO;
        }
        
        /// <summary>
        /// Start supply chain module
        /// </summary>
        private void StartSupplyChainModule()
        {
            var introStep = CreateSupplyChainIntroStep();
            _currentStepId = introStep.StepId;
            
            if (_tutorialManager != null)
            {
                _tutorialManager.StartTutorialStep(introStep);
            }
        }
        
        /// <summary>
        /// Create supply chain intro step
        /// </summary>
        private TutorialStepSO CreateSupplyChainIntroStep()
        {
            var stepSO = ScriptableObject.CreateInstance<TutorialStepSO>();
            stepSO.SetStepId("supply_chain_intro");
            stepSO.SetTitle("Supply Chain & Operations Management");
            stepSO.SetShortDescription("Optimize operations and supply chain efficiency for maximum profitability.");
            stepSO.InstructionText = "Efficient supply chain management reduces costs and improves quality. Learn optimization techniques and operational best practices.";
            stepSO.SetStepType(TutorialStepType.Introduction);
            stepSO.SetValidationType(TutorialValidationType.ButtonClick);
            stepSO.SetTimeoutDuration(0f);
            stepSO.SetCanSkip(false);
            
            return stepSO;
        }
        
        /// <summary>
        /// Start investment strategy module
        /// </summary>
        private void StartInvestmentStrategyModule()
        {
            var introStep = CreateInvestmentStrategyIntroStep();
            _currentStepId = introStep.StepId;
            
            if (_tutorialManager != null)
            {
                _tutorialManager.StartTutorialStep(introStep);
            }
        }
        
        /// <summary>
        /// Create investment strategy intro step
        /// </summary>
        private TutorialStepSO CreateInvestmentStrategyIntroStep()
        {
            var stepSO = ScriptableObject.CreateInstance<TutorialStepSO>();
            stepSO.SetStepId("investment_strategy_intro");
            stepSO.SetTitle("Investment Strategy & Capital Management");
            stepSO.SetShortDescription("Master advanced investment strategies and capital allocation decisions.");
            stepSO.InstructionText = "Strategic capital allocation and investment decisions determine long-term success. Learn professional investment evaluation and risk management techniques.";
            stepSO.SetStepType(TutorialStepType.Introduction);
            stepSO.SetValidationType(TutorialValidationType.ButtonClick);
            stepSO.SetTimeoutDuration(0f);
            stepSO.SetCanSkip(false);
            
            return stepSO;
        }
        
        /// <summary>
        /// Start advanced analytics module
        /// </summary>
        private void StartAdvancedAnalyticsModule()
        {
            var introStep = CreateAdvancedAnalyticsIntroStep();
            _currentStepId = introStep.StepId;
            
            if (_tutorialManager != null)
            {
                _tutorialManager.StartTutorialStep(introStep);
            }
        }
        
        /// <summary>
        /// Create advanced analytics intro step
        /// </summary>
        private TutorialStepSO CreateAdvancedAnalyticsIntroStep()
        {
            var stepSO = ScriptableObject.CreateInstance<TutorialStepSO>();
            stepSO.SetStepId("advanced_analytics_intro");
            stepSO.SetTitle("Advanced Business Analytics & Optimization");
            stepSO.SetShortDescription("Master data analytics and optimization for competitive advantage.");
            stepSO.InstructionText = "Advanced analytics provide competitive advantages through data-driven decision making. Learn statistical modeling and optimization techniques.";
            stepSO.SetStepType(TutorialStepType.Introduction);
            stepSO.SetValidationType(TutorialValidationType.ButtonClick);
            stepSO.SetTimeoutDuration(0f);
            stepSO.SetCanSkip(false);
            
            return stepSO;
        }
        
        /// <summary>
        /// Check if prerequisites are met
        /// </summary>
        private bool ArePrerequisitesMet()
        {
            // Check genetics tutorial completion
            if (PlayerPrefs.GetInt("GeneticsTutorialCompleted", 0) == 0)
                return false;
            
            // Check player level
            var playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
            if (playerLevel < _minimumPlayerLevel)
                return false;
            
            // Check business experience
            var businessExp = PlayerPrefs.GetInt("BusinessExperience", 0);
            if (businessExp < _minimumBusinessExperience)
                return false;
            
            // Check revenue threshold
            var totalRevenue = PlayerPrefs.GetFloat("TotalRevenue", 0f);
            if (totalRevenue < _minimumRevenue)
                return false;
            
            // Check required unlocks
            foreach (var unlock in _requiredUnlocks)
            {
                if (PlayerPrefs.GetInt($"Unlocked_{unlock}", 0) == 0)
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Check if module prerequisites are met
        /// </summary>
        private bool AreModulePrerequisitesMet(EconomicsTutorialModule module)
        {
            foreach (var prerequisite in module.Prerequisites)
            {
                if (prerequisite == "genetics_tutorial_completed")
                {
                    if (PlayerPrefs.GetInt("GeneticsTutorialCompleted", 0) == 0)
                        return false;
                }
                else if (_moduleCompletionStatus.ContainsKey(prerequisite))
                {
                    if (!_moduleCompletionStatus[prerequisite])
                        return false;
                }
                else
                {
                    // Check if it's a general unlock
                    if (PlayerPrefs.GetInt($"Unlocked_{prerequisite}", 0) == 0)
                        return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Handle tutorial step completed
        /// </summary>
        private void HandleTutorialStepCompleted(TutorialStepSO step)
        {
            if (step == null) return;
            
            string stepId = step.StepId;
            
            if (!_isEconomicsTutorialActive || stepId != _currentStepId)
                return;
            
            // Progress current module
            ProgressCurrentModule();
        }
        
        /// <summary>
        /// Progress current module
        /// </summary>
        private void ProgressCurrentModule()
        {
            if (string.IsNullOrEmpty(_currentModule.ModuleId))
                return;

            _currentStepId = "";
            
            // Update progress tracking
            var stepCount = GetModuleStepCount(_currentModule);
            var currentProgress = _moduleProgressTracking[_currentModule.ModuleId];
            _moduleProgressTracking[_currentModule.ModuleId] = Mathf.Min(currentProgress + 1f / stepCount, 1f);
            
            // Check if module is complete
            if (_moduleProgressTracking[_currentModule.ModuleId] >= 1f)
            {
                CompleteCurrentModule();
            }
            else
            {
                ContinueModuleNextStep();
            }
        }
        
        /// <summary>
        /// Get module step count
        /// </summary>
        private int GetModuleStepCount(EconomicsTutorialModule module)
        {
            // This would normally be determined by the actual step definitions
            // For now, using estimated values based on module complexity
            switch (module.ModuleId)
            {
                case "business_fundamentals":
                    return 12;
                case "financial_management":
                    return 15;
                case "market_analysis":
                    return 13;
                case "supply_chain_management":
                    return 16;
                case "investment_strategy":
                    return 17;
                case "advanced_analytics":
                    return 19;
                default:
                    return 12;
            }
        }
        
        /// <summary>
        /// Continue module next step
        /// </summary>
        private void ContinueModuleNextStep()
        {
            // This would load the next step definition for the current module
            // For now, we'll use a placeholder approach
            Debug.Log($"Continuing to next step in economics module: {_currentModule.ModuleId}");
        }
        
        /// <summary>
        /// Complete current module
        /// </summary>
        private void CompleteCurrentModule()
        {
            if (string.IsNullOrEmpty(_currentModule.ModuleId))
                return;

            // Mark module as completed
            _moduleCompletionStatus[_currentModule.ModuleId] = true;
            _moduleProgressTracking[_currentModule.ModuleId] = 1f;
            
            // Calculate performance score (placeholder - would be based on actual performance)
            var performanceScore = CalculateModulePerformanceScore(_currentModule);
            _modulePerformanceScores[_currentModule.ModuleId] = performanceScore;
            
            // Give completion rewards
            GiveModuleCompletionRewards(_currentModule, performanceScore);
            
            // Fire completion event
            _onEconomicsModuleCompleted?.Raise(_currentModule.ModuleId);
            
            Debug.Log($"Completed economics tutorial module: {_currentModule.ModuleName} (Score: {performanceScore:F1})");
            
            // Check if all modules are complete
            if (AreAllModulesComplete())
            {
                // Complete entire tutorial
                CompleteEconomicsTutorial();
            }
            else
            {
                // Move to next module
                MoveToNextModule();
            }
        }
        
        /// <summary>
        /// Calculate module performance score
        /// </summary>
        private float CalculateModulePerformanceScore(EconomicsTutorialModule module)
        {
            // Placeholder calculation - in full implementation would consider:
            // - Speed of completion
            // - Number of attempts
            // - Quiz/assessment scores
            // - Practical exercise performance
            
            var baseScore = 85f;
            var attemptPenalty = (_moduleAttemptCounts[module.ModuleId] - 1) * 5f;
            var difficultyBonus = GetDifficultyBonus(module.DifficultyLevel);
            
            return Mathf.Clamp(baseScore - attemptPenalty + difficultyBonus, 60f, 100f);
        }
        
        /// <summary>
        /// Get difficulty bonus
        /// </summary>
        private float GetDifficultyBonus(TutorialDifficultyLevel difficulty)
        {
            switch (difficulty)
            {
                case TutorialDifficultyLevel.Beginner:
                    return 0f;
                case TutorialDifficultyLevel.Intermediate:
                    return 5f;
                case TutorialDifficultyLevel.Advanced:
                    return 10f;
                case TutorialDifficultyLevel.Expert:
                    return 15f;
                default:
                    return 0f;
            }
        }
        
        /// <summary>
        /// Give module completion rewards
        /// </summary>
        private void GiveModuleCompletionRewards(EconomicsTutorialModule module, float performanceScore)
        {
            // Base skill points based on difficulty
            var baseSkillPoints = GetModuleSkillPointReward(module.DifficultyLevel);
            
            // Performance bonus
            var performanceMultiplier = performanceScore >= 90f ? 1.5f : performanceScore >= 80f ? 1.2f : 1.0f;
            var totalSkillPoints = Mathf.RoundToInt(baseSkillPoints * performanceMultiplier);
            
            var currentSkillPoints = PlayerPrefs.GetInt("SkillPoints", 0);
            PlayerPrefs.SetInt("SkillPoints", currentSkillPoints + totalSkillPoints);
            
            // Business experience points
            var businessExp = GetBusinessExperienceReward(module.DifficultyLevel, performanceScore);
            var currentBusinessExp = PlayerPrefs.GetInt("BusinessExperience", 0);
            PlayerPrefs.SetInt("BusinessExperience", currentBusinessExp + businessExp);
            
            // Unlock module-specific features
            foreach (var skill in module.BusinessSkills)
            {
                PlayerPrefs.SetInt($"Unlocked_Business_{skill.Replace(" ", "")}", 1);
            }
            
            // Special unlocks for advanced modules
            if (module.ModuleId == "investment_strategy")
            {
                PlayerPrefs.SetInt("Unlocked_AdvancedFinance", 1);
                PlayerPrefs.SetInt("Unlocked_CapitalMarkets", 1);
            }
            else if (module.ModuleId == "advanced_analytics")
            {
                PlayerPrefs.SetInt("Unlocked_DataAnalytics", 1);
                PlayerPrefs.SetInt("Unlocked_PredictiveModeling", 1);
            }
            
            Debug.Log($"Gave {totalSkillPoints} skill points and {businessExp} business experience for {module.ModuleName}");
        }
        
        /// <summary>
        /// Get module skill point reward
        /// </summary>
        private int GetModuleSkillPointReward(TutorialDifficultyLevel difficulty)
        {
            switch (difficulty)
            {
                case TutorialDifficultyLevel.Beginner:
                    return 4;
                case TutorialDifficultyLevel.Intermediate:
                    return 6;
                case TutorialDifficultyLevel.Advanced:
                    return 9;
                case TutorialDifficultyLevel.Expert:
                    return 15;
                default:
                    return 4;
            }
        }
        
        /// <summary>
        /// Get business experience reward
        /// </summary>
        private int GetBusinessExperienceReward(TutorialDifficultyLevel difficulty, float performanceScore)
        {
            var baseExp = difficulty switch
            {
                TutorialDifficultyLevel.Beginner => 25,
                TutorialDifficultyLevel.Intermediate => 40,
                TutorialDifficultyLevel.Advanced => 60,
                TutorialDifficultyLevel.Expert => 100,
                _ => 25
            };
            
            return Mathf.RoundToInt(baseExp * (performanceScore / 85f));
        }
        
        /// <summary>
        /// Check if all modules are complete
        /// </summary>
        private bool AreAllModulesComplete()
        {
            foreach (var module in _tutorialModules)
            {
                if (!_moduleCompletionStatus[module.ModuleId])
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Move to next module
        /// </summary>
        private void MoveToNextModule()
        {
            _currentModuleIndex++;
            
            if (_currentModuleIndex < _tutorialModules.Count)
            {
                StartTutorialModule(_currentModuleIndex);
            }
        }
        
        /// <summary>
        /// Complete economics tutorial
        /// </summary>
        private void CompleteEconomicsTutorial()
        {
            _isEconomicsTutorialActive = false;
            _isEconomicsTutorialCompleted = true;
            
            // Save completion status
            PlayerPrefs.SetInt("EconomicsTutorialCompleted", 1);
            
            // Calculate overall performance
            var overallPerformance = CalculateOverallPerformance();
            
            // Give final completion rewards
            GiveFinalCompletionRewards(overallPerformance);
            
            _onEconomicsTutorialCompleted?.Raise();
            
            Debug.Log($"Completed entire economics tutorial with {overallPerformance:F1}% performance");
        }
        
        /// <summary>
        /// Calculate overall performance
        /// </summary>
        private float CalculateOverallPerformance()
        {
            var totalScore = 0f;
            var moduleCount = 0;
            
            foreach (var module in _tutorialModules)
            {
                if (_modulePerformanceScores.ContainsKey(module.ModuleId))
                {
                    totalScore += _modulePerformanceScores[module.ModuleId];
                    moduleCount++;
                }
            }
            
            return moduleCount > 0 ? totalScore / moduleCount : 0f;
        }
        
        /// <summary>
        /// Give final completion rewards
        /// </summary>
        private void GiveFinalCompletionRewards(float overallPerformance)
        {
            // Bonus skill points based on overall performance
            var bonusSkillPoints = overallPerformance >= 95f ? 25 : overallPerformance >= 90f ? 20 : overallPerformance >= 85f ? 15 : 10;
            var currentSkillPoints = PlayerPrefs.GetInt("SkillPoints", 0);
            PlayerPrefs.SetInt("SkillPoints", currentSkillPoints + bonusSkillPoints);
            
            // Major business experience bonus
            var bonusBusinessExp = Mathf.RoundToInt(200 * (overallPerformance / 85f));
            var currentBusinessExp = PlayerPrefs.GetInt("BusinessExperience", 0);
            PlayerPrefs.SetInt("BusinessExperience", currentBusinessExp + bonusBusinessExp);
            
            // Unlock master business features
            PlayerPrefs.SetInt("Unlocked_BusinessMaster", 1);
            PlayerPrefs.SetInt("Unlocked_AdvancedAnalytics", 1);
            PlayerPrefs.SetInt("Unlocked_StrategicPlanning", 1);
            PlayerPrefs.SetInt("Unlocked_ExecutiveDashboard", 1);
            
            // Performance-based unlocks
            if (overallPerformance >= 90f)
            {
                PlayerPrefs.SetInt("Unlocked_ConsultingMode", 1);
                PlayerPrefs.SetInt("Unlocked_InvestorAccess", 1);
            }
            
            Debug.Log($"Gave final economics tutorial completion rewards: {bonusSkillPoints} skill points, {bonusBusinessExp} business experience");
        }
        
        /// <summary>
        /// Handle tutorial sequence completed
        /// </summary>
        private void HandleTutorialSequenceCompleted(TutorialSequenceSO sequence)
        {
            if (sequence == null) return;
            
            if (sequence.SequenceId == _economicsTutorialSequence?.SequenceId)
            {
                CompleteEconomicsTutorial();
            }
        }
        
        /// <summary>
        /// Get economics tutorial progress
        /// </summary>
        public EconomicsTutorialProgress GetTutorialProgress()
        {
            var totalModules = _tutorialModules.Count;
            var completedModules = 0;
            var totalProgress = 0f;
            var averagePerformance = 0f;
            
            foreach (var module in _tutorialModules)
            {
                if (_moduleCompletionStatus[module.ModuleId])
                    completedModules++;
                
                totalProgress += _moduleProgressTracking[module.ModuleId];
                
                if (_modulePerformanceScores.ContainsKey(module.ModuleId))
                    averagePerformance += _modulePerformanceScores[module.ModuleId];
            }
            
            if (completedModules > 0)
                averagePerformance /= completedModules;
            
            return new EconomicsTutorialProgress
            {
                IsActive = _isEconomicsTutorialActive,
                IsCompleted = _isEconomicsTutorialCompleted,
                CurrentModuleIndex = _currentModuleIndex,
                TotalModules = totalModules,
                CompletedModules = completedModules,
                OverallProgress = totalProgress / totalModules,
                AveragePerformance = averagePerformance,
                CurrentModuleId = string.IsNullOrEmpty(_currentModule.ModuleId) ? "" : _currentModule.ModuleId,
                CurrentStepId = _currentStepId
            };
        }
        
        protected override void OnDestroy()
        {
            // Unsubscribe from events
            if (_tutorialManager != null)
            {
                TutorialManager.OnStepCompleted -= HandleTutorialStepCompleted;
                TutorialManager.OnSequenceCompleted -= HandleTutorialSequenceCompleted;
            }
            
            base.OnDestroy();
        }
    }
    
    /// <summary>
    /// Economics tutorial module definition
    /// </summary>
    [System.Serializable]
    public struct EconomicsTutorialModule
    {
        public string ModuleId;
        public string ModuleName;
        public string Description;
        public float EstimatedDuration;
        public TutorialDifficultyLevel DifficultyLevel;
        public List<string> Prerequisites;
        public List<string> LearningObjectives;
        public List<string> BusinessSkills;
    }
    
    /// <summary>
    /// Economics tutorial progress information
    /// </summary>
    [System.Serializable]
    public struct EconomicsTutorialProgress
    {
        public bool IsActive;
        public bool IsCompleted;
        public int CurrentModuleIndex;
        public int TotalModules;
        public int CompletedModules;
        public float OverallProgress;
        public float AveragePerformance;
        public string CurrentModuleId;
        public string CurrentStepId;
    }
}