using HarmonyLib;
using Rewired;

namespace ThronefallTAS.Patches;

public static class Input
{
    [HarmonyPatch(typeof(UIFrame), "HandleMouseNavigation")]
    [HarmonyPrefix]
    private static bool HandleMouseNavigation()
    {
        return !Plugin.Instance.Running;
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.GetAxis), typeof(string))]
    [HarmonyPrefix]
    private static bool GetAxis(ref float __result, string actionName)
    {
        if (!Plugin.Instance.Running)
        {
            return true;
        }
        
        __result = Plugin.Instance.State.Axis(actionName);
        return false;
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.GetButton), typeof(string))]
    [HarmonyPrefix]
    private static bool GetButton(ref bool __result, string actionName)
    {
        if (!Plugin.Instance.Running)
        {
            return true;
        }

        __result = Plugin.Instance.State.Action(actionName);
        return false;
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.GetButtonDown), typeof(string))]
    [HarmonyPrefix]
    private static bool GetButtonDown(ref bool __result, string actionName)
    {
        if (!Plugin.Instance.Running)
        {
            return true;
        }

        __result = Plugin.Instance.State.ActionDown(actionName);
        return false;
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.GetButtonDown), typeof(string))]
    [HarmonyPrefix]
    private static bool GetButtonUp(ref bool __result, string actionName)
    {
        if (!Plugin.Instance.Running)
        {
            return true;
        }

        __result = Plugin.Instance.State.ActionUp(actionName);
        return false;
    }
}