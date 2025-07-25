using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core.Events;
using ProjectChimera.Data;
using ProjectChimera.Data.Events;

namespace ProjectChimera.Systems.Events
{
    using ILiveEventDefinition = ProjectChimera.Core.Events.ILiveEventDefinition;
    using CrisisType = ProjectChimera.Data.Events.CrisisType;
    using UrgencyLevel = ProjectChimera.Data.Events.UrgencyLevel;

    /// <summary>
    /// Implementation of crisis response events with emergency response and community mobilization features.
    /// </summary>
    public class CrisisResponseEvent : CommunityEventBase, ICrisisResponseEvent, IEducationalEvent
    {
        // ICrisisResponseEvent Implementation
        public CrisisType CrisisType { get; private set; }
        public UrgencyLevel UrgencyLevel { get; private set; }
        public CrisisContext CrisisContext { get; private set; }
        public EmergencyResponse EmergencyResponse { get; private set; }
        public bool IsEmergencyActive { get; private set; }

        // IEducationalEvent Implementation
        public List<LearningObjective> LearningObjectives { get; private set; }
        public EducationalContent EducationalMaterials { get; private set; }
        public bool RequiresScientificAccuracy { get; private set; }
        public bool IsScientificallyValidated { get; private set; }

        // Crisis response specific properties
        private Dictionary<string, EducationalProgress> _learningProgress;
        private List<EmergencyAction> _activeActions;
        private CrisisEventTemplateSO _crisisEventData;
        private DateTime _crisisStartTime;
        private TimeSpan _responseTime;

        public CrisisResponseEvent(ILiveEventDefinition definition) : base(definition, CreateCrisisCommunityGoal(definition))
        {
            
            // Initialize crisis context
            if (definition is CrisisEventTemplateSO crisisSO)
            {
                _crisisEventData = crisisSO;
                CrisisType = crisisSO.CrisisType;
                UrgencyLevel = crisisSO.UrgencyLevel;
            }
            else
            {
                CrisisType = CrisisType.CommunityNeed;
                UrgencyLevel = UrgencyLevel.Medium;
            }

            InitializeCrisisContext();
            InitializeEmergencyResponse();
            InitializeEducationalContent();
            
            _learningProgress = new Dictionary<string, EducationalProgress>();
            _activeActions = new List<EmergencyAction>();
            _crisisStartTime = DateTime.Now;
            IsEmergencyActive = true;

            // Adjust event urgency based on crisis type
            AdjustEventUrgency();
        }

        private static CommunityGoal CreateCrisisCommunityGoal(ILiveEventDefinition definition)
        {
            return new CommunityGoal
            {
                GoalId = $"crisis_response_{definition.EventId}",
                GoalType = "Crisis Response",
                TargetAmount = 100f, // 100% crisis resolution
                Description = $"Community response to {definition.EventName}",
                TimeLimit = definition.Duration
            };
        }

        private void InitializeCrisisContext()
        {
            CrisisContext = new CrisisContext
            {
                CrisisType = CrisisType,
                UrgencyLevel = UrgencyLevel,
                Description = Description,
                CrisisStartTime = _crisisStartTime,
                ExpectedDuration = Duration
            };
        }

        private void InitializeEmergencyResponse()
        {
            var responseActions = new List<string>();
            var responseResources = new Dictionary<string, object>();

            // Add crisis-specific response actions
            switch (CrisisType)
            {
                case CrisisType.EnvironmentalDisaster:
                    responseActions.AddRange(new[] { "environmental_assessment", "damage_control", "recovery_planning" });
                    responseResources["emergency_supplies"] = 100;
                    responseResources["environmental_experts"] = 5;
                    break;
                case CrisisType.LegalChanges:
                    responseActions.AddRange(new[] { "legal_analysis", "compliance_planning", "community_education" });
                    responseResources["legal_advisors"] = 3;
                    responseResources["documentation"] = 50;
                    break;
                case CrisisType.CommunityNeed:
                    responseActions.AddRange(new[] { "needs_assessment", "resource_mobilization", "community_support" });
                    responseResources["volunteers"] = 20;
                    responseResources["support_materials"] = 200;
                    break;
                case CrisisType.TechnicalChallenge:
                    responseActions.AddRange(new[] { "technical_analysis", "solution_development", "implementation" });
                    responseResources["technical_experts"] = 8;
                    responseResources["research_tools"] = 15;
                    break;
            }

            EmergencyResponse = new EmergencyResponse
            {
                ResponseId = $"emergency_response_{EventId}",
                ResponseActions = responseActions,
                ResponseResources = responseResources,
                IsActive = true
            };
        }

