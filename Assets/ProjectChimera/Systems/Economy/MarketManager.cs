using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.Economy;

namespace ProjectChimera.Systems.Economy
{
    /// <summary>
    /// Manages global market dynamics, pricing algorithms, supply/demand simulation,
    /// and market condition updates for the cannabis economy simulation.
    /// </summary>
    public class MarketManager : ChimeraManager
    {
        [Header("Market Configuration")]
        [SerializeField] private List<MarketProductSO> _marketProducts = new List<MarketProductSO>();
        [SerializeField] private MarketConditions _currentMarketConditions;
        [SerializeField] private float _marketUpdateInterval = 1f; // In-game days
        [SerializeField] private AnimationCurve _seasonalDemandCurve;
        
        [Header("Price Volatility")]
        [SerializeField, Range(0f, 0.5f)] private float _dailyVolatility = 0.05f;
        [SerializeField, Range(0f, 1f)] private float _marketStability = 0.7f;
        [SerializeField] private Vector2 _priceShockRange = new Vector2(0.1f, 0.3f);
        
        [Header("Supply & Demand")]
        [SerializeField] private SupplyDemandParameters _supplyDemandParams;
        [SerializeField] private List<DemandShock> _pendingDemandShocks = new List<DemandShock>();
        [SerializeField] private List<SupplyShock> _pendingSupplyShocks = new List<SupplyShock>();
        
        [Header("Market Events")]
        [SerializeField] private SimpleGameEventSO _marketUpdateEvent;
        [SerializeField] private SimpleGameEventSO _priceChangeEvent;
        [SerializeField] private SimpleGameEventSO _marketShockEvent;
        
        // Runtime Data
        private Dictionary<MarketProductSO, ProductMarketData> _productMarketData;
        private Dictionary<ProductCategory, CategoryMarketData> _categoryMarketData;
        private float _timeSinceLastUpdate;
        private Queue<MarketEvent> _recentEvents;
        private PlayerMarketReputation _playerReputation;
        
        public MarketConditions CurrentMarketConditions => _currentMarketConditions;
        public PlayerMarketReputation PlayerReputation => _playerReputation;
        
        // Events
        public System.Action<MarketProductSO, float, float> OnProductPriceChanged; // product, oldPrice, newPrice
        public System.Action<MarketConditions> OnMarketConditionsChanged;
        public System.Action<MarketEvent> OnMarketEventOccurred;
        public System.Action<MarketTransaction> OnSaleCompleted;
        public System.Action<float> OnProfitGenerated;
        
        protected override void OnManagerInitialize()
        {
            _productMarketData = new Dictionary<MarketProductSO, ProductMarketData>();
            _categoryMarketData = new Dictionary<ProductCategory, CategoryMarketData>();
            _recentEvents = new Queue<MarketEvent>();
            _playerReputation = new PlayerMarketReputation();
            
            InitializeProductMarketData();
            InitializeCategoryMarketData();
            SetupMarketConditions();
            
            Debug.Log("MarketManager initialized successfully");
        }
        
        protected override void OnManagerShutdown()
        {
            // Cleanup resources
        }
        
        protected override void OnManagerUpdate()
        {
            if (!IsInitialized) return;
            
            _timeSinceLastUpdate += Time.deltaTime;
            
            // Use scaled delta time for market updates
            // PHASE 1 PC-007: Use Unity's Time.deltaTime instead of direct TimeManager reference
            float gameTimeDelta = Time.deltaTime;
            
            if (_timeSinceLastUpdate >= _marketUpdateInterval * gameTimeDelta)
            {
                UpdateMarketConditions();
                UpdateProductPrices();
                ProcessMarketEvents();
                ProcessSupplyDemandShocks();
                
                _timeSinceLastUpdate = 0f;
                
                _marketUpdateEvent?.Raise();
            }
        }
        
        /// <summary>
        /// Gets current market price for a specific product.
        /// </summary>
        public float GetCurrentPrice(MarketProductSO product, bool isRetail, float qualityScore = 0.8f)
        {
            if (!_productMarketData.ContainsKey(product))
            {
                Debug.LogWarning($"Product {product.name} not found in market data");
                return product.BaseRetailPrice;
            }
            
            return product.CalculateCurrentPrice(isRetail, qualityScore, _currentMarketConditions);
        }
        
