using UnityEngine;

[CreateAssetMenu(fileName = "New Collision Parameters", menuName = "Scriptable Objects/Collision Parameters")]
public class CollisionParameters : ScriptableObject
{
    public float raySpacing;
    public float skinWidth;
    public float groundProbeDistance;
    public float maxSlopeAngle;
    public float slopeEpsilon;
}