        private void InitializeEducationalContent()
        {
            LearningObjectives = new List<LearningObjective>
            {
                new LearningObjective
                {
                    ObjectiveId = $"crisis_education_{CrisisType}",
                    Description = $"Learn about {CrisisType} response strategies",
                    CompletionThreshold = 0.8f,
                    IsRequired = true
                },
                new LearningObjective
                {
                    ObjectiveId = $"emergency_preparedness",
                    Description = "Understand emergency preparedness protocols",
                    CompletionThreshold = 0.7f,
                    IsRequired = false
                }
            };

            RequiresScientificAccuracy = true;
            IsScientificallyValidated = true;

            EducationalMaterials = new EducationalContent
            {
                ContentId = $"crisis_education_{EventId}",
                Title = $"Crisis Response: {CrisisType}",
                Description = $"Educational materials for responding to {CrisisType} situations",
                LearningObjectives = LearningObjectives,
                ResourceLinks = new List<string>(),
                QualityScore = 1.0f,
                IsScientificallyValidated = true
            };
        }

        private void AdjustEventUrgency()
        {
            // Adjust max participants and duration based on urgency
            switch (UrgencyLevel)
            {
                case UrgencyLevel.Critical:
                case UrgencyLevel.Emergency:
                    MaxParticipants = 2000; // Allow more participants for critical situations
                    break;
                case UrgencyLevel.High:
                    MaxParticipants = 1500;
                    break;
                case UrgencyLevel.Medium:
                    MaxParticipants = 1000;
                    break;
                case UrgencyLevel.Low:
                    MaxParticipants = 500;
                    break;
            }
        }

        // ICrisisResponseEvent Implementation
        public void ActivateEmergencyResponse()
        {
            if (!IsEmergencyActive)
            {
                IsEmergencyActive = true;
                EmergencyResponse.IsActive = true;
                _responseTime = DateTime.Now - _crisisStartTime;
                
                Debug.Log($"[CrisisResponseEvent] Emergency response activated for {CrisisType} after {_responseTime.TotalMinutes:F1} minutes");
                
                // Notify community of emergency activation
                MobilizeCommunityResponse();
            }
        }

        public void MobilizeCommunityResponse()
        {
            Debug.Log($"[CrisisResponseEvent] Mobilizing community response for {CrisisType} crisis");
            
            // Create emergency actions for each response action
            foreach (var actionName in EmergencyResponse.ResponseActions)
            {
                var emergencyAction = new EmergencyAction
                {
                    ActionId = $"{actionName}_{EventId}",
                    ActionName = actionName,
                    UrgencyLevel = UrgencyLevel,
                    RequiredParticipants = GetRequiredParticipants(actionName),
                    IsCompleted = false,
                    StartTime = DateTime.Now
                };
                
                _activeActions.Add(emergencyAction);
            }
            
            // Update community goal based on active actions
            CommunityGoal.TargetAmount = _activeActions.Count * 100f; // 100% completion per action
        }

        private int GetRequiredParticipants(string actionName)
        {
            return actionName switch
            {
                "environmental_assessment" => 10,
                "damage_control" => 20,
                "recovery_planning" => 15,
                "legal_analysis" => 5,
                "compliance_planning" => 8,
                "community_education" => 25,
                "needs_assessment" => 12,
                "resource_mobilization" => 30,
                "community_support" => 50,
                "technical_analysis" => 8,
                "solution_development" => 12,
                "implementation" => 20,
                _ => 10
            };
        }

        // IEducationalEvent Implementation
        public EducationalProgress TrackLearningProgress(PlayerProfile player)
        {
            if (!_learningProgress.ContainsKey(player.PlayerId))
            {
                _learningProgress[player.PlayerId] = new EducationalProgress
                {
                    PlayerId = player.PlayerId,
                    EventId = EventId,
                    StartTime = DateTime.Now,
                    LastUpdate = DateTime.Now,
                    ObjectiveProgress = new Dictionary<string, float>(),
                    OverallProgress = 0f
                };
            }

            return _learningProgress[player.PlayerId];
        }

        public void ValidateEducationalContent()
        {
            if (RequiresScientificAccuracy)
            {
                Debug.Log($"[CrisisResponseEvent] Validating educational content for {CrisisType} response");
            }
        }

