using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;
using LightSpectrumData = ProjectChimera.Data.Environment.LightSpectrumData;
using LightingZone = ProjectChimera.Data.Environment.LightingZone;
using PhotoperiodStage = ProjectChimera.Data.Environment.PhotoperiodStage;
using LightingZoneStatus = ProjectChimera.Data.Environment.LightingZoneStatus;
using LightingZoneSettings = ProjectChimera.Data.Environment.LightingZoneSettings;
using ActiveLightingFixture = ProjectChimera.Data.Environment.ActiveLightingFixture;
using LightingMaintenanceStatus = ProjectChimera.Data.Environment.LightingMaintenanceStatus;
using PlantGrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage;

namespace ProjectChimera.Systems.Environment
{
    /// <summary>
    /// PC013-6c: Specialized service for lighting system management
    /// Extracted from monolithic EnvironmentalManager.cs to handle grow lighting,
    /// spectrum control, photoperiod management, and DLI optimization.
    /// </summary>
    public class LightingSystemManager : ChimeraManager, IEnvironmentalService
    {
        [Header("Lighting Configuration")]
        [SerializeField] private float _lightingUpdateInterval = 30f; // 30 seconds
        [SerializeField] private bool _enableAutomaticLighting = true;
        [SerializeField] private bool _enableSpectralControl = true;
        [SerializeField] private int _maxLightingZones = 15;
        
        [Header("Default Light Settings")]
        [SerializeField] private float _defaultPPFD = 800f; // µmol/m²/s
        [SerializeField] private float _defaultDLI = 30f; // mol/m²/day
        [SerializeField] private int _defaultPhotoperiod = 18; // hours for vegetative
        
        [Header("Spectrum Control")]
        [SerializeField] private bool _enableRedSpectrum = true;
        [SerializeField] private bool _enableBlueSpectrum = true;
        [SerializeField] private bool _enableFarRedSpectrum = false;
        [SerializeField] private bool _enableUVSpectrum = false;
        
        [Header("Energy Management")]
        [SerializeField] private float _maxPowerConsumption = 10000f; // Watts
        [SerializeField] private float _energyEfficiency = 2.5f; // µmol/J
        
        // Lighting tracking data
        private Dictionary<string, LightingZone> _lightingZones = new Dictionary<string, LightingZone>();
        private Dictionary<string, List<SystemLightingFixture>> _zoneLighting = new Dictionary<string, List<SystemLightingFixture>>();
        private Dictionary<string, PhotoperiodSchedule> _photoperiodSchedules = new Dictionary<string, PhotoperiodSchedule>();
        private Dictionary<string, LightingPerformanceData> _performanceData = new Dictionary<string, LightingPerformanceData>();
        private Dictionary<string, LightingAlert> _activeLightingAlerts = new Dictionary<string, LightingAlert>();
        
        // Timing
        private float _lastLightingUpdate;
        private float _currentDayTime = 0f; // 0-24 hours
        
        public bool IsInitialized { get; private set; }
        
        // Properties
        public bool AutomaticLighting 
        { 
            get => _enableAutomaticLighting; 
            set => _enableAutomaticLighting = value; 
        }
        public bool SpectralControl 
        { 
            get => _enableSpectralControl; 
            set => _enableSpectralControl = value; 
        }
        public int ActiveLightingZones => _lightingZones.Count;
        public int ActiveAlerts => _activeLightingAlerts.Count;
        public float CurrentDayTime => _currentDayTime;
        
        // Events
        public System.Action<string, float> OnPPFDChanged;
        public System.Action<string, LightSpectrumData> OnSpectrumChanged;
        public System.Action<string, bool> OnLightingStateChanged;
        public System.Action<string, LightingAlert> OnLightingAlert;
        public System.Action<string, float> OnDLIUpdated;
        public System.Action<string, PhotoperiodPhase> OnPhotoperiodChanged;
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            Debug.Log("[LightingSystemManager] Initializing lighting systems...");
            
            _lastLightingUpdate = Time.time;
            _currentDayTime = 6f; // Start at 6 AM
            
            IsInitialized = true;
            Debug.Log($"[LightingSystemManager] Initialized. Auto-lighting: {_enableAutomaticLighting}, Spectral control: {_enableSpectralControl}");
        }
        
