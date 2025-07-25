using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Core.Optimization
{
    /// <summary>
    /// Base class for Unity components that can be pooled
    /// Provides standardized pooling lifecycle management
    /// </summary>
    public abstract class PoolableComponent : MonoBehaviour, IPoolable
    {
        [Header("Pooling Configuration")]
        [SerializeField] protected bool _resetOnReturn = true;
        [SerializeField] protected bool _validateOnReturn = true;
        
        // Pooling state tracking
        private bool _isPooled = false;
        private float _spawnTime;
        private int _poolReturnCount = 0;
        
        public bool IsPooled => _isPooled;
        public float TimeSinceSpawn => Time.time - _spawnTime;
        public int PoolReturnCount => _poolReturnCount;
        
        #region Unity Lifecycle
        
        protected virtual void Awake()
        {
            OnPoolableAwake();
        }
        
        protected virtual void OnEnable()
        {
            _spawnTime = Time.time;
            _isPooled = false;
            OnPoolableEnabled();
        }
        
        protected virtual void OnDisable()
        {
            OnPoolableDisabled();
        }
        
        #endregion
        
        #region IPoolable Implementation
        
        public virtual void Reset()
        {
            if (!_resetOnReturn) return;
            
            try
            {
                // Reset transform
                ResetTransform();
                
                // Reset component state
                ResetComponentState();
                
                // Validate state if enabled
                if (_validateOnReturn)
                {
                    ValidatePooledState();
                }
                
                _poolReturnCount++;
                _isPooled = true;
                
                OnPoolReset();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PoolableComponent] Error resetting {GetType().Name}: {ex.Message}", this);
                throw; // Re-throw to prevent pooling of corrupted object
            }
        }
        
        #endregion
        
        #region Transform Reset
        
        protected virtual void ResetTransform()
        {
            // Reset position, rotation, and scale to defaults
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            
            // Clear parent (will be set by pool manager)
            transform.SetParent(null);
        }
        
        #endregion
        
        #region Abstract/Virtual Methods
        
        /// <summary>
        /// Called during Awake - initialize poolable-specific setup
        /// </summary>
        protected virtual void OnPoolableAwake() { }
        
        /// <summary>
        /// Called when object is retrieved from pool or first created
        /// </summary>
        protected virtual void OnPoolableEnabled() { }
        
        /// <summary>
        /// Called when object is returned to pool
        /// </summary>
        protected virtual void OnPoolableDisabled() { }
        
        /// <summary>
        /// Reset component-specific state when returning to pool
        /// </summary>
        protected abstract void ResetComponentState();
        
        /// <summary>
        /// Validate that object is in correct state for pooling
        /// </summary>
        protected virtual void ValidatePooledState()
        {
            // Default validation - can be overridden
            if (transform.parent != null)
            {
                Debug.LogWarning($"[PoolableComponent] {GetType().Name} still has parent when returning to pool");
            }
        }
        
        /// <summary>
        /// Called after successful pool reset
        /// </summary>
        protected virtual void OnPoolReset() { }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Manually return this object to its pool
        /// </summary>
        public void ReturnToPool()
        {
            if (PooledObjectManager.Instance != null)
            {
                // Try to find and return to appropriate Unity pool
                // This requires the pool to be tracked elsewhere or referenced
                gameObject.SetActive(false);
            }
            else
            {
                // Fallback to destroy if no pool manager
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Get pooling statistics for this component
        /// </summary>
        public PoolableComponentStats GetPoolingStats()
        {
            return new PoolableComponentStats
            {
                ComponentType = GetType().Name,
                IsCurrentlyPooled = _isPooled,
                TimeSinceSpawn = TimeSinceSpawn,
                PoolReturnCount = _poolReturnCount,
                ResetOnReturn = _resetOnReturn,
                ValidateOnReturn = _validateOnReturn
            };
        }
        
        #endregion
    }
    
    /// <summary>
    /// Statistics for poolable component instances
    /// </summary>
    public class PoolableComponentStats
    {
        public string ComponentType { get; set; }
        public bool IsCurrentlyPooled { get; set; }
        public float TimeSinceSpawn { get; set; }
        public int PoolReturnCount { get; set; }
        public bool ResetOnReturn { get; set; }
        public bool ValidateOnReturn { get; set; }
        
        public override string ToString()
        {
            return $"{ComponentType}: {(IsCurrentlyPooled ? "Pooled" : "Active")}, " +
                   $"Spawn Time: {TimeSinceSpawn:F1}s, " +
                   $"Return Count: {PoolReturnCount}";
        }
    }
}