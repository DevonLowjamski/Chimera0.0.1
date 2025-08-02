using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.Progression;
using ProjectChimera.Data.Achievements;
using ProjectChimera.Data.Competition;
using ProjectChimera.Data.Research;
using ProjectChimera.Data.Economy;
using ProjectChimera.Systems.Registry;

// Type disambiguation aliases
using CompetitionType = ProjectChimera.Data.Competition.CompetitionType;
using Competition = ProjectChimera.Data.Competition.Competition;
using CompetitionRules = ProjectChimera.Data.Competition.CompetitionRules;
using CompetitionFormat = ProjectChimera.Data.Competition.CompetitionFormat;
using CompetitionStatus = ProjectChimera.Data.Competition.CompetitionStatus;
using MilestoneReward = ProjectChimera.Data.Progression.MilestoneReward;
using ProgressionAchievementReward = ProjectChimera.Data.Progression.AchievementReward;
using ResearchCategory = ProjectChimera.Data.Research.ResearchCategory;
using ResearchProject = ProjectChimera.Data.Research.ResearchProject;
using ResourceRequirements = ProjectChimera.Data.Research.ResourceRequirements;
using ResearchRequirements = ProjectChimera.Data.Research.ResearchRequirements;
using Achievement = ProjectChimera.Data.Achievements.Achievement;
using Discovery = ProjectChimera.Data.Research.Discovery;
using Innovation = ProjectChimera.Data.Research.Innovation;
using Breakthrough = ProjectChimera.Data.Research.Breakthrough;
using DiscoveryContext = ProjectChimera.Data.Research.DiscoveryContext;
using DiscoveryEvent = ProjectChimera.Data.Research.DiscoveryEvent;
using InnovationTrigger = ProjectChimera.Data.Research.InnovationTrigger;
using BreakthroughConditions = ProjectChimera.Data.Research.BreakthroughConditions;
using Technology = ProjectChimera.Data.Research.Technology;
using UnlockRequirements = ProjectChimera.Data.Research.UnlockRequirements;
using TechnologyPathAnalysis = ProjectChimera.Data.Research.TechnologyPathAnalysis;
using ResourceAllocation = ProjectChimera.Data.Research.ResourceAllocation;
using ResourceBudget = ProjectChimera.Data.Research.ResourceBudget;
using ResearchFacility = ProjectChimera.Data.Research.ResearchFacility;
using FacilityUpgrade = ProjectChimera.Data.Research.FacilityUpgrade;
using ResearchEquipment = ProjectChimera.Data.Research.ResearchEquipment;
using ResourceType = ProjectChimera.Data.Research.ResourceType;
using FacilityStatus = ProjectChimera.Data.Research.FacilityStatus;
using EquipmentStatus = ProjectChimera.Data.Research.EquipmentStatus;

// Trading-specific type aliases - Only for types that actually exist
using CompletedTransaction = ProjectChimera.Data.Economy.CompletedTransaction;
using PendingTransaction = ProjectChimera.Data.Economy.PendingTransaction;
using TransactionResult = ProjectChimera.Data.Economy.TransactionResult;
using TransactionStatus = ProjectChimera.Data.Economy.TransactionStatus;
using TradingTransactionType = ProjectChimera.Data.Economy.TradingTransactionType;
using PaymentMethod = ProjectChimera.Data.Economy.PaymentMethod;
using TradingPost = ProjectChimera.Data.Economy.TradingPost;
using TradingPostStatus = ProjectChimera.Data.Economy.TradingPostStatus;
using TradingPostType = ProjectChimera.Data.Economy.TradingPostType;
using OpportunityType = ProjectChimera.Data.Economy.OpportunityType;
using TradingOpportunity = ProjectChimera.Data.Economy.TradingOpportunity;
using CashTransferType = ProjectChimera.Data.Economy.CashTransferType;
using TradingProfitabilityAnalysis = ProjectChimera.Data.Economy.TradingProfitabilityAnalysis;
using TradingPerformanceMetrics = ProjectChimera.Data.Economy.TradingPerformanceMetrics;
using PlayerInventory = ProjectChimera.Data.Economy.PlayerInventory;
using InventoryItem = ProjectChimera.Data.Economy.InventoryItem;
using FinancialMetrics = ProjectChimera.Data.Economy.FinancialMetrics;
using TradingPostState = ProjectChimera.Data.Economy.TradingPostState;
using PlayerFinances = ProjectChimera.Data.Economy.PlayerFinances;
using LoanType = ProjectChimera.Data.Economy.LoanType;
using CreditRating = ProjectChimera.Data.Economy.CreditRating;

