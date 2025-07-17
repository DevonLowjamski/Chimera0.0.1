using UnityEngine;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Systems.Genetics;

namespace ProjectChimera
{
    /// <summary>
    /// Simple test to verify clean genetics system compiles and works
    /// </summary>
    public class CleanGeneticsTest : MonoBehaviour
    {
        [Header("Clean Genetics Test")]
        [SerializeField] private CleanGeneticsManager _geneticsManager;
        
        [Header("Test Configuration")]
        [SerializeField] private GeneticsDifficulty _testDifficulty = GeneticsDifficulty.Beginner;
        [SerializeField] private GeneticsGameMode _testGameMode = GeneticsGameMode.Breeding;
        [SerializeField] private GeneticsCompetitionType _testCompetitionType = GeneticsCompetitionType.Individual;
        
        private void Start()
        {
            TestCleanTypes();
            TestCleanManager();
        }
        
        private void TestCleanTypes()
        {
            // Test clean type instantiation
            var breedingResult = new GeneticsBreedingResult();
            var sensoryResponse = new GeneticsSensoryResponse();
            var competitionEntry = new GeneticsCompetitionEntry();
            var breedingChallenge = new GeneticsBreedingChallenge();
            var tournament = new GeneticsTournament();
            
            Debug.Log("Clean genetics types instantiated successfully!");
            Debug.Log($"Test configuration: Difficulty={_testDifficulty}, Mode={_testGameMode}, Competition={_testCompetitionType}");
        }
        
        private void TestCleanManager()
        {
            if (_geneticsManager == null)
            {
                _geneticsManager = FindObjectOfType<CleanGeneticsManager>();
            }
            
            if (_geneticsManager != null)
            {
                Debug.Log($"Clean Genetics Manager found: Ready={_geneticsManager.IsSystemReady}");
                Debug.Log($"Active challenges: {_geneticsManager.GetActiveChallenges().Count}");
                Debug.Log($"Active tournaments: {_geneticsManager.GetActiveTournaments().Count}");
            }
            else
            {
                Debug.LogWarning("No CleanGeneticsManager found in scene");
            }
        }
        
        [ContextMenu("Run Clean Test")]
        public void RunCleanTest()
        {
            TestCleanTypes();
            TestCleanManager();
        }
    }
}