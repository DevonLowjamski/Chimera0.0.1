using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.IPM;

namespace ProjectChimera.Systems.IPM
{
    /// <summary>
    /// Advanced tournament event scheduling and management system for IPM competitions.
    /// Part of PC017-2c: Add tournament event scheduling and management
    /// </summary>
    public class IPMTournamentScheduler : ChimeraManager
    {
        [Header("Scheduler Configuration")]
        [SerializeField] private bool _enableAutoScheduling = true;
        [SerializeField] private bool _enableRecurringTournaments = true;
        [SerializeField] private bool _enableConflictResolution = true;
        [SerializeField] private float _schedulingUpdateInterval = 60.0f; // 1 minute
        [SerializeField] private int _maxConcurrentTournaments = 10;
        [SerializeField] private float _minimumTournamentGap = 1800.0f; // 30 minutes
        
        [Header("Automatic Scheduling")]
        [SerializeField] private bool _enableDailyTournaments = true;
        [SerializeField] private bool _enableWeeklyTournaments = true;
        [SerializeField] private bool _enableSpecialEvents = true;
        [SerializeField] private int _dailyTournamentCount = 4;
        [SerializeField] private int _weeklyTournamentCount = 2;
        [SerializeField] private float _peakHoursMultiplier = 1.5f;
        
        [Header("Event Management")]
        [SerializeField] private bool _enableEventNotifications = true;
        [SerializeField] private bool _enablePreRegistration = true;
        [SerializeField] private float _registrationWindowHours = 24.0f;
        [SerializeField] private float _reminderIntervalHours = 2.0f;
        [SerializeField] private int _maxPreRegistrations = 128;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onTournamentScheduled;
        [SerializeField] private SimpleGameEventSO _onTournamentCancelled;
        [SerializeField] private SimpleGameEventSO _onScheduleConflict;
        [SerializeField] private SimpleGameEventSO _onRegistrationOpened;
        [SerializeField] private SimpleGameEventSO _onEventReminder;
        
        // Core scheduling data
        private Dictionary<string, ScheduledTournament> _scheduledTournaments = new Dictionary<string, ScheduledTournament>();
        private Dictionary<string, RecurringTournamentTemplate> _recurringTemplates = new Dictionary<string, RecurringTournamentTemplate>();
        private Dictionary<string, TournamentEventCalendar> _eventCalendars = new Dictionary<string, TournamentEventCalendar>();
        private List<SchedulingConflict> _activeConflicts = new List<SchedulingConflict>();
        
        // Event management
        private Dictionary<string, List<PlayerRegistration>> _preRegistrations = new Dictionary<string, List<PlayerRegistration>>();
        private Dictionary<string, EventNotification> _pendingNotifications = new Dictionary<string, EventNotification>();
        private Queue<ScheduledEvent> _eventQueue = new Queue<ScheduledEvent>();
        
        // Performance tracking
        private float _lastSchedulingUpdate;
        private int _totalTournamentsScheduled;
        private int _successfulTournaments;
        private int _cancelledTournaments;
        private float _averageParticipation;
        
        #region ChimeraManager Implementation
        
        protected override void OnManagerInitialize()
        {
            InitializeSchedulingSystem();
            LoadRecurringTemplates();
            LoadExistingSchedule();
            InitializeEventCalendars();
            _lastSchedulingUpdate = Time.time;
            
            Debug.Log($"[IPMTournamentScheduler] Initialized with auto-scheduling: {_enableAutoScheduling}, " +
                     $"Recurring tournaments: {_enableRecurringTournaments}, Max concurrent: {_maxConcurrentTournaments}");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!IsInitialized) return;
            
            float deltaTime = Time.time - _lastSchedulingUpdate;
            if (deltaTime >= _schedulingUpdateInterval)
            {
                ProcessScheduledEvents();
                UpdateTournamentSchedules();
                ProcessNotifications();
                ResolveSchedulingConflicts();
                
                if (_enableAutoScheduling)
                    ProcessAutoScheduling();
                    
                _lastSchedulingUpdate = Time.time;
            }
        }
        
