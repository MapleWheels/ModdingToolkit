using System.Runtime.Loader;
using System.IO;

namespace ModdingToolkit;

/***
 * NOTE: This class can have it's performance, reliability and testing (Assert) improved.
 * However, I do not have the time to do so.
 */

/// <summary>
/// Provides functionality for the loading, unloading and management of plugins implementing IAssemblyPlugin.
/// All plugins are loaded into their own AssemblyLoadContext along with their dependencies.
/// WARNING: [BLOCKING] functions should not be extensively used in hot-path code as it's not performant.
/// WARNING: [BLOCKING] functions can cause dead-lock if used together without allowing one to finish it's operations.
/// </summary>
public static class AssemblyManager
{
    #region ExternalAPI


    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// [BLOCKING]
    /// Checks if there are any AssemblyLoadContexts still in the process of unloading.
    /// </summary>
    public static bool IsCurrentlyUnloading
    {
        get
        {
            lock (_OpsLock)
            {
                return UnloadingACLs.Any();
            }
        }
    }

    /// <summary>
    /// Whether or not loaded assemblies have their plugins instantiated.
    /// </summary>
    public static bool PluginsLoaded { get; private set; } = false;

    /// <summary>
    /// [BLOCKING]
    /// Loads an assembly from the given file path and searches it for IAssemblyPlugin classes.
    /// NOTE: if [instancePlugins] is false, all plugins will have to be reloaded for the plugins to be instanced later.
    /// If [PluginsLoaded] is false, no loading will be allowed to take place.
    /// </summary>
    /// <param name="absFilePath">Absolute path to the assembly.</param>
    /// <param name="instancePlugins">Whether or not an IAssemblyPlugin types should be instanced,</param>
    /// <returns>Loading Operation Success Status.</returns>
    public static AssemblyLoadingSuccessState LoadAssembly(string absFilePath, bool instancePlugins = false)
    {
        AssemblyLoadingSuccessState alss = LoadAssembliesAndPluginsFromLocation(absFilePath, out var loadedAcl);
        if (alss == AssemblyLoadingSuccessState.Success && PluginsLoaded && instancePlugins)
        {
            if (loadedAcl is null)
                return AssemblyLoadingSuccessState.ACLLoadFailure;
            lock (_OpsLock)
            {
                foreach (Type type in loadedAcl.PluginTypes)
                {
                    IAssemblyPlugin? plugin = (IAssemblyPlugin?)Activator.CreateInstance(type);
                    if (plugin is null)
                        return AssemblyLoadingSuccessState.PluginInstanceFailure;
                    plugin.Initialize();
                    loadedAcl.LoadedPlugins.Add(plugin);
                }

                foreach (IAssemblyPlugin plugin in loadedAcl.LoadedPlugins)
                {
                    plugin.OnLoadCompleted();
                }
            }
        }
        return alss;
    }

    /// <summary>
    /// [BLOCKING]
    /// Deactivates the given plugin.
    /// WARNING: The plugin cannot be reloaded with reloading ALL plugins! This is because there isn't any guarantee
    /// that the object instance has been unloaded due to strong references other plugins might have to it.
    /// </summary>
    /// <param name="plugin"></param>
    [MethodImpl(MethodImplOptions.Synchronized | MethodImplOptions.NoInlining)]
    public static void UnloadPlugin(IAssemblyPlugin plugin)
    {
        lock (_OpsLock)
        {
            LoadedACL? acl = null;
            foreach (KeyValuePair<string,LoadedACL> loadedAcl in LoadedACLs)
            {
                if (loadedAcl.Value.LoadedPlugins.Contains(plugin))
                {
                    acl = loadedAcl.Value;
                    break;
                }
            }
            plugin.Dispose();
            acl?.LoadedPlugins.Remove(plugin);
        }
    }
    
