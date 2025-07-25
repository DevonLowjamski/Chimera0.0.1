using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.Automation;
// using ProjectChimera.Systems.AI;
using ProjectChimera.Data.Economy;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Facilities;
using ProjectChimera.Data.Environment;
// Resolve ambiguous references - prefer Automation types for automation system
using SensorConfiguration = ProjectChimera.Data.Automation.SensorConfiguration;
using AutomationAction = ProjectChimera.Data.Automation.AutomationAction;
using PredictiveModel = ProjectChimera.Data.Automation.PredictiveModel;
using SensorType = ProjectChimera.Data.Automation.SensorType;
using AlertSeverity = ProjectChimera.Data.Automation.AlertSeverity;
using TriggerType = ProjectChimera.Data.Automation.TriggerType;
using ActionType = ProjectChimera.Data.Automation.ActionType;
using AutomationTrigger = ProjectChimera.Data.Automation.AutomationTrigger;

namespace ProjectChimera.Systems.Automation
{
    /// <summary>
    /// Advanced Automation and IoT Management System for Project Chimera.
    /// Manages sensor networks, automated responses, monitoring dashboards, and smart facility control.
    /// Integrates with HVAC, Lighting, and other environmental systems for comprehensive automation.
    /// </summary>
    public class AutomationManager : ChimeraManager
    {
        [Header("Automation Configuration")]
        [SerializeField] private AutomationSettings _automationSettings;
        [SerializeField] private bool _enablePredictiveControl = true;
        [SerializeField] private bool _enableLearningAlgorithms = true;
        [SerializeField] private bool _enableRemoteMonitoring = true;
        
        [Header("Sensor Management")]
        [SerializeField] private int _maxSensorsPerZone = 50;
        [SerializeField] private float _sensorCalibrationInterval = 30f; // days
        [SerializeField] private bool _enableAutomaticCalibration = true;
        
        [Header("Alert Configuration")]
        [SerializeField] private bool _enableSmartAlerts = true;
        [SerializeField] private float _alertCooldownPeriod = 300f; // seconds
        [SerializeField] private int _maxAlertsPerHour = 20;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onSensorAlert;
        [SerializeField] private SimpleGameEventSO _onAutomationTriggered;
        [SerializeField] private SimpleGameEventSO _onDeviceStatusChanged;
        [SerializeField] private SimpleGameEventSO _onPredictiveAlert;
        
        // Core automation data
        private Dictionary<string, SensorConfiguration> _sensors = new Dictionary<string, SensorConfiguration>();
        private Dictionary<string, IoTDevice> _iotDevices = new Dictionary<string, IoTDevice>();
        private Dictionary<string, AutomationRule> _automationRules = new Dictionary<string, AutomationRule>();
        private Dictionary<string, MonitoringDashboard> _dashboards = new Dictionary<string, MonitoringDashboard>();
        private List<SensorReading> _recentReadings = new List<SensorReading>();
        private List<SmartAlert> _activeAlerts = new List<SmartAlert>();
        private List<AutomationLog> _automationLogs = new List<AutomationLog>();
        
        // Predictive systems
        private Dictionary<string, PredictiveModel> _predictiveModels = new Dictionary<string, PredictiveModel>();
        private Queue<SensorReading> _trainingData = new Queue<SensorReading>();
        
        // Timing and performance
        private float _lastSensorUpdate;
        private float _lastAutomationCheck;
        private float _lastPredictiveUpdate;
        
        public override ManagerPriority Priority => ManagerPriority.Normal;
        
        // Public Properties
        public int ActiveSensors => _sensors.Count(s => s.Value.IsActive);
        public int ConnectedDevices => _iotDevices.Count(d => d.Value.Status == IoTDeviceStatus.Online);
        public int ActiveAutomationRules => _automationRules.Count(r => r.Value.IsEnabled);
        public int ActiveAlerts => _activeAlerts.Count(a => a.Status == AlertStatus.Active);
        public AutomationSettings Settings => _automationSettings;
        
        // Events
        public System.Action<SmartAlert> OnSmartAlert;
        public System.Action<AutomationRule, SensorReading> OnAutomationTriggered;
        public System.Action<IoTDevice, IoTDeviceStatus> OnDeviceStatusChanged;
        public System.Action<PredictiveModel, Dictionary<string, float>> OnPredictionGenerated;
        
        protected override void OnManagerInitialize()
        {
            _lastSensorUpdate = Time.time;
            _lastAutomationCheck = Time.time;
            _lastPredictiveUpdate = Time.time;
            
            InitializeDefaultSensors();
            InitializeDefaultAutomationRules();
            SetupPredictiveModels();
            
            LogInfo("AutomationManager initialized with comprehensive IoT and automation capabilities");
        }
        
        protected override void OnManagerUpdate()
        {
            float currentTime = Time.time;
            
            // Sensor data collection
            if (currentTime - _lastSensorUpdate >= 1f / _automationSettings.SensorUpdateFrequency)
            {
                UpdateSensorReadings();
                ProcessSensorData();
                _lastSensorUpdate = currentTime;
            }
            
            // Automation rule evaluation
            if (currentTime - _lastAutomationCheck >= _automationSettings.AutomationResponseDelay)
            {
                EvaluateAutomationRules();
                ProcessPendingActions();
                _lastAutomationCheck = currentTime;
            }
            
            // Predictive analysis
            if (_enablePredictiveControl && currentTime - _lastPredictiveUpdate >= 300f) // Every 5 minutes
            {
                UpdatePredictiveModels();
                GeneratePredictions();
                _lastPredictiveUpdate = currentTime;
            }
            
            // Maintenance tasks
            CheckDeviceHealth();
            CleanupOldData();
        }
        
