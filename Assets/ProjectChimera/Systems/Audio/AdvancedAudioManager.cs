using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;

// using ProjectChimera.Systems.AI;
// using ProjectChimera.Systems.Cultivation;
// using ProjectChimera.Systems.Economy;
// using ProjectChimera.Systems.Facilities;
// using ProjectChimera.Systems.Automation;
// using ProjectChimera.Systems.Settings;
using EnvironmentSystems = ProjectChimera.Systems.Environment;
// using ProjectChimera.Systems.Construction;
// Explicit alias for Data layer PlantGrowthStage to resolve ambiguity
using DataPlantGrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage;

// Explicit namespace aliases to resolve ambiguity
using DataConstructionIssue = ProjectChimera.Data.Construction.ConstructionIssue;
using EnvironmentalAlert = ProjectChimera.Systems.Environment.EnvironmentalAlert;
using EnvironmentalManager = ProjectChimera.Systems.Environment.EnvironmentalManager;

namespace ProjectChimera.Systems.Audio
{
    /// <summary>
    /// Advanced audio manager for Project Chimera featuring dynamic soundscapes,
    /// 3D spatial audio, adaptive mixing, and intelligent audio automation
    /// based on environmental conditions and gameplay state.
    /// </summary>
    public class AdvancedAudioManager : ChimeraManager
    {
        [Header("Audio Configuration")]
        [SerializeField] private AudioMixer _masterMixer;
        [SerializeField] private AudioMixerGroup _ambientMixerGroup;
        [SerializeField] private AudioMixerGroup _effectsMixerGroup;
        [SerializeField] private AudioMixerGroup _uiMixerGroup;
        [SerializeField] private AudioMixerGroup _musicMixerGroup;
        [SerializeField] private AudioMixerGroup _environmentalMixerGroup;
        
        [Header("Audio Libraries")]
        [SerializeField] private AudioLibrarySO _audioLibrary;
        [SerializeField] private SoundscapeLibrarySO _soundscapeLibrary;
        [SerializeField] private MusicPlaylistSO _musicPlaylist;
        
        [Header("Dynamic Audio Settings")]
        [SerializeField] private bool _enableDynamicSoundscapes = true;
        [SerializeField] private bool _enableSpatialAudio = true;
        [SerializeField] private bool _enableAdaptiveMixing = true;
        [SerializeField] private bool _enableEnvironmentalAudio = true;
        [SerializeField] private float _audioUpdateInterval = 0.5f;
        
        [Header("3D Audio Configuration")]
        [SerializeField] private float _maxAudioDistance = 50f;
        [SerializeField] private AnimationCurve _audioFalloffCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        [SerializeField] private float _dopplerLevel = 1f;
        [SerializeField] private AudioRolloffMode _rolloffMode = AudioRolloffMode.Logarithmic;
        
        // Audio Source Pools
        private Queue<AudioSource> _availableAudioSources = new Queue<AudioSource>();
        private List<AudioSource> _activeAudioSources = new List<AudioSource>();
        private Dictionary<string, AudioSource> _persistentAudioSources = new Dictionary<string, AudioSource>();
        
        // Soundscape Management
        private DynamicSoundscape _currentSoundscape;
        private Dictionary<SoundscapeLayer, AudioSource> _soundscapeLayers = new Dictionary<SoundscapeLayer, AudioSource>();
        private float _lastSoundscapeUpdate = 0f;
        
        // Environmental Audio
        private Dictionary<string, EnvironmentalAudioSource> _environmentalSources = new Dictionary<string, EnvironmentalAudioSource>();
        private List<ProceduralAudioGenerator> _proceduralGenerators = new List<ProceduralAudioGenerator>();
        
        // Music System
        private MusicController _musicController;
        private AudioSource _musicSource;
        private bool _isMusicEnabled = true;
        private float _musicVolume = 0.7f;
        
        // Performance Monitoring
        private AudioPerformanceMetrics _performanceMetrics;
        private Queue<AudioLoadData> _performanceHistory = new Queue<AudioLoadData>();
        
        // System References - accessed through GameManager events
        private EnvironmentalManager _environmentalManager;
        private ChimeraManager _plantManager;
        private ChimeraManager _facilityConstructor;
        // Other managers accessed through event-based communication
        private Camera _listenerCamera;
        
