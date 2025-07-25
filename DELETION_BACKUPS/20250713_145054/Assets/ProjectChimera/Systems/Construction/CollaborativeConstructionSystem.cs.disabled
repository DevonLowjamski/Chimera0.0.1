using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Construction;
// Type alias to resolve ParticipantInfo conflict
using DataParticipantInfo = ProjectChimera.Data.Construction.ParticipantInfo;
using DataProjectType = ProjectChimera.Data.Construction.ProjectType;
// Explicit namespace aliases to resolve ambiguous references
using CollaborativeAction = ProjectChimera.Data.Construction.CollaborativeAction;
using ResourcePool = ProjectChimera.Data.Construction.ResourcePool;
using CollaborationSettings = ProjectChimera.Data.Construction.CollaborationSettings;
using ConstructionEventType = ProjectChimera.Data.Construction.EventType;
using ConstructionDifficultyLevel = ProjectChimera.Data.Construction.DifficultyLevel;
using CollaborativeProjectConfig = ProjectChimera.Data.Construction.CollaborativeProjectConfig;
using ConstructionActionType = ProjectChimera.Data.Construction.ActionType;
using ConstructionParticipantRole = ProjectChimera.Data.Construction.ParticipantRole;

namespace ProjectChimera.Systems.Construction
{
    /// <summary>
    /// Collaborative Construction System - Enables multi-player architectural design and construction
    /// 
    /// This system facilitates real-time collaborative building experiences where multiple players
    /// can work together on architectural projects, share resources, resolve conflicts, and learn
    /// from each other in a structured and engaging environment.
    /// </summary>
    public class CollaborativeConstructionSystem
    {
        // Core Systems
        private CollaborationServer _collaborationServer;
        private RealTimeSynchronization _syncSystem;
        private ConflictResolution _conflictResolver;
        private PermissionManagement _permissionManager;
        
        // Session Management
        private Dictionary<string, CollaborativeSession> _activeSessions = new Dictionary<string, CollaborativeSession>();
        private Dictionary<string, SessionParticipant> _onlineParticipants = new Dictionary<string, SessionParticipant>();
        
        // Communication Systems
        private CommunicationManager _communicationManager;
        private SharedResourceManager _resourceManager;
        private CollaborationAnalytics _analyticsSystem;
        
        // Configuration
        private int _maxCollaborators = 20;
        private float _syncInterval = 0.1f;
        private bool _enableRealTimeSync = true;
        
        public void Initialize()
        {
            InitializeCollaborationInfrastructure();
            InitializeCommunicationSystems();
            InitializeResourceManagement();
            InitializeAnalytics();
            
            Debug.Log("Collaborative Construction System initialized successfully");
        }
        
        public void SetMaxCollaborators(int maxCollaborators)
        {
            _maxCollaborators = Mathf.Clamp(maxCollaborators, 2, 50);
        }
        
        public void SetSyncInterval(float interval)
        {
            _syncInterval = Mathf.Clamp(interval, 0.01f, 1.0f);
        }
        
        /// <summary>
        /// Start a new collaborative construction project
        /// </summary>
        public CollaborativeSession StartCollaborativeProject(CollaborativeProjectConfig config)
        {
            string sessionId = Guid.NewGuid().ToString();
            
            var session = new CollaborativeSession
            {
                SessionId = sessionId,
                SessionName = config.ProjectName,
                Description = config.Description,
                Participants = new List<SessionParticipant>(),
                Project = CreateCollaborativeProject(config),
                StartTime = DateTime.Now,
                Status = SessionStatus.Waiting,
                Settings = config.Settings ?? CreateDefaultSettings(),
                SharedResources = InitializeSharedResources(config.ResourcePool),
                EventHistory = new List<CollaborationEvent>(),
                SessionData = new Dictionary<string, object>(),
                Communication = InitializeCommunicationChannels(config.Settings),
                ConflictResolution = InitializeConflictResolution(config.Settings)
            };
            
            // Add initial participants
            foreach (var participantInfo in config.Participants)
            {
                // Convert from Data.ParticipantInfo to Systems.ParticipantInfo
                var systemParticipantInfo = new ParticipantInfo
                {
                    PlayerId = participantInfo.PlayerId,
                    PlayerName = participantInfo.PlayerName,
                    Role = participantInfo.Role,
                    SkillLevel = participantInfo.SkillLevel,
                    Specializations = participantInfo.Specializations
                };
                AddParticipantToSession(session, systemParticipantInfo);
            }
            
            // Initialize collaboration server for this session
            _collaborationServer.CreateSession(session);
            
            _activeSessions[sessionId] = session;
            
            Debug.Log($"Started collaborative session: {config.ProjectName} with {config.Participants.Count} participants");
            return session;
        }
        
