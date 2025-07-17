using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.Construction;
using ProjectChimera.Data.Environment;
// Explicit aliases to resolve ambiguous references
using FacilitiesRoomTemplate = ProjectChimera.Data.Facilities.RoomTemplate;
using ConstructionRoomType = ProjectChimera.Data.Construction.RoomType;
using ConstructionEnvironmentalConditions = ProjectChimera.Data.Construction.EnvironmentalConditions;
using ConstructionComplianceStatus = ProjectChimera.Data.Construction.ComplianceStatus;
using ConstructionSecurityLevel = ProjectChimera.Data.Construction.SecurityLevel;
using RoomTemplate = ProjectChimera.Data.Facilities.RoomTemplate;

namespace ProjectChimera.Systems.Construction
{
    /// <summary>
    /// Advanced room creation and configuration system for Project Chimera.
    /// Handles dynamic room design, layout optimization, environmental configuration,
    /// and specialized cannabis cultivation room creation with regulatory compliance.
    /// </summary>
    public class RoomCreationManager : ChimeraManager
    {
        [Header("Room Creation Configuration")]
        [SerializeField] private bool _enableAdvancedRoomDesign = true;
        [SerializeField] private bool _enforceRegulations = true;
        [SerializeField] private bool _enableSmartLayoutOptimization = true;
        [SerializeField] private bool _validateEnvironmentalRequirements = true;
        [SerializeField] private float _defaultCeilingHeight = 3.5f;
        [SerializeField] private float _minRoomSize = 9f; // 3x3 meters minimum
        [SerializeField] private float _maxRoomSize = 500f; // 500 sq meters maximum
        
        [Header("Cannabis Room Specifications")]
        [SerializeField] private float _plantsPerSquareFoot = 0.25f; // 1 plant per 4 sq ft
        [SerializeField] private float _airChangesPerHour = 60f; // HVAC requirement
        [SerializeField] private float _lightingPowerDensity = 50f; // Watts per sq ft
        [SerializeField] private bool _requireSecureAccess = true;
        [SerializeField] private bool _enforceTraceability = true;
        
        [Header("Room Templates")]
        [SerializeField] private List<FacilitiesRoomTemplate> _roomTemplates = new List<FacilitiesRoomTemplate>();
        [SerializeField] private List<CannabisRoomTemplate> _cannabisRoomTemplates = new List<CannabisRoomTemplate>();
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onRoomCreated;
        [SerializeField] private SimpleGameEventSO _onRoomConfigured;
        [SerializeField] private SimpleGameEventSO _onRoomValidated;
        [SerializeField] private SimpleGameEventSO _onLayoutOptimized;
        
        // Core room management
        private Dictionary<string, Room> _activeRooms = new Dictionary<string, Room>();
        private Dictionary<string, RoomConfiguration> _roomConfigurations = new Dictionary<string, RoomConfiguration>();
        private List<RoomDesignSession> _activeDesignSessions = new List<RoomDesignSession>();
        
        // Design and validation systems
        private RoomLayoutOptimizer _layoutOptimizer;
        private EnvironmentalValidator _environmentalValidator;
        private RegulatoryComplianceChecker _complianceChecker;
        private RoomVisualizationSystem _visualizationSystem;
        
        // Room creation tools
        private InteractiveRoomDesigner _roomDesigner;
        private SmartLayoutGenerator _layoutGenerator;
        private EnvironmentalConfigurationWizard _environmentalWizard;
        
        // Performance tracking
        private RoomCreationMetrics _metrics;
        private Dictionary<string, RoomPerformanceData> _roomPerformance = new Dictionary<string, RoomPerformanceData>();
        
        // Events
        public System.Action<Room> OnRoomCreated;
        public System.Action<Room, RoomConfiguration> OnRoomConfigured;
        public System.Action<Room, ValidationResult> OnRoomValidated;
        public System.Action<string, List<Room>> OnLayoutOptimized;
        public System.Action<Room, ConstructionEnvironmentalConditions> OnEnvironmentalConfigured;
        
