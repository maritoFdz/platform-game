using UnityEngine;

public class WalkingState : IPlayerState
{
    private const float gravityMultiplier = 0f;
    private float coyoteCount;
    private float idleCount;

    public void EnterState(Player player)
    {
        player.PlayWalkingAnimation();
        player.velocity.y = 0;
    }

    public void UpdateState(Player player)
    {
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity, ref player.velocityXSmoothing, player.accelerationTimeGround); // simulates aceleration
        player.Move(gravityMultiplier);
        if (player.JumpPressed)
        {
            player.ConsumeJump();
            player.SwitchState(player.jumpingState);
            player.StopWalkingAnimation();
            return;
        }
        else if (player.IsRunning)
        {
            player.StopWalkingAnimation();
            player.SwitchState(player.runningState);
        }

        if (player.IsMoving)
        {
            idleCount = player.timeToIdle;
        }
        else
        {
            idleCount -= Time.deltaTime;
            if (idleCount <= 0)
            {
                player.StopWalkingAnimation();
                player.SwitchState(player.idleState);
            }
        }

        if (player.GroundBelow() || player.OnSlope())
        {
            coyoteCount = player.coyoteTime;
        }
        else
        {
            coyoteCount -= Time.deltaTime;
            if (coyoteCount <= 0)
            {
                player.StopWalkingAnimation();
                player.SwitchState(player.fallingState);
            }
        }
    }
}
