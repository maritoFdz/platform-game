using UnityEngine;

public class DyeEventState : IPlayerState
{
    private bool animates;
    private bool finished;

    public void EnterState(Player player)
    {
        player.StopMovementSounds();
        player.DisableInput();
        player.FreezeAnimator();
        player.ActivateLight();
        player.velocity = Vector2.zero;
        animates = false;
        finished = false;

        DyeEventManager.DyeEventEnd -= Finish;
        DyeEventManager.DyeEventEnd += Finish;

        if (player.IsDyeToucher)
        {
            DyeEventManager.DyeEventAnimation -= Animate;
            DyeEventManager.DyeEventAnimation += Animate;
        }
    }

    public void UpdateState(Player player)
    {
        if (animates)
        {
            animates = false;
            player.ResumeAnimator();
            player.StartDyeAnimation();
        }
        if (finished)
        {
            player.ResumeAnimator();
            player.SwitchState(player.idleState);
        }
    }

    public void Animate(float time) // de free el tiempo realmente
    {
        animates = true;
        DyeEventManager.DyeEventAnimation -= Animate;
    }
    private void Finish()
    {
        finished = true;
        DyeEventManager.DyeEventEnd -= Finish;
        DyeEventManager.DyeEventAnimation -= Animate;
    }
}
