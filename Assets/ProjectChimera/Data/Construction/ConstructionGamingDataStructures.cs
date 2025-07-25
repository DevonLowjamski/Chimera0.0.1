using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.Facilities;

namespace ProjectChimera.Data.Construction
{
    /// <summary>
    /// Construction gaming data structures for facility building mini-games and challenges
    /// Focuses on interactive construction puzzles, design competitions, and resource management games
    /// Transforms facility building from static placement into engaging construction challenges
    /// </summary>
    
    #region Construction Challenge Data
    
    [System.Serializable]
    public class ConstructionChallenge
    {
        public string ChallengeID;
        public string ChallengeName;
        public string Description;
        public ConstructionDifficulty Difficulty;
        public ConstructionChallengeType ChallengeType;
        public ConstructionObjective PrimaryObjective;
        public List<ConstructionObjective> SecondaryObjectives = new List<ConstructionObjective>();
        public ConstructionConstraints Constraints;
        public ConstructionRewards Rewards;
        public float TimeLimit;
        public bool IsUnlocked;
        public bool IsCompleted;
        public DateTime CreatedDate;
        public float BestScore;
        public string BestPlayerID;
    }
    
    [System.Serializable]
    public class ConstructionObjective
    {
        public string ObjectiveID;
        public string ObjectiveName;
        public string Description;
        public ConstructionGamingObjectiveType ObjectiveType;
        public float TargetValue;
        public float CurrentValue;
        public bool IsCompleted;
        public ConstructionRewards CompletionReward;
        public List<string> HintMessages = new List<string>();
    }
    
    [System.Serializable]
    public class ConstructionConstraints
    {
        public float MaxBudget;
        public float MaxArea;
        public int MaxRooms;
        public List<string> RequiredComponents = new List<string>();
        public List<string> ProhibitedComponents = new List<string>();
        public List<EnvironmentalConstraint> EnvironmentalLimits = new List<EnvironmentalConstraint>();
        public bool MustBeEnergyEfficient;
        public float MinimumEfficiencyRating;
        public bool MustBeSustainable;
        public SeasonalConstraints SeasonalRequirements;
    }
    
    [System.Serializable]
    public class ConstructionRewards
    {
        public int Experience;
        public int Currency;
        public List<string> UnlockedComponents = new List<string>();
        public List<string> UnlockedBlueprints = new List<string>();
        public List<string> UnlockedChallenges = new List<string>();
        public List<ConstructionAchievement> Achievements = new List<ConstructionAchievement>();
        public float DesignRatingBonus;
    }
    
    #endregion
    
    #region Building Component Data
    
    // Using BuildingComponent from ProjectChimera.Data.Facilities namespace
    
    [System.Serializable]
    public class ComponentConnection
    {
        public string ConnectionID;
        public ConnectionType ConnectionType;
        public Vector3 LocalPosition;
        public Vector3 Direction;
        public bool IsInput;
        public bool IsOutput;
        public float Capacity;
        public List<string> CompatibleConnections = new List<string>();
    }
    
    [System.Serializable]
    public class ComponentEffect
    {
        public string EffectName;
        public ComponentEffectType EffectType;
        public float EffectValue;
        public float Range;
        public bool IsPositive;
        public List<string> AffectedCategories = new List<string>();
    }
    
    [System.Serializable]
    public class ComponentRequirements
    {
        public int MinimumPlayerLevel;
        public List<string> PrerequisiteComponents = new List<string>();
        public List<string> RequiredAchievements = new List<string>();
        public float MinimumBudget;
        public bool RequiresSpecialization;
        public string SpecializationRequired;
    }
    
    #endregion
    
    #region Design Competition Data
    
    [System.Serializable]
    public class DesignCompetition
    {
        public string CompetitionID;
        public string CompetitionName;
        public string Description;
        public DesignCompetitionType CompetitionType;
        public DateTime StartDate;
        public DateTime SubmissionDeadline;
        public DateTime EndDate;
        public DesignBrief Brief;
        public List<DesignSubmission> Submissions = new List<DesignSubmission>();
        public DesignJudgingCriteria JudgingCriteria;
        public DesignPrizes Prizes;
        public CompetitionStatus Status;
        public int MaxSubmissions;
        public bool IsPublic;
        public float DifficultyRating;
    }
    
