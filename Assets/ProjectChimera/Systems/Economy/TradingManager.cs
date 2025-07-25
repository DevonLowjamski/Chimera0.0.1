using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.Economy;

namespace ProjectChimera.Systems.Economy
{
    /// <summary>
    /// Trading-specific transaction types to avoid conflicts with currency system
    /// </summary>
    public enum TradingTransactionType
    {
        Purchase,
        Sale
    }

    /// <summary>
    /// Manages trading operations, transaction processing, inventory management,
    /// and financial tracking for the cannabis marketplace simulation.
    /// </summary>
    public class TradingManager : ChimeraManager
    {
        [Header("Trading Configuration")]
        [SerializeField] private TradingSettings _tradingSettings;
        [SerializeField] private List<TradingPost> _availableTradingPosts = new List<TradingPost>();
        [SerializeField] private PlayerInventory _playerInventory;
        [SerializeField] private PlayerFinances _playerFinances;
        
        [Header("Transaction Processing")]
        [SerializeField] private TransactionSettings _transactionSettings;
        [SerializeField] private List<PaymentMethod> _availablePaymentMethods = new List<PaymentMethod>();
        [SerializeField] private float _transactionProcessingInterval = 0.1f; // In-game hours
        
        [Header("Market Integration")]
        [SerializeField] private bool _enableDynamicPricing = true;
        [SerializeField] private bool _enableSupplyDemandTracking = true;
        [SerializeField] private float _priceUpdateFrequency = 4f; // In-game hours
        
        [Header("Events")]
        [SerializeField] private SimpleGameEventSO _transactionCompletedEvent;
        [SerializeField] private SimpleGameEventSO _inventoryChangedEvent;
        [SerializeField] private SimpleGameEventSO _financialStatusChangedEvent;
        [SerializeField] private SimpleGameEventSO _tradingOpportunityEvent;
        
        // Runtime Data
        private Queue<PendingTransaction> _pendingTransactions;
        private List<CompletedTransaction> _transactionHistory;
        private Dictionary<MarketProductSO, ProductTradingData> _productTradingData;
        private Dictionary<TradingPost, TradingPostState> _tradingPostStates;
        private List<TradingOpportunity> _availableOpportunities;
        private float _timeSinceLastUpdate;
        
        public PlayerInventory PlayerInventory => _playerInventory;
        public PlayerFinances PlayerFinances => _playerFinances;
        public List<TradingOpportunity> AvailableOpportunities => _availableOpportunities;
        public List<CompletedTransaction> TransactionHistory => _transactionHistory;
        public PlayerReputation PlayerReputation { get; private set; }
        
        // Events
        public System.Action<CompletedTransaction> OnTransactionCompleted;
        public System.Action<InventoryItem, float> OnInventoryChanged; // item, quantityChange
        public System.Action<float, float> OnCashChanged; // oldAmount, newAmount
        public System.Action<TradingOpportunity> OnTradingOpportunityAvailable;
        
        protected override void OnManagerInitialize()
        {
            _pendingTransactions = new Queue<PendingTransaction>();
            _transactionHistory = new List<CompletedTransaction>();
            _productTradingData = new Dictionary<MarketProductSO, ProductTradingData>();
            _tradingPostStates = new Dictionary<TradingPost, TradingPostState>();
            _availableOpportunities = new List<TradingOpportunity>();
            
            InitializePlayerInventory();
            InitializePlayerFinances();
            InitializePlayerReputation();
            InitializeTradingPosts();
            InitializeProductTradingData();
            
            Debug.Log("TradingManager initialized successfully");
        }
        
        private void InitializePlayerReputation()
        {
            PlayerReputation = new PlayerReputation
            {
                OverallReputation = 0.5f,
                TransactionCount = 0,
                ReliabilityScore = 0.5f,
                QualityScore = 0.5f,
                InnovationScore = 0.5f,
                ProfessionalismScore = 0.5f,
                ComplianceScore = 0.5f
            };
        }
        
        protected override void OnManagerShutdown()
        {
            // Cleanup resources
        }
        
        protected override void OnManagerUpdate()
        {
            if (!IsInitialized) return;
            
            _timeSinceLastUpdate += Time.deltaTime;
            
            var timeManager = GameManager.Instance?.GetManager<TimeManager>();
            float gameTimeDelta = timeManager?.GetScaledDeltaTime() ?? Time.deltaTime;
            
            if (_timeSinceLastUpdate >= _transactionProcessingInterval * gameTimeDelta)
            {
                ProcessPendingTransactions();
                UpdateTradingPostStates();
                UpdateTradingOpportunities();
                ProcessInventoryDecay();
                UpdateFinancialMetrics();
                
                _timeSinceLastUpdate = 0f;
            }
        }
        
        /// <summary>
        /// Initiates a buy transaction for a product.
        /// </summary>
        public TransactionResult InitiateBuyTransaction(MarketProductSO product, float quantity, TradingPost tradingPost, PaymentMethod paymentMethod)
        {
            var result = new TransactionResult
            {
                Success = false,
                TransactionId = System.Guid.NewGuid().ToString(),
                Product = product,
                Quantity = quantity,
                TradingPost = tradingPost
            };
            
            // Get current market price
            var marketManager = GameManager.Instance.GetManager<MarketManager>();
            if (marketManager == null)
            {
                result.ErrorMessage = "Market system unavailable";
                return result;
            }
            
            float unitPrice = marketManager.GetCurrentPrice(product, false); // Wholesale price for buying
            float totalCost = unitPrice * quantity;
            
            // Apply trading post markup
            if (_tradingPostStates.ContainsKey(tradingPost))
            {
                totalCost *= _tradingPostStates[tradingPost].PriceMarkup;
            }
            
            // Check if player has enough funds
            if (!CanAffordTransaction(totalCost, paymentMethod))
            {
                result.ErrorMessage = "Insufficient funds";
                return result;
            }
            
            // Check trading post availability
            if (!IsTradingPostAvailable(tradingPost, product, quantity))
            {
                result.ErrorMessage = "Product unavailable at this trading post";
                return result;
            }
            
            // Create pending transaction
            var pendingTransaction = new PendingTransaction
            {
                TransactionId = result.TransactionId,
                TransactionType = TradingTransactionType.Purchase,
                Product = product,
                Quantity = quantity,
                UnitPrice = unitPrice,
                TotalValue = totalCost,
                TradingPost = tradingPost,
                PaymentMethod = paymentMethod,
                InitiationTime = System.DateTime.Now,
                EstimatedCompletionTime = CalculateTransactionTime(tradingPost, paymentMethod),
                Status = TransactionStatus.Pending
            };
            
            _pendingTransactions.Enqueue(pendingTransaction);
            
            result.Success = true;
            result.UnitPrice = unitPrice;
            result.TotalValue = totalCost;
            result.EstimatedCompletionTime = pendingTransaction.EstimatedCompletionTime;
            
            return result;
        }
        
