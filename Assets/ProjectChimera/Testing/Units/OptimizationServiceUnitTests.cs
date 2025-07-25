using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Core.Optimization;
using ProjectChimera.Testing.Core;
using ProjectChimera.Systems.SpeedTree;

namespace ProjectChimera.Testing.Units
{
    /// <summary>
    /// PC013-22: Unit tests for optimization service classes
    /// Tests object pooling, batch processing, and purge optimization systems
    /// </summary>
    [TestFixture]
    [Category("Unit Tests")]
    [Category("Optimization Services")]
    public class OptimizationServiceUnitTests : ChimeraTestBase
    {
        [SetUp]
        public void SetUp()
        {
            SetupTestEnvironment();
        }
        
        [TearDown]
        public void TearDown()
        {
            CleanupTestEnvironment();
        }
        
        #region ObjectPurgeManager Tests
        
        [Test]
        public void ObjectPurgeManager_Initialize_SetsUpCorrectly()
        {
            // Arrange
            var manager = new GameObject("TestObjectPurgeManager").AddComponent<ObjectPurgeManager>();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => manager.Initialize());
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(manager.gameObject);
        }
        
        [Test]
        public void ObjectPurgeManager_Instance_IsSingleton()
        {
            // Arrange
            var manager1 = new GameObject("TestObjectPurgeManager1").AddComponent<ObjectPurgeManager>();
            var manager2 = new GameObject("TestObjectPurgeManager2").AddComponent<ObjectPurgeManager>();
            
            manager1.Initialize();
            manager2.Initialize();
            
            // Act
            var instance1 = ObjectPurgeManager.Instance;
            var instance2 = ObjectPurgeManager.Instance;
            
            // Assert
            Assert.IsNotNull(instance1, "Instance should not be null");
            Assert.AreSame(instance1, instance2, "Should return same singleton instance");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(manager1.gameObject);
            UnityEngine.Object.DestroyImmediate(manager2.gameObject);
        }
        
