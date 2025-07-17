using UnityEngine;
using System;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Environment;

namespace ProjectChimera.Data.Automation
{
    /// <summary>
    /// Comprehensive data structures for automation and IoT systems in Project Chimera.
    /// Handles sensor networks, automated responses, monitoring, and smart facility management.
    /// </summary>

    [System.Serializable]
    public class AutomationSettings
    {
        [Range(0.1f, 10f)] public float SensorUpdateFrequency = 1f; // updates per second
        [Range(0.1f, 60f)] public float AutomationResponseDelay = 0.5f; // seconds
        [Range(1, 1000)] public int MaxSensorsPerZone = 50;
        [Range(1, 100)] public int MaxAutomationRules = 20;
        public bool EnablePredictiveControl = true;
        public bool EnableLearningAlgorithms = true;
        public bool EnableRemoteMonitoring = true;
        [Range(1f, 72f)] public float DataRetentionHours = 24f;
    }

    [System.Serializable]
    public class SensorConfiguration
    {
        public string SensorId;
        public string SensorName;
        public SensorType SensorType;
        public Vector3 Position;
        public string ZoneId;
        [Range(0.1f, 3600f)] public float ReadingInterval = 60f; // seconds
        [Range(0.01f, 1f)] public float Accuracy = 0.95f;
        public bool IsActive = true;
        public bool RequiresCalibration = false;
        public DateTime LastCalibration;
        public float CalibrationInterval = 30f; // days
        public SensorAlarmSettings AlarmSettings;
    }

    [System.Serializable]
    public class SensorAlarmSettings
    {
        public bool EnableAlarms = true;
        public float HighThreshold = 100f;
        public float LowThreshold = 0f;
        public float CriticalHighThreshold = 150f;
        public float CriticalLowThreshold = -10f;
        public AlarmPriority AlarmPriority = AlarmPriority.Normal;
        public bool EnableEmailAlerts = false;
        public bool EnableSMSAlerts = false;
    }

    [System.Serializable]
    public class SensorReading
    {
        public string SensorId;
        public SensorType SensorType;
        public DateTime Timestamp;
        public float Value;
        public string Unit;
        public SensorReadingStatus Status;
        public float Confidence = 1f;
        public Dictionary<string, object> Metadata = new Dictionary<string, object>();
    }

    [System.Serializable]
    public class AutomationRule
    {
        public string RuleId;
        public string RuleName;
        public string Description;
        public bool IsEnabled = true;
        public AutomationTrigger Trigger;
        public List<AutomationAction> Actions = new List<AutomationAction>();
        public AutomationCondition Condition;
        public int Priority = 1;
        public float CooldownPeriod = 300f; // seconds
        public DateTime LastTriggered;
        public int TimesTriggered = 0;
        public bool RequiresConfirmation = false;
        public string CreatedBy = "System";
        public DateTime CreatedDate;
    }

    [System.Serializable]
    public class AutomationTrigger
    {
        public TriggerType TriggerType;
        public string SourceSensorId;
        public float TriggerValue;
        public ComparisonOperator Operator;
        public float TriggerDuration = 0f; // seconds (0 = immediate)
        public bool RequiresMultipleSensors = false;
        public List<string> AdditionalSensorIds = new List<string>();
    }

    [System.Serializable]
    public class AutomationAction
    {
        public string ActionId;
        public ActionType ActionType;
        public string TargetEquipmentId;
        public string TargetZoneId;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
        public float DelaySeconds = 0f;
        public int MaxRetries = 3;
        public bool LogAction = true;
        public bool RequiresConfirmation = false;
    }

    [System.Serializable]
    public class AutomationCondition
    {
        public List<ConditionRule> Rules = new List<ConditionRule>();
        public LogicalOperator LogicalOperator = LogicalOperator.And;
        public bool InvertResult = false;
    }

    [System.Serializable]
    public class ConditionRule
    {
        public string SensorId;
        public ComparisonOperator Operator;
        public float Value;
        public TimeOfDay TimeRestriction;
        public List<DayOfWeek> ActiveDays = new List<DayOfWeek>();
    }

    [System.Serializable]
    public class TimeOfDay
    {
        public bool IsEnabled = false;
        public TimeSpan StartTime = TimeSpan.FromHours(6);
        public TimeSpan EndTime = TimeSpan.FromHours(22);
    }