        /// <summary>
        /// Gets market attractiveness score for a product category.
        /// </summary>
        public float GetMarketAttractiveness(ProductCategory category)
        {
            if (!_categoryMarketData.ContainsKey(category))
                return 0.5f;
            
            var categoryData = _categoryMarketData[category];
            return CalculateAttractivenessScore(categoryData);
        }
        
        /// <summary>
        /// Simulates selling a product to the market.
        /// </summary>
        public MarketTransaction ProcessSale(MarketProductSO product, float quantity, float qualityScore, bool isRetail = false)
        {
            var transaction = new MarketTransaction
            {
                Product = product,
                Quantity = quantity,
                QualityScore = qualityScore,
                IsRetail = isRetail,
                TransactionType = MarketTransactionType.Sale,
                Timestamp = System.DateTime.Now
            };
            
            // Calculate price and market impact
            float unitPrice = GetCurrentPrice(product, isRetail, qualityScore);
            transaction.UnitPrice = unitPrice;
            transaction.TotalValue = unitPrice * quantity;
            
            // Apply market impact (large sales can reduce prices)
            ApplyMarketImpact(product, quantity, MarketTransactionType.Sale);
            
            // Update player reputation based on quality
            UpdatePlayerReputationFromSale(product, qualityScore, quantity);
            
            // Invoke events for other systems
            OnSaleCompleted?.Invoke(transaction);
            OnProfitGenerated?.Invoke(transaction.TotalValue);
            
            return transaction;
        }
        
        /// <summary>
        /// Simulates purchasing a product from the market.
        /// </summary>
        public MarketTransaction ProcessPurchase(MarketProductSO product, float quantity, bool isRetail = false)
        {
            var transaction = new MarketTransaction
            {
                Product = product,
                Quantity = quantity,
                IsRetail = isRetail,
                TransactionType = MarketTransactionType.Purchase,
                Timestamp = System.DateTime.Now
            };
            
            float unitPrice = GetCurrentPrice(product, isRetail);
            transaction.UnitPrice = unitPrice;
            transaction.TotalValue = unitPrice * quantity;
            
            // Apply market impact (large purchases can increase prices)
            ApplyMarketImpact(product, quantity, MarketTransactionType.Purchase);
            
            return transaction;
        }
        
        /// <summary>
        /// Triggers a market shock event.
        /// </summary>
        public void TriggerMarketShock(MarketShockType shockType, float intensity, ProductCategory affectedCategory = ProductCategory.Flower)
        {
            var shockEvent = new MarketEvent
            {
                EventType = MarketEventType.Market_Shock,
                Description = $"{shockType} shock affecting {affectedCategory}",
                Intensity = intensity,
                AffectedCategory = affectedCategory,
                Duration = Random.Range(7f, 30f), // 7-30 days
                Timestamp = System.DateTime.Now
            };
            
            ProcessMarketEvent(shockEvent);
            _marketShockEvent?.Raise();
        }
        
        /// <summary>
        /// Gets demand forecast for a product category.
        /// </summary>
        public DemandForecast GetDemandForecast(ProductCategory category, int daysAhead = 30)
        {
            var forecast = new DemandForecast
            {
                Category = category,
                ForecastDays = daysAhead,
                CurrentDemand = _currentMarketConditions.DemandLevel,
                TrendDirection = CalculateDemandTrend(category),
                Confidence = CalculateForecastConfidence(category)
            };
            
            // Calculate projected demand levels
            forecast.ProjectedDemandLevels = new float[daysAhead];
            for (int i = 0; i < daysAhead; i++)
            {
                forecast.ProjectedDemandLevels[i] = ProjectDemandForDay(category, i + 1);
            }
            
            return forecast;
        }
        
        private void InitializeProductMarketData()
        {
            foreach (var product in _marketProducts)
            {
                var marketData = new ProductMarketData
                {
                    Product = product,
                    CurrentPrice = product.BaseRetailPrice,
                    DemandLevel = product.DemandProfile.BaseDemand / 100f,
                    SupplyLevel = Random.Range(0.4f, 0.8f),
                    Volatility = Random.Range(0.1f, 0.3f),
                    TrendDirection = Random.Range(-0.1f, 0.1f)
                };
                
                _productMarketData[product] = marketData;
            }
        }
        
