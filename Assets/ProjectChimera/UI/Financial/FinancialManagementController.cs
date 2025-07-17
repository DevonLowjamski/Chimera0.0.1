using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
// using ProjectChimera.Systems.Environment;
// using ProjectChimera.Systems.Economy;
using ProjectChimera.Data.Economy;
using ProjectChimera.Data.UI;

namespace ProjectChimera.UI.Financial
{
    /// <summary>
    /// Financial Management UI Controller for Project Chimera.
    /// Provides comprehensive economic management including trading, investments, and market analysis.
    /// Features game-like interfaces for making financial decisions and tracking portfolio performance.
    /// </summary>
    public class FinancialManagementController : MonoBehaviour
    {
        [Header("Financial UI Configuration")]
        [SerializeField] private UIDocument _financialDocument;
        [SerializeField] private FinancialUISettings _uiSettings;
        [SerializeField] private bool _enableRealTimeUpdates = true;
        [SerializeField] private float _updateInterval = 3f;
        
        [Header("Trading Configuration")]
        [SerializeField] private int _maxTradeHistory = 50;
        [SerializeField] private float _priceUpdateInterval = 1f;
        [SerializeField] private bool _enablePriceAlerts = true;
        
        [Header("Investment Configuration")]
        [SerializeField] private float _portfolioRefreshRate = 5f;
        [SerializeField] private bool _enableInvestmentAlerts = true;
        [SerializeField] private Vector2 _investmentLimits = new Vector2(100f, 50000f);
        
        [Header("Audio Feedback")]
        [SerializeField] private AudioClip _tradeCompleteSound;
        [SerializeField] private AudioClip _investmentSound;
        [SerializeField] private AudioClip _alertSound;
        [SerializeField] private AudioClip _profitSound;
        [SerializeField] private AudioSource _audioSource;
        
        // System references
        // private MarketManager _marketManager;
        // private TradingManager _tradingManager;
        // private InvestmentManager _investmentManager;
        
        // UI Elements - Main Navigation
        private VisualElement _rootElement;
        private Button _overviewTabButton;
        private Button _tradingTabButton;
        private Button _investmentsTabButton;
        private Button _marketTabButton;
        private Button _reportsTabButton;
        
        // Tab Panels
        private VisualElement _overviewPanel;
        private VisualElement _tradingPanel;
        private VisualElement _investmentsPanel;
        private VisualElement _marketPanel;
        private VisualElement _reportsPanel;
        
        // Overview Panel Elements
        private Label _netWorthDisplay;
        private Label _cashBalanceDisplay;
        private Label _portfolioValueDisplay;
        private Label _dailyPnLDisplay;
        private ProgressBar _profitMarginBar;
        private VisualElement _quickStatsContainer;
        private VisualElement _recentTransactionsList;
        
        // Trading Panel Elements
        private DropdownField _tradingPairSelector;
        private Label _currentPriceDisplay;
        private Label _priceChangeDisplay;
        private FloatField _tradeAmountField;
        private FloatField _tradePriceField;
        private Button _buyButton;
        private Button _sellButton;
        private Button _marketBuyButton;
        private Button _marketSellButton;
        private VisualElement _orderBookContainer;
        private VisualElement _tradeHistoryContainer;
        private VisualElement _openOrdersContainer;
        
        // Investment Panel Elements
        private VisualElement _portfolioSummary;
        private VisualElement _investmentOpportunities;
        private VisualElement _activeInvestmentsContainer;
        private Button _rebalanceButton;
        private Button _addInvestmentButton;
        private Slider _riskToleranceSlider;
        private Label _expectedReturnLabel;
        
        // Market Panel Elements
        private VisualElement _marketOverview;
        private VisualElement _priceChartContainer;
        private VisualElement _marketTrendsContainer;
        private VisualElement _newsContainer;
        private DropdownField _timeframeSelector;
        
        // Reports Panel Elements
        private VisualElement _performanceCharts;
        private VisualElement _profitLossStatement;
        private VisualElement _taxReporting;
        private Button _generateReportButton;
        private Button _exportDataButton;
        
