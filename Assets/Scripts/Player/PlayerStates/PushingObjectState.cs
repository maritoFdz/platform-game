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
        if (!target)
            Debug.Log("Papito algo esta dando bateo");
    }

    public void UpdateState(Player player)
    {
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity * 0.5f, ref player.velocityXSmoothing, player.accelerationTimeGround);
        // push
        if (Mathf.Sign(player.velocity.x) != pushDirection || !player.IsPushing() || !target)
        {
            player.SwitchState(player.walkingState);
        }
        else if (player.JumpPressed)
            player.SwitchState(player.jumpingState);
    }
}
