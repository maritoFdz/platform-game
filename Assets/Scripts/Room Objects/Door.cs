using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Door : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider2D col;
    [SerializeField] private LayerMask collisionLayer;

    [Header("Parameters")]
    [SerializeField] private float speed;
    [SerializeField] private float raycastOffset;
    [SerializeField] private float collisionTolerance;
    [SerializeField] private int rayAmount;

    private bool isClosing;
    public bool locked;
    private Vector2 target;
    private Vector2 initialPos;

    private void Awake()
    {
        initialPos = transform.position;
    }

    private void Update()
    {
        if (locked) return;

        if (isClosing)
        {
            float moveY = speed * Time.deltaTime;
            Vector2 origin = new(col.bounds.min.x, col.bounds.min.y - raycastOffset);
            float minDistance = moveY;
            float spacing = col.bounds.size.x / (rayAmount - 1);
            for (int i = 0; i < rayAmount; i++)
            {
                Vector2 rayOrigin = origin + i * spacing * Vector2.right;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, moveY, collisionLayer);
                Debug.DrawRay(rayOrigin, Vector2.down * moveY, Color.red);
                if (hit)
                    minDistance = Mathf.Min(hit.distance, minDistance);
            }

            moveY = minDistance;

            transform.position += Vector3.down * moveY;

            if (Vector3.Distance(transform.position, target) < collisionTolerance)
            {
                transform.position = target;
                locked = true;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, initialPos, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, initialPos) < collisionTolerance)
            {
                transform.position = initialPos;
                locked = true;
            }
        }
    }

    [ContextMenu("Open Door")]
    public void Open()
    {
        isClosing = false;
        locked = false;
    }

    [ContextMenu("Close Door")]
    public void Close()
    {
        isClosing = true;
        locked = false;
        Vector2 origin = new(transform.position.x, transform.position.y - col.bounds.size.y / 2 - raycastOffset);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, Mathf.Infinity, collisionLayer);
        if (hit)
            target = new(transform.position.x, hit.point.y + col.bounds.size.y / 2f);
    }
}
