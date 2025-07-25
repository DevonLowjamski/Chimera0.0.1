using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.IPM;

namespace ProjectChimera.Systems.IPM
{
    /// <summary>
    /// Clean, minimal IPM manager that eliminates circular dependencies
    /// Uses only existing types from ProjectChimera.Data.IPM namespace
    /// </summary>
    public class CleanIPMManager : ChimeraManager
    {
        [Header("Clean IPM Configuration")]
        [SerializeField] private bool _enablePestDetection = true;
        [SerializeField] private bool _enableTreatments = true;
        [SerializeField] private bool _enableAnalytics = true;
        [SerializeField] private bool _enableAutomation = false;
        
        // Using existing IPM data structures
        private List<PestInvasionData> _detectedPests = new List<PestInvasionData>();
        private List<BeneficialOrganismData> _beneficialOrganisms = new List<BeneficialOrganismData>();
        private List<DefenseStructureData> _defenseStructures = new List<DefenseStructureData>();
        private IPMPlayerProfile _playerProfile = new IPMPlayerProfile();
        private IPMAnalyticsData _analytics = new IPMAnalyticsData();
        
        // Simple state tracking
        private bool _isInitialized = false;
        private float _totalPestsDetected = 0f;
        private float _totalTreatmentsApplied = 0f;
        private float _currentThreatLevel = 0f;
        
        #region Manager Lifecycle
        
        public override string ManagerName => "Clean IPM Manager";
        
        protected override void OnManagerInitialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("CleanIPMManager already initialized");
                return;
            }
            
            InitializeBasicSystems();
            _isInitialized = true;
            
