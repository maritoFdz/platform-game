using System.Collections;
using UnityEngine;

public class PlayerAnimationStateController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;

    [Header("Parameters")]
    [SerializeField] private float idleWaitTime;

    private int isWalkingHash;
    private int isRunningHash;
    private int playIdleHash;

    private bool idleCancelled;

    private void Start()
    {
        isWalkingHash = Animator.StringToHash("IsWalking");
        isRunningHash = Animator.StringToHash("IsRunning");
        playIdleHash = Animator.StringToHash("PlayIdle");
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


    #region Animation Clips Events
    public void StartIdleCountdown()
    {
        if (!idleCancelled)
            StartCoroutine(IdleLoop());
    }
    #endregion
}
