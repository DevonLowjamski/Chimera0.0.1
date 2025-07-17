using UnityEngine;
using System;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Environment;

namespace ProjectChimera.Systems.Environment
{
    /// <summary>
    /// Optimization Competition Manager for Enhanced Environmental Gaming System v2.0
    /// Manages competitive optimization challenges and tournaments
    /// </summary>
    public class OptimizationCompetitionManager : MonoBehaviour
    {
        [Header("Competition Configuration")]
        [SerializeField] private bool _enableCompetitions = true;
        [SerializeField] private bool _enableOptimizationCompetitions = true;
        [SerializeField] private int _maxCompetitors = 50;
        [SerializeField] private float _competitionDuration = 60f; // minutes
        
        // Competition Data
        private Dictionary<string, OptimizationCompetition> _activeCompetitions = new Dictionary<string, OptimizationCompetition>();
        private List<CompetitionTemplate> _competitionTemplates = new List<CompetitionTemplate>();
        
        public bool IsInitialized { get; private set; }
        public int ActiveCompetitionsCount => _activeCompetitions.Count;
        
        /// <summary>
        /// Initialize the optimization competition manager
        /// </summary>
        public void Initialize()
        {
            LoadCompetitionTemplates();
            IsInitialized = true;
            Debug.Log("Optimization Competition Manager initialized");
        }
        
        /// <summary>
        /// Initialize with specific configuration
        /// </summary>
        /// <param name="enableOptimizationCompetitions">Enable optimization competitions</param>
        public void Initialize(bool enableOptimizationCompetitions)
        {
            _enableOptimizationCompetitions = enableOptimizationCompetitions;
            Initialize();
            Debug.Log($"Optimization Competition Manager initialized with competitions: {enableOptimizationCompetitions}");
        }
        
        private void LoadCompetitionTemplates()
        {
            // Load competition templates
            _competitionTemplates.Clear();
            
            // Add default templates
            _competitionTemplates.Add(new CompetitionTemplate
            {
                Name = "Energy Efficiency Challenge",
                Type = CompetitionType.EnergyOptimization,
                Duration = 30f,
                MaxParticipants = 20
            });
        }
        
        /// <summary>
        /// Shutdown the competition manager
        /// </summary>
        public void Shutdown()
        {
            _activeCompetitions.Clear();
            _competitionTemplates.Clear();
            IsInitialized = false;
            Debug.Log("Optimization Competition Manager shutdown");
        }
    }
    
    // Supporting classes
    [System.Serializable]
    public class OptimizationCompetition
    {
        public string CompetitionId;
        public string Name;
        public CompetitionType Type;
        public DateTime StartTime;
        public float Duration;
        public List<string> Participants = new List<string>();
        public bool IsActive = true;
    }
    
    [System.Serializable]
    public class CompetitionTemplate
    {
        public string Name;
        public CompetitionType Type;
        public float Duration;
        public int MaxParticipants;
    }
    
    public enum CompetitionType
    {
        EnergyOptimization,
        TemperatureControl,
        HumidityManagement,
        VPDOptimization,
        SystemEfficiency
    }
}