        // Properties
        public override ManagerPriority Priority => ManagerPriority.High;
        public Dictionary<string, Room> ActiveRooms => _activeRooms;
        public List<FacilitiesRoomTemplate> AvailableTemplates => _roomTemplates;
        public List<CannabisRoomTemplate> CannabisTemplates => _cannabisRoomTemplates;
        public RoomCreationMetrics Metrics => _metrics;
        public int TotalRoomsCreated => _activeRooms.Count;
        public float TotalRoomArea => _activeRooms.Values.Sum(r => r.FloorArea);
        
        protected override void OnManagerInitialize()
        {
            InitializeRoomCreationSystems();
            InitializeRoomTemplates();
            InitializeVisualizationSystem();
            InitializePerformanceTracking();
            
            LogInfo("RoomCreationManager initialized successfully");
        }
        
        protected override void OnManagerUpdate()
        {
            UpdateActiveDesignSessions();
            UpdateRoomPerformance();
            UpdateVisualizationSystem();
            UpdateMetrics();
        }
        
        protected override void OnManagerShutdown()
        {
            // Cleanup all active design sessions
            foreach (var session in _activeDesignSessions)
            {
                session.IsActive = false;
                session.IsComplete = true;
            }
            _activeDesignSessions.Clear();
            
            // Cleanup visualization system
            _visualizationSystem = null;
            
            // Clear all tracking dictionaries
            _activeRooms.Clear();
            _roomConfigurations.Clear();
            _roomPerformance.Clear();
            
            LogInfo("RoomCreationManager shutdown completed");
        }
        
        /// <summary>
        /// Create a new room with specified parameters
        /// </summary>
        public Room CreateRoom(string roomName, ConstructionRoomType roomType, Vector3 position, Vector2 dimensions, string projectId = null)
        {
            var roomId = Guid.NewGuid().ToString();
            
            var room = new Room
            {
                RoomId = roomId,
                RoomName = roomName,
                RoomType = roomType,
                Position = position,
                Dimensions = dimensions,
                FloorArea = dimensions.x * dimensions.y,
                CeilingHeight = _defaultCeilingHeight,
                ProjectId = projectId,
                CreationDate = DateTime.Now,
                Status = RoomStatus.Planning,
                Configuration = CreateDefaultConfiguration(roomType),
                EnvironmentalRequirements = GetEnvironmentalRequirements(roomType),
                SecurityLevel = GetRequiredSecurityLevel(roomType),
                RegulatoryRequirements = GetRegulatoryRequirements(roomType)
            };
            
            // Validate room specifications
            var validationResult = ValidateRoomSpecifications(room);
            if (!validationResult.IsValid)
            {
                LogWarning($"Room validation failed: {validationResult.ErrorMessage}");
                return null;
            }
            
            // Apply cannabis-specific requirements if applicable
            if (IsCannabisRoom(roomType))
            {
                ApplyCannabisRequirements(room);
            }
            
            // Register room
            _activeRooms[roomId] = room;
            _roomConfigurations[roomId] = room.Configuration;
            _roomPerformance[roomId] = new RoomPerformanceData { RoomId = roomId };
            
            // Initialize room systems
            InitializeRoomSystems(room);
            
            // Trigger events
            OnRoomCreated?.Invoke(room);
            _onRoomCreated?.Raise();
            
            LogInfo($"Created room: {roomName} ({roomType}) - {dimensions.x}x{dimensions.y}m");
            return room;
        }
        
        /// <summary>
        /// Create room from template
        /// </summary>
        public Room CreateRoomFromTemplate(string templateId, string roomName, Vector3 position, string projectId = null)
        {
            var template = _roomTemplates.FirstOrDefault(t => t.TemplateId == templateId);
            if (template == null)
            {
                LogError($"Room template not found: {templateId}");
                return null;
            }
            
            var constructionRoomType = ConvertFacilityRoomTypeToConstructionRoomType(template.RoomType);
            var room = CreateRoom(roomName, constructionRoomType, position, template.Dimensions, projectId);
            if (room != null)
            {
                ApplyTemplate(room, template);
            }
            
            return room;
        }
        
