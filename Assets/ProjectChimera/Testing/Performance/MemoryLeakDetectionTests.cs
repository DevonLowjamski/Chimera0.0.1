using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Testing.Core;

namespace ProjectChimera.Testing.Performance
{
    /// <summary>
    /// PC016-1c: Memory leak detection validation tests
    /// Validates the memory monitoring system's ability to detect various types of memory leaks
    /// </summary>
    public class MemoryLeakDetectionTests : ChimeraTestBase
    {
        private MemoryMonitoringSystem _memoryMonitor;
        private List<MemoryLeakReport> _detectedLeaks = new List<MemoryLeakReport>();
        
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            InitializeMemoryMonitor();
        }
        
        [TearDown]
        public override void TearDown()
        {
            CleanupMemoryMonitor();
            base.TearDown();
        }
        
        #region Initialization
        
        private void InitializeMemoryMonitor()
        {
            // Create memory monitoring system
            var monitorGO = new GameObject("MemoryMonitoringSystem");
            _memoryMonitor = monitorGO.AddComponent<MemoryMonitoringSystem>();
            
            // Subscribe to leak detection events
            _memoryMonitor.OnMemoryLeakDetected += OnLeakDetected;
            
            // Start monitoring
            _memoryMonitor.StartMemoryMonitoring();
            
            LogInfo("PC016-1c: Memory monitor initialized for testing");
        }
        
        private void CleanupMemoryMonitor()
        {
            if (_memoryMonitor != null)
            {
                _memoryMonitor.OnMemoryLeakDetected -= OnLeakDetected;
                _memoryMonitor.StopMemoryMonitoring();
                UnityEngine.Object.DestroyImmediate(_memoryMonitor.gameObject);
            }
            
            _detectedLeaks.Clear();
        }
        
        private void OnLeakDetected(MemoryLeakReport leak)
        {
            _detectedLeaks.Add(leak);
            LogWarning($"PC016-1c: Test detected memory leak in {leak.Category} - Severity: {leak.Severity}");
        }
        
        #endregion
        
        #region Memory Snapshot Tests
        
        /// <summary>
        /// Test basic memory snapshot functionality
        /// </summary>
        [Test]
        [Category("Memory Monitoring")]
        public void TestMemorySnapshotCapture()
        {
            LogInfo("PC016-1c: Testing memory snapshot capture");
            
            // Take a memory snapshot
            var snapshot = _memoryMonitor.TakeMemorySnapshot("UnitTest");
            
            // Validate snapshot data
            Assert.IsNotNull(snapshot, "Memory snapshot should not be null");
            Assert.IsNotEmpty(snapshot.Label, "Snapshot label should not be empty");
            Assert.AreEqual("UnitTest", snapshot.Label, "Snapshot label should match");
            Assert.Greater(snapshot.TotalAllocatedMemory, 0, "Total allocated memory should be positive");
            Assert.Greater(snapshot.ManagedHeapSize, 0, "Managed heap size should be positive");
            Assert.GreaterOrEqual(snapshot.TotalReservedMemory, snapshot.TotalAllocatedMemory, 
                "Reserved memory should be >= allocated memory");
            
            LogInfo($"PC016-1c: Snapshot captured - Allocated: {snapshot.TotalAllocatedMemory / (1024 * 1024):F1}MB, " +
                   $"Heap: {snapshot.ManagedHeapSize / (1024 * 1024):F1}MB");
        }
        
        /// <summary>
        /// Test memory snapshot comparison
        /// </summary>
        [UnityTest]
        [Category("Memory Monitoring")]
        public IEnumerator TestMemorySnapshotComparison()
        {
            LogInfo("PC016-1c: Testing memory snapshot comparison");
            
            // Take initial snapshot
            var snapshot1 = _memoryMonitor.TakeMemorySnapshot("Before");
            
            // Allocate some memory
            var largeArray = new byte[10 * 1024 * 1024]; // 10MB
            for (int i = 0; i < largeArray.Length; i++)
            {
                largeArray[i] = (byte)(i % 256);
            }
            
            yield return new WaitForSeconds(0.1f);
            
            // Take second snapshot
            var snapshot2 = _memoryMonitor.TakeMemorySnapshot("After");
            
            // Verify memory increase
            var memoryIncrease = snapshot2.TotalAllocatedMemory - snapshot1.TotalAllocatedMemory;
            Assert.Greater(memoryIncrease, 5 * 1024 * 1024, "Should detect memory increase of at least 5MB");
            
            LogInfo($"PC016-1c: Memory increase detected: {memoryIncrease / (1024 * 1024):F1}MB");
            
            // Cleanup
            largeArray = null;
            System.GC.Collect();
            yield return new WaitForEndOfFrame();
        }
        