        /// <summary>
        /// Initiates a sell transaction for a product.
        /// </summary>
        public TransactionResult InitiateSellTransaction(InventoryItem inventoryItem, float quantity, TradingPost tradingPost, PaymentMethod paymentMethod)
        {
            var result = new TransactionResult
            {
                Success = false,
                TransactionId = System.Guid.NewGuid().ToString(),
                Product = inventoryItem.Product,
                Quantity = quantity,
                TradingPost = tradingPost
            };
            
            // Check if player has enough inventory
            if (inventoryItem.Quantity < quantity)
            {
                result.ErrorMessage = "Insufficient inventory";
                return result;
            }
            
            // Get current market price
            var marketManager = GameManager.Instance.GetManager<MarketManager>();
            if (marketManager == null)
            {
                result.ErrorMessage = "Market system unavailable";
                return result;
            }
            
            float unitPrice = marketManager.GetCurrentPrice(inventoryItem.Product, true, inventoryItem.QualityScore); // Retail price for selling
            float totalRevenue = unitPrice * quantity;
            
            // Apply trading post commission
            if (_tradingPostStates.ContainsKey(tradingPost))
            {
                totalRevenue *= (1f - _tradingPostStates[tradingPost].CommissionRate);
            }
            
            // Check trading post acceptance
            if (!WillTradingPostAccept(tradingPost, inventoryItem, quantity))
            {
                result.ErrorMessage = "Trading post will not accept this product";
                return result;
            }
            
            // Create pending transaction
            var pendingTransaction = new PendingTransaction
            {
                TransactionId = result.TransactionId,
                TransactionType = TradingTransactionType.Sale,
                Product = inventoryItem.Product,
                Quantity = quantity,
                UnitPrice = unitPrice,
                TotalValue = totalRevenue,
                TradingPost = tradingPost,
                PaymentMethod = paymentMethod,
                InitiationTime = System.DateTime.Now,
                EstimatedCompletionTime = CalculateTransactionTime(tradingPost, paymentMethod),
                Status = TransactionStatus.Pending,
                SourceInventoryItem = inventoryItem
            };
            
            _pendingTransactions.Enqueue(pendingTransaction);
            
            result.Success = true;
            result.UnitPrice = unitPrice;
            result.TotalValue = totalRevenue;
            result.EstimatedCompletionTime = pendingTransaction.EstimatedCompletionTime;
            
            return result;
        }
        
        /// <summary>
        /// Gets current inventory for a specific product.
        /// </summary>
        public List<InventoryItem> GetInventoryForProduct(MarketProductSO product)
        {
            return _playerInventory.InventoryItems
                .Where(item => item.Product == product)
                .ToList();
        }
        
        /// <summary>
        /// Gets total quantity of a product in inventory.
        /// </summary>
        public float GetTotalInventoryQuantity(MarketProductSO product)
        {
            return _playerInventory.InventoryItems
                .Where(item => item.Product == product)
                .Sum(item => item.Quantity);
        }
        
        /// <summary>
        /// Gets current cash balance.
        /// </summary>
        public float GetCashBalance()
        {
            return _playerFinances.CashOnHand;
        }
        
        /// <summary>
        /// Gets net worth including inventory value.
        /// </summary>
        public float GetNetWorth()
        {
            float inventoryValue = CalculateInventoryValue();
            return _playerFinances.CashOnHand + _playerFinances.BankBalance + inventoryValue - _playerFinances.TotalDebt;
        }
        
        /// <summary>
        /// Gets trading opportunities based on market conditions.
        /// </summary>
        public List<TradingOpportunity> GetTradingOpportunities(OpportunityType opportunityType = OpportunityType.All)
        {
            if (opportunityType == OpportunityType.All)
                return _availableOpportunities;
            
            return _availableOpportunities.Where(op => op.OpportunityType == opportunityType).ToList();
        }
        
        /// <summary>
        /// Evaluates profitability of a potential transaction.
        /// </summary>
        public TradingProfitabilityAnalysis AnalyzeProfitability(MarketProductSO product, float quantity, TradingTransactionType transactionType)
        {
            var analysis = new TradingProfitabilityAnalysis
            {
                Product = product,
                Quantity = quantity,
                TransactionType = transactionType
            };
            
            var marketManager = GameManager.Instance.GetManager<MarketManager>();
            if (marketManager == null)
            {
                analysis.IsAnalysisValid = false;
                return analysis;
            }
            
            if (transactionType == TradingTransactionType.Purchase)
            {
                // Analyze buy opportunity
                float buyPrice = marketManager.GetCurrentPrice(product, false) * quantity;
                float sellPrice = marketManager.GetCurrentPrice(product, true) * quantity;
                
                analysis.EstimatedCost = buyPrice;
                analysis.EstimatedRevenue = sellPrice;
                analysis.EstimatedProfit = sellPrice - buyPrice;
                analysis.ProfitMargin = (analysis.EstimatedProfit / buyPrice) * 100f;
            }
            else
            {
                // Analyze sell opportunity
                var inventoryItems = GetInventoryForProduct(product);
                if (inventoryItems.Count > 0)
                {
                    float averageCost = inventoryItems.Sum(item => item.AcquisitionCost * item.Quantity) / 
                                      inventoryItems.Sum(item => item.Quantity);
                    float sellPrice = marketManager.GetCurrentPrice(product, true, 
                        inventoryItems.Average(item => item.QualityScore)) * quantity;
                    
                    analysis.EstimatedCost = averageCost * quantity;
                    analysis.EstimatedRevenue = sellPrice;
                    analysis.EstimatedProfit = sellPrice - (averageCost * quantity);
                    analysis.ProfitMargin = (analysis.EstimatedProfit / (averageCost * quantity)) * 100f;
                }
            }
            
            analysis.IsAnalysisValid = true;
            analysis.RecommendationScore = CalculateRecommendationScore(analysis);
            
            return analysis;
        }
        
