using UnityEngine;

[CreateAssetMenu(fileName = "New Player Parameters", menuName = "Scriptable Objects/Player Parameters")]
public class PlayerParameters : ScriptableObject
{
    [Header("Movement Settings")]
    public float moveSpeed;
    public float accelerationTimeGround;
    public float runningVelocityMultiplier;
    public float timeToIdle;
    public float coyoteTime;
    public bool canRun;

    [Header("Dash Settings")]
    public bool canDash;
    public bool canDashVertical;
    public bool canDashDiagonal;
    public float dashDistance;
    public float dashTime;
    public float dashCooldown;
    public float dashBufferTime;
    public float decelerationTimeDash;

    [Header("Jump Settings")]
    public float maxJumpHeight;
    public float minJumpHeight;
    public float maxHeightTime;
    public float jumpBufferTime;
    public float accelerationTimeAir;
    public float gravityFallMultiplier;
    public float hangTime;
    public float maxFallSpeed;

    [Header("Wall Movement Settings")]
    public float wallSlideSpeed;
    public float accelerationTimeWall;
    public float wallStickTime;
    public Vector2 frontDirectionJump;
    public Vector2 climbJump;
    public Vector2 fallOfJump;

    [Header("Slopes Movement Settings")]
    public float endSlopeBoostX;
    public Vector2 slopeSlidingJump;

    [Header("Scale Settings")]
    public Vector3 maxScale;
    public float upscalePerUnit;
    public float splashFallMinVelocity;
    public float splashWallMinVelocity;
    [Range(0f, 1f)] public float scaleReductionPerUnit;
    [Range(0.1f, 1f)] public float minNormalizedScale;

    [Header("Swimming Settings")]
    public float floatingForce;
    public float damping;
}
