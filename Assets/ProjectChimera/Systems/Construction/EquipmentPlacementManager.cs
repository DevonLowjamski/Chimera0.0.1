using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.Construction;
using ProjectChimera.Data.Facilities;
using ProjectChimera.Data.Equipment;
// Explicit type aliases to resolve ambiguous references
using ConstructionRoomType = ProjectChimera.Data.Construction.RoomType;
using FacilitiesRoomType = ProjectChimera.Data.Facilities.RoomType;
using ConstructionEquipmentStatus = ProjectChimera.Data.Construction.EquipmentStatus;
using ConstructionMaintenanceSchedule = ProjectChimera.Data.Construction.MaintenanceSchedule;
using ConstructionComplianceStatus = ProjectChimera.Data.Construction.ComplianceStatus;

namespace ProjectChimera.Systems.Construction
{
    /// <summary>
    /// Advanced equipment placement and management system for Project Chimera.
    /// Handles strategic equipment placement, performance optimization, maintenance scheduling,
    /// and integration with room systems for optimal cannabis cultivation environments.
    /// </summary>
    public class EquipmentPlacementManager : ChimeraManager
    {
        [Header("Equipment Placement Configuration")]
        [SerializeField] private bool _enableSmartPlacement = true;
        [SerializeField] private bool _enableAutoOptimization = true;
        [SerializeField] private bool _enableMaintenanceScheduling = true;
        [SerializeField] private bool _enablePerformanceMonitoring = true;
        [SerializeField] private float _placementGridSize = 0.5f;
        [SerializeField] private float _equipmentClearanceRadius = 1.0f;
        [SerializeField] private int _maxEquipmentPerRoom = 50;
        
        [Header("Performance Optimization")]
        [SerializeField] private float _optimizationUpdateInterval = 30f;
        [SerializeField] private float _performanceUpdateInterval = 5f;
        [SerializeField] private float _maintenanceCheckInterval = 3600f; // 1 hour
        [SerializeField] private bool _enablePredictiveOptimization = true;
        [SerializeField] private float _efficiencyThreshold = 0.8f;
        
        [Header("Cannabis-Specific Settings")]
        [SerializeField] private bool _enforceGrowthStageRequirements = true;
        [SerializeField] private bool _enableEnvironmentalOptimization = true;
        [SerializeField] private float _lightCoverageMinimum = 0.95f;
        [SerializeField] private float _airflowCoverageMinimum = 0.90f;
        [SerializeField] private float _nutrientAccessRadius = 2.0f;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onEquipmentPlaced;
        [SerializeField] private SimpleGameEventSO _onEquipmentRemoved;
        [SerializeField] private SimpleGameEventSO _onMaintenanceScheduled;
        [SerializeField] private SimpleGameEventSO _onPerformanceAlert;
        [SerializeField] private SimpleGameEventSO _onOptimizationCompleted;
        
        // Core equipment management
        private Dictionary<string, List<PlacedEquipment>> _roomEquipment = new Dictionary<string, List<PlacedEquipment>>();
        private Dictionary<string, EquipmentLayout> _roomLayouts = new Dictionary<string, EquipmentLayout>();
        private Dictionary<string, EquipmentPerformanceData> _equipmentPerformance = new Dictionary<string, EquipmentPerformanceData>();
        private Dictionary<string, ConstructionMaintenanceSchedule> _maintenanceSchedules = new Dictionary<string, ConstructionMaintenanceSchedule>();
        
        // Placement and optimization systems
        private EquipmentPlacementOptimizer _placementOptimizer;
        private SmartPlacementAlgorithm _smartPlacement;
        private EquipmentPerformanceMonitor _performanceMonitor;
        private MaintenanceScheduler _maintenanceScheduler;
        
        // Cannabis-specific systems
        private CannabisEquipmentOptimizer _cannabisOptimizer;
        private GrowthStageEquipmentManager _growthStageManager;
        private EnvironmentalEquipmentCoordinator _environmentalCoordinator;
        
        // Runtime tracking
        private Dictionary<string, EquipmentNetwork> _equipmentNetworks = new Dictionary<string, EquipmentNetwork>();
        private List<EquipmentPlacementTask> _placementQueue = new List<EquipmentPlacementTask>();
        private EquipmentPlacementMetrics _placementMetrics;
        