        /// <summary>
        /// Transfers cash between accounts.
        /// </summary>
        public bool TransferCash(float amount, CashTransferType transferType)
        {
            switch (transferType)
            {
                case CashTransferType.Cash_To_Bank:
                    if (_playerFinances.CashOnHand >= amount)
                    {
                        _playerFinances.CashOnHand -= amount;
                        _playerFinances.BankBalance += amount;
                        OnCashChanged?.Invoke(_playerFinances.CashOnHand + amount, _playerFinances.CashOnHand);
                        return true;
                    }
                    break;
                
                case CashTransferType.Bank_To_Cash:
                    if (_playerFinances.BankBalance >= amount)
                    {
                        _playerFinances.BankBalance -= amount;
                        _playerFinances.CashOnHand += amount;
                        OnCashChanged?.Invoke(_playerFinances.CashOnHand - amount, _playerFinances.CashOnHand);
                        return true;
                    }
                    break;
            }
            
            return false;
        }
        
        /// <summary>
        /// Executes a trade transaction directly (for UI compatibility).
        /// </summary>
        public bool ExecuteTrade(object tradeTransaction)
        {
            // This method exists for compatibility with UI controller calls
            // In a real implementation, this would process the trade directly
            // For now, we'll return true to indicate successful processing
            Debug.Log($"ExecuteTrade called with transaction: {tradeTransaction}");
            return true;
        }
        
        private void InitializePlayerInventory()
        {
            if (_playerInventory == null)
            {
                _playerInventory = new PlayerInventory
                {
                    InventoryItems = new List<InventoryItem>(),
                    MaxCapacity = _tradingSettings?.StartingInventoryCapacity ?? 1000f,
                    CurrentCapacity = 0f
                };
            }
        }
        
        private void InitializePlayerFinances()
        {
            if (_playerFinances == null)
            {
                _playerFinances = new PlayerFinances
                {
                    CashOnHand = _tradingSettings?.StartingCash ?? 10000f,
                    BankBalance = 0f,
                    TotalDebt = 0f,
                    CreditLimit = _tradingSettings?.StartingCreditLimit ?? 5000f,
                    MonthlyProfit = 0f,
                    AccountsReceivable = 0f,
                    AccountsPayable = 0f,
                    MonthlyCosts = 0f,
                    TransactionHistory = new List<FinancialRecord>()
                };
            }
        }
        
        private void InitializeTradingPosts()
        {
            foreach (var tradingPost in _availableTradingPosts)
            {
                var state = new TradingPostState
                {
                    TradingPost = tradingPost,
                    IsActive = true,
                    PriceMarkup = Random.Range(1.05f, 1.25f), // 5-25% markup
                    CommissionRate = Random.Range(0.02f, 0.08f), // 2-8% commission
                    ReputationWithPlayer = 0.5f,
                    AvailableProducts = new List<TradingPostProduct>(),
                    LastRestockDate = System.DateTime.Now
                };
                
                // Generate initial product availability
                RestockTradingPost(state);
                
                _tradingPostStates[tradingPost] = state;
            }
        }
        
        private void InitializeProductTradingData()
        {
            var marketManager = GameManager.Instance.GetManager<MarketManager>();
            if (marketManager == null) return;
            
            // This would typically get market products from the market manager
            // For now, we'll use a placeholder approach
        }
        
        private void ProcessPendingTransactions()
        {
            var completedTransactions = new List<PendingTransaction>();
            
            while (_pendingTransactions.Count > 0)
            {
                var transaction = _pendingTransactions.Peek();
                
                if (System.DateTime.Now >= transaction.EstimatedCompletionTime)
                {
                    _pendingTransactions.Dequeue();
                    
                    bool success = CompleteTransaction(transaction);
                    
                    var completedTransaction = new CompletedTransaction
                    {
                        TransactionId = transaction.TransactionId,
                        TransactionType = transaction.TransactionType,
                        Product = transaction.Product,
                        Quantity = transaction.Quantity,
                        UnitPrice = transaction.UnitPrice,
                        TotalValue = transaction.TotalValue,
                        TradingPost = transaction.TradingPost,
                        PaymentMethod = transaction.PaymentMethod,
                        CompletionTime = System.DateTime.Now,
                        Success = success,
                        QualityScore = transaction.SourceInventoryItem?.QualityScore ?? 0.8f
                    };
                    
                    _transactionHistory.Add(completedTransaction);
                    OnTransactionCompleted?.Invoke(completedTransaction);
                    _transactionCompletedEvent?.Raise();
                }
                else
                {
                    break; // No more transactions ready to complete
                }
            }
        }
        
