using UnityEngine;
using UnityEngine.UIElements;
using ProjectChimera.UI.Core;
using System;

namespace ProjectChimera.UI.Core
{
    /// <summary>
    /// Environmental status HUD component showing real-time environmental conditions.
    /// Displays temperature, humidity, CO2 levels, light intensity, and air circulation data.
    /// </summary>
    public class EnvironmentalStatusHUD : IDataBinding
    {
        private VisualElement _rootElement;
        private Label _temperatureLabel;
        private Label _humidityLabel;
        private Label _co2Label;
        private Label _lightLabel;
        private Label _airFlowLabel;
        private ProgressBar _temperatureBar;
        private ProgressBar _humidityBar;
        private ProgressBar _co2Bar;
        private VisualElement _statusIndicator;
        
        private EnvironmentalData _currentData;
        
        public VisualElement CreateHUDElement()
        {
            _rootElement = new VisualElement();
            _rootElement.name = "environmental-status-hud";
            _rootElement.AddToClassList("hud-panel");
            _rootElement.AddToClassList("environmental-hud");
            
            CreateHeaderSection();
            CreateDataSection();
            CreateIndicatorSection();
            
            return _rootElement;
        }
        
        private void CreateHeaderSection()
        {
            var header = new VisualElement();
            header.AddToClassList("hud-header");
            
            var titleLabel = new Label("Environmental Status");
            titleLabel.AddToClassList("hud-title");
            header.Add(titleLabel);
            
            var minimizeButton = new Button();
            minimizeButton.text = "−";
            minimizeButton.AddToClassList("hud-minimize-btn");
            header.Add(minimizeButton);
            
            _rootElement.Add(header);
        }
        
        private void CreateDataSection()
        {
            var dataContainer = new VisualElement();
            dataContainer.AddToClassList("hud-data-container");
            
            // Temperature
            var tempSection = CreateDataRow("Temperature", "°C");
            _temperatureLabel = tempSection.Q<Label>("value-label");
            _temperatureBar = new ProgressBar();
            _temperatureBar.AddToClassList("temp-progress-bar");
            _temperatureBar.lowValue = 10f;
            _temperatureBar.highValue = 35f;
            tempSection.Add(_temperatureBar);
            dataContainer.Add(tempSection);
            
            // Humidity
            var humiditySection = CreateDataRow("Humidity", "%");
            _humidityLabel = humiditySection.Q<Label>("value-label");
            _humidityBar = new ProgressBar();
            _humidityBar.AddToClassList("humidity-progress-bar");
            _humidityBar.lowValue = 0f;
            _humidityBar.highValue = 100f;
            humiditySection.Add(_humidityBar);
            dataContainer.Add(humiditySection);
            
            // CO2
            var co2Section = CreateDataRow("CO2", "ppm");
            _co2Label = co2Section.Q<Label>("value-label");
            _co2Bar = new ProgressBar();
            _co2Bar.AddToClassList("co2-progress-bar");
            _co2Bar.lowValue = 300f;
            _co2Bar.highValue = 1500f;
            co2Section.Add(_co2Bar);
            dataContainer.Add(co2Section);
            
            // Light Intensity
            var lightSection = CreateDataRow("Light", "PPFD");
            _lightLabel = lightSection.Q<Label>("value-label");
            dataContainer.Add(lightSection);
            
            // Air Flow
            var airFlowSection = CreateDataRow("Air Flow", "m/s");
            _airFlowLabel = airFlowSection.Q<Label>("value-label");
            dataContainer.Add(airFlowSection);
            
            _rootElement.Add(dataContainer);
        }
        
        private VisualElement CreateDataRow(string labelText, string unit)
        {
            var row = new VisualElement();
            row.AddToClassList("hud-data-row");
            
            var label = new Label(labelText);
            label.AddToClassList("data-label");
            row.Add(label);
            
            var valueContainer = new VisualElement();
            valueContainer.AddToClassList("value-container");
            
            var valueLabel = new Label("--");
            valueLabel.name = "value-label";
            valueLabel.AddToClassList("value-label");
            valueContainer.Add(valueLabel);
            
            var unitLabel = new Label(unit);
            unitLabel.AddToClassList("unit-label");
            valueContainer.Add(unitLabel);
            
            row.Add(valueContainer);
            
            return row;
        }
        
