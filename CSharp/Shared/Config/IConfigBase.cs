namespace ModdingToolkit.Config;

public interface IConfigBase
{
    public string Name { get; }
    public string ModName { get; }
    string GetStringValue();
}