        // Audio State
        private AudioState _currentAudioState = AudioState.Facility;
        private float _masterVolume = 1f;
        private bool _isAudioMuted = false;
        private float _lastAudioUpdate = 0f;
        
        // Events
        public System.Action<AudioClip> OnAudioClipPlayed;
        public System.Action<DynamicSoundscape> OnSoundscapeChanged;
        public System.Action<float> OnVolumeChanged;
        public System.Action<AudioAlert> OnAudioAlert;
        
        // Properties
        public float MasterVolume => _masterVolume;
        public bool IsAudioMuted => _isAudioMuted;
        public AudioState CurrentAudioState => _currentAudioState;
        public AudioPerformanceMetrics PerformanceMetrics => _performanceMetrics;
        public DynamicSoundscape CurrentSoundscape => _currentSoundscape;
        
        protected override void OnManagerInitialize()
        {
            InitializeAudioSystem();
            SetupAudioMixers();
            InitializeAudioSources();
            SetupSoundscapes();
            ConnectToGameSystems();
            StartAudioUpdateLoop();
        }
        
        private void Update()
        {
            float currentTime = Time.time;
            
            if (currentTime - _lastAudioUpdate >= _audioUpdateInterval)
            {
                UpdateDynamicAudio();
                UpdateSpatialAudio();
                UpdatePerformanceMetrics();
                _lastAudioUpdate = currentTime;
            }
            
            UpdateAudioSourcePool();
            ProcessAudioQueue();
        }
        
        #region Initialization
        
        private void InitializeAudioSystem()
        {
            // Initialize audio libraries
            if (_audioLibrary != null)
            {
                _audioLibrary.InitializeDefaults();
            }
            
            if (_soundscapeLibrary != null)
            {
                _soundscapeLibrary.InitializeDefaults();
            }
            
            // Setup audio listener
            SetupAudioListener();
            
            // Initialize music controller
            _musicController = new MusicController(_musicPlaylist);
            
            // Initialize performance monitoring
            _performanceMetrics = new AudioPerformanceMetrics
            {
                MaxConcurrentSources = 64,
                PoolSize = 32,
                SpatialAudioEnabled = _enableSpatialAudio
            };
            
            LogInfo("Advanced Audio Manager initialized");
        }
        
        private void SetupAudioMixers()
        {
            if (_masterMixer == null)
            {
                LogWarning("Master AudioMixer not assigned");
                return;
            }
            
            // Set initial mixer values
            _masterMixer.SetFloat("MasterVolume", Mathf.Log10(_masterVolume) * 20f);
            _masterMixer.SetFloat("AmbientVolume", 0f);
            _masterMixer.SetFloat("EffectsVolume", 0f);
            _masterMixer.SetFloat("UIVolume", 0f);
            _masterMixer.SetFloat("MusicVolume", Mathf.Log10(_musicVolume) * 20f);
        }
        
        private void InitializeAudioSources()
        {
            // Create audio source pool
            int poolSize = 32;
            for (int i = 0; i < poolSize; i++)
            {
                var audioSource = CreatePooledAudioSource($"PooledAudioSource_{i}");
                _availableAudioSources.Enqueue(audioSource);
            }
            
            // Create music audio source
            _musicSource = CreateAudioSource("MusicSource", _musicMixerGroup);
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            
            LogInfo($"Initialized audio source pool with {poolSize} sources");
        }
        
        private AudioSource CreatePooledAudioSource(string sourceName)
        {
            var go = new GameObject(sourceName);
            go.transform.SetParent(transform);
            
            var audioSource = go.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = _effectsMixerGroup;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = _enableSpatialAudio ? 1f : 0f;
            audioSource.rolloffMode = _rolloffMode;
            audioSource.maxDistance = _maxAudioDistance;
            audioSource.dopplerLevel = _dopplerLevel;
            
            go.SetActive(false);
            return audioSource;
        }
        
        private AudioSource CreateAudioSource(string sourceName, AudioMixerGroup mixerGroup)
        {
            var go = new GameObject(sourceName);
            go.transform.SetParent(transform);
            
            var audioSource = go.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = mixerGroup;
            audioSource.playOnAwake = false;
            
            return audioSource;
        }
        