        /// <summary>
        /// Registers a new sensor in the automation system.
        /// </summary>
        public bool RegisterSensor(SensorConfiguration sensorConfig)
        {
            if (_sensors.ContainsKey(sensorConfig.SensorId))
            {
                LogWarning($"Sensor {sensorConfig.SensorId} already registered");
                return false;
            }
            
            if (!ValidateSensorConfiguration(sensorConfig))
            {
                LogWarning($"Invalid sensor configuration for {sensorConfig.SensorId}");
                return false;
            }
            
            _sensors[sensorConfig.SensorId] = sensorConfig;
            
            // Create initial reading
            var initialReading = new SensorReading
            {
                SensorId = sensorConfig.SensorId,
                Timestamp = DateTime.Now,
                Value = GetInitialSensorValue(sensorConfig.SensorType),
                Unit = GetSensorUnit(sensorConfig.SensorType),
                Status = SensorReadingStatus.Valid,
                Confidence = sensorConfig.Accuracy
            };
            
            _recentReadings.Add(initialReading);
            
            LogInfo($"Registered sensor {sensorConfig.SensorName} ({sensorConfig.SensorType}) in zone {sensorConfig.ZoneId}");
            return true;
        }
        
        /// <summary>
        /// Creates and registers an automation rule.
        /// </summary>
        public string CreateAutomationRule(string ruleName, AutomationTrigger trigger, List<AutomationAction> actions, AutomationCondition condition = null)
        {
            var rule = new AutomationRule
            {
                RuleId = Guid.NewGuid().ToString(),
                RuleName = ruleName,
                IsEnabled = true,
                Trigger = trigger,
                Actions = actions,
                Condition = condition ?? new AutomationCondition(),
                Priority = 1,
                CooldownPeriod = _alertCooldownPeriod,
                CreatedBy = "System",
                CreatedDate = DateTime.Now
            };
            
            _automationRules[rule.RuleId] = rule;
            
            LogInfo($"Created automation rule: {ruleName}");
            return rule.RuleId;
        }
        
        /// <summary>
        /// Integrates with HVAC system for automated climate control.
        /// </summary>
        public void IntegrateWithHVAC(string zoneId, object hvacManager)
        {
            // Temperature control automation
            var tempSensorId = $"temp_sensor_{zoneId}";
            var humiditySensorId = $"humidity_sensor_{zoneId}";
            
            // Register temperature sensor if not exists
            if (!_sensors.ContainsKey(tempSensorId))
            {
                RegisterSensor(new SensorConfiguration
                {
                    SensorId = tempSensorId,
                    SensorName = $"Temperature Sensor - {zoneId}",
                    SensorType = SensorType.Temperature,
                    ZoneId = zoneId,
                    ReadingInterval = 30f,
                    Accuracy = 0.98f,
                    IsActive = true,
                    AlarmSettings = new SensorAlarmSettings
                    {
                        EnableAlarms = true,
                        LowThreshold = 18f,
                        HighThreshold = 28f,
                        CriticalLowThreshold = 15f,
                        CriticalHighThreshold = 35f,
                        AlarmPriority = AlarmPriority.High
                    }
                });
            }
            
            // Create temperature control rule
            CreateAutomationRule(
                $"Temperature Control - {zoneId}",
                new AutomationTrigger
                {
                    TriggerType = TriggerType.Threshold_Exceeded,
                    SourceSensorId = tempSensorId,
                    TriggerValue = 26f,
                    Operator = ComparisonOperator.GreaterThan,
                    TriggerDuration = 60f
                },
                new List<AutomationAction>
                {
                    new AutomationAction
                    {
                        ActionId = Guid.NewGuid().ToString(),
                        ActionType = ActionType.SetTemperature,
                        TargetZoneId = zoneId,
                        Parameters = new Dictionary<string, object> { { "temperature", 24f } },
                        LogAction = true
                    }
                }
            );
            
            LogInfo($"Integrated automation with HVAC system for zone {zoneId}");
        }
        