        // Performance timing
        private float _lastOptimizationUpdate = 0f;
        private float _lastPerformanceUpdate = 0f;
        private float _lastMaintenanceCheck = 0f;
        
        // Events
        public System.Action<PlacedEquipment> OnEquipmentPlaced;
        public System.Action<PlacedEquipment> OnEquipmentRemoved;
        public System.Action<string, EquipmentLayout> OnLayoutOptimized;
        public System.Action<string, ConstructionMaintenanceSchedule> OnMaintenanceScheduled;
        public System.Action<EquipmentPerformanceData> OnPerformanceAlert;
        public System.Action<string, OptimizationResult> OnOptimizationCompleted;
        public System.Action<string, EquipmentNetwork> OnNetworkUpdated;
        
        // Properties
        public override ManagerPriority Priority => ManagerPriority.High;
        public int TotalEquipmentCount => _roomEquipment.Values.Sum(list => list.Count);
        public int ActiveRoomsWithEquipment => _roomEquipment.Count;
        public float AverageRoomUtilization => CalculateAverageRoomUtilization();
        public EquipmentPlacementMetrics PlacementMetrics => _placementMetrics;
        public Dictionary<string, List<PlacedEquipment>> RoomEquipment => _roomEquipment;
        
        protected override void OnManagerInitialize()
        {
            InitializeEquipmentSystems();
            InitializeCannabisOptimization();
            InitializePerformanceMonitoring();
            InitializeMaintenanceScheduling();
            
            _placementMetrics = new EquipmentPlacementMetrics();
            
            LogInfo("EquipmentPlacementManager initialized successfully");
        }
        
        protected override void OnManagerUpdate()
        {
            float currentTime = Time.time;
            
            UpdatePlacementQueue();
            
            if (currentTime - _lastPerformanceUpdate >= _performanceUpdateInterval)
            {
                UpdatePerformanceMonitoring();
                _lastPerformanceUpdate = currentTime;
            }
            
            if (currentTime - _lastOptimizationUpdate >= _optimizationUpdateInterval)
            {
                UpdateOptimizationSystems();
                _lastOptimizationUpdate = currentTime;
            }
            
            if (currentTime - _lastMaintenanceCheck >= _maintenanceCheckInterval)
            {
                UpdateMaintenanceScheduling();
                _lastMaintenanceCheck = currentTime;
            }
            
            UpdateMetrics();
        }
        
        protected override void OnManagerShutdown()
        {
            // Cleanup all equipment systems
            foreach (var equipment in _roomEquipment.Values.SelectMany(list => list))
            {
                CleanupEquipment(equipment);
            }
            
            _roomEquipment.Clear();
            _roomLayouts.Clear();
            _equipmentPerformance.Clear();
            _maintenanceSchedules.Clear();
            _equipmentNetworks.Clear();
            _placementQueue.Clear();
            
            LogInfo("EquipmentPlacementManager shutdown completed");
        }
        
        /// <summary>
        /// Place equipment in a room with intelligent positioning
        /// </summary>
        public PlacedEquipment PlaceEquipment(string roomId, EquipmentDataSO equipmentData, Vector3 position, Quaternion rotation = default)
        {
            if (!ValidateEquipmentPlacement(roomId, equipmentData, position))
            {
                LogWarning($"Equipment placement validation failed for {equipmentData.EquipmentName} in room {roomId}");
                return null;
            }
            
            var placedEquipment = CreatePlacedEquipment(equipmentData, position, rotation);
            placedEquipment.RoomId = roomId;
            
            // Add to room equipment list
            if (!_roomEquipment.ContainsKey(roomId))
            {
                _roomEquipment[roomId] = new List<PlacedEquipment>();
            }
            _roomEquipment[roomId].Add(placedEquipment);
            
            // Initialize equipment systems
            InitializeEquipmentInstance(placedEquipment);
            
            // Update room layout and networks
            UpdateRoomLayout(roomId);
            UpdateEquipmentNetwork(roomId);
            
            // Trigger optimization if enabled
            if (_enableAutoOptimization)
            {
                OptimizeRoomEquipment(roomId);
            }
            
            // Trigger events
            OnEquipmentPlaced?.Invoke(placedEquipment);
            _onEquipmentPlaced?.Raise();
            
            LogInfo($"Placed equipment: {equipmentData.EquipmentName} in room {roomId}");
            return placedEquipment;
        }
        
