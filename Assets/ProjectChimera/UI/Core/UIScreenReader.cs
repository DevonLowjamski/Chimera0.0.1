using UnityEngine;
using System.Collections.Generic;
using System.Text;
using ProjectChimera.Core;
using ProjectChimera.Data.UI;

namespace ProjectChimera.UI.Core
{
    /// <summary>
    /// Screen reader implementation for UI accessibility in Project Chimera.
    /// Provides text-to-speech and navigation assistance for visually impaired users.
    /// </summary>
    public class UIScreenReader
    {
        private Queue<UIAnnouncement> _announcementQueue;
        private Dictionary<UIAnnouncementPriority, float> _priorityDelays;
        private StringBuilder _textBuffer;
        private bool _isInitialized = false;
        private bool _isSpeaking = false;
        private float _lastAnnouncementTime;
        
        // Text-to-speech settings
        private float _speechRate = 1f;
        private float _speechVolume = 1f;
        private int _speechPitch = 50;
        private string _speechVoice = "default";
        
        // Events
        public System.Action<string, UIAnnouncementPriority> OnAnnouncementRequested;
        public System.Action<string> OnSpeechStarted;
        public System.Action<string> OnSpeechCompleted;
        public System.Action OnSpeechInterrupted;
        
        // Properties
        public bool IsInitialized => _isInitialized;
        public bool IsSpeaking => _isSpeaking;
        public int QueuedAnnouncementCount => _announcementQueue?.Count ?? 0;
        public float SpeechRate => _speechRate;
        public float SpeechVolume => _speechVolume;
        
        /// <summary>
        /// Initialize screen reader
        /// </summary>
        public void Initialize()
        {
            _announcementQueue = new Queue<UIAnnouncement>();
            _textBuffer = new StringBuilder();
            
            InitializePriorityDelays();
            InitializeTextToSpeech();
            
            _isInitialized = true;
            
            Debug.Log("Screen reader initialized successfully");
        }
        
        /// <summary>
        /// Initialize priority delays for announcements
        /// </summary>
        private void InitializePriorityDelays()
        {
            _priorityDelays = new Dictionary<UIAnnouncementPriority, float>
            {
                [UIAnnouncementPriority.Critical] = 0f,      // Immediate
                [UIAnnouncementPriority.High] = 0.1f,       // Very quick
                [UIAnnouncementPriority.Normal] = 0.5f,     // Normal delay
                [UIAnnouncementPriority.Low] = 1f,          // Longer delay
                [UIAnnouncementPriority.Background] = 2f    // Background only
            };
        }
        
        /// <summary>
        /// Initialize text-to-speech system
        /// </summary>
        private void InitializeTextToSpeech()
        {
            // In a real implementation, this would initialize platform-specific TTS
            // For Unity, you might use:
            // - Windows: SAPI (Speech API)
            // - macOS: AVSpeechSynthesizer
            // - Mobile: Native TTS APIs
            // - Web: Web Speech API
            
            Debug.Log("Text-to-speech system initialized");
        }
        
        /// <summary>
        /// Announce text to screen reader
        /// </summary>
        public void Announce(string text, UIAnnouncementPriority priority = UIAnnouncementPriority.Normal)
        {
            if (!_isInitialized || string.IsNullOrEmpty(text))
                return;
            
            var announcement = new UIAnnouncement
            {
                Message = CleanTextForSpeech(text),
                Priority = priority,
                Timestamp = Time.time
            };
            
            // Handle priority interruption
            if (priority == UIAnnouncementPriority.Critical)
            {
                // Clear queue and interrupt current speech for critical announcements
                _announcementQueue.Clear();
                InterruptSpeech();
                
                SpeakImmediate(announcement.Message);
            }
            // else
            // {
                _announcementQueue.Enqueue(announcement);
            // }
            
            OnAnnouncementRequested?.Invoke(text, priority);
        }
        
