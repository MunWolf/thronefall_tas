using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ThronefallTAS.Patches;

public static class UpdateToFixedUpdate
{
    private static int _targetFPS = 60;
    private static bool _allowRealDelta = true;
    private static float TargetDeltaTime => 1.0f / _targetFPS;

    public static void SetTargetFPS(int fps)
    {
        _targetFPS = fps;
        Application.targetFrameRate = _targetFPS;
        Time.captureFramerate = _targetFPS;
        Time.fixedDeltaTime = TargetDeltaTime;
        Time.maximumDeltaTime = TargetDeltaTime;
        _allowRealDelta = false;
        //QualitySettings.vSyncCount = 0;
    }

    [HarmonyPatch(typeof(Time), nameof(Time.deltaTime), MethodType.Getter)]
    [HarmonyPostfix]
    private static void DeltaTime(ref float __result)
    {
        if (!_allowRealDelta)
        {
            __result = TargetDeltaTime * Time.timeScale;
        }
    }
}