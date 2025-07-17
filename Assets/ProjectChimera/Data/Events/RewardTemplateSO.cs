using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// ScriptableObject representing reward templates for events.
    /// </summary>
    [CreateAssetMenu(fileName = "New Reward Template", menuName = "Project Chimera/Events/Reward Template", order = 107)]
    public class RewardTemplateSO : ChimeraDataSO
    {
        [Header("Reward Identity")]
        [SerializeField] private string _rewardId;
        [SerializeField] private string _rewardName;
        [SerializeField] private string _description;
        [SerializeField] private RewardType _rewardType;
        
        [Header("Reward Properties")]
        [SerializeField] private int _rewardValue = 100;
        [SerializeField] private RarityTier _rarity = RarityTier.Common;
        [SerializeField] private bool _isStackable = true;
        [SerializeField] private int _maxQuantity = 1;
        
        // Properties
        public string RewardId => _rewardId;
        public string RewardName => _rewardName;
        public string Description => _description;
        public RewardType RewardType => _rewardType;
        public int RewardValue => _rewardValue;
        public RarityTier Rarity => _rarity;
        public bool IsStackable => _isStackable;
        public int MaxQuantity => _maxQuantity;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (string.IsNullOrEmpty(_rewardId))
            {
                _rewardId = $"reward_{name.ToLower().Replace(" ", "_")}";
            }
        }
    }
    
    public enum RewardType
    {
        Currency,
        Experience,
        Item,
        Strain,
        Equipment,
        Decoration,
        Achievement,
        Title
    }
} 