        /// <summary>
        /// Create specialized cannabis cultivation room
        /// </summary>
        public Room CreateCannabisRoom(CannabisRoomType cannabisType, string roomName, Vector3 position, Vector2 dimensions, string projectId = null)
        {
            var roomType = GetRoomTypeFromCannabisType(cannabisType);
            var room = CreateRoom(roomName, roomType, position, dimensions, projectId);
            
            if (room != null)
            {
                ConfigureCannabisRoom(room, cannabisType);
            }
            
            return room;
        }
        
        /// <summary>
        /// Configure room with specific parameters
        /// </summary>
        public bool ConfigureRoom(string roomId, RoomConfiguration configuration)
        {
            if (!_activeRooms.TryGetValue(roomId, out var room))
            {
                LogError($"Room not found: {roomId}");
                return false;
            }
            
            // Validate configuration
            var validationResult = ValidateConfiguration(room, configuration);
            if (!validationResult.IsValid)
            {
                LogWarning($"Configuration validation failed: {validationResult.ErrorMessage}");
                return false;
            }
            
            // Apply configuration
            room.Configuration = configuration;
            _roomConfigurations[roomId] = configuration;
            
            // Update room systems
            UpdateRoomSystems(room);
            
            // Update environmental conditions
            UpdateEnvironmentalConditions(room);
            
            // Trigger events
            OnRoomConfigured?.Invoke(room, configuration);
            _onRoomConfigured?.Raise();
            
            LogInfo($"Configured room: {room.RoomName}");
            return true;
        }
        
        /// <summary>
        /// Optimize room layout for maximum efficiency
        /// </summary>
        public LayoutOptimizationResult OptimizeRoomLayout(string roomId, OptimizationCriteria criteria)
        {
            if (!_activeRooms.TryGetValue(roomId, out var room))
            {
                LogError($"Room not found: {roomId}");
                return null;
            }
            
            var result = _layoutOptimizer.OptimizeLayout(room, criteria);
            
            if (result.IsSuccessful)
            {
                ApplyLayoutOptimization(room, result);
                OnLayoutOptimized?.Invoke(roomId, new List<Room> { room });
                _onLayoutOptimized?.Raise();
            }
            
            return result;
        }
        
        /// <summary>
        /// Batch optimize multiple rooms in a facility
        /// </summary>
        public FacilityOptimizationResult OptimizeFacilityLayout(string projectId, FacilityOptimizationCriteria criteria)
        {
            var projectRooms = _activeRooms.Values.Where(r => r.ProjectId == projectId).ToList();
            if (projectRooms.Count == 0)
            {
                LogWarning($"No rooms found for project: {projectId}");
                return null;
            }
            
            var result = _layoutOptimizer.OptimizeFacility(projectRooms, criteria);
            
            if (result.IsSuccessful)
            {
                foreach (var optimization in result.RoomOptimizations)
                {
                    var room = projectRooms.FirstOrDefault(r => r.RoomId == optimization.RoomId);
                    if (room != null)
                    {
                        ApplyLayoutOptimization(room, optimization);
                    }
                }
                
                OnLayoutOptimized?.Invoke(projectId, projectRooms);
                _onLayoutOptimized?.Raise();
            }
            
            return result;
        }
        
        /// <summary>
        /// Delete room and cleanup resources
        /// </summary>
        public bool DeleteRoom(string roomId)
        {
            if (!_activeRooms.TryGetValue(roomId, out var room))
            {
                LogError($"Room not found: {roomId}");
                return false;
            }
            
            // Cleanup room resources
            CleanupRoomSystems(room);
            
            // Remove from tracking
            _activeRooms.Remove(roomId);
            _roomConfigurations.Remove(roomId);
            _roomPerformance.Remove(roomId);
            
            LogInfo($"Deleted room: {room.RoomName}");
            return true;
        }
        
