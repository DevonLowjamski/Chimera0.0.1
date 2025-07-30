using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.IPM;

namespace ProjectChimera.Systems.IPM
{
    // Tournament scheduling enums
    public enum ScheduledTournamentStatus
    {
        Scheduled,
        RegistrationOpen,
        RegistrationClosed,
        InProgress,
        Completed,
        Cancelled,
        Postponed,
        WaitingForMinimumParticipants
    }
    
    public enum EventPriority
    {
        Low,
        Normal,
        High,
        Critical,
        Emergency
    }
    
    public enum ScheduledEventType
    {
        TournamentStart,
        TournamentEnd,
        RegistrationOpen,
        RegistrationClose,
        SendReminder,
        CheckMinimumParticipants,
        SendNotification,
        GenerateRecurring,
        CleanupExpired
    }
    
    public enum RegistrationStatus
    {
        NotRegistered,
        PreRegistered,
        Confirmed,
        Waitlisted,
        Cancelled,
        CheckedIn,
        NoShow
    }
    
    public enum NotificationType
    {
        RegistrationOpened,
        RegistrationClosing,
        TournamentStarting,
        TournamentCancelled,
        TournamentRescheduled,
        PlayerRegistered,
        PlayerCancelled,
        ReminderNotification,
        SystemAlert
    }
    
    public enum RecurrenceType
    {
        None,
        Daily,
        Weekly,
        Monthly,
        Custom
    }
    
    // Core scheduling structures
    [Serializable]
    public class ScheduledTournament
    {
        public string EventId;
        public TournamentConfiguration TournamentConfiguration;
        public DateTime ScheduledStartTime;
        public DateTime ScheduledEndTime;
        public DateTime RegistrationOpenTime;
        public DateTime RegistrationCloseTime;
        public ScheduledTournamentStatus Status;
        public EventPriority Priority;
        public string RecurrenceId; // Links to recurring template
        public string CreatedBy;
        public DateTime CreationTime;
        public int EstimatedParticipants;
        public int MinimumParticipants;
        public int MaximumParticipants;
        public bool AutoStart;
        public bool AllowLateRegistration;
        public NotificationSettings NotificationSettings;
        public string CancellationReason;
        public DateTime? CancellationTime;
        public Dictionary<string, object> EventMetadata = new Dictionary<string, object>();
        public List<string> ConflictingEvents = new List<string>();
    }
    
    [Serializable]
    public class TournamentScheduleRequest
    {
        public TournamentConfiguration TournamentConfig;
        public DateTime StartTime;
        public DateTime? EndTime;
        public DateTime? RegistrationOpenTime;
        public DateTime? RegistrationCloseTime;
        public EventPriority Priority = EventPriority.Normal;
        public string RecurrenceId;
        public string CreatedBy;
        public int EstimatedParticipants = 16;
        public int MinimumParticipants = 4;
        public int MaximumParticipants = 64;
        public bool AutoStart = false;
        public bool AllowLateRegistration = false;
        public NotificationSettings NotificationSettings;
        public Dictionary<string, object> CustomSettings = new Dictionary<string, object>();
    }
    
