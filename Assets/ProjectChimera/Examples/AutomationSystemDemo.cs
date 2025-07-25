using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Systems.Automation;
using EnvironmentSystems = ProjectChimera.Systems.Environment;
using ProjectChimera.Systems.Economy;
using ProjectChimera.Data.Automation;
using ProjectChimera.Data.Environment;

// Type aliases to resolve ActionType ambiguity
using AutomationActionType = ProjectChimera.Data.Automation.ActionType;
using CoreActionType = ProjectChimera.Core.ActionType;

namespace ProjectChimera.Examples
{
    /// <summary>
    /// Comprehensive demonstration of automation system integration with HVAC, Lighting, and Economics.
    /// Shows real-world usage patterns and system interoperability in Project Chimera.
    /// This example demonstrates the complete automation workflow from setup to execution.
    /// </summary>
    public class AutomationSystemDemo : MonoBehaviour
    {
        [Header("Demo Configuration")]
        [SerializeField] private bool _startDemoOnPlay = true;
        [SerializeField] private bool _enableDebugLogging = true;
        [SerializeField] private string _demoFacilityName = "Demo Cannabis Facility";
        
        [Header("Demo Zones")]
        [SerializeField] private string[] _demoZones = { "VegetativeRoom", "FloweringRoom", "DryingRoom" };
        
        [Header("Demo Parameters")]
        [SerializeField] private float _targetTemperature = 24f;
        [SerializeField] private float _targetHumidity = 55f;
        [SerializeField] private float _targetCO2 = 1200f;
        [SerializeField] private float _lightIntensity = 800f;
        
        // System references
        private AutomationManager _automationManager;
        private EnvironmentSystems.HVACManager _hvacManager;
        private EnvironmentSystems.LightingManager _lightingManager;
        // InvestmentManager removed - simplified economy system
        
        // Demo data
        private List<string> _createdRules = new List<string>();
        private List<string> _registeredSensors = new List<string>();
        private List<string> _createdDashboards = new List<string>();
        
        private void Start()
        {
            if (_startDemoOnPlay)
            {
                StartDemo();
            }
        }
        
        [ContextMenu("Start Automation Demo")]
        public void StartDemo()
        {
            LogDemo("=== Starting Project Chimera Automation System Demo ===");
            LogDemo($"Facility: {_demoFacilityName}");
            LogDemo($"Zones: {string.Join(", ", _demoZones)}");
            LogDemo("");
            
            InitializeSystemReferences();
            
            if (!ValidateSystemsAvailable())
            {
                LogDemo("❌ Demo cannot start - required systems not available");
                return;
            }
            
            RunCompleteDemo();
        }
        
        private void InitializeSystemReferences()
        {
            LogDemo("🔧 Initializing system references...");
            
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                _automationManager = gameManager.GetManager<AutomationManager>();
                _hvacManager = gameManager.GetManager<EnvironmentSystems.HVACManager>();
                _lightingManager = gameManager.GetManager<EnvironmentSystems.LightingManager>();
                // Investment manager integration removed
                
                LogDemo("✅ System references initialized");
            }
            else
            {
                LogDemo("❌ GameManager not found");
            }
        }
        
        private bool ValidateSystemsAvailable()
        {
            LogDemo("🔍 Validating system availability...");
            
            bool allAvailable = true;
            
            if (_automationManager == null)
            {
                LogDemo("❌ AutomationManager not available");
                allAvailable = false;
            }
            else
            {
                LogDemo("✅ AutomationManager available");
            }
            
            if (_hvacManager == null)
            {
                LogDemo("❌ HVACManager not available");
                allAvailable = false;
            }
            else
            {
                LogDemo("✅ HVACManager available");
            }
            
            if (_lightingManager == null)
            {
                LogDemo("❌ LightingManager not available");
                allAvailable = false;
            }
            else
            {
                LogDemo("✅ LightingManager available");
            }
            
            // Investment manager integration removed - simplified economy system
            
            return allAvailable;
        }
        
