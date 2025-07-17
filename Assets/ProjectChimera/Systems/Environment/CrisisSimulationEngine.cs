using UnityEngine;
using System;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Environment;

namespace ProjectChimera.Systems.Environment
{
    /// <summary>
    /// Crisis Simulation Engine for Enhanced Environmental Gaming System v2.0
    /// Generates and manages environmental crisis scenarios for training
    /// </summary>
    public class CrisisSimulationEngine : MonoBehaviour
    {
        [Header("Crisis Configuration")]
        [SerializeField] private bool _enableCrisisSimulation = true;
        [SerializeField] private float _crisisFrequency = 0.1f; // per hour
        [SerializeField] private int _maxSimultaneousCrises = 3;
        
        // Crisis Data
        private Dictionary<string, EnvironmentalCrisis> _activeCrises = new Dictionary<string, EnvironmentalCrisis>();
        private List<CrisisTemplate> _crisisTemplates = new List<CrisisTemplate>();
        
        public bool IsInitialized { get; private set; }
        public int ActiveCrisesCount => _activeCrises.Count;
        
        /// <summary>
        /// Initialize the crisis simulation engine
        /// </summary>
        public void Initialize()
        {
            LoadCrisisTemplates();
            IsInitialized = true;
            Debug.Log("Crisis Simulation Engine initialized");
        }
        
        /// <summary>
        /// Initialize with specific configuration
        /// </summary>
        /// <param name="enableCrisisSimulation">Enable crisis simulation</param>
        public void Initialize(bool enableCrisisSimulation)
        {
            _enableCrisisSimulation = enableCrisisSimulation;
            Initialize();
            Debug.Log($"Crisis Simulation Engine initialized with simulation: {enableCrisisSimulation}");
        }
        
        /// <summary>
        /// Generate a new environmental crisis
        /// </summary>
        /// <param name="crisisType">Type of crisis to generate</param>
        /// <param name="severity">Crisis severity level</param>
        /// <returns>Generated crisis</returns>
        public EnvironmentalCrisis GenerateCrisis(CrisisType crisisType, CrisisSeverity severity)
        {
            var crisis = new EnvironmentalCrisis
            {
                CrisisId = Guid.NewGuid().ToString(),
                Type = crisisType,
                Severity = severity,
                StartTime = DateTime.Now,
                IsActive = true,
                Description = GetCrisisDescription(crisisType, severity)
            };
            
            _activeCrises[crisis.CrisisId] = crisis;
            return crisis;
        }
        
        private void LoadCrisisTemplates()
        {
            // Load crisis templates
            _crisisTemplates.Clear();
            
            // Add default templates
            _crisisTemplates.Add(new CrisisTemplate
            {
                Type = CrisisType.PowerOutage,
                Severity = CrisisSeverity.Moderate,
                Duration = 30f,
                Description = "Electrical power disruption affecting environmental systems"
            });
        }
        
        private string GetCrisisDescription(CrisisType type, CrisisSeverity severity)
        {
            return $"{severity} {type} crisis requiring immediate response";
        }
        
        /// <summary>
        /// Shutdown the crisis simulation engine
        /// </summary>
        public void Shutdown()
        {
            _activeCrises.Clear();
            _crisisTemplates.Clear();
            IsInitialized = false;
            Debug.Log("Crisis Simulation Engine shutdown");
        }
    }
    
    // Supporting classes
    [System.Serializable]
    public class EnvironmentalCrisis
    {
        public string CrisisId;
        public CrisisType Type;
        public CrisisSeverity Severity;
        public DateTime StartTime;
        public float Duration;
        public bool IsActive;
        public string Description;
        public List<string> AffectedSystems = new List<string>();
    }
    
    [System.Serializable]
    public class CrisisTemplate
    {
        public CrisisType Type;
        public CrisisSeverity Severity;
        public float Duration;
        public string Description;
    }
    
    public enum CrisisType
    {
        PowerOutage,
        HVACFailure,
        TemperatureSpike,
        HumidityExtreme,
        SystemMalfunction,
        EquipmentFailure
    }
    
    public enum CrisisSeverity
    {
        Minor,
        Moderate,
        Major,
        Critical,
        Catastrophic
    }
}