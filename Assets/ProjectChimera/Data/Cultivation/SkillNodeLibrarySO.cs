using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Data.Cultivation
{
    /// <summary>
    /// Skill Node Library - ScriptableObject containing all skill node definitions
    /// Defines individual skill nodes, their requirements, and unlocked capabilities
    /// </summary>
    [CreateAssetMenu(fileName = "SkillNodeLibrary", menuName = "Project Chimera/Cultivation/Skill Node Library")]
    public class SkillNodeLibrarySO : ScriptableObject
    {
        [Header("Skill Node Definitions")]
        [SerializeField] private SkillNodeDefinition[] _skillNodes = new SkillNodeDefinition[0];
        
        [Header("Node Categories")]
        [SerializeField] private NodeCategory[] _nodeCategories = new NodeCategory[]
        {
            new NodeCategory { Type = SkillNodeType.Core, CategoryName = "Core Skills", Description = "Essential cultivation skills" },
            new NodeCategory { Type = SkillNodeType.Advanced, CategoryName = "Advanced Techniques", Description = "Specialized cultivation methods" },
            new NodeCategory { Type = SkillNodeType.Automation, CategoryName = "Automation", Description = "Automated system management" },
            new NodeCategory { Type = SkillNodeType.Science, CategoryName = "Science", Description = "Scientific understanding and analysis" },
            new NodeCategory { Type = SkillNodeType.Business, CategoryName = "Business", Description = "Commercial cultivation skills" },
            new NodeCategory { Type = SkillNodeType.Mastery, CategoryName = "Mastery", Description = "Expert-level capabilities" }
        };
        
        [Header("Progression Settings")]
        [Range(10f, 1000f)] public float BaseExperienceRequired = 100f;
        [Range(1.1f, 3f)] public float ExperienceScalingFactor = 1.5f;
        [Range(0.1f, 5f)] public float SkillBonusMultiplier = 1.2f;
        
        // Public Properties
        public SkillNodeDefinition[] AllNodes => _skillNodes;
        public NodeCategory[] NodeCategories => _nodeCategories;
        
        /// <summary>
        /// Get skill node by ID
        /// </summary>
        public SkillNodeDefinition GetSkillNode(string nodeId)
        {
            return _skillNodes.FirstOrDefault(node => node.NodeId == nodeId);
        }
        
        /// <summary>
        /// Get all skill nodes for a specific branch
        /// </summary>
        public SkillNodeDefinition[] GetNodesForBranch(SkillBranch branch)
        {
            return _skillNodes.Where(node => node.Branch == branch).ToArray();
        }
        
        /// <summary>
        /// Get all skill nodes of a specific type
        /// </summary>
        public SkillNodeDefinition[] GetNodesByType(SkillNodeType nodeType)
        {
            return _skillNodes.Where(node => node.NodeType == nodeType).ToArray();
        }
        
        /// <summary>
        /// Get root nodes (nodes with no prerequisites)
        /// </summary>
        public SkillNodeDefinition[] GetRootNodes()
        {
            return _skillNodes.Where(node => node.Prerequisites == null || node.Prerequisites.Length == 0).ToArray();
        }
        
        /// <summary>
        /// Get nodes that can be unlocked with current progression
        /// </summary>
        public SkillNodeDefinition[] GetAvailableNodes(List<string> unlockedNodeIds)
        {
            return _skillNodes.Where(node => 
                !unlockedNodeIds.Contains(node.NodeId) && 
                ArePrerequisitesMet(node, unlockedNodeIds)).ToArray();
        }
        
        /// <summary>
        /// Check if prerequisites are met for a node
        /// </summary>
        public bool ArePrerequisitesMet(SkillNodeDefinition node, List<string> unlockedNodeIds)
        {
            if (node.Prerequisites == null || node.Prerequisites.Length == 0)
                return true;
            
            foreach (var prerequisite in node.Prerequisites)
            {
                if (!unlockedNodeIds.Contains(prerequisite))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Get node category information
        /// </summary>
        public NodeCategory GetNodeCategory(SkillNodeType nodeType)
        {
            return _nodeCategories.FirstOrDefault(cat => cat.Type == nodeType);
        }
        
        /// <summary>
        /// Calculate experience required for node
        /// </summary>
        public float CalculateRequiredExperience(SkillNodeDefinition node)
        {
            var tierMultiplier = GetTierMultiplier(node.NodeType);
            var branchMultiplier = GetBranchMultiplier(node.Branch);
            
            return BaseExperienceRequired * tierMultiplier * branchMultiplier;
        }
        
        private float GetTierMultiplier(SkillNodeType nodeType)
        {
            return nodeType switch
            {
                SkillNodeType.Core => 1f,
                SkillNodeType.Advanced => 1.5f,
                SkillNodeType.Automation => 2f,
                SkillNodeType.Science => 2.5f,
                SkillNodeType.Business => 1.8f,
                SkillNodeType.Mastery => 3f,
                _ => 1f
            };
        }
        
        private float GetBranchMultiplier(SkillBranch branch)
        {
            return branch switch
            {
                SkillBranch.Cultivation => 1f,
                SkillBranch.Automation => 1.3f,
                SkillBranch.Science => 1.5f,
                SkillBranch.Business => 1.2f,
                SkillBranch.Genetics => 1.8f,
                SkillBranch.Processing => 1.1f,
                _ => 1f
            };
        }
        
        /// <summary>
        /// Create default skill node library
        /// </summary>
        [ContextMenu("Create Default Nodes")]
        public void CreateDefaultNodes()
        {
            var defaultNodes = new List<SkillNodeDefinition>();
            
            // Core Cultivation Nodes
            defaultNodes.AddRange(CreateCultivationNodes());
            
            // Automation Nodes
            defaultNodes.AddRange(CreateAutomationNodes());
            
            // Science Nodes
            defaultNodes.AddRange(CreateScienceNodes());
            
            // Business Nodes
            defaultNodes.AddRange(CreateBusinessNodes());
            
            // Genetics Nodes
            defaultNodes.AddRange(CreateGeneticsNodes());
            
            // Processing Nodes
            defaultNodes.AddRange(CreateProcessingNodes());
            
            _skillNodes = defaultNodes.ToArray();
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
        
        private SkillNodeDefinition[] CreateCultivationNodes()
        {
            return new SkillNodeDefinition[]
            {
                new SkillNodeDefinition
                {
                    NodeId = "cultivation_basic_watering",
                    NodeName = "Basic Watering",
                    Description = "Learn fundamental watering techniques",
                    Branch = SkillBranch.Cultivation,
                    NodeType = SkillNodeType.Core,
                    RequiredExperience = 50f,
                    UnlockedCapabilities = new string[] { "Manual Watering", "Water Quality Assessment" }
                },
                new SkillNodeDefinition
                {
                    NodeId = "cultivation_plant_training",
                    NodeName = "Plant Training",
                    Description = "Master LST and other training techniques",
                    Branch = SkillBranch.Cultivation,
                    NodeType = SkillNodeType.Advanced,
                    RequiredExperience = 150f,
                    Prerequisites = new string[] { "cultivation_basic_watering" },
                    UnlockedCapabilities = new string[] { "LST", "Topping", "Defoliation" }
                },
                new SkillNodeDefinition
                {
                    NodeId = "cultivation_pest_management",
                    NodeName = "Pest Management",
                    Description = "Identify and treat common pests",
                    Branch = SkillBranch.Cultivation,
                    NodeType = SkillNodeType.Advanced,
                    RequiredExperience = 200f,
                    Prerequisites = new string[] { "cultivation_basic_watering" },
                    UnlockedCapabilities = new string[] { "IPM", "Organic Treatments", "Beneficial Insects" }
                }
            };
        }
        
        private SkillNodeDefinition[] CreateAutomationNodes()
        {
            return new SkillNodeDefinition[]
            {
                new SkillNodeDefinition
                {
                    NodeId = "automation_basic_watering",
                    NodeName = "Automated Watering",
                    Description = "Set up basic irrigation automation",
                    Branch = SkillBranch.Automation,
                    NodeType = SkillNodeType.Automation,
                    RequiredExperience = 300f,
                    Prerequisites = new string[] { "cultivation_basic_watering" },
                    UnlockedCapabilities = new string[] { "Timer-based Watering", "Moisture Sensors" }
                },
                new SkillNodeDefinition
                {
                    NodeId = "automation_environmental_control",
                    NodeName = "Environmental Control",
                    Description = "Automate temperature and humidity",
                    Branch = SkillBranch.Automation,
                    NodeType = SkillNodeType.Automation,
                    RequiredExperience = 500f,
                    Prerequisites = new string[] { "automation_basic_watering" },
                    UnlockedCapabilities = new string[] { "Climate Control", "HVAC Integration", "Smart Sensors" }
                }
            };
        }
        
        private SkillNodeDefinition[] CreateScienceNodes()
        {
            return new SkillNodeDefinition[]
            {
                new SkillNodeDefinition
                {
                    NodeId = "science_plant_biology",
                    NodeName = "Plant Biology",
                    Description = "Understand plant physiology and growth",
                    Branch = SkillBranch.Science,
                    NodeType = SkillNodeType.Science,
                    RequiredExperience = 250f,
                    UnlockedCapabilities = new string[] { "Growth Analysis", "Nutrient Understanding", "Stress Identification" }
                },
                new SkillNodeDefinition
                {
                    NodeId = "science_data_analysis",
                    NodeName = "Data Analysis",
                    Description = "Analyze cultivation data for optimization",
                    Branch = SkillBranch.Science,
                    NodeType = SkillNodeType.Science,
                    RequiredExperience = 400f,
                    Prerequisites = new string[] { "science_plant_biology" },
                    UnlockedCapabilities = new string[] { "Statistical Analysis", "Trend Identification", "Predictive Modeling" }
                }
            };
        }
        
        private SkillNodeDefinition[] CreateBusinessNodes()
        {
            return new SkillNodeDefinition[]
            {
                new SkillNodeDefinition
                {
                    NodeId = "business_cost_management",
                    NodeName = "Cost Management",
                    Description = "Track and optimize operational costs",
                    Branch = SkillBranch.Business,
                    NodeType = SkillNodeType.Business,
                    RequiredExperience = 200f,
                    UnlockedCapabilities = new string[] { "Cost Tracking", "ROI Analysis", "Budget Planning" }
                }
            };
        }
        
        private SkillNodeDefinition[] CreateGeneticsNodes()
        {
            return new SkillNodeDefinition[]
            {
                new SkillNodeDefinition
                {
                    NodeId = "genetics_strain_selection",
                    NodeName = "Strain Selection",
                    Description = "Choose optimal strains for conditions",
                    Branch = SkillBranch.Genetics,
                    NodeType = SkillNodeType.Advanced,
                    RequiredExperience = 350f,
                    UnlockedCapabilities = new string[] { "Phenotype Analysis", "Strain Comparison", "Genetic Markers" }
                }
            };
        }
        
        private SkillNodeDefinition[] CreateProcessingNodes()
        {
            return new SkillNodeDefinition[]
            {
                new SkillNodeDefinition
                {
                    NodeId = "processing_harvest_timing",
                    NodeName = "Harvest Timing",
                    Description = "Determine optimal harvest windows",
                    Branch = SkillBranch.Processing,
                    NodeType = SkillNodeType.Advanced,
                    RequiredExperience = 300f,
                    UnlockedCapabilities = new string[] { "Trichome Analysis", "Harvest Optimization", "Quality Assessment" }
                }
            };
        }
        
        private void OnValidate()
        {
            ValidateSkillNodes();
        }
        
        private void ValidateSkillNodes()
        {
            for (int i = 0; i < _skillNodes.Length; i++)
            {
                var node = _skillNodes[i];
                
                // Ensure node has valid ID
                if (string.IsNullOrEmpty(node.NodeId))
                    node.NodeId = $"node_{i:D3}";
                
                // Ensure node has name
                if (string.IsNullOrEmpty(node.NodeName))
                    node.NodeName = $"Skill Node {i + 1}";
                
                // Validate experience requirements
                node.RequiredExperience = Mathf.Max(10f, node.RequiredExperience);
                
                // Validate prerequisites exist
                if (node.Prerequisites != null)
                {
                    for (int j = 0; j < node.Prerequisites.Length; j++)
                    {
                        var prerequisite = node.Prerequisites[j];
                        if (!_skillNodes.Any(n => n.NodeId == prerequisite))
                        {
                            Debug.LogWarning($"Node '{node.NodeName}' has invalid prerequisite: {prerequisite}");
                        }
                    }
                }
            }
        }
    }
    
    [System.Serializable]
    public class SkillNodeDefinition
    {
        [Header("Node Identity")]
        public string NodeId;
        public string NodeName;
        [TextArea(2, 4)] public string Description;
        
        [Header("Classification")]
        public SkillBranch Branch = SkillBranch.Cultivation;
        public SkillNodeType NodeType = SkillNodeType.Core;
        [Range(1, 10)] public int NodeTier = 1;
        
        [Header("Requirements")]
        [Range(10f, 10000f)] public float RequiredExperience = 100f;
        public string[] Prerequisites = new string[0];
        [Range(1, 100)] public int MinimumPlayerLevel = 1;
        
        [Header("Unlocks")]
        public string[] UnlockedCapabilities = new string[0];
        public string[] UnlockedEquipment = new string[0];
        public string[] UnlockedRecipes = new string[0];
        
        [Header("Bonuses")]
        [Range(0f, 2f)] public float SkillEfficiencyBonus = 0f;
        [Range(0f, 1f)] public float CostReductionBonus = 0f;
        [Range(0f, 2f)] public float ExperienceGainBonus = 0f;
        
        [Header("Visual")]
        public Sprite NodeIcon;
        public Color NodeColor = Color.white;
        public GameObject NodePrefab;
        
        [Header("Audio")]
        public AudioClip UnlockSound;
        public AudioClip ActivationSound;
    }
    
    [System.Serializable]
    public class NodeCategory
    {
        public SkillNodeType Type;
        public string CategoryName;
        [TextArea(1, 3)] public string Description;
        public Color CategoryColor = Color.white;
        public Sprite CategoryIcon;
    }
    
    // Note: SkillNodeType is defined in SkillTreeDataStructures.cs
}