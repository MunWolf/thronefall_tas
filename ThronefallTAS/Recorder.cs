using System.Collections.Generic;
using HarmonyLib;
using ThronefallTAS.Commands;

namespace ThronefallTAS;

public class Recorder
{
    public bool Recording { get; private set; }

    private static readonly Queue<string> StartedLoading = new();
    
    private readonly Dictionary<int, Serializer.InputFrame> _values = new();
    private Serializer.InputFrame stateFrame;

    private Serializer.InputFrame GetFrame(int frame)
    {
        if (!_values.TryGetValue(Plugin.Instance.CurrentFrame, out var list))
        {
            list = new Serializer.InputFrame();
            _values[frame] = list;
        }

        return list;
    }
    
    public void Update()
    {
        if (!Recording || Plugin.Instance.Paused)
        {
            return;
        }

        foreach (var v in typeof(Input).GetEnumValues())
        {
            var value = (Input)v;
            var last = stateFrame.Input(value);
            var state = false;
            foreach (var code in Converter.GetKeyCodes(value))
            {
                state |= UnityEngine.Input.GetKey(code);
            }
            
            if (last != state)
            {
                GetFrame(Plugin.Instance.CurrentFrame).Inputs[value] = state;
                stateFrame.Inputs[value] = state;
            }
        }

        while (StartedLoading.Count > 0)
        {
            var scene = StartedLoading.Dequeue();
            GetFrame(Plugin.Instance.CurrentFrame).Loads.Add(scene);
            Plugin.Instance.State.WaitForScenes.Add(scene);
        }
    }
    
    public void Start()
    {
        Recording = true;
        _values.Clear();
        stateFrame = new Serializer.InputFrame();
        GetFrame(Plugin.Instance.CurrentFrame).State = UnityEngine.Random.state;
    }
    
    public void Stop()
    {
        Recording = false;
        Serializer.Save(Plugin.Instance.RecordingPath.Value, _values);
        Plugin.Log.LogInfo($"Recording saved to '{Plugin.Instance.RecordingPath.Value}'");
    }

    [HarmonyPatch(typeof(SceneTransitionManager), "TransitionToScene")]
    [HarmonyPrefix]
    public static void TransitionToScene(string _scenename)
    {
        StartedLoading.Enqueue(_scenename);
    }
}