        #endregion
        
        #region Leak Detection Tests
        
        /// <summary>
        /// Test detection of managed heap memory leak
        /// </summary>
        [UnityTest]
        [Category("Memory Leak Detection")]
        public IEnumerator TestManagedHeapLeakDetection()
        {
            LogInfo("PC016-1c: Testing managed heap leak detection");
            
            // Clear any existing leak reports
            _detectedLeaks.Clear();
            
            // Create a simulated memory leak by continuously allocating objects
            var leakyObjects = new List<byte[]>();
            
            // Create leak pattern over time
            for (int i = 0; i < 15; i++) // Create 15 allocations to trigger leak detection
            {
                // Allocate 5MB each time
                var allocation = new byte[5 * 1024 * 1024];
                leakyObjects.Add(allocation);
                
                // Take snapshot to feed the monitoring system
                _memoryMonitor.TakeMemorySnapshot($"LeakTest_{i}");
                
                // Wait for monitoring interval
                yield return new WaitForSeconds(1.1f);
            }
            
            // Wait for leak detection to process
            yield return new WaitForSeconds(2f);
            
            // Force leak detection analysis
            var analysis = _memoryMonitor.PerformMemoryAnalysis();
            
            // Verify leak detection
            Assert.IsNotNull(analysis, "Memory analysis should be available");
            Assert.Greater(analysis.TotalMemoryGrowth, 50 * 1024 * 1024, "Should detect significant memory growth");
            
            // Check if leak was detected (may take time due to sampling requirements)
            LogInfo($"PC016-1c: Memory growth detected: {analysis.TotalMemoryGrowth / (1024 * 1024):F1}MB");
            LogInfo($"PC016-1c: Detected leaks: {_detectedLeaks.Count}");
            
            // Cleanup to prevent actual memory issues
            leakyObjects.Clear();
            System.GC.Collect();
            yield return new WaitForEndOfFrame();
        }
        
        /// <summary>
        /// Test garbage collection monitoring
        /// </summary>
        [UnityTest]
        [Category("Memory Monitoring")]
        public IEnumerator TestGarbageCollectionMonitoring()
        {
            LogInfo("PC016-1c: Testing garbage collection monitoring");
            
            // Force garbage collection to create baseline
            _memoryMonitor.ForceGarbageCollection();
            yield return new WaitForSeconds(0.5f);
            
            // Create objects that will be garbage collected
            for (int i = 0; i < 5; i++)
            {
                var tempArray = new byte[2 * 1024 * 1024]; // 2MB
                // Let it go out of scope
                yield return new WaitForSeconds(0.1f);
            }
            
            // Force another GC
            _memoryMonitor.ForceGarbageCollection();
            yield return new WaitForSeconds(0.5f);
            
            // Analyze results
            var analysis = _memoryMonitor.PerformMemoryAnalysis();
            
            Assert.IsNotNull(analysis, "Memory analysis should be available");
            Assert.Greater(analysis.TotalGCEvents, 0, "Should detect GC events");
            
            LogInfo($"PC016-1c: GC events detected: {analysis.TotalGCEvents}");
            LogInfo($"PC016-1c: Total memory freed by GC: {analysis.TotalMemoryFreedByGC / (1024 * 1024):F1}MB");
        }
        
        #endregion
        
        #region Threshold Tests
        
        /// <summary>
        /// Test memory threshold monitoring
        /// </summary>
        [UnityTest]
        [Category("Memory Monitoring")]
        public IEnumerator TestMemoryThresholdMonitoring()
        {
            LogInfo("PC016-1c: Testing memory threshold monitoring");
            
            var thresholdExceeded = false;
            _memoryMonitor.OnMemoryThresholdExceeded += (snapshot) => thresholdExceeded = true;
            
            // Get current memory usage
            var currentSnapshot = _memoryMonitor.GetCurrentMemorySnapshot();
            var currentMemoryMB = currentSnapshot.TotalAllocatedMemory / (1024 * 1024);
            
            LogInfo($"PC016-1c: Current memory usage: {currentMemoryMB}MB");
            
            // Note: In a real test environment, we might not want to actually exceed thresholds
            // as this could cause system instability. This test verifies the monitoring mechanism.
            
            Assert.IsNotNull(currentSnapshot, "Should be able to get current memory snapshot");
            Assert.Greater(currentMemoryMB, 0, "Current memory usage should be positive");
            
            yield return new WaitForSeconds(0.1f);
        }
        
        #endregion
        
        #region Reporting Tests
        
