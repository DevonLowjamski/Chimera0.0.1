using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

#if UNITY_VFX_GRAPH
using UnityEngine.VFX;
#endif

namespace ProjectChimera.Systems.Visuals
{
    /// <summary>
    /// Manages VFX attachment points on cannabis plants for sophisticated visual effects.
    /// Provides 11 scientifically-accurate attachment points per plant for trichrome development,
    /// stress visualization, growth effects, environmental responses, and genetic expression.
    /// </summary>
    public class PlantVFXAttachmentManager : ChimeraManager
    {
        [Header("VFX Attachment Configuration")]
        [SerializeField] private bool _enableAttachmentPoints = true;
        [SerializeField] private bool _enableAutoPositioning = true;
        [SerializeField] private float _attachmentUpdateInterval = 0.1f;
        
        [Header("Attachment Point Settings")]
        [SerializeField] private bool _enableDynamicAttachments = true;
        [SerializeField] private bool _enableGrowthBasedPositioning = true;
        [SerializeField] private float _attachmentPointRadius = 0.05f;
        [SerializeField] private LayerMask _plantLayers = 1;
        
        [Header("Cannabis-Specific Attachments")]
        [SerializeField] private bool _enableBudAttachments = true;
        [SerializeField] private bool _enableLeafAttachments = true;
        [SerializeField] private bool _enableStemAttachments = true;
        [SerializeField] private bool _enableRootAttachments = true;
        [SerializeField] private bool _enableTrichromeAttachments = true;
        
        [Header("Performance Settings")]
        [SerializeField] private int _maxConcurrentAttachments = 100;
        [SerializeField] private float _attachmentCullingDistance = 20f;
        [SerializeField] private bool _enableLODAttachments = true;
        [SerializeField] private bool _enableAdaptiveQuality = true;
        
        // Attachment Point Management
        private Dictionary<string, PlantAttachmentInstance> _plantAttachments = new Dictionary<string, PlantAttachmentInstance>();
        private Queue<string> _attachmentUpdateQueue = new Queue<string>();
        private List<AttachmentPointUpdate> _pendingAttachmentUpdates = new List<AttachmentPointUpdate>();
        
        // VFX Integration
        private CannabisVFXTemplateManager _vfxTemplateManager;
        private SpeedTreeVFXIntegrationManager _speedTreeIntegration;
        private TrichromeVFXController _trichromeController;
        private PlantHealthVFXController _healthVFXController;
        private EnvironmentalResponseVFXController _environmentalVFXController;
        
        // Attachment Point Definitions
        private Dictionary<VFXAttachmentType, AttachmentPointDefinition> _attachmentDefinitions;
        private AttachmentPerformanceMetrics _performanceMetrics;
        private float _lastAttachmentUpdate = 0f;
        private int _attachmentsProcessedThisFrame = 0;
        
        // Events
        public System.Action<string, VFXAttachmentType, Vector3> OnAttachmentPointCreated;
        public System.Action<string, VFXAttachmentType> OnAttachmentPointRemoved;
        public System.Action<string, PlantAttachmentInstance> OnPlantAttachmentsUpdated;
        public System.Action<AttachmentPerformanceMetrics> OnAttachmentPerformanceUpdate;
        
        // Properties
        public int ActiveAttachmentInstances => _plantAttachments.Count;
        public AttachmentPerformanceMetrics PerformanceMetrics => _performanceMetrics;
        public bool EnableAttachmentPoints => _enableAttachmentPoints;
        
        protected override void OnManagerInitialize()
        {
            InitializeAttachmentSystem();
            InitializeAttachmentDefinitions();
            ConnectToVFXSystems();
            InitializePerformanceTracking();
            StartAttachmentProcessing();
            LogInfo("Plant VFX Attachment Manager initialized with 11 attachment points per plant");
        }
        
        protected override void OnManagerShutdown()
        {
            // Clean up all plant attachments
            foreach (var kvp in _plantAttachments)
            {
                CleanupPlantAttachments(kvp.Value);
            }
            _plantAttachments.Clear();
            
            // Stop all coroutines
            StopAllCoroutines();
            
            LogInfo("Plant VFX Attachment Manager shutdown completed");
        }
        
        private void Update()
        {
            if (_enableAttachmentPoints && Time.time - _lastAttachmentUpdate >= _attachmentUpdateInterval)
            {
                ProcessAttachmentUpdates();
                UpdateAttachmentPositions();
                UpdatePerformanceMetrics();
                _lastAttachmentUpdate = Time.time;
            }
        }
        