        /// <summary>
        /// Remove equipment from a room
        /// </summary>
        public bool RemoveEquipment(string equipmentId)
        {
            var equipment = FindEquipmentById(equipmentId);
            if (equipment == null)
            {
                LogError($"Equipment not found: {equipmentId}");
                return false;
            }
            
            string roomId = equipment.RoomId;
            
            // Remove from room equipment list
            if (_roomEquipment.ContainsKey(roomId))
            {
                _roomEquipment[roomId].Remove(equipment);
                if (_roomEquipment[roomId].Count == 0)
                {
                    _roomEquipment.Remove(roomId);
                }
            }
            
            // Cleanup equipment systems
            CleanupEquipment(equipment);
            
            // Update room layout and networks
            UpdateRoomLayout(roomId);
            UpdateEquipmentNetwork(roomId);
            
            // Trigger events
            OnEquipmentRemoved?.Invoke(equipment);
            _onEquipmentRemoved?.Raise();
            
            LogInfo($"Removed equipment: {equipment.EquipmentName} from room {roomId}");
            return true;
        }
        
        /// <summary>
        /// Get optimal placement position for equipment in a room
        /// </summary>
        public Vector3 GetOptimalPlacementPosition(string roomId, EquipmentDataSO equipmentData)
        {
            if (!_enableSmartPlacement)
            {
                return Vector3.zero;
            }
            
            var room = GetRoomReference(roomId);
            if (room == null)
            {
                LogError($"Room not found: {roomId}");
                return Vector3.zero;
            }
            
            var existingEquipment = _roomEquipment.GetValueOrDefault(roomId, new List<PlacedEquipment>());
            
            return _smartPlacement.CalculateOptimalPosition(room, equipmentData, existingEquipment);
        }
        
        /// <summary>
        /// Optimize equipment layout for a specific room
        /// </summary>
        public OptimizationResult OptimizeRoomEquipment(string roomId)
        {
            if (!_roomEquipment.ContainsKey(roomId))
            {
                LogWarning($"No equipment found in room: {roomId}");
                return null;
            }
            
            var equipment = _roomEquipment[roomId];
            var room = GetRoomReference(roomId);
            if (room == null)
            {
                LogError($"Room reference not found: {roomId}");
                return null;
            }
            
            var optimizationResult = _placementOptimizer.OptimizeLayout(room, equipment);
            
            if (optimizationResult.IsSuccessful)
            {
                ApplyOptimization(roomId, optimizationResult);
                OnOptimizationCompleted?.Invoke(roomId, optimizationResult);
                _onOptimizationCompleted?.Raise();
            }
            
            return optimizationResult;
        }
        
        /// <summary>
        /// Get equipment performance data for a room
        /// </summary>
        public RoomEquipmentPerformance GetRoomPerformance(string roomId)
        {
            if (!_roomEquipment.ContainsKey(roomId))
            {
                return null;
            }
            
            var equipment = _roomEquipment[roomId];
            var performanceData = equipment.Select(eq => _equipmentPerformance.GetValueOrDefault(eq.EquipmentId))
                                            .Where(data => data != null)
                                            .ToList();
            
            return new RoomEquipmentPerformance
            {
                RoomId = roomId,
                EquipmentCount = equipment.Count,
                AverageEfficiency = performanceData.Average(data => data.Efficiency),
                PowerConsumption = performanceData.Sum(data => data.PowerConsumption),
                MaintenanceAlerts = performanceData.Count(data => data.RequiresMaintenance),
                PerformanceScore = CalculateRoomPerformanceScore(performanceData),
                CoverageMetrics = CalculateRoomCoverage(roomId),
                LastUpdated = DateTime.Now
            };
        }
        