        private void InitializeCategoryMarketData()
        {
            var categories = System.Enum.GetValues(typeof(ProductCategory)).Cast<ProductCategory>();
            
            foreach (var category in categories)
            {
                var categoryData = new CategoryMarketData
                {
                    Category = category,
                    OverallDemand = Random.Range(0.4f, 0.8f),
                    MarketShare = Random.Range(0.05f, 0.3f),
                    GrowthRate = Random.Range(-0.05f, 0.15f),
                    CompetitionLevel = Random.Range(0.3f, 0.9f)
                };
                
                _categoryMarketData[category] = categoryData;
            }
        }
        
        private void SetupMarketConditions()
        {
            _currentMarketConditions = new MarketConditions
            {
                DemandLevel = Random.Range(0.4f, 0.7f),
                SupplyLevel = Random.Range(0.4f, 0.7f),
                CurrentSeason = GetCurrentSeason(),
                EconomicHealth = Random.Range(0.6f, 0.8f),
                RegulatoryStability = Random.Range(0.7f, 0.9f)
            };
        }
        
        private void UpdateMarketConditions()
        {
            // Update seasonal factors
            var newSeason = GetCurrentSeason();
            if (newSeason != _currentMarketConditions.CurrentSeason)
            {
                _currentMarketConditions.CurrentSeason = newSeason;
                ApplySeasonalChanges(newSeason);
            }
            
            // Apply gradual market changes
            _currentMarketConditions.DemandLevel = ApplyMarketDrift(_currentMarketConditions.DemandLevel, 0.02f);
            _currentMarketConditions.SupplyLevel = ApplyMarketDrift(_currentMarketConditions.SupplyLevel, 0.02f);
            _currentMarketConditions.EconomicHealth = ApplyMarketDrift(_currentMarketConditions.EconomicHealth, 0.01f);
            
            OnMarketConditionsChanged?.Invoke(_currentMarketConditions);
        }
        
        private void UpdateProductPrices()
        {
            foreach (var kvp in _productMarketData.ToList())
            {
                var product = kvp.Key;
                var marketData = kvp.Value;
                
                float oldPrice = marketData.CurrentPrice;
                
                // Apply daily volatility
                float volatilityFactor = Random.Range(-_dailyVolatility, _dailyVolatility);
                
                // Apply supply/demand pressure
                float supplyDemandFactor = (marketData.DemandLevel - marketData.SupplyLevel) * 0.1f;
                
                // Apply market stability
                float stabilityFactor = (1f - _marketStability) * volatilityFactor;
                
                float priceChange = (volatilityFactor + supplyDemandFactor + stabilityFactor);
                marketData.CurrentPrice *= (1f + priceChange);
                
                // Clamp price to reasonable bounds
                marketData.CurrentPrice = Mathf.Clamp(marketData.CurrentPrice, 
                    product.BaseRetailPrice * 0.3f, 
                    product.BaseRetailPrice * 3f);
                
                if (Mathf.Abs(oldPrice - marketData.CurrentPrice) > 0.01f)
                {
                    OnProductPriceChanged?.Invoke(product, oldPrice, marketData.CurrentPrice);
                    _priceChangeEvent?.Raise();
                }
            }
        }
        
        private void ProcessMarketEvents()
        {
            // Process pending events
            while (_recentEvents.Count > 0 && 
                   (System.DateTime.Now - _recentEvents.Peek().Timestamp).TotalDays > 30)
            {
                _recentEvents.Dequeue();
            }
            
            // Random market events
            if (Random.Range(0f, 1f) < 0.05f) // 5% chance per update
            {
                GenerateRandomMarketEvent();
            }
        }
        
