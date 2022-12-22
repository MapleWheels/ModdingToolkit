using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Linq;
using Barotrauma;
using Barotrauma.Extensions;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace ModdingToolkit.Client;

/// <summary>
/// A collection of helper GUI functions. Mostly ripped from "Barotrauma/ClientSource/Settings/SettingsMenu.cs"
/// </summary>
public class GUIUtil
{
    public static (GUILayoutGroup Left, GUILayoutGroup Right) CreateSidebars(GUIFrame parent, bool split = false)
    {
        GUILayoutGroup layout = new GUILayoutGroup(new RectTransform(Vector2.One, parent.RectTransform), isHorizontal: true);
        GUILayoutGroup left = new GUILayoutGroup(new RectTransform((0.4875f, 1.0f), layout.RectTransform), isHorizontal: false);
        var centerFrame = new GUIFrame(new RectTransform((0.025f, 1.0f), layout.RectTransform), style: null);
        if (split)
        {
            new GUICustomComponent(new RectTransform(Vector2.One, centerFrame.RectTransform),
                onDraw: (sb, c) =>
                {
                    sb.DrawLine((c.Rect.Center.X, c.Rect.Top),
                        (c.Rect.Center.X, c.Rect.Bottom),
                        GUIStyle.TextColorDim,
                        2f);
                });
        }
        GUILayoutGroup right = new GUILayoutGroup(new RectTransform((0.4875f, 1.0f), layout.RectTransform), isHorizontal: false);
        return (left, right);
    }
    
    public static GUILayoutGroup CreateCenterLayout(GUIFrame parent)
        => new GUILayoutGroup(new RectTransform((0.5f, 1.0f), parent.RectTransform, Anchor.TopCenter, Pivot.TopCenter)) { ChildAnchor = Anchor.TopCenter };

    public static RectTransform NewItemRectT(GUILayoutGroup parent, float YAdjustRatio = 1.0f)
        => new RectTransform((1.0f, 0.06f * YAdjustRatio), parent.RectTransform, Anchor.CenterLeft);
    
    public static void Spacer(GUILayoutGroup parent, float yAdjustRatio = 1.0f) 
        => new GUIFrame(new RectTransform((1.0f, 0.03f * yAdjustRatio), parent.RectTransform, Anchor.CenterLeft), style: null);
    
    public static GUITextBlock Label(GUILayoutGroup parent, LocalizedString str, GUIFont font, float yAdjustRatio = 1.0f)
        => new GUITextBlock(NewItemRectT(parent, yAdjustRatio), str, font: font);
    
    public static GUIDropDown DropdownEnum<T>(GUILayoutGroup parent, Func<T, LocalizedString> textFunc, Func<T, LocalizedString>? tooltipFunc, T currentValue,
            Action<T> setter, float yAdjustRatio = 1.0f) where T : Enum
            => Dropdown(parent, textFunc, tooltipFunc, (T[])Enum.GetValues(typeof(T)), currentValue, setter, yAdjustRatio);
        
    public static GUIDropDown Dropdown<T>(GUILayoutGroup parent, Func<T, LocalizedString> textFunc, Func<T, LocalizedString>? tooltipFunc, IReadOnlyList<T> values, T currentValue, Action<T> setter, float yAdjustRatio = 1.0f)
    {
        var dropdown = new GUIDropDown(NewItemRectT(parent, yAdjustRatio));
        values.ForEach(v => dropdown.AddItem(text: textFunc(v), userData: v, toolTip: tooltipFunc?.Invoke(v) ?? null));
        int childIndex = values.IndexOf(currentValue);
        dropdown.Select(childIndex);
        dropdown.ListBox.ForceLayoutRecalculation();
        dropdown.ListBox.ScrollToElement(dropdown.ListBox.Content.GetChild(childIndex));
        dropdown.OnSelected = (dd, obj) =>
        {
            setter((T)obj);
            return true;
        };
        return dropdown;
    }

    public static (GUIScrollBar, GUITextBlock) Slider(GUILayoutGroup parent, Vector2 range, int steps, Func<float, string> labelFunc, float currentValue, Action<float> setter, LocalizedString? tooltip = null, float LayoutYAdjust = 1.0f)
    {
        var layout = new GUILayoutGroup(NewItemRectT(parent, LayoutYAdjust), isHorizontal: true);
        var slider = new GUIScrollBar(new RectTransform((0.72f, 1.0f), layout.RectTransform), style: "GUISlider")
        {
            Range = range,
            BarScrollValue = currentValue,
            Step = 1.0f / (float)(steps - 1),
            BarSize = 1.0f / steps
        };
        if (tooltip != null)
        {
            slider.ToolTip = tooltip;
        }
        var label = new GUITextBlock(new RectTransform((0.28f, 1.0f), layout.RectTransform),
            labelFunc(currentValue), wrap: false, textAlignment: Alignment.Center);
        slider.OnMoved = (sb, val) =>
        {
            label.Text = labelFunc(sb.BarScrollValue);
            setter(sb.BarScrollValue);
            return true;
        };
        return (slider, label);
    }

    public static GUITickBox Tickbox(GUILayoutGroup parent, LocalizedString label, LocalizedString tooltip, bool currentValue, Action<bool> setter, float yAdjustRatio = 1.0f)
    {
        var tickbox = new GUITickBox(NewItemRectT(parent, yAdjustRatio), label)
        {
            Selected = currentValue,
            ToolTip = tooltip,
            OnSelected = (tb) =>
            {
                setter(tb.Selected);
                return true;
            }
        };
        return tickbox;
    }
    
    public static string Percentage(float v) => ToolBox.GetFormattedPercentage(v);

    public static int Round(float v) => (int)MathF.Round(v);
}