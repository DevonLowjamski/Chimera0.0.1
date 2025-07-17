using UnityEngine;
using System;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation; // For cultivation data types
using ProjectChimera.Data.Events; // For event data types
// Resolve ambiguous references with explicit namespace aliases
using AutomationUnlockEventData = ProjectChimera.Core.Events.AutomationUnlockEventData;
using SkillNodeEventData = ProjectChimera.Data.Cultivation.SkillNodeEventData;
using PlayerChoiceEventData = ProjectChimera.Events.PlayerChoiceEventData;
using PlantCareEventData = ProjectChimera.Core.Events.PlantCareEventData;
using TimeScaleEventData = ProjectChimera.Events.TimeScaleEventData;

namespace ProjectChimera.Core.Events
{
    /// <summary>
    /// Cultivation Gaming Events - ScriptableObject-based event channels for cultivation gaming systems
    /// Provides decoupled communication between Enhanced Cultivation Gaming System components
    /// </summary>
    
    #region Plant Care Events
    
    [CreateAssetMenu(fileName = "Plant_Care_Performed_Event", menuName = "Project Chimera/Events/Cultivation/Plant Care Performed")]
    public class PlantCarePerformedEventSO : GameEventChannelSO
    {
        public System.Action<PlantCareEventData> OnPlantCarePerformed;
        
        public override void RaiseEvent(object data)
        {
            if (data is PlantCareEventData careData)
            {
                OnPlantCarePerformed?.Invoke(careData);
                base.RaiseEvent(data);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "Care_Quality_Improved_Event", menuName = "Project Chimera/Events/Cultivation/Care Quality Improved")]
    public class CareQualityImprovedEventSO : GameEventChannelSO
    {
        public System.Action<CareQualityEventData> OnCareQualityImproved;
        
        public override void RaiseEvent(object data)
        {
            if (data is CareQualityEventData qualityData)
            {
                OnCareQualityImproved?.Invoke(qualityData);
                base.RaiseEvent(data);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "Plant_Response_Triggered_Event", menuName = "Project Chimera/Events/Cultivation/Plant Response Triggered")]
    public class PlantResponseTriggeredEventSO : GameEventChannelSO
    {
        public System.Action<PlantResponseEventData> OnPlantResponseTriggered;
        
        public override void RaiseEvent(object data)
        {
            if (data is PlantResponseEventData responseData)
            {
                OnPlantResponseTriggered?.Invoke(responseData);
                base.RaiseEvent(data);
            }
        }
    }
    
    #endregion
    
    #region Automation Events
    
    [CreateAssetMenu(fileName = "Automation_Unlocked_Event", menuName = "Project Chimera/Events/Cultivation/Automation Unlocked")]
    public class AutomationUnlockedEventSO : GameEventChannelSO
    {
        public System.Action<AutomationUnlockEventData> OnAutomationUnlocked;
        
        public override void RaiseEvent(object data)
        {
            if (data is AutomationUnlockEventData unlockData)
            {
                OnAutomationUnlocked?.Invoke(unlockData);
                base.RaiseEvent(data);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "Manual_Task_Burden_Increased_Event", menuName = "Project Chimera/Events/Cultivation/Manual Task Burden Increased")]
    public class ManualTaskBurdenIncreasedEventSO : GameEventChannelSO
    {
        public System.Action<TaskBurdenEventData> OnManualTaskBurdenIncreased;
        
        public override void RaiseEvent(object data)
        {
            if (data is TaskBurdenEventData burdenData)
            {
                OnManualTaskBurdenIncreased?.Invoke(burdenData);
                base.RaiseEvent(data);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "Automation_Benefit_Realized_Event", menuName = "Project Chimera/Events/Cultivation/Automation Benefit Realized")]
    public class AutomationBenefitRealizedEventSO : GameEventChannelSO
    {
        public System.Action<AutomationBenefitEventData> OnAutomationBenefitRealized;
        
        public override void RaiseEvent(object data)
        {
            if (data is AutomationBenefitEventData benefitData)
            {
                OnAutomationBenefitRealized?.Invoke(benefitData);
                base.RaiseEvent(data);
            }
        }
    }
    
    #endregion
    
    #region Skill Tree Events
    
    [CreateAssetMenu(fileName = "Skill_Node_Unlocked_Event", menuName = "Project Chimera/Events/Cultivation/Skill Node Unlocked")]
    public class SkillNodeUnlockedEventSO : GameEventChannelSO
    {
        public System.Action<SkillNodeEventData> OnSkillNodeUnlocked;
        