    // Recurring tournament structures
    [Serializable]
    public class RecurringTournamentTemplate
    {
        public string TemplateId;
        public string Name;
        public TournamentConfiguration TournamentConfiguration;
        public RecurrencePattern RecurrencePattern;
        public DateTime StartDate;
        public DateTime? EndDate;
        public bool IsActive;
        public string CreatedBy;
        public DateTime CreationTime;
        public DateTime NextScheduledTime;
        public List<string> GeneratedEvents = new List<string>();
        public int MaxConcurrentInstances = 1;
        public bool AutoApproval = true;
        public Dictionary<string, object> TemplateSettings = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class RecurringTournamentRequest
    {
        public string Name;
        public TournamentConfiguration TournamentConfig;
        public RecurrencePattern RecurrencePattern;
        public DateTime StartDate;
        public DateTime? EndDate;
        public string CreatedBy;
        public int MaxConcurrentInstances = 1;
        public bool AutoApproval = true;
        public Dictionary<string, object> CustomSettings = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class RecurrencePattern
    {
        public RecurrenceType Type;
        public int Interval = 1; // Every N days/weeks/months
        public List<DayOfWeek> DaysOfWeek = new List<DayOfWeek>(); // For weekly
        public List<int> DaysOfMonth = new List<int>(); // For monthly
        public TimeSpan PreferredTime;
        public Dictionary<string, object> CustomParameters = new Dictionary<string, object>();
        public bool SkipHolidays;
        public List<DateTime> ExcludedDates = new List<DateTime>();
    }
    
    // Player registration structures
    [Serializable]
    public class PlayerRegistration
    {
        public string RegistrationId;
        public string EventId;
        public string PlayerId;
        public string PlayerName;
        public DateTime RegistrationTime;
        public RegistrationStatus Status;
        public float PlayerRating;
        public Dictionary<string, object> PreferredSettings = new Dictionary<string, object>();
        public string Notes;
        public DateTime? CheckInTime;
        public bool IsStandby;
        public int Priority; // For waitlist ordering
    }
    
    [Serializable]
    public class UpcomingTournament
    {
        public string EventId;
        public string Name;
        public DateTime StartTime;
        public DateTime EndTime;
        public TournamentType TournamentType;
        public int EstimatedParticipants;
        public RegistrationStatus RegistrationStatus;
        public DateTime RegistrationOpenTime;
        public DateTime RegistrationCloseTime;
        public bool IsRecurring;
        public float PrizePool;
        public float EntryFee;
        public string Description;
        public Dictionary<string, object> TournamentInfo = new Dictionary<string, object>();
    }
    
    // Event scheduling and processing
    [Serializable]
    public class ScheduledEvent
    {
        public string EventId;
        public ScheduledEventType EventType;
        public DateTime ExecutionTime;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
        public int RetryCount;
        public bool IsRecurring;
        public string Description;
        public EventPriority Priority;
    }
    
    [Serializable]
    public class TournamentEventCalendar
    {
        public string CalendarId;
        public string Name;
        public Dictionary<DateTime, List<ScheduledTournament>> EventsByDate = new Dictionary<DateTime, List<ScheduledTournament>>();
        public List<RecurringTournamentTemplate> RecurringEvents = new List<RecurringTournamentTemplate>();
        public DateTime LastUpdated;
        public Dictionary<string, object> CalendarSettings = new Dictionary<string, object>();
    }
    
    // Conflict resolution
    [Serializable]
    public class SchedulingConflict
    {
        public string ConflictId;
        public List<string> ConflictingEventIds = new List<string>();
        public string ConflictType; // "TimeOverlap", "ResourceConflict", "ParticipantLimit", etc.
        public string Description;
        public ConflictSeverity Severity;
        public DateTime DetectedTime;
        public bool IsResolved;
        public string ResolutionAction;
        public Dictionary<string, object> ConflictData = new Dictionary<string, object>();
    }
    
    public enum ConflictSeverity
    {
        Minor,
        Moderate,
        Major,
        Critical
    }
    
    [Serializable]
    public class ScheduleValidationResult
    {
        public bool IsValid;
        public string ErrorMessage;
        public List<string> Warnings = new List<string>();
        public List<SchedulingConflict> PotentialConflicts = new List<SchedulingConflict>();
        public Dictionary<string, object> ValidationData = new Dictionary<string, object>();
    }
    
    // Notification system
    [Serializable]
    public class NotificationSettings
    {
        public bool EnableReminders = true;
        public List<TimeSpan> ReminderIntervals = new List<TimeSpan> 
        { 
            TimeSpan.FromHours(24), 
            TimeSpan.FromHours(2), 
            TimeSpan.FromMinutes(30) 
        };
        public bool NotifyOnRegistration = true;
        public bool NotifyOnCancellation = true;
        public bool NotifyOnReschedule = true;
        public Dictionary<NotificationType, bool> NotificationPreferences = new Dictionary<NotificationType, bool>();
        public List<string> CustomRecipients = new List<string>();
    }
    
    [Serializable]
    public class EventNotification
    {
        public string NotificationId;
        public string EventId;
        public NotificationType Type;
        public string Title;
        public string Message;
        public DateTime ScheduledTime;
        public DateTime? SentTime;
        public List<string> Recipients = new List<string>();
        public bool IsSent;
        public Dictionary<string, object> NotificationData = new Dictionary<string, object>();
        public string TemplateId;
        public EventPriority Priority;
    }
    
    // Reporting and analytics
    [Serializable]
    public class TournamentScheduleData
    {
        public DateTime StartDate;
        public DateTime EndDate;
        public List<ScheduledTournament> ScheduledTournaments = new List<ScheduledTournament>();
        public List<RecurringTournamentTemplate> RecurringTemplates = new List<RecurringTournamentTemplate>();
        public int TotalScheduled;
        public List<SchedulingConflict> ActiveConflicts = new List<SchedulingConflict>();
        public TournamentSchedulerMetrics SystemMetrics;
        public Dictionary<string, object> ScheduleStatistics = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class TournamentSchedulerMetrics
    {
        public int TotalTournamentsScheduled;
        public int SuccessfulTournaments;
        public int CancelledTournaments;
        public int ActiveScheduledTournaments;
        public float AverageParticipation;
        public int RecurringTemplates;
        public int ActiveConflicts;
        public int PreRegistrations;
        public int PendingNotifications;
        public DateTime LastUpdateTime;
        public Dictionary<ScheduledTournamentStatus, int> StatusDistribution = new Dictionary<ScheduledTournamentStatus, int>();
        public Dictionary<TournamentType, int> TypeDistribution = new Dictionary<TournamentType, int>();
        public float SystemLoad; // 0-1 indicating scheduling system load
    }
    
    // Time zone and scheduling utilities
    [Serializable]
    public class TimeZoneSettings
    {
        public string TimeZoneId;
        public string DisplayName;
        public TimeSpan UtcOffset;
        public bool ObservesDaylightSaving;
        public Dictionary<string, object> LocalizationData = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class SchedulingWindow
    {
        public TimeSpan StartTime;
        public TimeSpan EndTime;
        public List<DayOfWeek> AllowedDays = new List<DayOfWeek>();
        public List<DateTime> ExcludedDates = new List<DateTime>();
        public Dictionary<string, object> WindowConstraints = new Dictionary<string, object>();
    }
    
    // Tournament templates and presets
    [Serializable]
    public class TournamentTemplate
    {
        public string TemplateId;
        public string Name;
        public string Description;
        public TournamentConfiguration DefaultConfiguration;
        public TimeSpan DefaultDuration;
        public int RecommendedParticipants;
        public Dictionary<string, object> TemplateSettings = new Dictionary<string, object>();
        public bool IsPublic;
        public string CreatedBy;
        public DateTime CreationTime;
        public int UsageCount;
    }
    
    // Event calendar and visualization
    [Serializable]
    public class CalendarEvent
    {
        public string EventId;
        public string Title;
        public DateTime StartTime;
        public DateTime EndTime;
        public string EventType;
        public string Description;
        public string Color; // For UI display
        public bool IsAllDay;
        public bool IsRecurring;
        public Dictionary<string, object> DisplayData = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class CalendarView
    {
        public DateTime ViewStart;
        public DateTime ViewEnd;
        public string ViewType; // "Day", "Week", "Month", "Year"
        public List<CalendarEvent> Events = new List<CalendarEvent>();
        public Dictionary<DateTime, int> EventCounts = new Dictionary<DateTime, int>();
        public List<SchedulingConflict> VisibleConflicts = new List<SchedulingConflict>();
    }
    
    // Advanced scheduling features
    [Serializable]
    public class SchedulingRule
    {
        public string RuleId;
        public string Name;
        public string Description;
        public RuleType Type;
        public Dictionary<string, object> Conditions = new Dictionary<string, object>();
        public Dictionary<string, object> Actions = new Dictionary<string, object>();
        public bool IsActive;
        public int Priority;
        public DateTime CreatedTime;
    }
    
    public enum RuleType
    {
        PreventOverlap,
        RequireMinimumGap,
        LimitConcurrent,
        EnforceTimeWindows,
        AutoCancel,
        AutoReschedule,
        NotificationTrigger,
        Custom
    }
    
    [Serializable]
    public class SchedulingPolicy
    {
        public string PolicyId;
        public string Name;
        public List<SchedulingRule> Rules = new List<SchedulingRule>();
        public Dictionary<string, object> GlobalSettings = new Dictionary<string, object>();
        public bool IsActive;
        public string AppliesTo; // "All", "TournamentType", "Region", etc.
        public DateTime EffectiveDate;
        public DateTime? ExpirationDate;
    }
    
    // Integration with external systems
    [Serializable]
    public class ExternalCalendarSync
    {
        public string SyncId;
        public string ExternalCalendarId;
        public string CalendarProvider; // "Google", "Outlook", "Apple", etc.
        public DateTime LastSyncTime;
        public bool IsActive;
        public SyncDirection SyncDirection;
        public Dictionary<string, object> SyncSettings = new Dictionary<string, object>();
        public List<string> SyncErrors = new List<string>();
    }
    
    public enum SyncDirection
    {
        Import,
        Export,
        Bidirectional
    }
    
    // Waitlist and overflow management
    [Serializable]
    public class TournamentWaitlist
    {
        public string EventId;
        public List<PlayerRegistration> WaitingPlayers = new List<PlayerRegistration>();
        public int MaxWaitlistSize;
        public bool AutoPromote;
        public DateTime LastProcessed;
        public Dictionary<string, object> WaitlistSettings = new Dictionary<string, object>();
    }
    
    // Performance and optimization
    [Serializable]
    public class SchedulingPerformanceMetrics
    {
        public TimeSpan AverageSchedulingTime;
        public TimeSpan AverageConflictResolutionTime;
        public int SchedulingOperationsPerSecond;
        public float MemoryUsage;
        public int CacheHitRate;
        public DateTime LastMeasurement;
        public Dictionary<string, float> DetailedMetrics = new Dictionary<string, float>();
    }
}