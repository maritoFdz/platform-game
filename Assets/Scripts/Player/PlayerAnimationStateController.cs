using System;
using System.Collections;
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

    public void Splash(float angle)
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
