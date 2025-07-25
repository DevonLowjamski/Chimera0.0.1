using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Terpene Library - Collection of terpene data for aromatic gaming system
    /// Contains comprehensive terpene information, properties, and interactions
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Terpene Library", menuName = "Project Chimera/Gaming/Terpene Library")]
    public class TerpeneLibrarySO : ChimeraDataSO
    {
        [Header("Terpene Collection")]
        public List<TerpeneData> Terpenes = new List<TerpeneData>();
        
        [Header("Terpene Categories")]
        public List<TerpeneCategoryData> Categories = new List<TerpeneCategoryData>();
        
        [Header("Terpene Interactions")]
        public List<TerpeneInteraction> Interactions = new List<TerpeneInteraction>();
        
        #region Runtime Methods
        
        public TerpeneData GetTerpene(string terpeneName)
        {
            return Terpenes.Find(t => t.TerpeneName == terpeneName);
        }
        
        public List<TerpeneData> GetTerpenesByCategory(TerpeneCategory category)
        {
            return Terpenes.FindAll(t => t.Category == category);
        }
        
        public List<TerpeneInteraction> GetTerpeneInteractions(string terpeneName)
        {
            return Interactions.FindAll(i => i.PrimaryTerpene == terpeneName || i.SecondaryTerpene == terpeneName);
        }
        
        #endregion
    }
    
    [System.Serializable]
    public class TerpeneData
    {
        public string TerpeneID;
        public string TerpeneName;
        public TerpeneCategory Category;
        public string ChemicalFormula;
        public float BoilingPoint;
        public List<string> AromaticDescriptors = new List<string>();
        public List<TerpeneEffect> Effects = new List<TerpeneEffect>();
        public float Concentration;
        public Sprite TerpeneIcon;
        public string Description;
    }
    
    [System.Serializable]
    public class TerpeneCategoryData
    {
        public string CategoryID;
        public string CategoryName;
        public TerpeneCategory Category;
        public Color CategoryColor = Color.white;
        public List<string> CommonDescriptors = new List<string>();
        public Sprite CategoryIcon;
        public string Description;
    }
    
    [System.Serializable]
    public class TerpeneInteraction
    {
        public string InteractionID;
        public string PrimaryTerpene;
        public string SecondaryTerpene;
        public TerpeneInteractionType InteractionType;
        public float SynergyStrength;
        public List<string> ResultingEffects = new List<string>();
        public string Description;
    }
    
    [System.Serializable]
    public class TerpeneEffect
    {
        public string EffectName;
        public TerpeneEffectType EffectType;
        public float Intensity;
        public float Duration;
        public string Description;
    }
    
    public enum TerpeneInteractionType
    {
        Synergistic,
        Antagonistic,
        Neutral,
        Masking,
        Enhancing,
        Modulating
    }
    
    public enum TerpeneEffectType
    {
        Aromatic,
        Therapeutic,
        Psychoactive,
        Flavor,
        Preservative,
        Antimicrobial
    }
}