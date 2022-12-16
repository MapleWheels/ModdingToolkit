using System.Globalization;
using Barotrauma.Networking;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ModdingToolkit.Config;

namespace ModConfigManager;

public class MSettingsMenu : Barotrauma.SettingsMenu, ISettingsMenu
{
    private static readonly Dictionary<IConfigBase.Category, List<IConfigBase>?> PerMenuConfig = new();
    private static readonly List<IConfigControl?> ControlsMenuConfig = new();
    public readonly Dictionary<GUIButton, Func<LocalizedString>> CustomInputValueNameGetters = new();

    public MSettingsMenu(RectTransform mainParent, GameSettings.Config setConfig = default) : base(mainParent, setConfig)
    {
    }

    private static void Init()
    {
        foreach (IConfigBase.Category category in Enum.GetValues<IConfigBase.Category>())
        {
            if (PerMenuConfig[category] is null)
                PerMenuConfig[category] = new List<IConfigBase>();
            else
                PerMenuConfig[category]!.Clear();
            foreach (IConfigBase member in ConfigManager.GetConfigMembers(category))
                PerMenuConfig[category]!.Add(member);
        }
        
        ControlsMenuConfig.Clear();
        foreach (IConfigControl member in ConfigManager.GetControlConfigs())
        {
            ControlsMenuConfig.Add(member);
        }
    }
    
    public new static Barotrauma.SettingsMenu Create(RectTransform mainParent)
    {
        Instance?.Close();
        Init();
        Instance = new MSettingsMenu(mainParent);
        return Instance;
    }

    public new void CreateAudioAndVCTab()
    {
#warning TODO: Implement custom menu.
        base.CreateAudioAndVCTab();
    }
    
    
    