        /// <summary>
        /// Get room performance analytics
        /// </summary>
        public RoomPerformanceData GetRoomPerformance(string roomId)
        {
            return _roomPerformance.GetValueOrDefault(roomId);
        }
        
        /// <summary>
        /// Get comprehensive room information
        /// </summary>
        public RoomInfo GetRoomInfo(string roomId)
        {
            if (!_activeRooms.TryGetValue(roomId, out var room))
            {
                return null;
            }
            
            var performance = GetRoomPerformance(roomId);
            var configuration = _roomConfigurations.GetValueOrDefault(roomId);
            
            return new RoomInfo
            {
                Room = room,
                Configuration = configuration,
                Performance = performance,
                EnvironmentalConditions = CalculateCurrentEnvironmentalConditions(room),
                CapacityUtilization = CalculateCapacityUtilization(room),
                ComplianceStatus = CheckComplianceStatus(room),
                OptimizationOpportunities = IdentifyOptimizationOpportunities(room)
            };
        }
        
        #region Private Implementation
        
        private void InitializeRoomCreationSystems()
        {
            _layoutOptimizer = new RoomLayoutOptimizer();
            _environmentalValidator = new EnvironmentalValidator();
            _complianceChecker = new RegulatoryComplianceChecker();
            _roomDesigner = new InteractiveRoomDesigner();
            _layoutGenerator = new SmartLayoutGenerator();
            _environmentalWizard = new EnvironmentalConfigurationWizard();
            
            _metrics = new RoomCreationMetrics();
        }
        
        private void InitializeRoomTemplates()
        {
            // Create default cannabis room templates
            _cannabisRoomTemplates.AddRange(new[]
            {
                CreateCannabisTemplate(CannabisRoomType.Propagation, new Vector2(6, 4), "Propagation Room"),
                CreateCannabisTemplate(CannabisRoomType.Vegetative, new Vector2(12, 8), "Vegetative Growth Room"),
                CreateCannabisTemplate(CannabisRoomType.Flowering, new Vector2(16, 12), "Flowering Room"),
                CreateCannabisTemplate(CannabisRoomType.Drying, new Vector2(8, 6), "Drying Room"),
                CreateCannabisTemplate(CannabisRoomType.Curing, new Vector2(10, 8), "Curing Room"),
                CreateCannabisTemplate(CannabisRoomType.Trimming, new Vector2(6, 6), "Trimming Room"),
                CreateCannabisTemplate(CannabisRoomType.Storage, new Vector2(8, 8), "Storage Room"),
                CreateCannabisTemplate(CannabisRoomType.Processing, new Vector2(12, 10), "Processing Room")
            });
        }
        
        private void InitializeVisualizationSystem()
        {
            _visualizationSystem = new RoomVisualizationSystem();
            _visualizationSystem.Initialize();
        }
        
        private void InitializePerformanceTracking()
        {
            // Initialize performance tracking systems
        }
        
        private void UpdateActiveDesignSessions()
        {
            foreach (var session in _activeDesignSessions.ToList())
            {
                session.Update();
                if (session.IsComplete)
                {
                    _activeDesignSessions.Remove(session);
                }
            }
        }
        
        private void UpdateRoomPerformance()
        {
            foreach (var kvp in _roomPerformance)
            {
                var performance = kvp.Value;
                var room = _activeRooms.GetValueOrDefault(kvp.Key);
                if (room != null)
                {
                    UpdatePerformanceMetrics(performance, room);
                }
            }
        }
        
        private void UpdateVisualizationSystem()
        {
            if (_visualizationSystem != null)
            {
                _visualizationSystem.Update();
            }
        }
        
        private void UpdateMetrics()
        {
            _metrics.TotalRoomsCreated = _activeRooms.Count;
            _metrics.TotalFloorArea = TotalRoomArea;
            _metrics.AverageRoomSize = _metrics.TotalRoomsCreated > 0 ? _metrics.TotalFloorArea / _metrics.TotalRoomsCreated : 0f;
        }
        
