using UnityEngine;
using ProjectChimera.Core;

#if UNITY_VFX_GRAPH
using UnityEngine.VFX;
#endif

namespace ProjectChimera.Systems.Visuals
{
    /// <summary>
    /// Manages URP-VFX Graph compatibility and optimization settings for Project Chimera.
    /// Ensures optimal performance for cannabis cultivation visual effects.
    /// </summary>
    public class URPVFXCompatibilityManager : ChimeraManager
    {
        [Header("URP Configuration")]
        [SerializeField] private ScriptableObject _urpAsset;
        [SerializeField] private bool _autoDetectURPAsset = true;
        [SerializeField] private bool _enableVFXGraphSupport = true;
        
        [Header("Quality Settings")]
        [SerializeField] private VFXQualityLevel _targetQualityLevel = VFXQualityLevel.High;
        [SerializeField] private bool _enableAdaptiveQuality = true;
        [SerializeField] private float _targetFrameRate = 60f;
        
        [Header("Performance Optimization")]
        [SerializeField] private int _maxSimultaneousVFX = 100;
        [SerializeField] private float _vfxCullingDistance = 50f;
        [SerializeField] private bool _enableVFXLOD = true;
        [SerializeField] private bool _enableGPUInstancing = true;
        
        [Header("Cannabis-Specific Settings")]
        [SerializeField] private int _maxTrichromeEffects = 50;
        [SerializeField] private int _maxGrowthAnimations = 25;
        [SerializeField] private int _maxEnvironmentalEffects = 20;
        [SerializeField] private bool _enableRealTimeGrowthVFX = true;
        
        // Runtime State
        private bool _urpDetected = false;
        private bool _vfxGraphAvailable = false;
        private VFXQualityLevel _currentQualityLevel;
        private float _currentFrameRate = 0f;
        private int _activeVFXCount = 0;
        
        // Performance Tracking
        private float _frameRateHistory = 0f;
        private float _lastQualityAdjustment = 0f;
        private const float QUALITY_ADJUSTMENT_COOLDOWN = 5f;
        
        // Events
        public System.Action<VFXQualityLevel> OnQualityLevelChanged;
        public System.Action<bool> OnVFXCompatibilityChanged;
        
        // Properties
        public bool URPDetected => _urpDetected;
        public bool VFXGraphAvailable => _vfxGraphAvailable;
        public VFXQualityLevel CurrentQualityLevel => _currentQualityLevel;
        public bool IsCompatibilityConfigured => _urpDetected && _vfxGraphAvailable;
        
        protected override void OnManagerInitialize()
        {
            DetectRenderPipeline();
            ConfigureVFXCompatibility();
            SetupQualitySettings();
            StartPerformanceMonitoring();
            LogInfo("URP-VFX Compatibility Manager initialized");
        }
        
        private void Update()
        {
            UpdatePerformanceMetrics();
            UpdateAdaptiveQuality();
        }
        
        #region Pipeline Detection and Configuration
        
        private void DetectRenderPipeline()
        {
            LogInfo("=== DETECTING RENDER PIPELINE ===");
            
            // Check current render pipeline using reflection to avoid assembly dependencies
            var currentPipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            
            if (currentPipeline != null)
            {
                string pipelineName = currentPipeline.GetType().Name;
                LogInfo($"Active render pipeline: {pipelineName}");
                
                if (pipelineName.Contains("UniversalRenderPipeline") || pipelineName.Contains("URP"))
                {
                    _urpDetected = true;
                    _urpAsset = currentPipeline;
                    LogInfo("✅ URP detected and configured");
                }
                else
                {
                    _urpDetected = false;
                    LogWarning($"⚠️ Non-URP pipeline detected: {pipelineName}");
                    LogWarning("VFX Graph performance may be suboptimal");
                }
            }
            else
            {
                _urpDetected = false;
                LogWarning("⚠️ Built-in render pipeline detected");
                LogWarning("Consider upgrading to URP for optimal VFX Graph performance");
            }
            
            // Auto-detect URP asset if needed
            if (_autoDetectURPAsset && _urpAsset == null)
            {
                AutoDetectURPAsset();
            }
        }
        
        private void AutoDetectURPAsset()
        {
            LogInfo("Auto-detecting URP asset...");
            
            #if UNITY_EDITOR
            // Look for URP assets in the project
            string[] urpAssetGUIDs = UnityEditor.AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
            
            if (urpAssetGUIDs.Length > 0)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(urpAssetGUIDs[0]);
                _urpAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                
                if (_urpAsset != null)
                {
                    LogInfo($"✅ URP asset auto-detected: {assetPath}");
                }
            }
            else
            {
                LogWarning("❌ No URP assets found in project");
            }
            #else
            LogInfo("URP asset auto-detection only available in editor");
            #endif
        }
        
