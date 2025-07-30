using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.IPM;
using ProjectChimera.Data.Environment;

namespace ProjectChimera.Systems.IPM
{
    /// <summary>
    /// Comprehensive IPM control methods implementation including biological, chemical, and cultural controls.
    /// Part of PC017-1b: Implement biological, chemical, and cultural control methods
    /// </summary>
    public class IPMControlMethods : ChimeraManager
    {
        [Header("Control System Configuration")]
        [SerializeField] private bool _enableBiologicalControl = true;
        [SerializeField] private bool _enableChemicalControl = true;
        [SerializeField] private bool _enableCulturalControl = true;
        [SerializeField] private float _controlEffectivenessMultiplier = 1.0f;
        [SerializeField] private float _resistanceDevelopmentRate = 0.05f;
        
        [Header("Biological Control Settings")]
        [SerializeField] private float _beneficialOrganismSurvivalRate = 0.8f;
        [SerializeField] private float _predationEfficiency = 0.6f;
        [SerializeField] private int _maxBeneficialPopulationPerZone = 5000;
        [SerializeField] private float _biologicalControlRange = 10.0f;
        
        [Header("Chemical Control Settings")]
        [SerializeField] private float _chemicalDegradationRate = 0.1f;
        [SerializeField] private float _environmentalImpactThreshold = 0.3f;
        [SerializeField] private float _resistanceRiskThreshold = 0.7f;
        [SerializeField] private bool _enableOrganicOnly = false;
        
        [Header("Cultural Control Settings")]
        [SerializeField] private float _sanitationEffectiveness = 0.4f;
        [SerializeField] private float _quarantineEffectiveness = 0.9f;
        [SerializeField] private float _cropRotationBenefit = 0.3f;
        [SerializeField] private float _environmentalModificationRange = 15.0f;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onBiologicalControlDeployed;
        [SerializeField] private SimpleGameEventSO _onChemicalApplicationApplied;
        [SerializeField] private SimpleGameEventSO _onCulturalControlImplemented;
        [SerializeField] private SimpleGameEventSO _onControlMethodEffectivenessChanged;
        
        // Core control data
        private Dictionary<string, BiologicalControlAgent> _activeBiologicalAgents = new Dictionary<string, BiologicalControlAgent>();
        private Dictionary<string, ChemicalApplication> _activeChemicalApplications = new Dictionary<string, ChemicalApplication>();
        private Dictionary<string, CulturalControlMeasure> _activeCulturalControls = new Dictionary<string, CulturalControlMeasure>();
        private Dictionary<PestType, ResistanceProfile> _pestResistanceProfiles = new Dictionary<PestType, ResistanceProfile>();
        
        // Performance tracking
        private Dictionary<string, ControlMethodEffectiveness> _controlEffectiveness = new Dictionary<string, ControlMethodEffectiveness>();
        private float _lastUpdateTime;
        private int _totalControlsDeployed;
        private float _overallSystemEffectiveness;
        
        #region ChimeraManager Implementation
        
        protected override void OnManagerInitialize()
        {
            InitializeControlSystems();
            InitializePestResistanceProfiles();
            _lastUpdateTime = Time.time;
            
            Debug.Log($"[IPMControlMethods] Initialized with {_activeBiologicalAgents.Count} biological agents, " +
                     $"{_activeChemicalApplications.Count} chemical applications, and {_activeCulturalControls.Count} cultural controls");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!IsInitialized) return;
            
            float deltaTime = Time.time - _lastUpdateTime;
            if (deltaTime >= 1.0f) // Update every second
            {
                UpdateBiologicalControl(deltaTime);
                UpdateChemicalControl(deltaTime);
                UpdateCulturalControl(deltaTime);
                UpdatePestResistance(deltaTime);
                UpdateControlEffectiveness(deltaTime);
                
                _lastUpdateTime = Time.time;
            }
        }
        
        protected override void OnManagerShutdown()
        {
            // Clean up all active control methods
            _activeBiologicalAgents.Clear();
            _activeChemicalApplications.Clear();
            _activeCulturalControls.Clear();
            _controlEffectiveness.Clear();
            _pestResistanceProfiles.Clear();
            
            Debug.Log("[IPMControlMethods] Manager shutdown completed - all control methods cleared");
        }
        
        #endregion
        
        #region Biological Control Methods
        
