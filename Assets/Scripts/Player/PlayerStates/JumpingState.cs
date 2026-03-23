using UnityEngine;
using UnityEngine.Windows;

public class JumpingState : IPlayerState
{
    private const float gravityMultiplier = 1f;
    private const int amountOfScaleUnits = 2;
    private bool freezeBehaviour;

    public void EnterState(Player player)
    {
        freezeBehaviour = false;
        player.MakeSplash(0f);
        player.velocity.y = player.jumpForce;
    }

    public void UpdateState(Player player)
    {
        if (freezeBehaviour) return;
        if (player.inputX != 0)
            player.FlipSprite(player.inputX);
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity, ref player.velocityXSmoothing, player.playerParameters.accelerationTimeAir);
        player.Move(false, false, gravityMultiplier);

        if (player.velocity.y <= 0 || player.CeilingAbove())
        {
            player.StopJumpingAnimation();
            player.SwitchState(player.fallingState);
        }
        else if ((player.WallLeft() && player.inputX == -1 || player.WallRight() && player.inputX == 1) && !player.OnSlope())
        {
            if (!player.HasSlopeNear((int)Mathf.Sign(player.inputX), 30))
            {
                if (player.WallLeft()) player.FlipSprite(-1);
                else player.FlipSprite(1);
                player.HandleWallSlidingStateTransition();
                freezeBehaviour = true;
            }
        }
        else if (player.JumpReleased)
        {
            if (player.velocity.y > player.minJumpForce)
                player.velocity.y = player.minJumpForce;
        }
    }
}
