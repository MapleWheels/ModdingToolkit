global using System;
global using System.Collections;
global using System.Collections.Generic;
global using System.Collections.Concurrent;
global using System.Collections.Immutable;
global using System.Reflection;
global using System.Reflection.Emit;
global using System.Runtime.CompilerServices;
global using System.Linq;
global using Barotrauma;
global using Barotrauma.Extensions;
global using HarmonyLib;
global using Microsoft.Xna.Framework;
global using ModdingToolkit;

[assembly: IgnoresAccessChecksTo("Barotrauma")]
[assembly: IgnoresAccessChecksTo("DedicatedServer")]
[assembly: IgnoresAccessChecksTo("NetScriptAssembly")]

namespace ModdingToolkitTestPlugin;