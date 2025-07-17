using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.UI
{
    /// <summary>
    /// Configuration ScriptableObject for the Chimera Debug Inspector.
    /// Follows Project Chimera's data-driven architecture patterns.
    /// </summary>
    [CreateAssetMenu(fileName = "New Debug Inspector Config", menuName = "Chimera/UI/Debug Inspector Config")]
    public class DebugInspectorConfigSO : ChimeraDataSO
    {
        [Header("Inspector Settings")]
        [SerializeField] private KeyCode _toggleKey = KeyCode.F12;
        [SerializeField] private float _updateInterval = 0.5f;
        [SerializeField] private bool _enableOnStart = false;
        [SerializeField] private bool _showInDevelopmentOnly = true;
        
        [Header("Display Configuration")]
        [SerializeField] private Vector2 _inspectorPosition = new Vector2(20, 20);
        [SerializeField] private Vector2 _inspectorSize = new Vector2(400, 600);
        [SerializeField] private float _backgroundOpacity = 0.8f;
        
        [Header("Performance Settings")]
        [SerializeField] private int _maxPlantsToDisplay = 50;
        [SerializeField] private bool _enableRealTimeUpdates = true;
        [SerializeField] private bool _enableEnvironmentalData = true;
        [SerializeField] private bool _enableGeneticData = true;
        
        [Header("Time Control Settings")]
        [SerializeField] private float[] _timeScaleOptions = { 0f, 0.5f, 1f, 2f, 3f, 5f };
        [SerializeField] private bool _enableTimeControls = true;
        
        public KeyCode ToggleKey => _toggleKey;
        public float UpdateInterval => _updateInterval;
        public bool EnableOnStart => _enableOnStart;
        public bool ShowInDevelopmentOnly => _showInDevelopmentOnly;
        public Vector2 InspectorPosition => _inspectorPosition;
        public Vector2 InspectorSize => _inspectorSize;
        public float BackgroundOpacity => _backgroundOpacity;
        public int MaxPlantsToDisplay => _maxPlantsToDisplay;
        public bool EnableRealTimeUpdates => _enableRealTimeUpdates;
        public bool EnableEnvironmentalData => _enableEnvironmentalData;
        public bool EnableGeneticData => _enableGeneticData;
        public float[] TimeScaleOptions => _timeScaleOptions;
        public bool EnableTimeControls => _enableTimeControls;
        
        protected override bool ValidateDataSpecific()
        {
            // Debug Inspector configuration validation
            return _updateInterval > 0f && _maxPlantsToDisplay > 0 && _timeScaleOptions != null && _timeScaleOptions.Length > 0;
        }
    }
}