        public override void RaiseEvent(object data)
        {
            if (data is SkillNodeEventData nodeData)
            {
                OnSkillNodeUnlocked?.Invoke(nodeData);
                base.RaiseEvent(data);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "Skill_Tree_Visualization_Updated_Event", menuName = "Project Chimera/Events/Cultivation/Skill Tree Visualization Updated")]
    public class SkillTreeVisualizationUpdatedEventSO : GameEventChannelSO
    {
        public System.Action<TreeVisualizationEventData> OnSkillTreeVisualizationUpdated;
        
        public override void RaiseEvent(object data)
        {
            if (data is TreeVisualizationEventData visualData)
            {
                OnSkillTreeVisualizationUpdated?.Invoke(visualData);
                base.RaiseEvent(data);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "Concept_Introduced_Event", menuName = "Project Chimera/Events/Cultivation/Concept Introduced")]
    public class ConceptIntroducedEventSO : GameEventChannelSO
    {
        public System.Action<ConceptIntroductionEventData> OnConceptIntroduced;
        
        public override void RaiseEvent(object data)
        {
            if (data is ConceptIntroductionEventData conceptData)
            {
                OnConceptIntroduced?.Invoke(conceptData);
                base.RaiseEvent(data);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "Equipment_Unlocked_Event", menuName = "Project Chimera/Events/Cultivation/Equipment Unlocked")]
    public class EquipmentUnlockedEventSO : GameEventChannelSO
    {
        public System.Action<EquipmentUnlockEventData> OnEquipmentUnlocked;
        
        public override void RaiseEvent(object data)
        {
            if (data is EquipmentUnlockEventData equipmentData)
            {
                OnEquipmentUnlocked?.Invoke(equipmentData);
                base.RaiseEvent(data);
            }
        }
    }
    
    #endregion
    
    #region Time Acceleration Events
    
    [CreateAssetMenu(fileName = "Time_Scale_Changed_Event", menuName = "Project Chimera/Events/Cultivation/Time Scale Changed")]
    public class TimeScaleChangedEventSO : GameEventChannelSO
    {
        public System.Action<TimeScaleEventData> OnTimeScaleChanged;
        
        public override void RaiseEvent(object data)
        {
            if (data is TimeScaleEventData timeData)
            {
                OnTimeScaleChanged?.Invoke(timeData);
                base.RaiseEvent(data);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "Time_Acceleration_Lock_Activated_Event", menuName = "Project Chimera/Events/Cultivation/Time Acceleration Lock Activated")]
    public class TimeAccelerationLockActivatedEventSO : GameEventChannelSO
    {
        public System.Action<TimeAccelerationLockEventData> OnTimeAccelerationLockActivated;
        
        public override void RaiseEvent(object data)
        {
            if (data is TimeAccelerationLockEventData lockData)
            {
                OnTimeAccelerationLockActivated?.Invoke(lockData);
                base.RaiseEvent(data);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "Time_Transition_Completed_Event", menuName = "Project Chimera/Events/Cultivation/Time Transition Completed")]
    public class TimeTransitionCompletedEventSO : GameEventChannelSO
    {
        public System.Action<TimeTransitionEventData> OnTimeTransitionCompleted;
        
        public override void RaiseEvent(object data)
        {
            if (data is TimeTransitionEventData transitionData)
            {
                OnTimeTransitionCompleted?.Invoke(transitionData);
                base.RaiseEvent(data);
            }
        }
    }
    
    #endregion
    
    #region Player Agency Events
    
    [CreateAssetMenu(fileName = "Player_Choice_Made_Event", menuName = "Project Chimera/Events/Cultivation/Player Choice Made")]
    public class PlayerChoiceMadeEventSO : GameEventChannelSO
    {
        public System.Action<PlayerChoiceEventData> OnPlayerChoiceMade;
        
        public override void RaiseEvent(object data)
        {
            if (data is PlayerChoiceEventData choiceData)
            {
                OnPlayerChoiceMade?.Invoke(choiceData);
                base.RaiseEvent(data);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "Cultivation_Path_Selected_Event", menuName = "Project Chimera/Events/Cultivation/Cultivation Path Selected")]
    public class CultivationPathSelectedEventSO : GameEventChannelSO
    {
        public System.Action<CultivationPathGamingEventData> OnCultivationPathSelected;
        
        public override void RaiseEvent(object data)
        {
            if (data is CultivationPathGamingEventData pathData)
            {
                OnCultivationPathSelected?.Invoke(pathData);
                base.RaiseEvent(data);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "Facility_Design_Completed_Event", menuName = "Project Chimera/Events/Cultivation/Facility Design Completed")]
    public class FacilityDesignCompletedEventSO : GameEventChannelSO
    {
        public System.Action<FacilityDesignGamingEventData> OnFacilityDesignCompleted;
        
        public override void RaiseEvent(object data)
        {
            if (data is FacilityDesignGamingEventData designData)
            {
                OnFacilityDesignCompleted?.Invoke(designData);
                base.RaiseEvent(data);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "Creative_Solution_Implemented_Event", menuName = "Project Chimera/Events/Cultivation/Creative Solution Implemented")]
    public class CreativeSolutionImplementedEventSO : GameEventChannelSO
    {
        public System.Action<CreativeSolutionEventData> OnCreativeSolutionImplemented;
        
