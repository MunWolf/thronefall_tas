using System.Collections;
using ThronefallTAS;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace ThronefallTAS.UI;

public static class UIManager
{
    public static readonly Color BackgroundColor = new(0.11f, 0.11f, 0.11f, 1.0f);
    public static readonly Color TransparentBackgroundColor = new(0.11f, 0.11f, 0.11f, 0.6f);
    public static readonly Color TextColor = new(0.78f, 0.65f, 0.46f, 1.0f);
    public static readonly Color ButtonTextColor = new(0.97f, 0.88f, 0.75f, 1.0f);
    public static readonly Color ButtonHoverTextColor = new(0.0f, 0.41f, 0.11f);
    public static readonly Color ExitButtonColor = new(0.176f, 0.165f, 0.149f);
    
    private static UIBase UiBase { get; set; }
    private static InfoPanel InfoPanel { get; set; }
    
    public static void Initialize()
    {
        UniverseLib.Universe.Init(OnInitialized, OnLogging);
    }
    
    private static void OnInitialized()
    {
        UiBase = UniversalUI.RegisterUI("com.badwolf.thronefall_mp", OnUpdate);
        var scaler = UiBase.Canvas.gameObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        UiBase.Canvas.overrideSorting = true;
        UiBase.Canvas.sortingOrder = 400000;
        UiBase.Canvas.UpdateCanvasRectTransform(true);
        InfoPanel = new InfoPanel(UiBase) { Enabled = Plugin.Instance.ShowInfoPanelOnStartup.Value };
    }
    
    private static void OnLogging(string text, LogType type)
    {
        switch (type)
        {
            case LogType.Assert:
            case LogType.Exception:
                Plugin.Log.LogFatal(text);
                break;
            case LogType.Error:
                Plugin.Log.LogError(text);
                break;
            case LogType.Warning:
                Plugin.Log.LogWarning(text);
                break;
            case LogType.Log:
            default:
                Plugin.Log.LogInfo(text);
                break;
        }
    }

    private static void OnUpdate()
    {
        
    }

    public static void ToggleInfoPanel()
    {
        InfoPanel.Enabled = !InfoPanel.Enabled;
    }
}