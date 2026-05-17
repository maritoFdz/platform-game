using UnityEngine;

public class WalkingState : IPlayerState
{
    private const float gravityMultiplier = 0f;
    private float coyoteCount;
    private float idleCount;

    public void EnterState(Player player)
    {
        AudioManager.instance.Play(AudioName.Movement);
        player.hasDashAir = false;
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
            AudioManager.instance.StopPlaying(AudioName.Movement);
            player.SwitchState(player.autoMoveState);
            return;
        }

        if (player.JumpPressed)
        {
            AudioManager.instance.StopPlaying(AudioName.Movement);
            player.ActivateDash();
            player.ConsumeJump();
            player.StopWalkingAnimation();
            player.HandleJumpingStateTransition();
            return;
        }
        else if (player.IsRunning && player.playerParameters.canRun)
        {
            player.StopWalkingAnimation();
            player.SwitchState(player.runningState);
            AudioManager.instance.StopPlaying(AudioName.Movement);
        }

        if (player.IsSliding())
        {
            AudioManager.instance.StopPlaying(AudioName.Movement);
            player.SwitchState(player.slopeSlidingState);
            player.StopWalkingAnimation();
            return;
        }

        if (player.IsDashing)
        {
            AudioManager.instance.StopPlaying(AudioName.Movement);
            player.StopWalkingAnimation();
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
                AudioManager.instance.StopPlaying(AudioName.Movement);
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
                AudioManager.instance.StopPlaying(AudioName.Movement);
                player.SwitchState(player.fallingState);
            }
        }

        if (player.IsPushing())
        {
            AudioManager.instance.StopPlaying(AudioName.Movement);
            player.SwitchState(player.pushingObjectState);
        }
    }
}
