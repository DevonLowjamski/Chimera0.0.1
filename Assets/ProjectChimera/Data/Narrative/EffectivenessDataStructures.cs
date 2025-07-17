using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Data.Narrative
{
    /// <summary>
    /// Data structures for tracking narrative effectiveness and player engagement
    /// </summary>
    
    [Serializable]
    public class EffectivenessDataPoint
    {
        public string DataPointId;
        public DateTime Timestamp;
        public float EffectivenessScore;
        public string MetricType;
        public string SourceId;
        public Dictionary<string, object> Metadata = new Dictionary<string, object>();
        
        // Additional properties for CharacterRelationshipSystem compatibility
        public float Effectiveness;
        public string Topic;
        public string Fearful; // Added for CharacterRelationshipSystem compatibility
    }
    
    [Serializable]
    public class EffectivenessTrend
    {
        public string TrendId;
        public TrendDirection Direction;
        public float TrendStrength;
        public DateTime StartTime;
        public DateTime EndTime;
        public List<EffectivenessDataPoint> DataPoints = new List<EffectivenessDataPoint>();
        public string Description;
    }
    
    public enum TrendDirection
    {
        Increasing,
        Decreasing,
        Stable,
        Fluctuating,
        Unknown
    }
    
    [Serializable]
    public class NarrativeEffectivenessMetrics
    {
        public float PlayerEngagement;
        public float StoryProgression;
        public float ChoiceImpact;
        public float CharacterDevelopment;
        public float DialogueQuality;
        public DateTime LastUpdated;
        public Dictionary<string, float> CustomMetrics = new Dictionary<string, float>();
    }
}