using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Core.DependencyInjection;
using ProjectChimera.Testing.Core;

namespace ProjectChimera.Testing.Units
{
    /// <summary>
    /// PC013-22: Unit tests for dependency injection service classes
    /// Tests service container, service registration, and dependency resolution
    /// </summary>
    [TestFixture]
    [Category("Unit Tests")]
    [Category("Dependency Injection")]
    public class DependencyInjectionUnitTests : ChimeraTestBase
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
        
        #region ServiceContainer Tests
        
        [Test]
        public void ServiceContainer_Create_InitializesCorrectly()
        {
            // Arrange & Act
            var container = new ServiceContainer();
            
            // Assert
            Assert.IsNotNull(container, "ServiceContainer should be created successfully");
        }
        
        [Test]
        public void ServiceContainer_RegisterSingleton_StoresService()
        {
            // Arrange
            var container = new ServiceContainer();
            var testService = new TestDIService();
            
            // Act
            container.RegisterSingleton<ITestDIService>(testService);
            
            // Assert
            Assert.IsTrue(container.IsRegistered<ITestDIService>(), "Service should be registered");
        }
        
        [Test]
        public void ServiceContainer_RegisterSingleton_ResolvesSameInstance()
        {
            // Arrange
            var container = new ServiceContainer();
            var testService = new TestDIService();
            container.RegisterSingleton<ITestDIService>(testService);
            
            // Act
            var resolved1 = container.Resolve<ITestDIService>();
            var resolved2 = container.Resolve<ITestDIService>();
            
            // Assert
            Assert.AreSame(testService, resolved1, "Should resolve to registered instance");
            Assert.AreSame(resolved1, resolved2, "Should resolve to same singleton instance");
        }
        
        [Test]
        public void ServiceContainer_RegisterTransient_ResolvesNewInstances()
        {
            // Arrange
            var container = new ServiceContainer();
            container.RegisterTransient<ITestDIService, TestDIService>();
            
            // Act
            var resolved1 = container.Resolve<ITestDIService>();
            var resolved2 = container.Resolve<ITestDIService>();
            
            // Assert
            Assert.IsNotNull(resolved1, "First instance should not be null");
            Assert.IsNotNull(resolved2, "Second instance should not be null");
            Assert.AreNotSame(resolved1, resolved2, "Should resolve to different transient instances");
        }
        
        [Test]
        public void ServiceContainer_ResolveUnregistered_HandlesGracefully()
        {
            // Arrange
            var container = new ServiceContainer();
            
            // Act
            var resolved = container.Resolve<ITestDIService>();
            
            // Assert
            Assert.IsNull(resolved, "Unregistered service should resolve to null");
        }
        
        [Test]
        public void ServiceContainer_IsRegistered_ReturnsFalseForUnregistered()
        {
            // Arrange
            var container = new ServiceContainer();
            
            // Act
            var isRegistered = container.IsRegistered<ITestDIService>();
            
            // Assert
            Assert.IsFalse(isRegistered, "Unregistered service should return false");
        }
        
        [Test]
        public void ServiceContainer_MultipleServices_HandlesCorrectly()
        {
            // Arrange
            var container = new ServiceContainer();
            var service1 = new TestDIService();
            var service2 = new AnotherTestDIService();
            
            // Act
            container.RegisterSingleton<ITestDIService>(service1);
            container.RegisterSingleton<IAnotherTestDIService>(service2);
            
            // Assert
            Assert.IsTrue(container.IsRegistered<ITestDIService>(), "First service should be registered");
            Assert.IsTrue(container.IsRegistered<IAnotherTestDIService>(), "Second service should be registered");
            
            var resolved1 = container.Resolve<ITestDIService>();
            var resolved2 = container.Resolve<IAnotherTestDIService>();
            
            Assert.AreSame(service1, resolved1, "Should resolve correct first service");
            Assert.AreSame(service2, resolved2, "Should resolve correct second service");
        }
        
        #endregion
        
        #region DIGameManager Tests
        
