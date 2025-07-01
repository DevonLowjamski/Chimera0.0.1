using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data
{
    /// <summary>
    /// Tree Visualization Configuration - Configuration for skill tree and progression visualization
    /// Defines visual styles, layouts, animations, and UI presentation
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Tree Visualization Config", menuName = "Project Chimera/Genetics/Tree Visualization Config")]
    public class TreeVisualizationConfigSO : ChimeraConfigSO
    {
        [Header("Visual Themes")]
        public List<TreeVisualizationTheme> VisualizationThemes = new List<TreeVisualizationTheme>();
        
        [Header("Node Visualization")]
        public NodeVisualizationSettings NodeSettings;
        
        [Header("Connection Visualization")]
        public ConnectionVisualizationSettings ConnectionSettings;
        
        [Header("Animation Settings")]
        public TreeAnimationSettings AnimationSettings;
        
        [Header("Layout Configuration")]
        public TreeLayoutSettings LayoutSettings;
        
        public TreeVisualizationTheme GetTheme(VisualizationThemeType themeType)
        {
            return VisualizationThemes.Find(t => t.ThemeType == themeType);
        }
        
        public Color GetNodeColor(NodeState nodeState, VisualizationThemeType theme)
        {
            var themeData = GetTheme(theme);
            if (themeData == null) return Color.white;
            
            return nodeState switch
            {
                NodeState.Locked => themeData.LockedNodeColor,
                NodeState.Available => themeData.AvailableNodeColor,
                NodeState.Unlocked => themeData.UnlockedNodeColor,
                NodeState.Mastered => themeData.MasteredNodeColor,
                _ => Color.white
            };
        }
    }
    
    [System.Serializable]
    public class TreeVisualizationTheme
    {
        public string ThemeName;
        public VisualizationThemeType ThemeType;
        
        [Header("Node Colors")]
        public Color LockedNodeColor = Color.gray;
        public Color AvailableNodeColor = Color.yellow;
        public Color UnlockedNodeColor = Color.green;
        public Color MasteredNodeColor = Color.gold;
        
        [Header("Connection Colors")]
        public Color ActiveConnectionColor = Color.white;
        public Color InactiveConnectionColor = Color.gray;
        public Color SynergyConnectionColor = Color.cyan;
        
        [Header("Background")]
        public Color BackgroundColor = Color.black;
        public Sprite BackgroundTexture;
        
        [Header("UI Elements")]
        public Color TextColor = Color.white;
        public Color AccentColor = Color.blue;
        public Font ThemeFont;
    }
    
    [System.Serializable]
    public class NodeVisualizationSettings
    {
        [Header("Node Sizing")]
        [Range(10f, 200f)] public float BaseNodeSize = 50f;
        [Range(0.1f, 3.0f)] public float NodeSizeVariation = 1.5f;
        [Range(0.1f, 2.0f)] public float MasteredNodeScale = 1.2f;
        
        [Header("Node Effects")]
        public bool EnableGlowEffect = true;
        public bool EnablePulseAnimation = true;
        [Range(0.1f, 5.0f)] public float GlowIntensity = 1.0f;
        [Range(0.5f, 3.0f)] public float PulseSpeed = 1.0f;
        
        [Header("Icons and Sprites")]
        public List<NodeIconSet> IconSets = new List<NodeIconSet>();
        public bool UseProgressBars = true;
        public bool ShowLevelNumbers = true;
    }
    
    [System.Serializable]
    public class ConnectionVisualizationSettings
    {
        [Header("Line Settings")]
        [Range(1f, 20f)] public float ConnectionLineWidth = 3f;
        [Range(0.1f, 1.0f)] public float ConnectionOpacity = 0.8f;
        public LineStyle ConnectionStyle = LineStyle.Straight;
        
        [Header("Animation")]
        public bool EnableFlowAnimation = true;
        [Range(0.1f, 5.0f)] public float FlowSpeed = 1.0f;
        public bool EnablePulseOnActivation = true;
        
        [Header("Interactive Effects")]
        public bool HighlightOnHover = true;
        public Color HoverHighlightColor = Color.white;
        [Range(1.1f, 3.0f)] public float HoverScaleMultiplier = 1.5f;
    }
    
    [System.Serializable]
    public class TreeAnimationSettings
    {
        [Header("Transition Animations")]
        [Range(0.1f, 2.0f)] public float NodeUnlockDuration = 0.5f;
        [Range(0.1f, 2.0f)] public float TreeRevealDuration = 1.0f;
        public AnimationCurve UnlockAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Idle Animations")]
        public bool EnableIdleAnimations = true;
        [Range(0.1f, 5.0f)] public float IdleAnimationSpeed = 1.0f;
        [Range(0.01f, 0.5f)] public float IdleAnimationIntensity = 0.1f;
        
        [Header("Special Effects")]
        public bool EnableUnlockParticles = true;
        public bool EnableMasteryEffects = true;
        public bool EnableSynergyVisuals = true;
    }
    
    [System.Serializable]
    public class TreeLayoutSettings
    {
        [Header("Grid Layout")]
        [Range(50f, 500f)] public float NodeSpacing = 100f;
        [Range(50f, 500f)] public float TierSpacing = 150f;
        public LayoutType LayoutType = LayoutType.Hierarchical;
        
        [Header("Auto Layout")]
        public bool EnableAutoLayout = true;
        public bool OptimizeForScreenSize = true;
        [Range(0.1f, 2.0f)] public float LayoutDensity = 1.0f;
        
        [Header("Navigation")]
        public bool EnablePanAndZoom = true;
        [Range(0.1f, 5.0f)] public float MinZoom = 0.5f;
        [Range(1f, 10f)] public float MaxZoom = 3.0f;
        public bool EnableMiniMap = true;
    }
    
    [System.Serializable]
    public class NodeIconSet
    {
        public SkillCategory Category;
        public List<Sprite> Icons = new List<Sprite>();
        public Sprite DefaultIcon;
    }
    
    public enum VisualizationThemeType
    {
        Default,
        Dark,
        Light,
        Organic,
        Technical,
        Elegant,
        Vibrant,
        Minimal
    }
    
    public enum NodeState
    {
        Locked,
        Available,
        Unlocked,
        Mastered
    }
    
    public enum LineStyle
    {
        Straight,
        Curved,
        Organic,
        Circuit,
        Dotted,
        Animated
    }
    
    public enum LayoutType
    {
        Hierarchical,
        Radial,
        Organic,
        Grid,
        Circular,
        Custom
    }
}