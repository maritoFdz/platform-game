using UnityEngine;

public class SwimingState : IPlayerState
{
    private float swimTimer;
    private float upscaleTimer;
    private SlimeSupply currentWater;

    public void EnterState(Player player)
    {
        currentWater = null;
        currentWater = player.GetCurrentwater();
        if (currentWater == null)
            player.SwitchState(player.idleState);
        player.MakeSplash(0f, true);
        if (player.playerParameters.splashFallMinVelocity <= Mathf.Abs(player.velocity.y))
            AudioManager.instance.Play(AudioName.SplashLoud);
        else AudioManager.instance.Play(AudioName.SplashWeak);
        player.PlayIdleAnimation();
    }

    public void UpdateState(Player player)
    {
        if (currentWater == null)
        {
            player.SwitchState(player.idleState);
            return;
        }

        upscaleTimer += Time.deltaTime;
        if (upscaleTimer >= 0.1f)
        {
            upscaleTimer = 0f;
            if (!player.IsFull)
                if (!currentWater.TryConsume(player.playerParameters.scaleReductionPerUnit * player.playerParameters.upscalePerUnit))
                {
                    player.ClearCurrentWater();
                    player.SwitchState(player.idleState);
                    return;
                }

            player.Upscale();
        }

        if (player.input.x != 0)
            player.FlipSprite(player.input.x);

        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity, ref player.velocityXSmoothing, player.playerParameters.accelerationTimeGround);
        swimTimer += Time.deltaTime;
        float wave = Mathf.Sin(swimTimer * 2f) * 5f;
        float surface = currentWater.GetSurfaceHeight();
        float difference = surface - player.transform.position.y;
        player.velocity.y += difference * player.playerParameters.floatingForce; // makes floating smooth
        player.velocity.y -= player.velocity.y * player.playerParameters.damping * Time.deltaTime;
        player.velocity.y += wave * Time.deltaTime;

        player.Move(false, false, 0f);

        if (player.JumpPressed)
        {
            player.StopIdleAnimation();
            player.MakeSplash(0f);
            player.ForceJumpingAnimation();
            AudioManager.instance.Play(AudioName.SplashOut);
            player.SwitchState(player.jumpingState);
            player.ClearCurrentWater();
        }
    }
}