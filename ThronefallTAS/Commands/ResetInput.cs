namespace ThronefallTAS.Commands;

public class ResetInput : ITasCommand
{
    public void Apply(TasState state)
    {
        state.Reset();
        Plugin.Log.LogInfo($"reset");
    }
}