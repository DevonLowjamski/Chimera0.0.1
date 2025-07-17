using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using System.Collections.Generic;
using System.Collections;
using System;

namespace ProjectChimera.Systems.Visuals
{
    /// <summary>
    /// Comprehensive VFX compatibility layer for Project Chimera cannabis cultivation simulation.
    /// Provides unified interfaces, type adapters, and compatibility shims for all VFX systems.
    /// Handles type conversions, data format translations, and ensures seamless integration
    /// between different VFX systems and core simulation components.
    /// </summary>
    public class VFXCompatibilityLayer : ChimeraManager
    {
        [Header("VFX Compatibility Configuration")]
        [SerializeField] private bool _enableVFXCompatibility = true;
        [SerializeField] private bool _enableTypeConversion = true;
        [SerializeField] private bool _enableDataAdaptation = true;
        [SerializeField] private bool _enableLegacySupport = true;
        
        [Header("Conversion Settings")]
        [SerializeField] private bool _enableVector3ToFloatConversion = true;
        [SerializeField] private bool _enableFloatToVector3Conversion = true;
        [SerializeField] private bool _enableEnvironmentalDataAdaptation = true;
        [SerializeField] private bool _enableGeneticDataAdaptation = true;
        
        [Header("Performance Settings")]
        [SerializeField] private int _maxConcurrentConversions = 100;
        [SerializeField] private bool _enableConversionCaching = true;
        [SerializeField] private float _cacheExpirationTime = 60f;
        
        // Compatibility State
        private Dictionary<string, IVFXCompatibleSystem> _registeredVFXSystems = new Dictionary<string, IVFXCompatibleSystem>();
        private Dictionary<string, object> _conversionCache = new Dictionary<string, object>();
        private Queue<VFXConversionRequest> _conversionQueue = new Queue<VFXConversionRequest>();
        
        // Data Adapters
        private VFXDataAdapter _dataAdapter;
        private VFXTypeConverter _typeConverter;
        private VFXLegacyShim _legacyShim;
        