        public void Shutdown()
        {
            Debug.Log("[LightingSystemManager] Shutting down lighting systems...");
            
            // Turn off all lights before shutdown
            foreach (var kvp in _zoneLighting)
            {
                TurnOffAllFixtures(kvp.Value);
            }
            
            _lightingZones.Clear();
            _zoneLighting.Clear();
            _photoperiodSchedules.Clear();
            _performanceData.Clear();
            _activeLightingAlerts.Clear();
            
            IsInitialized = false;
        }
        
        private void Update()
        {
            if (!IsInitialized) return;
            
            // Update day time simulation
            _currentDayTime += Time.deltaTime * (24f / 86400f); // 24-hour cycle in real seconds
            if (_currentDayTime >= 24f) _currentDayTime -= 24f;
            
            if (Time.time - _lastLightingUpdate >= _lightingUpdateInterval)
            {
                UpdateLightingSystems();
                _lastLightingUpdate = Time.time;
            }
        }
        
        /// <summary>
        /// Creates a new lighting zone
        /// </summary>
        public bool CreateLightingZone(string zoneId, float zoneArea, PlantGrowthStage growthStage)
        {
            if (string.IsNullOrEmpty(zoneId))
            {
                Debug.LogWarning("[LightingSystemManager] Cannot create zone with null or empty ID");
                return false;
            }
            
            if (_lightingZones.ContainsKey(zoneId))
            {
                Debug.LogWarning($"[LightingSystemManager] Lighting zone '{zoneId}' already exists");
                return false;
            }
            
            if (_lightingZones.Count >= _maxLightingZones)
            {
                Debug.LogWarning($"[LightingSystemManager] Maximum lighting zone limit ({_maxLightingZones}) reached");
                return false;
            }
            
            var lightingZone = new LightingZone
            {
                ZoneId = zoneId,
                ZoneName = $"Zone_{zoneId.Substring(0, 8)}",
                ZoneSettings = new LightingZoneSettings(),
                GrowthStage = growthStage,
                TargetDLI = CalculateOptimalDLI(growthStage),
                CurrentDLI = 0f,
                CurrentSpectrum = CalculateOptimalSpectrum(growthStage),
                TargetSpectrum = CalculateOptimalSpectrum(growthStage),
                LightingFixtures = new List<ActiveLightingFixture>(),
                PhotoperiodStage = PhotoperiodStage.Day,
                ZoneStatus = LightingZoneStatus.Active,
                AutomaticPhotoperiod = true,
                CreatedAt = System.DateTime.Now,
                LastUpdated = System.DateTime.Now
            };
            
            _lightingZones[zoneId] = lightingZone;
            _zoneLighting[zoneId] = new List<SystemLightingFixture>();
            _performanceData[zoneId] = new LightingPerformanceData();
            
            // Set up photoperiod schedule
            SetupPhotoperiodSchedule(zoneId, growthStage);
            
            // Install default lighting
            InstallDefaultLighting(zoneId, zoneArea, growthStage);
            
            Debug.Log($"[LightingSystemManager] Created lighting zone '{zoneId}' for {growthStage} stage");
            return true;
        }
        
        /// <summary>
        /// Installs lighting fixtures in a zone
        /// </summary>
        public bool InstallSystemLightingFixture(string zoneId, SystemLightingFixtureType fixtureType, float wattage, LightSpectrumData spectrum)
        {
            if (!_lightingZones.ContainsKey(zoneId))
            {
                Debug.LogWarning($"[LightingSystemManager] Zone '{zoneId}' not found for fixture installation");
                return false;
            }
            
            var fixture = new SystemLightingFixture
            {
                FixtureId = System.Guid.NewGuid().ToString(),
                Type = fixtureType,
                Wattage = wattage,
                Spectrum = spectrum,
                Efficiency = CalculateFixtureEfficiency(fixtureType),
                CurrentIntensity = 0f,
                IsOperational = true,
                InstallationDate = System.DateTime.Now
            };
            
            _zoneLighting[zoneId].Add(fixture);
            
            Debug.Log($"[LightingSystemManager] Installed {fixtureType} fixture in zone '{zoneId}' ({wattage}W)");
            return true;
        }
        
