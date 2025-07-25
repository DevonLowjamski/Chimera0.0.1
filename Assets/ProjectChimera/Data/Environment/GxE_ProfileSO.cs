using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using System.Collections.Generic;

namespace ProjectChimera.Data.Environment
{
    /// <summary>
    /// Defines how genetic traits interact with environmental conditions (Genotype x Environment interactions).
    /// Contains response curves and modifiers that determine how environmental factors affect trait expression.
    /// </summary>
    [CreateAssetMenu(fileName = "New GxE Profile", menuName = "Project Chimera/Environment/GxE Profile")]
    public class GxE_ProfileSO : ChimeraDataSO
    {
        [Header("Profile Identity")]
        [SerializeField] private string _profileName;
        [SerializeField, TextArea(3, 5)] private string _profileDescription;
        [SerializeField] private GxEInteractionType _interactionType = GxEInteractionType.Multiplicative;
        
        [Header("Target Traits")]
        [SerializeField] private List<TraitType> _targetTraits = new List<TraitType>();
        [SerializeField] private bool _appliesToAllTraits = false;
        
        [Header("Environmental Response Curves")]
        [SerializeField] private List<EnvironmentalResponseCurve> _responseCurves = new List<EnvironmentalResponseCurve>();
        
        [Header("Genetic Sensitivity")]
        [SerializeField, Range(0f, 2f)] private float _geneticSensitivity = 1f;
        [SerializeField] private List<GeneticSensitivityModifier> _geneSpecificSensitivities = new List<GeneticSensitivityModifier>();
        
        [Header("Interaction Thresholds")]
        [SerializeField] private List<InteractionThreshold> _interactionThresholds = new List<InteractionThreshold>();
        
        [Header("Phenotypic Plasticity")]
        [SerializeField, Range(0f, 1f)] private float _phenotypicPlasticity = 0.5f;
        [SerializeField] private bool _allowsEpigeneticChanges = false;
        [SerializeField, Range(0f, 1f)] private float _epigeneticStability = 0.8f;
        
        [Header("Stress Response")]
        [SerializeField] private List<StressResponseCurve> _stressResponses = new List<StressResponseCurve>();
        [SerializeField] private bool _hasAdaptiveResponse = true;
        [SerializeField, Range(0f, 1f)] private float _adaptationRate = 0.1f;
        
        // Public Properties
        public string ProfileName => _profileName;
        public string ProfileDescription => _profileDescription;
        public GxEInteractionType InteractionType => _interactionType;
        public List<TraitType> TargetTraits => _targetTraits;
        public bool AppliesToAllTraits => _appliesToAllTraits;
        public List<EnvironmentalResponseCurve> ResponseCurves => _responseCurves;
        public float GeneticSensitivity => _geneticSensitivity;
        public List<GeneticSensitivityModifier> GeneSpecificSensitivities => _geneSpecificSensitivities;
        public List<InteractionThreshold> InteractionThresholds => _interactionThresholds;
        public float PhenotypicPlasticity => _phenotypicPlasticity;
        public bool AllowsEpigeneticChanges => _allowsEpigeneticChanges;
        public float EpigeneticStability => _epigeneticStability;
        public List<StressResponseCurve> StressResponses => _stressResponses;
        public bool HasAdaptiveResponse => _hasAdaptiveResponse;
        public float AdaptationRate => _adaptationRate;
        
        /// <summary>
        /// Calculates the environmental modifier for a given trait based on current conditions.
        /// </summary>
        /// <param name="trait">The plant trait being affected</param>
        /// <param name="environment">Current environmental conditions</param>
        /// <param name="geneticValue">Base genetic value for the trait</param>
        /// <param name="gene">Specific gene contributing to the trait (optional)</param>
        /// <returns>Modified trait value</returns>
        public float CalculateEnvironmentalModifier(TraitType trait, ProjectChimera.Data.Cultivation.EnvironmentalConditions environment, float geneticValue, GeneDefinitionSO gene = null)
        {
            if (!_appliesToAllTraits && !_targetTraits.Contains(trait))
                return geneticValue;
            
            float environmentalEffect = 1f;
            
            // Calculate response from each environmental factor
            foreach (var responseCurve in _responseCurves)
            {
                float factorValue = GetEnvironmentalFactorValue(environment, responseCurve.EnvironmentalFactor);
                float response = responseCurve.ResponseCurve.Evaluate(factorValue);
                
                switch (_interactionType)
                {
                    case GxEInteractionType.Additive:
                        environmentalEffect += (response - 1f) * responseCurve.EffectStrength;
                        break;
                    case GxEInteractionType.Multiplicative:
                        environmentalEffect *= Mathf.Lerp(1f, response, responseCurve.EffectStrength);
                        break;
                    case GxEInteractionType.Threshold:
                        if (factorValue > responseCurve.ThresholdValue)
                            environmentalEffect *= response;
                        break;
                }
            }
            
            // Apply genetic sensitivity
            float geneticSensitivity = CalculateGeneticSensitivity(gene);
            environmentalEffect = Mathf.Lerp(1f, environmentalEffect, geneticSensitivity);
            
            // Apply phenotypic plasticity
            float plasticity = CalculatePhenotypicPlasticity(environment);
            environmentalEffect = Mathf.Lerp(1f, environmentalEffect, plasticity);
            
            // Calculate final modified value
            float modifiedValue = 0f;
            switch (_interactionType)
            {
                case GxEInteractionType.Additive:
                    modifiedValue = geneticValue + (environmentalEffect - 1f);
                    break;
                case GxEInteractionType.Multiplicative:
                    modifiedValue = geneticValue * environmentalEffect;
                    break;
                case GxEInteractionType.Threshold:
                    modifiedValue = geneticValue * environmentalEffect;
                    break;
                case GxEInteractionType.Epistatic:
                    modifiedValue = CalculateEpistaticInteraction(geneticValue, environmentalEffect);
                    break;
            }
            
            return Mathf.Max(0f, modifiedValue);
        }
        
