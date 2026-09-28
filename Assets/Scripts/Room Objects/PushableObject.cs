using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using static RaycastLayout;

public class PushableObject : MonoBehaviour, IResetteable, IPlatformCarrier
{
    [Header("References")]
    [SerializeField] private CollisionsHandler2D controller;
    [SerializeField] private Transform visual;
    [SerializeField] private PushableObjectParameters parameters;

    private Vector3 initialPos;
    private float rotationAngle;
    private float push;
    private Vector2 velocity;
    private float velocityXSmoothing;
    private Player playerPushing;
    private State currentState;
    private RaycastHit2D hitNormal;
    private bool hasStoredNormal = false;
    private Vector3 lastFramePosition;
    private string originalTag;
    private bool startedFrameFalling;

    private List<ICrushable> objectsToCrush;

    public LayerMask PassengerMask => parameters.passengerMask;
    public string OriginalTag => originalTag;

    [HideInInspector] public bool IsGrounded => currentState == State.Ground;

    private void Awake()
    {
        objectsToCrush = new List<ICrushable>();
    }

    private void Start()
    {
        initialPos = transform.position;
        currentState = State.Falling;
        originalTag = gameObject.tag;
    }

    private void Update()
    {
        objectsToCrush.Clear();
        float dt = Time.deltaTime;
        RaycastLayoutDetails info = controller.GetRaycastLayoutDetails();
        RaycastOrigins origins = controller.GetRaycastOrigins();
        int supportRays = 0;
        int leftHits = 0;
        int rightHits = 0;
        hasStoredNormal = false;
        for (int i = 0; i < info.horizontalRayAmount; i++)
        {
            Vector2 origin = origins.bottomLeft + Vector2.right * (info.horRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, info.skinWidth + 0.1f, info.collisionMask);

            if (hit)
            {
                if (!hasStoredNormal)
                {
                    hitNormal = hit;
                    hasStoredNormal = true;
                }
                supportRays++;
                if (i < info.horizontalRayAmount / 2) leftHits++;
                else rightHits++;
            }

            RaycastHit2D crushHit = Physics2D.Raycast(origin, Vector2.down, info.skinWidth + 0.1f, parameters.crushablesLayer);
            if (crushHit)
            {
                if (crushHit.collider.TryGetComponent<ICrushable>(out ICrushable crushable))
                    if (!objectsToCrush.Contains(crushable)) objectsToCrush.Add(crushable); // esto no es tan fula como para usar un hash set
            }
        }
        float supportRatio = (float) supportRays / info.horizontalRayAmount;

        IPlatformCarrier thisInterface = this;
        thisInterface.UpdateMovingPlatformTag(controller);
        startedFrameFalling = false;
        switch (currentState)
        {
            case State.Ground:
            {
                GroundUpdate(dt, leftHits, rightHits, supportRatio);
                break;
            }

            case State.Falling:
            {
                startedFrameFalling = true;
                FallingUpdate(dt);
                break;
            }

            case State.SlidingSlope:
            {
                SlidingUpdate(dt);
                break;
            }
        }
    }

    private void LateUpdate()
    {
        if (!startedFrameFalling) return;

        foreach (ICrushable crushable in objectsToCrush)
        {
            if (crushable == null) continue;
            if (crushable.IsPendingCrush) continue;
            if (crushable.IsColidingVer()) crushable.Crush(true, parameters.crushTime);
        }
    }

    private void GroundUpdate(float dt, int leftHits, int rightHits, float supportRatio)
    {
        CheckPlayerPushing();
        velocity.y = 0f;
        Vector2 displacement = velocity * dt;
        controller.ClampDisplacement(ref displacement);
        float target;
        if (playerPushing != null)
            target = push * parameters.pushSpeed;
        else
            target = 0f;
        velocity.x = Mathf.SmoothDamp(velocity.x, target, ref velocityXSmoothing, parameters.accelerationTime);
        if (playerPushing != null)
            playerPushing.ApplyExternalDisplacement(displacement);
        transform.Translate(displacement);
        float rotation = 0f;

        if (supportRatio < 1f)
        {
            float dir = (leftHits > rightHits) ? -1f : 1f;
            float targetRotation = 1f - supportRatio;
            rotation = dir * parameters.maxRotationAngle * targetRotation;
        }

        rotationAngle = Mathf.MoveTowards(rotationAngle, rotation, parameters.rotationVelocity * dt);

        if (visual != null)
        {
            visual.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);
            float tiltPercent = Mathf.Abs(rotationAngle) / parameters.maxRotationAngle;
            float yOffset = -tiltPercent * parameters.maxVisualOffset;
            visual.localPosition = new Vector3(0f, yOffset, 0f);
        }