        /// <summary>
        /// Registers a plant for VFX attachment management with 11 cannabis-specific attachment points.
        /// </summary>
        public PlantAttachmentInstance RegisterPlant(string plantId, Transform plantTransform, ProjectChimera.Data.Genetics.PlantGrowthStage growthStage)
        {
            if (_plantAttachments.ContainsKey(plantId))
            {
                LogWarning($"Plant {plantId} already registered for VFX attachments");
                return _plantAttachments[plantId];
            }
            
            var attachmentInstance = CreatePlantAttachmentInstance(plantId, plantTransform, growthStage);
            _plantAttachments[plantId] = attachmentInstance;
            
            // Initialize all 11 cannabis-specific attachment points
            InitializeAttachmentPoints(attachmentInstance);
            
            OnPlantAttachmentsUpdated?.Invoke(plantId, attachmentInstance);
            LogInfo($"🌿 Registered plant {plantId} with 11 VFX attachment points");
            
            return attachmentInstance;
        }
        
        /// <summary>
        /// Unregisters a plant from VFX attachment management.
        /// </summary>
        public void UnregisterPlant(string plantId)
        {
            if (!_plantAttachments.TryGetValue(plantId, out var instance))
                return;
                
            // Clean up all VFX effects at attachment points
            CleanupPlantAttachments(instance);
            _plantAttachments.Remove(plantId);
            
            LogInfo($"🌿 Unregistered plant {plantId} VFX attachments");
        }
        
        /// <summary>
        /// Gets attachment point position for a specific plant and attachment type.
        /// </summary>
        public Vector3 GetAttachmentPosition(string plantId, VFXAttachmentType attachmentType)
        {
            if (!_plantAttachments.TryGetValue(plantId, out var instance))
                return Vector3.zero;
                
            if (instance.AttachmentPoints.TryGetValue(attachmentType, out var attachmentPoint))
                return attachmentPoint.WorldPosition;
                
            return Vector3.zero;
        }
        
        /// <summary>
        /// Attaches a VFX effect to a specific attachment point on a plant.
        /// </summary>
        public bool AttachVFXEffect(string plantId, VFXAttachmentType attachmentType, GameObject vfxPrefab)
        {
            if (!_plantAttachments.TryGetValue(plantId, out var instance))
                return false;
                
            if (!instance.AttachmentPoints.TryGetValue(attachmentType, out var attachmentPoint))
                return false;
                
            #if UNITY_VFX_GRAPH
            var vfxInstance = Instantiate(vfxPrefab, attachmentPoint.WorldPosition, attachmentPoint.Rotation);
            vfxInstance.transform.SetParent(attachmentPoint.Transform);
            
            var vfxGraph = vfxInstance.GetComponent<VisualEffect>();
            if (vfxGraph != null)
            {
                attachmentPoint.AttachedVFX.Add(vfxGraph);
                OnAttachmentPointCreated?.Invoke(plantId, attachmentType, attachmentPoint.WorldPosition);
                return true;
            }
            #endif
            
            return false;
        }
        
        /// <summary>
        /// Updates attachment points based on plant growth and morphological changes.
        /// </summary>
        public void UpdatePlantAttachments(string plantId, ProjectChimera.Data.Genetics.PlantGrowthStage newGrowthStage, float growthProgress)
        {
            if (!_plantAttachments.TryGetValue(plantId, out var instance))
                return;
                
            instance.GrowthStage = newGrowthStage;
            instance.GrowthProgress = growthProgress;
            
            // Update attachment positions based on growth
            UpdateAttachmentPositionsForGrowth(instance);
            
            // Enable/disable attachment points based on growth stage
            UpdateAttachmentAvailability(instance);
            
            OnPlantAttachmentsUpdated?.Invoke(plantId, instance);
        }
        
        private void InitializeAttachmentSystem()
        {
            _attachmentUpdateQueue = new Queue<string>();
            _pendingAttachmentUpdates = new List<AttachmentPointUpdate>();
            _performanceMetrics = new AttachmentPerformanceMetrics();
        }
        
