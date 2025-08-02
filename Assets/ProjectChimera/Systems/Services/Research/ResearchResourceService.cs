using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Research;
using ProjectChimera.Systems.Registry;
using ResourceAllocation = ProjectChimera.Data.Research.ResourceAllocation;
using ResourceBudget = ProjectChimera.Data.Research.ResourceBudget;
using ResearchFacility = ProjectChimera.Data.Research.ResearchFacility;
using FacilityUpgrade = ProjectChimera.Data.Research.FacilityUpgrade;
using ResearchEquipment = ProjectChimera.Data.Research.ResearchEquipment;
using ResourceType = ProjectChimera.Data.Research.ResourceType;
using FacilityStatus = ProjectChimera.Data.Research.FacilityStatus;
using EquipmentStatus = ProjectChimera.Data.Research.EquipmentStatus;
using FacilityType = ProjectChimera.Data.Research.FacilityType;
using EquipmentType = ProjectChimera.Data.Research.EquipmentType;

namespace ProjectChimera.Systems.Services.Research
{
    /// <summary>
    /// PC014-2d: Research Resource Service
    /// Resource allocation, budgeting, and facility management
    /// Decomposed from ResearchManager (440 lines target)
    /// </summary>
    public class ResearchResourceService : MonoBehaviour, IResearchResourceService
    {
        #region Properties
        
        public bool IsInitialized { get; private set; }
        
        #endregion

        #region Private Fields
        
        [Header("Resource Configuration")]
        [SerializeField] private bool _enableResourceManagement = true;
        [SerializeField] private bool _enforceResourceLimits = true;
        [SerializeField] private float _resourceRegenerationRate = 1f;
        [SerializeField] private float _facilityMaintenanceCost = 0.1f;
        
        [Header("Available Resources")]
        [SerializeField] private Dictionary<ResourceType, float> _availableResources = new Dictionary<ResourceType, float>();
        [SerializeField] private Dictionary<ResourceType, float> _maxResourceLimits = new Dictionary<ResourceType, float>();
        [SerializeField] private Dictionary<string, ResourceAllocation> _activeAllocations = new Dictionary<string, ResourceAllocation>();
        [SerializeField] private Dictionary<string, ResourceBudget> _projectBudgets = new Dictionary<string, ResourceBudget>();
        
        [Header("Facilities & Equipment")]
        [SerializeField] private List<ResearchFacility> _availableFacilities = new List<ResearchFacility>();
        [SerializeField] private List<ResearchEquipment> _availableEquipment = new List<ResearchEquipment>();
        [SerializeField] private Dictionary<string, List<string>> _facilityReservations = new Dictionary<string, List<string>>();
        [SerializeField] private Dictionary<string, string> _equipmentAssignments = new Dictionary<string, string>();
        
        private float _lastMaintenanceCheck;
        private const float MAINTENANCE_CHECK_INTERVAL = 24f * 60f * 60f; // 24 hours
        
        #endregion

        #region Events
        
        public event Action<string, ResourceAllocation> OnResourcesAllocated;
        public event Action<string, string> OnFacilityReserved;
        public event Action<string, string> OnEquipmentAssigned;
        public event Action<ResourceType, float> OnResourceLimitReached;
        
        #endregion

        #region IService Implementation
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            Debug.Log("Initializing ResearchResourceService...");
            
            // Initialize resource system
            InitializeResourceSystem();
            
            // Initialize facilities
            InitializeFacilities();
            
            // Initialize equipment
            InitializeEquipment();
            
            // Load existing data
            LoadExistingResourceData();
            
            // Register with ServiceRegistry
            ServiceRegistry.Instance.RegisterService<IResearchResourceService>(this, ServiceDomain.Research);
            
            IsInitialized = true;
            Debug.Log("ResearchResourceService initialized successfully");
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            Debug.Log("Shutting down ResearchResourceService...");
            
            // Save resource state
            SaveResourceState();
            
