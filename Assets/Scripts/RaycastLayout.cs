using System;
using UnityEngine;
using static RaycastLayout;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastLayout : MonoBehaviour
{
    private const int minRayAmount = 2;
    private const float minSkinWitdh = 0.001f;

    [Header("References")]
    [SerializeField] protected BoxCollider2D col;

    [Header("Parameters")]
    [SerializeField] protected LayerMask collisionMask;
    [SerializeField] protected float skinWidth;
    [SerializeField] protected float raySpacing;
    protected float scaledSkinWidth;
    protected float scaledRaySpacing;
    protected int horizontalRayAmount;
    protected int verticalRayAmount;
    protected float horRaySpacing;
    protected float verRaySpacing;
    protected RaycastOrigins raycastOrigins;
    protected RaycastLayoutDetails raycastLayoutDetails;

    protected virtual void Awake()
    {
        scaledSkinWidth = skinWidth;
        SetRaySpacing();
    }

    protected void UpdateRaycast()
    {
        Bounds corners = col.bounds;
        corners.Expand(scaledSkinWidth * -2);
        raycastOrigins.topLeft = new Vector2(corners.min.x, corners.max.y);
        raycastOrigins.topRight = new Vector2(corners.max.x, corners.max.y);
        raycastOrigins.bottomLeft = new Vector2(corners.min.x, corners.min.y);
        raycastOrigins.bottomRight = new Vector2(corners.max.x, corners.min.y);
    }

    protected void SetRaySpacing(float reductionFactor = 1)
    {
        Bounds corners = col.bounds;
        scaledSkinWidth = Mathf.Max(skinWidth * reductionFactor, minSkinWitdh);
        corners.Expand(scaledSkinWidth * -2);
        scaledRaySpacing = raySpacing * reductionFactor;
        horizontalRayAmount = Mathf.RoundToInt(corners.size.y / scaledRaySpacing);
        verticalRayAmount = Mathf.RoundToInt(corners.size.x / scaledRaySpacing);
        horizontalRayAmount = Mathf.Max(minRayAmount, horizontalRayAmount);
        verticalRayAmount = Mathf.Max(minRayAmount, verticalRayAmount);
        horRaySpacing = corners.size.y / (horizontalRayAmount - 1);
        verRaySpacing = corners.size.x / (verticalRayAmount - 1);
        UpdateRaycastDetails();
    }

    private void UpdateRaycastDetails()
    {
        raycastLayoutDetails.skinWidth = scaledSkinWidth;
        raycastLayoutDetails.verRaySpacing = verRaySpacing;
        raycastLayoutDetails.horRaySpacing = horRaySpacing;
        raycastLayoutDetails.verticalRayAmount = verticalRayAmount;
        raycastLayoutDetails.horizontalRayAmount = horizontalRayAmount;
        raycastLayoutDetails.collisionMask = collisionMask;
    }

    public RaycastOrigins GetRaycastOrigins()
    {
        return raycastOrigins;
    }

    public RaycastLayoutDetails GetRaycastLayoutDetails()
    {
        return raycastLayoutDetails;
    }

    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }

    public struct RaycastLayoutDetails
    {
        public int verticalRayAmount, horizontalRayAmount;
        public float horRaySpacing, verRaySpacing, skinWidth;
        public LayerMask collisionMask;
    }
}