    [System.Serializable]
    public class DesignBrief
    {
        public string BriefTitle;
        public string BriefDescription;
        public FacilityType TargetFacilityType;
        public float MaxBudget;
        public float MaxFootprint;
        public int MinimumRooms;
        public int MaximumRooms;
        public List<string> RequiredFeatures = new List<string>();
        public List<string> OptionalFeatures = new List<string>();
        public DesignTheme Theme;
        public List<DesignConstraint> SpecialConstraints = new List<DesignConstraint>();
    }
    
    [System.Serializable]
    public class DesignSubmission
    {
        public string SubmissionID;
        public string PlayerID;
        public string PlayerName;
        public DateTime SubmissionDate;
        public FacilityBlueprint Blueprint;
        public string DesignNotes;
        public List<string> DesignHighlights = new List<string>();
        public DesignScores Scores;
        public bool IsValidated;
        public bool IsDisqualified;
        public string DisqualificationReason;
        public List<DesignComment> PublicComments = new List<DesignComment>();
        public List<DesignComment> JudgeComments = new List<DesignComment>();
    }
    
    [System.Serializable]
    public class DesignScores
    {
        public float OverallScore;
        public float FunctionalityScore;
        public float EfficiencyScore;
        public float CreativityScore;
        public float SustainabilityScore;
        public float AestheticsScore;
        public float InnovationScore;
        public float BudgetEfficiencyScore;
        public int PublicVotes;
        public float PublicRating;
        public int JudgeRank;
        public bool IsWinner;
    }
    
    [System.Serializable]
    public class DesignComment
    {
        public string CommentID;
        public string AuthorID;
        public string AuthorName;
        public DateTime CommentDate;
        public string CommentText;
        public CommentType CommentType;
        public float Rating;
        public bool IsPublic;
        public bool IsHighlighted;
    }
    
    #endregion
    
    #region Facility Blueprint Data
    
    [System.Serializable]
    public class FacilityBlueprint
    {
        public string BlueprintID;
        public string BlueprintName;
        public string Description;
        public string CreatorPlayerID;
        public DateTime CreatedDate;
        public DateTime LastModified;
        public FacilityType FacilityType;
        public Vector3 Dimensions;
        public List<RoomLayout> Rooms = new List<RoomLayout>();
        public List<ComponentPlacement> Components = new List<ComponentPlacement>();
        public List<UtilityConnection> Utilities = new List<UtilityConnection>();
        public BlueprintMetrics Metrics;
        public BlueprintCosts Costs;
        public bool IsPublic;
        public bool IsTemplate;
        public float PopularityRating;
        public int DownloadCount;
        public List<string> Tags = new List<string>();
    }
    
    // Using RoomLayout from ProjectChimera.Data.Facilities namespace
    
    [System.Serializable]
    public class ComponentPlacement
    {
        public string PlacementID;
        public string ComponentID;
        public string RoomID;
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;
        public bool IsActive;
        public Dictionary<string, object> Settings = new Dictionary<string, object>();
        public List<string> ConnectedComponents = new List<string>();
    }
    
    // Using UtilityConnection from ProjectChimera.Data.Facilities namespace
    
    [System.Serializable]
    public class BlueprintMetrics
    {
        public float TotalArea;
        public float UsableArea;
        public float EfficiencyRating;
        public float EnergyConsumption;
        public float WaterConsumption;
        public float MaintenanceRequirement;
        public int PlantCapacity;
        public float ProductionCapacity;
        public float SustainabilityScore;
        public float SafetyRating;
        public float AutomationLevel;
    }
    
    [System.Serializable]
    public class BlueprintCosts
    {
        public float ConstructionCost;
        public float EquipmentCost;
        public float InstallationCost;
        public float PermitCost;
        public float TotalInitialCost;
        public float MonthlyOperatingCost;
        public float AnnualMaintenanceCost;
        public float ROIProjection;
        public float BreakEvenMonths;
    }
    
    #endregion
    
    #region Mini-Game Data
    
