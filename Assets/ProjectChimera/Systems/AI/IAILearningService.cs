using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectChimera.Data.AI;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// Interface for AI Learning Service - defines machine learning and model management contracts
    /// </summary>
    public interface IAILearningService
    {
        // Initialization
        bool IsInitialized { get; }
        void Initialize();
        void Shutdown();
        
        // Core Properties
        int ActiveModelCount { get; }
        int TotalLearningRecords { get; }
        int PendingTrainingRequests { get; }
        LearningStatistics Statistics { get; }
        Dictionary<string, MLModelData> ActiveModels { get; }
        
        // Learning Data Management
        void AddLearningRecord(string actionTaken, string context, string outcome, float successRating);
        void ProcessPlayerFeedback(string feedbackType, float rating, Dictionary<string, object> context);
        
        // Model Management
        void CreateModel(string modelId, ModelType modelType, string[] inputFeatures);
        Task<bool> TrainModelAsync(string modelId);
        Task<PredictionResult> MakePredictionAsync(string modelId, Dictionary<string, float> inputFeatures);
        void UpdateModelPerformance(string modelId, float actualValue, string predictionId);
        
        // Learning Optimization
        void OptimizeLearning();
        
        // Data Access
        LearningStatistics GetLearningStatistics();
        Dictionary<string, float> GetFeatureImportance();
        List<LearningRecord> GetRecentLearningRecords(int count = 100);
        
        // Events
        event Action<LearningRecord> OnLearningRecordAdded;
        event Action<MLModelData> OnModelTrained;
        event Action<MLModelData> OnModelUpdated;
        event Action<PredictionResult> OnPredictionMade;
        event Action<ModelPerformanceMetrics> OnModelPerformanceUpdated;
        event Action<string> OnModelRetired;
    }
}