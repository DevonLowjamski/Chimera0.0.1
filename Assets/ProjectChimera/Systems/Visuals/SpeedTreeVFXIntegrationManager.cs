using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using System.Collections.Generic;
using System.Collections;
using System;

#if UNITY_VFX_GRAPH
using UnityEngine.VFX;
#endif

#if UNITY_SPEEDTREE
using UnityEngine.Rendering;
#endif

namespace ProjectChimera.Systems.Visuals
{
    /// <summary>
    /// Manages integration between SpeedTree cannabis plants and VFX Graph effects.
    /// Provides 11 VFX attachment points per plant and handles real-time visual updates
    /// based on genetic traits, growth stages, and environmental conditions.
    /// </summary>
    public class SpeedTreeVFXIntegrationManager : ChimeraManager
    {
        [Header("SpeedTree Integration")]
        [SerializeField] private bool _enableSpeedTreeVFX = true;
        [SerializeField] private bool _autoCreateAttachmentPoints = true;
        [SerializeField] private float _attachmentPointUpdateInterval = 0.2f;
        
        [Header("VFX Attachment Configuration")]
        [SerializeField] private int _vfxAttachmentPointsPerPlant = 11;
        [SerializeField] private bool _enableDynamicAttachmentPoints = true;
        [SerializeField] private float _attachmentPointRadius = 0.1f;
        
        [Header("Cannabis-Specific Integration")]
        [SerializeField] private bool _enableTrichromeAttachment = true;
        [SerializeField] private bool _enableBudSiteAttachment = true;
        [SerializeField] private bool _enableLeafAttachment = true;
        [SerializeField] private bool _enableStemAttachment = true;
        [SerializeField] private bool _enableRootZoneAttachment = true;
        
        [Header("Performance Settings")]
        [SerializeField] private int _maxConcurrentPlantUpdates = 10;
        [SerializeField] private float _cullingDistance = 30f;
        [SerializeField] private bool _enableLODIntegration = true;
        [SerializeField] private bool _enableGPUOptimization = true;
        
        [Header("Debug Visualization")]
        [SerializeField] private bool _showAttachmentPoints = false;
        [SerializeField] private bool _showVFXBounds = false;
        [SerializeField] private Color _attachmentPointColor = Color.cyan;
        
        // Integration State
        private Dictionary<string, SpeedTreeVFXPlant> _registeredPlants = new Dictionary<string, SpeedTreeVFXPlant>();
        private Queue<string> _plantsToUpdate = new Queue<string>();
        private CannabisVFXTemplateManager _vfxTemplateManager;
        
        // Attachment Point System
        private Dictionary<CannabisAttachmentPoint, Vector3> _standardAttachmentOffsets;
        private List<GameObject> _debugAttachmentMarkers = new List<GameObject>();
        
        // Performance Tracking
        private float _lastBatchUpdate = 0f;
        private int _plantsUpdatedThisFrame = 0;
        private SpeedTreeVFXPerformanceMetrics _performanceMetrics;
        
        // Events
        public System.Action<string, SpeedTreeVFXPlant> OnPlantRegistered;
        public System.Action<string> OnPlantUnregistered;
        public System.Action<string, CannabisAttachmentPoint, string> OnVFXAttached;
        public System.Action<SpeedTreeVFXPerformanceMetrics> OnPerformanceUpdate;
        
        // Properties
        public int RegisteredPlantsCount => _registeredPlants.Count;
        public bool SpeedTreeVFXEnabled => _enableSpeedTreeVFX;
        public SpeedTreeVFXPerformanceMetrics PerformanceMetrics => _performanceMetrics;
        
        protected override void OnManagerInitialize()
        {
            InitializeSpeedTreeVFXIntegration();
            InitializeAttachmentPointSystem();
            InitializePerformanceTracking();
            ConnectToVFXTemplateManager();
            LogInfo("SpeedTree VFX Integration Manager initialized");
        }
        
        private void Update()
        {
            if (_enableSpeedTreeVFX)
            {
                ProcessPlantUpdateQueue();
                UpdatePerformanceMetrics();
            }
        }
        
