using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ThronefallTAS.UI;
using UnityEngine;

namespace ThronefallTAS
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, "Thronefall TAS", PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Thronefall.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static System.Random Random = new();
        public static Plugin Instance { get; private set; }
        public static ManualLogSource Log { get; private set; }

        private void Awake()
        {
            Instance = this;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Log = Logger;

            UIManager.Initialize();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            
            // Patch all the methods.
            
            // Apply settings.
            Application.runInBackground = true;
        }

        private void Update()
        {
            
        }
    }
}
