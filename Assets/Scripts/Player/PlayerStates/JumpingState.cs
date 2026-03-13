using UnityEngine;

public class JumpingState : IPlayerState
{
    private const float gravityMultiplier = 1f;

    public void EnterState(Player player)
    {
        player.MakeSplash(0f);
        player.velocity.y = player.jumpForce;
    }

    public void UpdateState(Player player)
    {
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity, ref player.velocityXSmoothing, player.accelerationTimeAir);
        player.Move(gravityMultiplier);
        if (player.velocity.y <= 0 || player.CeilingAbove())
        {
            player.StopJumpingAnimation();
            player.SwitchState(player.fallingState);
        }
        else if ((player.WallLeft() && player.inputX == -1 || player.WallRight() && player.inputX == 1) && !player.OnSlope() && !player.HasSlopeNear())
        {
            player.StopJumpingAnimation();
            player.SwitchState(player.wallSlidingState);
        }
        else if (player.JumpReleased)
        {
            if (player.velocity.y > player.minJumpForce)
                player.velocity.y = player.minJumpForce;
        }
    }
}
