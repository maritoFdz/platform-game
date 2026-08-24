using UnityEngine;

public class SplitingState : IPlayerState
{
    public void EnterState(Player player)
    {
        player.forceSplit = true;
    }

    public void UpdateState(Player player)
    {
        if (!player.IsSplitting)
        {
            player.Split();
            player.forceSplit = false;
            player.SwitchState(player.idleState);
        }
    }
}