    [System.Serializable]
    public class ConstructionMinigame
    {
        public string MinigameID;
        public string MinigameName;
        public string Description;
        public MinigameType MinigameType;
        public MinigameDifficulty Difficulty;
        public float TimeLimit;
        public MinigameObjective Objective;
        public List<MinigameLevel> Levels = new List<MinigameLevel>();
        public MinigameRewards Rewards;
        public MinigameScoring ScoringSystem;
        public bool IsUnlocked;
        public float BestScore;
        public int TimesPlayed;
    }
    
    [System.Serializable]
    public class MinigameLevel
    {
        public int LevelNumber;
        public string LevelName;
        public string LevelDescription;
        public MinigameChallenge Challenge;
        public float ParScore;
        public float TimeLimit;
        public bool IsCompleted;
        public float BestScore;
        public int Stars;
        public MinigameRewards CompletionReward;
    }
    
    [System.Serializable]
    public class MinigameChallenge
    {
        public string ChallengeDescription;
        public ConstructionChallengeParameters Parameters;
        public List<string> SuccessConditions = new List<string>();
        public List<string> FailureConditions = new List<string>();
        public int MaxAttempts;
        public bool AllowHints;
        public List<string> HintMessages = new List<string>();
    }
    
    [System.Serializable]
    public class ConstructionChallengeParameters
    {
        public Dictionary<string, object> GameParameters = new Dictionary<string, object>();
        public List<string> AvailableTools = new List<string>();
        public List<ParameterConstraint> Constraints = new List<ParameterConstraint>();
        public float DifficultyMultiplier;
    }
    
    #endregion
    
    #region Player Progress Data
    
    [System.Serializable]
    public class ConstructionPlayerProfile
    {
        public string PlayerID;
        public string PlayerName;
        public int ConstructionLevel;
        public float TotalExperience;
        public ConstructionSpecialization Specialization;
        public List<string> UnlockedComponents = new List<string>();
        public List<string> UnlockedBlueprints = new List<string>();
        public List<string> CompletedChallenges = new List<string>();
        public List<string> WonCompetitions = new List<string>();
        public ConstructionStatistics Statistics;
        public ConstructionPreferences Preferences;
        public DateTime LastActivity;
        public float DesignRating;
        public int TotalDownloads;
    }
    
    [System.Serializable]
    public class ConstructionStatistics
    {
        public int BlueprintsCreated;
        public int CompetitionsEntered;
        public int CompetitionsWon;
        public int ChallengesCompleted;
        public float TotalBuildCost;
        public float AverageEfficiencyRating;
        public int MinigamesPlayed;
        public float AverageMinigameScore;
        public int PublicBlueprints;
        public float PopularityScore;
        public List<string> FavoriteComponents = new List<string>();
        public DesignStyle PreferredStyle;
    }
    
    [System.Serializable]
    public class ConstructionPreferences
    {
        public FacilityType PreferredFacilityType;
        public DesignTheme PreferredTheme;
        public DesignStyle PreferredStyle;
        public bool PreferSustainability;
        public bool PreferAutomation;
        public bool PreferAesthetics;
        public float BudgetPreference;
        public ComplexityLevel PreferredComplexity;
    }
    
    #endregion
    
    #region Supporting Classes
    
    [System.Serializable]
    public class EnvironmentalConstraint
    {
        public string ConstraintName;
        public EnvironmentalParameter Parameter;
        public float MinValue;
        public float MaxValue;
        public bool IsStrict;
    }
    
    [System.Serializable]
    public class SeasonalConstraints
    {
        public bool MustWorkInAllSeasons;
        public List<Season> CriticalSeasons = new List<Season>();
        public Dictionary<Season, float> SeasonalEfficiencyRequirements = new Dictionary<Season, float>();
    }
    
    [System.Serializable]
    public class ConstructionAchievement
    {
        public string AchievementID;
        public string AchievementName;
        public string Description;
        public AchievementCategory Category;
        public AchievementRarity Rarity;
        public bool IsUnlocked;
        public DateTime UnlockDate;
        public ConstructionRewards Rewards;
    }
    
    [System.Serializable]
    public class DesignConstraint
    {
        public string ConstraintName;
        public string Description;
        public ConstructionConstraintType ConstraintType;
        public object ConstraintValue;
        public bool IsMandatory;
        public float PenaltyWeight;
    }
    
