using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Controller))]
public class Player : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Controller controller;

    [Header("Movement Settings")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float gravityScale;

    private PlayerInputActions PlayerInput;
    private Vector2 velocity;
    private bool jumpPressed;

    private void Awake()
    {
        PlayerInput = new();
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
        if (controller.colDetails.above || controller.colDetails.below)
            velocity.y = 0;

        if (jumpPressed && controller.colDetails.below)
        {
            velocity.y = jumpForce;
            jumpPressed = false;
        }
        velocity.x = PlayerInput.Player.Move.ReadValue<float>() * moveSpeed;
        velocity.y -= gravityScale * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void Jump(InputAction.CallbackContext callback)
    {
        Debug.Log(callback);
        if (controller.colDetails.below)
            jumpPressed = true;
        //    velocity.y += jumpForce;
    }
}