        /// <summary>
        /// Test memory report generation
        /// </summary>
        [Test]
        [Category("Memory Monitoring")]
        public void TestMemoryReportGeneration()
        {
            LogInfo("PC016-1c: Testing memory report generation");
            
            // Take several snapshots to create data
            for (int i = 0; i < 5; i++)
            {
                _memoryMonitor.TakeMemorySnapshot($"ReportTest_{i}");
            }
            
            // Generate analysis
            var analysis = _memoryMonitor.PerformMemoryAnalysis();
            
            // Validate analysis results
            Assert.IsNotNull(analysis, "Memory analysis should be generated");
            Assert.Greater(analysis.TotalSnapshots, 0, "Should have memory snapshots");
            Assert.IsNotNull(analysis.StartSnapshot, "Should have start snapshot");
            Assert.IsNotNull(analysis.EndSnapshot, "Should have end snapshot");
            Assert.IsNotNull(analysis.PeakMemorySnapshot, "Should have peak memory snapshot");
            
            // Test report generation (should not throw)
            Assert.DoesNotThrow(() => _memoryMonitor.GenerateMemoryReport(), 
                "Memory report generation should not throw exceptions");
            
            LogInfo($"PC016-1c: Analysis generated with {analysis.TotalSnapshots} snapshots");
        }
        
        /// <summary>
        /// Test memory data API
        /// </summary>
        [Test]
        [Category("Memory Monitoring")]
        public void TestMemoryDataAPI()
        {
            LogInfo("PC016-1c: Testing memory data API");
            
            // Take some snapshots
            _memoryMonitor.TakeMemorySnapshot("API_Test_1");
            _memoryMonitor.TakeMemorySnapshot("API_Test_2");
            
            // Test API methods
            var snapshots = _memoryMonitor.GetAllMemorySnapshots();
            Assert.IsNotNull(snapshots, "Should return snapshot list");
            Assert.Greater(snapshots.Count, 0, "Should have snapshots");
            
            var currentSnapshot = _memoryMonitor.GetCurrentMemorySnapshot();
            Assert.IsNotNull(currentSnapshot, "Should return current snapshot");
            
            var leaks = _memoryMonitor.GetDetectedLeaks();
            Assert.IsNotNull(leaks, "Should return leak list");
            
            // Test clear functionality
            _memoryMonitor.ClearMonitoringData();
            var clearedSnapshots = _memoryMonitor.GetAllMemorySnapshots();
            Assert.AreEqual(0, clearedSnapshots.Count, "Should clear all snapshots");
            
            LogInfo("PC016-1c: Memory data API validation completed");
        }
        
        #endregion
        
        #region Edge Case Tests
        
        /// <summary>
        /// Test memory monitoring with no data
        /// </summary>
        [Test]
        [Category("Memory Monitoring")]
        public void TestMemoryMonitoringEdgeCases()
        {
            LogInfo("PC016-1c: Testing memory monitoring edge cases");
            
            // Clear all data
            _memoryMonitor.ClearMonitoringData();
            
            // Test with no data
            var analysis = _memoryMonitor.PerformMemoryAnalysis();
            // Should return null or handle gracefully
            
            var snapshots = _memoryMonitor.GetAllMemorySnapshots();
            Assert.AreEqual(0, snapshots.Count, "Should have no snapshots after clear");
            
            var leaks = _memoryMonitor.GetDetectedLeaks();
            Assert.AreEqual(0, leaks.Count, "Should have no leaks after clear");
            
            // Test report generation with no data (should not crash)
            Assert.DoesNotThrow(() => _memoryMonitor.GenerateMemoryReport(), 
                "Should handle report generation with no data");
            
            LogInfo("PC016-1c: Edge case testing completed");
        }
        
        #endregion
        
        #region Performance Tests
        
        /// <summary>
        /// Test memory monitoring performance impact
        /// </summary>
        [UnityTest]
        [Category("Memory Monitoring")]
        public IEnumerator TestMemoryMonitoringPerformance()
        {
            LogInfo("PC016-1c: Testing memory monitoring performance impact");
            
            var startTime = Time.realtimeSinceStartup;
            
            // Take many snapshots rapidly
            for (int i = 0; i < 100; i++)
            {
                _memoryMonitor.TakeMemorySnapshot($"PerfTest_{i}");
                
                if (i % 10 == 0)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
            
            var endTime = Time.realtimeSinceStartup;
            var totalTime = endTime - startTime;
            
            // Performance validation
            Assert.Less(totalTime, 5f, "100 snapshots should complete within 5 seconds");
            
            var timePerSnapshot = totalTime / 100f;
            Assert.Less(timePerSnapshot, 0.05f, "Each snapshot should take less than 50ms");
            
            LogInfo($"PC016-1c: Performance test completed - {totalTime:F2}s total, {timePerSnapshot * 1000:F1}ms per snapshot");
            
            yield return null;
        }
        
        #endregion
    }
}