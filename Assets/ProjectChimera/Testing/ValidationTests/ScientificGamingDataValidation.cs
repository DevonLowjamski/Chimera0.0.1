using UnityEngine;
using ProjectChimera.Data.Genetics;
using System;
using System.Collections.Generic;

/// <summary>
/// Validation test for scientific gaming data structures
/// Verifies clean data types compile and function correctly
/// </summary>
public class ScientificGamingDataValidation : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== Scientific Gaming Data Structures Validation ===");
        
        // Test Competition Data
        TestCompetitionData();
        
        // Test Research Data
        TestResearchData();
        
        // Test Breeding Challenge Data
        TestBreedingChallengeData();
        
        // Test Achievement Data
        TestAchievementData();
        
        // Test Genetic Submission Data
        TestGeneticSubmissionData();
        
        Debug.Log("✅ Scientific Gaming Data Structures validation completed successfully");
    }
    
    private void TestCompetitionData()
    {
        var competition = new CleanScientificCompetition
        {
            CompetitionID = "breeding_challenge_001",
            CompetitionName = "Advanced Breeding Challenge",
            Description = "Test breeding skills",
            CompetitionType = ScientificCompetitionType.BreedingChallenge,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(30),
            IsActive = true
        };
        
        var entry = new CleanCompetitionEntry
        {
            ParticipantID = "player_001",
            ParticipantName = "Test Player",
            SubmissionID = "submission_001",
            Score = 95.5f,
            Rank = 1,
            SubmissionDate = DateTime.Now
        };
        
        competition.Entries.Add(entry);
        
        Debug.Log($"✅ Competition data validated: {competition.CompetitionName} with {competition.Entries.Count} entries");
    }
    
    private void TestResearchData()
    {
        var research = new CleanGeneticResearchProject
        {
            ProjectID = "research_001",
            ProjectName = "Trait Mapping Study",
            Description = "Map genetic traits",
            ResearchType = GeneticResearchType.TraitMapping,
            StartDate = DateTime.Now,
            Progress = 0.75f,
            IsCompleted = false
        };
        
        var phase = new CleanResearchPhase
        {
            PhaseID = "phase_001",
            PhaseName = "Data Collection",
            Description = "Collect genetic samples",
            Progress = 1.0f,
            IsCompleted = true
        };
        
        research.Phases.Add(phase);
        
        Debug.Log($"✅ Research data validated: {research.ProjectName} at {research.Progress:P} completion");
    }
    
    private void TestBreedingChallengeData()
    {
        var challenge = new CleanBreedingChallenge
        {
            ChallengeID = "breeding_001",
            ChallengeName = "Quality Optimization Challenge",
            Description = "Maximize quality traits",
            ChallengeType = BreedingChallengeType.QualityMaximization,
            Difficulty = BreedingDifficulty.Advanced,
            IsActive = true
        };
        
        var objective = new CleanChallengeObjective
        {
            ObjectiveDescription = "Achieve target quality",
            MinimumQualityScore = 90.0f,
            RequiredGenerations = 5,
            RequireStability = true
        };
        
        var traitTarget = new CleanTraitTarget
        {
            TraitName = "Quality",
            TargetValue = 95.0f,
            ToleranceRange = 2.0f,
            IsRequired = true,
            Weight = 1.0f
        };
        
        objective.RequiredTraits.Add(traitTarget);
        challenge.Objective = objective;
        
        Debug.Log($"✅ Breeding challenge validated: {challenge.ChallengeName} ({challenge.Difficulty})");
    }
    
    private void TestAchievementData()
    {
        var achievement = new CleanScientificAchievement
        {
            AchievementID = "achievement_001",
            AchievementName = "Master Breeder",
            Description = "Complete 10 breeding challenges",
            AchievementType = ScientificAchievementType.BreedingMilestone,
            Progress = 0.8f,
            IsUnlocked = false,
            IsHidden = false
        };
        
        var criteria = new CleanAchievementCriteria
        {
            RequireAllCriteria = true,
            MinimumScore = 85.0f
        };
        
        var requirement = new CleanAchievementRequirement
        {
            RequirementType = "ChallengesCompleted",
            RequirementValue = "10",
            TargetValue = 10.0f,
            IsCompleted = false,
            CurrentProgress = 8.0f
        };
        
        criteria.Requirements.Add(requirement);
        achievement.Criteria = criteria;
        
        Debug.Log($"✅ Achievement validated: {achievement.AchievementName} at {achievement.Progress:P}");
    }
    
    private void TestGeneticSubmissionData()
    {
        var submission = new CleanGeneticSubmissionData
        {
            SubmissionID = "sub_001",
            StrainName = "Test Strain Alpha",
            SubmissionDate = DateTime.Now,
            SubmitterNotes = "High quality strain"
        };
        
        var geneticProfile = new CleanGeneticProfile
        {
            GeneticDiversity = 0.85f,
            StabilityScore = 0.92f
        };
        
        var alleleExpression = new CleanAlleleExpression
        {
            GeneID = "gene_001",
            AlleleID = "allele_A",
            ExpressionLevel = 0.95f,
            IsDominant = true,
            Contribution = 0.45f
        };
        
        geneticProfile.AlleleExpressions.Add(alleleExpression);
        submission.GeneticProfile = geneticProfile;
        
        var phenotype = new CleanPhenotypicData
        {
            Height = 150.0f,
            YieldPotential = 85.5f,
            FloweringTime = 65.0f,
            OverallQuality = 92.3f
        };
        
        var chemicalProfile = new CleanChemicalProfile
        {
            CompoundName = "THC",
            Concentration = 22.5f,
            Unit = "%",
            Variance = 1.2f
        };
        
        phenotype.ChemicalProfiles.Add(chemicalProfile);
        submission.PhenotypicData = phenotype;
        
        Debug.Log($"✅ Genetic submission validated: {submission.StrainName} (Quality: {submission.PhenotypicData.OverallQuality:F1})");
    }
}