        /// <summary>
        /// Process queued announcements
        /// </summary>
        public void ProcessAnnouncements()
        {
            if (!_isInitialized || _isSpeaking || _announcementQueue.Count == 0)
                return;
            
            var currentTime = Time.time;
            var announcement = _announcementQueue.Peek();
            
            // Check if enough time has passed based on priority
            var requiredDelay = _priorityDelays[announcement.Priority];
            if (currentTime - _lastAnnouncementTime < requiredDelay)
                return;
            
            // Process the announcement
            announcement = _announcementQueue.Dequeue();
            SpeakText(announcement.Message);
            
            _lastAnnouncementTime = currentTime;
        }
        
        /// <summary>
        /// Clean text for speech synthesis
        /// </summary>
        private string CleanTextForSpeech(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            
            _textBuffer.Clear();
            _textBuffer.Append(text);
            
            // Remove HTML/XML tags
            _textBuffer.Replace("<", " ");
            _textBuffer.Replace(">", " ");
            
            // Replace common abbreviations
            _textBuffer.Replace("&", " and ");
            _textBuffer.Replace("@", " at ");
            _textBuffer.Replace("#", " number ");
            _textBuffer.Replace("%", " percent ");
            _textBuffer.Replace("$", " dollars ");
            
            // Replace symbols with words
            _textBuffer.Replace("+", " plus ");
            _textBuffer.Replace("-", " minus ");
            _textBuffer.Replace("*", " times ");
            _textBuffer.Replace("/", " divided by ");
            _textBuffer.Replace("=", " equals ");
            
            // Handle UI-specific terms
            _textBuffer.Replace("btn", "button");
            _textBuffer.Replace("txt", "text");
            _textBuffer.Replace("img", "image");
            _textBuffer.Replace("lnk", "link");
            
            // Clean up whitespace
            while (_textBuffer.ToString().Contains("  "))
            {
                _textBuffer.Replace("  ", " ");
            }
            
            return _textBuffer.ToString().Trim();
        }
        
        /// <summary>
        /// Speak text immediately (for critical announcements)
        /// </summary>
        private void SpeakImmediate(string text)
        {
            SpeakTextInternal(text, true);
        }
        
        /// <summary>
        /// Speak text normally
        /// </summary>
        private void SpeakText(string text)
        {
            SpeakTextInternal(text, false);
        }
        
        /// <summary>
        /// Internal speech synthesis
        /// </summary>
        private void SpeakTextInternal(string text, bool immediate)
        {
            if (string.IsNullOrEmpty(text))
                return;
            
            _isSpeaking = true;
            OnSpeechStarted?.Invoke(text);
            
            // Platform-specific text-to-speech implementation
            #if UNITY_EDITOR || UNITY_STANDALONE_WIN
            SpeakTextWindows(text);
            #elif UNITY_STANDALONE_OSX
            SpeakTextMacOS(text);
            #elif UNITY_ANDROID
            SpeakTextAndroid(text);
            #elif UNITY_IOS
            SpeakTextIOS(text);
            #elif UNITY_WEBGL
            SpeakTextWebGL(text);
            #else
            // Fallback - log to console
            Debug.Log($"[Screen Reader] {text}");
            OnSpeechComplete(text);
            #endif
        }
        
        #if UNITY_EDITOR || UNITY_STANDALONE_WIN
        /// <summary>
        /// Windows text-to-speech using SAPI
        /// </summary>
        private void SpeakTextWindows(string text)
        {
            try
            {
                // In a real implementation, you would use Windows SAPI
                // This is a placeholder that simulates speech
                Debug.Log($"[Windows TTS] {text}");
                
                // Simulate speech duration
                var speechDuration = CalculateSpeechDuration(text);
                CoroutineRunner.Instance.StartCoroutine(SimulateSpeechCoroutine(text, speechDuration));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Windows TTS error: {ex.Message}");
                OnSpeechComplete(text);
            }
        }
        #endif
        
