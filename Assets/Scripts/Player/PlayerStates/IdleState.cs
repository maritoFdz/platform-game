using UnityEngine;

public class IdleState : IPlayerState
{
    private float coyoteCount;
    private const float targetVelocity = 0f;
    private const float gravityMultiplier = 0f;

    public void EnterState(Player player)
    {
        player.PlayIdleAnimation();
        player.velocity.y = 0;
    }

    public void UpdateState(Player player)
    {
        player.Move(gravityMultiplier);
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, targetVelocity, ref player.velocityXSmoothing, player.accelerationTimeGround);
        if (player.JumpPressed)
        {
            player.ConsumeJump();
            player.StopIdleAnimation();
            player.SwitchState(player.jumpingState);
            return;
        }
        else if (player.inputX != 0)
        {
            player.StopIdleAnimation();
            if (player.IsRunning)
                player.SwitchState(player.runningState);
            else
                player.SwitchState(player.walkingState);
            return;
        }

        if (player.GroundBelow() || player.OnSlope())
        {
            coyoteCount = player.coyoteTime;
        }
        else
        {
            coyoteCount -= Time.deltaTime;
            if (coyoteCount <= 0)
            {
                player.SwitchState(player.fallingState);
                player.StopIdleAnimation();
            }
        }
    }
}
