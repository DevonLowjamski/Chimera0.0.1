using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Data.Narrative;

namespace ProjectChimera.Data.Narrative
{
    /// <summary>
    /// Character event data structure for Project Chimera's narrative system
    /// </summary>
    [Serializable]
    public class CharacterEventData
    {
        [Header("Core Event Data")]
        public string EventId;
        public CharacterEventType EventType;
        public string CharacterId;
        public float Timestamp;
        
        [Header("Relationship Data")]
        public bool HasRelationshipData;
        public float TrustLevel;
        public float RespectLevel;
        public float InfluenceLevel;
        public RelationshipType RelationshipType;
        public string InteractionContext;
        
        [Header("Emotional Data")]
        public bool HasEmotionalData;
        public EmotionalState EmotionalState;
        public float EmotionalIntensity;
        public string EmotionalTrigger;
        
        [Header("Educational Content")]
        public bool HasEducationalContent;
        public bool IsEducationalMentor;
        public List<CultivationExpertise> ExpertiseAreas = new List<CultivationExpertise>();
        public float CredibilityLevel;
        public string EducationalTopic;
        public float TeachingEffectiveness;
        public bool IsScientificallyAccurate;
        public string LearningOutcome;
        
        [Header("Dialogue Context")]
        public bool HasDialogueData;
        public string DialogueId;
        public string DialogueText;
        public List<string> PlayerChoiceOptions = new List<string>();
        public string SelectedPlayerChoice;
        public float DialogueDuration;
        
        [Header("Story Context")]
        public string CurrentArcId;
        public string CurrentBeatId;
        public string StoryContext;
        public List<string> StoryFlags = new List<string>();
        
        [Header("Performance Data")]
        public float ProcessingTime;
        public int InteractionCount;
        public DateTime LastInteractionTime;
        
        [Header("Additional Context")]
        public Dictionary<string, object> AdditionalData = new Dictionary<string, object>();
        public string Notes;
        public float Priority = 1.0f;
    }

    /// <summary>
    /// Character event type enumeration
    /// </summary>
    public enum CharacterEventType
    {
        FirstMeeting,
        DialogueStarted,
        DialogueCompleted,
        ChoiceResponse,
        RelationshipChanged,
        EmotionalStateChanged,
        TrustGained,
        TrustLost,
        RespectGained,
        RespectLost,
        InfluenceGained,
        InfluenceLost,
        TeachingMoment,
        LearningAssessment,
        EducationalMilestone,
        MentorshipStarted,
        MentorshipProgressed,
        ConflictResolution,
        CharacterDeparture,
        CharacterReturn,
        SpecialEvent,
        Custom
    }
} 