// Additional Economy type aliases for services
using MarketProductSO = ProjectChimera.Data.Economy.MarketProductSO;

// Only include aliases for types that actually exist and are needed
// Most progression types are already defined in existing files

namespace ProjectChimera.Systems.Registry
{
    /// <summary>
    /// Comprehensive interface definitions for all 150+ specialized services
    /// Part of Module 2: Manager Decomposition - Service-Oriented Architecture
    /// </summary>

    #region Competition Services (CannabisCupManager → 4 services)

    /// <summary>
    /// PC014-1a: Competition Management Service Interface
    /// Handles tournament creation, scheduling, and lifecycle management
    /// </summary>
    public interface ICompetitionManagementService : IService
    {
        // Tournament Management
        string CreateTournament(string name, CompetitionType type, DateTime startDate, DateTime endDate);
        bool ScheduleCompetition(string tournamentId, DateTime scheduledDate);
        CompetitionStatus GetCompetitionStatus(string competitionId);
        List<Competition> GetActiveCompetitions();
        List<Competition> GetUpcomingCompetitions();
        bool CancelCompetition(string competitionId, string reason);
        
        // Event Lifecycle
        void StartCompetition(string competitionId);
        void EndCompetition(string competitionId);
        bool IsCompetitionActive(string competitionId);
        TimeSpan GetTimeUntilCompetition(string competitionId);
        
        // Rules and Formats
        void SetCompetitionRules(string competitionId, CompetitionRules rules);
        CompetitionRules GetCompetitionRules(string competitionId);
        List<CompetitionFormat> GetAvailableFormats();
        
        // Events
        event Action<string> OnCompetitionCreated;
        event Action<string> OnCompetitionStarted;
        event Action<string> OnCompetitionEnded;
    }

    /// <summary>
    /// PC014-1b: Judging Evaluation Service Interface
    /// Manages scoring algorithms, judge assignment, and results calculation
    /// </summary>
    public interface IJudgingEvaluationService : IService
    {
        // Scoring System
        float CalculateScore(string plantId, JudgingCriteria criteria);
        ScoreBreakdown GetDetailedScore(string plantId, string judgeId);
        void SubmitJudgeScore(string judgeId, string plantId, JudgeScorecard scorecard);
        
        // Judge Management
        bool AssignJudge(string judgeId, string competitionId);
        bool ValidateJudge(string judgeId);
        List<Judge> GetAssignedJudges(string competitionId);
        JudgeQualificationLevel GetJudgeLevel(string judgeId);
        
        // Results Processing
        CompetitionResults CalculateResults(string competitionId);
        PlantRanking GetPlantRanking(string competitionId);
        WinnerSelection DetermineWinners(string competitionId);
        bool ValidateResults(string competitionId);
        
        // Events
        event Action<string, string> OnScoreSubmitted; // judgeId, plantId
        event Action<string> OnResultsCalculated; // competitionId
    }

    /// <summary>
    /// PC014-1c: Participant Registration Service Interface
    /// Handles contestant registration, validation, and communication
    /// </summary>
    public interface IParticipantRegistrationService : IService
    {
        // Registration Management
        string RegisterParticipant(string playerId, string competitionId, PlantSubmission submission);
        bool ValidateRegistration(string registrationId);
        List<ParticipantRegistration> GetRegistrations(string competitionId);
        ParticipantRegistration GetRegistration(string registrationId);
        bool CancelRegistration(string registrationId);
        
        // Entry Management
        bool SubmitEntry(string registrationId, PlantEntry entry);
        PlantEntry GetEntry(string registrationId);
        bool ValidateEntry(PlantEntry entry, CompetitionRules rules);
        EntryStatus GetEntryStatus(string registrationId);
        
