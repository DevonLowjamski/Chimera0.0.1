using UnityEngine;
using System.Collections.Generic;

namespace ProjectChimera.Data.Environment
{
    /// <summary>
    /// Sophisticated light spectrum analysis for cannabis cultivation.
    /// Models the complete electromagnetic spectrum and its effects on plant physiology,
    /// cannabinoid production, and morphological development.
    /// </summary>
    [System.Serializable]
    public class LightSpectrumData
    {
        [Header("Photosynthetically Active Radiation (400-700nm)")]
        [Range(0f, 500f)] public float Violet_400_420nm = 20f;        // Deep blue, morphogenesis
        [Range(0f, 500f)] public float Blue_420_490nm = 100f;         // Photosynthesis, compact growth
        [Range(0f, 500f)] public float Green_490_550nm = 80f;         // Canopy penetration
        [Range(0f, 500f)] public float Yellow_550_590nm = 60f;        // Limited photosynthetic value
        [Range(0f, 500f)] public float Orange_590_630nm = 40f;        // Transition wavelengths
        [Range(0f, 500f)] public float Red_630_660nm = 120f;          // Primary photosynthesis
        [Range(0f, 500f)] public float DeepRed_660_700nm = 80f;       // Flowering trigger
        
        [Header("Extended Spectrum Effects")]
        [Range(0f, 100f)] public float UV_B_280_315nm = 2f;           // Stress response, trichomes
        [Range(0f, 100f)] public float UV_A_315_400nm = 15f;          // Cannabinoid production
        [Range(0f, 200f)] public float NearInfrared_700_800nm = 60f;  // Heat, shade avoidance
        [Range(0f, 150f)] public float FarRed_700_850nm = 40f;        // Photomorphogenesis
        
        [Header("Specialized Cannabis Wavelengths")]
        [Range(0f, 50f)] public float Cannabis_Specific_285nm = 1f;    // THC synthesis
        [Range(0f, 50f)] public float Cannabis_Specific_365nm = 8f;    // Terpene production
        [Range(0f, 100f)] public float Cannabis_Specific_385nm = 12f;  // Resin production
        [Range(0f, 100f)] public float Cannabis_Specific_660nm = 100f; // Peak photosynthesis
        [Range(0f, 80f)] public float Cannabis_Specific_730nm = 30f;   // Flowering hormone
        
        [Header("Light Quality Metrics")]
        [Range(0f, 5f)] public float PhotosyntheticEfficiency = 1f;    // Quantum yield
        [Range(0f, 2f)] public float RedToFarRedRatio = 1.2f;         // R:FR morphology control
        [Range(0f, 3f)] public float BlueToRedRatio = 0.8f;           // Growth pattern control
        [Range(0f, 10f)] public float UVToVisibleRatio = 0.05f;       // Stress/defense response
        
        [Header("Dynamic Light Properties")]
        public bool SpectrumStability = true;                          // Consistent output
        public float FlickerFrequency = 0f;                           // Hz, 0 = no flicker
        public float ColorTemperature = 3000f;                        // Kelvin
        public float ChromaticCoordinates_X = 0.33f;                  // CIE color space
        public float ChromaticCoordinates_Y = 0.33f;                  // CIE color space
        
        [Header("Photoperiod & Circadian")]
        public float DailyPhotoperiod = 18f;                          // Hours of light
        public AnimationCurve IntensityCurve;                         // Daily light cycle
        public AnimationCurve SpectrumShiftCurve;                     // Sunrise/sunset shifts
        public bool CircadianLighting = false;                        // Dynamic spectrum
        
        /// <summary>
        /// Calculates total Photosynthetically Active Radiation (PAR).
        /// </summary>
        public float GetTotalPAR()
        {
            return Violet_400_420nm + Blue_420_490nm + Green_490_550nm + 
                   Yellow_550_590nm + Orange_590_630nm + Red_630_660nm + DeepRed_660_700nm;
        }
        
        /// <summary>
        /// Gets photosynthetic quality factor based on spectrum distribution.
        /// </summary>
        public float GetPhotosyntheticQuality()
        {
            float totalPAR = GetTotalPAR();
            if (totalPAR == 0f) return 0f;
            
            // Weight different wavelengths by their photosynthetic efficiency
            float weightedEfficiency = (
                Blue_420_490nm * 0.8f +        // High efficiency
                Red_630_660nm * 1.0f +          // Peak efficiency  
                DeepRed_660_700nm * 0.9f +      // High efficiency
                Green_490_550nm * 0.3f +        // Lower efficiency but penetrates canopy
                Violet_400_420nm * 0.6f +       // Moderate efficiency
                Yellow_550_590nm * 0.2f +       // Low efficiency
                Orange_590_630nm * 0.4f         // Moderate efficiency
            ) / totalPAR;
            
            return Mathf.Clamp01(weightedEfficiency * PhotosyntheticEfficiency);
        }
        
        /// <summary>
        /// Calculates morphological control factor from blue:red ratio.
        /// </summary>
        public float GetMorphologyControl()
        {
            float blueContent = Blue_420_490nm + Violet_400_420nm;
            float redContent = Red_630_660nm + DeepRed_660_700nm;
            
            if (redContent == 0f) return 2f; // Very blue = compact growth
            
            float actualRatio = blueContent / redContent;
            
            // Optimal ratio for cannabis is around 0.8-1.2
            if (actualRatio >= 0.8f && actualRatio <= 1.2f)
                return 1f; // Optimal
            else if (actualRatio > 1.2f)
                return 1f + (actualRatio - 1.2f) * 0.5f; // Increasingly compact
            else
                return actualRatio / 0.8f; // Increasingly stretchy
        }
        
