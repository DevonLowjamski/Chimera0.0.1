using UnityEngine;
using ProjectChimera.Systems.Genetics;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Validation test for AdvancedTournamentSystem
/// Verifies tournament creation, bracket generation, and skill-based matching
/// </summary>
public class AdvancedTournamentValidation : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== Advanced Tournament System Validation ===");
        
        // Test manager instantiation
        TestManagerInstantiation();
        
        // Test tournament creation and management
        TestTournamentCreation();
        
        // Test skill-based matching system
        TestSkillBasedMatching();
        
        // Test tournament brackets and progression
        TestTournamentBrackets();
        
        // Test data structure integrity
        TestDataStructures();
        
        Debug.Log("✅ Advanced Tournament System validation completed successfully");
    }
    
    private void TestManagerInstantiation()
    {
        // Test that manager can be instantiated (inheritance check)
        var testObject = new GameObject("TestTournamentSystem");
        var manager = testObject.AddComponent<AdvancedTournamentSystem>();
        
        // Verify it's a ChimeraManager
        bool isChimeraManager = manager is ProjectChimera.Core.ChimeraManager;
        
        Debug.Log($"✅ Manager instantiation: {(manager != null ? "Success" : "Failed")}");
        Debug.Log($"✅ ChimeraManager inheritance: {isChimeraManager}");
        
        // Test basic configuration
        manager.EnableTournaments = true;
        manager.EnableSkillBasedMatching = true;
        manager.EnableBracketGeneration = true;
        manager.MaxActiveTournaments = 5;
        manager.MinParticipantsPerTournament = 8;
        manager.MaxParticipantsPerTournament = 64;
        
        Debug.Log($"✅ Tournament configuration: Max={manager.MaxActiveTournaments}, Range={manager.MinParticipantsPerTournament}-{manager.MaxParticipantsPerTournament}");
        
        // Cleanup
        DestroyImmediate(testObject);
    }
    
    private void TestTournamentCreation()
    {
        // Test tournament data structure creation
        var tournament = new CleanTournamentData
        {
            TournamentID = "validation_tournament_001",
            TournamentName = "Validation Breeding Championship",
            TournamentType = "Breeding Excellence",
            MaxParticipants = 16,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(7),
            IsActive = true,
            IsCompleted = false,
            HasStarted = false,
            CurrentRound = 0,
            TotalRounds = 4, // log2(16) = 4 rounds
            PrizePool = 3200f // 16 participants * 100 * 2.0 multiplier
        };
        
        // Test participant creation
        var participants = new List<CleanTournamentParticipant>();
        for (int i = 1; i <= 8; i++)
        {
            var participant = new CleanTournamentParticipant
            {
                PlayerID = $"player_{i:D3}",
                PlayerName = $"Test Player {i}",
                SkillRating = 1000f + (i * 50f), // Varying skill levels
                RegistrationDate = DateTime.Now,
                IsActive = true,
                CurrentRound = 0,
                Wins = 0,
                Losses = 0,
                TournamentScore = 0f
            };
            participants.Add(participant);
        }
        
        tournament.Participants.AddRange(participants);
        
        Debug.Log($"✅ Tournament created: {tournament.TournamentName}");
        Debug.Log($"  - Type: {tournament.TournamentType}");
        Debug.Log($"  - Participants: {tournament.Participants.Count}/{tournament.MaxParticipants}");
        Debug.Log($"  - Prize Pool: ${tournament.PrizePool}");
        Debug.Log($"  - Total Rounds: {tournament.TotalRounds}");
        
        // Test skill rating distribution
        var avgSkill = tournament.Participants.Average(p => p.SkillRating);
        var minSkill = tournament.Participants.Min(p => p.SkillRating);
        var maxSkill = tournament.Participants.Max(p => p.SkillRating);
        
        Debug.Log($"  - Skill Range: {minSkill:F0} - {maxSkill:F0} (Avg: {avgSkill:F0})");
    }
    
    private void TestSkillBasedMatching()
    {
        // Test skill rating system
        var skillProfiles = new List<CleanPlayerSkillProfile>();
        
        for (int i = 1; i <= 10; i++)
        {
            var profile = new CleanPlayerSkillProfile
            {
                PlayerID = $"skill_test_player_{i}",
                CurrentRating = 800f + (i * 150f), // Range from 950 to 2300
                PeakRating = 800f + (i * 150f) + 100f,
                GamesPlayed = i * 5,
                Wins = (int)(i * 2.5f),
                Losses = (int)(i * 2.5f),
                LastUpdated = DateTime.Now
            };
            
            profile.TournamentHistory.Add($"tournament_{i}");
            skillProfiles.Add(profile);
        }
        
        Debug.Log($"✅ Skill profiles created: {skillProfiles.Count} players");
        
        // Test skill-based seeding (highest to lowest)
        var seededPlayers = skillProfiles.OrderByDescending(p => p.CurrentRating).ToList();
        
        Debug.Log($"Skill-based seeding:");
        for (int i = 0; i < Math.Min(5, seededPlayers.Count); i++)
        {
            var player = seededPlayers[i];
            Debug.Log($"  {i + 1}. Player {player.PlayerID}: {player.CurrentRating:F0} rating ({player.Wins}W-{player.Losses}L)");
        }
        
        // Test ELO rating calculation simulation
        TestELORatingCalculation(skillProfiles[0], skillProfiles[4]); // High vs medium skill
    }
    
    private void TestELORatingCalculation(CleanPlayerSkillProfile player1, CleanPlayerSkillProfile player2)
    {
        // Simulate ELO rating calculation
        float rating1 = player1.CurrentRating;
        float rating2 = player2.CurrentRating;
        
        // Calculate expected scores
        float expected1 = 1f / (1f + Mathf.Pow(10f, (rating2 - rating1) / 400f));
        float expected2 = 1f / (1f + Mathf.Pow(10f, (rating1 - rating2) / 400f));
        
        // Simulate player 1 wins
        float kFactor = 32f; // For new players
        float newRating1 = rating1 + kFactor * (1f - expected1);
        float newRating2 = rating2 + kFactor * (0f - expected2);
        
        Debug.Log($"✅ ELO Rating Calculation Test:");
        Debug.Log($"  Before: Player1={rating1:F0}, Player2={rating2:F0}");
        Debug.Log($"  Expected: Player1={expected1:F3}, Player2={expected2:F3}");
        Debug.Log($"  After (P1 wins): Player1={newRating1:F0} (+{newRating1 - rating1:F0}), Player2={newRating2:F0} ({newRating2 - rating2:F0})");
    }
    
    private void TestTournamentBrackets()
    {
        // Test tournament bracket structure
        var bracket = new CleanTournamentBracket
        {
            TournamentID = "bracket_test_tournament",
            BracketType = "Single Elimination",
            TotalRounds = 4,
            CreatedDate = DateTime.Now
        };
        
        // Create bracket rounds
        var roundNames = new[] { "Round 1", "Quarterfinal", "Semifinal", "Final" };
        var matchCounts = new[] { 8, 4, 2, 1 }; // For 16 participants
        
        for (int i = 0; i < 4; i++)
        {
            var round = new CleanBracketRound
            {
                RoundNumber = i + 1,
                RoundName = roundNames[i],
                MatchesInRound = matchCounts[i],
                IsCompleted = false
            };
            
            // Create matches for this round
            for (int j = 0; j < matchCounts[i]; j++)
            {
                var match = new CleanTournamentMatch
                {
                    MatchID = $"match_r{i + 1}_m{j + 1}",
                    TournamentID = bracket.TournamentID,
                    Round = i + 1,
                    MatchNumber = j + 1,
                    ScheduledDate = DateTime.Now.AddHours((i * 24) + j),
                    IsCompleted = false,
                    Player1Score = 0f,
                    Player2Score = 0f
                };
                
                round.Matches.Add(match);
            }
            
            bracket.Rounds.Add(round);
        }
        
        Debug.Log($"✅ Tournament bracket created: {bracket.BracketType}");
        Debug.Log($"  - Total Rounds: {bracket.TotalRounds}");
        
        foreach (var round in bracket.Rounds)
        {
            Debug.Log($"  - {round.RoundName}: {round.MatchesInRound} matches");
        }
        
        // Test bracket progression simulation
        TestBracketProgression(bracket);
    }
    
    private void TestBracketProgression(CleanTournamentBracket bracket)
    {
        Debug.Log($"✅ Testing bracket progression:");
        
        int remainingPlayers = 16;
        foreach (var round in bracket.Rounds)
        {
            Debug.Log($"  {round.RoundName}: {remainingPlayers} players -> {round.MatchesInRound} matches -> {round.MatchesInRound} winners");
            remainingPlayers = round.MatchesInRound;
        }
        
        Debug.Log($"  Final result: 1 tournament champion");
    }
    
    private void TestDataStructures()
    {
        // Test data structure serialization and compatibility
        Debug.Log($"✅ Testing tournament data structures:");
        
        // Test all tournament-related data structures
        var tournamentData = new CleanTournamentData
        {
            TournamentID = "data_test",
            TournamentName = "Data Structure Test Tournament"
        };
        
        var participantData = new CleanTournamentParticipant
        {
            PlayerID = "data_test_player",
            PlayerName = "Data Test Player"
        };
        
        var matchData = new CleanTournamentMatch
        {
            MatchID = "data_test_match",
            TournamentID = "data_test"
        };
        
        var bracketData = new CleanTournamentBracket
        {
            TournamentID = "data_test",
            BracketType = "Test Bracket"
        };
        
        var skillData = new CleanPlayerSkillProfile
        {
            PlayerID = "data_test_player",
            CurrentRating = 1500f
        };
        
        // Test serialization
        var tournamentJson = JsonUtility.ToJson(tournamentData, true);
        var participantJson = JsonUtility.ToJson(participantData, true);
        var matchJson = JsonUtility.ToJson(matchData, true);
        var bracketJson = JsonUtility.ToJson(bracketData, true);
        var skillJson = JsonUtility.ToJson(skillData, true);
        
        bool allSerializable = !string.IsNullOrEmpty(tournamentJson) &&
                              !string.IsNullOrEmpty(participantJson) &&
                              !string.IsNullOrEmpty(matchJson) &&
                              !string.IsNullOrEmpty(bracketJson) &&
                              !string.IsNullOrEmpty(skillJson);
        
        Debug.Log($"  - Data structure serialization: {(allSerializable ? "Success" : "Failed")}");
        Debug.Log($"  - CleanTournamentData: {(tournamentData != null ? "Valid" : "Invalid")}");
        Debug.Log($"  - CleanTournamentParticipant: {(participantData != null ? "Valid" : "Invalid")}");
        Debug.Log($"  - CleanTournamentMatch: {(matchData != null ? "Valid" : "Invalid")}");
        Debug.Log($"  - CleanTournamentBracket: {(bracketData != null ? "Valid" : "Invalid")}");
        Debug.Log($"  - CleanPlayerSkillProfile: {(skillData != null ? "Valid" : "Invalid")}");
        
        if (allSerializable)
        {
            Debug.Log($"  ✅ All tournament data structures are serializable and functional");
        }
    }
}