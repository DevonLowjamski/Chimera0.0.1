using UnityEngine;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Runtime test event channels for core system testing.
    /// These are runtime-compatible versions for testing the core systems.
    /// </summary>

    [CreateAssetMenu(fileName = "Runtime Test Simple Event", menuName = "Project Chimera/Core/Runtime Test Simple Event")]
    public class RuntimeTestSimpleEventSO : SimpleGameEventSO
    {
        // Simple event for runtime testing - no additional functionality needed
    }

    [CreateAssetMenu(fileName = "Runtime Test String Event", menuName = "Project Chimera/Core/Runtime Test String Event")]
    public class RuntimeTestStringEventSO : StringGameEventSO
    {
        // String event for runtime testing - no additional functionality needed
    }
}