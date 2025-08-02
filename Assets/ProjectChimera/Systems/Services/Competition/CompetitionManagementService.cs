using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Systems.Registry;
using CompetitionData = ProjectChimera.Data.Competition;

namespace ProjectChimera.Systems.Services.CompetitionServices
{
    /// <summary>
    /// PC014-1a: Competition Management Service
    /// Handles tournament creation, scheduling, and lifecycle management
    /// Decomposed from CannabisCupManager (470 lines target)
    /// </summary>
    public class CompetitionManagementService : MonoBehaviour, ICompetitionManagementService
    {
        #region Properties
        
        public bool IsInitialized { get; private set; }
        
        #endregion

        #region Private Fields
        
        [Header("Configuration")]
        [SerializeField] private bool _enableCompetitions = true;
        [SerializeField] private bool _enableSeasonalEvents = true;
        [Range(1, 12)] [SerializeField] private int _competitionsPerYear = 4;
        [Range(1, 100)] [SerializeField] private int _maxEntriesPerCompetition = 50;
        [SerializeField] private float _competitionDuration = 2592000f; // 30 days
        
        [Header("Active Data")]
        [SerializeField] private List<CompetitionData.Competition> _activeCompetitions = new List<CompetitionData.Competition>();
        [SerializeField] private List<CompetitionData.Competition> _scheduledCompetitions = new List<CompetitionData.Competition>();
        [SerializeField] private List<CompetitionData.Competition> _completedCompetitions = new List<CompetitionData.Competition>();
        [SerializeField] private Dictionary<string, CompetitionData.CompetitionRules> _ruleTemplates = new Dictionary<string, CompetitionData.CompetitionRules>();
        [SerializeField] private List<CompetitionData.CompetitionFormat> _availableFormats = new List<CompetitionData.CompetitionFormat>();
        
        private int _nextCompetitionId = 1;
        
        #endregion

        #region Events
        
        public event Action<string> OnCompetitionCreated;
        public event Action<string> OnCompetitionStarted;
        public event Action<string> OnCompetitionEnded;
        public event Action<string> OnCompetitionCancelled;
        
        #endregion

        #region IService Implementation
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            Debug.Log("Initializing CompetitionManagementService...");
            
            // Initialize rule templates
            InitializeRuleTemplates();
            
            // Initialize competition formats
            InitializeCompetitionFormats();
            
            // Load existing competitions
            LoadExistingCompetitions();
            
            // Register with ServiceRegistry
            ServiceRegistry.Instance.RegisterService<ICompetitionManagementService>(this, ServiceDomain.Competition);
            
            IsInitialized = true;
            Debug.Log("CompetitionManagementService initialized successfully");
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            Debug.Log("Shutting down CompetitionManagementService...");
            
            // Save competition state
            SaveCompetitionState();
            
            // Clear collections
            _activeCompetitions.Clear();
            _scheduledCompetitions.Clear();
            _ruleTemplates.Clear();
            _availableFormats.Clear();
            
            IsInitialized = false;
            Debug.Log("CompetitionManagementService shutdown complete");
        }
        
        #endregion

        #region Tournament Management
        
        public string CreateTournament(string name, CompetitionData.CompetitionType type, DateTime startDate, DateTime endDate)
        {
            if (!IsInitialized)
            {
                Debug.LogError("CompetitionManagementService not initialized");
                return null;
            }

            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("Tournament name cannot be empty");
                return null;
            }

            if (startDate >= endDate)
            {
                Debug.LogError("Tournament start date must be before end date");
                return null;
            }

            string competitionId = GenerateCompetitionId();
            
            var competition = new CompetitionData.Competition
            {
                CompetitionId = competitionId,
                Name = name,
                Type = type,
                Tier = DetermineCompetitionTier(type),
                StartDate = startDate,
                EndDate = endDate,
                JudgingDeadline = endDate.AddDays(7),
                Categories = GetDefaultCategories(type),
                Rules = GetRulesForType(type),
                Entries = new List<CompetitionData.CompetitionEntry>(),
                IsActive = false,
                AcceptingEntries = true,
                EntryFee = CalculateEntryFee(type),
                Location = "Virtual",
                Metadata = new Dictionary<string, object>()
            };

