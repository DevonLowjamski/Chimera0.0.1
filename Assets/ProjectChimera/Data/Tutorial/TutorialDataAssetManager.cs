using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Tutorial
{
    /// <summary>
    /// Tutorial data asset manager for Project Chimera.
    /// Manages creation and organization of tutorial ScriptableObject assets.
    /// </summary>
    [CreateAssetMenu(fileName = "TutorialDataAssetManager", menuName = "Project Chimera/Tutorial/Tutorial Data Asset Manager")]
    public class TutorialDataAssetManager : ChimeraDataSO
    {
        [Header("Tutorial Sequence Assets")]
        [SerializeField] private List<TutorialSequenceSO> _availableSequences;
        [SerializeField] private TutorialSequenceSO _onboardingSequence;
        [SerializeField] private TutorialSequenceSO _cultivationSequence;
        [SerializeField] private TutorialSequenceSO _geneticsSequence;
        [SerializeField] private TutorialSequenceSO _economicsSequence;
        
        [Header("Tutorial Step Collections")]
        [SerializeField] private List<TutorialStepSO> _onboardingSteps;
        [SerializeField] private List<TutorialStepSO> _cultivationSteps;
        [SerializeField] private List<TutorialStepSO> _geneticsSteps;
        [SerializeField] private List<TutorialStepSO> _economicsSteps;
        
        [Header("Tutorial Configuration Assets")]
        [SerializeField] private TutorialConfigurationSO _globalConfiguration;
        [SerializeField] private OnboardingStepDefinitions _onboardingDefinitions;
        [SerializeField] private CultivationTutorialStepDefinitions _cultivationDefinitions;
        [SerializeField] private GeneticsTutorialStepDefinitions _geneticsDefinitions;
        [SerializeField] private EconomicsTutorialStepDefinitions _economicsDefinitions;
        
        [Header("Asset Creation Settings")]
        [SerializeField] private bool _autoCreateMissingAssets = true;
        [SerializeField] private string _assetBasePath = "Assets/ProjectChimera/Data/Tutorial/Generated/";
        
        // Properties
        public List<TutorialSequenceSO> AvailableSequences => _availableSequences;
        public TutorialSequenceSO OnboardingSequence => _onboardingSequence;
        public TutorialSequenceSO CultivationSequence => _cultivationSequence;
        public TutorialSequenceSO GeneticsSequence => _geneticsSequence;
        public TutorialSequenceSO EconomicsSequence => _economicsSequence;
        public TutorialConfigurationSO GlobalConfiguration => _globalConfiguration;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            // Initialize collections if null
            if (_availableSequences == null) _availableSequences = new List<TutorialSequenceSO>();
            if (_onboardingSteps == null) _onboardingSteps = new List<TutorialStepSO>();
            if (_cultivationSteps == null) _cultivationSteps = new List<TutorialStepSO>();
            if (_geneticsSteps == null) _geneticsSteps = new List<TutorialStepSO>();
            if (_economicsSteps == null) _economicsSteps = new List<TutorialStepSO>();
            
            // Auto-create missing assets if enabled
            if (_autoCreateMissingAssets && Application.isEditor)
            {
                ValidateAndCreateAssets();
            }
        }
        
        /// <summary>
        /// Validate and create missing tutorial assets
        /// </summary>
        [ContextMenu("Validate and Create Assets")]
        public void ValidateAndCreateAssets()
        {
            #if UNITY_EDITOR
            CreateSequenceAssets();
            CreateStepAssets();
            UpdateSequenceReferences();
            
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            
            Debug.Log("Tutorial data assets validated and created");
            #endif
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// Create tutorial sequence assets
        /// </summary>
        private void CreateSequenceAssets()
        {
            // Create onboarding sequence
            if (_onboardingSequence == null)
            {
                _onboardingSequence = CreateSequenceAsset("OnboardingSequence", "Onboarding Tutorial", 
                    "Complete first-time user experience and basic setup", TutorialDifficultyLevel.Beginner);
            }
            
            // Create cultivation sequence
            if (_cultivationSequence == null)
            {
                _cultivationSequence = CreateSequenceAsset("CultivationSequence", "Cultivation Mastery", 
                    "Master advanced growing techniques and plant care", TutorialDifficultyLevel.Intermediate);
            }
            
            // Create genetics sequence
            if (_geneticsSequence == null)
            {
                _geneticsSequence = CreateSequenceAsset("GeneticsSequence", "Breeding & Genetics", 
                    "Learn professional breeding techniques and genetic analysis", TutorialDifficultyLevel.Advanced);
            }
            
            // Create economics sequence
            if (_economicsSequence == null)
            {
                _economicsSequence = CreateSequenceAsset("EconomicsSequence", "Business & Economics", 
                    "Master cannabis business operations and financial management", TutorialDifficultyLevel.Expert);
            }
            
            // Update available sequences list
            _availableSequences.Clear();
            if (_onboardingSequence != null) _availableSequences.Add(_onboardingSequence);
            if (_cultivationSequence != null) _availableSequences.Add(_cultivationSequence);
            if (_geneticsSequence != null) _availableSequences.Add(_geneticsSequence);
            if (_economicsSequence != null) _availableSequences.Add(_economicsSequence);
        }
        
        /// <summary>
        /// Create a tutorial sequence asset
        /// </summary>
        private TutorialSequenceSO CreateSequenceAsset(string fileName, string sequenceName, string description, TutorialDifficultyLevel difficulty)
        {
            var sequence = ScriptableObject.CreateInstance<TutorialSequenceSO>();
            sequence.SetSequenceId(fileName.ToLower().Replace("sequence", "_tutorial"));
            sequence.SetSequenceName(sequenceName);
            sequence.SetDescription(description);
            sequence.SetDifficultyLevel(difficulty);
            sequence.SetIsRequired(fileName == "OnboardingSequence");
            
            var assetPath = $"{_assetBasePath}Sequences/{fileName}.asset";
            CreateDirectoryIfNeeded(assetPath);
            UnityEditor.AssetDatabase.CreateAsset(sequence, assetPath);
            
            Debug.Log($"Created tutorial sequence asset: {assetPath}");
            return sequence;
        }
        
        /// <summary>
        /// Create tutorial step assets from definitions
        /// </summary>
        private void CreateStepAssets()
        {
            CreateOnboardingStepAssets();
            CreateCultivationStepAssets();
            CreateGeneticsStepAssets();
            CreateEconomicsStepAssets();
        }
        
        /// <summary>
        /// Create onboarding step assets
        /// </summary>
        private void CreateOnboardingStepAssets()
        {
            if (_onboardingDefinitions == null) return;
            
            _onboardingSteps.Clear();
            var allSteps = _onboardingDefinitions.GetAllOnboardingSteps();
            
            foreach (var stepData in allSteps)
            {
                var stepAsset = CreateStepAsset(stepData, "Onboarding");
                if (stepAsset != null)
                {
                    _onboardingSteps.Add(stepAsset);
                }
            }
            
            Debug.Log($"Created {_onboardingSteps.Count} onboarding step assets");
        }
        
        /// <summary>
        /// Create cultivation step assets
        /// </summary>
        private void CreateCultivationStepAssets()
        {
            if (_cultivationDefinitions == null) return;
            
            _cultivationSteps.Clear();
            var allSteps = _cultivationDefinitions.GetAllCultivationSteps();
            
            foreach (var stepData in allSteps)
            {
                var stepAsset = CreateStepAsset(stepData, "Cultivation");
                if (stepAsset != null)
                {
                    _cultivationSteps.Add(stepAsset);
                }
            }
            
            Debug.Log($"Created {_cultivationSteps.Count} cultivation step assets");
        }
        
        /// <summary>
        /// Create genetics step assets
        /// </summary>
        private void CreateGeneticsStepAssets()
        {
            if (_geneticsDefinitions == null) return;
            
            _geneticsSteps.Clear();
            var allSteps = _geneticsDefinitions.GetAllGeneticsSteps();
            
            foreach (var stepData in allSteps)
            {
                var stepAsset = CreateStepAsset(stepData, "Genetics");
                if (stepAsset != null)
                {
                    _geneticsSteps.Add(stepAsset);
                }
            }
            
            Debug.Log($"Created {_geneticsSteps.Count} genetics step assets");
        }
        
        /// <summary>
        /// Create economics step assets
        /// </summary>
        private void CreateEconomicsStepAssets()
        {
            if (_economicsDefinitions == null) return;
            
            _economicsSteps.Clear();
            var allSteps = _economicsDefinitions.GetAllEconomicsSteps();
            
            foreach (var stepData in allSteps)
            {
                var stepAsset = CreateStepAsset(stepData, "Economics");
                if (stepAsset != null)
                {
                    _economicsSteps.Add(stepAsset);
                }
            }
            
            Debug.Log($"Created {_economicsSteps.Count} economics step assets");
        }
        
        /// <summary>
        /// Create a tutorial step asset from step data
        /// </summary>
        private TutorialStepSO CreateStepAsset(TutorialStepData stepData, string category)
        {
            var stepAsset = ScriptableObject.CreateInstance<TutorialStepSO>();
            
            // Set basic properties
            stepAsset.SetStepId(stepData.StepId);
            stepAsset.SetTitle(stepData.Title);
            stepAsset.SetShortDescription(stepData.Description);
            stepAsset.InstructionText = stepData.DetailedInstructions;
            
            // Set step configuration
            stepAsset.SetStepType(stepData.StepType);
            stepAsset.SetValidationType(stepData.ValidationType);
            stepAsset.SetValidationTarget(stepData.ValidationTarget);
            stepAsset.SetTimeoutDuration(stepData.TimeoutDuration);
            stepAsset.SetCanSkip(stepData.CanSkip);
            
            // Create asset
            var fileName = $"{stepData.StepId.Replace("_", "")}.asset";
            var assetPath = $"{_assetBasePath}Steps/{category}/{fileName}";
            CreateDirectoryIfNeeded(assetPath);
            
            // Check if asset already exists
            var existingAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TutorialStepSO>(assetPath);
            if (existingAsset != null)
            {
                return existingAsset;
            }
            
            UnityEditor.AssetDatabase.CreateAsset(stepAsset, assetPath);
            return stepAsset;
        }
        
        /// <summary>
        /// Update sequence references with step assets
        /// </summary>
        private void UpdateSequenceReferences()
        {
            UpdateSequenceSteps(_onboardingSequence, _onboardingSteps);
            UpdateSequenceSteps(_cultivationSequence, _cultivationSteps);
            UpdateSequenceSteps(_geneticsSequence, _geneticsSteps);
            UpdateSequenceSteps(_economicsSequence, _economicsSteps);
        }
        
        /// <summary>
        /// Update sequence with step references
        /// </summary>
        private void UpdateSequenceSteps(TutorialSequenceSO sequence, List<TutorialStepSO> steps)
        {
            if (sequence == null || steps == null) return;
            
            sequence.SetSteps(steps);
            sequence.SetStepCount(steps.Count);
            sequence.SetEstimatedDuration(CalculateSequenceDuration(steps));
            
            UnityEditor.EditorUtility.SetDirty(sequence);
        }
        
        /// <summary>
        /// Calculate sequence duration from steps
        /// </summary>
        private float CalculateSequenceDuration(List<TutorialStepSO> steps)
        {
            float totalDuration = 0f;
            
            foreach (var step in steps)
            {
                // Base time per step (reading/interaction time)
                totalDuration += 30f; // 30 seconds base per step
                
                // Add timeout duration if specified
                if (step.TimeoutDuration > 0)
                {
                    totalDuration += step.TimeoutDuration;
                }
                
                // Add complexity modifiers based on step type
                switch (step.StepType)
                {
                    case TutorialStepType.Introduction:
                        totalDuration += 15f;
                        break;
                    case TutorialStepType.Information:
                        totalDuration += 20f;
                        break;
                    case TutorialStepType.Interaction:
                        totalDuration += 45f;
                        break;
                    case TutorialStepType.Assessment:
                        totalDuration += 120f;
                        break;
                    case TutorialStepType.Problem_Solving:
                        totalDuration += 90f;
                        break;
                    case TutorialStepType.Project:
                        totalDuration += 300f;
                        break;
                }
            }
            
            return totalDuration / 60f; // Convert to minutes
        }
        
        /// <summary>
        /// Create directory if it doesn't exist
        /// </summary>
        private void CreateDirectoryIfNeeded(string assetPath)
        {
            var directory = System.IO.Path.GetDirectoryName(assetPath);
            if (!UnityEditor.AssetDatabase.IsValidFolder(directory))
            {
                var parentDir = System.IO.Path.GetDirectoryName(directory);
                var folderName = System.IO.Path.GetFileName(directory);
                
                if (!UnityEditor.AssetDatabase.IsValidFolder(parentDir))
                {
                    CreateDirectoryIfNeeded(parentDir + "/dummy.asset");
                }
                
                UnityEditor.AssetDatabase.CreateFolder(parentDir, folderName);
            }
        }
        #endif
        
        /// <summary>
        /// Get sequence by ID
        /// </summary>
        public TutorialSequenceSO GetSequenceById(string sequenceId)
        {
            return _availableSequences.FirstOrDefault(s => s.SequenceId == sequenceId);
        }
        
        /// <summary>
        /// Get steps for sequence
        /// </summary>
        public List<TutorialStepSO> GetStepsForSequence(string sequenceId)
        {
            switch (sequenceId.ToLower())
            {
                case "onboarding_tutorial":
                    return _onboardingSteps;
                case "cultivation_tutorial":
                    return _cultivationSteps;
                case "genetics_tutorial":
                    return _geneticsSteps;
                case "economics_tutorial":
                    return _economicsSteps;
                default:
                    return new List<TutorialStepSO>();
            }
        }
        
        /// <summary>
        /// Get step by ID
        /// </summary>
        public TutorialStepSO GetStepById(string stepId)
        {
            var allSteps = new List<TutorialStepSO>();
            allSteps.AddRange(_onboardingSteps);
            allSteps.AddRange(_cultivationSteps);
            allSteps.AddRange(_geneticsSteps);
            allSteps.AddRange(_economicsSteps);
            
            return allSteps.FirstOrDefault(s => s.StepId == stepId);
        }
        
        /// <summary>
        /// Get total tutorial count
        /// </summary>
        public int GetTotalStepCount()
        {
            return _onboardingSteps.Count + _cultivationSteps.Count + _geneticsSteps.Count + _economicsSteps.Count;
        }
        
        /// <summary>
        /// Get tutorial statistics
        /// </summary>
        public TutorialDataStatistics GetTutorialStatistics()
        {
            var stats = new TutorialDataStatistics();
            
            stats.TotalSequences = _availableSequences.Count;
            stats.TotalSteps = GetTotalStepCount();
            stats.OnboardingSteps = _onboardingSteps.Count;
            stats.CultivationSteps = _cultivationSteps.Count;
            stats.GeneticsSteps = _geneticsSteps.Count;
            stats.EconomicsSteps = _economicsSteps.Count;
            
            // Calculate total estimated duration
            stats.TotalEstimatedDuration = 0f;
            foreach (var sequence in _availableSequences)
            {
                // Calculate estimated duration from steps
                var stepDuration = sequence.TutorialSteps.Sum(step => step != null ? step.TimeoutDuration * 0.5f : 0f);
                stats.TotalEstimatedDuration += stepDuration;
            }
            
            return stats;
        }
        
        /// <summary>
        /// Validate data integrity
        /// </summary>
        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;
            
            // Validate sequences
            foreach (var sequence in _availableSequences)
            {
                if (sequence == null)
                {
                    LogError("Found null sequence in available sequences");
                    isValid = false;
                    continue;
                }
                
                if (string.IsNullOrEmpty(sequence.SequenceId))
                {
                    LogError($"Sequence {sequence.SequenceName} has empty ID");
                    isValid = false;
                }
                
                if (sequence.StepCount != sequence.TutorialSteps.Count)
                {
                    LogWarning($"Sequence {sequence.SequenceId} step count mismatch: {sequence.StepCount} vs {sequence.TutorialSteps.Count}");
                }
            }
            
            // Validate step collections
            ValidateStepCollection(_onboardingSteps, "Onboarding");
            ValidateStepCollection(_cultivationSteps, "Cultivation");
            ValidateStepCollection(_geneticsSteps, "Genetics");
            ValidateStepCollection(_economicsSteps, "Economics");
            
            var stats = GetTutorialStatistics();
            Debug.Log($"Tutorial data validation complete: {stats.TotalSequences} sequences, {stats.TotalSteps} steps, {stats.TotalEstimatedDuration:F1} minutes total");
            
            return isValid;
        }
        
        /// <summary>
        /// Validate step collection
        /// </summary>
        private void ValidateStepCollection(List<TutorialStepSO> steps, string collectionName)
        {
            var stepIds = new HashSet<string>();
            
            foreach (var step in steps)
            {
                if (step == null)
                {
                    LogError($"Found null step in {collectionName} collection");
                    continue;
                }
                
                if (string.IsNullOrEmpty(step.StepId))
                {
                    LogError($"Step in {collectionName} collection has empty ID");
                    continue;
                }
                
                if (stepIds.Contains(step.StepId))
                {
                    LogError($"Duplicate step ID in {collectionName} collection: {step.StepId}");
                }
                else
                {
                    stepIds.Add(step.StepId);
                }
            }
        }
    }
    
    /// <summary>
    /// Tutorial data statistics
    /// </summary>
    [System.Serializable]
    public struct TutorialDataStatistics
    {
        public int TotalSequences;
        public int TotalSteps;
        public int OnboardingSteps;
        public int CultivationSteps;
        public int GeneticsSteps;
        public int EconomicsSteps;
        public float TotalEstimatedDuration;
    }
}