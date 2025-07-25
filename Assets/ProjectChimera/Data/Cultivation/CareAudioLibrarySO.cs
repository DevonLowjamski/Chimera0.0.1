using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Cultivation
{
    /// <summary>
    /// Library of audio clips for plant care actions and feedback
    /// </summary>
    [CreateAssetMenu(fileName = "CareAudioLibrary", menuName = "Project Chimera/Cultivation/Care Audio Library")]
    public class CareAudioLibrarySO : ChimeraScriptableObject
    {
        [Header("Care Action Sounds")]
        [SerializeField] private List<CareAudioClip> _careActionSounds = new List<CareAudioClip>();
        
        [Header("Plant Response Sounds")]
        [SerializeField] private List<CareAudioClip> _plantResponseSounds = new List<CareAudioClip>();
        
        [Header("Ambient Sounds")]
        [SerializeField] private List<CareAudioClip> _ambientSounds = new List<CareAudioClip>();
        
        public List<CareAudioClip> CareActionSounds => _careActionSounds;
        public List<CareAudioClip> PlantResponseSounds => _plantResponseSounds;
        public List<CareAudioClip> AmbientSounds => _ambientSounds;
        
        public AudioClip GetCareActionSound(CultivationTaskType taskType)
        {
            var audioClip = _careActionSounds.Find(c => c.TaskType == taskType);
            return audioClip?.AudioClip;
        }
        
        public AudioClip GetPlantResponseSound(PlantResponseType responseType)
        {
            var audioClip = _plantResponseSounds.Find(c => c.ResponseType == responseType);
            return audioClip?.AudioClip;
        }
        
        public AudioClip GetRandomAmbientSound()
        {
            if (_ambientSounds.Count == 0) return null;
            int randomIndex = Random.Range(0, _ambientSounds.Count);
            return _ambientSounds[randomIndex].AudioClip;
        }
        
        public AudioClip GetCareAudioClip(CultivationTaskType taskType, float quality)
        {
            // Get care audio clip based on task type and quality
            var audioClip = _careActionSounds.Find(c => c.TaskType == taskType);
            return audioClip?.AudioClip;
        }
        
        public AudioClip GetFailureAudioClip(CultivationTaskType taskType)
        {
            // Get failure audio clip for task type
            var failureClip = _careActionSounds.Find(c => c.TaskType == taskType && c.IsFailureSound);
            return failureClip?.AudioClip;
        }
    }
    
    /// <summary>
    /// Audio clip data for care actions
    /// </summary>
    [System.Serializable]
    public class CareAudioClip
    {
        public string ClipName;
        public AudioClip AudioClip;
        public CultivationTaskType TaskType;
        public PlantResponseType ResponseType;
        [Range(0f, 1f)] public float Volume = 1f;
        [Range(0.1f, 3f)] public float Pitch = 1f;
        public bool Loop = false;
        public bool IsFailureSound = false;
    }
    
    /// <summary>
    /// Types of plant responses for audio feedback
    /// </summary>
    public enum PlantResponseType
    {
        Healthy,
        Stressed,
        Growing,
        Flowering,
        Wilting,
        Recovering,
        Thriving
    }
}