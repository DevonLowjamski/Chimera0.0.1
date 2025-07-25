using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.IPM
{
    /// <summary>
    /// Clean IPM data types that eliminate circular dependencies
    /// Created to replace conflicting IPM system types
    /// </summary>
    
    [Serializable]
    public class CleanIPMPestData
    {
        public string PestID = "";
        public string PestName = "";
        public string Description = "";
        public IPMPestType PestType = IPMPestType.Insect;
        public IPMSeverityLevel SeverityLevel = IPMSeverityLevel.Low;
        public float DamageRate = 0f;
        public List<string> AffectedPlantParts = new List<string>();
        public bool IsActive = false;
        public DateTime DetectionDate = DateTime.Now;
    }
    
    [Serializable]
    public class CleanIPMTreatment
    {
        public string TreatmentID = "";
        public string TreatmentName = "";
        public IPMTreatmentType TreatmentType = IPMTreatmentType.Biological;
        public float Effectiveness = 0f;
        public float Cost = 0f;
        public int ApplicationFrequency = 1;
        public bool IsOrganic = true;
        public List<string> TargetPests = new List<string>();
    }
    
    [Serializable]
    public class CleanIPMConfiguration
    {
        public string ConfigurationID = "";
        public bool EnableAutomaticDetection = true;
        public bool EnablePreventiveTreatment = true;
        public IPMDifficulty Difficulty = IPMDifficulty.Beginner;
        public float DetectionSensitivity = 0.5f;
        public float TreatmentThreshold = 0.3f;
    }
    
    [Serializable]
    public class CleanIPMBattleResult
    {
        public string BattleID = "";
        public string PestID = "";
        public string TreatmentID = "";
        public bool Success = false;
        public float EffectivenessScore = 0f;
        public DateTime BattleDate = DateTime.Now;
        public string Notes = "";
    }
    
    [Serializable]
    public class CleanIPMAnalytics
    {
        public float TotalPestsDetected = 0f;
        public float TotalTreatmentsApplied = 0f;
        public float SuccessRate = 0f;
        public float AverageResponseTime = 0f;
        public Dictionary<string, float> PestFrequency = new Dictionary<string, float>();
    }
    
    // Clean enums to avoid conflicts
    public enum IPMPestType
    {
        Insect,
        Mite,
        Disease,
        Virus,
        Bacteria,
        Fungus,
        Nematode,
        Other
    }
    
    public enum IPMSeverityLevel
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    public enum IPMTreatmentType
    {
        Biological,
        Chemical,
        Physical,
        Cultural,
        Integrated
    }
    
    public enum IPMDifficulty
    {
        Beginner,
        Intermediate,
        Advanced,
        Expert
    }
    
    public enum IPMBattleStatus
    {
        InProgress,
        Won,
        Lost,
        Stalemate
    }
}