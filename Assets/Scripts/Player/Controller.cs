using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller : MonoBehaviour
{
    private const int minRayAmount = 2;

    [Header("References")]
    [SerializeField] private BoxCollider2D col;

    [Header("Parameters")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float skinWidth;
    [SerializeField, Min(minRayAmount)] private int horizontalRayAmount;
    [SerializeField, Min(minRayAmount)] private int verticalRayAmount;

    private RaycastOrigins raycastOrigins;
    private float horRaySpacing;
    private float verRaySpacing;

    private void Awake()
    {
        SetRaySpacing();
    }

    private void Update()
    {
        UpdateRaycast();
    }

    private void UpdateRaycast()
    {
        Bounds corners = col.bounds;
        corners.Expand(skinWidth * -2);
        raycastOrigins.topLeft = new Vector2(corners.min.x, corners.max.y);
        raycastOrigins.topRight = new Vector2(corners.max.x, corners.max.y);
        raycastOrigins.bottomLeft = new Vector2(corners.min.x, corners.min.y);
        raycastOrigins.bottomRight = new Vector2(corners.max.x, corners.min.y);
    }

    private void SetRaySpacing()
    {
        Bounds corners = col.bounds;
        corners.Expand(skinWidth * -2);
        horRaySpacing = corners.size.y / (horizontalRayAmount - 1);
        verRaySpacing = corners.size.x / (verticalRayAmount - 1);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        for (int i = 0; i < verticalRayAmount; i++)
        {
            Vector2 origin = raycastOrigins.bottomRight + i * verRaySpacing * Vector2.left;
            Gizmos.DrawLine(origin, origin + Vector2.down);
        }
    }

    public void Move(Vector2 displacement)
    {
        UpdateRaycast();

        if (displacement.x != 0) CheckHorizontalCollision(ref  displacement);
        if (displacement.y != 0) CheckVerticalCollision(ref displacement);
        transform.Translate(displacement);
    }

    private void CheckVerticalCollision(ref Vector2 displacement)
    {
        float direction = Mathf.Sign(displacement.y);
        float rayLength = Mathf.Abs(displacement.y) + skinWidth;
        Vector2 rayCorner = (direction >= 0) ? raycastOrigins.topLeft : raycastOrigins.bottomLeft;
        for (int i = 0; i < verticalRayAmount; i++)
        {
            Vector2 rayOrigin = rayCorner + Vector2.right * (verRaySpacing * i + displacement.x); // consider the character can move while falling
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin,
                Vector2.up * direction,
                rayLength,
                collisionMask);
            if (hit)
            {   
                displacement.y = (hit.distance - skinWidth) * direction;
                rayLength = hit.distance; // for corners
            }
        }
    }

    private void CheckHorizontalCollision(ref Vector2 displacement)
    {
        float direction = Mathf.Sign(displacement.x);
        float rayLength = Mathf.Abs(displacement.x) + skinWidth;
        Vector2 rayCorner = (direction >= 0) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        for (int i = 0; i < horizontalRayAmount; i++)
        {
            Vector2 rayOrigin = rayCorner + Vector2.up * (horRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin,
                Vector2.right * direction,
                rayLength,
                collisionMask);
            if (hit)
            {
                displacement.x = (hit.distance - skinWidth) * direction;
                rayLength = hit.distance;
            }
        }
    }

    private struct RaycastOrigins
    {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }
}



