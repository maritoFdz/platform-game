using UnityEngine;

public class SwimingState : IPlayerState
{
    private float swimTimer;
    private float upscaleTimer;

    public void EnterState(Player player)
    {
        swimTimer = 0f;
        player.MakeSplash(0f);
        player.PlayIdleAnimation();
        player.velocity.y = -1f;
        player.velocityYSmoothing = 0f;
    }

    public void UpdateState(Player player)
    {
        if (player.inputX != 0)
            player.FlipSprite(player.inputX);
        upscaleTimer += Time.deltaTime;
        if (upscaleTimer >= 0.1f)
        {
            player.Upscale();
            upscaleTimer = 0f;
        }
        swimTimer += Time.deltaTime;
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity, ref player.velocityXSmoothing, player.playerParameters.accelerationTimeGround);
        float swimForce = Mathf.Sin(2 * swimTimer) * 0.2f;
        player.velocity.y = swimForce;
        player.Move(false, false, 0f);
        if (player.JumpPressed)
        {
            player.StopIdleAnimation();
            player.MakeSplash(0f);
            player.ForceJumpingAnimation();
            player.SwitchState(player.jumpingState);
        }
    }
}
