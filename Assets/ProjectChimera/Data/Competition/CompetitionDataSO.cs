using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Data.Competition
{
    /// <summary>
    /// Competition data types and enums for Phase 4.3 Competition System
    /// </summary>
    
    // Core Competition Enums
    public enum CompetitionType
    {
        Local, Regional, National, International, Invitational, Online
    }
    
    public enum CompetitionTier
    {
        Amateur, Professional, Elite, Championship, WorldClass
    }
    
    public enum CompetitorLevel
    {
        Novice, Intermediate, Advanced, Expert, Master, Champion
    }
    
    public enum CategoryType
    {
        Indica, Sativa, Hybrid, CBD, Autoflower, Outdoor, Indoor, Hydroponic
    }
    
    // Base ScriptableObject for competition data
    [CreateAssetMenu(fileName = "New Competition Data", menuName = "Project Chimera/Competition/Competition Data")]
    public class CompetitionDataSO : ScriptableObject
    {
        [Header("Competition Information")]
        public string CompetitionName;
        public CompetitionType Type;
        public CompetitionTier Tier;
        public string Description;
        public Sprite CompetitionLogo;
        
        [Header("Categories")]
        public List<CategoryType> AvailableCategories;
        
        [Header("Rules and Requirements")]
        public int MaxEntriesPerCategory = 3;
        public float EntryFee = 100f;
        public int MinimumPlantAge = 60; // days
        public float MinimumYield = 10f; // grams
    }
}