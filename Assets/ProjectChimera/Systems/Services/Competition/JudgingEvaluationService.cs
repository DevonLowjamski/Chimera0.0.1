using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Competition;
using ProjectChimera.Systems.Registry;
using JudgeQualificationLevel = ProjectChimera.Data.Competition.JudgeQualificationLevel;
using JudgingCriteria = ProjectChimera.Data.Competition.JudgingCriteria;
using ScoreBreakdown = ProjectChimera.Data.Competition.ScoreBreakdown;
using JudgeScorecard = ProjectChimera.Data.Competition.JudgeScorecard;
using Judge = ProjectChimera.Data.Competition.Judge;
using PlantRanking = ProjectChimera.Data.Competition.PlantRanking;
using WinnerSelection = ProjectChimera.Data.Competition.WinnerSelection;
using CompetitionResults = ProjectChimera.Data.Competition.CompetitionResults;
using JudgingSession = ProjectChimera.Data.Competition.JudgingSession;
using JudgeScore = ProjectChimera.Data.Competition.JudgeScore;

namespace ProjectChimera.Systems.Services.Competition
{
    /// <summary>
    /// PC014-1b: Judging Evaluation Service
    /// Manages scoring algorithms, judge assignment, and results calculation
    /// Decomposed from CannabisCupManager (460 lines target)
    /// </summary>
    public class JudgingEvaluationService : MonoBehaviour, IJudgingEvaluationService
    {
        #region Properties
        
        public bool IsInitialized { get; private set; }
        
        #endregion

        #region Private Fields
        
        [Header("Judging Configuration")]
        [SerializeField] private bool _enableCommunityJudging = true;
        [Range(0f, 1f)] [SerializeField] private float _judgingAccuracyWeight = 0.8f;
        [Range(1, 10)] [SerializeField] private int _minJudgesPerEntry = 3;
        [Range(1, 20)] [SerializeField] private int _maxJudgesPerEntry = 7;
        
        [Header("Active Data")]
        [SerializeField] private List<Judge> _activeJudges = new List<Judge>();
        [SerializeField] private List<JudgingSession> _activeSessions = new List<JudgingSession>();
        [SerializeField] private Dictionary<string, JudgingCriteria> _judgingStandards = new Dictionary<string, JudgingCriteria>();
        [SerializeField] private Dictionary<string, List<JudgeScore>> _entryScores = new Dictionary<string, List<JudgeScore>>();
        [SerializeField] private Dictionary<string, CompetitionResults> _calculatedResults = new Dictionary<string, CompetitionResults>();
        
        private Dictionary<string, JudgeQualificationLevel> _judgeQualifications = new Dictionary<string, JudgeQualificationLevel>();
        
        #endregion

        #region Events
        
        public event Action<string, string> OnScoreSubmitted; // judgeId, plantId
        public event Action<string> OnResultsCalculated; // competitionId
        public event Action<string, string> OnJudgeAssigned; // judgeId, competitionId
        public event Action<string, string> OnJudgingCompleted; // judgeId, entryId
        
        #endregion

        #region IService Implementation
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            Debug.Log("Initializing JudgingEvaluationService...");
            
            // Initialize judging standards
            InitializeJudgingStandards();
            
            // Load judge qualifications
            LoadJudgeQualifications();
            
            // Initialize scoring algorithms
            InitializeScoringAlgorithms();
            
            // Register with ServiceRegistry
            ServiceRegistry.Instance.RegisterService<IJudgingEvaluationService>(this, ServiceDomain.Competition);
            
            IsInitialized = true;
            Debug.Log("JudgingEvaluationService initialized successfully");
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            Debug.Log("Shutting down JudgingEvaluationService...");
            
            // Save judging state
            SaveJudgingState();
            
            // Clear collections
            _activeJudges.Clear();
            _activeSessions.Clear();
            _judgingStandards.Clear();
            _entryScores.Clear();
            _calculatedResults.Clear();
            _judgeQualifications.Clear();
            
            IsInitialized = false;
            Debug.Log("JudgingEvaluationService shutdown complete");
        }
        
        #endregion

        #region Scoring System
        