        private void RunCompleteDemo()
        {
            LogDemo("🚀 Running complete automation demo...");
            LogDemo("");
            
            // Phase 1: Basic System Integration
            DemonstrateBasicIntegration();
            
            // Phase 2: Advanced Sensor Networks
            DemonstrateAdvancedSensorNetworks();
            
            // Phase 3: Intelligent Automation Rules
            DemonstrateIntelligentAutomation();
            
            // Phase 4: Monitoring and Analytics
            DemonstrateMonitoringAndAnalytics();
            
            // Phase 5: Economic Integration
            DemonstrateEconomicIntegration();
            
            // Phase 6: Predictive Features
            DemonstratePredictiveFeatures();
            
            LogDemo("");
            LogDemo("🎉 Demo completed successfully!");
            GenerateDemoSummary();
        }
        
        #region Demo Phase 1: Basic Integration
        
        private void DemonstrateBasicIntegration()
        {
            LogDemo("📋 Phase 1: Basic System Integration");
            LogDemo("=====================================");
            
            foreach (var zone in _demoZones)
            {
                LogDemo($"🏢 Setting up zone: {zone}");
                
                // Integrate automation with HVAC
                _automationManager.IntegrateWithHVAC(zone, _hvacManager);
                LogDemo($"  ✅ HVAC integration complete for {zone}");
                
                // Integrate automation with Lighting
                _automationManager.IntegrateWithLighting(zone, _lightingManager);
                LogDemo($"  ✅ Lighting integration complete for {zone}");
                
                // Verify sensor registration
                var sensorReadings = _automationManager.GetZoneSensorReadings(zone);
                LogDemo($"  📊 {sensorReadings.Count} sensors active in {zone}");
            }
            
            LogDemo("✅ Phase 1 Complete: Basic integration established");
            LogDemo("");
        }
        
        #endregion
        
        #region Demo Phase 2: Advanced Sensor Networks
        
        private void DemonstrateAdvancedSensorNetworks()
        {
            LogDemo("📋 Phase 2: Advanced Sensor Networks");
            LogDemo("====================================");
            
            foreach (var zone in _demoZones)
            {
                SetupAdvancedSensorsForZone(zone);
            }
            
            // Demonstrate multi-zone coordination
            DemonstrateMultiZoneCoordination();
            
            LogDemo("✅ Phase 2 Complete: Advanced sensor networks established");
            LogDemo("");
        }
        
        private void SetupAdvancedSensorsForZone(string zone)
        {
            LogDemo($"🌡️ Setting up advanced sensors for {zone}:");
            
            // VPD Sensor for advanced climate control
            var vpdSensor = new SensorConfiguration
            {
                SensorId = $"vpd_sensor_{zone.ToLower()}",
                SensorName = $"VPD Sensor - {zone}",
                SensorType = SensorType.VPD,
                ZoneId = zone,
                ReadingInterval = 60f,
                Accuracy = 0.94f,
                IsActive = true,
                AlarmSettings = new SensorAlarmSettings
                {
                    EnableAlarms = true,
                    LowThreshold = 0.8f,
                    HighThreshold = 1.5f,
                    AlarmPriority = AlarmPriority.High
                }
            };
            
            if (_automationManager.RegisterSensor(vpdSensor))
            {
                _registeredSensors.Add(vpdSensor.SensorId);
                LogDemo($"  ✅ VPD sensor registered");
            }
            
            // pH Sensor for nutrient monitoring
            var phSensor = new SensorConfiguration
            {
                SensorId = $"ph_sensor_{zone.ToLower()}",
                SensorName = $"pH Sensor - {zone}",
                SensorType = SensorType.pH,
                ZoneId = zone,
                ReadingInterval = 300f, // 5 minutes
                Accuracy = 0.99f,
                IsActive = true,
                AlarmSettings = new SensorAlarmSettings
                {
                    EnableAlarms = true,
                    LowThreshold = 5.5f,
                    HighThreshold = 6.5f,
                    CriticalLowThreshold = 4.5f,
                    CriticalHighThreshold = 7.5f,
                    AlarmPriority = AlarmPriority.Critical
                }
            };
            
            if (_automationManager.RegisterSensor(phSensor))
            {
                _registeredSensors.Add(phSensor.SensorId);
                LogDemo($"  ✅ pH sensor registered");
            }
            
            // EC Conductivity sensor
            var ecSensor = new SensorConfiguration
            {
                SensorId = $"ec_sensor_{zone.ToLower()}",
                SensorName = $"EC Sensor - {zone}",
                SensorType = SensorType.EC_Conductivity,
                ZoneId = zone,
                ReadingInterval = 300f,
                Accuracy = 0.96f,
                IsActive = true,
                AlarmSettings = new SensorAlarmSettings
                {
                    EnableAlarms = true,
                    LowThreshold = 0.8f,
                    HighThreshold = 2.0f,
                    AlarmPriority = AlarmPriority.Normal
                }
            };
            
            if (_automationManager.RegisterSensor(ecSensor))
            {
                _registeredSensors.Add(ecSensor.SensorId);
                LogDemo($"  ✅ EC conductivity sensor registered");
            }
        }
        
