using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Barotrauma.Networking;
using Barotrauma.Steam;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ModdingToolkit.Client;
using ModdingToolkit.Config;
using MonoMod.Utils;

namespace ModConfigManager;

public class MSettingsMenu : Barotrauma.SettingsMenu, ISettingsMenu
{
    private static readonly Dictionary<Category, List<IDisplayable>> PerMenuConfig = new();
    private static readonly List<DisplayableControl> ControlsMenuConfig = new();
    private static readonly List<(object, IDisplayable)> ValuesToSave = new();
    //private List<IDisplayable> OrganizedList = new();
    public readonly Dictionary<GUIButton, Func<LocalizedString>> CustomInputValueNameGetters = new();
    // Gameplay Tab
    private GUILayoutGroup? Gameplay_MasterLayout;
    private string? Gameplay_SelectedMod;
    private string? Gameplay_SelectedCategory;
    private string? Gameplay_KeywordFilter;
    public static readonly string GAMEPLAY_SEARCHBAR_DELIMITER = ";";

    private record ResetHandle(string id, System.Action handle);
    private readonly Dictionary<Category, List<ResetHandle>> PerMenuResetHandles = new();

    public MSettingsMenu(RectTransform mainParent, GameSettings.Config setConfig = default) : base(mainParent, setConfig)
    {
    }
    

    private static void Init()
    {
        ReloadConfigs();
    }

    private static void ReloadConfigs()
    {
        ControlsMenuConfig.Clear();
        PerMenuConfig.Clear();
        
        foreach (Category category in Enum.GetValues<Category>())
        {
            if (category is Category.Ignore or Category.Controls)   // Controls are handled separately.
                continue;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (!PerMenuConfig.ContainsKey(category) || PerMenuConfig[category] is null)
                PerMenuConfig[category] = new List<IDisplayable>();
            else
                PerMenuConfig[category].Clear();
            foreach (IDisplayable member in ConfigManager.GetDisplayableConfigs().Where(d => d.MenuCategory == category))
                PerMenuConfig[category].Add(member);
        }
        
        ControlsMenuConfig.AddRange(ConfigManager.GetControlConfigs());
    }
    