        private void ProcessSupplyDemandShocks()
        {
            // Process demand shocks
            for (int i = _pendingDemandShocks.Count - 1; i >= 0; i--)
            {
                var shock = _pendingDemandShocks[i];
                shock.Duration -= _marketUpdateInterval;
                
                if (shock.Duration <= 0)
                {
                    _pendingDemandShocks.RemoveAt(i);
                }
            }
            
            // Process supply shocks
            for (int i = _pendingSupplyShocks.Count - 1; i >= 0; i--)
            {
                var shock = _pendingSupplyShocks[i];
                shock.Duration -= _marketUpdateInterval;
                
                if (shock.Duration <= 0)
                {
                    _pendingSupplyShocks.RemoveAt(i);
                }
            }
        }
        
        private Season GetCurrentSeason()
        {
            // Use current real-world date for seasons for now
            int dayOfYear = System.DateTime.Now.DayOfYear;
            
            if (dayOfYear >= 80 && dayOfYear < 172) return Season.Spring;
            if (dayOfYear >= 172 && dayOfYear < 266) return Season.Summer;
            if (dayOfYear >= 266 && dayOfYear < 355) return Season.Autumn;
            return Season.Winter;
        }
        
        private void ApplySeasonalChanges(Season newSeason)
        {
            foreach (var kvp in _productMarketData.ToList())
            {
                var product = kvp.Key;
                var marketData = kvp.Value;
                
                var seasonalModifier = product.SeasonalModifiers.Find(sm => sm.Season == newSeason);
                if (seasonalModifier != null)
                {
                    marketData.DemandLevel *= seasonalModifier.DemandMultiplier;
                    marketData.DemandLevel = Mathf.Clamp01(marketData.DemandLevel);
                }
            }
        }
        
        private float ApplyMarketDrift(float currentValue, float maxChange)
        {
            float change = Random.Range(-maxChange, maxChange);
            return Mathf.Clamp01(currentValue + change);
        }
        
        private void ApplyMarketImpact(MarketProductSO product, float quantity, MarketTransactionType type)
        {
            if (!_productMarketData.ContainsKey(product)) return;
            
            var marketData = _productMarketData[product];
            
            // Calculate market impact based on transaction size
            float marketSize = 1000f; // Assume market size of 1000kg for base calculation
            float impactFactor = quantity / marketSize;
            
            if (type == MarketTransactionType.Sale)
            {
                // Selling increases supply, decreases price pressure
                marketData.SupplyLevel += impactFactor * 0.1f;
                marketData.DemandLevel -= impactFactor * 0.05f;
            }
            else
            {
                // Buying decreases supply, increases price pressure
                marketData.SupplyLevel -= impactFactor * 0.1f;
                marketData.DemandLevel += impactFactor * 0.05f;
            }
            
            // Clamp values
            marketData.SupplyLevel = Mathf.Clamp01(marketData.SupplyLevel);
            marketData.DemandLevel = Mathf.Clamp01(marketData.DemandLevel);
        }
        
        private void UpdatePlayerReputationFromSale(MarketProductSO product, float qualityScore, float quantity)
        {
            float qualityImpact = (qualityScore - 0.7f) * 0.1f; // Above 0.7 improves reputation
            float volumeImpact = Mathf.Min(quantity / 100f, 0.05f); // Volume bonus caps at 0.05
            
            _playerReputation.QualityScore = Mathf.Clamp01(_playerReputation.QualityScore + qualityImpact);
            _playerReputation.VolumeScore = Mathf.Clamp01(_playerReputation.VolumeScore + volumeImpact);
            _playerReputation.ReliabilityScore = Mathf.Clamp01(_playerReputation.ReliabilityScore + 0.01f);
        }
        
        private float CalculateAttractivenessScore(CategoryMarketData categoryData)
        {
            float score = 0f;
            
            // High demand increases attractiveness
            score += categoryData.OverallDemand * 0.3f;
            
            // Growth rate impacts attractiveness
            score += (categoryData.GrowthRate + 0.05f) * 2f; // Normalize around 0
            
            // Lower competition increases attractiveness
            score += (1f - categoryData.CompetitionLevel) * 0.2f;
            
            return Mathf.Clamp01(score);
        }
        
        private TrendDirection CalculateDemandTrend(ProductCategory category)
        {
            if (!_categoryMarketData.ContainsKey(category))
                return TrendDirection.Stable;
            
            float growthRate = _categoryMarketData[category].GrowthRate;
            
            if (growthRate > 0.05f) return TrendDirection.Increasing;
            if (growthRate < -0.05f) return TrendDirection.Decreasing;
            return TrendDirection.Stable;
        }
        