            _scheduledCompetitions.Add(competition);
            
            OnCompetitionCreated?.Invoke(competitionId);
            Debug.Log($"Created tournament: {name} (ID: {competitionId})");
            
            return competitionId;
        }

        public bool ScheduleCompetition(string tournamentId, DateTime scheduledDate)
        {
            var competition = FindCompetition(tournamentId);
            if (competition == null)
            {
                Debug.LogError($"Competition not found: {tournamentId}");
                return false;
            }

            if (scheduledDate <= DateTime.Now)
            {
                Debug.LogError("Cannot schedule competition in the past");
                return false;
            }

            competition.StartDate = scheduledDate;
            competition.EndDate = scheduledDate.AddSeconds(_competitionDuration);
            competition.JudgingDeadline = competition.EndDate.AddDays(7);
            
            Debug.Log($"Scheduled competition {tournamentId} for {scheduledDate:yyyy-MM-dd}");
            return true;
        }

        public CompetitionData.CompetitionStatus GetCompetitionStatus(string competitionId)
        {
            var competition = FindCompetition(competitionId);
            if (competition == null)
                return CompetitionData.CompetitionStatus.NotFound;

            var now = DateTime.Now;
            
            if (now < competition.StartDate)
                return CompetitionData.CompetitionStatus.Scheduled;
            
            if (now >= competition.StartDate && now < competition.EndDate)
                return CompetitionData.CompetitionStatus.Active;
            
            if (now >= competition.EndDate && now < competition.JudgingDeadline)
                return CompetitionData.CompetitionStatus.Judging;
            
            if (competition.Results != null)
                return CompetitionData.CompetitionStatus.Complete;
            
            return CompetitionData.CompetitionStatus.Ended;
        }

        public List<CompetitionData.Competition> GetActiveCompetitions()
        {
            return new List<CompetitionData.Competition>(_activeCompetitions);
        }

        public List<CompetitionData.Competition> GetUpcomingCompetitions()
        {
            var now = DateTime.Now;
            return _scheduledCompetitions.Where(c => c.StartDate > now).ToList();
        }

        public bool CancelCompetition(string competitionId, string reason)
        {
            var competition = FindCompetition(competitionId);
            if (competition == null)
            {
                Debug.LogError($"Competition not found: {competitionId}");
                return false;
            }

            if (competition.IsActive)
            {
                Debug.LogError("Cannot cancel active competition");
                return false;
            }

            // Remove from scheduled or active lists
            _scheduledCompetitions.Remove(competition);
            _activeCompetitions.Remove(competition);
            
            // Store cancellation metadata
            competition.Metadata["CancellationReason"] = reason;
            competition.Metadata["CancellationDate"] = DateTime.Now;
            
            OnCompetitionCancelled?.Invoke(competitionId);
            Debug.Log($"Cancelled competition {competitionId}: {reason}");
            
            return true;
        }
        
        #endregion

        #region Event Lifecycle
        
        public void StartCompetition(string competitionId)
        {
            var competition = _scheduledCompetitions.FirstOrDefault(c => c.CompetitionId == competitionId);
            if (competition == null)
            {
                Debug.LogError($"Scheduled competition not found: {competitionId}");
                return;
            }

            if (DateTime.Now < competition.StartDate)
            {
                Debug.LogWarning($"Competition {competitionId} start date not reached");
                return;
            }

            // Move to active competitions
            _scheduledCompetitions.Remove(competition);
            _activeCompetitions.Add(competition);
            
            competition.IsActive = true;
            competition.AcceptingEntries = false;
            
            OnCompetitionStarted?.Invoke(competitionId);
            Debug.Log($"Started competition: {competition.Name}");
        }

        public void EndCompetition(string competitionId)
        {
            var competition = _activeCompetitions.FirstOrDefault(c => c.CompetitionId == competitionId);
            if (competition == null)
            {
                Debug.LogError($"Active competition not found: {competitionId}");
                return;
            }

            // Move to completed competitions
            _activeCompetitions.Remove(competition);
            _completedCompetitions.Add(competition);
            
            competition.IsActive = false;
            competition.AcceptingEntries = false;
            
            OnCompetitionEnded?.Invoke(competitionId);
            Debug.Log($"Ended competition: {competition.Name}");
        }

        public bool IsCompetitionActive(string competitionId)
        {
            return _activeCompetitions.Any(c => c.CompetitionId == competitionId);
        }

        public TimeSpan GetTimeUntilCompetition(string competitionId)
        {
            var competition = FindCompetition(competitionId);
            if (competition == null)
                return TimeSpan.Zero;

            var timeUntilStart = competition.StartDate - DateTime.Now;
            return timeUntilStart > TimeSpan.Zero ? timeUntilStart : TimeSpan.Zero;
        }
        
        #endregion

        #region Rules and Formats
        
        public void SetCompetitionRules(string competitionId, CompetitionData.CompetitionRules rules)
        {
            var competition = FindCompetition(competitionId);
            if (competition == null)
            {
                Debug.LogError($"Competition not found: {competitionId}");
                return;
            }

            if (competition.IsActive)
            {
                Debug.LogError("Cannot modify rules of active competition");
                return;
            }

            competition.Rules = rules;
            Debug.Log($"Updated rules for competition {competitionId}");
        }

        public CompetitionData.CompetitionRules GetCompetitionRules(string competitionId)
        {
            var competition = FindCompetition(competitionId);
            return competition?.Rules;
        }

        public List<CompetitionData.CompetitionFormat> GetAvailableFormats()
        {
            return new List<CompetitionData.CompetitionFormat>(_availableFormats);
        }
        
        #endregion

        #region Private Helper Methods
        
        private void InitializeRuleTemplates()
        {
            _ruleTemplates["Indoor"] = new CompetitionData.CompetitionRules
            {
                MaxPlantsPerEntry = 1,
                RequiredDocumentation = new List<string> { "GrowLog", "Photos", "NutrientSchedule" },
                JudgingCriteria = new List<string> { "Visual", "Aroma", "Potency", "Overall" },
                DisqualificationReasons = new List<string> { "Contamination", "Incomplete", "Fraud" }
            };
            
            _ruleTemplates["Outdoor"] = new CompetitionData.CompetitionRules
            {
                MaxPlantsPerEntry = 1,
                RequiredDocumentation = new List<string> { "GrowLog", "Photos", "WeatherData" },
                JudgingCriteria = new List<string> { "Visual", "Aroma", "Potency", "Size", "Overall" },
                DisqualificationReasons = new List<string> { "Contamination", "Incomplete", "Fraud" }
            };
        }

        private void InitializeCompetitionFormats()
        {
            _availableFormats.Add(new CompetitionData.CompetitionFormat
            {
                FormatId = "standard",
                Name = "Standard Competition",
                Duration = TimeSpan.FromDays(30),
                MaxEntries = 50,
                Categories = new List<string> { "Best Indica", "Best Sativa", "Best Hybrid" }
            });
            
            _availableFormats.Add(new CompetitionData.CompetitionFormat
            {
                FormatId = "championship",
                Name = "Championship Cup",
                Duration = TimeSpan.FromDays(60),
                MaxEntries = 100,
                Categories = new List<string> { "Overall Champion", "Best Genetics", "Best Cultivation" }
            });
        }

        private void LoadExistingCompetitions()
        {
            // TODO: Load from persistent storage
            Debug.Log("Loading existing competitions...");
        }

        private void SaveCompetitionState()
        {
            // TODO: Save to persistent storage
            Debug.Log("Saving competition state...");
        }

        private string GenerateCompetitionId()
        {
            return $"COMP_{DateTime.Now:yyyyMMdd}_{_nextCompetitionId++:D4}";
        }

        private CompetitionData.Competition FindCompetition(string competitionId)
        {
            var competition = _activeCompetitions.FirstOrDefault(c => c.CompetitionId == competitionId);
            if (competition != null) return competition;
            
            competition = _scheduledCompetitions.FirstOrDefault(c => c.CompetitionId == competitionId);
            if (competition != null) return competition;
            
            return _completedCompetitions.FirstOrDefault(c => c.CompetitionId == competitionId);
        }

        private CompetitionData.CompetitionTier DetermineCompetitionTier(CompetitionData.CompetitionType type)
        {
            return type switch
            {
                CompetitionData.CompetitionType.Championship => CompetitionData.CompetitionTier.Elite,
                CompetitionData.CompetitionType.Regional => CompetitionData.CompetitionTier.Professional,
                CompetitionData.CompetitionType.Local => CompetitionData.CompetitionTier.Amateur,
                CompetitionData.CompetitionType.Community => CompetitionData.CompetitionTier.Novice,
                _ => CompetitionData.CompetitionTier.Amateur
            };
        }

        private List<CompetitionData.CompetitionCategory> GetDefaultCategories(CompetitionData.CompetitionType type)
        {
            var categories = new List<CompetitionData.CompetitionCategory>();
            
            switch (type)
            {
                case CompetitionData.CompetitionType.Championship:
                    categories.Add(new CompetitionData.CompetitionCategory { CategoryId = "champion", Name = "Overall Champion" });
                    categories.Add(new CompetitionData.CompetitionCategory { CategoryId = "genetics", Name = "Best Genetics" });
                    categories.Add(new CompetitionData.CompetitionCategory { CategoryId = "cultivation", Name = "Best Cultivation" });
                    break;
                default:
                    categories.Add(new CompetitionData.CompetitionCategory { CategoryId = "indica", Name = "Best Indica" });
                    categories.Add(new CompetitionData.CompetitionCategory { CategoryId = "sativa", Name = "Best Sativa" });
                    categories.Add(new CompetitionData.CompetitionCategory { CategoryId = "hybrid", Name = "Best Hybrid" });
                    break;
            }
            
            return categories;
        }

        private CompetitionData.CompetitionRules GetRulesForType(CompetitionData.CompetitionType type)
        {
            string templateKey = type.ToString();
            if (_ruleTemplates.ContainsKey(templateKey))
            {
                return _ruleTemplates[templateKey];
            }
            
            return _ruleTemplates.ContainsKey("Indoor") ? _ruleTemplates["Indoor"] : new CompetitionData.CompetitionRules();
        }

        private float CalculateEntryFee(CompetitionData.CompetitionType type)
        {
            return type switch
            {
                CompetitionData.CompetitionType.Championship => 100f,
                CompetitionData.CompetitionType.Regional => 50f,
                CompetitionData.CompetitionType.Local => 25f,
                CompetitionData.CompetitionType.Community => 0f,
                _ => 25f
            };
        }
        
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        private void Update()
        {
            if (!IsInitialized) return;
            
            // Check for competitions that should start
            CheckScheduledCompetitions();
            
            // Check for competitions that should end
            CheckActiveCompetitions();
        }

        private void CheckScheduledCompetitions()
        {
            var now = DateTime.Now;
            var competitionsToStart = _scheduledCompetitions
                .Where(c => c.StartDate <= now && !c.IsActive)
                .ToList();

            foreach (var competition in competitionsToStart)
            {
                StartCompetition(competition.CompetitionId);
            }
        }

        private void CheckActiveCompetitions()
        {
            var now = DateTime.Now;
            var competitionsToEnd = _activeCompetitions
                .Where(c => c.EndDate <= now)
                .ToList();

            foreach (var competition in competitionsToEnd)
            {
                EndCompetition(competition.CompetitionId);
            }
        }
        
        #endregion
    }

}