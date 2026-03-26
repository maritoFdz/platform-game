using UnityEngine;

public class IdleState : IPlayerState
{
    private float coyoteCount;
    private const float targetVelocity = 0f;
    private const float gravityMultiplier = 0f;

    public void EnterState(Player player)
    {
        if (player.playerParameters.splashFallMinVelocity <= Mathf.Abs(player.velocity.y))
            player.MakeSplash(0f);
        player.PlayIdleAnimation();
        player.velocity.y = 0;
    }

    public void UpdateState(Player player)
    {
        if (player.inputX != 0)
            player.FlipSprite(player.inputX);
        if (player.velocity.x > 0.1f)
            player.PaintTrail();
        player.Move(true, false, gravityMultiplier);
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, targetVelocity, ref player.velocityXSmoothing, player.playerParameters.accelerationTimeGround);

        if (player.OnWater())
        {
            player.StopFallingAnimation();
            player.SwitchState(player.swimingState);
            return;
        }

        if (player.JumpPressed)
        {
            player.ConsumeJump();
            player.StopIdleAnimation();
            player.HandleJumpingStateTransition();
            return;
        }
        else if (player.inputX != 0)
        {
            player.StopIdleAnimation();
            if (player.IsRunning && player.playerParameters.canRun)
                player.SwitchState(player.runningState);
            else
                player.SwitchState(player.walkingState);
            return;
        }

        if (player.IsSliding())
        {
            player.SwitchState(player.slopeSlidingState);
            return;
        }

        if (player.GroundBelow() || player.OnSlope())
        {
            coyoteCount = player.playerParameters.coyoteTime;
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
