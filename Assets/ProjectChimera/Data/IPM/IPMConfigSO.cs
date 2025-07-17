using System;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.IPM
{
    /// <summary>
    /// Configuration ScriptableObject for the Enhanced IPM Gaming System.
    /// Contains all settings and parameters for pest management strategic combat.
    /// </summary>
    [CreateAssetMenu(fileName = "IPM_Config", menuName = "Project Chimera/IPM/IPM Configuration")]
    public class IPMConfigSO : ChimeraConfigSO
    {
        [Header("System Configuration")]
        [SerializeField] private bool _enableIPMGaming = true;
        [SerializeField] private bool _enableRealTimeInvasions = true;
        [SerializeField] private bool _enableMultiplayerIPM = true;
        [SerializeField] private bool _enableAIOpponents = true;
        [SerializeField] private float _invasionFrequency = 0.2f;

        [Header("Battle System Settings")]
        [SerializeField] private float _battleTimeScale = 1.0f;
        [SerializeField] private int _maxSimultaneousBattles = 5;
        [SerializeField] private bool _enableSlowMotionMode = true;
        [SerializeField] private AnimationCurve _difficultyProgression;
        [SerializeField] private float _battleDuration = 300f; // 5 minutes
        [SerializeField] private int _maxPestUnitsPerBattle = 500;

        [Header("Strategic Elements")]
        [SerializeField] private bool _enablePreventiveStrategy = true;
        [SerializeField] private bool _enableResourceManagement = true;
        [SerializeField] private bool _enableIntelligenceGathering = true;
        [SerializeField] private float _strategicPlanningTime = 30f;
        [SerializeField] private int _maxDefenseStructures = 100;
        [SerializeField] private float _defenseEffectivenessMultiplier = 1.0f;

        [Header("Biological Warfare Settings")]
        [SerializeField] private bool _enableBiologicalWarfare = true;
        [SerializeField] private float _beneficialOrganismEffectiveness = 1.0f;
        [SerializeField] private int _maxBeneficialOrganisms = 1000;
        [SerializeField] private float _biologicalWeaponCooldown = 60f;
        [SerializeField] private float _ecosystemBalanceThreshold = 0.8f;

        [Header("Chemical Precision Warfare")]
        [SerializeField] private bool _enableChemicalWarfare = true;
        [SerializeField] private float _chemicalApplicationPrecision = 0.95f;
        [SerializeField] private float _resistanceDevelopmentRate = 0.05f;
        [SerializeField] private int _maxChemicalApplications = 20;
        [SerializeField] private float _chemicalRotationInterval = 168f; // 1 week

        [Header("Environmental Warfare")]
        [SerializeField] private bool _enableEnvironmentalWarfare = true;
        [SerializeField] private float _microzoneCreationCost = 100f;
        [SerializeField] private float _environmentalBarrierEffectiveness = 0.8f;
        [SerializeField] private int _maxEnvironmentalZones = 50;
        [SerializeField] private float _climateManipulationPower = 1.0f;

        [Header("AI and Intelligence")]
        [SerializeField] private bool _enableAdvancedAI = true;
        [SerializeField] private float _aiAdaptationRate = 0.1f;
        [SerializeField] private int _maxAILearningCycles = 100;
        [SerializeField] private float _pestIntelligenceLevel = 0.5f;
        [SerializeField] private bool _enablePredictiveAnalytics = true;

        [Header("Performance Settings")]
        [SerializeField] private bool _enablePerformanceOptimization = true;
        [SerializeField] private int _maxIPMUpdatesPerFrame = 20;
        [SerializeField] private float _updateInterval = 0.1f; // 10 times per second
        [SerializeField] private bool _enableBatchProcessing = true;
        [SerializeField] private int _maxCachedBattles = 10;

        [Header("Multiplayer Configuration")]
        [SerializeField] private int _maxPlayersPerBattle = 4;
        [SerializeField] private bool _enableCooperativeMode = true;
        [SerializeField] private bool _enableCompetitiveMode = true;
        [SerializeField] private bool _enableTournamentMode = true;
        [SerializeField] private float _multiplayerSyncInterval = 0.05f; // 20 times per second

        [Header("Research and Technology")]
        [SerializeField] private bool _enableIPMResearch = true;
        [SerializeField] private int _maxResearchProjects = 10;
        [SerializeField] private float _researchSpeedMultiplier = 1.0f;
        [SerializeField] private bool _enableTechnologyUpgrades = true;
        [SerializeField] private int _maxTechnologyLevel = 10;

        [Header("Economic Balance")]
        [SerializeField] private float _resourceCostMultiplier = 1.0f;
        [SerializeField] private bool _enableDynamicPricing = true;
        [SerializeField] private float _economicInflationRate = 0.02f;
        [SerializeField] private int _maxResourceStorage = 10000;
        [SerializeField] private float _resourceRegenerationRate = 1.0f;

        // Public Properties
        public bool EnableIPMGaming => _enableIPMGaming;
        public bool EnableRealTimeInvasions => _enableRealTimeInvasions;
        public bool EnableMultiplayerIPM => _enableMultiplayerIPM;
        public bool EnableAIOpponents => _enableAIOpponents;
        public float InvasionFrequency => _invasionFrequency;

        public float BattleTimeScale => _battleTimeScale;
        public int MaxSimultaneousBattles => _maxSimultaneousBattles;
        public bool EnableSlowMotionMode => _enableSlowMotionMode;
        public AnimationCurve DifficultyProgression => _difficultyProgression;
        public float BattleDuration => _battleDuration;
        public int MaxPestUnitsPerBattle => _maxPestUnitsPerBattle;

        public bool EnablePreventiveStrategy => _enablePreventiveStrategy;
        public bool EnableResourceManagement => _enableResourceManagement;
        public bool EnableIntelligenceGathering => _enableIntelligenceGathering;
        public float StrategicPlanningTime => _strategicPlanningTime;
        public int MaxDefenseStructures => _maxDefenseStructures;
        public float DefenseEffectivenessMultiplier => _defenseEffectivenessMultiplier;

        public bool EnableBiologicalWarfare => _enableBiologicalWarfare;
        public float BeneficialOrganismEffectiveness => _beneficialOrganismEffectiveness;
        public int MaxBeneficialOrganisms => _maxBeneficialOrganisms;
        public float BiologicalWeaponCooldown => _biologicalWeaponCooldown;
        public float EcosystemBalanceThreshold => _ecosystemBalanceThreshold;

        public bool EnableChemicalWarfare => _enableChemicalWarfare;
        public float ChemicalApplicationPrecision => _chemicalApplicationPrecision;
        public float ResistanceDevelopmentRate => _resistanceDevelopmentRate;
        public int MaxChemicalApplications => _maxChemicalApplications;
        public float ChemicalRotationInterval => _chemicalRotationInterval;

        public bool EnableEnvironmentalWarfare => _enableEnvironmentalWarfare;
        public float MicrozoneCreationCost => _microzoneCreationCost;
        public float EnvironmentalBarrierEffectiveness => _environmentalBarrierEffectiveness;
        public int MaxEnvironmentalZones => _maxEnvironmentalZones;
        public float ClimateManipulationPower => _climateManipulationPower;

        public bool EnableAdvancedAI => _enableAdvancedAI;
        public float AIAdaptationRate => _aiAdaptationRate;
        public int MaxAILearningCycles => _maxAILearningCycles;
        public float PestIntelligenceLevel => _pestIntelligenceLevel;
        public bool EnablePredictiveAnalytics => _enablePredictiveAnalytics;

        public bool EnablePerformanceOptimization => _enablePerformanceOptimization;
        public int MaxIPMUpdatesPerFrame => _maxIPMUpdatesPerFrame;
        public float UpdateInterval => _updateInterval;
        public bool EnableBatchProcessing => _enableBatchProcessing;
        public int MaxCachedBattles => _maxCachedBattles;

        public int MaxPlayersPerBattle => _maxPlayersPerBattle;
        public bool EnableCooperativeMode => _enableCooperativeMode;
        public bool EnableCompetitiveMode => _enableCompetitiveMode;
        public bool EnableTournamentMode => _enableTournamentMode;
        public float MultiplayerSyncInterval => _multiplayerSyncInterval;

        public bool EnableIPMResearch => _enableIPMResearch;
        public int MaxResearchProjects => _maxResearchProjects;
        public float ResearchSpeedMultiplier => _researchSpeedMultiplier;
        public bool EnableTechnologyUpgrades => _enableTechnologyUpgrades;
        public int MaxTechnologyLevel => _maxTechnologyLevel;

        public float ResourceCostMultiplier => _resourceCostMultiplier;
        public bool EnableDynamicPricing => _enableDynamicPricing;
        public float EconomicInflationRate => _economicInflationRate;
        public int MaxResourceStorage => _maxResourceStorage;
        public float ResourceRegenerationRate => _resourceRegenerationRate;

        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Validate configuration values
            _invasionFrequency = Mathf.Clamp(_invasionFrequency, 0f, 1f);
            _battleTimeScale = Mathf.Clamp(_battleTimeScale, 0.1f, 10f);
            _maxSimultaneousBattles = Mathf.Clamp(_maxSimultaneousBattles, 1, 20);
            _maxPestUnitsPerBattle = Mathf.Clamp(_maxPestUnitsPerBattle, 10, 2000);
            _strategicPlanningTime = Mathf.Clamp(_strategicPlanningTime, 5f, 300f);
            _maxDefenseStructures = Mathf.Clamp(_maxDefenseStructures, 10, 500);
            _beneficialOrganismEffectiveness = Mathf.Clamp(_beneficialOrganismEffectiveness, 0.1f, 5f);
            _maxBeneficialOrganisms = Mathf.Clamp(_maxBeneficialOrganisms, 100, 5000);
            _chemicalApplicationPrecision = Mathf.Clamp01(_chemicalApplicationPrecision);
            _resistanceDevelopmentRate = Mathf.Clamp01(_resistanceDevelopmentRate);
            _aiAdaptationRate = Mathf.Clamp01(_aiAdaptationRate);
            _pestIntelligenceLevel = Mathf.Clamp01(_pestIntelligenceLevel);
            _updateInterval = Mathf.Clamp(_updateInterval, 0.01f, 1f);
            _maxPlayersPerBattle = Mathf.Clamp(_maxPlayersPerBattle, 1, 8);
            _multiplayerSyncInterval = Mathf.Clamp(_multiplayerSyncInterval, 0.01f, 1f);
            _researchSpeedMultiplier = Mathf.Clamp(_researchSpeedMultiplier, 0.1f, 10f);
            _maxTechnologyLevel = Mathf.Clamp(_maxTechnologyLevel, 1, 50);
        }

        /// <summary>
        /// Gets the configured difficulty multiplier based on player level.
        /// </summary>
        public float GetDifficultyMultiplier(int playerLevel)
        {
            if (_difficultyProgression == null)
                return 1f;

            var normalizedLevel = Mathf.Clamp01((float)playerLevel / 100f);
            return _difficultyProgression.Evaluate(normalizedLevel);
        }

        /// <summary>
        /// Calculates resource cost with multipliers applied.
        /// </summary>
        public float CalculateResourceCost(float baseCost)
        {
            return baseCost * _resourceCostMultiplier;
        }

        /// <summary>
        /// Determines if a battle can be started based on current configuration.
        /// </summary>
        public bool CanStartBattle(int currentActiveBattles)
        {
            return _enableIPMGaming && currentActiveBattles < _maxSimultaneousBattles;
        }

        /// <summary>
        /// Gets the maximum allowed entities for the specified battle component.
        /// </summary>
        public int GetMaxEntities(IPMEntityType entityType)
        {
            return entityType switch
            {
                IPMEntityType.PestUnits => _maxPestUnitsPerBattle,
                IPMEntityType.DefenseStructures => _maxDefenseStructures,
                IPMEntityType.BeneficialOrganisms => _maxBeneficialOrganisms,
                IPMEntityType.EnvironmentalZones => _maxEnvironmentalZones,
                IPMEntityType.ChemicalApplications => _maxChemicalApplications,
                _ => 100
            };
        }
    }

    public enum IPMEntityType
    {
        PestUnits,
        DefenseStructures,
        BeneficialOrganisms,
        EnvironmentalZones,
        ChemicalApplications
    }
}