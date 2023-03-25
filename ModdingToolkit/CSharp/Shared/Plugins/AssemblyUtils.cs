namespace ModdingToolkit;

/// <summary>
/// Adds interop for Barotrauma-specifics
/// </summary>
public static class AssemblyUtils
{
    /// <summary>
    /// [NOT THREAD-SAFE]
    /// Allows iteration over all types (including interfaces) in all loaded assemblies managed by the AsmMgr and base game.
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<Type> GetAllTypesInLoadedAssemblies()
    {
        foreach (var type in typeof(Barotrauma.GameMain).Assembly.GetSafeTypes())   
        {
            Utils.Logging.PrintMessage($"ASMUTIL1: {type.FullName}");
            yield return type;
        }

        foreach (Type type in AssemblyManager.GetAllTypesInLoadedAssemblies())
        {
            Utils.Logging.PrintMessage($"ASMUTIL2: {type.FullName}");
            yield return type;
        }
    }
    
    /// <summary>
    /// [NOT THREAD-SAFE]
    /// Allows iteration over all non-interface types in all loaded assemblies in the AsmMgr that are assignable to the given type (IsAssignableFrom).
    /// </summary>
    /// <typeparam name="T">The type to compare against</typeparam>
    /// <returns>An Enumerator for matching types.</returns>
    public static IEnumerable<Type> GetSubTypesInLoadedAssemblies<T>()
    {
        foreach (var type in typeof(Barotrauma.GameMain).Assembly.GetSafeTypes().Where(t => typeof(T).IsAssignableFrom(t) && !t.IsInterface))   
        {
            yield return type;
        }

        foreach (var type in AssemblyManager.GetSubTypesInLoadedAssemblies<T>())
        {
            yield return type;
        }
    }
    
    /// <summary>
    /// [NOT THREAD-SAFE]
    /// Allows iteration over all non-interface types in all loaded assemblies in the AsmMgr who's names contain the string.
    /// </summary>
    /// <param name="name">The string name of the type to search for</param>
    /// <returns>An Enumerator for matching types.</returns>
    public static IEnumerable<Type> GetMatchingTypesInLoadedAssemblies(string name)
    {
        foreach (var type in typeof(Barotrauma.GameMain).Assembly.GetSafeTypes().Where(t => t.Name.Equals(name) && !t.IsInterface))   
        {
            yield return type;
        }

        foreach (Type type in AssemblyManager.GetMatchingTypesInLoadedAssemblies(name))
        {
            yield return type;
        }
    }
}