        private bool CompleteTransaction(PendingTransaction transaction)
        {
            try
            {
                if (transaction.TransactionType == TradingTransactionType.Purchase)
                {
                    return CompletePurchaseTransaction(transaction);
                }
                else
                {
                    return CompleteSellTransaction(transaction);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error completing transaction {transaction.TransactionId}: {ex.Message}");
                return false;
            }
        }
        
        private bool CompletePurchaseTransaction(PendingTransaction transaction)
        {
            // Deduct payment
            if (!ProcessPayment(transaction.TotalValue, transaction.PaymentMethod))
                return false;
            
            // Add to inventory
            var inventoryItem = new InventoryItem
            {
                Product = transaction.Product,
                Quantity = transaction.Quantity,
                QualityScore = Random.Range(0.7f, 0.95f), // Random quality for purchased items
                AcquisitionCost = transaction.UnitPrice,
                AcquisitionDate = System.DateTime.Now,
                ExpirationDate = System.DateTime.Now.AddDays(transaction.Product.ShelfLife),
                StorageLocation = "Warehouse", // Default storage
                BatchId = System.Guid.NewGuid().ToString()
            };
            
            AddToInventory(inventoryItem);
            
            // Record financial transaction
            RecordFinancialTransaction(new FinancialRecord
            {
                TransactionType = FinancialTransactionType.Purchase,
                Amount = -transaction.TotalValue,
                Description = $"Purchased {transaction.Quantity}kg of {transaction.Product.ProductName}",
                Timestamp = System.DateTime.Now,
                RelatedTransactionId = transaction.TransactionId
            });
            
            return true;
        }
        
        private bool CompleteSellTransaction(PendingTransaction transaction)
        {
            // Remove from inventory
            if (!RemoveFromInventory(transaction.Product, transaction.Quantity))
                return false;
            
            // Add payment to finances
            ReceivePayment(transaction.TotalValue, transaction.PaymentMethod);
            
            // Update market data
            var marketManager = GameManager.Instance.GetManager<MarketManager>();
            if (marketManager != null)
            {
                // This would update market supply/demand
            }
            
            // Record financial transaction
            RecordFinancialTransaction(new FinancialRecord
            {
                TransactionType = FinancialTransactionType.Sale,
                Amount = transaction.TotalValue,
                Description = $"Sold {transaction.Quantity}kg of {transaction.Product.ProductName}",
                Timestamp = System.DateTime.Now,
                RelatedTransactionId = transaction.TransactionId
            });
            
            return true;
        }
        
        private void UpdateTradingPostStates()
        {
            foreach (var kvp in _tradingPostStates.ToList())
            {
                var state = kvp.Value;
                
                // Random restock
                if ((System.DateTime.Now - state.LastRestockDate).TotalDays >= 7 && Random.Range(0f, 1f) < 0.3f)
                {
                    RestockTradingPost(state);
                }
                
                // Update prices based on market conditions
                UpdateTradingPostPrices(state);
            }
        }
        
        private void UpdateTradingOpportunities()
        {
            // Clear old opportunities
            _availableOpportunities.RemoveAll(op => (System.DateTime.Now - op.DiscoveryDate).TotalDays > 3);
            
            // Generate new opportunities
            if (_tradingSettings != null && Random.Range(0f, 1f) < _tradingSettings.OpportunityGenerationRate)
            {
                GenerateRandomTradingOpportunity();
            }
        }
        
        private void ProcessInventoryDecay()
        {
            for (int i = _playerInventory.InventoryItems.Count - 1; i >= 0; i--)
            {
                var item = _playerInventory.InventoryItems[i];
                
                // Check for expiration
                if (System.DateTime.Now > item.ExpirationDate)
                {
                    // Remove expired items
                    _playerInventory.InventoryItems.RemoveAt(i);
                    _playerInventory.CurrentCapacity -= item.Quantity;
                    
                    OnInventoryChanged?.Invoke(item, -item.Quantity);
                    continue;
                }
                
                // Apply quality decay over time
                float daysSinceAcquisition = (float)(System.DateTime.Now - item.AcquisitionDate).TotalDays;
                float decayRate = item.Product.SpoilageRate;
                
                if (decayRate > 0 && daysSinceAcquisition > 0)
                {
                    float qualityLoss = decayRate * daysSinceAcquisition;
                    item.QualityScore = Mathf.Max(0.1f, item.QualityScore - qualityLoss);
                }
            }
        }
        
        private void UpdateFinancialMetrics()
        {
            // Update financial metrics like cash flow, profit/loss, etc.
            var recentTransactions = _transactionHistory
                .Where(t => (System.DateTime.Now - t.CompletionTime).TotalDays <= 30)
                .ToList();
            
            float monthlyRevenue = recentTransactions
                .Where(t => t.TransactionType == TradingTransactionType.Sale && t.Success)
                .Sum(t => t.TotalValue);
            
            float monthlyExpenses = recentTransactions
                .Where(t => t.TransactionType == TradingTransactionType.Purchase && t.Success)
                .Sum(t => t.TotalValue);
            
            _playerFinances.MonthlyProfit = monthlyRevenue - monthlyExpenses;
        }
        
        private bool CanAffordTransaction(float cost, PaymentMethod paymentMethod)
        {
            switch (paymentMethod.PaymentType)
            {
                case PaymentType.Cash:
                    return _playerFinances.CashOnHand >= cost;
                case PaymentType.Bank_Transfer:
                    return _playerFinances.BankBalance >= cost;
                case PaymentType.Credit:
                    return (_playerFinances.TotalDebt + cost) <= _playerFinances.CreditLimit;
                default:
                    return false;
            }
        }
        
        private bool IsTradingPostAvailable(TradingPost tradingPost, MarketProductSO product, float quantity)
        {
            if (!_tradingPostStates.ContainsKey(tradingPost))
                return false;
            
            var state = _tradingPostStates[tradingPost];
            if (!state.IsActive)
                return false;
            
            var availableProduct = state.AvailableProducts.Find(p => p.Product == product);
            return availableProduct != null && availableProduct.AvailableQuantity >= quantity;
        }
        
        private bool WillTradingPostAccept(TradingPost tradingPost, InventoryItem inventoryItem, float quantity)
        {
            if (!_tradingPostStates.ContainsKey(tradingPost))
                return false;
            
            var state = _tradingPostStates[tradingPost];
            
            // Check quality requirements
            if (inventoryItem.QualityScore < tradingPost.MinimumQualityThreshold)
                return false;
            
            // Check if trading post deals with this product type
            return tradingPost.AcceptedProductTypes.Contains(inventoryItem.Product.Category);
        }
        
        private System.DateTime CalculateTransactionTime(TradingPost tradingPost, PaymentMethod paymentMethod)
        {
            float baseHours = tradingPost.ProcessingTimeHours;
            
            // Payment method affects processing time
            switch (paymentMethod.PaymentType)
            {
                case PaymentType.Cash:
                    baseHours *= 0.5f; // Cash is faster
                    break;
                case PaymentType.Credit:
                    baseHours *= 1.5f; // Credit requires verification
                    break;
            }
            
            return System.DateTime.Now.AddHours(baseHours);
        }
        
        private bool ProcessPayment(float amount, PaymentMethod paymentMethod)
        {
            switch (paymentMethod.PaymentType)
            {
                case PaymentType.Cash:
                    if (_playerFinances.CashOnHand >= amount)
                    {
                        float oldCash = _playerFinances.CashOnHand;
                        _playerFinances.CashOnHand -= amount;
                        OnCashChanged?.Invoke(oldCash, _playerFinances.CashOnHand);
                        return true;
                    }
                    break;
                
                case PaymentType.Bank_Transfer:
                    if (_playerFinances.BankBalance >= amount)
                    {
                        _playerFinances.BankBalance -= amount;
                        return true;
                    }
                    break;
                
                case PaymentType.Credit:
                    if ((_playerFinances.TotalDebt + amount) <= _playerFinances.CreditLimit)
                    {
                        _playerFinances.TotalDebt += amount;
                        return true;
                    }
                    break;
            }
            
            return false;
        }
        
        private void ReceivePayment(float amount, PaymentMethod paymentMethod)
        {
            switch (paymentMethod.PaymentType)
            {
                case PaymentType.Cash:
                case PaymentType.Bank_Transfer:
                default:
                    float oldCash = _playerFinances.CashOnHand;
                    _playerFinances.CashOnHand += amount;
                    OnCashChanged?.Invoke(oldCash, _playerFinances.CashOnHand);
                    break;
            }
        }
        
        private void AddToInventory(InventoryItem item)
        {
            _playerInventory.InventoryItems.Add(item);
            _playerInventory.CurrentCapacity += item.Quantity;
            
            OnInventoryChanged?.Invoke(item, item.Quantity);
            _inventoryChangedEvent?.Raise();
        }
        
        private bool RemoveFromInventory(MarketProductSO product, float quantity)
        {
            float remainingToRemove = quantity;
            var itemsToRemove = new List<InventoryItem>();
            
            // Use FIFO (First In, First Out) for inventory removal
            var sortedItems = _playerInventory.InventoryItems
                .Where(item => item.Product == product)
                .OrderBy(item => item.AcquisitionDate)
                .ToList();
            
            foreach (var item in sortedItems)
            {
                if (remainingToRemove <= 0) break;
                
                if (item.Quantity <= remainingToRemove)
                {
                    // Remove entire item
                    remainingToRemove -= item.Quantity;
                    _playerInventory.CurrentCapacity -= item.Quantity;
                    itemsToRemove.Add(item);
                    OnInventoryChanged?.Invoke(item, -item.Quantity);
                }
                else
                {
                    // Partially remove from item
                    item.Quantity -= remainingToRemove;
                    _playerInventory.CurrentCapacity -= remainingToRemove;
                    OnInventoryChanged?.Invoke(item, -remainingToRemove);
                    remainingToRemove = 0;
                }
            }
            
            // Remove empty items
            foreach (var item in itemsToRemove)
            {
                _playerInventory.InventoryItems.Remove(item);
            }
            
            _inventoryChangedEvent?.Raise();
            return remainingToRemove == 0;
        }
        
        private float CalculateInventoryValue()
        {
            float totalValue = 0f;
            var marketManager = GameManager.Instance.GetManager<MarketManager>();
            
            if (marketManager != null)
            {
                foreach (var item in _playerInventory.InventoryItems)
                {
                    float currentPrice = marketManager.GetCurrentPrice(item.Product, true, item.QualityScore);
                    totalValue += currentPrice * item.Quantity;
                }
            }
            
            return totalValue;
        }
        
        private void RecordFinancialTransaction(FinancialRecord record)
        {
            _playerFinances.TransactionHistory.Add(record);
            
            // Keep only recent history
            var cutoffDate = System.DateTime.Now.AddDays(-365); // Keep 1 year of history
            _playerFinances.TransactionHistory.RemoveAll(r => r.Timestamp < cutoffDate);
            
            _financialStatusChangedEvent?.Raise();
        }
        
        private void RestockTradingPost(TradingPostState state)
        {
            state.AvailableProducts.Clear();
            
            // Generate random product availability based on trading post type
            foreach (var productType in state.TradingPost.AcceptedProductTypes)
            {
                // This would be more sophisticated in a real implementation
                // For now, just add some random products
                if (Random.Range(0f, 1f) < 0.7f) // 70% chance to have each type
                {
                    var product = new TradingPostProduct
                    {
                        // Product would be selected from available products of this type
                        AvailableQuantity = Random.Range(10f, 100f),
                        QualityRange = new Vector2(0.6f, 0.9f),
                        PriceModifier = Random.Range(0.9f, 1.1f)
                    };
                    
                    state.AvailableProducts.Add(product);
                }
            }
            
            state.LastRestockDate = System.DateTime.Now;
        }
        
        private void UpdateTradingPostPrices(TradingPostState state)
        {
            // Update markup based on market conditions and reputation
            float reputationBonus = (state.ReputationWithPlayer - 0.5f) * 0.1f; // ±5% based on reputation
            state.PriceMarkup = Mathf.Clamp(state.PriceMarkup - reputationBonus, 1.0f, 1.5f);
        }
        
        private void GenerateRandomTradingOpportunity()
        {
            var opportunityTypes = new[] { OpportunityType.Bulk_Discount, OpportunityType.Quality_Premium, 
                                         OpportunityType.Urgent_Sale, OpportunityType.Seasonal_Special };
            
            var opportunity = new TradingOpportunity
            {
                OpportunityId = System.Guid.NewGuid().ToString(),
                OpportunityType = opportunityTypes[Random.Range(0, opportunityTypes.Length)],
                Description = GenerateOpportunityDescription(opportunityTypes[Random.Range(0, opportunityTypes.Length)]),
                PriceModifier = Random.Range(0.7f, 1.3f),
                QuantityAvailable = Random.Range(50f, 500f),
                ExpirationDate = System.DateTime.Now.AddDays(Random.Range(1, 7)),
                DiscoveryDate = System.DateTime.Now,
                RequiredReputationLevel = Random.Range(0.3f, 0.8f)
            };
            
            _availableOpportunities.Add(opportunity);
            OnTradingOpportunityAvailable?.Invoke(opportunity);
            _tradingOpportunityEvent?.Raise();
        }
        
        private string GenerateOpportunityDescription(OpportunityType opportunityType)
        {
            switch (opportunityType)
            {
                case OpportunityType.Bulk_Discount:
                    return "Large quantity available at reduced price";
                case OpportunityType.Quality_Premium:
                    return "Premium quality product with higher margins";
                case OpportunityType.Urgent_Sale:
                    return "Urgent sale - below market price";
                case OpportunityType.Seasonal_Special:
                    return "Seasonal product with limited availability";
                default:
                    return "Trading opportunity available";
            }
        }
        
        private float CalculateRecommendationScore(TradingProfitabilityAnalysis analysis)
        {
            float score = 0f;
            
            // Profit margin scoring
            if (analysis.ProfitMargin > 30f) score += 0.4f;
            else if (analysis.ProfitMargin > 15f) score += 0.3f;
            else if (analysis.ProfitMargin > 5f) score += 0.2f;
            
            // Volume scoring
            if (analysis.Quantity > 100f) score += 0.2f;
            else if (analysis.Quantity > 50f) score += 0.1f;
            
            // Risk assessment
            if (analysis.EstimatedProfit > 0) score += 0.2f;
            
            // Market timing (would integrate with market analysis)
            score += 0.2f; // Placeholder
            
            return Mathf.Clamp01(score);
        }
    }
    
