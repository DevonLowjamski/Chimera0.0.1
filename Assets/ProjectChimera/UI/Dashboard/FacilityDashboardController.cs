using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
// using ProjectChimera.Systems.Automation;
// using ProjectChimera.Systems.Environment;
// using ProjectChimera.Systems.Economy;
using ProjectChimera.Data.UI;
using ProjectChimera.Data.Automation;
using TrendDirection = ProjectChimera.Data.UI.TrendDirection;
using ProjectChimera.UI.Core;

namespace ProjectChimera.UI.Dashboard
{
    /// <summary>
    /// Main Facility Dashboard Controller for Project Chimera.
    /// Provides a comprehensive overview of all facility systems including environmental controls,
    /// economic performance, automation status, and AI recommendations.
    /// Built using Unity UI Toolkit for modern, responsive interface design.
    /// </summary>
    public class FacilityDashboardController : ChimeraMonoBehaviour
    {
        public bool IsInitialized { get; private set; }
        public bool IsVisible { get; private set; }
        
        [Header("Dashboard Configuration")]
        [SerializeField] private UIDocument _dashboardDocument;
        [SerializeField] private DashboardSettings _dashboardSettings;
        [SerializeField] private bool _autoRefresh = true;
        [SerializeField] private float _refreshInterval = 5f;
        
        [Header("Theme Configuration")]
        [SerializeField] private UIThemeSO _currentTheme;
        [SerializeField] private bool _enableDarkMode = false;
        [SerializeField] private bool _enableAnimations = true;
        
        // System references
        // private AutomationManager _automationManager;
        // private HVACManager _hvacManager;
        // private LightingManager _lightingManager;
        // private InvestmentManager _investmentManager;
        // private TradingManager _tradingManager;
        // private MarketManager _marketManager;
        // private AIAdvisorManager _aiAdvisorManager;
        
        // UI Elements
        private VisualElement _rootElement;
        private VisualElement _headerSection;
        private VisualElement _systemStatusPanel;
        private VisualElement _environmentalPanel;
        private VisualElement _economicPanel;
        private VisualElement _automationPanel;
        private VisualElement _aiAdvisorPanel;
        private VisualElement _alertsPanel;
        
        // Status indicators
        private Label _facilityNameLabel;
        private Label _systemTimeLabel;
        private Label _overallStatusLabel;
        private ProgressBar _facilityEfficiencyBar;
        
        // Environmental displays
        private Label _temperatureDisplay;
        private Label _humidityDisplay;
        private Label _co2Display;
        private Label _lightingStatusDisplay;
        private ProgressBar _hvacEfficiencyBar;
        private ProgressBar _lightingEfficiencyBar;
        
        // Economic displays
        private Label _netWorthDisplay;
        private Label _dailyRevenueDisplay;
        private Label _profitMarginDisplay;
        private Label _cashFlowDisplay;
        private VisualElement _economicTrendIndicator;
        
        // Automation displays
        private Label _activeSensorsDisplay;
        private Label _activeRulesDisplay;
        private Label _activeAlertsDisplay;
        private Label _systemUptimeDisplay;
        
        // AI Advisor displays
        private VisualElement _recommendationsList;
        private Label _aiStatusDisplay;
        private Label _optimizationOpportunitiesDisplay;
        private ProgressBar _systemEfficiencyScore;
        
        // Data caching
        private DashboardData _cachedData = new DashboardData();
        private float _lastUpdateTime;
        
        // Events
        public System.Action<string> OnPanelSelected;
        public System.Action<DashboardAlert> OnAlertClicked;
        public System.Action<string> OnQuickActionTriggered;
        
        // Manager references (commented out to avoid circular dependencies)
        // private HVACManager _hvacManager;
        // private LightingManager _lightingManager;
        // private AutomationManager _automationManager;
        // private AIAdvisorManager _aiAdvisorManager;
        