    /// <summary>
    /// [BLOCKING]
    /// Allows iteration over all types in all loaded assemblies in the AsmMgr that are assignable to the given type (IsAssignableFrom).
    /// </summary>
    /// <typeparam name="T">The type to compare against</typeparam>
    /// <returns>An Enumerator for matching types.</returns>
    public static IEnumerator<Type> GetSubTypesInLoadedAssemblies<T>()
    {
        Type targetType = typeof(T);
        foreach (var type in typeof(AssemblyManager).Assembly.GetSafeTypes().Where(t => targetType.IsAssignableFrom(t)))   
        {
            yield return type;
        }

        lock (_OpsLock)
        {
            foreach (KeyValuePair<string,LoadedACL> loadedAcl in LoadedACLs)
            {
                if (loadedAcl.Value.Alc.TryGetTarget(out var acl))
                {
                    foreach (Assembly aclAssembly in acl.Assemblies)
                    {
                        foreach (var type in aclAssembly.GetSafeTypes().Where(t => targetType.IsAssignableFrom(t)))
                        {
                            yield return type;
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// [BLOCKING]
    /// Allows iteration over all types in all loaded assemblies in the AsmMgr who's names contain the string.
    /// </summary>
    /// <param name="name">The string name of the type to search for</param>
    /// <returns>An Enumerator for matching types.</returns>
    public static IEnumerator<Type> GetMatchingTypesInLoadedAssemblies(string name)
    {
        foreach (var type in typeof(AssemblyManager).Assembly.GetSafeTypes().Where(t => t.Name.Equals(name)))   
        {
            yield return type;
        }

        lock (_OpsLock)
        {
            foreach (KeyValuePair<string,LoadedACL> loadedAcl in LoadedACLs)
            {
                if (loadedAcl.Value.Alc.TryGetTarget(out var acl))
                {
                    foreach (Assembly aclAssembly in acl.Assemblies)
                    {
                        foreach (var type in aclAssembly.GetSafeTypes().Where(t => t.Name.Equals(name)))
                        {
                            yield return type;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// [BLOCKING]
    /// Allows iteration over all types in all loaded assemblies managed by the AsmMgr.
    /// </summary>
    /// <returns></returns>
    public static IEnumerator<Type> GetAllTypesInLoadedAssemblies()
    {
        foreach (var type in typeof(AssemblyManager).Assembly.GetSafeTypes())   
        {
            yield return type;
        }

        lock (_OpsLock)
        {
            foreach (KeyValuePair<string,LoadedACL> loadedAcl in LoadedACLs)
            {
                if (loadedAcl.Value.Alc.TryGetTarget(out var acl))
                {
                    foreach (Assembly aclAssembly in acl.Assemblies)
                    {
                        foreach (var type in aclAssembly.GetSafeTypes())
                        {
                            yield return type;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// [LOCKING]
    /// Allows iteration over the list of loaded plugins.
    /// </summary>
    /// <returns></returns>
    public static IEnumerator<(PluginInfo, IAssemblyPlugin)> GetActivePlugins()
    {
        lock (_OpsLock)
        {
            foreach (KeyValuePair<string,LoadedACL> loadedAcl in LoadedACLs)
            {
                foreach (IAssemblyPlugin plugin in loadedAcl.Value.LoadedPlugins)
                {
                    yield return (plugin.GetPluginInfo(), plugin);
                }
            }
        }
    }
    
    /// <summary>
    /// Gets all types in the given assembly. Handles invalid type scenarios.
    /// </summary>
    /// <param name="assembly">The assembly to scan</param>
    /// <returns>An enumerable collection of types.</returns>
    public static IEnumerable<Type> GetSafeTypes(this Assembly assembly)
    {
        // Based on https://github.com/Qkrisi/ktanemodkit/blob/master/Assets/Scripts/ReflectionHelper.cs#L53-L67

        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            DebugConsole.Log($"AsmPatcher: Error RE, unable to patch: {e.Message}.");
            return e.Types.Where(x => x != null);
        }
        catch (Exception e)
        {
            DebugConsole.Log($"AsmPatcher: Error E, unable to patch: {e.Message}.");
            return new List<Type>();
        }
    }

    #endregion

    #region InternalAPI

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
    internal static AssemblyLoadingSuccessState LoadAssembliesAndPluginsFromLocation(string filePath, out LoadedACL? loadedAcl)
    {
        loadedAcl = null;
        string vAssemblyName = System.IO.Path.GetFileName(filePath);
        lock (_OpsLock)
        {
            // Check if the assembly is already loaded in an ACL, most likely due to it being a dependency.
            foreach (var loadedAlc in LoadedACLs)
            {
                if (loadedAlc.Value.Alc.TryGetTarget(out var a))
                {
                    foreach (Assembly ass in a.Assemblies)
                    {
                        if (ass.FullName?.ToLower().Contains(vAssemblyName.ToLower()) ?? false)
                        {
                            return AssemblyLoadingSuccessState.AlreadyLoaded;
                        }
                    }
                }
            }
        }

        try
        {
            lock (_OpsLock)
            {
                AssemblyContextLoader acl = new AssemblyContextLoader(filePath);
                acl.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(filePath)));
                LoadedACL lc = new LoadedACL(
                    filePath,
                    new List<Type>(), //types that impl IAssemblyPlugin  
                    new List<IAssemblyPlugin>(), //loaded plugins from this assembly, none right now
                    new WeakReference<AssemblyContextLoader>(acl));

                foreach (Assembly aclAssembly in acl.Assemblies)
                {
                    #warning TODO: Remove debug statement.
                    LuaCsSetup.PrintCsMessage($"AssemblyManager: Search AssemblyName {aclAssembly.FullName}");
                    var r = GetPluginTypesFromAssembly(aclAssembly);
                    if (r is not null)
                        lc.PluginTypes?.AddRange(r);
                }

                LoadedACLs.Add(filePath, lc);
                loadedAcl = lc;
                return AssemblyLoadingSuccessState.Success;
            }
        }
        catch (ArgumentNullException ane)
        {
            LuaCsSetup.PrintCsError($"AssemblyManager: EXCEPTION<ANE>: {ane.Message}");
            return AssemblyLoadingSuccessState.BadFilePath;
        }
        catch (ArgumentException ae)
        {
            LuaCsSetup.PrintCsError($"AssemblyManager: EXCEPTION<AE>: {ae.Message}");
            return AssemblyLoadingSuccessState.BadFilePath;
        }
        catch (FileLoadException fle)
        {
            LuaCsSetup.PrintCsError($"AssemblyManager: EXCEPTION<FLE>: {fle.Message}");
            return AssemblyLoadingSuccessState.CannotLoadFile;
        }
        catch (FileNotFoundException fne)
        {
            LuaCsSetup.PrintCsError($"AssemblyManager: EXCEPTION<FNE>: {fne.Message}");
            return AssemblyLoadingSuccessState.NoAssemblyFound;
        }
        catch (BadImageFormatException bfe)
        {
            LuaCsSetup.PrintCsError($"AssemblyManager: EXCEPTION<BFE>: {bfe.Message}");
            return AssemblyLoadingSuccessState.InvalidAssembly;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
    internal static bool LoadPlugins(out List<PluginInfo> pInfo)
    {
        pInfo = new();

        if (PluginsLoaded)
            UnloadPlugins();
        
        lock (_OpsLock)
        {
            if (IsCurrentlyUnloading)   
                return false;
            
            foreach (var loadedAcl in LoadedACLs)
            {
                foreach (Type type in loadedAcl.Value.PluginTypes)
                {
                    IAssemblyPlugin? plugin = (IAssemblyPlugin?)Activator.CreateInstance(type);
                    if (plugin is not null)
                    {
                        plugin.Initialize();
                        pInfo.Add(plugin.GetPluginInfo());
                        loadedAcl.Value.LoadedPlugins.Add(plugin);
                    }
                }

                foreach (IAssemblyPlugin plugin in loadedAcl.Value.LoadedPlugins)
                {
                    plugin.OnLoadCompleted();
                }
            }

            PluginsLoaded = true;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
    internal static bool UnloadPlugins()
    {
        lock (_OpsLock)
        {
            if (IsCurrentlyUnloading)   
                return false;
            
            foreach (KeyValuePair<string, LoadedACL> loadedAcl in LoadedACLs)
            {
                foreach (IAssemblyPlugin plugin in loadedAcl.Value.LoadedPlugins)
                {
                    plugin.Dispose();
                }
                loadedAcl.Value.LoadedPlugins.Clear();
            }

            PluginsLoaded = false;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
    internal static bool ReloadAllPlugins(ref List<PluginInfo> pluginInfo)
    {
        if (!UnloadPlugins())
            return false;
        return LoadPlugins(out pluginInfo);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
    internal static void BeginDispose()
    {
        lock (_OpsLock)
        {
            foreach (KeyValuePair<string,LoadedACL> loadedAcl in LoadedACLs)
            {
                foreach (IAssemblyPlugin plugin in loadedAcl.Value.LoadedPlugins)
                {
                    plugin.Dispose();
                }

                PluginsLoaded = false;
                loadedAcl.Value.LoadedPlugins.Clear();
                loadedAcl.Value.PluginTypes.Clear();
                if (loadedAcl.Value.Alc.TryGetTarget(out var acl))
                {
                    acl.Unloading += CleanupReference;
                    UnloadingACLs.Add(new WeakReference<AssemblyContextLoader>(acl, true));
                    acl.Unload();
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized | MethodImplOptions.NoInlining)]
    internal static bool FinalizeDispose()
    {
        bool allUnloaded = true;
        lock (_OpsLock)
        {
            foreach (WeakReference<AssemblyContextLoader> weakReference in UnloadingACLs)
            {
                if (weakReference.TryGetTarget(out var acl))
                {
                    allUnloaded = false;
                }
            }
            if (allUnloaded)
                UnloadingACLs.Clear();
        }

        return allUnloaded;
    }

    [MethodImpl(MethodImplOptions.Synchronized | MethodImplOptions.NoInlining)]
    private static void CleanupReference(AssemblyLoadContext ctx)
    {
        lock (_OpsLock)
        {
            if (ctx is AssemblyContextLoader { } acl)
            {
                if (acl.Assemblies.Any())
                    return;

                WeakReference<AssemblyContextLoader>? unloadingACLRef = null;
                foreach (WeakReference<AssemblyContextLoader> reference in UnloadingACLs)
                {
                    if (reference.TryGetTarget(out var _acl))
                    {
                        if (acl.Equals(_acl))
                        {
                            unloadingACLRef = reference;
                            break;
                        }
                    }
                }
            
                if (unloadingACLRef is not null)
                { 
                    UnloadingACLs.Remove(unloadingACLRef);
                }
            }
        }
    }
    
    #endregion

    #region Data

    private static readonly Dictionary<string, LoadedACL> LoadedACLs = new();
    private static readonly List<WeakReference<AssemblyContextLoader>> UnloadingACLs= new();
    // ReSharper disable once InconsistentNaming
    private static readonly object _OpsLock = new object();

    #endregion

    #region FunctionsData

    private static List<Type>? GetPluginTypesFromAssembly(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes().Where(t => typeof(IAssemblyPlugin).IsAssignableFrom(t)).ToList();
        }
        catch (Exception e)
        {
            #warning TODO: Remove debug statements.
            LuaCsSetup.PrintCsError($"AssemblyManager::GetPluginTypesFromAssembly() | ERROR: AssemblyName: {assembly.FullName} | ExceptionMessage: {e.Message}");
            return null;
        }
    }
    
    #endregion

    #region TypeDefs

    internal record LoadedACL(string FilePath, 
        List<Type> PluginTypes, 
        List<IAssemblyPlugin> LoadedPlugins,
        WeakReference<AssemblyContextLoader> Alc);
    
    internal sealed class AssemblyContextLoader : AssemblyLoadContext
    {
        private AssemblyDependencyResolver dependencyResolver;
        private bool IsResolving = false;   //this is to avoid circular dependency lookup.
        
        public AssemblyContextLoader(string mainAssemblyLoadPath) : base(isCollectible: true)
        {
            dependencyResolver = new AssemblyDependencyResolver(mainAssemblyLoadPath);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            if (IsResolving)
                return null;    //circular loading fast exit.
            
            string? assPath = dependencyResolver.ResolveAssemblyToPath(assemblyName);
            if (assPath is not null)
                return LoadFromAssemblyPath(assPath);

            try
            {
                //try resolve against other loaded alcs
                IsResolving = true;
                Assembly? ass = Barotrauma.GameMain.LuaCs.CsScriptLoader.LoadFromAssemblyName(assemblyName);
                if (ass is not null)
                    return ass;
                foreach (var loadedAcL in LoadedACLs)
                {
                    if (loadedAcL.Value.Alc.TryGetTarget(out var acl))
                    {
                        ass = acl.LoadFromAssemblyName(assemblyName);
                        if (ass is not null)
                            return ass;
                    }
                }
            }
            finally
            {
                IsResolving = false;
            }
            
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

    public enum AssemblyLoadingSuccessState
    {
        ACLLoadFailure,
        AlreadyLoaded,
        BadFilePath,
        CannotLoadFile,
        InvalidAssembly,
        NoAssemblyFound,
        PluginInstanceFailure,
        Success
    }

    #endregion
}