            // Clear collections
            _availableResources.Clear();
            _maxResourceLimits.Clear();
            _activeAllocations.Clear();
            _projectBudgets.Clear();
            _availableFacilities.Clear();
            _availableEquipment.Clear();
            _facilityReservations.Clear();
            _equipmentAssignments.Clear();
            
            IsInitialized = false;
            Debug.Log("ResearchResourceService shutdown complete");
        }
        
        #endregion

        #region Resource Allocation
        
        public bool AllocateResources(string projectId, ResourceAllocation allocation)
        {
            if (!IsInitialized)
            {
                Debug.LogError("ResearchResourceService not initialized");
                return false;
            }

            if (string.IsNullOrEmpty(projectId))
            {
                Debug.LogError("Project ID cannot be null or empty");
                return false;
            }

            if (allocation?.AllocatedResources == null)
            {
                Debug.LogError("Invalid resource allocation");
                return false;
            }

            // Check resource availability
            if (_enforceResourceLimits && !ValidateResourceAvailability(allocation))
            {
                Debug.LogWarning($"Insufficient resources for allocation to project {projectId}");
                return false;
            }

            // Remove existing allocation if any
            if (_activeAllocations.ContainsKey(projectId))
            {
                ReleaseAllocation(projectId);
            }

            // Consume resources
            foreach (var resource in allocation.AllocatedResources)
            {
                if (_availableResources.ContainsKey(resource.Key))
                {
                    _availableResources[resource.Key] -= resource.Value;
                    _availableResources[resource.Key] = Mathf.Max(0f, _availableResources[resource.Key]);
                }
            }

            // Store allocation
            allocation.AllocationId = GenerateAllocationId();
            allocation.ProjectId = projectId;
            allocation.AllocationDate = DateTime.Now;
            allocation.IsActive = true;
            
            _activeAllocations[projectId] = allocation;

            OnResourcesAllocated?.Invoke(projectId, allocation);
            Debug.Log($"Allocated resources to project {projectId}");
            
            return true;
        }

        public ResourceAllocation GetResourceAllocation(string projectId)
        {
            if (_activeAllocations.ContainsKey(projectId))
            {
                return _activeAllocations[projectId];
            }
            
            return null;
        }

        public float GetAvailableResources(ResourceType resourceType)
        {
            if (_availableResources.ContainsKey(resourceType))
            {
                return _availableResources[resourceType];
            }
            
            return 0f;
        }

        public void UpdateResourceBudget(string projectId, ResourceBudget budget)
        {
            if (string.IsNullOrEmpty(projectId) || budget == null)
            {
                Debug.LogError("Invalid parameters for budget update");
                return;
            }

            budget.BudgetId = GenerateBudgetId();
            budget.ProjectId = projectId;
            
            _projectBudgets[projectId] = budget;
            
            Debug.Log($"Updated budget for project {projectId}");
        }
        
        #endregion

        #region Facility Management
        
        public List<ResearchFacility> GetAvailableFacilities()
        {
            return _availableFacilities
                .Where(f => f.Status == FacilityStatus.Available)
                .ToList();
        }

        public bool ReserveFacility(string facilityId, string projectId, TimeSpan duration)
        {
            var facility = _availableFacilities.FirstOrDefault(f => f.FacilityId == facilityId);
            if (facility == null)
            {
                Debug.LogError($"Facility not found: {facilityId}");
                return false;
            }

            if (facility.Status != FacilityStatus.Available)
            {
                Debug.LogWarning($"Facility {facilityId} is not available for reservation");
                return false;
            }

            // Check capacity
            if (facility.AssignedProjects.Count >= GetFacilityCapacity(facility))
            {
                Debug.LogWarning($"Facility {facilityId} is at capacity");
                return false;
            }

            // Reserve facility
            facility.Status = FacilityStatus.Occupied;
            facility.AssignedProjects.Add(projectId);
            
            // Track reservation
            if (!_facilityReservations.ContainsKey(facilityId))
            {
                _facilityReservations[facilityId] = new List<string>();
            }
            _facilityReservations[facilityId].Add(projectId);

            OnFacilityReserved?.Invoke(facilityId, projectId);
            Debug.Log($"Reserved facility {facilityId} for project {projectId}");
            
            return true;
        }

