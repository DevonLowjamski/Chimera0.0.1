using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Collaborative Project Library - Collection of projects for community collaboration system
    /// Contains project definitions, roles, and collaboration structures
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Collaborative Project Library", menuName = "Project Chimera/Gaming/Collaborative Project Library")]
    public class CollaborativeProjectLibrarySO : ChimeraDataSO
    {
        [Header("Project Collection")]
        public List<CollaborativeProject> Projects = new List<CollaborativeProject>();
        
        [Header("Project Categories")]
        public List<ProjectCategoryData> Categories = new List<ProjectCategoryData>();
        
        [Header("Project Templates")]
        public List<ProjectTemplate> Templates = new List<ProjectTemplate>();
        
        #region Runtime Methods
        
        public CollaborativeProject GetProject(string projectID)
        {
            return Projects.Find(p => p.ProjectID == projectID);
        }
        
        public List<CollaborativeProject> GetProjectsByType(ProjectType projectType)
        {
            return Projects.FindAll(p => p.ProjectType == projectType);
        }
        
        public List<CollaborativeProject> GetProjectsByComplexity(ProjectComplexity complexity)
        {
            return Projects.FindAll(p => p.Complexity == complexity);
        }
        
        public ProjectTemplate GetProjectTemplate(ProjectType projectType, ProjectComplexity complexity)
        {
            return Templates.Find(t => t.ProjectType == projectType && t.Complexity == complexity);
        }
        
        #endregion
    }
    
    [System.Serializable]
    public class CollaborativeProject
    {
        public string ProjectID;
        public string ProjectName;
        public ProjectType ProjectType;
        public ProjectComplexity Complexity;
        public int MinParticipants;
        public int MaxParticipants;
        public float EstimatedDuration;
        public List<ProjectObjective> Objectives = new List<ProjectObjective>();
        public List<ProjectRole> RequiredRoles = new List<ProjectRole>();
        public List<ProjectResource> RequiredResources = new List<ProjectResource>();
        public float CompletionReward;
        public string Description;
        public Sprite ProjectIcon;
    }
    
    [System.Serializable]
    public class ProjectCategoryData
    {
        public string CategoryID;
        public string CategoryName;
        public ProjectType ProjectType;
        public Color CategoryColor = Color.white;
        public Sprite CategoryIcon;
        public string Description;
    }
    
    [System.Serializable]
    public class ProjectTemplate
    {
        public string TemplateID;
        public string TemplateName;
        public ProjectType ProjectType;
        public ProjectComplexity Complexity;
        public List<TemplateObjective> DefaultObjectives = new List<TemplateObjective>();
        public List<ProjectRole> RecommendedRoles = new List<ProjectRole>();
        public float EstimatedTimeframe;
        public string Description;
    }
    
    [System.Serializable]
    public class ProjectObjective
    {
        public string ObjectiveID;
        public string ObjectiveName;
        public ProjectObjectiveType ObjectiveType;
        public float TargetValue;
        public float ImportanceWeight;
        public bool IsOptional;
        public List<string> Dependencies = new List<string>();
        public string Description;
    }
    
    [System.Serializable]
    public class ProjectResource
    {
        public string ResourceName;
        public ProjectResourceType ResourceType;
        public float RequiredAmount;
        public bool IsOptional;
        public string Description;
    }
    
    [System.Serializable]
    public class TemplateObjective
    {
        public string ObjectiveName;
        public ProjectObjectiveType ObjectiveType;
        public float DefaultTargetValue;
        public bool IsRequired;
        public string Description;
    }
    
    public enum ProjectResourceType
    {
        Time,
        Expertise,
        Equipment,
        Data,
        Research,
        Documentation,
        Analysis,
        Validation
    }
}