using UnityEngine;

public class AutoMoveState : IPlayerState
{
    private float endTime;
    private bool isFalling;

    public void EnterState(Player player)
    {
        isFalling = !player.GroundBelow();
        player.StopIdleAnimation();
        player.hasJumpAir = false;
        player.hasDashAir = false;
        player.pendingAutoMove = false;
        if (isFalling)
            player.PlayFallingAnimation();
        else
        {
            player.PlayWalkingAnimation();
            if (AudioManager.instance != null)
                AudioManager.instance.Play(AudioName.Movement);
        }
        player.velocity = Vector2.zero;
        player.velocityXSmoothing = 0;
        endTime = Time.time + player.autoMoveDuration;
    }

    public void UpdateState(Player player)
    {
        if (Time.time >= endTime)
        {
            player.EnableInput();
            if (!isFalling)
            {
                player.StopWalkingAnimation();
                if (AudioManager.instance != null) AudioManager.instance.StopPlaying(AudioName.Movement);
                player.SwitchState(player.idleState);
            }
            else
            {
                player.SwitchState(player.fallingState);
            }
            return;
        }

        if (player.GroundBelow())
        {
            if (isFalling)
            {
                isFalling = false;
                player.StopFallingAnimation();
                player.PlayWalkingAnimation();
                if (player.playerParameters.splashFallMinVelocity <= Mathf.Abs(player.velocity.y))
                    player.MakeSplash(0f, false, true);
                else if (player.velocity.y != 0 && AudioManager.instance != null) AudioManager.instance.Play(AudioName.FallWeak);
            }

            player.targetVelocity = player.autoMoveDir * player.autoMoveSpeed;
            player.FlipSprite(player.autoMoveDir);
            player.velocity.x = Mathf.SmoothDamp(player.velocity.x, player.targetVelocity, ref player.velocityXSmoothing, player.playerParameters.accelerationTimeGround);
            player.PaintTrail();
            player.Move(false, false, 0f);
        }
        else
        {
            isFalling = true;
            player.targetVelocity = 0f;
            player.FlipSprite(player.autoMoveDir);
            player.StopWalkingAnimation();
            if (AudioManager.instance != null) AudioManager.instance.StopPlaying(AudioName.Movement);
            player.PlayFallingAnimation();
            player.velocity.x = 0f;
            player.velocityXSmoothing = 0f;
            player.Move(false, false, player.playerParameters.gravityFallMultiplier);
        }
    }
}