        protected override void OnManagerShutdown()
        {
            SaveSchedulingData();
            SaveRecurringTemplates();
            
            _scheduledTournaments.Clear();
            _recurringTemplates.Clear();
            _eventCalendars.Clear();
            _preRegistrations.Clear();
            _pendingNotifications.Clear();
            
            Debug.Log("[IPMTournamentScheduler] Manager shutdown completed - all scheduling data saved");
        }
        
        #endregion
        
        #region Tournament Scheduling
        
        /// <summary>
        /// Schedule a new tournament event
        /// </summary>
        public string ScheduleTournament(TournamentScheduleRequest request)
        {
            // Validate scheduling request
            var validationResult = ValidateScheduleRequest(request);
            if (!validationResult.IsValid)
            {
                Debug.LogWarning($"[IPMTournamentScheduler] Invalid schedule request: {validationResult.ErrorMessage}");
                return null;
            }
            
            string eventId = GenerateEventId();
            
            var scheduledTournament = new ScheduledTournament
            {
                EventId = eventId,
                TournamentConfiguration = request.TournamentConfig,
                ScheduledStartTime = request.StartTime,
                ScheduledEndTime = request.EndTime ?? CalculateEndTime(request.StartTime, request.TournamentConfig),
                RegistrationOpenTime = request.RegistrationOpenTime ?? request.StartTime.AddHours(-_registrationWindowHours),
                RegistrationCloseTime = request.RegistrationCloseTime ?? request.StartTime.AddMinutes(-30),
                Status = ScheduledTournamentStatus.Scheduled,
                Priority = request.Priority,
                RecurrenceId = request.RecurrenceId,
                CreatedBy = request.CreatedBy,
                CreationTime = DateTime.Now,
                EstimatedParticipants = request.EstimatedParticipants,
                MinimumParticipants = request.MinimumParticipants,
                MaximumParticipants = request.MaximumParticipants,
                AutoStart = request.AutoStart,
                AllowLateRegistration = request.AllowLateRegistration,
                NotificationSettings = request.NotificationSettings ?? CreateDefaultNotificationSettings()
            };
            
            // Check for conflicts
            var conflicts = CheckForConflicts(scheduledTournament);
            if (conflicts.Count > 0 && _enableConflictResolution)
            {
                var resolved = ResolveSchedulingConflicts(scheduledTournament, conflicts);
                if (!resolved)
                {
                    Debug.LogWarning($"[IPMTournamentScheduler] Could not resolve scheduling conflicts for tournament {eventId}");
                    return null;
                }
            }
            
            _scheduledTournaments[eventId] = scheduledTournament;
            _totalTournamentsScheduled++;
            
            // Create calendar entry
            AddToEventCalendar(scheduledTournament);
            
            // Schedule notifications
            ScheduleEventNotifications(scheduledTournament);
            
            // Initialize pre-registration if enabled
            if (_enablePreRegistration)
            {
                _preRegistrations[eventId] = new List<PlayerRegistration>();
            }
            
            _onTournamentScheduled?.Raise();
            
            Debug.Log($"[IPMTournamentScheduler] Scheduled tournament '{scheduledTournament.TournamentConfiguration.Name}' " +
                     $"for {scheduledTournament.ScheduledStartTime:yyyy-MM-dd HH:mm}");
            
            return eventId;
        }
        
