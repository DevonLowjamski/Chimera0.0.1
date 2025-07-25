using UnityEngine;
using ProjectChimera.Systems.Genetics;
using ProjectChimera.Data.Genetics;
using System.Linq;

/// <summary>
/// Simple compilation test for GeneticResearchManager
/// Verifies CS1955 and CS1061 LINQ errors are resolved
/// </summary>
public class ResearchCompilationTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== Research Compilation Test ===");
        
        // Test manager instantiation
        var testObject = new GameObject("TestResearchManager");
        var manager = testObject.AddComponent<GeneticResearchManager>();
        
        // Test basic functionality
        manager.EnableResearch = true;
        bool canManageResearch = manager != null;
        
        Debug.Log($"✅ GeneticResearchManager compilation: {(canManageResearch ? "Success" : "Failed")}");
        
        // Test LINQ operations on research phases
        var phases = new System.Collections.Generic.List<CleanResearchPhase>
        {
            new CleanResearchPhase { IsCompleted = true, Progress = 1f },
            new CleanResearchPhase { IsCompleted = false, Progress = 0.5f },
            new CleanResearchPhase { IsCompleted = false, Progress = 0f }
        };
        
        // Test Count() LINQ method
        int completedCount = phases.Count(p => p.IsCompleted);
        
        // Test LastOrDefault() LINQ method
        var lastIncomplete = phases.LastOrDefault(p => !p.IsCompleted);
        
        Debug.Log($"✅ LINQ operations: {completedCount} completed phases, last incomplete progress: {lastIncomplete?.Progress:P}");
        
        // Test research project with phases
        var project = new CleanGeneticResearchProject
        {
            ProjectID = "test_research",
            ProjectName = "Test Research Project",
            ResearchType = GeneticResearchType.TraitMapping,
            Phases = phases
        };
        
        Debug.Log($"✅ Research project: {project.ProjectName} with {project.Phases.Count} phases");
        
        // Cleanup
        DestroyImmediate(testObject);
        
        Debug.Log("✅ Research compilation test completed successfully");
    }
}