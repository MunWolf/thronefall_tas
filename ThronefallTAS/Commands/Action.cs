namespace ThronefallTAS.Commands;

public class Action : ITasCommand
{
    public string Id;
    public bool Value;
    
    public void Apply(TasState state)
    {
        state.SetAction(Id, Value);
        Plugin.Log.LogInfo($"action {Id} -> {(Value ? "1" : "0")}");
    }
}