using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.IPM;

namespace ProjectChimera.Systems.IPM
{
    /// <summary>
    /// Comprehensive competitive tournament system for IPM battles and challenges.
    /// Part of PC017-2a: Create tournament framework and scoring system
    /// </summary>
    public class IPMTournamentSystem : ChimeraManager
    {
        [Header("Tournament Configuration")]
        [SerializeField] private bool _enableTournaments = true;
        [SerializeField] private bool _enableRankedMode = true;
        [SerializeField] private bool _enableSeasonalTournaments = true;
        [SerializeField] private int _maxParticipantsPerTournament = 64;
        [SerializeField] private float _tournamentDuration = 3600.0f; // 1 hour
        [SerializeField] private float _registrationPeriod = 1800.0f; // 30 minutes
        
        [Header("Scoring System")]
        [SerializeField] private float _baseScoreMultiplier = 1000.0f;
        [SerializeField] private float _difficultyMultiplier = 1.5f;
        [SerializeField] private float _speedBonusMultiplier = 2.0f;
        [SerializeField] private float _efficiencyBonusMultiplier = 1.8f;
        [SerializeField] private float _innovationBonusMultiplier = 1.3f;
        [SerializeField] private float _environmentalBonusMultiplier = 1.2f;
        
        [Header("Tournament Types")]
        [SerializeField] private bool _enableSingleElimination = true;
        [SerializeField] private bool _enableDoubleElimination = true;
        [SerializeField] private bool _enableRoundRobin = true;
        [SerializeField] private bool _enableSwissTournament = true;
        [SerializeField] private bool _enableKingOfTheHill = true;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onTournamentStarted;
        [SerializeField] private SimpleGameEventSO _onTournamentEnded;
        [SerializeField] private SimpleGameEventSO _onPlayerEliminated;
        [SerializeField] private SimpleGameEventSO _onNewChampion;
        [SerializeField] private SimpleGameEventSO _onScoreUpdated;
        
        // Core tournament data
        private Dictionary<string, Tournament> _activeTournaments = new Dictionary<string, Tournament>();
        private Dictionary<string, TournamentParticipant> _registeredPlayers = new Dictionary<string, TournamentParticipant>();
        private Dictionary<string, PlayerRating> _playerRatings = new Dictionary<string, PlayerRating>();
        private List<TournamentHistory> _tournamentHistory = new List<TournamentHistory>();
        
        // Scoring and ranking systems
        private ScoringEngine _scoringEngine;
        private RankingSystem _rankingSystem;
        private MatchmakingEngine _matchmakingEngine;
        private TournamentBracketGenerator _bracketGenerator;
        
        // Performance tracking
        private int _totalTournamentsHosted;
        private int _totalMatches;
        private float _averageTournamentDuration;
        private Dictionary<TournamentType, int> _tournamentTypeCount = new Dictionary<TournamentType, int>();
        
        #region ChimeraManager Implementation
        
        protected override void OnManagerInitialize()
        {
            InitializeTournamentSystems();
            InitializeScoringSystem();
            InitializeRankingSystem();
            LoadPlayerRatings();
            LoadTournamentHistory();
            
            Debug.Log($"[IPMTournamentSystem] Initialized with tournaments enabled: {_enableTournaments}, " +
                     $"Max participants: {_maxParticipantsPerTournament}, Ranked mode: {_enableRankedMode}");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!IsInitialized || !_enableTournaments) return;
            
            UpdateActiveTournaments();
            ProcessScheduledMatches();
            UpdatePlayerRatings();
            CheckTournamentTimeouts();
        }
        
        protected override void OnManagerShutdown()
        {
            SavePlayerRatings();
            SaveTournamentHistory();
            
            _activeTournaments.Clear();
            _registeredPlayers.Clear();
            _playerRatings.Clear();
            _tournamentHistory.Clear();
            
            Debug.Log("[IPMTournamentSystem] Manager shutdown completed - all tournament data saved");
        }
        
        #endregion
        
        #region Tournament Management
        