        // Crisis response specific methods
        public void CompleteEmergencyAction(string actionId, List<PlayerProfile> participants)
        {
            var action = _activeActions.Find(a => a.ActionId == actionId);
            if (action != null && !action.IsCompleted)
            {
                action.IsCompleted = true;
                action.CompletionTime = DateTime.Now;
                action.Participants = participants;
                
                // Update community progress
                float actionProgress = 100f; // Each action contributes 100 points
                UpdateCommunityProgress(new PlayerContribution
                {
                    PlayerId = "community_action",
                    TotalContribution = actionProgress
                });
                
                Debug.Log($"[CrisisResponseEvent] Emergency action {action.ActionName} completed by {participants.Count} participants");
                
                // Check if crisis is resolved
                if (IsCommunityGoalReached)
                {
                    ResolveCrisis();
                }
            }
        }

        private void ResolveCrisis()
        {
            IsEmergencyActive = false;
            EmergencyResponse.IsActive = false;
            
            var totalResponseTime = DateTime.Now - _crisisStartTime;
            Debug.Log($"[CrisisResponseEvent] Crisis {CrisisType} resolved after {totalResponseTime.TotalHours:F1} hours");
            
            // Grant bonus rewards for quick response
            if (totalResponseTime.TotalHours < 24)
            {
                GrantQuickResponseBonus();
            }
        }

        private void GrantQuickResponseBonus()
        {
            Debug.Log($"[CrisisResponseEvent] Granting quick response bonus for {CrisisType} crisis");
            // Implementation for bonus rewards
        }

        // Override base class methods for crisis response specific behavior
        protected override void OnEventStartedInternal()
        {
            base.OnEventStartedInternal();
            ActivateEmergencyResponse();
            Debug.Log($"[CrisisResponseEvent] {UrgencyLevel} {CrisisType} crisis event has started!");
        }

        protected override void OnEventEndedInternal(EventResult result)
        {
            base.OnEventEndedInternal(result);
            
            if (IsEmergencyActive)
            {
                Debug.LogWarning($"[CrisisResponseEvent] Crisis {CrisisType} ended without full resolution");
            }
            
            Debug.Log($"[CrisisResponseEvent] Crisis response event has ended!");
        }

        protected override void OnPlayerActionInternal(PlayerAction action)
        {
            base.OnPlayerActionInternal(action);
            
            // Track crisis response actions
            if (action.Type == ActionType.Help || action.Type == ActionType.Contribute)
            {
                var progress = TrackLearningProgress(action.Player);
                progress.OverallProgress = Math.Min(1.0f, progress.OverallProgress + 0.1f);
                
                // Check if player action completes an emergency action
                CheckForActionCompletion(action);
            }
        }

        private void CheckForActionCompletion(PlayerAction action)
        {
            foreach (var emergencyAction in _activeActions.Where(a => !a.IsCompleted))
            {
                if (emergencyAction.Participants.Count >= emergencyAction.RequiredParticipants)
                {
                    CompleteEmergencyAction(emergencyAction.ActionId, emergencyAction.Participants);
                }
            }
        }

        protected override bool ValidateParticipationRequirements(PlayerProfile player)
        {
            if (!base.ValidateParticipationRequirements(player))
                return false;

            // Additional crisis response validation
            if (UrgencyLevel >= UrgencyLevel.Critical)
            {
                // For critical situations, allow broader participation
                return true;
            }

            return true;
        }
    }

    // Crisis response specific data structures
    [Serializable]
    public class EmergencyAction
    {
        public string ActionId;
        public string ActionName;
        public string Description;
        public UrgencyLevel UrgencyLevel;
        public CrisisType ApplicableCrisisType;
        public UrgencyLevel RequiredUrgencyLevel;
        public int RequiredParticipants;
        public int MinParticipants;
        public int MaxParticipants;
        public float Duration; // in hours
        public List<PlayerProfile> Participants = new List<PlayerProfile>();
        public List<string> Requirements = new List<string>();
        public Dictionary<string, object> ActionData = new Dictionary<string, object>();
        public bool IsCompleted;
        public DateTime StartTime;
        public DateTime CompletionTime;
        
        public bool CanExecute(CrisisType crisisType, UrgencyLevel urgencyLevel, int participantCount)
        {
            return ApplicableCrisisType == crisisType &&
                   urgencyLevel >= RequiredUrgencyLevel &&
                   participantCount >= MinParticipants &&
                   participantCount <= MaxParticipants;
        }
    }
}