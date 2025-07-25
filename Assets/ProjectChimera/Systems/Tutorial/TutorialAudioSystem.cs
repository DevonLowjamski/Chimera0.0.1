using UnityEngine;
using ProjectChimera.Data.Tutorial;

namespace ProjectChimera.Systems.Tutorial
{
    /// <summary>
    /// Tutorial audio system for Project Chimera.
    /// Manages narration, sound effects, and audio feedback for tutorials.
    /// </summary>
    public class TutorialAudioSystem
    {
        private TutorialSettings _settings;
        private TutorialConfigurationSO _configuration;
        private AudioSource _narrationSource;
        private AudioSource _soundEffectSource;
        private bool _isInitialized;
        
        // Current audio state
        private AudioClip _currentNarration;
        private bool _isPlayingNarration;
        private float _narrationStartTime;
        
        // Properties
        public bool IsInitialized => _isInitialized;
        public bool IsPlayingNarration => _isPlayingNarration;
        public float NarrationProgress => GetNarrationProgress();
        
        public TutorialAudioSystem(TutorialSettings settings, TutorialConfigurationSO configuration)
        {
            _settings = settings;
            _configuration = configuration;
            
            InitializeAudioSystem();
        }
        
        /// <summary>
        /// Initialize audio system
        /// </summary>
        private void InitializeAudioSystem()
        {
            if (!_settings.EnableNarration && !_settings.EnableSoundEffects)
            {
                _isInitialized = true;
                return;
            }
            
            CreateAudioSources();
            
            _isInitialized = true;
            Debug.Log("Tutorial audio system initialized");
        }
        
        /// <summary>
        /// Create audio sources
        /// </summary>
        private void CreateAudioSources()
        {
            // Create narration audio source
            if (_settings.EnableNarration)
            {
                var narrationGO = new GameObject("TutorialNarrationAudio");
                _narrationSource = narrationGO.AddComponent<AudioSource>();
                _narrationSource.volume = _settings.NarrationVolume;
                _narrationSource.playOnAwake = false;
                _narrationSource.loop = false;
                
                Object.DontDestroyOnLoad(narrationGO);
            }
            
            // Create sound effect audio source
            if (_settings.EnableSoundEffects)
            {
                var soundEffectGO = new GameObject("TutorialSoundEffectAudio");
                _soundEffectSource = soundEffectGO.AddComponent<AudioSource>();
                _soundEffectSource.volume = _configuration.SoundEffectVolume;
                _soundEffectSource.playOnAwake = false;
                _soundEffectSource.loop = false;
                
                Object.DontDestroyOnLoad(soundEffectGO);
            }
        }
        
        /// <summary>
        /// Play step audio (narration)
        /// </summary>
        public void PlayStepAudio(TutorialStepSO step)
        {
            if (!_isInitialized || step == null)
                return;
            
            if (_settings.EnableNarration && step.NarrationClip != null)
            {
                PlayNarration(step.NarrationClip);
            }
        }
        
        /// <summary>
        /// Play narration clip
        /// </summary>
        public void PlayNarration(AudioClip clip)
        {
            if (!_settings.EnableNarration || _narrationSource == null || clip == null)
                return;
            
            // Stop current narration if playing
            StopNarration();
            
            _currentNarration = clip;
            _narrationSource.clip = clip;
            _narrationSource.volume = _settings.NarrationVolume;
            _narrationSource.Play();
            
            _isPlayingNarration = true;
            _narrationStartTime = Time.time;
            
            Debug.Log($"Playing tutorial narration: {clip.name}");
        }
        
        /// <summary>
        /// Stop current narration
        /// </summary>
        public void StopNarration()
        {
            if (_narrationSource != null && _isPlayingNarration)
            {
                _narrationSource.Stop();
                _isPlayingNarration = false;
                _currentNarration = null;
                
                Debug.Log("Stopped tutorial narration");
            }
        }
        
        /// <summary>
        /// Pause current narration
        /// </summary>
        public void PauseNarration()
        {
            if (_narrationSource != null && _isPlayingNarration)
            {
                _narrationSource.Pause();
                Debug.Log("Paused tutorial narration");
            }
        }
        
        /// <summary>
        /// Resume paused narration
        /// </summary>
        public void ResumeNarration()
        {
            if (_narrationSource != null && _isPlayingNarration)
            {
                _narrationSource.UnPause();
                Debug.Log("Resumed tutorial narration");
            }
        }
        
        /// <summary>
        /// Play step completed sound
        /// </summary>
        public void PlayStepCompletedSound()
        {
            if (_configuration?.StepCompletedSound != null)
            {
                PlaySoundEffect(_configuration.StepCompletedSound);
            }
        }
        
