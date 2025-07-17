using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core.Logging;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Advanced data validation framework for Project Chimera.
    /// Provides comprehensive validation, consistency checks, cross-reference validation,
    /// and automated data repair capabilities.
    /// 
    /// Phase 3.1 Enhanced Integration Feature - Advanced Data Validation and Consistency Checks
    /// </summary>
    public class AdvancedDataValidationFramework : MonoBehaviour
    {
        [Header("Validation Configuration")]
        [SerializeField] private bool _enableRealTimeValidation = true;
        [SerializeField] private bool _enableCrossReferenceValidation = true;
        [SerializeField] private bool _enableAutomaticRepair = false;
        [SerializeField] private float _validationIntervalSeconds = 30f;
        [SerializeField] private int _maxValidationErrors = 100;

        // Validation data and tracking
        private Dictionary<Type, List<ChimeraScriptableObject>> _registeredAssets = new Dictionary<Type, List<ChimeraScriptableObject>>();
        private List<ValidationResult> _validationResults = new List<ValidationResult>();
        private Dictionary<string, CrossReferenceRule> _crossReferenceRules = new Dictionary<string, CrossReferenceRule>();
        private ValidationMetrics _metrics = new ValidationMetrics();
        private float _lastValidationTime;

        public static AdvancedDataValidationFramework Instance { get; private set; }

        // Events for validation notifications
        public static event System.Action<ValidationResult> OnValidationIssueDetected;
        public static event System.Action<ValidationSummary> OnValidationCompleted;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeValidationFramework();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Validation result for individual assets or cross-references.
        /// </summary>
        [System.Serializable]
        public class ValidationResult
        {
            public string AssetName;
            public string AssetType;
            public ValidationSeverity Severity;
            public string ValidationRule;
            public string ErrorMessage;
            public string SuggestedFix;
            public bool CanAutoRepair;
            public DateTime DetectedAt;
            public Vector2 InspectorPosition; // For UI highlighting

            public bool IsError => Severity == ValidationSeverity.Error;
            public bool IsWarning => Severity == ValidationSeverity.Warning;
        }

        /// <summary>
        /// Cross-reference validation rule definition.
        /// </summary>
        public class CrossReferenceRule
        {
            public string RuleName;
            public Type SourceType;
            public Type TargetType;
            public string SourcePropertyName;
            public string TargetPropertyName;
            public bool IsRequired;
            public ValidationSeverity SeverityIfMissing;
            public Func<object, object, bool> CustomValidator;
        }

        /// <summary>
        /// Validation metrics for monitoring system health.
        /// </summary>
        [System.Serializable]
        public class ValidationMetrics
        {
            public int TotalAssetsValidated;
            public int ErrorsDetected;
            public int WarningsDetected;
            public int AutoRepairsPerformed;
            public float LastValidationDuration;
            public DateTime LastValidationTime;
            public Dictionary<string, int> ErrorsByCategory = new Dictionary<string, int>();
        }

        /// <summary>
        /// Validation summary for reporting.
        /// </summary>
        public class ValidationSummary
        {
            public ValidationMetrics Metrics;
            public List<ValidationResult> CriticalIssues;
            public List<ValidationResult> AllIssues;
            public bool ValidationPassed;
            public string Summary;
        }

        public enum ValidationSeverity
        {
            Info,
            Warning,
            Error,
            Critical
        }

        /// <summary>
        /// Initialize the validation framework with built-in rules.
        /// </summary>
        private void InitializeValidationFramework()
        {
            ChimeraLogger.Log("Validation", "Advanced Data Validation Framework initialized", this);
            
            // Register built-in cross-reference rules
            RegisterBuiltInCrossReferenceRules();
            
            // Discover and register all existing assets
            DiscoverAndRegisterAssets();
            
            // Start real-time validation if enabled
            if (_enableRealTimeValidation)
            {
                InvokeRepeating(nameof(PerformScheduledValidation), _validationIntervalSeconds, _validationIntervalSeconds);
            }
        }

        /// <summary>
        /// Register a ScriptableObject asset for validation.
        /// </summary>
        public void RegisterAsset(ChimeraScriptableObject asset)
        {
            if (asset == null) return;

            var assetType = asset.GetType();
            
            if (!_registeredAssets.ContainsKey(assetType))
            {
                _registeredAssets[assetType] = new List<ChimeraScriptableObject>();
            }

            if (!_registeredAssets[assetType].Contains(asset))
            {
                _registeredAssets[assetType].Add(asset);
                
                if (_enableRealTimeValidation)
                {
                    ValidateAsset(asset);
                }
            }
        }

        /// <summary>
        /// Unregister an asset from validation.
        /// </summary>
        public void UnregisterAsset(ChimeraScriptableObject asset)
        {
            if (asset == null) return;

            var assetType = asset.GetType();
            if (_registeredAssets.ContainsKey(assetType))
            {
                _registeredAssets[assetType].Remove(asset);
            }

            // Remove validation results for this asset
            _validationResults.RemoveAll(result => result.AssetName == asset.name);
        }

        /// <summary>
        /// Perform comprehensive validation of all registered assets.
        /// </summary>
        public ValidationSummary ValidateAllAssets()
        {
            var startTime = Time.realtimeSinceStartup;
            _validationResults.Clear();
            _metrics = new ValidationMetrics { LastValidationTime = DateTime.Now };

            ChimeraLogger.Log("Validation", "Starting comprehensive validation of all assets", this);

            // Validate individual assets
            foreach (var assetTypeGroup in _registeredAssets)
            {
                foreach (var asset in assetTypeGroup.Value)
                {
                    ValidateAsset(asset);
                    _metrics.TotalAssetsValidated++;
                }
            }

            // Perform cross-reference validation
            if (_enableCrossReferenceValidation)
            {
                PerformCrossReferenceValidation();
            }

            // Calculate metrics
            _metrics.LastValidationDuration = Time.realtimeSinceStartup - startTime;
            _metrics.ErrorsDetected = _validationResults.Count(r => r.IsError);
            _metrics.WarningsDetected = _validationResults.Count(r => r.IsWarning);

            // Count errors by category
            _metrics.ErrorsByCategory.Clear();
            foreach (var result in _validationResults)
            {
                if (!_metrics.ErrorsByCategory.ContainsKey(result.ValidationRule))
                {
                    _metrics.ErrorsByCategory[result.ValidationRule] = 0;
                }
                _metrics.ErrorsByCategory[result.ValidationRule]++;
            }

            // Attempt automatic repairs if enabled
            if (_enableAutomaticRepair)
            {
                PerformAutomaticRepairs();
            }

            var summary = GenerateValidationSummary();
            OnValidationCompleted?.Invoke(summary);

            ChimeraLogger.Log("Validation", $"Validation completed: {_metrics.ErrorsDetected} errors, {_metrics.WarningsDetected} warnings in {_metrics.LastValidationDuration:F2}s", this);

            return summary;
        }

        /// <summary>
        /// Validate a single asset with comprehensive checks.
        /// </summary>
        private void ValidateAsset(ChimeraScriptableObject asset)
        {
            if (asset == null) return;

            try
            {
                // Basic validation from asset itself
                if (!asset.ValidateData())
                {
                    AddValidationResult(asset, "Basic Validation", "Asset failed basic validation", ValidationSeverity.Error);
                }

                // Type-specific validation
                PerformTypeSpecificValidation(asset);
                
                // Reference validation
                ValidateAssetReferences(asset);
                
                // Data consistency validation
                ValidateDataConsistency(asset);
            }
            catch (Exception ex)
            {
                AddValidationResult(asset, "Validation Exception", $"Exception during validation: {ex.Message}", ValidationSeverity.Critical);
            }
        }

        /// <summary>
        /// Perform type-specific validation based on asset type.
        /// </summary>
        private void PerformTypeSpecificValidation(ChimeraScriptableObject asset)
        {
            var assetType = asset.GetType().Name;

            switch (assetType)
            {
                case "PlantStrainSO":
                    ValidatePlantStrain(asset);
                    break;
                case "GeneDefinitionSO":
                    ValidateGeneDefinition(asset);
                    break;
                case "EquipmentDataSO":
                    ValidateEquipmentData(asset);
                    break;
                case "EnvironmentalParametersSO":
                    ValidateEnvironmentalParameters(asset);
                    break;
                case "MarketProductSO":
                    ValidateMarketProduct(asset);
                    break;
                // Add more type-specific validations as needed
            }
        }

        /// <summary>
        /// Validate PlantStrainSO specific requirements.
        /// </summary>
        private void ValidatePlantStrain(ChimeraScriptableObject asset)
        {
            // Use reflection to get properties since we don't have direct access to PlantStrainSO
            var strainType = asset.GetType();
            
            // Check for required genetic properties
            var geneticProfileProperty = strainType.GetProperty("GeneticProfile");
            if (geneticProfileProperty != null)
            {
                var geneticProfile = geneticProfileProperty.GetValue(asset);
                if (geneticProfile == null)
                {
                    AddValidationResult(asset, "Plant Strain Validation", "Plant strain missing genetic profile", ValidationSeverity.Error, "Assign a genetic profile to this strain");
                }
            }

            // Validate cannabinoid profiles
            var cannabinoidProperty = strainType.GetProperty("CannabinoidProfile");
            if (cannabinoidProperty != null)
            {
                var cannabinoidProfile = cannabinoidProperty.GetValue(asset);
                if (cannabinoidProfile == null)
                {
                    AddValidationResult(asset, "Plant Strain Validation", "Plant strain missing cannabinoid profile", ValidationSeverity.Warning, "Add cannabinoid profile for accurate simulation");
                }
            }

            // Validate growth characteristics
            ValidateNumericRange(asset, "TypicalHeight", 10f, 500f, "Plant height should be between 10-500cm");
            ValidateNumericRange(asset, "FloweringTime", 30f, 150f, "Flowering time should be between 30-150 days");
        }

        /// <summary>
        /// Validate GeneDefinitionSO specific requirements.
        /// </summary>
        private void ValidateGeneDefinition(ChimeraScriptableObject asset)
        {
            var geneType = asset.GetType();
            
            // Check for valid alleles
            var allelesProperty = geneType.GetProperty("PossibleAlleles");
            if (allelesProperty != null)
            {
                var alleles = allelesProperty.GetValue(asset) as System.Collections.IList;
                if (alleles == null || alleles.Count < 2)
                {
                    AddValidationResult(asset, "Gene Definition Validation", "Gene must have at least 2 alleles", ValidationSeverity.Error, "Add dominant and recessive alleles");
                }
            }

            // Validate gene expression
            var expressionProperty = geneType.GetProperty("ExpressionPattern");
            if (expressionProperty != null)
            {
                var expression = expressionProperty.GetValue(asset);
                if (expression == null)
                {
                    AddValidationResult(asset, "Gene Definition Validation", "Gene missing expression pattern", ValidationSeverity.Warning, "Define how this gene is expressed");
                }
            }
        }

        /// <summary>
        /// Validate EquipmentDataSO specific requirements.
        /// </summary>
        private void ValidateEquipmentData(ChimeraScriptableObject asset)
        {
            ValidateNumericRange(asset, "PowerConsumption", 0f, 10000f, "Power consumption should be 0-10000W");
            ValidateNumericRange(asset, "PurchaseCost", 0f, 100000f, "Purchase cost should be reasonable");
            ValidateNumericRange(asset, "MaintenanceCost", 0f, 1000f, "Maintenance cost should be reasonable");
        }

        /// <summary>
        /// Validate environmental parameters.
        /// </summary>
        private void ValidateEnvironmentalParameters(ChimeraScriptableObject asset)
        {
            ValidateNumericRange(asset, "OptimalTemperature", 15f, 35f, "Optimal temperature should be 15-35°C");
            ValidateNumericRange(asset, "OptimalHumidity", 30f, 80f, "Optimal humidity should be 30-80%");
            ValidateNumericRange(asset, "OptimalCO2", 300f, 2000f, "Optimal CO2 should be 300-2000 ppm");
        }

        /// <summary>
        /// Validate market product data.
        /// </summary>
        private void ValidateMarketProduct(ChimeraScriptableObject asset)
        {
            ValidateNumericRange(asset, "BasePrice", 0.1f, 1000f, "Base price should be reasonable");
            ValidateNumericRange(asset, "QualityMultiplier", 0.5f, 3f, "Quality multiplier should be 0.5-3x");
        }

        /// <summary>
        /// Validate numeric property ranges.
        /// </summary>
        private void ValidateNumericRange(ChimeraScriptableObject asset, string propertyName, float min, float max, string errorMessage)
        {
            var property = asset.GetType().GetProperty(propertyName);
            if (property != null && property.PropertyType == typeof(float))
            {
                var value = (float)property.GetValue(asset);
                if (value < min || value > max)
                {
                    AddValidationResult(asset, "Range Validation", $"{propertyName}: {errorMessage} (current: {value})", ValidationSeverity.Warning, $"Set {propertyName} between {min} and {max}");
                }
            }
        }

        /// <summary>
        /// Validate asset references for null or missing references.
        /// </summary>
        private void ValidateAssetReferences(ChimeraScriptableObject asset)
        {
            var assetType = asset.GetType();
            var properties = assetType.GetProperties();

            foreach (var property in properties)
            {
                // Check ScriptableObject references
                if (typeof(ScriptableObject).IsAssignableFrom(property.PropertyType))
                {
                    var value = property.GetValue(asset);
                    if (value == null && IsRequiredReference(assetType, property.Name))
                    {
                        AddValidationResult(asset, "Reference Validation", $"Missing required reference: {property.Name}", ValidationSeverity.Error, $"Assign a {property.PropertyType.Name} to {property.Name}");
                    }
                }
                
                // Check array/list references
                if (property.PropertyType.IsArray || (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    var value = property.GetValue(asset) as System.Collections.IList;
                    if (value != null)
                    {
                        for (int i = 0; i < value.Count; i++)
                        {
                            if (value[i] == null)
                            {
                                AddValidationResult(asset, "Reference Validation", $"Null reference in {property.Name}[{i}]", ValidationSeverity.Warning, $"Remove null entry or assign valid reference");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determine if a reference property is required for the asset type.
        /// </summary>
        private bool IsRequiredReference(Type assetType, string propertyName)
        {
            // Define required references by asset type
            var requiredReferences = new Dictionary<string, string[]>
            {
                ["PlantStrainSO"] = new[] { "GeneticProfile" },
                ["GeneDefinitionSO"] = new[] { "PossibleAlleles" },
                ["EquipmentDataSO"] = new[] { "EquipmentType" },
                ["MarketProductSO"] = new[] { "ProductCategory" }
            };

            if (requiredReferences.TryGetValue(assetType.Name, out var required))
            {
                return required.Contains(propertyName);
            }

            return false;
        }

        /// <summary>
        /// Validate data consistency within the asset.
        /// </summary>
        private void ValidateDataConsistency(ChimeraScriptableObject asset)
        {
            var assetType = asset.GetType();
            
            // Check for logical inconsistencies based on asset type
            if (assetType.Name == "PlantStrainSO")
            {
                ValidatePlantStrainConsistency(asset);
            }
            else if (assetType.Name == "EquipmentDataSO")
            {
                ValidateEquipmentConsistency(asset);
            }
        }

        /// <summary>
        /// Validate plant strain data consistency.
        /// </summary>
        private void ValidatePlantStrainConsistency(ChimeraScriptableObject asset)
        {
            // Example: Check if flowering time matches strain type
            var strainType = GetPropertyValue<string>(asset, "StrainType");
            var floweringTime = GetPropertyValue<float>(asset, "FloweringTime");

            if (strainType == "Autoflower" && floweringTime > 100f)
            {
                AddValidationResult(asset, "Consistency Validation", "Autoflower strains typically flower in less than 100 days", ValidationSeverity.Warning, "Reduce flowering time or change strain type");
            }
        }

        /// <summary>
        /// Validate equipment data consistency.
        /// </summary>
        private void ValidateEquipmentConsistency(ChimeraScriptableObject asset)
        {
            var powerConsumption = GetPropertyValue<float>(asset, "PowerConsumption");
            var efficiency = GetPropertyValue<float>(asset, "Efficiency");

            if (powerConsumption > 1000f && efficiency < 0.8f)
            {
                AddValidationResult(asset, "Consistency Validation", "High power equipment should have high efficiency", ValidationSeverity.Warning, "Increase efficiency or reduce power consumption");
            }
        }

        /// <summary>
        /// Perform cross-reference validation between related assets.
        /// </summary>
        private void PerformCrossReferenceValidation()
        {
            foreach (var rule in _crossReferenceRules.Values)
            {
                ValidateCrossReference(rule);
            }
        }

        /// <summary>
        /// Validate a specific cross-reference rule.
        /// </summary>
        private void ValidateCrossReference(CrossReferenceRule rule)
        {
            if (!_registeredAssets.ContainsKey(rule.SourceType) || !_registeredAssets.ContainsKey(rule.TargetType))
            {
                return; // One or both types not registered
            }

            var sourceAssets = _registeredAssets[rule.SourceType];
            var targetAssets = _registeredAssets[rule.TargetType];

            foreach (var sourceAsset in sourceAssets)
            {
                var sourceValue = GetPropertyValue<object>(sourceAsset, rule.SourcePropertyName);
                if (sourceValue == null && rule.IsRequired)
                {
                    AddValidationResult(sourceAsset, "Cross-Reference Validation", $"Missing required reference to {rule.TargetType.Name}", rule.SeverityIfMissing);
                    continue;
                }

                if (sourceValue != null)
                {
                    var isValid = false;
                    
                    // Check if reference exists in target assets
                    foreach (var targetAsset in targetAssets)
                    {
                        var targetValue = GetPropertyValue<object>(targetAsset, rule.TargetPropertyName);
                        
                        if (rule.CustomValidator != null)
                        {
                            if (rule.CustomValidator(sourceValue, targetValue))
                            {
                                isValid = true;
                                break;
                            }
                        }
                        else if (sourceValue.Equals(targetValue) || sourceValue == targetAsset)
                        {
                            isValid = true;
                            break;
                        }
                    }

                    if (!isValid)
                    {
                        AddValidationResult(sourceAsset, "Cross-Reference Validation", $"Invalid reference to {rule.TargetType.Name}: {sourceValue}", rule.SeverityIfMissing);
                    }
                }
            }
        }

        /// <summary>
        /// Register built-in cross-reference validation rules.
        /// </summary>
        private void RegisterBuiltInCrossReferenceRules()
        {
            // Example rules - add more based on actual asset relationships
            RegisterCrossReferenceRule("PlantStrain_GeneticProfile", 
                typeof(ChimeraScriptableObject), typeof(ChimeraScriptableObject),
                "GeneticProfile", "name", true, ValidationSeverity.Error);

            RegisterCrossReferenceRule("Equipment_Category",
                typeof(ChimeraScriptableObject), typeof(ChimeraScriptableObject),
                "EquipmentCategory", "name", true, ValidationSeverity.Warning);
        }

        /// <summary>
        /// Register a cross-reference validation rule.
        /// </summary>
        public void RegisterCrossReferenceRule(string ruleName, Type sourceType, Type targetType, 
            string sourceProperty, string targetProperty, bool isRequired, ValidationSeverity severity)
        {
            _crossReferenceRules[ruleName] = new CrossReferenceRule
            {
                RuleName = ruleName,
                SourceType = sourceType,
                TargetType = targetType,
                SourcePropertyName = sourceProperty,
                TargetPropertyName = targetProperty,
                IsRequired = isRequired,
                SeverityIfMissing = severity
            };
        }

        /// <summary>
        /// Discover and register all existing assets in the project.
        /// </summary>
        private void DiscoverAndRegisterAssets()
        {
            var allAssets = Resources.FindObjectsOfTypeAll<ChimeraScriptableObject>();
            foreach (var asset in allAssets)
            {
                RegisterAsset(asset);
            }
            
            ChimeraLogger.Log("Validation", $"Discovered and registered {allAssets.Length} assets for validation", this);
        }

        /// <summary>
        /// Perform scheduled validation for real-time monitoring.
        /// </summary>
        private void PerformScheduledValidation()
        {
            if (Time.realtimeSinceStartup - _lastValidationTime < _validationIntervalSeconds)
            {
                return;
            }

            _lastValidationTime = Time.realtimeSinceStartup;
            
            // Perform lightweight validation
            var criticalIssues = _validationResults.Where(r => r.Severity == ValidationSeverity.Critical).ToList();
            if (criticalIssues.Count > 0)
            {
                ChimeraLogger.LogWarning("Validation", $"Critical validation issues detected: {criticalIssues.Count}", this);
            }
        }

        /// <summary>
        /// Perform automatic repairs on issues that can be auto-fixed.
        /// </summary>
        private void PerformAutomaticRepairs()
        {
            var repairableIssues = _validationResults.Where(r => r.CanAutoRepair).ToList();
            
            foreach (var issue in repairableIssues)
            {
                try
                {
                    if (AttemptAutomaticRepair(issue))
                    {
                        _metrics.AutoRepairsPerformed++;
                        _validationResults.Remove(issue);
                        ChimeraLogger.Log("Validation", $"Auto-repaired: {issue.AssetName} - {issue.ErrorMessage}", this);
                    }
                }
                catch (Exception ex)
                {
                    ChimeraLogger.LogError("Validation", $"Failed to auto-repair {issue.AssetName}: {ex.Message}", this);
                }
            }
        }

        /// <summary>
        /// Attempt to automatically repair a validation issue.
        /// </summary>
        private bool AttemptAutomaticRepair(ValidationResult issue)
        {
            // Implement specific auto-repair logic based on issue type
            switch (issue.ValidationRule)
            {
                case "Range Validation":
                    return AttemptRangeRepair(issue);
                case "Reference Validation":
                    return AttemptReferenceRepair(issue);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Attempt to repair range validation issues.
        /// </summary>
        private bool AttemptRangeRepair(ValidationResult issue)
        {
            // Simple range clamping for numeric values
            // Implementation would depend on specific property types
            return false; // Placeholder
        }

        /// <summary>
        /// Attempt to repair reference validation issues.
        /// </summary>
        private bool AttemptReferenceRepair(ValidationResult issue)
        {
            // Attempt to find and assign appropriate references
            // Implementation would depend on asset relationships
            return false; // Placeholder
        }

        /// <summary>
        /// Add a validation result to the collection.
        /// </summary>
        private void AddValidationResult(ChimeraScriptableObject asset, string rule, string message, ValidationSeverity severity, string suggestedFix = "")
        {
            var result = new ValidationResult
            {
                AssetName = asset.name,
                AssetType = asset.GetType().Name,
                ValidationRule = rule,
                ErrorMessage = message,
                Severity = severity,
                SuggestedFix = suggestedFix,
                DetectedAt = DateTime.Now,
                CanAutoRepair = !string.IsNullOrEmpty(suggestedFix) && severity != ValidationSeverity.Critical
            };

            _validationResults.Add(result);
            OnValidationIssueDetected?.Invoke(result);

            if (_validationResults.Count > _maxValidationErrors)
            {
                ChimeraLogger.LogWarning("Validation", $"Maximum validation errors ({_maxValidationErrors}) reached. Some errors may not be reported.", this);
            }
        }

        /// <summary>
        /// Get property value using reflection with error handling.
        /// </summary>
        private T GetPropertyValue<T>(object obj, string propertyName)
        {
            try
            {
                var property = obj.GetType().GetProperty(propertyName);
                if (property != null && property.CanRead)
                {
                    var value = property.GetValue(obj);
                    if (value is T)
                    {
                        return (T)value;
                    }
                }
            }
            catch (Exception ex)
            {
                ChimeraLogger.LogWarning("Validation", $"Error getting property {propertyName}: {ex.Message}", this);
            }

            return default(T);
        }

        /// <summary>
        /// Generate a comprehensive validation summary.
        /// </summary>
        private ValidationSummary GenerateValidationSummary()
        {
            var criticalIssues = _validationResults.Where(r => r.Severity == ValidationSeverity.Critical).ToList();
            var hasErrors = _validationResults.Any(r => r.IsError);

            return new ValidationSummary
            {
                Metrics = _metrics,
                CriticalIssues = criticalIssues,
                AllIssues = new List<ValidationResult>(_validationResults),
                ValidationPassed = !hasErrors,
                Summary = $"Validated {_metrics.TotalAssetsValidated} assets: {_metrics.ErrorsDetected} errors, {_metrics.WarningsDetected} warnings"
            };
        }

        /// <summary>
        /// Get current validation metrics.
        /// </summary>
        public ValidationMetrics GetValidationMetrics()
        {
            return _metrics;
        }

        /// <summary>
        /// Get all current validation results.
        /// </summary>
        public List<ValidationResult> GetValidationResults()
        {
            return new List<ValidationResult>(_validationResults);
        }

        /// <summary>
        /// Clear all validation results.
        /// </summary>
        public void ClearValidationResults()
        {
            _validationResults.Clear();
            _metrics.ErrorsDetected = 0;
            _metrics.WarningsDetected = 0;
            _metrics.ErrorsByCategory.Clear();
        }

        private void OnDestroy()
        {
            CancelInvoke();
            _registeredAssets.Clear();
            _validationResults.Clear();
            _crossReferenceRules.Clear();
        }
    }
}