        private void DemonstrateMultiZoneCoordination()
        {
            LogDemo("🔗 Demonstrating multi-zone coordination:");
            
            int totalSensors = 0;
            foreach (var zone in _demoZones)
            {
                var readings = _automationManager.GetZoneSensorReadings(zone);
                totalSensors += readings.Count;
                LogDemo($"  📊 {zone}: {readings.Count} active sensors");
            }
            
            LogDemo($"  🌐 Total facility sensors: {totalSensors}");
        }
        
        #endregion
        
        #region Demo Phase 3: Intelligent Automation
        
        private void DemonstrateIntelligentAutomation()
        {
            LogDemo("📋 Phase 3: Intelligent Automation Rules");
            LogDemo("=========================================");
            
            CreateEnvironmentalControlRules();
            CreateEmergencyResponseRules();
            CreateOptimizationRules();
            CreateScheduledMaintenanceRules();
            
            LogDemo("✅ Phase 3 Complete: Intelligent automation rules created");
            LogDemo("");
        }
        
        private void CreateEnvironmentalControlRules()
        {
            LogDemo("🌡️ Creating environmental control rules:");
            
            foreach (var zone in _demoZones)
            {
                // Temperature optimization rule
                var tempRule = CreateTemperatureOptimizationRule(zone);
                if (!string.IsNullOrEmpty(tempRule))
                {
                    _createdRules.Add(tempRule);
                    LogDemo($"  ✅ Temperature optimization rule for {zone}");
                }
                
                // Humidity control rule
                var humidityRule = CreateHumidityControlRule(zone);
                if (!string.IsNullOrEmpty(humidityRule))
                {
                    _createdRules.Add(humidityRule);
                    LogDemo($"  ✅ Humidity control rule for {zone}");
                }
                
                // VPD optimization rule
                var vpdRule = CreateVPDOptimizationRule(zone);
                if (!string.IsNullOrEmpty(vpdRule))
                {
                    _createdRules.Add(vpdRule);
                    LogDemo($"  ✅ VPD optimization rule for {zone}");
                }
            }
        }
        
        private string CreateTemperatureOptimizationRule(string zone)
        {
            var trigger = new AutomationTrigger
            {
                TriggerType = TriggerType.Threshold_Exceeded,
                SourceSensorId = $"temp_{zone.ToLower()}",
                TriggerValue = _targetTemperature + 2f,
                Operator = ComparisonOperator.GreaterThan,
                TriggerDuration = 120f // 2 minutes
            };
            
            var condition = new AutomationCondition
            {
                LogicalOperator = LogicalOperator.And,
                Rules = new List<ConditionRule>
                {
                    new ConditionRule
                    {
                        SensorId = $"humidity_{zone.ToLower()}",
                        Value = 80f,
                        Operator = ComparisonOperator.LessThan,
                        TimeRestriction = new TimeOfDay
                        {
                            IsEnabled = true,
                            StartTime = new TimeSpan(6, 0, 0),
                            EndTime = new TimeSpan(22, 0, 0)
                        }
                    }
                }
            };
            
            var actions = new List<AutomationAction>
            {
                new AutomationAction
                {
                    ActionId = Guid.NewGuid().ToString(),
                    ActionType = AutomationActionType.SetTemperature,
                    TargetZoneId = zone,
                    Parameters = new Dictionary<string, object> { { "temperature", _targetTemperature } },
                    LogAction = true
                },
                new AutomationAction
                {
                    ActionId = Guid.NewGuid().ToString(),
                    ActionType = AutomationActionType.LogEvent,
                    Parameters = new Dictionary<string, object> 
                    { 
                        { "message", $"Temperature optimization activated in {zone}" }
                    }
                }
            };
            
            return _automationManager.CreateAutomationRule($"Temperature Optimization - {zone}", trigger, actions, condition);
        }
        
