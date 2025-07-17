using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Core.Logging;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Manages the unlocking of skill nodes and their associated mechanics
    /// </summary>
    public class SkillNodeUnlockManager : MonoBehaviour
    {
        [Header("Unlock Configuration")]
        [SerializeField] private float _unlockAnimationDuration = 1.0f;
        [SerializeField] private float _unlockCooldown = 0.5f;
        
        private bool _isInitialized = false;
        private Dictionary<string, float> _lastUnlockTimes = new Dictionary<string, float>();
        private Queue<SkillNodeUnlockRequest> _unlockQueue = new Queue<SkillNodeUnlockRequest>();
        private bool _isProcessingUnlock = false;
        
        public void Initialize()
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("SkillNodeUnlockManager already initialized", this);
                return;
            }
            
            _isInitialized = true;
            ChimeraLogger.Log("SkillNodeUnlockManager initialized successfully", this);
        }
        
        /// <summary>
        /// Queue a skill node for unlocking
        /// </summary>
        public void QueueNodeUnlock(SkillNodeUnlockRequest request)
        {
            if (!_isInitialized)
            {
                ChimeraLogger.LogWarning("SkillNodeUnlockManager not initialized", this);
                return;
            }
            
            // Check cooldown
            if (_lastUnlockTimes.ContainsKey(request.NodeId))
            {
                var timeSinceLastUnlock = Time.time - _lastUnlockTimes[request.NodeId];
                if (timeSinceLastUnlock < _unlockCooldown)
                {
                    ChimeraLogger.LogWarning($"Node {request.NodeId} is on cooldown", this);
                    return;
                }
            }
            
            _unlockQueue.Enqueue(request);
            
            if (!_isProcessingUnlock)
            {
                StartCoroutine(ProcessUnlockQueue());
            }
        }
        
        /// <summary>
        /// Process the unlock queue
        /// </summary>
        private System.Collections.IEnumerator ProcessUnlockQueue()
        {
            _isProcessingUnlock = true;
            
            while (_unlockQueue.Count > 0)
            {
                var request = _unlockQueue.Dequeue();
                yield return StartCoroutine(ProcessSingleUnlock(request));
                
                // Wait for cooldown between unlocks
                yield return new WaitForSeconds(_unlockCooldown);
            }
            
            _isProcessingUnlock = false;
        }
        
        /// <summary>
        /// Process a single node unlock
        /// </summary>
        private System.Collections.IEnumerator ProcessSingleUnlock(SkillNodeUnlockRequest request)
        {
            ChimeraLogger.Log($"Processing unlock for node: {request.NodeId}", this);
            
            // Record unlock time
            _lastUnlockTimes[request.NodeId] = Time.time;
            
            // Play unlock animation
            yield return StartCoroutine(PlayUnlockAnimation(request));
            
            // Execute unlock logic
            ExecuteUnlockLogic(request);
            
            // Notify completion
            request.OnUnlockComplete?.Invoke(request.NodeId, true);
            
            ChimeraLogger.Log($"Node unlock completed: {request.NodeId}", this);
        }
        
        /// <summary>
        /// Play unlock animation
        /// </summary>
        private System.Collections.IEnumerator PlayUnlockAnimation(SkillNodeUnlockRequest request)
        {
            // Placeholder for unlock animation
            // In a real implementation, this would trigger visual effects
            
            var startTime = Time.time;
            var duration = _unlockAnimationDuration;
            
            while (Time.time - startTime < duration)
            {
                var progress = (Time.time - startTime) / duration;
                
                // Update animation progress
                request.OnAnimationProgress?.Invoke(progress);
                
                yield return null;
            }
            
            // Ensure animation completes at 100%
            request.OnAnimationProgress?.Invoke(1.0f);
        }
        
        /// <summary>
        /// Execute the actual unlock logic
        /// </summary>
        private void ExecuteUnlockLogic(SkillNodeUnlockRequest request)
        {
            // Apply unlock effects based on node type
            switch (request.NodeType)
            {
                case SkillNodeType.PlantCare:
                    UnlockPlantCareAbilities(request);
                    break;
                case SkillNodeType.EnvironmentalControl:
                    UnlockEnvironmentalControlAbilities(request);
                    break;
                case SkillNodeType.Genetics:
                    UnlockGeneticsAbilities(request);
                    break;
                case SkillNodeType.Automation:
                    UnlockAutomationAbilities(request);
                    break;
                case SkillNodeType.Construction:
                    UnlockConstructionAbilities(request);
                    break;
                case SkillNodeType.Business:
                    UnlockBusinessAbilities(request);
                    break;
                case SkillNodeType.Science:
                    UnlockScienceAbilities(request);
                    break;
                default:
                    ChimeraLogger.LogWarning($"Unknown node type: {request.NodeType}", this);
                    break;
            }
        }
        
        /// <summary>
        /// Unlock plant care abilities
        /// </summary>
        private void UnlockPlantCareAbilities(SkillNodeUnlockRequest request)
        {
            ChimeraLogger.Log($"Unlocking plant care abilities for node: {request.NodeId}", this);
            
            // Enable new plant care actions
            // This would integrate with the plant care system
        }
        
        /// <summary>
        /// Unlock environmental control abilities
        /// </summary>
        private void UnlockEnvironmentalControlAbilities(SkillNodeUnlockRequest request)
        {
            ChimeraLogger.Log($"Unlocking environmental control abilities for node: {request.NodeId}", this);
            
            // Enable new environmental control features
            // This would integrate with the environmental control system
        }
        
        /// <summary>
        /// Unlock genetics abilities
        /// </summary>
        private void UnlockGeneticsAbilities(SkillNodeUnlockRequest request)
        {
            ChimeraLogger.Log($"Unlocking genetics abilities for node: {request.NodeId}", this);
            
            // Enable new genetics features
            // This would integrate with the genetics system
        }
        
        /// <summary>
        /// Unlock automation abilities
        /// </summary>
        private void UnlockAutomationAbilities(SkillNodeUnlockRequest request)
        {
            ChimeraLogger.Log($"Unlocking automation abilities for node: {request.NodeId}", this);
            
            // Enable new automation features
            // This would integrate with the automation system
        }
        
        /// <summary>
        /// Unlock construction abilities
        /// </summary>
        private void UnlockConstructionAbilities(SkillNodeUnlockRequest request)
        {
            ChimeraLogger.Log($"Unlocking construction abilities for node: {request.NodeId}", this);
            
            // Enable new construction features
            // This would integrate with the construction system
        }
        
        /// <summary>
        /// Unlock business abilities
        /// </summary>
        private void UnlockBusinessAbilities(SkillNodeUnlockRequest request)
        {
            ChimeraLogger.Log($"Unlocking business abilities for node: {request.NodeId}", this);
            
            // Enable new business features
            // This would integrate with the business system
        }
        
        /// <summary>
        /// Unlock science abilities
        /// </summary>
        private void UnlockScienceAbilities(SkillNodeUnlockRequest request)
        {
            ChimeraLogger.Log($"Unlocking science abilities for node: {request.NodeId}", this);
            
            // Enable new science features
            // This would integrate with the science system
        }
        
        /// <summary>
        /// Process node unlock directly - called by TreeSkillProgressionSystem
        /// </summary>
        public void ProcessNodeUnlock(string nodeId, SkillNodeState nodeState)
        {
            if (!_isInitialized)
            {
                ChimeraLogger.LogWarning("SkillNodeUnlockManager not initialized", this);
                return;
            }

            var request = new SkillNodeUnlockRequest
            {
                NodeId = nodeId,
                NodeType = nodeState.NodeType,
                Branch = (SkillTreeBranch)nodeState.Branch
            };

            QueueNodeUnlock(request);
        }

        /// <summary>
        /// Check if a node is currently being unlocked
        /// </summary>
        public bool IsNodeUnlocking(string nodeId)
        {
            return _unlockQueue.ToArray().Any(request => request.NodeId == nodeId);
        }
        
        /// <summary>
        /// Get the number of pending unlocks
        /// </summary>
        public int GetPendingUnlockCount()
        {
            return _unlockQueue.Count;
        }
        
        /// <summary>
        /// Clear all pending unlocks
        /// </summary>
        public void ClearUnlockQueue()
        {
            _unlockQueue.Clear();
            ChimeraLogger.Log("Unlock queue cleared", this);
        }
        
        private void OnDestroy()
        {
            if (_isProcessingUnlock)
            {
                StopAllCoroutines();
            }
        }
    }
    
    /// <summary>
    /// Request data for skill node unlock
    /// </summary>
    [System.Serializable]
    public class SkillNodeUnlockRequest
    {
        public string NodeId;
        public SkillNodeType NodeType;
        public SkillTreeBranch Branch;
        public System.Action<string, bool> OnUnlockComplete;
        public System.Action<float> OnAnimationProgress;
        public Dictionary<string, object> UnlockData = new Dictionary<string, object>();
    }
} 