        /// <summary>
        /// Deploy biological control agents to target specific pest populations
        /// </summary>
        public bool DeployBiologicalControl(BeneficialOrganismType organismType, string zoneId, int releaseQuantity, List<PestType> targetPests)
        {
            if (!_enableBiologicalControl)
            {
                Debug.LogWarning("[IPMControlMethods] Biological control is disabled");
                return false;
            }
            
            string agentId = GenerateAgentId(organismType, zoneId);
            
            var biologicalAgent = new BiologicalControlAgent
            {
                AgentId = agentId,
                OrganismType = organismType,
                ZoneId = zoneId,
                InitialPopulation = releaseQuantity,
                CurrentPopulation = releaseQuantity,
                TargetPests = targetPests,
                DeploymentTime = DateTime.Now,
                IsActive = true,
                EstablishmentStatus = EstablishmentStatus.Establishing,
                HuntingEfficiency = CalculateHuntingEfficiency(organismType),
                SurvivalRate = _beneficialOrganismSurvivalRate,
                SearchRadius = _biologicalControlRange,
                EnvironmentalStress = 0.0f
            };
            
            _activeBiologicalAgents[agentId] = biologicalAgent;
            
            // Track effectiveness
            var effectiveness = new ControlMethodEffectiveness
            {
                ControlId = agentId,
                MethodType = ControlMethodType.Biological,
                ZoneId = zoneId,
                DeploymentTime = DateTime.Now,
                InitialEffectiveness = biologicalAgent.HuntingEfficiency,
                CurrentEffectiveness = biologicalAgent.HuntingEfficiency,
                CostEffectiveness = CalculateBiologicalCostEffectiveness(organismType, releaseQuantity),
                EnvironmentalImpact = 0.1f, // Biological control has minimal environmental impact
                ResistanceRisk = 0.05f // Low resistance development risk
            };
            
            _controlEffectiveness[agentId] = effectiveness;
            _totalControlsDeployed++;
            
            _onBiologicalControlDeployed?.Raise();
            
            Debug.Log($"[IPMControlMethods] Deployed {releaseQuantity} {organismType} agents in zone {zoneId} targeting {string.Join(", ", targetPests)}");
            return true;
        }
        
        /// <summary>
        /// Update biological control agent populations and effectiveness
        /// </summary>
        private void UpdateBiologicalControl(float deltaTime)
        {
            var agentsToRemove = new List<string>();
            
            foreach (var kvp in _activeBiologicalAgents)
            {
                var agent = kvp.Value;
                
                // Update population dynamics
                UpdateBiologicalAgentPopulation(agent, deltaTime);
                
                // Check for establishment
                UpdateEstablishmentStatus(agent);
                
                // Calculate pest control impact
                float controlImpact = CalculateBiologicalControlImpact(agent);
                
                // Update effectiveness tracking
                if (_controlEffectiveness.ContainsKey(agent.AgentId))
                {
                    var effectiveness = _controlEffectiveness[agent.AgentId];
                    effectiveness.CurrentEffectiveness = controlImpact;
                    effectiveness.TotalPestsControlled += (int)(controlImpact * agent.CurrentPopulation * deltaTime);
                }
                
                // Remove extinct populations
                if (agent.CurrentPopulation <= 0)
                {
                    agentsToRemove.Add(kvp.Key);
                    Debug.Log($"[IPMControlMethods] Biological agent {agent.OrganismType} in zone {agent.ZoneId} has become extinct");
                }
            }
            
            // Clean up extinct agents
            foreach (string agentId in agentsToRemove)
            {
                _activeBiologicalAgents.Remove(agentId);
                if (_controlEffectiveness.ContainsKey(agentId))
                {
                    _controlEffectiveness[agentId].IsActive = false;
                }
            }
        }
        
        /// <summary>
        /// Update biological agent population based on environmental factors and prey availability
        /// </summary>
        private void UpdateBiologicalAgentPopulation(BiologicalControlAgent agent, float deltaTime)
        {
            // Base survival rate
            float survivalMultiplier = agent.SurvivalRate;
            
            // Environmental stress impact
            survivalMultiplier *= (1.0f - agent.EnvironmentalStress);
            
            // Prey availability impact (simplified)
            float preyAvailability = GetPreyAvailability(agent.ZoneId, agent.TargetPests);
            survivalMultiplier *= Mathf.Clamp01(preyAvailability);
            
            // Population change
            float populationChange = agent.CurrentPopulation * (survivalMultiplier - 1.0f) * deltaTime;
            agent.CurrentPopulation = Mathf.RoundToInt(Mathf.Max(0, agent.CurrentPopulation + populationChange));
            
            // Cap population based on carrying capacity
            int maxPopulation = _maxBeneficialPopulationPerZone;
            agent.CurrentPopulation = Mathf.Min(agent.CurrentPopulation, maxPopulation);
        }
        
