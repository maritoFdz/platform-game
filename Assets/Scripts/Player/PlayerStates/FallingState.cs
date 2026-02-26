using UnityEngine;

public class FallingState : IPlayerState
{
    public void EnterState(Player player)
    {
    }

    public void UpdateState(Player player)
    {
        player.AddGravityForce(player.gravityFallMultiplier);
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity, ref player.velocityXSmoothing, player.accelerationTimeAir);
        player.Move();
        if (player.GroundDetected())
            player.SwitchState(player.groundState);
    }
}