        /// <summary>
        /// Create and register a new tournament
        /// </summary>
        public string CreateTournament(TournamentConfiguration config)
        {
            if (!_enableTournaments)
            {
                Debug.LogWarning("[IPMTournamentSystem] Tournaments are disabled");
                return null;
            }
            
            string tournamentId = GenerateTournamentId();
            
            var tournament = new Tournament
            {
                TournamentId = tournamentId,
                Name = config.Name,
                Description = config.Description,
                TournamentType = config.TournamentType,
                DifficultyLevel = config.DifficultyLevel,
                MaxParticipants = Mathf.Min(config.MaxParticipants, _maxParticipantsPerTournament),
                EntryFee = config.EntryFee,
                PrizePool = config.PrizePool,
                RegistrationStartTime = DateTime.Now,
                RegistrationEndTime = DateTime.Now.AddSeconds(_registrationPeriod),
                StartTime = DateTime.Now.AddSeconds(_registrationPeriod + 300), // 5 minutes after registration
                EndTime = DateTime.Now.AddSeconds(_registrationPeriod + 300 + _tournamentDuration),
                Status = TournamentStatus.Registration,
                Participants = new List<TournamentParticipant>(),
                Matches = new List<TournamentMatch>(),
                Rules = config.Rules,
                Restrictions = config.Restrictions,
                CreatedBy = config.CreatedBy,
                CreationTime = DateTime.Now
            };
            
            // Generate tournament bracket based on type
            tournament.Bracket = _bracketGenerator.GenerateBracket(tournament.TournamentType, tournament.MaxParticipants);
            
            _activeTournaments[tournamentId] = tournament;
            _totalTournamentsHosted++;
            
            if (_tournamentTypeCount.ContainsKey(tournament.TournamentType))
                _tournamentTypeCount[tournament.TournamentType]++;
            else
                _tournamentTypeCount[tournament.TournamentType] = 1;
            
            Debug.Log($"[IPMTournamentSystem] Created tournament '{tournament.Name}' ({tournament.TournamentType}) with ID {tournamentId}");
            return tournamentId;
        }
        
        /// <summary>
        /// Register a player for a tournament
        /// </summary>
        public bool RegisterPlayerForTournament(string tournamentId, string playerId, PlayerInfo playerInfo)
        {
            if (!_activeTournaments.ContainsKey(tournamentId))
            {
                Debug.LogWarning($"[IPMTournamentSystem] Tournament {tournamentId} not found");
                return false;
            }
            
            var tournament = _activeTournaments[tournamentId];
            
            if (tournament.Status != TournamentStatus.Registration)
            {
                Debug.LogWarning($"[IPMTournamentSystem] Tournament {tournamentId} is not accepting registrations");
                return false;
            }
            
            if (tournament.Participants.Count >= tournament.MaxParticipants)
            {
                Debug.LogWarning($"[IPMTournamentSystem] Tournament {tournamentId} is full");
                return false;
            }
            
            // Check if player is already registered
            if (tournament.Participants.Any(p => p.PlayerId == playerId))
            {
                Debug.LogWarning($"[IPMTournamentSystem] Player {playerId} is already registered for tournament {tournamentId}");
                return false;
            }
            
            // Create participant entry
            var participant = new TournamentParticipant
            {
                ParticipantId = GenerateParticipantId(),
                PlayerId = playerId,
                PlayerName = playerInfo.PlayerName,
                PlayerLevel = playerInfo.PlayerLevel,
                CurrentRating = GetPlayerRating(playerId),
                RegistrationTime = DateTime.Now,
                Status = ParticipantStatus.Registered,
                Seed = tournament.Participants.Count + 1,
                Wins = 0,
                Losses = 0,
                CurrentScore = 0,
                Statistics = new TournamentStatistics()
            };
            
            tournament.Participants.Add(participant);
            _registeredPlayers[participant.ParticipantId] = participant;
            
            Debug.Log($"[IPMTournamentSystem] Registered player {playerInfo.PlayerName} for tournament {tournament.Name}");
            return true;
        }
        
