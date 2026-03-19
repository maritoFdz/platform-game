using UnityEngine;

public class PushingObjectState : IPlayerState
{
    private float pushDirection;
    private PushableObject target;

    public void EnterState(Player player)
    {
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
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity, ref player.velocityXSmoothing, player.accelerationTimeGround);
        if (Mathf.Sign(player.inputX) != pushDirection || player.inputX == 0)
        {
            if (target != null)
            {
                target.SetAsTargetOf(null);
                target = null;
            }
            player.SwitchState(player.walkingState);
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
    }
}