        private float CalculateForecastConfidence(ProductCategory category)
        {
            // Base confidence on market stability and data availability
            float baseConfidence = _marketStability;
            
            // Reduce confidence for volatile categories
            if (_categoryMarketData.ContainsKey(category))
            {
                float volatility = _categoryMarketData[category].CompetitionLevel;
                baseConfidence *= (1f - volatility * 0.3f);
            }
            
            return Mathf.Clamp01(baseConfidence);
        }
        
        private float ProjectDemandForDay(ProductCategory category, int daysAhead)
        {
            if (!_categoryMarketData.ContainsKey(category))
                return 0.5f;
            
            var categoryData = _categoryMarketData[category];
            float currentDemand = categoryData.OverallDemand;
            float growthRate = categoryData.GrowthRate;
            
            // Apply growth rate over time
            float projectedDemand = currentDemand * (1f + (growthRate * daysAhead / 365f));
            
            // Add some randomness for uncertainty
            float uncertainty = Random.Range(-0.1f, 0.1f) * (daysAhead / 30f);
            projectedDemand += uncertainty;
            
            return Mathf.Clamp01(projectedDemand);
        }
        
        private void GenerateRandomMarketEvent()
        {
            var eventTypes = new MarketEventType[] { MarketEventType.Regulatory_Change, MarketEventType.Supply_Disruption, 
                                   MarketEventType.Demand_Spike, MarketEventType.Technology_Advancement };
            
            var randomEvent = new MarketEvent
            {
                EventType = eventTypes[Random.Range(0, eventTypes.Length)],
                Description = GenerateEventDescription(eventTypes[Random.Range(0, eventTypes.Length)]),
                Intensity = Random.Range(0.1f, 0.5f),
                AffectedCategory = (ProductCategory)Random.Range(0, System.Enum.GetValues(typeof(ProductCategory)).Length),
                Duration = Random.Range(5f, 20f),
                Timestamp = System.DateTime.Now
            };
            
            ProcessMarketEvent(randomEvent);
        }
        
        private string GenerateEventDescription(MarketEventType eventType)
        {
            switch (eventType)
            {
                case MarketEventType.Regulatory_Change:
                    return "New regulations affecting market dynamics";
                case MarketEventType.Supply_Disruption:
                    return "Supply chain disruption impacting availability";
                case MarketEventType.Demand_Spike:
                    return "Sudden increase in consumer demand";
                case MarketEventType.Technology_Advancement:
                    return "Technological breakthrough affecting production";
                default:
                    return "Market event occurred";
            }
        }
        
        private void ProcessMarketEvent(MarketEvent marketEvent)
        {
            _recentEvents.Enqueue(marketEvent);
            
            // Apply event effects based on type
            switch (marketEvent.EventType)
            {
                case MarketEventType.Demand_Spike:
                    ModifyDemandForCategory(marketEvent.AffectedCategory, marketEvent.Intensity);
                    break;
                case MarketEventType.Supply_Disruption:
                    ModifySupplyForCategory(marketEvent.AffectedCategory, -marketEvent.Intensity);
                    break;
                case MarketEventType.Regulatory_Change:
                    ModifyRegulatoryStability(-marketEvent.Intensity * 0.5f);
                    break;
            }
            
            OnMarketEventOccurred?.Invoke(marketEvent);
        }
        
        private void ModifyDemandForCategory(ProductCategory category, float modifier)
        {
            if (_categoryMarketData.ContainsKey(category))
            {
                _categoryMarketData[category].OverallDemand = 
                    Mathf.Clamp01(_categoryMarketData[category].OverallDemand + modifier);
            }
        }
        
        private void ModifySupplyForCategory(ProductCategory category, float modifier)
        {
            var affectedProducts = _marketProducts.Where(p => p.Category == category);
            foreach (var product in affectedProducts)
            {
                if (_productMarketData.ContainsKey(product))
                {
                    _productMarketData[product].SupplyLevel = 
                        Mathf.Clamp01(_productMarketData[product].SupplyLevel + modifier);
                }
            }
        }
        