        /// <summary>
        /// Add a participant to an existing collaborative session
        /// </summary>
        public bool AddParticipant(string sessionId, ParticipantInfo participantInfo)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                Debug.LogWarning($"Session not found: {sessionId}");
                return false;
            }
            
            if (session.Participants.Count >= _maxCollaborators)
            {
                Debug.LogWarning($"Session at maximum capacity: {sessionId}");
                return false;
            }
            
            return AddParticipantToSession(session, participantInfo);
        }
        
        /// <summary>
        /// Remove a participant from a collaborative session
        /// </summary>
        public bool RemoveParticipant(string sessionId, string participantId)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                Debug.LogWarning($"Session not found: {sessionId}");
                return false;
            }
            
            var participant = session.Participants.FirstOrDefault(p => p.ParticipantId == participantId);
            if (participant == null)
            {
                Debug.LogWarning($"Participant not found: {participantId}");
                return false;
            }
            
            // Record leave time
            participant.LeaveTime = DateTime.Now;
            participant.Status = ParticipantStatus.Offline;
            
            // Remove from online participants
            _onlineParticipants.Remove(participantId);
            
            // Log event
            LogCollaborationEvent(session, participantId, ProjectChimera.Data.Construction.EventType.Leave, "Participant left the session");
            
            // Check if session should be paused or terminated
            CheckSessionStatus(session);
            
            Debug.Log($"Participant {participantId} removed from session {sessionId}");
            return true;
        }
        
        /// <summary>
        /// Process a collaborative action from a participant
        /// </summary>
        public void ProcessCollaborativeAction(string playerId, CollaborativeAction action)
        {
            var participant = _onlineParticipants.GetValueOrDefault(playerId);
            if (participant == null)
            {
                Debug.LogWarning($"Participant not found or offline: {playerId}");
                return;
            }
            
            var session = _activeSessions.Values.FirstOrDefault(s => 
                s.Participants.Any(p => p.PlayerId == playerId));
            
            if (session == null)
            {
                Debug.LogWarning($"No active session found for player: {playerId}");
                return;
            }
            
            // Validate permissions
            if (!ValidateActionPermissions(session, participant, action))
            {
                Debug.LogWarning($"Action not permitted: {action.ActionType} by {playerId}");
                return;
            }
            
            // Check for conflicts
            var conflicts = _conflictResolver.CheckForConflicts(session, action);
            if (conflicts.Count > 0)
            {
                HandleConflicts(session, action, conflicts);
                return;
            }
            
            // Apply the action
            ApplyCollaborativeAction(session, participant, action);
            
            // Synchronize with other participants
            if (_enableRealTimeSync)
            {
                _syncSystem.SynchronizeAction(session, action);
            }
            
            // Update metrics
            UpdateParticipantMetrics(participant, action);
            
            // Log event
            LogCollaborationEvent(session, playerId, ProjectChimera.Data.Construction.EventType.Action, $"Performed {action.ActionType}");
        }
        
        /// <summary>
        /// Update a collaborative session (called each frame)
        /// </summary>
        public void UpdateSession(CollaborativeSession session)
        {
            if (session.Status != SessionStatus.Active) return;
            
            // Update synchronization
            if (_enableRealTimeSync)
            {
                _syncSystem.UpdateSynchronization(session);
            }
            
            // Update communication systems
            _communicationManager.UpdateCommunication(session);
            
            // Process pending conflicts
            _conflictResolver.ProcessPendingConflicts(session);
            
            // Update shared resources
            _resourceManager.UpdateSharedResources(session);
            
            // Update analytics
            _analyticsSystem.UpdateSessionAnalytics(session);
            
            // Check for inactive participants
            CheckParticipantActivity(session);
        }
        
        /// <summary>
        /// End a collaborative session
        /// </summary>
        public void EndCollaborativeSession(string sessionId)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                Debug.LogWarning($"Session not found: {sessionId}");
                return;
            }
            
            session.Status = SessionStatus.Completed;
            session.EndTime = DateTime.Now;
            
            // Notify all participants
            foreach (var participant in session.Participants.Where(p => p.Status == ParticipantStatus.Online))
            {
                NotifyParticipant(participant, "Session has ended");
            }
            
            // Generate session summary
            GenerateSessionSummary(session);
            
            // Cleanup
            _collaborationServer.EndSession(session);
            _activeSessions.Remove(sessionId);
            
            Debug.Log($"Collaborative session ended: {sessionId}");
        }
        
        public void Shutdown()
        {
            // End all active sessions
            foreach (var session in _activeSessions.Values.ToList())
            {
                EndCollaborativeSession(session.SessionId);
            }
            
            _collaborationServer?.Shutdown();
            Debug.Log("Collaborative Construction System shut down");
        }
        
        #region Private Implementation
        
        private void InitializeCollaborationInfrastructure()
        {
            _collaborationServer = new CollaborationServer();
            _syncSystem = new RealTimeSynchronization();
            _conflictResolver = new ConflictResolution();
            _permissionManager = new PermissionManagement();
            
            _collaborationServer.Initialize();
            _syncSystem.Initialize();
        }
        
        private void InitializeCommunicationSystems()
        {
            _communicationManager = new CommunicationManager();
            _communicationManager.Initialize();
        }
        
        private void InitializeResourceManagement()
        {
            _resourceManager = new SharedResourceManager();
            _resourceManager.Initialize();
        }
        
        private void InitializeAnalytics()
        {
            _analyticsSystem = new CollaborationAnalytics();
            _analyticsSystem.Initialize();
        }
        
        private ConstructionProject CreateCollaborativeProject(CollaborativeProjectConfig config)
        {
            return new ConstructionProject
            {
                ProjectId = Guid.NewGuid().ToString(),
                ProjectName = config.ProjectName,
                ProjectType = DataProjectType.Mixed, // Default for collaborative projects
                GamingFeatures = new ConstructionGamingFeatures
                {
                    EnableCollaboration = true,
                    EnableChallenges = true,
                    EnableEducation = true,
                    DifficultyLevel = ProjectChimera.Data.Construction.DifficultyLevel.Medium
                },
                CreationTime = DateTime.Now,
                Status = ProjectStatus.Planning,
                Participants = config.Participants.Select(p => p.PlayerId).ToList()
            };
        }
        
        private CollaborationSettings CreateDefaultSettings()
        {
            return new CollaborationSettings
            {
                MaxParticipants = _maxCollaborators,
                AllowSpectators = true,
                EnableVoiceChat = true,
                EnableTextChat = true,
                EnableScreenSharing = false,
                RequirePermissionForChanges = true,
                EnableRealTimeSync = _enableRealTimeSync,
                SyncInterval = _syncInterval,
                ConflictResolution = ConflictResolutionMode.Voting
            };
        }
        
        private SharedResources InitializeSharedResources(ResourcePool resourcePool)
        {
            return new SharedResources
            {
                Budget = new SharedBudget(),
                Materials = new SharedMaterialPool(),
                Equipment = new SharedEquipmentPool(),
                Knowledge = new SharedKnowledgeBase(),
                Blueprints = new SharedBlueprintLibrary()
            };
        }
        
        private CommunicationChannels InitializeCommunicationChannels(CollaborationSettings settings)
        {
            return new CommunicationChannels
            {
                VoiceChatEnabled = settings?.EnableVoiceChat ?? true,
                TextChatEnabled = settings?.EnableTextChat ?? true,
                VideoEnabled = false,
                ChatHistory = new List<ChatMessage>(),
                VoiceSessions = new List<VoiceSession>(),
                ScreenSharing = null
            };
        }
        
        private ConflictResolutionSystem InitializeConflictResolution(CollaborationSettings settings)
        {
            return new ConflictResolutionSystem
            {
                Mode = settings?.ConflictResolution ?? ConflictResolutionMode.Voting,
                ActiveConflicts = new List<ActiveConflict>(),
                ResolvedConflicts = new List<ResolvedConflict>(),
                Rules = new ConflictResolutionRules()
            };
        }
        
        private bool AddParticipantToSession(CollaborativeSession session, ParticipantInfo participantInfo)
        {
            var determinedRole = DetermineParticipantRole(session, participantInfo);
            var participant = new SessionParticipant
            {
                ParticipantId = Guid.NewGuid().ToString(),
                PlayerId = participantInfo.PlayerId,
                PlayerName = participantInfo.PlayerName,
                Role = determinedRole,
                Permissions = GeneratePermissions(determinedRole),
                JoinTime = DateTime.Now,
                Status = ParticipantStatus.Online,
                Metrics = new CollaborationMetrics(),
                Contributions = new List<string>()
            };
            
            session.Participants.Add(participant);
            _onlineParticipants[participant.ParticipantId] = participant;
            
            // Log event
            LogCollaborationEvent(session, participant.PlayerId, ConstructionEventType.Join, "Participant joined the session");
            
            // Start session if this was the last expected participant
            if (session.Status == SessionStatus.Waiting && 
                session.Participants.Count >= 2) // Minimum for collaboration
            {
                session.Status = SessionStatus.Active;
            }
            
            return true;
        }
        
        private ConstructionParticipantRole DetermineParticipantRole(CollaborativeSession session, ParticipantInfo participantInfo)
        {
            // First participant becomes lead
            if (session.Participants.Count == 0)
            {
                return ConstructionParticipantRole.Lead;
            }
            
            // Use provided role or default to designer
            return participantInfo.Role ?? ConstructionParticipantRole.Designer;
        }
        
        private List<Permission> GeneratePermissions(ConstructionParticipantRole role)
        {
            var permissions = new List<Permission>();
            
            switch (role)
            {
                case ConstructionParticipantRole.Lead:
                    // Lead has all permissions
                    permissions.Add(new Permission { Name = "FullAccess", Allowed = true });
                    break;
                    
                case ConstructionParticipantRole.Architect:
                case ConstructionParticipantRole.Engineer:
                case ConstructionParticipantRole.Designer:
                    permissions.Add(new Permission { Name = "Edit", Allowed = true });
                    permissions.Add(new Permission { Name = "Comment", Allowed = true });
                    permissions.Add(new Permission { Name = "Share", Allowed = true });
                    break;
                    
                case ConstructionParticipantRole.Reviewer:
                    permissions.Add(new Permission { Name = "Comment", Allowed = true });
                    permissions.Add(new Permission { Name = "Suggest", Allowed = true });
                    break;
                    
                case ConstructionParticipantRole.Observer:
                    permissions.Add(new Permission { Name = "View", Allowed = true });
                    break;
            }
            
            return permissions;
        }
        
        private bool ValidateActionPermissions(CollaborativeSession session, SessionParticipant participant, CollaborativeAction action)
        {
            return _permissionManager.ValidatePermission(participant, action);
        }
        
        private void HandleConflicts(CollaborativeSession session, CollaborativeAction action, List<object> conflicts)
        {
            foreach (var conflict in conflicts)
            {
                _conflictResolver.AddConflict(session, action, conflict);
            }
        }
        
        private void ApplyCollaborativeAction(CollaborativeSession session, SessionParticipant participant, CollaborativeAction action)
        {
            // Apply the action to the session data
            switch (action.ActionType)
            {
                case ConstructionActionType.PlaceComponent:
                    ApplyPlaceComponentAction(session, action);
                    break;
                    
                case ConstructionActionType.ModifyComponent:
                    ApplyModifyComponentAction(session, action);
                    break;
                    
                case ConstructionActionType.DeleteComponent:
                    ApplyDeleteComponentAction(session, action);
                    break;
                    
                case ConstructionActionType.AddComment:
                    ApplyAddCommentAction(session, action);
                    break;
                    
                case ConstructionActionType.ShareResource:
                    ApplyShareResourceAction(session, action);
                    break;
                    
                default:
                    Debug.LogWarning($"Unknown action type: {action.ActionType}");
                    break;
            }
            
            // Record contribution
            participant.Contributions.Add($"{action.ActionType} at {DateTime.Now}");
        }
        
        private void UpdateParticipantMetrics(SessionParticipant participant, CollaborativeAction action)
        {
            participant.Metrics.TotalActions++;
            participant.Metrics.AcceptedActions++;
            participant.Metrics.ContributionScore += CalculateContributionScore(action);
        }
        
        private void LogCollaborationEvent(CollaborativeSession session, string participantId, ProjectChimera.Data.Construction.EventType eventType, string description)
        {
            var collaborationEvent = new CollaborationEvent
            {
                EventId = Guid.NewGuid().ToString(),
                ParticipantId = participantId,
                EventType = eventType,
                Timestamp = DateTime.Now,
                Description = description,
                AffectedParticipants = new List<string>()
            };
            
            session.EventHistory.Add(collaborationEvent);
        }
        
        private void CheckSessionStatus(CollaborativeSession session)
        {
            var activeParticipants = session.Participants.Count(p => p.Status == ParticipantStatus.Online);
            
            if (activeParticipants < 2)
            {
                session.Status = SessionStatus.Paused;
            }
        }
        
        private void CheckParticipantActivity(CollaborativeSession session)
        {
            var inactiveThreshold = TimeSpan.FromMinutes(5);
            var now = DateTime.Now;
            
            foreach (var participant in session.Participants.Where(p => p.Status == ParticipantStatus.Online))
            {
                // Check if participant has been inactive
                // This would require tracking last activity time
                // For now, just mark as away if no recent activity
            }
        }
        
        private void NotifyParticipant(SessionParticipant participant, string message)
        {
            // Send notification to participant
            Debug.Log($"Notification to {participant.PlayerName}: {message}");
        }
        
        private void GenerateSessionSummary(CollaborativeSession session)
        {
            var duration = session.EndTime.Value - session.StartTime;
            var totalActions = session.EventHistory.Count(e => e.EventType == ProjectChimera.Data.Construction.EventType.Action);
            var participantCount = session.Participants.Count;
            
            Debug.Log($"Session Summary - Duration: {duration.TotalMinutes:F1} minutes, " +
                     $"Participants: {participantCount}, Actions: {totalActions}");
        }
        
        // Action implementation methods
        private void ApplyPlaceComponentAction(CollaborativeSession session, CollaborativeAction action) { }
        private void ApplyModifyComponentAction(CollaborativeSession session, CollaborativeAction action) { }
        private void ApplyDeleteComponentAction(CollaborativeSession session, CollaborativeAction action) { }
        private void ApplyAddCommentAction(CollaborativeSession session, CollaborativeAction action) { }
        private void ApplyShareResourceAction(CollaborativeSession session, CollaborativeAction action) { }
        
        private float CalculateContributionScore(CollaborativeAction action)
        {
            return action.ActionType switch
            {
                ConstructionActionType.PlaceComponent => 10f,
                ConstructionActionType.ModifyComponent => 8f,
                ConstructionActionType.DeleteComponent => 5f,
                ConstructionActionType.AddComment => 3f,
                ConstructionActionType.ShareResource => 7f,
                _ => 1f
            };
        }
        
        #endregion
    }
    
    #region Supporting Classes
    
    public class CollaborationServer
    {
        public void Initialize() { }
        public void CreateSession(CollaborativeSession session) { }
        public void EndSession(CollaborativeSession session) { }
        public void Shutdown() { }
    }
    
    public class RealTimeSynchronization
    {
        public void Initialize() { }
        public void SynchronizeAction(CollaborativeSession session, CollaborativeAction action) { }
        public void UpdateSynchronization(CollaborativeSession session) { }
    }
    
    public class ConflictResolution
    {
        public List<object> CheckForConflicts(CollaborativeSession session, CollaborativeAction action) => new List<object>();
        public void AddConflict(CollaborativeSession session, CollaborativeAction action, object conflict) { }
        public void ProcessPendingConflicts(CollaborativeSession session) { }
    }
    
    public class PermissionManagement
    {
        public bool ValidatePermission(SessionParticipant participant, CollaborativeAction action) => true;
    }
    
    public class CommunicationManager
    {
        public void Initialize() { }
        public void UpdateCommunication(CollaborativeSession session) { }
    }
    
    public class SharedResourceManager
    {
        public void Initialize() { }
        public void UpdateSharedResources(CollaborativeSession session) { }
    }
    
    public class CollaborationAnalytics
    {
        public void Initialize() { }
        public void UpdateSessionAnalytics(CollaborativeSession session) { }
    }
    
    public class ParticipantInfo
    {
        public string PlayerId;
        public string PlayerName;
        public ConstructionParticipantRole? Role;
        public SkillLevel SkillLevel;
        public List<string> Specializations;
    }
    
    #endregion
}