        private void InitializeAttachmentDefinitions()
        {
            _attachmentDefinitions = new Dictionary<VFXAttachmentType, AttachmentPointDefinition>();
            
            // Define all 11 cannabis-specific attachment points
            _attachmentDefinitions[VFXAttachmentType.ApicalMeristem] = new AttachmentPointDefinition
            {
                Name = "Apical Meristem",
                Description = "Main growth tip for growth and stress VFX",
                LocalPosition = new Vector3(0f, 1f, 0f),
                AvailableFromStage = ProjectChimera.Data.Genetics.PlantGrowthStage.Seedling,
                VFXTypes = new List<CannabisVFXType> { CannabisVFXType.PlantGrowth, CannabisVFXType.HealthIndicator }
            };
            
            _attachmentDefinitions[VFXAttachmentType.PrimaryBudSite] = new AttachmentPointDefinition
            {
                Name = "Primary Bud Site",
                Description = "Main cola development and trichrome VFX",
                LocalPosition = new Vector3(0f, 0.8f, 0f),
                AvailableFromStage = ProjectChimera.Data.Genetics.PlantGrowthStage.Flowering,
                VFXTypes = new List<CannabisVFXType> { CannabisVFXType.TrichromeGrowth, CannabisVFXType.PlantGrowth }
            };
            
            _attachmentDefinitions[VFXAttachmentType.SecondaryBudSites] = new AttachmentPointDefinition
            {
                Name = "Secondary Bud Sites",
                Description = "Side branch bud development VFX",
                LocalPosition = new Vector3(0.2f, 0.6f, 0f),
                AvailableFromStage = ProjectChimera.Data.Genetics.PlantGrowthStage.Flowering,
                VFXTypes = new List<CannabisVFXType> { CannabisVFXType.PlantGrowth, CannabisVFXType.TrichromeGrowth }
            };
            
            _attachmentDefinitions[VFXAttachmentType.FanLeaves] = new AttachmentPointDefinition
            {
                Name = "Fan Leaves",
                Description = "Large leaves for photosynthesis and stress visualization",
                LocalPosition = new Vector3(0.3f, 0.5f, 0.1f),
                AvailableFromStage = ProjectChimera.Data.Genetics.PlantGrowthStage.Vegetative,
                VFXTypes = new List<CannabisVFXType> { CannabisVFXType.EnvironmentalResponse, CannabisVFXType.HealthIndicator }
            };
            
            _attachmentDefinitions[VFXAttachmentType.SugarLeaves] = new AttachmentPointDefinition
            {
                Name = "Sugar Leaves",
                Description = "Small trichome-rich leaves near buds",
                LocalPosition = new Vector3(0.1f, 0.7f, 0.05f),
                AvailableFromStage = ProjectChimera.Data.Genetics.PlantGrowthStage.Flowering,
                VFXTypes = new List<CannabisVFXType> { CannabisVFXType.TrichromeGrowth, CannabisVFXType.EnvironmentalResponse }
            };
            
            _attachmentDefinitions[VFXAttachmentType.MainStem] = new AttachmentPointDefinition
            {
                Name = "Main Stem",
                Description = "Primary structural support and nutrient transport",
                LocalPosition = new Vector3(0f, 0.3f, 0f),
                AvailableFromStage = ProjectChimera.Data.Genetics.PlantGrowthStage.Seedling,
                VFXTypes = new List<CannabisVFXType> { CannabisVFXType.NutrientDeficiency, CannabisVFXType.PlantGrowth }
            };
            
            _attachmentDefinitions[VFXAttachmentType.BranchNodes] = new AttachmentPointDefinition
            {
                Name = "Branch Nodes",
                Description = "Node points where branches emerge",
                LocalPosition = new Vector3(0.05f, 0.4f, 0f),
                AvailableFromStage = ProjectChimera.Data.Genetics.PlantGrowthStage.Vegetative,
                VFXTypes = new List<CannabisVFXType> { CannabisVFXType.PlantGrowth, CannabisVFXType.HealthIndicator }
            };
            
            _attachmentDefinitions[VFXAttachmentType.RootZone] = new AttachmentPointDefinition
            {
                Name = "Root Zone",
                Description = "Root system for nutrient uptake visualization",
                LocalPosition = new Vector3(0f, -0.1f, 0f),
                AvailableFromStage = ProjectChimera.Data.Genetics.PlantGrowthStage.Germination,
                VFXTypes = new List<CannabisVFXType> { CannabisVFXType.NutrientDeficiency, CannabisVFXType.PlantGrowth }
            };
            
            _attachmentDefinitions[VFXAttachmentType.TrichromeZones] = new AttachmentPointDefinition
            {
                Name = "Trichrome Zones",
                Description = "High-density trichrome areas on buds and leaves",
                LocalPosition = new Vector3(0.08f, 0.75f, 0.02f),
                AvailableFromStage = ProjectChimera.Data.Genetics.PlantGrowthStage.Flowering,
                VFXTypes = new List<CannabisVFXType> { CannabisVFXType.TrichromeGrowth, CannabisVFXType.HarvestReadiness }
            };
            
            _attachmentDefinitions[VFXAttachmentType.EnvironmentalSensors] = new AttachmentPointDefinition
            {
                Name = "Environmental Sensors",
                Description = "Points that respond to environmental changes",
                LocalPosition = new Vector3(0.2f, 0.8f, 0.15f),
                AvailableFromStage = ProjectChimera.Data.Genetics.PlantGrowthStage.Seedling,
                VFXTypes = new List<CannabisVFXType> { CannabisVFXType.EnvironmentalResponse, CannabisVFXType.HealthIndicator }
            };
            
            _attachmentDefinitions[VFXAttachmentType.GeneticMarkers] = new AttachmentPointDefinition
            {
                Name = "Genetic Markers",
                Description = "Visual indicators of genetic trait expression",
                LocalPosition = new Vector3(0f, 0.6f, 0f),
                AvailableFromStage = ProjectChimera.Data.Genetics.PlantGrowthStage.Vegetative,
                VFXTypes = new List<CannabisVFXType> { CannabisVFXType.GeneticTraits, CannabisVFXType.HealthIndicator }
            };
        }
        