            Debug.Log("CleanIPMManager initialized successfully");
        }
        
        protected override void OnManagerShutdown()
        {
            _isInitialized = false;
            _detectedPests.Clear();
            _beneficialOrganisms.Clear();
            _defenseStructures.Clear();
            
            Debug.Log("CleanIPMManager shutdown successfully");
        }
        
        private void InitializeBasicSystems()
        {
            // Initialize player profile
            _playerProfile.PlayerId = System.Guid.NewGuid().ToString();
            _playerProfile.PlayerName = "IPM Player";
            _playerProfile.IPMLevel = 1;
            _playerProfile.TotalExperience = 0f;
            _playerProfile.ProfileCreated = System.DateTime.Now;
            
            // Initialize analytics
            _analytics.AnalyticsId = System.Guid.NewGuid().ToString();
            _analytics.PlayerId = _playerProfile.PlayerId;
            _analytics.CollectionTime = System.DateTime.Now;
            _analytics.EfficiencyScore = 0f;
            _analytics.ResourceUtilization = 0f;
            
            if (_enableTreatments)
            {
                CreateSampleBeneficialOrganisms();
            }
        }
        
        #endregion
        
        #region Public API
        
        public bool IsSystemReady => _isInitialized;
        
        public float GetTotalPestsDetected() => _totalPestsDetected;
        public float GetTotalTreatmentsApplied() => _totalTreatmentsApplied;
        public float GetCurrentThreatLevel() => _currentThreatLevel;
        public float GetSuccessRate() => _analytics.EfficiencyScore;
        
        public List<PestInvasionData> GetDetectedPests() => new List<PestInvasionData>(_detectedPests);
        public List<BeneficialOrganismData> GetBeneficialOrganisms() => new List<BeneficialOrganismData>(_beneficialOrganisms);
        public List<DefenseStructureData> GetDefenseStructures() => new List<DefenseStructureData>(_defenseStructures);
        public IPMPlayerProfile GetPlayerProfile() => _playerProfile;
        public IPMAnalyticsData GetAnalytics() => _analytics;
        
        #endregion
        
        #region Pest Detection System
        
        public bool DetectPest(string pestName, ProjectChimera.Data.IPM.PestType pestType)
        {
            if (!_isInitialized || !_enablePestDetection)
                return false;
            
            var pest = new PestInvasionData
            {
                InvasionId = System.Guid.NewGuid().ToString(),
                PestType = pestType,
                PopulationSize = UnityEngine.Random.Range(10, 100),
                AggressionLevel = UnityEngine.Random.Range(0.1f, 0.8f),
                OriginPoint = Vector3.zero,
                ReproductionRate = UnityEngine.Random.Range(0.1f, 0.3f),
                InvasionStartTime = System.DateTime.Now,
                IsAdaptive = false
            };
            
            _detectedPests.Add(pest);
            _totalPestsDetected++;
            UpdateThreatLevel();
            
            Debug.Log($"Pest detected: {pestName} (Type: {pestType})");
            return true;
        }
        
        public void UpdateThreatLevel()
        {
            if (_detectedPests.Count == 0)
            {
                _currentThreatLevel = 0f;
                return;
            }
            
            float totalThreat = 0f;
            foreach (var pest in _detectedPests)
            {
                float threatContribution = (pest.PopulationSize / 100f) * pest.AggressionLevel;
                totalThreat += threatContribution;
            }
            
            _currentThreatLevel = Mathf.Clamp01(totalThreat / _detectedPests.Count);
        }
        
        #endregion
        
        #region Treatment System
        
        public bool ApplyBeneficialOrganism(string pestID, BeneficialOrganismType organismType)
        {
            if (!_isInitialized || !_enableTreatments)
                return false;
            
            var pest = _detectedPests.Find(p => p.InvasionId == pestID);
            
            if (pest != null)
            {
                var organism = new BeneficialOrganismData
                {
                    OrganismId = System.Guid.NewGuid().ToString(),
                    Type = organismType,
                    PopulationSize = UnityEngine.Random.Range(50, 200),
                    ReleaseLocation = Vector3.zero,
                    HuntingEfficiency = UnityEngine.Random.Range(0.6f, 0.9f),
                    SurvivalRate = UnityEngine.Random.Range(0.7f, 0.95f),
                    ReleaseTime = System.DateTime.Now,
                    IsEstablished = true
                };
                
                _beneficialOrganisms.Add(organism);
                _totalTreatmentsApplied++;
                
                // Reduce pest population
                int reduction = Mathf.RoundToInt(pest.PopulationSize * organism.HuntingEfficiency * 0.5f);
                pest.PopulationSize = Mathf.Max(0, pest.PopulationSize - reduction);
                
                bool success = pest.PopulationSize < 10;
                UpdateThreatLevel();
                UpdateAnalytics();
                
                Debug.Log($"Treatment {(success ? "successful" : "partially effective")}: {organismType} vs {pest.PestType}");
                return success;
            }
            
            return false;
        }
        
        private void CreateSampleBeneficialOrganisms()
        {
            var organism1 = new BeneficialOrganismData
            {
                OrganismId = System.Guid.NewGuid().ToString(),
                Type = BeneficialOrganismType.Ladybugs,
                PopulationSize = 100,
                ReleaseLocation = Vector3.zero,
                HuntingEfficiency = 0.8f,
                SurvivalRate = 0.9f,
                ReleaseTime = System.DateTime.Now,
                IsEstablished = false
            };
            
            var organism2 = new BeneficialOrganismData
            {
                OrganismId = System.Guid.NewGuid().ToString(),
                Type = BeneficialOrganismType.PredatoryMites,
                PopulationSize = 150,
                ReleaseLocation = Vector3.zero,
                HuntingEfficiency = 0.75f,
                SurvivalRate = 0.85f,
                ReleaseTime = System.DateTime.Now,
                IsEstablished = false
            };
            
            _beneficialOrganisms.Add(organism1);
            _beneficialOrganisms.Add(organism2);
        }
        
        #endregion
        
        #region Analytics System
        
        private void UpdateAnalytics()
        {
            _analytics.CollectionTime = System.DateTime.Now;
            _analytics.EfficiencyScore = _totalTreatmentsApplied > 0 ? 
                Mathf.Clamp01((_totalTreatmentsApplied - _detectedPests.Count) / _totalTreatmentsApplied) : 0f;
            _analytics.ResourceUtilization = _beneficialOrganisms.Count > 0 ? 
                Mathf.Clamp01(_beneficialOrganisms.Count / 10f) : 0f;
        }
        
        #endregion
        
        #region Debug and Testing
        
        [ContextMenu("Test Pest Detection")]
        private void TestPestDetection()
        {
            DetectPest("Test Aphids", ProjectChimera.Data.IPM.PestType.Aphids);
        }
        
        [ContextMenu("Test Treatment")]
        private void TestTreatment()
        {
            if (_detectedPests.Count > 0)
            {
                ApplyBeneficialOrganism(_detectedPests[0].InvasionId, BeneficialOrganismType.Ladybugs);
            }
        }
        
        [ContextMenu("Print Status")]
        private void PrintStatus()
        {
            Debug.Log($"IPM Manager Status: Initialized={_isInitialized}, " +
                     $"Pests={_totalPestsDetected}, Treatments={_totalTreatmentsApplied}, " +
                     $"Threat={_currentThreatLevel:F2}, Efficiency={_analytics.EfficiencyScore:F2}");
        }
        
        #endregion
    }
}