using System;
using System.Collections.Generic;
using ProjectChimera.Data.AI;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// Interface for AI Personality Service - defines personality adaptation and player interaction contracts
    /// </summary>
    public interface IAIPersonalityService
    {
        // Initialization
        bool IsInitialized { get; }
        void Initialize();
        void Shutdown();
        
        // Core Properties
        AIPersonality CurrentPersonality { get; }
        CommunicationStyle CurrentCommunicationStyle { get; }
        PersonalityProfile ActiveProfile { get; }
        PlayerBehaviorAnalysis BehaviorAnalysis { get; }
        int InteractionCount { get; }
        
        // Player Interaction Management
        void RecordPlayerInteraction(string interactionType, string context, float satisfaction = 0.5f);
        void ProcessPlayerFeedback(string feedbackType, float rating, string context = "");
        
        // Personality Adaptation
        void AdaptPersonality();
        
        // Behavior Analysis
        void AnalyzePlayerBehavior();
        
        // Data Access
        PersonalityProfile GetPersonalityProfile();
        PlayerBehaviorAnalysis GetBehaviorAnalysis();
        Dictionary<string, float> GetPlayerPreferences();
        List<PlayerInteractionData> GetRecentInteractions(int count = 50);
        
        // Events
        event Action<AIPersonality, AIPersonality> OnPersonalityChanged;
        event Action<CommunicationStyle> OnCommunicationStyleChanged;
        event Action<PlayerInteractionData> OnPlayerInteractionRecorded;
        event Action<PersonalityProfile> OnPersonalityProfileUpdated;
        event Action<PlayerBehaviorAnalysis> OnBehaviorAnalysisUpdated;
    }
}