        /// <summary>
        /// Start a tournament if registration is complete
        /// </summary>
        public bool StartTournament(string tournamentId)
        {
            if (!_activeTournaments.ContainsKey(tournamentId))
            {
                Debug.LogWarning($"[IPMTournamentSystem] Tournament {tournamentId} not found");
                return false;
            }
            
            var tournament = _activeTournaments[tournamentId];
            
            if (tournament.Status != TournamentStatus.Registration)
            {
                Debug.LogWarning($"[IPMTournamentSystem] Tournament {tournamentId} cannot be started from current status: {tournament.Status}");
                return false;
            }
            
            if (tournament.Participants.Count < 2)
            {
                Debug.LogWarning($"[IPMTournamentSystem] Tournament {tournamentId} needs at least 2 participants to start");
                return false;
            }
            
            // Finalize seeding based on player ratings
            FinalizeSeeding(tournament);
            
            // Generate matches for first round
            GenerateFirstRoundMatches(tournament);
            
            tournament.Status = TournamentStatus.InProgress;
            tournament.StartTime = DateTime.Now;
            
            _onTournamentStarted?.Raise();
            
            Debug.Log($"[IPMTournamentSystem] Started tournament {tournament.Name} with {tournament.Participants.Count} participants");
            return true;
        }
        
        #endregion
        
        #region Scoring System
        
        /// <summary>
        /// Calculate match score based on performance metrics
        /// </summary>
        public float CalculateMatchScore(MatchPerformance performance)
        {
            float baseScore = _baseScoreMultiplier;
            
            // Difficulty multiplier
            float difficultyBonus = baseScore * (_difficultyMultiplier - 1.0f) * 
                GetDifficultyMultiplier(performance.DifficultyLevel);
            
            // Speed bonus (faster completion = higher score)
            float speedBonus = 0;
            if (performance.CompletionTime > 0)
            {
                float speedRatio = performance.OptimalTime / performance.CompletionTime;
                speedBonus = baseScore * (_speedBonusMultiplier - 1.0f) * Mathf.Clamp01(speedRatio);
            }
            
            // Efficiency bonus (resource optimization)
            float efficiencyBonus = baseScore * (_efficiencyBonusMultiplier - 1.0f) * 
                performance.ResourceEfficiency;
            
            // Innovation bonus (creative solutions)
            float innovationBonus = baseScore * (_innovationBonusMultiplier - 1.0f) * 
                performance.InnovationScore;
            
            // Environmental bonus (sustainable practices)
            float environmentalBonus = baseScore * (_environmentalBonusMultiplier - 1.0f) * 
                performance.EnvironmentalScore;
            
            // Penalties for failures or poor performance
            float penalties = baseScore * performance.PenaltyMultiplier;
            
            float totalScore = baseScore + difficultyBonus + speedBonus + efficiencyBonus + 
                             innovationBonus + environmentalBonus - penalties;
            
            return Mathf.Max(0, totalScore);
        }
        
        /// <summary>
        /// Process match result and update tournament standings
        /// </summary>
        public void ProcessMatchResult(string tournamentId, string matchId, MatchResult result)
        {
            if (!_activeTournaments.ContainsKey(tournamentId))
            {
                Debug.LogWarning($"[IPMTournamentSystem] Tournament {tournamentId} not found");
                return;
            }
            
            var tournament = _activeTournaments[tournamentId];
            var match = tournament.Matches.FirstOrDefault(m => m.MatchId == matchId);
            
            if (match == null)
            {
                Debug.LogWarning($"[IPMTournamentSystem] Match {matchId} not found in tournament {tournamentId}");
                return;
            }
            
            // Update match with result
            match.Result = result;
            match.Status = MatchStatus.Completed;
            match.EndTime = DateTime.Now;
            
            // Update participant statistics
            UpdateParticipantStats(tournament, match, result);
            
            // Check if tournament round is complete
            if (IsRoundComplete(tournament))
            {
                AdvanceToNextRound(tournament);
            }
            
            // Check if tournament is complete
            if (IsTournamentComplete(tournament))
            {
                CompleteTournament(tournament);
            }
            
            _onScoreUpdated?.Raise();
            _totalMatches++;
            
            Debug.Log($"[IPMTournamentSystem] Processed match result for {matchId} in tournament {tournament.Name}");
        }
        
