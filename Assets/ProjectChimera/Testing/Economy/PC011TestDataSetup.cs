using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Systems.Economy;
using ProjectChimera.Data.Economy;
// Explicit alias to resolve InventoryItem ambiguity
using InventoryItem = ProjectChimera.Systems.Economy.InventoryItem;

namespace ProjectChimera.Testing.Economy
{
    /// <summary>
    /// PC-011 Test Data Setup - Creates test data for PC-011 testing
    /// </summary>
    public class PC011TestDataSetup : MonoBehaviour
    {
        [Header("Test Data Configuration")]
        [SerializeField] private bool _createTestDataOnStart = true;
        [SerializeField] private int _testProductCount = 5;
        [SerializeField] private int _testInventoryItemCount = 10;
        
        [Header("Generated Test Data")]
        [SerializeField] private List<MarketProductSO> _testProducts = new List<MarketProductSO>();
        [SerializeField] private List<InventoryItem> _testInventoryItems = new List<InventoryItem>();
        
        private void Start()
        {
            if (_createTestDataOnStart)
            {
                CreateTestData();
            }
        }
        
        [ContextMenu("Create Test Data")]
        public void CreateTestData()
        {
            Debug.Log("[PC011TestDataSetup] Creating test data...");
            
            CreateTestProducts();
            CreateTestInventoryItems();
            
            Debug.Log($"[PC011TestDataSetup] Created {_testProducts.Count} test products and {_testInventoryItems.Count} test inventory items");
        }
        
        private void CreateTestProducts()
        {
            _testProducts.Clear();
            
            for (int i = 0; i < _testProductCount; i++)
            {
                var product = ScriptableObject.CreateInstance<MarketProductSO>();
                product.name = $"TestProduct_{i:D2}";
                _testProducts.Add(product);
            }
        }
        
        private void CreateTestInventoryItems()
        {
            _testInventoryItems.Clear();
            
            for (int i = 0; i < _testInventoryItemCount; i++)
            {
                var item = new InventoryItem
                {
                    ItemId = System.Guid.NewGuid().ToString(),
                    Product = _testProducts.Count > 0 ? _testProducts[i % _testProducts.Count] : null,
                    Quantity = Random.Range(0.1f, 5f),
                    QualityScore = Random.Range(0.3f, 1f),
                    InitialQualityScore = Random.Range(0.8f, 1f),
                    AcquisitionDate = System.DateTime.Now.AddDays(-Random.Range(0, 30)),
                    AcquisitionCost = Random.Range(20f, 100f),
                    StorageLocation = $"storage_zone_{Random.Range(1, 4)}",
                    BatchId = $"BATCH_{System.DateTime.Now:yyyyMMdd}_{Random.Range(1000, 9999)}",
                    DegradationRate = Random.Range(0.001f, 0.01f),
                    LastQualityUpdate = System.DateTime.Now,
                    QualityDegradationHistory = new List<QualityDegradationRecord>(),
                    StorageHistory = new List<StorageConditionsHistory>()
                };
                
                _testInventoryItems.Add(item);
            }
        }
        
        [ContextMenu("Clear Test Data")]
        public void ClearTestData()
        {
            _testProducts.Clear();
            _testInventoryItems.Clear();
            Debug.Log("[PC011TestDataSetup] Test data cleared");
        }
        
        public List<MarketProductSO> GetTestProducts()
        {
            return _testProducts;
        }
        
        public List<InventoryItem> GetTestInventoryItems()
        {
            return _testInventoryItems;
        }
    }
}