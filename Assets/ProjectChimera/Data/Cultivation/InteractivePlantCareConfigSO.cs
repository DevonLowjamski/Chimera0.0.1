using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Cultivation
{
    /// <summary>
    /// Interactive Plant Care Configuration - Settings for hands-on cultivation mechanics
    /// Defines care action parameters, feedback systems, and skill-based precision
    /// </summary>
    [CreateAssetMenu(fileName = "Interactive_Plant_Care_Config", menuName = "Project Chimera/Cultivation/Interactive Plant Care Config")]
    public class InteractivePlantCareConfigSO : ChimeraDataSO
    {
        [Header("Care Action Base Settings")]
        [Range(0.1f, 2.0f)] public float BaseActionEfficiency = 1.0f;
        [Range(0.1f, 5.0f)] public float SkillMultiplierMax = 2.5f;
        [Range(0.1f, 1.0f)] public float ActionSuccessThreshold = 0.6f;
        [Range(0.1f, 2.0f)] public float DifficultyScaling = 1.0f;
        
        [Header("Feedback System Settings")]
        [Range(0.1f, 3.0f)] public float VisualFeedbackIntensity = 1.5f;
        [Range(0.1f, 2.0f)] public float AudioFeedbackVolume = 1.0f;
        [Range(0.1f, 5.0f)] public float HapticFeedbackStrength = 1.0f;
        [Range(0.1f, 2.0f)] public float FeedbackDuration = 1.0f;
        
        [Header("Skill Progression Settings")]
        [Range(0.1f, 10.0f)] public float BaseSkillGain = 1.0f;
        [Range(1.1f, 3.0f)] public float QualityBonusMultiplier = 1.5f;
        [Range(0.1f, 1.0f)] public float SkillDecayRate = 0.05f;
        [Range(1.0f, 100.0f)] public float SkillLevelThreshold = 25.0f;
        [Range(1.0f, 10.0f)] public float BaseSkillLevel = 1.0f;
        [Range(10.0f, 100.0f)] public float MaxSkillLevel = 50.0f;
        [Range(0.1f, 10.0f)] public float MaxTimingWindow = 3.0f;
        [Range(0.1f, 3.0f)] public float ToolQualityBonus = 1.5f;
        
        [Header("Action Relevance Settings")]
        [Range(0.1f, 1.0f)] public float MinActionRelevanceThreshold = 0.3f;
        
        [Header("Care Quality Parameters")]
        [SerializeField] private CareQualitySettings _qualitySettings;
        [SerializeField] private CareTimingSettings _timingSettings;
        [SerializeField] private CarePrecisionSettings _precisionSettings;
        
        [Header("Plant Response Configuration")]
        [SerializeField] private PlantResponseSettings _responseSettings;
        [SerializeField] private HealthIndicatorSettings _healthIndicatorSettings;
        [SerializeField] private GrowthResponseSettings _growthResponseSettings;
        
        [Header("Tool Integration Settings")]
        [Range(0.1f, 3.0f)] public float ToolEfficiencyMultiplier = 1.2f;
        
        [Header("Skill Milestone Settings")]
        public List<SkillMilestone> SkillMilestones = new List<SkillMilestone>();
        [Range(0.1f, 2.0f)] public float ToolWearRate = 0.8f;
        [Range(0.1f, 1.0f)] public float ToolMainttenanceThreshold = 0.3f;
        [Range(1.0f, 10.0f)] public float ToolUpgradeThreshold = 5.0f;
        
        // Public Properties
        public CareQualitySettings QualitySettings => _qualitySettings;
        public CareTimingSettings TimingSettings => _timingSettings;
        public CarePrecisionSettings PrecisionSettings => _precisionSettings;
        public PlantResponseSettings ResponseSettings => _responseSettings;
        public HealthIndicatorSettings HealthIndicatorSettings => _healthIndicatorSettings;
        public GrowthResponseSettings GrowthResponseSettings => _growthResponseSettings;
        
        #region Validation
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            ValidateSettings();
        }
        
        private void ValidateSettings()
        {
            // Ensure all values are within reasonable ranges
            BaseActionEfficiency = Mathf.Clamp(BaseActionEfficiency, 0.1f, 2.0f);
            SkillMultiplierMax = Mathf.Clamp(SkillMultiplierMax, 0.1f, 5.0f);
            ActionSuccessThreshold = Mathf.Clamp01(ActionSuccessThreshold);
            DifficultyScaling = Mathf.Clamp(DifficultyScaling, 0.1f, 2.0f);
            
            VisualFeedbackIntensity = Mathf.Clamp(VisualFeedbackIntensity, 0.1f, 3.0f);
            AudioFeedbackVolume = Mathf.Clamp(AudioFeedbackVolume, 0.1f, 2.0f);
            HapticFeedbackStrength = Mathf.Clamp(HapticFeedbackStrength, 0.1f, 5.0f);
            FeedbackDuration = Mathf.Clamp(FeedbackDuration, 0.1f, 2.0f);
            
            BaseSkillGain = Mathf.Clamp(BaseSkillGain, 0.1f, 10.0f);
            QualityBonusMultiplier = Mathf.Clamp(QualityBonusMultiplier, 1.1f, 3.0f);
            SkillDecayRate = Mathf.Clamp01(SkillDecayRate);
            SkillLevelThreshold = Mathf.Clamp(SkillLevelThreshold, 1.0f, 100.0f);
        }
        
        #endregion
    }
    
    #region Care Quality Settings
    
    [System.Serializable]
    public class CareQualitySettings
    {
        [Header("Quality Calculation")]
        [Range(0.1f, 1.0f)] public float PerfectActionThreshold = 0.95f;
        [Range(0.1f, 1.0f)] public float ExcellentActionThreshold = 0.85f;
        [Range(0.1f, 1.0f)] public float GoodActionThreshold = 0.7f;
        [Range(0.1f, 1.0f)] public float AverageActionThreshold = 0.5f;
        
        [Header("Quality Bonuses")]
        [Range(1.0f, 3.0f)] public float PerfectQualityBonus = 2.0f;
        [Range(1.0f, 2.5f)] public float ExcellentQualityBonus = 1.5f;
        [Range(1.0f, 2.0f)] public float GoodQualityBonus = 1.2f;
        [Range(1.0f, 1.5f)] public float AverageQualityBonus = 1.0f;
        
        [Header("Quality Penalties")]
        [Range(0.1f, 1.0f)] public float PoorQualityPenalty = 0.8f;
        [Range(0.1f, 1.0f)] public float FailedQualityPenalty = 0.5f;
        
        public CareQuality CalculateQuality(float actionPrecision)
        {
            if (actionPrecision >= PerfectActionThreshold) return CareQuality.Perfect;
            if (actionPrecision >= ExcellentActionThreshold) return CareQuality.Excellent;
            if (actionPrecision >= GoodActionThreshold) return CareQuality.Good;
            if (actionPrecision >= AverageActionThreshold) return CareQuality.Average;
            return CareQuality.Poor;
        }
        
        public float GetQualityMultiplier(CareQuality quality)
        {
            return quality switch
            {
                CareQuality.Perfect => PerfectQualityBonus,
                CareQuality.Excellent => ExcellentQualityBonus,
                CareQuality.Good => GoodQualityBonus,
                CareQuality.Average => AverageQualityBonus,
                CareQuality.Poor => PoorQualityPenalty,
                CareQuality.Failed => FailedQualityPenalty,
                _ => 1.0f
            };
        }
    }
    
    #endregion
    
    #region Care Timing Settings
    
    [System.Serializable]
    public class CareTimingSettings
    {
        [Header("Timing Windows")]
        [Range(0.1f, 5.0f)] public float OptimalTimingWindow = 2.0f;
        [Range(0.1f, 10.0f)] public float AcceptableTimingWindow = 5.0f;
        [Range(1.0f, 24.0f)] public float CriticalTimingWindow = 12.0f;
        
        [Header("Timing Bonuses")]
        [Range(1.0f, 2.0f)] public float OptimalTimingBonus = 1.3f;
        [Range(1.0f, 1.5f)] public float AcceptableTimingBonus = 1.1f;
        [Range(0.5f, 1.0f)] public float LateTimingPenalty = 0.8f;
        
        [Header("Frequency Settings")]
        [Range(0.5f, 8.0f)] public float WateringFrequencyHours = 2.0f;
        [Range(12.0f, 168.0f)] public float PruningFrequencyHours = 72.0f;
        [Range(1.0f, 24.0f)] public float TrainingFrequencyHours = 6.0f;
        [Range(24.0f, 336.0f)] public float FertilizingFrequencyHours = 168.0f;
    }
    
    #endregion
    
    #region Care Precision Settings
    
    [System.Serializable]
    public class CarePrecisionSettings
    {
        [Header("Precision Requirements")]
        [Range(0.1f, 1.0f)] public float WateringPrecisionRequired = 0.7f;
        [Range(0.1f, 1.0f)] public float PruningPrecisionRequired = 0.9f;
        [Range(0.1f, 1.0f)] public float TrainingPrecisionRequired = 0.8f;
        [Range(0.1f, 1.0f)] public float FertilizingPrecisionRequired = 0.6f;
        
        [Header("Precision Calculation")]
        [Range(0.1f, 2.0f)] public float SkillInfluence = 1.0f;
        [Range(0.1f, 2.0f)] public float ToolInfluence = 0.8f;
        [Range(0.1f, 2.0f)] public float ExperienceInfluence = 0.6f;
        [Range(0.1f, 1.0f)] public float RandomVariance = 0.1f;
        
        public float CalculatePrecision(float skillLevel, float toolQuality, float experience)
        {
            float basePrecision = 0.5f;
            float skillBonus = (skillLevel / 100f) * SkillInfluence;
            float toolBonus = toolQuality * ToolInfluence;
            float experienceBonus = Mathf.Clamp01(experience / 1000f) * ExperienceInfluence;
            float randomFactor = Random.Range(-RandomVariance, RandomVariance);
            
            return Mathf.Clamp01(basePrecision + skillBonus + toolBonus + experienceBonus + randomFactor);
        }
    }
    
    #endregion
    
    #region Plant Response Settings
    
    [System.Serializable]
    public class PlantResponseSettings
    {
        [Header("Visual Response")]
        [Range(0.1f, 2.0f)] public float LeafMovementIntensity = 1.0f;
        [Range(0.1f, 5.0f)] public float ColorChangeIntensity = 1.5f;
        [Range(0.1f, 3.0f)] public float GrowthAnimationSpeed = 1.0f;
        [Range(0.1f, 2.0f)] public float HealthIndicatorOpacity = 0.8f;
        
        [Header("Response Timing")]
        [Range(0.1f, 2.0f)] public float ImmediateResponseDelay = 0.2f;
        [Range(1.0f, 10.0f)] public float ShortTermResponseDelay = 3.0f;
        [Range(10.0f, 60.0f)] public float LongTermResponseDelay = 30.0f;
        
        [Header("Response Intensity")]
        [Range(0.1f, 3.0f)] public float PositiveResponseMultiplier = 1.5f;
        [Range(0.1f, 2.0f)] public float NegativeResponseMultiplier = 1.2f;
        [Range(0.1f, 1.0f)] public float NeutralResponseMultiplier = 0.8f;
    }
    
    #endregion
    
    #region Health Indicator Settings
    
    [System.Serializable]
    public class HealthIndicatorSettings
    {
        [Header("Health Visualization")]
        [SerializeField] public Color ExcellentHealthColor = Color.green;
        [SerializeField] public Color GoodHealthColor = Color.yellow;
        [SerializeField] public Color PoorHealthColor = Color.orange;
        [SerializeField] public Color CriticalHealthColor = Color.red;
        
        [Header("Indicator Behavior")]
        [Range(0.1f, 2.0f)] public float IndicatorFadeSpeed = 1.0f;
        [Range(0.1f, 5.0f)] public float IndicatorPulseRate = 2.0f;
        [Range(0.1f, 1.0f)] public float IndicatorAlpha = 0.7f;
        
        [Header("Health Thresholds")]
        [Range(0.1f, 1.0f)] public float ExcellentHealthThreshold = 0.9f;
        [Range(0.1f, 1.0f)] public float GoodHealthThreshold = 0.7f;
        [Range(0.1f, 1.0f)] public float PoorHealthThreshold = 0.4f;
        [Range(0.1f, 1.0f)] public float CriticalHealthThreshold = 0.2f;
        
        public Color GetHealthColor(float healthValue)
        {
            if (healthValue >= ExcellentHealthThreshold) return ExcellentHealthColor;
            if (healthValue >= GoodHealthThreshold) return GoodHealthColor;
            if (healthValue >= PoorHealthThreshold) return PoorHealthColor;
            return CriticalHealthColor;
        }
    }
    
    #endregion
    
    #region Growth Response Settings
    
    [System.Serializable]
    public class GrowthResponseSettings
    {
        [Header("Growth Rate Modifiers")]
        [Range(0.1f, 3.0f)] public float ExcellentCareGrowthBonus = 1.5f;
        [Range(1.0f, 2.0f)] public float GoodCareGrowthBonus = 1.2f;
        [Range(0.5f, 1.0f)] public float PoorCareGrowthPenalty = 0.8f;
        [Range(0.1f, 0.5f)] public float CriticalCareGrowthPenalty = 0.4f;
        
        [Header("Growth Visual Effects")]
        [Range(0.1f, 2.0f)] public float GrowthParticleIntensity = 1.0f;
        [Range(0.1f, 5.0f)] public float NewGrowthHighlightDuration = 3.0f;
        [Range(0.1f, 2.0f)] public float BranchDevelopmentSpeed = 1.0f;
        [Range(0.1f, 2.0f)] public float LeafUnfoldingSpeed = 1.2f;
        
        [Header("Milestone Responses")]
        [Range(1.0f, 10.0f)] public float StageTransitionCelebration = 5.0f;
        [Range(0.1f, 2.0f)] public float FirstFlowerCelebration = 1.5f;
        [Range(0.1f, 2.0f)] public float MaturityCelebration = 1.8f;
        
        public float GetGrowthModifier(CareQuality averageCareQuality)
        {
            return averageCareQuality switch
            {
                CareQuality.Perfect => ExcellentCareGrowthBonus,
                CareQuality.Excellent => ExcellentCareGrowthBonus,
                CareQuality.Good => GoodCareGrowthBonus,
                CareQuality.Average => 1.0f,
                CareQuality.Poor => PoorCareGrowthPenalty,
                CareQuality.Failed => CriticalCareGrowthPenalty,
                _ => 1.0f
            };
        }
    }
    
    #endregion
    
    // Enums
    public enum CareQuality
    {
        Perfect,
        Excellent,
        Good,
        Average,
        Poor,
        Failed
    }
}