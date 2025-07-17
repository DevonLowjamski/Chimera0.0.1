using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Data.Narrative;

namespace ProjectChimera.Systems.Narrative
{
    /// <summary>
    /// Individual character relationship implementation for Project Chimera's narrative system.
    /// Manages trust, respect, and influence levels with dynamic emotional modeling,
    /// memory tracking, and educational mentorship validation.
    /// </summary>
    [Serializable]
    public class CharacterRelationship : ICharacterRelationship
    {
        [Header("Core Relationship Data")]
        [SerializeField] private string _characterId;
        [SerializeField] private RelationshipType _type = RelationshipType.Neutral;
        [SerializeField] private float _trustLevel = 50f;
        [SerializeField] private float _respectLevel = 50f;
        [SerializeField] private float _influenceLevel = 25f;
        
        [Header("Emotional State")]
        [SerializeField] private EmotionalState _currentEmotionalState = EmotionalState.Neutral;
        [SerializeField] private float _emotionalIntensity = 0.5f;
        [SerializeField] private DateTime _lastEmotionalUpdate = DateTime.Now;
        
        [Header("Relationship Dynamics")]
        [SerializeField] private bool _isAntagonistic = false;
        [SerializeField] private float _conflictLevel = 0f;
        [SerializeField] private float _compatibility = 0.5f;
        [SerializeField] private float _loyaltyLevel = 50f;
        
        [Header("Memory and History")]
        [SerializeField] private List<SharedMemory> _sharedMemories = new List<SharedMemory>();
        [SerializeField] private List<RelationshipEvent> _relationshipHistory = new List<RelationshipEvent>();
        [SerializeField] private int _maxMemoryEntries = 100;
        [SerializeField] private int _maxHistoryEntries = 200;
        
        [Header("Influence and Impact")]
        [SerializeField] private Dictionary<string, float> _influenceAreas = new Dictionary<string, float>();
        [SerializeField] private List<string> _emotionalTriggers = new List<string>();
        [SerializeField] private float _persuasionResistance = 0.5f;
        
        // Character profile reference
        private CharacterProfileSO _characterProfile;
        
        // Events
        public event Action<float> OnTrustChanged;
        public event Action<float> OnRespectChanged;
        public event Action<RelationshipStatus> OnStatusChanged;
        
        // Properties
        public string CharacterId => _characterId;
        public RelationshipType Type 
        { 
            get => _type; 
            set => _type = value; 
        }
        public float TrustLevel 
        { 
            get => _trustLevel; 
            set => _trustLevel = Mathf.Clamp(value, 0f, 100f); 
        }
        public float RespectLevel 
        { 
            get => _respectLevel; 
            set => _respectLevel = Mathf.Clamp(value, 0f, 100f); 
        }
        public float InfluenceLevel 
        { 
            get => _influenceLevel; 
            set => _influenceLevel = Mathf.Clamp(value, 0f, 100f); 
        }
        public EmotionalState CurrentEmotionalState { get => _currentEmotionalState; set => _currentEmotionalState = value; }
        public float EmotionalIntensity { get => _emotionalIntensity; set => _emotionalIntensity = value; }
        public bool IsAntagonistic { get => _isAntagonistic; set => _isAntagonistic = value; }
        public float ConflictLevel { get => _conflictLevel; set => _conflictLevel = value; }
        public CharacterProfileSO CharacterProfile 
        { 
            get => _characterProfile; 
            set => _characterProfile = value; 
        }
        
        public CharacterRelationship(string characterId)
        {
            _characterId = characterId;
            _sharedMemories = new List<SharedMemory>();
            _relationshipHistory = new List<RelationshipEvent>();
            _influenceAreas = new Dictionary<string, float>();
            _emotionalTriggers = new List<string>();
        }
        
        #region ICharacterRelationship Implementation
        
        public void UpdateRelationship(PlayerAction action, ActionContext context)
        {
            var previousStatus = GetRelationshipStatus();
            
            // Calculate relationship changes based on action and character personality
            var changes = CalculateRelationshipChanges(action, context);
            
            // Apply changes
            ModifyTrust(changes.TrustChange);
            ModifyRespect(changes.RespectChange);
            ModifyInfluence(changes.InfluenceChange);
            
            // Update emotional state
            UpdateEmotionalState(action, context, changes);
            
            // Record relationship event
            RecordRelationshipEvent(action, context, changes);
            
            // Check for status change
            var newStatus = GetRelationshipStatus();
            if (newStatus != previousStatus)
            {
                OnStatusChanged?.Invoke(newStatus);
            }
            
            // Update compatibility if significant interaction
            if (Mathf.Abs(changes.TrustChange) > 5f || Mathf.Abs(changes.RespectChange) > 5f)
            {
                UpdateCompatibility(action, changes);
            }
        }
        