        #endregion
        
        #region Chemical Control Methods
        
        /// <summary>
        /// Apply chemical control treatment to target pest populations
        /// </summary>
        public bool ApplyChemicalControl(string chemicalName, ChemicalType chemicalType, string zoneId, 
            float concentration, List<PestType> targetPests, ApplicationMethod method)
        {
            if (!_enableChemicalControl)
            {
                Debug.LogWarning("[IPMControlMethods] Chemical control is disabled");
                return false;
            }
            
            // Check organic-only restriction
            if (_enableOrganicOnly && !IsOrganicChemical(chemicalName))
            {
                Debug.LogWarning($"[IPMControlMethods] Non-organic chemical {chemicalName} rejected due to organic-only policy");
                return false;
            }
            
            string applicationId = GenerateApplicationId(chemicalName, zoneId);
            
            var chemicalApplication = new ChemicalApplication
            {
                ApplicationId = applicationId,
                ChemicalName = chemicalName,
                ChemicalType = chemicalType,
                ZoneId = zoneId,
                Concentration = concentration,
                TargetPests = targetPests,
                ApplicationMethod = method,
                ApplicationTime = DateTime.Now,
                IsActive = true,
                InitialEffectiveness = CalculateChemicalEffectiveness(chemicalName, chemicalType, concentration),
                CurrentEffectiveness = 0.0f, // Will be calculated during update
                RemainingDuration = GetChemicalDuration(chemicalType),
                EnvironmentalImpact = CalculateEnvironmentalImpact(chemicalName, concentration),
                ResistanceRisk = CalculateResistanceRisk(chemicalType, targetPests)
            };
            
            // Apply immediate effect
            ApplyImmediateChemicalEffect(chemicalApplication);
            
            _activeChemicalApplications[applicationId] = chemicalApplication;
            
            // Track effectiveness
            var effectiveness = new ControlMethodEffectiveness
            {
                ControlId = applicationId,
                MethodType = ControlMethodType.Chemical,
                ZoneId = zoneId,
                DeploymentTime = DateTime.Now,
                InitialEffectiveness = chemicalApplication.InitialEffectiveness,
                CurrentEffectiveness = chemicalApplication.InitialEffectiveness,
                CostEffectiveness = CalculateChemicalCostEffectiveness(chemicalName, concentration),
                EnvironmentalImpact = chemicalApplication.EnvironmentalImpact,
                ResistanceRisk = chemicalApplication.ResistanceRisk
            };
            
            _controlEffectiveness[applicationId] = effectiveness;
            _totalControlsDeployed++;
            
            _onChemicalApplicationApplied?.Raise();
            
            Debug.Log($"[IPMControlMethods] Applied {chemicalName} ({chemicalType}) at {concentration}% concentration in zone {zoneId}");
            return true;
        }
        
        /// <summary>
        /// Update chemical application effectiveness and degradation
        /// </summary>
        private void UpdateChemicalControl(float deltaTime)
        {
            var applicationsToRemove = new List<string>();
            
            foreach (var kvp in _activeChemicalApplications)
            {
                var application = kvp.Value;
                
                // Degrade chemical effectiveness over time
                float degradationRate = _chemicalDegradationRate * GetDegradationMultiplier(application.ChemicalType);
                application.CurrentEffectiveness = Mathf.Max(0, application.CurrentEffectiveness - (degradationRate * deltaTime));
                
                // Reduce remaining duration
                application.RemainingDuration -= deltaTime;
                
                // Update resistance development
                UpdateResistanceDevelopment(application, deltaTime);
                
                // Calculate ongoing pest control impact
                float controlImpact = CalculateChemicalControlImpact(application);
                
                // Update effectiveness tracking
                if (_controlEffectiveness.ContainsKey(application.ApplicationId))
                {
                    var effectiveness = _controlEffectiveness[application.ApplicationId];
                    effectiveness.CurrentEffectiveness = controlImpact;
                    effectiveness.TotalPestsControlled += (int)(controlImpact * 100 * deltaTime); // Simplified calculation
                }
                
                // Remove expired applications
                if (application.RemainingDuration <= 0 || application.CurrentEffectiveness <= 0.01f)
                {
                    applicationsToRemove.Add(kvp.Key);
                    Debug.Log($"[IPMControlMethods] Chemical application {application.ChemicalName} in zone {application.ZoneId} has expired");
                }
            }
            
            // Clean up expired applications
            foreach (string applicationId in applicationsToRemove)
            {
                _activeChemicalApplications.Remove(applicationId);
                if (_controlEffectiveness.ContainsKey(applicationId))
                {
                    _controlEffectiveness[applicationId].IsActive = false;
                }
            }
        }
        
