using UnityEngine;

public class PushingObjectState : IPlayerState
{
    private float pushDirection;
    private PushableObject target;
    public bool drop;

    public void EnterState(Player player)
    {
        drop = false;
        pushDirection = Mathf.Sign(player.inputX);
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
        if (Mathf.Sign(player.inputX) != pushDirection || player.inputX == 0 || drop)
        {
            if (target != null)
            {
                target.SetAsTargetOf(null);
                target = null;
            }
            player.SwitchState(player.idleState);
            return;
        }

        if (player.JumpPressed)
        {
            if (target != null)
            {
                target.SetAsTargetOf(null);
                target = null;
            }
            player.SwitchState(player.jumpingState);
            return;
        }
        if (target != null)
            target.SetDirection(player.inputX);
        player.Move(true, false, 0f);
        player.PaintTrail();
    }
}
