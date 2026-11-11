using UnityEngine;

public class DyeEventState : IPlayerState
{
    private bool animates;
    public void EnterState(Player player)
    {
        player.StopMovementSounds();
        player.DisableInput();
        player.FreezeAnimator();
        player.ActivateLight();
        player.velocity = Vector2.zero;
        animates = false;
        DyeEventManager.DyeEventAnimation -= Animate;
        DyeEventManager.DyeEventAnimation += Animate;
    }

    public void UpdateState(Player player)
    {
        if (animates)
        {
            animates = false;
            player.ResumeAnimator();
            player.StartDyeAnimation();
        }
        if (player.IsActive)
        {
            DyeEventManager.DyeEventAnimation -= Animate;
            player.SwitchState(player.idleState);
        }
    }

    public void Animate(float time) // de free el tiempo realmente
    {
        animates = true;
        DyeEventManager.DyeEventAnimation -= Animate;
    }
}
