using UnityEngine;

public class SplitingState : IPlayerState
{
    public void EnterState(Player player)
    {
        player.forceSplit = true;
        player.DoContinuousRumble();
    }

    public void UpdateState(Player player)
    {
        if (!player.IsSplitting)
        {
            player.StopContinuousRumble();
            player.Split();
            player.forceSplit = false;
            player.SwitchState(player.throwingState);
        }
    }
}
