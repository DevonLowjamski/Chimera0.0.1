using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Diagnostics;
using ProjectChimera.Core;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Testing;

namespace ProjectChimera.Testing.Performance
{
    /// <summary>
    /// Performance testing suite for Cultivation system
    /// </summary>
    public class CultivationPerformanceTests : ChimeraTestBase
    {
        private CultivationManager _cultivationManager;
        private Stopwatch _stopwatch;
        
        [SetUp]
        public void SetUp()
        {
            SetupTestEnvironment();
            _cultivationManager = CreateTestManager<CultivationManager>();
            _stopwatch = new Stopwatch();
        }
        
        [Test]
        [Performance]
        public void CultivationManager_InitializationPerformance()
        {
            _stopwatch.Start();
            
            // Initialize cultivation manager
            _cultivationManager.Initialize();
            
            _stopwatch.Stop();
            
            // Assert initialization takes less than 100ms
            Assert.Less(_stopwatch.ElapsedMilliseconds, 100, 
                $"Cultivation initialization took {_stopwatch.ElapsedMilliseconds}ms, expected < 100ms");
        }
        
        [Test]
        [Performance]
        public void CultivationManager_PlantCreationPerformance()
        {
            _cultivationManager.Initialize();
            
            _stopwatch.Start();
            
            // Create multiple plants
            for (int i = 0; i < 100; i++)
            {
                _cultivationManager.CreatePlant($"TestPlant_{i}", Vector3.zero);
            }
            
            _stopwatch.Stop();
            
            // Assert plant creation takes less than 500ms for 100 plants
            Assert.Less(_stopwatch.ElapsedMilliseconds, 500, 
                $"Creating 100 plants took {_stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
        }
        
        [Test]
        [Performance]
        public void CultivationManager_UpdateCyclePerformance()
        {
            _cultivationManager.Initialize();
            
            // Create some test plants
            for (int i = 0; i < 10; i++)
            {
                _cultivationManager.CreatePlant($"TestPlant_{i}", Vector3.zero);
            }
            
            _stopwatch.Start();
            
            // Run multiple update cycles
            for (int i = 0; i < 100; i++)
            {
                _cultivationManager.UpdateCultivation(Time.fixedDeltaTime);
            }
            
            _stopwatch.Stop();
            
            // Assert update cycles take less than 50ms
            Assert.Less(_stopwatch.ElapsedMilliseconds, 50, 
                $"100 cultivation updates took {_stopwatch.ElapsedMilliseconds}ms, expected < 50ms");
        }
        
        [Test]
        [Performance]
        public void CultivationManager_MemoryUsageTest()
        {
            long initialMemory = GC.GetTotalMemory(true);
            
            _cultivationManager.Initialize();
            
            // Create many plants to test memory usage
            for (int i = 0; i < 1000; i++)
            {
                _cultivationManager.CreatePlant($"TestPlant_{i}", Vector3.zero);
            }
            
            long finalMemory = GC.GetTotalMemory(true);
            long memoryDelta = finalMemory - initialMemory;
            
            // Memory usage should be reasonable (less than 50MB for 1000 plants)
            Assert.Less(memoryDelta, 50 * 1024 * 1024, 
                $"Memory usage for 1000 plants: {memoryDelta / (1024 * 1024)}MB, expected < 50MB");
        }
        
        [UnityTest]
        [Performance]
        public IEnumerator CultivationManager_LongTermPerformanceTest()
        {
            _cultivationManager.Initialize();
            
            // Create test plants
            for (int i = 0; i < 20; i++)
            {
                _cultivationManager.CreatePlant($"TestPlant_{i}", Vector3.zero);
            }
            
            _stopwatch.Start();
            
            // Simulate long-term operation
            for (int frame = 0; frame < 1000; frame++)
            {
                _cultivationManager.UpdateCultivation(Time.fixedDeltaTime);
                
                // Yield periodically to prevent blocking
                if (frame % 100 == 0)
                {
                    yield return null;
                }
            }
            
            _stopwatch.Stop();
            
            // Long-term performance should remain stable
            Assert.Less(_stopwatch.ElapsedMilliseconds, 1000, 
                $"1000 frame simulation took {_stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
        }
        
        [Test]
        [Performance]
        public void CultivationManager_BatchOperationPerformance()
        {
            _cultivationManager.Initialize();
            
            _stopwatch.Start();
            
            // Test batch plant creation
            _cultivationManager.CreatePlantBatch(100, "BatchTestPlant");
            
            _stopwatch.Stop();
            
            // Batch operations should be more efficient
            Assert.Less(_stopwatch.ElapsedMilliseconds, 200, 
                $"Batch creation of 100 plants took {_stopwatch.ElapsedMilliseconds}ms, expected < 200ms");
        }
        
        [TearDown]
        public void TearDown()
        {
            _stopwatch?.Stop();
            CleanupTestEnvironment();
        }
    }
}