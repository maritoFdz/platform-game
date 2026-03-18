using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PushableObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    public void Push(Vector2 moveAmount)
    {
        rb.MovePosition(transform.position +  (Vector3) moveAmount);
    }
}
