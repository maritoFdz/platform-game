using UnityEngine;

public class IdleState : IPlayerState
{
    private float coyoteCount;
    private const float targetVelocity = 0f;
    private const float gravityMultiplier = 0f;

    public void EnterState(Player player)
    {
        player.hasDashAir = false;
        if (player.playerParameters.splashFallMinVelocity <= Mathf.Abs(player.velocity.y))
            player.MakeSplash(0f);
        else if (player.velocity.y != 0 && AudioManager.instance != null) AudioManager.instance.Play(AudioName.FallWeak);
        player.PlayIdleAnimation();
        AudioManager.instance.StopPlaying(AudioName.Movement);
        player.velocity.y = 0;
    }

    public void UpdateState(Player player)
    {
        if (player.input.x != 0)
            player.FlipSprite(player.input.x);
        if (player.velocity.x > 0.1f)
            player.PaintTrail();
        player.Move(true, false, gravityMultiplier);
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, targetVelocity, ref player.velocityXSmoothing, player.playerParameters.accelerationTimeGround);

        if (player.pendingAutoMove)
        {
            player.SwitchState(player.autoMoveState);
            return;
        }

        if (player.OnWater())
        {
            player.StopFallingAnimation();
            player.SwitchState(player.swimingState);
            return;
        }

        if (player.IsDashing)
        {
            player.SwitchState(player.dashingState);
            return;
        }

        else if (player.JumpPressed)
        {
            player.ActivateDash();
            player.ConsumeJump();
            player.StopIdleAnimation();
            player.HandleJumpingStateTransition();
            return;
        }
        else if (player.input.x != 0)
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
                player.ActivateDash();
                player.SwitchState(player.fallingState);
                player.StopIdleAnimation();
            }
        }
    }
}