        #endregion
        
        #region Bracket Management
        
        /// <summary>
        /// Generate first round matches based on tournament type and seeding
        /// </summary>
        private void GenerateFirstRoundMatches(Tournament tournament)
        {
            tournament.Matches.Clear();
            
            switch (tournament.TournamentType)
            {
                case TournamentType.SingleElimination:
                    GenerateSingleEliminationMatches(tournament);
                    break;
                case TournamentType.DoubleElimination:
                    GenerateDoubleEliminationMatches(tournament);
                    break;
                case TournamentType.RoundRobin:
                    GenerateRoundRobinMatches(tournament);
                    break;
                case TournamentType.Swiss:
                    GenerateSwissMatches(tournament);
                    break;
                case TournamentType.KingOfTheHill:
                    GenerateKingOfTheHillMatches(tournament);
                    break;
            }
            
            tournament.CurrentRound = 1;
            tournament.TotalRounds = CalculateTotalRounds(tournament);
        }
        
        /// <summary>
        /// Generate single elimination bracket matches
        /// </summary>
        private void GenerateSingleEliminationMatches(Tournament tournament)
        {
            var participants = tournament.Participants.OrderBy(p => p.Seed).ToList();
            int matchNumber = 1;
            
            for (int i = 0; i < participants.Count; i += 2)
            {
                if (i + 1 < participants.Count)
                {
                    var match = new TournamentMatch
                    {
                        MatchId = GenerateMatchId(tournament.TournamentId, matchNumber),
                        TournamentId = tournament.TournamentId,
                        Round = 1,
                        MatchNumber = matchNumber,
                        Player1Id = participants[i].ParticipantId,
                        Player2Id = participants[i + 1].ParticipantId,
                        Status = MatchStatus.Scheduled,
                        ScheduledTime = DateTime.Now.AddMinutes(matchNumber * 5), // Stagger matches
                        EstimatedDuration = TimeSpan.FromMinutes(30),
                        MatchType = MatchType.Elimination
                    };
                    
                    tournament.Matches.Add(match);
                    matchNumber++;
                }
                else
                {
                    // Bye for odd number of participants
                    participants[i].Status = ParticipantStatus.AdvancedByBye;
                }
            }
        }
        
        #endregion
        
        #region Rating and Ranking System
        
        /// <summary>
        /// Update player rating based on tournament performance
        /// </summary>
        private void UpdatePlayerRating(string playerId, TournamentResult result)
        {
            if (!_playerRatings.ContainsKey(playerId))
            {
                _playerRatings[playerId] = new PlayerRating
                {
                    PlayerId = playerId,
                    CurrentRating = 1200, // Starting rating
                    PeakRating = 1200,
                    LowestRating = 1200,
                    TournamentCount = 0,
                    WinCount = 0,
                    LossCount = 0
                };
            }
            
            var rating = _playerRatings[playerId];
            
            // Calculate rating change using modified ELO system
            float ratingChange = CalculateRatingChange(rating, result);
            
            rating.CurrentRating += ratingChange;
            rating.PeakRating = Mathf.Max(rating.PeakRating, rating.CurrentRating);
            rating.LowestRating = Mathf.Min(rating.LowestRating, rating.CurrentRating);
            rating.TournamentCount++;
            
            if (result.Placement <= result.TotalParticipants / 2)
                rating.WinCount++;
            else
                rating.LossCount++;
            
            rating.LastUpdated = DateTime.Now;
            
            Debug.Log($"[IPMTournamentSystem] Updated rating for {playerId}: {rating.CurrentRating:F0} ({ratingChange:+0;-0})");
        }
        
