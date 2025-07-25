using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Data.AI
{
    /// <summary>
    /// AI gaming data structures for optimization challenges and automation competitions
    /// Focuses on intelligent systems, machine learning mini-games, and optimization puzzles
    /// Transforms AI from complex algorithms into engaging learning experiences
    /// </summary>
    
    #region AI Challenge Data
    
    [System.Serializable]
    public class AIOptimizationChallenge
    {
        public string ChallengeID;
        public string ChallengeName;
        public string Description;
        public AIChallengeDifficulty Difficulty;
        public AIChallengeType ChallengeType;
        public OptimizationObjective PrimaryObjective;
        public List<OptimizationObjective> SecondaryObjectives = new List<OptimizationObjective>();
        public AIConstraints Constraints;
        public AIRewards Rewards;
        public float TimeLimit;
        public bool IsUnlocked;
        public bool IsCompleted;
        public DateTime CreatedDate;
        public float BestScore;
        public string BestPlayerID;
        public AIAlgorithmType RequiredAlgorithm;
    }
    
    [System.Serializable]
    public class OptimizationObjective
    {
        public string ObjectiveID;
        public string ObjectiveName;
        public string Description;
        public OptimizationMetric MetricType;
        public float TargetValue;
        public float CurrentValue;
        public float Weight;
        public bool IsMaximization;
        public bool IsCompleted;
        public List<string> HintMessages = new List<string>();
    }
    
    [System.Serializable]
    public class AIConstraints
    {
        public float MaxComputationTime;
        public int MaxIterations;
        public float MaxMemoryUsage;
        public List<string> AllowedAlgorithms = new List<string>();
        public List<string> ProhibitedAlgorithms = new List<string>();
        public List<ParameterConstraint> ParameterLimits = new List<ParameterConstraint>();
        public bool RequireRealTimePerformance;
        public float MinAccuracy;
        public float MaxErrorRate;
    }
    
    [System.Serializable]
    public class AIRewards
    {
        public int Experience;
        public int Currency;
        public List<string> UnlockedAlgorithms = new List<string>();
        public List<string> UnlockedDatasets = new List<string>();
        public List<string> UnlockedChallenges = new List<string>();
        public List<AIAchievement> Achievements = new List<AIAchievement>();
        public float SkillRatingBonus;
    }
    
    #endregion
    
    #region Automation Competition Data
    
    [System.Serializable]
    public class AutomationCompetition
    {
        public string CompetitionID;
        public string CompetitionName;
        public string Description;
        public AutomationCompetitionType CompetitionType;
        public DateTime StartDate;
        public DateTime EndDate;
        public AutomationBrief Brief;
        public List<AutomationSubmission> Submissions = new List<AutomationSubmission>();
        public AutomationJudgingCriteria JudgingCriteria;
        public AutomationPrizes Prizes;
        public CompetitionStatus Status;
        public int MaxSubmissions;
        public bool IsPublic;
        public float DifficultyRating;
    }
    
    [System.Serializable]
    public class AutomationBrief
    {
        public string BriefTitle;
        public string BriefDescription;
        public AutomationDomain TargetDomain;
        public List<string> RequiredCapabilities = new List<string>();
        public List<string> OptionalCapabilities = new List<string>();
        public PerformanceRequirements Performance;
        public List<TestScenario> TestScenarios = new List<TestScenario>();
    }
    
    [System.Serializable]
    public class AutomationSubmission
    {
        public string SubmissionID;
        public string PlayerID;
        public string PlayerName;
        public DateTime SubmissionDate;
        public AIAlgorithmBlueprint Algorithm;
        public string ImplementationNotes;
        public List<string> TechnicalHighlights = new List<string>();
        public AutomationScores Scores;
        public bool IsValidated;
        public bool IsDisqualified;
        public string DisqualificationReason;
        public List<TechnicalComment> TechnicalComments = new List<TechnicalComment>();
        public List<PerformanceMetric> BenchmarkResults = new List<PerformanceMetric>();
    }
    
    [System.Serializable]
    public class AutomationScores
    {
        public float OverallScore;
        public float PerformanceScore;
        public float EfficiencyScore;
        public float InnovationScore;
        public float ReliabilityScore;
        public float ScalabilityScore;
        public float MaintainabilityScore;
        public float UserExperienceScore;
        public int PeerVotes;
        public float PeerRating;
        public int ExpertRank;
        public bool IsWinner;
    }
    
    #endregion
    
    #region AI Algorithm Data
    
    [System.Serializable]
    public class AIAlgorithmBlueprint
    {
        public string BlueprintID;
        public string BlueprintName;
        public string Description;
        public string CreatorPlayerID;
        public DateTime CreatedDate;
        public DateTime LastModified;
        public AIAlgorithmType AlgorithmType;
        public List<AlgorithmParameter> Parameters = new List<AlgorithmParameter>();
        public List<AlgorithmStep> Steps = new List<AlgorithmStep>();
        public AlgorithmPerformance PerformanceMetrics;
        public AlgorithmComplexity Complexity;
        public bool IsPublic;
        public bool IsTemplate;
        public float PopularityRating;
        public int UsageCount;
        public List<string> Tags = new List<string>();
    }
    
    [System.Serializable]
    public class AlgorithmParameter
    {
        public string ParameterName;
        public ParameterDataType DataType;
        public object DefaultValue;
        public object MinValue;
        public object MaxValue;
        public string Description;
        public bool IsOptimizable;
        public bool IsRequired;
        public ParameterCategory Category;
    }
    
    [System.Serializable]
    public class AlgorithmStep
    {
        public string StepID;
        public string StepName;
        public AlgorithmOperation Operation;
        public Dictionary<string, object> StepParameters = new Dictionary<string, object>();
        public List<string> InputConnections = new List<string>();
        public List<string> OutputConnections = new List<string>();
        public float ExecutionTime;
        public bool IsParallel;
        public int Priority;
    }
    
    [System.Serializable]
    public class AlgorithmPerformance
    {
        public float Accuracy;
        public float Precision;
        public float Recall;
        public float F1Score;
        public float ExecutionTime;
        public float MemoryUsage;
        public float CPUUsage;
        public float Scalability;
        public float Robustness;
        public DateTime LastBenchmark;
    }
    
    [System.Serializable]
    public class AlgorithmComplexity
    {
        public ComplexityClass TimeComplexity;
        public ComplexityClass SpaceComplexity;
        public int CodeLines;
        public int ConfigurationParameters;
        public float MaintainabilityIndex;
        public float LearningCurve;
    }
    
    #endregion
    
    #region AI Mini-Game Data
    
    [System.Serializable]
    public class AIMinigame
    {
        public string MinigameID;
        public string MinigameName;
        public string Description;
        public AIMinigameType MinigameType;
        public AIMinigameDifficulty Difficulty;
        public float TimeLimit;
        public AIMinigameObjective Objective;
        public List<AIMinigameLevel> Levels = new List<AIMinigameLevel>();
        public AIMinigameRewards Rewards;
        public AIMinigameScoring ScoringSystem;
        public bool IsUnlocked;
        public float BestScore;
        public int TimesPlayed;
    }
    
    [System.Serializable]
    public class AIMinigameLevel
    {
        public int LevelNumber;
        public string LevelName;
        public string LevelDescription;
        public AIMinigameChallenge Challenge;
        public float ParScore;
        public float TimeLimit;
        public bool IsCompleted;
        public float BestScore;
        public int Stars;
        public AIMinigameRewards CompletionReward;
    }
    
    [System.Serializable]
    public class AIMinigameChallenge
    {
        public string ChallengeDescription;
        public AILearningScenario Scenario;
        public List<string> SuccessConditions = new List<string>();
        public List<string> FailureConditions = new List<string>();
        public int MaxAttempts;
        public bool AllowHints;
        public List<string> HintMessages = new List<string>();
        public AIDataset TrainingData;
        public AIDataset TestData;
    }
    
    [System.Serializable]
    public class AILearningScenario
    {
        public string ScenarioName;
        public LearningTask TaskType;
        public DatasetType DataType;
        public int DataSize;
        public float NoiseLevel;
        public Dictionary<string, object> ScenarioParameters = new Dictionary<string, object>();
        public List<string> ExpectedOutcomes = new List<string>();
    }
    
    [System.Serializable]
    public class AIDataset
    {
        public string DatasetID;
        public string DatasetName;
        public DatasetType DataType;
        public int SampleCount;
        public int FeatureCount;
        public float DataQuality;
        public bool HasLabels;
        public DataComplexity Complexity;
        public List<string> DataSources = new List<string>();
    }
    
    #endregion
    
    #region Player Progress Data
    
    [System.Serializable]
    public class AIPlayerProfile
    {
        public string PlayerID;
        public string PlayerName;
        public int AILevel;
        public float TotalExperience;
        public AISpecialization Specialization;
        public List<string> UnlockedAlgorithms = new List<string>();
        public List<string> UnlockedDatasets = new List<string>();
        public List<string> CompletedChallenges = new List<string>();
        public List<string> WonCompetitions = new List<string>();
        public AIStatistics Statistics;
        public AIPreferences Preferences;
        public DateTime LastActivity;
        public float SkillRating;
        public int TotalOptimizations;
    }
    
    [System.Serializable]
    public class AIStatistics
    {
        public int AlgorithmsCreated;
        public int CompetitionsEntered;
        public int CompetitionsWon;
        public int ChallengesCompleted;
        public float TotalOptimizationGains;
        public float AverageAccuracy;
        public int MinigamesPlayed;
        public float AverageMinigameScore;
        public int PublicAlgorithms;
        public float InnovationScore;
        public List<string> FavoriteAlgorithms = new List<string>();
        public AIPersonality PersonalityType;
    }
    
    [System.Serializable]
    public class AIPreferences
    {
        public AIAlgorithmType PreferredAlgorithmType;
        public DatasetType PreferredDataType;
        public ComplexityLevel PreferredComplexity;
        public bool PreferPerformance;
        public bool PreferInterpretability;
        public bool PreferNovelty;
        public float AccuracyTolerance;
        public LearningStyle PreferredLearningStyle;
    }
    
    #endregion
    
    #region Supporting Classes
    
    [System.Serializable]
    public class ParameterConstraint
    {
        public string ParameterName;
        public object MinValue;
        public object MaxValue;
        public bool IsStrict;
        public string ViolationMessage;
    }
    
    [System.Serializable]
    public class AIAchievement
    {
        public string AchievementID;
        public string AchievementName;
        public string Description;
        public AchievementCategory Category;
        public AchievementRarity Rarity;
        public bool IsUnlocked;
        public DateTime UnlockDate;
        public AIRewards Rewards;
    }
    
    [System.Serializable]
    public class PerformanceRequirements
    {
        public float MaxResponseTime;
        public float MinAccuracy;
        public float MaxMemoryUsage;
        public float MinThroughput;
        public float MaxErrorRate;
        public bool RequireRealTime;
    }
    
    [System.Serializable]
    public class TestScenario
    {
        public string ScenarioName;
        public string Description;
        public TestDifficulty Difficulty;
        public Dictionary<string, object> InputParameters = new Dictionary<string, object>();
        public Dictionary<string, object> ExpectedOutputs = new Dictionary<string, object>();
        public float TimeLimit;
    }
    
    [System.Serializable]
    public class TechnicalComment
    {
        public string CommentID;
        public string AuthorID;
        public string AuthorName;
        public DateTime CommentDate;
        public string CommentText;
        public TechnicalCommentType CommentType;
        public float TechnicalRating;
        public bool IsPublic;
        public bool IsHighlighted;
    }
    
    [System.Serializable]
    public class PerformanceMetric
    {
        public string MetricName;
        public string MetricType;
        public float Value;
        public string Unit;
        public DateTime MeasuredAt;
        public string TestEnvironment;
    }
    
    [System.Serializable]
    public class AutomationJudgingCriteria
    {
        public float PerformanceWeight;
        public float EfficiencyWeight;
        public float InnovationWeight;
        public float ReliabilityWeight;
        public float ScalabilityWeight;
        public float MaintainabilityWeight;
        public float UserExperienceWeight;
        public bool UsePeerReview;
        public float PeerReviewWeight;
        public bool UseExpertPanel;
        public float ExpertPanelWeight;
    }
    
    [System.Serializable]
    public class AutomationPrizes
    {
        public List<AutomationPrize> FirstPlacePrizes = new List<AutomationPrize>();
        public List<AutomationPrize> SecondPlacePrizes = new List<AutomationPrize>();
        public List<AutomationPrize> ThirdPlacePrizes = new List<AutomationPrize>();
        public List<AutomationPrize> ParticipationPrizes = new List<AutomationPrize>();
        public List<AutomationPrize> SpecialCategoryPrizes = new List<AutomationPrize>();
    }
    
    [System.Serializable]
    public class AutomationPrize
    {
        public string PrizeName;
        public string Description;
        public PrizeType PrizeType;
        public object PrizeValue;
        public bool IsUnique;
        public string IconURL;
    }
    
    [System.Serializable]
    public class AIMinigameObjective
    {
        public string ObjectiveDescription;
        public AIGamingObjectiveType ObjectiveType;
        public float TargetValue;
        public string SuccessMessage;
        public string FailureMessage;
    }
    
    [System.Serializable]
    public class AIMinigameScoring
    {
        public ScoringType ScoringType;
        public float MaxScore;
        public Dictionary<string, float> ScoringWeights = new Dictionary<string, float>();
        public float AccuracyBonus;
        public float SpeedBonus;
        public float EfficiencyBonus;
    }
    
    [System.Serializable]
    public class AIMinigameRewards
    {
        public int Experience;
        public int Currency;
        public List<string> UnlockedContent = new List<string>();
        public float BonusMultiplier;
    }
    
    #endregion
    
    #region Enums
    
    public enum AIChallengeDifficulty
    {
        Tutorial,
        Beginner,
        Intermediate,
        Advanced,
        Expert,
        Research_Level,
        Theoretical
    }
    
    public enum AIChallengeType
    {
        Optimization_Challenge,
        Pattern_Recognition,
        Prediction_Task,
        Classification_Problem,
        Clustering_Challenge,
        Neural_Network_Design,
        Genetic_Algorithm_Tuning,
        Reinforcement_Learning
    }
    
    public enum OptimizationMetric
    {
        Accuracy,
        Precision,
        Recall,
        F1_Score,
        Speed,
        Memory_Efficiency,
        Energy_Consumption,
        Cost_Effectiveness,
        Robustness,
        Interpretability
    }
    
    public enum AIAlgorithmType
    {
        Linear_Regression,
        Decision_Tree,
        Random_Forest,
        Neural_Network,
        Deep_Learning,
        Genetic_Algorithm,
        Reinforcement_Learning,
        Support_Vector_Machine,
        K_Means_Clustering,
        Naive_Bayes
    }
    
    public enum AutomationCompetitionType
    {
        Optimization_Contest,
        Innovation_Challenge,
        Performance_Championship,
        Efficiency_Competition,
        Real_Time_Challenge,
        Scalability_Test,
        Robustness_Trial,
        Creative_AI_Contest
    }
    
    public enum CompetitionStatus
    {
        Upcoming,
        Registration_Open,
        In_Progress,
        Judging,
        Completed,
        Cancelled
    }
    
    public enum AutomationDomain
    {
        Cannabis_Cultivation,
        Environmental_Control,
        Resource_Management,
        Quality_Assurance,
        Process_Optimization,
        Predictive_Maintenance,
        Supply_Chain,
        Financial_Planning
    }
    
    public enum ParameterDataType
    {
        Float,
        Integer,
        Boolean,
        String,
        Enum,
        Array,
        Object
    }
    
    public enum ParameterCategory
    {
        Learning_Rate,
        Model_Architecture,
        Regularization,
        Optimization,
        Data_Processing,
        Feature_Selection,
        Hyperparameter,
        Environment
    }
    
    public enum AlgorithmOperation
    {
        Data_Input,
        Data_Processing,
        Feature_Extraction,
        Model_Training,
        Model_Evaluation,
        Optimization_Step,
        Output_Generation,
        Feedback_Loop
    }
    
    public enum ComplexityClass
    {
        O_1,        // Constant
        O_log_n,    // Logarithmic
        O_n,        // Linear
        O_n_log_n,  // Linearithmic
        O_n2,       // Quadratic
        O_n3,       // Cubic
        O_2n,       // Exponential
        O_n_factorial // Factorial
    }
    
    public enum AIMinigameType
    {
        Parameter_Tuning,
        Algorithm_Assembly,
        Data_Cleaning,
        Feature_Engineering,
        Model_Selection,
        Hyperparameter_Search,
        Neural_Architecture_Search,
        Ensemble_Building
    }
    
    public enum AIMinigameDifficulty
    {
        Easy,
        Medium,
        Hard,
        Expert,
        Master
    }
    
    public enum LearningTask
    {
        Classification,
        Regression,
        Clustering,
        Dimensionality_Reduction,
        Reinforcement_Learning,
        Generative_Modeling,
        Anomaly_Detection,
        Time_Series_Prediction
    }
    
    public enum DatasetType
    {
        Numerical,
        Categorical,
        Text,
        Image,
        Audio,
        Time_Series,
        Graph,
        Mixed
    }
    
    public enum DataComplexity
    {
        Simple,
        Moderate,
        Complex,
        Very_Complex,
        Extreme
    }
    
    public enum AISpecialization
    {
        Machine_Learning_Engineer,
        Deep_Learning_Specialist,
        Optimization_Expert,
        Data_Scientist,
        AI_Researcher,
        Automation_Engineer,
        Performance_Optimizer,
        Algorithm_Designer
    }
    
    public enum AIPersonality
    {
        Analytical,
        Creative,
        Systematic,
        Experimental,
        Performance_Focused,
        Innovation_Driven,
        Detail_Oriented,
        Big_Picture_Thinker
    }
    
    public enum ComplexityLevel
    {
        Simple,
        Moderate,
        Complex,
        Very_Complex,
        Extreme
    }
    
    public enum LearningStyle
    {
        Visual,
        Analytical,
        Experimental,
        Theoretical,
        Practical,
        Collaborative,
        Independent,
        Interactive
    }
    
    public enum AchievementCategory
    {
        Algorithm_Design,
        Optimization,
        Competition,
        Innovation,
        Performance,
        Learning,
        Teaching,
        Research
    }
    
    public enum AchievementRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    public enum TestDifficulty
    {
        Basic,
        Intermediate,
        Advanced,
        Expert,
        Research_Level
    }
    
    public enum TechnicalCommentType
    {
        Code_Review,
        Performance_Analysis,
        Algorithm_Suggestion,
        Bug_Report,
        Optimization_Tip,
        Documentation,
        Question,
        Praise
    }
    
    public enum PrizeType
    {
        Currency,
        Algorithms,
        Datasets,
        Experience,
        Recognition,
        Access,
        Mentorship,
        Research_Credits
    }
    
    public enum AIGamingObjectiveType
    {
        Accuracy_Target,
        Speed_Target,
        Efficiency_Target,
        Innovation_Target,
        Completion_Target,
        Learning_Target
    }
    
    public enum ScoringType
    {
        Points,
        Percentage,
        Accuracy_Based,
        Speed_Based,
        Efficiency_Based,
        Composite
    }
    
    #endregion
}