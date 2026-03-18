using UnityEngine;

public class PushingObjectState : IPlayerState
{
    private int pushDirection;
    public void EnterState(Player player)
    {
        pushDirection = player.velocity.x >= 0 ? 1 : -1;
        player.velocity.x = 0f;
        player.velocityXSmoothing = 0f;
    }

    public void UpdateState(Player player)
    {
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity * 0.5f, ref player.velocityXSmoothing, player.accelerationTimeGround);
        // push
        if (Mathf.Sign(player.velocity.x) != pushDirection || !player.IsPushing())
        {
            player.SwitchState(player.walkingState);
        }
        else if (player.JumpPressed)
            player.SwitchState(player.jumpingState);
    }
}