        /// <summary>
        /// Calculate rating change using ELO-based system
        /// </summary>
        private float CalculateRatingChange(PlayerRating playerRating, TournamentResult result)
        {
            // K-factor based on rating and tournament count
            float kFactor = CalculateKFactor(playerRating);
            
            // Expected score based on placement
            float expectedScore = CalculateExpectedScore(playerRating.CurrentRating, result);
            
            // Actual score based on performance
            float actualScore = CalculateActualScore(result);
            
            return kFactor * (actualScore - expectedScore);
        }
        
        #endregion
        
        #region Public API Methods
        
        /// <summary>
        /// Get comprehensive tournament information
        /// </summary>
        public TournamentInfo GetTournamentInfo(string tournamentId)
        {
            if (!_activeTournaments.ContainsKey(tournamentId))
                return null;
            
            var tournament = _activeTournaments[tournamentId];
            
            return new TournamentInfo
            {
                Tournament = tournament,
                CurrentStandings = GetCurrentStandings(tournament),
                NextMatches = GetUpcomingMatches(tournament),
                CompletedMatches = tournament.Matches.Where(m => m.Status == MatchStatus.Completed).ToList(),
                Statistics = CalculateTournamentStatistics(tournament)
            };
        }
        
        /// <summary>
        /// Get player's tournament history and statistics
        /// </summary>
        public PlayerTournamentRecord GetPlayerRecord(string playerId)
        {
            var playerHistory = _tournamentHistory.Where(t => 
                t.Results.Any(r => r.PlayerId == playerId)).ToList();
            
            var record = new PlayerTournamentRecord
            {
                PlayerId = playerId,
                TotalTournaments = playerHistory.Count,
                TotalWins = 0,
                TotalLosses = 0,
                BestPlacement = int.MaxValue,
                AverageScore = 0,
                FavoriteStrategy = StrategyType.Integrated,
                RecentForm = new List<TournamentResult>()
            };
            
            foreach (var tournament in playerHistory)
            {
                var playerResult = tournament.Results.First(r => r.PlayerId == playerId);
                record.TotalWins += playerResult.Wins;
                record.TotalLosses += playerResult.Losses;
                record.BestPlacement = Mathf.Min(record.BestPlacement, playerResult.Placement);
                record.AverageScore += playerResult.FinalScore;
                record.RecentForm.Add(playerResult);
            }
            
            if (playerHistory.Count > 0)
            {
                record.AverageScore /= playerHistory.Count;
                record.RecentForm = record.RecentForm.OrderByDescending(r => r.TournamentEndTime)
                    .Take(10).ToList();
            }
            
            return record;
        }
        
