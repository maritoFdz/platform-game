using UnityEngine;

public class ThrowingState : IPlayerState
{
    private bool freezeBehaviour;
    private float direction;
    public void EnterState(Player player)
    {
        direction = -player.GetFacingDir();
        // todo anim
        freezeBehaviour = false;
        player.velocity = new Vector2 (player.playerParameters.throwInitialVelocity.x * direction, player.playerParameters.throwInitialVelocity.y);
        player.PlayFallingAnimation();
    }

    public void UpdateState(Player player)
    {
        if (player.input.x != 0) player.FlipSprite(player.input.x);
        if (freezeBehaviour) return;
        if (Mathf.Sign(player.input.x) == Mathf.Sign(direction))
            player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity * player.playerParameters.throwSpeedFrontMultiplier, ref player.velocityXSmoothing, player.playerParameters.accelerationTimeThrow);
        else
            player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity * player.playerParameters.throwSpeedBackMultiplier, ref player.velocityXSmoothing, player.playerParameters.accelerationTimeThrow);
        if (player.OnWater())
        {
            player.StopFallingAnimation();
            player.SwitchState(player.swimingState);
            return;
        }

        player.Move(false, false);

        if (player.CeilingAbove())
        {
            player.SwitchState(player.fallingState);
            return;
        }

        if (player.JumpPressed && player.CanDoubleJump)
        {
            Debug.Log("Entrar entrar tecnicamente si entre");
            player.hasJumpAir = true;
            player.HandleJumpingStateTransition();
            player.StopFallingAnimation();
            return;
        }

        if (player.IsDashing && !player.hasDashAir)
        {
            player.hasDashAir = true;
            player.SwitchState(player.dashingState);
            return;
        }

        if ((player.WallLeft() || player.WallRight()) && !player.IsFrozen)
        {
            Debug.Log("si choque jejeje");
            float dir = player.WallLeft() ? -1 : 1;
            player.FlipSprite(dir);
            if (player.playerParameters.splashWallMinVelocity <= Mathf.Abs(player.velocity.x))
                player.MakeSplash(90f * dir);
            else AudioManager.instance.Play(AudioName.FallWeak);
            player.StopFallingAnimation();
            player.HandleWallSlidingStateTransition();
            freezeBehaviour = true;
        }
        else if (player.GroundBelow())
        {
            player.velocityXSmoothing *= 0.1f;
            player.velocity.x *= 0.1f;
            player.StopFallingAnimation();
            player.ActivateDash();
            player.SwitchState(player.idleState);
        }
    }
}