        /// <summary>
        /// Schedule maintenance for equipment
        /// </summary>
        public ConstructionMaintenanceSchedule ScheduleMaintenance(string equipmentId, MaintenanceType maintenanceType, DateTime scheduledDate)
        {
            var equipment = FindEquipmentById(equipmentId);
            if (equipment == null)
            {
                LogError($"Equipment not found for maintenance scheduling: {equipmentId}");
                return null;
            }
            
            var schedule = new ConstructionMaintenanceSchedule
            {
                ScheduleId = Guid.NewGuid().ToString(),
                EquipmentId = equipmentId,
                MaintenanceType = maintenanceType,
                ScheduledDate = scheduledDate,
                Status = MaintenanceStatus.Scheduled,
                Priority = DetermineMaintenancePriority(equipment, maintenanceType),
                EstimatedDuration = EstimateMaintenanceDuration(equipment, maintenanceType),
                CreatedDate = DateTime.Now
            };
            
            _maintenanceSchedules[schedule.ScheduleId] = schedule;
            
            OnMaintenanceScheduled?.Invoke(equipmentId, schedule);
            _onMaintenanceScheduled?.Raise();
            
            LogInfo($"Scheduled {maintenanceType} maintenance for equipment {equipment.EquipmentName}");
            return schedule;
        }
        
        /// <summary>
        /// Get comprehensive equipment information for a room
        /// </summary>
        public RoomEquipmentInfo GetRoomEquipmentInfo(string roomId)
        {
            if (!_roomEquipment.ContainsKey(roomId))
            {
                return null;
            }
            
            var equipment = _roomEquipment[roomId];
            var layout = _roomLayouts.GetValueOrDefault(roomId);
            var network = _equipmentNetworks.GetValueOrDefault(roomId);
            var performance = GetRoomPerformance(roomId);
            
            return new RoomEquipmentInfo
            {
                RoomId = roomId,
                Equipment = equipment,
                Layout = layout,
                Network = network,
                Performance = performance,
                MaintenanceSchedules = GetRoomMaintenanceSchedules(roomId),
                OptimizationOpportunities = IdentifyOptimizationOpportunities(roomId),
                ComplianceStatus = CheckEquipmentCompliance(roomId),
                LastUpdated = DateTime.Now
            };
        }
        
        #region Private Implementation
        
        private void InitializeEquipmentSystems()
        {
            _placementOptimizer = new EquipmentPlacementOptimizer();
            _smartPlacement = new SmartPlacementAlgorithm();
            _performanceMonitor = new EquipmentPerformanceMonitor();
            _maintenanceScheduler = new MaintenanceScheduler();
        }
        
        private void InitializeCannabisOptimization()
        {
            _cannabisOptimizer = new CannabisEquipmentOptimizer();
            _growthStageManager = new GrowthStageEquipmentManager();
            _environmentalCoordinator = new EnvironmentalEquipmentCoordinator();
        }
        
        private void InitializePerformanceMonitoring()
        {
            // Initialize performance monitoring systems
        }
        
        private void InitializeMaintenanceScheduling()
        {
            // Initialize maintenance scheduling systems
        }
        
        private void UpdatePlacementQueue()
        {
            foreach (var task in _placementQueue.ToList())
            {
                if (ProcessPlacementTask(task))
                {
                    _placementQueue.Remove(task);
                }
            }
        }
        
        private void UpdatePerformanceMonitoring()
        {
            foreach (var roomEquipment in _roomEquipment)
            {
                foreach (var equipment in roomEquipment.Value)
                {
                    UpdateEquipmentPerformance(equipment);
                }
            }
        }
        
        private void UpdateOptimizationSystems()
        {
            if (_enableAutoOptimization)
            {
                foreach (var roomId in _roomEquipment.Keys)
                {
                    var performance = GetRoomPerformance(roomId);
                    if (performance != null && performance.PerformanceScore < _efficiencyThreshold)
                    {
                        OptimizeRoomEquipment(roomId);
                    }
                }
            }
        }
        
        private void UpdateMaintenanceScheduling()
        {
            _maintenanceScheduler.UpdateSchedules(_maintenanceSchedules);
        }
        
        private void UpdateMetrics()
        {
            _placementMetrics.TotalEquipmentPlaced = TotalEquipmentCount;
            _placementMetrics.ActiveRooms = ActiveRoomsWithEquipment;
            _placementMetrics.AverageUtilization = AverageRoomUtilization;
            _placementMetrics.LastUpdated = DateTime.Now;
        }
        
