using UnityEngine;
using UnityEngine.Rendering;

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
        if (player.inputX != 0)
            player.FlipSprite(player.inputX);
        if (freezeBehaviour) return;
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity, ref player.velocityXSmoothing, player.playerParameters.accelerationTimeAir);
        if (player.OnWater())
        {
            player.StopFallingAnimation();
            player.SwitchState(player.swimingState);
            return;
        }
        player.Move(false, false, player.playerParameters.gravityFallMultiplier);
        if (player.IsDashing)
        {
            player.SwitchState(player.dashingState);
            return;
        }
        if (player.WallLeft() || player.WallRight())
        {
            if (!player.HasSlopeNear((int)Mathf.Sign(player.inputX), 30))
            {
                if (player.WallLeft()) player.FlipSprite(-1);
                else player.FlipSprite(1);
                player.StopFallingAnimation();
                player.HandleWallSlidingStateTransition();
                freezeBehaviour = true;
            }
        }
        else if (player.GroundBelow())
        {
            player.StopFallingAnimation();
            player.SwitchState(player.idleState);
        }    
    }
}
