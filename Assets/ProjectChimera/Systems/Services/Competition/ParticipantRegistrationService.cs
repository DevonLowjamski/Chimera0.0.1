using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Competition;
using ProjectChimera.Systems.Registry;
using RegistrationStatus = ProjectChimera.Data.Competition.RegistrationStatus;
using EntryStatus = ProjectChimera.Data.Competition.EntryStatus;
using PlantSubmission = ProjectChimera.Data.Competition.PlantSubmission;
using ParticipantRegistration = ProjectChimera.Data.Competition.ParticipantRegistration;
using PlantEntry = ProjectChimera.Data.Competition.PlantEntry;
using CompetitionRequirements = ProjectChimera.Data.Competition.CompetitionRequirements;
using QualificationResult = ProjectChimera.Data.Competition.QualificationResult;
using CompetitionRules = ProjectChimera.Data.Competition.CompetitionRules;
using ParticipantProfile = ProjectChimera.Data.Competition.ParticipantProfile;
using CompetitionEntry = ProjectChimera.Data.Competition.CompetitionEntry;

namespace ProjectChimera.Systems.Services.Competition
{
    /// <summary>
    /// PC014-1c: Participant Registration Service
    /// Handles contestant registration, validation, and communication
    /// Decomposed from CannabisCupManager (470 lines target)
    /// </summary>
    public class ParticipantRegistrationService : MonoBehaviour, IParticipantRegistrationService
    {
        #region Properties
        
        public bool IsInitialized { get; private set; }
        
        #endregion

        #region Private Fields
        
        [Header("Registration Configuration")]
        [SerializeField] private bool _enableOpenRegistration = true;
        [SerializeField] private bool _requirePreQualification = false;
        [Range(1, 100)] [SerializeField] private int _maxEntriesPerCompetition = 50;
        [Range(1, 5)] [SerializeField] private int _maxEntriesPerParticipant = 1;
        
        [Header("Active Data")]
        [SerializeField] private List<ParticipantRegistration> _activeRegistrations = new List<ParticipantRegistration>();
        [SerializeField] private List<CompetitionEntry> _pendingEntries = new List<CompetitionEntry>();
        [SerializeField] private Dictionary<string, ParticipantProfile> _participantProfiles = new Dictionary<string, ParticipantProfile>();
        [SerializeField] private Dictionary<string, List<string>> _competitionEntries = new Dictionary<string, List<string>>();
        
        private Dictionary<string, CompetitionRequirements> _competitionRequirements = new Dictionary<string, CompetitionRequirements>();
        private int _nextRegistrationId = 1;
        private int _nextEntryId = 1;
        
        #endregion

        #region Events
        
        public event Action<string> OnParticipantRegistered;
        public event Action<string> OnEntrySubmitted;
        public event Action<string> OnRegistrationCancelled;
        public event Action<string, string> OnEntryValidated; // entryId, status
        
        #endregion

        #region IService Implementation
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            Debug.Log("Initializing ParticipantRegistrationService...");
            
            // Initialize competition requirements
            InitializeCompetitionRequirements();
            
            // Load existing registrations
            LoadExistingRegistrations();
            
            // Initialize validation rules
            InitializeValidationRules();
            
            // Register with ServiceRegistry
            ServiceRegistry.Instance.RegisterService<IParticipantRegistrationService>(this, ServiceDomain.Competition);
            
            IsInitialized = true;
            Debug.Log("ParticipantRegistrationService initialized successfully");
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            Debug.Log("Shutting down ParticipantRegistrationService...");
            
            // Save registration state
            SaveRegistrationState();
            
            // Clear collections
            _activeRegistrations.Clear();
            _pendingEntries.Clear();
            _participantProfiles.Clear();
            _competitionEntries.Clear();
            _competitionRequirements.Clear();
            
            IsInitialized = false;
            Debug.Log("ParticipantRegistrationService shutdown complete");
        }
        
        #endregion

        #region Registration Management
        