        private void ConfigureVFXCompatibility()
        {
            LogInfo("=== CONFIGURING VFX GRAPH COMPATIBILITY ===");
            
            #if UNITY_VFX_GRAPH
            _vfxGraphAvailable = true;
            LogInfo("✅ VFX Graph package detected");
            
            if (_urpDetected && _urpAsset != null)
            {
                ConfigureURPForVFX();
            }
            
            #else
            _vfxGraphAvailable = false;
            LogWarning("❌ VFX Graph package not available");
            return;
            #endif
            
            OnVFXCompatibilityChanged?.Invoke(IsCompatibilityConfigured);
        }
        
        private void ConfigureURPForVFX()
        {
            if (_urpAsset == null) return;
            
            LogInfo("Configuring URP settings for optimal VFX Graph performance...");
            
            #if UNITY_EDITOR
            try
            {
                // Enable depth texture for VFX collision detection using reflection
                var serializedObject = new UnityEditor.SerializedObject(_urpAsset);
                
                // Enable depth texture
                var depthTextureProp = serializedObject.FindProperty("m_RequireDepthTexture");
                if (depthTextureProp != null)
                {
                    depthTextureProp.boolValue = true;
                    LogInfo("✅ Depth texture enabled for VFX collision detection");
                }
                
                // Enable opaque texture for VFX that need scene color
                var opaqueTextureProp = serializedObject.FindProperty("m_RequireOpaqueTexture");
                if (opaqueTextureProp != null)
                {
                    opaqueTextureProp.boolValue = true;
                    LogInfo("✅ Opaque texture enabled for advanced VFX effects");
                }
                
                serializedObject.ApplyModifiedProperties();
                LogInfo("✅ URP configured for optimal VFX Graph compatibility");
            }
            catch (System.Exception ex)
            {
                LogWarning($"Could not configure URP settings via SerializedObject: {ex.Message}");
                LogInfo("Manual URP configuration may be required");
            }
            #else
            LogInfo("URP configuration only available in editor mode");
            #endif
        }
        
        #endregion
        
        #region Quality Management
        
        private void SetupQualitySettings()
        {
            LogInfo("=== SETTING UP VFX QUALITY CONFIGURATION ===");
            
            _currentQualityLevel = _targetQualityLevel;
            ApplyQualitySettings(_currentQualityLevel);
            
            LogInfo($"✅ VFX quality level set to: {_currentQualityLevel}");
        }
        
        private void ApplyQualitySettings(VFXQualityLevel qualityLevel)
        {
            switch (qualityLevel)
            {
                case VFXQualityLevel.Low:
                    ApplyLowQualitySettings();
                    break;
                case VFXQualityLevel.Medium:
                    ApplyMediumQualitySettings();
                    break;
                case VFXQualityLevel.High:
                    ApplyHighQualitySettings();
                    break;
                case VFXQualityLevel.Ultra:
                    ApplyUltraQualitySettings();
                    break;
            }
            
            _currentQualityLevel = qualityLevel;
            OnQualityLevelChanged?.Invoke(qualityLevel);
        }
        
        private void ApplyLowQualitySettings()
        {
            _maxSimultaneousVFX = 25;
            _maxTrichromeEffects = 10;
            _maxGrowthAnimations = 5;
            _maxEnvironmentalEffects = 5;
            _vfxCullingDistance = 25f;
            _enableVFXLOD = true;
            _enableRealTimeGrowthVFX = false;
            
            #if UNITY_VFX_GRAPH
            // Set global VFX quality multipliers
            VFXManager.fixedTimeStep = 1f / 30f; // Lower update rate
            VFXManager.maxDeltaTime = 0.1f;
            #endif
            
            LogInfo("Applied LOW quality VFX settings");
        }
        
        private void ApplyMediumQualitySettings()
        {
            _maxSimultaneousVFX = 50;
            _maxTrichromeEffects = 25;
            _maxGrowthAnimations = 12;
            _maxEnvironmentalEffects = 10;
            _vfxCullingDistance = 35f;
            _enableVFXLOD = true;
            _enableRealTimeGrowthVFX = true;
            
            #if UNITY_VFX_GRAPH
            VFXManager.fixedTimeStep = 1f / 45f;
            VFXManager.maxDeltaTime = 0.066f;
            #endif
            
            LogInfo("Applied MEDIUM quality VFX settings");
        }
        
        private void ApplyHighQualitySettings()
        {
            _maxSimultaneousVFX = 100;
            _maxTrichromeEffects = 50;
            _maxGrowthAnimations = 25;
            _maxEnvironmentalEffects = 20;
            _vfxCullingDistance = 50f;
            _enableVFXLOD = true;
            _enableRealTimeGrowthVFX = true;
            
            #if UNITY_VFX_GRAPH
            VFXManager.fixedTimeStep = 1f / 60f;
            VFXManager.maxDeltaTime = 0.033f;
            #endif
            
            LogInfo("Applied HIGH quality VFX settings");
        }
        
