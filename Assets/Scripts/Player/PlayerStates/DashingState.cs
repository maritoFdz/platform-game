using UnityEngine;

public class DashingState : IPlayerState
{
    private float dashCounter;
    private bool freezeBehaviour;
    private Vector2 initialVelocity;
    private const float diagonalValue = 0.7071f;

    public void EnterState(Player player)
    {
        player.hasDashAir = true;
        freezeBehaviour = false;
        player.ConsumeDash();
        dashCounter = 0f;
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
        initialVelocity = player.velocity;
    }

    public void UpdateState(Player player)
    {
        if (freezeBehaviour) return;
        dashCounter += Time.deltaTime;

        player.Move(true, false, 0f);
        if (player.GroundBelow())
        {
            if (initialVelocity.y != 0f)
            {
                player.ActivateDash();
                player.velocity = Vector2.zero;
                player.MakeSplash(0f);
                player.StopFallingAnimation();
                player.SwitchState(player.idleState);
                return;
            }
            else player.PaintTrail();
        }

        if (player.CeilingAbove())
        {
            player.SwitchState(player.fallingState);
            return;
        }

        if (player.CollisionLeft() && initialVelocity.x < 0 || player.CollisionRight() && initialVelocity.x > 0)
        {
            player.SwitchState(player.idleState);
            return;
        }

        if (player.WallLeft() || player.WallRight())
        {
            player.velocity.x = 0f;
            player.StopFallingAnimation();
            player.HandleWallSlidingStateTransition();
            freezeBehaviour = true;
            return;
        }

        if (dashCounter >= player.playerParameters.dashTime)
        {
            bool fallingDash = initialVelocity.y < 0;
            bool grounded = player.GroundBelow();

            float extraGroundTime = grounded ? player.playerParameters.decelerationTimeDash * 0.7f : 0f;

            if (dashCounter < player.playerParameters.dashTime + extraGroundTime)
                return;

            float decelTimeX = grounded ? player.playerParameters.decelerationTimeDash * 0.25f : player.playerParameters.decelerationTimeDash;
            float targetY = fallingDash ? player.playerParameters.maxFallSpeed : 0f;

            player.velocity.x = Mathf.SmoothDamp(player.velocity.x, 0f, ref player.velocityXSmoothing, decelTimeX);

            player.velocity.y = Mathf.SmoothDamp(player.velocity.y, targetY, ref player.velocityYSmoothing, player.playerParameters.decelerationTimeDash);

            if (fallingDash)
                player.SwitchState(player.fallingState);

            if (Mathf.Abs(player.velocity.x) <= 0.25f && Mathf.Abs(player.velocity.y) <= 0.25f)
            {
                player.velocity = Vector2.zero;

                if (grounded)
                {
                    if (fallingDash)
                        player.ActivateDash();

                    if (player.input.x != 0f)
                        player.SwitchState(player.walkingState);
                    else
                        player.SwitchState(player.idleState);
                }
                else
                {
                    player.SwitchState(player.fallingState);
                }
            }
        }
    }
}
