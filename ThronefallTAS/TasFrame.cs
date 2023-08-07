using System;
using System.Collections.Generic;
using ThronefallTAS.Commands;

namespace ThronefallTAS;

public class TasFrame
{
    public List<ITasCommand> Commands = new();

    public void Apply(TasState state)
    {
        foreach (var command in Commands)
        {
            command.Apply(state);
        }
    }
}