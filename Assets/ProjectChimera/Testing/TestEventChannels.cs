using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Testing
{
    /// <summary>
    /// Test event channels for validating the event system.
    /// </summary>

    [CreateAssetMenu(fileName = "Test Simple Event", menuName = "Project Chimera/Testing/Test Simple Event")]
    public class TestSimpleEventSO : SimpleGameEventSO
    {
        // Simple event for testing - no additional functionality needed
    }

    [CreateAssetMenu(fileName = "Test String Event", menuName = "Project Chimera/Testing/Test String Event")]
    public class TestStringEventSO : StringGameEventSO
    {
        // String event for testing - no additional functionality needed
    }

    [CreateAssetMenu(fileName = "Test Float Event", menuName = "Project Chimera/Testing/Test Float Event")]
    public class TestFloatEventSO : FloatGameEventSO
    {
        // Float event for testing - no additional functionality needed
    }

    [CreateAssetMenu(fileName = "Test Int Event", menuName = "Project Chimera/Testing/Test Int Event")]
    public class TestIntEventSO : IntGameEventSO
    {
        // Int event for testing - no additional functionality needed
    }
}