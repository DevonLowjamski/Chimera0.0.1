using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core.Interfaces;
using ProjectChimera.Data.IPM;
using ProjectChimera.Systems.Gaming.IPM;
using DataIPMBattleConfiguration = ProjectChimera.Data.IPM.IPMBattleConfiguration;
using DataIPMBattleOutcome = ProjectChimera.Data.IPM.IPMBattleOutcome;

namespace ProjectChimera.Systems.Gaming.Interfaces
{
    /// <summary>
    /// Base interface for all IPM gaming system components.
    /// Provides common functionality and metrics for IPM systems integration.
    /// </summary>
    public interface IIPMSystem : IConfigurableGameSystem<IPMConfigSO>
    {
        IPMSystemMetrics GetMetrics();
    }

    /// <summary>
    /// Interface for IPM battle management and strategic combat systems.
    /// Handles strategic combat scenarios against pest invasions.
    /// </summary>
    public interface IIPMBattleManager : IIPMSystem
    {
        event Action<IPMBattleData> OnBattleStarted;
        event Action<IPMBattleData> OnBattleEnded;
        event Action<IPMBattlePhase> OnBattlePhaseChanged;
        
        bool StartBattle(DataIPMBattleConfiguration config);
        bool EndBattle(string battleId, DataIPMBattleOutcome result);
        IPMBattleData GetBattleData(string battleId);
        List<IPMBattleData> GetActiveBattles();
        bool CanStartBattle();
        void UpdateBattle(string battleId, float deltaTime);
    }

    /// <summary>
    /// Interface for pest invasion simulation and management.
    /// Handles dynamic pest behavior and adaptive AI systems.
    /// </summary>
    public interface IIPMPestManager : IIPMSystem
    {
        event Action<ProjectChimera.Data.IPM.PestInvasionData> OnInvasionStarted;
        event Action<ProjectChimera.Data.IPM.PestInvasionData> OnInvasionEnded;
        event Action<ProjectChimera.Data.IPM.PestType, int> OnPestPopulationChanged;
        
        void SpawnInvasion(ProjectChimera.Data.IPM.PestInvasionData invasionData);
        void UpdateInvasion(string invasionId, float deltaTime);
        void EndInvasion(string invasionId);
        List<ProjectChimera.Data.IPM.PestInvasionData> GetActiveInvasions();
        int GetPestPopulation(ProjectChimera.Data.IPM.PestType pestType);
        void ApplyPestControl(string invasionId, ProjectChimera.Data.IPM.IPMStrategyType strategy, float effectiveness);
    }

    /// <summary>
    /// Interface for biological control agent management.
    /// Handles beneficial organism deployment and ecosystem balance.
    /// </summary>
    public interface IIPMBiologicalManager : IIPMSystem
    {
        event Action<ProjectChimera.Data.IPM.BeneficialOrganismData> OnOrganismDeployed;
        event Action<ProjectChimera.Data.IPM.BeneficialOrganismData> OnOrganismEstablished;
        event Action<ProjectChimera.Data.IPM.BeneficialOrganismType, int> OnPopulationChanged;
        
        void DeployOrganism(ProjectChimera.Data.IPM.BeneficialOrganismData organismData);
        void UpdateOrganisms(float deltaTime);
        void RemoveOrganism(string organismId);
        List<ProjectChimera.Data.IPM.BeneficialOrganismData> GetActiveOrganisms();
        float CalculateEcosystemBalance();
        bool CanDeployOrganism(ProjectChimera.Data.IPM.BeneficialOrganismType type);
    }

    /// <summary>
    /// Interface for defense structure placement and management.
    /// Handles tactical positioning and effectiveness optimization.
    /// </summary>
    public interface IIPMDefenseManager : IIPMSystem
    {
        event Action<ProjectChimera.Data.IPM.DefenseStructureData> OnStructureBuilt;
        event Action<ProjectChimera.Data.IPM.DefenseStructureData> OnStructureDestroyed;
        event Action<ProjectChimera.Data.IPM.DefenseStructureData> OnStructureUpgraded;
        
