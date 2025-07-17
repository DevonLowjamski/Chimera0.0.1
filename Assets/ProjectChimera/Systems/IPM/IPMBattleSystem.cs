using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.IPM;

namespace ProjectChimera.Systems.IPM
{
    /// <summary>
    /// IPM Battle System - Strategic pest combat mini-games and engaging pest management
    /// Transforms pest control into fun strategic combat challenges rather than tedious management
    /// Features tactical pest battles, defense strategies, and exciting victory celebrations
    /// 
    /// ABSTRACT METHOD VERIFICATION COMPLETE:
    /// ✅ OnManagerInitialize() - implemented
    /// ✅ OnManagerShutdown() - implemented
    /// </summary>
    public class IPMBattleSystem : ChimeraManager
    {
        [Header("Battle Configuration")]
        public bool EnableIPMBattles = true;
        public bool EnableStrategicMode = true;
        public bool EnableRealTimeMode = true;
        public bool EnableTournamentMode = true;
        
        [Header("Combat Settings")]
        public int MaxActiveBattles = 3;
        public float BaseAttackPower = 10f;
        public float BaseDefensePower = 8f;
        public float CriticalHitChance = 0.15f;
        public float VictoryBonusMultiplier = 1.5f;
        
        [Header("Battle Collections")]
        [SerializeField] private List<PestBattle> activeBattles = new List<PestBattle>();
        [SerializeField] private List<PestBattle> completedBattles = new List<PestBattle>();
        [SerializeField] private List<IPMWeapon> availableWeapons = new List<IPMWeapon>();
        [SerializeField] private List<PestEnemy> pestLibrary = new List<PestEnemy>();
        
        [Header("Player Arsenal")]
        [SerializeField] private Dictionary<string, PlayerIPMProfile> playerProfiles = new Dictionary<string, PlayerIPMProfile>();
        [SerializeField] private List<IPMStrategy> availableStrategies = new List<IPMStrategy>();
        [SerializeField] private List<DefenseUpgrade> defenseUpgrades = new List<DefenseUpgrade>();
        
        [Header("Battle State")]
        [SerializeField] private DateTime lastBattleUpdate = DateTime.Now;
        [SerializeField] private int totalBattlesWon = 0;
        [SerializeField] private int totalPestsDefeated = 0;
        [SerializeField] private float totalDamageDealt = 0f;
        [SerializeField] private List<string> availableBattleModes = new List<string>();
        
        // Events for battle excitement and celebrations
        public static event Action<PestBattle> OnBattleStarted;
        public static event Action<PestBattle> OnBattleWon;
        public static event Action<PestBattle> OnBattleLost;
        public static event Action<string, int> OnPestDefeated;
        public static event Action<string, float> OnCriticalHit;
        public static event Action<IPMWeapon> OnWeaponUnlocked;
        
        protected override void OnManagerInitialize()
        {
            // Register with GameManager using verified pattern
            GameManager.Instance?.RegisterManager(this);
            
            // Initialize IPM battle system
            InitializeBattleSystem();
            
            if (EnableIPMBattles)
            {
                StartBattleSystem();
            }
            
            Debug.Log("✅ IPMBattleSystem initialized successfully");
        }
        
        protected override void OnManagerShutdown()
        {
            // Clean up battle system
            if (EnableIPMBattles)
            {
                StopBattleSystem();
            }
            
            // Clear all events to prevent memory leaks
            OnBattleStarted = null;
            OnBattleWon = null;
            OnBattleLost = null;
            OnPestDefeated = null;
            OnCriticalHit = null;
            OnWeaponUnlocked = null;
            
            Debug.Log("✅ IPMBattleSystem shutdown successfully");
        }
        
        private void InitializeBattleSystem()
        {
            // Initialize collections if empty
            if (activeBattles == null) activeBattles = new List<PestBattle>();
            if (completedBattles == null) completedBattles = new List<PestBattle>();
            if (availableWeapons == null) availableWeapons = new List<IPMWeapon>();
            if (pestLibrary == null) pestLibrary = new List<PestEnemy>();
            if (playerProfiles == null) playerProfiles = new Dictionary<string, PlayerIPMProfile>();
            if (availableStrategies == null) availableStrategies = new List<IPMStrategy>();
            if (defenseUpgrades == null) defenseUpgrades = new List<DefenseUpgrade>();
            if (availableBattleModes == null) availableBattleModes = new List<string>();
            
            // Initialize battle content
            InitializeBattleModes();
            InitializePestLibrary();
            InitializeWeaponArsenal();
            InitializeStrategies();
            InitializeDefenseUpgrades();
        }
        
        private void InitializeBattleModes()
        {
            // Fun, engaging battle modes for pest combat
            availableBattleModes.Clear();
            availableBattleModes.Add("Quick Strike"); // Fast-paced pest elimination
            availableBattleModes.Add("Defense Tower"); // Strategic defense against waves
            availableBattleModes.Add("Pest Hunt"); // Search and destroy missions
            availableBattleModes.Add("Survival Mode"); // Endless waves of pests
            availableBattleModes.Add("Boss Battle"); // Epic confrontations with super pests
            availableBattleModes.Add("Stealth Ops"); // Quiet elimination missions
            availableBattleModes.Add("Blitz Attack"); // Overwhelming force deployment
            availableBattleModes.Add("Championship"); // Tournament-style competition
            
            Debug.Log($"✅ Battle modes initialized: {availableBattleModes.Count} modes available");
        }
        
