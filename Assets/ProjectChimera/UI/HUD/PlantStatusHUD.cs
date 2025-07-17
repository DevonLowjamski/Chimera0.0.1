using UnityEngine;
using UnityEngine.UIElements;
using ProjectChimera.UI.Core;
using ProjectChimera.Data.Genetics;
using System;

namespace ProjectChimera.UI.Core
{
    /// <summary>
    /// Plant status HUD component showing real-time plant cultivation data.
    /// Displays plant counts, health averages, growth stages, and harvest readiness.
    /// </summary>
    public class PlantStatusHUD : IDataBinding
    {
        private VisualElement _rootElement;
        private Label _totalPlantsLabel;
        private Label _healthyPlantsLabel;
        private Label _vegPlantsLabel;
        private Label _flowerPlantsLabel;
        private Label _harvestReadyLabel;
        private Label _averageHealthLabel;
        private ProgressBar _healthBar;
        private VisualElement _stageDistributionContainer;
        private VisualElement _alertsContainer;
        
        private PlantStatusData _currentData;
        
        public VisualElement CreateHUDElement()
        {
            _rootElement = new VisualElement();
            _rootElement.name = "plant-status-hud";
            _rootElement.AddToClassList("hud-panel");
            _rootElement.AddToClassList("plant-hud");
            
            CreateHeaderSection();
            CreateSummarySection();
            CreateStageDistributionSection();
            CreateHealthSection();
            CreateAlertsSection();
            
            return _rootElement;
        }
        
        private void CreateHeaderSection()
        {
            var header = new VisualElement();
            header.AddToClassList("hud-header");
            
            var titleLabel = new Label("Plant Status");
            titleLabel.AddToClassList("hud-title");
            header.Add(titleLabel);
            
            var refreshButton = new Button();
            refreshButton.text = "↻";
            refreshButton.AddToClassList("hud-refresh-btn");
            header.Add(refreshButton);
            
            var minimizeButton = new Button();
            minimizeButton.text = "−";
            minimizeButton.AddToClassList("hud-minimize-btn");
            header.Add(minimizeButton);
            
            _rootElement.Add(header);
        }
        
        private void CreateSummarySection()
        {
            var summaryContainer = new VisualElement();
            summaryContainer.AddToClassList("plant-summary-container");
            
            // Total plants
            var totalRow = CreateSummaryRow("Total Plants", "0");
            _totalPlantsLabel = totalRow.Q<Label>("value-label");
            summaryContainer.Add(totalRow);
            
            // Healthy plants
            var healthyRow = CreateSummaryRow("Healthy", "0");
            _healthyPlantsLabel = healthyRow.Q<Label>("value-label");
            summaryContainer.Add(healthyRow);
            
            // Ready to harvest
            var harvestRow = CreateSummaryRow("Ready to Harvest", "0");
            _harvestReadyLabel = harvestRow.Q<Label>("value-label");
            harvestRow.AddToClassList("harvest-ready-row");
            summaryContainer.Add(harvestRow);
            
            _rootElement.Add(summaryContainer);
        }
        
        private void CreateStageDistributionSection()
        {
            var stageSection = new VisualElement();
            stageSection.AddToClassList("stage-distribution-section");
            
            var stageTitle = new Label("Growth Stages");
            stageTitle.AddToClassList("section-title");
            stageSection.Add(stageTitle);
            
            _stageDistributionContainer = new VisualElement();
            _stageDistributionContainer.AddToClassList("stage-distribution-container");
            
            // Vegetative plants
            var vegRow = CreateStageRow("Vegetative", "0", "stage-veg");
            _vegPlantsLabel = vegRow.Q<Label>("value-label");
            _stageDistributionContainer.Add(vegRow);
            
            // Flowering plants
            var flowerRow = CreateStageRow("Flowering", "0", "stage-flower");
            _flowerPlantsLabel = flowerRow.Q<Label>("value-label");
            _stageDistributionContainer.Add(flowerRow);
            
            stageSection.Add(_stageDistributionContainer);
            _rootElement.Add(stageSection);
        }
        