        private void ModifyRegulatoryStability(float modifier)
        {
            _currentMarketConditions.RegulatoryStability = 
                Mathf.Clamp01(_currentMarketConditions.RegulatoryStability + modifier);
        }
        
        /// <summary>
        /// Gets portfolio metrics for the financial dashboard.
        /// </summary>
        public PortfolioMetrics GetPortfolioMetrics()
        {
            return new PortfolioMetrics
            {
                TotalValue = 100000f,
                DailyChange = 2.5f,
                WeeklyChange = 8.2f,
                MonthlyChange = 15.7f,
                YearToDateChange = 45.3f,
                ProfitLoss = 15000f,
                TopPerformers = new List<string> { "Cannabis Futures", "Equipment ETF", "Real Estate Fund" },
                RiskScore = 0.65f,
                Diversification = 0.78f,
                LiquidityRatio = 0.45f
            };
        }
        
        /// <summary>
        /// Gets current player funds/cash balance.
        /// </summary>
        public float PlayerFunds
        {
            get
            {
                // Get from TradingManager if available, otherwise return default
                var tradingManager = GameManager.Instance?.GetManager<TradingManager>();
                return tradingManager?.GetCashBalance() ?? 25000f;
            }
        }
        
        /// <summary>
        /// Calculates estimated daily revenue based on recent transactions.
        /// </summary>
        public float CalculateDailyRevenue()
        {
            // Get from TradingManager if available, otherwise return estimated value
            var tradingManager = GameManager.Instance?.GetManager<TradingManager>();
            if (tradingManager != null)
            {
                var recentSales = tradingManager.TransactionHistory
                    .Where(t => t.TransactionType == TradingTransactionType.Sale && 
                               t.Success && 
                               (System.DateTime.Now - t.CompletionTime).TotalDays <= 7)
                    .ToList();
                
                if (recentSales.Any())
                {
                    return recentSales.Sum(t => t.TotalValue) / 7f; // Average daily revenue over last week
                }
            }
            
            return 2500f; // Default daily revenue estimate
        }
        
        /// <summary>
        /// Calculates estimated daily expenses based on recent transactions.
        /// </summary>
        public float CalculateDailyExpenses()
        {
            // Get from TradingManager if available, otherwise return estimated value
            var tradingManager = GameManager.Instance?.GetManager<TradingManager>();
            if (tradingManager != null)
            {
                var recentPurchases = tradingManager.TransactionHistory
                    .Where(t => t.TransactionType == TradingTransactionType.Purchase && 
                               t.Success && 
                               (System.DateTime.Now - t.CompletionTime).TotalDays <= 7)
                    .ToList();
                
                if (recentPurchases.Any())
                {
                    return recentPurchases.Sum(t => t.TotalValue) / 7f; // Average daily expenses over last week
                }
            }
            
            return 1500f; // Default daily expenses estimate
        }
        
        /// <summary>
        /// Gets current market trends for display in UI.
        /// </summary>
        public List<MarketTrend> GetMarketTrends()
        {
            var trends = new List<MarketTrend>();
            
            foreach (var categoryData in _categoryMarketData)
            {
                trends.Add(new MarketTrend
                {
                    Category = categoryData.Key.ToString(),
                    TrendDirection = CalculateDemandTrend(categoryData.Key),
                    ChangePercentage = categoryData.Value.GrowthRate * 100f,
                    MarketShare = categoryData.Value.MarketShare,
                    Demand = categoryData.Value.OverallDemand
                });
            }
            
            return trends;
        }
        
        /// <summary>
        /// Gets financial data for loading/saving.
        /// </summary>
        public object GetFinancialData()
        {
            return new
            {
                LastUpdate = System.DateTime.Now,
                MarketData = _marketProducts.ToDictionary(
                    product => product.ProductName,
                    product => new { 
                        Price = _productMarketData.ContainsKey(product) ? _productMarketData[product].CurrentPrice : 0f,
                        Volume = _productMarketData.ContainsKey(product) ? _productMarketData[product].DemandLevel : 0f
                    }
                ),
                PlayerStats = new
                {
                    NetWorth = 150000f,
                    CashBalance = 25000f,
                    InvestmentValue = 100000f,
                    DebtBalance = 50000f
                }
            };
        }
    }
    