        /// <summary>
        /// Predicts flowering response based on red:far-red ratio.
        /// </summary>
        public float GetFloweringInduction()
        {
            float redContent = Red_630_660nm + DeepRed_660_700nm;
            float farRedContent = FarRed_700_850nm + Cannabis_Specific_730nm;
            
            if (farRedContent == 0f) return 1f; // High R:FR promotes flowering
            
            float rfrRatio = redContent / farRedContent;
            
            // Cannabis flowers better with high R:FR (1.2+)
            return Mathf.Clamp01(rfrRatio / 1.5f);
        }
        
        /// <summary>
        /// Calculates terpene enhancement potential from specific wavelengths.
        /// </summary>
        public float GetTerpeneEnhancingRatio()
        {
            float uvContent = UV_A_315_400nm + Cannabis_Specific_365nm;
            float blueContent = Blue_420_490nm + Violet_400_420nm;
            float totalVisible = GetTotalPAR();
            
            if (totalVisible == 0f) return 0f;
            
            // Terpenes enhanced by UV and blue light stress
            float uvFactor = uvContent / (totalVisible * 0.1f); // UV should be ~10% of visible
            float blueFactor = blueContent / (totalVisible * 0.3f); // Blue should be ~30% of visible
            
            return Mathf.Clamp01((uvFactor + blueFactor) / 2f);
        }
        
        /// <summary>
        /// Predicts cannabinoid production enhancement.
        /// </summary>
        public CannabinoidLightResponse GetCannabinoidResponse()
        {
            var response = new CannabinoidLightResponse();
            
            // THC production enhanced by UV-B stress and specific wavelengths
            response.THCEnhancement = Mathf.Clamp01(
                (UV_B_280_315nm / 5f) + 
                (Cannabis_Specific_285nm / 2f) + 
                (Cannabis_Specific_385nm / 15f)
            );
            
            // CBD production enhanced by stable, high-quality light
            response.CBDEnhancement = GetPhotosyntheticQuality() * 
                (SpectrumStability ? 1f : 0.7f);
            
            // Trichome production enhanced by UV and far-red
            response.TrichomeEnhancement = Mathf.Clamp01(
                (UV_A_315_400nm + UV_B_280_315nm) / 20f + 
                FarRed_700_850nm / 100f
            );
            
            // Overall cannabinoid quality
            response.OverallQuality = (response.THCEnhancement + 
                response.CBDEnhancement + response.TrichomeEnhancement) / 3f;
            
            return response;
        }
        
        /// <summary>
        /// Calculates light stress factors.
        /// </summary>
        public LightStressFactors GetStressFactors()
        {
            var stress = new LightStressFactors();
            
            // UV stress (beneficial in small amounts)
            float totalUV = UV_A_315_400nm + UV_B_280_315nm;
            stress.UVStress = totalUV > 20f ? (totalUV - 20f) / 30f : 0f;
            
            // Heat stress from infrared
            stress.HeatStress = NearInfrared_700_800nm > 100f ? 
                (NearInfrared_700_800nm - 100f) / 100f : 0f;
            
            // Light burn from excessive intensity
            float totalIntensity = GetTotalPAR() + totalUV + NearInfrared_700_800nm;
            stress.LightBurnStress = totalIntensity > 1000f ? 
                (totalIntensity - 1000f) / 500f : 0f;
            
            // Spectrum imbalance stress
            stress.SpectrumImbalance = CalculateSpectrumImbalance();
            
            // Flicker stress
            stress.FlickerStress = FlickerFrequency > 0f && FlickerFrequency < 100f ? 
                0.5f : 0f; // Flicker below 100Hz is stressful
            
            return stress;
        }
        
        /// <summary>
        /// Applies time-of-day spectrum shifts for circadian lighting.
        /// </summary>
        public void ApplyCircadianShift(float timeOfDay)
        {
            if (!CircadianLighting || SpectrumShiftCurve == null) return;
            
            float shiftFactor = SpectrumShiftCurve.Evaluate(timeOfDay / 24f);
            
            // Morning: more blue, evening: more red
            if (timeOfDay < 12f) // Morning
            {
                Blue_420_490nm *= 1f + shiftFactor * 0.3f;
                Red_630_660nm *= 1f - shiftFactor * 0.2f;
            }
            else // Evening
            {
                Blue_420_490nm *= 1f - shiftFactor * 0.2f;
                Red_630_660nm *= 1f + shiftFactor * 0.3f;
                FarRed_700_850nm *= 1f + shiftFactor * 0.4f;
            }
        }
        
        private float CalculateSpectrumImbalance()
        {
            float totalPAR = GetTotalPAR();
            if (totalPAR == 0f) return 1f;
            
            // Check for severely unbalanced spectrum
            float maxComponent = Mathf.Max(Blue_420_490nm, Red_630_660nm, Green_490_550nm);
            float imbalance = maxComponent / totalPAR;
            
            // Ideal spectrum has no single component > 40% of total
            return imbalance > 0.4f ? (imbalance - 0.4f) / 0.6f : 0f;
        }
    }
    
    /// <summary>
    /// Cannabinoid response to light spectrum.
    /// </summary>
    [System.Serializable]
    public class CannabinoidLightResponse
    {
        public float THCEnhancement;
        public float CBDEnhancement;
        public float TrichomeEnhancement;
        public float OverallQuality;
    }
    
    /// <summary>
    /// Light-induced stress factors.
    /// </summary>
    [System.Serializable]
    public class LightStressFactors
    {
        public float UVStress;
        public float HeatStress;
        public float LightBurnStress;
        public float SpectrumImbalance;
        public float FlickerStress;
    }
}