        [Test]
        public void ObjectPurgeManager_PurgeUnusedObjects_ExecutesWithoutErrors()
        {
            // Arrange
            var manager = new GameObject("TestObjectPurgeManager").AddComponent<ObjectPurgeManager>();
            manager.Initialize();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => manager.PurgeUnusedObjects());
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(manager.gameObject);
        }
        
        #endregion
        
        #region PlantUpdateOptimizer Tests
        
        [Test]
        public void PlantUpdateOptimizer_Initialize_SetsUpCorrectly()
        {
            // Arrange
            var optimizer = new GameObject("TestPlantUpdateOptimizer").AddComponent<PlantUpdateOptimizer>();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => optimizer.Initialize());
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(optimizer.gameObject);
        }
        
        [Test]
        public void PlantUpdateOptimizer_Instance_IsSingleton()
        {
            // Arrange
            var optimizer1 = new GameObject("TestPlantUpdateOptimizer1").AddComponent<PlantUpdateOptimizer>();
            var optimizer2 = new GameObject("TestPlantUpdateOptimizer2").AddComponent<PlantUpdateOptimizer>();
            
            optimizer1.Initialize();
            optimizer2.Initialize();
            
            // Act
            var instance1 = PlantUpdateOptimizer.Instance;
            var instance2 = PlantUpdateOptimizer.Instance;
            
            // Assert
            Assert.IsNotNull(instance1, "Instance should not be null");
            Assert.AreSame(instance1, instance2, "Should return same singleton instance");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(optimizer1.gameObject);
            UnityEngine.Object.DestroyImmediate(optimizer2.gameObject);
        }
        
        [Test]
        public void PlantUpdateOptimizer_OptimizePlantUpdates_HandlesEmptyList()
        {
            // Arrange
            var optimizer = new GameObject("TestPlantUpdateOptimizer").AddComponent<PlantUpdateOptimizer>();
            optimizer.Initialize();
            var emptyList = new List<GameObject>();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => optimizer.OptimizePlantUpdates(emptyList));
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(optimizer.gameObject);
        }
        
        [Test]
        public void PlantUpdateOptimizer_GetOptimizerStats_ReturnsValidStats()
        {
            // Arrange
            var optimizer = new GameObject("TestPlantUpdateOptimizer").AddComponent<PlantUpdateOptimizer>();
            optimizer.Initialize();
            
            // Act
            var stats = optimizer.GetOptimizerStats();
            
            // Assert
            Assert.IsNotNull(stats, "Stats should not be null");
            Assert.GreaterOrEqual(stats.CurrentUpdateTime, 0f, "Update time should be non-negative");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(optimizer.gameObject);
        }
        
        #endregion
        
        #region PlantBatchProcessor Tests
        
        [Test]
        public void PlantBatchProcessor_Initialize_SetsUpCorrectly()
        {
            // Arrange
            var processor = new GameObject("TestPlantBatchProcessor").AddComponent<PlantBatchProcessor>();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => processor.Initialize());
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(processor.gameObject);
        }
        
        [Test]
        public void PlantBatchProcessor_Instance_IsSingleton()
        {
            // Arrange
            var processor1 = new GameObject("TestPlantBatchProcessor1").AddComponent<PlantBatchProcessor>();
            var processor2 = new GameObject("TestPlantBatchProcessor2").AddComponent<PlantBatchProcessor>();
            
            processor1.Initialize();
            processor2.Initialize();
            
            // Act
            var instance1 = PlantBatchProcessor.Instance;
            var instance2 = PlantBatchProcessor.Instance;
            
            // Assert
            Assert.IsNotNull(instance1, "Instance should not be null");
            Assert.AreSame(instance1, instance2, "Should return same singleton instance");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(processor1.gameObject);
            UnityEngine.Object.DestroyImmediate(processor2.gameObject);
        }
        
        [Test]
        public void PlantBatchProcessor_ProcessPlantCollection_HandlesEmptyCollection()
        {
            // Arrange
            var processor = new GameObject("TestPlantBatchProcessor").AddComponent<PlantBatchProcessor>();
            processor.Initialize();
            var emptyCollection = new List<GameObject>();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => processor.ProcessPlantCollection(emptyCollection));
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(processor.gameObject);
        }
        
        [Test]
        public void PlantBatchProcessor_GetProcessorStats_ReturnsValidStats()
        {
            // Arrange
            var processor = new GameObject("TestPlantBatchProcessor").AddComponent<PlantBatchProcessor>();
            processor.Initialize();
            
            // Act
            var stats = processor.GetProcessorStats();
            
            // Assert
            Assert.IsNotNull(stats, "Stats should not be null");
            Assert.GreaterOrEqual(stats.TotalPlantsProcessed, 0, "Total plants processed should be non-negative");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(processor.gameObject);
        }
        
        #endregion
        
        #region PooledObjectManager Tests
        
        [Test]
        public void PooledObjectManager_Initialize_SetsUpCorrectly()
        {
            // Arrange
            var manager = new GameObject("TestPooledObjectManager").AddComponent<PooledObjectManager>();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => manager.Initialize());
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(manager.gameObject);
        }
        
        [Test]
        public void PooledObjectManager_Instance_IsSingleton()
        {
            // Arrange
            var manager1 = new GameObject("TestPooledObjectManager1").AddComponent<PooledObjectManager>();
            var manager2 = new GameObject("TestPooledObjectManager2").AddComponent<PooledObjectManager>();
            
            manager1.Initialize();
            manager2.Initialize();
            
            // Act
            var instance1 = PooledObjectManager.Instance;
            var instance2 = PooledObjectManager.Instance;
            
            // Assert
            Assert.IsNotNull(instance1, "Instance should not be null");
            Assert.AreSame(instance1, instance2, "Should return same singleton instance");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(manager1.gameObject);
            UnityEngine.Object.DestroyImmediate(manager2.gameObject);
        }
        
        [Test]
        public void PooledObjectManager_GetDataDictionary_ReturnsValidDictionary()
        {
            // Arrange
            var manager = new GameObject("TestPooledObjectManager").AddComponent<PooledObjectManager>();
            manager.Initialize();
            
            // Act
            var dictionary = manager.GetDataDictionary();
            
            // Assert
            Assert.IsNotNull(dictionary, "Dictionary should not be null");
            
            // Return to pool
            manager.ReturnDataDictionary(dictionary);
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(manager.gameObject);
        }
        
        [Test]
        public void PooledObjectManager_DataDictionaryPooling_ReusesObjects()
        {
            // Arrange
            var manager = new GameObject("TestPooledObjectManager").AddComponent<PooledObjectManager>();
            manager.Initialize();
            
            // Act
            var dict1 = manager.GetDataDictionary();
            manager.ReturnDataDictionary(dict1);
            var dict2 = manager.GetDataDictionary();
            
            // Assert
            Assert.AreSame(dict1, dict2, "Should reuse the same dictionary object");
            
            // Cleanup
            manager.ReturnDataDictionary(dict2);
            UnityEngine.Object.DestroyImmediate(manager.gameObject);
        }
        
        [Test]
        public void PooledObjectManager_GetPoolManagerStats_ReturnsValidStats()
        {
            // Arrange
            var manager = new GameObject("TestPooledObjectManager").AddComponent<PooledObjectManager>();
            manager.Initialize();
            
            // Act
            var stats = manager.GetPoolManagerStats();
            
            // Assert
            Assert.IsNotNull(stats, "Stats should not be null");
            Assert.GreaterOrEqual(stats.GenericPoolCount, 0, "Pool count should be non-negative");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(manager.gameObject);
        }
        
        #endregion
        
        #region Collection Purger Tests
        
        [Test]
        public void UICollectionPurger_Initialize_SetsUpCorrectly()
        {
            // Arrange
            var purger = new GameObject("TestUICollectionPurger").AddComponent<UICollectionPurger>();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => purger.Initialize());
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(purger.gameObject);
        }
        
        [Test]
        public void UICollectionPurger_Instance_IsSingleton()
        {
            // Arrange
            var purger1 = new GameObject("TestUICollectionPurger1").AddComponent<UICollectionPurger>();
            var purger2 = new GameObject("TestUICollectionPurger2").AddComponent<UICollectionPurger>();
            
            purger1.Initialize();
            purger2.Initialize();
            
            // Act
            var instance1 = UICollectionPurger.Instance;
            var instance2 = UICollectionPurger.Instance;
            
            // Assert
            Assert.IsNotNull(instance1, "Instance should not be null");
            Assert.AreSame(instance1, instance2, "Should return same singleton instance");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(purger1.gameObject);
            UnityEngine.Object.DestroyImmediate(purger2.gameObject);
        }
        
        [Test]
        public void PlantUpdateCollectionPurger_Initialize_SetsUpCorrectly()
        {
            // Arrange
            var purger = new GameObject("TestPlantUpdateCollectionPurger").AddComponent<PlantUpdateCollectionPurger>();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => purger.Initialize());
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(purger.gameObject);
        }
        
        [Test]
        public void PlantUpdateCollectionPurger_Instance_IsSingleton()
        {
            // Arrange
            var purger1 = new GameObject("TestPlantUpdateCollectionPurger1").AddComponent<PlantUpdateCollectionPurger>();
            var purger2 = new GameObject("TestPlantUpdateCollectionPurger2").AddComponent<PlantUpdateCollectionPurger>();
            
            purger1.Initialize();
            purger2.Initialize();
            
            // Act
            var instance1 = PlantUpdateCollectionPurger.Instance;
            var instance2 = PlantUpdateCollectionPurger.Instance;
            
            // Assert
            Assert.IsNotNull(instance1, "Instance should not be null");
            Assert.AreSame(instance1, instance2, "Should return same singleton instance");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(purger1.gameObject);
            UnityEngine.Object.DestroyImmediate(purger2.gameObject);
        }
        
        #endregion
        
        #region GenericObjectPool Tests
        
        [Test]
        public void GenericObjectPool_CreateAndGet_ReturnsValidObject()
        {
            // Arrange
            var pool = new GenericObjectPool<TestPoolableObject>(5);
            
            // Act
            var obj = pool.Get();
            
            // Assert
            Assert.IsNotNull(obj, "Pool should return a valid object");
            Assert.AreEqual(42, obj.TestValue, "Object should have default test value");
        }
        
        [Test]
        public void GenericObjectPool_ReturnAndReuse_ReusesObjects()
        {
            // Arrange
            var pool = new GenericObjectPool<TestPoolableObject>(5);
            
            // Act
            var obj1 = pool.Get();
            obj1.TestValue = 99; // Modify the object
            pool.Return(obj1);
            
            var obj2 = pool.Get();
            
            // Assert
            Assert.AreSame(obj1, obj2, "Pool should reuse the same object");
            Assert.AreEqual(42, obj2.TestValue, "Object should be reset to default value");
        }
        
        [Test]
        public void GenericObjectPool_ExceedCapacity_CreatesNewObjects()
        {
            // Arrange
            var pool = new GenericObjectPool<TestPoolableObject>(2);
            
            // Act
            var obj1 = pool.Get();
            var obj2 = pool.Get();
            var obj3 = pool.Get(); // Exceeds initial capacity
            
            // Assert
            Assert.IsNotNull(obj1, "First object should be valid");
            Assert.IsNotNull(obj2, "Second object should be valid");
            Assert.IsNotNull(obj3, "Third object should be valid");
            Assert.AreNotSame(obj1, obj3, "Objects should be different instances");
        }
        
        #endregion
        
        #region Helper Classes
        
        /// <summary>
        /// Test class for object pooling tests
        /// </summary>
        public class TestPoolableObject
        {
            public int TestValue { get; set; } = 42;
            
            public void Reset()
            {
                TestValue = 42;
            }
        }
        
        #endregion
    }
}