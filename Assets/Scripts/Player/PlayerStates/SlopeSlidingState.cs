public class SlopeSlidingState : IPlayerState
{
    private int direction;
    public void EnterState(Player player)
    {
        direction = player.WallLeft() ? -1 : 1;
        player.FlipSprite(-direction);
    }

    public void UpdateState(Player player)
    {
        player.Move(true, false);
        if (player.JumpPressed)
        {
            player.velocity.x = player.playerParameters.slopeSlidingJump.x * -direction;
            player.velocity.y = player.playerParameters.slopeSlidingJump.y;
            player.SwitchState(player.fallingState);
        }
        else if (!player.IsSliding())
        {
            player.velocity.x = player.playerParameters.endSlopeBoostX * -direction;
            player.SwitchState(player.idleState);
        }
    }
}