        private RoomConfiguration CreateDefaultConfiguration(RoomType roomType)
        {
            return new RoomConfiguration
            {
                ConfigurationId = Guid.NewGuid().ToString(),
                RoomType = roomType,
                MaxOccupancy = CalculateMaxOccupancy(roomType),
                EnvironmentalSettings = GetDefaultEnvironmentalSettings(roomType),
                SecuritySettings = GetDefaultSecuritySettings(roomType),
                EquipmentRequirements = GetDefaultEquipmentRequirements(roomType),
                AccessControlLevel = GetDefaultAccessLevel(roomType),
                MonitoringLevel = GetDefaultMonitoringLevel(roomType)
            };
        }
        
        private ConstructionEnvironmentalConditions GetEnvironmentalRequirements(ConstructionRoomType roomType)
        {
            return roomType switch
            {
                ConstructionRoomType.Vegetative => new ConstructionEnvironmentalConditions
                {
                    TemperatureRange = new Vector2(22f, 26f),
                    HumidityRange = new Vector2(55f, 70f),
                    CO2Level = 800f,
                    LightIntensity = 400f,
                    PhotoperiodHours = 18f
                },
                ConstructionRoomType.Flowering => new ConstructionEnvironmentalConditions
                {
                    TemperatureRange = new Vector2(20f, 24f),
                    HumidityRange = new Vector2(40f, 50f),
                    CO2Level = 1200f,
                    LightIntensity = 600f,
                    PhotoperiodHours = 12f
                },
                ConstructionRoomType.Drying => new ConstructionEnvironmentalConditions
                {
                    TemperatureRange = new Vector2(18f, 21f),
                    HumidityRange = new Vector2(45f, 55f),
                    CO2Level = 400f,
                    LightIntensity = 0f,
                    PhotoperiodHours = 0f
                },
                _ => new ConstructionEnvironmentalConditions()
            };
        }
        
        private ConstructionSecurityLevel GetRequiredSecurityLevel(ConstructionRoomType roomType)
        {
            return roomType switch
            {
                ConstructionRoomType.Vegetative or ConstructionRoomType.Flowering => ConstructionSecurityLevel.High,
                ConstructionRoomType.Storage or ConstructionRoomType.Processing => ConstructionSecurityLevel.Maximum,
                ConstructionRoomType.Drying or ConstructionRoomType.Curing => ConstructionSecurityLevel.Medium,
                _ => ConstructionSecurityLevel.Standard
            };
        }
        
        private List<RegulatoryRequirement> GetRegulatoryRequirements(ConstructionRoomType roomType)
        {
            var requirements = new List<RegulatoryRequirement>();
            
            if (IsCannabisRoom(roomType))
            {
                requirements.AddRange(new[]
                {
                    new RegulatoryRequirement { RequirementType = "Secure Access", Description = "Room must have controlled access" },
                    new RegulatoryRequirement { RequirementType = "Video Surveillance", Description = "24/7 video monitoring required" },
                    new RegulatoryRequirement { RequirementType = "Environmental Monitoring", Description = "Continuous environmental data logging" },
                    new RegulatoryRequirement { RequirementType = "Inventory Tracking", Description = "Real-time plant and product tracking" }
                });
            }
            
            return requirements;
        }
        
        private bool IsCannabisRoom(ConstructionRoomType roomType)
        {
            return roomType == ConstructionRoomType.Vegetative || roomType == ConstructionRoomType.Flowering || 
                   roomType == ConstructionRoomType.Drying || roomType == ConstructionRoomType.Curing ||
                   roomType == ConstructionRoomType.Storage || roomType == ConstructionRoomType.Processing;
        }
        
        private CannabisRoomTemplate CreateCannabisTemplate(CannabisRoomType cannabisType, Vector2 dimensions, string name)
        {
            return new CannabisRoomTemplate
            {
                TemplateId = Guid.NewGuid().ToString(),
                TemplateName = name,
                CannabisRoomType = cannabisType,
                RoomType = GetRoomTypeFromCannabisType(cannabisType),
                Dimensions = dimensions,
                OptimalPlantCount = CalculateOptimalPlantCount(dimensions),
                EnvironmentalPreset = GetEnvironmentalRequirements(GetRoomTypeFromCannabisType(cannabisType)),
                EquipmentList = GetRequiredEquipment(cannabisType),
                CreationDate = DateTime.Now
            };
        }
        
