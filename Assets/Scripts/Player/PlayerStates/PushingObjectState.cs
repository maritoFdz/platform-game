using UnityEngine;

public class PushingObjectState : IPlayerState
{
    private int pushDirection;
    private PushableObject target;
    public void EnterState(Player player)
    {
        pushDirection = player.velocity.x >= 0 ? 1 : -1;
        player.velocity.x = 0f;
        player.velocityXSmoothing = 0f;
        target = player.GetPushableObject(pushDirection);
    }

    public void UpdateState(Player player)
    {
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity * 0.5f, ref player.velocityXSmoothing, player.accelerationTimeGround);
        if (Mathf.Sign(player.inputX) != pushDirection || !player.IsPushing())
        {
            target.SetDirection(0);
            target = null;
            player.SwitchState(player.walkingState);
        }
        else if (player.JumpPressed)
        {
            target.SetDirection(0);
            target = null;
            player.SwitchState(player.jumpingState);
        }
        else
        {
            target.SetDirection(player.inputX);
            player.Move(true, false, 0f);
        }
    }
}