        // Qualification System
        bool CheckQualification(string playerId, CompetitionRequirements requirements);
        QualificationResult ValidateQualification(string playerId, string competitionId);
        List<string> GetQualificationRequirements(string competitionId);
        
        // Communication
        void NotifyParticipant(string registrationId, string message);
        void BroadcastToParticipants(string competitionId, string message);
        
        // Events
        event Action<string> OnParticipantRegistered;
        event Action<string> OnEntrySubmitted;
    }

    /// <summary>
    /// PC014-1d: Competition Rewards Service Interface
    /// Manages prize distribution and achievement integration
    /// </summary>
    public interface ICompetitionRewardsService : IService
    {
        // Prize System
        void DistributePrizes(string competitionId, CompetitionResults results);
        Prize GetPrize(PlacementPosition position, CompetitionType type);
        List<Prize> GetAvailablePrizes(string competitionId);
        bool ClaimPrize(string winnerId, string prizeId);
        
        // Achievement Integration
        void ProcessCompetitionAchievements(string competitionId, CompetitionResults results);
        List<Achievement> GetCompetitionAchievements();
        bool UnlockAchievement(string playerId, string achievementId);
        
        // Winner Recognition
        void RecognizeWinner(string winnerId, string competitionId, PlacementPosition position);
        WinnerProfile CreateWinnerProfile(string winnerId, CompetitionResults results);
        List<WinnerProfile> GetHallOfFame();
        
        // Reward History
        List<RewardHistory> GetPlayerRewards(string playerId);
        RewardStatistics GetRewardStatistics(string playerId);
        
        // Events
        event Action<string, string> OnPrizeDistributed; // winnerId, prizeId
        event Action<string, string> OnAchievementUnlocked; // playerId, achievementId
    }

    #endregion

    #region Research Services (ResearchManager → 4 services)

    /// <summary>
    /// PC014-2a: Research Project Service Interface
    /// Individual project management and progress tracking
    /// </summary>
    public interface IResearchProjectService : IService
    {
        // Project Management
        string CreateProject(string name, ResearchCategory category, ResearchRequirements requirements);
        bool StartProject(string projectId);
        bool CompleteProject(string projectId);
        ResearchProject GetProject(string projectId);
        List<ResearchProject> GetActiveProjects();
        
        // Progress Tracking
        void UpdateProgress(string projectId, float progressDelta);
        float GetProgress(string projectId);
        TimeSpan GetEstimatedTimeToCompletion(string projectId);
        bool IsProjectComplete(string projectId);
        
        // Resource Management
        bool ValidateResources(string projectId);
        ResearchRequirements GetResourceRequirements(string projectId);
        void ConsumeResources(string projectId);
        
        // Events
        event Action<string> OnProjectStarted;
        event Action<string> OnProjectCompleted;
        event Action<string, float> OnProgressUpdated;
    }

    /// <summary>
    /// PC014-2b: Technology Tree Service Interface
    /// Technology dependency management and unlock progression
    /// </summary>
    public interface ITechnologyTreeService : IService
    {
        // Tree Navigation
        List<Technology> GetAvailableTechnologies();
        List<Technology> GetUnlockedTechnologies();
        List<Technology> GetDependencies(string technologyId);
        List<Technology> GetDependents(string technologyId);
        
        // Unlock System
        bool UnlockTechnology(string technologyId);
        bool IsTechnologyUnlocked(string technologyId);
        bool CanUnlockTechnology(string technologyId);
        UnlockRequirements GetUnlockRequirements(string technologyId);
        
        // Path Optimization
        List<string> FindOptimalPath(string targetTechnologyId);
        TechnologyPathAnalysis AnalyzePath(string targetTechnologyId);
        List<Technology> GetRecommendedTechnologies();
        
        // Events
        event Action<string> OnTechnologyUnlocked;
        event Action<List<string>> OnPathUpdated;
    }

    /// <summary>
    /// PC014-2c: Discovery System Service Interface
    /// New technology discovery and innovation events
    /// </summary>
    public interface IDiscoverySystemService : IService
    {
        // Discovery Mechanics
        Discovery TriggerDiscovery(DiscoveryContext context);
        bool ProcessDiscoveryEvent(DiscoveryEvent discoveryEvent);
        List<Discovery> GetRecentDiscoveries();
        Discovery GetDiscovery(string discoveryId);
        