        public override void RaiseEvent(object data)
        {
            if (data is CreativeSolutionEventData solutionData)
            {
                OnCreativeSolutionImplemented?.Invoke(solutionData);
                base.RaiseEvent(data);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "Choice_Consequence_Realized_Event", menuName = "Project Chimera/Events/Cultivation/Choice Consequence Realized")]
    public class ChoiceConsequenceRealizedEventSO : GameEventChannelSO
    {
        public System.Action<ConsequenceEventData> OnChoiceConsequenceRealized;
        
        public override void RaiseEvent(object data)
        {
            if (data is ConsequenceEventData consequenceData)
            {
                OnChoiceConsequenceRealized?.Invoke(consequenceData);
                base.RaiseEvent(data);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "Expression_Value_Increased_Event", menuName = "Project Chimera/Events/Cultivation/Expression Value Increased")]
    public class ExpressionValueIncreasedEventSO : GameEventChannelSO
    {
        public System.Action<ExpressionLevelEventData> OnExpressionValueIncreased;
        
        public override void RaiseEvent(object data)
        {
            if (data is ExpressionLevelEventData expressionData)
            {
                OnExpressionValueIncreased?.Invoke(expressionData);
                base.RaiseEvent(data);
            }
        }
    }
    
    #endregion
    
    #region Gaming Performance Events
    
    [CreateAssetMenu(fileName = "Gaming_Session_Started_Event", menuName = "Project Chimera/Events/Cultivation/Gaming Session Started")]
    public class GamingSessionStartedEventSO : GameEventChannelSO
    {
        public System.Action<GamingSessionEventData> OnGamingSessionStarted;
        
        public override void RaiseEvent(object data)
        {
            if (data is GamingSessionEventData sessionData)
            {
                OnGamingSessionStarted?.Invoke(sessionData);
                base.RaiseEvent(data);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "Gaming_Session_Ended_Event", menuName = "Project Chimera/Events/Cultivation/Gaming Session Ended")]
    public class GamingSessionEndedEventSO : GameEventChannelSO
    {
        public System.Action<GamingSessionEventData> OnGamingSessionEnded;
        
        public override void RaiseEvent(object data)
        {
            if (data is GamingSessionEventData sessionData)
            {
                OnGamingSessionEnded?.Invoke(sessionData);
                base.RaiseEvent(data);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "Gaming_Milestone_Achieved_Event", menuName = "Project Chimera/Events/Cultivation/Gaming Milestone Achieved")]
    public class GamingMilestoneAchievedEventSO : GameEventChannelSO
    {
        public System.Action<GamingMilestoneEventData> OnGamingMilestoneAchieved;
        
        public override void RaiseEvent(object data)
        {
            if (data is GamingMilestoneEventData milestoneData)
            {
                OnGamingMilestoneAchieved?.Invoke(milestoneData);
                base.RaiseEvent(data);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "Engagement_Level_Changed_Event", menuName = "Project Chimera/Events/Cultivation/Engagement Level Changed")]
    public class EngagementLevelChangedEventSO : GameEventChannelSO
    {
        public System.Action<EngagementEventData> OnEngagementLevelChanged;
        
        public override void RaiseEvent(object data)
        {
            if (data is EngagementEventData engagementData)
            {
                OnEngagementLevelChanged?.Invoke(engagementData);
                base.RaiseEvent(data);
            }
        }
    }
    
    #endregion
    
    #region Integration Events
    
    [CreateAssetMenu(fileName = "Skill_Progression_Triggered_Event", menuName = "Project Chimera/Events/Cultivation/Skill Progression Triggered")]
    public class SkillProgressionTriggeredEventSO : GameEventChannelSO
    {
        public System.Action<SkillProgressionEventData> OnSkillProgressionTriggered;
        
        public override void RaiseEvent(object data)
        {
            if (data is SkillProgressionEventData progressionData)
            {
                OnSkillProgressionTriggered?.Invoke(progressionData);
                base.RaiseEvent(data);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "Interdependency_Bonus_Applied_Event", menuName = "Project Chimera/Events/Cultivation/Interdependency Bonus Applied")]
    public class InterdependencyBonusAppliedEventSO : GameEventChannelSO
    {
        public System.Action<InterdependencyBonusEventData> OnInterdependencyBonusApplied;
        
        public override void RaiseEvent(object data)
        {
            if (data is InterdependencyBonusEventData bonusData)
            {
                OnInterdependencyBonusApplied?.Invoke(bonusData);
                base.RaiseEvent(data);
            }
        }
    }
    
    #endregion
}