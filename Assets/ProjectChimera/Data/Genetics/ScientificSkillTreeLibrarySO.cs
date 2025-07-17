using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Scientific Skill Tree Library - Collection of skill trees for Enhanced Scientific Gaming System v2.0
    /// Contains all skill tree definitions for genetics, aromatics, competition, and community systems
    /// </summary>
    [CreateAssetMenu(fileName = "New Scientific Skill Tree Library", menuName = "Project Chimera/Gaming/Scientific Skill Tree Library")]
    public class ScientificSkillTreeLibrarySO : ChimeraDataSO
    {
        [Header("Skill Tree Collection")]
        public List<SkillTreeData> SkillTrees = new List<SkillTreeData>();
        
        [Header("Skill Tree Categories")]
        public List<SkillTreeCategoryData> Categories = new List<SkillTreeCategoryData>();
        
        [Header("Cross-Tree Dependencies")]
        public List<SkillTreeDependency> Dependencies = new List<SkillTreeDependency>();
        
        #region Runtime Methods
        
        public SkillTreeData GetSkillTree(SkillCategory category)
        {
            return SkillTrees.Find(st => st.Category == category);
        }
        
        public List<SkillTreeData> GetSkillTreesByCategory(SkillTreeCategoryData category)
        {
            return SkillTrees.FindAll(st => st.CategoryID == category.CategoryID);
        }
        
        public bool HasDependency(string skillTreeId, string dependencyId)
        {
            return Dependencies.Exists(d => d.SkillTreeID == skillTreeId && d.DependencySkillTreeID == dependencyId);
        }
        
        #endregion
    }
    
    [System.Serializable]
    public class SkillTreeData
    {
        public string SkillTreeID;
        public string SkillTreeName;
        public SkillCategory Category;
        public string CategoryID;
        public List<SkillNodeData> SkillNodes = new List<SkillNodeData>();
        public Sprite SkillTreeIcon;
        public string Description;
    }
    
    [System.Serializable]
    public class SkillNodeData
    {
        public string NodeID;
        public string NodeName;
        public List<string> Prerequisites = new List<string>();
        public float UnlockCost;
        public SkillNodeType NodeType;
        public string Description;
        public Sprite NodeIcon;
    }
    
    [System.Serializable]
    public class SkillTreeCategoryData
    {
        public string CategoryID;
        public string CategoryName;
        public Color CategoryColor = Color.white;
        public Sprite CategoryIcon;
        public string Description;
    }
    
    [System.Serializable]
    public class SkillTreeDependency
    {
        public string SkillTreeID;
        public string DependencySkillTreeID;
        public DependencyType DependencyType;
        public float RequiredProgress;
    }
    
    public enum SkillNodeType
    {
        Basic,
        Intermediate,
        Advanced,
        Master,
        Legendary
    }
    
    public enum DependencyType
    {
        Prerequisite,
        Enhancement,
        Synergy,
        Unlock
    }
}