        // Innovation System
        Innovation ProcessInnovation(InnovationTrigger trigger);
        List<Innovation> GetPlayerInnovations(string playerId);
        bool ValidateInnovation(Innovation innovation);
        
        // Breakthrough Mechanics
        Breakthrough ProcessBreakthrough(BreakthroughConditions conditions);
        List<Breakthrough> GetBreakthroughs();
        bool IsBreakthroughEligible(BreakthroughConditions conditions);
        
        // Events
        event Action<Discovery> OnDiscoveryMade;
        event Action<Innovation> OnInnovationAchieved;
        event Action<Breakthrough> OnBreakthroughOccurred;
    }

    /// <summary>
    /// PC014-2d: Research Resource Service Interface
    /// Resource allocation, budgeting, and facility management
    /// </summary>
    public interface IResearchResourceService : IService
    {
        // Resource Allocation
        bool AllocateResources(string projectId, ResourceAllocation allocation);
        ResourceAllocation GetResourceAllocation(string projectId);
        float GetAvailableResources(ResourceType resourceType);
        void UpdateResourceBudget(string projectId, ResourceBudget budget);
        
        // Resource Validation and Consumption
        bool ValidateResources(string projectId);
        bool ConsumeResources(string projectId, ResourceRequirements requirements);
        
        // Facility Management
        List<ResearchFacility> GetAvailableFacilities();
        bool ReserveFacility(string facilityId, string projectId, TimeSpan duration);
        FacilityStatus GetFacilityStatus(string facilityId);
        void UpgradeFacility(string facilityId, FacilityUpgrade upgrade);
        
        // Equipment Tracking
        List<ResearchEquipment> GetAvailableEquipment();
        bool AssignEquipment(string equipmentId, string projectId);
        EquipmentStatus GetEquipmentStatus(string equipmentId);
        void MaintenanceEquipment(string equipmentId);
        
        // Events
        event Action<string, ResourceAllocation> OnResourcesAllocated;
        event Action<string, string> OnFacilityReserved;
    }

    #endregion

    #region Progression Services (ComprehensiveProgressionManager → 5 services)

    /// <summary>
    /// PC014-3a: Experience Management Service Interface
    /// XP calculation, distribution, and level progression
    /// </summary>
    public interface IExperienceManagementService : IService
    {
        // Experience System
        void AwardExperience(string playerId, ExperienceSourceData source, float amount);
        float GetExperience(string playerId);
        int GetLevel(string playerId);
        int GetLevelFromExperience(float experience);
        float GetExperienceForLevel(int level);
        float GetExperienceToNextLevel(string playerId);
        
        // Level Progression
        bool CheckLevelUp(string playerId);
        LevelUpResult ProcessLevelUp(string playerId);
        List<LevelReward> GetLevelRewards(int level);
        
        // Experience Sources
        void RegisterExperienceSource(ExperienceSourceData source);
        List<ExperienceSourceData> GetExperienceSources();
        ExperienceMultiplier GetExperienceMultiplier(string playerId);
        
        // Events
        event Action<string, float> OnExperienceAwarded;
        event Action<string, int> OnLevelUp;
    }

    /// <summary>
    /// PC014-3b: Skill Tree Management Service Interface
    /// Skill point allocation and ability unlock management
    /// </summary>
    public interface ISkillTreeManagementService : IService
    {
        // Skill Points
        void AwardSkillPoints(string playerId, int points);
        int GetAvailableSkillPoints(string playerId);
        int GetTotalSkillPoints(string playerId);
        bool SpendSkillPoints(string playerId, string skillId, int points);
        
        // Skill Management
        bool UnlockSkill(string playerId, string skillId);
        bool IsSkillUnlocked(string playerId, string skillId);
        int GetSkillLevel(string playerId, string skillId);
        bool CanUnlockSkill(string playerId, string skillId);
        
        // Tree Navigation
        List<Skill> GetAvailableSkills(string playerId);
        List<Skill> GetUnlockedSkills(string playerId);
        List<Skill> GetSkillDependencies(string skillId);
        SkillPath FindOptimalSkillPath(string playerId, string targetSkillId);
        
