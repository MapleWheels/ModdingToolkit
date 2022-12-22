A toolkit providing GUI, configuration, networking and Assembly-based plugin system for Barotrauma.
Currently in Alpha. Incomplete.

Readme is WIP.

[Steam Workshop]()

## Feature List

[☑ Complete ⬛ Partial ☐ Not Available]


☑ Assembly-based Plugin Loading

☑ Configuration Loading/Saving

☑ Settings Menu Integration (requires SettingsMenu plugin)

☑ Automatic Dependency Resolution 

⬛ Custom GUI Helpers

☐ Network Sync


## For C# Developers: How to Use - Assembly Plugin System.

#### Note: this mod requires [LuaCsForBarotrauma](https://steamcommunity.com/workshop/filedetails/?id=2559634234) installed with Client-Side Lua enabled.
#### NOTE2: Not all features are implemented, see beginning of this README for details.


1. Download a copy of the Release Assemblies package.  
2. Create a CSharp Solution in the IDE of your Choice with two projects, one for the Client and one for the Server. This project should be setup as described in the [CsForBarotrauma](https://evilfactory.github.io/LuaCsForBarotrauma/cs-docs/html/index.html) documentation.
3. Add the DLL files from the folder to your projects as per the below:
- Add the `Barotrauma.dll` and the `Client/NetScriptAssembly.dll` to your Client project and;
- Add the `DedicatedServer.dll` and the `Server/NetScriptAssembly.dll` to your Server project.
- Add all other `.dll` assemblies to both projects.
4. In the Project Settings;
- For both projects: Change the assembly name to be `<YourModName>.plugin`. This is required for .NET's Assembly Security Protocol.
- For both projects: Under the Debug Build Configuration, change the Platform Target from `Any CPU` to `x64`.
- For CLIENT project: Define the constant `CLIENT`.
- For SERVER project: Define the constant `SERVER`.
5. Create a plugin by having your plugin class implement the `IAssemblyPlugin` interface. The Modloader will automatically find and load your mod class at startup.

Example:
```csharp
namespace MyMod;

public class MyPlugin : IAssemblyPlugin
{
    void Initialize()
    {
        //Called the moment your plugin is instantiated.
    }
    
    void OnLoadCompleted()
    {
        //Called once ALL plugins have had their Initialize() functions called.
    }
    
    PluginInfo GetPluginInfo()
    {
        //Format: Unique Plugin ID, Version, Special-Case Dependencies (Not Yet Implemented)
        return new PluginInfo("AuthorName.MyMod", "0.0.0.0", ImmutableArray<string>.Empty); 
    }
    
    void Dispose()
    {
        //Called before your mod is unloaded. YOU MUST cleanup all references and instances stored by code from the main game!
    }
}
```

6. Your compiled mod's DLL **MUST** end with `.plugin.dll`!
7. In your Barotrauma Mod's ContentPackage folder, you must store your compiled DLL in **one** of four locations:
- `<ContentPackageRoot>/bin/Client/Standard` will load your mod on the CLIENT only if the ContentPackage is ENABLED.
- `<ContentPackageRoot>/bin/Client/Forced` will load your mod on the CLIENT always, even if the ContentPackage is disabled.
- `<ContentPackageRoot>/bin/Server/Standard` will load your mod on the SERVER only if the ContentPackage is ENABLED.
- `<ContentPackageRoot>/bin/Server/Forced` will load your mod on the SERVER always, even if the ContentPackage is disabled.
8. And finally, if using the Steam Workshop, add this mod as a Workshop Dependency.

#### NOTE: You can use the following Console Commands to unload/reload plugins:
- `cl_unloadassemblies`
- `cl_reloadassemblies`

## For C# Developers: How to Use - Config Variables

#### NOTE: You do NOT need to use the Assembly Plugin system in order to use the below. You can make use of this as a regular LuaCs mod.
#### NOTE2: Not all features are implemented, see beginning of this README for details.

All configuration variables are stored under `<Game_Root>/Config/<ModName>/<ModName>.xml`. ModName is the sanitized version of the Mod Name value so they may not match.

To create a configuration var simply use the API as per the below examples. The API is defined in the `ConfigManager` class (in `/Client` and `/Shared`).

You can manipulate these vars using the following Console Commands:

- `cl_cfglistvars` : Lists all defined vars, their types and their values.
- `cl_cfggetvar <ModName> <VarName>` : Prints out the data for this value.
- `cl_cfgsetvar <ModName> <VarName> <NewValue>` : Sets the var to a new value (use quotes for entries with spaces)
- `cl_cfgsaveall` : Saves all loaded vars to disk.

```csharp
void Initialize()
{
    // Availability: CLIENT AND SERVER
    // Simplest verion
    IConfigEntry<bool> mySimpleVar = ConfigManager.AddConfigEntry<bool>(
        "MyVarName1",   // [REQUIRED] Variable name
        "MyModName",    // [REQUIRED] Mod name, used for the config file name.
        false           // [REQUIRED] Default value.
    );
    
    
    // Availability: CLIENT AND SERVER
    // T implements IConvertible: Primitive variable (bool, int, float, etc) and string, enum.
    IConfigEntry<int> myConfigvar = ConfigManager.AddConfigEntry<int>(
        "MyVarName2",    // [REQUIRED] Variable name
        "MyModName",    // [REQUIRED] Mod name, used for the config file name.
        10,             // [REQUIRED] Default value.
        IConfigBase.Category.Gameplay,  // [OPTIONAL] Which menu in the settings should it appear under?
        IConfigBase.NetworkSync.NoSync, // [OPTIONAL] Whether or not it should be synced between server and clients. IGNORED for IConfigControl.
        // [OPTIONAL] Called whenever the value is sucessfully updated.
        () =>                           
        { 
            LuaCsSetup.PrintCsMessage($"Value updated! Now: {myConfigVar.Value}") 
        },
        // [OPTIONAL] Predicate/Validation for a new value, returning false stops Var.Value from being changed. 
        (newVal) =>                     
        { 
            return newVal > 20; 
        }
    );
    
    // Availability: CLIENT AND SERVER
    // Dropdown List of strings
    IConfigList myConfigList = ConfigManager.AddConfigList(
        "MyListVar3",       // [REQUIRED] Variable name
        "MyModName",        // [REQUIRED] Mod name, used for the config file name.
        "SomeValue2",       // [REQUIRED] Default value, MUST exist in the list of values.
        new List<string>(){ "SomeValue1", "SomeValue2", "SomeValue3" }, // [REQUIRED] List of values in the list.
        IConfigBase.NetworkSync.NoSync, // [OPTIONAL] Which menu in the settings should it appear under?
        IConfigBase.Category.Gameplay,  // [OPTIONAL] Whether or not it should be synced between server and clients. IGNORED for IConfigControl.
        // [OPTIONAL] Predicate/Validation for a new value, returning false stops Var.Value from being changed. 
        (newValue) => 
        { 
            return ValidateChoice(newValue); 
        },
        // [OPTIONAL] Called whenever the value is sucessfully updated.
        () =>                           
        {
            LuaCsSetup.PrintCsMessage($"Value updated! Now: {myConfigList.Value}") 
        }
    );
    
    // Availability: CLIENT ONLY
    IControlConfig myKeybind = ConfigManage.AddConfigKeyOrMouseBind(
        "MyListVar4",           // [REQUIRED] Variable name
        "MyModName",            // [REQUIRED] Mod name, used for the config file name.
        new KeyOrMouse(Keys.C), // [REQUIRED] Default value. Use the "MouseButton" Enum for mouse binds.
        // [OPTIONAL] Called whenever the value is sucessfully updated.
        () =>                           
        {
            LuaCsSetup.PrintCsMessage($"Value updated! Now: {myKeybind.GetStringValue()}") 
        }   
    );
}
```