    [System.Serializable]
    public class MonitoringDashboard
    {
        public string DashboardId;
        public string DashboardName;
        public List<DashboardWidget> Widgets = new List<DashboardWidget>();
        public DashboardLayout Layout;
        public bool IsPublic = false;
        public List<string> AuthorizedUsers = new List<string>();
        public DateTime LastUpdated;
        public float RefreshRate = 5f; // seconds
    }

    [System.Serializable]
    public class DashboardWidget
    {
        public string WidgetId;
        public string WidgetName;
        public WidgetType WidgetType;
        public Vector2 Position;
        public Vector2 Size;
        public string DataSourceId; // Sensor ID or calculated metric
        public Dictionary<string, object> Configuration = new Dictionary<string, object>();
        public bool ShowAlerts = true;
        public TimeRange DataTimeRange = TimeRange.Last24Hours;
    }

    [System.Serializable]
    public class IoTDevice
    {
        public string DeviceId;
        public string DeviceName;
        public IoTDeviceType DeviceType;
        public string Manufacturer;
        public string Model;
        public string FirmwareVersion;
        public Vector3 Position;
        public string ZoneId;
        public IoTDeviceStatus Status;
        public DateTime LastSeen;
        public float BatteryLevel = 1f;
        public string NetworkAddress;
        public DeviceCapabilities Capabilities;
        public List<string> SensorIds = new List<string>();
        public bool RequiresFirmwareUpdate = false;
    }

    [System.Serializable]
    public class DeviceCapabilities
    {
        public bool SupportsRemoteControl = true;
        public bool SupportsOTA_Updates = true;
        public bool HasDisplay = false;
        public bool HasBattery = false;
        public bool SupportsWiFi = true;
        public bool SupportsBluetooth = false;
        public bool SupportsEthernet = false;
        public List<SensorType> SupportedSensors = new List<SensorType>();
        public List<string> SupportedProtocols = new List<string>();
    }

    [System.Serializable]
    public class AutomationLog
    {
        public DateTime Timestamp;
        public LogLevel LogLevel;
        public string Component;
        public string Message;
        public string RuleId;
        public string SensorId;
        public string ActionId;
        public Dictionary<string, object> Context = new Dictionary<string, object>();
    }

    [System.Serializable]
    public class PredictiveModel
    {
        public string ModelId;
        public string ModelName;
        public PredictiveModelType ModelType;
        public List<string> InputSensorIds = new List<string>();
        public string TargetVariable;
        public float Accuracy = 0f;
        public DateTime LastTrained;
        public int TrainingDataPoints = 0;
        public Dictionary<string, float> ModelParameters = new Dictionary<string, float>();
        public bool IsActive = false;
        public TimeSpan PredictionHorizon = TimeSpan.FromHours(4);
    }

    [System.Serializable]
    public class SmartAlert
    {
        public string AlertId;
        public DateTime Timestamp;
        public AlertSeverity Severity;
        public string Title;
        public string Description;
        public string SourceSensorId;
        public string ZoneId;
        public AlertStatus Status;
        public List<string> AffectedEquipment = new List<string>();
        public Dictionary<string, object> AlertData = new Dictionary<string, object>();
        public List<AlertAction> SuggestedActions = new List<AlertAction>();
        public bool RequiresImmediateAttention = false;
        public DateTime? AcknowledgedAt;
        public string AcknowledgedBy;
    }

    [System.Serializable]
    public class AlertAction
    {
        public string ActionDescription;
        public ActionType ActionType;
        public string TargetEquipmentId;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
        public bool IsAutomated = true;
        public float ConfidenceScore = 1f;
    }

    [System.Serializable]
    public class DeviceGroup
    {
        public string GroupId;
        public string GroupName;
        public List<string> DeviceIds;
        public DateTime CreatedDate;
    }

    // Enums for automation system
    public enum SensorType
    {
        Temperature,
        Humidity,
        CO2,
        CO2Level,
        Light_Intensity,
        LightLevel,
        Light_Spectrum,
        pH,
        EC_Conductivity,
        Water_Level,
        Air_Pressure,
        Air_Velocity,
        AirFlow,
        Soil_Moisture,
        Leaf_Temperature,
        VPD,
        PPFD,
        DLI,
        Motion,
        Door_Status,
        Power_Consumption,
        Vibration,
        Sound_Level
    }

    public enum TriggerType
    {
        Threshold_Exceeded,
        Threshold_Below,
        Value_Changed,
        Time_Based,
        Equipment_Status_Changed,
        Pattern_Detected,
        Prediction_Alert,
        Manual_Trigger,
        Emergency_Stop
    }

