using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Linq;
using Barotrauma;
using Barotrauma.Extensions;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ModConfigManager.Client.Patches;

namespace ModConfigManager.Client;

internal class ModSettingsMenu : IModSettingsMenu
{
    public static Dictionary<Tab, GUIFrame>? MainMenuScreen_menuTabs;
    public Bindable<Barotrauma.SettingsMenu, bool>? inputBoxSelectedThisFrame;
    public Bindable<Barotrauma.SettingsMenu, GameSettings.Config>? unsavedConfig;

    public ModSettingsMenu()
    {
        
    }

    public void ReloadModMenu()
    {
        MainMenuScreen_menuTabs = Unsafe.As<Dictionary<Tab, GUIFrame>>(
            AccessTools.DeclaredField(typeof(Barotrauma.MainMenuScreen), "menuTabs").GetValue(GameMain.MainMenuScreen));
        SettingsMenu.Create(MainMenuScreen_menuTabs![Tab.Settings].RectTransform);
    }

    public void Dispose()
    {
        MainMenuScreen_menuTabs = null;
        inputBoxSelectedThisFrame?.Dispose();
        unsavedConfig?.Dispose();
    }

    ~ModSettingsMenu()
    {
        Dispose();
    }

    public void CreateControlsTab(SettingsMenu instance, Dictionary<GUIButton, Func<LocalizedString>> inputButtonValueNameGetters, 
        GUIFrame mainFrame, GUILayoutGroup tabber, GUIFrame contentFrame, GUILayoutGroup bottom,
        ImmutableHashSet<InputType> LegacyInputTypes)
    {
        if (!IsReady())
            InitReflection(instance);
        
        GUIFrame content = instance.CreateNewContentFrame(SettingsMenu.Tab.Controls);
        
        GUILayoutGroup layout = GUIUtil.CreateCenterLayout(content);
            
        GUIUtil.Label(layout, TextManager.Get("AimAssist"), GUIStyle.SubHeadingFont);
        GUIUtil.Slider(layout, (0, 1), 101, GUIUtil.Percentage, unsavedConfig.Value.AimAssistAmount, (v) =>
        {
            var c = unsavedConfig.Value;
            c.AimAssistAmount = v;
            unsavedConfig.Value = c;
        }, TextManager.Get("AimAssistTooltip"));
        GUIUtil.Tickbox(layout, TextManager.Get("EnableMouseLook"), TextManager.Get("EnableMouseLookTooltip"), unsavedConfig.Value.EnableMouseLook,
            (v) =>
            {
                var c = unsavedConfig.Value;
                c.EnableMouseLook = v;
                unsavedConfig.Value = c;
            });
        GUIUtil.Spacer(layout);

        GUIListBox keyMapList =
            new GUIListBox(new RectTransform((2.0f, 0.7f),
                layout.RectTransform))
            {
                CanBeFocused = false,
                OnSelected = (_, __) => false
            };
        GUIUtil.Spacer(layout);
        
        GUILayoutGroup createInputRowLayout()
            => new GUILayoutGroup(new RectTransform((1.0f, 0.1f), keyMapList.Content.RectTransform), isHorizontal: true);

        inputButtonValueNameGetters.Clear();
        Action<KeyOrMouse>? currentSetter = null;
        void addInputToRow(GUILayoutGroup currRow, LocalizedString labelText, Func<LocalizedString> valueNameGetter, Action<KeyOrMouse> valueSetter, bool isLegacyBind = false)
        {
            var inputFrame = new GUIFrame(new RectTransform((0.5f, 1.0f), currRow.RectTransform),
                style: null);
            if (isLegacyBind)
            {
                labelText = TextManager.GetWithVariable("legacyitemformat", "[name]", labelText);
            }
            var label = new GUITextBlock(new RectTransform((0.6f, 1.0f), inputFrame.RectTransform), labelText,
                font: GUIStyle.SmallFont) {ForceUpperCase = ForceUpperCase.Yes};
            var inputBox = new GUIButton(
                new RectTransform((0.4f, 1.0f), inputFrame.RectTransform, Anchor.TopRight, Pivot.TopRight),
                valueNameGetter(), style: "GUITextBoxNoIcon")
            {
                OnClicked = (btn, obj) =>
                {
                    inputButtonValueNameGetters.Keys.ForEach(b =>
                    {
                        if (b != btn) { b.Selected = false; }
                    });
                    bool willBeSelected = !btn.Selected;
                    if (willBeSelected)
                    {
                        inputBoxSelectedThisFrame.Value = true;
                        currentSetter = (v) =>
                        {
                            valueSetter(v);
                            btn.Text = valueNameGetter();
                        };
                    }

                    btn.Selected = willBeSelected;
                    return true;
                }
            };
            if (isLegacyBind)
            {
                label.TextColor = Color.Lerp(label.TextColor, label.DisabledTextColor, 0.5f);
                inputBox.Color = Color.Lerp(inputBox.Color, inputBox.DisabledColor, 0.5f);
                inputBox.TextColor = Color.Lerp(inputBox.TextColor, label.DisabledTextColor, 0.5f);
            }
            inputButtonValueNameGetters.Add(inputBox, valueNameGetter);
        }

        var inputListener = new GUICustomComponent(new RectTransform(Vector2.Zero, layout.RectTransform), onUpdate: (deltaTime, component) =>
        {
            if (currentSetter is null) { return; }

            if (PlayerInput.PrimaryMouseButtonClicked() && inputBoxSelectedThisFrame.Value)
            {
                inputBoxSelectedThisFrame.Value = false;
                return;
            }

            void clearSetter()
            {
                currentSetter = null;
                inputButtonValueNameGetters.Keys.ForEach(b => b.Selected = false);
            }
            
            void callSetter(KeyOrMouse v)
            {
                currentSetter?.Invoke(v);
                clearSetter();
            }
            
            var pressedKeys = PlayerInput.GetKeyboardState.GetPressedKeys();
            if (pressedKeys?.Any() ?? false)
            {
                if (pressedKeys.Contains(Keys.Escape))
                {
                    clearSetter();
                }
                else
                {
                    callSetter(pressedKeys.First());
                }
            }
            else if (PlayerInput.PrimaryMouseButtonClicked() &&
                    (GUI.MouseOn == null || !(GUI.MouseOn is GUIButton) || GUI.MouseOn.IsChildOf(keyMapList.Content)))
            {
                callSetter(MouseButton.PrimaryMouse);
            }
            else if (PlayerInput.SecondaryMouseButtonClicked())
            {
                callSetter(MouseButton.SecondaryMouse);
            }
            else if (PlayerInput.MidButtonClicked())
            {
                callSetter(MouseButton.MiddleMouse);
            }
            else if (PlayerInput.Mouse4ButtonClicked())
            {
                callSetter(MouseButton.MouseButton4);
            }
            else if (PlayerInput.Mouse5ButtonClicked())
            {
                callSetter(MouseButton.MouseButton5);
            }
            else if (PlayerInput.MouseWheelUpClicked())
            {
                callSetter(MouseButton.MouseWheelUp);
            }
            else if (PlayerInput.MouseWheelDownClicked())
            {
                callSetter(MouseButton.MouseWheelDown);
            }
        });
        
        InputType[] inputTypes = (InputType[])Enum.GetValues(typeof(InputType));
        InputType[][] inputTypeColumns =
        {
            inputTypes.Take(inputTypes.Length - (inputTypes.Length / 2)).ToArray(),
            inputTypes.TakeLast(inputTypes.Length / 2).ToArray()
        };
        
        for (int i = 0; i < inputTypes.Length; i+=2)
        {
            var currRow = createInputRowLayout();
            for (int j = 0; j < 2; j++)
            {
                var column = inputTypeColumns[j];
                if (i / 2 >= column.Length) { break; }
                var input = column[i / 2];
                addInputToRow(
                    currRow,
                    TextManager.Get($"InputType.{input}"),
                    () => unsavedConfig.Value.KeyMap.Bindings[input].Name,
                    (v) =>
                    {
                        var c = unsavedConfig.Value;
                        c.KeyMap = c.KeyMap.WithBinding(input, v);
                        unsavedConfig.Value = c;
                    },
                    LegacyInputTypes.Contains(input));
            }
        }

        for (int i = 0; i < unsavedConfig.Value.InventoryKeyMap.Bindings.Length; i += 2)
        {
            var currRow = createInputRowLayout();
            for (int j = 0; j < 2; j++)
            {
                int currIndex = i + j;
                if (currIndex >= unsavedConfig.Value.InventoryKeyMap.Bindings.Length) { break; }

                var input = unsavedConfig.Value.InventoryKeyMap.Bindings[currIndex];
                addInputToRow(
                    currRow,
                    TextManager.GetWithVariable("inventoryslotkeybind", "[slotnumber]", (currIndex + 1).ToString(CultureInfo.InvariantCulture)),
                    () => unsavedConfig.Value.InventoryKeyMap.Bindings[currIndex].Name,
                    (v) =>
                    {
                        var c = unsavedConfig.Value;
                        c.InventoryKeyMap = c.InventoryKeyMap.WithBinding(currIndex, v);
                        unsavedConfig.Value = c;
                    });
            }
        }

        GUILayoutGroup resetControlsHolder =
            new GUILayoutGroup(new RectTransform((1.75f, 0.1f), layout.RectTransform), isHorizontal: true, childAnchor: Anchor.Center)
            {
                RelativeSpacing = 0.1f
            };

        var defaultBindingsButton =
            new GUIButton(new RectTransform(new Vector2(0.45f, 1.0f), resetControlsHolder.RectTransform),
                TextManager.Get("Reset"), style: "GUIButtonSmall")
            {
                ToolTip = TextManager.Get("SetDefaultBindingsTooltip"),
                OnClicked = (_, userdata) =>
                {
                    var c = unsavedConfig.Value;
                    c.InventoryKeyMap = GameSettings.Config.InventoryKeyMapping.GetDefault();
                    c.KeyMap = GameSettings.Config.KeyMapping.GetDefault();
                    foreach (var btn in inputButtonValueNameGetters.Keys)
                    {
                        btn.Text = inputButtonValueNameGetters[btn]();
                    }
                    instance.SelectTab(SettingsMenu.Tab.Controls);
                    unsavedConfig.Value = c;
                    return true; 
                }
            };
    }

