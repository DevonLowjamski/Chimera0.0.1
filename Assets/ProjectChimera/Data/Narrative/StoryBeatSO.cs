using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Narrative
{
    [CreateAssetMenu(fileName = "New Story Beat", menuName = "Project Chimera/Narrative/Story Beat")]
    public class StoryBeatSO : ChimeraDataSO
    {
        [Header("Beat Configuration")]
        [SerializeField] private string _beatId;
        [SerializeField] private string _beatName;
        [SerializeField] private string _description;
        
        [Header("Beat Properties")]
        [SerializeField] private List<string> _requirements = new List<string>();
        [SerializeField] private List<CharacterProfileSO> _involvedCharacters = new List<CharacterProfileSO>();
        [SerializeField] private float _importance = 1.0f;
        
        [Header("Beat Content")]
        [SerializeField] private List<string> _eventIds = new List<string>();
        [SerializeField] private List<string> _choiceIds = new List<string>();
        [SerializeField] private List<string> _tags = new List<string>();
        [SerializeField] private List<StoryEventSO> _chapterEvents = new List<StoryEventSO>();
        [SerializeField] private List<string> _introducedCharacters = new List<string>();
        [SerializeField] private List<string> _availableQuests = new List<string>();
        
        // Properties
        public string BeatId => _beatId;
        public string BeatName => _beatName;
        public string Description => _description;
        public List<string> Requirements => _requirements;
        public List<CharacterProfileSO> InvolvedCharacters => _involvedCharacters;
        public float Importance => _importance;
        public List<string> EventIds => _eventIds;
        public List<string> ChoiceIds => _choiceIds;
        public List<string> Tags => _tags;
        public List<StoryEventSO> ChapterEvents => _chapterEvents;
        public List<string> IntroducedCharacters => _introducedCharacters;
        public List<string> AvailableQuests => _availableQuests;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (string.IsNullOrEmpty(_beatId))
            {
                _beatId = name.Replace(" ", "").Replace("Beat", "").ToLower();
            }
            
            if (string.IsNullOrEmpty(_beatName))
            {
                _beatName = name.Replace("Beat", "").Trim();
            }
            
            _importance = Mathf.Clamp01(_importance);
        }
    }
}
