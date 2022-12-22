using System.Globalization;
using System.Text.RegularExpressions;
using Barotrauma.Networking;
using Barotrauma.Steam;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ModdingToolkit.Config;

namespace ModConfigManager;

public class MSettingsMenu : Barotrauma.SettingsMenu, ISettingsMenu
{
    private static readonly Dictionary<IConfigBase.Category, List<IConfigBase>?> PerMenuConfig = new();
    private static readonly List<IConfigControl?> ControlsMenuConfig = new();
    private static readonly List<(object, IConfigBase)> ValuesToSave = new();
    private List<IConfigBase> OrganizedList = new();

    public readonly Dictionary<GUIButton, Func<LocalizedString>> CustomInputValueNameGetters = new();

    private record ResetHandle(string id, System.Action handle);
    private readonly Dictionary<IConfigBase.Category, List<ResetHandle>> PerMenuResetHandles = new();

    public MSettingsMenu(RectTransform mainParent, GameSettings.Config setConfig = default) : base(mainParent, setConfig)
    {
    }
    

    private static void Init()
    {
        foreach (IConfigBase.Category category in Enum.GetValues<IConfigBase.Category>())
        {
            if (!PerMenuConfig.ContainsKey(category))
                PerMenuConfig.Add(category, new List<IConfigBase>());
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
        LuaCsSetup.PrintCsMessage("MCMC: Create Invoke.");
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
        void addInputToRow(GUILayoutGroup currRow, LocalizedString labelText, 
            Func<LocalizedString> valueNameGetter, Action<KeyOrMouse> valueSetter, bool isLegacyBind = false)
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
                new RectTransform((0.4f, 1.0f), 
                    inputFrame.RectTransform, Anchor.TopRight, Pivot.TopRight),
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
                if (input is null)
                    continue;
                
                addInputToRowCustom(
                    currentRow,
                    $"{input.ModName}: {input.Name}",
                    () =>
                    {
                        foreach ((object, IConfigBase) tuple in ValuesToSave)
                        {
                            if (tuple.Item2 is IConfigControl icc
                                && icc == input
                                && tuple.Item1 is KeyOrMouse km)
                            {
                                return km.MouseButton == MouseButton.None
                                    ? new RawLString(FormatControlString(km.Key.ToString()))
                                    : new RawLString(km.MouseButton.ToString());
                            }
                        }
                        return new RawLString(FormatControlString(input.GetStringValue()));
                    },
                    kom =>
                    {
                        if (kom.MouseButton == MouseButton.None)
                        {
                            if (input.ValidateString(kom.Key.ToString()))
                                AddOrUpdateUnsavedChange(kom, input);
                        }
                        else if (input.ValidateString(kom.MouseButton.ToString()))
                        {
                            AddOrUpdateUnsavedChange(kom, input);
                        }

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
                    unsavedConfig.InventoryKeyMap = GameSettings.Config.InventoryKeyMapping.GetDefault();
                    unsavedConfig.KeyMap = GameSettings.Config.KeyMapping.GetDefault();
                    ValuesToSave.Clear();
                    foreach (IConfigControl? control in ControlsMenuConfig)
                    {
                        if (control is null)
                            continue;
                        AddOrUpdateUnsavedChange(control.DefaultValue, control);
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

    public void AddOrUpdateUnsavedChange(object newVal, IConfigBase config)
    {
        ValuesToSave.RemoveAll(tuple => tuple.Item2.Equals(config));
        ValuesToSave.Add((newVal, config));
    }

    public object? GetUnsavedValue(IConfigBase config)
    {
        foreach ((object, IConfigBase) tuple in ValuesToSave)
        {
            if (tuple.Item2.Equals(config))
            {
                return tuple.Item1;
            }
        }

        return null;
    }

    public new void CreateGameplayTab()
    {
        GUIFrame content = CreateNewContentFrame(Tab.Gameplay);
        var (left, right) = CreateSidebars(content);
        
        #region LEFT_FRAME
        var languages = TextManager.AvailableLanguages
            .OrderBy(l => TextManager.GetTranslatedLanguageName(l).ToIdentifier())
            .ToArray();
        Label(left, TextManager.Get("Language"), GUIStyle.SubHeadingFont);
        Dropdown(left, v => TextManager.GetTranslatedLanguageName(v), null, languages, unsavedConfig.Language, v => unsavedConfig.Language = v);
        Spacer(left);
            
        Tickbox(left, TextManager.Get("PauseOnFocusLost"), TextManager.Get("PauseOnFocusLostTooltip"), unsavedConfig.PauseOnFocusLost, v => unsavedConfig.PauseOnFocusLost = v);
        Spacer(left);
            
        Tickbox(left, TextManager.Get("DisableInGameHints"), TextManager.Get("DisableInGameHintsTooltip"), unsavedConfig.DisableInGameHints, v => unsavedConfig.DisableInGameHints = v);
        var resetInGameHintsButton =
            new GUIButton(new RectTransform(new Vector2(1.0f, 1.0f), left.RectTransform),
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
        Spacer(left);

        Label(left, TextManager.Get("ShowEnemyHealthBars"), GUIStyle.SubHeadingFont);
        DropdownEnum(left, v => TextManager.Get($"ShowEnemyHealthBars.{v}"), null, unsavedConfig.ShowEnemyHealthBars, v => unsavedConfig.ShowEnemyHealthBars = v);
        Spacer(left);

        Label(left, TextManager.Get("HUDScale"), GUIStyle.SubHeadingFont);
        Slider(left, (0.75f, 1.25f), 51, Percentage, unsavedConfig.Graphics.HUDScale, v => unsavedConfig.Graphics.HUDScale = v);
        Label(left, TextManager.Get("InventoryScale"), GUIStyle.SubHeadingFont);
        Slider(left, (0.75f, 1.25f), 51, Percentage, unsavedConfig.Graphics.InventoryScale, v => unsavedConfig.Graphics.InventoryScale = v);
        Label(left, TextManager.Get("TextScale"), GUIStyle.SubHeadingFont);
        Slider(left, (0.75f, 1.25f), 51, Percentage, unsavedConfig.Graphics.TextScale, v => unsavedConfig.Graphics.TextScale = v);
            
#if !OSX
        Spacer(left);
        var statisticsTickBox = new GUITickBox(NewItemRectT(left), TextManager.Get("statisticsconsenttickbox"))
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

        #endregion

        #region RIGHT_FRAME

        if (!PerMenuResetHandles.ContainsKey(IConfigBase.Category.Gameplay))
            PerMenuResetHandles.Add(IConfigBase.Category.Gameplay, new List<ResetHandle>());
        else
            PerMenuResetHandles[IConfigBase.Category.Gameplay].Clear();
        
        if (PerMenuConfig[IConfigBase.Category.Gameplay] is null)
            return;
        List<IConfigBase> cfgList = PerMenuConfig[IConfigBase.Category.Gameplay]!;
        
        int groupCount = cfgList.GroupBy(x => x.ModName).Count();
        int entryCount = cfgList.Count;
        Vector2 entrySize = (1.0f, 0.119f);
        float size = Math.Max(1.0f, groupCount * 0.2f + entryCount * entrySize.Y);

        OrganizedList = cfgList
            .OrderBy(x => x.ModName)
            .ThenBy(x => x.Name)
            .ToList();
        
        #warning Remove Debug Messages
        LuaCsSetup.PrintCsMessage($"MENUDEV: GrpCnt={groupCount}, entryCnt={entryCount}, size={size}");
        foreach (IConfigBase configBase in OrganizedList)
        {
            LuaCsSetup.PrintCsMessage($"MENUDEV-LOOP: ModName={configBase.ModName}, Name={configBase.Name}");
        }

        Label(right, "Please note: You must press ENTER in the value textbox\nfor the new value to be registered!",
            GUIStyle.SmallFont);
        GUIListBox rightListBox = new GUIListBox(
            new RectTransform((1.0f, 0.9f), right.RectTransform),
            false,
            Color.DarkOliveGreen,
            "",
            true,
            false)
        {
            CanBeFocused = false,
            OnSelected = (_, _) => false
        };

        GUIButton resetAllVars = new GUIButton(new RectTransform((0.5f, 0.1f), right.RectTransform),
            "Reset", color: Color.Beige)
        {
            OnClicked = (button, o) =>
            {
                if (PerMenuResetHandles.ContainsKey(IConfigBase.Category.Gameplay))
                {
                    foreach (var resetHandle in PerMenuResetHandles[IConfigBase.Category.Gameplay])
                    {
                        resetHandle.handle?.Invoke();
                    }
                }
                return false;
            },
            OnAddedToGUIUpdateList = component =>
            {
                component.Enabled = CurrentTab == Tab.Gameplay;
            }
        };

        GUIFrame testFrame = new GUIFrame(
        new RectTransform((1.0f, size), rightListBox.Content.RectTransform),
        "", Color.DarkOliveGreen);

        GUILayoutGroup contentGroup = new GUILayoutGroup(
            new RectTransform((1.0f, 1.0f), testFrame.RectTransform),
            false);

        string _header = string.Empty;

        foreach (IConfigBase configBase in OrganizedList)
        {
            if (!_header.Equals(configBase.ModName))
            {
                _header = configBase.ModName;
                ModdingToolkit.Client.GUIUtil.Spacer(contentGroup, 1f/size);
                ModdingToolkit.Client.GUIUtil.Label(contentGroup, $"{_header} Settings", GUIStyle.LargeFont, 1f/size);
                ModdingToolkit.Client.GUIUtil.Spacer(contentGroup, 1f/size);
            }

            PerMenuResetHandles[IConfigBase.Category.Gameplay].Add(new ResetHandle(
                configBase.ModName+configBase.Name,
                AddListEntry(contentGroup, configBase, (1.0f, entrySize.Y/size), 1f/size))
            );
        }
        
        System.Action AddListEntry(GUILayoutGroup layoutGroup, IConfigBase entry, Vector2 scaleRatio, float yAdjustRatio = 1.0f)
        {
            ModdingToolkit.Client.GUIUtil.Label(layoutGroup, entry.Name, GUIStyle.SubHeadingFont, yAdjustRatio);
            if (entry.GetDisplayType() == IConfigBase.DisplayType.Tickbox)
            {
                var tickbox = ModdingToolkit.Client.GUIUtil.Tickbox(layoutGroup, "??", "??",
                    (bool)Convert.ChangeType(entry.GetStringValue(), TypeCode.Boolean),
                    (v) => AddOrUpdateUnsavedChange(v.ToString(), entry), yAdjustRatio);
                return () =>
                {
                    bool b;
                    try
                    {
                        b = (bool)Convert.ChangeType(entry.GetStringDefaultValue(), TypeCode.Boolean);
                    }
                    catch
                    {
                        b = false;
                    }

                    tickbox.Selected = b;
                    AddOrUpdateUnsavedChange(b.ToString(), entry);
                };
            }
            if (entry.GetDisplayType() == IConfigBase.DisplayType.DropdownList
                     && entry is IConfigList icl)
            {
                var dropdown = ModdingToolkit.Client.GUIUtil.Dropdown<string>(layoutGroup, 
                    s => new RawLString(s), 
                    s => new RawLString(s),
                    icl.GetReadOnlyList(), icl.Value, 
                    s => AddOrUpdateUnsavedChange(s, icl), yAdjustRatio);
                return () =>
                {
                    int index = icl.GetDefaultValueIndex();
                    if (index < 0)
                        return;
                    dropdown.Select(index);
                    AddOrUpdateUnsavedChange(icl.GetStringDefaultValue(), icl);
                };
            }
            if (entry.GetDisplayType() == IConfigBase.DisplayType.Slider)
            {
                if (entry is IConfigRangeFloat icf)
                {
                    float cv;
                    try
                    {
                        cv = (float)Convert.ChangeType(GetUnsavedValue(icf), TypeCode.Single)!;
                    }
                    catch
                    {
                        cv = icf.Value;
                    }

                    var (slider, label) = ModdingToolkit.Client.GUIUtil.Slider(layoutGroup,
                        new Vector2(icf.MinValue, icf.MaxValue), icf.Steps, 
                        f => f.ToString(), cv, 
                        f => AddOrUpdateUnsavedChange(f.ToString(), entry), LayoutYAdjust: yAdjustRatio);
                    return () =>
                    {
                        slider.BarScrollValue = icf.DefaultValue;
                        label.Text = (RichString)slider.BarScrollValue.ToString();
                        AddOrUpdateUnsavedChange(icf.DefaultValue.ToString(), icf);
                    };
                }
                if (entry is IConfigRangeInt ici)
                {
                    int cv;
                    try
                    {
                        cv = (int)Convert.ChangeType(GetUnsavedValue(ici), TypeCode.Int32)!;
                    }
                    catch
                    {
                        cv = ici.Value;
                    }

                    var (slider, label) = ModdingToolkit.Client.GUIUtil.Slider(layoutGroup,
                        new Vector2(ici.MinValue, ici.MaxValue), ici.Steps, 
                        f => f.ToString(), cv, 
                        f => AddOrUpdateUnsavedChange(f.ToString(), entry), LayoutYAdjust: yAdjustRatio);
                    
                    return () =>
                    {
                        slider.BarScrollValue = ici.DefaultValue;
                        label.Text = (RichString)slider.BarScrollValue.ToString();
                        AddOrUpdateUnsavedChange(ici.DefaultValue.ToString(), ici);
                    };
                }

                return () => { };
            }
            //GUINumberInput breaks formatting, includes unknown padding
            /*if (entry.GetDisplayType() == IConfigBase.DisplayType.Number)
            {
                var numInput = new ModConfigManager.CustomGUI.GUINumberInput(
                    new RectTransform(scaleRatio, layoutGroup.RectTransform),
                    NumberType.Float, yAdjustRatio: yAdjustRatio)
                {
                    OnValueChanged = input =>
                    {
                        AddOrUpdateUnsavedChange(input.FloatValue.ToString(), entry);
                    }
                };
                
                if (float.TryParse(entry.GetStringValue(), out var v))
                {
                    numInput.FloatValue = v;
                }

                return () =>
                {
                    if (float.TryParse(entry.GetStringDefaultValue(), out var fl))
                    {
                        numInput.FloatValue = fl;
                        AddOrUpdateUnsavedChange(entry.GetStringDefaultValue(), entry);
                    }
                };
            }*/
            if (entry.GetDisplayType() == IConfigBase.DisplayType.Standard
                     || entry.GetDisplayType() == IConfigBase.DisplayType.Number)
            {
                var textBox = new GUITextBox(
                    new RectTransform(scaleRatio, layoutGroup.RectTransform),
                    entry.GetStringValue(),
                    createPenIcon: false
                )
                {
                    OnEnterPressed = (box, text) =>
                    {
                        if (entry.ValidateString(text))
                            AddOrUpdateUnsavedChange(text, entry);
                        else
                        {
                            string s = String.Empty;
                            try
                            {
                                s = (string)Convert.ChangeType(GetUnsavedValue(entry), TypeCode.String)!;
                            }
                            catch
                            {
                                s = entry.GetStringValue();
                            }

                            box.Text = s;
                        }
                        return true;
                    }
                };
                return () =>
                {
                    string s = entry.GetStringDefaultValue();
                    textBox.Text = s;
                    AddOrUpdateUnsavedChange(s, entry);
                };
            }

            return () => { };
        }
        
        #endregion
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
        Dropdown(
            left, 
            (m) => $"{m.Width}x{m.Height}", 
            null, 
            supportedResolutions, 
            currentResolution,
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
        ValuesToSave.Clear();

        if (GameMain.Client is null || GameSettings.CurrentConfig.Audio.VoiceSetting == VoiceMode.Disabled)
        {
            VoipCapture.Instance?.Dispose();
        }
        mainFrame.Parent.RemoveChild(mainFrame);
        if (Instance == this) { Instance = null; }

        GUI.SettingsMenuOpen = false;
    }
    
    public new void ApplyInstalledModChanges()
    {
        GameSettings.SetCurrentConfig(unsavedConfig);
        
        foreach ((object, IConfigBase) tuple in ValuesToSave)
        {
            if (tuple.Item1 is KeyOrMouse { } kom 
               && tuple.Item2 is IConfigControl { } icc)
            {
                if (kom.MouseButton == MouseButton.None)
                    icc.SetValueFromString(kom.Key.ToString());
                else
                    icc.SetValueFromString(kom.MouseButton.ToString());
            }
            else if (tuple.Item1 is string { } s)
                tuple.Item2.SetValueFromString(s);
            
            bool save = ConfigManager.Save(tuple.Item2);
            LuaCsSetup.PrintCsMessage($"Saving Config: {tuple.Item2.ModName}:{tuple.Item2.Name}. Success: {save}");
        }
        ValuesToSave.Clear();
        
        if (WorkshopMenu is MutableWorkshopMenu mutableWorkshopMenu &&
            mutableWorkshopMenu.CurrentTab == MutableWorkshopMenu.Tab.InstalledMods)
        {
            mutableWorkshopMenu.Apply();
        }
        GameSettings.SaveCurrentConfig();
    }

    private static readonly Regex rg = new Regex(@"^D{1,}[0-9]$");
    protected string FormatControlString(string s)
    {
        if (rg.IsMatch(s))
            s = s.Replace("D", "");
        return s;
    }
}