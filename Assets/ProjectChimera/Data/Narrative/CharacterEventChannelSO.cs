using System;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Narrative
{
    /// <summary>
    /// Event channel for character-related narrative events
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterEventChannel", menuName = "Project Chimera/Narrative/Character Event Channel")]
    public class CharacterEventChannelSO : GameEventSO<CharacterEventData>
    {
        [Header("Character Event Settings")]
        [SerializeField] private bool _logEvents = true;
        
        /// <summary>
        /// C# event for direct subscription (alternative to listener pattern)
        /// </summary>
        public event Action<CharacterEventData> OnEventRaised;
        
        public new void Raise(CharacterEventData characterData)
        {
            if (_logEvents)
            {
                Debug.Log($"[CharacterEvent] {characterData?.CharacterId}: {characterData?.EventType}");
            }
            
            // Raise C# event
            OnEventRaised?.Invoke(characterData);
            
            // Also call base implementation for listener pattern
            base.Raise(characterData);
        }
    }
    
    /// <summary>
    /// Data structure for character events (extending existing CharacterEventData if needed)
    /// </summary>
    [System.Serializable]
    public class CharacterEventInfo : CharacterEventData
    {
        public string EventDescription;
        public float EventImpact;
        public string LocationId;
    }
}