        private void CreateHealthSection()
        {
            var healthSection = new VisualElement();
            healthSection.AddToClassList("health-section");
            
            var healthTitle = new Label("Average Health");
            healthTitle.AddToClassList("section-title");
            healthSection.Add(healthTitle);
            
            var healthContainer = new VisualElement();
            healthContainer.AddToClassList("health-container");
            
            _averageHealthLabel = new Label("--");
            _averageHealthLabel.AddToClassList("health-value-label");
            healthContainer.Add(_averageHealthLabel);
            
            var percentLabel = new Label("%");
            percentLabel.AddToClassList("health-unit-label");
            healthContainer.Add(percentLabel);
            
            _healthBar = new ProgressBar();
            _healthBar.AddToClassList("health-progress-bar");
            _healthBar.lowValue = 0f;
            _healthBar.highValue = 100f;
            healthContainer.Add(_healthBar);
            
            healthSection.Add(healthContainer);
            _rootElement.Add(healthSection);
        }
        
        private void CreateAlertsSection()
        {
            var alertsSection = new VisualElement();
            alertsSection.AddToClassList("alerts-section");
            
            var alertsTitle = new Label("Plant Alerts");
            alertsTitle.AddToClassList("section-title");
            alertsSection.Add(alertsTitle);
            
            _alertsContainer = new VisualElement();
            _alertsContainer.AddToClassList("alerts-container");
            alertsSection.Add(_alertsContainer);
            
            _rootElement.Add(alertsSection);
        }
        
        private VisualElement CreateSummaryRow(string labelText, string initialValue)
        {
            var row = new VisualElement();
            row.AddToClassList("summary-row");
            
            var label = new Label(labelText);
            label.AddToClassList("summary-label");
            row.Add(label);
            
            var valueLabel = new Label(initialValue);
            valueLabel.name = "value-label";
            valueLabel.AddToClassList("summary-value");
            row.Add(valueLabel);
            
            return row;
        }
        
        private VisualElement CreateStageRow(string stageName, string initialValue, string cssClass)
        {
            var row = new VisualElement();
            row.AddToClassList("stage-row");
            row.AddToClassList(cssClass);
            
            var indicator = new VisualElement();
            indicator.AddToClassList("stage-indicator");
            row.Add(indicator);
            
            var label = new Label(stageName);
            label.AddToClassList("stage-label");
            row.Add(label);
            
            var valueLabel = new Label(initialValue);
            valueLabel.name = "value-label";
            valueLabel.AddToClassList("stage-value");
            row.Add(valueLabel);
            
            return row;
        }
        
        public void UpdateData(object data)
        {
            if (data is PlantStatusData plantData)
            {
                UpdateData(plantData);
            }
        }
        
        public void UpdateData(PlantStatusData data)
        {
            _currentData = data;
            
            // Update summary
            _totalPlantsLabel.text = data.TotalPlants.ToString();
            _healthyPlantsLabel.text = data.HealthyPlants.ToString();
            _harvestReadyLabel.text = data.ReadyToHarvest.ToString();
            
            // Update stage distribution
            _vegPlantsLabel.text = data.PlantsInVeg.ToString();
            _flowerPlantsLabel.text = data.PlantsInFlower.ToString();
            
            // Update health
            _averageHealthLabel.text = $"{data.AverageHealth:F0}";
            _healthBar.value = data.AverageHealth;
            UpdateHealthStatus(data.AverageHealth);
            
            // Update alerts
            UpdateAlerts(data);
            
            // Update visual indicators
            UpdateVisualIndicators(data);
        }
        
        private void UpdateHealthStatus(float averageHealth)
        {
            _healthBar.RemoveFromClassList("health-excellent");
            _healthBar.RemoveFromClassList("health-good");
            _healthBar.RemoveFromClassList("health-warning");
            _healthBar.RemoveFromClassList("health-critical");
            
            if (averageHealth >= 90f)
            {
                _healthBar.AddToClassList("health-excellent");
            }
            else if (averageHealth >= 70f)
            {
                _healthBar.AddToClassList("health-good");
            }
            else if (averageHealth >= 50f)
            {
                _healthBar.AddToClassList("health-warning");
            }
            else
            {
                _healthBar.AddToClassList("health-critical");
            }
        }
        