        // Placeholder variables to prevent compilation errors
        private object _hvacManager = null;
        private object _lightingManager = null;
        private List<object> savedRules = new List<object>();
        
        protected override void Awake()
        {
            base.Awake();
            IsInitialized = true;
        }
        
        private void Start()
        {
            InitializeDashboard();
            InitializeSystemReferences();
            SetupUIElements();
            ApplyTheme();
            
            if (_autoRefresh)
            {
                InvokeRepeating(nameof(RefreshDashboard), 1f, _refreshInterval);
            }
        }
        
        private void InitializeDashboard()
        {
            if (_dashboardDocument == null)
            {
                Debug.LogError("Dashboard UI Document not assigned!");
                return;
            }
            
            _rootElement = _dashboardDocument.rootVisualElement;
            _lastUpdateTime = Time.time;
            
            Debug.Log("Facility Dashboard initialized successfully");
        }
        
        private void InitializeSystemReferences()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogWarning("GameManager not found - dashboard will show placeholder data");
                return;
            }
            
            // _automationManager = gameManager.GetManager<AutomationManager>();
            // _hvacManager = gameManager.GetManager<HVACManager>();
            // _lightingManager = gameManager.GetManager<LightingManager>();
            // _investmentManager = gameManager.GetManager<InvestmentManager>();
            // _tradingManager = gameManager.GetManager<TradingManager>();
            // _marketManager = gameManager.GetManager<MarketManager>();
            // _aiAdvisorManager = gameManager.GetManager<AIAdvisorManager>();
            
