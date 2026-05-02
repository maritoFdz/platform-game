using UnityEngine;

public class AutoMoveState : IPlayerState
{
    private float endTime;

    public void EnterState(Player player)
    {
        player.pendingAutoMove = false;
        player.PlayWalkingAnimation();
        player.velocity = Vector2.zero;
        player.velocityXSmoothing = 0;
        endTime = Time.time + player.autoMoveDuration;
    }

    public void UpdateState(Player player)
    {
        if (Time.time >= endTime)
        {
            player.StopWalkingAnimation();
            if (AudioManager.instance != null) AudioManager.instance.StopPlaying(AudioName.Movement);
            player.SwitchState(player.idleState);
            player.EnableInput();
            return;
        }

        player.targetVelocity = player.autoMoveDir * player.autoMoveSpeed;
        player.FlipSprite(player.autoMoveDir);
        player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity, ref player.velocityXSmoothing, player.playerParameters.accelerationTimeGround);
        player.PaintTrail();
        player.Move(false, false, 0f);
    }
}