        /// <summary>
        /// Integrates with lighting system for automated light control.
        /// </summary>
        public void IntegrateWithLighting(string zoneId, object lightingManager)
        {
            // Light intensity sensor
            var lightSensorId = $"light_sensor_{zoneId}";
            var dliSensorId = $"dli_sensor_{zoneId}";
            
            // Register light sensors
            RegisterSensor(new SensorConfiguration
            {
                SensorId = lightSensorId,
                SensorName = $"Light Intensity Sensor - {zoneId}",
                SensorType = SensorType.Light_Intensity,
                ZoneId = zoneId,
                ReadingInterval = 60f,
                Accuracy = 0.95f,
                IsActive = true,
                AlarmSettings = new SensorAlarmSettings
                {
                    EnableAlarms = true,
                    LowThreshold = 200f,
                    HighThreshold = 1000f,
                    AlarmPriority = AlarmPriority.Normal
                }
            });
            
            RegisterSensor(new SensorConfiguration
            {
                SensorId = dliSensorId,
                SensorName = $"DLI Sensor - {zoneId}",
                SensorType = SensorType.DLI,
                ZoneId = zoneId,
                ReadingInterval = 300f, // 5 minutes
                Accuracy = 0.92f,
                IsActive = true
            });
            
            // Create photoperiod automation rule
            CreateAutomationRule(
                $"Photoperiod Control - {zoneId}",
                new AutomationTrigger
                {
                    TriggerType = TriggerType.Time_Based,
                    TriggerValue = 6f, // 6 AM
                    Operator = ComparisonOperator.Equals
                },
                new List<AutomationAction>
                {
                    new AutomationAction
                    {
                        ActionId = Guid.NewGuid().ToString(),
                        ActionType = ActionType.TurnOnLight,
                        TargetZoneId = zoneId,
                        Parameters = new Dictionary<string, object> { { "intensity", 0.8f } }
                    }
                }
            );
            
            LogInfo($"Integrated automation with lighting system for zone {zoneId}");
        }
        
        /// <summary>
        /// Creates a monitoring dashboard for a specific zone.
        /// </summary>
        public string CreateMonitoringDashboard(string dashboardName, string zoneId)
        {
            var dashboard = new MonitoringDashboard
            {
                DashboardId = Guid.NewGuid().ToString(),
                DashboardName = dashboardName,
                Layout = DashboardLayout.Grid,
                IsPublic = false,
                LastUpdated = DateTime.Now,
                RefreshRate = 5f
            };
            
            // Add default widgets for the zone
            var zoneSensors = _sensors.Values.Where(s => s.ZoneId == zoneId).ToList();
            
            foreach (var sensor in zoneSensors.Take(6)) // Limit to 6 widgets initially
            {
                dashboard.Widgets.Add(new DashboardWidget
                {
                    WidgetId = Guid.NewGuid().ToString(),
                    WidgetName = $"{sensor.SensorName} Reading",
                    WidgetType = GetWidgetTypeForSensor(sensor.SensorType),
                    Position = new Vector2(dashboard.Widgets.Count % 3, dashboard.Widgets.Count / 3),
                    Size = new Vector2(1, 1),
                    DataSourceId = sensor.SensorId,
                    ShowAlerts = true,
                    DataTimeRange = TimeRange.Last24Hours
                });
            }
            
            _dashboards[dashboard.DashboardId] = dashboard;
            
            LogInfo($"Created monitoring dashboard '{dashboardName}' for zone {zoneId}");
            return dashboard.DashboardId;
        }
        
        /// <summary>
        /// Gets current sensor readings for a specific zone.
        /// </summary>
        public List<SensorReading> GetZoneSensorReadings(string zoneId)
        {
            var zoneSensors = _sensors.Values.Where(s => s.ZoneId == zoneId).Select(s => s.SensorId);
            return _recentReadings.Where(r => zoneSensors.Contains(r.SensorId))
                                 .GroupBy(r => r.SensorId)
                                 .Select(g => g.OrderByDescending(r => r.Timestamp).First())
                                 .ToList();
        }
        
        /// <summary>
        /// Generates a comprehensive automation report.
        /// </summary>
        public AutomationReport GenerateAutomationReport(TimeSpan period)
        {
            var endTime = DateTime.Now;
            var startTime = endTime - period;
            
            var periodReadings = _recentReadings.Where(r => r.Timestamp >= startTime && r.Timestamp <= endTime).ToList();
            var periodLogs = _automationLogs.Where(l => l.Timestamp >= startTime && l.Timestamp <= endTime).ToList();
            var periodAlerts = _activeAlerts.Where(a => a.Timestamp >= startTime && a.Timestamp <= endTime).ToList();
            
            return new AutomationReport
            {
                ReportPeriod = period,
                GeneratedAt = DateTime.Now,
                TotalSensorReadings = periodReadings.Count,
                TotalAutomationActions = periodLogs.Count(l => l.LogLevel == LogLevel.Info && l.Component == "AutomationAction"),
                TotalAlerts = periodAlerts.Count,
                CriticalAlerts = periodAlerts.Count(a => a.Severity == AlertSeverity.Critical),
                SystemUptime = CalculateSystemUptime(period),
                EnergyOptimizationSavings = CalculateEnergySavings(period),
                TopPerformingSensors = GetTopPerformingSensors(periodReadings),
                MostTriggeredRules = GetMostTriggeredRules(period)
            };
        }
        
