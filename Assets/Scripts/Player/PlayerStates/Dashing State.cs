using UnityEngine;
using UnityEngine.Windows;

public class DashingState : IPlayerState
{
    private float dashTimer;
    private bool freezeBehaviour;
    private const float diagonalValue = 0.7071f;

    public void EnterState(Player player)
    {
        freezeBehaviour = false;
        player.ConsumeDash();
        dashTimer = 0f;
        player.velocity = Vector2.zero;
        player.velocityXSmoothing = player.velocityYSmoothing = 0f;
        float dashSpeed = player.playerParameters.dashDistance / player.playerParameters.dashTime;
        if (player.input == Vector2.zero || (player.input.x == 0 && !player.playerParameters.canDashVertical && !player.playerParameters.canDashDiagonal))
            player.input = new Vector2(-player.GetFacingDir(), 0);
        else if (!player.playerParameters.canDashVertical && !player.playerParameters.canDashDiagonal)
            player.input.y = 0f;
        else if (player.playerParameters.canDashVertical && !player.playerParameters.canDashDiagonal)
        {
            // vertical movement over horizontal
            if (player.input.y != 0f) player.input = new Vector2(0f, Mathf.Sign(player.input.x));
        }
        else
        {
            if (player.input.y != 0f && player.input.x != 0f)
                player.input = new Vector2(Mathf.Sign(player.input.x) * diagonalValue, Mathf.Sign(player.input.y) * diagonalValue);
            else if (player.input.y != 0f)
                player.input = new Vector2(0f, Mathf.Sign(player.input.y));
            else
                player.input = new Vector2(Mathf.Sign(player.input.x), 0f);
        }
        player.velocity = player.input * dashSpeed;
    }

    public void UpdateState(Player player)
    {
        if (freezeBehaviour) return;
        float dt = Time.deltaTime;
        dashTimer += dt;
        player.Move(true, false, 0f);
        if (player.GroundBelow())
        {
            if (player.velocity.y != 0f)
            {
                player.velocity = Vector2.zero;
                player.MakeSplash(0f);
                player.StopFallingAnimation();
                player.SwitchState(player.idleState);
            }    
            else player.PaintTrail();
        }

        if ((player.WallLeft() || player.WallRight()) && !player.OnSlope())
            if (!player.HasSlopeNear((int)Mathf.Sign(player.input.x), 30))
            {
                player.velocity.x = 0f;
                player.StopFallingAnimation();
                player.HandleWallSlidingStateTransition();
                freezeBehaviour = true;
            }

        if (dashTimer >= player.playerParameters.dashTime)
        {
            player.velocity = Vector2.zero;
            player.SwitchState(player.idleState);
        }
    }
}
