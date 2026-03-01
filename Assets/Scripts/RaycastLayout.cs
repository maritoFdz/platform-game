using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastLayout : MonoBehaviour
{
    private const int minRayAmount = 2;

    [Header("References")]
    [SerializeField] protected BoxCollider2D col;

    [Header("Parameters")]
    [SerializeField] protected LayerMask collisionMask;
    [SerializeField] protected float skinWidth;
    [SerializeField, Min(minRayAmount)] protected int horizontalRayAmount;
    [SerializeField, Min(minRayAmount)] protected int verticalRayAmount;
    protected RaycastOrigins raycastOrigins;
    protected float horRaySpacing;
    protected float verRaySpacing;

    protected virtual void Awake()
    {
        SetRaySpacing();
    }

    protected void UpdateRaycast()
    {
        Bounds corners = col.bounds;
        corners.Expand(skinWidth * -2);
        raycastOrigins.topLeft = new Vector2(corners.min.x, corners.max.y);
        raycastOrigins.topRight = new Vector2(corners.max.x, corners.max.y);
        raycastOrigins.bottomLeft = new Vector2(corners.min.x, corners.min.y);
        raycastOrigins.bottomRight = new Vector2(corners.max.x, corners.min.y);
    }

    protected void SetRaySpacing()
    {
        Bounds corners = col.bounds;
        corners.Expand(skinWidth * -2);
        horRaySpacing = corners.size.y / (horizontalRayAmount - 1);
        verRaySpacing = corners.size.x / (verticalRayAmount - 1);
    }

    protected struct RaycastOrigins
    {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }
}
