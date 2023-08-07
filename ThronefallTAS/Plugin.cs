using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ThronefallTAS.Patches;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThronefallTAS
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, "Thronefall TAS", PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Thronefall.exe")]
    [DefaultExecutionOrder(-10000000)]
    public class Plugin : BaseUnityPlugin
    {
        public enum State
        {
            Idle,
            Running,
            Paused
        }
        
        private ConfigEntry<string> _tasPath;
        
        public static ManualLogSource Log { get; private set; }

        public static bool TasRunning { get; private set; }
        public static TasState TasState { get; private set; } = new();

        private Dictionary<int, TasFrame> _tasFrames = new();
        private int _currentFrame;
        private bool _paused;
        
        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Log = Logger;

            _tasPath = Config.Bind("General", "TasPath", "C:/tas.txt", "Path to the Tas to launch");

            var harmony = new Harmony("com.badwolf.thronefall_tas");
            harmony.PatchAll(typeof(UpdateToFixedUpdate));
            harmony.PatchAll(typeof(Patches.Input));
            
            // Apply settings.
            Application.runInBackground = true;
            // Force FPS to 60 so 
            UpdateToFixedUpdate.SetTargetFPS(60);
            SceneManager.sceneLoaded += OnSceneChanged;
            
            _tasFrames = Serializer.Load(_tasPath.Value);
        }

        private void StartTas()
        {
            TasRunning = true;
            TasState = new TasState();
            _currentFrame = 0;
        }
        
        private void Update()
        {
            HandleTasControls();
            
            if (!TasRunning || _paused || TasState.WaitForScenes.Count > 0)
            {
                return;
            }

            
            if (_tasFrames.TryGetValue(_currentFrame, out var frame))
            {
                Log.LogInfo($"applying frame {_currentFrame}");
                frame.Apply(TasState);
            }
            
            ++_currentFrame;
        }

        private void HandleTasControls()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
            {
                _tasFrames = Serializer.Load(_tasPath.Value);
                if (TasRunning)
                {
                    Log.LogInfo($"Started TAS");
                    StartTas();
                }
            }
            
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
            {
                _paused = !_paused;
                Time.timeScale = _paused ? 0 : 1;
                Log.LogInfo(_paused ? "Paused TAS" : "Resumed TAS");
            }
            
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            {
                TasRunning = !TasRunning;
                if (TasRunning)
                {
                    Log.LogInfo($"Started TAS");
                    StartTas();
                }
                else
                {
                    Log.LogInfo($"Stopped TAS");
                }
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha5))
            {
                Log.LogInfo($"Go to Main Menu");
                SceneTransitionManager.instance.TransitionToMainMenu();
            }
        }
        
        private void OnSceneChanged(Scene scene, LoadSceneMode mode)
        {
            Log.LogInfo($"Scene loaded: {scene.name}");
            TasState.WaitForScenes.Remove(scene.name);
        }
    }
}
