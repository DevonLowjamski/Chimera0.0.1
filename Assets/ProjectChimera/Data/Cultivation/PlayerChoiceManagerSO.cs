using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Cultivation
{
    /// <summary>
    /// Manages player choice configurations and effects
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerChoiceManager", menuName = "Project Chimera/Cultivation/Player Choice Manager")]
    public class PlayerChoiceManagerSO : ChimeraScriptableObject
    {
        [Header("Choice Configuration")]
        [SerializeField] private List<PlayerChoice> _availableChoices = new List<PlayerChoice>();
        
        public List<PlayerChoice> AvailableChoices => _availableChoices;
        
        public PlayerChoice GetChoice(string choiceId)
        {
            return _availableChoices.Find(c => c.ChoiceId == choiceId);
        }
    }
    
    /// <summary>
    /// Player choice data structure
    /// </summary>
    [System.Serializable]
    public class PlayerChoice
    {
        public string ChoiceId;
        public string ChoiceName;
        public string Description;
        public bool IsUnlocked = false;
        public bool IsSelected = false;
        public Dictionary<string, object> ChoiceParameters = new Dictionary<string, object>();
        
        public PlayerChoice()
        {
            ChoiceId = System.Guid.NewGuid().ToString();
        }
    }
}