using UnityEngine;
using UnityEngine.UIElements;
using ProjectChimera.Core;
using ProjectChimera.UI.Core;
using ProjectChimera.Data.Economy;
// using ProjectChimera.Systems.Economy;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ProjectChimera.UI.Financial
{
    /// <summary>
    /// Comprehensive currency management UI panel for Project Chimera.
    /// Provides real-time currency display, transaction history, financial analytics,
    /// budget management, and economic insights for engaging financial gameplay.
    /// </summary>
    public class CurrencyPanel : VisualElement
    {
        // Currency Panel Configuration
        private bool _enableRealTimeUpdates = true;
        private bool _showTransactionHistory = true;
        private bool _enableFinancialAnalytics = true;
        private float _updateInterval = 1f;
        
        // Visual Settings
        private bool _enableAnimations = true;
        private bool _showCurrencyIcons = true;
        private bool _enableColorCoding = true;
        private Color _positiveColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        private Color _negativeColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        
        // System References
        // private CurrencyManager _currencyManager;
        
        // UI Elements
        private VisualElement _rootContainer;
        private VisualElement _headerContainer;
        private VisualElement _currencyDisplayContainer;
        private VisualElement _tabContainer;
        private VisualElement _contentContainer;
        
        // Tab Elements
        private Button _overviewTab;
        private Button _transactionsTab;
        private Button _budgetTab;
        private Button _analyticsTab;
        
        // Content Panels
        private VisualElement _overviewContent;
        private VisualElement _transactionsContent;
        private VisualElement _budgetContent;
        private VisualElement _analyticsContent;
        
        // Currency Display Elements
        private Dictionary<CurrencyType, VisualElement> _currencyElements = new Dictionary<CurrencyType, VisualElement>();
        private Dictionary<CurrencyType, Label> _currencyAmountLabels = new Dictionary<CurrencyType, Label>();
        private Dictionary<CurrencyType, Label> _currencyChangeLabels = new Dictionary<CurrencyType, Label>();
        
        // Transaction History
        private ScrollView _transactionScrollView;
        private VisualElement _transactionContainer;
        private TextField _transactionSearchField;
        private DropdownField _transactionFilterDropdown;
        
        // Budget Management
        private ScrollView _budgetScrollView;
        private VisualElement _budgetContainer;
        private Button _createBudgetButton;
        
        // Analytics
        private VisualElement _analyticsChartContainer;
        private Label _netWorthLabel;
        private Label _burnRateLabel;
        private Label _runwayLabel;
        
        // State Management
        private string _activeTab = "overview";
        private float _lastUpdateTime = 0f;
        private List<Transaction> _displayedTransactions = new List<Transaction>();
        private Dictionary<CurrencyType, float> _lastKnownAmounts = new Dictionary<CurrencyType, float>();
        
        public void OnPanelInitialized()
        {
            // Panel initialization logic
            
            // Find system references
            // _currencyManager = GameManager.Instance?.GetManager<CurrencyManager>();
            
            // if (_currencyManager == null)
            // {
                // LogError("CurrencyManager not found - UI disabled");
                return;
            // }
            
            CreateUI();
            SubscribeToEvents();
            RefreshAllData();
            
            LogInfo("CurrencyPanel initialized");
        }
        
        private void Update()
        {
            // if (_currencyManager == null) return;
            
            // Update currency displays at specified interval
            if (_enableRealTimeUpdates && Time.time - _lastUpdateTime >= _updateInterval)
            {
                UpdateCurrencyDisplays();
                UpdateActiveTabContent();
                _lastUpdateTime = Time.time;
            }
        }
        
        private void CreateUI()
        {
            _rootContainer = new VisualElement();
            _rootContainer.name = "currency-panel";
            _rootContainer.AddToClassList("currency-panel");
            _rootContainer.style.width = new Length(100, LengthUnit.Percent);
            _rootContainer.style.height = new Length(100, LengthUnit.Percent);
            _rootContainer.style.paddingLeft = 15f;
            _rootContainer.style.paddingRight = 15f;
            _rootContainer.style.paddingTop = 15f;
            _rootContainer.style.paddingBottom = 15f;
            
            CreateHeader();
            CreateCurrencyDisplay();
            CreateTabSystem();
            CreateContentPanels();
            
            this.Add(_rootContainer);
            
            // Show overview tab by default
            ShowTab("overview");
        }
        
        private void CreateHeader()
        {
            _headerContainer = new VisualElement();
            _headerContainer.name = "currency-header";
            _headerContainer.style.marginBottom = 20f;
            _headerContainer.style.alignItems = Align.Center;
            
            var titleLabel = new Label("💰 Financial Management");
            titleLabel.AddToClassList("panel-title");
            titleLabel.style.fontSize = 24f;
            titleLabel.style.color = new Color(0.9f, 0.7f, 0.2f, 1f);
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            
            var subtitleLabel = new Label("Track your currencies, transactions, and financial performance");
            subtitleLabel.style.fontSize = 14f;
            subtitleLabel.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            subtitleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            subtitleLabel.style.marginTop = 5f;
            
            _headerContainer.Add(titleLabel);
            _headerContainer.Add(subtitleLabel);
            _rootContainer.Add(_headerContainer);
        }
        
        private void CreateCurrencyDisplay()
        {
            _currencyDisplayContainer = new VisualElement();
            _currencyDisplayContainer.name = "currency-display";
            _currencyDisplayContainer.style.flexDirection = FlexDirection.Row;
            _currencyDisplayContainer.style.justifyContent = Justify.SpaceAround;
            _currencyDisplayContainer.style.marginBottom = 20f;
            _currencyDisplayContainer.style.paddingTop = 15f;
            _currencyDisplayContainer.style.paddingBottom = 15f;
            _currencyDisplayContainer.style.paddingLeft = 15f;
            _currencyDisplayContainer.style.paddingRight = 15f;
            _currencyDisplayContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            _currencyDisplayContainer.style.borderTopLeftRadius = 8f;
            _currencyDisplayContainer.style.borderTopRightRadius = 8f;
            _currencyDisplayContainer.style.borderBottomLeftRadius = 8f;
            _currencyDisplayContainer.style.borderBottomRightRadius = 8f;
            
            var currencyTypes = new[] { CurrencyType.Cash, CurrencyType.Credits, CurrencyType.ResearchPoints, CurrencyType.ReputationPoints };
            
            foreach (var currencyType in currencyTypes)
            {
                var currencyElement = CreateCurrencyElement(currencyType);
                _currencyDisplayContainer.Add(currencyElement);
                _currencyElements[currencyType] = currencyElement;
            }
            
            _rootContainer.Add(_currencyDisplayContainer);
        }
        
        private VisualElement CreateCurrencyElement(CurrencyType currencyType)
        {
            var container = new VisualElement();
            container.name = $"currency-{currencyType}";
            container.style.alignItems = Align.Center;
            container.style.flexGrow = 1f;
            container.style.paddingTop = 10f;
            container.style.paddingBottom = 10f;
            container.style.paddingLeft = 10f;
            container.style.paddingRight = 10f;
            
            var iconLabel = new Label(GetCurrencyIcon(currencyType));
            iconLabel.style.fontSize = 20f;
            iconLabel.style.marginBottom = 5f;
            
            var nameLabel = new Label(GetCurrencyName(currencyType));
            nameLabel.style.fontSize = 12f;
            nameLabel.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            nameLabel.style.marginBottom = 3f;
            
            var amountLabel = new Label("0");
            amountLabel.style.fontSize = 18f;
            amountLabel.style.color = Color.white;
            amountLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            amountLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _currencyAmountLabels[currencyType] = amountLabel;
            
            var changeLabel = new Label("");
            changeLabel.style.fontSize = 10f;
            changeLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            changeLabel.style.marginTop = 2f;
            _currencyChangeLabels[currencyType] = changeLabel;
            
            container.Add(iconLabel);
            container.Add(nameLabel);
            container.Add(amountLabel);
            container.Add(changeLabel);
            
            return container;
        }
        
        private void CreateTabSystem()
        {
            _tabContainer = new VisualElement();
            _tabContainer.name = "tab-container";
            _tabContainer.style.flexDirection = FlexDirection.Row;
            _tabContainer.style.marginBottom = 15f;
            _tabContainer.style.justifyContent = Justify.Center;
            
            _overviewTab = CreateTabButton("📊 Overview", "overview");
            _transactionsTab = CreateTabButton("📋 Transactions", "transactions");
            _budgetTab = CreateTabButton("📈 Budgets", "budgets");
            _analyticsTab = CreateTabButton("📉 Analytics", "analytics");
            
            _tabContainer.Add(_overviewTab);
            _tabContainer.Add(_transactionsTab);
            _tabContainer.Add(_budgetTab);
            _tabContainer.Add(_analyticsTab);
            
            _rootContainer.Add(_tabContainer);
        }
        
        private Button CreateTabButton(string text, string tabId)
        {
            var button = new Button();
            button.text = text;
            button.name = $"tab-{tabId}";
            button.AddToClassList("tab-button");
            button.style.paddingTop = 10f;
            button.style.paddingBottom = 10f;
            button.style.paddingLeft = 10f;
            button.style.paddingRight = 10f;
            button.style.marginRight = 5f;
            button.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            button.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            button.style.borderTopLeftRadius = 6f;
            button.style.borderTopRightRadius = 6f;
            button.style.borderBottomWidth = 0f;
            button.style.minWidth = 120f;
            button.clicked += () => ShowTab(tabId);
            
            return button;
        }
        
        private void CreateContentPanels()
        {
            _contentContainer = new VisualElement();
            _contentContainer.name = "content-container";
            _contentContainer.style.flexGrow = 1f;
            _contentContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            _contentContainer.style.borderTopLeftRadius = 8f;
            _contentContainer.style.borderTopRightRadius = 8f;
            _contentContainer.style.borderBottomLeftRadius = 8f;
            _contentContainer.style.borderBottomRightRadius = 8f;
            _contentContainer.style.paddingTop = 15f;
            _contentContainer.style.paddingBottom = 15f;
            _contentContainer.style.paddingLeft = 15f;
            _contentContainer.style.paddingRight = 15f;
            
            CreateOverviewContent();
            CreateTransactionsContent();
            CreateBudgetContent();
            CreateAnalyticsContent();
            
            _rootContainer.Add(_contentContainer);
        }
        
        private void CreateOverviewContent()
        {
            _overviewContent = new VisualElement();
            _overviewContent.name = "overview-content";
            _overviewContent.style.display = DisplayStyle.None;
            
            var summaryContainer = new VisualElement();
            summaryContainer.style.flexDirection = FlexDirection.Row;
            summaryContainer.style.marginBottom = 20f;
            
            // Net Worth Card
            var netWorthCard = CreateInfoCard("💎 Net Worth", "$0", "Your total financial value");
            _netWorthLabel = netWorthCard.Q<Label>("value-label");
            
            // Monthly Income Card
            var incomeCard = CreateInfoCard("📈 Monthly Income", "$0", "Average monthly earnings");
            
            // Monthly Expenses Card
            var expensesCard = CreateInfoCard("📉 Monthly Expenses", "$0", "Average monthly spending");
            
            summaryContainer.Add(netWorthCard);
            summaryContainer.Add(incomeCard);
            summaryContainer.Add(expensesCard);
            
            // Recent Transactions
            var recentLabel = new Label("Recent Transactions");
            recentLabel.style.fontSize = 16f;
            recentLabel.style.color = Color.white;
            recentLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            recentLabel.style.marginBottom = 10f;
            
            var recentTransactionsContainer = new VisualElement();
            recentTransactionsContainer.style.maxHeight = 200f;
            recentTransactionsContainer.style.backgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.8f);
            recentTransactionsContainer.style.borderTopLeftRadius = 6f;
            recentTransactionsContainer.style.borderTopRightRadius = 6f;
            recentTransactionsContainer.style.borderBottomLeftRadius = 6f;
            recentTransactionsContainer.style.borderBottomRightRadius = 6f;
            recentTransactionsContainer.style.paddingTop = 10f;
            recentTransactionsContainer.style.paddingBottom = 10f;
            recentTransactionsContainer.style.paddingLeft = 10f;
            recentTransactionsContainer.style.paddingRight = 10f;
            
            _overviewContent.Add(summaryContainer);
            _overviewContent.Add(recentLabel);
            _overviewContent.Add(recentTransactionsContainer);
            _contentContainer.Add(_overviewContent);
        }
        
        private void CreateTransactionsContent()
        {
            _transactionsContent = new VisualElement();
            _transactionsContent.name = "transactions-content";
            _transactionsContent.style.display = DisplayStyle.None;
            
            // Transaction Controls
            var controlsContainer = new VisualElement();
            controlsContainer.style.flexDirection = FlexDirection.Row;
            controlsContainer.style.marginBottom = 15f;
            controlsContainer.style.justifyContent = Justify.SpaceBetween;
            
            _transactionSearchField = new TextField();
            _transactionSearchField.name = "transaction-search";
            _transactionSearchField.textEdition.placeholder = "Search transactions...";
            _transactionSearchField.style.flexGrow = 1f;
            _transactionSearchField.style.marginRight = 10f;
            
            var filterOptions = new List<string> { "All", "Income", "Expenses", "Transfers" };
            _transactionFilterDropdown = new DropdownField("Filter:", filterOptions, 0);
            _transactionFilterDropdown.style.minWidth = 150f;
            
            controlsContainer.Add(_transactionSearchField);
            controlsContainer.Add(_transactionFilterDropdown);
            
            // Transaction List
            _transactionScrollView = new ScrollView();
            _transactionScrollView.style.flexGrow = 1f;
            _transactionScrollView.style.maxHeight = 400f;
            
            _transactionContainer = new VisualElement();
            _transactionContainer.name = "transaction-container";
            _transactionScrollView.Add(_transactionContainer);
            
            _transactionsContent.Add(controlsContainer);
            _transactionsContent.Add(_transactionScrollView);
            _contentContainer.Add(_transactionsContent);
        }
        
        private void CreateBudgetContent()
        {
            _budgetContent = new VisualElement();
            _budgetContent.name = "budget-content";
            _budgetContent.style.display = DisplayStyle.None;
            
            // Budget Controls
            var budgetHeader = new VisualElement();
            budgetHeader.style.flexDirection = FlexDirection.Row;
            budgetHeader.style.justifyContent = Justify.SpaceBetween;
            budgetHeader.style.alignItems = Align.Center;
            budgetHeader.style.marginBottom = 15f;
            
            var budgetTitle = new Label("Budget Management");
            budgetTitle.style.fontSize = 18f;
            budgetTitle.style.color = Color.white;
            budgetTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            _createBudgetButton = new Button();
            _createBudgetButton.text = "➕ Create Budget";
            _createBudgetButton.style.backgroundColor = new Color(0.2f, 0.6f, 0.2f, 1f);
            _createBudgetButton.style.color = Color.white;
            _createBudgetButton.style.borderTopLeftRadius = 4f;
            _createBudgetButton.style.borderTopRightRadius = 4f;
            _createBudgetButton.style.borderBottomLeftRadius = 4f;
            _createBudgetButton.style.borderBottomRightRadius = 4f;
            _createBudgetButton.style.paddingTop = 8f;
            _createBudgetButton.style.paddingBottom = 8f;
            _createBudgetButton.style.paddingLeft = 8f;
            _createBudgetButton.style.paddingRight = 8f;
            _createBudgetButton.clicked += OnCreateBudgetClicked;
            
            budgetHeader.Add(budgetTitle);
            budgetHeader.Add(_createBudgetButton);
            
            // Budget List
            _budgetScrollView = new ScrollView();
            _budgetScrollView.style.flexGrow = 1f;
            _budgetScrollView.style.maxHeight = 400f;
            
            _budgetContainer = new VisualElement();
            _budgetContainer.name = "budget-container";
            _budgetScrollView.Add(_budgetContainer);
            
            _budgetContent.Add(budgetHeader);
            _budgetContent.Add(_budgetScrollView);
            _contentContainer.Add(_budgetContent);
        }
        
        private void CreateAnalyticsContent()
        {
            _analyticsContent = new VisualElement();
            _analyticsContent.name = "analytics-content";
            _analyticsContent.style.display = DisplayStyle.None;
            
            // Financial Metrics
            var metricsContainer = new VisualElement();
            metricsContainer.style.flexDirection = FlexDirection.Row;
            metricsContainer.style.marginBottom = 20f;
            
            var burnRateCard = CreateInfoCard("🔥 Burn Rate", "$0/day", "Daily spending rate");
            _burnRateLabel = burnRateCard.Q<Label>("value-label");
            
            var runwayCard = CreateInfoCard("🛫 Runway", "∞ days", "Days until funds depleted");
            _runwayLabel = runwayCard.Q<Label>("value-label");
            
            var profitCard = CreateInfoCard("💹 Profit Margin", "0%", "Net income percentage");
            
            metricsContainer.Add(burnRateCard);
            metricsContainer.Add(runwayCard);
            metricsContainer.Add(profitCard);
            
            // Chart Placeholder
            _analyticsChartContainer = new VisualElement();
            _analyticsChartContainer.style.height = 200f;
            _analyticsChartContainer.style.backgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.8f);
            _analyticsChartContainer.style.borderTopLeftRadius = 6f;
            _analyticsChartContainer.style.borderTopRightRadius = 6f;
            _analyticsChartContainer.style.borderBottomLeftRadius = 6f;
            _analyticsChartContainer.style.borderBottomRightRadius = 6f;
            _analyticsChartContainer.style.alignItems = Align.Center;
            _analyticsChartContainer.style.justifyContent = Justify.Center;
            
            var chartPlaceholder = new Label("📊 Financial Charts Coming Soon");
            chartPlaceholder.style.fontSize = 16f;
            chartPlaceholder.style.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            _analyticsChartContainer.Add(chartPlaceholder);
            
            _analyticsContent.Add(metricsContainer);
            _analyticsContent.Add(_analyticsChartContainer);
            _contentContainer.Add(_analyticsContent);
        }
        
        private VisualElement CreateInfoCard(string title, string value, string description)
        {
            var card = new VisualElement();
            card.style.flexGrow = 1f;
            card.style.marginRight = 10f;
            card.style.paddingTop = 15f;
            card.style.paddingBottom = 15f;
            card.style.paddingLeft = 15f;
            card.style.paddingRight = 15f;
            card.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            card.style.borderTopLeftRadius = 8f;
            card.style.borderTopRightRadius = 8f;
            card.style.borderBottomLeftRadius = 8f;
            card.style.borderBottomRightRadius = 8f;
            card.style.alignItems = Align.Center;
            
            var titleLabel = new Label(title);
            titleLabel.style.fontSize = 14f;
            titleLabel.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            titleLabel.style.marginBottom = 5f;
            titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            
            var valueLabel = new Label(value);
            valueLabel.name = "value-label";
            valueLabel.style.fontSize = 20f;
            valueLabel.style.color = Color.white;
            valueLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            valueLabel.style.marginBottom = 5f;
            valueLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            
            var descLabel = new Label(description);
            descLabel.style.fontSize = 10f;
            descLabel.style.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            descLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            
            card.Add(titleLabel);
            card.Add(valueLabel);
            card.Add(descLabel);
            
            return card;
        }
        
        private void ShowTab(string tabId)
        {
            _activeTab = tabId;
            
            // Reset all tab appearances
            _overviewTab.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            _overviewTab.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            _transactionsTab.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            _transactionsTab.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            _budgetTab.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            _budgetTab.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            _analyticsTab.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            _analyticsTab.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            
            // Hide all content panels
            _overviewContent.style.display = DisplayStyle.None;
            _transactionsContent.style.display = DisplayStyle.None;
            _budgetContent.style.display = DisplayStyle.None;
            _analyticsContent.style.display = DisplayStyle.None;
            
            // Show active tab and content
            var activeColor = new Color(0.2f, 0.6f, 0.8f, 1f);
            
            switch (tabId)
            {
                case "overview":
                    _overviewTab.style.backgroundColor = activeColor;
                    _overviewTab.style.color = Color.white;
                    _overviewContent.style.display = DisplayStyle.Flex;
                    RefreshOverviewData();
                    break;
                case "transactions":
                    _transactionsTab.style.backgroundColor = activeColor;
                    _transactionsTab.style.color = Color.white;
                    _transactionsContent.style.display = DisplayStyle.Flex;
                    RefreshTransactionData();
                    break;
                case "budgets":
                    _budgetTab.style.backgroundColor = activeColor;
                    _budgetTab.style.color = Color.white;
                    _budgetContent.style.display = DisplayStyle.Flex;
                    RefreshBudgetData();
                    break;
                case "analytics":
                    _analyticsTab.style.backgroundColor = activeColor;
                    _analyticsTab.style.color = Color.white;
                    _analyticsContent.style.display = DisplayStyle.Flex;
                    RefreshAnalyticsData();
                    break;
            }
        }
        
        private void UpdateCurrencyDisplays()
        {
            // if (_currencyManager == null) return;
            
            // var displayData = _currencyManager.GetCurrencyDisplayData();
            
            // Placeholder data for compilation
            var displayData = new List<CurrencyDisplayData>();
            
            foreach (var data in displayData)
            {
                if (_currencyAmountLabels.TryGetValue(data.CurrencyType, out var amountLabel))
                {
                    amountLabel.text = data.FormattedAmount;
                    
                    // Update change indicator
                    if (_currencyChangeLabels.TryGetValue(data.CurrencyType, out var changeLabel))
                    {
                        if (_lastKnownAmounts.TryGetValue(data.CurrencyType, out var lastAmount))
                        {
                            float change = data.Amount - lastAmount;
                            if (Mathf.Abs(change) > 0.01f)
                            {
                                string changeText = change > 0 ? $"+{change:F2}" : $"{change:F2}";
                                changeLabel.text = changeText;
                                changeLabel.style.color = change > 0 ? _positiveColor : _negativeColor;
                            }
                        }
                        _lastKnownAmounts[data.CurrencyType] = data.Amount;
                    }
                }
            }
        }
        
        private void UpdateActiveTabContent()
        {
            switch (_activeTab)
            {
                case "overview":
                    RefreshOverviewData();
                    break;
                case "transactions":
                    RefreshTransactionData();
                    break;
                case "budgets":
                    RefreshBudgetData();
                    break;
                case "analytics":
                    RefreshAnalyticsData();
                    break;
            }
        }
        
        private void RefreshAllData()
        {
            UpdateCurrencyDisplays();
            if (_activeTab == "overview") RefreshOverviewData();
        }
        
        private void RefreshOverviewData()
        {
            // if (_currencyManager == null || _netWorthLabel == null) return;
            
            // var report = _currencyManager.GetCurrentFinancialReport();
            
            // Placeholder data for compilation
            var report = new FinancialReport { NetWorth = 0f };
            
            _netWorthLabel.text = $"${report.NetWorth:F2}";
        }
        
        private void RefreshTransactionData()
        {
            // if (_currencyManager == null) return;
            
            _transactionContainer.Clear();
            // var transactions = _currencyManager.RecentTransactions;
            
            // Placeholder data for compilation
            var transactions = new List<Transaction>();
            
            foreach (var transaction in transactions.Take(20))
            {
                var transactionElement = CreateTransactionElement(transaction);
                _transactionContainer.Add(transactionElement);
            }
        }
        
        private void RefreshBudgetData()
        {
            // if (_currencyManager == null) return;
            
            _budgetContainer.Clear();
            
            var placeholderLabel = new Label("No budgets created yet. Click 'Create Budget' to get started.");
            placeholderLabel.style.fontSize = 14f;
            placeholderLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            placeholderLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            placeholderLabel.style.marginTop = 50f;
            
            _budgetContainer.Add(placeholderLabel);
        }
        
        private void RefreshAnalyticsData()
        {
            // if (_currencyManager == null) return;
            
            // var report = _currencyManager.GetCurrentFinancialReport();
            
            // Placeholder data for compilation
            var report = new FinancialReport { NetWorth = 0f, BurnRate = 0f, RunwayDays = 0f };
            
            if (_burnRateLabel != null)
                _burnRateLabel.text = $"${report.BurnRate:F2}/day";
            
            if (_runwayLabel != null)
            {
                if (report.RunwayDays == float.MaxValue)
                    _runwayLabel.text = "∞ days";
                else
                    _runwayLabel.text = $"{report.RunwayDays:F0} days";
            }
        }
        
        private VisualElement CreateTransactionElement(Transaction transaction)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.justifyContent = Justify.SpaceBetween;
            container.style.alignItems = Align.Center;
            container.style.paddingTop = 8f;
            container.style.paddingBottom = 8f;
            container.style.paddingLeft = 8f;
            container.style.paddingRight = 8f;
            container.style.marginBottom = 5f;
            container.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            container.style.borderTopLeftRadius = 4f;
            container.style.borderTopRightRadius = 4f;
            container.style.borderBottomLeftRadius = 4f;
            container.style.borderBottomRightRadius = 4f;
            
            var leftContainer = new VisualElement();
            leftContainer.style.flexGrow = 1f;
            
            var descriptionLabel = new Label(transaction.Description);
            descriptionLabel.style.fontSize = 12f;
            descriptionLabel.style.color = Color.white;
            
            var categoryLabel = new Label(transaction.Category.ToString());
            categoryLabel.style.fontSize = 10f;
            categoryLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            
            leftContainer.Add(descriptionLabel);
            leftContainer.Add(categoryLabel);
            
            var rightContainer = new VisualElement();
            rightContainer.style.alignItems = Align.FlexEnd;
            
            var amountLabel = new Label($"${transaction.Amount:F2}");
            amountLabel.style.fontSize = 12f;
            amountLabel.style.color = transaction.Type == TransactionType.Income ? _positiveColor : _negativeColor;
            amountLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            var timeLabel = new Label(transaction.Timestamp.ToString("HH:mm"));
            timeLabel.style.fontSize = 10f;
            timeLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            
            rightContainer.Add(amountLabel);
            rightContainer.Add(timeLabel);
            
            container.Add(leftContainer);
            container.Add(rightContainer);
            
            return container;
        }
        
        private void OnCreateBudgetClicked()
        {
            // Simplified budget creation - in full implementation would show a modal dialog
            // _currencyManager.CreateBudget("New Budget", 1000f);
            RefreshBudgetData();
        }
        
        private string GetCurrencyIcon(CurrencyType currencyType)
        {
            return currencyType switch
            {
                CurrencyType.Cash => "💰",
                CurrencyType.Credits => "🪙",
                CurrencyType.ResearchPoints => "🔬",
                CurrencyType.ReputationPoints => "⭐",
                _ => "💎"
            };
        }
        
        private string GetCurrencyName(CurrencyType currencyType)
        {
            return currencyType switch
            {
                CurrencyType.Cash => "Cash",
                CurrencyType.Credits => "Credits",
                CurrencyType.ResearchPoints => "Research",
                CurrencyType.ReputationPoints => "Reputation",
                _ => "Unknown"
            };
        }
        
        private void SubscribeToEvents()
        {
            // if (_currencyManager != null)
            // {
                // _currencyManager.OnCurrencyChanged += OnCurrencyChanged;
                // _currencyManager.OnTransactionCompleted += OnTransactionCompleted;
                // _currencyManager.OnFinancialMilestone += OnFinancialMilestone;
            // }
        }
        
        private void OnCurrencyChanged(CurrencyType currencyType, float oldAmount, float newAmount)
        {
            UpdateCurrencyDisplays();
        }
        
        private void OnTransactionCompleted(Transaction transaction)
        {
            if (_activeTab == "transactions")
            {
                RefreshTransactionData();
            }
        }
        
        private void OnFinancialMilestone(float milestoneAmount)
        {
            LogInfo($"Financial milestone reached: ${milestoneAmount:F2}");
        }
        
        public void OnBeforeHide()
        {
            // Unsubscribe from events
            // if (_currencyManager != null)
            // {
                // _currencyManager.OnCurrencyChanged -= OnCurrencyChanged;
                // _currencyManager.OnTransactionCompleted -= OnTransactionCompleted;
                // _currencyManager.OnFinancialMilestone -= OnFinancialMilestone;
            // }
            
            // Panel cleanup logic
        }
        
        private void LogInfo(string message)
        {
            Debug.Log($"[CurrencyPanel] {message}");
        }
    }
    
    // Temporary data structures for compilation
    [System.Serializable]
    public class CurrencyDisplayData
    {
        public CurrencyType CurrencyType;
        public float Amount;
        public float Change;
        public string DisplayName;
        public string FormattedAmount => $"${Amount:F2}";
    }
    
    [System.Serializable]
    public class Transaction
    {
        public string Description;
        public TransactionCategory Category;
        public TransactionType Type;
        public float Amount;
        public System.DateTime Timestamp;
    }
    
    public enum TransactionCategory
    {
        Equipment,
        Supplies,
        Labor,
        Utilities,
        Sales,
        Other
    }
    
    public enum TransactionType
    {
        Income,
        Expense
    }
}