        // Data and State
        private Dictionary<string, MarketData> _marketPrices = new Dictionary<string, MarketData>();
        private List<TradeTransaction> _tradeHistory = new List<TradeTransaction>();
        private List<Investment> _activeInvestments = new List<Investment>();
        private Dictionary<string, PriceAlert> _priceAlerts = new Dictionary<string, PriceAlert>();
        private FinancialPortfolio _portfolio = new FinancialPortfolio();
        private string _currentTab = "overview";
        private string _selectedTradingPair = "CANNABIS/USD";
        private float _lastUpdateTime;
        private bool _isUpdating = false;
        
        // Events
        public System.Action<TradeTransaction> OnTradeExecuted;
        public System.Action<Investment> OnInvestmentMade;
        public System.Action<string> OnTabChanged;
        public System.Action<PriceAlert> OnPriceAlert;
        public System.Action<TradingFinancialReport> OnReportGenerated;
        
        private void Start()
        {
            InitializeController();
            InitializeSystemReferences();
            SetupUIElements();
            SetupEventHandlers();
            LoadFinancialData();
            
            if (_enableRealTimeUpdates)
            {
                InvokeRepeating(nameof(UpdateFinancialData), 1f, _updateInterval);
                InvokeRepeating(nameof(UpdateMarketPrices), 0.5f, _priceUpdateInterval);
            }
        }
        
        private void InitializeController()
        {
            if (_financialDocument == null)
            {
                Debug.LogError("Financial UI Document not assigned!");
                return;
            }
            
            _rootElement = _financialDocument.rootVisualElement;
            _lastUpdateTime = Time.time;
            
            // Initialize portfolio
            _portfolio = new FinancialPortfolio
            {
                CashBalance = 25000f,
                TotalValue = 125000f,
                ProfitLoss = 8500f,
                DailyChange = 2.3f
            };
            
            Debug.Log("Financial Management Controller initialized");
        }
        
        private void InitializeSystemReferences()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogWarning("GameManager not found - using simulation mode");
                return;
            }
            
            // _marketManager = gameManager.GetManager<MarketManager>();
            // _tradingManager = gameManager.GetManager<TradingManager>();
            // _investmentManager = gameManager.GetManager<InvestmentManager>();
            
