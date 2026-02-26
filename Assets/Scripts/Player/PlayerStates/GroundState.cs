public class GroundState : IPlayerState
{
    public void EnterState(Player player)
    {
        player.velocity.y = 0;
    }

    public void UpdateState(Player player)
    {
        player.velocity.y = 0;
        player.Move();
        if (player.jumpPressed)
        {
            player.jumpPressed = false;
            player.SwitchState(player.jumpingState);
        }
        else if (!player.GroundDetected())
            player.SwitchState(player.fallingState);
    }
}
