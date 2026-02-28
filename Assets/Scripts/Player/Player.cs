using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CollisionsHandler2D))]
public class Player : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CollisionsHandler2D controller;

    [Header("Movement Settings")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float maxHeightTime;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpBufferTime;
    public float accelerationTimeGround;
    public float accelerationTimeAir;
    public float gravityFallMultiplier;

    [HideInInspector] public float gravityScale;
    [HideInInspector] public float jumpForce;
    [HideInInspector] public Vector2 velocity;
    [HideInInspector] public float velocityXSmoothing;
    [HideInInspector] public float targetVelocity;

    private PlayerInputActions PlayerInput;
    public bool JumpPressed => jumpBufferCounter > 0;
    private IPlayerState currentState;
    private float jumpBufferCounter;

    // states instances
    public FallingState fallingState = new();
    public GroundState groundState = new();
    public JumpingState jumpingState = new();

    private void Awake()
    {
        PlayerInput = new();
    }

    private void Start()
    {
        SwitchState(groundState);
        gravityScale = -(2 * jumpHeight) / Mathf.Pow(maxHeightTime, 2);
        jumpForce = Mathf.Abs(gravityScale * maxHeightTime);
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
        targetVelocity = PlayerInput.Player.Move.ReadValue<float>() * moveSpeed;
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

    public bool GroundDetected()
    {
        return controller.colDetails.below;
    }

    public bool OnSlope()
    {
        return controller.colDetails.onSlope || controller.colDetails.onSlopeDescent;
    }

    public void ConsumeJump()
    {
        jumpBufferCounter = 0;
    }

    private void Jump(InputAction.CallbackContext callback)
    {
        jumpBufferCounter = jumpBufferTime;
    }
}
