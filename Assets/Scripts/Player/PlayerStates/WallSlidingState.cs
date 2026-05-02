using UnityEngine;

public class WallSlidingState : IPlayerState
{
    private int direction;
    private float dirDecisionTimer;
    private float dropTimer;
    private const float gravityMultiplier = 0f;
    private const float wallEndTimer = 0.1f;
    private bool freezeBehaviour;
    private float wallAttachTimer;

    public void EnterState(Player player)
    {
        player.hasDashAir = false;
        player.ActivateDash();
        freezeBehaviour = false;
        direction = player.WallLeft() ? -1 : 1;
        player.FlipSprite(direction);
        player.velocity = Vector2.zero; // cancel all movement
        player.velocityXSmoothing = 0f;
        player.velocityYSmoothing = 0f;
        dirDecisionTimer = player.playerParameters.wallStickTime;
        dropTimer = player.playerParameters.wallStickTime;
        wallAttachTimer = wallEndTimer;
        AudioManager.instance.Play(AudioName.Sliding);
    }

    public void UpdateState(Player player)
    {
        if (freezeBehaviour) return;
        player.velocity.y = Mathf.SmoothDamp(player.velocity.y, -player.playerParameters.wallSlideSpeed, ref player.velocityYSmoothing, player.playerParameters.accelerationTimeWall);
        player.velocity.x = direction * 0.1f;
        player.Move(false, true, gravityMultiplier);
        player.PaintTrail();
        bool onWall = direction == -1 ? player.WallLeft() : player.WallRight();

        if (!onWall)
        {
            wallAttachTimer -= Time.deltaTime;
            if (wallAttachTimer <= 0f)
            {
                AudioManager.instance.StopPlaying(AudioName.Sliding);
                player.SwitchState(player.fallingState);
                return;
            }
        }
        else
            wallAttachTimer = wallEndTimer;

        if (player.JumpPressed || player.playerParameters.wallStickTime != dirDecisionTimer) // if is jump pressed or countdown to chose direction has started
        {
            if (dirDecisionTimer < 0 || player.input.x != 0f) ExecuteJump(player);
            else dirDecisionTimer -= Time.deltaTime;
        }
        else if (player.OnSlope() || player.GroundBelow())
        {
            AudioManager.instance.StopPlaying(AudioName.Sliding);
            player.SwitchState(player.idleState);
        }
        else if (-player.input.x == direction || player.playerParameters.wallStickTime != dropTimer)
        {
            dropTimer -= Time.deltaTime;

            if (dropTimer <= 0f)
            {
                player.velocity.x = player.playerParameters.fallOfMove.x * -direction;
                player.velocity.y = player.playerParameters.fallOfMove.y;
                AudioManager.instance.StopPlaying(AudioName.Sliding);
                player.SwitchState(player.fallingState);
            }
        }
    }

    private void ExecuteJump(Player player)
    {
        if (direction == Mathf.Sign(player.input.x) && player.input.x != 0)
            player.wallJumpState.jumpType = WallJump.Climb;
        else if (direction == Mathf.Sign(- player.input.x) && player.input.x != 0)
            player.wallJumpState.jumpType = WallJump.Front;
        else
        {
            player.velocity.x = player.playerParameters.fallOfMove.x * -direction;
            player.velocity.y = player.playerParameters.fallOfMove.y;
            player.SwitchState(player.fallingState);
            return;
        }
        player.HandleJumpingStateTransition();
        AudioManager.instance.StopPlaying(AudioName.Sliding);
        player.MakeSplash(90f * direction, true);
        freezeBehaviour = true;
        return;
    }
}