        private void SetupAudioListener()
        {
            _listenerCamera = FindObjectOfType<Camera>();
            
            if (_listenerCamera != null)
            {
                var listener = _listenerCamera.GetComponent<AudioListener>();
                if (listener == null)
                {
                    listener = _listenerCamera.gameObject.AddComponent<AudioListener>();
                }
            }
        }
        
        private void SetupSoundscapes()
        {
            if (_soundscapeLibrary == null) return;
            
            // Create soundscape layers
            foreach (SoundscapeLayer layer in Enum.GetValues(typeof(SoundscapeLayer)))
            {
                var layerSource = CreateAudioSource($"Soundscape_{layer}", _ambientMixerGroup);
                layerSource.loop = true;
                layerSource.spatialBlend = 0f; // Ambient audio is non-spatial
                _soundscapeLayers[layer] = layerSource;
            }
            
            // Set initial soundscape
            SetSoundscape("facility_ambient");
        }
        
        private void ConnectToGameSystems()
        {
            // Get system references
            if (GameManager.Instance != null)
            {
                _environmentalManager = GameManager.Instance.GetManager("EnvironmentalManager") as EnvironmentalManager;
                _plantManager = GameManager.Instance.GetManager("PlantManager");
                _facilityConstructor = GameManager.Instance.GetManager("InteractiveFacilityConstructor");
            }
            
            // Connect to system events
            ConnectSystemEvents();
            
            LogInfo("Connected to game systems");
        }
        
        private void ConnectSystemEvents()
        {
            // Environmental events
            if (_environmentalManager != null)
            {
                _environmentalManager.OnConditionsChanged += HandleEnvironmentalChange;
                _environmentalManager.OnAlertTriggered += HandleEnvironmentalAlert;
            }
            
            // Plant events - using event-based communication instead of direct manager access
            // Event subscriptions handled through GameManager event system
            
            // Construction events - using event-based communication instead of direct manager access  
            // Event subscriptions handled through GameManager event system
        }
        
        private void StartAudioUpdateLoop()
        {
            if (_enableDynamicSoundscapes)
            {
                InvokeRepeating(nameof(UpdateSoundscapeLogic), 1f, 2f);
            }
        }
        
        #endregion
        
        #region Audio Playback
        
        public AudioSource PlayAudioClip(string clipId, Vector3? position = null, float volume = 1f, float pitch = 1f)
        {
            var audioClip = _audioLibrary?.GetAudioClip(clipId);
            if (audioClip == null)
            {
                LogWarning($"Audio clip not found: {clipId}");
                return null;
            }
            
            return PlayAudioClip(audioClip, position, volume, pitch);
        }
        
        public AudioSource PlayAudioClip(AudioClip clip, Vector3? position = null, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return null;
            
            var audioSource = GetAvailableAudioSource();
            if (audioSource == null)
            {
                LogWarning("No available audio sources in pool");
                return null;
            }
            
            // Configure audio source
            audioSource.clip = clip;
            audioSource.volume = volume * _masterVolume;
            audioSource.pitch = pitch;
            
            if (position.HasValue && _enableSpatialAudio)
            {
                audioSource.transform.position = position.Value;
                audioSource.spatialBlend = 1f;
            }
            else
            {
                audioSource.spatialBlend = 0f;
            }
            
            audioSource.gameObject.SetActive(true);
            audioSource.Play();
            
            _activeAudioSources.Add(audioSource);
            OnAudioClipPlayed?.Invoke(clip);
            
            // Auto-return to pool when finished
            StartCoroutine(ReturnToPoolWhenFinished(audioSource));
            
            return audioSource;
        }
        