        #endregion
        
        #region Cultural Control Methods
        
        /// <summary>
        /// Implement cultural control measures to prevent and manage pest populations
        /// </summary>
        public bool ImplementCulturalControl(CulturalControlType controlType, string zoneId, 
            Dictionary<string, object> parameters)
        {
            if (!_enableCulturalControl)
            {
                Debug.LogWarning("[IPMControlMethods] Cultural control is disabled");
                return false;
            }
            
            string controlId = GenerateCulturalControlId(controlType, zoneId);
            
            var culturalControl = new CulturalControlMeasure
            {
                ControlId = controlId,
                ControlType = controlType,
                ZoneId = zoneId,
                Parameters = parameters,
                ImplementationTime = DateTime.Now,
                IsActive = true,
                Effectiveness = CalculateCulturalControlEffectiveness(controlType),
                MaintenanceRequired = GetMaintenanceRequirement(controlType),
                Duration = GetCulturalControlDuration(controlType),
                AffectedPestTypes = GetAffectedPestTypes(controlType)
            };
            
            // Apply immediate cultural control effects
            ApplyImmediateCulturalEffect(culturalControl);
            
            _activeCulturalControls[controlId] = culturalControl;
            
            // Track effectiveness
            var effectiveness = new ControlMethodEffectiveness
            {
                ControlId = controlId,
                MethodType = ControlMethodType.Cultural,
                ZoneId = zoneId,
                DeploymentTime = DateTime.Now,
                InitialEffectiveness = culturalControl.Effectiveness,
                CurrentEffectiveness = culturalControl.Effectiveness,
                CostEffectiveness = CalculateCulturalCostEffectiveness(controlType),
                EnvironmentalImpact = 0.05f, // Cultural controls have very low environmental impact
                ResistanceRisk = 0.01f // Very low resistance development risk
            };
            
            _controlEffectiveness[controlId] = effectiveness;
            _totalControlsDeployed++;
            
            _onCulturalControlImplemented?.Raise();
            
            Debug.Log($"[IPMControlMethods] Implemented {controlType} cultural control in zone {zoneId}");
            return true;
        }
        
        /// <summary>
        /// Update cultural control measures and their ongoing effects
        /// </summary>
        private void UpdateCulturalControl(float deltaTime)
        {
            var controlsToRemove = new List<string>();
            
            foreach (var kvp in _activeCulturalControls)
            {
                var control = kvp.Value;
                
                // Update duration
                control.Duration -= deltaTime;
                
                // Check maintenance requirements
                if (control.MaintenanceRequired && ShouldPerformMaintenance(control))
                {
                    PerformCulturalControlMaintenance(control);
                }
                
                // Calculate ongoing effectiveness
                float currentEffectiveness = CalculateOngoingCulturalEffectiveness(control);
                
                // Update effectiveness tracking
                if (_controlEffectiveness.ContainsKey(control.ControlId))
                {
                    var effectiveness = _controlEffectiveness[control.ControlId];
                    effectiveness.CurrentEffectiveness = currentEffectiveness;
                    effectiveness.TotalPestsControlled += (int)(currentEffectiveness * 50 * deltaTime); // Simplified calculation
                }
                
                // Remove expired controls
                if (control.Duration <= 0 && !IsPermanentControl(control.ControlType))
                {
                    controlsToRemove.Add(kvp.Key);
                    Debug.Log($"[IPMControlMethods] Cultural control {control.ControlType} in zone {control.ZoneId} has expired");
                }
            }
            
            // Clean up expired controls
            foreach (string controlId in controlsToRemove)
            {
                _activeCulturalControls.Remove(controlId);
                if (_controlEffectiveness.ContainsKey(controlId))
                {
                    _controlEffectiveness[controlId].IsActive = false;
                }
            }
        }
        
