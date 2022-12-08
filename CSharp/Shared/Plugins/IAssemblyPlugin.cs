namespace ModdingToolkit;

public interface IAssemblyPlugin
{
    PluginInfo Initialize();
    void Dispose();
}