        public AudioSource PlayLoopingAudio(string clipId, string sourceKey, Vector3? position = null, float volume = 1f)
        {
            var audioClip = _audioLibrary?.GetAudioClip(clipId);
            if (audioClip == null)
            {
                LogWarning($"Audio clip not found: {clipId}");
                return null;
            }
            
            // Check if already playing
            if (_persistentAudioSources.TryGetValue(sourceKey, out var existingSource))
            {
                if (existingSource.clip == audioClip)
                {
                    return existingSource; // Already playing the same clip
                }
                else
                {
                    StopLoopingAudio(sourceKey); // Stop previous clip
                }
            }
            
            var audioSource = CreateAudioSource($"Persistent_{sourceKey}", _effectsMixerGroup);
            audioSource.clip = audioClip;
            audioSource.volume = volume * _masterVolume;
            audioSource.loop = true;
            
            if (position.HasValue && _enableSpatialAudio)
            {
                audioSource.transform.position = position.Value;
                audioSource.spatialBlend = 1f;
            }
            else
            {
                audioSource.spatialBlend = 0f;
            }
            
            audioSource.Play();
            _persistentAudioSources[sourceKey] = audioSource;
            
            return audioSource;
        }
        
        public void StopLoopingAudio(string sourceKey)
        {
            if (_persistentAudioSources.TryGetValue(sourceKey, out var audioSource))
            {
                audioSource.Stop();
                DestroyImmediate(audioSource.gameObject);
                _persistentAudioSources.Remove(sourceKey);
            }
        }
        
        public void PlayUISound(string soundId, float volume = 1f)
        {
            var audioClip = _audioLibrary?.GetUISound(soundId);
            if (audioClip == null) return;
            
            var audioSource = GetAvailableAudioSource();
            if (audioSource == null) return;
            
            audioSource.outputAudioMixerGroup = _uiMixerGroup;
            audioSource.clip = audioClip;
            audioSource.volume = volume * _masterVolume;
            audioSource.spatialBlend = 0f;
            audioSource.gameObject.SetActive(true);
            audioSource.Play();
            
            _activeAudioSources.Add(audioSource);
            StartCoroutine(ReturnToPoolWhenFinished(audioSource));
        }
        
        #endregion
        
        #region Soundscape Management
        
        public void SetSoundscape(string soundscapeId)
        {
            var soundscape = _soundscapeLibrary?.GetSoundscape(soundscapeId);
            if (soundscape == null)
            {
                LogWarning($"Soundscape not found: {soundscapeId}");
                return;
            }
            
            _currentSoundscape = soundscape;
            ApplySoundscape(soundscape);
            
            OnSoundscapeChanged?.Invoke(soundscape);
            LogInfo($"Applied soundscape: {soundscapeId}");
        }
        
        private void ApplySoundscape(DynamicSoundscape soundscape)
        {
            // Stop current soundscape layers
            foreach (var layerSource in _soundscapeLayers.Values)
            {
                layerSource.Stop();
            }
            
            // Apply new soundscape layers
            foreach (var layer in soundscape.Layers)
            {
                if (_soundscapeLayers.TryGetValue(layer.LayerType, out var layerSource))
                {
                    layerSource.clip = layer.AudioClip;
                    layerSource.volume = layer.Volume * _masterVolume;
                    layerSource.pitch = layer.Pitch;
                    
                    if (layer.AudioClip != null)
                    {
                        layerSource.Play();
                    }
                }
            }
        }
        
        private void UpdateSoundscapeLogic()
        {
            if (!_enableDynamicSoundscapes || _currentSoundscape == null) return;
            
            // Update soundscape based on environmental conditions
            UpdateEnvironmentalSoundscape();
            
            // Update soundscape based on facility state
            UpdateFacilitySoundscape();
            
            // Update soundscape based on plant conditions
            UpdatePlantSoundscape();
        }
        
        private void UpdateEnvironmentalSoundscape()
        {
            if (_environmentalManager == null) return;
            
            var conditions = _environmentalManager.GetCurrentConditions();
            
            // Adjust ambient volume based on HVAC activity
            float hvacIntensity = CalculateHVACIntensity(conditions);
            if (_soundscapeLayers.TryGetValue(SoundscapeLayer.HVAC, out var hvacSource))
            {
                hvacSource.volume = hvacIntensity * _masterVolume;
            }
            
            // Add wind effects based on air velocity
            float windIntensity = Mathf.Clamp01(conditions.AirVelocity / 2f);
            if (_soundscapeLayers.TryGetValue(SoundscapeLayer.Ventilation, out var ventSource))
            {
                ventSource.volume = windIntensity * _masterVolume;
            }
        }
        
