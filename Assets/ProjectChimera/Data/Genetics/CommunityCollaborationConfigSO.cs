using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics.Scientific;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Community Collaboration Configuration - Configuration for social features and mentorship systems
    /// Defines community interaction parameters, collaboration mechanics, and social recognition systems
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Community Collaboration Config", menuName = "Project Chimera/Gaming/Community Collaboration Config")]
    public class CommunityCollaborationConfigSO : ChimeraConfigSO
    {
        [Header("Community Settings")]
        [Range(0.1f, 3.0f)] public float CommunityContributionRewardMultiplier = 1.5f;
        [Range(0.1f, 5.0f)] public float MentorshipReputationBonus = 2.0f;
        [Range(1.0f, 10.0f)] public float CollaborativeProjectThreshold = 4.0f;
        [Range(0.1f, 1.0f)] public float PeerEndorsementWeight = 0.8f;
        
        [Header("Mentorship System")]
        public List<MentorshipProgramConfig> MentorshipPrograms = new List<MentorshipProgramConfig>();
        public List<MentorQualificationCriteria> MentorQualifications = new List<MentorQualificationCriteria>();
        public List<MentorshipRewardConfig> MentorshipRewards = new List<MentorshipRewardConfig>();
        
        [Header("Collaborative Projects")]
        public List<CollaborativeProjectTemplate> ProjectTemplates = new List<CollaborativeProjectTemplate>();
        public List<ProjectRoleDefinition> ProjectRoles = new List<ProjectRoleDefinition>();
        [Range(2, 20)] public int MaxProjectParticipants = 10;
        [Range(1, 365)] public int DefaultProjectDuration = 30;
        
        [Header("Social Recognition")]
        public List<SocialAchievementConfig> SocialAchievements = new List<SocialAchievementConfig>();
        public List<CommunityRankConfig> CommunityRanks = new List<CommunityRankConfig>();
        public List<EndorsementTypeConfig> EndorsementTypes = new List<EndorsementTypeConfig>();
        
        [Header("Knowledge Sharing")]
        public List<KnowledgeContributionType> ContributionTypes = new List<KnowledgeContributionType>();
        [Range(0.1f, 5.0f)] public float InnovationSharingBonus = 2.5f;
        public bool EnableKnowledgeDocumentation = true;
        public bool EnableCommunityValidation = true;
        
        [Header("Community Events")]
        public List<CommunityEventConfig> CommunityEvents = new List<CommunityEventConfig>();
        public List<SeasonalCommunityActivity> SeasonalActivities = new List<SeasonalCommunityActivity>();
        public bool EnableGlobalCommunityEvents = true;
        
        [Header("Reputation System")]
        [Range(0.01f, 1.0f)] public float ReputationDecayRate = 0.05f;
        [Range(0.1f, 5.0f)] public float CommunityContributionMultiplier = 1.8f;
        [Range(1, 365)] public int ReputationCalculationPeriod = 30;
        public bool EnableReputationTiers = true;
        
        [Header("Innovation Showcase")]
        public List<InnovationShowcaseConfig> ShowcaseConfigurations = new List<InnovationShowcaseConfig>();
        [Range(0.1f, 2.0f)] public float ShowcaseVisibilityBonus = 1.3f;
        public bool EnablePeerReviews = true;
        public bool EnableInnovationVoting = true;
        
        [Header("Expert Consultation")]
        public List<ExpertiseAreaConfig> ExpertiseAreas = new List<ExpertiseAreaConfig>();
        public List<ConsultationTypeConfig> ConsultationTypes = new List<ConsultationTypeConfig>();
        [Range(1, 100)] public int MaxConsultationsPerExpert = 20;
        
        [Header("Performance Settings")]
        [Range(1, 100)] public int MaxConcurrentMentorships = 25;
        [Range(1, 50)] public int MaxConcurrentProjects = 15;
        [Range(0.1f, 5.0f)] public float CommunityProcessingOptimization = 1.0f;
        
        #region Validation
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            ValidateMentorshipPrograms();
            ValidateProjectTemplates();
            ValidateCommunityRanks();
            ValidateReputationSettings();
        }
        
        private void ValidateMentorshipPrograms()
        {
            if (MentorshipPrograms.Count == 0)
            {
                Debug.LogWarning("No mentorship programs defined", this);
            }
            
            foreach (var program in MentorshipPrograms)
            {
                if (program.MaxMentees <= 0)
                {
                    Debug.LogWarning($"Mentorship program {program.ProgramName} has invalid max mentees", this);
                }
            }
        }
        
        private void ValidateProjectTemplates()
        {
            foreach (var template in ProjectTemplates)
            {
                if (template.MinParticipants > template.MaxParticipants)
                {
                    Debug.LogError($"Project template {template.ProjectName} has invalid participant range", this);
                }
            }
        }
        
        private void ValidateCommunityRanks()
        {
            if (CommunityRanks.Count == 0)
            {
                Debug.LogWarning("No community ranks defined", this);
                return;
            }
            
            float lastThreshold = 0f;
            foreach (var rank in CommunityRanks)
            {
                if (rank.ReputationThreshold <= lastThreshold)
                {
                    Debug.LogWarning($"Community rank {rank.RankName} has invalid threshold ordering", this);
                }
                lastThreshold = rank.ReputationThreshold;
            }
        }
        
        private void ValidateReputationSettings()
        {
            if (ReputationDecayRate > 0.5f)
            {
                Debug.LogWarning("Reputation decay rate is very high - reputation may decay too quickly", this);
            }
        }
        
        #endregion
        
        #region Runtime Methods
        
        public MentorshipProgramConfig GetMentorshipProgram(MentorshipProgramType programType)
        {
            return MentorshipPrograms.Find(p => p.ProgramType == programType);
        }
        
        public bool QualifiesAsMentor(CommunityMemberProfile profile, ExpertiseArea area)
        {
            var qualification = MentorQualifications.Find(q => q.ExpertiseArea == area);
            if (qualification == null) return false;
            
            return EvaluateMentorQualification(qualification, profile);
        }
        
        public CollaborativeProjectTemplate GetProjectTemplate(ProjectType projectType, ProjectComplexity complexity)
        {
            return ProjectTemplates.Find(t => t.ProjectType == projectType && t.Complexity == complexity);
        }
        
        public CommunityRankConfig GetCommunityRank(float currentReputation)
        {
            CommunityRankConfig currentRank = null;
            foreach (var rank in CommunityRanks)
            {
                if (currentReputation >= rank.ReputationThreshold)
                {
                    currentRank = rank;
                }
                else
                {
                    break;
                }
            }
            return currentRank;
        }
        
        public List<ProjectRoleDefinition> GetAvailableRoles(ProjectType projectType)
        {
            return ProjectRoles.FindAll(r => r.ApplicableProjectTypes.Contains(projectType));
        }
        
        public float CalculateContributionValue(KnowledgeContributionType contributionType, ContributionQuality quality)
        {
            var baseValue = GetContributionBaseValue(contributionType);
            var qualityMultiplier = GetQualityMultiplier(quality);
            
            return baseValue * qualityMultiplier * CommunityContributionMultiplier;
        }
        
        public List<CommunityEventConfig> GetActiveCommunityEvents()
        {
            return CommunityEvents.FindAll(e => e.IsActive && IsEventCurrentlyRunning(e));
        }
        
        public InnovationShowcaseConfig GetShowcaseConfig(InnovationShowcaseType showcaseType)
        {
            return ShowcaseConfigurations.Find(s => s.ShowcaseType == showcaseType);
        }
        
        public List<ExpertiseAreaConfig> GetExpertiseAreasForMember(CommunityMemberProfile profile)
        {
            return ExpertiseAreas.FindAll(area => profile.ExpertiseLevel >= area.MinimumExpertiseLevel);
        }
        
        #endregion
        
        #region Helper Methods
        
        private bool EvaluateMentorQualification(MentorQualificationCriteria qualification, CommunityMemberProfile profile)
        {
            if (profile.CommunityReputation < qualification.MinimumReputation) return false;
            if (profile.ExpertiseLevel < qualification.MinimumExpertiseLevel) return false;
            if (profile.CommunityContributions < qualification.MinimumContributions) return false;
            
            foreach (var requiredAchievement in qualification.RequiredAchievements)
            {
                if (!profile.UnlockedAchievements.Contains(requiredAchievement))
                    return false;
            }
            
            return true;
        }
        
        private float GetContributionBaseValue(KnowledgeContributionType contributionType)
        {
            var contribution = ContributionTypes.Find(c => c.ContributionType == contributionType.ContributionType);
            return contribution?.BaseValue ?? 1.0f;
        }
        
        private float GetQualityMultiplier(ContributionQuality quality)
        {
            return quality switch
            {
                ContributionQuality.Basic => 1.0f,
                ContributionQuality.Good => 1.3f,
                ContributionQuality.Excellent => 1.7f,
                ContributionQuality.Outstanding => 2.2f,
                ContributionQuality.Groundbreaking => 3.0f,
                _ => 1.0f
            };
        }
        
        private bool IsEventCurrentlyRunning(CommunityEventConfig eventConfig)
        {
            // Simplified event timing check - in real implementation would check actual dates
            return eventConfig.IsRecurring || eventConfig.IsOneTime;
        }
        
        #endregion
    }
    
    #region Data Structures
    
    [System.Serializable]
    public class MentorshipProgramConfig
    {
        public string ProgramName;
        public MentorshipProgramType ProgramType;
        [Range(1, 20)] public int MaxMentees = 5;
        [Range(1, 365)] public int ProgramDuration = 90;
        [Range(0.1f, 5.0f)] public float ReputationReward = 2.0f;
        public List<ExpertiseArea> FocusAreas = new List<ExpertiseArea>();
        public List<MentorshipActivity> Activities = new List<MentorshipActivity>();
        public string Description;
    }
    
    [System.Serializable]
    public class MentorQualificationCriteria
    {
        public ExpertiseArea ExpertiseArea;
        [Range(0f, 1000f)] public float MinimumReputation = 50f;
        [Range(1, 50)] public int MinimumExpertiseLevel = 10;
        [Range(0, 100)] public int MinimumContributions = 5;
        public List<string> RequiredAchievements = new List<string>();
        public bool RequiresCommunityEndorsement = true;
    }
    
    [System.Serializable]
    public class MentorshipRewardConfig
    {
        public string RewardName;
        public MentorshipMilestone MilestoneType;
        [Range(0.1f, 10.0f)] public float ReputationReward = 1.0f;
        public List<string> UnlockedFeatures = new List<string>();
        public bool IsRecurringReward = false;
    }
    
    [System.Serializable]
    public class CollaborativeProjectTemplate
    {
        public string ProjectName;
        public ProjectType ProjectType;
        public ProjectComplexity Complexity;
        [Range(2, 20)] public int MinParticipants = 3;
        [Range(2, 20)] public int MaxParticipants = 8;
        [Range(1, 365)] public int EstimatedDuration = 30;
        [Range(0.1f, 10.0f)] public float CompletionReward = 3.0f;
        public List<ProjectObjective> Objectives = new List<ProjectObjective>();
        public List<ProjectRole> RequiredRoles = new List<ProjectRole>();
        public string Description;
    }
    
    [System.Serializable]
    public class ProjectRoleDefinition
    {
        public string RoleName;
        public ProjectRole Role;
        public List<ProjectType> ApplicableProjectTypes = new List<ProjectType>();
        [Range(0.1f, 3.0f)] public float ContributionWeight = 1.0f;
        public List<RoleResponsibility> Responsibilities = new List<RoleResponsibility>();
        public List<ExpertiseArea> RequiredExpertise = new List<ExpertiseArea>();
    }
    
    
    [System.Serializable]
    public class SocialAchievementConfig
    {
        public string AchievementName;
        public SocialAchievementType AchievementType;
        public List<SocialCriterion> Criteria = new List<SocialCriterion>();
        [Range(0.1f, 10.0f)] public float ReputationReward = 2.0f;
        public bool IsLegacyAchievement = false;
        public string Description;
        public Sprite AchievementIcon;
    }
    
    [System.Serializable]
    public class SocialCriterion
    {
        public string CriterionName;
        public SocialCriterionType CriterionType;
        public float RequiredValue;
        public string Description;
    }
    
    [System.Serializable]
    public class CommunityRankConfig
    {
        public string RankName;
        public CommunityRank Rank;
        [Range(0f, 1000f)] public float ReputationThreshold = 0f;
        [Range(0.1f, 5.0f)] public float RankMultiplier = 1.0f;
        public Color RankColor = Color.white;
        public List<string> UnlockedPrivileges = new List<string>();
        public string Description;
        public Sprite RankIcon;
    }
    
    [System.Serializable]
    public class EndorsementTypeConfig
    {
        public string EndorsementName;
        public EndorsementType Type;
        [Range(0.1f, 5.0f)] public float EndorsementValue = 1.0f;
        public List<ExpertiseArea> ApplicableAreas = new List<ExpertiseArea>();
        public bool RequiresVerification = false;
    }
    
    [System.Serializable]
    public class KnowledgeContributionType
    {
        public string ContributionName;
        public ContributionType ContributionType;
        [Range(0.1f, 10.0f)] public float BaseValue = 1.0f;
        public bool RequiresPeerReview = false;
        public bool EnablesInnovationTracking = true;
        public string Description;
    }
    
    [System.Serializable]
    public class CommunityEventConfig
    {
        public string EventName;
        public CommunityEventType EventType;
        public bool IsActive = true;
        public bool IsRecurring = false;
        public bool IsOneTime = true;
        [Range(1, 365)] public int DurationDays = 7;
        [Range(0.1f, 5.0f)] public float EventBonus = 1.5f;
        public List<string> EventRewards = new List<string>();
    }
    
    [System.Serializable]
    public class SeasonalCommunityActivity
    {
        public string ActivityName;
        public GameSeason Season;
        public bool IsActive = true;
        [Range(0.1f, 3.0f)] public float SeasonalBonus = 1.2f;
        public List<SeasonalObjective> Objectives = new List<SeasonalObjective>();
    }
    
    
    [System.Serializable]
    public class InnovationShowcaseConfig
    {
        public string ShowcaseName;
        public InnovationShowcaseType ShowcaseType;
        [Range(1, 365)] public int SubmissionPeriod = 30;
        [Range(1, 100)] public int MaxSubmissions = 50;
        public bool EnablePeerVoting = true;
        public bool EnableExpertJudging = true;
        [Range(0.1f, 10.0f)] public float ShowcaseReward = 5.0f;
    }
    
    [System.Serializable]
    public class ExpertiseAreaConfig
    {
        public string AreaName;
        public ExpertiseArea Area;
        [Range(1, 50)] public int MinimumExpertiseLevel = 5;
        [Range(0.1f, 5.0f)] public float ExpertiseMultiplier = 1.0f;
        public List<string> RelatedSkills = new List<string>();
        public string Description;
    }
    
    [System.Serializable]
    public class ConsultationTypeConfig
    {
        public string ConsultationName;
        public ConsultationType Type;
        [Range(1, 480)] public int DurationMinutes = 60;
        [Range(0.1f, 5.0f)] public float ConsultationValue = 2.0f;
        public List<ExpertiseArea> RequiredExpertise = new List<ExpertiseArea>();
    }
    
    [System.Serializable]
    public class CommunityMemberProfile
    {
        public string MemberID;
        public string MemberName;
        public float CommunityReputation;
        public int ExpertiseLevel;
        public int CommunityContributions;
        public List<string> UnlockedAchievements = new List<string>();
        public List<ExpertiseArea> ExpertiseAreas = new List<ExpertiseArea>();
        public CommunityRank CurrentRank;
    }
    
    [System.Serializable]
    public class MentorshipActivity
    {
        public string ActivityName;
        public MentorshipActivityType ActivityType;
        [Range(1, 480)] public int DurationMinutes = 60;
        [Range(0.1f, 3.0f)] public float ProgressValue = 1.0f;
    }
    
    [System.Serializable]
    public class RoleResponsibility
    {
        public string ResponsibilityName;
        public RoleResponsibilityType ResponsibilityType;
        public float ImportanceWeight = 1.0f;
        public string Description;
    }
    
    #endregion
}
