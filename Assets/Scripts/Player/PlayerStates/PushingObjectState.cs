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
        if (target)
            target.SetAsTargetOf(player);
        else
            player.SwitchState(player.idleState);
    }

    public void UpdateState(Player player)
    {
        if (Mathf.Sign(player.inputX) != pushDirection)
        {
            target.SetAsTargetOf(null);
            target = null;
            player.SwitchState(player.walkingState);
        }
        else if (player.JumpPressed)
        {
            target.SetAsTargetOf(null);
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