            Debug.Log("Financial Management connected to economic systems");
        }
        
        private void SetupUIElements()
        {
            // Main navigation tabs
            _overviewTabButton = _rootElement.Q<Button>("overview-tab");
            _tradingTabButton = _rootElement.Q<Button>("trading-tab");
            _investmentsTabButton = _rootElement.Q<Button>("investments-tab");
            _marketTabButton = _rootElement.Q<Button>("market-tab");
            _reportsTabButton = _rootElement.Q<Button>("reports-tab");
            
            // Tab panels
            _overviewPanel = _rootElement.Q<VisualElement>("overview-panel");
            _tradingPanel = _rootElement.Q<VisualElement>("trading-panel");
            _investmentsPanel = _rootElement.Q<VisualElement>("investments-panel");
            _marketPanel = _rootElement.Q<VisualElement>("market-panel");
            _reportsPanel = _rootElement.Q<VisualElement>("reports-panel");
            
            // Overview elements
            _netWorthDisplay = _rootElement.Q<Label>("net-worth-display");
            _cashBalanceDisplay = _rootElement.Q<Label>("cash-balance-display");
            _portfolioValueDisplay = _rootElement.Q<Label>("portfolio-value-display");
            _dailyPnLDisplay = _rootElement.Q<Label>("daily-pnl-display");
            _profitMarginBar = _rootElement.Q<ProgressBar>("profit-margin-bar");
            _quickStatsContainer = _rootElement.Q<VisualElement>("quick-stats-container");
            _recentTransactionsList = _rootElement.Q<VisualElement>("recent-transactions-list");
            
            // Trading elements
            _tradingPairSelector = _rootElement.Q<DropdownField>("trading-pair-selector");
            _currentPriceDisplay = _rootElement.Q<Label>("current-price-display");
            _priceChangeDisplay = _rootElement.Q<Label>("price-change-display");
            _tradeAmountField = _rootElement.Q<FloatField>("trade-amount-field");
            _tradePriceField = _rootElement.Q<FloatField>("trade-price-field");
            _buyButton = _rootElement.Q<Button>("buy-button");
            _sellButton = _rootElement.Q<Button>("sell-button");
            _marketBuyButton = _rootElement.Q<Button>("market-buy-button");
            _marketSellButton = _rootElement.Q<Button>("market-sell-button");
            _orderBookContainer = _rootElement.Q<VisualElement>("order-book-container");
            _tradeHistoryContainer = _rootElement.Q<VisualElement>("trade-history-container");
            _openOrdersContainer = _rootElement.Q<VisualElement>("open-orders-container");
            
            // Investment elements
            _portfolioSummary = _rootElement.Q<VisualElement>("portfolio-summary");
            _investmentOpportunities = _rootElement.Q<VisualElement>("investment-opportunities");
            _activeInvestmentsContainer = _rootElement.Q<VisualElement>("active-investments");
            _rebalanceButton = _rootElement.Q<Button>("rebalance-button");
            _addInvestmentButton = _rootElement.Q<Button>("add-investment-button");
            _riskToleranceSlider = _rootElement.Q<Slider>("risk-tolerance-slider");
            _expectedReturnLabel = _rootElement.Q<Label>("expected-return-label");
            
            // Market elements
            _marketOverview = _rootElement.Q<VisualElement>("market-overview");
            _priceChartContainer = _rootElement.Q<VisualElement>("price-chart-container");
            _marketTrendsContainer = _rootElement.Q<VisualElement>("market-trends-container");
            _newsContainer = _rootElement.Q<VisualElement>("news-container");
            _timeframeSelector = _rootElement.Q<DropdownField>("timeframe-selector");
            
            // Reports elements
            _performanceCharts = _rootElement.Q<VisualElement>("performance-charts");
            _profitLossStatement = _rootElement.Q<VisualElement>("profit-loss-statement");
            _taxReporting = _rootElement.Q<VisualElement>("tax-reporting");
            _generateReportButton = _rootElement.Q<Button>("generate-report-button");
            _exportDataButton = _rootElement.Q<Button>("export-data-button");
            
            SetupTradingPairs();
            SetupTimeframes();
            SetupInitialState();
        }
        
        private void SetupTradingPairs()
        {
            if (_tradingPairSelector != null)
            {
                _tradingPairSelector.choices = new List<string>
                {
                    "CANNABIS/USD", "CBD/USD", "THC/USD", "HEMP/USD", 
                    "FLOWER/USD", "CONCENTRATE/USD", "EDIBLES/USD"
                };
                _tradingPairSelector.value = _selectedTradingPair;
            }
        }
        
        private void SetupTimeframes()
        {
            if (_timeframeSelector != null)
            {
                _timeframeSelector.choices = new List<string>
                {
                    "1H", "4H", "1D", "1W", "1M", "3M", "1Y"
                };
                _timeframeSelector.value = "1D";
            }
        }
        
        private void SetupInitialState()
        {
            // Show overview panel by default
            ShowPanel("overview");
            
            // Initialize risk tolerance
            if (_riskToleranceSlider != null)
            {
                _riskToleranceSlider.value = 0.5f;
            }
        }
        
        private void SetupEventHandlers()
        {
            // Tab navigation
            _overviewTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("overview"));
            _tradingTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("trading"));
            _investmentsTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("investments"));
            _marketTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("market"));
            _reportsTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("reports"));
            
            // Trading controls
            _tradingPairSelector?.RegisterValueChangedCallback(evt => OnTradingPairChanged(evt.newValue));
            _buyButton?.RegisterCallback<ClickEvent>(evt => ExecuteTrade(TradeType.Buy, OrderType.Limit));
            _sellButton?.RegisterCallback<ClickEvent>(evt => ExecuteTrade(TradeType.Sell, OrderType.Limit));
            _marketBuyButton?.RegisterCallback<ClickEvent>(evt => ExecuteTrade(TradeType.Buy, OrderType.Market));
            _marketSellButton?.RegisterCallback<ClickEvent>(evt => ExecuteTrade(TradeType.Sell, OrderType.Market));
            
            // Investment controls
            _rebalanceButton?.RegisterCallback<ClickEvent>(evt => RebalancePortfolio());
            _addInvestmentButton?.RegisterCallback<ClickEvent>(evt => ShowInvestmentDialog());
            _riskToleranceSlider?.RegisterValueChangedCallback(evt => UpdateRiskTolerance(evt.newValue));
            
            // Market controls
            _timeframeSelector?.RegisterValueChangedCallback(evt => UpdatePriceChart(evt.newValue));
            
            // Report controls
            _generateReportButton?.RegisterCallback<ClickEvent>(evt => GenerateFinancialReport());
            _exportDataButton?.RegisterCallback<ClickEvent>(evt => ExportFinancialData());
        }
        
        #region Panel Management
        
        private void ShowPanel(string panelName)
        {
            // Hide all panels
            _overviewPanel?.AddToClassList("hidden");
            _tradingPanel?.AddToClassList("hidden");
            _investmentsPanel?.AddToClassList("hidden");
            _marketPanel?.AddToClassList("hidden");
            _reportsPanel?.AddToClassList("hidden");
            
            // Remove active state from all tabs
            _overviewTabButton?.RemoveFromClassList("tab-active");
            _tradingTabButton?.RemoveFromClassList("tab-active");
            _investmentsTabButton?.RemoveFromClassList("tab-active");
            _marketTabButton?.RemoveFromClassList("tab-active");
            _reportsTabButton?.RemoveFromClassList("tab-active");
            
            // Show selected panel and activate tab
            switch (panelName)
            {
                case "overview":
                    _overviewPanel?.RemoveFromClassList("hidden");
                    _overviewTabButton?.AddToClassList("tab-active");
                    RefreshOverviewData();
                    break;
                case "trading":
                    _tradingPanel?.RemoveFromClassList("hidden");
                    _tradingTabButton?.AddToClassList("tab-active");
                    RefreshTradingData();
                    break;
                case "investments":
                    _investmentsPanel?.RemoveFromClassList("hidden");
                    _investmentsTabButton?.AddToClassList("tab-active");
                    RefreshInvestmentData();
                    break;
                case "market":
                    _marketPanel?.RemoveFromClassList("hidden");
                    _marketTabButton?.AddToClassList("tab-active");
                    RefreshMarketData();
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
        
        #region Trading Operations
        
        private void OnTradingPairChanged(string newPair)
        {
            _selectedTradingPair = newPair;
            UpdateCurrentPrice();
            RefreshOrderBook();
            UpdatePriceChart("1D");
        }
        
        private void ExecuteTrade(TradeType tradeType, OrderType orderType)
        {
            if (_isUpdating) return;
            
            float amount = _tradeAmountField?.value ?? 0f;
            float price = orderType == OrderType.Market ? GetCurrentMarketPrice() : (_tradePriceField?.value ?? 0f);
            
            if (amount <= 0)
            {
                Debug.LogWarning("Invalid trade amount");
                return;
            }
            
            var trade = new TradeTransaction
            {
                TradeId = Guid.NewGuid().ToString(),
                TradingPair = _selectedTradingPair,
                TradeType = tradeType,
                OrderType = orderType,
                Amount = amount,
                Price = price,
                Timestamp = DateTime.Now,
                Status = TradeStatus.Executed
            };
            
            // Validate trade
            if (!ValidateTrade(trade))
            {
                Debug.LogWarning("Trade validation failed");
                return;
            }
            
            // Execute trade
            // if (_tradingManager != null)
            // Execute trade (placeholder implementation)
            bool success = true; // TODO: Implement actual trading logic
            if (!success)
            {
                Debug.LogWarning("Trade execution failed");
                return;
            }
            
            // Update portfolio
            UpdatePortfolioAfterTrade(trade);
            
            // Add to history
            _tradeHistory.Add(trade);
            if (_tradeHistory.Count > _maxTradeHistory)
            {
                _tradeHistory.RemoveAt(0);
            }
            
            // Refresh UI
            RefreshTradingData();
            RefreshOverviewData();
            
            // Play sound and trigger event
            PlaySound(_tradeCompleteSound);
            OnTradeExecuted?.Invoke(trade);
            
            Debug.Log($"Executed {tradeType} order: {amount} {_selectedTradingPair} @ {price:C}");
        }
        
        private bool ValidateTrade(TradeTransaction trade)
        {
            float totalCost = trade.Amount * trade.Price;
            
            if (trade.TradeType == TradeType.Buy)
            {
                return _portfolio.CashBalance >= totalCost;
            }
            // else
            // {
                // Check if we have enough of the asset to sell
                return true; // Simplified validation
            // }
        }
        
        private void UpdatePortfolioAfterTrade(TradeTransaction trade)
        {
            float totalValue = trade.Amount * trade.Price;
            
            if (trade.TradeType == TradeType.Buy)
            {
                _portfolio.CashBalance -= totalValue;
                // Add to holdings (simplified)
            }
            // else
            // {
                _portfolio.CashBalance += totalValue;
                // Remove from holdings (simplified)
            // }
            
            _portfolio.TotalValue = _portfolio.CashBalance + CalculateHoldingsValue();
        }
        
        #endregion
        
        #region Investment Management
        
        private void ShowInvestmentDialog()
        {
            // Would show modal dialog for creating new investment
            Debug.Log("Show investment creation dialog");
        }
        
        private void RebalancePortfolio()
        {
            // if (_investmentManager == null) return;
            
            // TODO: Implement portfolio rebalancing
            var rebalanceResult = new { Success = true }; // Placeholder
            
            if (rebalanceResult != null && rebalanceResult.Success)
            {
                RefreshInvestmentData();
                RefreshOverviewData();
                PlaySound(_investmentSound);
                Debug.Log("Portfolio rebalanced successfully");
            }
            // else
            // {
                Debug.LogWarning("Portfolio rebalancing failed");
            // }
        }
        
        private void UpdateRiskTolerance(float riskLevel)
        {
            if (_expectedReturnLabel != null)
            {
                float expectedReturn = CalculateExpectedReturn(riskLevel);
                _expectedReturnLabel.text = $"Expected Return: {expectedReturn:P}";
            }
            
            RefreshInvestmentOpportunities();
        }
        
        #endregion
        
        #region Data Updates
        
        [ContextMenu("Update Financial Data")]
        public void UpdateFinancialData()
        {
            if (_isUpdating) return;
            
            _isUpdating = true;
            
            // Update portfolio values
            UpdatePortfolioMetrics();
            
            // Update current tab data
            switch (_currentTab)
            {
                case "overview":
                    RefreshOverviewData();
                    break;
                case "trading":
                    RefreshTradingData();
                    break;
                case "investments":
                    RefreshInvestmentData();
                    break;
                case "market":
                    RefreshMarketData();
                    break;
                case "reports":
                    RefreshReportsData();
                    break;
            }
            
            _lastUpdateTime = Time.time;
            _isUpdating = false;
        }
        
        private void UpdateMarketPrices()
        {
            // Simulate market price updates
            foreach (var pair in _tradingPairSelector?.choices ?? new List<string>())
            {
                if (!_marketPrices.ContainsKey(pair))
                {
                    _marketPrices[pair] = new MarketData
                    {
                        Symbol = pair,
                        Price = UnityEngine.Random.Range(50f, 500f),
                        Change24h = UnityEngine.Random.Range(-10f, 10f),
                        Volume = UnityEngine.Random.Range(1000f, 100000f)
                    };
                }
                // else
                // {
                    var data = _marketPrices[pair];
                    float priceChange = UnityEngine.Random.Range(-0.02f, 0.02f);
                    data.Price += data.Price * priceChange;
                    data.Change24h = priceChange * 100f;
                    data.LastUpdate = DateTime.Now;
                // }
            }
            
            // Update current price display
            if (_currentTab == "trading")
            {
                UpdateCurrentPrice();
            }
            
            // Check price alerts
            CheckPriceAlerts();
        }
        
        private void UpdatePortfolioMetrics()
        {
            // if (_marketManager != null)
            // {
                // TODO: Implement portfolio metrics
                var metrics = new { TotalValue = 100000f, ProfitLoss = 5000f, DailyChange = 2.5f }; // Placeholder
                if (metrics != null)
                {
                    _portfolio.TotalValue = metrics.TotalValue;
                    _portfolio.ProfitLoss = metrics.ProfitLoss;
                    _portfolio.DailyChange = metrics.DailyChange;
                }
            // }
            // else
            // {
                // Simulate portfolio changes
                float change = UnityEngine.Random.Range(-0.005f, 0.005f);
                _portfolio.TotalValue += _portfolio.TotalValue * change;
                _portfolio.DailyChange = change * 100f;
                _portfolio.ProfitLoss = _portfolio.TotalValue - 125000f; // Starting value
            // }
        }
        
        #endregion
        
        #region UI Refresh Methods
        
        private void RefreshOverviewData()
        {
            if (_netWorthDisplay != null)
                _netWorthDisplay.text = $"${_portfolio.TotalValue:N0}";
            
            if (_cashBalanceDisplay != null)
                _cashBalanceDisplay.text = $"${_portfolio.CashBalance:N0}";
            
            if (_portfolioValueDisplay != null)
                _portfolioValueDisplay.text = $"${CalculateHoldingsValue():N0}";
            
            if (_dailyPnLDisplay != null)
            {
                _dailyPnLDisplay.text = $"{(_portfolio.DailyChange >= 0 ? "+" : "")}{_portfolio.DailyChange:F2}%";
                _dailyPnLDisplay.RemoveFromClassList("positive");
                _dailyPnLDisplay.RemoveFromClassList("negative");
                _dailyPnLDisplay.AddToClassList(_portfolio.DailyChange >= 0 ? "positive" : "negative");
            }
            
            if (_profitMarginBar != null)
            {
                float profitMargin = (_portfolio.ProfitLoss / 125000f) + 0.5f; // Normalize around 0.5
                _profitMarginBar.value = Mathf.Clamp01(profitMargin);
            }
            
            UpdateQuickStats();
            UpdateRecentTransactions();
        }
        
        private void RefreshTradingData()
        {
            UpdateCurrentPrice();
            UpdateOrderBook();
            UpdateTradeHistory();
            UpdateOpenOrders();
        }
        
        private void RefreshInvestmentData()
        {
            UpdatePortfolioSummary();
            UpdateInvestmentOpportunities();
            UpdateActiveInvestments();
        }
        
        private void RefreshMarketData()
        {
            UpdateMarketOverview();
            UpdatePriceChart(_timeframeSelector?.value ?? "1D");
            UpdateMarketTrends();
            UpdateMarketNews();
        }
        
        private void RefreshReportsData()
        {
            UpdatePerformanceCharts();
            UpdateProfitLossStatement();
            UpdateTaxReporting();
        }
        
        #endregion
        
        #region Helper Methods
        
        private void LoadFinancialData()
        {
            // Load saved financial data - simplified approach to avoid dynamic casting issues
            Debug.Log("Financial data loading initiated");
            
            // For now, initialize with default values
            // In a full implementation, this would load from persistent storage
            if (_portfolio.TotalValue == 0f)
            {
                _portfolio.TotalValue = 125000f; // Starting portfolio value
                _portfolio.CashBalance = 25000f;
                _portfolio.ProfitLoss = 0f;
                _portfolio.DailyChange = 0f;
                _portfolio.LastUpdated = DateTime.Now;
            }
            
            Debug.Log("Financial data loaded");
        }
        
        private float GetCurrentMarketPrice()
        {
            if (_marketPrices.ContainsKey(_selectedTradingPair))
            {
                return _marketPrices[_selectedTradingPair].Price;
            }
            return 100f; // Default price
        }
        
        private void UpdateCurrentPrice()
        {
            if (_currentPriceDisplay != null && _priceChangeDisplay != null)
            {
                var marketData = _marketPrices.GetValueOrDefault(_selectedTradingPair);
                if (marketData != null)
                {
                    _currentPriceDisplay.text = $"${marketData.Price:F2}";
                    _priceChangeDisplay.text = $"{(marketData.Change24h >= 0 ? "+" : "")}{marketData.Change24h:F2}%";
                    
                    _priceChangeDisplay.RemoveFromClassList("positive");
                    _priceChangeDisplay.RemoveFromClassList("negative");
                    _priceChangeDisplay.AddToClassList(marketData.Change24h >= 0 ? "positive" : "negative");
                }
            }
        }
        
        private float CalculateHoldingsValue()
        {
            // Simplified calculation - would include all asset holdings
            return _portfolio.TotalValue - _portfolio.CashBalance;
        }
        
        private float CalculateExpectedReturn(float riskLevel)
        {
            // Risk vs return calculation
            return 0.03f + (riskLevel * 0.15f); // 3% to 18% expected return
        }
        
        private void CheckPriceAlerts()
        {
            if (!_enablePriceAlerts) return;
            
            foreach (var alert in _priceAlerts.Values)
            {
                var currentPrice = GetCurrentMarketPrice();
                bool triggered = false;
                
                if (alert.AlertType == PriceAlertType.Above && currentPrice >= alert.TargetPrice)
                    triggered = true;
                else if (alert.AlertType == PriceAlertType.Below && currentPrice <= alert.TargetPrice)
                    triggered = true;
                
                if (triggered && !alert.IsTriggered)
                {
                    alert.IsTriggered = true;
                    PlaySound(_alertSound);
                    OnPriceAlert?.Invoke(alert);
                    Debug.Log($"Price alert triggered: {alert.Symbol} {alert.AlertType} ${alert.TargetPrice}");
                }
            }
        }
        
        private void UpdateQuickStats()
        {
            // Update quick statistics in overview
        }
        
        private void UpdateRecentTransactions()
        {
            if (_recentTransactionsList == null) return;
            
            _recentTransactionsList.Clear();
            
            var recentTrades = _tradeHistory.TakeLast(5);
            foreach (var trade in recentTrades)
            {
                var transactionElement = CreateTransactionElement(trade);
                _recentTransactionsList.Add(transactionElement);
            }
        }
        
        private void UpdateOrderBook()
        {
            // Update order book display
            Debug.Log("Order book updated");
        }
        
        private void RefreshOrderBook()
        {
            if (_orderBookContainer == null) return;
            
            // Clear existing order book
            _orderBookContainer.Clear();
            
            // Add buy orders
            var buyOrdersHeader = new Label("Buy Orders");
            buyOrdersHeader.AddToClassList("order-book-header");
            _orderBookContainer.Add(buyOrdersHeader);
            
            for (int i = 0; i < 5; i++)
            {
                var orderElement = new VisualElement();
                orderElement.AddToClassList("order-book-item");
                orderElement.AddToClassList("buy-order");
                
                var priceLabel = new Label($"${UnityEngine.Random.Range(95f, 100f):F2}");
                var amountLabel = new Label($"{UnityEngine.Random.Range(1f, 50f):F1}");
                
                orderElement.Add(priceLabel);
                orderElement.Add(amountLabel);
                _orderBookContainer.Add(orderElement);
            }
            
            // Add sell orders
            var sellOrdersHeader = new Label("Sell Orders");
            sellOrdersHeader.AddToClassList("order-book-header");
            _orderBookContainer.Add(sellOrdersHeader);
            
            for (int i = 0; i < 5; i++)
            {
                var orderElement = new VisualElement();
                orderElement.AddToClassList("order-book-item");
                orderElement.AddToClassList("sell-order");
                
                var priceLabel = new Label($"${UnityEngine.Random.Range(100f, 105f):F2}");
                var amountLabel = new Label($"{UnityEngine.Random.Range(1f, 50f):F1}");
                
                orderElement.Add(priceLabel);
                orderElement.Add(amountLabel);
                _orderBookContainer.Add(orderElement);
            }
        }
        
        private void UpdateTradeHistory()
        {
            // Update trade history display
        }
        
        private void UpdateOpenOrders()
        {
            // Update open orders display
        }
        
        private void UpdatePortfolioSummary()
        {
            // Update portfolio allocation summary
        }
        
        private void UpdateInvestmentOpportunities()
        {
            // Update available investment opportunities
        }
        
        private void RefreshInvestmentOpportunities()
        {
            UpdateInvestmentOpportunities();
            Debug.Log("Investment opportunities refreshed");
        }
        
        private void UpdateActiveInvestments()
        {
            // Update active investments display
        }
        
        private void UpdateMarketOverview()
        {
            // Update market overview statistics
        }
        
        private void UpdatePriceChart(string timeframe)
        {
            // Update price chart for selected timeframe
            Debug.Log($"Updating price chart for {_selectedTradingPair} - {timeframe}");
        }
        
        private void UpdateMarketTrends()
        {
            // Update market trends analysis
        }
        
        private void UpdateMarketNews()
        {
            // Update market news feed
        }
        
        private void UpdatePerformanceCharts()
        {
            // Update performance analytics charts
        }
        
        private void UpdateProfitLossStatement()
        {
            // Update P&L statement
        }
        
        private void UpdateTaxReporting()
        {
            // Update tax reporting information
        }
        
        private VisualElement CreateTransactionElement(TradeTransaction trade)
        {
            var element = new VisualElement();
            element.AddToClassList("transaction-item");
            
            var typeLabel = new Label(trade.TradeType.ToString());
            typeLabel.AddToClassList("transaction-type");
            typeLabel.AddToClassList(trade.TradeType == TradeType.Buy ? "buy-type" : "sell-type");
            
            var pairLabel = new Label(trade.TradingPair);
            pairLabel.AddToClassList("transaction-pair");
            
            var amountLabel = new Label($"{trade.Amount:F4}");
            amountLabel.AddToClassList("transaction-amount");
            
            var priceLabel = new Label($"${trade.Price:F2}");
            priceLabel.AddToClassList("transaction-price");
            
            var timeLabel = new Label(trade.Timestamp.ToString("HH:mm:ss"));
            timeLabel.AddToClassList("transaction-time");
            
            element.Add(typeLabel);
            element.Add(pairLabel);
            element.Add(amountLabel);
            element.Add(priceLabel);
            element.Add(timeLabel);
            
            return element;
        }
        
        private void GenerateFinancialReport()
        {
            var report = new TradingFinancialReport
            {
                ReportId = Guid.NewGuid().ToString(),
                GeneratedAt = DateTime.Now,
                Period = "Monthly",
                TotalValue = _portfolio.TotalValue,
                ProfitLoss = _portfolio.ProfitLoss,
                TradeCount = _tradeHistory.Count,
                InvestmentCount = _activeInvestments.Count
            };
            
            OnReportGenerated?.Invoke(report);
            Debug.Log("Financial report generated");
        }
        
        private void ExportFinancialData()
        {
            // Export financial data to file
            Debug.Log("Exporting financial data");
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
    
    // Supporting data structures
    [System.Serializable]
    public class FinancialPortfolio
    {
        public float CashBalance;
        public float TotalValue;
        public float ProfitLoss;
        public float DailyChange;
        public DateTime LastUpdated;
    }
    
    [System.Serializable]
    public class TradeTransaction
    {
        public string TradeId;
        public string TradingPair;
        public TradeType TradeType;
        public OrderType OrderType;
        public float Amount;
        public float Price;
        public DateTime Timestamp;
        public TradeStatus Status;
    }
    
    [System.Serializable]
    public class MarketData
    {
        public string Symbol;
        public float Price;
        public float Change24h;
        public float Volume;
        public DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class Investment
    {
        public string InvestmentId;
        public string Name;
        public float Amount;
        public float CurrentValue;
        public float ReturnRate;
        public DateTime InvestmentDate;
        public InvestmentType Type;
    }
    
    [System.Serializable]
    public class PriceAlert
    {
        public string AlertId;
        public string Symbol;
        public float TargetPrice;
        public PriceAlertType AlertType;
        public bool IsTriggered;
        public DateTime CreatedAt;
    }
    
    [System.Serializable]
    public class TradingFinancialReport
    {
        public string ReportId;
        public DateTime GeneratedAt;
        public string Period;
        public float TotalValue;
        public float ProfitLoss;
        public int TradeCount;
        public int InvestmentCount;
    }
    
    public enum TradeType
    {
        Buy,
        Sell
    }
    
    public enum OrderType
    {
        Market,
        Limit,
        Stop,
        StopLimit
    }
    
    public enum TradeStatus
    {
        Pending,
        Executed,
        Cancelled,
        Failed
    }
    
    public enum InvestmentType
    {
        Stocks,
        Bonds,
        Cannabis,
        Equipment,
        RealEstate,
        Cryptocurrency
    }
    
    public enum PriceAlertType
    {
        Above,
        Below
    }
    
    [System.Serializable]
    public class FinancialUISettings
    {
        public bool EnableAdvancedTrading = true;
        public bool EnableInvestments = true;
        public bool EnablePriceAlerts = true;
        public float DefaultUpdateInterval = 3f;
        public bool PlaySoundEffects = true;
    }
}