    [System.Serializable]
    public class SupplyDemandParameters
    {
        [Range(0f, 1f)] public float BaseVolatility = 0.2f;
        [Range(0f, 0.1f)] public float DailyDriftMax = 0.02f;
        [Range(0f, 1f)] public float SeasonalInfluence = 0.3f;
        [Range(0f, 1f)] public float EventInfluence = 0.5f;
    }
    
    [System.Serializable]
    public class ProductMarketData
    {
        public MarketProductSO Product;
        public float CurrentPrice;
        public float DemandLevel;
        public float SupplyLevel;
        public float Volatility;
        public float TrendDirection;
        public List<PricePoint> PriceHistory = new List<PricePoint>();
    }
    
    [System.Serializable]
    public class CategoryMarketData
    {
        public ProductCategory Category;
        public float OverallDemand;
        public float MarketShare;
        public float GrowthRate;
        public float CompetitionLevel;
    }
    
    [System.Serializable]
    public class PricePoint
    {
        public float Price;
        public System.DateTime Timestamp;
    }
    
    [System.Serializable]
    public class PlayerMarketReputation
    {
        [Range(0f, 1f)] public float QualityScore = 0.5f;
        [Range(0f, 1f)] public float VolumeScore = 0.5f;
        [Range(0f, 1f)] public float ReliabilityScore = 0.5f;
        [Range(0f, 1f)] public float InnovationScore = 0.5f;
        
        public float OverallReputation => (QualityScore + VolumeScore + ReliabilityScore + InnovationScore) / 4f;
    }
    
    [System.Serializable]
    public class MarketTransaction
    {
        public MarketProductSO Product;
        public float Quantity;
        public float QualityScore;
        public float UnitPrice;
        public float TotalValue;
        public bool IsRetail;
        public MarketTransactionType TransactionType;
        public System.DateTime Timestamp;
    }
    
    [System.Serializable]
    public class MarketEvent
    {
        public MarketEventType EventType;
        public string Description;
        public float Intensity;
        public ProductCategory AffectedCategory;
        public float Duration; // In days
        public System.DateTime Timestamp;
    }
    
    [System.Serializable]
    public class DemandShock
    {
        public ProductCategory AffectedCategory;
        public float Magnitude;
        public float Duration;
        public string Reason;
    }
    
    [System.Serializable]
    public class SupplyShock
    {
        public ProductCategory AffectedCategory;
        public float Magnitude;
        public float Duration;
        public string Reason;
    }
    
    [System.Serializable]
    public class DemandForecast
    {
        public ProductCategory Category;
        public int ForecastDays;
        public float CurrentDemand;
        public TrendDirection TrendDirection;
        public float Confidence;
        public float[] ProjectedDemandLevels;
    }
    
    public enum MarketTransactionType
    {
        Sale,
        Purchase
    }
    
    public enum MarketEventType
    {
        Price_Shock,
        Demand_Spike,
        Supply_Disruption,
        Regulatory_Change,
        Technology_Advancement,
        Seasonal_Change,
        Market_Shock,
        Competition_Entry
    }
    
    public enum MarketShockType
    {
        Supply_Chain_Disruption,
        Regulatory_Crackdown,
        Economic_Recession,
        Technology_Breakthrough,
        Consumer_Trend_Shift
    }
    
    public enum TrendDirection
    {
        Decreasing,
        Stable,
        Increasing
    }
    
    /// <summary>
    /// Portfolio metrics data structure for UI display.
    /// </summary>
    [System.Serializable]
    public class PortfolioMetrics
    {
        public float TotalValue;
        public float DailyChange;
        public float WeeklyChange;
        public float MonthlyChange;
        public float YearToDateChange;
        public float ProfitLoss;
        public List<string> TopPerformers;
        public float RiskScore;
        public float Diversification;
        public float LiquidityRatio;
    }
    
    /// <summary>
    /// Market trend data structure for UI display.
    /// </summary>
    [System.Serializable]
    public class MarketTrend
    {
        public string Category;
        public TrendDirection TrendDirection;
        public float ChangePercentage;
        public float MarketShare;
        public float Demand;
    }
}