    public new static Barotrauma.SettingsMenu Create(RectTransform mainParent)
    {
        LuaCsSetup.PrintCsMessage("MCMC: Create Invoke.");
        Instance?.Close();
        Init();
        Instance = new MSettingsMenu(mainParent);
        return Instance;
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
                    $"{input.Displayable.ModName}: {input.Displayable.Name}",
                    () =>
                    {
                        foreach ((object, IConfigBase) tuple in ValuesToSave)
                        {
                            if (tuple.Item2 is IConfigControl icc
                                && icc == input.Control
                                && tuple.Item1 is KeyOrMouse km)
                            {
                                return km.MouseButton == MouseButton.None
                                    ? new RawLString(FormatControlString(km.Key.ToString()))
                                    : new RawLString(km.MouseButton.ToString());
                            }
                        }
                        return new RawLString(FormatControlString(input.Displayable.GetStringValue()));
                    },
                    kom =>
                    {
                        if (kom.MouseButton == MouseButton.None)
                        {
                            if (input.Displayable.ValidateString(kom.Key.ToString()))
                                AddOrUpdateUnsavedChange(kom, input.Displayable);
                        }
                        else if (input.Displayable.ValidateString(kom.MouseButton.ToString()))
                        {
                            AddOrUpdateUnsavedChange(kom, input.Displayable);
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
                    foreach (DisplayableControl displayableControl in ControlsMenuConfig)
                    {
                        AddOrUpdateUnsavedChange(displayableControl.Control.DefaultValue, displayableControl.Displayable);
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

    public void AddOrUpdateUnsavedChange(object newVal, IDisplayable config)
    {
        ValuesToSave.RemoveAll(tuple => tuple.Item2.Equals(config));
        ValuesToSave.Add((newVal, config));
    }

    public object? GetUnsavedValue(IDisplayable config)
    {
        foreach ((object, IDisplayable) tuple in ValuesToSave)
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
        
        // Layout
        Gameplay_MasterLayout = new GUILayoutGroup(new RectTransform((1f, 1f), content.RectTransform));
        Gameplay_KeywordFilter = "";
        Gameplay_SelectedMod = "";
        Gameplay_SelectedCategory = "";
        ReloadConfigs();
        Gameplay_GenerateGameplayScreen();
    }
    

    private bool KeywordFilterContainsAny(IDisplayable? displayable, string? keywords, bool trueIfKeywordsNull = true)
    {
        if (displayable is null)
            return false;
        if (keywords.IsNullOrWhiteSpace())
            return trueIfKeywordsNull;

        string[] keywordsS = keywords.Split(GAMEPLAY_SEARCHBAR_DELIMITER);
        
        Stack<string> keywordStack = new Stack<string>();

        foreach (string s in keywordsS)
        {
            if (!s.IsNullOrWhiteSpace())
                keywordStack.Push(s.ToLowerInvariant().Trim());
        }

        if (keywordStack.Count < 1)
            return trueIfKeywordsNull;
        
        return RecursiveCheck();
        
        bool RecursiveCheck()
        {
            string s = keywordStack.Pop();
            return
                (!displayable.Name.IsNullOrWhiteSpace() && displayable.Name.ToLowerInvariant().Contains(s))
                || (!displayable.DisplayName.IsNullOrWhiteSpace() && displayable.DisplayName.ToLowerInvariant().Contains(s))
                || (!displayable.ModName.IsNullOrWhiteSpace() && displayable.ModName.ToLowerInvariant().Contains(s))
                || (!displayable.DisplayModName.IsNullOrWhiteSpace() && displayable.DisplayModName.ToLowerInvariant().Contains(s))
                || (keywordStack.Count > 0 && RecursiveCheck());
        }
    }

    private void Gameplay_GenerateVanillaList(GUILayoutGroup containerGroup)
    {
        var languages = TextManager.AvailableLanguages
            .OrderBy(l => TextManager.GetTranslatedLanguageName(l).ToIdentifier())
            .ToArray();
        GUIUtil.Label(containerGroup, TextManager.Get("Language"), GUIStyle.SubHeadingFont, Vector2.One);
        GUIUtil.Dropdown(containerGroup, v => TextManager.GetTranslatedLanguageName(v), null, languages, unsavedConfig.Language, v => unsavedConfig.Language = v, Vector2.One);
        GUIUtil.Spacer(containerGroup, Vector2.One);
            
        GUIUtil.Tickbox(containerGroup, TextManager.Get("PauseOnFocusLost"), TextManager.Get("PauseOnFocusLostTooltip"), unsavedConfig.PauseOnFocusLost, v => unsavedConfig.PauseOnFocusLost = v, Vector2.One);
        GUIUtil.Spacer(containerGroup, Vector2.One);
            
        GUIUtil.Tickbox(containerGroup, TextManager.Get("DisableInGameHints"), TextManager.Get("DisableInGameHintsTooltip"), unsavedConfig.DisableInGameHints, v => unsavedConfig.DisableInGameHints = v, Vector2.One);
        var resetInGameHintsButton =
            new GUIButton(new RectTransform(new Vector2(1.0f, 1.0f), containerGroup.RectTransform),
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
        GUIUtil.Spacer(containerGroup, Vector2.One);

        GUIUtil.Label(containerGroup, TextManager.Get("ShowEnemyHealthBars"), GUIStyle.SubHeadingFont, Vector2.One);
        GUIUtil.DropdownEnum(containerGroup, v => TextManager.Get($"ShowEnemyHealthBars.{v}"), null, unsavedConfig.ShowEnemyHealthBars, v => unsavedConfig.ShowEnemyHealthBars = v, Vector2.One);
        GUIUtil.Spacer(containerGroup, Vector2.One);

        GUIUtil.Label(containerGroup, TextManager.Get("HUDScale"), GUIStyle.SubHeadingFont, Vector2.One);
        GUIUtil.Slider(containerGroup, (0.75f, 1.25f), 51, Percentage, unsavedConfig.Graphics.HUDScale, v => unsavedConfig.Graphics.HUDScale = v, null, Vector2.One);
        GUIUtil.Label(containerGroup, TextManager.Get("InventoryScale"), GUIStyle.SubHeadingFont, Vector2.One);
        GUIUtil.Slider(containerGroup, (0.75f, 1.25f), 51, Percentage, unsavedConfig.Graphics.InventoryScale, v => unsavedConfig.Graphics.InventoryScale = v, null, Vector2.One);
        GUIUtil.Label(containerGroup, TextManager.Get("TextScale"), GUIStyle.SubHeadingFont, Vector2.One);
        GUIUtil.Slider(containerGroup, (0.75f, 1.25f), 51, Percentage, unsavedConfig.Graphics.TextScale, v => unsavedConfig.Graphics.TextScale = v, null, Vector2.One);
            
#if !OSX
        GUIUtil.Spacer(containerGroup, Vector2.One);
        var statisticsTickBox = new GUITickBox(NewItemRectT(containerGroup), TextManager.Get("statisticsconsenttickbox"))
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

    private void Gameplay_GenerateGameplayScreen()
    {
        if (Gameplay_MasterLayout is null)
            return;

        #region DATA_DEF

        // Defs
        string modIncludeAllOption = "All Modded";
        string categoryIncludeAllOption = "All";
        
        // Mod List
        var modDisplayablesList = new List<IDisplayable>();
        
        if (PerMenuConfig.ContainsKey(Category.Gameplay)
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            && PerMenuConfig[Category.Gameplay] is not null
            && PerMenuConfig[Category.Gameplay].Any())
        {
            modDisplayablesList.AddRange(PerMenuConfig[Category.Gameplay]
                .Where(d => KeywordFilterContainsAny(d, Gameplay_KeywordFilter))
                .ToList());
        }

        // Build Mod List for Dropdown
        List<string> modListDropdown = new List<string>();
        modListDropdown.Add(modIncludeAllOption);
        modListDropdown.Add("Vanilla");
        modListDropdown.AddRange(modDisplayablesList
            .GroupBy(d => d.DisplayModName)
            .Select(d => d.Key));
        modListDropdown = modListDropdown.Distinct().ToList();

        //--- Set current mod selection
        if (Gameplay_SelectedMod.IsNullOrWhiteSpace() || !modListDropdown.Contains(Gameplay_SelectedMod))
            Gameplay_SelectedMod = modIncludeAllOption;
        
        // Build Category List for Dropdown
        List<string> categoryList = new List<string>();
        categoryList.Add(categoryIncludeAllOption);
        categoryList.AddRange(modDisplayablesList
            .Where(d => Gameplay_SelectedMod.Equals(modIncludeAllOption) || d.DisplayModName.Equals(Gameplay_SelectedMod))
            .GroupBy(d => d.DisplayCategory)
            .Select(dg => dg.Key));
        categoryList = categoryList.Distinct().ToList();

        //--- Set current category selection
        if (Gameplay_SelectedCategory.IsNullOrWhiteSpace() || !categoryList.Contains(Gameplay_SelectedCategory))
            Gameplay_SelectedCategory = categoryIncludeAllOption;
        
        // Filter list of displayables
        modDisplayablesList = modDisplayablesList
            .Where(d => 
                (d.DisplayModName.Equals(Gameplay_SelectedMod) || Gameplay_SelectedMod.Equals(modIncludeAllOption)) 
                && (d.DisplayCategory.Equals(Gameplay_SelectedCategory) || Gameplay_SelectedCategory.Equals(categoryIncludeAllOption))
            )
            .OrderBy(d => d.DisplayModName)
            .ThenBy(d => d.DisplayName)
            .ToList();

        #endregion

        #region GUI_MENU_HEADER

        // Begin building GUI Elements
        // Clear existing view
        GUIUtil.ClearChildElements(Gameplay_MasterLayout);
        
        // Header
        var topHeader = GUIUtil.Label(Gameplay_MasterLayout, "Gameplay Settings", GUIStyle.LargeFont, Vector2.One);
        GUIUtil.Spacer(Gameplay_MasterLayout, Vector2.One);
        
        // GUI Container Init
        var modListDropdownGroup = new GUILayoutGroup(
            new RectTransform((1f, 0.06f), Gameplay_MasterLayout.RectTransform), true, Anchor.CenterLeft);
        var categoryListDropdownGroup = new GUILayoutGroup(
            new RectTransform((1f, 0.06f), Gameplay_MasterLayout.RectTransform), true, Anchor.CenterLeft);
        var searchBarGroup = new GUILayoutGroup(
            new RectTransform((1f, 0.06f), Gameplay_MasterLayout.RectTransform), true, Anchor.CenterLeft);
        
        var modListLabelElement = GUIUtil.Label(modListDropdownGroup, new RawLString("Mod: "), GUIStyle.SubHeadingFont, (0.2f, 1f));
        var modListDropdownElement = GUIUtil.Dropdown(
            modListDropdownGroup,
            s => new RawLString(s),
            s => new RawLString(s),
            modListDropdown,
            Gameplay_SelectedMod,
            s =>
            {
                Gameplay_SelectedMod = s;
                Gameplay_GenerateGameplayScreen();
            },
            (0.79f, 1f));

        GUIUtil.Label(categoryListDropdownGroup, new RawLString("Category: "), GUIStyle.SubHeadingFont, (0.2f, 1f));
        GUIUtil.Dropdown(
            categoryListDropdownGroup,
            s => new RawLString(s),
            s => new RawLString(s),
            categoryList,
            Gameplay_SelectedCategory,
            s =>
            {
                Gameplay_SelectedCategory = s;
                Gameplay_GenerateGameplayScreen();
            },
            (0.79f, 1f));

        GUIUtil.Label(searchBarGroup, new RawLString("Search Mods: "), GUIStyle.SubHeadingFont, (0.2f, 1f));
        new GUITextBox(
            new RectTransform((0.79f, 1f), searchBarGroup.RectTransform),
            Gameplay_KeywordFilter)
        {
            OnEnterPressed = (box, text) =>
            {
                Gameplay_KeywordFilter = text;
                Gameplay_GenerateGameplayScreen();
                return true;
            }
        };

        #endregion

        #region GUI_SETTINGS_ENTRIES

        Vector2 entrySize = (1.0f, 0.122f);
        float groupGount = modDisplayablesList.GroupBy(d => d.DisplayModName).Count();
        float size = Math.Max(1.0f, 0.2f * groupGount + modDisplayablesList.Count * entrySize.Y);

        // Settings List
        var modDisplayListGroup = new GUILayoutGroup(
            new RectTransform((1f, 0.7f), Gameplay_MasterLayout.RectTransform));
        var modDisplayListBox = new GUIListBox(
            new RectTransform((1f, 1f), modDisplayListGroup.RectTransform),
            false,
            Color.Black)
        {
            CanBeFocused = false,
            OnSelected = (_, _) => false
        };

        GUIFrame displayableContentFrame = new GUIFrame(
            new RectTransform((1.0f, size), modDisplayListBox.Content.RectTransform),
            "", Color.DarkOliveGreen);

        GUILayoutGroup displayableContentGroup = new GUILayoutGroup(
            new RectTransform((1.0f, 1.0f), displayableContentFrame.RectTransform),
            false);
        
        if (Gameplay_SelectedMod.Equals("Vanilla"))
        {
            Gameplay_GenerateVanillaList(displayableContentGroup);
            return;
        }
        
        // Reset button
        GUIButton resetAllVars = new GUIButton(new RectTransform((0.2f, 0.1f), Gameplay_MasterLayout.RectTransform),
            "Reset", color: Color.Beige)
        {
            OnClicked = (button, o) =>
            {
                if (PerMenuResetHandles.ContainsKey(Category.Gameplay))
                {
                    foreach (var resetHandle in PerMenuResetHandles[Category.Gameplay])
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
        
        // Populate displayable GUIListBox

        string _header = string.Empty;

        if (!PerMenuResetHandles.ContainsKey(Category.Gameplay))
            PerMenuResetHandles.Add(Category.Gameplay, new List<ResetHandle>());
        
        foreach (IDisplayable displayable in modDisplayablesList)
        {
            if (!_header.Equals(displayable.DisplayModName))
            {
                _header = displayable.DisplayModName;
                GUIUtil.Spacer(displayableContentGroup, new Vector2(1f, 1f/size));
                GUIUtil.Label(displayableContentGroup, $"{_header}", GUIStyle.LargeFont, new Vector2(1f, 1f/size));
                GUIUtil.Spacer(displayableContentGroup, new Vector2(1f, 1f/size));
            }

            PerMenuResetHandles[Category.Gameplay].Add(new ResetHandle(
                displayable.ModName+displayable.Name,
                AddListEntry(displayableContentGroup, displayable, (1.0f, entrySize.Y/size), new Vector2(1f, 1f/size)))
            );
        }

        #endregion
    }

    private System.Action AddListEntry(GUILayoutGroup layoutGroup, IDisplayable entry, Vector2 scaleRatio, Vector2 adjustRatio)
    {
        GUIUtil.Label(layoutGroup, new RawLString(entry.DisplayName), GUIStyle.SmallFont, adjustRatio);
        if (entry.GetDisplayType() == DisplayType.Tickbox)
        {
            var tickbox = GUIUtil.Tickbox(layoutGroup, "", new RawLString(entry.Tooltip),
                (bool)Convert.ChangeType(entry.GetStringValue(), TypeCode.Boolean),
                (v) => AddOrUpdateUnsavedChange(v.ToString(), entry), adjustRatio);
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
        if (entry.GetDisplayType() == DisplayType.DropdownList
                 && entry is IConfigList icl)
        {
            var dropdown = GUIUtil.Dropdown<string>(layoutGroup, 
                s => new RawLString(s), 
                s => new RawLString(entry.Tooltip),
                icl.GetReadOnlyList(), icl.Value, 
                s => AddOrUpdateUnsavedChange(s, entry), adjustRatio);
            return () =>
            {
                int index = icl.GetDefaultValueIndex();
                if (index < 0)
                    return;
                dropdown.Select(index);
                AddOrUpdateUnsavedChange(icl.GetStringDefaultValue(), entry);
            };
        }
        if (entry.GetDisplayType() == DisplayType.Slider)
        {
            if (entry is IConfigRangeFloat icf)
            {
                float cv;
                try
                {
                    cv = (float)Convert.ChangeType(GetUnsavedValue(entry), TypeCode.Single)!;
                }
                catch
                {
                    cv = icf.Value;
                }

                var (slider, label) = GUIUtil.Slider(layoutGroup,
                    new Vector2(icf.MinValue, icf.MaxValue), icf.Steps, 
                    f => f.ToString(), cv, 
                    f => AddOrUpdateUnsavedChange(f.ToString(), entry),new RawLString(entry.Tooltip), adjustRatio);
                return () =>
                {
                    slider.BarScrollValue = icf.DefaultValue;
                    label.Text = (RichString)slider.BarScrollValue.ToString();
                    AddOrUpdateUnsavedChange(icf.DefaultValue.ToString(), entry);
                };
            }
            if (entry is IConfigRangeInt ici)
            {
                int cv;
                try
                {
                    cv = (int)Convert.ChangeType(GetUnsavedValue(entry), TypeCode.Int32)!;
                }
                catch
                {
                    cv = ici.Value;
                }

                var (slider, label) = GUIUtil.Slider(layoutGroup,
                    new Vector2(ici.MinValue, ici.MaxValue), ici.Steps, 
                    f => f.ToString(), cv, 
                    f => AddOrUpdateUnsavedChange(f.ToString(), entry), new RawLString(entry.Tooltip), adjustRatio);
                
                return () =>
                {
                    slider.BarScrollValue = ici.DefaultValue;
                    label.Text = (RichString)slider.BarScrollValue.ToString();
                    AddOrUpdateUnsavedChange(ici.DefaultValue.ToString(), entry);
                };
            }

            return () => { };
        }
        if (entry.GetDisplayType() == DisplayType.Standard
            || entry.GetDisplayType() == DisplayType.Number)
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