        bool BuildStructure(ProjectChimera.Data.IPM.DefenseStructureData structureData);
        void UpgradeStructure(string structureId, float upgradeLevel);
        void DestroyStructure(string structureId);
        List<ProjectChimera.Data.IPM.DefenseStructureData> GetActiveStructures();
        bool CanBuildStructure(ProjectChimera.Data.IPM.DefenseStructureType type, Vector3 position);
        float CalculateDefenseCoverage(Vector3 position);
    }

    /// <summary>
    /// Interface for environmental manipulation and control.
    /// Handles microclimate creation and environmental warfare tactics.
    /// </summary>
    public interface IIPMEnvironmentalManager : IIPMSystem
    {
        event Action<ProjectChimera.Data.IPM.EnvironmentalZoneData> OnZoneCreated;
        event Action<ProjectChimera.Data.IPM.EnvironmentalZoneData> OnZoneModified;
        event Action<ProjectChimera.Data.IPM.EnvironmentalZoneData> OnZoneDestroyed;
        
        void CreateEnvironmentalZone(ProjectChimera.Data.IPM.EnvironmentalZoneData zoneData);
        void ModifyZone(string zoneId, Dictionary<string, float> parameters);
        void DestroyZone(string zoneId);
        List<ProjectChimera.Data.IPM.EnvironmentalZoneData> GetActiveZones();
        float CalculateEnvironmentalEffectiveness(Vector3 position, ProjectChimera.Data.IPM.PestType pestType);
        bool CanCreateZone(ProjectChimera.Data.IPM.EnvironmentalZoneType type, Vector3 position);
    }

    /// <summary>
    /// Interface for chemical application precision systems.
    /// Handles targeted chemical warfare with resistance management.
    /// </summary>
    public interface IIPMChemicalManager : IIPMSystem
    {
        event Action<ProjectChimera.Data.IPM.ChemicalApplicationData> OnChemicalApplied;
        event Action<string, float> OnResistanceDeveloped;
        event Action<string, float> OnApplicationDegraded;
        
        void ApplyChemical(ProjectChimera.Data.IPM.ChemicalApplicationData applicationData);
        void UpdateApplications(float deltaTime);
        void RemoveApplication(string applicationId);
        List<ProjectChimera.Data.IPM.ChemicalApplicationData> GetActiveApplications();
        float CalculateResistanceLevel(ProjectChimera.Data.IPM.PestType pestType, string chemicalType);
        bool CanApplyChemical(string chemicalType, Vector3 position);
    }

    /// <summary>
    /// Interface for IPM strategy planning and optimization.
    /// Handles AI-driven strategy recommendations and optimization.
    /// </summary>
    public interface IIPMStrategyManager : IIPMSystem
    {
        event Action<ProjectChimera.Data.IPM.IPMStrategyPlan> OnStrategyCreated;
        event Action<ProjectChimera.Data.IPM.IPMStrategyPlan> OnStrategyExecuted;
        event Action<ProjectChimera.Data.IPM.IPMStrategyPlan> OnStrategyCompleted;
        
        ProjectChimera.Data.IPM.IPMStrategyPlan CreateStrategy(ProjectChimera.Data.IPM.IPMStrategyType strategyType, Dictionary<string, object> parameters);
        void ExecuteStrategy(string planId);
        void ModifyStrategy(string planId, Dictionary<string, object> modifications);
        List<ProjectChimera.Data.IPM.IPMStrategyPlan> GetActiveStrategies();
        ProjectChimera.Data.IPM.IPMRecommendation GetRecommendation(IPMBattleData battleData);
        float CalculateStrategyEffectiveness(string planId);
    }

    /// <summary>
    /// Interface for IPM analytics and performance monitoring.
    /// Handles data collection, analysis, and insights generation.
    /// </summary>
    public interface IIPMAnalyticsManager : IIPMSystem
    {
        event Action<ProjectChimera.Data.IPM.IPMAnalyticsData> OnAnalyticsCollected;
        event Action<ProjectChimera.Data.IPM.IPMLeaderboardEntry> OnLeaderboardUpdated;
        event Action<ProjectChimera.Data.IPM.IPMLearningData> OnLearningDataGenerated;
        
