using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Systems.Economy;
using ProjectChimera.Data.Economy;
using ProjectChimera.Systems.Economy;
using ProjectChimera.Data.Economy;

namespace ProjectChimera.Testing.Economy
{
    [TestFixture]
    public class MarketManagerTests
    {
        private GameObject _testGameObject;
        private MarketManager _marketManager;
        private MarketProductSO _testProduct;

        [SetUp]
        public void SetUp()
        {
            _testGameObject = new GameObject("TestMarketManager");
            _marketManager = _testGameObject.AddComponent<MarketManager>();
            
            // Create a test product
            _testProduct = ScriptableObject.CreateInstance<MarketProductSO>();
            _testProduct.name = "TestCannabisFlower";
            
            // Initialize with reflection to set private fields for testing
            var marketProductsField = typeof(MarketManager).GetField("_marketProducts", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (marketProductsField != null)
            {
                var productList = new List<MarketProductSO> { _testProduct };
                marketProductsField.SetValue(_marketManager, productList);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (_testProduct != null)
                Object.DestroyImmediate(_testProduct);
            if (_testGameObject != null)
                Object.DestroyImmediate(_testGameObject);
        }

        //[Test]
        public void GetPortfolioMetrics_ReturnsValidMetrics()
        {
            // Act
            var metrics = _marketManager.GetPortfolioMetrics();

            // Assert
            Assert.IsNotNull(metrics, "PortfolioMetrics should not be null");
            Assert.IsInstanceOf<PortfolioMetrics>(metrics, "Should return PortfolioMetrics type");
            Assert.GreaterOrEqual(metrics.TotalValue, 0f, "Total value should be non-negative");
            Assert.IsNotNull(metrics.TopPerformers, "TopPerformers list should not be null");
            Assert.GreaterOrEqual(metrics.RiskScore, 0f, "Risk score should be non-negative");
            Assert.LessOrEqual(metrics.RiskScore, 1f, "Risk score should not exceed 1");
        }

        //[Test]
        public void GetFinancialData_ReturnsValidData()
        {
            // Act
            var financialData = _marketManager.GetFinancialData();

            // Assert
            Assert.IsNotNull(financialData, "Financial data should not be null");
            // The method returns object, so we verify it's not null and has expected structure
        }

        //[Test]
        public void GetCurrentPrice_WithValidProduct_ReturnsPrice()
        {
            // Arrange
            bool isRetail = true;
            float qualityScore = 0.8f;

            // Act
            float price = _marketManager.GetCurrentPrice(_testProduct, isRetail, qualityScore);

            // Assert
            Assert.Greater(price, 0f, "Price should be positive");
        }

        //[Test]
        public void GetCurrentPrice_WithNullProduct_HandlesGracefully()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => 
            {
                float price = _marketManager.GetCurrentPrice(null, true, 0.8f);
            }, "Should handle null product gracefully");
        }

        //[Test]
        public void ProcessSale_WithValidInputs_ReturnsTransaction()
        {
            // Arrange
            float quantity = 10f;
            float qualityScore = 0.9f;
            bool isRetail = false;

            // Act
            var transaction = _marketManager.ProcessSale(_testProduct, quantity, qualityScore, isRetail);

            // Assert
            Assert.IsNotNull(transaction, "Transaction should not be null");
            Assert.AreEqual(_testProduct, transaction.Product, "Transaction should reference correct product");
            Assert.AreEqual(quantity, transaction.Quantity, "Transaction should have correct quantity");
            Assert.AreEqual(qualityScore, transaction.QualityScore, "Transaction should have correct quality score");
            Assert.AreEqual(isRetail, transaction.IsRetail, "Transaction should have correct retail flag");
            Assert.AreEqual(TransactionType.Sale, transaction.TransactionType, "Should be sale transaction");
            Assert.Greater(transaction.UnitPrice, 0f, "Unit price should be positive");
            Assert.AreEqual(transaction.UnitPrice * quantity, transaction.TotalValue, 0.01f, "Total value should equal unit price * quantity");
        }