    [System.Serializable]
    public class TradingSettings
    {
        [Range(100f, 10000f)] public float StartingCash = 5000f;
        [Range(100f, 5000f)] public float StartingInventoryCapacity = 1000f;
        [Range(1000f, 50000f)] public float StartingCreditLimit = 10000f;
        [Range(0f, 1f)] public float OpportunityGenerationRate = 0.1f;
        public bool EnableInventoryDecay = true;
        public bool EnableDynamicPricing = true;
    }
    
    [System.Serializable]
    public class TransactionSettings
    {
        [Range(0.1f, 24f)] public float DefaultProcessingTimeHours = 2f;
        [Range(0f, 0.1f)] public float TransactionFeeRate = 0.025f;
        [Range(0f, 0.05f)] public float CreditProcessingFee = 0.03f;
        public bool RequireIdentityVerification = true;
        public bool EnableInstantTransactions = false;
    }
    
    [System.Serializable]
    public class PlayerInventory
    {
        public List<InventoryItem> InventoryItems = new List<InventoryItem>();
        public float MaxCapacity = 1000f;
        public float CurrentCapacity = 0f;
        public string DefaultStorageLocation = "Warehouse";
        
        /// <summary>
        /// PC-009-2: Adds harvested product to player inventory
        /// </summary>
        public bool AddHarvestedProduct(MarketProductSO product, float quantity, float qualityScore, string batchId)
        {
            if (CurrentCapacity + quantity > MaxCapacity)
            {
                Debug.LogWarning($"[PlayerInventory] Cannot add {quantity}g - exceeds capacity ({CurrentCapacity + quantity:F1}/{MaxCapacity})");
                return false;
            }
            
            var inventoryItem = new InventoryItem
            {
                Product = product,
                Quantity = quantity,
                QualityScore = qualityScore,
                AcquisitionCost = 0f, // Harvested products have no acquisition cost
                AcquisitionDate = System.DateTime.Now,
                ExpirationDate = System.DateTime.Now.AddDays(product != null ? 365f : 180f), // Default expiration
                StorageLocation = DefaultStorageLocation,
                BatchId = batchId,
                Metadata = new Dictionary<string, object>
                {
                    ["Source"] = "Harvest",
                    ["HarvestDate"] = System.DateTime.Now.ToString("yyyy-MM-dd")
                }
            };
            
            InventoryItems.Add(inventoryItem);
            CurrentCapacity += quantity;
            
            Debug.Log($"[PlayerInventory] Added {quantity:F1}g of {product?.DisplayName ?? "Unknown Product"} (Quality: {qualityScore:F1}%, Batch: {batchId})");
            return true;
        }
        
