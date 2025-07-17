using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Narrative
{
    /// <summary>
    /// Comprehensive story arc library ScriptableObject containing all narrative story arcs for Project Chimera's
    /// campaign system. Features enterprise-grade story management with branching narratives, educational integration,
    /// and scientific accuracy validation for cannabis cultivation storylines.
    /// </summary>
    [CreateAssetMenu(fileName = "New Story Arc Library", menuName = "Project Chimera/Narrative/Story Arc Library", order = 103)]
    public class StoryArcLibrarySO : ChimeraDataSO
    {
        [Header("Story Arc Collections")]
        [SerializeField] private List<StoryArcSO> _noviceGrowerArcs = new List<StoryArcSO>();
        [SerializeField] private List<StoryArcSO> _entrepreneurArcs = new List<StoryArcSO>();
        [SerializeField] private List<StoryArcSO> _masterBreederArcs = new List<StoryArcSO>();
        [SerializeField] private List<StoryArcSO> _communityLeaderArcs = new List<StoryArcSO>();
        [SerializeField] private List<StoryArcSO> _innovationPioneerArcs = new List<StoryArcSO>();
        [SerializeField] private List<StoryArcSO> _sidestoryArcs = new List<StoryArcSO>();
        [SerializeField] private List<StoryArcSO> _eventArcs = new List<StoryArcSO>();
        
        [Header("Arc Dependencies and Progression")]
        [SerializeField] private List<ArcDependency> _arcDependencies = new List<ArcDependency>();
        [SerializeField] private List<ArcProgression> _progressionPaths = new List<ArcProgression>();
        [SerializeField] private List<ArcUnlockCondition> _unlockConditions = new List<ArcUnlockCondition>();
        
        [Header("Educational Integration")]
        [SerializeField] private List<EducationalMapping> _educationalMappings = new List<EducationalMapping>();
        [SerializeField] private List<ScientificValidation> _scientificValidations = new List<ScientificValidation>();
        [SerializeField] private float _minimumEducationalContent = 0.6f;
        
        [Header("Branching and Choices")]
        [SerializeField] private List<BranchingPoint> _majorBranchingPoints = new List<BranchingPoint>();
        [SerializeField] private List<ConsequenceMapping> _consequenceMappings = new List<ConsequenceMapping>();
        [SerializeField] private int _maxSimultaneousArcs = 3;
        
        [Header("Performance Configuration")]
        [SerializeField] private bool _enableArcCaching = true;
        [SerializeField] private int _maxCachedArcs = 10;
        [SerializeField] private float _arcUpdateInterval = 1.0f;
        [SerializeField] private bool _enableProgressiveLoading = true;
        
        // Public Properties
        public IReadOnlyList<StoryArcSO> NoviceGrowerArcs => _noviceGrowerArcs.AsReadOnly();
        public IReadOnlyList<StoryArcSO> EntrepreneurArcs => _entrepreneurArcs.AsReadOnly();
        public IReadOnlyList<StoryArcSO> MasterBreederArcs => _masterBreederArcs.AsReadOnly();
        public IReadOnlyList<StoryArcSO> CommunityLeaderArcs => _communityLeaderArcs.AsReadOnly();
        public IReadOnlyList<StoryArcSO> InnovationPioneerArcs => _innovationPioneerArcs.AsReadOnly();
        public IReadOnlyList<StoryArcSO> SidestoryArcs => _sidestoryArcs.AsReadOnly();
        public IReadOnlyList<StoryArcSO> EventArcs => _eventArcs.AsReadOnly();
        
        public IReadOnlyList<ArcDependency> ArcDependencies => _arcDependencies.AsReadOnly();
        public IReadOnlyList<ArcProgression> ProgressionPaths => _progressionPaths.AsReadOnly();
        public IReadOnlyList<ArcUnlockCondition> UnlockConditions => _unlockConditions.AsReadOnly();
        
        public IReadOnlyList<EducationalMapping> EducationalMappings => _educationalMappings.AsReadOnly();
        public IReadOnlyList<ScientificValidation> ScientificValidations => _scientificValidations.AsReadOnly();
        public float MinimumEducationalContent => _minimumEducationalContent;
        
        public IReadOnlyList<BranchingPoint> MajorBranchingPoints => _majorBranchingPoints.AsReadOnly();
        public IReadOnlyList<ConsequenceMapping> ConsequenceMappings => _consequenceMappings.AsReadOnly();
        public int MaxSimultaneousArcs => _maxSimultaneousArcs;
        
        public bool EnableArcCaching => _enableArcCaching;
        public int MaxCachedArcs => _maxCachedArcs;
        public float ArcUpdateInterval => _arcUpdateInterval;
        public bool EnableProgressiveLoading => _enableProgressiveLoading;
        
        /// <summary>
        /// Get all story arcs in the library
        /// </summary>
        public List<StoryArcSO> GetAllStoryArcs()
        {
            var allArcs = new List<StoryArcSO>();
            allArcs.AddRange(_noviceGrowerArcs);
            allArcs.AddRange(_entrepreneurArcs);
            allArcs.AddRange(_masterBreederArcs);
            allArcs.AddRange(_communityLeaderArcs);
            allArcs.AddRange(_innovationPioneerArcs);
            allArcs.AddRange(_sidestoryArcs);
            allArcs.AddRange(_eventArcs);
            
            return allArcs.Distinct().ToList();
        }
        
        /// <summary>
        /// Find story arc by ID
        /// </summary>
        public StoryArcSO GetStoryArcById(string arcId)
        {
            return GetAllStoryArcs().FirstOrDefault(arc => arc.ArcId == arcId);
        }
        
        /// <summary>
        /// Get story arcs by category
        /// </summary>
        public List<StoryArcSO> GetStoryArcsByCategory(StoryArcCategory category)
        {
            return category switch
            {
                StoryArcCategory.NoviceGrower => _noviceGrowerArcs.ToList(),
                StoryArcCategory.Entrepreneur => _entrepreneurArcs.ToList(),
                StoryArcCategory.MasterBreeder => _masterBreederArcs.ToList(),
                StoryArcCategory.CommunityLeader => _communityLeaderArcs.ToList(),
                StoryArcCategory.InnovationPioneer => _innovationPioneerArcs.ToList(),
                StoryArcCategory.Sidestory => _sidestoryArcs.ToList(),
                StoryArcCategory.Event => _eventArcs.ToList(),
                _ => new List<StoryArcSO>()
            };
        }
        
        /// <summary>
        /// Get available story arcs based on player progress and conditions
        /// </summary>
        public List<StoryArcSO> GetAvailableStoryArcs(PlayerProgressData playerProgress)
        {
            var availableArcs = new List<StoryArcSO>();
            var allArcs = GetAllStoryArcs();
            
            foreach (var arc in allArcs)
            {
                if (IsArcAvailable(arc, playerProgress))
                {
                    availableArcs.Add(arc);
                }
            }
            
            return availableArcs;
        }
        
        /// <summary>
        /// Check if story arc is available based on unlock conditions
        /// </summary>
        public bool IsArcAvailable(StoryArcSO arc, PlayerProgressData playerProgress)
        {
            if (arc == null || playerProgress == null) return false;
            
            // Check if arc is already completed
            if (playerProgress.CompletedArcs.Contains(arc.ArcId)) return false;
            
            // Check if arc is currently active
            if (playerProgress.ActiveArcs.Contains(arc.ArcId)) return true;
            
            // Check unlock conditions
            var unlockCondition = _unlockConditions.FirstOrDefault(uc => uc.ArcId == arc.ArcId);
            if (unlockCondition != null)
            {
                return EvaluateUnlockCondition(unlockCondition, playerProgress);
            }
            
            // Check dependencies
            var dependency = _arcDependencies.FirstOrDefault(dep => dep.DependentArcId == arc.ArcId);
            if (dependency != null)
            {
                return dependency.RequiredArcIds.All(requiredId => playerProgress.CompletedArcs.Contains(requiredId));
            }
            
            // Default availability for arcs without specific conditions
            return true;
        }
        
        private bool EvaluateUnlockCondition(ArcUnlockCondition condition, PlayerProgressData playerProgress)
        {
            // Evaluate required level
            if (condition.RequiredLevel > playerProgress.PlayerLevel) return false;
            
            // Evaluate required skills
            foreach (var requiredSkill in condition.RequiredSkills)
            {
                if (!playerProgress.SkillLevels.ContainsKey(requiredSkill.SkillId) ||
                    playerProgress.SkillLevels[requiredSkill.SkillId] < requiredSkill.RequiredLevel)
                {
                    return false;
                }
            }
            
            // Evaluate required achievements
            if (!condition.RequiredAchievements.All(achievement => playerProgress.UnlockedAchievements.Contains(achievement)))
            {
                return false;
            }
            
            // Evaluate custom conditions
            foreach (var customCondition in condition.CustomConditions)
            {
                if (!EvaluateCustomCondition(customCondition, playerProgress))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private bool EvaluateCustomCondition(CustomCondition condition, PlayerProgressData playerProgress)
        {
            // Custom condition evaluation logic
            return condition.ConditionType switch
            {
                ConditionType.PlantsHarvested => playerProgress.TotalPlantsHarvested >= condition.RequiredValue,
                ConditionType.BusinessRevenue => playerProgress.TotalRevenue >= condition.RequiredValue,
                ConditionType.StrainsBred => playerProgress.StrainsBred >= condition.RequiredValue,
                ConditionType.CommunityReputation => playerProgress.CommunityReputation >= condition.RequiredValue,
                ConditionType.TechnologicalAdvancement => playerProgress.TechnologyLevel >= condition.RequiredValue,
                ConditionType.TimeSpent => playerProgress.TotalPlayTime >= condition.RequiredValue,
                ConditionType.CompletedArcs => playerProgress.CompletedArcs.Count >= condition.RequiredValue,
                _ => true
            };
        }
        
        /// <summary>
        /// Get next recommended story arc based on player preferences and progress
        /// </summary>
        public StoryArcSO GetRecommendedNextArc(PlayerProgressData playerProgress, PlayerPreferences preferences)
        {
            var availableArcs = GetAvailableStoryArcs(playerProgress);
            if (!availableArcs.Any()) return null;
            
            // Score arcs based on player preferences and progress
            var scoredArcs = availableArcs.Select(arc => new
            {
                Arc = arc,
                Score = CalculateArcScore(arc, playerProgress, preferences)
            }).OrderByDescending(x => x.Score);
            
            return scoredArcs.First().Arc;
        }
        
        private float CalculateArcScore(StoryArcSO arc, PlayerProgressData playerProgress, PlayerPreferences preferences)
        {
            float score = 0f;
            
            // Base score from arc priority
            score += (int)arc.Priority * 10f;
            
            // Educational content preference
            var educationalMapping = _educationalMappings.FirstOrDefault(em => em.ArcId == arc.ArcId);
            if (educationalMapping != null)
            {
                score += educationalMapping.EducationalValue * preferences.EducationalContentPreference * 5f;
            }
            
            // Story type preference
            score += preferences.GetStoryTypePreference(arc.Category) * 15f;
            
            // Difficulty preference
            var difficultyMatch = 1f - Mathf.Abs(arc.DifficultyLevel - preferences.PreferredDifficulty);
            score += difficultyMatch * 8f;
            
            // Recency bonus (prefer arcs that haven't been played recently)
            if (!playerProgress.RecentlyCompletedArcs.Contains(arc.ArcId))
            {
                score += 5f;
            }
            
            return score;
        }
        
        /// <summary>
        /// Validate educational content in story arcs
        /// </summary>
        public bool ValidateEducationalContent()
        {
            var allArcs = GetAllStoryArcs();
            var validationResults = new List<bool>();
            
            foreach (var arc in allArcs)
            {
                var educationalMapping = _educationalMappings.FirstOrDefault(em => em.ArcId == arc.ArcId);
                if (educationalMapping != null)
                {
                    // Check if educational content meets minimum threshold
                    var isValid = educationalMapping.EducationalValue >= _minimumEducationalContent;
                    
                    // Validate scientific accuracy
                    var scientificValidation = _scientificValidations.FirstOrDefault(sv => sv.ArcId == arc.ArcId);
                    if (scientificValidation != null)
                    {
                        isValid = isValid && scientificValidation.IsScientificallyAccurate;
                    }
                    
                    validationResults.Add(isValid);
                }
                else
                {
                    validationResults.Add(false);
                }
            }
            
            return validationResults.All(result => result);
        }
        
        /// <summary>
        /// Get progression path from current to target arc
        /// </summary>
        public List<string> GetProgressionPath(string currentArcId, string targetArcId)
        {
            var path = new List<string>();
            var visited = new HashSet<string>();
            
            if (FindPath(currentArcId, targetArcId, path, visited))
            {
                return path;
            }
            
            return new List<string>(); // No path found
        }
        
        private bool FindPath(string currentArcId, string targetArcId, List<string> path, HashSet<string> visited)
        {
            if (currentArcId == targetArcId)
            {
                path.Add(currentArcId);
                return true;
            }
            
            if (visited.Contains(currentArcId)) return false;
            visited.Add(currentArcId);
            
            // Find progression options from current arc
            var progressionOptions = _progressionPaths.Where(pp => pp.FromArcId == currentArcId);
            
            foreach (var progression in progressionOptions)
            {
                if (FindPath(progression.ToArcId, targetArcId, path, visited))
                {
                    path.Insert(0, currentArcId);
                    return true;
                }
            }
            
            return false;
        }
        
        protected override bool ValidateDataSpecific()
        {
            var isValid = true;
            var validationErrors = new List<string>();
            
            // Validate arc collections
            var allArcs = GetAllStoryArcs();
            var arcIds = allArcs.Select(arc => arc?.ArcId).ToList();
            var duplicateIds = arcIds.Where(id => !string.IsNullOrEmpty(id))
                                    .GroupBy(id => id)
                                    .Where(g => g.Count() > 1)
                                    .Select(g => g.Key)
                                    .ToList();
            
            if (duplicateIds.Any())
            {
                validationErrors.Add($"Duplicate arc IDs found: {string.Join(", ", duplicateIds)}");
                isValid = false;
            }
            
            // Validate null references
            if (allArcs.Any(arc => arc == null))
            {
                validationErrors.Add("Null arc references found in library");
                isValid = false;
            }
            
            // Validate dependencies
            foreach (var dependency in _arcDependencies)
            {
                if (GetStoryArcById(dependency.DependentArcId) == null)
                {
                    validationErrors.Add($"Invalid dependent arc ID: {dependency.DependentArcId}");
                    isValid = false;
                }
                
                foreach (var requiredArcId in dependency.RequiredArcIds)
                {
                    if (GetStoryArcById(requiredArcId) == null)
                    {
                        validationErrors.Add($"Invalid required arc ID: {requiredArcId}");
                        isValid = false;
                    }
                }
            }
            
            // Validate educational mappings
            foreach (var mapping in _educationalMappings)
            {
                if (GetStoryArcById(mapping.ArcId) == null)
                {
                    validationErrors.Add($"Invalid arc ID in educational mapping: {mapping.ArcId}");
                    isValid = false;
                }
                
                if (mapping.EducationalValue < 0f || mapping.EducationalValue > 1f)
                {
                    validationErrors.Add($"Educational value out of range for arc: {mapping.ArcId}");
                    isValid = false;
                }
            }
            
            // Validate performance settings
            if (_maxSimultaneousArcs <= 0)
            {
                validationErrors.Add("Max Simultaneous Arcs must be greater than 0");
                isValid = false;
            }
            
            if (_maxCachedArcs <= 0)
            {
                validationErrors.Add("Max Cached Arcs must be greater than 0");
                isValid = false;
            }
            
            if (_arcUpdateInterval <= 0f)
            {
                validationErrors.Add("Arc Update Interval must be greater than 0");
                isValid = false;
            }
            
            // Validate educational content minimum
            if (_minimumEducationalContent < 0f || _minimumEducationalContent > 1f)
            {
                validationErrors.Add("Minimum Educational Content must be between 0 and 1");
                isValid = false;
            }
            
            // Log validation results
            if (!isValid)
            {
                Debug.LogError($"[StoryArcLibrarySO] Validation failed for {name}: {string.Join(", ", validationErrors)}");
            }
            
            return base.ValidateDataSpecific() && isValid;
        }
    }
    
    // Supporting data structures
    [Serializable]
    public class ArcDependency
    {
        public string DependentArcId;
        public List<string> RequiredArcIds = new List<string>();
        public DependencyType Type = DependencyType.Sequential;
    }
    
    [Serializable]
    public class ArcProgression
    {
        public string FromArcId;
        public string ToArcId;
        public float TransitionWeight = 1.0f;
        public List<string> TransitionConditions = new List<string>();
    }
    
    [Serializable]
    public class ArcUnlockCondition
    {
        public string ArcId;
        public int RequiredLevel = 1;
        public List<SkillRequirement> RequiredSkills = new List<SkillRequirement>();
        public List<string> RequiredAchievements = new List<string>();
        public List<CustomCondition> CustomConditions = new List<CustomCondition>();
    }
    
    [Serializable]
    public class EducationalMapping
    {
        public string ArcId;
        public float EducationalValue = 0.7f;
        public List<string> LearningOutcomes = new List<string>();
        public List<CultivationTopic> CoveredTopics = new List<CultivationTopic>();
        public bool IsScientificallyValidated = true;
    }
    
    [Serializable]
    public class ScientificValidation
    {
        public string ArcId;
        public bool IsScientificallyAccurate = true;
        public List<string> ValidatedConcepts = new List<string>();
        public List<string> ValidationSources = new List<string>();
        public string ValidatorCredentials;
        public DateTime ValidationDate;
    }
    
    [Serializable]
    public class BranchingPoint
    {
        public string PointId;
        public string ArcId;
        public string BeatId;
        public List<BranchingChoice> Choices = new List<BranchingChoice>();
        public BranchingType Type = BranchingType.PlayerChoice;
    }
    
    [Serializable]
    public class BranchingChoice
    {
        public string ChoiceId;
        public string ChoiceText;
        public string TargetArcId;
        public string TargetBeatId;
        public List<Consequence> Consequences = new List<Consequence>();
    }
    
    [Serializable]
    public class ConsequenceMapping
    {
        public string ChoiceId;
        public List<Consequence> ImmediateConsequences = new List<Consequence>();
        public List<DelayedConsequence> DelayedConsequences = new List<DelayedConsequence>();
    }
    
    
    [Serializable]
    public class DelayedConsequence : Consequence
    {
        public float DelayTime = 600f; // 10 minutes default
        public string TriggerCondition;
    }
    
    [Serializable]
    public class SkillRequirement
    {
        public string SkillId;
        public float RequiredLevel = 1.0f;
    }
    
    [Serializable]
    public class CustomCondition
    {
        public ConditionType ConditionType;
        public float RequiredValue = 1.0f;
        public string Description;
    }
    
        // Note: Enums and shared data structures moved to NarrativeDataStructures.cs to prevent duplicates
}