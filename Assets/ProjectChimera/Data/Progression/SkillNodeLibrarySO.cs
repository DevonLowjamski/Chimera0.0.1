using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Data
{
    /// <summary>
    /// Skill Node Library - Library of individual skill nodes and their definitions
    /// Contains detailed skill node data, effects, and interconnections
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Skill Node Library", menuName = "Project Chimera/Progression/Skill Node Library")]
    public class SkillNodeLibrarySO : ChimeraDataSO
    {
        [Header("Skill Node Collections")]
        public List<SkillNodeCollection> NodeCollections = new List<SkillNodeCollection>();
        
        [Header("Individual Nodes")]
        public List<DetailedSkillNode> SkillNodes = new List<DetailedSkillNode>();
        
        [Header("Node Connections")]
        public List<SkillNodeConnection> NodeConnections = new List<SkillNodeConnection>();
        
        public DetailedSkillNode GetSkillNode(string nodeId)
        {
            return SkillNodes.Find(n => n.NodeId == nodeId);
        }
        
        public List<DetailedSkillNode> GetNodesByCategory(SkillCategory category)
        {
            return SkillNodes.FindAll(n => n.Category == category);
        }
        
        public List<DetailedSkillNode> GetConnectedNodes(string nodeId)
        {
            var connections = NodeConnections.FindAll(c => c.FromNodeId == nodeId || c.ToNodeId == nodeId);
            var connectedNodes = new List<DetailedSkillNode>();
            
            foreach (var connection in connections)
            {
                var targetId = connection.FromNodeId == nodeId ? connection.ToNodeId : connection.FromNodeId;
                var node = GetSkillNode(targetId);
                if (node != null) connectedNodes.Add(node);
            }
            
            return connectedNodes;
        }
        
        public bool AreNodesConnected(string nodeA, string nodeB)
        {
            return NodeConnections.Exists(c => 
                (c.FromNodeId == nodeA && c.ToNodeId == nodeB) ||
                (c.FromNodeId == nodeB && c.ToNodeId == nodeA));
        }
    }
    
    [System.Serializable]
    public class SkillNodeCollection
    {
        public string CollectionName;
        public SkillTreeType TreeType;
        public List<string> NodeIds = new List<string>();
        [Range(0.1f, 3.0f)] public float CollectionBonus = 1.0f;
        public string Description;
    }
    
    [System.Serializable]
    public class DetailedSkillNode
    {
        public string NodeId;
        public string NodeName;
        public SkillCategory Category;
        public SkillTier Tier;
        
        [Header("Level Progression")]
        [Range(1, 20)] public int MaxLevel = 10;
        public List<SkillLevelData> LevelData = new List<SkillLevelData>();
        
        [Header("Effects and Bonuses")]
        public List<NodeEffect> Effects = new List<NodeEffect>();
        public List<NodeSynergy> Synergies = new List<NodeSynergy>();
        
        [Header("Requirements")]
        public List<NodeRequirement> Requirements = new List<NodeRequirement>();
        
        [Header("Visual Data")]
        public Vector2 NodePosition;
        public Color NodeColor = Color.white;
        public Sprite NodeIcon;
        public string Description;
        public string FlavorText;
    }
    
    [System.Serializable]
    public class SkillLevelData
    {
        [Range(1, 20)] public int Level;
        [Range(0f, 100000f)] public float ExperienceRequired;
        [Range(0.1f, 10.0f)] public float EffectMultiplier = 1.0f;
        public List<string> UnlockedFeatures = new List<string>();
        public string LevelDescription;
    }
    
    [System.Serializable]
    public class NodeEffect
    {
        public string EffectName;
        public NodeEffectType EffectType;
        [Range(0.01f, 5.0f)] public float BaseValue = 1.0f;
        [Range(0.01f, 1.0f)] public float PerLevelIncrease = 0.1f;
        public string TargetSystem;
        public string Description;
    }
    
    [System.Serializable]
    public class NodeSynergy
    {
        public string SynergyName;
        public List<string> RequiredNodes = new List<string>();
        [Range(0.1f, 5.0f)] public float SynergyBonus = 1.5f;
        public string Description;
    }
    
    [System.Serializable]
    public class NodeRequirement
    {
        public RequirementType RequirementType;
        public string RequirementValue;
        [Range(1, 100)] public int RequiredAmount = 1;
        public string Description;
    }
    
    [System.Serializable]
    public class SkillNodeConnection
    {
        public string FromNodeId;
        public string ToNodeId;
        public ConnectionType ConnectionType;
        public bool IsBidirectional = false;
        [Range(0.1f, 3.0f)] public float ConnectionStrength = 1.0f;
    }
    
    public enum SkillTier
    {
        Basic,
        Intermediate,
        Advanced,
        Expert,
        Master,
        Legendary
    }
    
    public enum NodeEffectType
    {
        PercentageBonus,
        FlatBonus,
        Multiplier,
        UnlockFeature,
        ReduceCost,
        ReduceTime,
        IncreaseCapacity,
        EnableAutomation
    }
    
    public enum RequirementType
    {
        Level,
        Experience,
        Achievement,
        PrerequisiteNode,
        QuestCompletion,
        ResourceAmount,
        SkillMastery,
        CommunityRank
    }
    
    public enum ConnectionType
    {
        Prerequisite,
        Synergy,
        Alternative,
        Enhancement,
        Specialization,
        Advanced,
        Mastery,
        Legacy
    }
}