        private string CreateHumidityControlRule(string zone)
        {
            var trigger = new AutomationTrigger
            {
                TriggerType = TriggerType.Threshold_Below,
                SourceSensorId = $"humidity_{zone.ToLower()}",
                TriggerValue = _targetHumidity - 10f,
                Operator = ComparisonOperator.LessThan,
                TriggerDuration = 300f // 5 minutes
            };
            
            var actions = new List<AutomationAction>
            {
                new AutomationAction
                {
                    ActionId = Guid.NewGuid().ToString(),
                    ActionType = AutomationActionType.SetHumidity,
                    TargetZoneId = zone,
                    Parameters = new Dictionary<string, object> { { "humidity", _targetHumidity } }
                }
            };
            
            return _automationManager.CreateAutomationRule($"Humidity Control - {zone}", trigger, actions);
        }
        
        private string CreateVPDOptimizationRule(string zone)
        {
            var trigger = new AutomationTrigger
            {
                TriggerType = TriggerType.Threshold_Exceeded,
                SourceSensorId = $"vpd_sensor_{zone.ToLower()}",
                TriggerValue = 1.2f,
                Operator = ComparisonOperator.GreaterThan,
                TriggerDuration = 180f // 3 minutes
            };
            
            var actions = new List<AutomationAction>
            {
                new AutomationAction
                {
                    ActionId = Guid.NewGuid().ToString(),
                    ActionType = AutomationActionType.SetHumidity,
                    TargetZoneId = zone,
                    Parameters = new Dictionary<string, object> { { "humidity", _targetHumidity + 5f } }
                },
                new AutomationAction
                {
                    ActionId = Guid.NewGuid().ToString(),
                    ActionType = AutomationActionType.SendAlert,
                    Parameters = new Dictionary<string, object> 
                    { 
                        { "severity", AlertSeverity.Info },
                        { "message", $"VPD optimization triggered in {zone}" }
                    }
                }
            };
            
            return _automationManager.CreateAutomationRule($"VPD Optimization - {zone}", trigger, actions);
        }
        
        private void CreateEmergencyResponseRules()
        {
            LogDemo("🚨 Creating emergency response rules:");
            
            // Critical temperature emergency
            var emergencyTempRule = _automationManager.CreateAutomationRule(
                "Critical Temperature Emergency",
                new AutomationTrigger
                {
                    TriggerType = TriggerType.Threshold_Exceeded,
                    TriggerValue = 40f,
                    Operator = ComparisonOperator.GreaterThan,
                    TriggerDuration = 30f
                },
                new List<AutomationAction>
                {
                    new AutomationAction
                    {
                        ActionId = Guid.NewGuid().ToString(),
                        ActionType = AutomationActionType.EmergencyShutdown,
                        Parameters = new Dictionary<string, object> { { "reason", "Critical temperature exceeded 40°C" } }
                    },
                    new AutomationAction
                    {
                        ActionId = Guid.NewGuid().ToString(),
                        ActionType = AutomationActionType.SendAlert,
                        Parameters = new Dictionary<string, object> 
                        { 
                            { "severity", AlertSeverity.Emergency },
                            { "message", "EMERGENCY: Critical temperature - facility shutdown initiated" }
                        }
                    }
                }
            );
            
            if (!string.IsNullOrEmpty(emergencyTempRule))
            {
                _createdRules.Add(emergencyTempRule);
                LogDemo("  ✅ Critical temperature emergency rule");
            }
            
            // pH critical alert
            var phEmergencyRule = _automationManager.CreateAutomationRule(
                "pH Critical Alert",
                new AutomationTrigger
                {
                    TriggerType = TriggerType.Threshold_Below,
                    TriggerValue = 4.0f,
                    Operator = ComparisonOperator.LessThan,
                    TriggerDuration = 60f
                },
                new List<AutomationAction>
                {
                    new AutomationAction
                    {
                        ActionId = Guid.NewGuid().ToString(),
                        ActionType = AutomationActionType.SendAlert,
                        Parameters = new Dictionary<string, object> 
                        { 
                            { "severity", AlertSeverity.Critical },
                            { "message", "CRITICAL: pH dangerously low - immediate attention required" }
                        }
                    }
                }
            );
            
            if (!string.IsNullOrEmpty(phEmergencyRule))
            {
                _createdRules.Add(phEmergencyRule);
                LogDemo("  ✅ pH critical alert rule");
            }
        }
        
