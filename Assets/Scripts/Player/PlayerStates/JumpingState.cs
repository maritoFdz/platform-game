public class JumpingState : IPlayerState
{
    public void EnterState(Player player)
    {
        player.velocity.y = player.jumpForce;
    }

    public void UpdateState(Player player)
    {
        player.AddGravityForce();
        player.Move();
        if (player.velocity.y <= 0)
            player.SwitchState(player.fallingState);
    }
}
