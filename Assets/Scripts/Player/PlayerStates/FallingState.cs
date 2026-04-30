using UnityEngine;

public class FallingState : IPlayerState
{
    private bool freezeBehaviour;
    public void EnterState(Player player)
    {
        freezeBehaviour = false;
        if (player.CeilingAbove())
        {
            player.velocity.y = player.gravityScale * 0.1f; // arbitrary low value to avoid making a smooth transition when player should just fall
            player.MakeSplash(180f);
        }
        player.PlayFallingAnimation();
    }

    public void UpdateState(Player player)
    {
        if (player.input.x != 0) player.FlipSprite(player.input.x);
        if (freezeBehaviour) return;
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity, ref player.velocityXSmoothing, player.playerParameters.accelerationTimeAir);
        if (player.OnWater())
        {
            player.StopFallingAnimation();
            player.SwitchState(player.swimingState);
            return;
        }

        player.Move(false, false, player.playerParameters.gravityFallMultiplier);
        if (player.IsDashing && !player.hasDashAir)
        {
            player.hasDashAir = true;
            player.SwitchState(player.dashingState);
            return;
        }
        if ((player.WallLeft() || player.WallRight()) && !player.IsFrozen)
        {
            float dir = player.WallLeft() ? -1 : 1;
            player.FlipSprite(dir);
            if (player.playerParameters.splashWallMinVelocity <= Mathf.Abs(player.velocity.x))
                player.MakeSplash(90f * dir);
            player.StopFallingAnimation();
            player.HandleWallSlidingStateTransition();
            freezeBehaviour = true;
        }
        else if (player.GroundBelow())
        {
            player.StopFallingAnimation();
            player.ActivateDash();
            player.SwitchState(player.idleState);
        }    
    }
}
