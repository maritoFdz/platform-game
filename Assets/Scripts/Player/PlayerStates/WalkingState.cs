using UnityEngine;

public class WalkingState : IPlayerState
{
    private const float gravityMultiplier = 0f;
    private float coyoteCount;
    private float idleCount;

    public void EnterState(Player player)
    {
        player.PlayWalkingAnimation();
        player.velocity.y = 0;
    }

    public void UpdateState(Player player)
    {
        if (player.input.x != 0)
            player.FlipSprite(player.input.x);
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity, ref player.velocityXSmoothing, player.playerParameters.accelerationTimeGround); // simulates acceleration
        player.Move(true, false, gravityMultiplier);

        if (player.pendingAutoMove)
        {
            player.SwitchState(player.autoMoveState);
            return;
        }

        if (player.JumpPressed)
        {
            player.ConsumeJump();
            player.StopWalkingAnimation();
            player.HandleJumpingStateTransition();
            return;
        }
        else if (player.IsRunning && player.playerParameters.canRun)
        {
            player.StopWalkingAnimation();
            player.SwitchState(player.runningState);
        }

        if (player.IsSliding())
        {
            player.SwitchState(player.slopeSlidingState);
            return;
        }

        if (player.IsDashing)
        {
            player.SwitchState(player.dashingState);
            return;
        }

        if (player.IsMoving)
        {
            idleCount = player.playerParameters.timeToIdle;
            player.PaintTrail();
        }
        else
        {
            idleCount -= Time.deltaTime;
            if (idleCount <= 0)
            {
                player.StopWalkingAnimation();
                player.SwitchState(player.idleState);
            }
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
                player.StopWalkingAnimation();
                player.SwitchState(player.fallingState);
            }
        }

        if (player.IsPushing())
        {
            player.SwitchState(player.pushingObjectState);
        }
    }
}