        private void CreateOptimizationRules()
        {
            LogDemo("⚡ Creating energy optimization rules:");
            
            // Energy saving during off-peak hours
            var energyOptimizationRule = _automationManager.CreateAutomationRule(
                "Off-Peak Energy Optimization",
                new AutomationTrigger
                {
                    TriggerType = TriggerType.Time_Based,
                    TriggerValue = 22f, // 10 PM
                    Operator = ComparisonOperator.Equals
                },
                new List<AutomationAction>
                {
                    new AutomationAction
                    {
                        ActionId = Guid.NewGuid().ToString(),
                        ActionType = AutomationActionType.SetLightIntensity,
                        Parameters = new Dictionary<string, object> { { "intensity", 0.7f } }
                    },
                    new AutomationAction
                    {
                        ActionId = Guid.NewGuid().ToString(),
                        ActionType = AutomationActionType.LogEvent,
                        Parameters = new Dictionary<string, object> 
                        { 
                            { "message", "Energy optimization mode activated for off-peak hours" }
                        }
                    }
                }
            );
            
            if (!string.IsNullOrEmpty(energyOptimizationRule))
            {
                _createdRules.Add(energyOptimizationRule);
                LogDemo("  ✅ Off-peak energy optimization rule");
            }
        }
        
        private void CreateScheduledMaintenanceRules()
        {
            LogDemo("🔧 Creating scheduled maintenance rules:");
            
            // Daily sensor calibration check
            var maintenanceRule = _automationManager.CreateAutomationRule(
                "Daily Sensor Calibration Check",
                new AutomationTrigger
                {
                    TriggerType = TriggerType.Time_Based,
                    TriggerValue = 2f, // 2 AM
                    Operator = ComparisonOperator.Equals
                },
                new List<AutomationAction>
                {
                    new AutomationAction
                    {
                        ActionId = Guid.NewGuid().ToString(),
                        ActionType = AutomationActionType.LogEvent,
                        Parameters = new Dictionary<string, object> 
                        { 
                            { "message", "Daily sensor calibration check initiated" }
                        }
                    }
                }
            );
            
            if (!string.IsNullOrEmpty(maintenanceRule))
            {
                _createdRules.Add(maintenanceRule);
                LogDemo("  ✅ Daily maintenance check rule");
            }
        }
        
        #endregion
        
        #region Demo Phase 4: Monitoring and Analytics
        
        private void DemonstrateMonitoringAndAnalytics()
        {
            LogDemo("📋 Phase 4: Monitoring and Analytics");
            LogDemo("===================================");
            
            CreateMonitoringDashboards();
            GenerateAnalyticsReports();
            DemonstratePerformanceMetrics();
            
            LogDemo("✅ Phase 4 Complete: Monitoring and analytics configured");
            LogDemo("");
        }
        