        #endregion
        
        #region Integrated Control Methods
        
        /// <summary>
        /// Implement integrated pest management strategy combining multiple control methods
        /// </summary>
        public bool ImplementIntegratedStrategy(string zoneId, IntegratedIPMStrategy strategy)
        {
            bool success = true;
            int methodsDeployed = 0;
            
            // Deploy biological controls
            foreach (var bioControl in strategy.BiologicalControls)
            {
                if (DeployBiologicalControl(bioControl.OrganismType, zoneId, bioControl.ReleaseQuantity, bioControl.TargetPests))
                {
                    methodsDeployed++;
                }
                else
                {
                    success = false;
                }
            }
            
            // Apply chemical controls
            foreach (var chemControl in strategy.ChemicalControls)
            {
                if (ApplyChemicalControl(chemControl.ChemicalName, chemControl.ChemicalType, zoneId, 
                    chemControl.Concentration, chemControl.TargetPests, chemControl.Method))
                {
                    methodsDeployed++;
                }
                else
                {
                    success = false;
                }
            }
            
            // Implement cultural controls
            foreach (var cultControl in strategy.CulturalControls)
            {
                if (ImplementCulturalControl(cultControl.ControlType, zoneId, cultControl.Parameters))
                {
                    methodsDeployed++;
                }
                else
                {
                    success = false;
                }
            }
            
            Debug.Log($"[IPMControlMethods] Integrated strategy deployed {methodsDeployed} control methods in zone {zoneId}. Success: {success}");
            return success;
        }
        