        private void CreateIndicatorSection()
        {
            var indicatorContainer = new VisualElement();
            indicatorContainer.AddToClassList("status-indicator-container");
            
            _statusIndicator = new VisualElement();
            _statusIndicator.AddToClassList("status-indicator");
            _statusIndicator.AddToClassList("status-normal");
            
            var statusLabel = new Label("Normal");
            statusLabel.AddToClassList("status-label");
            _statusIndicator.Add(statusLabel);
            
            indicatorContainer.Add(_statusIndicator);
            _rootElement.Add(indicatorContainer);
        }
        
        public void UpdateData(object data)
        {
            if (data is EnvironmentalData envData)
            {
                UpdateData(envData);
            }
        }
        
        public void UpdateData(EnvironmentalData data)
        {
            _currentData = data;
            
            // Update temperature
            _temperatureLabel.text = $"{data.Temperature:F1}";
            _temperatureBar.value = data.Temperature;
            UpdateTemperatureStatus(data.Temperature);
            
            // Update humidity
            _humidityLabel.text = $"{data.Humidity:F0}";
            _humidityBar.value = data.Humidity;
            UpdateHumidityStatus(data.Humidity);
            
            // Update CO2
            _co2Label.text = $"{data.CO2Level:F0}";
            _co2Bar.value = data.CO2Level;
            UpdateCO2Status(data.CO2Level);
            
            // Update light intensity
            _lightLabel.text = $"{data.LightIntensity:F0}";
            
            // Update air flow
            _airFlowLabel.text = $"{data.AirVelocity:F1}";
            
            // Update overall status
            UpdateOverallStatus();
        }
        
        private void UpdateTemperatureStatus(float temperature)
        {
            _temperatureBar.RemoveFromClassList("status-warning");
            _temperatureBar.RemoveFromClassList("status-critical");
            
            if (temperature < 18f || temperature > 30f)
            {
                _temperatureBar.AddToClassList("status-critical");
            }
            else if (temperature < 20f || temperature > 28f)
            {
                _temperatureBar.AddToClassList("status-warning");
            }
        }
        
        private void UpdateHumidityStatus(float humidity)
        {
            _humidityBar.RemoveFromClassList("status-warning");
            _humidityBar.RemoveFromClassList("status-critical");
            
            if (humidity < 30f || humidity > 80f)
            {
                _humidityBar.AddToClassList("status-critical");
            }
            else if (humidity < 40f || humidity > 70f)
            {
                _humidityBar.AddToClassList("status-warning");
            }
        }
        
        private void UpdateCO2Status(float co2)
        {
            _co2Bar.RemoveFromClassList("status-warning");
            _co2Bar.RemoveFromClassList("status-critical");
            
            if (co2 < 350f || co2 > 1200f)
            {
                _co2Bar.AddToClassList("status-critical");
            }
            else if (co2 < 400f || co2 > 1000f)
            {
                _co2Bar.AddToClassList("status-warning");
            }
        }
        
        private void UpdateOverallStatus()
        {
            if (_currentData == null) return;
            
            _statusIndicator.RemoveFromClassList("status-normal");
            _statusIndicator.RemoveFromClassList("status-warning");
            _statusIndicator.RemoveFromClassList("status-critical");
            
            var statusLabel = _statusIndicator.Q<Label>();
            
            bool hasCritical = HasCriticalConditions();
            bool hasWarning = HasWarningConditions();
            
            if (hasCritical)
            {
                _statusIndicator.AddToClassList("status-critical");
                statusLabel.text = "Critical";
            }
            else if (hasWarning)
            {
                _statusIndicator.AddToClassList("status-warning");
                statusLabel.text = "Warning";
            }
            else
            {
                _statusIndicator.AddToClassList("status-normal");
                statusLabel.text = "Normal";
            }
        }
        
        private bool HasCriticalConditions()
        {
            return _currentData.Temperature < 18f || _currentData.Temperature > 30f ||
                   _currentData.Humidity < 30f || _currentData.Humidity > 80f ||
                   _currentData.CO2Level < 350f || _currentData.CO2Level > 1200f;
        }
        
        private bool HasWarningConditions()
        {
            return _currentData.Temperature < 20f || _currentData.Temperature > 28f ||
                   _currentData.Humidity < 40f || _currentData.Humidity > 70f ||
                   _currentData.CO2Level < 400f || _currentData.CO2Level > 1000f;
        }
        
        public void SetVisible(bool visible)
        {
            _rootElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        public void SetDraggable(bool draggable)
        {
            if (draggable)
            {
                _rootElement.AddToClassList("draggable");
            }
            else
            {
                _rootElement.RemoveFromClassList("draggable");
            }
        }
    }
}