        private void InitializePestLibrary()
        {
            // Create diverse pest enemies with varying difficulty and abilities
            var pestData = new[]
            {
                ("Aphid Swarm", ProjectChimera.Data.IPM.PestType.Aphids, 1, 20f, 5f, "Fast-breeding soft-bodied insects"),
                ("Spider Mite Colony", ProjectChimera.Data.IPM.PestType.SpiderMites, 2, 35f, 8f, "Tiny but destructive web spinners"),
                ("Whitefly Cloud", ProjectChimera.Data.IPM.PestType.Whiteflies, 2, 30f, 12f, "Flying pests that drain plant energy"),
                ("Thrips Brigade", ProjectChimera.Data.IPM.PestType.Thrips, 3, 45f, 15f, "Microscopic raiders with piercing mouths"),
                ("Fungus Gnat Army", ProjectChimera.Data.IPM.PestType.Fungusgnats, 2, 25f, 10f, "Soil-dwelling larvae attackers"),
                ("Scale Insect Fortress", ProjectChimera.Data.IPM.PestType.Scale, 4, 80f, 25f, "Armored pests with strong defenses"),
                ("Mealybug Cluster", ProjectChimera.Data.IPM.PestType.Mealybugs, 3, 60f, 18f, "Cotton-like pests that secrete honeydew"),
                ("Caterpillar Tank", ProjectChimera.Data.IPM.PestType.Caterpillars, 5, 120f, 35f, "Heavy-hitting leaf destroyers"),
                ("Root Aphid Miners", ProjectChimera.Data.IPM.PestType.Aphids, 4, 70f, 22f, "Underground root system attackers"),
                ("Super Pest Boss", ProjectChimera.Data.IPM.PestType.Nematodes, 10, 500f, 100f, "Legendary evolved super pest")
            };
            
            foreach (var (name, type, difficulty, health, damage, description) in pestData)
            {
                var pest = new PestEnemy
                {
                    PestID = $"pest_{name.ToLower().Replace(" ", "_")}",
                    PestName = name,
                    PestType = type,
                    DifficultyLevel = difficulty,
                    MaxHealth = health,
                    CurrentHealth = health,
                    AttackPower = damage,
                    Description = description,
                    IsUnlocked = difficulty <= 3, // Easier pests start unlocked
                    SpecialAbilities = GeneratePestAbilities(name, type, difficulty),
                    WeaknessTypes = GeneratePestWeaknesses(type),
                    ResistanceTypes = GeneratePestResistances(type)
                };
                
                pestLibrary.Add(pest);
            }
            
            Debug.Log($"✅ Pest library initialized: {pestLibrary.Count} pest enemies");
        }
        
        private void InitializeWeaponArsenal()
        {
            // Create diverse IPM weapons with different strengths and tactical uses
            var weaponData = new[]
            {
                ("Neem Oil Spray", IPMWeaponType.Organic, 15f, 1, "Natural organic pest deterrent"),
                ("Insecticidal Soap", IPMWeaponType.Organic, 12f, 1, "Gentle but effective soap solution"),
                ("Ladybug Army", IPMWeaponType.Biological, 25f, 2, "Beneficial predator insects"),
                ("Sticky Trap Matrix", IPMWeaponType.Physical, 10f, 1, "Strategic trap placement"),
                ("Pyrethrin Blaster", IPMWeaponType.Chemical, 30f, 3, "Powerful botanical insecticide"),
                ("Predator Mite Squad", IPMWeaponType.Biological, 35f, 3, "Elite predatory mite forces"),
                ("Diatomaceous Earth Cannon", IPMWeaponType.Physical, 20f, 2, "Abrasive silica particle weapon"),
                ("Bacillus Thuringiensis Bomb", IPMWeaponType.Biological, 40f, 4, "Bacterial warfare specialist"),
                ("Essential Oil Blaster", IPMWeaponType.Organic, 18f, 2, "Aromatic pest confusion weapon"),
                ("Ultimate IPM Destroyer", IPMWeaponType.Ultimate, 100f, 10, "Legendary multi-modal pest eliminator")
            };
            
            foreach (var (name, type, damage, level, description) in weaponData)
            {
                var weapon = new IPMWeapon
                {
                    WeaponID = $"weapon_{name.ToLower().Replace(" ", "_")}",
                    WeaponName = name,
                    WeaponType = type,
                    BaseDamage = damage,
                    RequiredLevel = level,
                    Description = description,
                    IsUnlocked = level <= 2, // Basic weapons start unlocked
                    CriticalChance = CalculateWeaponCriticalChance(type),
                    EffectiveAgainst = GenerateWeaponEffectiveness(type),
                    SpecialEffects = GenerateWeaponEffects(name, type)
                };
                
                availableWeapons.Add(weapon);
            }
            
            Debug.Log($"✅ Weapon arsenal initialized: {availableWeapons.Count} IPM weapons");
        }
        
