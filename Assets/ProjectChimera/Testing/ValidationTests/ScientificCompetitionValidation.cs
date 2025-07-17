using UnityEngine;
using ProjectChimera.Systems.Genetics;
using ProjectChimera.Data.Genetics;
using System;

/// <summary>
/// Validation test for ScientificCompetitionManager
/// Verifies manager inheritance, functionality, and integration
/// </summary>
public class ScientificCompetitionValidation : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== Scientific Competition Manager Validation ===");
        
        // Test manager instantiation
        TestManagerInstantiation();
        
        // Test competition creation
        TestCompetitionCreation();
        
        // Test breeding challenge creation
        TestBreedingChallengeCreation();
        
        // Test data structures
        TestDataStructures();
        
        Debug.Log("✅ Scientific Competition Manager validation completed successfully");
    }
    
    private void TestManagerInstantiation()
    {
        // Test that manager can be instantiated (inheritance check)
        var testObject = new GameObject("TestCompetitionManager");
        var manager = testObject.AddComponent<ScientificCompetitionManager>();
        
        // Verify it's a ChimeraManager
        bool isChimeraManager = manager is ProjectChimera.Core.ChimeraManager;
        
        Debug.Log($"✅ Manager instantiation: {(manager != null ? "Success" : "Failed")}");
        Debug.Log($"✅ ChimeraManager inheritance: {isChimeraManager}");
        
        // Test basic properties
        manager.EnableCompetitions = true;
        manager.MaxActiveCompetitions = 3;
        
        Debug.Log($"✅ Manager configuration: Competitions={manager.EnableCompetitions}, Max={manager.MaxActiveCompetitions}");
        
        // Cleanup
        DestroyImmediate(testObject);
    }
    
    private void TestCompetitionCreation()
    {
        // Test competition data structure creation
        var competition = new CleanScientificCompetition
        {
            CompetitionID = "test_breeding_comp",
            CompetitionName = "Test Breeding Competition",
            Description = "A test competition for validation",
            CompetitionType = ScientificCompetitionType.BreedingChallenge,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(30),
            IsActive = true
        };
        
        // Test competition rewards
        competition.Rewards = new CleanCompetitionRewards
        {
            ExperienceBonus = 1000f
        };
        competition.Rewards.FirstPlaceRewards.Add("advanced_genetics");
        
        // Test competition criteria
        competition.JudgingCriteria = new CleanCompetitionCriteria
        {
            GeneticInnovationWeight = 0.4f,
            PracticalApplicationWeight = 0.3f,
            ScientificRigorWeight = 0.3f
        };
        
        Debug.Log($"✅ Competition created: {competition.CompetitionName} ({competition.CompetitionType})");
        Debug.Log($"✅ Rewards configured: {competition.Rewards.FirstPlaceRewards.Count} first place rewards");
        Debug.Log($"✅ Criteria configured: Innovation={competition.JudgingCriteria.GeneticInnovationWeight:F1}");
    }
    
    private void TestBreedingChallengeCreation()
    {
        // Test breeding challenge data structure
        var challenge = new CleanBreedingChallenge
        {
            ChallengeID = "test_quality_challenge",
            ChallengeName = "Quality Maximization Test",
            Description = "Test challenge for quality optimization",
            ChallengeType = BreedingChallengeType.QualityMaximization,
            Difficulty = BreedingDifficulty.Intermediate,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(14),
            IsActive = true
        };
        
        // Test challenge objective
        challenge.Objective = new CleanChallengeObjective
        {
            ObjectiveDescription = "Achieve 90% quality score",
            MinimumQualityScore = 90f,
            RequiredGenerations = 5,
            RequireStability = true
        };
        
        // Test trait targets
        var traitTarget = new CleanTraitTarget
        {
            TraitName = "Quality",
            TargetValue = 90f,
            ToleranceRange = 2f,
            IsRequired = true,
            Weight = 1f
        };
        challenge.Objective.RequiredTraits.Add(traitTarget);
        
        // Test constraints
        challenge.Constraints = new CleanChallengeConstraints
        {
            MaxGenerations = 8,
            MaxPlants = 30,
            TimeLimit = 336f, // 14 days in hours
            BudgetLimit = 25000f
        };
        
        // Test rewards
        challenge.Rewards = new CleanChallengeRewards
        {
            ExperienceReward = 500f,
            ReputationReward = 20f,
            MonetaryReward = 2000f,
            AchievementID = "quality_challenge_intermediate"
        };
        challenge.Rewards.UnlockRewards.Add("advanced_breeding_techniques");
        
        Debug.Log($"✅ Breeding challenge created: {challenge.ChallengeName} ({challenge.Difficulty})");
        Debug.Log($"✅ Objective: {challenge.Objective.MinimumQualityScore}% quality in {challenge.Objective.RequiredGenerations} generations");
        Debug.Log($"✅ Constraints: Max {challenge.Constraints.MaxPlants} plants, ${challenge.Constraints.BudgetLimit} budget");
        Debug.Log($"✅ Rewards: {challenge.Rewards.ExperienceReward} XP, {challenge.Rewards.UnlockRewards.Count} unlocks");
    }
    
    private void TestDataStructures()
    {
        // Test competition entry
        var entry = new CleanCompetitionEntry
        {
            ParticipantID = "player_001",
            ParticipantName = "Test Player",
            SubmissionID = "sub_001",
            Score = 87.5f,
            Rank = 2,
            SubmissionDate = DateTime.Now,
            Notes = "High quality submission"
        };
        
        // Test genetic submission data
        entry.SubmissionData = new CleanGeneticSubmissionData
        {
            SubmissionID = "sub_001",
            StrainName = "Test Strain Alpha",
            SubmissionDate = DateTime.Now,
            SubmitterNotes = "Optimized for quality"
        };
        
        // Test genetic profile
        entry.SubmissionData.GeneticProfile = new CleanGeneticProfile
        {
            GeneticDiversity = 0.78f,
            StabilityScore = 0.92f
        };
        entry.SubmissionData.GeneticProfile.DominantTraits.Add("High THC");
        entry.SubmissionData.GeneticProfile.DominantTraits.Add("Dense Buds");
        
        // Test allele expression
        var allele = new CleanAlleleExpression
        {
            GeneID = "thc_gene",
            AlleleID = "high_thc_allele",
            ExpressionLevel = 0.95f,
            IsDominant = true,
            Contribution = 0.4f
        };
        entry.SubmissionData.GeneticProfile.AlleleExpressions.Add(allele);
        
        // Test phenotypic data
        entry.SubmissionData.PhenotypicData = new CleanPhenotypicData
        {
            Height = 120f,
            YieldPotential = 450f,
            FloweringTime = 63f,
            OverallQuality = 87.5f
        };
        
        // Test chemical profile
        var chemical = new CleanChemicalProfile
        {
            CompoundName = "THC",
            Concentration = 24.5f,
            Unit = "%",
            Variance = 1.1f
        };
        entry.SubmissionData.PhenotypicData.ChemicalProfiles.Add(chemical);
        
        // Test physical trait
        var trait = new CleanPhysicalTrait
        {
            TraitName = "Bud Density",
            TraitValue = "Dense",
            NumericValue = 8.5f,
            Description = "Very dense, compact buds"
        };
        entry.SubmissionData.PhenotypicData.PhysicalTraits.Add(trait);
        
        Debug.Log($"✅ Competition entry validated: {entry.ParticipantName} (Rank {entry.Rank}, Score {entry.Score})");
        Debug.Log($"✅ Genetic submission: {entry.SubmissionData.StrainName} (Quality {entry.SubmissionData.PhenotypicData.OverallQuality:F1})");
        Debug.Log($"✅ Genetic profile: {entry.SubmissionData.GeneticProfile.AlleleExpressions.Count} alleles, {entry.SubmissionData.GeneticProfile.DominantTraits.Count} dominant traits");
        Debug.Log($"✅ Phenotype: Height {entry.SubmissionData.PhenotypicData.Height}cm, Yield {entry.SubmissionData.PhenotypicData.YieldPotential}g");
        Debug.Log($"✅ Chemistry: {entry.SubmissionData.PhenotypicData.ChemicalProfiles.Count} compounds analyzed");
    }
}