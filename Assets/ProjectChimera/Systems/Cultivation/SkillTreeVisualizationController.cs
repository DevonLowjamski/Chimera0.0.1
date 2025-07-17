using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Core.Logging;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Controls the visual representation of the skill tree progression
    /// </summary>
    public class SkillTreeVisualizationController : MonoBehaviour
    {
        [Header("Visualization Configuration")]
        [SerializeField] private Transform _treeRootTransform;
        [SerializeField] private GameObject _branchPrefab;
        [SerializeField] private GameObject _nodePrefab;
        [SerializeField] private Material _activeBranchMaterial;
        [SerializeField] private Material _inactiveBranchMaterial;
        [SerializeField] private Material _lockedNodeMaterial;
        [SerializeField] private Material _unlockedNodeMaterial;
        
        [Header("Animation Settings")]
        [SerializeField] private float _growthAnimationSpeed = 1.0f;
        [SerializeField] private float _vibrancyUpdateSpeed = 2.0f;
        [SerializeField] private AnimationCurve _growthCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private bool _isInitialized = false;
        private Dictionary<SkillBranch, BranchVisualization> _branchVisualizations = new Dictionary<SkillBranch, BranchVisualization>();
        private Dictionary<string, NodeVisualization> _nodeVisualizations = new Dictionary<string, NodeVisualization>();
        private TreeVisualizationState _currentState;
        private TreeVisualizationState _targetState;
        
        // Animation state
        private bool _isAnimatingGrowth = false;
        private float _growthAnimationTime = 0f;
        private float _lastVibrancyUpdate = 0f;
        
        public void Initialize()
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("SkillTreeVisualizationController already initialized", this);
                return;
            }
            
            SetupTreeStructure();
            InitializeVisualizationComponents();
            
            _isInitialized = true;
            ChimeraLogger.Log("SkillTreeVisualizationController initialized successfully", this);
        }
        
        public void Initialize(Dictionary<SkillBranch, BranchProgressionState> branchStates, Dictionary<string, SkillNodeState> nodeStates)
        {
            Initialize();
            
            // Store references to data states
            foreach (var kvp in branchStates)
            {
                if (_branchVisualizations.ContainsKey(kvp.Key))
                {
                    UpdateBranchVisualization(kvp.Key, kvp.Value);
                }
            }
            
            foreach (var kvp in nodeStates)
            {
                CreateNodeVisualization(kvp.Value);
            }
        }
        
        /// <summary>
        /// Update the tree visualization based on progression state
        /// </summary>
        public void UpdateTreeVisualization(TreeVisualizationState newState)
        {
            if (!_isInitialized)
            {
                ChimeraLogger.LogWarning("SkillTreeVisualizationController not initialized", this);
                return;
            }
            
            _targetState = newState;
            
            // Update branch visualizations
            UpdateBranchVisualizations();
            
            // Update node visualizations
            UpdateNodeVisualizations();
            
            // Update overall tree appearance
            UpdateTreeAppearance();
            
            ChimeraLogger.Log($"Tree visualization updated - Health: {newState.OverallTreeHealth:F2}, Stage: {newState.TreeGrowthStage}", this);
        }
        
        /// <summary>
        /// Play growth animation for the tree
        /// </summary>
        public void PlayGrowthAnimation(float deltaTime)
        {
            if (!_isInitialized) return;
            
            if (!_isAnimatingGrowth)
            {
                _isAnimatingGrowth = true;
                _growthAnimationTime = 0f;
            }
            
            _growthAnimationTime += deltaTime * _growthAnimationSpeed;
            
            var animationProgress = _growthCurve.Evaluate(Mathf.Clamp01(_growthAnimationTime));
            
            // Apply growth animation to branches
            foreach (var branchViz in _branchVisualizations.Values)
            {
                AnimateBranchGrowth(branchViz, animationProgress);
            }
            
            // Check if animation is complete
            if (_growthAnimationTime >= 1.0f)
            {
                _isAnimatingGrowth = false;
                _growthAnimationTime = 0f;
            }
        }
        
        /// <summary>
        /// Update visual effects over time
        /// </summary>
        public void UpdateEffects(float deltaTime)
        {
            if (!_isInitialized) return;
            
            // Update vibrancy effects
            if (Time.time - _lastVibrancyUpdate > 1.0f / _vibrancyUpdateSpeed)
            {
                UpdateVibrancyEffects();
                _lastVibrancyUpdate = Time.time;
            }
            
            // Update particle effects
            UpdateParticleEffects(deltaTime);
            
            // Update lighting effects
            UpdateLightingEffects(deltaTime);
        }
        
        /// <summary>
        /// Setup the basic tree structure
        /// </summary>
        private void SetupTreeStructure()
        {
            if (_treeRootTransform == null)
            {
                var rootGO = new GameObject("SkillTreeRoot");
                rootGO.transform.SetParent(transform);
                _treeRootTransform = rootGO.transform;
            }
            
            // Create branch containers
            foreach (SkillBranch branch in System.Enum.GetValues(typeof(SkillBranch)))
            {
                CreateBranchVisualization(branch);
            }
        }
        
        /// <summary>
        /// Create visualization for a specific branch
        /// </summary>
        private void CreateBranchVisualization(SkillBranch branch)
        {
            var branchGO = new GameObject($"Branch_{branch}");
            branchGO.transform.SetParent(_treeRootTransform);
            
            // Position branch based on type
            var branchPosition = GetBranchPosition(branch);
            branchGO.transform.localPosition = branchPosition;
            
            var branchViz = new BranchVisualization
            {
                Branch = branch,
                GameObject = branchGO,
                Renderer = SetupBranchRenderer(branchGO),
                CurrentVibrancy = 0.1f,
                TargetVibrancy = 0.1f,
                GrowthLevel = TreeGrowthLevel.Seed
            };
            
            _branchVisualizations[branch] = branchViz;
        }
        
        /// <summary>
        /// Get the 3D position for a branch in the tree
        /// </summary>
        private Vector3 GetBranchPosition(SkillBranch branch)
        {
            // Position branches in a circular pattern around the tree
            var branchCount = System.Enum.GetValues(typeof(SkillBranch)).Length;
            var branchIndex = (int)branch;
            var angle = (branchIndex / (float)branchCount) * 360f * Mathf.Deg2Rad;
            var radius = GetBranchRadius(branch);
            var height = GetBranchHeight(branch);
            
            return new Vector3(
                Mathf.Cos(angle) * radius,
                height,
                Mathf.Sin(angle) * radius
            );
        }
        
        /// <summary>
        /// Get the radius from center for a branch type
        /// </summary>
        private float GetBranchRadius(SkillBranch branch)
        {
            return branch switch
            {
                SkillBranch.BasicCultivation => 1.5f,  // Core branch, closer to center
                SkillBranch.AdvancedCultivation => 2.0f,  // Medium branch
                SkillBranch.Genetics => 2.5f,     // Major branch
                SkillBranch.Environment => 2.0f,  // Medium branch
                SkillBranch.PostHarvest => 2.0f,     // Medium branch
                SkillBranch.Business => 2.5f,    // Outer branch
                _ => 2.0f
            };
        }
        
        /// <summary>
        /// Get the height for a branch type
        /// </summary>
        private float GetBranchHeight(SkillBranch branch)
        {
            return branch switch
            {
                SkillBranch.BasicCultivation => 0.0f,   // Root level
                SkillBranch.AdvancedCultivation => 1.0f,  // Mid level
                SkillBranch.Genetics => 1.5f,      // Upper level
                SkillBranch.Environment => 1.0f,   // Mid level
                SkillBranch.PostHarvest => 2.0f,      // Top level
                SkillBranch.Business => 2.5f,     // Top level
                _ => 1.0f
            };
        }
        
        /// <summary>
        /// Setup renderer for a branch
        /// </summary>
        private Renderer SetupBranchRenderer(GameObject branchGO)
        {
            // Add visual components
            var meshRenderer = branchGO.AddComponent<MeshRenderer>();
            var meshFilter = branchGO.AddComponent<MeshFilter>();
            
            // Create a simple branch mesh (cylinder for now)
            meshFilter.mesh = CreateBranchMesh();
            meshRenderer.material = _inactiveBranchMaterial;
            
            return meshRenderer;
        }
        
        /// <summary>
        /// Create a simple mesh for branch visualization
        /// </summary>
        private Mesh CreateBranchMesh()
        {
            // Create a simple cylinder mesh for the branch
            var mesh = new Mesh();
            
            // Simple cylinder vertices (simplified)
            var vertices = new Vector3[]
            {
                new Vector3(0, 0, 0),      // Bottom center
                new Vector3(0.1f, 0, 0),   // Bottom edge
                new Vector3(0, 1, 0),      // Top center
                new Vector3(0.1f, 1, 0)    // Top edge
            };
            
            var triangles = new int[]
            {
                0, 1, 2,
                1, 3, 2
            };
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            
            return mesh;
        }
        
        /// <summary>
        /// Initialize visualization components
        /// </summary>
        private void InitializeVisualizationComponents()
        {
            // Setup initial state
            _currentState = new TreeVisualizationState
            {
                OverallTreeHealth = 0.1f,
                TreeGrowthStage = TreeGrowthLevel.Seed,
                TreeVibrancy = 0f,
                CanopyDevelopment = 0f,
                RootSystemStrength = 0f,
                FloweringProgress = 0f
            };
            
            _targetState = _currentState;
        }
        
        /// <summary>
        /// Update branch visualizations
        /// </summary>
        private void UpdateBranchVisualizations()
        {
            foreach (var branchViz in _branchVisualizations.Values)
            {
                // Update material based on vibrancy
                var material = branchViz.CurrentVibrancy > 0.3f ? _activeBranchMaterial : _inactiveBranchMaterial;
                if (branchViz.Renderer.material != material)
                {
                    branchViz.Renderer.material = material;
                }
                
                // Update scale based on growth level
                var scale = GetScaleForGrowthLevel(branchViz.GrowthLevel);
                branchViz.GameObject.transform.localScale = Vector3.Lerp(
                    branchViz.GameObject.transform.localScale,
                    scale,
                    Time.deltaTime * _vibrancyUpdateSpeed
                );
            }
        }
        
        /// <summary>
        /// Update node visualizations
        /// </summary>
        private void UpdateNodeVisualizations()
        {
            foreach (var nodeViz in _nodeVisualizations.Values)
            {
                // Update node appearance based on unlock status
                var material = nodeViz.IsUnlocked ? _unlockedNodeMaterial : _lockedNodeMaterial;
                if (nodeViz.Renderer.material != material)
                {
                    nodeViz.Renderer.material = material;
                }
                
                // Update node effects
                UpdateNodeEffects(nodeViz);
            }
        }
        
        /// <summary>
        /// Update overall tree appearance
        /// </summary>
        private void UpdateTreeAppearance()
        {
            if (_targetState == null) return;
            
            // Smooth transition to target state
            _currentState.OverallTreeHealth = Mathf.Lerp(
                _currentState.OverallTreeHealth,
                _targetState.OverallTreeHealth,
                Time.deltaTime * _vibrancyUpdateSpeed
            );
            
            _currentState.TreeVibrancy = Mathf.Lerp(
                _currentState.TreeVibrancy,
                _targetState.TreeVibrancy,
                Time.deltaTime * _vibrancyUpdateSpeed
            );
            
            // Update tree growth stage if needed
            if (_currentState.TreeGrowthStage != _targetState.TreeGrowthStage)
            {
                _currentState.TreeGrowthStage = _targetState.TreeGrowthStage;
                OnTreeGrowthStageChanged(_targetState.TreeGrowthStage);
            }
        }
        
        /// <summary>
        /// Handle tree growth stage changes
        /// </summary>
        private void OnTreeGrowthStageChanged(TreeGrowthLevel newStage)
        {
            ChimeraLogger.Log($"Tree growth stage changed to: {newStage}", this);
            
            // Trigger stage-specific effects
            switch (newStage)
            {
                case TreeGrowthLevel.Seedling:
                    TriggerSeedlingEffects();
                    break;
                case TreeGrowthLevel.Vegetative:
                    TriggerVegetativeEffects();
                    break;
                case TreeGrowthLevel.Mature:
                    TriggerMatureEffects();
                    break;
                case TreeGrowthLevel.Flowering:
                    TriggerFloweringEffects();
                    break;
                case TreeGrowthLevel.FullyFlowered:
                    TriggerFullyFloweredEffects();
                    break;
            }
        }
        
        /// <summary>
        /// Get scale for a growth level
        /// </summary>
        private Vector3 GetScaleForGrowthLevel(TreeGrowthLevel growthLevel)
        {
            return growthLevel switch
            {
                TreeGrowthLevel.Seed => Vector3.one * 0.1f,
                TreeGrowthLevel.Seedling => Vector3.one * 0.3f,
                TreeGrowthLevel.Vegetative => Vector3.one * 0.6f,
                TreeGrowthLevel.Mature => Vector3.one * 0.8f,
                TreeGrowthLevel.Flowering => Vector3.one * 0.9f,
                TreeGrowthLevel.FullyFlowered => Vector3.one * 1.0f,
                _ => Vector3.one * 0.5f
            };
        }
        
        /// <summary>
        /// Animate branch growth
        /// </summary>
        private void AnimateBranchGrowth(BranchVisualization branchViz, float progress)
        {
            // Scale animation
            var targetScale = GetScaleForGrowthLevel(branchViz.GrowthLevel);
            var currentScale = branchViz.GameObject.transform.localScale;
            var newScale = Vector3.Lerp(currentScale, targetScale, progress);
            branchViz.GameObject.transform.localScale = newScale;
            
            // Color animation
            if (branchViz.Renderer.material.HasProperty("_Color"))
            {
                var baseColor = branchViz.CurrentVibrancy > 0.3f ? Color.green : Color.gray;
                var animatedColor = Color.Lerp(Color.gray, baseColor, progress);
                branchViz.Renderer.material.color = animatedColor;
            }
        }
        
        /// <summary>
        /// Update vibrancy effects
        /// </summary>
        private void UpdateVibrancyEffects()
        {
            foreach (var branchViz in _branchVisualizations.Values)
            {
                // Smooth vibrancy transition
                branchViz.CurrentVibrancy = Mathf.Lerp(
                    branchViz.CurrentVibrancy,
                    branchViz.TargetVibrancy,
                    Time.deltaTime * _vibrancyUpdateSpeed
                );
                
                // Update visual effects based on vibrancy
                UpdateBranchVibrancyEffects(branchViz);
            }
        }
        
        /// <summary>
        /// Update particle effects
        /// </summary>
        private void UpdateParticleEffects(float deltaTime)
        {
            // Placeholder for particle effects
            // In a real implementation, this would manage particle systems
        }
        
        /// <summary>
        /// Update lighting effects
        /// </summary>
        private void UpdateLightingEffects(float deltaTime)
        {
            // Placeholder for lighting effects
            // In a real implementation, this would manage dynamic lighting
        }
        
        /// <summary>
        /// Update node effects
        /// </summary>
        private void UpdateNodeEffects(NodeVisualization nodeViz)
        {
            // Placeholder for node-specific effects
            // In a real implementation, this would handle node glow, particles, etc.
        }
        
        /// <summary>
        /// Update branch vibrancy effects
        /// </summary>
        private void UpdateBranchVibrancyEffects(BranchVisualization branchViz)
        {
            // Update material properties based on vibrancy
            if (branchViz.Renderer.material.HasProperty("_EmissionColor"))
            {
                var emissionIntensity = branchViz.CurrentVibrancy * 0.5f;
                var emissionColor = Color.green * emissionIntensity;
                branchViz.Renderer.material.SetColor("_EmissionColor", emissionColor);
            }
        }
        
        /// <summary>
        /// Animate a node unlock with visual effects
        /// </summary>
        public void AnimateNodeUnlock(string nodeId)
        {
            if (!_isInitialized)
            {
                ChimeraLogger.LogWarning("SkillTreeVisualizationController not initialized", this);
                return;
            }
            
            if (_nodeVisualizations.TryGetValue(nodeId, out var nodeViz))
            {
                nodeViz.IsUnlocked = true;
                nodeViz.UnlockProgress = 1.0f;
                
                // Trigger unlock animation
                StartCoroutine(AnimateNodeUnlockEffect(nodeViz));
                
                ChimeraLogger.Log($"Animating node unlock: {nodeId}", this);
            }
        }
        
        /// <summary>
        /// Update tree growth visualization
        /// </summary>
        public void UpdateTreeGrowth(TreeGrowthLevel growthLevel, float vibrancy)
        {
            if (!_isInitialized)
            {
                ChimeraLogger.LogWarning("SkillTreeVisualizationController not initialized", this);
                return;
            }
            
            _targetState.TreeGrowthStage = growthLevel;
            _targetState.TreeVibrancy = vibrancy;
            _targetState.OverallTreeHealth = vibrancy;
            
            UpdateTreeAppearance();
        }
        
        /// <summary>
        /// Update branch growth visualization
        /// </summary>
        public void UpdateBranchGrowth(SkillBranch branch, BranchProgressionState branchState)
        {
            if (!_isInitialized || !_branchVisualizations.ContainsKey(branch))
                return;
            
            var branchViz = _branchVisualizations[branch];
            branchViz.TargetVibrancy = branchState.BranchVibrancy;
            branchViz.GrowthLevel = branchState.BranchGrowthLevel;
            
            UpdateBranchVisualization(branch, branchState);
        }
        
        /// <summary>
        /// Create visualization for a node
        /// </summary>
        private void CreateNodeVisualization(SkillNodeState nodeState)
        {
            if (_nodeVisualizations.ContainsKey(nodeState.NodeId))
                return;
            
            var nodeGO = new GameObject($"Node_{nodeState.NodeId}");
            
            // Find parent branch
            if (_branchVisualizations.TryGetValue(nodeState.Branch, out var branchViz))
            {
                nodeGO.transform.SetParent(branchViz.GameObject.transform);
            }
            else
            {
                nodeGO.transform.SetParent(_treeRootTransform);
            }
            
            // Position node
            var nodePosition = GetNodePosition(nodeState);
            nodeGO.transform.localPosition = nodePosition;
            
            // Setup visual components
            var meshRenderer = nodeGO.AddComponent<MeshRenderer>();
            var meshFilter = nodeGO.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateNodeMesh();
            
            var nodeViz = new NodeVisualization
            {
                NodeId = nodeState.NodeId,
                NodeType = nodeState.NodeType,
                GameObject = nodeGO,
                Renderer = meshRenderer,
                IsUnlocked = nodeState.IsUnlocked,
                UnlockProgress = nodeState.IsUnlocked ? 1.0f : 0.0f
            };
            
            // Set initial material
            meshRenderer.material = nodeState.IsUnlocked ? _unlockedNodeMaterial : _lockedNodeMaterial;
            
            _nodeVisualizations[nodeState.NodeId] = nodeViz;
        }
        
        /// <summary>
        /// Update branch visualization state
        /// </summary>
        private void UpdateBranchVisualization(SkillBranch branch, BranchProgressionState branchState)
        {
            if (!_branchVisualizations.TryGetValue(branch, out var branchViz))
                return;
            
            branchViz.TargetVibrancy = branchState.BranchVibrancy;
            branchViz.GrowthLevel = branchState.BranchGrowthLevel;
        }
        
        /// <summary>
        /// Get position for a node within its branch
        /// </summary>
        private Vector3 GetNodePosition(SkillNodeState nodeState)
        {
            // Simple positioning along the branch
            var branchNodes = _nodeVisualizations.Values.Count(n => n.NodeType == nodeState.NodeType);
            var nodeIndex = branchNodes;
            var spacing = 0.5f;
            
            return new Vector3(0, nodeIndex * spacing, 0);
        }
        
        /// <summary>
        /// Create a simple mesh for node visualization
        /// </summary>
        private Mesh CreateNodeMesh()
        {
            // Create a simple sphere-like mesh for nodes
            var mesh = new Mesh();
            
            // Simple cube vertices for now
            var vertices = new Vector3[]
            {
                new Vector3(-0.1f, -0.1f, -0.1f), // Bottom vertices
                new Vector3( 0.1f, -0.1f, -0.1f),
                new Vector3( 0.1f, -0.1f,  0.1f),
                new Vector3(-0.1f, -0.1f,  0.1f),
                new Vector3(-0.1f,  0.1f, -0.1f), // Top vertices
                new Vector3( 0.1f,  0.1f, -0.1f),
                new Vector3( 0.1f,  0.1f,  0.1f),
                new Vector3(-0.1f,  0.1f,  0.1f)
            };
            
            var triangles = new int[]
            {
                0, 1, 2, 2, 3, 0, // Bottom
                4, 7, 6, 6, 5, 4, // Top
                0, 4, 5, 5, 1, 0, // Front
                2, 6, 7, 7, 3, 2, // Back
                0, 3, 7, 7, 4, 0, // Left
                1, 5, 6, 6, 2, 1  // Right
            };
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            
            return mesh;
        }
        
        /// <summary>
        /// Animate node unlock effect
        /// </summary>
        private System.Collections.IEnumerator AnimateNodeUnlockEffect(NodeVisualization nodeViz)
        {
            var animationDuration = 1.0f;
            var startTime = Time.time;
            var startScale = nodeViz.GameObject.transform.localScale;
            var targetScale = startScale * 1.2f;
            
            while (Time.time - startTime < animationDuration)
            {
                var progress = (Time.time - startTime) / animationDuration;
                var scale = Vector3.Lerp(startScale, targetScale, Mathf.Sin(progress * Mathf.PI));
                nodeViz.GameObject.transform.localScale = scale;
                
                yield return null;
            }
            
            // Reset to normal scale
            nodeViz.GameObject.transform.localScale = startScale;
        }
        
        // Stage-specific effect methods
        private void TriggerSeedlingEffects() { /* Implement seedling effects */ }
        private void TriggerVegetativeEffects() { /* Implement vegetative effects */ }
        private void TriggerMatureEffects() { /* Implement mature effects */ }
        private void TriggerFloweringEffects() { /* Implement flowering effects */ }
        private void TriggerFullyFloweredEffects() { /* Implement fully flowered effects */ }
        
        private void OnDestroy()
        {
            // Clean up resources
            foreach (var branchViz in _branchVisualizations.Values)
            {
                if (branchViz.GameObject != null)
                {
                    DestroyImmediate(branchViz.GameObject);
                }
            }
            _branchVisualizations.Clear();
            _nodeVisualizations.Clear();
        }
    }
    
    /// <summary>
    /// Visualization data for a skill tree branch
    /// </summary>
    [System.Serializable]
    public class BranchVisualization
    {
        public SkillBranch Branch;
        public GameObject GameObject;
        public Renderer Renderer;
        public float CurrentVibrancy;
        public float TargetVibrancy;
        public TreeGrowthLevel GrowthLevel;
    }
    
    /// <summary>
    /// Visualization data for a skill node
    /// </summary>
    [System.Serializable]
    public class NodeVisualization
    {
        public string NodeId;
        public SkillNodeType NodeType;
        public GameObject GameObject;
        public Renderer Renderer;
        public bool IsUnlocked;
        public float UnlockProgress;
    }
} 