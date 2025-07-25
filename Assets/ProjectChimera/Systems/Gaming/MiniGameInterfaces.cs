using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectChimera.Systems.Gaming
{
    /// <summary>
    /// Core interface for all mini-games in Project Chimera
    /// </summary>
    public interface IMiniGame
    {
        string GameId { get; }
        string GameName { get; }
        string Description { get; }
        MiniGameCategory Category { get; }
        DifficultyLevel CurrentDifficulty { get; }
        
        // Lifecycle
        void Initialize(MiniGameContext context);
        void StartGame(MiniGameParameters parameters);
        void UpdateGame(float deltaTime);
        void EndGame(MiniGameResult result);
        void Cleanup();
        
        // Events
        event Action<MiniGameResult> OnGameCompleted;
        event Action<MiniGameProgress> OnProgressUpdated;
        event Action<string> OnGameEvent;
    }
    
    /// <summary>
    /// Base implementation of mini-game interface with common functionality
    /// </summary>
    public abstract class MiniGameBase : IMiniGame
    {
        public abstract string GameId { get; }
        public abstract string GameName { get; }
        public abstract string Description { get; }
        public abstract MiniGameCategory Category { get; }
        
        public DifficultyLevel CurrentDifficulty { get; protected set; }
        
        public event Action<MiniGameResult> OnGameCompleted;
        public event Action<MiniGameProgress> OnProgressUpdated;
        public event Action<string> OnGameEvent;
        
        protected MiniGameContext _context;
        protected MiniGameParameters _parameters;
        protected bool _isActive;
        protected float _gameTimer;
        
        public virtual void Initialize(MiniGameContext context)
        {
            _context = context;
            OnInitialize();
        }
        
        public virtual void StartGame(MiniGameParameters parameters)
        {
            _parameters = parameters;
            CurrentDifficulty = parameters.Difficulty;
            _isActive = true;
            _gameTimer = 0f;
            
            OnStartGame();
        }
        
        public virtual void UpdateGame(float deltaTime)
        {
            if (!_isActive) return;
            
            _gameTimer += deltaTime;
            OnUpdateGame(deltaTime);
        }
        
        public virtual void EndGame(MiniGameResult result)
        {
            _isActive = false;
            OnEndGame(result);
            OnGameCompleted?.Invoke(result);
        }
        
        public virtual void Cleanup()
        {
            OnCleanup();
        }
        
        protected virtual void OnInitialize() { }
        protected virtual void OnStartGame() { }
        protected virtual void OnUpdateGame(float deltaTime) { }
        protected virtual void OnEndGame(MiniGameResult result) { }
        protected virtual void OnCleanup() { }
        
        protected void FireProgressUpdate(MiniGameProgress progress)
        {
            OnProgressUpdated?.Invoke(progress);
        }
        
        protected void FireGameEvent(string eventMessage)
        {
            OnGameEvent?.Invoke(eventMessage);
        }
    }
    
    /// <summary>
    /// Categories of mini-games for organization and filtering
    /// </summary>
    public enum MiniGameCategory
    {
        Cultivation,
        Genetics,
        Business,
        Environmental,
        Technical,
        Social,
        Educational,
        Crisis
    }
    
    // DifficultyLevel enum moved to MiniGameDataTypes.cs to avoid duplicates
    
    /// <summary>
    /// Status of a mini-game session
    /// </summary>
    public enum MiniGameStatus
    {
        Created,
        Starting,
        Active,
        Paused,
        Completed,
        Failed,
        Timeout,
        Error
    }
    
    /// <summary>
    /// Context data for mini-game initialization
    /// </summary>
    [Serializable]
    public class MiniGameContext
    {
        public GameObject PlayerReference;
        public Transform UIParent;
        public Dictionary<string, object> GlobalContext = new Dictionary<string, object>();
        public bool EnableTutorials = true;
        public float UIScale = 1.0f;
    }
    
    /// <summary>
    /// Parameters for starting a mini-game
    /// </summary>
    [Serializable]
    public class MiniGameParameters
    {
        public DifficultyLevel Difficulty = DifficultyLevel.Adaptive;
        public float TimeLimit = 300f; // 5 minutes default
        public string Context = "general";
        public Dictionary<string, object> CustomParameters = new Dictionary<string, object>();
        
        // Integration with other systems
        public PlantInstance PlantReference;
        public EnvironmentalCrisis CrisisData;
        public MarketData MarketContext;
        public EquipmentData EquipmentContext;
    }
    
    /// <summary>
    /// Progress tracking during mini-game play
    /// </summary>
    [Serializable]
    public class MiniGameProgress
    {
        public string GameId;
        public string SessionId;
        public float CompletionPercentage; // 0-100
        public float CurrentScore;
        public float Accuracy;
        public int CorrectAnswers;
        public int TotalAttempts;
        public float TimeElapsed;
        public List<string> AchievedMilestones = new List<string>();
        public Dictionary<string, float> DetailedMetrics = new Dictionary<string, float>();
    }
    
    /// <summary>
    /// Final result of a completed mini-game
    /// </summary>
    [Serializable]
    public class MiniGameResult
    {
        public string SessionId;
        public string GameId;
        public float Score;
        public float Accuracy;
        public float CompletionTime;
        public DifficultyLevel Difficulty;
        public bool PerfectBonus;
        public float StreakMultiplier;
        public bool IsTimeout;
        public Dictionary<string, float> EducationalMetrics = new Dictionary<string, float>();
        public List<string> UnlockedContent = new List<string>();
        public float ExperienceGained;
        public int CurrencyEarned;
        
        public bool IsSuccess() => Score >= GetPassingScore();
        
        private float GetPassingScore()
        {
            return Difficulty switch
            {
                DifficultyLevel.Tutorial => 50f,
                DifficultyLevel.Beginner => 60f,
                DifficultyLevel.Intermediate => 70f,
                DifficultyLevel.Advanced => 80f,
                DifficultyLevel.Hard => 82f,
                DifficultyLevel.Expert => 85f,
                DifficultyLevel.Master => 90f,
                _ => 70f
            };
        }
    }
    
    /// <summary>
    /// Current mini-game session data
    /// </summary>
    [Serializable]
    public class MiniGameSession
    {
        public string SessionId;
        public string GameId;
        public IMiniGameImplementation MiniGame;
        public MiniGameParameters Parameters;
        public DateTime StartTime;
        public DateTime EndTime;
        public MiniGameStatus Status;
        public float TimeLimit;
    }
    
    /// <summary>
    /// Active session wrapper with runtime data
    /// </summary>
    public class ActiveMiniGameSession
    {
        public MiniGameSession Session;
        public float TimeRemaining;
        public MiniGamePerformanceTracker PerformanceTracker;
    }
    
    /// <summary>
    /// Player statistics across all mini-games
    /// </summary>
    [Serializable]
    public class MiniGameStatistics
    {
        public int TotalGamesPlayed;
        public int SuccessfulGames;
        public float TotalTimeSpent;
        public float AverageScore;
        public Dictionary<string, GameSpecificStats> GameSpecificStats = new Dictionary<string, GameSpecificStats>();
        public Dictionary<string, float> SkillLevels = new Dictionary<string, float>();
        public List<string> UnlockedGames = new List<string>();
        public DateTime LastPlayed;
        
        public float GetSuccessRate() => TotalGamesPlayed > 0 ? (float)SuccessfulGames / TotalGamesPlayed : 0f;
        public float GetAverageSessionTime() => TotalGamesPlayed > 0 ? TotalTimeSpent / TotalGamesPlayed : 0f;
    }
    
    // GameSpecificStats class moved to MiniGameDataTypes.cs to avoid duplicates
    
    /// <summary>
    /// Performance metrics for mini-game optimization
    /// </summary>
    [Serializable]
    public class GamePerformanceMetrics
    {
        public float AverageScore;
        public float AverageCompletionTime;
        public float SuccessRate;
        public int TotalSessions;
        public float MemoryUsage;
        public float AverageFrameRate;
        public List<float> RecentScores = new List<float>();
    }
    
    /// <summary>
    /// Suggestion for contextual mini-game activation
    /// </summary>
    public class MiniGameSuggestion
    {
        public string GameId;
        public MiniGameParameters Parameters;
        public float Urgency; // 0-1, higher = more urgent
        public DateTime ExpirationTime;
        public string Reason;
        public List<string> ExpectedBenefits = new List<string>();
    }
    
    /// <summary>
    /// Performance tracking for mini-game sessions
    /// </summary>
    [Serializable]
    public class MiniGamePerformanceTracker
    {
        public string SessionId;
        public float FrameRate;
        public float MemoryUsage;
        public float CpuUsage;
        public List<float> PerformanceSamples = new List<float>();
        public DateTime StartTime;
        public DateTime LastUpdate;
        
        public void UpdatePerformance(float deltaTime)
        {
            FrameRate = 1f / deltaTime;
            LastUpdate = DateTime.Now;
            PerformanceSamples.Add(FrameRate);
            
            // Keep only last 100 samples
            if (PerformanceSamples.Count > 100)
            {
                PerformanceSamples.RemoveAt(0);
            }
        }
        
        public float GetAverageFrameRate()
        {
            return PerformanceSamples.Count > 0 ? PerformanceSamples.Average() : 0f;
        }
        
        public float CalculateFinalScore()
        {
            // Placeholder score calculation based on performance
            return GetAverageFrameRate() > 30f ? 85f : 70f;
        }
        
        public float CalculateAccuracy()
        {
            // Placeholder accuracy calculation
            return 0.85f; // 85% accuracy
        }
        
        public bool IsPerfectPerformance()
        {
            // Perfect performance if frame rate is consistently high
            return GetAverageFrameRate() > 55f;
        }
        
        public Dictionary<string, float> CalculateEducationalMetrics()
        {
            return new Dictionary<string, float>
            {
                {"technical_skill", 0.8f},
                {"problem_solving", 0.7f},
                {"attention_to_detail", 0.9f}
            };
        }
        
        public void UpdateProgress(MiniGameProgress progress)
        {
            // Update internal progress tracking
            LastUpdate = DateTime.Now;
        }
    }
}