        private void ConnectToVFXSystems()
        {
            _vfxTemplateManager = FindFirstObjectByType<CannabisVFXTemplateManager>();
            _speedTreeIntegration = FindFirstObjectByType<SpeedTreeVFXIntegrationManager>();
            _trichromeController = FindFirstObjectByType<TrichromeVFXController>();
            _healthVFXController = FindFirstObjectByType<PlantHealthVFXController>();
            _environmentalVFXController = FindFirstObjectByType<EnvironmentalResponseVFXController>();
            
            if (_vfxTemplateManager == null)
                LogWarning("⚠️ CannabisVFXTemplateManager not found - VFX templates unavailable");
                
            if (_speedTreeIntegration == null)
                LogWarning("⚠️ SpeedTreeVFXIntegrationManager not found - SpeedTree integration unavailable");
        }
        
        private void InitializePerformanceTracking()
        {
            _performanceMetrics = new AttachmentPerformanceMetrics
            {
                ActiveAttachments = 0,
                TotalAttachmentPoints = 0,
                AttachmentsPerSecond = 0f,
                AverageUpdateTime = 0f,
                PeakAttachments = 0,
                MemoryUsage = 0f
            };
        }
        
        private void StartAttachmentProcessing()
        {
            if (_enableAttachmentPoints)
            {
                StartCoroutine(AttachmentUpdateCoroutine());
                LogInfo("🎭 VFX attachment processing started");
            }
        }
        
        private PlantAttachmentInstance CreatePlantAttachmentInstance(string plantId, Transform plantTransform, ProjectChimera.Data.Genetics.PlantGrowthStage growthStage)
        {
            return new PlantAttachmentInstance
            {
                PlantId = plantId,
                PlantTransform = plantTransform,
                GrowthStage = growthStage,
                GrowthProgress = 0f,
                AttachmentPoints = new Dictionary<VFXAttachmentType, AttachmentPoint>(),
                IsActive = true,
                LastUpdate = Time.time
            };
        }
        
        private void InitializeAttachmentPoints(PlantAttachmentInstance instance)
        {
            foreach (var definition in _attachmentDefinitions)
            {
                var attachmentType = definition.Key;
                var def = definition.Value;
                
                // Check if attachment point is available for current growth stage
                if (instance.GrowthStage >= def.AvailableFromStage)
                {
                    var attachmentPoint = CreateAttachmentPoint(instance, attachmentType, def);
                    instance.AttachmentPoints[attachmentType] = attachmentPoint;
                }
            }
        }
        
