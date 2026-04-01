using UnityEngine;

[CreateAssetMenu(fileName = "New Pushable Object Parameters", menuName = "Scriptable Objects/Pushable Object Parameters")]
public class PushableObjectParameters : ScriptableObject
{
    public float pushSpeed;
    public float accelerationTime;
    public float slopeSlideMultiplier;
    public float gravity;
    public float rotationVelocity;
    public float maxRotationAngle;
    public int groundSlopeFrontTol;
    public int slopeBelowTol;
    public int slideSlopeBeneathTol;
    public float maxVisualOffset;
    public float pushDistance;
}
