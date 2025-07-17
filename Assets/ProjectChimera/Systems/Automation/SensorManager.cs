using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Automation;

namespace ProjectChimera.Systems.Automation
{
    /// <summary>
    /// Sensor Manager for Project Chimera
    /// Manages sensor network and data collection
    /// </summary>
    public class SensorManager : ChimeraManager
    {
        [Header("Sensor Configuration")]
        [SerializeField] private bool _enableSensorNetwork = true;
        [SerializeField] private float _sensorUpdateInterval = 1f;
        [SerializeField] private int _maxSensors = 100;
        [SerializeField] private bool _enableDataLogging = true;
        
        // Sensor data
        private Dictionary<string, SensorData> _registeredSensors = new Dictionary<string, SensorData>();
        private List<SensorReading> _recentReadings = new List<SensorReading>();
        
        // State
        private float _lastUpdateTime;
        
        // Events
        public System.Action<SensorReading> OnSensorReading;
        public System.Action<string> OnSensorConnected;
        public System.Action<string> OnSensorDisconnected;
        
        public string ManagerName => "Sensor";
        
        protected override void OnManagerInitialize()
        {
            _lastUpdateTime = Time.time;
            
            if (_enableSensorNetwork)
            {
                InvokeRepeating(nameof(UpdateSensors), 1f, _sensorUpdateInterval);
                InitializeDefaultSensors();
            }
            
            LogDebug("Sensor Manager initialized");
        }
        
        protected override void OnManagerShutdown()
        {
            CancelInvoke();
            LogDebug("Sensor Manager shutdown");
        }
        
        // Public interface methods
        public List<SensorData> GetAllSensors()
        {
            return new List<SensorData>(_registeredSensors.Values);
        }
        
        public List<SensorReading> GetRecentReadings(string sensorId = null)
        {
            if (string.IsNullOrEmpty(sensorId))
            {
                return new List<SensorReading>(_recentReadings);
            }
            
            return _recentReadings.FindAll(r => r.SensorId == sensorId);
        }
        
        public List<SensorReading> GetAllSensorReadings()
        {
            return new List<SensorReading>(_recentReadings);
        }
        
        public SensorData GetSensor(string sensorId)
        {
            return _registeredSensors.ContainsKey(sensorId) ? _registeredSensors[sensorId] : null;
        }
        
        public bool RegisterSensor(string sensorId, string sensorName, string sensorType, string zoneId)
        {
            if (_registeredSensors.ContainsKey(sensorId))
            {
                Debug.LogWarning($"Sensor {sensorId} already registered");
                return false;
            }
            
            if (_registeredSensors.Count >= _maxSensors)
            {
                Debug.LogError("Maximum number of sensors reached");
                return false;
            }
            
            var sensor = new SensorData
            {
                SensorId = sensorId,
                SensorName = sensorName,
                SensorType = sensorType,
                ZoneId = zoneId,
                IsOnline = true,
                LastReading = DateTime.Now,
                History = new List<SensorReading>()
            };
            
            _registeredSensors[sensorId] = sensor;
            OnSensorConnected?.Invoke(sensorId);
            
            Debug.Log($"Sensor registered: {sensorName} ({sensorId})");
            return true;
        }
        
        public void UnregisterSensor(string sensorId)
        {
            if (_registeredSensors.ContainsKey(sensorId))
            {
                _registeredSensors.Remove(sensorId);
                OnSensorDisconnected?.Invoke(sensorId);
                Debug.Log($"Sensor unregistered: {sensorId}");
            }
        }
        
        public void RecordSensorReading(string sensorId, float value, Dictionary<string, object> metadata = null)
        {
            if (!_registeredSensors.ContainsKey(sensorId))
            {
                Debug.LogWarning($"Sensor {sensorId} not registered");
                return;
            }
            
            var sensor = _registeredSensors[sensorId];
            var reading = new SensorReading
            {
                SensorId = sensorId,
                Value = value,
                Timestamp = DateTime.Now,
                Unit = GetSensorUnit(sensor.SensorType),
                Status = SensorReadingStatus.Valid,
                Confidence = 0.95f,
                Metadata = metadata ?? new Dictionary<string, object>()
            };
            
            // Update sensor data
            sensor.LastValue = value;
            sensor.LastReading = reading.Timestamp;
            sensor.History.Add(reading);
            
            // Limit history size
            if (sensor.History.Count > 1000)
            {
                sensor.History.RemoveAt(0);
            }
            
            // Add to recent readings
            _recentReadings.Add(reading);
            if (_recentReadings.Count > 10000)
            {
                _recentReadings.RemoveAt(0);
            }
            
            OnSensorReading?.Invoke(reading);
        }
        
        private string GetSensorUnit(string sensorType)
        {
            switch (sensorType.ToLower())
            {
                case "temperature":
                    return "°C";
                case "humidity":
                    return "%";
                case "ph":
                    return "pH";
                case "light":
                    return "PPFD";
                case "co2":
                    return "ppm";
                default:
                    return "";
            }
        }
        
        private void UpdateSensors()
        {
            foreach (var sensor in _registeredSensors.Values)
            {
                // Simulate sensor readings for development
                if (sensor.IsOnline)
                {
                    float simulatedValue = GenerateSimulatedReading(sensor.SensorType);
                    RecordSensorReading(sensor.SensorId, simulatedValue);
                }
            }
        }
        
        private float GenerateSimulatedReading(string sensorType)
        {
            switch (sensorType.ToLower())
            {
                case "temperature":
                    return UnityEngine.Random.Range(18f, 28f);
                case "humidity":
                    return UnityEngine.Random.Range(40f, 80f);
                case "ph":
                    return UnityEngine.Random.Range(5.5f, 7.5f);
                case "light":
                    return UnityEngine.Random.Range(200f, 1000f);
                case "co2":
                    return UnityEngine.Random.Range(300f, 1200f);
                default:
                    return UnityEngine.Random.Range(0f, 100f);
            }
        }
        
        private void InitializeDefaultSensors()
        {
            // Register some default sensors for development
            RegisterSensor("temp_01", "Temperature Sensor 1", "temperature", "zone_a");
            RegisterSensor("humid_01", "Humidity Sensor 1", "humidity", "zone_a");
            RegisterSensor("ph_01", "pH Sensor 1", "ph", "zone_a");
            RegisterSensor("light_01", "Light Sensor 1", "light", "zone_a");
            RegisterSensor("co2_01", "CO2 Sensor 1", "co2", "zone_a");
        }
    }
    
    [System.Serializable]
    public class SensorData
    {
        public string SensorId;
        public string SensorName;
        public string SensorType;
        public string ZoneId;
        public bool IsOnline;
        public float LastValue;
        public DateTime LastReading;
        public List<SensorReading> History;
    }
} 