        // Events
        event Action<string, string> OnSkillUnlocked;
        event Action<string, int> OnSkillPointsAwarded;
    }

    /// <summary>
    /// PC014-3c: Progression Achievement Service Interface
    /// Milestone tracking and achievement unlock logic
    /// </summary>
    public interface IProgressionAchievementService : IService
    {
        // Achievement Tracking
        void TrackAchievementProgress(string playerId, string achievementId, float progress);
        float GetAchievementProgress(string playerId, string achievementId);
        bool IsAchievementUnlocked(string playerId, string achievementId);
        List<Achievement> GetUnlockedAchievements(string playerId);
        
        // Milestone System
        void RegisterMilestone(Milestone milestone);
        bool CheckMilestone(string playerId, string milestoneId);
        List<Milestone> GetAchievedMilestones(string playerId);
        MilestoneReward GetMilestoneReward(string milestoneId);
        
        // Reward Distribution
        void DistributeAchievementReward(string playerId, string achievementId);
        List<ProgressionAchievementReward> GetPendingRewards(string playerId);
        void ClaimReward(string playerId, string rewardId);
        
        // Events
        event Action<string, string> OnAchievementUnlocked;
        event Action<string, string> OnMilestoneAchieved;
    }

    /// <summary>
    /// PC014-3d: Progression Analytics Service Interface
    /// Player progression data analysis and insights
    /// </summary>
    public interface IProgressionAnalyticsService : IService
    {
        // Analytics Collection
        void RecordProgressionEvent(string playerId, ProgressionEvent progressionEvent);
        ProgressionAnalytics GetPlayerAnalytics(string playerId);
        List<ProgressionMetric> GetProgressionMetrics(string playerId, TimeSpan timeRange);
        
        // Performance Insights
        ProgressionInsight GenerateInsights(string playerId);
        List<ProgressionRecommendation> GetRecommendations(string playerId);
        ProgressionEfficiency CalculateEfficiency(string playerId);
        
        // Comparative Analysis
        PlayerRanking GetPlayerRanking(string playerId);
        ProgressionComparison CompareWithPeers(string playerId);
        List<ProgressionBenchmark> GetBenchmarks();
        
        // Events
        event Action<string, ProgressionEvent> OnProgressionEventRecorded;
        event Action<string, ProgressionInsight> OnInsightGenerated;
    }

    /// <summary>
    /// PC014-3e: Milestone Tracking Service Interface
    /// Major milestone detection and long-term progression goals
    /// </summary>
    public interface IMilestoneTrackingService : IService
    {
        // Milestone Management
        void RegisterMilestone(string playerId, Milestone milestone);
        bool CheckMilestoneCompletion(string playerId, string milestoneId);
        List<Milestone> GetActiveMilestones(string playerId);
        List<Milestone> GetCompletedMilestones(string playerId);
        
        // Progress Tracking
        void UpdateMilestoneProgress(string playerId, string milestoneId, float progress);
        float GetMilestoneProgress(string playerId, string milestoneId);
        TimeSpan GetEstimatedTimeToCompletion(string playerId, string milestoneId);
        
        // Reward System
        void DistributeMilestoneReward(string playerId, string milestoneId);
        MilestoneReward GetMilestoneReward(string milestoneId);
        List<MilestoneReward> GetPendingRewards(string playerId);
        
        // Long-term Goals
        void SetLongTermGoal(string playerId, LongTermGoal goal);
        LongTermGoal GetLongTermGoal(string playerId);
        GoalProgress GetGoalProgress(string playerId);
        
        // Events
        event Action<string, string> OnMilestoneCompleted;
        event Action<string, string> OnMilestoneRewardDistributed;
    }

    #endregion

    #region Trading Services (TradingManager → 3 services)

    /// <summary>
    /// PC014-4a: Transaction Processing Service Interface
    /// Handles buy/sell transactions, payment processing, and transaction history
    /// </summary>
    public interface ITransactionProcessingService : IService
    {
        // Properties
        List<CompletedTransaction> TransactionHistory { get; }
        
