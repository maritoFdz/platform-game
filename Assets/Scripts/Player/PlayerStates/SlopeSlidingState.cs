using UnityEngine;

public class SlopeSlidingState : IPlayerState
{
    private int direction;
    public void EnterState(Player player)
    {
        direction = player.WallLeft() ? -1 : 1;
    }

    public void UpdateState(Player player)
    {
        player.Move(true, false);
        if (player.JumpPressed)
        {
            player.velocity.x = player.slopeSlidingJump.x * -direction;
            player.velocity.y = player.slopeSlidingJump.y;
            player.SwitchState(player.fallingState);
        }
        else if (!player.IsSliding())
        {
            player.velocity.x = player.endSlopeBoostX * -direction;
            player.SwitchState(player.idleState);
        }
    }
}