        /// <summary>
        /// Cancel a scheduled tournament
        /// </summary>
        public bool CancelTournament(string eventId, string reason = "Cancelled by administrator")
        {
            if (!_scheduledTournaments.ContainsKey(eventId))
            {
                Debug.LogWarning($"[IPMTournamentScheduler] Tournament {eventId} not found for cancellation");
                return false;
            }
            
            var tournament = _scheduledTournaments[eventId];
            tournament.Status = ScheduledTournamentStatus.Cancelled;
            tournament.CancellationReason = reason;
            tournament.CancellationTime = DateTime.Now;
            
            // Notify pre-registered players
            NotifyPreRegisteredPlayers(eventId, NotificationType.TournamentCancelled, reason);
            
            // Remove from calendar
            RemoveFromEventCalendar(tournament);
            
            // Clean up pre-registrations
            if (_preRegistrations.ContainsKey(eventId))
            {
                _preRegistrations.Remove(eventId);
            }
            
            _cancelledTournaments++;
            _onTournamentCancelled?.Raise();
            
            Debug.Log($"[IPMTournamentScheduler] Cancelled tournament {eventId}: {reason}");
            return true;
        }
        
        /// <summary>
        /// Reschedule an existing tournament
        /// </summary>
        public bool RescheduleTournament(string eventId, DateTime newStartTime, DateTime? newEndTime = null)
        {
            if (!_scheduledTournaments.ContainsKey(eventId))
            {
                Debug.LogWarning($"[IPMTournamentScheduler] Tournament {eventId} not found for rescheduling");
                return false;
            }
            
            var tournament = _scheduledTournaments[eventId];
            var oldStartTime = tournament.ScheduledStartTime;
            
            tournament.ScheduledStartTime = newStartTime;
            tournament.ScheduledEndTime = newEndTime ?? CalculateEndTime(newStartTime, tournament.TournamentConfiguration);
            tournament.RegistrationOpenTime = newStartTime.AddHours(-_registrationWindowHours);
            tournament.RegistrationCloseTime = newStartTime.AddMinutes(-30);
            
            // Check for new conflicts
            var conflicts = CheckForConflicts(tournament);
            if (conflicts.Count > 0)
            {
                Debug.LogWarning($"[IPMTournamentScheduler] Rescheduling would create conflicts for tournament {eventId}");
                // Revert changes
                tournament.ScheduledStartTime = oldStartTime;
                return false;
            }
            
            // Update calendar
            UpdateEventCalendar(tournament);
            
            // Reschedule notifications
            RescheduleEventNotifications(tournament);
            
            // Notify pre-registered players
            NotifyPreRegisteredPlayers(eventId, NotificationType.TournamentRescheduled, 
                $"Tournament rescheduled to {newStartTime:yyyy-MM-dd HH:mm}");
            
            Debug.Log($"[IPMTournamentScheduler] Rescheduled tournament {eventId} from {oldStartTime:HH:mm} to {newStartTime:HH:mm}");
            return true;
        }
        
        #endregion
        
        #region Recurring Tournaments
        
        /// <summary>
        /// Create a recurring tournament template
        /// </summary>
        public string CreateRecurringTournament(RecurringTournamentRequest request)
        {
            string templateId = GenerateTemplateId();
            
            var template = new RecurringTournamentTemplate
            {
                TemplateId = templateId,
                Name = request.Name,
                TournamentConfiguration = request.TournamentConfig,
                RecurrencePattern = request.RecurrencePattern,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = true,
                CreatedBy = request.CreatedBy,
                CreationTime = DateTime.Now,
                NextScheduledTime = CalculateNextOccurrence(request.RecurrencePattern, request.StartDate),
                GeneratedEvents = new List<string>(),
                MaxConcurrentInstances = request.MaxConcurrentInstances,
                AutoApproval = request.AutoApproval
            };
            
            _recurringTemplates[templateId] = template;
            
            // Generate initial set of tournaments
            GenerateRecurringTournaments(template, 30); // Generate for next 30 days
            
            Debug.Log($"[IPMTournamentScheduler] Created recurring tournament template '{template.Name}' with ID {templateId}");
            return templateId;
        }
        
