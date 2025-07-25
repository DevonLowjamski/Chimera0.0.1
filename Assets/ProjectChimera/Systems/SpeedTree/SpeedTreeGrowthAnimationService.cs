using UnityEngine;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.SpeedTree;
using System.Collections.Generic;
using System.Collections;

#if UNITY_SPEEDTREE
using SpeedTree;
#endif

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// PC013-8b: SpeedTreeGrowthAnimationService extracted from AdvancedSpeedTreeManager
    /// Handles growth animation, stage transitions, and morphological changes over time.
    /// Manages the dynamic growth visualization of cannabis plants through their lifecycle.
    /// </summary>
    public class SpeedTreeGrowthAnimationService
    {
        private readonly SpeedTreeGrowthConfigSO _growthConfig;
        private readonly bool _enableGrowthAnimation;
        private readonly float _growthAnimationSpeed;
        
        private Dictionary<int, GrowthAnimationState> _activeAnimations;
        private Dictionary<int, PlantGrowthStage> _plantStages;
        private Coroutine _growthUpdateCoroutine;
        private MonoBehaviour _coroutineHost;
        
        // Events for growth animation
        public System.Action<int, PlantGrowthStage> OnGrowthStageChanged;
        public System.Action<int, float> OnGrowthProgressUpdated;
        public System.Action<int, GrowthMilestone> OnGrowthMilestoneReached;
        
        public SpeedTreeGrowthAnimationService(SpeedTreeGrowthConfigSO growthConfig, 
            MonoBehaviour coroutineHost, bool enableGrowthAnimation = true, float growthAnimationSpeed = 1f)
        {
            _growthConfig = growthConfig;
            _coroutineHost = coroutineHost;
            _enableGrowthAnimation = enableGrowthAnimation;
            _growthAnimationSpeed = growthAnimationSpeed;
            
            _activeAnimations = new Dictionary<int, GrowthAnimationState>();
            _plantStages = new Dictionary<int, PlantGrowthStage>();
            
            if (_enableGrowthAnimation && _coroutineHost != null)
            {
                StartGrowthAnimationSystem();
            }
            
            Debug.Log($"[SpeedTreeGrowthAnimationService] Initialized with animation speed: {_growthAnimationSpeed}");
        }
        
        /// <summary>
        /// Starts growth animation for a plant instance.
        /// </summary>
        public void StartGrowthAnimation(SpeedTreePlantData plantInstance)
        {
            if (plantInstance == null || !_enableGrowthAnimation)
            {
                Debug.LogWarning("[SpeedTreeGrowthAnimationService] Cannot start growth animation: invalid parameters");
                return;
            }
            
            var animationState = new GrowthAnimationState
            {
                InstanceId = plantInstance.InstanceId,
                CurrentStage = plantInstance.CurrentGrowthStage,
                GrowthProgress = plantInstance.GrowthProgress,
                StartTime = Time.time,
                LastUpdateTime = Time.time,
                GrowthRate = CalculateGrowthRate(plantInstance),
                IsActive = true
            };
            
            _activeAnimations[plantInstance.InstanceId] = animationState;
            _plantStages[plantInstance.InstanceId] = plantInstance.CurrentGrowthStage;
            
            Debug.Log($"[SpeedTreeGrowthAnimationService] Started growth animation for plant: {plantInstance.InstanceId}");
        }
        
        /// <summary>
        /// Stops growth animation for a plant instance.
        /// </summary>
        public void StopGrowthAnimation(int instanceId)
        {
            if (_activeAnimations.TryGetValue(instanceId, out var animationState))
            {
                animationState.IsActive = false;
                _activeAnimations.Remove(instanceId);
                _plantStages.Remove(instanceId);
                
                Debug.Log($"[SpeedTreeGrowthAnimationService] Stopped growth animation for plant: {instanceId}");
            }
        }
        
        /// <summary>
        /// Updates growth progress for a specific plant.
        /// </summary>
        public void UpdateGrowthProgress(int instanceId, float deltaTime, SpeedTreePlantData plantData)
        {
            if (!_activeAnimations.TryGetValue(instanceId, out var animationState) || !animationState.IsActive)
                return;
            
            if (plantData?.Renderer == null)
                return;
            
            // Calculate growth increment
            var growthIncrement = CalculateGrowthIncrement(animationState, deltaTime, plantData);
            
            // Update growth progress
            var newProgress = Mathf.Clamp01(animationState.GrowthProgress + growthIncrement);
            var progressChanged = !Mathf.Approximately(newProgress, animationState.GrowthProgress);
            
            if (progressChanged)
            {
                animationState.GrowthProgress = newProgress;
                animationState.LastUpdateTime = Time.time;
                
                // Update plant data
                plantData.GrowthProgress = newProgress;
                
                // Apply visual changes
                ApplyGrowthVisualization(plantData, animationState);
                
                // Check for stage transitions
                CheckGrowthStageTransition(instanceId, animationState, plantData);
                
                // Check for milestones
                CheckGrowthMilestones(instanceId, animationState);
                
                OnGrowthProgressUpdated?.Invoke(instanceId, newProgress);
            }
        }
        
        /// <summary>
        /// Forces a growth stage transition.
        /// </summary>
        public void TransitionToGrowthStage(int instanceId, PlantGrowthStage newStage, SpeedTreePlantData plantData)
        {
            if (!_activeAnimations.TryGetValue(instanceId, out var animationState))
            {
                Debug.LogWarning($"[SpeedTreeGrowthAnimationService] No animation state for plant: {instanceId}");
                return;
            }
            
            var oldStage = animationState.CurrentStage;
            animationState.CurrentStage = newStage;
            _plantStages[instanceId] = newStage;
            
            if (plantData != null)
            {
                plantData.CurrentGrowthStage = newStage;
                
                // Apply stage-specific visual changes
                ApplyStageTransitionEffects(plantData, oldStage, newStage);
            }
            
            OnGrowthStageChanged?.Invoke(instanceId, newStage);
            
            Debug.Log($"[SpeedTreeGrowthAnimationService] Plant {instanceId} transitioned from {oldStage} to {newStage}");
        }
        
        /// <summary>
        /// Gets current growth animation state for a plant.
        /// </summary>
        public GrowthAnimationState GetGrowthAnimationState(int instanceId)
        {
            return _activeAnimations.TryGetValue(instanceId, out var state) ? state : null;
        }
        
        /// <summary>
        /// Gets growth animation statistics.
        /// </summary>
        public GrowthAnimationStats GetGrowthAnimationStats()
        {
            var activeCount = 0;
            var totalProgress = 0f;
            
            foreach (var animation in _activeAnimations.Values)
            {
                if (animation.IsActive)
                {
                    activeCount++;
                    totalProgress += animation.GrowthProgress;
                }
            }
            
            return new GrowthAnimationStats
            {
                ActiveAnimations = activeCount,
                TotalAnimations = _activeAnimations.Count,
                AverageProgress = activeCount > 0 ? totalProgress / activeCount : 0f,
                AnimationSpeed = _growthAnimationSpeed,
                SystemEnabled = _enableGrowthAnimation
            };
        }
        
        /// <summary>
        /// Cleans up resources and stops all animations.
        /// </summary>
        public void Cleanup()
        {
            if (_growthUpdateCoroutine != null && _coroutineHost != null)
            {
                _coroutineHost.StopCoroutine(_growthUpdateCoroutine);
                _growthUpdateCoroutine = null;
            }
            
            _activeAnimations.Clear();
            _plantStages.Clear();
            
            Debug.Log("[SpeedTreeGrowthAnimationService] Cleanup completed");
        }
        
        // Private methods
        private void StartGrowthAnimationSystem()
        {
            if (_coroutineHost != null)
            {
                _growthUpdateCoroutine = _coroutineHost.StartCoroutine(GrowthAnimationUpdateCoroutine());
            }
        }
        
        private IEnumerator GrowthAnimationUpdateCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.1f); // Update every 100ms
                
                var deltaTime = 0.1f;
                var animationsToUpdate = new List<int>(_activeAnimations.Keys);
                
                foreach (var instanceId in animationsToUpdate)
                {
                    if (_activeAnimations.TryGetValue(instanceId, out var animationState) && animationState.IsActive)
                    {
                        // This would need access to plant data - handled by orchestrator
                        // UpdateGrowthProgress(instanceId, deltaTime, plantData);
                    }
                }
            }
        }
        
        private float CalculateGrowthRate(SpeedTreePlantData plantInstance)
        {
            var baseRate = _growthConfig?.BaseGrowthRate ?? 0.01f;
            
            // Apply genetic modifiers
            var geneticModifier = plantInstance.GeneticData?.GrowthRateGenes?.ExpressedValue ?? 1f;
            
            // Apply environmental modifiers (would come from environmental service)
            var environmentalModifier = 1f;
            
            // Apply health modifier
            var healthModifier = Mathf.Lerp(0.5f, 1.2f, plantInstance.Health);
            
            return baseRate * geneticModifier * environmentalModifier * healthModifier * _growthAnimationSpeed;
        }
        
        private float CalculateGrowthIncrement(GrowthAnimationState animationState, float deltaTime, SpeedTreePlantData plantData)
        {
            var baseIncrement = animationState.GrowthRate * deltaTime;
            
            // Apply stage-specific growth modifiers
            var stageModifier = GetStageGrowthModifier(animationState.CurrentStage);
            
            // Apply stress factors
            var stressModifier = Mathf.Lerp(0.3f, 1f, 1f - plantData.StressLevel);
            
            return baseIncrement * stageModifier * stressModifier;
        }
        
        private float GetStageGrowthModifier(PlantGrowthStage stage)
        {
            return stage switch
            {
                PlantGrowthStage.Seed => 0.5f,
                PlantGrowthStage.Germination => 0.8f,
                PlantGrowthStage.Seedling => 1.2f,
                PlantGrowthStage.Vegetative => 1.5f,
                PlantGrowthStage.Flowering => 1.0f,
                PlantGrowthStage.Mature => 0.2f,
                _ => 1.0f
            };
        }
        
        private void ApplyGrowthVisualization(SpeedTreePlantData plantData, GrowthAnimationState animationState)
        {
            if (plantData?.Renderer == null) return;
            
            #if UNITY_SPEEDTREE
            var renderer = plantData.Renderer;
            
            // Scale based on growth progress
            var baseScale = Vector3.one;
            var targetScale = CalculateTargetScale(plantData, animationState);
            var currentScale = Vector3.Lerp(baseScale * 0.1f, targetScale, animationState.GrowthProgress);
            
            renderer.transform.localScale = currentScale;
            
            // Update material properties to reflect growth
            UpdateGrowthMaterialProperties(renderer, animationState);
            
            // Update geometry complexity based on growth stage
            UpdateGeometryComplexity(renderer, animationState);
            #endif
        }
        
        private Vector3 CalculateTargetScale(SpeedTreePlantData plantData, GrowthAnimationState animationState)
        {
            var baseScale = Vector3.one;
            
            // Apply genetic height variation
            var heightModifier = plantData.GeneticData?.HeightGenes?.ExpressedValue ?? 1f;
            
            // Apply stage-specific scaling
            var stageScale = GetStageScale(animationState.CurrentStage);
            
            return baseScale * heightModifier * stageScale;
        }
        
        private float GetStageScale(PlantGrowthStage stage)
        {
            return stage switch
            {
                PlantGrowthStage.Seed => 0.05f,
                PlantGrowthStage.Germination => 0.1f,
                PlantGrowthStage.Seedling => 0.3f,
                PlantGrowthStage.Vegetative => 0.8f,
                PlantGrowthStage.Flowering => 1.0f,
                PlantGrowthStage.Mature => 1.0f,
                _ => 1.0f
            };
        }
        
        private void UpdateGrowthMaterialProperties(SpeedTreeRenderer renderer, GrowthAnimationState animationState)
        {
            #if UNITY_SPEEDTREE
            var rendererComponent = renderer.GetComponent<Renderer>();
            if (rendererComponent?.materials == null) return;
            
            foreach (var material in rendererComponent.materials)
            {
                if (material != null)
                {
                    material.SetFloat("_GrowthProgress", animationState.GrowthProgress);
                    material.SetFloat("_StageIndex", (float)animationState.CurrentStage);
                    material.SetFloat("_Maturity", CalculateMaturityLevel(animationState));
                }
            }
            #endif
        }
        
        private void UpdateGeometryComplexity(SpeedTreeRenderer renderer, GrowthAnimationState animationState)
        {
            #if UNITY_SPEEDTREE
            // Adjust LOD based on growth stage for performance
            var lodBias = CalculateLODBias(animationState);
            
            // Apply complexity settings
            renderer.enableCrossFade = animationState.GrowthProgress > 0.5f;
            #endif
        }
        
        private float CalculateLODBias(GrowthAnimationState animationState)
        {
            // Younger plants can use simpler geometry
            return Mathf.Lerp(2f, 1f, animationState.GrowthProgress);
        }
        
        private float CalculateMaturityLevel(GrowthAnimationState animationState)
        {
            var stageWeight = animationState.CurrentStage switch
            {
                PlantGrowthStage.Seed => 0.0f,
                PlantGrowthStage.Germination => 0.1f,
                PlantGrowthStage.Seedling => 0.2f,
                PlantGrowthStage.Vegetative => 0.5f,
                PlantGrowthStage.Flowering => 0.8f,
                PlantGrowthStage.Mature => 1.0f,
                _ => 0.5f
            };
            
            return Mathf.Lerp(stageWeight, stageWeight + 0.2f, animationState.GrowthProgress);
        }
        
        private void CheckGrowthStageTransition(int instanceId, GrowthAnimationState animationState, SpeedTreePlantData plantData)
        {
            var currentStage = animationState.CurrentStage;
            var shouldTransition = false;
            var nextStage = currentStage;
            
            // Check if ready for next stage based on progress thresholds
            switch (currentStage)
            {
                case PlantGrowthStage.Seed when animationState.GrowthProgress >= 0.2f:
                    nextStage = PlantGrowthStage.Germination;
                    shouldTransition = true;
                    break;
                case PlantGrowthStage.Germination when animationState.GrowthProgress >= 0.4f:
                    nextStage = PlantGrowthStage.Seedling;
                    shouldTransition = true;
                    break;
                case PlantGrowthStage.Seedling when animationState.GrowthProgress >= 0.6f:
                    nextStage = PlantGrowthStage.Vegetative;
                    shouldTransition = true;
                    break;
                case PlantGrowthStage.Vegetative when animationState.GrowthProgress >= 0.8f:
                    nextStage = PlantGrowthStage.Flowering;
                    shouldTransition = true;
                    break;
                case PlantGrowthStage.Flowering when animationState.GrowthProgress >= 1.0f:
                    nextStage = PlantGrowthStage.Mature;
                    shouldTransition = true;
                    break;
            }
            
            if (shouldTransition)
            {
                TransitionToGrowthStage(instanceId, nextStage, plantData);
            }
        }
        
        private void CheckGrowthMilestones(int instanceId, GrowthAnimationState animationState)
        {
            var milestones = new (float Progress, GrowthMilestone Milestone)[]
            {
                (0.25f, GrowthMilestone.FirstLeaves),
                (0.5f, GrowthMilestone.StructuralDevelopment),
                (0.75f, GrowthMilestone.FloweringOnset),
                (1.0f, GrowthMilestone.FullMaturity)
            };
            
            foreach (var milestone in milestones)
            {
                if (animationState.GrowthProgress >= milestone.Progress && !animationState.ReachedMilestones.Contains(milestone.Milestone))
                {
                    animationState.ReachedMilestones.Add(milestone.Milestone);
                    OnGrowthMilestoneReached?.Invoke(instanceId, milestone.Milestone);
                }
            }
        }
        
        private void ApplyStageTransitionEffects(SpeedTreePlantData plantData, PlantGrowthStage oldStage, PlantGrowthStage newStage)
        {
            if (plantData?.Renderer == null) return;
            
            // Apply visual effects for stage transition
            // This could include particle effects, material changes, etc.
            
            Debug.Log($"[SpeedTreeGrowthAnimationService] Applied transition effects: {oldStage} -> {newStage}");
        }
    }
    
    /// <summary>
    /// Growth animation state tracking.
    /// </summary>
    [System.Serializable]
    public class GrowthAnimationState
    {
        public int InstanceId;
        public PlantGrowthStage CurrentStage;
        public float GrowthProgress;
        public float StartTime;
        public float LastUpdateTime;
        public float GrowthRate;
        public bool IsActive;
        public List<GrowthMilestone> ReachedMilestones = new List<GrowthMilestone>();
    }
    
    /// <summary>
    /// Growth animation statistics.
    /// </summary>
    [System.Serializable]
    public class GrowthAnimationStats
    {
        public int ActiveAnimations;
        public int TotalAnimations;
        public float AverageProgress;
        public float AnimationSpeed;
        public bool SystemEnabled;
    }
    
}