        void CollectAnalytics(ProjectChimera.Data.IPM.IPMAnalyticsData analyticsData);
        ProjectChimera.Data.IPM.IPMAnalyticsData GetPlayerAnalytics(string playerId);
        List<ProjectChimera.Data.IPM.IPMLeaderboardEntry> GetLeaderboard(int count = 10);
        ProjectChimera.Data.IPM.IPMLearningData GenerateLearningData(string playerId);
        void UpdatePerformanceMetrics(string playerId, Dictionary<string, float> metrics);
        ProjectChimera.Data.IPM.IPMRecommendation GetPersonalizedRecommendation(string playerId);
    }

    /// <summary>
    /// Interface for IPM resource management and economy.
    /// Handles resource allocation, cost calculations, and economic balance.
    /// </summary>
    public interface IIPMResourceManager : IIPMSystem
    {
        event Action<string, ProjectChimera.Data.IPM.IPMResourceData> OnResourceUpdated;
        event Action<string, float> OnResourceDepleted;
        event Action<string, float> OnResourceGenerated;
        
        bool ConsumeResource(string resourceId, int amount);
        void AddResource(string resourceId, int amount);
        ProjectChimera.Data.IPM.IPMResourceData GetResource(string resourceId);
        Dictionary<string, ProjectChimera.Data.IPM.IPMResourceData> GetAllResources();
        bool CanAfford(Dictionary<string, int> requiredResources);
        float CalculateTotalCost(Dictionary<string, int> resources);
    }

    /// <summary>
    /// Interface for IPM AI and machine learning systems.
    /// Handles adaptive AI behavior and learning algorithms.
    /// </summary>
    public interface IIPMAIManager : IIPMSystem
    {
        event Action<ProjectChimera.Data.IPM.PestAIBehavior> OnAIBehaviorUpdated;
        event Action<ProjectChimera.Data.IPM.IPMLearningData> OnLearningCompleted;
        event Action<ProjectChimera.Data.IPM.IPMRecommendation> OnRecommendationGenerated;
        
        void UpdateAIBehavior(ProjectChimera.Data.IPM.PestType pestType, Dictionary<string, float> learningData);
        ProjectChimera.Data.IPM.PestAIBehavior GetAIBehavior(ProjectChimera.Data.IPM.PestType pestType);
        void ProcessLearningData(ProjectChimera.Data.IPM.IPMLearningData learningData);
        ProjectChimera.Data.IPM.IPMRecommendation GenerateRecommendation(string playerId, IPMBattleData battleData);
        float PredictStrategyEffectiveness(ProjectChimera.Data.IPM.IPMStrategyType strategy, IPMBattleData battleData);
    }

    /// <summary>
    /// Interface for IPM networking and multiplayer systems.
    /// Handles multiplayer battles, synchronization, and collaborative strategies.
    /// </summary>
    public interface IIPMNetworkManager : IIPMSystem
    {
        event Action<string> OnPlayerJoined;
        event Action<string> OnPlayerLeft;
        event Action<ProjectChimera.Data.IPM.IPMGameEvent> OnNetworkEventReceived;
        
        bool StartMultiplayerSession(DataIPMBattleConfiguration config);
        void EndMultiplayerSession(string sessionId);
        void SendNetworkEvent(ProjectChimera.Data.IPM.IPMGameEvent gameEvent);
        void SynchronizeState(string sessionId);
        List<string> GetConnectedPlayers();
        bool IsHost();
    }

    /// <summary>
    /// Interface for IPM notification and event management.
    /// Handles player notifications, achievement unlocks, and event triggers.
    /// </summary>
    public interface IIPMNotificationManager : IIPMSystem
    {
        event Action<ProjectChimera.Data.IPM.IPMNotification> OnNotificationReceived;
        event Action<ProjectChimera.Data.IPM.IPMGameEvent> OnGameEventTriggered;
        
        void SendNotification(ProjectChimera.Data.IPM.IPMNotification notification);
        void TriggerGameEvent(ProjectChimera.Data.IPM.IPMGameEvent gameEvent);
        List<ProjectChimera.Data.IPM.IPMNotification> GetUnreadNotifications(string playerId);
        void MarkNotificationAsRead(string notificationId);
        void SubscribeToEvents(string playerId, List<string> eventTypes);
        void UnsubscribeFromEvents(string playerId, List<string> eventTypes);
    }
}