using UnityEngine;
using System.Collections.Generic;

public class PlatformCollisionsHandler2D : RaycastLayout
{
    [SerializeField] LayerMask platformPassengersMask;
    [SerializeField] Vector2 velocity;

    private void Update()
    {
        UpdateRaycast();
        Vector2 displacement = velocity * Time.deltaTime;
        MovePassengers(displacement);
        transform.Translate(displacement);
    }

    private void MovePassengers(Vector2 displacement)
    {
        HashSet<Transform> detectedPassengers = new();
        float directionX = Mathf.Sign(displacement.x);
        float directionY = Mathf.Sign(displacement.y);

        // vertical movement
        if (displacement.y != 0)
        {
            float rayLength = Mathf.Abs(displacement.y) + skinWidth;
            Vector2 rayCorner = directionY >= 0 ? raycastOrigins.topLeft : raycastOrigins.bottomLeft;
            for (int i = 0; i < verticalRayAmount; i++)
            {
                Vector2 rayOrigin = rayCorner + Vector2.right * (verRaySpacing * i); // consider the character can move while falling
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin,
                    Vector2.up * directionY,
                    rayLength,
                    platformPassengersMask);
                Debug.DrawRay(rayOrigin, directionY * rayLength * Vector2.up, Color.red);
                if (hit)
                {
                    if (!detectedPassengers.Contains(hit.transform))
                    {
                        detectedPassengers.Add(hit.transform);
                        float pushY = displacement.y - (hit.distance - skinWidth) * directionY;
                        float pushX = (directionY >= 0) ? displacement.x : 0f;
                        hit.transform.Translate(new Vector3(pushX, pushY));
                    }
                }
            }
        }

        // horizontal movement
        if (velocity.x != 0)
        {
            float rayLength = Mathf.Abs(displacement.x) + skinWidth;
            Vector2 rayCorner = directionX >= 0 ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
            for (int i = 0; i < horizontalRayAmount; i++)
            {
                Vector2 rayOrigin = rayCorner + Vector2.up * (horRaySpacing * i); // consider the character can move while falling
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin,
                    Vector2.right * directionX,
                    rayLength,
                    platformPassengersMask);
                if (hit)
                {
                    if (!detectedPassengers.Contains(hit.transform))
                    {
                        detectedPassengers.Add(hit.transform);
                        float pushY = 0f;
                        float pushX = displacement.x - (hit.distance - skinWidth) * directionX;
                        hit.transform.Translate(new Vector3(pushX, pushY));
                    }
                }
            }
        }
    }
}
