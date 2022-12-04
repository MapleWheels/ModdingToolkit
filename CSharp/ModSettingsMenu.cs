using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Linq;
using Barotrauma;
using Barotrauma.Extensions;
using HarmonyLib;
using Microsoft.Xna.Framework;
using ModConfigManager.Client.Patches;

namespace ModConfigManager.CSharp;

public class ModSettingsMenu : IModSettingsMenu
{
    public void CreateControlsTab(SettingsMenu instance, ref Dictionary<GUIButton, Func<LocalizedString>> inputButtonValueNameGetters, ref GameSettings.Config unsavedConfig,
        GUIFrame mainFrame, GUILayoutGroup tabber, GUIFrame contentFrame, GUILayoutGroup bottom,
        ImmutableHashSet<InputType> LegacyInputTypes)
    {
        throw new NotImplementedException();
    }

    public void CreateGameplayTab(SettingsMenu instance, ref Dictionary<GUIButton, Func<LocalizedString>> inputButtonValueNameGetters, ref GameSettings.Config unsavedConfig,
        GUIFrame mainFrame, GUILayoutGroup tabber, GUIFrame contentFrame, GUILayoutGroup bottom,
        ImmutableHashSet<InputType> LegacyInputTypes)
    {
        throw new NotImplementedException();
    }

    public void CreateGraphicsTab(SettingsMenu instance, ref Dictionary<GUIButton, Func<LocalizedString>> inputButtonValueNameGetters, ref GameSettings.Config unsavedConfig,
        GUIFrame mainFrame, GUILayoutGroup tabber, GUIFrame contentFrame, GUILayoutGroup bottom,
        ImmutableHashSet<InputType> LegacyInputTypes)
    {
        throw new NotImplementedException();
    }
}