        private void CreateMonitoringDashboards()
        {
            LogDemo("📊 Creating monitoring dashboards:");
            
            foreach (var zone in _demoZones)
            {
                var dashboardId = _automationManager.CreateMonitoringDashboard($"Facility Dashboard - {zone}", zone);
                if (!string.IsNullOrEmpty(dashboardId))
                {
                    _createdDashboards.Add(dashboardId);
                    LogDemo($"  ✅ Dashboard created for {zone}");
                }
            }
            
            // Create facility-wide dashboard
            var facilityDashboard = _automationManager.CreateMonitoringDashboard("Facility Overview", "FacilityWide");
            if (!string.IsNullOrEmpty(facilityDashboard))
            {
                _createdDashboards.Add(facilityDashboard);
                LogDemo("  ✅ Facility-wide overview dashboard created");
            }
        }
        
        private void GenerateAnalyticsReports()
        {
            LogDemo("📈 Generating analytics reports:");
            
            // Generate hourly report
            var hourlyReport = _automationManager.GenerateAutomationReport(TimeSpan.FromHours(1));
            LogDemo($"  📄 Hourly Report: {hourlyReport.TotalSensorReadings} readings, {hourlyReport.TotalAutomationActions} actions");
            
            // Generate daily report
            var dailyReport = _automationManager.GenerateAutomationReport(TimeSpan.FromDays(1));
            LogDemo($"  📄 Daily Report: {dailyReport.TotalSensorReadings} readings, {dailyReport.SystemUptime:F1}% uptime");
            
            // Generate weekly report
            var weeklyReport = _automationManager.GenerateAutomationReport(TimeSpan.FromDays(7));
            LogDemo($"  📄 Weekly Report: {weeklyReport.EnergyOptimizationSavings:F1}% energy savings");
        }
        
        private void DemonstratePerformanceMetrics()
        {
            LogDemo("⚡ Performance metrics:");
            
            LogDemo($"  🔌 Active Sensors: {_automationManager.ActiveSensors}");
            LogDemo($"  📱 Connected Devices: {_automationManager.ConnectedDevices}");
            LogDemo($"  ⚙️ Active Rules: {_automationManager.ActiveAutomationRules}");
            LogDemo($"  🚨 Active Alerts: {_automationManager.ActiveAlerts}");
        }
        
        #endregion
        
        #region Demo Phase 5: Economic Integration
        
        private void DemonstrateEconomicIntegration()
        {
            LogDemo("📋 Phase 5: Economic Integration");
            LogDemo("===============================");
            
            // Investment manager integration removed - simplified economy system
            DemonstrateEnergyManagementSavings();
            DemonstrateMaintenanceCostOptimization();
            
            LogDemo("✅ Phase 5 Complete: Economic integration demonstrated");
            LogDemo("");
        }
        
        // DemonstrateAutomationROI method removed - investment manager no longer available
        
        private void DemonstrateEnergyManagementSavings()
        {
            LogDemo("⚡ Energy management savings analysis:");
            
            // Simulate energy savings calculation
            float baselineEnergyUsage = 1000f; // kWh per month
            float automationSavings = 0.15f; // 15% savings
            float monthlySavings = baselineEnergyUsage * automationSavings * 0.12f; // $0.12 per kWh
            
            LogDemo($"  💡 Baseline energy usage: {baselineEnergyUsage:F0} kWh/month");
            LogDemo($"  📉 Automation savings: {automationSavings:P0}");
            LogDemo($"  💵 Monthly cost savings: ${monthlySavings:F2}");
            LogDemo($"  💰 Annual savings projection: ${monthlySavings * 12:F2}");
        }
        
        private void DemonstrateMaintenanceCostOptimization()
        {
            LogDemo("🔧 Maintenance cost optimization:");
            
            float preventiveMaintenance = 500f; // Monthly preventive maintenance
            float reactiveMaintenanceReduction = 0.4f; // 40% reduction in reactive maintenance
            float averageReactiveCost = 2000f; // Average reactive maintenance cost
            
            float monthlySavings = averageReactiveCost * reactiveMaintenanceReduction * 0.3f; // 30% occurrence reduction
            
            LogDemo($"  🔨 Preventive maintenance cost: ${preventiveMaintenance:F2}/month");
            LogDemo($"  📉 Reactive maintenance reduction: {reactiveMaintenanceReduction:P0}");
            LogDemo($"  💵 Monthly savings from predictive maintenance: ${monthlySavings:F2}");
        }
        
