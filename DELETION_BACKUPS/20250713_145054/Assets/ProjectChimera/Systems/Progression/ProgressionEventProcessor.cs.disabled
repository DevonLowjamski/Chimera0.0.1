using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Data.Progression;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Progression event processor for handling progression events
    /// </summary>
    public class ProgressionEventProcessor
    {
        private ComprehensiveProgressionManager _progressionManager;
        private Queue<ProgressionEvent> _eventQueue = new Queue<ProgressionEvent>();
        private bool _isProcessing = false;
        
        public ProgressionEventProcessor(ComprehensiveProgressionManager manager)
        {
            _progressionManager = manager;
        }
        
        /// <summary>
        /// Add event to processing queue
        /// </summary>
        public void QueueEvent(ProgressionEvent progressionEvent)
        {
            if (progressionEvent != null && !progressionEvent.IsProcessed)
            {
                _eventQueue.Enqueue(progressionEvent);
            }
        }
        
        /// <summary>
        /// Process all queued events
        /// </summary>
        public void ProcessQueuedEvents()
        {
            if (_isProcessing) return;
            
            _isProcessing = true;
            
            while (_eventQueue.Count > 0)
            {
                var progressionEvent = _eventQueue.Dequeue();
                ProcessEvent(progressionEvent);
            }
            
            _isProcessing = false;
        }
        
        /// <summary>
        /// Process a single progression event
        /// </summary>
        public void ProcessEvent(ProgressionEvent progressionEvent)
        {
            if (progressionEvent == null || progressionEvent.IsProcessed)
                return;
            
            switch (progressionEvent.EventType)
            {
                case ProgressionEventType.Skill_Level_Up:
                    ProcessSkillLevelUpEvent(progressionEvent);
                    break;
                    
                case ProgressionEventType.Research_Completed:
                    ProcessResearchCompletedEvent(progressionEvent);
                    break;
                    
                case ProgressionEventType.Achievement_Unlocked:
                    ProcessAchievementUnlockedEvent(progressionEvent);
                    break;
                    
                case ProgressionEventType.Content_Unlocked:
                    ProcessContentUnlockedEvent(progressionEvent);
                    break;
                    
                case ProgressionEventType.Experience_Gained:
                    ProcessExperienceGainedEvent(progressionEvent);
                    break;
                    
                case ProgressionEventType.Milestone_Reached:
                    ProcessMilestoneReachedEvent(progressionEvent);
                    break;
                    
                case ProgressionEventType.Plant_Harvested:
                    ProcessPlantHarvestedEvent(progressionEvent);
                    break;
                    
                case ProgressionEventType.Construction_Completed:
                    ProcessConstructionCompletedEvent(progressionEvent);
                    break;
                    
                case ProgressionEventType.Sale_Completed:
                    ProcessSaleCompletedEvent(progressionEvent);
                    break;
                    
                default:
                    Debug.LogWarning($"Unknown progression event type: {progressionEvent.EventType}");
                    break;
            }
            
            // Mark event as processed
            progressionEvent.IsProcessed = true;
            progressionEvent.EventTime = System.DateTime.Now;
        }
        
        private void ProcessSkillLevelUpEvent(ProgressionEvent progressionEvent)
        {
            // Handle skill level up consequences
            Debug.Log($"Processing skill level up: {progressionEvent.TargetId} to level {progressionEvent.EventValue}");
            
            // Check for achievement unlocks
            _progressionManager.CheckAchievementProgress();
            
            // Check for content unlocks
            _progressionManager.CheckContentUnlocks();
        }
        
        private void ProcessResearchCompletedEvent(ProgressionEvent progressionEvent)
        {
            // Handle research completion consequences
            Debug.Log($"Processing research completion: {progressionEvent.TargetId}");
            
            // Check for achievement unlocks
            _progressionManager.CheckAchievementProgress();
            
            // Check for content unlocks
            _progressionManager.CheckContentUnlocks();
        }
        
        private void ProcessAchievementUnlockedEvent(ProgressionEvent progressionEvent)
        {
            // Handle achievement unlock consequences
            Debug.Log($"Processing achievement unlock: {progressionEvent.TargetId}");
            
            // Apply achievement rewards
            // Check for additional content unlocks
        }
        
        private void ProcessContentUnlockedEvent(ProgressionEvent progressionEvent)
        {
            // Handle content unlock consequences
            Debug.Log($"Processing content unlock: {progressionEvent.TargetId}");
        }
        
        private void ProcessExperienceGainedEvent(ProgressionEvent progressionEvent)
        {
            // Handle experience gain consequences
            Debug.Log($"Processing experience gain: {progressionEvent.EventValue} from {progressionEvent.SourceSystem}");
        }
        
        private void ProcessMilestoneReachedEvent(ProgressionEvent progressionEvent)
        {
            // Handle milestone reached consequences
            Debug.Log($"Processing milestone reached: {progressionEvent.TargetId}");
            
            // Check for achievement unlocks
            _progressionManager.CheckAchievementProgress();
        }
        
        private void ProcessPlantHarvestedEvent(ProgressionEvent progressionEvent)
        {
            // Handle plant harvest consequences
            Debug.Log($"Processing plant harvest: {progressionEvent.TargetId}");
            
            // Update cultivation statistics
            // Check for cultivation achievements
        }
        
        private void ProcessConstructionCompletedEvent(ProgressionEvent progressionEvent)
        {
            // Handle construction completion consequences
            Debug.Log($"Processing construction completion: {progressionEvent.TargetId}");
            
            // Update construction statistics
            // Check for construction achievements
        }
        
        private void ProcessSaleCompletedEvent(ProgressionEvent progressionEvent)
        {
            // Handle sale completion consequences
            Debug.Log($"Processing sale completion: {progressionEvent.EventValue} profit");
            
            // Update business statistics
            // Check for business achievements
        }
        
        /// <summary>
        /// Get number of queued events
        /// </summary>
        public int GetQueuedEventCount()
        {
            return _eventQueue.Count;
        }
        
        /// <summary>
        /// Clear all queued events
        /// </summary>
        public void ClearQueue()
        {
            _eventQueue.Clear();
        }
    }
} 