using System;
using Random = UnityEngine.Random;

namespace ThronefallTAS.Commands;

public class SetRandomSeed : ITasCommand
{
    public int S0 = 0;
    public int S1 = 0;
    public int S2 = 0;
    public int S3 = 0;
    
    public void Apply(TasState state)
    {
        Random.state = new Random.State(){ s0 = S0, s1 = S1, s2 = S2, s3 = S3 };
        Plugin.Log.LogInfo($"seed {S0} {S1} {S2} {S3}");
    }
}