        private AttachmentPoint CreateAttachmentPoint(PlantAttachmentInstance instance, VFXAttachmentType type, AttachmentPointDefinition definition)
        {
            var attachmentGO = new GameObject($"VFX_Attachment_{type}");
            attachmentGO.transform.SetParent(instance.PlantTransform);
            attachmentGO.transform.localPosition = definition.LocalPosition;
            attachmentGO.transform.localRotation = Quaternion.identity;
            
            return new AttachmentPoint
            {
                Type = type,
                Transform = attachmentGO.transform,
                LocalPosition = definition.LocalPosition,
                WorldPosition = attachmentGO.transform.position,
                Rotation = Quaternion.identity,
                IsActive = true,
                AttachedVFX = new List<MonoBehaviour>(),
                Definition = definition
            };
        }
        
        private void ProcessAttachmentUpdates()
        {
            int attachmentsToProcess = Mathf.Min(_maxConcurrentAttachments / 10, _attachmentUpdateQueue.Count);
            _attachmentsProcessedThisFrame = 0;
            
            for (int i = 0; i < attachmentsToProcess && _attachmentUpdateQueue.Count > 0; i++)
            {
                var plantId = _attachmentUpdateQueue.Dequeue();
                if (_plantAttachments.TryGetValue(plantId, out var instance))
                {
                    UpdatePlantAttachmentPositions(instance);
                    _attachmentsProcessedThisFrame++;
                }
            }
        }
        
        private void UpdateAttachmentPositions()
        {
            foreach (var kvp in _plantAttachments)
            {
                var instance = kvp.Value;
                if (!instance.IsActive || Time.time - instance.LastUpdate < _attachmentUpdateInterval)
                    continue;
                    
                _attachmentUpdateQueue.Enqueue(kvp.Key);
                instance.LastUpdate = Time.time;
            }
        }
        
        private void UpdatePlantAttachmentPositions(PlantAttachmentInstance instance)
        {
            foreach (var kvp in instance.AttachmentPoints)
            {
                var attachmentPoint = kvp.Value;
                if (attachmentPoint.Transform != null)
                {
                    attachmentPoint.WorldPosition = attachmentPoint.Transform.position;
                    attachmentPoint.Rotation = attachmentPoint.Transform.rotation;
                }
            }
        }
        
        private void UpdateAttachmentPositionsForGrowth(PlantAttachmentInstance instance)
        {
            foreach (var kvp in instance.AttachmentPoints)
            {
                var attachmentType = kvp.Key;
                var attachmentPoint = kvp.Value;
                var definition = attachmentPoint.Definition;
                
                if (_enableGrowthBasedPositioning)
                {
                    // Adjust position based on growth progress
                    var growthScale = Mathf.Lerp(0.5f, 1.5f, instance.GrowthProgress);
                    var adjustedPosition = definition.LocalPosition * growthScale;
                    
                    attachmentPoint.Transform.localPosition = adjustedPosition;
                    attachmentPoint.LocalPosition = adjustedPosition;
                }
            }
        }
        
        private void UpdateAttachmentAvailability(PlantAttachmentInstance instance)
        {
            foreach (var definition in _attachmentDefinitions)
            {
                var attachmentType = definition.Key;
                var def = definition.Value;
                
                bool shouldBeActive = instance.GrowthStage >= def.AvailableFromStage;
                
                if (shouldBeActive && !instance.AttachmentPoints.ContainsKey(attachmentType))
                {
                    // Create new attachment point
                    var attachmentPoint = CreateAttachmentPoint(instance, attachmentType, def);
                    instance.AttachmentPoints[attachmentType] = attachmentPoint;
                }
                else if (!shouldBeActive && instance.AttachmentPoints.ContainsKey(attachmentType))
                {
                    // Remove attachment point
                    var attachmentPoint = instance.AttachmentPoints[attachmentType];
                    CleanupAttachmentPoint(attachmentPoint);
                    instance.AttachmentPoints.Remove(attachmentType);
                }
            }
        }
        
        private void CleanupPlantAttachments(PlantAttachmentInstance instance)
        {
            foreach (var kvp in instance.AttachmentPoints)
            {
                CleanupAttachmentPoint(kvp.Value);
            }
            instance.AttachmentPoints.Clear();
        }
        
        private void CleanupAttachmentPoint(AttachmentPoint attachmentPoint)
        {
            // Clean up VFX effects
            foreach (var vfx in attachmentPoint.AttachedVFX)
            {
                if (vfx != null)
                    Destroy(vfx.gameObject);
            }
            
            // Destroy attachment point GameObject
            if (attachmentPoint.Transform != null)
                Destroy(attachmentPoint.Transform.gameObject);
        }
        