        public FacilityStatus GetFacilityStatus(string facilityId)
        {
            var facility = _availableFacilities.FirstOrDefault(f => f.FacilityId == facilityId);
            return facility?.Status ?? FacilityStatus.Offline;
        }

        public void UpgradeFacility(string facilityId, FacilityUpgrade upgrade)
        {
            var facility = _availableFacilities.FirstOrDefault(f => f.FacilityId == facilityId);
            if (facility == null)
            {
                Debug.LogError($"Facility not found: {facilityId}");
                return;
            }

            if (facility.Status != FacilityStatus.Available)
            {
                Debug.LogError($"Cannot upgrade facility {facilityId} - not available");
                return;
            }

            // Check upgrade requirements
            if (facility.Level < upgrade.RequiredLevel)
            {
                Debug.LogError($"Facility {facilityId} does not meet upgrade requirements");
                return;
            }

            // Apply upgrade
            facility.Status = FacilityStatus.Upgrading;
            
            // TODO: Implement actual upgrade logic with timing
            ApplyFacilityUpgrade(facility, upgrade);
            
            Debug.Log($"Started upgrade for facility {facilityId}");
        }
        
        #endregion

        #region Equipment Tracking
        
        public List<ResearchEquipment> GetAvailableEquipment()
        {
            return _availableEquipment
                .Where(e => e.Status == EquipmentStatus.Available)
                .ToList();
        }

        public bool AssignEquipment(string equipmentId, string projectId)
        {
            var equipment = _availableEquipment.FirstOrDefault(e => e.EquipmentId == equipmentId);
            if (equipment == null)
            {
                Debug.LogError($"Equipment not found: {equipmentId}");
                return false;
            }

            if (equipment.Status != EquipmentStatus.Available)
            {
                Debug.LogWarning($"Equipment {equipmentId} is not available");
                return false;
            }

            // Assign equipment
            equipment.Status = EquipmentStatus.InUse;
            equipment.AssignedProject = projectId;
            
            _equipmentAssignments[equipmentId] = projectId;

            OnEquipmentAssigned?.Invoke(equipmentId, projectId);
            Debug.Log($"Assigned equipment {equipmentId} to project {projectId}");
            
            return true;
        }

        public EquipmentStatus GetEquipmentStatus(string equipmentId)
        {
            var equipment = _availableEquipment.FirstOrDefault(e => e.EquipmentId == equipmentId);
            return equipment?.Status ?? EquipmentStatus.Broken;
        }

        public void MaintenanceEquipment(string equipmentId)
        {
            var equipment = _availableEquipment.FirstOrDefault(e => e.EquipmentId == equipmentId);
            if (equipment == null)
            {
                Debug.LogError($"Equipment not found: {equipmentId}");
                return;
            }

            equipment.Status = EquipmentStatus.Maintenance;
            equipment.LastMaintenance = DateTime.Now;
            
            // TODO: Implement maintenance duration and restoration
            
            Debug.Log($"Started maintenance for equipment {equipmentId}");
        }
        
        #endregion

        #region Validation Methods
        
        public bool ValidateResources(string projectId)
        {
            if (!_activeAllocations.ContainsKey(projectId))
            {
                // Check if project has basic resource requirements
                return HasMinimumResources();
            }

            var allocation = _activeAllocations[projectId];
            return allocation.IsActive;
        }

        public void ConsumeResources(string projectId)
        {
            if (!_activeAllocations.ContainsKey(projectId))
            {
                Debug.LogWarning($"No resource allocation found for project {projectId}");
                return;
            }

            var allocation = _activeAllocations[projectId];
            
            // Resource consumption is handled during allocation
            // This method can be used for ongoing consumption
            
            Debug.Log($"Consumed resources for project {projectId}");
        }
        
        #endregion

        #region Private Helper Methods
        
