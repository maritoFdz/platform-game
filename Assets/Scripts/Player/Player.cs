using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CollisionsHandler2D))]
public class Player : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CollisionsHandler2D controller;
    [SerializeField] private PlayerAnimationStateController animationController;

    [Header("Movement Settings")]
    [SerializeField] private float maxJumpHeight;
    [SerializeField] private float minJumpHeight;
    [SerializeField] private float maxHeightTime;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpBufferTime;
    public float timeToIdle;
    public float coyoteTime;
    public float accelerationTimeGround;
    public float accelerationTimeAir;
    public float accelerationTimeWall;
    public float gravityFallMultiplier;
    public float runningVelocityMultiplier;
    public float wallSlideSpeed;

    [Header("Wall Movement Settings")]
    public Vector2 frontDirectionJump;
    public Vector2 climbJump;
    public Vector2 fallOfJump;
    public float wallStickTime;

    [HideInInspector] public float gravityScale;
    [HideInInspector] public float jumpForce;
    [HideInInspector] public float minJumpForce;
    [HideInInspector] public float inputX;
    [HideInInspector] public Vector2 velocity;
    [HideInInspector] public float velocityXSmoothing;
    [HideInInspector] public float velocityYSmoothing;
    [HideInInspector] public float targetVelocity;

    private PlayerInput PlayerInput;
    public bool JumpPressed => jumpBufferCounter > 0;
    public bool JumpReleased => !PlayerInput.Player.Jump.IsPressed();
    public bool IsRunning => PlayerInput.Player.Run.IsPressed();
    public bool IsMoving => PlayerInput.Player.Move.IsPressed();
    private float jumpBufferCounter;

    private IPlayerState currentState;
    // states instances
    public IdleState idleState = new();
    public FallingState fallingState = new();
    public WalkingState walkingState = new();
    public RunningState runningState = new();
    public JumpingState jumpingState = new();
    public WallSlidingState wallSlidingState = new();

    private void Awake()
    {
        PlayerInput = new();
    }

    private void Start()
    {
        SwitchState(idleState);
        gravityScale = -(2 * maxJumpHeight) / Mathf.Pow(maxHeightTime, 2);
        jumpForce = Mathf.Abs(gravityScale * maxHeightTime);
        minJumpForce = Mathf.Sqrt(2 * Mathf.Abs(gravityScale) * minJumpHeight);
    }

    private void OnEnable()
    {
        PlayerInput.Player.Enable();
        PlayerInput.Player.Jump.performed += Jump;
    }

    private void OnDisable()
    {
        PlayerInput.Player.Jump.performed -= Jump;
        PlayerInput.Disable();
    }

    private void Update()
    {
        if (jumpBufferCounter > 0f)
            jumpBufferCounter -= Time.deltaTime;
        inputX = PlayerInput.Player.Move.ReadValue<float>();
        if (inputX != 0)
            animationController.FlipX(inputX < 0);
        targetVelocity = inputX * moveSpeed;
        currentState.UpdateState(this);
    }

    public void SwitchState(IPlayerState nextState)
    {
        currentState = nextState;
        Debug.Log(nextState.ToString());
        currentState.EnterState(this);
    }

    public void Move(float gravityMultiplier = 1f)
    {
        float dt = Time.deltaTime;
        Vector2 acceleration = new(0, gravityScale * gravityMultiplier);
        Vector2 deltaMove = velocity * dt + 0.5f * dt * dt * acceleration;
        controller.Move(deltaMove);
        velocity += acceleration * dt;
    }

    public void ConsumeJump()
    {
        jumpBufferCounter = 0;
    }

    private void Jump(InputAction.CallbackContext callback)
    {
        jumpBufferCounter = jumpBufferTime;
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
        return controller.colDetails.left;
    }

    public bool WallRight()
    {
        return controller.colDetails.right;
    }

    public bool HasSlopeNear()
    {
        return controller.IsSlopeBelow();
    }
    #endregion

    #region Animations related methods called by states
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

    public void PlayJumpingAnimation()
    {
        animationController.PlayJumping();
    }
    public void StopJumpingAnimation()
    {
        animationController.StopJumping();
    }
    #endregion

    #region Events called by animations
    public void HandleJumpingStateTransition()
    {
        PlayJumpingAnimation();
    }
    #endregion
}