        private void InitializeStrategies()
        {
            // Create strategic approaches for different battle scenarios
            var strategyData = new[]
            {
                ("Aggressive Assault", "All-out attack with maximum damage", 1.3f, 0.8f),
                ("Defensive Fortress", "Strong defenses with counterattacks", 0.9f, 1.4f),
                ("Precision Strike", "Targeted attacks on pest weaknesses", 1.1f, 1.0f),
                ("Biological Warfare", "Use beneficial insects against pests", 1.0f, 1.2f),
                ("Integrated Approach", "Balanced multi-modal strategy", 1.1f, 1.1f),
                ("Stealth Operations", "Quiet elimination without detection", 0.8f, 1.3f),
                ("Overwhelming Force", "Maximum firepower deployment", 1.5f, 0.7f),
                ("Tactical Retreat", "Strategic withdrawal and regrouping", 0.6f, 1.5f)
            };
            
            foreach (var (name, description, attackMod, defenseMod) in strategyData)
            {
                var strategy = new IPMStrategy
                {
                    StrategyID = $"strategy_{name.ToLower().Replace(" ", "_")}",
                    StrategyName = name,
                    Description = description,
                    AttackModifier = attackMod,
                    DefenseModifier = defenseMod,
                    IsUnlocked = true,
                    EffectDuration = TimeSpan.FromMinutes(5),
                    CooldownTime = TimeSpan.FromMinutes(10)
                };
                
                availableStrategies.Add(strategy);
            }
            
            Debug.Log($"✅ Strategies initialized: {availableStrategies.Count} tactical approaches");
        }
        
        private void InitializeDefenseUpgrades()
        {
            // Create defense upgrades for facility protection
            var upgradeData = new[]
            {
                ("Reinforced Screens", "Stronger physical barriers", 20f, 1),
                ("Air Filtration Plus", "Enhanced air filtering system", 15f, 2),
                ("UV Sterilization", "Ultraviolet pest deterrent", 25f, 3),
                ("Automated Sentries", "AI-powered pest detection", 35f, 4),
                ("Bio-Shield Matrix", "Beneficial microorganism barrier", 30f, 3),
                ("Electromagnetic Field", "High-tech pest disruption", 45f, 5),
                ("Pheromone Scramblers", "Chemical communication disruption", 28f, 4),
                ("Fortress Mode", "Ultimate facility defense system", 100f, 10)
            };
            
            foreach (var (name, description, effectiveness, level) in upgradeData)
            {
                var upgrade = new DefenseUpgrade
                {
                    UpgradeID = $"defense_{name.ToLower().Replace(" ", "_")}",
                    UpgradeName = name,
                    Description = description,
                    DefenseBonus = effectiveness,
                    RequiredLevel = level,
                    IsUnlocked = level <= 2,
                    MaintenanceCost = effectiveness * 2f,
                    UpgradeCategory = CategorizeDefenseUpgrade(name)
                };
                
                defenseUpgrades.Add(upgrade);
            }
            
            Debug.Log($"✅ Defense upgrades initialized: {defenseUpgrades.Count} facility protections");
        }
        
        private void StartBattleSystem()
        {
            // Start IPM battle system
            lastBattleUpdate = DateTime.Now;
            
            Debug.Log("✅ IPM battle system started - strategic pest combat active");
        }
        
        private void StopBattleSystem()
        {
            // Clean up battle system
            Debug.Log("✅ IPM battle system stopped");
        }
        
        private void Update()
        {
            if (!EnableIPMBattles) return;
            
            // Update active battles
            UpdateActiveBattles();
        }
        
        private void UpdateActiveBattles()
        {
            // Process ongoing battles
            foreach (var battle in activeBattles.ToList())
            {
                if (battle.IsActive)
                {
                    UpdateBattleState(battle);
                    
                    // Check for battle completion
                    if (IsBattleComplete(battle))
                    {
                        CompleteBattle(battle);
                    }
                }
            }
        }
        
        #region Public API Methods
        
        /// <summary>
        /// Start a new pest battle with specified mode and difficulty
        /// </summary>
        public PestBattle StartPestBattle(string battleMode, int difficulty = 1, string playerId = "current_player")
        {
            if (!EnableIPMBattles) return null;
            
            if (activeBattles.Count >= MaxActiveBattles)
            {
                Debug.LogWarning($"Maximum active battles limit reached ({MaxActiveBattles})");
                return null;
            }
            
            var playerProfile = GetOrCreatePlayerProfile(playerId);
            var selectedPests = SelectBattlePests(battleMode, difficulty);
            
            var battle = new PestBattle
            {
                BattleID = $"battle_{DateTime.Now.Ticks}",
                BattleName = $"{battleMode} Battle",
                BattleMode = battleMode,
                Difficulty = difficulty,
                StartTime = DateTime.Now,
                IsActive = true,
                IsCompleted = false,
                PlayerID = playerId,
                EnemyPests = selectedPests,
                PlayerWeapons = GetPlayerWeapons(playerId),
                CurrentTurn = 1,
                PlayerHealth = 100f,
                TotalScore = 0f,
                VictoryCondition = GenerateVictoryCondition(battleMode)
            };
            
            activeBattles.Add(battle);
            OnBattleStarted?.Invoke(battle);
            
            Debug.Log($"✅ Pest battle started: {battle.BattleName} (Difficulty: {difficulty})");
            return battle;
        }
        