        public string RegisterParticipant(string playerId, string competitionId, PlantSubmission submission)
        {
            if (!IsInitialized)
            {
                Debug.LogError("ParticipantRegistrationService not initialized");
                return null;
            }

            if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(competitionId))
            {
                Debug.LogError("Player ID and Competition ID are required");
                return null;
            }

            // Check if competition is accepting entries
            if (!IsCompetitionAcceptingEntries(competitionId))
            {
                Debug.LogError($"Competition {competitionId} is not accepting entries");
                return null;
            }

            // Check qualification requirements
            var qualificationResult = ValidateQualification(playerId, competitionId);
            if (!qualificationResult.IsQualified)
            {
                Debug.LogError($"Player {playerId} does not meet qualification requirements: {qualificationResult.FailureReason}");
                return null;
            }

            // Check entry limits
            if (!CheckEntryLimits(playerId, competitionId))
            {
                Debug.LogError($"Entry limits exceeded for player {playerId} in competition {competitionId}");
                return null;
            }

            string registrationId = GenerateRegistrationId();
            
            var registration = new ParticipantRegistration
            {
                RegistrationId = registrationId,
                PlayerId = playerId,
                CompetitionId = competitionId,
                RegistrationDate = DateTime.Now,
                Status = RegistrationStatus.Pending,
                PlantSubmission = submission,
                IsValidated = false,
                ValidationErrors = new List<string>()
            };

            _activeRegistrations.Add(registration);
            
            // Update participant profile
            UpdateParticipantProfile(playerId, competitionId);
            
            // Add to competition entries tracking
            if (!_competitionEntries.ContainsKey(competitionId))
            {
                _competitionEntries[competitionId] = new List<string>();
            }
            _competitionEntries[competitionId].Add(registrationId);
            
            OnParticipantRegistered?.Invoke(registrationId);
            Debug.Log($"Registered participant {playerId} for competition {competitionId} (ID: {registrationId})");
            
