using UnityEngine;

public class DyeEventState : IPlayerState
{
    public void EnterState(Player player)
    {
        player.StopMovementSounds();
        player.DisableInput();
        player.velocity = Vector2.zero;
    }

    public void UpdateState(Player player)
    {

    }
}