        #region Initialization
        
        private void InitializeSpeedTreeVFXIntegration()
        {
            LogInfo("=== INITIALIZING SPEEDTREE-VFX INTEGRATION ===");
            
            #if UNITY_SPEEDTREE
            LogInfo("✅ SpeedTree package detected - full integration enabled");
            #else
            LogWarning("⚠️ SpeedTree package not available - using fallback integration");
            #endif
            
            #if UNITY_VFX_GRAPH
            LogInfo("✅ VFX Graph package detected - VFX attachment enabled");
            #else
            LogWarning("⚠️ VFX Graph package not available - VFX attachment disabled");
            _enableSpeedTreeVFX = false;
            #endif
            
            // Initialize performance metrics
            _performanceMetrics = new SpeedTreeVFXPerformanceMetrics
            {
                RegisteredPlants = 0,
                ActiveVFXAttachments = 0,
                UpdatesPerSecond = 0f,
                AverageUpdateTime = 0f,
                LastUpdate = DateTime.Now
            };
        }
        
        private void InitializeAttachmentPointSystem()
        {
            LogInfo("Setting up cannabis plant VFX attachment points...");
            
            // Define standard attachment point offsets for cannabis plants
            _standardAttachmentOffsets = new Dictionary<CannabisAttachmentPoint, Vector3>
            {
                // Primary bud sites
                [CannabisAttachmentPoint.MainCola] = new Vector3(0f, 0.8f, 0f),
                [CannabisAttachmentPoint.UpperBudSite1] = new Vector3(0.15f, 0.7f, 0f),
                [CannabisAttachmentPoint.UpperBudSite2] = new Vector3(-0.15f, 0.7f, 0f),
                [CannabisAttachmentPoint.MidBudSite1] = new Vector3(0.2f, 0.5f, 0.1f),
                [CannabisAttachmentPoint.MidBudSite2] = new Vector3(-0.2f, 0.5f, -0.1f),
                
                // Leaf attachment points
                [CannabisAttachmentPoint.FanLeaf1] = new Vector3(0.25f, 0.6f, 0f),
                [CannabisAttachmentPoint.FanLeaf2] = new Vector3(-0.25f, 0.6f, 0f),
                [CannabisAttachmentPoint.SugarLeaf1] = new Vector3(0.1f, 0.75f, 0.05f),
                
                // Stem and structural points
                [CannabisAttachmentPoint.MainStem] = new Vector3(0f, 0.3f, 0f),
                [CannabisAttachmentPoint.Branch1] = new Vector3(0.3f, 0.4f, 0f),
                [CannabisAttachmentPoint.RootZone] = new Vector3(0f, -0.1f, 0f)
            };
            
            LogInfo($"✅ Configured {_standardAttachmentOffsets.Count} standard attachment points");
        }
        
        private void InitializePerformanceTracking()
        {
            InvokeRepeating(nameof(UpdateDetailedPerformanceMetrics), 1f, 1f);
        }
        
        private void ConnectToVFXTemplateManager()
        {
            _vfxTemplateManager = FindObjectOfType<CannabisVFXTemplateManager>();
            
            if (_vfxTemplateManager == null)
            {
                LogWarning("CannabisVFXTemplateManager not found - creating new instance");
                var templateManagerObject = new GameObject("CannabisVFXTemplateManager");
                _vfxTemplateManager = templateManagerObject.AddComponent<CannabisVFXTemplateManager>();
            }
            
            LogInfo("✅ Connected to VFX Template Manager");
        }
        
        #endregion
        
        #region Plant Registration
        
