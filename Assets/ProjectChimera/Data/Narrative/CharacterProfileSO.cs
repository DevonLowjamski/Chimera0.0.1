
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.Narrative;

namespace ProjectChimera.Data.Narrative
{
    [CreateAssetMenu(fileName = "New Character Profile", menuName = "Project Chimera/Narrative/Character Profile")]
    public class CharacterProfileSO : ScriptableObject
    {
        public string characterID;
        public string characterName;
        public string PersonalityId; // Added to fix CS1061
        public CharacterPersonality Personality; // Added for CharacterRelationshipSystem compatibility
        
        [Header("Default Relationship Values")]
        public float DefaultTrustLevel = 0.5f;
        public float DefaultRespectLevel = 0.5f;
        public float InfluenceCapacity = 1.0f;
        
        [Header("Educational Properties")]
        public bool CanTeach = false;
        public float TeachingEffectiveness = 0.7f;
        
        [Header("Character Type")]
        public bool IsCompanion = false;

        // Properties for compatibility
        public string CharacterName => characterName;
        public string CharacterId => characterID;
        
        // Additional properties for DialogueProcessingEngine compatibility
        public float CredibilityLevel = 0.8f;
        public CultivationExpertise Expertise = CultivationExpertise.None;
        public List<string> ValidatedKnowledgeAreas = new List<string>();

        // Other character properties...
    }
}