        public void ModifyTrust(float change)
        {
            var previousTrust = _trustLevel;
            _trustLevel = Mathf.Clamp(_trustLevel + change, 0f, 100f);
            
            if (Mathf.Abs(_trustLevel - previousTrust) > 0.01f)
            {
                OnTrustChanged?.Invoke(_trustLevel);
            }
        }
        
        public void ModifyRespect(float change)
        {
            var previousRespect = _respectLevel;
            _respectLevel = Mathf.Clamp(_respectLevel + change, 0f, 100f);
            
            if (Mathf.Abs(_respectLevel - previousRespect) > 0.01f)
            {
                OnRespectChanged?.Invoke(_respectLevel);
            }
        }
        
        public void ModifyInfluence(float change)
        {
            _influenceLevel = Mathf.Clamp(_influenceLevel + change, 0f, 100f);
        }
        
        public void AddSharedMemory(SharedMemory memory)
        {
            _sharedMemories.Add(memory);
            
            // Maintain memory limit
            if (_sharedMemories.Count > _maxMemoryEntries)
            {
                // Remove oldest memory that isn't marked as important
                var oldestNonImportant = _sharedMemories
                    .Where(m => m.EmotionalImpact < 0.8f)
                    .OrderBy(m => m.Timestamp)
                    .FirstOrDefault();
                
                if (oldestNonImportant != null)
                {
                    _sharedMemories.Remove(oldestNonImportant);
                }
                else
                {
                    // If all memories are important, remove the oldest one
                    _sharedMemories.RemoveAt(0);
                }
            }
        }
        
        public List<SharedMemory> GetSharedMemories()
        {
            return new List<SharedMemory>(_sharedMemories);
        }
        
        public List<RelationshipEvent> GetRelationshipHistory()
        {
            return new List<RelationshipEvent>(_relationshipHistory);
        }
        
        public bool CanInfluencePlayer()
        {
            // Character can influence player based on relationship level and their influence capacity
            var relationshipLevel = GetOverallRelationshipLevel();
            var minimumInfluenceThreshold = _isAntagonistic ? 30f : 40f;
            
            return relationshipLevel >= minimumInfluenceThreshold && _influenceLevel > 25f;
        }
        
        public float GetOverallRelationshipLevel()
        {
            // Weight trust and respect more heavily than influence
            return (_trustLevel * 0.4f + _respectLevel * 0.4f + _influenceLevel * 0.2f);
        }
        
        public RelationshipStatus GetRelationshipStatus()
        {
            var level = GetOverallRelationshipLevel();
            
            // Adjust thresholds based on antagonistic status
            if (_isAntagonistic)
            {
                return level switch
                {
                    < 15f => RelationshipStatus.Hostile,
                    < 35f => RelationshipStatus.Unfriendly,
                    < 55f => RelationshipStatus.Neutral,
                    < 75f => RelationshipStatus.Friendly,
                    < 90f => RelationshipStatus.Close,
                    _ => RelationshipStatus.Intimate
                };
            }
            else
            {
                return level switch
                {
                    < 20f => RelationshipStatus.Hostile,
                    < 40f => RelationshipStatus.Unfriendly,
                    < 60f => RelationshipStatus.Neutral,
                    < 80f => RelationshipStatus.Friendly,
                    < 95f => RelationshipStatus.Close,
                    _ => RelationshipStatus.Intimate
                };
            }
        }
        
        #endregion
        
        #region Advanced Relationship Logic
        
        private RelationshipChanges CalculateRelationshipChanges(PlayerAction action, ActionContext context)
        {
            var changes = new RelationshipChanges();
            
            // Base changes from action type
            var baseChanges = GetBaseActionChanges(action.Type);
            changes.TrustChange = baseChanges.TrustChange;
            changes.RespectChange = baseChanges.RespectChange;
            changes.InfluenceChange = baseChanges.InfluenceChange;
            
            // Apply personality modifiers
            if (_characterProfile != null)
            {
                ApplyPersonalityModifiers(ref changes, action, context);
            }
            
            // Apply relationship context modifiers
            ApplyContextModifiers(ref changes, context);
            
            // Apply current emotional state modifiers
            ApplyEmotionalStateModifiers(ref changes);
            
            // Apply compatibility modifiers
            changes.TrustChange *= _compatibility;
            changes.RespectChange *= _compatibility;
            
            // Apply antagonistic modifiers
            if (_isAntagonistic)
            {
                changes.TrustChange *= 0.5f; // Harder to build trust
                changes.RespectChange *= 0.7f; // Harder to gain respect
                
                // Negative actions have amplified effect
                if (changes.TrustChange < 0) changes.TrustChange *= 1.5f;
                if (changes.RespectChange < 0) changes.RespectChange *= 1.3f;
            }
            
            return changes;
        }
        
