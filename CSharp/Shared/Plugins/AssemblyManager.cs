using System.Runtime.Loader;

namespace ModdingToolkit;

public static class AssemblyManager
{
    #region ExternalAPI

    public static bool IsLoaded { get; private set; }
    
    public static bool LoadAssembly(string absFilePath)
    {
        throw new NotImplementedException();
    }

    public static void UnloadPlugin(IAssemblyPlugin plugin)
    {
        throw new NotImplementedException();
    }

    public static bool TryGetActivePlugin(string name, out IAssemblyPlugin plugin)
    {
        throw new NotImplementedException();
    }

    public static IEnumerator<KeyValuePair<string, IAssemblyPlugin>> GetActivePlugins()
    {
        foreach (var plugin in LoadedPlugins)
        {
            yield return plugin;
        }
    }
    
    public static bool TryGetAssembly(string name, out Assembly? assembly)
    {
        throw new NotImplementedException();
    }

    public static IEnumerator<KeyValuePair<string, Assembly>> GetActivePluginAssemblies()
    {
        foreach (KeyValuePair<string,Assembly> loadedAssembly in ActiveAssemblies)
        {
            yield return loadedAssembly;
        }
    }

    public static IEnumerator<KeyValuePair<string, Assembly>> GetLoadedAssemblies()
    {
        foreach (KeyValuePair<string,Assembly> registeredAssembly in LoadedAssemblies)
        {
            yield return registeredAssembly;
        }
    }

    #endregion

    #region InternalAPI
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void LoadPlugins()
    {
        throw new NotImplementedException();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void UnloadPlugins()
    {
        throw new NotImplementedException();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void ReloadAllPlugins()
    {
        throw new NotImplementedException();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static Func<bool> BeginDispose()
    {
        throw new NotImplementedException();
    }
    
    #endregion

    #region Data

    private static readonly Dictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>();
    private static readonly Dictionary<string, Assembly> ActiveAssemblies = new Dictionary<string, Assembly>();
    private static readonly Dictionary<string, IAssemblyPlugin> LoadedPlugins = new Dictionary<string, IAssemblyPlugin>();
    private static readonly List<WeakReference<Assembly>> UnloadingAssemblies = new List<WeakReference<Assembly>>();

    #endregion

    #region FunctionsData

    

    #endregion

    #region TypeDefs

    private sealed class AssemblyContextLoader : AssemblyLoadContext
    {
        private AssemblyDependencyResolver dependencyResolver;

        public AssemblyContextLoader(string mainAssemblyLoadPath) : base(isCollectible: true)
        {
            dependencyResolver = new AssemblyDependencyResolver(mainAssemblyLoadPath);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            string? assPath = dependencyResolver.ResolveAssemblyToPath(assemblyName);
            if (assPath is not null)
                return LoadFromAssemblyPath(assPath);
            return null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string? libraryPath = dependencyResolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
                return LoadUnmanagedDllFromPath(libraryPath);
            return IntPtr.Zero;
        }
    }

    #endregion
}