        private void InitializeDefaultSensors()
        {
            // Create default sensor configurations for common zones
            var defaultZones = new[] { "VegetativeRoom", "FloweringRoom", "NurseryRoom" };
            
            foreach (var zone in defaultZones)
            {
                // Temperature sensor
                RegisterSensor(new SensorConfiguration
                {
                    SensorId = $"temp_{zone.ToLower()}",
                    SensorName = $"Temperature - {zone}",
                    SensorType = SensorType.Temperature,
                    ZoneId = zone,
                    ReadingInterval = 30f,
                    Accuracy = 0.98f,
                    IsActive = true,
                    AlarmSettings = new SensorAlarmSettings
                    {
                        EnableAlarms = true,
                        LowThreshold = 18f,
                        HighThreshold = 28f,
                        AlarmPriority = AlarmPriority.High
                    }
                });
                
                // Humidity sensor
                RegisterSensor(new SensorConfiguration
                {
                    SensorId = $"humidity_{zone.ToLower()}",
                    SensorName = $"Humidity - {zone}",
                    SensorType = SensorType.Humidity,
                    ZoneId = zone,
                    ReadingInterval = 30f,
                    Accuracy = 0.95f,
                    IsActive = true,
                    AlarmSettings = new SensorAlarmSettings
                    {
                        EnableAlarms = true,
                        LowThreshold = 40f,
                        HighThreshold = 70f,
                        AlarmPriority = AlarmPriority.High
                    }
                });
                
                // CO2 sensor
                RegisterSensor(new SensorConfiguration
                {
                    SensorId = $"co2_{zone.ToLower()}",
                    SensorName = $"CO2 - {zone}",
                    SensorType = SensorType.CO2,
                    ZoneId = zone,
                    ReadingInterval = 60f,
                    Accuracy = 0.92f,
                    IsActive = true,
                    AlarmSettings = new SensorAlarmSettings
                    {
                        EnableAlarms = true,
                        LowThreshold = 800f,
                        HighThreshold = 1500f,
                        AlarmPriority = AlarmPriority.Normal
                    }
                });
            }
        }
        
        private void InitializeDefaultAutomationRules()
        {
            // Emergency temperature control
            CreateAutomationRule(
                "Emergency Temperature Control",
                new AutomationTrigger
                {
                    TriggerType = TriggerType.Threshold_Exceeded,
                    TriggerValue = 35f,
                    Operator = ComparisonOperator.GreaterThan,
                    TriggerDuration = 30f
                },
                new List<AutomationAction>
                {
                    new AutomationAction
                    {
                        ActionId = Guid.NewGuid().ToString(),
                        ActionType = ActionType.EmergencyShutdown,
                        Parameters = new Dictionary<string, object> { { "reason", "Critical temperature exceeded" } },
                        RequiresConfirmation = false,
                        LogAction = true
                    },
                    new AutomationAction
                    {
                        ActionId = Guid.NewGuid().ToString(),
                        ActionType = ActionType.SendAlert,
                        Parameters = new Dictionary<string, object> 
                        { 
                            { "severity", AlertSeverity.Emergency },
                            { "message", "Critical temperature alert - Emergency shutdown initiated" }
                        }
                    }
                }
            );
            
            // Low humidity alert
            CreateAutomationRule(
                "Low Humidity Alert",
                new AutomationTrigger
                {
                    TriggerType = TriggerType.Threshold_Below,
                    TriggerValue = 30f,
                    Operator = ComparisonOperator.LessThan,
                    TriggerDuration = 300f // 5 minutes
                },
                new List<AutomationAction>
                {
                    new AutomationAction
                    {
                        ActionId = Guid.NewGuid().ToString(),
                        ActionType = ActionType.SetHumidity,
                        Parameters = new Dictionary<string, object> { { "humidity", 45f } }
                    }
                }
            );
        }
        
        private void SetupPredictiveModels()
        {
            if (!_enablePredictiveControl) return;
            
            // Environmental prediction model
            var environmentModel = new PredictiveModel
            {
                ModelId = "environmental_prediction",
                ModelName = "Environmental Condition Predictor",
                ModelType = PredictiveModelType.Time_Series,
                InputSensorIds = _sensors.Values
                    .Where(s => s.SensorType == SensorType.Temperature || 
                               s.SensorType == SensorType.Humidity || 
                               s.SensorType == SensorType.CO2)
                    .Select(s => s.SensorId)
                    .ToList(),
                TargetVariable = "environmental_health",
                PredictionHorizon = TimeSpan.FromHours(4),
                IsActive = true
            };
            
            _predictiveModels[environmentModel.ModelId] = environmentModel;
        }
        
        private void UpdateSensorReadings()
        {
            foreach (var sensor in _sensors.Values.Where(s => s.IsActive))
            {
                var timeSinceLastReading = (DateTime.Now - GetLastReadingTime(sensor.SensorId)).TotalSeconds;
                
                if (timeSinceLastReading >= sensor.ReadingInterval)
                {
                    var reading = GenerateSensorReading(sensor);
                    _recentReadings.Add(reading);
                    
                    // Check for alerts
                    CheckSensorAlarms(sensor, reading);
                }
            }
        }
        
        private void ProcessSensorData()
        {
            // Remove old readings (keep last 24 hours)
            var cutoffTime = DateTime.Now.AddHours(-_automationSettings.DataRetentionHours);
            _recentReadings.RemoveAll(r => r.Timestamp < cutoffTime);
            
            // Update training data for predictive models
            if (_enableLearningAlgorithms)
            {
                foreach (var reading in _recentReadings.TakeLast(100))
                {
                    _trainingData.Enqueue(reading);
                    if (_trainingData.Count > 10000) // Keep last 10k readings
                        _trainingData.Dequeue();
                }
            }
        }
        
        private void EvaluateAutomationRules()
        {
            foreach (var rule in _automationRules.Values.Where(r => r.IsEnabled))
            {
                if (ShouldTriggerRule(rule))
                {
                    ExecuteAutomationRule(rule);
                }
            }
        }
        