        /// <summary>
        /// Get comprehensive control effectiveness report for a zone
        /// </summary>
        public ControlEffectivenessReport GetZoneControlReport(string zoneId)
        {
            var report = new ControlEffectivenessReport
            {
                ZoneId = zoneId,
                ReportTime = DateTime.Now,
                BiologicalControls = new List<BiologicalControlAgent>(),
                ChemicalApplications = new List<ChemicalApplication>(),
                CulturalControls = new List<CulturalControlMeasure>(),
                OverallEffectiveness = 0.0f,
                EnvironmentalImpact = 0.0f,
                CostEffectiveness = 0.0f,
                ResistanceRisk = 0.0f
            };
            
            // Collect biological controls
            foreach (var agent in _activeBiologicalAgents.Values.Where(a => a.ZoneId == zoneId))
            {
                report.BiologicalControls.Add(agent);
            }
            
            // Collect chemical applications
            foreach (var application in _activeChemicalApplications.Values.Where(a => a.ZoneId == zoneId))
            {
                report.ChemicalApplications.Add(application);
            }
            
            // Collect cultural controls
            foreach (var control in _activeCulturalControls.Values.Where(c => c.ZoneId == zoneId))
            {
                report.CulturalControls.Add(control);
            }
            
            // Calculate overall metrics
            var zoneEffectiveness = _controlEffectiveness.Values.Where(e => e.ZoneId == zoneId && e.IsActive);
            
            if (zoneEffectiveness.Any())
            {
                report.OverallEffectiveness = zoneEffectiveness.Average(e => e.CurrentEffectiveness);
                report.EnvironmentalImpact = zoneEffectiveness.Average(e => e.EnvironmentalImpact);
                report.CostEffectiveness = zoneEffectiveness.Average(e => e.CostEffectiveness);
                report.ResistanceRisk = zoneEffectiveness.Average(e => e.ResistanceRisk);
            }
            
            return report;
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private void InitializeControlSystems()
        {
            _activeBiologicalAgents.Clear();
            _activeChemicalApplications.Clear();
            _activeCulturalControls.Clear();
            _controlEffectiveness.Clear();
            _totalControlsDeployed = 0;
            _overallSystemEffectiveness = 0.0f;
        }
        
        private void InitializePestResistanceProfiles()
        {
            _pestResistanceProfiles.Clear();
            
            // Initialize resistance profiles for all pest types
            foreach (PestType pestType in System.Enum.GetValues(typeof(PestType)))
            {
                _pestResistanceProfiles[pestType] = new ResistanceProfile
                {
                    PestType = pestType,
                    ChemicalResistance = new Dictionary<ChemicalType, float>(),
                    BiologicalResistance = new Dictionary<BeneficialOrganismType, float>(),
                    ResistanceDevelopmentRate = UnityEngine.Random.Range(0.01f, 0.1f)
                };
                
                // Initialize with base resistance levels
                foreach (ChemicalType chemType in System.Enum.GetValues(typeof(ChemicalType)))
                {
                    _pestResistanceProfiles[pestType].ChemicalResistance[chemType] = UnityEngine.Random.Range(0.0f, 0.2f);
                }
            }
        }
        
        private void UpdatePestResistance(float deltaTime)
        {
            foreach (var resistanceProfile in _pestResistanceProfiles.Values)
            {
                // Update chemical resistance based on active applications
                foreach (var application in _activeChemicalApplications.Values)
                {
                    if (application.TargetPests.Contains(resistanceProfile.PestType))
                    {
                        float resistanceIncrease = resistanceProfile.ResistanceDevelopmentRate * 
                                                 application.CurrentEffectiveness * deltaTime;
                        
                        if (resistanceProfile.ChemicalResistance.ContainsKey(application.ChemicalType))
                        {
                            resistanceProfile.ChemicalResistance[application.ChemicalType] = 
                                Mathf.Min(1.0f, resistanceProfile.ChemicalResistance[application.ChemicalType] + resistanceIncrease);
                        }
                    }
                }
            }
        }
        
        private void UpdateControlEffectiveness(float deltaTime)
        {
            if (_controlEffectiveness.Count == 0)
            {
                _overallSystemEffectiveness = 0.0f;
                return;
            }
            
            float totalEffectiveness = 0.0f;
            int activeControls = 0;
            
            foreach (var effectiveness in _controlEffectiveness.Values)
            {
                if (effectiveness.IsActive)
                {
                    totalEffectiveness += effectiveness.CurrentEffectiveness;
                    activeControls++;
                }
            }
            
            _overallSystemEffectiveness = activeControls > 0 ? totalEffectiveness / activeControls : 0.0f;
            
            _onControlMethodEffectivenessChanged?.Raise();
        }
        
        // Calculation helper methods
        private float CalculateHuntingEfficiency(BeneficialOrganismType organismType)
        {
            return organismType switch
            {
                BeneficialOrganismType.Ladybugs => 0.7f,
                BeneficialOrganismType.LacewingLarvae => 0.8f,
                BeneficialOrganismType.PredatoryMites => 0.6f,
                BeneficialOrganismType.Parasitoids => 0.9f,
                BeneficialOrganismType.PredatoryBeetles => 0.75f,
                _ => 0.5f
            };
        }
        
        private float CalculateChemicalEffectiveness(string chemicalName, ChemicalType chemicalType, float concentration)
        {
            float baseEffectiveness = chemicalType switch
            {
                ChemicalType.Insecticide => 0.85f,
                ChemicalType.Miticide => 0.75f,
                ChemicalType.Fungicide => 0.8f,
                ChemicalType.Bactericide => 0.7f,
                ChemicalType.Nematicide => 0.65f,
                ChemicalType.Organic => 0.6f,
                _ => 0.5f
            };
            
            return baseEffectiveness * (concentration / 100.0f) * _controlEffectivenessMultiplier;
        }
        
        private float CalculateCulturalControlEffectiveness(CulturalControlType controlType)
        {
            return controlType switch
            {
                CulturalControlType.Sanitation => _sanitationEffectiveness,
                CulturalControlType.Quarantine => _quarantineEffectiveness,
                CulturalControlType.CropRotation => _cropRotationBenefit,
                CulturalControlType.EnvironmentalModification => 0.5f,
                CulturalControlType.PhysicalBarriers => 0.8f,
                CulturalControlType.CompanionPlanting => 0.4f,
                _ => 0.3f
            };
        }
        
        // ID generation methods
        private string GenerateAgentId(BeneficialOrganismType organismType, string zoneId)
        {
            return $"BIO_{organismType}_{zoneId}_{DateTime.Now.Ticks}";
        }
        
        private string GenerateApplicationId(string chemicalName, string zoneId)
        {
            return $"CHEM_{chemicalName}_{zoneId}_{DateTime.Now.Ticks}";
        }
        
        private string GenerateCulturalControlId(CulturalControlType controlType, string zoneId)
        {
            return $"CULT_{controlType}_{zoneId}_{DateTime.Now.Ticks}";
        }
        
        // Placeholder methods for complex calculations (to be implemented based on specific requirements)
        private float GetPreyAvailability(string zoneId, List<PestType> targetPests) => 0.7f;
        private void UpdateEstablishmentStatus(BiologicalControlAgent agent) { }
        private float CalculateBiologicalControlImpact(BiologicalControlAgent agent) => agent.HuntingEfficiency * (agent.CurrentPopulation / agent.InitialPopulation);
        private void ApplyImmediateChemicalEffect(ChemicalApplication application) { }
        private float GetChemicalDuration(ChemicalType chemicalType) => 72.0f; // 3 days default
        private float CalculateEnvironmentalImpact(string chemicalName, float concentration) => concentration * 0.01f;
        private float CalculateResistanceRisk(ChemicalType chemicalType, List<PestType> targetPests) => 0.3f;
        private bool IsOrganicChemical(string chemicalName) => chemicalName.ToLower().Contains("organic");
        private float GetDegradationMultiplier(ChemicalType chemicalType) => 1.0f;
        private void UpdateResistanceDevelopment(ChemicalApplication application, float deltaTime) { }
        private float CalculateChemicalControlImpact(ChemicalApplication application) => application.CurrentEffectiveness;
        private void ApplyImmediateCulturalEffect(CulturalControlMeasure control) { }
        private bool GetMaintenanceRequirement(CulturalControlType controlType) => true;
        private float GetCulturalControlDuration(CulturalControlType controlType) => float.MaxValue;
        private List<PestType> GetAffectedPestTypes(CulturalControlType controlType) => new List<PestType>();
        private bool ShouldPerformMaintenance(CulturalControlMeasure control) => false;
        private void PerformCulturalControlMaintenance(CulturalControlMeasure control) { }
        private float CalculateOngoingCulturalEffectiveness(CulturalControlMeasure control) => control.Effectiveness;
        private bool IsPermanentControl(CulturalControlType controlType) => true;
        private float CalculateBiologicalCostEffectiveness(BeneficialOrganismType organismType, int quantity) => 0.8f;
        private float CalculateChemicalCostEffectiveness(string chemicalName, float concentration) => 0.6f;
        private float CalculateCulturalCostEffectiveness(CulturalControlType controlType) => 0.9f;
        
        #endregion
        
        #region Public API Methods
        
        /// <summary>
        /// Get system status and performance metrics
        /// </summary>
        public IPMControlSystemStatus GetSystemStatus()
        {
            return new IPMControlSystemStatus
            {
                TotalActiveBiologicalAgents = _activeBiologicalAgents.Count,
                TotalActiveChemicalApplications = _activeChemicalApplications.Count,
                TotalActiveCulturalControls = _activeCulturalControls.Count,
                TotalControlsDeployed = _totalControlsDeployed,
                OverallSystemEffectiveness = _overallSystemEffectiveness,
                BiologicalControlEnabled = _enableBiologicalControl,
                ChemicalControlEnabled = _enableChemicalControl,
                CulturalControlEnabled = _enableCulturalControl,
                LastUpdateTime = DateTime.Now
            };
        }
        
        /// <summary>
        /// Remove specific control method
        /// </summary>
        public bool RemoveControlMethod(string controlId)
        {
            bool removed = false;
            
            if (_activeBiologicalAgents.Remove(controlId))
            {
                removed = true;
                Debug.Log($"[IPMControlMethods] Removed biological control agent {controlId}");
            }
            else if (_activeChemicalApplications.Remove(controlId))
            {
                removed = true;
                Debug.Log($"[IPMControlMethods] Removed chemical application {controlId}");
            }
            else if (_activeCulturalControls.Remove(controlId))
            {
                removed = true;
                Debug.Log($"[IPMControlMethods] Removed cultural control {controlId}");
            }
            
            if (removed && _controlEffectiveness.ContainsKey(controlId))
            {
                _controlEffectiveness[controlId].IsActive = false;
            }
            
            return removed;
        }
        
        /// <summary>
        /// Get all active control methods in a specific zone
        /// </summary>
        public List<string> GetActiveControlsInZone(string zoneId)
        {
            var controls = new List<string>();
            
            controls.AddRange(_activeBiologicalAgents.Values.Where(a => a.ZoneId == zoneId).Select(a => a.AgentId));
            controls.AddRange(_activeChemicalApplications.Values.Where(a => a.ZoneId == zoneId).Select(a => a.ApplicationId));
            controls.AddRange(_activeCulturalControls.Values.Where(c => c.ZoneId == zoneId).Select(c => c.ControlId));
            
            return controls;
        }
        
        #endregion
    }
}