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

    [Header("Other")]
    [SerializeField] private Color frozenColor;
    [SerializeField] private float colorSpeed;
    public Coroutine colorRoutine;
    private Material material;
    private float normalFreezeAmount;

    public float FacingDir => spriteRenderer.flipX ? 1f : -1f;

    private int isWalkingHash;
    private int isRunningHash;
    private int isFallingHash;
    private int playIdleHash;
    private int startJumpHash;
    private int endJumpHash;
    private int freezeAmountHash;
    private int instantJumpHash;
    private int stickWallHash;
    private int isSlidingWallHash;
    private int instantFallHash;

    private bool idleCancelled;

    private void Awake()
    {
        material = spriteRenderer.material;
        isWalkingHash = Animator.StringToHash("IsWalking");
        isRunningHash = Animator.StringToHash("IsRunning");
        isFallingHash = Animator.StringToHash("IsFalling");
        isSlidingWallHash = Animator.StringToHash("IsSlidingWall");
        playIdleHash = Animator.StringToHash("PlayIdle");
        startJumpHash = Animator.StringToHash("StartJump");
        endJumpHash = Animator.StringToHash("EndJump");
        stickWallHash = Animator.StringToHash("StickWall");
        instantJumpHash = Animator.StringToHash("InstantJump");
        instantFallHash = Animator.StringToHash("InstantJumpEnd");
        freezeAmountHash = Shader.PropertyToID("_Freeze_Amount");
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
        float currentFreeze = normalFreezeAmount;
        float timer = 0f;
        while (true)
        {
            if (player.onFreezeTile) // changes player color and actualizes counter so staying on an ice tile doesnt count as time frozen
            {
                normalFreezeAmount = Mathf.Lerp(normalFreezeAmount, 1f, Time.deltaTime * colorSpeed);
                currentFreeze = normalFreezeAmount;
                timer = 0f;
            }
            else
            {
                timer += Time.deltaTime;
                float changeRatio = Mathf.Clamp01(timer / duration); // normalizes time to exit frozen state
                normalFreezeAmount = Mathf.Lerp(currentFreeze, 0f, changeRatio);
            }

            material.SetFloat(freezeAmountHash, normalFreezeAmount);

            if (!player.onFreezeTile && normalFreezeAmount <= 0.001f)
            {
                normalFreezeAmount = 0;
                material.SetFloat(freezeAmountHash, normalFreezeAmount);
                colorRoutine = null;
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator UnFreezeColorCo()
    {
        while (normalFreezeAmount >= 0.001f)
        {
            normalFreezeAmount = Mathf.Lerp(normalFreezeAmount, 0f, Time.deltaTime * colorSpeed * 10);
            material.SetFloat(freezeAmountHash, normalFreezeAmount);
            yield return null;
        }
        normalFreezeAmount = 0f;
        material.SetFloat(freezeAmountHash, normalFreezeAmount);
        colorRoutine = null;
    }

    public void ResetFreezeColor()
    {
        if (colorRoutine != null)
            StopCoroutine(colorRoutine);

        colorRoutine = null;
        normalFreezeAmount = 0f;
        material.SetFloat(freezeAmountHash, normalFreezeAmount);
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
        animator.ResetTrigger(instantFallHash);
    }

    public void PlayJumping()
    {
        animator.SetTrigger(startJumpHash);
    }

    public void StopJumping()
    {
        animator.SetTrigger(endJumpHash);
    }

    public void ForceInstantJump()
    {
        animator.SetTrigger(instantJumpHash);
    }

    public void ForceInstantFall()
    {
        animator.SetTrigger(instantFallHash);
    }

    public void PlayWallSliding()
    {
        animator.SetTrigger(stickWallHash);
        animator.SetBool(isSlidingWallHash, true);
    }

    public void StopWallSliding()
    {
        animator.SetBool(isSlidingWallHash, false);
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

    public void ExecuteWallJump()
    {
        player.SwitchState(player.wallJumpState);
    }

    public void StickWall()
    {
        player.SwitchState(player.wallSlidingState);
    }

    public void StartFallingAnimation()
    {
        animator.SetTrigger(endJumpHash);
    }
    #endregion
}