        private void ApplyUltraQualitySettings()
        {
            _maxSimultaneousVFX = 200;
            _maxTrichromeEffects = 100;
            _maxGrowthAnimations = 50;
            _maxEnvironmentalEffects = 40;
            _vfxCullingDistance = 75f;
            _enableVFXLOD = false; // Disable LOD for maximum quality
            _enableRealTimeGrowthVFX = true;
            
            #if UNITY_VFX_GRAPH
            VFXManager.fixedTimeStep = 1f / 90f; // Higher update rate
            VFXManager.maxDeltaTime = 0.022f;
            #endif
            
            LogInfo("Applied ULTRA quality VFX settings");
        }
        
        #endregion
        
        #region Performance Monitoring
        
        private void StartPerformanceMonitoring()
        {
            if (_enableAdaptiveQuality)
            {
                InvokeRepeating(nameof(MonitorPerformance), 1f, 1f);
                LogInfo("✅ Adaptive quality monitoring started");
            }
        }
        
        private void UpdatePerformanceMetrics()
        {
            // Update frame rate tracking
            _currentFrameRate = 1f / Time.deltaTime;
            _frameRateHistory = Mathf.Lerp(_frameRateHistory, _currentFrameRate, Time.deltaTime);
            
            // Count active VFX
            #if UNITY_VFX_GRAPH
            _activeVFXCount = FindObjectsOfType<VisualEffect>().Length;
            #endif
        }
        
        private void UpdateAdaptiveQuality()
        {
            if (!_enableAdaptiveQuality) return;
            if (Time.time - _lastQualityAdjustment < QUALITY_ADJUSTMENT_COOLDOWN) return;
            
            // Check if performance is below target
            if (_frameRateHistory < _targetFrameRate * 0.8f) // 20% tolerance
            {
                // Decrease quality if possible
                VFXQualityLevel newLevel = GetLowerQualityLevel(_currentQualityLevel);
                if (newLevel != _currentQualityLevel)
                {
                    ApplyQualitySettings(newLevel);
                    LogWarning($"Performance below target ({_frameRateHistory:F1} FPS), reducing VFX quality to {newLevel}");
                    _lastQualityAdjustment = Time.time;
                }
            }
            // Check if performance is well above target
            else if (_frameRateHistory > _targetFrameRate * 1.2f) // 20% headroom
            {
                // Increase quality if possible
                VFXQualityLevel newLevel = GetHigherQualityLevel(_currentQualityLevel);
                if (newLevel != _currentQualityLevel && newLevel <= _targetQualityLevel)
                {
                    ApplyQualitySettings(newLevel);
                    LogInfo($"Performance above target ({_frameRateHistory:F1} FPS), increasing VFX quality to {newLevel}");
                    _lastQualityAdjustment = Time.time;
                }
            }
        }
        
        private VFXQualityLevel GetLowerQualityLevel(VFXQualityLevel current)
        {
            return current switch
            {
                VFXQualityLevel.Ultra => VFXQualityLevel.High,
                VFXQualityLevel.High => VFXQualityLevel.Medium,
                VFXQualityLevel.Medium => VFXQualityLevel.Low,
                VFXQualityLevel.Low => VFXQualityLevel.Low,
                _ => current
            };
        }
        
        private VFXQualityLevel GetHigherQualityLevel(VFXQualityLevel current)
        {
            return current switch
            {
                VFXQualityLevel.Low => VFXQualityLevel.Medium,
                VFXQualityLevel.Medium => VFXQualityLevel.High,
                VFXQualityLevel.High => VFXQualityLevel.Ultra,
                VFXQualityLevel.Ultra => VFXQualityLevel.Ultra,
                _ => current
            };
        }
        
        private void MonitorPerformance()
        {
            LogInfo($"VFX Performance: {_frameRateHistory:F1} FPS | Active VFX: {_activeVFXCount} | Quality: {_currentQualityLevel}");
        }
        
        #endregion
        
        #region Public Interface
        
        public void SetQualityLevel(VFXQualityLevel qualityLevel)
        {
            _targetQualityLevel = qualityLevel;
            ApplyQualitySettings(qualityLevel);
            LogInfo($"VFX quality manually set to: {qualityLevel}");
        }
        
        public void SetAdaptiveQuality(bool enabled)
        {
            _enableAdaptiveQuality = enabled;
            
            if (enabled)
            {
                StartPerformanceMonitoring();
                LogInfo("✅ Adaptive quality enabled");
            }
            else
            {
                CancelInvoke(nameof(MonitorPerformance));
                LogInfo("❌ Adaptive quality disabled");
            }
        }
        