        /// <summary>
        /// Controls lighting for optimal plant growth
        /// </summary>
        public void ControlLighting(string zoneId, float targetPPFD, LightSpectrumData targetSpectrum)
        {
            if (!_lightingZones.TryGetValue(zoneId, out LightingZone zone))
            {
                Debug.LogWarning($"[LightingSystemManager] Zone '{zoneId}' not found for lighting control");
                return;
            }
            
            var fixtures = _zoneLighting[zoneId];
            var currentPhase = GetCurrentPhotoperiodPhase(zoneId);
            float currentPPFD = 0f;
            
            // Determine if lights should be on based on photoperiod
            bool lightsOn = currentPhase == PhotoperiodPhase.Lights_On;
            
            if (lightsOn)
            {
                // Adjust fixtures to achieve target PPFD and spectrum
                AdjustFixturesForTarget(fixtures, targetPPFD, targetSpectrum);
                // Convert DLI to PPFD for calculation (approximate conversion)
                currentPPFD = zone.CurrentDLI * 46.3f; // Convert mol/m²/day to µmol/m²/s
                zone.CurrentDLI = CalculateDailyLightIntegral(currentPPFD);
            }
            else
            {
                // Turn off lights during dark period
                TurnOffAllFixtures(fixtures);
                zone.CurrentDLI = 0f;
            }
            
            // DLI is already calculated above
            _lightingZones[zoneId] = zone;
            
            // Update performance metrics
            UpdateLightingPerformanceData(zoneId);
            
            // Check for alerts
            CheckLightingAlerts(zoneId);
            
            // Fire events
            // Convert DLI to PPFD for event
            currentPPFD = zone.CurrentDLI * 46.3f;
            OnPPFDChanged?.Invoke(zoneId, currentPPFD);
            OnDLIUpdated?.Invoke(zoneId, zone.CurrentDLI);
            OnPhotoperiodChanged?.Invoke(zoneId, currentPhase);
            
            Debug.Log($"[LightingSystemManager] Controlled lighting for zone '{zoneId}' - PPFD: {currentPPFD:F0}, Phase: {currentPhase}");
        }
        
        /// <summary>
        /// Sets up photoperiod schedule for a zone
        /// </summary>
        public void SetupPhotoperiodSchedule(string zoneId, PlantGrowthStage growthStage)
        {
            var schedule = new PhotoperiodSchedule();
            
            switch (growthStage)
            {
                case PlantGrowthStage.Seed:
                case PlantGrowthStage.Germination:
                case PlantGrowthStage.Seedling:
                case PlantGrowthStage.Vegetative:
                    schedule.LightHours = 18;
                    schedule.DarkHours = 6;
                    schedule.LightStartTime = 6f; // 6 AM
                    break;
                    
                case PlantGrowthStage.Flowering:
                    schedule.LightHours = 12;
                    schedule.DarkHours = 12;
                    schedule.LightStartTime = 6f; // 6 AM
                    break;
                    
                default:
                    schedule.LightHours = 18;
                    schedule.DarkHours = 6;
                    schedule.LightStartTime = 6f;
                    break;
            }
            
            _photoperiodSchedules[zoneId] = schedule;
            
            Debug.Log($"[LightingSystemManager] Set photoperiod for zone '{zoneId}': {schedule.LightHours}L/{schedule.DarkHours}D");
        }
        
        /// <summary>
        /// Updates lighting systems for all zones
        /// </summary>
        private void UpdateLightingSystems()
        {
            foreach (var kvp in _lightingZones)
            {
                string zoneId = kvp.Key;
                LightingZone zone = kvp.Value;
                
                if (zone.ZoneStatus != LightingZoneStatus.Active) continue;
                
                // Auto-control lighting if enabled
                if (_enableAutomaticLighting)
                {
                    var optimalSpectrum = CalculateOptimalSpectrum(zone.GrowthStage);
                    // Convert DLI to PPFD for control method
                    float targetPPFD = zone.TargetDLI * 46.3f;
                    ControlLighting(zoneId, targetPPFD, optimalSpectrum);
                }
                
                // Update fixture maintenance status
                UpdateFixtureStatus(zoneId);
            }
        }
        
