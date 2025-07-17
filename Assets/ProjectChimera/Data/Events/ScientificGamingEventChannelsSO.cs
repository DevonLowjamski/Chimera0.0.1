using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// Scientific Gaming Event Channels - Centralized event communication for Enhanced Scientific Gaming System v2.0
    /// Provides typed event channels for genetics, aromatics, competition, community, and progression systems
    /// Implements event-driven architecture for loose coupling between scientific gaming components
    /// </summary>
    [CreateAssetMenu(fileName = "New Scientific Gaming Event Channels", menuName = "Project Chimera/Events/Scientific Gaming Event Channels")]
    public class ScientificGamingEventChannelsSO : ChimeraDataSO
    {
        [Header("Core Scientific Gaming Events")]
        public GameEventChannelSO OnScientificSystemInitialized;
        public GameEventChannelSO OnScientificSystemShutdown;
        public GameEventChannelSO OnCrossSystemSynergyActivated;
        public GameEventChannelSO OnScientificMilestoneReached;
        public GameEventChannelSO OnInnovationBreakthroughAchieved;
        public GameEventChannelSO OnScientificReputationChanged;
        public GameEventChannelSO OnGlobalScientificEventTriggered;
        
        [Header("Genetics Gaming Events")]
        public GameEventChannelSO OnGeneticDiscoveryMade;
        public GameEventChannelSO OnBreedingChallengeStarted;
        public GameEventChannelSO OnBreedingChallengeCompleted;
        public GameEventChannelSO OnBreedingChallengeSuccess;
        public GameEventChannelSO OnBreedingChallengeFailed;
        public GameEventChannelSO OnGeneticInnovationAchieved;
        public GameEventChannelSO OnBreedingProjectCompleted;
        public GameEventChannelSO OnNovelTraitDiscovered;
        public GameEventChannelSO OnGeneticPuzzleSolved;
        public GameEventChannelSO OnBreedingMasteryUnlocked;
        public GameEventChannelSO OnGeneticsSynergyActivated;
        public GameEventChannelSO OnInheritancePatternDiscovered;
        
        [Header("Aromatic Gaming Events")]
        public GameEventChannelSO OnSensorySkillImproved;
        public GameEventChannelSO OnAromaticMasteryAchieved;
        public GameEventChannelSO OnTerpeneIdentified;
        public GameEventChannelSO OnTerpeneIdentificationSuccess;
        public GameEventChannelSO OnTerpeneIdentificationFailed;
        public GameEventChannelSO OnBlendingProjectStarted;
        public GameEventChannelSO OnBlendingProjectCompleted;
        public GameEventChannelSO OnAromaticInnovationCreated;
        public GameEventChannelSO OnSynergyDiscovered;
        public GameEventChannelSO OnSensoryMilestoneReached;
        public GameEventChannelSO OnAromaticProfilePerfected;
        public GameEventChannelSO OnFlavorCombinationUnlocked;
        
        [Header("Competition Events")]
        public GameEventChannelSO OnCompetitionEntered;
        public GameEventChannelSO OnCompetitionStarted;
        public GameEventChannelSO OnCompetitionCompleted;
        public GameEventChannelSO OnTournamentWon;
        public GameEventChannelSO OnTournamentLost;
        public GameEventChannelSO OnReputationGained;
        public GameEventChannelSO OnReputationLost;
        public GameEventChannelSO OnRankingChanged;
        public GameEventChannelSO OnAchievementEarned;
        public GameEventChannelSO OnEliteTierUnlocked;
        public GameEventChannelSO OnLegacyMilestoneReached;
        public GameEventChannelSO OnCompetitiveStreakAchieved;
        public GameEventChannelSO OnMatchmakingCompleted;
        public GameEventChannelSO OnSeasonRankingFinalized;
        
        [Header("Community Events")]
        public GameEventChannelSO OnMentorshipEstablished;
        public GameEventChannelSO OnMentorshipCompleted;
        public GameEventChannelSO OnMentorshipMilestoneReached;
        public GameEventChannelSO OnInnovationShared;
        public GameEventChannelSO OnCommunityProjectJoined;
        public GameEventChannelSO OnCommunityProjectCompleted;
        public GameEventChannelSO OnPeerEndorsementReceived;
        public GameEventChannelSO OnKnowledgeContributed;
        public GameEventChannelSO OnCommunityReputationChanged;
        public GameEventChannelSO OnCollaborativeBreakthrough;
        public GameEventChannelSO OnCommunityMilestoneReached;
        public GameEventChannelSO OnExpertConsultationRequested;
        public GameEventChannelSO OnCommunityEventParticipated;
        public GameEventChannelSO OnSocialRecognitionReceived;
        
        [Header("Progression Events")]
        public GameEventChannelSO OnSkillUnlocked;
        public GameEventChannelSO OnSkillLevelIncreased;
        public GameEventChannelSO OnSkillMasteryAchieved;
        public GameEventChannelSO OnProgressionMilestoneReached;
        public GameEventChannelSO OnExpertiseRecognized;
        public GameEventChannelSO OnProgressionSynergyActivated;
        public GameEventChannelSO OnScientificRankAdvanced;
        public GameEventChannelSO OnMasteryUnlocked;
        public GameEventChannelSO OnLegacyAchievementEarned;
        public GameEventChannelSO OnSeasonalProgressionCompleted;
        public GameEventChannelSO OnExperienceGained;
        public GameEventChannelSO OnLevelUp;
        public GameEventChannelSO OnSkillTreeCompleted;
        
        [Header("Integration Events")]
        public GameEventChannelSO OnSystemIntegrationActivated;
        public GameEventChannelSO OnCrossSystemAchievementUnlocked;
        public GameEventChannelSO OnUnifiedProgressionMilestone;
        public GameEventChannelSO OnGlobalSynergyActivated;
        public GameEventChannelSO OnIntegrationBonusApplied;
        public GameEventChannelSO OnSystemBalanceAchieved;
        public GameEventChannelSO OnFullIntegrationCompleted;
        
        [Header("Analytics Events")]
        public GameEventChannelSO OnPerformanceMetricUpdated;
        public GameEventChannelSO OnEfficiencyMilestoneReached;
        public GameEventChannelSO OnAnalyticsReportGenerated;
        public GameEventChannelSO OnOptimizationSuggested;
        public GameEventChannelSO OnProgressionAnalysisCompleted;
        
        [Header("Seasonal Events")]
        public GameEventChannelSO OnSeasonalEventStarted;
        public GameEventChannelSO OnSeasonalEventCompleted;
        public GameEventChannelSO OnSeasonalBonusActivated;
        public GameEventChannelSO OnSeasonalMilestoneReached;
        public GameEventChannelSO OnSeasonalRewardEarned;
        
        #region Validation
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            ValidateEventChannels();
        }
        
        private void ValidateEventChannels()
        {
            var eventChannels = GetAllEventChannels();
            var nullChannels = new List<string>();
            
            foreach (var channel in eventChannels)
            {
                if (channel.Value == null)
                {
                    nullChannels.Add(channel.Key);
                }
            }
            
            if (nullChannels.Count > 0)
            {
                Debug.LogWarning($"Unassigned event channels found: {string.Join(", ", nullChannels)}", this);
            }
        }
        
        #endregion
        
        #region Runtime Methods
        
        public void InitializeEventChannels()
        {
            var eventChannels = GetAllEventChannels();
            
            foreach (var channel in eventChannels)
            {
                if (channel.Value != null)
                {
                    channel.Value.Initialize();
                }
            }
            
            Debug.Log($"Initialized {eventChannels.Count} scientific gaming event channels", this);
        }
        
        public void RaiseEvent(string eventName, object eventData = null)
        {
            var eventChannel = GetEventChannel(eventName);
            eventChannel?.RaiseEvent(eventData);
        }
        
        public void SubscribeToEvent(string eventName, System.Action<object> callback)
        {
            var eventChannel = GetEventChannel(eventName);
            eventChannel?.RegisterListener(callback);
        }
        
        public void UnsubscribeFromEvent(string eventName, System.Action<object> callback)
        {
            var eventChannel = GetEventChannel(eventName);
            eventChannel?.UnregisterListener(callback);
        }
        
        public GameEventChannelSO GetEventChannel(string eventName)
        {
            var eventChannels = GetAllEventChannels();
            eventChannels.TryGetValue(eventName, out var channel);
            return channel;
        }
        
        public List<string> GetAllEventNames()
        {
            return new List<string>(GetAllEventChannels().Keys);
        }
        
        public Dictionary<string, GameEventChannelSO> GetEventChannelsByCategory(ScientificGamingEventCategory category)
        {
            var allChannels = GetAllEventChannels();
            var filteredChannels = new Dictionary<string, GameEventChannelSO>();
            
            foreach (var channel in allChannels)
            {
                if (GetEventCategory(channel.Key) == category)
                {
                    filteredChannels[channel.Key] = channel.Value;
                }
            }
            
            return filteredChannels;
        }
        
        #endregion
        
        #region Helper Methods
        
        private Dictionary<string, GameEventChannelSO> GetAllEventChannels()
        {
            var channels = new Dictionary<string, GameEventChannelSO>();
            
            // Core Scientific Gaming Events
            channels["OnScientificSystemInitialized"] = OnScientificSystemInitialized;
            channels["OnScientificSystemShutdown"] = OnScientificSystemShutdown;
            channels["OnCrossSystemSynergyActivated"] = OnCrossSystemSynergyActivated;
            channels["OnScientificMilestoneReached"] = OnScientificMilestoneReached;
            channels["OnInnovationBreakthroughAchieved"] = OnInnovationBreakthroughAchieved;
            channels["OnScientificReputationChanged"] = OnScientificReputationChanged;
            channels["OnGlobalScientificEventTriggered"] = OnGlobalScientificEventTriggered;
            
            // Genetics Gaming Events
            channels["OnGeneticDiscoveryMade"] = OnGeneticDiscoveryMade;
            channels["OnBreedingChallengeStarted"] = OnBreedingChallengeStarted;
            channels["OnBreedingChallengeCompleted"] = OnBreedingChallengeCompleted;
            channels["OnBreedingChallengeSuccess"] = OnBreedingChallengeSuccess;
            channels["OnBreedingChallengeFailed"] = OnBreedingChallengeFailed;
            channels["OnGeneticInnovationAchieved"] = OnGeneticInnovationAchieved;
            channels["OnBreedingProjectCompleted"] = OnBreedingProjectCompleted;
            channels["OnNovelTraitDiscovered"] = OnNovelTraitDiscovered;
            channels["OnGeneticPuzzleSolved"] = OnGeneticPuzzleSolved;
            channels["OnBreedingMasteryUnlocked"] = OnBreedingMasteryUnlocked;
            channels["OnGeneticsSynergyActivated"] = OnGeneticsSynergyActivated;
            channels["OnInheritancePatternDiscovered"] = OnInheritancePatternDiscovered;
            
            // Aromatic Gaming Events
            channels["OnSensorySkillImproved"] = OnSensorySkillImproved;
            channels["OnAromaticMasteryAchieved"] = OnAromaticMasteryAchieved;
            channels["OnTerpeneIdentified"] = OnTerpeneIdentified;
            channels["OnTerpeneIdentificationSuccess"] = OnTerpeneIdentificationSuccess;
            channels["OnTerpeneIdentificationFailed"] = OnTerpeneIdentificationFailed;
            channels["OnBlendingProjectStarted"] = OnBlendingProjectStarted;
            channels["OnBlendingProjectCompleted"] = OnBlendingProjectCompleted;
            channels["OnAromaticInnovationCreated"] = OnAromaticInnovationCreated;
            channels["OnSynergyDiscovered"] = OnSynergyDiscovered;
            channels["OnSensoryMilestoneReached"] = OnSensoryMilestoneReached;
            channels["OnAromaticProfilePerfected"] = OnAromaticProfilePerfected;
            channels["OnFlavorCombinationUnlocked"] = OnFlavorCombinationUnlocked;
            
            // Competition Events
            channels["OnCompetitionEntered"] = OnCompetitionEntered;
            channels["OnCompetitionStarted"] = OnCompetitionStarted;
            channels["OnCompetitionCompleted"] = OnCompetitionCompleted;
            channels["OnTournamentWon"] = OnTournamentWon;
            channels["OnTournamentLost"] = OnTournamentLost;
            channels["OnReputationGained"] = OnReputationGained;
            channels["OnReputationLost"] = OnReputationLost;
            channels["OnRankingChanged"] = OnRankingChanged;
            channels["OnAchievementEarned"] = OnAchievementEarned;
            channels["OnEliteTierUnlocked"] = OnEliteTierUnlocked;
            channels["OnLegacyMilestoneReached"] = OnLegacyMilestoneReached;
            channels["OnCompetitiveStreakAchieved"] = OnCompetitiveStreakAchieved;
            channels["OnMatchmakingCompleted"] = OnMatchmakingCompleted;
            channels["OnSeasonRankingFinalized"] = OnSeasonRankingFinalized;
            
            // Community Events
            channels["OnMentorshipEstablished"] = OnMentorshipEstablished;
            channels["OnMentorshipCompleted"] = OnMentorshipCompleted;
            channels["OnMentorshipMilestoneReached"] = OnMentorshipMilestoneReached;
            channels["OnInnovationShared"] = OnInnovationShared;
            channels["OnCommunityProjectJoined"] = OnCommunityProjectJoined;
            channels["OnCommunityProjectCompleted"] = OnCommunityProjectCompleted;
            channels["OnPeerEndorsementReceived"] = OnPeerEndorsementReceived;
            channels["OnKnowledgeContributed"] = OnKnowledgeContributed;
            channels["OnCommunityReputationChanged"] = OnCommunityReputationChanged;
            channels["OnCollaborativeBreakthrough"] = OnCollaborativeBreakthrough;
            channels["OnCommunityMilestoneReached"] = OnCommunityMilestoneReached;
            channels["OnExpertConsultationRequested"] = OnExpertConsultationRequested;
            channels["OnCommunityEventParticipated"] = OnCommunityEventParticipated;
            channels["OnSocialRecognitionReceived"] = OnSocialRecognitionReceived;
            
            // Progression Events
            channels["OnSkillUnlocked"] = OnSkillUnlocked;
            channels["OnSkillLevelIncreased"] = OnSkillLevelIncreased;
            channels["OnSkillMasteryAchieved"] = OnSkillMasteryAchieved;
            channels["OnProgressionMilestoneReached"] = OnProgressionMilestoneReached;
            channels["OnExpertiseRecognized"] = OnExpertiseRecognized;
            channels["OnProgressionSynergyActivated"] = OnProgressionSynergyActivated;
            channels["OnScientificRankAdvanced"] = OnScientificRankAdvanced;
            channels["OnMasteryUnlocked"] = OnMasteryUnlocked;
            channels["OnLegacyAchievementEarned"] = OnLegacyAchievementEarned;
            channels["OnSeasonalProgressionCompleted"] = OnSeasonalProgressionCompleted;
            channels["OnExperienceGained"] = OnExperienceGained;
            channels["OnLevelUp"] = OnLevelUp;
            channels["OnSkillTreeCompleted"] = OnSkillTreeCompleted;
            
            // Integration Events
            channels["OnSystemIntegrationActivated"] = OnSystemIntegrationActivated;
            channels["OnCrossSystemAchievementUnlocked"] = OnCrossSystemAchievementUnlocked;
            channels["OnUnifiedProgressionMilestone"] = OnUnifiedProgressionMilestone;
            channels["OnGlobalSynergyActivated"] = OnGlobalSynergyActivated;
            channels["OnIntegrationBonusApplied"] = OnIntegrationBonusApplied;
            channels["OnSystemBalanceAchieved"] = OnSystemBalanceAchieved;
            channels["OnFullIntegrationCompleted"] = OnFullIntegrationCompleted;
            
            // Analytics Events
            channels["OnPerformanceMetricUpdated"] = OnPerformanceMetricUpdated;
            channels["OnEfficiencyMilestoneReached"] = OnEfficiencyMilestoneReached;
            channels["OnAnalyticsReportGenerated"] = OnAnalyticsReportGenerated;
            channels["OnOptimizationSuggested"] = OnOptimizationSuggested;
            channels["OnProgressionAnalysisCompleted"] = OnProgressionAnalysisCompleted;
            
            // Seasonal Events
            channels["OnSeasonalEventStarted"] = OnSeasonalEventStarted;
            channels["OnSeasonalEventCompleted"] = OnSeasonalEventCompleted;
            channels["OnSeasonalBonusActivated"] = OnSeasonalBonusActivated;
            channels["OnSeasonalMilestoneReached"] = OnSeasonalMilestoneReached;
            channels["OnSeasonalRewardEarned"] = OnSeasonalRewardEarned;
            
            return channels;
        }
        
        private ScientificGamingEventCategory GetEventCategory(string eventName)
        {
            if (eventName.StartsWith("OnGenetic") || eventName.StartsWith("OnBreeding") || eventName.Contains("Inheritance"))
                return ScientificGamingEventCategory.Genetics;
            
            if (eventName.StartsWith("OnSensory") || eventName.StartsWith("OnAromatic") || eventName.StartsWith("OnTerpene") || eventName.StartsWith("OnBlending") || eventName.Contains("Flavor"))
                return ScientificGamingEventCategory.Aromatics;
            
            if (eventName.StartsWith("OnCompetition") || eventName.StartsWith("OnTournament") || eventName.Contains("Ranking") || eventName.Contains("Elite"))
                return ScientificGamingEventCategory.Competition;
            
            if (eventName.StartsWith("OnMentorship") || eventName.StartsWith("OnCommunity") || eventName.Contains("Peer") || eventName.Contains("Social") || eventName.Contains("Knowledge"))
                return ScientificGamingEventCategory.Community;
            
            if (eventName.StartsWith("OnSkill") || eventName.StartsWith("OnProgression") || eventName.Contains("Experience") || eventName.Contains("Level") || eventName.Contains("Mastery"))
                return ScientificGamingEventCategory.Progression;
            
            if (eventName.Contains("Integration") || eventName.Contains("Synergy") || eventName.Contains("CrossSystem"))
                return ScientificGamingEventCategory.Integration;
            
            if (eventName.Contains("Seasonal"))
                return ScientificGamingEventCategory.Seasonal;
            
            if (eventName.Contains("Analytics") || eventName.Contains("Performance") || eventName.Contains("Efficiency"))
                return ScientificGamingEventCategory.Analytics;
            
            return ScientificGamingEventCategory.Core;
        }
        
        #endregion
    }
    
    #region Enums
    
    public enum ScientificGamingEventCategory
    {
        Core,
        Genetics,
        Aromatics,
        Competition,
        Community,
        Progression,
        Integration,
        Analytics,
        Seasonal
    }
    
    #endregion
}