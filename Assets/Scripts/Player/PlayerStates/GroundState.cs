using UnityEngine;

public class GroundState : IPlayerState
{
    private const float gravityMultiplier = 0f;
    private float coyoteCount;

    public void EnterState(Player player)
    {
        player.velocity.y = 0;
    }

    public void UpdateState(Player player)
    {
        coyoteCount -= Time.deltaTime;
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity, ref player.velocityXSmoothing, player.accelerationTimeGround); // simulates aceleration
        player.Move(gravityMultiplier);
        if (player.JumpPressed)
        {
            player.ConsumeJump();
            player.SwitchState(player.jumpingState);
            return;
        }
        if (player.GroundDetected() || player.OnSlope())
        {
            coyoteCount = player.coyoteTime;
        }
        else
        {
            coyoteCount -= Time.deltaTime;
            if (coyoteCount <= 0)
                player.SwitchState(player.fallingState);
        }
    }
}