        private void UpdatePerformanceMetrics()
        {
            _performanceMetrics.ActiveAttachments = _plantAttachments.Count;
            _performanceMetrics.TotalAttachmentPoints = _plantAttachments.Values.Sum(p => p.AttachmentPoints.Count);
            _performanceMetrics.AttachmentsPerSecond = _attachmentsProcessedThisFrame / Time.deltaTime;
            _performanceMetrics.PeakAttachments = Mathf.Max(_performanceMetrics.PeakAttachments, _performanceMetrics.ActiveAttachments);
            
            OnAttachmentPerformanceUpdate?.Invoke(_performanceMetrics);
        }
        
        private IEnumerator AttachmentUpdateCoroutine()
        {
            while (_enableAttachmentPoints)
            {
                yield return new WaitForSeconds(_attachmentUpdateInterval);
                
                if (_pendingAttachmentUpdates.Count > 0)
                {
                    ProcessPendingAttachmentUpdates();
                }
            }
        }
        
        private void ProcessPendingAttachmentUpdates()
        {
            foreach (var update in _pendingAttachmentUpdates)
            {
                if (_plantAttachments.TryGetValue(update.PlantId, out var instance))
                {
                    UpdatePlantAttachments(update.PlantId, update.NewGrowthStage, update.GrowthProgress);
                }
            }
            _pendingAttachmentUpdates.Clear();
        }
        
    }
    
    /// <summary>
    /// Cannabis-specific VFX attachment point types for comprehensive visual effects.
    /// Each attachment point serves specific scientific and visual purposes.
    /// </summary>
    public enum VFXAttachmentType
    {
        ApicalMeristem,        // Main growth tip
        PrimaryBudSite,        // Main cola
        SecondaryBudSites,     // Side branch buds
        FanLeaves,             // Large photosynthetic leaves
        SugarLeaves,           // Small trichome-rich leaves
        MainStem,              // Primary structural support
        BranchNodes,           // Branch emergence points
        RootZone,              // Root system area
        TrichromeZones,        // High-density trichrome areas
        EnvironmentalSensors,  // Environmental response points
        GeneticMarkers         // Genetic trait visualization points
    }
    
    /// <summary>
    /// Instance data for a plant's VFX attachment system.
    /// </summary>
    [System.Serializable]
    public class PlantAttachmentInstance
    {
        public string PlantId;
        public Transform PlantTransform;
        public ProjectChimera.Data.Genetics.PlantGrowthStage GrowthStage;
        public float GrowthProgress;
        public Dictionary<VFXAttachmentType, AttachmentPoint> AttachmentPoints;
        public bool IsActive;
        public float LastUpdate;
    }
    
    /// <summary>
    /// Individual VFX attachment point data.
    /// </summary>
    [System.Serializable]
    public class AttachmentPoint
    {
        public VFXAttachmentType Type;
        public Transform Transform;
        public Vector3 LocalPosition;
        public Vector3 WorldPosition;
        public Quaternion Rotation;
        public bool IsActive;
        public List<MonoBehaviour> AttachedVFX;
        public AttachmentPointDefinition Definition;
    }
    
    /// <summary>
    /// Definition of an attachment point type.
    /// </summary>
    [System.Serializable]
    public class AttachmentPointDefinition
    {
        public string Name;
        public string Description;
        public Vector3 LocalPosition;
        public ProjectChimera.Data.Genetics.PlantGrowthStage AvailableFromStage;
        public List<CannabisVFXType> VFXTypes;
    }
    
    /// <summary>
    /// Update data for attachment point modifications.
    /// </summary>
    [System.Serializable]
    public class AttachmentPointUpdate
    {
        public string PlantId;
        public ProjectChimera.Data.Genetics.PlantGrowthStage NewGrowthStage;
        public float GrowthProgress;
        public VFXAttachmentType AttachmentType;
        public Vector3 NewPosition;
    }
    
    /// <summary>
    /// Performance metrics for VFX attachment system.
    /// </summary>
    [System.Serializable]
    public class AttachmentPerformanceMetrics
    {
        public int ActiveAttachments;
        public int TotalAttachmentPoints;
        public float AttachmentsPerSecond;
        public float AverageUpdateTime;
        public int PeakAttachments;
        public float MemoryUsage;
    }
}