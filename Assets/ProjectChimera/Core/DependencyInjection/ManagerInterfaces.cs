using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.Cultivation;
using EnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Progression;
using ProjectChimera.Systems.SpeedTree;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Core manager interfaces for dependency injection
    /// These interfaces define contracts for Project Chimera's manager system
    /// </summary>

    /// <summary>
    /// Interface for Time Manager - handles game time, pausing, and temporal calculations
    /// </summary>
    public interface ITimeManager : IChimeraManager
    {
        float GameTime { get; }
        float DeltaTime { get; }
        float UnscaledDeltaTime { get; }
        float TimeScale { get; set; }
        bool IsPaused { get; }

        void PauseTime();
        void ResumeTime();
        void SetTimeScale(float scale);
        float GetScaledTime(float baseTime);
    }

    /// <summary>
    /// Interface for Data Manager - handles save/load operations and data persistence
    /// </summary>
    public interface IDataManager : IChimeraManager
    {
        bool HasSaveData { get; }
        string CurrentSaveFile { get; }

        void SaveGame(string saveFileName = null);
        void LoadGame(string saveFileName = null);
        void AutoSave();
        void DeleteSave(string saveFileName);
        IEnumerable<string> GetSaveFiles();
    }

    /// <summary>
    /// Interface for Event Manager - handles game-wide event communication
    /// </summary>
    public interface IEventManager : IChimeraManager
    {
        void Subscribe<T>(Action<T> callback) where T : class;
        void Unsubscribe<T>(Action<T> callback) where T : class;
        void Publish<T>(T eventData) where T : class;
        void PublishImmediate<T>(T eventData) where T : class;
        int GetSubscriberCount<T>() where T : class;
    }

    /// <summary>
    /// Interface for Settings Manager - handles game configuration and preferences
    /// </summary>
    public interface ISettingsManager : IChimeraManager
    {
        T GetSetting<T>(string key, T defaultValue = default);
        void SetSetting<T>(string key, T value);
        bool HasSetting(string key);
        void SaveSettings();
        void LoadSettings();
        void ResetToDefaults();
        IEnumerable<string> GetAllSettingKeys();
    }

    /// <summary>
    /// Interface for Plant Manager - handles plant lifecycle and cultivation
    /// </summary>
    public interface IPlantManager : IChimeraManager
    {
        int TotalPlantCount { get; }
        int HealthyPlantCount { get; }
        int MaturePlantCount { get; }

        GameObject CreatePlant(Vector3 position, PlantStrainSO strain = null);
        void RemovePlant(GameObject plant);
        void UpdatePlant(GameObject plant, float deltaTime);
        IEnumerable<GameObject> GetAllPlants();
        IEnumerable<GameObject> GetPlantsInRadius(Vector3 center, float radius);
        IEnumerable<GameObject> GetPlantsByStrain(PlantStrainSO strain);
        IEnumerable<GameObject> GetPlantsByGrowthStage(PlantGrowthStage stage);
    }

    /// <summary>
    /// Interface for Genetics Manager - handles breeding, genetics, and strain management
    /// </summary>
    public interface IGeneticsManager : IChimeraManager
    {
        IEnumerable<PlantStrainSO> AvailableStrains { get; }
        int DiscoveredTraitsCount { get; }

        PlantStrainSO BreedPlants(PlantStrainSO parent1, PlantStrainSO parent2);
        CannabisGenotype GenerateGenotype(PlantStrainSO strain);
        CannabisGeneticsEngine.CannabisPhenotype ExpressPhenotype(CannabisGenotype genotype, EnvironmentalConditions conditions);
        void DiscoverTrait(string traitId);
        bool IsTraitDiscovered(string traitId);
        float CalculateBreedingSuccess(PlantStrainSO parent1, PlantStrainSO parent2);
    }

    /// <summary>
    /// Interface for Environmental Manager - handles climate control and environmental conditions
    /// </summary>
    public interface IEnvironmentalManager : IChimeraManager
    {
        EnvironmentalConditions CurrentConditions { get; }
        float Temperature { get; set; }
        float Humidity { get; set; }
        float CO2Level { get; set; }
        float LightIntensity { get; set; }

        void SetTargetConditions(EnvironmentalConditions conditions);
        void UpdateEnvironment(float deltaTime);
        EnvironmentalConditions GetConditionsAtPosition(Vector3 position);
        void RegisterEnvironmentalZone(EnvironmentalZone zone);
        void UnregisterEnvironmentalZone(EnvironmentalZone zone);
    }

    /// <summary>
    /// Interface for Progression Manager - handles player progression, skills, and achievements
    /// </summary>
    public interface IProgressionManager : IChimeraManager
    {
        int PlayerLevel { get; }
        float CurrentExperience { get; }
        float ExperienceToNextLevel { get; }
        int SkillPoints { get; }
        int UnlockedAchievements { get; }

        void AddExperience(float amount, string source = null);
        void UnlockSkill(string skillId);
        void UnlockAchievement(string achievementId);
        bool IsSkillUnlocked(string skillId);
        bool IsAchievementUnlocked(string achievementId);
        IEnumerable<string> GetUnlockedSkills();
        IEnumerable<string> GetUnlockedAchievements();
    }

    /// <summary>
    /// Interface for Research Manager - handles research projects and technology unlocks
    /// </summary>
    public interface IResearchManager : IChimeraManager
    {
        IEnumerable<ResearchProjectSO> AvailableProjects { get; }
        IEnumerable<ResearchProjectSO> CompletedProjects { get; }
        ResearchProjectSO CurrentProject { get; }
        float CurrentProjectProgress { get; }

        void StartResearch(ResearchProjectSO project);
        void AddResearchProgress(float amount);
        void CompleteCurrentProject();
        bool IsProjectCompleted(ResearchProjectSO project);
        bool IsProjectAvailable(ResearchProjectSO project);
        IEnumerable<ResearchProjectSO> GetProjectsByCategory(ResearchCategory category);
    }

    /// <summary>
    /// Interface for Economy Manager - handles financial systems and market dynamics
    /// </summary>
    public interface IEconomyManager : IChimeraManager
    {
        float PlayerMoney { get; }
        float TotalRevenue { get; }
        float TotalExpenses { get; }
        float NetProfit { get; }

        bool CanAfford(float amount);
        void AddMoney(float amount, string source = null);
        void SpendMoney(float amount, string reason = null);
        void ProcessTransaction(float amount, string description);
        IEnumerable<Transaction> GetTransactionHistory(int count = 10);
        float GetMarketPrice(string itemId);
        void UpdateMarketPrices();
    }

    /// <summary>
    /// Interface for UI Manager - handles user interface coordination
    /// </summary>
    public interface IUIManager : IChimeraManager
    {
        bool IsUIOpen { get; }
        string CurrentPanel { get; }

        void ShowPanel(string panelId);
        void HidePanel(string panelId);
        void TogglePanel(string panelId);
        bool IsPanelOpen(string panelId);
        void ShowNotification(string message, float duration = 3f);
        void ShowDialog(string title, string message, System.Action onConfirm = null);
        void UpdateUI();
    }

    /// <summary>
    /// Interface for Audio Manager - handles music, sound effects, and audio settings
    /// </summary>
    public interface IAudioManager : IChimeraManager
    {
        float MasterVolume { get; set; }
        float MusicVolume { get; set; }
        float SFXVolume { get; set; }
        bool IsMuted { get; set; }

        void PlayMusic(string musicId, bool loop = true);
        void StopMusic();
        void PlaySFX(string sfxId, Vector3 position = default);
        void PlaySFX(string sfxId, float volume = 1f);
        void SetMasterVolume(float volume);
        void SetMusicVolume(float volume);
        void SetSFXVolume(float volume);
    }

    /// <summary>
    /// Base data structures used by manager interfaces
    /// </summary>

    public class Transaction
    {
        public DateTime Timestamp { get; set; }
        public float Amount { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public TransactionType Type { get; set; }
    }

    public enum TransactionType
    {
        Income,
        Expense,
        Transfer
    }

    public enum ResearchCategory
    {
        Genetics,
        Environment,
        Equipment,
        Processing,
        Business,
        Automation
    }

    public enum PlantGrowthStage
    {
        Seed,
        Germination,
        Seedling,
        Vegetative,
        PreFlower,
        Flowering,
        Maturation,
        Harvest,
        Drying,
        Curing
    }

    public class EnvironmentalZone
    {
        public string Id { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Size { get; set; }
        public EnvironmentalConditions Conditions { get; set; }
        public float Priority { get; set; }
    }
}