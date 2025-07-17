using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Narrative
{
    [CreateAssetMenu(fileName = "New Story Choice", menuName = "Project Chimera/Narrative/Story Choice")]
    public class StoryChoiceSO : ChimeraDataSO
    {
        [Header("Choice Configuration")]
        [SerializeField] private string _choiceId;
        [SerializeField] private string _choiceText;
        [SerializeField] private string _description;
        
        [Header("Choice Options")]
        [SerializeField] private List<ChoiceOption> _options = new List<ChoiceOption>();
        [SerializeField] private List<Consequence> _consequences = new List<Consequence>();
        [SerializeField] private Dictionary<int, List<string>> _consequencesByOption = new Dictionary<int, List<string>>();
        
        [Header("Availability Conditions")]
        [SerializeField] private List<string> _requiredFlags = new List<string>();
        [SerializeField] private List<string> _excludedFlags = new List<string>();
        [SerializeField] private bool _isRepeatable = true;
        
        // Properties
        public string ChoiceId => _choiceId;
        public string ChoiceText => _choiceText;
        public string Description => _description;
        public List<ChoiceOption> Options => _options;
        public List<Consequence> Consequences => _consequences;
        public Dictionary<int, List<string>> ConsequencesByOption => _consequencesByOption ?? (_consequencesByOption = new Dictionary<int, List<string>>());
        public List<string> RequiredFlags => _requiredFlags;
        public List<string> ExcludedFlags => _excludedFlags;
        public bool IsRepeatable => _isRepeatable;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (string.IsNullOrEmpty(_choiceId))
            {
                _choiceId = name.Replace(" ", "").Replace("Choice", "").ToLower();
            }
        }
        
        /// <summary>
        /// Check if this choice is available based on player flags
        /// </summary>
        public bool IsAvailable(List<string> playerFlags)
        {
            if (playerFlags == null) playerFlags = new List<string>();
            
            // Check required flags
            foreach (var requiredFlag in _requiredFlags)
            {
                if (!playerFlags.Contains(requiredFlag))
                    return false;
            }
            
            // Check excluded flags
            foreach (var excludedFlag in _excludedFlags)
            {
                if (playerFlags.Contains(excludedFlag))
                    return false;
            }
            
            return true;
        }
    }
}