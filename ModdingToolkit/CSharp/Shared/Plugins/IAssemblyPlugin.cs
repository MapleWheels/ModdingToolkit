namespace ModdingToolkit;

public interface IAssemblyPlugin
{
    void Initialize();
    void OnLoadCompleted();
    PluginInfo GetPluginInfo();
    void Dispose();
}