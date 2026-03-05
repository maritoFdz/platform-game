using UnityEngine;

public class FallingState : IPlayerState
{
    public void EnterState(Player player)
    {
        if (player.CeilingAbove())
            player.velocity.y = player.gravityScale * 0.1f; // arbitrary low value to avoid making a smooth transition when player should just fall
    }

    public void UpdateState(Player player)
    {
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity, ref player.velocityXSmoothing, player.accelerationTimeAir);
        player.Move(player.gravityFallMultiplier);
        if (player.GroundBelow())
            player.SwitchState(player.idleState);
        else if (player.WallLeft() || player.WallRight())
        {
            player.SwitchState(player.wallSlidingState);
        }
    }
}
