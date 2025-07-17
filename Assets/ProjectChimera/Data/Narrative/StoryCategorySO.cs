using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Narrative;

namespace ProjectChimera.Data.Narrative
{
    /// <summary>
    /// Story Category ScriptableObject for Project Chimera narrative system.
    /// Represents categories for organizing and filtering story content.
    /// </summary>
    [CreateAssetMenu(fileName = "New Story Category", menuName = "Project Chimera/Narrative/Story Category", order = 205)]
    public class StoryCategorySO : ChimeraDataSO
    {
        [Header("Category Configuration")]
        [SerializeField] private string _categoryId;
        [SerializeField] private string _categoryName;
        [SerializeField] private string _description;
        [SerializeField] private Color _categoryColor = Color.white;
        
        [Header("Category Properties")]
        [SerializeField] private int _displayOrder = 0;
        [SerializeField] private bool _isMainCategory = false;
        [SerializeField] private bool _isVisible = true;
        [SerializeField] private Sprite _categoryIcon;
        
        [Header("Category Metadata")]
        [SerializeField] private List<string> _parentCategoryIds = new List<string>();
        [SerializeField] private List<string> _childCategoryIds = new List<string>();
        [SerializeField] private List<string> _relatedCategoryIds = new List<string>();
        
        [Header("Educational Content")]
        [SerializeField] private bool _hasEducationalContent = false;
        [SerializeField] private List<string> _educationalTags = new List<string>();
        [SerializeField] private string _skillArea;
        [SerializeField] private int _recommendedLevel = 1;
        
        // Properties
        public string CategoryId => _categoryId;
        public string CategoryName => _categoryName;
        public string Description => _description;
        public Color CategoryColor => _categoryColor;
        public int DisplayOrder => _displayOrder;
        public bool IsMainCategory => _isMainCategory;
        public bool IsVisible => _isVisible;
        public Sprite CategoryIcon => _categoryIcon;
        public List<string> ParentCategoryIds => _parentCategoryIds;
        public List<string> ChildCategoryIds => _childCategoryIds;
        public List<string> RelatedCategoryIds => _relatedCategoryIds;
        public bool HasEducationalContent => _hasEducationalContent;
        public List<string> EducationalTags => _educationalTags;
        public string SkillArea => _skillArea;
        public int RecommendedLevel => _recommendedLevel;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (string.IsNullOrEmpty(_categoryId))
            {
                _categoryId = name.Replace(" ", "").Replace("Category", "").ToLower();
            }
            
            if (string.IsNullOrEmpty(_categoryName))
            {
                _categoryName = name.Replace("Category", "").Trim();
            }
        }
        
        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(_categoryId))
            {
                LogError("Category ID cannot be empty");
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(_categoryName))
            {
                LogError("Category name cannot be empty");
                isValid = false;
            }
            
            if (_recommendedLevel < 1)
            {
                LogError("Recommended level must be at least 1");
                isValid = false;
            }
            
            return isValid;
        }
        
        /// <summary>
        /// Check if this category is a child of the specified parent category
        /// </summary>
        public bool IsChildOf(string parentCategoryId)
        {
            return _parentCategoryIds.Contains(parentCategoryId);
        }
        
        /// <summary>
        /// Check if this category is a parent of the specified child category
        /// </summary>
        public bool IsParentOf(string childCategoryId)
        {
            return _childCategoryIds.Contains(childCategoryId);
        }
        
        /// <summary>
        /// Check if this category is related to the specified category
        /// </summary>
        public bool IsRelatedTo(string otherCategoryId)
        {
            return _relatedCategoryIds.Contains(otherCategoryId);
        }
        
        /// <summary>
        /// Get the hierarchy level of this category (0 for root categories)
        /// </summary>
        public int GetHierarchyLevel()
        {
            return _parentCategoryIds.Count;
        }
        
        /// <summary>
        /// Check if this category is appropriate for the given player level
        /// </summary>
        public bool IsAppropriateForLevel(int playerLevel)
        {
            // Allow some flexibility around the recommended level
            int levelVariance = 2;
            return playerLevel >= (_recommendedLevel - levelVariance);
        }
    }
} 