        // Singleton Instance
        private static VFXCompatibilityLayer _instance;
        public static VFXCompatibilityLayer Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<VFXCompatibilityLayer>();
                return _instance;
            }
        }
        
        protected override void OnManagerInitialize()
        {
            if (_instance == null)
                _instance = this;
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            InitializeCompatibilityLayer();
            InitializeDataAdapters();
            RegisterVFXSystems();
        }
        
        protected override void OnManagerShutdown()
        {
            _registeredVFXSystems.Clear();
            _conversionCache.Clear();
            
            if (_instance == this)
                _instance = null;
        }
        
        private void InitializeCompatibilityLayer()
        {
            _dataAdapter = new VFXDataAdapter();
            _typeConverter = new VFXTypeConverter();
            _legacyShim = new VFXLegacyShim();
        }
        
        private void InitializeDataAdapters()
        {
            // Initialize type conversion mappings
            _typeConverter.RegisterConversion<Vector3, float>((vector) => vector.magnitude);
            _typeConverter.RegisterConversion<float, Vector3>((value) => new Vector3(value, value, value));
            _typeConverter.RegisterConversion<EnvironmentalConditions, Vector3>((env) => new Vector3(env.WindDirection, env.LightDirection, env.AirFlow));
            _typeConverter.RegisterConversion<EnvironmentalConditions, float>((env) => env.LightIntensity);
        }
        
        private void RegisterVFXSystems()
        {
            // Find and register all VFX systems that implement the compatibility interface
            var vfxSystems = FindObjectsOfType<MonoBehaviour>();
            foreach (var system in vfxSystems)
            {
                if (system is IVFXCompatibleSystem compatibleSystem)
                {
                    RegisterVFXSystem(system.GetType().Name, compatibleSystem);
                }
            }
        }
        
        public void RegisterVFXSystem(string systemName, IVFXCompatibleSystem vfxSystem)
        {
            if (!_registeredVFXSystems.ContainsKey(systemName))
            {
                _registeredVFXSystems[systemName] = vfxSystem;
                vfxSystem.InitializeCompatibility(this);
            }
        }
        
        public void UnregisterVFXSystem(string systemName)
        {
            if (_registeredVFXSystems.TryGetValue(systemName, out IVFXCompatibleSystem system))
            {
                system.CleanupCompatibility();
                _registeredVFXSystems.Remove(systemName);
            }
        }
        
        // Type Conversion Methods
        public T ConvertType<T>(object input)
        {
            if (!_enableTypeConversion) return default(T);
            
            return _typeConverter.Convert<T>(input);
        }
        
        public Vector3 ConvertToVector3(object input)
        {
            if (input is Vector3 vector3) return vector3;
            if (input is float floatValue) return new Vector3(floatValue, floatValue, floatValue);
            if (input is EnvironmentalConditions env) return new Vector3(env.WindDirection, env.LightDirection, env.AirFlow);
            return Vector3.zero;
        }
        
        public float ConvertToFloat(object input)
        {
            if (input is float floatValue) return floatValue;
            if (input is Vector3 vector3) return vector3.magnitude;
            if (input is EnvironmentalConditions env) return env.LightIntensity;
            return 0f;
        }
        
        // Environmental Data Adaptation
        public EnvironmentalConditions AdaptEnvironmentalData(object input)
        {
            if (!_enableEnvironmentalDataAdaptation) return null;
            
            return _dataAdapter.AdaptEnvironmentalData(input);
        }
        
        // Genetic Data Adaptation
        public CannabisGeneticProfile AdaptGeneticData(object input)
        {
            if (!_enableGeneticDataAdaptation) return null;
            
            return _dataAdapter.AdaptGeneticData(input);
        }
        
        // Legacy System Support
        public object CreateLegacyWrapper(object modernData, Type legacyType)
        {
            if (!_enableLegacySupport) return null;
            
            return _legacyShim.CreateWrapper(modernData, legacyType);
        }
        
        // Method Overload Compatibility
        public void ProcessEnvironmentalResponse(Vector3 input)
        {
            var adapted = AdaptEnvironmentalData(input);
            ProcessEnvironmentalResponse(adapted);
        }
        
        public void ProcessEnvironmentalResponse(float input)
        {
            var adapted = AdaptEnvironmentalData(input);
            ProcessEnvironmentalResponse(adapted);
        }
        
        public void ProcessEnvironmentalResponse(EnvironmentalConditions input)
        {
            // Main implementation - all other overloads route here
            foreach (var vfxSystem in _registeredVFXSystems.Values)
            {
                vfxSystem.UpdateEnvironmentalResponse(input);
            }
        }
        
        // Growth Animation Compatibility
        public void ProcessGrowthAnimation(Vector3 input)
        {
            var adapted = _dataAdapter.AdaptGrowthData(input);
            ProcessGrowthAnimation(adapted);
        }
        
        public void ProcessGrowthAnimation(float input)
        {
            var adapted = _dataAdapter.AdaptGrowthData(input);
            ProcessGrowthAnimation(adapted);
        }
        
        public void ProcessGrowthAnimation(GrowthAnimationData input)
        {
            // Main implementation
            foreach (var vfxSystem in _registeredVFXSystems.Values)
            {
                vfxSystem.UpdateGrowthAnimation(input);
            }
        }
    }
    
    // VFX Compatibility Interfaces
    public interface IVFXCompatibleSystem
    {
        void InitializeCompatibility(VFXCompatibilityLayer compatibilityLayer);
        void CleanupCompatibility();
        void UpdateEnvironmentalResponse(EnvironmentalConditions conditions);
        void UpdateGrowthAnimation(GrowthAnimationData growthData);
    }
    
    // Data Adapter Classes
    public class VFXDataAdapter
    {
        public EnvironmentalConditions AdaptEnvironmentalData(object input)
        {
            if (input is EnvironmentalConditions existing) return existing;
            
            if (input is Vector3 vector)
            {
                return new EnvironmentalConditions
                {
                    LightIntensity = 800f,
                    Temperature = 24f,
                    Humidity = 65f,
                    AirFlow = vector.magnitude,
                    BarometricPressure = 1013.25f,
                    LightDirection = vector.y,
                    WindDirection = vector.x,
                    WindSpeed = vector.z,
                    CO2Level = 400f
                };
            }
            
            if (input is float value)
            {
                return new EnvironmentalConditions
                {
                    LightIntensity = value,
                    Temperature = 24f,
                    Humidity = 65f,
                    AirFlow = value * 0.1f,
                    BarometricPressure = 1013.25f,
                    LightDirection = 270f,
                    WindDirection = 0f,
                    WindSpeed = value * 0.05f,
                    CO2Level = 400f
                };
            }
            
            return new EnvironmentalConditions();
        }
        
        public CannabisGeneticProfile AdaptGeneticData(object input)
        {
            // Implementation for genetic data adaptation
            return new CannabisGeneticProfile
            {
                HeightPotential = 0.8f,
                WidthPotential = 0.7f,
                BranchingPotential = 0.9f,
                BudSizePotential = 0.8f,
                BudDensityPotential = 0.7f,
                TrichromeDensityPotential = 0.9f,
                THCPotential = 0.2f,
                CBDPotential = 0.1f,
                ColorVariationPotential = 0.5f,
                ApicalDominancePotential = 0.8f,
                SativaPercentage = 0.6f,
                IndicaPercentage = 0.4f,
                RuderalisPercentage = 0f
            };
        }
        
        public GrowthAnimationData AdaptGrowthData(object input)
        {
            return new GrowthAnimationData
            {
                GrowthRate = input is float f ? f : (input is Vector3 v ? v.magnitude : 1f),
                GrowthDirection = input is Vector3 vec ? vec.normalized : Vector3.up,
                GrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage.Vegetative,
                AnimationSpeed = 1f
            };
        }
    }
    
    public class VFXTypeConverter
    {
        private Dictionary<(Type, Type), Func<object, object>> _converters = new Dictionary<(Type, Type), Func<object, object>>();
        
        public void RegisterConversion<TFrom, TTo>(Func<TFrom, TTo> converter)
        {
            _converters[(typeof(TFrom), typeof(TTo))] = (obj) => converter((TFrom)obj);
        }
        
        public T Convert<T>(object input)
        {
            if (input is T directMatch) return directMatch;
            
            if (_converters.TryGetValue((input.GetType(), typeof(T)), out var converter))
            {
                return (T)converter(input);
            }
            
            return default(T);
        }
    }
    
    public class VFXLegacyShim
    {
        public object CreateWrapper(object modernData, Type legacyType)
        {
            // Implementation for creating legacy compatibility wrappers
            return Activator.CreateInstance(legacyType);
        }
    }
    
    // Supporting Data Structures
    [System.Serializable]
    public class VFXConversionRequest
    {
        public object InputData;
        public Type TargetType;
        public string SystemName;
        public float RequestTime;
    }
    
    [System.Serializable]
    public class GrowthAnimationData
    {
        public string PlantInstanceId;
        public float GrowthRate;
        public float GrowthProgress;
        public Vector3 GrowthDirection;
        public ProjectChimera.Data.Genetics.PlantGrowthStage GrowthStage;
        public float AnimationSpeed;
    }
    
    // Stub Classes for Disabled VFX Systems (Temporary)
    public class DynamicGrowthAnimationSystemStub : MonoBehaviour, IVFXCompatibleSystem
    {
        public void InitializeCompatibility(VFXCompatibilityLayer compatibilityLayer) { }
        public void CleanupCompatibility() { }
        public void UpdateEnvironmentalResponse(EnvironmentalConditions conditions) { }
        public void UpdateGrowthAnimation(GrowthAnimationData growthData) { }
    }
    
    public class EnvironmentalResponseVFXControllerStub : MonoBehaviour, IVFXCompatibleSystem
    {
        public void InitializeCompatibility(VFXCompatibilityLayer compatibilityLayer) { }
        public void CleanupCompatibility() { }
        public void UpdateEnvironmentalResponse(EnvironmentalConditions conditions) { }
        public void UpdateGrowthAnimation(GrowthAnimationData growthData) { }
    }
}