        public void SetTargetFrameRate(float targetFPS)
        {
            _targetFrameRate = Mathf.Clamp(targetFPS, 30f, 120f);
            LogInfo($"Target frame rate set to: {_targetFrameRate} FPS");
        }
        
        public void OptimizeForHardware()
        {
            LogInfo("=== OPTIMIZING VFX FOR CURRENT HARDWARE ===");
            
            // Detect GPU memory and performance characteristics
            int vramMB = SystemInfo.graphicsMemorySize;
            string gpuName = SystemInfo.graphicsDeviceName.ToLower();
            
            LogInfo($"GPU: {SystemInfo.graphicsDeviceName}");
            LogInfo($"VRAM: {vramMB} MB");
            
            // Auto-adjust settings based on hardware
            VFXQualityLevel recommendedQuality;
            
            if (vramMB >= 8192 && (gpuName.Contains("rtx") || gpuName.Contains("rx 6") || gpuName.Contains("rx 7")))
            {
                recommendedQuality = VFXQualityLevel.Ultra;
                LogInfo("High-end GPU detected - recommending Ultra quality");
            }
            else if (vramMB >= 4096)
            {
                recommendedQuality = VFXQualityLevel.High;
                LogInfo("Mid-range GPU detected - recommending High quality");
            }
            else if (vramMB >= 2048)
            {
                recommendedQuality = VFXQualityLevel.Medium;
                LogInfo("Entry-level GPU detected - recommending Medium quality");
            }
            else
            {
                recommendedQuality = VFXQualityLevel.Low;
                LogInfo("Low-end GPU detected - recommending Low quality");
            }
            
            SetQualityLevel(recommendedQuality);
        }
        
        public URPVFXCompatibilityReport GetCompatibilityReport()
        {
            return new URPVFXCompatibilityReport
            {
                URPDetected = _urpDetected,
                VFXGraphAvailable = _vfxGraphAvailable,
                CurrentQualityLevel = _currentQualityLevel,
                CurrentFrameRate = _currentFrameRate,
                ActiveVFXCount = _activeVFXCount,
                MaxSimultaneousVFX = _maxSimultaneousVFX,
                AdaptiveQualityEnabled = _enableAdaptiveQuality,
                IsOptimallyConfigured = IsCompatibilityConfigured && _currentFrameRate >= _targetFrameRate * 0.9f
            };
        }
        
        #endregion
        
        #region Context Menu Actions
        
        [ContextMenu("Detect Render Pipeline")]
        public void ManualDetectRenderPipeline()
        {
            DetectRenderPipeline();
        }
        
        [ContextMenu("Configure VFX Compatibility")]
        public void ManualConfigureVFX()
        {
            ConfigureVFXCompatibility();
        }
        
        [ContextMenu("Optimize for Current Hardware")]
        public void ManualOptimizeForHardware()
        {
            OptimizeForHardware();
        }
        
        [ContextMenu("Test VFX Performance")]
        public void TestVFXPerformance()
        {
            LogInfo("=== VFX PERFORMANCE TEST ===");
            LogInfo($"Current Frame Rate: {_currentFrameRate:F1} FPS");
            LogInfo($"Target Frame Rate: {_targetFrameRate} FPS");
            LogInfo($"Active VFX Count: {_activeVFXCount}/{_maxSimultaneousVFX}");
            LogInfo($"Quality Level: {_currentQualityLevel}");
            LogInfo($"URP Compatible: {_urpDetected}");
            LogInfo($"VFX Graph Available: {_vfxGraphAvailable}");
            LogInfo($"Adaptive Quality: {(_enableAdaptiveQuality ? "Enabled" : "Disabled")}");
            
            if (IsCompatibilityConfigured)
            {
                LogInfo("🎉 RENDER PIPELINE COMPATIBILITY: OPTIMAL");
            }
            else
            {
                LogWarning("⚠️ RENDER PIPELINE COMPATIBILITY: NEEDS ATTENTION");
            }
        }
        
        #endregion
        
        protected override void OnManagerShutdown()
        {
            CancelInvoke();
            LogInfo("URP-VFX Compatibility Manager shutdown complete");
        }
    }
    
    #region Supporting Data Structures
    
    [System.Serializable]
    public enum VFXQualityLevel
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Ultra = 3
    }
    
    [System.Serializable]
    public class URPVFXCompatibilityReport
    {
        public bool URPDetected;
        public bool VFXGraphAvailable;
        public VFXQualityLevel CurrentQualityLevel;
        public float CurrentFrameRate;
        public int ActiveVFXCount;
        public int MaxSimultaneousVFX;
        public bool AdaptiveQualityEnabled;
        public bool IsOptimallyConfigured;
    }
    
    #endregion
}