            return registrationId;
        }

        public bool ValidateRegistration(string registrationId)
        {
            var registration = _activeRegistrations.FirstOrDefault(r => r.RegistrationId == registrationId);
            if (registration == null)
            {
                Debug.LogError($"Registration not found: {registrationId}");
                return false;
            }

            var validationErrors = new List<string>();
            
            // Validate plant submission
            if (!ValidatePlantSubmission(registration.PlantSubmission, validationErrors))
            {
                registration.ValidationErrors = validationErrors;
                registration.Status = RegistrationStatus.ValidationFailed;
                return false;
            }

            // Validate competition requirements
            if (!ValidateCompetitionRequirements(registration, validationErrors))
            {
                registration.ValidationErrors = validationErrors;
                registration.Status = RegistrationStatus.ValidationFailed;
                return false;
            }

            // Validate documentation
            if (!ValidateDocumentation(registration, validationErrors))
            {
                registration.ValidationErrors = validationErrors;
                registration.Status = RegistrationStatus.ValidationFailed;
                return false;
            }

            registration.IsValidated = true;
            registration.Status = RegistrationStatus.Validated;
            registration.ValidationDate = DateTime.Now;
            registration.ValidationErrors.Clear();
            
            Debug.Log($"Registration {registrationId} validated successfully");
            return true;
        }

        public List<ParticipantRegistration> GetRegistrations(string competitionId)
        {
            return _activeRegistrations
                .Where(r => r.CompetitionId == competitionId)
                .ToList();
        }

        public ParticipantRegistration GetRegistration(string registrationId)
        {
            return _activeRegistrations.FirstOrDefault(r => r.RegistrationId == registrationId);
        }

        public bool CancelRegistration(string registrationId)
        {
            var registration = _activeRegistrations.FirstOrDefault(r => r.RegistrationId == registrationId);
            if (registration == null)
            {
                Debug.LogError($"Registration not found: {registrationId}");
                return false;
            }

            if (registration.Status == RegistrationStatus.Submitted)
            {
                Debug.LogError("Cannot cancel submitted registration");
                return false;
            }

            _activeRegistrations.Remove(registration);
            
            // Remove from competition entries
            if (_competitionEntries.ContainsKey(registration.CompetitionId))
            {
                _competitionEntries[registration.CompetitionId].Remove(registrationId);
            }
            
            OnRegistrationCancelled?.Invoke(registrationId);
            Debug.Log($"Cancelled registration {registrationId}");
            
            return true;
        }
        
        #endregion

        #region Entry Management
        
        public bool SubmitEntry(string registrationId, PlantEntry entry)
        {
            var registration = _activeRegistrations.FirstOrDefault(r => r.RegistrationId == registrationId);
            if (registration == null)
            {
                Debug.LogError($"Registration not found: {registrationId}");
                return false;
            }

            if (registration.Status != RegistrationStatus.Validated)
            {
                Debug.LogError($"Registration {registrationId} must be validated before entry submission");
                return false;
            }

            // Validate entry data
            var competitionRules = GetCompetitionRules(registration.CompetitionId);
            if (!ValidateEntry(entry, competitionRules))
            {
                Debug.LogError($"Entry validation failed for registration {registrationId}");
                return false;
            }

            string entryId = GenerateEntryId();
            
            var competitionEntry = new CompetitionEntry
            {
                EntryId = entryId,
                RegistrationId = registrationId,
                CompetitionId = registration.CompetitionId,
                PlayerId = registration.PlayerId,
                CategoryId = entry.CategoryId,
                SubmissionDate = DateTime.Now,
                PlantData = registration.PlantSubmission,
                Status = EntryStatus.Submitted,
                IsEligible = true
            };

            _pendingEntries.Add(competitionEntry);
            registration.Status = RegistrationStatus.Submitted;
            registration.EntryId = entryId;
            
            OnEntrySubmitted?.Invoke(entryId);
            Debug.Log($"Entry submitted for registration {registrationId} (Entry ID: {entryId})");
            
            return true;
        }

        public PlantEntry GetEntry(string registrationId)
        {
            var registration = _activeRegistrations.FirstOrDefault(r => r.RegistrationId == registrationId);
            if (registration == null || string.IsNullOrEmpty(registration.EntryId))
            {
                return null;
            }

            var entry = _pendingEntries.FirstOrDefault(e => e.EntryId == registration.EntryId);
            if (entry == null)
            {
                return null;
            }

            return new PlantEntry
            {
                EntryId = entry.EntryId,
                CategoryId = entry.CategoryId,
                PlantSubmission = entry.PlantData,
                SubmissionDate = entry.SubmissionDate,
                Status = entry.Status
            };
        }

        public bool ValidateEntry(PlantEntry entry, CompetitionRules rules)
        {
            if (entry == null || rules == null)
                return false;

            // Check required documentation
            if (!ValidateRequiredDocumentation(entry, rules.RequiredDocumentation))
            {
                Debug.LogError("Missing required documentation");
                return false;
            }

            // Check plant submission validity
            if (!ValidatePlantSubmissionForEntry(entry.PlantSubmission))
            {
                Debug.LogError("Invalid plant submission data");
                return false;
            }

            // Check category eligibility
            if (!ValidateCategoryEligibility(entry))
            {
                Debug.LogError("Entry not eligible for selected category");
                return false;
            }

            return true;
        }

        public EntryStatus GetEntryStatus(string registrationId)
        {
            var registration = _activeRegistrations.FirstOrDefault(r => r.RegistrationId == registrationId);
            if (registration == null || string.IsNullOrEmpty(registration.EntryId))
            {
                return EntryStatus.NotFound;
            }

            var entry = _pendingEntries.FirstOrDefault(e => e.EntryId == registration.EntryId);
            return entry?.Status ?? EntryStatus.NotFound;
        }
        
        #endregion

        #region Qualification System
        
        public bool CheckQualification(string playerId, CompetitionRequirements requirements)
        {
            if (!_participantProfiles.ContainsKey(playerId))
            {
                CreateParticipantProfile(playerId);
            }

            var profile = _participantProfiles[playerId];
            
            // Check minimum experience
            if (profile.CompetitionsEntered < requirements.MinimumCompetitions)
                return false;

            // Check skill level
            if (profile.SkillLevel < requirements.MinimumSkillLevel)
                return false;

            // Check age requirement
            if (profile.Age < requirements.MinimumAge)
                return false;

            // Check region eligibility
            if (requirements.RegionRestricted && !requirements.EligibleRegions.Contains(profile.Region))
                return false;

            return true;
        }

        public QualificationResult ValidateQualification(string playerId, string competitionId)
        {
            if (!_competitionRequirements.ContainsKey(competitionId))
            {
                return new QualificationResult
                {
                    IsQualified = true,
                    PlayerId = playerId,
                    CompetitionId = competitionId
                };
            }

            var requirements = _competitionRequirements[competitionId];
            var result = new QualificationResult
            {
                PlayerId = playerId,
                CompetitionId = competitionId,
                CheckedRequirements = requirements
            };

            if (!CheckQualification(playerId, requirements))
            {
                result.IsQualified = false;
                result.FailureReason = DetermineFailureReason(playerId, requirements);
            }
            else
            {
                result.IsQualified = true;
            }

            return result;
        }

        public List<string> GetQualificationRequirements(string competitionId)
        {
            if (!_competitionRequirements.ContainsKey(competitionId))
            {
                return new List<string> { "No specific requirements" };
            }

            var requirements = _competitionRequirements[competitionId];
            var reqList = new List<string>();

            if (requirements.MinimumCompetitions > 0)
                reqList.Add($"Minimum {requirements.MinimumCompetitions} previous competitions");
            
            if (requirements.MinimumSkillLevel > 1)
                reqList.Add($"Skill level {requirements.MinimumSkillLevel} or higher");
            
            if (requirements.MinimumAge > 0)
                reqList.Add($"Minimum age {requirements.MinimumAge}");
            
            if (requirements.RegionRestricted)
                reqList.Add($"Region restriction: {string.Join(", ", requirements.EligibleRegions)}");

            return reqList;
        }
        
        #endregion

        #region Communication
        
        public void NotifyParticipant(string registrationId, string message)
        {
            var registration = _activeRegistrations.FirstOrDefault(r => r.RegistrationId == registrationId);
            if (registration == null)
            {
                Debug.LogError($"Registration not found: {registrationId}");
                return;
            }

            // TODO: Integrate with notification system
            Debug.Log($"Notification to {registration.PlayerId}: {message}");
            
            // Store notification in registration history
            if (registration.NotificationHistory == null)
                registration.NotificationHistory = new List<string>();
            
            registration.NotificationHistory.Add($"{DateTime.Now:yyyy-MM-dd HH:mm}: {message}");
        }

        public void BroadcastToParticipants(string competitionId, string message)
        {
            var registrations = GetRegistrations(competitionId);
            
            foreach (var registration in registrations)
            {
                NotifyParticipant(registration.RegistrationId, message);
            }
            
            Debug.Log($"Broadcast sent to {registrations.Count} participants in competition {competitionId}");
        }
        
        #endregion

        #region Private Helper Methods
        
        private void InitializeCompetitionRequirements()
        {
            // Initialize default requirements for different competition types
            _competitionRequirements["Championship"] = new CompetitionRequirements
            {
                MinimumCompetitions = 5,
                MinimumSkillLevel = 3,
                MinimumAge = 21,
                RegionRestricted = false,
                RequiredCertifications = new List<string> { "Advanced Cultivation" }
            };
            
            _competitionRequirements["Regional"] = new CompetitionRequirements
            {
                MinimumCompetitions = 2,
                MinimumSkillLevel = 2,
                MinimumAge = 18,
                RegionRestricted = true,
                EligibleRegions = new List<string> { "North", "South", "East", "West" }
            };
        }

        private void LoadExistingRegistrations()
        {
            // TODO: Load from persistent storage
            Debug.Log("Loading existing registrations...");
        }

        private void InitializeValidationRules()
        {
            Debug.Log("Initialized validation rules");
        }

        private void SaveRegistrationState()
        {
            Debug.Log("Saving registration state...");
        }

        private string GenerateRegistrationId()
        {
            return $"REG_{DateTime.Now:yyyyMMdd}_{_nextRegistrationId++:D4}";
        }

        private string GenerateEntryId()
        {
            return $"ENT_{DateTime.Now:yyyyMMdd}_{_nextEntryId++:D4}";
        }

        private bool IsCompetitionAcceptingEntries(string competitionId)
        {
            // TODO: Check with CompetitionManagementService
            return _enableOpenRegistration;
        }

        private bool CheckEntryLimits(string playerId, string competitionId)
        {
            var playerRegistrations = _activeRegistrations
                .Where(r => r.PlayerId == playerId && r.CompetitionId == competitionId)
                .Count();
            
            return playerRegistrations < _maxEntriesPerParticipant;
        }

        private void UpdateParticipantProfile(string playerId, string competitionId)
        {
            if (!_participantProfiles.ContainsKey(playerId))
            {
                CreateParticipantProfile(playerId);
            }

            var profile = _participantProfiles[playerId];
            profile.CompetitionsEntered++;
            profile.LastCompetitionDate = DateTime.Now;
            
            if (!profile.CompetitionHistory.Contains(competitionId))
            {
                profile.CompetitionHistory.Add(competitionId);
            }
        }

        private void CreateParticipantProfile(string playerId)
        {
            _participantProfiles[playerId] = new ParticipantProfile
            {
                PlayerId = playerId,
                CreatedDate = DateTime.Now,
                CompetitionsEntered = 0,
                SkillLevel = 1,
                Age = 21, // Default age
                Region = "Unknown",
                CompetitionHistory = new List<string>()
            };
        }

        private bool ValidatePlantSubmission(PlantSubmission submission, List<string> errors)
        {
            if (submission == null)
            {
                errors.Add("Plant submission is required");
                return false;
            }

            if (string.IsNullOrEmpty(submission.StrainName))
            {
                errors.Add("Strain name is required");
                return false;
            }

            if (submission.TotalYield <= 0)
            {
                errors.Add("Total yield must be greater than 0");
                return false;
            }

            return true;
        }

        private bool ValidateCompetitionRequirements(ParticipantRegistration registration, List<string> errors)
        {
            var qualification = ValidateQualification(registration.PlayerId, registration.CompetitionId);
            if (!qualification.IsQualified)
            {
                errors.Add(qualification.FailureReason);
                return false;
            }

            return true;
        }

        private bool ValidateDocumentation(ParticipantRegistration registration, List<string> errors)
        {
            // TODO: Validate required documentation based on competition rules
            return true;
        }

        private CompetitionRules GetCompetitionRules(string competitionId)
        {
            // TODO: Get rules from CompetitionManagementService
            return new CompetitionRules();
        }

        private bool ValidateRequiredDocumentation(PlantEntry entry, List<string> requiredDocs)
        {
            // TODO: Check if all required documentation is present
            return true;
        }

        private bool ValidatePlantSubmissionForEntry(PlantSubmission submission)
        {
            return submission != null && !string.IsNullOrEmpty(submission.StrainName);
        }

        private bool ValidateCategoryEligibility(PlantEntry entry)
        {
            // TODO: Validate category eligibility based on plant characteristics
            return true;
        }

        private string DetermineFailureReason(string playerId, CompetitionRequirements requirements)
        {
            if (!_participantProfiles.ContainsKey(playerId))
                return "Player profile not found";

            var profile = _participantProfiles[playerId];
            
            if (profile.CompetitionsEntered < requirements.MinimumCompetitions)
                return $"Insufficient competition experience (need {requirements.MinimumCompetitions}, have {profile.CompetitionsEntered})";
            
            if (profile.SkillLevel < requirements.MinimumSkillLevel)
                return $"Skill level too low (need {requirements.MinimumSkillLevel}, have {profile.SkillLevel})";
            
            if (profile.Age < requirements.MinimumAge)
                return $"Age requirement not met (need {requirements.MinimumAge}, have {profile.Age})";
            
            if (requirements.RegionRestricted && !requirements.EligibleRegions.Contains(profile.Region))
                return $"Region not eligible (your region: {profile.Region})";

            return "Unknown qualification issue";
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