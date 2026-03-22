using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class WaitingState : IPlayerState
{
    private bool onGround;
    private bool needsPush;
    private int slopeDir;
    public void EnterState(Player player)
    {
        onGround = false;
        needsPush = false;
        player.velocity = Vector2.zero;
        player.velocityXSmoothing = 0;
        player.velocityYSmoothing = 0;
        player.PlayIdleAnimation();
    }

    public void UpdateState(Player player)
    {
        if (!onGround)
        {
            player.Move(false, false, player.gravityFallMultiplier);
            onGround = player.GroundBelow();
        }
        else if (player.IsSliding())
        {
            if (player.splashFallMinVelocity <= Mathf.Abs(player.velocity.y))
                player.MakeSplash(0f);
            needsPush = true;
            slopeDir = player.WallLeft() ? -1 : 1;
            player.FlipSprite(-slopeDir);
            player.Move(true, false);
        }
        else
        {
            if (player.splashFallMinVelocity <= Mathf.Abs(player.velocity.y))
                player.MakeSplash(0f);
            player.velocity.y = 0f;
            if (needsPush)
                player.velocity.x = Mathf.MoveTowards(-slopeDir * player.endSlopeBoostX, 0f, 1f * Time.deltaTime);
            player.Move(false, false, 0);
        }
    }
}