        public float CalculateScore(string plantId, JudgingCriteria criteria)
        {
            if (!IsInitialized)
            {
                Debug.LogError("JudgingEvaluationService not initialized");
                return 0f;
            }

            if (!_entryScores.ContainsKey(plantId))
            {
                Debug.LogWarning($"No scores found for plant {plantId}");
                return 0f;
            }

            var scores = _entryScores[plantId];
            if (!scores.Any())
            {
                return 0f;
            }

            // Calculate weighted average based on judge qualifications
            float totalScore = 0f;
            float totalWeight = 0f;

            foreach (var score in scores)
            {
                if (!_judgeQualifications.ContainsKey(score.JudgeId))
                    continue;

                var qualification = _judgeQualifications[score.JudgeId];
                float judgeWeight = GetJudgeWeight(qualification);
                
                float adjustedScore = CalculateAdjustedScore(score, criteria);
                
                totalScore += adjustedScore * judgeWeight;
                totalWeight += judgeWeight;
            }

            return totalWeight > 0 ? totalScore / totalWeight : 0f;
        }

        public ScoreBreakdown GetDetailedScore(string plantId, string judgeId)
        {
            if (!_entryScores.ContainsKey(plantId))
            {
                return null;
            }

            var judgeScore = _entryScores[plantId].FirstOrDefault(s => s.JudgeId == judgeId);
            if (judgeScore == null)
            {
                return null;
            }

            return new ScoreBreakdown
            {
                PlantId = plantId,
                JudgeId = judgeId,
                VisualScore = judgeScore.VisualScore,
                AromaScore = judgeScore.AromaScore,
                PotencyScore = judgeScore.PotencyScore,
                OverallScore = judgeScore.OverallScore,
                TotalScore = judgeScore.TotalScore,
                Comments = judgeScore.Comments,
                SubmissionTime = judgeScore.SubmissionTime
            };
        }

        public void SubmitJudgeScore(string judgeId, string plantId, JudgeScorecard scorecard)
        {
            if (!IsInitialized)
            {
                Debug.LogError("JudgingEvaluationService not initialized");
                return;
            }

            if (!ValidateJudge(judgeId))
            {
                Debug.LogError($"Invalid judge: {judgeId}");
                return;
            }

            if (!ValidateScorecard(scorecard))
            {
                Debug.LogError("Invalid scorecard data");
                return;
            }

            var judgeScore = new JudgeScore
            {
                JudgeId = judgeId,
                PlantId = plantId,
                VisualScore = scorecard.VisualScore,
                AromaScore = scorecard.AromaScore,
                PotencyScore = scorecard.PotencyScore,
                OverallScore = scorecard.OverallScore,
                TotalScore = CalculateTotalScore(scorecard),
                Comments = scorecard.Comments,
                SubmissionTime = DateTime.Now,
                IsValidated = true
            };

            if (!_entryScores.ContainsKey(plantId))
            {
                _entryScores[plantId] = new List<JudgeScore>();
            }

            // Remove existing score from same judge
            _entryScores[plantId].RemoveAll(s => s.JudgeId == judgeId);
            
            // Add new score
            _entryScores[plantId].Add(judgeScore);
            
            OnScoreSubmitted?.Invoke(judgeId, plantId);
            Debug.Log($"Score submitted by judge {judgeId} for plant {plantId}: {judgeScore.TotalScore:F2}");
        }
        
        #endregion

        #region Judge Management
        
        public bool AssignJudge(string judgeId, string competitionId)
        {
            if (!IsInitialized)
            {
                Debug.LogError("JudgingEvaluationService not initialized");
                return false;
            }

            var judge = _activeJudges.FirstOrDefault(j => j.JudgeId == judgeId);
            if (judge == null)
            {
                Debug.LogError($"Judge not found: {judgeId}");
                return false;
            }

            if (!ValidateJudge(judgeId))
            {
                Debug.LogError($"Judge validation failed: {judgeId}");
                return false;
            }

            // Check if judge is already assigned to competition
            if (judge.AssignedCompetitions.Contains(competitionId))
            {
                Debug.LogWarning($"Judge {judgeId} already assigned to competition {competitionId}");
                return true;
            }

            // Check judge availability
            if (judge.AssignedCompetitions.Count >= judge.MaxConcurrentCompetitions)
            {
                Debug.LogError($"Judge {judgeId} has reached maximum concurrent competitions");
                return false;
            }

            judge.AssignedCompetitions.Add(competitionId);
            
            OnJudgeAssigned?.Invoke(judgeId, competitionId);
            Debug.Log($"Assigned judge {judgeId} to competition {competitionId}");
            
            return true;
        }

