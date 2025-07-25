using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// Genetic Research Manager - Restored from disabled genetics research features
    /// Handles genetic research projects, scientific progression, and discovery tracking
    /// Uses only verified types from ScientificGamingDataStructures to prevent compilation errors
    /// Operates independently within Genetics assembly to avoid assembly dependency issues
    /// 
    /// ABSTRACT METHOD VERIFICATION COMPLETE:
    /// ✅ OnManagerInitialize() - implemented
    /// ✅ OnManagerShutdown() - implemented
    /// </summary>
    public class GeneticResearchManager : ChimeraManager
    {
        [Header("Research Configuration")]
        public bool EnableResearch = true;
        public bool EnableCollaborativeResearch = true;
        public bool EnableResearchAcceleration = false;
        public float ResearchUpdateInterval = 30f;
        
        [Header("Research Settings")]
        public int MaxActiveProjects = 3;
        public int MaxCollaborativeProjects = 2;
        public float BaseResearchSpeed = 1f;
        public bool EnableAutoResearchProgression = true;
        
        [Header("Research Collections")]
        [SerializeField] private List<CleanGeneticResearchProject> activeProjects = new List<CleanGeneticResearchProject>();
        [SerializeField] private List<CleanGeneticResearchProject> completedProjects = new List<CleanGeneticResearchProject>();
        [SerializeField] private List<CleanGeneticResearchProject> availableProjects = new List<CleanGeneticResearchProject>();
        [SerializeField] private Dictionary<string, float> researchProgress = new Dictionary<string, float>();
        
        [Header("Research State")]
        [SerializeField] private DateTime lastResearchUpdate = DateTime.Now;
        [SerializeField] private float totalResearchExperience = 0f;
        [SerializeField] private int researchLevel = 1;
        [SerializeField] private List<string> unlockedTechniques = new List<string>();
        
        // Events using verified event patterns
        public static event Action<CleanGeneticResearchProject> OnResearchProjectStarted;
        public static event Action<CleanGeneticResearchProject> OnResearchProjectCompleted;
        public static event Action<CleanResearchPhase> OnResearchPhaseCompleted;
        public static event Action<string, float> OnResearchProgressUpdated;
        public static event Action<CleanResearchRewards> OnResearchRewardsAwarded;
        
        protected override void OnManagerInitialize()
        {
            // Register with GameManager using verified pattern
            GameManager.Instance?.RegisterManager(this);
            
            // Note: No cross-assembly dependencies to avoid CS0246 errors
            // Research system operates independently within Genetics assembly
            
            // Initialize research system
            InitializeResearchSystem();
            
            if (EnableResearch)
            {
                StartResearchTracking();
            }
            
            Debug.Log("✅ GeneticResearchManager initialized successfully");
        }
        
        protected override void OnManagerShutdown()
        {
            // Clean up research tracking
            if (EnableResearch)
            {
                StopResearchTracking();
            }
            
            // Clear all events to prevent memory leaks
            OnResearchProjectStarted = null;
            OnResearchProjectCompleted = null;
            OnResearchPhaseCompleted = null;
            OnResearchProgressUpdated = null;
            OnResearchRewardsAwarded = null;
            
            Debug.Log("✅ GeneticResearchManager shutdown successfully");
        }
        
        private void InitializeResearchSystem()
        {
            // Initialize collections if empty
            if (activeProjects == null) activeProjects = new List<CleanGeneticResearchProject>();
            if (completedProjects == null) completedProjects = new List<CleanGeneticResearchProject>();
            if (availableProjects == null) availableProjects = new List<CleanGeneticResearchProject>();
            if (researchProgress == null) researchProgress = new Dictionary<string, float>();
            if (unlockedTechniques == null) unlockedTechniques = new List<string>();
            
            // Create default research projects for testing
            CreateDefaultResearchProjects();
            
            // Initialize research progression
            InitializeResearchProgression();
        }
        
        private void CreateDefaultResearchProjects()
        {
            // Create example genetic research projects using only verified types
            var traitMappingProject = new CleanGeneticResearchProject
            {
                ProjectID = "trait_mapping_001",
                ProjectName = "Cannabis Trait Mapping Study",
                Description = "Map genetic markers associated with key cannabis traits",
                ResearchType = GeneticResearchType.TraitMapping,
                StartDate = DateTime.Now,
                Progress = 0f,
                IsCompleted = false,
                Phases = new List<CleanResearchPhase>
                {
                    new CleanResearchPhase
                    {
                        PhaseID = "data_collection",
                        PhaseName = "Data Collection",
                        Description = "Collect genetic samples and phenotypic data",
                        Progress = 0f,
                        IsCompleted = false,
                        RequiredActions = new List<string> { "collect_samples", "record_phenotypes", "prepare_analysis" }
                    },
                    new CleanResearchPhase
                    {
                        PhaseID = "genetic_analysis",
                        PhaseName = "Genetic Analysis",
                        Description = "Analyze genetic markers and correlations",
                        Progress = 0f,
                        IsCompleted = false,
                        RequiredActions = new List<string> { "sequence_genomes", "identify_markers", "statistical_analysis" }
                    }
                },
                Requirements = new CleanResearchRequirements
                {
                    MinSkillLevel = 3,
                    RequiredEquipment = new List<string> { "genetic_sequencer", "laboratory" },
                    RequiredResources = new List<string> { "plant_samples", "analysis_reagents" },
                    EstimatedTimeHours = 168f, // 1 week
                    ResourceCost = 15000f
                },
                ExpectedRewards = new CleanResearchRewards
                {
                    ExperienceGain = 750f,
                    UnlockedTechniques = new List<string> { "trait_prediction", "marker_assisted_selection" },
                    ReputationGain = 25f
                }
            };
            
            var breedingOptimizationProject = new CleanGeneticResearchProject
            {
                ProjectID = "breeding_optimization_001",
                ProjectName = "Breeding Efficiency Optimization",
                Description = "Develop improved breeding strategies for faster trait fixation",
                ResearchType = GeneticResearchType.BreedingOptimization,
                StartDate = DateTime.Now,
                Progress = 0f,
                IsCompleted = false,
                Phases = new List<CleanResearchPhase>
                {
                    new CleanResearchPhase
                    {
                        PhaseID = "strategy_development",
                        PhaseName = "Strategy Development",
                        Description = "Develop and test breeding strategies",
                        Progress = 0f,
                        IsCompleted = false,
                        RequiredActions = new List<string> { "design_crosses", "simulation_testing", "optimization" }
                    }
                },
                Requirements = new CleanResearchRequirements
                {
                    MinSkillLevel = 5,
                    RequiredEquipment = new List<string> { "breeding_chamber", "analysis_tools" },
                    RequiredResources = new List<string> { "breeding_stock", "genetic_data" },
                    EstimatedTimeHours = 336f, // 2 weeks
                    ResourceCost = 25000f
                },
                ExpectedRewards = new CleanResearchRewards
                {
                    ExperienceGain = 1500f,
                    UnlockedTechniques = new List<string> { "accelerated_breeding", "generation_skipping" },
                    ReputationGain = 50f
                }
            };
            
            // Add to available projects
            availableProjects.Add(traitMappingProject);
            availableProjects.Add(breedingOptimizationProject);
        }
        
        private void InitializeResearchProgression()
        {
            // Initialize basic research progression
            researchLevel = 1;
            totalResearchExperience = 0f;
            
            // Add basic techniques
            unlockedTechniques.Add("basic_observation");
            unlockedTechniques.Add("phenotype_recording");
            
            Debug.Log("✅ Research progression initialized - Level 1 researcher");
        }
        
        private void StartResearchTracking()
        {
            // Start research tracking and progression
            lastResearchUpdate = DateTime.Now;
            
            Debug.Log("✅ Research tracking started - operating independently");
        }
        
        private void StopResearchTracking()
        {
            // Clean up research tracking
            Debug.Log("✅ Research tracking stopped");
        }
        
        private void Update()
        {
            if (!EnableResearch || !EnableAutoResearchProgression) return;
            
            // Update research progress automatically
            if ((DateTime.Now - lastResearchUpdate).TotalSeconds >= ResearchUpdateInterval)
            {
                UpdateActiveResearchProjects();
                lastResearchUpdate = DateTime.Now;
            }
        }
        
        private void UpdateActiveResearchProjects()
        {
            var completedProjects = new List<CleanGeneticResearchProject>();
            
            foreach (var project in activeProjects.ToList())
            {
                // Update project progress
                float progressRate = CalculateResearchProgressRate(project);
                project.Progress += progressRate;
                project.Progress = Mathf.Clamp01(project.Progress);
                
                // Update individual phases
                UpdateProjectPhases(project);
                
                // Check for completion
                if (project.Progress >= 1f && !project.IsCompleted)
                {
                    CompleteResearchProject(project);
                    completedProjects.Add(project);
                }
                
                OnResearchProgressUpdated?.Invoke(project.ProjectID, project.Progress);
            }
            
            // Move completed projects
            foreach (var project in completedProjects)
            {
                activeProjects.Remove(project);
                this.completedProjects.Add(project);
            }
        }
        
        #region Public API Methods
        
        /// <summary>
        /// Start a new research project
        /// </summary>
        public bool StartResearchProject(string projectId)
        {
            if (!EnableResearch) return false;
            
            if (activeProjects.Count >= MaxActiveProjects)
            {
                Debug.LogWarning($"Maximum active research projects limit reached ({MaxActiveProjects})");
                return false;
            }
            
            var project = availableProjects.FirstOrDefault(p => p.ProjectID == projectId);
            if (project == null)
            {
                Debug.LogWarning($"Research project not found: {projectId}");
                return false;
            }
            
            // Check requirements
            if (!CanStartResearchProject(project))
            {
                Debug.LogWarning($"Requirements not met for research project: {project.ProjectName}");
                return false;
            }
            
            // Start the project
            project.StartDate = DateTime.Now;
            project.Progress = 0f;
            project.IsCompleted = false;
            
            activeProjects.Add(project);
            availableProjects.Remove(project);
            
            OnResearchProjectStarted?.Invoke(project);
            
            Debug.Log($"✅ Started research project: {project.ProjectName}");
            return true;
        }
        
        /// <summary>
        /// Get all available research projects
        /// </summary>
        public List<CleanGeneticResearchProject> GetAvailableResearchProjects()
        {
            return new List<CleanGeneticResearchProject>(availableProjects);
        }
        
        /// <summary>
        /// Get all active research projects
        /// </summary>
        public List<CleanGeneticResearchProject> GetActiveResearchProjects()
        {
            return new List<CleanGeneticResearchProject>(activeProjects);
        }
        
        /// <summary>
        /// Get completed research projects
        /// </summary>
        public List<CleanGeneticResearchProject> GetCompletedResearchProjects()
        {
            return new List<CleanGeneticResearchProject>(completedProjects);
        }
        
        /// <summary>
        /// Get research project by ID
        /// </summary>
        public CleanGeneticResearchProject GetResearchProject(string projectId)
        {
            var project = activeProjects.FirstOrDefault(p => p.ProjectID == projectId);
            if (project != null) return project;
            
            project = availableProjects.FirstOrDefault(p => p.ProjectID == projectId);
            if (project != null) return project;
            
            return completedProjects.FirstOrDefault(p => p.ProjectID == projectId);
        }
        
        /// <summary>
        /// Get current research level and experience
        /// </summary>
        public (int level, float experience, float experienceToNext) GetResearchProgression()
        {
            float experienceToNext = GetExperienceRequiredForLevel(researchLevel + 1) - totalResearchExperience;
            return (researchLevel, totalResearchExperience, experienceToNext);
        }
        
        /// <summary>
        /// Get unlocked research techniques
        /// </summary>
        public List<string> GetUnlockedTechniques()
        {
            return new List<string>(unlockedTechniques);
        }
        
        /// <summary>
        /// Check if player can start a research project
        /// </summary>
        public bool CanStartResearchProject(CleanGeneticResearchProject project)
        {
            if (project == null) return false;
            
            // Check research level requirement
            if (researchLevel < project.Requirements.MinSkillLevel)
                return false;
            
            // Check for required techniques
            foreach (var technique in project.Requirements.RequiredEquipment)
            {
                if (!HasRequiredCapability(technique))
                    return false;
            }
            
            return true;
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private float CalculateResearchProgressRate(CleanGeneticResearchProject project)
        {
            // Base progress rate per update
            float baseRate = BaseResearchSpeed / project.Requirements.EstimatedTimeHours;
            float updateRate = ResearchUpdateInterval / 3600f; // Convert seconds to hours
            
            // Apply research level bonus
            float levelBonus = 1f + (researchLevel * 0.1f);
            
            // Apply acceleration if enabled
            float accelerationBonus = EnableResearchAcceleration ? 2f : 1f;
            
            return baseRate * updateRate * levelBonus * accelerationBonus;
        }
        
        private void UpdateProjectPhases(CleanGeneticResearchProject project)
        {
            foreach (var phase in project.Phases)
            {
                if (phase.IsCompleted) continue;
                
                // Simple phase progression based on overall project progress
                int phaseIndex = project.Phases.IndexOf(phase);
                float phaseStartProgress = (float)phaseIndex / project.Phases.Count;
                float phaseEndProgress = (float)(phaseIndex + 1) / project.Phases.Count;
                
                if (project.Progress >= phaseStartProgress)
                {
                    phase.Progress = Mathf.Clamp01((project.Progress - phaseStartProgress) / (phaseEndProgress - phaseStartProgress));
                    
                    if (phase.Progress >= 1f && !phase.IsCompleted)
                    {
                        CompleteResearchPhase(project, phase);
                    }
                }
            }
        }
        
        private void CompleteResearchPhase(CleanGeneticResearchProject project, CleanResearchPhase phase)
        {
            phase.IsCompleted = true;
            
            // Mark all required actions as completed
            phase.CompletedActions = new List<string>(phase.RequiredActions);
            
            OnResearchPhaseCompleted?.Invoke(phase);
            
            Debug.Log($"✅ Research phase completed: {phase.PhaseName} in {project.ProjectName}");
        }
        
        private void CompleteResearchProject(CleanGeneticResearchProject project)
        {
            project.IsCompleted = true;
            
            // Award research rewards
            AwardResearchRewards(project.ExpectedRewards);
            
            OnResearchProjectCompleted?.Invoke(project);
            
            Debug.Log($"✅ Research project completed: {project.ProjectName}");
        }
        
        private void AwardResearchRewards(CleanResearchRewards rewards)
        {
            // Award experience
            totalResearchExperience += rewards.ExperienceGain;
            CheckForLevelUp();
            
            // Unlock new techniques
            foreach (var technique in rewards.UnlockedTechniques)
            {
                if (!unlockedTechniques.Contains(technique))
                {
                    unlockedTechniques.Add(technique);
                    Debug.Log($"✅ New technique unlocked: {technique}");
                }
            }
            
            OnResearchRewardsAwarded?.Invoke(rewards);
            
            Debug.Log($"✅ Research rewards awarded: {rewards.ExperienceGain} XP, {rewards.UnlockedTechniques.Count} techniques");
        }
        
        private void CheckForLevelUp()
        {
            float requiredExp = GetExperienceRequiredForLevel(researchLevel + 1);
            
            while (totalResearchExperience >= requiredExp && researchLevel < 20) // Max level 20
            {
                researchLevel++;
                Debug.Log($"✅ Research level up! Now level {researchLevel}");
                
                requiredExp = GetExperienceRequiredForLevel(researchLevel + 1);
            }
        }
        
        private float GetExperienceRequiredForLevel(int level)
        {
            // Exponential experience curve
            return Mathf.Pow(level, 2.2f) * 500f;
        }
        
        private bool HasRequiredCapability(string capability)
        {
            // Simple capability check - can be expanded
            return unlockedTechniques.Contains(capability) || 
                   capability == "basic_equipment" || 
                   researchLevel >= 3; // Higher level researchers have more capabilities
        }
        
        #endregion
        
        #region Testing and Validation Methods
        
        /// <summary>
        /// Test method to validate genetic research system functionality
        /// </summary>
        public void TestResearchSystem()
        {
            Debug.Log("=== Testing Genetic Research System ===");
            Debug.Log($"Research Enabled: {EnableResearch}");
            Debug.Log($"Available Projects: {availableProjects.Count}");
            Debug.Log($"Active Projects: {activeProjects.Count}");
            Debug.Log($"Completed Projects: {completedProjects.Count}");
            Debug.Log($"Research Level: {researchLevel}");
            Debug.Log($"Total Experience: {totalResearchExperience}");
            Debug.Log($"Unlocked Techniques: {unlockedTechniques.Count}");
            
            // Test starting a research project
            if (EnableResearch && availableProjects.Count > 0)
            {
                var testProject = availableProjects[0];
                bool canStart = CanStartResearchProject(testProject);
                Debug.Log($"✓ Can start test project '{testProject.ProjectName}': {canStart}");
                
                if (canStart)
                {
                    bool started = StartResearchProject(testProject.ProjectID);
                    Debug.Log($"✓ Test project started: {started}");
                }
            }
            
            Debug.Log("✅ Genetic research system test completed");
        }
        
        #endregion
    }
}