        /// <summary>
        /// Adjusts fixtures to achieve target PPFD and spectrum
        /// </summary>
        private void AdjustFixturesForTarget(List<SystemLightingFixture> fixtures, float targetPPFD, LightSpectrumData targetSpectrum)
        {
            float totalCurrentPPFD = 0f;
            int operationalCount = fixtures.Count(f => f.IsOperational);
            
            for (int i = 0; i < fixtures.Count; i++)
            {
                var fixture = fixtures[i];
                if (!fixture.IsOperational) continue;
                
                // Calculate optimal intensity for this fixture
                float fixtureContribution = targetPPFD / operationalCount;
                float maxPPFD = fixture.Wattage * fixture.Efficiency; // Simplified calculation
                
                fixture.CurrentIntensity = Mathf.Clamp01(fixtureContribution / maxPPFD);
                totalCurrentPPFD += fixture.CurrentIntensity * maxPPFD;
                
                // Adjust spectrum if spectral control is enabled
                if (_enableSpectralControl)
                {
                    AdjustFixtureSpectrum(fixture, targetSpectrum);
                }
                
                // Update the fixture in the list
                fixtures[i] = fixture;
            }
        }
        
        /// <summary>
        /// Adjusts individual fixture spectrum
        /// </summary>
        private void AdjustFixtureSpectrum(SystemLightingFixture fixture, LightSpectrumData targetSpectrum)
        {
            // This would involve controlling individual LED channels in a real system
            fixture.Spectrum = targetSpectrum;
            OnSpectrumChanged?.Invoke(fixture.FixtureId, targetSpectrum);
        }
        
        /// <summary>
        /// Turns off all fixtures in a list
        /// </summary>
        private void TurnOffAllFixtures(List<SystemLightingFixture> fixtures)
        {
            for (int i = 0; i < fixtures.Count; i++)
            {
                var fixture = fixtures[i];
                fixture.CurrentIntensity = 0f;
                OnLightingStateChanged?.Invoke(fixture.FixtureId, false);
                fixtures[i] = fixture;
            }
        }
        
        /// <summary>
        /// Turns off a single fixture (by reference)
        /// </summary>
        private void TurnOffFixture(ref SystemLightingFixture fixture)
        {
            fixture.CurrentIntensity = 0f;
            OnLightingStateChanged?.Invoke(fixture.FixtureId, false);
        }
        
        /// <summary>
        /// Gets current photoperiod phase for a zone
        /// </summary>
        private PhotoperiodPhase GetCurrentPhotoperiodPhase(string zoneId)
        {
            if (!_photoperiodSchedules.TryGetValue(zoneId, out PhotoperiodSchedule schedule))
                return PhotoperiodPhase.Lights_On;
            
            float lightEndTime = schedule.LightStartTime + schedule.LightHours;
            if (lightEndTime > 24f) lightEndTime -= 24f;
            
            bool isLightPeriod;
            if (schedule.LightStartTime < lightEndTime)
            {
                // Normal case: light period doesn't cross midnight
                isLightPeriod = _currentDayTime >= schedule.LightStartTime && _currentDayTime < lightEndTime;
            }
            else
            {
                // Light period crosses midnight
                isLightPeriod = _currentDayTime >= schedule.LightStartTime || _currentDayTime < lightEndTime;
            }
            
            return isLightPeriod ? PhotoperiodPhase.Lights_On : PhotoperiodPhase.Lights_Off;
        }
        
        /// <summary>
        /// Calculates actual PPFD from fixtures
        /// </summary>
        private float CalculateActualPPFD(List<SystemLightingFixture> fixtures)
        {
            float totalPPFD = 0f;
            
            foreach (var fixture in fixtures.Where(f => f.IsOperational))
            {
                float maxPPFD = fixture.Wattage * fixture.Efficiency;
                totalPPFD += fixture.CurrentIntensity * maxPPFD;
            }
            
            return totalPPFD;
        }
        
        /// <summary>
        /// Calculates daily light integral
        /// </summary>
        private float CalculateDailyLightIntegral(float currentPPFD)
        {
            // DLI = PPFD × photoperiod in hours × 3.6 / 1,000,000
            // This is a simplified calculation for the current moment
            return currentPPFD * 3.6f / 1000000f;
        }
        
        /// <summary>
        /// Updates fixture status and maintenance
        /// </summary>
        private void UpdateFixtureStatus(string zoneId)
        {
            if (!_zoneLighting.TryGetValue(zoneId, out List<SystemLightingFixture> fixtures))
                return;
            
            for (int i = 0; i < fixtures.Count; i++)
            {
                var fixture = fixtures[i];
                
                // Simulate LED degradation over time
                var daysSinceInstallation = (System.DateTime.Now - fixture.InstallationDate).TotalDays;
                if (daysSinceInstallation > 0)
                {
                    // LEDs degrade about 0.01% per day of operation
                    fixture.Efficiency = Mathf.Max(fixture.Efficiency - (float)(daysSinceInstallation * 0.0001f), 0.7f);
                }
                
                // Check for maintenance needs (after 3 years or 70% efficiency)
                if (daysSinceInstallation > 1095 || fixture.Efficiency < 0.8f)
                {
                    fixture.MaintenanceRequired = true;
                }
                
                fixtures[i] = fixture;
            }
        }
        
