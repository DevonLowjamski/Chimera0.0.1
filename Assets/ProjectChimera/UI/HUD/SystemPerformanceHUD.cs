using UnityEngine;
using UnityEngine.UIElements;
using ProjectChimera.UI.Core;
using System;

namespace ProjectChimera.UI.Core
{
    /// <summary>
    /// System performance HUD component showing real-time system metrics.
    /// Displays power consumption, efficiency, system load, and active alerts.
    /// </summary>
    public class SystemPerformanceHUD : IDataBinding
    {
        private VisualElement _rootElement;
        private Label _powerConsumptionLabel;
        private Label _efficiencyLabel;
        private Label _systemLoadLabel;
        private Label _alertsLabel;

        public VisualElement CreateHUDElement()
        {
            _rootElement = new VisualElement();
            _rootElement.name = "system-performance-hud";
            _rootElement.AddToClassList("hud-element");
            _rootElement.AddToClassList("performance-hud");

            var title = new Label("System Performance");
            title.AddToClassList("hud-title");
            _rootElement.Add(title);

            var content = new VisualElement();
            content.AddToClassList("hud-content");

            _powerConsumptionLabel = new Label("Power: 0 kW");
            _powerConsumptionLabel.AddToClassList("hud-value");
            content.Add(_powerConsumptionLabel);

            _efficiencyLabel = new Label("Efficiency: 0%");
            _efficiencyLabel.AddToClassList("hud-value");
            content.Add(_efficiencyLabel);

            _systemLoadLabel = new Label("Load: 0%");
            _systemLoadLabel.AddToClassList("hud-value");
            content.Add(_systemLoadLabel);

            _alertsLabel = new Label("Alerts: 0");
            _alertsLabel.AddToClassList("hud-value");
            content.Add(_alertsLabel);

            _rootElement.Add(content);
            return _rootElement;
        }

        public void UpdateData(object data)
        {
            if (data is SystemPerformanceData perfData)
            {
                _powerConsumptionLabel.text = $"Power: {perfData.TotalPowerConsumption:F1} kW";
                _efficiencyLabel.text = $"Efficiency: {perfData.EnergyEfficiency:F1}%";
                _systemLoadLabel.text = $"Load: {perfData.SystemLoad:F1}%";
                _alertsLabel.text = $"Alerts: {perfData.ActiveAlerts}";
            }
        }
    }
}