        /// <summary>
        /// Execute an attack in an active battle
        /// </summary>
        public BattleActionResult ExecuteAttack(string battleId, string weaponId, string targetPestId)
        {
            var battle = activeBattles.FirstOrDefault(b => b.BattleID == battleId && b.IsActive);
            if (battle == null) return null;
            
            var weapon = battle.PlayerWeapons.FirstOrDefault(w => w.WeaponID == weaponId);
            var targetPest = battle.EnemyPests.FirstOrDefault(p => p.PestID == targetPestId && p.CurrentHealth > 0);
            
            if (weapon == null || targetPest == null) return null;
            
            // Calculate attack damage
            var result = CalculateAttackDamage(weapon, targetPest, battle);
            
            // Apply damage to pest
            targetPest.CurrentHealth = Mathf.Max(0f, targetPest.CurrentHealth - result.DamageDealt);
            
            // Update battle state
            battle.TotalScore += result.ScoreGained;
            battle.CurrentTurn++;
            
            // Check for pest defeat
            if (targetPest.CurrentHealth <= 0)
            {
                targetPest.IsDefeated = true;
                result.PestDefeated = true;
                totalPestsDefeated++;
                OnPestDefeated?.Invoke(targetPest.PestName, battle.CurrentTurn);
            }
            
            // Check for critical hit celebration
            if (result.IsCriticalHit)
            {
                OnCriticalHit?.Invoke(weapon.WeaponName, result.DamageDealt);
            }
            
            // Process pest counterattack if still alive
            if (targetPest.CurrentHealth > 0)
            {
                result.CounterAttackDamage = ExecutePestCounterAttack(targetPest, battle);
                battle.PlayerHealth = Mathf.Max(0f, battle.PlayerHealth - result.CounterAttackDamage);
            }
            
            totalDamageDealt += result.DamageDealt;
            
            Debug.Log($"Attack executed: {weapon.WeaponName} → {targetPest.PestName} " +
                     $"({result.DamageDealt:F0} damage{(result.IsCriticalHit ? " CRITICAL!" : "")})");
            
            return result;
        }
        
        /// <summary>
        /// Use a strategic approach in battle
        /// </summary>
        public bool UseStrategy(string battleId, string strategyId)
        {
            var battle = activeBattles.FirstOrDefault(b => b.BattleID == battleId && b.IsActive);
            var strategy = availableStrategies.FirstOrDefault(s => s.StrategyID == strategyId && s.IsUnlocked);
            
            if (battle == null || strategy == null) return false;
            
            // Apply strategy effects
            battle.ActiveStrategy = strategy;
            battle.StrategyStartTime = DateTime.Now;
            
            Debug.Log($"Strategy activated: {strategy.StrategyName} in {battle.BattleName}");
            return true;
        }
        