        /// <summary>
        /// Updates performance data for a zone
        /// </summary>
        private void UpdateLightingPerformanceData(string zoneId)
        {
            if (!_performanceData.TryGetValue(zoneId, out LightingPerformanceData data))
                return;
            
            var fixtures = _zoneLighting[zoneId];
            
            // Calculate total power consumption
            data.TotalPowerConsumption = fixtures.Where(f => f.IsOperational)
                .Sum(f => f.Wattage * f.CurrentIntensity);
            
            // Calculate average efficiency
            var operationalFixtures = fixtures.Where(f => f.IsOperational).ToList();
            data.AverageEfficiency = operationalFixtures.Any() 
                ? operationalFixtures.Average(f => f.Efficiency) 
                : 0f;
            
            // Update runtime
            data.TotalRuntime += Time.deltaTime;
            data.LastUpdated = System.DateTime.Now;
            
            _performanceData[zoneId] = data;
        }
        
        /// <summary>
        /// Checks for lighting alerts
        /// </summary>
        private void CheckLightingAlerts(string zoneId)
        {
            List<string> issues = new List<string>();
            var fixtures = _zoneLighting[zoneId];
            var zone = _lightingZones[zoneId];
            var performanceData = _performanceData[zoneId];
            
            // Fixture alerts
            var failedFixtures = fixtures.Where(f => !f.IsOperational).Count();
            if (failedFixtures > 0)
            {
                issues.Add($"{failedFixtures} lighting fixtures offline");
            }
            
            var maintenanceNeeded = fixtures.Where(f => f.MaintenanceRequired).Count();
            if (maintenanceNeeded > 0)
            {
                issues.Add($"{maintenanceNeeded} fixtures need maintenance");
            }
            
            // Performance alerts
            if (zone.CurrentDLI < zone.TargetDLI * 0.8f)
            {
                issues.Add($"Low DLI: {zone.CurrentDLI:F1} (target: {zone.TargetDLI:F1})");
            }
            
            if (performanceData.AverageEfficiency < 0.7f)
            {
                issues.Add($"Low fixture efficiency: {performanceData.AverageEfficiency:F1}%");
            }
            
            // Create or clear alerts
            string alertKey = $"lighting_{zoneId}";
            
            if (issues.Count > 0)
            {
                var alert = new LightingAlert
                {
                    ZoneId = zoneId,
                    Issues = issues,
                    Severity = DetermineLightingAlertSeverity(issues),
                    Timestamp = System.DateTime.Now
                };
                
                _activeLightingAlerts[alertKey] = alert;
                OnLightingAlert?.Invoke(zoneId, alert);
            }
            else if (_activeLightingAlerts.ContainsKey(alertKey))
            {
                _activeLightingAlerts.Remove(alertKey);
            }
        }
        
        /// <summary>
        /// Installs default lighting for a new zone
        /// </summary>
        private void InstallDefaultLighting(string zoneId, float zoneArea, PlantGrowthStage growthStage)
        {
            // Calculate required lighting based on zone size and growth stage
            float requiredWattage = CalculateRequiredWattage(zoneArea, growthStage);
            var optimalSpectrum = CalculateOptimalSpectrum(growthStage);
            
            // Install LED fixtures (assuming 200W fixtures)
            int fixtureCount = Mathf.CeilToInt(requiredWattage / 200f);
            
            for (int i = 0; i < fixtureCount; i++)
            {
                InstallSystemLightingFixture(zoneId, SystemLightingFixtureType.LED_FullSpectrum, 200f, optimalSpectrum);
            }
        }
        
        /// <summary>
        /// Calculates optimal PPFD for growth stage
        /// </summary>
        private float CalculateOptimalPPFD(PlantGrowthStage growthStage)
        {
            switch (growthStage)
            {
                case PlantGrowthStage.Seed:
                case PlantGrowthStage.Germination:
                    return 200f;
                case PlantGrowthStage.Seedling:
                    return 400f;
                case PlantGrowthStage.Vegetative:
                    return 600f;
                case PlantGrowthStage.Flowering:
                    return 800f;
                default:
                    return _defaultPPFD;
            }
        }
        
