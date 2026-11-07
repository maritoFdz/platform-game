using UnityEngine;

public class PushingObjectState : IPlayerState
{
    private float pushDirection;
    private PushableObject target;
    public bool drop;

    public void EnterState(Player player)
    {
        drop = false;
        pushDirection = Mathf.Sign(player.input.x);
        player.velocity.x = 0f;
        player.velocityXSmoothing = 0f;
        target = player.GetPushableObject(pushDirection);
        AudioManager.instance.Play(AudioName.Movement);
        AudioManager.instance.Play(AudioName.Push);
        if (target && target.IsGrounded)
            target.SetAsTargetOf(player);
        else
        {
            AudioManager.instance.StopPlaying(AudioName.Push);
            player.SwitchState(player.idleState);
        }
    }

    public void UpdateState(Player player)
    {
        if (player.pendingAutoMove)
        {
            if (target != null)
            {
                target.SetAsTargetOf(null);
                target = null;
            }
            AudioManager.instance.StopPlaying(AudioName.Movement);
            AudioManager.instance.StopPlaying(AudioName.Push);
            player.SwitchState(player.autoMoveState);
            return;
        }

        if (Mathf.Sign(player.input.x) != pushDirection || player.input.x == 0 || drop)
        {
            if (target != null)
            {
                target.SetAsTargetOf(null);
                target = null;
            }
            AudioManager.instance.StopPlaying(AudioName.Movement);
            AudioManager.instance.StopPlaying(AudioName.Push);
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
            AudioManager.instance.StopPlaying(AudioName.Movement);
            AudioManager.instance.StopPlaying(AudioName.Push);
            player.StopWalkingAnimation();
            player.ForceJumpingAnimation();
            player.SwitchState(player.jumpingState);
            return;
        }
        if (target != null)
            target.SetDirection(player.input.x);
        player.Move(true, false);
        player.PaintTrail();
    }
}
