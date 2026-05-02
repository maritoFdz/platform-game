public class SlopeSlidingState : IPlayerState
{
    private int direction;
    public void EnterState(Player player)
    {
        AudioManager.instance.Play(AudioName.Sliding);
        direction = player.WallLeft() ? -1 : 1;
        player.FlipSprite(-direction);
    }

    public void UpdateState(Player player)
    {
        player.Move(true, false, player.playerParameters.gravitySlideMultiplier);
        if (player.JumpPressed)
        {
            player.velocity.x = player.playerParameters.slopeSlidingJump.x * -direction;
            player.velocity.y = player.playerParameters.slopeSlidingJump.y;
            AudioManager.instance.StopPlaying(AudioName.Sliding);
            player.SwitchState(player.fallingState);
        }
        else if (!player.IsSliding())
        {
            player.velocity.x = player.playerParameters.endSlopeBoostX * -direction;
            AudioManager.instance.Play(AudioName.Sliding);
            player.SwitchState(player.idleState);
        }
    }
}