        /// <summary>
        /// Generate tournaments from recurring template
        /// </summary>
        private void GenerateRecurringTournaments(RecurringTournamentTemplate template, int daysAhead)
        {
            var endDate = DateTime.Now.AddDays(daysAhead);
            var currentDate = template.NextScheduledTime;
            
            while (currentDate <= endDate && template.IsActive)
            {
                // Create tournament configuration for this instance
                var config = CloneTournamentConfiguration(template.TournamentConfiguration);
                config.Name = $"{template.Name} - {currentDate:MMM dd}";
                
                var scheduleRequest = new TournamentScheduleRequest
                {
                    TournamentConfig = config,
                    StartTime = currentDate,
                    RecurrenceId = template.TemplateId,
                    CreatedBy = "System",
                    AutoStart = template.AutoApproval,
                    Priority = EventPriority.Normal
                };
                
                string eventId = ScheduleTournament(scheduleRequest);
                if (eventId != null)
                {
                    template.GeneratedEvents.Add(eventId);
                }
                
                // Calculate next occurrence
                currentDate = CalculateNextOccurrence(template.RecurrencePattern, currentDate);
            }
            
            template.NextScheduledTime = currentDate;
        }
        
        #endregion
        
        #region Player Registration
        
        /// <summary>
        /// Register player for a scheduled tournament
        /// </summary>
        public bool RegisterPlayerForScheduledTournament(string eventId, string playerId, PlayerInfo playerInfo)
        {
            if (!_scheduledTournaments.ContainsKey(eventId))
            {
                Debug.LogWarning($"[IPMTournamentScheduler] Tournament {eventId} not found for registration");
                return false;
            }
            
            var tournament = _scheduledTournaments[eventId];
            
            // Check registration window
            var now = DateTime.Now;
            if (now < tournament.RegistrationOpenTime)
            {
                Debug.LogWarning($"[IPMTournamentScheduler] Registration not yet open for tournament {eventId}");
                return false;
            }
            
            if (now > tournament.RegistrationCloseTime && !tournament.AllowLateRegistration)
            {
                Debug.LogWarning($"[IPMTournamentScheduler] Registration closed for tournament {eventId}");
                return false;
            }
            
            if (!_preRegistrations.ContainsKey(eventId))
            {
                _preRegistrations[eventId] = new List<PlayerRegistration>();
            }
            
            var registrations = _preRegistrations[eventId];
            
            // Check if already registered
            if (registrations.Any(r => r.PlayerId == playerId))
            {
                Debug.LogWarning($"[IPMTournamentScheduler] Player {playerId} already registered for tournament {eventId}");
                return false;
            }
            
            // Check capacity
            if (registrations.Count >= tournament.MaximumParticipants)
            {
                Debug.LogWarning($"[IPMTournamentScheduler] Tournament {eventId} is full");
                return false;
            }
            
            var registration = new PlayerRegistration
            {
                RegistrationId = GenerateRegistrationId(),
                EventId = eventId,
                PlayerId = playerId,
                PlayerName = playerInfo.PlayerName,
                RegistrationTime = DateTime.Now,
                Status = RegistrationStatus.Confirmed,
                PlayerRating = GetPlayerRating(playerId),
                PreferredSettings = new Dictionary<string, object>()
            };
            
            registrations.Add(registration);
            
            // Send confirmation notification
            SendRegistrationConfirmation(registration, tournament);
            
            Debug.Log($"[IPMTournamentScheduler] Registered player {playerInfo.PlayerName} for tournament {tournament.TournamentConfiguration.Name}");
            return true;
        }
        