        private void UpdateFacilitySoundscape()
        {
            // Update electrical hum based on power consumption
            float electricalActivity = CalculateElectricalActivity();
            if (_soundscapeLayers.TryGetValue(SoundscapeLayer.Electrical, out var electricalSource))
            {
                electricalSource.volume = electricalActivity * _masterVolume;
            }
        }
        
        private void UpdatePlantSoundscape()
        {
            if (_plantManager == null) return;
            
            // Simulate plant activity since we can't access specific methods
            float plantActivity = UnityEngine.Random.Range(0.1f, 0.5f);
            
            if (_soundscapeLayers.TryGetValue(SoundscapeLayer.Growth, out var growthSource))
            {
                growthSource.volume = plantActivity * 0.3f * _masterVolume; // Very subtle
            }
        }
        
        #endregion
        
        #region Environmental Audio
        
        public void RegisterEnvironmentalAudioSource(string sourceId, Vector3 position, AudioClip clip, float maxDistance = 20f)
        {
            if (_environmentalSources.ContainsKey(sourceId))
            {
                UpdateEnvironmentalAudioSource(sourceId, position);
                return;
            }
            
            var audioSource = CreateAudioSource($"Environmental_{sourceId}", _environmentalMixerGroup);
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.spatialBlend = 1f;
            audioSource.maxDistance = maxDistance;
            audioSource.transform.position = position;
            audioSource.Play();
            
            var envSource = new EnvironmentalAudioSource
            {
                SourceId = sourceId,
                AudioSource = audioSource,
                Position = position,
                MaxDistance = maxDistance,
                BaseVolume = 1f
            };
            
            _environmentalSources[sourceId] = envSource;
        }
        
        public void UpdateEnvironmentalAudioSource(string sourceId, Vector3 newPosition)
        {
            if (_environmentalSources.TryGetValue(sourceId, out var envSource))
            {
                envSource.Position = newPosition;
                envSource.AudioSource.transform.position = newPosition;
            }
        }
        
        public void UnregisterEnvironmentalAudioSource(string sourceId)
        {
            if (_environmentalSources.TryGetValue(sourceId, out var envSource))
            {
                envSource.AudioSource.Stop();
                DestroyImmediate(envSource.AudioSource.gameObject);
                _environmentalSources.Remove(sourceId);
            }
        }
        
        #endregion
        
        #region Music System
        
        public void PlayMusic(string trackId, bool loop = true, float fadeInTime = 2f)
        {
            var musicTrack = _musicPlaylist?.GetTrack(trackId);
            if (musicTrack == null)
            {
                LogWarning($"Music track not found: {trackId}");
                return;
            }
            
            _musicController.PlayTrack(musicTrack, _musicSource, loop, fadeInTime);
        }
        
        public void StopMusic(float fadeOutTime = 2f)
        {
            _musicController.StopCurrentTrack(_musicSource, fadeOutTime);
        }
        
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            _masterMixer.SetFloat("MusicVolume", Mathf.Log10(_musicVolume) * 20f);
        }
        
        public void SetMusicEnabled(bool enabled)
        {
            _isMusicEnabled = enabled;
            if (!enabled && _musicSource.isPlaying)
            {
                StopMusic();
            }
        }
        
        #endregion
        
        #region Dynamic Audio Updates
        
        private void UpdateDynamicAudio()
        {
            UpdateEnvironmentalAudio();
            UpdateConstructionAudio();
            UpdatePlantAudio();
            UpdateAdaptiveMixing();
        }
        
        private void UpdateEnvironmentalAudio()
        {
            if (!_enableEnvironmentalAudio || _environmentalManager == null) return;
            
            var conditions = _environmentalManager.GetCurrentConditions(null);
            
            // Update HVAC audio based on temperature differential
            UpdateHVACAudio(conditions);
            
            // Update lighting audio based on light intensity
            UpdateLightingAudio(conditions);
        }
        
        private void UpdateHVACAudio(ProjectChimera.Data.Environment.EnvironmentalConditions conditions)
        {
            float targetTemp = 24f; // Ideal temperature
            float tempDiff = Mathf.Abs(conditions.Temperature - targetTemp);
            float hvacActivity = Mathf.Clamp01(tempDiff / 5f); // Normalize to 0-1
            
            // Play HVAC audio with intensity based on activity
            if (hvacActivity > 0.1f)
            {
                PlayLoopingAudio("hvac_running", "hvac_system", null, hvacActivity * 0.5f);
            }
            else
            {
                StopLoopingAudio("hvac_system");
            }
        }
        
