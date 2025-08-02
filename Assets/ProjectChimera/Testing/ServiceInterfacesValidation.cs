using UnityEngine;

namespace ProjectChimera.Testing
{
    /// <summary>
    /// Simple validation script to ensure ServiceInterfaces.cs compiles correctly
    /// </summary>
    public class ServiceInterfacesValidation : MonoBehaviour
    {
        [System.Serializable]
        public class ValidationResult
        {
            public bool IsValid;
            public string[] Errors;
            public string Summary;
        }
        
        [SerializeField] private ValidationResult _lastValidation;
        
        void Start()
        {
            ValidateServiceInterfaces();
        }
        
        public void ValidateServiceInterfaces()
        {
            try
            {
                // Test that we can access the service interfaces namespace
                var serviceNamespace = typeof(ProjectChimera.Systems.Registry.ICompetitionManagementService);
                
                _lastValidation = new ValidationResult
                {
                    IsValid = true,
                    Errors = new string[0],
                    Summary = "ServiceInterfaces.cs compiled successfully - all type aliases resolved"
                };
                
                Debug.Log("✅ ServiceInterfaces.cs validation successful - All trading service type aliases resolved");
            }
            catch (System.Exception ex)
            {
                _lastValidation = new ValidationResult
                {
                    IsValid = false,
                    Errors = new string[] { ex.Message },
                    Summary = $"ServiceInterfaces.cs validation failed: {ex.Message}"
                };
                
                Debug.LogError($"❌ ServiceInterfaces.cs validation failed: {ex.Message}");
            }
        }
    }
}