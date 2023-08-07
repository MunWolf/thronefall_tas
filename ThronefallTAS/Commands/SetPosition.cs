using System.Linq;
using UnityEngine;

namespace ThronefallTAS.Commands;

public class SetPosition : ITasCommand
{
    public Vector3 Position;
    
    public void Apply(TasState state)
    {
        var player = PlayerManager.Instance.RegisteredPlayers.FirstOrDefault();
        if (player == null)
        {
            return;
        }

        var controller = player.GetComponent<CharacterController>();
        controller.enabled = false;
        player.transform.position = Position;
        controller.enabled = true;
        Plugin.Log.LogInfo($"position {Position}");
    }
}