        private void InitializeResourceSystem()
        {
            // Initialize resource pools
            _availableResources[ResourceType.Currency] = 100000f;
            _availableResources[ResourceType.Time] = 1000f;
            _availableResources[ResourceType.Expertise] = 500f;
            _availableResources[ResourceType.Equipment] = 100f;
            _availableResources[ResourceType.Materials] = 1000f;
            _availableResources[ResourceType.Energy] = 500f;
            _availableResources[ResourceType.Space] = 200f;

            // Set maximum limits
            _maxResourceLimits[ResourceType.Currency] = 1000000f;
            _maxResourceLimits[ResourceType.Time] = 10000f;
            _maxResourceLimits[ResourceType.Expertise] = 5000f;
            _maxResourceLimits[ResourceType.Equipment] = 1000f;
            _maxResourceLimits[ResourceType.Materials] = 10000f;
            _maxResourceLimits[ResourceType.Energy] = 5000f;
            _maxResourceLimits[ResourceType.Space] = 2000f;

            Debug.Log("Resource system initialized with base resources");
        }

        private void InitializeFacilities()
        {
            // Create basic laboratory
            var basicLab = new ResearchFacility
            {
                FacilityId = "basic_lab_001",
                Name = "Basic Research Laboratory",
                Type = FacilityType.Laboratory,
                Status = FacilityStatus.Available,
                Capabilities = new List<string> { "Basic Research", "Analysis" },
                AssignedProjects = new List<string>(),
                EfficiencyRating = 1.0f
            };
            _availableFacilities.Add(basicLab);

            // Create greenhouse facility
            var greenhouse = new ResearchFacility
            {
                FacilityId = "greenhouse_001",
                Name = "Research Greenhouse",
                Type = FacilityType.Greenhouse,
                Status = FacilityStatus.Available,
                Capabilities = new List<string> { "Plant Cultivation", "Genetic Studies" },
                AssignedProjects = new List<string>(),
                EfficiencyRating = 1.2f
            };
            _availableFacilities.Add(greenhouse);

            Debug.Log($"Initialized {_availableFacilities.Count} research facilities");
        }

        private void InitializeEquipment()
        {
            // Create basic analytical equipment
            var microscope = new ResearchEquipment
            {
                EquipmentId = "microscope_001",
                Name = "High-Power Microscope",
                Type = EquipmentType.Analytical,
                Status = EquipmentStatus.Available,
                Capabilities = new List<string> { "Microscopy", "Sample Analysis" },
                EfficiencyBonus = 1.1f,
                MaintenanceRequirement = 0.1f,
                LastMaintenance = DateTime.Now
            };
            _availableEquipment.Add(microscope);

            // Create cultivation equipment
            var growthChamber = new ResearchEquipment
            {
                EquipmentId = "growth_chamber_001",
                Name = "Controlled Growth Chamber",
                Type = EquipmentType.Cultivation,
                Status = EquipmentStatus.Available,
                Capabilities = new List<string> { "Controlled Environment", "Growth Studies" },
                EfficiencyBonus = 1.3f,
                MaintenanceRequirement = 0.2f,
                LastMaintenance = DateTime.Now
            };
            _availableEquipment.Add(growthChamber);

            Debug.Log($"Initialized {_availableEquipment.Count} research equipment items");
        }

        private void LoadExistingResourceData()
        {
            // TODO: Load from persistent storage
            Debug.Log("Loading existing resource data...");
        }

        private void SaveResourceState()
        {
            // TODO: Save to persistent storage
            Debug.Log("Saving resource state...");
        }

        private bool ValidateResourceAvailability(ResourceAllocation allocation)
        {
            foreach (var resource in allocation.AllocatedResources)
            {
                float available = GetAvailableResources(resource.Key);
                if (available < resource.Value)
                {
                    return false;
                }
            }
            return true;
        }

