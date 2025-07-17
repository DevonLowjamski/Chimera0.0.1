using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Scientific Gaming UI Configuration - UI settings for Enhanced Scientific Gaming System v2.0
    /// Defines interface layouts, visual themes, and interaction patterns
    /// </summary>
    [CreateAssetMenu(fileName = "New Scientific Gaming UI Config", menuName = "Project Chimera/Gaming/Scientific Gaming UI Config")]
    public class ScientificGamingUIConfigSO : ChimeraDataSO
    {
        [Header("UI Theme Configuration")]
        public Color PrimaryColor = Color.blue;
        public Color SecondaryColor = Color.green;
        public Color AccentColor = Color.yellow;
        public Color BackgroundColor = Color.black;
        
        [Header("Layout Settings")]
        public List<UILayoutConfig> LayoutConfigurations = new List<UILayoutConfig>();
        public List<UIAnimationConfig> AnimationConfigurations = new List<UIAnimationConfig>();
        
        [Header("Interaction Settings")]
        public bool EnableHapticFeedback = true;
        public bool EnableSoundEffects = true;
        public float AnimationSpeed = 1.0f;
        
        #region Runtime Methods
        
        public UILayoutConfig GetLayoutConfig(UILayoutType layoutType)
        {
            return LayoutConfigurations.Find(l => l.LayoutType == layoutType);
        }
        
        public UIAnimationConfig GetAnimationConfig(UIAnimationType animationType)
        {
            return AnimationConfigurations.Find(a => a.AnimationType == animationType);
        }
        
        #endregion
    }
    
    [System.Serializable]
    public class UILayoutConfig
    {
        public string LayoutName;
        public UILayoutType LayoutType;
        public Vector2 PreferredSize;
        public UIAnchorType AnchorType;
        public string Description;
    }
    
    [System.Serializable]
    public class UIAnimationConfig
    {
        public string AnimationName;
        public UIAnimationType AnimationType;
        public float Duration = 0.3f;
        public AnimationCurve AnimationCurve;
        public string Description;
    }
    
    public enum UILayoutType
    {
        MainMenu,
        SkillTree,
        AchievementPanel,
        ProgressionView,
        CompetitionInterface,
        CommunityPanel
    }
    
    public enum UIAnimationType
    {
        FadeIn,
        FadeOut,
        SlideIn,
        SlideOut,
        Scale,
        Pulse,
        Highlight
    }
    
    public enum UIAnchorType
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
}