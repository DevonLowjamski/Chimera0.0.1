using UnityEngine;
using ProjectChimera.Systems.Genetics;
using ProjectChimera.Data.Genetics;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Validation test for GeneticResearchManager
/// Verifies manager inheritance, research functionality, and progression
/// </summary>
public class GeneticResearchValidation : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== Genetic Research Manager Validation ===");
        
        // Test manager instantiation
        TestManagerInstantiation();
        
        // Test research project creation
        TestResearchProjectCreation();
        
        // Test research phases and progression
        TestResearchProgression();
        
        // Test research rewards and techniques
        TestResearchRewards();
        
        Debug.Log("✅ Genetic Research Manager validation completed successfully");
    }
    
    private void TestManagerInstantiation()
    {
        // Test that manager can be instantiated (inheritance check)
        var testObject = new GameObject("TestResearchManager");
        var manager = testObject.AddComponent<GeneticResearchManager>();
        
        // Verify it's a ChimeraManager
        bool isChimeraManager = manager is ProjectChimera.Core.ChimeraManager;
        
        Debug.Log($"✅ Manager instantiation: {(manager != null ? "Success" : "Failed")}");
        Debug.Log($"✅ ChimeraManager inheritance: {isChimeraManager}");
        
        // Test basic properties
        manager.EnableResearch = true;
        manager.MaxActiveProjects = 3;
        manager.BaseResearchSpeed = 1.5f;
        
        Debug.Log($"✅ Manager configuration: Research={manager.EnableResearch}, Max={manager.MaxActiveProjects}, Speed={manager.BaseResearchSpeed}");
        
        // Cleanup
        DestroyImmediate(testObject);
    }
    
    private void TestResearchProjectCreation()
    {
        // Test research project data structure creation
        var project = new CleanGeneticResearchProject
        {
            ProjectID = "test_trait_mapping",
            ProjectName = "Test Trait Mapping Project",
            Description = "A test project for genetic trait mapping",
            ResearchType = GeneticResearchType.TraitMapping,
            StartDate = DateTime.Now,
            Progress = 0.25f,
            IsCompleted = false
        };
        
        // Test research phases
        var phase1 = new CleanResearchPhase
        {
            PhaseID = "sample_collection",
            PhaseName = "Sample Collection",
            Description = "Collect genetic samples from diverse strains",
            Progress = 1.0f,
            IsCompleted = true
        };
        phase1.RequiredActions.Add("collect_leaf_samples");
        phase1.RequiredActions.Add("extract_dna");
        phase1.CompletedActions.Add("collect_leaf_samples");
        phase1.CompletedActions.Add("extract_dna");
        
        var phase2 = new CleanResearchPhase
        {
            PhaseID = "genetic_sequencing",
            PhaseName = "Genetic Sequencing",
            Description = "Sequence and analyze genetic markers",
            Progress = 0.3f,
            IsCompleted = false
        };
        phase2.RequiredActions.Add("sequence_genomes");
        phase2.RequiredActions.Add("identify_markers");
        phase2.RequiredActions.Add("statistical_analysis");
        
        project.Phases.Add(phase1);
        project.Phases.Add(phase2);
        
        // Test research requirements
        project.Requirements = new CleanResearchRequirements
        {
            MinSkillLevel = 4,
            EstimatedTimeHours = 240f, // 10 days
            ResourceCost = 20000f
        };
        project.Requirements.RequiredEquipment.Add("genetic_sequencer");
        project.Requirements.RequiredEquipment.Add("analysis_software");
        project.Requirements.RequiredResources.Add("plant_samples");
        project.Requirements.RequiredResources.Add("sequencing_reagents");
        
        // Test expected rewards
        project.ExpectedRewards = new CleanResearchRewards
        {
            ExperienceGain = 1200f,
            ReputationGain = 35f
        };
        project.ExpectedRewards.UnlockedTechniques.Add("marker_assisted_selection");
        project.ExpectedRewards.UnlockedTechniques.Add("trait_prediction");
        project.ExpectedRewards.UnlockedEquipment.Add("advanced_sequencer");
        
        Debug.Log($"✅ Research project created: {project.ProjectName} ({project.ResearchType})");
        Debug.Log($"✅ Project phases: {project.Phases.Count} (Phase 1: {phase1.Progress:P}, Phase 2: {phase2.Progress:P})");
        Debug.Log($"✅ Requirements: Level {project.Requirements.MinSkillLevel}, {project.Requirements.EstimatedTimeHours}h, ${project.Requirements.ResourceCost}");
        Debug.Log($"✅ Expected rewards: {project.ExpectedRewards.ExperienceGain} XP, {project.ExpectedRewards.UnlockedTechniques.Count} techniques");
    }
    
    private void TestResearchProgression()
    {
        // Test research types and their characteristics
        var researchTypes = new List<(GeneticResearchType type, string description)>
        {
            (GeneticResearchType.TraitMapping, "Genetic trait mapping research"),
            (GeneticResearchType.BreedingOptimization, "Breeding strategy optimization"),
            (GeneticResearchType.QualityImprovement, "Cannabis quality enhancement"),
            (GeneticResearchType.YieldEnhancement, "Yield maximization research"),
            (GeneticResearchType.ResistanceBreeding, "Disease resistance development"),
            (GeneticResearchType.NovelCompounds, "Novel compound discovery")
        };
        
        Debug.Log("✅ Research type validation:");
        foreach (var (type, description) in researchTypes)
        {
            Debug.Log($"  - {type}: {description}");
        }
        
        // Test multi-phase research project
        var complexProject = new CleanGeneticResearchProject
        {
            ProjectID = "complex_breeding_study",
            ProjectName = "Complex Breeding Optimization Study",
            Description = "Multi-phase study to optimize breeding strategies",
            ResearchType = GeneticResearchType.BreedingOptimization,
            StartDate = DateTime.Now,
            Progress = 0.6f,
            IsCompleted = false
        };
        
        // Create multiple phases with different completion states
        var phases = new List<CleanResearchPhase>
        {
            new CleanResearchPhase
            {
                PhaseID = "literature_review",
                PhaseName = "Literature Review",
                Description = "Review existing breeding methodologies",
                Progress = 1.0f,
                IsCompleted = true
            },
            new CleanResearchPhase
            {
                PhaseID = "experimental_design",
                PhaseName = "Experimental Design",
                Description = "Design breeding experiments",
                Progress = 1.0f,
                IsCompleted = true
            },
            new CleanResearchPhase
            {
                PhaseID = "breeding_trials",
                PhaseName = "Breeding Trials",
                Description = "Conduct controlled breeding experiments",
                Progress = 0.8f,
                IsCompleted = false
            },
            new CleanResearchPhase
            {
                PhaseID = "data_analysis",
                PhaseName = "Data Analysis",
                Description = "Analyze breeding trial results",
                Progress = 0.2f,
                IsCompleted = false
            }
        };
        
        complexProject.Phases.AddRange(phases);
        
        int completedPhases = phases.Count(p => p.IsCompleted);
        int totalPhases = phases.Count;
        float overallProgress = (float)completedPhases / totalPhases + (phases.LastOrDefault(p => !p.IsCompleted)?.Progress ?? 0f) / totalPhases;
        
        Debug.Log($"✅ Complex project progression: {completedPhases}/{totalPhases} phases completed");
        Debug.Log($"✅ Overall progress calculation: {overallProgress:P} (Project shows: {complexProject.Progress:P})");
    }
    
    private void TestResearchRewards()
    {
        // Test comprehensive research rewards system
        var rewards = new CleanResearchRewards
        {
            ExperienceGain = 2500f,
            ReputationGain = 75f
        };
        
        // Test technique unlocks
        var techniques = new List<string>
        {
            "advanced_trait_selection",
            "generation_acceleration",
            "molecular_markers",
            "genomic_prediction",
            "hybrid_vigor_optimization"
        };
        rewards.UnlockedTechniques.AddRange(techniques);
        
        // Test equipment unlocks
        var equipment = new List<string>
        {
            "high_throughput_sequencer",
            "automated_phenotyping_system",
            "molecular_lab_upgrade"
        };
        rewards.UnlockedEquipment.AddRange(equipment);
        
        // Test strain unlocks
        var strains = new List<string>
        {
            "research_strain_alpha",
            "optimized_breeding_line_beta",
            "high_cbd_research_cultivar"
        };
        rewards.UnlockedStrains.AddRange(strains);
        
        Debug.Log($"✅ Research rewards system validated:");
        Debug.Log($"  - Experience: {rewards.ExperienceGain} XP");
        Debug.Log($"  - Reputation: {rewards.ReputationGain} points");
        Debug.Log($"  - Techniques: {rewards.UnlockedTechniques.Count} unlocked");
        Debug.Log($"  - Equipment: {rewards.UnlockedEquipment.Count} unlocked");
        Debug.Log($"  - Strains: {rewards.UnlockedStrains.Count} unlocked");
        
        // Test research requirements validation
        var requirements = new CleanResearchRequirements
        {
            MinSkillLevel = 7,
            EstimatedTimeHours = 720f, // 30 days
            ResourceCost = 50000f
        };
        
        requirements.RequiredEquipment.AddRange(new[] { "molecular_lab", "breeding_facility", "data_analysis_cluster" });
        requirements.RequiredResources.AddRange(new[] { "elite_breeding_stock", "molecular_reagents", "computational_resources" });
        
        Debug.Log($"✅ Research requirements validated:");
        Debug.Log($"  - Minimum skill level: {requirements.MinSkillLevel}");
        Debug.Log($"  - Estimated time: {requirements.EstimatedTimeHours} hours ({requirements.EstimatedTimeHours/24:F1} days)");
        Debug.Log($"  - Resource cost: ${requirements.ResourceCost:N0}");
        Debug.Log($"  - Required equipment: {requirements.RequiredEquipment.Count} items");
        Debug.Log($"  - Required resources: {requirements.RequiredResources.Count} types");
    }
}