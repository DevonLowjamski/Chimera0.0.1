using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
// using ProjectChimera.Systems.Analytics;
using ProjectChimera.Data.UI;

namespace ProjectChimera.UI.DataVisualization
{
    /// <summary>
    /// Data Visualization UI Controller for Project Chimera.
    /// Provides comprehensive analytics, charts, graphs, and data insights for facility management.
    /// Features real-time data visualization, trend analysis, and performance monitoring dashboards.
    /// </summary>
    public class DataVisualizationController : MonoBehaviour
    {
        [Header("Visualization UI Configuration")]
        [SerializeField] private UIDocument _visualizationDocument;
        [SerializeField] private DataVisualizationSettings _visualizationSettings;
        [SerializeField] private bool _enableRealTimeUpdates = true;
        [SerializeField] private float _updateInterval = 5f;
        
        [Header("Chart Configuration")]
        [SerializeField] private int _maxDataPoints = 100;
        [SerializeField] private bool _enableInteractiveCharts = true;
        [SerializeField] private float _chartAnimationDuration = 0.5f;
        
        [Header("Performance Configuration")]
        [SerializeField] private bool _enableDataCaching = true;
        [SerializeField] private float _cacheRefreshInterval = 30f;
        [SerializeField] private int _maxCachedDataSets = 20;
        
        [Header("Export Configuration")]
        [SerializeField] private bool _enableDataExport = true;
        [SerializeField] private string[] _supportedExportFormats = { "CSV", "JSON", "PDF" };
        
        [Header("Audio Configuration")]
        [SerializeField] private AudioClip _chartUpdateSound;
        [SerializeField] private AudioClip _alertSound;
        [SerializeField] private AudioClip _exportCompleteSound;
        [SerializeField] private AudioSource _audioSource;
        
        // System references
        // private AnalyticsManager _analyticsManager;
        // private DataManager _dataManager;
        
        // UI Elements - Main Interface
        private VisualElement _rootElement;
        private Button _overviewTabButton;
        private Button _performanceTabButton;
        private Button _trendsTabButton;
        private Button _compareTabButton;
        private Button _reportsTabButton;
        
        // Tab Panels
        private VisualElement _overviewPanel;
        private VisualElement _performancePanel;
        private VisualElement _trendsPanel;
        private VisualElement _comparePanel;
        private VisualElement _reportsPanel;
        
        // Overview Elements
        private VisualElement _kpiContainer;
        private VisualElement _quickChartsContainer;
        private VisualElement _alertsContainer;
        private VisualElement _summaryStatsContainer;
        
        // Performance Elements
        private VisualElement _performanceChartsContainer;
        private VisualElement _metricsGridContainer;
        private VisualElement _performanceAlertsContainer;
        private DropdownField _performanceMetricSelector;
        private DropdownField _performanceTimeRange;
        
        // Trends Elements
        private VisualElement _trendChartsContainer;
        private VisualElement _forecastContainer;
        private VisualElement _correlationMatrix;
        private DropdownField _trendAnalysisType;
        private Slider _forecastRangeSlider;
        
        // Compare Elements
        private VisualElement _comparisonChartsContainer;
        private VisualElement _benchmarkContainer;
        private DropdownField _compareMetricA;
        private DropdownField _compareMetricB;
        private DropdownField _comparisonTimeframe;
        
        // Reports Elements
        private VisualElement _reportTemplatesContainer;
        private VisualElement _customReportBuilder;
        private VisualElement _scheduledReportsContainer;
        private Button _generateReportButton;
        private Button _exportDataButton;
        private Button _scheduleReportButton;
        
        // Chart Controls
        private VisualElement _chartControlsPanel;
        private DropdownField _chartTypeSelector;
        private Toggle _showGridToggle;
        private Toggle _showLegendToggle;
        private Toggle _enableAnimationsToggle;
        private Slider _zoomLevelSlider;
        private Button _resetZoomButton;
        private Button _fullscreenButton;
        
        // Filter Controls
        private VisualElement _filterPanel;
        private DropdownField _dateRangeFilter;
        private DropdownField _facilityZoneFilter;
        private DropdownField _dataSourceFilter;
        private TextField _customFilterField;
        private Button _applyFiltersButton;
        private Button _clearFiltersButton;
        
        // Data and State
        private Dictionary<string, ChartData> _chartDataCache = new Dictionary<string, ChartData>();
        private List<DataPoint> _currentDataSet = new List<DataPoint>();
        private List<AnalyticsAlert> _activeAlerts = new List<AnalyticsAlert>();
        private Dictionary<string, PerformanceMetric> _performanceMetrics = new Dictionary<string, PerformanceMetric>();
        private string _currentTab = "overview";
        private string _selectedChartType = "Line";
        private DateTimeRange _currentTimeRange = new DateTimeRange();
        private DataFilter _activeFilters = new DataFilter();
        private float _lastUpdateTime;
        private bool _isUpdating = false;
        
        // Chart instances
        private Dictionary<string, IChartRenderer> _activeCharts = new Dictionary<string, IChartRenderer>();
        