        public bool ValidateJudge(string judgeId)
        {
            var judge = _activeJudges.FirstOrDefault(j => j.JudgeId == judgeId);
            if (judge == null)
                return false;

            // Check certification status
            if (!judge.IsCertified)
                return false;

            // Check qualification level
            if (!_judgeQualifications.ContainsKey(judgeId))
                return false;

            var qualification = _judgeQualifications[judgeId];
            return qualification != JudgeQualificationLevel.Unqualified;
        }

        public List<Judge> GetAssignedJudges(string competitionId)
        {
            return _activeJudges
                .Where(j => j.AssignedCompetitions.Contains(competitionId))
                .ToList();
        }

        public JudgeQualificationLevel GetJudgeLevel(string judgeId)
        {
            if (_judgeQualifications.ContainsKey(judgeId))
            {
                return _judgeQualifications[judgeId];
            }
            
            return JudgeQualificationLevel.Unqualified;
        }
        
        #endregion

        #region Results Processing
        
        public CompetitionResults CalculateResults(string competitionId)
        {
            if (!IsInitialized)
            {
                Debug.LogError("JudgingEvaluationService not initialized");
                return null;
            }

            if (_calculatedResults.ContainsKey(competitionId))
            {
                return _calculatedResults[competitionId];
            }

            var results = new CompetitionResults
            {
                CompetitionId = competitionId,
                CalculationDate = DateTime.Now,
                Rankings = new List<PlantRanking>(),
                CategoryWinners = new Dictionary<string, string>(),
                OverallWinner = null,
                IsFinalized = false
            };

            // Get all entries for competition
            var competitionEntries = GetCompetitionEntries(competitionId);
            
            // Calculate scores for each entry
            var entryScores = new Dictionary<string, float>();
            foreach (var entry in competitionEntries)
            {
                if (_entryScores.ContainsKey(entry.PlantId))
                {
                    var criteria = GetJudgingCriteriaForCompetition(competitionId);
                    entryScores[entry.PlantId] = CalculateScore(entry.PlantId, criteria);
                }
            }

            // Create rankings
            var sortedEntries = entryScores
                .OrderByDescending(kvp => kvp.Value)
                .Select((kvp, index) => new PlantRanking
                {
                    PlantId = kvp.Key,
                    Rank = index + 1,
                    Score = kvp.Value,
                    CompetitionId = competitionId
                })
                .ToList();

            results.Rankings = sortedEntries;
            
            // Determine overall winner
            if (sortedEntries.Any())
            {
                results.OverallWinner = sortedEntries.First().PlantId;
            }

            // Calculate category winners
            CalculateCategoryWinners(results, competitionEntries);
            
            _calculatedResults[competitionId] = results;
            
            OnResultsCalculated?.Invoke(competitionId);
            Debug.Log($"Calculated results for competition {competitionId}");
            
            return results;
        }

        public PlantRanking GetPlantRanking(string competitionId)
        {
            if (!_calculatedResults.ContainsKey(competitionId))
            {
                CalculateResults(competitionId);
            }

            var results = _calculatedResults[competitionId];
            return results?.Rankings?.FirstOrDefault();
        }

        public WinnerSelection DetermineWinners(string competitionId)
        {
            var results = CalculateResults(competitionId);
            if (results == null)
                return null;

            return new WinnerSelection
            {
                CompetitionId = competitionId,
                OverallWinner = results.OverallWinner,
                CategoryWinners = results.CategoryWinners,
                TopThree = results.Rankings.Take(3).Select(r => r.PlantId).ToList(),
                SelectionDate = DateTime.Now
            };
        }

        public bool ValidateResults(string competitionId)
        {
            if (!_calculatedResults.ContainsKey(competitionId))
                return false;

            var results = _calculatedResults[competitionId];
            
            // Validate that all entries have minimum required scores
            var competitionEntries = GetCompetitionEntries(competitionId);
            foreach (var entry in competitionEntries)
            {
                if (!_entryScores.ContainsKey(entry.PlantId))
                    return false;

                var scores = _entryScores[entry.PlantId];
                if (scores.Count < _minJudgesPerEntry)
                    return false;
            }

            // Validate score consistency
            foreach (var ranking in results.Rankings)
            {
                if (ranking.Score < 0 || ranking.Score > 100)
                    return false;
            }

            results.IsValidated = true;
            return true;
        }
        
        #endregion

        #region Private Helper Methods
        
