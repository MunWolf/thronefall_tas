using System.Collections.Generic;

namespace ThronefallTAS;

public class TasState
{
    public readonly List<string> WaitForScenes = new();

    private readonly Dictionary<string, float> _axis = new();
    private readonly Dictionary<string, bool> _actions = new();
    private readonly HashSet<string> _actionsDown = new();
    private readonly HashSet<string> _actionsUp = new();

    public void Reset()
    {
        foreach (var action in _actions)
        {
            if (action.Value && !_actionsDown.Contains(action.Key))
            {
                _actionsUp.Add(action.Key);
            }
        }
        
        _axis.Clear();
        _actions.Clear();
        _actionsDown.Clear();
    }
    
    public float Axis(string axis)
    {
        return _axis.TryGetValue(axis, out var value) ? value : 0.0f;
    }

    public void SetAxis(string axis, float value)
    {
        _axis[axis] = value;
    }

    public bool Action(string action)
    {
        return _actions.TryGetValue(action, out var value) && value;
    }

    public bool ActionDown(string action)
    {
        return _actionsDown.Contains(action);
    }

    public bool ActionUp(string action)
    {
        return _actionsUp.Contains(action);
    }

    public void SetAction(string action, bool value)
    {
        var current = Action(action);
        if (current == value)
        {
            return;
        }

        _actions[action] = value;
        if (value)
        {
            UpdateActionUpDown(action, _actionsDown, _actionsUp);
        }
        else
        {
            UpdateActionUpDown(action, _actionsUp, _actionsDown);
        }
    }

    private static void UpdateActionUpDown(string action, ISet<string> toSet, ICollection<string> toCheck)
    {
        if (!toCheck.Contains(action))
        {
            toSet.Add(action);
        }
        else
        {
            toCheck.Remove(action);
        }
    }
    
    public void Update()
    {
        // Reset single frame dictionaries.
        _actionsDown.Clear();
        _actionsUp.Clear();
    }
}