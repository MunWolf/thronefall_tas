using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ThronefallTAS.UI;
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
        public ConfigEntry<bool> ShowInfoPanelOnStartup;
        public ConfigEntry<string> RecordingPath;
        private ConfigEntry<string> _tasPath;
        
        public static Plugin Instance { get; private set; }
        public static ManualLogSource Log { get; private set; }

        public bool Recording => _recorder.Recording;
        public bool Running { get; private set; }
        public bool Paused => _paused || (_frameStepping && !_doFrameStep);
        
        public TasState State { get; private set; } = new();
        public int CurrentFrame { get; set; }

        private Recorder _recorder = new();
        private Dictionary<int, TasFrame> _tasFrames = new();
        private int _lastFrame;

        private bool _paused;
        private bool _frameStepping;
        private bool _doFrameStep;
        
        private void Awake()
        {
            Instance = this;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Log = Logger;

            _tasPath = Config.Bind("General", "TasPath", "C:/tas.txt", "Path to the Tas to launch");
            RecordingPath = Config.Bind("General", "RecordingPath", "C:/recording.txt", "Path to the file we record to");
            ShowInfoPanelOnStartup = Config.Bind("General", "ShowInfoAtStartup", false, "If the info panel is shown on startup");

            UIManager.Initialize();
            
            var harmony = new Harmony("com.badwolf.thronefall_tas");
            harmony.PatchAll(typeof(UpdateToFixedUpdate));
            harmony.PatchAll(typeof(Patches.Input));
            harmony.PatchAll(typeof(Recorder));
            
            // Apply settings.
            Application.runInBackground = true;
            // Force FPS to 60 so 
            UpdateToFixedUpdate.SetTargetFPS(60);
            SceneManager.sceneLoaded += OnSceneChanged;
            
            _lastFrame = Serializer.Load(_tasPath.Value, out _tasFrames);
        }

        private void StartTas()
        {
            Running = true;
            State = new TasState();
            CurrentFrame = 0;
        }
        
        private void Update()
        {
            HandleTasControls();
            _recorder.Update();
            
            if ((!Running && !Recording) || Paused || State.WaitForScenes.Count > 0)
            {
                return;
            }

            if (Running)
            {
                State.Update();
                if (_tasFrames.TryGetValue(CurrentFrame, out var frame))
                {
                    Log.LogInfo($"applying frame {CurrentFrame}");
                    frame.Apply(State);
                }

                if (CurrentFrame >= _lastFrame)
                {
                    Running = false;
                    return;
                }
            }
            
            ++CurrentFrame;
        }

        private void HandleTasControls()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
            {
                _lastFrame = Serializer.Load(_tasPath.Value, out _tasFrames);
                if (Running)
                {
                    Log.LogInfo($"Started TAS");
                    StartTas();
                }
            }
            
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
            {
                _paused = !_paused;
                Log.LogInfo(_paused ? "Paused TAS" : "Resumed TAS");
            }
            
            if (!Recording && UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            {
                Running = !Running;
                if (Running)
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

            if (!Running && UnityEngine.Input.GetKeyDown(KeyCode.Alpha9))
            {
                if (Recording)
                {
                    _recorder.Stop();
                }
                else
                {
                    CurrentFrame = 0;
                    _recorder.Start();
                }
                
                Log.LogInfo($"Toggle recording {Recording}");
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha0))
            {
                Log.LogInfo($"Toggle info panel");
                UIManager.ToggleInfoPanel();
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.O))
            {
                _frameStepping = !_frameStepping;
                Log.LogInfo($"Toggle frame stepping {_frameStepping}");
            }
            
            _doFrameStep = _frameStepping && UnityEngine.Input.GetKeyDown(KeyCode.P);
            Time.timeScale = Paused ? 0 : 1;
        }
        
        private void OnSceneChanged(Scene scene, LoadSceneMode mode)
        {
            Log.LogInfo($"Scene loaded: {scene.name}");
            State.WaitForScenes.Remove(scene.name);
        }
    }
}
