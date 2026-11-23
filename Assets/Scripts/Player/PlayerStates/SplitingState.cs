using UnityEngine;

public class SplitingState : IPlayerState
{
    private bool endTriggered;

    public void EnterState(Player player)
    {
        endTriggered = false;
        player.forceSplit = player.IsSplitHeld;
        player.DoContinuousRumble();
        player.StartSplitAnimation();
    }

    public void UpdateState(Player player)
    {
        if (!player.IsSplitting && !endTriggered)
        {
            endTriggered = true;
            player.EndSplitAnimation();
        }

        if (endTriggered && player.splitAnimComplete)
        {
            player.splitAnimComplete = false;
            player.StopContinuousRumble();
            player.Split();
            player.forceSplit = false;
            player.SwitchState(player.throwingState);
        }
    }
}
