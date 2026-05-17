using System.Collections;
using UnityEngine;

public class DashingState : IPlayerState
{
    private float endReduction;
    private bool collidedWall;
    private bool startedOnAir;
    private float dashCounter;
    private bool freezeBehaviour;
    private Vector2 initialVelocity;
    private const float diagonalValue = 0.7071f;

    public void EnterState(Player player)
    {
        collidedWall = false;
        endReduction = 1f; // time to cut from the dash end
        AudioManager.instance.PlayRandom(AudioName.DashOne, AudioName.DashTwo);
        player.hasDashAir = true;
        freezeBehaviour = false;
        player.ConsumeDash();
        dashCounter = 0f;
        player.velocity = Vector2.zero;
        player.velocityXSmoothing = player.velocityYSmoothing = 0f;
        float dashSpeed = player.playerParameters.dashDistance / player.playerParameters.dashTime;
        if (player.input == Vector2.zero || (player.input.x == 0 && !player.playerParameters.canDashVertical && !player.playerParameters.canDashDiagonal))
            player.input = new Vector2(-player.GetFacingDir(), 0);
        else if (!player.playerParameters.canDashVertical && !player.playerParameters.canDashDiagonal)
            player.input.y = 0f;
        else if (player.playerParameters.canDashVertical && !player.playerParameters.canDashDiagonal)
        {
            if (player.input.x != 0f) player.input = new Vector2(Mathf.Sign(player.input.x), 0f);
            else player.input = new Vector2(0, Mathf.Sign(player.input.y));
        }
        else
        {
            if (player.input.y != 0f && player.input.x != 0f)
                player.input = new Vector2(Mathf.Sign(player.input.x) * diagonalValue, Mathf.Sign(player.input.y) * diagonalValue);
            else if (player.input.y != 0f)
                player.input = new Vector2(0f, Mathf.Sign(player.input.y));
            else
                player.input = new Vector2(Mathf.Sign(player.input.x), 0f);
        }
        player.velocity = player.input * dashSpeed;
        initialVelocity = player.velocity;
        startedOnAir = !player.GroundBelow();
        if (startedOnAir)
        {
            player.ForceFallingAnimation();
            player.StartCoroutine(StopFall(player));
        }
        else
            player.StopFallingAnimation();
        Debug.Log(startedOnAir);
    }

    public void UpdateState(Player player)
    {
        if (freezeBehaviour) return;
        dashCounter += Time.deltaTime;

        player.Move(true, false, 0f);
        if (player.GroundBelow())
        {
            if (initialVelocity.y != 0f || startedOnAir)
            {
                player.ActivateDash();
                player.velocity = Vector2.zero;
                player.MakeSplash(0f);
                player.ForceFallingAnimation();
                player.StopFallingAnimation();
                AudioManager.instance.Play(AudioName.FallHeavy);
                player.SwitchState(player.idleState);
                Debug.Log("Toco piso");
                return;
            }
            else player.PaintTrail();
        }

        if (player.CeilingAbove())
        {
            AudioManager.instance.Play(AudioName.FallHeavy);
            player.SwitchState(player.fallingState);
            return;
        }

        if ((player.WallLeft() || player.WallRight()) && !player.IsFrozen)
        {
            float dir = player.WallLeft() ? -1 : 1;
            player.FlipSprite(dir);
            if (player.playerParameters.splashWallMinVelocity <= Mathf.Abs(player.velocity.x))
                player.MakeSplash(90f * dir);
            player.StopFallingAnimation();
            player.HandleWallSlidingStateTransition();
            freezeBehaviour = true;
            return;
        }
        else if(player.CollisionLeft() || player.CollisionRight())
        {
            collidedWall = true;
            endReduction = 0.5f; // if collides with wall, stop dashing sooner
            dashCounter = player.playerParameters.dashTime;
        }

        if (dashCounter >= player.playerParameters.dashTime)
        {
            bool fallingDash = initialVelocity.y < 0;

            float extraGroundTime = player.GroundBelow() && !collidedWall ? player.playerParameters.decelerationTimeDash * 0.7f : 0f;

            if (dashCounter < player.playerParameters.dashTime + extraGroundTime)
                return;

            float decelTimeX = (player.GroundBelow() ? player.playerParameters.decelerationTimeDash * 0.25f : player.playerParameters.decelerationTimeDash) * endReduction;
            float targetY = fallingDash ? player.playerParameters.maxFallSpeed : 0f;
            player.velocity.x = Mathf.SmoothDamp(player.velocity.x, 0f, ref player.velocityXSmoothing, decelTimeX);
            player.velocity.y = Mathf.SmoothDamp(player.velocity.y, targetY, ref player.velocityYSmoothing, player.playerParameters.decelerationTimeDash * endReduction);

            if (fallingDash)
                player.SwitchState(player.fallingState);

            if (Mathf.Abs(player.velocity.x) <= 0.25f && Mathf.Abs(player.velocity.y) <= 0.25f)
            {
                player.velocity = Vector2.zero;

                if (player.GroundBelow())
                {
                    if (fallingDash)
                        player.ActivateDash();
                    player.SwitchState(player.idleState);
                    Debug.Log("Asi tan natural");
                }
                else
                {
                    player.SwitchState(player.fallingState);
                }
            }
        }
    }

    private IEnumerator StopFall(Player player)
    {
        yield return null;
        player.StopFallingAnimation();
    }
}
