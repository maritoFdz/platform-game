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
    private float spacing;
    private Vector2 dir;
    private Vector2 rayOriginVector;

    private void Awake()
    {
        initialPos = transform.position;
    }

    private void Start()
    {
        float rotation = transform.eulerAngles.z;
        switch (rotation)
        {
            case 0f:
                dir = Vector3.down;
                rayOriginVector = Vector2.right;
                spacing = col.bounds.size.x / (rayAmount - 1);
                break;
            case 90f:
                dir = Vector3.right;
                rayOriginVector = Vector2.up;
                spacing = col.bounds.size.y / (rayAmount - 1);
                break;
            case 270f:
                dir = Vector3.left;
                rayOriginVector = Vector2.up;
                spacing = col.bounds.size.y / (rayAmount - 1);
                break;
            case 180f:
                dir = Vector3.up;
                rayOriginVector = Vector2.right;
                spacing = col.bounds.size.x / (rayAmount - 1);
                break;
        }
    }

    private void Update()
    {
        if (locked) return;

        if (isClosing)
        {
            float displacement = speed * Time.deltaTime;
            float minDistance = displacement;

            Vector2 baseOrigin = Vector2.zero;
            if (dir == Vector2.down)
                baseOrigin = new Vector2(col.bounds.min.x, col.bounds.min.y - raycastOffset);
            else if (dir == Vector2.up)
                baseOrigin = new Vector2(col.bounds.min.x, col.bounds.max.y + raycastOffset);
            else if (dir == Vector2.right)
                baseOrigin = new Vector2(col.bounds.max.x + raycastOffset, col.bounds.min.y);
            else if (dir == Vector2.left)
                baseOrigin = new Vector2(col.bounds.min.x - raycastOffset, col.bounds.min.y);

            for (int i = 0; i < rayAmount; i++)
            {
                Vector2 rayOrigin = baseOrigin + i * spacing * rayOriginVector;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, dir, displacement, collisionLayer);
                Debug.DrawRay(rayOrigin, dir * displacement, Color.red);
                if (hit)
                    minDistance = Mathf.Min(hit.distance, minDistance);
            }

            displacement = minDistance;

            transform.position += (Vector3) dir * displacement;

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
        Vector2 origin = Vector2.zero;

        if (dir == Vector2.down)
            origin = new Vector2(col.bounds.center.x, col.bounds.min.y - raycastOffset);
        else if (dir == Vector2.up)
            origin = new Vector2(col.bounds.center.x, col.bounds.max.y + raycastOffset);
        else if (dir == Vector2.right)
            origin = new Vector2(col.bounds.max.x + raycastOffset, col.bounds.center.y);
        else if (dir == Vector2.left)
            origin = new Vector2(col.bounds.min.x - raycastOffset, col.bounds.center.y);
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, Mathf.Infinity, collisionLayer);

        if (hit)
        {
            float extent = (dir == Vector2.up || dir == Vector2.down)
                ? col.bounds.extents.y
                : col.bounds.extents.x;

            target = hit.point - dir * extent;
        }
    }
}
