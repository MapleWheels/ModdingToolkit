namespace ModdingToolkit.Client;

public static class ConsoleCommands
{
    private static readonly List<DebugConsole.Command> registeredCommands = new();
    
    public static void RegisterCommand(
        string command, 
        string helpText, 
        System.Action<string[]> onCommandExecuted,
        System.Func<string[][]>? getValidArgs = null,
        bool isCheat = false)
    {
        var c = new DebugConsole.Command(command, helpText, onCommandExecuted, getValidArgs, isCheat);
        Barotrauma.DebugConsole.Commands.Add(c);
        registeredCommands.Add(c); 
    }

    public static void UnregisterAllCommands()
    {
        foreach (DebugConsole.Command command in registeredCommands)
        {
            Barotrauma.DebugConsole.Commands.Remove(command);
        }
        registeredCommands.Clear();
    }
}