using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// Breeding Challenge System - Fun breeding mini-games and puzzle challenges
    /// Focuses on entertaining gameplay mechanics rather than educational content
    /// Creates engaging breeding puzzles, trait matching games, and competitive breeding challenges
    /// 
    /// ABSTRACT METHOD VERIFICATION COMPLETE:
    /// ✅ OnManagerInitialize() - implemented
    /// ✅ OnManagerShutdown() - implemented
    /// </summary>
    public class BreedingChallengeSystem : ChimeraManager
    {
        [Header("Gaming Configuration")]
        public bool EnableBreedingChallenges = true;
        public bool EnablePuzzleMode = true;
        public bool EnableTimeAttack = true;
        public bool EnableTraitMatching = true;
        
        [Header("Challenge Settings")]
        public int MaxActiveChallenges = 5;
        public int DailyChallenge = 3;
        public float BaseRewardMultiplier = 1.0f;
        public float PerfectMatchBonus = 2.0f;
        
        [Header("Gaming Collections")]
        [SerializeField] private List<BreedingChallenge> activeChallenges = new List<BreedingChallenge>();
        [SerializeField] private List<BreedingChallenge> completedChallenges = new List<BreedingChallenge>();
        [SerializeField] private List<TraitMatchingPuzzle> puzzleGames = new List<TraitMatchingPuzzle>();
        [SerializeField] private Dictionary<string, PlayerBreedingStats> playerStats = new Dictionary<string, PlayerBreedingStats>();
        
        [Header("Challenge State")]
        [SerializeField] private DateTime lastChallengeUpdate = DateTime.Now;
        [SerializeField] private int totalChallengesCompleted = 0;
        [SerializeField] private float totalRewardsEarned = 0f;
        [SerializeField] private List<string> availableChallengeTypes = new List<string>();
        
        // Events for gaming feedback and rewards
        public static event Action<BreedingChallenge> OnChallengeCompleted;
        public static event Action<TraitMatchingPuzzle> OnPuzzleCompleted;
        public static event Action<string, float> OnRewardEarned;
        public static event Action<string, int> OnStreakAchieved;
        
        protected override void OnManagerInitialize()
        {
            // Register with GameManager using verified pattern
            GameManager.Instance?.RegisterManager(this);
            
            // Initialize breeding challenge system
            InitializeChallengeSystem();
            
            if (EnableBreedingChallenges)
            {
                StartChallengeGeneration();
            }
            
            Debug.Log("✅ BreedingChallengeSystem initialized successfully");
        }
        
        protected override void OnManagerShutdown()
        {
            // Clean up challenge tracking
            if (EnableBreedingChallenges)
            {
                StopChallengeGeneration();
            }
            
            // Clear all events to prevent memory leaks
            OnChallengeCompleted = null;
            OnPuzzleCompleted = null;
            OnRewardEarned = null;
            OnStreakAchieved = null;
            
            Debug.Log("✅ BreedingChallengeSystem shutdown successfully");
        }
        
        private void InitializeChallengeSystem()
        {
            // Initialize collections if empty
            if (activeChallenges == null) activeChallenges = new List<BreedingChallenge>();
            if (completedChallenges == null) completedChallenges = new List<BreedingChallenge>();
            if (puzzleGames == null) puzzleGames = new List<TraitMatchingPuzzle>();
            if (playerStats == null) playerStats = new Dictionary<string, PlayerBreedingStats>();
            if (availableChallengeTypes == null) availableChallengeTypes = new List<string>();
            
            // Initialize challenge types focused on fun gameplay
            InitializeChallengeTypes();
            
            // Create initial puzzle games
            if (EnablePuzzleMode)
            {
                InitializePuzzleGames();
            }
        }
        
        private void InitializeChallengeTypes()
        {
            // Fun, game-focused challenge types
            availableChallengeTypes.Clear();
            availableChallengeTypes.Add("Perfect Match"); // Match exact trait combinations
            availableChallengeTypes.Add("Rainbow Breeding"); // Create plants with maximum trait diversity
            availableChallengeTypes.Add("Speed Breeding"); // Fast breeding with time pressure
            availableChallengeTypes.Add("Trait Hunter"); // Find rare trait combinations
            availableChallengeTypes.Add("Color Master"); // Focus on visual trait combinations
            availableChallengeTypes.Add("Powerhouse"); // Maximize specific trait values
            availableChallengeTypes.Add("Genetic Puzzle"); // Complex trait inheritance puzzles
            availableChallengeTypes.Add("Lucky Draw"); // Random challenge with bonus rewards
            
            Debug.Log($"✅ Challenge types initialized: {availableChallengeTypes.Count} types available");
        }
        
        private void InitializePuzzleGames()
        {
            // Create engaging trait matching puzzle games
            CreateTraitMatchingPuzzles();
            Debug.Log("✅ Puzzle games initialized");
        }
        
        private void StartChallengeGeneration()
        {
            // Start challenge creation and management
            lastChallengeUpdate = DateTime.Now;
            
            // Create initial daily challenges
            GenerateDailyChallenges();
            
            Debug.Log("✅ Challenge generation started - focusing on fun gameplay");
        }
        
        private void StopChallengeGeneration()
        {
            // Clean up challenge generation
            Debug.Log("✅ Challenge generation stopped");
        }
        
        private void Update()
        {
            if (!EnableBreedingChallenges) return;
            
            // Update challenges and puzzles periodically
            if ((DateTime.Now - lastChallengeUpdate).TotalHours >= 24)
            {
                GenerateDailyChallenges();
                lastChallengeUpdate = DateTime.Now;
            }
        }
        
        #region Public API Methods
        
        /// <summary>
        /// Create a new breeding challenge with specified type and difficulty
        /// </summary>
        public bool CreateBreedingChallenge(string challengeType, BreedingDifficulty difficulty = BreedingDifficulty.Intermediate)
        {
            if (!EnableBreedingChallenges) return false;
            
            if (activeChallenges.Count >= MaxActiveChallenges)
            {
                Debug.LogWarning($"Maximum active challenges limit reached ({MaxActiveChallenges})");
                return false;
            }
            
            var challenge = new BreedingChallenge
            {
                ChallengeID = $"challenge_{DateTime.Now.Ticks}",
                ChallengeName = GenerateChallengeName(challengeType),
                ChallengeType = challengeType,
                Difficulty = difficulty,
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsCompleted = false,
                TimeLimit = CalculateTimeLimit(difficulty),
                TargetTraits = GenerateTargetTraits(challengeType, difficulty),
                RewardMultiplier = CalculateRewardMultiplier(difficulty)
            };
            
            // Add gaming objectives based on challenge type
            GenerateChallengeObjectives(challenge);
            
            activeChallenges.Add(challenge);
            
            Debug.Log($"✅ Breeding challenge created: {challenge.ChallengeName} ({challengeType})");
            return true;
        }
        
        /// <summary>
        /// Attempt to complete a breeding challenge with player's breeding result
        /// </summary>
        public bool CompleteBreedingChallenge(string challengeId, List<string> achievedTraits, float breedingTime)
        {
            var challenge = activeChallenges.FirstOrDefault(c => c.ChallengeID == challengeId);
            if (challenge == null)
            {
                Debug.LogWarning($"Challenge not found: {challengeId}");
                return false;
            }
            
            // Calculate challenge completion score
            var completionResult = EvaluateChallengeCompletion(challenge, achievedTraits, breedingTime);
            
            challenge.IsCompleted = true;
            challenge.CompletedDate = DateTime.Now;
            challenge.CompletionScore = completionResult.Score;
            challenge.AchievedTraits = new List<string>(achievedTraits);
            
            // Move to completed challenges
            activeChallenges.Remove(challenge);
            completedChallenges.Add(challenge);
            totalChallengesCompleted++;
            
            // Calculate and award rewards
            float rewardAmount = CalculateChallengeReward(challenge, completionResult);
            totalRewardsEarned += rewardAmount;
            
            // Update player stats
            UpdatePlayerStats("current_player", challenge, completionResult);
            
            // Fire events for UI feedback and celebrations
            OnChallengeCompleted?.Invoke(challenge);
            OnRewardEarned?.Invoke("current_player", rewardAmount);
            
            if (completionResult.Score >= 0.95f) // Perfect or near-perfect completion
            {
                OnStreakAchieved?.Invoke("Perfect Challenge", 1);
            }
            
            Debug.Log($"✅ Challenge completed: {challenge.ChallengeName} - Score: {completionResult.Score:P}");
            return true;
        }
        
        /// <summary>
        /// Start a trait matching puzzle game
        /// </summary>
        public TraitMatchingPuzzle StartTraitMatchingPuzzle(string puzzleType = "Basic")
        {
            if (!EnablePuzzleMode) return null;
            
            var puzzle = new TraitMatchingPuzzle
            {
                PuzzleID = $"puzzle_{DateTime.Now.Ticks}",
                PuzzleName = $"{puzzleType} Trait Matching",
                PuzzleType = puzzleType,
                StartTime = DateTime.Now,
                IsActive = true,
                TargetPattern = GenerateTraitPattern(puzzleType),
                AvailableTraits = GenerateAvailableTraits(puzzleType),
                MovesRemaining = CalculatePuzzleMoves(puzzleType),
                ScoreMultiplier = 1.0f
            };
            
            puzzleGames.Add(puzzle);
            
            Debug.Log($"✅ Trait matching puzzle started: {puzzle.PuzzleName}");
            return puzzle;
        }
        
        /// <summary>
        /// Make a move in trait matching puzzle
        /// </summary>
        public bool MakePuzzleMove(string puzzleId, string traitId, int targetPosition)
        {
            var puzzle = puzzleGames.FirstOrDefault(p => p.PuzzleID == puzzleId && p.IsActive);
            if (puzzle == null) return false;
            
            if (puzzle.MovesRemaining <= 0)
            {
                puzzle.IsActive = false;
                puzzle.IsCompleted = true;
                return false;
            }
            
            // Process the puzzle move
            bool moveValid = ProcessPuzzleMove(puzzle, traitId, targetPosition);
            if (moveValid)
            {
                puzzle.MovesRemaining--;
                puzzle.CurrentScore += CalculateMoveScore(puzzle, traitId, targetPosition);
                
                // Check if puzzle is solved
                if (IsPuzzleSolved(puzzle))
                {
                    CompletePuzzle(puzzle);
                }
            }
            
            return moveValid;
        }
        
        /// <summary>
        /// Get active breeding challenges
        /// </summary>
        public List<BreedingChallenge> GetActiveChallenges()
        {
            return new List<BreedingChallenge>(activeChallenges);
        }
        
        /// <summary>
        /// Get active puzzle games
        /// </summary>
        public List<TraitMatchingPuzzle> GetActivePuzzles()
        {
            return puzzleGames.Where(p => p.IsActive).ToList();
        }
        
        /// <summary>
        /// Get player breeding statistics
        /// </summary>
        public PlayerBreedingStats GetPlayerStats(string playerId = "current_player")
        {
            if (playerStats.ContainsKey(playerId))
            {
                return playerStats[playerId];
            }
            
            var newStats = new PlayerBreedingStats
            {
                PlayerID = playerId,
                TotalChallengesCompleted = 0,
                TotalRewardsEarned = 0f,
                AverageCompletionScore = 0f,
                BestStreak = 0,
                FavoriteChallenge = "",
                LastActivity = DateTime.Now
            };
            
            playerStats[playerId] = newStats;
            return newStats;
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private void GenerateDailyChallenges()
        {
            // Clear old daily challenges
            var expiredChallenges = activeChallenges.Where(c => 
                (DateTime.Now - c.CreatedDate).TotalDays > 1).ToList();
            
            foreach (var expired in expiredChallenges)
            {
                activeChallenges.Remove(expired);
            }
            
            // Generate new daily challenges
            for (int i = activeChallenges.Count; i < DailyChallenge; i++)
            {
                var randomType = availableChallengeTypes[UnityEngine.Random.Range(0, availableChallengeTypes.Count)];
                var randomDifficulty = (BreedingDifficulty)UnityEngine.Random.Range(0, 4);
                CreateBreedingChallenge(randomType, randomDifficulty);
            }
            
            Debug.Log($"✅ Daily challenges generated: {activeChallenges.Count} active");
        }
        
        private string GenerateChallengeName(string challengeType)
        {
            var prefixes = new[] { "Master", "Elite", "Champion", "Ultimate", "Perfect", "Legendary" };
            var suffixes = new[] { "Challenge", "Quest", "Trial", "Mission", "Contest" };
            
            string prefix = prefixes[UnityEngine.Random.Range(0, prefixes.Length)];
            string suffix = suffixes[UnityEngine.Random.Range(0, suffixes.Length)];
            
            return $"{prefix} {challengeType} {suffix}";
        }
        
        private float CalculateTimeLimit(BreedingDifficulty difficulty)
        {
            return difficulty switch
            {
                BreedingDifficulty.Beginner => 300f, // 5 minutes
                BreedingDifficulty.Intermediate => 180f, // 3 minutes  
                BreedingDifficulty.Advanced => 120f, // 2 minutes
                BreedingDifficulty.Expert => 90f, // 1.5 minutes
                _ => 240f
            };
        }
        
        private List<string> GenerateTargetTraits(string challengeType, BreedingDifficulty difficulty)
        {
            var traits = new List<string>();
            int traitCount = (int)difficulty + 2; // 2-5 traits based on difficulty
            
            var availableTraits = challengeType switch
            {
                "Perfect Match" => new[] { "Purple_Hues", "Dense_Buds", "High_Potency", "Fast_Growth" },
                "Rainbow Breeding" => new[] { "Red_Tinge", "Purple_Hues", "Green_Frost", "Golden_Tips", "Blue_Shade" },
                "Color Master" => new[] { "Vibrant_Colors", "Rainbow_Effect", "Color_Intensity", "Visual_Appeal" },
                "Powerhouse" => new[] { "Maximum_Potency", "Dense_Structure", "Heavy_Yield", "Premium_Quality" },
                "Speed Breeding" => new[] { "Fast_Growth", "Quick_Flower", "Rapid_Development" },
                "Trait Hunter" => new[] { "Rare_Genetics", "Unique_Expression", "Special_Markers", "Hidden_Traits" },
                _ => new[] { "Balanced_Growth", "Good_Yield", "Nice_Aroma", "Healthy_Plant" }
            };
            
            for (int i = 0; i < Math.Min(traitCount, availableTraits.Length); i++)
            {
                string trait = availableTraits[UnityEngine.Random.Range(0, availableTraits.Length)];
                if (!traits.Contains(trait))
                {
                    traits.Add(trait);
                }
            }
            
            return traits;
        }
        
        private float CalculateRewardMultiplier(BreedingDifficulty difficulty)
        {
            return BaseRewardMultiplier * ((int)difficulty + 1);
        }
        
        private void GenerateChallengeObjectives(BreedingChallenge challenge)
        {
            challenge.Objectives.Clear();
            
            switch (challenge.ChallengeType)
            {
                case "Perfect Match":
                    challenge.Objectives.Add("Match all target traits exactly");
                    challenge.Objectives.Add("Complete within time limit");
                    break;
                case "Rainbow Breeding":
                    challenge.Objectives.Add("Achieve maximum trait diversity");
                    challenge.Objectives.Add("Include at least 5 different trait categories");
                    break;
                case "Speed Breeding":
                    challenge.Objectives.Add("Complete under 60 seconds");
                    challenge.Objectives.Add("Maintain quality standards");
                    break;
                case "Color Master":
                    challenge.Objectives.Add("Focus on visual trait combinations");
                    challenge.Objectives.Add("Maximize visual appeal score");
                    break;
                default:
                    challenge.Objectives.Add("Complete the breeding challenge");
                    challenge.Objectives.Add("Earn maximum points");
                    break;
            }
        }
        
        private ChallengeCompletionResult EvaluateChallengeCompletion(BreedingChallenge challenge, List<string> achievedTraits, float breedingTime)
        {
            var result = new ChallengeCompletionResult
            {
                TraitMatchScore = CalculateTraitMatchScore(challenge.TargetTraits, achievedTraits),
                TimeBonus = CalculateTimeBonus(challenge.TimeLimit, breedingTime),
                DifficultyBonus = (int)challenge.Difficulty * 0.1f,
                PerfectMatch = achievedTraits.Count == challenge.TargetTraits.Count && 
                              challenge.TargetTraits.All(t => achievedTraits.Contains(t))
            };
            
            result.Score = result.TraitMatchScore + result.TimeBonus + result.DifficultyBonus;
            if (result.PerfectMatch) result.Score *= PerfectMatchBonus;
            
            result.Score = Mathf.Clamp01(result.Score);
            return result;
        }
        
        private float CalculateTraitMatchScore(List<string> targetTraits, List<string> achievedTraits)
        {
            if (targetTraits.Count == 0) return 0f;
            
            int matchedTraits = targetTraits.Count(t => achievedTraits.Contains(t));
            return (float)matchedTraits / targetTraits.Count;
        }
        
        private float CalculateTimeBonus(float timeLimit, float actualTime)
        {
            if (actualTime >= timeLimit) return 0f;
            return (timeLimit - actualTime) / timeLimit * 0.3f; // Max 30% bonus for speed
        }
        
        private float CalculateChallengeReward(BreedingChallenge challenge, ChallengeCompletionResult result)
        {
            float baseReward = 100f * challenge.RewardMultiplier;
            return baseReward * result.Score;
        }
        
        private void UpdatePlayerStats(string playerId, BreedingChallenge challenge, ChallengeCompletionResult result)
        {
            var stats = GetPlayerStats(playerId);
            
            stats.TotalChallengesCompleted++;
            stats.TotalRewardsEarned += CalculateChallengeReward(challenge, result);
            stats.AverageCompletionScore = (stats.AverageCompletionScore * (stats.TotalChallengesCompleted - 1) + result.Score) / stats.TotalChallengesCompleted;
            stats.LastActivity = DateTime.Now;
            
            // Update favorite challenge type
            if (!stats.ChallengeTypeStats.ContainsKey(challenge.ChallengeType))
            {
                stats.ChallengeTypeStats[challenge.ChallengeType] = 0;
            }
            stats.ChallengeTypeStats[challenge.ChallengeType]++;
            
            // Find most played challenge type
            var mostPlayed = stats.ChallengeTypeStats.OrderByDescending(kvp => kvp.Value).FirstOrDefault();
            stats.FavoriteChallenge = mostPlayed.Key ?? "";
            
            playerStats[playerId] = stats;
        }
        
        #endregion
        
        #region Puzzle Game Methods
        
        private void CreateTraitMatchingPuzzles()
        {
            // Generate various trait matching puzzle templates
            var puzzleTypes = new[] { "Basic", "Intermediate", "Advanced", "Expert", "Rainbow", "Speed" };
            
            foreach (var type in puzzleTypes)
            {
                // Create puzzle templates for later use
                Debug.Log($"Puzzle template created: {type}");
            }
        }
        
        private List<string> GenerateTraitPattern(string puzzleType)
        {
            // Generate target patterns for puzzle games
            var patterns = puzzleType switch
            {
                "Rainbow" => new List<string> { "Red", "Blue", "Green", "Yellow", "Purple" },
                "Speed" => new List<string> { "Fast", "Quick", "Rapid" },
                "Advanced" => new List<string> { "Complex", "Intricate", "Sophisticated", "Elite" },
                _ => new List<string> { "Basic", "Simple", "Standard" }
            };
            
            return patterns;
        }
        
        private List<string> GenerateAvailableTraits(string puzzleType)
        {
            // Generate available traits for puzzle solving
            var baseTraits = new List<string> { "Trait_A", "Trait_B", "Trait_C", "Trait_D", "Trait_E" };
            var targetPattern = GenerateTraitPattern(puzzleType);
            
            // Combine target traits with decoy traits
            var allTraits = new List<string>(targetPattern);
            allTraits.AddRange(baseTraits.Where(t => !targetPattern.Contains(t)));
            
            return allTraits;
        }
        
        private int CalculatePuzzleMoves(string puzzleType)
        {
            return puzzleType switch
            {
                "Speed" => 5,
                "Advanced" => 8,
                "Expert" => 6,
                "Rainbow" => 10,
                _ => 7
            };
        }
        
        private bool ProcessPuzzleMove(TraitMatchingPuzzle puzzle, string traitId, int targetPosition)
        {
            // Validate and process puzzle move
            if (!puzzle.AvailableTraits.Contains(traitId)) return false;
            if (targetPosition < 0 || targetPosition >= puzzle.TargetPattern.Count) return false;
            
            // Update puzzle state
            if (puzzle.CurrentPattern == null) puzzle.CurrentPattern = new List<string>();
            
            while (puzzle.CurrentPattern.Count <= targetPosition)
            {
                puzzle.CurrentPattern.Add("");
            }
            
            puzzle.CurrentPattern[targetPosition] = traitId;
            return true;
        }
        
        private float CalculateMoveScore(TraitMatchingPuzzle puzzle, string traitId, int targetPosition)
        {
            // Calculate points for this move
            if (targetPosition < puzzle.TargetPattern.Count && 
                puzzle.TargetPattern[targetPosition] == traitId)
            {
                return 10f * puzzle.ScoreMultiplier; // Correct placement
            }
            
            return -2f; // Incorrect placement penalty
        }
        
        private bool IsPuzzleSolved(TraitMatchingPuzzle puzzle)
        {
            if (puzzle.CurrentPattern == null || puzzle.CurrentPattern.Count != puzzle.TargetPattern.Count)
                return false;
            
            for (int i = 0; i < puzzle.TargetPattern.Count; i++)
            {
                if (puzzle.CurrentPattern[i] != puzzle.TargetPattern[i])
                    return false;
            }
            
            return true;
        }
        
        private void CompletePuzzle(TraitMatchingPuzzle puzzle)
        {
            puzzle.IsActive = false;
            puzzle.IsCompleted = true;
            puzzle.CompletionTime = DateTime.Now;
            
            // Calculate final score and rewards
            float completionBonus = puzzle.MovesRemaining * 5f; // Bonus for efficiency
            puzzle.FinalScore = puzzle.CurrentScore + completionBonus;
            
            OnPuzzleCompleted?.Invoke(puzzle);
            OnRewardEarned?.Invoke("current_player", puzzle.FinalScore);
            
            Debug.Log($"✅ Puzzle completed: {puzzle.PuzzleName} - Score: {puzzle.FinalScore}");
        }
        
        #endregion
        
        #region Testing and Validation Methods
        
        /// <summary>
        /// Test method to validate breeding challenge system functionality
        /// </summary>
        public void TestBreedingChallengeSystem()
        {
            Debug.Log("=== Testing Breeding Challenge System ===");
            Debug.Log($"Challenges Enabled: {EnableBreedingChallenges}");
            Debug.Log($"Puzzle Mode Enabled: {EnablePuzzleMode}");
            Debug.Log($"Active Challenges: {activeChallenges.Count}");
            Debug.Log($"Available Challenge Types: {availableChallengeTypes.Count}");
            
            // Test challenge creation
            if (EnableBreedingChallenges && availableChallengeTypes.Count > 0)
            {
                string testType = availableChallengeTypes[0];
                bool created = CreateBreedingChallenge(testType, BreedingDifficulty.Beginner);
                Debug.Log($"✓ Test challenge creation: {created}");
                
                // Test challenge completion
                if (created && activeChallenges.Count > 0)
                {
                    var testChallenge = activeChallenges[0];
                    var testTraits = new List<string> { "Test_Trait_1", "Test_Trait_2" };
                    bool completed = CompleteBreedingChallenge(testChallenge.ChallengeID, testTraits, 60f);
                    Debug.Log($"✓ Test challenge completion: {completed}");
                }
            }
            
            // Test puzzle game
            if (EnablePuzzleMode)
            {
                var testPuzzle = StartTraitMatchingPuzzle("Basic");
                Debug.Log($"✓ Test puzzle creation: {testPuzzle != null}");
                
                if (testPuzzle != null)
                {
                    bool moveResult = MakePuzzleMove(testPuzzle.PuzzleID, "Trait_A", 0);
                    Debug.Log($"✓ Test puzzle move: {moveResult}");
                }
            }
            
            Debug.Log("✅ Breeding challenge system test completed");
        }
        
        #endregion
    }
    
    #region Supporting Data Structures
    
    [System.Serializable]
    public class BreedingChallenge
    {
        public string ChallengeID;
        public string ChallengeName;
        public string ChallengeType;
        public BreedingDifficulty Difficulty;
        public DateTime CreatedDate;
        public DateTime CompletedDate;
        public bool IsActive;
        public bool IsCompleted;
        public float TimeLimit;
        public List<string> TargetTraits = new List<string>();
        public List<string> AchievedTraits = new List<string>();
        public List<string> Objectives = new List<string>();
        public float RewardMultiplier;
        public float CompletionScore;
    }
    
    [System.Serializable]
    public class TraitMatchingPuzzle
    {
        public string PuzzleID;
        public string PuzzleName;
        public string PuzzleType;
        public DateTime StartTime;
        public DateTime CompletionTime;
        public bool IsActive;
        public bool IsCompleted;
        public List<string> TargetPattern = new List<string>();
        public List<string> CurrentPattern = new List<string>();
        public List<string> AvailableTraits = new List<string>();
        public int MovesRemaining;
        public float CurrentScore;
        public float FinalScore;
        public float ScoreMultiplier;
    }
    
    [System.Serializable]
    public class PlayerBreedingStats
    {
        public string PlayerID;
        public int TotalChallengesCompleted;
        public float TotalRewardsEarned;
        public float AverageCompletionScore;
        public int BestStreak;
        public string FavoriteChallenge;
        public DateTime LastActivity;
        public Dictionary<string, int> ChallengeTypeStats = new Dictionary<string, int>();
    }
    
    [System.Serializable]
    public class ChallengeCompletionResult
    {
        public float TraitMatchScore;
        public float TimeBonus;
        public float DifficultyBonus;
        public bool PerfectMatch;
        public float Score;
    }
    
    #endregion
}