            Debug.Log($"Dashboard connected to {CountAvailableSystems()} available systems");
        }
        
        private void SetupUIElements()
        {
            // Header section
            _headerSection = _rootElement.Q<VisualElement>("header-section");
            _facilityNameLabel = _rootElement.Q<Label>("facility-name");
            _systemTimeLabel = _rootElement.Q<Label>("system-time");
            _overallStatusLabel = _rootElement.Q<Label>("overall-status");
            _facilityEfficiencyBar = _rootElement.Q<ProgressBar>("facility-efficiency");
            
            // System status panel
            _systemStatusPanel = _rootElement.Q<VisualElement>("system-status-panel");
            
            // Environmental panel
            _environmentalPanel = _rootElement.Q<VisualElement>("environmental-panel");
            _temperatureDisplay = _rootElement.Q<Label>("temperature-display");
            _humidityDisplay = _rootElement.Q<Label>("humidity-display");
            _co2Display = _rootElement.Q<Label>("co2-display");
            _lightingStatusDisplay = _rootElement.Q<Label>("lighting-status");
            _hvacEfficiencyBar = _rootElement.Q<ProgressBar>("hvac-efficiency");
            _lightingEfficiencyBar = _rootElement.Q<ProgressBar>("lighting-efficiency");
            
            // Economic panel
            _economicPanel = _rootElement.Q<VisualElement>("economic-panel");
            _netWorthDisplay = _rootElement.Q<Label>("net-worth-display");
            _dailyRevenueDisplay = _rootElement.Q<Label>("daily-revenue-display");
            _profitMarginDisplay = _rootElement.Q<Label>("profit-margin-display");
            _cashFlowDisplay = _rootElement.Q<Label>("cash-flow-display");
            _economicTrendIndicator = _rootElement.Q<VisualElement>("economic-trend");
            
            // Automation panel
            _automationPanel = _rootElement.Q<VisualElement>("automation-panel");
            _activeSensorsDisplay = _rootElement.Q<Label>("active-sensors");
            _activeRulesDisplay = _rootElement.Q<Label>("active-rules");
            _activeAlertsDisplay = _rootElement.Q<Label>("active-alerts");
            _systemUptimeDisplay = _rootElement.Q<Label>("system-uptime");
            
            // AI Advisor panel
            _aiAdvisorPanel = _rootElement.Q<VisualElement>("ai-advisor-panel");
            _recommendationsList = _rootElement.Q<VisualElement>("recommendations-list");
            _aiStatusDisplay = _rootElement.Q<Label>("ai-status");
            _optimizationOpportunitiesDisplay = _rootElement.Q<Label>("optimization-opportunities");
            _systemEfficiencyScore = _rootElement.Q<ProgressBar>("system-efficiency-score");
            
            // Alerts panel
            _alertsPanel = _rootElement.Q<VisualElement>("alerts-panel");
            
            SetupEventHandlers();
        }
        
        private void SetupEventHandlers()
        {
            // Panel navigation buttons
            _rootElement.Q<Button>("environmental-btn")?.RegisterCallback<ClickEvent>(evt => NavigateToPanel("environmental"));
            _rootElement.Q<Button>("economic-btn")?.RegisterCallback<ClickEvent>(evt => NavigateToPanel("economic"));
            _rootElement.Q<Button>("automation-btn")?.RegisterCallback<ClickEvent>(evt => NavigateToPanel("automation"));
            _rootElement.Q<Button>("ai-advisor-btn")?.RegisterCallback<ClickEvent>(evt => NavigateToPanel("ai-advisor"));
            
            // Quick action buttons
            _rootElement.Q<Button>("emergency-stop-btn")?.RegisterCallback<ClickEvent>(evt => TriggerQuickAction("emergency-stop"));
            _rootElement.Q<Button>("optimize-all-btn")?.RegisterCallback<ClickEvent>(evt => TriggerQuickAction("optimize-all"));
            _rootElement.Q<Button>("refresh-data-btn")?.RegisterCallback<ClickEvent>(evt => RefreshDashboard());
            
            // Settings and preferences
            _rootElement.Q<Button>("theme-toggle-btn")?.RegisterCallback<ClickEvent>(evt => ToggleTheme());
            _rootElement.Q<Button>("settings-btn")?.RegisterCallback<ClickEvent>(evt => OpenSettings());
        }
        
        [ContextMenu("Refresh Dashboard")]
        public void RefreshDashboard()
        {
            _cachedData = CollectDashboardData();
            UpdateAllDisplays();
            _lastUpdateTime = Time.time;
        }
        
        private DashboardData CollectDashboardData()
        {
            var data = new DashboardData
            {
                Timestamp = DateTime.Now,
                FacilityName = _dashboardSettings?.FacilityName ?? "Project Chimera Facility",
                OverallStatus = CalculateOverallStatus()
            };
            
            // Environmental data
            data.Environmental = CollectEnvironmentalData();
            
            // Economic data
            data.Economic = CollectEconomicData();
            
            // Automation data
            data.Automation = CollectAutomationData();
            
            // AI Advisor data
            data.AIAdvisor = CollectAIAdvisorData();
            
            // System performance
            data.Performance = CollectPerformanceData();
            
            return data;
        }
        
        private EnvironmentalDashboardData CollectEnvironmentalData()
        {
            var data = new EnvironmentalDashboardData();
            
            // if (_automationManager != null)
            // {
                // Get zone sensor readings for primary cultivation areas
                var zones = new[] { "VegetativeRoom", "FloweringRoom", "DryingRoom" };
                var allReadings = new List<ProjectChimera.Data.Automation.SensorReading>();
                
                foreach (var zone in zones)
                {
                    // var zoneReadings = _automationManager.GetZoneSensorReadings(zone);
                    var zoneReadings = new List<ProjectChimera.Data.Automation.SensorReading>(); // Placeholder
                    allReadings.AddRange(zoneReadings);
                }
                
                // Average environmental conditions
                var tempReadings = allReadings.Where(r => r.SensorId.Contains("temp")).ToList();
                var humidityReadings = allReadings.Where(r => r.SensorId.Contains("humidity")).ToList();
                var co2Readings = allReadings.Where(r => r.SensorId.Contains("co2")).ToList();
                
                data.AverageTemperature = tempReadings.Any() ? tempReadings.Average(r => r.Value) : 22f;
                data.AverageHumidity = humidityReadings.Any() ? humidityReadings.Average(r => r.Value) : 55f;
                data.AverageCO2 = co2Readings.Any() ? co2Readings.Average(r => r.Value) : 1000f;
                
                // data.ActiveSensors = _automationManager.ActiveSensors;
                // data.ActiveAlerts = _automationManager.ActiveAlerts;
            // }
            // else
            // {
                // Placeholder data when systems not available
                data.AverageTemperature = 24.5f;
                data.AverageHumidity = 58.2f;
                data.AverageCO2 = 1150f;
                data.ActiveSensors = 0;
                data.ActiveAlerts = 0;
            // }
            
            // HVAC efficiency (would be calculated from actual system data)
            data.HVACEfficiency = _hvacManager != null ? 89.3f : 0f;
            
            // Lighting efficiency (would be calculated from actual system data)
            data.LightingEfficiency = _lightingManager != null ? 92.7f : 0f;
            data.LightingStatus = _lightingManager != null ? "Operational" : "Unavailable";
            
            return data;
        }
        
        private EconomicDashboardData CollectEconomicData()
        {
            var data = new EconomicDashboardData();
            
            // if (_investmentManager != null)
            // {
                // var dashboard = _investmentManager.GetFinancialDashboard();
                var dashboard = new { NetWorth = 125000f, MonthlyIncome = 10000f, MonthlyExpenses = 8500f, FinancialHealthScore = 0.78f }; // Placeholder
                data.NetWorth = dashboard.NetWorth;
                data.CashFlow = dashboard.MonthlyIncome - dashboard.MonthlyExpenses;
                data.ProfitMargin = dashboard.MonthlyIncome > 0 ? ((dashboard.MonthlyIncome - dashboard.MonthlyExpenses) / dashboard.MonthlyIncome) * 100f : 0f;
                data.FinancialHealthScore = dashboard.FinancialHealthScore;
            // }
            // else
            // {
                data.NetWorth = 125000f;
                data.CashFlow = 8500f;
                data.ProfitMargin = 18.5f;
                data.FinancialHealthScore = 0.78f;
            // }
            
            // if (_tradingManager != null)
            // {
                // Would get actual trading data
                data.DailyRevenue = 2850f;
                data.WeeklyRevenue = 18200f;
                data.MonthlyRevenue = 76500f;
            // }
            // else
            // {
                data.DailyRevenue = 2850f;
                data.WeeklyRevenue = 18200f;
                data.MonthlyRevenue = 76500f;
            // }
            
            data.TrendDirection = data.CashFlow > 0 ? TrendDirection.Up : TrendDirection.Down;
            
            return data;
        }
        
        private AutomationDashboardData CollectAutomationData()
        {
            var data = new AutomationDashboardData();
            
            // if (_automationManager != null)
            // {
                // data.ActiveSensors = _automationManager.ActiveSensors;
                // data.ConnectedDevices = _automationManager.ConnectedDevices;
                // data.ActiveRules = _automationManager.ActiveAutomationRules;
                // data.ActiveAlerts = _automationManager.ActiveAlerts;
                
                // Generate uptime report
                // var report = _automationManager.GenerateAutomationReport(TimeSpan.FromDays(1));
                var report = new { SystemUptime = 98.5f, EnergyOptimizationSavings = 15.2f }; // Placeholder
                data.SystemUptime = report.SystemUptime;
                data.EnergyOptimization = report.EnergyOptimizationSavings;
            // }
            // else
            // {
                data.ActiveSensors = 0;
                data.ConnectedDevices = 0;
                data.ActiveRules = 0;
                data.ActiveAlerts = 0;
                data.SystemUptime = 0f;
                data.EnergyOptimization = 0f;
            // }
            
            return data;
        }
        
        private AIAdvisorDashboardData CollectAIAdvisorData()
        {
            var data = new AIAdvisorDashboardData();
            
            // if (_aiAdvisorManager != null)
            // {
                // data.ActiveRecommendations = _aiAdvisorManager.ActiveRecommendations;
                // data.CriticalInsights = _aiAdvisorManager.CriticalInsights;
                // data.OptimizationOpportunities = _aiAdvisorManager.OptimizationOpportunities;
                // data.SystemEfficiencyScore = _aiAdvisorManager.SystemEfficiencyScore;
                
                // var topRecommendations = _aiAdvisorManager.GetActiveRecommendations().Take(3).ToList();
                var topRecommendations = new List<object>(); // Placeholder
                data.TopRecommendations = topRecommendations.Select(r => "Sample Recommendation").ToList();
                
                data.AIStatus = "Active";
            // }
            // else
            // {
                data.ActiveRecommendations = 0;
                data.CriticalInsights = 0;
                data.OptimizationOpportunities = 0;
                data.SystemEfficiencyScore = 0.75f;
                data.TopRecommendations = new List<string> { "AI Advisor not available" };
                data.AIStatus = "Unavailable";
            // }
            
            return data;
        }
        
        private PerformanceDashboardData CollectPerformanceData()
        {
            return new PerformanceDashboardData
            {
                FrameRate = 1f / Time.deltaTime,
                MemoryUsage = GC.GetTotalMemory(false) / (1024f * 1024f),
                ActiveSystems = CountAvailableSystems(),
                LastUpdateTime = _lastUpdateTime
            };
        }
        
        private void UpdateAllDisplays()
        {
            UpdateHeaderSection();
            UpdateEnvironmentalPanel();
            UpdateEconomicPanel();
            UpdateAutomationPanel();
            UpdateAIAdvisorPanel();
            UpdateAlertsPanel();
        }
        
        private void UpdateHeaderSection()
        {
            if (_facilityNameLabel != null)
                _facilityNameLabel.text = _cachedData.FacilityName;
            
            if (_systemTimeLabel != null)
                _systemTimeLabel.text = DateTime.Now.ToString("HH:mm:ss");
            
            if (_overallStatusLabel != null)
            {
                _overallStatusLabel.text = _cachedData.OverallStatus.ToString();
                _overallStatusLabel.RemoveFromClassList("status-optimal");
                _overallStatusLabel.RemoveFromClassList("status-warning");
                _overallStatusLabel.RemoveFromClassList("status-critical");
                _overallStatusLabel.AddToClassList($"status-{_cachedData.OverallStatus.ToString().ToLower()}");
            }
            
            if (_facilityEfficiencyBar != null)
            {
                float efficiency = CalculateFacilityEfficiency();
                _facilityEfficiencyBar.value = efficiency;
                _facilityEfficiencyBar.title = $"Facility Efficiency: {efficiency:P1}";
            }
        }
        
        private void UpdateEnvironmentalPanel()
        {
            var env = _cachedData.Environmental;
            
            if (_temperatureDisplay != null)
                _temperatureDisplay.text = $"{env.AverageTemperature:F1}°C";
            
            if (_humidityDisplay != null)
                _humidityDisplay.text = $"{env.AverageHumidity:F1}%";
            
            if (_co2Display != null)
                _co2Display.text = $"{env.AverageCO2:F0} ppm";
            
            if (_lightingStatusDisplay != null)
                _lightingStatusDisplay.text = env.LightingStatus;
            
            if (_hvacEfficiencyBar != null)
            {
                _hvacEfficiencyBar.value = env.HVACEfficiency / 100f;
                _hvacEfficiencyBar.title = $"HVAC: {env.HVACEfficiency:F1}%";
            }
            
            if (_lightingEfficiencyBar != null)
            {
                _lightingEfficiencyBar.value = env.LightingEfficiency / 100f;
                _lightingEfficiencyBar.title = $"Lighting: {env.LightingEfficiency:F1}%";
            }
        }
        
        private void UpdateEconomicPanel()
        {
            var econ = _cachedData.Economic;
            
            if (_netWorthDisplay != null)
                _netWorthDisplay.text = $"${econ.NetWorth:N0}";
            
            if (_dailyRevenueDisplay != null)
                _dailyRevenueDisplay.text = $"${econ.DailyRevenue:N0}";
            
            if (_profitMarginDisplay != null)
            {
                _profitMarginDisplay.text = $"{econ.ProfitMargin:F1}%";
                UpdateTrendIndicator(_profitMarginDisplay, econ.ProfitMargin, 15f); // 15% target
            }
            
            if (_cashFlowDisplay != null)
            {
                _cashFlowDisplay.text = $"${econ.CashFlow:N0}";
                UpdateTrendIndicator(_cashFlowDisplay, econ.CashFlow, 0f); // Positive cash flow target
            }
            
            if (_economicTrendIndicator != null)
            {
                _economicTrendIndicator.RemoveFromClassList("trend-up");
                _economicTrendIndicator.RemoveFromClassList("trend-down");
                _economicTrendIndicator.RemoveFromClassList("trend-stable");
                _economicTrendIndicator.AddToClassList($"trend-{econ.TrendDirection.ToString().ToLower()}");
            }
        }
        
        private void UpdateAutomationPanel()
        {
            var auto = _cachedData.Automation;
            
            if (_activeSensorsDisplay != null)
                _activeSensorsDisplay.text = auto.ActiveSensors.ToString();
            
            if (_activeRulesDisplay != null)
                _activeRulesDisplay.text = auto.ActiveRules.ToString();
            
            if (_activeAlertsDisplay != null)
            {
                _activeAlertsDisplay.text = auto.ActiveAlerts.ToString();
                UpdateAlertIndicator(_activeAlertsDisplay, auto.ActiveAlerts);
            }
            
            if (_systemUptimeDisplay != null)
                _systemUptimeDisplay.text = $"{auto.SystemUptime:F1}%";
        }
        
        private void UpdateAIAdvisorPanel()
        {
            var ai = _cachedData.AIAdvisor;
            
            if (_aiStatusDisplay != null)
            {
                _aiStatusDisplay.text = ai.AIStatus;
                _aiStatusDisplay.RemoveFromClassList("ai-active");
                _aiStatusDisplay.RemoveFromClassList("ai-unavailable");
                _aiStatusDisplay.AddToClassList($"ai-{ai.AIStatus.ToLower()}");
            }
            
            if (_optimizationOpportunitiesDisplay != null)
                _optimizationOpportunitiesDisplay.text = ai.OptimizationOpportunities.ToString();
            
            if (_systemEfficiencyScore != null)
            {
                _systemEfficiencyScore.value = ai.SystemEfficiencyScore;
                _systemEfficiencyScore.title = $"System Efficiency: {ai.SystemEfficiencyScore:P1}";
            }
            
            UpdateRecommendationsList(ai.TopRecommendations);
        }
        
        private void UpdateAlertsPanel()
        {
            if (_alertsPanel == null) return;
            
            // Clear existing alerts
            _alertsPanel.Clear();
            
            // Add current alerts
            var totalAlerts = _cachedData.Automation.ActiveAlerts + _cachedData.AIAdvisor.CriticalInsights;
            
            if (totalAlerts == 0)
            {
                var noAlertsLabel = new Label("No active alerts");
                noAlertsLabel.AddToClassList("no-alerts-message");
                _alertsPanel.Add(noAlertsLabel);
            }
            // else
            // {
                // Add automation alerts
                for (int i = 0; i < Math.Min(_cachedData.Automation.ActiveAlerts, 3); i++)
                {
                    var alertElement = CreateAlertElement($"Automation Alert {i + 1}", "Environmental monitoring", ProjectChimera.Data.UI.AlertSeverity.Warning);
                    _alertsPanel.Add(alertElement);
                }
                
                // Add AI insights
                for (int i = 0; i < Math.Min(_cachedData.AIAdvisor.CriticalInsights, 2); i++)
                {
                    var insightElement = CreateAlertElement($"Critical Insight {i + 1}", "Performance optimization", ProjectChimera.Data.UI.AlertSeverity.Info);
                    _alertsPanel.Add(insightElement);
                }
            // }
        }
        
        private VisualElement CreateAlertElement(string title, string description, ProjectChimera.Data.UI.AlertSeverity severity)
        {
            var alertContainer = new VisualElement();
            alertContainer.AddToClassList("alert-item");
            alertContainer.AddToClassList($"alert-{severity.ToString().ToLower()}");
            
            var titleLabel = new Label(title);
            titleLabel.AddToClassList("alert-title");
            
            var descLabel = new Label(description);
            descLabel.AddToClassList("alert-description");
            
            var timeLabel = new Label(DateTime.Now.ToString("HH:mm"));
            timeLabel.AddToClassList("alert-time");
            
            alertContainer.Add(titleLabel);
            alertContainer.Add(descLabel);
            alertContainer.Add(timeLabel);
            
            alertContainer.RegisterCallback<ClickEvent>(evt => 
            {
                OnAlertClicked?.Invoke(new DashboardAlert { Title = title, Description = description, Severity = severity });
            });
            
            return alertContainer;
        }
        
        private void UpdateRecommendationsList(List<string> recommendations)
        {
            if (_recommendationsList == null) return;
            
            _recommendationsList.Clear();
            
            if (recommendations == null || !recommendations.Any())
            {
                var noRecsLabel = new Label("No active recommendations");
                noRecsLabel.AddToClassList("no-recommendations-message");
                _recommendationsList.Add(noRecsLabel);
                return;
            }
            
            foreach (var recommendation in recommendations.Take(3))
            {
                var recElement = new VisualElement();
                recElement.AddToClassList("recommendation-item");
                
                var recLabel = new Label(recommendation);
                recLabel.AddToClassList("recommendation-text");
                
                var viewButton = new Button(() => NavigateToPanel("ai-advisor")) { text = "View" };
                viewButton.AddToClassList("recommendation-button");
                
                recElement.Add(recLabel);
                recElement.Add(viewButton);
                _recommendationsList.Add(recElement);
            }
        }
        
        private void UpdateTrendIndicator(VisualElement element, float value, float target)
        {
            element.RemoveFromClassList("trend-positive");
            element.RemoveFromClassList("trend-negative");
            element.RemoveFromClassList("trend-neutral");
            
            if (value > target)
                element.AddToClassList("trend-positive");
            else if (value < target * 0.8f)
                element.AddToClassList("trend-negative");
            else
                element.AddToClassList("trend-neutral");
        }
        
        private void UpdateAlertIndicator(VisualElement element, int alertCount)
        {
            element.RemoveFromClassList("alert-none");
            element.RemoveFromClassList("alert-low");
            element.RemoveFromClassList("alert-medium");
            element.RemoveFromClassList("alert-high");
            
            if (alertCount == 0)
                element.AddToClassList("alert-none");
            else if (alertCount <= 2)
                element.AddToClassList("alert-low");
            else if (alertCount <= 5)
                element.AddToClassList("alert-medium");
            else
                element.AddToClassList("alert-high");
        }
        
        private SystemStatus CalculateOverallStatus()
        {
            int criticalIssues = 0;
            int warnings = 0;
            
            // Check environmental systems
            if (_cachedData.Environmental.ActiveAlerts > 3) criticalIssues++;
            if (_cachedData.Environmental.HVACEfficiency < 80f) warnings++;
            if (_cachedData.Environmental.LightingEfficiency < 85f) warnings++;
            
            // Check economic health
            if (_cachedData.Economic.CashFlow < 0) criticalIssues++;
            if (_cachedData.Economic.ProfitMargin < 10f) warnings++;
            
            // Check automation systems
            if (_cachedData.Automation.SystemUptime < 95f) warnings++;
            if (_cachedData.Automation.ActiveAlerts > 5) criticalIssues++;
            
            // Check AI advisor
            if (_cachedData.AIAdvisor.SystemEfficiencyScore < 0.7f) warnings++;
            
            if (criticalIssues > 0)
                return SystemStatus.Critical;
            else if (warnings > 2)
                return SystemStatus.Warning;
            else
                return SystemStatus.Optimal;
        }
        
        private float CalculateFacilityEfficiency()
        {
            float environmentalScore = (_cachedData.Environmental.HVACEfficiency + _cachedData.Environmental.LightingEfficiency) / 200f;
            float economicScore = _cachedData.Economic.FinancialHealthScore;
            float automationScore = _cachedData.Automation.SystemUptime / 100f;
            float aiScore = _cachedData.AIAdvisor.SystemEfficiencyScore;
            
            return (environmentalScore + economicScore + automationScore + aiScore) / 4f;
        }
        
        private int CountAvailableSystems()
        {
            int count = 0;
            // if (_automationManager != null) count++;
            // if (_hvacManager != null) count++;
            // if (_lightingManager != null) count++;
            // if (_investmentManager != null) count++;
            // if (_tradingManager != null) count++;
            // if (_marketManager != null) count++;
            // if (_aiAdvisorManager != null) count++;
            return count;
        }
        
        private void NavigateToPanel(string panelName)
        {
            OnPanelSelected?.Invoke(panelName);
            Debug.Log($"Navigating to panel: {panelName}");
        }
        
        private void TriggerQuickAction(string actionName)
        {
            switch (actionName)
            {
                case "emergency-stop":
                    // if (_automationManager != null)
                    // {
                        Debug.Log("Emergency stop triggered - would halt all systems");
                        // Would trigger emergency shutdown procedures
                    // }
                    break;
                    
                case "optimize-all":
                    // if (_aiAdvisorManager != null)
                    // {
                        Debug.Log("System optimization triggered");
                        // Would trigger AI-driven optimization
                    // }
                    break;
            }
            
            OnQuickActionTriggered?.Invoke(actionName);
        }
        
        private void ToggleTheme()
        {
            _enableDarkMode = !_enableDarkMode;
            ApplyTheme();
        }
        
        private void ApplyTheme()
        {
            if (_currentTheme == null) return;
            
            _rootElement.RemoveFromClassList("light-theme");
            _rootElement.RemoveFromClassList("dark-theme");
            _rootElement.AddToClassList(_enableDarkMode ? "dark-theme" : "light-theme");
            
            Debug.Log($"Applied {(_enableDarkMode ? "dark" : "light")} theme to dashboard");
        }
        
        private void OpenSettings()
        {
            Debug.Log("Opening dashboard settings");
            // Would open settings panel
        }
        
        public void SetAutoRefresh(bool enabled)
        {
            _autoRefresh = enabled;
            
            if (enabled && !IsInvoking(nameof(RefreshDashboard)))
            {
                InvokeRepeating(nameof(RefreshDashboard), 1f, _refreshInterval);
            }
            // else if (!enabled && IsInvoking(nameof(RefreshDashboard)))
            // {
                CancelInvoke(nameof(RefreshDashboard));
            // }
        }
        
        public void SetRefreshInterval(float interval)
        {
            _refreshInterval = Mathf.Max(1f, interval);
            
            if (_autoRefresh)
            {
                CancelInvoke(nameof(RefreshDashboard));
                InvokeRepeating(nameof(RefreshDashboard), 1f, _refreshInterval);
            }
        }
        
        public DashboardData GetCurrentData()
        {
            return _cachedData;
        }
        
        private void OnDestroy()
        {
            CancelInvoke();
        }
        
        public void Show()
        {
            IsVisible = true;
        }
        
        public void Hide()
        {
            IsVisible = false;
        }
    }
}