namespace ThronefallTAS.Commands;

public class Axis : ITasCommand
{
    public string Id;
    public float Value;    
    
    public void Apply(TasState state)
    {
        state.SetAxis(Id, Value);
        Plugin.Log.LogInfo($"axis {Id} -> {Value}");
    }
}