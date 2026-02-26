using UnityEngine;

public class JumpingState : IPlayerState
{
    public void EnterState(Player player)
    {
        player.velocity.y = player.jumpForce;
    }

    public void UpdateState(Player player)
    {
        player.AddGravityForce();
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity, ref player.velocityXSmoothing, player.accelerationTimeAir);
        player.Move();
        if (player.velocity.y <= 0)
            player.SwitchState(player.fallingState);
    }
}