        float playerPush = controller.IsNextToSlope(-1, parameters.groundSlopeFrontTol) ? -1 : 1;

        if (supportRatio <= 0f && !controller.colDetails.onMovingPlatform)
        {
            currentState = State.Falling;

            if (playerPushing != null)
            {
                visual.localPosition = new Vector3(0f, 0f, 0f);
                playerPushing.pushingObjectState.drop = true;
                playerPushing = null;
            }
        }
        else if (controller.colDetails.onSlopeSlide || controller.colDetails.onSlope || controller.colDetails.onSlopeDescent || controller.IsNextToSlope(-1, parameters.groundSlopeFrontTol) || controller.IsNextToSlope(1, parameters.groundSlopeFrontTol))
        {
            if (playerPushing != null && (controller.IsNextToSlope(-1, parameters.groundSlopeFrontTol) || controller.IsNextToSlope(1, parameters.groundSlopeFrontTol)))
                playerPushing.velocity.x = -playerPush * parameters.slopeSlideMultiplier * 1.5f;
            currentState = State.SlidingSlope;
        }
    }

    private void CheckPlayerPushing()
    {
        if (playerPushing != null)
        {
            if (Vector3.Distance(playerPushing.transform.position, transform.position) > parameters.pushDistance)
            {
                playerPushing.pushingObjectState.drop = true;
                playerPushing = null;
            }
        }
    }

    private void FallingUpdate(float dt)
    {
        velocity.y += parameters.gravity * dt;
        velocity.x = Mathf.SmoothDamp(velocity.x, 0f, ref velocityXSmoothing, parameters.accelerationTimeAir);
        Vector2 displacement = velocity * dt;
        controller.ClampDisplacement(ref displacement);
        transform.Translate(displacement);
        float rotationTarget = (Mathf.Approximately(Mathf.Abs(rotationAngle), 90)) ? rotationAngle : -Mathf.Sign(velocity.x) * 90;
        rotationAngle = Mathf.MoveTowards(rotationAngle, rotationTarget, parameters.rotationVelocity * dt);
        visual.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);
        if (controller.colDetails.below)
        {
            rotationAngle = 0f;
            velocity.y = 0f;
            currentState = State.Ground;
        }
        else if (controller.colDetails.onSlopeSlide)
            currentState = State.SlidingSlope;
    }

    private void SlidingUpdate(float dt)
    {
        velocity.y += parameters.gravity * dt;
        Vector2 move = velocity * dt;
        controller.ClampDisplacement(ref move);
        transform.Translate(move);

        if (visual != null && hasStoredNormal)
        {
            float slopeAngle = Vector2.Angle(hitNormal.normal, Vector2.up);

            float targetRot = slopeAngle;
            rotationAngle = Mathf.MoveTowards(rotationAngle, targetRot, parameters.rotationVelocity * dt);

            visual.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);
            float xOffset = -hitNormal.normal.x * parameters.maxVisualOffset * 2;
            float yOffset = -Mathf.Abs(hitNormal.normal.x) * parameters.maxVisualOffset * 2;

            visual.localPosition = new Vector3(xOffset, yOffset, 0f);
        }

        if (playerPushing != null)
        {
            playerPushing.pushingObjectState.drop = true;
            playerPushing = null;
        }

        float dir = controller.IsNextToSlope(-1, parameters.slideSlopeBeneathTol) ? -1 : 1;
        if (!controller.colDetails.onSlopeSlide && !controller.colDetails.onSlope && !controller.colDetails.onSlopeDescent)
        {
            if (controller.colDetails.below && (controller.IsNextToSlope(-1, parameters.groundSlopeFrontTol) || controller.IsNextToSlope(1, parameters.groundSlopeFrontTol)) && !controller.FallInFront(dir, 10))
            {
                velocity.x = -dir * parameters.slopeSlideMultiplier;
                float rotationTarget = (Mathf.Approximately(Mathf.Abs(rotationAngle), 90)) ? rotationAngle : Mathf.Sign(velocity.x) * 90;
                rotationAngle = Mathf.MoveTowards(rotationAngle, rotationTarget, parameters.rotationVelocity * 2 * dt);
                visual.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);
                currentState = State.Ground;
            }
            else if (controller.colDetails.below)
                currentState = State.Falling;
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

    public void ResetEntity()
    {
        transform.position = initialPos;
        velocity = Vector2.zero;
        velocityXSmoothing = 0f;
        currentState = State.Falling;
        startedFrameFalling = false;
    }

    public List<Transform> GetPassengersOnTop()
    {
        return controller.GetTransformsInDirection(Vector2.up, PassengerMask, parameters.upwardsDetectionEpsilon);
    }

    private enum State { Falling, Ground, SlidingSlope }
}