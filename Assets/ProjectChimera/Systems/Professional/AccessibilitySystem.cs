using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Systems.Professional
{
    /// <summary>
    /// Accessibility System for Professional Polish
    /// Provides comprehensive accessibility features including colorblind support,
    /// high contrast mode, large text, and screen reader compatibility
    /// </summary>
    public class AccessibilitySystem
    {
        private bool _isInitialized = false;
        private bool _colorblindSupport = false;
        private bool _highContrast = false;
        private bool _largeText = false;
        private bool _screenReaderSupport = false;
        private float _uiScale = 1.0f;
        
        private Dictionary<string, ColorProfile> _colorProfiles = new Dictionary<string, ColorProfile>();
        private List<Text> _textComponents = new List<Text>();
        private List<Image> _imageComponents = new List<Image>();
        private ColorProfile _originalColorProfile;
        private float _originalFontSize = 12f;
        
        public void Initialize()
        {
            LoadColorProfiles();
            CacheUIComponents();
            SaveOriginalSettings();
            
            _isInitialized = true;
            Debug.Log("Accessibility System initialized");
        }
        
        public void SetColorblindSupport(bool enabled)
        {
            if (!_isInitialized) return;
            
            _colorblindSupport = enabled;
            
            if (enabled)
            {
                ApplyColorblindFriendlyColors();
            }
            else
            {
                RestoreOriginalColors();
            }
        }
        
        public void SetHighContrast(bool enabled)
        {
            if (!_isInitialized) return;
            
            _highContrast = enabled;
            
            if (enabled)
            {
                ApplyHighContrastColors();
            }
            else
            {
                RestoreOriginalColors();
            }
        }
        
        public void SetLargeText(bool enabled)
        {
            if (!_isInitialized) return;
            
            _largeText = enabled;
            
            if (enabled)
            {
                ApplyLargeTextSizes();
            }
            else
            {
                RestoreOriginalTextSizes();
            }
        }
        
        public void SetScreenReaderSupport(bool enabled)
        {
            if (!_isInitialized) return;
            
            _screenReaderSupport = enabled;
            
            if (enabled)
            {
                EnableScreenReaderFeatures();
            }
            else
            {
                DisableScreenReaderFeatures();
            }
        }
        
        public void SetUIScale(float scale)
        {
            if (!_isInitialized) return;
            
            _uiScale = Mathf.Clamp(scale, 0.5f, 2.0f);
            ApplyUIScale();
        }
        
        public void SetColorProfile(string profileName)
        {
            if (!_isInitialized || !_colorProfiles.ContainsKey(profileName)) return;
            
            var profile = _colorProfiles[profileName];
            ApplyColorProfile(profile);
        }
        
        public void AddAccessibilityTooltip(GameObject target, string tooltip)
        {
            var accessibilityTooltip = target.GetComponent<AccessibilityTooltip>();
            if (accessibilityTooltip == null)
            {
                accessibilityTooltip = target.AddComponent<AccessibilityTooltip>();
            }
            
            accessibilityTooltip.SetTooltipText(tooltip);
        }
        
        public void EnableKeyboardNavigation(bool enabled)
        {
            var selectables = Object.FindObjectsOfType<Selectable>();
            foreach (var selectable in selectables)
            {
                var navigation = selectable.navigation;
                navigation.mode = enabled ? Navigation.Mode.Automatic : Navigation.Mode.None;
                selectable.navigation = navigation;
            }
        }
        
        public void SetFocusIndicator(GameObject target, bool visible)
        {
            var focusIndicator = target.GetComponent<FocusIndicator>();
            if (focusIndicator == null && visible)
            {
                focusIndicator = target.AddComponent<FocusIndicator>();
            }
            
            if (focusIndicator != null)
            {
                focusIndicator.SetVisible(visible);
            }
        }
        
        public void Cleanup()
        {
            RestoreOriginalSettings();
            
            _textComponents.Clear();
            _imageComponents.Clear();
            _colorProfiles.Clear();
            
            _isInitialized = false;
        }
        
        private void LoadColorProfiles()
        {
            // Standard color profile
            _colorProfiles["Standard"] = new ColorProfile
            {
                Primary = new Color(0.2f, 0.6f, 1.0f),
                Secondary = new Color(0.8f, 0.4f, 0.2f),
                Success = new Color(0.2f, 0.8f, 0.2f),
                Warning = new Color(1.0f, 0.8f, 0.0f),
                Error = new Color(1.0f, 0.2f, 0.2f),
                Background = new Color(0.1f, 0.1f, 0.1f),
                Text = new Color(1.0f, 1.0f, 1.0f)
            };
            
            // Protanopia (red-blind) friendly
            _colorProfiles["Protanopia"] = new ColorProfile
            {
                Primary = new Color(0.0f, 0.4f, 1.0f),
                Secondary = new Color(1.0f, 0.6f, 0.0f),
                Success = new Color(0.0f, 0.6f, 1.0f),
                Warning = new Color(1.0f, 0.8f, 0.0f),
                Error = new Color(0.6f, 0.6f, 0.6f),
                Background = new Color(0.1f, 0.1f, 0.1f),
                Text = new Color(1.0f, 1.0f, 1.0f)
            };
            
            // Deuteranopia (green-blind) friendly
            _colorProfiles["Deuteranopia"] = new ColorProfile
            {
                Primary = new Color(0.0f, 0.4f, 1.0f),
                Secondary = new Color(1.0f, 0.6f, 0.0f),
                Success = new Color(0.0f, 0.4f, 1.0f),
                Warning = new Color(1.0f, 0.8f, 0.0f),
                Error = new Color(0.8f, 0.0f, 0.8f),
                Background = new Color(0.1f, 0.1f, 0.1f),
                Text = new Color(1.0f, 1.0f, 1.0f)
            };
            
            // Tritanopia (blue-blind) friendly
            _colorProfiles["Tritanopia"] = new ColorProfile
            {
                Primary = new Color(1.0f, 0.0f, 0.4f),
                Secondary = new Color(0.0f, 0.8f, 0.4f),
                Success = new Color(0.0f, 0.8f, 0.0f),
                Warning = new Color(1.0f, 0.4f, 0.4f),
                Error = new Color(1.0f, 0.0f, 0.0f),
                Background = new Color(0.1f, 0.1f, 0.1f),
                Text = new Color(1.0f, 1.0f, 1.0f)
            };
            
            // High contrast profile
            _colorProfiles["HighContrast"] = new ColorProfile
            {
                Primary = new Color(1.0f, 1.0f, 1.0f),
                Secondary = new Color(0.8f, 0.8f, 0.8f),
                Success = new Color(1.0f, 1.0f, 1.0f),
                Warning = new Color(1.0f, 1.0f, 0.0f),
                Error = new Color(1.0f, 0.0f, 0.0f),
                Background = new Color(0.0f, 0.0f, 0.0f),
                Text = new Color(1.0f, 1.0f, 1.0f)
            };
        }
        
        private void CacheUIComponents()
        {
            _textComponents = Object.FindObjectsOfType<Text>().ToList();
            _imageComponents = Object.FindObjectsOfType<Image>().ToList();
        }
        
        private void SaveOriginalSettings()
        {
            _originalColorProfile = new ColorProfile
            {
                Primary = new Color(0.2f, 0.6f, 1.0f),
                Secondary = new Color(0.8f, 0.4f, 0.2f),
                Success = new Color(0.2f, 0.8f, 0.2f),
                Warning = new Color(1.0f, 0.8f, 0.0f),
                Error = new Color(1.0f, 0.2f, 0.2f),
                Background = new Color(0.1f, 0.1f, 0.1f),
                Text = new Color(1.0f, 1.0f, 1.0f)
            };
            
            if (_textComponents.Count > 0)
            {
                _originalFontSize = _textComponents[0].fontSize;
            }
        }
        
        private void ApplyColorblindFriendlyColors()
        {
            // Detect the most common type of colorblindness and apply appropriate profile
            // For this example, we'll use Deuteranopia (most common)
            ApplyColorProfile(_colorProfiles["Deuteranopia"]);
        }
        
        private void ApplyHighContrastColors()
        {
            ApplyColorProfile(_colorProfiles["HighContrast"]);
        }
        
        private void ApplyColorProfile(ColorProfile profile)
        {
            // Apply to images with specific tags
            foreach (var image in _imageComponents)
            {
                if (image == null) continue;
                
                if (image.CompareTag("Primary"))
                {
                    image.color = profile.Primary;
                }
                else if (image.CompareTag("Secondary"))
                {
                    image.color = profile.Secondary;
                }
                else if (image.CompareTag("Success"))
                {
                    image.color = profile.Success;
                }
                else if (image.CompareTag("Warning"))
                {
                    image.color = profile.Warning;
                }
                else if (image.CompareTag("Error"))
                {
                    image.color = profile.Error;
                }
                else if (image.CompareTag("Background"))
                {
                    image.color = profile.Background;
                }
            }
            
            // Apply to text components
            foreach (var text in _textComponents)
            {
                if (text != null)
                {
                    text.color = profile.Text;
                }
            }
        }
        
        private void RestoreOriginalColors()
        {
            if (_originalColorProfile != null)
            {
                ApplyColorProfile(_originalColorProfile);
            }
        }
        
        private void ApplyLargeTextSizes()
        {
            foreach (var text in _textComponents)
            {
                if (text != null)
                {
                    text.fontSize = Mathf.RoundToInt(_originalFontSize * 1.5f);
                }
            }
        }
        
        private void RestoreOriginalTextSizes()
        {
            foreach (var text in _textComponents)
            {
                if (text != null)
                {
                    text.fontSize = Mathf.RoundToInt(_originalFontSize);
                }
            }
        }
        
        private void ApplyUIScale()
        {
            var canvases = Object.FindObjectsOfType<Canvas>();
            foreach (var canvas in canvases)
            {
                var scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.scaleFactor = _uiScale;
                }
            }
        }
        
        private void EnableScreenReaderFeatures()
        {
            // Add ARIA-like attributes to UI elements
            var selectables = Object.FindObjectsOfType<Selectable>();
            foreach (var selectable in selectables)
            {
                var screenReaderHelper = selectable.GetComponent<ScreenReaderHelper>();
                if (screenReaderHelper == null)
                {
                    screenReaderHelper = selectable.gameObject.AddComponent<ScreenReaderHelper>();
                }
                
                screenReaderHelper.SetEnabled(true);
            }
        }
        
        private void DisableScreenReaderFeatures()
        {
            var screenReaderHelpers = Object.FindObjectsOfType<ScreenReaderHelper>();
            foreach (var helper in screenReaderHelpers)
            {
                helper.SetEnabled(false);
            }
        }
        
        private void RestoreOriginalSettings()
        {
            RestoreOriginalColors();
            RestoreOriginalTextSizes();
        }
    }
    
    // Supporting classes
    [System.Serializable]
    public class ColorProfile
    {
        public Color Primary;
        public Color Secondary;
        public Color Success;
        public Color Warning;
        public Color Error;
        public Color Background;
        public Color Text;
    }
    
    public class AccessibilityTooltip : MonoBehaviour
    {
        [SerializeField] private string _tooltipText;
        [SerializeField] private GameObject _tooltipPrefab;
        
        public void SetTooltipText(string text)
        {
            _tooltipText = text;
        }
        
        private void OnMouseEnter()
        {
            ShowTooltip();
        }
        
        private void OnMouseExit()
        {
            HideTooltip();
        }
        
        private void ShowTooltip()
        {
            if (!string.IsNullOrEmpty(_tooltipText))
            {
                // Show tooltip with text
                Debug.Log($"Tooltip: {_tooltipText}");
            }
        }
        
        private void HideTooltip()
        {
            // Hide tooltip
        }
    }
    
    public class FocusIndicator : MonoBehaviour
    {
        [SerializeField] private GameObject _focusRing;
        [SerializeField] private Color _focusColor = Color.yellow;
        
        private void Start()
        {
            if (_focusRing == null)
            {
                CreateFocusRing();
            }
        }
        
        private void CreateFocusRing()
        {
            _focusRing = new GameObject("FocusRing");
            _focusRing.transform.SetParent(transform);
            _focusRing.transform.localPosition = Vector3.zero;
            
            var image = _focusRing.AddComponent<Image>();
            image.color = _focusColor;
            image.raycastTarget = false;
            
            _focusRing.SetActive(false);
        }
        
        public void SetVisible(bool visible)
        {
            if (_focusRing != null)
            {
                _focusRing.SetActive(visible);
            }
        }
        
        private void OnSelectEnter()
        {
            SetVisible(true);
        }
        
        private void OnSelectExit()
        {
            SetVisible(false);
        }
    }
    
    public class ScreenReaderHelper : MonoBehaviour
    {
        [SerializeField] private string _ariaLabel;
        [SerializeField] private string _ariaDescription;
        [SerializeField] private bool _isEnabled = false;
        
        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
        }
        
        public void SetAriaLabel(string label)
        {
            _ariaLabel = label;
        }
        
        public void SetAriaDescription(string description)
        {
            _ariaDescription = description;
        }
        
        public void AnnounceToScreenReader(string message)
        {
            if (_isEnabled)
            {
                // In a real implementation, this would interface with platform screen readers
                Debug.Log($"Screen Reader: {message}");
            }
        }
    }
}