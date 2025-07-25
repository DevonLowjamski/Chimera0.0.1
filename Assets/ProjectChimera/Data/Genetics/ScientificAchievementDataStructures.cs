using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Data structures for scientific achievement tracking and progression.
    /// This file contains the core data types for managing scientific achievements,
    /// research milestones, and progression tracking in the cannabis cultivation system.
    /// </summary>
    
    // ScientificAchievement moved to ScientificAchievementDatabaseSO.cs to resolve namespace conflict
    
    /// <summary>
    /// Tracks research progress and scientific discoveries.
    /// </summary>
    [System.Serializable]
    public class ResearchProgress
    {
        public string ResearchID;
        public string ResearchName;
        public ResearchCategory Category;
        public float CompletionPercentage;
        public List<string> CompletedExperiments;
        public List<string> PendingExperiments;
        public DateTime StartDate;
        public DateTime? CompletionDate;
    }
    
    /// <summary>
    /// Categories of scientific research.
    /// </summary>
    public enum ResearchCategory
    {
        Genetics,
        Biochemistry,
        Cultivation,
        Environmental,
        Breeding,
        Analytics
    }
    
    /// <summary>
    /// Represents a scientific experiment result.
    /// </summary>
    [System.Serializable]
    public class ExperimentResult
    {
        public string ExperimentID;
        public string ExperimentName;
        public DateTime ConductedDate;
        public List<string> Participants;
        public SimpleTraitData TraitData; // Simplified trait data instead of TraitExpressionResult
        public float AccuracyScore;
        public bool IsSuccessful;
        public string Notes;
    }
    
    /// <summary>
    /// Simplified trait data for experiment results.
    /// </summary>
    [System.Serializable]
    public class SimpleTraitData
    {
        public string GenotypeID;
        public float OverallFitness;
        public float HeightExpression;
        public float THCExpression;
        public float CBDExpression;
        public float YieldExpression;
        public string Value; // Additional property for Systems compatibility
    }
    
    /// <summary>
    /// Scientific collaboration data.
    /// </summary>
    [System.Serializable]
    public class CollaborationData
    {
        public string CollaborationID;
        public List<string> Participants;
        public string ProjectName;
        public CollaborationType Type;
        public DateTime StartDate;
        public DateTime? EndDate;
        public List<ExperimentResult> SharedResults;
    }
    
    /// <summary>
    /// Types of scientific collaboration.
    /// </summary>
    public enum CollaborationType
    {
        Research,
        Breeding,
        DataSharing,
        Mentorship,
        Competition
    }
} 