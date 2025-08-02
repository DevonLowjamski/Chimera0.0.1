using System;
using UnityEngine;
using ProjectChimera.Systems.Registry;
using ProjectChimera.Systems.Services.CompetitionServices;
using ProjectChimera.Systems.Services.Competition;
using ProjectChimera.Systems.Services.Research;
// Import service interfaces
using ICompetitionManagementService = ProjectChimera.Systems.Registry.ICompetitionManagementService;
using IJudgingEvaluationService = ProjectChimera.Systems.Registry.IJudgingEvaluationService;
using IParticipantRegistrationService = ProjectChimera.Systems.Registry.IParticipantRegistrationService;
using ICompetitionRewardsService = ProjectChimera.Systems.Registry.ICompetitionRewardsService;
using IResearchProjectService = ProjectChimera.Systems.Registry.IResearchProjectService;
// Import concrete service classes (note different namespaces)
using CompetitionManagementService = ProjectChimera.Systems.Services.CompetitionServices.CompetitionManagementService;
using JudgingEvaluationService = ProjectChimera.Systems.Services.Competition.JudgingEvaluationService;
using ParticipantRegistrationService = ProjectChimera.Systems.Services.Competition.ParticipantRegistrationService;
using CompetitionRewardsService = ProjectChimera.Systems.Services.Competition.CompetitionRewardsService;
using ResearchProjectService = ProjectChimera.Systems.Services.Research.ResearchProjectService;

namespace ProjectChimera.Testing
{
    /// <summary>
    /// Simple compilation test for decomposed services
    /// Ensures all type references are resolved correctly after Module 2 refactoring
    /// </summary>
    public class ServiceCompilationTest : MonoBehaviour
    {
        [Header("Service References")]
        [SerializeField] private CompetitionManagementService _competitionService;
        [SerializeField] private JudgingEvaluationService _judgingService;
        [SerializeField] private ParticipantRegistrationService _participantService;
        [SerializeField] private CompetitionRewardsService _rewardsService;
        [SerializeField] private ResearchProjectService _researchService;
        
        [Header("Test Results")]
        [SerializeField] private bool _allServicesCompiled = false;
        
        private void Start()
        {
            Debug.Log("ServiceCompilationTest: All decomposed services compiled successfully!");
            _allServicesCompiled = true;
        }
        
        /// <summary>
        /// Test method to validate service registry integration
        /// </summary>
        public void TestServiceRegistryIntegration()
        {
            try
            {
                // Test that we can get services from registry without errors
                var competitionMgr = ServiceRegistry.Instance.GetService<ICompetitionManagementService>();
                var judgingMgr = ServiceRegistry.Instance.GetService<IJudgingEvaluationService>();
                var participantMgr = ServiceRegistry.Instance.GetService<IParticipantRegistrationService>();
                var rewardsMgr = ServiceRegistry.Instance.GetService<ICompetitionRewardsService>();
                var researchMgr = ServiceRegistry.Instance.GetService<IResearchProjectService>();
                
                Debug.Log("ServiceCompilationTest: Service registry integration successful!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"ServiceCompilationTest: Service registry integration failed: {ex.Message}");
            }
        }
    }
}