        /// <summary>
        /// Calculates optimal DLI for growth stage
        /// </summary>
        private float CalculateOptimalDLI(PlantGrowthStage growthStage)
        {
            switch (growthStage)
            {
                case PlantGrowthStage.Seed:
                case PlantGrowthStage.Germination:
                    return 12f;
                case PlantGrowthStage.Seedling:
                    return 20f;
                case PlantGrowthStage.Vegetative:
                    return 35f;
                case PlantGrowthStage.Flowering:
                    return 40f;
                default:
                    return _defaultDLI;
            }
        }
        
        /// <summary>
        /// Calculates optimal spectrum for growth stage
        /// </summary>
        private LightSpectrumData CalculateOptimalSpectrum(PlantGrowthStage growthStage)
        {
            switch (growthStage)
            {
                case PlantGrowthStage.Seedling:
                case PlantGrowthStage.Vegetative:
                    return new LightSpectrumData
                    {
                        RedPercent = 30f,
                        BluePercent = 40f,
                        GreenPercent = 20f,
                        FarRedPercent = 5f,
                        UVPercent = 5f
                    };
                    
                case PlantGrowthStage.Flowering:
                    return new LightSpectrumData
                    {
                        RedPercent = 50f,
                        BluePercent = 25f,
                        GreenPercent = 15f,
                        FarRedPercent = 8f,
                        UVPercent = 2f
                    };
                    
                default:
                    return new LightSpectrumData
                    {
                        RedPercent = 40f,
                        BluePercent = 30f,
                        GreenPercent = 20f,
                        FarRedPercent = 8f,
                        UVPercent = 2f
                    };
            }
        }
        
        /// <summary>
        /// Calculates required wattage for zone
        /// </summary>
        private float CalculateRequiredWattage(float zoneArea, PlantGrowthStage growthStage)
        {
            float wattsPerM2 = growthStage == PlantGrowthStage.Flowering ? 40f : 30f;
            return zoneArea * wattsPerM2;
        }
        
        /// <summary>
        /// Calculates fixture efficiency
        /// </summary>
        private float CalculateFixtureEfficiency(SystemLightingFixtureType fixtureType)
        {
            switch (fixtureType)
            {
                case SystemLightingFixtureType.LED_FullSpectrum:
                    return 2.8f; // µmol/J
                case SystemLightingFixtureType.LED_RedBlue:
                    return 3.0f;
                case SystemLightingFixtureType.HPS:
                    return 1.7f;
                case SystemLightingFixtureType.CMH:
                    return 1.9f;
                default:
                    return _energyEfficiency;
            }
        }
        
        /// <summary>
        /// Determines lighting alert severity
        /// </summary>
        private LightingAlertSeverity DetermineLightingAlertSeverity(List<string> issues)
        {
            if (issues.Any(i => i.Contains("offline"))) return LightingAlertSeverity.Critical;
            if (issues.Any(i => i.Contains("Low PPFD"))) return LightingAlertSeverity.High;
            if (issues.Count >= 2) return LightingAlertSeverity.Medium;
            return LightingAlertSeverity.Low;
        }
        
        /// <summary>
        /// Gets lighting zone data
        /// </summary>
        public LightingZone GetLightingZone(string zoneId)
        {
            return _lightingZones.TryGetValue(zoneId, out LightingZone zone) 
                ? zone 
                : new LightingZone();
        }
        
        /// <summary>
        /// Gets photoperiod schedule for a zone
        /// </summary>
        public PhotoperiodSchedule GetPhotoperiodSchedule(string zoneId)
        {
            return _photoperiodSchedules.TryGetValue(zoneId, out PhotoperiodSchedule schedule) 
                ? schedule 
                : new PhotoperiodSchedule();
        }
        
        /// <summary>
        /// Gets performance data for a zone
        /// </summary>
        public LightingPerformanceData GetPerformanceData(string zoneId)
        {
            return _performanceData.TryGetValue(zoneId, out LightingPerformanceData data) 
                ? data 
                : new LightingPerformanceData();
        }
        
        /// <summary>
        /// Gets all active lighting alerts
        /// </summary>
        public IReadOnlyDictionary<string, LightingAlert> GetActiveAlerts()
        {
            return _activeLightingAlerts;
        }
        
