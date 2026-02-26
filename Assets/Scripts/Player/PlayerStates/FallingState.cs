public class FallingState : IPlayerState
{
    public void EnterState(Player player)
    {
    }

    public void UpdateState(Player player)
    {
        player.AddGravityForce();
        player.Move();
        if (player.GroundDetected())
            player.SwitchState(player.groundState);
    }
}
