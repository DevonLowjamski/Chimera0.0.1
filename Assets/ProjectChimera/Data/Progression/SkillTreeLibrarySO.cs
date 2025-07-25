using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Data.Progression
{
    /// <summary>
    /// ScriptableObject library containing all skill trees in the game
    /// </summary>
    [CreateAssetMenu(fileName = "SkillTreeLibrary", menuName = "Project Chimera/Progression/Skill Tree Library")]
    public class SkillTreeLibrarySO : ScriptableObject
    {
        [Header("Skill Trees")]
        [SerializeField] private List<SkillTreeSO> _skillTrees = new List<SkillTreeSO>();
        
        [Header("Library Settings")]
        [SerializeField] private bool _enableSkillSynergies = true;
        [SerializeField] private float _globalSkillMultiplier = 1f;
        
        /// <summary>
        /// Get all skill trees in the library
        /// </summary>
        public List<SkillTreeSO> GetAllSkillTrees()
        {
            return new List<SkillTreeSO>(_skillTrees);
        }
        
        /// <summary>
        /// Get skill tree by ID
        /// </summary>
        public SkillTreeSO GetSkillTree(string treeId)
        {
            return _skillTrees.FirstOrDefault(tree => tree.TreeId == treeId);
        }
        
        /// <summary>
        /// Get skill trees by category
        /// </summary>
        public List<SkillTreeSO> GetSkillTreesByCategory(SkillCategory category)
        {
            return _skillTrees.Where(tree => tree.Category == category).ToList();
        }
        
        /// <summary>
        /// Get skill trees by domain
        /// </summary>
        public List<SkillTreeSO> GetSkillTreesByDomain(SkillDomain domain)
        {
            return _skillTrees.Where(tree => tree.Domain == domain).ToList();
        }
        
        /// <summary>
        /// Get all skills from all trees
        /// </summary>
        public List<SkillNodeSO> GetAllSkills()
        {
            var allSkills = new List<SkillNodeSO>();
            foreach (var tree in _skillTrees)
            {
                allSkills.AddRange(tree.Skills);
            }
            return allSkills;
        }
        
        /// <summary>
        /// Get skill by ID from any tree
        /// </summary>
        public SkillNodeSO GetSkill(string skillId)
        {
            foreach (var tree in _skillTrees)
            {
                var skill = tree.Skills.FirstOrDefault(s => s.SkillId == skillId);
                if (skill != null) return skill;
            }
            return null;
        }
        
        /// <summary>
        /// Get skills by category from all trees
        /// </summary>
        public List<SkillNodeSO> GetSkillsByCategory(SkillCategory category)
        {
            var skills = new List<SkillNodeSO>();
            foreach (var tree in _skillTrees)
            {
                if (tree.Category == category)
                {
                    skills.AddRange(tree.Skills);
                }
            }
            return skills;
        }
        
        /// <summary>
        /// Get prerequisite skills for a given skill
        /// </summary>
        public List<SkillNodeSO> GetPrerequisiteSkills(string skillId)
        {
            var skill = GetSkill(skillId);
            if (skill == null) return new List<SkillNodeSO>();
            
            var prerequisites = new List<SkillNodeSO>();
            foreach (var prereqId in skill.PrerequisiteSkillIds)
            {
                var prereqSkill = GetSkill(prereqId);
                if (prereqSkill != null)
                {
                    prerequisites.Add(prereqSkill);
                }
            }
            return prerequisites;
        }
        
        /// <summary>
        /// Check if skill prerequisites are met
        /// </summary>
        public bool ArePrerequisitesMet(string skillId, Dictionary<string, int> playerSkillLevels)
        {
            var prerequisites = GetPrerequisiteSkills(skillId);
            foreach (var prereq in prerequisites)
            {
                if (!playerSkillLevels.ContainsKey(prereq.SkillId) || 
                    playerSkillLevels[prereq.SkillId] < prereq.RequiredLevel)
                {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// Get available skills for player (prerequisites met)
        /// </summary>
        public List<SkillNodeSO> GetAvailableSkills(Dictionary<string, int> playerSkillLevels, List<string> unlockedSkills)
        {
            var availableSkills = new List<SkillNodeSO>();
            var allSkills = GetAllSkills();
            
            foreach (var skill in allSkills)
            {
                if (!unlockedSkills.Contains(skill.SkillId) && 
                    ArePrerequisitesMet(skill.SkillId, playerSkillLevels))
                {
                    availableSkills.Add(skill);
                }
            }
            
            return availableSkills;
        }
        
        /// <summary>
        /// Validate library integrity
        /// </summary>
        public bool ValidateLibrary()
        {
            // Check for duplicate skill IDs
            var allSkills = GetAllSkills();
            var skillIds = allSkills.Select(s => s.SkillId).ToList();
            if (skillIds.Count != skillIds.Distinct().Count())
            {
                Debug.LogError("SkillTreeLibrary contains duplicate skill IDs");
                return false;
            }
            
            // Check for invalid prerequisites
            foreach (var skill in allSkills)
            {
                foreach (var prereqId in skill.PrerequisiteSkillIds)
                {
                    if (GetSkill(prereqId) == null)
                    {
                        Debug.LogError($"Skill {skill.SkillId} has invalid prerequisite: {prereqId}");
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Get library statistics
        /// </summary>
        public SkillLibraryStats GetLibraryStats()
        {
            var allSkills = GetAllSkills();
            return new SkillLibraryStats
            {
                TotalTrees = _skillTrees.Count,
                TotalSkills = allSkills.Count,
                SkillsByCategory = System.Enum.GetValues(typeof(SkillCategory))
                    .Cast<SkillCategory>()
                    .ToDictionary(cat => cat, cat => allSkills.Count(s => s.SkillCategory == cat)),
                SkillsByDomain = System.Enum.GetValues(typeof(SkillDomain))
                    .Cast<SkillDomain>()
                    .ToDictionary(dom => dom, dom => _skillTrees.Count(t => t.Domain == dom))
            };
        }
    }
    
    /// <summary>
    /// Statistics about the skill library
    /// </summary>
    [System.Serializable]
    public class SkillLibraryStats
    {
        public int TotalTrees;
        public int TotalSkills;
        public Dictionary<SkillCategory, int> SkillsByCategory = new Dictionary<SkillCategory, int>();
        public Dictionary<SkillDomain, int> SkillsByDomain = new Dictionary<SkillDomain, int>();
    }
} 