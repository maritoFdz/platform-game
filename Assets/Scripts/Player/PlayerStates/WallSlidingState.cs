using UnityEngine;

public class WallSlidingState : IPlayerState
{
    private const float gravityMultiplier = 0f;
    public void EnterState(Player player)
    {
        player.velocity.y = 0;
    }

    public void UpdateState(Player player)
    {
        player.velocity.y = Mathf.SmoothDamp(player.velocity.y, - player.wallSlideSpeed, ref player.velocityYSmoothing, player.accelerationTimeWall);
        player.Move(gravityMultiplier);
        if (player.OnSlope() || player.GroundDetected())
        {
            player.SwitchState(player.groundState);
        }
    }
}
