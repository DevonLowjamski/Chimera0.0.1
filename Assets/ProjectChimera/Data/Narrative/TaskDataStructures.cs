using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Data.Narrative
{
    /// <summary>
    /// Data structures for story campaign tasks and objectives
    /// </summary>
    
    [Serializable]
    public class TaskData
    {
        public string TaskId;
        public string TaskName;
        public string Description;
        public StoryTaskType Type;
        public TaskStatus Status;
        public int Priority;
        public DateTime CreatedTime;
        public DateTime? DueTime;
        public DateTime? CompletedTime;
        public List<string> Prerequisites = new List<string>();
        public List<string> Rewards = new List<string>();
        public Dictionary<string, object> TaskParameters = new Dictionary<string, object>();
        public float ProgressPercentage;
    }
    
    public enum StoryTaskType
    {
        Main,
        Side,
        Optional,
        Tutorial,
        Daily,
        Weekly,
        Achievement,
        Challenge
    }
    
    public enum TaskStatus
    {
        NotStarted,
        Available,
        InProgress,
        Completed,
        Failed,
        Locked,
        Expired
    }
    
    [Serializable]
    public class StoryEventData
    {
        public string EventId;
        public string EventName;
        public string Description;
        public TaskStoryEventType Type;
        public DateTime EventTime;
        public string TriggeredBy;
        public List<string> AffectedCharacters = new List<string>();
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
        public bool IsCompleted;
        
        // Additional properties for StoryCampaignManager compatibility
        public string EventType;
        public bool ArcStarted;
        public string ArcId;
        public DateTime Timestamp;
    }
    
    public enum TaskStoryEventType
    {
        Dialogue,
        Choice,
        Cutscene,
        Battle,
        Discovery,
        Relationship,
        Consequence,
        Revelation
    }
    
    // EducationalEventData is defined in NarrativeDataStructures.cs to avoid duplicates
    
    public enum EducationalEventType
    {
        Tutorial,
        Lesson,
        Quiz,
        Experiment,
        Simulation,
        Reference,
        Tip,
        Warning
    }
}