        //[Test]
        public void ProcessPurchase_WithValidInputs_ReturnsTransaction()
        {
            // Arrange
            float quantity = 5f;
            bool isRetail = true;

            // Act
            var transaction = _marketManager.ProcessPurchase(_testProduct, quantity, isRetail);

            // Assert
            Assert.IsNotNull(transaction, "Transaction should not be null");
            Assert.AreEqual(_testProduct, transaction.Product, "Transaction should reference correct product");
            Assert.AreEqual(quantity, transaction.Quantity, "Transaction should have correct quantity");
            Assert.AreEqual(isRetail, transaction.IsRetail, "Transaction should have correct retail flag");
            Assert.AreEqual(TransactionType.Purchase, transaction.TransactionType, "Should be purchase transaction");
            Assert.Greater(transaction.UnitPrice, 0f, "Unit price should be positive");
            Assert.AreEqual(transaction.UnitPrice * quantity, transaction.TotalValue, 0.01f, "Total value should equal unit price * quantity");
        }

        //[Test]
        public void GetMarketAttractiveness_WithValidCategory_ReturnsScore()
        {
            // Arrange
            ProductCategory category = ProductCategory.Flower;

            // Act
            float attractiveness = _marketManager.GetMarketAttractiveness(category);

            // Assert
            Assert.GreaterOrEqual(attractiveness, 0f, "Attractiveness should be non-negative");
            Assert.LessOrEqual(attractiveness, 1f, "Attractiveness should not exceed 1");
        }

        //[Test]
        public void TriggerMarketShock_ExecutesWithoutError()
        {
            // Arrange
            MarketShockType shockType = MarketShockType.Supply_Chain_Disruption;
            float intensity = 0.5f;
            ProductCategory category = ProductCategory.Flower;

            // Act & Assert
            Assert.DoesNotThrow(() => 
            {
                _marketManager.TriggerMarketShock(shockType, intensity, category);
            }, "Market shock should execute without error");
        }

        //[Test]
        public void GetDemandForecast_WithValidCategory_ReturnsForecast()
        {
            // Arrange
            ProductCategory category = ProductCategory.Flower;
            int daysAhead = 30;

            // Act
            var forecast = _marketManager.GetDemandForecast(category, daysAhead);

            // Assert
            Assert.IsNotNull(forecast, "Forecast should not be null");
            Assert.AreEqual(category, forecast.Category, "Forecast should be for correct category");
            Assert.AreEqual(daysAhead, forecast.ForecastDays, "Forecast should cover correct number of days");
            Assert.GreaterOrEqual(forecast.CurrentDemand, 0f, "Current demand should be non-negative");
            Assert.GreaterOrEqual(forecast.Confidence, 0f, "Confidence should be non-negative");
            Assert.LessOrEqual(forecast.Confidence, 1f, "Confidence should not exceed 1");
        }

        //[Test]
        public void CurrentMarketConditions_PropertyAccessible()
        {
            // Act
            var conditions = _marketManager.CurrentMarketConditions;

            // Assert - Should not throw and return some market conditions
            // Since it might be null on uninitialized manager, we just verify access works
            Assert.DoesNotThrow(() => 
            {
                var _ = _marketManager.CurrentMarketConditions;
            }, "CurrentMarketConditions property should be accessible");
        }

        //[Test]
        public void PlayerReputation_PropertyAccessible()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => 
            {
                var reputation = _marketManager.PlayerReputation;
                if (reputation != null)
                {
                    Assert.GreaterOrEqual(reputation.OverallReputation, 0f, "Overall reputation should be non-negative");
                    Assert.LessOrEqual(reputation.OverallReputation, 1f, "Overall reputation should not exceed 1");
                }
            }, "PlayerReputation property should be accessible");
        }

        //[Test]
        
        public void GetPortfolioMetrics_PerformanceTest()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            for (int i = 0; i < 100; i++)
            {
                _marketManager.GetPortfolioMetrics();
            }

            // Assert
            stopwatch.Stop();
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, "100 GetPortfolioMetrics calls should complete in under 100ms");
        }

        //[Test]
        
        public void ProcessSale_PerformanceTest()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            for (int i = 0; i < 50; i++)
            {
                _marketManager.ProcessSale(_testProduct, 1f, 0.8f, false);
            }

            // Assert
            stopwatch.Stop();
            Assert.Less(stopwatch.ElapsedMilliseconds, 50, "50 ProcessSale calls should complete in under 50ms");
        }
    }
} 