    public new void CreateControlsTab()
    {
        //Note: Keep this code close to vanilla for easy porting of new features.

        GUIFrame content = CreateNewContentFrame(Tab.Controls);

        GUILayoutGroup layout = CreateCenterLayout(content);
        
        Label(layout, TextManager.Get("AimAssist"), GUIStyle.SubHeadingFont);
        Slider(layout, (0, 1), 101, Percentage, unsavedConfig.AimAssistAmount, (v) => unsavedConfig.AimAssistAmount = v, TextManager.Get("AimAssistTooltip"));
        Tickbox(layout, TextManager.Get("EnableMouseLook"), TextManager.Get("EnableMouseLookTooltip"), unsavedConfig.EnableMouseLook, (v) => unsavedConfig.EnableMouseLook = v);
        Spacer(layout);

        GUIListBox keyMapList =
            new GUIListBox(new RectTransform((2.0f, 0.7f),
                layout.RectTransform))
            {
                CanBeFocused = false,
                OnSelected = (_, __) => false
            };
        Spacer(layout);
        
        GUILayoutGroup createInputRowLayout()
            => new GUILayoutGroup(new RectTransform((1.0f, 0.1f), keyMapList.Content.RectTransform), isHorizontal: true);

        inputButtonValueNameGetters.Clear();
        CustomInputValueNameGetters.Clear();
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
                        inputBoxSelectedThisFrame = true;
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
        
        void addInputToRowCustom(GUILayoutGroup currRow, LocalizedString labelText, Func<LocalizedString> valueNameGetter, Action<KeyOrMouse> valueSetter)
        {
            var inputFrame = new GUIFrame(new RectTransform((0.5f, 1.0f), currRow.RectTransform),
                style: null);
            var label = new GUITextBlock(new RectTransform((0.6f, 1.0f), inputFrame.RectTransform), labelText,
                font: GUIStyle.SmallFont) {ForceUpperCase = ForceUpperCase.Yes};
            var inputBox = new GUIButton(
                new RectTransform((0.4f, 1.0f), inputFrame.RectTransform, Anchor.TopRight, Pivot.TopRight),
                valueNameGetter(), style: "GUITextBoxNoIcon")
            {
                OnClicked = (btn, obj) =>
                {
                    CustomInputValueNameGetters.Keys.ForEach(b =>
                    {
                        if (b != btn) { b.Selected = false; }
                    });
                    bool willBeSelected = !btn.Selected;
                    if (willBeSelected)
                    {
                        inputBoxSelectedThisFrame = true;
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
            CustomInputValueNameGetters.Add(inputBox, valueNameGetter);
        }

        var inputListener = new GUICustomComponent(
            new RectTransform(Vector2.Zero, 
                layout.RectTransform), 
            onUpdate: (deltaTime, component) =>
        {
            if (currentSetter is null) { return; }

            if (PlayerInput.PrimaryMouseButtonClicked() && inputBoxSelectedThisFrame)
            {
                inputBoxSelectedThisFrame = false;
                return;
            }

            void clearSetter()
            {
                currentSetter = null;
                inputButtonValueNameGetters.Keys.ForEach(b => b.Selected = false);
                CustomInputValueNameGetters.Keys.ForEach(b => b.Selected = false);
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
                    () => unsavedConfig.KeyMap.Bindings[input].Name,
                    (v) => unsavedConfig.KeyMap = unsavedConfig.KeyMap.WithBinding(input, v),
                    LegacyInputTypes.Contains(input));
            }
        }

        for (int i = 0; i < unsavedConfig.InventoryKeyMap.Bindings.Length; i += 2)
        {
            var currRow = createInputRowLayout();
            for (int j = 0; j < 2; j++)
            {
                int currIndex = i + j;
                if (currIndex >= unsavedConfig.InventoryKeyMap.Bindings.Length) { break; }

                var input = unsavedConfig.InventoryKeyMap.Bindings[currIndex];
                addInputToRow(
                    currRow,
                    TextManager.GetWithVariable(
                        "inventoryslotkeybind", 
                        "[slotnumber]", 
                        (currIndex + 1).ToString(CultureInfo.InvariantCulture)
                        ),
                    () => unsavedConfig.InventoryKeyMap.Bindings[currIndex].Name,
                    (v) => unsavedConfig.InventoryKeyMap = unsavedConfig.InventoryKeyMap.WithBinding(currIndex, v));
            }
        }

        for (int i = 0; i < ControlsMenuConfig.Count; i += 2)
        {
            var currentRow = createInputRowLayout();
            for (int j = 0; j < 2; j++)
            {
                int currentIndex = i + j;
                if (currentIndex >= ControlsMenuConfig.Count)
                    break;

                var input = ControlsMenuConfig[currentIndex];
                addInputToRowCustom(
                    currentRow,
                    $"{input.ModName}: {input.Name}",
                    () => new RawLString(input.GetStringValue()),
                    (kom => input.Value = kom)
                    );
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
                    unsavedConfig.InventoryKeyMap = GameSettings.Config.InventoryKeyMapping.GetDefault();
                    unsavedConfig.KeyMap = GameSettings.Config.KeyMapping.GetDefault();
                    foreach (IConfigControl? control in ControlsMenuConfig)
                    {
                        control?.SetValueAsDefault();
                    }
                    
                    foreach (var btn in inputButtonValueNameGetters.Keys)
                    {
                        btn.Text = inputButtonValueNameGetters[btn]();
                    }
                    foreach (var btn in CustomInputValueNameGetters.Keys)
                    {
                        btn.Text = CustomInputValueNameGetters[btn]();
                    }
                    
                    Instance?.SelectTab(Tab.Controls);
                    return true; 
                }
            };
    }

    public new void CreateGameplayTab()
    {
#warning TODO: Implement custom menu.
        base.CreateGameplayTab();
    }

    public new void CreateGraphicsTab()
    {
        GUIFrame content = CreateNewContentFrame(Tab.Graphics);

        var (left, right) = CreateSidebars(content);

        List<(int Width, int Height)> supportedResolutions =
            GameMain.GraphicsDeviceManager.GraphicsDevice.Adapter.SupportedDisplayModes
                .Where(m => m.Format == SurfaceFormat.Color)
                .Select(m => (m.Width, m.Height))
                .Where(m => m.Width >= GameSettings.Config.GraphicsSettings.MinSupportedResolution.X
                    && m.Height >= GameSettings.Config.GraphicsSettings.MinSupportedResolution.Y)
                .ToList();
        var currentResolution = (unsavedConfig.Graphics.Width, unsavedConfig.Graphics.Height);
        if (!supportedResolutions.Contains(currentResolution))
        {
            supportedResolutions.Add(currentResolution);
        }
        
        Label(left, TextManager.Get("Resolution"), GUIStyle.SubHeadingFont);
        Dropdown(left, (m) => $"{m.Width}x{m.Height}", null, supportedResolutions, currentResolution,
            (res) =>
            {
                unsavedConfig.Graphics.Width = res.Width;
                unsavedConfig.Graphics.Height = res.Height;
            });
        Spacer(left);

        Label(left, TextManager.Get("DisplayMode"), GUIStyle.SubHeadingFont);
        DropdownEnum(left, (m) => TextManager.Get($"{m}"), null, unsavedConfig.Graphics.DisplayMode, v => unsavedConfig.Graphics.DisplayMode = v);
        Spacer(left);

        Tickbox(left, TextManager.Get("EnableVSync"), TextManager.Get("EnableVSyncTooltip"), unsavedConfig.Graphics.VSync, v => unsavedConfig.Graphics.VSync = v);
        Tickbox(left, TextManager.Get("EnableTextureCompression"), TextManager.Get("EnableTextureCompressionTooltip"), unsavedConfig.Graphics.CompressTextures, v => unsavedConfig.Graphics.CompressTextures = v);
        
        Label(right, TextManager.Get("LOSEffect"), GUIStyle.SubHeadingFont);
        DropdownEnum(right, (m) => TextManager.Get($"LosMode{m}"), null, unsavedConfig.Graphics.LosMode, v => unsavedConfig.Graphics.LosMode = v);
        Spacer(right);

        Label(right, TextManager.Get("LightMapScale"), GUIStyle.SubHeadingFont);
        Slider(right, (0.5f, 1.0f), 11, v => TextManager.GetWithVariable("percentageformat", "[value]", Round(v * 100).ToString()).Value, unsavedConfig.Graphics.LightMapScale, v => unsavedConfig.Graphics.LightMapScale = v, TextManager.Get("LightMapScaleTooltip"));
        Spacer(right);

        Label(right, TextManager.Get("VisibleLightLimit"), GUIStyle.SubHeadingFont);
        Slider(right, (10, 210), 21, v => v > 200 ? TextManager.Get("unlimited").Value : Round(v).ToString(), unsavedConfig.Graphics.VisibleLightLimit, 
            v =>  unsavedConfig.Graphics.VisibleLightLimit = v > 200 ? int.MaxValue : Round(v), TextManager.Get("VisibleLightLimitTooltip"));
        Spacer(right);

        Tickbox(right, TextManager.Get("RadialDistortion"), TextManager.Get("RadialDistortionTooltip"), unsavedConfig.Graphics.RadialDistortion, v => unsavedConfig.Graphics.RadialDistortion = v);
        Tickbox(right, TextManager.Get("ChromaticAberration"), TextManager.Get("ChromaticAberrationTooltip"), unsavedConfig.Graphics.ChromaticAberration, v => unsavedConfig.Graphics.ChromaticAberration = v);

        Label(right, TextManager.Get("ParticleLimit"), GUIStyle.SubHeadingFont);
        Slider(right, (100, 1500), 15, v => Round(v).ToString(), unsavedConfig.Graphics.ParticleLimit, v => unsavedConfig.Graphics.ParticleLimit = Round(v));
        Spacer(right);
    }

    public new void Close()
    {
        ControlsMenuConfig.Clear();
        PerMenuConfig.Clear();

        if (GameMain.Client is null || GameSettings.CurrentConfig.Audio.VoiceSetting == VoiceMode.Disabled)
        {
            VoipCapture.Instance?.Dispose();
        }
        mainFrame.Parent.RemoveChild(mainFrame);
        if (Instance == this) { Instance = null; }

        GUI.SettingsMenuOpen = false;
    }
}