using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ProjectChimera.Systems.Professional
{
    /// <summary>
    /// Extension Framework for Professional Polish
    /// Provides mod support, scripting API, and custom content capabilities
    /// </summary>
    public class ExtensionFramework
    {
        private bool _isInitialized = false;
        private string _modsDirectory;
        private Dictionary<string, ExtensionInfo> _availableExtensions = new Dictionary<string, ExtensionInfo>();
        private Dictionary<string, LoadedExtension> _loadedExtensions = new Dictionary<string, LoadedExtension>();
        private ExtensionAPI _api;
        
        public void Initialize(string modsDirectory)
        {
            _modsDirectory = modsDirectory;
            _api = new ExtensionAPI();
            
            CreateModsDirectoryIfNeeded();
            ScanForExtensions();
            
            _isInitialized = true;
            Debug.Log($"Extension Framework initialized with {_availableExtensions.Count} extensions found");
        }
        
        public List<string> GetAvailableExtensions()
        {
            return _availableExtensions.Keys.ToList();
        }
        
        public bool LoadExtension(string extensionName)
        {
            if (!_isInitialized || !_availableExtensions.ContainsKey(extensionName))
            {
                Debug.LogWarning($"Extension not found: {extensionName}");
                return false;
            }
            
            if (_loadedExtensions.ContainsKey(extensionName))
            {
                Debug.LogWarning($"Extension already loaded: {extensionName}");
                return true;
            }
            
            try
            {
                var extensionInfo = _availableExtensions[extensionName];
                var loadedExtension = LoadExtensionFromFile(extensionInfo);
                
                if (loadedExtension != null)
                {
                    _loadedExtensions[extensionName] = loadedExtension;
                    loadedExtension.Initialize(_api);
                    
                    Debug.Log($"Extension loaded successfully: {extensionName}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load extension {extensionName}: {ex.Message}");
            }
            
            return false;
        }
        
        public bool UnloadExtension(string extensionName)
        {
            if (!_loadedExtensions.ContainsKey(extensionName))
            {
                return false;
            }
            
            try
            {
                var loadedExtension = _loadedExtensions[extensionName];
                loadedExtension.Cleanup();
                _loadedExtensions.Remove(extensionName);
                
                Debug.Log($"Extension unloaded: {extensionName}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to unload extension {extensionName}: {ex.Message}");
                return false;
            }
        }
        
        public ExtensionInfo GetExtensionInfo(string extensionName)
        {
            return _availableExtensions.GetValueOrDefault(extensionName);
        }
        
        public bool IsExtensionLoaded(string extensionName)
        {
            return _loadedExtensions.ContainsKey(extensionName);
        }
        
        public void ReloadExtension(string extensionName)
        {
            if (IsExtensionLoaded(extensionName))
            {
                UnloadExtension(extensionName);
            }
            
            LoadExtension(extensionName);
        }
        
        public void Cleanup()
        {
            // Unload all extensions
            var loadedExtensionNames = _loadedExtensions.Keys.ToList();
            foreach (var extensionName in loadedExtensionNames)
            {
                UnloadExtension(extensionName);
            }
            
            _availableExtensions.Clear();
            _loadedExtensions.Clear();
            
            _isInitialized = false;
        }
        
        private void CreateModsDirectoryIfNeeded()
        {
            var fullPath = Path.Combine(Application.persistentDataPath, _modsDirectory);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                Debug.Log($"Created mods directory: {fullPath}");
            }
        }
        
        private void ScanForExtensions()
        {
            var fullPath = Path.Combine(Application.persistentDataPath, _modsDirectory);
            if (!Directory.Exists(fullPath)) return;
            
            // Scan for .dll files
            var dllFiles = Directory.GetFiles(fullPath, "*.dll", SearchOption.AllDirectories);
            foreach (var dllFile in dllFiles)
            {
                ScanDllForExtensions(dllFile);
            }
            
            // Scan for .json manifest files
            var manifestFiles = Directory.GetFiles(fullPath, "manifest.json", SearchOption.AllDirectories);
            foreach (var manifestFile in manifestFiles)
            {
                ScanManifestForExtensions(manifestFile);
            }
        }
        
        private void ScanDllForExtensions(string dllPath)
        {
            try
            {
                var assembly = Assembly.LoadFrom(dllPath);
                var extensionTypes = assembly.GetTypes()
                    .Where(t => t.GetInterface(nameof(IChimeraExtension)) != null)
                    .ToList();
                
                foreach (var extensionType in extensionTypes)
                {
                    var extensionInfo = new ExtensionInfo
                    {
                        Name = extensionType.Name,
                        FilePath = dllPath,
                        Type = ExtensionType.Compiled,
                        Version = GetExtensionVersion(extensionType),
                        Description = GetExtensionDescription(extensionType),
                        Author = GetExtensionAuthor(extensionType),
                        AssemblyType = extensionType
                    };
                    
                    _availableExtensions[extensionInfo.Name] = extensionInfo;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to scan DLL {dllPath}: {ex.Message}");
            }
        }
        
        private void ScanManifestForExtensions(string manifestPath)
        {
            try
            {
                var manifestContent = File.ReadAllText(manifestPath);
                var manifest = JsonUtility.FromJson<ExtensionManifest>(manifestContent);
                
                var extensionInfo = new ExtensionInfo
                {
                    Name = manifest.Name,
                    FilePath = Path.GetDirectoryName(manifestPath),
                    Type = ExtensionType.Script,
                    Version = manifest.Version,
                    Description = manifest.Description,
                    Author = manifest.Author,
                    ScriptFiles = manifest.ScriptFiles ?? new List<string>(),
                    Dependencies = manifest.Dependencies ?? new List<string>()
                };
                
                _availableExtensions[extensionInfo.Name] = extensionInfo;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to scan manifest {manifestPath}: {ex.Message}");
            }
        }
        
        private LoadedExtension LoadExtensionFromFile(ExtensionInfo extensionInfo)
        {
            switch (extensionInfo.Type)
            {
                case ExtensionType.Compiled:
                    return LoadCompiledExtension(extensionInfo);
                case ExtensionType.Script:
                    return LoadScriptExtension(extensionInfo);
                default:
                    return null;
            }
        }
        
        private LoadedExtension LoadCompiledExtension(ExtensionInfo extensionInfo)
        {
            try
            {
                var instance = Activator.CreateInstance(extensionInfo.AssemblyType) as IChimeraExtension;
                if (instance != null)
                {
                    return new LoadedExtension
                    {
                        Info = extensionInfo,
                        Instance = instance,
                        Type = ExtensionType.Compiled
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create instance of {extensionInfo.Name}: {ex.Message}");
            }
            
            return null;
        }
        
        private LoadedExtension LoadScriptExtension(ExtensionInfo extensionInfo)
        {
            try
            {
                var scriptExtension = new ScriptExtension();
                scriptExtension.LoadScripts(extensionInfo.FilePath, extensionInfo.ScriptFiles);
                
                return new LoadedExtension
                {
                    Info = extensionInfo,
                    Instance = scriptExtension,
                    Type = ExtensionType.Script
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load script extension {extensionInfo.Name}: {ex.Message}");
            }
            
            return null;
        }
        
        private string GetExtensionVersion(Type extensionType)
        {
            var versionAttribute = extensionType.GetCustomAttribute<ExtensionVersionAttribute>();
            return versionAttribute?.Version ?? "1.0.0";
        }
        
        private string GetExtensionDescription(Type extensionType)
        {
            var descriptionAttribute = extensionType.GetCustomAttribute<ExtensionDescriptionAttribute>();
            return descriptionAttribute?.Description ?? "No description available";
        }
        
        private string GetExtensionAuthor(Type extensionType)
        {
            var authorAttribute = extensionType.GetCustomAttribute<ExtensionAuthorAttribute>();
            return authorAttribute?.Author ?? "Unknown";
        }
    }
    
    // Extension interfaces and base classes
    public interface IChimeraExtension
    {
        string Name { get; }
        string Version { get; }
        void Initialize(ExtensionAPI api);
        void Update();
        void Cleanup();
    }
    
    public abstract class ChimeraExtensionBase : IChimeraExtension
    {
        public abstract string Name { get; }
        public abstract string Version { get; }
        
        protected ExtensionAPI API { get; private set; }
        
        public virtual void Initialize(ExtensionAPI api)
        {
            API = api;
            OnInitialize();
        }
        
        public virtual void Update()
        {
            OnUpdate();
        }
        
        public virtual void Cleanup()
        {
            OnCleanup();
        }
        
        protected abstract void OnInitialize();
        protected virtual void OnUpdate() { }
        protected virtual void OnCleanup() { }
    }
    
    // Extension API for mod developers
    public class ExtensionAPI
    {
        public void LogInfo(string message)
        {
            Debug.Log($"[Extension] {message}");
        }
        
        public void LogWarning(string message)
        {
            Debug.LogWarning($"[Extension] {message}");
        }
        
        public void LogError(string message)
        {
            Debug.LogError($"[Extension] {message}");
        }
        
        public GameObject FindGameObject(string name)
        {
            return GameObject.Find(name);
        }
        
        public T FindComponent<T>() where T : Component
        {
            return UnityEngine.Object.FindObjectOfType<T>();
        }
        
        public T[] FindComponents<T>() where T : Component
        {
            return UnityEngine.Object.FindObjectsOfType<T>();
        }
        
        public void RegisterEventHandler(string eventName, Action<object> handler)
        {
            // Register event handler with the main game system
        }
        
        public void TriggerEvent(string eventName, object data)
        {
            // Trigger event in the main game system
        }
        
        public void AddUIElement(GameObject uiElement, Transform parent)
        {
            if (uiElement != null && parent != null)
            {
                uiElement.transform.SetParent(parent, false);
            }
        }
        
        public Texture2D LoadTexture(string path)
        {
            return Resources.Load<Texture2D>(path);
        }
        
        public AudioClip LoadAudioClip(string path)
        {
            return Resources.Load<AudioClip>(path);
        }
        
        public GameObject LoadPrefab(string path)
        {
            return Resources.Load<GameObject>(path);
        }
    }
    
    // Script extension support
    public class ScriptExtension : IChimeraExtension
    {
        public string Name { get; private set; } = "Script Extension";
        public string Version { get; private set; } = "1.0.0";
        
        private List<string> _scriptContents = new List<string>();
        
        public void LoadScripts(string basePath, List<string> scriptFiles)
        {
            foreach (var scriptFile in scriptFiles)
            {
                var fullPath = Path.Combine(basePath, scriptFile);
                if (File.Exists(fullPath))
                {
                    var content = File.ReadAllText(fullPath);
                    _scriptContents.Add(content);
                }
            }
        }
        
        public void Initialize(ExtensionAPI api)
        {
            // Initialize script execution environment
            Debug.Log($"Script extension initialized with {_scriptContents.Count} scripts");
        }
        
        public void Update()
        {
            // Update script execution if needed
        }
        
        public void Cleanup()
        {
            _scriptContents.Clear();
        }
    }
    
    // Supporting data structures
    public enum ExtensionType
    {
        Compiled,
        Script
    }
    
    [System.Serializable]
    public class ExtensionInfo
    {
        public string Name;
        public string FilePath;
        public ExtensionType Type;
        public string Version;
        public string Description;
        public string Author;
        public Type AssemblyType;
        public List<string> ScriptFiles = new List<string>();
        public List<string> Dependencies = new List<string>();
    }
    
    public class LoadedExtension
    {
        public ExtensionInfo Info;
        public IChimeraExtension Instance;
        public ExtensionType Type;
        
        public void Initialize(ExtensionAPI api)
        {
            Instance?.Initialize(api);
        }
        
        public void Update()
        {
            Instance?.Update();
        }
        
        public void Cleanup()
        {
            Instance?.Cleanup();
        }
    }
    
    [System.Serializable]
    public class ExtensionManifest
    {
        public string Name;
        public string Version;
        public string Description;
        public string Author;
        public List<string> ScriptFiles;
        public List<string> Dependencies;
    }
    
    // Extension attributes
    [AttributeUsage(AttributeTargets.Class)]
    public class ExtensionVersionAttribute : Attribute
    {
        public string Version { get; }
        
        public ExtensionVersionAttribute(string version)
        {
            Version = version;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class ExtensionDescriptionAttribute : Attribute
    {
        public string Description { get; }
        
        public ExtensionDescriptionAttribute(string description)
        {
            Description = description;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class ExtensionAuthorAttribute : Attribute
    {
        public string Author { get; }
        
        public ExtensionAuthorAttribute(string author)
        {
            Author = author;
        }
    }
}