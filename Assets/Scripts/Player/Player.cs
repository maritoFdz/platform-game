using System;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CollisionsHandler2D))]
public class Player : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CollisionsHandler2D controller;
    [SerializeField] private PlayerAnimationStateController animationController;
    [SerializeField] private TilesInteractionHandler tilesController;
    public PlayerParameters playerParameters;
    [SerializeField] private Player playerPrefab;

    [HideInInspector] public float gravityScale;
    [HideInInspector] public float jumpForce;
    [HideInInspector] public float minJumpForce;
    [HideInInspector] public Vector2 input;
    [HideInInspector] public Vector2 velocity;
    [HideInInspector] public float velocityXSmoothing;
    [HideInInspector] public float velocityYSmoothing;
    [HideInInspector] public float targetVelocity;
    [HideInInspector] public bool onFreezeTile;
    [HideInInspector] public bool pendingAutoMove;
    [HideInInspector] public bool hasDashAir;
    [HideInInspector] public float autoMoveDir;
    [HideInInspector] public float autoMoveSpeed;
    [HideInInspector] public float autoMoveDuration;

    private PlayerInput playerInput;
    public bool JumpPressed => jumpBufferCounter > 0;
    public bool JumpReleased => !playerInput.Player.Jump.IsPressed();
    public bool IsRunning => playerInput.Player.Run.IsPressed();
    public bool IsMoving => playerInput.Player.Move.IsPressed();
    public bool IsFrozen => Time.time < freezeTime;
    public bool IsDashing => dashBufferCounter > 0 && dashCooldownCounter <= 0 && playerParameters.canDash;
    public bool IsFull => transform.localScale.Equals(playerParameters.maxScale);

    public bool IsActive => isActive;

    private float freezeTime;
    private float jumpBufferCounter;
    private float dashBufferCounter;
    private float dashCooldownCounter;
    private float normalizedScale;
    private float moveAmount;
    private bool isActive;

    private IPlayerState currentState;
    // states instances
    public IdleState idleState = new();
    public FallingState fallingState = new();
    public WalkingState walkingState = new();
    public DashingState dashingState = new();
    public RunningState runningState = new();
    public JumpingState jumpingState = new();
    public WallSlidingState wallSlidingState = new();
    public WallJumpState wallJumpState = new();
    public SlopeSlidingState slopeSlidingState = new();
    public PushingObjectState pushingObjectState = new();
    public SwimingState swimingState = new();
    public AutoMoveState autoMoveState = new();

    private void Start()
    {
        SwitchState(idleState);
        gravityScale = -(2 * playerParameters.maxJumpHeight) / Mathf.Pow(playerParameters.maxHeightTime, 2);
        jumpForce = Mathf.Abs(gravityScale * playerParameters.maxHeightTime);
        minJumpForce = Mathf.Sqrt(2 * Mathf.Abs(gravityScale) * playerParameters.minJumpHeight);
        PlayerSwitchManager.instance.Add(this);
    }

    private void OnEnable()
    {
        ApplyScale();
    }

    private void Update()
    {
        HandleCounters();
        input = isActive ? playerInput.Player.Move.ReadValue<Vector2>() : new Vector2(0f, 0f);
        targetVelocity = input.x * playerParameters.moveSpeed;
        currentState.UpdateState(this);
        tilesController.HandleTilesCollision();
    }

    private void HandleCounters()
    {
        if (jumpBufferCounter > 0f)
            jumpBufferCounter -= Time.deltaTime;
        if (dashCooldownCounter > 0f)
            dashCooldownCounter -= Time.deltaTime;
        if (dashBufferCounter > 0f)
            dashBufferCounter -= Time.deltaTime;
    }

    public void SwitchState(IPlayerState nextState)
    {
        currentState = nextState;
        Debug.Log(nextState.ToString());
        currentState.EnterState(this);
    }

    public void Move(bool storeHorMovement, bool storeVerMovement, float gravityMultiplier = 1f)
    {
        float dt = Time.deltaTime;
        Vector2 acceleration = new(0, gravityScale * gravityMultiplier);
        Vector2 deltaMove = GetDisplacement(dt, acceleration);
        if (currentState is DashingState && controller.colDetails.onSlopeDescent)
        {
            float direction = Mathf.Sign(deltaMove.x);
            deltaMove.x += Mathf.Abs(deltaMove.y) * direction;
            deltaMove.y = 0f;
        }
        transform.Translate(deltaMove);
        if (storeHorMovement && !IsFrozen) moveAmount += Mathf.Abs(deltaMove.x);
        if (storeVerMovement && !IsFrozen) moveAmount += Mathf.Abs(deltaMove.y);
        Shrink();
        velocity += acceleration * dt;
    }

    public Vector2 GetDisplacement(float dt, Vector2 acceleration)
    {
        velocity.y = Mathf.Max(velocity.y, -playerParameters.maxFallSpeed);
        Vector2 deltaMove = velocity * dt + 0.5f * dt * dt * acceleration;
        controller.ClampDisplacement(ref deltaMove);
        return deltaMove;
    }

    public void ConsumeJump()
    {
        jumpBufferCounter = 0f;
    }

    public void ConsumeDash()
    {
        dashCooldownCounter = playerParameters.dashCooldown;
        dashBufferCounter = 0f;
    }

    public void ActivateDash()
    {
        dashCooldownCounter = 0f;
    }

    private void Jump(InputAction.CallbackContext callback)
    {
        if (!isActive) return;
        jumpBufferCounter = playerParameters.jumpBufferTime;
    }

    private void Dash(InputAction.CallbackContext context)
    {
        if (!isActive) return;
        dashBufferCounter = playerParameters.dashBufferTime;
    }

    public float GetFacingDir()
    {
        return animationController.FacingDir;
    }

    public void MakeSplash(float rotation, bool skipSound = false)
    {
        if (IsFrozen) return;
        if (!tilesController || !animationController) return;
        tilesController.PaintSplash(transform.position, rotation);
        animationController.MakeSplash(rotation);
        Shrink(true, 2);
        if (!skipSound) AudioManager.instance.Play(AudioName.FallHeavy);
    }

    public void PaintTrail()
    {
        if (!IsFrozen)
            tilesController.PaintTrail();
    }

    private void Split(InputAction.CallbackContext callback)
    {
        if (IsFrozen) return;
        if (!isActive) return;
        if (normalizedScale / 2 > playerParameters.minNormalizedScale)
        {
            normalizedScale /= 2;
            ApplyScale();
            Player child = Instantiate(playerPrefab, transform.position + new Vector3(0.1f, 0, 0), Quaternion.identity);
            child.SetNormalizedScale(normalizedScale);
            AudioManager.instance.Play(AudioName.Split);
        }
    }

    public void Shrink(bool forcedScaleLoss = false, int scaleLossUnits = 1)
    {
        if (IsFrozen) return;
        if (moveAmount < 1 && !forcedScaleLoss) return;
        if (!forcedScaleLoss) moveAmount--;
        normalizedScale = Mathf.Max(normalizedScale - playerParameters.scaleReductionPerUnit * scaleLossUnits, playerParameters.minNormalizedScale);
        ApplyScale();
        if (normalizedScale == playerParameters.minNormalizedScale)
            KillPlayer();
        
    }

    public void Upscale()
    {
        if (IsFrozen) return;
        normalizedScale = Mathf.Min(normalizedScale + playerParameters.scaleReductionPerUnit * playerParameters.upscalePerUnit, 1f);
        ApplyScale();
    }

    private void ApplyScale()
    {
        transform.localScale = playerParameters.maxScale * normalizedScale;
        controller.UpdateCollisions(normalizedScale);
    }

    public void KillPlayer()
    {
        // todo animation death event
        animationController.ResetFreezeColor();
        PlayerSwitchManager.instance.Erase(this);
    }

    public void Freeze(float time)
    {
        freezeTime = Time.time + time;
        animationController.StartFreezeEffect(time);
    }

    public void UnFreeze()
    {
        freezeTime = 0;
        animationController.StopFreezeEffect();
    }

    public void ApplyExternalDisplacement(Vector2 deltaMove)
    {
        Vector2 moveAmount = new(deltaMove.x, deltaMove.y);
        controller.ClampDisplacement(ref moveAmount);
        transform.Translate(moveAmount);
        this.moveAmount += moveAmount.x;
    }

    public void StartAutoMove(float direction, float speed, float duration)
    {
        pendingAutoMove = true;
        autoMoveDir = direction;
        autoMoveSpeed = speed;
        autoMoveDuration = duration;
    }

    public void SetNormalizedScale(float value)
    {
        normalizedScale = value;
        ApplyScale();
    }

    public void SetInput(PlayerInput input)
    {
        playerInput = input;
    }

    public void EnableInput()
    {
        SetActiverState(true);
        playerInput.Player.Jump.performed -= Jump;
        playerInput.Player.Jump.performed += Jump;

        playerInput.Player.Split.performed -= Split;
        playerInput.Player.Split.performed += Split;

        playerInput.Player.Dash.performed -= Dash;
        playerInput.Player.Dash.performed += Dash;
    }

    public void SetActiverState(bool enable)
    {
        isActive = enable;
    }

    public void DisableInput()
    {
        SetActiverState(false);
        playerInput.Player.Jump.performed -= Jump;
        playerInput.Player.Split.performed -= Split;
        playerInput.Player.Dash.performed -= Dash;
    }

    #region Collisions related methods called by states
    public bool GroundBelow()
    {
        return controller.colDetails.below;
    }

    public bool CeilingAbove()
    {
        return controller.colDetails.above;
    }

    public bool OnSlope()
    {
        return controller.colDetails.onSlope || controller.colDetails.onSlopeDescent;
    }

    public bool WallLeft()
    {
        if (controller.colDetails.left)
            return tilesController.IsWallScalable(Vector2.left);
        return false;
    }

    public bool WallRight()
    {
        if (controller.colDetails.right)
            return tilesController.IsWallScalable(Vector2.right);
        return false;
    }

    public bool CollisionLeft()
    {
        return controller.colDetails.left;
    }

    public bool CollisionRight()
    {
        return controller.colDetails.right;
    }

    public bool HasSlopeNear(int direction, int tolerance = 1)
    {
        return controller.IsNextToSlope(direction, tolerance) || controller.IsSlopeBelow(tolerance);
    }

    public bool IsSliding()
    {
        return controller.colDetails.onSlopeSlide;
    }

    public bool IsPushing()
    {
        return controller.colDetails.nextPushable;
    }

    public bool OnWater()
    {
        return tilesController.CheckWater();
    }

    public PushableObject GetPushableObject(float direction)
    {
        Vector2 raycastOrigin = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.right * direction, Mathf.Infinity, controller.GetPushableLayer());
        if (!hit)
            return null;
        return hit.transform.GetComponent<PushableObject>();
    }

    public SlimeSupply GetCurrentwater()
    {
        if (tilesController.currentWater  == null) return null;
        return tilesController.currentWater;
    }

    public void ClearCurrentWater()
    {
        tilesController.currentWater = null;
    }

    public float GetWaterSurface()
    {
        if (tilesController.currentWater == null) return -1f;
        return tilesController.currentWater.GetSurfaceHeight();
    }
    #endregion

    #region Animations related methods called by states
    public void FlipSprite(float direction)
    {
        animationController.FlipX(direction < 0);
    }

    public void PlayIdleAnimation()
    {
        animationController.PlayIdle();
    }

    public void StopIdleAnimation()
    {
        animationController.StopIdle();
    }

    public void PlayWalkingAnimation()
    {
        animationController.PlayWalking();
    }

    public void StopWalkingAnimation()
    {
        animationController.StopWalking();
    }

    public void PlayRunningAnimation()
    {
        animationController.PlayRunning();
    }

    public void StopRunningAnimation()
    {
        animationController.StopRunning();
    }

    public void PlayFallingAnimation()
    {
        animationController.PlayFalling();
    }

    public void StopFallingAnimation()
    {
        animationController.StopFalling();
    }

    public void ForceJumpingAnimation()
    {
        animationController.ForceInstantJump();
    }

    public void ForceFallingAnimation()
    {
        animationController.ForceInstantFall();
    }

    public void StopJumpingAnimation()
    {
        animationController.StopJumping();
    }

    public void HandleJumpingStateTransition()
    {
        animationController.PlayJumping();
    }

    public void HandleWallSlidingStateTransition()
    {
        animationController.PlayWallSliding();
    }

    public void StopWallSlidingAnimation()
    {
        animationController.StopWallSliding();
    }

    #endregion
}
