using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerAnimationStateController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Player player;
    [SerializeField] private SpriteRenderer materialRenderer;

    [Header("Parameters")]
    [SerializeField] private float idleWaitTime;

    [Header("VFX Settings")]
    public GameObject splashVFXPrefab;

    [Header("Other")]
    [SerializeField] private Color frozenColor;
    [SerializeField] private float colorSpeed;

    [Header("Ice Effect Settings")]
    [SerializeField] private Material iceMaterial;
    [SerializeField] private GameObject fog;
    [SerializeField] private GameObject snowflakesParticles;
    [SerializeField] private float snowflakesFrequency;

    public Coroutine colorRoutine;
    private float normalFreezeAmount;
    private float snowflakesCounter;
    private GameObject currentFog;

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
    private int anyStateBlockedHash;

    private bool idleCancelled;

    private void Awake()
    {
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
        anyStateBlockedHash = Animator.StringToHash("AnyStateBlocked");
        freezeAmountHash = Shader.PropertyToID("_Freeze_Amount");
    }

    private void Update()
    {
        if (snowflakesCounter > 0f)
            snowflakesCounter -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        SyncMaterialRenderer();
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

    private void SyncMaterialRenderer() // hasta ahora, solo el hielo
    {
        if (materialRenderer == null) return;
        bool visible = normalFreezeAmount > 0.001f;
        materialRenderer.enabled = visible; // opcional, puedes omitir esto y dejar que el alpha 0 baste
        if (!HasMaterialsRendering()) return;
        materialRenderer.sprite = spriteRenderer.sprite;
        materialRenderer.flipX = spriteRenderer.flipX;
    }

    private bool HasMaterialsRendering()
    {
        return normalFreezeAmount > 0.001f;
    }

    public void StartFreezeEffect(float freezeTime)
    {
        if (materialRenderer == null) return;
        if (currentFog == null)
        {
            currentFog = Instantiate(fog, transform);
            currentFog.transform.localPosition = Vector3.zero;
        }
        colorRoutine ??= StartCoroutine(FreezeColorCo(freezeTime));
    }

    public void StopFreezeEffect()
    {
        if (materialRenderer == null) return;
        if (colorRoutine != null)
            StopCoroutine(colorRoutine); // stops current color effect
        colorRoutine = StartCoroutine(UnFreezeColorCo());
    }

    private IEnumerator FreezeColorCo(float duration)
    {
        float currentFreeze = normalFreezeAmount;
        float timer = 0f;
        snowflakesCounter = snowflakesFrequency;
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

            iceMaterial.SetFloat(freezeAmountHash, normalFreezeAmount);
            if (snowflakesCounter <= 0)
            {
                snowflakesCounter = snowflakesFrequency;
                Instantiate(snowflakesParticles, transform.position, Quaternion.identity);
            }

            if (!player.onFreezeTile && normalFreezeAmount <= 0.001f)
            {
                normalFreezeAmount = 0;
                iceMaterial.SetFloat(freezeAmountHash, normalFreezeAmount);
                colorRoutine = null;
                if (currentFog != null)
                {
                    Destroy(currentFog);
                    currentFog = null;
                }
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
            iceMaterial.SetFloat(freezeAmountHash, normalFreezeAmount);
            yield return null;
        }

        if (currentFog != null)
        {
            Destroy(currentFog);
            currentFog = null;
        }
        normalFreezeAmount = 0f;
        iceMaterial.SetFloat(freezeAmountHash, normalFreezeAmount);
        colorRoutine = null;
    }

    public void ResetFreezeColor()
    {
        if (materialRenderer == null) return;
        if (colorRoutine != null)
            StopCoroutine(colorRoutine);

        if (currentFog != null)
        {
            Destroy(currentFog);
            currentFog = null;
        }
        colorRoutine = null;
        normalFreezeAmount = 0f;
        iceMaterial.SetFloat(freezeAmountHash, normalFreezeAmount);
    }

    public void PlayIdle()
    {
        idleCancelled = false;
        animator.SetBool(isWalkingHash, false);
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
        LockAnyStateTransitions();
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
        EnableAnyStateTransitions();
    }

    public void ExecuteWallJump()
    {
        player.SwitchState(player.wallJumpState);
        EnableAnyStateTransitions();
    }

    public void StickWall()
    {
        player.SwitchState(player.wallSlidingState);
    }

    public void StartFallingAnimation()
    {
        animator.SetTrigger(endJumpHash);
    }

    public void LockAnyStateTransitions()
    {
        animator.SetBool(anyStateBlockedHash, true);
    }

    public void EnableAnyStateTransitions()
    {
        animator.SetBool(anyStateBlockedHash, false);
    }
    #endregion
}