        private void ReleaseAllocation(string projectId)
        {
            if (!_activeAllocations.ContainsKey(projectId))
                return;

            var allocation = _activeAllocations[projectId];
            
            // Return resources
            foreach (var resource in allocation.AllocatedResources)
            {
                if (_availableResources.ContainsKey(resource.Key))
                {
                    _availableResources[resource.Key] += resource.Value;
                    
                    // Cap at maximum limit
                    if (_maxResourceLimits.ContainsKey(resource.Key))
                    {
                        _availableResources[resource.Key] = Mathf.Min(
                            _availableResources[resource.Key], 
                            _maxResourceLimits[resource.Key]
                        );
                    }
                }
            }

            _activeAllocations.Remove(projectId);
        }

        private bool HasMinimumResources()
        {
            // Check if we have minimum resources for basic operations
            return GetAvailableResources(ResourceType.Currency) >= 1000f &&
                   GetAvailableResources(ResourceType.Time) >= 10f &&
                   GetAvailableResources(ResourceType.Expertise) >= 5f;
        }

        private string GenerateAllocationId()
        {
            return $"ALLOC_{DateTime.Now:yyyyMMddHHmmss}_{UnityEngine.Random.Range(1000, 9999)}";
        }

        private string GenerateBudgetId()
        {
            return $"BUDGET_{DateTime.Now:yyyyMMddHHmmss}_{UnityEngine.Random.Range(1000, 9999)}";
        }

        private int GetFacilityCapacity(ResearchFacility facility)
        {
            return facility.Type switch
            {
                FacilityType.Laboratory => 3,
                FacilityType.Greenhouse => 5,
                FacilityType.ProcessingCenter => 2,
                FacilityType.AnalyticsCenter => 4,
                FacilityType.Library => 10,
                _ => 1
            };
        }

        private void ApplyFacilityUpgrade(ResearchFacility facility, FacilityUpgrade upgrade)
        {
            // Apply upgrade improvements
            if (upgrade.Improvements != null)
            {
                foreach (var improvement in upgrade.Improvements)
                {
                    ApplyFacilityImprovement(facility, improvement);
                }
            }

            // Complete upgrade immediately for now
            facility.Status = FacilityStatus.Available;
            
            Debug.Log($"Applied upgrade to facility {facility.FacilityId}");
        }

        private void ApplyFacilityImprovement(ResearchFacility facility, FacilityImprovement improvement)
        {
            switch (improvement.Type)
            {
                case ImprovementType.Efficiency:
                    facility.EfficiencyRating += improvement.ImprovementValue;
                    break;
                case ImprovementType.Capacity:
                    // Capacity improvements would need to be handled differently
                    break;
                default:
                    Debug.Log($"Applied {improvement.Type} improvement to {facility.Name}");
                    break;
            }
        }
        
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        private void Update()
        {
            if (!IsInitialized) return;

            // Check for maintenance needs
            if (Time.time - _lastMaintenanceCheck >= MAINTENANCE_CHECK_INTERVAL)
            {
                PerformMaintenanceChecks();
                _lastMaintenanceCheck = Time.time;
            }

            // Regenerate resources
            RegenerateResources();
        }

        private void PerformMaintenanceChecks()
        {
            foreach (var equipment in _availableEquipment)
            {
                var timeSinceLastMaintenance = DateTime.Now - equipment.LastMaintenance;
                if (timeSinceLastMaintenance.TotalHours > 168) // 1 week
                {
                    if (equipment.Status == EquipmentStatus.InUse || equipment.Status == EquipmentStatus.Available)
                    {
                        Debug.LogWarning($"Equipment {equipment.Name} requires maintenance");
                    }
                }
            }
        }

        private void RegenerateResources()
        {
            // Slowly regenerate certain resources
            float deltaTime = Time.deltaTime;
            float regenAmount = _resourceRegenerationRate * deltaTime;
            
            // Regenerate time and expertise slowly
            if (_availableResources.ContainsKey(ResourceType.Time))
            {
                _availableResources[ResourceType.Time] += regenAmount * 0.1f;
                _availableResources[ResourceType.Time] = Mathf.Min(
                    _availableResources[ResourceType.Time],
                    _maxResourceLimits[ResourceType.Time]
                );
            }
        }
        
        #endregion
    }
}