        /// <summary>
        /// Get current tournament system metrics
        /// </summary>
        public TournamentSystemMetrics GetSystemMetrics()
        {
            return new TournamentSystemMetrics
            {
                TotalTournamentsHosted = _totalTournamentsHosted,
                ActiveTournaments = _activeTournaments.Count,
                TotalRegisteredPlayers = _registeredPlayers.Count,
                TotalMatches = _totalMatches,
                AverageTournamentDuration = _averageTournamentDuration,
                TournamentTypeDistribution = new Dictionary<TournamentType, int>(_tournamentTypeCount),
                AverageParticipantsPerTournament = _activeTournaments.Count > 0 ? 
                    (float)_activeTournaments.Values.Average(t => t.Participants.Count) : 0,
                LastUpdateTime = DateTime.Now
            };
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private void InitializeTournamentSystems()
        {
            _activeTournaments.Clear();
            _registeredPlayers.Clear();
            _playerRatings.Clear();
            _tournamentHistory.Clear();
            _tournamentTypeCount.Clear();
            
            _totalTournamentsHosted = 0;
            _totalMatches = 0;
            _averageTournamentDuration = 0;
        }
        
        private void InitializeScoringSystem()
        {
            _scoringEngine = new ScoringEngine(_baseScoreMultiplier, _difficultyMultiplier);
        }
        
        private void InitializeRankingSystem()
        {
            _rankingSystem = new RankingSystem();
            _matchmakingEngine = new MatchmakingEngine();
            _bracketGenerator = new TournamentBracketGenerator();
        }
        
        private void UpdateActiveTournaments()
        {
            var tournamentsToUpdate = _activeTournaments.Values.ToList();
            foreach (var tournament in tournamentsToUpdate)
            {
                UpdateTournamentStatus(tournament);
            }
        }
        
        private void ProcessScheduledMatches()
        {
            foreach (var tournament in _activeTournaments.Values)
            {
                var scheduledMatches = tournament.Matches.Where(m => 
                    m.Status == MatchStatus.Scheduled && 
                    DateTime.Now >= m.ScheduledTime).ToList();
                
                foreach (var match in scheduledMatches)
                {
                    match.Status = MatchStatus.InProgress;
                    match.StartTime = DateTime.Now;
                }
            }
        }
        
        private void UpdatePlayerRatings()
        {
            // Placeholder for periodic rating updates
        }
        
        private void CheckTournamentTimeouts()
        {
            var expiredTournaments = _activeTournaments.Values.Where(t => 
                DateTime.Now > t.EndTime && t.Status == TournamentStatus.InProgress).ToList();
                
            foreach (var tournament in expiredTournaments)
            {
                CompleteTournament(tournament);
            }
        }
        
        // Placeholder methods for complex tournament logic
        private string GenerateTournamentId() => $"TOURN_{DateTime.Now.Ticks}";
        private string GenerateParticipantId() => $"PART_{DateTime.Now.Ticks}";
        private string GenerateMatchId(string tournamentId, int matchNumber) => $"{tournamentId}_M{matchNumber:D3}";
        private void LoadPlayerRatings() { }
        private void SavePlayerRatings() { }
        private void LoadTournamentHistory() { }
        private void SaveTournamentHistory() { }
        private float GetPlayerRating(string playerId) => _playerRatings.ContainsKey(playerId) ? _playerRatings[playerId].CurrentRating : 1200;
        private void FinalizeSeeding(Tournament tournament) { }
        private float GetDifficultyMultiplier(IPMDifficultyLevel difficulty) => (float)difficulty / 10.0f;
        private void UpdateParticipantStats(Tournament tournament, TournamentMatch match, MatchResult result) { }
        private bool IsRoundComplete(Tournament tournament) => !tournament.Matches.Any(m => m.Round == tournament.CurrentRound && m.Status != MatchStatus.Completed);
        private void AdvanceToNextRound(Tournament tournament) { tournament.CurrentRound++; }
        private bool IsTournamentComplete(Tournament tournament) => tournament.CurrentRound >= tournament.TotalRounds;
        private void CompleteTournament(Tournament tournament) { tournament.Status = TournamentStatus.Completed; _onTournamentEnded?.Raise(); }
        private int CalculateTotalRounds(Tournament tournament) => Mathf.CeilToInt(Mathf.Log(tournament.Participants.Count, 2));
        private void GenerateDoubleEliminationMatches(Tournament tournament) { }
        private void GenerateRoundRobinMatches(Tournament tournament) { }
        private void GenerateSwissMatches(Tournament tournament) { }
        private void GenerateKingOfTheHillMatches(Tournament tournament) { }
        private void UpdateTournamentStatus(Tournament tournament) { }
        private float CalculateKFactor(PlayerRating rating) => 32.0f; // Standard K-factor
        private float CalculateExpectedScore(float rating, TournamentResult result) => 0.5f;
        private float CalculateActualScore(TournamentResult result) => result.Placement <= 3 ? 1.0f : 0.0f;
        private List<TournamentParticipant> GetCurrentStandings(Tournament tournament) => tournament.Participants.OrderByDescending(p => p.CurrentScore).ToList();
        private List<TournamentMatch> GetUpcomingMatches(Tournament tournament) => tournament.Matches.Where(m => m.Status == MatchStatus.Scheduled).Take(5).ToList();
        private TournamentStatistics CalculateTournamentStatistics(Tournament tournament) => new TournamentStatistics();
        
        #endregion
    }
}