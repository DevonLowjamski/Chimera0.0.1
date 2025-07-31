using System;
using System.Threading.Tasks;
using ProjectChimera.Data.AI;
using ProjectChimera.Data.Cultivation;
using AIRecommendationPriority = ProjectChimera.Data.AI.RecommendationPriority;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// Interface for AI Recommendation Service
    /// Handles AI recommendation generation, management, and player interaction tracking
    /// </summary>
    public interface IAIRecommendationService
    {
        /// <summary>
        /// Whether the service is initialized and ready for use
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Number of currently active recommendations
        /// </summary>
        int ActiveRecommendationCount { get; }
        
        /// <summary>
        /// Number of pending recommendations waiting to be activated
        /// </summary>
        int PendingRecommendationCount { get; }
        
        /// <summary>
        /// Array of all currently active recommendations
        /// </summary>
        AIRecommendation[] ActiveRecommendations { get; }
        
        /// <summary>
        /// Current recommendation statistics
        /// </summary>
        RecommendationStatistics Statistics { get; }
        
        /// <summary>
        /// Initialize the recommendation service
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Shutdown the recommendation service
        /// </summary>
        void Shutdown();
        
        /// <summary>
        /// Generate recommendations based on cultivation analysis results
        /// </summary>
        /// <param name="analysisResult">Results from cultivation analysis</param>
        /// <returns>Array of generated recommendations</returns>
        Task<AIRecommendation[]> GenerateRecommendationsAsync(CultivationAnalysisResult analysisResult);
        
        /// <summary>
        /// Create a single recommendation with specified parameters
        /// </summary>
        /// <param name="title">Recommendation title</param>
        /// <param name="description">Detailed description</param>
        /// <param name="priority">Priority level</param>
        /// <param name="category">Recommendation category</param>
        /// <returns>Created recommendation</returns>
        AIRecommendation CreateRecommendation(string title, string description, AIRecommendationPriority priority, string category);
        
        /// <summary>
        /// Mark a recommendation as implemented by the player
        /// </summary>
        /// <param name="recommendationId">ID of the recommendation to implement</param>
        void ImplementRecommendation(string recommendationId);
        
        /// <summary>
        /// Dismiss a recommendation with optional reason
        /// </summary>
        /// <param name="recommendationId">ID of the recommendation to dismiss</param>
        /// <param name="dismissalReason">Reason for dismissal</param>
        void DismissRecommendation(string recommendationId, string dismissalReason);
        
        /// <summary>
        /// Get all recommendations for a specific category
        /// </summary>
        /// <param name="category">Category to filter by</param>
        /// <returns>Array of recommendations in the category</returns>
        AIRecommendation[] GetRecommendationsByCategory(string category);
        
        /// <summary>
        /// Get all recommendations with a specific priority level
        /// </summary>
        /// <param name="priority">Priority level to filter by</param>
        /// <returns>Array of recommendations with the specified priority</returns>
        AIRecommendation[] GetRecommendationsByPriority(AIRecommendationPriority priority);
        
        // Events
        
        /// <summary>
        /// Fired when new recommendations are generated
        /// </summary>
        event Action<AIRecommendation[]> OnRecommendationsGenerated;
        
        /// <summary>
        /// Fired when a single recommendation is created
        /// </summary>
        event Action<AIRecommendation> OnRecommendationCreated;
        
        /// <summary>
        /// Fired when a recommendation is implemented by the player
        /// </summary>
        event Action<AIRecommendation> OnRecommendationImplemented;
        
        /// <summary>
        /// Fired when a recommendation is dismissed by the player
        /// </summary>
        event Action<AIRecommendation> OnRecommendationDismissed;
        
        /// <summary>
        /// Fired when a recommendation expires
        /// </summary>
        event Action<AIRecommendation> OnRecommendationExpired;
    }
}