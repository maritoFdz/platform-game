using UnityEngine;

public class GroundState : IPlayerState
{
    private const float gravityMultiplier = 0f;

    public void EnterState(Player player)
    {
        player.velocity.y = 0;
    }

    public void UpdateState(Player player)
    {
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity, ref player.velocityXSmoothing, player.accelerationTimeGround); // simulates aceleration
        player.Move(gravityMultiplier);
        if (player.JumpPressed)
        {
            player.ConsumeJump();
            player.SwitchState(player.jumpingState);
        }
        else if (!player.GroundDetected() && !player.OnSlope())
            player.SwitchState(player.fallingState);
    }
}
