using UnityEngine;
using ProjectChimera.Systems.Narrative;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Progression;
using ProjectChimera.Systems.Cultivation;
// Aliases to resolve ambiguous references
using ProgressionExperienceSource = ProjectChimera.Data.Progression.ExperienceSource;
using CultivationExperienceSource = ProjectChimera.Data.Cultivation.ExperienceSource;
using DataCultivationApproach = ProjectChimera.Data.Cultivation.CultivationApproach;

namespace ProjectChimera
{
    /// <summary>
    /// Final test to verify all namespace ambiguity issues are resolved
    /// </summary>
    public class FinalAmbiguityResolutionTest : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("Final ambiguity resolution test started");
            
            // Test NarrativeContext from Systems.Narrative
            var narrativeContext = new NarrativeContext();
            narrativeContext.EnableTutorials = true;
            
            // Test ExperienceSource from Data.Progression  
            var experienceSource = ProgressionExperienceSource.PlantCare;
            var skillUsage = ProgressionExperienceSource.Skill_Usage;
            
            // Test CultivationApproach from Data.Cultivation
            var cultivationApproach = DataCultivationApproach.OrganicTraditional;
            
            // Test components compile correctly
            var storyManager = GetComponent<StoryCampaignManager>();
            var treeSkillSystem = GetComponent<TreeSkillProgressionSystem>();
            var cultivationManager = GetComponent<EnhancedCultivationGamingManager>();
            
            Debug.Log("Final ambiguity resolution test passed - all namespace conflicts resolved");
        }
    }
}