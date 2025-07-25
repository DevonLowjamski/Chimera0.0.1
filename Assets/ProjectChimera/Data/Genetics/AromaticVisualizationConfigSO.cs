using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Aromatic Visualization Configuration - Visual settings for aromatic gaming interface
    /// Defines terpene displays, sensory training visuals, and blending interface elements
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Aromatic Visualization Config", menuName = "Project Chimera/Gaming/Aromatic Visualization Config")]
    public class AromaticVisualizationConfigSO : ChimeraDataSO
    {
        [Header("Terpene Display Settings")]
        public Color MonoterpeneColor = Color.yellow;
        public Color SesquiterpeneColor = Color.orange;
        public Color DiterpeneColor = Color.red;
        public Color AlcoholColor = Color.blue;
        public Color EsterColor = Color.green;
        
        [Header("Aromatic Wheel Configuration")]
        public List<AromaticWheelSegment> WheelSegments = new List<AromaticWheelSegment>();
        public float WheelRadius = 200f;
        public bool EnableWheelAnimation = true;
        
        [Header("Sensory Interface")]
        public List<SensoryVisualConfig> SensoryConfigurations = new List<SensoryVisualConfig>();
        public List<BlendingVisualConfig> BlendingConfigurations = new List<BlendingVisualConfig>();
        
        [Header("Animation Settings")]
        public float TerpeneAnimationSpeed = 1.0f;
        public bool EnableAromaticAnimations = true;
        public AnimationCurve AromaticTransitionCurve;
        
        #region Runtime Methods
        
        public AromaticWheelSegment GetWheelSegment(TerpeneCategory category)
        {
            return WheelSegments.Find(s => s.TerpeneCategory == category);
        }
        
        public SensoryVisualConfig GetSensoryVisual(SensoryTrainingType trainingType)
        {
            return SensoryConfigurations.Find(s => s.TrainingType == trainingType);
        }
        
        public BlendingVisualConfig GetBlendingVisual(BlendingChallengeType challengeType)
        {
            return BlendingConfigurations.Find(b => b.ChallengeType == challengeType);
        }
        
        #endregion
    }
    
    [System.Serializable]
    public class AromaticWheelSegment
    {
        public string SegmentName;
        public TerpeneCategory TerpeneCategory;
        public Color SegmentColor = Color.white;
        public float StartAngle;
        public float EndAngle;
        public List<string> RepresentativeTerpenes = new List<string>();
        public Sprite SegmentIcon;
        public string Description;
    }
    
    [System.Serializable]
    public class SensoryVisualConfig
    {
        public string ConfigName;
        public SensoryTrainingType TrainingType;
        public Sprite TrainingIcon;
        public Color TrainingColor = Color.white;
        public List<SensoryVisualElement> VisualElements = new List<SensoryVisualElement>();
        public string Description;
    }
    
    [System.Serializable]
    public class BlendingVisualConfig
    {
        public string ConfigName;
        public BlendingChallengeType ChallengeType;
        public Sprite BlendingIcon;
        public Color BlendingColor = Color.white;
        public List<BlendingVisualElement> VisualElements = new List<BlendingVisualElement>();
        public string Description;
    }
    
    [System.Serializable]
    public class SensoryVisualElement
    {
        public string ElementName;
        public SensoryElementType ElementType;
        public Vector2 Position;
        public Vector2 Size;
        public Color ElementColor = Color.white;
        public Sprite ElementSprite;
        public AnimationClip ElementAnimation;
    }
    
    [System.Serializable]
    public class BlendingVisualElement
    {
        public string ElementName;
        public BlendingElementType ElementType;
        public Vector2 Position;
        public Vector2 Size;
        public Color ElementColor = Color.white;
        public Sprite ElementSprite;
        public float ConcentrationLevel;
    }
    
    public enum SensoryElementType
    {
        TerpeneIdentifier,
        ConcentrationMeter,
        QualityIndicator,
        TimerDisplay,
        ScoreDisplay,
        FeedbackIcon,
        ProgressBar
    }
    
    public enum BlendingElementType
    {
        TerpeneContainer,
        BlendingVessel,
        ConcentrationSlider,
        MixingAnimation,
        ResultDisplay,
        SynergyIndicator,
        QualityMeter
    }
}