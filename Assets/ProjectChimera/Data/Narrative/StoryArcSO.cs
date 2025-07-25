using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Narrative
{
    [CreateAssetMenu(fileName = "New Story Arc", menuName = "Project Chimera/Narrative/Story Arc")]
    public class StoryArcSO : ChimeraDataSO
    {
        [Header("Arc Configuration")]
        [SerializeField] private string _arcId;
        [SerializeField] private string _storyId;
        [SerializeField] private string _arcName;
        [SerializeField] private string _description;
        
        [Header("Arc Properties")]
        [SerializeField] private StoryArcCategory _category = StoryArcCategory.NoviceGrower;
        [SerializeField] private int _priority = 1;
        [SerializeField] private float _difficultyLevel = 0.5f;
        [SerializeField] private bool _isMainStory = false;
        
        [Header("Arc Content")]
        [SerializeField] private List<string> _categoryId = new List<string>();
        [SerializeField] private List<string> _tagIds = new List<string>();
        [SerializeField] private List<string> _themeIds = new List<string>();
        
        [Header("Arc Data")]
        [SerializeField] private List<ConsequenceTemplate> _consequenceTemplates = new List<ConsequenceTemplate>();
        [SerializeField] private List<string> _characterIds = new List<string>();
        [SerializeField] private List<StoryBeatSO> _storyBeats = new List<StoryBeatSO>();
        
        // Properties
        public string ArcId => _arcId;
        public string StoryId => _storyId;
        public string ArcName => _arcName;
        public string Description => _description;
        public StoryArcCategory Category => _category;
        public int Priority => _priority;
        public float DifficultyLevel => _difficultyLevel;
        public string CategoryId => _categoryId.Count > 0 ? _categoryId[0] : "";
        public List<string> TagIds => _tagIds;
        public List<string> ThemeIds => _themeIds;
        public List<ConsequenceTemplate> ConsequenceTemplates => _consequenceTemplates;
        public List<StoryBeatSO> StoryBeats => _storyBeats;
        public string StoryName => _arcName;
        public List<StoryBeatSO> Chapters => _storyBeats;
        
        // Methods
        public bool IsMainStory() => _isMainStory;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (string.IsNullOrEmpty(_arcId))
            {
                _arcId = name.Replace(" ", "").Replace("Arc", "").ToLower();
            }
            
            if (string.IsNullOrEmpty(_storyId))
            {
                _storyId = _arcId;
            }
            
            if (string.IsNullOrEmpty(_arcName))
            {
                _arcName = name.Replace("Arc", "").Trim();
            }
            
            _difficultyLevel = Mathf.Clamp01(_difficultyLevel);
        }
    }
}