        /// <summary>
        /// PC-009-3: Gets available quantity of a specific product for selling
        /// </summary>
        public float GetAvailableQuantity(MarketProductSO product)
        {
            return InventoryItems
                .Where(item => item.Product == product)
                .Sum(item => item.Quantity);
        }
        
        /// <summary>
        /// PC-009-3: Removes quantity from inventory (for sales)
        /// </summary>
        public bool RemoveProduct(MarketProductSO product, float quantity)
        {
            var availableItems = InventoryItems
                .Where(item => item.Product == product)
                .OrderBy(item => item.AcquisitionDate) // FIFO - First In, First Out
                .ToList();
            
            float remainingToRemove = quantity;
            var itemsToRemove = new List<InventoryItem>();
            
            foreach (var item in availableItems)
            {
                if (remainingToRemove <= 0) break;
                
                if (item.Quantity <= remainingToRemove)
                {
                    // Remove entire item
                    remainingToRemove -= item.Quantity;
                    CurrentCapacity -= item.Quantity;
                    itemsToRemove.Add(item);
                }
                else
                {
                    // Partial removal
                    item.Quantity -= remainingToRemove;
                    CurrentCapacity -= remainingToRemove;
                    remainingToRemove = 0;
                }
            }
            
            // Remove empty items
            foreach (var item in itemsToRemove)
            {
                InventoryItems.Remove(item);
            }
            
            return remainingToRemove <= 0.001f; // Allow for floating point precision
        }
        