        private bool ShouldTriggerRule(AutomationRule rule)
        {
            // Check cooldown period
            if ((DateTime.Now - rule.LastTriggered).TotalSeconds < rule.CooldownPeriod)
                return false;
            
            // Check trigger condition
            var triggerMet = EvaluateTrigger(rule.Trigger);
            
            // Check additional conditions if any
            if (rule.Condition != null && rule.Condition.Rules.Any())
            {
                var conditionMet = EvaluateCondition(rule.Condition);
                return triggerMet && conditionMet;
            }
            
            return triggerMet;
        }
        
        private bool EvaluateTrigger(AutomationTrigger trigger)
        {
            var latestReading = _recentReadings
                .Where(r => r.SensorId == trigger.SourceSensorId)
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefault();
            
            if (latestReading == null) return false;
            
            switch (trigger.TriggerType)
            {
                case TriggerType.Threshold_Exceeded:
                    return CompareValues(latestReading.Value, trigger.TriggerValue, trigger.Operator);
                
                case TriggerType.Threshold_Below:
                    return latestReading.Value < trigger.TriggerValue;
                
                case TriggerType.Time_Based:
                    return DateTime.Now.Hour == (int)trigger.TriggerValue;
                
                default:
                    return false;
            }
        }
        
        private bool EvaluateCondition(AutomationCondition condition)
        {
            if (!condition.Rules.Any()) return true;
            
            var results = condition.Rules.Select(rule => EvaluateConditionRule(rule)).ToList();
            
            bool finalResult = condition.LogicalOperator switch
            {
                LogicalOperator.And => results.All(r => r),
                LogicalOperator.Or => results.Any(r => r),
                LogicalOperator.Xor => results.Count(r => r) == 1,
                LogicalOperator.Not => !results.First(),
                _ => false
            };
            
            return condition.InvertResult ? !finalResult : finalResult;
        }
        
        private bool EvaluateConditionRule(ConditionRule rule)
        {
            var latestReading = _recentReadings
                .Where(r => r.SensorId == rule.SensorId)
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefault();
            
            if (latestReading == null) return false;
            
            // Check time restrictions
            if (rule.TimeRestriction.IsEnabled)
            {
                var currentTime = DateTime.Now.TimeOfDay;
                if (currentTime < rule.TimeRestriction.StartTime || currentTime > rule.TimeRestriction.EndTime)
                    return false;
            }
            
            // Check day restrictions
            if (rule.ActiveDays.Any() && !rule.ActiveDays.Contains(DateTime.Now.DayOfWeek))
                return false;
            
            return CompareValues(latestReading.Value, rule.Value, rule.Operator);
        }
        
        private bool CompareValues(float value1, float value2, ComparisonOperator op)
        {
            return op switch
            {
                ComparisonOperator.GreaterThan => value1 > value2,
                ComparisonOperator.LessThan => value1 < value2,
                ComparisonOperator.Equals => Mathf.Approximately(value1, value2),
                ComparisonOperator.GreaterThanOrEqual => value1 >= value2,
                ComparisonOperator.LessThanOrEqual => value1 <= value2,
                ComparisonOperator.NotEquals => !Mathf.Approximately(value1, value2),
                _ => false
            };
        }
        
        private void ExecuteAutomationRule(AutomationRule rule)
        {
            rule.LastTriggered = DateTime.Now;
            rule.TimesTriggered++;
            
            foreach (var action in rule.Actions)
            {
                ExecuteAction(action, rule);
            }
            
            LogAutomationEvent(LogLevel.Info, "AutomationRule", $"Executed rule: {rule.RuleName}", rule.RuleId);
            OnAutomationTriggered?.Invoke(rule, GetLatestReading(rule.Trigger.SourceSensorId));
            _onAutomationTriggered?.Raise();
        }
        
        private void ExecuteAction(AutomationAction action, AutomationRule parentRule)
        {
            try
            {
                switch (action.ActionType)
                {
                    case ActionType.SetTemperature:
                        ExecuteHVACAction(action);
                        break;
                    
                    case ActionType.SetHumidity:
                        ExecuteHVACAction(action);
                        break;
                    
                    case ActionType.TurnOnLight:
                    case ActionType.TurnOffLight:
                    case ActionType.SetLightIntensity:
                        ExecuteLightingAction(action);
                        break;
                    
                    case ActionType.SendAlert:
                        ExecuteAlertAction(action, parentRule);
                        break;
                    
                    case ActionType.EmergencyShutdown:
                        ExecuteEmergencyShutdown(action);
                        break;
                    
                    case ActionType.LogEvent:
                        ExecuteLogAction(action);
                        break;
                }
                
                if (action.LogAction)
                {
                    LogAutomationEvent(LogLevel.Info, "AutomationAction", $"Executed action: {action.ActionType}", action.ActionId);
                }
            }
            catch (Exception ex)
            {
                LogAutomationEvent(LogLevel.Error, "AutomationAction", $"Failed to execute action {action.ActionType}: {ex.Message}", action.ActionId);
            }
        }
        