        #if UNITY_STANDALONE_OSX
        /// <summary>
        /// macOS text-to-speech using say command
        /// </summary>
        private void SpeakTextMacOS(string text)
        {
            try
            {
                // In a real implementation, you would use macOS AVSpeechSynthesizer
                Debug.Log($"[macOS TTS] {text}");
                
                var speechDuration = CalculateSpeechDuration(text);
                CoroutineRunner.Instance.StartCoroutine(SimulateSpeechCoroutine(text, speechDuration));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"macOS TTS error: {ex.Message}");
                OnSpeechComplete(text);
            }
        }
        #endif
        
        #if UNITY_ANDROID
        /// <summary>
        /// Android text-to-speech
        /// </summary>
        private void SpeakTextAndroid(string text)
        {
            try
            {
                // In a real implementation, you would use Android TTS API
                Debug.Log($"[Android TTS] {text}");
                
                var speechDuration = CalculateSpeechDuration(text);
                CoroutineRunner.Instance.StartCoroutine(SimulateSpeechCoroutine(text, speechDuration));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Android TTS error: {ex.Message}");
                OnSpeechComplete(text);
            }
        }
        #endif
        
        #if UNITY_IOS
        /// <summary>
        /// iOS text-to-speech
        /// </summary>
        private void SpeakTextIOS(string text)
        {
            try
            {
                // In a real implementation, you would use iOS AVSpeechSynthesizer
                Debug.Log($"[iOS TTS] {text}");
                
                var speechDuration = CalculateSpeechDuration(text);
                CoroutineRunner.Instance.StartCoroutine(SimulateSpeechCoroutine(text, speechDuration));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"iOS TTS error: {ex.Message}");
                OnSpeechComplete(text);
            }
        }
        #endif
        
        #if UNITY_WEBGL
        /// <summary>
        /// WebGL text-to-speech using Web Speech API
        /// </summary>
        private void SpeakTextWebGL(string text)
        {
            try
            {
                // In a real implementation, you would use Web Speech API
                Debug.Log($"[WebGL TTS] {text}");
                
                var speechDuration = CalculateSpeechDuration(text);
                CoroutineRunner.Instance.StartCoroutine(SimulateSpeechCoroutine(text, speechDuration));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"WebGL TTS error: {ex.Message}");
                OnSpeechComplete(text);
            }
        }
        #endif
        
        /// <summary>
        /// Calculate estimated speech duration
        /// </summary>
        private float CalculateSpeechDuration(string text)
        {
            // Average speaking rate is about 150-160 words per minute
            var words = text.Split(' ').Length;
            var wordsPerSecond = 2.5f / _speechRate; // Adjustable by speech rate
            return words / wordsPerSecond;
        }
        
        /// <summary>
        /// Simulate speech coroutine (for platforms without real TTS)
        /// </summary>
        private System.Collections.IEnumerator SimulateSpeechCoroutine(string text, float duration)
        {
            yield return new WaitForSeconds(duration);
            OnSpeechComplete(text);
        }
        
        /// <summary>
        /// Handle speech completion
        /// </summary>
        private void OnSpeechComplete(string text)
        {
            _isSpeaking = false;
            OnSpeechCompleted?.Invoke(text);
        }
        
        /// <summary>
        /// Interrupt current speech
        /// </summary>
        public void InterruptSpeech()
        {
            if (!_isSpeaking)
                return;
            
            _isSpeaking = false;
            OnSpeechInterrupted?.Invoke();
            
            // Platform-specific speech interruption
            #if UNITY_EDITOR || UNITY_STANDALONE_WIN
            InterruptSpeechWindows();
            #elif UNITY_STANDALONE_OSX
            InterruptSpeechMacOS();
            #elif UNITY_ANDROID
            InterruptSpeechAndroid();
            #elif UNITY_IOS
            InterruptSpeechIOS();
            #elif UNITY_WEBGL
            InterruptSpeechWebGL();
            #endif
        }
        
        #if UNITY_EDITOR || UNITY_STANDALONE_WIN
        private void InterruptSpeechWindows()
        {
            // Windows SAPI interrupt implementation
            Debug.Log("[Windows TTS] Speech interrupted");
        }
        #endif
        
        #if UNITY_STANDALONE_OSX
        private void InterruptSpeechMacOS()
        {
            // macOS speech interrupt implementation
            Debug.Log("[macOS TTS] Speech interrupted");
        }
        #endif
        
        #if UNITY_ANDROID
        private void InterruptSpeechAndroid()
        {
            // Android TTS interrupt implementation
            Debug.Log("[Android TTS] Speech interrupted");
        }
        #endif
        
        #if UNITY_IOS
        private void InterruptSpeechIOS()
        {
            // iOS speech interrupt implementation
            Debug.Log("[iOS TTS] Speech interrupted");
        }
        #endif
        
        #if UNITY_WEBGL
        private void InterruptSpeechWebGL()
        {
            // WebGL speech interrupt implementation
            Debug.Log("[WebGL TTS] Speech interrupted");
        }
        #endif
        
        /// <summary>
        /// Stop all speech and clear queue
        /// </summary>
        public void StopSpeech()
        {
            InterruptSpeech();
            _announcementQueue.Clear();
        }
        
        /// <summary>
        /// Set speech rate
        /// </summary>
        public void SetSpeechRate(float rate)
        {
            _speechRate = Mathf.Clamp(rate, 0.1f, 3f);
        }
        
        /// <summary>
        /// Set speech volume
        /// </summary>
        public void SetSpeechVolume(float volume)
        {
            _speechVolume = Mathf.Clamp01(volume);
        }
        
        /// <summary>
        /// Set speech pitch
        /// </summary>
        public void SetSpeechPitch(int pitch)
        {
            _speechPitch = Mathf.Clamp(pitch, 0, 100);
        }
        
        /// <summary>
        /// Set speech voice
        /// </summary>
        public void SetSpeechVoice(string voice)
        {
            _speechVoice = voice ?? "default";
        }
        
        /// <summary>
        /// Get available voices
        /// </summary>
        public List<string> GetAvailableVoices()
        {
            // In a real implementation, this would query the TTS system
            return new List<string> { "default", "male", "female", "child" };
        }
        
        /// <summary>
        /// Test speech synthesis
        /// </summary>
        public void TestSpeech()
        {
            Announce("Screen reader test. This is a test of the text to speech system.", UIAnnouncementPriority.High);
        }
        
        /// <summary>
        /// Get screen reader statistics
        /// </summary>
        public UIScreenReaderStats GetStats()
        {
            return new UIScreenReaderStats
            {
                IsInitialized = _isInitialized,
                IsSpeaking = _isSpeaking,
                QueuedAnnouncements = _announcementQueue.Count,
                SpeechRate = _speechRate,
                SpeechVolume = _speechVolume,
                SpeechPitch = _speechPitch,
                CurrentVoice = _speechVoice
            };
        }
        
        /// <summary>
        /// Cleanup screen reader resources
        /// </summary>
        public void Cleanup()
        {
            StopSpeech();
            _announcementQueue?.Clear();
            _textBuffer?.Clear();
            _isInitialized = false;
            
            Debug.Log("Screen reader cleaned up");
        }
        
        /// <summary>
        /// Shutdown screen reader (alias for Cleanup)
        /// </summary>
        public void Shutdown()
        {
            Cleanup();
        }
    }
    
    /// <summary>
    /// Screen reader statistics
    /// </summary>
    public struct UIScreenReaderStats
    {
        public bool IsInitialized;
        public bool IsSpeaking;
        public int QueuedAnnouncements;
        public float SpeechRate;
        public float SpeechVolume;
        public int SpeechPitch;
        public string CurrentVoice;
    }
    
    /// <summary>
    /// Coroutine runner for screen reader (singleton helper)
    /// </summary>
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;
        
        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("CoroutineRunner");
                    _instance = go.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
    }
}