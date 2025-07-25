using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Data.Economy.Gaming
{
    /// <summary>
    /// Economic gaming data structures for advanced trading and market gaming systems
    /// Focuses on entertaining market dynamics, trading competitions, and economic challenges
    /// Transforms economics from spreadsheets into exciting financial gaming experiences
    /// Uses separate namespace to avoid conflicts with existing economy types
    /// </summary>
    
    #region Market Trading Data
    
    [System.Serializable]
    public class TradingChallenge
    {
        public string ChallengeID;
        public string ChallengeName;
        public string Description;
        public TradingDifficulty Difficulty;
        public TradingChallengeType ChallengeType;
        public float TimeLimit;
        public float StartingCapital;
        public float TargetProfit;
        public List<string> AllowedCommodities = new List<string>();
        public List<string> ProhibitedCommodities = new List<string>();
        public TradingConstraints Constraints;
        public TradingRewards Rewards;
        public bool IsUnlocked;
        public bool IsCompleted;
        public DateTime CreatedDate;
        public float BestScore;
        public string BestPlayerID;
    }
    
    [System.Serializable]
    public class TradingSession
    {
        public string SessionID;
        public string PlayerID;
        public DateTime StartTime;
        public DateTime EndTime;
        public float InitialCapital;
        public float CurrentCapital;
        public float TotalProfit;
        public float ProfitPercentage;
        public List<TradingTransaction> Transactions = new List<TradingTransaction>();
        public List<MarketPosition> Positions = new List<MarketPosition>();
        public TradingSessionStatus Status;
        public TradingPerformance Performance;
        public bool IsActive;
    }
    
    [System.Serializable]
    public class TradingTransaction
    {
        public string TransactionID;
        public DateTime Timestamp;
        public string CommodityID;
        public GamingTransactionType TransactionType;
        public float Quantity;
        public float Price;
        public float TotalValue;
        public float Commission;
        public string MarketID;
        public TransactionTrigger Trigger;
        public string Notes;
    }
    
    [System.Serializable]
    public class MarketPosition
    {
        public string PositionID;
        public string CommodityID;
        public float Quantity;
        public float AveragePrice;
        public float CurrentPrice;
        public float UnrealizedPnL;
        public float RealizedPnL;
        public DateTime OpenDate;
        public DateTime? CloseDate;
        public GamingPositionType PositionType;
        public bool IsOpen;
    }
    
    [System.Serializable]
    public class TradingConstraints
    {
        public float MaxPositionSize;
        public float MaxLeverage;
        public int MaxDailyTrades;
        public float MinHoldingPeriod;
        public bool AllowShortSelling;
        public bool AllowOptionsTrading;
        public bool AllowFuturesTrading;
        public List<string> RestrictedMarkets = new List<string>();
    }
    
    [System.Serializable]
    public class TradingPerformance
    {
        public float TotalReturn;
        public float AnnualizedReturn;
        public float MaxDrawdown;
        public float SharpeRatio;
        public float WinRate;
        public int TotalTrades;
        public int WinningTrades;
        public int LosingTrades;
        public float AverageWin;
        public float AverageLoss;
        public float ProfitFactor;
        public DateTime LastUpdate;
    }
    
    #endregion
    
    #region Market Competition Data
    
    [System.Serializable]
    public class TradingCompetition
    {
        public string CompetitionID;
        public string CompetitionName;
        public string Description;
        public TradingCompetitionType CompetitionType;
        public DateTime StartDate;
        public DateTime EndDate;
        public float EntryFee;
        public float StartingCapital;
        public int MaxParticipants;
        public List<string> ParticipantIDs = new List<string>();
        public List<CompetitionLeaderboard> Leaderboard = new List<CompetitionLeaderboard>();
        public CompetitionRules Rules;
        public CompetitionPrizes Prizes;
        public CompetitionStatus Status;
        public bool IsPublic;
        public string SponsorID;
    }
    
    [System.Serializable]
    public class CompetitionLeaderboard
    {
        public int Rank;
        public string PlayerID;
        public string PlayerName;
        public float CurrentCapital;
        public float TotalReturn;
        public float RiskAdjustedReturn;
        public int TotalTrades;
        public float WinRate;
        public DateTime LastUpdate;
        public bool IsActive;
    }
    
    [System.Serializable]
    public class CompetitionRules
    {
        public List<string> AllowedStrategies = new List<string>();
        public List<string> ProhibitedStrategies = new List<string>();
        public float MaxLeverage;
        public int MaxDailyTrades;
        public bool AllowBotTrading;
        public bool AllowCopyTrading;
        public float MinimumActivity;
        public string DisqualificationCriteria;
    }
    
    [System.Serializable]
    public class CompetitionPrizes
    {
        public List<TradingPrize> FirstPlacePrizes = new List<TradingPrize>();
        public List<TradingPrize> SecondPlacePrizes = new List<TradingPrize>();
        public List<TradingPrize> ThirdPlacePrizes = new List<TradingPrize>();
        public List<TradingPrize> ParticipationPrizes = new List<TradingPrize>();
        public TradingPrize GrandPrize;
        public float TotalPrizePool;
    }
    
    [System.Serializable]
    public class TradingPrize
    {
        public string PrizeName;
        public string Description;
        public TradingPrizeType PrizeType;
        public float PrizeValue;
        public bool IsUnique;
        public string IconURL;
    }
    
    #endregion
    
    #region Market Simulation Data
    
    [System.Serializable]
    public class MarketSimulation
    {
        public string SimulationID;
        public string SimulationName;
        public MarketSimulationType SimulationType;
        public DateTime StartDate;
        public DateTime EndDate;
        public List<CommodityMarket> Markets = new List<CommodityMarket>();
        public List<MarketEvent> ScheduledEvents = new List<MarketEvent>();
        public GamingMarketConditions InitialConditions;
        public SimulationParameters Parameters;
        public bool IsActive;
        public float TimeAcceleration;
    }
    
    [System.Serializable]
    public class CommodityMarket
    {
        public string MarketID;
        public string CommodityID;
        public string CommodityName;
        public CommodityType CommodityType;
        public float CurrentPrice;
        public float OpenPrice;
        public float HighPrice;
        public float LowPrice;
        public float Volume;
        public float MarketCap;
        public List<GamingPricePoint> PriceHistory = new List<GamingPricePoint>();
        public GamingMarketTrend Trend;
        public float Volatility;
        public float Liquidity;
        public DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class GamingPricePoint
    {
        public DateTime Timestamp;
        public float Open;
        public float High;
        public float Low;
        public float Close;
        public float Volume;
    }
    
    [System.Serializable]
    public class MarketEvent
    {
        public string EventID;
        public string EventName;
        public string Description;
        public MarketEventType EventType;
        public DateTime ScheduledTime;
        public List<string> AffectedMarkets = new List<string>();
        public MarketImpact Impact;
        public float Probability;
        public bool HasOccurred;
        public DateTime? ActualTime;
    }
    
    [System.Serializable]
    public class MarketImpact
    {
        public float PriceImpact;
        public float VolumeImpact;
        public float VolatilityImpact;
        public float Duration;
        public ImpactDirection Direction;
        public List<string> SecondaryEffects = new List<string>();
    }
    
    [System.Serializable]
    public class GamingMarketConditions
    {
        public MarketSentiment OverallSentiment;
        public float InterestRate;
        public float InflationRate;
        public float EconomicGrowth;
        public SeasonalFactors Seasonality;
        public List<MacroeconomicIndicator> Indicators = new List<MacroeconomicIndicator>();
        public DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class MacroeconomicIndicator
    {
        public string IndicatorName;
        public float CurrentValue;
        public float PreviousValue;
        public float ChangePercentage;
        public IndicatorTrend Trend;
        public float Importance;
    }
    
    #endregion
    
    #region Trading Strategy Data
    
    // TradingStrategy already exists in base Economy namespace
    
    [System.Serializable]
    public class StrategyRule
    {
        public string RuleID;
        public string RuleName;
        public RuleType RuleType;
        public List<RuleCondition> Conditions = new List<RuleCondition>();
        public List<RuleAction> Actions = new List<RuleAction>();
        public float Priority;
        public bool IsActive;
    }
    
    [System.Serializable]
    public class RuleCondition
    {
        public string ConditionName;
        public ConditionType ConditionType;
        public string Parameter;
        public ComparisonOperator Operator;
        public float Value;
        public LogicalOperator LogicalOperator;
    }
    
    [System.Serializable]
    public class RuleAction
    {
        public string ActionName;
        public ActionType ActionType;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
        public float ExecutionDelay;
    }
    
    [System.Serializable]
    public class StrategyParameter
    {
        public string ParameterName;
        public ParameterType ParameterType;
        public object DefaultValue;
        public object MinValue;
        public object MaxValue;
        public string Description;
        public bool IsOptimizable;
    }
    
    [System.Serializable]
    public class StrategyPerformance
    {
        public float TotalReturn;
        public float AnnualizedReturn;
        public float MaxDrawdown;
        public float SharpeRatio;
        public float CalmarRatio;
        public float WinRate;
        public int TotalTrades;
        public float AverageHoldingPeriod;
        public DateTime BacktestStartDate;
        public DateTime BacktestEndDate;
    }
    
    #endregion
    
    #region Player Economics Data
    
    [System.Serializable]
    public class EconomicPlayerProfile
    {
        public string PlayerID;
        public string PlayerName;
        public int TradingLevel;
        public float TotalExperience;
        public EconomicSpecialization Specialization;
        public List<string> UnlockedStrategies = new List<string>();
        public List<string> UnlockedMarkets = new List<string>();
        public List<string> CompletedChallenges = new List<string>();
        public List<string> WonCompetitions = new List<string>();
        public EconomicStatistics Statistics;
        public EconomicPreferences Preferences;
        public DateTime LastActivity;
        public float TradingRating;
        public int TotalEarnings;
    }
    
    [System.Serializable]
    public class EconomicStatistics
    {
        public int ChallengesCompleted;
        public int CompetitionsEntered;
        public int CompetitionsWon;
        public float TotalProfitEarned;
        public float LargestSingleTrade;
        public float BestDailyReturn;
        public float WorstDailyReturn;
        public int ConsecutiveWins;
        public int ConsecutiveLosses;
        public float AverageTradeSize;
        public List<string> FavoriteStrategies = new List<string>();
        public TradingPersonality PersonalityType;
    }
    
    [System.Serializable]
    public class EconomicPreferences
    {
        public TradingStylePreference TradingStyle;
        public RiskTolerance RiskTolerance;
        public List<CommodityType> PreferredCommodities = new List<CommodityType>();
        public bool PreferAutomation;
        public bool PreferSocialTrading;
        public bool PreferCompetitions;
        public float PreferredTradeSize;
        public TimeHorizon PreferredTimeHorizon;
    }
    
    #endregion
    
    #region Supporting Classes
    
    [System.Serializable]
    public class TradingRewards
    {
        public int Experience;
        public int Currency;
        public List<string> UnlockedStrategies = new List<string>();
        public List<string> UnlockedMarkets = new List<string>();
        public List<string> UnlockedChallenges = new List<string>();
        public List<TradingAchievement> Achievements = new List<TradingAchievement>();
        public float RatingBonus;
    }
    
    [System.Serializable]
    public class TradingAchievement
    {
        public string AchievementID;
        public string AchievementName;
        public string Description;
        public TradingAchievementCategory Category;
        public TradingAchievementRarity Rarity;
        public bool IsUnlocked;
        public DateTime UnlockDate;
        public TradingRewards Rewards;
    }
    
    [System.Serializable]
    public class SimulationParameters
    {
        public float MarketVolatility;
        public float EventFrequency;
        public float TrendStrength;
        public bool EnableRandomEvents;
        public bool EnableSeasonality;
        public float LiquidityFactor;
        public float TransactionCosts;
    }
    
    [System.Serializable]
    public class SeasonalFactors
    {
        public Dictionary<string, float> MonthlyMultipliers = new Dictionary<string, float>();
        public Dictionary<string, float> QuarterlyTrends = new Dictionary<string, float>();
        public bool EnableHarvestSeason;
        public bool EnableHolidayEffects;
    }
    
    #endregion
    
    #region Enums
    
    public enum TradingDifficulty
    {
        Tutorial,
        Beginner,
        Intermediate,
        Advanced,
        Expert,
        Master,
        Legendary
    }
    
    public enum TradingChallengeType
    {
        Profit_Target,
        Risk_Management,
        Volatility_Trading,
        Trend_Following,
        Arbitrage_Challenge,
        Options_Strategy,
        Futures_Trading,
        Portfolio_Building
    }
    
    public enum TradingSessionStatus
    {
        Active,
        Paused,
        Completed,
        Cancelled,
        Liquidated
    }
    
    public enum GamingPositionType
    {
        Long,
        Short,
        Option_Long,
        Option_Short,
        Future_Long,
        Future_Short
    }
    
    public enum GamingTransactionType
    {
        Buy,
        Sell,
        Short_Sell,
        Cover_Short,
        Option_Buy,
        Option_Sell,
        Future_Buy,
        Future_Sell
    }
    
    public enum GamingMarketTrend
    {
        Strong_Uptrend,
        Uptrend,
        Sideways,
        Downtrend,
        Strong_Downtrend,
        Volatile,
        Unknown
    }
    
    // TransactionType moved to base Economy namespace to avoid conflicts
    
    public enum TransactionTrigger
    {
        Manual,
        Stop_Loss,
        Take_Profit,
        Strategy_Signal,
        Market_Order,
        Limit_Order,
        Time_Based
    }
    
    // PositionType moved to base Economy namespace to avoid conflicts
    
    public enum TradingCompetitionType
    {
        Daily_Challenge,
        Weekly_Contest,
        Monthly_Championship,
        Seasonal_Tournament,
        Special_Event,
        Corporate_Challenge,
        Beginner_League,
        Professional_Series
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
    
    public enum TradingPrizeType
    {
        Currency,
        Experience,
        Strategies,
        Market_Access,
        Recognition,
        Physical_Prize,
        Special_Features,
        Mentorship
    }
    
    public enum MarketSimulationType
    {
        Historical_Replay,
        Predictive_Model,
        Stress_Test,
        Random_Walk,
        Event_Driven,
        Seasonal_Pattern,
        Custom_Scenario
    }
    
    public enum CommodityType
    {
        Cannabis_Flower,
        Cannabis_Extracts,
        Cannabis_Edibles,
        Cannabis_Seeds,
        Growing_Equipment,
        Processing_Equipment,
        Packaging_Materials,
        Testing_Services,
        Real_Estate,
        Licenses
    }
    
    // MarketTrend moved to base Economy namespace to avoid conflicts
    
    public enum MarketEventType
    {
        Regulatory_Change,
        Supply_Shortage,
        Demand_Surge,
        Technology_Breakthrough,
        Market_Crash,
        Economic_Report,
        Seasonal_Event,
        Company_News
    }
    
    public enum ImpactDirection
    {
        Positive,
        Negative,
        Neutral,
        Mixed
    }
    
    public enum MarketSentiment
    {
        Very_Bullish,
        Bullish,
        Neutral,
        Bearish,
        Very_Bearish,
        Uncertain
    }
    
    public enum IndicatorTrend
    {
        Rising,
        Falling,
        Stable,
        Volatile
    }
    
    // TradingStrategyType moved to base Economy namespace to avoid conflicts
    
    public enum RuleType
    {
        Entry_Rule,
        Exit_Rule,
        Risk_Management,
        Position_Sizing,
        Market_Filter,
        Time_Filter
    }
    
    // ConditionType moved to base Economy namespace to avoid conflicts
    
    public enum ComparisonOperator
    {
        Greater_Than,
        Less_Than,
        Equal_To,
        Not_Equal_To,
        Greater_Equal,
        Less_Equal,
        Between,
        Outside_Range
    }
    
    public enum LogicalOperator
    {
        And,
        Or,
        Not,
        Xor
    }
    
    public enum ActionType
    {
        Buy_Signal,
        Sell_Signal,
        Close_Position,
        Set_Stop_Loss,
        Set_Take_Profit,
        Adjust_Position,
        Send_Alert,
        Log_Event
    }
    
    public enum ParameterType
    {
        Float,
        Integer,
        Boolean,
        String,
        Enum,
        List
    }
    
    public enum EconomicSpecialization
    {
        Day_Trader,
        Swing_Trader,
        Position_Trader,
        Arbitrage_Specialist,
        Options_Trader,
        Futures_Specialist,
        Risk_Manager,
        Quantitative_Analyst
    }
    
    public enum TradingPersonality
    {
        Conservative,
        Moderate,
        Aggressive,
        Speculative,
        Analytical,
        Intuitive,
        Systematic,
        Opportunistic
    }
    
    public enum TradingStylePreference
    {
        Scalping,
        Day_Trading,
        Swing_Trading,
        Position_Trading,
        Buy_And_Hold,
        Algorithmic,
        Social_Trading
    }
    
    // RiskTolerance moved to base Economy namespace to avoid conflicts
    
    public enum TimeHorizon
    {
        Seconds,
        Minutes,
        Hours,
        Days,
        Weeks,
        Months,
        Years
    }
    
    public enum TradingAchievementCategory
    {
        Trading,
        Risk_Management,
        Strategy,
        Competition,
        Social,
        Learning,
        Innovation,
        Mastery
    }
    
    public enum TradingAchievementRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    #endregion
}