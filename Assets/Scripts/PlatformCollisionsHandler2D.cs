using UnityEngine;
using System.Collections.Generic;

public class PlatformCollisionsHandler2D : RaycastLayout
{
    [SerializeField] private LayerMask platformPassengersMask;
    [SerializeField] private Vector2 velocity;
    [SerializeField] private float upwardsDetectionEpsilon;

    private List<PassengerDetails> passengers;
    private Dictionary<Transform, CollisionsHandler2D> knownPassengers; // to reduce GetComponent<CollisionsHandler2D>() calls 

    protected override void Awake()
    {
        knownPassengers = new Dictionary<Transform, CollisionsHandler2D>();
        base.Awake();
    }

    private void Update()
    {
        UpdateRaycast();
        Vector2 displacement = velocity * Time.deltaTime;
        CalculatePassengersDisplacement(displacement);
        MovePassengers(true); // move passengers that need first
        transform.Translate(displacement);
        MovePassengers(false); // move the rest
    }

    private void MovePassengers(bool movePassengerFirst)
    {
        foreach (var passenger in passengers)
        {
            if (!knownPassengers.ContainsKey(passenger.transform))
                knownPassengers.Add(passenger.transform, passenger.transform.GetComponent<CollisionsHandler2D>());
            if (passenger.moveFirst == movePassengerFirst)
                knownPassengers[passenger.transform].Move(passenger.displacement, passenger.onPlatform);
        }
    }

    private void CalculatePassengersDisplacement(Vector2 displacement)
    {
        passengers = new List<PassengerDetails>();
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
                Vector2 rayOrigin = rayCorner + Vector2.right * (verRaySpacing * i);
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
                        PassengerDetails passenger = new(hit.transform, new Vector2(pushX, pushY), directionY >= 0, true); // platform should move after passenger if passenger is in front of platform's displacement
                        passengers.Add(passenger);
                    }
                }
            }
        }

        // horizontal pushing
        if (velocity.x != 0)
        {
            float rayLength = Mathf.Abs(displacement.x) + skinWidth;
            Vector2 rayCorner = directionX >= 0 ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
            for (int i = 0; i < horizontalRayAmount; i++)
            {
                Vector2 rayOrigin = rayCorner + Vector2.up * (horRaySpacing * i);
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
                        PassengerDetails passenger = new(hit.transform, new Vector2(pushX, pushY), false, true); // platform should push passenger whenever direction is heading
                        passengers.Add(passenger);
                    }
                }
            }
        }

        // if it is going downwards or is moving just horizontal
        if (directionY < 0 || displacement.y == 0 && displacement.x != 0)
        {
            float rayLength = skinWidth + upwardsDetectionEpsilon;
            for (int i = 0; i < verticalRayAmount; i++)
            {
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin,
                    Vector2.up,
                    rayLength,
                    platformPassengersMask);
                if (hit)
                {
                    if (!detectedPassengers.Contains(hit.transform))
                    {
                        detectedPassengers.Add(hit.transform);
                        float pushY = displacement.y;
                        float pushX = displacement.x;
                        PassengerDetails passenger = new(hit.transform, new Vector2(pushX, pushY), true, false); // platform should move before passenger on top
                        passengers.Add(passenger);
                    }
                }
            }
        }
    }

    private struct PassengerDetails
    {
        public Transform transform;
        public Vector2 displacement;
        public bool onPlatform;
        public bool moveFirst;

        public PassengerDetails(Transform transform, Vector2 displacement, bool onPlatform, bool moveFirst)
        {
            this.transform = transform;
            this.displacement = displacement;
            this.onPlatform = onPlatform;
            this.moveFirst = moveFirst;
        }
    }
}
