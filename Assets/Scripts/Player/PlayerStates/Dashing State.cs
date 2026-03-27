using UnityEngine;

public class DashingState : IPlayerState
{
    private float dashVelocity;
    private float dashTimer;
    private bool freezeBehaviour;

    public void EnterState(Player player)
    {
        freezeBehaviour = false;
        player.ConsumeDash();
        float direction = player.GetFacingDir();
        player.velocity = Vector2.zero;
        player.velocityXSmoothing = player.velocityYSmoothing = 0f;
        dashVelocity = player.playerParameters.dashDistance / player.playerParameters.dashTime * -direction;
        player.velocity.x = dashVelocity;
        dashTimer = 0f;
    }

    public void UpdateState(Player player)
    {
        if (freezeBehaviour) return;
        float dt = Time.deltaTime;
        dashTimer += dt;
        player.Move(true, false, 0f);
        if (player.GroundBelow()) player.PaintTrail();

        if ((player.WallLeft() || player.WallRight()) && !player.OnSlope())
            if (!player.HasSlopeNear((int)Mathf.Sign(player.inputX), 30))
            {
                player.velocity.x = 0f;
                player.HandleWallSlidingStateTransition();
                freezeBehaviour = true;
            }

        if (dashTimer >= player.playerParameters.dashTime)
        {
            player.velocity.x = 0f;
            player.SwitchState(player.idleState);
        }
    }
}