        public string RegisterSpeedTreePlant(GameObject plantObject, PlantStrainSO strainData = null)
        {
            if (plantObject == null)
            {
                LogError("Cannot register null plant object");
                return null;
            }
            
            string plantId = Guid.NewGuid().ToString();
            
            var speedTreePlant = new SpeedTreeVFXPlant
            {
                PlantId = plantId,
                GameObject = plantObject,
                Transform = plantObject.transform,
                StrainData = strainData,
                IsActive = true,
                RegistrationTime = Time.time,
                LastUpdateTime = 0f,
                AttachmentPoints = new Dictionary<CannabisAttachmentPoint, VFXAttachmentData>(),
                ActiveVFXInstances = new List<string>()
            };
            
            // Detect SpeedTree components
            #if UNITY_SPEEDTREE
            speedTreePlant.SpeedTreeRenderer = plantObject.GetComponent<Renderer>();
            speedTreePlant.SpeedTreeWind = plantObject.GetComponent<WindZone>();
            #endif
            
            // Create attachment points
            CreateAttachmentPoints(speedTreePlant);
            
            _registeredPlants[plantId] = speedTreePlant;
            _plantsToUpdate.Enqueue(plantId);
            
            LogInfo($"SpeedTree plant registered: {plantId[..8]} with {speedTreePlant.AttachmentPoints.Count} attachment points");
            OnPlantRegistered?.Invoke(plantId, speedTreePlant);
            
            return plantId;
        }
        
        public void UnregisterSpeedTreePlant(string plantId)
        {
            if (!_registeredPlants.ContainsKey(plantId))
            {
                LogWarning($"Plant not registered: {plantId}");
                return;
            }
            
            var plant = _registeredPlants[plantId];
            
            // Clean up VFX instances
            foreach (string vfxInstanceId in plant.ActiveVFXInstances)
            {
                _vfxTemplateManager?.DestroyVFXInstance(vfxInstanceId);
            }
            
            // Clean up attachment point objects
            foreach (var attachmentData in plant.AttachmentPoints.Values)
            {
                if (attachmentData.AttachmentObject != null)
                {
                    DestroyImmediate(attachmentData.AttachmentObject);
                }
            }
            
            _registeredPlants.Remove(plantId);
            
            LogInfo($"SpeedTree plant unregistered: {plantId[..8]}");
            OnPlantUnregistered?.Invoke(plantId);
        }
        
        private void CreateAttachmentPoints(SpeedTreeVFXPlant plant)
        {
            LogInfo($"Creating {_vfxAttachmentPointsPerPlant} attachment points for plant {plant.PlantId[..8]}");
            
            foreach (var kvp in _standardAttachmentOffsets)
            {
                var attachmentPoint = kvp.Key;
                var offset = kvp.Value;
                
                // Create attachment point object
                var attachmentObject = new GameObject($"VFX_Attachment_{attachmentPoint}");
                attachmentObject.transform.SetParent(plant.Transform);
                attachmentObject.transform.localPosition = offset;
                
                // Add attachment component
                var attachmentComponent = attachmentObject.AddComponent<VFXAttachmentComponent>();
                attachmentComponent.Initialize(attachmentPoint, plant.PlantId);
                
                var attachmentData = new VFXAttachmentData
                {
                    AttachmentPoint = attachmentPoint,
                    AttachmentObject = attachmentObject,
                    AttachmentComponent = attachmentComponent,
                    LocalOffset = offset,
                    IsActive = true,
                    AttachedVFXIds = new List<string>()
                };
                
                plant.AttachmentPoints[attachmentPoint] = attachmentData;
                
                // Create debug marker if enabled
                if (_showAttachmentPoints)
                {
                    CreateDebugAttachmentMarker(attachmentObject, attachmentPoint);
                }
            }
            
            LogInfo($"✅ Created {plant.AttachmentPoints.Count} attachment points");
        }
        
        private void CreateDebugAttachmentMarker(GameObject attachmentObject, CannabisAttachmentPoint pointType)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = $"Debug_Marker_{pointType}";
            marker.transform.SetParent(attachmentObject.transform);
            marker.transform.localPosition = Vector3.zero;
            marker.transform.localScale = Vector3.one * 0.05f;
            
            var renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = _attachmentPointColor;
            }
            
