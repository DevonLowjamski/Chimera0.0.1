using UnityEngine;
using ProjectChimera.Data.Construction;

namespace ProjectChimera.Systems.Construction
{
    /// <summary>
    /// Interface for basic construction gaming system lifecycle management
    /// </summary>
    public interface IConstructionGamingSystemManager
    {
        bool IsConstructionGamingEnabled { get; }
        void Initialize();
        void UpdateSystem(float deltaTime);
        void Shutdown();
    }
}