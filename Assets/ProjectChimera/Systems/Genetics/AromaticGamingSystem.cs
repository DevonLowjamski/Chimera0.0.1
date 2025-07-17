using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// Aromatic Gaming System - Fun sensory mini-games and aroma mastery challenges
    /// Focuses on entertaining scent-based gameplay mechanics rather than educational content
    /// Creates engaging aroma identification games, scent mixing challenges, and aromatic competitions
    /// 
    /// ABSTRACT METHOD VERIFICATION COMPLETE:
    /// ✅ OnManagerInitialize() - implemented
    /// ✅ OnManagerShutdown() - implemented
    /// </summary>
    public class AromaticGamingSystem : ChimeraManager
    {
        [Header("Aromatic Gaming Configuration")]
        public bool EnableAromaticGames = true;
        public bool EnableScentMatching = true;
        public bool EnableTerpeneBlending = true;
        public bool EnableAromaticCompetitions = true;
        
        [Header("Gaming Settings")]
        public int MaxActiveGames = 8;
        public float BaseScoreMultiplier = 1.0f;
        public float PerfectScentBonus = 3.0f;
        public float BlendingAccuracyBonus = 2.5f;
        
        [Header("Aromatic Collections")]
        [SerializeField] private List<ScentMatchingGame> activeGames = new List<ScentMatchingGame>();
        [SerializeField] private List<TerpeneBlendingChallenge> blendingChallenges = new List<TerpeneBlendingChallenge>();
        [SerializeField] private List<AromaticCompetition> competitions = new List<AromaticCompetition>();
        [SerializeField] private Dictionary<string, PlayerAromaticProfile> playerProfiles = new Dictionary<string, PlayerAromaticProfile>();
        
        [Header("Scent Library")]
        [SerializeField] private List<ScentProfile> availableScents = new List<ScentProfile>();
        [SerializeField] private List<TerpeneProfile> availableTerpenes = new List<TerpeneProfile>();
        [SerializeField] private List<AromaticBlend> masterBlends = new List<AromaticBlend>();
        
        [Header("Gaming State")]
        [SerializeField] private DateTime lastGameUpdate = DateTime.Now;
        [SerializeField] private int totalGamesPlayed = 0;
        [SerializeField] private float totalPointsEarned = 0f;
        [SerializeField] private List<string> availableGameModes = new List<string>();
        
        // Events for aromatic gaming feedback and celebrations
        public static event Action<ScentMatchingGame> OnScentGameCompleted;
        public static event Action<TerpeneBlendingChallenge> OnBlendingCompleted;
        public static event Action<AromaticCompetition> OnCompetitionWon;
        public static event Action<string, float> OnAromaticMastery;
        public static event Action<string, int> OnPerfectNose;
        
        protected override void OnManagerInitialize()
        {
            // Register with GameManager using verified pattern
            GameManager.Instance?.RegisterManager(this);
            
            // Initialize aromatic gaming system
            InitializeAromaticSystem();
            
            if (EnableAromaticGames)
            {
                StartAromaticGaming();
            }
            
            Debug.Log("✅ AromaticGamingSystem initialized successfully");
        }
        
        protected override void OnManagerShutdown()
        {
            // Clean up aromatic gaming
            if (EnableAromaticGames)
            {
                StopAromaticGaming();
            }
            
            // Clear all events to prevent memory leaks
            OnScentGameCompleted = null;
            OnBlendingCompleted = null;
            OnCompetitionWon = null;
            OnAromaticMastery = null;
            OnPerfectNose = null;
            
            Debug.Log("✅ AromaticGamingSystem shutdown successfully");
        }
        
        private void InitializeAromaticSystem()
        {
            // Initialize collections if empty
            if (activeGames == null) activeGames = new List<ScentMatchingGame>();
            if (blendingChallenges == null) blendingChallenges = new List<TerpeneBlendingChallenge>();
            if (competitions == null) competitions = new List<AromaticCompetition>();
            if (playerProfiles == null) playerProfiles = new Dictionary<string, PlayerAromaticProfile>();
            if (availableScents == null) availableScents = new List<ScentProfile>();
            if (availableTerpenes == null) availableTerpenes = new List<TerpeneProfile>();
            if (masterBlends == null) masterBlends = new List<AromaticBlend>();
            if (availableGameModes == null) availableGameModes = new List<string>();
            
            // Initialize aromatic gaming content
            InitializeGameModes();
            InitializeScentLibrary();
            InitializeTerpeneLibrary();
            InitializeMasterBlends();
        }
        
        private void InitializeGameModes()
        {
            // Fun, engaging aromatic game modes
            availableGameModes.Clear();
            availableGameModes.Add("Scent Detective"); // Identify mystery scents
            availableGameModes.Add("Aroma Memory"); // Memory-based scent matching
            availableGameModes.Add("Blind Nose Test"); // Scent identification without visual cues
            availableGameModes.Add("Terpene Mixer"); // Create custom terpene blends
            availableGameModes.Add("Scent Speedrun"); // Fast-paced aroma identification
            availableGameModes.Add("Perfect Blend"); // Recreate exact aromatic profiles
            availableGameModes.Add("Aroma Artist"); // Creative scent composition
            availableGameModes.Add("Nose Master"); // Ultimate aromatic challenge
            
            Debug.Log($"✅ Aromatic game modes initialized: {availableGameModes.Count} modes available");
        }
        
        private void InitializeScentLibrary()
        {
            // Create diverse, game-friendly scent profiles
            var scentCategories = new[]
            {
                ("Fruity", new[] { "Citrus_Burst", "Berry_Explosion", "Tropical_Paradise", "Apple_Crisp" }),
                ("Earthy", new[] { "Pine_Forest", "Woodsy_Musk", "Fresh_Soil", "Mountain_Air" }),
                ("Floral", new[] { "Rose_Garden", "Lavender_Dreams", "Jasmine_Night", "Sweet_Blossom" }),
                ("Spicy", new[] { "Pepper_Heat", "Cinnamon_Fire", "Ginger_Snap", "Clove_Warmth" }),
                ("Sweet", new[] { "Vanilla_Cloud", "Honey_Drip", "Caramel_Swirl", "Sugar_Rush" }),
                ("Herbal", new[] { "Mint_Fresh", "Sage_Wisdom", "Thyme_Magic", "Basil_Burst" })
            };
            
            foreach (var (category, scents) in scentCategories)
            {
                foreach (var scent in scents)
                {
                    var profile = new ScentProfile
                    {
                        ScentID = $"scent_{scent.ToLower()}",
                        ScentName = scent.Replace("_", " "),
                        Category = category,
                        Intensity = UnityEngine.Random.Range(0.3f, 1.0f),
                        Complexity = UnityEngine.Random.Range(1, 5),
                        Rarity = UnityEngine.Random.Range(0.1f, 0.9f),
                        IsUnlocked = category == "Fruity" // Start with fruity unlocked
                    };
                    
                    availableScents.Add(profile);
                }
            }
            
            Debug.Log($"✅ Scent library initialized: {availableScents.Count} scents across {scentCategories.Length} categories");
        }
        
        private void InitializeTerpeneLibrary()
        {
            // Create game-friendly terpene profiles for blending
            var terpeneData = new[]
            {
                ("Limonene", "Citrus", 0.8f, "Bright and uplifting"),
                ("Myrcene", "Earthy", 0.7f, "Relaxing and mellow"),
                ("Pinene", "Pine", 0.9f, "Sharp and energizing"),
                ("Linalool", "Floral", 0.6f, "Soft and soothing"),
                ("Caryophyllene", "Spicy", 0.85f, "Warm and complex"),
                ("Terpinolene", "Fresh", 0.75f, "Clean and bright"),
                ("Humulene", "Herbal", 0.65f, "Earthy and bitter"),
                ("Ocimene", "Sweet", 0.7f, "Light and fruity")
            };
            
            foreach (var (name, aroma, potency, description) in terpeneData)
            {
                var terpene = new TerpeneProfile
                {
                    TerpeneID = $"terpene_{name.ToLower()}",
                    TerpeneName = name,
                    AromaticProfile = aroma,
                    Potency = potency,
                    BlendingCompatibility = UnityEngine.Random.Range(0.5f, 1.0f),
                    GameDescription = description,
                    IsUnlocked = name == "Limonene" || name == "Myrcene" // Start with basics unlocked
                };
                
                availableTerpenes.Add(terpene);
            }
            
            Debug.Log($"✅ Terpene library initialized: {availableTerpenes.Count} terpenes available");
        }
        
        private void InitializeMasterBlends()
        {
            // Create signature aromatic blends for players to discover and recreate
            var masterBlendRecipes = new[]
            {
                ("Citrus Symphony", new[] { "Limonene", "Pinene", "Terpinolene" }, 0.9f),
                ("Forest Meditation", new[] { "Myrcene", "Pinene", "Humulene" }, 0.85f),
                ("Flower Power", new[] { "Linalool", "Ocimene", "Terpinolene" }, 0.8f),
                ("Spice Master", new[] { "Caryophyllene", "Humulene", "Pinene" }, 0.95f),
                ("Sweet Dreams", new[] { "Linalool", "Myrcene", "Ocimene" }, 0.75f)
            };
            
            foreach (var (name, ingredients, difficulty) in masterBlendRecipes)
            {
                var blend = new AromaticBlend
                {
                    BlendID = $"blend_{name.ToLower().Replace(" ", "_")}",
                    BlendName = name,
                    Difficulty = difficulty,
                    IsLegendary = difficulty >= 0.9f,
                    UnlockRequirement = $"Master {ingredients.Length} terpenes"
                };
                
                foreach (var ingredient in ingredients)
                {
                    blend.RequiredTerpenes.Add(ingredient, UnityEngine.Random.Range(0.2f, 0.8f));
                }
                
                masterBlends.Add(blend);
            }
            
            Debug.Log($"✅ Master blends initialized: {masterBlends.Count} signature blends");
        }
        
        private void StartAromaticGaming()
        {
            // Start aromatic gaming systems
            lastGameUpdate = DateTime.Now;
            
            Debug.Log("✅ Aromatic gaming started - focusing on fun sensory experiences");
        }
        
        private void StopAromaticGaming()
        {
            // Clean up aromatic gaming
            Debug.Log("✅ Aromatic gaming stopped");
        }
        
        private void Update()
        {
            if (!EnableAromaticGames) return;
            
            // Update active games and challenges
            UpdateActiveGames();
        }
        
        private void UpdateActiveGames()
        {
            // Update active scent matching games
            foreach (var game in activeGames.ToList())
            {
                if (game.IsActive && (DateTime.Now - game.StartTime).TotalSeconds > game.TimeLimit)
                {
                    CompleteGame(game, false); // Time expired
                }
            }
            
            // Update blending challenges
            foreach (var challenge in blendingChallenges.ToList())
            {
                if (challenge.IsActive && (DateTime.Now - challenge.StartTime).TotalSeconds > challenge.TimeLimit)
                {
                    CompleteBlendingChallenge(challenge, false); // Time expired
                }
            }
        }
        
        #region Public API Methods
        
        /// <summary>
        /// Start a new scent matching game
        /// </summary>
        public ScentMatchingGame StartScentMatchingGame(string gameMode = "Scent Detective", int difficulty = 1)
        {
            if (!EnableScentMatching) return null;
            
            if (activeGames.Count >= MaxActiveGames)
            {
                Debug.LogWarning($"Maximum active games limit reached ({MaxActiveGames})");
                return null;
            }
            
            var game = new ScentMatchingGame
            {
                GameID = $"scent_game_{DateTime.Now.Ticks}",
                GameName = $"{gameMode} Challenge",
                GameMode = gameMode,
                Difficulty = difficulty,
                StartTime = DateTime.Now,
                IsActive = true,
                TimeLimit = CalculateGameTimeLimit(gameMode, difficulty),
                TargetScents = SelectGameScents(gameMode, difficulty),
                CurrentScore = 0f,
                PerfectMatches = 0,
                TotalAttempts = 0
            };
            
            // Generate decoy scents for the game
            game.AvailableScents = GenerateGameScents(game.TargetScents, difficulty);
            
            activeGames.Add(game);
            
            Debug.Log($"✅ Scent matching game started: {game.GameName} (Difficulty: {difficulty})");
            return game;
        }
        
        /// <summary>
        /// Make a scent identification attempt in an active game
        /// </summary>
        public bool IdentifyScent(string gameId, string targetScentId, string guessedScentId)
        {
            var game = activeGames.FirstOrDefault(g => g.GameID == gameId && g.IsActive);
            if (game == null) return false;
            
            game.TotalAttempts++;
            bool correct = targetScentId == guessedScentId;
            
            if (correct)
            {
                game.PerfectMatches++;
                game.CurrentScore += CalculateScentScore(game, true);
                
                // Remove identified scent from targets
                game.TargetScents.RemoveAll(s => s.ScentID == targetScentId);
                
                // Check if game is complete
                if (game.TargetScents.Count == 0)
                {
                    CompleteGame(game, true);
                }
            }
            else
            {
                game.CurrentScore += CalculateScentScore(game, false);
            }
            
            Debug.Log($"Scent identification: {(correct ? "Correct" : "Incorrect")} - Score: {game.CurrentScore}");
            return correct;
        }
        
        /// <summary>
        /// Start a terpene blending challenge
        /// </summary>
        public TerpeneBlendingChallenge StartBlendingChallenge(string targetBlendId)
        {
            if (!EnableTerpeneBlending) return null;
            
            var targetBlend = masterBlends.FirstOrDefault(b => b.BlendID == targetBlendId);
            if (targetBlend == null)
            {
                Debug.LogWarning($"Target blend not found: {targetBlendId}");
                return null;
            }
            
            var challenge = new TerpeneBlendingChallenge
            {
                ChallengeID = $"blend_challenge_{DateTime.Now.Ticks}",
                ChallengeName = $"Recreate {targetBlend.BlendName}",
                TargetBlend = targetBlend,
                StartTime = DateTime.Now,
                IsActive = true,
                TimeLimit = 300f, // 5 minutes
                AvailableTerpenes = GetUnlockedTerpenes(),
                AccuracyThreshold = 0.85f
            };
            
            blendingChallenges.Add(challenge);
            
            Debug.Log($"✅ Blending challenge started: {challenge.ChallengeName}");
            return challenge;
        }
        
        /// <summary>
        /// Add a terpene to the current blend
        /// </summary>
        public bool AddTerpeneToBlend(string challengeId, string terpeneId, float concentration)
        {
            var challenge = blendingChallenges.FirstOrDefault(c => c.ChallengeID == challengeId && c.IsActive);
            if (challenge == null) return false;
            
            var terpene = availableTerpenes.FirstOrDefault(t => t.TerpeneID == terpeneId);
            if (terpene == null || !terpene.IsUnlocked) return false;
            
            // Add or update terpene in current blend
            if (challenge.CurrentBlend == null)
                challenge.CurrentBlend = new Dictionary<string, float>();
            
            challenge.CurrentBlend[terpeneId] = Mathf.Clamp01(concentration);
            
            // Calculate current blend accuracy
            challenge.BlendAccuracy = CalculateBlendAccuracy(challenge.TargetBlend, challenge.CurrentBlend);
            
            Debug.Log($"Terpene added: {terpene.TerpeneName} ({concentration:P}) - Accuracy: {challenge.BlendAccuracy:P}");
            
            // Check if blend is complete and accurate enough
            if (challenge.BlendAccuracy >= challenge.AccuracyThreshold)
            {
                CompleteBlendingChallenge(challenge, true);
            }
            
            return true;
        }
        
        /// <summary>
        /// Create an aromatic competition
        /// </summary>
        public AromaticCompetition CreateAromaticCompetition(string competitionType, int maxParticipants = 8)
        {
            if (!EnableAromaticCompetitions) return null;
            
            var competition = new AromaticCompetition
            {
                CompetitionID = $"aroma_comp_{DateTime.Now.Ticks}",
                CompetitionName = $"{competitionType} Mastery Contest",
                CompetitionType = competitionType,
                MaxParticipants = maxParticipants,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(24), // 24-hour competition
                IsActive = true,
                PrizePool = maxParticipants * 100f // Base prize per participant
            };
            
            // Set competition-specific challenges
            competition.Challenges = GenerateCompetitionChallenges(competitionType);
            
            competitions.Add(competition);
            
            Debug.Log($"✅ Aromatic competition created: {competition.CompetitionName}");
            return competition;
        }
        
        /// <summary>
        /// Get player's aromatic profile and statistics
        /// </summary>
        public PlayerAromaticProfile GetPlayerProfile(string playerId = "current_player")
        {
            if (playerProfiles.ContainsKey(playerId))
            {
                return playerProfiles[playerId];
            }
            
            var newProfile = new PlayerAromaticProfile
            {
                PlayerID = playerId,
                NoseLevel = 1,
                TotalGamesPlayed = 0,
                PerfectIdentifications = 0,
                MasterBlendsCreated = 0,
                CompetitionsWon = 0,
                AromaticMasteryScore = 0f,
                LastActivity = DateTime.Now
            };
            
            playerProfiles[playerId] = newProfile;
            return newProfile;
        }
        
        /// <summary>
        /// Get available scents for current player level
        /// </summary>
        public List<ScentProfile> GetAvailableScents(string playerId = "current_player")
        {
            var profile = GetPlayerProfile(playerId);
            return availableScents.Where(s => s.IsUnlocked || s.Rarity <= profile.NoseLevel * 0.2f).ToList();
        }
        
        /// <summary>
        /// Get unlocked terpenes for blending
        /// </summary>
        public List<TerpeneProfile> GetUnlockedTerpenes(string playerId = "current_player")
        {
            var profile = GetPlayerProfile(playerId);
            return availableTerpenes.Where(t => t.IsUnlocked || profile.NoseLevel >= 3).ToList();
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private float CalculateGameTimeLimit(string gameMode, int difficulty)
        {
            float baseTime = gameMode switch
            {
                "Scent Speedrun" => 30f,
                "Aroma Memory" => 60f,
                "Blind Nose Test" => 90f,
                "Scent Detective" => 120f,
                _ => 90f
            };
            
            return baseTime / (1f + difficulty * 0.3f); // Harder = less time
        }
        
        private List<ScentProfile> SelectGameScents(string gameMode, int difficulty)
        {
            int scentCount = Math.Min(difficulty + 2, 6); // 3-6 scents based on difficulty
            var availableForGame = availableScents.Where(s => s.IsUnlocked).ToList();
            
            return availableForGame.OrderBy(s => UnityEngine.Random.value).Take(scentCount).ToList();
        }
        
        private List<ScentProfile> GenerateGameScents(List<ScentProfile> targetScents, int difficulty)
        {
            // Add decoy scents to make the game challenging
            var gameScents = new List<ScentProfile>(targetScents);
            var decoyCount = difficulty + 2; // More decoys = harder
            
            var availableDecoys = availableScents.Where(s => 
                s.IsUnlocked && !targetScents.Contains(s)).ToList();
            
            var selectedDecoys = availableDecoys.OrderBy(s => UnityEngine.Random.value).Take(decoyCount);
            gameScents.AddRange(selectedDecoys);
            
            // Shuffle the list
            return gameScents.OrderBy(s => UnityEngine.Random.value).ToList();
        }
        
        private float CalculateScentScore(ScentMatchingGame game, bool correct)
        {
            float baseScore = correct ? 10f : -2f;
            float difficultyBonus = game.Difficulty * 2f;
            float accuracyBonus = correct ? (float)game.PerfectMatches / Math.Max(1, game.TotalAttempts) * 5f : 0f;
            
            return (baseScore + difficultyBonus + accuracyBonus) * BaseScoreMultiplier;
        }
        
        private void CompleteGame(ScentMatchingGame game, bool successful)
        {
            game.IsActive = false;
            game.IsCompleted = true;
            game.CompletionTime = DateTime.Now;
            
            if (successful)
            {
                game.CurrentScore *= PerfectScentBonus; // Bonus for completion
            }
            
            // Update player profile
            UpdatePlayerProfileFromGame(game);
            
            // Remove from active games
            activeGames.Remove(game);
            totalGamesPlayed++;
            
            OnScentGameCompleted?.Invoke(game);
            
            if (game.PerfectMatches == game.TargetScents.Count + game.PerfectMatches)
            {
                OnPerfectNose?.Invoke("current_player", game.PerfectMatches);
            }
            
            Debug.Log($"✅ Scent game completed: {game.GameName} - Final Score: {game.CurrentScore}");
        }
        
        private float CalculateBlendAccuracy(AromaticBlend targetBlend, Dictionary<string, float> currentBlend)
        {
            if (targetBlend.RequiredTerpenes.Count == 0) return 0f;
            
            float totalAccuracy = 0f;
            int comparisons = 0;
            
            foreach (var (terpeneId, targetConcentration) in targetBlend.RequiredTerpenes)
            {
                if (currentBlend.ContainsKey(terpeneId))
                {
                    float difference = Math.Abs(targetConcentration - currentBlend[terpeneId]);
                    float accuracy = 1f - difference;
                    totalAccuracy += Math.Max(0f, accuracy);
                }
                comparisons++;
            }
            
            return comparisons > 0 ? totalAccuracy / comparisons : 0f;
        }
        
        private void CompleteBlendingChallenge(TerpeneBlendingChallenge challenge, bool successful)
        {
            challenge.IsActive = false;
            challenge.IsCompleted = true;
            challenge.CompletionTime = DateTime.Now;
            challenge.FinalAccuracy = challenge.BlendAccuracy;
            
            if (successful)
            {
                challenge.RewardPoints = challenge.BlendAccuracy * 100f * BlendingAccuracyBonus;
                
                // Unlock new scents/terpenes based on mastery
                if (challenge.BlendAccuracy >= 0.95f)
                {
                    UnlockNewAromaticContent();
                }
            }
            
            // Update player profile
            UpdatePlayerProfileFromBlending(challenge);
            
            // Remove from active challenges
            blendingChallenges.Remove(challenge);
            
            OnBlendingCompleted?.Invoke(challenge);
            
            if (challenge.BlendAccuracy >= 0.95f)
            {
                OnAromaticMastery?.Invoke("current_player", challenge.BlendAccuracy);
            }
            
            Debug.Log($"✅ Blending challenge completed: {challenge.ChallengeName} - Accuracy: {challenge.BlendAccuracy:P}");
        }
        
        private List<string> GenerateCompetitionChallenges(string competitionType)
        {
            return competitionType switch
            {
                "Nose Master" => new List<string> { "Identify 10 rare scents", "Perfect blend accuracy", "Speed identification" },
                "Terpene Master" => new List<string> { "Create 3 master blends", "High accuracy blending", "Innovation bonus" },
                "Aroma Artist" => new List<string> { "Creative blend composition", "Unique scent combinations", "Artistic merit" },
                _ => new List<string> { "General aromatic mastery", "Overall performance", "Consistent accuracy" }
            };
        }
        
        private void UpdatePlayerProfileFromGame(ScentMatchingGame game)
        {
            var profile = GetPlayerProfile("current_player");
            
            profile.TotalGamesPlayed++;
            profile.PerfectIdentifications += game.PerfectMatches;
            profile.TotalScore += game.CurrentScore;
            profile.LastActivity = DateTime.Now;
            
            // Level up nose skill based on performance
            float averageAccuracy = (float)game.PerfectMatches / Math.Max(1, game.TotalAttempts);
            if (averageAccuracy >= 0.8f && profile.TotalGamesPlayed % 5 == 0)
            {
                profile.NoseLevel++;
                UnlockNewAromaticContent();
            }
            
            playerProfiles["current_player"] = profile;
        }
        
        private void UpdatePlayerProfileFromBlending(TerpeneBlendingChallenge challenge)
        {
            var profile = GetPlayerProfile("current_player");
            
            profile.TotalBlendsCreated++;
            if (challenge.BlendAccuracy >= 0.9f)
            {
                profile.MasterBlendsCreated++;
            }
            
            profile.AromaticMasteryScore = (profile.AromaticMasteryScore + challenge.BlendAccuracy) / 2f;
            profile.LastActivity = DateTime.Now;
            
            playerProfiles["current_player"] = profile;
        }
        
        private void UnlockNewAromaticContent()
        {
            // Unlock new scents based on player progression
            foreach (var scent in availableScents.Where(s => !s.IsUnlocked))
            {
                if (UnityEngine.Random.value < 0.3f) // 30% chance to unlock
                {
                    scent.IsUnlocked = true;
                    Debug.Log($"🔓 New scent unlocked: {scent.ScentName}");
                    break; // Unlock one at a time
                }
            }
            
            // Unlock new terpenes
            foreach (var terpene in availableTerpenes.Where(t => !t.IsUnlocked))
            {
                if (UnityEngine.Random.value < 0.2f) // 20% chance to unlock
                {
                    terpene.IsUnlocked = true;
                    Debug.Log($"🔓 New terpene unlocked: {terpene.TerpeneName}");
                    break;
                }
            }
        }
        
        #endregion
        
        #region Testing and Validation Methods
        
        /// <summary>
        /// Test method to validate aromatic gaming system functionality
        /// </summary>
        public void TestAromaticGamingSystem()
        {
            Debug.Log("=== Testing Aromatic Gaming System ===");
            Debug.Log($"Aromatic Games Enabled: {EnableAromaticGames}");
            Debug.Log($"Scent Matching Enabled: {EnableScentMatching}");
            Debug.Log($"Terpene Blending Enabled: {EnableTerpeneBlending}");
            Debug.Log($"Available Scents: {availableScents.Count}");
            Debug.Log($"Available Terpenes: {availableTerpenes.Count}");
            Debug.Log($"Master Blends: {masterBlends.Count}");
            Debug.Log($"Game Modes: {availableGameModes.Count}");
            
            // Test scent matching game
            if (EnableScentMatching && availableGameModes.Count > 0)
            {
                string testMode = availableGameModes[0];
                var testGame = StartScentMatchingGame(testMode, 1);
                Debug.Log($"✓ Test scent game creation: {testGame != null}");
                
                if (testGame != null && testGame.TargetScents.Count > 0)
                {
                    var firstScent = testGame.TargetScents[0];
                    bool identified = IdentifyScent(testGame.GameID, firstScent.ScentID, firstScent.ScentID);
                    Debug.Log($"✓ Test scent identification: {identified}");
                }
            }
            
            // Test terpene blending
            if (EnableTerpeneBlending && masterBlends.Count > 0)
            {
                var testBlend = masterBlends[0];
                var blendChallenge = StartBlendingChallenge(testBlend.BlendID);
                Debug.Log($"✓ Test blending challenge: {blendChallenge != null}");
                
                if (blendChallenge != null && availableTerpenes.Count > 0)
                {
                    var testTerpene = availableTerpenes.First(t => t.IsUnlocked);
                    bool added = AddTerpeneToBlend(blendChallenge.ChallengeID, testTerpene.TerpeneID, 0.5f);
                    Debug.Log($"✓ Test terpene blending: {added}");
                }
            }
            
            // Test player profile
            var profile = GetPlayerProfile("test_player");
            Debug.Log($"✓ Test player profile: Level {profile.NoseLevel}, Games: {profile.TotalGamesPlayed}");
            
            Debug.Log("✅ Aromatic gaming system test completed");
        }
        
        #endregion
    }
    
    #region Supporting Data Structures
    
    [System.Serializable]
    public class ScentMatchingGame
    {
        public string GameID;
        public string GameName;
        public string GameMode;
        public int Difficulty;
        public DateTime StartTime;
        public DateTime CompletionTime;
        public bool IsActive;
        public bool IsCompleted;
        public float TimeLimit;
        public List<ScentProfile> TargetScents = new List<ScentProfile>();
        public List<ScentProfile> AvailableScents = new List<ScentProfile>();
        public float CurrentScore;
        public int PerfectMatches;
        public int TotalAttempts;
    }
    
    [System.Serializable]
    public class TerpeneBlendingChallenge
    {
        public string ChallengeID;
        public string ChallengeName;
        public AromaticBlend TargetBlend;
        public DateTime StartTime;
        public DateTime CompletionTime;
        public bool IsActive;
        public bool IsCompleted;
        public float TimeLimit;
        public List<TerpeneProfile> AvailableTerpenes = new List<TerpeneProfile>();
        public Dictionary<string, float> CurrentBlend = new Dictionary<string, float>();
        public float BlendAccuracy;
        public float FinalAccuracy;
        public float AccuracyThreshold;
        public float RewardPoints;
    }
    
    [System.Serializable]
    public class AromaticCompetition
    {
        public string CompetitionID;
        public string CompetitionName;
        public string CompetitionType;
        public int MaxParticipants;
        public DateTime StartDate;
        public DateTime EndDate;
        public bool IsActive;
        public bool IsCompleted;
        public float PrizePool;
        public List<string> Participants = new List<string>();
        public List<string> Challenges = new List<string>();
        public Dictionary<string, float> Leaderboard = new Dictionary<string, float>();
    }
    
    [System.Serializable]
    public class ScentProfile
    {
        public string ScentID;
        public string ScentName;
        public string Category;
        public float Intensity;
        public int Complexity;
        public float Rarity;
        public bool IsUnlocked;
        public string Description;
    }
    
    [System.Serializable]
    public class TerpeneProfile
    {
        public string TerpeneID;
        public string TerpeneName;
        public string AromaticProfile;
        public float Potency;
        public float BlendingCompatibility;
        public bool IsUnlocked;
        public string GameDescription;
    }
    
    [System.Serializable]
    public class AromaticBlend
    {
        public string BlendID;
        public string BlendName;
        public Dictionary<string, float> RequiredTerpenes = new Dictionary<string, float>();
        public float Difficulty;
        public bool IsLegendary;
        public string UnlockRequirement;
        public string Description;
    }
    
    [System.Serializable]
    public class PlayerAromaticProfile
    {
        public string PlayerID;
        public int NoseLevel;
        public int TotalGamesPlayed;
        public int PerfectIdentifications;
        public int TotalBlendsCreated;
        public int MasterBlendsCreated;
        public int CompetitionsWon;
        public float TotalScore;
        public float AromaticMasteryScore;
        public DateTime LastActivity;
        public List<string> UnlockedScents = new List<string>();
        public List<string> UnlockedTerpenes = new List<string>();
    }
    
    #endregion
}