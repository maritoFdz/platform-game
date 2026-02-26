using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Controller))]
public class Player : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Controller controller;

    [Header("Movement Settings")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float maxHeightTime;
    [SerializeField] private float moveSpeed;

    [HideInInspector] public float gravityScale;
    [HideInInspector] public float jumpForce;
    [HideInInspector] public Vector2 velocity;
    [HideInInspector] public bool jumpPressed;
    private PlayerInputActions PlayerInput;

    private IPlayerState currentState;
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
        gravityScale = - (2 * jumpHeight) / Mathf.Pow(maxHeightTime, 2);
        jumpForce = Mathf.Abs(gravityScale *  maxHeightTime);
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
        velocity.x = PlayerInput.Player.Move.ReadValue<float>() * moveSpeed;
        currentState.UpdateState(this);
    }

    public void SwitchState(IPlayerState nextState)
    {
        currentState = nextState;
        Debug.Log(nextState.ToString());
        currentState.EnterState(this);
    }

    public void AddGravityForce()
    {
        velocity.y += gravityScale * Time.deltaTime;
    }

    public void Move()
    {
        controller.Move(velocity * Time.deltaTime);
    }

    public bool GroundDetected()
    {
        return controller.colDetails.below;
    }

    private void Jump(InputAction.CallbackContext callback)
    {
        jumpPressed = true;
    }
}
