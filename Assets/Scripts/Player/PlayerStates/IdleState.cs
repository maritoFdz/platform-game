using UnityEngine;

public class IdleState : IPlayerState
{
    private float coyoteCount;
    private const float targetVelocity = 0f;
    private const float gravityMultiplier = 0f;

    public void EnterState(Player player)
    {
        player.hasJumpAir = false;
        player.hasDashAir = false;
        player.PlayIdleAnimation();
        AudioManager.instance.StopPlaying(AudioName.Movement);
        player.velocity.y = 0;
        player.NotifyIdleChange();
    }

    public void UpdateState(Player player)
    {
        if (player.input.x != 0)
            player.FlipSprite(player.input.x);
        if (player.velocity.x > 0.1f)
            player.PaintTrail();
        player.Move(true, false, gravityMultiplier);
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, targetVelocity, ref player.velocityXSmoothing, player.playerParameters.accelerationTimeGround);

        if (player.IsSplitting)
        {
            player.StopIdleAnimation();
            SwitchState(player.splitingState, player);
            return;
        }

        if (player.pendingAutoMove)
        {
            SwitchState(player.autoMoveState, player);
            return;
        }

        if (player.OnWater())
        {
            player.StopFallingAnimation();
            SwitchState(player.swimingState, player);
            return;
        }

        if (player.IsDashing)
        {
            SwitchState(player.dashingState, player);
            return;
        }

        else if (player.JumpPressed)
        {
            player.ActivateDash();
            player.ConsumeJump();
            player.StopIdleAnimation();
            player.NotifyIdleChange();
            player.HandleJumpingStateTransition();
            return;
        }
        else if (player.input.x != 0)
        {
            player.StopIdleAnimation();
            if (player.IsRunning && player.playerParameters.canRun)
                SwitchState(player.runningState, player );
            else
                SwitchState(player.walkingState, player);
            return;
        }

        if (player.IsSliding())
        {
            SwitchState(player.slopeSlidingState, player);
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
                SwitchState(player.fallingState, player);
                player.StopIdleAnimation();
            }
        }
    }

    private void SwitchState(IPlayerState state, Player player)
    {
        player.NotifyIdleChange();
        player.SwitchState(state);
    }
}
