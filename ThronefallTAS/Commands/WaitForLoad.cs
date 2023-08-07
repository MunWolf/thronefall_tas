namespace ThronefallTAS.Commands;

public class WaitForLoad : ITasCommand
{
    public string Scene;
    
    public void Apply(TasState state)
    {
        state.WaitForScenes.Add(Scene);
        Plugin.Log.LogInfo($"wait on {Scene}");
    }
}