    public void CreateGameplayTab(SettingsMenu instance, Dictionary<GUIButton, Func<LocalizedString>> inputButtonValueNameGetters, 
        GUIFrame mainFrame, GUILayoutGroup tabber, GUIFrame contentFrame, GUILayoutGroup bottom,
        ImmutableHashSet<InputType> LegacyInputTypes)
    {
        if (!IsReady())
            InitReflection(instance);
        
        GUIFrame content = instance.CreateNewContentFrame(SettingsMenu.Tab.Gameplay);

        GUILayoutGroup layout = GUIUtil.CreateCenterLayout(content);

        var languages = TextManager.AvailableLanguages
            .OrderBy(l => TextManager.GetTranslatedLanguageName(l).ToIdentifier())
            .ToArray();
        GUIUtil.Label(layout, TextManager.Get("Language"), GUIStyle.SubHeadingFont);
        GUIUtil.Dropdown(layout, (v) => TextManager.GetTranslatedLanguageName(v), null, languages, unsavedConfig.Value.Language, (v) =>
        {
            var c = unsavedConfig.Value;
            c.Language = v;
            unsavedConfig.Value = c;
        });
        GUIUtil.Spacer(layout);
        
        GUIUtil.Tickbox(layout, TextManager.Get("PauseOnFocusLost"), TextManager.Get("PauseOnFocusLostTooltip"), unsavedConfig.Value.PauseOnFocusLost, (v) =>
            {
                var c = unsavedConfig.Value;
                c.PauseOnFocusLost = v;
                unsavedConfig.Value = c;
            });
        GUIUtil.Spacer(layout);
        
        GUIUtil.Tickbox(layout, TextManager.Get("DisableInGameHints"), TextManager.Get("DisableInGameHintsTooltip"), unsavedConfig.Value.DisableInGameHints, (v) =>
            {
                var c = unsavedConfig.Value;
                c.DisableInGameHints = v;
                unsavedConfig.Value = c;
            });
        var resetInGameHintsButton =
            new GUIButton(new RectTransform(new Vector2(1.0f, 1.0f), layout.RectTransform),
                TextManager.Get("ResetInGameHints"), style: "GUIButtonSmall")
            {
                OnClicked = (button, o) =>
                {
                    var msgBox = new GUIMessageBox(TextManager.Get("ResetInGameHints"),
                        TextManager.Get("ResetInGameHintsTooltip"),
                        buttons: new[] { TextManager.Get("Yes"), TextManager.Get("No") });
                    msgBox.Buttons[0].OnClicked = (guiButton, o1) =>
                    {
                        IgnoredHints.Instance.Clear();
                        msgBox.Close();
                        return false;
                    };
                    msgBox.Buttons[1].OnClicked = msgBox.Close;
                    return false;
                }
            };
        GUIUtil.Spacer(layout);
        
        GUIUtil.Label(layout, TextManager.Get("HUDScale"), GUIStyle.SubHeadingFont);
        GUIUtil.Slider(layout, (0.75f, 1.25f), 51, GUIUtil.Percentage, unsavedConfig.Value.Graphics.HUDScale, (v) =>
            {
                var c = unsavedConfig.Value;
                c.Graphics.HUDScale = v;
                unsavedConfig.Value = c;
            });
        GUIUtil.Label(layout, TextManager.Get("InventoryScale"), GUIStyle.SubHeadingFont);
        GUIUtil.Slider(layout, (0.75f, 1.25f), 51, GUIUtil.Percentage, unsavedConfig.Value.Graphics.InventoryScale, (v) =>
            {
                var c = unsavedConfig.Value;
                c.Graphics.InventoryScale = v;
                unsavedConfig.Value = c;
            });
        GUIUtil.Label(layout, TextManager.Get("TextScale"), GUIStyle.SubHeadingFont);
        GUIUtil.Slider(layout, (0.75f, 1.25f), 51, GUIUtil.Percentage, unsavedConfig.Value.Graphics.TextScale, (v) =>
            {
                var c = unsavedConfig.Value;
                c.Graphics.TextScale = v;
                unsavedConfig.Value = c;
            });
#if !OSX
        GUIUtil.Spacer(layout);
        var statisticsTickBox = new GUITickBox(GUIUtil.NewItemRectT(layout), TextManager.Get("statisticsconsenttickbox"))
        {
            OnSelected = tickBox =>
            {
                GameAnalyticsManager.SetConsent(
                    tickBox.Selected
                        ? GameAnalyticsManager.Consent.Ask
                        : GameAnalyticsManager.Consent.No);
                return false;
            }
        };
#if DEBUG
        statisticsTickBox.Enabled = false;
#endif
        void updateGATickBoxToolTip()
            => statisticsTickBox.ToolTip = TextManager.Get($"GameAnalyticsStatus.{GameAnalyticsManager.UserConsented}");
        updateGATickBoxToolTip();
        
        var cachedConsent = GameAnalyticsManager.Consent.Unknown;
        var statisticsTickBoxUpdater = new GUICustomComponent(
            new RectTransform(Vector2.Zero, statisticsTickBox.RectTransform),
            onUpdate: (deltaTime, component) =>
        {
            bool shouldTickBoxBeSelected = GameAnalyticsManager.UserConsented == GameAnalyticsManager.Consent.Yes;
            
            bool shouldUpdateTickBoxState = cachedConsent != GameAnalyticsManager.UserConsented
                                            || statisticsTickBox.Selected != shouldTickBoxBeSelected;

            if (!shouldUpdateTickBoxState) { return; }

            updateGATickBoxToolTip();
            cachedConsent = GameAnalyticsManager.UserConsented;
            GUITickBox.OnSelectedHandler prevHandler = statisticsTickBox.OnSelected;
            statisticsTickBox.OnSelected = null;
            statisticsTickBox.Selected = shouldTickBoxBeSelected;
            statisticsTickBox.OnSelected = prevHandler;
            statisticsTickBox.Enabled = GameAnalyticsManager.UserConsented != GameAnalyticsManager.Consent.Error;
        });
#endif
        
        
    }