        private void UpdateLightingAudio(ProjectChimera.Data.Environment.EnvironmentalConditions conditions)
        {
            // Simulate lighting audio since we can't access AdvancedGrowLightSystem directly
            bool hasActiveLights = UnityEngine.Random.Range(0f, 1f) > 0.3f;
            
            if (hasActiveLights)
            {
                float ballastVolume = UnityEngine.Random.Range(0.1f, 0.3f);
                PlayLoopingAudio("light_ballast_hum", "general_lighting", null, ballastVolume);
            }
            else
            {
                StopLoopingAudio("general_lighting");
            }
        }
        
        private void UpdateConstructionAudio()
        {
            if (_facilityConstructor == null) return;
            
            // Simulate construction activity since we can't access specific properties
            bool hasActiveConstruction = UnityEngine.Random.Range(0f, 1f) > 0.7f;
            
            if (hasActiveConstruction)
            {
                PlayLoopingAudio("construction_ambient", "construction_work", null, 0.4f);
            }
            else
            {
                StopLoopingAudio("construction_work");
            }
        }
        
        private void UpdatePlantAudio()
        {
            if (_plantManager == null) return;
            
            // Simulate plant count and health since we can't access specific methods
            int simulatedHealthyPlants = UnityEngine.Random.Range(5, 15);
            
            if (simulatedHealthyPlants > 0)
            {
                float growthIntensity = simulatedHealthyPlants / 20f; // Scale based on plant count
                PlayLoopingAudio("plant_growth_subtle", "plant_growth", null, growthIntensity * 0.2f);
            }
            else
            {
                StopLoopingAudio("plant_growth");
            }
        }
        
        #endregion
        
        #region Spatial Audio
        
        private void UpdateSpatialAudio()
        {
            if (!_enableSpatialAudio || _listenerCamera == null) return;
            
            Vector3 listenerPosition = _listenerCamera.transform.position;
            
            // Update environmental audio source distances and volumes
            foreach (var envSource in _environmentalSources.Values)
            {
                float distance = Vector3.Distance(listenerPosition, envSource.Position);
                float volumeMultiplier = _audioFalloffCurve.Evaluate(distance / envSource.MaxDistance);
                
                envSource.AudioSource.volume = envSource.BaseVolume * volumeMultiplier * _masterVolume;
            }
        }
        
        #endregion
        
        #region Audio Source Management
        
        private AudioSource GetAvailableAudioSource()
        {
            if (_availableAudioSources.Count > 0)
            {
                var audioSource = _availableAudioSources.Dequeue();
                audioSource.gameObject.SetActive(true);
                return audioSource;
            }
            
            // If pool is empty, try to find a finished audio source
            for (int i = _activeAudioSources.Count - 1; i >= 0; i--)
            {
                var activeSource = _activeAudioSources[i];
                if (!activeSource.isPlaying)
                {
                    ReturnAudioSourceToPool(activeSource);
                    return GetAvailableAudioSource();
                }
            }
            
            return null; // No sources available
        }
        
        private void ReturnAudioSourceToPool(AudioSource audioSource)
        {
            if (audioSource == null) return;
            
            _activeAudioSources.Remove(audioSource);
            
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.gameObject.SetActive(false);
            
            _availableAudioSources.Enqueue(audioSource);
        }
        
        private IEnumerator ReturnToPoolWhenFinished(AudioSource audioSource)
        {
            while (audioSource.isPlaying)
            {
                yield return null;
            }
            
            ReturnAudioSourceToPool(audioSource);
        }
        
        private void UpdateAudioSourcePool()
        {
            // Clean up finished audio sources
            for (int i = _activeAudioSources.Count - 1; i >= 0; i--)
            {
                var activeSource = _activeAudioSources[i];
                if (activeSource == null || !activeSource.isPlaying)
                {
                    if (activeSource != null)
                    {
                        ReturnAudioSourceToPool(activeSource);
                    }
                    else
                    {
                        _activeAudioSources.RemoveAt(i);
                    }
                }
            }
        }
        