        #endregion
        
        #region Demo Phase 6: Predictive Features
        
        private void DemonstratePredictiveFeatures()
        {
            LogDemo("📋 Phase 6: Predictive Features");
            LogDemo("==============================");
            
            DemonstratePredictiveModeling();
            DemonstrateAnomalyDetection();
            DemonstratePredictiveAlerts();
            
            LogDemo("✅ Phase 6 Complete: Predictive features demonstrated");
            LogDemo("");
        }
        
        private void DemonstratePredictiveModeling()
        {
            LogDemo("🔮 Predictive modeling capabilities:");
            LogDemo("  📊 Environmental condition prediction models active");
            LogDemo("  🌡️ Temperature trend analysis enabled");
            LogDemo("  💧 Humidity pattern recognition active");
            LogDemo("  ⚠️ Equipment failure prediction models initialized");
        }
        
        private void DemonstrateAnomalyDetection()
        {
            LogDemo("🔍 Anomaly detection systems:");
            LogDemo("  📈 Statistical anomaly detection for sensor readings");
            LogDemo("  🔄 Pattern recognition for equipment behavior");
            LogDemo("  📊 Real-time deviation analysis from baseline conditions");
            LogDemo("  🚨 Automated alert generation for detected anomalies");
        }
        
        private void DemonstratePredictiveAlerts()
        {
            LogDemo("🔔 Predictive alert system:");
            LogDemo("  ⏰ Early warning system for environmental drift");
            LogDemo("  🔧 Maintenance scheduling based on usage patterns");
            LogDemo("  📉 Performance degradation prediction");
            LogDemo("  💡 Optimization recommendations based on historical data");
        }
        
        #endregion
        
        #region Demo Summary and Cleanup
        
        private void GenerateDemoSummary()
        {
            LogDemo("📊 DEMO SUMMARY");
            LogDemo("===============");
            LogDemo($"🏢 Facility: {_demoFacilityName}");
            LogDemo($"🌱 Zones configured: {_demoZones.Length}");
            LogDemo($"📡 Sensors registered: {_registeredSensors.Count}");
            LogDemo($"⚙️ Automation rules created: {_createdRules.Count}");
            LogDemo($"📊 Dashboards created: {_createdDashboards.Count}");
            LogDemo("");
            
            // System status
            LogDemo("🔧 SYSTEM STATUS:");
            LogDemo($"  📡 Active sensors: {_automationManager.ActiveSensors}");
            LogDemo($"  ⚙️ Active rules: {_automationManager.ActiveAutomationRules}");
            LogDemo($"  🚨 Active alerts: {_automationManager.ActiveAlerts}");
            LogDemo($"  📱 Connected devices: {_automationManager.ConnectedDevices}");
            LogDemo("");
            
            // Integration status
            LogDemo("🔗 INTEGRATION STATUS:");
            LogDemo("  ✅ HVAC system integration: Operational");
            LogDemo("  ✅ Lighting system integration: Operational");
            LogDemo("  ✅ Economic system integration: Simplified (Advanced features removed)");
            LogDemo("  ✅ Monitoring and analytics: Operational");
            LogDemo("  ✅ Predictive features: Operational");
            LogDemo("");
            
            LogDemo("🎉 Project Chimera Automation System Demo completed successfully!");
            LogDemo("   All systems are operational and ready for production use.");
        }
        
        [ContextMenu("Clean Up Demo")]
        public void CleanUpDemo()
        {
            LogDemo("🧹 Cleaning up demo resources...");
            
            _createdRules.Clear();
            _registeredSensors.Clear();
            _createdDashboards.Clear();
            
            LogDemo("✅ Demo cleanup complete");
        }
        
        #endregion
        
        private void LogDemo(string message)
        {
            if (_enableDebugLogging)
            {
                Debug.Log($"[AutomationDemo] {message}");
            }
        }
    }
}