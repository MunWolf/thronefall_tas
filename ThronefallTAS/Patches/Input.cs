using HarmonyLib;
using Rewired;

namespace ThronefallTAS.Patches;

public static class Input
{
    [HarmonyPatch(typeof(UIFrame), "HandleMouseNavigation")]
    [HarmonyPrefix]
    private static bool HandleMouseNavigation()
    {
        return !Plugin.TasRunning;
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.GetAxis), typeof(string))]
    [HarmonyPrefix]
    private static bool GetAxis(ref float __result, string actionName)
    {
        if (!Plugin.TasRunning)
        {
            return true;
        }
        
        __result = Plugin.TasState.Axis(actionName);
        return false;
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.GetButton), typeof(string))]
    [HarmonyPrefix]
    private static bool GetButton(ref bool __result, string actionName)
    {
        if (!Plugin.TasRunning)
        {
            return true;
        }

        __result = Plugin.TasState.Action(actionName);
        return false;
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.GetButtonDown), typeof(string))]
    [HarmonyPrefix]
    private static bool GetButtonDown(ref bool __result, string actionName)
    {
        if (!Plugin.TasRunning)
        {
            return true;
        }

        __result = Plugin.TasState.ActionDown(actionName);
        return false;
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.GetButtonDown), typeof(string))]
    [HarmonyPrefix]
    private static bool GetButtonUp(ref bool __result, string actionName)
    {
        if (!Plugin.TasRunning)
        {
            return true;
        }

        __result = Plugin.TasState.ActionUp(actionName);
        return false;
    }
}