        // Transaction Processing
        TransactionResult InitiateBuyTransaction(MarketProductSO product, float quantity, TradingPost tradingPost, PaymentMethod paymentMethod, string playerId);
        TransactionResult InitiateSellTransaction(InventoryItem inventoryItem, float quantity, TradingPost tradingPost, PaymentMethod paymentMethod, string playerId);
        bool CancelTransaction(string transactionId);
        TransactionStatus GetTransactionStatus(string transactionId);
        List<PendingTransaction> GetPendingTransactions(string playerId = null);
        
        // Payment Processing
        bool ProcessPayment(PendingTransaction transaction);
        List<PaymentMethod> GetAvailablePaymentMethods(string playerId);
        bool ValidatePaymentMethod(PaymentMethod paymentMethod, float transactionAmount);
        
        // Transaction History
        List<CompletedTransaction> GetTransactionHistory(string playerId = null, DateTime? startDate = null, DateTime? endDate = null);
        TradingPerformanceMetrics GetPerformanceMetrics(string playerId);
        
        // Events
        event Action<CompletedTransaction> OnTransactionCompleted;
        event Action<PendingTransaction> OnTransactionStarted;
        event Action<string, string> OnTransactionFailed;
        event Action<string> OnTransactionCancelled;
    }

    /// <summary>
    /// PC014-4b: Trading Post Management Service Interface
    /// Manages trading posts, availability, pricing, and opportunities
    /// </summary>
    public interface ITradingPostManagementService : IService
    {
        // Trading Post Management
        List<TradingPost> GetAvailableTradingPosts();
        List<TradingPost> GetTradingPostsByType(TradingPostType type);
        TradingPost GetTradingPost(string tradingPostId);
        bool IsTradingPostAvailable(TradingPost tradingPost, MarketProductSO product, float quantity);
        TradingPostStatus GetTradingPostStatus(string tradingPostId);
        
        // Product Availability
        List<MarketProductSO> GetAvailableProducts(string tradingPostId);
        float GetProductQuantity(string tradingPostId, MarketProductSO product);
        bool IsProductAvailable(string tradingPostId, MarketProductSO product, float quantity);
        
        // Trading Opportunities
        List<TradingOpportunity> GetTradingOpportunities(OpportunityType opportunityType = OpportunityType.All);
        TradingOpportunity GetTradingOpportunity(string opportunityId);
        bool IsOpportunityValid(string opportunityId);
        void UpdateTradingOpportunities();
        
        // Events
        event Action<TradingPost> OnTradingPostStatusChanged;
        event Action<TradingOpportunity> OnTradingOpportunityAdded;
        event Action<string> OnTradingOpportunityExpired;
    }

    /// <summary>
    /// PC014-4c: Financial Management Service Interface
    /// Player finances, inventory tracking, and profitability analysis
    /// </summary>
    public interface IFinancialManagementService : IService
    {
        // Financial Management
        float GetCashBalance(string playerId);
        float GetNetWorth(string playerId);
        bool TransferCash(string playerId, float amount, CashTransferType transferType);
        FinancialMetrics GetFinancialMetrics(string playerId);
        
        // Inventory Management
        PlayerInventory GetPlayerInventory(string playerId);
        List<InventoryItem> GetInventoryForProduct(string playerId, MarketProductSO product);
        float GetTotalInventoryQuantity(string playerId, MarketProductSO product);
        bool AddToInventory(string playerId, InventoryItem item);
        bool RemoveFromInventory(string playerId, string itemId, float quantity);
        
        // Profitability Analysis
        TradingProfitabilityAnalysis AnalyzeProfitability(MarketProductSO product, float quantity, TradingTransactionType transactionType);
        float CalculateBreakEvenPrice(MarketProductSO product, float quantity);
        float EstimateProfit(MarketProductSO product, float quantity, float buyPrice, float sellPrice);
        
        // Events
        event Action<string, float, float> OnCashChanged; // playerId, oldAmount, newAmount
        event Action<string, InventoryItem, float> OnInventoryChanged; // playerId, item, quantityChange
        event Action<string, FinancialMetrics> OnFinancialMetricsUpdated; // playerId, metrics
    }

    #endregion

    // All data types moved to their respective Data namespace assemblies
    // ServiceInterfaces.cs only contains interface definitions
}