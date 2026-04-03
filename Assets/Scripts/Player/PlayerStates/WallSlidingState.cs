using UnityEngine;

public class WallSlidingState : IPlayerState
{
    private int direction;
    private float dirDecisionTimer;
    private float dropTimer;
    private const float gravityMultiplier = 0f;
    private const float wallEndTimer = 0.1f;
    private bool freezeBehaviour;
    private float wallAttachTimer;

    public void EnterState(Player player)
    {
        freezeBehaviour = false;
        direction = player.WallLeft() ? -1 : 1;
        if (player.playerParameters.splashWallMinVelocity <= Mathf.Abs(player.velocity.x))
            player.MakeSplash(90f * direction);
        player.velocity = Vector2.zero; // cancel all movement
        player.velocityXSmoothing = 0f;
        player.velocityYSmoothing = 0f;
        dirDecisionTimer = player.playerParameters.wallStickTime;
        dropTimer = player.playerParameters.wallStickTime;
        wallAttachTimer = wallEndTimer;
        player.FlipSprite(direction);
    }

    public void UpdateState(Player player)
    {
        if (freezeBehaviour) return;
        player.velocity.y = Mathf.SmoothDamp(player.velocity.y, -player.playerParameters.wallSlideSpeed, ref player.velocityYSmoothing, player.playerParameters.accelerationTimeWall);
        player.velocity.x = direction * 0.1f;
        player.Move(false, true, gravityMultiplier);
        player.PaintTrail();
        bool onWall = direction == -1 ? player.WallLeft() : player.WallRight();

        if (!onWall)
        {
            wallAttachTimer -= Time.deltaTime;
            if (wallAttachTimer <= 0f)
            {
                player.SwitchState(player.fallingState);
                return;
            }
        }
        else
            wallAttachTimer = wallEndTimer;

        if (player.JumpPressed || player.playerParameters.wallStickTime != dirDecisionTimer) // if is jump pressed or countdown to chose direction has started
        {
            if (dirDecisionTimer < 0 || player.input.x != 0f) ExecuteJump(player);
            else dirDecisionTimer -= Time.deltaTime;
        }
        else if (player.OnSlope() || player.GroundBelow())
        {
            player.SwitchState(player.walkingState);
        }
        else if (-player.input.x == direction || player.playerParameters.wallStickTime != dropTimer)
        {
            dropTimer -= Time.deltaTime;

            if (dropTimer <= 0f)
            {
                player.velocity.x = player.playerParameters.fallOfJump.x * -direction;
                player.velocity.y = player.playerParameters.fallOfJump.y;
                player.SwitchState(player.fallingState);
            }
        }
    }

    private void ExecuteJump(Player player)
    {
        if (direction == player.input.x)
        {
            player.velocity.x = player.playerParameters.climbJump.x * -direction;
            player.velocity.y = player.playerParameters.climbJump.y;
        }
        else if (direction == -player.input.x)
        {
            player.velocity.x = player.playerParameters.frontDirectionJump.x * -direction;
            player.velocity.y = player.playerParameters.frontDirectionJump.y;
        }
        else
        {
            player.velocity.x = player.playerParameters.fallOfJump.x * -direction;
            player.velocity.y = player.playerParameters.fallOfJump.y;
        }
        player.MakeSplash(90f * direction);
        player.HandleJumpingStateTransition();
        freezeBehaviour = true;
    }
}
