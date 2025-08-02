using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Economy;
using ProjectChimera.Systems.Registry;

namespace ProjectChimera.Systems.Services.Economy
{
    /// <summary>
    /// PC014-4b: Trading Post Management Service
    /// Handles trading post operations, availability, and state management
    /// Decomposed from TradingManager (500 lines target)
    /// </summary>
    public class TradingPostManagementService : MonoBehaviour, ITradingPostManagementService
    {
        #region Properties
        
        public bool IsInitialized { get; private set; }
        
        #endregion

        #region Private Fields
        
        [Header("Trading Post Configuration")]
        [SerializeField] private bool _enableTradingPosts = true;
        [SerializeField] private float _restockInterval = 24f; // Hours
        [SerializeField] private float _priceUpdateInterval = 6f; // Hours
        [SerializeField] private int _maxTradingPosts = 20;
        
        [Header("Trading Post Data")]
        [SerializeField] private List<TradingPost> _availableTradingPosts = new List<TradingPost>();
        [SerializeField] private Dictionary<TradingPost, TradingPostState> _tradingPostStates = new Dictionary<TradingPost, TradingPostState>();
        [SerializeField] private List<TradingOpportunity> _currentOpportunities = new List<TradingOpportunity>();
        
        [Header("Market Settings")]
        [SerializeField] private float _basePriceMarkup = 1.1f;
        [SerializeField] private float _maxPriceMarkup = 1.5f;
        [SerializeField] private float _reputationPriceModifier = 0.1f;
        
        private float _lastRestockUpdate;
        private float _lastPriceUpdate;
        
        #endregion

        #region Events
        
        public event Action<TradingPost, TradingPostState> OnTradingPostStateChanged;
        public event Action<TradingOpportunity> OnTradingOpportunityCreated;
        public event Action<TradingPost> OnTradingPostRestocked;
        public event Action<TradingPost, float> OnTradingPostPricesUpdated;
        
        #endregion

        #region IService Implementation
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            Debug.Log("Initializing TradingPostManagementService...");
            
            // Initialize collections
            InitializeTradingPostSystem();
            
            // Load trading posts
            InitializeTradingPosts();
            
            // Generate initial opportunities
            GenerateInitialOpportunities();
            
            // Register with ServiceRegistry
            ServiceRegistry.Instance.RegisterService<ITradingPostManagementService>(this, ServiceDomain.Economy);
            
            IsInitialized = true;
            Debug.Log("TradingPostManagementService initialized successfully");
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            Debug.Log("Shutting down TradingPostManagementService...");
            
            // Save trading post states
            SaveTradingPostStates();
            
            // Clear collections
            _availableTradingPosts.Clear();
            _tradingPostStates.Clear();
            _currentOpportunities.Clear();
            
            IsInitialized = false;
            Debug.Log("TradingPostManagementService shutdown complete");
        }
        
        #endregion

        #region Trading Post Management
        
        public List<TradingPost> GetAvailableTradingPosts()
        {
            return _availableTradingPosts.Where(tp => tp.IsActive).ToList();
        }

        public TradingPostState GetTradingPostState(TradingPost tradingPost)
        {
            return _tradingPostStates.GetValueOrDefault(tradingPost, null);
        }

        public bool IsTradingPostAvailable(TradingPost tradingPost, MarketProductSO product, float quantity)
        {
            if (!_enableTradingPosts || tradingPost == null || !tradingPost.IsActive)
                return false;

            var state = GetTradingPostState(tradingPost);
            if (state == null)
                return false;

            // Check if trading post accepts this product type
            if (!tradingPost.AcceptedProductTypes.Contains(product.ProductType))
                return false;

            // Check available quantity
            var availableProduct = state.AvailableProducts.FirstOrDefault(p => p.Product == product);
            if (availableProduct == null || availableProduct.AvailableQuantity < quantity)
                return false;

            return true;
        }

        public float GetTradingPostPrice(TradingPost tradingPost, MarketProductSO product, float quantity, bool isBuying)
        {
            var state = GetTradingPostState(tradingPost);
            if (state == null)
                return 0f;

            float basePrice = product.BasePrice;
            float markup = isBuying ? state.PriceMarkup : (1f / state.PriceMarkup);
            
            return basePrice * markup * quantity;
        }

