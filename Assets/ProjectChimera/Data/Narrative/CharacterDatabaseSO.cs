
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Data.Narrative
{
    [CreateAssetMenu(fileName = "New Character Database", menuName = "Project Chimera/Narrative/Character Database")]
    public class CharacterDatabaseSO : ScriptableObject
    {
        public List<CharacterProfileSO> characters;
        
        [Header("Relationship Configuration")]
        public List<PredefinedRelationship> PredefinedRelationships = new List<PredefinedRelationship>();
        public List<EducationalRole> EducationalRoles = new List<EducationalRole>();

        // Properties for compatibility
        public List<CharacterProfileSO> Characters => characters;

        public CharacterProfileSO GetCharacter(string characterId)
        {
            return characters.Find(c => c.characterID == characterId);
        }

        public CharacterProfileSO GetCharacterById(string characterId)
        {
            return GetCharacter(characterId);
        }

        public List<CharacterProfileSO> GetAllCharacters()
        {
            return new List<CharacterProfileSO>(characters);
        }
    }

    [System.Serializable]
    public class PredefinedRelationship
    {
        public string CharacterAId;
        public string CharacterBId;
        public RelationshipType RelationshipType;
        public float InitialTrustLevel = 0.5f;
        public float InitialRespectLevel = 0.5f;
        public float InitialInfluenceLevel = 0.5f;
        
        // Additional properties for CharacterRelationshipSystem compatibility
        public bool IsAntagonistic;
        public float ConflictLevel;
        public List<string> SharedHistory = new List<string>();
    }

    [System.Serializable]
    public class EducationalRole
    {
        public string CharacterId;
        public List<CultivationExpertise> ExpertiseAreas = new List<CultivationExpertise>();
        public bool IsMentor = false;
        public float TeachingEffectiveness = 0.8f;
        public CultivationExpertise Expertise = CultivationExpertise.Beginner;
        public float CredibilityLevel = 0.8f;
        public List<string> ValidatedKnowledgeAreas = new List<string>();
        public bool IsScientificallyAccurate = true;
    }
}
