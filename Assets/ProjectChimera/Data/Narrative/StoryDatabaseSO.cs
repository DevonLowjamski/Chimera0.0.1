using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Narrative
{
    /// <summary>
    /// Comprehensive story database ScriptableObject for Project Chimera's narrative system.
    /// Contains all story arcs, chapters, events, choices, and branching narrative content
    /// with cannabis cultivation integration and educational storytelling elements.
    /// </summary>
    [CreateAssetMenu(fileName = "New Story Database", menuName = "Project Chimera/Narrative/Story Database", order = 101)]
    public class StoryDatabaseSO : ChimeraDataSO
    {
        [Header("Story Collection")]
        [SerializeField] private List<StoryArcSO> _storyArcs = new List<StoryArcSO>();
        [SerializeField] private List<StoryChapterSO> _chapters = new List<StoryChapterSO>();
        [SerializeField] private List<StoryEventSO> _storyEvents = new List<StoryEventSO>();
        [SerializeField] private List<StoryChoiceSO> _choices = new List<StoryChoiceSO>();
        [SerializeField] private List<StoryBeatSO> _storyBeats = new List<StoryBeatSO>();
        
        [Header("Story Organization")]
        [SerializeField] private string _beginningStoryId;
        [SerializeField] private List<string> _mainStoryIds = new List<string>();
        [SerializeField] private List<string> _sideStoryIds = new List<string>();
        [SerializeField] private List<string> _educationalStoryIds = new List<string>();
        [SerializeField] private List<string> _cultivationStoryIds = new List<string>();
        
        [Header("Story Categories")]
        [SerializeField] private List<StoryCategorySO> _storyCategories = new List<StoryCategorySO>();
        [SerializeField] private List<StoryTagSO> _storyTags = new List<StoryTagSO>();
        [SerializeField] private List<StoryThemeSO> _storyThemes = new List<StoryThemeSO>();
        
        [Header("Progression and Dependencies")]
        [SerializeField] private Dictionary<string, List<string>> _storyDependencies = new Dictionary<string, List<string>>();
        [SerializeField] private Dictionary<string, List<string>> _unlockConditions = new Dictionary<string, List<string>>();
        [SerializeField] private Dictionary<string, float> _difficultyRatings = new Dictionary<string, float>();
        
        [Header("Integration Content")]
        [SerializeField] private List<CultivationStoryMapping> _cultivationMappings = new List<CultivationStoryMapping>();
        [SerializeField] private List<EducationalStoryMapping> _educationalMappings = new List<EducationalStoryMapping>();
        [SerializeField] private List<EventStoryMapping> _eventMappings = new List<EventStoryMapping>();
        
        [Header("Analytics and Metrics")]
        [SerializeField] private bool _enableStoryAnalytics = true;
        [SerializeField] private bool _trackPlayerChoices = true;
        [SerializeField] private bool _trackCompletionRates = true;
        [SerializeField] private bool _trackEngagementMetrics = true;
        
        // Runtime caches
        private Dictionary<string, StoryArcSO> _storyArcCache = new Dictionary<string, StoryArcSO>();
        private Dictionary<string, StoryChapterSO> _chapterCache = new Dictionary<string, StoryChapterSO>();
        private Dictionary<string, StoryEventSO> _eventCache = new Dictionary<string, StoryEventSO>();
        private Dictionary<string, StoryChoiceSO> _choiceCache = new Dictionary<string, StoryChoiceSO>();
        private Dictionary<string, StoryBeatSO> _beatCache = new Dictionary<string, StoryBeatSO>();
        
        // Properties
        public List<StoryArcSO> StoryArcs => _storyArcs;
        public List<StoryChapterSO> Chapters => _chapters;
        public List<StoryEventSO> StoryEvents => _storyEvents;
        public List<StoryChoiceSO> Choices => _choices;
        public List<StoryBeatSO> StoryBeats => _storyBeats;
        public List<StoryCategorySO> StoryCategories => _storyCategories;
        public List<StoryTagSO> StoryTags => _storyTags;
        public List<StoryThemeSO> StoryThemes => _storyThemes;
        public string BeginningStoryId => _beginningStoryId;
        public List<string> MainStoryIds => _mainStoryIds;
        public List<string> SideStoryIds => _sideStoryIds;
        public List<string> EducationalStoryIds => _educationalStoryIds;
        public List<string> CultivationStoryIds => _cultivationStoryIds;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Rebuild caches after validation
            RebuildCaches();
            
            // Validate story organization
            ValidateStoryOrganization();
            
            // Validate dependencies
            ValidateStoryDependencies();
        }
        
        private void OnEnable()
        {
            RebuildCaches();
        }
        
        #region Cache Management
        
        private void RebuildCaches()
        {
            // Clear existing caches
            _storyArcCache.Clear();
            _chapterCache.Clear();
            _eventCache.Clear();
            _choiceCache.Clear();
            _beatCache.Clear();
            
            // Rebuild story arc cache
            foreach (var storyArc in _storyArcs)
            {
                if (storyArc != null && !string.IsNullOrEmpty(storyArc.StoryId))
                {
                    _storyArcCache[storyArc.StoryId] = storyArc;
                }
            }
            
            // Rebuild chapter cache
            foreach (var chapter in _chapters)
            {
                if (chapter != null && !string.IsNullOrEmpty(chapter.ChapterId))
                {
                    _chapterCache[chapter.ChapterId] = chapter;
                }
            }
            
            // Rebuild event cache
            foreach (var storyEvent in _storyEvents)
            {
                if (storyEvent != null && !string.IsNullOrEmpty(storyEvent.EventId))
                {
                    _eventCache[storyEvent.EventId] = storyEvent;
                }
            }
            
            // Rebuild choice cache
            foreach (var choice in _choices)
            {
                if (choice != null && !string.IsNullOrEmpty(choice.ChoiceId))
                {
                    _choiceCache[choice.ChoiceId] = choice;
                }
            }
            
            // Rebuild beat cache
            foreach (var beat in _storyBeats)
            {
                if (beat != null && !string.IsNullOrEmpty(beat.BeatId))
                {
                    _beatCache[beat.BeatId] = beat;
                }
            }
        }
        
        #endregion
        
        #region Story Retrieval
        
        public StoryArcSO GetStoryArc(string storyId)
        {
            if (string.IsNullOrEmpty(storyId))
                return null;
            
            return _storyArcCache.GetValueOrDefault(storyId);
        }
        
        public StoryArcSO GetBeginningStory()
        {
            return GetStoryArc(_beginningStoryId);
        }
        
        public List<StoryArcSO> GetMainStories()
        {
            return _mainStoryIds.Select(GetStoryArc).Where(story => story != null).ToList();
        }
        
        public List<StoryArcSO> GetSideStories()
        {
            return _sideStoryIds.Select(GetStoryArc).Where(story => story != null).ToList();
        }
        
        public List<StoryArcSO> GetEducationalStories()
        {
            return _educationalStoryIds.Select(GetStoryArc).Where(story => story != null).ToList();
        }
        
        public List<StoryArcSO> GetCultivationStories()
        {
            return _cultivationStoryIds.Select(GetStoryArc).Where(story => story != null).ToList();
        }
        
        public StoryChapterSO GetChapter(string chapterId)
        {
            if (string.IsNullOrEmpty(chapterId))
                return null;
            
            return _chapterCache.GetValueOrDefault(chapterId);
        }
        
        public StoryEventSO GetStoryEvent(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
                return null;
            
            return _eventCache.GetValueOrDefault(eventId);
        }
        
        public StoryChoiceSO GetChoice(string choiceId)
        {
            if (string.IsNullOrEmpty(choiceId))
                return null;
            
            return _choiceCache.GetValueOrDefault(choiceId);
        }
        
        public StoryBeatSO GetStoryBeat(string beatId)
        {
            if (string.IsNullOrEmpty(beatId))
                return null;
            
            return _beatCache.GetValueOrDefault(beatId);
        }
        
        #endregion
        
        #region Story Querying and Filtering
        
        public List<StoryArcSO> GetStoriesByCategory(string categoryId)
        {
            return _storyArcs.Where(story => story != null && story.CategoryId == categoryId).ToList();
        }
        
        public List<StoryArcSO> GetStoriesByTag(string tagId)
        {
            return _storyArcs.Where(story => story != null && story.TagIds.Contains(tagId)).ToList();
        }
        
        public List<StoryArcSO> GetStoriesByTheme(string themeId)
        {
            return _storyArcs.Where(story => story != null && story.ThemeIds.Contains(themeId)).ToList();
        }
        
        public List<StoryArcSO> GetStoriesByDifficulty(float minDifficulty, float maxDifficulty)
        {
            return _storyArcs.Where(story => 
            {
                if (story == null) return false;
                var difficulty = _difficultyRatings.GetValueOrDefault(story.StoryId, 0.5f);
                return difficulty >= minDifficulty && difficulty <= maxDifficulty;
            }).ToList();
        }
        
        public List<StoryArcSO> GetAvailableStories(List<string> completedStoryIds)
        {
            var availableStories = new List<StoryArcSO>();
            
            foreach (var story in _storyArcs)
            {
                if (story == null) continue;
                
                if (IsStoryAvailable(story.StoryId, completedStoryIds))
                {
                    availableStories.Add(story);
                }
            }
            
            return availableStories;
        }
        
        public bool IsStoryAvailable(string storyId, List<string> completedStoryIds)
        {
            if (string.IsNullOrEmpty(storyId) || completedStoryIds.Contains(storyId))
                return false;
            
            // Check dependencies
            if (_storyDependencies.TryGetValue(storyId, out var dependencies))
            {
                foreach (var dependency in dependencies)
                {
                    if (!completedStoryIds.Contains(dependency))
                        return false;
                }
            }
            
            return true;
        }
        
        #endregion
        
        #region Integration Mappings
        
        public List<StoryArcSO> GetStoriesForCultivationActivity(string activityType)
        {
            var mappings = _cultivationMappings.Where(m => m.CultivationActivity == activityType);
            return mappings.SelectMany(m => m.StoryIds).Select(GetStoryArc).Where(s => s != null).ToList();
        }
        
        public List<StoryArcSO> GetStoriesForEducationalTopic(string topicId)
        {
            var mappings = _educationalMappings.Where(m => m.EducationalTopic == topicId);
            return mappings.SelectMany(m => m.StoryIds).Select(GetStoryArc).Where(s => s != null).ToList();
        }
        
        public List<StoryArcSO> GetStoriesForEvent(string eventType)
        {
            var mappings = _eventMappings.Where(m => m.EventType == eventType);
            return mappings.SelectMany(m => m.StoryIds).Select(GetStoryArc).Where(s => s != null).ToList();
        }
        
        #endregion
        
        #region Story Management
        
        public void AddStoryArc(StoryArcSO storyArc)
        {
            if (storyArc == null || _storyArcs.Contains(storyArc))
                return;
            
            _storyArcs.Add(storyArc);
            _storyArcCache[storyArc.StoryId] = storyArc;
        }
        
        public void RemoveStoryArc(string storyId)
        {
            var storyArc = GetStoryArc(storyId);
            if (storyArc != null)
            {
                _storyArcs.Remove(storyArc);
                _storyArcCache.Remove(storyId);
            }
        }
        
        public void SetBeginningStory(string storyId)
        {
            if (GetStoryArc(storyId) != null)
            {
                _beginningStoryId = storyId;
            }
        }
        
        public void AddToMainStories(string storyId)
        {
            if (GetStoryArc(storyId) != null && !_mainStoryIds.Contains(storyId))
            {
                _mainStoryIds.Add(storyId);
            }
        }
        
        public void AddToSideStories(string storyId)
        {
            if (GetStoryArc(storyId) != null && !_sideStoryIds.Contains(storyId))
            {
                _sideStoryIds.Add(storyId);
            }
        }
        
        #endregion
        
        #region Validation
        
        private void ValidateStoryOrganization()
        {
            // Validate beginning story exists
            if (!string.IsNullOrEmpty(_beginningStoryId) && GetStoryArc(_beginningStoryId) == null)
            {
                Debug.LogWarning($"Beginning story '{_beginningStoryId}' not found in story database");
                _beginningStoryId = string.Empty;
            }
            
            // Validate main stories exist
            _mainStoryIds.RemoveAll(id => GetStoryArc(id) == null);
            
            // Validate side stories exist
            _sideStoryIds.RemoveAll(id => GetStoryArc(id) == null);
            
            // Validate educational stories exist
            _educationalStoryIds.RemoveAll(id => GetStoryArc(id) == null);
            
            // Validate cultivation stories exist
            _cultivationStoryIds.RemoveAll(id => GetStoryArc(id) == null);
        }
        
        private void ValidateStoryDependencies()
        {
            var validStoryIds = _storyArcs.Where(s => s != null).Select(s => s.StoryId).ToHashSet();
            
            // Remove invalid dependencies
            var keysToRemove = new List<string>();
            foreach (var kvp in _storyDependencies)
            {
                if (!validStoryIds.Contains(kvp.Key))
                {
                    keysToRemove.Add(kvp.Key);
                    continue;
                }
                
                // Remove invalid dependency references
                kvp.Value.RemoveAll(dep => !validStoryIds.Contains(dep));
            }
            
            // Remove entries with invalid keys
            foreach (var key in keysToRemove)
            {
                _storyDependencies.Remove(key);
            }
        }
        
        #endregion
        
        #region Statistics and Analytics
        
        public StoryDatabaseStatistics GetStatistics()
        {
            return new StoryDatabaseStatistics
            {
                TotalStoryArcs = _storyArcs.Count,
                TotalChapters = _chapters.Count,
                TotalEvents = _storyEvents.Count,
                TotalChoices = _choices.Count,
                TotalBeats = _storyBeats.Count,
                MainStories = _mainStoryIds.Count,
                SideStories = _sideStoryIds.Count,
                EducationalStories = _educationalStoryIds.Count,
                CultivationStories = _cultivationStoryIds.Count,
                Categories = _storyCategories.Count,
                Tags = _storyTags.Count,
                Themes = _storyThemes.Count
            };
        }
        
        #endregion
    }
    
    // Supporting data structures
    [Serializable]
    public class CultivationStoryMapping
    {
        public string CultivationActivity;
        public List<string> StoryIds = new List<string>();
        public float TriggerThreshold = 1.0f;
        public bool IsRepeatable = false;
    }
    
    [Serializable]
    public class EducationalStoryMapping
    {
        public string EducationalTopic;
        public List<string> StoryIds = new List<string>();
        public string SkillLevel = "beginner";
        public bool RequiresCompletion = false;
    }
    
    [Serializable]
    public class EventStoryMapping
    {
        public string EventType;
        public List<string> StoryIds = new List<string>();
        public string EventTrigger = "start";
        public bool IsOptional = true;
    }
    
    [Serializable]
    public class StoryDatabaseStatistics
    {
        public int TotalStoryArcs;
        public int TotalChapters;
        public int TotalEvents;
        public int TotalChoices;
        public int TotalBeats;
        public int MainStories;
        public int SideStories;
        public int EducationalStories;
        public int CultivationStories;
        public int Categories;
        public int Tags;
        public int Themes;
    }
    
    // Placeholder classes for compilation
    // NOTE: StoryArcSO and related classes moved to separate files to avoid duplicate definitions
    // See StoryArcSO.cs, StoryChapterSO.cs, etc.
}