using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Events;
using ProjectChimera.Events;
using EventPlayerChoiceEventData = ProjectChimera.Events.PlayerChoiceEventData;
using DataCultivationPathData = ProjectChimera.Data.Cultivation.CultivationPathData;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Manages cultivation learning paths and player progression routes
    /// </summary>
    public class CultivationPathManager : MonoBehaviour
    {
        [Header("Path Management")]
        [SerializeField] private CultivationPathLibrarySO _pathLibrary;
        [SerializeField] private List<DataCultivationPathData> _activePaths = new List<DataCultivationPathData>();
        
        private bool _isInitialized = false;
        
        public void Initialize(CultivationPathLibrarySO pathLibrary)
        {
            _pathLibrary = pathLibrary;
            _isInitialized = true;
        }
        
        public void UpdateSystem(float deltaTime)
        {
            if (!_isInitialized) return;
            
            // Update cultivation path system
            foreach (var path in _activePaths)
            {
                UpdatePathProgress(path, deltaTime);
            }
        }
        
        public void StartPath(string pathId)
        {
            var pathData = _pathLibrary.GetPath(pathId);
            if (pathData != null && !_activePaths.Contains(pathData))
            {
                _activePaths.Add(pathData);
            }
        }
        
        public void CompletePath(string pathId)
        {
            var path = _activePaths.Find(p => p.PathId == pathId);
            if (path != null)
            {
                path.ProgressData.IsCompleted = true;
                path.ProgressData.CompletionDate = System.DateTime.Now;
            }
        }
        
        public void UpdatePath(EventPlayerChoiceEventData choiceData)
        {
            if (!_isInitialized) return;
            
            // Update active paths based on player choice
            foreach (var path in _activePaths)
            {
                if (!path.IsCompleted())
                {
                    UpdatePathBasedOnChoice(path, choiceData);
                }
            }
        }
        
        private void UpdatePathBasedOnChoice(DataCultivationPathData path, EventPlayerChoiceEventData choiceData)
        {
            // Apply choice influence to path progression
            if (!string.IsNullOrEmpty(choiceData.ChoiceId))
            {
                // Simulate path progression based on player choices
                var progressGain = CalculateChoiceProgressGain(choiceData);
                path.ProgressData.TotalTimeSpent += progressGain;
                
                // Check if this choice unlocks new path segments
                CheckForPathUnlocks(path, choiceData);
            }
        }
        
        private float CalculateChoiceProgressGain(EventPlayerChoiceEventData choiceData)
        {
            // Simple progress calculation based on choice complexity
            return 0.1f; // Base progress gain
        }
        
        private void CheckForPathUnlocks(DataCultivationPathData path, EventPlayerChoiceEventData choiceData)
        {
            // Check if player choices have unlocked new path segments
            var currentStage = path.GetCurrentStage();
            if (currentStage != null && !string.IsNullOrEmpty(choiceData.ChoiceDescription))
            {
                // Simplified unlock logic
                if (choiceData.ChoiceDescription.Length > 0)
                {
                    currentStage.ProgressData.TimeSpent += 0.5f;
                }
            }
        }
        
        private void UpdatePathProgress(DataCultivationPathData path, float deltaTime)
        {
            if (path.IsCompleted()) return;
            
            // Update path progression logic
            path.ProgressData.TotalTimeSpent += deltaTime / 3600f; // Convert to hours
            
            // Example progression - advance through stages based on time
            var currentStage = path.GetCurrentStage();
            if (currentStage != null && !currentStage.IsCompleted())
            {
                // Simulate learning progress
                currentStage.ProgressData.TimeSpent += deltaTime / 3600f;
                
                // Check if stage should be completed (simplified logic)
                if (currentStage.ProgressData.TimeSpent >= currentStage.EstimatedHours)
                {
                    currentStage.ProgressData.IsCompleted = true;
                    currentStage.ProgressData.CompletionDate = System.DateTime.Now;
                    path.AdvanceToNextStage();
                }
            }
        }
    }
}