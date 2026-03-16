using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAnimationStateController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Player player;

    [Header("Parameters")]
    [SerializeField] private float idleWaitTime;

    [Header("VFX Settings")]
    public GameObject splashVFXPrefab;

    [Header("Other")]
    [SerializeField] private Color frozenColor;
    [SerializeField] private float colorSpeed;
    public Coroutine colorRoutine;

    private int isWalkingHash;
    private int isRunningHash;
    private int isFallingHash;
    private int playIdleHash;
    private int startJumpHash;
    private int endJumpHash;

    private bool idleCancelled;

    private void Start()
    {
        isWalkingHash = Animator.StringToHash("IsWalking");
        isRunningHash = Animator.StringToHash("IsRunning");
        isFallingHash = Animator.StringToHash("IsFalling");
        playIdleHash = Animator.StringToHash("PlayIdle");
        startJumpHash = Animator.StringToHash("StartJump");
        endJumpHash = Animator.StringToHash("EndJump");
    }

    private IEnumerator IdleLoop()
    {
        yield return new WaitForSeconds(idleWaitTime);
        if (idleCancelled) yield break;
        animator.SetTrigger(playIdleHash);
    }

    public void FlipX(bool isFacingLeft)
    {
        spriteRenderer.flipX = isFacingLeft;
    }

    public void StartFreezeEffect(float freezeTime)
    {
        colorRoutine ??= StartCoroutine(FreezeColorCo(freezeTime));
    }

    public void StopFreezeEffect()
    {
        if (colorRoutine != null)
            StopCoroutine(colorRoutine); // stops current color effect

        colorRoutine = StartCoroutine(UnFreezeColorCo());
    }

    private IEnumerator FreezeColorCo(float duration)
    {
        Color startColor = spriteRenderer.color;
        float timer = 0f;
        while (true)
        {
            if (player.onFreezeTile) // changes player color and actualizes counter so staying on an ice tile doesnt count as time frozen
            {
                spriteRenderer.color = Color.Lerp(spriteRenderer.color, frozenColor, Time.deltaTime * colorSpeed);
                startColor = frozenColor;
                timer = 0f;
            }
            else
            {
                timer += Time.deltaTime;
                float changeRatio = Mathf.Clamp01(timer / duration); // normalizes time to exit frozen state
                spriteRenderer.color = Color.Lerp(startColor, Color.white, changeRatio);
            }

            if (!player.onFreezeTile && spriteRenderer.color == Color.white)
            {
                spriteRenderer.color = Color.white;
                colorRoutine = null;
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator UnFreezeColorCo()
    {
        while (spriteRenderer.color != Color.white)
        {
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.white, Time.deltaTime * colorSpeed * 10);
            yield return null;
        }
        spriteRenderer.color = Color.white;
        colorRoutine = null;
    }

    public void ResetFreezeColor()
    {
        if (colorRoutine != null)
            StopCoroutine(colorRoutine);

        colorRoutine = null;
        spriteRenderer.color = Color.white;
    }

    public void PlayIdle()
    {
        idleCancelled = false;
        animator.SetTrigger(playIdleHash);
    }

    public void StopIdle()
    {
        idleCancelled = true;
    }

    public void PlayWalking()
    {
        animator.SetBool(isWalkingHash, true);
    }

    public void StopWalking()
    {
        animator.SetBool(isWalkingHash, false);
    }

    public void PlayRunning()
    {
        animator.SetBool(isRunningHash, true);
    }

    public void StopRunning()
    {
        animator.SetBool(isRunningHash, false);
    }

    public void PlayFalling()
    {
        animator.SetBool(isFallingHash, true);
    }

    public void StopFalling()
    {
        animator.SetBool(isFallingHash, false);
    }

    public void PlayJumping()
    {
        animator.SetTrigger(startJumpHash);
    }

    public void StopJumping()
    {
        animator.SetTrigger(endJumpHash);
    }

    public void MakeSplash(float angle)
    {
        Instantiate(splashVFXPrefab, player.transform.position, Quaternion.Euler(0, 0, angle));
    }

    #region Animation Clips Events
    public void StartIdleCountdown()
    {
        if (!idleCancelled)
            StartCoroutine(IdleLoop());
    }
    
    public void ExecuteJump()
    {
        player.SwitchState(player.jumpingState);
    }

    public void StartFallingAnimation()
    {
        animator.SetTrigger(endJumpHash);
    }
    #endregion
}
