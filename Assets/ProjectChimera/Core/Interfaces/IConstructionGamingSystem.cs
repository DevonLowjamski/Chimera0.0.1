using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Interface for the Enhanced Construction Gaming System
    /// Defines core contracts for strategic construction gaming features
    /// </summary>
    public interface IConstructionGamingSystem
    {
        /// <summary>
        /// Start a new construction challenge with specified parameters
        /// </summary>
        /// <param name="challengeId">Unique identifier for the challenge</param>
        /// <returns>True if challenge started successfully</returns>
        bool StartConstructionChallenge(string challengeId);
        
        /// <summary>
        /// Complete a construction challenge with solution data
        /// </summary>
        /// <param name="challengeId">Challenge identifier</param>
        /// <param name="solutionData">Solution data for evaluation</param>
        /// <returns>True if challenge completed successfully</returns>
        bool CompleteConstructionChallenge(string challengeId, object solutionData);
        
        /// <summary>
        /// Enable collaborative mode for multi-player construction
        /// </summary>
        /// <param name="sessionId">Collaboration session identifier</param>
        /// <returns>True if collaborative mode enabled successfully</returns>
        bool EnableCollaborativeMode(string sessionId);
        
        /// <summary>
        /// Process a generic gaming action
        /// </summary>
        /// <param name="actionId">Action identifier</param>
        /// <param name="actionData">Action-specific data</param>
        /// <returns>True if action processed successfully</returns>
        bool ProcessGamingAction(string actionId, object actionData);
        
        /// <summary>
        /// Check if construction gaming features are currently enabled
        /// </summary>
        bool IsConstructionGamingEnabled { get; }
        
        /// <summary>
        /// Check if collaborative building features are enabled
        /// </summary>
        bool IsCollaborativeBuildingEnabled { get; }
        
        /// <summary>
        /// Check if architectural challenges are enabled
        /// </summary>
        bool IsArchitecturalChallengesEnabled { get; }
        
        /// <summary>
        /// Get the current number of active challenges
        /// </summary>
        int ActiveChallengesCount { get; }
        
        /// <summary>
        /// Get the current number of active collaborations
        /// </summary>
        int ActiveCollaborationsCount { get; }
        
        /// <summary>
        /// Event triggered when a challenge is started
        /// </summary>
        event Action<object> OnChallengeStarted;
        
        /// <summary>
        /// Event triggered when a challenge is completed
        /// </summary>
        event Action<object, object> OnChallengeCompleted;
        
        /// <summary>
        /// Event triggered when an architectural breakthrough is discovered
        /// </summary>
        event Action<object> OnArchitecturalBreakthrough;
        
        /// <summary>
        /// Event triggered when a collaborative session starts
        /// </summary>
        event Action<object> OnCollaborationStarted;
        
        /// <summary>
        /// Event triggered when a competition is won
        /// </summary>
        event Action<object> OnCompetitionWon;
        
        /// <summary>
        /// Event triggered when a certification is earned
        /// </summary>
        event Action<object> OnCertificationEarned;
        
        /// <summary>
        /// Event triggered when an innovation is unlocked
        /// </summary>
        event Action<object> OnInnovationUnlocked;
    }
    
    
    /// <summary>
    /// Interface for construction project management
    /// </summary>
    public interface IConstructionProject
    {
        string ProjectId { get; }
        string ProjectName { get; }
        ProjectType Type { get; }
        ComplexityLevel Complexity { get; }
        ConstructionPhase CurrentPhase { get; }
        
        object ProjectBlueprint { get; }
        object Resources { get; }
        object Schedule { get; }
        object Standards { get; }
        
        void StartConstruction(object parameters);
        void UpdateConstruction(float deltaTime);
        void ProcessBuildingAction(object action);
        void HandleConstructionEvent(object constructionEvent);
        void CompleteConstruction(object criteria);
    }
    
    /// <summary>
    /// Interface for collaborative construction features
    /// </summary>
    public interface ICollaborativeConstruction
    {
        /// <summary>
        /// Start a collaborative construction session
        /// </summary>
        string StartCollaborativeSession(CollaborativeProjectConfig config);
        
        /// <summary>
        /// Add a participant to the collaboration
        /// </summary>
        bool AddParticipant(string sessionId, ParticipantInfo participant);
        
        /// <summary>
        /// Remove a participant from the collaboration
        /// </summary>
        bool RemoveParticipant(string sessionId, string participantId);
        
        /// <summary>
        /// Process a collaborative action
        /// </summary>
        bool ProcessCollaborativeAction(string sessionId, CollaborativeAction action);
        
        /// <summary>
        /// Get current session state
        /// </summary>
        CollaborativeSessionState GetSessionState(string sessionId);
        
        /// <summary>
        /// End a collaborative session
        /// </summary>
        bool EndCollaborativeSession(string sessionId, SessionEndReason reason);
        
        /// <summary>
        /// Check if a session is active
        /// </summary>
        bool IsSessionActive(string sessionId);
        
        /// <summary>
        /// Get list of active sessions
        /// </summary>
        List<string> GetActiveSessions();
    }
    
    /// <summary>
    /// Interface for architectural challenge management
    /// </summary>
    public interface IArchitecturalChallengeManager
    {
        /// <summary>
        /// Generate a new challenge with specified parameters
        /// </summary>
        object GenerateChallenge(ChallengeType type, DifficultyLevel difficulty, object parameters);
        
        /// <summary>
        /// Evaluate a solution for a challenge
        /// </summary>
        object EvaluateChallengeSolution(object challenge, object solution);
        
        /// <summary>
        /// Get available challenges for a player
        /// </summary>
        List<object> GetAvailableChallenges(string playerId);
        
        /// <summary>
        /// Submit a solution for evaluation
        /// </summary>
        bool SubmitChallengeSolution(string challengeId, object solution);
        
        /// <summary>
        /// Check if a player can access a challenge
        /// </summary>
        bool CanAccessChallenge(string playerId, string challengeId);
    }

    // Enums that are specific to construction interfaces (not data structures)
    public enum ComplexityLevel
    {
        Simple,
        Moderate,
        Complex,
        Advanced,
        Expert
    }
    
    public enum ProjectType
    {
        Residential,
        Commercial,
        Industrial,
        Agricultural,
        Research,
        Mixed
    }
    
    public enum ChallengeType
    {
        TimeTrial,
        Budget,
        Quality,
        Efficiency,
        Innovation,
        Safety
    }
    
    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard,
        Expert
    }
    
    public enum ConstructionPhase
    {
        Planning,
        Foundation,
        Structure,
        Systems,
        Finishing,
        Inspection,
        Completed
    }
    
    public enum SessionEndReason
    {
        Completed,
        Cancelled,
        Timeout,
        Error,
        PlayerLeft
    }
    
    public enum CertificationGrade
    {
        Fail,
        Pass,
        Merit,
        Distinction
    }
    
    public enum ActionType
    {
        PlaceComponent,
        ModifyComponent,
        RemoveComponent,
        ConfigureSystem,
        ValidateDesign,
        ShareBlueprint,
        RequestReview,
        ProvideReview,
        SubmitSolution,
        VoteOnSolution,
        DiscussDesign,
        RequestHelp,
        OfferHelp,
        InviteCollab,
        CreateVariant,
        SetPermissions,
        ScheduleMeeting,
        DocumentDecision,
        RecordLearning,
        UpdateProgress,
        FinalizeDesign
    }
    
    public enum CollaborationRole
    {
        Lead,
        Architect,
        Engineer,
        Designer,
        Reviewer,
        Contributor,
        Observer
    }

    // Supporting classes that are specific to interface contracts (minimal implementations)
    [Serializable]
    public class CollaborativeProjectConfig
    {
        public string ProjectName;
        public string Description;
        public List<ParticipantInfo> Participants = new List<ParticipantInfo>();
        public List<RoleDefinition> RoleDefinitions = new List<RoleDefinition>();
        public ResourcePool ResourcePool;
        public CollaborationSettings Settings;
    }
    
    [Serializable]
    public class CollaborativeAction
    {
        public string ActionId;
        public ActionType ActionType;
        public string PlayerId;
        public object ActionData;
        public DateTime Timestamp;
    }
    
    [Serializable]
    public class ParticipantInfo
    {
        public string PlayerId;
        public string PlayerName;
        public CollaborationRole Role;
        public List<string> Permissions = new List<string>();
        public DateTime JoinTime;
        public bool IsActive;
    }
    
    // ChallengeParameters is defined in ProjectChimera.Data.Construction
    // This interface uses object parameters to avoid circular dependencies

    // Placeholder classes for interface support - actual implementations should be in Data assemblies
    [Serializable] public class ResourceRequirements { }
    [Serializable] public class TimelineSchedule { }
    [Serializable] public class QualityStandards { }
    [Serializable] public class ConstructionParameters { }
    [Serializable] public class BuildingAction { }
    [Serializable] public class ConstructionEvent { }
    [Serializable] public class CompletionCriteria { }
    [Serializable] public class CollaborativeSessionState { }
    [Serializable] public class RoleDefinition { }
    [Serializable] public class ResourcePool { }
    [Serializable] public class CollaborationSettings { }
    
    // All data structures for construction challenges are now defined in ProjectChimera.Data.Construction
    // This interface uses those types via the using directive above
}