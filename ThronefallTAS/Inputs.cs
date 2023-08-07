using System.Collections.Generic;

namespace ThronefallTAS;

public enum Input
{
    W,
    A,
    S,
    D,
    Space,
    Shift,
    Ctrl,
    Alt,
    Tab,
    Esc
}

public enum GameAction
{
    MenuApply,
    MenuRight,
    MenuLeft,
    MenuUp,
    MenuDown,
    PauseMenu,
    Cancel,
    Sprint,
    SprintToggle,
    PreviewBuildOptions,
    CallNight,
    LockTarget,
    CommandUnits,
    PauseMenuAndCancel,
    Interact,
}

public enum GameAxis
{
    MoveVertical,
    MoveHorizontal,
}

public static class Converter
{
    private static readonly Dictionary<string, Input> StringToInput = new()
    {
        { "w", Input.W },
        { "s", Input.S },
        { "a", Input.A },
        { "d", Input.D },
        { "space", Input.Space },
        { "shift", Input.Shift },
        { "ctrl", Input.Ctrl },
        { "alt", Input.Alt },
        { "tab", Input.Tab },
        { "esc", Input.Esc },
    };
    
    private static readonly Dictionary<Input, GameAction[]> InputToActions = new()
    {
        { Input.W, new [] { GameAction.MenuUp } },
        { Input.S, new [] { GameAction.MenuDown } },
        { Input.A, new [] { GameAction.MenuLeft } },
        { Input.D, new [] { GameAction.MenuRight } },
        { Input.Space, new [] { GameAction.Interact, GameAction.MenuApply, GameAction.CallNight } },
        { Input.Shift, new [] { GameAction.Sprint } },
        { Input.Ctrl, new [] { GameAction.CommandUnits } },
        { Input.Alt, new [] { GameAction.LockTarget, GameAction.PreviewBuildOptions } },
        { Input.Tab, new [] { GameAction.SprintToggle } },
        { Input.Esc, new [] { GameAction.Cancel, GameAction.PauseMenu, GameAction.PauseMenuAndCancel } },
    };

    private static readonly Dictionary<GameAction, string> ActionToString = new()
    {
        { GameAction.MenuApply, "Menu Apply" },
        { GameAction.MenuRight, "Menu Right" },
        { GameAction.MenuLeft, "Menu Left" },
        { GameAction.MenuUp, "Menu Up" },
        { GameAction.MenuDown, "Menu Down" },
        { GameAction.PauseMenu, "Pause Menu" },
        { GameAction.Cancel, "Cancel" },
        { GameAction.Sprint, "Sprint" },
        { GameAction.SprintToggle, "Sprint Toggle" },
        { GameAction.PreviewBuildOptions, "Preview Build Options" },
        { GameAction.CallNight, "Call Night" },
        { GameAction.LockTarget, "Lock Target" },
        { GameAction.CommandUnits, "Command Units" },
        { GameAction.PauseMenuAndCancel, "Pause Menu & Cancel" },
        { GameAction.Interact, "Interact" },
    };

    private static readonly Dictionary<Input, (GameAxis, bool)> InputToAxis = new()
    {
        { Input.W, (GameAxis.MoveVertical, true) },
        { Input.S, (GameAxis.MoveVertical, false) },
        { Input.A, (GameAxis.MoveHorizontal, true) },
        { Input.D, (GameAxis.MoveHorizontal, false) },
    };

    private static readonly Dictionary<GameAxis, string> AxisToString = new()
    {
        { GameAxis.MoveVertical, "Move Vertical" },
        { GameAxis.MoveHorizontal, "Move Horizontal" },
    };

    public static Input? GetInput(string str)
    {
        return StringToInput.TryGetValue(str, out var value) ? value : null;
    }
    
    public static IEnumerable<GameAction> GetActions(Input input)
    {
        return InputToActions[input];
    }

    public static string GetString(GameAction input)
    {
        return ActionToString[input];
    }
    
    public static (GameAxis, bool) GetAxis(Input input)
    {
        return InputToAxis[input];
    }

    public static string GetString(GameAxis input)
    {
        return AxisToString[input];
    }
}