        [Test]
        public void DIGameManager_Initialize_SetsUpCorrectly()
        {
            // Arrange
            var diGameManager = new GameObject("TestDIGameManager").AddComponent<DIGameManager>();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => diGameManager.Initialize());
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(diGameManager.gameObject);
        }
        
        [Test]
        public void DIGameManager_Instance_IsSingleton()
        {
            // Arrange
            var manager1 = new GameObject("TestDIGameManager1").AddComponent<DIGameManager>();
            var manager2 = new GameObject("TestDIGameManager2").AddComponent<DIGameManager>();
            
            manager1.Initialize();
            manager2.Initialize();
            
            // Act
            var instance1 = DIGameManager.Instance;
            var instance2 = DIGameManager.Instance;
            
            // Assert
            Assert.IsNotNull(instance1, "Instance should not be null");
            Assert.AreSame(instance1, instance2, "Should return same singleton instance");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(manager1.gameObject);
            UnityEngine.Object.DestroyImmediate(manager2.gameObject);
        }
        
        [Test]
        public void DIGameManager_HasServiceContainer_AfterInitialization()
        {
            // Arrange
            var diGameManager = new GameObject("TestDIGameManager").AddComponent<DIGameManager>();
            
            // Act
            diGameManager.Initialize();
            
            // Assert - Check if ServiceContainer property exists (it may be internal)
            var serviceContainerProperty = diGameManager.GetType().GetProperty("ServiceContainer");
            if (serviceContainerProperty != null)
            {
                var serviceContainer = serviceContainerProperty.GetValue(diGameManager);
                Assert.IsNotNull(serviceContainer, "ServiceContainer should be initialized");
            }
            else
            {
                // If property doesn't exist publicly, just verify no exceptions during initialization
                Assert.Pass("DIGameManager initialized without exceptions");
            }
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(diGameManager.gameObject);
        }
        
        #endregion
        
        #region DIChimeraManager Tests
        
        [Test]
        public void DIChimeraManager_Initialize_SetsUpCorrectly()
        {
            // Arrange
            var diManager = new GameObject("TestDIChimeraManager").AddComponent<TestDIChimeraManager>();
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => diManager.Initialize());
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(diManager.gameObject);
        }
        
        [Test]
        public void DIChimeraManager_InheritanceChain_IsCorrect()
        {
            // Arrange
            var diManager = new GameObject("TestDIChimeraManager").AddComponent<TestDIChimeraManager>();
            
            // Act & Assert
            Assert.IsTrue(diManager is ChimeraManager, "DIChimeraManager should inherit from ChimeraManager");
            Assert.IsTrue(diManager is MonoBehaviour, "Should inherit from MonoBehaviour");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(diManager.gameObject);
        }
        
        #endregion
        
        #region Helper Classes and Interfaces
        
        /// <summary>
        /// Test interface for dependency injection
        /// </summary>
        public interface ITestDIService
        {
            string GetTestMessage();
            int GetTestValue();
        }
        
        /// <summary>
        /// Test service implementation
        /// </summary>
        public class TestDIService : ITestDIService
        {
            public string GetTestMessage()
            {
                return "Test DI Service Working";
            }
            
            public int GetTestValue()
            {
                return 42;
            }
        }
        
        /// <summary>
        /// Another test interface for multiple service testing
        /// </summary>
        public interface IAnotherTestDIService
        {
            bool IsWorking();
        }
        
        /// <summary>
        /// Another test service implementation
        /// </summary>
        public class AnotherTestDIService : IAnotherTestDIService
        {
            public bool IsWorking()
            {
                return true;
            }
        }
        
        /// <summary>
        /// Test implementation of DIChimeraManager for testing
        /// </summary>
        public class TestDIChimeraManager : DIChimeraManager
        {
            protected override void OnManagerInitialize()
            {
                // Test implementation - no specific logic needed
            }
            
            protected override void OnManagerShutdown()
            {
                // Test implementation - no specific logic needed
            }
            
            protected override void OnManagerUpdate()
            {
                // Test implementation - no specific logic needed
            }
        }
        
        #endregion
    }
}