        private void ExecuteHVACAction(AutomationAction action)
        {
            // var hvacManager = GameManager.Instance.GetManager<HVACManager>(); // HVACManager type to be resolved - temporarily commented out
            // if (hvacManager == null) return;
            
            switch (action.ActionType)
            {
                case ActionType.SetTemperature:
                    if (action.Parameters.ContainsKey("temperature"))
                    {
                        float temperature = Convert.ToSingle(action.Parameters["temperature"]);
                        // Integration with HVAC system
                        LogInfo($"Setting temperature to {temperature}°C for zone {action.TargetZoneId}");
                    }
                    break;
                
                case ActionType.SetHumidity:
                    if (action.Parameters.ContainsKey("humidity"))
                    {
                        float humidity = Convert.ToSingle(action.Parameters["humidity"]);
                        LogInfo($"Setting humidity to {humidity}% for zone {action.TargetZoneId}");
                    }
                    break;
            }
        }
        
        private void ExecuteLightingAction(AutomationAction action)
        {
            // var lightingManager = GameManager.Instance.GetManager<LightingManager>(); // LightingManager type to be resolved - temporarily commented out
            // if (lightingManager == null) return;
            
            switch (action.ActionType)
            {
                case ActionType.TurnOnLight:
                    if (action.Parameters.ContainsKey("intensity"))
                    {
                        float intensity = Convert.ToSingle(action.Parameters["intensity"]);
                        LogInfo($"Turning on lights at {intensity * 100}% intensity for zone {action.TargetZoneId}");
                    }
                    break;
                
                case ActionType.TurnOffLight:
                    LogInfo($"Turning off lights for zone {action.TargetZoneId}");
                    break;
                
                case ActionType.SetLightIntensity:
                    if (action.Parameters.ContainsKey("intensity"))
                    {
                        float intensity = Convert.ToSingle(action.Parameters["intensity"]);
                        LogInfo($"Setting light intensity to {intensity * 100}% for zone {action.TargetZoneId}");
                    }
                    break;
            }
        }
        
        private void ExecuteAlertAction(AutomationAction action, AutomationRule parentRule)
        {
            var alert = new SmartAlert
            {
                AlertId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now,
                Severity = action.Parameters.ContainsKey("severity") ? 
                    (AlertSeverity)action.Parameters["severity"] : AlertSeverity.Warning,
                Title = $"Automation Alert: {parentRule.RuleName}",
                Description = action.Parameters.ContainsKey("message") ? 
                    action.Parameters["message"].ToString() : "Automation rule triggered",
                SourceSensorId = parentRule.Trigger.SourceSensorId,
                Status = AlertStatus.Active,
                RequiresImmediateAttention = ((AlertSeverity)action.Parameters.GetValueOrDefault("severity", AlertSeverity.Warning)) >= AlertSeverity.Critical
            };
            
            _activeAlerts.Add(alert);
            OnSmartAlert?.Invoke(alert);
            _onSensorAlert?.Raise();
        }
        
        private void ExecuteEmergencyShutdown(AutomationAction action)
        {
            string reason = action.Parameters.ContainsKey("reason") ? 
                action.Parameters["reason"].ToString() : "Emergency shutdown triggered";
            
            // Shutdown HVAC systems
            // var hvacManager = GameManager.Instance.GetManager<HVACManager>(); // HVACManager type to be resolved - temporarily commented out
            // if (hvacManager != null)
            // {
            //     hvacManager.EmergencyShutdown(reason);
            // }
            LogWarning($"Emergency shutdown initiated: {reason}");
            
            // Shutdown lighting systems
            // var lightingManager = GameManager.Instance.GetManager<LightingManager>(); // LightingManager type to be resolved - temporarily commented out
            // if (lightingManager != null)
            // {
            //     lightingManager.EmergencyShutdown(reason);
            // }
            
            // Create emergency alert
            var emergencyAlert = new SmartAlert
            {
                AlertId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now,
                Severity = AlertSeverity.Emergency,
                Title = "EMERGENCY SHUTDOWN",
                Description = reason,
                Status = AlertStatus.Active,
                RequiresImmediateAttention = true
            };
            
            _activeAlerts.Add(emergencyAlert);
            OnSmartAlert?.Invoke(emergencyAlert);
        }
        
        private void ExecuteLogAction(AutomationAction action)
        {
            string message = action.Parameters.ContainsKey("message") ? 
                action.Parameters["message"].ToString() : "Automation log event";
            
            LogInfo($"Automation Log: {message}");
        }
        
        private void ProcessPendingActions()
        {
            // Process any delayed actions
            // This would handle actions with DelaySeconds > 0
        }
        
        private void UpdatePredictiveModels()
        {
            if (!_enablePredictiveControl || _trainingData.Count < 100) return;
            
            foreach (var model in _predictiveModels.Values.Where(m => m.IsActive))
            {
                UpdateModelTraining(model);
            }
        }
        
        private void GeneratePredictions()
        {
            foreach (var model in _predictiveModels.Values.Where(m => m.IsActive))
            {
                var predictions = GenerateModelPredictions(model);
                if (predictions.Any())
                {
                    OnPredictionGenerated?.Invoke(model, predictions);
                    
                    // Check for prediction-based alerts
                    CheckPredictiveAlerts(model, predictions);
                }
            }
        }
        
