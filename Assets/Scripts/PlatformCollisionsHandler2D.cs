using UnityEngine;
using System.Collections.Generic;

public class PlatformCollisionsHandler2D : RaycastLayout
{
    [Header("Platform Parameters")]
    [SerializeField] private LayerMask platformPassengersMask;
    [SerializeField] private Vector2[] relativeWaypoints;
    [SerializeField] private float speed;
    [SerializeField] private bool hasCyclicalPath;
    [SerializeField] private float waypointDelay;
    [SerializeField] private float waypointVisRadius;
    [SerializeField, Range(0, 3)] private float easingIntensity;
    [SerializeField] private float upwardsDetectionEpsilon;

    private Vector3[] waypoints;
    private int currentWaypointIndex;
    private float normalizedDistance;
    private float delayCounter;
    private List<PassengerDetails> passengers;
    private Dictionary<Transform, CollisionsHandler2D> knownPassengers; // to reduce GetComponent<CollisionsHandler2D>() calls 

    protected override void Awake()
    {
        knownPassengers = new Dictionary<Transform, CollisionsHandler2D>();
        waypoints = new Vector3[relativeWaypoints.Length];
        base.Awake();
    }

    private void Start()
    {
        RelativeWaypoints2Global();
    }

    private void RelativeWaypoints2Global()
    {
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = transform.position + (Vector3)(relativeWaypoints[i]);
        }
    }

    private void Update()
    {
        UpdateRaycast();
        Vector2 displacement = GetPlatformDisplacement();
        CalculatePassengersDisplacement(displacement);
        MovePassengers(true); // move passengers that need first
        transform.Translate(displacement);
        MovePassengers(false); // move the rest
    }

    private Vector2 GetPlatformDisplacement()
    {
        if (delayCounter > 0f)
        {
            delayCounter -= Time.deltaTime;
            return Vector2.zero;
        }
        Vector3 currentWaypoint = waypoints[currentWaypointIndex];
        Vector3 nextWaypoint = waypoints[currentWaypointIndex + 1];
        normalizedDistance += speed * Time.deltaTime / Vector3.Distance(currentWaypoint, nextWaypoint);
        normalizedDistance = Mathf.Clamp01(normalizedDistance);
        float easedNormalizedDistance = Ease(normalizedDistance);  
        Vector3 nextPos = Vector3.Lerp(currentWaypoint, nextWaypoint, easedNormalizedDistance); // makes movement smoother while 
        if (normalizedDistance >= 1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % (waypoints.Length -1);
            normalizedDistance = 0f;
            delayCounter = waypointDelay;
            if (currentWaypointIndex == 0) // if a full travel has been made
            {
                if (!hasCyclicalPath)
                    System.Array.Reverse(waypoints);
            }
        }
        return nextPos - transform.position;
    }

    private float Ease(float x)
    {
        return Mathf.Pow(x, easingIntensity) / (Mathf.Pow(x, easingIntensity) + Mathf.Pow(1 - x, easingIntensity));
    }

    private void MovePassengers(bool movePassengerFirst)
    {
        foreach (var passenger in passengers)
        {
            if (!knownPassengers.ContainsKey(passenger.transform))
                knownPassengers.Add(passenger.transform, passenger.transform.GetComponent<CollisionsHandler2D>());
            if (passenger.moveFirst == movePassengerFirst)
            {
                CollisionsHandler2D passengerValue = knownPassengers[passenger.transform];
                passengerValue.Move(passenger.displacement, passenger.onPlatform);
            }
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
        if (displacement.x != 0)
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (relativeWaypoints != null)
        {
            for (int i = 0; i < relativeWaypoints.Length; i++)
                Gizmos.DrawWireSphere(!Application.isPlaying ? transform.position + (Vector3)(relativeWaypoints[i]) : waypoints[i], waypointVisRadius);
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
