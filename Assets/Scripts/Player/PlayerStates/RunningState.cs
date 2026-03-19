using UnityEngine;

public class RunningState : IPlayerState
{
    private const float gravityMultiplier = 0f;
    private float targetVelocity;
    private float coyoteCount;

    public void EnterState(Player player)
    {
        player.PlayRunningAnimation();
    }

    public void UpdateState(Player player)
    {
        targetVelocity = player.targetVelocity * player.runningVelocityMultiplier;
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, targetVelocity, ref player.velocityXSmoothing, player.accelerationTimeGround);
        player.Move(true, false, gravityMultiplier);
        player.PaintTrail();
        if (player.JumpPressed)
        {
            player.ConsumeJump();
            player.StopRunningAnimation();
            player.HandleJumpingStateTransition();
            return;
        }

        if (!player.IsRunning && !player.IsMoving)
        {
            player.StopRunningAnimation();
            player.SwitchState(player.idleState);
            return;
        }
        else if (!player.IsRunning)
        {
            player.StopRunningAnimation();
            player.SwitchState(player.walkingState);
            return;
        }

        if (player.IsSliding())
        {
            player.StopRunningAnimation();
            player.SwitchState(player.slopeSlidingState);
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
                player.StopRunningAnimation();
                player.SwitchState(player.fallingState);
            }
        }

        if (player.IsPushing())
        {
            player.StopRunningAnimation();
            player.SwitchState(player.pushingObjectState);
        }
    }
}
