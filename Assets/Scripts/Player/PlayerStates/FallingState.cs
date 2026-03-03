using UnityEngine;

public class FallingState : IPlayerState
{
    public void EnterState(Player player)
    {
        player.velocity.y = player.gravityScale * 0.1f;
    }

    public void UpdateState(Player player)
    {
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity, ref player.velocityXSmoothing, player.accelerationTimeAir);
        player.Move(player.gravityFallMultiplier);
        if (player.GroundBelow())
            player.SwitchState(player.groundState);
        else if (player.WallLeft() || player.WallRight())
        {
            player.SwitchState(player.wallSlidingState);
        }
    }
}
