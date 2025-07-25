using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Progression;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Skill Tree Configuration - Defines skill trees, nodes, and progression paths
    /// for scientific gaming systems
    /// </summary>
    [CreateAssetMenu(fileName = "New Skill Tree Config", menuName = "Project Chimera/Genetics/Skill Tree Config")]
    public class SkillTreeConfigSO : ChimeraConfigSO
    {
        [Header("Skill Tree Settings")]
        [Range(1, 10)] public int MaxSkillTrees = 5;
        [Range(1, 100)] public int MaxNodesPerTree = 50;
        [Range(0.1f, 5.0f)] public float SkillPointMultiplier = 1.0f;
        [Range(0.1f, 3.0f)] public float ExperienceRequirementMultiplier = 1.0f;
        
        [Header("Skill Node Configuration")]
        public List<SkillNodeTemplate> SkillNodeTemplates = new List<SkillNodeTemplate>();
        public List<SkillCategoryDefinition> SkillCategories = new List<SkillCategoryDefinition>();
        public List<SkillPrerequisite> Prerequisites = new List<SkillPrerequisite>();
        
        [Header("Progression Settings")]
        [Range(1f, 100f)] public float BaseSkillPointCost = 1f;
        [Range(0.1f, 2.0f)] public float CostProgressionRate = 1.2f;
        public bool EnableSkillRefund = true;
        [Range(0f, 1f)] public float RefundEfficiency = 0.8f;
        
        [Header("Visual Configuration")]
        public Color LockedNodeColor = Color.gray;
        public Color AvailableNodeColor = Color.yellow;
        public Color UnlockedNodeColor = Color.green;
        public Color MasteredNodeColor = Color.blue;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (SkillNodeTemplates.Count == 0)
            {
                Debug.LogWarning("No skill node templates defined", this);
            }
            
            if (SkillCategories.Count == 0)
            {
                Debug.LogWarning("No skill categories defined", this);
            }
        }
    }
    
    [System.Serializable]
    public class SkillNodeTemplate
    {
        public string NodeId;
        public string NodeName;
        public string Description;
        public SkillCategory Category;
        [Range(1f, 100f)] public float SkillPointCost = 1f;
        public List<string> PrerequisiteNodes = new List<string>();
        public List<SkillEffect> Effects = new List<SkillEffect>();
        public Sprite NodeIcon;
    }
    
    [System.Serializable]
    public class SkillCategoryDefinition
    {
        public string CategoryName;
        public SkillCategory Category;
        public Color CategoryColor = Color.white;
        public Sprite CategoryIcon;
        public string Description;
    }
    
    [System.Serializable]
    public class SkillPrerequisite
    {
        public string SkillNodeId;
        public List<string> RequiredNodes = new List<string>();
        public int MinimumLevel = 1;
        public float MinimumExperience = 0f;
    }
    
}