    public void CreateGraphicsTab(SettingsMenu instance, Dictionary<GUIButton, Func<LocalizedString>> inputButtonValueNameGetters, 
        GUIFrame mainFrame, GUILayoutGroup tabber, GUIFrame contentFrame, GUILayoutGroup bottom,
        ImmutableHashSet<InputType> LegacyInputTypes)
    {
        if (!IsReady())
            InitReflection(instance);
        
        GUIFrame content = instance.CreateNewContentFrame(SettingsMenu.Tab.Graphics);
        
        var (left, right) = GUIUtil.CreateSidebars(content);

            List<(int Width, int Height)> supportedResolutions =
                GameMain.GraphicsDeviceManager.GraphicsDevice.Adapter.SupportedDisplayModes
                    .Where(m => m.Format == SurfaceFormat.Color)
                    .Select(m => (m.Width, m.Height))
                    .Where(m => m.Width >= GameSettings.Config.GraphicsSettings.MinSupportedResolution.X
                        && m.Height >= GameSettings.Config.GraphicsSettings.MinSupportedResolution.Y)
                    .ToList();
            var currentResolution = (unsavedConfig.Value.Graphics.Width, unsavedConfig.Value.Graphics.Height);
            if (!supportedResolutions.Contains(currentResolution))
            {
                supportedResolutions.Add(currentResolution);
            }
            
            GUIUtil.Label(left, TextManager.Get("Resolution"), GUIStyle.SubHeadingFont);
            GUIUtil.Dropdown(left, (m) => $"{m.Width}x{m.Height}", null, supportedResolutions, currentResolution,
                (res) =>
                {
                    var c = unsavedConfig.Value;
                    c.Graphics.Width = res.Width;
                    c.Graphics.Height = res.Height;
                    unsavedConfig.Value = c;
                });
            GUIUtil.Spacer(left);

            GUIUtil.Label(left, TextManager.Get("DisplayMode"), GUIStyle.SubHeadingFont);
            GUIUtil.DropdownEnum(left, (m) => TextManager.Get($"{m}"), null, unsavedConfig.Value.Graphics.DisplayMode, (v) =>
            {
                var c = unsavedConfig.Value;
                c.Graphics.DisplayMode = v;
                unsavedConfig.Value = c;
            });
            GUIUtil.Spacer(left);

            GUIUtil.Tickbox(left, TextManager.Get("EnableVSync"), TextManager.Get("EnableVSyncTooltip"), unsavedConfig.Value.Graphics.VSync, (v) =>
                {
                    var c = unsavedConfig.Value;
                    c.Graphics.VSync = v;
                    unsavedConfig.Value = c;
                });
            GUIUtil.Tickbox(left, TextManager.Get("EnableTextureCompression"), TextManager.Get("EnableTextureCompressionTooltip"), unsavedConfig.Value.Graphics.CompressTextures, (v) =>
                {
                    var c = unsavedConfig.Value;
                    c.Graphics.CompressTextures = v;
                    unsavedConfig.Value = c;
                });
            
            GUIUtil.Label(right, TextManager.Get("LOSEffect"), GUIStyle.SubHeadingFont);
            GUIUtil.DropdownEnum(right, (m) => TextManager.Get($"LosMode{m}"), null, unsavedConfig.Value.Graphics.LosMode, (v) =>
                {
                    var c = unsavedConfig.Value;
                    c.Graphics.LosMode = v;
                    unsavedConfig.Value = c;
                });
            GUIUtil.Spacer(right);

            GUIUtil.Label(right, TextManager.Get("LightMapScale"), GUIStyle.SubHeadingFont);
            GUIUtil.Slider(right, (0.5f, 1.0f), 11, (v) => TextManager.GetWithVariable("percentageformat", "[value]", GUIUtil.Round(v * 100).ToString()).Value, unsavedConfig.Value.Graphics.LightMapScale, (v) =>
                {
                    var c = unsavedConfig.Value;
                    c.Graphics.LightMapScale = v;
                    unsavedConfig.Value = c;
                }, TextManager.Get("LightMapScaleTooltip"));
            GUIUtil.Spacer(right);

            GUIUtil.Label(right, TextManager.Get("VisibleLightLimit"), GUIStyle.SubHeadingFont);
            GUIUtil.Slider(right, (10, 210), 21, (v) => v > 200 ? TextManager.Get("unlimited").Value : GUIUtil.Round(v).ToString(), unsavedConfig.Value.Graphics.VisibleLightLimit,
                (v) =>
                {
                    var c = unsavedConfig.Value;
                    c.Graphics.VisibleLightLimit = v > 200 ? int.MaxValue : GUIUtil.Round(v);
                    unsavedConfig.Value = c;
                }, TextManager.Get("VisibleLightLimitTooltip"));
            GUIUtil.Spacer(right);

            GUIUtil.Tickbox(right, TextManager.Get("RadialDistortion"), TextManager.Get("RadialDistortionTooltip"), unsavedConfig.Value.Graphics.RadialDistortion, (v) =>
                {
                    var c = unsavedConfig.Value;
                    c.Graphics.RadialDistortion = v;
                    unsavedConfig.Value = c;
                });
            GUIUtil.Tickbox(right, TextManager.Get("ChromaticAberration"), TextManager.Get("ChromaticAberrationTooltip"), unsavedConfig.Value.Graphics.ChromaticAberration, (v) =>
                {
                    var c = unsavedConfig.Value;
                    c.Graphics.ChromaticAberration = v;
                    unsavedConfig.Value = c;
                });

            GUIUtil.Label(right, TextManager.Get("ParticleLimit"), GUIStyle.SubHeadingFont);
            GUIUtil.Slider(right, (100, 1500), 15, (v) => GUIUtil.Round(v).ToString(), unsavedConfig.Value.Graphics.ParticleLimit, (v) =>
                {
                    var c = unsavedConfig.Value;
                    c.Graphics.ParticleLimit = GUIUtil.Round(v);
                    unsavedConfig.Value = c;
                });
            GUIUtil.Spacer(right);
    }