        private RelationshipChanges GetBaseActionChanges(ActionType actionType)
        {
            return actionType switch
            {
                ActionType.Help => new RelationshipChanges { TrustChange = 8f, RespectChange = 5f, InfluenceChange = 2f },
                ActionType.Support => new RelationshipChanges { TrustChange = 5f, RespectChange = 8f, InfluenceChange = 3f },
                ActionType.Dialogue => new RelationshipChanges { TrustChange = 1f, RespectChange = 1f, InfluenceChange = 0.5f },
                ActionType.Choice => new RelationshipChanges { TrustChange = 0f, RespectChange = 2f, InfluenceChange = 1f },
                ActionType.ItemGive => new RelationshipChanges { TrustChange = 3f, RespectChange = 2f, InfluenceChange = 1f },
                ActionType.Betray => new RelationshipChanges { TrustChange = -15f, RespectChange = -8f, InfluenceChange = -5f },
                ActionType.Ignore => new RelationshipChanges { TrustChange = -2f, RespectChange = -3f, InfluenceChange = -1f },
                ActionType.ItemTake => new RelationshipChanges { TrustChange = -5f, RespectChange = -3f, InfluenceChange = -2f },
                _ => new RelationshipChanges()
            };
        }
        
        private void ApplyPersonalityModifiers(ref RelationshipChanges changes, PlayerAction action, ActionContext context)
        {
            var personality = _characterProfile.Personality;
            
            // Trust modifiers based on personality
            if (personality.Skepticism > 0.7f)
            {
                changes.TrustChange *= 0.7f; // Skeptical characters are slower to trust
            }
            
            if (personality.Loyalty > 0.8f && changes.TrustChange > 0)
            {
                changes.TrustChange *= 1.2f; // Loyal characters value trust-building actions more
            }
            
            // Respect modifiers
            if (personality.Strictness > 0.7f)
            {
                changes.RespectChange *= 0.8f; // Strict characters are harder to impress
            }
            
            if (personality.Compassion > 0.8f && action.Type == ActionType.Help)
            {
                changes.RespectChange *= 1.3f; // Compassionate characters appreciate help
            }
            
            // Influence modifiers
            if (personality.Independence > 0.7f)
            {
                changes.InfluenceChange *= 0.6f; // Independent characters resist influence
            }
            
            if (personality.Leadership > 0.8f)
            {
                changes.InfluenceChange *= 1.2f; // Leaders understand influence dynamics
            }
        }
        
        private void ApplyContextModifiers(ref RelationshipChanges changes, ActionContext context)
        {
            // Witness modifier
            if (context.Witnesses != null && context.Witnesses.Count > 0)
            {
                // Public actions have amplified respect impact
                changes.RespectChange *= 1.2f;
                
                // But may reduce trust impact (less personal)
                changes.TrustChange *= 0.9f;
            }
            
            // Severity modifier
            var severityMultiplier = Mathf.Lerp(0.5f, 2.0f, context.Severity);
            changes.TrustChange *= severityMultiplier;
            changes.RespectChange *= severityMultiplier;
            changes.InfluenceChange *= severityMultiplier;
            
            // Location context
            if (context.AdditionalData.ContainsKey("location"))
            {
                var location = context.AdditionalData["location"].ToString();
                ApplyLocationModifiers(ref changes, location);
            }
        }
        
        private void ApplyLocationModifiers(ref RelationshipChanges changes, string location)
        {
            switch (location.ToLower())
            {
                case "private":
                    changes.TrustChange *= 1.3f; // Private settings build more trust
                    break;
                case "public":
                    changes.RespectChange *= 1.2f; // Public settings affect respect more
                    break;
                case "workplace":
                    changes.RespectChange *= 1.1f; // Professional context
                    changes.TrustChange *= 0.9f;
                    break;
                case "home":
                    changes.TrustChange *= 1.4f; // Home settings are very personal
                    changes.InfluenceChange *= 1.2f;
                    break;
            }
        }
        