        private void UpdateAlerts(PlantStatusData data)
        {
            _alertsContainer.Clear();
            
            var alerts = GenerateAlerts(data);
            
            if (alerts.Count == 0)
            {
                var noAlertsLabel = new Label("No active alerts");
                noAlertsLabel.AddToClassList("no-alerts-label");
                _alertsContainer.Add(noAlertsLabel);
            }
            else
            {
                foreach (var alert in alerts)
                {
                    var alertElement = CreateAlertElement(alert);
                    _alertsContainer.Add(alertElement);
                }
            }
        }
        
        private System.Collections.Generic.List<PlantAlert> GenerateAlerts(PlantStatusData data)
        {
            var alerts = new System.Collections.Generic.List<PlantAlert>();
            
            // Low health alert
            if (data.AverageHealth < 50f)
            {
                alerts.Add(new PlantAlert
                {
                    AlertType = PlantAlertType.LowHealth,
                    Message = "Average plant health is critically low",
                    Severity = AlertSeverity.Critical
                });
            }
            else if (data.AverageHealth < 70f)
            {
                alerts.Add(new PlantAlert
                {
                    AlertType = PlantAlertType.LowHealth,
                    Message = "Plant health needs attention",
                    Severity = AlertSeverity.Warning
                });
            }
            
            // Harvest ready alert
            if (data.ReadyToHarvest > 0)
            {
                alerts.Add(new PlantAlert
                {
                    AlertType = PlantAlertType.HarvestReady,
                    Message = $"{data.ReadyToHarvest} plant(s) ready for harvest",
                    Severity = AlertSeverity.Info
                });
            }
            
            // No plants alert
            if (data.TotalPlants == 0)
            {
                alerts.Add(new PlantAlert
                {
                    AlertType = PlantAlertType.NoPlants,
                    Message = "No plants in cultivation",
                    Severity = AlertSeverity.Warning
                });
            }
            
            return alerts;
        }
        
        private VisualElement CreateAlertElement(PlantAlert alert)
        {
            var alertElement = new VisualElement();
            alertElement.AddToClassList("plant-alert");
            alertElement.AddToClassList($"alert-{alert.Severity.ToString().ToLower()}");
            
            var iconElement = new VisualElement();
            iconElement.AddToClassList("alert-icon");
            alertElement.Add(iconElement);
            
            var messageLabel = new Label(alert.Message);
            messageLabel.AddToClassList("alert-message");
            alertElement.Add(messageLabel);
            
            return alertElement;
        }
        
        private void UpdateVisualIndicators(PlantStatusData data)
        {
            // Update harvest ready indicator
            var harvestRow = _harvestReadyLabel.parent;
            harvestRow.RemoveFromClassList("has-harvest");
            
            if (data.ReadyToHarvest > 0)
            {
                harvestRow.AddToClassList("has-harvest");
            }
            
            // Update stage distribution visuals
            UpdateStagePercentages(data);
        }
        
        private void UpdateStagePercentages(PlantStatusData data)
        {
            if (data.TotalPlants == 0) return;
            
            float vegPercentage = (float)data.PlantsInVeg / data.TotalPlants;
            float flowerPercentage = (float)data.PlantsInFlower / data.TotalPlants;
            
            // Update progress bars for visual representation
            var vegRow = _vegPlantsLabel.parent;
            var flowerRow = _flowerPlantsLabel.parent;
            
            vegRow.style.opacity = Mathf.Max(0.3f, vegPercentage);
            flowerRow.style.opacity = Mathf.Max(0.3f, flowerPercentage);
        }
        
        public void SetVisible(bool visible)
        {
            _rootElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        public void SetCompactMode(bool compact)
        {
            if (compact)
            {
                _rootElement.AddToClassList("compact-mode");
            }
            else
            {
                _rootElement.RemoveFromClassList("compact-mode");
            }
        }
    }
    
    [System.Serializable]
    public class PlantAlert
    {
        public PlantAlertType AlertType;
        public string Message;
        public AlertSeverity Severity;
        public System.DateTime Timestamp;
    }
    
    public enum PlantAlertType
    {
        LowHealth,
        HarvestReady,
        NoPlants,
        WateringNeeded,
        NutrientDeficiency,
        GrowthStalled
    }
    
    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical
    }
}