using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// ScriptableObject representing event categories for organizing and filtering events.
    /// </summary>
    [CreateAssetMenu(fileName = "New Event Category", menuName = "Project Chimera/Events/Event Category", order = 106)]
    public class EventCategorySO : ChimeraDataSO
    {
        [Header("Category Identity")]
        [SerializeField] private string _categoryId;
        [SerializeField] private string _categoryName;
        [SerializeField] private string _description;
        [SerializeField] private Color _categoryColor = Color.white;
        
        [Header("Category Properties")]
        [SerializeField] private List<string> _eventIds = new List<string>();
        [SerializeField] private bool _isActiveCategory = true;
        [SerializeField] private int _displayOrder = 0;
        
        // Properties
        public string CategoryId => _categoryId;
        public string CategoryName => _categoryName;
        public string Description => _description;
        public Color CategoryColor => _categoryColor;
        public IReadOnlyList<string> EventIds => _eventIds;
        public bool IsActiveCategory => _isActiveCategory;
        public int DisplayOrder => _displayOrder;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (string.IsNullOrEmpty(_categoryId))
            {
                _categoryId = $"category_{name.ToLower().Replace(" ", "_")}";
            }
        }
    }
} 