    private void InitReflection(SettingsMenu instance)
    {
        MainMenuScreen_menuTabs = Unsafe.As<Dictionary<Tab, GUIFrame>>(
            AccessTools.DeclaredField(typeof(Barotrauma.MainMenuScreen), "menuTabs").GetValue(GameMain.MainMenuScreen));
        inputBoxSelectedThisFrame = new Bindable<SettingsMenu, bool>(nameof(inputBoxSelectedThisFrame)).Bind(instance);
        unsavedConfig = new Bindable<SettingsMenu, GameSettings.Config>(nameof(unsavedConfig)).Bind(instance);
    }
    //defs

    private bool IsReady() =>
        inputBoxSelectedThisFrame is not null
        && unsavedConfig is not null
        && inputBoxSelectedThisFrame.IsValid
        && unsavedConfig.IsValid;

    //copied from main menu layout-cast
    public enum Tab
    {
        NewGame = 0,
        LoadGame = 1,
        HostServer = 2,
        Settings = 3,
        Tutorials = 4,
        JoinServer = 5,
        CharacterEditor = 6,
        SubmarineEditor = 7,
        SteamWorkshop = 8,
        Credits = 9,
        Empty = 10
    }
}

/// <summary>
/// Reflection wrappers for private SettingsMenu methods.
/// </summary>
internal static class SettingsMenuExt
{
    public static readonly Type BT_SettingsMenu = typeof(Barotrauma.SettingsMenu);
    public static readonly MethodInfo SM_CreateNewContentFrame
        = AccessTools.DeclaredMethod(BT_SettingsMenu, "CreateNewContentFrame");
    public static readonly MethodInfo SM_CreateCenterLayout
        = AccessTools.DeclaredMethod(BT_SettingsMenu, "CreateCenterLayout");

    public static GUIFrame CreateNewContentFrame(this SettingsMenu instance, Barotrauma.SettingsMenu.Tab tab)
        => (GUIFrame)SM_CreateNewContentFrame.Invoke(instance, new object[]{tab})!;
    public static GUILayoutGroup CreateCenterLayout(GUIFrame parent) =>
        (GUILayoutGroup)SM_CreateCenterLayout.Invoke(null, new object[] { parent })!;

}