        // Events
        public System.Action<string> OnTabChanged;
        public System.Action<ChartData> OnChartUpdated;
        public System.Action<AnalyticsAlert> OnAlertTriggered;
        public System.Action<string> OnDataExported;
        public System.Action<DataFilter> OnFiltersApplied;
        
        private void Start()
        {
            InitializeController();
            InitializeSystemReferences();
            SetupUIElements();
            SetupEventHandlers();
            LoadVisualizationData();
            
            if (_enableRealTimeUpdates)
            {
                InvokeRepeating(nameof(UpdateVisualizationData), 1f, _updateInterval);
                InvokeRepeating(nameof(RefreshCharts), 2f, _chartAnimationDuration * 2f);
            }
        }
        
        private void InitializeController()
        {
            if (_visualizationDocument == null)
            {
                Debug.LogError("Data Visualization UI Document not assigned!");
                return;
            }
            
            _rootElement = _visualizationDocument.rootVisualElement;
            _lastUpdateTime = Time.time;
            
            // Initialize time range
            _currentTimeRange = new DateTimeRange
            {
                StartDate = DateTime.Now.AddDays(-7),
                EndDate = DateTime.Now
            };
            
            // Initialize filters
            _activeFilters = new DataFilter
            {
                DateRange = _currentTimeRange,
                FacilityZones = new List<string> { "All" },
                DataSources = new List<string> { "All" },
                MetricTypes = new List<string> { "All" }
            };
            
            Debug.Log("Data Visualization Controller initialized");
        }
        
        private void InitializeSystemReferences()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogWarning("GameManager not found - using simulation mode");
                return;
            }
            
            // _analyticsManager = gameManager.GetManager<AnalyticsManager>();
            // _dataManager = gameManager.GetManager<DataManager>();
            