        /// <summary>
        /// Calculates stress response effects on trait expression.
        /// </summary>
        public float CalculateStressResponse(TraitType trait, ProjectChimera.Data.Cultivation.EnvironmentalConditions environment, float currentStressLevel)
        {
            var stressResponse = _stressResponses.Find(sr => sr.AffectedTrait == trait);
            if (stressResponse == null) return 1f;
            
            float stressModifier = stressResponse.ResponseCurve.Evaluate(currentStressLevel);
            
            // Apply adaptive response if enabled
            if (_hasAdaptiveResponse && currentStressLevel > stressResponse.AdaptationThreshold)
            {
                float adaptationBonus = _adaptationRate * (currentStressLevel - stressResponse.AdaptationThreshold);
                stressModifier += adaptationBonus;
            }
            
            return Mathf.Max(stressResponse.MinResponse, stressModifier);
        }
        
        private float GetEnvironmentalFactorValue(ProjectChimera.Data.Cultivation.EnvironmentalConditions environment, EnvironmentalFactor factor)
        {
            switch (factor)
            {
                case EnvironmentalFactor.Temperature: return environment.Temperature;
                case EnvironmentalFactor.Light: return environment.LightIntensity;
                case EnvironmentalFactor.Humidity: return environment.Humidity;
                case EnvironmentalFactor.CO2: return environment.CO2Level;
                default: return 1f;
            }
        }
        
        private float CalculateGeneticSensitivity(GeneDefinitionSO gene)
        {
            if (gene == null) return _geneticSensitivity;
            
            var geneSensitivity = _geneSpecificSensitivities.Find(gs => gs.Gene == gene);
            return geneSensitivity != null ? geneSensitivity.SensitivityModifier : _geneticSensitivity;
        }
        
        private float CalculatePhenotypicPlasticity(ProjectChimera.Data.Cultivation.EnvironmentalConditions environment)
        {
            // Environmental stress can increase phenotypic plasticity
            float stressLevel = CalculateOverallStressLevel(environment);
            return Mathf.Lerp(_phenotypicPlasticity, 1f, stressLevel * 0.3f);
        }
        
        private float CalculateOverallStressLevel(ProjectChimera.Data.Cultivation.EnvironmentalConditions environment)
        {
            // Simplified stress calculation - could be more sophisticated
            float tempStress = Mathf.Max(0f, Mathf.Abs(environment.Temperature - 23f) / 10f);
            float humidityStress = Mathf.Max(0f, Mathf.Abs(environment.Humidity - 55f) / 30f);
            float lightStress = environment.LightIntensity < 300f ? (300f - environment.LightIntensity) / 300f : 0f;
            
            return Mathf.Clamp01((tempStress + humidityStress + lightStress) / 3f);
        }
        
        private float CalculateEpistaticInteraction(float geneticValue, float environmentalEffect)
        {
            // Complex interaction where environment can mask or enhance genetic effects
            if (environmentalEffect < 0.5f)
                return geneticValue * 0.5f; // Environment suppresses genetic expression
            else if (environmentalEffect > 1.5f)
                return geneticValue * 1.5f; // Environment enhances genetic expression
            else
                return geneticValue * environmentalEffect;
        }
        
        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(_profileName))
            {
                Debug.LogError($"GxE Profile {name}: Profile name cannot be empty", this);
                isValid = false;
            }
            
            if (_responseCurves.Count == 0)
            {
                Debug.LogWarning($"GxE Profile {name}: No response curves defined", this);
                isValid = false;
            }
            
            if (!_appliesToAllTraits && _targetTraits.Count == 0)
            {
                Debug.LogWarning($"GxE Profile {name}: No target traits specified", this);
                isValid = false;
            }
            
            return isValid;
        }
    }
    
    [System.Serializable]
    public class EnvironmentalResponseCurve
    {
        public EnvironmentalFactor EnvironmentalFactor;
        public AnimationCurve ResponseCurve;
        [Range(0f, 2f)] public float EffectStrength = 1f;
        public float ThresholdValue = 0f; // For threshold-type interactions
        [TextArea(2, 3)] public string ResponseDescription;
    }
    
    [System.Serializable]
    public class GeneticSensitivityModifier
    {
        public GeneDefinitionSO Gene;
        [Range(0f, 3f)] public float SensitivityModifier = 1f;
        [TextArea(2, 3)] public string SensitivityDescription;
    }
    
    [System.Serializable]
    public class InteractionThreshold
    {
        public EnvironmentalFactor Factor;
        public float ThresholdValue;
        public float BelowThresholdModifier = 1f;
        public float AboveThresholdModifier = 1f;
        [TextArea(2, 3)] public string ThresholdDescription;
    }
    
    [System.Serializable]
    public class StressResponseCurve
    {
        public TraitType AffectedTrait;
        public AnimationCurve ResponseCurve;
        [Range(0f, 1f)] public float MinResponse = 0.1f;
        public float AdaptationThreshold = 0.5f;
        [TextArea(2, 3)] public string StressDescription;
    }
    
    public enum GxEInteractionType
    {
        Additive,          // Environmental effect adds to genetic value
        Multiplicative,    // Environmental effect multiplies genetic value
        Threshold,         // Environmental effect triggers at specific threshold
        Epistatic,         // Complex interaction where environment masks/enhances genes
        Compensatory,      // Environment compensates for genetic deficiencies
        Synergistic       // Environment and genetics work together for enhanced effect
    }
}