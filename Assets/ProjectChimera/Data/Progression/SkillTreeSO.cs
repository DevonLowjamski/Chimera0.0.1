using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Progression
{
    /// <summary>
    /// ScriptableObject representing an individual skill tree
    /// </summary>
    [CreateAssetMenu(fileName = "SkillTree", menuName = "Project Chimera/Progression/Skill Tree")]
    public class SkillTreeSO : ChimeraDataSO
    {
        [Header("Tree Identity")]
        [SerializeField] private string _treeId = "";
        [SerializeField] private string _treeName = "";
        [SerializeField] private string _description = "";
        [SerializeField] private SkillDomain _domain = SkillDomain.Cultivation;
        [SerializeField] private SkillCategory _category = SkillCategory.Basic;
        
        [Header("Visual Properties")]
        [SerializeField] private Sprite _treeIcon;
        [SerializeField] private Color _treeColor = Color.white;
        [SerializeField] private Vector2 _treePosition = Vector2.zero;
        
        [Header("Tree Structure")]
        [SerializeField] private List<SkillNodeSO> _skills = new List<SkillNodeSO>();
        [SerializeField] private List<SkillTreeBranch> _branches = new List<SkillTreeBranch>();
        
        [Header("Progression Settings")]
        [SerializeField] private int _unlockLevel = 1;
        [SerializeField] private bool _isVisible = true;
        [SerializeField] private bool _isUnlocked = false;
        [SerializeField] private List<string> _prerequisiteTrees = new List<string>();
        
        [Header("Tree Bonuses")]
        [SerializeField] private float _experienceMultiplier = 1f;
        [SerializeField] private List<PassiveBonus> _treeBonuses = new List<PassiveBonus>();
        
        // Properties
        public string TreeId => _treeId;
        public string TreeName => _treeName;
        public string Description => _description;
        public SkillDomain Domain => _domain;
        public SkillCategory Category => _category;
        public Sprite TreeIcon => _treeIcon;
        public Color TreeColor => _treeColor;
        public Vector2 TreePosition => _treePosition;
        public List<SkillNodeSO> Skills => _skills;
        public List<SkillTreeBranch> Branches => _branches;
        public int UnlockLevel => _unlockLevel;
        public bool IsVisible => _isVisible;
        public bool IsUnlocked => _isUnlocked;
        public List<string> PrerequisiteTrees => _prerequisiteTrees;
        public float ExperienceMultiplier => _experienceMultiplier;
        public List<PassiveBonus> TreeBonuses => _treeBonuses;
        
        /// <summary>
        /// Get all skills in this tree
        /// </summary>
        public List<SkillNodeSO> GetAllSkills()
        {
            return _skills.ToList();
        }
        
        /// <summary>
        /// Get skills by category
        /// </summary>
        public List<SkillNodeSO> GetSkillsByCategory(SkillCategory category)
        {
            return _skills.Where(skill => skill.SkillCategory == category).ToList();
        }
        
        /// <summary>
        /// Get skills by level requirement
        /// </summary>
        public List<SkillNodeSO> GetSkillsByLevel(int level)
        {
            return _skills.Where(skill => skill.UnlockLevel <= level).ToList();
        }
        
        /// <summary>
        /// Check if tree prerequisites are met
        /// </summary>
        public bool ArePrerequisitesMet(List<string> completedTrees)
        {
            return _prerequisiteTrees.All(prereq => completedTrees.Contains(prereq));
        }
        
        /// <summary>
        /// Get tree completion percentage
        /// </summary>
        public float GetCompletionPercentage(Dictionary<string, int> skillLevels)
        {
            if (_skills.Count == 0) return 1f;
            
            int completedSkills = 0;
            foreach (var skill in _skills)
            {
                if (skillLevels.ContainsKey(skill.SkillId) && skillLevels[skill.SkillId] >= skill.MaxLevel)
                {
                    completedSkills++;
                }
            }
            
            return (float)completedSkills / _skills.Count;
        }
        
        /// <summary>
        /// Get total experience required for tree completion
        /// </summary>
        public float GetTotalExperienceRequired()
        {
            return _skills.Sum(skill => skill.GetTotalExperienceRequired());
        }
        
        /// <summary>
        /// Validate tree structure
        /// </summary>
        public bool ValidateTreeStructure()
        {
            // Check for duplicate skill IDs
            var skillIds = _skills.Select(s => s.SkillId).ToList();
            if (skillIds.Count != skillIds.Distinct().Count())
                return false;
            
            // Check for circular dependencies
            foreach (var skill in _skills)
            {
                if (HasCircularDependency(skill, new HashSet<string>()))
                    return false;
            }
            
            return true;
        }
        
        private bool HasCircularDependency(SkillNodeSO skill, HashSet<string> visited)
        {
            if (visited.Contains(skill.SkillId))
                return true;
            
            visited.Add(skill.SkillId);
            
            foreach (var prereqId in skill.PrerequisiteSkillIds)
            {
                var prereqSkill = _skills.FirstOrDefault(s => s.SkillId == prereqId);
                if (prereqSkill != null && HasCircularDependency(prereqSkill, new HashSet<string>(visited)))
                    return true;
            }
            
            return false;
        }
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Auto-generate tree ID if empty
            if (string.IsNullOrEmpty(_treeId))
            {
                _treeId = name.ToLower().Replace(" ", "_");
            }
            
            // Validate tree structure
            if (!ValidateTreeStructure())
            {
                Debug.LogWarning($"Skill tree {_treeName} has structural issues (circular dependencies or duplicate IDs)");
            }
        }
    }
} 