        private void ApplyEmotionalStateModifiers(ref RelationshipChanges changes)
        {
            switch (_currentEmotionalState)
            {
                case EmotionalState.Happy:
                    changes.TrustChange *= 1.2f;
                    changes.RespectChange *= 1.1f;
                    break;
                case EmotionalState.Angry:
                    changes.TrustChange *= 0.5f;
                    changes.RespectChange *= 0.7f;
                    if (changes.TrustChange < 0) changes.TrustChange *= 1.5f; // Amplify negative trust
                    break;
                case EmotionalState.Sad:
                    changes.TrustChange *= 0.8f;
                    changes.RespectChange *= 0.9f;
                    break;
                case EmotionalState.Fearful:
                    changes.TrustChange *= 0.6f;
                    changes.InfluenceChange *= 0.5f;
                    break;
                case EmotionalState.Excited:
                    changes.TrustChange *= 1.1f;
                    changes.RespectChange *= 1.2f;
                    changes.InfluenceChange *= 1.3f;
                    break;
                case EmotionalState.Disappointed:
                    changes.RespectChange *= 0.6f;
                    if (changes.RespectChange > 0) changes.RespectChange *= 0.5f; // Harder to regain respect
                    break;
            }
            
            // Apply emotional intensity
            var intensityModifier = Mathf.Lerp(0.8f, 1.3f, _emotionalIntensity);
            changes.TrustChange *= intensityModifier;
            changes.RespectChange *= intensityModifier;
            changes.InfluenceChange *= intensityModifier;
        }
        
        private void UpdateEmotionalState(PlayerAction action, ActionContext context, RelationshipChanges changes)
        {
            var previousState = _currentEmotionalState;
            
            // Determine new emotional state based on relationship changes
            var totalChange = changes.TrustChange + changes.RespectChange + changes.InfluenceChange;
            
            if (totalChange > 10f)
            {
                _currentEmotionalState = EmotionalState.Happy;
                _emotionalIntensity = Mathf.Min(1f, _emotionalIntensity + 0.2f);
            }
            else if (totalChange > 5f)
            {
                _currentEmotionalState = EmotionalState.Excited;
                _emotionalIntensity = Mathf.Min(1f, _emotionalIntensity + 0.1f);
            }
            else if (totalChange < -10f)
            {
                _currentEmotionalState = action.Type == ActionType.Betray ? EmotionalState.Angry : EmotionalState.Disappointed;
                _emotionalIntensity = Mathf.Min(1f, _emotionalIntensity + 0.3f);
            }
            else if (totalChange < -5f)
            {
                _currentEmotionalState = EmotionalState.Sad;
                _emotionalIntensity = Mathf.Min(1f, _emotionalIntensity + 0.15f);
            }
            else
            {
                // Gradual return to neutral
                if (_currentEmotionalState != EmotionalState.Neutral)
                {
                    _emotionalIntensity = Mathf.Max(0f, _emotionalIntensity - 0.1f);
                    if (_emotionalIntensity < 0.2f)
                    {
                        _currentEmotionalState = EmotionalState.Neutral;
                        _emotionalIntensity = 0.5f;
                    }
                }
            }
            
            _lastEmotionalUpdate = DateTime.Now;
            
            // Add emotional trigger if significant change
            if (_currentEmotionalState != previousState && totalChange != 0f)
            {
                var trigger = $"{action.Type}_{context.ContextType}_{DateTime.Now:yyyy-MM-dd}";
                if (!_emotionalTriggers.Contains(trigger))
                {
                    _emotionalTriggers.Add(trigger);
                    
                    // Maintain trigger list size
                    if (_emotionalTriggers.Count > 20)
                    {
                        _emotionalTriggers.RemoveAt(0);
                    }
                }
            }
        }
        
        private void RecordRelationshipEvent(PlayerAction action, ActionContext context, RelationshipChanges changes)
        {
            var relationshipEvent = new RelationshipEvent
            {
                Action = action,
                Context = context,
                TrustChange = changes.TrustChange,
                RespectChange = changes.RespectChange,
                InfluenceChange = changes.InfluenceChange,
                Timestamp = DateTime.Now
            };
            
            _relationshipHistory.Add(relationshipEvent);
            
            // Maintain history limit
            if (_relationshipHistory.Count > _maxHistoryEntries)
            {
                _relationshipHistory.RemoveAt(0);
            }
        }
        
        private void UpdateCompatibility(PlayerAction action, RelationshipChanges changes)
        {
            // Update compatibility based on positive interactions
            var totalPositiveChange = Mathf.Max(0, changes.TrustChange) + Mathf.Max(0, changes.RespectChange);
            
            if (totalPositiveChange > 5f)
            {
                _compatibility = Mathf.Min(1f, _compatibility + 0.05f);
            }
            else if (changes.TrustChange < -5f || changes.RespectChange < -5f)
            {
                _compatibility = Mathf.Max(0f, _compatibility - 0.03f);
            }
        }
        