        private ConstructionRoomType GetRoomTypeFromCannabisType(CannabisRoomType cannabisType)
        {
            return cannabisType switch
            {
                CannabisRoomType.Propagation => ConstructionRoomType.Propagation,
                CannabisRoomType.Vegetative => ConstructionRoomType.Vegetative,
                CannabisRoomType.Flowering => ConstructionRoomType.Flowering,
                CannabisRoomType.Drying => ConstructionRoomType.Drying,
                CannabisRoomType.Curing => ConstructionRoomType.Curing,
                CannabisRoomType.Trimming => ConstructionRoomType.Processing,
                CannabisRoomType.Storage => ConstructionRoomType.Storage,
                CannabisRoomType.Processing => ConstructionRoomType.Processing,
                _ => ConstructionRoomType.General
            };
        }
        
        private int CalculateOptimalPlantCount(Vector2 dimensions)
        {
            float area = dimensions.x * dimensions.y;
            return Mathf.FloorToInt(area * _plantsPerSquareFoot);
        }
        
        private List<string> GetRequiredEquipment(CannabisRoomType cannabisType)
        {
            return cannabisType switch
            {
                CannabisRoomType.Vegetative => new List<string> { "LED Grow Lights", "HVAC System", "Humidity Control", "CO2 Generator", "Air Circulation Fans" },
                CannabisRoomType.Flowering => new List<string> { "High-Intensity Lights", "HVAC System", "Dehumidifier", "CO2 Generator", "Air Circulation Fans", "Light Timers" },
                CannabisRoomType.Drying => new List<string> { "Dehumidifier", "Air Circulation Fans", "Temperature Control", "Hanging Racks" },
                CannabisRoomType.Curing => new List<string> { "Climate Control", "Humidity Monitoring", "Storage Containers", "Air Circulation" },
                _ => new List<string>()
            };
        }
        
        private ValidationResult ValidateRoomSpecifications(Room room)
        {
            var result = new ValidationResult { IsValid = true };
            
            // Size validation
            if (room.FloorArea < _minRoomSize)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Room size {room.FloorArea:F1} sq m is below minimum {_minRoomSize} sq m";
                return result;
            }
            
            if (room.FloorArea > _maxRoomSize)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Room size {room.FloorArea:F1} sq m exceeds maximum {_maxRoomSize} sq m";
                return result;
            }
            
            // Cannabis-specific validation
            if (IsCannabisRoom(room.RoomType))
            {
                result = ValidateCannabisRoomRequirements(room);
            }
            
