using System.Collections.Generic;
using ProjectChimera.Data.Automation;

namespace ProjectChimera.Systems.Environment
{
    /// <summary>
    /// PC014-2b: Interface for sensor network management service
    /// </summary>
    public interface ISensorNetworkService : IEnvironmentalService
    {
        void RegisterSensor(string sensorId, SensorType type, string zoneId);
        SensorReading GetLatestReading(string sensorId);
        IEnumerable<SensorReading> GetHistoricalData(string sensorId, int hours);
        void CalibrateSensor(string sensorId, float calibrationOffset);
        void SetAlertThresholds(string sensorId, float minValue, float maxValue);
    }

    // Use SensorReading and SensorType from ProjectChimera.Data.Automation.AutomationDataStructures
    // to avoid duplicate definitions
}