using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Data.Progression
{
    /// <summary>
    /// ScriptableObject database containing all research projects in the game
    /// </summary>
    [CreateAssetMenu(fileName = "ResearchDatabase", menuName = "Project Chimera/Progression/Research Database")]
    public class ResearchDatabaseSO : ScriptableObject
    {
        [Header("Research Projects")]
        [SerializeField] private List<ResearchProjectSO> _researchProjects = new List<ResearchProjectSO>();
        
        [Header("Research Categories")]
        [SerializeField] private List<ResearchCategorySO> _researchCategories = new List<ResearchCategorySO>();
        
        [Header("Database Settings")]
        [SerializeField] private float _globalResearchSpeedMultiplier = 1f;
        [SerializeField] private bool _enableResearchSynergies = true;
        
        /// <summary>
        /// Get all research projects in the database
        /// </summary>
        public List<ResearchProjectSO> GetAllResearchProjects()
        {
            return new List<ResearchProjectSO>(_researchProjects);
        }
        
        /// <summary>
        /// Get research project by ID
        /// </summary>
        public ResearchProjectSO GetResearchProject(string projectId)
        {
            return _researchProjects.FirstOrDefault(project => project.ProjectId == projectId);
        }
        
        /// <summary>
        /// Get research project by ID (compatibility alias)
        /// </summary>
        public ResearchProjectSO GetResearch(string researchId)
        {
            return GetResearchProject(researchId);
        }
        
        /// <summary>
        /// Get research projects by category
        /// </summary>
        public List<ResearchProjectSO> GetResearchByCategory(ResearchCategory category)
        {
            return _researchProjects.Where(project => project.ResearchCategory == category).ToList();
        }
        
        /// <summary>
        /// Get research projects by tier
        /// </summary>
        public List<ResearchProjectSO> GetResearchByTier(ResearchTier tier)
        {
            return _researchProjects.Where(project => project.Tier == tier).ToList();
        }
        
        /// <summary>
        /// Get available research projects (prerequisites met)
        /// </summary>
        public List<ResearchProjectSO> GetAvailableResearch(List<string> completedResearch = null, Dictionary<string, int> playerSkillLevels = null)
        {
            if (completedResearch == null) completedResearch = new List<string>();
            if (playerSkillLevels == null) playerSkillLevels = new Dictionary<string, int>();
            
            var availableResearch = new List<ResearchProjectSO>();
            
            foreach (var project in _researchProjects)
            {
                if (!completedResearch.Contains(project.ProjectId) && 
                    ArePrerequisitesMet(project, completedResearch, playerSkillLevels))
                {
                    availableResearch.Add(project);
                }
            }
            
            return availableResearch;
        }
        
        /// <summary>
        /// Check if research prerequisites are met
        /// </summary>
        public bool ArePrerequisitesMet(ResearchProjectSO project, List<string> completedResearch, Dictionary<string, int> playerSkillLevels)
        {
            // Check research prerequisites
            foreach (var prereqId in project.Prerequisites)
            {
                if (!completedResearch.Contains(prereqId))
                {
                    return false;
                }
            }
            
            // Check skill requirements
            foreach (var skillReq in project.SkillRequirements)
            {
                if (!playerSkillLevels.ContainsKey(skillReq.SkillId) || 
                    playerSkillLevels[skillReq.SkillId] < skillReq.RequiredLevel)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Get research projects that unlock from completing a specific research
        /// </summary>
        public List<ResearchProjectSO> GetUnlockedByResearch(string completedResearchId)
        {
            return _researchProjects.Where(project => 
                project.Prerequisites.Contains(completedResearchId)).ToList();
        }
        
        /// <summary>
        /// Get research category by ID
        /// </summary>
        public ResearchCategorySO GetResearchCategory(ResearchCategory category)
        {
            return _researchCategories.FirstOrDefault(cat => cat.CategoryType == category);
        }
        
        /// <summary>
        /// Get all research categories
        /// </summary>
        public List<ResearchCategorySO> GetAllCategories()
        {
            return new List<ResearchCategorySO>(_researchCategories);
        }
        
        /// <summary>
        /// Get research projects by complexity
        /// </summary>
        public List<ResearchProjectSO> GetResearchByComplexity(ResearchComplexity complexity)
        {
            return _researchProjects.Where(project => project.Complexity == complexity).ToList();
        }
        
        /// <summary>
        /// Get research projects by estimated duration
        /// </summary>
        public List<ResearchProjectSO> GetResearchByDuration(float minHours, float maxHours)
        {
            return _researchProjects.Where(project => 
                project.EstimatedDurationHours >= minHours && 
                project.EstimatedDurationHours <= maxHours).ToList();
        }
        
        /// <summary>
        /// Search research projects by name or description
        /// </summary>
        public List<ResearchProjectSO> SearchResearch(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) return GetAllResearchProjects();
            
            searchTerm = searchTerm.ToLower();
            return _researchProjects.Where(project =>
                project.ProjectName.ToLower().Contains(searchTerm) ||
                project.Description.ToLower().Contains(searchTerm) ||
                project.Keywords.Any(keyword => keyword.ToLower().Contains(searchTerm))
            ).ToList();
        }
        
        /// <summary>
        /// Get recommended research based on player progress
        /// </summary>
        public List<ResearchProjectSO> GetRecommendedResearch(List<string> completedResearch, Dictionary<string, int> playerSkillLevels, int maxRecommendations = 5)
        {
            var availableResearch = GetAvailableResearch(completedResearch, playerSkillLevels);
            
            // Sort by priority and relevance
            return availableResearch
                .OrderByDescending(project => project.Priority)
                .ThenBy(project => project.EstimatedDurationHours)
                .Take(maxRecommendations)
                .ToList();
        }
        
        /// <summary>
        /// Validate database integrity
        /// </summary>
        public bool ValidateDatabase()
        {
            // Check for duplicate project IDs
            var projectIds = _researchProjects.Select(p => p.ProjectId).ToList();
            if (projectIds.Count != projectIds.Distinct().Count())
            {
                Debug.LogError("ResearchDatabase contains duplicate project IDs");
                return false;
            }
            
            // Check for invalid prerequisites
            foreach (var project in _researchProjects)
            {
                foreach (var prereqId in project.Prerequisites)
                {
                    if (GetResearchProject(prereqId) == null)
                    {
                        Debug.LogError($"Research {project.ProjectId} has invalid prerequisite: {prereqId}");
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Get database statistics
        /// </summary>
        public ResearchDatabaseStats GetDatabaseStats()
        {
            return new ResearchDatabaseStats
            {
                TotalProjects = _researchProjects.Count,
                TotalCategories = _researchCategories.Count,
                ProjectsByCategory = System.Enum.GetValues(typeof(ResearchCategory))
                    .Cast<ResearchCategory>()
                    .ToDictionary(cat => cat, cat => _researchProjects.Count(p => p.ResearchCategory == cat)),
                ProjectsByTier = System.Enum.GetValues(typeof(ResearchTier))
                    .Cast<ResearchTier>()
                    .ToDictionary(tier => tier, tier => _researchProjects.Count(p => p.Tier == tier)),
                AverageResearchDuration = _researchProjects.Average(p => p.EstimatedDurationHours)
            };
        }
        
        /// <summary>
        /// Get research tree structure for visualization
        /// </summary>
        public Dictionary<string, List<string>> GetResearchTree()
        {
            var tree = new Dictionary<string, List<string>>();
            
            foreach (var project in _researchProjects)
            {
                tree[project.ProjectId] = new List<string>(project.Prerequisites);
            }
            
            return tree;
        }
    }
    
    /// <summary>
    /// Statistics about the research database
    /// </summary>
    [System.Serializable]
    public class ResearchDatabaseStats
    {
        public int TotalProjects;
        public int TotalCategories;
        public Dictionary<ResearchCategory, int> ProjectsByCategory = new Dictionary<ResearchCategory, int>();
        public Dictionary<ResearchTier, int> ProjectsByTier = new Dictionary<ResearchTier, int>();
        public float AverageResearchDuration;
    }
} 