        /// <summary>
        /// Play sequence completed sound
        /// </summary>
        public void PlaySequenceCompletedSound()
        {
            if (_configuration?.SequenceCompletedSound != null)
            {
                PlaySoundEffect(_configuration.SequenceCompletedSound);
            }
        }
        
        /// <summary>
        /// Play error sound
        /// </summary>
        public void PlayErrorSound()
        {
            if (_configuration?.ErrorSound != null)
            {
                PlaySoundEffect(_configuration.ErrorSound);
            }
        }
        
        /// <summary>
        /// Play sound effect
        /// </summary>
        public void PlaySoundEffect(AudioClip clip)
        {
            if (!_settings.EnableSoundEffects || _soundEffectSource == null || clip == null)
                return;
            
            _soundEffectSource.volume = _configuration.SoundEffectVolume;
            _soundEffectSource.PlayOneShot(clip);
            
            Debug.Log($"Playing tutorial sound effect: {clip.name}");
        }
        
        /// <summary>
        /// Set narration volume
        /// </summary>
        public void SetNarrationVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            
            if (_narrationSource != null)
            {
                _narrationSource.volume = volume;
            }
            
            Debug.Log($"Set tutorial narration volume to: {volume}");
        }
        
        /// <summary>
        /// Set sound effect volume
        /// </summary>
        public void SetSoundEffectVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            
            if (_soundEffectSource != null)
            {
                _soundEffectSource.volume = volume;
            }
            
            Debug.Log($"Set tutorial sound effect volume to: {volume}");
        }
        
        /// <summary>
        /// Get narration progress (0-1)
        /// </summary>
        private float GetNarrationProgress()
        {
            if (!_isPlayingNarration || _currentNarration == null)
                return 0f;
            
            if (_narrationSource != null && _narrationSource.isPlaying)
            {
                return _narrationSource.time / _currentNarration.length;
            }
            
            return 1f; // Completed
        }
        
        /// <summary>
        /// Update audio system
        /// </summary>
        public void UpdateAudio()
        {
            if (!_isInitialized)
                return;
            
            // Check if narration has finished
            if (_isPlayingNarration && _narrationSource != null && !_narrationSource.isPlaying)
            {
                _isPlayingNarration = false;
                _currentNarration = null;
                Debug.Log("Tutorial narration completed");
            }
        }
        
        /// <summary>
        /// Mute all tutorial audio
        /// </summary>
        public void MuteAll()
        {
            if (_narrationSource != null)
            {
                _narrationSource.mute = true;
            }
            
            if (_soundEffectSource != null)
            {
                _soundEffectSource.mute = true;
            }
            
            Debug.Log("Muted all tutorial audio");
        }
        
        /// <summary>
        /// Unmute all tutorial audio
        /// </summary>
        public void UnmuteAll()
        {
            if (_narrationSource != null)
            {
                _narrationSource.mute = false;
            }
            
            if (_soundEffectSource != null)
            {
                _soundEffectSource.mute = false;
            }
            
            Debug.Log("Unmuted all tutorial audio");
        }
        
        /// <summary>
        /// Get audio system status
        /// </summary>
        public TutorialAudioStatus GetAudioStatus()
        {
            return new TutorialAudioStatus
            {
                IsInitialized = _isInitialized,
                NarrationEnabled = _settings.EnableNarration,
                SoundEffectsEnabled = _settings.EnableSoundEffects,
                IsPlayingNarration = _isPlayingNarration,
                NarrationProgress = GetNarrationProgress(),
                NarrationVolume = _narrationSource?.volume ?? 0f,
                SoundEffectVolume = _soundEffectSource?.volume ?? 0f,
                IsMuted = (_narrationSource?.mute ?? false) || (_soundEffectSource?.mute ?? false)
            };
        }
        
        /// <summary>
        /// Cleanup audio system
        /// </summary>
        public void Cleanup()
        {
            StopNarration();
            
            if (_narrationSource != null)
            {
                Object.Destroy(_narrationSource.gameObject);
                _narrationSource = null;
            }
            
            if (_soundEffectSource != null)
            {
                Object.Destroy(_soundEffectSource.gameObject);
                _soundEffectSource = null;
            }
            
            _isInitialized = false;
            Debug.Log("Tutorial audio system cleaned up");
        }
    }
    
    /// <summary>
    /// Tutorial audio system status
    /// </summary>
    public struct TutorialAudioStatus
    {
        public bool IsInitialized;
        public bool NarrationEnabled;
        public bool SoundEffectsEnabled;
        public bool IsPlayingNarration;
        public float NarrationProgress;
        public float NarrationVolume;
        public float SoundEffectVolume;
        public bool IsMuted;
    }
}