        /// <summary>
        /// Update inventory statistics and process quality degradation
        /// </summary>
        public void UpdateInventoryStatistics()
        {
            CurrentCapacity = InventoryItems.Sum(item => item.Quantity);
            
            // Process quality degradation
            var now = System.DateTime.Now;
            foreach (var item in InventoryItems)
            {
                if (item.LastQualityUpdate < now)
                {
                    var daysSinceUpdate = (now - item.LastQualityUpdate).TotalDays;
                    var degradationAmount = item.DegradationRate * (float)daysSinceUpdate;
                    var newQuality = Mathf.Max(0f, item.QualityScore - degradationAmount);
                    
                    if (newQuality != item.QualityScore)
                    {
                        item.DegradationHistory.Add($"{now:yyyy-MM-dd}: Quality {item.QualityScore:F2} -> {newQuality:F2}");
                        item.QualityScore = newQuality;
                    }
                    
                    item.LastQualityUpdate = now;
                }
            }
        }
    }
    
    [System.Serializable]
    public class PlayerFinances
    {
        public float CashOnHand = 5000f;
        public float BankBalance = 0f;
        public float TotalDebt = 0f;
        public float CreditLimit = 10000f;
        public float MonthlyProfit = 0f;
        public float AccountsReceivable = 0f;
        public float AccountsPayable = 0f;
        public float MonthlyCosts = 0f;
        public List<FinancialRecord> TransactionHistory = new List<FinancialRecord>();
    }
    
    [System.Serializable]
    public class InventoryItem
    {
        public string ItemId = System.Guid.NewGuid().ToString();
        public MarketProductSO Product;
        public float Quantity;
        public float QualityScore;
        public float InitialQualityScore;
        public float AcquisitionCost;
        public System.DateTime AcquisitionDate;
        public System.DateTime ExpirationDate;
        public System.DateTime LastQualityUpdate = System.DateTime.Now;
        public string StorageLocation;
        public string BatchId;
        public float DegradationRate = 0.01f; // 1% per day default
        public List<string> DegradationHistory = new List<string>();
        public Dictionary<string, object> Metadata = new Dictionary<string, object>();
        public BatchTrackingInfo BatchInfo;
        public StorageEnvironment CurrentStorageConditions;
        public List<QualityDegradationRecord> QualityDegradationHistory = new List<QualityDegradationRecord>();
        public List<StorageConditionsHistory> StorageHistory = new List<StorageConditionsHistory>();
        
        public void UpdateStorageConditions(StorageEnvironment conditions)
        {
            CurrentStorageConditions = conditions;
            // Apply storage condition effects on degradation rate
            if (conditions != null)
            {
                float conditionMultiplier = 1.0f;
                
                // Temperature effects
                if (conditions.Temperature > 25f || conditions.Temperature < 15f)
                    conditionMultiplier *= 1.5f;
                    
                // Humidity effects
                if (conditions.Humidity > 65f || conditions.Humidity < 45f)
                    conditionMultiplier *= 1.3f;
                    
                // Light exposure effects
                if (conditions.LightExposure)
                    conditionMultiplier *= 2.0f;
                    
                // Air circulation effects
                if (!conditions.AirCirculation)
                    conditionMultiplier *= 1.2f;
                    
                DegradationRate = 0.01f * conditionMultiplier;
            }
        }
        
        public void CalculateCurrentQuality()
        {
            var now = System.DateTime.Now;
            var daysSinceLastUpdate = (now - LastQualityUpdate).TotalDays;
            
            if (daysSinceLastUpdate > 0)
            {
                var degradationAmount = DegradationRate * (float)daysSinceLastUpdate;
                QualityScore = Mathf.Max(0f, QualityScore - degradationAmount);
                LastQualityUpdate = now;
            }
        }
        
        public void RecordQualityDegradation(float previousQuality, float currentQuality, string reason)
        {
            var degradationEntry = $"{System.DateTime.Now:yyyy-MM-dd HH:mm}: Quality {previousQuality:F2} -> {currentQuality:F2} ({reason})";
            DegradationHistory.Add(degradationEntry);
            
            // Keep history manageable
            if (DegradationHistory.Count > 100)
            {
                DegradationHistory.RemoveAt(0);
            }
        }
    }
    
    [System.Serializable]
    public class BatchTrackingInfo
    {
        public string SourcePlantId;
        public string StrainName;
        public System.DateTime HarvestDate;
        public float HarvestQuality;
        public string ProcessingMethod;
        public List<string> ProcessingHistory = new List<string>();
    }
    
    [System.Serializable]
    public class StorageEnvironment
    {
        public float Temperature = 20f; // Celsius
        public float Humidity = 55f; // Percentage
        public bool LightExposure = false;
        public bool AirCirculation = true;
    }
    
    [System.Serializable]
    public class QualityDegradationRecord
    {
        public System.DateTime Timestamp;
        public float PreviousQuality;
        public float NewQuality;
        public float DegradationAmount;
        public string Reason;
        public StorageEnvironment StorageConditions;
    }
    
    [System.Serializable]
    public class StorageConditionsHistory
    {
        public System.DateTime Timestamp;
        public StorageEnvironment Conditions;
        public string ChangeReason;
        public float QualityImpact;
    }
    
    [System.Serializable]
    public class PlayerReputation
    {
        public float OverallReputation = 0.5f; // 0-1 scale
        public int TransactionCount = 0;
        public float ReliabilityScore = 0.5f; // On-time delivery, quality consistency
        public float QualityScore = 0.5f; // Average quality of products sold
        public float InnovationScore = 0.5f; // Innovation in cultivation methods, new strains
        public float ProfessionalismScore = 0.5f; // Business conduct, communication
        public float ComplianceScore = 0.5f; // Legal and regulatory compliance
        public Dictionary<string, float> TradingPostReputation = new Dictionary<string, float>();
        public List<ReputationEvent> ReputationHistory = new List<ReputationEvent>();
        
        // Computed property for overall score calculation
        public float OverallScore 
        { 
            get 
            { 
                return (ReliabilityScore + QualityScore + InnovationScore + ProfessionalismScore + ComplianceScore) / 5f;
            } 
        }
        
        // Methods for score updates
        public void UpdateInnovationScore(float delta, string reason)
        {
            InnovationScore = Mathf.Clamp01(InnovationScore + delta);
            RecordReputationChange("Innovation", delta, reason);
            UpdateOverallReputation();
        }
        