            return result;
        }
        
        private ValidationResult ValidateCannabisRoomRequirements(Room room)
        {
            var result = new ValidationResult { IsValid = true };
            
            // Security validation
            if (_requireSecureAccess && room.SecurityLevel < SecurityLevel.High)
            {
                result.IsValid = false;
                result.ErrorMessage = "Cannabis rooms require high security level";
                return result;
            }
            
            // Environmental validation
            var envResult = _environmentalValidator.ValidateEnvironmentalRequirements(room);
            if (!envResult.IsValid)
            {
                return envResult;
            }
            
            // Regulatory compliance
            if (_enforceRegulations)
            {
                var complianceResult = _complianceChecker.CheckCompliance(room);
                if (!complianceResult.IsValid)
                {
                    return complianceResult;
                }
            }
            
            return result;
        }
        
        private void ApplyCannabisRequirements(Room room)
        {
            room.SecurityLevel = SecurityLevel.High;
            room.RegulatoryRequirements.AddRange(GetRegulatoryRequirements(room.RoomType));
        }
        
        private void InitializeRoomSystems(Room room)
        {
            // Initialize monitoring systems, environmental controls, etc.
        }
        
        private void ApplyTemplate(Room room, RoomTemplate template)
        {
            // Apply template-specific configurations
        }
        
        private void ConfigureCannabisRoom(Room room, CannabisRoomType cannabisType)
        {
            // Apply cannabis-specific configuration based on type
            var template = _cannabisRoomTemplates.FirstOrDefault(t => t.CannabisRoomType == cannabisType);
            if (template != null)
            {
                room.MaxOccupancy = template.OptimalPlantCount;
                room.EnvironmentalRequirements = template.EnvironmentalPreset;
            }
        }
        
        private ValidationResult ValidateConfiguration(Room room, RoomConfiguration configuration)
        {
            return new ValidationResult { IsValid = true };
        }
        
        private void UpdateRoomSystems(Room room)
        {
            // Update room systems when configuration changes
        }
        
        private void UpdateEnvironmentalConditions(Room room)
        {
            // Update environmental conditions based on configuration
        }
        
        private void ApplyLayoutOptimization(Room room, LayoutOptimizationResult result)
        {
            // Apply optimization results to room
        }
        
        private void CleanupRoomSystems(Room room)
        {
            // Cleanup resources when room is deleted
        }
        
        private ConstructionEnvironmentalConditions CalculateCurrentEnvironmentalConditions(Room room)
        {
            return room.EnvironmentalRequirements ?? new ConstructionEnvironmentalConditions();
        }
        
        private float CalculateCapacityUtilization(Room room)
        {
            return room.CapacityUtilization;
        }
        
        private ConstructionComplianceStatus CheckComplianceStatus(Room room)
        {
            return new ConstructionComplianceStatus { IsCompliant = true };
        }
        
        private List<OptimizationOpportunity> IdentifyOptimizationOpportunities(Room room)
        {
            return new List<OptimizationOpportunity>();
        }
        
        private void UpdatePerformanceMetrics(RoomPerformanceData performance, Room room)
        {
            performance.LastUpdated = DateTime.Now;
            // Update performance metrics based on room status
        }
        
        private int CalculateMaxOccupancy(RoomType roomType)
        {
            return roomType switch
            {
                RoomType.Vegetative => 50,
                RoomType.Flowering => 40,
                RoomType.Drying => 100,
                RoomType.Storage => 200,
                _ => 20
            };
        }
        
        private EnvironmentalSettings GetDefaultEnvironmentalSettings(RoomType roomType)
        {
            return new EnvironmentalSettings
            {
                AutomaticClimateControl = true,
                CO2Supplementation = IsCannabisRoom(roomType),
                DehumidificationEnabled = true,
                AirCirculationEnabled = true,
                TargetTemperature = 23f,
                TargetHumidity = 60f,
                TargetCO2 = IsCannabisRoom(roomType) ? 800f : 400f
            };
        }
        
        private SecuritySettings GetDefaultSecuritySettings(RoomType roomType)
        {
            return new SecuritySettings
            {
                AccessControlEnabled = IsCannabisRoom(roomType),
                VideoSurveillanceEnabled = IsCannabisRoom(roomType),
                MotionDetectionEnabled = false,
                AlarmSystemEnabled = IsCannabisRoom(roomType),
                BiometricAccess = roomType == RoomType.Storage,
                MaxSimultaneousAccess = 2
            };
        }
        
        private List<string> GetDefaultEquipmentRequirements(RoomType roomType)
        {
            return roomType switch
            {
                RoomType.Vegetative => new List<string> { "LED Grow Lights", "HVAC System", "Humidity Control", "CO2 Generator" },
                RoomType.Flowering => new List<string> { "High-Intensity Lights", "HVAC System", "Dehumidifier", "CO2 Generator", "Light Timers" },
                RoomType.Drying => new List<string> { "Dehumidifier", "Air Circulation Fans", "Temperature Control", "Hanging Racks" },
                RoomType.Storage => new List<string> { "Climate Control", "Security System", "Inventory Management", "Storage Containers" },
                _ => new List<string>()
            };
        }
        
        private AccessControlLevel GetDefaultAccessLevel(RoomType roomType)
        {
            return roomType switch
            {
                RoomType.Vegetative or RoomType.Flowering => AccessControlLevel.Authorized,
                RoomType.Storage => AccessControlLevel.HighSecurity,
                RoomType.Processing => AccessControlLevel.Authorized,
                _ => AccessControlLevel.Restricted
            };
        }
        
        private MonitoringLevel GetDefaultMonitoringLevel(RoomType roomType)
        {
            return roomType switch
            {
                RoomType.Vegetative or RoomType.Flowering => MonitoringLevel.Comprehensive,
                RoomType.Storage => MonitoringLevel.Enhanced,
                RoomType.Drying or RoomType.Curing => MonitoringLevel.Standard,
                _ => MonitoringLevel.Basic
            };
        }
        
        /// <summary>
        /// Convert Facilities.RoomType to Construction.RoomType
        /// </summary>
        private ConstructionRoomType ConvertFacilityRoomTypeToConstructionRoomType(ProjectChimera.Data.Facilities.RoomType facilityRoomType)
        {
            return facilityRoomType switch
            {
                ProjectChimera.Data.Facilities.RoomType.Propagation => ConstructionRoomType.Propagation,
                ProjectChimera.Data.Facilities.RoomType.Vegetative => ConstructionRoomType.Vegetative,
                ProjectChimera.Data.Facilities.RoomType.Flowering => ConstructionRoomType.Flowering,
                ProjectChimera.Data.Facilities.RoomType.Drying => ConstructionRoomType.Drying,
                ProjectChimera.Data.Facilities.RoomType.Curing => ConstructionRoomType.Curing,
                ProjectChimera.Data.Facilities.RoomType.Storage => ConstructionRoomType.Storage,
                ProjectChimera.Data.Facilities.RoomType.Processing => ConstructionRoomType.Processing,
                ProjectChimera.Data.Facilities.RoomType.Laboratory => ConstructionRoomType.Laboratory,
                ProjectChimera.Data.Facilities.RoomType.Office => ConstructionRoomType.Office,
                ProjectChimera.Data.Facilities.RoomType.Security => ConstructionRoomType.Security,
                ProjectChimera.Data.Facilities.RoomType.Maintenance => ConstructionRoomType.Utility,
                ProjectChimera.Data.Facilities.RoomType.Trimming => ConstructionRoomType.Processing,
                _ => ConstructionRoomType.General
            };
        }
        
        #endregion
    }
    
    #region Supporting Classes and Enums
    
    public enum CannabisRoomType
    {
        Propagation,
        Vegetative,
        Flowering,
        Drying,
        Curing,
        Trimming,
        Storage,
        Processing
    }
    
    [System.Serializable]
    public class CannabisRoomTemplate
    {
        public string TemplateId;
        public string TemplateName;
        public CannabisRoomType CannabisRoomType;
        public RoomType RoomType;
        public Vector2 Dimensions;
        public int OptimalPlantCount;
        public ConstructionEnvironmentalConditions EnvironmentalPreset;
        public List<string> EquipmentList;
        public DateTime CreationDate;
    }
    
    [System.Serializable]
    public class RoomCreationMetrics
    {
        public int TotalRoomsCreated;
        public float TotalFloorArea;
        public float AverageRoomSize;
        public Dictionary<RoomType, int> RoomTypeDistribution = new Dictionary<RoomType, int>();
        public float OptimizationSuccessRate;
        public int TotalOptimizationsPerformed;
    }
    
    [System.Serializable]
    public class RoomInfo
    {
        public Room Room;
        public RoomConfiguration Configuration;
        public RoomPerformanceData Performance;
        public ConstructionEnvironmentalConditions EnvironmentalConditions;
        public float CapacityUtilization;
        public ConstructionComplianceStatus ComplianceStatus;
        public List<OptimizationOpportunity> OptimizationOpportunities;
    }
    
    #endregion
}