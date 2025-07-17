using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Mentorship Program Configuration - Configuration for mentorship systems in community collaboration
    /// Defines mentorship program parameters, mentor qualifications, and mentorship activities
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Mentorship Program Config", menuName = "Project Chimera/Gaming/Mentorship Program Config")]
    public class MentorshipProgramConfigSO : ChimeraConfigSO
    {
        [Header("Program Settings")]
        public string ProgramName = "";
        public MentorshipProgramType ProgramType = MentorshipProgramType.BasicGuidance;
        [Range(1, 20)] public int MaxMentees = 5;
        [Range(1, 365)] public int ProgramDuration = 90;
        [Range(0.1f, 5.0f)] public float ReputationReward = 2.0f;
        
        [Header("Focus Areas")]
        public List<ExpertiseArea> FocusAreas = new List<ExpertiseArea>();
        
        [Header("Program Activities")]
        public List<MentorshipActivity> Activities = new List<MentorshipActivity>();
        
        [Header("Qualification Requirements")]
        public List<MentorQualificationCriteria> QualificationCriteria = new List<MentorQualificationCriteria>();
        
        [Header("Reward Structure")]
        public List<MentorshipRewardConfig> Rewards = new List<MentorshipRewardConfig>();
        
        [TextArea(3, 5)]
        public string Description = "";
        
        #region Validation
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            ValidateProgram();
            ValidateActivities();
            ValidateQualifications();
        }
        
        private void ValidateProgram()
        {
            if (MaxMentees <= 0)
            {
                Debug.LogWarning($"Mentorship program {ProgramName} has invalid max mentees", this);
            }
            
            if (ProgramDuration <= 0)
            {
                Debug.LogWarning($"Mentorship program {ProgramName} has invalid duration", this);
            }
        }
        
        private void ValidateActivities()
        {
            if (Activities.Count == 0)
            {
                Debug.LogWarning($"Mentorship program {ProgramName} has no activities defined", this);
            }
        }
        
        private void ValidateQualifications()
        {
            if (QualificationCriteria.Count == 0)
            {
                Debug.LogWarning($"Mentorship program {ProgramName} has no qualification criteria", this);
            }
        }
        
        #endregion
        
        #region Runtime Methods
        
        public bool QualifiesAsMentor(CommunityMemberProfile profile)
        {
            foreach (var criteria in QualificationCriteria)
            {
                if (!EvaluateQualification(criteria, profile))
                {
                    return false;
                }
            }
            return true;
        }
        
        public MentorshipActivity GetActivity(MentorshipActivityType activityType)
        {
            return Activities.Find(a => a.ActivityType == activityType);
        }
        
        public MentorshipRewardConfig GetReward(MentorshipMilestone milestone)
        {
            return Rewards.Find(r => r.MilestoneType == milestone);
        }
        
        private bool EvaluateQualification(MentorQualificationCriteria criteria, CommunityMemberProfile profile)
        {
            if (profile.CommunityReputation < criteria.MinimumReputation) return false;
            if (profile.ExpertiseLevel < criteria.MinimumExpertiseLevel) return false;
            if (profile.CommunityContributions < criteria.MinimumContributions) return false;
            
            foreach (var requiredAchievement in criteria.RequiredAchievements)
            {
                if (!profile.UnlockedAchievements.Contains(requiredAchievement))
                    return false;
            }
            
            return true;
        }
        
        #endregion
    }
}