        private void CheckPredictiveAlerts(PredictiveModel model, Dictionary<string, float> predictions)
        {
            foreach (var prediction in predictions)
            {
                if (prediction.Value > 0.8f) // High confidence prediction of issue
                {
                    var alert = new SmartAlert
                    {
                        AlertId = Guid.NewGuid().ToString(),
                        Timestamp = DateTime.Now,
                        Severity = AlertSeverity.Warning,
                        Title = $"Predictive Alert: {model.ModelName}",
                        Description = $"Model predicts potential issue with {prediction.Key} (confidence: {prediction.Value:P0})",
                        Status = AlertStatus.Active
                    };
                    
                    _activeAlerts.Add(alert);
                    _onPredictiveAlert?.Raise();
                }
            }
        }
        
        private void CheckDeviceHealth()
        {
            foreach (var device in _iotDevices.Values)
            {
                var timeSinceLastSeen = (DateTime.Now - device.LastSeen).TotalMinutes;
                
                if (timeSinceLastSeen > 15 && device.Status == IoTDeviceStatus.Online)
                {
                    device.Status = IoTDeviceStatus.Offline;
                    OnDeviceStatusChanged?.Invoke(device, IoTDeviceStatus.Offline);
                    _onDeviceStatusChanged?.Raise();
                    
                    LogWarning($"Device {device.DeviceName} is now offline");
                }
                
                if (device.Capabilities.HasBattery && device.BatteryLevel < 0.2f)
                {
                    device.Status = IoTDeviceStatus.LowBattery;
                    LogWarning($"Device {device.DeviceName} has low battery: {device.BatteryLevel:P0}");
                }
            }
        }
        
        private void CleanupOldData()
        {
            var cutoffTime = DateTime.Now.AddHours(-_automationSettings.DataRetentionHours);
            
            // Clean up old readings
            _recentReadings.RemoveAll(r => r.Timestamp < cutoffTime);
            
            // Clean up old logs
            _automationLogs.RemoveAll(l => l.Timestamp < cutoffTime);
            
            // Clean up resolved alerts older than 24 hours
            var alertCutoff = DateTime.Now.AddDays(-1);
            _activeAlerts.RemoveAll(a => a.Status == AlertStatus.Resolved && a.Timestamp < alertCutoff);
        }
        
        // Helper methods
        private bool ValidateSensorConfiguration(SensorConfiguration config)
        {
            return !string.IsNullOrEmpty(config.SensorId) &&
                   !string.IsNullOrEmpty(config.SensorName) &&
                   !string.IsNullOrEmpty(config.ZoneId) &&
                   config.ReadingInterval > 0 &&
                   config.Accuracy > 0 && config.Accuracy <= 1;
        }
        
        private float GetInitialSensorValue(SensorType sensorType)
        {
            return sensorType switch
            {
                SensorType.Temperature => 22f,
                SensorType.Humidity => 55f,
                SensorType.CO2 => 1000f,
                SensorType.Light_Intensity => 400f,
                SensorType.pH => 6.0f,
                SensorType.EC_Conductivity => 1.2f,
                _ => 0f
            };
        }
        
        private string GetSensorUnit(SensorType sensorType)
        {
            return sensorType switch
            {
                SensorType.Temperature => "°C",
                SensorType.Humidity => "%",
                SensorType.CO2 => "ppm",
                SensorType.Light_Intensity => "PPFD",
                SensorType.pH => "pH",
                SensorType.EC_Conductivity => "mS/cm",
                SensorType.VPD => "kPa",
                SensorType.DLI => "mol/m²/day",
                _ => ""
            };
        }
        
        private WidgetType GetWidgetTypeForSensor(SensorType sensorType)
        {
            return sensorType switch
            {
                SensorType.Temperature or SensorType.Humidity or SensorType.CO2 => WidgetType.Gauge,
                SensorType.Light_Intensity or SensorType.DLI => WidgetType.LineChart,
                SensorType.Motion or SensorType.Door_Status => WidgetType.Status_Light,
                _ => WidgetType.Number_Display
            };
        }
        
        private DateTime GetLastReadingTime(string sensorId)
        {
            var lastReading = _recentReadings
                .Where(r => r.SensorId == sensorId)
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefault();
            
            return lastReading?.Timestamp ?? DateTime.Now.AddDays(-1);
        }
        
        private SensorReading GenerateSensorReading(SensorConfiguration sensor)
        {
            // Simulate sensor reading based on current environmental conditions
            var baseValue = GetInitialSensorValue(sensor.SensorType);
            var variation = UnityEngine.Random.Range(-0.1f, 0.1f) * baseValue;
            var value = Mathf.Max(0, baseValue + variation);
            
            return new SensorReading
            {
                SensorId = sensor.SensorId,
                Timestamp = DateTime.Now,
                Value = value,
                Unit = GetSensorUnit(sensor.SensorType),
                Status = SensorReadingStatus.Valid,
                Confidence = sensor.Accuracy * UnityEngine.Random.Range(0.95f, 1.0f)
            };
        }
        
        private void CheckSensorAlarms(SensorConfiguration sensor, SensorReading reading)
        {
            if (!sensor.AlarmSettings.EnableAlarms) return;
            
            bool criticalHigh = reading.Value >= sensor.AlarmSettings.CriticalHighThreshold;
            bool criticalLow = reading.Value <= sensor.AlarmSettings.CriticalLowThreshold;
            bool high = reading.Value >= sensor.AlarmSettings.HighThreshold;
            bool low = reading.Value <= sensor.AlarmSettings.LowThreshold;
            
            if (criticalHigh || criticalLow)
            {
                CreateAlert(sensor, reading, AlertSeverity.Critical, 
                    criticalHigh ? "Critical high value" : "Critical low value");
            }
            else if (high || low)
            {
                CreateAlert(sensor, reading, AlertSeverity.Warning,
                    high ? "High value warning" : "Low value warning");
            }
        }
        
