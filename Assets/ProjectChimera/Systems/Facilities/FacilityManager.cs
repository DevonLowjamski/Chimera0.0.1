using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Facilities;
using ProjectChimera.Data.Environment;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Systems.Facilities
{
    /// <summary>
    /// Comprehensive facility management system for cultivation facility construction,
    /// equipment deployment, and operational management. Handles complex facility
    /// planning, construction simulation, and equipment optimization.
    /// </summary>
    public class FacilityManager : ChimeraManager
    {
        [Header("Facility Management")]
        [SerializeField] private bool _enableRealTimeConstruction = true;
        [SerializeField] private bool _enableEquipmentOptimization = true;
        [SerializeField] private bool _enableUtilityManagement = true;
        [SerializeField] private float _constructionUpdateFrequency = 1f; // Updates per second
        
        [Header("Construction Parameters")]
        [SerializeField] private float _constructionSpeedMultiplier = 1f;
        [SerializeField] private bool _requirePermits = true;
        [SerializeField] private bool _requireInspections = true;
        [SerializeField] private float _qualityControlThreshold = 0.95f;
        
        [Header("Equipment Management")]
        [SerializeField] private int _maxEquipmentPerRoom = 50;
        [SerializeField] private bool _enableAutomaticMaintenance = true;
        [SerializeField] private float _equipmentEfficiencyThreshold = 0.8f;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onFacilityConstructed;
        [SerializeField] private SimpleGameEventSO _onEquipmentInstalled;
        [SerializeField] private SimpleGameEventSO _onMaintenanceRequired;
        [SerializeField] private SimpleGameEventSO _onUtilityAlert;
        
        // Facility Management Data
        private Dictionary<string, FacilityInstance> _facilities = new Dictionary<string, FacilityInstance>();
        private Dictionary<string, ConstructionProject> _activeConstructionProjects = new Dictionary<string, ConstructionProject>();
        private Dictionary<string, EquipmentInstance> _equipmentRegistry = new Dictionary<string, EquipmentInstance>();
        private Dictionary<string, MaintenanceSchedule> _maintenanceSchedules = new Dictionary<string, MaintenanceSchedule>();
        private FacilityOperationsData _operationsData = new FacilityOperationsData();
        private float _lastConstructionUpdate;
        
        public override ManagerPriority Priority => ManagerPriority.High;
        
        // Public Properties
        public int ActiveFacilities => _facilities.Count;
        public int ActiveConstructionProjects => _activeConstructionProjects.Count;
        public int RegisteredEquipment => _equipmentRegistry.Count;
        public bool EnableRealTimeConstruction => _enableRealTimeConstruction;
        public FacilityOperationsData OperationsData => _operationsData;
        
        protected override void OnManagerInitialize()
        {
            _lastConstructionUpdate = Time.time;
            LogInfo("FacilityManager initialized for advanced cultivation facility management");
        }
        
        protected override void OnManagerUpdate()
        {
            float currentTime = Time.time;
            
            // Construction progress updates
            if (_enableRealTimeConstruction && currentTime - _lastConstructionUpdate >= 1f / _constructionUpdateFrequency)
            {
                UpdateConstructionProjects();
                ProcessMaintenanceSchedules();
                UpdateEquipmentOperations();
                _lastConstructionUpdate = currentTime;
            }
            
            // Monitor facility operations
            MonitorFacilityOperations();
        }
        
        /// <summary>
        /// Creates a new facility based on configuration template.
        /// </summary>
        public string CreateFacility(string facilityName, FacilityConfigSO config, Vector3 position)
        {
            var facility = config.CreateFacilityInstance(facilityName, position);
            _facilities[facility.FacilityId] = facility;
            
            // Start construction project if needed
            if (_enableRealTimeConstruction)
            {
                StartConstructionProject(facility, config);
            }
            else
            {
                // Instant completion for simplified gameplay
                CompleteFacilityConstruction(facility.FacilityId);
            }
            
            LogInfo($"Created facility '{facilityName}' with ID {facility.FacilityId}");
            return facility.FacilityId;
        }
        
        /// <summary>
        /// Starts construction project for a facility.
        /// </summary>
        public string StartConstructionProject(FacilityInstance facility, FacilityConfigSO config)
        {
            var project = new ConstructionProject
            {
                ProjectId = System.Guid.NewGuid().ToString(),
                FacilityId = facility.FacilityId,
                ProjectName = $"Construction of {facility.FacilityName}",
                EstimatedDays = config.EstimateConstructionDays(),
                EstimatedCost = config.EstimatedConstructionCost,
                Status = ConstructionStatus.Planned,
                StartDate = System.DateTime.Now,
                RequiredPermits = config.RegulatoryRequirements.RequiresSpecialPermits,
                RequiredInspections = config.RegulatoryRequirements.RequiresInspections,
                Progress = 0f
            };
            
            // Create construction phases
            project.ConstructionPhases = CreateConstructionPhases(facility, config);
            
            _activeConstructionProjects[project.ProjectId] = project;
            facility.ConstructionStarted = System.DateTime.Now;
            
            LogInfo($"Started construction project for facility {facility.FacilityName}");
            return project.ProjectId;
        }
        
        /// <summary>
        /// Installs equipment in a specific room.
        /// </summary>
        public string InstallEquipment(string facilityId, string roomId, FacilityEquipmentDataSO equipmentData, Vector3 position)
        {
            if (!_facilities.TryGetValue(facilityId, out var facility))
            {
                LogWarning($"Facility {facilityId} not found for equipment installation");
                return null;
            }
            
            var room = facility.Rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room == null)
            {
                LogWarning($"Room {roomId} not found in facility {facilityId}");
                return null;
            }
            
            // Check room capacity
            if (room.Equipment.Count >= _maxEquipmentPerRoom)
            {
                LogWarning($"Room {roomId} has reached maximum equipment capacity");
                return null;
            }
            
            // Create equipment instance
            var equipment = new EquipmentInstance
            {
                EquipmentId = System.Guid.NewGuid().ToString(),
                EquipmentName = equipmentData.EquipmentName,
                EquipmentData = equipmentData,
                Position = position,
                Status = EquipmentStatus.Offline,
                InstallationDate = System.DateTime.Now,
                LastMaintenance = System.DateTime.Now,
                PowerConsumption = equipmentData.PowerConsumption,
                EfficiencyRating = 1f,
                Schedule = new EquipmentSchedule()
            };
            
            // Add to room and registry
            room.Equipment.Add(equipment);
            _equipmentRegistry[equipment.EquipmentId] = equipment;
            
            // Update room power consumption
            room.PowerConsumption += equipment.PowerConsumption;
            
            // Create maintenance schedule
            CreateMaintenanceSchedule(equipment);
            
            _onEquipmentInstalled?.Raise();
            LogInfo($"Installed {equipmentData.EquipmentName} in room {room.RoomName}");
            
            return equipment.EquipmentId;
        }
        
        /// <summary>
        /// Configures room automation and environmental targets.
        /// </summary>
        public void ConfigureRoomAutomation(string facilityId, string roomId, EnvironmentalTargets targets)
        {
            if (!_facilities.TryGetValue(facilityId, out var facility))
                return;
            
            var room = facility.Rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room?.EnvironmentController == null)
                return;
            
            room.EnvironmentController.Targets = targets;
            room.EnvironmentController.AutomationEnabled = true;
            room.EnvironmentController.Status = ControllerStatus.Active;
            
            // Configure equipment for automation
            foreach (var equipment in room.Equipment.Where(e => e.EquipmentData.HasAutomation))
            {
                ConfigureEquipmentForAutomation(equipment, targets);
            }
            
            LogInfo($"Configured automation for room {room.RoomName}");
        }
        
        /// <summary>
        /// Optimizes equipment operation for energy efficiency and performance.
        /// </summary>
        public EquipmentOptimizationResult OptimizeEquipmentOperation(string facilityId, string roomId)
        {
            if (!_facilities.TryGetValue(facilityId, out var facility))
                return null;
            
            var room = facility.Rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room == null)
                return null;
            
            var optimization = new EquipmentOptimizationResult
            {
                RoomId = roomId,
                Timestamp = System.DateTime.Now,
                OriginalPowerConsumption = room.PowerConsumption,
                OptimizationActions = new List<string>()
            };
            
            float totalSavings = 0f;
            
            // Optimize lighting systems
            var lightingEquipment = room.Equipment.Where(e => e.EquipmentData.Category == EquipmentCategory.Lighting);
            foreach (var light in lightingEquipment)
            {
                float savings = OptimizeLightingEquipment(light, room.EnvironmentController?.Targets);
                totalSavings += savings;
                if (savings > 0)
                {
                    optimization.OptimizationActions.Add($"Optimized {light.EquipmentName}: {savings:F1}W saved");
                }
            }
            
            // Optimize HVAC systems
            var hvacEquipment = room.Equipment.Where(e => e.EquipmentData.Category == EquipmentCategory.HVAC);
            foreach (var hvac in hvacEquipment)
            {
                float savings = OptimizeHVACEquipment(hvac, room.EnvironmentController?.Targets);
                totalSavings += savings;
                if (savings > 0)
                {
                    optimization.OptimizationActions.Add($"Optimized {hvac.EquipmentName}: {savings:F1}W saved");
                }
            }
            
            // Update room power consumption
            room.PowerConsumption -= totalSavings;
            optimization.OptimizedPowerConsumption = room.PowerConsumption;
            optimization.EnergySavings = totalSavings;
            optimization.EfficiencyGain = totalSavings / optimization.OriginalPowerConsumption;
            
            LogInfo($"Equipment optimization completed for room {room.RoomName}: {totalSavings:F1}W saved");
            return optimization;
        }
        
        /// <summary>
        /// Gets comprehensive facility status and metrics.
        /// </summary>
        public FacilityStatusReport GetFacilityStatus(string facilityId)
        {
            if (!_facilities.TryGetValue(facilityId, out var facility))
                return null;
            
            var report = new FacilityStatusReport
            {
                FacilityId = facilityId,
                FacilityName = facility.FacilityName,
                Timestamp = System.DateTime.Now,
                IsOperational = facility.IsOperational,
                ConstructionProgress = facility.ConstructionProgress,
                TotalRooms = facility.Rooms.Count,
                OperationalRooms = facility.Rooms.Count(r => r.ConstructionStatus == ConstructionStatus.Operational),
                TotalEquipment = facility.Rooms.Sum(r => r.Equipment.Count),
                OnlineEquipment = facility.Rooms.Sum(r => r.Equipment.Count(e => e.Status == EquipmentStatus.Online)),
                TotalPowerConsumption = facility.Rooms.Sum(r => r.PowerConsumption),
                MaintenanceAlerts = GetMaintenanceAlerts(facilityId)
            };
            
            // Calculate efficiency metrics
            report.OverallEfficiency = CalculateFacilityEfficiency(facility);
            report.EquipmentUptime = report.TotalEquipment > 0 ? (float)report.OnlineEquipment / report.TotalEquipment : 0f;
            
            return report;
        }
        
        /// <summary>
        /// Performs emergency shutdown of facility systems.
        /// </summary>
        public void EmergencyShutdown(string facilityId, string reason)
        {
            if (!_facilities.TryGetValue(facilityId, out var facility))
                return;
            
            foreach (var room in facility.Rooms)
            {
                if (room.EnvironmentController != null)
                {
                    room.EnvironmentController.Status = ControllerStatus.Emergency;
                }
                
                foreach (var equipment in room.Equipment)
                {
                    equipment.Status = EquipmentStatus.Offline;
                    equipment.OperationalLevel = 0f;
                }
            }
            
            facility.IsOperational = false;
            
            LogWarning($"Emergency shutdown initiated for facility {facility.FacilityName}: {reason}");
            _onUtilityAlert?.Raise();
        }
        
        private void UpdateConstructionProjects()
        {
            var completedProjects = new List<string>();
            
            foreach (var project in _activeConstructionProjects.Values)
            {
                if (project.Status == ConstructionStatus.Completed)
                    continue;
                
                // Update construction progress
                float progressPerDay = 1f / project.EstimatedDays;
                float progressPerSecond = progressPerDay / 86400f; // Convert to per second
                project.Progress += progressPerSecond * _constructionSpeedMultiplier;
                
                // Update facility construction progress
                if (_facilities.TryGetValue(project.FacilityId, out var facility))
                {
                    facility.ConstructionProgress = project.Progress;
                }
                
                // Check for completion
                if (project.Progress >= 1f)
                {
                    project.Status = ConstructionStatus.Completed;
                    project.CompletionDate = System.DateTime.Now;
                    CompleteFacilityConstruction(project.FacilityId);
                    completedProjects.Add(project.ProjectId);
                    
                    _onFacilityConstructed?.Raise();
                }
            }
            
            // Remove completed projects
            foreach (var projectId in completedProjects)
            {
                _activeConstructionProjects.Remove(projectId);
            }
        }
        
        private void ProcessMaintenanceSchedules()
        {
            foreach (var schedule in _maintenanceSchedules.Values)
            {
                ProcessEquipmentMaintenance(schedule);
            }
        }
        
        private void UpdateEquipmentOperations()
        {
            foreach (var equipment in _equipmentRegistry.Values)
            {
                // Update equipment efficiency based on age and maintenance
                UpdateEquipmentEfficiency(equipment);
                
                // Check for automation schedules
                ProcessEquipmentSchedule(equipment);
            }
        }
        
        private void MonitorFacilityOperations()
        {
            _operationsData.TotalFacilities = _facilities.Count;
            _operationsData.TotalPowerConsumption = _facilities.Values.Sum(f => f.Rooms.Sum(r => r.PowerConsumption));
            _operationsData.AverageEfficiency = _facilities.Values.Average(f => CalculateFacilityEfficiency(f));
        }
        
        private List<ConstructionPhase> CreateConstructionPhases(FacilityInstance facility, FacilityConfigSO config)
        {
            var phases = new List<ConstructionPhase>();
            
            // Foundation phase
            phases.Add(new ConstructionPhase
            {
                PhaseName = "Foundation & Structure",
                Duration = config.ConstructionParameters.EstimatedDays * 0.3f,
                Description = "Site preparation, foundation, and structural work"
            });
            
            // Infrastructure phase
            phases.Add(new ConstructionPhase
            {
                PhaseName = "Infrastructure Installation",
                Duration = config.ConstructionParameters.EstimatedDays * 0.4f,
                Description = "Electrical, plumbing, and HVAC installation"
            });
            
            // Finishing phase
            phases.Add(new ConstructionPhase
            {
                PhaseName = "Finishing & Equipment",
                Duration = config.ConstructionParameters.EstimatedDays * 0.3f,
                Description = "Interior finishing and equipment installation"
            });
            
            return phases;
        }
        
        private void CompleteFacilityConstruction(string facilityId)
        {
            if (!_facilities.TryGetValue(facilityId, out var facility))
                return;
            
            facility.ConstructionProgress = 1f;
            facility.IsOperational = true;
            
            // Set all rooms to operational
            foreach (var room in facility.Rooms)
            {
                room.ConstructionStatus = ConstructionStatus.Operational;
                if (room.EnvironmentController != null)
                {
                    room.EnvironmentController.Status = ControllerStatus.Active;
                }
            }
            
            LogInfo($"Facility {facility.FacilityName} construction completed and is now operational");
        }
        
        private void CreateMaintenanceSchedule(EquipmentInstance equipment)
        {
            var schedule = new MaintenanceSchedule
            {
                EquipmentId = equipment.EquipmentId,
                Tasks = new List<MaintenanceTask>()
            };
            
            var requirements = equipment.EquipmentData.MaintenanceRequirements;
            
            // Add standard maintenance tasks
            if (requirements.CleaningFrequency > 0)
            {
                schedule.Tasks.Add(new MaintenanceTask
                {
                    TaskName = "Cleaning",
                    Interval = System.TimeSpan.FromDays(requirements.CleaningFrequency),
                    LastCompleted = System.DateTime.Now
                });
            }
            
            if (requirements.CalibrationFrequency > 0)
            {
                schedule.Tasks.Add(new MaintenanceTask
                {
                    TaskName = "Calibration",
                    Interval = System.TimeSpan.FromDays(requirements.CalibrationFrequency),
                    LastCompleted = System.DateTime.Now
                });
            }
            
            _maintenanceSchedules[equipment.EquipmentId] = schedule;
        }
        
        private void ConfigureEquipmentForAutomation(EquipmentInstance equipment, EnvironmentalTargets targets)
        {
            // Configure equipment based on its category and targets
            switch (equipment.EquipmentData.Category)
            {
                case EquipmentCategory.Lighting:
                    ConfigureLightingAutomation(equipment, targets);
                    break;
                case EquipmentCategory.HVAC:
                    ConfigureHVACAutomation(equipment, targets);
                    break;
                case EquipmentCategory.Irrigation:
                    ConfigureIrrigationAutomation(equipment, targets);
                    break;
            }
        }
        
        private void ConfigureLightingAutomation(EquipmentInstance equipment, EnvironmentalTargets targets)
        {
            if (targets.LightSchedule != null)
            {
                equipment.Schedule.EnableSchedule = true;
                equipment.Schedule.ScheduleType = ProjectChimera.Data.Facilities.ScheduleType.Daily;
                
                var scheduleEntry = new ScheduleEntry
                {
                    StartTime = targets.LightSchedule.LightsOn,
                    EndTime = targets.LightSchedule.LightsOff,
                    PowerLevel = 1f,
                    IsEnabled = true
                };
                
                // Add all days
                for (int i = 0; i < 7; i++)
                {
                    scheduleEntry.ActiveDays.Add((System.DayOfWeek)i);
                }
                
                equipment.Schedule.Schedule.Add(scheduleEntry);
            }
        }
        
        private void ConfigureHVACAutomation(EquipmentInstance equipment, EnvironmentalTargets targets)
        {
            // Set HVAC to maintain temperature and humidity targets
            equipment.RuntimeParameters["TargetTemperature"] = (targets.TemperatureRange.x + targets.TemperatureRange.y) / 2f;
            equipment.RuntimeParameters["TargetHumidity"] = (targets.HumidityRange.x + targets.HumidityRange.y) / 2f;
            equipment.Status = EquipmentStatus.Online;
        }
        
        private void ConfigureIrrigationAutomation(EquipmentInstance equipment, EnvironmentalTargets targets)
        {
            // Configure irrigation based on environmental needs
            equipment.RuntimeParameters["IrrigationFrequency"] = 24f; // Hours
            equipment.RuntimeParameters["IrrigationDuration"] = 15f; // Minutes
            equipment.Status = EquipmentStatus.Online;
        }
        
        private float OptimizeLightingEquipment(EquipmentInstance light, EnvironmentalTargets targets)
        {
            if (!light.EquipmentData.IsDimmable)
                return 0f;
            
            // Optimize lighting based on current needs and efficiency
            float currentPower = light.PowerConsumption * light.OperationalLevel;
            float optimalLevel = CalculateOptimalLightLevel(targets);
            
            if (optimalLevel < light.OperationalLevel)
            {
                float savings = currentPower * (light.OperationalLevel - optimalLevel);
                light.OperationalLevel = optimalLevel;
                return savings;
            }
            
            return 0f;
        }
        
        private float OptimizeHVACEquipment(EquipmentInstance hvac, EnvironmentalTargets targets)
        {
            // Optimize HVAC operation based on environmental targets
            float currentPower = hvac.PowerConsumption * hvac.OperationalLevel;
            float optimalLevel = CalculateOptimalHVACLevel(targets);
            
            if (optimalLevel < hvac.OperationalLevel)
            {
                float savings = currentPower * (hvac.OperationalLevel - optimalLevel);
                hvac.OperationalLevel = optimalLevel;
                return savings;
            }
            
            return 0f;
        }
        
        private float CalculateOptimalLightLevel(EnvironmentalTargets targets)
        {
            // Simplified optimization - in full implementation would consider 
            // plant growth stage, current light levels, time of day, etc.
            return Random.Range(0.7f, 1f);
        }
        
        private float CalculateOptimalHVACLevel(EnvironmentalTargets targets)
        {
            // Simplified optimization - in full implementation would consider
            // current vs target conditions, outdoor weather, etc.
            return Random.Range(0.6f, 0.9f);
        }
        
        private float CalculateFacilityEfficiency(FacilityInstance facility)
        {
            if (!facility.Rooms.Any())
                return 0f;
            
            float totalEfficiency = 0f;
            int equipmentCount = 0;
            
            foreach (var room in facility.Rooms)
            {
                foreach (var equipment in room.Equipment)
                {
                    totalEfficiency += equipment.EfficiencyRating;
                    equipmentCount++;
                }
            }
            
            return equipmentCount > 0 ? totalEfficiency / equipmentCount : 0f;
        }
        
        private void UpdateEquipmentEfficiency(EquipmentInstance equipment)
        {
            // Efficiency degrades over time without maintenance
            float daysSinceLastMaintenance = (float)(System.DateTime.Now - equipment.LastMaintenance).TotalDays;
            float maintenanceFrequency = equipment.EquipmentData.MaintenanceRequirements.CleaningFrequency;
            
            if (daysSinceLastMaintenance > maintenanceFrequency)
            {
                float degradation = (daysSinceLastMaintenance - maintenanceFrequency) / maintenanceFrequency * 0.1f;
                equipment.EfficiencyRating = Mathf.Max(0.5f, 1f - degradation);
                
                if (equipment.EfficiencyRating < _equipmentEfficiencyThreshold)
                {
                    _onMaintenanceRequired?.Raise();
                }
            }
        }
        
        private void ProcessEquipmentSchedule(EquipmentInstance equipment)
        {
            if (!equipment.Schedule.EnableSchedule || !equipment.Schedule.Schedule.Any())
                return;
            
            var now = System.DateTime.Now;
            var currentTime = now.TimeOfDay;
            var currentDay = now.DayOfWeek;
            
            foreach (var entry in equipment.Schedule.Schedule.Where(e => e.IsEnabled))
            {
                if (entry.ActiveDays.Contains(currentDay) &&
                    currentTime >= entry.StartTime &&
                    currentTime <= entry.EndTime)
                {
                    equipment.OperationalLevel = entry.PowerLevel;
                    equipment.Status = EquipmentStatus.Online;
                    break;
                }
            }
        }
        
        private void ProcessEquipmentMaintenance(MaintenanceSchedule schedule)
        {
            if (!_equipmentRegistry.TryGetValue(schedule.EquipmentId, out var equipment))
                return;
            
            foreach (var task in schedule.Tasks)
            {
                var nextDue = task.LastCompleted.Add(task.Interval);
                if (System.DateTime.Now >= nextDue)
                {
                    // Maintenance is due
                    if (_enableAutomaticMaintenance)
                    {
                        PerformAutomaticMaintenance(equipment, task);
                    }
                    else
                    {
                        _onMaintenanceRequired?.Raise();
                    }
                }
            }
        }
        
        private void PerformAutomaticMaintenance(EquipmentInstance equipment, MaintenanceTask task)
        {
            task.LastCompleted = System.DateTime.Now;
            equipment.LastMaintenance = System.DateTime.Now;
            equipment.EfficiencyRating = 1f; // Restore full efficiency
            
            LogInfo($"Automatic maintenance performed on {equipment.EquipmentName}: {task.TaskName}");
        }
        
        private List<string> GetMaintenanceAlerts(string facilityId)
        {
            var alerts = new List<string>();
            
            if (!_facilities.TryGetValue(facilityId, out var facility))
                return alerts;
            
            foreach (var room in facility.Rooms)
            {
                foreach (var equipment in room.Equipment)
                {
                    if (equipment.EfficiencyRating < _equipmentEfficiencyThreshold)
                    {
                        alerts.Add($"{equipment.EquipmentName} in {room.RoomName} requires maintenance");
                    }
                }
            }
            
            return alerts;
        }
        
        protected override void OnManagerShutdown()
        {
            _facilities.Clear();
            _activeConstructionProjects.Clear();
            _equipmentRegistry.Clear();
            _maintenanceSchedules.Clear();
            
            LogInfo("FacilityManager shutdown complete");
        }
    }
    
    // Supporting data structures for facility management
    
    [System.Serializable]
    public class ConstructionProject
    {
        public string ProjectId;
        public string FacilityId;
        public string ProjectName;
        public float EstimatedDays;
        public float EstimatedCost;
        public ConstructionStatus Status;
        public System.DateTime StartDate;
        public System.DateTime CompletionDate;
        public bool RequiredPermits;
        public bool RequiredInspections;
        public float Progress; // 0-1
        public List<ConstructionPhase> ConstructionPhases = new List<ConstructionPhase>();
    }
    
    [System.Serializable]
    public class ConstructionPhase
    {
        public string PhaseName;
        public float Duration; // Days
        public string Description;
        public float Progress; // 0-1
        public bool IsCompleted;
    }
    
    [System.Serializable]
    public class EquipmentOptimizationResult
    {
        public string RoomId;
        public System.DateTime Timestamp;
        public float OriginalPowerConsumption;
        public float OptimizedPowerConsumption;
        public float EnergySavings;
        public float EfficiencyGain;
        public List<string> OptimizationActions;
    }
    
    [System.Serializable]
    public class FacilityStatusReport
    {
        public string FacilityId;
        public string FacilityName;
        public System.DateTime Timestamp;
        public bool IsOperational;
        public float ConstructionProgress;
        public int TotalRooms;
        public int OperationalRooms;
        public int TotalEquipment;
        public int OnlineEquipment;
        public float TotalPowerConsumption;
        public float OverallEfficiency;
        public float EquipmentUptime;
        public List<string> MaintenanceAlerts;
    }
    
    [System.Serializable]
    public class FacilityOperationsData
    {
        public int TotalFacilities;
        public float TotalPowerConsumption;
        public float AverageEfficiency;
        public int TotalMaintenanceAlerts;
        public float ConstructionProgress;
    }
}