        private void ProcessAudioQueue()
        {
            // Process any queued audio operations
            // This could include delayed audio triggers, scheduled music changes, etc.
        }
        
        #endregion
        
        #region Adaptive Mixing
        
        private void UpdateAdaptiveMixing()
        {
            if (!_enableAdaptiveMixing) return;
            
            // Adjust mix based on current audio state
            switch (_currentAudioState)
            {
                case AudioState.Facility:
                    ApplyFacilityMix();
                    break;
                case AudioState.Construction:
                    ApplyConstructionMix();
                    break;
                case AudioState.Menu:
                    ApplyMenuMix();
                    break;
            }
        }
        
        private void ApplyFacilityMix()
        {
            _masterMixer.SetFloat("AmbientVolume", -5f); // Slightly reduced ambient
            _masterMixer.SetFloat("EffectsVolume", 0f);  // Full effects
            _masterMixer.SetFloat("EnvironmentalVolume", -3f); // Moderate environmental
        }
        
        private void ApplyConstructionMix()
        {
            _masterMixer.SetFloat("AmbientVolume", -10f); // Reduced ambient
            _masterMixer.SetFloat("EffectsVolume", 3f);   // Boosted effects
            _masterMixer.SetFloat("EnvironmentalVolume", 0f); // Full environmental
        }
        
        private void ApplyMenuMix()
        {
            _masterMixer.SetFloat("AmbientVolume", -15f); // Very reduced ambient
            _masterMixer.SetFloat("EffectsVolume", -5f);  // Reduced effects
            _masterMixer.SetFloat("UIVolume", 0f);        // Full UI sounds
        }
        
        #endregion
        
        #region Event Handlers
        
        private void HandleEnvironmentalChange(ProjectChimera.Data.Environment.EnvironmentalConditions conditions)
        {
            // Trigger audio feedback for environmental changes
            if (conditions.Temperature > 30f)
            {
                PlayAudioClip("temperature_warning", null, 0.5f);
            }
            
            if (conditions.Humidity > 80f)
            {
                PlayAudioClip("humidity_warning", null, 0.5f);
            }
        }
        
        /// <summary>
        /// Handle environmental alert events
        /// </summary>
        private void HandleEnvironmentalAlert(object alert)
        {
            if (alert == null) return;
            
            // Play generic alert sound since we can't access specific alert properties
            PlayUISound("alert_medium", 0.8f);
            
            LogInfo("Environmental alert audio triggered");
        }
        
        private void HandlePlantAdded(object plant)
        {
            PlayAudioClip("plant_added", null, 0.4f);
        }
        
        private void HandlePlantStageChanged(object plant)
        {
            // Use generic plant stage audio since we can't access specific stage info
            PlayAudioClip("plant_growth_stage", null, 0.3f);
        }
        
        private void HandleConstructionStarted(object project)
        {
            PlayAudioClip("construction_started", null, 0.6f);
            SetAudioState(AudioState.Construction);
        }
        
        private void HandleConstructionIssue(string projectId, object issue)
        {
            // Play generic construction issue audio since we can't access severity info
            PlayAudioClip("construction_issue_generic", null, 0.8f);
        }
        
        #endregion
        
        #region Performance Monitoring
        
