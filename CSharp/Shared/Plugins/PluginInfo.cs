namespace ModdingToolkit;

public record PluginInfo(string ModName, string Version, ImmutableArray<string> Dependencies);