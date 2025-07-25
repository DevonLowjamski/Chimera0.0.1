using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics.Scientific;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Reputation Configuration - Defines reputation systems, disciplines, and social recognition
    /// for scientific gaming systems
    /// </summary>
    [CreateAssetMenu(fileName = "New Reputation Config", menuName = "Project Chimera/Genetics/Reputation Config")]
    public class ReputationConfigSO : ChimeraConfigSO
    {
        [Header("Reputation Settings")]
        [Range(0f, 10000f)] public float MaxReputation = 1000f;
        [Range(0f, 1000f)] public float StartingReputation = 0f;
        [Range(0.01f, 1.0f)] public float ReputationDecayRate = 0.05f;
        [Range(0.1f, 5.0f)] public float ReputationGainMultiplier = 1.0f;
        
        [Header("Scientific Disciplines")]
        public List<ScientificDisciplineTemplate> DisciplineTemplates = new List<ScientificDisciplineTemplate>();
        public List<ReputationSourceTemplate> ReputationSources = new List<ReputationSourceTemplate>();
        public List<ReputationTierTemplate> ReputationTiers = new List<ReputationTierTemplate>();
        
        [Header("Social Recognition")]
        public List<RecognitionTemplate> RecognitionTemplates = new List<RecognitionTemplate>();
        public bool EnablePeerEndorsement = true;
        public bool EnableExpertRecognition = true;
        [Range(0.1f, 2.0f)] public float SocialMultiplier = 1.2f;
        
        [Header("Cross-System Integration")]
        public bool EnableCrossSystemReputation = true;
        [Range(0.1f, 1.0f)] public float CrossSystemTransferRate = 0.3f;
        public List<string> IntegratedSystems = new List<string>();
        
        [Header("Visual Configuration")]
        public Color HighReputationColor = Color.gold;
        public Color MediumReputationColor = Color.cyan;
        public Color LowReputationColor = Color.white;
        public Color NegativeReputationColor = Color.red;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (DisciplineTemplates.Count == 0)
            {
                Debug.LogWarning("No scientific disciplines defined", this);
            }
            
            if (ReputationSources.Count == 0)
            {
                Debug.LogWarning("No reputation sources defined", this);
            }
            
            if (ReputationDecayRate > 0.5f)
            {
                Debug.LogWarning("Reputation decay rate is very high", this);
            }
        }
    }
    
    [System.Serializable]
    public class ScientificDisciplineTemplate
    {
        public string DisciplineId;
        public string DisciplineName;
        public ScientificDiscipline Discipline;
        public string Description;
        [Range(0.1f, 5.0f)] public float ReputationMultiplier = 1.0f;
        [Range(0.1f, 3.0f)] public float ExpertiseBonus = 1.2f;
        public Color DisciplineColor = Color.white;
        public Sprite DisciplineIcon;
        public List<string> CoreCompetencies = new List<string>();
        public List<string> RelatedDisciplines = new List<string>();
    }
    
    [System.Serializable]
    public class ReputationSourceTemplate
    {
        public string SourceId;
        public string SourceName;
        public ReputationSource Source;
        public string Description;
        [Range(0.1f, 10.0f)] public float BaseReputationValue = 1.0f;
        [Range(0.1f, 5.0f)] public float QualityMultiplier = 1.0f;
        public List<ScientificDiscipline> ApplicableDisciplines = new List<ScientificDiscipline>();
        public bool RequiresValidation = false;
        public bool IsRecurring = false;
    }
    
    [System.Serializable]
    public class ReputationTierTemplate
    {
        public string TierId;
        public string TierName;
        public ReputationTier Tier;
        [Range(0f, 10000f)] public float MinimumReputation = 0f;
        [Range(0f, 10000f)] public float MaximumReputation = 1000f;
        public Color TierColor = Color.white;
        public Sprite TierIcon;
        public List<string> UnlockedFeatures = new List<string>();
        public List<string> SpecialPrivileges = new List<string>();
        [Range(0.1f, 3.0f)] public float TierMultiplier = 1.0f;
    }
    
    [System.Serializable]
    public class RecognitionTemplate
    {
        public string RecognitionId;
        public string RecognitionName;
        public RecognitionType Type;
        public string Description;
        [Range(1f, 1000f)] public float ReputationRequirement = 100f;
        [Range(0.1f, 10.0f)] public float RecognitionValue = 5.0f;
        public List<ScientificDiscipline> EligibleDisciplines = new List<ScientificDiscipline>();
        public Sprite RecognitionIcon;
        public bool IsExclusive = false;
        public bool IsTemporary = false;
        public float Duration = 0f; // 0 = permanent
    }
    
    public enum ReputationTier
    {
        Novice,
        Apprentice,
        Practitioner,
        Expert,
        Master,
        Grandmaster,
        Legend
    }
    
    // RecognitionType moved to AchievementDataStructures.cs to resolve namespace conflict
}