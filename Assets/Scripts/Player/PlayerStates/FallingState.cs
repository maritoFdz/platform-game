using UnityEngine;

public class FallingState : IPlayerState
{
    public void EnterState(Player player)
    {
        Debug.Log(player.transform.position.y);
    }

    public void UpdateState(Player player)
    {
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity, ref player.velocityXSmoothing, player.accelerationTimeAir);
        player.Move(player.gravityFallMultiplier);
        if (player.GroundDetected())
            player.SwitchState(player.groundState);
    }
}