        private void CreateAlert(SensorConfiguration sensor, SensorReading reading, AlertSeverity severity, string description)
        {
            var alert = new SmartAlert
            {
                AlertId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now,
                Severity = severity,
                Title = $"Sensor Alert: {sensor.SensorName}",
                Description = $"{description}: {reading.Value:F1} {reading.Unit}",
                SourceSensorId = sensor.SensorId,
                ZoneId = sensor.ZoneId,
                Status = AlertStatus.Active,
                RequiresImmediateAttention = severity >= AlertSeverity.Critical
            };
            
            _activeAlerts.Add(alert);
            OnSmartAlert?.Invoke(alert);
            _onSensorAlert?.Raise();
        }
        
        private SensorReading GetLatestReading(string sensorId)
        {
            return _recentReadings
                .Where(r => r.SensorId == sensorId)
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefault();
        }
        
        private void LogAutomationEvent(LogLevel level, string component, string message, string relatedId = null)
        {
            var logEntry = new AutomationLog
            {
                Timestamp = DateTime.Now,
                LogLevel = level,
                Component = component,
                Message = message,
                RuleId = relatedId
            };
            
            _automationLogs.Add(logEntry);
            
            // Also log to Unity console
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    LogInfo($"[{component}] {message}");
                    break;
                case LogLevel.Warning:
                    LogWarning($"[{component}] {message}");
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    LogError($"[{component}] {message}");
                    break;
            }
        }
        
        private void UpdateModelTraining(PredictiveModel model)
        {
            // Simplified model training simulation
            var relevantData = _trainingData
                .Where(r => model.InputSensorIds.Contains(r.SensorId))
                .TakeLast(1000)
                .ToList();
            
            if (relevantData.Count >= 100)
            {
                model.TrainingDataPoints = relevantData.Count;
                model.Accuracy = UnityEngine.Random.Range(0.7f, 0.95f);
                model.LastTrained = DateTime.Now;
            }
        }
        
        private Dictionary<string, float> GenerateModelPredictions(PredictiveModel model)
        {
            var predictions = new Dictionary<string, float>();
            
            // Simulate predictions based on model type
            switch (model.ModelType)
            {
                case PredictiveModelType.Time_Series:
                    predictions["temperature_anomaly"] = UnityEngine.Random.Range(0f, 1f);
                    predictions["humidity_spike"] = UnityEngine.Random.Range(0f, 1f);
                    predictions["equipment_failure"] = UnityEngine.Random.Range(0f, 0.3f);
                    break;
                
                case PredictiveModelType.Anomaly_Detection:
                    predictions["anomaly_detected"] = UnityEngine.Random.Range(0f, 0.2f);
                    break;
            }
            
            return predictions;
        }
        
        private float CalculateSystemUptime(TimeSpan period)
        {
            // Calculate system uptime percentage
            var totalMinutes = period.TotalMinutes;
            var downtime = _automationLogs
                .Where(l => l.LogLevel == LogLevel.Error && l.Component == "System")
                .Count() * 5; // Assume 5 minutes downtime per error
            
            return Math.Max(0, (float)((totalMinutes - downtime) / totalMinutes * 100));
        }
        
        private float CalculateEnergySavings(TimeSpan period)
        {
            // Calculate energy savings from automation optimizations
            var optimizationActions = _automationLogs
                .Where(l => l.Message.Contains("optimization") || l.Message.Contains("efficiency"))
                .Count();
            
            return optimizationActions * 2.5f; // Estimate 2.5% savings per optimization action
        }
        
        private List<string> GetTopPerformingSensors(List<SensorReading> readings)
        {
            return readings
                .GroupBy(r => r.SensorId)
                .Where(g => g.Count() > 10)
                .OrderByDescending(g => g.Average(r => r.Confidence))
                .Take(5)
                .Select(g => g.Key)
                .ToList();
        }
        
        private List<string> GetMostTriggeredRules(TimeSpan period)
        {
            return _automationRules.Values
                .Where(r => r.TimesTriggered > 0)
                .OrderByDescending(r => r.TimesTriggered)
                .Take(5)
                .Select(r => r.RuleName)
                .ToList();
        }
        
        protected override void OnManagerShutdown()
        {
            _sensors.Clear();
            _iotDevices.Clear();
            _automationRules.Clear();
            _dashboards.Clear();
            _recentReadings.Clear();
            _activeAlerts.Clear();
            _automationLogs.Clear();
            _predictiveModels.Clear();
            
            LogInfo("AutomationManager shutdown complete");
        }
    }
    
    // Supporting report class
    [System.Serializable]
    public class AutomationReport
    {
        public TimeSpan ReportPeriod;
        public DateTime GeneratedAt;
        public int TotalSensorReadings;
        public int TotalAutomationActions;
        public int TotalAlerts;
        public int CriticalAlerts;
        public float SystemUptime; // Percentage
        public float EnergyOptimizationSavings; // Percentage
        public List<string> TopPerformingSensors;
        public List<string> MostTriggeredRules;
    }
}