        #endregion
        
        #region Memory and History Methods
        
        /// <summary>
        /// Get memories from a specific time period
        /// </summary>
        public List<SharedMemory> GetRecentMemories(TimeSpan timeSpan)
        {
            var cutoffTime = DateTime.Now.Subtract(timeSpan);
            return _sharedMemories.Where(memory => memory.Timestamp >= cutoffTime).ToList();
        }
        
        /// <summary>
        /// Get memories by type
        /// </summary>
        public List<SharedMemory> GetMemoriesByType(MemoryType type)
        {
            return _sharedMemories.Where(memory => memory.Type == type).ToList();
        }
        
        /// <summary>
        /// Get most impactful memories
        /// </summary>
        public List<SharedMemory> GetMostImpactfulMemories(int count = 5)
        {
            return _sharedMemories
                .OrderByDescending(memory => memory.EmotionalImpact)
                .Take(count)
                .ToList();
        }
        
        /// <summary>
        /// Get relationship trend over time
        /// </summary>
        public RelationshipTrend GetRelationshipTrend(TimeSpan timeSpan)
        {
            var cutoffTime = DateTime.Now.Subtract(timeSpan);
            var recentEvents = _relationshipHistory.Where(e => e.Timestamp >= cutoffTime).ToList();
            
            if (recentEvents.Count < 2)
            {
                return RelationshipTrend.Stable;
            }
            
            var totalTrustChange = recentEvents.Sum(e => e.TrustChange);
            var totalRespectChange = recentEvents.Sum(e => e.RespectChange);
            var totalChange = totalTrustChange + totalRespectChange;
            
            return totalChange switch
            {
                > 10f => RelationshipTrend.Improving,
                < -10f => RelationshipTrend.Declining,
                _ => RelationshipTrend.Stable
            };
        }
        
        #endregion
        
        #region Influence and Persuasion
        
        /// <summary>
        /// Calculate influence power in specific area
        /// </summary>
        public float GetInfluencePower(string area)
        {
            var baseInfluence = _influenceLevel / 100f;
            
            if (_influenceAreas.TryGetValue(area, out var areaInfluence))
            {
                return baseInfluence * areaInfluence;
            }
            
            return baseInfluence * 0.5f; // Default influence in unknown areas
        }
        
        /// <summary>
        /// Set influence power in specific area
        /// </summary>
        public void SetInfluenceArea(string area, float power)
        {
            _influenceAreas[area] = Mathf.Clamp01(power);
        }
        
        /// <summary>
        /// Check if character would agree to a request
        /// </summary>
        public bool WouldAgreeToRequest(RequestType requestType, float requestDifficulty = 0.5f)
        {
            var baseWillingness = GetOverallRelationshipLevel() / 100f;
            
            // Adjust based on request type
            var typeModifier = requestType switch
            {
                RequestType.Information => 1.0f,
                RequestType.SmallFavor => 0.8f,
                RequestType.LargeFavor => 0.5f,
                RequestType.PersonalSecret => 0.3f,
                RequestType.RiskyAction => 0.2f,
                _ => 0.7f
            };
            
            var adjustedWillingness = baseWillingness * typeModifier;
            
            // Apply difficulty modifier
            adjustedWillingness *= (1f - requestDifficulty * 0.5f);
            
            // Apply personality factors
            if (_characterProfile != null)
            {
                if (_characterProfile.Personality.Loyalty > 0.8f)
                    adjustedWillingness *= 1.2f;
                if (_characterProfile.Personality.Compassion > 0.8f && requestType == RequestType.SmallFavor)
                    adjustedWillingness *= 1.3f;
                if (_characterProfile.Personality.Skepticism > 0.7f)
                    adjustedWillingness *= 0.8f;
            }
            
            return adjustedWillingness > 0.6f;
        }
        
        #endregion
    }
    
    // Supporting structures
    [Serializable]
    public struct RelationshipChanges
    {
        public float TrustChange;
        public float RespectChange;
        public float InfluenceChange;
    }
    
    public enum RelationshipTrend
    {
        Declining,
        Stable,
        Improving,
        Volatile
    }
    
    public enum RequestType
    {
        Information,
        SmallFavor,
        LargeFavor,
        PersonalSecret,
        RiskyAction,
        EmotionalSupport,
        BusinessPartnership
    }
}