        public bool ReserveTradingPostProduct(TradingPost tradingPost, MarketProductSO product, float quantity)
        {
            if (!IsTradingPostAvailable(tradingPost, product, quantity))
                return false;

            var state = GetTradingPostState(tradingPost);
            var availableProduct = state.AvailableProducts.FirstOrDefault(p => p.Product == product);
            
            if (availableProduct != null)
            {
                availableProduct.AvailableQuantity -= quantity;
                OnTradingPostStateChanged?.Invoke(tradingPost, state);
                Debug.Log($"Reserved {quantity} of {product.ProductName} at {tradingPost.TradingPostName}");
                return true;
            }

            return false;
        }

        public void UpdateTradingPostReputation(TradingPost tradingPost, float reputationChange)
        {
            var state = GetTradingPostState(tradingPost);
            if (state != null)
            {
                state.ReputationWithPlayer = Mathf.Clamp01(state.ReputationWithPlayer + reputationChange);
                UpdateTradingPostPrices(state);
                OnTradingPostStateChanged?.Invoke(tradingPost, state);
                Debug.Log($"Updated reputation with {tradingPost.TradingPostName}: {state.ReputationWithPlayer:F2}");
            }
        }
        
        #endregion

        #region Trading Opportunities
        
        public List<TradingOpportunity> GetCurrentTradingOpportunities()
        {
            return _currentOpportunities.Where(o => o.IsActive && o.ExpirationDate > DateTime.Now).ToList();
        }

        public TradingOpportunity GetTradingOpportunity(string opportunityId)
        {
            return _currentOpportunities.FirstOrDefault(o => o.OpportunityId == opportunityId);
        }

        public bool ClaimTradingOpportunity(string opportunityId, string playerId)
        {
            var opportunity = GetTradingOpportunity(opportunityId);
            if (opportunity == null || !opportunity.IsActive)
                return false;

            opportunity.IsActive = false;
            opportunity.ClaimedBy = playerId;
            opportunity.ClaimedDate = DateTime.Now;

            Debug.Log($"Player {playerId} claimed trading opportunity: {opportunity.OpportunityType}");
            return true;
        }

