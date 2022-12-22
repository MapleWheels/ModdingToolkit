namespace ModdingToolkit.Client;

public static class ConsoleCommands
{
    private record CommandBuilder(
        string Name, string HelpText, 
        LuaCsAction OnExecute, 
        LuaCsFunc GetValidArgs,
        bool IsCheat);
    
    private static readonly List<CommandBuilder> registeredCommands = new();
    
    public static bool IsLoaded { get; private set; } = false;
    
    public static void RegisterCommand(
        string command, 
        string helpText, 
        LuaCsAction? onCommandExecuted,
        LuaCsFunc? getValidArgs = null,
        bool isCheat = false)
    {
        registeredCommands.Add(new CommandBuilder(command, helpText, onCommandExecuted, getValidArgs, isCheat));
    }

    public static void UnregisterAllCommands()
    {
        if (IsLoaded)
            UnloadAllCommands();
        registeredCommands.Clear();
    }

    public static void ReloadAllCommands()
    {
        if (IsLoaded)
            UnloadAllCommands();
        foreach (CommandBuilder command in registeredCommands)
        {
            GameMain.LuaCs.Game.AddCommand(command.Name, command.HelpText, command.OnExecute, 
                command.GetValidArgs, command.IsCheat);
        }
        IsLoaded = true;
    }

    public static void UnloadAllCommands()
    {
        if (!IsLoaded)
            return;

        foreach (CommandBuilder command in registeredCommands)
        {
            GameMain.LuaCs.Game.RemoveCommand(command.Name);
        }
        
        IsLoaded = false;
    }
    
    
}