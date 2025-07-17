using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Narrative
{
    /// <summary>
    /// Event channel for consequence-related narrative events
    /// </summary>
    [CreateAssetMenu(fileName = "ConsequenceEventChannel", menuName = "Project Chimera/Narrative/Consequence Event Channel")]
    public class ConsequenceEventChannelSO : GameEventSO<ConsequenceData>
    {
        [Header("Consequence Event Settings")]
        [SerializeField] private bool _logEvents = true;
        
        public new void Raise(ConsequenceData consequenceData)
        {
            if (_logEvents)
            {
                Debug.Log($"[ConsequenceEvent] {consequenceData?.TargetId}: {consequenceData?.Description}");
            }
            
            base.Raise(consequenceData);
        }
    }
    
}