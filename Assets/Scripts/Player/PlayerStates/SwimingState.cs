using UnityEngine;

public class SwimingState : IPlayerState
{
    private float swimTimer;
    private float upscaleTimer;
    private SlimeSupply currentWater;

    public void EnterState(Player player)
    {
        currentWater = null;
        player.MakeSplash(0f);
        player.PlayIdleAnimation();
        currentWater = player.GetCurrentwater();
        if (currentWater == null)
            player.SwitchState(player.idleState);
    }

    public void UpdateState(Player player)
    {
        upscaleTimer += Time.deltaTime;
        if (upscaleTimer >= 0.1f)
        {
            upscaleTimer = 0f;
            if (!currentWater.TryConsume(player.playerParameters.scaleReductionPerUnit * player.playerParameters.upscalePerUnit))
            {
                player.ClearCurrentWater();
                player.SwitchState(player.idleState);
                return;
            }

            player.Upscale();
        }

        if (player.inputX != 0)
            player.FlipSprite(player.inputX);

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
            player.SwitchState(player.jumpingState);
            player.ClearCurrentWater();
        }
    }
}