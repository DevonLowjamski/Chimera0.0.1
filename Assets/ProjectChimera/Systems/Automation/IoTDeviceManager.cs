using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Automation;

namespace ProjectChimera.Systems.Automation
{
    /// <summary>
    /// IoT Device Manager for Project Chimera
    /// Manages IoT device network and communication
    /// </summary>
    public class IoTDeviceManager : ChimeraManager
    {
        [Header("IoT Configuration")]
        [SerializeField] private bool _enableIoTNetwork = true;
        [SerializeField] private float _deviceScanInterval = 10f;
        [SerializeField] private int _maxDevices = 50;
        [SerializeField] private bool _enableAutoDiscovery = true;
        
        // Device data
        private Dictionary<string, IoTDevice> _connectedDevices = new Dictionary<string, IoTDevice>();
        private List<DeviceGroup> _deviceGroups = new List<DeviceGroup>();
        
        // State
        private float _lastScanTime;
        
        // Events
        public System.Action<IoTDevice> OnDeviceConnected;
        public System.Action<string> OnDeviceDisconnected;
        public System.Action<IoTDevice, string, object> OnDevicePropertyChanged;
        
        public string ManagerName => "IoTDevice";
        
        protected override void OnManagerInitialize()
        {
            _lastScanTime = Time.time;
            
            if (_enableIoTNetwork)
            {
                if (_enableAutoDiscovery)
                {
                    InvokeRepeating(nameof(ScanForDevices), 2f, _deviceScanInterval);
                }
                
                InitializeDefaultDevices();
            }
            
            LogDebug("IoT Device Manager initialized");
        }
        
        protected override void OnManagerShutdown()
        {
            DisconnectAllDevices();
            CancelInvoke();
            LogDebug("IoT Device Manager shutdown");
        }
        
        // Public interface methods
        public List<IoTDevice> GetAllDevices()
        {
            return new List<IoTDevice>(_connectedDevices.Values);
        }
        
        public List<IoTDevice> GetDevicesByType(IoTDeviceType deviceType)
        {
            var devices = new List<IoTDevice>();
            foreach (var device in _connectedDevices.Values)
            {
                if (device.DeviceType == deviceType)
                {
                    devices.Add(device);
                }
            }
            return devices;
        }
        
        public IoTDevice GetDevice(string deviceId)
        {
            return _connectedDevices.ContainsKey(deviceId) ? _connectedDevices[deviceId] : null;
        }
        
        public bool ConnectDevice(string deviceId, string deviceName, IoTDeviceType deviceType, string ipAddress = null)
        {
            if (_connectedDevices.ContainsKey(deviceId))
            {
                Debug.LogWarning($"Device {deviceId} already connected");
                return false;
            }
            
            if (_connectedDevices.Count >= _maxDevices)
            {
                Debug.LogError("Maximum number of devices reached");
                return false;
            }
            
            var device = new IoTDevice
            {
                DeviceId = deviceId,
                DeviceName = deviceName,
                DeviceType = deviceType,
                Status = IoTDeviceStatus.Online,
                NetworkAddress = ipAddress ?? GenerateSimulatedIP(),
                LastSeen = DateTime.Now,
                BatteryLevel = 1.0f,
                Capabilities = new DeviceCapabilities()
            };
            
            _connectedDevices[deviceId] = device;
            OnDeviceConnected?.Invoke(device);
            
            Debug.Log($"Device connected: {deviceName} ({deviceId})");
            return true;
        }
        
        public void DisconnectDevice(string deviceId)
        {
            if (_connectedDevices.ContainsKey(deviceId))
            {
                _connectedDevices.Remove(deviceId);
                OnDeviceDisconnected?.Invoke(deviceId);
                Debug.Log($"Device disconnected: {deviceId}");
            }
        }
        
        public void SetDeviceProperty(string deviceId, string propertyName, object value)
        {
            if (!_connectedDevices.ContainsKey(deviceId))
            {
                Debug.LogWarning($"Device {deviceId} not connected");
                return;
            }
            
            var device = _connectedDevices[deviceId];
            device.LastSeen = DateTime.Now;
            
            OnDevicePropertyChanged?.Invoke(device, propertyName, value);
        }
        
        public T GetDeviceProperty<T>(string deviceId, string propertyName, T defaultValue = default(T))
        {
            if (!_connectedDevices.ContainsKey(deviceId))
            {
                return defaultValue;
            }
            
            var device = _connectedDevices[deviceId];
            return defaultValue;
        }
        
        public DeviceGroup CreateDeviceGroup(string groupName, List<string> deviceIds)
        {
            var group = new DeviceGroup
            {
                GroupId = Guid.NewGuid().ToString(),
                GroupName = groupName,
                DeviceIds = deviceIds ?? new List<string>(),
                CreatedDate = DateTime.Now
            };
            
            _deviceGroups.Add(group);
            Debug.Log($"Device group created: {groupName}");
            return group;
        }
        
        public List<DeviceGroup> GetDeviceGroups()
        {
            return new List<DeviceGroup>(_deviceGroups);
        }
        
        private void ScanForDevices()
        {
            // Simulate device discovery
            if (UnityEngine.Random.Range(0f, 1f) < 0.1f) // 10% chance to discover new device
            {
                string deviceId = $"device_{UnityEngine.Random.Range(1000, 9999)}";
                string deviceName = $"IoT Device {deviceId.Substring(deviceId.Length - 4)}";
                IoTDeviceType deviceType = (IoTDeviceType)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(IoTDeviceType)).Length);
                
                ConnectDevice(deviceId, deviceName, deviceType);
            }
            
            // Update device status
            foreach (var device in _connectedDevices.Values)
            {
                device.LastSeen = DateTime.Now;
                
                // Simulate some device activity - just update LastSeen for now
                // Since we removed Properties dictionary, we don't set individual properties
            }
        }
        
        private string GenerateSimulatedIP()
        {
            return $"192.168.1.{UnityEngine.Random.Range(10, 254)}";
        }
        
        private void InitializeDefaultDevices()
        {
            // Connect some default devices for development
            ConnectDevice("pump_01", "Water Pump 1", IoTDeviceType.Actuator, "192.168.1.10");
            ConnectDevice("fan_01", "Ventilation Fan 1", IoTDeviceType.Actuator, "192.168.1.11");
            ConnectDevice("light_ctrl_01", "Light Controller 1", IoTDeviceType.Controller, "192.168.1.12");
            ConnectDevice("cam_01", "Security Camera 1", IoTDeviceType.Camera, "192.168.1.13");
        }
        
        private void CheckNetworkHealth()
        {
            foreach (var device in _connectedDevices.Values)
            {
                var timeSinceLastSeen = (DateTime.Now - device.LastSeen).TotalSeconds;
                if (timeSinceLastSeen > 30 && device.Status == IoTDeviceStatus.Online) // 30 seconds timeout
                {
                    Debug.LogWarning($"Device {device.DeviceId} appears offline");
                    device.Status = IoTDeviceStatus.Offline;
                }
            }
        }
        
        private void DisconnectAllDevices()
        {
            // Disconnect all devices gracefully
            foreach (var device in _connectedDevices.Values)
            {
                DisconnectDevice(device.DeviceId);
            }
            _connectedDevices.Clear();
        }
    }
} 