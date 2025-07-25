using ProjectChimera.Data.Environment;

namespace ProjectChimera.Systems.Environment
{
    /// <summary>
    /// PC014-2b: Interface for lighting system management service
    /// </summary>
    public interface ILightingService : IEnvironmentalService
    {
        void SetLightingConditions(string zoneId, LightingConditions conditions);
        LightingConditions GetLightingConditions(string zoneId);
        void UpdatePhotoperiod(string zoneId, float photoperiodHours);
        float CalculateDLI(string zoneId);
        void SetSpectrumProfile(string zoneId, LightSpectrumData spectrum);
    }
}