    public enum ActionType
    {
        SetEquipmentPower,
        SetEquipmentLevel,
        SetTemperature,
        SetHumidity,
        TurnOnLight,
        TurnOffLight,
        SetLightIntensity,
        SetLightSpectrum,
        StartIrrigation,
        StopIrrigation,
        SendAlert,
        LogEvent,
        RunCustomScript,
        EmergencyShutdown,
        SendNotification
    }

    public enum ComparisonOperator
    {
        GreaterThan,
        LessThan,
        Equals,
        GreaterThanOrEqual,
        LessThanOrEqual,
        NotEquals,
        Between,
        Outside
    }

    public enum LogicalOperator
    {
        And,
        Or,
        Xor,
        Not
    }

    public enum WidgetType
    {
        LineChart,
        BarChart,
        Gauge,
        Number_Display,
        Status_Light,
        Alert_List,
        Equipment_Grid,
        Map_View,
        Trend_Arrow,
        Progress_Bar
    }

    public enum TimeRange
    {
        Last1Hour,
        Last6Hours,
        Last24Hours,
        Last7Days,
        Last30Days,
        Custom
    }

    public enum IoTDeviceType
    {
        Sensor_Hub,
        Environmental_Sensor,
        Smart_Switch,
        Smart_Relay,
        Camera,
        Gateway,
        Controller,
        Display_Panel,
        Mobile_Device,
        Actuator
    }

    public enum IoTDeviceStatus
    {
        Online,
        Offline,
        Error,
        Updating,
        Standby,
        Maintenance,
        LowBattery,
        Calibrating
    }

    public enum SensorReadingStatus
    {
        Valid,
        Invalid,
        OutOfRange,
        CalibrationNeeded,
        SensorError,
        NoData
    }

    public enum AlarmPriority
    {
        Low,
        Normal,
        High,
        Critical,
        Emergency
    }

    public enum AlertSeverity
    {
        Info,
        Warning,
        Error,
        Critical,
        Emergency
    }

    public enum AlertStatus
    {
        Active,
        Acknowledged,
        Resolved,
        Dismissed,
        Escalated
    }

    public enum DashboardLayout
    {
        Grid,
        Flexible,
        Tabbed,
        Split_View,
        Full_Screen
    }

    /// <summary>
    /// Automation schedule for time-based automation rules and tasks
    /// </summary>
    [System.Serializable]
    public class AutomationSchedule
    {
        public string ScheduleId;
        public string ScheduleName;
        public string Description;
        public bool IsActive = true;
        public DateTime StartDate;
        public DateTime? EndDate;
        
        // Compatibility properties for testing
        public DateTime StartTime 
        { 
            get => StartDate; 
            set => StartDate = value; 
        }
        
        public DateTime EndTime 
        { 
            get => EndDate ?? StartDate.AddHours(8); 
            set => EndDate = value; 
        }
        
        public ScheduleRepeatPattern RepeatPattern { get; set; } = ScheduleRepeatPattern.Once;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
        
        public ScheduleType ScheduleType;
        public List<ScheduleTimeSlot> TimeSlots = new List<ScheduleTimeSlot>();
        public List<string> RuleIds = new List<string>();
        public string ZoneId;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
        public int Priority = 1;
        public bool AllowOverride = true;
        public DateTime LastExecuted;
        public DateTime NextExecution;
        public int ExecutionCount = 0;
    }

    /// <summary>
    /// Time slot configuration for automation schedules
    /// </summary>
    [System.Serializable]
    public class ScheduleTimeSlot
    {
        public DayOfWeek[] DaysOfWeek;
        public TimeSpan StartTime;
        public TimeSpan EndTime;
        public bool IsEnabled = true;
        public Dictionary<string, object> SlotParameters = new Dictionary<string, object>();
        
        // Compatibility property for testing
        public string ActionType 
        { 
            get => SlotParameters.ContainsKey("ActionType") ? SlotParameters["ActionType"].ToString() : "Default";
            set => SlotParameters["ActionType"] = value;
        }
    }

    public enum ScheduleType
    {
        OneTime,
        Daily,
        Weekly,
        Monthly,
        Interval,
        Conditional
    }

    /// <summary>
    /// Schedule repeat pattern enumeration for automation schedules
    /// </summary>
    public enum ScheduleRepeatPattern
    {
        Once,
        Daily,
        Weekly,
        Monthly,
        Yearly,
        Custom
    }

    public enum PredictiveModelType
    {
        Linear_Regression,
        Neural_Network,
        Decision_Tree,
        Time_Series,
        Pattern_Recognition,
        Anomaly_Detection
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }
}