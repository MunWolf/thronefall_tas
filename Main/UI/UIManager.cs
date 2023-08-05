using System.Collections;
using UnityEngine;
using UniverseLib.UI;

namespace ThronefallTAS.UI;

public static class UIManager
{
    private static UIBase UiBase { get; set; }
    private static TasPanel TasPanel { get; set; }

    private const float PreventOpeningTimeout = 0.2f;
    private static float _preventOpeningTimer = 0.0f;
    
    public static void Initialize()
    {
        UniverseLib.Universe.Init(UIManager.OnInitialized, OnLogging);
    }
    
    private static void OnInitialized()
    {
        UiBase = UniversalUI.RegisterUI("com.badwolf.thronefall_mp", OnUpdate);
        TasPanel = new TasPanel(UiBase)
        {
            Enabled = false
        };
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
        _preventOpeningTimer -= Time.deltaTime;
    }

    public static void OpenNetworkPanel()
    {
        if (!TasPanel.Enabled && _preventOpeningTimer < 0.0f)
        {
            LocalGamestate.Instance.SetPlayerFreezeState(true);
            TasPanel.Enabled = true;
        }
    }

    public static void CloseNetworkPanel()
    {
        if (TasPanel.Enabled)
        {
            _preventOpeningTimer = PreventOpeningTimeout;
            LocalGamestate.Instance.SetPlayerFreezeState(false);
            TasPanel.Enabled = false;
        }
    }
}