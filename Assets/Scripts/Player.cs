using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;   

    [Header("Movement Settings")]
    [SerializeField] private float upForce;
    [SerializeField] private float moveSpeed;

    private PlayerInputActions PlayerInput;
    private float direction;

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
        direction = PlayerInput.Player.Move.ReadValue<float>();
        Move();
    }

    private void Move()
    {
        rb.AddForce(direction * moveSpeed * Vector2.right);
    }

    private void Jump(InputAction.CallbackContext callback)
    {
        rb.AddForce(Vector3.up *  upForce);
    }
}