        #region ChimeraManager Implementation
        
        protected override void OnManagerInitialize()
        {
            // Manager-specific initialization logic (already implemented in Initialize method)
        }
        
        protected override void OnManagerShutdown()
        {
            // Manager-specific shutdown logic (already implemented in Shutdown method)
        }
        
        #endregion
    }
    
    
    /// <summary>
    /// Lighting fixture data
    /// </summary>
    [System.Serializable]
    public struct SystemLightingFixture
    {
        public string FixtureId;
        public SystemLightingFixtureType Type;
        public float Wattage;
        public LightSpectrumData Spectrum;
        public float Efficiency; // µmol/J
        public float CurrentIntensity; // 0-1
        public bool IsOperational;
        public bool MaintenanceRequired;
        public System.DateTime InstallationDate;
    }
    
    // Removed duplicate LightSpectrumData struct - using ProjectChimera.Data.Environment.LightSpectrumData instead
    
    /// <summary>
    /// Photoperiod schedule
    /// </summary>
    [System.Serializable]
    public struct PhotoperiodSchedule
    {
        public int LightHours;
        public int DarkHours;
        public float LightStartTime; // Hour of day (0-24)
    }
    
    /// <summary>
    /// Lighting performance data
    /// </summary>
    [System.Serializable]
    public struct LightingPerformanceData
    {
        public float TotalPowerConsumption;
        public float AverageEfficiency;
        public float TotalRuntime;
        public System.DateTime LastUpdated;
    }
    
    /// <summary>
    /// Lighting alert information
    /// </summary>
    [System.Serializable]
    public struct LightingAlert
    {
        public string ZoneId;
        public List<string> Issues;
        public LightingAlertSeverity Severity;
        public System.DateTime Timestamp;
    }
    
    /// <summary>
    /// Lighting fixture types
    /// </summary>
    public enum SystemLightingFixtureType
    {
        LED_FullSpectrum,
        LED_RedBlue,
        HPS, // High Pressure Sodium
        CMH, // Ceramic Metal Halide
        Fluorescent,
        CFL
    }
    
    /// <summary>
    /// Photoperiod phases
    /// </summary>
    public enum PhotoperiodPhase
    {
        Lights_On,
        Lights_Off,
        Sunrise,
        Sunset
    }
    
    
    /// <summary>
    /// Lighting alert severity levels
    /// </summary>
    public enum LightingAlertSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    
    /// <summary>
    /// Light spectrum data for environmental systems
    /// </summary>
    [System.Serializable]
    public struct LightSpectrumDataSystemData
    {
        public float RedPercent;
        public float BluePercent;
        public float GreenPercent;
        public float FarRedPercent;
        public float UVPercent;
        
        // Specific wavelength properties for advanced lighting systems
        public float UV_A_315_400nm;
        public float Blue_420_490nm;
        public float Green_490_550nm;
        public float Red_630_660nm;
        public float FarRed_700_850nm;
        
        // Additional wavelength properties
        public float Violet_400_420nm;
        public float Yellow_550_590nm;
        public float Orange_590_630nm;
        public float DeepRed_660_700nm;
        public float UV_B_280_315nm;
        public float NearInfrared_700_800nm;
        public float Cannabis_Specific_285nm;
        public float Cannabis_Specific_365nm;
        public float Cannabis_Specific_385nm;
        public float Cannabis_Specific_660nm;
        public float Cannabis_Specific_730nm;
        
        // Light quality metrics
        public float PhotosyntheticEfficiency;
        public float RedToFarRedRatio;
        public float BlueToRedRatio;
        public float UVToVisibleRatio;
        
        // Spectrum stability and control
        public bool SpectrumStability;
        public float FlickerFrequency;
        public float ColorTemperature;
        public float ChromaticCoordinates_X;
        public float ChromaticCoordinates_Y;
        
        // Photoperiod and circadian
        public float DailyPhotoperiod;
        public bool CircadianLighting;
        
        public float GetTerpeneEnhancingRatio()
        {
            // Enhanced terpene production with specific spectrum ratios
            return (UVPercent + FarRedPercent) / 100f;
        }
        
        public float GetTotalPAR()
        {
            return Violet_400_420nm + Blue_420_490nm + Green_490_550nm + 
                   Yellow_550_590nm + Orange_590_630nm + Red_630_660nm + DeepRed_660_700nm;
        }
        
        public float GetPhotosyntheticQuality()
        {
            float totalPAR = GetTotalPAR();
            if (totalPAR == 0f) return 0f;
            
            float weightedEfficiency = (
                Blue_420_490nm * 0.8f +
                Red_630_660nm * 1.0f +
                DeepRed_660_700nm * 0.9f +
                Green_490_550nm * 0.3f +
                Violet_400_420nm * 0.6f +
                Yellow_550_590nm * 0.2f +
                Orange_590_630nm * 0.4f
            ) / totalPAR;
            
            return UnityEngine.Mathf.Clamp01(weightedEfficiency * PhotosyntheticEfficiency);
        }
        
        public static LightSpectrumData CreateDefault()
        {
            return new LightSpectrumData
            {
                RedPercent = 40f,
                BluePercent = 30f,
                GreenPercent = 20f,
                FarRedPercent = 8f,
                UVPercent = 2f,
                UV_A_315_400nm = 15f,
                Blue_420_490nm = 100f,
                Green_490_550nm = 80f,
                Red_630_660nm = 120f,
                FarRed_700_850nm = 40f,
                Violet_400_420nm = 20f,
                Yellow_550_590nm = 60f,
                Orange_590_630nm = 40f,
                DeepRed_660_700nm = 80f,
                UV_B_280_315nm = 2f,
                NearInfrared_700_800nm = 60f,
                Cannabis_Specific_285nm = 1f,
                Cannabis_Specific_365nm = 8f,
                Cannabis_Specific_385nm = 12f,
                Cannabis_Specific_660nm = 100f,
                Cannabis_Specific_730nm = 30f,
                PhotosyntheticEfficiency = 1f,
                RedToFarRedRatio = 1.2f,
                BlueToRedRatio = 0.8f,
                UVToVisibleRatio = 0.05f,
                SpectrumStability = true,
                FlickerFrequency = 0f,
                ColorTemperature = 3000f,
                ChromaticCoordinates_X = 0.33f,
                ChromaticCoordinates_Y = 0.33f,
                DailyPhotoperiod = 18f,
                CircadianLighting = false
            };
        }
        
        // Operator overloads for spectrum arithmetic
        public static LightSpectrumDataSystemData operator +(LightSpectrumDataSystemData a, LightSpectrumDataSystemData b)
        {
            return new LightSpectrumDataSystemData
            {
                RedPercent = a.RedPercent + b.RedPercent,
                BluePercent = a.BluePercent + b.BluePercent,
                GreenPercent = a.GreenPercent + b.GreenPercent,
                FarRedPercent = a.FarRedPercent + b.FarRedPercent,
                UVPercent = a.UVPercent + b.UVPercent,
                UV_A_315_400nm = a.UV_A_315_400nm + b.UV_A_315_400nm,
                Blue_420_490nm = a.Blue_420_490nm + b.Blue_420_490nm,
                Green_490_550nm = a.Green_490_550nm + b.Green_490_550nm,
                Red_630_660nm = a.Red_630_660nm + b.Red_630_660nm,
                FarRed_700_850nm = a.FarRed_700_850nm + b.FarRed_700_850nm
            };
        }
        
        public static bool operator ==(LightSpectrumDataSystemData a, LightSpectrumDataSystemData b)
        {
            return a.Equals(b);
        }
        
        public static bool operator !=(LightSpectrumDataSystemData a, LightSpectrumDataSystemData b)
        {
            return !a.Equals(b);
        }
        
        public override bool Equals(object obj)
        {
            if (obj is LightSpectrumDataSystemData other)
            {
                return RedPercent == other.RedPercent && BluePercent == other.BluePercent;
            }
            return false;
        }
        
        public override int GetHashCode()
        {
            return RedPercent.GetHashCode() ^ BluePercent.GetHashCode();
        }
    }
    
    /// <summary>
    /// Active lighting fixtures collection
    /// </summary>
    [System.Serializable]
    public struct ActiveSystemLightingFixtures
    {
        public List<SystemLightingFixture> Fixtures;
        public float TotalPowerConsumption;
        public float AverageEfficiency;
        public int OperationalCount;
        
        public ActiveSystemLightingFixtures(List<SystemLightingFixture> fixtures)
        {
            Fixtures = fixtures ?? new List<SystemLightingFixture>();
            TotalPowerConsumption = 0f;
            AverageEfficiency = 0f;
            OperationalCount = 0;
        }
    }
}