    [System.Serializable]
    public class DesignJudgingCriteria
    {
        public float FunctionalityWeight;
        public float EfficiencyWeight;
        public float CreativityWeight;
        public float SustainabilityWeight;
        public float AestheticsWeight;
        public float InnovationWeight;
        public float BudgetWeight;
        public bool UsePublicVoting;
        public float PublicVoteWeight;
        public bool UsePeerReview;
        public float PeerReviewWeight;
    }
    
    [System.Serializable]
    public class DesignPrizes
    {
        public List<DesignPrize> FirstPlacePrizes = new List<DesignPrize>();
        public List<DesignPrize> SecondPlacePrizes = new List<DesignPrize>();
        public List<DesignPrize> ThirdPlacePrizes = new List<DesignPrize>();
        public List<DesignPrize> ParticipationPrizes = new List<DesignPrize>();
        public List<DesignPrize> SpecialCategoryPrizes = new List<DesignPrize>();
    }
    
    [System.Serializable]
    public class DesignPrize
    {
        public string PrizeName;
        public string Description;
        public PrizeType PrizeType;
        public object PrizeValue;
        public bool IsUnique;
        public string IconURL;
    }
    
    [System.Serializable]
    public class RoomFeature
    {
        public string FeatureName;
        public RoomFeatureType FeatureType;
        public Vector3 Position;
        public Dictionary<string, object> Properties = new Dictionary<string, object>();
        public bool IsRequired;
        public float Cost;
    }
    
    [System.Serializable]
    public class RoomEnvironment
    {
        public float Temperature;
        public float Humidity;
        public float LightLevel;
        public float AirCirculation;
        public float CO2Level;
        public bool HasClimateControl;
        public bool HasAirFiltration;
        public bool HasLightControl;
    }
    
    [System.Serializable]
    public class RoomAccess
    {
        public AccessLevel AccessLevel;
        public List<string> RequiredPermissions = new List<string>();
        public bool RequiresKeycard;
        public bool HasSecurityCamera;
        public bool HasEmergencyExit;
    }
    
    [System.Serializable]
    public class ConnectionEfficiency
    {
        public float EfficiencyRating;
        public float PowerLoss;
        public float MaintenanceRequirement;
        public List<string> EfficiencyFactors = new List<string>();
    }
    
    [System.Serializable]
    public class MinigameObjective
    {
        public string ObjectiveDescription;
        public ConstructionGamingObjectiveType ObjectiveType;
        public float TargetValue;
        public string SuccessMessage;
        public string FailureMessage;
    }
    
    [System.Serializable]
    public class MinigameScoring
    {
        public ScoringType ScoringType;
        public float MaxScore;
        public Dictionary<string, float> ScoringWeights = new Dictionary<string, float>();
        public float TimeBonus;
        public float AccuracyBonus;
        public float EfficiencyBonus;
    }
    
    [System.Serializable]
    public class MinigameRewards
    {
        public int Experience;
        public int Currency;
        public List<string> UnlockedContent = new List<string>();
        public float BonusMultiplier;
    }
    
    [System.Serializable]
    public class ParameterConstraint
    {
        public string ParameterName;
        public object MinValue;
        public object MaxValue;
        public bool IsStrict;
        public string ViolationMessage;
    }
    
    #endregion
    
    #region Enums
    
    public enum ConstructionDifficulty
    {
        Tutorial,
        Beginner,
        Intermediate,
        Advanced,
        Expert,
        Master,
        Legendary
    }
    
    public enum ConstructionChallengeType
    {
        Speed_Build,
        Budget_Challenge,
        Efficiency_Optimization,
        Creative_Design,
        Problem_Solving,
        Renovation_Challenge,
        Sustainability_Focus,
        Technology_Integration
    }
    
    public enum ConstructionObjectiveType
    {
        Build_Within_Budget,
        Achieve_Efficiency_Rating,
        Complete_Within_Time,
        Use_Specific_Components,
        Meet_Capacity_Requirements,
        Maximize_Sustainability,
        Minimize_Footprint,
        Achieve_Automation_Level
    }
    
    public enum ComponentType
    {
        Structural,
        Environmental,
        Electrical,
        Plumbing,
        HVAC,
        Lighting,
        Automation,
        Security,
        Safety,
        Aesthetic
    }
    