        /// <summary>
        /// Upgrade player's IPM arsenal
        /// </summary>
        public bool UpgradeWeapon(string weaponId, string playerId = "current_player")
        {
            var weapon = availableWeapons.FirstOrDefault(w => w.WeaponID == weaponId);
            var playerProfile = GetOrCreatePlayerProfile(playerId);
            
            if (weapon == null) return false;
            
            if (playerProfile.IPMLevel >= weapon.RequiredLevel)
            {
                weapon.IsUnlocked = true;
                if (!playerProfile.UnlockedWeapons.Contains(weaponId))
                {
                    playerProfile.UnlockedWeapons.Add(weaponId);
                    OnWeaponUnlocked?.Invoke(weapon);
                }
                
                Debug.Log($"🔓 Weapon unlocked: {weapon.WeaponName}");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Get player's IPM battle profile
        /// </summary>
        public PlayerIPMProfile GetPlayerProfile(string playerId = "current_player")
        {
            return GetOrCreatePlayerProfile(playerId);
        }
        
        /// <summary>
        /// Get active battles for player
        /// </summary>
        public List<PestBattle> GetActiveBattles(string playerId = "current_player")
        {
            return activeBattles.Where(b => b.PlayerID == playerId && b.IsActive).ToList();
        }
        
        /// <summary>
        /// Get available weapons for player level
        /// </summary>
        public List<IPMWeapon> GetAvailableWeapons(string playerId = "current_player")
        {
            var playerProfile = GetOrCreatePlayerProfile(playerId);
            return availableWeapons.Where(w => 
                w.IsUnlocked || w.RequiredLevel <= playerProfile.IPMLevel).ToList();
        }
        
        /// <summary>
        /// Get battle statistics and achievements
        /// </summary>
        public IPMBattleStats GetBattleStats()
        {
            var stats = new IPMBattleStats
            {
                TotalBattlesWon = totalBattlesWon,
                TotalPestsDefeated = totalPestsDefeated,
                TotalDamageDealt = totalDamageDealt,
                ActiveBattlesCount = activeBattles.Count(b => b.IsActive),
                AvailableWeaponsCount = availableWeapons.Count(w => w.IsUnlocked),
                AvailableStrategiesCount = availableStrategies.Count(s => s.IsUnlocked),
                LastBattleUpdate = lastBattleUpdate
            };
            
            return stats;
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private PlayerIPMProfile GetOrCreatePlayerProfile(string playerId)
        {
            if (playerProfiles.ContainsKey(playerId))
            {
                return playerProfiles[playerId];
            }
            
            var newProfile = new PlayerIPMProfile
            {
                PlayerID = playerId,
                IPMLevel = 1,
                TotalBattlesWon = 0,
                TotalPestsDefeated = 0,
                FavoriteWeapon = "",
                LastBattleDate = DateTime.MinValue,
                UnlockedWeapons = new List<string>(),
                UnlockedStrategies = new List<string>(),
                BattleHistory = new List<string>()
            };
            
            // Start with basic weapons unlocked
            var basicWeapons = availableWeapons.Where(w => w.RequiredLevel <= 1).Select(w => w.WeaponID);
            newProfile.UnlockedWeapons.AddRange(basicWeapons);
            
            playerProfiles[playerId] = newProfile;
            return newProfile;
        }
        
        private List<PestEnemy> SelectBattlePests(string battleMode, int difficulty)
        {
            var suitablePests = pestLibrary.Where(p => 
                p.IsUnlocked && p.DifficultyLevel <= difficulty + 2).ToList();
            
            int pestCount = battleMode switch
            {
                "Quick Strike" => Math.Min(2, suitablePests.Count),
                "Defense Tower" => Math.Min(5, suitablePests.Count),
                "Boss Battle" => 1,
                "Survival Mode" => Math.Min(8, suitablePests.Count),
                _ => Math.Min(3, suitablePests.Count)
            };
            
            var selectedPests = suitablePests.OrderBy(p => UnityEngine.Random.value)
                .Take(pestCount).ToList();
            
            // Create battle instances of pests
            var battlePests = new List<PestEnemy>();
            foreach (var pest in selectedPests)
            {
                var battlePest = new PestEnemy
                {
                    PestID = $"{pest.PestID}_battle_{DateTime.Now.Ticks}",
                    PestName = pest.PestName,
                    PestType = pest.PestType,
                    DifficultyLevel = pest.DifficultyLevel,
                    MaxHealth = pest.MaxHealth,
                    CurrentHealth = pest.MaxHealth,
                    AttackPower = pest.AttackPower,
                    Description = pest.Description,
                    IsUnlocked = pest.IsUnlocked,
                    IsDefeated = false,
                    SpecialAbilities = new List<string>(pest.SpecialAbilities),
                    WeaknessTypes = new List<IPMWeaponType>(pest.WeaknessTypes),
                    ResistanceTypes = new List<IPMWeaponType>(pest.ResistanceTypes)
                };
                
                battlePests.Add(battlePest);
            }
            
            return battlePests;
        }
        
        private List<IPMWeapon> GetPlayerWeapons(string playerId)
        {
            var playerProfile = GetOrCreatePlayerProfile(playerId);
            return availableWeapons.Where(w => 
                playerProfile.UnlockedWeapons.Contains(w.WeaponID)).ToList();
        }
        
        private BattleActionResult CalculateAttackDamage(IPMWeapon weapon, PestEnemy targetPest, PestBattle battle)
        {
            var result = new BattleActionResult
            {
                WeaponUsed = weapon.WeaponName,
                TargetPest = targetPest.PestName,
                IsCriticalHit = UnityEngine.Random.value < weapon.CriticalChance
            };
            
            // Base damage calculation
            float baseDamage = weapon.BaseDamage;
            
            // Apply strategy modifiers
            if (battle.ActiveStrategy != null)
            {
                baseDamage *= battle.ActiveStrategy.AttackModifier;
            }
            
            // Check for weapon effectiveness
            if (weapon.EffectiveAgainst.Contains(targetPest.PestType.ToString()))
            {
                baseDamage *= 1.5f; // 50% bonus against effective targets
            }
            
            // Check for pest resistances
            if (targetPest.ResistanceTypes.Contains(weapon.WeaponType))
            {
                baseDamage *= 0.7f; // 30% reduction for resistant pests
            }
            
            // Apply critical hit
            if (result.IsCriticalHit)
            {
                baseDamage *= 2.0f;
            }
            
            result.DamageDealt = baseDamage;
            result.ScoreGained = baseDamage * 10f; // Score based on damage
            
            return result;
        }
        
        private float ExecutePestCounterAttack(PestEnemy pest, PestBattle battle)
        {
            float counterDamage = pest.AttackPower;
            
            // Apply defensive strategy modifiers
            if (battle.ActiveStrategy != null)
            {
                counterDamage /= battle.ActiveStrategy.DefenseModifier;
            }
            
            return counterDamage;
        }
        
        private void UpdateBattleState(PestBattle battle)
        {
            // Update battle timing and conditions
            var battleDuration = DateTime.Now - battle.StartTime;
            
            // Check for time-based events
            if (battle.BattleMode == "Survival Mode" && battleDuration.TotalMinutes > 1)
            {
                // Spawn additional pests in survival mode
                SpawnAdditionalPests(battle);
            }
            
            // Update strategy duration
            if (battle.ActiveStrategy != null)
            {
                var strategyDuration = DateTime.Now - battle.StrategyStartTime;
                if (strategyDuration > battle.ActiveStrategy.EffectDuration)
                {
                    battle.ActiveStrategy = null; // Strategy expires
                }
            }
        }
        
        private bool IsBattleComplete(PestBattle battle)
        {
            // Check victory conditions
            bool allPestsDefeated = battle.EnemyPests.All(p => p.IsDefeated);
            bool playerDefeated = battle.PlayerHealth <= 0;
            
            // Check specific victory conditions
            bool victoryConditionMet = battle.VictoryCondition switch
            {
                "Eliminate All Pests" => allPestsDefeated,
                "Survive Time Limit" => (DateTime.Now - battle.StartTime).TotalMinutes >= 5,
                "Defeat Boss" => battle.EnemyPests.Any(p => p.DifficultyLevel >= 10 && p.IsDefeated),
                _ => allPestsDefeated
            };
            
            return victoryConditionMet || playerDefeated;
        }
        
        private void CompleteBattle(PestBattle battle)
        {
            battle.IsActive = false;
            battle.IsCompleted = true;
            battle.EndTime = DateTime.Now;
            
            // Determine battle outcome
            bool victory = battle.PlayerHealth > 0 && CheckVictoryConditions(battle);
            battle.IsVictory = victory;
            
            if (victory)
            {
                battle.FinalScore = battle.TotalScore * VictoryBonusMultiplier;
                totalBattlesWon++;
                UpdatePlayerProfileFromVictory(battle);
                OnBattleWon?.Invoke(battle);
            }
            else
            {
                battle.FinalScore = battle.TotalScore * 0.5f; // Reduced score for defeat
                OnBattleLost?.Invoke(battle);
            }
            
            // Move to completed battles
            activeBattles.Remove(battle);
            completedBattles.Add(battle);
            
            Debug.Log($"🏆 Battle completed: {battle.BattleName} - {(victory ? "VICTORY" : "DEFEAT")} " +
                     $"(Score: {battle.FinalScore:F0})");
        }
        
        private bool CheckVictoryConditions(PestBattle battle)
        {
            return battle.VictoryCondition switch
            {
                "Eliminate All Pests" => battle.EnemyPests.All(p => p.IsDefeated),
                "Survive Time Limit" => true, // Already survived if battle is complete
                "Defeat Boss" => battle.EnemyPests.Any(p => p.DifficultyLevel >= 10 && p.IsDefeated),
                _ => battle.EnemyPests.All(p => p.IsDefeated)
            };
        }
        
        private void UpdatePlayerProfileFromVictory(PestBattle battle)
        {
            var playerProfile = GetOrCreatePlayerProfile(battle.PlayerID);
            
            playerProfile.TotalBattlesWon++;
            playerProfile.TotalPestsDefeated += battle.EnemyPests.Count(p => p.IsDefeated);
            playerProfile.LastBattleDate = DateTime.Now;
            
            // Level up based on victories
            if (playerProfile.TotalBattlesWon % 5 == 0) // Level up every 5 victories
            {
                playerProfile.IPMLevel++;
                UnlockNewContent(playerProfile);
            }
            
            // Track battle history
            playerProfile.BattleHistory.Insert(0, $"{battle.BattleName}:Victory");
            if (playerProfile.BattleHistory.Count > 20) // Keep last 20 battles
            {
                playerProfile.BattleHistory.RemoveAt(playerProfile.BattleHistory.Count - 1);
            }
        }
        
        private void UnlockNewContent(PlayerIPMProfile profile)
        {
            // Unlock new weapons based on level
            var newWeapons = availableWeapons.Where(w => 
                !w.IsUnlocked && w.RequiredLevel <= profile.IPMLevel).ToList();
            
            foreach (var weapon in newWeapons)
            {
                weapon.IsUnlocked = true;
                profile.UnlockedWeapons.Add(weapon.WeaponID);
                OnWeaponUnlocked?.Invoke(weapon);
            }
            
            // Unlock new pests
            var newPests = pestLibrary.Where(p => 
                !p.IsUnlocked && p.DifficultyLevel <= profile.IPMLevel).ToList();
            
            foreach (var pest in newPests)
            {
                pest.IsUnlocked = true;
            }
        }
        
        private void SpawnAdditionalPests(PestBattle battle)
        {
            if (battle.EnemyPests.Count < 10) // Limit total pests
            {
                var newPest = pestLibrary.Where(p => p.IsUnlocked)
                    .OrderBy(p => UnityEngine.Random.value).First();
                
                var battlePest = new PestEnemy
                {
                    PestID = $"{newPest.PestID}_spawn_{DateTime.Now.Ticks}",
                    PestName = newPest.PestName,
                    PestType = newPest.PestType,
                    DifficultyLevel = newPest.DifficultyLevel,
                    MaxHealth = newPest.MaxHealth,
                    CurrentHealth = newPest.MaxHealth,
                    AttackPower = newPest.AttackPower,
                    Description = newPest.Description,
                    IsUnlocked = newPest.IsUnlocked,
                    IsDefeated = false,
                    SpecialAbilities = new List<string>(newPest.SpecialAbilities),
                    WeaknessTypes = new List<IPMWeaponType>(newPest.WeaknessTypes),
                    ResistanceTypes = new List<IPMWeaponType>(newPest.ResistanceTypes)
                };
                
                battle.EnemyPests.Add(battlePest);
            }
        }
        
        private string GenerateVictoryCondition(string battleMode)
        {
            return battleMode switch
            {
                "Quick Strike" => "Eliminate All Pests",
                "Defense Tower" => "Survive Time Limit",
                "Boss Battle" => "Defeat Boss",
                "Survival Mode" => "Survive Time Limit",
                _ => "Eliminate All Pests"
            };
        }
        
        private List<string> GeneratePestAbilities(string name, ProjectChimera.Data.IPM.PestType type, int difficulty)
        {
            var abilities = new List<string>();
            
            if (difficulty >= 3) abilities.Add("Rapid Reproduction");
            if (difficulty >= 5) abilities.Add("Armor Plating");
            if (difficulty >= 7) abilities.Add("Toxic Secretion");
            if (difficulty >= 10) abilities.Add("Evolutionary Adaptation");
            
            return abilities;
        }
        
        private List<IPMWeaponType> GeneratePestWeaknesses(ProjectChimera.Data.IPM.PestType type)
        {
            return type switch
            {
                ProjectChimera.Data.IPM.PestType.Aphids => new List<IPMWeaponType> { IPMWeaponType.Biological, IPMWeaponType.Organic },
                ProjectChimera.Data.IPM.PestType.SpiderMites => new List<IPMWeaponType> { IPMWeaponType.Physical, IPMWeaponType.Chemical },
                ProjectChimera.Data.IPM.PestType.Thrips => new List<IPMWeaponType> { IPMWeaponType.Biological },
                ProjectChimera.Data.IPM.PestType.Whiteflies => new List<IPMWeaponType> { IPMWeaponType.Organic },
                ProjectChimera.Data.IPM.PestType.Fungusgnats => new List<IPMWeaponType> { IPMWeaponType.Physical },
                ProjectChimera.Data.IPM.PestType.Caterpillars => new List<IPMWeaponType> { IPMWeaponType.Biological },
                ProjectChimera.Data.IPM.PestType.Leafminers => new List<IPMWeaponType> { IPMWeaponType.Chemical },
                ProjectChimera.Data.IPM.PestType.Mealybugs => new List<IPMWeaponType> { IPMWeaponType.Organic },
                ProjectChimera.Data.IPM.PestType.Scale => new List<IPMWeaponType> { IPMWeaponType.Chemical },
                ProjectChimera.Data.IPM.PestType.Nematodes => new List<IPMWeaponType> { IPMWeaponType.Biological },
                _ => new List<IPMWeaponType> { IPMWeaponType.Organic }
            };
        }
        
        private List<IPMWeaponType> GeneratePestResistances(ProjectChimera.Data.IPM.PestType type)
        {
            return type switch
            {
                ProjectChimera.Data.IPM.PestType.Aphids => new List<IPMWeaponType> { IPMWeaponType.Physical },
                ProjectChimera.Data.IPM.PestType.SpiderMites => new List<IPMWeaponType> { IPMWeaponType.Organic },
                ProjectChimera.Data.IPM.PestType.Thrips => new List<IPMWeaponType> { IPMWeaponType.Chemical },
                ProjectChimera.Data.IPM.PestType.Whiteflies => new List<IPMWeaponType> { IPMWeaponType.Physical },
                ProjectChimera.Data.IPM.PestType.Fungusgnats => new List<IPMWeaponType> { IPMWeaponType.Organic },
                ProjectChimera.Data.IPM.PestType.Caterpillars => new List<IPMWeaponType> { IPMWeaponType.Chemical },
                ProjectChimera.Data.IPM.PestType.Leafminers => new List<IPMWeaponType> { IPMWeaponType.Physical },
                ProjectChimera.Data.IPM.PestType.Mealybugs => new List<IPMWeaponType> { IPMWeaponType.Biological },
                ProjectChimera.Data.IPM.PestType.Scale => new List<IPMWeaponType> { IPMWeaponType.Organic },
                ProjectChimera.Data.IPM.PestType.Nematodes => new List<IPMWeaponType> { IPMWeaponType.Chemical },
                _ => new List<IPMWeaponType>()
            };
        }
        
        private float CalculateWeaponCriticalChance(IPMWeaponType type)
        {
            return type switch
            {
                IPMWeaponType.Biological => 0.2f,
                IPMWeaponType.Chemical => 0.15f,
                IPMWeaponType.Physical => 0.1f,
                IPMWeaponType.Organic => 0.25f,
                IPMWeaponType.Ultimate => 0.5f,
                _ => 0.1f
            };
        }
        
        private List<string> GenerateWeaponEffectiveness(IPMWeaponType type)
        {
            return type switch
            {
                IPMWeaponType.Biological => new List<string> { "Aphids", "Thrips" },
                IPMWeaponType.Chemical => new List<string> { "SpiderMites", "Caterpillars" },
                IPMWeaponType.Physical => new List<string> { "Mealybugs" },
                IPMWeaponType.Organic => new List<string> { "Whiteflies" },
                IPMWeaponType.Ultimate => new List<string> { "Aphids", "SpiderMites", "Thrips", "Whiteflies" },
                _ => new List<string>()
            };
        }
        
        private List<string> GenerateWeaponEffects(string name, IPMWeaponType type)
        {
            var effects = new List<string>();
            
            if (name.Contains("Spray")) effects.Add("Area Effect");
            if (name.Contains("Trap")) effects.Add("Persistent");
            if (name.Contains("Army") || name.Contains("Squad")) effects.Add("Multi-Hit");
            if (type == IPMWeaponType.Ultimate) effects.Add("All Damage Types");
            
            return effects;
        }
        
        private string CategorizeDefenseUpgrade(string name)
        {
            if (name.Contains("Screen") || name.Contains("Barrier")) return "Physical";
            if (name.Contains("Filter") || name.Contains("Air")) return "Environmental";
            if (name.Contains("UV") || name.Contains("Electromagnetic")) return "Technology";
            if (name.Contains("Bio") || name.Contains("Pheromone")) return "Biological";
            return "General";
        }
        
        #endregion
        
        #region Testing and Validation Methods
        
        /// <summary>
        /// Test method to validate IPM battle system functionality
        /// </summary>
        public void TestIPMBattleSystem()
        {
            Debug.Log("=== Testing IPM Battle System ===");
            Debug.Log($"Battles Enabled: {EnableIPMBattles}");
            Debug.Log($"Strategic Mode: {EnableStrategicMode}");
            Debug.Log($"Available Battle Modes: {availableBattleModes.Count}");
            Debug.Log($"Pest Library: {pestLibrary.Count}");
            Debug.Log($"Weapon Arsenal: {availableWeapons.Count}");
            Debug.Log($"Available Strategies: {availableStrategies.Count}");
            
            // Test battle creation
            if (EnableIPMBattles && availableBattleModes.Count > 0)
            {
                string testMode = availableBattleModes[0];
                var testBattle = StartPestBattle(testMode, 1, "test_player");
                Debug.Log($"✓ Test battle creation: {testBattle != null}");
                
                // Test attack execution
                if (testBattle != null && testBattle.PlayerWeapons.Count > 0 && testBattle.EnemyPests.Count > 0)
                {
                    var weapon = testBattle.PlayerWeapons[0];
                    var pest = testBattle.EnemyPests[0];
                    var result = ExecuteAttack(testBattle.BattleID, weapon.WeaponID, pest.PestID);
                    Debug.Log($"✓ Test attack execution: {result != null}");
                }
                
                // Test strategy usage
                if (testBattle != null && availableStrategies.Count > 0)
                {
                    var strategy = availableStrategies[0];
                    bool strategyUsed = UseStrategy(testBattle.BattleID, strategy.StrategyID);
                    Debug.Log($"✓ Test strategy usage: {strategyUsed}");
                }
            }
            
            // Test player profile
            var profile = GetPlayerProfile("test_player");
            Debug.Log($"✓ Test player profile: Level {profile.IPMLevel}, Battles Won: {profile.TotalBattlesWon}");
            
            // Test battle statistics
            var stats = GetBattleStats();
            Debug.Log($"✓ Test battle stats: {stats.TotalBattlesWon} battles won, {stats.TotalPestsDefeated} pests defeated");
            
            Debug.Log("✅ IPM battle system test completed");
        }
        
        #endregion
    }
    
    #region Supporting Data Structures
    
    [System.Serializable]
    public class PestBattle
    {
        public string BattleID;
        public string BattleName;
        public string BattleMode;
        public int Difficulty;
        public DateTime StartTime;
        public DateTime EndTime;
        public bool IsActive;
        public bool IsCompleted;
        public bool IsVictory;
        public string PlayerID;
        public List<PestEnemy> EnemyPests = new List<PestEnemy>();
        public List<IPMWeapon> PlayerWeapons = new List<IPMWeapon>();
        public IPMStrategy ActiveStrategy;
        public DateTime StrategyStartTime;
        public int CurrentTurn;
        public float PlayerHealth;
        public float TotalScore;
        public float FinalScore;
        public string VictoryCondition;
    }
    
    [System.Serializable]
    public class PestEnemy
    {
        public string PestID;
        public string PestName;
        public ProjectChimera.Data.IPM.PestType PestType;
        public int DifficultyLevel;
        public float MaxHealth;
        public float CurrentHealth;
        public float AttackPower;
        public string Description;
        public bool IsUnlocked;
        public bool IsDefeated;
        public List<string> SpecialAbilities = new List<string>();
        public List<IPMWeaponType> WeaknessTypes = new List<IPMWeaponType>();
        public List<IPMWeaponType> ResistanceTypes = new List<IPMWeaponType>();
    }
    
    [System.Serializable]
    public class IPMWeapon
    {
        public string WeaponID;
        public string WeaponName;
        public IPMWeaponType WeaponType;
        public float BaseDamage;
        public int RequiredLevel;
        public string Description;
        public bool IsUnlocked;
        public float CriticalChance;
        public List<string> EffectiveAgainst = new List<string>();
        public List<string> SpecialEffects = new List<string>();
    }
    
    [System.Serializable]
    public class IPMStrategy
    {
        public string StrategyID;
        public string StrategyName;
        public string Description;
        public float AttackModifier;
        public float DefenseModifier;
        public bool IsUnlocked;
        public TimeSpan EffectDuration;
        public TimeSpan CooldownTime;
    }
    
    [System.Serializable]
    public class DefenseUpgrade
    {
        public string UpgradeID;
        public string UpgradeName;
        public string Description;
        public float DefenseBonus;
        public int RequiredLevel;
        public bool IsUnlocked;
        public float MaintenanceCost;
        public string UpgradeCategory;
    }
    
    [System.Serializable]
    public class PlayerIPMProfile
    {
        public string PlayerID;
        public int IPMLevel;
        public int TotalBattlesWon;
        public int TotalPestsDefeated;
        public string FavoriteWeapon;
        public DateTime LastBattleDate;
        public List<string> UnlockedWeapons = new List<string>();
        public List<string> UnlockedStrategies = new List<string>();
        public List<string> BattleHistory = new List<string>();
    }
    
    [System.Serializable]
    public class BattleActionResult
    {
        public string WeaponUsed;
        public string TargetPest;
        public float DamageDealt;
        public bool IsCriticalHit;
        public bool PestDefeated;
        public float ScoreGained;
        public float CounterAttackDamage;
    }
    
    [System.Serializable]
    public class IPMBattleStats
    {
        public int TotalBattlesWon;
        public int TotalPestsDefeated;
        public float TotalDamageDealt;
        public int ActiveBattlesCount;
        public int AvailableWeaponsCount;
        public int AvailableStrategiesCount;
        public DateTime LastBattleUpdate;
    }
    
    // Removed conflicting PestType enum - using ProjectChimera.Data.IPM.PestType instead
    public enum IPMBattlePestCategory
    {
        Insect,
        Arachnid,
        Larva,
        Mutant
    }
    
    public enum IPMWeaponType
    {
        Organic,
        Chemical,
        Biological,
        Physical,
        Ultimate
    }
    
    #endregion
}