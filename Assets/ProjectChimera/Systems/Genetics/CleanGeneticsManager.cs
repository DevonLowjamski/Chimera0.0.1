using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// Clean, minimal genetics manager that eliminates circular dependencies
    /// Uses only shared types from ProjectChimera.Data.Genetics namespace
    /// </summary>
    public class CleanGeneticsManager : ChimeraManager
    {
        [Header("Clean Genetics Configuration")]
        [SerializeField] private bool _enableBreeding = true;
        [SerializeField] private bool _enableSensoryTraining = true;
        [SerializeField] private bool _enableCompetitions = true;
        
        // Clean data collections
        private List<GeneticsBreedingResult> _breedingResults = new List<GeneticsBreedingResult>();
        private List<GeneticsSensoryResponse> _sensoryResponses = new List<GeneticsSensoryResponse>();
        private List<GeneticsCompetitionEntry> _competitionEntries = new List<GeneticsCompetitionEntry>();
        private List<GeneticsBreedingChallenge> _activeChallenges = new List<GeneticsBreedingChallenge>();
        private List<GeneticsTournament> _activeTournaments = new List<GeneticsTournament>();
        
        // Simple state tracking
        private bool _isInitialized = false;
        private int _totalBreedings = 0;
        private int _totalSensoryTests = 0;
        private int _totalCompetitions = 0;
        
        #region Manager Lifecycle
        
        public override string ManagerName => "Clean Genetics Manager";
        
        protected override void OnManagerInitialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("CleanGeneticsManager already initialized");
                return;
            }
            
            InitializeBasicSystems();
            _isInitialized = true;
            
            Debug.Log("CleanGeneticsManager initialized successfully");
        }
        
        protected override void OnManagerShutdown()
        {
            _isInitialized = false;
            _breedingResults.Clear();
            _sensoryResponses.Clear();
            _competitionEntries.Clear();
            _activeChallenges.Clear();
            _activeTournaments.Clear();
            
            Debug.Log("CleanGeneticsManager shutdown successfully");
        }
        
        private void InitializeBasicSystems()
        {
            // Initialize with minimal data
            if (_enableBreeding)
            {
                CreateSampleBreedingChallenge();
            }
            
            if (_enableCompetitions)
            {
                CreateSampleTournament();
            }
        }
        
        #endregion
        
        #region Public API
        
        public bool IsSystemReady => _isInitialized;
        
        public int GetTotalBreedings() => _totalBreedings;
        public int GetTotalSensoryTests() => _totalSensoryTests;
        public int GetTotalCompetitions() => _totalCompetitions;
        
        public List<GeneticsBreedingChallenge> GetActiveChallenges() => new List<GeneticsBreedingChallenge>(_activeChallenges);
        public List<GeneticsTournament> GetActiveTournaments() => new List<GeneticsTournament>(_activeTournaments);
        
        #endregion
        
        #region Breeding System
        
        public GeneticsBreedingResult PerformBreeding(string parent1, string parent2)
        {
            if (!_isInitialized || !_enableBreeding)
            {
                return new GeneticsBreedingResult { Success = false };
            }
            
            var result = new GeneticsBreedingResult
            {
                Success = true,
                Parent1ID = parent1,
                Parent2ID = parent2,
                OffspringID = System.Guid.NewGuid().ToString(),
                BreedingAccuracy = UnityEngine.Random.Range(0.5f, 1.0f)
            };
            
            _breedingResults.Add(result);
            _totalBreedings++;
            
            return result;
        }
        
        private void CreateSampleBreedingChallenge()
        {
            var challenge = new GeneticsBreedingChallenge
            {
                ChallengeID = System.Guid.NewGuid().ToString(),
                ChallengeName = "Basic Cannabis Breeding",
                Description = "Learn the fundamentals of cannabis genetics",
                MaxGenerations = 3,
                TimeLimit = 600f
            };
            
            challenge.RequiredTraits.Add("THC");
            challenge.RequiredTraits.Add("CBD");
            challenge.AvailableParents.Add("Strain_A");
            challenge.AvailableParents.Add("Strain_B");
            
            _activeChallenges.Add(challenge);
        }
        
        #endregion
        
        #region Sensory System
        
        public GeneticsSensoryResponse PerformSensoryTest(string terpeneID, string userResponse)
        {
            if (!_isInitialized || !_enableSensoryTraining)
            {
                return new GeneticsSensoryResponse { IsCorrect = false };
            }
            
            var response = new GeneticsSensoryResponse
            {
                TerpeneIdentified = userResponse,
                CorrectAnswer = terpeneID,
                IsCorrect = userResponse == terpeneID,
                ResponseTime = UnityEngine.Random.Range(1f, 10f),
                Confidence = UnityEngine.Random.Range(0.3f, 0.9f)
            };
            
            _sensoryResponses.Add(response);
            _totalSensoryTests++;
            
            return response;
        }
        
        #endregion
        
        #region Competition System
        
        public GeneticsCompetitionEntry EnterCompetition(string participantID, string competitionID)
        {
            if (!_isInitialized || !_enableCompetitions)
            {
                return new GeneticsCompetitionEntry { IsActive = false };
            }
            
            var entry = new GeneticsCompetitionEntry
            {
                EntryID = System.Guid.NewGuid().ToString(),
                ParticipantID = participantID,
                CompetitionID = competitionID,
                Score = UnityEngine.Random.Range(50f, 100f)
            };
            
            _competitionEntries.Add(entry);
            _totalCompetitions++;
            
            return entry;
        }
        
        private void CreateSampleTournament()
        {
            var tournament = new GeneticsTournament
            {
                TournamentID = System.Guid.NewGuid().ToString(),
                TournamentName = "Beginner Cannabis Genetics Tournament",
                Description = "A friendly competition for new cultivators",
                MaxParticipants = 50,
                StartTime = System.DateTime.Now,
                EndTime = System.DateTime.Now.AddDays(7)
            };
            
            _activeTournaments.Add(tournament);
        }
        
        #endregion
        
        #region Debug and Testing
        
        [ContextMenu("Test Breeding")]
        private void TestBreeding()
        {
            var result = PerformBreeding("TestParent1", "TestParent2");
            Debug.Log($"Breeding test: Success={result.Success}, Accuracy={result.BreedingAccuracy:F2}");
        }
        
        [ContextMenu("Test Sensory")]
        private void TestSensory()
        {
            var response = PerformSensoryTest("Limonene", "Limonene");
            Debug.Log($"Sensory test: Correct={response.IsCorrect}, Time={response.ResponseTime:F2}s");
        }
        
        [ContextMenu("Print Status")]
        private void PrintStatus()
        {
            Debug.Log($"Genetics Manager Status: Initialized={_isInitialized}, " +
                     $"Breedings={_totalBreedings}, Sensory={_totalSensoryTests}, Competitions={_totalCompetitions}");
        }
        
        #endregion
    }
}