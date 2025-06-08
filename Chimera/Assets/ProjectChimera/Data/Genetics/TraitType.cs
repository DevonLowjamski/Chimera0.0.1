using UnityEngine;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Enumeration of quantitative traits that can be bred and selected for in cannabis cultivation.
    /// These traits are influenced by genetics, environment, and their interactions (GxE).
    /// </summary>
    public enum TraitType
    {
        // Yield and Biomass Traits
        Yield,                   // Simplified yield reference (same as TotalYield)
        TotalYield,              // Overall plant yield (grams)
        FlowerYield,             // Flower-specific yield
        BiomassProduction,       // Total plant biomass
        HarvestIndex,            // Ratio of flower to total biomass
        Quality,                 // Overall quality metric
        
        // Plant Morphology Traits
        PlantHeight,             // Final plant height
        InternodalSpacing,       // Distance between nodes
        BranchingDensity,        // Number of branches
        LeafSize,                // Average leaf size
        StemThickness,           // Main stem diameter
        
        // Flowering and Development Traits
        FloweringTime,           // Days from seed to flower
        MaturationTime,          // Time to full maturity
        VegetativeGrowthRate,    // Growth rate during veg stage
        FlowerDevelopmentRate,   // Rate of flower development
        
        // Chemical Profile Traits
        Potency,                 // General potency reference (same as THCPotency)
        THCPotency,              // THC percentage
        CBDContent,              // CBD percentage
        CBGContent,              // CBG percentage
        TerpeneProduction,       // Total terpene content
        FlavonoidContent,        // Flavonoid levels
        
        // Stress Tolerance Traits
        HeatTolerance,           // Tolerance to high temperatures
        ColdTolerance,           // Tolerance to low temperatures
        DroughtTolerance,        // Water stress tolerance
        NutrientStressTolerance, // Nutrient deficiency tolerance
        LightStressTolerance,    // High light intensity tolerance
        
        // Disease and Pest Resistance
        DiseaseResistance,       // General disease resistance
        PowderyMildewResistance, // Resistance to powdery mildew
        BotrytisResistance,      // Resistance to bud rot
        RootRotResistance,       // Resistance to root diseases
        AphidResistance,         // Resistance to aphids
        SpiderMiteResistance,    // Resistance to spider mites
        
        // Environmental Adaptation
        HumidityTolerance,       // Tolerance to high/low humidity
        pHAdaptability,          // Adaptation to pH variations
        SalinityTolerance,       // Salt stress tolerance
        CO2ResponseEfficiency,   // Efficiency under elevated CO2
        
        // Quality Traits
        BudDensity,              // Flower compactness
        TrichomeProduction,      // Trichome density
        AromaIntensity,          // Smell/terpene intensity
        FlavorProfile,           // Taste characteristics
        ShelfLife,               // Post-harvest stability
        
        // Resource Use Efficiency
        NitrogenUseEfficiency,   // Nitrogen uptake efficiency
        PhosphorusUseEfficiency, // Phosphorus uptake efficiency
        WaterUseEfficiency,      // Water consumption efficiency
        LightUseEfficiency,      // Photosynthetic efficiency
        
        // Specialized Traits
        AutofloweringTendency,   // Day-neutral flowering
        SexStability,            // Hermaphrodite resistance
        SeedProduction,          // Seed yield for breeding
        ClonabilityIndex,        // Ease of vegetative propagation
        CuringResponse,          // Response to post-harvest curing
        
        // Hybrid Vigor Traits
        HeterosisEffect,         // Overall hybrid vigor
        GrowthVigor,             // Vegetative growth vigor
        YieldHeterosis,          // Yield improvement in hybrids
        StressRecovery           // Recovery from stress events
    }
} 