    public enum ComponentCategory
    {
        Foundation,
        Walls,
        Roofing,
        Flooring,
        Equipment,
        Utilities,
        Controls,
        Monitoring,
        Storage,
        Decoration
    }
    
    public enum ComponentEffectType
    {
        Efficiency_Boost,
        Cost_Reduction,
        Power_Consumption,
        Maintenance_Reduction,
        Capacity_Increase,
        Quality_Improvement,
        Automation_Enhancement,
        Safety_Improvement
    }
    
    public enum ComponentRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythical
    }
    
    public enum ConnectionType
    {
        Electrical,
        Water,
        Air,
        Data,
        Mechanical,
        Structural,
        Pneumatic,
        Hydraulic
    }
    
    public enum DesignCompetitionType
    {
        Open_Design,
        Themed_Challenge,
        Innovation_Contest,
        Efficiency_Competition,
        Sustainability_Challenge,
        Budget_Challenge,
        Speed_Design,
        Collaborative_Project
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
    
    public enum DesignTheme
    {
        Modern_Industrial,
        Eco_Friendly,
        High_Tech,
        Minimalist,
        Traditional,
        Futuristic,
        Rustic,
        Luxury
    }
    
    // Using FacilityType, RoomType, and UtilityType from ProjectChimera.Data.Facilities
    
    public enum MinigameType
    {
        Pipe_Connection,
        Wire_Routing,
        Component_Assembly,
        Layout_Puzzle,
        Resource_Management,
        Time_Management,
        Efficiency_Optimization,
        Problem_Diagnosis
    }
    
    public enum MinigameDifficulty
    {
        Easy,
        Medium,
        Hard,
        Expert,
        Master
    }
    
    public enum ConstructionSpecialization
    {
        Structural_Engineer,
        Environmental_Designer,
        Automation_Specialist,
        Sustainability_Expert,
        Cost_Optimizer,
        Innovation_Pioneer,
        Aesthetic_Designer,
        Efficiency_Master
    }
    
    public enum DesignStyle
    {
        Functional,
        Innovative,
        Sustainable,
        Aesthetic,
        Efficient,
        Traditional,
        Experimental,
        Balanced
    }
    
    public enum ComplexityLevel
    {
        Simple,
        Moderate,
        Complex,
        Very_Complex,
        Extreme
    }
    
    public enum EnvironmentalParameter
    {
        Temperature,
        Humidity,
        Light_Level,
        Air_Quality,
        Noise_Level,
        Vibration,
        Pressure,
        Chemical_Exposure
    }
    
    public enum Season
    {
        Spring,
        Summer,
        Fall,
        Winter
    }
    
    public enum AchievementCategory
    {
        Construction,
        Design,
        Competition,
        Innovation,
        Efficiency,
        Sustainability,
        Social,
        Mastery
    }
    
    public enum AchievementRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    public enum ConstructionConstraintType
    {
        Budget,
        Area,
        Component,
        Performance,
        Environmental,
        Regulatory,
        Safety,
        Aesthetic
    }
    
    public enum CommentType
    {
        General,
        Technical,
        Aesthetic,
        Suggestion,
        Criticism,
        Praise,
        Question,
        Judge_Review
    }
    
    public enum RoomFeatureType
    {
        Door,
        Window,
        Vent,
        Outlet,
        Light_Fixture,
        Drain,
        Shelf,
        Workstation,
        Storage,
        Equipment_Mount
    }
    
    public enum AccessLevel
    {
        Public,
        Restricted,
        Private,
        Secure,
        Emergency_Only
    }
    
    public enum ConstructionGamingObjectiveType
    {
        Score_Target,
        Time_Limit,
        Accuracy_Target,
        Efficiency_Target,
        Completion_Target,
        Innovation_Target
    }
    
    public enum ScoringType
    {
        Points,
        Percentage,
        Time_Based,
        Accuracy_Based,
        Efficiency_Based,
        Composite
    }
    
    public enum PrizeType
    {
        Currency,
        Components,
        Blueprints,
        Experience,
        Recognition,
        Access,
        Customization,
        Special_Item
    }
    
    #endregion
}