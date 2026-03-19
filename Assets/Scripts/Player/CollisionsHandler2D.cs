using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CollisionsHandler2D : RaycastLayout
{
    [Header("Collisions Parameters")]
    [SerializeField] private float groundProbeDistance;
    [SerializeField] private float maxSlopeAngle;
    [SerializeField] private float slopeEpsilon;
    [SerializeField] private string pushableLayerName;

    public CollisionDetails colDetails;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        UpdateRaycast();
    }

    public void UpdateCollisions(float normalizedScale)
    {
        SetRaySpacing(normalizedScale);
        UpdateRaycast();
    }

    public void ClampDisplacement(ref Vector2 displacement, bool onMovingPlatform = false)
    {
        UpdateRaycast();
        colDetails.ResetCollisions();
        if (displacement.y <= 0) DescendSlope(ref displacement);
        if (displacement.x != 0) CheckHorizontalCollision(ref displacement);
        CheckVerticalCollision(ref displacement);
        if (onMovingPlatform)
            colDetails.below = true;
    }

    private void CheckVerticalCollision(ref Vector2 displacement)
    {
        // diferent implementation respect to horizontal because rays need to go down if displacement.y = 0 which only occurs when character is on the ground or in jump's max height
        float direction = displacement.y > 0f ? 1f : -1f;
        float rayLength = Mathf.Abs(displacement.y) + scaledSkinWidth;
        if (rayLength < scaledSkinWidth + groundProbeDistance)
            rayLength = scaledSkinWidth + groundProbeDistance;
        Vector2 rayCorner = direction >= 0 ? raycastOrigins.topLeft : raycastOrigins.bottomLeft;
        for (int i = 0; i < verticalRayAmount; i++)
        {
            Vector2 rayOrigin = rayCorner + Vector2.right * (verRaySpacing * i + displacement.x); // consider the character can move while falling
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin,
                Vector2.up * direction,
                rayLength,
                collisionMask);
            Debug.DrawRay(rayOrigin, direction * rayLength * Vector2.up, Color.red);
            if (hit)
            {   
                displacement.y = (hit.distance - scaledSkinWidth) * direction;
                rayLength = hit.distance; // for corners

                if (colDetails.onSlope)
                    displacement.x = displacement.y / Mathf.Tan(colDetails.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(displacement.x);
                colDetails.below = !(colDetails.above = direction >= 0);
            }
        }
    }

    private void CheckHorizontalCollision(ref Vector2 displacement)
    {
        float direction = Mathf.Sign(displacement.x);
        float rayLength = Mathf.Abs(displacement.x) + scaledSkinWidth;
        Vector2 rayCorner = direction >= 0 ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        for (int i = 0; i < horizontalRayAmount; i++)
        {
            Vector2 rayOrigin = rayCorner + Vector2.up * (horRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin,
                Vector2.right * direction,
                rayLength,
                collisionMask);
            Debug.DrawRay(rayOrigin, direction * rayLength * Vector2.right, Color.red);
            if (hit)
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer(pushableLayerName))
                {
                    colDetails.nextPushable = true;
                }
                if (hit.distance == 0) continue;
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if (i == 0 && slopeAngle <= maxSlopeAngle)
                {
                    float slopeStartDistance = slopeAngle != colDetails.prevSlopeAngle ? hit.distance - scaledSkinWidth : 0;
                    displacement.x -= slopeStartDistance * direction; // avoids getting more displacement when reaching a slope because of previous slope displacement values
                    ClimbSlope(ref displacement, slopeAngle);
                    displacement.x += slopeStartDistance;
                }

                if (!colDetails.onSlope || slopeAngle > maxSlopeAngle)
                {
                    displacement.x = Mathf.Min(Mathf.Abs(displacement.x), (hit.distance - scaledSkinWidth)) * direction;
                    rayLength = Mathf.Min(Mathf.Abs(displacement.x), hit.distance); // this prevents an error because a ray touching a slope with higher angle than current slope 
                    if (colDetails.onSlope)
                        displacement.y = Mathf.Tan(colDetails.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(displacement.x);
                    colDetails.left = !(colDetails.right = direction >= 0);
                }
            }
        }
    }

    private void ClimbSlope(ref Vector2 displacement, float slopeAngle)
    {
        float slopeDisplacement = Mathf.Abs(displacement.x);
        float slopeAngleRad = slopeAngle * Mathf.Deg2Rad;
        if (displacement.y < slopeDisplacement * Mathf.Sin(slopeAngleRad)) // checks that jumping is not overrided by slope behavior which can happen in steep slopes
        {
            displacement.x = slopeDisplacement * Mathf.Cos(slopeAngleRad) * Mathf.Sign(displacement.x);
            displacement.y = slopeDisplacement * Mathf.Sin(slopeAngleRad);
            colDetails.below = true;
            colDetails.onSlope = true;
            colDetails.slopeAngle = slopeAngle;
        }
    }

    private void DescendSlope(ref Vector2 displacement)
    {
        RaycastHit2D hitLeft = Physics2D.Raycast(raycastOrigins.bottomLeft, Vector2.down, groundProbeDistance + scaledSkinWidth, collisionMask);
        RaycastHit2D hitRight = Physics2D.Raycast(raycastOrigins.bottomRight, Vector2.down, groundProbeDistance + scaledSkinWidth, collisionMask);
        SlideSlope(hitLeft, ref displacement);
        SlideSlope(hitRight, ref displacement);
        if (colDetails.onSlopeSlide) return;
        float direction = Mathf.Sign(displacement.x);
        Vector2 rayOrigin = direction >= 0 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, collisionMask);
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            float yDisplacement = Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(displacement.x);
            if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
            {
                if (Mathf.Sign(hit.normal.x) == direction)
                    if (hit.distance - scaledSkinWidth - yDisplacement <= slopeEpsilon) // when accelerating yDisplacement can be small enough to make the character walk away from the slope
                    {
                        float slopeDisplacement = Mathf.Abs(displacement.x);
                        float slopeAngleRad = slopeAngle * Mathf.Deg2Rad;
                        displacement.x = slopeDisplacement * Mathf.Cos(slopeAngleRad) * Mathf.Sign(displacement.x);
                        displacement.y -= slopeDisplacement * Mathf.Sin(slopeAngleRad);
                        colDetails.below = true;
                        colDetails.onSlopeDescent = true;
                        colDetails.slopeAngle = slopeAngle;
                    }
            }
        }
    }

    private void SlideSlope(RaycastHit2D hit, ref Vector2 displacement)
    {
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle > maxSlopeAngle)
            {
                displacement.x = (Mathf.Abs(displacement.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * hit.normal.x;
                colDetails.slopeAngle = slopeAngle;
                colDetails.onSlopeSlide = true;
            }
        }
    }

    public bool IsSlopeBelow()
    {
        float probeLength = scaledSkinWidth + groundProbeDistance * 2f;
        RaycastHit2D leftHit = Physics2D.Raycast(raycastOrigins.bottomLeft,
            Vector2.down,
            probeLength,
            collisionMask);
        RaycastHit2D rightHit = Physics2D.Raycast(raycastOrigins.bottomRight,
            Vector2.down,
            probeLength,
            collisionMask);
        if (leftHit)
        {
            float angle = Vector2.Angle(leftHit.normal, Vector2.up);
            if (angle > 0 && angle <= maxSlopeAngle)
                return true;
        }
        if (rightHit)
        {
            float angle = Vector2.Angle(rightHit.normal, Vector2.up);
            if (angle > 0 && angle <= maxSlopeAngle)
                return true;
        }
        return false;
    }

    public LayerMask GetPushableLayer()
    {
        return 1 << LayerMask.NameToLayer(pushableLayerName);
    }

    public struct CollisionDetails
    {
        public bool above, below, left, right, onSlope, onSlopeDescent, onSlopeSlide, nextPushable;
        public float slopeAngle, prevSlopeAngle;

        public void ResetCollisions()
        {
            above = below = left = right = onSlope = onSlopeDescent = onSlopeSlide = nextPushable = false;
            prevSlopeAngle = slopeAngle;
            slopeAngle = 0;
        }
    }

    private void OnDrawGizmos()
    {
        float probeLength = scaledSkinWidth + groundProbeDistance;
        if (!Application.isPlaying) return;
        Gizmos.color = Color.red;
        RaycastHit2D leftHit = Physics2D.Raycast(raycastOrigins.bottomLeft,
            Vector2.down,
            probeLength,
            collisionMask);
        RaycastHit2D rightHit = Physics2D.Raycast(raycastOrigins.bottomRight,
            Vector2.down,
            probeLength,
            collisionMask);
        Gizmos.DrawLine(raycastOrigins.bottomLeft, raycastOrigins.bottomLeft + new Vector2(0, probeLength));
        Gizmos.DrawLine(raycastOrigins.bottomRight, raycastOrigins.bottomRight + new Vector2(0, probeLength));
        Gizmos.color = Color.blue;
        if (leftHit)
            Gizmos.DrawLine(leftHit.point, leftHit.point + leftHit.normal * 0.1f);
        if (rightHit)
            Gizmos.DrawLine(rightHit.point, rightHit.point + rightHit.normal * 0.1f);
    }
}