        private bool ValidateEquipmentPlacement(string roomId, EquipmentDataSO equipmentData, Vector3 position)
        {
            // Validate room exists
            var room = GetRoomReference(roomId);
            if (room == null)
            {
                return false;
            }
            
            // Check room capacity
            var currentCount = _roomEquipment.GetValueOrDefault(roomId, new List<PlacedEquipment>()).Count;
            if (currentCount >= _maxEquipmentPerRoom)
            {
                LogWarning($"Room {roomId} has reached maximum equipment capacity");
                return false;
            }
            
            // Check position validity
            if (!IsValidPlacementPosition(roomId, position, equipmentData))
            {
                return false;
            }
            
            // Check cannabis-specific requirements
            if (_enforceGrowthStageRequirements)
            {
                if (!ValidateGrowthStageRequirements(roomId, equipmentData))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private PlacedEquipment CreatePlacedEquipment(EquipmentDataSO equipmentData, Vector3 position, Quaternion rotation)
        {
            return new PlacedEquipment
            {
                EquipmentId = Guid.NewGuid().ToString(),
                EquipmentName = equipmentData.EquipmentName,
                EquipmentType = equipmentData.EquipmentType,
                Position = position,
                Rotation = rotation,
                Status = ConstructionEquipmentStatus.Active,
                InstallationDate = DateTime.Now,
                PowerConsumption = equipmentData.PowerConsumption,
                EfficiencyRating = equipmentData.EfficiencyRating,
                MaintenanceInterval = equipmentData.MaintenanceInterval,
                LastMaintenanceDate = DateTime.Now,
                OperationalParameters = new Dictionary<string, float>(),
                PerformanceMetrics = new EquipmentPerformanceMetrics()
            };
        }
        
        private void InitializeEquipmentInstance(PlacedEquipment equipment)
        {
            // Initialize equipment performance tracking
            _equipmentPerformance[equipment.EquipmentId] = new EquipmentPerformanceData
            {
                EquipmentId = equipment.EquipmentId,
                Efficiency = equipment.EfficiencyRating,
                PowerConsumption = equipment.PowerConsumption,
                OperationalHours = 0f,
                RequiresMaintenance = false,
                LastUpdated = DateTime.Now
            };
        }
        
        private void CleanupEquipment(PlacedEquipment equipment)
        {
            _equipmentPerformance.Remove(equipment.EquipmentId);
            
            // Remove from maintenance schedules
            var schedulesToRemove = _maintenanceSchedules.Where(kvp => kvp.Value.EquipmentId == equipment.EquipmentId).ToList();
            foreach (var schedule in schedulesToRemove)
            {
                _maintenanceSchedules.Remove(schedule.Key);
            }
        }
        
        private void UpdateRoomLayout(string roomId)
        {
            var equipment = _roomEquipment.GetValueOrDefault(roomId, new List<PlacedEquipment>());
            var room = GetRoomReference(roomId);
            if (room == null) return;
            
            _roomLayouts[roomId] = new EquipmentLayout
            {
                RoomId = roomId,
                EquipmentPositions = equipment.ToDictionary(eq => eq.EquipmentId, eq => eq.Position),
                LayoutEfficiency = CalculateLayoutEfficiency(equipment),
                CoverageMetrics = CalculateRoomCoverage(roomId),
                LastUpdated = DateTime.Now
            };
        }
        
        private void UpdateEquipmentNetwork(string roomId)
        {
            var equipment = _roomEquipment.GetValueOrDefault(roomId, new List<PlacedEquipment>());
            
            _equipmentNetworks[roomId] = new EquipmentNetwork
            {
                RoomId = roomId,
                NetworkNodes = equipment.Select(eq => new NetworkNode
                {
                    EquipmentId = eq.EquipmentId,
                    Position = eq.Position,
                    ConnectionStrength = CalculateConnectionStrength(eq, equipment),
                    NetworkRole = DetermineNetworkRole(eq)
                }).ToList(),
                NetworkEfficiency = CalculateNetworkEfficiency(equipment),
                LastUpdated = DateTime.Now
            };
        }
        
        private PlacedEquipment FindEquipmentById(string equipmentId)
        {
            return _roomEquipment.Values.SelectMany(list => list)
                                        .FirstOrDefault(eq => eq.EquipmentId == equipmentId);
        }
        
        private Room GetRoomReference(string roomId)
        {
            // This would integrate with RoomCreationManager to get room data
            return null; // Placeholder implementation
        }
        
        private float CalculateAverageRoomUtilization()
        {
            if (_roomEquipment.Count == 0) return 0f;
            
            return _roomEquipment.Values.Average(equipment => equipment.Count / (float)_maxEquipmentPerRoom);
        }
        
        private bool IsValidPlacementPosition(string roomId, Vector3 position, EquipmentDataSO equipmentData)
        {
            var existingEquipment = _roomEquipment.GetValueOrDefault(roomId, new List<PlacedEquipment>());
            
            // Check clearance from other equipment
            foreach (var equipment in existingEquipment)
            {
                float distance = Vector3.Distance(position, equipment.Position);
                if (distance < _equipmentClearanceRadius)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private bool ValidateGrowthStageRequirements(string roomId, EquipmentDataSO equipmentData)
        {
            return _growthStageManager.ValidateEquipmentForRoom(roomId, equipmentData);
        }
        
        private bool ProcessPlacementTask(EquipmentPlacementTask task)
        {
            // Process placement task logic
            return true; // Placeholder
        }
        
        private void UpdateEquipmentPerformance(PlacedEquipment equipment)
        {
            var performanceData = _equipmentPerformance.GetValueOrDefault(equipment.EquipmentId);
            if (performanceData != null)
            {
                _performanceMonitor.UpdatePerformance(equipment, performanceData);
            }
        }
        
        private void ApplyOptimization(string roomId, OptimizationResult result)
        {
            // Apply optimization results to equipment layout
        }
        
        private float CalculateRoomPerformanceScore(List<EquipmentPerformanceData> performanceData)
        {
            if (performanceData.Count == 0) return 0f;
            return performanceData.Average(data => data.Efficiency);
        }
        
        private CoverageMetrics CalculateRoomCoverage(string roomId)
        {
            return new CoverageMetrics
            {
                LightCoverage = 0.95f,
                AirflowCoverage = 0.90f,
                NutrientAccess = 0.85f,
                MonitoringCoverage = 0.98f
            };
        }
        
        private List<ConstructionMaintenanceSchedule> GetRoomMaintenanceSchedules(string roomId)
        {
            var roomEquipment = _roomEquipment.GetValueOrDefault(roomId, new List<PlacedEquipment>());
            var equipmentIds = roomEquipment.Select(eq => eq.EquipmentId).ToHashSet();
            
            return _maintenanceSchedules.Values
                                       .Where(schedule => equipmentIds.Contains(schedule.EquipmentId))
                                       .ToList();
        }
        
        private List<OptimizationOpportunity> IdentifyOptimizationOpportunities(string roomId)
        {
            return new List<OptimizationOpportunity>();
        }
        
        private ConstructionComplianceStatus CheckEquipmentCompliance(string roomId)
        {
            return new ConstructionComplianceStatus { IsCompliant = true };
        }
        
        private float CalculateLayoutEfficiency(List<PlacedEquipment> equipment)
        {
            return equipment.Count > 0 ? equipment.Average(eq => eq.EfficiencyRating) : 0f;
        }
        
        private float CalculateConnectionStrength(PlacedEquipment equipment, List<PlacedEquipment> allEquipment)
        {
            return 1.0f; // Placeholder
        }
        
        private NetworkRole DetermineNetworkRole(PlacedEquipment equipment)
        {
            return NetworkRole.Node; // Placeholder
        }
        
        private float CalculateNetworkEfficiency(List<PlacedEquipment> equipment)
        {
            return equipment.Count > 0 ? equipment.Average(eq => eq.EfficiencyRating) : 0f;
        }
        
        private MaintenancePriority DetermineMaintenancePriority(PlacedEquipment equipment, MaintenanceType maintenanceType)
        {
            return MaintenancePriority.Normal;
        }
        
        private int EstimateMaintenanceDuration(PlacedEquipment equipment, MaintenanceType maintenanceType)
        {
            return 2; // Hours
        }
        
        #endregion
    }
}