        private void InitializeJudgingStandards()
        {
            _judgingStandards["Standard"] = new JudgingCriteria
            {
                VisualWeight = 0.25f,
                AromaWeight = 0.25f,
                PotencyWeight = 0.30f,
                OverallWeight = 0.20f,
                MaxScore = 100f,
                RequiredCertification = true
            };
            
            _judgingStandards["Championship"] = new JudgingCriteria
            {
                VisualWeight = 0.20f,
                AromaWeight = 0.20f,
                PotencyWeight = 0.35f,
                OverallWeight = 0.25f,
                MaxScore = 100f,
                RequiredCertification = true
            };
        }

        private void LoadJudgeQualifications()
        {
            // Initialize with default qualifications
            foreach (var judge in _activeJudges)
            {
                _judgeQualifications[judge.JudgeId] = DetermineJudgeQualification(judge);
            }
        }

        private void InitializeScoringAlgorithms()
        {
            Debug.Log("Initialized scoring algorithms");
        }

        private void SaveJudgingState()
        {
            Debug.Log("Saving judging state...");
        }

        private float GetJudgeWeight(JudgeQualificationLevel qualification)
        {
            return qualification switch
            {
                JudgeQualificationLevel.Master => 1.0f,
                JudgeQualificationLevel.Expert => 0.9f,
                JudgeQualificationLevel.Advanced => 0.8f,
                JudgeQualificationLevel.Intermediate => 0.7f,
                JudgeQualificationLevel.Novice => 0.6f,
                _ => 0.5f
            };
        }

        private float CalculateAdjustedScore(JudgeScore score, JudgingCriteria criteria)
        {
            return (score.VisualScore * criteria.VisualWeight +
                   score.AromaScore * criteria.AromaWeight +
                   score.PotencyScore * criteria.PotencyWeight +
                   score.OverallScore * criteria.OverallWeight) * _judgingAccuracyWeight;
        }

        private bool ValidateScorecard(JudgeScorecard scorecard)
        {
            return scorecard.VisualScore >= 0 && scorecard.VisualScore <= 100 &&
                   scorecard.AromaScore >= 0 && scorecard.AromaScore <= 100 &&
                   scorecard.PotencyScore >= 0 && scorecard.PotencyScore <= 100 &&
                   scorecard.OverallScore >= 0 && scorecard.OverallScore <= 100;
        }

        private float CalculateTotalScore(JudgeScorecard scorecard)
        {
            return (scorecard.VisualScore + scorecard.AromaScore + 
                   scorecard.PotencyScore + scorecard.OverallScore) / 4f;
        }

        private List<CompetitionEntry> GetCompetitionEntries(string competitionId)
        {
            // TODO: Get entries from CompetitionManagementService
            return new List<CompetitionEntry>();
        }

        private JudgingCriteria GetJudgingCriteriaForCompetition(string competitionId)
        {
            // TODO: Get criteria based on competition type
            return _judgingStandards.ContainsKey("Standard") ? 
                   _judgingStandards["Standard"] : 
                   new JudgingCriteria();
        }

        private void CalculateCategoryWinners(CompetitionResults results, List<CompetitionEntry> entries)
        {
            // Group entries by category and find winners
            var categoryGroups = entries.GroupBy(e => e.CategoryId);
            
            foreach (var group in categoryGroups)
            {
                var categoryEntries = group.ToList();
                var categoryRankings = results.Rankings
                    .Where(r => categoryEntries.Any(e => e.PlantId == r.PlantId))
                    .OrderByDescending(r => r.Score)
                    .ToList();

                if (categoryRankings.Any())
                {
                    results.CategoryWinners[group.Key] = categoryRankings.First().PlantId;
                }
            }
        }

        private JudgeQualificationLevel DetermineJudgeQualification(Judge judge)
        {
            // Determine qualification based on judge experience and certifications
            if (judge.YearsExperience >= 10 && judge.CompetitionsJudged >= 50)
                return JudgeQualificationLevel.Master;
            if (judge.YearsExperience >= 5 && judge.CompetitionsJudged >= 25)
                return JudgeQualificationLevel.Expert;
            if (judge.YearsExperience >= 3 && judge.CompetitionsJudged >= 10)
                return JudgeQualificationLevel.Advanced;
            if (judge.YearsExperience >= 1 && judge.CompetitionsJudged >= 5)
                return JudgeQualificationLevel.Intermediate;
            if (judge.IsCertified)
                return JudgeQualificationLevel.Novice;
            
            return JudgeQualificationLevel.Unqualified;
        }
        
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Shutdown();
        }
        
        #endregion
    }

    // Data structures moved to ProjectChimera.Data.Competition.CompetitionStructures
}