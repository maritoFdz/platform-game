using UnityEngine;

public class WallSlidingState : IPlayerState
{
    private int direction;
    private float dirDecisionTimer;
    private float dropTimer;
    private const float gravityMultiplier = 0f;

    public void EnterState(Player player)
    {
        direction = player.WallLeft() ? -1 : 1;
        player.velocity = Vector2.zero; // cancel all movement
        player.velocityXSmoothing = 0f;
        player.velocityYSmoothing = 0f;
        dirDecisionTimer = player.wallStickTime;
        dropTimer = player.wallStickTime;
    }

    public void UpdateState(Player player)
    {
        player.velocity.y = Mathf.SmoothDamp(player.velocity.y, -player.wallSlideSpeed, ref player.velocityYSmoothing, player.accelerationTimeWall);
        player.Move(gravityMultiplier);
        if (player.JumpPressed || player.wallStickTime != dirDecisionTimer) // if is jump pressed or countdown to chose direction has started
        {
            if (dirDecisionTimer < 0 || player.inputX != 0f) ExecuteJump(player);
            else dirDecisionTimer -= Time.deltaTime;
        }
        else if (player.OnSlope() || player.GroundBelow())
        {
            player.SwitchState(player.walkingState);
        }
        else if (-player.inputX == direction || player.wallStickTime != dropTimer)
        {
            dropTimer -= Time.deltaTime;

            if (dropTimer <= 0f)
            {
                player.velocity.x = player.fallOfJump.x * -direction;
                player.velocity.y = player.fallOfJump.y;
                player.SwitchState(player.fallingState);
            }
        }
    }

    private void ExecuteJump(Player player)
    {
        if (direction == player.inputX)
        {
            player.velocity.x = player.climbJump.x * -direction;
            player.velocity.y = player.climbJump.y;
        }
        else if (direction == -player.inputX)
        {
            player.velocity.x = player.frontDirectionJump.x * -direction;
            player.velocity.y = player.frontDirectionJump.y;
        }
        else
        {
            player.velocity.x = player.fallOfJump.x * -direction;
            player.velocity.y = player.fallOfJump.y;
        }
        player.SwitchState(player.fallingState);
    }
}
