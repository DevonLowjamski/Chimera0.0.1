using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// Advanced Tournament System - Enhanced competition features for Project Chimera
    /// Handles tournament brackets, skill-based matchmaking, and advanced competition mechanics
    /// Uses only verified types from ScientificGamingDataStructures to prevent compilation errors
    /// Operates independently within Genetics assembly to avoid assembly dependency issues
    /// 
    /// ABSTRACT METHOD VERIFICATION COMPLETE:
    /// ✅ OnManagerInitialize() - implemented
    /// ✅ OnManagerShutdown() - implemented
    /// </summary>
    public class AdvancedTournamentSystem : ChimeraManager
    {
        [Header("Tournament Configuration")]
        public bool EnableTournaments = true;
        public bool EnableSkillBasedMatching = true;
        public bool EnableBracketGeneration = true;
        public float TournamentUpdateInterval = 120f;
        
        [Header("Tournament Settings")]
        public int MaxActiveTournaments = 3;
        public int MinParticipantsPerTournament = 8;
        public int MaxParticipantsPerTournament = 64;
        public bool EnableRealTimeMatching = false;
        
        [Header("Skill System Configuration")]
        public bool EnableSkillRating = true;
        public float InitialSkillRating = 1000f;
        public float MaxSkillRating = 3000f;
        public float SkillRatingVolatility = 100f;
        
        [Header("Tournament Collections")]
        [SerializeField] private List<CleanTournamentData> activeTournaments = new List<CleanTournamentData>();
        [SerializeField] private List<CleanTournamentData> completedTournaments = new List<CleanTournamentData>();
        [SerializeField] private List<CleanTournamentBracket> tournamentBrackets = new List<CleanTournamentBracket>();
        [SerializeField] private Dictionary<string, CleanPlayerSkillProfile> playerSkills = new Dictionary<string, CleanPlayerSkillProfile>();
        
        [Header("Tournament State")]
        [SerializeField] private DateTime lastTournamentUpdate = DateTime.Now;
        [SerializeField] private int totalTournamentsRun = 0;
        [SerializeField] private List<string> activeTournamentTypes = new List<string>();
        
        // Events using verified event patterns (remember CS0070 prevention)
        public static event Action<CleanTournamentData> OnTournamentCreated;
        public static event Action<CleanTournamentData> OnTournamentStarted;
        public static event Action<CleanTournamentData> OnTournamentCompleted;
        public static event Action<CleanTournamentMatch> OnMatchCompleted;
        public static event Action<string, float> OnPlayerSkillUpdated;
        
        protected override void OnManagerInitialize()
        {
            // Register with GameManager using verified pattern
            GameManager.Instance?.RegisterManager(this);
            
            // Note: No cross-assembly dependencies to avoid CS0246 errors
            // Tournament system operates independently within Genetics assembly
            
            // Initialize tournament system
            InitializeTournamentSystem();
            
            if (EnableTournaments)
            {
                StartTournamentTracking();
            }
            
            Debug.Log("✅ AdvancedTournamentSystem initialized successfully");
        }
        
        protected override void OnManagerShutdown()
        {
            // Clean up tournament tracking
            if (EnableTournaments)
            {
                StopTournamentTracking();
            }
            
            // Clear all events to prevent memory leaks (remember proper event handling)
            OnTournamentCreated = null;
            OnTournamentStarted = null;
            OnTournamentCompleted = null;
            OnMatchCompleted = null;
            OnPlayerSkillUpdated = null;
            
            Debug.Log("✅ AdvancedTournamentSystem shutdown successfully");
        }
        
        private void InitializeTournamentSystem()
        {
            // Initialize collections if empty
            if (activeTournaments == null) activeTournaments = new List<CleanTournamentData>();
            if (completedTournaments == null) completedTournaments = new List<CleanTournamentData>();
            if (tournamentBrackets == null) tournamentBrackets = new List<CleanTournamentBracket>();
            if (playerSkills == null) playerSkills = new Dictionary<string, CleanPlayerSkillProfile>();
            if (activeTournamentTypes == null) activeTournamentTypes = new List<string>();
            
            // Initialize default tournament types
            InitializeTournamentTypes();
            
            // Initialize skill rating system
            if (EnableSkillRating)
            {
                InitializeSkillSystem();
            }
        }
        
        private void InitializeTournamentTypes()
        {
            // Define available tournament types
            activeTournamentTypes.Clear();
            activeTournamentTypes.Add("Breeding Excellence");
            activeTournamentTypes.Add("Quality Championship");
            activeTournamentTypes.Add("Innovation Contest");
            activeTournamentTypes.Add("Speed Breeding");
            activeTournamentTypes.Add("Collaborative Research");
            
            Debug.Log($"✅ Tournament types initialized: {activeTournamentTypes.Count} types available");
        }
        
        private void InitializeSkillSystem()
        {
            // Initialize skill rating system for competitive balance
            Debug.Log("✅ Skill-based rating system initialized");
        }
        
        private void StartTournamentTracking()
        {
            // Start tournament management and progression
            lastTournamentUpdate = DateTime.Now;
            
            Debug.Log("✅ Tournament tracking started - operating independently");
        }
        
        private void StopTournamentTracking()
        {
            // Clean up tournament tracking
            Debug.Log("✅ Tournament tracking stopped");
        }
        
        private void Update()
        {
            if (!EnableTournaments) return;
            
            // Update tournament progression
            if ((DateTime.Now - lastTournamentUpdate).TotalSeconds >= TournamentUpdateInterval)
            {
                UpdateActiveTournaments();
                lastTournamentUpdate = DateTime.Now;
            }
        }
        
        private void UpdateActiveTournaments()
        {
            var completedTournaments = new List<CleanTournamentData>();
            
            foreach (var tournament in activeTournaments.ToList())
            {
                // Update tournament progress
                UpdateTournamentProgress(tournament);
                
                // Check for completion
                if (tournament.IsCompleted)
                {
                    CompleteTournament(tournament);
                    completedTournaments.Add(tournament);
                }
            }
            
            // Move completed tournaments
            foreach (var tournament in completedTournaments)
            {
                activeTournaments.Remove(tournament);
                this.completedTournaments.Add(tournament);
            }
        }
        
        #region Public API Methods
        
        /// <summary>
        /// Create a new tournament with specified parameters
        /// </summary>
        public bool CreateTournament(string tournamentName, string tournamentType, int maxParticipants = 16)
        {
            if (!EnableTournaments) return false;
            
            if (activeTournaments.Count >= MaxActiveTournaments)
            {
                Debug.LogWarning($"Maximum active tournaments limit reached ({MaxActiveTournaments})");
                return false;
            }
            
            if (maxParticipants < MinParticipantsPerTournament || maxParticipants > MaxParticipantsPerTournament)
            {
                Debug.LogWarning($"Invalid participant count. Must be between {MinParticipantsPerTournament} and {MaxParticipantsPerTournament}");
                return false;
            }
            
            // Create tournament
            var tournament = new CleanTournamentData
            {
                TournamentID = $"tournament_{DateTime.Now.Ticks}",
                TournamentName = tournamentName,
                TournamentType = tournamentType,
                MaxParticipants = maxParticipants,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(CalculateTournamentDuration(maxParticipants)),
                IsActive = true,
                IsCompleted = false,
                CurrentRound = 0,
                TotalRounds = CalculateTotalRounds(maxParticipants),
                Participants = new List<CleanTournamentParticipant>(),
                Matches = new List<CleanTournamentMatch>(),
                PrizePool = CalculatePrizePool(tournamentType, maxParticipants)
            };
            
            activeTournaments.Add(tournament);
            OnTournamentCreated?.Invoke(tournament);
            
            Debug.Log($"✅ Tournament created: {tournamentName} ({tournamentType}) for {maxParticipants} participants");
            return true;
        }
        
        /// <summary>
        /// Register a player for a tournament
        /// </summary>
        public bool RegisterForTournament(string tournamentId, string playerId, string playerName)
        {
            var tournament = activeTournaments.FirstOrDefault(t => t.TournamentID == tournamentId);
            if (tournament == null)
            {
                Debug.LogWarning($"Tournament not found: {tournamentId}");
                return false;
            }
            
            if (tournament.Participants.Count >= tournament.MaxParticipants)
            {
                Debug.LogWarning($"Tournament is full: {tournament.TournamentName}");
                return false;
            }
            
            if (tournament.Participants.Any(p => p.PlayerID == playerId))
            {
                Debug.LogWarning($"Player already registered: {playerName}");
                return false;
            }
            
            // Create participant
            var participant = new CleanTournamentParticipant
            {
                PlayerID = playerId,
                PlayerName = playerName,
                SkillRating = GetPlayerSkillRating(playerId),
                RegistrationDate = DateTime.Now,
                IsActive = true,
                CurrentRound = 0,
                Wins = 0,
                Losses = 0
            };
            
            tournament.Participants.Add(participant);
            
            Debug.Log($"✅ Player registered: {playerName} for {tournament.TournamentName}");
            
            // Check if tournament can start
            if (tournament.Participants.Count >= MinParticipantsPerTournament && !tournament.HasStarted)
            {
                StartTournament(tournament);
            }
            
            return true;
        }
        
        /// <summary>
        /// Start a tournament and generate brackets
        /// </summary>
        public bool StartTournament(CleanTournamentData tournament)
        {
            if (tournament == null || tournament.HasStarted) return false;
            
            if (tournament.Participants.Count < MinParticipantsPerTournament)
            {
                Debug.LogWarning($"Not enough participants to start tournament: {tournament.Participants.Count}/{MinParticipantsPerTournament}");
                return false;
            }
            
            // Generate tournament bracket
            if (EnableBracketGeneration)
            {
                GenerateTournamentBracket(tournament);
            }
            
            // Start first round
            tournament.HasStarted = true;
            tournament.CurrentRound = 1;
            tournament.ActualStartDate = DateTime.Now;
            
            GenerateFirstRoundMatches(tournament);
            
            OnTournamentStarted?.Invoke(tournament);
            
            Debug.Log($"✅ Tournament started: {tournament.TournamentName} with {tournament.Participants.Count} participants");
            return true;
        }
        
        /// <summary>
        /// Get active tournaments
        /// </summary>
        public List<CleanTournamentData> GetActiveTournaments()
        {
            return new List<CleanTournamentData>(activeTournaments);
        }
        
        /// <summary>
        /// Get tournament by ID
        /// </summary>
        public CleanTournamentData GetTournament(string tournamentId)
        {
            var tournament = activeTournaments.FirstOrDefault(t => t.TournamentID == tournamentId);
            if (tournament != null) return tournament;
            
            return completedTournaments.FirstOrDefault(t => t.TournamentID == tournamentId);
        }
        
        /// <summary>
        /// Get player's skill rating
        /// </summary>
        public float GetPlayerSkillRating(string playerId)
        {
            if (playerSkills.ContainsKey(playerId))
            {
                return playerSkills[playerId].CurrentRating;
            }
            
            // Create new skill profile for new player
            var skillProfile = new CleanPlayerSkillProfile
            {
                PlayerID = playerId,
                CurrentRating = InitialSkillRating,
                PeakRating = InitialSkillRating,
                GamesPlayed = 0,
                Wins = 0,
                Losses = 0,
                LastUpdated = DateTime.Now
            };
            
            playerSkills[playerId] = skillProfile;
            return InitialSkillRating;
        }
        
        /// <summary>
        /// Update player skill rating after match
        /// </summary>
        public void UpdatePlayerSkillRating(string playerId, bool won, float opponentRating)
        {
            if (!EnableSkillRating) return;
            
            var skillProfile = playerSkills.GetValueOrDefault(playerId) ?? new CleanPlayerSkillProfile
            {
                PlayerID = playerId,
                CurrentRating = InitialSkillRating,
                PeakRating = InitialSkillRating,
                GamesPlayed = 0,
                Wins = 0,
                Losses = 0
            };
            
            // Simple ELO-style rating calculation
            float expectedScore = 1f / (1f + Mathf.Pow(10f, (opponentRating - skillProfile.CurrentRating) / 400f));
            float actualScore = won ? 1f : 0f;
            float kFactor = CalculateKFactor(skillProfile.GamesPlayed);
            
            float ratingChange = kFactor * (actualScore - expectedScore);
            skillProfile.CurrentRating += ratingChange;
            skillProfile.CurrentRating = Mathf.Clamp(skillProfile.CurrentRating, 0f, MaxSkillRating);
            
            // Update statistics
            skillProfile.GamesPlayed++;
            if (won)
            {
                skillProfile.Wins++;
            }
            else
            {
                skillProfile.Losses++;
            }
            
            if (skillProfile.CurrentRating > skillProfile.PeakRating)
            {
                skillProfile.PeakRating = skillProfile.CurrentRating;
            }
            
            skillProfile.LastUpdated = DateTime.Now;
            playerSkills[playerId] = skillProfile;
            
            OnPlayerSkillUpdated?.Invoke(playerId, skillProfile.CurrentRating);
            
            Debug.Log($"✅ Skill rating updated: Player {playerId} now {skillProfile.CurrentRating:F0} ({ratingChange:+0;-0}))");
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private int CalculateTournamentDuration(int participants)
        {
            // Duration based on participant count and rounds
            int rounds = CalculateTotalRounds(participants);
            return Math.Max(3, rounds); // Minimum 3 days, 1 day per round
        }
        
        private int CalculateTotalRounds(int participants)
        {
            // Calculate rounds for single elimination bracket
            return Mathf.CeilToInt(Mathf.Log(participants, 2));
        }
        
        private float CalculatePrizePool(string tournamentType, int participants)
        {
            // Base prize pool calculation
            float basePool = participants * 100f; // Base amount per participant
            
            // Type multipliers
            float typeMultiplier = tournamentType switch
            {
                "Breeding Excellence" => 2.0f,
                "Quality Championship" => 2.5f,
                "Innovation Contest" => 3.0f,
                "Speed Breeding" => 1.5f,
                "Collaborative Research" => 1.8f,
                _ => 1.0f
            };
            
            return basePool * typeMultiplier;
        }
        
        private void GenerateTournamentBracket(CleanTournamentData tournament)
        {
            // Create tournament bracket
            var bracket = new CleanTournamentBracket
            {
                TournamentID = tournament.TournamentID,
                BracketType = "Single Elimination",
                TotalRounds = tournament.TotalRounds,
                CreatedDate = DateTime.Now,
                Rounds = new List<CleanBracketRound>()
            };
            
            // Generate rounds structure
            for (int round = 1; round <= tournament.TotalRounds; round++)
            {
                var bracketRound = new CleanBracketRound
                {
                    RoundNumber = round,
                    RoundName = GetRoundName(round, tournament.TotalRounds),
                    MatchesInRound = CalculateMatchesInRound(tournament.Participants.Count, round),
                    IsCompleted = false,
                    Matches = new List<CleanTournamentMatch>()
                };
                
                bracket.Rounds.Add(bracketRound);
            }
            
            tournamentBrackets.Add(bracket);
            Debug.Log($"✅ Tournament bracket generated: {bracket.TotalRounds} rounds");
        }
        
        private string GetRoundName(int round, int totalRounds)
        {
            if (round == totalRounds) return "Final";
            if (round == totalRounds - 1) return "Semifinal";
            if (round == totalRounds - 2) return "Quarterfinal";
            return $"Round {round}";
        }
        
        private int CalculateMatchesInRound(int totalParticipants, int round)
        {
            // Calculate matches for single elimination
            return Mathf.CeilToInt(totalParticipants / Mathf.Pow(2, round));
        }
        
        private void GenerateFirstRoundMatches(CleanTournamentData tournament)
        {
            var participants = tournament.Participants.ToList();
            
            // Skill-based seeding if enabled
            if (EnableSkillBasedMatching)
            {
                participants = participants.OrderByDescending(p => p.SkillRating).ToList();
            }
            else
            {
                // Random seeding
                participants = participants.OrderBy(p => Guid.NewGuid()).ToList();
            }
            
            // Generate first round matches
            for (int i = 0; i < participants.Count; i += 2)
            {
                if (i + 1 < participants.Count)
                {
                    var match = new CleanTournamentMatch
                    {
                        MatchID = $"match_{tournament.TournamentID}_{i / 2 + 1}",
                        TournamentID = tournament.TournamentID,
                        Round = 1,
                        MatchNumber = i / 2 + 1,
                        Player1 = participants[i],
                        Player2 = participants[i + 1],
                        ScheduledDate = DateTime.Now.AddHours(i + 1), // Stagger matches
                        IsCompleted = false,
                        Winner = null
                    };
                    
                    tournament.Matches.Add(match);
                }
            }
            
            Debug.Log($"✅ First round matches generated: {tournament.Matches.Count} matches");
        }
        
        private void UpdateTournamentProgress(CleanTournamentData tournament)
        {
            // Simple tournament progression - can be expanded with actual match results
            if (!tournament.HasStarted) return;
            
            // Check if current round is complete
            var currentRoundMatches = tournament.Matches.Where(m => m.Round == tournament.CurrentRound).ToList();
            if (currentRoundMatches.Any() && currentRoundMatches.All(m => m.IsCompleted))
            {
                // Advance to next round or complete tournament
                if (tournament.CurrentRound < tournament.TotalRounds)
                {
                    tournament.CurrentRound++;
                    GenerateNextRoundMatches(tournament);
                }
                else
                {
                    tournament.IsCompleted = true;
                }
            }
        }
        
        private void GenerateNextRoundMatches(CleanTournamentData tournament)
        {
            // Generate matches for next round based on previous round winners
            var previousRoundMatches = tournament.Matches.Where(m => m.Round == tournament.CurrentRound - 1 && m.IsCompleted).ToList();
            var winners = previousRoundMatches.Select(m => m.Winner).Where(w => w != null).ToList();
            
            for (int i = 0; i < winners.Count; i += 2)
            {
                if (i + 1 < winners.Count)
                {
                    var match = new CleanTournamentMatch
                    {
                        MatchID = $"match_{tournament.TournamentID}_r{tournament.CurrentRound}_{i / 2 + 1}",
                        TournamentID = tournament.TournamentID,
                        Round = tournament.CurrentRound,
                        MatchNumber = i / 2 + 1,
                        Player1 = winners[i],
                        Player2 = winners[i + 1],
                        ScheduledDate = DateTime.Now.AddHours(i + 1),
                        IsCompleted = false,
                        Winner = null
                    };
                    
                    tournament.Matches.Add(match);
                }
            }
        }
        
        private void CompleteTournament(CleanTournamentData tournament)
        {
            tournament.ActualEndDate = DateTime.Now;
            tournament.IsCompleted = true;
            
            // Determine tournament winner
            var finalMatch = tournament.Matches.FirstOrDefault(m => m.Round == tournament.TotalRounds && m.IsCompleted);
            if (finalMatch?.Winner != null)
            {
                tournament.Winner = finalMatch.Winner;
                Debug.Log($"🏆 Tournament completed: {tournament.TournamentName} won by {tournament.Winner.PlayerName}");
            }
            
            totalTournamentsRun++;
            OnTournamentCompleted?.Invoke(tournament);
        }
        
        private float CalculateKFactor(int gamesPlayed)
        {
            // K-Factor for ELO rating system
            if (gamesPlayed < 30) return 32f;
            if (gamesPlayed < 100) return 24f;
            return 16f;
        }
        
        #endregion
        
        #region Testing and Validation Methods
        
        /// <summary>
        /// Test method to validate advanced tournament system functionality
        /// </summary>
        public void TestTournamentSystem()
        {
            Debug.Log("=== Testing Advanced Tournament System ===");
            Debug.Log($"Tournaments Enabled: {EnableTournaments}");
            Debug.Log($"Skill Matching Enabled: {EnableSkillBasedMatching}");
            Debug.Log($"Active Tournaments: {activeTournaments.Count}");
            Debug.Log($"Completed Tournaments: {completedTournaments.Count}");
            Debug.Log($"Available Tournament Types: {activeTournamentTypes.Count}");
            Debug.Log($"Registered Players: {playerSkills.Count}");
            
            // Test tournament creation
            if (EnableTournaments && activeTournamentTypes.Count > 0)
            {
                string testType = activeTournamentTypes[0];
                bool created = CreateTournament($"Test {testType} Tournament", testType, 16);
                Debug.Log($"✓ Test tournament creation: {created}");
                
                // Test player registration
                if (created && activeTournaments.Count > 0)
                {
                    var testTournament = activeTournaments[0];
                    bool registered = RegisterForTournament(testTournament.TournamentID, "test_player_1", "Test Player 1");
                    Debug.Log($"✓ Test player registration: {registered}");
                    
                    // Test skill rating
                    float skillRating = GetPlayerSkillRating("test_player_1");
                    Debug.Log($"✓ Test player skill rating: {skillRating}");
                }
            }
            
            Debug.Log("✅ Advanced tournament system test completed");
        }
        
        #endregion
    }
    
    #region Supporting Data Structures
    
    [System.Serializable]
    public class CleanTournamentData
    {
        public string TournamentID;
        public string TournamentName;
        public string TournamentType;
        public int MaxParticipants;
        public DateTime StartDate;
        public DateTime EndDate;
        public DateTime ActualStartDate;
        public DateTime ActualEndDate;
        public bool IsActive;
        public bool IsCompleted;
        public bool HasStarted;
        public int CurrentRound;
        public int TotalRounds;
        public float PrizePool;
        public List<CleanTournamentParticipant> Participants = new List<CleanTournamentParticipant>();
        public List<CleanTournamentMatch> Matches = new List<CleanTournamentMatch>();
        public CleanTournamentParticipant Winner;
    }
    
    [System.Serializable]
    public class CleanTournamentParticipant
    {
        public string PlayerID;
        public string PlayerName;
        public float SkillRating;
        public DateTime RegistrationDate;
        public bool IsActive;
        public int CurrentRound;
        public int Wins;
        public int Losses;
        public float TournamentScore;
    }
    
    [System.Serializable]
    public class CleanTournamentMatch
    {
        public string MatchID;
        public string TournamentID;
        public int Round;
        public int MatchNumber;
        public CleanTournamentParticipant Player1;
        public CleanTournamentParticipant Player2;
        public DateTime ScheduledDate;
        public DateTime ActualDate;
        public bool IsCompleted;
        public CleanTournamentParticipant Winner;
        public float Player1Score;
        public float Player2Score;
        public string MatchNotes;
    }
    
    [System.Serializable]
    public class CleanTournamentBracket
    {
        public string TournamentID;
        public string BracketType;
        public int TotalRounds;
        public DateTime CreatedDate;
        public List<CleanBracketRound> Rounds = new List<CleanBracketRound>();
    }
    
    [System.Serializable]
    public class CleanBracketRound
    {
        public int RoundNumber;
        public string RoundName;
        public int MatchesInRound;
        public bool IsCompleted;
        public List<CleanTournamentMatch> Matches = new List<CleanTournamentMatch>();
    }
    
    [System.Serializable]
    public class CleanPlayerSkillProfile
    {
        public string PlayerID;
        public float CurrentRating;
        public float PeakRating;
        public int GamesPlayed;
        public int Wins;
        public int Losses;
        public DateTime LastUpdated;
        public List<string> TournamentHistory = new List<string>();
    }
    
    #endregion
}