            // Remove collider
            var collider = marker.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
            }
            
            _debugAttachmentMarkers.Add(marker);
        }
        
        #endregion
        
        #region VFX Attachment System
        
        public string AttachVFXToPlant(string plantId, CannabisAttachmentPoint attachmentPoint, CannabisVFXType vfxType)
        {
            if (!_registeredPlants.ContainsKey(plantId))
            {
                LogError($"Plant not registered: {plantId}");
                return null;
            }
            
            var plant = _registeredPlants[plantId];
            
            if (!plant.AttachmentPoints.ContainsKey(attachmentPoint))
            {
                LogError($"Attachment point not found: {attachmentPoint} on plant {plantId[..8]}");
                return null;
            }
            
            var attachmentData = plant.AttachmentPoints[attachmentPoint];
            
            // Create VFX instance
            string vfxInstanceId = _vfxTemplateManager.CreateVFXInstance(
                vfxType, 
                attachmentData.AttachmentObject.transform, 
                plant.GameObject.GetComponent<MonoBehaviour>()
            );
            
            if (vfxInstanceId != null)
            {
                // Track VFX instance
                attachmentData.AttachedVFXIds.Add(vfxInstanceId);
                plant.ActiveVFXInstances.Add(vfxInstanceId);
                
                LogInfo($"VFX attached: {vfxType} to {attachmentPoint} on plant {plantId[..8]}");
                OnVFXAttached?.Invoke(plantId, attachmentPoint, vfxInstanceId);
                
                return vfxInstanceId;
            }
            
            return null;
        }
        
        public void DetachVFXFromPlant(string plantId, string vfxInstanceId)
        {
            if (!_registeredPlants.ContainsKey(plantId))
            {
                LogError($"Plant not registered: {plantId}");
                return;
            }
            
            var plant = _registeredPlants[plantId];
            
            // Remove from plant's active VFX list
            plant.ActiveVFXInstances.Remove(vfxInstanceId);
            
            // Remove from attachment point
            foreach (var attachmentData in plant.AttachmentPoints.Values)
            {
                attachmentData.AttachedVFXIds.Remove(vfxInstanceId);
            }
            
            // Destroy VFX instance
            _vfxTemplateManager.DestroyVFXInstance(vfxInstanceId);
            
            LogInfo($"VFX detached: {vfxInstanceId[..8]} from plant {plantId[..8]}");
        }
        
        public void AttachStandardCannabisVFX(string plantId, PlantStrainSO strainData = null)
        {
            if (!_registeredPlants.ContainsKey(plantId))
            {
                LogError($"Plant not registered: {plantId}");
                return;
            }
            
            LogInfo($"Attaching standard cannabis VFX to plant {plantId[..8]}");
            
            // Attach health indicator to main cola
            AttachVFXToPlant(plantId, CannabisAttachmentPoint.MainCola, CannabisVFXType.HealthIndicator);
            
            // Attach trichrome effects to bud sites
            AttachVFXToPlant(plantId, CannabisAttachmentPoint.UpperBudSite1, CannabisVFXType.TrichromeGrowth);
            AttachVFXToPlant(plantId, CannabisAttachmentPoint.UpperBudSite2, CannabisVFXType.TrichromeGrowth);
            
            // Attach growth effects to main stem
            AttachVFXToPlant(plantId, CannabisAttachmentPoint.MainStem, CannabisVFXType.PlantGrowth);
            
            // Attach environmental response to fan leaves
            AttachVFXToPlant(plantId, CannabisAttachmentPoint.FanLeaf1, CannabisVFXType.EnvironmentalResponse);
            AttachVFXToPlant(plantId, CannabisAttachmentPoint.FanLeaf2, CannabisVFXType.EnvironmentalResponse);
            
            LogInfo($"✅ Standard cannabis VFX attached to plant {plantId[..8]}");
        }
        
        #endregion
        
        #region Plant Update System
        
        private void ProcessPlantUpdateQueue()
        {
            if (Time.time - _lastBatchUpdate < _attachmentPointUpdateInterval)
                return;
            
            _plantsUpdatedThisFrame = 0;
            
            // Process plants in batches to maintain performance
            while (_plantsToUpdate.Count > 0 && _plantsUpdatedThisFrame < _maxConcurrentPlantUpdates)
            {
                string plantId = _plantsToUpdate.Dequeue();
                
                if (_registeredPlants.ContainsKey(plantId))
                {
                    UpdatePlantVFXIntegration(plantId);
                    _plantsUpdatedThisFrame++;
                    
                    // Re-queue for next update cycle
                    _plantsToUpdate.Enqueue(plantId);
                }
            }
            
            _lastBatchUpdate = Time.time;
        }
        
        private void UpdatePlantVFXIntegration(string plantId)
        {
            var plant = _registeredPlants[plantId];
            
            // Skip if plant is too far away
            if (_enableLODIntegration && IsPlantCulled(plant))
            {
                SetPlantVFXActive(plant, false);
                return;
            }
            
            SetPlantVFXActive(plant, true);
            
            // Update attachment point positions based on SpeedTree animation
            UpdateAttachmentPointPositions(plant);
            
            // Update VFX parameters based on plant state
            UpdatePlantVFXParameters(plant);
            
            plant.LastUpdateTime = Time.time;
        }
        
        private bool IsPlantCulled(SpeedTreeVFXPlant plant)
        {
            if (Camera.main == null) return false;
            
            float distance = Vector3.Distance(Camera.main.transform.position, plant.Transform.position);
            return distance > _cullingDistance;
        }
        
        private void SetPlantVFXActive(SpeedTreeVFXPlant plant, bool active)
        {
            foreach (var attachmentData in plant.AttachmentPoints.Values)
            {
                if (attachmentData.AttachmentObject != null)
                {
                    attachmentData.AttachmentObject.SetActive(active);
                }
            }
        }
        
        private void UpdateAttachmentPointPositions(SpeedTreeVFXPlant plant)
        {
            #if UNITY_SPEEDTREE
            // In a full implementation, this would read SpeedTree bone/vertex positions
            // and update attachment points to follow plant animation
            
            // For now, we'll apply a simple wind-like movement
            float windEffect = Mathf.Sin(Time.time * 2f) * 0.02f;
            
            foreach (var kvp in plant.AttachmentPoints)
            {
                var attachmentPoint = kvp.Key;
                var attachmentData = kvp.Value;
                
                if (attachmentData.AttachmentObject != null)
                {
                    Vector3 basePosition = attachmentData.LocalOffset;
                    
                    // Apply wind effect to upper parts of the plant
                    if (attachmentPoint == CannabisAttachmentPoint.MainCola ||
                        attachmentPoint == CannabisAttachmentPoint.UpperBudSite1 ||
                        attachmentPoint == CannabisAttachmentPoint.UpperBudSite2)
                    {
                        basePosition.x += windEffect;
                    }
                    
                    attachmentData.AttachmentObject.transform.localPosition = basePosition;
                }
            }
            #endif
        }
        
        private void UpdatePlantVFXParameters(SpeedTreeVFXPlant plant)
        {
            // Update VFX parameters based on plant genetic traits and environmental conditions
            foreach (string vfxInstanceId in plant.ActiveVFXInstances)
            {
                // Get strain-specific parameters
                if (plant.StrainData != null)
                {
                    UpdateVFXFromStrainData(vfxInstanceId, plant.StrainData);
                }
                
                // Update based on growth stage and health
                UpdateVFXFromPlantState(vfxInstanceId, plant);
            }
        }
        
        private void UpdateVFXFromStrainData(string vfxInstanceId, PlantStrainSO strainData)
        {
            // Update VFX parameters based on genetic traits
            _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "GeneticVariation", 0.6f);
            _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "TraitExpression", 0.8f);
            
            // Color variations based on strain
            Color strainColor = Color.Lerp(Color.green, Color.purple, 0.3f);
            _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "TraitColor", strainColor);
        }
        
        private void UpdateVFXFromPlantState(string vfxInstanceId, SpeedTreeVFXPlant plant)
        {
            // Update based on simulated plant state
            float healthLevel = 0.8f; // Would get from plant component
            float growthStage = 0.6f; // Would get from plant lifecycle
            float trichromeAmount = 0.4f; // Would get from genetics
            
            _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "HealthLevel", healthLevel);
            _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "GrowthStage", growthStage);
            _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "TrichromeAmount", trichromeAmount);
        }
        
        #endregion
        
        #region Performance Monitoring
        
        private void UpdatePerformanceMetrics()
        {
            _performanceMetrics.RegisteredPlants = _registeredPlants.Count;
            _performanceMetrics.UpdatesPerSecond = _plantsUpdatedThisFrame / Time.deltaTime;
            _performanceMetrics.LastUpdate = DateTime.Now;
            
            // Count active VFX attachments
            int activeAttachments = 0;
            foreach (var plant in _registeredPlants.Values)
            {
                activeAttachments += plant.ActiveVFXInstances.Count;
            }
            _performanceMetrics.ActiveVFXAttachments = activeAttachments;
        }
        
        private void UpdateDetailedPerformanceMetrics()
        {
            OnPerformanceUpdate?.Invoke(_performanceMetrics);
        }
        
        #endregion
        
        #region Public Interface
        
        public SpeedTreeVFXPlant GetRegisteredPlant(string plantId)
        {
            return _registeredPlants.ContainsKey(plantId) ? _registeredPlants[plantId] : null;
        }
        
        public List<SpeedTreeVFXPlant> GetAllRegisteredPlants()
        {
            return new List<SpeedTreeVFXPlant>(_registeredPlants.Values);
        }
        
        public void SetDebugVisualization(bool showAttachmentPoints, bool showVFXBounds)
        {
            _showAttachmentPoints = showAttachmentPoints;
            _showVFXBounds = showVFXBounds;
            
            // Update debug markers
            foreach (var marker in _debugAttachmentMarkers)
            {
                if (marker != null)
                {
                    marker.SetActive(_showAttachmentPoints);
                }
            }
            
            LogInfo($"Debug visualization updated: Attachment Points: {showAttachmentPoints}, VFX Bounds: {showVFXBounds}");
        }
        
        public void SetCullingDistance(float distance)
        {
            _cullingDistance = Mathf.Max(5f, distance);
            LogInfo($"VFX culling distance set to: {_cullingDistance}m");
        }
        
        public SpeedTreeVFXIntegrationReport GetIntegrationReport()
        {
            return new SpeedTreeVFXIntegrationReport
            {
                RegisteredPlantsCount = _registeredPlants.Count,
                TotalAttachmentPoints = _registeredPlants.Count * _vfxAttachmentPointsPerPlant,
                ActiveVFXInstances = _performanceMetrics.ActiveVFXAttachments,
                UpdatesPerSecond = _performanceMetrics.UpdatesPerSecond,
                SpeedTreeEnabled = _enableSpeedTreeVFX,
                VFXGraphEnabled = _vfxTemplateManager != null,
                CullingDistance = _cullingDistance,
                PerformanceMetrics = _performanceMetrics
            };
        }
        
        #endregion
        
        #region Context Menu Actions
        
        [ContextMenu("Show Integration Status")]
        public void ShowIntegrationStatus()
        {
            LogInfo("=== SPEEDTREE-VFX INTEGRATION STATUS ===");
            LogInfo($"Registered Plants: {_registeredPlants.Count}");
            LogInfo($"Total Attachment Points: {_registeredPlants.Count * _vfxAttachmentPointsPerPlant}");
            LogInfo($"Active VFX Instances: {_performanceMetrics.ActiveVFXAttachments}");
            LogInfo($"SpeedTree Integration: {(_enableSpeedTreeVFX ? "ENABLED" : "DISABLED")}");
            LogInfo($"VFX Template Manager: {(_vfxTemplateManager != null ? "CONNECTED" : "NOT FOUND")}");
            LogInfo($"Debug Visualization: {(_showAttachmentPoints ? "ENABLED" : "DISABLED")}");
        }
        
        [ContextMenu("Create Test Plant")]
        public void CreateTestPlant()
        {
            var testPlant = new GameObject("Test_Cannabis_Plant");
            testPlant.transform.position = Vector3.zero;
            
            string plantId = RegisterSpeedTreePlant(testPlant);
            AttachStandardCannabisVFX(plantId);
            
            LogInfo($"Test plant created with ID: {plantId[..8]}");
        }
        
        [ContextMenu("Clear All Plants")]
        public void ClearAllPlants()
        {
            var plantIds = new List<string>(_registeredPlants.Keys);
            foreach (string plantId in plantIds)
            {
                UnregisterSpeedTreePlant(plantId);
            }
            
            LogInfo("All registered plants cleared");
        }
        
        [ContextMenu("Toggle Debug Visualization")]
        public void ToggleDebugVisualization()
        {
            SetDebugVisualization(!_showAttachmentPoints, !_showVFXBounds);
        }
        
        #endregion
        
        protected override void OnManagerShutdown()
        {
            StopAllCoroutines();
            ClearAllPlants();
            CancelInvoke();
            
            // Cleanup debug markers
            foreach (var marker in _debugAttachmentMarkers)
            {
                if (marker != null)
                {
                    DestroyImmediate(marker);
                }
            }
            _debugAttachmentMarkers.Clear();
            
            LogInfo("SpeedTree VFX Integration Manager shutdown complete");
        }
    }
    
    #region Supporting Components
    
    /// <summary>
    /// Component attached to VFX attachment points for identification and management
    /// </summary>
    public class VFXAttachmentComponent : MonoBehaviour
    {
        [SerializeField] private CannabisAttachmentPoint _attachmentPoint;
        [SerializeField] private string _parentPlantId;
        
        public CannabisAttachmentPoint AttachmentPoint => _attachmentPoint;
        public string ParentPlantId => _parentPlantId;
        
        public void Initialize(CannabisAttachmentPoint attachmentPoint, string plantId)
        {
            _attachmentPoint = attachmentPoint;
            _parentPlantId = plantId;
            name = $"VFX_Attachment_{attachmentPoint}";
        }
    }
    
    #endregion
    
    #region Supporting Data Structures
    
    [System.Serializable]
    public enum CannabisAttachmentPoint
    {
        MainCola = 0,
        UpperBudSite1 = 1,
        UpperBudSite2 = 2,
        MidBudSite1 = 3,
        MidBudSite2 = 4,
        FanLeaf1 = 5,
        FanLeaf2 = 6,
        SugarLeaf1 = 7,
        MainStem = 8,
        Branch1 = 9,
        RootZone = 10
    }
    
    [System.Serializable]
    public class SpeedTreeVFXPlant
    {
        public string PlantId;
        public GameObject GameObject;
        public Transform Transform;
        public PlantStrainSO StrainData;
        public bool IsActive;
        public float RegistrationTime;
        public float LastUpdateTime;
        public Dictionary<CannabisAttachmentPoint, VFXAttachmentData> AttachmentPoints;
        public List<string> ActiveVFXInstances;
        
        #if UNITY_SPEEDTREE
        public Renderer SpeedTreeRenderer;
        public WindZone SpeedTreeWind;
        #endif
    }
    
    [System.Serializable]
    public class VFXAttachmentData
    {
        public CannabisAttachmentPoint AttachmentPoint;
        public GameObject AttachmentObject;
        public VFXAttachmentComponent AttachmentComponent;
        public Vector3 LocalOffset;
        public bool IsActive;
        public List<string> AttachedVFXIds;
    }
    
    [System.Serializable]
    public class SpeedTreeVFXPerformanceMetrics
    {
        public int RegisteredPlants;
        public int ActiveVFXAttachments;
        public float UpdatesPerSecond;
        public float AverageUpdateTime;
        public DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class SpeedTreeVFXIntegrationReport
    {
        public int RegisteredPlantsCount;
        public int TotalAttachmentPoints;
        public int ActiveVFXInstances;
        public float UpdatesPerSecond;
        public bool SpeedTreeEnabled;
        public bool VFXGraphEnabled;
        public float CullingDistance;
        public SpeedTreeVFXPerformanceMetrics PerformanceMetrics;
    }
    
    #endregion
}