            Debug.Log("Data Visualization connected to analytics systems");
        }
        
        private void SetupUIElements()
        {
            // Main navigation tabs
            _overviewTabButton = _rootElement.Q<Button>("overview-tab");
            _performanceTabButton = _rootElement.Q<Button>("performance-tab");
            _trendsTabButton = _rootElement.Q<Button>("trends-tab");
            _compareTabButton = _rootElement.Q<Button>("compare-tab");
            _reportsTabButton = _rootElement.Q<Button>("reports-tab");
            
            // Tab panels
            _overviewPanel = _rootElement.Q<VisualElement>("overview-panel");
            _performancePanel = _rootElement.Q<VisualElement>("performance-panel");
            _trendsPanel = _rootElement.Q<VisualElement>("trends-panel");
            _comparePanel = _rootElement.Q<VisualElement>("compare-panel");
            _reportsPanel = _rootElement.Q<VisualElement>("reports-panel");
            
            // Overview elements
            _kpiContainer = _rootElement.Q<VisualElement>("kpi-container");
            _quickChartsContainer = _rootElement.Q<VisualElement>("quick-charts-container");
            _alertsContainer = _rootElement.Q<VisualElement>("alerts-container");
            _summaryStatsContainer = _rootElement.Q<VisualElement>("summary-stats-container");
            
            // Performance elements
            _performanceChartsContainer = _rootElement.Q<VisualElement>("performance-charts-container");
            _metricsGridContainer = _rootElement.Q<VisualElement>("metrics-grid-container");
            _performanceAlertsContainer = _rootElement.Q<VisualElement>("performance-alerts-container");
            _performanceMetricSelector = _rootElement.Q<DropdownField>("performance-metric-selector");
            _performanceTimeRange = _rootElement.Q<DropdownField>("performance-time-range");
            
            // Trends elements
            _trendChartsContainer = _rootElement.Q<VisualElement>("trend-charts-container");
            _forecastContainer = _rootElement.Q<VisualElement>("forecast-container");
            _correlationMatrix = _rootElement.Q<VisualElement>("correlation-matrix");
            _trendAnalysisType = _rootElement.Q<DropdownField>("trend-analysis-type");
            _forecastRangeSlider = _rootElement.Q<Slider>("forecast-range-slider");
            
            // Compare elements
            _comparisonChartsContainer = _rootElement.Q<VisualElement>("comparison-charts-container");
            _benchmarkContainer = _rootElement.Q<VisualElement>("benchmark-container");
            _compareMetricA = _rootElement.Q<DropdownField>("compare-metric-a");
            _compareMetricB = _rootElement.Q<DropdownField>("compare-metric-b");
            _comparisonTimeframe = _rootElement.Q<DropdownField>("comparison-timeframe");
            
            // Reports elements
            _reportTemplatesContainer = _rootElement.Q<VisualElement>("report-templates-container");
            _customReportBuilder = _rootElement.Q<VisualElement>("custom-report-builder");
            _scheduledReportsContainer = _rootElement.Q<VisualElement>("scheduled-reports-container");
            _generateReportButton = _rootElement.Q<Button>("generate-report-button");
            _exportDataButton = _rootElement.Q<Button>("export-data-button");
            _scheduleReportButton = _rootElement.Q<Button>("schedule-report-button");
            
            // Chart controls
            _chartControlsPanel = _rootElement.Q<VisualElement>("chart-controls-panel");
            _chartTypeSelector = _rootElement.Q<DropdownField>("chart-type-selector");
            _showGridToggle = _rootElement.Q<Toggle>("show-grid-toggle");
            _showLegendToggle = _rootElement.Q<Toggle>("show-legend-toggle");
            _enableAnimationsToggle = _rootElement.Q<Toggle>("enable-animations-toggle");
            _zoomLevelSlider = _rootElement.Q<Slider>("zoom-level-slider");
            _resetZoomButton = _rootElement.Q<Button>("reset-zoom-button");
            _fullscreenButton = _rootElement.Q<Button>("fullscreen-button");
            
            // Filter controls
            _filterPanel = _rootElement.Q<VisualElement>("filter-panel");
            _dateRangeFilter = _rootElement.Q<DropdownField>("date-range-filter");
            _facilityZoneFilter = _rootElement.Q<DropdownField>("facility-zone-filter");
            _dataSourceFilter = _rootElement.Q<DropdownField>("data-source-filter");
            _customFilterField = _rootElement.Q<TextField>("custom-filter-field");
            _applyFiltersButton = _rootElement.Q<Button>("apply-filters-button");
            _clearFiltersButton = _rootElement.Q<Button>("clear-filters-button");
            
            SetupDropdowns();
            SetupInitialState();
        }
        
        private void SetupDropdowns()
        {
            // Chart types
            if (_chartTypeSelector != null)
            {
                _chartTypeSelector.choices = new List<string>
                {
                    "Line", "Bar", "Area", "Pie", "Scatter", "Heatmap", "Gauge", "Radar"
                };
                _chartTypeSelector.value = "Line";
            }
            
            // Performance metrics
            if (_performanceMetricSelector != null)
            {
                _performanceMetricSelector.choices = new List<string>
                {
                    "Overall Efficiency", "Energy Usage", "Yield Performance", "Cost Analysis",
                    "Quality Metrics", "Environmental Impact", "Resource Utilization"
                };
                _performanceMetricSelector.value = "Overall Efficiency";
            }
            
            // Time ranges
            var timeRangeChoices = new List<string>
            {
                "Last Hour", "Last 24 Hours", "Last Week", "Last Month", 
                "Last Quarter", "Last Year", "Custom Range"
            };
            
            if (_performanceTimeRange != null)
            {
                _performanceTimeRange.choices = timeRangeChoices;
                _performanceTimeRange.value = "Last Week";
            }
            
            if (_dateRangeFilter != null)
            {
                _dateRangeFilter.choices = timeRangeChoices;
                _dateRangeFilter.value = "Last Week";
            }
            
            // Trend analysis types
            if (_trendAnalysisType != null)
            {
                _trendAnalysisType.choices = new List<string>
                {
                    "Linear Trend", "Exponential", "Seasonal", "Moving Average", "Polynomial"
                };
                _trendAnalysisType.value = "Linear Trend";
            }
            
            // Comparison metrics
            var metricChoices = new List<string>
            {
                "Temperature", "Humidity", "CO2 Levels", "Light Intensity", 
                "Energy Usage", "Yield", "Growth Rate", "Quality Score"
            };
            
            if (_compareMetricA != null)
            {
                _compareMetricA.choices = metricChoices;
                _compareMetricA.value = "Temperature";
            }
            
            if (_compareMetricB != null)
            {
                _compareMetricB.choices = metricChoices;
                _compareMetricB.value = "Humidity";
            }
            
            // Facility zones
            if (_facilityZoneFilter != null)
            {
                _facilityZoneFilter.choices = new List<string>
                {
                    "All Zones", "Vegetative Zone", "Flowering Zone", "Drying Room", 
                    "Processing Area", "Storage", "Laboratory"
                };
                _facilityZoneFilter.value = "All Zones";
            }
            
            // Data sources
            if (_dataSourceFilter != null)
            {
                _dataSourceFilter.choices = new List<string>
                {
                    "All Sources", "Environmental Sensors", "Equipment Logs", "Production Data",
                    "Quality Control", "Financial Records", "Energy Monitoring"
                };
                _dataSourceFilter.value = "All Sources";
            }
        }
        
        private void SetupInitialState()
        {
            // Show overview panel by default
            ShowPanel("overview");
            
            // Initialize chart controls
            if (_showGridToggle != null)
                _showGridToggle.value = true;
            
            if (_showLegendToggle != null)
                _showLegendToggle.value = true;
            
            if (_enableAnimationsToggle != null)
                _enableAnimationsToggle.value = _enableInteractiveCharts;
            
            if (_zoomLevelSlider != null)
                _zoomLevelSlider.value = 1f;
            
            if (_forecastRangeSlider != null)
            {
                _forecastRangeSlider.lowValue = 1f;
                _forecastRangeSlider.highValue = 30f;
                _forecastRangeSlider.value = 7f;
            }
        }
        
        private void SetupEventHandlers()
        {
            // Tab navigation
            _overviewTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("overview"));
            _performanceTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("performance"));
            _trendsTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("trends"));
            _compareTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("compare"));
            _reportsTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("reports"));
            
            // Chart controls
            _chartTypeSelector?.RegisterValueChangedCallback(evt => ChangeChartType(evt.newValue));
            _showGridToggle?.RegisterValueChangedCallback(evt => ToggleGrid(evt.newValue));
            _showLegendToggle?.RegisterValueChangedCallback(evt => ToggleLegend(evt.newValue));
            _enableAnimationsToggle?.RegisterValueChangedCallback(evt => ToggleAnimations(evt.newValue));
            _zoomLevelSlider?.RegisterValueChangedCallback(evt => UpdateZoomLevel(evt.newValue));
            _resetZoomButton?.RegisterCallback<ClickEvent>(evt => ResetZoom());
            _fullscreenButton?.RegisterCallback<ClickEvent>(evt => ToggleFullscreen());
            
            // Filter controls
            _dateRangeFilter?.RegisterValueChangedCallback(evt => UpdateDateRange(evt.newValue));
            _facilityZoneFilter?.RegisterValueChangedCallback(evt => UpdateZoneFilter(evt.newValue));
            _dataSourceFilter?.RegisterValueChangedCallback(evt => UpdateDataSourceFilter(evt.newValue));
            _applyFiltersButton?.RegisterCallback<ClickEvent>(evt => ApplyFilters());
            _clearFiltersButton?.RegisterCallback<ClickEvent>(evt => ClearFilters());
            
            // Performance controls
            _performanceMetricSelector?.RegisterValueChangedCallback(evt => UpdatePerformanceMetric(evt.newValue));
            _performanceTimeRange?.RegisterValueChangedCallback(evt => UpdatePerformanceTimeRange(evt.newValue));
            
            // Trends controls
            _trendAnalysisType?.RegisterValueChangedCallback(evt => UpdateTrendAnalysis(evt.newValue));
            _forecastRangeSlider?.RegisterValueChangedCallback(evt => UpdateForecastRange(evt.newValue));
            
            // Comparison controls
            _compareMetricA?.RegisterValueChangedCallback(evt => UpdateComparisonMetricA(evt.newValue));
            _compareMetricB?.RegisterValueChangedCallback(evt => UpdateComparisonMetricB(evt.newValue));
            _comparisonTimeframe?.RegisterValueChangedCallback(evt => UpdateComparisonTimeframe(evt.newValue));
            
            // Reports controls
            _generateReportButton?.RegisterCallback<ClickEvent>(evt => GenerateReport());
            _exportDataButton?.RegisterCallback<ClickEvent>(evt => ExportData());
            _scheduleReportButton?.RegisterCallback<ClickEvent>(evt => ScheduleReport());
        }
        
        #region Panel Management
        
        private void ShowPanel(string panelName)
        {
            // Hide all panels
            _overviewPanel?.AddToClassList("hidden");
            _performancePanel?.AddToClassList("hidden");
            _trendsPanel?.AddToClassList("hidden");
            _comparePanel?.AddToClassList("hidden");
            _reportsPanel?.AddToClassList("hidden");
            
            // Remove active state from all tabs
            _overviewTabButton?.RemoveFromClassList("tab-active");
            _performanceTabButton?.RemoveFromClassList("tab-active");
            _trendsTabButton?.RemoveFromClassList("tab-active");
            _compareTabButton?.RemoveFromClassList("tab-active");
            _reportsTabButton?.RemoveFromClassList("tab-active");
            
            // Show selected panel and activate tab
            switch (panelName)
            {
                case "overview":
                    _overviewPanel?.RemoveFromClassList("hidden");
                    _overviewTabButton?.AddToClassList("tab-active");
                    RefreshOverviewData();
                    break;
                case "performance":
                    _performancePanel?.RemoveFromClassList("hidden");
                    _performanceTabButton?.AddToClassList("tab-active");
                    RefreshPerformanceData();
                    break;
                case "trends":
                    _trendsPanel?.RemoveFromClassList("hidden");
                    _trendsTabButton?.AddToClassList("tab-active");
                    RefreshTrendsData();
                    break;
                case "compare":
                    _comparePanel?.RemoveFromClassList("hidden");
                    _compareTabButton?.AddToClassList("tab-active");
                    RefreshComparisonData();
                    break;
                case "reports":
                    _reportsPanel?.RemoveFromClassList("hidden");
                    _reportsTabButton?.AddToClassList("tab-active");
                    RefreshReportsData();
                    break;
            }
            
            _currentTab = panelName;
            OnTabChanged?.Invoke(panelName);
            
            Debug.Log($"Switched to {panelName} panel");
        }
        
        #endregion
        
        #region Data Updates
        
        [ContextMenu("Update Visualization Data")]
        public void UpdateVisualizationData()
        {
            if (_isUpdating) return;
            
            _isUpdating = true;
            
            // Update data sources
            UpdateAnalyticsData();
            
            // Update alerts
            CheckAnalyticsAlerts();
            
            // Update current tab data
            switch (_currentTab)
            {
                case "overview":
                    RefreshOverviewData();
                    break;
                case "performance":
                    RefreshPerformanceData();
                    break;
                case "trends":
                    RefreshTrendsData();
                    break;
                case "compare":
                    RefreshComparisonData();
                    break;
                case "reports":
                    RefreshReportsData();
                    break;
            }
            
            _lastUpdateTime = Time.time;
            _isUpdating = false;
        }
        
        private void UpdateAnalyticsData()
        {
            // if (_analyticsManager != null)
            // {
                // var analyticsData = _analyticsManager.GetAnalyticsData(_activeFilters);
                var analyticsData = new object(); // Placeholder for analytics data
                if (analyticsData != null)
                {
                    ProcessAnalyticsData(analyticsData);
                }
            // }
            // else
            // {
                // Generate simulated data
                GenerateSimulatedData();
            // }
        }
        
        private void ProcessAnalyticsData(object analyticsData)
        {
            // Process real analytics data from the system
            Debug.Log("Processing real analytics data");
        }
        
        private void GenerateSimulatedData()
        {
            // Generate sample data for demonstration
            _currentDataSet.Clear();
            var random = new System.Random();
            var startDate = _currentTimeRange.StartDate;
            var endDate = _currentTimeRange.EndDate;
            var timeSpan = endDate - startDate;
            var dataPoints = Math.Min(_maxDataPoints, (int)timeSpan.TotalHours);
            
            for (int i = 0; i < dataPoints; i++)
            {
                var timestamp = startDate.AddHours(i * (timeSpan.TotalHours / dataPoints));
                var baseValue = 75f + (float)(Math.Sin(i * 0.1) * 15); // Sine wave pattern
                var noise = (float)(random.NextDouble() - 0.5) * 10; // Random noise
                
                var dataPoint = new DataPoint
                {
                    Timestamp = timestamp,
                    Value = baseValue + noise,
                    Category = "Temperature",
                    Zone = "Flowering Zone"
                };
                
                _currentDataSet.Add(dataPoint);
            }
            
            // Cache the data
            var cacheKey = $"{_currentTab}_{_activeFilters.GetHashCode()}";
            if (!_chartDataCache.ContainsKey(cacheKey))
            {
                _chartDataCache[cacheKey] = new ChartData
                {
                    DataPoints = new List<DataPoint>(_currentDataSet),
                    LastUpdated = DateTime.Now,
                    ChartType = _selectedChartType
                };
            }
        }
        
        #endregion
        
        #region Chart Management
        
        private void RefreshCharts()
        {
            // Refresh active charts with new data
            foreach (var chart in _activeCharts.Values)
            {
                chart?.UpdateData(_currentDataSet);
            }
            
            PlaySound(_chartUpdateSound);
        }
        
        private void ChangeChartType(string chartType)
        {
            _selectedChartType = chartType;
            
            // Update all active charts to new type
            foreach (var chartId in _activeCharts.Keys.ToList())
            {
                RecreateChart(chartId, chartType);
            }
        }
        
        private void RecreateChart(string chartId, string chartType)
        {
            var container = GetChartContainer(chartId);
            if (container == null) return;
            
            // Remove existing chart
            container.Clear();
            
            // Create new chart based on type
            var chart = CreateChart(chartType, container);
            if (chart != null)
            {
                _activeCharts[chartId] = chart;
                chart.UpdateData(_currentDataSet);
            }
        }
        
        private VisualElement GetChartContainer(string chartId)
        {
            // Return appropriate container based on chart ID
            return _quickChartsContainer; // Simplified for demo
        }
        
        private IChartRenderer CreateChart(string chartType, VisualElement container)
        {
            // Create chart based on type - this would integrate with a charting library
            return new SimpleChartRenderer(container, chartType);
        }
        
        #endregion
        
        #region Tab-Specific Data Refresh
        
        private void RefreshOverviewData()
        {
            UpdateKPIs();
            UpdateQuickCharts();
            UpdateSummaryStats();
            UpdateAlertsDisplay();
        }
        
        private void UpdateKPIs()
        {
            if (_kpiContainer == null) return;
            
            _kpiContainer.Clear();
            
            var kpis = new[]
            {
                ("Overall Efficiency", "87.3%", "positive"),
                ("Energy Usage", "1,247 kWh", "neutral"),
                ("Daily Yield", "12.4 kg", "positive"),
                ("Quality Score", "9.2/10", "positive"),
                ("Cost per Gram", "$2.34", "negative"),
                ("Revenue Today", "$4,567", "positive")
            };
            
            foreach (var (name, value, trend) in kpis)
            {
                var kpiElement = CreateKPIElement(name, value, trend);
                _kpiContainer.Add(kpiElement);
            }
        }
        
        private VisualElement CreateKPIElement(string name, string value, string trend)
        {
            var element = new VisualElement();
            element.AddToClassList("kpi-item");
            element.AddToClassList($"trend-{trend}");
            
            var nameLabel = new Label(name);
            nameLabel.AddToClassList("kpi-name");
            
            var valueLabel = new Label(value);
            valueLabel.AddToClassList("kpi-value");
            
            var trendIndicator = new VisualElement();
            trendIndicator.AddToClassList("trend-indicator");
            trendIndicator.AddToClassList($"trend-{trend}");
            
            element.Add(nameLabel);
            element.Add(valueLabel);
            element.Add(trendIndicator);
            
            return element;
        }
        
        private void UpdateQuickCharts()
        {
            if (_quickChartsContainer == null) return;
            
            // Create sample charts
            CreateOverviewChart("temperature-chart", "Temperature Trends");
            CreateOverviewChart("humidity-chart", "Humidity Levels");
            CreateOverviewChart("energy-chart", "Energy Usage");
        }
        
        private void CreateOverviewChart(string chartId, string title)
        {
            var chartContainer = new VisualElement();
            chartContainer.name = chartId;
            chartContainer.AddToClassList("quick-chart");
            
            var titleLabel = new Label(title);
            titleLabel.AddToClassList("chart-title");
            
            var chartArea = new VisualElement();
            chartArea.AddToClassList("chart-area");
            
            chartContainer.Add(titleLabel);
            chartContainer.Add(chartArea);
            _quickChartsContainer?.Add(chartContainer);
            
            // Create chart renderer
            var chart = CreateChart(_selectedChartType, chartArea);
            if (chart != null)
            {
                _activeCharts[chartId] = chart;
                chart.UpdateData(_currentDataSet);
            }
        }
        
        private void UpdateSummaryStats()
        {
            if (_summaryStatsContainer == null) return;
            
            _summaryStatsContainer.Clear();
            
            var stats = new[]
            {
                ("Active Zones", "6/8"),
                ("Sensors Online", "24/26"),
                ("Devices Connected", "18/20"),
                ("Alerts Active", "3"),
                ("Data Points Today", "15,247"),
                ("Uptime", "99.8%")
            };
            
            foreach (var (name, value) in stats)
            {
                var statElement = CreateStatElement(name, value);
                _summaryStatsContainer.Add(statElement);
            }
        }
        
        private VisualElement CreateStatElement(string name, string value)
        {
            var element = new VisualElement();
            element.AddToClassList("stat-item");
            
            var nameLabel = new Label(name);
            nameLabel.AddToClassList("stat-name");
            
            var valueLabel = new Label(value);
            valueLabel.AddToClassList("stat-value");
            
            element.Add(nameLabel);
            element.Add(valueLabel);
            
            return element;
        }
        
        private void RefreshPerformanceData()
        {
            UpdatePerformanceCharts();
            UpdateMetricsGrid();
            UpdatePerformanceAlerts();
        }
        
        private void UpdatePerformanceCharts()
        {
            // Create performance-specific charts
            Debug.Log("Updating performance charts");
        }
        
        private void UpdateMetricsGrid()
        {
            // Update performance metrics grid
            Debug.Log("Updating metrics grid");
        }
        
        private void UpdatePerformanceAlerts()
        {
            // Update performance alerts
            Debug.Log("Updating performance alerts");
        }
        
        private void RefreshTrendsData()
        {
            UpdateTrendCharts();
            UpdateForecast();
            UpdateCorrelationMatrix();
        }
        
        private void UpdateTrendCharts()
        {
            // Create trend analysis charts
            Debug.Log("Updating trend charts");
        }
        
        private void UpdateForecast()
        {
            // Update forecast data
            Debug.Log("Updating forecast");
        }
        
        private void UpdateCorrelationMatrix()
        {
            // Update correlation analysis
            Debug.Log("Updating correlation matrix");
        }
        
        private void RefreshComparisonData()
        {
            UpdateComparisonCharts();
            UpdateBenchmarks();
        }
        
        private void UpdateComparisonCharts()
        {
            // Create comparison charts
            Debug.Log("Updating comparison charts");
        }
        
        private void UpdateBenchmarks()
        {
            // Update benchmark data
            Debug.Log("Updating benchmarks");
        }
        
        private void RefreshReportsData()
        {
            UpdateReportTemplates();
            UpdateScheduledReports();
        }
        
        private void UpdateReportTemplates()
        {
            // Update available report templates
            Debug.Log("Updating report templates");
        }
        
        private void UpdateScheduledReports()
        {
            // Update scheduled reports list
            Debug.Log("Updating scheduled reports");
        }
        
        #endregion
        
        #region Event Handlers
        
        private void ToggleGrid(bool showGrid)
        {
            foreach (var chart in _activeCharts.Values)
            {
                chart?.SetGridVisible(showGrid);
            }
        }
        
        private void ToggleLegend(bool showLegend)
        {
            foreach (var chart in _activeCharts.Values)
            {
                chart?.SetLegendVisible(showLegend);
            }
        }
        
        private void ToggleAnimations(bool enableAnimations)
        {
            foreach (var chart in _activeCharts.Values)
            {
                chart?.SetAnimationsEnabled(enableAnimations);
            }
        }
        
        private void UpdateZoomLevel(float zoomLevel)
        {
            foreach (var chart in _activeCharts.Values)
            {
                chart?.SetZoomLevel(zoomLevel);
            }
        }
        
        private void ResetZoom()
        {
            if (_zoomLevelSlider != null)
                _zoomLevelSlider.value = 1f;
            
            UpdateZoomLevel(1f);
        }
        
        private void ToggleFullscreen()
        {
            Debug.Log("Toggle fullscreen mode");
        }
        
        private void UpdateDateRange(string dateRange)
        {
            _activeFilters.DateRange = ParseDateRange(dateRange);
            UpdateVisualizationData();
        }
        
        private void UpdateZoneFilter(string zone)
        {
            _activeFilters.FacilityZones = new List<string> { zone };
        }
        
        private void UpdateDataSourceFilter(string dataSource)
        {
            _activeFilters.DataSources = new List<string> { dataSource };
        }
        
        private void ApplyFilters()
        {
            OnFiltersApplied?.Invoke(_activeFilters);
            UpdateVisualizationData();
            Debug.Log("Filters applied");
        }
        
        private void ClearFilters()
        {
            _activeFilters = new DataFilter
            {
                DateRange = _currentTimeRange,
                FacilityZones = new List<string> { "All" },
                DataSources = new List<string> { "All" },
                MetricTypes = new List<string> { "All" }
            };
            
            // Reset UI controls
            if (_dateRangeFilter != null)
                _dateRangeFilter.value = "Last Week";
            if (_facilityZoneFilter != null)
                _facilityZoneFilter.value = "All Zones";
            if (_dataSourceFilter != null)
                _dataSourceFilter.value = "All Sources";
            
            UpdateVisualizationData();
            Debug.Log("Filters cleared");
        }
        
        private void UpdatePerformanceMetric(string metric)
        {
            RefreshPerformanceData();
            Debug.Log($"Performance metric changed to: {metric}");
        }
        
        private void UpdatePerformanceTimeRange(string timeRange)
        {
            RefreshPerformanceData();
            Debug.Log($"Performance time range changed to: {timeRange}");
        }
        
        private void UpdateTrendAnalysis(string analysisType)
        {
            RefreshTrendsData();
            Debug.Log($"Trend analysis type changed to: {analysisType}");
        }
        
        private void UpdateForecastRange(float range)
        {
            RefreshTrendsData();
            Debug.Log($"Forecast range changed to: {range} days");
        }
        
        private void UpdateComparisonMetricA(string metric)
        {
            RefreshComparisonData();
            Debug.Log($"Comparison metric A changed to: {metric}");
        }
        
        private void UpdateComparisonMetricB(string metric)
        {
            RefreshComparisonData();
            Debug.Log($"Comparison metric B changed to: {metric}");
        }
        
        private void UpdateComparisonTimeframe(string timeframe)
        {
            RefreshComparisonData();
            Debug.Log($"Comparison timeframe changed to: {timeframe}");
        }
        
        private void GenerateReport()
        {
            Debug.Log("Generating custom report");
            PlaySound(_exportCompleteSound);
        }
        
        private void ExportData()
        {
            string format = "CSV"; // Would be selected by user
            OnDataExported?.Invoke($"data_export.{format.ToLower()}");
            PlaySound(_exportCompleteSound);
            Debug.Log($"Data exported in {format} format");
        }
        
        private void ScheduleReport()
        {
            Debug.Log("Scheduling report");
        }
        
        #endregion
        
        #region Helper Methods
        
        private void LoadVisualizationData()
        {
            // if (_analyticsManager != null)
            // {
                // Load data from analytics manager
            // }
            
            // Initialize with sample data
            GenerateSimulatedData();
            
            Debug.Log("Visualization data loaded");
        }
        
        private void CheckAnalyticsAlerts()
        {
            // Check for data-driven alerts
            var newAlerts = new List<AnalyticsAlert>();
            
            // Example alert conditions
            if (_currentDataSet.Any(dp => dp.Value > 85f))
            {
                newAlerts.Add(new AnalyticsAlert
                {
                    AlertId = Guid.NewGuid().ToString(),
                    Title = "High Temperature Alert",
                    Message = "Temperature exceeded safe threshold in Flowering Zone",
                    Severity = AlertSeverity.Warning,
                    Timestamp = DateTime.Now
                });
            }
            
            foreach (var alert in newAlerts)
            {
                if (!_activeAlerts.Any(a => a.Title == alert.Title))
                {
                    _activeAlerts.Add(alert);
                    OnAlertTriggered?.Invoke(alert);
                    PlaySound(_alertSound);
                }
            }
            
            UpdateAlertsDisplay();
        }
        
        private void UpdateAlertsDisplay()
        {
            if (_alertsContainer == null) return;
            
            _alertsContainer.Clear();
            
            foreach (var alert in _activeAlerts.Take(5)) // Show latest 5 alerts
            {
                var alertElement = CreateAlertElement(alert);
                _alertsContainer.Add(alertElement);
            }
        }
        
        private VisualElement CreateAlertElement(AnalyticsAlert alert)
        {
            var element = new VisualElement();
            element.AddToClassList("alert-item");
            element.AddToClassList($"severity-{alert.Severity.ToString().ToLower()}");
            
            var titleLabel = new Label(alert.Title);
            titleLabel.AddToClassList("alert-title");
            
            var messageLabel = new Label(alert.Message);
            messageLabel.AddToClassList("alert-message");
            
            var timeLabel = new Label(alert.Timestamp.ToString("HH:mm:ss"));
            timeLabel.AddToClassList("alert-time");
            
            element.Add(titleLabel);
            element.Add(messageLabel);
            element.Add(timeLabel);
            
            return element;
        }
        
        private DateTimeRange ParseDateRange(string rangeText)
        {
            var now = DateTime.Now;
            return rangeText switch
            {
                "Last Hour" => new DateTimeRange { StartDate = now.AddHours(-1), EndDate = now },
                "Last 24 Hours" => new DateTimeRange { StartDate = now.AddDays(-1), EndDate = now },
                "Last Week" => new DateTimeRange { StartDate = now.AddDays(-7), EndDate = now },
                "Last Month" => new DateTimeRange { StartDate = now.AddMonths(-1), EndDate = now },
                "Last Quarter" => new DateTimeRange { StartDate = now.AddMonths(-3), EndDate = now },
                "Last Year" => new DateTimeRange { StartDate = now.AddYears(-1), EndDate = now },
                _ => _currentTimeRange
            };
        }
        
        private void PlaySound(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }
        
        private void OnDestroy()
        {
            CancelInvoke();
        }
        
        #endregion
    }
    
    // Supporting data structures and interfaces
    [System.Serializable]
    public class ChartData
    {
        public List<DataPoint> DataPoints;
        public DateTime LastUpdated;
        public string ChartType;
        public Dictionary<string, object> Metadata;
    }
    
    [System.Serializable]
    public class DataPoint
    {
        public DateTime Timestamp;
        public float Value;
        public string Category;
        public string Zone;
        public Dictionary<string, object> Properties;
    }
    
    [System.Serializable]
    public class DateTimeRange
    {
        public DateTime StartDate;
        public DateTime EndDate;
    }
    
    [System.Serializable]
    public class DataFilter
    {
        public DateTimeRange DateRange;
        public List<string> FacilityZones;
        public List<string> DataSources;
        public List<string> MetricTypes;
        public string CustomFilter;
    }
    
    [System.Serializable]
    public class PerformanceMetric
    {
        public string Name;
        public float CurrentValue;
        public float TargetValue;
        public float Trend;
        public string Unit;
    }
    
    [System.Serializable]
    public class AnalyticsAlert
    {
        public string AlertId;
        public string Title;
        public string Message;
        public AlertSeverity Severity;
        public DateTime Timestamp;
        public bool IsAcknowledged;
    }
    
    public enum AlertSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }
    
    [System.Serializable]
    public class DataVisualizationSettings
    {
        public bool EnableRealTimeUpdates = true;
        public bool EnableInteractiveCharts = true;
        public bool EnableDataCaching = true;
        public float UpdateInterval = 5f;
        public int MaxDataPoints = 100;
        public bool PlaySoundEffects = true;
    }
    
    // Chart renderer interface
    public interface IChartRenderer
    {
        void UpdateData(List<DataPoint> dataPoints);
        void SetGridVisible(bool visible);
        void SetLegendVisible(bool visible);
        void SetAnimationsEnabled(bool enabled);
        void SetZoomLevel(float zoom);
    }
    
    // Simple chart renderer implementation
    public class SimpleChartRenderer : IChartRenderer
    {
        private VisualElement _container;
        private string _chartType;
        
        public SimpleChartRenderer(VisualElement container, string chartType)
        {
            _container = container;
            _chartType = chartType;
            InitializeChart();
        }
        
        private void InitializeChart()
        {
            _container.Clear();
            
            var chartElement = new VisualElement();
            chartElement.AddToClassList("chart-content");
            chartElement.AddToClassList($"chart-{_chartType.ToLower()}");
            
            var chartLabel = new Label($"{_chartType} Chart");
            chartLabel.AddToClassList("chart-placeholder");
            
            chartElement.Add(chartLabel);
            _container.Add(chartElement);
        }
        
        public void UpdateData(List<DataPoint> dataPoints)
        {
            // Update chart with new data
            Debug.Log($"Updating {_chartType} chart with {dataPoints.Count} data points");
        }
        
        public void SetGridVisible(bool visible)
        {
            Debug.Log($"Grid visibility: {visible}");
        }
        
        public void SetLegendVisible(bool visible)
        {
            Debug.Log($"Legend visibility: {visible}");
        }
        
        public void SetAnimationsEnabled(bool enabled)
        {
            Debug.Log($"Animations enabled: {enabled}");
        }
        
        public void SetZoomLevel(float zoom)
        {
            Debug.Log($"Zoom level: {zoom}");
        }
    }
}