        /// <summary>
        /// Get upcoming tournaments for player
        /// </summary>
        public List<UpcomingTournament> GetUpcomingTournaments(string playerId = null, int daysAhead = 7)
        {
            var upcomingTournaments = new List<UpcomingTournament>();
            var cutoffDate = DateTime.Now.AddDays(daysAhead);
            
            foreach (var tournament in _scheduledTournaments.Values)
            {
                if (tournament.Status == ScheduledTournamentStatus.Scheduled && 
                    tournament.ScheduledStartTime <= cutoffDate &&
                    tournament.ScheduledStartTime >= DateTime.Now)
                {
                    var upcomingTournament = new UpcomingTournament
                    {
                        EventId = tournament.EventId,
                        Name = tournament.TournamentConfiguration.Name,
                        StartTime = tournament.ScheduledStartTime,
                        EndTime = tournament.ScheduledEndTime,
                        TournamentType = tournament.TournamentConfiguration.TournamentType,
                        EstimatedParticipants = tournament.EstimatedParticipants,
                        RegistrationStatus = GetPlayerRegistrationStatus(tournament.EventId, playerId),
                        RegistrationOpenTime = tournament.RegistrationOpenTime,
                        RegistrationCloseTime = tournament.RegistrationCloseTime,
                        IsRecurring = !string.IsNullOrEmpty(tournament.RecurrenceId),
                        PrizePool = tournament.TournamentConfiguration.PrizePool,
                        EntryFee = tournament.TournamentConfiguration.EntryFee
                    };
                    
                    upcomingTournaments.Add(upcomingTournament);
                }
            }
            
            return upcomingTournaments.OrderBy(t => t.StartTime).ToList();
        }
        
        #endregion
        
        #region Event Processing
        
        /// <summary>
        /// Process scheduled events that are ready to execute
        /// </summary>
        private void ProcessScheduledEvents()
        {
            var now = DateTime.Now;
            var eventsToProcess = new List<ScheduledEvent>();
            
            while (_eventQueue.Count > 0)
            {
                var scheduledEvent = _eventQueue.Peek();
                if (scheduledEvent.ExecutionTime <= now)
                {
                    eventsToProcess.Add(_eventQueue.Dequeue());
                }
                else
                {
                    break; // Queue is ordered by time, so we can stop here
                }
            }
            
            foreach (var scheduledEvent in eventsToProcess)
            {
                ProcessScheduledEvent(scheduledEvent);
            }
        }
        
        /// <summary>
        /// Process individual scheduled event
        /// </summary>
        private void ProcessScheduledEvent(ScheduledEvent scheduledEvent)
        {
            switch (scheduledEvent.EventType)
            {
                case ScheduledEventType.TournamentStart:
                    StartScheduledTournament(scheduledEvent.EventId);
                    break;
                    
                case ScheduledEventType.RegistrationOpen:
                    OpenTournamentRegistration(scheduledEvent.EventId);
                    break;
                    
                case ScheduledEventType.RegistrationClose:
                    CloseTournamentRegistration(scheduledEvent.EventId);
                    break;
                    
                case ScheduledEventType.SendReminder:
                    SendTournamentReminder(scheduledEvent.EventId);
                    break;
                    
                case ScheduledEventType.CheckMinimumParticipants:
                    CheckMinimumParticipants(scheduledEvent.EventId);
                    break;
            }
        }
        
        #endregion
        
        #region Public API Methods
        
        /// <summary>
        /// Get comprehensive tournament schedule
        /// </summary>
        public TournamentScheduleData GetTournamentSchedule(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now;
            endDate ??= DateTime.Now.AddDays(30);
            
            var scheduleData = new TournamentScheduleData
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                ScheduledTournaments = _scheduledTournaments.Values
                    .Where(t => t.ScheduledStartTime >= startDate && t.ScheduledStartTime <= endDate)
                    .OrderBy(t => t.ScheduledStartTime)
                    .ToList(),
                RecurringTemplates = _recurringTemplates.Values.Where(t => t.IsActive).ToList(),
                TotalScheduled = 0,
                ActiveConflicts = _activeConflicts,
                SystemMetrics = GetSchedulerMetrics()
            };
            
            scheduleData.TotalScheduled = scheduleData.ScheduledTournaments.Count;
            