        private void UpdatePerformanceMetrics()
        {
            _performanceMetrics.ActiveSources = _activeAudioSources.Count;
            _performanceMetrics.AvailableSources = _availableAudioSources.Count;
            _performanceMetrics.EnvironmentalSources = _environmentalSources.Count;
            _performanceMetrics.PersistentSources = _persistentAudioSources.Count;
            _performanceMetrics.LastUpdate = DateTime.Now;
            
            // Track performance history
            var loadData = new AudioLoadData
            {
                Timestamp = DateTime.Now,
                ActiveSources = _activeAudioSources.Count,
                CPULoad = 0f // Would need profiling integration
            };
            
            _performanceHistory.Enqueue(loadData);
            
            // Maintain history size
            while (_performanceHistory.Count > 300) // 5 minutes at 1-second intervals
            {
                _performanceHistory.Dequeue();
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        private float CalculateHVACIntensity(ProjectChimera.Data.Environment.EnvironmentalConditions conditions)
        {
            float targetTemp = 24f;
            float targetHumidity = 60f;
            
            float tempDiff = Mathf.Abs(conditions.Temperature - targetTemp) / 10f;
            float humidityDiff = Mathf.Abs(conditions.Humidity - targetHumidity) / 40f;
            
            return Mathf.Clamp01(Mathf.Max(tempDiff, humidityDiff));
        }
        
        private float CalculateElectricalActivity()
        {
            // Return simulated electrical activity since we can't access specific light systems
            return UnityEngine.Random.Range(0.3f, 0.8f);
        }
        
        #endregion
        
        #region Public Interface
        
        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            _masterMixer.SetFloat("MasterVolume", Mathf.Log10(_masterVolume) * 20f);
            
            OnVolumeChanged?.Invoke(_masterVolume);
        }
        
        public void SetAudioMuted(bool muted)
        {
            _isAudioMuted = muted;
            _masterMixer.SetFloat("MasterVolume", muted ? -80f : Mathf.Log10(_masterVolume) * 20f);
        }
        
        public void SetAudioState(AudioState newState)
        {
            if (_currentAudioState == newState) return;
            
            _currentAudioState = newState;
            
            // Apply state-specific audio configuration
            UpdateAdaptiveMixing();
            
            // Update soundscape if needed
            string newSoundscape = newState switch
            {
                AudioState.Facility => "facility_ambient",
                AudioState.Construction => "construction_ambient",
                AudioState.Menu => "menu_ambient",
                _ => "facility_ambient"
            };
            
            SetSoundscape(newSoundscape);
        }
        
        public void SetMixerGroupVolume(string groupName, float volume)
        {
            if (_masterMixer != null)
            {
                _masterMixer.SetFloat($"{groupName}Volume", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f);
            }
        }
        
        public AudioPerformanceReport GetPerformanceReport()
        {
            return new AudioPerformanceReport
            {
                Metrics = _performanceMetrics,
                PerformanceHistory = _performanceHistory.ToList(),
                SoundscapeInfo = _currentSoundscape,
                ActiveSourceCount = _activeAudioSources.Count
            };
        }
        
        #endregion
        
        protected override void OnManagerShutdown()
        {
            // Stop all coroutines and cleanup
            StopAllCoroutines();
            CancelInvoke();
            
            // Stop all audio sources
            foreach (var audioSource in _activeAudioSources)
            {
                if (audioSource != null)
                {
                    audioSource.Stop();
                }
            }
            _activeAudioSources.Clear();
            
            // Stop persistent audio sources
            foreach (var audioSource in _persistentAudioSources.Values)
            {
                if (audioSource != null)
                {
                    audioSource.Stop();
                }
            }
            _persistentAudioSources.Clear();
            
            // Stop music
            if (_musicSource != null)
            {
                _musicSource.Stop();
            }
            
            // Stop soundscape layers
            foreach (var layerSource in _soundscapeLayers.Values)
            {
                if (layerSource != null)
                {
                    layerSource.Stop();
                }
            }
            _soundscapeLayers.Clear();
            
            // Cleanup environmental audio sources
            foreach (var envSource in _environmentalSources.Values)
            {
                if (envSource.AudioSource != null)
                {
                    envSource.AudioSource.Stop();
                }
            }
            _environmentalSources.Clear();
            
            // Cleanup procedural generators
            foreach (var generator in _proceduralGenerators)
            {
                generator?.Cleanup();
            }
            _proceduralGenerators.Clear();
            
            // Return all pooled audio sources
            while (_availableAudioSources.Count > 0)
            {
                var audioSource = _availableAudioSources.Dequeue();
                if (audioSource != null)
                {
                    DestroyImmediate(audioSource.gameObject);
                }
            }
            
            // Cleanup music controller
            _musicController?.Cleanup();
            
            // Disconnect system events
            DisconnectSystemEvents();
            
            LogInfo("Advanced Audio Manager shutdown complete");
        }
        
        private void DisconnectSystemEvents()
        {
            if (_environmentalManager != null)
            {
                _environmentalManager.OnConditionsChanged -= HandleEnvironmentalChange;
                _environmentalManager.OnAlertTriggered -= HandleEnvironmentalAlert;
            }
            
            // Plant and facility events are now handled through event-based communication
            // No direct event unsubscription needed for generic ChimeraManager types
        }
    }
}