        public void UpdateProfessionalismScore(float delta, string reason)
        {
            ProfessionalismScore = Mathf.Clamp01(ProfessionalismScore + delta);
            RecordReputationChange("Professionalism", delta, reason);
            UpdateOverallReputation();
        }
        
        public void UpdateComplianceScore(float delta, string reason)
        {
            ComplianceScore = Mathf.Clamp01(ComplianceScore + delta);
            RecordReputationChange("Compliance", delta, reason);
            UpdateOverallReputation();
        }
        
        private void RecordReputationChange(string eventType, float change, string description)
        {
            ReputationHistory.Add(new ReputationEvent
            {
                Timestamp = System.DateTime.Now,
                EventType = eventType,
                ReputationChange = change,
                Description = description,
                TradingPostId = "GLOBAL"
            });
            
            // Keep history manageable
            if (ReputationHistory.Count > 200)
            {
                ReputationHistory.RemoveAt(0);
            }
        }
        
        private void UpdateOverallReputation()
        {
            OverallReputation = OverallScore;
        }
    }
    
    [System.Serializable]
    public class ReputationEvent
    {
        public System.DateTime Timestamp;
        public string EventType;
        public float ReputationChange;
        public string Description;
        public string TradingPostId;
    }
    
    [System.Serializable]
    public class TradingPost
    {
        public string TradingPostName;
        public TradingPostType PostType;
        public List<ProductCategory> AcceptedProductTypes = new List<ProductCategory>();
        public float MinimumQualityThreshold = 0.6f;
        public float ProcessingTimeHours = 2f;
        public Vector2 Location;
        public string Description;
    }
    
    [System.Serializable]
    public class TradingPostState
    {
        public TradingPost TradingPost;
        public bool IsActive = true;
        public float PriceMarkup = 1.1f;
        public float CommissionRate = 0.05f;
        public float ReputationWithPlayer = 0.5f;
        public List<TradingPostProduct> AvailableProducts = new List<TradingPostProduct>();
        public System.DateTime LastRestockDate;
    }
    
    [System.Serializable]
    public class TradingPostProduct
    {
        public MarketProductSO Product;
        public float AvailableQuantity;
        public Vector2 QualityRange;
        public float PriceModifier = 1f;
          }
      
      [System.Serializable]
      public class PaymentMethod
      {
          public PaymentType PaymentType;
          public string MethodName;
          public float ProcessingFeeRate = 0.025f;
          public float ProcessingTimeHours = 1f;
          public bool IsInstant = false;
      }
      
      [System.Serializable]
    public class PendingTransaction
    {
        public string TransactionId;
        public TradingTransactionType TransactionType;
        public MarketProductSO Product;
        public float Quantity;
        public float UnitPrice;
        public float TotalValue;
        public TradingPost TradingPost;
        public PaymentMethod PaymentMethod;
        public System.DateTime InitiationTime;
        public System.DateTime EstimatedCompletionTime;
        public TransactionStatus Status;
        public InventoryItem SourceInventoryItem; // For sell transactions
    }
    
    [System.Serializable]
    public class CompletedTransaction
    {
        public string TransactionId;
        public TradingTransactionType TransactionType;
        public MarketProductSO Product;
        public float Quantity;
        public float UnitPrice;
        public float TotalValue;
        public TradingPost TradingPost;
        public PaymentMethod PaymentMethod;
        public System.DateTime CompletionTime;
        public bool Success;
        public float QualityScore;
        public string Notes;
    }
    
    [System.Serializable]
    public class TransactionResult
    {
        public bool Success;
        public string TransactionId;
        public MarketProductSO Product;
        public float Quantity;
        public float UnitPrice;
        public float TotalValue;
        public TradingPost TradingPost;
        public System.DateTime EstimatedCompletionTime;
        public string ErrorMessage;
    }
    
    [System.Serializable]
    public class TradingOpportunity
    {
        public string OpportunityId;
        public OpportunityType OpportunityType;
        public MarketProductSO Product;
        public string Description;
        public float PriceModifier;
        public float QuantityAvailable;
        public System.DateTime ExpirationDate;
        public System.DateTime DiscoveryDate;
        public float RequiredReputationLevel;
    }
    
    [System.Serializable]
    public class TradingProfitabilityAnalysis
    {
        public MarketProductSO Product;
        public float Quantity;
        public TradingTransactionType TransactionType;
        public float EstimatedCost;
        public float EstimatedRevenue;
        public float EstimatedProfit;
        public float ProfitMargin; // Percentage
        public float RecommendationScore;
        public bool IsAnalysisValid;
        public string RecommendationReason;
    }
    
    [System.Serializable]
    public class FinancialRecord
    {
        public FinancialTransactionType TransactionType;
        public float Amount;
        public string Description;
        public System.DateTime Timestamp;
        public string RelatedTransactionId;
        public string Category;
    }
    
    [System.Serializable]
    public class ProductTradingData
    {
        public MarketProductSO Product;
        public float AveragePrice = 0f;
        public float TotalVolume = 0f;
        public int TransactionCount = 0;
        public System.DateTime LastTradedDate;
        public List<float> PriceHistory = new List<float>();
    }
    
    public enum TradingPostType
    {
        Wholesale_Distributor,
        Retail_Dispensary,
        Processing_Facility,
        Online_Marketplace,
        Farmers_Market,
        Auction_House,
        Private_Buyer
          }
      
      public enum PaymentType
      {
          Cash,
          Bank_Transfer,
          Credit,
          Cryptocurrency,
          Barter,
          Consignment
      }
      
      public enum TransactionStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Cancelled
    }
    
    public enum OpportunityType
    {
        All,
        Bulk_Discount,
        Quality_Premium,
        Urgent_Sale,
        Seasonal_Special,
        New_Market,
        Liquidation
    }
    
    public enum CashTransferType
    {
        Cash_To_Bank,
        Bank_To_Cash
    }
    
    public enum FinancialTransactionType
    {
        Sale,
        Purchase,
        Investment,
        Loan_Payment,
        Interest_Earned,
        Fee_Paid,
        Tax_Payment,
        Transfer
    }
}