            return scheduleData;
        }
        
        /// <summary>
        /// Get scheduler performance metrics
        /// </summary>
        public TournamentSchedulerMetrics GetSchedulerMetrics()
        {
            return new TournamentSchedulerMetrics
            {
                TotalTournamentsScheduled = _totalTournamentsScheduled,
                SuccessfulTournaments = _successfulTournaments,
                CancelledTournaments = _cancelledTournaments,
                ActiveScheduledTournaments = _scheduledTournaments.Count(t => t.Value.Status == ScheduledTournamentStatus.Scheduled),
                AverageParticipation = _averageParticipation,
                RecurringTemplates = _recurringTemplates.Count,
                ActiveConflicts = _activeConflicts.Count,
                PreRegistrations = _preRegistrations.Values.Sum(r => r.Count),
                PendingNotifications = _pendingNotifications.Count,
                LastUpdateTime = DateTime.Now
            };
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private void InitializeSchedulingSystem()
        {
            _scheduledTournaments.Clear();
            _recurringTemplates.Clear();
            _eventCalendars.Clear();
            _preRegistrations.Clear();
            _pendingNotifications.Clear();
            _activeConflicts.Clear();
            
            _totalTournamentsScheduled = 0;
            _successfulTournaments = 0;
            _cancelledTournaments = 0;
            _averageParticipation = 0.0f;
        }
        
        // Placeholder methods for complex scheduling logic
        private string GenerateEventId() => $"EVENT_{DateTime.Now.Ticks}";
        private string GenerateTemplateId() => $"TEMPLATE_{DateTime.Now.Ticks}";
        private string GenerateRegistrationId() => $"REG_{DateTime.Now.Ticks}";
        private ScheduleValidationResult ValidateScheduleRequest(TournamentScheduleRequest request) => new ScheduleValidationResult { IsValid = true };
        private DateTime CalculateEndTime(DateTime startTime, TournamentConfiguration config) => startTime.AddHours(2);
        private NotificationSettings CreateDefaultNotificationSettings() => new NotificationSettings();
        private List<SchedulingConflict> CheckForConflicts(ScheduledTournament tournament) => new List<SchedulingConflict>();
        private bool ResolveSchedulingConflicts(ScheduledTournament tournament, List<SchedulingConflict> conflicts) => true;
        private void AddToEventCalendar(ScheduledTournament tournament) { }
        private void ScheduleEventNotifications(ScheduledTournament tournament) { }
        private void NotifyPreRegisteredPlayers(string eventId, NotificationType type, string message) { }
        private void RemoveFromEventCalendar(ScheduledTournament tournament) { }
        private void UpdateEventCalendar(ScheduledTournament tournament) { }
        private void RescheduleEventNotifications(ScheduledTournament tournament) { }
        private DateTime CalculateNextOccurrence(RecurrencePattern pattern, DateTime currentDate) => currentDate.AddDays(1);
        private TournamentConfiguration CloneTournamentConfiguration(TournamentConfiguration config) => config;
        private float GetPlayerRating(string playerId) => 1200.0f;
        private void SendRegistrationConfirmation(PlayerRegistration registration, ScheduledTournament tournament) { }
        private RegistrationStatus GetPlayerRegistrationStatus(string eventId, string playerId) => RegistrationStatus.NotRegistered;
        private void StartScheduledTournament(string eventId) { }
        private void OpenTournamentRegistration(string eventId) { _onRegistrationOpened?.Raise(); }
        private void CloseTournamentRegistration(string eventId) { }
        private void SendTournamentReminder(string eventId) { _onEventReminder?.Raise(); }
        private void CheckMinimumParticipants(string eventId) { }
        private void ProcessAutoScheduling() { }
        private void UpdateTournamentSchedules() { }
        private void ProcessNotifications() { }
        private void ResolveSchedulingConflicts() { }
        private void LoadRecurringTemplates() { }
        private void LoadExistingSchedule() { }
        private void InitializeEventCalendars() { }
        private void SaveSchedulingData() { }
        private void SaveRecurringTemplates() { }
        
        #endregion
    }
}