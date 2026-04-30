using UnityEngine;

public class JumpingState : IPlayerState
{
    private float gravityMultiplier;
    private bool freezeBehaviour;
    private bool isHanging;
    private float hangCounter;

    public void EnterState(Player player)
    {
        gravityMultiplier = 1f;
        isHanging = false;
        hangCounter = 0f;
        freezeBehaviour = false;
        player.MakeSplash(0f);
        player.velocity.y = player.jumpForce;
    }

    public void UpdateState(Player player)
    {
        if (freezeBehaviour) return;
        if (player.input.x != 0) player.FlipSprite(player.input.x);

        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity, ref player.velocityXSmoothing, player.playerParameters.accelerationTimeAir);
        player.Move(false, false, gravityMultiplier);

        if (player.IsDashing && !player.hasDashAir)
        {
            player.hasDashAir = true;
            player.SwitchState(player.dashingState);
            return;
        }

        if (player.velocity.y <= 0 || player.CeilingAbove())
        {
            player.velocity.y = 0f;

            if (player.CeilingAbove())
            {
                player.SwitchState(player.fallingState);
                return;
            }
            
            if (!isHanging)
            {
                gravityMultiplier = 0f;
                isHanging = true;
            }

            hangCounter += Time.deltaTime;

            if (hangCounter >= player.playerParameters.hangTime)
            {
                player.StopJumpingAnimation();
                player.SwitchState(player.fallingState);
            }
        }

        if ((player.WallLeft() && player.input.x == -1 || player.WallRight() && player.input.x == 1) && !player.IsFrozen)
        {
            float dir = player.WallLeft() ? -1 : 1;
            player.FlipSprite(dir);
            if (player.playerParameters.splashWallMinVelocity <= Mathf.Abs(player.velocity.x))
                player.MakeSplash(90f * dir);
            player.HandleWallSlidingStateTransition();
            freezeBehaviour = true;
        }
        else if (player.JumpReleased)
        {
            if (player.velocity.y > player.minJumpForce)
                player.velocity.y = player.minJumpForce;
        }
    }
}
