
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Data.Narrative
{
    [CreateAssetMenu(fileName = "New Dialogue Library", menuName = "Project Chimera/Narrative/Dialogue Library")]
    public class DialogueLibrarySO : ScriptableObject
    {
        public List<DialogueEntry> dialogues;

        public DialogueEntry GetDialogue(string dialogueId)
        {
            return dialogues.Find(d => d.dialogueID == dialogueId);
        }
    }

    [System.Serializable]
    public class DialogueEntry
    {
        public string dialogueID;
        public string DialogueText;
        public bool AllowsSkipping;
        [SerializeField] private float _displayDuration;
        public List<string> Tags = new List<string>();
        public string StartNodeId;
        public MemoryType MemoryType;
        public string DialogueChoice;
        public string SpeakerId;
        
        public float DisplayDuration => _displayDuration;
        
        // Method versions for compatibility 
        public bool AllowSkipping() { return AllowsSkipping; }
        public bool AllowSkipping(DialogueEntry entry) { return AllowsSkipping; }
        public float GetDisplayDuration() { return _displayDuration; }
        public float GetDisplayDurationForEntry(DialogueEntry entry) { return _displayDuration; }
        
        // Additional compatibility methods for method calls
        public float GetDisplayDurationMethod() { return _displayDuration; }
        public string GetDialogueText() { return DialogueText; }
    }
}