        public void GenerateTradingOpportunity()
        {
            var opportunityTypes = new[] { 
                OpportunityType.Bulk_Discount, 
                OpportunityType.Quality_Premium, 
                OpportunityType.Urgent_Sale, 
                OpportunityType.Seasonal_Special 
            };

            var selectedType = opportunityTypes[UnityEngine.Random.Range(0, opportunityTypes.Length)];
            var tradingPost = _availableTradingPosts[UnityEngine.Random.Range(0, _availableTradingPosts.Count)];

            var opportunity = new TradingOpportunity
            {
                OpportunityId = Guid.NewGuid().ToString(),
                OpportunityType = selectedType,
                TradingPost = tradingPost,
                PriceModifier = GetOpportunityPriceModifier(selectedType),
                QualityRequirement = GetOpportunityQualityRequirement(selectedType),
                MinQuantity = UnityEngine.Random.Range(10f, 50f),
                MaxQuantity = UnityEngine.Random.Range(50f, 200f),
                ExpirationDate = DateTime.Now.AddHours(UnityEngine.Random.Range(6, 72)),
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            _currentOpportunities.Add(opportunity);
            OnTradingOpportunityCreated?.Invoke(opportunity);
            Debug.Log($"Generated trading opportunity: {selectedType} at {tradingPost.TradingPostName}");
        }
        
        #endregion

        #region Private Helper Methods
        
        private void InitializeTradingPostSystem()
        {
            if (_tradingPostStates == null)
                _tradingPostStates = new Dictionary<TradingPost, TradingPostState>();
            
            if (_currentOpportunities == null)
                _currentOpportunities = new List<TradingOpportunity>();
            
            Debug.Log("Trading post system initialized");
        }

        private void InitializeTradingPosts()
        {
            if (_availableTradingPosts.Count == 0)
            {
                CreateDefaultTradingPosts();
            }

            // Initialize states for all trading posts
            foreach (var tradingPost in _availableTradingPosts)
            {
                if (!_tradingPostStates.ContainsKey(tradingPost))
                {
                    _tradingPostStates[tradingPost] = CreateDefaultTradingPostState(tradingPost);
                }
            }

            Debug.Log($"Initialized {_availableTradingPosts.Count} trading posts");
        }

        private void CreateDefaultTradingPosts()
        {
            // Create sample trading posts - would be loaded from data in production
            var dispensary = new TradingPost
            {
                TradingPostId = "dispensary_001",
                TradingPostName = "Green Valley Dispensary",
                TradingPostType = TradingPostType.Dispensary,
                Location = "Downtown",
                AcceptedProductTypes = new List<ProductType> { ProductType.Flower, ProductType.Concentrate },
                IsActive = true
            };

            var processor = new TradingPost
            {
                TradingPostId = "processor_001",
                TradingPostName = "Premium Extracts Co",
                TradingPostType = TradingPostType.Processor,
                Location = "Industrial District",
                AcceptedProductTypes = new List<ProductType> { ProductType.Biomass, ProductType.Trim },
                IsActive = true
            };

            _availableTradingPosts.Add(dispensary);
            _availableTradingPosts.Add(processor);
        }

        private TradingPostState CreateDefaultTradingPostState(TradingPost tradingPost)
        {
            var state = new TradingPostState
            {
                TradingPost = tradingPost,
                IsOpen = true,
                PriceMarkup = _basePriceMarkup,
                ReputationWithPlayer = 0.5f,
                LastRestockDate = DateTime.Now.AddHours(-UnityEngine.Random.Range(1, 12)),
                AvailableProducts = new List<TradingPostProduct>()
            };

            RestockTradingPost(state);
            return state;
        }

        private void GenerateInitialOpportunities()
        {
            // Generate 2-5 initial opportunities
            int numOpportunities = UnityEngine.Random.Range(2, 6);
            for (int i = 0; i < numOpportunities; i++)
            {
                GenerateTradingOpportunity();
            }
        }

        private void SaveTradingPostStates()
        {
            // TODO: Implement persistent storage
            Debug.Log("Saving trading post states...");
        }

        private void RestockTradingPost(TradingPostState state)
        {
            state.AvailableProducts.Clear();

            foreach (var productType in state.TradingPost.AcceptedProductTypes)
            {
                if (UnityEngine.Random.Range(0f, 1f) < 0.7f) // 70% chance to have each type
                {
                    var product = new TradingPostProduct
                    {
                        AvailableQuantity = UnityEngine.Random.Range(10f, 100f),
                        QualityRange = new Vector2(0.6f, 0.9f),
                        PriceModifier = UnityEngine.Random.Range(0.9f, 1.1f)
                    };

                    state.AvailableProducts.Add(product);
                }
            }

            state.LastRestockDate = DateTime.Now;
            OnTradingPostRestocked?.Invoke(state.TradingPost);
            Debug.Log($"Restocked trading post: {state.TradingPost.TradingPostName}");
        }

        private void UpdateTradingPostPrices(TradingPostState state)
        {
            float reputationBonus = (state.ReputationWithPlayer - 0.5f) * _reputationPriceModifier;
            float oldMarkup = state.PriceMarkup;
            state.PriceMarkup = Mathf.Clamp(oldMarkup - reputationBonus, 1.0f, _maxPriceMarkup);
            
            OnTradingPostPricesUpdated?.Invoke(state.TradingPost, state.PriceMarkup);
        }

        private float GetOpportunityPriceModifier(OpportunityType type)
        {
            return type switch
            {
                OpportunityType.Bulk_Discount => 0.8f,
                OpportunityType.Quality_Premium => 1.3f,
                OpportunityType.Urgent_Sale => 0.7f,
                OpportunityType.Seasonal_Special => 0.9f,
                _ => 1.0f
            };
        }

        private float GetOpportunityQualityRequirement(OpportunityType type)
        {
            return type switch
            {
                OpportunityType.Quality_Premium => 0.8f,
                OpportunityType.Seasonal_Special => 0.7f,
                _ => 0.5f
            };
        }
        
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        private void Update()
        {
            if (!IsInitialized || !_enableTradingPosts) return;

            float currentTime = Time.time;

            // Update trading post restocking
            if (currentTime - _lastRestockUpdate >= _restockInterval * 3600f) // Convert hours to seconds
            {
                UpdateTradingPostRestocking();
                _lastRestockUpdate = currentTime;
            }

            // Update trading post prices
            if (currentTime - _lastPriceUpdate >= _priceUpdateInterval * 3600f)
            {
                UpdateAllTradingPostPrices();
                _lastPriceUpdate = currentTime;
            }

            // Clean up expired opportunities
            CleanupExpiredOpportunities();
        }

        private void UpdateTradingPostRestocking()
        {
            foreach (var kvp in _tradingPostStates)
            {
                var state = kvp.Value;
                if ((DateTime.Now - state.LastRestockDate).TotalHours >= _restockInterval)
                {
                    RestockTradingPost(state);
                }
            }
        }

        private void UpdateAllTradingPostPrices()
        {
            foreach (var state in _tradingPostStates.Values)
            {
                UpdateTradingPostPrices(state);
            }
        }

        private void CleanupExpiredOpportunities()
        {
            _currentOpportunities.RemoveAll(o => o.ExpirationDate < DateTime.Now);
        }
        
        #endregion
    }
}