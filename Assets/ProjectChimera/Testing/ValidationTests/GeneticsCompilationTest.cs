using UnityEngine;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera
{
    /// <summary>
    /// Simple compilation test for clean genetics system types
    /// </summary>
    public class GeneticsCompilationTest : MonoBehaviour
    {
        [Header("Clean Test Configuration")]
        [SerializeField] private GeneticsDifficulty _testDifficulty = GeneticsDifficulty.Beginner;
        [SerializeField] private GeneticsGameMode _testMode = GeneticsGameMode.Breeding;
        [SerializeField] private GeneticsCompetitionType _testCompetition = GeneticsCompetitionType.Individual;
        
        private void Start()
        {
            TestCleanTypes();
        }
        
        private void TestCleanTypes()
        {
            // Test clean enum usage
            var difficulty = GeneticsDifficulty.Advanced;
            var mode = GeneticsGameMode.Competition;
            var competition = GeneticsCompetitionType.Tournament;
            
            // Test clean class instantiation
            var breedingResult = new GeneticsBreedingResult();
            var sensoryResponse = new GeneticsSensoryResponse();
            var competitionEntry = new GeneticsCompetitionEntry();
            var breedingChallenge = new GeneticsBreedingChallenge();
            var tournament = new GeneticsTournament();
            
            Debug.Log($"Clean genetics compilation test successful! Difficulty: {difficulty}, Mode: {mode}, Competition: {competition}");
            Debug.Log($"Created breeding result with success: {breedingResult.Success}");
        }
    }
}