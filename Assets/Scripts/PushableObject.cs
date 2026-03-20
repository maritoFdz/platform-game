using UnityEngine;
using static RaycastLayout;
public class PushableObject : MonoBehaviour
{
    
    [Header("References")]
    [SerializeField] private CollisionsHandler2D controller;
    [SerializeField] private Transform visual;

    [Header("Parameters")]
    [SerializeField] private float pushSpeed;
    [SerializeField] private float accelerationTime;
    [SerializeField] private float slopeSlideMultiplier;
    [SerializeField] private float gravity;
    [SerializeField] private float maxTilt = 30f;
    [SerializeField] private int groundSlopeFrontTol;
    [SerializeField] private int slopeBelowTol;
    [SerializeField] private int slideSlopeBeneathTol;

    private float tilt;
    private float push;
    private Vector2 velocity;
    private float velocityXSmoothing;
    private Player playerPushing;
    private State currentState;

    private void Start()
    {
        currentState = State.Falling;
        visual.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        RaycastLayoutDetails info = controller.GetRaycastLayoutDetails();
        RaycastOrigins origins = controller.GetRaycastOrigins();

        float rayLength = info.skinWidth + 0.2f;

        int supportRays = 0;
        int leftHits = 0;
        int rightHits = 0;

        for (int i = 0; i < info.horizontalRayAmount; i++)
        {
            Vector2 origin = origins.bottomLeft + Vector2.right * (info.horRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, info.collisionMask);

            if (hit)
            {
                supportRays++;
                if (i < info.horizontalRayAmount / 2)
                    leftHits++;
                else
                    rightHits++;
            }
        }

        float supportRatio = (float)supportRays / info.horizontalRayAmount;

        switch (currentState)
        {
            case State.Falling:
                {
                    Debug.Log("Falling");
                    velocity.y += gravity * dt;
                    velocity.x = 0.5f * velocity.x;
                    velocityXSmoothing = 0;
                    Vector2 displacement = velocity * dt;
                    controller.ClampDisplacement(ref displacement);
                    transform.Translate(displacement);
                    visual.localRotation = Quaternion.Lerp(visual.localRotation, Quaternion.identity, dt * 5f);
                    if (controller.colDetails.below)
                    {
                        velocity.y = 0f;
                        currentState = State.Ground;
                    }
                    else if (controller.colDetails.onSlopeSlide)
                        currentState = State.SlidingSlope;
                    break;
                }

            case State.Ground:
                {
                    velocity.y = 0f;
                    Debug.Log("Ground");
                    Vector2 displacement = velocity * dt;
                    controller.ClampDisplacement(ref displacement);
                    float target;
                    if (playerPushing != null)
                        target = push * pushSpeed;
                    else
                        target = 0f;
                    velocity.x = Mathf.SmoothDamp(velocity.x, target, ref velocityXSmoothing, accelerationTime);
                    if (playerPushing != null)
                        playerPushing.ApplyExternalDisplacement(displacement);

                    transform.Translate(displacement);

                    float targetTilt = 0f;

                    if (supportRatio < 1f)
                    {
                        float dir = (leftHits > rightHits) ? -1f : 1f;
                        float lack = 1f - supportRatio;

                        targetTilt = dir * maxTilt * lack;
                    }

                    tilt = Mathf.Lerp(tilt, targetTilt, dt * 10f);

                    if (visual != null)
                        visual.localRotation = Quaternion.Euler(0f, 0f, tilt);

                    float playerPush = controller.IsNextToSlope(-1, groundSlopeFrontTol) ? -1 : 1;

                    if (supportRatio <= 0.05f)
                    {
                        currentState = State.Falling;

                        if (playerPushing != null)
                        {
                            playerPushing.pushingObjectState.drop = true;
                            playerPushing = null;
                        }
                    }
                    else if (controller.colDetails.onSlopeSlide || controller.colDetails.onSlope || controller.colDetails.onSlopeDescent || controller.IsNextToSlope(-1, groundSlopeFrontTol) || controller.IsNextToSlope(1, groundSlopeFrontTol))
                    {
                        if (playerPushing != null)
                            playerPushing.velocity.x = -playerPush * slopeSlideMultiplier * 1.5f;
                        currentState = State.SlidingSlope;
                    }

                    break;
                }

            case State.SlidingSlope:
            {
                Debug.Log("SlidingSlope");
                velocity.y += gravity * dt;
                Vector2 move = velocity * dt;
                controller.ClampDisplacement(ref move);
                transform.Translate(move);

                if (visual != null)
                    visual.localRotation = Quaternion.Lerp(visual.localRotation, Quaternion.identity, dt * 5f);

                if (playerPushing != null)
                {
                    playerPushing.pushingObjectState.drop = true;
                    playerPushing = null;
                }

                if (!controller.colDetails.onSlopeSlide && !controller.colDetails.onSlope && !controller.colDetails.onSlopeDescent && !controller.IsSlopeBelow(slopeBelowTol, true))
                {
                        if (controller.colDetails.below)
                        {
                            float dir = controller.IsNextToSlope(-1, slideSlopeBeneathTol) ? -1 : 1;
                            velocity.x = -dir * slopeSlideMultiplier;
                            Debug.Log(velocity.x);
                            currentState = State.Ground;
                        }
                        else
                            currentState = State.Falling;
                }
                break;
            }
        }
    }

    public void SetAsTargetOf(Player player)
    {
        playerPushing = player;
    }

    public void SetDirection(float push)
    {
        this.push = push;
    }

    private enum State { Falling, Ground, SlidingSlope }
}